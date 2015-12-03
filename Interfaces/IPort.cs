using System.IO.Ports;

namespace Interfaces
{
    public interface IPort
    {
        event SerialDataReceivedEventHandler DataReceived;

        long OverallReadedBytes { get; }
        int Read(byte[] buffer, int offset, int count);
        void Write(byte[] buffer, int offset, int count);
    }
}
