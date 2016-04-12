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

		public string ConditionsLevelInfo
		{
			get 
			{
				return string.Join (", ", conditionDetectors.Select (cd => string.Concat((cd.Level * 100).ToString ("0"), "%")));
			}
		}

        public LightSensorGuard(ILightSensorService lss, ONBSSyncContext syncContext)
        {
            this.lss = Ensure.ArgumentIsNotNull(lss);
            this.syncContext = Ensure.ArgumentIsNotNull(syncContext);

            conditionDetectors = new ConditionDetectorBase[]
            {
                new BalanceCondition(4, 5, 7),
                new LevelAttackCondition(LightSensorIndexes.Sensor_A, 0.15),
                new LevelAttackCondition(LightSensorIndexes.Sensor_B, 0.2)
            };

            lss.ReadResult += SensorReadResult;
        }

        void SensorReadResult(LightSensorIndexes sensor, byte value)
        {
			var c1 = conditionDetectors [0].Accept (sensor, value);
			var c2 = conditionDetectors [1].Accept (sensor, value);
			var c3 = conditionDetectors [2].Accept (sensor, value);

			var result = c1 && (c2 != c3);

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
