// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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
using BlueBasics.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static BlueBasics.ClassesStatic.IO;

namespace BlueBasics.ClassesStatic;

public static class Develop {

    #region Fields

    public static bool AllReadOnly;
    public static DateTime LastUserActionUtc = new(1900, 1, 1);

    public static MessageDelegate? MessageDG;
    public static string MonitorMessage = "Monitor-Message";
    private static readonly DateTime ProgrammStarted = DateTime.UtcNow;
    private static readonly object SyncLockObject = new();
    private static string _currentTraceLogFile = string.Empty;
    private static bool _deleteTraceLog = true;
    private static ErrorType? _isTraceLogging;
    private static string _lastDebugMessage = string.Empty;
    private static DateTime _lastDebugTime = DateTime.UtcNow;
    private static TextWriterTraceListener? _traceListener;

    #endregion

    #region Delegates

    public delegate void MessageDelegate(ErrorType type, object? reference, string category, ImageCode symbol, string message, int indent);

    #endregion

    #region Properties

    [DefaultValue(false)]
    public static bool Exited { get; private set; }

    public static string OrigingLanguage { get; private set; } = "DE";

    public static string OrigingNumberDecimalSeparator { get; private set; } = ",";

    [DefaultValue(false)] private static bool ServiceStarted { get; set; }

    #endregion

    #region Methods

    public static void AbortAppIfStackOverflow() {
        var stackTrace = new StackTrace();
        if (stackTrace.FrameCount > 200) {
            DebugError("Stack-Overflow abgefangen!");
        }
    }

    [DoesNotReturn]
    public static void AbortExe(bool endtracelog) {
        try {
            if (endtracelog) {
                TraceLogging_End();
            }
        } catch { /* TraceLogging kann fehlschlagen, Programm wird sowieso beendet */ }

        Exited = true;
        // http://geekswithblogs.net/mtreadwell/archive/2004/06/06/6123.aspx
        Environment.Exit(1);
        Application.Exit();
    }

    public static string AppExe() => $"{AppPath()}{AppName()}.exe";

    public static string AppName() {
        try {
            var exA = Assembly.GetEntryAssembly();
            if (exA != null) {
                var name = exA.GetName().Name;
                if (name != null) {
                    return name;
                }
            }
        } catch { /* Assembly-Information nicht verfügbar */ }
        return "Programm von Christian Peter";
    }

    /// <summary>
    /// Den Pfad aus dem die Executable stammt, mit abschließenenden \
    /// </summary>
    /// <returns></returns>
    public static string AppPath() => AppDomain.CurrentDomain.BaseDirectory.NormalizePath();

    [DoesNotReturn]
    public static Exception DebugError(string message) {
        DebugPrint(ErrorType.Error, message);
        return new InvalidOperationException(message);
    }

    /// <summary>
    /// Gibt die Meldung Unbekannte Item aus
    /// </summary>
    /// <param name="warnung"></param>
    public static void DebugPrint(IHasKeyName warnung) => DebugPrint($"Unbekanntes Item: {warnung.KeyName}");

    public static void DebugPrint(string warnung) => DebugPrint(ErrorType.Warning, warnung);

    public static void DebugPrint(ErrorType art, string message, Exception ex) {
        if (art is not ErrorType.Info and not ErrorType.DevelopInfo && IsHostRunning()) { Debugger.Break(); }
        DebugPrint(art, message + "\r\nMeldung: " + ex.Message + "\r\n" + ex.StackTrace);
    }

    public static void DebugPrint(string message, Exception warnung) => DebugPrint(ErrorType.Warning, message, warnung);

    public static void DebugPrint<T>(T @enum) where T : Enum => DebugPrint("Ein Wert einer Enumeration konnte nicht verarbeitet werden.\r\nEnumeration: " + @enum.GetType().FullName + "\r\nParameter: " + @enum);

