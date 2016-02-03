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
        private bool inProgress;

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
            if (inProgress)
                return;

            switch (name)
            {
                case "QuickView":
                    {
                        var playerPage = hc.GetController<IUIController>().ShowPage("DashPlayer", null, CreatePlayerProcessRunner(fileInfo.FullName)) as ExternalApplicationPage;
                        playerPage.Run();
                    }
                    break;

                case "ProtectDeletion":
                    Exception error = null;
                    try
                    {
                        fileInfo = await Task.Run(() => hc.GetController<IDashCamController>().ProtectDeletion(fileInfo));
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

                case "ViewNormal":
                    {
                        inProgress = true;
                        var mp4FileInfo = await Task.Run(() => hc.GetController<IDashCamController>().GetMP4File(fileInfo));
                        inProgress = false;
                        if (!Disposed)
                        {
                            var playerPage = hc.GetController<IUIController>().ShowPage("DashPlayer", null, CreatePlayerProcessRunner(mp4FileInfo.FullName)) as ExternalApplicationPage;
                            playerPage.Run();
                        }
                    }
                    break;

                case "CopyExternal":
                    {
                        try
                        {
                            inProgress = true;
                            var mp4FileInfo = await Task.Run(() => hc.GetController<IDashCamController>().GetMP4File(fileInfo));
                            // TODO File.Copy()
                            inProgress = false;
                        }
                        catch (Exception ex)
                        {
                            hc.Logger.Log(this, ex);
                        }
                    }
                    break;

                case "Remove":
                    var dr = await hc.GetController<IUIController>().ShowDialogAsync(new YesNoDialog("Delete file", "Are you really want to delete this file?", "Delete", "Cancel", hc, 10000, DialogResults.No));
                    if (dr == DialogResults.Yes)
                    {
                        hc.GetController<IDashCamController>().Cleanup(fileInfo);
                        hc.SyncContext.Post((o) => Action(new PageModelActionEventArgs(ModelNames.ButtonCancel, Interfaces.Input.ButtonStates.Press)), null);
                    }
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

        private IProcessRunner CreatePlayerProcessRunner(string filePath)
        {
            var config = new ProcessConfig
            {
                ExePath = hc.Config.GetString(ConfigNames.DashCamPlayerExe),
                Args = string.Format(hc.Config.GetString(ConfigNames.DashCamPlayerArg), filePath),
                WaitForUI = false,
                RedirectStandardInput = false,
                RedirectStandardOutput = false
            };

            return hc.ProcessRunnerFactory.Create(config);
        }
    }
}
