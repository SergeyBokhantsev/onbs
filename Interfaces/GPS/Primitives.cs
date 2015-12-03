using System;

namespace Interfaces.GPS
{
    public struct GeoCoordinate
    {
        private const double equolityTolerance = 0.000001;

        public double Degrees;

        public GeoCoordinate(double degrees)
        {
            Degrees = degrees;
        }

        #region OPERATORS OVERLOAD
        public static bool operator >(GeoCoordinate c1, GeoCoordinate c2)
        {
            return c1.Degrees > c2.Degrees;
        }

        public static bool operator >(GeoCoordinate c, double d)
        {
            return c.Degrees > d;
        }

        public static bool operator >=(GeoCoordinate c1, GeoCoordinate c2)
        {
            return c1.Degrees >= c2.Degrees;
        }

        public static bool operator >=(GeoCoordinate c, double d)
        {
            return c.Degrees >= d;
        }

        public static bool operator <(GeoCoordinate c1, GeoCoordinate c2)
        {
            return c1.Degrees < c2.Degrees;
        }

        public static bool operator <(GeoCoordinate c, double d)
        {
            return c.Degrees < d;
        }

        public static bool operator <=(GeoCoordinate c1, GeoCoordinate c2)
        {
            return c1.Degrees <= c2.Degrees;
        }

        public static bool operator <=(GeoCoordinate c, double d)
        {
            return c.Degrees <= d;
        }

        public static bool operator ==(GeoCoordinate c1, GeoCoordinate c2)
        {
            return Math.Abs(c1.Degrees - c2.Degrees) < equolityTolerance;
        }

        public static bool operator ==(GeoCoordinate c, double d)
        {
            return Math.Abs(c.Degrees - d) < equolityTolerance;
        }

        public static bool operator !=(GeoCoordinate c1, GeoCoordinate c2)
        {
            return Math.Abs(c1.Degrees - c2.Degrees) > equolityTolerance;
        }

        public static bool operator !=(GeoCoordinate c, double d)
        {
            return Math.Abs(c.Degrees - d) > equolityTolerance;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is double)
                return Math.Abs(this.Degrees - (double)obj) < equolityTolerance;

            if (obj is int)
                return Math.Abs(this.Degrees - (int)obj) < equolityTolerance;

            if (obj is GeoCoordinate)
                return Math.Abs(this.Degrees - ((GeoCoordinate)obj).Degrees) < equolityTolerance;

            return false;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public static GeoCoordinate operator +(GeoCoordinate c, double d)
        {
            return new GeoCoordinate { Degrees = c.Degrees + d };
        }

        public static double operator +(GeoCoordinate c1, GeoCoordinate c2)
        {
            return c1.Degrees + c2.Degrees;
        }

        public static double operator -(GeoCoordinate c1, GeoCoordinate c2)
        {
            return c1.Degrees - c2.Degrees;
        }

        public static GeoCoordinate operator -(GeoCoordinate c, double d)
        {
            return new GeoCoordinate(c.Degrees - d);
        }

        public static GeoCoordinate operator /(GeoCoordinate c, double d)
        {
            return new GeoCoordinate { Degrees = c.Degrees / d };
        }

        public static GeoCoordinate operator *(GeoCoordinate c, double d)
        {
            return new GeoCoordinate { Degrees = c.Degrees * d };
        }
        #endregion

        public static explicit operator GeoCoordinate(double value)
        {
            return new GeoCoordinate { Degrees = value };
        }

		public override string ToString ()
		{
			return Degrees.ToString ();
		}
    }

    public struct GeoPoint
    {
        public GeoCoordinate Lat;
        public GeoCoordinate Lon;

        public GeoPoint(double lat, double lon)
        {
            Lat = new GeoCoordinate { Degrees = lat };
            Lon = new GeoCoordinate { Degrees = lon };
        }

        public bool Valid
        {
            get
            {
                return Lat != 0 && Lon != 0;
            }
        }

        public override string ToString()
        {
            return String.Concat(Lat.Degrees.ToString("F4"), ", ", Lon.Degrees.ToString("F4"));
        }
    }

    public struct GeoSize
    {
        public double Lat;
        public double Lon;
    }

    public struct GeoRect
    {
        private const double equolityTolerance = 0.000001;

        public GeoCoordinate Lat;
        public GeoCoordinate Lon;
        public double LatSize;
        public double LonSize;

        public bool Contains(GeoPoint p)
        {
            return p.Lat >= Lat && p.Lat <= Lat + LatSize && p.Lon >= Lon && p.Lon <= Lon.Degrees + LonSize;
        }

        public bool Valid
        {
            get
            {
                return Math.Abs(Lat.Degrees) > equolityTolerance && Math.Abs(Lon.Degrees) > equolityTolerance;
            }
        }

        public void Clear()
        {
            Lat.Degrees = 0;
            Lon.Degrees = 0;
        }

        public GeoRect Inflate(double latSize, double lonSize)
        {
            return new GeoRect
            {
                Lat = (GeoCoordinate)(this.Lat.Degrees - latSize / 2),
                Lon = (GeoCoordinate)(this.Lon.Degrees - lonSize / 2),
                LatSize = this.LatSize + latSize,
                LonSize = this.LonSize + lonSize
            };
        }
    }

    public class GPRMC
    {
        private GeoPoint location;
        private DateTime time;

        public GeoPoint Location { get { return location; } }
        public DateTime Time { get { return time; } }
        public bool Active { get; private set; }
        public double Speed { get; private set; }
        public double TrackAngle { get; set; }
        public double MagneticVariation { get; private set; }

        public long Revision { get; private set; }

        public GPRMC()
        {
        }

        public GPRMC(GeoPoint location, DateTime time, bool active, long revision)
        {
            this.location = location;
            this.time = time;
            Active = active;
            Revision = revision;
        }

        public bool Parse(string[] items)
        {
            const int i_lat = 3;
            const int i_lng = 5;

            try
            {
                var hour = 0;
                var min = 0;
                var seconds = 0;

                //GPRMC,123519,A,4807.038,N,01131.000,E,022.4,084.4,230394,003.1,W
                if (items[1].Length > 0)
                {
                    hour = int.Parse(items[1].Substring(0, 2));
                    min = int.Parse(items[1].Substring(2, 2));
                    seconds = int.Parse(items[1].Substring(4, 2));
                }

                if (items[2].Length == 1)
                    Active = items[2] == "A";

                if (items[i_lat].Length > 0)
                {
                    location.Lat.Degrees = int.Parse(items[i_lat].Substring(0, 2));
                    location.Lat.Degrees += double.Parse(items[i_lat].Substring(2)) / 60d;
                }

                if (items[i_lng].Length > 0)
                {
                    location.Lon.Degrees = int.Parse(items[i_lng].Substring(0, 3));
                    location.Lon.Degrees += double.Parse(items[i_lng].Substring(3)) / 60d;
                }

                if (items[7].Length > 0)
                {
                    Speed = double.Parse(items[7]) * 1.852;
                }

                if (items[8].Length > 0)
                {
                    TrackAngle = double.Parse(items[8]);
                }

                if (items[9].Length > 0)
                {
                    var day = int.Parse(items[9].Substring(0, 2));
                    var month = int.Parse(items[9].Substring(2, 2));
                    var year = 2000 + int.Parse(items[9].Substring(4, 2));

                    time = new DateTime(year, month, day, hour, min, seconds);
                }

                if (items[10].Length > 0)
                {
                    MagneticVariation = double.Parse(items[10]);
                }

                Revision++;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
