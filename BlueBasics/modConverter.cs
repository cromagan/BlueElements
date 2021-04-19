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

using BlueBasics.Enums;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Text;
using static BlueBasics.FileOperations;

namespace BlueBasics {
    public static class modConverter {
        #region  Konvertier-Routinen
        private static readonly string SerialNr2Path_LastSearch = string.Empty;
        private static readonly string SerialNr2Path_LastErgebnis = string.Empty;

        public static string ByteToBin8(byte b) {
            var x = Convert.ToString(b, 2);
            while (true) {
                if (x.Length == 8) { return x; }
                x = "0" + x;
            }
        }

        public static byte Bin8ToByte(string bIN8) {
            return Convert.ToByte(bIN8, 2);
        }

        public static Bitmap StringWIN1252ToBitmap(string tXT) {
            if (string.IsNullOrEmpty(tXT)) {
                return null;
            }

            var b = tXT.ToByteWIN1252();
            var BMP = ByteToBitmap(b);
            return BMP;
        }

        public static Bitmap StringUnicodeToBitmap(string unicodeTXT) {
            if (string.IsNullOrEmpty(unicodeTXT)) {
                return null;
            }

            var b = unicodeTXT.Unicode_ToByte();
            var BMP = ByteToBitmap(b);
            return BMP;
        }

        // public static string UTF8toString(string S) {

        // }

        // public static string StringtoUTF8(string S) {
        //    return Encoding.Defxault.GetString(Encoding.UTF8.GetBytes(S));
        // }

        public static string BitmapToBase64(Bitmap bMP, ImageFormat bFormat) {
            if (bMP == null) { return string.Empty; }

            if (bMP.PixelFormat != PixelFormat.Format32bppPArgb) { bMP = Bitmap_ChangePixelFormat(bMP); }

            string base64 = null;
            var memory = new MemoryStream();
            bMP.Save(memory, bFormat);
            base64 = Convert.ToBase64String(memory.ToArray());
            memory.Close();
            return base64;
        }

        public static Bitmap Base64ToBitmap(string base64) {
            try {
                using var memory = new MemoryStream(Convert.FromBase64String(base64));
                var oBitmap = new Bitmap(memory);
                memory.Close();
                return oBitmap;
            } catch {
                return null;
            }
        }

        public static byte[] FileToByte(string dateiname) {
            byte[] b = null;
            var obFi = new FileStream(dateiname, FileMode.Open, FileAccess.Read);

            var r = new BinaryReader(obFi);
            b = r.ReadBytes((int)new FileInfo(dateiname).Length);

            r.Close();
            r.Dispose();

            obFi.Close();
            obFi.Dispose();

            return b;
        }

        public static void ByteToFile(string dateiname, byte[] b) {
            if (FileExists(dateiname)) {
                Develop.DebugPrint("Datei soll überschrieben werden: " + dateiname);
                return;
            }

            // Stop
            var l = File.Create(dateiname);
            l.Write(b, 0, b.Length);
            l.Flush();
            l.Close();
            l.Dispose();
        }

        private static Bitmap Bitmap_ChangePixelFormat(Bitmap oldBmp) {
            modAllgemein.CollectGarbage();

            return new Bitmap(oldBmp);
            // Return oldBmp.Clone(New Rectangle(0, 0, oldBmp.Width, oldBmp.Height), NewFormat)
        }

        public static byte[] BitmapToByte(Bitmap bMP, ImageFormat format) {
            if (bMP == null) { return null; }

            if (bMP.PixelFormat != PixelFormat.Format32bppPArgb) { bMP = Bitmap_ChangePixelFormat(bMP); }

            var MemSt = new MemoryStream();
            bMP.Save(MemSt, format);
            return MemSt.ToArray();
        }

        public static string BitmapToStringUnicode(Bitmap bMP, ImageFormat format) {
            if (bMP == null) { return string.Empty; }
            return new string(Encoding.Unicode.GetChars(BitmapToByte(bMP, format)));
        }

