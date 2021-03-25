using System;
using Gtk;


namespace pianocam
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			Application.Init();
            LoadWin win = new LoadWin();
            win.Show();
			Application.Run();
		}
	}
}
