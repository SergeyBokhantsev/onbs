using Interfaces;
using Interfaces.UI;
using ProcessRunnerNamespace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIModels.MultipurposeModels;

namespace UIModels
{
    class ZXGamesListModel : RotaryListModel<string>
    {
		private static int focused = 0;

        private readonly List<ListItem<string>> list;

        public ZXGamesListModel(string viewName, IHostController hc, MappedPage pageDescriptor)
            : base(viewName, hc, pageDescriptor, "list", 10, focused)
        {
            SetProperty(ModelNames.PageTitle, "ZX Games list");
            list = CreateList();

			this.Disposing += (s, e) => focused = SelectedIndexAbsolute;
        }

        private List<ListItem<string>> CreateList()
        {
            var res = new List<ListItem<string>>();

            var romFolders = Path.Combine(hc.Config.DataFolder, "ZXRoms");

            if (Directory.Exists(romFolders))
            {
				ListItem<string>.PrepareItems(hc.SyncContext, ref res, Directory.GetFiles(romFolders).OrderBy(s => s), ItemSelected, GetItemCaption);
            }

            return res;
        }

		private string GetItemCaption(string fname)
		{
			string result;
			var name = Path.GetFileName (fname);

			if (!string.IsNullOrWhiteSpace (name)) 
			{
				result = name.Substring (0, Math.Min (name.Length, 40));
			} 
			else 
			{
				result = "NO NAME";
			}

			if (string.IsNullOrWhiteSpace (result))
				throw new NullReferenceException (fname);

			return result;
		}

        private void ItemSelected(object sender, EventArgs e)
        {
           var item = (ListItem<string>)sender;
			var page = hc.GetController<IUIController>().ShowPage("ZXEmulator", null, CreateProcessRunner(item.Value)) as ExternalApplicationPage;
			page.Run ();
        }

        private ProcessRunner CreateProcessRunner(string romFilePath)
        {
            return ProcessRunner.ForInteractiveApp(hc.Config.GetString(ConfigNames.ZXEmulatorExe),
                string.Format(hc.Config.GetString(ConfigNames.ZXEmulatorArgs), romFilePath));
        }

        protected override IList<ListItem<string>> QueryItems(int skip, int take)
        {
            return list.Skip(skip).Take(take).ToList();
        }
    }
}
