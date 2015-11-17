using System;
using System.IO;
using System.Linq;
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
            var config = new Mocks.Config();
            var provider = new WeatherProvider(new Mocks.Logger(), config.DataFolder);

            //ACT
            var forecast = provider.GetForecast("33345");

            //ASSERT
            Assert.IsNotNull(forecast);
            Assert.IsNotNull(forecast.fact.First().temperature.Value);
        }

        [TestMethod]
        public void GetForecastAsync()
        {
            //INIT
            var config = new Mocks.Config();
            var provider = new WeatherProvider(new Mocks.Logger(), config.DataFolder);
            forecast forecast = null;
            var callbackExecuted = false;

            Action<forecast> callback = fc =>
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

        [TestMethod]
        public void GetAddres()
        {
            //INIT
            var provider = new GeocodingProvider(new Mocks.Logger());

            //ACT
            var addres = provider.GetAddres(new Interfaces.GPS.GeoPoint(50.4221855, 30.6592546));

            //ASSERT
            Assert.IsNotNull(addres);
            Assert.AreEqual("Украина, Киев, Тростянецкая улица, 53", addres);
        }

        [TestMethod]
        public void GetAddresAsync()
        {
            //INIT
            var provider = new GeocodingProvider(new Mocks.Logger());
            string addres = null;
            var callbackExecuted = false;

            Action<string> callback = a =>
            {
                addres = a;
                callbackExecuted = true;
            };

            //ACT
            provider.GetAddresAsync(new Interfaces.GPS.GeoPoint(50.4221855, 30.6592546), callback);

            while (!callbackExecuted)
                Thread.Sleep(1000);

            //ASSERT
            Assert.IsNotNull(addres);
            Assert.AreEqual("Украина, Киев, Тростянецкая улица, 53", addres);
        }
    }
}
