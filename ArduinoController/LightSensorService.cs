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

        private ILogger logger;

        public LightSensorService(ILogger logger)
        {
            this.logger = logger;
        }

        private void OnFrameToSend(STPFrame frame)
        {
            FrameToSend?.Invoke(frame);
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
            ReadResult?.Invoke(sensorIndex, sensorValue);
        }
    }
}
