using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.TravelClientTest
{
    [TestClass]
    public class TestGeneralLogging
    {
        private const string serviceUrl = "http://onbs2.azurewebsites.net";
        //private const string serviceUrl =  "http://localhost:22424/";

        private const string userKey = "1";
        private const string vehicleId = "UnitTestsVehicle";

        [TestMethod]
        public async Task CreateNewGeneralLogTest()
        {
            //INIT
            var serviceUri = new Uri(serviceUrl);
            var logClient = new TravelsClient.GeneralLoggerClient(serviceUri, userKey, vehicleId);

            //ACT
            var logId = (await logClient.CreateNewLogAsync("test initial body")).Value;

            //ASSERT
            Assert.IsTrue(logId != -1);
        }

        [TestMethod]
        public async Task AppendGeneralLogTest()
        {
            //INIT
            var serviceUri = new Uri(serviceUrl);
            var logClient = new TravelsClient.GeneralLoggerClient(serviceUri, userKey, vehicleId);

            //ACT
            var logId = (await logClient.CreateNewLogAsync("test initial body")).Value;
            Assert.IsTrue(logId != -1);

            var result = await logClient.AppendLogAsync(logId, "appended message");

            //ASSERT
            Assert.IsTrue(result.Success);
        }
    }
}
