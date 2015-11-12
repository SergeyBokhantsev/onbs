using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.UI;
using UIController;

namespace UIModels.ConfigPages
{
    public class NavitCommonConfigPage : ModelBase
    {
        private const string YES = "Yes";
        private const string NO = "No";

        public NavitCommonConfigPage(string viewName, IHostController hc, ApplicationMap map, object arg)
            : base(viewName, hc, map)
        {
            SetProperty(ModelNames.PageTitle, "Navit common options");            
            UpdateButtonLabels();
        }

        protected override void DoAction(string name, PageModelActionEventArgs actionArgs)
        {
            switch (name)
            {
                case "ActivateGPS":
                    hc.Config.InvertBoolSetting(ConfigNames.NavitGPSActive);
                    break;

                case "Autozoom":
                    hc.Config.InvertBoolSetting(ConfigNames.NavitAutozoom);
                    break;

                case "Menubar":
                    hc.Config.InvertBoolSetting(ConfigNames.NavitMenubar);
                    break;

                case "Toolbar":
                    hc.Config.InvertBoolSetting(ConfigNames.NavitToolbar);
                    break;

                case "Statusbar":
                    hc.Config.InvertBoolSetting(ConfigNames.NavitStatusbar);
                    break;

                case "KeepNorth":
                    hc.Config.InvertBoolSetting(ConfigNames.NavitKeepNorth);
                    break;

                case "LockRoad":
                    hc.Config.InvertBoolSetting(ConfigNames.NavitLockOnRoad);
                    break;

                case "Zoom":
                    var zoom = hc.Config.GetInt(ConfigNames.NavitZoom) + 15;
                    hc.Config.Set<int>(ConfigNames.NavitZoom, zoom > 256 ? 0 : zoom);
                    break;

                default:
                    base.DoAction(name, actionArgs);
                    break;
            }

            UpdateButtonLabels();
        }

        private void UpdateButtonLabels()
        {
            map.UpdateLabelForAction(this, "ActivateGPS", string.Concat("Activate GPS: ", hc.Config.GetBool(ConfigNames.NavitGPSActive) ? YES : NO));
            map.UpdateLabelForAction(this, "Autozoom", string.Concat("Autozoom: ", hc.Config.GetBool(ConfigNames.NavitAutozoom) ? YES : NO));
            map.UpdateLabelForAction(this, "Menubar", string.Concat("Show Menubar: ", hc.Config.GetBool(ConfigNames.NavitMenubar) ? YES : NO));
            map.UpdateLabelForAction(this, "Toolbar", string.Concat("Show Toolbar: ", hc.Config.GetBool(ConfigNames.NavitToolbar) ? YES : NO));
            map.UpdateLabelForAction(this, "Statusbar", string.Concat("Show Statusbar: ", hc.Config.GetBool(ConfigNames.NavitStatusbar) ? YES : NO));
            map.UpdateLabelForAction(this, "KeepNorth", string.Concat("Keep North orient: ", hc.Config.GetBool(ConfigNames.NavitKeepNorth) ? YES : NO));
            map.UpdateLabelForAction(this, "LockRoad", string.Concat("Lock on Road: ", hc.Config.GetBool(ConfigNames.NavitLockOnRoad) ? YES : NO));
            map.UpdateLabelForAction(this, "Zoom", string.Concat("Zoom: ", hc.Config.GetInt(ConfigNames.NavitZoom).ToString()));
        }
    }
}
