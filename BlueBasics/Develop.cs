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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using static BlueBasics.FileOperations;

namespace BlueBasics {
    public static class Develop {
        private static readonly object _SyncLockObject = new();

        private static string _LastDebugMessage = string.Empty;
        private static DateTime _LastDebugTime = DateTime.Now;
        private static string _CurrentTraceLogFile = string.Empty;
        private static TextWriterTraceListener _TraceListener;
        private static bool _DeleteTraceLog = true;
        private static bool _IsTraceLogging;
        private static readonly DateTime _ProgrammStarted = DateTime.Now;
        private static DateTime _StopUhr = DateTime.Now;

        [DefaultValue(false)]
        public static bool Exited { get; private set; } = false;

        [DefaultValue(false)]
        public static bool ServiceStarted { get; private set; } = false;

        public static bool IsHostRunning() => Debugger.IsAttached;

        public static void TraceLogging_End() {
            try {
                if (!string.IsNullOrEmpty(_CurrentTraceLogFile)) {
                    // Trace-Log soll umgeleitet werden
                    Trace.WriteLine("    </table>");
                    Trace.WriteLine("  </body>");
                    Trace.WriteLine(" </html>");
                    Trace.Listeners.Remove(_TraceListener);
                    _TraceListener.Flush();
                    _TraceListener.Close();
                    _TraceListener.Dispose();
                    _TraceListener = null;
                    if (_DeleteTraceLog && FileExists(_CurrentTraceLogFile)) { DeleteFile(_CurrentTraceLogFile, false); }
                }
            } catch {
            }

            _CurrentTraceLogFile = string.Empty;
            _DeleteTraceLog = true;
        }

