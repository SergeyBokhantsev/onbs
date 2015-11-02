using System;
using Interfaces.UI;
using Interfaces;
using GtkApplication.Pages;

namespace GtkApplication
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class OBD_DTCPage : Gtk.Bin
	{
		private const string m_Codes = "<span {0} size='20000'>{1}</span>";

		public OBD_DTCPage (IPageModel model, Style style, ILogger logger)
		{
			this.Build ();

			style.Window.Apply (eventbox_codes);

			var binder = new ModelBinder (model, logger);

			binder.BindLabelMarkup(label_codes, "codes", v => CommonBindings.CreateMarkup(m_Codes, CommonBindings.m_FG_BLUE, v != null ? v.ToString() : "None"));

			binder.UpdateBindings();
		}
	}
}

