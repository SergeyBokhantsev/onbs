using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces.Input;

namespace Interfaces.UI
{
    public delegate void PageModelPropertyChangedHandler(string propertyName);

    public class PageModelActionEventArgs : EventArgs
    {
        public string ActionName { get; private set; }

        public PageModelActionEventArgs(string actionName)
        {
            ActionName = actionName;
        }
    }

    public interface IPageModel : IDisposable
    {
        event PageModelPropertyChangedHandler PropertyChanged;
        event EventHandler Disposing;
        string Name { get; }
        T GetProperty<T>(string name);
        void SetProperty(string name, object value);
        void RefreshAllProps();
        void Action(PageModelActionEventArgs actionArgs);
        void Button(Buttons button, ButtonStates state);
    }
}
