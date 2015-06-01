using System;
using System.Drawing;
using Gdk;
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

        Pixmap pm;
        System.Drawing.Graphics g;
        Bitmap bmp;

		public MainPage(IPageModel model)
		{
			this.Build();

            this.model = model;
            
            binder = new ModelBinder(model);

            binder.BindLabelText(label_f1);
            binder.BindButtonClick(bStart, "navit");

            //image1.ExposeEvent += Image1_ExposeEvent;

            pm = new Pixmap(null, 100, 100, 24);
            g = Gtk.DotNet.Graphics.FromDrawable(pm);

            //drawingarea1.ExposeEvent += Image1_ExposeEvent;

            //bmp = new Bitmap(@"C:\Users\sergeyb\Desktop\New folder (6)\inktools\btn_ink_erase2_sel.png");
          
           // image1.SetFromPixmap(pm, null);
		}

        private void Image1_ExposeEvent(object o, ExposeEventArgs args)
        {
            var r = new Random();
            g.DrawRectangle(new Pen(System.Drawing.Color.FromArgb(r.Next(255), r.Next(255), r.Next(255))), new System.Drawing.Rectangle(0, 0, 90, 90));

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.RotateTransform(17);

            g.DrawImage(bmp, new PointF(0, 0));           

            Gdk.Rectangle area = args.Event.Area;
            args.Event.Window.DrawDrawable(drawingarea1.Style.WhiteGC, pm,
              area.X, area.Y,
                  area.X, area.Y,
                   area.Width, area.Height);

           // image1.SetFromPixmap(pm, null);

            //using (System.Drawing.Graphics graphics =
            //        Gtk.DotNet.Graphics.FromDrawable(args.Event.Window))
            //{
            //    // draw your stuff here...
            //    graphics.DrawLine(new System.Drawing.Pen(System.Drawing.Brushes.Black), 0, 0, 30, 40);
            //}
        }
    }
}

