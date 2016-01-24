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
        private readonly bool simpleFrameMode;

        public STPEncoder(byte[] frameBeginMark, byte[] frameEndMark, bool simpleFrameMode)
        {
            this.frameBeginMark = frameBeginMark;
            this.frameEndMark = frameEndMark;
            this.simpleFrameMode = simpleFrameMode;
        }

        public static byte CalculateChecksum(byte[] data)
        {
            byte res = 0;
            foreach (var b in data)
                res ^= b;
            return res;
        }

        public byte[] Encode(STPFrame frame)
        {
            return simpleFrameMode ? EncodeSimpleMode(frame) : EncodeFullMode(frame);
        }

        private byte[] EncodeSimpleMode(STPFrame frame)
        {        
            var res = new byte[frameBeginMark.Length + frame.Data.Length + frameEndMark.Length];
            int resLen = 0;

            //BEGIN MARK
            frameBeginMark.CopyTo(res, 0, frameBeginMark.Length);
            resLen += frameBeginMark.Length;
            //DATA
            frame.Data.CopyTo(res, resLen, frame.Data.Length);
            resLen += frame.Data.Length;
            //END MARK
            frameEndMark.CopyTo(res, resLen, frameEndMark.Length);

            return res;
        }

        private byte[] EncodeFullMode(STPFrame frame)
        {
            const int internalDataLen = 4; //frame type(1), frameId(2), checksum(1)
            var res = new byte[frameBeginMark.Length + internalDataLen + frame.Data.Length + frameEndMark.Length];
            int resLen = 0;

            //BEGIN MARK
            frameBeginMark.CopyTo(res, 0, frameBeginMark.Length);
            resLen += frameBeginMark.Length;
            //TYPE
            res[resLen] = (byte)frame.Type;
            resLen++;
            //ID
            res[resLen++] = (byte)((frame.Id >> 8) & 0xFF);
            res[resLen++] = (byte)(frame.Id & 0xFF);
            //CHECKSUM
            res[resLen++] = CalculateChecksum(frame.Data);
            //DATA
            frame.Data.CopyTo(res, resLen, frame.Data.Length);
            resLen += frame.Data.Length;
            //END MARK
            frameEndMark.CopyTo(res, resLen, frameEndMark.Length);

            return res;
        }
    }

}
