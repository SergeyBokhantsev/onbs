using Interfaces.Relays;
using Interfaces.SerialTransportProtocol;

namespace Interfaces
{
    public static class ArduinoComands
    {
        public const byte Ping = 100;
        public const byte HoldPower = 101;
    }

    public interface IArduinoController : IController, IMetricsProvider
    {
        IRelayService RelayService { get; }
        bool IsCommunicationOk { get; }
        void RegisterFrameAcceptor(IFramesAcceptor acceptor);
        void UnregisterFrameAcceptor(IFramesAcceptor acceptor);
        void RegisterFrameProvider(IFrameProvider provider);
        void UnregisterFrameProvider(IFrameProvider provider);
        void HoldPower();
    }
}
