using Elm327;
using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elm327Controller
{
    public class Elm327Controller : Elm327.Client, IElm327Controller
    {
        private readonly object locker = new object();
        private readonly IHostController hc;
        private bool active;

        public string Error { get; private set; }        

        public Elm327Controller(IHostController hc)
            :base(hc.Logger)
        {
            this.hc = hc;
        }

        private bool EnsureClient()
        {
            if (active)
            {
                return true;
            }

            if (Error == null)
            {
                try
                {
                    var portName = hc.Config.GetString(ConfigNames.Elm327Port);
                    Run(portName);
                    Reset();
                    return active = true;
                }
                catch (Exception ex)
                {
                    hc.Logger.Log(this, ex);
                    Error = ex.Message;
                    return false;
                }
            }

            return false;
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
                if (EnsureClient())
                {
                    result = FirstHexString(Send(0x0100 + pid));
                }
            }

            return result;
        }
    }
}
