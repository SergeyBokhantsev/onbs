using Interfaces;
using Interfaces.UI;
using System;
using UIModels.MultipurposeModels;

namespace UIModels
{
    class CommonConfigPage : ConfigPageBase
    {
        private const string SaveAndReturn = "SaveAndReturn";
        private const string ChangeLogLevel = "ChangeLogLevel";

        public CommonConfigPage(string viewName, IHostController hc, MappedPage pageDescriptor)
            :base(viewName, hc, pageDescriptor)
        {
            SetProperty(ModelNames.PageTitle, "Common Configuration");
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

                case ChangeLogLevel:
                    var levelStr = hc.Config.GetString(ConfigNames.LogLevel);
                    var level = (LogLevels)Enum.Parse(typeof(LogLevels), levelStr);
                    level = ((int)level == 0) ? LogLevels.Debug : level - 1;
                    hc.Config.Set(ConfigNames.LogLevel, level.ToString());
                    UpdateLogLevelProperty();
                    break;

                default:
                    base.DoAction(name, actionArgs);
                    break;
            }
        }

        private void UpdateLogLevelProperty()
        {
            var levelStr = hc.Config.GetString(ConfigNames.LogLevel);
            UpdateLabelForAction(ChangeLogLevel, string.Concat("Log Level: ", levelStr));
        }
    }
}