        public static void TraceLogging_Start(string traceFileName) {
            TraceLogging_End();
            _DeleteTraceLog = true;
            if (FileExists(_CurrentTraceLogFile)) { File.Delete(_CurrentTraceLogFile); }

            _CurrentTraceLogFile = TempFile(traceFileName);
            _TraceListener = new TextWriterTraceListener(_CurrentTraceLogFile);
            Trace.Listeners.Add(_TraceListener);
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

        public static void AbortExe() {
            Exited = true;
            // http://geekswithblogs.net/mtreadwell/archive/2004/06/06/6123.aspx
            Environment.Exit(-1);
            System.Windows.Forms.Application.Exit();
        }

        public static string AppName() {
            try {
                var ex_a = Assembly.GetEntryAssembly();
                return ex_a.GetName().Name;
            } catch {
                return "Programm von Christian Peter";
            }
        }

        public static string AppExe() => System.Windows.Forms.Application.StartupPath + "\\" + AppName() + ".exe";

        public static void HTML_AddFoot(List<string> l) {
            l.Add("  </body>");
            l.Add("</html>");
        }

        public static void HTML_AddHead(List<string> l, string title) {
            l.Add("<!DOctypex HTML PUBLIC \"-//W3C//DTD HTML 4.01//EN\"");
            l.Add("\"http://www.w3.org/TR/html4/strict.dtd\">");
            l.Add("<html>");
            l.Add("  <head>");
            l.Add("    <title>" + title + "</title>");
            l.Add("  </head>");
            l.Add("<body>");
        }

        public static void DebugPrint(string warnung) => DebugPrint(enFehlerArt.Warnung, warnung);

        public static void DebugPrint(enFehlerArt art, Exception ex) {
            if (art != enFehlerArt.Info && art != enFehlerArt.DevelopInfo && IsHostRunning()) { Debugger.Break(); }
            DebugPrint(art, "Es wurde ein allgemeiner Fehler abgefangen.\r\nMeldung: " + ex.Message + "\r\n" + ex.StackTrace.ToString());
        }

        public static void DebugPrint(Exception warnung) => DebugPrint(enFehlerArt.Warnung, warnung);

        public static void DebugPrint_RoutineMussUeberschriebenWerden() {
            if (IsHostRunning()) { Debugger.Break(); }
            DebugPrint(enFehlerArt.Warnung, "Diese Funktion muss noch überschrieben werden.");
        }

        public static void DebugPrint_NichtImplementiert() {
            if (IsHostRunning()) { Debugger.Break(); }
            DebugPrint(enFehlerArt.Fehler, "Diese Funktion ist vom Entwickler noch nicht implementiert.");
        }

        public static void DebugPrint(object _Enum) => DebugPrint(enFehlerArt.Warnung, "Ein Wert einer Enumeration konnte nicht verarbeitet werden.\r\nEnumeration: " + _Enum.GetType().FullName + "\r\nParameter: " + _Enum);

        public static void DebugPrint_MissingCommand(string comand) => DebugPrint(enFehlerArt.Warnung, "Ein Wert einer Kontextmenü-Befehls konnte nicht verarbeitet werden.\r\nBefehl: " + comand);

        public static void DebugPrint(enFehlerArt art, string meldung) {
            lock (_SyncLockObject) {
                try {
                    if (_IsTraceLogging) {
                        if (art == enFehlerArt.Fehler) { AbortExe(); }
                        return;
                    }
                    _IsTraceLogging = true;

                    if (art == enFehlerArt.Fehler) { _LastDebugMessage = string.Empty; }

                    if (DateTime.Now.Subtract(_LastDebugTime).TotalSeconds > 5) { _LastDebugMessage = string.Empty; }

                    var net = art + (";" + meldung);
                    if (net == _LastDebugMessage) {
                        _IsTraceLogging = false;
                        return;
                    }

                    _LastDebugMessage = net;
                    _LastDebugTime = DateTime.Now;
                    var First = true;
                    var tmp = _CurrentTraceLogFile;
                    var strace = new StackTrace(true);
                    var Nr = 100;
                    List<string> l = null;
                    Trace.WriteLine("<tr>");

                    switch (art) {
                        case enFehlerArt.DevelopInfo:
                            if (!IsHostRunning()) {
                                _IsTraceLogging = false;
                                return;
                            }

                            Trace.WriteLine("<th><font size = 3>Runtime-Info");
                            Nr = 5;
                            break;

                        case enFehlerArt.Info:
                            Trace.WriteLine("<th><font size = 3>Info");
                            Nr = 5;
                            break;

                        case enFehlerArt.Warnung:
                            if (IsHostRunning()) { Debugger.Break(); }

                            Trace.WriteLine("<th><font color =777700>Warnung<font color =000000>");
                            _DeleteTraceLog = false;
                            break;

                        case enFehlerArt.Fehler:
                            if (IsHostRunning()) { Debugger.Break(); }

                            if (!FileExists(tmp)) { l = new List<string>(); }

                            Trace.WriteLine("<th><font color =FF0000>Fehler<font color =000000>");
                            _DeleteTraceLog = false;
                            break;

                        default:
                            Trace.WriteLine("<th>?");
                            _DeleteTraceLog = false;
                            break;
                    }

                    Trace.WriteLine("<br>" + DateTime.Now.ToString(Constants.Format_Date) + "<br>Thread-Id: " + Thread.CurrentThread.ManagedThreadId + "</th>");
                    Trace.WriteLine("<th ALIGN=LEFT>");
                    for (var z = 0; z <= Math.Min(Nr + 2, strace.FrameCount - 2); z++) {
                        if (!strace.GetFrame(z).GetMethod().Name.Contains("DebugPrint")) {
                            if (First) { Trace.WriteLine("<font color =0000FF>"); }
                            Trace.WriteLine("<font size = 1>" + strace.GetFrame(z).GetMethod().ReflectedType.FullName.CreateHtmlCodes(true) + "<font size = 2> " + strace.GetFrame(z).GetMethod().ToString().CreateHtmlCodes(true).TrimStart("Void ") + "<br>");

                            l?.Add("<font size = 1>" + strace.GetFrame(z).GetMethod().ReflectedType.FullName.CreateHtmlCodes(true) + "<font size = 2> " + strace.GetFrame(z).GetMethod().ToString().CreateHtmlCodes(true).TrimStart("Void ") + "<br>");

                            if (First) { Trace.WriteLine("<font color =000000>"); }
                            First = false;
                        }
                    }

                    meldung = meldung.Replace("<br>", "\r", RegexOptions.IgnoreCase).CreateHtmlCodes(true);
                    Trace.WriteLine("</th><th ALIGN=LEFT><font size = 3>" + meldung + "</th>");

                    Trace.WriteLine("</tr>");
                    if (art == enFehlerArt.Fehler) {
                        TraceLogging_End();
                        var endl = new List<string>();
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
                            endl.AddRange(l);
                        }

                        HTML_AddFoot(endl);
                        endl.Save(TempFile(string.Empty, "Endmeldung", "html"), true, System.Text.Encoding.UTF8);
                        AbortExe();
                        return;
                    }

                    _IsTraceLogging = false;
                } catch { }
            }
        }

