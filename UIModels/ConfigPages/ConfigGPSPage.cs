﻿using Interfaces;
using Interfaces.Input;
using Interfaces.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIController;

namespace UIModels
{
    public class ConfigGPSPage : ModelBase
    {
        private class CfgNames
        {
            public const string GPSDEnabled = "GPSDEnabled";
        }

        private const string ToggleGPSDaemon = "ToggleGPSDaemon";

        public ConfigGPSPage(string viewName, IHostController hc, MappedPage pageDescriptor, object arg)
            :base(viewName, hc, pageDescriptor)
        {
            SetProperty(ModelNames.PageTitle, "GPS Configuration");

            UpdateGPSdaemonProperty();
        }

        protected override void DoAction(string name, PageModelActionEventArgs actionArgs)
        {
            switch (name)
            { 
                case ToggleGPSDaemon:
                    hc.Config.InvertBoolSetting(CfgNames.GPSDEnabled);
                    UpdateGPSdaemonProperty();
                    break;

                default:
                    base.DoAction(name, actionArgs);
                    break;
            }
        }

        private void UpdateGPSdaemonProperty()
        {
            var enabled = hc.Config.GetBool(CfgNames.GPSDEnabled);
            UpdateLabelForAction(ToggleGPSDaemon, string.Concat("GPS Daemon ", enabled ? "enabled" : "disabled"));
        }
    }
}
