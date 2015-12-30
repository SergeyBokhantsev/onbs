using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Interfaces.MiniDisplay;

namespace UIModels.MiniDisplay
{

    public class Demo
    {
        private readonly IMiniDisplayGraphics mc;

        public Demo(IMiniDisplayController c)
        {
            mc = c.Graphics;
        }

        public void RunDemo1()
        {
            Random r = new Random();

            ShowCaption("DEMO 1");

            ShowCaption("FILL");
            mc.Fill();
            mc.Update();
            Thread.Sleep(2000);

            ShowCaption("INVERT");
            mc.Print(0, 20, "INVERT", TextAlingModes.Center);
            mc.Update();
            mc.Invert(true);
            mc.Update();
            Thread.Sleep(1000);

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
            for (int i = 0; i < 100; ++i)
            {
                mc.DrawLine((byte)r.Next(128), (byte)r.Next(64),
                            (byte)r.Next(128), (byte)r.Next(64));
                mc.Update();
                Thread.Sleep(5);
            }

            Thread.Sleep(1000);

            for (int i = 0; i < 100; ++i)
            {
                mc.ClearLine((byte)r.Next(128), (byte)r.Next(128), (byte)r.Next(128), (byte)r.Next(128));
                mc.Update();
            }

            Thread.Sleep(1000);

            ShowCaption("PIXEL");

            //for (byte y=0; y<64; y++) {
            //    for (byte x=0; x<128; ++x) {
            //        mc.SetPixel (x, y);
            //        mc.Update ();
            //    }
            //}

            for (int i = 0; i < 100; ++i)
            {
                mc.SetPixel((byte)r.Next(128), (byte)r.Next(64));
                mc.Update();
                Thread.Sleep(5);
            }

            //for (int i = 0; i < 100; ++i)
            //{
            //    mc.InvertPixel((byte)r.Next(128), (byte)r.Next(128));
            //    mc.Update();
            //}

            mc.Fill();
            mc.Update();

            for (int i = 0; i < 100; ++i)
            {
                mc.ClearPixel((byte)r.Next(128), (byte)r.Next(128));
                mc.Update();
            }

            ShowCaption("RECTANGLES");
            for (int i = 0; i < 13; ++i)
            {
                mc.DrawRect((byte)(i * 2), (byte)(i * 2), (byte)(128 - i * 2 - 1), (byte)(64 - i * 2 - 1));
                mc.Update();
                Thread.Sleep(200);
            }

            for (int i = 0; i < 13; ++i)
            {
                mc.ClearRect((byte)(i * 2), (byte)(i * 2), (byte)(128 - i * 2 - 1), (byte)(64 - i * 2 - 1));
                mc.Update();
                Thread.Sleep(200);
            }

            ShowCaption("CIRCLES");
            for (int i = 0; i < 13; ++i)
            {
                mc.DrawCircle((byte)64, (byte)32, (byte)(i * 2));
                mc.Update();
                Thread.Sleep(200);
            }

            for (int i = 0; i < 13; ++i)
            {
                mc.ClearCircle((byte)64, (byte)32, (byte)(i * 2));
                mc.Update();
                Thread.Sleep(200);
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
