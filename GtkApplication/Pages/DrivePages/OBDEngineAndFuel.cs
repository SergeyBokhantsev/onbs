using System;
using Interfaces.UI;
using Interfaces;
using Gtk;
using Gdk;

namespace GtkApplication
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OBDEngineAndFuel : Gtk.Bin
	{
		Pixbuf mapContents = new Pixbuf (@"C:\Users\Mau\Desktop\Car.png");

		int wWidth;
		int wHeight;

		public OBDEngineAndFuel(IPageModel model, Style style, ILogger logger)
		{
			this.Build();
		
			wWidth = mapContents.Width; wHeight = mapContents.Height;
			d_chart.SetSizeRequest (wWidth, wHeight);

			d_chart.ExposeEvent += ChartExposeEvent;

		
		}

		void ChartExposeEvent (object o, Gtk.ExposeEventArgs _args)
		{
			_args.Event.Window.DrawLine(Style.BlackGC, 0, 0, 100, 100);

			Widget widget = (Widget) o;

			Gdk.Rectangle area = _args.Event.Area;
//			widget.GdkWindow.DrawPixbuf(widget.Style.BlackGC,
//				mapContents,
//				area.X, area.Y,
//				area.X, area.Y,
//				wWidth, wHeight,
//				RgbDither.Normal,
//				area.X, area.Y);

			_args.RetVal = true;
		}
	}
}

