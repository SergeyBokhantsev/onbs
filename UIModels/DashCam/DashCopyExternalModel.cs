using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.UI;
using System.IO;
using UIModels.MultipurposeModels;
using UIModels.Dialogs;
using System.Threading;

namespace UIModels
{
    public class DashCopyExternalModel : RotaryListModel<string>
    {
        private readonly FileInfo fileInfo;
        private CancellationTokenSource cts;

        private readonly List<ListItem<string>> items;

        public DashCopyExternalModel(string viewName, IHostController hc, MappedPage pageDescriptor, object arg)
            : base(viewName, hc, pageDescriptor, "list", 10)
        {
            fileInfo = arg as FileInfo;
            Ensure.ArgumentIsNotNull(fileInfo);

            var drives = Directory.GetLogicalDrives();
            ListItem<string>.PrepareItems(hc.SyncContext, ref items, drives, OnClick, drive => drive);

            UpdateInfo();
        }

        protected override IList<RotaryListModel<string>.ListItem<string>> QueryItems(int skip, int take)
        {
            return items.Skip(skip).Take(take).ToList();
        }

        protected override void DoAction(string name, PageModelActionEventArgs actionArgs)
        {
            if (name == "Cancel")
            {
                if (cts != null)
                {
                    cts.Cancel();
                }
                else
                {
                    hc.GetController<IUIController>().ShowPage("DashFileOptions", null, fileInfo);
                }
            }
            else
            {
                base.DoAction(name, actionArgs);
            }
        }

        private async void OnClick(object sender, EventArgs e)
        {
            Exception error = null;
            string destinationFilePath = null;

            try
            {
                cts = new CancellationTokenSource();

                var drivePath = ((ListItem<string>)sender).Value;

                UpdateInfo("Converting to MP4 file...");
                var mp4FileInfo = await Task.Run(() => hc.GetController<IDashCamController>().GetMP4File(fileInfo));
                
                destinationFilePath = Path.Combine(drivePath, mp4FileInfo.Name);

                if (File.Exists(destinationFilePath))
                {
                    File.Delete(destinationFilePath);
                    await Task.Delay(1000);
                }

                Action<int> progressAction = percent => UpdateInfo(string.Concat("Copied: ", percent, "%"));

                await Task.Run(() => hc.GetController<IDashCamController>().Copy(mp4FileInfo, destinationFilePath, cts.Token, progressAction));

                progressAction(100);
            }
            catch (OperationCanceledException ex)
            {
                hc.Logger.Log(this, ex);
                error = ex;
            }
            catch (Exception ex)
            {
                hc.Logger.Log(this, ex);
                error = ex;
            }
            finally
            {
                cts = null;
            }

            if (error is OperationCanceledException)
            {
                await hc.GetController<IUIController>().ShowDialogAsync(new OkDialog("File not copied", "Operation was cancelled", "Close", hc, 60000));
            }
            else if (error != null)
            {
                await hc.GetController<IUIController>().ShowDialogAsync(new OkDialog("Error", error.Message, "Close", hc, 60000));
            }
            else
            {
                await hc.GetController<IUIController>().ShowDialogAsync(new OkDialog("Copied succesfully", string.Concat("File was copied to ", destinationFilePath), "Ok", hc, 30000));
                hc.GetController<IUIController>().ShowPage("DashFileOptions", null, fileInfo);
            }
        }

        private void UpdateInfo(string caption = null)
        {
            SetProperty("file_name", caption ?? fileInfo.Name);
            SetProperty("file_props", string.Concat(((double)fileInfo.Length / 1000000d).ToString("0 Mb"), " Created: ", fileInfo.CreationTime));
        }
    }
}
