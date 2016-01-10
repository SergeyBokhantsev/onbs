using Interfaces;
using Interfaces.GPS;
using Interfaces.UI;
using System;
using System.Collections.Generic;

namespace UIModels
{
    public abstract class CommonPageBase : ModelBase
    {
        protected readonly static Dictionary<string, object> crossPageProperties = new Dictionary<string, object>();

        protected readonly IHostTimer primaryTimer;
        protected readonly IHostTimer secondaryTimer;

        protected readonly IGPSController gpsController;

        protected CommonPageBase(string viewName, IHostController hc, MappedPage pageDescriptor)
            :base(viewName, hc, pageDescriptor)
        {
            this.Disposing += OnDisposing;

            foreach (var key in crossPageProperties.Keys)
                SetProperty(key, crossPageProperties[key]);

            gpsController = hc.GetController<IGPSController>();

            primaryTimer = hc.CreateTimer(1000, OnPrimaryTick, true, true, "common primary timer");
            secondaryTimer = hc.CreateTimer(60000, OnSecondaryTimer, true, true, "common secondary timer");
        }

        protected virtual void OnPrimaryTick(IHostTimer timer)
        {
            if (Disposed)
                return;

            SetProperty("ard_status", hc.GetController<IArduinoController>().IsCommunicationOk);
            SetProperty("inet_status", hc.Config.IsInternetConnected);
            SetProperty("gps_status", hc.Config.IsGPSLock);

            if (hc.Config.IsSystemTimeValid)
                SetProperty("time", DateTime.Now.AddHours(hc.Config.GetInt(ConfigNames.SystemTimeLocalZone)));
        }

        protected virtual void OnSecondaryTimer(IHostTimer timer)
        {
        }

        protected virtual void OnDisposing(object sender, EventArgs e)
        {
            primaryTimer.Dispose();
            secondaryTimer.Dispose();

            crossPageProperties["ard_status"] = GetProperty<bool>("ard_status");
            crossPageProperties["inet_status"] = GetProperty<bool>("inet_status");
            crossPageProperties["gps_status"] = GetProperty<bool>("gps_status");
        }
    }
}
