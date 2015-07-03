using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IDispatcherTimer : IDisposable
    {
        /// <summary>
        /// Interval in milliseconds. 
        /// Warning: minimal span depends on Dispatcher implementation!
        /// </summary>
        int Span { get; set; }
        bool Enabled { get; set; }
    }
}
