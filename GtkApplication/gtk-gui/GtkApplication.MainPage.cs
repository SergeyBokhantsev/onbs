
// This file has been generated by the GUI designer. Do not modify.
namespace GtkApplication
{
	public partial class MainPage
	{
		private global::Gtk.VBox vbox1;
		
		private global::Gtk.Label lCaption;
		
		private global::Gtk.Button bStart;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget GtkApplication.MainPage
			global::Stetic.BinContainer.Attach (this);
			this.Name = "GtkApplication.MainPage";
			// Container child GtkApplication.MainPage.Gtk.Container+ContainerChild
			this.vbox1 = new global::Gtk.VBox ();
			this.vbox1.Name = "vbox1";
			this.vbox1.Spacing = 6;
			// Container child vbox1.Gtk.Box+BoxChild
			this.lCaption = new global::Gtk.Label ();
			this.lCaption.Name = "welcome";
			this.lCaption.LabelProp = global::Mono.Unix.Catalog.GetString ("label1");
			this.vbox1.Add (this.lCaption);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.lCaption]));
			w1.Position = 0;
			w1.Expand = false;
			w1.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.bStart = new global::Gtk.Button ();
			this.bStart.HeightRequest = 50;
			this.bStart.CanFocus = true;
			this.bStart.Name = "bStart";
			this.bStart.UseUnderline = true;
			this.bStart.Label = global::Mono.Unix.Catalog.GetString ("GtkButton");
			this.vbox1.Add (this.bStart);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.bStart]));
			w2.Position = 1;
			w2.Expand = false;
			w2.Fill = false;
			this.Add (this.vbox1);
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.Hide ();
		}
	}
}
