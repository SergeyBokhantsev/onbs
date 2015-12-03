using Interfaces.Input;
using Interfaces.SerialTransportProtocol;

namespace Interfaces
{
    public delegate void ButtonPressedEventHandler(Buttons button, ButtonStates state);

    public interface IInputController : IController, IFramesAcceptor
    {
        event ButtonPressedEventHandler ButtonPressed;
    }
}
