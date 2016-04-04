using System;
using Interfaces.UI;
using Interfaces;
using GtkApplication.Pages;

namespace GtkApplication
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class LightSensorInfoPage : Gtk.Bin
	{
		private const string m_TEXT = "<span {0} size='40000'>{1}</span>";
		private const string m_MESSAGE = "<span {0} size='16000'>{1}</span>";

		public LightSensorInfoPage(IPageModel model, Style style, ILogger logger)
		{
			this.Build();

			var binder = new ModelBinder (model, logger);

			style.Window.Apply (eventbox1);
			style.Window.Apply (eventbox2);
			style.Window.Apply (eventbox3);
			style.Window.Apply (eventbox4);
			style.Window.Apply (eventbox5);

			l_separator.Markup = CommonBindings.CreateMarkup (m_TEXT, CommonBindings.m_FG_GRAY_DARK, "|");

			binder.BindLabelMarkup(l_sensor_a, "sensor_a", v => CommonBindings.CreateMarkup(m_TEXT, CommonBindings.m_FG_GRAY, v));
			binder.BindLabelMarkup(l_sensor_b, "sensor_b", v => CommonBindings.CreateMarkup(m_TEXT, CommonBindings.m_FG_GRAY, v));

			binder.BindLabelMarkup(l_message, "message", v => CommonBindings.CreateMarkup(m_MESSAGE, CommonBindings.m_FG_GRAY, v));
		}
	}
}

