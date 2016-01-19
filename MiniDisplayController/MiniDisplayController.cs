using Interfaces;
using Interfaces.MiniDisplay;
using Interfaces.SerialTransportProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

namespace Implementation.MiniDisplay
{
    public class MiniDisplayController : IMiniDisplayController, IDisposable
    {
        public event FrameToSendDelegate FrameToSend;

        private readonly ManualResetEventSlim framesWaitHandler = new ManualResetEventSlim(false);
        private readonly MiniDisplayGraphics graphics;

        private bool disposed;

        public IMiniDisplayGraphics Graphics
        {
            get
            {
                return graphics;
            }
        }

        public MiniDisplayController(ILogger logger)
        {
            graphics = new MiniDisplayGraphics(framesWaitHandler);

            var senderThread = new Thread(FrameSenderLoop);
            senderThread.Name = "MiniDisplay";
            senderThread.IsBackground = true;
            senderThread.Priority = ThreadPriority.BelowNormal;
            senderThread.Start();
        }

        private void FrameSenderLoop()
        {
            while (!disposed)
            {
                framesWaitHandler.Wait();

                while (true)
                {
                    STPFrame frame = null;

                    lock (graphics.Frames)
                    {
                        if (graphics.Frames.Any())
                        {
                            frame = graphics.Frames.Dequeue();
                        }
                        else
                        {
                            framesWaitHandler.Reset();
                            break;
                        }
                    }

                    if (disposed)
                        return;

                    if (frame.Data[0] == (byte)OLEDCommands.INTERNAL_SEND_DELAY)
                    {
                        Thread.Sleep((int)OLEDCommands.INTERNAL_SEND_DELAY);
                        continue;
                    }

                    var delayAfterSend = 0;

					switch((OLEDCommands)frame.Data [0])
					{
						case OLEDCommands.OLED_COMMAND_UPDATE:
						case OLEDCommands.OLED_COMMAND_INVERT:
                        case OLEDCommands.OLED_COMMAND_BRIGHTNESS:
							delayAfterSend = 100;
							break;
					}

                    OnFrameToSend(frame, delayAfterSend);
                }
            }
        }

        private void OnFrameToSend(STPFrame frame, int delayAfterSend)
        {
            var handler = FrameToSend;
            if (handler != null)
            {
                handler(frame, delayAfterSend);
            }
        }

        public void ResetQueue()
        {
            lock (graphics.Frames)
            {
                graphics.Frames.Clear();
            }
        }

        public async Task WaitQueueFlushes()
        {
            await Task.Run(() =>
                {
                    while (!disposed)
                    {
                        lock (graphics.Frames)
                        {
                            if (!graphics.Frames.Any())
                                return;
                        }

                        Thread.Sleep(500);
                    }
                });
        }

        public void Dispose()
        {
            disposed = true;
        }
    }

    public enum OLEDCommands : byte
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
        OLED_COMMAND_BRIGHTNESS = 16,

