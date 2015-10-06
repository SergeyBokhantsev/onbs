using System;
using Interfaces.UI;
using Interfaces;
using Gtk;
using Gdk;
using GtkApplication.Pages;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace GtkApplication
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OBDEngineAndFuel : Gtk.Bin
	{
		private const string m_Primary1Value = "<span foreground='#30AACC' size='25000'>{0}</span>";
		private const string m_Primary1Max = "<span foreground='#cccccc' size='20000'>{0}</span>";
		private const string m_Primary2Value = "<span foreground='#CCCC38' size='25000'>{0}</span>";
		private const string m_Primary2Max = "<span foreground='#cccccc' size='20000'>{0}</span>";
		private const string m_Par = "<span foreground='#cccccc' size='20000'>{0} </span><span foreground='#cccccc' size='40000'>{1}</span><span foreground='#cccccc' size='20000'> {2}</span>";

		private Gdk.GC chartGC;
		private Gdk.GC primary1GC;
		private Gdk.GC primary2GC;
		private Gdk.GC primary3GC;

		//Pixbuf mapContents = new Pixbuf (@"C:\Users\Mau\Desktop\Car.png");

		//int wWidth;
		//int wHeight;

		private ChartData<double> primary1Data = new ChartData<double> (200);
		private ChartData<double> primary2Data = new ChartData<double> (200);
		private ChartData<double> primary3Data = new ChartData<double> (200);

		private string par1prefix;
		private string par2prefix;
		private string par3prefix;

		private string par1suffix;
		private string par2suffix;
		private string par3suffix;

		public OBDEngineAndFuel(IPageModel model, Style style, ILogger logger)
		{
			this.Build();
		
			//wWidth = mapContents.Width; wHeight = mapContents.Height;
			//d_chart.SetSizeRequest (wWidth, wHeight);

			d_chart.ExposeEvent += ChartExposeEvent;

			style.Window.Apply(eventbox_primary1);
			style.Window.Apply(eventbox_primary2);
			style.Window.Apply(eventbox_secondary1);
			style.Window.Apply(eventbox_secondary2);
			style.Window.Apply(eventbox_secondary3);
			style.Window.Apply(eventbox_primary1_max);
			style.Window.Apply(eventbox_primary2_max);

			var binder = new ModelBinder (model, logger);

			progressbar_primary2.ModifyFg(StateType.Prelight, new Color(80,80,0));
			progressbar_primary2.ModifyBg(StateType.Prelight, new Color(80,80,0));

			binder.BindCustomAction<double>(v => 
				{
				primary1Data.AddPoint(v);
				var max = Math.Max(v, primary1Data.MaxValue);
				progressbar_primary1.Fraction = max > 0 ? (v / max) : 0;

					label_primary1.Markup = CommonBindings.CreateMarkup(m_Primary1Value, v.ToString("0.###"));
				label_primary1_max.Markup = CommonBindings.CreateMarkup(m_Primary1Max, max.ToString("0.###"));
				}, "primary1");

			binder.BindCustomAction<double>(v => 
			{
				primary2Data.AddPoint(v);
				var max = Math.Max(v, primary2Data.MaxValue);
				progressbar_primary2.Fraction = max > 0 ? (v / max) : 0;

				label_primary2.Markup = CommonBindings.CreateMarkup(m_Primary2Value, v.ToString("0.###"));
				label_primary2_max.Markup = CommonBindings.CreateMarkup(m_Primary2Max, max.ToString("0.###"));
			}, "primary2");

			binder.BindCustomAction<double>(v => 
			{
				primary3Data.AddPoint(v);
			}, "primary3");

			binder.BindCustomAction<string>(prefix => par1prefix = prefix, "secondary1prefix");
			binder.BindCustomAction<string>(suffix => par1suffix = suffix, "secondary1suffix");
			binder.BindCustomAction<double>(par1 => label_secondary1.Markup = CommonBindings.CreateMarkup(m_Par, par1prefix, par1.ToString("0.###"), par1suffix), "secondary1");

			binder.BindCustomAction<string>(prefix => par2prefix = prefix, "secondary2prefix");
			binder.BindCustomAction<string>(suffix => par2suffix = suffix, "secondary2suffix");
			binder.BindCustomAction<double>(par2 => label_secondary2.Markup = CommonBindings.CreateMarkup(m_Par, par2prefix, par2.ToString("0.###"), par2suffix), "secondary2");

			binder.BindCustomAction<string>(prefix => par3prefix = prefix, "secondary3prefix");
			binder.BindCustomAction<string>(suffix => par3suffix = suffix, "secondary3suffix");
			binder.BindCustomAction<double>(par3 => label_secondary3.Markup = CommonBindings.CreateMarkup(m_Par, par3prefix, par3.ToString("0.###"), par3suffix), "secondary3");

            binder.BindCustomAction<object>(o => d_chart.QueueDraw(), "refresh");

			d_chart.ModifyBg(StateType.Normal, style.Window.Bg);

            binder.UpdateBindings();
		}

		private void DrawChart(ChartData<double> chart, Gdk.EventExpose e, Gdk.GC gc)
		{
			if (chart.PointsCount > 1)
			{
				double pxPerValueByY = (double)e.Area.Height / (double)chart.MaxValue;

				var p = chart.GetPoints().GetEnumerator();
				p.MoveNext();

				int x = 0;
                int y = (int)(p.Current * pxPerValueByY);

				for (int i =0; i < chart.PointsCount -1; ++i)
				{
                    p.MoveNext();

					int xx = (int)((double)e.Area.Width / ((double)chart.MaxLen - 1d) * i);
                    int yy = (int)(p.Current * pxPerValueByY);

					e.Window.DrawLine(gc, x, e.Area.Height - y, xx, e.Area.Height - yy);

                    x = xx;
                    y = yy;
				}
			}
		}

		private void DrawChartTable(Gdk.EventExpose e, Gdk.GC gc)
		{
			int vertBarsCount = 5;
			int vertBarHeight = e.Area.Height / vertBarsCount;

			for (int i = 0; i < vertBarsCount; ++i)
			{
				int y = vertBarHeight * i;
				e.Window.DrawLine(gc, 0, y, e.Area.Width, y);
			}

			e.Window.DrawLine(gc, 0, e.Area.Height - 1, e.Area.Width, e.Area.Height - 1);
		}

		void ChartExposeEvent (object o, Gtk.ExposeEventArgs _args)
		{
			if (chartGC == null)
			{
				chartGC = new Gdk.GC(_args.Event.Window);
				chartGC.RgbFgColor = new Color (120, 120, 155);
				chartGC.SetLineAttributes(1, LineStyle.OnOffDash, CapStyle.Butt, JoinStyle.Bevel);
			}



			if (primary1GC == null)
			{
				primary1GC = new Gdk.GC(_args.Event.Window);
				primary1GC.RgbFgColor = new Color (220, 190, 55);
				primary1GC.SetLineAttributes(4, LineStyle.Solid, CapStyle.Butt, JoinStyle.Bevel);
			}

			DrawChart(primary1Data, _args.Event, primary1GC);

			if (primary2GC == null)
			{
				primary2GC = new Gdk.GC(_args.Event.Window);
				primary2GC.RgbFgColor = new Color (120, 80, 255);
				primary2GC.SetLineAttributes(4, LineStyle.Solid, CapStyle.Butt, JoinStyle.Bevel);
			}

			DrawChart(primary2Data, _args.Event, primary2GC);

			if (primary3GC == null)
			{
				primary3GC = new Gdk.GC(_args.Event.Window);
				primary3GC.RgbFgColor = new Color (255, 30, 30);
				primary3GC.SetLineAttributes(4, LineStyle.Solid, CapStyle.Butt, JoinStyle.Bevel);
			}

			DrawChart(primary3Data, _args.Event, primary3GC);

			DrawChartTable(_args.Event, chartGC);

			//_args.Event.Window.DrawLine(Style.BlackGC, 0, 0, 100, 100);

            //var s = new Gtk.Style();
            
			//Widget widget = (Widget) o;

			//Gdk.Rectangle area = _args.Event.Area;
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

	internal class ChartData<T>
		where T: IComparable<T>
	{
        private readonly int maxLen;
        private readonly Queue<T> points;


		public int PointsCount
		{
			get
			{
				return points.Count;
			}
		}

		public int MaxLen
		{
			get
			{
				return maxLen;
			}
		}

        public T MaxValue
        {
            get;
            private set;
        }

        public ChartData(int pointsCount)
        {
            this.maxLen = pointsCount;
            this.points = new Queue<T>(pointsCount);
        }

		public virtual T FindCurrentMax()
        {
            if (points.Count == 0)
                return default(T);
            else
            {
                var max = points.Peek();

                foreach (var p in points)
                {
                    if (p.CompareTo(max) > 0)
                        max = p;
                }

                return max;
            }
        }

        public virtual void AddPoint(T point)
        {
            if (points.Count == maxLen)
                points.Dequeue();
            points.Enqueue(point);

            if (MaxValue.CompareTo(point) < 0)
                MaxValue = point;
        }

        public IEnumerable<T> GetPoints()
        {
            return points;
        }
	}
}

