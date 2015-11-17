using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp
{
    internal class NotAuthorizedException : Exception
    {
        public NotAuthorizedException()
            : base("Invalid user key was provided")
        {
        }
    }

    internal class TravelNotFoundException : Exception
    {
        public TravelNotFoundException(int id)
            : base(string.Format("Travel '{0}' was not found", id))
        {
        }
    }

    internal class LogEntryNotFoundException : Exception
    {
        public LogEntryNotFoundException(int id)
            : base(string.Format("LogEntry '{0}' was not found", id))
        {
        }
    }
}
