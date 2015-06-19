using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.Input;
using Interfaces.UI;

namespace UIController.Models.ConfigPages
{
    public class ShutdownPage : ModelBase
    {
        private readonly IHostController hostController;

        public ShutdownPage(IHostController hostController)
            : base("SystemConfigurationPage", hostController.Dispatcher, hostController.Logger)
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
                        hostController.Dispatcher.Exit();
                    }
                    break;

                case ModelNames.ButtonF2:
                    if (args.State == ButtonStates.Press)
                    {
                        var command = hostController.Config.GetString(ConfigNames.SystemRestartCommand);
                        var arg = hostController.Config.GetString(ConfigNames.SystemRestartArg);

                        hostController.ProcessRunnerFactory.Create(command, arg, true, false).Run();
                    }
                    break;

                case ModelNames.ButtonF3:
                    if (args.State == ButtonStates.Press)
                    {
                        var command = hostController.Config.GetString(ConfigNames.SystemShutdownCommand);
                        var arg = hostController.Config.GetString(ConfigNames.SystemShutdownArg);

                        hostController.ProcessRunnerFactory.Create(command, arg, true, false).Run();
                    }
                    break;

                case ModelNames.ButtonCancel:
                    if (args.State == ButtonStates.Press)
                    {
                        hostController.GetController<IUIController>().ShowMainPage();
                    }
                    break;
            }
        }
    }
}
