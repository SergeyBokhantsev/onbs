using Interfaces;
using Interfaces.MiniDisplay;
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
        OLED_COMMAND_UPDATE = 15,
        OLED_COMMAND_BRIGHTNESS = 16
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
        private readonly byte[] twoByteData = new byte[2];
        private readonly byte[] threeByteData = new byte[3];
        private readonly byte[] fourByteData = new byte[4];
        private readonly byte[] fiveByteData = new byte[5];

        public MiniDisplayController(ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");

            this.logger = logger;
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

        public void Update()
        {
            oneByteData[0] = (byte)OLEDCommands.OLED_COMMAND_UPDATE;
            CreateAndSendFrame(oneByteData);
			System.Threading.Thread.Sleep (25);
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

        public void Print(byte x, byte y, string text, TextAlingModes align = TextAlingModes.None)
        {
            var data = new byte[text.Length + 4];

            data[0] = (byte)OLEDCommands.OLED_COMMAND_PRINT;

            switch(align)
            {
                case TextAlingModes.None:
                    data[1] = (byte)OLEDTextAlignModes.OLED_TEXT_X_ALIGN_MODE_NONE;
                    break;
                case TextAlingModes.Left:
                    data[1] = (byte)OLEDTextAlignModes.OLED_TEXT_X_ALIGN_MODE_LEFT;
                    break;
                case TextAlingModes.Center:
                    data[1] = (byte)OLEDTextAlignModes.OLED_TEXT_X_ALIGN_MODE_CENTER;
                    break;
                case TextAlingModes.Right:
                    data[1] = (byte)OLEDTextAlignModes.OLED_TEXT_X_ALIGN_MODE_RIGHT;
                    break;
            }

            data[2] = x;
            data[3] = y;

            for (int i=0; i<text.Length; ++i)
            {
                data[4 + i] = (byte)text[i];
            }

			CreateAndSendFrame (data);
        }

        public void SetFont(Fonts font)
        {
            twoByteData[0] = (byte)OLEDCommands.OLED_COMMAND_FONT;
            twoByteData[1] = (byte)font;
            CreateAndSendFrame(twoByteData);
        }

        public void Invert()
        {
            oneByteData[0] = (byte)OLEDCommands.OLED_COMMAND_INVERT;
            CreateAndSendFrame(oneByteData);
        }

        public void SetPixel(byte x, byte y)
        {
            fourByteData[0] = (byte)OLEDCommands.OLED_COMMAND_PIXEL;
            fourByteData[1] = 1;
            fourByteData[2] = x;
            fourByteData[3] = y;
            CreateAndSendFrame(fourByteData);
        }

        public void ClearPixel(byte x, byte y)
        {
            fourByteData[0] = (byte)OLEDCommands.OLED_COMMAND_PIXEL;
            fourByteData[1] = 0;
            fourByteData[2] = x;
            fourByteData[3] = y;
            CreateAndSendFrame(fourByteData);
        }

        public void InvertPixel(byte x, byte y)
        {
            threeByteData[0] = (byte)OLEDCommands.OLED_COMMAND_INVERT_PIXEL;
            threeByteData[1] = x;
            threeByteData[2] = y;
            CreateAndSendFrame(threeByteData);
        }

        public void DrawLine(byte x1, byte y1, byte x2, byte y2)
        {
            fiveByteData[0] = (byte)OLEDCommands.OLED_COMMAND_DRAW_LINE;
            fiveByteData[1] = x1;
            fiveByteData[2] = y1;
            fiveByteData[3] = x2;
            fiveByteData[4] = y2;
            CreateAndSendFrame(fiveByteData);
        }

        public void ClearLine(byte x1, byte y1, byte x2, byte y2)
        {
            fiveByteData[0] = (byte)OLEDCommands.OLED_COMMAND_CLR_LINE;
            fiveByteData[1] = x1;
            fiveByteData[2] = y1;
            fiveByteData[3] = x2;
            fiveByteData[4] = y2;
            CreateAndSendFrame(fiveByteData);
        }

        public void DrawRect(byte x1, byte y1, byte x2, byte y2)
        {
            fiveByteData[0] = (byte)OLEDCommands.OLED_COMMAND_DRAW_RECT;
            fiveByteData[1] = x1;
            fiveByteData[2] = y1;
            fiveByteData[3] = x2;
            fiveByteData[4] = y2;
            CreateAndSendFrame(fiveByteData);
        }

        public void ClearRect(byte x1, byte y1, byte x2, byte y2)
        {
            fiveByteData[0] = (byte)OLEDCommands.OLED_COMMAND_CLR_RECT;
            fiveByteData[1] = x1;
            fiveByteData[2] = y1;
            fiveByteData[3] = x2;
            fiveByteData[4] = y2;
            CreateAndSendFrame(fiveByteData);
        }

        public void DrawRoundRect(byte x1, byte y1, byte x2, byte y2)
        {
            fiveByteData[0] = (byte)OLEDCommands.OLED_COMMAND_DRAW_ROUND_RECT;
            fiveByteData[1] = x1;
            fiveByteData[2] = y1;
            fiveByteData[3] = x2;
            fiveByteData[4] = y2;
            CreateAndSendFrame(fiveByteData);
        }

        public void ClearRoundRect(byte x1, byte y1, byte x2, byte y2)
        {
            fiveByteData[0] = (byte)OLEDCommands.OLED_COMMAND_CLR_ROUND_RECT;
            fiveByteData[1] = x1;
            fiveByteData[2] = y1;
            fiveByteData[3] = x2;
            fiveByteData[4] = y2;
            CreateAndSendFrame(fiveByteData);
        }

        public void DrawCircle(byte x, byte y, byte radius)
        {
            fourByteData[0] = (byte)OLEDCommands.OLED_COMMAND_DRAW_CIRCLE;
            fourByteData[1] = x;
            fourByteData[2] = y;
            fourByteData[3] = radius;
            CreateAndSendFrame(fourByteData);
        }

        public void ClearCircle(byte x, byte y, byte radius)
        {
            fourByteData[0] = (byte)OLEDCommands.OLED_COMMAND_CLR_CIRCLE;
            fourByteData[1] = x;
            fourByteData[2] = y;
            fourByteData[3] = radius;
            CreateAndSendFrame(fourByteData);
        }

        public void Brightness(byte level)
        {
            twoByteData[0] = (byte)OLEDCommands.OLED_COMMAND_BRIGHTNESS;
            twoByteData[1] = level;
            CreateAndSendFrame(twoByteData);
        }
    }
}
