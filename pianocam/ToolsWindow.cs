using System;
using System.Collections.Generic;
using System.Linq;

namespace pianocam
{
    public partial class ToolsWindow : Gtk.Window
    {
        public EditorSession ThisSession;
		public TimelineViewWidget timelineView;

        public ToolsWindow() :
                base(Gtk.WindowType.Toplevel)
        {
            this.Build();
            TransposeSetup();
        }

        #region Transpose

        private struct KeySignature
        {
            public string ShortName;
            public string LongName;
            public KeySignatureType Type;
            public int Delta;
            public int BaseMidiNumber;
            public int[] Notes;
        }

        private enum KeySignatureType
        {
            KST_Minor = 0,
            KST_Major = 1
        }

        private readonly KeySignature[] signatures = new KeySignature[]
        {
            new KeySignature() {Type = KeySignatureType.KST_Major, BaseMidiNumber = 0, Delta = 0, ShortName = "C", LongName = "Dó Maior"},
            new KeySignature() {Type = KeySignatureType.KST_Major, BaseMidiNumber = 1, Delta = 1, ShortName = "C#", LongName = "Dó sustenido Maior"},
            new KeySignature() {Type = KeySignatureType.KST_Major, BaseMidiNumber = 2, Delta = 2, ShortName = "D", LongName = "Ré Maior"},
            new KeySignature() {Type = KeySignatureType.KST_Major, BaseMidiNumber = 3, Delta = 3, ShortName = "D#", LongName = "Ré sustenido Maior"},
            new KeySignature() {Type = KeySignatureType.KST_Major, BaseMidiNumber = 4, Delta = 4, ShortName = "E", LongName = "Mi Maior"},
            new KeySignature() {Type = KeySignatureType.KST_Major, BaseMidiNumber = 5, Delta = 5, ShortName = "F", LongName = "Fá Maior"},
            new KeySignature() {Type = KeySignatureType.KST_Major, BaseMidiNumber = 6, Delta = 6, ShortName = "F#", LongName = "Fá sustenido Maior"},
            new KeySignature() {Type = KeySignatureType.KST_Major, BaseMidiNumber = 7, Delta = 7, ShortName = "G", LongName = "Sol Maior"},
            new KeySignature() {Type = KeySignatureType.KST_Major, BaseMidiNumber = 8, Delta = 8, ShortName = "G#", LongName = "Sol sustenido Maior"},
            new KeySignature() {Type = KeySignatureType.KST_Major, BaseMidiNumber = 9, Delta = 9, ShortName = "A", LongName = "Lá Maior"},
            new KeySignature() {Type = KeySignatureType.KST_Major, BaseMidiNumber = 10, Delta = 10, ShortName = "A#", LongName = "Lá sustenido Maior"},
            new KeySignature() {Type = KeySignatureType.KST_Major, BaseMidiNumber = 11, Delta = 11, ShortName = "B", LongName = "Si Maior"},

            new KeySignature() {Type = KeySignatureType.KST_Minor, BaseMidiNumber = 9, Delta = 0, ShortName = "Am", LongName = "Lá menor"},
            new KeySignature() {Type = KeySignatureType.KST_Minor, BaseMidiNumber = 10, Delta = 1, ShortName = "Bbm", LongName = "Si bemol menor"},
            new KeySignature() {Type = KeySignatureType.KST_Minor, BaseMidiNumber = 11, Delta = 2, ShortName = "Bm", LongName = "Si menor"},
            new KeySignature() {Type = KeySignatureType.KST_Minor, BaseMidiNumber = 12, Delta = 3, ShortName = "Cm", LongName = "Dó menor"},
            new KeySignature() {Type = KeySignatureType.KST_Minor, BaseMidiNumber = 13, Delta = 4, ShortName = "C#m", LongName = "Dó sustenido menor"},
            new KeySignature() {Type = KeySignatureType.KST_Minor, BaseMidiNumber = 14, Delta = 5, ShortName = "Dm", LongName = "Ré menor"},
            new KeySignature() {Type = KeySignatureType.KST_Minor, BaseMidiNumber = 15, Delta = 6, ShortName = "Ebm", LongName = "Mi bemol menor"},
            new KeySignature() {Type = KeySignatureType.KST_Minor, BaseMidiNumber = 16, Delta = 7, ShortName = "Em", LongName = "Mi menor"},
            new KeySignature() {Type = KeySignatureType.KST_Minor, BaseMidiNumber = 17, Delta = 8, ShortName = "Fm", LongName = "Fá menor"},
            new KeySignature() {Type = KeySignatureType.KST_Minor, BaseMidiNumber = 18, Delta = 9, ShortName = "F#m", LongName = "Fá sustenido menor"},
            new KeySignature() {Type = KeySignatureType.KST_Minor, BaseMidiNumber = 19, Delta = 10, ShortName = "Gm", LongName = "Sol menor"},
            new KeySignature() {Type = KeySignatureType.KST_Minor, BaseMidiNumber = 20, Delta = 11, ShortName = "G#m", LongName = "Sol sustenido menor"},
        };

