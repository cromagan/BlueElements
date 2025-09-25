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

    /// <summary>
    /// Wird verwendet für File-Dialoge und ist nicht weiter relevant.
    /// </summary>
    public static string LastFilePath = string.Empty;

    private const int _fileExistenceCheckRetryCount = 20;
    private static readonly ConcurrentDictionary<string, (DateTime CheckTime, bool Result)> _canWriteCache = new();
    private static readonly object _fileOperationLock = new();

    #endregion

    #region Delegates

    public delegate FileOperationResult DoThis(params object[] args);

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
            if (TryFileExists(thisf).ReturnValue is true) {
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

    public static bool DirectoryMove(string oldName, string newName, bool abortIfFailed) => ProcessFile(TryMoveDirectory, abortIfFailed, abortIfFailed ? 60 : 5, oldName, newName) is true;

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

    public static bool FileCopy(string source, string target, bool abortIfFailed) => ProcessFile(TryFileCopy, abortIfFailed, abortIfFailed ? 60 : 5, source, target) is true;

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
                            "DB" or "MDB" or "BDB" or "MBDB" or "CBDB" => FileFormat.Table,
                            "BDBC" => FileFormat.TableChunk,
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

    public static string[] GetDirectories(string pfad) => ProcessFile(TryGetDirectories, false, 5, pfad, "*", SearchOption.TopDirectoryOnly) as string[] ?? Array.Empty<string>();

    public static string[] GetDirectories(string pfad, string pattern, SearchOption suchOption) => ProcessFile(TryGetDirectories, false, 5, pfad, pattern, suchOption) as string[] ?? Array.Empty<string>();

    public static FileInfo? GetFileInfo(string datei) => ProcessFile(TryGetFileInfo, false, 5, datei) as FileInfo;

    public static string[] GetFiles(string pfad, string pattern, SearchOption suchOption)
                        => ProcessFile(TryGetFiles, false, 5, pfad, pattern, suchOption) as string[] ?? Array.Empty<string>();

    public static string[] GetFiles(string pfad)
                     => ProcessFile(TryGetFiles, false, 5, pfad, "*", SearchOption.TopDirectoryOnly) as string[] ?? Array.Empty<string>();

    /// <summary>
    /// Liefert Dateiinformationen mit Fehlerbehandlung und Wiederholungsversuchen
    /// </summary>
    /// <param name="filename">Der Dateiname</param>
    /// <param name="mustDo">True wenn die Funktion auf jeden Fall ein Ergebnis liefern muss</param>
    /// <returns>Dateizeitstempel und -größe als String</returns>
    public static string GetFileState(string filename, bool abortIfFailed) => ProcessFile(TryGetFileState, abortIfFailed, abortIfFailed ? 60 : 5, filename) as string ?? string.Empty;

    /// <summary>
    /// Lädt alle Bytes aus einer Datei mit automatischer Retry-Logik
    /// </summary>
    /// <param name="filename">Der Pfad zur zu ladenden Datei</param>
    /// <returns>Die geladenen Bytes oder ein leeres Array bei Fehler</returns>
    public static byte[] LoadAllBytes(string filename) {
        var result = ProcessFile(TryLoadAllBytes, false, 60, filename);
        return result as byte[] ?? Array.Empty<byte>();
    }

    /// <summary>
    /// Lädt Bytes aus einer Datei mit automatischer Retry-Logik und Dekomprimierung
    /// </summary>
    public static (byte[] bytes, string fileinfo, bool failed) LoadAndUnzipAllBytes(string filename) {
        if (string.IsNullOrEmpty(filename)) { return (Array.Empty<byte>(), string.Empty, true); }

        var result = ProcessFile(TryLoadAndUnzipAllBytes, false, 60, filename);

        // Rückgabe ist ein object, das wir zu unserem Tupel casten müssen
        if (result is not null and ValueTuple<byte[], string, bool> loadResult) {
            return loadResult;
        }

        return (Array.Empty<byte>(), string.Empty, true);
    }

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
            var result = processMethod(args); // Jetzt FileOperationResult statt Tuple
            if (!result.Retry) { return result.ReturnValue; }

            // Bei abortIfFailed=true weiter versuchen, aber nach 60 Sekunden eine Fehlermeldung ausgeben
            if (startTime.ElapsedMilliseconds > trySeconds * 1000) {
                var argsStr = string.Join(", ", args.Select(a => a?.ToString() ?? "null"));

                if (abortIfFailed) {
                    Develop.DebugPrint(ErrorType.Error, "Datei-Befehl konnte nicht ausgeführt werden:\r\n" + argsStr);
                }

                return result.ReturnValue;
            }

            if (stopw.ElapsedMilliseconds > 5000) {
                var operation = processMethod.Method.Name.Replace("Try", "").Replace("File", "").Replace("Dir", "");
                var fileName = args.Length > 0 ? args[0]?.ToString()?.FileNameWithSuffix() ?? "unbekannt" : "unbekannt";
                var mess = "Keine weiteren Informationen vorhanden";
                if (result.ReturnValue is string m) { mess = m; }

                Develop.Message?.Invoke(ErrorType.Info, null, Develop.MonitorMessage, ImageCode.Diskette, $"Warte auf Abschluss einer Dateioperation ({operation}) von {fileName}... ({mess})", 0);
                stopw = Stopwatch.StartNew();
            }

            Thread.Sleep(200);
        }
    }

    /// <summary>
    /// Liest den gesamten Text aus einer Datei mit der angegebenen Kodierung
    /// </summary>
    /// <param name="path">Der Pfad zur zu lesenden Datei</param>
    /// <param name="encoding">Die zu verwendende Kodierung</param>
    /// <returns>Der gesamte Inhalt der Datei als String</returns>
    public static string ReadAllText(string path, FileShare share, Encoding encoding) {
        var result = ProcessFile(TryReadAllText, false, 60, path, share, encoding);
        return result as string ?? string.Empty;
    }

    public static string ReadAllText(string path, Encoding encoding) {
        var result = ProcessFile(TryReadAllText, false, 60, path, FileShare.Read, encoding);
        return result as string ?? string.Empty;
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
        } while (TryFileExists(filename).ReturnValue is true);
        return filename;
    }

    public static FileOperationResult TryGetFileInfo(object?[] args) {
        if (args.Length < 1) { return FileOperationResult.SuccessNull; }
        var datei = args[0] as string;

        if (string.IsNullOrWhiteSpace(datei)) { return FileOperationResult.SuccessNull; }

        try {
            var fi = new FileInfo(datei);
            // Zugriff testen (kann IOException / UnauthorizedAccess auslösen)
            //_ = fi.Length; // leichte Operation zum Validieren
            return new(fi);
        } catch (UnauthorizedAccessException) {
            return FileOperationResult.SuccessNull;
        } catch (FileNotFoundException) {
            return FileOperationResult.SuccessNull;
        } catch (DirectoryNotFoundException) {
            return FileOperationResult.SuccessNull;
        } catch {
            return FileOperationResult.DoRetry;
        }
    }

    /// <summary>
    /// Speichert alle Bytes in eine Datei (Overload ohne abortIfFailed)
    /// </summary>
    /// <param name="filename">Zieldatei</param>
    /// <param name="bytes">Zu speichernde Bytes</param>
    /// <returns>True bei Erfolg</returns>
    public static bool WriteAllBytes(string filename, byte[] bytes) => WriteAllBytes(filename, bytes, false);

    /// <summary>
    /// Speichert alle Bytes in eine Datei mit automatischer Retry-Logik und Verzeichniserstellung
    /// </summary>
    /// <param name="filename">Zieldatei</param>
    /// <param name="bytes">Zu speichernde Bytes</param>
    /// <param name="abortIfFailed">True für garantierte Ausführung (sonst Programmabbruch)</param>
    /// <returns>True bei Erfolg</returns>
    public static bool WriteAllBytes(string filename, byte[] bytes, bool abortIfFailed) => ProcessFile(TryWriteAllBytes, abortIfFailed, abortIfFailed ? 60 : 5, filename, bytes) is true;

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

    private static FileOperationResult TryCanWrite(params object[] args) {
        if (args.Length < 1 || args[0] is not string file) { return FileOperationResult.ValueFalse; }

        lock (_fileOperationLock) {
            if (!CanWriteInDirectory(file.FilePath())) { return FileOperationResult.ValueFalse; }

            var fileUpper = file.ToUpperInvariant();

            // Prüfen, ob wir für diese Datei bereits ein Ergebnis haben und ob es noch gültig ist
            if (_canWriteCache.TryGetValue(fileUpper, out var cacheEntry) &&
                DateTime.UtcNow.Subtract(cacheEntry.CheckTime).TotalSeconds <= 2) {
                return new(cacheEntry.Result);
            }

            // Vor Zugriff auf Cache, diesen ggf. bereinigen
            CleanupCanWriteCache();

            // Wenn kein gültiges Ergebnis vorliegt, führe die Prüfung durch
            var result = false;

            if (TryFileExists(file).ReturnValue is true) {
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

            return new(result);
        }
    }

    private static FileOperationResult TryCreateDirectory(params object[] args) {
        if (args.Length < 1 || args[0] is not string dir) { return FileOperationResult.ValueFalse; }
        dir = dir.CheckPath();

        if (string.IsNullOrEmpty(dir) || !dir.IsFormat(FormatHolder.Filepath)) { return FileOperationResult.ValueFalse; }

        if (TryDirectoryExists(dir).ReturnValue is true) { return FileOperationResult.ValueTrue; }

        try {
            Directory.CreateDirectory(dir);
            return FileOperationResult.ValueTrue;
        } catch (IOException) {
            return FileOperationResult.DoRetry;
        } catch {
            return FileOperationResult.ValueFalse;
        }
    }

    private static FileOperationResult TryDeleteDir(params object[] args) {
        if (args.Length < 1 || args[0] is not string directory) { return FileOperationResult.ValueFalse; }

        directory = directory.CheckPath();
        if (TryDirectoryExists(directory).ReturnValue is not true) { return FileOperationResult.ValueTrue; }

        try {
            Directory.Delete(directory, true);
            RemoveFromCanWriteCache(directory);

            // Warten bis das Verzeichnis gelöscht ist - verwende TryDirectoryExists negiert
            return new(ProcessFile(TryDirectoryExists, false, 60, directory) is not true);
        } catch {
            return FileOperationResult.DoRetry;
        }
    }

    private static FileOperationResult TryDeleteFile(params object[] args) {
        if (args.Length < 1 || args[0] is not string file) { return FileOperationResult.ValueFalse; }

        if (TryFileExists(file).ReturnValue is not true) { return FileOperationResult.ValueTrue; }

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
            if (!CanWrite(file)) { return FileOperationResult.ValueFalse; }

            File.Delete(file);
            RemoveFromCanWriteCache(file);

            // Warten, bis die Datei wirklich gelöscht ist
            for (var i = 0; i < _fileExistenceCheckRetryCount; i++) {
                if (TryFileExists(file).ReturnValue is not true) { return FileOperationResult.ValueTrue; }
                Thread.Sleep(200);
            }
            return new(TryFileExists(file).ReturnValue is not true);
        } catch {
            return FileOperationResult.DoRetry;
        }
    }

    private static FileOperationResult TryDirectoryExists(params object[] args) {
        if (args.Length < 1 || args[0] is not string pfad) { return FileOperationResult.ValueFalse; }

        if (pfad.Length < 3) { return FileOperationResult.ValueFalse; }

        var p = pfad.CheckPath();

        if (!p.IsFormat(FormatHolder.Filepath)) { return FileOperationResult.ValueFalse; }

        try {
            return new(Directory.Exists(p));
        } catch (IOException) {
            return FileOperationResult.DoRetry;  // Netzwerk-IO-Fehler
        } catch (UnauthorizedAccessException) {
            return FileOperationResult.DoRetry;  // Berechtigungsfehler
        } catch {
            return FileOperationResult.ValueFalse; // Andere Fehler
        }
    }

    private static FileOperationResult TryFileCopy(params object[] args) {
        if (args.Length < 2 || args[0] is not string source || args[1] is not string target) { return FileOperationResult.ValueFalse; }

        if (source == target) { return FileOperationResult.ValueTrue; }
        if (TryFileExists(source).ReturnValue is not true) { return FileOperationResult.ValueFalse; }
        if (TryFileExists(target).ReturnValue is true) { return FileOperationResult.ValueFalse; }

        try {
            var sourceInfo = new FileInfo(source);
            File.Copy(source, target);

            // Warten bis die Datei mit korrekter Größe existiert - verwende ProcessFile
            return new(ProcessFile(TryWaitForCopiedFile, false, 60, target, sourceInfo.Length) is true);
        } catch {
            return FileOperationResult.DoRetry;
        }
    }

    private static FileOperationResult TryFileExists(params object[] args) {
        if (args.Length < 1 || args[0] is not string file) { return FileOperationResult.ValueFalse; }

        if (string.IsNullOrEmpty(file) || !file.IsFormat(FormatHolder.FilepathAndName)) { return FileOperationResult.ValueFalse; }

        try {
            return new(File.Exists(file));
        } catch (IOException) {
            return FileOperationResult.DoRetry;
        } catch (UnauthorizedAccessException) {
            return FileOperationResult.DoRetry;
        } catch {
            return FileOperationResult.ValueFalse;
        }
    }

    private static FileOperationResult TryGetDirectories(object?[] args) {
        if (args.Length < 3) { return new(Array.Empty<string>()); }
        var directory = args[0] as string;
        var pattern = args[1] as string;
        var option = args[2] is SearchOption so ? so : SearchOption.TopDirectoryOnly;

        if (string.IsNullOrWhiteSpace(directory)) { return new(Array.Empty<string>()); }

        try {
            if (!DirectoryExists(directory)) { return new(Array.Empty<string>()); }

            pattern = string.IsNullOrWhiteSpace(pattern) ? "*" : pattern;

            var dirs = Directory.GetDirectories(directory, pattern, option);
            return new(dirs);
        } catch (UnauthorizedAccessException) {
            return new(Array.Empty<string>());
        } catch {
            return FileOperationResult.DoRetry;
        }
    }

    private static FileOperationResult TryGetFiles(object?[] args) {
        if (args.Length < 3) { return new(Array.Empty<string>()); }
        var pfad = args[0] as string;
        var pattern = args[1] as string;
        var option = args[2] is SearchOption so ? so : SearchOption.TopDirectoryOnly;

        if (string.IsNullOrWhiteSpace(pfad))
            return new(Array.Empty<string>());

        try {
            if (!DirectoryExists(pfad)) { return new(Array.Empty<string>()); }

            pattern = string.IsNullOrWhiteSpace(pattern) ? "*" : pattern;

            var files = Directory.GetFiles(pfad, pattern, option);
            return new(files);
        } catch (UnauthorizedAccessException) {
            return new(Array.Empty<string>());
        } catch {
            return FileOperationResult.DoRetry;
        }
    }

    private static FileOperationResult TryGetFileState(params object[] args) {
        if (args.Length < 1 || args[0] is not string filename) { return new(string.Empty); }

        try {
            FileInfo f = new(filename);
            return new(f.LastWriteTimeUtc.ToString1() + "-" + f.Length);
        } catch {
            return FileOperationResult.DoRetry;
        }
    }

    private static FileOperationResult TryLoadAllBytes(params object[] args) {
        if (args.Length < 1 || args[0] is not string filename) {
            return new(Array.Empty<byte>());
        }

        if (string.IsNullOrWhiteSpace(filename)) {
            return new(Array.Empty<byte>());
        }

        try {
            // Prüfen ob Datei existiert
            if (TryFileExists(filename).ReturnValue is not true) {
                return new(Array.Empty<byte>());
            }

            // Bytes laden
            var bytes = File.ReadAllBytes(filename);
            return new(bytes);
        } catch (IOException) {
            return FileOperationResult.DoRetry;  // Retry bei I/O-Fehlern
        } catch (UnauthorizedAccessException) {
            return new(Array.Empty<byte>()); // Kein Retry bei Berechtigungsfehlern
        } catch {
            return new(Array.Empty<byte>()); // Keine Retry bei anderen Fehlern
        }
    }

    private static FileOperationResult TryLoadAndUnzipAllBytes(params object[] args) {
        if (args.Length < 1 || args[0] is not string filename) {
            return new((Array.Empty<byte>(), string.Empty, true));
        }

        try {
            // Direkter Aufruf der Try-Methode anstatt GetFileInfo
            var result = TryGetFileState(filename);
            var fileinfo = result.ReturnValue as string ?? string.Empty;

            if (string.IsNullOrEmpty(fileinfo)) {
                return new((Array.Empty<byte>(), string.Empty, true), result.Retry);
            }

            var bLoaded = File.ReadAllBytes(filename);

            if (bLoaded.IsZipped()) {
                bLoaded = bLoaded.UnzipIt() ?? bLoaded;
            }

            return new((bLoaded, fileinfo, false));
        } catch (IOException) {
            return FileOperationResult.DoRetry;  // Retry bei IO-Fehlern
        } catch {
            return new((Array.Empty<byte>(), string.Empty, true)); // Keine Retry bei anderen Fehlern
        }
    }

    private static FileOperationResult TryMoveDirectory(params object[] args) {
        if (args.Length < 2 || args[0] is not string oldName || args[1] is not string newName) { return FileOperationResult.ValueFalse; }

        if (oldName == newName) { return FileOperationResult.ValueTrue; }
        if (TryDirectoryExists(oldName).ReturnValue is not true) { return FileOperationResult.ValueFalse; }
        if (TryDirectoryExists(newName).ReturnValue is true) { return FileOperationResult.ValueFalse; }

        try {
            Directory.Move(oldName, newName);
            RemoveFromCanWriteCache(oldName);
            RemoveFromCanWriteCache(newName);

            // Warten, bis das Verzeichnis am neuen Ort existiert
            for (var i = 0; i < _fileExistenceCheckRetryCount; i++) {
                if (TryDirectoryExists(newName).ReturnValue is true && TryDirectoryExists(oldName).ReturnValue is not true) { return FileOperationResult.ValueTrue; }
                Thread.Sleep(200);
            }
            return new(TryDirectoryExists(newName).ReturnValue is true && TryDirectoryExists(oldName).ReturnValue is not true);
        } catch {
            return FileOperationResult.DoRetry;
        }
    }

    private static FileOperationResult TryMoveFile(params object[] args) {
        if (args.Length < 2 || args[0] is not string oldName || args[1] is not string newName) { return FileOperationResult.ValueFalse; }

        if (oldName == newName) { return FileOperationResult.ValueTrue; }
        if (TryFileExists(oldName).ReturnValue is not true) { return FileOperationResult.ValueFalse; }
        if (TryFileExists(newName).ReturnValue is true) { return FileOperationResult.ValueFalse; }

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
                if (TryFileExists(newName).ReturnValue is true && TryFileExists(oldName).ReturnValue is not true) { return FileOperationResult.ValueTrue; }
                Thread.Sleep(200);
            }

            return new(TryFileExists(newName).ReturnValue is true && TryFileExists(oldName).ReturnValue is not true);
        } catch {
            return FileOperationResult.DoRetry;
        }
    }

    private static FileOperationResult TryReadAllText(params object[] args) {
        if (args.Length < 3 || args[0] is not string filename || args[1] is not FileShare share || args[2] is not Encoding encoding) {
            return new(string.Empty);
        }

        if (string.IsNullOrWhiteSpace(filename)) { return new(string.Empty); }

        try {
            // Prüfen ob Datei existiert
            if (TryFileExists(filename).ReturnValue is not true) { return new(string.Empty); }

            //// Text aus Datei lesen
            //var content = File.ReadAllText(filename, encoding);
            using var reader = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, share), encoding);
            var content = reader.ReadToEnd();

            return new(content);
        } catch (IOException) {
            return FileOperationResult.DoRetry;  // Retry bei IO-Fehlern
        } catch (UnauthorizedAccessException) {
            return new(string.Empty);  // Kein Retry bei Berechtigungsfehlern
        } catch {
            return new(string.Empty); // Keine Retry bei anderen Fehlern
        }
    }

    private static FileOperationResult TryWaitForCopiedFile(params object[] args) {
        if (args.Length < 2 || args[0] is not string targetFile || args[1] is not long expectedSize) { return FileOperationResult.ValueFalse; }

        if (TryFileExists(targetFile).ReturnValue is true) {
            try {
                var targetInfo = new FileInfo(targetFile);
                if (targetInfo.Length == expectedSize) {
                    return FileOperationResult.ValueTrue;
                }
            } catch {
                // Bei Fehler beim Dateizugriff retry
            }
        }
        return FileOperationResult.DoRetry;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="args">Filename, Byte[]</param>
    /// <returns></returns>
    private static FileOperationResult TryWriteAllBytes(params object[] args) {
        if (args.Length < 2 || args[0] is not string filename || args[1] is not byte[] bytes) {
            return FileOperationResult.ValueFalse;
        }

        try {
            if (Develop.AllReadOnly) { return FileOperationResult.ValueTrue; }

            filename = filename.CheckFile();

            var pfad = filename.FilePath();
            if (!CreateDirectory(pfad)) { return FileOperationResult.ValueFalse; }

            // Prüfen ob wir schreiben können
            if (!CanWrite(filename)) { return FileOperationResult.ValueFalse; }

            using FileStream fs = new(filename, FileMode.Create, FileAccess.Write, FileShare.None);
            fs.Write(bytes, 0, bytes.Length);
            fs.Flush();
            fs.Close();

            //File.WriteAllBytes(filename, bytes);
            return FileOperationResult.ValueTrue;
        } catch (UnauthorizedAccessException) {
            return FileOperationResult.ValueFalse; // Kein Retry bei Berechtigungsfehlern
        } catch {
            return FileOperationResult.DoRetry;  // Retry bei anderen Fehlern
        }
    }

    #endregion
}