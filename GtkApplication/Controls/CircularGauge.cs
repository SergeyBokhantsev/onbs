using Gtk;
using System;
using Cairo;

namespace GtkApplication
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CircularGauge : DrawingArea
	{
		private readonly EventBox parent;
		private readonly Style style;

        private int val;

        public int Value
        {
            get { return val; }
            set 
			{ 
				val = value; 
				this.QueueDraw(); 
			}
        }

		public CircularGauge(EventBox parent, Style style)
		{
			this.parent = parent;
			this.style = style;

			parent.Add(this);

			ModifyBg(StateType.Normal, style.Window.Bg);
		}

        protected override bool OnExposeEvent(Gdk.EventExpose args)
        {
            Cairo.Context cr = Gdk.CairoHelper.Create(args.Window);

			cr.LineWidth = 40;

			cr.SetSourceRGB(0.3, 0.4, 0.6);

            int width, height;
			width = Allocation.Width;
			height = Allocation.Height;

			cr.Translate(width / 2, height / 2);
			cr.Arc(0, 0, (width < height ? width : height) / 2 - 10, 0, 2);
			cr.StrokePreserve();

			cr.SetSourceRGB(0.3, 0.4, 0.6);
			//cr.Fill();

			//((IDisposable)cr.Target).Dispose();
			((IDisposable)cr).Dispose();

            return true;
        }
	}
}
