using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;

namespace SensorProcessing
{
    public class LightSensorGuard : IDisposable
    {
        public event Action ConditionEvent;

        private readonly ILightSensorService lss;
        private readonly ONBSSyncContext syncContext;
        private readonly ConditionDetectorBase[] conditionDetectors;

        private bool disposed;

        public LightSensorGuard(ILightSensorService lss, ONBSSyncContext syncContext)
        {
            this.lss = Ensure.ArgumentIsNotNull(lss);
            this.syncContext = Ensure.ArgumentIsNotNull(syncContext);

            conditionDetectors = new ConditionDetectorBase[]
            {
                new BalanceCondition(5, 5),
                new LevelAttackCondition(LightSensorIndexes.Sensor_A, 0.5, 0.3),
                new LevelAttackCondition(LightSensorIndexes.Sensor_B, 0.5, 0.3)
            };

            lss.ReadResult += SensorReadResult;
        }

        void SensorReadResult(LightSensorIndexes sensor, byte value)
        {
            var result = conditionDetectors.All(cd => cd.Accept(sensor, value));

            if (result)
            {
                syncContext.Post((o) => OnConditionEvent(), null, "LightSensorGuard ConditionEvent");
            }
        }

        private void OnConditionEvent()
        {
            var handler = ConditionEvent;
            if (handler != null && !disposed)
            {
                handler();
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                lss.ReadResult -= SensorReadResult;
                disposed = true;
            }
        }
    }
}
