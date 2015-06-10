using System;
using Interfaces.UI;
using Gtk;
using Interfaces;
using GtkApplication.Pages;

namespace GtkApplication
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SystemConfigurationPage : Gtk.Bin
	{
		private readonly IPageModel model;
		private readonly ModelBinder binder;
		private readonly Style style;
		private readonly ILogger logger;

		public SystemConfigurationPage (IPageModel model, Style style, ILogger logger)
		{
			this.model = model;
			this.style = style;
			this.logger = logger;
			this.binder = new ModelBinder (model, logger);

			this.Build();

			style.Window.Apply(label_caption, eventbox_caption);

			binder.BindLabelText(label_caption);

			InitializeButton(eventbox_exit_button, style.CancelButton, ModelNames.ButtonCancel, TextAligment.CenterMiddle);
			InitializeButton(eventbox_next_button, style.AcceptButton, ModelNames.ButtonAccept, TextAligment.CenterMiddle);
			InitializeButton(eventbox_f1_button, style.CommonButton, ModelNames.ButtonF1, TextAligment.LeftMiddle);
			InitializeButton(eventbox_f2_button, style.CommonButton, ModelNames.ButtonF2, TextAligment.LeftMiddle);
			InitializeButton(eventbox_f3_button, style.CommonButton, ModelNames.ButtonF3, TextAligment.LeftMiddle);
			InitializeButton(eventbox_f4_button, style.CommonButton, ModelNames.ButtonF4, TextAligment.LeftMiddle);
			InitializeButton(eventbox_f5_button, style.CommonButton, ModelNames.ButtonF5, TextAligment.LeftMiddle);
			InitializeButton(eventbox_f6_button, style.CommonButton, ModelNames.ButtonF6, TextAligment.LeftMiddle);
			InitializeButton(eventbox_f7_button, style.CommonButton, ModelNames.ButtonF7, TextAligment.LeftMiddle);
			InitializeButton(eventbox_f8_button, style.CommonButton, ModelNames.ButtonF8, TextAligment.LeftMiddle);
		}

		private void InitializeButton(EventBox box, LookAndFeel lf, string buttonName, TextAligment align)
		{
			var btnLabelPropertyName = ModelNames.ResolveButtonLabelName(buttonName);

			if (!string.IsNullOrEmpty(model.GetProperty<string>(btnLabelPropertyName)))
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

