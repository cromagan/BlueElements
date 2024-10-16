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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using static BlueBasics.IO;

namespace BlueBasics;

public static class Develop {

    #region Fields

    public static bool AllReadOnly = false;
    public static DateTime LastUserActionUtc = new(1900, 1, 1);

    private static readonly DateTime ProgrammStarted = DateTime.UtcNow;

    private static readonly object SyncLockObject = new();

    private static string _currentTraceLogFile = string.Empty;

    private static bool _deleteTraceLog = true;

    private static bool _isTraceLogging;

    private static string _lastDebugMessage = string.Empty;

    private static DateTime _lastDebugTime = DateTime.UtcNow;

    private static TextWriterTraceListener? _traceListener;

    #endregion

    #region Properties

    [DefaultValue(false)]
    public static bool Exited { get; private set; }

    public static string OrigingLanguage { get; private set; } = "DE";

    public static string OrigingNumberDecimalSeparator { get; private set; } = ",";

    [DefaultValue(false)]
    public static bool ServiceStarted { get; private set; }

    #endregion

    #region Methods

    public static void AbortExe() {
        Exited = true;
        // http://geekswithblogs.net/mtreadwell/archive/2004/06/06/6123.aspx
        Environment.Exit(-1);
        Application.Exit();
    }

    public static string AppExe() => Application.StartupPath + "\\" + AppName() + ".exe";

    public static string AppName() {
        try {
            var exA = Assembly.GetEntryAssembly();
            if (exA != null) {
                return exA.GetName().Name;
            }
        } catch { }
        return "Programm von Christian Peter";
    }

    public static void CheckStackForOverflow() {
        StackTrace stackTrace = new();
        if (stackTrace.FrameCount > 100) {
            DebugPrint(FehlerArt.Fehler, "Stack-Overflow abgefangen!");
        }
    }

    /// <summary>
    /// Gibt die Meldung Unbekannte Item aus
    /// </summary>
    /// <param name="warnung"></param>
    public static void DebugPrint(IHasKeyName warnung) => DebugPrint(FehlerArt.Warnung, "Unbekanntes Item:" + warnung.KeyName);

    public static void DebugPrint(string warnung) => DebugPrint(FehlerArt.Warnung, warnung);

    public static void DebugPrint(FehlerArt art, string message, Exception ex) {
        if (art is not FehlerArt.Info and not FehlerArt.DevelopInfo && IsHostRunning()) { Debugger.Break(); }
        DebugPrint(art, message + "\r\nMeldung: " + ex.Message + "\r\n" + ex.StackTrace);
    }

    public static void DebugPrint(string message, Exception warnung) => DebugPrint(FehlerArt.Warnung, message, warnung);

    public static void DebugPrint<T>(T @enum) where T : Enum => DebugPrint(FehlerArt.Warnung, "Ein Wert einer Enumeration konnte nicht verarbeitet werden.\r\nEnumeration: " + @enum.GetType().FullName + "\r\nParameter: " + @enum);

