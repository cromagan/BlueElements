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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlueBasics.Enums;
using static BlueBasics.Generic;

namespace BlueBasics;

// ReSharper disable once InconsistentNaming
public static class IO {

    #region Fields

    private const int CanWriteTryintervall = 10;
    private static readonly List<string> NoWriteAccess = [];
    private static readonly List<string> WriteAccess = [];
    private static DateTime _canWriteLastCheck = DateTime.UtcNow.Subtract(new TimeSpan(10, 10, 10));
    private static string _canWriteLastFile = string.Empty;
    private static bool _canWriteLastResult;
    private static string _lastFilePath = string.Empty;

    #endregion

    #region Delegates

    private delegate bool DoThis(string file1, string file2);

    #endregion

    #region Methods

    public static string CalculateMd5(string filename) {
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
            if (tryItForSeconds < CanWriteTryintervall) { return false; }
            if (DateTime.UtcNow.Subtract(s).TotalSeconds > tryItForSeconds) { return false; }
        }
    }

    // public static bool CanWriteInDirectory(string DirectoryPath)
    // {
    //    if (string.IsNullOrEmpty(DirectoryPath)) { return false; }
    // var AccessRight = FileSystemRights.CreateFiles;
    // var Allow = false;
    //    var Deny = false;
    // try
    //    {
    //        var rules = Directory.GetAccessControl(DirectoryPath).GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier));
    //        var identity = WindowsIdentity.GetCurrent();
    // foreach (FileSystemAccessRule rule in rules)
    //        {
    //            if (identity.Groups.Contains(rule.IdentityReference))
    //            {
    //                if ((AccessRight & rule.FileSystemRights) == AccessRight)
    //                {
    //                    if (rule.AccessControlType == AccessControlType.Allow) { Allow = true; }
    //                    if (rule.AccessControlType == AccessControlType.Deny) { Deny = true; }
    //                }
    //            }
    //        }
    //    }
    //    catch
    //    {
    //        return false;
    //    }
    // return Allow && !Deny;
    // }
    public static bool CanWriteInDirectory(string directory) {
        if (string.IsNullOrEmpty(directory)) { return false; }
        var dirUpper = directory.ToUpper();
        if (WriteAccess.Contains(dirUpper)) { return true; }
        if (NoWriteAccess.Contains(dirUpper)) { return false; }
        try {
            using (_ = File.Create(Path.Combine(directory, Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose)) { }
            _ = WriteAccess.AddIfNotExists(dirUpper); // Multitasking
            return true;
        } catch {
            _ = NoWriteAccess.AddIfNotExists(dirUpper); // Multitasking
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

        if (pfad.Substring(pfad.Length - 1) != "\\") { pfad += "\\"; }
        if (pfad.IndexOf("\\\\", 1, StringComparison.Ordinal) > 0) { Develop.DebugPrint("Achtung, Doppelslash: " + pfad); }
        if (pfad.Length > 1 && pfad.Substring(0, 1) == "\\" && pfad.Substring(0, 2) != "\\\\") { Develop.DebugPrint("Achtung, Doppelslash: " + pfad); }

        if (pfad.Length > 1 && pfad.IndexOf(":", 2, StringComparison.Ordinal) > 0) {
            pfad = pfad.Substring(0, 3) + pfad.Substring(3).RemoveChars(":");
        }

        try {
            return Path.GetFullPath(pfad);
        } catch { }
        return pfad;
    }

    public static string ChecksumFileName(string name) {
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

    public static bool CopyFile(string source, string target, bool toBeSure) => ProcessFile(TryCopyFile, source, target, 5, toBeSure);

    public static bool DeleteDir(string directory, bool toBeSure) => ProcessFile(TryDeleteDir, directory, directory, 5, toBeSure);

    /// <summary>
    ///
    /// </summary>
    /// <param name="filelist"></param>
    /// <returns>True, wenn mindestens eine Datei gelöscht wurde.</returns>
    public static bool DeleteFile(IEnumerable<string> filelist) {
        var lockMe = new object();
        var did = false;

        _ = Parallel.ForEach(filelist, thisf => {
            if (FileExists(thisf)) {
                _ = DeleteFile(thisf, false);

                if (!did) {
                    lock (lockMe) {
                        did = true;
                    }
                }
            }
        });

        //for (var Z = 0; Z < filelist.Count; Z++) {
        //    if (!FileExists(filelist[Z])) { filelist[Z] = string.Empty; }
        //}
        //filelist = filelist.SortedDistinctList();
        //if (filelist.Count == 0) { return false; }
        //var del = false;
        //foreach (var ThisFile in filelist) {
        //    if (DeleteFile(ThisFile, false)) { del = true; }
        //}
        return did;
    }

    public static bool DeleteFile(string file, bool toBeSure) => !FileExists(file) || ProcessFile(TryDeleteFile, file, file, 5, toBeSure);

    public static bool DirectoryExists(string pfad) => pfad.Length >= 3 && Directory.Exists(pfad.CheckPath());

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

    public static bool FileExists(string? file) {
        try {
            return file != null && !string.IsNullOrEmpty(file) && !file.ContainsChars(Constants.Char_PfadSonderZeichen) && File.Exists(file);
        } catch {
            // Objekt wird an anderer stelle benutzt?!?
            Develop.CheckStackForOverflow();
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
        : filename.FileSuffix().ToUpper() switch {
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
            "DB" or "MDB" or "BDB" or "MBDB" => FileFormat.Database,
            "LNK" or "URL" => FileFormat.Link,
            "BCR" => FileFormat.BlueCreativeFile,
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

    // public static string LoadFromDisk(string DateiName) {
    // switch (DateiName.FileSuffix()) {
    //        case "XML":
    //            return File.ReadAllText(DateiName, Encoding.UTF8);
    //        default:
    //            return File.ReadAllText(DateiName, Encoding.Defxault);
    //    }
    // }
    //public static string LoadFromDiskUTF8(string dateiName)
    //{
    //    return File.ReadAllText(dateiName, Encoding.UTF8);
    //}
    //public static string LoadFromDisk(string dateiName,Encoding code)
    //{
    //    return File.ReadAllText(dateiName, code);
    //}
    public static string GetFileInfo(string filename, bool mustDo) {
        try {
            FileInfo f = new(filename);
            return f.LastWriteTimeUtc.ToString(Constants.Format_Date, CultureInfo.InvariantCulture) + "-" + f.Length;
        } catch {
            if (!mustDo) { return string.Empty; }
            Develop.CheckStackForOverflow();
            Pause(0.5, false);
            return GetFileInfo(filename, mustDo);
        }
    }

    public static List<string>? GetFilesWithFileSelector(string defaultpath, bool multi) {
        if (string.IsNullOrEmpty(_lastFilePath)) {
            if (!string.IsNullOrEmpty(defaultpath)) {
                _lastFilePath = defaultpath;
            }
        }

        using OpenFileDialog f = new();
        f.CheckFileExists = true;
        f.CheckPathExists = true;
        f.Multiselect = multi;
        f.InitialDirectory = _lastFilePath;
        f.Title = "Datei hinzufügen:";
        _ = f.ShowDialog();
        if (f.FileNames == null) { return null; }

        if (!multi && f.FileNames.Length != 1) { return null; }
        var x = new List<string>();
        x.AddRange(f.FileNames);
        if (f.FileNames != null && f.FileNames.GetUpperBound(0) > 0) {
            _lastFilePath = f.FileNames[0].FilePath();
        }

        return x;
    }

    public static bool MoveDirectory(string oldName, string newName, bool toBeSure) => ProcessFile(TryMoveDirectory, oldName, newName, 5, toBeSure);

    public static bool MoveFile(string oldName, string newName, bool toBeSure) => ProcessFile(TryMoveFile, oldName, newName, 5, toBeSure);

    public static bool MoveFile(string oldName, string newName, int tries, bool toBeSure) => ProcessFile(TryMoveFile, oldName, newName, tries, toBeSure);

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
        pfad = pfad.CheckPath();
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

    public static string TempFile(string pfad, string wunschname, string suffix) {
        if (string.IsNullOrEmpty(pfad)) { pfad = Path.GetTempPath(); }
        if (string.IsNullOrEmpty(suffix)) { suffix = "tmp"; }
        if (string.IsNullOrEmpty(wunschname)) { wunschname = UserName + DateTime.UtcNow.ToString(Constants.Format_Date6, CultureInfo.InvariantCulture); }
        var z = -1;
        pfad = pfad.CheckPath();
        if (!DirectoryExists(pfad)) { _ = Directory.CreateDirectory(pfad); }
        wunschname = wunschname.RemoveChars(Constants.Char_DateiSonderZeichen);
        string? filename;
        do {
            z++;
            filename = z > 0 ? pfad + wunschname + "_" + z.ToString(Constants.Format_Integer5) + "." + suffix : pfad + wunschname + "." + suffix;
        } while (FileExists(filename));
        return filename;
    }

    /// <summary>
    /// Speichert den Text. Der Pfad wird - falls es nicht existiert - erstellt.
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="contents"></param>
    /// <param name="encoding"></param>
    /// <param name="executeAfter"></param>
    public static bool WriteAllText(string filename, string contents, Encoding encoding, bool executeAfter) {
        try {
            filename = filename.CheckFile();

            var pfad = filename.FilePath();
            if (!DirectoryExists(pfad)) { _ = Directory.CreateDirectory(pfad); }

            File.WriteAllText(filename, contents, encoding);
            if (executeAfter) { _ = ExecuteFile(filename); }
            return true;
        } catch {
            //  Develop.DebugPrint(FehlerArt.Info, "Fehler beim Speichern der Datei: " + filename, ex);
            return false;
        }
    }

    private static bool CanWrite(string file) {
        // Private lassen, das andere CanWrite greift auf diese zu.
        // Aber das andere prüft zusätzlich die Schreibrechte im Verzeichnis
        // http://www.vbarchiv.net/tipps/tipp_1281.html
        if (_canWriteLastResult) { _canWriteLastFile = string.Empty; }
        if (DateTime.UtcNow.Subtract(_canWriteLastCheck).TotalSeconds > CanWriteTryintervall) { _canWriteLastFile = string.Empty; }
        if (_canWriteLastFile != file.ToUpper()) {
            var startTime = DateTime.UtcNow;
            if (FileExists(file)) {
                try {
                    // Versuch, Datei EXKLUSIV zu öffnen
                    using (FileStream obFi = new(file, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                        obFi.Close();
                    }
                    _canWriteLastResult = DateTime.UtcNow.Subtract(startTime).TotalSeconds < 1;
                } catch {
                    // Bei Fehler ist die Datei in Benutzung
                    _canWriteLastResult = false;
                }
            }
            _canWriteLastCheck = DateTime.UtcNow;
        }
        _canWriteLastFile = file.ToUpper();
        return _canWriteLastResult;
    }

    /// <summary>
    /// Führt einen Datei-Befehl aus (z. B. löschen oder umbenennen)
    /// </summary>
    /// <param name="processMethod"></param>
    /// <param name="file1"></param>
    /// <param name="file2"></param>
    /// <param name="toBeSure">Stellt sicher, dass der Befehl ausgeführt wird. Ansonsten wird das Programm abgebrochen</param>
    /// <returns>True bei Erfolg</returns>
    private static bool ProcessFile(DoThis processMethod, string file1, string file2, int tries, bool toBeSure) {
        var mytry = 0;
        var startTime = DateTime.UtcNow;
        while (!processMethod(file1, file2)) {
            mytry++;
            if (mytry > tries) {
                if (!toBeSure) { return false; }
                if (DateTime.UtcNow.Subtract(startTime).TotalSeconds > 60) { Develop.DebugPrint(FehlerArt.Fehler, "Datei-Befehl konnte nicht ausgeführt werden:\r\n" + file1 + "\r\n" + file2); }
            }
            Pause(0.2, false);
        }
        return true;
    }

    private static bool TryCopyFile(string source, string target) {
        if (source == target) { return true; }
        if (!FileExists(source)) { return false; }
        if (FileExists(target)) { return false; }
        try {
            File.Copy(source, target);
        } catch {
            //Develop.DebugPrint(enFehlerArt.Info, ex);
            return false;
        }
        return true; // FileExists(target);
    }

    private static bool TryDeleteDir(string pfad, string _) {
        pfad = pfad.CheckPath();
        if (!DirectoryExists(pfad)) { return true; }
        try {
            Directory.Delete(pfad, true);
        } catch {
            //Develop.DebugPrint(enFehlerArt.Info, ex);
            return false;
        }
        return true; // Kein PathExist - evtl. hat ein anderer just den Pfad neu erstellt
    }

    private static bool TryDeleteFile(string thisFile, string _) {
        if (!FileExists(thisFile)) { return true; }

        // Komisch, manche Dateien können zwar gelöscht werden, die Attribute aber nicht geändert (Berechtigungen?)
        try {
            if (File.GetAttributes(thisFile).HasFlag(FileAttributes.ReadOnly)) {
                File.SetAttributes(thisFile, FileAttributes.Normal);
            }
        } catch {
            //Develop.DebugPrint(enFehlerArt.Info, ex);
        }
        try {
            CanWrite(thisFile, 0.5);
            File.Delete(thisFile);
        } catch {
            //Develop.DebugPrint(enFehlerArt.Info, ex);
            return false;
        }
        return true;  // Kein Fileexists - evtl. hat ein anderer just den Pfad neu erstellt
    }

    private static bool TryMoveDirectory(string oldName, string newName) {
        if (oldName == newName) { return true; }
        if (!DirectoryExists(oldName)) { return false; }
        if (DirectoryExists(newName)) { return false; }
        try {
            Directory.Move(oldName, newName);
        } catch {
            //Develop.DebugPrint(enFehlerArt.Info, ex);
            return false;
        }
        return true; // FileExists(newName) && !FileExists(oldName);
    }

    private static bool TryMoveFile(string oldName, string newName) {
        if (oldName == newName) { return true; }
        if (!FileExists(oldName)) { return false; }
        if (FileExists(newName)) { return false; }
        try {
            File.Move(oldName, newName);
        } catch {
            //Develop.DebugPrint(enFehlerArt.Info, ex);
            return false;
        }
        return true; // FileExists(newName) && !FileExists(oldName);
    }

    #endregion
}