        public static Bitmap ByteToBitmap(byte[] value) {
            if (value == null || value.GetUpperBound(0) == 0) { return null; }

            try {
                using var ms = new MemoryStream(value);
                return new Bitmap(ms);
                // return (Bitmap)Image.FromStream(fs);
            } catch {
                Develop.DebugPrint("Fehler bei der Umwandlung!");
                return null;
            }
        }

        // public static string FileToString(string Dateiname) {
        //    try {
        //        var b = FileToByte(Dateiname);
        //        return b.ToStringWIN1252();
        //    } catch {
        //        return string.Empty;
        //    }
        // }

        public static void CartesianToPolar(PointF ko, ref double r, ref double win) {
            r = Math.Sqrt(ko.X * ko.X + ko.Y * ko.Y);
            win = Convert.ToDouble(Geometry.Winkel(0M, 0M, (decimal)ko.X, (decimal)ko.Y));
        }

        #endregion

        public static decimal PixelToMM(decimal pixel, int dPI) {
            return pixel / dPI * 25.4M;
        }

        public static decimal mmToPixel(decimal mM, int dPI) {
            return mM * dPI / 25.4M;
        }

        //private static string GetDriveSerialNumber(string drive)
        //{
        //    var driveSerial = string.Empty;
        //    var driveFixed = Path.GetPathRoot(drive);
        //    driveFixed = driveFixed.Replace("\\", string.Empty);

        //    using (var querySearch = new ManagementObjectSearcher("SELECT VolumeSerialNumber FROM Win32_LogicalDisk Where Name = '" + driveFixed + "'"))
        //    {
        //        using var queryCollection = querySearch.Get();
        //        foreach (ManagementObject moItem in queryCollection)
        //        {
        //            driveSerial = Convert.ToString(moItem["VolumeSerialNumber"]);
        //            break;
        //        }
        //    }

        //    return driveSerial;
        //}

        //public static string Path2SerialNr(string path)
        //{
        //    if (path.Substring(1, 2) != ":\\") { return path; }
        //    if (!PathExists(path.Substring(0, 3))) { return path; }
        //    return GetDriveSerialNumber(path) + path.Substring(1);
        //}

        //public static string SerialNr2Path(string pfad)
        //{
        //    var MustSearch = true;

        //    if (pfad.Length > 2 && pfad.Substring(1, 2) == ":\\") { MustSearch = false; }
        //    if (pfad.Length > 1 && pfad.Substring(0, 2) == "\\\\") { MustSearch = false; }

        //    if (pfad.StartsWith("..\\"))
        //    {
        //        pfad = System.Windows.Forms.Application.StartupPath + "\\" + pfad;
        //        MustSearch = false;
        //    }

        //    if (pfad.Length > 8 && string.Equals(pfad.Substring(0, 9), "APPPATH:\\", StringComparison.OrdinalIgnoreCase))
        //    {
        //        pfad = System.Windows.Forms.Application.StartupPath + pfad.Substring(8);
        //        MustSearch = false;
        //    }

        //    var xq = 0; // = Pfad.IndexOf(":\")
        //    while (true)
        //    {
        //        xq = (pfad + "\\").IndexOf("..\\");
        //        if (xq < 0) { break; }
        //        pfad = pfad.Substring(0, xq).PathParent() + pfad.Substring(xq + 3);
        //    }

        //    if (!MustSearch) { return pfad; }

        //    xq = pfad.IndexOf(":\\");
        //    if (xq < 0) { return pfad; }

        //    if (SerialNr2Path_LastSearch == pfad.Substring(0, xq) && !string.IsNullOrEmpty(SerialNr2Path_LastErgebnis))
        //    {
        //        return SerialNr2Path_LastErgebnis + pfad.Substring(xq);
        //    }

        //    SerialNr2Path_LastSearch = pfad.Substring(0, xq);

        //    var odrive = DriveInfo.GetDrives();

        //    for (var z = 0; z <= odrive.GetUpperBound(0); z++)
        //    {
        //        if (odrive[z].IsReady && GetDriveSerialNumber(odrive[z].Name) == pfad.Substring(0, xq))
        //        {
        //            SerialNr2Path_LastErgebnis = odrive[z].Name.Substring(0, 1);
        //            return SerialNr2Path_LastErgebnis + pfad.Substring(xq);
        //        }
        //    }

