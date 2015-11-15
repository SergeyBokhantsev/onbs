using Interfaces;

using Interfaces.GPS;
using Interfaces.Input;
using Interfaces.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YandexServicesProvider;
using System.Reflection;
using System.IO;
using System.Threading;
using UIController;

namespace UIModels
{
    public class TrafficPage : CommonPageBase
    {
        private readonly StaticMapProvider provider;

        private ManualResetGuard downloadOperationScope = new ManualResetGuard();
        private int[] scales = new[] { 13, 12, 8 }; 
        private int scale = 0;

        public TrafficPage(string viewName, IHostController hc, MappedPage pageDescriptor, object arg)
            : base(viewName, hc, pageDescriptor)
        {
            this.provider = new StaticMapProvider(hc.Logger);
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
            downloadOperationScope.ExecuteIfFree(BeginDownload, OnDownloadException);
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

                var stream = await provider.GetMapAsync(location, 600, 450, scales[scale], MapLayers.map | MapLayers.trf);

                if (stream != null)
                {
                    SetProperty("status", null);
                    SetProperty("traffic_image_stream", stream);
                    downloadOperationScope.Reset();
                }
                else
                {
                    SetProperty("status", "Download error.");
                }
            }
        }
    }
}
