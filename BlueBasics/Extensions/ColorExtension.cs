#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
// https://github.com/cromagan/BlueElements
// 
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER  
// DEALINGS IN THE SOFTWARE. 
#endregion

using System;
using System.Drawing;
using System.Globalization;

namespace BlueBasics
{
    public static partial class Extensions
    {
        public static Color FromHSB(float Hue, float Satuation, float Brightness, byte Alpha)
        {
            Hue /= 360;
            double r, g, b;

            if (Math.Abs(Brightness) < 0.001) { return Color.FromArgb(Alpha, 0, 0, 0); }

            if (Math.Abs(Satuation) < 0.001)
            {
                r = Brightness;
                g = Brightness;
                b = Brightness;
            }
            else
            {
                double temp2;
                if (Brightness <= 0.5F)
                {
                    temp2 = Brightness * (1.0 + Satuation);
                }
                else
                {
                    temp2 = Brightness + Satuation - Brightness * Satuation;
                }
                var temp1 = 2.0 * Brightness - temp2;

                double[] t3 = { Hue + 1.0 / 3.0, Hue, Hue - 1.0 / 3.0 };
                double[] clr = { 0, 0, 0 };

                for (var i = 0 ; i <= 2 ; i++)
                {
                    if (t3[i] < 0) { t3[i] += 1.0; }
                    if (t3[i] > 1) { t3[i] -= 1.0; }
                    if (6.0 * t3[i] < 1.0)
                    {
                        clr[i] = temp1 + (temp2 - temp1) * t3[i] * 6.0;
                    }
                    else if (2.0 * t3[i] < 1.0)
                    {
                        clr[i] = temp2;
                    }
                    else if (3.0 * t3[i] < 2.0)
                    {
                        clr[i] = temp1 + (temp2 - temp1) * (2.0 / 3.0 - t3[i]) * 6.0;
                    }
                    else
                    {
                        clr[i] = temp1;
                    }
                }

                r = clr[0];
                g = clr[1];
                b = clr[2];
            }


            if (r > 1) { r = 1; }
            if (g > 1) { g = 1; }
            if (b > 1) { b = 1; }
            if (r < 0) { r = 0; }
            if (g < 0) { g = 0; }
            if (b < 0) { b = 0; }


            return Color.FromArgb(Alpha, (int)(255 * r), (int)(255 * g), (int)(255 * b));
        }


        public static Color SoftLightMix(Color BaseColor, Color BlendColor, double opacity)
        {
            return MixColor(Color.FromArgb((int)SoftLightMath(BaseColor.R, BlendColor.R),
                                           (int)SoftLightMath(BaseColor.G, BlendColor.G),
                                           (int)SoftLightMath(BaseColor.B, BlendColor.B)), BaseColor, opacity);
        }

        public static Color OverlayMix(Color BaseColor, Color BlendColor, double opacity)
        {
            return MixColor(Color.FromArgb((int)OverlayMath(BaseColor.R, BlendColor.R),
                                           (int)OverlayMath(BaseColor.G, BlendColor.G),
                                           (int)OverlayMath(BaseColor.B, BlendColor.B)), BaseColor, opacity);
        }

        #region  Blend Mode Mathematics 

        private static double SoftLightMath(int @base, int blend)
        {
            var dbase = @base / 255.0;
            var dblend = blend / 255.0;


            if (dblend < 0.5)
            {
                return (2 * dbase * dblend + Math.Pow(dbase, 2) * (1 - 2 * dblend)) * 255;
            }

            return (Math.Sqrt(dbase) * (2 * dblend - 1) + 2 * dbase * (1 - dblend)) * 255;
        }

        public static double OverlayMath(int @base, int blend)
        {
            var dbase = @base / 255.0;
            var dblend = blend / 255.0;

            if (dbase < 0.5)
            {
                return 2 * dbase * dblend * 255;
            }

            return (1 - 2 * (1 - dbase) * (1 - dblend)) * 255;
        }

        #endregion