        private readonly string[] KeySymbols = { "C", "C#/Db","D","D#/Eb","E","F","F#/Gb","G","G#/Ab","A","A#/Bb", "B"};
        private readonly int[,] keyNotesProgression = new int[,] 
        {   { 0, 3, 3, 5, 7, 8, 10}, /* escala menor*/
            { 0, 2, 4, 5, 7, 9, 11 } /* escala maior */};


        private int midiDelta = 0;

        protected void TransposeSetup()
        {
            for (int k = 0; k < signatures.Length; k++)
            {
                signatures[k].Notes = new int[7];
                for (int i = 0; i < 7; i++)
                {
                    signatures[k].Notes[i] = signatures[k].BaseMidiNumber + keyNotesProgression[(int)signatures[k].Type,i];
                }
            }
            octaveShift.Value = 0;

            System.Threading.Tasks.Task.Run(() => 
            {
                while(ThisSession == null){}

                    foreach (var it in ThisSession.Music.Instruments)
                        transposeTargeSelection.AppendText(it.ToString());

                transposeTargeSelection.Active = 0;
                
            });

            OnTransposeChanged(null, null);
        }

        protected void RebindCombobox(Gtk.ComboBox cb, string[] values)
        {
            cb.Clear();

            var store = new Gtk.ListStore(typeof(string));
            Gtk.CellRendererText text = new Gtk.CellRendererText();
            cb.PackEnd(text, true);
            cb.AddAttribute(text, "text", 0);

            foreach (var curr in values)
            {
                store.AppendValues(curr);
            }

            cb.Model = store;
            cb.Active = 0;
        }

        protected string GetKeySymbol(int note)
        {
            return KeySymbols[note % 12];
        }

        protected void FillOriginTargetCombos()
        {
            KeySignatureType type = (KeySignatureType)keySignatureTypeSelector.Active;

            List<string> items = new List<string>();
            foreach (var key in signatures.Where(x => x.Type == type))
            {
                string label = string.Format("{0} [{1}]", key.ShortName, key.LongName);
                items.Add(label);
            }
            RebindCombobox(originalKey, items.ToArray());
            RebindCombobox(desiredKey, items.ToArray());
        }

        protected void FillExplanatoryText()
        {
            KeySignature original = signatures[originalKey.Active + 12 * (1 - (int)keySignatureTypeSelector.Active)];
            KeySignature desired  = signatures[desiredKey.Active  + 12 * (1 - (int)keySignatureTypeSelector.Active)];

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendFormat("{0}\t\t->\t\t{1}\n----\t\t\t\t----\n", original.ShortName, desired.ShortName);
            for (int i = 0; i < original.Notes.Length; i++)
                sb.AppendFormat("{0}\t\t->\t\t{1}\n", GetKeySymbol(original.Notes[i]), GetKeySymbol(desired.Notes[i]));

            sb.AppendFormat("{0} {1} oitava{2}\n", octaveShift.Value < 0 ? "-" : "+", Math.Abs(octaveShift.Value), Math.Abs(octaveShift.Value) > 1 ? "s" : "");

            midiDelta = (int)octaveShift.Value * 12 + desired.Delta - original.Delta;

            sb.AppendFormat("\nVariação total de nota MIDI: {0}\n", midiDelta);

            textview1.Buffer.Clear();
            textview1.Buffer.Text = sb.ToString();
        }

