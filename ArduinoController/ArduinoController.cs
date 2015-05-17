using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces.SerialTransportProtocol;
using System.IO.Ports;
using SerialTransportProtocol;

namespace ArduinoController
{
    public class ArduinoController : IArduinoController
    {
        private readonly IPort port;
        private readonly IDispatcher dispatcher;
        private readonly ILogger logger;
        private readonly ISTPCodec codec;

        public IList<IFramesAcceptor> FrameAcceptors
        {
            get; private set;
        }

        public ArduinoController(IPort port, IDispatcher dispatcher, ILogger logger)
        {
            this.port = port;
            this.dispatcher = dispatcher;
            this.logger = logger;

            FrameAcceptors = new List<IFramesAcceptor>();

            var frameBeginMarker = Encoding.UTF8.GetBytes(":<:");
            var frameEndMarker = Encoding.UTF8.GetBytes(":>:");
            codec = new STPCodec(frameBeginMarker, frameEndMarker);

            logger.Log(string.Format("{0} created.", this.GetType().Name), LogLevels.Info);

            port.DataReceived += DataReceived;
        }

        private void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            logger.Log("DataReceived event", LogLevels.Debug);

            var frames = codec.Decode(port);

            if (frames != null && frames.Any())
            {
                foreach (var acceptor in FrameAcceptors)
                {
                    dispatcher.Invoke(null, null, (s, a) => acceptor.AcceptFrames(frames.Where(f => f.Type == acceptor.FrameType)));
                    logger.Log(string.Format("Frames were dispatched for {0} acceptor", acceptor.FrameType), LogLevels.Debug);
                }
            }
        }
    }
}