    public static void DebugPrint(ErrorType type, string message) {
        lock (SyncLockObject) {
            try {
                if (_isTraceLogging != null) {
                    if (_isTraceLogging == ErrorType.Error) { return; }
                    if (type != ErrorType.Error) { return; }
                }
                _isTraceLogging = type;
                if (type == ErrorType.Error) { _lastDebugMessage = string.Empty; }
                if (DateTime.UtcNow.Subtract(_lastDebugTime).TotalSeconds > 5) { _lastDebugMessage = string.Empty; }
                var net = type + (";" + message);
                if (net == _lastDebugMessage) {
                    _isTraceLogging = null;
                    return;
                }
                _lastDebugMessage = net;
                _lastDebugTime = DateTime.UtcNow;
                var first = true;
                var tmp = _currentTraceLogFile;
                var strace = new StackTrace(true);
                var nr = 100;
                List<string>? l = null;
                Trace.WriteLine("<tr>");

                switch (type) {
                    case ErrorType.DevelopInfo:
                        if (!IsHostRunning()) {
                            _isTraceLogging = null;
                            return;
                        }
                        Trace.WriteLine("<th><font size = 3>Runtime-Info");
                        nr = 5;
                        break;

                    case ErrorType.Info:
                        Trace.WriteLine("<th><font size = 3>Info");
                        MessageDG?.Invoke(type, null, "Info", ImageCode.Information, message, 0);
                        nr = 5;
                        break;

                    case ErrorType.Warning:
                        MessageDG?.Invoke(type, null, "Warnung", ImageCode.Warnung, message, 0);

                        if (IsHostRunning()) { Debugger.Break(); }
                        Trace.WriteLine("<th><font color =777700>Warnung<font color =000000>");
                        _deleteTraceLog = false;
                        break;

                    case ErrorType.Error:
                        MessageDG?.Invoke(type, null, "Fehler, Programmabbruch", ImageCode.Kritisch, message, 0);
                        if (IsHostRunning()) { Debugger.Break(); }
                        if (!FileExists(tmp)) { l = []; }
                        Trace.WriteLine("<th><font color =FF0000>Fehler<font color =000000>");
                        _deleteTraceLog = false;
                        break;

                    default:
                        MessageDG?.Invoke(type, null, "Info Unbekannten Typs", ImageCode.Sonne, message, 0);
                        Trace.WriteLine("<th>?");
                        _deleteTraceLog = false;
                        break;
                }
                Trace.WriteLine("<br>" + DateTime.UtcNow.ToString1() + " UTC<br>Thread-Id: " + Environment.CurrentManagedThreadId + "</th>");
                Trace.WriteLine("<th ALIGN=LEFT>");
                for (var z = 0; z <= Math.Min(nr + 2, strace.FrameCount - 2); z++) {
                    var frame = strace.GetFrame(z);
                    var method = frame?.GetMethod();
                    if (method == null) { continue; }
                    if (!method.Name.Contains(nameof(DebugPrint))) {
                        if (first) { Trace.WriteLine("<font color =0000FF>"); }
                        var methodInfo = method.ReflectedType?.FullName ?? string.Empty;
                        var methodStr = method.ToString()?.CreateHtmlCodes()?.TrimStart("Void ") ?? string.Empty;
                        Trace.WriteLine("<font size = 1>" + methodInfo.CreateHtmlCodes() + "<font size = 2> " + methodStr + "<br>");
                        l?.Add("<font size = 1>" + methodInfo.CreateHtmlCodes() + "<font size = 2> " + methodStr + "<br>");
                        if (first) { Trace.WriteLine("<font color =000000>"); }
                        first = false;
                    }
                }
                message = message.Replace("<br>", "\r", RegexOptions.IgnoreCase).CreateHtmlCodes();
                Trace.WriteLine("</th><th ALIGN=LEFT><font size = 3>" + message + "</th>");
                Trace.WriteLine("</tr>");
                if (type == ErrorType.Error) {
                    TraceLogging_End();
                    List<string> endl = [];
                    HTML_AddHead(endl, "Beenden...");
                    endl.Add("<center>");
                    endl.Add("<font size = 10>");
                    endl.Add("Entschuldigen Sie bitte...<br><font size =5><br>");
                    endl.Add("Das Programm <b>" + AppName() + "</b> musste beendet werden.<br>");
                    endl.Add("<br>");
                    endl.Add("<u><b>Grund:</u></b><br>");
                    endl.Add(message);
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
                    AbortExe(false);
                    return;
                }
                _isTraceLogging = null;
            } catch { /* TraceLogging-Bereinigung fehlerhaft, nicht kritisch */ }
        }
    }

    public static void Debugprint_BackgroundThread() {
        DebugPrint("Totes Fenster!");
    }

    public static void DebugPrint_InvokeRequired(bool invokeRequired, [DoesNotReturnIf(true)] bool doEnd) {
        if (!invokeRequired) { return; }
        if (IsHostRunning()) { Debugger.Break(); }

        DebugPrint(doEnd ? ErrorType.Error : ErrorType.Warning, "Es wird von einem Unterthread zugegriffen.");
    }

    public static void DebugPrint_MissingCommand(string command) => DebugPrint("Ein Wert einer Kontextmenü-Befehls konnte nicht verarbeitet werden.\r\nBefehl: " + command);

    public static void DebugPrint_NichtImplementiert([DoesNotReturnIf(true)] bool doEnd) => DebugPrint(doEnd ? ErrorType.Error : ErrorType.Warning,
            "Diese Funktion ist vom Entwickler noch nicht implementiert.");

    public static void DebugPrint_ReadOnly() => DebugPrint("Der Wert ist schreibgeschützt.");

