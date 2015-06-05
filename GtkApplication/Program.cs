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

		private readonly LookAndFeel style;

        public App(ILogger logger)
        {
            this.logger = logger;

			style = new LookAndFeel ();
			style.Bg = new Gdk.Color (0,30,50);
			style.HoverColor = new Gdk.Color (100, 0, 0);
			style.Fg = new Gdk.Color (255, 255, 255);
			style.ClickColor = new Gdk.Color (255, 0, 0);
        }

		public void Run()
		{
            try
            {
                Application.Init();
                win = new MainWindow(logger);

				win.ModifyBg(StateType.Normal, style.Bg);

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

            if (win.Child != null)
                win.Remove(win.Child);

            switch (showPageArgs.Model.Name)
            {
                case "MainPage":
				win.Add(new MainPage(showPageArgs.Model, style, logger));
                    break;

				case "ExternalApplicationPage":
				win.Add(new ExternalApplicationPage (showPageArgs.Model, style, logger));
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
