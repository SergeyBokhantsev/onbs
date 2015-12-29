using Interfaces;
using Interfaces.SerialTransportProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcpServer;

namespace ArduinoBoardEmulator
{
    internal class ArduinoServer
    {
        public event Action PingSignal;

        private readonly Server tcpServer;

        private ArduinoClient client;
        private FakeGPS gps;

        public MiniDisplay MiniDisplay
        {
            get;
            private set;
        }

        public Relay Relay
        {
            get;
            private set;
        }

        public ArduinoServer(ILogger logger)
        {
            gps = new FakeGPS();

            MiniDisplay = new ArduinoBoardEmulator.MiniDisplay(new System.Drawing.Size(128, 64));
            Relay = new ArduinoBoardEmulator.Relay();

            tcpServer = new Server(33400, logger);
            tcpServer.ClientConnected += tcpServer_ClientConnected;
            tcpServer.Start();
        }

        void tcpServer_ClientConnected(System.Net.Sockets.TcpClient tcpClient)
        {
            lock (tcpServer)
            {
                if (client != null)
                {
                    client.Dispose();
                }

                client = new ArduinoClient(this, new IncomingClient(tcpClient), gps);
            }

            client.Run();
        }

        internal void SendButtonPress(int num)
        {
            lock (tcpServer)
            {
                if (client != null)
                {
                    client.AddOutcoming(new STPFrame(new byte[] { (byte)num, 43 }, STPFrame.Types.Button));
                }
            }
        }

        public void OnPing()
        {
            var handler = PingSignal;
            if (handler != null)
                handler();
        }
    }
}
