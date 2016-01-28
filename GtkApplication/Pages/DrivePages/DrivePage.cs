using System;
using Interfaces.UI;
using Interfaces;
using System.Diagnostics;
using GtkApplication.Pages;
using Interfaces.GPS;
using CB = GtkApplication.CommonBindings;

namespace GtkApplication
{
	[System.ComponentModel.ToolboxItem(true)]
    public partial class DrivePage : Gtk.Bin
	{
		private const string m_SPEED = "<span {0} {1} size='90000'><b>{2}</b></span>";
		private const string m_TRAVEL_SPAN = "<span {0} {1} size='20000'>{2} min</span>";
		private const string m_DISTANCE = "<span {0} {1} size='20000'>{2} km</span>";

		private const string m_LAT = "<span {0} {1} size='14000'>Lat: {2}</span>";
		private const string m_LON = "<span {0} {1} size='14000'>Lon: {2}</span>";

		private const string m_CPUINFO = "<span {0} {1} size='10000'>CPU: {2}</span>";

		private const string m_EXPORTED_POINTS = "<span {0} {1} size='14000'>Track: {2}</span>";
		private const string m_HEADING = "<span {0} {1} size='20000'>{2}</span>";
		private const string m_AIR_TEMP = "<span {0} {1} size='14000'>{2}</span>";
		private const string m_ENG_TEMP = "<span {0} {1} size='14000'>{2}</span>";

		public DrivePage (IPageModel model, Style style, ILogger logger)
		{
			this.Build();

			var binder = new ModelBinder (model, logger);

			CommonBindings.CreateStatusbar (binder, hbox1, style);
			CommonBindings.CreateTaskbarButtons (binder, hbox5, style);

            binder.BindCustomAction<object>(speed =>
				label_speed.Markup = 
	CB.CreateMarkup(m_SPEED, CB.m_FG_WHITE, CB.m_BG_EMPTY, speed)
                , "speed");
			
            binder.BindCustomAction<double>(travel_span => 
				label_travel_span.Markup = CB.CreateMarkup(m_TRAVEL_SPAN, CB.m_FG_GRAY, CB.m_BG_EMPTY, travel_span.ToString("0"))
                , "travel_span");

            binder.BindCustomAction<double>(distance => 
				label_distance.Markup = CB.CreateMarkup(m_DISTANCE, CB.m_FG_GRAY_DARK, CB.m_BG_EMPTY, (distance / 1000).ToString("0"))
                , "distance");

			binder.BindCustomAction<GeoPoint>(location => 
			{
					label_lat.Markup = CB.CreateMarkup(m_LAT, CB.m_FG_GRAY, CB.m_BG_EMPTY, location.Lat.Degrees.ToString("0.00000"));
					label_lon.Markup = CB.CreateMarkup(m_LON, CB.m_FG_GRAY, CB.m_BG_EMPTY, location.Lon.Degrees.ToString("0.00000"));
			}
				, "location");

			binder.BindCustomAction<string>(exported_points =>
				label_exported_points.Markup = CB.CreateMarkup(m_EXPORTED_POINTS, CB.m_FG_GRAY, CB.m_BG_EMPTY, exported_points)
				, "exported_points");

			binder.BindCustomAction<string>(heading =>
				label_heading.Markup = CB.CreateMarkup(m_HEADING, CB.m_FG_GRAY, CB.m_BG_EMPTY, heading)
				, "heading");

			binder.BindCustomAction<string>(air_temp =>
				label_air_temp.Markup = CB.CreateMarkup(m_AIR_TEMP, CB.m_FG_WHITE, CB.m_BG_EMPTY, air_temp)
				, "air_temp");

			binder.BindCustomAction<int>(eng_temp =>
                label_eng_temp.Markup = CB.CreateMarkup(m_ENG_TEMP, CB.m_FG_GRAY_DARK, CB.m_BG_EMPTY, eng_temp > int.MinValue ? string.Concat(eng_temp, "° C") : "NA")
				, "eng_temp");

			binder.BindCustomAction<string>(info =>
				label_cpu_info.Markup = CB.CreateMarkup(m_CPUINFO, CB.m_FG_GRAY_DARK, CB.m_BG_EMPTY, info)
				, "cpu_info");

			binder.BindCustomAction<string>(icon_path => image_weather_icon.File = icon_path, "weather_icon");

			binder.BindCustomAction<string>(icon_path => image_oil_temp.File = icon_path, "oil_temp_icon");

			binder.UpdateBindings ();
		}
    }
}

