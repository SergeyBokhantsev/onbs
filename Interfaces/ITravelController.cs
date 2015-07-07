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
        void RequestNewTravel(string name);
        void MarkCurrentPositionWithCustomPoint();
    }
}
