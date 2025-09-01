// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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

using BlueBasics.Enums;
using BlueBasics.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static BlueBasics.Generic;

namespace BlueBasics;

// ReSharper disable once InconsistentNaming
public static class IO {

    #region Fields

    public static string LastFilePath = string.Empty;
    private const int _fileExistenceCheckRetryCount = 20;
    private static readonly ConcurrentDictionary<string, (DateTime CheckTime, bool Result)> _canWriteCache = new();
    private static readonly object _fileOperationLock = new();

    #endregion

    #region Delegates

    public delegate (object? returnValue, bool retry) DoThis(params object[] args);

    #endregion

    #region Methods

    /// <summary>
    /// Prüft, ob eine Datei gespeichert werden kann, basierend auf Dateizugriff und zeitlichen Beschränkungen
    /// </summary>
    /// <param name="filename">Der Pfad zur zu prüfenden Datei</param>
    /// <param name="recentWriteThresholdSeconds">Schwellwert in Sekunden für kürzliche Schreibvorgänge</param>
    /// <returns>Leerer String bei Erfolg, ansonsten Fehlermeldung</returns>
    public static string CanSaveFile(string filename, int recentWriteThresholdSeconds) {
        if (string.IsNullOrEmpty(filename)) { return "Kein Dateiname angegeben."; }

        // Prüfen ob Datei schreibbar ist
        if (!CanWrite(filename)) { return "Windows blockiert die Datei."; }

        if (recentWriteThresholdSeconds > 0) {
            // Prüfen ob kürzlich geschrieben wurde
            try {
                FileInfo fileInfo = new(filename);
                if (DateTime.UtcNow.Subtract(fileInfo.LastWriteTimeUtc).TotalSeconds < recentWriteThresholdSeconds) {
                    return "Anderer Speichervorgang noch nicht abgeschlossen.";
                }
            } catch {
                return "Dateizugriffsfehler.";
            }
        }
        return string.Empty;
    }

    public static bool CanWrite(string filename) => ProcessFile(TryCanWrite, false, 5, filename) is true;

    public static bool CanWriteInDirectory(string directory) {
        if (string.IsNullOrEmpty(directory)) { return false; }

        directory = directory.CheckPath();

        if (!directory.IsFormat(FormatHolder.Filepath)) { return false; }

        var dirUpper = directory.ToUpperInvariant();

        // Prüfen, ob Ergebnis bereits im Cache ist und noch gültig
        if (_canWriteCache.TryGetValue(dirUpper, out var cacheEntry) &&
            DateTime.UtcNow.Subtract(cacheEntry.CheckTime).TotalSeconds <= 300) {
            return cacheEntry.Result;
        }

        // Vor Zugriff auf Cache, diesen ggf. bereinigen
        CleanupCanWriteCache();

        try {
            // Temporäre Testdatei mit zufälligem Namen erstellen
            var randomFileName = Path.Combine(directory, Path.GetRandomFileName());
            using (_ = File.Create(randomFileName, 1, FileOptions.DeleteOnClose)) { }

            // Erfolg im Cache speichern
            _canWriteCache[dirUpper] = (DateTime.UtcNow, true);
            return true;
        } catch {
            // Fehler im Cache speichern
            _canWriteCache[dirUpper] = (DateTime.UtcNow, false);
            return false;
        }
    }

    public static string CheckFile(this string pfad) => pfad.FilePath().CheckPath() + pfad.FileNameWithSuffix();

