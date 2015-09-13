using Interfaces;
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

namespace UIModels
{
    public class DrivePage : CommonPageBase
    {
        private readonly ITravelController tc;

        private readonly WeatherProvider weather;
        private readonly GeocodingProvider geocoder;

        private readonly ManualResetGuard weatherGuard = new ManualResetGuard();
        private readonly IOperationGuard geocoderGuard = new TimedGuard(new TimeSpan(0, 0, 3));

        private readonly IElm327Controller elm327;
        

        public DrivePage(IHostController hc)
            :base(hc, "DrivePage")
        {
            this.tc = hc.GetController<ITravelController>();

            this.weather = new WeatherProvider(hc.Logger);
            this.geocoder = new GeocodingProvider(hc.Logger);

            hc.GetController<IGPSController>().GPRMCReseived += GPRMCReseived;

            elm327 = hc.GetController<IElm327Controller>();

            var elmThread = new Thread(RequestElm);
            elmThread.IsBackground = true;
            elmThread.Start();
        }

        private void RequestElm()
        {
            while(!Disposed)
            {
                var rpm = elm327.GetRPM();
                hc.Dispatcher.Invoke(rpm, null, OnRPM);
            }
        }

        private void OnRPM(object value, EventArgs e)
        {
            if (!Disposed)
                SetProperty("speed", value ?? 0);
        }

        protected override void OnSecondaryTimer(object sender, EventArgs e)
        {
            if (!Disposed && hc.Config.IsInternetConnected)
            {
                weatherGuard.ExecuteIfFree(() => 
                    weather.GetForecastAsync(hc.Config.GetString(ConfigNames.WeatherCityId), OnWeatherForecast));
            }

            base.OnSecondaryTimer(sender, e);
        }

        private void OnWeatherForecast(forecast f)
        {
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

        protected override void OnPrimaryTick(object sender, EventArgs e)
        {
            if (!Disposed)
            {
                SetProperty("exported_points", string.Format("{0}/{1}", tc.BufferedPoints, tc.SendedPoints));
                SetProperty("travel_span", tc.TravelTime.TotalMinutes);
                SetProperty("distance", tc.TravelDistance);
            }

            base.OnPrimaryTick(sender, e);
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
               // SetProperty("speed", gprmc.Active ? gprmc.Speed : 0);
                SetProperty("location", gprmc.Location);

                if (gprmc.Active && hc.Config.IsInternetConnected)
                {
                    geocoderGuard.ExecuteIfFree(() => geocoder.GetAddresAsync(gprmc.Location, addres => SetProperty("heading", addres)));
                }
            }
        }
    }
}
