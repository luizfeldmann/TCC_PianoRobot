using System;
using System.IO;
using System.Diagnostics;

namespace pianocam
{
    [Serializable]
    public class EditorSession
    {
        public SheetMusic Music { get; private set; }
        public Timeline Timeline { get; private set;  }
		public int SelectedTrack;

        public String FileName  { get {
                    return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "PROGRAMS", ProgramName + ".pcamseq");
            }}

        public string ProgramName { get; private set;}

        public EditorSession()
        {
            Debug.WriteLine("CREATED A SESSION");
        }

        public static EditorSession FromMidi(string midifilename)
        {
            Debug.WriteLine("Load from MIDI file: " + midifilename);

            EditorSession sess = new EditorSession();
            sess.Music = new SheetMusic();
            sess.Music.LoadFromMidi(midifilename);
            sess.ProgramName = sess.Music.SequenceName;

            if (sess.ProgramName == "")
                sess.ProgramName = System.IO.Path.GetFileNameWithoutExtension(midifilename); //+ DateTime.Now.ToString(" dd-MM-yyyy HH-mm");

            sess.AskForName();

            sess.Timeline = new Timeline();

            return sess;
        }

        public void AskForName()
        {
            var input = new Gtk.Entry(ProgramName);
            input.MaxLength = 32;
            Gtk.MessageDialog dialog = new Gtk.MessageDialog(null,
                     Gtk.DialogFlags.Modal, Gtk.MessageType.Question,
                     Gtk.ButtonsType.Ok, "INPUT PROGRAM NAME:");
            dialog.VBox.Add(input);
            dialog.ShowAll();
            dialog.Response += delegate (object o, Gtk.ResponseArgs args) {
                ProgramName = input.Text;
                dialog.Destroy();
            };

            dialog.Run();
        }

        public static EditorSession FromSaved(string savedfilename)
        {
            Debug.WriteLine("Load from saved program: " + savedfilename);
            var file = System.IO.File.OpenRead(savedfilename);
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            object output = binaryFormatter.Deserialize(file);
            file.Close();

            return (EditorSession)output;
        }

        public void SaveSession()
        {
            var file = System.IO.File.OpenWrite(FileName);
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            binaryFormatter.Serialize(file, this);
            file.Close();

            Debug.WriteLine("Saved session to file!");
        }
    }
}
