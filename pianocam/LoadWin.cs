using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;

namespace pianocam
{
    public partial class LoadWin : Gtk.Window
    {
        public LoadWin() :
                base(Gtk.WindowType.Toplevel)
        {
            this.Build();

			string myExe = Assembly.GetEntryAssembly().Location;
			string exePath = System.IO.Path.GetDirectoryName(myExe);
            string progPath = System.IO.Path.Combine(exePath, "PROGRAMS");

            Debug.WriteLine("Listing programs from " + progPath);

            IEnumerable files = Directory.EnumerateFiles(progPath);

            var model = generateTreeModel(treeview1, new string[]{"File","Path"}, typeof(string), typeof(string));

            foreach (string file in files)
            {
                string name = System.IO.Path.GetFileNameWithoutExtension(file);
                Debug.WriteLine("\t" + name);
                model.AppendValues(name, file);
            }

            ShowAll();
        }

        public Gtk.ListStore generateTreeModel(Gtk.TreeView tv, string[] headers, params Type[] types)
        {
            int i = 0;
            foreach (string h in headers)
            {
                Gtk.TreeViewColumn col = new Gtk.TreeViewColumn();
                Gtk.CellRendererText cell = new Gtk.CellRendererText();

                col.Title = h;
                col.PackStart(cell, true);

                tv.AppendColumn(col);
                col.AddAttribute(cell, "text", i);
                i++;
            }

            Gtk.ListStore listModel = new Gtk.ListStore(types);
            tv.Model = listModel;

            return listModel;
        }

        protected void OnTreeview1RowActivated(object o, Gtk.RowActivatedArgs args)
        {
            Gtk.TreeSelection selection = treeview1.Selection;
            Gtk.TreeModel model;
            Gtk.TreeIter iter;
            List<string> rc = new List<string>();
            if (selection.GetSelected(out model, out iter))
            {
                for (int n = 0; n < treeview1.Columns.Length; n++)
                {
                    rc.Add(model.GetValue(iter, n).ToString());
                }
            }

            SequenceWindow.NewSession(EditorSession.FromSaved(rc[1]));
            this.Destroy();
        }

        protected void OnImportButtonClicked(object sender, EventArgs e)
        {
            Gtk.FileChooserDialog filechooser =
        new Gtk.FileChooserDialog("Importar arquivo de partitura", this, Gtk.FileChooserAction.Open,
            "Cancel", Gtk.ResponseType.Cancel, "Import", Gtk.ResponseType.Accept);
            filechooser.Filter = new Gtk.FileFilter();
            filechooser.Filter.AddPattern("*.mid");
            filechooser.Filter.AddPattern("*.midi");
            filechooser.Filter.Name = "Arquivo MIDI;";
            filechooser.SelectMultiple = false;
            if (filechooser.Run() == (int)Gtk.ResponseType.Accept)
            {
                SequenceWindow.NewSession(EditorSession.FromMidi(filechooser.Filename));
                filechooser.Destroy();
                this.Destroy();
            }
        }

        protected void OnDeleteEvent(object o, Gtk.DeleteEventArgs args)
        {
            Gtk.Application.Quit();
        }
    }
}
