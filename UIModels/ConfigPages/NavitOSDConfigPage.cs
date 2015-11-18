using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.UI;
using UIController;

namespace UIModels
{
    class NavitOSDConfigPage : ModelBase
    {
        private const string YES = "Yes";
        private const string NO = "No";

        public NavitOSDConfigPage(string viewName, IHostController hc, MappedPage pageDescriptor)
            : base(viewName, hc, pageDescriptor)
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
            UpdateLabelForAction("Compass", string.Concat("Compass: ", hc.Config.GetBool(ConfigNames.NavitOSDCompass) ? YES : NO));
            UpdateLabelForAction("ETA", string.Concat("ETA: ", hc.Config.GetBool(ConfigNames.NavitOSDETA) ? YES : NO));
            UpdateLabelForAction("Navigation", string.Concat("Navigation: ", hc.Config.GetBool(ConfigNames.NavitOSDNavigation) ? YES : NO));
            UpdateLabelForAction("DistanceToNext", string.Concat("Distance to Next: ", hc.Config.GetBool(ConfigNames.NavitOSDNavigationDistanceToNext) ? YES : NO));
            UpdateLabelForAction("DistanceToTarget", string.Concat("Distance to Target: ", hc.Config.GetBool(ConfigNames.NavitOSDNavigationDistanceToTarget) ? YES : NO));
            UpdateLabelForAction("NextTurn", string.Concat("Next Turn: ", hc.Config.GetBool(ConfigNames.NavitOSDNavigationNextTurn) ? YES : NO));
        }
    }
}
