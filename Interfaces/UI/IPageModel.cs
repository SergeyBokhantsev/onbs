﻿using System;
using System.Collections.Generic;
using Interfaces.Input;

namespace Interfaces.UI
{
    public delegate void PageModelPropertyChangedHandler(string propertyName);

    public interface IListItem
    {
        event EventHandler CaptionChanged;
        event EventHandler FocusChanged;

        string Caption { get; set; }
        bool Focused { get; set; }
        void Click();
    }

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
       // event PageModelFocusChangedHandler FocusChanged;
        event EventHandler Disposing;

        string ViewName { get; }
        bool NoDialogsAllowed { get; }
       // IControl Focused { get; }
        //IEnumerable<IControl> Controls { get; }
        T GetProperty<T>(string name);
        void SetProperty(string name, object value);
        void RefreshAllProps();
        void Action(PageModelActionEventArgs actionArgs);
    }

	public static class ModelNames
    {
        public const string UnmappedAction = "UnmappedAction";

        public const string ButtonAccept = "Accept";
        public const string ButtonCancel = "Cancel";
        public const string ButtonF1 = "F1";
        public const string ButtonF2 = "F2";

        public const string ButtonNext = "Next";
        public const string ButtonPrev = "Prev";
        public const string ButtonSelect = "Select";

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

        public const string ButtonNextLabel = "Next_label";
        public const string ButtonPrevLabel = "Prev_label";
        public const string ButtonSelectLabel = "Select_label";

        public const string ButtonF3Label = "F3_label";
        public const string ButtonF4Label = "F4_label";
        public const string ButtonF5Label = "F5_label";
        public const string ButtonF6Label = "F6_label";
        public const string ButtonF7Label = "F7_label";
        public const string ButtonF8Label = "F8_label";

        public const string PageTitle = "page_title";

        public static string ResolveButtonLabelName(string buttonName)
        {
            return string.Concat(buttonName, "_label");
        }

        public static KeyValuePair<string, string>[] GetButtonAndLabelNames()
        {
            return new[] 
            { 
                new KeyValuePair<string, string>(ButtonAccept, ButtonAcceptLabel),
                new KeyValuePair<string, string>(ButtonCancel, ButtonCancelLabel),
                new KeyValuePair<string, string>(ButtonF1, ButtonF1Label),
                new KeyValuePair<string, string>(ButtonF2, ButtonF2Label),

                new KeyValuePair<string, string>(ButtonNext, ButtonNextLabel),
                new KeyValuePair<string, string>(ButtonPrev, ButtonPrevLabel),
                new KeyValuePair<string, string>(ButtonSelect, ButtonSelectLabel),

                new KeyValuePair<string, string>(ButtonF3, ButtonF3Label),
                new KeyValuePair<string, string>(ButtonF4, ButtonF4Label),
                new KeyValuePair<string, string>(ButtonF5, ButtonF5Label),
                new KeyValuePair<string, string>(ButtonF6, ButtonF6Label),
                new KeyValuePair<string, string>(ButtonF7, ButtonF7Label),
                new KeyValuePair<string, string>(ButtonF8, ButtonF8Label)
            };
        }
    }
}
