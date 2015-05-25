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
    public class GPSD
    {
        private readonly TcpServer.Server server;
        private readonly IGPSController gps;
        private readonly ILogger logger;
        private readonly LockingProperty<List<GPSDClient>> clients;

        public GPSD(IGPSController gps, ILogger logger)
        {
            this.gps = gps;
            this.logger = logger;
            this.clients = new LockingProperty<List<GPSDClient>>();
            this.clients.Value = new List<GPSDClient>();

            server = new TcpServer.Server(2947, logger);

            //gps.GPRMCReseived += GPRMCReseived;
        }

        void GPRMCReseived(GPRMC gprmc)
        {
            lock (clients)
            {
                clients.Value.RemoveAll(c => { if (!c.Active) { c.Dispose(); return true; } else return false; });
                clients.Value.ForEach(c => c.SetGPRMC(gprmc));
            }
        }

        public void Start()
        {
			try
			{
            	server.ClientConnected += ClientConnected;
            	server.Start();
			}
			catch (Exception ex) 
			{

			}
        }

        void ClientConnected(IncomingClient client)
        {
            var gclient = new GPSDClient(client);
            clients.Value.Add(gclient);
	    	gclient.Run();
        }
        
    }
}
