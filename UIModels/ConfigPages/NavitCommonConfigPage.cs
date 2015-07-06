using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.UI;

namespace UIModels.ConfigPages
{
    public class NavitCommonConfigPage : ModelBase
    {
        private const string YES = "Yes";
        private const string NO = "No";

        private readonly IHostController hc;
        private readonly IConfig cfg;

        public NavitCommonConfigPage(IHostController hc)
            :base("CommonVertcalStackPage", hc.Dispatcher, hc.Logger)
        {
            this.hc = hc;
            this.cfg = hc.Config;

            SetProperty("label_caption", "Navit common options");
            SetProperty(ModelNames.ButtonCancelLabel, "Return to Main Menu");
            SetProperty(ModelNames.ButtonAcceptLabel, "Go to Navit OSD Config");

            UpdateButtonLabels();
        }

        protected override void DoAction(Interfaces.UI.PageModelActionEventArgs e)
        {
            switch (e.ActionName)
            {
                case ModelNames.ButtonF1:
                    cfg.InvertBoolSetting(ConfigNames.NavitGPSActive);
                    break;

                case ModelNames.ButtonF2:
                    cfg.InvertBoolSetting(ConfigNames.NavitAutozoom);
                    break;

                case ModelNames.ButtonF3:
                    cfg.InvertBoolSetting(ConfigNames.NavitMenubar);
                    break;

                case ModelNames.ButtonF4:
                    cfg.InvertBoolSetting(ConfigNames.NavitToolbar);
                    break;

                case ModelNames.ButtonF5:
                    cfg.InvertBoolSetting(ConfigNames.NavitStatusbar);
                    break;

                case ModelNames.ButtonF6:
                    cfg.InvertBoolSetting(ConfigNames.NavitKeepNorth);
                    break;

                case ModelNames.ButtonF7:
                    cfg.InvertBoolSetting(ConfigNames.NavitLockOnRoad);
                    break;

                case ModelNames.ButtonF8:
                    var zoom = cfg.GetInt(ConfigNames.NavitZoom) + 15;
                    cfg.Set<int>(ConfigNames.NavitZoom, zoom > 256 ? 0 : zoom);
                    break;

                case ModelNames.ButtonAccept:
                    var nextPage = new NavitOSDConfigPage(hc);
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
            SetProperty(ModelNames.ButtonF1Label, string.Concat("Activate GPS: ", cfg.GetBool(ConfigNames.NavitGPSActive) ? YES : NO));
            SetProperty(ModelNames.ButtonF2Label, string.Concat("Autozoom: ", cfg.GetBool(ConfigNames.NavitAutozoom) ? YES : NO));
            SetProperty(ModelNames.ButtonF3Label, string.Concat("Show Menubar: ", cfg.GetBool(ConfigNames.NavitMenubar) ? YES : NO));
            SetProperty(ModelNames.ButtonF4Label, string.Concat("Show Toolbar: ", cfg.GetBool(ConfigNames.NavitToolbar) ? YES : NO));
            SetProperty(ModelNames.ButtonF5Label, string.Concat("Show Statusbar: ", cfg.GetBool(ConfigNames.NavitStatusbar) ? YES : NO));
            SetProperty(ModelNames.ButtonF6Label, string.Concat("Keep North orient: ", cfg.GetBool(ConfigNames.NavitKeepNorth) ? YES : NO));
            SetProperty(ModelNames.ButtonF7Label, string.Concat("Lock on Road: ", cfg.GetBool(ConfigNames.NavitLockOnRoad) ? YES : NO));
            SetProperty(ModelNames.ButtonF8Label, string.Concat("Zoom: ", cfg.GetInt(ConfigNames.NavitZoom).ToString()));
        }
    }
}
