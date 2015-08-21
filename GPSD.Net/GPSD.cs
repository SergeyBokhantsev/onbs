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
        private readonly IDispatcher dispatcher;
        private readonly ILogger logger;
        private readonly IConfig config;
        private readonly List<GPSDClient> clients;

        //http://www.catb.org/gpsd/gpsd_json.html
        //http://wiki.navit-project.org/index.php/Configuration
        public GPSD(IGPSController gps, IDispatcher dispatcher, IConfig config, ILogger logger)
        {
            this.gps = gps;
            this.dispatcher = dispatcher;
            this.config = config;
            this.logger = logger;
            this.clients = new List<GPSDClient>();

            if (config.GetBool(ConfigNames.GPSDEnabled))
            {
                server = new TcpServer.Server(2947, logger);

                gps.GPRMCReseived += GPRMCReseived;
                gps.NMEAReceived += NMEAReceived;
            }
        }

        void NMEAReceived(string nmea)
        {
            lock (clients)
            {
                clients.ForEach(c => c.nmea.Value = nmea);
            }
        }

        void GPRMCReseived(GPRMC gprmc)
        {
            lock (clients)
            {
                clients.ForEach(c => c.gprmc.Value = gprmc);
            }
        }

        public void Start()
        {
            try
            {
                if (server != null)
                {
                    server.ClientConnected += ClientConnected;
                    server.Start();
                }
            }
            catch (Exception ex)
            {
                logger.Log(this, ex);
            }
        }

        void ClientConnected(IncomingClient client)
        {
            var gclient = new GPSDClient(client, dispatcher, logger);

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
                if (server != null)
                {
                    server.ClientConnected -= ClientConnected;
                    server.Stop();
                }

                lock (clients)
                {
                    clients.ForEach(c => c.Dispose());
                    clients.Clear();
                }
            }
            catch (Exception ex)
            {
                logger.Log(this, ex);
            }
        }
    }
}
