using System;
using Interfaces.UI;
using GtkApplication.Pages;
using System.Collections.Generic;
using Interfaces;

namespace GtkApplication
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ExternalApplicationPage : Gtk.Bin
	{
        private readonly IPageModel model;
        private readonly ModelBinder binder;

		public ExternalApplicationPage (IPageModel model, Style style, ILogger logger)
		{
			this.Build();

			var bClose = new FlatButton (box_close_button, style.CancelButton);

			style.TextBox.Apply (label_launch_info, eventbox1);

            this.model = model;
			binder = new ModelBinder(model, logger);
            
            binder.BindLabelText(label_launch_info);
            
            binder.BindEventBoxBgColor(eventbox1, "is_error", new Dictionary<string, Gdk.Color>()
            {
					{ "0", style.TextBox.Bg },
                	{ "1", new Gdk.Color(200, 10, 10) }
            });

			binder.BindFlatButtonLabel(bClose, "button_exit_label", "CANCEL");
			binder.BindFlatButtonClick(bClose, "Cancel");
		}
	}
}

