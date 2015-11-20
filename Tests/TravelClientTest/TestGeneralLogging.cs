﻿using System;
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
        public void CreateNewGeneralLogTest()
        {
            //INIT
            var serviceUri = new Uri(serviceUrl);
            var logClient = new TravelsClient.GeneralLoggerClient(serviceUri, userKey, vehicleId);

            //ACT
            var logId = logClient.CreateNewLog("test initial body");

            //ASSERT
            Assert.IsTrue(logId != -1);
        }

        [TestMethod]
        public void AppendGeneralLogTest()
        {
            //INIT
            var serviceUri = new Uri(serviceUrl);
            var logClient = new TravelsClient.GeneralLoggerClient(serviceUri, userKey, vehicleId);

            //ACT
            var logId = logClient.CreateNewLog("test initial body");
            Assert.IsTrue(logId != -1);

            logClient.AppendLog(logId, "appended message");
        }
    }
}
