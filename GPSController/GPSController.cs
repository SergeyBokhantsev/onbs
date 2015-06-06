using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.SerialTransportProtocol;
using SerialTransportProtocol;

namespace GPSController
{
    public class GPSController : IGPSController, IFramesAcceptor
    {
        public event Action<Interfaces.GPS.GPRMC> GPRMCReseived;
        public event Action<string> NMEAReceived;

        public event MetricsUpdatedEventHandler MetricsUpdated;

        private readonly IDispatcher dispatcher;
        private readonly ILogger logger;
        private readonly STPCodec codec;
        private readonly NmeaParser parser;

        private int gpsFramesCount;
        private int nmeaSentencesCount;
        private int gprmcCount;

        public STPFrame.Types FrameType
        {
            get { return STPFrame.Types.GPS; }
        }

        public GPSController(IDispatcher dispatcher, ILogger logger)
        {
            if (dispatcher == null)
                throw new ArgumentNullException("dispatcher");

            if (logger == null)
                throw new ArgumentNullException("logger");

            this.dispatcher = dispatcher;
            this.logger = logger;

            codec = new STPCodec(
                new byte[] { (byte)'$' },
                new byte[] { 13, 10 },
                STPFrame.Types.GPS);

            parser = new NmeaParser();
            parser.GPRMC += parser_GPRMC;

            logger.Log(this, "GPS controller created", LogLevels.Info);
        }

        private void parser_GPRMC(Interfaces.GPS.GPRMC obj)
        {
            Interlocked.Increment(ref gprmcCount);

            var handler = GPRMCReseived;
            if (handler != null)
                handler(obj);
        }

        public void AcceptFrames(IEnumerable<STPFrame> frames)
        {
            if (frames != null && frames.Any())
            {
                Interlocked.Increment(ref gpsFramesCount);

                var concatenatedNmea = new StringBuilder();

                var nmeaSentences = codec.Decode(frames);

                if (nmeaSentences != null && nmeaSentences.Any())
                {
					Interlocked.Add (ref nmeaSentencesCount, nmeaSentences.Count);

                    foreach (var f in nmeaSentences)
                    {
                        var nmea = Encoding.Default.GetString(f.Data);
                        parser.Accept(nmea);
                        concatenatedNmea.Append(nmea);
                    }

                    if (concatenatedNmea.Length > 0)
                    {
                        var concatenatedStr = concatenatedNmea.ToString();
                        logger.Log(this, string.Format("Received gps sentence: '{0}'", concatenatedStr), LogLevels.Debug);

                        var handler = NMEAReceived;
                        if (handler != null)
                            handler(concatenatedStr);
                    }
                }

                UpdateMetrics(false);
            }
        }

        private void UpdateMetrics(bool is_error)
        {
            var handler = MetricsUpdated;

            if (handler != null)
            {
                var metrics = new Metrics("GPS Controller", 5);

                metrics.Add(0, "GPS Frames", gpsFramesCount);
                metrics.Add(1, "NMEA", nmeaSentencesCount);
                metrics.Add(2, "GPRMC", gprmcCount);
                metrics.Add(3, "Loc", parser.LastGPRMC.Location);
                metrics.Add(4, "_is_error", false);

                dispatcher.Invoke(null, null, new EventHandler((s, e) => handler(this, metrics)));
            }
        }
    }
}
