using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Interfaces;
using System.Threading;

namespace YandexServicesProvider
{
    public class WeatherReport
    {
        public int Temperature { get; private set; }

        public string Conditions { get; private set; }

        public WeatherReport(forecastFact f)
        {
            Temperature = int.Parse(f.temperature.Value);
            Conditions = f.weather_type;
        }
    }

    public class WeatherForecast
    {
        public WeatherReport Fact { get; private set; }

        internal WeatherForecast(forecast f)
        {
            Fact = new WeatherReport(f.fact.First());
        }
    }

    public class WeatherProvider
    {
        private readonly ILogger logger;
        private readonly HttpClient.Client client = new HttpClient.Client();

        public WeatherProvider(ILogger logger)
        {
            this.logger = logger;
        }

        public WeatherForecast GetForecast(string cityId)
        {
            string url = string.Format("http://export.yandex.ru/weather-ng/forecasts/{0}.xml", cityId);

            try
            {
                using (var responce = client.Get(new Uri(url)))
                {
                    if (responce.Status == System.Net.HttpStatusCode.OK)
                    {
                        using (var inputStream = responce.GetStream())
                        {
                            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(forecast));
                            
                            var fc = serializer.Deserialize(inputStream) as forecast;

                            return new WeatherForecast(fc);
                        }
                    }
                    else
                    {
                        logger.Log(this, string.Format("Unable to get weather forecast: {0}", responce.Error), LogLevels.Warning);
                        return null;
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Log(this, ex);
                return null;
            }
        }

        public void GetForecastAsync(string cityId, Action<WeatherForecast> callback)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                var forecast = GetForecast(cityId);

                if (callback != null)
                    callback(forecast);
            });
        }
    }
}
