using System;
using Interfaces.UI;
using Interfaces;

namespace GtkLauncher
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			try
			{
				var app = new GtkApplication.App (new ConsoleLogger ());

				app.ShowPage(GetMainPage());

				app.Run();
			}
			catch (Exception ex)
			{
				
			}

			Console.ReadKey();
		}

		private static IPageModel GetMainPage()
		{
			var page = new EmptyPageModel ("MainPage");

			page.SetProperty("nav_btn_caption", "Navigation");
			page.SetProperty("cam_btn_caption", "Camera");

			return page;
		}

		private static IPageModel GetExternalAppPage()
		{
			var page = new EmptyPageModel ("ExternalApplicationPage");

			page.SetProperty ("is_error", "1");
			page.SetProperty ("button_exit_label", "Exit");
			page.SetProperty ("label_launch_info", "var app = new GtkApplication.App (new ConsoleLogger ());var app = new GtkApplication.App (new ConsoleLogger ());var app = new GtkApplication.App (new ConsoleLogger ());var app = new GtkApplication.App (new ConsoleLogger ());var app = new GtkApplication.App (new ConsoleLogger ());var app = new GtkApplication.App (new ConsoleLogger ());");

			return page;
		}
	}
}
