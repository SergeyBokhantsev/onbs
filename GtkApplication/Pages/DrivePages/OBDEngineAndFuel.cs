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
		private const string m_FuelFlowValue = "<span foreground='#30AACC' size='25000'>{0}</span>";
		private const string m_FuelFlowMax = "<span foreground='#cccccc' size='20000'>{0}</span>";
		private const string m_PRMValue = "<span foreground='#CCCC38' size='25000'>{0}</span>";
		private const string m_PRMMax = "<span foreground='#cccccc' size='20000'>{0}</span>";
		private const string m_Par = "<span foreground='#cccccc' size='20000'>{0} </span><span foreground='#cccccc' size='40000'>{1}</span>";

		private Gdk.GC prmGC;
		private Gdk.GC flowGC;

		//Pixbuf mapContents = new Pixbuf (@"C:\Users\Mau\Desktop\Car.png");

		//int wWidth;
		//int wHeight;

		private ChartData<double> prmData = new ChartData<double> (200);
		private ChartData<double> flowData = new ChartData<double> (200);

		private string par1caption;
		private string par2caption;

		public OBDEngineAndFuel(IPageModel model, Style style, ILogger logger)
		{
			this.Build();
		
			//wWidth = mapContents.Width; wHeight = mapContents.Height;
			//d_chart.SetSizeRequest (wWidth, wHeight);

			d_chart.ExposeEvent += ChartExposeEvent;

			style.Window.Apply(eventbox_flow);
			style.Window.Apply(eventbox_prm);
			style.Window.Apply(eventbox_par1);
			style.Window.Apply(eventbox_par2);
			style.Window.Apply(eventbox_flow_max);
			style.Window.Apply(eventbox_prm_max);

			var binder = new ModelBinder (model, logger);

			progressbar_prm.ModifyFg(StateType.Prelight, new Color(80,80,0));
			progressbar_prm.ModifyBg(StateType.Prelight, new Color(80,80,0));

			binder.BindCustomAction<double>(v => 
				{
				flowData.AddPoint(v);
				var max = Math.Max(v, flowData.MaxValue);
				progressbar_flow.Fraction = max > 0 ? (v / max) : 0;

					label_flow.Markup = CommonBindings.CreateMarkup(m_FuelFlowValue, v.ToString("0.0"));
				label_flow_max.Markup = CommonBindings.CreateMarkup(m_FuelFlowMax, max.ToString("0.0"));
				}, "flow");

			binder.BindCustomAction<double>(v => 
			{
				prmData.AddPoint(v);
				var maxPRM = Math.Max(v, prmData.MaxValue);
				progressbar_prm.Fraction = maxPRM > 0 ? (v / maxPRM) : 0;

				label_prm.Markup = CommonBindings.CreateMarkup(m_PRMValue, v.ToString("0"));
				label_prm_max.Markup = CommonBindings.CreateMarkup(m_PRMMax, maxPRM.ToString("0"));
			}, "rpm");

			binder.BindCustomAction<string>(par1caption => this.par1caption = par1caption, "par1caption");
			binder.BindCustomAction<string>(par1 => label_par1.Markup = CommonBindings.CreateMarkup(m_Par, par1caption, par1), "par1");

			binder.BindCustomAction<string>(par2caption => this.par2caption = par2caption, "par2caption");
			binder.BindCustomAction<string>(par2 => label_par2.Markup = CommonBindings.CreateMarkup(m_Par, par2caption, par2), "par2");

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

		void ChartExposeEvent (object o, Gtk.ExposeEventArgs _args)
		{
			if (prmGC == null)
			{
				prmGC = new Gdk.GC(_args.Event.Window);
				prmGC.RgbFgColor = new Color (220, 190, 55);
				prmGC.SetLineAttributes(6, LineStyle.Solid, CapStyle.Butt, JoinStyle.Bevel);
			}

			DrawChart(prmData, _args.Event, prmGC);

			if (flowGC == null)
			{
				flowGC = new Gdk.GC(_args.Event.Window);
				flowGC.RgbFgColor = new Color (120, 80, 255);
				flowGC.SetLineAttributes(2, LineStyle.Solid, CapStyle.Butt, JoinStyle.Bevel);
			}

			DrawChart(flowData, _args.Event, flowGC);

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

