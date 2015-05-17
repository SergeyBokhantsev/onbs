using System;
using Gtk;
using GtkApplication.Pages;
using Interfaces.UI;

namespace GtkApplication
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class MainPage : Gtk.Bin
	{
        private readonly IPageModel model;
        private readonly ModelBinder binder;

		public MainPage(IPageModel model)
		{
			this.Build();

            this.model = model;
            
            binder = new ModelBinder(model);

            binder.BindLabelText(welcome);
            binder.BindButtonClick(bStart, "start");
		}
	}
}

