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

        public MainPage(IHostController hostController)
            :base(typeof(MainPage).Name, hostController.Dispatcher, hostController.Logger)
        {
            this.hostController = hostController;

            this.Disposing += MainPageDisposing;

            var arduController = hostController.GetController<IArduinoController>();
            arduController.MetricsUpdated += OnMetricsUpdated;

            SetProperty("nav_btn_caption", "Navigation");
            SetProperty("cam_btn_caption", "Camera");
        }

        private void OnMetricsUpdated(object sender, IMetrics metrics)
        {
            SetProperty("metrics", metrics);
        }

        void MainPageDisposing(object sender, EventArgs e)
        {
            var arduController = hostController.GetController<IArduinoController>();
            arduController.MetricsUpdated -= OnMetricsUpdated;
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
                            var uiController = hostController.GetController<IUIController>();
                            var runner = hostController.CreateProcessRunner(navigationAppKey);
                            var page = new ExternalApplicationPage(runner, hostController.Dispatcher, hostController.Logger, uiController);
                            uiController.ShowPage(page);
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
                            var runner = hostController.CreateProcessRunner(cameraAppKey);
                            var page = new ExternalApplicationPage(runner, hostController.Dispatcher, hostController.Logger, uiController);
                            uiController.ShowPage(page);
                            page.Run();
                        }
                    }
                    break;
            }
        }
    }
}
