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
		private const string m_PrimaryTitle = "<span {0} size='15000'>{1}</span>";
		private const string m_PrimaryValue = "<span {0} size='25000'><b>{1}</b></span>";
		private const string m_PrimaryMax = "<span {0} size='10000'>{1}</span>";

		private const string m_Par = "<span foreground='#cccccc' size='10000'>{0} </span><span foreground='#cccccc' size='20000'>{1}</span><span foreground='#cccccc' size='10000'> {2}</span>";

		private Gdk.GC chartGC;
		private Gdk.GC primary1GC;
		private Gdk.GC primary2GC;
		private Gdk.GC primary3GC;

		private IChart<double>[] primary = new IChart<double>[3];
		private IChart<double>[] secondary = new IChart<double>[6];

        private const int chartDrawPointsCount = 100;

		public OBDEngineAndFuel(IPageModel model, Style style, ILogger logger)
		{
			this.Build();

            primary[0] = model.GetProperty<IChart<double>>("primary1");
            primary[1] = model.GetProperty<IChart<double>>("primary2");
            primary[2] = model.GetProperty<IChart<double>>("primary3");

            secondary[0] = model.GetProperty<IChart<double>>("secondary1");
            secondary[1] = model.GetProperty<IChart<double>>("secondary2");
            secondary[2] = model.GetProperty<IChart<double>>("secondary3");
			secondary[3] = model.GetProperty<IChart<double>>("secondary4");
			secondary[4] = model.GetProperty<IChart<double>>("secondary5");
			secondary[5] = model.GetProperty<IChart<double>>("secondary6");

			d_chart.ExposeEvent += ChartExposeEvent;

			style.Window.Apply(eventbox_primary_title1);
			style.Window.Apply(eventbox_primary_value1);
			style.Window.Apply(eventbox_primary_max1);

			style.Window.Apply(eventbox_primary_title2);
			style.Window.Apply(eventbox_primary_value2);
			style.Window.Apply(eventbox_primary_max2);

			style.Window.Apply(eventbox_primary_title3);
			style.Window.Apply(eventbox_primary_value3);
			style.Window.Apply(eventbox_primary_max3);

			style.CommonButton.Apply(eventbox_secondary1);
			style.AcceptButton.Apply(eventbox_secondary2);
			style.CommonButton.Apply(eventbox_secondary3);
			style.AcceptButton.Apply(eventbox_secondary4);
			style.CommonButton.Apply(eventbox_secondary5);
			style.AcceptButton.Apply(eventbox_secondary6);

			var binder = new ModelBinder (model, logger);

            binder.BindCustomAction<object>(Refresh, "refresh");

			d_chart.ModifyBg(StateType.Normal, style.Window.Bg);

			binder.BindLabelMarkup(label_primary_title1, "primary1", chart => CommonBindings.CreateMarkup(m_PrimaryTitle, CommonBindings.m_FG_RED, ((IChart<double>)chart).Title));

			binder.BindLabelMarkup(label_primary_title2, "primary2", chart => CommonBindings.CreateMarkup(m_PrimaryTitle, CommonBindings.m_FG_BLUE, ((IChart<double>)chart).Title));

			binder.BindLabelMarkup(label_primary_title3, "primary3", chart => CommonBindings.CreateMarkup(m_PrimaryTitle, CommonBindings.m_FG_YELLOW, ((IChart<double>)chart).Title));

            binder.UpdateBindings();
		}

        private void Refresh(object arg)
        {
			UpdatePrimaryBar(label_primary_value1, label_primary_max1, primary[0], CommonBindings.m_FG_RED);
			UpdatePrimaryBar(label_primary_value2, label_primary_max2, primary[1], CommonBindings.m_FG_BLUE);
			UpdatePrimaryBar(label_primary_value3, label_primary_max3, primary[2], CommonBindings.m_FG_YELLOW);

            d_chart.QueueDraw();

            UpdateSecondary(label_secondary1, secondary[0]);
            UpdateSecondary(label_secondary2, secondary[1]);
            UpdateSecondary(label_secondary3, secondary[2]);
			UpdateSecondary(label_secondary4, secondary[3]);
			UpdateSecondary(label_secondary5, secondary[4]);
			UpdateSecondary(label_secondary6, secondary[5]);
        }

		private void UpdatePrimaryBar(Label label_value, Label label_max, IChart<double> chart, string color)
		{
			if (chart != null)
			{
				label_value.Markup = CommonBindings.CreateMarkup(m_PrimaryValue, color, chart.Last.ToString("0"));
				label_max.Markup = CommonBindings.CreateMarkup(m_PrimaryMax, color, chart.Max.ToString("0"));
			}
		}

        private void UpdateSecondary(Label label, IChart<double> chart)
        {
			if (chart != null)
			{
            	label.Markup = CommonBindings.CreateMarkup(m_Par, chart.Title, chart.Last.ToString("0.##"), chart.UnitText);
				label.UseMarkup = true;
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

            DrawChart(primary[0], _args.Event, primary1GC);
            DrawChart(primary[1], _args.Event, primary2GC);
			DrawChart(primary[2], _args.Event, primary3GC);

			DrawChartTable(_args.Event, chartGC);

			_args.RetVal = true;
		}
	}

}

