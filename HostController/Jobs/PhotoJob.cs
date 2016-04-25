using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace HostController.Jobs
{
    public class PhotoJob
    {
        private readonly IHostTimer jobTimer;
        private readonly IHostController hc;

        private volatile int busy;

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
            jobTimer = hc.CreateTimer(PeriodMs, TakePhoto, Enabled, true, "PhotoJob");
            hc.Config.Changed += Config_Changed;
        }

        private void TakePhoto(IHostTimer obj)
        {
            if (busy > 0)
                return;

            if (hc.Config.IsInternetConnected)
            {
                hc.Logger.LogIfDebug(this, "PhotoJob.TakePhoto begin");
                hc.GetController<IDashCamController>().OrderPicture(800, 600, OnPicture);
                busy = 1;
            }
            else
            {
                hc.Logger.LogIfDebug(this, "Skipping photo job - no inet connection.");
            }
        }

        private async void OnPicture(MemoryStream ms)
        {
            busy = 0;

            hc.Logger.LogIfDebug(this, "PhotoJob.OnPicture begin");

            if (hc.Config.IsInternetConnected && ms != null && ms.Length > 0)
            {
                try
                {
                    var fileData = new RemoteFileMetadata { Stream = ms, Name = "/Photos/auto.jpg" };
                    await hc.RemoteStorageService.UploadFile(fileData);
                    hc.Logger.Log(this, string.Format("DashCam photo uploaded succerfully as {0}.", fileData.Name), LogLevels.Info);
                }
                catch (Exception ex)
                {
                    hc.Logger.Log(this, string.Format("Failed to upload picture: {0}", ex.Message), LogLevels.Warning);
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
