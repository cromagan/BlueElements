// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;

using System.Text;

namespace BlueBasics.ClassesStatic;

public static class Converter {

    #region Methods

    public static Bitmap? Base64ToBitmap(string base64) {
        try {
            using var memory = new System.IO.MemoryStream(Convert.FromBase64String(base64));
            var bmp = new Bitmap(memory);
            memory.Close();
            return bmp;
        } catch {
            return null;
        }
    }

    public static string BitmapToBase64(Bitmap? bMp, ImageFormat bFormat) {
        if (bMp == null) { return string.Empty; }
        if (bMp.PixelFormat != PixelFormat.Format32bppPArgb) { bMp = Bitmap_ChangePixelFormat(bMp); }
        if (bMp == null) { return string.Empty; }
        var memory = new System.IO.MemoryStream();
        bMp.Save(memory, bFormat);
        var base64 = Convert.ToBase64String(memory.ToArray());
        memory.Close();
        return base64;
    }

    public static Color ColorParse(string input) {
        if (string.IsNullOrEmpty(input)) { return Color.Transparent; }
        return ColorTryParse(input, out var color) ? color : Color.Transparent;
    }

    public static bool ColorTryParse(string input, out Color color) {
        color = Color.Transparent; // Standardwert bei Fehler

        if (string.IsNullOrWhiteSpace(input)) { return false; }

        input = input.Trim().ToLowerInvariant();

        if (input.StartsWith('#')) { input = input[1..]; }

        if (!(input.Length is 6 or 8 && input.ContainsOnlyChars(Constants.Char_Numerals + "abcdef"))) { return false; }

        try {
            switch (input.Length) {
                case 6: // RGB
                    {
                        var r = int.Parse(input.AsSpan(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                        var g = int.Parse(input.AsSpan(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                        var b = int.Parse(input.AsSpan(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                        color = Color.FromArgb(255, r, g, b);
                        return true;
                    }

                case 8: // ARGB
                    {
                        var a = int.Parse(input.AsSpan(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                        var r = int.Parse(input.AsSpan(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                        var g = int.Parse(input.AsSpan(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                        var b = int.Parse(input.AsSpan(6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                        color = Color.FromArgb(a, r, g, b);
                        return true;
                    }
            }
        } catch {
            return false;
        }
        // 1. Versuche als Integer zu parsen (ARGB)
        if (int.TryParse(input, out var intValue)) {
            color = Color.FromArgb(intValue);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Löst nie einen Fehler aus. Kann der Wert nicht geparsed werden, wird DateTime.UtcNow zurückgegeben.
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
    /// Löst nie einen Fehler aus. Kann der Wert nicht geparsed werden, wird 0 zurückgegeben.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static double DoubleParse(string? s) => string.IsNullOrEmpty(s) ? 0 : DoubleTryParse(s, out var v) ? v : 0f;

    /// <summary>
    /// Versucht einen String in ein Double zu parsen, unabhängig von lokalen Trennzeichen.
    /// </summary>
    public static bool DoubleTryParse(string? s, out double result) {
        result = 0;
        string? normalized = Normalize(s);
        if (normalized == null) { return false; }

        return double.TryParse(
            normalized,
            NumberStyles.Float | NumberStyles.AllowLeadingSign,
            CultureInfo.InvariantCulture,
            out result
        );
    }

    /// <summary>
    /// Löst nie einen Fehler aus. Kann der Wert nicht geparsed werden, wird 0 zurückgegeben.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static float FloatParse(string? s) => string.IsNullOrEmpty(s) ? 0 : FloatTryParse(s, out var v) ? v : 0f;

    /// <summary>
    /// Versucht einen String in ein Float zu parsen, nutzt die gleiche Logik wie DoubleTryParse.
    /// </summary>
    public static bool FloatTryParse(string? s, out float result) {
        result = 0;
        string? normalized = Normalize(s);
        if (normalized == null) { return false; }

        return float.TryParse(
            normalized,
            NumberStyles.Float | NumberStyles.AllowLeadingSign,
            CultureInfo.InvariantCulture,
            out result
        );
    }

    /// <summary>
    /// Löst nie einen Fehler aus. Kann der Wert nicht geparsed werden, wird 0 zurückgegeben.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static int IntParse(string? s) => int.TryParse(s, out var v) ? v : 0;

    public static int IntParse(int? s) => s ?? 0;

    public static int IntParse(double? s) => s == null ? 0 : (int)Math.Round((double)s, 0, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Löst nie einen Fehler aus. Kann der Wert nicht geparsed werden, wird 0 zurückgegeben.
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

    /// <summary>
    /// Die zentrale Logik: Bereinigt den String und bringt ihn in ein Format (Punkt als Dezimaltrenner),
    /// das von InvariantCulture verstanden wird.
    /// </summary>
    private static string? Normalize(string? s) {
        if (string.IsNullOrWhiteSpace(s)) { return null; }

        string input = s.Trim();

        // 1. Vorzeichen behandeln
        bool isNegative = input.StartsWith('-');
        if (isNegative || input.StartsWith('+')) {
            input = input[1..];
        }

        if (input.Length == 0) { return null; }

        // 2. Exponent (z.B. e+10) isolieren, damit er nicht durch die Trenner-Logik läuft
        string exponent = "";
        int expIdx = input.IndexOfAny(new[] { 'e', 'E' });
        if (expIdx >= 0) {
            exponent = input[expIdx..];
            input = input[..expIdx];
        }

        // 3. Trenner-Analyse (Punkt vs. Komma)
        int lastDot = input.LastIndexOf('.');
        int lastComma = input.LastIndexOf(',');

        // Wir definieren den LETZTEN Trenner als das Dezimalzeichen
        int decimalIdx = Math.Max(lastDot, lastComma);

        StringBuilder cleanNumber = new StringBuilder();
        if (isNegative)
            cleanNumber.Append('-');

        if (decimalIdx == -1) {
            // Keine Trenner vorhanden: Nur Ziffern erlaubt
            foreach (char c in input) {
                if (!char.IsDigit(c)) { return null; }
                cleanNumber.Append(c);
            }
        } else {
            // Trenner vorhanden: Vorkommateil säubern, Nachkommateil prüfen
            string integerPart = input[..decimalIdx];
            string fractionalPart = input[(decimalIdx + 1)..];

            // Vorkommateil: Ziffern behalten, Punkte/Kommas ignorieren (Tausendertrenner)
            foreach (char c in integerPart) {
                if (char.IsDigit(c))
                    cleanNumber.Append(c);
                else if (c == '.' || c == ',') { continue; } else { return null; }
            }

            // Dezimalpunkt für InvariantCulture setzen
            cleanNumber.Append('.');

            // Nachkommateil: NUR Ziffern erlaubt
            foreach (char c in fractionalPart) {
                if (!char.IsDigit(c)) { return null; }
                cleanNumber.Append(c);
            }
        }

        // Exponent wieder dran und fertig
        cleanNumber.Append(exponent);
        return cleanNumber.ToString();
    }

    #endregion
}