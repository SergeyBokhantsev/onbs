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
        private readonly LockingProperty<List<Client>> clients;

        public GPSD(IGPSController gps, ILogger logger)
        {
            this.gps = gps;
            this.logger = logger;
            this.clients = new LockingProperty<List<Client>>();
            this.clients.Value = new List<Client>();

            server = new TcpServer.Server(2947, logger);

            //gps.GPRMCReseived += GPRMCReseived;
        }

        void GPRMCReseived(GPRMC gprmc)
        {
            lock (clients)
            {
                clients.Value.ForEach(c => c.SetGPRMC(gprmc));
            }
        }

        public void Start()
        {
            server.ClientConnected += ClientConnected;
            server.Start();
        }

        void ClientConnected(Stream stream)
        {
            var client = new Client(stream);
            clients.Value.Add(client);
        }
        
    }
}
