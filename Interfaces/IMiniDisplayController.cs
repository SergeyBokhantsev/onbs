using Interfaces.SerialTransportProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        Medium = 1,
        BigNumbers = 2
    }

    public interface IMiniDisplayController : IController, IFrameProvider
    {
        void Update();
        void Fill();
        void Cls();
        void SetFont(Fonts font);
        void Print(int x, int y, string text, TextAlingModes align = TextAlingModes.None);
    }
}
