using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IPort
    {
        event SerialDataReceivedEventHandler DataReceived;

        long OverallReadedBytes { get; }
        int Read(byte[] buffer, int offset, int count);
    }
}