    /// <summary>
    /// Standard Pfad-Korrekturen. z.B. Doppelte Slashes, Backslashes. Gibt den Pfad mit abschließenden \ zurück.
    /// </summary>
    /// <param name="pfad"></param>
    /// <returns></returns>
    public static string CheckPath(this string pfad) {
        if (string.IsNullOrEmpty(pfad)) { return string.Empty; } // Kann vorkommen, wenn ein Benutzer einen Pfad per Hand eingeben darf
        if (pfad.Length > 6 && string.Equals(pfad.Substring(0, 7), "http://", StringComparison.OrdinalIgnoreCase)) { return pfad; }
        if (pfad.Length > 7 && string.Equals(pfad.Substring(0, 8), "https://", StringComparison.OrdinalIgnoreCase)) { return pfad; }

        if (pfad.Contains("/")) { pfad = pfad.Replace("/", "\\"); }

        if (pfad.Contains("%")) {
            var homep = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\";
            pfad = pfad.Replace("%homepath%\\", homep, RegexOptions.IgnoreCase);
            pfad = pfad.Replace("%homepath%", homep, RegexOptions.IgnoreCase);
        }

        if (pfad.Length == 0 || pfad.Substring(pfad.Length - 1) != "\\") { pfad += "\\"; }
        if (pfad.IndexOf("\\\\", 1, StringComparison.Ordinal) > 0) { Develop.DebugPrint("Achtung, Doppelslash: " + pfad); }
        if (pfad.Length > 1 && pfad.Substring(0, 1) == "\\" && pfad.Substring(0, 2) != "\\\\") { Develop.DebugPrint("Achtung, Doppelslash: " + pfad); }

        if (pfad.Length > 1 && pfad.IndexOf(":", 2, StringComparison.Ordinal) > 0) {
            pfad = pfad.Substring(0, 3) + pfad.Substring(3).RemoveChars(":");
        }

        try {
            return Path.GetFullPath(pfad);
        } catch { }

        Develop.CheckStackOverflow();
        return CheckPath(pfad);
    }

    public static bool CopyFile(string source, string target, bool abortIfFailed) => ProcessFile(TryCopyFile, abortIfFailed, abortIfFailed ? 60 : 5, source, target) is true;

    /// <summary>
    /// Erstellt ein Verzeichnis, mit Fehlerbehandlung und Wiederholungsversuchen
    /// </summary>
    /// <param name="dir">Das zu erstellende Verzeichnis</param>
    /// <returns>True, wenn das Verzeichnis (dann) existiert</returns>
    public static bool CreateDirectory(string dir) { return ProcessFile(TryCreateDirectory, false, 5, dir) is true; }

    public static bool DeleteDir(string directory, bool abortIfFailed) => ProcessFile(TryDeleteDir, abortIfFailed, abortIfFailed ? 60 : 5, directory) is true;

    /// <summary>
    /// Löscht eine Liste von Dateien
    /// </summary>
    /// <param name="filelist">Liste der zu löschenden Dateien</param>
    /// <returns>True, wenn mindestens eine Datei gelöscht wurde.</returns>
    public static bool DeleteFile(IEnumerable<string>? filelist) {
        if (filelist is not { }) { return false; }

        var lockMe = new object();
        var did = false;

        _ = Parallel.ForEach(filelist, thisf => {
            if (TryFileExists(thisf).returnValue is true) {
                if (DeleteFile(thisf, false)) {
                    lock (lockMe) {
                        did = true;
                    }
                }
            }
        });

        return did;
    }

    public static bool DeleteFile(string file, bool abortIfFailed) => ProcessFile(TryDeleteFile, abortIfFailed, abortIfFailed ? 60 : 5, file) is true;

    /// <summary>
    /// Prüft, ob ein Verzeichnis existiert, mit zusätzlichen Prüfungen und Fehlerbehandlung
    /// </summary>
    /// <param name="pfad">Der zu prüfende Pfad</param>
    /// <returns>True, wenn das Verzeichnis existiert</returns>
    public static bool DirectoryExists(string pfad) => ProcessFile(TryDirectoryExists, false, 5, pfad) is true;

    public static bool ExecuteFile(string fileName, string arguments = "", bool waitForExit = false, bool logException = true) {
        try {
            if (string.IsNullOrEmpty(fileName) && string.IsNullOrEmpty(arguments)) { return false; }
            var process = string.IsNullOrWhiteSpace(arguments) ? Process.Start(fileName) : Process.Start(fileName, arguments);
            if (waitForExit) {
                if (process == null) { return true; }// Windows 8, DANKE!
                process.WaitForExit();
                process.Dispose();
            }
        } catch (Exception ex) {
            if (logException) { Develop.DebugPrint("ExecuteFile konnte nicht ausgeführt werden:<br>" + ex.Message + "<br>Datei: " + fileName); }
            return false;
        }
        return true;
    }

