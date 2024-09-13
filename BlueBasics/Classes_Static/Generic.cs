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

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using static BlueBasics.IO;
using Clipboard = System.Windows.Clipboard;
using Point = System.Drawing.Point;

namespace BlueBasics;

public static class Generic {

    #region Fields

    public static string UserGroup = Constants.Everybody;

    private static int _getUniqueKeyCount;

    private static string _getUniqueKeyLastTime = "InitialDummy";

    private static string _gotUserName = string.Empty;

    #endregion

    #region Properties

    public static string UserName {
        get {
            if (!string.IsNullOrEmpty(_gotUserName)) { return _gotUserName; }
            _gotUserName = WindowsIdentity.GetCurrent().Name;
            if (_gotUserName.Contains("\\")) { _gotUserName = _gotUserName.FileNameWithSuffix(); }
            return _gotUserName;
        }
        set => _gotUserName = value;
    }

    #endregion

    #region Methods

    public static void CollectGarbage() {
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
        //GC.WaitForPendingFinalizers();
        //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
        _ = GC.WaitForFullGCComplete(1000);
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

    /// <summary>
    /// Erstellt eine URL-Verknüpfung, die im Explorer mittels Click geöffnet werden lann
    /// </summary>
    /// <param name="saveTo"></param>
    /// <param name="linkUrl"></param>
    /// <returns></returns>
    // ReSharper disable once UnusedMember.Global
    public static bool CreateInternetLink(string saveTo, string linkUrl) {
        var title = "unbekannt";
        // string deskDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        try {
            using WebClient x = new();
            var source = x.DownloadString(linkUrl);
            title = Regex.Match(source, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"].Value;
            title = title.RemoveChars(Constants.Char_DateiSonderZeichen);
        } catch { }

        title = title.ReduceToChars(Constants.Char_Buchstaben + Constants.Char_Buchstaben.ToUpperInvariant() + "!.,()+-_ " + Constants.Char_Numerals);
        using StreamWriter writer = new(TempFile(saveTo.TrimEnd("\\") + "\\" + title + ".url"));
        writer.WriteLine("[InternetShortcut]");
        writer.WriteLine("URL=" + linkUrl);
        writer.Flush();
        return true;
    }

    /// <summary>
    /// Erstellt eine Datei-Verknüpfung, die im Explorer mittels Click geöffnet werden lann
    /// </summary>
    /// <returns></returns>
    // ReSharper disable once UnusedMember.Global
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
            Develop.DebugPrint("Fehler beim Shortcut anlegen", ex);
            return false;
        }
    }

    public static string Download(string url) {
        // My.Computer.Network.DownloadFile("http://.png", "C:\TMP\a.png")
        using WebClient wc = new();
        wc.Encoding = Encoding.UTF8;
        return wc.DownloadString(url);
    }

    public static Image? DownloadImage(string url) {
        var request = WebRequest.Create(url);
        var response = request.GetResponse();
        var remoteStream = response.GetResponseStream();
        if (remoteStream != null) {
            StreamReader readStream = new(remoteStream);
            var img = Image.FromStream(remoteStream);
            response.Close();
            remoteStream.Close();

            readStream.Close();
            return img;
        }

        return null;
    }

    public static Stream? GetEmmbedResource(Assembly? assembly, string name) {
        if (assembly == null) { return null; }
        if (string.IsNullOrEmpty(name)) { return null; }

        return assembly.GetManifestResourceNames()
            .Where(thisString => thisString.EndsWith("." + name))
            .Select(assembly.GetManifestResourceStream).FirstOrDefault();
    }

    public static List<Type> GetEnumerableOfType<T>() where T : class {
        List<Type> l = [];
        foreach (var thisas in AppDomain.CurrentDomain.GetAssemblies()) {
            try {
                foreach (var thist in thisas.GetTypes()) {
                    if (thist is { IsClass: true, IsAbstract: false } && thist.IsSubclassOf(typeof(T))) {
                        l.Add(thist);
                    }
                }
            } catch { }
        }
        return l;
    }

    public static string GetHashString(this string? inputString) {
        if (inputString is { }) {
            var sb = new StringBuilder();
            foreach (var b in GetHash(inputString)) {
                _ = sb.Append(b.ToString("X2"));
            }

            return sb.ToString();
        }
        return string.Empty;
    }

    public static List<T> GetInstaceOfType<T>(params object?[] constructorArgs) where T : class {
        List<T> l = [];
        foreach (var thisas in AppDomain.CurrentDomain.GetAssemblies()) {
            try {
                foreach (var thist in thisas.GetTypes()) {
                    try {
                        if (thist is { IsClass: true, IsAbstract: false } && typeof(T).IsAssignableFrom(thist) && HasMatchingConstructor(thist, constructorArgs)) {
                            l.Add((T)Activator.CreateInstance(thist, constructorArgs));
                        }
                    } catch { }
                }
            } catch { }
        }
        return l;
    }

    public static string GetUniqueKey(int tmp, string type) {
        var x = DateTime.UtcNow.AddYears(-2020).Ticks;
        var s = type + "\r\n" + UserName + "\r\n" + Thread.CurrentThread.ManagedThreadId + "\r\n" + Environment.MachineName;
        var key = x + (s.GetHashCode() * 100000000) + tmp;
        return key < 0 ? (key * -1).ToString() : key.ToString();
    }

    public static string GetUniqueKey() {
        // Hashtag: UniqueInternal
        var neueZeit = DateTime.UtcNow.ToString7().ReduceToChars(Constants.Char_Numerals);
        if (neueZeit == _getUniqueKeyLastTime) {
            _getUniqueKeyCount++;
        } else {
            _getUniqueKeyCount = 0;
            _getUniqueKeyLastTime = neueZeit;
        }
        return "ID_" + neueZeit + "_" + _getUniqueKeyCount.ToStringInt3();
    }

    public static bool IsAdministrator() => string.Equals(UserGroup, Constants.Administrator, StringComparison.OrdinalIgnoreCase);

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
        _ = Process.Start(new ProcessStartInfo(browserName, adds + " " + url));
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

    public static void LoadAllAssemblies(string assemblyDirectory) {
        // Alle Dateien mit der Erweiterung ".dll" im Verzeichnis abrufen
        var assemblyFiles = Directory.GetFiles(assemblyDirectory, "*.dll");

        // Alle Assemblys laden und instanziieren
        foreach (var assemblyFile in assemblyFiles) {
            try {
                _ = Assembly.LoadFrom(assemblyFile);
                //Console.WriteLine(assembly.FullName);
            } catch {
                //Console.WriteLine($"Fehler beim Laden der Assembly: {ex.Message}");
            }
        }
    }

    public static void Pause(double seconds, bool allowEvents) {
        if (seconds <= 0) { return; }

        var endTicks = Environment.TickCount + (long)(seconds * 1000);

        while (Environment.TickCount < endTicks) {
            if (allowEvents) { Develop.DoEvents(); }
        }
    }

    public static int PointOnScreenNr(Point pointToCheck) {
        for (var zSc = 0; zSc <= Screen.AllScreens.GetUpperBound(0); zSc++) {
            if (pointToCheck.X >= Screen.AllScreens[zSc].Bounds.Left && pointToCheck.Y >= Screen.AllScreens[zSc].Bounds.Top && pointToCheck.X < Screen.AllScreens[zSc].Bounds.Right && pointToCheck.Y < Screen.AllScreens[zSc].Bounds.Bottom) {
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
        for (var zSc = 0; zSc <= Screen.AllScreens.GetUpperBound(0); zSc++) {
            x1 = Math.Min(x1, Screen.AllScreens[zSc].Bounds.Left);
            y1 = Math.Min(y1, Screen.AllScreens[zSc].Bounds.Top);
            x2 = Math.Max(x2, Screen.AllScreens[zSc].Bounds.Right);
            y2 = Math.Max(y2, Screen.AllScreens[zSc].Bounds.Bottom);
        }
        Point gp = new(x1, y1);
        Size sz = new(-x1 + x2, -y1 + y2);
        return new Rectangle(gp, sz);
    }

    public static void Swap<T>(ref T w1, ref T w2) => (w1, w2) = (w2, w1);

    private static IEnumerable<byte> GetHash(this string inputString) {
        using HashAlgorithm algorithm = SHA256.Create();
        return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
    }

    private static bool HasMatchingConstructor(Type type, object?[] constructorArgs) {
        // Wenn keine Konstruktorargumente bereitgestellt werden, suchen Sie nach einem parameterlosen Konstruktor
        if (constructorArgs.Length == 0) {
            return type.GetConstructor(Type.EmptyTypes) != null;
        }

        // Überprüfen Sie, ob ein Konstruktor existiert, der den Typen der bereitgestellten Argumente entspricht
        var constructorInfo = type.GetConstructor(
            constructorArgs.Select(arg => arg.GetType()).ToArray()
        );

        return constructorInfo != null;
    }

    #endregion
}