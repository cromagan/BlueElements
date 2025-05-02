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
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Text;

namespace BlueBasics;

public static class Converter {

    #region Methods

    public static Bitmap? Base64ToBitmap(string base64) {
        try {
            using MemoryStream memory = new(Convert.FromBase64String(base64));
            Bitmap oBitmap = new(memory);
            memory.Close();
            return oBitmap;
        } catch {
            return null;
        }
    }

    public static string BitmapToBase64(Bitmap? bMp, ImageFormat bFormat) {
        if (bMp == null) { return string.Empty; }
        if (bMp.PixelFormat != PixelFormat.Format32bppPArgb) { bMp = Bitmap_ChangePixelFormat(bMp); }
        if (bMp == null) { return string.Empty; }
        MemoryStream memory = new();
        bMp.Save(memory, bFormat);
        var base64 = Convert.ToBase64String(memory.ToArray());
        memory.Close();
        return base64;
    }

    /// <summary>
    /// L�st nie einen Fehler aus. Kann der Wert nicht geparsed werden, wird DateTime.UtcNow zur�ckgegeben.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static DateTime DateTimeParse(string s) {
        if (string.IsNullOrEmpty(s)) { return DateTime.UtcNow; }
        if (DateTimeTryParse(s, out var result)) { return result; }
        //Develop.DebugPrint(ErrorType.Warning, "Datum kann nicht geparsed werden: " + s);
        return DateTime.UtcNow;
    }

    public static bool DateTimeTryParse(string s, out DateTime result) {
        // https://docs.microsoft.com/de-de/dotnet/standard/base-types/standard-date-and-time-format-strings?view=netframework-4.8
        result = default;
        if (string.IsNullOrEmpty(s)) { return false; }

        s = s.TrimEnd(" Uhr");

        // Versuche, das Datum und die Uhrzeit mit den definierten Formaten zu parsen.
        return DateTime.TryParseExact(s.Trim(), Constants.DateTimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
    }

    /// <summary>
    /// L�st nie einen Fehler aus. Kann der Wert nicht geparsed werden, wird 0 zur�ckgegeben.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static double DoubleParse(string? s) => string.IsNullOrEmpty(s) ? 0 : DoubleTryParse(s, out var v) ? v : (double)0f;

    /// <summary>
    /// L�st nie einen Fehler aus. Kann der Wert nicht geparsed werden, wird 0 zur�ckgegeben.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static bool DoubleTryParse(string? s, out double result) {
        result = 0;
        if (s == null || string.IsNullOrEmpty(s)) { return false; }

        return double.TryParse(s, out result)
               || double.TryParse(s.Replace(",", "."), out result)
               || double.TryParse(s.Replace(".", ","), out result);
    }

    /// <summary>
    /// L�st nie einen Fehler aus. Kann der Wert nicht geparsed werden, wird 0 zur�ckgegeben.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static float FloatParse(string? s) => string.IsNullOrEmpty(s) ? 0 : FloatTryParse(s, out var v) ? v : 0f;

    /// <summary>
    /// L�st nie einen Fehler aus. Kann der Wert nicht geparsed werden, wird 0 zur�ckgegeben.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static bool FloatTryParse(string? s, out float result) {
        result = 0;
        if (s == null || string.IsNullOrEmpty(s)) { return false; }

        return float.TryParse(s, out result)
               || float.TryParse(s.Replace(",", "."), out result)
               || float.TryParse(s.Replace(".", ","), out result);
    }

    /// <summary>
    /// L�st nie einen Fehler aus. Kann der Wert nicht geparsed werden, wird 0 zur�ckgegeben.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static int IntParse(string? s) => int.TryParse(s, out var v) ? v : 0;

    public static int IntParse(int? s) => s == null ? 0 : (int)s;

    public static int IntParse(double? s) => s == null ? 0 : (int)Math.Round((double)s, 0, MidpointRounding.AwayFromZero);

    /// <summary>
    /// L�st nie einen Fehler aus. Kann der Wert nicht geparsed werden, wird 0 zur�ckgegeben.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static long LongParse(string? s) => long.TryParse(s, out var v) ? v : 0;

    public static float MmToPixel(float mM, int dPi) => mM * dPi / 25.4f;

    public static float PixelToMm(float pixel, int dPi) => pixel * 25.4f / dPi;

    public static string StringtoUtf8(this string s) => Encoding.Default.GetString(Encoding.UTF8.GetBytes(s));

    private static Bitmap? Bitmap_ChangePixelFormat(Image? oldBmp) {
        if (oldBmp == null) { return null; }
        Generic.CollectGarbage();
        return new Bitmap(oldBmp);
        // Return oldBmp.Clone(New Rectangle(0, 0, oldBmp.Width, oldBmp.Height), NewFormat)
    }

    #endregion
}