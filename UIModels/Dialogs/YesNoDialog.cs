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
        public YesNoDialog(string caption, string message, string yesCaption, string noCaption, IHostController hc, int timeout, DialogResults defaultResult)
            : base("YesNoDialog", hc)
        {
            this.timeout = timeout;
            this.defaultResult = defaultResult;

            SetProperty(CaptionPropertyName, caption);

            MessagePropertyName = "msg";
            SetProperty(MessagePropertyName, message);

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
