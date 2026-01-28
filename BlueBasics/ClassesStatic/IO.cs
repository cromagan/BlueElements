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

public static class IO {

    #region Fields

    /// <summary>
    /// Wird verwendet für File-Dialoge und ist nicht weiter relevant.
    /// </summary>
    public static string LastFilePath = string.Empty;

    private static readonly ConcurrentDictionary<string, (DateTime CheckTime, bool Result)> _canWriteCache = new();
    private static readonly object _fileOperationLock = new();

    private static readonly int _retryCount = 20;

    private static readonly int _retrySleep = 100;

    #endregion

    #region Delegates

    public delegate FileOperationResult DoThis(List<string> affectingFiles, params object?[] args);

    #endregion

    #region Methods

    /// <summary>
    /// Prüft, ob eine Datei gespeichert werden kann, basierend auf Dateizugriff und zeitlichen Beschränkungen
    /// </summary>
    /// <param name="filename">Der Pfad zur zu prüfenden Datei</param>
    /// <param name="recentWriteThresholdSeconds">Schwellwert in Sekunden für kürzliche Schreibvorgänge</param>
    /// <returns>Leerer String bei Erfolg, ansonsten Fehlermeldung</returns>
    public static FileOperationResult CanSaveFile(string filename, int recentWriteThresholdSeconds) {
        if (string.IsNullOrEmpty(filename)) { return new("Kein Dateiname angegeben.", false, true); }

        // Prüfen ob Datei schreibbar ist
        if (!CanWrite(filename)) { return new("Windows blockiert die Datei.", true, true); }

        if (recentWriteThresholdSeconds > 0) {
            // Prüfen ob kürzlich geschrieben wurde
            try {
                var fileInfo = new FileInfo(filename);
                if (DateTime.UtcNow.Subtract(fileInfo.LastWriteTimeUtc).TotalSeconds < recentWriteThresholdSeconds) {
                    return new("Anderer Speichervorgang noch nicht abgeschlossen.", true, true);
                }
            } catch {
                return new("Dateizugriffsfehler.", false, true);
            }
        }
        return FileOperationResult.ValueStringEmpty;
    }

    public static bool CanWrite(string filename) => ProcessFile(TryCanWrite, [filename], false, 5) is true;

