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
		private const string m_PrimaryValue = "<span {0} size='25000'>{1}</span>";
		private const string m_PrimaryMax = "<span {0} size='20000'>{1}</span>";
		private const string m_Par = "<span foreground='#cccccc' size='20000'>{0} </span><span foreground='#cccccc' size='40000'>{1}</span><span foreground='#cccccc' size='20000'> {2}</span>";

		private Gdk.GC chartGC;
		private Gdk.GC primary1GC;
		private Gdk.GC primary2GC;
		private Gdk.GC primary3GC;

        private IChart<double> primary1;
        private IChart<double> primary2;
        private IChart<double> primary3;

        private IChart<double> secondary1;
        private IChart<double> secondary2;
        private IChart<double> secondary3;

        private const int chartDrawPointsCount = 100;

		public OBDEngineAndFuel(IPageModel model, Style style, ILogger logger)
		{
			this.Build();

            primary1 = model.GetProperty<IChart<double>>("primary1");
            primary2 = model.GetProperty<IChart<double>>("primary2");
            primary3 = model.GetProperty<IChart<double>>("primary3");

            secondary1 = model.GetProperty<IChart<double>>("secondary1");
            secondary2 = model.GetProperty<IChart<double>>("secondary2");
            secondary3 = model.GetProperty<IChart<double>>("secondary3");

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

            binder.BindCustomAction<object>(Refresh, "refresh");

			d_chart.ModifyBg(StateType.Normal, style.Window.Bg);

            binder.UpdateBindings();
		}

        private void Refresh(object arg)
        {
			UpdatePrimaryBar(progressbar_primary1, label_primary1, label_primary1_max, primary1, CommonBindings.m_FG_RED);
			UpdatePrimaryBar(progressbar_primary2, label_primary2, label_primary2_max, primary2, CommonBindings.m_FG_BLUE);

            d_chart.QueueDraw();

            UpdateSecondary(label_secondary1, secondary1);
            UpdateSecondary(label_secondary2, secondary2);
            UpdateSecondary(label_secondary3, secondary3);
        }

		private void UpdatePrimaryBar(ProgressBar bar, Label label, Label label_max, IChart<double> chart, string color)
		{
			if (chart != null)
			{
				bar.Fraction = chart.Max > 0 ? (chart.Last / chart.Max) : 0;
				label.Markup = CommonBindings.CreateMarkup(m_PrimaryValue, color, chart.Last.ToString("0"));
				label_max.Markup = CommonBindings.CreateMarkup(m_PrimaryMax, color, chart.Max.ToString("0"));
			}
		}

        private void UpdateSecondary(Label label, IChart<double> chart)
        {
			if (chart != null)
			{
            	label.Markup = CommonBindings.CreateMarkup(m_Par, chart.Title, chart.Last.ToString("0.##"), chart.UnitText);
			}
        }

		private void DrawChart(IChart<double> chart, Gdk.EventExpose e, Gdk.GC gc)
		{
			if (chart != null && chart.Count > 1)
			{
				double pxPerValueByY = (double)e.Area.Height / chart.Scale;

                int x = 0;
                int y = 0;
                int count = 0;

                Action<double> visitor = p =>
                {
                    int xx = (int)((double)e.Area.Width / ((double)chartDrawPointsCount - 1d) * count);
                    int yy = (int)(p * pxPerValueByY);

                    if (count > 0)
                        e.Window.DrawLine(gc, x, e.Area.Height - y, xx, e.Area.Height - yy);

                    x = xx;
                    y = yy;
                    count++;
                };

				chart.Visit(visitor, chartDrawPointsCount);
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
				primary1GC.RgbFgColor = new Color (255, 50, 50);
				primary1GC.SetLineAttributes(4, LineStyle.Solid, CapStyle.Butt, JoinStyle.Bevel);
			}

			if (primary2GC == null)
			{
				primary2GC = new Gdk.GC(_args.Event.Window);
				primary2GC.RgbFgColor = new Color (50, 50, 255);
				primary2GC.SetLineAttributes(4, LineStyle.Solid, CapStyle.Butt, JoinStyle.Bevel);
			}

			if (primary3GC == null)
			{
				primary3GC = new Gdk.GC(_args.Event.Window);
				primary3GC.RgbFgColor = new Color (255, 255, 30);
				primary3GC.SetLineAttributes(4, LineStyle.Solid, CapStyle.Butt, JoinStyle.Bevel);
			}

            DrawChart(primary1, _args.Event, primary1GC);
            DrawChart(primary2, _args.Event, primary2GC);
			DrawChart(primary3, _args.Event, primary3GC);

			DrawChartTable(_args.Event, chartGC);

			_args.RetVal = true;
		}
	}

}

