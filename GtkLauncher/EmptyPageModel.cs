﻿using System;
using System.Collections.Generic;
using Interfaces.UI;

namespace GtkLauncher
{
	public class EmptyPageModel : IPageModel
	{
		private Dictionary<string, object> props = new Dictionary<string, object> ();

		public EmptyPageModel(string modelName)
		{
			Name = modelName;
		}

		#region IPageModel implementation

		public event Action<PageModelActionEventArgs> OnAction;

		public event PageModelPropertyChangedHandler PropertyChanged;

		public event EventHandler Disposing;

		public T GetProperty<T>(string name)
		{
			if (props.ContainsKey (name))
				return (T)props [name];
			else
				return default(T);
		}

		public void SetProperty(string name, object value)
		{
			props [name] = value;

			if (PropertyChanged != null)
				PropertyChanged(name);
		}

		public void RefreshAllProps()
		{
			if (PropertyChanged != null) {
				foreach (var key in props.Keys)
					PropertyChanged (key);
			}
		}

		public void Action(PageModelActionEventArgs actionArgs)
		{
			if (OnAction != null)
				OnAction(actionArgs);
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
