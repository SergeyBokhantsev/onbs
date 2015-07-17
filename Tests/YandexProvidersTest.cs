using System;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YandexServicesProvider;

namespace Tests
{
    [TestClass]
    public class YandexProvidersTest
    {
        [TestMethod]
        public void GetMap()
        {
            //INIT
            var provider = new StaticMapProvider(new Mocks.Logger());

            //ACT
            var mapStream = provider.GetMap(new Interfaces.GPS.GeoPoint(50.4, 30.4), 100, 100, 10, MapLayers.sat | MapLayers.trf);

            //ASSERT
            Assert.IsNotNull(mapStream);
            Assert.IsTrue(mapStream.Length > 0);
        }

        [TestMethod]
        public void GetMapAsync()
        {
            //INIT
            var provider = new StaticMapProvider(new Mocks.Logger());
            Stream mapStream = null;
            var callbackExecuted = false;

            Action<Stream> callback = stream =>
                {
                    mapStream = stream;
                    callbackExecuted = true;
                };

            //ACT
            provider.GetMapAsync(new Interfaces.GPS.GeoPoint(50.4, 30.4), 100, 100, 10, MapLayers.sat | MapLayers.trf, callback);

            while (!callbackExecuted)
                Thread.Sleep(1000);

            //ASSERT
            Assert.IsNotNull(mapStream);
            Assert.IsTrue(mapStream.Length > 0);
        }

        [TestMethod]
        public void GetWeatherForecast()
        {
            //INIT
            var provider = new WeatherProvider(new Mocks.Logger());

            //ACT
            var forecast = provider.GetForecast("33345");

            //ASSERT
            Assert.IsNotNull(forecast);
            Assert.IsTrue(forecast.Fact.Temperature != -1);
        }

        [TestMethod]
        public void GetForecastAsync()
        {
            //INIT
            var provider = new WeatherProvider(new Mocks.Logger());
            WeatherForecast forecast = null;
            var callbackExecuted = false;

            Action<WeatherForecast> callback = fc =>
            {
                forecast = fc;
                callbackExecuted = true;
            };

            //ACT
            provider.GetForecastAsync("33345", callback);

            while (!callbackExecuted)
                Thread.Sleep(1000);

            //ASSERT
            Assert.IsNotNull(forecast);
        }
    }
}