        public static bool IsRunning() => Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).GetUpperBound(0) > 0;

        public static void DoEvents() => System.Windows.Forms.Application.DoEvents();

        public static void Debugprint_BackgroundThread() {
            if (!Thread.CurrentThread.IsBackground) { return; }

            DebugPrint(enFehlerArt.Warnung, "Totes Fenster!");
        }

        public static void DebugPrint_Disposed(bool disposedValue) {
            if (!disposedValue) { return; }
            if (IsHostRunning()) { Debugger.Break(); }
            DebugPrint(enFehlerArt.Fehler, "Das Objekt wurde zur Laufzeit verworfen.");
        }

        public static void DebugPrint_InvokeRequired(bool invokeRequired, bool fehler) {
            if (!invokeRequired) { return; }
            if (IsHostRunning()) { Debugger.Break(); }

            if (fehler) {
                DebugPrint(enFehlerArt.Fehler, "Es wird von einem Unterthread zugegriffen.");
            } else {
                DebugPrint(enFehlerArt.Warnung, "Es wird von einem Unterthread zugegriffen.");
            }
        }

        public static void CheckStackForOverflow() {
            var stackTrace = new StackTrace();
            if (stackTrace.GetFrames().GetUpperBound(0) > 300) {
                DebugPrint(enFehlerArt.Fehler, "Stack-Overflow abgefangen!");
            }
        }

        public static void StartService() {
            if (ServiceStarted) { return; }

            ServiceStarted = true;
            var ci = new CultureInfo("de-DE");
            ci.NumberFormat.CurrencyGroupSeparator = string.Empty;
            ci.NumberFormat.NumberGroupSeparator = string.Empty;
            ci.NumberFormat.PercentGroupSeparator = string.Empty;
            ci.NumberFormat.NumberDecimalSeparator = ",";
            System.Windows.Forms.Application.CurrentCulture = ci;
            CultureInfo.DefaultThreadCurrentCulture = ci;
            CultureInfo.DefaultThreadCurrentUICulture = ci;
            TraceLogging_Start(TempFile(string.Empty, AppName() + "-Trace.html"));

            var Check = new System.Windows.Forms.Timer();
            Check.Tick += CloseAfter12Hours;
            Check.Interval = 60000;
            Check.Enabled = true;
        }

        private static void CloseAfter12Hours(object sender, System.EventArgs e) {
            if (DateTime.Now.Subtract(_ProgrammStarted).TotalHours > 12) {
                if (IsHostRunning()) { return; }
                DebugPrint(enFehlerArt.Info, "Das Programm wird nach 12 Stunden automatisch geschlossen.");
                TraceLogging_End();
                AbortExe();
            }
        }

        /// <summary>
        /// Schreibt die vergangene Zeit in MS in die Konsole.
        /// Wir kein Text angegeben, wird nur die Zeit zurückgesetzt, ohne einer Ausgabe.
        /// </summary>
        /// <param name="txt"></param>
        public static void StopUhr(string txt) {
            if (!string.IsNullOrEmpty(txt)) {
                var x = DateTime.Now.Subtract(_StopUhr);
                Console.WriteLine("### STOPUHR ### " + x.TotalMilliseconds.ToString(Constants.Format_Float5_1) + " ms ----> " + txt);
            }

            _StopUhr = DateTime.Now;
        }
    }
}
