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
    class NavitOSDConfigPage : ModelBase
    {
        private const string YES = "Yes";
        private const string NO = "No";

        public NavitOSDConfigPage(string viewName, IHostController hc, ApplicationMap map, object arg)
            : base(viewName, hc, map)
        {
            SetProperty(ModelNames.PageTitle, "Navit OSD configuration");            
            UpdateButtonLabels();
        }

        protected override void DoAction(string name, PageModelActionEventArgs actionArgs)
        {
            switch (name)
            {
                case "Compass":
                    hc.Config.InvertBoolSetting(ConfigNames.NavitOSDCompass);
                    break;

                case "ETA":
                    hc.Config.InvertBoolSetting(ConfigNames.NavitOSDETA);
                    break;

                case "Navigation":
                    hc.Config.InvertBoolSetting(ConfigNames.NavitOSDNavigation);
                    break;

                case "DistanceToNext":
                    hc.Config.InvertBoolSetting(ConfigNames.NavitOSDNavigationDistanceToNext);
                    break;

                case "DistanceToTarget":
                    hc.Config.InvertBoolSetting(ConfigNames.NavitOSDNavigationDistanceToTarget);
                    break;

                case "NextTurn":
                    hc.Config.InvertBoolSetting(ConfigNames.NavitOSDNavigationNextTurn);
                    break;
            }

            UpdateButtonLabels();
        }

        private void UpdateButtonLabels()
        {
            map.UpdateLabelForAction(this, "Compass", string.Concat("Compass: ", hc.Config.GetBool(ConfigNames.NavitOSDCompass) ? YES : NO));
            map.UpdateLabelForAction(this, "ETA", string.Concat("ETA: ", hc.Config.GetBool(ConfigNames.NavitOSDETA) ? YES : NO));
            map.UpdateLabelForAction(this, "Navigation", string.Concat("Navigation: ", hc.Config.GetBool(ConfigNames.NavitOSDNavigation) ? YES : NO));
            map.UpdateLabelForAction(this, "DistanceToNext", string.Concat("Distance to Next: ", hc.Config.GetBool(ConfigNames.NavitOSDNavigationDistanceToNext) ? YES : NO));
            map.UpdateLabelForAction(this, "DistanceToTarget", string.Concat("Distance to Target: ", hc.Config.GetBool(ConfigNames.NavitOSDNavigationDistanceToTarget) ? YES : NO));
            map.UpdateLabelForAction(this, "NextTurn", string.Concat("Next Turn: ", hc.Config.GetBool(ConfigNames.NavitOSDNavigationNextTurn) ? YES : NO));
        }
    }
}
