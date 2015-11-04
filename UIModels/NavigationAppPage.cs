using Interfaces;
using Interfaces.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces.Input;

namespace UIModels
{
    public class NavigationAppPage : ExternalApplicationPage
    {
        private readonly IAutomationController automation;

        public NavigationAppPage(IHostController hostController)
            :base(typeof(ExternalApplicationPage).Name, 
            CreateProcessRunner(hostController),
            hostController.SyncContext,
            hostController.Logger,
            hostController.GetController<IUIController>())
        {
            automation = hostController.GetController<IAutomationController>();
        }

        private static IProcessRunner CreateProcessRunner(IHostController hc)
        {
            try
            {
                var config = hc.Config;

                var templatePath = config.GetString(ConfigNames.NavitConfigTemplatePath);
                var outFile = config.GetString(ConfigNames.NavitConfigPath);

                var navitConfig = new NavitConfigGenerator.NavitConfiguration();

                navitConfig.KeepNorthOrient = config.GetBool(ConfigNames.NavitKeepNorth);
                navitConfig.Autozoom = config.GetBool(ConfigNames.NavitAutozoom);
                navitConfig.GPSActive = config.GetBool(ConfigNames.NavitGPSActive);
                navitConfig.Menubar = config.GetBool(ConfigNames.NavitMenubar);
                navitConfig.Statusbar = config.GetBool(ConfigNames.NavitStatusbar);
                navitConfig.Toolbar = config.GetBool(ConfigNames.NavitToolbar);
				navitConfig.Zoom = config.GetInt(ConfigNames.NavitZoom);
				navitConfig.LockOnRoad = config.GetBool(ConfigNames.NavitLockOnRoad);
				navitConfig.OSDCompass = config.GetBool(ConfigNames.NavitOSDCompass);
				navitConfig.OSDETA = config.GetBool(ConfigNames.NavitOSDETA);
				navitConfig.OSDNavigation = config.GetBool(ConfigNames.NavitOSDNavigation);
				navitConfig.OSDNavigationDistanceToNext = config.GetBool(ConfigNames.NavitOSDNavigationDistanceToNext);
				navitConfig.OSDNavigationDistanceToTarget = config.GetBool(ConfigNames.NavitOSDNavigationDistanceToTarget);
				navitConfig.OSDNavigationNextTurn = config.GetBool(ConfigNames.NavitOSDNavigationNextTurn);

                navitConfig.Center = hc.GetController<IGPSController>().Location;

                navitConfig.WriteConfig(templatePath, outFile);

                var exe = config.GetString(ConfigNames.NavitExe);
                var args = string.Format(config.GetString(ConfigNames.NavitArgs), outFile);

                return hc.ProcessRunnerFactory.Create(exe, args, true);
            }
            catch (Exception ex)
            {
                hc.Logger.Log(hc, string.Concat("Cannot create Navit runner: ", ex.Message), LogLevels.Error);
                hc.Logger.Log(hc, ex);
                return null;
            }
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
