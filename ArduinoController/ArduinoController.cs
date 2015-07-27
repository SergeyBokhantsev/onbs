using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces.SerialTransportProtocol;
using System.IO.Ports;
using SerialTransportProtocol;
using System.Diagnostics;

namespace ArduinoController
{
    public class ArduinoController : IArduinoController
    {
        public event MetricsUpdatedEventHandler MetricsUpdated;

        private const string metricReadedBytes = "Bytes received";
        private const string metricDecodedFrames = "Frames decoded";
        private const string metricElapsed = "Process time";
        private const string metricIsError = "_is_error";

        private readonly IPort port;
        private readonly IDispatcher dispatcher;
        private readonly IDispatcherTimer arduinoWatchdogTimer;
        private readonly ILogger logger;
        private readonly ISTPCodec codec;
		private readonly ISTPCodec arduinoCommandCodec;
        private readonly List<IFramesAcceptor> acceptors = new List<IFramesAcceptor>();

        private long decodedFramesCount;

        public bool IsCommunicationOk
        {
            get;
            private set;
        }

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

            arduinoWatchdogTimer = dispatcher.CreateTimer(5000, (s, e) => 
            {
                Send(new byte[] { (byte)'d' });
                logger.LogIfDebug(this, "Ping command sended to Arduino");
            });
            arduinoWatchdogTimer.Enabled = true;

            logger.Log(this, string.Format("{0} created.", this.GetType().Name), LogLevels.Info);

            port.DataReceived += DataReceived;
        }

        private void Send(byte[] data)
        {
            try
            {
                var serializedFrame = codec.Encode(new STPFrame(data, STPFrame.Types.ArduCommand));
                port.Write(serializedFrame, 0, serializedFrame.Length);
            }
            catch (Exception ex)
            {
                logger.Log(this, "Exception sending data to Arduino", LogLevels.Error);
                logger.Log(this, ex);
            }
        }

        private IEnumerable<STPFrame> ProcessArduinoCommands(IEnumerable<STPFrame> inputFrames)
        {
            var convertedFrames = arduinoCommandCodec.Decode(inputFrames);
            if (convertedFrames != null)
            {
                foreach (var frame in convertedFrames)
                {
                    if (frame.Type == STPFrame.Types.ArduCommand)
                    {
                        if ((char)frame.Data[0] == 'D')
                        {
                            logger.LogIfDebug(this, "Arduino ping received");
                        }
                        else
                            yield return frame;
                    }
                }
            }

            yield break;
        }

        private void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            logger.LogIfDebug(this, "DataReceived event");

            bool is_error;
            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                var frames = codec.Decode(port);

                if (frames != null && frames.Any())
                {
					decodedFramesCount += frames.Count;

                    //Converting ArduCommands
                    var arduinoFrames = ProcessArduinoCommands(frames.Where(f => f.Type == STPFrame.Types.ArduCommand));
                    if (arduinoFrames != null)
                        frames.AddRange(arduinoFrames);

                    foreach (var acceptor in acceptors)
                    {
                        dispatcher.Invoke(this, null, (s, a) => acceptor.AcceptFrames(frames.Where(f => f.Type == acceptor.FrameType)));
                        logger.LogIfDebug(this, string.Format("Frames were dispatched for {0} acceptor", acceptor.FrameType));
                    }
                }

                is_error = false;
            }
            catch (Exception ex)
            {
                logger.Log(this, ex);
                is_error = true;
            }

            sw.Stop();
            UpdateMetrics(is_error, sw.ElapsedMilliseconds);
            IsCommunicationOk = !is_error;
        }

        private void UpdateMetrics(bool is_error, long elapsed)
        {
            var handler = MetricsUpdated;

            if (handler != null)
            {
                var metrics = new Metrics("Arduino Controller", 4);

                metrics.Add(0, metricReadedBytes, port.OverallReadedBytes);
                metrics.Add(1, metricDecodedFrames, decodedFramesCount);
                metrics.Add(2, metricElapsed, elapsed);
                metrics.Add(3, metricIsError, is_error);

                dispatcher.Invoke(this, null, new EventHandler((s,e) => handler(this, metrics)));
            }
        }

        public void RegisterFrameAcceptor(IFramesAcceptor acceptor)
        {
            lock (acceptors)
            {
                if (!acceptors.Contains(acceptor))
                    acceptors.Add(acceptor);
                else
                    throw new Exception("Acceptor already exist");
            }
        }

        public void UnregisterFrameAcceptor(IFramesAcceptor acceptor)
        {
            lock (acceptors)
            {
                if (acceptors.Contains(acceptor))
                    acceptors.Remove(acceptor);
                else
                    throw new Exception("Acceptor is not exist");
            }
        }
    }
}
