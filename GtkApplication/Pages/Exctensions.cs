using System;
using Gtk;
using Interfaces.UI;
using Interfaces;
using GtkApplication.Pages;

namespace GtkApplication
{
	internal static class Exctensions
	{
		internal static void InitializeButton(this ModelBinder binder, Style style, EventBox box, LookAndFeel lf, string buttonName, TextAligment align)
		{
			var btnLabelPropertyName = ModelNames.ResolveButtonLabelName(buttonName);

			if (!string.IsNullOrEmpty(binder.Model.GetProperty<string>(btnLabelPropertyName)))
			{
				var btn = new FlatButton (box, lf, align);

				binder.BindFlatButtonLabel(btn, btnLabelPropertyName, buttonName);
				binder.BindFlatButtonClick(btn, buttonName);
			} else
			{
				var stubLabel = new Label ();
				box.Add(stubLabel);
				style.Window.Apply(stubLabel, box);
			}
		}
	}
}

