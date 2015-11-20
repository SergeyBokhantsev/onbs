
using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBD
{
    public enum PID : uint
    {
        SupportedFunctions = 0x00,
        MonitorStatus = 0x01,
        FuelSystemStatus = 0x03,
        EngineLoad = 0x04,
        CoolantTemp = 0x05,
        MAP = 0x0B,
        IntakeAirTemp = 0x0F,
        MAF = 0x10,
        ThrottlePosition = 0x11,
        EngineRPM = 0x0C,
        Speed = 0x0D,
    };

    public struct MonitorStatus
    {
        public bool MIL { get; private set; }
        public int DTC_Count { get; private set; }

        public MonitorStatus(bool mil, int dtc_count)
            :this()
        {
            MIL = mil;
            DTC_Count = dtc_count;
        }
    }

    public class OBDProcessor
    {
        private readonly IElm327Controller elm;

        public OBDProcessor(IElm327Controller elm)
        {
            if (elm == null)
                throw new ArgumentNullException("elm");

            this.elm = elm;
        }

        public MonitorStatus? GetMonitorStatus()
        {
            return elm.GetPIDValue<MonitorStatus>((uint)PID.MonitorStatus, 6, bytes =>
            {
                var mil = (bytes[2] & 128) > 0;
                var dtc_count = bytes[2] & 127;
                return new MonitorStatus(mil, dtc_count);
            });
        }

        public IEnumerable<string> GetTroubleCodes()
        {
            var SAEFrame = elm.GetTroubleCodes();

            var codeClasses = new Dictionary<string , string> { {"0", "P0"}, {"1", "P1"}, {"2", "P2"}, {"3", "P3"}, {"4", "C0"}, {"5", "C1"}, {"6", "C2"}, {"7", "C3"}, {"8", "B0"}, {"9", "B1"}, {"A", "B2"}, {"B", "B3"}, {"C", "U0"}, {"D", "U1"}, {"E", "U2"}, {"F", "U3"} };

            if (SAEFrame != null && SAEFrame.Length == 12)
            {
                for(var i = 0; i< SAEFrame.Length; i+=4)
                {
                    if (SAEFrame[i] == '0' && SAEFrame[i + 1] == '0' && SAEFrame[i + 2] == '0' && SAEFrame[i + 3] == '0')
                        continue;

                    yield return string.Concat(codeClasses[SAEFrame[i].ToString()], SAEFrame.Substring(i + 1, 3));
                }
            }
            else
                yield break;
        }

        public int? GetSpeed()
        {
            return elm.GetPIDValue<int>((uint)PID.Speed, 3, bytes => (int)bytes[2]);
        }

        public int? GetRPM()
        {
            return elm.GetPIDValue<int>((uint)PID.EngineRPM, 4, bytes => (bytes[2] * 256 + bytes[3]) / 4);
        }

        public int? GetEngineLoad()
        {
            return elm.GetPIDValue<int>((uint)PID.EngineLoad, 3, bytes => (int)bytes[2] * 100 / 255);
        }

        public int? GetCoolantTemp()
        {
            return elm.GetPIDValue<int>((uint)PID.CoolantTemp, 3, bytes => (int)bytes[2] - 40);
        }

        /// <summary>
        /// Not supported in Lanos
        /// </summary>
        /// <returns></returns>
        public double? GetMAF()
        {
            return elm.GetPIDValue<double>((uint)PID.MAF, 4, bytes => (((double)bytes[2] * 256d) + (double)bytes[3]) / 100d);
        }

        public int? GetMAP()
        {
            return elm.GetPIDValue<int>((uint)PID.MAP, 3, bytes => (int)bytes[2]);
        }

        public int? GetIntakeAirTemp()
        {
            return elm.GetPIDValue<int>((uint)PID.IntakeAirTemp, 3, bytes => (int)bytes[2] - 40);
        }

        /// <summary>
        /// Liters per hour consumption
        /// </summary>
        public double GetFuelFlowPerHour(double map, double rpm, double iat)
        {
            const double volumetricEfficiency = 95 / 100;
            const double displacement = 1.5; //liters
            const double stoich = 14.7;

            double imap = rpm * map / (iat + 273.15);
            double maf = imap / 120 * volumetricEfficiency * displacement * (28.97) / 8.314;
            return maf / (stoich * 454 * 6.17 / 3.78) * 3600;
        }

        public int? GetThrottlePosition()
        {
            return elm.GetPIDValue<int>((uint)PID.ThrottlePosition, 3, bytes => (int)bytes[2] * 100 / 255);
        }

        public IEnumerable<PID> GetSupportedPids()
        {
            var result = new List<PID>();

            uint group = 0;

            while (true)
            {
                var pidFourBytes = elm.GetPIDValue((uint)PID.SupportedFunctions + group);

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
    }
}
