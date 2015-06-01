using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces.GPS
{
    public static class Helpers
    {
        public static double DegreeToRadian(double degree)
        {
            return degree * 0.0174444444444444;
        }

        public static double RadianToDegree(double radian)
        {
            return radian * 57.2957795130823208767981;
        }

        public static double GetHeading(GeoPoint start, GeoPoint end)
        {
            var startLat = DegreeToRadian(start.Lat.Degrees);
            var startLong = DegreeToRadian(start.Lon.Degrees);
            var endLat = DegreeToRadian(end.Lat.Degrees);
            var endLong = DegreeToRadian(end.Lon.Degrees);

            var dLong = endLong - startLong;

            var dPhi = Math.Log(Math.Tan(endLat / 2d + Math.PI / 4d) / Math.Tan(startLat / 2d + Math.PI / 4d));

            if (Math.Abs(dLong) > Math.PI)
            {
                if (dLong > 0d)
                    dLong = -(2d * Math.PI - dLong);
                else
                    dLong = (2d * Math.PI + dLong);
            }

            return (RadianToDegree(Math.Atan2(dLong, dPhi)) + 360.0) % 360.0;
        }
    }
}

