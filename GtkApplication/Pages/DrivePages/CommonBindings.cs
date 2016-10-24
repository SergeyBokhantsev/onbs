using System;
using Interfaces.UI;
using GtkApplication.Pages;
using Gtk;
using Interfaces;

namespace GtkApplication
{
	internal class CommonBindings
	{
		public const string m_BG_RED = "background='#FF0000'";
		public const string m_BG_GRAY = "background='#666666'";
		public const string m_BG_EMPTY = "";

		public const string m_FG_TOP_STATUS_TEXT = "foreground='#205768'";
		public const string m_FG_WHITE = "foreground='#FFFFFF'";
		public const string m_FG_GRAY = "foreground='#d6d6d6'";
		public const string m_FG_GRAY_DARK = "foreground='#aaaaaa'";

		public const string m_FG_YELLOW = "foreground='#FFFF00'";
		public const string m_FG_RED = "foreground='#FF0000'";
		public const string m_FG_BLUE = "foreground='#0000FF'";

		private const string m_ARD = "<span {0} {1} size='12000'>  ARD  </span>";
		private const string m_GPS = "<span {0} {1} size='12000'>  GPS  </span>";
		private const string m_INET = "<span {0} {1} size='12000'>  INET  </span>";
		private const string m_DIM = "<span {0} {1} size='12000'>  Dark  </span>";
		private const string m_WARN = "<span {0} {1} size='12000'>  WARN  </span>";

		public const string m_WND_STATUS = "<span {0} {1} size='12000'>{2}</span>";

		private const string m_TIME = "<span {0} {1} size='18000'>{2}</span><span {0} {1} size='10000'> : {3}</span>";

        private const string m_TASKBAR_BUTTON = "<span {0} size='8000'>{1}</span><span foreground='#606060' size='15000'>{2}</span>";

		public static void CreateStatusbar(ModelBinder binder, HBox bar, Style style)
		{
			bar.Homogeneous = true;

			// WINDOW STATUS LABEL
			var wnd_status_label = new Label();
            wnd_status_label.UseMarkup = true;
            wnd_status_label.Markup = CreateMarkup(m_WND_STATUS, m_BG_EMPTY, m_FG_GRAY, string.Empty);
			binder.BindLabelMarkup (wnd_status_label, "wnd_status", o => CreateMarkup (m_WND_STATUS, m_BG_EMPTY, m_FG_GRAY, o));
			bar.Add (wnd_status_label);

			// SYSTEM STATUS LEDS
			var system_status_box = new HBox();
			system_status_box.Homogeneous = true;
			bar.Add (system_status_box);

			// ARDUINO LED
			var arduino_status_label = new Label();
            arduino_status_label.UseMarkup = true;
            arduino_status_label.Markup = CreateMarkup(m_ARD, m_FG_TOP_STATUS_TEXT, m_BG_RED);
			binder.BindCustomAction<bool> (status => arduino_status_label.Markup = CreateMarkup (m_ARD, m_FG_TOP_STATUS_TEXT, status ? m_BG_EMPTY : m_BG_RED), "ard_status");
			system_status_box.Add (arduino_status_label);

			// GPS LED
			var gps_status_label = new Label();
            gps_status_label.UseMarkup = true;
            gps_status_label.Markup = CreateMarkup(m_GPS, m_FG_TOP_STATUS_TEXT, m_BG_RED);
			binder.BindCustomAction<bool> (status => gps_status_label.Markup = CreateMarkup (m_GPS, m_FG_TOP_STATUS_TEXT, status ? m_BG_EMPTY : m_BG_RED), "gps_status");
			system_status_box.Add (gps_status_label);

			// INET LED
			var inet_status_label = new Label();
            inet_status_label.UseMarkup = true;
            inet_status_label.Markup = CreateMarkup(m_INET, m_FG_TOP_STATUS_TEXT, m_BG_RED);
			binder.BindCustomAction<bool> (status => inet_status_label.Markup = CreateMarkup (m_INET, m_FG_TOP_STATUS_TEXT, status ? m_BG_EMPTY : m_BG_RED), "inet_status");
			system_status_box.Add (inet_status_label);

			// DIM CONDITION LED
			var dim_condition_label = new Label();
			dim_condition_label.UseMarkup = true;
			dim_condition_label.Markup = CreateMarkup(m_DIM, m_FG_TOP_STATUS_TEXT, m_BG_EMPTY);
			binder.BindCustomAction<bool>(status => dim_condition_label.Markup = CreateMarkup(m_DIM, m_FG_TOP_STATUS_TEXT, status ? m_BG_EMPTY : m_BG_GRAY), "dim_light");
			system_status_box.Add(dim_condition_label);

			// WARNING LED
			var warning_label = new Label();
			warning_label.UseMarkup = true;
			warning_label.Markup = CreateMarkup(m_WARN, m_FG_TOP_STATUS_TEXT, m_BG_RED);
			binder.BindCustomAction<bool>(status => warning_label.Markup = CreateMarkup(m_WARN, m_FG_TOP_STATUS_TEXT, status ? m_BG_EMPTY : m_BG_RED), "warning_log");
			system_status_box.Add(warning_label);


			// TIME LABEL
			var time_label = new Label();
            time_label.Xalign = 1;
            time_label.UseMarkup = true;
            time_label.Markup = CreateMarkup(m_TIME, m_FG_GRAY, m_BG_EMPTY, DateTime.Now.ToString("HH:mm"), DateTime.Now.ToString("ss"));
			binder.BindCustomAction<DateTime> (time => time_label.Markup = CreateMarkup (m_TIME, m_FG_GRAY, m_BG_EMPTY, time.ToString ("HH:mm"), time.ToString ("ss")), "time");
            bar.Add(time_label);
		}

