using System;
using Interfaces.UI;

namespace GtkLauncher
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			try
			{
				var app = new GtkApplication.App (new ConsoleLogger ());

				app.ShowPage(GetExternalAppPage());

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

			page.SetProperty("nav_btn_caption", "Test string");

			return page;
		}

		private static IPageModel GetExternalAppPage()
		{
			var page = new EmptyPageModel ("ExternalApplicationPage");

			page.SetProperty ("is_error", "0");

			return page;
		}
	}
}
