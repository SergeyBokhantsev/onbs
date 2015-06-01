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
		private readonly ISTPCodec arduinoCommandCodec;
        private readonly List<IFramesAcceptor> acceptors = new List<IFramesAcceptor>();

        public ArduinoController(IPort port, IDispatcher dispatcher, ILogger logger)
        {
            this.port = port;
            this.dispatcher = dispatcher;
            this.logger = logger;

            var frameBeginMarker = Encoding.UTF8.GetBytes(":<:");
            var frameEndMarker = Encoding.UTF8.GetBytes(":>:");
            codec = new STPCodec(frameBeginMarker, frameEndMarker);

			
			var arduFrameBeginMarker = Encoding.UTF8.GetBytes("ac{");
			var arduFrameEndMarker = Encoding.UTF8.GetBytes("}");
			arduinoCommandCodec = new STPCodec(arduFrameBeginMarker, arduFrameEndMarker);

            logger.Log(this, string.Format("{0} created.", this.GetType().Name), LogLevels.Info);

            port.DataReceived += DataReceived;
        }

        private void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            logger.LogIfDebug(this, "DataReceived event");

            var frames = codec.Decode(port);

            if (frames != null && frames.Any())
            {
				//Converting ArduCommands
				var convertedFrames = arduinoCommandCodec.Decode(frames.Where (f => f.Type == STPFrame.Types.ArduCommand));
				if (convertedFrames != null)
					frames.AddRange(convertedFrames);

                foreach (var acceptor in acceptors)
                {
                    dispatcher.Invoke(null, null, (s, a) => acceptor.AcceptFrames(frames.Where(f => f.Type == acceptor.FrameType)));
                    logger.LogIfDebug(this, string.Format("Frames were dispatched for {0} acceptor", acceptor.FrameType));
                }
            }
        }

        public void RegisterFrameAcceptor(IFramesAcceptor acceptor)
        {
            acceptors.Add(acceptor);
        }

        public void UnregisterFrameAcceptor(IFramesAcceptor acceptor)
        {
            acceptors.Remove(acceptor);
        }
    }
}
