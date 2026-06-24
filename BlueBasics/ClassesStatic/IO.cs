// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Classes;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using static BlueBasics.ClassesStatic.Generic;

namespace BlueBasics.ClassesStatic;

public static class IO {

    #region Fields

    /// <summary>
    /// Maximale Dateigröße in Bytes, die in den Read-Cache aufgenommen wird.
    /// Dateien oberhalb dieser Grenze werden immer direkt gelesen.
    /// </summary>
    private const long MaxCacheableFileSize = 1024 * 1024;

    private static readonly ConcurrentCache<string, (DateTime CheckTime, OperationResult Result)> _canWriteCache = new(StringComparer.OrdinalIgnoreCase, 500);

    private static readonly object _fileOperationLock = new();

    /// <summary>
    /// Read-Cache für kleine Dateien. Key = normalisierter Dateipfad.
    /// Value = (Dateilänge, UTC-LastWriteTime, rohe Bytes).
    /// Treffer-Prüfung: Länge UND LastWriteTime müssen übereinstimmen.
    /// Automatisches Trimmen läuft global über <see cref="ConcurrentCache{TKey, TValue}"/>.
    /// </summary>
    private static readonly ConcurrentCache<string, (long Length, DateTime LastWriteTimeUtc, byte[] Content)> _readCache = new(StringComparer.OrdinalIgnoreCase, 500);

    private static readonly int _retryCount = 20;

    private static readonly int _retrySleep = 100;

    #endregion

    #region Delegates

    public delegate OperationResult DoThis(List<string> affectingFiles, params object?[] args);

    #endregion

    #region Properties

    /// <summary>
    /// Wird verwendet für File-Dialoge und ist nicht weiter relevant.
    /// </summary>
    public static string LastFilePath { get; set; } = string.Empty;

    #endregion

    #region Methods

    /// <summary>
    /// Prüft, ob eine Datei gespeichert werden kann, basierend auf Dateizugriff und zeitlichen Beschränkungen
    /// </summary>
    /// <param name="filename">Der Pfad zur zu prüfenden Datei</param>
    /// <param name="recentWriteThresholdSeconds">Schwellwert in Sekunden für kürzliche Schreibvorgänge</param>
    /// <returns></returns>
    public static OperationResult CanWriteFile(string filename, int recentWriteThresholdSeconds) => ProcessFile(TryCanWriteFile, [filename], false, recentWriteThresholdSeconds + 5, recentWriteThresholdSeconds);

    public static OperationResult CanWriteInDirectory(string directory) {
        if (string.IsNullOrEmpty(directory)) { return OperationResult.Failed($"Verzeichnis '{directory}' existiert nicht"); }

        directory = directory.NormalizePath();

        if (!directory.IsFormat(FormatHolder_Filepath.Instance)) { return OperationResult.Failed($"'{directory}' ist kein gültiger Verzeichnissname"); }

        lock (_fileOperationLock) {
            if (_canWriteCache.TryGetValue(directory, out var cacheEntry) &&
                DateTime.UtcNow.Subtract(cacheEntry.CheckTime).TotalSeconds <= 300) {
                return cacheEntry.Result;
            }

            try {
                var randomFileName = Path.Combine(directory, Path.GetRandomFileName());
                File.Create(randomFileName, 1, FileOptions.DeleteOnClose).Dispose();

                _canWriteCache[directory] = (DateTime.UtcNow, OperationResult.Success);
                return OperationResult.Success;
            } catch (Exception ex) {
                var result = OperationResult.Failed($"Keine Schreibrechte im Verzeichniss '{directory}'.\r\n{ex.Message}");
                _canWriteCache[directory] = (DateTime.UtcNow, result);
                return result;
            }
        }
    }

    /// <summary>
    /// Erstellt ein Verzeichnis, mit Fehlerbehandlung und Wiederholungsversuchen
    /// </summary>
    /// <param name="directory">Das zu erstellende Verzeichnis</param>
    /// <returns>True, wenn das Verzeichnis (dann) existiert</returns>
    public static OperationResult CreateDirectory(string directory) => ProcessFile(TryCreateDirectory, [directory], false, 5);

