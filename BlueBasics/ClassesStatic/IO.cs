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

using BlueBasics.Classes;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static BlueBasics.ClassesStatic.Generic;

namespace BlueBasics.ClassesStatic;

public static class IO {

    #region Fields

    /// <summary>
    /// Wird verwendet für File-Dialoge und ist nicht weiter relevant.
    /// </summary>
    public static string LastFilePath = string.Empty;

    private static readonly ConcurrentDictionary<string, (DateTime CheckTime, OperationResult Result)> _canWriteCache = new();
    private static readonly object _fileOperationLock = new();

    private static readonly int _retryCount = 20;

    private static readonly int _retrySleep = 100;

    #endregion

    #region Delegates

    public delegate OperationResult DoThis(List<string> affectingFiles, params object?[] args);

    #endregion

    #region Methods

    /// <summary>
    /// Prüft, ob eine Datei gespeichert werden kann, basierend auf Dateizugriff und zeitlichen Beschränkungen
    /// </summary>
    /// <param name="filename">Der Pfad zur zu prüfenden Datei</param>
    /// <param name="recentWriteThresholdSeconds">Schwellwert in Sekunden für kürzliche Schreibvorgänge</param>
    /// <returns></returns>
    public static string CanWriteFile(string filename, int recentWriteThresholdSeconds) => ProcessFile(TryCanWriteFile, [filename], false, recentWriteThresholdSeconds + 5, recentWriteThresholdSeconds).FailedReason;

    public static string CanWriteInDirectory(string directory) {
        if (string.IsNullOrEmpty(directory)) { return $"Verzeichniss '{directory}' existiert nicht"; }

        directory = directory.NormalizePath();

        if (!directory.IsFormat(FormatHolder.Filepath)) { return $"'{directory}' ist kein gültiger Verzeichnissname"; }

        var dirUpper = directory.ToUpperInvariant();

        // Prüfen, ob Ergebnis bereits im Cache ist und noch gültig
        if (_canWriteCache.TryGetValue(dirUpper, out var cacheEntry) &&
            DateTime.UtcNow.Subtract(cacheEntry.CheckTime).TotalSeconds <= 300) {
            return cacheEntry.Result.FailedReason;
        }

        // Vor Zugriff auf Cache, diesen ggf. bereinigen
        CleanupCanWriteCache();

        try {
            // Temporäre Testdatei mit zufälligem Namen erstellen
            var randomFileName = Path.Combine(directory, Path.GetRandomFileName());
            using (_ = File.Create(randomFileName, 1, FileOptions.DeleteOnClose)) { }

            // Erfolg im Cache speichern
            _canWriteCache[dirUpper] = (DateTime.UtcNow, new OperationResult());
            return string.Empty;
        } catch (Exception ex) {
            // Fehler im Cache speichern
            _canWriteCache[dirUpper] = (DateTime.UtcNow, OperationResult.Failed($"Keine Schreibrechte im Verzeichniss '{directory}'."));
            return ex.ToString();
        }
    }

    /// <summary>
    /// Erstellt ein Verzeichnis, mit Fehlerbehandlung und Wiederholungsversuchen
    /// </summary>
    /// <param name="directory">Das zu erstellende Verzeichnis</param>
    /// <returns>True, wenn das Verzeichnis (dann) existiert</returns>
    public static bool CreateDirectory(string directory) => ProcessFile(TryCreateDirectory, [directory], false, 5).IsSuccessful;

    public static bool DeleteDir(string directory, bool abortIfFailed) => ProcessFile(TryDeleteDir, [directory], abortIfFailed, abortIfFailed ? 60 : 5).IsSuccessful;

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

    /// <summary>
    /// Versucht die Datei zu löschen. Das Programm wird nicht abgebrochen.
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="tryForSeconds"></param>
    /// <returns></returns>
    public static bool DeleteFile(string filename, float tryForSeconds) => ProcessFile(TryDeleteFile, [filename], false, tryForSeconds).IsSuccessful;

    public static bool DeleteFile(string filename, bool abortIfFailed) => ProcessFile(TryDeleteFile, [filename], abortIfFailed, abortIfFailed ? 60 : 5).IsSuccessful;

