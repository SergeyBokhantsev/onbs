using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.Input;
using Interfaces.UI;

namespace UIModels.ConfigPages
{
    public class ShutdownPage : ModelBase
    {
        private readonly IHostController hostController;

        public ShutdownPage(IHostController hostController)
            : base("CommonVertcalStackPage", hostController.SyncContext, hostController.Logger)
        {
            this.hostController = hostController;

            SetProperty("label_caption", "System power management");
            SetProperty(ModelNames.ButtonCancelLabel, "Return to Main Menu");

            SetProperty(ModelNames.ButtonF1Label, "Quit Application");
            SetProperty(ModelNames.ButtonF2Label, "Restart");
            SetProperty(ModelNames.ButtonF3Label, "Shutdown");
        }

        protected override void DoAction(PageModelActionEventArgs args)
        {
            switch (args.ActionName)
            {
                case ModelNames.ButtonF1:
                    if (args.State == ButtonStates.Press)
                    {
                        hostController.Shutdown(HostControllerShutdownModes.Exit);
                    }
                    break;

                case ModelNames.ButtonF2:
                    if (args.State == ButtonStates.Press)
                    {
                        hostController.Shutdown(HostControllerShutdownModes.Restart);
                    }
                    break;

                case ModelNames.ButtonF3:
                    if (args.State == ButtonStates.Press)
                    {
                        hostController.Shutdown(HostControllerShutdownModes.Shutdown);
                    }
                    break;

                case ModelNames.ButtonCancel:
                    if (args.State == ButtonStates.Press)
                    {
                        hostController.GetController<IUIController>().ShowDefaultPage();
                    }
                    break;
            }
        }
    }
}