        bool bIgnoreUpdate = false;
        protected void OnTransposeChanged(object sender, EventArgs e)
        {
            if (bIgnoreUpdate)
                return;

            bIgnoreUpdate = true;
            if (sender != originalKey && sender != desiredKey)
                FillOriginTargetCombos();
            
            FillExplanatoryText();
            bIgnoreUpdate = false;
        }

        protected void transposeRun(object sender, EventArgs e)
        {
            var it = ThisSession.Music.Instruments[transposeTargeSelection.Active];
                foreach (var note in it.Notes)
                {
                    note.NoteNumber = note.NoteNumber + midiDelta;
                }
        }

        protected void OnDetectScaleButtonClicked(object sender, EventArgs e)
        {
            int totalNotes = 0;
            int[] results = new int[signatures.Length]; //[signature, accidentals]
            for (int i = 0; i < signatures.Length; i++)
            {
                results[i] = 0;
                var it = ThisSession.Music.Instruments[transposeTargeSelection.Active];

                    foreach (var note in it.Notes)
                    {
                        totalNotes++;
                        if (!signatures[i].Notes.Any(x => x % 12 == note.NoteNumber % 12))
                            results[i]++;
                    }

            }

            int min = 1000000;
            int chosen = 0;
            for (int i = 0; i < results.Length; i++)
            {
                if (results[i] < min)
                {
                    chosen = i;
                    min = results[i];
                }
            }
            float precision = (1f - ((float)min/(float)totalNotes))*100f;

            string report = string.Format("[{0}] {1}\n{2} acidentais entre {3}\nAdequação {4}", signatures[chosen].ShortName, signatures[chosen].LongName, min, totalNotes, precision);

            Gtk.MessageDialog md = new Gtk.MessageDialog(this,
                Gtk.DialogFlags.DestroyWithParent, Gtk.MessageType.Info, Gtk.ButtonsType.Close, report);
            md.Run();
            md.Destroy();
        }

        #endregion

        #region Conformity

        private readonly int FirstKey = Configs.ReadInt("PianoFirstKey");
        private readonly int LastKey = Configs.ReadInt("PianoLastKey");

        protected void OnCorrectButtonClicked(object sender, EventArgs e)
        {
            RunConformity();
        }

        protected void WriteNoteDescription(System.Text.StringBuilder sb, MusicNote note)
        {
            sb.AppendFormat("\n[CH{0} PA {1}] {2}{3}({4})\t{5}\t\t-\t\t{6}", note.Channel,note.Patch, GetKeySymbol(note.NoteNumber),(int)(note.NoteNumber / 12),note.NoteNumber, note.StartTime, note.EndTime);
        }

        protected void RunConformity()
        {
            int totalErrors = 0;
            int localErrors = 0;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            // note overlay
            sb.AppendLine("\n\nANÁLISE DE SOBREPOSIÇÃO\n");
            localErrors = 0;

            foreach (var it in ThisSession.Music.Instruments)
            {
                MusicNote note = null;
                while ((note = it.Notes.FirstOrDefault(x => it.Notes.Any(y => x != y && x.NoteNumber == y.NoteNumber && x.StartTime >= y.StartTime && x.EndTime <= y.EndTime))) != null)
                {
                    localErrors++;
                    totalErrors++;
                    WriteNoteDescription(sb, note);
                    it.Notes = it.Notes.Except(new MusicNote[] { note }).ToArray();
                }
            }


            sb.AppendFormat("\nPares sobrepostos: {0}\n", localErrors);
            sb.AppendLine("===========================");

            // note out of range
            sb.AppendLine("\n\nANÁLISE DE INTERVALO\n");
            localErrors = 0;

            foreach (var it in ThisSession.Music.Instruments)
            {
                var except = it.Notes.Where(x => x.NoteNumber < FirstKey || x.NoteNumber > LastKey);
                localErrors += except.Count();
                totalErrors += except.Count();

                it.Notes = it.Notes.Except(except).ToArray();
            }

            sb.AppendFormat("\nNotas fora do intervalo: {0}\n", localErrors);
            sb.AppendLine("===========================");
            sb.AppendFormat("\n\nTotal de inconformidades: {0}\n", totalErrors);

            textviewConformity.Buffer.Text = sb.ToString();
        }

