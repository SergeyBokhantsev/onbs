using System;
using Interfaces.UI;
using Interfaces;
using System.Threading;
using Interfaces.GPS;

namespace GtkLauncher
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			try
			{
				var app = new GtkApplication.App (new ConsoleLogger ());

				app.ShowPage(GetDrivePage());

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

			page.SetProperty(ModelNames.ButtonF1Label, "Navigation");
			page.SetProperty(ModelNames.ButtonF2Label, "Camera");


			page.SetProperty("time_valid", "1");
			var timer = new Timer (new TimerCallback (o => page.SetProperty("time", DateTime.Now)), null, 500, 1000);

			page.SetProperty ("label_inet_status", "CONNECTED");
			page.SetProperty ("inet_status", "0");

			page.SetProperty("_timer", timer);

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

		private static IPageModel GetSystemConfigurationPage()
		{
			bool gpsdEnabled = true;

			var page = new EmptyPageModel ("CommonVertcalStackPage");

			page.SetProperty ("label_caption", "Custom configuration");

			page.SetProperty(ModelNames.ButtonCancelLabel, "Return");
			page.SetProperty(ModelNames.ButtonAcceptLabel, "Next page");

			page.SetProperty(ModelNames.ButtonF1Label, "GPSD Enabled");

			page.OnAction += arg => {

				switch(arg.ActionName)
				{
					case "F1":
						if (arg.State == Interfaces.Input.ButtonStates.Press)
						{
							gpsdEnabled = !gpsdEnabled;
						page.SetProperty(ModelNames.ButtonF1Label, gpsdEnabled ? "GPSD Enabled" : "GPSD Disabled");
						}
						break;
				}

			};

			return page;
		}

		private static IPageModel GetCommonYesNoPage()
		{
			bool gpsdEnabled = true;

			var page = new EmptyPageModel ("CommonYesNoPage");

			page.SetProperty ("label_caption", "Custom yes/no dialog");
			page.SetProperty ("label_message", "Do you?");

			page.SetProperty(ModelNames.ButtonCancelLabel, "Surely YES");
			page.SetProperty(ModelNames.ButtonAcceptLabel, "Maybe later");

			return page;
		}

		private static IPageModel GetDrivePage()
		{
			var page = new EmptyPageModel ("DrivePage");
            
			double speed = 0;

			var timer = new Timer (new TimerCallback (o =>
			{
				page.SetProperty("time", DateTime.Now);

				page.SetProperty("ard_status", !page.GetProperty<bool>("ard_status"));
				page.SetProperty("gps_status", !page.GetProperty<bool>("gps_status"));
				page.SetProperty("inet_status", !page.GetProperty<bool>("inet_status"));

				page.SetProperty("speed", speed++);
				page.SetProperty("time", DateTime.Now);

				page.SetProperty("travel_span", speed);
				page.SetProperty("distance", speed * 1000);

				page.SetProperty("location", new GeoPoint(50.56897 + speed/1000, 30.76539 + speed/1000));

				page.SetProperty("exported_points", string.Concat(speed, "/", speed));
				page.SetProperty("heading", "West" + speed.ToString());
				page.SetProperty("air_temp", speed);
			}), 
				null, 500, 200);

			page.SetProperty("_timer", timer);

			page.OnAction += (PageModelActionEventArgs obj) =>
			{
				switch (obj.ActionName)
				{
					case ModelNames.ButtonF1:
						page.SetProperty("a", 1);
						break;
				}
			};

			return page;
		}
	}
}
