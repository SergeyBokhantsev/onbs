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

		private const string m_TIME = "<span {0} {1} size='18000'>{2}</span><span {0} {1} size='10000'> : {3}</span>";

		private const string m_TASKBAR_BUTTON = "<span foreground='#606060' size='15000'>{0}</span>";

		public static void CreateTaskbarButtons(ModelBinder ModelBinder, HBox taskbar, Style style)
		{
			var buttonLabelPropertyNames = new string[] 
			{ 
				ModelNames.ButtonF1Label, 
				ModelNames.ButtonF2Label,
				ModelNames.ButtonF3Label,
				ModelNames.ButtonF4Label,
				ModelNames.ButtonF5Label,
				ModelNames.ButtonF6Label,
				ModelNames.ButtonF7Label,
				ModelNames.ButtonF8Label,
				ModelNames.ButtonAcceptLabel,
				ModelNames.ButtonCancelLabel
			};

			foreach (var propName in buttonLabelPropertyNames) 
			{
				var labelValue = string.Concat (ModelBinder.Model.GetProperty<object> (propName));

				if (!string.IsNullOrEmpty (labelValue)) 
				{
					var eventBox = new EventBox ();

					eventBox.HeightRequest = 10;

					taskbar.Add (eventBox);

					var boxChild = (Gtk.Box.BoxChild)taskbar[eventBox];
					boxChild.Expand = true;
					boxChild.Fill = true;
                    

					var button = new Label ();

					button.HeightRequest = 10;

					eventBox.Add (button);

					style.Window.Apply (eventBox);

                    

					button.UseMarkup = true;
					button.Markup = CreateMarkup (m_TASKBAR_BUTTON, labelValue);
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

		public static string CreateMarkup(string template, params string[] args)
		{
			return string.Format(template, args);
		}
	}
}

