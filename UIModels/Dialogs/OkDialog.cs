using System.Collections.Generic;
using Interfaces;
using Interfaces.UI;
using Interfaces.Input;

namespace UIModels.Dialogs
{
    public class OkDialog : DialogModelBase
    {
        public OkDialog(string caption, string message, string okCaption, IHostController hc, int timeout)
            : base(hc)
        {
            this.timeout = timeout;
            this.defaultResult = DialogResults.Ok;

            Caption = caption;
            Message = message;
            Buttons = new Dictionary<DialogResults, string> { { DialogResults.Ok, okCaption } };
        }

        public override void HardwareButtonClick(Buttons button)
        {
            switch (button)
            {
                case Interfaces.Input.Buttons.Accept:
                    OnButtonClick(DialogResults.Ok);
                    break;

                case Interfaces.Input.Buttons.Cancel:
                    OnButtonClick(DialogResults.Ok);
                    break;

                default:
                    base.HardwareButtonClick(button);
                    break;
            }
        }
    }
}
