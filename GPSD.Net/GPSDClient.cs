using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.GPS;
using TcpServer;

namespace GPSD.Net
{
    internal class GPSDClient : IDisposable
    {
        private enum Modes { Off = 0, WatchProcessing, JsonMode };

        private readonly IncomingClient tcpClient;

        private readonly LockingProperty<GPRMC> gprmc = new LockingProperty<GPRMC>();

        private readonly AutoResetEvent updateEvent = new AutoResetEvent(false);

        private LockingProperty<Modes> mode = new LockingProperty<Modes>(Modes.Off);

        private string query;

        public bool Active
        {
            get
            {
                return tcpClient.Active;
            }
        }

        public GPSDClient(IncomingClient tcpClient)
        {
            if (tcpClient == null)
                throw new ArgumentNullException("tcpClient");

            this.tcpClient = tcpClient;

            tcpClient.BytesReceived += BytesReceived;

            SendHello();
            Process();
        }

        void BytesReceived(byte[] buffer, int count)
        {
            query += Encoding.Default.GetString(buffer, 0, count);

            if (query.EndsWith("\n"))
            {
                mode.Value = Modes.WatchProcessing;

                query = string.Empty;

                updateEvent.Set();
            }
        }

        public void SetGPRMC(GPRMC gprmc)
        {
            this.gprmc.Value = gprmc;
            updateEvent.Set();
        }

        private void Process()
        {
            while (Active)
            {
                //updateEvent.WaitOne();
                Thread.Sleep(1000);

                switch (mode.Value)
                {
                    case Modes.WatchProcessing:
                        ProcessWatchQuery();
                        break;

                    case Modes.JsonMode:
                        SendJson();
                        break;
                }
            }
        }

        private class VersionMsg
        {
            public string @class { get { return "VERSION"; } }
            public string release { get { return "3.9"; } }
            public string rev { get { return "3.9"; } }
            public string proto_major { get { return "3"; } }
            public string proto_minor { get { return "8"; } }
        }

//        {"class":"DEVICES","devices":[{"class":"DEVICE","path":"/dev/ttyUSB0",
//                   "activated":1269959537.20,"native":0,"bps":4800,"parity":"N",
//                   "stopbits":1,"cycle":1.00}]}
//{"class":"WATCH","enable":true,"json":true,"nmea":false,"raw":0,
//                 "scaled":false,"timing":false,"pps":false}

        private class DeviceMsg
        {
            public string @class { get { return "DEVICE"; } }
            public string path { get { return "/dev/ttyUSB0"; } }
            public double activated { get { return 1269959537.20; } }
            public int native { get { return 0; } }
            public int bps { get { return 9600; } }
            public string parity { get { return "N"; } }
            public int stopbits { get { return 1; } }
            public double cycle { get { return 1; } }
        }

        private class DevicesMsg
        {
            public string @class { get { return "DEVICES"; } }
            public object[] devices { get { return new object[] { new DeviceMsg() }; } }
        }

        private class WatchMsg
        {
            public string @class { get { return "WATCH"; } }
            public bool enable { get { return true; } }
            public bool json { get { return true; } }
            public bool nmea { get { return false; } }
            public int raw { get { return 0; } }
            public bool scaled { get { return false; } }
            public bool timing { get { return false; } }
            public bool pps { get { return false; } }
        }

        private void SendHello()
        {
            var hello = Json.JsonSerializer.Serialize(new VersionMsg(), Encoding.Default);
            tcpClient.Write(hello, 0, hello.Length);
        }

        private void ProcessWatchQuery()
        {
            var dm = Json.JsonSerializer.Serialize(new DevicesMsg(), Encoding.Default);
            var wm = Json.JsonSerializer.Serialize(new WatchMsg(), Encoding.Default);

            tcpClient.Write(dm, 0, dm.Length);
            tcpClient.Write(wm, 0, wm.Length);

            mode.Value = Modes.JsonMode;
        }

        //{"class":"TPV","time":"2010-04-30T11:48:20.10Z","ept":0.005,
        //       "lat":46.498204497,"lon":7.568061439,"alt":1327.689,
        //        "epx":15.319,"epy":17.054,"epv":124.484,"track":10.3797,
        //        "speed":0.091,"climb":-0.085,"eps":34.11,"mode":3}

        private class TPVMsg
        {
            public string @class { get { return "TPV"; } }
            public string time { get; set; }
            public double ept { get; set; }
            public double lat { get; set; }
            public double lon { get; set; }
            public double alt { get; set; }
            public double epx { get; set; }
            public double epy { get; set; }
            public double epv { get; set; }
            public double track { get; set; }
            public double speed { get; set; }
            public double climb { get; set; }
            public double eps { get; set; }
            public int mode { get; set; }
        }

        private void SendJson()
        {
            var tpv = new TPVMsg
            {
                time = "2010-04-30T11:48:20.10Z",
                ept = 0.005,
                lat = 46.498204497,
                lon = 7.568061439,
                alt = 1327.689,
                epx = 15.319,
                epy = 17.054,
                epv = 124.484,
                track = 10.3797,
                speed = 0.091,
                climb = -0.085,
                eps = 34.11,
                mode = 3
            };

            var bytes = Json.JsonSerializer.Serialize(tpv, Encoding.Default);

            tcpClient.Write(bytes, 0, bytes.Length);
        }

        public void Dispose()
        {
            tcpClient.Dispose();
        }
    }
}
