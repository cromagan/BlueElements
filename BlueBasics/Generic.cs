// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using static BlueBasics.FileOperations;

namespace BlueBasics {

    public static class Generic {

        #region Fields

        private static string _gotUserName = string.Empty;

        #endregion

        #region Methods

        public static void CollectGarbage() {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            //GC.WaitForPendingFinalizers();
            //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForFullGCComplete(1000);
        }

        public static bool CopytoClipboard(string text) {
            //https://stackoverflow.com/questions/68666/clipbrd-e-cant-open-error-when-setting-the-clipboard-from-net

            for (var i = 0; i < 10; i++) {
                try {
                    Clipboard.SetText(text);
                    return true;
                } catch { }

                Pause(0.01, false);
            }
            return false;
        }

        public static bool CreateInternetLink(string saveTo, string linkUrl) {
            var title = "unbekannt";
            // string deskDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            try {
                using WebClient x = new();
                var source = x.DownloadString(linkUrl);
                title = Regex.Match(source, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"].Value;
                title = title.RemoveChars(Constants.Char_DateiSonderZeichen);
            } catch {
                // Title = "unbekannt";
                // DebugPrint(enFehlerArt.Warnung, ex);
                // return false;
            }
            title = title.ReduceToChars(Constants.Char_Buchstaben + Constants.Char_Buchstaben.ToUpper() + "!.,()+-_ " + Constants.Char_Numerals);
            using StreamWriter writer = new(TempFile(saveTo.TrimEnd("\\") + "\\" + title + ".url"));
            writer.WriteLine("[InternetShortcut]");
            writer.WriteLine("URL=" + linkUrl);
            writer.Flush();
            return true;
        }

        public static bool CreateShortCut(string saveTo, string linkName) {
            try {
                // string deskDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                using StreamWriter writer = new(TempFile(saveTo + linkName + ".url"));
                var app = Assembly.GetExecutingAssembly().Location;
                writer.WriteLine("[InternetShortcut]");
                writer.WriteLine("URL=file:///" + app);
                writer.WriteLine("IconIndex=0");
                var icon = app.Replace('\\', '/');
                writer.WriteLine("IconFile=" + icon);
                writer.Flush();
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
            if (remoteStream != null) {
                remoteStream.Close();
            }

            readStream.Close();
            return img;
        }

        public static Stream GetEmmbedResource(Assembly assembly, string name) => (from thisString in assembly.GetManifestResourceNames() where thisString.EndsWith("." + name) select assembly.GetManifestResourceStream(thisString)).FirstOrDefault();

        public static string GetUrlFileDestination(string filename) {
            var d = File.ReadAllText(filename, Encoding.UTF8).SplitAndCutByCRToList();
            return d.TagGet("URL");
        }

        public static void LaunchBrowser(string url) {
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

        public static void Pause(double sekunden, bool doEvents) {
            if (sekunden <= 0) { return; }
            if (!doEvents) {
                Thread.Sleep((int)(sekunden * 1000));
                return;
            }
            TimeSpan akTimer;
            var firstTimer = DateTime.Now;
            do {
                Develop.DoEvents();
                akTimer = DateTime.Now.Subtract(firstTimer);
            } while (akTimer.TotalSeconds < sekunden);
        }

        public static int PointOnScreenNr(System.Drawing.Point pointToCheck) {
            for (var zSc = 0; zSc <= System.Windows.Forms.Screen.AllScreens.GetUpperBound(0); zSc++) {
                if (pointToCheck.X >= System.Windows.Forms.Screen.AllScreens[zSc].Bounds.Left && pointToCheck.Y >= System.Windows.Forms.Screen.AllScreens[zSc].Bounds.Top && pointToCheck.X < System.Windows.Forms.Screen.AllScreens[zSc].Bounds.Right && pointToCheck.Y < System.Windows.Forms.Screen.AllScreens[zSc].Bounds.Bottom) {
                    return zSc;
                }
            }
            return 0;
        }

        public static Rectangle RectangleOfAllScreens() {
            var x1 = int.MaxValue;
            var y1 = int.MaxValue;
            var x2 = int.MinValue;
            var y2 = int.MinValue;
            for (var zSc = 0; zSc <= System.Windows.Forms.Screen.AllScreens.GetUpperBound(0); zSc++) {
                x1 = Math.Min(x1, System.Windows.Forms.Screen.AllScreens[zSc].Bounds.Left);
                y1 = Math.Min(y1, System.Windows.Forms.Screen.AllScreens[zSc].Bounds.Top);
                x2 = Math.Max(x2, System.Windows.Forms.Screen.AllScreens[zSc].Bounds.Right);
                y2 = Math.Max(y2, System.Windows.Forms.Screen.AllScreens[zSc].Bounds.Bottom);
            }
            System.Drawing.Point gp = new(x1, y1);
            System.Drawing.Size sz = new(-x1 + x2, -y1 + y2);
            return new Rectangle(gp, sz);
        }

        /// <summary>
        /// Gibt einen Wert zwischen 0 und 1 zurpück
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double Sigmoid(double x) => 1 / (1 + Math.Exp(-x));

        public static void Swap<T>(ref T w1, ref T w2) {
            var w3 = w1;
            w1 = w2;
            w2 = w3;
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
            if (!string.IsNullOrEmpty(_gotUserName)) { return _gotUserName; }
            _gotUserName = WindowsIdentity.GetCurrent().Name;
            if (_gotUserName.Contains("\\")) { _gotUserName = _gotUserName.FileNameWithSuffix(); }
            return _gotUserName;
        }

        #endregion
    }
}