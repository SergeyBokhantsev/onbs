﻿using Interfaces;
using Interfaces.SerialTransportProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDisplayController
{
    internal enum OLEDCommands : byte
    {
        OLED_COMMAND_CLS = 0,
        OLED_COMMAND_FILL = 1,
        OLED_COMMAND_INVERT = 2,
        OLED_COMMAND_PIXEL = 3,
        OLED_COMMAND_INVERT_PIXEL = 4,
        OLED_COMMAND_PRINT = 5,
        OLED_COMMAND_FONT = 6,
        OLED_COMMAND_DRAW_LINE = 7,
        OLED_COMMAND_CLR_LINE = 8,
        OLED_COMMAND_DRAW_RECT = 9,
        OLED_COMMAND_CLR_RECT = 10,
        OLED_COMMAND_DRAW_ROUND_RECT = 11,
        OLED_COMMAND_CLR_ROUND_RECT = 12,
        OLED_COMMAND_DRAW_CIRCLE = 13,
        OLED_COMMAND_CLR_CIRCLE = 14,
        OLED_COMMAND_UPDATE = 15
    }

    internal enum OLEDTextAlignModes
    {
        OLED_TEXT_X_ALIGN_MODE_NONE = 0,
        OLED_TEXT_X_ALIGN_MODE_LEFT = 1,
        OLED_TEXT_X_ALIGN_MODE_CENTER = 2,
        OLED_TEXT_X_ALIGN_MODE_RIGHT = 3
    }

    public class MiniDisplayController : IMiniDisplayController
    {
        public event Action<STPFrame> FrameToSend;

        private readonly ILogger logger;
        private readonly byte[] oneByteData = new byte[1];

        public MiniDisplayController(ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");

            this.logger = logger;
        }

        public void Update()
        {
            oneByteData[0] = (byte)OLEDCommands.OLED_COMMAND_UPDATE;
            CreateAndSendFrame(oneByteData);
        }

        public void Fill()
        {
            oneByteData[0] = (byte)OLEDCommands.OLED_COMMAND_FILL;
            CreateAndSendFrame(oneByteData);
        }

        public void Cls()
        {
            oneByteData[0] = (byte)OLEDCommands.OLED_COMMAND_CLS;
            CreateAndSendFrame(oneByteData);
        }

        private void CreateAndSendFrame(byte[] data)
        {
            var handler = FrameToSend;
            if (handler != null)
            {
                var frame = new STPFrame(data, STPFrame.Types.MiniDisplay);
                handler(frame);
            }
        }
    }
}