        //    SerialNr2Path_LastSearch = string.Empty;
        //    return pfad;
        //}

        /// <summary>
        /// Löst nie einen Fehler aus. Kann der Wert nicht geparsed werden, wird 0 zurückgegeben.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static int IntParse(string s) {
            if (string.IsNullOrEmpty(s)) { return 0; }
            if (int.TryParse(s, out var result)) { return result; }
            Develop.DebugPrint(enFehlerArt.Warnung, "Int kann nicht geparsed werden: " + s);
            return 0;
        }

        /// <summary>
        /// Löst nie einen Fehler aus. Kann der Wert nicht geparsed werden, wird 0 zurückgegeben.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static long LongParse(string s) {
            if (string.IsNullOrEmpty(s)) { return 0; }
            if (long.TryParse(s, out var result)) { return result; }
            Develop.DebugPrint(enFehlerArt.Warnung, "Long kann nicht geparsed werden: " + s);
            return 0;
        }

        /// <summary>
        /// Löst nie einen Fehler aus. Kann der Wert nicht geparsed werden, wird 0 zurückgegeben.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static decimal DecimalParse(string s) {
            if (string.IsNullOrEmpty(s)) { return 0; }
            if (decimal.TryParse(s, out var result)) { return result; }
            // Develop.DebugPrint(enFehlerArt.Warnung, "Decimal kann nicht geparsed werden: " + s);
            return 0;
        }

        /// <summary>
        /// Löst nie einen Fehler aus. Kann der Wert nicht geparsed werden, wird 0 zurückgegeben.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static double DoubleParse(string s) {
            if (string.IsNullOrEmpty(s)) { return 0; }
            if (double.TryParse(s, out var result)) { return result; }

            if (double.TryParse(s.Replace(",", "."), out var result2)) { return result2; }
            if (double.TryParse(s.Replace(".", ","), out var result3)) { return result3; }

            Develop.DebugPrint(enFehlerArt.Warnung, "Double kann nicht geparsed werden: " + s);
            return 0;
        }

        /// <summary>
        /// Löst nie einen Fehler aus. Kann der Wert nicht geparsed werden, wird 0 zurückgegeben.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static float FloatParse(string s) {
            if (string.IsNullOrEmpty(s)) { return 0; }
            if (float.TryParse(s, out var result)) { return result; }

            if (float.TryParse(s.Replace(",", "."), out var result2)) { return result2; }
            if (float.TryParse(s.Replace(".", ","), out var result3)) { return result3; }

            Develop.DebugPrint(enFehlerArt.Warnung, "float kann nicht geparsed werden: " + s);
            return 0;
        }

        /// <summary>
        /// Löst nie einen Fehler aus. Kann der Wert nicht geparsed werden, wird DateTime.Now zurückgegeben.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static DateTime DateTimeParse(string s) {
            if (DateTimeTryParse(s, out var result)) { return result; }
            Develop.DebugPrint(enFehlerArt.Warnung, "Datum kann nicht geparsed werden: " + s);
            return DateTime.Now;
        }

        public static bool DateTimeTryParse(string s, out DateTime result) {
            // https://docs.microsoft.com/de-de/dotnet/standard/base-types/standard-date-and-time-format-strings?view=netframework-4.8
            if (DateTime.TryParseExact(s, Constants.Format_Date5, CultureInfo.InvariantCulture, DateTimeStyles.None, out result)) { return true; }
            if (DateTime.TryParseExact(s, Constants.Format_Date6, CultureInfo.InvariantCulture, DateTimeStyles.None, out result)) { return true; }
            if (DateTime.TryParseExact(s, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out result)) { return true; }
            if (DateTime.TryParseExact(s, "dd.MM.yyyy H:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out result)) { return true; }
            if (DateTime.TryParseExact(s, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out result)) { return true; }
            if (DateTime.TryParseExact(s, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out result)) { return true; }
            if (DateTime.TryParseExact(s, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out result)) { return true; }
            if (DateTime.TryParseExact(s, "dd.MM.yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out result)) { return true; }
            if (DateTime.TryParseExact(s, "d.M.yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out result)) { return true; }

            return false;
        }

    }
}