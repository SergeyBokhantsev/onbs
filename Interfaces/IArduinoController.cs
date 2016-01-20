using Interfaces.Relays;
using Interfaces.SerialTransportProtocol;

namespace Interfaces
{
    public enum ArduinoComands
    {
        //Outcoming
        PingRequest = 100,
        HoldPower = 102,
        SetTime = 105,
        GetTimeRequest = 106,
        Beep = 108,

        //Incoming
        PingResponce = 101,
        ComandFailed = 103,
        ShutdownSignal = 104,
        GetTimeResponse = 107,
        CommandConfirmation = 108
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
