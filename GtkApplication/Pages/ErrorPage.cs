using System;
using Interfaces;
using Interfaces.UI;
using GtkApplication.Pages;

namespace GtkApplication
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ErrorPage : Gtk.Bin
	{
		public ErrorPage(IPageModel model, Style style, ILogger logger)
		{
			this.Build();

			var binder = new ModelBinder(model, logger);

			eventbox_error.ModifyBg(Gtk.StateType.Normal, new Gdk.Color(255, 0, 0));
			eventbox_error.ModifyFg(Gtk.StateType.Normal, new Gdk.Color(255, 255, 255));
					
			binder.BindCustomAction<Exception>(e =>  
			{
				var message = e.Message;

				if (e.InnerException != null)
				{
					message += Environment.NewLine + "Inner Exception: " + e.InnerException.Message;
				}

				l_error.Text = message; 
			}, "ex");

			binder.UpdateBindings();
		}
	}

	public class ErrorPageModel : IPageModel
	{
		private readonly Exception ex;

		public ErrorPageModel(Exception ex)
		{
			this.ex = ex;
		}

		#region IPageModel implementation
		public event PageModelPropertyChangedHandler PropertyChanged;
		public event EventHandler Disposing;
		public T GetProperty<T>(string name)
		{
			if (name == "ex")
				return (T)(object)ex;
			else
				return default(T);
		}
		public void SetProperty(string name, object value)
		{
			
		}
		public void RefreshAllProps()
		{
			OnPropertyChanged("ex");
		}
		public void Action(PageModelActionEventArgs actionArgs)
		{			
		}
		public string Name
		{
			get
			{
				return "ErrorPage";
			}
		}
		#endregion

		private void OnPropertyChanged(string propName)
		{
			var handler = PropertyChanged;
			if (handler != null)
				handler(propName);
		}

		#region IDisposable implementation
		public void Dispose()
		{
			var handler = Disposing;
			if (handler != null)
				handler(this, new EventArgs());
		}
		#endregion

        public bool NoDialogsAllowed
        {
            get { return false; }
        }
    }
}

