
// This file has been generated by the GUI designer. Do not modify.

public partial class MainWindow
{
	private global::Gtk.VBox vbox1;

	protected virtual void Build ()
	{
		global::Stetic.Gui.Initialize (this);
		// Widget MainWindow
		this.WidthRequest = 800;
		this.HeightRequest = 480;
		this.Name = "MainWindow";
		this.Title = global::Mono.Unix.Catalog.GetString ("MainWindow");
		this.WindowPosition = ((global::Gtk.WindowPosition)(4));
		this.DefaultWidth = 800;
		this.DefaultHeight = 480;
		// Container child MainWindow.Gtk.Container+ContainerChild
		this.vbox1 = new global::Gtk.VBox ();
		this.vbox1.Name = "vbox1";
		this.vbox1.Spacing = 6;
		this.Add (this.vbox1);
		if ((this.Child != null)) {
			this.Child.ShowAll ();
		}
		this.Show ();
		this.DeleteEvent += new global::Gtk.DeleteEventHandler (this.OnDeleteEvent);
	}
}
