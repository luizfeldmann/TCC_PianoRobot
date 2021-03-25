using System;
using System.Linq;
using System.Collections.Generic;

namespace pianocam
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class TimelineViewWidget : Gtk.Bin
    {
        public float CurrentTimeMs { get; set; }
        public float CursorTimeMs { get; set; }

        public SheetMusic Music;
        public InstrumentTrack Track;
        public Timeline timeline;
        public PianoKey[] KeysReference;

        public float pixelsPerSecond = Configs.ReadInt("TimelinePixelPerSecond");
        private int tickmarkInterval = Configs.ReadInt("TimelineTickmarkInterval");
        private int pathPlotResolutionMs = Configs.ReadInt("PathPlotResolutionMs");

        private int Height;
        private int Width;

        private object CurrentSelection;
        private List<KeyValuePair<object, Cairo.Rectangle>> SelectableObjects = new List<KeyValuePair<object, Cairo.Rectangle>>();

        private static readonly Cairo.Color DeltaTimeColor = new Cairo.Color(0.1,0.1,0.1);
        private static readonly Cairo.Color BackgroundColor = ColorFromHex("FFFFFF");
        private static readonly Cairo.Color SelectionColor = new Cairo.Color(0.5,0,0.5,1);
        private static readonly Cairo.Color[] KeyframeColor = { new Cairo.Color(0.9, 0.9, 0.0, 0.5), new Cairo.Color(0.0, 0.9, 0.0, 0.5), ColorFromHex("FF9900") };
        private static readonly Cairo.Color TickColor = new Cairo.Color(0.5,0.5,0.5, 0.2);
        private static readonly Cairo.Color[] FingerColors = new Cairo.Color[] 
        {
            ColorFromHex("8EB8E5"),
            ColorFromHex("533A7B"),
            ColorFromHex("888098"),
            ColorFromHex("CFB3CD"),
            ColorFromHex("DFC2F2"),

            ColorFromHex("584B53"),
            ColorFromHex("9D5C63"),
            ColorFromHex("62BEC1"),
            ColorFromHex("5AD2F4"),
            ColorFromHex("EF6F6C")
        };

        public TimelineViewWidget()
        {
            this.Build();
        }

        private int TimeMsToY(float time)
        {
            return (int)(Height - pixelsPerSecond * (time - CurrentTimeMs)/1000f);
        }

        protected PianoKey GetKey(int midicode)
        {
                return KeysReference.First(x => x.MidiNumber == midicode);
        }

        private static Cairo.Color ColorFromHex(string hex)
        {
            int red = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            int green = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            int blue = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.AllowHexSpecifier);

            return new Cairo.Color(red / 256f, green / 256f, blue / 256f, 0.5);
        }

        protected void OnDrawAreaExposeEvent(object o, Gtk.ExposeEventArgs args)
        {
            if (Music == null || Track == null ||  timeline == null)
                return;
            
            Height = args.Event.Area.Height;
            Width = args.Event.Area.Width;


            SelectableObjects.Clear();
            Cairo.Context cc = Gdk.CairoHelper.Create(args.Event.Window);
            cc.SetSourceColor(BackgroundColor);
            cc.Paint();

            // draw tickmarks
            cc.LineWidth = 1f;
            cc.SetSourceColor(TickColor);
            cc.SelectFontFace("Arial", Cairo.FontSlant.Normal, Cairo.FontWeight.Normal);
            cc.SetFontSize(10);
            for (int time = 0; time < Music.DurationMs; time += tickmarkInterval)
            {
                int y = TimeMsToY(time);
                if (y < 0 || y > Height)
                    continue;

                cc.MoveTo(0, y);
                cc.ShowText(TimeSpan.FromMilliseconds(time).ToString("mm\\:ss\\.ff"));
                cc.LineTo(Width, y);
                cc.Stroke();
            }

            // draw reference guide
            foreach (var key in KeysReference.Where(x => x.MidiNumber % 12 == 0 || x.MidiNumber % 12 == 5))
            {
                cc.MoveTo(key.StartPixel, 0);
                cc.LineTo(key.StartPixel, Height);
                cc.Stroke();
            }

            // draw notes
            foreach (var note in Track.Notes)
            {
                int bottomY = TimeMsToY(note.StartTime);
                int topY = TimeMsToY(note.EndTime);

                if (bottomY < 0 || topY > Height)
                    continue;
                var key = GetKey(note.NoteNumber);


                Cairo.Rectangle rect = new Cairo.Rectangle(key.StartPixel, topY, key.EndPixel - key.StartPixel, bottomY - topY);
                SelectableObjects.Add(new KeyValuePair<object, Cairo.Rectangle>(note,rect));

                if (note == CurrentSelection)
                    cc.SetSourceColor(SelectionColor);
                else
                    cc.SetSourceColor(key.GetColor(true));

                cc.Rectangle(rect);
                cc.Fill(); // preencher
            }

            // draw paths
            PianoKey referenceKey = GetKey(RobotStateHelper.CoordinateSystemOffset);
            float keywidth = referenceKey.EndPixel - referenceKey.StartPixel;

            float[] oldx = new float[10];
            for (int time = (int)CurrentTimeMs; time < CurrentTimeMs + (float)(1000f * Height) / pixelsPerSecond; time += pathPlotResolutionMs)
            {
                RobotState state = timeline.InterpolateState(time);
                int y = TimeMsToY(time);
                int yold = TimeMsToY(time - pathPlotResolutionMs);

                for (int f = 0; f < oldx.Length; f++)
                {
                    float coordinate = RobotStateHelper.GetFingerPosition(f, state);
                    float x = (referenceKey.StartPixel + keywidth / 2) + coordinate * keywidth;

                    if (time > CurrentTimeMs)
                    {
                        cc.LineWidth = RobotStateHelper.GetFinger(f, state).bMoving ? keywidth / 6 : keywidth / 2;
                        cc.SetSourceColor(FingerColors[f]);
                        cc.MoveTo(oldx[f], yold);
                        cc.LineTo(x, y);
                        cc.Stroke();
                    }

                    oldx[f] = x;
                }
            }

            // draw cursor
            int cursorY = TimeMsToY(CursorTimeMs);
            if (cursorY > 0 && cursorY < Height)
            {
                cc.LineWidth = 1;
                cc.SetSourceRGB(1, 0, 0);
                cc.MoveTo(0, cursorY);
                cc.LineTo(Width, cursorY);
                cc.Stroke();
            }

            // draw keyframe bar
            Keyframe lastKf = null;
            cc.LineWidth = 5;
            foreach (Keyframe key in timeline.GetKeyframes())
            {
                int y = TimeMsToY(key.Time);

                if (lastKf != null)
                {
                    int y2 = TimeMsToY(lastKf.Time);
                    int midpoint = (int)(0.5f * (y2 + y));

                    if (midpoint > 0 && midpoint < Height)
                    {
                        string deltatext = String.Format("ΔT {0:0.00}", (key.Time - lastKf.Time) / 1000f);
                        var extent = cc.TextExtents(deltatext);

                        cc.SetSourceColor(DeltaTimeColor);

                        if (y2 - y > extent.Width) 
                        {
                            cc.MoveTo(Width - extent.Height, midpoint - extent.Width / 2f);
                            cc.Save();
                            cc.Rotate(Math.PI / 2f);
                            cc.ShowText(deltatext);
                            cc.Restore();
                        }
                        else
                        {
                            cc.MoveTo(Width - extent.Width, midpoint);
                            cc.ShowText(deltatext);
                        }
                    }
                }

                lastKf = key;

                if (y < 0 || y > Height)
                    continue;

                cc.SetSourceColor(KeyframeColor[(int)key.Time == (int)CursorTimeMs ? 1 : 0]);
                cc.MoveTo(0, y - cc.LineWidth / 2);
                cc.LineTo(Width, y - cc.LineWidth / 2);
                cc.Stroke();

                var state = timeline.InterpolateState(key.Time);
                cc.SetSourceColor(KeyframeColor[2]);
                for (int j = 0; j < 2; j++)
                {
                    cc.MoveTo((referenceKey.StartPixel) + (j == 0 ? state.LeftHand.Position : state.RightHand.Position) * keywidth, y - cc.LineWidth / 2);
                    cc.LineTo(cc.CurrentPoint.X + 5 * keywidth, cc.CurrentPoint.Y);
                    cc.Stroke();
                }

            }

            cc.Dispose();
        }


        protected void OnDrawAreaButtonPressEvent(object o, Gtk.ButtonPressEventArgs args)
        {
            var pair = SelectableObjects.FirstOrDefault(x =>
                                      x.Value.X < args.Event.X && x.Value.X + x.Value.Width > args.Event.X
                                                                && x.Value.Y < args.Event.Y && x.Value.Y + x.Value.Height > args.Event.Y);

            //if (pair.Key != null)
                CurrentSelection = pair.Key;
            

            CursorTimeMs = CurrentTimeMs + 1000 * (this.Allocation.Height - (float)args.Event.Y) / pixelsPerSecond;
            this.GrabFocus();
        }

        [GLib.ConnectBefore]
        protected void OnKeyPressEvent(object o, Gtk.KeyPressEventArgs args)
        {
            if (CurrentSelection == null)
                return;

            int move = 0;
            if (args.Event.Key == Gdk.Key.Left)
                move = -1;
            else if (args.Event.Key == Gdk.Key.Right)
                move = 1;

            if (CurrentSelection is MusicNote && move != 0)
            {
                (CurrentSelection as MusicNote).NoteNumber += move;
                UndoHelper.AddMemory(CurrentSelection).action = (object obj) => (obj as MusicNote).NoteNumber -= move;
            }
        }

        protected void OnFocusOutEvent(object o, Gtk.FocusOutEventArgs args)
        {
            //if (vs)
            //    this.GrabFocus
        }

        protected void OnScrollEvent(object o, Gtk.ScrollEventArgs args)
        {
            //args.Event.
            //System.Diagnostics.Debug.WriteLine("SCROLL in timeline");
        }
    }
}
