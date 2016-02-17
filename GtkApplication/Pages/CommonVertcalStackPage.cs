using System;
using Interfaces.UI;
using Gtk;
using Interfaces;
using GtkApplication.Pages;
using GtkApplication.Controls;

namespace GtkApplication
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CommonVertcalStackPage : Gtk.Bin
	{
        private const string m_ITEM = "<span {0} size='15000'>{1}</span>";

		private readonly IPageModel model;
		private readonly Style style;
		private readonly ILogger logger;
        private readonly VerticalRotaryList vrl;

        public CommonVertcalStackPage(IPageModel model, Style style, ILogger logger)
		{
			this.model = model;
			this.style = style;
			this.logger = logger;
			var binder = new ModelBinder (model, logger);

			this.Build();

			style.Window.Apply(label_caption, eventbox_caption);

            binder.BindLabelText(label_caption, ModelNames.PageTitle);

            vrl = new VerticalRotaryList(binder, vbox1, style, model.GetProperty<string>("items_source_prop_name"));

            binder.InitializeButton(style, eventbox_exit_button, style.CancelButton, ModelNames.ButtonCancel, TextAligment.CenterMiddle);
            binder.InitializeButton(style, eventbox_next_button, style.AcceptButton, ModelNames.ButtonAccept, TextAligment.CenterMiddle);

            CreateUpDownButtons(binder, style);
            
            binder.UpdateBindings();
		}

        private void CreateUpDownButtons(ModelBinder binder, Style style)
        {
            var btn = new FlatButton(eventbox_bUp, style.CommonButton, TextAligment.CenterMiddle);
            btn.WidthRequest = 20;

            binder.BindFlatButtonLabel(btn, ModelNames.ButtonPrevLabel);
            binder.BindFlatButtonClick(btn, ModelNames.ButtonPrev);

            btn = new FlatButton(eventbox_bDown, style.CommonButton, TextAligment.CenterMiddle);
            btn.WidthRequest = 20;

            binder.BindFlatButtonLabel(btn, ModelNames.ButtonNextLabel);
            binder.BindFlatButtonClick(btn, ModelNames.ButtonNext);
        }
	}
}

