
// This file has been generated by the GUI designer. Do not modify.
namespace GtkApplication
{
	public partial class TrafficPage
	{
		private global::Gtk.VBox vbox1;
		
		private global::Gtk.HBox hbox1;
		
		private global::Gtk.Label label15;
		
		private global::Gtk.HBox hbox2;
		
		private global::Gtk.Label label_arduino_status;
		
		private global::Gtk.Label label_gps_status;
		
		private global::Gtk.Label label_inet_status;
		
		private global::Gtk.Label label_time;
		
		private global::Gtk.HSeparator hseparator2;
		
		private global::Gtk.Image image_traffic;
		
		private global::Gtk.HSeparator hseparator1;
		
		private global::Gtk.HBox hbox5;
		
		private global::Gtk.EventBox eventbox_drive;
		
		private global::Gtk.Label label5;
		
		private global::Gtk.EventBox eventbox_nav;
		
		private global::Gtk.Label label6;
		
		private global::Gtk.EventBox eventbox_cam;
		
		private global::Gtk.Label label7;
		
		private global::Gtk.EventBox eventbox_weather;
		
		private global::Gtk.Label label8;
		
		private global::Gtk.EventBox eventbox_traffic;
		
		private global::Gtk.Label label9;
		
		private global::Gtk.EventBox eventbox_options;
		
