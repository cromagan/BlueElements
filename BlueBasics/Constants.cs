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

#nullable enable

using System;
using System.Text;

// http://www.carlosag.net/tools/codetranslator/
// http://converter.telerik.com/

namespace BlueBasics;

public static class Constants {

    #region Fields

    public const string Administrator = "#Administrator";
    public const string AllowedCharsVariableName = Char_az + Char_AZ + "_" + Char_Numerals;

    public const string Char_az = "abcdefghijklmnopqrstuvwxyz";

    public const string Char_AZ = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public const string Char_Buchstaben = "abcdefghijklmnopqrstuvwxyzäöüßáéíóúâêîôûàèìòùžñ";

    public const string Char_DateiSonderZeichen = "\\/:*?\"<>|\r\n";

    public const string Char_Numerals = "0123456789";

    public const string Char_PfadSonderZeichen = "*?\"<>|\r\n";

    public const string Everybody = "#Everybody";

    public const char FirstSortChar = '-';

    public const string Format_Date = "yyyy/MM/dd HH:mm:ss.fff";
    public const string Format_Date10 = "yyyy_MM_dd";
    public const string Format_Date2 = "yyyy/MM/dd HH:mm:ss";
    public const string Format_Date3 = "yyyy/MM/dd";
    public const string Format_Date4 = "yyyy-MM-dd_HH-mm-ss";
    public const string Format_Date5 = "dd.MM.yyyy HH:mm:ss";
    public const string Format_Date6 = "dd.MM.yyyy";
    public const string Format_Date7 = "dd.MM.yyyy HH:mm:ss.fff";
    public const string Format_Date9 = "yyyy-MM-dd HH:mm:ss.fff";
    public const string Format_Float1 = "0.#";
    public const string Format_Float10 = "0.##########";
    public const string Format_Float10_3 = "0000000000.###";
    public const string Format_Float2 = "0.##";
    public const string Format_Float5 = "0.#####";
    public const string Format_Float9 = "0.#########";
    public const string Format_Integer1 = "0";
    public const string Format_Integer10 = "0000000000";
    public const string Format_Integer2 = "00";
    public const string Format_Integer3 = "000";
    public const string Format_Integer4 = "0000";
    public const string Format_Integer5 = "00000";
    public const string Format_Integer7 = "0000000";
    public const char SecondSortChar = 'X';
    public static readonly string Char_NotFromClip = (char)3 + ((char)22).ToString() + (char)24 + "\n";

    // public static readonly string[] Umrechnungen = { "1000 μm = 1 mm", "10 mm = 1 cm", "10 cm = 1 dm", "10 dm = 1 m", "1000 m = 1 km", "1000 μg = 1 mg", "1000 mg = 1 g", "1000 g = 1 kg", "1000 kg = 1 t", "1 d = 24 h", "1 h = 60 min", "1 min = 60 s", "1000 ms = 1 s", "1000 μl = 1 ml", "10 ml = 1 cl", "10 cl = 1 dl", "10 dl = 1 l", "100 l = 1 hl", "1 kcal = 4,187 kJ", "1000 cal = 1 kcal", "1000 J = 1 kJ", "1 mph = 1,609344 km/h", "1 m/s = 3600 m/h", "1 m/s = 3,6 km/h", "1 € = 100 ct", "1 byte = 8 bit", "1 MB = 1024 byte", "1 GB = 1024 MB", "1 TB = 1024 GB" }
    public static readonly Random GlobalRnd = new();

    public static readonly Encoding Win1252 = Encoding.GetEncoding(1252);

    public static float DefaultTolerance = 0.0001f;
    public static double FineTolerance = 0.0000001d;
    public static float IntTolerance = 0.5f;

    #endregion
}