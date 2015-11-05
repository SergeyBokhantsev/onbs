using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Interfaces.Input;

namespace Interfaces.UI
{
    public delegate void PageModelPropertyChangedHandler(string propertyName);

    public class PageModelActionEventArgs : EventArgs
    {
        public ButtonStates State { get; private set; }

        public string ActionName { get; private set; }

        public PageModelActionEventArgs(string actionName, ButtonStates state)
        {
            ActionName = actionName;
            State = state;
        }
    }

    public interface IPageModel : IDisposable
    {
        event PageModelPropertyChangedHandler PropertyChanged;
        event EventHandler Disposing;
        string Name { get; }
        bool NoDialogsAllowed { get; }
        T GetProperty<T>(string name);
        void SetProperty(string name, object value);
        void RefreshAllProps();
        void Action(PageModelActionEventArgs actionArgs);
    }

	public static class ModelNames
    {
		public const string ButtonAccept = "Accept";
        public const string ButtonCancel = "Cancel";
        public const string ButtonF1 = "F1";
        public const string ButtonF2 = "F2";
        public const string ButtonF3 = "F3";
        public const string ButtonF4 = "F4";
        public const string ButtonF5 = "F5";
        public const string ButtonF6 = "F6";
        public const string ButtonF7 = "F7";
        public const string ButtonF8 = "F8";

		public const string ButtonAcceptLabel = "Accept_label";
		public const string ButtonCancelLabel = "Cancel_label";
		public const string ButtonF1Label = "F1_label";
		public const string ButtonF2Label = "F2_label";
		public const string ButtonF3Label = "F3_label";
		public const string ButtonF4Label = "F4_label";
		public const string ButtonF5Label = "F5_label";
		public const string ButtonF6Label = "F6_label";
		public const string ButtonF7Label = "F7_label";
		public const string ButtonF8Label = "F8_label";

		public static string ResolveButtonLabelName(string buttonName)
		{
			return string.Concat(buttonName, "_label");
		}
    }
}
