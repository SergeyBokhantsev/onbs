
// This file has been generated by the GUI designer. Do not modify.
namespace GtkApplication
{
	public partial class TrafficPage
	{
		private global::Gtk.VBox vbox1;

		private global::Gtk.HBox hbox1;

		private global::Gtk.HSeparator hseparator2;

		private global::Gtk.Image image_traffic;

		private global::Gtk.HSeparator hseparator1;

		private global::Gtk.HBox hbox5;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget GtkApplication.TrafficPage
			global::Stetic.BinContainer.Attach(this);
			this.Name = "GtkApplication.TrafficPage";
			// Container child GtkApplication.TrafficPage.Gtk.Container+ContainerChild
			this.vbox1 = new global::Gtk.VBox();
			this.vbox1.Name = "vbox1";
			this.vbox1.Spacing = 6;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox();
			this.hbox1.HeightRequest = 35;
			this.hbox1.Name = "hbox1";
			this.hbox1.Homogeneous = true;
			this.hbox1.Spacing = 6;
			this.hbox1.BorderWidth = ((uint)(5));
			this.vbox1.Add(this.hbox1);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hbox1]));
			w1.Position = 0;
			w1.Expand = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hseparator2 = new global::Gtk.HSeparator();
			this.hseparator2.Name = "hseparator2";
			this.vbox1.Add(this.hseparator2);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hseparator2]));
			w2.Position = 1;
			w2.Expand = false;
			w2.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.image_traffic = new global::Gtk.Image();
			this.image_traffic.Name = "image_traffic";
			this.vbox1.Add(this.image_traffic);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.image_traffic]));
			w3.Position = 2;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hseparator1 = new global::Gtk.HSeparator();
			this.hseparator1.Name = "hseparator1";
			this.vbox1.Add(this.hseparator1);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hseparator1]));
			w4.Position = 3;
			w4.Expand = false;
			w4.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hbox5 = new global::Gtk.HBox();
			this.hbox5.HeightRequest = 40;
			this.hbox5.Name = "hbox5";
			this.hbox5.Homogeneous = true;
			this.hbox5.Spacing = 6;
			this.vbox1.Add(this.hbox5);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hbox5]));
			w5.PackType = ((global::Gtk.PackType)(1));
			w5.Position = 4;
			w5.Expand = false;
			this.Add(this.vbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}
