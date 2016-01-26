using Interfaces;
using Interfaces.SerialTransportProtocol;
using SerialTransportProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoController
{
    public class ArduinoFrameReceiver : IFramesAcceptor
    {
        private readonly ArduinoController arduController;
        private readonly ILogger logger;
        private readonly ISTPCodec arduinoCommandCodec;

        public STPFrame.Types FrameType
        {
            get { return STPFrame.Types.ArduCommand; }
        }

        public ArduinoFrameReceiver(ArduinoController arduController, ILogger logger)
        {
            this.arduController = arduController;
            this.logger = logger;

            var arduFrameBeginMarker = Encoding.ASCII.GetBytes("<-!");
            var arduFrameEndMarker = Encoding.ASCII.GetBytes("!->");
            arduinoCommandCodec = new STPCodec(arduFrameBeginMarker, arduFrameEndMarker, true);
        }

        public void AcceptFrames(IEnumerable<STPFrame> transportFrames)
        {
            var commandFrames = arduinoCommandCodec.Decode(transportFrames);
            if (commandFrames != null)
            {
                commandFrames.ForEach(arduController.ProcessArduinoCommand);
            }
        }
    }
}
