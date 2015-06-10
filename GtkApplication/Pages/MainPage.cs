using System;
using System.Drawing;
using Gdk;
using Gtk;
using GtkApplication.Pages;
using Interfaces;
using Interfaces.UI;
using System.Text;

namespace GtkApplication
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class MainPage : Gtk.Bin
	{
        private readonly IPageModel model;
        private readonly ModelBinder binder;
		private readonly Style style;

		public MainPage(IPageModel model, Style style, ILogger logger)
		{
			this.model = model;
			this.style = style;

			this.Build();

//			var bF1 = new FlatButton (box_F1, style.CommonButton);
//			var bF2 = new FlatButton (box_F2, style.CommonButton);
//			var bF3 = new FlatButton (box_F3, style.CommonButton);
//			var bF4 = new FlatButton (box_F4, style.CommonButton);
//			var bF5 = new FlatButton (box_F5, style.CommonButton);
//			var bF6 = new FlatButton (box_F6, style.CommonButton);
//			var bF7 = new FlatButton (box_F7, style.CommonButton);
//			var bF8 = new FlatButton (box_F8, style.CommonButton);
//			var bAccept = new FlatButton (box_Accept, style.AcceptButton);
//			var bCancel = new FlatButton (box_Cancel, style.CancelButton);
			          
			style.TextBox.Apply (label_arduino_metrics_caption, eventbox_arduino_metrics_caption);
			style.TextBox.Apply (label_arduino_metrics, eventbox_arduino_metrics);

			style.TextBox.Apply(label_gps_metrics_caption, eventbox_gps_metrics_caption);
			style.TextBox.Apply(label_gps_metrics, eventbox_gps_metrics);

            binder = new ModelBinder(model, logger);

			InitializeButton(box_Cancel, style.CancelButton, ModelNames.ButtonCancel, TextAligment.CenterMiddle);
			InitializeButton(box_Accept, style.AcceptButton, ModelNames.ButtonAccept, TextAligment.CenterMiddle);
			InitializeButton(box_F1, style.CommonButton, ModelNames.ButtonF1, TextAligment.CenterMiddle);
			InitializeButton(box_F2, style.CommonButton, ModelNames.ButtonF2, TextAligment.CenterMiddle);
			InitializeButton(box_F3, style.CommonButton, ModelNames.ButtonF3, TextAligment.CenterMiddle);
			InitializeButton(box_F4, style.CommonButton, ModelNames.ButtonF4, TextAligment.CenterMiddle);
			InitializeButton(box_F5, style.CommonButton, ModelNames.ButtonF5, TextAligment.CenterMiddle);
			InitializeButton(box_F6, style.CommonButton, ModelNames.ButtonF6, TextAligment.CenterMiddle);
			InitializeButton(box_F7, style.CommonButton, ModelNames.ButtonF7, TextAligment.CenterMiddle);
			InitializeButton(box_F8, style.CommonButton, ModelNames.ButtonF8, TextAligment.CenterMiddle);

			
//			binder.BindFlatButtonLabel(bF1, "nav_btn_caption", "F1");
//            binder.BindFlatButtonClick(bF1, "nav");
//
//            binder.BindFlatButtonLabel(bF2, "cam_btn_caption", "F2");
//            binder.BindFlatButtonClick(bF2, "cam");

            binder.BindMetrics(UpdateMetrics, "metrics");
		}

		private void InitializeButton(EventBox box, LookAndFeel lf, string buttonName, TextAligment align)
		{
			var btnLabelPropertyName = ModelNames.ResolveButtonLabelName(buttonName);

			if (!string.IsNullOrEmpty(model.GetProperty<string>(btnLabelPropertyName)))
			{
				var btn = new FlatButton (box, lf, align);

				binder.BindFlatButtonLabel(btn, btnLabelPropertyName, buttonName);
				binder.BindFlatButtonClick(btn, buttonName);
			} else
			{
				var stubLabel = new Label ();
				box.Add(stubLabel);
				style.Window.Apply(stubLabel, box);
			}
		}

        private void UpdateMetrics(IMetrics metrics)
        {
			if (metrics == null)
				return;

            switch (metrics.ProviderName)
            {
                case "Arduino Controller":
                    UpdateArduinoMetrics(metrics);
                    break;

                case "GPS Controller":
                    UpdateGPSMetrics(metrics);
                    break;
            }
        }

        private void UpdateArduinoMetrics(IMetrics metrics)
        {
			bool is_error = true;
            StringBuilder text = new StringBuilder();

            foreach (var pair in metrics)
            {
                if (pair.Key.StartsWith("_"))
                {
                    if (pair.Key == "_is_error" && pair.Value is bool && (bool)pair.Value == false)
                        is_error = false;
                }
                else
                {
                    text.Append(string.Concat(pair.Key, ": ", pair.Value, Environment.NewLine));
                }
            }

			label_arduino_metrics.Text = text.ToString().TrimEnd();
			eventbox_arduino_metrics_caption.ModifyBg (StateType.Normal, is_error ? new Gdk.Color (200, 0, 0) : style.TextBox.Bg);
        }

		private void UpdateGPSMetrics(IMetrics metrics)
		{
			bool is_error = true;
			StringBuilder text = new StringBuilder();

			foreach (var pair in metrics)
			{
				if (pair.Key.StartsWith("_"))
				{
					if (pair.Key == "_is_error" && pair.Value is bool && (bool)pair.Value == false)
						is_error = false;
				}
				else
				{
					text.Append(string.Concat(pair.Key, ": ", pair.Value, Environment.NewLine));
				}
			}

			label_gps_metrics.Text = text.ToString().TrimEnd();
			//eventbox_arduino_metrics_caption.ModifyBg (StateType.Normal, is_error ? new Gdk.Color (200, 0, 0) : style.TextBox.Bg);
		}
    }
}

