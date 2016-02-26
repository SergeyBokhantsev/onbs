using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.UI;
using UIModels.Dialogs;
using UIModels.MultipurposeModels;

namespace UIModels
{
    public class DashFileOptionsModel : RotaryListModel<string>
    {
        private static FileInfo fileInfo;
        private bool inProgress;

        private readonly List<ListItem<string>> items;

        public DashFileOptionsModel(string viewName, IHostController hc, MappedPage pageDescriptor, object arg)
            : base(viewName, hc, pageDescriptor, "list", 10)
        {
            var fi = arg as FileInfo;
            if (fi != null)
                fileInfo = fi;

            Ensure.ArgumentIsNotNull(fileInfo);

            ListItem<string>.PrepareItem(hc.SyncContext, ref items, "QuickView", OnClick, "Quick view");
            ListItem<string>.PrepareItem(hc.SyncContext, ref items, "ProtectDeletion", OnClick, "Protect deletion");
            ListItem<string>.PrepareItem(hc.SyncContext, ref items, "ViewNormal", OnClick, "View normally");
            ListItem<string>.PrepareItem(hc.SyncContext, ref items, "CopyExternal", OnClick, "Copy to External storage");
            ListItem<string>.PrepareItem(hc.SyncContext, ref items, "Remove", OnClick, "Remove this video");

            UpdateInfo();
        }

        private async void OnClick(object sender, EventArgs e)
        {
            if (inProgress)
                return;

            switch (((ListItem<string>)sender).Value)
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
                        hc.GetController<IUIController>().ShowPage("DashCopyExternal", null, fileInfo);
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
                RedirectStandardInput = true,
                RedirectStandardOutput = false
            };

            return hc.ProcessRunnerFactory.Create(config);
        }

        protected override IList<RotaryListModel<string>.ListItem<string>> QueryItems(int skip, int take)
        {
            if (skip == 0)
                return items;
            else
                return null;
        }
    }
}
