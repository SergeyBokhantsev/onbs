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
    public class GPSD
    {
        private readonly TcpServer.Server server;
        private readonly IGPSController gps;
        private readonly ILogger logger;

        private readonly object gprmcLocker = new object();
        private GPRMC gprmc;

        public GPSD(IGPSController gps, ILogger logger)
        {
            this.gps = gps;
            this.logger = logger;

            server = new TcpServer.Server(2947, logger);

            gps.GPRMCReseived += GPRMCReseived;
        }

        void GPRMCReseived(GPRMC gprmc)
        {
            lock (gprmcLocker)
            {
                this.gprmc = gprmc;
            }
        }

        public void Start()
        {
            server.ClientConnected += ClientConnected;
            server.Start();
        }

        void ClientConnected(Stream stream)
        {
            SendHello(stream);

            GPRMC gprmc;
            gprmc.Rev = long.MaxValue;
            bool updated = true;

            while (true)
            {
                lock (gprmcLocker)
                {
                    if (updated = this.gprmc.Rev != gprmc.Rev)
                    {
                        gprmc = this.gprmc;
                    }
                }

                if (updated)
                {
                    SendGPRMC(gprmc, stream);
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
        }

        private void SendGPRMC(GPRMC gprmc, Stream stream)
        {
            throw new NotImplementedException();
        }

        
    }
}
