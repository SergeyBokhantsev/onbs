using System;
using Gtk;
using Interfaces;

public partial class MainWindow: Gtk.Window
{
	public MainWindow (ILogger logger) 
        : base (Gtk.WindowType.Toplevel)
	{
		Build();

        logger.Log(this, "MainWindow created", Interfaces.LogLevels.Debug);

		//this.Fullscreen();
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}
}