    public static bool DeleteDir(string directory, bool abortIfFailed) => ProcessFile(TryDeleteDir, [directory], abortIfFailed, abortIfFailed ? 60 : 5).IsSuccessful;

    /// <summary>
    /// Löscht eine Liste von Dateien
    /// </summary>
    /// <param name="filelist">Liste der zu löschenden Dateien</param>
    /// <returns>True, wenn mindestens eine Datei gelöscht wurde.</returns>
    public static bool DeleteFile(IEnumerable<string>? filelist) {
        if (filelist is null) { return false; }

        var results = filelist.AsParallel()
                   .Select(thisf => FileExists(thisf) && DeleteFile(thisf, false))
                   .ToArray();

        return results.Contains(true);
    }

    /// <summary>
    /// Versucht die Datei zu löschen. Das Programm wird nicht abgebrochen.
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="tryForSeconds"></param>
    /// <returns></returns>
    public static bool DeleteFile(string filename, float tryForSeconds) {
        var result = ProcessFile(TryDeleteFile, [filename], false, tryForSeconds, true);
        if (result.IsSuccessful) {
            _readCache.TryRemove(filename.NormalizeFile(), out _);
        }
        return result.IsSuccessful;
    }

    public static bool DeleteFile(string filename, bool abortIfFailed) {
        var result = ProcessFile(TryDeleteFile, [filename], abortIfFailed, abortIfFailed ? 60 : 5, true);
        if (result.IsSuccessful) {
            _readCache.TryRemove(filename.NormalizeFile(), out _);
        }
        return result.IsSuccessful;
    }

    /// <summary>
    /// Versucht die Datei zu löschen. Bei <paramref name="confirmResult"/> = false
    /// wird nach <see cref="File.Delete"/> nicht auf die tatsächliche Löschung gewartet
    /// — geeignet für nicht-kritische Aufräumarbeiten ohne Wartezeit.
    /// </summary>
    public static bool DeleteFile(string filename, float tryForSeconds, bool confirmResult) {
        var result = ProcessFile(TryDeleteFile, [filename], false, tryForSeconds, confirmResult);
        if (result.IsSuccessful || !confirmResult) {
            _readCache.TryRemove(filename.NormalizeFile(), out _);
        }
        return result.IsSuccessful;
    }

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

            // Anpassung für .NET Core / .NET 5+: UseShellExecute muss explizit auf true gesetzt werden,
            // damit Verzeichnisse oder URLs über die Shell (Explorer) geöffnet werden.
            var startInfo = new ProcessStartInfo {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = true
            };

            var process = Process.Start(startInfo);

            if (waitForExit) {
                if (process is null) { return true; }// Windows 8, DANKE!
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
        name = name.Replace('/', '\\');
        var z = name.LastIndexOf('\\');
        return z < 0 ? string.Empty : name[..(z + 1)];
    }

