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

		//Pixbuf mapContents = new Pixbuf (@"C:\Users\Mau\Desktop\Car.png");

		//int wWidth;
		//int wHeight;

        private DoubleChartData prmData = new DoubleChartData(20, new TimeSpan(0, 0, 1));
        private double maxFlow = 1;

		public OBDEngineAndFuel(IPageModel model, Style style, ILogger logger)
		{
			this.Build();
		
			//wWidth = mapContents.Width; wHeight = mapContents.Height;
			//d_chart.SetSizeRequest (wWidth, wHeight);

			d_chart.ExposeEvent += ChartExposeEvent;

			style.Window.Apply(eventbox_flow);
			style.Window.Apply(eventbox_prm);
			style.Window.Apply(eventbox_flow_max);
			style.Window.Apply(eventbox_prm_max);

			var binder = new ModelBinder (model, logger);

			progressbar_prm.ModifyFg(StateType.Prelight, new Color(80,80,0));
			progressbar_prm.ModifyBg(StateType.Prelight, new Color(80,80,0));

			binder.BindCustomAction<double>(v => 
				{
					maxFlow = Math.Max(v, maxFlow);
					progressbar_flow.Fraction = v / (maxFlow * 1.2);
					label_flow.Markup = CommonBindings.CreateMarkup(m_FuelFlowValue, v.ToString("0.0"));
				label_flow_max.Markup = CommonBindings.CreateMarkup(m_FuelFlowMax, maxFlow.ToString("0.0"));
				}, "flow");

			binder.BindCustomAction<double>(v => 
			{
				prmData.AddPoint(v);
				var maxPRM = Math.Max(v, prmData.GetMaxValue());
				progressbar_prm.Fraction = maxPRM > 0 ? (v / maxPRM) : 0;

				label_prm.Markup = CommonBindings.CreateMarkup(m_PRMValue, v.ToString("0"));
				label_prm_max.Markup = CommonBindings.CreateMarkup(m_PRMMax, maxPRM.ToString("0"));

					d_chart.QueueDraw();
			}, "prm");

            binder.UpdateBindings();
		}

		void ChartExposeEvent (object o, Gtk.ExposeEventArgs _args)
		{
			_args.Event.Window.DrawLine(Style.BlackGC, 0, 0, 100, 100);

            var s = new Gtk.Style();
            

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

	internal class ChartData<T>
		where T: IComparable<T>
	{
        private readonly int maxPointsCount;
        private readonly Queue<T> points;

        public ChartData(int pointsCount)
        {
            this.maxPointsCount = pointsCount;
            this.points = new Queue<T>(pointsCount);
        }

		public virtual T GetMaxValue()
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
            if (points.Count == maxPointsCount)
                points.Dequeue();
            points.Enqueue(point);
        }

        public IEnumerable<T> GetPoints()
        {
            return points;
        }
	}

    internal class DoubleChartData : ChartData<double>
    {
        private readonly TimeSpan interval;

        private double tempValue;
        private double valuesCount;
        private DateTime timeToAddValue;

        public DoubleChartData(int pointsCount, TimeSpan interval)
            :base(pointsCount)
        {
            this.interval = interval;
        }

        public override double GetMaxValue()
        {
            var max = base.GetMaxValue();

			if (Double.IsNaN (max))
				max = 0;

			return valuesCount > 0 ? Math.Max (max, tempValue / valuesCount) : max;
        }

        public override void AddPoint(double point)
        {
			tempValue += point;
			valuesCount++;

			var now = DateTime.Now;

			if (now > timeToAddValue)
            {
				timeToAddValue = now + interval;
				base.AddPoint(tempValue / valuesCount);
                tempValue = 0;
                valuesCount = 0;
            } 
        }
    }
}

