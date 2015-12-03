using System;
using Interfaces.GPS;

namespace GPSD.Net.Messages
{
    /// <summary>
    /// http://www.catb.org/gpsd/gpsd_json.html
    /// </summary>
    internal class TPVMsg : Message
    {
        /// <summary>
        /// Time/date stamp in ISO8601 format, UTC. May have a fractional part of up to .001sec precision. May be absent if mode is not 2 or 3.
        /// </summary>
        public string time { get; set; }

        /// <summary>
        /// Estimated timestamp error (%f, seconds, 95% confidence). Present if time is present.
        /// </summary>
        public double ept { get; set; }

        /// <summary>
        /// Latitude in degrees: +/- signifies North/South. Present when mode is 2 or 3.
        /// </summary>
        public double lat { get; set; }

        /// <summary>
        /// Longitude in degrees: +/- signifies East/West. Present when mode is 2 or 3.
        /// </summary>
        public double lon { get; set; }

        /// <summary>
        /// Altitude in meters. Present if mode is 3.
        /// </summary>
        public double alt { get; set; }

        /// <summary>
        /// Longitude error estimate in meters, 95% confidence. Present if mode is 2 or 3 and DOPs can be calculated from the satellite view.
        /// </summary>
        public double epx { get; set; }

        /// <summary>
        /// Latitude error estimate in meters, 95% confidence. Present if mode is 2 or 3 and DOPs can be calculated from the satellite view.
        /// </summary>
        public double epy { get; set; }

        /// <summary>
        /// Estimated vertical error in meters, 95% confidence. Present if mode is 3 and DOPs can be calculated from the satellite view.
        /// </summary>
        public double epv { get; set; }

        /// <summary>
        /// Course over ground, degrees from true north.
        /// </summary>
        public double track { get; set; }

        /// <summary>
        /// Speed over ground, meters per second.
        /// </summary>
        public double speed { get; set; }

        /// <summary>
        /// Climb (positive) or sink (negative) rate, meters per second.
        /// </summary>
        public double climb { get; set; }

        /// <summary>
        /// Direction error estimate in degrees, 95% confidence.
        /// </summary>
        public double epd { get; set; }

        /// <summary>
        /// Speed error estinmate in meters/sec, 95% confidence.
        /// </summary>
        public double eps { get; set; }

        /// <summary>
        /// Climb/sink error estimate in meters/sec, 95% confidence.
        /// </summary>
        public double epc { get; set; }

        /// <summary>
        /// NMEA mode: %d, 0=no mode value yet seen, 1=no fix, 2=2D, 3=3D.
        /// </summary>
        public int mode { get; set; }

        public TPVMsg()
            :base("TPVMsg")
        {
        }

        public TPVMsg(GPRMC gprmc)
            :this()
        {
            mode = gprmc.Active ? 2 : 1;

            if (gprmc.Time != default(DateTime))
                time = gprmc.Time.ToString("yyyy-MM-ddTHH:mm:ssZ");

            if (mode > 1)
            {
                lat = gprmc.Location.Lat.Degrees;
                lon = gprmc.Location.Lon.Degrees;
                speed = gprmc.Speed * 1000d / 3600d;
                track = gprmc.TrackAngle;
            }
        }
    }
}
