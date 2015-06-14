﻿using Interfaces;
using Interfaces.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces.Input;

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
			switch (args.ActionName) {
			case "F1":
				if (args.State == ButtonStates.Press || args.State == ButtonStates.Hold)
					automation.Key (AutomationKeys.Control, AutomationKeys.minus);
				break;

			case "F2":
				if (args.State == ButtonStates.Press || args.State == ButtonStates.Hold)
					automation.Key (AutomationKeys.Up);
				break;

			case "F3":
				if (args.State == ButtonStates.Press || args.State == ButtonStates.Hold)
					automation.Key (AutomationKeys.Control, AutomationKeys.plus);
				break;

			case "F5":
				if (args.State == ButtonStates.Press || args.State == ButtonStates.Hold)
					automation.Key (AutomationKeys.Left);
				break;

			case "F6":
				if (args.State == ButtonStates.Press || args.State == ButtonStates.Hold)
					automation.Key (AutomationKeys.Down);
				break;

			case "F7":
				if (args.State == ButtonStates.Press || args.State == ButtonStates.Hold)
					automation.Key (AutomationKeys.Right);
				break;

			case "F4":
				if (args.State == ButtonStates.Press || args.State == ButtonStates.Hold)
					automation.Key (AutomationKeys.a);
				break;

			case "F8":
				if (args.State == ButtonStates.Press || args.State == ButtonStates.Hold)
					automation.Key (AutomationKeys.d);
				break;

			default:
				base.DoAction (args);
				break;
			}
		}
    }
}