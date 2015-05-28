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

        private readonly ILogger logger;
        private readonly STPCodec codec;
        private readonly NmeaParser parser;

        public STPFrame.Types FrameType
        {
            get { return STPFrame.Types.GPS; }
        }

        public GPSController(ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");

            this.logger = logger;

            codec = new STPCodec(
                new byte[] { (byte)'$' },
                new byte[] { 13, 10 },
                STPFrame.Types.GPS);

            parser = new NmeaParser();
            parser.GPRMC += parser_GPRMC;

            logger.Log("GPS controller created", LogLevels.Info);
        }

        private void parser_GPRMC(Interfaces.GPS.GPRMC obj)
        {
            var handler = GPRMCReseived;
            if (handler != null)
                handler(obj);
        }

        public void AcceptFrames(IEnumerable<STPFrame> frames)
        {
            var concatenatedNmea = new StringBuilder();

            var gpsFrames = codec.Decode(frames);

            if (gpsFrames != null && gpsFrames.Any())
            {
                foreach (var f in gpsFrames)
                {
                    var nmea = Encoding.Default.GetString(f.Data);
                    parser.Accept(nmea);
                    concatenatedNmea.Append(nmea);
                }

                if (concatenatedNmea.Length > 0)
                {
                    var concatenatedStr = concatenatedNmea.ToString();
                    logger.Log(string.Format("Received gps sentence: '{0}'", concatenatedStr), LogLevels.Debug);

                    var handler = NMEAReceived;
                    if (handler != null)
                        handler(concatenatedStr);
                }
            }
        }
    }
}
