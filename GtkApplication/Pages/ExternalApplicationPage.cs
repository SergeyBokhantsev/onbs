using System;
using Interfaces.UI;
using GtkApplication.Pages;

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
            binder.BindButtonClick(button_close, "close");
		}
	}
}