    public static bool CanWriteInDirectory(string directory) {
        if (string.IsNullOrEmpty(directory)) { return false; }

        directory = directory.NormalizePath();

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

    /// <summary>
    /// Erstellt ein Verzeichnis, mit Fehlerbehandlung und Wiederholungsversuchen
    /// </summary>
    /// <param name="directory">Das zu erstellende Verzeichnis</param>
    /// <returns>True, wenn das Verzeichnis (dann) existiert</returns>
    public static bool CreateDirectory(string directory) => ProcessFile(TryCreateDirectory, [directory], false, 5) is true;

    public static bool DeleteDir(string directory, bool abortIfFailed) => ProcessFile(TryDeleteDir, [directory], abortIfFailed, abortIfFailed ? 60 : 5) is true;

    /// <summary>
    /// Löscht eine Liste von Dateien
    /// </summary>
    /// <param name="filelist">Liste der zu löschenden Dateien</param>
    /// <returns>True, wenn mindestens eine Datei gelöscht wurde.</returns>
    public static bool DeleteFile(IEnumerable<string>? filelist) {
        if (filelist is not { }) { return false; }

        var lockMe = new object();
        var did = false;

        Parallel.ForEach(filelist, thisf => {
            if (FileExists(thisf)) {
                if (DeleteFile(thisf, false)) {
                    lock (lockMe) {
                        did = true;
                    }
                }
            }
        });

        return did;
    }

    public static bool DeleteFile(string filename, bool abortIfFailed) => ProcessFile(TryDeleteFile, [filename], abortIfFailed, abortIfFailed ? 60 : 5) is true;

    /// <summary>
    /// Prüft, ob ein Verzeichnis existiert, mit zusätzlichen Prüfungen und Fehlerbehandlung
    /// </summary>
    /// <param name="directory">Der zu prüfende Pfad</param>
    /// <returns>True, wenn das Verzeichnis existiert</returns>
    public static bool DirectoryExists(string directory) => ProcessFile(TryDirectoryExists, [directory], false, 5) is true;

    public static bool DirectoryMove(string oldName, string newName, bool abortIfFailed) => ProcessFile(TryMoveDirectory, [oldName, newName], abortIfFailed, abortIfFailed ? 60 : 5) is true;

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

    public static bool FileCopy(string source, string target, bool abortIfFailed) => ProcessFile(TryFileCopy, [source, target], abortIfFailed, abortIfFailed ? 60 : 5) is true;

    /// <summary>
    /// Prüft, ob eine Datei existiert, mit Fehlerbehandlung und Wiederholungsversuchen
    /// </summary>
    /// <param name="file">Die zu prüfende Datei</param>
    /// <returns>True, wenn die Datei existiert</returns>
    public static bool FileExists(string? file) => ProcessFile(TryFileExists, [file ?? string.Empty], false, 5) is true;

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

    public static string[] GetDirectories(string directory) => ProcessFile(TryGetDirectories, [directory], false, 5, "*", SearchOption.TopDirectoryOnly) as string[] ?? [];

    public static string[] GetDirectories(string directory, string pattern, SearchOption suchOption) => ProcessFile(TryGetDirectories, [directory], false, 5, pattern, suchOption) as string[] ?? [];

    public static FileInfo? GetFileInfo(string filename) => ProcessFile(TryGetFileInfo, [filename], false, 5) as FileInfo;

    public static string[] GetFiles(string directory, string pattern, SearchOption suchOption)
                            => ProcessFile(TryGetFiles, [directory], false, 5, pattern, suchOption) as string[] ?? [];

    public static string[] GetFiles(string directory)
                         => ProcessFile(TryGetFiles, [directory], false, 5, "*", SearchOption.TopDirectoryOnly) as string[] ?? [];

    /// <summary>
    /// Liefert Dateiinformationen mit Fehlerbehandlung und Wiederholungsversuchen
    /// </summary>
    /// <param name="filename">Der Dateiname</param>
    /// <param name="abortIfFailed"></param>
    /// <param name="time"></param>
    /// <returns>Dateizeitstempel und -größe als String</returns>
    public static string GetFileState(string filename, bool abortIfFailed, float time) => ProcessFile(TryGetFileState, [filename], abortIfFailed, time) as string ?? string.Empty;

    /// <summary>
    /// Verschiebt eine Datei mit erweiterter Fehlerbehandlung und Wartezeit bis die Datei verfügbar ist
    /// </summary>
    /// <param name="oldName">Quellpfad</param>
    /// <param name="newName">Zielpfad</param>
    /// <param name="abortIfFailed">True für garantierte Ausführung (sonst Programmabbruch)</param>
    /// <returns>True bei Erfolg</returns>
    public static bool MoveFile(string oldName, string newName, bool abortIfFailed) => ProcessFile(TryMoveFile, [oldName, newName], abortIfFailed, abortIfFailed ? 60 : 5) is true;

    public static string NormalizeFile(this string pfad) => pfad.FilePath().NormalizePath() + pfad.FileNameWithSuffix();

    /// <summary>
    /// Standard Pfad-Korrekturen. z.B. Doppelte Slashes, Backslashes. Gibt den Pfad mit abschließenden \ zurück.
    /// </summary>
    /// <param name="pfad"></param>
    /// <returns></returns>
    public static string NormalizePath(this string pfad) {
        if (string.IsNullOrEmpty(pfad)) { return string.Empty; } // Kann vorkommen, wenn ein Benutzer einen Pfad per Hand eingeben darf
        if (pfad.Length > 6 && string.Equals(pfad.Substring(0, 7), "http://", StringComparison.OrdinalIgnoreCase)) { return pfad; }
        if (pfad.Length > 7 && string.Equals(pfad.Substring(0, 8), "https://", StringComparison.OrdinalIgnoreCase)) { return pfad; }

        if (pfad.Contains("/")) { pfad = pfad.Replace("/", "\\"); }

        if (pfad.Contains("%")) {
            var homep = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\";
            pfad = pfad.Replace("%homepath%\\", homep, RegexOptions.IgnoreCase);
            pfad = pfad.Replace("%homepath%", homep, RegexOptions.IgnoreCase);
        }

        if (pfad.Length == 0 || !pfad.EndsWith("\\")) { pfad += "\\"; }
        if (pfad.IndexOf("\\\\", 1, StringComparison.Ordinal) > 0) { Develop.DebugPrint("Achtung, Doppelslash: " + pfad); }
        if (pfad.Length > 1 && pfad.Substring(0, 1) == "\\" && pfad.Substring(0, 2) != "\\\\") { Develop.DebugPrint("Achtung, Doppelslash: " + pfad); }

        if (pfad.Length > 1 && pfad.IndexOf(":", 2, StringComparison.Ordinal) > 0) {
            pfad = pfad.Substring(0, 3) + pfad.Substring(3).RemoveChars(":");
        }

        try {
            return Path.GetFullPath(pfad);
        } catch { }

        Develop.AbortAppIfStackOverflow();
        return NormalizePath(pfad);
    }

    /// <summary>
    /// Gibt einen höher gelegenden Ordner mit abschließenden \ zurück
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="anzahlParents"></param>
    /// <returns></returns>
    public static string PathParent(this string directory, int anzahlParents) {
        for (var z = 1; z <= anzahlParents; z++) {
            directory = directory.PathParent();
        }
        return directory;
    }

    /// <summary>
    /// Gibt einen höher gelegenden Ordner mit abschließenden \ zurück
    /// </summary>
    /// <param name="directory"></param>
    public static string PathParent(this string? directory) {
        if (directory is not { }) { return string.Empty; }

        var z = directory.Length;

        while (true) {
            z--;
            if (z <= 1) { return string.Empty; }
            if (directory.Substring(z - 1, 1) == "\\") { return directory.Substring(0, z); }
        }
    }

    /// <summary>
    /// Führt einen Datei-Befehl mit erweiterter Fehlerbehandlung und Wiederholungsversuchen aus
    /// </summary>
    /// <param name="processMethod">Die auszuführende Methode</param>
    /// <param name="affectingFiles"></param>
    /// <param name="abortIfFailed">True für garantierte Ausführung (sonst Programmabbruch)</param>
    /// <param name="trySeconds"></param>
    /// <param name="args">Variable Parameter</param>
    public static object? ProcessFile(DoThis processMethod, List<string> affectingFiles, bool abortIfFailed, float trySeconds, params object?[] args) {
        var startTime = Stopwatch.StartNew();
        var stopw = Stopwatch.StartNew();

        while (true) {
            var result = processMethod(affectingFiles, args);
            if (!result.Retry) {
                if (result.Failed) { return null; }
                return result.ReturnValue;
            }

            // Bei abortIfFailed=true weiter versuchen, aber nach 60 Sekunden eine Fehlermeldung ausgeben
            if (startTime.ElapsedMilliseconds > trySeconds * 1000) {
                var argsStr = string.Join(", ", args.Select(a => a?.ToString() ?? "null"));

                if (abortIfFailed) {
                    Develop.DebugPrint(ErrorType.Error, "Datei-Befehl konnte nicht ausgeführt werden:\r\n" + argsStr);
                }

                return null;
            }

            if (stopw.ElapsedMilliseconds > 5000) {
                var operation = processMethod.Method.Name.Replace("Try", "").Replace("File", "").Replace("Dir", "");
                var fileName = affectingFiles.Count > 0 ? affectingFiles[0].FileNameWithSuffix() ?? "unbekannt" : "unbekannt";
                var mess = string.Empty;
                if (result.ReturnValue is string m) { mess = $"({m})"; }

                Develop.Message(ErrorType.Info, null, Develop.MonitorMessage, ImageCode.Diskette, $"Warte auf Abschluss einer Dateioperation ({operation}) von {fileName}... {mess}", 0);
                stopw = Stopwatch.StartNew();
            }

            Thread.Sleep(200);
        }
    }

    /// <summary>
    /// Lädt alle Bytes aus einer Datei mit automatischer Retry-Logik
    /// </summary>
    /// <param name="filename">Der Pfad zur zu ladenden Datei</param>
    /// <param name="time"></param>
    /// <returns>Die geladenen Bytes oder ein leeres Array bei Fehler</returns>
    public static byte[] ReadAllBytes(string filename, float time) {
        var result = ProcessFile(TryReadAllBytes, [filename], false, time);
        return result as byte[] ?? [];
    }

    /// <summary>
    /// Liest den gesamten Text aus einer Datei mit der angegebenen Kodierung
    /// </summary>
    /// <param name="filename">Der Pfad zur zu lesenden Datei</param>
    /// <param name="encoding">Die zu verwendende Kodierung</param>
    /// <returns>Der gesamte Inhalt der Datei als String</returns>
    public static string ReadAllText(string filename, Encoding encoding) {
        var result = ProcessFile(TryReadAllBytes, [filename], false, 10);
        var b = result as byte[] ?? [];

        // UTF-8 BOM (EF BB BF) entfernen, falls am Anfang vorhanden
        if (b.Length >= 3 && b[0] == 0xEF && b[1] == 0xBB && b[2] == 0xBF) {
            var nb = new byte[b.Length - 3];
            Buffer.BlockCopy(b, 3, nb, 0, nb.Length);
            b = nb;
        }

        return encoding.GetString(b);
    }

    /// <summary>
    /// Lädt Bytes aus einer Datei mit automatischer Retry-Logik und Dekomprimierung
    /// </summary>
    public static ByteData? ReadAndUnzipAllBytes(string filename) => ProcessFile(TryReadAndUnzipAllBytes, [filename], false, 60) as ByteData;

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
    public static string TempFile(string directory, string preferedfilename, string suffix) {
        if (string.IsNullOrEmpty(directory)) { directory = Path.GetTempPath(); }
        if (string.IsNullOrEmpty(suffix)) { suffix = "tmp"; }
        if (string.IsNullOrEmpty(preferedfilename)) { preferedfilename = UserName + DateTime.UtcNow.ToString6(); }
        var z = -1;
        directory = directory.NormalizePath();
        TryCreateDirectory([directory]);
        preferedfilename = preferedfilename.ReduceToChars(Constants.Char_Numerals + " _+-#" + Constants.Char_Buchstaben + Constants.Char_Buchstaben.ToUpperInvariant());

        if (preferedfilename.Length > 80) { preferedfilename = preferedfilename.Substring(0, 80); }

        string? filename;
        do {
            z++;
            filename = z > 0 ? directory + preferedfilename + "_" + z.ToString5() + "." + suffix : directory + preferedfilename + "." + suffix;
        } while (TryFileExists([filename]).ReturnValue is true);
        return filename;
    }

    public static FileOperationResult TryGetFileInfo(List<string> affectingFiles, params object?[] args) {
        if (affectingFiles.Count != 1 || affectingFiles[0] is not { } filename) { return FileOperationResult.ValueFailed; }

        if (string.IsNullOrWhiteSpace(filename)) { return FileOperationResult.ValueFailed; }

        try {
            var fi = new FileInfo(filename);
            return new(fi);
        } catch (UnauthorizedAccessException) {
            return FileOperationResult.ValueFailed;
        } catch (FileNotFoundException) {
            return FileOperationResult.ValueFailed;
        } catch (DirectoryNotFoundException) {
            return FileOperationResult.ValueFailed;
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
    public static bool WriteAllBytes(string filename, byte[] bytes, bool abortIfFailed) => ProcessFile(TryWriteAllBytes, [filename], abortIfFailed, abortIfFailed ? 60 : 5, bytes) is true;

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
            filename = filename.NormalizeFile();

            var pfad = filename.FilePath();
            if (!CreateDirectory(pfad)) { return false; }

            File.WriteAllText(filename, contents, encoding);
            if (executeAfter) { ExecuteFile(filename); }
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
                if (_canWriteCache.Count < 1000) { return; }

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

    private static FileOperationResult TryCanWrite(List<string> affectingFiles, params object?[] args) {
        if (affectingFiles.Count != 1 || affectingFiles[0] is not { } filename) { return FileOperationResult.ValueFailed; }

        filename = filename.NormalizeFile();

        lock (_fileOperationLock) {
            if (!CanWriteInDirectory(filename.FilePath())) { return FileOperationResult.ValueFalse; }

            var fileUpper = filename.ToUpperInvariant();

            // Prüfen, ob wir für diese Datei bereits ein Ergebnis haben und ob es noch gültig ist
            if (_canWriteCache.TryGetValue(fileUpper, out var cacheEntry) &&
                DateTime.UtcNow.Subtract(cacheEntry.CheckTime).TotalSeconds <= 2) {
                return new(cacheEntry.Result);
            }

            // Vor Zugriff auf Cache, diesen ggf. bereinigen
            CleanupCanWriteCache();

            // Wenn kein gültiges Ergebnis vorliegt, führe die Prüfung durch
            var result = false;

            if (TryFileExists(affectingFiles).ReturnValue is true) {
                try {
                    // Versuch, Datei EXKLUSIV zu öffnen
                    using (var obFi = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.Read)) {
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

    private static FileOperationResult TryCreateDirectory(List<string> affectingFiles, params object?[] args) {
        if (affectingFiles.Count != 1 || affectingFiles[0] is not { } directory) { return FileOperationResult.ValueFailed; }
        directory = directory.NormalizePath();

        if (string.IsNullOrEmpty(directory) || !directory.IsFormat(FormatHolder.Filepath)) { return FileOperationResult.ValueFalse; }

        if (TryDirectoryExists(affectingFiles).ReturnValue is true) { return FileOperationResult.ValueTrue; }

        try {
            if (Develop.AllReadOnly) { return FileOperationResult.ValueTrue; }
            Directory.CreateDirectory(directory);
            return FileOperationResult.ValueTrue;
        } catch (IOException) {
            return FileOperationResult.DoRetry;
        } catch {
            return FileOperationResult.ValueFalse;
        }
    }

    private static FileOperationResult TryDeleteDir(List<string> affectingFiles, params object?[] args) {
        if (affectingFiles.Count != 1 || affectingFiles[0] is not { } directory) { return FileOperationResult.ValueFailed; }

        directory = directory.NormalizePath();
        if (TryDirectoryExists(affectingFiles).ReturnValue is not true) { return FileOperationResult.ValueTrue; }

        try {
            if (Develop.AllReadOnly) { return FileOperationResult.ValueTrue; }
            Directory.Delete(directory, true);
            RemoveFromCanWriteCache(directory);

            // Warten, bis die Datei wirklich gelöscht ist
            var count = 0;
            do {
                if (TryDirectoryExists(affectingFiles).ReturnValue is not true) { return FileOperationResult.ValueTrue; }
                if (count++ > _retryCount) { return FileOperationResult.ValueFalse; }
                Thread.Sleep(_retrySleep);
            } while (true);
        } catch {
            return FileOperationResult.DoRetry;
        }
    }

    private static FileOperationResult TryDeleteFile(List<string> affectingFiles, params object?[] args) {
        if (affectingFiles.Count != 1 || affectingFiles[0] is not { } filename) { return FileOperationResult.ValueFailed; }

        if (TryFileExists(affectingFiles).ReturnValue is not true) { return FileOperationResult.ValueTrue; }

        filename = filename.NormalizeFile();

        if (Develop.AllReadOnly) { return FileOperationResult.ValueTrue; }

        // Komisch, manche Dateien können zwar gelöscht werden, die Attribute aber nicht geändert (Berechtigungen?)
        try {
            if (File.GetAttributes(filename).HasFlag(FileAttributes.ReadOnly)) {
                File.SetAttributes(filename, FileAttributes.Normal);
            }
        } catch {
            // Fehler ignorieren - wir probieren trotzdem zu löschen
        }

        try {
            RemoveFromCanWriteCache(filename);
            if (TryCanWrite(affectingFiles).ReturnValue is not true) { return FileOperationResult.ValueFalse; }

            File.Delete(filename);
            RemoveFromCanWriteCache(filename);

            // Warten, bis die Datei wirklich gelöscht ist
            var count = 0;
            do {
                if (TryFileExists(affectingFiles).ReturnValue is not true) { return FileOperationResult.ValueTrue; }
                if (count++ > _retryCount) { return FileOperationResult.ValueFalse; }
                Thread.Sleep(_retrySleep);
            } while (true);
        } catch {
            return FileOperationResult.DoRetry;
        }
    }

    private static FileOperationResult TryDirectoryExists(List<string> affectingFiles, params object?[] args) {
        if (affectingFiles.Count != 1 || affectingFiles[0] is not { } directory) { return FileOperationResult.ValueFailed; }

        if (directory.Length < 3) { return FileOperationResult.ValueFalse; }

        var p = directory.NormalizePath();

        if (!p.IsFormat(FormatHolder.Filepath)) { return FileOperationResult.ValueFalse; }

        try {
            return new(Directory.Exists(p));
        } catch (IOException) {
            return FileOperationResult.DoRetry;  // Netzwerk-IO-Fehler
        } catch (UnauthorizedAccessException) {
            return FileOperationResult.ValueFalse;  // Berechtigungsfehler
        } catch {
            return FileOperationResult.ValueFalse; // Andere Fehler
        }
    }

    private static FileOperationResult TryFileCopy(List<string> affectingFiles, params object?[] args) {
        if (affectingFiles.Count != 2 || affectingFiles[0] is not { } source || affectingFiles[1] is not { } target) { return FileOperationResult.ValueFalse; }

        source = source.NormalizeFile();
        target = target.NormalizeFile();

        if (source == target) { return FileOperationResult.ValueTrue; }
        if (TryFileExists([source]).ReturnValue is not true) { return FileOperationResult.ValueFalse; }
        if (TryFileExists([target]).ReturnValue is true) { return FileOperationResult.ValueFalse; }

        if (TryGetFileInfo([source]).ReturnValue is not FileInfo sourceInfo) { return FileOperationResult.ValueFalse; }

        try {
            if (Develop.AllReadOnly) { return FileOperationResult.ValueTrue; }
            File.Copy(source, target);

            // Warten, bis die Datei wirklich gelöscht ist
            var count = 0;
            do {
                if (TryFileExists([target]).ReturnValue is true &&
                    TryGetFileInfo([target]).ReturnValue is FileInfo targetFileInfo &&
                    targetFileInfo.Length == sourceInfo.Length) { return FileOperationResult.ValueTrue; }

                if (count++ > _retryCount) { return FileOperationResult.ValueFalse; }
                Thread.Sleep(_retrySleep);
            } while (true);
        } catch {
            return FileOperationResult.DoRetry;
        }
    }

    private static FileOperationResult TryFileExists(List<string> affectingFiles, params object?[] args) {
        if (affectingFiles.Count != 1 || affectingFiles[0] is not { } filename) { return FileOperationResult.ValueFailed; }

        if (string.IsNullOrEmpty(filename) || !filename.IsFormat(FormatHolder.FilepathAndName)) { return FileOperationResult.ValueFalse; }

        try {
            return new(File.Exists(filename));
        } catch (IOException) {
            return FileOperationResult.DoRetry;
        } catch (UnauthorizedAccessException) {
            return FileOperationResult.ValueFalse;
        } catch {
            return FileOperationResult.ValueFalse;
        }
    }

    private static FileOperationResult TryGetDirectories(List<string> affectingFiles, params object?[] args) {
        if (affectingFiles.Count != 1 || affectingFiles[0] is not { } directory) { return FileOperationResult.ValueFailed; }

        if (args.Length < 2) { return FileOperationResult.ValueFailed; }
        var pattern = args[0] as string;
        var option = args[1] is SearchOption so ? so : SearchOption.TopDirectoryOnly;

        if (TryDirectoryExists([directory]).ReturnValue is not true) { return new(Array.Empty<string>()); }

        pattern = string.IsNullOrWhiteSpace(pattern) ? "*" : pattern;

        try {
            return new(Directory.GetDirectories(directory, pattern, option));
        } catch (UnauthorizedAccessException) {
            return FileOperationResult.ValueFailed;
        } catch {
            return FileOperationResult.DoRetry;
        }
    }

    private static FileOperationResult TryGetFiles(List<string> affectingFiles, params object?[] args) {
        if (affectingFiles.Count != 1 || affectingFiles[0] is not { } directory) { return FileOperationResult.ValueFailed; }

        if (args.Length < 2) { return FileOperationResult.ValueFailed; }
        var pattern = args[0] as string;
        var option = args[1] is SearchOption so ? so : SearchOption.TopDirectoryOnly;

        if (TryDirectoryExists([directory]).ReturnValue is not true) { return new(Array.Empty<string>()); }

        pattern = string.IsNullOrWhiteSpace(pattern) ? "*" : pattern;

        try {
            return new(Directory.GetFiles(directory, pattern, option));
        } catch (UnauthorizedAccessException) {
            return FileOperationResult.ValueFailed;
        } catch {
            return FileOperationResult.DoRetry;
        }
    }

    private static FileOperationResult TryGetFileState(List<string> affectingFiles, params object?[] args) {
        if (affectingFiles.Count != 1 || affectingFiles[0] is not { } filename) { return FileOperationResult.ValueFailed; }

        try {
            var f = new FileInfo(filename);
            return new(f.LastWriteTimeUtc.ToString1() + "-" + f.Length);
        } catch (UnauthorizedAccessException) {
            return FileOperationResult.ValueFailed;
        } catch {
            return FileOperationResult.DoRetry;
        }
    }

    private static FileOperationResult TryMoveDirectory(List<string> affectingFiles, params object?[] args) {
        if (affectingFiles.Count < 2 || affectingFiles[0] is not { } oldName || affectingFiles[1] is not { } newName) { return FileOperationResult.ValueFalse; }

        oldName = oldName.NormalizePath();
        newName = newName.NormalizePath();

        if (oldName == newName) { return FileOperationResult.ValueTrue; }
        if (TryDirectoryExists([oldName]).ReturnValue is not true) { return FileOperationResult.ValueFalse; }
        if (TryDirectoryExists([newName]).ReturnValue is true) { return FileOperationResult.ValueFalse; }

        try {
            if (Develop.AllReadOnly) { return FileOperationResult.ValueTrue; }
            Directory.Move(oldName, newName);
            RemoveFromCanWriteCache(oldName);
            RemoveFromCanWriteCache(newName);

            // Warten, bis das Verzeichnis wirklich verschoben ist
            var count = 0;
            do {
                if (TryDirectoryExists([newName]).ReturnValue is true &&
                    TryDirectoryExists([oldName]).ReturnValue is not true) { return FileOperationResult.ValueTrue; }

                if (count++ > _retryCount) { return FileOperationResult.ValueFalse; }
                Thread.Sleep(_retrySleep);
            } while (true);
        } catch {
            return FileOperationResult.DoRetry;
        }
    }

    private static FileOperationResult TryMoveFile(List<string> affectingFiles, params object?[] args) {
        if (affectingFiles.Count < 2 || affectingFiles[0] is not { } oldName || affectingFiles[1] is not { } newName) { return FileOperationResult.ValueFalse; }

        oldName = oldName.NormalizeFile();
        newName = newName.NormalizeFile();

        if (oldName == newName) { return FileOperationResult.ValueTrue; }
        if (TryFileExists([oldName]).ReturnValue is not true) { return FileOperationResult.ValueFalse; }
        if (TryFileExists([newName]).ReturnValue is true) { return FileOperationResult.ValueFalse; }

        try {
            if (Develop.AllReadOnly) { return FileOperationResult.ValueTrue; }
            // Sicherstellen, dass das Zielverzeichnis existiert
            var targetDir = Path.GetDirectoryName(newName);
            if (!string.IsNullOrEmpty(targetDir)) { TryCreateDirectory([targetDir]); }

            File.Move(oldName, newName);
            RemoveFromCanWriteCache(oldName);
            RemoveFromCanWriteCache(newName);

            // Warten, bis die Datei wirklich verschoben ist
            var count = 0;
            do {
                if (TryFileExists([newName]).ReturnValue is true &&
                    TryFileExists([oldName]).ReturnValue is not true) { return FileOperationResult.ValueTrue; }

                if (count++ > _retryCount) { return FileOperationResult.ValueFalse; }
                Thread.Sleep(_retrySleep);
            } while (true);
        } catch {
            return FileOperationResult.DoRetry;
        }
    }

    private static FileOperationResult TryReadAllBytes(List<string> affectingFiles, params object?[] args) {
        if (affectingFiles.Count != 1 || affectingFiles[0] is not { } filename) { return FileOperationResult.ValueFailed; }

        if (string.IsNullOrWhiteSpace(filename)) { return FileOperationResult.ValueFailed; }

        try {
            // Prüfen ob Datei existiert
            if (TryFileExists([filename]).ReturnValue is not true) { return FileOperationResult.ValueFailed; }

            // FileStream wirft FileNotFoundException automatisch
            using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);

            // Sicheres Lesen aller Bytes
            var bytes = new byte[fs.Length];
            var totalRead = 0;
            while (totalRead < bytes.Length) {
                var read = fs.Read(bytes, totalRead, bytes.Length - totalRead);
                if (read == 0) { break; } // EOF erreicht
                totalRead += read;
            }

            return new FileOperationResult(bytes);
        } catch (IOException) {
            return FileOperationResult.DoRetry;  // Retry bei I/O-Fehlern
        } catch (UnauthorizedAccessException) {
            return FileOperationResult.ValueFailed;
        } catch {
            return FileOperationResult.DoRetry;
        }
    }

    //private static FileOperationResult TryReadAllText(List<string> affectingFiles, params object?[] args) {
    //    if (affectingFiles.Count != 1 || affectingFiles[0] is not string filename) { return FileOperationResult.ValueFailed; }

    //    if (args.Length < 2 || args[0] is not FileShare share || args[1] is not Encoding encoding) { return FileOperationResult.ValueFailed; }

    //    if (string.IsNullOrWhiteSpace(filename)) { return FileOperationResult.ValueStringEmpty; }

    //    try {
    //        // Prüfen ob Datei existiert
    //        if (TryFileExists([filename]).ReturnValue is not true) { return FileOperationResult.ValueStringEmpty; }

    //        //// Text aus Datei lesen
    //        //var content = File.ReadAllText(filename, encoding);
    //        using var reader = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, share), encoding);
    //        var content = reader.ReadToEnd();

    //        return new(content);
    //    } catch (IOException) {
    //        return FileOperationResult.DoRetry;  // Retry bei IO-Fehlern
    //    } catch (UnauthorizedAccessException) {
    //        return FileOperationResult.ValueFailed;
    //    } catch {
    //        return FileOperationResult.DoRetry;
    //    }
    //}

    private static FileOperationResult TryReadAndUnzipAllBytes(List<string> affectingFiles, params object?[] args) {
        if (affectingFiles.Count != 1 || affectingFiles[0] is not { } filename) { return FileOperationResult.ValueFailed; }

        try {
            // Direkter Aufruf der Try-Methode anstatt GetFileInfo
            var state1 = TryGetFileState([filename]);
            if (state1.Retry) { return FileOperationResult.DoRetry; }
            if (state1.Failed) { return FileOperationResult.ValueFailed; }
            if (state1.ReturnValue is not string fileinfo1 || string.IsNullOrEmpty(fileinfo1)) { return FileOperationResult.DoRetry; }

            var result = TryReadAllBytes([filename]);
            if (result.Retry) { return FileOperationResult.DoRetry; }
            if (result.Failed) { return FileOperationResult.ValueFailed; }

            var bytes = result.ReturnValue as byte[] ?? [];
            if (bytes.IsZipped()) { bytes = bytes.UnzipIt(); }
            if (bytes == null) { return FileOperationResult.ValueFailed; }

            var state2 = TryGetFileState([filename]);
            if (state2.ReturnValue is not string fileinfo2 || fileinfo1 != fileinfo2) { return FileOperationResult.DoRetry; }

            return new(new ByteData(bytes, fileinfo1));
        } catch {
            return FileOperationResult.DoRetry;
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="affectingFiles"></param>
    /// <param name="args">Filename, Byte[]</param>
    /// <returns></returns>
    private static FileOperationResult TryWriteAllBytes(List<string> affectingFiles, params object?[] args) {
        if (affectingFiles.Count != 1 || affectingFiles[0] is not { } filename) { return FileOperationResult.ValueFalse; }

        if (args.Length != 1 || args[0] is not byte[] bytes) { return FileOperationResult.ValueFalse; }

        try {
            if (Develop.AllReadOnly) { return FileOperationResult.ValueTrue; }

            filename = filename.NormalizeFile();

            var directory = filename.FilePath();
            if (TryCreateDirectory([directory]).ReturnValue is not true) { return FileOperationResult.ValueFalse; }

            // Prüfen ob wir schreiben können
            if (TryCanWrite([filename]).ReturnValue is not true) { return FileOperationResult.ValueFalse; }

            using var fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
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