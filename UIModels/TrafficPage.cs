using Interfaces;
using Interfaces.UI;
using System;
using YandexServicesProvider;
using System.IO;
using System.Threading.Tasks;

namespace UIModels
{
    public class TrafficPage : DrivePageBase
    {
        private readonly StaticMapProvider provider;

        private readonly ManualResetGuard downloadOperationScope = new ManualResetGuard();
        private readonly int[] scales = new[] { 13, 12, 8 }; 
        private int scale;

        public TrafficPage(string viewName, IHostController hc, MappedPage pageDescriptor)
            : base(viewName, hc, pageDescriptor)
        {
            this.provider = new StaticMapProvider();

            SetProperty("daisy_path", Path.Combine(hc.Config.DataFolder, "loader_small.gif"));
        }

		protected override async Task DoAction(string name, PageModelActionEventArgs actionArgs)
        {
            switch (name)
            {
                case "Scale":
                    if (++scale == scales.Length) scale = 0;
                    SetProperty("traffic_image_stream", null);
					await RefreshTraffic();
                    break;

                case "Refresh":
					await RefreshTraffic();
                    break;

			default:
				await base.DoAction (name, actionArgs);
				break;
            }
        }

        protected override async Task OnSecondaryTimer()
        {
            await RefreshTraffic();
            await base.OnSecondaryTimer();
        }

		private async Task RefreshTraffic()
        {
			await downloadOperationScope.ExecuteIfFreeAsync(() => BeginDownload(), OnDownloadException);
        }

        private void OnDownloadException(Exception ex)
        {
            SetProperty("status", "Unexpected error.");
            hc.Logger.Log(this, ex);
        }

        private async Task BeginDownload()
        {
            if (hc.Config.IsInternetConnected)
            {
                var location = hc.GetController<IGPSController>().Location;
                SetProperty("status", "Loading...");
                SetProperty("traffic_image_stream", null);

                var result = await provider.GetMapAsync(location, 600, 450, scales[scale], MapLayers.map | MapLayers.trf);

                if (result.Success)
                {
                    SetProperty("status", null);
                    SetProperty("traffic_image_stream", result.Value);
                }
                else
                {
                    SetProperty("status", result.ErrorMessage);
                    SetProperty("traffic_image_stream", new MemoryStream(File.ReadAllBytes(Path.Combine(hc.Config.DataFolder, "error.png"))));
                }

                downloadOperationScope.Reset();
            }
        }
    }
}
