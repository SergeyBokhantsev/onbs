using System;
using Gtk;
using Interfaces;
using Interfaces.UI;
using System.Diagnostics.Contracts;
using System.Reflection;

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
                AppDomain.CurrentDomain.UnhandledException += (s, e) => logger.Log(s, e.ExceptionObject as Exception);

                Application.Init();
                win = new MainWindow(logger);

				win.ModifyBg(StateType.Normal, style.Window.Bg);
                
                win.Show();
                win.Fullscreen();
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
			
			win.Add(CreatePage(showPageArgs.Model));

            win.ShowAll();
        }

		private Gtk.Bin CreatePage(IPageModel model)
		{
			try
			{
				var pageType = Type.GetType(string.Concat("GtkApplication.", model.Name));
				var constructor = pageType.GetConstructor(new Type[] { typeof(IPageModel), typeof(Style), typeof(ILogger) });
				var page = constructor.Invoke(new object[] { model, style, logger }) as Gtk.Bin;

				if (page == null)
					throw new Exception();
			
				return page ;
			} catch (Exception ex)
			{
				//TODO: implement error page
				throw;
			}
		}

        public void ShowPage(IPageModel model)
        {
            Application.Invoke(null, new ShowPageEventArgs(model), ShowPage);
        }

        public void Shutdown()
        {
            Application.Quit();
        }
    }
}
