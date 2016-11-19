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
    public class GPSMetricsProvider : MetricsProvider
    {
        public GenericMetric<int> GPSFrames = new GenericMetric<int>("GPS Frames", 0);
        public GenericMetric<int> NMEA = new GenericMetric<int>("NMEA", 0);
        public GenericMetric<int> GPRMC = new GenericMetric<int>("GPRMC", 0);
        public GenericMetric<GeoPoint> Location = new GenericMetric<GeoPoint>("Location", new GeoPoint());

        public GPSMetricsProvider(ILogger logger)
            : base(logger, "GPS Controller")
        {
            Initialize(GPSFrames, NMEA, GPRMC, Location);
        }
    }

    public class GPSController : IGPSController, IFramesAcceptor
    {
        public event Action<Interfaces.GPS.GPRMC> GPRMCReseived;
        public event Action<string> NMEAReceived;

        private readonly ONBSSyncContext syncContext;
        private readonly ILogger logger;
        private readonly IConfig config;
        private readonly STPCodec codec;
        private readonly NmeaParser parser;
        private readonly IMetricsService metricsService;

        private int gpsFramesCount;
        private int nmeaSentencesCount;
        private int gprmcCount;
        private GPRMC lastGprmc;
        private LockingProperty<GeoPoint> location = new LockingProperty<GeoPoint>();
        private bool shutdown;

        private readonly IdleMeter gpsCoordinateIdleMeter = new IdleMeter();
        private GeoPoint idleBasePoint;

        private GPSMetricsProvider metricsProvider;

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

        public int IdleMinutes 
        {
            get 
            {
                return gpsCoordinateIdleMeter.IdleMinutes;
            }
        }

        public GPSController(IConfig config, ONBSSyncContext syncContext, ILogger logger, IMetricsService metricsService)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            if (syncContext == null)
                throw new ArgumentNullException("syncContext");

            if (metricsService == null)
                throw new ArgumentNullException("metricsService");

            if (logger == null)
                throw new ArgumentNullException("logger");

            this.config = config;
            this.syncContext = syncContext;
            this.logger = logger;
            this.metricsService = metricsService;

            metricsProvider = new GPSMetricsProvider(logger);
            metricsService.RegisterProvider(metricsProvider);
            

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

            if (obj.Active && Interfaces.GPS.Helpers.GetDistance(idleBasePoint, obj.Location) > 50)
            {
                idleBasePoint = obj.Location;
                gpsCoordinateIdleMeter.Reset();
            }

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
            metricsProvider.OpenBatch();

            metricsProvider.SummaryState = is_error ? ColoredStates.Red : ColoredStates.Normal;

            metricsProvider.GPSFrames.Set(gpsFramesCount);
            metricsProvider.NMEA.Set(nmeaSentencesCount);
            metricsProvider.GPRMC.Set(gprmcCount);
			metricsProvider.Location.Set(lastGprmc.Location, lastGprmc.Active ? ColoredStates.Normal : ColoredStates.Red);

            metricsProvider.CommitBatch();
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

            metricsService.UnregisterProvider(metricsProvider);

            GPRMCReseived = null;
            NMEAReceived = null;

            SaveLocation();

            logger.Log(this, "GPS Controller shutdown", LogLevels.Info);
        }
    }
}
