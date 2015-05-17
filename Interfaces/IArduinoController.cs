using Interfaces.SerialTransportProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IArduinoController : IController
    {
        IList<IFramesAcceptor> FrameAcceptors { get; }
    }
}
