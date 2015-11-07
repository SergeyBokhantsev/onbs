using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TcpServer
{
    public delegate void BytesReceivedEventHandler(byte[] buffer, int count);

    public class IncomingClient : IDisposable
    {
        private static int clientNumber;

        private BytesReceivedEventHandler bytesReceived;

        private readonly NetworkStream stream;
        private readonly TcpClient client;

        private readonly byte[] readBuffer = new byte[1024];

        private bool disposed;

        public bool Active
        {
            get
            {
                return client.Connected && stream.CanRead && stream.CanWrite;
            }
        }

        public int Number
        {
            get;
            private set;
        }

        public IncomingClient(TcpClient client)
        {
            if (client == null)
                throw new ArgumentNullException("tcp client");
            
            Number = Interlocked.Increment(ref clientNumber);
            
            this.stream = client.GetStream();
            this.client = client;
        }

        public void Start(BytesReceivedEventHandler bytesReceived)
        {
            if (this.bytesReceived != null)
                throw new InvalidOperationException("already started");

            if (bytesReceived == null)
                throw new ArgumentNullException("bytesReceived");

            this.bytesReceived = bytesReceived;
            BeginReadAsync();
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            if (Active)
            {
                stream.Write(buffer, offset, count);
            }
        }

        private void BeginReadAsync()
        {
            try
            {
                if (!disposed)
                    stream.BeginRead(readBuffer, 0, readBuffer.Length, ReadCallback, null);
            }
            catch
            {
                if (Active)
                    BeginReadAsync();
            }
        }

        private void ReadCallback(IAsyncResult ar)
        {
            try
            {
                if (Active)
                {
                    var count = stream.EndRead(ar);

                    if (!disposed && count > 0)
                    {
                        bytesReceived(readBuffer, count);
                    }
                    else
                        System.Threading.Thread.Sleep(100);
                }
            }
			catch 
			{
			}
            finally
            {
                if (Active)
                    BeginReadAsync();
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                stream.Dispose();
                disposed = true;
            }
        }
    }
}
