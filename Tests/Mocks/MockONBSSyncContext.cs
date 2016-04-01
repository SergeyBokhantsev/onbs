using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;

namespace Tests.Mocks
{
    public class MockONBSSyncContext : ONBSSyncContext
    {
        public override void Post(System.Threading.SendOrPostCallback d, object state, string details)
        {
            if (d != null)
                d(state);
        }

        public override void Post(System.Threading.SendOrPostCallback d, object state)
        {
            if (d != null)
                d(state);
        }

        public override void Send(System.Threading.SendOrPostCallback d, object state)
        {
            if (d != null)
                d(state);
        }
    }
}
