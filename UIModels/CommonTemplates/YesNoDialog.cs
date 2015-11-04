using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.UI;

namespace UIModels.CommonTemplates
{
    public class YesNoDialog : ModelBase
    {
        private readonly Action yesAction;
        private readonly Action noAction;

        public YesNoDialog(IHostController hostController, Action yesAction, Action noAction, string caption, string message, string yesCaption = "Yes", string noCaption = "No")
            : base("CommonYesNoPage", hostController.SyncContext, hostController.Logger)
        {
            this.yesAction = yesAction;
            this.noAction = noAction;

            onlyPressButtonEvents = true;
            SetProperty("label_caption", caption);
            SetProperty("label_message", message);
            SetProperty(ModelNames.ButtonAcceptLabel, yesCaption);
            SetProperty(ModelNames.ButtonCancelLabel, noCaption);
        }

        protected override void DoAction(Interfaces.UI.PageModelActionEventArgs e)
        {
            switch (e.ActionName)
            {
                case ModelNames.ButtonAccept:
                    if (yesAction != null)
                        yesAction();
                    break;

                case ModelNames.ButtonCancel:
                    if (noAction != null)
                        noAction();
                    break;
            }
        }
    }
}
