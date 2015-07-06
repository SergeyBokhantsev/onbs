using Interfaces;
using Interfaces.Input;
using Interfaces.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIModels.ConfigPages
{
    class CommonConfigPage : ModelBase
    {
        private class CfgNames
        {
            public const string ArduinoPortFake = "ArduinoPortFake";
            public const string LogLevel = "LogLevel";
        }

        private readonly IHostController hostController;
        private readonly IConfig config;

        public CommonConfigPage(IHostController hostController)
            : base("CommonVertcalStackPage", hostController.Dispatcher, hostController.Logger)
        {
            this.hostController = hostController;
            this.config = hostController.Config;

            SetProperty("label_caption", "Common Configuration");
            SetProperty(ModelNames.ButtonCancelLabel, "Return to Main Menu");
            SetProperty(ModelNames.ButtonAcceptLabel, "Go to GPS Config");

            SetUseArduinoPortFakeProperty();
            SetLogLevelProperty();
        }

        protected override void DoAction(PageModelActionEventArgs args)
        {
            switch (args.ActionName)
            {
                case ModelNames.ButtonCancel:
                    if (args.State == ButtonStates.Press)
                    {
                        hostController.Config.Save();
                        hostController.GetController<IUIController>().ShowDefaultPage();
                    }
                    break;

                case ModelNames.ButtonAccept:
                    if (args.State == ButtonStates.Press)
                    {
                        var page = new ConfigGPSPage(hostController);
                        hostController.GetController<IUIController>().ShowPage(page);
                    }
                    break;

                case ModelNames.ButtonF1:
                    if (args.State == ButtonStates.Press)
                    {
                        var useFake = config.GetBool(CfgNames.ArduinoPortFake);
                        config.Set(CfgNames.ArduinoPortFake, !useFake);
                        SetUseArduinoPortFakeProperty();
                    }
                    break;

                case ModelNames.ButtonF2:
                    if (args.State == ButtonStates.Press)
                    {
                        var levelStr = config.GetString(CfgNames.LogLevel);
                        var level = (LogLevels)Enum.Parse(typeof(LogLevels), levelStr);
                        level = ((int)level == 0) ? LogLevels.Debug : level - 1;
                        config.Set<string>(CfgNames.LogLevel, level.ToString());
                        SetLogLevelProperty();
                    }
                    break;
            }
        }

        private void SetUseArduinoPortFakeProperty()
        {
            var useFake = config.GetBool(CfgNames.ArduinoPortFake);
            SetProperty(ModelNames.ButtonF1Label, string.Concat("Use fake arduino: ", useFake ? "Yes" : "No"));
        }

        private void SetLogLevelProperty()
        {
            var levelStr = config.GetString(CfgNames.LogLevel);
            SetProperty(ModelNames.ButtonF2Label, string.Concat("Log Level: ", levelStr));
        }
    }
}
