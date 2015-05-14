using System;
using Gtk;
using Interfaces;

namespace GtkApplication
{
	public class App : IUIHost
	{
        public App(IHostController controller)
        {

        }

		public void Run()
		{
			Application.Init ();
			MainWindow win = new MainWindow ();
			win.Show ();
			Application.Run();
		}
	}
}
