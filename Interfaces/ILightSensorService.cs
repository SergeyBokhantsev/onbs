using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public enum LightSensorIndexes { Sensor_A, Sensor_B };

    public interface ILightSensorService
    {
        event Action<LightSensorIndexes, byte> ReadResult;

        void ReadSensor(LightSensorIndexes index);
    }
}
