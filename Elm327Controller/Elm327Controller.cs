using Elm327;
using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elm327Controller
{
    public class Elm327Controller : IElm327Controller, IDisposable
    {
        private readonly object locker = new object();
        private readonly IHostController hc;
        private Elm327.Client elm;
        private bool disposed;
        
        public string Error { get; private set; }        

        public Elm327Controller(IHostController hc)
        {
            this.hc = hc;
        }

        public void Reset()
        {
            lock (locker)
            {
                if (elm != null)
                    elm.Dispose();

                Error = null;
                elm = null;
            }
        }

        private bool EnsureElm()
        {
            if (Error != null)
            {
                return false;
            }
            else if (disposed)
            {
                Error = "Controller is disposed!";
                return false;
            }
            else if (elm == null)
            {
                try
                {
                    var portName = hc.Config.GetString(ConfigNames.Elm327Port);
                    elm = new Client(hc.Logger);
                    elm.Run(portName);

                    if (elm.Reset())
                        return true;
                    else
                    {
                        Error = "Unable to reset Elm327 module";
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    hc.Logger.Log(this, ex);
                    Error = ex.Message;
                    return false;
                }
            }
            else
                return true;
        }

        public Nullable<T> GetPIDValue<T>(uint pid, int expectedBytesCount, Func<byte[], T> formula)
            where T: struct
        {
            var bytes = GetPIDValue(pid);

            if (bytes != null && bytes.Length == expectedBytesCount)
                return formula(bytes);
            else
                return null;
        }

        public byte[] GetPIDValue(uint pid)
        {
            byte[] result = null;

            lock (locker)
            {
                if (EnsureElm())
                {
                    result = FirstHexString(elm.Send(0x0100 + pid, "X4"));
                }
            }

            return result;
        }

        public byte[] GetTroubleCodes()
        {
            byte[] result = null;

            lock (locker)
            {
                if (EnsureElm())
                {
                    result = FirstHexString(elm.Send(0x03, "X2"));
                }
            }

            return result;
        }

        private byte[] FirstHexString(string[] response)
        {
            if (response != null)
            {
                foreach (var line in response)
                {
                    if (IsHexString(line))
                    {
                        return HexToBytes(line);
                    }
                }
            }

            return null;
        }

        private bool IsHexString(string str)
        {
            for (int i = 0; i < str.Length; ++i)
            {
                if (!(str[i] >= 48 && str[i] <= 57)
                    && !(str[i] >= 65 && str[i] <= 70)
                    && !(str[i] >= 97 && str[i] <= 102)
                    && str[i] != 32)
                    return false;
            }

            return true;
        }

        private byte[] HexToBytes(string str)
        {
            str = str.Replace(" ", string.Empty);

            hc.Logger.LogIfDebug(this, string.Concat("Begin converting to HEX: ", str));

            var ret = new byte[str.Length / 2];

            for (int i = 0; i < str.Length; i += 2)
            {
                ret[i / 2] = Convert.ToByte(str.Substring(i, 2), 16);
            }

            hc.Logger.LogIfDebug(this, string.Concat("Resulting bytes: ", string.Join(", ", ret)));

            return ret;
        }

        public void Dispose()
        {
            lock (locker)
            {
                if (!disposed)
                {
                    disposed = true;
                    Reset();
                }
            }
        }
    }
}
