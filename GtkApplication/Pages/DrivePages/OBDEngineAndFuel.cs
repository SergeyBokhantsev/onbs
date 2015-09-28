using System;
using Interfaces.UI;
using Interfaces;
using Gtk;
using Gdk;
using GtkApplication.Pages;

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

		private double maxFlow = 1;
		private double maxPRM = 1;

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
					progressbar_flow.Fraction = v / maxFlow;
					label_flow.Markup = CommonBindings.CreateMarkup(m_FuelFlowValue, v.ToString("0.0"));
				label_flow_max.Markup = CommonBindings.CreateMarkup(m_FuelFlowMax, maxFlow.ToString("0.0"));
				}, "flow");

			binder.BindCustomAction<double>(v => 
			{
				maxPRM = Math.Max(v, maxPRM);
				progressbar_prm.Fraction = v / maxPRM;
				label_prm.Markup = CommonBindings.CreateMarkup(m_PRMValue, v.ToString("0"));
				label_prm_max.Markup = CommonBindings.CreateMarkup(m_PRMMax, maxPRM.ToString("0.0")); 
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
}

