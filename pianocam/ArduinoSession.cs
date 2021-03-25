using System;
using System.IO.Ports;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Timers;
using System.Diagnostics;

namespace pianocam
{
    public class ArduinoSession : IDisposable
    {
		private SerialPort Port;
		private Timer SerialPollTimer;

        private const int BaudRate = 57600;
        private const Parity parity = Parity.None;
        private const int DataBits = 8;
        private const StopBits stopBits = StopBits.Two;

		public event EventHandler HardwareAborted;

        public ArduinoSession()
        {
            string portName = ChooseDialog();
            System.Diagnostics.Debug.WriteLine("ARDUINO SESSION START: " + portName);
            Port = new SerialPort(portName, BaudRate, parity, DataBits, stopBits);
            Port.DtrEnable = true;
            Port.RtsEnable = true;
            Port.Open();

			SerialPollTimer = new Timer(100);
			SerialPollTimer.Elapsed += (object sender, ElapsedEventArgs e) => PollPort();

			System.Threading.Thread.Sleep(5000);

			while (Port.BytesToRead > 0)
			    Port.ReadChar(); // limpar buffer

			SerialPollTimer.Start();
            System.Diagnostics.Debug.WriteLine("RUNNING COMMS");
        }

        public void Dispose()
        {
			SerialPollTimer.Stop();

            if (Port != null)
                if (Port.IsOpen)
                    Port.Close();

            System.Diagnostics.Debug.WriteLine("ARDUINO SESSION DISPOSED!");
        }
              
		private void PollPort()
		{
			if (Port.BytesToRead <= 0)
				return;
			
            
			string data = Port.ReadExisting();

			if (data.Contains("E") || data.Contains("R"))
				HardwareAborted(this, null);

			System.Diagnostics.Debug.WriteLine("ROBOT: " + data);
		}

        private string ChooseDialog()
        {
            int index;
            string[] availablePorts = SerialPort.GetPortNames();
            var input = new Gtk.ComboBox(availablePorts);
			input.Active = 0;

            Gtk.MessageDialog dialog = new Gtk.MessageDialog(null, Gtk.DialogFlags.Modal, Gtk.MessageType.Question, Gtk.ButtonsType.Ok, "PORTA SERIAL:");
            dialog.VBox.Add(input);
            dialog.ShowAll();
            dialog.Run();
            index = input.Active;
            dialog.Destroy();

            return availablePorts[index];
        }


        private void SendFrame(SerialFrame frame)
        {
            SendFrame(frame.GetData());
        }

        private void SendFrame(byte[] frame)
        {
            const byte SLIP_END = 0xC0;
            const byte SLIP_ESC = 0xDB;
            const byte SLIP_ESC_END = 0xDC;
            const byte SLIP_ESC_ESC = 0xDE;

            MemoryStream frame_escaped = new MemoryStream();

            frame_escaped.WriteByte(SLIP_END);

            for (int i = 0; i < frame.Length; i++)
            {
                switch (frame[i])
                {
                    case SLIP_END:
                        frame_escaped.WriteByte(SLIP_ESC);
                        frame_escaped.WriteByte(SLIP_ESC_END);
						Debug.WriteLine("ESCAPE_END");
                        break;
                    case SLIP_ESC:
                        frame_escaped.WriteByte(SLIP_ESC);
                        frame_escaped.WriteByte(SLIP_ESC_ESC);
						Debug.WriteLine("ESCAPE_ESC");
                        break;
                    default:
                        frame_escaped.WriteByte(frame[i]);
                        break;
                }
            }

            frame_escaped.WriteByte(SLIP_END);


            Port.Write(frame_escaped.ToArray(), 0, (int)frame_escaped.Length);
			//Debug.WriteLine("FRAME: " + BitConverter.ToString(frame_escaped.ToArray()).Replace("-","-"));
            frame_escaped.Close();
        }

        public void End()
        {
            SendFrame(new byte[]{(byte)'E'});
            System.Diagnostics.Debug.WriteLine("SEND CMD: END");
        }

        public void WriteLCD(string text)
        {
			byte[] pkt = new byte[34];

            for (int i = 0; i < pkt.Length; i++)
                pkt[i] = (byte)'\0';

			pkt[0] = (byte)'T';
			byte[] ascii = System.Text.Encoding.ASCII.GetBytes(text);
			for (int i = 0; i < Math.Min(32, ascii.Length); i++)
				pkt[i+1] = ascii[i];
			
            SendFrame(pkt);
            System.Diagnostics.Debug.WriteLine("SEND LCD: " + text);
        }
       
        public void SetAllFingers(bool[] states)
		{
			byte[] pkt = new byte[11];
			pkt[0] = (byte)'F';

			for (int i = 1; i < 11; i++)
			{
				pkt[i] = (byte)(states[i-1] ?  0xFF : 0x00);
			}

            SendFrame(pkt);
            //System.Diagnostics.Debug.WriteLine("SEND FINGERS: " + string.Join("-", states.Select(b => b.ToString()).ToArray()));
		}

        public void InterpolateToState(int DurationMs, int[] AbsSteps, float[] FingerAnglesDeg)
		{
			var frame1 = new SerialFrame((byte)'M');

			frame1.AddArduinoInt((short)DurationMs);
			frame1.AddArduinoInt((short)AbsSteps[0]);
			frame1.AddArduinoInt((short)AbsSteps[1]);

			SendFrame(frame1);

			var frame2 = new SerialFrame((byte)'R');

			frame2.AddArduinoInt((short)DurationMs);

			for (int i = 0; i < 10; i++)
			{
				short angle = (short)(FingerAnglesDeg[i] * 10.0);
				frame2.AddArduinoInt(angle);
			}
           
			SendFrame(frame2);

            //System.Diagnostics.Debug.WriteLine(String.Format("SEND KEYFRAME: Duration({0})", (short)DurationMs));
            //System.Diagnostics.Debug.WriteLine(String.Format("LEFT HAND: {0}", (short)AbsSteps[0]));
            //System.Diagnostics.Debug.WriteLine(String.Format("RIGHT HAND: {0}", (short)AbsSteps[1]));
			//System.Diagnostics.Debug.WriteLine(String.Format("FINGERS: {0}", string.Join(" ", FingerAnglesDeg.Select(b => (short)(b * 10.0) )) ));
		}
    }

    public class SerialFrame : IDisposable
    {
        private MemoryStream memory;
        private BinaryWriter bw;

        public SerialFrame(byte cmdCode)
        {
            memory = new MemoryStream();
            bw = new BinaryWriter(memory);

            memory.WriteByte(cmdCode);
        }

        public void AddBytes(byte b)
        {
            memory.WriteByte(b);
        }

        public void AddBytes(byte[] b)
        {
            memory.Write(b, 0, b.Length);
        }

        public void AddText(string text)
        {
            using (StreamWriter writer = new StreamWriter(memory))
                writer.Write(text);
        }

        public void AddBool(bool bo)
        {
            memory.WriteByte((byte)(bo ? 0xFF : 0x00));
        }

        public void AddArduinoInt(short number)
        {
            bw.Write((Int16)number);
        }
        

        public byte[] GetData()
        {
            return memory.ToArray();
        }

        public void Dispose()
        {
            bw.Dispose();
            memory.Dispose();
        }
    }

}