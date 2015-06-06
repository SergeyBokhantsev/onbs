using Interfaces;
using Interfaces.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIController.Models
{
    public class NavigationAppPage : ExternalApplicationPage
    {
        private readonly IAutomationController automation;

        public NavigationAppPage(IHostController hostController, string navAppKey)
            :base(typeof(ExternalApplicationPage).Name, 
            hostController.ProcessRunnerFactory.Create(navAppKey),
            hostController.Dispatcher,
            hostController.Logger,
            hostController.GetController<IUIController>())
        {
            automation = hostController.GetController<IAutomationController>();
        }

        protected override void DoAction(PageModelActionEventArgs args)
        {
            switch (args.ActionName)
            {
                case "F1":
                    automation.Key(AutomationKeys.Control, AutomationKeys.minus);
                    break;

                case "F2":
                    automation.Key(AutomationKeys.Up);
                    break;

                case "F3":
                    automation.Key(AutomationKeys.Control, AutomationKeys.plus);
                    break;

                case "F5":
                    automation.Key(AutomationKeys.Left);
                    break;

                case "F6":
                    automation.Key(AutomationKeys.Down);
                    break;

                case "F7":
                    automation.Key(AutomationKeys.Right);
                    break;

                default:
                    base.DoAction(args);
                    break;
            }
        }  
    }
}
