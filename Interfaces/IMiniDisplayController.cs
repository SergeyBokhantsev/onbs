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
        IMiniDisplayGraphics Graphics { get; }
        void ResetQueue();
    }

    public interface IMiniDisplayGraphics
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

        //INTERNAL

        void Delay(int ms);
    }
}
