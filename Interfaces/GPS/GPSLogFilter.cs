using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces.GPS
{
    public class GPSLogFilter
    {
        private readonly double distanceToSpeedRatio;
        private readonly int deadZoneMeters;
        private readonly int deadZoneSpeed;
        private GPRMC lastGprmc;

        public GPSLogFilter(double distanceToSpeedRatio, int deadZoneMeters, int deadZoneSpeed)
        {
            this.distanceToSpeedRatio = distanceToSpeedRatio;
            this.deadZoneMeters = deadZoneMeters;
            this.deadZoneSpeed = deadZoneSpeed;
        }

        public bool Match(GPRMC gprmc)
        {
            if (lastGprmc != null)
            {
                if (gprmc.Speed < deadZoneSpeed)
                    return false;

                var distance = Math.Abs(Interfaces.GPS.Helpers.GetDistance(lastGprmc.Location, gprmc.Location));

                if (distance < deadZoneMeters)
                    return false;

                var stepDistance = gprmc.Speed * distanceToSpeedRatio;

                if (distance >= stepDistance)
                {
                    lastGprmc = gprmc;
                    return true;
                }
                else
                    return false;
            }
            else
            {
                lastGprmc = gprmc;
                return true;
            }
        }
    }
}
