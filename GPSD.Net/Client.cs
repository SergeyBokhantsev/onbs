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
                mode = Modes.WatchReceived;
            }
        }

        private void SendHello()
        {
            var helloStr = "{\"class\":\"VERSION\",\"release\":\"3.9\",\"rev\":\"3.9\",\"proto_major\":3,\"proto_minor\":8}";
            var helloBin = Encoding.Default.GetBytes(helloStr);
            stream.Write(helloBin, 0, helloBin.Length);
        }

        private void SendWatchResponse()
        {


            mode = Modes.JsonMode;
        }

        private void SendJson()
        {
            
        }
    }
}
