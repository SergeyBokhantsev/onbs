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
        private readonly IHostController hc;
        private bool active;

        public string Error { get; private set; }

        public enum PID : uint
        {
            SupportedFunctions = 0x0100,
            MonitorStatus = 0x0101,
            FuelSystemStatus = 0x0103,
            EngineLoad = 0x0104,
            CoolantTemp = 0x0105,
            MAF = 0x0110,
            EngineRPM = 0x010C,
            Speed = 0x010D,
        };

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

        public int? GetSpeed()
        {
            if (!EnsureClient())
                return null;

            var bytes = FirstHexString(Send((uint)PID.Speed));

            if (bytes != null && bytes.Length == 3)
                return (int)bytes[2];
            else
                return null;
        }

        public int? GetRPM()
        {
            if (!EnsureClient())
                return null;

            var bytes = FirstHexString(Send((uint)PID.EngineRPM));

            if (bytes != null && bytes.Length == 4)
            {
                return (bytes[2] * 256 + bytes[3]) / 4;
            }
            else
                return null;
        }

        public int? GetEngineLoad()
        {
            if (!EnsureClient())
                return null;

            var bytes = FirstHexString(Send((uint)PID.EngineLoad));

            if (bytes != null && bytes.Length == 3)
            {
                return (int)bytes[2] * 100 / 255;
            }
            else
                return null;
        }

        public int? GetCoolantTemp()
        {
            if (!EnsureClient())
                return null;

            var bytes = FirstHexString(Send((uint)PID.CoolantTemp));

            if (bytes != null && bytes.Length == 3)
            {
                return (int)bytes[2] - 40;
            }
            else
                return null;
        }

        public double? GetMAF()
        {
            Nullable<double> d = new Nullable<double>();

            return GetPIDValue<Nullable<double>>(PID.MAF, 4, bytes => { return d; });
        }

        private T GetPIDValue<T>(PID pid, int expectedBytesCount, Func<byte[], T> formula)
            where T : struct
        {
            if (EnsureClient())
            {
                var bytes = FirstHexString(Send((uint)pid));

                if (bytes != null && bytes.Length == expectedBytesCount)
                    return formula(bytes);
            }

            return default(T);
        }
    }
}
