using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YandexServicesProvider;

namespace Tests
{
    [TestClass]
    public class YandexProvidersTest
    {
        [TestMethod]
        public async Task GetMap()
        {
            //INIT
            var provider = new StaticMapProvider();

            //ACT
            var result = await provider.GetMapAsync(new Interfaces.GPS.GeoPoint(50.4, 30.4), 100, 100, 10, MapLayers.sat | MapLayers.trf);

            //ASSERT
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.Length > 0);
        }

        [TestMethod]
        public async Task GetMapAsync()
        {
            //INIT
            var provider = new StaticMapProvider();
            Stream mapStream = null;
            var callbackExecuted = false;

            Action<Stream> callback = stream =>
                {
                    mapStream = stream;
                    callbackExecuted = true;
                };

            //ACT
            var result = await provider.GetMapAsync(new Interfaces.GPS.GeoPoint(50.4, 30.4), 100, 100, 10, MapLayers.sat | MapLayers.trf);

            //ASSERT
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.Value.Length > 0);
        }

        //[TestMethod]
        //public void GetWeatherForecast()
        //{
        //    //INIT
        //    var config = new Mocks.Config();
        //    var provider = new WeatherProvider(new Mocks.Logger(), config.DataFolder);

        //    //ACT
        //    var forecast = provider.GetForecast("33345");

        //    //ASSERT
        //    Assert.IsNotNull(forecast);
        //    Assert.IsNotNull(forecast.fact.First().temperature.Value);
        //}

        //[TestMethod]
        //public void GetForecastAsync()
        //{
        //    //INIT
        //    var config = new Mocks.Config();
        //    var provider = new WeatherProvider(new Mocks.Logger(), config.DataFolder);
        //    forecast forecast = null;
        //    var callbackExecuted = false;

        //    Action<forecast> callback = fc =>
        //    {
        //        forecast = fc;
        //        callbackExecuted = true;
        //    };

        //    //ACT
        //    provider.GetForecastAsync("33345", callback);

        //    while (!callbackExecuted)
        //        Thread.Sleep(1000);

        //    //ASSERT
        //    Assert.IsNotNull(forecast);
        //}

        [TestMethod]
        public async Task GetAddres()
        {
            //INIT
            var provider = new GeocodingProvider();

            //ACT
            var result = await provider.GetAddresAsync(new Interfaces.GPS.GeoPoint(50.4221855, 30.6592546));

            //ASSERT
            Assert.IsTrue(result.Success);
            Assert.AreEqual("Украина, Киев, Тростянецкая улица, 53", result.Value);
        }

        [TestMethod]
        public async Task GetAddresAsync()
        {
            //INIT
            var provider = new GeocodingProvider();
        
            //ACT
            var result = await provider.GetAddresAsync(new Interfaces.GPS.GeoPoint(50.4221855, 30.6592546));

            //ASSERT
            Assert.IsTrue(result.Success);
            Assert.AreEqual("Украина, Киев, Тростянецкая улица, 53", result.Value);
        }
    }
}
