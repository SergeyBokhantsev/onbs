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
            SupportedFunctions = 0x00,
            MonitorStatus = 0x01,
            FuelSystemStatus = 0x03,
            EngineLoad = 0x04,
            CoolantTemp = 0x05,
            MAF = 0x10,
            EngineRPM = 0x0C,
            Speed = 0x0D,
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
            return GetPIDValue<int>(PID.Speed, 3, bytes => (int)bytes[2]);
        }

        public int? GetRPM()
        {
            return GetPIDValue<int>(PID.EngineRPM, 4, bytes => (bytes[2] * 256 + bytes[3]) / 4);
        }

        public int? GetEngineLoad()
        {
            return GetPIDValue<int>(PID.EngineLoad, 3, bytes => (int)bytes[2] * 100 / 255);
        }

        public int? GetCoolantTemp()
        {
            return GetPIDValue<int>(PID.CoolantTemp, 3, bytes => (int)bytes[2] - 40);
        }

        public double? GetMAF()
        {
            return GetPIDValue<double>(PID.MAF, 4, bytes => (((double)bytes[2] * 256d) + (double)bytes[3]) / 100d);
        }

        public IEnumerable<PID> GetSupportedPids()
        {
            var result = new List<PID>();

            uint group = 0;

            while (true)
            {
                var pidFourBytes = GetPIDValue(PID.SupportedFunctions + group);

                if (pidFourBytes == null || pidFourBytes.Length != 6)
                    break;

                pidFourBytes = pidFourBytes.Skip(2).ToArray();

                for (int b = 0; b < 4; ++b)
                {
                    byte mask = 128;

                    for (int bit = 0; bit < 8; ++bit)
                    {
                        if ((mask & pidFourBytes[b]) == mask)
                        {
                            result.Add((PID)(group + (b * 8) + bit + 1));
                        }

                        mask = (byte)(mask >> 1);
                    }
                }

                group += 0x20;

                if (result.Last() != (PID)(group))
                    break;
            }

            return result;
        }

        private Nullable<T> GetPIDValue<T>(PID pid, int expectedBytesCount, Func<byte[], T> formula)
            where T: struct
        {
            var bytes = GetPIDValue(pid);

            if (bytes != null && bytes.Length == expectedBytesCount)
                return formula(bytes);
            else
                return null;
        }

        private byte[] GetPIDValue(PID pid)
        {
            byte[] result = null;

            if (EnsureClient())
            {
                result = FirstHexString(Send(0x0100 + (uint)pid));
            }

            return result;
        }
    }
}