    public static void DebugPrint_RoutineMussUeberschriebenWerden([DoesNotReturnIf(true)] bool doEnd) => DebugPrint(doEnd ? ErrorType.Error : ErrorType.Warning, "Diese Routine muss überschrieben werden.");

    public static void DoEvents() => Application.DoEvents();

    public static async Task<T?> GetSafePropertyValueAsync<T>(Func<T> propertyFunc) {
        if (propertyFunc == null) {
            DebugError("Die Eigenschaft darf nicht null sein.");
            return default;
        }

        try {
            var context = SynchronizationContext.Current;

            // Wenn wir bereits auf dem UI-Thread sind, direkt ausführen  }
            if (context is WindowsFormsSynchronizationContext) { return propertyFunc(); }

            // Nicht auf UI-Thread - zum UI-Thread marshallen
            if (context != null) {
                var tcs = new TaskCompletionSource<T?>();

                context.Post(_ => {
                    try {
                        var result = propertyFunc();
                        tcs.SetResult(result);
                    } catch (Exception ex) {
                        tcs.SetException(ex);
                    }
                }, null);

                return await tcs.Task;
            }

            // Fallback: Kein SynchronizationContext - in Background Thread ausführen
            return await Task.Run(propertyFunc);
        } catch (Exception ex) {
            DebugError($"Fehler beim Abrufen der Eigenschaft: {ex.Message}");
            return default;
        }
    }

    public static async Task InvokeAsync(Action action) {
        if (action == null) {
            DebugError("Die Action darf nicht null sein.");
            return;
        }

        try {
            var context = SynchronizationContext.Current;

            // Wenn wir bereits auf dem UI-Thread sind, direkt ausführen
            if (context is WindowsFormsSynchronizationContext) {
                action();
                return;
            }

            // Nicht auf UI-Thread - zum UI-Thread marshallen
            if (context != null) {
                var tcs = new TaskCompletionSource<bool>();

                context.Post(_ => {
                    try {
                        action();
                        tcs.SetResult(true);
                    } catch (Exception ex) {
                        tcs.SetException(ex);
                    }
                }, null);

                await tcs.Task;
                return;
            }

            // Fallback: Kein SynchronizationContext - in Background Thread ausführen
            await Task.Run(action);
        } catch (Exception ex) {
            DebugError($"Fehler beim Ausführen der Action: {ex.Message}");
        }
    }

    public static bool IsAllreadyRunning() {
        return Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).GetUpperBound(0) > 0;
    }

    public static bool IsHostRunning() => Debugger.IsAttached;

    public static void Message(ErrorType type, object? reference, string category, ImageCode symbol, string message, int indent) => MessageDG?.Invoke(type, reference, category, symbol, message, indent);

    public static void SetUserDidSomething() => LastUserActionUtc = DateTime.UtcNow;

    public static void StartService() {
        if (ServiceStarted) { return; }
        ServiceStarted = true;

        OrigingLanguage = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName.ToUpperInvariant();

        var info = (NumberFormatInfo)NumberFormatInfo.CurrentInfo.Clone();

        OrigingNumberDecimalSeparator = info.NumberDecimalSeparator;

        var ci = new CultureInfo("de-DE") {
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
        } catch { /* SetCompatibleTextRenderingDefault kann im Designmode fehlschlagen */ }

        TraceLogging_Start(TempFile(string.Empty, AppName() + "-Trace.html"));

        Generic.LoadAllAssemblies(AppPath());

        _ = new System.Threading.Timer(_ => CloseAfter12Hours(), null, 60000, 60000);
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

                if (_deleteTraceLog && FileExists(_currentTraceLogFile)) { DeleteFile(_currentTraceLogFile, false); }
            }
        } catch { /* TraceLogging_End: Fehler beim Schliessen des TraceListeners */ }
        _currentTraceLogFile = string.Empty;
        _deleteTraceLog = true;
    }

    public static void TraceLogging_Start(string traceFileName) {
        TraceLogging_End();
        _deleteTraceLog = true;
        if (FileExists(_currentTraceLogFile)) { DeleteFile(_currentTraceLogFile, false); }
        _currentTraceLogFile = TempFile(traceFileName);
        _traceListener = new TextWriterTraceListener(_currentTraceLogFile);
        Trace.Listeners.Add(_traceListener);
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
        } catch { /* TraceLogging_Start: Fehler beim Schreiben des HTML-Headers */ }
    }

    private static void CloseAfter12Hours() {
        if (DateTime.UtcNow.Subtract(ProgrammStarted).TotalHours > 12) {
            if (IsHostRunning()) { return; }
            DebugPrint(ErrorType.Info, "Das Programm wird nach 12 Stunden automatisch geschlossen.");
            AbortExe(true);
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