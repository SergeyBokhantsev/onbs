using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public class IdleMeter
    {
        private volatile int lastActivityTime;

        public int IdleMinutes
        {
            get
            {
                return IdleMilliseconds / 60000;
            }
        }

        public int IdleSeconds
        {
            get
            {
                return IdleMilliseconds / 1000;
            }
        }

        public int IdleMilliseconds
        {
            get
            {
                return Environment.TickCount - lastActivityTime;
            }
        }

        public IdleMeter()
        {
            Reset();
        }

        public void Reset()
        {
            lastActivityTime = Environment.TickCount;
        }
    }
}
