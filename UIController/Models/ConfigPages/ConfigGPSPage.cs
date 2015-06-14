﻿using Interfaces;
using Interfaces.Input;
using Interfaces.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIController.Models.ConfigPages
{
    public class ConfigGPSPage : ModelBase
    {
        private class CfgNames
        {
            public const string GPSDEnabled = "GPSDEnabled";
        }

        private readonly IHostController hostController;
        private readonly IConfig config;

        public ConfigGPSPage(IHostController hostController)
            : base("SystemConfigurationPage", hostController.Dispatcher, hostController.Logger)
        {
            this.hostController = hostController;
            this.config = hostController.Config;

            SetProperty("label_caption", "GPS Configuration");
            SetProperty(ModelNames.ButtonCancelLabel, "Return to Main Menu");
            
            SetProperty(ModelNames.ButtonAcceptLabel, "Go to Common Config");

            SetGPSdaemonProperty();
        }

        protected override void DoAction(PageModelActionEventArgs args)
        {
           switch (args.ActionName)
           {
               case ModelNames.ButtonCancel:
                   if (args.State == ButtonStates.Press)
                   {
                       hostController.Config.Save();
                       hostController.GetController<IUIController>().ShowMainPage();
                   }
                   break;

               case ModelNames.ButtonAccept:
                   if (args.State == ButtonStates.Press)
                   {
                       var page = new CommonConfigPage(hostController);
                       hostController.GetController<IUIController>().ShowPage(page);
                   }
                   break;

               case ModelNames.ButtonF1:
                   if (args.State == ButtonStates.Press)
                   {
                       var enabled = config.GetBool(CfgNames.GPSDEnabled);
                       config.Set(CfgNames.GPSDEnabled, !enabled);
                       SetGPSdaemonProperty();
                   }
                   break;
           }
        }

        private void SetGPSdaemonProperty()
        {
            var enabled = config.GetBool(CfgNames.GPSDEnabled);
            SetProperty(ModelNames.ButtonF1Label, string.Concat("GPS Daemon ", enabled ? "enabled" : "disabled"));
        }
    }
}
