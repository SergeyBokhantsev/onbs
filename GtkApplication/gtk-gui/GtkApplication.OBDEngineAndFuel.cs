
// This file has been generated by the GUI designer. Do not modify.
namespace GtkApplication
{
	public partial class OBDEngineAndFuel
	{
		private global::Gtk.VBox vbox1;
		
		private global::Gtk.HBox hbox1;
		
		private global::Gtk.DrawingArea d_chart;
		
		private global::Gtk.VBox vbox2;
		
		private global::Gtk.VBox vbox3;
		
		private global::Gtk.EventBox eventbox_primary_title1;
		
		private global::Gtk.Label label_primary_title1;
		
		private global::Gtk.EventBox eventbox_primary_value1;
		
		private global::Gtk.Label label_primary_value1;
		
		private global::Gtk.EventBox eventbox_primary_max1;
		
		private global::Gtk.Label label_primary_max1;
		
		private global::Gtk.VBox vbox4;
		
		private global::Gtk.EventBox eventbox_primary_title2;
		
		private global::Gtk.Label label_primary_title2;
		
		private global::Gtk.EventBox eventbox_primary_value2;
		
		private global::Gtk.Label label_primary_value2;
		
		private global::Gtk.EventBox eventbox_primary_max2;
		
		private global::Gtk.Label label_primary_max2;
		
		private global::Gtk.VBox vbox5;
		
		private global::Gtk.EventBox eventbox_primary_title3;
		
		private global::Gtk.Label label_primary_title3;
		
		private global::Gtk.EventBox eventbox_primary_value3;
		
		private global::Gtk.Label label_primary_value3;
		
		private global::Gtk.EventBox eventbox_primary_max3;
		
		private global::Gtk.Label label_primary_max3;
		
		private global::Gtk.HBox hbox3;
		
		private global::Gtk.EventBox eventbox_secondary1;
		
		private global::Gtk.Label label_secondary1;
		
		private global::Gtk.EventBox eventbox_secondary2;
		
		private global::Gtk.Label label_secondary2;
		
		private global::Gtk.EventBox eventbox_secondary3;
		
		private global::Gtk.Label label_secondary3;
		
		private global::Gtk.HBox hbox4;
		
		private global::Gtk.EventBox eventbox_secondary4;
		
		private global::Gtk.Label label_secondary4;
		
		private global::Gtk.EventBox eventbox_secondary5;
		
		private global::Gtk.Label label_secondary5;
		
		private global::Gtk.EventBox eventbox_secondary6;
		