        #endregion

        #region Time remap

        protected void OnApplyTimeRescaleClicked(object sender, EventArgs e)
        {
            float scale = 1f/(float)timeRescaleRange.Value;

            foreach (var it in ThisSession.Music.Instruments)
            {
                foreach (var note in it.Notes)
                {
                    note.StartTime *= scale;
                    note.EndTime *= scale;
                }
            }

			foreach (var kf in ThisSession.Timeline.GetKeyframes())
				kf.Time = (int)(kf.Time*scale);
            
            ThisSession.Music.DurationMs *= scale;

            timeRescaleRange.Value = scale;
        }

        protected void OnInsertGapButtonClicked(object sender, EventArgs e)
        {
            int gapMs = (int)gapRange.Value;

            List<int> insertGap = new List<int>();
            foreach (var it in ThisSession.Music.Instruments)
            {
                foreach (var note in it.Notes)
                    insertGap.Add((int)note.EndTime);
            }

            insertGap = insertGap.Distinct().ToList();

            foreach (var it in ThisSession.Music.Instruments)
            {
                foreach (int gapTime in insertGap)
                {
                    foreach (var note in it.Notes.Where(x => x.StartTime >= gapTime))
                    {
                        note.StartTime += gapMs;
                        note.EndTime += gapMs;
                    }
                }
            }

            ThisSession.Music.DurationMs += insertGap.Count() * gapMs;
        }

        #endregion

        protected void closeWindow(object sender, EventArgs e)
        {
            this.Destroy();
        }

		protected void OnButtonRemoveTimeClicked(object sender, EventArgs e)
		{
			int remms = -(int)(timeTemperValue.Value * 1000);
			timeshift(remms);
		}

        private void timeshift(int ms)
		{
			int cursor = (int)timelineView.CursorTimeMs;
				
            
            foreach (var it in ThisSession.Music.Instruments)
            {
				List<MusicNote> removelist_note = new List<MusicNote>();

                foreach (var note in it.Notes.Where(x => x.StartTime >= cursor))
                {
                    note.StartTime += ms;
                    note.EndTime += ms;

					if (note.StartTime < cursor)
						removelist_note.Add(note);
                }

				it.Notes = it.Notes.Except(removelist_note).ToArray();
            }

			List<Keyframe> removelist_keyframe = new List<Keyframe>();
			foreach (var kf in ThisSession.Timeline.GetKeyframes().Where(kk => kk.Time > cursor))
			{
				kf.Time += ms;
				if (kf.Time <= cursor)
					removelist_keyframe.Add(kf);
			}

			foreach (var kf in removelist_keyframe)
			    ThisSession.Timeline.DeleteKeyframe(kf);

			ThisSession.Music.DurationMs += ms;
		}

		protected void OnButtonAddTimeClicked(object sender, EventArgs e)
		{
			int addms = (int)(timeTemperValue.Value * 1000);
			timeshift(addms);
		}

		protected void OnTrimAfterBtnClicked(object sender, EventArgs e)
		{
			int cursor = (int)timelineView.CursorTimeMs;
			timeshift(-(int)(ThisSession.Music.DurationMs - cursor));
		}

		protected void OnTrimBeforeBtnClicked(object sender, EventArgs e)
		{
			int cursor = (int)timelineView.CursorTimeMs;
			timelineView.CursorTimeMs = 0;

			timeshift(-cursor);
		}
	}
}
