using System;
using Interfaces.UI;
using Interfaces;
using System.Diagnostics;
using GtkApplication.Pages;
using Interfaces.GPS;

namespace GtkApplication
{
	[System.ComponentModel.ToolboxItem(true)]
    public partial class DrivePage : Gtk.Bin
	{
		private const string m_BG_RED = "background='#FF0000'";
		private const string m_BG_EMPTY = "";

		private const string m_FG_TOP_STATUS_TEXT = "foreground='#205768'";
		private const string m_FG_WHITE = "foreground='#FFFFFF'";
		private const string m_FG_GRAY = "foreground='#d6d6d6'";
		private const string m_FG_GRAY_DARK = "foreground='#aaaaaa'";

		private const string m_ARD = "<span {0} {1} size='12000'>  ARD  </span>";
		private const string m_GPS = "<span {0} {1} size='12000'>  GPS  </span>";
		private const string m_INET = "<span {0} {1} size='12000'>  INET  </span>";

		private const string m_SPEED = "<span {0} {1} size='90000'><b>{2}</b></span>";
		private const string m_TIME = "<span {0} {1} size='18000'>{2}</span><span {0} {1} size='10000'> : {3}</span>";
		private const string m_TRAVEL_SPAN = "<span {0} {1} size='20000'>{2} min</span>";
		private const string m_DISTANCE = "<span {0} {1} size='20000'>{2} km</span>";

		private const string m_LAT = "<span {0} {1} size='14000'>Lat: {2}</span>";
		private const string m_LON = "<span {0} {1} size='14000'>Lon: {2}</span>";

		private const string m_EXPORTED_POINTS = "<span {0} {1} size='14000'>Track: </span><span {0} {1} size='20000'>{2}</span>";
		private const string m_HEADING = "<span {0} {1} size='14000'>Heading: </span><span {0} {1} size='20000'>{2}</span>";
		private const string m_AIR_TEMP = "<span {0} {1} size='14000'>Air temp: </span><span {0} {1} size='20000'>{2}</span>";

        private readonly IPageModel model;
        private readonly int threadId;

		public DrivePage (IPageModel model, Style style, ILogger logger)
		{
            threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;

            this.model = model;

			this.Build();

			var binder = new ModelBinder (model, logger);

            binder.BindCustomAction<bool>(ardStatus => 
				label_arduino_status.Markup = CreateMarkup(m_ARD, m_FG_TOP_STATUS_TEXT, ardStatus ? m_BG_EMPTY : m_BG_RED)
                , "ard_status");

            binder.BindCustomAction<bool>(gpsStatus => 
                label_gps_status.LabelProp = CreateMarkup(m_GPS, m_FG_TOP_STATUS_TEXT, gpsStatus ? m_BG_EMPTY : m_BG_RED)
                , "gps_status");

            binder.BindCustomAction<bool>(inetStatus => 
				label_inet_status.Markup = CreateMarkup(m_INET, m_FG_TOP_STATUS_TEXT, inetStatus ? m_BG_EMPTY : m_BG_RED)
                , "inet_status");

            binder.BindCustomAction<double>(speed =>
				label_speed.Markup = CreateMarkup(m_SPEED, m_FG_WHITE, m_BG_EMPTY, speed.ToString("0"))
                , "speed");

            binder.BindCustomAction<DateTime>(time => 
				label_time.Markup = CreateMarkup(m_TIME, m_FG_GRAY, m_BG_EMPTY, time.ToString("HH:mm"), time.ToString("ss"))
                , "time");
			
            binder.BindCustomAction<double>(travel_span => 
				label_travel_span.Markup = CreateMarkup(m_TRAVEL_SPAN, m_FG_GRAY, m_BG_EMPTY, travel_span.ToString("0"))
                , "travel_span");

            binder.BindCustomAction<double>(distance => 
				label_distance.Markup = CreateMarkup(m_DISTANCE, m_FG_GRAY_DARK, m_BG_EMPTY, (distance / 1000).ToString("0"))
                , "distance");

			binder.BindCustomAction<GeoPoint>(location => 
			{
				label_lat.Markup = CreateMarkup(m_LAT, m_FG_GRAY, m_BG_EMPTY, location.Lat.Degrees.ToString("0.00000"));
				label_lon.Markup = CreateMarkup(m_LON, m_FG_GRAY, m_BG_EMPTY, location.Lon.Degrees.ToString("0.00000"));
			}
				, "location");

			binder.BindCustomAction<string>(exported_points =>
				label_exported_points.Markup = CreateMarkup(m_EXPORTED_POINTS, m_FG_GRAY, m_BG_EMPTY, exported_points)
				, "exported_points");

			binder.BindCustomAction<string>(heading =>
				label_heading.Markup = CreateMarkup(m_HEADING, m_FG_GRAY, m_BG_EMPTY, heading)
				, "heading");

			binder.BindCustomAction<double>(air_temp =>
				label_air_temp.Markup = CreateMarkup(m_AIR_TEMP, m_FG_WHITE, m_BG_EMPTY, air_temp.ToString("0"))
				, "air_temp");

			model.RefreshAllProps();
		}

        private string CreateMarkup(string template, params string[] args)
        {
            return string.Format(template, args);
        }
    }
}

