using System;
using System.Drawing;
using Gdk;
using Gtk;
using GtkApplication.Pages;
using Interfaces;
using Interfaces.UI;
using System.Text;

namespace GtkApplication
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class MainPage : Gtk.Bin
	{
        private readonly IPageModel model;
        private readonly ModelBinder binder;

      //  Pixmap pm;
       // System.Drawing.Graphics g;
      //  Bitmap bmp;

		public MainPage(IPageModel model, LookAndFeel style, ILogger logger)
		{
			this.Build();

			var bF1 = new FlatButton (eventbox1, style);

            this.model = model;

			binder = new ModelBinder(model, logger);
		
			binder.BindFlatButtonLabel(bF1, "nav_btn_caption", "F1");
            binder.BindFlatButtonClick(bF1, "nav");

            binder.BindButtonLabel(btn_f2, "cam_btn_caption", "F2");
            binder.BindButtonClick(btn_f2, "cam");

            binder.BindButtonLabel(btn_f3, string.Empty);
            binder.BindButtonLabel(btn_f4, string.Empty);
            binder.BindButtonLabel(btn_f5, string.Empty);
            binder.BindButtonLabel(btn_f6, string.Empty);
            binder.BindButtonLabel(btn_f7, string.Empty);
            binder.BindButtonLabel(btn_f8, string.Empty);
            binder.BindButtonLabel(btn_cancel, string.Empty);
            binder.BindButtonLabel(btn_accept, string.Empty);

            binder.BindMetrics(UpdateMetrics, "metrics");

			//eventbox1.ModifyBg(StateType.Normal, new Gdk.Color(200,30,50));
			//eventbox1.ModifyFg(StateType.Normal, new Gdk.Color(100,30,50));
		//	btn_f1.ModifyFg(StateType.Normal, new Gdk.Color(0,30,50));



            //image1.ExposeEvent += Image1_ExposeEvent;

         //   pm = new Pixmap(null, 100, 100, 24);
         //   g = Gtk.DotNet.Graphics.FromDrawable(pm);

            //drawingarea1.ExposeEvent += Image1_ExposeEvent;

            //bmp = new Bitmap(@"C:\Users\sergeyb\Desktop\New folder (6)\inktools\btn_ink_erase2_sel.png");
          
           // image1.SetFromPixmap(pm, null);

            model.SetProperty(string.Empty, string.Empty);
		}

        private void UpdateMetrics(IMetrics metrics)
        {
            switch (metrics.ProviderName)
            {
                case "Arduino Controller":
                    UpdateArduinoMetrics(metrics);
                    break;
            }
        }

        private void UpdateArduinoMetrics(IMetrics metrics)
        {
            StringBuilder text = new StringBuilder();
            foreach (var pair in metrics)
            {
                text.Append(string.Concat(pair.Key, ": ", pair.Value, Environment.NewLine));
            }
            label_arduino_metrics.Buffer.Text = text.ToString().TrimEnd();
        }

        private void Image1_ExposeEvent(object o, ExposeEventArgs args)
        {
//            var r = new Random();
//            g.DrawRectangle(new Pen(System.Drawing.Color.FromArgb(r.Next(255), r.Next(255), r.Next(255))), new System.Drawing.Rectangle(0, 0, 90, 90));
//
//            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
//            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
//            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
//            g.RotateTransform(17);
//
//            g.DrawImage(bmp, new PointF(0, 0));           
//
//            Gdk.Rectangle area = args.Event.Area;
//            args.Event.Window.DrawDrawable(drawingarea1.Style.WhiteGC, pm,
//              area.X, area.Y,
//                  area.X, area.Y,
//                   area.Width, area.Height);




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

