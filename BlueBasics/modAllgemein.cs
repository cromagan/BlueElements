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

using BlueBasics.Enums;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Media.Imaging;
using static BlueBasics.FileOperations;

namespace BlueBasics {

    public static class modAllgemein {

        #region Fields

        // private static readonly object Pause_Sleeper = new object();
        private static string _GotUserName = string.Empty;

        #endregion

        #region Methods

        public static void AddRad(this GraphicsPath gP, PointF middle, PointF startP, float wink) {
            var radius = (float)Math.Abs(Geometry.Länge(middle, startP));
            var startw = (float)Geometry.Winkel(middle, startP);
            gP.AddArc(middle.X - radius, middle.Y - radius, radius * 2, radius * 2, -startw, -wink);
        }

        public static void CollectGarbage() {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            //GC.WaitForPendingFinalizers();
            //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForFullGCComplete(1000);
        }

        public static bool CreateInternetLink(string saveTo, string linkUrl) {
            var Title = "unbekannt";
            // string deskDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            try {
                using WebClient x = new();
                var source = x.DownloadString(linkUrl);
                Title = Regex.Match(source, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"].Value;
                Title = Title.RemoveChars(Constants.Char_DateiSonderZeichen);
            } catch {
                // Title = "unbekannt";
                // DebugPrint(enFehlerArt.Warnung, ex);
                // return false;
            }
            Title = Title.ReduceToChars(Constants.Char_Buchstaben + Constants.Char_Buchstaben.ToUpper() + "!.,()+-_ " + Constants.Char_Numerals);
            using (StreamWriter writer = new(TempFile(saveTo.TrimEnd("\\") + "\\" + Title + ".url"))) {
                writer.WriteLine("[InternetShortcut]");
                writer.WriteLine("URL=" + linkUrl);
                writer.Flush();
            }
            return true;
        }

        public static bool CreateShortCut(string saveTo, string linkName) {
            try {
                // string deskDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                using (StreamWriter writer = new(TempFile(saveTo + linkName + ".url"))) {
                    var app = Assembly.GetExecutingAssembly().Location;
                    writer.WriteLine("[InternetShortcut]");
                    writer.WriteLine("URL=file:///" + app);
                    writer.WriteLine("IconIndex=0");
                    var icon = app.Replace('\\', '/');
                    writer.WriteLine("IconFile=" + icon);
                    writer.Flush();
                }
                return true;
            } catch (Exception ex) {
                Develop.DebugPrint(enFehlerArt.Warnung, ex);
                return false;
            }
        }

        public static string Download(string url) {
            // My.Computer.Network.DownloadFile("http://.png", "C:\TMP\a.png")
            using WebClient wc = new();
            wc.Encoding = Encoding.UTF8;
            return wc.DownloadString(url);
        }

        public static Image DownloadImage(string url) {
            var request = WebRequest.Create(url);
            var response = request.GetResponse();
            var remoteStream = response.GetResponseStream();
            StreamReader readStream = new(remoteStream);
            var img = Image.FromStream(remoteStream);
            response.Close();
            remoteStream.Close();
            readStream.Close();
            return img;
        }

