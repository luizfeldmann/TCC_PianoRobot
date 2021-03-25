using System;
using System.Linq;
using System.Diagnostics;

namespace pianocam
{
    public partial class SequenceWindow : Gtk.Window
    {
        private EditorSession MySession;
        private MidiPlayer MPlayer;
        private ToolboxWindow toolwindow;

        public SequenceWindow() :
                base(Gtk.WindowType.Toplevel)
        {
            this.Build();


        }

        private void Setup()
        {
            Title += " - " + MySession.ProgramName;
            

            MPlayer = new MidiPlayer(MySession.Music);
            MPlayer.EndOfTrack += new EventHandler(OnEndOfTrack);

            vscale1.CanFocus = false;
            OnVscale1ValueChanged(this, null);

            timelineviewwidget1.Music = MySession.Music;
            timelineviewwidget1.KeysReference = keyboardwidget1.KeysList;
            timelineviewwidget1.timeline = MySession.Timeline;

            toolwindow = new ToolboxWindow();
            toolwindow.Timeline = MySession.Timeline;
            toolwindow.Owner = this;
            toolwindow.ShowAll();

			foreach (var inst in MySession.Music.Instruments)
                combobox1.AppendText(inst.ToString());
            combobox1.Active = MySession.SelectedTrack;

            GLib.Timeout.Add((uint)Configs.ReadInt("AnimationInterval"), new GLib.TimeoutHandler(AnimationFrame));
        }

        public static void NewSession(EditorSession mySess)
        {
            SequenceWindow win = new SequenceWindow();
            win.MySession = mySess;
            win.ShowAll();
            win.Setup();
        }

        protected void OnSaveActionActivated(object sender, EventArgs e)
        {
            MySession.SaveSession();
        }

        protected void OnPreferencesActionActivated(object sender, EventArgs e)
        {
            Configs.OpenEditor();
        }

        protected void OnGotoLastActionActivated(object sender, EventArgs e)
        {
            combobox1.Sensitive = false;
            saveAction.Sensitive = false;
            gotoLastAction.Sensitive = false;
            toolwindow.Sensitive = false;
            toolwindow.Visible = false;
			networkAction.Sensitive = false;
            hscaleZoom.Sensitive = false;
            toolsAction.Sensitive = false;
            MPlayer.Play();
        }

        protected void OnStopActionActivated(object sender, EventArgs e)
        {
            combobox1.Sensitive = true;
            saveAction.Sensitive = true;
            gotoLastAction.Sensitive = true;
            toolwindow.Visible = true;
            toolwindow.Sensitive = true;
			networkAction.Sensitive = true;
            hscaleZoom.Sensitive = true;
            toolsAction.Sensitive = true;
            MPlayer.Stop();
            OnCombobox1Changed(sender, e);

			if (robot != null)
			{
				robot.End();
				robot = null;
			}
        }

        protected void OnEndOfTrack(object sender, EventArgs e)
        {
			Gtk.Application.Invoke(delegate
			{
				OnStopActionActivated(sender, e);
			});
        }
        
		RobotPlayer robot;
        protected void OnNetworkActionActivated(object sender, EventArgs e)
        {         
			robot = new RobotPlayer(MySession.ProgramName, timelineviewwidget1.Track, MySession.Timeline, timelineviewwidget1.KeysReference);
			robot.controller.HardwareAborted += OnEndOfTrack;

            // animacao
            vscale1.Value = 0;
            MPlayer.SetFilter(timelineviewwidget1.Track.Channel, false);
            OnGotoLastActionActivated(sender, e);
        }

        protected void OnCombobox1Changed(object sender, EventArgs e)
        {
            InstrumentTrack seltrack = MySession.Music.Instruments.First(x => x.ToString() == combobox1.ActiveText);
            if (seltrack == null)
                return;
            
            MPlayer.SetFilter(seltrack.Channel, true);
            timelineviewwidget1.Track = seltrack;
			MySession.SelectedTrack = combobox1.Active;
        }

        protected void OnDeleteEvent(object o, Gtk.DeleteEventArgs args)
        {
            MPlayer.Terminate();
            toolwindow.Destroy();
            Gtk.Application.Quit();
        }

        private bool AnimationFrame()
        {
            try
            {
                timelineviewwidget1.CurrentTimeMs = MPlayer.CurrentTime;
                timelineviewwidget1.QueueDraw(); // atualiza linha do tempo a cada frame

                if (MPlayer.IsPlaying)
                {
                    vscale1.Value = MPlayer.CurrentTime;
                    vscale1.GdkWindow.ProcessUpdates(true);


                    if (robot != null)
                    {
                        robot.ProcessInstant(MPlayer.CurrentTime);
                        keyboardwidget1.SetOnNotes(robot.ActiveMidicodes); // vinculado ao robô atualizar pelo programa
                    }
                    else
                        keyboardwidget1.SetOnNotes(MPlayer.OnNotes.Select(x => x.NoteNumber).ToArray()); // desvinculado do robô atualizar pelo playback
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ANIMATION EXCEPTION: {0} {1}", ex.GetType(), ex.Message);
            }

            return true;
        }

        protected void OnVscale1ValueChanged(object sender, EventArgs e)
        {
            if (MPlayer.IsPlaying) // ao tocar a barra de rolagem fica insensivel
                return;

            if (vscale1.Adjustment.Upper != MySession.Music.DurationMs)
                vscale1.SetRange(0, MySession.Music.DurationMs);

            MPlayer.CurrentTime = (int)vscale1.Value;
        }

        public void SetCursorTime(int time)
        {
            timelineviewwidget1.CursorTimeMs = time;
        }
        public int GetCursorTime()
        {
            return (int)timelineviewwidget1.CursorTimeMs;
        }

        protected void OnToolsActionActivated(object sender, EventArgs e)
        {
            ToolsWindow toolsWindow = new ToolsWindow();
            toolsWindow.ThisSession = MySession;
			toolsWindow.timelineView = timelineviewwidget1;
            toolsWindow.ShowAll();
        }

        protected void OnHscaleZoomValueChanged(object sender, EventArgs e)
        {
            float factor = (float)hscaleZoom.Value / 100f;

            float pold = timelineviewwidget1.pixelsPerSecond;
            float pnew = (float)Configs.ReadInt("TimelinePixelPerSecond") * factor;


            float tc = timelineviewwidget1.CursorTimeMs;
            float taold = MPlayer.CurrentTime;//timelineviewwidget1.CurrentTimeMs;
            float tanew = tc + (pold / pnew) * (taold - tc);

            timelineviewwidget1.pixelsPerSecond = pnew;

            if (tanew >0 && tanew < MySession.Music.DurationMs )
                vscale1.Value = (int)tanew;
        }

        protected void OnScrollEvent(object o, Gtk.ScrollEventArgs args)
        {
            if (MPlayer.IsPlaying)
                return;
            
            int increment = args.Event.Direction == Gdk.ScrollDirection.Up ? 1 : -1;

            if (args.Event.State != Gdk.ModifierType.ControlMask)
            {
                int newtime = MPlayer.CurrentTime + (int)(increment * 50000f / hscaleZoom.Value);

                vscale1.Value = (int)Math.Min(Math.Max(0, newtime), MySession.Music.DurationMs);
            }
            else
            {
                hscaleZoom.Value -= hscaleZoom.Adjustment.StepIncrement * increment;
            }
        }


        protected void OnKeyPressEvent(object o, Gtk.KeyPressEventArgs args)
        {
            UndoHelper.CheckUndoKeypress(args);
        }
    }
}
