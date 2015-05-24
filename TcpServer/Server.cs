using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Interfaces;

namespace TcpServer
{
    public class Server
    {
        public event Action<IncomingClient> ClientConnected;

        private readonly TcpListener listener;
        private readonly ILogger logger;

        private long requestsCount;
        private long pendingCount;

        private bool started;

        public Server(int port, ILogger logger)
        {
            this.listener = new TcpListener(System.Net.IPAddress.Any, port);
            this.logger = logger;
        }

        public void Start()
        {
            if (started)
                throw new InvalidOperationException("This listener is already started!");

            listener.Start();
            logger.Log("Listener started", LogLevels.Debug);

            started = true;

            new Thread(() =>
            {
                Thread.CurrentThread.Name = "Tcp server";

                while (started)
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessClient), listener.AcceptTcpClient());
                }
            }).Start();
        }

        public void Stop()
        {
            started = false;
            listener.Stop();
        }

        private void ProcessClient(object state)
        {
            long id = 0;

            var client = state as TcpClient;

            if (client == null || !started)
                return;

            Interlocked.Increment(ref pendingCount);

            client.SendTimeout = 30000;

            try
            {
                id = Interlocked.Increment(ref requestsCount);

                logger.Log(string.Concat("Tcp client accepted, Id = ", id), LogLevels.Debug);

                if (client.Connected)
                {
                    OnClientConnected(client);
                }
                else
                {
                    logger.Log(string.Format("Skipping Tcp client #{0} (not connected)", id), LogLevels.Debug);
                }
            }
            finally
            {
                client.Close();

                logger.Log(string.Concat("Tcp client closed, Id = ", id), LogLevels.Debug);
                var pending = Interlocked.Decrement(ref pendingCount);

                logger.Log(string.Concat("Pending = ", pending), LogLevels.Debug);
            }
        }

        private void OnClientConnected(TcpClient client)
        {
            if (started)
            {
                var handler = ClientConnected;
                if (handler != null)
                    using (var ic = new IncomingClient(client))
                    {
                        handler(ic);
                    }
            }
        }
    }
}
