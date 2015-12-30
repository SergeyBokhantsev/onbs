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

        private void Turn(Relay relay, RelayActions action)
        {
            var data = new byte[3] { RelayCommands.TURN, (byte)relay, (byte)action };
            var frame = new STPFrame(data, STPFrame.Types.Relay);
            OnFrameToSend(frame);
        }

        private void OnFrameToSend(STPFrame frame)
        {
            var handler = FrameToSend;
            if (handler != null)
                handler(frame, 20);
        }

        public void Enable(Relay relay)
        {
            Turn(relay, RelayActions.Enable);
        }

        public void Disable(Relay relay)
        {
            Turn(relay, RelayActions.Disable);
        }
    }
}
