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

			binder.BindLabelMarkup(label_codes, "codes", v => CommonBindings.CreateMarkup(m_Codes, CommonBindings.m_FG_BLUE, v != null ? v.ToString() : "None"));

			label_buttons.UseMarkup = true;
			label_buttons.Markup = CommonBindings.CreateMarkup (m_Buttons, CommonBindings.m_FG_GRAY, CreateButtonsList (model));

			binder.UpdateBindings();
		}

		private string CreateButtonsList(IPageModel model)
		{
			var result = new StringBuilder ();

			foreach (var buttonNamePair in ModelNames.GetButtonAndLabelNames()) 
			{
				var bCaption = model.GetProperty<object> (buttonNamePair.Value);

				if (bCaption != null) 
				{
					result.AppendFormat ("{0}: {1}        ", buttonNamePair.Key, bCaption);
				}
			}

			return result.ToString ();
		}
	}
}

