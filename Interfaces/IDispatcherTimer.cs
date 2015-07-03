using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IDispatcherTimer : IDisposable
    {
        int SpanSeconds { get; set; }
        bool Enabled { get; set; }
    }
}
