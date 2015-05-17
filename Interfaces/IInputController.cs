using Interfaces.Input;
using Interfaces.SerialTransportProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public delegate void ButtonPressedEventHandler(Buttons button, ButtonStates state);

    public interface IInputController : IController, IFramesAcceptor
    {
        event ButtonPressedEventHandler ButtonPressed;
    }
}
