using Interfaces.SerialTransportProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public enum TextAlingModes
    {
        None,
        Left,
        Right,
        Center
    }

    public interface IMiniDisplayController : IController, IFrameProvider
    {
        void Update();
        void Fill();
        void Cls();
        void Print(int x, int y, string text, TextAlingModes align = TextAlingModes.None);
    }
}
