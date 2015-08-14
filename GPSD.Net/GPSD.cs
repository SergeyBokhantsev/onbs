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
        private readonly IConfig config;
        private readonly List<GPSDClient> clients;

        //http://www.catb.org/gpsd/gpsd_json.html
        //http://wiki.navit-project.org/index.php/Configuration
        public GPSD(IGPSController gps, IConfig config, ILogger logger)
        {
            this.gps = gps;
            this.config = config;
            this.logger = logger;
            this.clients = new List<GPSDClient>();

            server = new TcpServer.Server(2947, logger);

            gps.GPRMCReseived += GPRMCReseived;
            gps.NMEAReceived += NMEAReceived;
        }

        void NMEAReceived(string nmea)
        {
            lock (clients)
            {
                if (config.GetBool(ConfigNames.GPSDEnabled))
                    clients.ForEach(c => c.SetNMEA(nmea));
            }
        }

        void GPRMCReseived(GPRMC gprmc)
        {
            lock (clients)
            {
                clients.ForEach(c => c.SetGPRMC(gprmc));
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
                logger.Log(this, ex);
            }
        }

        //This handler invoked by dedicated thread
        void ClientConnected(IncomingClient client)
        {
            var gclient = new GPSDClient(client, logger);

            lock (clients)
            {
                clients.Add(gclient);
            }

            try
            {
                gclient.Run();
            }
            catch (Exception ex)
            {
                logger.Log(this, ex);
            }
            finally
            {
                client.Dispose();

                lock (clients)
                {
                    clients.Remove(gclient);
                }
            }
        }

        public void Stop()
        {
            try
            {
                server.ClientConnected -= ClientConnected;
                server.Stop();

                lock (clients)
                {
                    clients.ForEach(c => c.Dispose());
                    clients.Clear();
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}
