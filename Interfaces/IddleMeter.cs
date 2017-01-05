using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public class IdleMeter
    {
        private readonly ISessionConfig config;
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
                return config.Uptime - lastActivityTime;
            }
        }

        public IdleMeter(ISessionConfig config)
        {
            if (null == config)
                throw new ArgumentNullException("config");

            this.config = config;

            Reset();
        }

        public void Reset()
        {
            lastActivityTime = config.Uptime;
        }
    }
}
