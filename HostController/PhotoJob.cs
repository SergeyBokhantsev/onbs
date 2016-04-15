using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace HostController
{
    public class PhotoJob
    {
        private readonly IHostTimer jobTimer;
        private readonly IHostController hc;

        private int PeriodMs
        {
            get
            {
                return hc.Config.GetInt(ConfigNames.PhotoJobPeriodMinutes) * 60000;
            }
        }

        private bool Enabled
        {
            get
            {
                return hc.Config.GetBool(ConfigNames.PhotoJobEnabled);
            }
        }

        public PhotoJob(IHostController hc)
        {
            this.hc = hc;
            jobTimer = hc.CreateTimer(PeriodMs, TakePhoto, Enabled, false, "PhotoJob");
            hc.Config.Changed += Config_Changed;
        }

        private void TakePhoto(IHostTimer obj)
        {
            if (hc.Config.IsInternetConnected)
            {
                hc.Logger.LogIfDebug(this, "PhotoJob.TakePhoto begin");
                hc.GetController<IDashCamController>().OrderPicture(800, 600, OnPicture);
            }
            else
            {
                hc.Logger.LogIfDebug(this, "Skipping photo job - no inet connection.");
            }
        }

        private async void OnPicture(MemoryStream ms)
        {
            hc.Logger.LogIfDebug(this, "PhotoJob.OnPicture begin");

            if (ms != null && ms.Length > 0)
            {
                try
                {
                    var fileData = new RemoteFileMetadata { Stream = ms, Name = "/Photos/auto" };
                    await hc.RemoteStorageService.UploadFile(fileData);
                    hc.Logger.Log(this, string.Format("DashCam photo uploaded succerfully as {0}.", fileData.Name), LogLevels.Info);
                }
                catch (Exception ex)
                {
                    hc.Logger.Log(this, ex);
                }
            }
            else
            {
                hc.Logger.Log(this, "Picture stream is null or empty", LogLevels.Warning);
            }
        }

        private void Config_Changed(string name)
        {
            if (name == ConfigNames.PhotoJobEnabled)
            {
                jobTimer.IsEnabled = Enabled;
            }
            else if (name == ConfigNames.PhotoJobPeriodMinutes)
            {
                jobTimer.Span = PeriodMs;
            }
        }
    }
}
