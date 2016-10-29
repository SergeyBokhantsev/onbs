using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TravelsClient;

namespace Tests.TravelClientTest
{
    [TestClass]
    public class TestTravelClient
    {
        private const string serviceUrl ="http://onbs2.azurewebsites.net";
        //private const string serviceUrl =  "http://localhost:22424/";

        private const string userKey = "1";
        private const string vehicleId = "UnitTestsVehicle";

        private async Task <Travel> CreateTravel(TravelsClient.Client client, string name)
        {
            var result = await client.OpenTravelAsync(name);

            if (result.Success)
                return result.Value;
            else
                throw result.MakeException();
        }

        private async Task DeleteTravel(TravelsClient.Client client, Travel travel)
        {
            var result = await client.DeleteTravelAsync(travel);

            if (!result.Success)
                throw result.MakeException();
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
        public async Task CreateAndDeleteTravel()
        {
            //INIT
            var client = new TravelsClient.Client(new Uri(serviceUrl), userKey, vehicleId, new Mocks.Logger());
            var name = Guid.NewGuid().ToString();

            //ACT
            var travel = await CreateTravel(client, name);

            //ASSERT
            Assert.IsNotNull(travel);
            Assert.AreEqual(name, travel.Name);

            //Cleanup
            DeleteTravel(client, travel);
        }

        [TestMethod]
        public async Task GetTravel()
        {
            //INIT
            var client = new TravelsClient.Client(new Uri(serviceUrl), userKey, vehicleId, new Mocks.Logger());
            var name = Guid.NewGuid().ToString();
            var travel = await CreateTravel(client, name);

            //ACT
            var result = await client.GetTravelAsync(travel.ID);
            var receivedTravel = result.Value;

            //ASSERT
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(receivedTravel);
            Assert.AreEqual(travel.Name, receivedTravel.Name);
            Assert.AreEqual(travel.ID, receivedTravel.ID);

            //Cleanup
            DeleteTravel(client, travel);
        }

        [TestMethod]
        public async Task CloseTravel()
        {
            //INIT
            var client = new TravelsClient.Client(new Uri(serviceUrl), userKey, vehicleId, new Mocks.Logger());
            var name = Guid.NewGuid().ToString();
            var travel = await CreateTravel(client, name);

            Assert.IsFalse(travel.Closed);

            //ACT
            await client.CloseTravelAsync(travel);
            var receivedTravel = await client.GetTravelAsync(travel.ID);

            //ASSERT
            Assert.IsNotNull(receivedTravel);
            Assert.IsTrue(receivedTravel.Value.Closed);

            //Cleanup
            DeleteTravel(client, travel);
        }

        [TestMethod]
        public async Task FindActiveTravel()
        {
            //INIT
            var client = new TravelsClient.Client(new Uri(serviceUrl), userKey, vehicleId, new Mocks.Logger());

            var travel1 = await CreateTravel(client, Guid.NewGuid().ToString());
            Thread.Sleep(1000);
            var travel2 = await CreateTravel(client, Guid.NewGuid().ToString());            

            //ACT 1
            var activeTravel = await client.FindActiveTravelAsync();

            //ASSERT 1
            Assert.IsNotNull(activeTravel);
            Assert.AreEqual(travel2.ID, activeTravel.Value.ID);

            // ACT 2
            await client.CloseTravelAsync(travel2);
            activeTravel = await client.FindActiveTravelAsync();

            //ASSERT 2
            Assert.IsNotNull(activeTravel);
            Assert.AreEqual(travel1.ID, activeTravel.Value.ID);

            //Cleanup
            DeleteTravel(client, travel1);
            DeleteTravel(client, travel2);
        }

        [TestMethod]
        public async Task FindActiveTravelForNonExistVehicle()
        {
            //INIT
            var client = new TravelsClient.Client(new Uri(serviceUrl), userKey, Guid.NewGuid().ToString(), new Mocks.Logger());

            //ACT 1
            var activeTravelResult = await client.FindActiveTravelAsync();

            //ASSERT 1
            Assert.IsTrue(activeTravelResult.Success);
            Assert.IsNull(activeTravelResult.Value);
        }

        [TestMethod]
        public async Task RenameTravel()
        {
            //INIT
            var client = new TravelsClient.Client(new Uri(serviceUrl), userKey, vehicleId, new Mocks.Logger());
            var name = Guid.NewGuid().ToString();
            var travel = await CreateTravel(client, name);

            //ACT
            travel.Name = "newname";
            var result = await client.RenameTravelAsync(travel);
            Assert.IsTrue(result.Success);

            var getResult = await client.GetTravelAsync(travel.ID);
            Assert.IsTrue(getResult.Success);
            var receivedTravel = getResult.Value;

            //ASSERT
            Assert.IsNotNull(receivedTravel);
            Assert.AreEqual(travel.Name, receivedTravel.Name);

            //Cleanup
            DeleteTravel(client, travel);
        }

        [TestMethod]
        public async Task AddTravelPoint()
        {
            //INIT
            var client = new TravelsClient.Client(new Uri(serviceUrl), userKey, vehicleId, new Mocks.Logger());
            var name = Guid.NewGuid().ToString();
            var travel = await CreateTravel(client, name);

            Assert.AreEqual(travel.StartTime, travel.EndTime);

            //ACT
            Thread.Sleep(1000);
            var tp = new TravelPoint { Description = Guid.NewGuid().ToString(), Lat = 50, Lon = 30, Speed = 15.57, Type = TravelPointTypes.AutoTrackPoint };
            await client.AddTravelPointAsync(tp, travel);
            travel = (await client.GetTravelAsync(travel.ID)).Value;

            //ASSERT
            Assert.IsNotNull(travel);
            Assert.AreNotEqual(travel.StartTime, travel.EndTime);

            //Cleanup
            DeleteTravel(client, travel);
        }

        [TestMethod]
        public async Task AddTravelPointFewTimes()
        {
            //INIT
            var client = new TravelsClient.Client(new Uri(serviceUrl), userKey, vehicleId, new Mocks.Logger());
            var name = Guid.NewGuid().ToString();
            var travel = await CreateTravel(client, name);

            Assert.AreEqual(travel.StartTime, travel.EndTime);

            //ACT
            for (int i = 0; i < 10; ++i)
            {
                var tp = new TravelPoint { Description = Guid.NewGuid().ToString(), Lat = 50, Lon = 30, Speed = 15.57, Type = TravelPointTypes.AutoTrackPoint };
                Assert.IsTrue((await client.AddTravelPointAsync(tp, travel)).Success);
            }

            travel = (await client.GetTravelAsync(travel.ID)).Value;

            //ASSERT
            Assert.AreNotEqual(travel.StartTime, travel.EndTime);

            //Cleanup
            DeleteTravel(client, travel);
        }

        [TestMethod]
        public async Task AddTravelPoints()
        {
            //INIT
            var client = new TravelsClient.Client(new Uri(serviceUrl), userKey, vehicleId, new Mocks.Logger());
            var name = Guid.NewGuid().ToString();
            var travel = await CreateTravel(client, name);

            Assert.AreEqual(travel.StartTime, travel.EndTime);

            //ACT
            Thread.Sleep(1000);
            var tps = new List<TravelPoint>();
            for (int i = 0; i < 100; ++i)
            {
                var tp = new TravelPoint { Description = Guid.NewGuid().ToString(), Lat = 50, Lon = 30, Speed = 15.57, Type = TravelPointTypes.ManualTrackPoint, Time = DateTime.Now };
                Thread.Sleep(100);
                tps.Add(tp);
            }
            await client.AddTravelPointAsync(tps, travel);

            travel = (await client.GetTravelAsync(travel.ID)).Value;

            //ASSERT
            Assert.AreNotEqual(travel.StartTime, travel.EndTime);

            //Cleanup
            DeleteTravel(client, travel);
        }

        [TestMethod]
        public async Task AddTravelPoints_CheckEndTime()
        {
            //INIT
            var client = new TravelsClient.Client(new Uri(serviceUrl), userKey, vehicleId, new Mocks.Logger());
            var name = Guid.NewGuid().ToString();
            var travel = await CreateTravel(client, name);

            Assert.AreEqual(travel.StartTime, travel.EndTime);

            //ACT
            Thread.Sleep(1000);
            var tps = new List<TravelPoint>();
            for (int i = 0; i < 1; ++i)
            {
                var tp = new TravelPoint { Description = Guid.NewGuid().ToString(), Lat = 50, Lon = 30, Speed = 15.57, Type = TravelPointTypes.ManualTrackPoint, Time = DateTime.Now.AddMinutes(3) };
                Thread.Sleep(100);
                tps.Add(tp);
            }
            await client.AddTravelPointAsync(tps, travel);

            Thread.Sleep(3000); //To allow web app to commit changes

            travel = (await client.GetTravelAsync(travel.ID)).Value;

            //ASSERT
            Assert.AreEqual(tps.First().Time.ToUniversalTime().ToString(), travel.EndTime.ToUniversalTime().ToString());

            //Cleanup
            DeleteTravel(client, travel);
        }

        [TestMethod]
        public async Task AddTravelPoints_EqualPoints()
        {
            //INIT
            var client = new TravelsClient.Client(new Uri(serviceUrl), userKey, vehicleId, new Mocks.Logger());
            var name = Guid.NewGuid().ToString();
            var travel = await CreateTravel(client, name);

            Assert.AreEqual(travel.StartTime, travel.EndTime);

            //ACT
            Thread.Sleep(1000);
            var tps = new List<TravelPoint>();
            var time = DateTime.Now;
            for (int i = 0; i < 100; ++i)
            {
                var tp = new TravelPoint { Description = "asd", Lat = 50, Lon = 30, Speed = 15.57, Type = TravelPointTypes.ManualTrackPoint, Time = time };
                tps.Add(tp);
            }
            var result = await client.AddTravelPointAsync(tps, travel);

            travel = (await client.GetTravelAsync(travel.ID)).Value;

            //ASSERT
            Assert.AreNotEqual(travel.StartTime, travel.EndTime);

            //Cleanup
            DeleteTravel(client, travel);
        }

        [TestMethod]
        public async Task AddTravelPoints_RandomTimePoints()
        {
            //INIT
            var client = new TravelsClient.Client(new Uri(serviceUrl), userKey, vehicleId, new Mocks.Logger());
            var name = Guid.NewGuid().ToString();
            var travel = await CreateTravel(client, name);

            Assert.AreEqual(travel.StartTime, travel.EndTime);

            //ACT
            Thread.Sleep(1000);
            var tps = new List<TravelPoint>();
            var rnd = new Random();
            for (int i = 0; i < 50; ++i)
            {
                var tp = new TravelPoint { Description = "asd", Lat = 50, Lon = 30, Speed = 0, Type = TravelPointTypes.ManualTrackPoint, Time = DateTime.Now.AddMinutes(rnd.Next(20)-10) };
                tps.Add(tp);
            }
            var result = await client.AddTravelPointAsync(tps, travel);

            travel = (await client.GetTravelAsync(travel.ID)).Value;

            //ASSERT
            Assert.AreNotEqual(travel.StartTime, travel.EndTime);

            //Cleanup
            DeleteTravel(client, travel);
        }
    }
}
