using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using Interfaces.SerialTransportProtocol;
using SerialTransportProtocol;
using System.Threading;
using Interfaces.Input;

namespace HostController
{
    public class MockArduPort : IPort
    {
        public event SerialDataReceivedEventHandler DataReceived;

        private Queue<byte> buffer;
        private ISTPCodec codec;

        private string fakeNmea;
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

            var t = new Thread(() => SetData());
            t.IsBackground = true;
            t.Start();
        }

        private void SetData()
        {
            while(true)
            {
               // var data = codec.Encode(new STPFrame(new byte[] { (byte)Buttons.Accept, (byte)ButtonStates.Press }, STPFrame.Types.Button)).ToList();

                if (fakeNmeaPos + 10 >= fakeNmea.Length)
                    fakeNmeaPos = 0;

                var nmeaPart = fakeNmea.Substring(fakeNmeaPos, 10);
                var nmeaData = codec.Encode(new STPFrame(Encoding.Default.GetBytes(nmeaPart), STPFrame.Types.GPS)).ToList();

                fakeNmeaPos += 10;

                lock(buffer)
                {
                   // data.ForEach(buffer.Enqueue);
                    nmeaData.ForEach(buffer.Enqueue);
                }

                if (DataReceived != null)
                    DataReceived(null, null);

                Thread.Sleep(1000);

                throw new Exception("fooo");
            }
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
    }
}
