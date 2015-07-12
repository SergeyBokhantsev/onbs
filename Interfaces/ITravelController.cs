using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces.GPS;

namespace Interfaces
{
    public interface ITravelController : IController, IMetricsProvider
    {
        int BufferedPoints { get; }
        int SendedPoints { get; }
        /// <summary>
        /// Elapsed time in minutes from the very first received gps point
        /// </summary>
        TimeSpan TravelTime { get; }
        /// <summary>
        /// Path lenght in meters from the very first received gps point
        /// </summary>
        double TravelDistance { get; }
        void RequestNewTravel(string name);
        void MarkCurrentPositionWithCustomPoint();
    }
}
