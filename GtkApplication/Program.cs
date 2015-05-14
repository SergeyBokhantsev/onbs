using System;
using Gtk;
using Interfaces;
using Interfaces.UI;
using System.Diagnostics.Contracts;

namespace GtkApplication
{
	public class App : IUIHost
	{
        private class ShowPageEventArgs : EventArgs
        {
            public IPageModel Model { get; private set; }

            public ShowPageEventArgs(IPageModel model)
            {
                Model = model;
            }
        }

        private ILogger logger;
        private MainWindow win;

        public App(ILogger logger)
        {
            this.logger = logger;
        }

		public void Run()
		{
			Application.Init();
			win = new MainWindow(logger);
			win.Show();
			Application.Run();
		}

        private void ShowPage(object sender, EventArgs args)
        {
            var showPageArgs = args as ShowPageEventArgs;

            if (showPageArgs == null || showPageArgs.Model == null)
                throw new ArgumentException("model");

            switch (showPageArgs.Model.Name)
            {
                case "MainPage":
                    //win.ch
                    break;

                default:
                    throw new NotImplementedException(showPageArgs.Model.Name);
            }
        }

        public void ShowPage(IPageModel model)
        {
            Application.Invoke(null, new ShowPageEventArgs(model), ShowPage);
        }
    }
}
