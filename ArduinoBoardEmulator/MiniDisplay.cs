using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Interfaces.SerialTransportProtocol;
using System.Text.RegularExpressions;
using Interfaces.MiniDisplay;
using Implementation.MiniDisplay;

namespace ArduinoBoardEmulator
{
    internal class FontDescriptor
    {
        public Interfaces.MiniDisplay.Fonts Name;
        public Size Size;
        public Font EmulationFont;
    }

    internal class MiniDisplay
    {
        public event Action<Bitmap> Updated;

        private Bitmap b;
        private Graphics g;

        private FontDescriptor font;

        public MiniDisplay(Size displaySize)
        {
            b = new Bitmap(displaySize.Width, displaySize.Height);
            g = Graphics.FromImage(b);
            Cls();
        }

        public void ProcessFrame(STPFrame frame)
        {
            var command = (OLEDCommands)frame.Data[0];

            switch (command)
            {
                case OLEDCommands.OLED_COMMAND_CLS:
                    Cls();
                    break;

                case OLEDCommands.OLED_COMMAND_UPDATE:
                    Update();
                    break;

                case OLEDCommands.OLED_COMMAND_FONT:
                    var fontName = (Interfaces.MiniDisplay.Fonts)frame.Data[1];
                    SetFont(fontName);
                    break;

                case OLEDCommands.OLED_COMMAND_PRINT:
                    {
                        var align = (OLEDTextAlignModes)frame.Data[1];
                        var x = frame.Data[2];
                        var y = frame.Data[3];
                        var textLen = frame.Data.Length - 4;
                        var text = Encoding.ASCII.GetString(frame.Data, 4, textLen);
                        Print(align, x, y, text);
                    }
                    break;

                case OLEDCommands.OLED_COMMAND_FILL:
                    Fill();
                    break;

                case OLEDCommands.OLED_COMMAND_DRAW_LINE:
                    {
                        var x1 = frame.Data[1];
                        var y1 = frame.Data[2];
                        var x2 = frame.Data[3];
                        var y2 = frame.Data[4];
                        DrawLine(x1, y1, x2, y2);
                    }
                    break;

                case OLEDCommands.OLED_COMMAND_CLR_LINE:
                    {
                        var x1 = frame.Data[1];
                        var y1 = frame.Data[2];
                        var x2 = frame.Data[3];
                        var y2 = frame.Data[4];
                        ClearLine(x1, y1, x2, y2);
                    }
                    break;

                case OLEDCommands.OLED_COMMAND_PIXEL:
                    {
                        var set = frame.Data[1] != 0;
                        var x = frame.Data[2];
                        var y = frame.Data[3];
                        Pixel(set, x, y);
                    }
                    break;

                case OLEDCommands.OLED_COMMAND_DRAW_RECT:
                    {
                        var x1 = frame.Data[1];
                        var y1 = frame.Data[2];
                        var x2 = frame.Data[3];
                        var y2 = frame.Data[4];
                        DrawRect(x1, y1, x2, y2);
                    }
                    break;

                case OLEDCommands.OLED_COMMAND_CLR_RECT:
                    {
                        var x1 = frame.Data[1];
                        var y1 = frame.Data[2];
                        var x2 = frame.Data[3];
                        var y2 = frame.Data[4];
                        ClearRect(x1, y1, x2, y2);
                    }
                    break;

                case OLEDCommands.OLED_COMMAND_DRAW_ROUND_RECT:
                    {
                        var x1 = frame.Data[1];
                        var y1 = frame.Data[2];
                        var x2 = frame.Data[3];
                        var y2 = frame.Data[4];
                        DrawRoundRect(x1, y1, x2, y2);
                    }
                    break;

                case OLEDCommands.OLED_COMMAND_CLR_ROUND_RECT:
                    {
                        var x1 = frame.Data[1];
                        var y1 = frame.Data[2];
                        var x2 = frame.Data[3];
                        var y2 = frame.Data[4];
                        ClearRoundRect(x1, y1, x2, y2);
                    }
                    break;

                case OLEDCommands.OLED_COMMAND_DRAW_CIRCLE:
                    {
                        var x = frame.Data[1];
                        var y = frame.Data[2];
                        var rad = frame.Data[3];
                        DrawCircle(x, y, rad);
                    }
                    break;

                case OLEDCommands.OLED_COMMAND_CLR_CIRCLE:
                    {
                        var x = frame.Data[1];
                        var y = frame.Data[2];
                        var rad = frame.Data[3];
                        ClearCircle(x, y, rad);
                    }
                    break;

                case OLEDCommands.OLED_COMMAND_BRIGHTNESS:
                    break;

                case OLEDCommands.OLED_COMMAND_INVERT:
                    Invert();
                    break;

                case OLEDCommands.OLED_COMMAND_INVERT_PIXEL:
                    {
                        var x = frame.Data[1];
                        var y = frame.Data[2];
                        InvertPixel(x, y);
                    }
                    break;

                default:
                    throw new NotImplementedException(command.ToString());
                    break;
            }
        }

