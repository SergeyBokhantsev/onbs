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
    public class WeatherProvider
    {
        private readonly ILogger logger;
        private readonly HttpClient.Client client = new HttpClient.Client();

        public WeatherProvider(ILogger logger)
        {
            this.logger = logger;
        }

        public forecast GetForecast(string cityId)
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

                            return fc;
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

        public void GetForecastAsync(string cityId, Action<forecast> callback)
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
