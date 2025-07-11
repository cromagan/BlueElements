// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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

#nullable enable

using System;
using System.Drawing;
using System.Linq;

namespace BlueBasics;

public static partial class Extensions {

    #region Methods

    public static Color Brighten(this Color color, double value) => FromHsb(color.GetHue(), color.GetSaturation(), (float)(color.GetBrightness() * (1 + value)), color.A);

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
            double[] t3 = [hue + (1.0 / 3.0), hue, hue - (1.0 / 3.0)];
            double[] clr = [0, 0, 0];
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

    public static bool IsMagentaOrTransparent(this Color col) => col.ToArgb() == -65281 || col.A == 0;

    //Used: Only BZL
    public static bool IsNearBlack(this Color color, double maxBrightness) => color.A != 0 && (color.ToArgb() == 0 || color.GetBrightness() <= maxBrightness);

    public static bool IsNearWhite(this Color col, double minBrightness) => col.ToArgb() == -1 || col.A == 0 || col.GetBrightness() >= minBrightness;

    public static Color MakeTransparent(this Color color, byte alpha) => Color.FromArgb(alpha, color.R, color.G, color.B);

    public static Color MixColor(this Color color1, Color color2, double color1Prozent) {
        if (color1Prozent > 1) { color1Prozent = 1; }
        if (color1Prozent < 0) { color1Prozent = 0; }
        var color2Prozent = 1 - color1Prozent;
        return Color.FromArgb((int)((color1.R * color1Prozent) + (color2.R * color2Prozent)),
            (int)((color1.G * color1Prozent) + (color2.G * color2Prozent)),
            (int)((color1.B * color1Prozent) + (color2.B * color2Prozent)));
    }

    public static string Name(this Color color) {
        if (color.IsKnownColor) {
            return color.ToKnownColor().ToString();
        }

        // F�r custom colors, versuche die n�chstliegende bekannte Farbe zu finden
        var knownColors = Enum.GetValues(typeof(KnownColor))
            .Cast<KnownColor>()
            .Where(k => k != KnownColor.Transparent)
            .Select(k => Color.FromKnownColor(k))
            .ToList();

        var closest = knownColors
            .OrderBy(c => Math.Abs(c.R - color.R) + Math.Abs(c.G - color.G) + Math.Abs(c.B - color.B))
            .First();

        return closest.ToKnownColor().ToString();
    }

    public static Color SetAlpha(this Color color, byte newAlpha) => Color.FromArgb(newAlpha, color.R, color.G, color.B);

    public static Color ToGrey(this Color color) {
        var w = (int)Math.Min(255, ((color.R * 77) + (color.G * 150) + (color.B * 28)) / 255.0);
        return Color.FromArgb(color.A, w, w, w);
    }

    public static string ToHtmlCode(this Color? color) {
        if(color is not { } c) { return string.Empty; }

        if (c.A < 255) {
            // Alpha, Red, Green, Blue
            return $"#{c.A:x2}{c.R:x2}{c.G:x2}{c.B:x2}";
        } else {
            // Nur Red, Green, Blue
            return $"#{c.R:x2}{c.G:x2}{c.B:x2}";
        }
    }

    public static string ToHtmlCode(this Color color) {
 
        if (color.A < 255) {
            // Alpha, Red, Green, Blue
            return $"#{color.A:x2}{color.R:x2}{color.G:x2}{color.B:x2}";
        } else {
            // Nur Red, Green, Blue
            return $"#{color.R:x2}{color.G:x2}{color.B:x2}";
        }
    }


    #endregion
}