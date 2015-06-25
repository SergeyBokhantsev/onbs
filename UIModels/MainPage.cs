﻿using Interfaces.UI;
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
        private readonly Timer timer;

        private List<IMetricsProvider> metricsProviders;

        public MainPage(IHostController hostController)
            :base(typeof(MainPage).Name, hostController.Dispatcher, hostController.Logger)
        {
            this.hostController = hostController;

            this.Disposing += MainPageDisposing;

            SubscribeMetricsProviders();

            SetProperty(ModelNames.ButtonF1Label, "Navigation");
            SetProperty(ModelNames.ButtonF2Label, "Camera");
			SetProperty (ModelNames.ButtonF3Label, "Set time test");
			SetProperty(ModelNames.ButtonF8Label, "Configuration");
            SetProperty(ModelNames.ButtonCancelLabel, "Power");

			SetProperty("time_valid", "1");
			SetProperty("time", null);

            timer = new Timer(1000);
            timer.Elapsed += TimerTick;
            timer.Start();
        }

        private void TimerTick(object sender, ElapsedEventArgs e)
        {
            SetProperty("time", DateTime.Now);
            SetProperty("time_valid", hostController.Config.IsSystemTimeValid ? "1" : "0");
        }

        private void SubscribeMetricsProviders()
        {
            if (metricsProviders != null)
                throw new InvalidOperationException("metrics providers has been already initialized.");

            metricsProviders = new List<IMetricsProvider>();

            metricsProviders.Add(hostController.GetController<IArduinoController>());
            metricsProviders.Add(hostController.GetController<IGPSController>());

            metricsProviders.ForEach(mp => mp.MetricsUpdated += OnMetricsUpdated);
        }

        private void OnMetricsUpdated(object sender, IMetrics metrics)
        {
            SetProperty("metrics", metrics);
        }

        void MainPageDisposing(object sender, EventArgs e)
        {
            UnsubscribeMetricsProviders();
            timer.Stop();
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
                case ModelNames.ButtonF1:
                    {
                        if (args.State == ButtonStates.Press)
                        {
                            var page = new NavigationAppPage(hostController);
                            hostController.GetController<IUIController>().ShowPage(page);
                            page.Run();
                        }
                    }
                    break;

                case ModelNames.ButtonF2:
                    {
                        if (args.State == ButtonStates.Press)
                        {
                            var page = new WebCamPage(hostController, cameraAppKey);
                            hostController.GetController<IUIController>().ShowPage(page);
                            page.Run();
                        }
                    }
                    break;

			case ModelNames.ButtonF3:
				{
					if (args.State == ButtonStates.Press) {
						var pr = hostController.ProcessRunnerFactory.Create ("settime");
						pr.Run ();
						
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

                case ModelNames.ButtonCancel:
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