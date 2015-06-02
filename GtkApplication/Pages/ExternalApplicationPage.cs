using System;
using Interfaces.UI;
using GtkApplication.Pages;
using System.Collections.Generic;

namespace GtkApplication
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ExternalApplicationPage : Gtk.Bin
	{
        private readonly IPageModel model;
        private readonly ModelBinder binder;

		public ExternalApplicationPage (IPageModel model)
		{
			this.Build();

            this.model = model;
            binder = new ModelBinder(model);
            
            binder.BindLabelText(label_launch_info);
            
            binder.BindEventBoxBgColor(eventbox1, "is_error", new Dictionary<string, Gdk.Color>()
            {
                { "0", ModelBinder.Qpaque },
                { "1", new Gdk.Color(255, 10, 10) }
            });

            binder.BindButtonLabel(button_close, "button_exit_label");
            binder.BindButtonClick(button_close, "close");
		}
	}
}

