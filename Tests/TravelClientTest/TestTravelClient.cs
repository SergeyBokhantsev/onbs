using System;
using System.Collections.Generic;
using System.Threading;
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

        private Travel CreateTravel(TravelsClient.Client client, string name)
        {
            var result = client.OpenTravel(name);

            if (result.Success)
                return result.Travel;
            else
                throw new Exception(result.Error);
        }

        private void DeleteTravel(TravelsClient.Client client, Travel travel)
        {
            var result = client.DeleteTravel(travel);

            if (!result.Success)
                throw new Exception(result.Error);
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
        public void CreateAndDeleteTravel()
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

        [TestMethod]
        public void GetTravel()
        {
            //INIT
            var client = new TravelsClient.Client(new Uri(serviceUrl), userKey, vehicleId, new Mocks.Logger());
            var name = Guid.NewGuid().ToString();
            var travel = CreateTravel(client, name);

            //ACT
            var result = client.GetTravel(travel.ID);
            var receivedTravel = result.Travel;

            //ASSERT
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(receivedTravel);
            Assert.AreEqual(travel.Name, receivedTravel.Name);
            Assert.AreEqual(travel.ID, receivedTravel.ID);

            //Cleanup
            DeleteTravel(client, travel);
        }

        [TestMethod]
        public void CloseTravel()
        {
            //INIT
            var client = new TravelsClient.Client(new Uri(serviceUrl), userKey, vehicleId, new Mocks.Logger());
            var name = Guid.NewGuid().ToString();
            var travel = CreateTravel(client, name);

            Assert.IsFalse(travel.Closed);

            //ACT
            client.CloseTravel(travel);
            var receivedTravel = client.GetTravel(travel.ID);

            //ASSERT
            Assert.IsNotNull(receivedTravel);
            Assert.IsTrue(receivedTravel.Travel.Closed);

            //Cleanup
            DeleteTravel(client, travel);
        }

        [TestMethod]
        public void FindActiveTravel()
        {
            //INIT
            var client = new TravelsClient.Client(new Uri(serviceUrl), userKey, vehicleId, new Mocks.Logger());

            var travel1 = CreateTravel(client, Guid.NewGuid().ToString());
            Thread.Sleep(1000);
            var travel2 = CreateTravel(client, Guid.NewGuid().ToString());            

            //ACT 1
            var activeTravel = client.FindActiveTravel();

            //ASSERT 1
            Assert.IsNotNull(activeTravel);
            Assert.AreEqual(travel2.ID, activeTravel.Travel.ID);

            // ACT 2
            client.CloseTravel(travel2);
            activeTravel = client.FindActiveTravel();

            //ASSERT 2
            Assert.IsNotNull(activeTravel);
            Assert.AreEqual(travel1.ID, activeTravel.Travel.ID);

            //Cleanup
            DeleteTravel(client, travel1);
            DeleteTravel(client, travel2);
        }

        [TestMethod]
        public void FindActiveTravelForNonExistVehicle()
        {
            //INIT
            var client = new TravelsClient.Client(new Uri(serviceUrl), userKey, Guid.NewGuid().ToString(), new Mocks.Logger());

            //ACT 1
            var activeTravelResult = client.FindActiveTravel();

            //ASSERT 1
            Assert.IsTrue(activeTravelResult.Success);
            Assert.IsNull(activeTravelResult.Travel);
        }

        [TestMethod]
        public void RenameTravel()
        {
            //INIT
            var client = new TravelsClient.Client(new Uri(serviceUrl), userKey, vehicleId, new Mocks.Logger());
            var name = Guid.NewGuid().ToString();
            var travel = CreateTravel(client, name);

            //ACT
            travel.Name = "newname";
            var result = client.RenameTravel(travel);
            Assert.IsTrue(result.Success);

            var getResult = client.GetTravel(travel.ID);
            Assert.IsTrue(getResult.Success);
            var receivedTravel = getResult.Travel;

            //ASSERT
            Assert.IsNotNull(receivedTravel);
            Assert.AreEqual(travel.Name, receivedTravel.Name);

            //Cleanup
            DeleteTravel(client, travel);
        }

        [TestMethod]
        public void AddTravelPoint()
        {
            //INIT
            var client = new TravelsClient.Client(new Uri(serviceUrl), userKey, vehicleId, new Mocks.Logger());
            var name = Guid.NewGuid().ToString();
            var travel = CreateTravel(client, name);

            Assert.AreEqual(travel.StartTime, travel.EndTime);

            //ACT
            Thread.Sleep(1000);
            var tp = new TravelPoint { Description = Guid.NewGuid().ToString(), Lat = 50, Lon = 30, Speed = 15.57, Type = TravelPointTypes.AutoTrackPoint };
            client.AddTravelPoint(tp, travel);
            travel = client.GetTravel(travel.ID).Travel;

            //ASSERT
            Assert.IsNotNull(travel);
            Assert.AreNotEqual(travel.StartTime, travel.EndTime);

            //Cleanup
            DeleteTravel(client, travel);
        }

        [TestMethod]
        public void AddTravelPointFewTimes()
        {
            //INIT
            var client = new TravelsClient.Client(new Uri(serviceUrl), userKey, vehicleId, new Mocks.Logger());
            var name = Guid.NewGuid().ToString();
            var travel = CreateTravel(client, name);

            Assert.AreEqual(travel.StartTime, travel.EndTime);

            //ACT
            for (int i = 0; i < 10; ++i)
            {
                var tp = new TravelPoint { Description = Guid.NewGuid().ToString(), Lat = 50, Lon = 30, Speed = 15.57, Type = TravelPointTypes.AutoTrackPoint };
                client.AddTravelPoint(tp, travel);
            }

            travel = client.GetTravel(travel.ID).Travel;

            //ASSERT
            Assert.AreNotEqual(travel.StartTime, travel.EndTime);

            //Cleanup
            DeleteTravel(client, travel);
        }

        [TestMethod]
        public void AddTravelPoints()
        {
            //INIT
            var client = new TravelsClient.Client(new Uri(serviceUrl), userKey, vehicleId, new Mocks.Logger());
            var name = Guid.NewGuid().ToString();
            var travel = CreateTravel(client, name);

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
            client.AddTravelPoint(tps, travel);

            travel = client.GetTravel(travel.ID).Travel;

            //ASSERT
            Assert.AreNotEqual(travel.StartTime, travel.EndTime);

            //Cleanup
            DeleteTravel(client, travel);
        }

        [TestMethod]
        public void AddTravelPoints_EqualPoints()
        {
            //INIT
            var client = new TravelsClient.Client(new Uri(serviceUrl), userKey, vehicleId, new Mocks.Logger());
            var name = Guid.NewGuid().ToString();
            var travel = CreateTravel(client, name);

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
            var result = client.AddTravelPoint(tps, travel);

            travel = client.GetTravel(travel.ID).Travel;

            //ASSERT
            Assert.AreNotEqual(travel.StartTime, travel.EndTime);

            //Cleanup
            DeleteTravel(client, travel);
        }

        [TestMethod]
        public void AddTravelPoints_RandomTimePoints()
        {
            //INIT
            var client = new TravelsClient.Client(new Uri(serviceUrl), userKey, vehicleId, new Mocks.Logger());
            var name = Guid.NewGuid().ToString();
            var travel = CreateTravel(client, name);

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
            var result = client.AddTravelPoint(tps, travel);

            travel = client.GetTravel(travel.ID).Travel;

            //ASSERT
            Assert.AreNotEqual(travel.StartTime, travel.EndTime);

            //Cleanup
            DeleteTravel(client, travel);
        }
    }
}