		private global::Gtk.Label label_secondary6;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget GtkApplication.OBDEngineAndFuel
			global::Stetic.BinContainer.Attach (this);
			this.Name = "GtkApplication.OBDEngineAndFuel";
			// Container child GtkApplication.OBDEngineAndFuel.Gtk.Container+ContainerChild
			this.vbox1 = new global::Gtk.VBox ();
			this.vbox1.Name = "vbox1";
			this.vbox1.Spacing = 6;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox ();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.d_chart = new global::Gtk.DrawingArea ();
			this.d_chart.HeightRequest = 300;
			this.d_chart.Name = "d_chart";
			this.hbox1.Add (this.d_chart);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.d_chart]));
			w1.Position = 0;
			w1.Padding = ((uint)(10));
			// Container child hbox1.Gtk.Box+BoxChild
			this.vbox2 = new global::Gtk.VBox ();
			this.vbox2.WidthRequest = 200;
			this.vbox2.Name = "vbox2";
			this.vbox2.Homogeneous = true;
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.vbox3 = new global::Gtk.VBox ();
			this.vbox3.Name = "vbox3";
			this.vbox3.Spacing = -4;
			// Container child vbox3.Gtk.Box+BoxChild
			this.eventbox_primary_title1 = new global::Gtk.EventBox ();
			this.eventbox_primary_title1.Name = "eventbox_primary_title1";
			// Container child eventbox_primary_title1.Gtk.Container+ContainerChild
			this.label_primary_title1 = new global::Gtk.Label ();
			this.label_primary_title1.Name = "label_primary_title1";
			this.label_primary_title1.LabelProp = global::Mono.Unix.Catalog.GetString ("label1");
			this.eventbox_primary_title1.Add (this.label_primary_title1);
			this.vbox3.Add (this.eventbox_primary_title1);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.eventbox_primary_title1]));
			w3.Position = 0;
			w3.Expand = false;
			w3.Fill = false;
			// Container child vbox3.Gtk.Box+BoxChild
			this.eventbox_primary_value1 = new global::Gtk.EventBox ();
			this.eventbox_primary_value1.Name = "eventbox_primary_value1";
			// Container child eventbox_primary_value1.Gtk.Container+ContainerChild
			this.label_primary_value1 = new global::Gtk.Label ();
			this.label_primary_value1.Name = "label_primary_value1";
			this.label_primary_value1.LabelProp = global::Mono.Unix.Catalog.GetString ("label2");
			this.eventbox_primary_value1.Add (this.label_primary_value1);
			this.vbox3.Add (this.eventbox_primary_value1);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.eventbox_primary_value1]));
			w5.Position = 1;
			w5.Expand = false;
			w5.Fill = false;
			// Container child vbox3.Gtk.Box+BoxChild
			this.eventbox_primary_max1 = new global::Gtk.EventBox ();
			this.eventbox_primary_max1.Name = "eventbox_primary_max1";
			// Container child eventbox_primary_max1.Gtk.Container+ContainerChild
			this.label_primary_max1 = new global::Gtk.Label ();
			this.label_primary_max1.Name = "label_primary_max1";
			this.label_primary_max1.LabelProp = global::Mono.Unix.Catalog.GetString ("label3");
			this.eventbox_primary_max1.Add (this.label_primary_max1);
			this.vbox3.Add (this.eventbox_primary_max1);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.eventbox_primary_max1]));
			w7.Position = 2;
			w7.Expand = false;
			w7.Fill = false;
			this.vbox2.Add (this.vbox3);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.vbox3]));
			w8.Position = 0;
			w8.Expand = false;
			w8.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.vbox4 = new global::Gtk.VBox ();
			this.vbox4.Name = "vbox4";
			this.vbox4.Spacing = -4;
			// Container child vbox4.Gtk.Box+BoxChild
			this.eventbox_primary_title2 = new global::Gtk.EventBox ();
			this.eventbox_primary_title2.Name = "eventbox_primary_title2";
			// Container child eventbox_primary_title2.Gtk.Container+ContainerChild
			this.label_primary_title2 = new global::Gtk.Label ();
			this.label_primary_title2.Name = "label_primary_title2";
			this.label_primary_title2.LabelProp = global::Mono.Unix.Catalog.GetString ("label1");
			this.eventbox_primary_title2.Add (this.label_primary_title2);
			this.vbox4.Add (this.eventbox_primary_title2);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.vbox4 [this.eventbox_primary_title2]));
			w10.Position = 0;
			w10.Expand = false;
			w10.Fill = false;
			// Container child vbox4.Gtk.Box+BoxChild
			this.eventbox_primary_value2 = new global::Gtk.EventBox ();
			this.eventbox_primary_value2.Name = "eventbox_primary_value2";
			// Container child eventbox_primary_value2.Gtk.Container+ContainerChild
			this.label_primary_value2 = new global::Gtk.Label ();
			this.label_primary_value2.Name = "label_primary_value2";
			this.label_primary_value2.LabelProp = global::Mono.Unix.Catalog.GetString ("label2");
			this.eventbox_primary_value2.Add (this.label_primary_value2);
			this.vbox4.Add (this.eventbox_primary_value2);
			global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.vbox4 [this.eventbox_primary_value2]));
			w12.Position = 1;
			w12.Expand = false;
			w12.Fill = false;
			// Container child vbox4.Gtk.Box+BoxChild
			this.eventbox_primary_max2 = new global::Gtk.EventBox ();
			this.eventbox_primary_max2.Name = "eventbox_primary_max2";
			// Container child eventbox_primary_max2.Gtk.Container+ContainerChild
			this.label_primary_max2 = new global::Gtk.Label ();
			this.label_primary_max2.Name = "label_primary_max2";
			this.label_primary_max2.LabelProp = global::Mono.Unix.Catalog.GetString ("label3");
			this.eventbox_primary_max2.Add (this.label_primary_max2);
			this.vbox4.Add (this.eventbox_primary_max2);
			global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.vbox4 [this.eventbox_primary_max2]));
			w14.Position = 2;
			w14.Expand = false;
			w14.Fill = false;
			this.vbox2.Add (this.vbox4);
			global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.vbox4]));
			w15.Position = 1;
			w15.Expand = false;
			w15.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.vbox5 = new global::Gtk.VBox ();
			this.vbox5.Name = "vbox5";
			this.vbox5.Spacing = -4;
			// Container child vbox5.Gtk.Box+BoxChild
			this.eventbox_primary_title3 = new global::Gtk.EventBox ();
			this.eventbox_primary_title3.Name = "eventbox_primary_title3";
			// Container child eventbox_primary_title3.Gtk.Container+ContainerChild
			this.label_primary_title3 = new global::Gtk.Label ();
			this.label_primary_title3.Name = "label_primary_title3";
			this.label_primary_title3.LabelProp = global::Mono.Unix.Catalog.GetString ("label1");
			this.eventbox_primary_title3.Add (this.label_primary_title3);
			this.vbox5.Add (this.eventbox_primary_title3);
			global::Gtk.Box.BoxChild w17 = ((global::Gtk.Box.BoxChild)(this.vbox5 [this.eventbox_primary_title3]));
			w17.Position = 0;
			w17.Expand = false;
			w17.Fill = false;
			// Container child vbox5.Gtk.Box+BoxChild
			this.eventbox_primary_value3 = new global::Gtk.EventBox ();
			this.eventbox_primary_value3.Name = "eventbox_primary_value3";
			// Container child eventbox_primary_value3.Gtk.Container+ContainerChild
			this.label_primary_value3 = new global::Gtk.Label ();
			this.label_primary_value3.Name = "label_primary_value3";
			this.label_primary_value3.LabelProp = global::Mono.Unix.Catalog.GetString ("label2");
			this.eventbox_primary_value3.Add (this.label_primary_value3);
			this.vbox5.Add (this.eventbox_primary_value3);
			global::Gtk.Box.BoxChild w19 = ((global::Gtk.Box.BoxChild)(this.vbox5 [this.eventbox_primary_value3]));
			w19.Position = 1;
			w19.Expand = false;
			w19.Fill = false;
			// Container child vbox5.Gtk.Box+BoxChild
			this.eventbox_primary_max3 = new global::Gtk.EventBox ();
			this.eventbox_primary_max3.Name = "eventbox_primary_max3";
			// Container child eventbox_primary_max3.Gtk.Container+ContainerChild
			this.label_primary_max3 = new global::Gtk.Label ();
			this.label_primary_max3.Name = "label_primary_max3";
			this.label_primary_max3.LabelProp = global::Mono.Unix.Catalog.GetString ("label3");
			this.eventbox_primary_max3.Add (this.label_primary_max3);
			this.vbox5.Add (this.eventbox_primary_max3);
			global::Gtk.Box.BoxChild w21 = ((global::Gtk.Box.BoxChild)(this.vbox5 [this.eventbox_primary_max3]));
			w21.Position = 2;
			w21.Expand = false;
			w21.Fill = false;
			this.vbox2.Add (this.vbox5);
			global::Gtk.Box.BoxChild w22 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.vbox5]));
			w22.Position = 2;
			w22.Expand = false;
			w22.Fill = false;
			this.hbox1.Add (this.vbox2);
			global::Gtk.Box.BoxChild w23 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.vbox2]));
			w23.Position = 1;
			w23.Expand = false;
			w23.Fill = false;
			this.vbox1.Add (this.hbox1);
			global::Gtk.Box.BoxChild w24 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.hbox1]));
			w24.Position = 0;
			w24.Expand = false;
			w24.Fill = false;
			w24.Padding = ((uint)(10));
			// Container child vbox1.Gtk.Box+BoxChild
			this.hbox3 = new global::Gtk.HBox ();
			this.hbox3.Name = "hbox3";
			this.hbox3.Homogeneous = true;
			this.hbox3.Spacing = 6;
			// Container child hbox3.Gtk.Box+BoxChild
			this.eventbox_secondary1 = new global::Gtk.EventBox ();
			this.eventbox_secondary1.Name = "eventbox_secondary1";
			// Container child eventbox_secondary1.Gtk.Container+ContainerChild
			this.label_secondary1 = new global::Gtk.Label ();
			this.label_secondary1.Name = "label_secondary1";
			this.label_secondary1.Xpad = 40;
			this.label_secondary1.Ypad = 10;
			this.label_secondary1.Justify = ((global::Gtk.Justification)(2));
			this.eventbox_secondary1.Add (this.label_secondary1);
			this.hbox3.Add (this.eventbox_secondary1);
			global::Gtk.Box.BoxChild w26 = ((global::Gtk.Box.BoxChild)(this.hbox3 [this.eventbox_secondary1]));
			w26.Position = 0;
			// Container child hbox3.Gtk.Box+BoxChild
			this.eventbox_secondary2 = new global::Gtk.EventBox ();
			this.eventbox_secondary2.Name = "eventbox_secondary2";
			// Container child eventbox_secondary2.Gtk.Container+ContainerChild
			this.label_secondary2 = new global::Gtk.Label ();
			this.label_secondary2.Name = "label_secondary2";
			this.label_secondary2.Xpad = 40;
			this.label_secondary2.Ypad = 10;
			this.label_secondary2.Justify = ((global::Gtk.Justification)(2));
			this.eventbox_secondary2.Add (this.label_secondary2);
			this.hbox3.Add (this.eventbox_secondary2);
			global::Gtk.Box.BoxChild w28 = ((global::Gtk.Box.BoxChild)(this.hbox3 [this.eventbox_secondary2]));
			w28.Position = 1;
			// Container child hbox3.Gtk.Box+BoxChild
			this.eventbox_secondary3 = new global::Gtk.EventBox ();
			this.eventbox_secondary3.Name = "eventbox_secondary3";
			// Container child eventbox_secondary3.Gtk.Container+ContainerChild
			this.label_secondary3 = new global::Gtk.Label ();
			this.label_secondary3.Name = "label_secondary3";
			this.label_secondary3.Xpad = 40;
			this.label_secondary3.Ypad = 10;
			this.label_secondary3.Justify = ((global::Gtk.Justification)(2));
			this.eventbox_secondary3.Add (this.label_secondary3);
			this.hbox3.Add (this.eventbox_secondary3);
			global::Gtk.Box.BoxChild w30 = ((global::Gtk.Box.BoxChild)(this.hbox3 [this.eventbox_secondary3]));
			w30.Position = 2;
			this.vbox1.Add (this.hbox3);
			global::Gtk.Box.BoxChild w31 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.hbox3]));
			w31.Position = 1;
			w31.Expand = false;
			w31.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hbox4 = new global::Gtk.HBox ();
			this.hbox4.Name = "hbox4";
			this.hbox4.Homogeneous = true;
			this.hbox4.Spacing = 6;
			// Container child hbox4.Gtk.Box+BoxChild
			this.eventbox_secondary4 = new global::Gtk.EventBox ();
			this.eventbox_secondary4.Name = "eventbox_secondary4";
			// Container child eventbox_secondary4.Gtk.Container+ContainerChild
			this.label_secondary4 = new global::Gtk.Label ();
			this.label_secondary4.Name = "label_secondary4";
			this.label_secondary4.Xpad = 40;
			this.label_secondary4.Ypad = 10;
			this.label_secondary4.UseMarkup = true;
			this.label_secondary4.Justify = ((global::Gtk.Justification)(2));
			this.eventbox_secondary4.Add (this.label_secondary4);
			this.hbox4.Add (this.eventbox_secondary4);
			global::Gtk.Box.BoxChild w33 = ((global::Gtk.Box.BoxChild)(this.hbox4 [this.eventbox_secondary4]));
			w33.Position = 0;
			// Container child hbox4.Gtk.Box+BoxChild
			this.eventbox_secondary5 = new global::Gtk.EventBox ();
			this.eventbox_secondary5.Name = "eventbox_secondary5";
			// Container child eventbox_secondary5.Gtk.Container+ContainerChild
			this.label_secondary5 = new global::Gtk.Label ();
			this.label_secondary5.Name = "label_secondary5";
			this.label_secondary5.Xpad = 40;
			this.label_secondary5.Ypad = 10;
			this.label_secondary5.Justify = ((global::Gtk.Justification)(2));
			this.eventbox_secondary5.Add (this.label_secondary5);
			this.hbox4.Add (this.eventbox_secondary5);
			global::Gtk.Box.BoxChild w35 = ((global::Gtk.Box.BoxChild)(this.hbox4 [this.eventbox_secondary5]));
			w35.Position = 1;
			// Container child hbox4.Gtk.Box+BoxChild
			this.eventbox_secondary6 = new global::Gtk.EventBox ();
			this.eventbox_secondary6.Name = "eventbox_secondary6";
			// Container child eventbox_secondary6.Gtk.Container+ContainerChild
			this.label_secondary6 = new global::Gtk.Label ();
			this.label_secondary6.Name = "label_secondary6";
			this.label_secondary6.Xpad = 40;
			this.label_secondary6.Ypad = 10;
			this.label_secondary6.Justify = ((global::Gtk.Justification)(2));
			this.eventbox_secondary6.Add (this.label_secondary6);
			this.hbox4.Add (this.eventbox_secondary6);
			global::Gtk.Box.BoxChild w37 = ((global::Gtk.Box.BoxChild)(this.hbox4 [this.eventbox_secondary6]));
			w37.Position = 2;
			this.vbox1.Add (this.hbox4);
			global::Gtk.Box.BoxChild w38 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.hbox4]));
			w38.Position = 2;
			w38.Expand = false;
			w38.Fill = false;
			this.Add (this.vbox1);
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.Hide ();
		}
	}
}
