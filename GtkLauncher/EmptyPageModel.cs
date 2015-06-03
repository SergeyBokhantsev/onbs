using System;
using Interfaces.UI;

namespace GtkLauncher
{
	public class EmptyPageModel : IPageModel
	{
		public EmptyPageModel(string modelName)
		{
			Name = modelName;
		}

		#region IPageModel implementation

		public event PageModelPropertyChangedHandler PropertyChanged;

		public event EventHandler Disposing;

		public T GetProperty<T>(string name)
		{
			return default(T);
		}

		public void SetProperty(string name, object value)
		{
		}

		public void RefreshAllProps()
		{

		}

		public void Action(PageModelActionEventArgs actionArgs)
		{

		}

		public void Button(Interfaces.Input.Buttons button, Interfaces.Input.ButtonStates state)
		{

		}

		public string Name
		{
			get;
			private set;
		}

		#endregion

		#region IDisposable implementation

		public void Dispose()
		{

		}

		#endregion
	}
}

