using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class GPSDTests
    {
        [TestMethod]
        public void ConstructGPSD()
        {
            //INIT
            var logger = new Mocks.Logger();
            var gps = new Mocks.GPSController();
            var config = CreateConfig();
            var gpsd = new GPSD.Net.GPSD(gps, config, logger);
        }

        private IConfig CreateConfig()
        {
            var cfg = new Mocks.Config();

            cfg.Set<bool>(Interfaces.ConfigNames.GPSDEnabled, true);

            return cfg;
        }

        [TestMethod]
        public void GPSDTestWatch()
        {
            //INIT
            var logger = new Mocks.Logger();
            var gps = new Mocks.GPSController();
            var config = CreateConfig();
            var gpsd = new GPSD.Net.GPSD(gps, config, logger);
            var tcpClient = new TcpClient();

            //ACT
            try
            {
                gpsd.Start();

                tcpClient = new TcpClient();
                tcpClient.Connect("localhost", 2947);
                var stream = tcpClient.GetStream();

                Thread.Sleep(500);

                // READING HELLO
                var sr = new StreamReader(stream);
                var response = sr.ReadLine();
                Json.JsonObj json = null;
                Json.JsonParser.TryParse(Encoding.Default.GetBytes(response), out json);

                Assert.IsNotNull(json);
                Assert.AreEqual("VERSION", Json.JPath.GetFieldValue<string>(json, "class"));

                // WRITING WATCH
                var watch = "?WATCH={\"enable\":true,\"nmea\":true}\n";
                var data = Encoding.Default.GetBytes(watch);
                stream.Write(data, 0, data.Length);

                Thread.Sleep(500);

                // READING DEVICES
                response = sr.ReadLine();
                Json.JsonParser.TryParse(Encoding.Default.GetBytes(response), out json);

                Assert.IsNotNull(json);
                Assert.AreEqual("DEVICES", Json.JPath.GetFieldValue<string>(json, "class"));

                // READING WATCH
                response = sr.ReadLine();
                Json.JsonParser.TryParse(Encoding.Default.GetBytes(response), out json);

                Assert.IsNotNull(json);
                Assert.AreEqual("WATCH", Json.JPath.GetFieldValue<string>(json, "class"));

                // READING nmea
                var nmea = new char[128];
                int readed = 0;
                int iterations = 0;
                while (readed < nmea.Length)
                {
                    readed += sr.Read(nmea, 0, nmea.Length);

                    Thread.Sleep(500);

                    if (iterations > 100)
                        throw new Exception();
                }
            }
            finally
            {
                gpsd.Stop();
                tcpClient.Close();
            }
        }
    }
}
