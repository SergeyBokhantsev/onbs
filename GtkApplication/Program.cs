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

		private readonly Style style;

        public App(ILogger logger)
        {
            this.logger = logger;

			style = GetStyle ();
        }

		private Style GetStyle()
		{
			var window = new LookAndFeel { 
				Bg = new Gdk.Color (0, 30, 50),
				HoverColor = new Gdk.Color (100, 0, 0),
				Fg = new Gdk.Color (255, 255, 255),
				ClickColor = new Gdk.Color (255, 0, 0)
			};

			var commonButton = new LookAndFeel { 
				Bg = new Gdk.Color (50, 50, 50),
				HoverColor = new Gdk.Color (30, 30, 30),
				Fg = new Gdk.Color (255, 255, 255),
				ClickColor = new Gdk.Color (100, 100, 100)
			};

			var acceptButton = new LookAndFeel { 
				Bg = new Gdk.Color (20, 80, 30),
				HoverColor = new Gdk.Color (30, 130, 100),
				Fg = new Gdk.Color (255, 255, 255),
				ClickColor = new Gdk.Color (60, 200, 120)
			};

			var cancelButton = new LookAndFeel { 
				Bg = new Gdk.Color (100, 0, 0),
				HoverColor = new Gdk.Color (130, 30, 0),
				Fg = new Gdk.Color (255, 255, 255),
				ClickColor = new Gdk.Color (255, 70, 70)
			};

			var textBox = new LookAndFeel { 
				Bg = new Gdk.Color (0, 30, 50),
				HoverColor = new Gdk.Color (0, 0, 0),
				Fg = new Gdk.Color (220, 220, 180),
				ClickColor = new Gdk.Color (255, 0, 0)
			};

			return new Style (window, commonButton, acceptButton, cancelButton, textBox);
		}

		public void Run()
		{
            try
            {
                Application.Init();
                win = new MainWindow(logger);

				win.ModifyBg(StateType.Normal, style.Window.Bg);

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
