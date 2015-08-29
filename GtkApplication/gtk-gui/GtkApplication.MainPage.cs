
// This file has been generated by the GUI designer. Do not modify.
namespace GtkApplication
{
	public partial class MainPage
	{
		private global::Gtk.VBox base_vbox;
		private global::Gtk.HBox upper_buttons_hbox;
		private global::Gtk.EventBox box_F1;
		private global::Gtk.EventBox box_F2;
		private global::Gtk.EventBox box_F3;
		private global::Gtk.EventBox box_F4;
		private global::Gtk.EventBox box_Accept;
		private global::Gtk.HBox bottom_buttons_hbox;
		private global::Gtk.EventBox box_F5;
		private global::Gtk.EventBox box_F6;
		private global::Gtk.EventBox box_F7;
		private global::Gtk.EventBox box_F8;
		private global::Gtk.EventBox box_Cancel;
		private global::Gtk.HBox hbox2;
		private global::Gtk.EventBox eventbox_inet_status;
		private global::Gtk.Label label_inet_status;
		private global::Gtk.EventBox eventbox2;
		private global::Gtk.Label label2;
		private global::Gtk.EventBox eventbox3;
		private global::Gtk.Label label3;
		private global::Gtk.HBox hbox1;
		private global::Gtk.VBox vbox1;
		private global::Gtk.EventBox eventbox_arduino_metrics_caption;
		private global::Gtk.Label label_arduino_metrics_caption;
		private global::Gtk.EventBox eventbox_arduino_metrics;
		private global::Gtk.Label label_arduino_metrics;
		private global::Gtk.VBox vbox2;
		private global::Gtk.EventBox eventbox_gps_metrics_caption;
		private global::Gtk.Label label_gps_metrics_caption;
		private global::Gtk.EventBox eventbox_gps_metrics;
		private global::Gtk.Label label_gps_metrics;
		private global::Gtk.VBox vbox3;
		private global::Gtk.EventBox eventbox_travel_caption;
		private global::Gtk.Label label_travel_caption;
		private global::Gtk.EventBox eventbox_travel_metrics;
		private global::Gtk.Label label_travel_metrics;
		private global::Gtk.EventBox eventbox_time;
		private global::Gtk.Label label_time;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget GtkApplication.MainPage
			global::Stetic.BinContainer.Attach (this);
			this.WidthRequest = 800;
			this.HeightRequest = 480;
			this.Name = "GtkApplication.MainPage";
			// Container child GtkApplication.MainPage.Gtk.Container+ContainerChild
			this.base_vbox = new global::Gtk.VBox ();
			this.base_vbox.Name = "base_vbox";
			this.base_vbox.Spacing = 6;
			// Container child base_vbox.Gtk.Box+BoxChild
			this.upper_buttons_hbox = new global::Gtk.HBox ();
			this.upper_buttons_hbox.HeightRequest = 120;
			this.upper_buttons_hbox.Name = "upper_buttons_hbox";
			this.upper_buttons_hbox.Homogeneous = true;
			this.upper_buttons_hbox.Spacing = 6;
			this.upper_buttons_hbox.BorderWidth = ((uint)(10));
			// Container child upper_buttons_hbox.Gtk.Box+BoxChild
			this.box_F1 = new global::Gtk.EventBox ();
			this.box_F1.Name = "box_F1";
			this.upper_buttons_hbox.Add (this.box_F1);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.upper_buttons_hbox [this.box_F1]));
			w1.Position = 0;
			// Container child upper_buttons_hbox.Gtk.Box+BoxChild
			this.box_F2 = new global::Gtk.EventBox ();
			this.box_F2.Name = "box_F2";
			this.upper_buttons_hbox.Add (this.box_F2);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.upper_buttons_hbox [this.box_F2]));
			w2.Position = 1;
			// Container child upper_buttons_hbox.Gtk.Box+BoxChild
			this.box_F3 = new global::Gtk.EventBox ();
			this.box_F3.Name = "box_F3";
			this.upper_buttons_hbox.Add (this.box_F3);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.upper_buttons_hbox [this.box_F3]));
			w3.Position = 2;
			// Container child upper_buttons_hbox.Gtk.Box+BoxChild
			this.box_F4 = new global::Gtk.EventBox ();
			this.box_F4.Name = "box_F4";
			this.upper_buttons_hbox.Add (this.box_F4);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.upper_buttons_hbox [this.box_F4]));
			w4.Position = 3;
			// Container child upper_buttons_hbox.Gtk.Box+BoxChild
			this.box_Accept = new global::Gtk.EventBox ();
			this.box_Accept.Name = "box_Accept";
			this.upper_buttons_hbox.Add (this.box_Accept);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.upper_buttons_hbox [this.box_Accept]));
			w5.Position = 4;
			this.base_vbox.Add (this.upper_buttons_hbox);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.base_vbox [this.upper_buttons_hbox]));
			w6.Position = 0;
			w6.Expand = false;
			// Container child base_vbox.Gtk.Box+BoxChild
			this.bottom_buttons_hbox = new global::Gtk.HBox ();
			this.bottom_buttons_hbox.HeightRequest = 120;
			this.bottom_buttons_hbox.Name = "bottom_buttons_hbox";
			this.bottom_buttons_hbox.Homogeneous = true;
			this.bottom_buttons_hbox.Spacing = 6;
			this.bottom_buttons_hbox.BorderWidth = ((uint)(10));
			// Container child bottom_buttons_hbox.Gtk.Box+BoxChild
			this.box_F5 = new global::Gtk.EventBox ();
			this.box_F5.Name = "box_F5";
			this.bottom_buttons_hbox.Add (this.box_F5);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.bottom_buttons_hbox [this.box_F5]));
			w7.Position = 0;
			// Container child bottom_buttons_hbox.Gtk.Box+BoxChild
			this.box_F6 = new global::Gtk.EventBox ();
			this.box_F6.Name = "box_F6";
			this.bottom_buttons_hbox.Add (this.box_F6);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.bottom_buttons_hbox [this.box_F6]));
			w8.Position = 1;
			// Container child bottom_buttons_hbox.Gtk.Box+BoxChild
			this.box_F7 = new global::Gtk.EventBox ();
			this.box_F7.Name = "box_F7";
			this.bottom_buttons_hbox.Add (this.box_F7);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.bottom_buttons_hbox [this.box_F7]));
			w9.Position = 2;
			// Container child bottom_buttons_hbox.Gtk.Box+BoxChild
			this.box_F8 = new global::Gtk.EventBox ();
			this.box_F8.Name = "box_F8";
			this.bottom_buttons_hbox.Add (this.box_F8);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.bottom_buttons_hbox [this.box_F8]));
			w10.Position = 3;
			// Container child bottom_buttons_hbox.Gtk.Box+BoxChild
			this.box_Cancel = new global::Gtk.EventBox ();
			this.box_Cancel.Name = "box_Cancel";
			this.bottom_buttons_hbox.Add (this.box_Cancel);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.bottom_buttons_hbox [this.box_Cancel]));
			w11.Position = 4;
			this.base_vbox.Add (this.bottom_buttons_hbox);
			global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.base_vbox [this.bottom_buttons_hbox]));
			w12.Position = 1;
			w12.Expand = false;
			// Container child base_vbox.Gtk.Box+BoxChild
			this.hbox2 = new global::Gtk.HBox ();
			this.hbox2.Name = "hbox2";
			this.hbox2.Spacing = 6;
			this.hbox2.BorderWidth = ((uint)(10));
			// Container child hbox2.Gtk.Box+BoxChild
			this.eventbox_inet_status = new global::Gtk.EventBox ();
			this.eventbox_inet_status.HeightRequest = 30;
			this.eventbox_inet_status.Name = "eventbox_inet_status";
			// Container child eventbox_inet_status.Gtk.Container+ContainerChild
			this.label_inet_status = new global::Gtk.Label ();
			this.label_inet_status.Name = "label_inet_status";
			this.eventbox_inet_status.Add (this.label_inet_status);
			this.hbox2.Add (this.eventbox_inet_status);
			global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.eventbox_inet_status]));
			w14.Position = 0;
			// Container child hbox2.Gtk.Box+BoxChild
			this.eventbox2 = new global::Gtk.EventBox ();
			this.eventbox2.Name = "eventbox2";
			// Container child eventbox2.Gtk.Container+ContainerChild
			this.label2 = new global::Gtk.Label ();
			this.label2.Name = "label2";
			this.eventbox2.Add (this.label2);
			this.hbox2.Add (this.eventbox2);
			global::Gtk.Box.BoxChild w16 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.eventbox2]));
			w16.Position = 1;
			// Container child hbox2.Gtk.Box+BoxChild
			this.eventbox3 = new global::Gtk.EventBox ();
			this.eventbox3.Name = "eventbox3";
			// Container child eventbox3.Gtk.Container+ContainerChild
			this.label3 = new global::Gtk.Label ();
			this.label3.Name = "label3";
			this.eventbox3.Add (this.label3);
			this.hbox2.Add (this.eventbox3);
			global::Gtk.Box.BoxChild w18 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.eventbox3]));
			w18.Position = 2;
			this.base_vbox.Add (this.hbox2);
			global::Gtk.Box.BoxChild w19 = ((global::Gtk.Box.BoxChild)(this.base_vbox [this.hbox2]));
			w19.Position = 2;
			w19.Expand = false;
			w19.Fill = false;
			// Container child base_vbox.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox ();
			this.hbox1.HeightRequest = 0;
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			this.hbox1.BorderWidth = ((uint)(10));
			// Container child hbox1.Gtk.Box+BoxChild
			this.vbox1 = new global::Gtk.VBox ();
			this.vbox1.WidthRequest = 160;
			this.vbox1.HeightRequest = 10;
			this.vbox1.Name = "vbox1";
			this.vbox1.Spacing = 6;
			this.vbox1.BorderWidth = ((uint)(6));
			// Container child vbox1.Gtk.Box+BoxChild
			this.eventbox_arduino_metrics_caption = new global::Gtk.EventBox ();
			this.eventbox_arduino_metrics_caption.Name = "eventbox_arduino_metrics_caption";
			// Container child eventbox_arduino_metrics_caption.Gtk.Container+ContainerChild
			this.label_arduino_metrics_caption = new global::Gtk.Label ();
			this.label_arduino_metrics_caption.Name = "label_arduino_metrics_caption";
			this.label_arduino_metrics_caption.Xalign = 1F;
			this.label_arduino_metrics_caption.LabelProp = global::Mono.Unix.Catalog.GetString ("Arduino communication:");
			this.eventbox_arduino_metrics_caption.Add (this.label_arduino_metrics_caption);
			this.vbox1.Add (this.eventbox_arduino_metrics_caption);
			global::Gtk.Box.BoxChild w21 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.eventbox_arduino_metrics_caption]));
			w21.Position = 0;
			w21.Expand = false;
			w21.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.eventbox_arduino_metrics = new global::Gtk.EventBox ();
			this.eventbox_arduino_metrics.Name = "eventbox_arduino_metrics";
			// Container child eventbox_arduino_metrics.Gtk.Container+ContainerChild
			this.label_arduino_metrics = new global::Gtk.Label ();
			this.label_arduino_metrics.HeightRequest = 100;
			this.label_arduino_metrics.Name = "label_arduino_metrics";
			this.label_arduino_metrics.Xalign = 0.06F;
			this.label_arduino_metrics.Yalign = 0.06F;
			this.eventbox_arduino_metrics.Add (this.label_arduino_metrics);
			this.vbox1.Add (this.eventbox_arduino_metrics);
			global::Gtk.Box.BoxChild w23 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.eventbox_arduino_metrics]));
			w23.Position = 1;
			w23.Expand = false;
			w23.Fill = false;
			this.hbox1.Add (this.vbox1);
			global::Gtk.Box.BoxChild w24 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.vbox1]));
			w24.Position = 0;
			w24.Expand = false;
			w24.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.vbox2 = new global::Gtk.VBox ();
			this.vbox2.WidthRequest = 160;
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			this.vbox2.BorderWidth = ((uint)(6));
			// Container child vbox2.Gtk.Box+BoxChild
			this.eventbox_gps_metrics_caption = new global::Gtk.EventBox ();
			this.eventbox_gps_metrics_caption.Name = "eventbox_gps_metrics_caption";
			// Container child eventbox_gps_metrics_caption.Gtk.Container+ContainerChild
			this.label_gps_metrics_caption = new global::Gtk.Label ();
			this.label_gps_metrics_caption.Name = "label_gps_metrics_caption";
			this.label_gps_metrics_caption.LabelProp = global::Mono.Unix.Catalog.GetString ("GPS controller:");
			this.eventbox_gps_metrics_caption.Add (this.label_gps_metrics_caption);
			this.vbox2.Add (this.eventbox_gps_metrics_caption);
			global::Gtk.Box.BoxChild w26 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.eventbox_gps_metrics_caption]));
			w26.Position = 0;
			w26.Expand = false;
			w26.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.eventbox_gps_metrics = new global::Gtk.EventBox ();
			this.eventbox_gps_metrics.Name = "eventbox_gps_metrics";
			// Container child eventbox_gps_metrics.Gtk.Container+ContainerChild
			this.label_gps_metrics = new global::Gtk.Label ();
			this.label_gps_metrics.HeightRequest = 100;
			this.label_gps_metrics.Name = "label_gps_metrics";
			this.label_gps_metrics.Xalign = 0.06F;
			this.label_gps_metrics.Yalign = 0.06F;
			this.eventbox_gps_metrics.Add (this.label_gps_metrics);
			this.vbox2.Add (this.eventbox_gps_metrics);
			global::Gtk.Box.BoxChild w28 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.eventbox_gps_metrics]));
			w28.Position = 1;
			w28.Expand = false;
			w28.Fill = false;
			this.hbox1.Add (this.vbox2);
			global::Gtk.Box.BoxChild w29 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.vbox2]));
			w29.Position = 1;
			w29.Expand = false;
			w29.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.vbox3 = new global::Gtk.VBox ();
			this.vbox3.WidthRequest = 160;
			this.vbox3.Name = "vbox3";
			this.vbox3.Spacing = 6;
			this.vbox3.BorderWidth = ((uint)(6));
			// Container child vbox3.Gtk.Box+BoxChild
			this.eventbox_travel_caption = new global::Gtk.EventBox ();
			this.eventbox_travel_caption.Name = "eventbox_travel_caption";
			// Container child eventbox_travel_caption.Gtk.Container+ContainerChild
			this.label_travel_caption = new global::Gtk.Label ();
			this.label_travel_caption.Name = "label_travel_caption";
			this.label_travel_caption.LabelProp = global::Mono.Unix.Catalog.GetString ("Travel:");
			this.eventbox_travel_caption.Add (this.label_travel_caption);
			this.vbox3.Add (this.eventbox_travel_caption);
			global::Gtk.Box.BoxChild w31 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.eventbox_travel_caption]));
			w31.Position = 0;
			w31.Expand = false;
			w31.Fill = false;
			// Container child vbox3.Gtk.Box+BoxChild
			this.eventbox_travel_metrics = new global::Gtk.EventBox ();
			this.eventbox_travel_metrics.Name = "eventbox_travel_metrics";
			// Container child eventbox_travel_metrics.Gtk.Container+ContainerChild
			this.label_travel_metrics = new global::Gtk.Label ();
			this.label_travel_metrics.HeightRequest = 100;
			this.label_travel_metrics.Name = "label_travel_metrics";
			this.label_travel_metrics.Xalign = 0.06F;
			this.label_travel_metrics.Yalign = 0.06F;
			this.eventbox_travel_metrics.Add (this.label_travel_metrics);
			this.vbox3.Add (this.eventbox_travel_metrics);
			global::Gtk.Box.BoxChild w33 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.eventbox_travel_metrics]));
			w33.Position = 1;
			w33.Expand = false;
			w33.Fill = false;
			this.hbox1.Add (this.vbox3);
			global::Gtk.Box.BoxChild w34 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.vbox3]));
			w34.Position = 2;
			w34.Expand = false;
			w34.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.eventbox_time = new global::Gtk.EventBox ();
			this.eventbox_time.Name = "eventbox_time";
			// Container child eventbox_time.Gtk.Container+ContainerChild
			this.label_time = new global::Gtk.Label ();
			this.label_time.Name = "label_time";
			this.label_time.Yalign = 0F;
			this.label_time.LabelProp = global::Mono.Unix.Catalog.GetString ("--:--:--");
			this.eventbox_time.Add (this.label_time);
			this.hbox1.Add (this.eventbox_time);
			global::Gtk.Box.BoxChild w36 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.eventbox_time]));
			w36.Position = 3;
			this.base_vbox.Add (this.hbox1);
			global::Gtk.Box.BoxChild w37 = ((global::Gtk.Box.BoxChild)(this.base_vbox [this.hbox1]));
			w37.Position = 3;
			this.Add (this.base_vbox);
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.Hide ();
		}
	}
}
