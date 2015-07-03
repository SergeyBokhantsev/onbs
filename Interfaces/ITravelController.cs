using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface ITravelController : IController
    {
        string TravelName { get; }
        DateTime TravelStarted { get; }
        void StartNewTravel(string name);
        void RenameTravel(string name);
    }
}
