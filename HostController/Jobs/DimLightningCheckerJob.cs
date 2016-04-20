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
        private readonly TimedGuard operationGuard_A = new TimedGuard(new TimeSpan(0, 0, 10));
        private readonly TimedGuard operationGuard_B = new TimedGuard(new TimeSpan(0, 0, 10));

        public DimLightningCheckerJob(IHostController hc, Configuration config)
        {
            this.hc = Ensure.ArgumentIsNotNull(hc);
            this.config = Ensure.ArgumentIsNotNull(config);

            hc.GetController<IArduinoController>().LightSensorService.ReadResult += LightSensorService_ReadResult;

            hc.CreateTimer(30*60000, Check, true, true, "Dim lightning checker");
        }

        void LightSensorService_ReadResult(LightSensorIndexes sensor, byte value)
        {
            var guard = sensor == LightSensorIndexes.Sensor_A ? operationGuard_A : operationGuard_B;

            guard.ExecuteIfFree(() =>
            {
                config.IsDimLighting = (int)value <= config.GetInt(ConfigNames.DimLightningGate);
                hc.Logger.Log(this, string.Format("Dim condition resolved as: {0} basing on sensor {1}", config.IsDimLighting ? "Dark" : "Light", sensor), LogLevels.Info);
            });
        }

        private void Check(IHostTimer obj)
        {
            hc.GetController<IArduinoController>().LightSensorService.ReadSensor(LightSensorIndexes.All);
        }
    }
}
