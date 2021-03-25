using System;
using System.Linq;
using Gtk;

namespace pianocam
{
    public partial class ToolboxWindow : Gtk.Window
    {
        public Timeline Timeline { get; set; }
        public SequenceWindow Owner { get; set; }

        private Gtk.SpinButton[] Nudges;

        public ToolboxWindow() :
                base(Gtk.WindowType.Toplevel)
        {
            this.Build();
            refreshAction.Activate();

            Nudges = new Gtk.SpinButton[] { def1, def2, def3, def4, def5, def6, def7, def8, def9, def10};

            lefthandScale.Adjustment.Lower = RobotStateHelper.MinPosition;
            lefthandScale.Adjustment.Upper = RobotStateHelper.MaxPosition;

            righthandScale.Adjustment.Lower = lefthandScale.Adjustment.Lower;
            righthandScale.Adjustment.Upper = lefthandScale.Adjustment.Upper;

            for (int i = 0; i < Nudges.Length; i++)
            {
                Nudges[i].Adjustment.Upper = RobotStateHelper.MaxDeflection;
                Nudges[i].Adjustment.Lower = -Nudges[i].Adjustment.Upper;
            }

            System.Threading.Tasks.Task.Run(() => { while (Timeline == null)
                { }
                LoadState(Timeline.GetAt(0)); });
        }

        private bool bLoadingState = false;
        protected void LoadState(Keyframe frame)
        {
            bLoadingState = true;

            Owner.SetCursorTime(frame.Time);

            lefthandScale.Value = frame.State.LeftHand.Position;
            righthandScale.Value = frame.State.RightHand.Position;

            for (int i = 0; i < Nudges.Length; i++)
            {
                Nudges[i].Value = (i < 5 ? frame.State.LeftHand : frame.State.RightHand).Fingers[i % 5].Deflection;
            }

            bLoadingState = false;
        }


        protected void OnGoBackActionActivated(object sender, EventArgs e)
        {
            Keyframe prev = Timeline.GetPrevious(Owner.GetCursorTime()-1);
            if (prev == null)
                return;
            
            LoadState(prev);
        }

        protected void OnGoForwardActionActivated(object sender, EventArgs e)
        {
            Keyframe next = Timeline.GetNext(Owner.GetCursorTime());
            if (next == null)
                return;

            LoadState(next);
        }

        protected void OnDeleteActionActivated(object sender, EventArgs e)
        {
            Keyframe curr = Timeline.GetAt(Owner.GetCursorTime());
            if (curr == null)
                return;
            if (curr.Time == 0)
                return;
            
            Timeline.DeleteKeyframe(curr);
            UndoHelper.AddUndoAction((object m) => Timeline.NewKeyframe(curr.Time).State = curr.State );
        }

        protected void OnConvertActionActivated(object sender, EventArgs e)
        {
            int time = Owner.GetCursorTime();

            RobotState state = new RobotState();
            state.LeftHand = new RobotHand() { Velocity = 0, Position = (float)lefthandScale.Value, Fingers = new RobotFinger[5] };
            state.RightHand = new RobotHand(){ Velocity = 0, Position = (float) righthandScale.Value, Fingers = new RobotFinger[5] };

            for (int i = 0; i < Nudges.Length; i++)
            {
                RobotHand hand = i < 5 ? state.LeftHand : state.RightHand;
                hand.Fingers[i % 5].Deflection = (float)Nudges[i].Value;
                
            }

            Keyframe kf = Timeline.GetAt(time);

            var status = time <= 0 ? RobotStateError.RSE_InvalidChange : RobotStateHelper.CheckState(state);
            if (status != RobotStateError.RSE_None)
            {
                MessageDialog md = new MessageDialog(this,
                DialogFlags.DestroyWithParent, MessageType.Warning, ButtonsType.Close, status.ToString());
                md.Run();
                md.Destroy();

                if (kf == null)
                    kf = Timeline.GetPrevious(time);

                LoadState(kf);

                return;
            }


            if (kf == null)
            {
                kf = Timeline.NewKeyframe(time);
                kf.State = state;
                UndoHelper.AddUndoAction((object m) => Timeline.DeleteKeyframe(kf));
            }
            else
            {
                UndoHelper.AddMemory(kf.State).action = (object old) => { kf.State = (RobotState)old; LoadState(kf);  };
                kf.State = state;
            }

            Owner.SetCursorTime(kf.Time);
        }

        protected void OnRefreshActionToggled(object sender, EventArgs e)
        {
            convertAction1.Sensitive = !refreshAction.Active;
        }

        protected void ValueChanged(object sender, EventArgs e)
        {
            if (refreshAction.Active && !bLoadingState)
                OnConvertActionActivated(sender, e);
        }

        private int selectedNudge = -1;
        [GLib.ConnectBefore]
        protected void OnKeyPressEvent(object o, KeyPressEventArgs args)
        {
            if (UndoHelper.CheckUndoKeypress(args))
                return;

            this.Focus = null;

            if (args.Event.Key >= Gdk.Key.Key_0 && args.Event.Key <= Gdk.Key.Key_9)
            {
                int num = args.Event.Key - Gdk.Key.Key_0;
                if (num == 0) num = 10;

                selectedNudge = num - 1;
            }   

            int adjustside = 0;
            switch (args.Event.Key)
            {
                case Gdk.Key.Up: OnGoForwardActionActivated(o, args); break;
                case Gdk.Key.Down: OnGoBackActionActivated(o, args); break;
                case Gdk.Key.Left: adjustside = -1; break;
                case Gdk.Key.Right: adjustside = 1; break;
            }

            if (selectedNudge >= 0 && adjustside != 0)
            {
                Nudges[selectedNudge].Value += adjustside;
            }
        }

        protected void OnKeyReleaseEvent(object o, KeyReleaseEventArgs args)
        {
            if (args.Event.Key >= Gdk.Key.Key_0 && args.Event.Key <= Gdk.Key.Key_9)
                selectedNudge = -1;
        }
    }
}
