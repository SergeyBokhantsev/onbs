﻿using Interfaces;
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
    public class ArduinoMetricsProvider : MetricsProvider
    {
        public GenericMetric<string> Message = new GenericMetric<string>("Message", String.Empty);
        public GenericMetric<long> BytesReceived = new GenericMetric<long>("Bytes received", 0);
        public GenericMetric<long> FramesDecoded = new GenericMetric<long>("Frames decoded", 0);
        public GenericMetric<long> ProcessTime = new GenericMetric<long>("Process time", 0);
        public GenericMetric<int> PendingPing = new GenericMetric<int>("Pending ping", 0);

        public ArduinoMetricsProvider(ILogger logger)
            : base(logger, "Arduino Controller")
        {
            Initialize(Message, BytesReceived, FramesDecoded, ProcessTime, PendingPing);
        }
    }

    public class ArduinoController : IArduinoController
    {
        private readonly IPort port;
        private readonly ONBSSyncContext syncContext;
        private readonly ILogger logger;
		private readonly IHostController hc;
        private readonly ISTPCodec codec;

        private readonly ArduinoMetricsProvider metricsProvider;

        private readonly List<IFramesAcceptor> acceptors = new List<IFramesAcceptor>();
        private readonly List<IFrameProvider> providers = new List<IFrameProvider>();

        //private readonly Queue<Tuple<STPFrame, int>> outcomingQueue = new Queue<Tuple<STPFrame, int>>();
        //private readonly ManualResetEventSlim outcomingSignal = new ManualResetEventSlim(false);
        private readonly Queue<byte> outcomingDump = new Queue<byte>(1024);
        private readonly CommunicationQueue outcomingQueue;

        private readonly ArduinoFrameReceiver arduFrameReceiver;
        private long decodedFramesCount;
		private int ardPingPendings;

		private bool pingEnabled = true;
        private const int pingInterval = 3000;
        private DateTime pingTimestamp;

        public bool IsCommunicationOk
        {
            get
			{
				return ardPingPendings < 2 && outcomingQueue.CommunicationState;
			}
        }

        public IRelayService RelayService
        {
            get;
            private set;
        }

        public ILightSensorService LightSensorService
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

            metricsProvider = new ArduinoMetricsProvider(hc.Logger);
            hc.MetricsService.RegisterProvider(metricsProvider);

            var frameBeginMarker = Encoding.UTF8.GetBytes(":<:");
            var frameEndMarker = Encoding.UTF8.GetBytes(":>:");
            codec = new STPCodec(frameBeginMarker, frameEndMarker);			

            var relayService = new RelayService(hc.Logger);
            RegisterFrameProvider(relayService);
            RelayService = relayService;

            var lss = new LightSensorService(hc.Logger, hc.SyncContext);
            RegisterFrameProvider(lss);
            LightSensorService = lss;

            logger.Log(this, string.Format("{0} created.", this.GetType().Name), LogLevels.Info);

            arduFrameReceiver = new ArduinoFrameReceiver(this, hc.Logger);
            RegisterFrameAcceptor(arduFrameReceiver);

            port.DataReceived += DataReceived;

            outcomingQueue = new CommunicationQueue(hc.Config.GetInt(ConfigNames.ArduinoConfirmationTimeout));
            outcomingQueue.SendFrame += frame =>
                {
                    var serializedFrame = codec.Encode(frame);
                    port.Write(serializedFrame, 0, serializedFrame.Length);
                    Dump(serializedFrame);
                };

            var pingTimer = hc.CreateTimer(5000, t =>  
            {
                if (pingEnabled)
                {
                    var frameData = new byte[] { (byte)ArduinoComands.PingRequest };
                    Send(new STPFrame(frameData, STPFrame.Types.ArduCommand));
                    MetricMessage("Send PING");
                }
            }, true, true, "Arduino ping");
        }
        internal void ProcessArduinoCommand(STPFrame frame)
        {
            if (frame.Data.Length == 0)
            {
                logger.Log(this, string.Concat("Invalid arduino command frame was received. Raw frame: ", frame.ToString()), LogLevels.Warning);
                MetricMessage("Invalid command received", ColoredStates.Yellow);
            }
            else
            {
                var incomingCommand = (ArduinoComands)frame.Data[0];

                switch (incomingCommand)
                {
                    case ArduinoComands.CommandConfirmation:
                        if (frame.Data.Length == 3)
                        {
                            int id = (frame.Data[1] << 8) + frame.Data[2];
                            outcomingQueue.ConfirmFrame((ushort)id);
                            MetricMessage("Frame confirmation");
                        }
                        else
                        {
                            logger.Log(this, string.Concat("Invalid command confirmation frame was received. Raw frame: ", frame.ToString()), LogLevels.Warning);
                            MetricMessage("Frame confirmation invalid", ColoredStates.Yellow);
                        }
                        break;

                    case ArduinoComands.ComandFailed:
                        if (frame.Data.Length == 3)
                        {
                            var resultForFrameType = (STPFrame.Types)frame.Data[1];
                            byte result = frame.Data[2];
                            logger.Log(this, string.Format("An 'Arduino command failed' responce was received for frame type {0}; result {1}. Raw frame: {2}",
                                resultForFrameType, result, frame.ToString()), LogLevels.Warning);

                            LogDump();

                            MetricMessage("Command failed", ColoredStates.Yellow);
                        }
                        else
                        {
                            logger.Log(this, string.Concat("Invalid command result frame was received. Raw frame: ", frame.ToString()), LogLevels.Warning);
                            MetricMessage("Invalid command result", ColoredStates.Yellow);
                        }
                        break;

                    case ArduinoComands.PingResponce:
                        Interlocked.Exchange(ref ardPingPendings, 0);
                        logger.LogIfDebug(this, "Arduino ping received");
                        MetricMessage("PING response");
                        break;

                    case ArduinoComands.ShutdownSignal:
                        logger.Log(this, "Shutdown signal received from arduino", LogLevels.Info);
                        syncContext.Post(o => hc.Shutdown(HostControllerShutdownModes.Shutdown), null, "ArdController.ProcessArduinoCommand -> hc.Shutdown");
                        MetricMessage("SHUTDOWN signal");
                        break;

                    case ArduinoComands.GetTimeResponse:
                        if (frame.Data.Length == 7)
                        {
                            MetricMessage("GetTimeResponse");
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
                                handler(new DateTime(year, month, day, hour, min, sec, DateTimeKind.Local));
                            }
                        }
                        else
                        {
                            logger.Log(this, string.Concat("Get time respone invalid. Raw bytes: ", frame), LogLevels.Warning);
                            MetricMessage("Invalid GetTimeResponse", ColoredStates.Yellow);
                        }
                        GetArduinoTimeHandler = null;
                        break;

                    case ArduinoComands.LightSensorResponse:
                        MetricMessage("LightSensorResponse");
                        if (!((LightSensorService)LightSensorService).ProcessResponse(frame))
                        {
                            logger.Log(this, string.Concat("LightSensorResponse is invalid. Raw bytes: ", frame), LogLevels.Warning);
                        }
                        break;

                    default:
                        logger.Log(this, string.Concat("Unexpected command frame was received. Raw frame: ", frame.ToString()), LogLevels.Warning);
                        MetricMessage("Unexpected command received");
                        break;
                }
            }
        }

        private void MetricMessage(string message, ColoredStates state = ColoredStates.Normal)
        {
            metricsProvider.Message.Set(message, state);
        }

        private void Send(STPFrame frame)
        {
            outcomingQueue.Enqueue(frame);
        }

        //private void SendingLoop(object state)
        //{
        //    while (true)
        //    {
        //        Tuple<STPFrame, int> item = null;

        //        outcomingSignal.Wait(1000);

        //        if (pingEnabled && pingTimestamp.AddMilliseconds(pingInterval) < DateTime.Now)
        //        {
        //            var frameData = new byte[] { (byte)ArduinoComands.PingRequest };
        //            item = new Tuple<STPFrame,int>(new STPFrame(frameData, STPFrame.Types.ArduCommand), 0);
        //            Interlocked.Increment(ref ardPingPendings);
        //            logger.LogIfDebug(this, "Ping command sended to Arduino");
        //            pingTimestamp = DateTime.Now;
        //        }
        //        else
        //        {
        //            lock (outcomingQueue)
        //            {
        //                if (outcomingQueue.Any())
        //                {
        //                    item = outcomingQueue.Dequeue();
        //                }
        //                else
        //                {
        //                    outcomingSignal.Reset();
        //                    continue;
        //                }
        //            }
        //        }

        //        try
        //        {
        //            var serializedFrame = codec.Encode(item.Item1);
        //            port.Write(serializedFrame, 0, serializedFrame.Length);
        //            Dump(serializedFrame);

        //            if (item.Item2 > 0)
        //                Thread.Sleep(Math.Min(item.Item2, 3000));
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Log(this, "Exception sending data to Arduino", LogLevels.Error);
        //            logger.Log(this, ex);
        //        }
        //    }
        //}

        [Conditional("DEBUG")]
        private void Dump(byte[] bytes)
        {
            lock (outcomingDump)
            {
                foreach (var b in bytes)
                {
                    if (outcomingDump.Count == 1024)
                    {
                        outcomingDump.Dequeue();
                    }

                    outcomingDump.Enqueue(b);
                }
            }
        }

        [Conditional("DEBUG")]
        private void LogDump()
        {
            byte[] dump;

            lock (outcomingDump)
            {
                dump = outcomingDump.ToArray();
                outcomingDump.Clear();
            }

            var dumpStr = string.Concat("Arduino outcoming dump: ", string.Join(", ", dump.Select(b => b.ToString())));
            logger.Log(this, dumpStr, LogLevels.Warning);
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

					lock (acceptors)
					{
	                    foreach (var acceptor in acceptors)
	                    {
	                        syncContext.Post(o => acceptor.AcceptFrames(frames.Where(f => f.Type == acceptor.FrameType)), null, string.Concat("ArdController -> AcceptFrames, type ", acceptor.FrameType));
	                        logger.LogIfDebug(this, string.Format("Frames were dispatched for {0} acceptor", acceptor.FrameType));
	                    }
					}
                }
            }
            catch (Exception ex)
            {
                logger.Log(this, ex);
            }

            sw.Stop();

			UpdateMetrics(sw.ElapsedTicks);
        }

        private void UpdateMetrics(long elapsed)
        {
            metricsProvider.OpenBatch();

            metricsProvider.SummaryState = IsCommunicationOk ? ColoredStates.Normal : ColoredStates.Red;

            metricsProvider.BytesReceived.Set(port.OverallReadedBytes);
            metricsProvider.FramesDecoded.Set(decodedFramesCount);
            metricsProvider.ProcessTime.Set(elapsed);
            metricsProvider.PendingPing.Set(ardPingPendings);

            metricsProvider.CommitBatch();
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
            pingEnabled = false;
            logger.Log(this, "Stop pinging Arduino", LogLevels.Info);
		}

        public Task<bool> HoldPower()
        {
            logger.Log(this, "Sending HoldPower command to Arduino", LogLevels.Info);
            var frame = new STPFrame(new byte[] { (byte)ArduinoComands.HoldPower }, STPFrame.Types.ArduCommand);
            Send(frame);
            return frame.WaitDeliveryAsync(5000);
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
                              , STPFrame.Types.ArduCommand));
        }

        public void GetArduinoTime(Action<DateTime> handler)
        {
            logger.Log(this, "Sending get time request to Arduino", LogLevels.Info);
            GetArduinoTimeHandler = handler;
            Send(new STPFrame(new byte[] { (byte)ArduinoComands.GetTimeRequest }, STPFrame.Types.ArduCommand));
        }

        public Task<bool> Beep(ushort beepMs, ushort pauseMs = 0, byte count = 1)
        {
            logger.Log(this, "Sending beep request to Arduino", LogLevels.Info);
            var frame = new STPFrame(new byte[] { (byte)ArduinoComands.Beep,
            (byte)((beepMs >> 8) & 0xFF),
            (byte)(beepMs & 0xFF),
            (byte)((pauseMs >> 8) & 0xFF),
            (byte)(pauseMs & 0xFF),
            count
            }, STPFrame.Types.ArduCommand);
            Send(frame);
            return frame.WaitDeliveryAsync(5000);
        }
    }
}
