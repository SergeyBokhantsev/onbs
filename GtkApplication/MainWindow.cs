using System;
using Gdk;
using Gtk;
using Interfaces;

public partial class MainWindow: Gtk.Window
{
	public MainWindow (ILogger logger) 
        : base (Gtk.WindowType.Toplevel)
	{
		Build();

        logger.Log(this, "MainWindow created", Interfaces.LogLevels.Debug);

        this.HideOnDelete();       
	}    

    protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
        this.Hide();
        Application.Quit();
		a.RetVal = true;
	}
}
