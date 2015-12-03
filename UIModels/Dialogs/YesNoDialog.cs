using System.Collections.Generic;
using Interfaces;
using Interfaces.UI;
using Interfaces.Input;

namespace UIModels.Dialogs
{
    public class YesNoDialog : DialogModelBase
    {
        public YesNoDialog(string caption, string message, string yesCaption, string noCaption, IHostController hc, int timeout, DialogResults defaultResult)
            : base(hc)
        {
            this.timeout = timeout;
            this.defaultResult = defaultResult;

            Caption = caption;
            Message = message;
            Buttons = new Dictionary<DialogResults, string> { { DialogResults.Yes, yesCaption }, { DialogResults.No, noCaption } };
        }

        public override void HardwareButtonClick(Buttons button)
        {
            switch (button)
            {
                case Interfaces.Input.Buttons.Accept:
                    OnButtonClick(DialogResults.Yes);
                    break;

                case Interfaces.Input.Buttons.Cancel:
                    OnButtonClick(DialogResults.No);
                    break;

                default:
                    base.HardwareButtonClick(button);
                    break;
            }
        }
    }
}
