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
			var app = new GtkApplication.App (new ConsoleLogger ());

			app.ShowPage(GetGridPageModel());

			app.Run(false);

			Console.ReadKey();
		}

		private static IPageModel GetGridPageModel()
		{
			var page = new EmptyPageModel("GridPage");

			var dataModel = new Interfaces.UI.Models.TextGrigDataModel("A", "B", "C");

			dataModel.AddRow("1a", "1b", "1c");
			dataModel.AddRow("2a", "2b", "2c", "2d");
			dataModel.AddRow("3a", "3b");
			dataModel.AddRow("4a");

			page.SetProperty("grid", dataModel);

			int i = 0;

			var timer = new Timer(new TimerCallback(o =>
			{

				dataModel.Set(0, 1, DateTime.Now.ToString());

				i++;

				dataModel.RemoveRow(2);

				dataModel.InsertRow(1, i.ToString(), i.ToString(), i.ToString());

				dataModel.UpdateRow(3, DateTime.Now.ToString(), DateTime.Now.ToString());


			}), null, 500, 50);

			return page;
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
			var page = new EmptyPageModel("ExternalApplicationPage");

			page.SetProperty("is_error", "1");
			page.SetProperty("button_exit_label", "Exit");
			page.SetProperty("label_launch_info", "var app = new GtkApplication.App (new ConsoleLogger ());var app = new GtkApplication.App (new ConsoleLogger ());var app = new GtkApplication.App (new ConsoleLogger ());var app = new GtkApplication.App (new ConsoleLogger ());var app = new GtkApplication.App (new ConsoleLogger ());var app = new GtkApplication.App (new ConsoleLogger ());");

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
				page.SetProperty("dim_light", !page.GetProperty<bool>("dim_light"));
				page.SetProperty("warning_log", !page.GetProperty<bool>("warning_log"));

				page.SetProperty("speed", speed++);
				page.SetProperty("time", DateTime.Now);

				page.SetProperty("travel_span", speed);
				page.SetProperty("distance", speed * 1000);

				page.SetProperty("location", new GeoPoint(50.56897 + speed/1000, 30.76539 + speed/1000));

				page.SetProperty("exported_points", string.Concat(speed, "/", speed));
				page.SetProperty("heading", "Украина, Киев, Тростянецкая улица, " + speed.ToString());
				page.SetProperty("air_temp", "Ясно, переменная облачность, дождь, гроза");

					page.SetProperty("weather_icon", @"C:\Users\serg\Documents\GitHub\onbs4\HostController\Data\weather\bkn_sn_d.png");
					page.SetProperty("oil_temp_icon", @"C:\Users\serg\Documents\GitHub\onbs4\HostController\Data\icons\oiltemp.png");

					page.SetProperty("cpu_info", "800 Mhz / 30 C");
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

		private static IPageModel GetWeatherPage()
		{
			var page = new EmptyPageModel ("WeatherPage");

			double counter = 0;

			var timer = new Timer (new TimerCallback (o =>
				{
					page.SetProperty("time", DateTime.Now);

					page.SetProperty("ard_status", !page.GetProperty<bool>("ard_status"));
					page.SetProperty("gps_status", !page.GetProperty<bool>("gps_status"));
					page.SetProperty("inet_status", !page.GetProperty<bool>("inet_status"));

					page.SetProperty("time", DateTime.Now);

				}), 
				null, 500, 200);

			page.SetProperty("_timer", timer);

			page.SetProperty ("now_icon_path", @"D:\onbs\HostController\Data\weather\bkn_sn_d.png");
			page.SetProperty ("now_condition", "облачно, большой дождь");
			page.SetProperty ("now_temp", "15");

			page.SetProperty ("next1_caption", "Вечером");
			page.SetProperty ("next1_icon_path", @"D:\onbs\HostController\Data\weather\bkn_sn_d.png");
			page.SetProperty ("next1_condition", "облачно");
			page.SetProperty ("next1_temp", "25");

			page.SetProperty ("next2_caption", "Ночью");
			page.SetProperty ("next2_icon_path", @"D:\onbs\HostController\Data\weather\bkn_sn_d.png");
			page.SetProperty ("next2_condition", "облачно, возможен небольшой дождь");
			page.SetProperty ("next2_temp", "35");

			page.SetProperty ("info1", "Восход: 05:05Закат: 21:02");
			page.SetProperty ("info2", "Ветер: 4,0 м/с З");
			page.SetProperty ("info3", "Влажность: 42%");
			page.SetProperty ("info4", "Давление: 746 мм рт. ст.");
			page.SetProperty ("info5", "Данные на 18:30");

			page.SetProperty("day1_caption", "Завтра");
			page.SetProperty("day1_date", "12.13.1555");
			page.SetProperty("day1_image_path", @"D:\onbs\HostController\Data\weather\bkn_sn_d.png");
			page.SetProperty("day1_day_temp", "35");
			page.SetProperty("day1_night_temp", "12");

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

		private static IPageModel GetTrafficPage()
		{
			var page = new EmptyPageModel ("TrafficPage");

			var timer = new Timer (new TimerCallback (o =>
				{
					page.SetProperty("time", DateTime.Now);

					page.SetProperty("ard_status", !page.GetProperty<bool>("ard_status"));
					page.SetProperty("gps_status", !page.GetProperty<bool>("gps_status"));
					page.SetProperty("inet_status", !page.GetProperty<bool>("inet_status"));
				}), 
				null, 500, 200);

			page.SetProperty("_timer", timer);

			page.SetProperty("traffic_image_path", @"C:\Users\sergeyb\Desktop\static-maps.yandex.ru.png");

			return page;
		}

		private static IPageModel GetOBD_DTCPage()
		{
			var page = new EmptyPageModel ("OBD_DTCPage");

			page.SetProperty("codes", string.Join("\r\n", new string[] 
				{ "P0480: Неисправность цепи управления реле вентилятора № 1", 
					"P1612: Ошибка сброса контроллера",
					"P0105: ",
					"P1530: ",
					"P0304: Пропуски воспламенения в цилиндре 4",
					"P0116: Выход сигнала датчика температуры охлаждающей жидкости за допустимый диапазон"
				}));

			page.SetProperty (ModelNames.ButtonAcceptLabel, "Refresh");
			page.SetProperty(ModelNames.ButtonF1Label, "Clear");
			page.SetProperty (ModelNames.ButtonCancelLabel, "Back");

			return page;
		}

		private static IPageModel GetOBDEngineAndFuelPage()
		{
			var page = new EmptyPageModel ("OBDEngineAndFuel");

			//page.SetProperty ("flow", 19.248);
			//page.SetProperty ("prm", 1240d);

			Random r = new Random ();
			var flow = 0d;
			var oboroty = 0d;

			var prmIncrement = 25;
			var prmIncrementTimes = 0;

			ChartOfDouble rpm = new ChartOfDouble { Title = "RPM", Scale = 5000 };
			ChartOfDouble load = new ChartOfDouble { Title = "Load", UnitText = "%", Scale = 100 };
			ChartOfDouble speed = new ChartOfDouble { Title = "Speed", UnitText = "km/h", Scale = 100 };
			ChartOfDouble coolant = new ChartOfDouble { Title = "C-temp", UnitText = "°", Scale = 100 };
			ChartOfDouble throttle = new ChartOfDouble { Title = "Thr.", UnitText = "%", Scale = 100 };
			ChartOfDouble s3 = new ChartOfDouble { Title = "s3.", UnitText = "%", Scale = 100 };

			ChartOfDouble s4 = new ChartOfDouble { Title = "s4.", UnitText = "%", Scale = 100 };
			ChartOfDouble s5 = new ChartOfDouble { Title = "s5.", UnitText = "%", Scale = 100 };
			ChartOfDouble s6 = new ChartOfDouble { Title = "s6.", UnitText = "%", Scale = 100 };

			page.SetProperty("primary1", rpm);
			page.SetProperty("primary2", load);
			page.SetProperty("primary3", speed);
			page.SetProperty("secondary1", coolant);
			page.SetProperty("secondary2", throttle);
			page.SetProperty("secondary3", s3);
			page.SetProperty("secondary4", s4);
			page.SetProperty("secondary5", s5);
			page.SetProperty("secondary6", s6);

			s3.Add(30d);

			var timer = new Timer (new TimerCallback (o => {
				page.SetProperty ("time", DateTime.Now);


				oboroty += prmIncrement;

				if (++prmIncrementTimes == 10)
				{
					prmIncrement *= -1;
					prmIncrementTimes = 0;
				}
					
				flow += 2.55;

				rpm.Add(oboroty);
				load.Add(flow);
				speed.Add(r.NextDouble() * 50);
				coolant.Add(r.NextDouble() * 100);
				throttle.Add(r.NextDouble());
				s3.DuplicateLast();

				s4.Add(r.NextDouble());
				s5.Add(r.NextDouble());
				s6.Add(r.NextDouble());

				page.SetProperty("refresh", null);
			}), 
				            null, 500, 100);
			
			page.SetProperty ("_timer", timer);

			return page;
		}

		private static IPageModel GetMultilineView()
		{
			var page = new EmptyPageModel ("MultilineView");

			var logList = new System.Collections.Concurrent.ConcurrentQueue<string> ();

			page.SetProperty (ModelNames.PageTitle, "shutdown...");

			logList.Enqueue("Starting shutdown...");

			page.SetProperty ("lines_queue", logList);

			var timer = new Timer (new TimerCallback (o => 
				{
					logList.Enqueue(Guid.NewGuid().ToString());
					page.RaisePropertyChangedEvent("lines_queue");
				}
			), null, 200, 200);

			page.SetProperty ("timer", timer);

			return page;
		}

		private static IPageModel GetFileOptionsPage()
		{
			var page = new EmptyPageModel ("FileOptionsPage");

			page.SetProperty ("file_name", "Myvideo1");
			page.SetProperty ("file_props", "Created: 1111, Size: 333");

			page.SetProperty (ModelNames.ButtonF1Label, "Quick preview");
			page.SetProperty (ModelNames.ButtonF2Label, "Protect deletion");
			page.SetProperty (ModelNames.ButtonF3Label, "View normally");
			page.SetProperty (ModelNames.ButtonF4Label, "Copy to external storage");
			page.SetProperty (ModelNames.ButtonF5Label, "Remove");
			page.SetProperty (ModelNames.ButtonCancelLabel, "Return to files list");

			return page;
		}

		private static IPageModel GetLightSensorInfoModel()
		{
			var page = new EmptyPageModel ("LightSensorInfoPage");

			page.SetProperty ("sensor_a", "256");
			page.SetProperty ("sensor_b", "35");
			page.SetProperty ("message", "No message...");

			Random r = new Random ();

			var timer = new Timer (new TimerCallback (o => 
			{
					page.SetProperty ("sensor_a", r.Next(255));
					page.SetProperty ("sensor_b", r.Next(255));
				}), null, 500, 100);

			page.SetProperty ("_timer", timer);

			return page;
		}
	}
}
