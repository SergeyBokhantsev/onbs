﻿using Interfaces.Relays;
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
        LightSensorRequest = 110,

        //Incoming
        PingResponce = 101,
        ComandFailed = 103,
        ShutdownSignal = 104,
        GetTimeResponse = 107,
        CommandConfirmation = 109,
        LightSensorResponse = 111
    }

    public interface IArduinoController : IController
    {
        IRelayService RelayService { get; }
        ILightSensorService LightSensorService  { get; }
        bool IsCommunicationOk { get; }
        void RegisterFrameAcceptor(IFramesAcceptor acceptor);
        void UnregisterFrameAcceptor(IFramesAcceptor acceptor);
        void RegisterFrameProvider(IFrameProvider provider);
        void UnregisterFrameProvider(IFrameProvider provider);
    }
}
