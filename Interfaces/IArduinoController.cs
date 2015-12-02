using Interfaces.SerialTransportProtocol;

namespace Interfaces
{
    public static class ArduinoComands
    {
        public const char ArduinoPingRequest = 'd';
        public const char ArduinoPingResponce = 'D';
    }

    public interface IArduinoController : IController, IMetricsProvider
    {
        bool IsCommunicationOk { get; }
        void RegisterFrameAcceptor(IFramesAcceptor acceptor);
        void UnregisterFrameAcceptor(IFramesAcceptor acceptor);
        void RegisterFrameProvider(IFrameProvider provider);
        void UnregisterFrameProvider(IFrameProvider provider);
    }
}
