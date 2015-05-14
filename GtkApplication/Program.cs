using System;
using Gtk;
using Interfaces;
using Interfaces.UI;
using System.Diagnostics.Contracts;

namespace GtkApplication
{
	public class App : IUIHost
	{
        private IHostController controller;
        private MainWindow win;

        public App(IHostController controller)
        {
            this.controller = controller;
            Run();
        }

		private void Run()
		{
			Application.Init ();
			win = new MainWindow(controller);
			win.Show();
			Application.Run();
		}

        public void ShowPage(IPageModel model)
        {
            Contract.Requires(model != null);

            switch (model.Name)
            {
                case "MainPage":
                    win.ch
                    break;

                default:
                    throw new NotImplementedException(model.Name);
            }
        }
    }
}