        /// <summary>
        /// Every delay frame is 100 ms
        /// </summary>
        INTERNAL_SEND_DELAY = 100
    }

    public enum OLEDTextAlignModes
    {
        OLED_TEXT_X_ALIGN_MODE_NONE = 0,
        OLED_TEXT_X_ALIGN_MODE_LEFT = 1,
        OLED_TEXT_X_ALIGN_MODE_CENTER = 2,
        OLED_TEXT_X_ALIGN_MODE_RIGHT = 3
    }

    public class MiniDisplayGraphics : IMiniDisplayGraphics
    {
        private readonly ManualResetEventSlim waitHandle;

        public Queue<STPFrame> Frames
        {
            get;
            private set;
        }

        public MiniDisplayGraphics(ManualResetEventSlim waitHandle)
        {
            Frames = new Queue<STPFrame>();
            this.waitHandle = waitHandle;
        }

        private void CreateFrame(byte[] data)
        {
            var frame = new STPFrame(data, STPFrame.Types.MiniDisplay);

            lock (Frames)
            {
                Frames.Enqueue(frame);
                waitHandle.Set();
            }
        }

        public void Update()
        {
            CreateFrame(new byte[1] { (byte)OLEDCommands.OLED_COMMAND_UPDATE });
        }

        public void Fill()
        {
            CreateFrame(new byte[1] { (byte)OLEDCommands.OLED_COMMAND_FILL });
        }

        public void Cls()
        {
            CreateFrame(new byte[1] { (byte)OLEDCommands.OLED_COMMAND_CLS });
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

            CreateFrame(data);
        }

        public void SetFont(Fonts font)
        {
            CreateFrame(new byte[2] { (byte)OLEDCommands.OLED_COMMAND_FONT, (byte)font });
        }

        public void Invert(bool mode)
        {
            CreateFrame(new byte[2] { (byte)OLEDCommands.OLED_COMMAND_INVERT, 
				mode ? (byte)1 : (byte)0 });
        }

        public void SetPixel(byte x, byte y)
        {
            CreateFrame(new byte[4] { (byte)OLEDCommands.OLED_COMMAND_PIXEL, (byte)1, x, y });
        }

        public void ClearPixel(byte x, byte y)
        {
            CreateFrame(new byte[4] { (byte)OLEDCommands.OLED_COMMAND_PIXEL, (byte)0, x, y });
        }

        public void InvertPixel(byte x, byte y)
        {
            CreateFrame(new byte[3] { (byte)OLEDCommands.OLED_COMMAND_INVERT_PIXEL, x, y });
        }

        public void DrawLine(byte x1, byte y1, byte x2, byte y2)
        {
            CreateFrame(new byte[5] { (byte)OLEDCommands.OLED_COMMAND_DRAW_LINE, x1, y1, x2, y2 });
        }

        public void ClearLine(byte x1, byte y1, byte x2, byte y2)
        {
            CreateFrame(new byte[5] { (byte)OLEDCommands.OLED_COMMAND_CLR_LINE, x1, y1, x2, y2 });
        }

        public void DrawRect(byte x1, byte y1, byte x2, byte y2)
        {
            CreateFrame(new byte[5] { (byte)OLEDCommands.OLED_COMMAND_DRAW_RECT, x1, y1, x2, y2 });
        }

        public void ClearRect(byte x1, byte y1, byte x2, byte y2)
        {
            CreateFrame(new byte[5] { (byte)OLEDCommands.OLED_COMMAND_CLR_RECT, x1, y1, x2, y2 });
        }

        public void DrawRoundRect(byte x1, byte y1, byte x2, byte y2)
        {
            CreateFrame(new byte[5] { (byte)OLEDCommands.OLED_COMMAND_DRAW_ROUND_RECT, x1, y1, x2, y2 });
        }

        public void ClearRoundRect(byte x1, byte y1, byte x2, byte y2)
        {
            CreateFrame(new byte[5] { (byte)OLEDCommands.OLED_COMMAND_CLR_ROUND_RECT, x1, y1, x2, y2 });
        }

        public void DrawCircle(byte x, byte y, byte radius)
        {
            CreateFrame(new byte[4] { (byte)OLEDCommands.OLED_COMMAND_DRAW_CIRCLE, x, y, radius });
        }

        public void ClearCircle(byte x, byte y, byte radius)
        {
            CreateFrame(new byte[4] { (byte)OLEDCommands.OLED_COMMAND_CLR_CIRCLE, x, y, radius });
        }

        public void Brightness(byte level)
        {
            CreateFrame(new byte[2] { (byte)OLEDCommands.OLED_COMMAND_BRIGHTNESS, level });
        }

        public void Delay(int ms)
        {
            int delayFrameCount = ms / 100;

            for (int i = 0; i < delayFrameCount; ++i)
                CreateFrame(new byte[1] { (byte)OLEDCommands.INTERNAL_SEND_DELAY });
        }
    }
}
