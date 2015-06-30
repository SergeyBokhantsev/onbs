using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TravelsClient;

namespace Tests.TravelClientTest
{
    [TestClass]
    public class TestTravelClient
    {
        private const string serviceUrl = "http://onbs.azurewebsites.net";
        private const string userKey = "UnitTests";
        private const string vehicleId = "UnitTestsVehicle";

        private Travel CreateTravel(TravelsClient.Client client, string name)
        {
            return client.CreateNewTravel(name);
        }

        private void DeleteTravel(TravelsClient.Client client, Travel travel)
        {
            client.DeleteTravel(travel);
        }

        [TestMethod]
        public void CreateTravelClient()
        {
            //INIT-ACT
            var client = new TravelsClient.Client(new Uri(serviceUrl), userKey, vehicleId, new Mocks.Logger());

            //ASSERT
            Assert.IsNotNull(client);
        }

        [TestMethod]
        public void CreateTravel()
        {
            //INIT
            var client = new TravelsClient.Client(new Uri(serviceUrl), userKey, vehicleId, new Mocks.Logger());
            var name = Guid.NewGuid().ToString();

            //ACT
            var travel = CreateTravel(client, name);

            //ASSERT
            Assert.IsNotNull(travel);
            Assert.AreEqual(name, travel.Name);

            //Cleanup
            DeleteTravel(client, travel);
        }
    }
}
