using Interfaces;
using Interfaces.SerialTransportProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoController
{
    public class LightSensorService : ILightSensorService, IFrameProvider
    {
        public event FrameToSendDelegate FrameToSend;

        public event Action<LightSensorIndexes, byte> ReadResult;

        private readonly ILogger logger;
        private readonly ONBSSyncContext syncContext;

        public LightSensorService(ILogger logger, ONBSSyncContext syncContext)
        {
            this.logger = logger;
            this.syncContext = syncContext;
        }

        private void OnFrameToSend(STPFrame frame)
        {
            var handler = FrameToSend;
            if (handler != null)
                handler(frame);
        }

        public void ReadSensor(LightSensorIndexes index)
        {
            var frame = new STPFrame(new byte[] { (byte)ArduinoComands.LightSensorRequest, (byte)index }, STPFrame.Types.ArduCommand);
            OnFrameToSend(frame);
        }

        internal bool ProcessResponse(STPFrame frame)
        {
            if (frame.Data.Length == 3)
            {
                var sensorIndex = frame.Data[1];
                var sensorValue = frame.Data[2];

                OnReadResult((LightSensorIndexes)sensorIndex, sensorValue);

                return true;
            }
            else
                return false;
        }

        private void OnReadResult(LightSensorIndexes sensorIndex, byte sensorValue)
        {
            var handler = ReadResult;
            if (handler != null)
            {
                syncContext.Post(h => ((Action<LightSensorIndexes, byte>)h)(sensorIndex, sensorValue), handler, "LightSensor.ReadResult handler");
            }
        }
    }
}
