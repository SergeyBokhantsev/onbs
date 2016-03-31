using Interfaces;
using Interfaces.Input;
using Interfaces.SerialTransportProtocol;
using SerialTransportProtocol;
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
        private readonly STPCodec codec;

        private readonly IdleMeter iddleMeter = new IdleMeter();

        public STPFrame.Types FrameType
        {
            get
            {
                return STPFrame.Types.Button;
            }
        }

        public int IddleMinutes
        {
            get
            {
                return iddleMeter.IdleMinutes;
            }
        }

        public InputController(ILogger logger)
        {
            this.logger = logger;

            codec = new STPCodec(Encoding.ASCII.GetBytes("[<]"), Encoding.ASCII.GetBytes("[>]"), true);
        }

        public void AcceptFrames(IEnumerable<STPFrame> transportFrames)
        {
            var buttonFrames = codec.Decode(transportFrames);
            if (buttonFrames != null)
            {
                iddleMeter.Reset();

                foreach (var frame in buttonFrames)
                {
                    try
                    {
                        logger.LogIfDebug(this, "Button frame received");
                        var button = (Buttons)frame.Data[0];
                        var state = (ButtonStates)frame.Data[1];
                        logger.LogIfDebug(this, string.Format("Button parsed: {0}, {1}", button, state));
                        OnButtonPressed(button, state);
                    }
                    catch (Exception ex)
                    {
                        logger.Log(this, ex);
                    }
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
