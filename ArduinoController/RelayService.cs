using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.Relays;
using Interfaces.SerialTransportProtocol;

namespace ArduinoController
{
    public class RelayService : IRelayService, IFrameProvider
    {
        public event FrameToSendDelegate FrameToSend;

        private readonly ILogger logger;

        public RelayService(ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");

            this.logger = logger;
        }

        public void Schedule(Relay relay, RelayActions action, byte delaySec)
        {
            var data = new byte[4] { RelayCommands.SCHEDULE_COMMAND, (byte)relay, (byte)action, delaySec };
            var frame = new STPFrame(data, STPFrame.Types.Relay);
            OnFrameToSend(frame);
        }

        public void Unschedule(Relay relay)
        {
            var data = new byte[2] { RelayCommands.UNSCHEDULE_COMMAND, (byte)relay };
            var frame = new STPFrame(data, STPFrame.Types.Relay);
            OnFrameToSend(frame);
        }

        private void OnFrameToSend(STPFrame frame)
        {
            var handler = FrameToSend;
            if (handler != null)
                handler(frame, 20);
        }
    }
}
