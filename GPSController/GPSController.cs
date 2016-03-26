using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.SerialTransportProtocol;
using SerialTransportProtocol;
using Interfaces.GPS;
using System.Diagnostics;

namespace GPSController
{
    public class GPSController : IGPSController, IFramesAcceptor
    {
        public event Action<Interfaces.GPS.GPRMC> GPRMCReseived;
        public event Action<string> NMEAReceived;

        public event MetricsUpdatedEventHandler MetricsUpdated;

        private readonly ONBSSyncContext syncContext;
        private readonly ILogger logger;
        private readonly IConfig config;
        private readonly STPCodec codec;
        private readonly NmeaParser parser;

        private int gpsFramesCount;
        private int nmeaSentencesCount;
        private int gprmcCount;
        private GPRMC lastGprmc;
        private LockingProperty<GeoPoint> location = new LockingProperty<GeoPoint>();
        private bool shutdown;

        public GeoPoint Location
        {
            get
            {
                return location.Value;
            }
        }

        public STPFrame.Types FrameType
        {
            get { return STPFrame.Types.GPS; }
        }

        public GPSController(IConfig config, ONBSSyncContext syncContext, ILogger logger)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            if (syncContext == null)
                throw new ArgumentNullException("syncContext");

            if (logger == null)
                throw new ArgumentNullException("logger");

            this.config = config;
            this.syncContext = syncContext;
            this.logger = logger;

            codec = new STPCodec(
                new byte[] { (byte)'$' },
                new byte[] { 13, 10 },
                true);

            lastGprmc = new GPRMC();

            ReadLocation();

            parser = new NmeaParser();
            parser.GPRMC += parser_GPRMC;

            logger.Log(this, "GPS controller created", LogLevels.Info);
        }

        private void parser_GPRMC(Interfaces.GPS.GPRMC obj)
        {
            Interlocked.Increment(ref gprmcCount);

			//obj = new GPRMC (new GeoPoint (50.5, 30.5), DateTime.Now, true, gprmcCount);

            lastGprmc = obj;

            if (obj.Active)
                location.Value = obj.Location;

            var handler = GPRMCReseived;
            if (!shutdown && handler != null)
                handler(obj);
        }

		[Conditional("DEBUG")]
		private void LogIncomingRaw(IEnumerable<STPFrame> frames)
		{
			StringBuilder sb = new StringBuilder();

			foreach (var f in frames)
			{
				var str = Encoding.Default.GetString (f.Data);
				sb.Append (str);
			}

			logger.LogIfDebug (this, sb.ToString ());
		}

        public void AcceptFrames(IEnumerable<STPFrame> transportFrames)
        {
            if (!shutdown && transportFrames != null && transportFrames.Any())
            {
                Interlocked.Increment(ref gpsFramesCount);

				//LogIncomingRaw (frames);

                var concatenatedNmea = new StringBuilder();

                var nmeaSentences = codec.Decode(transportFrames);

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
            }

            UpdateMetrics(lastGprmc == null || !lastGprmc.Active);
        }

        private void UpdateMetrics(bool is_error)
        {
            var handler = MetricsUpdated;

            if (handler != null && !shutdown)
            {
                var metrics = new Metrics("GPS Controller", 5);

                metrics.Add(0, "GPS Frames", gpsFramesCount);
                metrics.Add(1, "NMEA", nmeaSentencesCount);
                metrics.Add(2, "GPRMC", gprmcCount);
                metrics.Add(3, "Loc", lastGprmc.Location);
                metrics.Add(4, "_is_error", is_error);

                syncContext.Post(PostMetrics, metrics, "GPSController.UpdateMetrics");
            }
        }

        private void PostMetrics(object state)
        {
            var handler = MetricsUpdated;

            if (handler != null && !shutdown)
                handler(this, state as Metrics);
        }

        private void ReadLocation()
        {
            try
            {
                var items = config.GetString(ConfigNames.GPSLocation).Split(';');
                location.Value = new GeoPoint(double.Parse(items[0]), double.Parse(items[1]));
            }
            catch (Exception ex)
            {
                logger.Log(this, string.Concat("Unable to read GPS Location from configuration: ", ex.Message), LogLevels.Warning);
                location.Value = new GeoPoint(50.5, 30.5);
            }
        }

        private void SaveLocation()
        {
            var loc = location.Value;
            config.Set<string>(ConfigNames.GPSLocation, string.Concat(loc.Lon.Degrees.ToString(), ";", loc.Lat.Degrees.ToString()));
        }

        public void Shutdown()
        {
            shutdown = true;

            GPRMCReseived = null;
            NMEAReceived = null;
            MetricsUpdated = null;

            SaveLocation();

            logger.Log(this, "GPS Controller shutdown", LogLevels.Info);
        }
    }
}