    public static void DebugPrint(FehlerArt art, string meldung) {
        lock (SyncLockObject) {
            try {
                if (_isTraceLogging) {
                    if (art == FehlerArt.Fehler) { AbortExe(); }
                    return;
                }
                _isTraceLogging = true;
                if (art == FehlerArt.Fehler) { _lastDebugMessage = string.Empty; }
                if (DateTime.UtcNow.Subtract(_lastDebugTime).TotalSeconds > 5) { _lastDebugMessage = string.Empty; }
                var net = art + (";" + meldung);
                if (net == _lastDebugMessage) {
                    _isTraceLogging = false;
                    return;
                }
                _lastDebugMessage = net;
                _lastDebugTime = DateTime.UtcNow;
                var first = true;
                var tmp = _currentTraceLogFile;
                StackTrace strace = new(true);
                var nr = 100;
                List<string>? l = null;
                Trace.WriteLine("<tr>");
                switch (art) {
                    case FehlerArt.DevelopInfo:
                        if (!IsHostRunning()) {
                            _isTraceLogging = false;
                            return;
                        }
                        Trace.WriteLine("<th><font size = 3>Runtime-Info");
                        nr = 5;
                        break;

                    case FehlerArt.Info:
                        Trace.WriteLine("<th><font size = 3>Info");
                        nr = 5;
                        break;

                    case FehlerArt.Warnung:
                        if (IsHostRunning()) { Debugger.Break(); }
                        Trace.WriteLine("<th><font color =777700>Warnung<font color =000000>");
                        _deleteTraceLog = false;
                        break;

                    case FehlerArt.Fehler:
                        if (IsHostRunning()) { Debugger.Break(); }
                        if (!FileExists(tmp)) { l = []; }
                        Trace.WriteLine("<th><font color =FF0000>Fehler<font color =000000>");
                        _deleteTraceLog = false;
                        break;

                    default:
                        Trace.WriteLine("<th>?");
                        _deleteTraceLog = false;
                        break;
                }
                Trace.WriteLine("<br>" + DateTime.UtcNow.ToString1() + " UTC<br>Thread-Id: " + Thread.CurrentThread.ManagedThreadId + "</th>");
                Trace.WriteLine("<th ALIGN=LEFT>");
                for (var z = 0; z <= Math.Min(nr + 2, strace.FrameCount - 2); z++) {
                    if (!strace.GetFrame(z).GetMethod().Name.Contains("DebugPrint")) {
                        if (first) { Trace.WriteLine("<font color =0000FF>"); }
                        Trace.WriteLine("<font size = 1>" + strace.GetFrame(z).GetMethod().ReflectedType.FullName.CreateHtmlCodes(true) + "<font size = 2> " + strace.GetFrame(z).GetMethod().ToString().CreateHtmlCodes(true).TrimStart("Void ") + "<br>");
                        l?.Add("<font size = 1>" + strace.GetFrame(z).GetMethod().ReflectedType.FullName.CreateHtmlCodes(true) + "<font size = 2> " + strace.GetFrame(z).GetMethod().ToString().CreateHtmlCodes(true).TrimStart("Void ") + "<br>");
                        if (first) { Trace.WriteLine("<font color =000000>"); }
                        first = false;
                    }
                }
                meldung = meldung.Replace("<br>", "\r", RegexOptions.IgnoreCase).CreateHtmlCodes(true);
                Trace.WriteLine("</th><th ALIGN=LEFT><font size = 3>" + meldung + "</th>");
                Trace.WriteLine("</tr>");
                if (art == FehlerArt.Fehler) {
                    TraceLogging_End();
                    List<string> endl = [];
                    HTML_AddHead(endl, "Beenden...");
                    endl.Add("<center>");
                    endl.Add("<font size = 10>");
                    endl.Add("Entschuldigen Sie bitte...<br><font size =5><br>");
                    endl.Add("Das Programm <b>" + AppName() + "</b> musste beendet werden.<br>");
                    endl.Add("<br>");
                    endl.Add("<u><b>Grund:</u></b><br>");
                    endl.Add(meldung);
                    endl.Add("<br>");
                    endl.Add("<font size = 1>");
                    endl.Add("Datum: " + DateTime.Now);
                    if (FileExists(tmp)) {
                        endl.Add("<br>");
                        endl.Add("<a href=\"file://" + tmp + "\">Tracelog öffnen</a>");
                    } else {
                        if (l != null) { endl.AddRange(l); }
                    }
                    HTML_AddFoot(endl);
                    endl.WriteAllText(TempFile(string.Empty, "Endmeldung", "html"), Encoding.UTF8, true);
                    AbortExe();
                    return;
                }
                _isTraceLogging = false;
            } catch { }
        }
    }

    public static void Debugprint_BackgroundThread() {
        if (!Thread.CurrentThread.IsBackground) { return; }
        DebugPrint(FehlerArt.Warnung, "Totes Fenster!");
    }

    public static void DebugPrint_InvokeRequired(bool invokeRequired, bool fehler) {
        if (!invokeRequired) { return; }
        if (IsHostRunning()) { Debugger.Break(); }

        DebugPrint(fehler ? FehlerArt.Fehler : FehlerArt.Warnung, "Es wird von einem Unterthread zugegriffen.");
    }

    public static void DebugPrint_MissingCommand(string command) => DebugPrint(FehlerArt.Warnung, "Ein Wert einer Kontextmenü-Befehls konnte nicht verarbeitet werden.\r\nBefehl: " + command);

    public static void DebugPrint_NichtImplementiert(bool doend) {
        DebugPrint(doend ? FehlerArt.Fehler : FehlerArt.Warnung,
            "Diese Funktion ist vom Entwickler noch nicht implementiert.");
    }

    public static void DebugPrint_ReadOnly() => DebugPrint(FehlerArt.Warnung, "Der Wert ist schreibgeschützt.");

    public static void DebugPrint_RoutineMussUeberschriebenWerden(bool doend) {
        DebugPrint(doend ? FehlerArt.Fehler : FehlerArt.Warnung, "Diese Routine muss überschrieben werden.");
    }

