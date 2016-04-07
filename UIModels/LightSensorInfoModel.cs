using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.UI;
using SensorProcessing;

namespace UIModels
{
    public class LightSensorInfoModel : ModelBase
    {
        private readonly ILightSensorService lss;
        private readonly LightSensorGuard lsg;

        public LightSensorInfoModel(string viewName, IHostController hc, MappedPage pageDescriptor)
            : base(viewName, hc, pageDescriptor)
        {
            this.lss = hc.GetController<IArduinoController>().LightSensorService;

            lss.ReadResult += Lss_ReadResult;
            lsg = new LightSensorGuard(lss, hc.SyncContext);
            lsg.ConditionEvent += () => OnMessage("GUARD EVENT: " + DateTime.Now.ToLongTimeString());

            this.Disposing += LightSensorInfoModel_Disposing;

            hc.CreateTimer(500, t => ReadSensors(), true, false, "Light sensors reading timer");

            OnMessage("Working...");
        }

        private void ReadSensors()
        {
            lss.ReadSensor(LightSensorIndexes.Sensor_A);
            lss.ReadSensor(LightSensorIndexes.Sensor_B);
        }

        private void LightSensorInfoModel_Disposing(object sender, EventArgs e)
        {
            if (lss != null)
                lss.ReadResult -= Lss_ReadResult;

            if (lsg != null)
                lsg.Dispose();
        }

        private void Lss_ReadResult(LightSensorIndexes sensorIndex, byte value)
        {
            switch (sensorIndex)
            {
                case LightSensorIndexes.Sensor_A:
                    SetProperty("sensor_a", value);
                    break;

                case LightSensorIndexes.Sensor_B:
                    SetProperty("sensor_b", value);
                    break;

                default:
                    OnMessage("Invalid result");
                    break;
            }
        }

        private void OnMessage(string message)
        {
            SetProperty("message", message);
        }
    }
}
