using Interfaces;
using Interfaces.GPS;
using Interfaces.MiniDisplay;
using Interfaces.UI;
using OBD;
using System;
using System.Linq;
using UIModels.MiniDisplay;
using YandexServicesProvider;
using System.Threading;
using System.IO;
using UIModels.MultipurposeModels;
using System.Collections.Generic;

namespace UIModels
{
    public class DrivePage : DrivePageBase
    {
        private static int focusedItemIndex;

        private readonly ITravelController tc;

        private readonly WeatherProvider weather;
        private readonly GeocodingProvider geocoder;

        private readonly IOperationGuard weatherGuard = new InterlockedGuard();
        private readonly IOperationGuard geocoderGuard = new TimedGuard(new TimeSpan(0, 0, 3));
        private readonly IOperationGuard obdGuard = new InterlockedGuard();
        private readonly IOperationGuard minidisplayGuard = new TimedGuard(new TimeSpan(0, 0, 4));
        private readonly IOperationGuard cpuInfoGuard = new TimedGuard(new TimeSpan(0, 0, 10));

        private readonly DriveMiniDisplayModel miniDisplayModel;

		private readonly IElm327Controller elm;
        private readonly OBDProcessor obdProcessor;

        public DrivePage(string viewName, IHostController hc, MappedPage pageDescriptor)
            : base(viewName, hc, pageDescriptor, focusedItemIndex)
        {
            elm = hc.GetController<IElm327Controller>();
            obdProcessor = new OBDProcessor(elm);

            miniDisplayModel = new DriveMiniDisplayModel(hc, pageDescriptor.Name);

            this.tc = hc.GetController<ITravelController>();

            this.weather = new WeatherProvider(hc.Logger, hc.Config.DataFolder);
            this.geocoder = new GeocodingProvider(hc.Logger);

			SetProperty("oil_temp_icon", Path.Combine(hc.Config.DataFolder, "icons", "OilTemp.png"));

            gpsController.GPRMCReseived += GPRMCReseived;         
        }

        protected override void OnSecondaryTimer(IHostTimer timer)
        {
            if (!Disposed)
            {
                weatherGuard.ExecuteIfFreeAsync(UpdateWeatherForecast);
            }

            base.OnSecondaryTimer(timer);
        }

        protected override void DoAction(string name, PageModelActionEventArgs actionArgs)
        {

            base.DoAction(name, actionArgs);
        }

        private void UpdateOBD()
        {
            var engineTemp = obdProcessor.GetCoolantTemp() ?? int.MinValue;
            miniDisplayModel.EngineTemp = engineTemp;
            SetProperty("eng_temp", engineTemp);

			if (!string.IsNullOrEmpty (elm.Error))
				elm.Reset();

            Thread.Sleep(3000);
        }

        private void UpdateWeatherForecast()
        {
            if (!hc.Config.IsInternetConnected)
                return;

            var cityId = hc.Config.GetString(ConfigNames.WeatherCityId);

            if (string.IsNullOrWhiteSpace(cityId))
                return;

            var f = weather.GetForecast(cityId);

            if (!Disposed && f != null)
            {
                int t = int.MinValue;
                int.TryParse(f.fact.First().temperature.Value, out t);
                miniDisplayModel.AirTemp = t;

                SetProperty("air_temp", string.Format("{0}°, {1}", f.fact.First().temperature.Value, f.fact.First().weather_type));

                SetProperty("weather_icon", weather.GetWeatherIcon(f.fact.First().imagev3.First().Value));
            }
            else
            {
                SetProperty("air_temp", null);
                SetProperty("weather_icon", null);
            }
        }

        private void UpdateAddres(GeoPoint location)
        {
            if (!Disposed && hc.Config.IsInternetConnected)
            {
                var addres = geocoder.GetAddres(location);
                SetProperty("heading", addres);
            }
        }

        private void UpdateMiniDisplay()
        {
            miniDisplayModel.BufferedPoints = tc.BufferedPoints;
            miniDisplayModel.SendedPoints = tc.SendedPoints;
            miniDisplayModel.Draw();
        }

        protected override void OnPrimaryTick(IHostTimer timer)
        {
            if (!Disposed)
            {
                SetProperty("exported_points", string.Format("{0}/{1}", tc.BufferedPoints, tc.SendedPoints));
                SetProperty("travel_span", tc.TravelTime.TotalMinutes);
                SetProperty("distance", tc.TravelDistance);

                obdGuard.ExecuteIfFreeAsync(UpdateOBD);
                minidisplayGuard.ExecuteIfFree(UpdateMiniDisplay);
                cpuInfoGuard.ExecuteIfFreeAsync(UpdateCpuInfo, ex => SetProperty("cpu_info", "Error"));
            }

            base.OnPrimaryTick(timer);
        }

        private void UpdateCpuInfo()
        {
            if (!Disposed)
            {
                var cpuSpeed = NixHelpers.CPUInfo.GetCPUSpeed(hc.ProcessRunnerFactory);
                var cpuTemp = NixHelpers.CPUInfo.GetCPUTemp(hc.ProcessRunnerFactory);

                string info = string.Format("{0} Mhz ({1}°)", 
                    cpuSpeed.HasValue ? cpuSpeed.Value.ToString() : "-",
                    cpuTemp.HasValue ? cpuTemp.Value.ToString("0.0") : "-");

                if (!Disposed)
                    SetProperty("cpu_info", info);
            }
        }

        protected override void OnDisposing(object sender, EventArgs e)
        {
            focusedItemIndex = SelectedIndexAbsolute;

            weatherGuard.Dispose();
            geocoderGuard.Dispose();

            gpsController.GPRMCReseived -= GPRMCReseived;
            base.OnDisposing(sender, e);
        }

        private void GPRMCReseived(GPRMC gprmc)
        {
            if (!Disposed)
            {
                SetProperty("speed", gprmc.Active ? gprmc.Speed.ToString("0") : "-");
                SetProperty("location", gprmc.Location);

                if (gprmc.Active)
                {
                    geocoderGuard.ExecuteIfFreeAsync(() => UpdateAddres(gprmc.Location));
                }
            }
        }
    }
}
