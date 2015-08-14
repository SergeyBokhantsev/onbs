using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elm327
{
    internal class OBDProcessor
    {
        internal string CreateRequest(Elm327FunctionTypes type)
        {
            return string.Concat(((uint)type).ToString("X4"), "\r\n");
        }

        internal IElm327Response GetResponse(byte[] bytes)
        {
            if (bytes.Length < 2)
                return new Elm327Response<byte[]>(Elm327FunctionTypes.Error, bytes);

            var type = (Elm327FunctionTypes)(BitConverter.ToInt16(bytes, 0) - 0x4000);

            switch (type)
            {
                case Elm327FunctionTypes.EngineRPM:
                    return ParseEngineRPM(bytes);

                case Elm327FunctionTypes.Speed:
                    return ParseSpeed(bytes);

                default:
                    return new Elm327Response<byte[]>(Elm327FunctionTypes.Error, bytes);
            }
        }

        private IElm327Response ParseSpeed(byte[] bytes)
        {
            if (bytes.Length != 3)
                return new Elm327Response<byte[]>(Elm327FunctionTypes.Error, bytes);

            return new Elm327Response<int>(Elm327FunctionTypes.EngineRPM, (int)bytes[2]);
        }

        private IElm327Response ParseEngineRPM(byte[] bytes)
        {
            if (bytes.Length != 4)
                return new Elm327Response<byte[]>(Elm327FunctionTypes.Error, bytes);

            var rpm = BitConverter.ToInt16(bytes, 2) / 4;

            return new Elm327Response<int>(Elm327FunctionTypes.EngineRPM, rpm);
        }
    }
}
