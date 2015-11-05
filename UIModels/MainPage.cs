using Interfaces.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.Input;
using System.Timers;

namespace UIModels
{
    public class MainPage : ModelBase
    {
		private const string cameraAppKey = "cam";

        private readonly IHostController hostController;
        private readonly IHostTimer timer;

        private readonly int localTimeZone;

        private List<IMetricsProvider> metricsProviders;

        public MainPage(IHostController hostController)
            :base(typeof(MainPage).Name, hostController.SyncContext, hostController.Logger)
        {
            this.hostController = hostController;

            this.Disposing += MainPageDisposing;

            this.localTimeZone = hostController.Config.GetInt(ConfigNames.SystemTimeLocalZone);

            SubscribeMetricsProviders();

            SetProperty(ModelNames.ButtonF1Label, "Power");
            SetProperty(ModelNames.ButtonF2Label, "Camera");
            SetProperty(ModelNames.ButtonF5Label, "Save this GPS point");
            SetProperty(ModelNames.ButtonF6Label, "Start new Travel");
			SetProperty(ModelNames.ButtonF8Label, "Configuration");
            SetProperty(ModelNames.ButtonCancelLabel, "Drive");

			SetProperty("time_valid", "1");
			SetProperty("time", null);

            timer = hostController.CreateTimer(1000, TimerTick, true);
        }

        private void TimerTick()
        {
            SetProperty("time", DateTime.Now.ToUniversalTime().AddHours(localTimeZone));
            SetProperty("time_valid", hostController.Config.IsSystemTimeValid ? "1" : "0");

            SetProperty("label_inet_status", hostController.Config.IsInternetConnected ? "Internet OK" : "NO INTERNET CONNECTION");
            SetProperty("inet_status", hostController.Config.IsInternetConnected ? "1" : "0");

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

            metricsProviders.Add(hostController.GetController<IArduinoController>());
            metricsProviders.Add(hostController.GetController<IGPSController>());
            metricsProviders.Add(hostController.GetController<ITravelController>());

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

        protected override void DoAction(PageModelActionEventArgs args)
        {
            switch (args.ActionName)
            {
                case ModelNames.ButtonCancel:
                    {
                        if (args.State == ButtonStates.Press)
                        {
                            var page = new DrivePage(hostController);
                            hostController.GetController<IUIController>().ShowPage(page);
                        }
                    }
                    break;

                case ModelNames.ButtonF2:
                    {
                        if (args.State == ButtonStates.Press)
                        {
                            hostController.GetController<IUIController>().ShowDialog(null);
                            return;

                            var page = new WebCamPage(hostController, cameraAppKey);
                            hostController.GetController<IUIController>().ShowPage(page);
                            page.Run();
                        }
                    }
                    break;

            case ModelNames.ButtonF5:
                {
                    if (args.State == ButtonStates.Press)
                    {
                        hostController.GetController<ITravelController>().MarkCurrentPositionWithCustomPoint();
                    }
                }
                break;

            case ModelNames.ButtonF6:
                {
                    if (args.State == ButtonStates.Press)
                    {
                        var dialog = new UIModels.CommonTemplates.YesNoDialog(hostController,
                            () =>
                            { // YES
                                var travelController = hostController.GetController<ITravelController>();
                                travelController.RequestNewTravel(string.Concat("Manual at", DateTime.Now.AddHours(hostController.Config.GetInt(ConfigNames.SystemTimeLocalZone)).ToString("HH:mm")));
                                hostController.GetController<IUIController>().ShowDefaultPage();
                            },
                            () =>
                            { // NO
                                hostController.GetController<IUIController>().ShowDefaultPage();
                            },
                            "Confirm opening new travel",
                            "Do you really want to request new travel?");

                        hostController.GetController<IUIController>().ShowPage(dialog);
                    }
                }
                break;

                case ModelNames.ButtonF8:
                    {
                        if (args.State == ButtonStates.Press)
                        {
                            hostController.GetController<IUIController>().ShowPage(new ConfigPages.CommonConfigPage(hostController));
                        }
                    }
                    break;

                case ModelNames.ButtonF1:
                    {
                        if (args.State == ButtonStates.Press)
                        {
                            hostController.GetController<IUIController>().ShowPage(new ConfigPages.ShutdownPage(hostController));
                        }
                    }
                    break;
            }
        }
    }
}
