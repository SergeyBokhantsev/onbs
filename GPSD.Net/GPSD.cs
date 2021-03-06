﻿using System;
using System.Collections.Generic;
using Interfaces;
using Interfaces.GPS;
using System.Net.Sockets;

namespace GPSD.Net
{
    public class GPSD
    {
        private readonly TcpServer.Server server;
        private readonly ILogger logger;
        private readonly List<GPSDClient> clients;
        private readonly IConfig config;

        //http://www.catb.org/gpsd/gpsd_json.html
        //http://wiki.navit-project.org/index.php/Configuration
        public GPSD(IGPSController gps, IConfig config, ILogger logger)
        {
            this.logger = logger;
            this.clients = new List<GPSDClient>();
            this.config = config;

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

        void ClientConnected(TcpClient client)
        {
            GPSDClient gclient = null;
            
            try
            {
                using (var stream = client.GetStream())
                {
                    gclient = new GPSDClient(client, stream, logger, config);

                    lock (clients)
                    {
                        clients.Add(gclient);
                    }

                    gclient.Run();
                }
            }
            catch (Exception ex)
            {
                logger.Log(this, ex);
            }
            finally
            {
                if (gclient != null)
                {
					gclient.Dispose();

                    lock (clients)
                    {
                        clients.Remove(gclient);
                    }
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
