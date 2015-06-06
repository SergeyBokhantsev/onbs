using Interfaces.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.Input;

namespace UIController.Models
{
    public class MainPage : ModelBase
    {
        private const string navigationAppKey = "nav";
		private const string cameraAppKey = "cam";

        private readonly IHostController hostController;

        private List<IMetricsProvider> metricsProviders;

        public MainPage(IHostController hostController)
            :base(typeof(MainPage).Name, hostController.Dispatcher, hostController.Logger)
        {
            this.hostController = hostController;

            this.Disposing += MainPageDisposing;

            SubscribeMetricsProviders();

            SetProperty("nav_btn_caption", "Navigation");
            SetProperty("cam_btn_caption", "Camera");
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
                case navigationAppKey:
                case "F1":
                    {
                        if (args.State == ButtonStates.Press)
                        {
                            var page = new NavigationAppPage(hostController, navigationAppKey);
                            hostController.GetController<IUIController>().ShowPage(page);
                            page.Run();
                        }
                    }
                    break;

                case cameraAppKey:
                case "F2":
                    {
                        if (args.State == ButtonStates.Press)
                        {
                            var uiController = hostController.GetController<IUIController>();
                            var runner = hostController.ProcessRunnerFactory.Create(cameraAppKey);
                            var page = new ExternalApplicationPage(typeof(ExternalApplicationPage).Name, runner, hostController.Dispatcher, hostController.Logger, uiController);
                            uiController.ShowPage(page);
                            page.Run();
                        }
                    }
                    break;

			case "F4":
				if (args.State == ButtonStates.Press) 
				{
					hostController.GetController<IAutomationController> ()
					.Key (AutomationKeys.Alt, AutomationKeys.Tab);
				}
				break;
            }
        }
    }
}
