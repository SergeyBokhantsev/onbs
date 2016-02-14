using Interfaces;
using Interfaces.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIModels.Dialogs;
using UIModels.MultipurposeModels;

namespace UIModels
{
    public class DashCamCatalogModel : RotaryListModel<FileInfo>
    {
        private readonly FileInfo[] files;
        private static int selectedIndex;

        private bool leavingDashCam = true;

        public DashCamCatalogModel(string viewName, IHostController hc, MappedPage pageDescriptor)
            : base(viewName, hc, pageDescriptor, "list", 10, selectedIndex)
        {
            SetProperty(ModelNames.PageTitle, "Dash camera files");

            hc.Config.Set(ConfigNames.DashCamRecorderEnabled, false);

            files = hc.GetController<IDashCamController>().GetVideoFilesInfo();

            Disposing += DashCamCatalogModel_Disposing;
        }

        protected override IList<ListItem<FileInfo>> QueryItems(int skip, int take)
        {
            List<ListItem<FileInfo>> res = null;

            ListItem<FileInfo>.PrepareItems(hc.SyncContext, ref res, files.Skip(skip).Take(take), ClickHandler, fi => fi.Name);

            return res;
        }

        private void ClickHandler(object sender, EventArgs e)
        {
            var fileInfoItem = (RotaryListModel<FileInfo>.ListItem<FileInfo>)sender;
            leavingDashCam = false;
            hc.GetController<IUIController>().ShowPage("DashFileOptions", null, fileInfoItem.Value);
        }

        private async void DashCamCatalogModel_Disposing(object sender, EventArgs e)
        {
            selectedIndex = this.SelectedIndexAbsolute;

            if (leavingDashCam)
            {
                selectedIndex = 0;

                var dr = await hc.GetController<IUIController>().ShowDialogAsync(new YesNoDialog("Resume recording", "Do you want to resume video recording?", "Resume", "No", hc, 10000, DialogResults.No));
                if (dr == DialogResults.Yes)
                {
                    hc.Config.Set(ConfigNames.DashCamRecorderEnabled, true);
                }
            }
        }
    }
}
