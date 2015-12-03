using System;

namespace Interfaces
{
    public interface IHostTimer : IDisposable
    {
        /// <summary>
        /// Interval in milliseconds. 
        /// Warning: minimal span depends on Dispatcher implementation!
        /// </summary>
        int Span { get; set; }
        bool IsEnabled { get; set; }
    }
}
