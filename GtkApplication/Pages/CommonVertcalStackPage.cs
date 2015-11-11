using System;
using Interfaces.UI;
using Gtk;
using Interfaces;
using GtkApplication.Pages;

namespace GtkApplication
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CommonVertcalStackPage : Gtk.Bin
	{
		private readonly IPageModel model;
		private readonly ModelBinder binder;
		private readonly Style style;
		private readonly ILogger logger;

        public CommonVertcalStackPage(IPageModel model, Style style, ILogger logger)
		{
			this.model = model;
			this.style = style;
			this.logger = logger;
			this.binder = new ModelBinder (model, logger);

			this.Build();

			style.Window.Apply(label_caption, eventbox_caption);

            binder.BindLabelText(label_caption, ModelNames.PageTitle);

			binder.InitializeButton(style, eventbox_exit_button, style.CancelButton, ModelNames.ButtonCancel, TextAligment.CenterMiddle);
			binder.InitializeButton(style, eventbox_next_button, style.AcceptButton, ModelNames.ButtonAccept, TextAligment.CenterMiddle);
			binder.InitializeButton(style, eventbox_f1_button, style.CommonButton, ModelNames.ButtonF1, TextAligment.LeftMiddle);
			binder.InitializeButton(style, eventbox_f2_button, style.CommonButton, ModelNames.ButtonF2, TextAligment.LeftMiddle);
			binder.InitializeButton(style, eventbox_f3_button, style.CommonButton, ModelNames.ButtonF3, TextAligment.LeftMiddle);
			binder.InitializeButton(style, eventbox_f4_button, style.CommonButton, ModelNames.ButtonF4, TextAligment.LeftMiddle);
			binder.InitializeButton(style, eventbox_f5_button, style.CommonButton, ModelNames.ButtonF5, TextAligment.LeftMiddle);
			binder.InitializeButton(style, eventbox_f6_button, style.CommonButton, ModelNames.ButtonF6, TextAligment.LeftMiddle);
			binder.InitializeButton(style, eventbox_f7_button, style.CommonButton, ModelNames.ButtonF7, TextAligment.LeftMiddle);
			binder.InitializeButton(style, eventbox_f8_button, style.CommonButton, ModelNames.ButtonF8, TextAligment.LeftMiddle);
		}
	}
}

