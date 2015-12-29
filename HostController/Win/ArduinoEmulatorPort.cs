using Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace HostController.Win
{
    public class ArduinoEmulatorPort : IPort
    {
        private class IncomingChunk
        {
            public byte[] Data
            {
                get;
                private set;
            }

            private int Len
            {
                get; 
                set;
            }

            public int Offset
            {
                get;
                set;
            }

            public int Count
            {
                get
                {
                    return Len - Offset;
                }
            }

            public IncomingChunk(byte[] data, int len)
            {
                Data = data;
                Len = len;
            }
        }

        public event SerialDataReceivedEventHandler DataReceived;

        private TcpClient tcpClient;
        private NetworkStream stream;

        private readonly List<IncomingChunk> incoming = new List<IncomingChunk>();

        public bool Connected { get { return stream != null; } }
         
        public long OverallReadedBytes
        {
            get { return 0; }
        }

        public ArduinoEmulatorPort()
        {
            Connect(0);
        }

        private async Task Connect(int delay)
        {
            if (delay > 0)
                await Task.Delay(delay);

            // ReSharper disable once UseObjectOrCollectionInitializer
            tcpClient = new TcpClient();
            tcpClient.Client.ReceiveTimeout = 300000;
            tcpClient.Client.SendTimeout = 300000;
            tcpClient.NoDelay = true;

            try
            {
                await tcpClient.ConnectAsync("localhost", 33400);

                if (tcpClient.Client != null && tcpClient.Connected)
                {
                    stream = tcpClient.GetStream();

                    while (stream != null && stream.CanRead)
                    {
                        var data = new byte[1024];

                        var readed = await stream.ReadAsync(data, 0, data.Length);

                        lock (incoming)
                        {
                            incoming.Add(new IncomingChunk(data, readed));
                        }

                        var handler = DataReceived;
                        if (handler != null)
                            handler(this, null);
                    }
                }
                else
                {
                    Connect(3000);
                }
            }
            catch
            {
                tcpClient.Close();
                Connect(3000);
            }
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            IncomingChunk chunk = null;

            lock (incoming)
            {
                if (incoming.Any())
                {
                    chunk = incoming.First();
                }
            }

            if (chunk != null)
            {
                var countToCopy = Math.Min(chunk.Count, count);

                Array.Copy(chunk.Data, chunk.Offset, buffer, offset, countToCopy);

                if (countToCopy < chunk.Count)
                {
                    chunk.Offset = countToCopy;
                }
                else
                {
                    lock (incoming)
                    {
                        incoming.Remove(chunk);
                    }
                }

                return countToCopy;
            }
            else
            {
                return 0;
            }
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            if (stream != null)
            {
                try
                {
                    stream.Write(buffer, offset, count);
                }
                catch (IOException)
                {
                    stream.Close();
                    stream = null;
                    tcpClient.Close();
                    Connect(3000);
                }
            }
        }
    }
}
