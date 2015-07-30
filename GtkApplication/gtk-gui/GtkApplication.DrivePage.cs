
// This file has been generated by the GUI designer. Do not modify.
namespace GtkApplication
{
	public partial class DrivePage
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
		
		private global::Gtk.HBox hbox4;
		
		private global::Gtk.VBox vbox3;
		
		private global::Gtk.HBox hbox6;
		
		private global::Gtk.Image image_weather_icon;
		
		private global::Gtk.Label label_air_temp;
		
		private global::Gtk.HBox hbox3;
		
		private global::Gtk.HSeparator hseparator3;
		
		private global::Gtk.Label label_heading;
		
		private global::Gtk.Label label_exported_points;
		
		private global::Gtk.Label label_lon;
		
		private global::Gtk.Label label_lat;
		
		private global::Gtk.HSeparator hseparator4;
		
		private global::Gtk.Image image1;
		
		private global::Gtk.VBox vbox2;
		
		private global::Gtk.Label label_speed;
		
		private global::Gtk.Label label12;
		
		private global::Gtk.HSeparator hseparator5;
		
		private global::Gtk.Label label_travel_span;
		
		private global::Gtk.Label label_distance;
		
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
			// Widget GtkApplication.DrivePage
			global::Stetic.BinContainer.Attach (this);
			this.WidthRequest = 800;
			this.HeightRequest = 480;
			this.Name = "GtkApplication.DrivePage";
			// Container child GtkApplication.DrivePage.Gtk.Container+ContainerChild
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
			this.hbox4 = new global::Gtk.HBox ();
			this.hbox4.Name = "hbox4";
			this.hbox4.Spacing = 6;
			// Container child hbox4.Gtk.Box+BoxChild
			this.vbox3 = new global::Gtk.VBox ();
			this.vbox3.WidthRequest = 250;
			this.vbox3.Name = "vbox3";
			this.vbox3.Spacing = 6;
			this.vbox3.BorderWidth = ((uint)(20));
			// Container child vbox3.Gtk.Box+BoxChild
			this.hbox6 = new global::Gtk.HBox ();
			this.hbox6.Name = "hbox6";
			this.hbox6.Spacing = 6;
			// Container child hbox6.Gtk.Box+BoxChild
			this.image_weather_icon = new global::Gtk.Image ();
			this.image_weather_icon.Name = "image_weather_icon";
			this.hbox6.Add (this.image_weather_icon);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.hbox6 [this.image_weather_icon]));
			w9.Position = 0;
			w9.Expand = false;
			w9.Fill = false;
			w9.Padding = ((uint)(20));
			// Container child hbox6.Gtk.Box+BoxChild
			this.label_air_temp = new global::Gtk.Label ();
			this.label_air_temp.Name = "label_air_temp";
			this.label_air_temp.Yalign = 0.73F;
			this.label_air_temp.LabelProp = "<span foreground=\'#d6d6d6\' size=\'14000\'>Air temp: </span><span foreground=\'#d6d6d" +
			"6\' size=\'20000\'>--</span>";
			this.label_air_temp.UseMarkup = true;
			this.label_air_temp.Wrap = true;
			this.hbox6.Add (this.label_air_temp);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.hbox6 [this.label_air_temp]));
			w10.Position = 1;
			w10.Expand = false;
			w10.Fill = false;
			this.vbox3.Add (this.hbox6);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.hbox6]));
			w11.Position = 0;
			w11.Expand = false;
			w11.Fill = false;
			// Container child vbox3.Gtk.Box+BoxChild
			this.hbox3 = new global::Gtk.HBox ();
			this.hbox3.Name = "hbox3";
			this.hbox3.Spacing = 6;
			// Container child hbox3.Gtk.Box+BoxChild
			this.hseparator3 = new global::Gtk.HSeparator ();
			this.hseparator3.WidthRequest = 80;
			this.hseparator3.Name = "hseparator3";
			this.hbox3.Add (this.hseparator3);
			global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.hbox3 [this.hseparator3]));
			w12.Position = 1;
			w12.Fill = false;
			this.vbox3.Add (this.hbox3);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.hbox3]));
			w13.Position = 1;
			w13.Expand = false;
			w13.Fill = false;
			// Container child vbox3.Gtk.Box+BoxChild
			this.label_heading = new global::Gtk.Label ();
			this.label_heading.Name = "label_heading";
			this.label_heading.Yalign = 0.73F;
			this.label_heading.LabelProp = "<span foreground=\'#d6d6d6\' size=\'14000\'>Heading: </span><span foreground=\'#d6d6d6" +
			"\' size=\'20000\'>--</span>";
			this.label_heading.UseMarkup = true;
			this.label_heading.Wrap = true;
			this.label_heading.Justify = ((global::Gtk.Justification)(2));
			this.vbox3.Add (this.label_heading);
			global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.label_heading]));
			w14.Position = 2;
			w14.Expand = false;
			w14.Fill = false;
			// Container child vbox3.Gtk.Box+BoxChild
			this.label_exported_points = new global::Gtk.Label ();
			this.label_exported_points.Name = "label_exported_points";
			this.label_exported_points.Xalign = 0.1F;
			this.label_exported_points.Yalign = 0.73F;
			this.label_exported_points.LabelProp = "<span foreground=\'#d6d6d6\' size=\'14000\'>Lon: --</span>";
			this.label_exported_points.UseMarkup = true;
			this.vbox3.Add (this.label_exported_points);
			global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.label_exported_points]));
			w15.PackType = ((global::Gtk.PackType)(1));
			w15.Position = 3;
			w15.Expand = false;
			w15.Fill = false;
			// Container child vbox3.Gtk.Box+BoxChild
			this.label_lon = new global::Gtk.Label ();
			this.label_lon.Name = "label_lon";
			this.label_lon.Xalign = 0.1F;
			this.label_lon.Yalign = 0.73F;
			this.label_lon.LabelProp = "<span foreground=\'#d6d6d6\' size=\'14000\'>Lon: --</span>";
			this.label_lon.UseMarkup = true;
			this.vbox3.Add (this.label_lon);
			global::Gtk.Box.BoxChild w16 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.label_lon]));
			w16.PackType = ((global::Gtk.PackType)(1));
			w16.Position = 4;
			w16.Expand = false;
			w16.Fill = false;
			// Container child vbox3.Gtk.Box+BoxChild
			this.label_lat = new global::Gtk.Label ();
			this.label_lat.Name = "label_lat";
			this.label_lat.Xalign = 0.1F;
			this.label_lat.Yalign = 0.73F;
			this.label_lat.LabelProp = "<span foreground=\'#d6d6d6\' size=\'14000\'>Lat: --</span>";
			this.label_lat.UseMarkup = true;
			this.vbox3.Add (this.label_lat);
			global::Gtk.Box.BoxChild w17 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.label_lat]));
			w17.PackType = ((global::Gtk.PackType)(1));
			w17.Position = 5;
			w17.Expand = false;
			w17.Fill = false;
			// Container child vbox3.Gtk.Box+BoxChild
			this.hseparator4 = new global::Gtk.HSeparator ();
			this.hseparator4.WidthRequest = 50;
			this.hseparator4.Name = "hseparator4";
			this.vbox3.Add (this.hseparator4);
			global::Gtk.Box.BoxChild w18 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.hseparator4]));
			w18.PackType = ((global::Gtk.PackType)(1));
			w18.Position = 6;
			w18.Expand = false;
			w18.Fill = false;
			this.hbox4.Add (this.vbox3);
			global::Gtk.Box.BoxChild w19 = ((global::Gtk.Box.BoxChild)(this.hbox4 [this.vbox3]));
			w19.Position = 0;
			// Container child hbox4.Gtk.Box+BoxChild
			this.image1 = new global::Gtk.Image ();
			this.image1.Name = "image1";
			this.hbox4.Add (this.image1);
			global::Gtk.Box.BoxChild w20 = ((global::Gtk.Box.BoxChild)(this.hbox4 [this.image1]));
			w20.Position = 1;
			w20.Expand = false;
			w20.Fill = false;
			// Container child hbox4.Gtk.Box+BoxChild
			this.vbox2 = new global::Gtk.VBox ();
			this.vbox2.WidthRequest = 260;
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.label_speed = new global::Gtk.Label ();
			this.label_speed.HeightRequest = 150;
			this.label_speed.Name = "label_speed";
			this.label_speed.Yalign = 0.29F;
			this.label_speed.LabelProp = "<span foreground=\'#FFFFFF\' size=\'90000\'><b>-</b></span>";
			this.label_speed.UseMarkup = true;
			this.label_speed.Justify = ((global::Gtk.Justification)(2));
			this.vbox2.Add (this.label_speed);
			global::Gtk.Box.BoxChild w21 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.label_speed]));
			w21.Position = 0;
			w21.Expand = false;
			w21.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.label12 = new global::Gtk.Label ();
			this.label12.HeightRequest = 35;
			this.label12.Name = "label12";
			this.label12.Yalign = 1F;
			this.label12.LabelProp = "<span foreground=\'#FFFFFF\' size=\'20000\'>km/h</span>";
			this.label12.UseMarkup = true;
			this.vbox2.Add (this.label12);
			global::Gtk.Box.BoxChild w22 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.label12]));
			w22.Position = 1;
			w22.Expand = false;
			w22.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hseparator5 = new global::Gtk.HSeparator ();
			this.hseparator5.Name = "hseparator5";
			this.vbox2.Add (this.hseparator5);
			global::Gtk.Box.BoxChild w23 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.hseparator5]));
			w23.Position = 2;
			w23.Expand = false;
			w23.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.label_travel_span = new global::Gtk.Label ();
			this.label_travel_span.HeightRequest = 35;
			this.label_travel_span.Name = "label_travel_span";
			this.label_travel_span.Yalign = 1F;
			this.label_travel_span.LabelProp = "<span foreground=\'#D6D6D6\' size=\'20000\'>0 min</span>";
			this.label_travel_span.UseMarkup = true;
			this.vbox2.Add (this.label_travel_span);
			global::Gtk.Box.BoxChild w24 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.label_travel_span]));
			w24.Position = 3;
			w24.Expand = false;
			w24.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.label_distance = new global::Gtk.Label ();
			this.label_distance.HeightRequest = 35;
			this.label_distance.Name = "label_distance";
			this.label_distance.Yalign = 1F;
			this.label_distance.LabelProp = "<span foreground=\'#AAAAAA\' size=\'20000\'>0 km</span>";
			this.label_distance.UseMarkup = true;
			this.vbox2.Add (this.label_distance);
			global::Gtk.Box.BoxChild w25 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.label_distance]));
			w25.Position = 4;
			w25.Expand = false;
			w25.Fill = false;
			this.hbox4.Add (this.vbox2);
			global::Gtk.Box.BoxChild w26 = ((global::Gtk.Box.BoxChild)(this.hbox4 [this.vbox2]));
			w26.PackType = ((global::Gtk.PackType)(1));
			w26.Position = 2;
			w26.Fill = false;
			this.vbox1.Add (this.hbox4);
			global::Gtk.Box.BoxChild w27 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.hbox4]));
			w27.Position = 2;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hseparator1 = new global::Gtk.HSeparator ();
			this.hseparator1.Name = "hseparator1";
			this.vbox1.Add (this.hseparator1);
			global::Gtk.Box.BoxChild w28 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.hseparator1]));
			w28.Position = 3;
			w28.Expand = false;
			w28.Fill = false;
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
			this.label5.LabelProp = global::Mono.Unix.Catalog.GetString ("<span foreground=\'#d39c2f\' size=\'15000\'><b>DRIVE</b></span>");
			this.label5.UseMarkup = true;
			this.eventbox_drive.Add (this.label5);
			this.hbox5.Add (this.eventbox_drive);
			global::Gtk.Box.BoxChild w30 = ((global::Gtk.Box.BoxChild)(this.hbox5 [this.eventbox_drive]));
			w30.Position = 0;
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
			global::Gtk.Box.BoxChild w32 = ((global::Gtk.Box.BoxChild)(this.hbox5 [this.eventbox_nav]));
			w32.Position = 1;
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
			global::Gtk.Box.BoxChild w34 = ((global::Gtk.Box.BoxChild)(this.hbox5 [this.eventbox_cam]));
			w34.Position = 2;
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
			global::Gtk.Box.BoxChild w36 = ((global::Gtk.Box.BoxChild)(this.hbox5 [this.eventbox_weather]));
			w36.Position = 3;
			// Container child hbox5.Gtk.Box+BoxChild
			this.eventbox_traffic = new global::Gtk.EventBox ();
			this.eventbox_traffic.Name = "eventbox_traffic";
			// Container child eventbox_traffic.Gtk.Container+ContainerChild
			this.label9 = new global::Gtk.Label ();
			this.label9.Name = "label9";
			this.label9.LabelProp = global::Mono.Unix.Catalog.GetString ("<span foreground=\'#606060\' size=\'15000\'>TRAF</span>");
			this.label9.UseMarkup = true;
			this.eventbox_traffic.Add (this.label9);
			this.hbox5.Add (this.eventbox_traffic);
			global::Gtk.Box.BoxChild w38 = ((global::Gtk.Box.BoxChild)(this.hbox5 [this.eventbox_traffic]));
			w38.Position = 4;
			w38.Expand = false;
			w38.Fill = false;
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
			global::Gtk.Box.BoxChild w40 = ((global::Gtk.Box.BoxChild)(this.hbox5 [this.eventbox_options]));
			w40.Position = 5;
			this.vbox1.Add (this.hbox5);
			global::Gtk.Box.BoxChild w41 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.hbox5]));
			w41.PackType = ((global::Gtk.PackType)(1));
			w41.Position = 4;
			w41.Expand = false;
			w41.Fill = false;
			this.Add (this.vbox1);
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.Hide ();
		}
	}
}
