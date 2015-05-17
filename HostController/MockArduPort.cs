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

        public MockArduPort()
        {
            var frameBeginMarker = Encoding.UTF8.GetBytes(":<:");
            var frameEndMarker = Encoding.UTF8.GetBytes(":>:");
            codec = new STPCodec(frameBeginMarker, frameEndMarker);

            buffer = new Queue<byte>();

            new Thread(() => SetData()).Start();
        }

        private void SetData()
        {
            while(true)
            {
                var data = codec.Encode(new STPFrame(new byte[] { (byte)Buttons.Accept, (byte)ButtonStates.Press }, STPFrame.Types.Button)).ToList();
                
                lock(buffer)
                {
                    data.ForEach(buffer.Enqueue);
                }

                if (DataReceived != null)
                    DataReceived(null, null);

                Thread.Sleep(1000);
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
