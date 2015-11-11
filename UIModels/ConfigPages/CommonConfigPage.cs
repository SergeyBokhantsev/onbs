using Interfaces;
using Interfaces.Input;
using Interfaces.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIController;

namespace UIModels.ConfigPages
{
    class CommonConfigPage : ModelBase
    {
        private class CfgNames
        {
            public const string ArduinoPortFake = "ArduinoPortFake";
            public const string LogLevel = "LogLevel";
        }

        private const string SaveAndReturn = "SaveAndReturn";
        private const string ToggleArduinoMode = "ToggleArduinoMode";
        private const string ChangeLogLevel = "ChangeLogLevel";

        public CommonConfigPage(string viewName, IHostController hc, ApplicationMap map, object arg)
            :base(viewName, hc, map, arg)
        {
            SetProperty(ModelNames.PageTitle, "Common Configuration");
            UpdateUseArduinoPortFakeProperty();
            UpdateLogLevelProperty();
        }

        protected override void DoAction(string name, PageModelActionEventArgs actionArgs)
        {
            switch (name)
            {
                case SaveAndReturn:
                    hc.Config.Save();
                    hc.GetController<IUIController>().ShowDefaultPage();
                    break;

                case ToggleArduinoMode:
                    hc.Config.InvertBoolSetting(CfgNames.ArduinoPortFake);
                    UpdateUseArduinoPortFakeProperty();
                    break;

                case ChangeLogLevel:
                    var levelStr = hc.Config.GetString(CfgNames.LogLevel);
                    var level = (LogLevels)Enum.Parse(typeof(LogLevels), levelStr);
                    level = ((int)level == 0) ? LogLevels.Debug : level - 1;
                    hc.Config.Set<string>(CfgNames.LogLevel, level.ToString());
                    UpdateLogLevelProperty();
                    break;

                default:
                    base.DoAction(name, actionArgs);
                    break;
            }
        }

        private void UpdateUseArduinoPortFakeProperty()
        {
            var useFake = hc.Config.GetBool(CfgNames.ArduinoPortFake);
            map.UpdateLabelForAction(this, ToggleArduinoMode, string.Concat("Use fake arduino: ", useFake ? "Yes" : "No"));
        }

        private void UpdateLogLevelProperty()
        {
            var levelStr = hc.Config.GetString(CfgNames.LogLevel);
            map.UpdateLabelForAction(this, ChangeLogLevel, string.Concat("Log Level: ", levelStr));
        }
    }
}
