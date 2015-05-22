using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces.GPS
{
    public struct Coordinate
    {
        /// <summary>
        /// Y axis
        /// </summary>
        public double Lat;

        /// <summary>
        /// X axis
        /// </summary>
        public double Lon;
    }

    public struct GPRMC
    {
        public long Rev;
        public bool Active;
        public Coordinate Location;
    }
}