		private global::Gtk.Label label_options;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget GtkApplication.TrafficPage
			global::Stetic.BinContainer.Attach (this);
			this.Name = "GtkApplication.TrafficPage";
			// Container child GtkApplication.TrafficPage.Gtk.Container+ContainerChild
			this.vbox1 = new global::Gtk.VBox ();
			this.vbox1.Name = "vbox1";
			this.vbox1.Spacing = 6;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox ();
			this.hbox1.Name = "hbox1";
			this.hbox1.Homogeneous = true;
			this.hbox1.Spacing = 6;
			this.hbox1.BorderWidth = ((uint)(5));
			// Container child hbox1.Gtk.Box+BoxChild
			this.label15 = new global::Gtk.Label ();
			this.label15.Name = "label15";
			this.hbox1.Add (this.label15);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.label15]));
			w1.Position = 0;
			// Container child hbox1.Gtk.Box+BoxChild
			this.hbox2 = new global::Gtk.HBox ();
			this.hbox2.Name = "hbox2";
			this.hbox2.Homogeneous = true;
			this.hbox2.Spacing = 6;
			// Container child hbox2.Gtk.Box+BoxChild
			this.label_arduino_status = new global::Gtk.Label ();
			this.label_arduino_status.Name = "label_arduino_status";
			this.label_arduino_status.LabelProp = global::Mono.Unix.Catalog.GetString ("<span foreground=\'#205768\' size=\'12000\'>ARD</span>");
			this.label_arduino_status.UseMarkup = true;
			this.hbox2.Add (this.label_arduino_status);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.label_arduino_status]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			// Container child hbox2.Gtk.Box+BoxChild
			this.label_gps_status = new global::Gtk.Label ();
			this.label_gps_status.Name = "label_gps_status";
			this.label_gps_status.LabelProp = global::Mono.Unix.Catalog.GetString ("<span foreground=\'#205768\' size=\'12000\'>  GPS  </span>");
			this.label_gps_status.UseMarkup = true;
			this.hbox2.Add (this.label_gps_status);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.label_gps_status]));
			w3.Position = 1;
			w3.Expand = false;
			w3.Fill = false;
			// Container child hbox2.Gtk.Box+BoxChild
			this.label_inet_status = new global::Gtk.Label ();
			this.label_inet_status.Name = "label_inet_status";
			this.label_inet_status.LabelProp = global::Mono.Unix.Catalog.GetString ("<span foreground=\'#205768\' size=\'12000\'>INET</span>");
			this.label_inet_status.UseMarkup = true;
			this.hbox2.Add (this.label_inet_status);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.label_inet_status]));
			w4.Position = 2;
			w4.Expand = false;
			w4.Fill = false;
			this.hbox1.Add (this.hbox2);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.hbox2]));
			w5.Position = 1;
			w5.Expand = false;
			w5.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.label_time = new global::Gtk.Label ();
			this.label_time.Name = "label_time";
			this.label_time.Xalign = 1F;
			this.label_time.LabelProp = "<span foreground=\'#d6d6d6\' size=\'18000\'>--:--</span><span size=\'10000\'> : --</spa" +
			"n>";
			this.label_time.UseMarkup = true;
			this.hbox1.Add (this.label_time);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.label_time]));
			w6.PackType = ((global::Gtk.PackType)(1));
			w6.Position = 2;
			this.vbox1.Add (this.hbox1);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.hbox1]));
			w7.Position = 0;
			w7.Expand = false;
			w7.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hseparator2 = new global::Gtk.HSeparator ();
			this.hseparator2.Name = "hseparator2";
			this.vbox1.Add (this.hseparator2);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.hseparator2]));
			w8.Position = 1;
			w8.Expand = false;
			w8.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.image_traffic = new global::Gtk.Image ();
			this.image_traffic.Name = "image_traffic";
			this.vbox1.Add (this.image_traffic);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.image_traffic]));
			w9.Position = 2;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hseparator1 = new global::Gtk.HSeparator ();
			this.hseparator1.Name = "hseparator1";
			this.vbox1.Add (this.hseparator1);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.hseparator1]));
			w10.Position = 3;
			w10.Expand = false;
			w10.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hbox5 = new global::Gtk.HBox ();
			this.hbox5.Name = "hbox5";
			this.hbox5.Homogeneous = true;
			this.hbox5.Spacing = 6;
			this.hbox5.BorderWidth = ((uint)(10));
			// Container child hbox5.Gtk.Box+BoxChild
			this.eventbox_drive = new global::Gtk.EventBox ();
			this.eventbox_drive.Name = "eventbox_drive";
			// Container child eventbox_drive.Gtk.Container+ContainerChild
			this.label5 = new global::Gtk.Label ();
			this.label5.Name = "label5";
			this.label5.LabelProp = global::Mono.Unix.Catalog.GetString ("<span foreground=\'#606060\' size=\'15000\'>DRIVE</span>");
			this.label5.UseMarkup = true;
			this.eventbox_drive.Add (this.label5);
			this.hbox5.Add (this.eventbox_drive);
			global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.hbox5 [this.eventbox_drive]));
			w12.Position = 0;
			// Container child hbox5.Gtk.Box+BoxChild
			this.eventbox_nav = new global::Gtk.EventBox ();
			this.eventbox_nav.Name = "eventbox_nav";
			// Container child eventbox_nav.Gtk.Container+ContainerChild
			this.label6 = new global::Gtk.Label ();
			this.label6.Name = "label6";
			this.label6.LabelProp = global::Mono.Unix.Catalog.GetString ("<span foreground=\'#606060\' size=\'15000\'>NAV</span>");
			this.label6.UseMarkup = true;
			this.eventbox_nav.Add (this.label6);
			this.hbox5.Add (this.eventbox_nav);
			global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.hbox5 [this.eventbox_nav]));
			w14.Position = 1;
			// Container child hbox5.Gtk.Box+BoxChild
			this.eventbox_cam = new global::Gtk.EventBox ();
			this.eventbox_cam.Name = "eventbox_cam";
			// Container child eventbox_cam.Gtk.Container+ContainerChild
			this.label7 = new global::Gtk.Label ();
			this.label7.Name = "label7";
			this.label7.LabelProp = global::Mono.Unix.Catalog.GetString ("<span foreground=\'#606060\' size=\'15000\'>CAM</span>");
			this.label7.UseMarkup = true;
			this.eventbox_cam.Add (this.label7);
			this.hbox5.Add (this.eventbox_cam);
			global::Gtk.Box.BoxChild w16 = ((global::Gtk.Box.BoxChild)(this.hbox5 [this.eventbox_cam]));
			w16.Position = 2;
			// Container child hbox5.Gtk.Box+BoxChild
			this.eventbox_weather = new global::Gtk.EventBox ();
			this.eventbox_weather.Name = "eventbox_weather";
			// Container child eventbox_weather.Gtk.Container+ContainerChild
			this.label8 = new global::Gtk.Label ();
			this.label8.Name = "label8";
			this.label8.LabelProp = global::Mono.Unix.Catalog.GetString ("<span foreground=\'#606060\' size=\'15000\'>WTHR</span>");
			this.label8.UseMarkup = true;
			this.eventbox_weather.Add (this.label8);
			this.hbox5.Add (this.eventbox_weather);
			global::Gtk.Box.BoxChild w18 = ((global::Gtk.Box.BoxChild)(this.hbox5 [this.eventbox_weather]));
			w18.Position = 3;
			// Container child hbox5.Gtk.Box+BoxChild
			this.eventbox_traffic = new global::Gtk.EventBox ();
			this.eventbox_traffic.Name = "eventbox_traffic";
			// Container child eventbox_traffic.Gtk.Container+ContainerChild
			this.label9 = new global::Gtk.Label ();
			this.label9.Name = "label9";
			this.label9.LabelProp = global::Mono.Unix.Catalog.GetString ("<span foreground=\'#d39c2f\' size=\'15000\'><b>TRAF</b></span>");
			this.label9.UseMarkup = true;
			this.eventbox_traffic.Add (this.label9);
			this.hbox5.Add (this.eventbox_traffic);
			global::Gtk.Box.BoxChild w20 = ((global::Gtk.Box.BoxChild)(this.hbox5 [this.eventbox_traffic]));
			w20.Position = 4;
			w20.Expand = false;
			w20.Fill = false;
			// Container child hbox5.Gtk.Box+BoxChild
			this.eventbox_options = new global::Gtk.EventBox ();
			this.eventbox_options.Name = "eventbox_options";
			// Container child eventbox_options.Gtk.Container+ContainerChild
			this.label_options = new global::Gtk.Label ();
			this.label_options.Name = "label_options";
			this.label_options.LabelProp = global::Mono.Unix.Catalog.GetString ("<span foreground=\'#606060\' size=\'15000\'>OPT</span>");
			this.label_options.UseMarkup = true;
			this.eventbox_options.Add (this.label_options);
			this.hbox5.Add (this.eventbox_options);
			global::Gtk.Box.BoxChild w22 = ((global::Gtk.Box.BoxChild)(this.hbox5 [this.eventbox_options]));
			w22.Position = 5;
			this.vbox1.Add (this.hbox5);
			global::Gtk.Box.BoxChild w23 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.hbox5]));
			w23.PackType = ((global::Gtk.PackType)(1));
			w23.Position = 4;
			w23.Expand = false;
			w23.Fill = false;
			this.Add (this.vbox1);
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.Hide ();
		}
	}
}
