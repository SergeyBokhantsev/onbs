using Interfaces;
using Interfaces.SerialTransportProtocol;
using SerialTransportProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TcpServer;

namespace ArduinoBoardEmulator
{
    internal class ArduinoClient : IPort, IDisposable
    {
        private readonly ArduinoServer server;
        private readonly FakeGPS gps;
        private readonly IncomingClient client;
        private readonly STPCodec codec;
        private readonly STPCodec arduinoCommandCodec;

        private byte[] incoming;
        private int incomingLen;

        private Queue<STPFrame> outcoming = new Queue<STPFrame>();

        private bool disposed;

        public ArduinoClient(ArduinoServer server, IncomingClient client, FakeGPS gps)
        {
            this.server = server;
            this.client = client;
            this.gps = gps;

            gps.NMEA += gps_NMEA;
            
            var frameBeginMarker = Encoding.UTF8.GetBytes(":<:");
            var frameEndMarker = Encoding.UTF8.GetBytes(":>:");
            codec = new STPCodec(frameBeginMarker, frameEndMarker);

            var arduFrameBeginMarker = Encoding.UTF8.GetBytes("ac{");
            var arduFrameEndMarker = Encoding.UTF8.GetBytes("}");
            arduinoCommandCodec = new STPCodec(arduFrameBeginMarker, arduFrameEndMarker);
        }

        void gps_NMEA(byte[] data)
        {
            AddOutcoming(new STPFrame(data, STPFrame.Types.GPS));
        }

        private void client_BytesReceived(byte[] buffer, int count)
        {
            incoming = buffer;
            incomingLen = count;
            var frames = codec.Decode(this);

            Process(frames);
        }

        private void Process(List<STPFrame> frames)
        {
            foreach (var frame in frames)
            {
                switch(frame.Type)
                {
                    case STPFrame.Types.ArduCommand:
                        if (frame.Data.Length == 1 && frame.Data.First() == ArduinoComands.ArduinoPingRequest)
                        {
                            var outFrameData = arduinoCommandCodec.Encode(new STPFrame(new byte[] { (byte)ArduinoComands.ArduinoPingResponce }, STPFrame.Types.ArduCommand));
                            AddOutcoming(new STPFrame(outFrameData, STPFrame.Types.ArduCommand));
                            server.OnPing();
                        }
                        break;

                    case STPFrame.Types.MiniDisplay:
                        server.MiniDisplay.ProcessFrame(frame);
                        break;
                }
            }
        }

        public void AddOutcoming(STPFrame frame)
        {
            lock (outcoming)
            {
                outcoming.Enqueue(frame);
            }
        }

        public void Run()
        {
            client.Start(client_BytesReceived);

            while (!disposed)
            {
                STPFrame frame = null;

                lock(outcoming)
                {
                    if (outcoming.Any())
                    {
                        frame = outcoming.Dequeue();
                    }
                }

                if (frame != null)
                {
                    var data = codec.Encode(frame);
                    client.Write(data, 0, data.Length);
                }
                else
                    Thread.Sleep(100);
            }
        }

        public event System.IO.Ports.SerialDataReceivedEventHandler DataReceived;

        public long OverallReadedBytes
        {
            get;
            set;
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            var len = Math.Min(incomingLen, count);
            Array.Copy(incoming, 0, buffer, offset, len);
            OverallReadedBytes += len;
            return len;
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                client.Dispose();
                gps.NMEA -= gps_NMEA;
            }
        }
    }
}
