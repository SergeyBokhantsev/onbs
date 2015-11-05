using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.UI;

namespace UIModels.Dialogs
{
    public class YesNoDialog : DialogModel
    {
        public YesNoDialog(string caption, string message, string yesCaption, string noCaption, SynchronizationContext syncContext, ILogger logger)
            : base("YesNoDialog", syncContext, logger)
        {
            CaptionPropertyName = "caption";
            SetProperty(CaptionPropertyName, caption);

            Buttons = new Dictionary<Interfaces.UI.DialogResults, string> { { DialogResults.Yes, yesCaption }, { DialogResults.No, noCaption } };
        }

        protected override void DoAction(PageModelActionEventArgs actionArgs)
        {
            switch (actionArgs.ActionName)
            {
                case ModelNames.ButtonAccept:
                    OnButtonClick(DialogResults.Yes);
                    break;

                case ModelNames.ButtonCancel:
                    OnButtonClick(DialogResults.No);
                    break;

                default:
                    base.DoAction(actionArgs);
                    break;
            }
        }
    }
}
