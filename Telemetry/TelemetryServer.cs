using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TcpServer;

namespace Telemetry
{
    public class TelemetryServer
    {
        private readonly ILogger logger;
        private readonly Server server;

        public TelemetryServer(ILogger logger)
        {
            this.logger = Ensure.ArgumentIsNotNull(logger);

            server = new Server(18110, logger);
            server.ClientConnected += server_ClientConnected;
            server.Start();
        }

        void server_ClientConnected(TcpClient tcpClient)
        {
            logger.Log(this, "INCOMING TELEMETRY CLIENT!", LogLevels.Warning);

            var client = new IncomingClient(tcpClient);
            client.Start(OnClientReceive);

            client.Write(new byte[] { 1 }, 0, 1);    
        }

        private void OnClientReceive(byte[] buffer, int count)
        {
            
        }
    }
}
