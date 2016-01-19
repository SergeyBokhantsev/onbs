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

namespace UIModels
{
    public class DrivePage : CommonPageBase
    {
        private readonly ITravelController tc;

        private readonly WeatherProvider weather;
        private readonly GeocodingProvider geocoder;

        private readonly IOperationGuard weatherGuard = new InterlockedGuard();
        private readonly IOperationGuard geocoderGuard = new TimedGuard(new TimeSpan(0, 0, 3));
        private readonly IOperationGuard obdGuard = new InterlockedGuard();
        private readonly IOperationGuard minidisplayGuard = new TimedGuard(new TimeSpan(0, 0, 4));

        private readonly DriveMiniDisplayModel miniDisplayModel;

		private readonly IElm327Controller elm;
        private readonly OBDProcessor obdProcessor;

        public DrivePage(string viewName, IHostController hc, MappedPage pageDescriptor)
            : base(viewName, hc, pageDescriptor)
        {
            elm = hc.GetController<IElm327Controller>();
            obdProcessor = new OBDProcessor(elm);

            miniDisplayModel = new DriveMiniDisplayModel(hc, pageDescriptor.Name);

            this.tc = hc.GetController<ITravelController>();

            this.weather = new WeatherProvider(hc.Logger, hc.Config.DataFolder);
            this.geocoder = new GeocodingProvider(hc.Logger);

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
            }

            base.OnPrimaryTick(timer);
        }

        protected override void OnDisposing(object sender, EventArgs e)
        {
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
