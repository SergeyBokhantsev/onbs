using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Interfaces.GPS;

namespace Tests
{
    [TestClass]
    public class GPSHelpers
    {
        [TestMethod]
        public void GetHeadingTest0()
        {
            //INIT
            var start = new GeoPoint(50, 30);
            var end = new GeoPoint(60, 30);

            //ACT
            var heading = Interfaces.GPS.Helpers.GetHeading(start, end);

            //ASSERT
            Assert.AreEqual(0, heading);
        }

        [TestMethod]
        public void GetHeadingTest180()
        {
            //INIT
            var start = new GeoPoint(50, 30);
            var end = new GeoPoint(40, 30);

            //ACT
            var heading = Interfaces.GPS.Helpers.GetHeading(start, end);

            //ASSERT
            Assert.AreEqual(180, heading);
        }

        [TestMethod]
        public void GetHeadingTest90()
        {
            //INIT
            var start = new GeoPoint(50, 30);
            var end = new GeoPoint(50, 40);

            //ACT
            var heading = Interfaces.GPS.Helpers.GetHeading(start, end);

            //ASSERT
            Assert.AreEqual(90, heading);
        }

        [TestMethod]
        public void GetHeadingTest270()
        {
            //INIT
            var start = new GeoPoint(50, 30);
            var end = new GeoPoint(50, 20);

            //ACT
            var heading = Interfaces.GPS.Helpers.GetHeading(start, end);

            //ASSERT
            Assert.AreEqual(270, heading);
        }

        [TestMethod]
        public void GetHeadingTest45()
        {
            //INIT
            var start = new GeoPoint(50, 30);
            var end = new GeoPoint(60, 40);

            //ACT
            var heading = Interfaces.GPS.Helpers.GetHeading(start, end);

            //ASSERT
            Assert.IsTrue(heading > 20 && heading < 40);
        }

        [TestMethod]
        public void GetHeadingTest135()
        {
            //INIT
            var start = new GeoPoint(50, 30);
            var end = new GeoPoint(40, 40);

            //ACT
            var heading = Interfaces.GPS.Helpers.GetHeading(start, end);

            //ASSERT
            Assert.IsTrue(heading > 100 && heading < 170);
        }
    }
}
