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

        internal static FileInfo SelectedFile;

        public DashCamCatalogModel(string viewName, IHostController hc, MappedPage pageDescriptor)
            : base(viewName, hc, pageDescriptor)
        {
            SetProperty(ModelNames.PageTitle, "Dash camera files");

            var dcc = hc.GetController<IDashCamController>();
            dcc.Stop();

            fileInfo = dcc.GetVideoFilesInfo();

            FillList();

            this.Disposing += DashCamCatalogModel_Disposing;
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
                        SelectedFile = fileInfo[index];
                        this.Disposing -= DashCamCatalogModel_Disposing;
                        var playerModel = hc.GetController<IUIController>().ShowPage("DashPlayer", null) as DashPlayerModel;
                        playerModel.Run();
                    }
                    break;

                default:
                    base.DoAction(name, actionArgs);
                    break;
            }
        }

        async void DashCamCatalogModel_Disposing(object sender, EventArgs e)
        {
            var dr = await hc.GetController<IUIController>().ShowDialogAsync(new YesNoDialog("Resume recording", "Do you want to resume video recording?", "Resume", "No", hc, 10000, DialogResults.No));

            if (dr == DialogResults.Yes)
            {
                hc.GetController<IDashCamController>().StartRecording();
            }
        }
    }
}
