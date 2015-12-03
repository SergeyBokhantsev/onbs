using Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using Interfaces.SerialTransportProtocol;
using SerialTransportProtocol;
using System.Threading;

namespace HostController.Win
{
    public class MockArduPort : IPort
    {
        public event SerialDataReceivedEventHandler DataReceived;

        private readonly Queue<byte> buffer;
        private readonly ISTPCodec codec;

        private readonly string fakeNmea;
        private int fakeNmeaPos;

        private long readedCount;

        public long OverallReadedBytes
        {
            get
            {
                return Interlocked.Read(ref readedCount);
            }
        }

        public MockArduPort()
        {
            var frameBeginMarker = Encoding.UTF8.GetBytes(":<:");
            var frameEndMarker = Encoding.UTF8.GetBytes(":>:");
            codec = new STPCodec(frameBeginMarker, frameEndMarker);

            buffer = new Queue<byte>();

            fakeNmea = System.IO.File.ReadAllText("./Data/fake_nmea.txt");

            var t = new Thread(SetData) {IsBackground = true};
            t.Start();
        }

        private void SetData()
        {
            while(true)
            {
				const int chunk = 60;

				if (fakeNmeaPos + chunk >= fakeNmea.Length)
                    fakeNmeaPos = 0;

				var nmeaPart = fakeNmea.Substring(fakeNmeaPos, chunk);
                var nmeaData = codec.Encode(new STPFrame(Encoding.Default.GetBytes(nmeaPart), STPFrame.Types.GPS)).ToList();

				fakeNmeaPos += chunk;

                lock(buffer)
                {
                    nmeaData.ForEach(buffer.Enqueue);
                }

                if (DataReceived != null)
                    DataReceived(null, null);

                Thread.Sleep(1000);
            }
            // ReSharper disable once FunctionNeverReturns
        }

        public int Read(byte[] buff, int offset, int count)
        {
            int readed = 0;

            lock (buffer)
            {
                for (int i = 0; i < count; ++i)
                {
                    if (buffer.Any())
                    {
                        buff[offset + i] = buffer.Dequeue();
                        readed++;
                        Interlocked.Increment(ref readedCount);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return readed;
        }

        public void Write(byte[] sourceBuffer, int offset, int count)
        {
        }
    }
}
