﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Interfaces;
using System.Threading;
using System.IO;
using System.Reflection;
using HttpServiceNamespace;

namespace YandexServicesProvider
{
    public class WeatherProvider
    {
        private readonly string iconPath;

        //private readonly ILogger logger;
        //private readonly HttpService client = new HttpService(null, null) { HttpGetTimeout = new TimeSpan(0, 0, 10), HttpPostTimeout = new TimeSpan(0, 0, 10) };

        public WeatherProvider(ILogger logger, string dataFolder)
        {
           // this.logger = logger;
            this.iconPath = Path.Combine(dataFolder, "weather");
        }

        public forecast GetForecast(string cityId)
        {
            return null;
        }
        //{
        //    string url = string.Format("http://export.yandex.ru/weather-ng/forecasts/{0}.xml", cityId);

        //    try
        //    {
        //        using (var responce = client.Get(new Uri(url), 2, 3000))
        //        {
        //            if (responce.Status == System.Net.HttpStatusCode.OK)
        //            {
        //                using (var inputStream = responce.GetStream())
        //                {
        //                    var serializer = new System.Xml.Serialization.XmlSerializer(typeof(forecast));
                            
        //                    var fc = serializer.Deserialize(inputStream) as forecast;

        //                    return fc;
        //                }
        //            }
        //            else
        //            {
        //                logger.Log(this, string.Format("Unable to get weather forecast: {0}", responce.Error), LogLevels.Warning);
        //                return null;
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Log(this, ex);
        //        return null;
        //    }
        //}

        //public void GetForecastAsync(string cityId, Action<forecast> callback)
        //{
        //    ThreadPool.QueueUserWorkItem(state =>
        //    {
        //        var forecast = GetForecast(cityId);

        //        if (callback != null)
        //            callback(forecast);
        //    });
        //}

        //public async Task<forecast> GetForecastAsync(string cityId)
        //{
        //    return await Task.Run<forecast>(() => GetForecast(cityId));
        //}

        public string GetWeatherIcon(string code)
        {
            return Path.Combine(iconPath, string.Concat(code, ".png"));
        }
    }
}
