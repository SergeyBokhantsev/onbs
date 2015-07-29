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

namespace UIModels
{
    public class TrafficPage : CommonPageBase
    {
        private readonly StaticMapProvider provider;

        private ManualResetGuard downloadOperationScope = new ManualResetGuard();

        public TrafficPage(IHostController hc)
            :base(hc, "TrafficPage")
        {
            this.provider = new StaticMapProvider(hc.Logger);
        }

        protected override void OnSecondaryTimer(object sender, EventArgs e)
        {
            downloadOperationScope.ExecuteIfFree(BeginDownload, OnDownloadException);
            base.OnSecondaryTimer(sender, e);
        }

        private void RefreshTraffic(object sender, EventArgs e)
        {
            downloadOperationScope.ExecuteIfFree(BeginDownload, OnDownloadException);
        }

        private void OnDownloadException(Exception ex)
        {
            hc.Logger.Log(this, ex);
        }

        private void BeginDownload()
        {
            if (hc.Config.IsInternetConnected)
            {
                var location = hc.GetController<IGPSController>().Location;
                provider.GetMapAsync(location, 600, 450, 12, MapLayers.map | MapLayers.trf,
                    mapStream =>
                    {
                        if (mapStream != null)
                        {
                            SetProperty("traffic_image_stream", mapStream);
                            downloadOperationScope.Reset();
                        }
                    });
            }
        }
    }
}
