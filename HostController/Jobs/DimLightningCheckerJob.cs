using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostController.Jobs
{
    internal class DimLightningCheckerJob
    {
        private readonly IHostController hc;
        private readonly Configuration config;
        private readonly TimedGuard operationGuard_A = new TimedGuard(new TimeSpan(0, 0, 0, 0, fastCheckInterval));
        private readonly TimedGuard operationGuard_B = new TimedGuard(new TimeSpan(0, 0, 0, 0, fastCheckInterval));
        private readonly IHostTimer checkTimer;

        private const int slowCheckInterval = 5 * 60000; // 5 min
        private const int fastCheckInterval = 10000; // 10 sec
        private int A =-1;
        private int B =-1;

        public DimLightningCheckerJob(IHostController hc, Configuration config)
        {
            this.hc = Ensure.ArgumentIsNotNull(hc);
            this.config = Ensure.ArgumentIsNotNull(config);

            hc.GetController<IArduinoController>().LightSensorService.ReadResult += LightSensorService_ReadResult;

            checkTimer = hc.CreateTimer(slowCheckInterval, Check, true, true, "Dim lightning checker");
        }

        void LightSensorService_ReadResult(LightSensorIndexes sensor, byte value)
        {
            var guard = sensor == LightSensorIndexes.Sensor_A ? operationGuard_A : operationGuard_B;

            guard.ExecuteIfFree(() =>
            {
                if (guard == operationGuard_A)
                    A = value;
                else
                    B = value;

                if (A > -1 && B > -1)
                {
                    var gate = config.GetInt(ConfigNames.DimLightningGate);
                    var dim = A <= gate || B <= gate;

                    if (config.IsDimLighting != dim)
                    {
                        config.IsDimLighting = dim;
                        checkTimer.Span = fastCheckInterval;
                        hc.Logger.Log(this, string.Format("Dim condition resolved as: {0}. Sensor A {1}, sensor B {2}, gate {3}", config.IsDimLighting ? "Dark" : "Light", A, B, gate), LogLevels.Info);
                    }
                    else
                    {
                        checkTimer.Span = slowCheckInterval;
                    }                    
                }
            });
        }

        private void Check(IHostTimer obj)
        {
            hc.GetController<IArduinoController>().LightSensorService.ReadSensor(LightSensorIndexes.All);
        }
    }
}
