#region BlueElements - a collection of useful tools, database and controls
// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
namespace BlueBasics {
    public static partial class Extensions {
        public static Color FromHSB(this float hue, float satuation, float brightness, byte alpha) {
            hue /= 360;
            double r, g, b;
            if (Math.Abs(brightness) < 0.001) { return Color.FromArgb(alpha, 0, 0, 0); }
            if (Math.Abs(satuation) < 0.001) {
                r = brightness;
                g = brightness;
                b = brightness;
            } else {
                var temp2 = brightness <= 0.5F ? brightness * (1.0 + satuation) : brightness + satuation - (brightness * satuation);
                var temp1 = (2.0 * brightness) - temp2;
                double[] t3 = { hue + (1.0 / 3.0), hue, hue - (1.0 / 3.0) };
                double[] clr = { 0, 0, 0 };
                for (var i = 0; i <= 2; i++) {
                    if (t3[i] < 0) { t3[i] += 1.0; }
                    if (t3[i] > 1) { t3[i] -= 1.0; }
                    clr[i] = 6.0 * t3[i] < 1.0 ? temp1 + ((temp2 - temp1) * t3[i] * 6.0)
                            : 2.0 * t3[i] < 1.0 ? temp2
                            : 3.0 * t3[i] < 2.0 ? temp1 + ((temp2 - temp1) * ((2.0 / 3.0) - t3[i]) * 6.0)
                            : temp1;
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
            return Color.FromArgb(alpha, (int)(255 * r), (int)(255 * g), (int)(255 * b));
        }
        public static Color SoftLightMix(this Color baseColor, Color blendColor, double opacity) => MixColor(Color.FromArgb((int)SoftLightMath(baseColor.R, blendColor.R),
(int)SoftLightMath(baseColor.G, blendColor.G),
(int)SoftLightMath(baseColor.B, blendColor.B)), baseColor, opacity);
        public static Color OverlayMix(this Color baseColor, Color blendColor, double opacity) => MixColor(Color.FromArgb((int)OverlayMath(baseColor.R, blendColor.R),
(int)OverlayMath(baseColor.G, blendColor.G),
(int)OverlayMath(baseColor.B, blendColor.B)), baseColor, opacity);
        #region  Blend Mode Mathematics
        private static double SoftLightMath(int @base, int blend) {
            var dbase = @base / 255.0;
            var dblend = blend / 255.0;
            return dblend < 0.5
                ? ((2 * dbase * dblend) + (Math.Pow(dbase, 2) * (1 - (2 * dblend)))) * 255
                : ((Math.Sqrt(dbase) * ((2 * dblend) - 1)) + (2 * dbase * (1 - dblend))) * 255;
        }
        public static double OverlayMath(this int @base, int blend) {
            var dbase = @base / 255.0;
            var dblend = blend / 255.0;
            return dbase < 0.5 ? 2 * dbase * dblend * 255 : (1 - (2 * (1 - dbase) * (1 - dblend))) * 255;
        }
        #endregion
        public static Color MixColor(this Color color1, Color color2, double color1Prozent) {
            if (color1Prozent > 1) { color1Prozent = 1; }
            if (color1Prozent < 0) { color1Prozent = 0; }
            var Color2Prozent = 1 - color1Prozent;
            return Color.FromArgb((int)((color1.R * color1Prozent) + (color2.R * Color2Prozent)),
                                  (int)((color1.G * color1Prozent) + (color2.G * Color2Prozent)),
                                  (int)((color1.B * color1Prozent) + (color2.B * Color2Prozent)));
        }
        public static bool IsNearWhite(this Color col, double minBrightness) => col.ToArgb() == -1 || col.A == 0 || col.GetBrightness() >= minBrightness;
        public static bool IsNearBlack(this Color color, double maxBrightness) => color.A != 0 && (color.ToArgb() == 0 || color.GetBrightness() <= maxBrightness);
        //public static Color ToSepia(this Color color) {
        //    var r = (int)Math.Min(255, color.R * 0.393 + color.G * 0.769 + color.B * 0.189);
        //    var g = (int)Math.Min(255, color.R * 0.349 + color.G * 0.686 + color.B * 0.168);
        //    var b = (int)Math.Min(255, color.R * 0.272 + color.G * 0.534 + color.B * 0.131);
        //    return Color.FromArgb(255, r, g, b);
        //}
        public static string ToHTMLCode(this Color color) {
            var r = Convert.ToString(color.R, 16);
            if (r.Length < 2) { r = "0" + r; }
            var g = Convert.ToString(color.G, 16);
            if (g.Length < 2) { g = "0" + g; }
            var b = Convert.ToString(color.B, 16);
            if (b.Length < 2) { b = "0" + b; }
            var a = string.Empty;
            if (color.A != 255) {
                a = Convert.ToString(color.A, 16);
                if (a.Length < 2) { a = "0" + a; }
            }
            return a + r + g + b;
        }
        public static Color FromHTMLCode(this string color) {
            if (!color.IsHTMLColorCode()) {
                Develop.DebugPrint("Farbcode ungültig: " + color);
                return Color.Magenta;
            }
            var a = 255;
            var PL = 0;
            if (color.Length > 6) {
                a = int.Parse(color.Substring(0, 2), NumberStyles.HexNumber);
                PL = 2;
            }
            var r = int.Parse(color.Substring(PL, 2), NumberStyles.HexNumber);
            var g = int.Parse(color.Substring(PL + 2, 2), NumberStyles.HexNumber);
            var b = int.Parse(color.Substring(PL + 4, 2), NumberStyles.HexNumber);
            return Color.FromArgb(a, r, g, b);
        }
        //public static bool IsMagenta(this Color col) {
        //    return (col.ToArgb() == -65281);
        //}
        public static bool IsMagentaOrTransparent(this Color col) => col.ToArgb() == -65281 || col.A == 0;
        //public static bool IsTransparent(this Color col) {
        //    return (col.A == 0);
        //}
        //public static string ColorName(this Color col) {
        //    var c = col.ToHTMLCode().ToLower();
        //    switch (c) {
        //        case "0000ff":
        //            return "Blue";
        //        case "ff00ff":
        //            return "Magenta";
        //        case "ffffff":
        //            return "White";
        //        case "000000":
        //            return "Black";
        //        case "d394b4":
        //            return "Pale Pink";
        //        case "ffaed7":
        //            return "Light Pink";
        //        case "ff69b4":
        //            return "Hot Pink";
        //        case "f5f4ea":
        //            return "Disabled XP-Button";
        //        case "0095dd":
        //            return "Glossy Cyan Button Color";
        //        case "919b9c":
        //            return "XP Frame Border Grey";
        //        case "f8dfb1":
        //            return "Glossy Orange Disabled";
        //        case "87b7cd":
        //            return "Glossy Cyan Disabled Button Color";
        //        case "2fbbff":
        //            return "Glossy Cyan Highlight Button Color";
        //        case "d7e7ee":
        //            return "Glossy Cyan Form Color";
        //        case "003c74":
        //            return "XP Button Border Dark Blue";
        //        case "7f9db9":
        //            return "XP Text Border Light Blue";
        //        case "c9c7ba":
        //            return "XP Disabled Border Grey";
        //        case "f7fafb":
        //            return "Glossy Cyan Light Form Color";
        //        // break; case Is = "96ff96" : Return "Unipaint Calender Green"
        //        // break; case Is = "c8ffc8" : Return "Unipaint Saturday Green"
        //        // break; case Is = "ff8080" : Return "Unipaint Today Red"
        //        case "d1ccc1":
        //            return "XP Pressed Button Dark Grey";
        //        case "e3e2da":
        //            return "XP Pressed Button Middle Grey";
        //        case "efeeea":
        //            return "XP Pressed Button Light Grey";
        //        case "ffb834":
        //            return "XP Mouseover Button Dark Orange";
        //        case "ffda8c":
        //            return "XP Mouseover Button Middle Orange";
        //        case "ffe696":
        //            return "XP Mouseover Button Light Orange";
        //        case "ffffee":
        //            return "XP Quickinfo Pale Yellow";
        //        case "ffffe1":
        //            return "XP Ballon ToolTip Pale Yellow";
        //        case "fdffff":
        //            return "XP Cyan Button Light";
        //        case "d2eafe":
        //            return "XP Cyan Button Middle";
        //        case "b9ddfb":
        //            return "XP Cyan Button Dark";
        //        case "97aee2":
        //            return "XP Cyan Button Border Light Blue";
        //        case "f7f7f4":
        //            return "XP Blue Button Disabled Light Grey";
        //        case "edede6":
        //            return "XP Blue Button Disabled Middle Grey";
        //        case "e6e6dd":
        //            return "XP Blue Button Disabled Dark Grey";
        //        case "f4f3ee":
        //            return "XP Blue Tabstrip Grey";
        //        case "f3f3ec":
        //            return "XP Slider Body Grey Darker";
        //        case "fefefb":
        //            return "XP Slider Body Grey Lighter";
        //        case "ece9d8":
        //            return "XP Form Grey";
        //        case "e6eefc":
        //            return "XP Blue Button Light";
        //        case "c1d3fb":
        //            return "XP Blue Button Middle";
        //        case "aec8f7":
        //            return "XP Blue Button Dark";
        //        case "bdbaa2":
        //            return "XP Disabled Toolbar Dark Button Grey";
        //        case "316ac5":
        //            return "XP Text Marker Dark Blue";
        //        case "f4f2e8":
        //            return "XP Button Middle Grey";
        //        case "dcd7cb":
        //            return "XP Button Dark Grey";
        //        case "eeede5":
        //            return "XP Slider Border Grey";
        //        case "e8e8df":
        //            return "XP Disabled Button Border Grey";
        //        case "a6a6a6":
        //            return "XP Disabled Text Grey";
        //        case "777777":
        //            return "XP Text Marker Disabled Grey";
        //        case "c2dbff":
        //            return "XP Focus Blue 1";
        //        case "8cb4f2":
        //            return "XP Focus Blue 2";
        //        case "404040":
        //            return "XP Focus Dot Line Grey";
        //        case "dbdbdb":
        //            return "Glossy Cyan Disabled Text";
        //        case "0046d5":
        //            return "Glossy Cyan Text";
        //        case "498dab":
        //            return "Glossy Cyan Form Border Color";
        //        case "00000000":
        //            return "Transparent";
        //        case "eaeaea":
        //            return "Win 10 Button Grey";
        //        case "b6b6b6":
        //            return "Win 10 Button Border Grey";
        //        case "efefef":
        //            return "Win 10 Disabled Button Grey";
        //        case "d8d8d8":
        //            return "Win 10 Disabled Button Border Grey";
        //        case "9d9d9d":
        //            return "Win 10 Disabled Text Grey";
        //        case "f0f0f0":
        //            return "Win 10 Form Grey";
        //        case "acacac":
        //            return "Win 10 Border Frame Grey";
        //        case "bababa":
        //            return "Win 10 Border Slider Grey";
        //        case "dddddd":
        //            return "Win 10 Background Slider Grey";
        //        default:
        //            return c;
        //    }
        //}
        public static Color SetAlpha(this Color color, byte newAlpha) => Color.FromArgb(newAlpha, color.R, color.G, color.B);
        public static Color ToGrey(this Color color) {
            var W = (int)Math.Min(255, ((color.R * 77) + (color.G * 150) + (color.B * 28)) / 255.0);
            return Color.FromArgb(color.A, W, W, W);
        }
        public static Color Brighten(this Color color, double value) => FromHSB(color.GetHue(), color.GetSaturation(), (float)(color.GetBrightness() * (1 + value)), color.A);
        public static Color Darken(this Color color, double value) => FromHSB(color.GetHue(), color.GetSaturation(), (float)(color.GetBrightness() * (1 - value)), color.A);
        public static Color ClosestHueColor(this Color target, List<Color> colors) {
            // https://stackoverflow.com/questions/27374550/how-to-compare-color-object-and-get-closest-color-in-an-color/27375621
            var hue1 = target.GetHue();
            var diffs = colors.Select(n => getHueDistance(n.GetHue(), hue1));
            var diffMin = diffs.Min(n => n);
            return colors[diffs.ToList().FindIndex(n => n == diffMin)];
        }
        public static Color ClosestRGBColor(this Color target, List<Color> colors) {
            // https://stackoverflow.com/questions/27374550/how-to-compare-color-object-and-get-closest-color-in-an-color/27375621
            var colorDiffs = colors.Select(n => ColorDiff(n, target)).Min(n => n);
            return colors[colors.FindIndex(n => ColorDiff(n, target) == colorDiffs)];
        }
        // weighed distance using hue, saturation and brightness
        public static Color ClosestHSVColor(this Color target, List<Color> colors, float factorSat, float factorBri) {
            // https://stackoverflow.com/questions/27374550/how-to-compare-color-object-and-get-closest-color-in-an-color/27375621
            var hue1 = target.GetHue();
            var num1 = ColorNum(target, factorSat, factorBri);
            var diffs = colors.Select(n => Math.Abs(ColorNum(n, factorSat, factorBri) - num1) + getHueDistance(n.GetHue(), hue1));
            var diffMin = diffs.Min(x => x);
            return colors[diffs.ToList().FindIndex(n => n == diffMin)];
        }
        // color brightness as perceived:
        public static float getBrightness(this Color c) => ((c.R * 0.299f) + (c.G * 0.587f) + (c.B * 0.114f)) / 256f;
        // distance between two hues:
        public static float getHueDistance(this float hue1, float hue2) {
            var d = Math.Abs(hue1 - hue2);
            return d > 180 ? 360 - d : d;
        }
        // weighed only by saturation and brightness (from my trackbars)
        public static float ColorNum(this Color c, float factorSat, float factorBri) => (c.GetSaturation() * factorSat) + (getBrightness(c) * factorBri);
        // distance in RGB space
        public static int ColorDiff(this Color c1, Color c2) => (int)Math.Sqrt(((c1.R - c2.R) * (c1.R - c2.R))
+ ((c1.G - c2.G) * (c1.G - c2.G))
+ ((c1.B - c2.B) * (c1.B - c2.B)));
    }
}