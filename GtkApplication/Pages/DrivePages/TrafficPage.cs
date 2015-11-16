using System;
using Interfaces.UI;
using Interfaces;
using System.Diagnostics;
using GtkApplication.Pages;
using Interfaces.GPS;
using CB = GtkApplication.CommonBindings;
using Gdk;
using System.IO;

namespace GtkApplication
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class TrafficPage : Gtk.Bin
	{
		private readonly CommonBindings commonBindings;
        private Gdk.PixbufAnimation daisy;

		public TrafficPage (IPageModel model, Style style, ILogger logger)
		{
			this.Build ();
		
			var binder = new ModelBinder (model, logger);

			CommonBindings.CreateTaskbarButtons (binder, hbox5, style);

			binder.BindLabelMarkup(label15, "wnd_status", o => 
				CommonBindings.CreateMarkup(
					CommonBindings.m_WND_STATUS,
					CommonBindings.m_BG_EMPTY,
					CommonBindings.m_FG_GRAY,
					o != null ? o.ToString() : string.Empty));

            binder.BindCustomAction<string>(gifPath =>
            {
                if (gifPath != null)
                {
                    daisy = new Gdk.PixbufAnimation(gifPath);
                }
            }, "daisy_path");

			binder.BindCustomAction<Stream> (imageStream => 
				{
                    //ONLY MEMORY STREAMS IMPL.
                    var stream = imageStream as MemoryStream;

                    if (stream != null)
                    {
                        var loader = new PixbufLoader();
                        loader.Write(stream.ToArray());
                        loader.Close();
                        image_traffic.Pixbuf = loader.Pixbuf;
                    }
                    else if (daisy != null)
                    {
                        image_traffic.PixbufAnimation = daisy;
                    }
				}, "traffic_image_stream");

			binder.UpdateBindings();
		}

		private Pixbuf Scale(string filePath, int size)
		{
			return new Pixbuf (filePath).ScaleSimple(size, size, InterpType.Bilinear);
		}
	}
}

