﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpServer
{
    public delegate void BytesReceivedEventHandler(byte[] buffer, int count);

    public class IncomingClient : IDisposable
    {
        public event BytesReceivedEventHandler BytesReceived;

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

        public IncomingClient(TcpClient client)
        {
            if (client == null)
                throw new ArgumentNullException("tcp client");

            this.stream = client.GetStream();
            this.client = client;
            BeginReadAsync();
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            if (Active)
                stream.Write(buffer, offset, count);
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
                var count = stream.EndRead(ar);

                var handler = BytesReceived;
                if (!disposed && handler != null)
                    handler(readBuffer, count);
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