    /// <summary>
    /// Prüft, ob ein Verzeichnis existiert, mit zusätzlichen Prüfungen und Fehlerbehandlung
    /// </summary>
    /// <param name="directory">Der zu prüfende Pfad</param>
    /// <returns>True, wenn das Verzeichnis existiert</returns>
    public static bool DirectoryExists(string directory) => ProcessFile(TryDirectoryExists, [directory], false, 5).Value is true;

    public static bool DirectoryMove(string oldName, string newName, bool abortIfFailed) => ProcessFile(TryMoveDirectory, [oldName, newName], abortIfFailed, abortIfFailed ? 60 : 5).IsSuccessful;

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

    public static bool FileCopy(string source, string target, bool abortIfFailed) => ProcessFile(TryFileCopy, [source, target], abortIfFailed, abortIfFailed ? 60 : 5).IsSuccessful;

    /// <summary>
    /// Prüft, ob eine Datei existiert, mit Fehlerbehandlung und Wiederholungsversuchen
    /// </summary>
    /// <param name="file">Die zu prüfende Datei</param>
    /// <returns>True, wenn die Datei existiert</returns>
    public static bool FileExists(string? file) => ProcessFile(TryFileExists, [file ?? string.Empty], false, 5).Value is true;

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

    public static string[] GetDirectories(string directory) => ProcessFile(TryGetDirectories, [directory], false, 5, "*", SearchOption.TopDirectoryOnly).Value as string[] ?? [];

    public static string[] GetDirectories(string directory, string pattern, SearchOption suchOption) => ProcessFile(TryGetDirectories, [directory], false, 5, pattern, suchOption).Value as string[] ?? [];

    public static FileInfo? GetFileInfo(string filename) => ProcessFile(TryGetFileInfo, [filename], false, 5).Value as FileInfo;

    public static string[] GetFiles(string directory, string pattern, SearchOption suchOption)
                            => ProcessFile(TryGetFiles, [directory], false, 5, pattern, suchOption).Value as string[] ?? [];

    public static string[] GetFiles(string directory)
                         => ProcessFile(TryGetFiles, [directory], false, 5, "*", SearchOption.TopDirectoryOnly).Value as string[] ?? [];

    /// <summary>
    /// Liefert Dateiinformationen mit Fehlerbehandlung und Wiederholungsversuchen
    /// </summary>
    /// <param name="filename">Der Dateiname</param>
    /// <param name="abortIfFailed"></param>
    /// <param name="time"></param>
    /// <returns>Dateizeitstempel und -größe als String</returns>
    public static string GetFileState(string filename, bool abortIfFailed, float time) => ProcessFile(TryGetFileState, [filename], abortIfFailed, time).Value as string ?? string.Empty;

