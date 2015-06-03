using System;

namespace GtkLauncher
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			try
			{
				var app = new GtkApplication.App (new ConsoleLogger ());
				var page = new EmptyPageModel ("MainPage");
				app.ShowPage(page);
				app.Run();
			}
			catch (Exception ex)
			{
				
			}

			Console.ReadKey();
		}
	}
}
