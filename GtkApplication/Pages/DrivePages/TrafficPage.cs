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

			CommonBindings.CreateStatusbar (binder, hbox1, style);
			CommonBindings.CreateTaskbarButtons (binder, hbox5, style);

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

