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
        private readonly int maxSecondsGap;
        private readonly int maxMetersGap;
        private readonly List<GPRMC> points = new List<GPRMC>();
        private GPRMC lastGprmc;

        public int Count
        {
            get
            {
                return points.Count;
            }
        }

        public GPSLogFilter(int maxSecondsGap, int maxMetersGap)
        {
            this.maxSecondsGap = maxSecondsGap;
            this.maxMetersGap = maxMetersGap;
        }

        public void Log(GPRMC gprmc)
        {
            if (Match(gprmc))
            {
                lock (points)
                {
                    points.Add(gprmc);
                }
            }
        }

        public GPRMC[] WithdrawPoints()
        {
            lock (points)
            {
                if (points.Any())
                {
                    var result = points.ToArray();
                    points.Clear();
                    return result;
                }
                else
                    return null;
            }
        }

        private bool Match(GPRMC gprmc)
        {
            if (lastGprmc != null)
            {
                var secondsGap = (gprmc.Time - lastGprmc.Time).TotalSeconds;

                if (secondsGap < maxSecondsGap)
                {
                    var distanceGap = Math.Abs(Interfaces.GPS.Helpers.GetDistance(lastGprmc.Location, gprmc.Location));

                    if (distanceGap < maxMetersGap)
                        return false;
                }
            }

            lastGprmc = gprmc;

            return true;
        }
    }
}
