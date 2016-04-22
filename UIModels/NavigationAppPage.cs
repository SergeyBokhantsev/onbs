using Interfaces;
using Interfaces.UI;
using System;
using System.IO;
using UIModels.MiniDisplay;

namespace UIModels
{
    public sealed class NavigationAppPage : ExternalApplicationPage
    {
        private readonly IAutomationController automation;
        private readonly NavigationMiniDisplayModel miniDisplay;
        private IHostTimer timer;

        public NavigationAppPage(string viewName, IHostController hc, MappedPage pageDescriptor)
            : base(viewName, hc, pageDescriptor, CreateProcessRunner(hc))
        {
            automation = hc.GetController<IAutomationController>();
            miniDisplay = new NavigationMiniDisplayModel(hc, pageDescriptor.Name);

            this.Disposing += NavigationAppPage_Disposing;
        }

        void NavigationAppPage_Disposing(object sender, EventArgs e)
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }
        }

        private void OnTimer(IHostTimer obj)
        {
            if (obj.IsEnabled)
            {
                OnMiniDisplayUpdate();
            }
        }

        protected override void Initialize()
        {
            Run();
            timer = hc.CreateTimer(5000, OnTimer, true, true, "NavigationAppPage timer");
            base.Initialize();
        }
        
        private static IProcessRunner CreateProcessRunner(IHostController hc)
        {
            try
            {
                var config = hc.Config;
                
                var templatePath = Path.Combine(hc.Config.DataFolder, config.GetString(ConfigNames.NavitConfigTemplatePath));
				var outFile = Path.Combine(hc.Config.DataFolder, config.GetString(ConfigNames.NavitConfigPath));

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

                var processConfig = new ProcessConfig
                {
                    ExePath = config.GetString(ConfigNames.NavitExe),
                    Args = string.Format(config.GetString(ConfigNames.NavitArgs), outFile),
                    WaitForUI = true,
					RedirectStandardOutput = false,
					RedirectStandardInput = false
                };

                return hc.ProcessRunnerFactory.Create(processConfig);
            }
            catch (Exception ex)
            {
                hc.Logger.Log(hc, string.Concat("Cannot create Navit runner: ", ex.Message), LogLevels.Error);
                hc.Logger.Log(hc, ex);
                return null;
            }
        }

        protected override void DoAction(string name, PageModelActionEventArgs actionArgs)
        {
            switch(name)
            { 
			case "WheelUp":
				automation.MouseClick (AutomationMouseClickTypes.WheelUp);
				break;

			case "WheelDown":
				automation.MouseClick (AutomationMouseClickTypes.WheelDown);
				break;

                case "+":
                    automation.Key(AutomationKeys.Control, AutomationKeys.plus);
                    break;

                case "-":
                    automation.Key(AutomationKeys.Control, AutomationKeys.minus);
                    break;

                case "Up":
                    automation.Key(AutomationKeys.Up);
                    break;

                case "Down":
                    automation.Key(AutomationKeys.Down);
                    break;

                case "Left":
                    automation.Key(AutomationKeys.Left);
                    break;

                case "Right":
                    automation.Key(AutomationKeys.Right);
                    break;

                case "a":
                    automation.Key(AutomationKeys.a);
                    break;

                case "d":
                    automation.Key(AutomationKeys.d);
                    break;

                case "GPSPause":
                    hc.Config.InvertBoolSetting(ConfigNames.GPSDPaused);
                    OnMiniDisplayUpdate();
                    break;

                default:
                    base.DoAction(name, actionArgs);
                    break;
            }
        }

        private void OnMiniDisplayUpdate()
        {
            miniDisplay.Draw();
            hc.Logger.LogIfDebug(this, "OnMiniDisplayUpdate");
        }
    }
}
