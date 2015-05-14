using System;
using Gtk;
using Interfaces;

public partial class MainWindow: Gtk.Window
{
	public MainWindow (IHostController controller) 
        : base (Gtk.WindowType.Toplevel)
	{
		Build();

        controller.Logger.Log("MainWindow created", Interfaces.LogLevels.Debug);
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}
}