        private void InvertPixel(byte x, byte y)
        {
            if (b.GetPixel(x, y).GetBrightness() < 0.5)
            {
                b.SetPixel(x, y, Color.White);
            }
            else
            {
                b.SetPixel(x, y, Color.Black);
            }
        }

        private void Invert()
        {
            for(int y=0; y<b.Height; ++y)
                for(int x=0; x<b.Width; ++x)
                {
                    if (b.GetPixel(x, y).GetBrightness() < 0.5)
                    {
                        b.SetPixel(x, y, Color.White);
                    }
                    else
                    {
                        b.SetPixel(x, y, Color.Black);
                    }
                }
        }

        private void ClearCircle(byte x, byte y, byte rad)
        {
            g.DrawEllipse(Pens.Black, new Rectangle(x - rad, y - rad, rad * 2, rad * 2));
        }

        private void DrawCircle(byte x, byte y, byte rad)
        {
            g.DrawEllipse(Pens.White, new Rectangle(x - rad, y - rad, rad * 2, rad * 2));
        }

        private void ClearRoundRect(byte x1, byte y1, byte x2, byte y2)
        {
            g.DrawRectangle(Pens.Black, Rectangle.FromLTRB(x1, y1, x2, y2));
        }

        private void DrawRoundRect(byte x1, byte y1, byte x2, byte y2)
        {
            g.DrawRectangle(Pens.White, Rectangle.FromLTRB(x1, y1, x2, y2));
        }

        private void ClearRect(byte x1, byte y1, byte x2, byte y2)
        {
            g.DrawRectangle(Pens.Black, Rectangle.FromLTRB(x1, y1, x2, y2));
        }

        private void DrawRect(byte x1, byte y1, byte x2, byte y2)
        {
            g.DrawRectangle(Pens.White, Rectangle.FromLTRB(x1, y1, x2, y2));
        }

        private void Pixel(bool set, byte x, byte y)
        {
            b.SetPixel(x, y, set ? System.Drawing.Color.White : System.Drawing.Color.Black);
        }

        private void ClearLine(byte x1, byte y1, byte x2, byte y2)
        {
            g.DrawLine(Pens.Black, x1, y1, x2, y2);
        }

        private void DrawLine(byte x1, byte y1, byte x2, byte y2)
        {
            g.DrawLine(Pens.White, x1, y1, x2, y2);
        }

        private void Fill()
        {
            g.FillRectangle(Brushes.White, 0, 0, b.Width, b.Height);
        }

        private void Print(Implementation.MiniDisplay.OLEDTextAlignModes align, byte x, byte y, string text)
        {
            if (font == null)
                throw new InvalidOperationException("Font not providen");

            if (font.Name == Interfaces.MiniDisplay.Fonts.BigNumbers || font.Name == Interfaces.MiniDisplay.Fonts.MediumNumbers)
            {
                Regex isOnlyNumbersRegex = new Regex("^[0-9-. ]+$");
                if (!isOnlyNumbersRegex.IsMatch(text))
                {
                    throw new Exception("Font mismatch!");
                }
            }

            var textBounds = new Rectangle(x, y, font.Size.Width * text.Length, font.Size.Height);

            switch (align)
            {
                case OLEDTextAlignModes.OLED_TEXT_X_ALIGN_MODE_CENTER:
                    textBounds.X = ((b.Width - textBounds.Width) / 2);
                    break;

                case OLEDTextAlignModes.OLED_TEXT_X_ALIGN_MODE_RIGHT:
                    textBounds.X = b.Width - textBounds.Width;
                    break;
            }

            g.DrawRectangle(Pens.DimGray, textBounds);

            var format = new StringFormat() { Alignment = StringAlignment.Center };

            for (int i=0; i<text.Length; ++i)
            {
                var charRect = new Rectangle((int)textBounds.X + font.Size.Width * i, y, font.Size.Width, font.Size.Height);
                //g.DrawRectangle(Pens.DimGray, charRect);
                g.DrawString(text[i].ToString(), font.EmulationFont, Brushes.White, charRect, format);
            }
        }

        private void SetFont(Interfaces.MiniDisplay.Fonts fontName)
        {
            switch (fontName)
            {
                case Interfaces.MiniDisplay.Fonts.Small:
                    font = new FontDescriptor { Name = Interfaces.MiniDisplay.Fonts.Small, Size = new Size(6, 8), EmulationFont = new Font("Arial", 5f) };
                    break;

                case Interfaces.MiniDisplay.Fonts.MediumNumbers:
                    font = new FontDescriptor { Name = Interfaces.MiniDisplay.Fonts.MediumNumbers, Size = new Size(12, 16), EmulationFont = new Font("Arial", 10f) };
                    break;

                case Interfaces.MiniDisplay.Fonts.BigNumbers:
                    font = new FontDescriptor { Name = Interfaces.MiniDisplay.Fonts.BigNumbers, Size = new Size(14, 24), EmulationFont = new Font("Arial", 18f) };
                    break;
            }
        }

        private void Cls()
        {
            g.FillRectangle(Brushes.Black, 0, 0, b.Width, b.Height);
        }

        private void Update()
        {
            var handler = Updated;
            if (handler != null)
                handler(b);
        }
    }
}
