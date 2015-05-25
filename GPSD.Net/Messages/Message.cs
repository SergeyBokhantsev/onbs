using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPSD.Net.Messages
{
    internal abstract class Message
    {
        public string @class { get; private set; }

        protected Message(string className)
        {
            @class = className;
        }
    }
}
