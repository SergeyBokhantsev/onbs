
// This file has been generated by the GUI designer. Do not modify.
namespace GtkApplication
{
	public partial class OBD_DTCPage
	{
		private global::Gtk.VBox vbox1;

		private global::Gtk.EventBox eventbox_codes;

		private global::Gtk.Label label_codes;

		private global::Gtk.EventBox eventbox_buttons;

		private global::Gtk.HBox hbox1;

		private global::Gtk.EventBox eventbox_refresh;

		private global::Gtk.EventBox eventbox_reset;

		private global::Gtk.EventBox eventbox_back;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget GtkApplication.OBD_DTCPage
			global::Stetic.BinContainer.Attach(this);
			this.Name = "GtkApplication.OBD_DTCPage";
			// Container child GtkApplication.OBD_DTCPage.Gtk.Container+ContainerChild
			this.vbox1 = new global::Gtk.VBox();
			this.vbox1.Name = "vbox1";
			this.vbox1.Spacing = 6;
			// Container child vbox1.Gtk.Box+BoxChild
			this.eventbox_codes = new global::Gtk.EventBox();
			this.eventbox_codes.Name = "eventbox_codes";
			// Container child eventbox_codes.Gtk.Container+ContainerChild
			this.label_codes = new global::Gtk.Label();
			this.label_codes.WidthRequest = 700;
			this.label_codes.Name = "label_codes";
			this.label_codes.Xalign = 0.1F;
			this.label_codes.Yalign = 0.1F;
			this.label_codes.LabelProp = global::Mono.Unix.Catalog.GetString("Выход сигнала датчика температуры охлаждающей жидкости за допустимый диапазон");
			this.label_codes.Wrap = true;
			this.eventbox_codes.Add(this.label_codes);
			this.vbox1.Add(this.eventbox_codes);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.eventbox_codes]));
			w2.Position = 0;
			// Container child vbox1.Gtk.Box+BoxChild
			this.eventbox_buttons = new global::Gtk.EventBox();
			this.eventbox_buttons.HeightRequest = 60;
			this.eventbox_buttons.Name = "eventbox_buttons";
			// Container child eventbox_buttons.Gtk.Container+ContainerChild
			this.hbox1 = new global::Gtk.HBox();
			this.hbox1.HeightRequest = 0;
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.eventbox_refresh = new global::Gtk.EventBox();
			this.eventbox_refresh.Name = "eventbox_refresh";
			this.hbox1.Add(this.eventbox_refresh);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.eventbox_refresh]));
			w3.Position = 0;
			// Container child hbox1.Gtk.Box+BoxChild
			this.eventbox_reset = new global::Gtk.EventBox();
			this.eventbox_reset.Name = "eventbox_reset";
			this.hbox1.Add(this.eventbox_reset);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.eventbox_reset]));
			w4.Position = 1;
			// Container child hbox1.Gtk.Box+BoxChild
			this.eventbox_back = new global::Gtk.EventBox();
			this.eventbox_back.Name = "eventbox_back";
			this.hbox1.Add(this.eventbox_back);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.eventbox_back]));
			w5.Position = 2;
			this.eventbox_buttons.Add(this.hbox1);
			this.vbox1.Add(this.eventbox_buttons);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.eventbox_buttons]));
			w7.Position = 1;
			w7.Expand = false;
			w7.Padding = ((uint)(10));
			this.Add(this.vbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}
