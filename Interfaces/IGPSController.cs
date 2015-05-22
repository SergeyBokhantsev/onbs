using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces.GPS;

namespace Interfaces
{
    public interface IGPSController : IController
    {
        event Action<GPRMC> GPRMCReseived;
    }
}
