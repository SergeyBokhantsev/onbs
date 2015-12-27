using Interfaces.SerialTransportProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Interfaces.MiniDisplay
{
    public enum TextAlingModes
    {
        None,
        Left,
        Right,
        Center
    }

    public enum Fonts
    {
        Small = 0,
        MediumNumbers = 1,
        BigNumbers = 2
    }

    public interface IMiniDisplayController : IController, IFrameProvider
    {
        void Update();
        void Fill();
        void Cls();
        void SetFont(Fonts font);
        void Print(byte x, byte y, string text, TextAlingModes align = TextAlingModes.None);
        void Invert();
        void SetPixel(byte x, byte y);
        void ClearPixel(byte x, byte y);
        void InvertPixel(byte x, byte y);
        void DrawLine(byte x1, byte y1, byte x2, byte y2);
        void ClearLine(byte x1, byte y1, byte x2, byte y2);
        void DrawRect(byte x1, byte y1, byte x2, byte y2);
        void ClearRect(byte x1, byte y1, byte x2, byte y2);
        void DrawRoundRect(byte x1, byte y1, byte x2, byte y2);
        void ClearRoundRect(byte x1, byte y1, byte x2, byte y2);
        void DrawCircle(byte x, byte y, byte radius);
        void ClearCircle(byte x, byte y, byte radius);
        void Brightness(byte level);
    }

    public class Demo
    {
        private readonly IMiniDisplayController mc;

        public Demo(IMiniDisplayController mc)
        {
            this.mc = mc;
        }

        public void RunDemo1()
        {
            Random r = new Random();

            ShowCaption("DEMO 1");

            ShowCaption("FILL");
            mc.Fill();
            mc.Update();
            Thread.Sleep(2000);

            ShowCaption("PRINT");
            mc.SetFont(Fonts.Small);
            mc.Print(0, 15, "Left", TextAlingModes.Left);
            mc.Update();
            Thread.Sleep(1000);

            mc.Print(0, 35, "Center", TextAlingModes.Center);
            mc.Update();
            Thread.Sleep(1000);

            mc.Print(0, 50, "Right", TextAlingModes.Right);
            mc.Update();
            Thread.Sleep(2000);

            ShowCaption("LINES");
            for (int i = 0; i< 100; ++i)
            {
                mc.DrawLine((byte)r.Next(128), (byte)r.Next(64), 
				            (byte)r.Next(128), (byte)r.Next(64));
                mc.Update();
				Thread.Sleep (5);
            }

            Thread.Sleep(1000);

            for (int i = 0; i < 100; ++i)
            {
                mc.ClearLine((byte)r.Next(128), (byte)r.Next(128), (byte)r.Next(128), (byte)r.Next(128));
                mc.Update();
            }

            Thread.Sleep(1000);

            ShowCaption("PIXEL");

			for (byte y=0; y<64; y++) {
				for (byte x=0; x<128; ++x) {
					mc.SetPixel (x, y);
					mc.Update ();
				}
			}

            for (int i = 0; i < 1000; ++i)
            {
                mc.SetPixel((byte)r.Next(128), (byte)r.Next(64));
                mc.Update();
				Thread.Sleep (5);
            }

            for (int i = 0; i < 1000; ++i)
            {
                mc.InvertPixel((byte)r.Next(128), (byte)r.Next(128));
                mc.Update();
            }

            for (int i = 0; i < 1000; ++i)
            {
                mc.ClearPixel((byte)r.Next(128), (byte)r.Next(128));
                mc.Update();
            }

            ShowCaption("OVER...");
        }

        private void ShowCaption(string message)
        {
            mc.Cls();
            mc.SetFont(Fonts.Small);
            mc.Print(0, 20, message, TextAlingModes.Center);
            mc.Update();
            Thread.Sleep(2000);
            mc.Cls();
        }
    }
}
