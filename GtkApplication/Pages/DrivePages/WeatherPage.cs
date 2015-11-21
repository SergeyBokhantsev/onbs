using System;
using Interfaces.UI;
using Interfaces;
using System.Diagnostics;
using GtkApplication.Pages;
using Interfaces.GPS;
using CB = GtkApplication.CommonBindings;
using Gdk;

namespace GtkApplication
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class WeatherPage : Gtk.Bin
	{
		private readonly CommonBindings commonBindings;

		private const string m_WEATHER_CONDITION_LARGE = "<span {0} {1} size='10000'>{2}</span>";
		private const string m_TEMP_LARGE = "<span {0} {1} size='18000'>{2} C°</span>";
		private const string m_TEMP_MEDIUM = "<span {0} {1} size='13000'>{2} C°</span>";
		private const string m_WEATHER_CAPTION = "<span {0} {1} size='12000'>{2}</span>";
		private const string m_WEATHER_INFO = "<span {0} {1} size='10000'>{2}</span>";

		private readonly Color ColorNow = new Color (30, 60, 100);
		private readonly Color ColorNext1 = new Color (20, 50, 80);
		private readonly Color ColorNext2 = new Color (100, 20, 5);

		public WeatherPage (IPageModel model, Style style, ILogger logger)
		{
			this.Build();

			var binder = new ModelBinder (model, logger);

			CommonBindings.CreateStatusbar (binder, hbox1, style);
			CommonBindings.CreateTaskbarButtons (binder, hbox5, style);

//			// FACT
//			style.Window.Apply (eventbox_weather_now_caption);
//			style.Window.Apply (eventbox_weather_now_condition);
//			style.Window.Apply (eventbox_temp_now);
//
//			// NEXT 1
//			style.Window.Apply (eventbox_weather_next1_caption);
//			style.Window.Apply (eventbox_weather_next1_condition);
//			style.Window.Apply (eventbox_weather_next1_temp);

			// NEXT 2
			style.Window.Apply (eventbox_weather_next2_caption);
			style.Window.Apply (eventbox_weather_next2_condition);
			style.Window.Apply (eventbox_weather_next2_temp);

			// INFO
			style.Window.Apply (eventbox_info1);
			style.Window.Apply (eventbox_info2);
			style.Window.Apply (eventbox_info3);
			style.Window.Apply (eventbox_info4);
			style.Window.Apply (eventbox_info5);

			eventbox_image_weather_now.ModifyBg(Gtk.StateType.Normal, ColorNow);
			eventbox_weather_now_condition.ModifyBg(Gtk.StateType.Normal, ColorNow);
			eventbox_temp_now.ModifyBg(Gtk.StateType.Normal, ColorNow);
			eventbox_weather_now_caption.ModifyBg(Gtk.StateType.Normal, ColorNow);
			style.Window.Apply(eventbox_weather_now_separator);
			//eventbox_weather_now_separator.ModifyBg(Gtk.StateType.Normal, ColorNow);

			eventbox_image_weather_next1.ModifyBg(Gtk.StateType.Normal, ColorNext1);
			eventbox_weather_next1_condition.ModifyBg(Gtk.StateType.Normal, ColorNext1);
			eventbox_weather_next1_temp.ModifyBg(Gtk.StateType.Normal, ColorNext1);
			eventbox_weather_next1_caption.ModifyBg(Gtk.StateType.Normal, ColorNext1);
			eventbox_weather_next1_separator.ModifyBg(Gtk.StateType.Normal, ColorNext1);

			// FACT
			binder.BindCustomAction<string>(now_icon_path => image_weather_now.File = now_icon_path, "now_icon_path");
			binder.BindCustomAction<string>(now_condition => 
				label_weather_now_condition.Markup = CB.CreateMarkup(m_WEATHER_CONDITION_LARGE, CB.m_BG_EMPTY, CB.m_FG_GRAY, now_condition)
				, "now_condition");
			binder.BindCustomAction<string>(now_temp => 
				label_temp_now.Markup = CB.CreateMarkup(m_TEMP_LARGE, CB.m_BG_EMPTY, CB.m_FG_GRAY, now_temp)
				, "now_temp");

			// NEXT 1
			binder.BindCustomAction<string>(caption => label_weather_next1_caption.Markup = CB.CreateMarkup(m_WEATHER_CAPTION, CB.m_BG_EMPTY, CB.m_FG_GRAY, caption)
				, "next1_caption");
			binder.BindCustomAction<string>(icon_path => image_weather_next1.File = icon_path, "next1_icon_path");
			binder.BindCustomAction<string>(condition => 
				label_weather_next1_condition.Markup = CB.CreateMarkup(m_WEATHER_CONDITION_LARGE, CB.m_BG_EMPTY, CB.m_FG_GRAY, condition)
				, "next1_condition");
			binder.BindCustomAction<string>(temp => 
				label_weather_next1_temp.Markup = CB.CreateMarkup(m_TEMP_MEDIUM, CB.m_BG_EMPTY, CB.m_FG_GRAY, temp)
				, "next1_temp");

			// NEXT 2
			binder.BindCustomAction<string>(caption => label_weather_next2_caption.Markup = CB.CreateMarkup(m_WEATHER_CAPTION, CB.m_BG_EMPTY, CB.m_FG_GRAY, caption)
				, "next2_caption");
			binder.BindCustomAction<string>(icon_path => image_weather_next2.File = icon_path, "next2_icon_path");
			binder.BindCustomAction<string>(condition => 
				label_weather_next2_condition.Markup = CB.CreateMarkup(m_WEATHER_CONDITION_LARGE, CB.m_BG_EMPTY, CB.m_FG_GRAY, condition)
				, "next2_condition");
			binder.BindCustomAction<string>(temp => 
				label_weather_next2_temp.Markup = CB.CreateMarkup(m_TEMP_MEDIUM, CB.m_BG_EMPTY, CB.m_FG_GRAY, temp)
				, "next2_temp");

			//INFO
			binder.BindCustomAction<string>(info => label_info1.Markup = CB.CreateMarkup(m_WEATHER_INFO, CB.m_BG_EMPTY, CB.m_FG_GRAY_DARK, info)
				, "info1");
			binder.BindCustomAction<string>(info => label_info2.Markup = CB.CreateMarkup(m_WEATHER_INFO, CB.m_BG_EMPTY, CB.m_FG_GRAY_DARK, info)
				, "info2");
			binder.BindCustomAction<string>(info => label_info3.Markup = CB.CreateMarkup(m_WEATHER_INFO, CB.m_BG_EMPTY, CB.m_FG_GRAY_DARK, info)
				, "info3");
			binder.BindCustomAction<string>(info => label_info4.Markup = CB.CreateMarkup(m_WEATHER_INFO, CB.m_BG_EMPTY, CB.m_FG_GRAY_DARK, info)
				, "info4");
			binder.BindCustomAction<string>(info => label_info5.Markup = CB.CreateMarkup(m_WEATHER_INFO, CB.m_BG_EMPTY, CB.m_FG_GRAY_DARK, info)
				, "info5");

			CreateAndBindForecast(binder, 1);
			CreateAndBindForecast(binder, 2);
			CreateAndBindForecast(binder, 3);
			CreateAndBindForecast(binder, 4);
			CreateAndBindForecast(binder, 5);
			CreateAndBindForecast(binder, 6);

			binder.UpdateBindings();
		}

		private void CreateAndBindForecast(ModelBinder binder, int day)
		{
			var label_caption = new Gtk.Label () { UseMarkup = true };
			binder.BindCustomAction<string>(text => label_caption.Markup = 
				CB.CreateMarkup("<span {0} {1} size='12000'>{2}</span>", CB.m_BG_EMPTY, CB.m_FG_GRAY, text), 
				string.Format("day{0}_caption", day));

			var label_date = new Gtk.Label () { UseMarkup = true };
			binder.BindCustomAction<string>(text => label_date.Markup = 
				CB.CreateMarkup("<span {0} {1} size='9000'>{2}</span>", CB.m_BG_EMPTY, CB.m_FG_GRAY_DARK, text), 
				string.Format("day{0}_date", day));

			var image = new Gtk.Image ();
			binder.BindCustomAction<string>(image_path => image.Pixbuf = Scale(image_path, 24), 
				string.Format("day{0}_image_path", day));

			var label_day_temp = new Gtk.Label () { UseMarkup = true };
			binder.BindCustomAction<string>(text => label_day_temp.Markup = 
				CB.CreateMarkup("<span {0} {1} size='12000'>{2}°</span>", CB.m_BG_EMPTY, CB.m_FG_GRAY, text), 
				string.Format("day{0}_day_temp", day));

			var label_night_temp = new Gtk.Label () { UseMarkup = true };
			binder.BindCustomAction<string>(text => label_night_temp.Markup = 
				CB.CreateMarkup("<span {0} {1} size='9000'>{2}°</span>", CB.m_BG_EMPTY, CB.m_FG_GRAY_DARK, text), 
				string.Format("day{0}_night_temp", day));

			var stack = new Gtk.VBox () { BorderWidth = 15 };
			stack.Add(label_caption);
			stack.Add(label_date);
			stack.Add(image);
			stack.Add(label_day_temp);
			stack.Add(label_night_temp);
			box_forecast.Add(stack);
		}

		private Pixbuf Scale(string filePath, int size)
		{
			return new Pixbuf (filePath).ScaleSimple(size, size, InterpType.Bilinear);
		}
	}
}