    /// <summary>
    /// Gibt die Zeichen ohne den Punk zurück. Beispiel: txt
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string FileSuffix(this string name) {
        try {
            if (string.IsNullOrEmpty(name)) { return string.Empty; }
            if (!name.Contains('.')) { return string.Empty; }
            var l = Path.GetExtension(name);
            return string.IsNullOrEmpty(l) ? string.Empty : l[1..];
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
                                "DB" or "MDB" or "BDB" or "MBDB" or "TBLH" => FileFormat.Table,
                                "TBLC" => FileFormat.TableChunk,
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
        pathx = pathx.Replace('/', '\\').TrimEnd('\\');
        if (!pathx.Contains('\\')) { return string.Empty; }
        var z = pathx.Length;
        if (z < 2) { return string.Empty; }
        while (true) {
            z--;
            if (pathx[z] == '\\') { return pathx[(z + 1)..]; }
            if (z < 1) { return string.Empty; }
        }
    }

    public static string[] GetDirectories(string directory) => ProcessFile(TryGetDirectories, [directory], false, 5, "*", SearchOption.TopDirectoryOnly).Value as string[] ?? [];

    public static string[] GetDirectories(string directory, string pattern, SearchOption suchOption) => ProcessFile(TryGetDirectories, [directory], false, 5, pattern, suchOption).Value as string[] ?? [];

    public static FileInfo? GetFileInfo(string filename) => ProcessFile(TryGetFileInfo, [filename], false, 5).Value as FileInfo;

    public static FileInfo? GetFileInfo(string filename, bool abortIfFailed, float time) => ProcessFile(TryGetFileInfo, [filename], abortIfFailed, time).Value as FileInfo;

    public static string[] GetFiles(string directory, string pattern, SearchOption suchOption)
                                => ProcessFile(TryGetFiles, [directory], false, 5, pattern, suchOption).Value as string[] ?? [];

    public static string[] GetFiles(string directory)
                         => ProcessFile(TryGetFiles, [directory], false, 5, "*", SearchOption.TopDirectoryOnly).Value as string[] ?? [];

    /// <summary>
    /// Verschiebt eine Datei mit erweiterter Fehlerbehandlung und Wartezeit bis die Datei verfügbar ist
    /// </summary>
    /// <param name="oldName">Quellpfad</param>
    /// <param name="newName">Zielpfad</param>
    /// <param name="abortIfFailed">True für garantierte Ausführung (sonst Programmabbruch)</param>
    /// <returns>True bei Erfolg</returns>
    public static bool MoveFile(string oldName, string newName, bool abortIfFailed) {
        var success = ProcessFile(TryMoveFile, [oldName, newName], abortIfFailed, abortIfFailed ? 60 : 5).IsSuccessful;
        if (success) {
            _readCache.TryRemove(oldName.NormalizeFile(), out _);
            _readCache.TryRemove(newName.NormalizeFile(), out _);
        }
        return success;
    }

    public static string NormalizeFile(this string pfad) => pfad.FilePath().NormalizePath() + pfad.FileNameWithSuffix();

    /// <summary>
    /// Standard Pfad-Korrekturen. z.B. Doppelte Slashes, Backslashes. Gibt den Pfad mit abschließenden \ zurück.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string NormalizePath(this string path) {
        if (string.IsNullOrWhiteSpace(path)) { return path; }

        // 1. Vereinheitlichen der Trennzeichen
        if (path.Contains('/')) {
            path = path.Replace('/', '\\');
        }

        // 2. Platzhalter ersetzen
        if (path.Contains('%')) {
            // AppDocumentPath Logik (Direkt berechnen statt Rekursion)
            var homep = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Develop.AppName());
            // Einfache Ersetzung ohne Rekursion
            path = Regex.Replace(path, Regex.Escape("%appdocumentpath%"), homep, RegexOptions.IgnoreCase);

            // System-Variablen (wie %TEMP%)
            path = Environment.ExpandEnvironmentVariables(path);
        }

