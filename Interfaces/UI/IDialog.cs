﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Interfaces.UI
{
    public enum DialogResults
    {
        Help = -11,
        Apply = -10,
        No = -9,
        Yes = -8,
        Close = -7,
        Cancel = -6,
        Ok = -5,
        DeleteEvent = -4,
        Accept = -3,
        Reject = -2,
        None = -1,
    }

    public interface IDialogModel : IPageModel
    {
        // Event to UI
        event Action<DialogResults> ButtonClick;

        // Events to logic
        event Action Shown;
        event Action<DialogResults> Closed;

        string CaptionPropertyName { get; }
        string MessagePropertyName { get; }
        string RemainingTimePropertyName { get; }
        Dictionary<DialogResults, string> Buttons { get; }

        void OnShown();
        void OnClosed(DialogResults result);
    }
}