        public static Color MixColor(Color Color1, Color Color2, double Color1Prozent)
        {

            if (Color1Prozent > 1) { Color1Prozent = 1; }
            if (Color1Prozent < 0) { Color1Prozent = 0; }
            var Color2Prozent = 1 - Color1Prozent;

            return Color.FromArgb((int)(Color1.R * Color1Prozent + Color2.R * Color2Prozent),
                                  (int)(Color1.G * Color1Prozent + Color2.G * Color2Prozent),
                                  (int)(Color1.B * Color1Prozent + Color2.B * Color2Prozent));
        }





        public static bool IsNearWhite(this Color Col, double MinBrightness)
        {
            if (Col.ToArgb() == -1) { return true; }
            if (Col.A == 0) { return true; }
            if (Col.GetBrightness() >= MinBrightness) { return true; }
            return false;
        }

        public static bool IsNearBlack(this Color color, double MaxBrightness)
        {
            if (color.A == 0) { return false; }
            if (color.ToArgb() == 0) { return true; }
            if (color.GetBrightness() <= MaxBrightness) { return true; }
            return false;
        }



        public static Color ToSepia(this Color color)
        {
            var r = (int)Math.Min(255, color.R * 0.393 + color.G * 0.769 + color.B * 0.189);
            var g = (int)Math.Min(255, color.R * 0.349 + color.G * 0.686 + color.B * 0.168);
            var b = (int)Math.Min(255, color.R * 0.272 + color.G * 0.534 + color.B * 0.131);

            return Color.FromArgb(255, r, g, b);

        }

        public static string ToHTMLCode(this Color color)
        {
            var r = Convert.ToString(color.R, 16);
            if (r.Length < 2) { r = "0" + r; }
            var g = Convert.ToString(color.G, 16);
            if (g.Length < 2) { g = "0" + g; }
            var b = Convert.ToString(color.B, 16);
            if (b.Length < 2) { b = "0" + b; }

            var a = "";
            if (color.A != 255)
            {
                a = Convert.ToString(color.A, 16);
                if (a.Length < 2) { a = "0" + a; }
            }

            return a + r + g + b;
        }

        public static Color FromHTMLCode(this string color)
        {
            if (!color.IsHTMLColorCode())
            {
                Develop.DebugPrint("Farbcode ung�ltig: " + color);
                return Color.Magenta;
            }

            var a = 255;
            var PL = 0;
            if (color.Length > 6)
            {
                a = int.Parse(color.Substring(0, 2), NumberStyles.HexNumber);
                PL = 2;
            }

            var r = int.Parse(color.Substring(PL, 2), NumberStyles.HexNumber);
            var g = int.Parse(color.Substring(PL + 2, 2), NumberStyles.HexNumber);
            var b = int.Parse(color.Substring(PL + 4, 2), NumberStyles.HexNumber);

            return Color.FromArgb(a, r, g, b);
        }

        public static bool IsMagenta(this Color Col)
        {
            if (Col.ToArgb() == -65281) { return true; }
            if (Col.A == 0) { return true; }
            return false;
        }

        public static bool IsTransparent(this Color Col)
        {
            return (Col.A == 0);
        }


