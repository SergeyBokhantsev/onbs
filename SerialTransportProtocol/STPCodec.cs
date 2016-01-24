using Interfaces;
using Interfaces.SerialTransportProtocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialTransportProtocol
{
    public class STPCodec : ISTPCodec
    {
        private readonly STPDecoder decoder;
        private readonly STPEncoder encoder;

        private const int tempBufferSize = 1024 * 5;
        private readonly byte[] tempBufer = new byte[tempBufferSize];

        public STPCodec(byte[] frameBeginMark, byte[] frameEndMark, bool simpleFrameMode = false)
        {
            this.decoder = new STPDecoder(frameBeginMark, frameEndMark, simpleFrameMode);
            this.encoder = new STPEncoder(frameBeginMark, frameEndMark, simpleFrameMode);
        }

        // System.Text.StringBuilder dump = new System.Text.StringBuilder();

        public List<STPFrame> Decode(IPort port)
        {
            List<STPFrame> res = null;

            var readed = port.Read(tempBufer, 0, tempBufer.Length);

            //if (dump.Length > 100000)
            //    dump.Clear();
            //for (int i = 0; i < readed; ++i)
            //{
            //    var b = tempBufer[i];
            //    dump.Append(b > 0 ? (char)b : '0');
            //}

            if (readed > 0)
            {
                decoder.Accept(tempBufer, readed, ref res);
            }

            return res;
        }

        public List<STPFrame> Decode(IEnumerable<STPFrame> frames)
        {
            if (frames == null)
                return null;

            List<STPFrame> res = null;

            foreach (var f in frames)
            {
                var frame = f as STPFrame;

                if (frame != null && frame.Data.Length > 0)
                {
                    decoder.Accept(frame.Data, frame.Data.Length, ref res);
                }
            }

            return res;
        }

        public byte[] Encode(STPFrame frame)
        {
            return encoder.Encode(frame);
        }
    }

}
