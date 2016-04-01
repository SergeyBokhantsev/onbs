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
        public override void Post(System.Threading.SendOrPostCallback callback, object state, string details)
        {
        }
    }
}
