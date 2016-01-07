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
		private readonly IHostController hc;
        private readonly ISTPCodec codec;
		private readonly ISTPCodec arduinoCommandCodec;

        private readonly List<IFramesAcceptor> acceptors = new List<IFramesAcceptor>();
        private readonly List<IFrameProvider> providers = new List<IFrameProvider>();

        private readonly Queue<Tuple<STPFrame, int>> outcomingQueue = new Queue<Tuple<STPFrame, int>>();
        private readonly ManualResetEventSlim outcomingSignal = new ManualResetEventSlim(false);

        private long decodedFramesCount;
		private int ardPingPendings;

		private IHostTimer pingTimer;

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

        private Action<DateTime> GetArduinoTimeHandler
        {
            get;
            set;
        }

        public ArduinoController(IPort port, IHostController hc)
        {
			this.hc = hc;
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

            pingTimer = hc.CreateTimer(5000, t => 
            {
                var frameData = new byte[] { (byte)ArduinoComands.PingRequest };
                Send(new STPFrame(frameData, STPFrame.Types.ArduCommand), 0);
				Interlocked.Increment(ref ardPingPendings);
                logger.LogIfDebug(this, "Ping command sended to Arduino");
                UpdateMetrics(0);
            }, true, true);

            logger.Log(this, string.Format("{0} created.", this.GetType().Name), LogLevels.Info);

            port.DataReceived += DataReceived;

            var sendingThread = new Thread(SendingLoop);
            sendingThread.IsBackground = true;
            sendingThread.Priority = ThreadPriority.Normal;
            sendingThread.Name = "Arduino send";
            sendingThread.Start();
        }

        private void Send(STPFrame frame, int delayAfterSend)
        {
            lock (outcomingQueue)
            {
                outcomingQueue.Enqueue(new Tuple<STPFrame, int>(frame, delayAfterSend));
            }

            outcomingSignal.Set();
        }

        private void SendingLoop(object state)
        {
            while (true)
            {
                outcomingSignal.Wait();

                Tuple<STPFrame, int> item = null;

                lock (outcomingQueue)
                {
                    if (outcomingQueue.Any())
                    {
                        item = outcomingQueue.Dequeue();
                    }
                    else
                    {
                        continue;
                    }
                }

                try
                {
                    var serializedFrame = codec.Encode(item.Item1);
                    port.Write(serializedFrame, 0, serializedFrame.Length);

                    if (item.Item2 > 0)
                        Thread.Sleep(item.Item2);
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
                        ProcessArduinoCommand(frame);
                    }
					else 
						yield return frame;
                }
            }

            yield break;
        }

        private void ProcessArduinoCommand(STPFrame frame)
        {

            if (frame.Data.Length == 0)
            {
                logger.Log(this, string.Concat("Invalid arduino command frame was received. Raw frame: ", frame.ToString()), LogLevels.Warning);
            }
            else
            {
                var incomingCommand = (ArduinoComands)frame.Data[0];

                switch (incomingCommand)
                {
                    case ArduinoComands.ComandFailed:
                        if (frame.Data.Length == 3)
                        {
                            var resultForFrameType = (STPFrame.Types)frame.Data[1];
                            byte result = frame.Data[2];
                            logger.Log(this, string.Format("An 'Arduino command failed' responce was received for frame type {0}; result {1}. Raw frame: {2}", 
                                resultForFrameType, result, frame.ToString()), LogLevels.Warning);
                        }
                        else
                        {
                            logger.Log(this, string.Concat("Invalid command result frame was received. Raw frame: ", frame.ToString()), LogLevels.Warning);
                        }
                        break;

                    case ArduinoComands.PingResponce:
                        Interlocked.Exchange(ref ardPingPendings, 0);
                        logger.LogIfDebug(this, "Arduino ping received");
                        break;

                    case ArduinoComands.ShutdownSignal:
                        logger.Log(this, "Shutdown signal received from arduino", LogLevels.Info);
                        syncContext.Post(o => hc.Shutdown(HostControllerShutdownModes.Shutdown), null);
                        break;

                    case ArduinoComands.GetTimeResponse:
                        if (frame.Data.Length == 7)
                        {
                            var handler = GetArduinoTimeHandler;
                            logger.Log(this, string.Concat("Get time respone received, ", handler != null ? "handler exist" : "handler doesn't exist"), LogLevels.Info);                            
                            if (handler != null)
                            {
                                var hour = (int)frame.Data[1];
                                var min = (int)frame.Data[2];
                                var sec = (int)frame.Data[3];
                                var day = (int)frame.Data[4];
                                var month = (int)frame.Data[5];
                                var year = (int)frame.Data[6] + 2000;
                                handler(new DateTime(year, month, day, hour, min, sec));
                            }
                        }
                        else
                        {
                            logger.Log(this, string.Concat("Get time respone invalid. Raw bytes: ", frame), LogLevels.Warning);
                        }
                        GetArduinoTimeHandler = null;
                        break;

                    default:
                        logger.Log(this, string.Concat("Unexpected command frame was received. Raw frame: ", frame.ToString()), LogLevels.Warning);
                        break;
                }
            }
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

		public void StopPing()
		{
            logger.Log(this, "Stop pinging Arduino", LogLevels.Info);
			pingTimer.Dispose ();
		}

        public void HoldPower()
        {
            logger.Log(this, "Sending HoldPower command to Arduino", LogLevels.Info);
            Send(new STPFrame(new byte[] { (byte)ArduinoComands.HoldPower }, STPFrame.Types.ArduCommand), 100);
        }

        public void SetTimeToArduino()
        {
            logger.Log(this, "Sending current time to Arduino", LogLevels.Info);
            var now = DateTime.Now;
            Send(new STPFrame(new byte[] { (byte)ArduinoComands.SetTime,
                                           (byte)now.Hour,
                                           (byte)now.Minute,
                                           (byte)now.Second,
                                           (byte)now.Day,
                                           (byte)now.Month,
                                           (byte)(now.Year - 2000)}
                              , STPFrame.Types.ArduCommand), 100);
        }

        public void GetArduinoTime(Action<DateTime> handler)
        {
            logger.Log(this, "Sending get time request to Arduino", LogLevels.Info);
            GetArduinoTimeHandler = handler;
            Send(new STPFrame(new byte[] { (byte)ArduinoComands.GetTimeRequest }, STPFrame.Types.ArduCommand), 100);
        }

        public void Beep(ushort beepMs, ushort pauseMs = 0, byte count = 1)
        {
            logger.Log(this, "Sending beep request to Arduino", LogLevels.Info);
            Send(new STPFrame(new byte[] { (byte)ArduinoComands.Beep,
            (byte)((beepMs >> 8) & 0xFF),
            (byte)(beepMs & 0xFF),
            (byte)((pauseMs >> 8) & 0xFF),
            (byte)(pauseMs & 0xFF),
            count
            }, STPFrame.Types.ArduCommand), 100);
        }
    }
}
