using System;
using Interfaces.UI;
using Interfaces;
using GtkApplication.Pages;
using System.Text;

namespace GtkApplication
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class OBD_DTCPage : Gtk.Bin
	{
		private const string m_Codes = "<span {0} size='18000'>{1}</span>";
		private const string m_Buttons = "<span {0} size='12000'>{1}</span>";

		public OBD_DTCPage (IPageModel model, Style style, ILogger logger)
		{
			this.Build ();

			style.Window.Apply (eventbox_codes);
			style.Window.Apply (eventbox_buttons);

			var binder = new ModelBinder (model, logger);

			binder.BindCustomAction<string>(content => label_codes.Markup = CommonBindings.CreateMarkup(m_Codes, CommonBindings.m_FG_YELLOW, content), "codes");

			binder.InitializeButton(style, eventbox_refresh, style.CommonButton, ModelNames.ButtonAccept, TextAligment.CenterMiddle);
			binder.InitializeButton(style, eventbox_reset, style.CommonButton, ModelNames.ButtonF1, TextAligment.CenterMiddle);
			binder.InitializeButton(style, eventbox_back, style.CancelButton, ModelNames.ButtonCancel, TextAligment.CenterMiddle);

			binder.UpdateBindings();
		}
	}
}

