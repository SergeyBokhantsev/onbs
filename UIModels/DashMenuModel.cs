using Interfaces;
using Interfaces.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIModels
{
    public class DashMenuModel : ModelBase
    {
        public DashMenuModel(string viewName, IHostController hc, MappedPage pageDescriptor)
            : base(viewName, hc, pageDescriptor)
        {
            hc.Config.Changed += Config_Changed;
            this.Disposing += DashMenuModel_Disposing;

            UpdateLabels();
        }

        void DashMenuModel_Disposing(object sender, EventArgs e)
        {
            hc.Config.Changed -= Config_Changed;
        }

        void Config_Changed(string obj)
        {
            UpdateLabels();
        }

        protected override void DoAction(string name, PageModelActionEventArgs actionArgs)
        {
            switch(name)
            {
                case "ToggleDashcamEnable":
                    hc.Config.InvertBoolSetting(ConfigNames.DashCamRecorderEnabled);
                    UpdateLabels();
                    break;

                case "ToggleDashcamPreview":
                    hc.Config.InvertBoolSetting(ConfigNames.DashCamRecorderPreviewEnabled);
                    UpdateLabels();
                    break;

                default:
                    base.DoAction(name, actionArgs);
                    break;
            }
        }

        private void UpdateLabels()
        {
            UpdateLabelForAction("ToggleDashcamEnable", string.Concat("DashCam ", hc.Config.GetBool(ConfigNames.DashCamRecorderEnabled) ? "enabled" : "DISABLED"));
            UpdateLabelForAction("ToggleDashcamPreview", string.Concat("DashCam preview: ", hc.Config.GetBool(ConfigNames.DashCamRecorderPreviewEnabled) ? "on" : "off"));
        }
    }
}