        // public static Bitmap GetBitmap(BitmapSource source)
        // {
        //    var bmp = new Bitmap(source.PixelWidth, source.PixelHeight, PixelFormat.Format32bppPArgb);
        //    var data = bmp.LockBits(new Rectangle(Point.Empty, bmp.Size), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
        //    source.CopyPixels(System.Windows.Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
        //    bmp.UnlockBits(data);
        //    return bmp;
        // }
        public static Bitmap GetBitmap(BitmapSource bitmapsource, int maxSize) {
            CollectGarbage();
            Pause(0.1, true);
            Bitmap bitmap;
            using (MemoryStream outStream = new()) {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            if (maxSize > 0) {
                bitmap = BitmapExt.Resize(bitmap, maxSize, maxSize, enSizeModes.Breite_oder_Höhe_Anpassen_OhneVergrößern, InterpolationMode.HighQualityBicubic, true);
            }
            return bitmap;
        }

        public static BitmapExt GetEmmbedBitmap(Assembly assembly, string name) {
            if (name.Contains("|")) { return null; }
            if (name.Contains("[")) { return null; }
            using var d = GetEmmbedResource(assembly, name);
            if (d == null) { return null; }
            switch (name.FileType()) {
                case enFileFormat.Image:
                    return new BitmapExt(new Bitmap(d));

                case enFileFormat.Icon:
                    return new BitmapExt(new Icon(d));

                default:
                    Develop.DebugPrint(GetEmmbedResource(assembly, name));
                    return null;
            }
        }

        // public static string ChecksumFileName(string name)
        // {
        // name = name.Replace("\\", "}");
        //    name = name.Replace("/", "}");
        //    name = name.Replace(":", "}");
        //    name = name.Replace("?", "}");
        // name = name.Replace("\r", "");
        // if (name.Length < 100) { return name; }
        // var nn = "";
        // for (var z = 0; z <= name.Length - 21; z++)
        //    {
        //        nn = nn + name.Substring(z, 1);
        //    }
        //    nn = nn + name.Substring(name.Length - 20);
        // return nn;
        // }
        public static Stream GetEmmbedResource(Assembly assembly, string name) => (from ThisString in assembly.GetManifestResourceNames() where ThisString.EndsWith("." + name) select assembly.GetManifestResourceStream(ThisString)).FirstOrDefault();

        public static string GetUrlFileDestination(string filename) {
            var D = File.ReadAllText(filename, Encoding.UTF8).SplitByCRToList();
            return D.TagGet("URL");
        }

        public static void IntensifyBitmap(ref Bitmap bMP) {
            for (var X = 0; X < bMP.Width; X++) {
                for (var Y = 0; Y < bMP.Height; Y++) {
                    var c = bMP.GetPixel(X, Y);
                    if (c.A > 0.5 && bMP.GetPixel(X, Y).GetBrightness() < 0.9) { bMP.SetPixel(X, Y, Color.Black); }
                }
            }
        }

        // BMP_Clear
        // BMP_Generate3DBorder
        // GR_DrawSpecialFilledRect
        // IMG_MiniDateiBin
        //
        //
        //
        /// <summary>
        /// Pixelgenaue Collisionsanalyse zweier Bitmaps
        /// </summary>
        /// <param name="image1"></param>
        /// <param name="pos1">Linke obere Eckte des 1. Bildes</param>
        /// <param name="image2"></param>
        /// <param name="pos2">Linke obere Eckte des 2. Bildes</param>
        /// <param name="accuracy">Genauigkeit der Prüfung. Bei 1 wird jeder Pixel geprüft. Bei z.B. 3 wird nur jeder dritte Pixel geprüft.</param>
        /// <returns></returns>
        public static bool IntersectsWith(Bitmap image1, Point pos1, Bitmap image2, Point pos2, int accuracy) {
            if (image1 == null || image2 == null) { return false; }
            Rectangle Koord1 = new(pos1, image1.Size);
            Rectangle Koord2 = new(pos2, image2.Size);
            if (!Koord1.IntersectsWith(Koord2)) { return false; }
            Rectangle Schnitt = new(Koord1.Location, Koord1.Size);
            Schnitt.Intersect(Koord2);
            for (var x = Schnitt.Left; x < Schnitt.Right; x += accuracy) {
                for (var y = Schnitt.Top; y < Schnitt.Bottom; y += accuracy) {
                    if (!image1.GetPixel(x - Koord1.X, y - Koord1.Y).IsNearWhite(0.9) && !image2.GetPixel(x - Koord2.X, y - Koord2.Y).IsNearWhite(0.9)) {
                        return true;
                    }
                }
            }
            return false;
        }

        public static string LastMouseButton() => Convert.ToBoolean(GetAsyncKeyState(0x1)) ? "Links"
        : Convert.ToBoolean(GetAsyncKeyState(0x2)) ? "Rechts"
: Convert.ToBoolean(GetAsyncKeyState(0x4)) ? "Mitte"
: string.Empty;

        public static void launchBrowser(string url) {
            var browserName = "iexplore.exe";
            var adds = string.Empty;
            using (var userChoiceKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice")) {
                var progIdValue = userChoiceKey?.GetValue("Progid");
                if (progIdValue != null) {
                    if (progIdValue.ToString().IndexOf("chrome", StringComparison.OrdinalIgnoreCase) >= 0) {
                        browserName = "chrome.exe";
                    } else if (progIdValue.ToString().IndexOf("firefox", StringComparison.OrdinalIgnoreCase) >= 0) {
                        browserName = "firefox.exe";
                        // adds = "-private-window -url";
                    } else if (progIdValue.ToString().IndexOf("safari", StringComparison.OrdinalIgnoreCase) >= 0) {
                        browserName = "safari.exe";
                    } else if (progIdValue.ToString().IndexOf("opera", StringComparison.OrdinalIgnoreCase) >= 0) {
                        browserName = "opera.exe";
                    }
                }
            }
            // browserName = "edge.exe";
            Process.Start(new ProcessStartInfo(browserName, adds + " " + url));
        }

        public static int LevenshteinDistance(string txt1, string txt2) {
            var l1 = txt1.Length;
            var l2 = txt2.Length;
            var d = new int[l1 + 2, l2 + 2];
            if (l1 == 0) { return l2; }
            if (l2 == 0) { return l1; }
            for (var i = 0; i <= l1; i++) {
                d[i, 0] = i;
            }
            for (var j = 0; j <= l2; j++) {
                d[0, j] = j;
            }
            for (var i = 1; i <= l1; i++) {
                for (var j = 1; j <= l2; j++) {
                    var cost = txt2[j - 1].ToString()[0] == txt1[i - 1] ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }
            return d[l1, l2];
        }

        public static void Magnify(Bitmap screenshot, Point point, Graphics gR, bool swapX) {
            const int w1 = 200; // Größe des Rechteckes
            const int w5 = 10; // Pixel zum vergrößerm
            int x;
            if (!swapX) {
                x = 150 - (int)(w1 / 2.0);
                if (point.X < screenshot.Width / 2.0) { x = screenshot.Width - 150 - (int)(w1 / 2.0); }
            } else {
                x = screenshot.Width - 150 - (int)(w1 / 2.0);
                if (point.X < screenshot.Width / 2.0) { x = 150 - (int)(w1 / 2.0); }
            }
            var y = 150 - (int)(w1 / 2.0);
            if (point.Y < screenshot.Height / 2.0) { y = screenshot.Height - 150 - (int)(w1 / 2.0); }
            Rectangle r = new(x, y, w1, w1);
            for (var z = 5; z >= 0; z--) {
                r.Inflate(1, 1);
                // r.Expand(0, 0, 1, 1)
                var w = Convert.ToByte(255 / (double)10 * z);
                gR.DrawRectangle(new Pen(Color.FromArgb(w, 0, 0, 0)), r);
            }
            r.Inflate(-5, -5);
            gR.InterpolationMode = InterpolationMode.NearestNeighbor;
            gR.PixelOffsetMode = PixelOffsetMode.Half;
            gR.DrawImage(screenshot, r, new Rectangle(point.X - w5, point.Y - w5, (w5 * 2) + 1, (w5 * 2) + 1), GraphicsUnit.Pixel);
            gR.DrawRectangle(Pens.Black, r);
            var Mitte = r.PointOf(enAlignment.Horizontal_Vertical_Center);
            gR.DrawLine(new Pen(Color.FromArgb(128, 255, 255, 255), 3), Mitte.X, Mitte.Y - 7, Mitte.X, Mitte.Y + 6);
            gR.DrawLine(new Pen(Color.FromArgb(128, 255, 255, 255), 3), Mitte.X - 7, Mitte.Y, Mitte.X + 6, Mitte.Y);
            gR.DrawLine(new Pen(Color.FromArgb(20, 255, 0, 0)), Mitte.X, r.Top, Mitte.X, r.Bottom);
            gR.DrawLine(new Pen(Color.FromArgb(20, 255, 0, 0)), r.Left, Mitte.Y, r.Right, Mitte.Y);
            gR.DrawLine(Pens.Red, Mitte.X, Mitte.Y - 6, Mitte.X, Mitte.Y + 5);
            gR.DrawLine(Pens.Red, Mitte.X - 6, Mitte.Y, Mitte.X + 5, Mitte.Y);
        }

        public static string Nummer(this string nr, int stellen) {
            var M = string.Empty;
            if (nr[0] == '-') {
                M = "-";
                nr = nr.Remove(0, 1);
            }
            StringBuilder x = new();
            for (var z = 1; z <= stellen - nr.Length; z++) {
                x.Append("0");
            }
            x.Append(nr);
            return M + x;
        }

        public static void Pause(double sekunden, bool doEvents) {
            if (sekunden <= 0) { return; }
            if (!doEvents) {
                Thread.Sleep((int)(sekunden * 1000));
                return;
            }
            TimeSpan AkTimer;
            var FirstTimer = DateTime.Now;
            do {
                Develop.DoEvents();
                AkTimer = DateTime.Now.Subtract(FirstTimer);
            } while (AkTimer.TotalSeconds < sekunden);
        }

        public static int PointOnScreenNr(Point cP) {
            for (var zSC = 0; zSC <= System.Windows.Forms.Screen.AllScreens.GetUpperBound(0); zSC++) {
                if (cP.X >= System.Windows.Forms.Screen.AllScreens[zSC].Bounds.Left && cP.Y >= System.Windows.Forms.Screen.AllScreens[zSC].Bounds.Top && cP.X < System.Windows.Forms.Screen.AllScreens[zSC].Bounds.Right && cP.Y < System.Windows.Forms.Screen.AllScreens[zSC].Bounds.Bottom) {
                    return zSC;
                }
            }
            return 0;
        }

        public static GraphicsPath Poly_Arrow(Rectangle rect) {
            GraphicsPath p = new();
            /// --------+  >
            ///         | /
            ///         |/
            ///
            PointF plusOben = new((float)(rect.Left + (rect.Width * 0.5)), (float)(rect.PointOf(enAlignment.VerticalCenter_Right).Y - (rect.Height * 0.18)));
            PointF plusUnten = new((float)(rect.Left + (rect.Width * 0.5)), (float)(rect.PointOf(enAlignment.VerticalCenter_Right).Y + (rect.Height * 0.18)));
            p.AddLine(rect.PointOf(enAlignment.VerticalCenter_Right), new PointF(plusUnten.X, rect.Bottom));
            p.AddLine(p.GetLastPoint(), plusUnten);
            p.AddLine(p.GetLastPoint(), new PointF(rect.Left, plusUnten.Y));
            p.AddLine(p.GetLastPoint(), new PointF(rect.Left, plusOben.Y));
            p.AddLine(p.GetLastPoint(), plusOben);
            p.AddLine(p.GetLastPoint(), new PointF(plusOben.X, rect.Top));
            p.CloseFigure();
            return p;
        }

        public static GraphicsPath Poly_Bruchlinie(Rectangle rect) {
            GraphicsPath p = new();
            p.AddLine(rect.PointOf(enAlignment.Top_Left), rect.PointOf(enAlignment.Top_Right));
            p.AddLine(p.GetLastPoint(), rect.PointOf(enAlignment.Bottom_Right));
            p.AddLine(p.GetLastPoint(), rect.PointOf(enAlignment.Bottom_Left));
            var versX = rect.Width / 6;
            var versY = -rect.Height / 10;
            var pu = p.GetLastPoint();
            for (var z = 0; z < 10; z++) {
                pu.Y += versY;
                pu.X += versX;
                versX *= -1;
                p.AddLine(p.GetLastPoint(), pu);
            }
            p.CloseFigure();
            return p;
        }

        public static GraphicsPath Poly_Rechteck(Rectangle rect) {
            GraphicsPath tempPoly_Rechteck = new();
            tempPoly_Rechteck.AddRectangle(rect);
            tempPoly_Rechteck.CloseFigure();
            return tempPoly_Rechteck;
        }

        public static GraphicsPath Poly_RoundRec(Rectangle rect, int radius) => Poly_RoundRec(rect.X, rect.Y, rect.Width, rect.Height, radius);

        public static GraphicsPath Poly_RoundRec(int x, int y, int width, int height, int radius) {
            if (width < 1 || height < 1) { return null; }
            GraphicsPath tempPoly_RoundRec = new();
            if (radius > (height / 2.0) + 2) { radius = (int)(height / 2.0) + 2; }
            if (radius > (width / 2.0) + 2) { radius = (int)(width / 2.0) + 2; }
            tempPoly_RoundRec.AddLine(x + radius, y, x + width - radius, y);
            AddRad90(x + width - radius, y, radius, 270); // OK
            tempPoly_RoundRec.AddLine(x + width, y + radius, x + width, y + height - radius);
            AddRad90(x + width - radius, y + height - radius, radius, 0);
            tempPoly_RoundRec.AddLine(x + width - radius, y + height, x + radius, y + height);
            AddRad90(x, y + height - radius, radius, 90);
            tempPoly_RoundRec.AddLine(x, y + height - radius, x, y + radius);
            AddRad90(x, y, radius, 180); // OK
            tempPoly_RoundRec.CloseFigure();
            return tempPoly_RoundRec;

            void AddRad90(int mxX, int mxY, int Radius, int gradStart) => tempPoly_RoundRec.AddArc(mxX, mxY, Radius, Radius, gradStart, 90);
        }

        public static GraphicsPath Poly_Triangle(PointF p1, PointF p2, PointF p3) {
            GraphicsPath P = new();
            P.AddLine(p1, p2);
            P.AddLine(p2, p3);
            P.CloseFigure();
            return P;
        }

        public static Rectangle RectangleOfAllScreens() {
            var x1 = int.MaxValue;
            var y1 = int.MaxValue;
            var x2 = int.MinValue;
            var y2 = int.MinValue;
            for (var zSC = 0; zSC <= System.Windows.Forms.Screen.AllScreens.GetUpperBound(0); zSC++) {
                x1 = Math.Min(x1, System.Windows.Forms.Screen.AllScreens[zSC].Bounds.Left);
                y1 = Math.Min(y1, System.Windows.Forms.Screen.AllScreens[zSC].Bounds.Top);
                x2 = Math.Max(x2, System.Windows.Forms.Screen.AllScreens[zSC].Bounds.Right);
                y2 = Math.Max(y2, System.Windows.Forms.Screen.AllScreens[zSC].Bounds.Bottom);
            }
            Point GP = new(x1, y1);
            Size sz = new(-x1 + x2, -y1 + y2);
            return new Rectangle(GP, sz);
        }

        /// <summary>
        /// Gibt einen Wert zwischen 0 und 1 zurpück
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double Sigmoid(double x) => 1 / (1 + Math.Exp(-x));

        public static List<Bitmap> SplitTiff(string fileName, int maxSize) {
            // Open a Stream and decode a TIFF image
            FileStream imageStreamSource = new(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            List<Bitmap> l = new();
            var frames = 1;
            try {
                TiffBitmapDecoder decoder = new(imageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                frames = decoder.Frames.Count;
                foreach (var frame in decoder.Frames) {
                    l.Add(GetBitmap(frame, maxSize));
                }
            } catch {
                try {
                    l.Clear();
                    CollectGarbage();
                    var x = (Bitmap)BitmapExt.Image_FromFile(fileName);
                    l.Add(BitmapExt.Resize(x, maxSize, maxSize, enSizeModes.Breite_oder_Höhe_Anpassen_OhneVergrößern, InterpolationMode.HighQualityBicubic, true));
                    if (frames > 1) {
                        Bitmap x2 = new(200, 200);
                        var gr = Graphics.FromImage(x2);
                        gr.Clear(Color.White);
                        gr.DrawString("Weitere Blätter vorhanden!", new Font("Arial", 9), Brushes.Red, new Point(0, 0));
                        l.Add(x2);
                    }
                } catch (Exception ex) {
                    l.Clear();
                    CollectGarbage();
                    Bitmap x2 = new(200, 200);
                    var gr = Graphics.FromImage(x2);
                    gr.Clear(Color.White);
                    gr.DrawString("Vorschaubild fehlgeschlagen!", new Font("Arial", 9), Brushes.Red, new Point(0, 0));
                    l.Add(x2);
                    Develop.DebugPrint(ex);
                }
            }
            imageStreamSource.Close();
            imageStreamSource.Dispose();
            return l;
        }

        public static void Swap<T>(ref T w1, ref T w2) {
            var W3 = w1;
            w1 = w2;
            w2 = W3;
        }

        /// <summary>
        /// Gibt einen Wert zwischen -1 und 1 zurück
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static float TangensHyperbolicus(double x) {
            if (x > 20) { return 1; }
            var et = (float)Math.Pow(Math.E, x);
            return (et / (1 + et) * 2) - 1;
        }

        public static string UserName() {
            if (!string.IsNullOrEmpty(_GotUserName)) { return _GotUserName; }
            _GotUserName = WindowsIdentity.GetCurrent().Name;
            if (_GotUserName.Contains("\\")) { _GotUserName = _GotUserName.FileNameWithSuffix(); }
            return _GotUserName;
        }

        [DllImport("user32", EntryPoint = "GetAsyncKeyState", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern short GetAsyncKeyState(int vKey);

        #endregion

        // public static string Nummer(this double Nr, int Stellen, int NachKomma)
        // {
        //    return Nummer(Nr.ToString(), Stellen, NachKomma);
        // }
        // public static string Nummer(this double Nr, int Stellen, int NachKomma)
        // {
        //    return Nummer(Nr.ToString(), Stellen, NachKomma);
        // }
        // public static string Nummer(this string Nrs, int Stellen, int NachKomma)
        // {
        // if (!Nrs.Contains(","))
        //    {
        //        Nrs = Nrs + ",0";
        //    }
        // var T = Nrs.Split(',');
        // T[0] = Nummer(T[0], Stellen);
        // if (T[1].Length > NachKomma)
        //    {
        //        T[1] = T[1].TrimEnd('0');
        //    }
        // do
        //    {
        //        if (T[1].Length < NachKomma)
        //        {
        //            T[1] = T[1] + "0";
        //        }
        //        else
        //        {
        //            break;
        //        }
        //    } while (true);
        // if (T[1].Length > 0)
        //    {
        //        return T[0] + "," + T[1];
        //    }
        // return T[0];
        // }
    }
}