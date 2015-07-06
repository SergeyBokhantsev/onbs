using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.UI;

namespace UIModels.ConfigPages
{
    class NavitOSDConfigPage : ModelBase
    {
        private const string YES = "Yes";
        private const string NO = "No";

        private readonly IHostController hc;
        private readonly IConfig cfg;

        public NavitOSDConfigPage(IHostController hc)
            : base("CommonVertcalStackPage", hc.Dispatcher, hc.Logger)
        {
            this.hc = hc;
            this.cfg = hc.Config;

            SetProperty("label_caption", "Navit OSD configuration");
            SetProperty(ModelNames.ButtonCancelLabel, "Return to Main Menu");
            SetProperty(ModelNames.ButtonAcceptLabel, "Go to common config");

            UpdateButtonLabels();
        }

        protected override void DoAction(PageModelActionEventArgs e)
        {
            switch (e.ActionName)
            {
                case ModelNames.ButtonF1:
                    cfg.InvertBoolSetting(ConfigNames.NavitOSDCompass);
                    break;

                case ModelNames.ButtonF2:
                    cfg.InvertBoolSetting(ConfigNames.NavitOSDETA);
                    break;

                case ModelNames.ButtonF3:
                    cfg.InvertBoolSetting(ConfigNames.NavitOSDNavigation);
                    break;

                case ModelNames.ButtonF4:
                    cfg.InvertBoolSetting(ConfigNames.NavitOSDNavigationDistanceToNext);
                    break;

                case ModelNames.ButtonF5:
                    cfg.InvertBoolSetting(ConfigNames.NavitOSDNavigationDistanceToTarget);
                    break;

                case ModelNames.ButtonF6:
                    cfg.InvertBoolSetting(ConfigNames.NavitOSDNavigationNextTurn);
                    break;

                case ModelNames.ButtonAccept:
                    var nextPage = new CommonConfigPage(hc);
                    hc.GetController<IUIController>().ShowPage(nextPage);
                    break;

                case ModelNames.ButtonCancel:
                    cfg.Save();
                    hc.GetController<IUIController>().ShowDefaultPage();
                    break;
            }

            UpdateButtonLabels();
        }

        private void UpdateButtonLabels()
        {
            SetProperty(ModelNames.ButtonF1Label, string.Concat("Compass: ", cfg.GetBool(ConfigNames.NavitOSDCompass) ? YES : NO));
            SetProperty(ModelNames.ButtonF2Label, string.Concat("ETA: ", cfg.GetBool(ConfigNames.NavitOSDETA) ? YES : NO));
            SetProperty(ModelNames.ButtonF3Label, string.Concat("Navigation: ", cfg.GetBool(ConfigNames.NavitOSDNavigation) ? YES : NO));
            SetProperty(ModelNames.ButtonF4Label, string.Concat("Distance to Next: ", cfg.GetBool(ConfigNames.NavitOSDNavigationDistanceToNext) ? YES : NO));
            SetProperty(ModelNames.ButtonF5Label, string.Concat("Distance to Target: ", cfg.GetBool(ConfigNames.NavitOSDNavigationDistanceToTarget) ? YES : NO));
            SetProperty(ModelNames.ButtonF6Label, string.Concat("Next Turn: ", cfg.GetBool(ConfigNames.NavitOSDNavigationNextTurn) ? YES : NO));
        }
    }
}
