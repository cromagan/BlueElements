// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Text;
using BlueBasics.Enums;

#nullable enable

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
    /// Löst nie einen Fehler aus. Kann der Wert nicht geparsed werden, wird DateTime.UtcNow zurückgegeben.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static DateTime DateTimeParse(string s) {
        if (string.IsNullOrEmpty(s)) { return DateTime.UtcNow; }
        if (DateTimeTryParse(s, out var result)) { return result; }
        Develop.DebugPrint(FehlerArt.Warnung, "Datum kann nicht geparsed werden: " + s);
        return DateTime.UtcNow;
    }

    public static bool DateTimeTryParse(string? s, out DateTime result) {
        // https://docs.microsoft.com/de-de/dotnet/standard/base-types/standard-date-and-time-format-strings?view=netframework-4.8
        if (s == null || string.IsNullOrEmpty(s)) {
            result = default;
            return false;
        }

        s = s.TrimEnd(" Uhr");

        return DateTime.TryParseExact(s, Constants.Format_Date5, CultureInfo.InvariantCulture, DateTimeStyles.None, out result)
               || DateTime.TryParseExact(s, Constants.Format_Date6, CultureInfo.InvariantCulture, DateTimeStyles.None, out result)
               || DateTime.TryParseExact(s, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out result)
               || DateTime.TryParseExact(s, "dd.MM.yyyy H:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out result)
               || DateTime.TryParseExact(s, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out result)
               || DateTime.TryParseExact(s, Constants.Format_Date2, CultureInfo.InvariantCulture, DateTimeStyles.None, out result)
               || DateTime.TryParseExact(s, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out result)
               || DateTime.TryParseExact(s, "dd.MM.yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out result)
               || DateTime.TryParseExact(s, "d.M.yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out result)
               || DateTime.TryParseExact(s, "d.M.yy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out result)
               || DateTime.TryParseExact(s, Constants.Format_Date, CultureInfo.InvariantCulture, DateTimeStyles.None, out result)
               || DateTime.TryParseExact(s, Constants.Format_Date7, CultureInfo.InvariantCulture, DateTimeStyles.None, out result)
               || DateTime.TryParseExact(s, Constants.Format_Date9, CultureInfo.InvariantCulture, DateTimeStyles.None, out result)
               || DateTime.TryParseExact(s, Constants.Format_Date10, CultureInfo.InvariantCulture, DateTimeStyles.None, out result)
               || DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result)
               || DateTime.TryParseExact(s, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out result)
               || DateTime.TryParseExact(s, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out result)
               || DateTime.TryParseExact(s, Constants.Format_Date9, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
    }

    /// <summary>
    /// Löst nie einen Fehler aus. Kann der Wert nicht geparsed werden, wird 0 zurückgegeben.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static double DoubleParse(string? s) {
        if (string.IsNullOrEmpty(s)) { return 0; }

        if (DoubleTryParse(s, out var v)) { return v; }

        return 0f;
    }

    /// <summary>
    /// Löst nie einen Fehler aus. Kann der Wert nicht geparsed werden, wird 0 zurückgegeben.
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
        //if (double.TryParse(s, out var result)) { return result; }
        //if (double.TryParse(s.Replace(",", "."), out var result2)) { return result2; }
        //if (double.TryParse(s.Replace(".", ","), out var result3)) { return result3; }

        //return 0;
    }

    public static T EnsureNotNull<T>(T? value) where T : class {
        if (value == null) {
            Develop.DebugPrint(FehlerArt.Fehler, "Wert ist null!");
            throw new ArgumentNullException();
        }

        return value;
    }

    /// <summary>
    /// Löst nie einen Fehler aus. Kann der Wert nicht geparsed werden, wird 0 zurückgegeben.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static float FloatParse(string? s) {
        if (string.IsNullOrEmpty(s)) { return 0; }

        if (FloatTryParse(s, out var v)) { return v; }

        return 0f;
    }

    /// <summary>
    /// Löst nie einen Fehler aus. Kann der Wert nicht geparsed werden, wird 0 zurückgegeben.
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
    /// Löst nie einen Fehler aus. Kann der Wert nicht geparsed werden, wird 0 zurückgegeben.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static int IntParse(string? s) {
        if (string.IsNullOrEmpty(s)) { return 0; }

        if (IntTryParse(s, out var v)) { return v; }

        return 0;
    }

    /// <summary>
    /// Löst nie einen Fehler aus. Kann der Wert nicht geparsed werden, wird 0 zurückgegeben.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static bool IntTryParse(string? s, out int result) {
        result = 0;
        if (string.IsNullOrEmpty(s)) { return false; }

        return int.TryParse(s, out result);
    }

    /// <summary>
    /// Löst nie einen Fehler aus. Kann der Wert nicht geparsed werden, wird 0 zurückgegeben.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static long LongParse(string? s) {
        if (string.IsNullOrEmpty(s)) { return 0; }

        if (LongTryParse(s, out var v)) { return v; }

        return 0;
    }

    /// <summary>
    /// Löst nie einen Fehler aus. Kann der Wert nicht geparsed werden, wird 0 zurückgegeben.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static bool LongTryParse(string? s, out long result) {
        result = 0;
        if (string.IsNullOrEmpty(s)) { return false; }

        return long.TryParse(s, out result);
    }

    public static float MmToPixel(float mM, int dPi) => mM * dPi / 25.4f;

    public static float PixelToMm(float pixel, int dPi) => pixel / dPi * 25.4f;

    public static string StringtoUtf8(this string s) => Encoding.Default.GetString(Encoding.UTF8.GetBytes(s));

    private static Bitmap? Bitmap_ChangePixelFormat(Image? oldBmp) {
        if (oldBmp == null) { return null; }
        Generic.CollectGarbage();
        return new Bitmap(oldBmp);
        // Return oldBmp.Clone(New Rectangle(0, 0, oldBmp.Width, oldBmp.Height), NewFormat)
    }

    #endregion
}