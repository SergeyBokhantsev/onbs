
// This file has been generated by the GUI designer. Do not modify.
namespace GtkApplication
{
	public partial class CommonYesNoPage
	{
		private global::Gtk.VBox vbox3;

		private global::Gtk.EventBox eventbox_caption;

		private global::Gtk.Label label_caption;

		private global::Gtk.EventBox eventbox_message;

		private global::Gtk.Label label_message;

		private global::Gtk.HBox hbox1;

		private global::Gtk.EventBox eventbox_no_button;

		private global::Gtk.EventBox eventbox_yes_button;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget GtkApplication.CommonYesNoPage
			global::Stetic.BinContainer.Attach(this);
			this.Name = "GtkApplication.CommonYesNoPage";
			// Container child GtkApplication.CommonYesNoPage.Gtk.Container+ContainerChild
			this.vbox3 = new global::Gtk.VBox();
			this.vbox3.Name = "vbox3";
			this.vbox3.Spacing = 6;
			this.vbox3.BorderWidth = ((uint)(10));
			// Container child vbox3.Gtk.Box+BoxChild
			this.eventbox_caption = new global::Gtk.EventBox();
			this.eventbox_caption.Name = "eventbox_caption";
			// Container child eventbox_caption.Gtk.Container+ContainerChild
			this.label_caption = new global::Gtk.Label();
			this.label_caption.Name = "label_caption";
			this.label_caption.LabelProp = global::Mono.Unix.Catalog.GetString("use label_caption property to set text here");
			this.eventbox_caption.Add(this.label_caption);
			this.vbox3.Add(this.eventbox_caption);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox3[this.eventbox_caption]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			// Container child vbox3.Gtk.Box+BoxChild
			this.eventbox_message = new global::Gtk.EventBox();
			this.eventbox_message.Name = "eventbox_message";
			// Container child eventbox_message.Gtk.Container+ContainerChild
			this.label_message = new global::Gtk.Label();
			this.label_message.HeightRequest = 150;
			this.label_message.Name = "label_message";
			this.label_message.LabelProp = global::Mono.Unix.Catalog.GetString("use label_message property to set text here");
			this.eventbox_message.Add(this.label_message);
			this.vbox3.Add(this.eventbox_message);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.vbox3[this.eventbox_message]));
			w4.Position = 1;
			w4.Expand = false;
			w4.Fill = false;
			// Container child vbox3.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox();
			this.hbox1.HeightRequest = 150;
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.eventbox_no_button = new global::Gtk.EventBox();
			this.eventbox_no_button.HeightRequest = 200;
			this.eventbox_no_button.Name = "eventbox_no_button";
			this.eventbox_no_button.BorderWidth = ((uint)(20));
			this.hbox1.Add(this.eventbox_no_button);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.eventbox_no_button]));
			w5.Position = 0;
			// Container child hbox1.Gtk.Box+BoxChild
			this.eventbox_yes_button = new global::Gtk.EventBox();
			this.eventbox_yes_button.HeightRequest = 200;
			this.eventbox_yes_button.Name = "eventbox_yes_button";
			this.eventbox_yes_button.BorderWidth = ((uint)(20));
			this.hbox1.Add(this.eventbox_yes_button);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.eventbox_yes_button]));
			w6.Position = 1;
			this.vbox3.Add(this.hbox1);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.vbox3[this.hbox1]));
			w7.Position = 2;
			w7.Expand = false;
			this.Add(this.vbox3);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}
