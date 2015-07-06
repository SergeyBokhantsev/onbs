using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface ITravelController : IController, IMetricsProvider
    {
        //string TravelName { get; }
        //DateTime TravelStarted { get; }
        void RequestNewTravel();
        //void RenameTravel(string name);
    }
}
