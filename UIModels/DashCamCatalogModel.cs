using Interfaces;
using Interfaces.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIModels.Dialogs;

namespace UIModels
{
    public class DashCamCatalogModel : ModelBase
    {
        private readonly FileInfo[] fileInfo;
        private static int skip;

        public DashCamCatalogModel(string viewName, IHostController hc, MappedPage pageDescriptor)
            : base(viewName, hc, pageDescriptor)
        {
            SetProperty(ModelNames.PageTitle, "Dash camera files");

            hc.Config.Set(ConfigNames.DashCamRecorderEnabled, false);

            fileInfo = hc.GetController<IDashCamController>().GetVideoFilesInfo();

            FillList();

            Disposing += DashCamCatalogModel_Disposing;
        }

        private async void DashCamCatalogModel_Disposing(object sender, EventArgs e)
        {
            var dr = await hc.GetController<IUIController>().ShowDialogAsync(new YesNoDialog("Resume recording", "Do you want to resume video recording?", "Resume", "No", hc, 10000, DialogResults.No));
            if (dr == DialogResults.Yes)
            {
                hc.Config.Set(ConfigNames.DashCamRecorderEnabled, true);
            }
        }

        private void FillList()
        {
            for (int i=0; i<8; ++i)
            {
                string label = "  ---";

                if (i + skip < fileInfo.Length)
                {
                    label = fileInfo[i + skip].Name; 
                }

                UpdateLabelForAction(i.ToString(), label);
            }
        }

        protected override void DoAction(string name, PageModelActionEventArgs actionArgs)
        {
            switch (name)
            {
                case "More":
                    if (skip + 8 < fileInfo.Length)
                        skip += 8;
                    else
                        skip = 0;
                    FillList();
                    break;

                case "0":
                case "1":
                case "2":
                case "3":
                case "4":
                case "5":
                case "6":
                case "7":
                    var index = skip + int.Parse(name);
                    if (fileInfo.Length > index)
                    {
                        Disposing -= DashCamCatalogModel_Disposing;
                        hc.GetController<IUIController>().ShowPage("DashFileOptions", null, fileInfo[index]);
                    }
                    break;

                default:
                    base.DoAction(name, actionArgs);
                    break;
            }
        }
    }
}
