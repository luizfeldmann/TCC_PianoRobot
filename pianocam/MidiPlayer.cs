using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Timers;
using System.Linq;

namespace pianocam
{
    public class MidiPlayer
    {
        private SheetMusic music;
        private Process fluidprocess;

        private Timer timer;
        private BiasStopwatch watch;

        public bool IsPlaying { get; private set; }
        public List<MusicNote> OnNotes { get; private set; }

        public delegate void NoteEventHandler(bool bOn, MusicNote note);
        public event NoteEventHandler NoteEvent;
        public event EventHandler EndOfTrack;

        public int CurrentTime 
        {
            get { return watch.ElapsedMilliseconds; }
            set { watch.ElapsedMilliseconds = value; }
        }

        public MidiPlayer(SheetMusic mus)
        {
            IsPlaying = false;
            music = mus;

            // start fluidsynth process
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = Configs.Read("SynthName");
            psi.UseShellExecute = false;
            psi.Arguments = Configs.Read("SynthArguments") + System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "synth", Configs.Read("SoundFontName"));
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardInput = true;
            fluidprocess = Process.Start(psi);

            Debug.WriteLine("MIDI PLAYER: CREATE PROCESS '{1} {2}' ID: {0}", fluidprocess.Id, psi.FileName, psi.Arguments);

            timer = new Timer(Configs.ReadInt("SynthInterval"));
            timer.Elapsed += new ElapsedEventHandler(synthEvent);
            watch = new BiasStopwatch();
            OnNotes = new List<MusicNote>();

            PlaybackFilter = delegate (InstrumentTrack it)
            {
                return true;
            }; // allow every instrument to play
        }

        private Predicate<InstrumentTrack> PlaybackFilter; // predicate to filter which channels to play
        private void synthEvent(object source, ElapsedEventArgs e)
        {
            List<MusicNote> currentNotes = new List<MusicNote>();
            foreach (InstrumentTrack it in music.Instruments.Where(x => PlaybackFilter(x)))
                currentNotes.AddRange(it.Notes.Where(x => x.StartTime <= watch.ElapsedMilliseconds && x.EndTime >= watch.ElapsedMilliseconds));

            // notas novas
            foreach (MusicNote note in currentNotes.FindAll(x => !OnNotes.Contains(x)))
            {
                fluidprocess.StandardInput.WriteLine("noteon {0} {1} {2}", note.Channel, note.NoteNumber, note.Velocity);
                OnNotes.Add(note);
                NoteEvent(true, note);
            }

            // notas terminaram
            foreach (MusicNote note in OnNotes.FindAll(x => !currentNotes.Contains(x)))
            {
                fluidprocess.StandardInput.WriteLine("noteoff {0} {1} {2}", note.Channel, note.NoteNumber, note.Velocity);
                OnNotes.Remove(note);
                NoteEvent(false, note);
            }

            if (CurrentTime >= music.DurationMs)
            {
                Stop();
                EndOfTrack(this, null);
                Debug.WriteLine("MIDI PLAYER: END OF TRACK");
                //CurrentTime = 0;
            }
        }

        // change patches
        private void configureFs()
        {
            foreach (InstrumentTrack it in music.Instruments)
                fluidprocess.StandardInput.WriteLine("prog {0} {1}", it.Channel, it.Patch);
        }

        public void SetFilter(int channel, bool bSolo)
        {
            if (IsPlaying)
                throw new Exception("Cannot change filter during playback!");
            PlaybackFilter = delegate (InstrumentTrack it)
            {
                if (bSolo)
                    return it.Channel == channel;
                else
                    return it.Channel != channel;
            };
        }

        public void Play()
        {
            if (IsPlaying)
                return;

            configureFs();

            watch.Start();
            timer.Start();
            IsPlaying = true;

            Debug.WriteLine("Midi player START");
        }

        public void Stop()
        {
            if (!IsPlaying)
                return;

            fluidprocess.StandardInput.WriteLine("reset");
            OnNotes.Clear();
            watch.Stop();
            timer.Stop();
            IsPlaying = false;

            Debug.WriteLine("Midi player STOP");
        }

        public void Terminate()
        {
            Stop();
            fluidprocess.Kill();
        }
    }

    public class BiasStopwatch
    {
        private Stopwatch watch = new Stopwatch();
        private TimeSpan bias = new TimeSpan(0);

        public int ElapsedMilliseconds
        {
            get { return (int)bias.Add(watch.Elapsed).TotalMilliseconds; }
            set {
                bias = new TimeSpan(0,0,0,0,value);
                if (watch.IsRunning)
                    watch.Restart();
                else
                    watch.Reset();
            }
        }

        public bool IsRunning { get { return watch.IsRunning; }}

        public void Stop()
        {
            watch.Stop();    
        }

        public void Start()
        {
            watch.Start();
        }
    }
}
