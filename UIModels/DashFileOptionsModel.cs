using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.UI;
using UIModels.Dialogs;

namespace UIModels
{
    public class DashFileOptionsModel : ModelBase
    {
        private static FileInfo fileInfo;

        public DashFileOptionsModel(string viewName, IHostController hc, MappedPage pageDescriptor, object arg)
            : base(viewName, hc, pageDescriptor)
        {
            var fi = arg as FileInfo;
            if (fi != null)
                fileInfo = fi;

            Ensure.ArgumentIsNotNull(fileInfo);

            UpdateInfo();
        }

        protected override async void DoAction(string name, PageModelActionEventArgs actionArgs)
        {
            switch (name)
            {
                case "QuickView":
                    var playerPage = hc.GetController<IUIController>().ShowPage("DashPlayer", null, CreateQuickViewProcessRunner()) as ExternalApplicationPage;
                    playerPage.Run();
                    break;

                case "ProtectDeletion":
                    Exception error = null;
                    try
                    {
                        await Task.Run(() => hc.GetController<IDashCamController>().ProtectDeletion(fileInfo));
                    }
                    catch(Exception ex)
                    {
                        hc.Logger.Log(this, ex);
                        error = ex;
                    }

                    if (error != null)
                        await hc.GetController<IUIController>().ShowDialogAsync(new OkDialog("Error", error.Message, "Close", hc, 5000));

                    UpdateInfo();
                    break;

                default:
                    base.DoAction(name, actionArgs);
                    break;
            }
        }

        private void UpdateInfo()
        {
            SetProperty("file_name", string.Concat(fileInfo.Name, hc.GetController<IDashCamController>().IsProtected(fileInfo) ? " (PROTECTED)" : null));
            SetProperty("file_props", ((double)fileInfo.Length / 1000000d).ToString("0 Mb"));
        }

        private IProcessRunner CreateQuickViewProcessRunner()
        {
            var config = new ProcessConfig
            {
                ExePath = hc.Config.GetString(ConfigNames.DashCamQuickPlay_h264Exe),
                Args = string.Format(hc.Config.GetString(ConfigNames.DashCamQuickPlay_h264Arg), fileInfo.FullName),
                WaitForUI = false,
                RedirectStandardInput = false,
                RedirectStandardOutput = false
            };

            return hc.ProcessRunnerFactory.Create(config);
        }
    }
}
