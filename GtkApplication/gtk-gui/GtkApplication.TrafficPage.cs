
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

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget GtkApplication.TrafficPage
			global::Stetic.BinContainer.Attach (this);
			this.Name = "GtkApplication.TrafficPage";
			// Container child GtkApplication.TrafficPage.Gtk.Container+ContainerChild
			this.vbox1 = new global::Gtk.VBox ();
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
			this.hbox5.HeightRequest = 40;
			this.hbox5.Name = "hbox5";
			this.hbox5.Homogeneous = true;
			this.hbox5.Spacing = 6;
			this.vbox1.Add (this.hbox5);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.hbox5]));
			w11.PackType = ((global::Gtk.PackType)(1));
			w11.Position = 4;
			w11.Expand = false;
			this.Add (this.vbox1);
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.Hide ();
		}
	}
}
