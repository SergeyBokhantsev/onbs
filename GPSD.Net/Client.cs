using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.GPS;

namespace GPSD.Net
{
    internal class Client
    {
        private enum Modes { Off = 0, WatchReceived, JsonMode };

        private readonly Stream stream;

        private readonly LockingProperty<GPRMC> gprmc = new LockingProperty<GPRMC>();

        private bool updated;
        private Modes mode;

        public Client(Stream stream)
        {
            this.stream = stream;

            SendHello();
            Process();
        }

        public void SetGPRMC(GPRMC gprmc)
        {
            this.gprmc.Value = gprmc;
            this.updated = true;
        }

        private void Process()
        {
            new Thread(() => Read()).Start();

            while (true)
            {
                switch (mode)
                {
                    case Modes.WatchReceived:
                        SendWatchResponse();
                        break;

                    case Modes.JsonMode:
                        if (updated)
                            SendJson();
                        else
                            Thread.Sleep(1000);
                        break;
                }
            }
        }
        
        private void Read()
        {
            var buffer = new byte[2048];

            while (true)
            {
                int size = stream.Read(buffer, 0, buffer.Length);
                var query = Encoding.Default.GetString(buffer, 0, size);
                //TODO: parse query
                //mode = Modes.WatchReceived;
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

        private void SendHello()
        {
            var hello = Json.JsonSerializer.Serialize(new VersionMsg(), Encoding.Default);
            stream.Write(hello, 0, hello.Length);
        }

        private void SendWatchResponse()
        {
            var dm = Json.JsonSerializer.Serialize(new DevicesMsg(), Encoding.Default);

            stream.Write(dm, 0, dm.Length);

            mode = Modes.JsonMode;
        }

        private void SendJson()
        {
            
        }
    }
}
