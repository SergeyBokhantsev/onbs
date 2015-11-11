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

		public void Run(bool fullscreen)
		{
            try
            {
                GLib.ExceptionManager.UnhandledException += ProcessUnhandledException;

                Application.Init();
                win = new MainWindow(logger);

				win.ModifyBg(StateType.Normal, style.Window.Bg);
                
                win.Show();

				if (fullscreen)
					win.Fullscreen();
				
                Application.Run();
            }
            catch (Exception ex)
            {
                logger.Log(this, "Exception in UI Host", LogLevels.Fatal);
                logger.Log(this, ex);
            }
		}

        void ProcessUnhandledException(GLib.UnhandledExceptionArgs args)
        {
            logger.Log(this, args.ExceptionObject as Exception);
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
				var pageType = Type.GetType(string.Concat("GtkApplication.", model.ViewName));
				var constructor = pageType.GetConstructor(new Type[] { typeof(IPageModel), typeof(Style), typeof(ILogger) });
				var page = constructor.Invoke(new object[] { model, style, logger }) as Gtk.Bin;

				if (page == null)
					throw new Exception("No page was constructed");
			
				return page ;
			} catch (Exception ex)
			{
				var errorPageModel = new ErrorPageModel (ex);
				return new ErrorPage (errorPageModel, style, logger);
			}
		}

        public void ShowPage(IPageModel model)
        {
            Application.Invoke(this, new ShowPageEventArgs(model), ShowPage);
        }

        public void ShowDialog(IDialogModel model)
        {
            Application.Invoke((s, e) => ShowDialog(this, new ShowPageEventArgs(model)));
        }

        private void ShowDialog(object sender, EventArgs args)
        {
            var model = (args as ShowPageEventArgs).Model as IDialogModel;

            Func<string> getWindowCaption = () => string.Format("({0}) {1}", model.GetProperty<object>(model.RemainingTimePropertyName) ?? "*", model.GetProperty<object>(model.CaptionPropertyName));

            var dlg = new Gtk.MessageDialog(win, DialogFlags.DestroyWithParent, MessageType.Warning, ButtonsType.None, model.GetProperty<object>(model.MessagePropertyName).ToString());

            if (model.Buttons != null)
            {
                foreach (var btn in model.Buttons)
                {
                    dlg.AddButton(btn.Value, (ResponseType)btn.Key);
                }
            }

            model.PropertyChanged += name => 
            {
                if (name == model.RemainingTimePropertyName)
                    Application.Invoke((s, e) => dlg.Title = getWindowCaption());
            };

            model.ButtonClick += dr => Application.Invoke((s, e) => dlg.Respond((ResponseType)dr));

            dlg.Response += (s, e) => { model.OnClosed((DialogResults)e.ResponseId); dlg.Destroy(); };
            dlg.Modal = true;
            dlg.Shown += (s, e) => model.OnShown();
            dlg.Close += (s, e) => model.OnClosed(DialogResults.None);

            var box = new HBox();
            box.Add(new Label() { Text = model.GetProperty<object>(model.MessagePropertyName).ToString() });

            dlg.CreatePangoContext();
            dlg.CreatePangoLayout("sdfsdf");
            

            dlg.Run();
            dlg.Destroy();
        }

        public void Shutdown()
        {
            Application.Quit();
        }
    }
}