		public static void CreateTaskbarButtons(ModelBinder binder, HBox taskbar, Style style)
		{
            var buttonLabelPropertyNames = new Tuple<string, string, string>[] 
			{ 
                new Tuple<string, string, string>(ModelNames.ButtonF1, "1", m_FG_GRAY_DARK),
				new Tuple<string, string, string>(ModelNames.ButtonF2, "2", m_FG_GRAY_DARK),
				new Tuple<string, string, string>(ModelNames.ButtonF3, "3", m_FG_GRAY_DARK),
                new Tuple<string, string, string>(ModelNames.ButtonF4, "4", m_FG_GRAY_DARK),
                new Tuple<string, string, string>(ModelNames.ButtonF5, "5", m_FG_GRAY_DARK),
			    new Tuple<string, string, string>(ModelNames.ButtonF6, "6", m_FG_GRAY_DARK),
			    new Tuple<string, string, string>(ModelNames.ButtonF7, "7", m_FG_GRAY_DARK),
			    new Tuple<string, string, string>(ModelNames.ButtonF8, "8", m_FG_GRAY_DARK),
			    new Tuple<string, string, string>(ModelNames.ButtonAccept, "A", "foreground='#20dd20'"),
			    new Tuple<string, string, string>(ModelNames.ButtonCancel, "C", "foreground='#ff2020'")
            };

			foreach (var prop in buttonLabelPropertyNames) 
			{
				var labelValue = string.Concat (binder.Model.GetProperty<object> (ModelNames.ResolveButtonLabelName(prop.Item1)));

				if (!string.IsNullOrEmpty (labelValue)) 
				{
					var eventBox = new EventBox ();
					taskbar.Add (eventBox);

                    binder.BindEventBoxClick(eventBox, prop.Item1);

					var boxChild = (Gtk.Box.BoxChild)taskbar[eventBox];
					boxChild.Expand = true;
					boxChild.Fill = true;
                   
					var button = new Label ();
					eventBox.Add (button);

					style.Window.Apply (eventBox);

                    button.Yalign = 0.2f;

					button.UseMarkup = true;
					button.Markup = CreateMarkup (m_TASKBAR_BUTTON, prop.Item3, string.Concat("[", prop.Item2, "] "), labelValue);
				}
			}
		}

		public CommonBindings (ModelBinder binder, Style style, ILogger logger, 
			EventBox eventbox_drive,
			EventBox eventbox_nav,
			EventBox eventbox_cam,
			EventBox eventbox_weather,
			EventBox eventbox_traffic,
			EventBox eventbox_options,
			Label label_arduino_status,
			Label label_gps_status,
			Label label_inet_status,
			Label label_time)
		{
			style.Window.Apply(eventbox_drive);
			style.Window.Apply(eventbox_nav);
			style.Window.Apply(eventbox_cam);
			style.Window.Apply(eventbox_weather);
			style.Window.Apply(eventbox_traffic);
			style.Window.Apply(eventbox_options);

			binder.BindCustomAction<bool>(ardStatus => 
				label_arduino_status.Markup = CreateMarkup(m_ARD, m_FG_TOP_STATUS_TEXT, ardStatus ? m_BG_EMPTY : m_BG_RED)
				, "ard_status");

			binder.BindCustomAction<bool>(gpsStatus => 
				label_gps_status.LabelProp = CreateMarkup(m_GPS, m_FG_TOP_STATUS_TEXT, gpsStatus ? m_BG_EMPTY : m_BG_RED)
				, "gps_status");

			binder.BindCustomAction<bool>(inetStatus => 
				label_inet_status.Markup = CreateMarkup(m_INET, m_FG_TOP_STATUS_TEXT, inetStatus ? m_BG_EMPTY : m_BG_RED)
				, "inet_status");

			binder.BindCustomAction<DateTime>(time => 
				label_time.Markup = CreateMarkup(m_TIME, m_FG_GRAY, m_BG_EMPTY, time.ToString("HH:mm"), time.ToString("ss"))
				, "time");
			
			binder.BindEventBoxClick(eventbox_drive, ModelNames.ButtonF1);
			binder.BindEventBoxClick(eventbox_nav, ModelNames.ButtonF2);
			binder.BindEventBoxClick(eventbox_cam, ModelNames.ButtonF3);
			binder.BindEventBoxClick(eventbox_weather, ModelNames.ButtonF4);
			binder.BindEventBoxClick(eventbox_traffic, ModelNames.ButtonF5);
			binder.BindEventBoxClick(eventbox_options, ModelNames.ButtonCancel);
		}

		public static string CreateMarkup(string template, params object[] args)
		{
			return string.Format(template, args);
		}
	}
}

