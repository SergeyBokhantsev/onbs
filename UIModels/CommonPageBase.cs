using Interfaces;
using Interfaces.GPS;
using Interfaces.Input;
using Interfaces.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIController;

namespace UIModels
{
    public abstract class CommonPageBase : ModelBase
    {
        protected readonly IHostTimer primaryTimer;
        protected readonly IHostTimer secondaryTimer;

        protected readonly IGPSController gpsController;

        protected CommonPageBase(string viewName, IHostController hc, MappedPage pageDescriptor)
            :base(viewName, hc, pageDescriptor)
        {
            this.Disposing += OnDisposing;

            gpsController = hc.GetController<IGPSController>();
            gpsController.GPRMCReseived += GPRMCReseived;

            primaryTimer = hc.CreateTimer(1000, OnPrimaryTick, true, true);
            secondaryTimer = hc.CreateTimer(60000, OnSecondaryTimer, true, true);
        }

        void GPRMCReseived(GPRMC gprmc)
        {
            if (!Disposed)
            {
                SetProperty("gps_status", gprmc.Active);
            }
        }

        protected virtual void OnPrimaryTick(IHostTimer timer)
        {
            if (Disposed)
                return;

            SetProperty("ard_status", hc.GetController<IArduinoController>().IsCommunicationOk);
            SetProperty("inet_status", hc.Config.IsInternetConnected);

            if (hc.Config.IsSystemTimeValid)
                SetProperty("time", DateTime.Now.AddHours(hc.Config.GetInt(ConfigNames.SystemTimeLocalZone)));
        }

        protected virtual void OnSecondaryTimer(IHostTimer timer)
        {
        }

        protected virtual void OnDisposing(object sender, EventArgs e)
        {
            gpsController.GPRMCReseived -= GPRMCReseived;
            primaryTimer.Dispose();
            secondaryTimer.Dispose();
        }
    }
}
