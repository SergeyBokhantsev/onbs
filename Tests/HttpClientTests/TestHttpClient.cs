using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.HttpClientTests
{
    [TestClass]
    public class TestHttpClient
    {
        [TestMethod]
        public void HttpClientGetValidUrl()
        {
            //INIT
            var client = new HttpClient.Client();
            var uri = new Uri("https://www.google.com.ua/");

            //ACT
            var response = client.Get(uri);

            //ASSERT
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.Status);
            Assert.AreEqual("OK", response.Error);

            response.Dispose();
        }

        [TestMethod]
        public void HttpClientGetInvalidUrl()
        {
            //INIT
            var client = new HttpClient.Client();
            var uri = new Uri("https://www.google.com.ua/some_fake_page");

            //ACT
            var response = client.Get(uri);

            //ASSERT
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.NotFound, response.Status);
            Assert.IsNotNull(response.Error);
            Assert.AreNotEqual("OK", response.Error);
        }

        [TestMethod]
        public void HttpClientPostInvalidUrl()
        {
            //INIT
            var client = new HttpClient.Client();
            var uri = new Uri("https://www.google.com");

            //ACT
            var response = client.Post(uri, "hhh", 3, 1);

            //ASSERT
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.NotFound, response.Status);
            Assert.IsNotNull(response.Error);
            Assert.AreNotEqual("OK", response.Error);
        }
    }
}
