using System;
using System.Collections.Generic;

namespace pianocam
{
    public static class UndoHelper
    {
        private static readonly int maxHistorySize = 20;
        private static List<UndoInfo> history = new List<UndoInfo>();

        public class UndoInfo
        {
            public Action<object> action;
            public object memory;
        }

        public static void PerformUndo()
        {
            if (history.Count <= 0)
                return;
            try
            {
                history[history.Count - 1].action.DynamicInvoke(history[history.Count - 1].memory);
            }
            catch (Exception ex)
            {
                Gtk.MessageDialog md = new Gtk.MessageDialog(null,
                                                             Gtk.DialogFlags.DestroyWithParent, Gtk.MessageType.Warning, Gtk.ButtonsType.Close, "ERRO AO DESFAZER:\r\n{0}", ex.Message);
                md.Run();
                md.Destroy();
            }

            history.RemoveAt(history.Count - 1);
        }

        public static UndoInfo AddMemory(object mem)
        {
            var kvp = new UndoInfo() {memory = mem, action = (object m) => {}};
            history.Add(kvp);

            while (history.Count > maxHistorySize)
                history.RemoveAt(0);

            return kvp;
        }

        public static void AddUndoAction(Action<object> action)
        {
            history.Add(new UndoInfo() { memory = null, action = action });

            while (history.Count > maxHistorySize)
                history.RemoveAt(0);
        }

        public static bool CheckUndoKeypress(Gtk.KeyPressEventArgs args)
        {
            //System.Diagnostics.Debug.WriteLine(args.Event.State);
            if (((args.Event.State == Gdk.ModifierType.ControlMask) || args.Event.State == Gdk.ModifierType.Mod1Mask) && (args.Event.Key == Gdk.Key.z || args.Event.Key == Gdk.Key.Z))
            {
                UndoHelper.PerformUndo();
                return true;
            }
            
            return false;

        }
    }
}
