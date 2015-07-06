using System;
using Interfaces.UI;
using Interfaces;
using GtkApplication.Pages;

namespace GtkApplication
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class CommonYesNoPage : Gtk.Bin
	{
		private readonly IPageModel model;
		private readonly ModelBinder binder;
		private readonly Style style;
		private readonly ILogger logger;

		public CommonYesNoPage (IPageModel model, Style style, ILogger logger)
		{
			this.model = model;
			this.style = style;
			this.logger = logger;
			this.binder = new ModelBinder (model, logger);

			this.Build ();

			style.Window.Apply(label_caption, eventbox_caption);
			style.Window.Apply(label_message, eventbox_message);

			binder.BindLabelText(label_caption);

			//binder.BindLabelText(label_message);
			binder.BindLabelMarkup(label_message, null, new Func<object, string>(t =>
				{
					return string.Format("<span size='28000'>{0}</span>", t as string);
				}));

			binder.InitializeButton(style, eventbox_no_button, style.CancelButton, ModelNames.ButtonCancel, TextAligment.CenterMiddle);
			binder.InitializeButton(style, eventbox_yes_button, style.AcceptButton, ModelNames.ButtonAccept, TextAligment.CenterMiddle);
		}
	}
}

