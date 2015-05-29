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
            try
            {
                Application.Init();
                win = new MainWindow(logger);
                win.Show();
                Application.Run();
            }
            catch (Exception ex)
            {
                logger.Log(this, "Exception in UI Host", LogLevels.Fatal);
                logger.Log(this, ex);
            }
		}

        private void ShowPage(object sender, EventArgs args)
        {
            var showPageArgs = args as ShowPageEventArgs;

            if (showPageArgs == null || showPageArgs.Model == null)
                throw new ArgumentException("model");

            win.Child = null;

            switch (showPageArgs.Model.Name)
            {
                case "MainPage":
                    win.Add(new MainPage(showPageArgs.Model));
                    break;

                default:
                    throw new NotImplementedException(showPageArgs.Model.Name);
            }

            win.ShowAll();
        }

        public void ShowPage(IPageModel model)
        {
            Application.Invoke(null, new ShowPageEventArgs(model), ShowPage);
        }
    }
}
