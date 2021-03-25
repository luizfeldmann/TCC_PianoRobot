using System;
using System.Collections.Generic;
using NAudio.Midi;

namespace pianocam
{
    [Serializable]
	public class SheetMusic
	{
        public string SequenceName = "";
		public InstrumentTrack[] Instruments;
        public float DurationMs { get; set; }

		public SheetMusic()
		{
            DurationMs = 0;
		}

		public void LoadFromMidi(string filename)
		{
			MidiFile mfile = new MidiFile(filename);
			List<KeyValuePair<int, int>> channel_x_patch = new List<KeyValuePair<int, int>>();
			List<MusicNote> notes = new List<MusicNote>();

            float tickDuration = 60000f / (float)(120 * mfile.DeltaTicksPerQuarterNote);

            for (int track = 0; track < mfile.Tracks; track++)
                foreach (MidiEvent me in mfile.Events[track])
			{
				if (MidiEvent.IsNoteOn(me))
				{
					NoteOnEvent noe = me as NoteOnEvent;
					MusicNote mn = new MusicNote()
					{
						StartTime = noe.AbsoluteTime * tickDuration,
						EndTime = (noe.AbsoluteTime + noe.NoteLength) * tickDuration,
						Channel = noe.Channel,
						NoteNumber = noe.NoteNumber,
                        Velocity = noe.Velocity,
						//NoteName = noe.NoteName
					};
                    DurationMs = Math.Max(DurationMs, mn.EndTime);
					notes.Add(mn);
				}
				if (me is MetaEvent)
				{
					switch ((me as MetaEvent).MetaEventType)
					{
						case MetaEventType.ProgramName:
						case MetaEventType.SequenceTrackName:
							SequenceName = (me as TextEvent).Text;
							break;

						case MetaEventType.SetTempo:
							TempoEvent te = me as TempoEvent;
							tickDuration = 60000f / (float)(te.Tempo * mfile.DeltaTicksPerQuarterNote);
						break;

                        case MetaEventType.TimeSignature:
                                TimeSignatureEvent ts = me as TimeSignatureEvent;
                            // not suported yet
                            break;
                                             
					}
				}
				if (me is PatchChangeEvent)
				{
					PatchChangeEvent pe = (me as PatchChangeEvent);
                    channel_x_patch.Add(new KeyValuePair<int, int>(pe.Channel, pe.Patch));
				}
			}

            Instruments = new InstrumentTrack[channel_x_patch.Count];
			for (int i = 0; i < Instruments.Length; i++)
			{
				Instruments[i] = new InstrumentTrack()
				{
                    Patch = channel_x_patch[i].Value,
                    Channel = channel_x_patch[i].Key,
                    PatchName = PatchChangeEvent.GetPatchName(channel_x_patch[i].Value),
                    Notes = notes.FindAll(x => x.Channel == channel_x_patch[i].Key).ToArray()
				};
			}
		}
	}

    [Serializable]
	public class InstrumentTrack
	{
		public int Channel;
		public int Patch;
		public string PatchName;
		public MusicNote[] Notes;

		public override string ToString()
		{
            return string.Format("CH {0} PA {1} [{2}]", Channel, Patch, PatchName);
		}
	}

    [Serializable]
	public class MusicNote
	{
		public float StartTime;
		public float EndTime;
		public float Duration { get { return EndTime - StartTime; } }
		public int Channel;
		public int Patch;
		public int NoteNumber;
        public int Velocity;
	}
}
