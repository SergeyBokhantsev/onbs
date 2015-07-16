using Interfaces;
using Interfaces.GPS;
using Interfaces.Input;
using Interfaces.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIModels
{
    public class DrivePage : ModelBase
    {
        private readonly IHostController hc;
        private readonly IDispatcherTimer timer;
        private readonly ITravelController tc;

        public DrivePage(IHostController hc)
            :base("DrivePage", hc.Dispatcher, hc.Logger)
        {
            if (hc == null)
                throw new ArgumentNullException("IHostController");

            onlyPressButtonEvents = true;

            this.hc = hc;
            this.tc = hc.GetController<ITravelController>();

            this.Disposing += DrivePage_Disposing;

            timer = hc.Dispatcher.CreateTimer(1000, OnTimer);
            timer.Enabled = true;

            hc.GetController<IGPSController>().GPRMCReseived += GPRMCReseived;
           // hc.GetController<ITravelController>().MetricsUpdated += TravelMetricsUpdated;
        }

        private void OnTimer(object sender, EventArgs e)
        {
            SetProperty("ard_status", hc.GetController<IArduinoController>().IsCommunicationOk);
            SetProperty("inet_status", hc.Config.IsInternetConnected);

            SetProperty("exported_points", string.Format("{0}/{1}", tc.BufferedPoints, tc.SendedPoints));
            SetProperty("travel_span", tc.TravelTime.TotalMinutes);
            SetProperty("distance", tc.TravelDistance);

            if (hc.Config.IsSystemTimeValid)
                SetProperty("time", DateTime.Now.AddHours(hc.Config.GetInt(ConfigNames.SystemTimeLocalZone)));
        }

        void DrivePage_Disposing(object sender, EventArgs e)
        {
            timer.Dispose();
            hc.GetController<IGPSController>().GPRMCReseived -= GPRMCReseived;
           // hc.GetController<ITravelController>().MetricsUpdated -= TravelMetricsUpdated;
        }
        void GPRMCReseived(GPRMC gprmc)
        {
            SetProperty("gps_status", gprmc.Active);
            SetProperty("speed", gprmc.Active ? gprmc.Speed : 0);
            SetProperty("location", gprmc.Location);
        }

        protected override void DoAction(Interfaces.UI.PageModelActionEventArgs args)
        {
            switch (args.ActionName)
            {
                case ModelNames.ButtonCancel:
                    hc.GetController<IUIController>().ShowPage(new UIModels.MainPage(hc));
                    break;

                case ModelNames.ButtonF1:
                    ExternalApplicationPage page = new NavigationAppPage(hc);
                    hc.GetController<IUIController>().ShowPage(page);
                    page.Run();
                    break;

                case ModelNames.ButtonF2:
                    page = new WebCamPage(hc, "cam");
                    hc.GetController<IUIController>().ShowPage(page);
                    page.Run();
                    break;
            }
        }
    }
}
