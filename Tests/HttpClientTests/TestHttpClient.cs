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
            Assert.IsNull(response.Error);
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
        }
    }
}
