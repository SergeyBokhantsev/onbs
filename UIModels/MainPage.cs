using Interfaces.UI;
using System;
using System.Collections.Generic;
using Interfaces;

namespace UIModels
{
    public class MainPage : ModelBase
    {
        private readonly IHostTimer timer;

        private readonly int localTimeZone;

        private List<IMetricsProvider> metricsProviders;

        public MainPage(string viewName, IHostController hc, MappedPage pageDescriptor)
            : base(viewName, hc, pageDescriptor)
        {
            this.Disposing += MainPageDisposing;

            this.localTimeZone = hc.Config.GetInt(ConfigNames.SystemTimeLocalZone);

            SubscribeMetricsProviders();

			SetProperty("time_valid", "1");
			SetProperty("time", null);

            timer = hc.CreateTimer(1000, TimerTick, true, true, "main page timer");
        }

        private void TimerTick(IHostTimer t)
        {
            SetProperty("time", DateTime.Now.ToUniversalTime().AddHours(localTimeZone));
            SetProperty("time_valid", hc.Config.IsSystemTimeValid ? "1" : "0");

            SetProperty("label_inet_status", hc.Config.IsInternetConnected ? "Internet OK" : "NO INTERNET CONNECTION");
            SetProperty("inet_status", hc.Config.IsInternetConnected ? "1" : "0");

            dynamic hostScheduler = System.Threading.SynchronizationContext.Current;

            if (hostScheduler != null)
            {
                var load = hostScheduler.Load;
                SetProperty(ModelNames.ButtonF2Label, load.ToString());
            }
        }

        private void SubscribeMetricsProviders()
        {
            if (metricsProviders != null)
                throw new InvalidOperationException("metrics providers has been already initialized.");

            metricsProviders = new List<IMetricsProvider>();

            metricsProviders.Add(hc.GetController<IArduinoController>());
            metricsProviders.Add(hc.GetController<IGPSController>());
            metricsProviders.Add(hc.GetController<ITravelController>());

            metricsProviders.ForEach(mp => mp.MetricsUpdated += OnMetricsUpdated);
        }

        private void OnMetricsUpdated(object sender, IMetrics metrics)
        {
            SetProperty("metrics", metrics);
        }

        void MainPageDisposing(object sender, EventArgs e)
        {
            UnsubscribeMetricsProviders();
            timer.Dispose();
        }

        private void UnsubscribeMetricsProviders()
        {
            if (metricsProviders != null)
            {
                metricsProviders.ForEach(mp => mp.MetricsUpdated -= OnMetricsUpdated);
                metricsProviders = null;
            }
        }
    }
}
