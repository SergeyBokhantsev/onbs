using Interfaces;
using Interfaces.UI;
using System;
using YandexServicesProvider;
using System.IO;

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
            this.provider = new StaticMapProvider(hc.Logger);

            SetProperty("daisy_path", Path.Combine(hc.Config.DataFolder, "loader_small.gif"));
        }

        protected override void DoAction(string name, PageModelActionEventArgs actionArgs)
        {
            switch (name)
            {
                case "Scale":
                    if (++scale == scales.Length) scale = 0;
                    SetProperty("traffic_image_stream", null);
                    RefreshTraffic();
                    break;

                case "Refresh":
                    RefreshTraffic();
                    break;

                default:
                    base.DoAction(name, actionArgs);
                    break;
            }
        }

        protected override void OnSecondaryTimer(IHostTimer timer)
        {
            RefreshTraffic();
            base.OnSecondaryTimer(timer);
        }

        private void RefreshTraffic()
        {
            downloadOperationScope.ExecuteIfFree(BeginDownload, OnDownloadException);
        }

        private void OnDownloadException(Exception ex)
        {
            SetProperty("status", "Unexpected error.");
            hc.Logger.Log(this, ex);
        }

        private async void BeginDownload()
        {
            if (hc.Config.IsInternetConnected)
            {
                var location = hc.GetController<IGPSController>().Location;
                SetProperty("status", "Loading...");
                SetProperty("traffic_image_stream", null);

                var stream = await provider.GetMapAsync(location, 600, 450, scales[scale], MapLayers.map | MapLayers.trf);
                
                if (stream != null)
                {
                    SetProperty("status", null);
                    SetProperty("traffic_image_stream", stream);
                    
                }
                else
                {
                    SetProperty("status", "Download error.");
                    SetProperty("traffic_image_stream", new MemoryStream(File.ReadAllBytes(Path.Combine(hc.Config.DataFolder, "error.png"))));
                }

                downloadOperationScope.Reset();
            }
        }
    }
}
