﻿using Interfaces;
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

        private readonly Dictionary<Buttons, Buttons> btnMap = new Dictionary<Buttons, Buttons>()
        {
            { Buttons.F1, Buttons.F1 },
            { Buttons.F2, Buttons.F1 },
            { Buttons.F3, Buttons.F1 },
            { Buttons.F4, Buttons.F1 },
            { Buttons.F5, Buttons.F1 },
            { Buttons.F6, Buttons.F1 },
            { Buttons.F7, Buttons.F1 },
            { Buttons.F8, Buttons.F1 },
            { Buttons.Accept, Buttons.Accept },
            { Buttons.Cancel, Buttons.Cancel },
        };

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
                    logger.LogIfDebug(this, "Button frame received");
                    var button = btnMap[(Buttons)frame.Data[0]];
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

        private void OnButtonPressed(Buttons button, ButtonStates state)
        {
            var handler = ButtonPressed;
            if (handler != null)
                handler(button, state);
        }
    }
}
