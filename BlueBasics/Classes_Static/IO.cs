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
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
    private const int _fileExistenceCheckRetryCount = 10;
    private const int _fileOperationRetryCount = 5;
    private static readonly ConcurrentDictionary<string, (DateTime CheckTime, bool Result)> _canWriteCache = new();
    private static readonly object _fileOperationLock = new();

    #endregion

    #region Delegates

    private delegate bool DoThis(string file1, string file2);

    #endregion

    #region Methods

    public static string CalculateMd5(string filename) {
        //TODO: Unused
        if (!FileExists(filename)) { return string.Empty; }
        using var md5 = MD5.Create();
        using var stream = File.OpenRead(filename);
        var hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
    }

    public static bool CanWrite(string filename, double tryItForSeconds) {
        if (!CanWriteInDirectory(filename.FilePath())) { return false; }
        var s = DateTime.UtcNow;
        while (true) {
            if (CanWrite(filename)) { return true; }
            if (DateTime.UtcNow.Subtract(s).TotalSeconds > tryItForSeconds) { return false; }
            Thread.Sleep(100); // Kleine Pause um CPU-Last zu reduzieren
        }
    }

    public static bool CanWriteInDirectory(string directory) {
        if (string.IsNullOrEmpty(directory)) { return false; }

        // Sicherstellen, dass Directory mit einem \ endet
        if (!directory.EndsWith("\\")) {
            directory += "\\";
        }

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

    public static string ChecksumFileName(string name) {
        //Used: Only BZL
        name = name.Replace("\\", "}");
        name = name.Replace("/", "}");
        name = name.Replace(":", "}");
        name = name.Replace("?", "}");
        name = name.Replace("\r", string.Empty);
        if (name.Length < 100) { return name; }
        var nn = string.Empty;
        for (var z = 0; z <= name.Length - 21; z++) {
            nn += name.Substring(z, 1);
        }
        nn += name.Substring(name.Length - 20);
        return nn;
    }

    public static bool CopyFile(string source, string target, bool toBeSure) => ProcessFile(TryCopyFile, source, target, _fileOperationRetryCount, toBeSure);

    public static bool DeleteDir(string directory, bool toBeSure) => ProcessFile(TryDeleteDir, directory, directory, _fileOperationRetryCount, toBeSure);

    /// <summary>
    /// Löscht eine Liste von Dateien
    /// </summary>
    /// <param name="filelist">Liste der zu löschenden Dateien</param>
    /// <returns>True, wenn mindestens eine Datei gelöscht wurde.</returns>
    public static bool DeleteFile(IEnumerable<string> filelist) {
        var lockMe = new object();
        var did = false;

        _ = Parallel.ForEach(filelist, thisf => {
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

    public static bool DeleteFile(string file, int tries, bool toBeSure) => ProcessFile(TryDeleteFile, file, file, tries, toBeSure);

    public static bool DeleteFile(string file, bool toBeSure) => !FileExists(file) || ProcessFile(TryDeleteFile, file, file, _fileOperationRetryCount, toBeSure);

    /// <summary>
    /// Prüft, ob ein Verzeichnis existiert, mit zusätzlichen Prüfungen und Fehlerbehandlung
    /// </summary>
    /// <param name="pfad">Der zu prüfende Pfad</param>
    /// <returns>True, wenn das Verzeichnis existiert</returns>
    public static bool DirectoryExists(string pfad) {
        if (pfad.Length < 3) {
            return false;
        }

        var p = pfad.CheckPath();

        // Mehrere Versuche mit exponentieller Verzögerung
        for (var attempt = 0; attempt < _fileOperationRetryCount; attempt++) {
            try {
                return Directory.Exists(p);
            } catch (Exception) {
                // Bei Fehler kurz warten und erneut versuchen
                if (attempt < _fileOperationRetryCount - 1) {
                    Thread.Sleep(100);
                }
            }
        }

        // Wenn alle Versuche fehlschlagen
        return false;
    }

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
    public static bool FileExists(string? file) {
        try {
            return file != null && !string.IsNullOrEmpty(file) && !file.ContainsChars(Constants.Char_PfadSonderZeichen) && File.Exists(file);
        } catch {
            // Objekt wird an anderer stelle benutzt?!?
            Develop.CheckStackOverflow();
            return FileExists(file);
        }
    }

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

    public static string FileNameWithSuffix(this string name) => string.IsNullOrEmpty(name) ? string.Empty : Path.GetFileName(name);

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
    public static string GetFileInfo(string filename, bool mustDo) {
        try {
            FileInfo f = new(filename);
            return f.LastWriteTimeUtc.ToString1() + "-" + f.Length;
        } catch {
            if (!mustDo) { return string.Empty; }
            Develop.CheckStackOverflow();
            Pause(0.5, false);
            return GetFileInfo(filename, mustDo);
        }
    }

    public static bool MoveDirectory(string oldName, string newName, bool toBeSure) => ProcessFile(TryMoveDirectory, oldName, newName, _fileOperationRetryCount, toBeSure);

    /// <summary>
    /// Verschiebt eine Datei mit erweiterter Fehlerbehandlung und Wartezeit bis die Datei verfügbar ist
    /// </summary>
    /// <param name="oldName">Quellpfad</param>
    /// <param name="newName">Zielpfad</param>
    /// <param name="toBeSure">True für garantierte Ausführung (sonst Programmabbruch)</param>
    /// <returns>True bei Erfolg</returns>
    public static bool MoveFile(string oldName, string newName, bool toBeSure) {
        var result = ProcessFile(TryMoveFile, oldName, newName, _fileOperationRetryCount, toBeSure);

        // Nach erfolgreichem Verschieben warten, bis die Datei tatsächlich am Zielort existiert
        if (result) {
            return WaitForFileExists(newName, 1000);
        }

        return false;
    }

    public static bool MoveFile(string oldName, string newName, int tries, bool toBeSure) {
        var result = ProcessFile(TryMoveFile, oldName, newName, tries, toBeSure);

        // Nach erfolgreichem Verschieben warten, bis die Datei tatsächlich am Zielort existiert
        if (result) {
            return WaitForFileExists(newName, 1000);
        }

        return false;
    }

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
    public static string PathParent(this string pfad) {
        //pfad = pfad.CheckPath();
        var z = pfad.Length;

        while (true) {
            z--;
            if (z <= 1) { return string.Empty; }
            if (pfad.Substring(z - 1, 1) == "\\") { return pfad.Substring(0, z); }
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
        if (!DirectoryExists(pfad)) { _ = Directory.CreateDirectory(pfad); }
        wunschname = wunschname.ReduceToChars(Constants.Char_Numerals + " _+-#" + Constants.Char_Buchstaben + Constants.Char_Buchstaben.ToUpperInvariant());

        if (wunschname.Length > 80) { wunschname = wunschname.Substring(0, 80); }

        string? filename;
        do {
            z++;
            filename = z > 0 ? pfad + wunschname + "_" + z.ToStringInt5() + "." + suffix : pfad + wunschname + "." + suffix;
        } while (FileExists(filename));
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
            if (!DirectoryExists(pfad)) { _ = Directory.CreateDirectory(pfad); }

            File.WriteAllText(filename, contents, encoding);
            if (executeAfter) { _ = ExecuteFile(filename); }
            return true;
        } catch {
            //  Develop.DebugPrint(ErrorType.Info, "Fehler beim Speichern der Datei: " + filename, ex);
            return false;
        }
    }

    private static bool CanWrite(string file) {
        // Private lassen, das andere CanWrite greift auf diese zu.
        // Aber das andere prüft zusätzlich die Schreibrechte im Verzeichnis
        lock (_fileOperationLock) {
            var fileUpper = file.ToUpperInvariant();

            // Prüfen, ob wir für diese Datei bereits ein Ergebnis haben und ob es noch gültig ist
            if (_canWriteCache.TryGetValue(fileUpper, out var cacheEntry) &&
                DateTime.UtcNow.Subtract(cacheEntry.CheckTime).TotalSeconds <= 10) {
                return cacheEntry.Result;
            }

            // Vor Zugriff auf Cache, diesen ggf. bereinigen
            CleanupCanWriteCache();

            // Wenn kein gültiges Ergebnis vorliegt, führe die Prüfung durch
            var startTime = DateTime.UtcNow;
            var result = false;

            if (FileExists(file)) {
                try {
                    // Versuch, Datei EXKLUSIV zu öffnen
                    using (FileStream obFi = new(file, FileMode.Open, FileAccess.ReadWrite, FileShare.Read)) {
                        obFi.Close();
                    }
                    result = DateTime.UtcNow.Subtract(startTime).TotalSeconds < 1;
                } catch {
                    // Bei Fehler ist die Datei in Benutzung
                    result = false;
                }
            }

            // Ergebnis im Cache speichern
            _canWriteCache[fileUpper] = (DateTime.UtcNow, result);

            return result;
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
    /// Führt einen Datei-Befehl mit erweiterter Fehlerbehandlung und Wiederholungsversuchen aus
    /// </summary>
    /// <param name="processMethod">Die auszuführende Methode</param>
    /// <param name="file1">Erster Dateipfad (Quelle)</param>
    /// <param name="file2">Zweiter Dateipfad (Ziel)</param>
    /// <param name="tries">Maximale Anzahl von Versuchen</param>
    /// <param name="toBeSure">True für garantierte Ausführung (sonst Programmabbruch)</param>
    /// <returns>True bei Erfolg</returns>
    private static bool ProcessFile(DoThis processMethod, string file1, string file2, int tries, bool toBeSure) {
        if (Develop.AllReadOnly) { return true; }

        // Bei gleichen Dateinamen direkt true zurückgeben
        if (file1 == file2 && processMethod != TryDeleteFile && processMethod != TryDeleteDir) {
            return true;
        }

        var mytry = 0;
        var startTime = DateTime.UtcNow;

        while (!processMethod(file1, file2)) {
            mytry++;

            // Wenn maximale Anzahl Versuche erreicht ist
            if (mytry > tries) {
                if (!toBeSure) { return false; }

                // Bei toBeSure=true weiter versuchen, aber nach 60 Sekunden eine Fehlermeldung ausgeben
                if (DateTime.UtcNow.Subtract(startTime).TotalSeconds > 60) {
                    Develop.DebugPrint(ErrorType.Error, "Datei-Befehl konnte nicht ausgeführt werden:\r\n" + file1 + "\r\n" + file2);
                    return false;
                }
            }
            Thread.Sleep(200);
        }

        return true;
    }

    private static bool TryCopyFile(string source, string target) {
        if (source == target) { return true; }
        if (!FileExists(source)) { return false; }
        if (FileExists(target)) { return false; }

        try {
            var sourceInfo = new FileInfo(source);
            File.Copy(source, target);

            // Warten, bis die Datei tatsächlich am Zielort existiert
            for (var i = 0; i < _fileExistenceCheckRetryCount; i++) {
                if (FileExists(target)) {
                    // Zusätzlich prüfen, ob die Zieldatei die gleiche Größe hat wie die Quelldatei
                    var targetInfo = new FileInfo(target);
                    if (targetInfo.Length == sourceInfo.Length) {
                        return true; // Datei existiert und hat die richtige Größe
                    }
                }
                Thread.Sleep(200);
            }
        } catch {
            return false;
        }

        return false;
    }

    private static bool TryDeleteDir(string pfad, string _) {
        pfad = pfad.CheckPath();
        if (!DirectoryExists(pfad)) { return true; }

        try {
            Directory.Delete(pfad, true);

            // Warten, bis das Verzeichnis wirklich gelöscht ist
            for (var i = 0; i < _fileExistenceCheckRetryCount; i++) {
                if (!DirectoryExists(pfad)) { return true; }
                Thread.Sleep(200);
            }
        } catch {
            return false;
        }

        return !DirectoryExists(pfad);
    }

    private static bool TryDeleteFile(string thisFile, string _) {
        if (!FileExists(thisFile)) { return true; }

        // Komisch, manche Dateien können zwar gelöscht werden, die Attribute aber nicht geändert (Berechtigungen?)
        try {
            if (File.GetAttributes(thisFile).HasFlag(FileAttributes.ReadOnly)) {
                File.SetAttributes(thisFile, FileAttributes.Normal);
            }
        } catch {
            // Fehler ignorieren - wir probieren trotzdem zu löschen
        }

        try {
            if (!CanWrite(thisFile, 0.5)) {
                return false; // Wenn die Datei nicht verfügbar ist, gleich abbrechen
            }

            File.Delete(thisFile);

            // Warten, bis die Datei wirklich gelöscht ist
            for (var i = 0; i < _fileExistenceCheckRetryCount; i++) {
                if (!FileExists(thisFile)) {
                    return true;
                }
                Thread.Sleep(200);
            }
        } catch {
            return false;
        }

        return !FileExists(thisFile);
    }

    private static bool TryMoveDirectory(string oldName, string newName) {
        if (oldName == newName) { return true; }
        if (!DirectoryExists(oldName)) { return false; }
        if (DirectoryExists(newName)) { return false; }

        try {
            Directory.Move(oldName, newName);

            // Warten, bis das Verzeichnis am neuen Ort existiert
            for (var i = 0; i < _fileExistenceCheckRetryCount; i++) {
                if (DirectoryExists(newName) && !DirectoryExists(oldName)) { return true; }
                Thread.Sleep(200);
            }
        } catch {
            return false;
        }

        return DirectoryExists(newName) && !DirectoryExists(oldName);
    }

    private static bool TryMoveFile(string oldName, string newName) {
        if (oldName == newName) { return true; }
        if (!FileExists(oldName)) { return false; }
        if (FileExists(newName)) { return false; }

        try {
            // Sicherstellen, dass das Zielverzeichnis existiert
            var targetDir = Path.GetDirectoryName(newName);
            if (!string.IsNullOrEmpty(targetDir) && !DirectoryExists(targetDir)) {
                Directory.CreateDirectory(targetDir);
            }

            File.Move(oldName, newName);

            // Warten, bis die Datei am neuen Ort existiert und an der alten Position verschwunden ist
            for (var i = 0; i < _fileExistenceCheckRetryCount; i++) {
                if (FileExists(newName) && !FileExists(oldName)) { return true; }
                Thread.Sleep(200);
            }
        } catch {
            return false;
        }

        return FileExists(newName) && !FileExists(oldName);
    }

    /// <summary>
    /// Überprüft, ob eine Datei existiert, und wartet gegebenenfalls bis sie verfügbar ist
    /// </summary>
    /// <param name="file">Die zu prüfende Datei</param>
    /// <param name="timeoutMs">Timeout in Millisekunden (0 = kein Warten)</param>
    /// <returns>True, wenn die Datei existiert</returns>
    private static bool WaitForFileExists(string file, int timeoutMs) {
        if (FileExists(file)) { return true; }

        if (timeoutMs <= 0) { return false; }

        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.ElapsedMilliseconds < timeoutMs) {
            if (FileExists(file)) { return true; }
            Thread.Sleep(200);
        }

        return FileExists(file);
    }

    #endregion
}