    /// <summary>
    /// Prüft, ob eine Datei existiert, mit Fehlerbehandlung und Wiederholungsversuchen
    /// </summary>
    /// <param name="file">Die zu prüfende Datei</param>
    /// <returns>True, wenn die Datei existiert</returns>
    public static bool FileExists(string? file) { return ProcessFile(TryFileExists, false, 5, file ?? string.Empty) is true; }

    /// <summary>
    /// Gibt den Dateinamen ohne Suffix zurück.
    /// </summary>
    /// <param name="name">Der ganze Pfad der Datei.</param>
    /// <returns>Dateiname ohne Suffix</returns>
    /// <remarks></remarks>
    public static string FileNameWithoutSuffix(this string name) {
        try {
            return string.IsNullOrEmpty(name) ? string.Empty : Path.GetFileNameWithoutExtension(name);
        } catch {
            Develop.DebugPrint("Fehler bei: " + name);
            return string.Empty;
        }
    }

    public static string FileNameWithSuffix(this string name) {
        try {
            return string.IsNullOrEmpty(name) ? string.Empty : Path.GetFileName(name);
        } catch {
            return string.Empty;
        }
    }

    /// <summary>
    /// Gibt den Dateipad eines Dateistrings zurück, mit abschließenden \.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string FilePath(this string name) {
        if (string.IsNullOrEmpty(name)) { return string.Empty; }
        // Return Path.GetDirectoryName(Name) & "\" ' <---- Versagt ab 260 Zeichen
        name = name.Replace("/", "\\");
        var z = name.LastIndexOf("\\", StringComparison.Ordinal);
        return z < 0 ? string.Empty : name.Substring(0, z + 1);
    }

    public static string FileSuffix(this string name) {
        try {
            if (string.IsNullOrEmpty(name)) { return string.Empty; }
            if (!name.Contains(".")) { return string.Empty; }
            var l = Path.GetExtension(name);
            return string.IsNullOrEmpty(l) ? string.Empty : l.Substring(1);
        } catch {
            // Illegales Zeichen im Pfad?
            return string.Empty;
        }
    }

    public static FileFormat FileType(this string filename) => string.IsNullOrEmpty(filename)
                    ? FileFormat.Unknown
                    : filename.FileSuffix().ToUpperInvariant() switch {
                        "DOC" or "DOCX" or "RTF" or "ODT" => FileFormat.WordKind,
                        "TXT" or "INI" or "INFO" => FileFormat.Textdocument,
                        "XLS" or "XLA" or "XLSX" or "XLSM" or "ODS" => FileFormat.ExcelKind,
                        "CSV" => FileFormat.CSV,
                        "PPT" or "PPS" or "PPA" => FileFormat.PowerPointKind,
                        "MSG" or "EML" => FileFormat.EMail,
                        "PDF" => FileFormat.Pdf,
                        "HTM" or "HTML" => FileFormat.HTML,
                        "JPG" or "JPEG" or "BMP" or "TIFF" or "TIF" or "GIF" or "PNG" => FileFormat.Image,
                        "ICO" => FileFormat.Icon,
                        "ZIP" or "RAR" or "7Z" => FileFormat.CompressedArchive,
                        "AVI" or "DIVX" or "MPG" or "MPEG" or "WMV" or "FLV" or "MP4" or "MKV" or "M4V" => FileFormat.Movie,
                        "EXE" or "BAT" or "SCR" => FileFormat.Executable,
                        "CHM" => FileFormat.HelpFile,
                        "XML" => FileFormat.XMLFile,
                        "VCF" => FileFormat.Visitenkarte,
                        "MP3" or "WAV" or "AAC" => FileFormat.Sound,
                        "B4A" or "BAS" or "CS" => FileFormat.ProgrammingCode,// case "DLL":
                        "DB" or "MDB" or "BDB" or "MBDB" or "CBDB" => FileFormat.Database,
                        "BDBC" => FileFormat.DatabaseChunk,
                        "LNK" or "URL" => FileFormat.Link,
                        "BCR" => FileFormat.BlueCreativeFile,
                        "BCS" => FileFormat.BlueCreativeSymbol,
                        _ => FileFormat.Unknown
                    };

    /// <summary>
    /// Gibt von einem Pfad den letzten Ordner zurück
    /// </summary>
    /// <param name="pathx"></param>
    /// <returns></returns>
    public static string Folder(this string pathx) {
        if (string.IsNullOrEmpty(pathx)) { return string.Empty; }
        // Kann vorkommen, wenn ein Benutzer einen Pfad
        // per Hand eingeben darf
        pathx = pathx.Replace("/", "\\").TrimEnd('\\');
        if (!pathx.Contains("\\")) { return pathx; }
        var z = pathx.Length;
        if (z < 2) { return string.Empty; }
        while (true) {
            z--;
            if (pathx.Substring(z, 1) == "\\") { return pathx.Substring(z + 1); }
            if (z < 1) { return string.Empty; }
        }
    }

    /// <summary>
    /// Liefert Dateiinformationen mit Fehlerbehandlung und Wiederholungsversuchen
    /// </summary>
    /// <param name="filename">Der Dateiname</param>
    /// <param name="mustDo">True wenn die Funktion auf jeden Fall ein Ergebnis liefern muss</param>
    /// <returns>Dateizeitstempel und -größe als String</returns>
    public static string GetFileInfo(string filename, bool abortIfFailed) => ProcessFile(TryGetFileInfo, abortIfFailed, abortIfFailed ? 60 : 5, filename) as string ?? string.Empty;

    /// <summary>
    /// Lädt Bytes aus einer Datei mit automatischer Retry-Logik und Dekomprimierung
    /// </summary>
    public static (byte[] bytes, string fileinfo, bool failed) LoadBytesFromDisk(string filename, bool autoDecompress) {
        if (string.IsNullOrEmpty(filename)) { return (Array.Empty<byte>(), string.Empty, true); }

        var result = ProcessFile(TryLoadBytesFromDisk, false, 60, filename, autoDecompress);

        // Rückgabe ist ein object, das wir zu unserem Tupel casten müssen
        if (result is not null and ValueTuple<byte[], string, bool> loadResult) {
            return loadResult;
        }

        return (Array.Empty<byte>(), string.Empty, true);
    }

    public static bool MoveDirectory(string oldName, string newName, bool abortIfFailed) => ProcessFile(TryMoveDirectory, abortIfFailed, abortIfFailed ? 60 : 5, oldName, newName) is true;

    /// <summary>
    /// Verschiebt eine Datei mit erweiterter Fehlerbehandlung und Wartezeit bis die Datei verfügbar ist
    /// </summary>
    /// <param name="oldName">Quellpfad</param>
    /// <param name="newName">Zielpfad</param>
    /// <param name="abortIfFailed">True für garantierte Ausführung (sonst Programmabbruch)</param>
    /// <returns>True bei Erfolg</returns>
    public static bool MoveFile(string oldName, string newName, bool abortIfFailed) => ProcessFile(TryMoveFile, abortIfFailed, abortIfFailed ? 60 : 5, oldName, newName) is true;

    /// <summary>
    /// Gibt einen höher gelegenden Ordner mit abschließenden \ zurück
    /// </summary>
    /// <param name="pfad"></param>
    /// <param name="anzahlParents"></param>
    /// <returns></returns>
    public static string PathParent(this string pfad, int anzahlParents) {
        for (var z = 1; z <= anzahlParents; z++) {
            pfad = pfad.PathParent();
        }
        return pfad;
    }

    /// <summary>
    /// Gibt einen höher gelegenden Ordner mit abschließenden \ zurück
    /// </summary>
    /// <param name="pfad"></param>
    public static string PathParent(this string? pfad) {
        if (pfad is not { }) { return string.Empty; }

        var z = pfad.Length;

        while (true) {
            z--;
            if (z <= 1) { return string.Empty; }
            if (pfad.Substring(z - 1, 1) == "\\") { return pfad.Substring(0, z); }
        }
    }

    /// <summary>
    /// Führt einen Datei-Befehl mit erweiterter Fehlerbehandlung und Wiederholungsversuchen aus
    /// </summary>
    /// <param name="processMethod">Die auszuführende Methode</param>
    /// <param name="abortIfFailed">True für garantierte Ausführung (sonst Programmabbruch)</param>
    /// <param name="args">Variable Parameter</param>
    public static object? ProcessFile(DoThis processMethod, bool abortIfFailed, int trySeconds, params object[] args) {
        if (Develop.AllReadOnly) { return true; }

        var startTime = Stopwatch.StartNew();
        var stopw = Stopwatch.StartNew();

        while (true) {
            var (returnValue, retry) = processMethod(args);
            if (!retry) { return returnValue; }

            // Bei abortIfFailed=true weiter versuchen, aber nach 60 Sekunden eine Fehlermeldung ausgeben
            if (startTime.ElapsedMilliseconds > trySeconds * 1000) {
                var argsStr = string.Join(", ", args.Select(a => a?.ToString() ?? "null"));

                if (abortIfFailed) {
                    Develop.DebugPrint(ErrorType.Error, "Datei-Befehl konnte nicht ausgeführt werden:\r\n" + argsStr);
                }

                return returnValue;
            }

            if (stopw.ElapsedMilliseconds > 5000) {
                var operation = processMethod.Method.Name.Replace("Try", "").Replace("File", "").Replace("Dir", "");
                var fileName = args.Length > 0 ? args[0]?.ToString()?.FileNameWithSuffix() ?? "unbekannt" : "unbekannt";
                var mess = "Keine weiteren Informationen vorhanden";
                if (returnValue is string m) { mess = m; }

                Develop.Message?.Invoke(ErrorType.Info, null, Develop.MonitorMessage, ImageCode.Diskette, $"Warte auf Abschluss einer Dateioperation ({operation}) von {fileName}... ({mess})", 0);
                stopw = Stopwatch.StartNew();
            }

            Thread.Sleep(200);
        }
    }

    public static string TempFile(string newPath, string filename) {
        var dn = filename.FileNameWithoutSuffix();
        var ds = filename.FileSuffix();
        return TempFile(newPath, dn, ds);
    }

    public static string TempFile(string fullName) {
        var dp = fullName.FilePath();
        var dn = fullName.FileNameWithoutSuffix();
        var ds = fullName.FileSuffix();
        return TempFile(dp, dn, ds);
    }

    /// <summary>
    /// Erstellt einen eindeutigen temporären Dateinamen mit Berücksichtigung von Multithread-Zugriffen
    /// </summary>
    public static string TempFile(string pfad, string wunschname, string suffix) {
        if (string.IsNullOrEmpty(pfad)) { pfad = Path.GetTempPath(); }
        if (string.IsNullOrEmpty(suffix)) { suffix = "tmp"; }
        if (string.IsNullOrEmpty(wunschname)) { wunschname = UserName + DateTime.UtcNow.ToString6(); }
        var z = -1;
        pfad = pfad.CheckPath();
        CreateDirectory(pfad);
        wunschname = wunschname.ReduceToChars(Constants.Char_Numerals + " _+-#" + Constants.Char_Buchstaben + Constants.Char_Buchstaben.ToUpperInvariant());

        if (wunschname.Length > 80) { wunschname = wunschname.Substring(0, 80); }

        string? filename;
        do {
            z++;
            filename = z > 0 ? pfad + wunschname + "_" + z.ToStringInt5() + "." + suffix : pfad + wunschname + "." + suffix;
        } while (TryFileExists(filename).returnValue is true);
        return filename;
    }

    /// <summary>
    /// Speichert den Text in einer Datei.
    /// </summary>
    /// <param name="filename">Zieldatei</param>
    /// <param name="contents">Inhalt</param>
    /// <param name="encoding">Kodierung</param>
    /// <param name="executeAfter">Datei nach dem Speichern ausführen</param>
    /// <returns>True bei Erfolg</returns>
    public static bool WriteAllText(string filename, string contents, Encoding encoding, bool executeAfter) {
        try {
            if (Develop.AllReadOnly) { return true; }
            filename = filename.CheckFile();

            var pfad = filename.FilePath();
            if (!CreateDirectory(pfad)) { return false; }

            File.WriteAllText(filename, contents, encoding);
            if (executeAfter) { _ = ExecuteFile(filename); }
            return true;
        } catch {
            //  Develop.DebugPrint(ErrorType.Info, "Fehler beim Speichern der Datei: " + filename, ex);
            return false;
        }
    }

    /// <summary>
    /// Bereinigt den _canWriteCache wenn er zu viele Einträge enthält oder Einträge zu alt sind
    /// </summary>
    private static void CleanupCanWriteCache() {
        // Wenn weniger als 1000 Einträge, nichts tun
        if (_canWriteCache.Count < 1000) { return; }

        // Lock verwenden, um Thread-Sicherheit zu gewährleisten
        lock (_fileOperationLock) {
            try {
                // Wenn zwischenzeitlich aufgeräumt wurde, nichts tun
                if (_canWriteCache.Count < 1000) {
                    return;
                }

                // Aktuelle Zeit für Altersprüfung
                var now = DateTime.UtcNow;

                // Einträge identifizieren, die älter als 10 Minuten sind
                var keysToRemove = _canWriteCache
                    .Where(kvp => now.Subtract(kvp.Value.CheckTime).TotalMinutes > 10)
                    .Select(kvp => kvp.Key)
                    .ToList();

                // Alte Einträge entfernen
                foreach (var key in keysToRemove) {
                    _canWriteCache.TryRemove(key, out _);
                }

                // Wenn noch immer mehr als 500 Einträge, die ältesten entfernen
                if (_canWriteCache.Count > 500) {
                    var oldestEntries = _canWriteCache
                        .OrderBy(kvp => kvp.Value.CheckTime)
                        .Take(_canWriteCache.Count - 500)
                        .Select(kvp => kvp.Key)
                        .ToList();

                    foreach (var key in oldestEntries) {
                        _canWriteCache.TryRemove(key, out _);
                    }
                }
            } catch { }
        }
    }

    /// <summary>
    /// Entfernt Einträge aus dem _canWriteCache basierend auf einer Datei oder einem Verzeichnis
    /// </summary>
    /// <param name="fileOrDirectory">Datei oder Verzeichnis, dessen Cache-Einträge entfernt werden sollen</param>
    private static void RemoveFromCanWriteCache(string fileOrDirectory) {
        if (string.IsNullOrEmpty(fileOrDirectory)) { return; }

        lock (_fileOperationLock) {
            try {
                var pathUpper = fileOrDirectory.ToUpperInvariant();

                // Alle Cache-Einträge entfernen, die mit diesem Verzeichnispfad beginnen
                var keysToRemove = _canWriteCache.Keys
                    .Where(key => key.StartsWith(pathUpper))
                    .ToList();

                foreach (var key in keysToRemove) {
                    _canWriteCache.TryRemove(key, out _);
                }
            } catch {
                // Fehler ignorieren - Cache-Bereinigung ist nicht kritisch
            }
        }
    }

    private static (object? returnValue, bool retry) TryCanWrite(params object[] args) {
        if (args.Length < 1 || args[0] is not string file) { return (false, false); }

        lock (_fileOperationLock) {
            if (!CanWriteInDirectory(file.FilePath())) { return (false, false); }

            var fileUpper = file.ToUpperInvariant();

            // Prüfen, ob wir für diese Datei bereits ein Ergebnis haben und ob es noch gültig ist
            if (_canWriteCache.TryGetValue(fileUpper, out var cacheEntry) &&
                DateTime.UtcNow.Subtract(cacheEntry.CheckTime).TotalSeconds <= 2) {
                return (cacheEntry.Result, false);
            }

            // Vor Zugriff auf Cache, diesen ggf. bereinigen
            CleanupCanWriteCache();

            // Wenn kein gültiges Ergebnis vorliegt, führe die Prüfung durch
            var result = false;

            if (TryFileExists(file).returnValue is true) {
                try {
                    // Versuch, Datei EXKLUSIV zu öffnen
                    using (FileStream obFi = new(file, FileMode.Open, FileAccess.ReadWrite, FileShare.Read)) {
                        obFi.Close();
                    }
                    result = true;
                } catch {
                    // Bei Fehler ist die Datei in Benutzung
                    result = false;
                }
            } else {
                result = true;
            }

            // Ergebnis im Cache speichern
            _canWriteCache[fileUpper] = (DateTime.UtcNow, result);

            return (result, false);
        }
    }

    private static (object? returnValue, bool retry) TryCopyFile(params object[] args) {
        if (args.Length < 2 || args[0] is not string source || args[1] is not string target) { return (false, false); }

        if (source == target) { return (true, false); }
        if (TryFileExists(source).returnValue is not true) { return (false, false); }
        if (TryFileExists(target).returnValue is true) { return (false, false); }

        try {
            var sourceInfo = new FileInfo(source);
            File.Copy(source, target);

            // Warten bis die Datei mit korrekter Größe existiert - verwende ProcessFile
            return (ProcessFile(TryWaitForCopiedFile, false, 60, target, sourceInfo.Length) is true, false);
        } catch {
            return (false, true);
        }
    }

    private static (object? returnValue, bool retry) TryCreateDirectory(params object[] args) {
        if (args.Length < 1 || args[0] is not string dir) { return (false, false); }
        dir = dir.CheckPath();

        if (string.IsNullOrEmpty(dir) || !dir.IsFormat(FormatHolder.Filepath)) { return (false, false); }

        if (TryDirectoryExists(dir).returnValue is true) { return (true, false); }

        try {
            Directory.CreateDirectory(dir);
            return (true, false);
        } catch (IOException) {
            return (false, true);
        } catch {
            return (false, false);
        }
    }

    private static (object? returnValue, bool retry) TryDeleteDir(params object[] args) {
        if (args.Length < 1 || args[0] is not string directory) { return (false, false); }

        directory = directory.CheckPath();
        if (TryDirectoryExists(directory).returnValue is not true) { return (true, false); }

        try {
            Directory.Delete(directory, true);
            RemoveFromCanWriteCache(directory);

            // Warten bis das Verzeichnis gelöscht ist - verwende TryDirectoryExists negiert
            return (ProcessFile(TryDirectoryExists, false, 60, directory) is not true, false);
        } catch {
            return (false, true);
        }
    }

    private static (object? returnValue, bool retry) TryDeleteFile(params object[] args) {
        if (args.Length < 1 || args[0] is not string file) { return (false, false); }

        if (TryFileExists(file).returnValue is not true) { return (true, false); }

        // Komisch, manche Dateien können zwar gelöscht werden, die Attribute aber nicht geändert (Berechtigungen?)
        try {
            if (File.GetAttributes(file).HasFlag(FileAttributes.ReadOnly)) {
                File.SetAttributes(file, FileAttributes.Normal);
            }
        } catch {
            // Fehler ignorieren - wir probieren trotzdem zu löschen
        }

        try {
            RemoveFromCanWriteCache(file);
            if (!CanWrite(file)) { return (false, false); }

            File.Delete(file);
            RemoveFromCanWriteCache(file);

            // Warten, bis die Datei wirklich gelöscht ist
            for (var i = 0; i < _fileExistenceCheckRetryCount; i++) {
                if (TryFileExists(file).returnValue is not true) { return (true, false); }
                Thread.Sleep(200);
            }
            return (TryFileExists(file).returnValue is not true, false);
        } catch {
            return (false, true);
        }
    }

    private static (object? returnValue, bool retry) TryDirectoryExists(params object[] args) {
        if (args.Length < 1 || args[0] is not string pfad) { return (false, false); }

        if (pfad.Length < 3) { return (false, false); }

        var p = pfad.CheckPath();

        if (!p.IsFormat(FormatHolder.Filepath)) { return (false, false); }

        try {
            return (Directory.Exists(p), false);
        } catch (IOException) {
            return (false, true);  // Netzwerk-IO-Fehler
        } catch (UnauthorizedAccessException) {
            return (false, true);  // Berechtigungsfehler
        } catch {
            return (false, false); // Andere Fehler
        }
    }

    private static (object? returnValue, bool retry) TryFileExists(params object[] args) {
        if (args.Length < 1 || args[0] is not string file) { return (false, false); }

        if (string.IsNullOrEmpty(file) || !file.IsFormat(FormatHolder.FilepathAndName)) { return (false, false); }

        try {
            return (File.Exists(file), false);
        } catch (IOException) {
            return (false, true);
        } catch (UnauthorizedAccessException) {
            return (false, true);
        } catch {
            return (false, false);
        }
    }

    private static (object? returnValue, bool retry) TryGetFileInfo(params object[] args) {
        if (args.Length < 1 || args[0] is not string filename) { return (string.Empty, false); }

        try {
            FileInfo f = new(filename);
            return (f.LastWriteTimeUtc.ToString1() + "-" + f.Length, false);
        } catch {
            return (string.Empty, true);
        }
    }

    private static (object? returnValue, bool retry) TryLoadBytesFromDisk(params object[] args) {
        if (args.Length < 2 || args[0] is not string filename || args[1] is not bool autoDecompress) {
            return ((Array.Empty<byte>(), string.Empty, true), false);
        }

        try {
            // Direkter Aufruf der Try-Methode anstatt GetFileInfo
            var (fileinfoResult, retry) = TryGetFileInfo(filename);
            var fileinfo = fileinfoResult as string ?? string.Empty;

            if (string.IsNullOrEmpty(fileinfo)) {
                return ((Array.Empty<byte>(), string.Empty, true), retry);
            }

            var bLoaded = File.ReadAllBytes(filename);

            if (autoDecompress && bLoaded.IsZipped()) {
                bLoaded = bLoaded.UnzipIt() ?? bLoaded;
            }

            return ((bLoaded, fileinfo, false), false);
        } catch (IOException) {
            return ((Array.Empty<byte>(), string.Empty, true), true);  // Retry bei IO-Fehlern
        } catch {
            return ((Array.Empty<byte>(), string.Empty, true), false); // Keine Retry bei anderen Fehlern
        }
    }

    private static (object? returnValue, bool retry) TryMoveDirectory(params object[] args) {
        if (args.Length < 2 || args[0] is not string oldName || args[1] is not string newName) { return (false, false); }

        if (oldName == newName) { return (true, false); }
        if (TryDirectoryExists(oldName).returnValue is not true) { return (false, false); }
        if (TryDirectoryExists(newName).returnValue is true) { return (false, false); }

        try {
            Directory.Move(oldName, newName);
            RemoveFromCanWriteCache(oldName);
            RemoveFromCanWriteCache(newName);

            // Warten, bis das Verzeichnis am neuen Ort existiert
            for (var i = 0; i < _fileExistenceCheckRetryCount; i++) {
                if (TryDirectoryExists(newName).returnValue is true && TryDirectoryExists(oldName).returnValue is not true) { return (true, false); }
                Thread.Sleep(200);
            }
            return (TryDirectoryExists(newName).returnValue is true && TryDirectoryExists(oldName).returnValue is not true, false);
        } catch {
            return (false, true);
        }
    }

    private static (object? returnValue, bool retry) TryMoveFile(params object[] args) {
        if (args.Length < 2 || args[0] is not string oldName || args[1] is not string newName) { return (false, false); }

        if (oldName == newName) { return (true, false); }
        if (TryFileExists(oldName).returnValue is not true) { return (false, false); }
        if (TryFileExists(newName).returnValue is true) { return (false, false); }

        try {
            // Sicherstellen, dass das Zielverzeichnis existiert
            var targetDir = Path.GetDirectoryName(newName);
            if (!string.IsNullOrEmpty(targetDir)) {
                CreateDirectory(targetDir);
            }

            File.Move(oldName, newName);
            RemoveFromCanWriteCache(oldName);
            RemoveFromCanWriteCache(newName);

            // Warten, bis die Datei am neuen Ort existiert und an der alten Position verschwunden ist
            for (var i = 0; i < _fileExistenceCheckRetryCount; i++) {
                if (TryFileExists(newName).returnValue is true && TryFileExists(oldName).returnValue is not true) { return (true, false); }
                Thread.Sleep(200);
            }

            return (TryFileExists(newName).returnValue is true && TryFileExists(oldName).returnValue is not true, false);
        } catch {
            return (false, true);
        }
    }

    private static (object? returnValue, bool retry) TryWaitForCopiedFile(params object[] args) {
        if (args.Length < 2 || args[0] is not string targetFile || args[1] is not long expectedSize) { return (false, false); }

        if (TryFileExists(targetFile).returnValue is true) {
            try {
                var targetInfo = new FileInfo(targetFile);
                if (targetInfo.Length == expectedSize) {
                    return (true, false);
                }
            } catch {
                // Bei Fehler beim Dateizugriff retry
            }
        }
        return (false, true);
    }

    #endregion
}