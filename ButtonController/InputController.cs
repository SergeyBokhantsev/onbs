using Interfaces;
using Interfaces.Input;
using Interfaces.SerialTransportProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InputController
{
    public class InputController : IInputController
    {
        public event ButtonPressedEventHandler ButtonPressed;

        private readonly ILogger logger;

        public STPFrame.Types FrameType
        {
            get
            {
                return STPFrame.Types.Button;
            }
        }

        public InputController(ILogger logger)
        {
            this.logger = logger;
        }

        public void AcceptFrames(IEnumerable<STPFrame> frames)
        {
            foreach (var frame in frames)
            {
                try
                {
                    logger.Log("Button frame received", LogLevels.Debug);
                    var button = (Buttons)frame.Data[0];
                    var state = (ButtonStates)frame.Data[1];
                    logger.Log(string.Format("Button parsed: {0}, {1}", button, state), LogLevels.Debug);
                    OnButtonPressed(button, state);
                }
                catch (Exception ex)
                {
                    logger.Log(ex);
                }
            }
        }

        private void OnButtonPressed(Buttons button, ButtonStates state)
        {
            var handler = ButtonPressed;
            if (handler != null)
                handler(button, state);
        }
    }
}