        public static string ColorName(this Color Col)
        {
            var c = Col.ToHTMLCode().ToLower();

            switch (c)
            {
                case "0000ff":
                    return "Blue";
                case "ff00ff":
                    return "Magenta";

                case "ffffff":
                    return "White";
                case "000000":
                    return "Black";
                case "d394b4":
                    return "Pale Pink";
                case "ffaed7":
                    return "Light Pink";

                case "ff69b4":
                    return "Hot Pink";
                case "f5f4ea":
                    return "Disabled XP-Button";
                case "0095dd":
                    return "Glossy Cyan Button Color";
                case "919b9c":
                    return "XP Frame Border Grey";
                case "f8dfb1":
                    return "Glossy Orange Disabled";
                case "87b7cd":
                    return "Glossy Cyan Disabled Button Color";
                case "2fbbff":
                    return "Glossy Cyan Highlight Button Color";
                case "d7e7ee":
                    return "Glossy Cyan Form Color";
                case "003c74":
                    return "XP Button Border Dark Blue";
                case "7f9db9":
                    return "XP Text Border Light Blue";
                case "c9c7ba":
                    return "XP Disabled Border Grey";
                case "f7fafb":
                    return "Glossy Cyan Light Form Color";
                //break; case Is = "96ff96" : Return "Unipaint Calender Green"
                //break; case Is = "c8ffc8" : Return "Unipaint Saturday Green"
                //break; case Is = "ff8080" : Return "Unipaint Today Red"
                case "d1ccc1":
                    return "XP Pressed Button Dark Grey";
                case "e3e2da":
                    return "XP Pressed Button Middle Grey";
                case "efeeea":
                    return "XP Pressed Button Light Grey";
                case "ffb834":
                    return "XP Mouseover Button Dark Orange";
                case "ffda8c":
                    return "XP Mouseover Button Middle Orange";
                case "ffe696":
                    return "XP Mouseover Button Light Orange";
                case "ffffee":
                    return "XP Quickinfo Pale Yellow";
                case "ffffe1":
                    return "XP Ballon ToolTip Pale Yellow";
                case "fdffff":
                    return "XP Cyan Button Light";
                case "d2eafe":
                    return "XP Cyan Button Middle";
                case "b9ddfb":
                    return "XP Cyan Button Dark";
                case "97aee2":
                    return "XP Cyan Button Border Light Blue";
                case "f7f7f4":
                    return "XP Blue Button Disabled Light Grey";
                case "edede6":
                    return "XP Blue Button Disabled Middle Grey";
                case "e6e6dd":
                    return "XP Blue Button Disabled Dark Grey";
                case "f4f3ee":
                    return "XP Blue Tabstrip Grey";
                case "f3f3ec":
                    return "XP Slider Body Grey Darker";
                case "fefefb":
                    return "XP Slider Body Grey Lighter";
                case "ece9d8":
                    return "XP Form Grey";
                case "e6eefc":
                    return "XP Blue Button Light";
                case "c1d3fb":
                    return "XP Blue Button Middle";
                case "aec8f7":
                    return "XP Blue Button Dark";
                case "bdbaa2":
                    return "XP Disabled Toolbar Dark Button Grey";
                case "316ac5":
                    return "XP Text Marker Dark Blue";
                case "f4f2e8":
                    return "XP Button Middle Grey";
                case "dcd7cb":
                    return "XP Button Dark Grey";

                case "eeede5":
                    return "XP Slider Border Grey";
                case "e8e8df":
                    return "XP Disabled Button Border Grey";
                case "a6a6a6":
                    return "XP Disabled Text Grey";

                case "777777":
                    return "XP Text Marker Disabled Grey";
                case "c2dbff":
                    return "XP Focus Blue 1";
                case "8cb4f2":
                    return "XP Focus Blue 2";
                case "404040":
                    return "XP Focus Dot Line Grey";



                case "dbdbdb":
                    return "Glossy Cyan Disabled Text";
                case "0046d5":
                    return "Glossy Cyan Text";
                case "498dab":
                    return "Glossy Cyan Form Border Color";

                case "00000000":
                    return "Transparent";

                case "eaeaea":
                    return "Win 10 Button Grey";
                case "b6b6b6":
                    return "Win 10 Button Border Grey";
                case "efefef":
                    return "Win 10 Disabled Button Grey";
                case "d8d8d8":
                    return "Win 10 Disabled Button Border Grey";
                case "9d9d9d":
                    return "Win 10 Disabled Text Grey";

                case "f0f0f0":
                    return "Win 10 Form Grey";
                case "acacac":
                    return "Win 10 Border Frame Grey";
                case "bababa":
                    return "Win 10 Border Slider Grey";
                case "dddddd":
                    return "Win 10 Background Slider Grey";
                default:

                    return c;
            }

        }

        public static Color SetAlpha(this Color color, byte NewAlpha)
        {
            return Color.FromArgb(NewAlpha, color.R, color.G, color.B);
        }


        public static Color ToGrey(this Color color)
        {
            var W = (int)Math.Min(255, (color.R * 77 + color.G * 150 + color.B * 28) / 255.0);
            return Color.FromArgb(color.A, W, W, W);
        }

        public static Color Brighten(this Color color, double Value)
        {
            return FromHSB(color.GetHue(), color.GetSaturation(), (float)(color.GetBrightness() * (1 + Value)), color.A);
        }

        public static Color Darken(this Color color, double Value)
        {
            return FromHSB(color.GetHue(), color.GetSaturation(), (float)(color.GetBrightness() * (1 - Value)), color.A);
        }
    }
}