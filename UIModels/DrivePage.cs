﻿using Interfaces;
using Interfaces.GPS;
using Interfaces.Input;
using Interfaces.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YandexServicesProvider;
using UIController;

namespace UIModels
{
    public class DrivePage : CommonPageBase
    {
        private readonly ITravelController tc;

        private readonly WeatherProvider weather;
        private readonly GeocodingProvider geocoder;

        private readonly ManualResetGuard weatherGuard = new ManualResetGuard();
        private readonly IOperationGuard geocoderGuard = new TimedGuard(new TimeSpan(0, 0, 3));

        public DrivePage(string viewName, IHostController hc, MappedPage pageDescriptor, object arg)
            : base(viewName, hc, pageDescriptor)
        {
            this.tc = hc.GetController<ITravelController>();

            this.weather = new WeatherProvider(hc.Logger, hc.Config.DataFolder);
            this.geocoder = new GeocodingProvider(hc.Logger);

            hc.GetController<IGPSController>().GPRMCReseived += GPRMCReseived;
        }

        protected override void OnSecondaryTimer(IHostTimer timer)
        {
            if (!Disposed)
            {
                weatherGuard.ExecuteIfFree(UpdateWeatherForecast);
            }

            base.OnSecondaryTimer(timer);
        }

        private async void UpdateWeatherForecast()
        {
            if (!hc.Config.IsInternetConnected)
                return;

            var cityId = hc.Config.GetString(ConfigNames.WeatherCityId);

            if (string.IsNullOrWhiteSpace(cityId))
                return;

            var f = await weather.GetForecastAsync(cityId);

            if (!Disposed && f != null)
            {
                SetProperty("air_temp", f != null
                    ? string.Format("{0}°, {1}", f.fact.First().temperature.Value, f.fact.First().weather_type)
                    : "--");

                SetProperty("weather_icon", weather.GetWeatherIcon(f.fact.First().imagev3.First().Value));
            }
            else
            {
                SetProperty("air_temp", null);
                SetProperty("weather_icon", null);
            }

            weatherGuard.Reset();
        }

        private async void UpdateAddres(GeoPoint location)
        {
            if (!Disposed && hc.Config.IsInternetConnected)
            {
                var addres = await geocoder.GetAddresAsync(location);
                SetProperty("heading", addres);
            }
        }

        protected override void OnPrimaryTick(IHostTimer timer)
        {
            if (!Disposed)
            {
                SetProperty("exported_points", string.Format("{0}/{1}", tc.BufferedPoints, tc.SendedPoints));
                SetProperty("travel_span", tc.TravelTime.TotalMinutes);
                SetProperty("distance", tc.TravelDistance);
            }

            base.OnPrimaryTick(timer);
        }

        protected override void OnDisposing(object sender, EventArgs e)
        {
            weatherGuard.Dispose();
            geocoderGuard.Dispose();

            hc.GetController<IGPSController>().GPRMCReseived -= GPRMCReseived;
            base.OnDisposing(sender, e);
        }

        void GPRMCReseived(GPRMC gprmc)
        {
            if (!Disposed)
            {
                SetProperty("gps_status", gprmc.Active);
                SetProperty("speed", gprmc.Active ? gprmc.Speed.ToString("0") : "-");
                SetProperty("location", gprmc.Location);

                if (gprmc.Active)
                {
                    geocoderGuard.ExecuteIfFree(() => UpdateAddres(gprmc.Location));
                }
            }
        }
    }
}
