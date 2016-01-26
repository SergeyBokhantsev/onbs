using System;

namespace Interfaces.GPS
{
    public class GPSLogFilter
    {
        private readonly double distanceToSpeedRatio;
        private readonly int deadZoneMeters;
        private readonly int deadZoneSpeed;
        private GPRMC lastAcceptedGprmc;
        private GPRMC lastComedGprmc;

        public GPRMC LastKnownLocation
        {
            get { return lastComedGprmc; }
        }

        public GPSLogFilter(double distanceToSpeedRatio, int deadZoneMeters, int deadZoneSpeed)
        {
            this.distanceToSpeedRatio = distanceToSpeedRatio;
            this.deadZoneMeters = deadZoneMeters;
            this.deadZoneSpeed = deadZoneSpeed;
        }

        public bool Match(GPRMC gprmc)
        {
            lastComedGprmc = gprmc;

            if (lastAcceptedGprmc != null)
            {
                if (gprmc.Speed < deadZoneSpeed)
                    return false;

                var distance = Math.Abs(Helpers.GetDistance(lastAcceptedGprmc.Location, gprmc.Location));

                if (distance < deadZoneMeters)
                    return false;

                var stepDistance = gprmc.Speed * distanceToSpeedRatio;

                if (distance >= stepDistance)
                {
                    lastAcceptedGprmc = gprmc;
                    return true;
                }
                else
                    return false;
            }
            else
            {
                lastAcceptedGprmc = gprmc;
                return true;
            }
        }
    }
}
