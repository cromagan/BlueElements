// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace BlueBasics;

public static partial class Extensions {

    #region Methods

    public static Color Brighten(this Color color, double value) => FromHsb(color.GetHue(), color.GetSaturation(), (float)(color.GetBrightness() * (1 + value)), color.A);

    /// <summary>
    ///  weighed distance using hue, saturation and brightness
    /// </summary>
    /// <param name="target"></param>
    /// <param name="colors"></param>
    /// <param name="factorSat"></param>
    /// <param name="factorBri"></param>
    /// <returns></returns>
    public static Color ClosestHsvColor(this Color target, List<Color> colors, float factorSat, float factorBri) {
        // https://stackoverflow.com/questions/27374550/how-to-compare-color-object-and-get-closest-color-in-an-color/27375621
        var hue1 = target.GetHue();
        var num1 = ColorNum(target, factorSat, factorBri);
        var diffs = colors.Select(n => Math.Abs(ColorNum(n, factorSat, factorBri) - num1) + GetHueDistance(n.GetHue(), hue1));
        var diffMin = diffs.Min(x => x);
        return colors[diffs.ToList().FindIndex(n => n == diffMin)];
    }

    public static Color ClosestHueColor(this Color target, List<Color> colors) {
        // https://stackoverflow.com/questions/27374550/how-to-compare-color-object-and-get-closest-color-in-an-color/27375621
        var hue1 = target.GetHue();
        var diffs = colors.Select(n => GetHueDistance(n.GetHue(), hue1));
        var diffMin = diffs.Min(n => n);
        return colors[diffs.ToList().FindIndex(n => n == diffMin)];
    }

    public static Color ClosestRgbColor(this Color target, List<Color> colors) {
        // https://stackoverflow.com/questions/27374550/how-to-compare-color-object-and-get-closest-color-in-an-color/27375621
        var colorDiffs = colors.Select(n => ColorDiff(n, target)).Min(n => n);
        return colors[colors.FindIndex(n => ColorDiff(n, target) == colorDiffs)];
    }

    /// <summary>
    /// distance in RGB space
    /// </summary>
    /// <param name="c1"></param>
    /// <param name="c2"></param>
    /// <returns></returns>
    public static int ColorDiff(this Color c1, Color c2) => (int)Math.Sqrt(((c1.R - c2.R) * (c1.R - c2.R))
                                                                           + ((c1.G - c2.G) * (c1.G - c2.G))
                                                                           + ((c1.B - c2.B) * (c1.B - c2.B)));

    /// <summary>
    /// weighed only by saturation and brightness (from my trackbars)
    /// </summary>
    /// <param name="c"></param>
    /// <param name="factorSat"></param>
    /// <param name="factorBri"></param>
    /// <returns></returns>
    public static float ColorNum(this Color c, float factorSat, float factorBri) => (c.GetSaturation() * factorSat) + (GetBrightness(c) * factorBri);

    public static Color Darken(this Color color, double value) => FromHsb(color.GetHue(), color.GetSaturation(), (float)(color.GetBrightness() * (1 - value)), color.A);

    public static Color FromHsb(this float hue, float satuation, float brightness, byte alpha) {
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

    public static Color FromHtmlCode(this string color) {
        if (!color.IsHtmlColorCode()) {
            Develop.DebugPrint("Farbcode ung�ltig: " + color);
            return Color.Magenta;
        }
        var a = 255;
        var pl = 0;
        if (color.Length > 6) {
            a = int.Parse(color.Substring(0, 2), NumberStyles.HexNumber);
            pl = 2;
        }
        var r = int.Parse(color.Substring(pl, 2), NumberStyles.HexNumber);
        var g = int.Parse(color.Substring(pl + 2, 2), NumberStyles.HexNumber);
        var b = int.Parse(color.Substring(pl + 4, 2), NumberStyles.HexNumber);
        return Color.FromArgb(a, r, g, b);
    }

    /// <summary>
    ///  color brightness as perceived:
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public static float GetBrightness(this Color c) => ((c.R * 0.299f) + (c.G * 0.587f) + (c.B * 0.114f)) / 256f;

    /// <summary>
    /// distance between two hues:
    /// </summary>
    /// <param name="hue1"></param>
    /// <param name="hue2"></param>
    /// <returns></returns>
    public static float GetHueDistance(this float hue1, float hue2) {
        var d = Math.Abs(hue1 - hue2);
        return d > 180 ? 360 - d : d;
    }

    public static bool IsMagentaOrTransparent(this Color col) => col.ToArgb() == -65281 || col.A == 0;

    public static bool IsNearBlack(this Color color, double maxBrightness) => color.A != 0 && (color.ToArgb() == 0 || color.GetBrightness() <= maxBrightness);

    public static bool IsNearWhite(this Color col, double minBrightness) => col.ToArgb() == -1 || col.A == 0 || col.GetBrightness() >= minBrightness;

    public static Color MixColor(this Color color1, Color color2, double color1Prozent) {
        if (color1Prozent > 1) { color1Prozent = 1; }
        if (color1Prozent < 0) { color1Prozent = 0; }
        var color2Prozent = 1 - color1Prozent;
        return Color.FromArgb((int)((color1.R * color1Prozent) + (color2.R * color2Prozent)),
            (int)((color1.G * color1Prozent) + (color2.G * color2Prozent)),
            (int)((color1.B * color1Prozent) + (color2.B * color2Prozent)));
    }

    public static double OverlayMath(this int @base, int blend) {
        var dbase = @base / 255.0;
        var dblend = blend / 255.0;
        return dbase < 0.5 ? 2 * dbase * dblend * 255 : (1 - (2 * (1 - dbase) * (1 - dblend))) * 255;
    }

    public static Color OverlayMix(this Color baseColor, Color blendColor, double opacity) => MixColor(Color.FromArgb((int)OverlayMath(baseColor.R, blendColor.R),
        (int)OverlayMath(baseColor.G, blendColor.G),
        (int)OverlayMath(baseColor.B, blendColor.B)), baseColor, opacity);

    public static Color SetAlpha(this Color color, byte newAlpha) => Color.FromArgb(newAlpha, color.R, color.G, color.B);

    public static Color SoftLightMix(this Color baseColor, Color blendColor, double opacity) => MixColor(Color.FromArgb((int)SoftLightMath(baseColor.R, blendColor.R),
        (int)SoftLightMath(baseColor.G, blendColor.G),
        (int)SoftLightMath(baseColor.B, blendColor.B)), baseColor, opacity);

    public static Color ToGrey(this Color color) {
        var w = (int)Math.Min(255, ((color.R * 77) + (color.G * 150) + (color.B * 28)) / 255.0);
        return Color.FromArgb(color.A, w, w, w);
    }

    public static string ToHtmlCode(this Color color) {
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

    private static double SoftLightMath(int @base, int blend) {
        var dbase = @base / 255.0;
        var dblend = blend / 255.0;
        return dblend < 0.5
            ? ((2 * dbase * dblend) + (Math.Pow(dbase, 2) * (1 - (2 * dblend)))) * 255
            : ((Math.Sqrt(dbase) * ((2 * dblend) - 1)) + (2 * dbase * (1 - dblend))) * 255;
    }

    #endregion
}