    /// <summary>
    /// Verschiebt eine Datei mit erweiterter Fehlerbehandlung und Wartezeit bis die Datei verfügbar ist
    /// </summary>
    /// <param name="oldName">Quellpfad</param>
    /// <param name="newName">Zielpfad</param>
    /// <param name="abortIfFailed">True für garantierte Ausführung (sonst Programmabbruch)</param>
    /// <returns>True bei Erfolg</returns>
    public static bool MoveFile(string oldName, string newName, bool abortIfFailed) => ProcessFile(TryMoveFile, [oldName, newName], abortIfFailed, abortIfFailed ? 60 : 5).IsSuccessful;

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
        return pfad.NormalizePath();
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
    public static OperationResult ProcessFile(DoThis processMethod, List<string> affectingFiles, bool abortIfFailed, float trySeconds, params object?[] args) {
        var startTime = Stopwatch.StartNew();
        var stopw = Stopwatch.StartNew();

        while (true) {
            var result = processMethod(affectingFiles, args);
            if (!result.IsRetryable) { return result; }

            // Bei abortIfFailed=true weiter versuchen, aber nach 60 Sekunden eine Fehlermeldung ausgeben
            if (startTime.ElapsedMilliseconds > trySeconds * 1000) {
                var argsStr = string.Join(", ", args.Select(a => a?.ToString() ?? "null"));

                if (abortIfFailed) {
                    Develop.DebugPrint(ErrorType.Error, $"Datei-Befehl konnte nicht ausgeführt werden:\r\n{argsStr}\r\n{result.FailedReason}");
                }

                return OperationResult.Failed(result.FailedReason); // nun als failed, mit dem Original-Grund
            }

            if (stopw.ElapsedMilliseconds > 3000) {
                var operation = processMethod.Method.Name.Replace("Try", "").Replace("File", "").Replace("Dir", "");
                var fileName = affectingFiles.Count > 0 ? affectingFiles[0].FileNameWithSuffix() ?? "unbekannt" : "unbekannt";
                Develop.Message(ErrorType.Info, null, Develop.MonitorMessage, ImageCode.Diskette, $"Warte auf Abschluss einer Dateioperation ({operation}) von {fileName}... {result.FailedReason}", 0);
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
    public static OperationResult ReadAllBytes(string filename, float time) => ProcessFile(TryReadAllBytes, [filename], false, time);

    /// <summary>
    /// Liest den gesamten Text aus einer Datei mit der angegebenen Kodierung
    /// </summary>
    /// <param name="filename">Der Pfad zur zu lesenden Datei</param>
    /// <param name="encoding">Die zu verwendende Kodierung</param>
    /// <returns>Der gesamte Inhalt der Datei als String</returns>
    public static string ReadAllText(string filename, Encoding encoding) {
        var result = ProcessFile(TryReadAllBytes, [filename], false, 10);
        var b = result.Value as byte[] ?? [];

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
    public static OperationResult ReadAndUnzipAllBytes(string filename) => ProcessFile(TryReadAndUnzipAllBytes, [filename], false, 60);

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
        } while (TryFileExists([filename]).Value is true);
        return filename;
    }

    public static OperationResult TryGetFileInfo(List<string> affectingFiles, params object?[] args) {
        if (affectingFiles.Count != 1 || affectingFiles[0] is not { } filename) { return OperationResult.FailedInternalError; }

        if (string.IsNullOrWhiteSpace(filename)) { return OperationResult.Failed("Kein Dateiname angegeben"); }

        try {
            var fi = new FileInfo(filename);
            return new(fi);
        } catch (UnauthorizedAccessException ex) {
            return OperationResult.Failed(ex);
        } catch (FileNotFoundException ex) {
            return OperationResult.Failed(ex);
        } catch (DirectoryNotFoundException ex) {
            return OperationResult.Failed(ex);
        } catch (Exception ex) {
            return OperationResult.FailedRetryable(ex);
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
    /// Speichert alle Bytes in eine Datei mit automatischer Retry-Logik und Verzeichnisserstellung
    /// </summary>
    /// <param name="filename">Zieldatei</param>
    /// <param name="bytes">Zu speichernde Bytes</param>
    /// <param name="abortIfFailed">True für garantierte Ausführung (sonst Programmabbruch)</param>
    /// <returns>True bei Erfolg</returns>
    public static bool WriteAllBytes(string filename, byte[] bytes, bool abortIfFailed) => ProcessFile(TryWriteAllBytes, [filename], abortIfFailed, abortIfFailed ? 60 : 5, bytes).IsSuccessful;

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

    private static OperationResult TryCanWriteFile(List<string> affectingFiles, params object?[] args) {
        if (affectingFiles.Count != 1 || affectingFiles[0] is not { } filename) { return OperationResult.FailedInternalError; }
        if (args.Length != 1 || args[0] is not int recentWriteThresholdSeconds) { return OperationResult.FailedInternalError; }

        filename = filename.NormalizeFile();

        lock (_fileOperationLock) {
            var t = CanWriteInDirectory(filename.FilePath());
            if (!string.IsNullOrEmpty(t)) { return OperationResult.Failed(t); }

            var fileUpper = filename.ToUpperInvariant();

            // Prüfen, ob wir für diese Datei bereits ein Ergebnis haben und ob es noch gültig ist
            if (_canWriteCache.TryGetValue(fileUpper, out var cacheEntry) &&
                DateTime.UtcNow.Subtract(cacheEntry.CheckTime).TotalSeconds <= 2) {
                return cacheEntry.Result;
            }

            // Vor Zugriff auf Cache, diesen ggf. bereinigen
            CleanupCanWriteCache();

            // Wenn kein gültiges Ergebnis vorliegt, führe die Prüfung durch
            var result = string.Empty;

            if (TryFileExists(affectingFiles).Value is true) {
                // Prüfen ob kürzlich geschrieben wurde
                if (recentWriteThresholdSeconds > 0) {
                    try {
                        var fileInfo = new FileInfo(filename);
                        if (DateTime.UtcNow.Subtract(fileInfo.LastWriteTimeUtc).TotalSeconds < recentWriteThresholdSeconds) {
                            return OperationResult.FailedRetryable("Anderer Speichervorgang noch nicht abgeschlossen.");
                        }
                    } catch (Exception ex) {
                        return OperationResult.FailedRetryable(ex);
                    }
                }

                try {
                    // Versuch, Datei EXKLUSIV zu öffnen
                    using (var obFi = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.Read)) {
                        obFi.Close();
                    }
                } catch (Exception ex) {
                    // Bei Fehler ist die Datei in Benutzung
                    result = ex.ToString();
                }
            }

            var opr = new OperationResult(false, result);

            // Ergebnis im Cache speichern
            _canWriteCache[fileUpper] = (DateTime.UtcNow, opr);

            return opr;
        }
    }

    private static OperationResult TryCreateDirectory(List<string> affectingFiles, params object?[] args) {
        if (affectingFiles.Count != 1 || affectingFiles[0] is not { } directory) { return OperationResult.FailedInternalError; }
        directory = directory.NormalizePath();

        if (string.IsNullOrEmpty(directory) || !directory.IsFormat(FormatHolder.Filepath)) { return OperationResult.Failed("Verzeichnisname ungültig"); }

        if (TryDirectoryExists(affectingFiles).Value is true) { return OperationResult.Success; }

        try {
            if (Develop.AllReadOnly) { return OperationResult.Success; }
            Directory.CreateDirectory(directory);
            return OperationResult.Success;
        } catch (IOException ex) {
            return OperationResult.FailedRetryable(ex);
        } catch (Exception ex) {
            return OperationResult.Failed(ex);
        }
    }

    private static OperationResult TryDeleteDir(List<string> affectingFiles, params object?[] args) {
        if (affectingFiles.Count != 1 || affectingFiles[0] is not { } directory) { return OperationResult.FailedInternalError; }

        directory = directory.NormalizePath();
        if (TryDirectoryExists(affectingFiles).Value is not true) { return OperationResult.Success; }

        try {
            if (Develop.AllReadOnly) { return OperationResult.Success; }
            Directory.Delete(directory, true);
            RemoveFromCanWriteCache(directory);

            // Warten, bis die Datei wirklich gelöscht ist
            var count = 0;
            do {
                if (TryDirectoryExists(affectingFiles).Value is not true) { return OperationResult.Success; }
                if (count++ > _retryCount) { return OperationResult.Failed("Verzeichnis konnte nicht gelöscht werden"); }
                Thread.Sleep(_retrySleep);
            } while (true);
        } catch (Exception ex) {
            return OperationResult.FailedRetryable(ex);
        }
    }

    private static OperationResult TryDeleteFile(List<string> affectingFiles, params object?[] args) {
        if (affectingFiles.Count != 1 || affectingFiles[0] is not { } filename) { return OperationResult.FailedInternalError; }

        if (TryFileExists(affectingFiles).Value is not true) { return OperationResult.Success; }

        filename = filename.NormalizeFile();

        if (Develop.AllReadOnly) { return OperationResult.Success; }

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
            var tr = CanWriteFile(filename, 1);
            if (!string.IsNullOrEmpty(tr)) { return OperationResult.Failed(tr); }

            File.Delete(filename);
            RemoveFromCanWriteCache(filename);

            // Warten, bis die Datei wirklich gelöscht ist
            var count = 0;
            do {
                if (TryFileExists(affectingFiles).Value is not true) { return OperationResult.Success; }
                if (count++ > _retryCount) { return OperationResult.Failed("Datei konnte nicht gelöscht werden"); }
                Thread.Sleep(_retrySleep);
            } while (true);
        } catch (Exception ex) {
            return OperationResult.FailedRetryable(ex);
        }
    }

    private static OperationResult TryDirectoryExists(List<string> affectingFiles, params object?[] args) {
        if (affectingFiles.Count != 1 || affectingFiles[0] is not { } directoryx) { return OperationResult.FailedInternalError; }

        var directory = directoryx.NormalizePath();
        if (string.IsNullOrEmpty(directory) || !directory.IsFormat(FormatHolder.Filepath)) { return OperationResult.Failed("Verzeichnisname ungültig"); }

        try {
            return new(Directory.Exists(directory));
        } catch (IOException ex) {
            return OperationResult.FailedRetryable(ex);  // Netzwerk-IO-Fehler
        } catch (UnauthorizedAccessException) {
            return OperationResult.SuccessFalse;  // Berechtigungsfehler
        } catch {
            return OperationResult.SuccessFalse; // Andere Fehler
        }
    }

    private static OperationResult TryFileCopy(List<string> affectingFiles, params object?[] args) {
        if (affectingFiles.Count != 2 || affectingFiles[0] is not { } source || affectingFiles[1] is not { } target) { return OperationResult.FailedInternalError; }

        source = source.NormalizeFile();
        target = target.NormalizeFile();

        if (source == target) { return OperationResult.Success; }
        if (TryFileExists([source]).Value is not true) { return OperationResult.Failed("Quell-Datei existiert nicht."); }
        if (TryFileExists([target]).Value is true) { return OperationResult.Failed("Ziel-Datei existiert bereits."); }

        if (TryGetFileInfo([source]).Value is not FileInfo sourceInfo) { return OperationResult.FailedInternalError; }

        try {
            if (Develop.AllReadOnly) { return OperationResult.Success; }
            File.Copy(source, target);

            // Warten, bis die Datei wirklich gelöscht ist
            var count = 0;
            do {
                if (TryFileExists([target]).Value is true &&
                    TryGetFileInfo([target]).Value is FileInfo targetFileInfo &&
                    targetFileInfo.Length == sourceInfo.Length) { return OperationResult.Success; }

                if (count++ > _retryCount) { return OperationResult.Failed("Fehler im Kopier-Vorgang"); }
                Thread.Sleep(_retrySleep);
            } while (true);
        } catch (Exception ex) {
            return OperationResult.FailedRetryable(ex);
        }
    }

    private static OperationResult TryFileExists(List<string> affectingFiles, params object?[] args) {
        if (affectingFiles.Count != 1 || affectingFiles[0] is not { } filename) { return OperationResult.FailedInternalError; }

        if (string.IsNullOrEmpty(filename) || !filename.IsFormat(FormatHolder.FilepathAndName)) { return OperationResult.Failed("Dateiname ungültig"); }

        try {
            return new(File.Exists(filename));
        } catch (IOException ex) {
            return OperationResult.FailedRetryable(ex);
        } catch (UnauthorizedAccessException) {
            return OperationResult.SuccessFalse;
        } catch {
            return OperationResult.SuccessFalse;
        }
    }

    private static OperationResult TryGetDirectories(List<string> affectingFiles, params object?[] args) {
        if (affectingFiles.Count != 1 || affectingFiles[0] is not { } directory) { return OperationResult.FailedInternalError; }

        if (args.Length < 2) { return OperationResult.FailedInternalError; }
        var pattern = args[0] as string;
        var option = args[1] is SearchOption so ? so : SearchOption.TopDirectoryOnly;

        if (TryDirectoryExists([directory]).Value is not true) { return new(Array.Empty<string>()); }

        pattern = string.IsNullOrWhiteSpace(pattern) ? "*" : pattern;

        try {
            return new(Directory.GetDirectories(directory, pattern, option));
        } catch (UnauthorizedAccessException ex) {
            return OperationResult.Failed(ex);
        } catch (Exception ex) {
            return OperationResult.FailedRetryable(ex);
        }
    }

    private static OperationResult TryGetFiles(List<string> affectingFiles, params object?[] args) {
        if (affectingFiles.Count != 1 || affectingFiles[0] is not { } directory) { return OperationResult.FailedInternalError; }

        if (args.Length < 2) { return OperationResult.FailedInternalError; }
        var pattern = args[0] as string;
        var option = args[1] is SearchOption so ? so : SearchOption.TopDirectoryOnly;

        if (TryDirectoryExists([directory]).Value is not true) { return new(Array.Empty<string>()); }

        pattern = string.IsNullOrWhiteSpace(pattern) ? "*" : pattern;

        try {
            return new(Directory.GetFiles(directory, pattern, option));
        } catch (UnauthorizedAccessException ex) {
            return OperationResult.Failed(ex);
        } catch (Exception ex) {
            return OperationResult.FailedRetryable(ex);
        }
    }

    private static OperationResult TryGetFileState(List<string> affectingFiles, params object?[] args) {
        if (affectingFiles.Count != 1 || affectingFiles[0] is not { } filename) { return OperationResult.FailedInternalError; }

        try {
            var f = new FileInfo(filename);
            return new(f.LastWriteTimeUtc.ToString1() + "-" + f.Length);
        } catch (UnauthorizedAccessException ex) {
            return OperationResult.Failed(ex);
        } catch (Exception ex) {
            return OperationResult.FailedRetryable(ex);
        }
    }

    private static OperationResult TryMoveDirectory(List<string> affectingFiles, params object?[] args) {
        if (affectingFiles.Count < 2 || affectingFiles[0] is not { } oldName || affectingFiles[1] is not { } newName) { return OperationResult.FailedInternalError; }

        oldName = oldName.NormalizePath();
        newName = newName.NormalizePath();

        if (oldName == newName) { return OperationResult.Success; }
        if (TryDirectoryExists([oldName]).Value is not true) { return OperationResult.Failed("Quell-Verzeichnis existiert nicht"); }
        if (TryDirectoryExists([newName]).Value is true) { return OperationResult.Failed("Ziel-Verzeischniss existiert bereits"); }

        try {
            if (Develop.AllReadOnly) { return OperationResult.Success; }
            Directory.Move(oldName, newName);
            RemoveFromCanWriteCache(oldName);
            RemoveFromCanWriteCache(newName);

            // Warten, bis das Verzeichnis wirklich verschoben ist
            var count = 0;
            do {
                if (TryDirectoryExists([newName]).Value is true &&
                    TryDirectoryExists([oldName]).Value is not true) { return OperationResult.Success; }

                if (count++ > _retryCount) { return OperationResult.Failed("Verschieben fehlgeschlagen"); }
                Thread.Sleep(_retrySleep);
            } while (true);
        } catch (Exception ex) {
            return OperationResult.FailedRetryable(ex);
        }
    }

    private static OperationResult TryMoveFile(List<string> affectingFiles, params object?[] args) {
        if (affectingFiles.Count < 2 || affectingFiles[0] is not { } oldName || affectingFiles[1] is not { } newName) { return OperationResult.FailedInternalError; }

        oldName = oldName.NormalizeFile();
        newName = newName.NormalizeFile();

        if (oldName == newName) { return OperationResult.Success; }
        if (TryFileExists([oldName]).Value is not true) { return OperationResult.Failed("Quell-Datei existiert nicht"); }
        if (TryFileExists([newName]).Value is true) { return OperationResult.Failed("Ziel-Datei existiert bereits"); }

        try {
            if (Develop.AllReadOnly) { return OperationResult.Success; }
            // Sicherstellen, dass das Zielverzeichnis existiert
            var targetDir = Path.GetDirectoryName(newName);
            if (!string.IsNullOrEmpty(targetDir)) { TryCreateDirectory([targetDir]); }

            File.Move(oldName, newName);
            RemoveFromCanWriteCache(oldName);
            RemoveFromCanWriteCache(newName);

            // Warten, bis die Datei wirklich verschoben ist
            var count = 0;
            do {
                if (TryFileExists([newName]).Value is true &&
                    TryFileExists([oldName]).Value is not true) { return OperationResult.Success; }

                if (count++ > _retryCount) { return OperationResult.Failed("Kopieren fehlgeschlagen"); }
                Thread.Sleep(_retrySleep);
            } while (true);
        } catch (Exception ex) {
            return OperationResult.FailedRetryable(ex);
        }
    }

    private static OperationResult TryReadAllBytes(List<string> affectingFiles, params object?[] args) {
        if (affectingFiles.Count != 1 || affectingFiles[0] is not { } filename) { return OperationResult.FailedInternalError; }

        if (string.IsNullOrEmpty(filename) || !filename.IsFormat(FormatHolder.FilepathAndName)) { return OperationResult.Failed("Dateiname ungültig"); }

        try {
            // Prüfen ob Datei existiert
            if (TryFileExists([filename]).Value is not true) { return OperationResult.Failed("Datei existiert nicht"); }

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

            return new OperationResult(bytes);
        } catch (IOException ex) {
            return OperationResult.FailedRetryable(ex);  // Retry bei I/O-Fehlern
        } catch (UnauthorizedAccessException ex) {
            return OperationResult.Failed(ex);
        } catch (Exception ex) {
            return OperationResult.FailedRetryable(ex);
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

    private static OperationResult TryReadAndUnzipAllBytes(List<string> affectingFiles, params object?[] args) {
        if (affectingFiles.Count != 1 || affectingFiles[0] is not { } filename) { return OperationResult.FailedInternalError; }

        try {
            // Direkter Aufruf der Try-Methode anstatt GetFileInfo
            var state1 = TryGetFileState([filename]);
            if (state1.IsFailed) { return state1; }
            if (state1.Value is not string fileinfo1 || string.IsNullOrEmpty(fileinfo1)) { return OperationResult.FailedInternalError; }

            var result = TryReadAllBytes([filename]);
            if (result.IsFailed) { return result; }

            var bytes = result.Value as byte[] ?? [];
            if (bytes.IsZipped()) { bytes = bytes.UnzipIt(); }
            if (bytes == null) { return OperationResult.Failed("Entpacken fehlgeschlagen"); }

            var state2 = TryGetFileState([filename]);
            if (state2.Value is not string fileinfo2 || fileinfo1 != fileinfo2) { return OperationResult.FailedRetryable("Datei inzwischen verändert"); }

            return new(new ByteData(bytes, fileinfo1));
        } catch (Exception ex) {
            return OperationResult.FailedRetryable(ex);
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="affectingFiles"></param>
    /// <param name="args">Filename, Byte[]</param>
    /// <returns></returns>
    private static OperationResult TryWriteAllBytes(List<string> affectingFiles, params object?[] args) {
        if (affectingFiles.Count != 1 || affectingFiles[0] is not { } filename) { return OperationResult.FailedInternalError; }

        if (args.Length != 1 || args[0] is not byte[] bytes) { return OperationResult.FailedInternalError; }

        try {
            if (Develop.AllReadOnly) { return OperationResult.Success; }

            filename = filename.NormalizeFile();

            var directory = filename.FilePath();
            var t = TryCreateDirectory([directory]);
            if (t.IsFailed) { return t; }

            // Prüfen ob wir schreiben können
            var tr = CanWriteFile(filename, 1);
            if (!string.IsNullOrEmpty(tr)) { return OperationResult.Failed(tr); }

            using var fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
            fs.Write(bytes, 0, bytes.Length);
            fs.Flush();
            fs.Close();

            //File.WriteAllBytes(filename, bytes);
            return OperationResult.Success;
        } catch (UnauthorizedAccessException ex) {
            return OperationResult.Failed(ex); // Kein Retry bei Berechtigungsfehlern
        } catch (Exception ex) {
            return OperationResult.FailedRetryable(ex);  // Retry bei anderen Fehlern
        }
    }

    #endregion
}