        // 3. Sonderfall: Web-URLs (keine Pfad-Normalisierung nötig)
        if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) {
            return path;
        }

        // 4. Relative Pfade schützen (Dein Kernanliegen)
        // Wenn der Pfad nicht "rooted" ist (z.B. "Hallo\"), geben wir ihn direkt zurück.
        if (!Path.IsPathRooted(path)) {
            return path;
        }

        // 5. Validierung von Laufwerksbuchstaben (Doppelpunkt-Korrektur)
        // Verhindert "C::\..." -> "C:\..."
        if (path.Length > 1 && path[1] == ':') {
            var drive = path[..3];
            var rest = path[3..].Replace(":", string.Empty);
            path = $"{drive}{rest}";
        }

        // 6. Abschließender Backslash (nur wenn es kein File-Pfad sein soll)
        if (!path.EndsWith('\\')) {
            path += "\\";
        }

        // 7. Finales Aufräumen über das System (nur für absolute Pfade sicher)
        try {
            // GetFullPath bereinigt jetzt nur noch absolute Pfade (z.B. C:\Temp\..\)
            return Path.GetFullPath(path);
        } catch {
            return path; // Im Fehlerfall den bisher gesäuberten String zurückgeben
        }
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
        if (directory is null) { return string.Empty; }

        var z = directory.Length;

        while (true) {
            z--;
            if (z <= 1) { return string.Empty; }
            if (directory[z - 1] == '\\') { return directory[..z]; }
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
                var filesStr = affectingFiles.Count > 0 ? string.Join(", ", affectingFiles) : "<keine>";

                if (abortIfFailed) {
                    Develop.DebugError($"Datei-Befehl '{processMethod.Method.Name}' konnte nicht ausgeführt werden:\r\n{filesStr}\r\n{result.FailedReason}");
                }

                return OperationResult.Failed(result.FailedReason); // nun als failed, mit dem Original-Grund
            }

            if (stopw.ElapsedMilliseconds > 3000) {
                var operation = processMethod.Method.Name.Replace("Try", string.Empty).Replace("File", string.Empty).Replace("Dir", string.Empty);
                var fileName = affectingFiles.Count > 0 ? affectingFiles[0].FileNameWithSuffix() ?? "unbekannt" : "unbekannt";
                Develop.Message(ErrorType.Info, null, Develop.MonitorMessage, ImageCode.Diskette, $"Warte auf Abschluss einer Dateioperation ({operation}) von {fileName}... {result.FailedReason}", 0);
                stopw = Stopwatch.StartNew();
            }

            Thread.Sleep(200);
        }
    }

    /// <summary>
    /// Lädt alle Bytes aus einer Datei mit automatischer Retry-Logik.
    /// Kleine Dateien werden transparent im Read-Cache gehalten: beim erneuten
    /// Aufruf wird — sofern Dateigröße und LastWriteTime unverändert sind —
    /// der gecachte Inhalt zurückgegeben, ohne Festplatte zu berühren.
    /// </summary>
    /// <param name="filename">Der Pfad zur zu ladenden Datei</param>
    /// <param name="time"></param>
    /// <returns>Die geladenen Bytes oder ein leeres Array bei Fehler</returns>
    public static OperationResult ReadAllBytes(string filename, float time) {
        if (TryGetFromReadCache(filename, out var cached)) { return new OperationResult(cached); }

        var result = ProcessFile(TryReadAllBytes, [filename], false, time);
        if (result.IsSuccessful && result.Value is byte[] bytes) { AddToReadCache(filename, bytes); }
        return result;
    }

    /// <summary>
    /// Liest den gesamten Text aus einer Datei. Das Encoding wird automatisch
    /// anhand des BOMs erkannt (UTF-8, UTF-16 LE/BE, UTF-32 LE/BE). Ist kein
    /// BOM vorhanden, wird UTF-8 als Fallback verwendet.
    /// </summary>
    /// <param name="filename">Der Pfad zur zu lesenden Datei</param>
    /// <returns>Der gesamte Inhalt der Datei als String</returns>
    public static string ReadAllText(string filename) {
        var result = ReadAllBytes(filename, 10);
        var b = result.Value as byte[] ?? [];

        // BOM-Erkennung: Reihenfolge wichtig, da UTF-32 LE (FF FE 00 00)
        // mit UTF-16 LE (FF FE) beginnt und UTF-32 BE (00 00 FE FF) mit
        // UTF-16 BE (FE FF) verwechselt werden kann. Deshalb 4-Byte-BOMs zuerst.
        if (b.Length >= 3 && b[0] == 0xEF && b[1] == 0xBB && b[2] == 0xBF) {
            return Encoding.UTF8.GetString(b, 3, b.Length - 3);
        }
        if (b.Length >= 4 && b[0] == 0xFF && b[1] == 0xFE && b[2] == 0x00 && b[3] == 0x00) {
            return Encoding.UTF32.GetString(b, 4, b.Length - 4);
        }
        if (b.Length >= 4 && b[0] == 0x00 && b[1] == 0x00 && b[2] == 0xFE && b[3] == 0xFF) {
            return new UTF32Encoding(bigEndian: true, byteOrderMark: false).GetString(b, 4, b.Length - 4);
        }
        if (b.Length >= 2 && b[0] == 0xFF && b[1] == 0xFE) {
            return Encoding.Unicode.GetString(b, 2, b.Length - 2);
        }
        if (b.Length >= 2 && b[0] == 0xFE && b[1] == 0xFF) {
            return Encoding.BigEndianUnicode.GetString(b, 2, b.Length - 2);
        }

        return Encoding.UTF8.GetString(b);
    }

    /// <summary>
    /// Liest den gesamten Text aus einer Datei mit dem angegebenen Encoding.
    /// Das Encoding ist verbindlich. Ein vorhandenes BOM wird nur als Offset
    /// verwendet (also übersprungen), nicht um das Encoding zu wechseln.
    /// Für Auto-Detection statt dessen <see cref="ReadAllText(string)"/> verwenden.
    /// </summary>
    /// <param name="filename">Der Pfad zur zu lesenden Datei</param>
    /// <param name="encoding">Die verbindlich zu verwendende Kodierung.</param>
    /// <returns>Der gesamte Inhalt der Datei als String</returns>
    public static string ReadAllText(string filename, Encoding encoding) {
        var result = ReadAllBytes(filename, 10);
        var b = result.Value as byte[] ?? [];

        // BOM dient nur als Offset, das Encoding ist verbindlich.
        // Reihenfolge wichtig: 4-Byte-BOMs (UTF-32) VOR 2-Byte-BOMs (UTF-16) prüfen,
        // da UTF-32 LE (FF FE 00 00) mit UTF-16 LE (FF FE) beginnt.
        var bomOffset = 0;
        if (b.Length >= 3 && b[0] == 0xEF && b[1] == 0xBB && b[2] == 0xBF) {
            bomOffset = 3;
        } else if (b.Length >= 4 && b[0] == 0xFF && b[1] == 0xFE && b[2] == 0x00 && b[3] == 0x00) {
            bomOffset = 4;
        } else if (b.Length >= 4 && b[0] == 0x00 && b[1] == 0x00 && b[2] == 0xFE && b[3] == 0xFF) {
            bomOffset = 4;
        } else if (b.Length >= 2 && b[0] == 0xFF && b[1] == 0xFE) {
            bomOffset = 2;
        } else if (b.Length >= 2 && b[0] == 0xFE && b[1] == 0xFF) {
            bomOffset = 2;
        }

        return encoding.GetString(b, bomOffset, b.Length - bomOffset);
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
    public static string TempFile(string directory, string preferedfilename, string suffix) {
        if (string.IsNullOrEmpty(directory)) { directory = Path.GetTempPath(); }
        if (string.IsNullOrEmpty(suffix)) { suffix = "tmp"; }
        if (string.IsNullOrEmpty(preferedfilename)) { preferedfilename = UserName + DateTime.UtcNow.ToString6(); }
        var z = -1;
        directory = directory.NormalizePath();
        TryCreateDirectory([directory]);
        preferedfilename = preferedfilename.ReduceToChars(Constants.Char_Numerals + " _+-#" + Constants.Char_Buchstaben + Constants.Char_Buchstaben.ToUpperInvariant());

        if (preferedfilename.Length > 80) { preferedfilename = preferedfilename[..80]; }

        string? filename;
        do {
            z++;
            filename = z > 0 ? $"{directory}{preferedfilename}_{z.ToString5()}.{suffix}" : $"{directory}{preferedfilename}.{suffix}";
        } while (TryFileExists([filename]).Value is true);
        return filename;
    }

    public static OperationResult TryGetFileInfo(List<string> affectingFiles, params object?[] args) {
        if (affectingFiles.Count != 1 || affectingFiles[0] is not { } filename) { return OperationResult.FailedInternalError; }

        if (string.IsNullOrWhiteSpace(filename)) { return OperationResult.Failed("Kein Dateiname angegeben"); }

        var v = TryFileExists([filename]);
        if (v.IsFailed) { return v; }
        if (v.Value is false) { return OperationResult.FailedRetryable("Datei existiert nicht."); }

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
    public static OperationResult WriteAllBytes(string filename, byte[] bytes) => WriteAllBytes(filename, bytes, false);

    /// <summary>
    /// Speichert alle Bytes in eine Datei mit automatischer Retry-Logik und Verzeichnisserstellung
    /// </summary>
    /// <param name="filename">Zieldatei</param>
    /// <param name="bytes">Zu speichernde Bytes</param>
    /// <param name="abortIfFailed">True für garantierte Ausführung (sonst Programmabbruch)</param>
    /// <returns>True bei Erfolg</returns>
    public static OperationResult WriteAllBytes(string filename, byte[] bytes, bool abortIfFailed) {
        var result = ProcessFile(TryWriteAllBytes, [filename], abortIfFailed, abortIfFailed ? 60 : 5, bytes);
        if (result.IsSuccessful) {
            _readCache.TryRemove(filename.NormalizeFile(), out _);
        }
        return result;
    }

    /// <summary>
    /// Speichert den Text in einer Datei.
    /// </summary>
    /// <param name="filename">Zieldatei</param>
    /// <param name="contents">Inhalt</param>
    /// <param name="encoding">Kodierung</param>
    /// <param name="executeAfter">Datei nach dem Speichern ausführen</param>
    /// <returns>True bei Erfolg</returns>
    public static OperationResult WriteAllText(string filename, string contents, Encoding encoding, bool executeAfter) {
        try {
            if (Develop.AllReadOnly) { return OperationResult.Success; }
            filename = filename.NormalizeFile();

            var pfad = filename.FilePath();
            var dirResult = CreateDirectory(pfad);
            if (dirResult.IsFailed) { return dirResult; }

            File.WriteAllText(filename, contents, encoding);
            _readCache.TryRemove(filename, out _);
            if (executeAfter) { ExecuteFile(filename); }
            return OperationResult.Success;
        } catch (Exception ex) {
            return OperationResult.Failed($"Fehler beim Speichern der Datei: {filename}\r\n{ex.Message}");
        }
    }

    /// <summary>
    /// Nimmt eine Datei in den Read-Cache auf, sofern sie klein genug ist.
    /// Dateigröße und LastWriteTimeUtc werden aus dem Dateisystem ermittelt,
    /// damit nachfolgende Lesezugriffe per <see cref="TryGetFromReadCache"/>
    /// auf Aktualität geprüft werden können.
    /// </summary>
    private static void AddToReadCache(string filename, byte[] content) {
        if (string.IsNullOrEmpty(filename)) { return; }
        if (content is null || content.LongLength > MaxCacheableFileSize) { return; }

        var normFile = filename.NormalizeFile();
        try {
            var fi = new FileInfo(normFile);
            if (!fi.Exists) { return; }

            // Defensiv kopieren: der Aufrufer hält evtl. noch eine Referenz auf das
            // Original-Array und könnte es verändern. Der Cache braucht einen eigenen Stand.
            _readCache[normFile] = (fi.Length, fi.LastWriteTimeUtc, (byte[])content.Clone());
        } catch {
            // Cache-Fehler sind nicht kritisch
        }
    }

    /// <summary>
    /// Entfernt Einträge aus dem _canWriteCache basierend auf einer Datei oder einem Verzeichnis
    /// </summary>
    private static void RemoveFromCanWriteCache(string fileOrDirectory) {
        if (string.IsNullOrEmpty(fileOrDirectory)) { return; }

        lock (_fileOperationLock) {
            try {
                var pathUpper = fileOrDirectory.ToUpperInvariant();

                // Alle Cache-Einträge entfernen, die mit diesem Verzeichnispfad beginnen
                var keysToRemove = _canWriteCache.Keys
                    .Where(key => key.StartsWith(pathUpper, StringComparison.Ordinal))
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
            if (t.IsFailed) { return t; }

            // Prüfen, ob wir für diese Datei bereits ein Ergebnis haben und ob es noch gültig ist
            if (_canWriteCache.TryGetValue(filename, out var cacheEntry) &&
                DateTime.UtcNow.Subtract(cacheEntry.CheckTime).TotalSeconds <= 2) {
                return cacheEntry.Result;
            }

            // Wenn kein gültiges Ergebnis vorliegt, führe die Prüfung durch
            if (TryFileExists([filename]).Value is true) {
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
                    using var obFi = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                    // Datei erfolgreich geöffnet, wird am Ende des using-Blocks geschlossen
                } catch {
                    // Bei Fehler ist die Datei in Benutzung.
                    // Nicht cachen und retryable zurückgeben, damit ProcessFile die Datei erneut probieren kann.
                    return OperationResult.FailedRetryable("Die Datei wird von einem anderen Prozess verwendet.");
                }
            }

            // Erfolgsfall im Cache speichern
            _canWriteCache[filename] = (DateTime.UtcNow, OperationResult.Success);
            return OperationResult.Success;
        }
    }

    private static OperationResult TryCreateDirectory(List<string> affectingFiles, params object?[] args) {
        if (affectingFiles.Count != 1 || affectingFiles[0] is not { } directory) { return OperationResult.FailedInternalError; }
        directory = directory.NormalizePath();

        if (!directory.IsFormat(FormatHolder_Filepath.Instance)) { return OperationResult.Failed("Verzeichnisname ungültig"); }

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

            // Warten, bis das Verzeichnis wirklich gelöscht ist
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

        var confirmResult = true;
        if (args is { Length: > 0 } && args[0] is bool c) { confirmResult = c; }

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
            File.Delete(filename);

            if (confirmResult) {
                // Warten, bis die Datei wirklich gelöscht ist
                var count = 0;
                do {
                    if (TryFileExists(affectingFiles).Value is not true) { return OperationResult.Success; }
                    if (count++ > _retryCount) { return OperationResult.Failed("Datei konnte nicht gelöscht werden"); }
                    Thread.Sleep(_retrySleep);
                } while (true);
            }

            return OperationResult.Success;
        } catch (Exception ex) {
            return OperationResult.FailedRetryable(ex);
        }
    }

    private static OperationResult TryDirectoryExists(List<string> affectingFiles, params object?[] args) {
        if (affectingFiles.Count != 1 || affectingFiles[0] is not { } directoryx) { return OperationResult.FailedInternalError; }

        var directory = directoryx.NormalizePath();
        if (!directory.IsFormat(FormatHolder_Filepath.Instance)) { return OperationResult.Failed("Verzeichnisname ungültig"); }

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

            // Warten, bis die Datei wirklich kopiert ist
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

        if (!filename.IsFormat(FormatHolder_FilepathAndName.Instance)) { return OperationResult.Failed("Dateiname ungültig"); }

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

    /// <summary>
    /// Prüft, ob die Datei im Read-Cache vorliegt und aktuell ist.
    /// Aktualitätskriterium: Dateigröße und LastWriteTimeUtc müssen mit dem
    /// Dateisystem übereinstimmen. Bei Abweichung wird der Eintrag verworfen.
    /// </summary>
    private static bool TryGetFromReadCache(string filename, out byte[] content) {
        content = [];
        if (string.IsNullOrEmpty(filename)) { return false; }

        var normFile = filename.NormalizeFile();
        if (!_readCache.TryGetValue(normFile, out var entry)) { return false; }

        try {
            var fi = new FileInfo(normFile);
            if (!fi.Exists) {
                _readCache.TryRemove(normFile, out _);
                return false;
            }
            if (fi.Length != entry.Length || fi.LastWriteTimeUtc != entry.LastWriteTimeUtc) {
                _readCache.TryRemove(normFile, out _);
                return false;
            }
            // Kopie zurückgeben, damit Aufrufer das Array verändern können, ohne den
            // Cache-Eintrag (der bei künftigen Cache-Hits verwendet wird) zu korrumpieren.
            content = (byte[])entry.Content.Clone();
            return true;
        } catch {
            _readCache.TryRemove(normFile, out _);
            return false;
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

                if (count++ > _retryCount) { return OperationResult.Failed("Verschieben fehlgeschlagen"); }
                Thread.Sleep(_retrySleep);
            } while (true);
        } catch (Exception ex) {
            return OperationResult.FailedRetryable(ex);
        }
    }

    private static OperationResult TryReadAllBytes(List<string> affectingFiles, params object?[] args) {
        if (affectingFiles.Count != 1 || affectingFiles[0] is not { } filename) { return OperationResult.FailedInternalError; }

        if (!filename.IsFormat(FormatHolder_FilepathAndName.Instance)) { return OperationResult.Failed("Dateiname ungültig"); }

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
            if (tr.IsFailed) { return tr; }

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