
// This file has been generated by the GUI designer. Do not modify.
namespace GtkApplication
{
	public partial class ExternalApplicationPage
	{
		private global::Gtk.VBox vbox1;

		private global::Gtk.EventBox eventbox1;

		private global::Gtk.Label label_launch_info;

		private global::Gtk.EventBox box_close_button;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget GtkApplication.ExternalApplicationPage
			global::Stetic.BinContainer.Attach(this);
			this.Name = "GtkApplication.ExternalApplicationPage";
			// Container child GtkApplication.ExternalApplicationPage.Gtk.Container+ContainerChild
			this.vbox1 = new global::Gtk.VBox();
			this.vbox1.Name = "vbox1";
			this.vbox1.Spacing = 6;
			// Container child vbox1.Gtk.Box+BoxChild
			this.eventbox1 = new global::Gtk.EventBox();
			this.eventbox1.Name = "eventbox1";
			this.eventbox1.BorderWidth = ((uint)(10));
			// Container child eventbox1.Gtk.Container+ContainerChild
			this.label_launch_info = new global::Gtk.Label();
			this.label_launch_info.Name = "label_launch_info";
			this.label_launch_info.LabelProp = global::Mono.Unix.Catalog.GetString("label1");
			this.label_launch_info.Wrap = true;
			this.eventbox1.Add(this.label_launch_info);
			this.vbox1.Add(this.eventbox1);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.eventbox1]));
			w2.Position = 1;
			w2.Padding = ((uint)(20));
			// Container child vbox1.Gtk.Box+BoxChild
			this.box_close_button = new global::Gtk.EventBox();
			this.box_close_button.HeightRequest = 80;
			this.box_close_button.Name = "box_close_button";
			this.box_close_button.BorderWidth = ((uint)(10));
			this.vbox1.Add(this.box_close_button);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.box_close_button]));
			w3.Position = 2;
			w3.Expand = false;
			this.Add(this.vbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}
