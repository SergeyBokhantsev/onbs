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

            image1.ExposeEvent += Image1_ExposeEvent;
		}

        private void Image1_ExposeEvent(object o, ExposeEventArgs args)
        {
            using (System.Drawing.Graphics graphics =
                    Gtk.DotNet.Graphics.FromDrawable(args.Event.Window))
            {
                // draw your stuff here...
                graphics.DrawLine(new System.Drawing.Pen(System.Drawing.Brushes.Black), 0, 0, 30, 40);
            }
        }
    }
}

