using Interfaces.SerialTransportProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialTransportProtocol
{
    internal class STPEncoder
    {
        private readonly byte[] frameBeginMark;
        private readonly byte[] frameEndMark;

        public STPEncoder(byte[] frameBeginMark, byte[] frameEndMark)
        {
            this.frameBeginMark = frameBeginMark;
            this.frameEndMark = frameEndMark;
        }

        public byte[] Encode(STPFrame frame)
        {
            var res = new byte[frameBeginMark.Length + 1 + frame.Data.Length + frameEndMark.Length];
            int resLen = 0;

            //BEGIN MARK
            frameBeginMark.CopyTo(res, 0, frameBeginMark.Length);
            resLen += frameBeginMark.Length;
            //TYPE
            res[resLen] = (byte)frame.Type;
            resLen++;
            //DATA
            frame.Data.CopyTo(res, resLen, frame.Data.Length);
            resLen += frame.Data.Length;
            //END MARK
            frameEndMark.CopyTo(res, resLen, frameEndMark.Length);

            return res;
        }
    }

}
