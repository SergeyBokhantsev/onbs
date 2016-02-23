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
        private FileInfo fileInfo;
        private bool inProgress;
        private bool cancelRequest;

        private readonly List<ListItem<string>> items;

        public DashCopyExternalModel(string viewName, IHostController hc, MappedPage pageDescriptor, object arg)
            : base(viewName, hc, pageDescriptor, "list", 10)
        {
            var fi = arg as FileInfo;
            if (fi != null)
                fileInfo = fi;

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
                if (inProgress)
                {
                    cancelRequest = true;
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
                cancelRequest = false;
                inProgress = true;

                var drivePath = ((ListItem<string>)sender).Value;

                destinationFilePath = Path.Combine(drivePath, fileInfo.Name);

                if (File.Exists(destinationFilePath))
                {
                    File.Delete(destinationFilePath);
                    await Task.Delay(1000);
                }

                await Task.Run(() =>
                {
                    long overalCopied = 0;
                    DateTime updateTime = DateTime.MinValue;
                    byte[] buffer = new byte[4096];
                    using (var stream = fileInfo.OpenRead())
                    {
                        using (var outStream = File.Create(destinationFilePath, buffer.Length, FileOptions.WriteThrough))
                        {
                            int readed = 1;

                            while (readed > 0 && !cancelRequest)
                            {
                                readed = stream.Read(buffer, 0, buffer.Length);
                                outStream.Write(buffer, 0, readed);
                                overalCopied += readed;

                                if (DateTime.Now > updateTime)
                                {
                                    var percent = (int)(((double)overalCopied / ((double)fileInfo.Length + 1)) * 100);
                                    UpdateInfo(string.Concat("Copied: ", percent));
                                    updateTime = DateTime.Now.AddMilliseconds(500);
                                }                                
                            }

                            UpdateInfo();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                hc.Logger.Log(this, ex);
                error = ex;
            }
            finally
            {
                inProgress = false;
            }

            if (cancelRequest)
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
