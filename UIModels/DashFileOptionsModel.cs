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
                    {
                        Exception error = null;
                        try
                        {
                            fileInfo = await Task.Run(() => hc.GetController<IDashCamController>().ProtectDeletion(fileInfo));
                        }
                        catch (Exception ex)
                        {
                            hc.Logger.Log(this, ex);
                            error = ex;
                        }

                        if (error != null)
                            await hc.GetController<IUIController>().ShowDialogAsync(new OkDialog("Error", error.Message, "Close", hc, 5000));
                    }

                    UpdateInfo();
                    break;

                case "ViewNormal":
                    {
                        SetInprogress(true);
                        var mp4FileInfo = await Task.Run(() => hc.GetController<IDashCamController>().GetMP4File(fileInfo));
                        SetInprogress(false);
                        if (!Disposed)
                        {
                            var playerPage = hc.GetController<IUIController>().ShowPage("DashPlayer", null, CreatePlayerProcessRunner(mp4FileInfo.FullName)) as ExternalApplicationPage;
                            playerPage.Run();
                        }
                    }
                    break;

                case "CopyExternal":
                    {
                        Exception error = null;
                        string destFilePath = null;

                        try
                        {
                            var externalDrive = hc.Config.GetString(ConfigNames.DashCamExternalStorageDrive);
                            if (!Directory.GetLogicalDrives().Any(d => d.Equals(externalDrive, StringComparison.InvariantCultureIgnoreCase)))
                            {
                                await hc.GetController<IUIController>().ShowDialogAsync(new OkDialog("Error", string.Format("Unable to save. No '{0}' drive exist.", externalDrive), "Close", hc, 5000));
                                return;
                            }

                            SetInprogress(true);
                            var mp4FileInfo = await Task.Run(() => hc.GetController<IDashCamController>().GetMP4File(fileInfo));
                            if (!Disposed)
                            {
                                SetInprogress(true, "Copying to the storage...");
                                destFilePath = await Task.Run(() => hc.GetController<IDashCamController>().Copy(mp4FileInfo, externalDrive));
                            }
                        }
                        catch (Exception ex)
                        {
                            hc.Logger.Log(this, ex);
                            error = ex;
                        }
                        finally
                        {
                            SetInprogress(false);
                        }

                        if (error != null)
                            await hc.GetController<IUIController>().ShowDialogAsync(new OkDialog("Error", error.Message, "Close", hc, 5000));
                        else
                            await hc.GetController<IUIController>().ShowDialogAsync(new OkDialog("Error", string.Concat("Copied successfully to ", destFilePath), "Ok", hc, 5000));
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

        private void SetInprogress(bool value, string message = null)
        {
            inProgress = value;

            if (inProgress)
                SetProperty("file_props", message ?? "Convertation in progress...");
            else
                UpdateInfo();
        }

        private void UpdateInfo()
        {
            SetProperty("file_name", string.Concat(fileInfo.Name, hc.GetController<IDashCamController>().IsProtected(fileInfo) ? " (PROTECTED)" : null));
            SetProperty("file_props", string.Concat(((double)fileInfo.Length / 1000000d).ToString("0 Mb"), " Created: ", fileInfo.CreationTime));
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
