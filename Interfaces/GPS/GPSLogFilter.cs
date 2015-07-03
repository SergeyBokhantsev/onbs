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
        private List<GPRMC> points = new List<GPRMC>();
        private GPRMC lastGprmc;

        public GPSLogFilter()
        {
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
            throw new NotImplementedException();
        }
    }
}
