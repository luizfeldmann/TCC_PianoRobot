using System;
using System.Linq;

namespace pianocam
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class KeyboardWidget : Gtk.Bin
    {
        public int FirstKey { get; private set; }
        public int LastKey { get; private set; }
        public PianoKey[] KeysList { get; private set; }
        private PianoKey[] WhiteKeys;
        private PianoKey[] BlackKeys;
        public static readonly string[] KeySymbols = { "C","C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"};


        public KeyboardWidget()
        {
            this.Build();

            FirstKey = Configs.ReadInt("PianoFirstKey");
            LastKey = Configs.ReadInt("PianoLastKey");

            // generate key list
            KeysList = new PianoKey[LastKey - FirstKey + 1];
            for (int i = 0; i < KeysList.Length;i++)
            {
                var key = new PianoKey();
                key.MidiNumber = FirstKey + i;
                key.Octave = (key.MidiNumber / 12) - 1;
                key.Symbol = KeySymbols[key.MidiNumber % 12] + key.Octave;
                key.bSharp = key.Symbol.Contains("#");
                KeysList[i] = key;
            }
            WhiteKeys = KeysList.Where(x => !x.bSharp).ToArray();
            BlackKeys = KeysList.Where(x => x.bSharp).ToArray();
        }

        public void SetOnNotes(int[] midicodes)
        {
            bool bAnyChange = false;
            foreach (PianoKey key in KeysList)
            {
                bool bOn = midicodes.Contains(key.MidiNumber);

                if (key.bDisplayOn != bOn)
                    bAnyChange = true;
                
                key.bDisplayOn = bOn;
            }

            if (bAnyChange) // Repintar apenas caso necessário aka. alguma alteração de tecla ativa
                this.QueueDraw();
        }

        protected void OnDrawareaExposeEvent(object o, Gtk.ExposeEventArgs args)
        {
            var area = args.Event.Area;
            float width = area.Width;
            float height = area.Height;
            float keywidth = width / WhiteKeys.Length;

            Cairo.Context cc = Gdk.CairoHelper.Create(args.Event.Window);

            // apagar com branco
            //cc.SetSourceColor(new Cairo.Color(1, 1, 1));
            //cc.Rectangle(0, 0, width, height);
            //cc.Fill();

            // presets
            cc.SelectFontFace("Arial", Cairo.FontSlant.Normal, Cairo.FontWeight.Normal);
            cc.SetFontSize(keywidth/2);
            cc.LineWidth = 5;

            // teclas brancas
            for (int i = 0; i < WhiteKeys.Length;i++)
            {
                var key = WhiteKeys[i];
                key.StartPixel = i * keywidth;
                key.EndPixel = (i + 1) * keywidth;
                key.WidgetOrder = i;

                cc.SetSourceColor(key.GetColor());
                cc.Rectangle(key.StartPixel, 0, key.EndPixel - key.StartPixel, height);
                cc.Fill();

                cc.SetSourceColor(new Cairo.Color(0,0,0));
                cc.MoveTo(key.StartPixel, height * ( 0.65 + 0.1 * (i % 2) ) );
                cc.ShowText(key.Symbol);

                cc.MoveTo(key.StartPixel, height * 0.9);
                cc.ShowText(key.MidiNumber.ToString());
            }

            // teclas pretas
            for (int i = 0; i < BlackKeys.Length; i++)
            {
                var key = BlackKeys[i];
                key.StartPixel = WhiteKeys.First(x=>x.MidiNumber == key.MidiNumber-1).EndPixel - keywidth/4;
                key.EndPixel = key.StartPixel + keywidth/2;
                key.WidgetOrder = i;

                cc.Rectangle(key.StartPixel, 0, key.EndPixel - key.StartPixel, height / 2);
                cc.SetSourceColor(key.GetColor());
                cc.Fill();
            }

            cc.Dispose();
        }
    }

    public class PianoKey
    {
        public bool bDisplayOn;
        public int MidiNumber;
        public int Octave;
        public string Symbol;
        public bool bSharp;
        public float StartPixel;
        public float EndPixel;
        public int WidgetOrder;

        private static readonly Cairo.Color[] white = { new Cairo.Color(1, 1, 1), new Cairo.Color(0.92, 0.92, 1) };
        private static readonly Cairo.Color[] black = { new Cairo.Color(0, 0, 0), new Cairo.Color(0.3, 0.3, 0.3) };
        private static readonly Cairo.Color[] played = { new Cairo.Color(0.3, 0.7, 0.3), new Cairo.Color(0.3, 0.3, 0.7), new Cairo.Color(0.7, 0.3, 0.3) };

        public Cairo.Color GetColor(bool bPlay)
        {
            if (bPlay)
                return played[bSharp ? 2 : (WidgetOrder % 2)];
            else
                return bSharp ? black[WidgetOrder % 2] : white[WidgetOrder % 2];
        }

        public Cairo.Color GetColor()
        {
            return GetColor(bDisplayOn);
        }
    }
}
