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
using System.Threading;
using Interfaces.Relays;

namespace ArduinoController
{
    public class ArduinoController : IArduinoController
    {
        public event MetricsUpdatedEventHandler MetricsUpdated;

        private const string metricReadedBytes = "Bytes received";
        private const string metricDecodedFrames = "Frames decoded";
        private const string metricElapsed = "Process time";
		private const string metricPendingPing = "Pending ping";
        private const string metricIsError = "_is_error";

        private readonly IPort port;
        private readonly SynchronizationContext syncContext;
        private readonly ILogger logger;
        private readonly ISTPCodec codec;
		private readonly ISTPCodec arduinoCommandCodec;

        private readonly List<IFramesAcceptor> acceptors = new List<IFramesAcceptor>();
        private readonly List<IFrameProvider> providers = new List<IFrameProvider>();

        private long decodedFramesCount;
		private int ardPingPendings;

        public bool IsCommunicationOk
        {
            get
			{
				return ardPingPendings < 2;
			}
        }

        public IRelayService RelayService
        {
            get;
            private set;
        }

        public ArduinoController(IPort port, IHostController hc)
        {
            this.port = port;
            this.syncContext = hc.SyncContext;
            this.logger = hc.Logger;

            var frameBeginMarker = Encoding.UTF8.GetBytes(":<:");
            var frameEndMarker = Encoding.UTF8.GetBytes(":>:");
            codec = new STPCodec(frameBeginMarker, frameEndMarker);

			var arduFrameBeginMarker = Encoding.UTF8.GetBytes("ac{");
			var arduFrameEndMarker = Encoding.UTF8.GetBytes("}");
			arduinoCommandCodec = new STPCodec(arduFrameBeginMarker, arduFrameEndMarker);

            var relayService = new RelayService(hc.Logger);
            RegisterFrameProvider(relayService);
            RelayService = relayService;

            hc.CreateTimer(5000, t => 
            {
                var frameData = new byte[] { ArduinoComands.Ping };
                Send(new STPFrame(frameData, STPFrame.Types.ArduCommand), 0);
				Interlocked.Increment(ref ardPingPendings);
                logger.LogIfDebug(this, "Ping command sended to Arduino");
                UpdateMetrics(0);
            }, true, true);

            logger.Log(this, string.Format("{0} created.", this.GetType().Name), LogLevels.Info);

            port.DataReceived += DataReceived;
        }

        private void Send(STPFrame frame, int delayAfterSend)
        {
            lock (port)
            {
                try
                {
                    var serializedFrame = codec.Encode(frame);
                    port.Write(serializedFrame, 0, serializedFrame.Length);

                    if (delayAfterSend > 0)
                        Thread.Sleep(delayAfterSend);
                }
                catch (Exception ex)
                {
                    logger.Log(this, "Exception sending data to Arduino", LogLevels.Error);
                    logger.Log(this, ex);
                }
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
						var responseOnFrame = (STPFrame.Types)frame.Data[0];

						if (responseOnFrame == STPFrame.Types.ArduCommand
						    && frame.Data.Length > 1
						    && frame.Data[1] == ArduinoComands.Ping)
                        {
							Interlocked.Exchange(ref ardPingPendings, 0);
                            logger.LogIfDebug(this, "Arduino ping received");
                        }
						else
						{
                            logger.Log(this, 
                                string.Format("Error frame responce: type {0}, all bytes: {1}", responseOnFrame, string.Concat(frame.Data.Select(b => string.Concat("'", b.ToString(), ", ")))), 
                                LogLevels.Error);
						}
                    }
					else 
						yield return frame;
                }
            }

            yield break;
        }

        private void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            logger.LogIfDebug(this, "DataReceived event");

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
                        syncContext.Post(o => acceptor.AcceptFrames(frames.Where(f => f.Type == acceptor.FrameType)), null);
                        logger.LogIfDebug(this, string.Format("Frames were dispatched for {0} acceptor", acceptor.FrameType));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(this, ex);
            }

            sw.Stop();
            UpdateMetrics(sw.ElapsedMilliseconds);
        }

        private void UpdateMetrics(long elapsed)
        {
            var handler = MetricsUpdated;

            if (handler != null)
            {
                var metrics = new Metrics("Arduino Controller", 5);

                metrics.Add(0, metricReadedBytes, port.OverallReadedBytes);
                metrics.Add(1, metricDecodedFrames, decodedFramesCount);
                metrics.Add(2, metricElapsed, elapsed);
				metrics.Add(3, metricPendingPing, ardPingPendings);
                metrics.Add(4, metricIsError, !IsCommunicationOk);

                syncContext.Post(o => handler(this, metrics), null);
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

        public void RegisterFrameProvider(IFrameProvider provider)
        {
            provider.FrameToSend += Send;
        }

        public void UnregisterFrameProvider(IFrameProvider provider)
        {
            provider.FrameToSend -= Send;
        }

        public void HoldPower()
        {
            Send(new STPFrame(new byte[] { ArduinoComands.HoldPower }, STPFrame.Types.ArduCommand), 100);
        }
    }
}
