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
using BlueBasics.Enums;
using Microsoft.Win32;
using static BlueBasics.FileOperations;
using System.Windows.Media.Imaging;

namespace BlueBasics
{
    public static class modAllgemein
    {



        //private static readonly object Pause_Sleeper = new object();
        private static string _GotUserName = "";

        [DllImport("user32", EntryPoint = "GetAsyncKeyState", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern short GetAsyncKeyState(int vKey);

        #region  Grafics/BMP - Bearbeitung 
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
        /// <param name="Image1"></param>
        /// <param name="Pos1">Linke obere Eckte des 1. Bildes</param>
        /// <param name="Image2"></param>
        /// <param name="Pos2">Linke obere Eckte des 2. Bildes</param>
        /// <param name="Accuracy">Genauigkeit der Prüfung. Bei 1 wird jeder Pixel geprüft. Bei z.B. 3 wird nur jeder dritte Pixel geprüft.</param>
        /// <returns></returns>

        public static bool IntersectsWith(Bitmap Image1, Point Pos1, Bitmap Image2, Point Pos2, int Accuracy)
        {

            if (Image1 == null || Image2 == null) { return false; }


            var Koord1 = new Rectangle(Pos1, Image1.Size);
            var Koord2 = new Rectangle(Pos2, Image2.Size);

            if (!Koord1.IntersectsWith(Koord2)) { return false; }

            var Schnitt = new Rectangle(Koord1.Location, Koord1.Size);
            Schnitt.Intersect(Koord2);

            for (var x = Schnitt.Left; x < Schnitt.Right; x += Accuracy)
            {
                for (var y = Schnitt.Top; y < Schnitt.Bottom; y += Accuracy)
                {
                    if (!Image1.GetPixel(x - Koord1.X, y - Koord1.Y).IsNearWhite(0.9) && !Image2.GetPixel(x - Koord2.X, y - Koord2.Y).IsNearWhite(0.9))
                    {
                        return true;
                    }
                }
            }
            return false;

        }




        public static List<Bitmap> SplitTiff(string fileName, int MaxSize)
        {

            // Open a Stream and decode a TIFF image
            var imageStreamSource = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            var l = new List<Bitmap>();
            var frames = 1;

            try
            {

                var decoder = new TiffBitmapDecoder(imageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                frames = decoder.Frames.Count;

                foreach (var frame in decoder.Frames)
                {
                    l.Add(GetBitmap(frame, MaxSize));
                }

            }
            catch
            {


                try
                {
                    l.Clear();
                    CollectGarbage();

                    var x = (Bitmap)BitmapExt.Image_FromFile(fileName);
                    l.Add(BitmapExt.Resize(x, MaxSize, MaxSize, enSizeModes.Breite_oder_Höhe_Anpassen_OhneVergrößern, InterpolationMode.HighQualityBicubic, true));


                    if (frames > 1)
                    {
                        var x2 = new Bitmap(200, 200);
                        var gr = Graphics.FromImage(x2);
                        gr.Clear(Color.White);
                        gr.DrawString("Weitere Blätter vorhanden!", new Font("Arial", 9), Brushes.Red, new Point(0, 0));
                        l.Add(x2);
                    }
                }
                catch (Exception ex)
                {
                    l.Clear();
                    CollectGarbage();
                    var x2 = new Bitmap(200, 200);
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



        //public static Bitmap GetBitmap(BitmapSource source)
        //{
        //    var bmp = new Bitmap(source.PixelWidth, source.PixelHeight, PixelFormat.Format32bppPArgb);
        //    var data = bmp.LockBits(new Rectangle(Point.Empty, bmp.Size), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
        //    source.CopyPixels(System.Windows.Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
        //    bmp.UnlockBits(data);
        //    return bmp;
        //}

        public static Bitmap GetBitmap(BitmapSource bitmapsource, int MaxSize)
        {
            CollectGarbage();
            Pause(0.1, true);

            Bitmap bitmap;
            using (var outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }

            if (MaxSize > 0)
            {
                bitmap = BitmapExt.Resize(bitmap, MaxSize, MaxSize, enSizeModes.Breite_oder_Höhe_Anpassen_OhneVergrößern, InterpolationMode.HighQualityBicubic, true);
            }

            return bitmap;
        }





        public static void Magnify(Bitmap Screenshot, Point Point, Graphics GR, bool SwapX)
        {
            const int w1 = 200; // Größe des Rechteckes
            const int w5 = 10; // Pixel zum vergrößerm

            var x = 0;

            if (!SwapX)
            {
                x = 150 - (int)(w1 / 2.0);
                if (Point.X < Screenshot.Width / 2.0) { x = Screenshot.Width - 150 - (int)(w1 / 2.0); }
            }
            else
            {
                x = Screenshot.Width - 150 - (int)(w1 / 2.0);
                if (Point.X < Screenshot.Width / 2.0) { x = 150 - (int)(w1 / 2.0); }
            }


            var y = 150 - (int)(w1 / 2.0);
            if (Point.Y < Screenshot.Height / 2.0) { y = Screenshot.Height - 150 - (int)(w1 / 2.0); }

            var r = new Rectangle(x, y, w1, w1);


            for (var z = 5; z >= 0; z--)
            {
                r.Inflate(1, 1);
                // r.Expand(0, 0, 1, 1)
                var w = Convert.ToByte(255 / (double)10 * z);
                GR.DrawRectangle(new Pen(Color.FromArgb(w, 0, 0, 0)), r);
            }

            r.Inflate(-5, -5);

            GR.InterpolationMode = InterpolationMode.NearestNeighbor;
            GR.PixelOffsetMode = PixelOffsetMode.Half;
            GR.DrawImage(Screenshot, r, new Rectangle(Point.X - w5, Point.Y - w5, w5 * 2 + 1, w5 * 2 + 1), GraphicsUnit.Pixel);
            GR.DrawRectangle(Pens.Black, r);

            var Mitte = r.PointOf(enAlignment.Horizontal_Vertical_Center);

            GR.DrawLine(new Pen(Color.FromArgb(128, 255, 255, 255), 3), Mitte.X, Mitte.Y - 7, Mitte.X, Mitte.Y + 6);
            GR.DrawLine(new Pen(Color.FromArgb(128, 255, 255, 255), 3), Mitte.X - 7, Mitte.Y, Mitte.X + 6, Mitte.Y);


            GR.DrawLine(new Pen(Color.FromArgb(20, 255, 0, 0)), Mitte.X, r.Top, Mitte.X, r.Bottom);
            GR.DrawLine(new Pen(Color.FromArgb(20, 255, 0, 0)), r.Left, Mitte.Y, r.Right, Mitte.Y);

            GR.DrawLine(Pens.Red, Mitte.X, Mitte.Y - 6, Mitte.X, Mitte.Y + 5);
            GR.DrawLine(Pens.Red, Mitte.X - 6, Mitte.Y, Mitte.X + 5, Mitte.Y);
        }

        public static Rectangle RectangleOfAllScreens()
        {

            var x1 = int.MaxValue;
            var y1 = int.MaxValue;
            var x2 = int.MinValue;
            var y2 = int.MinValue;


            for (var zSC = 0; zSC <= System.Windows.Forms.Screen.AllScreens.GetUpperBound(0); zSC++)
            {
                x1 = Math.Min(x1, System.Windows.Forms.Screen.AllScreens[zSC].Bounds.Left);
                y1 = Math.Min(y1, System.Windows.Forms.Screen.AllScreens[zSC].Bounds.Top);

                x2 = Math.Max(x2, System.Windows.Forms.Screen.AllScreens[zSC].Bounds.Right);
                y2 = Math.Max(y2, System.Windows.Forms.Screen.AllScreens[zSC].Bounds.Bottom);
            }


            var GP = new Point(x1, y1);
            var sz = new Size(-x1 + x2, -y1 + y2);

            return new Rectangle(GP, sz);
        }

        #endregion

        #region  Polygone 


        public static GraphicsPath Poly_Triangle(PointF P1, PointF P2, PointF P3)
        {

            var P = new GraphicsPath();

            P.AddLine(P1, P2);
            P.AddLine(P2, P3);


            P.CloseFigure();


            return P;
        }

        public static GraphicsPath Poly_RoundRec(Rectangle rect, int radius)
        {
            return Poly_RoundRec(rect.X, rect.Y, rect.Width, rect.Height, radius);
        }


        public static GraphicsPath Poly_RoundRec(int x, int y, int width, int height, int radius)
        {

            if (width < 1 || height < 1) { return null; }


            var tempPoly_RoundRec = new GraphicsPath();
            if (radius > height / 2.0 + 2) { radius = (int)(height / 2.0) + 2; }
            if (radius > width / 2.0 + 2) { radius = (int)(width / 2.0) + 2; }

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


            void AddRad90(int MxX, int MxY, int Radius, int GradStart)
            {
                tempPoly_RoundRec.AddArc(MxX, MxY, Radius, Radius, GradStart, 90);
            }

        }


        public static GraphicsPath Poly_Rechteck(Rectangle rect)
        {
            GraphicsPath tempPoly_Rechteck = null;
            tempPoly_Rechteck = new GraphicsPath();
            tempPoly_Rechteck.AddRectangle(rect);
            tempPoly_Rechteck.CloseFigure();
            return tempPoly_Rechteck;
        }

        public static GraphicsPath Poly_Bruchlinie(Rectangle rect)
        {
            GraphicsPath p = null;
            p = new GraphicsPath();
            p.AddLine(rect.PointOf(enAlignment.Top_Left), rect.PointOf(enAlignment.Top_Right));
            p.AddLine(p.GetLastPoint(), rect.PointOf(enAlignment.Bottom_Right));
            p.AddLine(p.GetLastPoint(), rect.PointOf(enAlignment.Bottom_Left));


            var versX = rect.Width / 6;
            var versY = -rect.Height / 10;

            var pu = p.GetLastPoint();

            for (var z = 0; z < 10; z++)
            {
                pu.Y += versY;
                pu.X += versX;
                versX *= -1;

                p.AddLine(p.GetLastPoint(), pu);
            }

            p.CloseFigure();
            return p;

        }



        public static GraphicsPath Poly_Arrow(Rectangle rect)
        {
            var p = new GraphicsPath();



            /// --------+  >
            ///         | /
            ///         |/
            ///        

            var plusOben = new PointF((float)(rect.Left + rect.Width * 0.5), (float)(rect.PointOf(enAlignment.VerticalCenter_Right).Y - rect.Height * 0.18));
            var plusUnten = new PointF((float)(rect.Left + rect.Width * 0.5), (float)(rect.PointOf(enAlignment.VerticalCenter_Right).Y + rect.Height * 0.18));

            p.AddLine(rect.PointOf(enAlignment.VerticalCenter_Right), new PointF(plusUnten.X, rect.Bottom));
            p.AddLine(p.GetLastPoint(), plusUnten);
            p.AddLine(p.GetLastPoint(), new PointF(rect.Left, plusUnten.Y));
            p.AddLine(p.GetLastPoint(), new PointF(rect.Left, plusOben.Y));
            p.AddLine(p.GetLastPoint(), plusOben);
            p.AddLine(p.GetLastPoint(), new PointF(plusOben.X, rect.Top));


            p.CloseFigure();
            return p;




        }




        public static void AddRad(this GraphicsPath GP, PointF middle, PointF startP, float Wink)
        {

            var radius = (float)Math.Abs(Geometry.Länge(middle, startP));
            var startw = (float)Geometry.Winkel(middle, startP);


            GP.AddArc(middle.X - radius, middle.Y - radius, radius * 2, radius * 2, -startw, -Wink);



        }



        #endregion


        #region  Variablen und String 


        public static void Swap<T>(ref T W1, ref T W2)
        {
            var W3 = W1;
            W1 = W2;
            W2 = W3;
        }




        #region  Nummer 






        public static string Nummer(this string Nr, int Stellen)
        {

            var M = "";
            if (Nr[0] == '-')
            {
                M = "-";
                Nr = Nr.Remove(0, 1);
            }

            var x = new StringBuilder();

            for (var z = 1; z <= Stellen - Nr.Length; z++)
            {
                x.Append("0");
            }

            x.Append(Nr);
            return M + x;
        }

        //public static string Nummer(this decimal Nr, int Stellen, int NachKomma)
        //{
        //    return Nummer(Nr.ToString(), Stellen, NachKomma);
        //}

        //public static string Nummer(this double Nr, int Stellen, int NachKomma)
        //{
        //    return Nummer(Nr.ToString(), Stellen, NachKomma);
        //}

        //public static string Nummer(this string Nrs, int Stellen, int NachKomma)
        //{

        //    if (!Nrs.Contains(","))
        //    {
        //        Nrs = Nrs + ",0";
        //    }

        //    var T = Nrs.Split(',');

        //    T[0] = Nummer(T[0], Stellen);

        //    if (T[1].Length > NachKomma)
        //    {
        //        T[1] = T[1].TrimEnd('0');
        //    }


        //    do
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

        //    if (T[1].Length > 0)
        //    {
        //        return T[0] + "," + T[1];
        //    }

        //    return T[0];

        //}



        #endregion


        #endregion

        #region  Datum und Zeit 


        public static void Pause(double sekunden, bool doEvents)
        {

            if (sekunden <= 0) { return; }


            if (!doEvents)
            {
                Thread.Sleep((int)(sekunden * 1000));
                return;
            }

            TimeSpan AkTimer;
            var FirstTimer = DateTime.Now;

            do
            {
                Develop.DoEvents();
                AkTimer = DateTime.Now.Subtract(FirstTimer);
            } while (!(AkTimer.TotalSeconds >= sekunden));
        }


        #endregion

        public static void CollectGarbage()
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForFullGCComplete();

        }

        public static string UserName()
        {
            if (!string.IsNullOrEmpty(_GotUserName)) { return _GotUserName; }
            _GotUserName = WindowsIdentity.GetCurrent().Name;
            if (_GotUserName.Contains("\\")) { _GotUserName = _GotUserName.FileNameWithSuffix(); }
            return _GotUserName;
        }


        public static bool ExecuteFile(string FileName, string Arguments = null, bool WaitForExit = false, bool LogException = true)
        {
            try
            {
                if (string.IsNullOrEmpty(FileName) && string.IsNullOrEmpty(Arguments)) { return false; }

                Process Processx = null;

                if (Arguments == null)
                {
                    Processx = Process.Start(FileName);
                }
                else
                {
                    Processx = Process.Start(FileName, Arguments);
                }

                if (WaitForExit)
                {
                    if (Processx == null) { return true; }// Windows 8, DANKE!

                    Processx.WaitForExit();
                    Processx.Dispose();
                }
            }
            catch (Exception ex)
            {
                if (LogException) { Develop.DebugPrint("ExecuteFile konnte nicht ausgeführt werden:<br>" + ex.Message + "<br>Datei: " + FileName); }
                return false;
            }


            return true;
        }




        public static string ChecksumFileName(string name)
        {

            name = name.Replace("\\", "}");
            name = name.Replace("/", "}");
            name = name.Replace(":", "}");
            name = name.Replace("?", "}");

            name = name.Replace("\r", "");

            if (name.Length < 100) { return name; }

            var nn = "";

            for (var z = 0; z <= name.Length - 21; z++)
            {
                nn = nn + name.Substring(z, 1);
            }
            nn = nn + name.Substring(name.Length - 20);


            return nn;
        }


        public static string LastMouseButton()
        {
            const int VK_LBUTTON = 0x1;
            const int VK_RBUTTON = 0x2;
            const int VK_MBUTTON = 0x4;

            if (Convert.ToBoolean(GetAsyncKeyState(VK_LBUTTON)))
            {
                return "Links";
            }

            if (Convert.ToBoolean(GetAsyncKeyState(VK_RBUTTON)))
            {
                return "Rechts";
            }

            if (Convert.ToBoolean(GetAsyncKeyState(VK_MBUTTON)))
            {
                return "Mitte";
            }

            return string.Empty;
        }


        public static void SaveToDisk(string DateiName, string Text2Save, bool ExecuteAfter)
        {

            switch (DateiName.FileType())
            {
                case enFileFormat.HTML:
                case enFileFormat.XMLFile:
                    File.WriteAllText(DateiName, Text2Save, Encoding.UTF8);

                    break;
                case enFileFormat.ProgrammingCode:
                    File.WriteAllText(DateiName, Text2Save, Encoding.Unicode);
                    break;
                default:
                    File.WriteAllText(DateiName, Text2Save, Encoding.Default);
                    break;
            }

            if (ExecuteAfter) { ExecuteFile(DateiName); }

        }

        public static string LoadFromDisk(string DateiName)
        {


            switch (DateiName.FileSuffix())
            {
                case "XML":
                    return File.ReadAllText(DateiName, Encoding.UTF8);
                default:
                    return File.ReadAllText(DateiName, Encoding.Default);
            }


        }


        public static string Download(string Url)
        {
            //  My.Computer.Network.DownloadFile("http://.png", "C:\TMP\a.png")

            using (var wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                return wc.DownloadString(Url);
            }
        }

        public static bool CreateInternetLink(string SaveTo, string linkUrl)
        {


            var Title = "unbekannt";

            //string deskDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            try
            {
                var x = new WebClient();
                var source = x.DownloadString(linkUrl);
                Title = Regex.Match(source, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"].Value;
                Title = Title.RemoveChars(Constants.Char_DateiSonderZeichen);
            }
            catch (Exception)
            {
                //Title = "unbekannt";
                //DebugPrint(enFehlerArt.Warnung, ex);
                //return false;
            }

            Title = Title.ReduceToChars(Constants.Char_Buchstaben + Constants.Char_Buchstaben.ToUpper() + "!.,()+-_ " + Constants.Char_Numerals);

            using (var writer = new StreamWriter(TempFile(SaveTo.TrimEnd("\\") + "\\" + Title + ".url")))
            {
                writer.WriteLine("[InternetShortcut]");
                writer.WriteLine("URL=" + linkUrl);
                writer.Flush();
            }
            return true;

        }

        public static string GetUrlFileDestination(string Filename)
        {
            var D = LoadFromDisk(Filename).SplitByCRToList();
            return D.TagGet("URL");
        }

        public static bool CreateShortCut(string SaveTo, string linkName)
        {
            try
            {
                //string deskDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

                using (var writer = new StreamWriter(TempFile(SaveTo + linkName + ".url")))
                {
                    var app = Assembly.GetExecutingAssembly().Location;
                    writer.WriteLine("[InternetShortcut]");
                    writer.WriteLine("URL=file:///" + app);
                    writer.WriteLine("IconIndex=0");
                    var icon = app.Replace('\\', '/');
                    writer.WriteLine("IconFile=" + icon);
                    writer.Flush();
                }
                return true;
            }
            catch (Exception ex)
            {
                Develop.DebugPrint(enFehlerArt.Warnung, ex);
                return false;
            }
        }

        public static Image DownloadImage(string Url)
        {

            WebResponse response = null;
            Stream remoteStream = null;
            StreamReader readStream = null;
            var request = WebRequest.Create(Url);
            response = request.GetResponse();
            remoteStream = response.GetResponseStream();
            readStream = new StreamReader(remoteStream);
            var img = Image.FromStream(remoteStream);
            response.Close();
            remoteStream.Close();
            readStream.Close();

            return img;
        }


        public static void launchBrowser(string url)
        {
            var browserName = "iexplore.exe";
            var adds = string.Empty;
            using (var userChoiceKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice"))
            {
                var progIdValue = userChoiceKey?.GetValue("Progid");
                if (progIdValue != null)
                {
                    if (progIdValue.ToString().ToLower().Contains("chrome"))
                    {
                        browserName = "chrome.exe";
                    }
                    else if (progIdValue.ToString().ToLower().Contains("firefox"))
                    {
                        browserName = "firefox.exe";
                        //adds = "-private-window -url";
                    }
                    else if (progIdValue.ToString().ToLower().Contains("safari"))
                    {
                        browserName = "safari.exe";
                    }
                    else if (progIdValue.ToString().ToLower().Contains("opera"))
                    {
                        browserName = "opera.exe";
                    }
                }
            }
            //browserName = "edge.exe";

            Process.Start(new ProcessStartInfo(browserName, adds + " " + url));
        }

        public static int PointOnScreenNr(Point CP)
        {

            for (var zSC = 0; zSC <= System.Windows.Forms.Screen.AllScreens.GetUpperBound(0); zSC++)
            {
                if (CP.X >= System.Windows.Forms.Screen.AllScreens[zSC].Bounds.Left && CP.Y >= System.Windows.Forms.Screen.AllScreens[zSC].Bounds.Top && CP.X < System.Windows.Forms.Screen.AllScreens[zSC].Bounds.Right && CP.Y < System.Windows.Forms.Screen.AllScreens[zSC].Bounds.Bottom)
                {
                    return zSC;
                }
            }

            return 0;
        }


        //public static string ChecksumFileName(string name)
        //{

        //    name = name.Replace("\\", "}");
        //    name = name.Replace("/", "}");
        //    name = name.Replace(":", "}");
        //    name = name.Replace("?", "}");

        //    name = name.Replace("\r", "");

        //    if (name.Length < 100) { return name; }

        //    var nn = "";

        //    for (var z = 0; z <= name.Length - 21; z++)
        //    {
        //        nn = nn + name.Substring(z, 1);
        //    }
        //    nn = nn + name.Substring(name.Length - 20);


        //    return nn;
        //}






        public static Stream GetEmmbedResource(Assembly assembly, string Name)
        {
            return (from ThisString in assembly.GetManifestResourceNames() where ThisString.EndsWith("." + Name) select assembly.GetManifestResourceStream(ThisString)).FirstOrDefault();
        }

        public static BitmapExt GetEmmbedBitmap(Assembly assembly, string Name)
        {

            if (Name.Contains("|")) { return null; }
            if (Name.Contains("[")) { return null; }

            using (var d = GetEmmbedResource(assembly, Name))
            {
                if (d == null) { return null; }

                switch (Name.FileType())
                {
                    case enFileFormat.Image:
                        return new BitmapExt(new Bitmap(d));

                    case enFileFormat.Icon:
                        return new BitmapExt(new Icon(d).ToBitmap());

                    default:
                        Develop.DebugPrint(GetEmmbedResource(assembly, Name));
                        return null;
                }
            }
        }


        public static int LevenshteinDistance(string txt1, string txt2)
        {
            var l1 = txt1.Length;
            var l2 = txt2.Length;
            var d = new int[l1 + 2, l2 + 2];

            if (l1 == 0) { return l2; }
            if (l2 == 0) { return l1; }



            for (var i = 0; i <= l1; i++)
            {
                d[i, 0] = i;
            }

            for (var j = 0; j <= l2; j++)
            {
                d[0, j] = j;
            }

            for (var i = 1; i <= l1; i++)
            {
                for (var j = 1; j <= l2; j++)
                {

                    var cost = 0;
                    if (txt2[j - 1].ToString()[0] == txt1[i - 1])
                    {
                        cost = 0;
                    }
                    else
                    {
                        cost = 1;
                    }

                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }

            return d[l1, l2];
        }




        public static void IntensifyBitmap(ref Bitmap BMP)
        {
            for (var X = 0; X < BMP.Width; X++)
            {
                for (var Y = 0; Y < BMP.Height; Y++)
                {
                    var c = BMP.GetPixel(X, Y);
                    if (c.A > 0.5 && BMP.GetPixel(X, Y).GetBrightness() < 0.9) { BMP.SetPixel(X, Y, Color.Black); }
                }
            }
        }

        /// <summary>
        /// Gibt einen Wert zwischen 0 und 1 zurpück
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double Sigmoid(double x)
        {
            return 1 / (1 + Math.Exp(-x));
        }

        /// <summary>
        /// Gibt einen Wert zwischen -1 und 1 zurück
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static float TangensHyperbolicus(double x)
        {

            if (x > 20) { return 1; }

            var et = (float)Math.Pow(Math.E, x);
            return et / (1 + et) * 2 - 1;
        }


        public static byte[] SimpleCrypt(byte[] b, string Pass, int Direction, int Start, int End)
        {
            if (string.IsNullOrEmpty(Pass)) { return b; }
            if (End <= Start) { return b; }

            for (var z = Start; z <= End; z++)
            {
                var TMP = b[z] + Pass[z % Pass.Length] * Direction;
                if (TMP < 0) { TMP += 256; }
                if (TMP > 255) { TMP -= 256; }
                b[z] = (byte)TMP;
            }
            return b;
        }


        public static byte[] SimpleCrypt(byte[] b, string Pass, int Direction)
        {
            return SimpleCrypt(b, Pass, Direction, 0, b.GetUpperBound(0));
        }


        public static List<byte> SimpleCrypt(List<byte> b, string Pass, int Direction, int Start, int End)
        {
            if (string.IsNullOrEmpty(Pass)) { return b; }
            if (End <= Start) { return b; }

            for (var z = Start; z <= End; z++)
            {
                var TMP = b[z] + Pass[z % Pass.Length] * Direction;
                if (TMP < 0) { TMP += 256; }
                if (TMP > 255) { TMP -= 256; }
                b[z] = (byte)TMP;
            }
            return b;
        }


        public static List<byte> SimpleCrypt(List<byte> b, string Pass, int Direction)
        {
            return SimpleCrypt(b, Pass, Direction, 0, b.Count - 1);
        }

    }
}