    public static void DoEvents() => Application.DoEvents();

    public static bool IsAllreadyRunning() => Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).GetUpperBound(0) > 0;

    public static bool IsHostRunning() => Debugger.IsAttached;

    public static void SetUserDidSomething() => LastUserActionUtc = DateTime.UtcNow;

    public static void StartService() {
        if (ServiceStarted) { return; }
        ServiceStarted = true;

        OrigingLanguage = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName.ToUpperInvariant();

        var info = (NumberFormatInfo)NumberFormatInfo.CurrentInfo.Clone();

        OrigingNumberDecimalSeparator = info.NumberDecimalSeparator;

        CultureInfo ci = new("de-DE") {
            NumberFormat = {
                CurrencyGroupSeparator = string.Empty,
                NumberGroupSeparator = string.Empty,
                PercentGroupSeparator = string.Empty,
                NumberDecimalSeparator = ","
            }
        };
        Application.CurrentCulture = ci;
        CultureInfo.DefaultThreadCurrentCulture = ci;
        CultureInfo.DefaultThreadCurrentUICulture = ci;
        Application.EnableVisualStyles();

        try {
            // Try Block erforderlich, weil im der Designmode ab und zu abstürzt
            Application.SetCompatibleTextRenderingDefault(false);
        } catch { }

        TraceLogging_Start(TempFile(string.Empty, AppName() + "-Trace.html"));

        Generic.LoadAllAssemblies(Application.StartupPath);

        System.Windows.Forms.Timer check = new();
        check.Tick += CloseAfter12Hours;
        check.Interval = 60000;
        check.Enabled = true;
    }

    public static void TraceLogging_End() {
        try {
            if (!string.IsNullOrEmpty(_currentTraceLogFile)) {
                // Trace-Log soll umgeleitet werden
                Trace.WriteLine("    </table>");
                Trace.WriteLine("  </body>");
                Trace.WriteLine(" </html>");

                if (_traceListener != null) {
                    Trace.Listeners.Remove(_traceListener);
                    _traceListener.Flush();
                    _traceListener.Close();
                    _traceListener.Dispose();
                    _traceListener = null;
                }

                if (_deleteTraceLog && FileExists(_currentTraceLogFile)) { _ = DeleteFile(_currentTraceLogFile, false); }
            }
        } catch { }
        _currentTraceLogFile = string.Empty;
        _deleteTraceLog = true;
    }

    public static void TraceLogging_Start(string traceFileName) {
        TraceLogging_End();
        _deleteTraceLog = true;
        if (FileExists(_currentTraceLogFile)) { File.Delete(_currentTraceLogFile); }
        _currentTraceLogFile = TempFile(traceFileName);
        _traceListener = new TextWriterTraceListener(_currentTraceLogFile);
        _ = Trace.Listeners.Add(_traceListener);
        try {
            Trace.AutoFlush = true;
            Trace.WriteLine("<!DOctypex HTML PUBLIC \"-//W3C//DTD HTML 4.01//EN\"");
            Trace.WriteLine("\"http://www.w3.org/TR/html4/strict.dtd\">");
            Trace.WriteLine("<html>");
            Trace.WriteLine("  <head>");
            Trace.WriteLine("    <title>Tracelog</title>");
            Trace.WriteLine("  </head>");
            Trace.WriteLine("<body>");
            Trace.WriteLine("  <Font face=\"Arial\" Size=\"2\">");
            Trace.WriteLine("  <table border=\"1\" cellspacing=\"1\" cellpadding=\"1\" align=\"left\">");
        } catch { }
    }

    private static void CloseAfter12Hours(object sender, System.EventArgs e) {
        if (DateTime.UtcNow.Subtract(ProgrammStarted).TotalHours > 12) {
            if (IsHostRunning()) { return; }
            DebugPrint(FehlerArt.Info, "Das Programm wird nach 12 Stunden automatisch geschlossen.");
            TraceLogging_End();
            AbortExe();
        }
    }

    private static void HTML_AddFoot(List<string> l) {
        l.Add("  </body>");
        l.Add("</html>");
    }

    private static void HTML_AddHead(List<string> l, string title) {
        l.Add("<!DOctypex HTML PUBLIC \"-//W3C//DTD HTML 4.01//EN\"");
        l.Add("\"http://www.w3.org/TR/html4/strict.dtd\">");
        l.Add("<html>");
        l.Add("  <head>");
        l.Add("    <title>" + title + "</title>");
        l.Add("  </head>");
        l.Add("<body>");
    }

    #endregion
}