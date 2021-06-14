﻿// Authors:
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

using BlueBasics.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using static BlueBasics.modAllgemein;

namespace BlueBasics {

    public static class FileOperations {

        #region Fields

        public static int _canWrite_tryintervall = 10;

        private static readonly List<string> NoWriteAccess = new();

        // private static string LastCheck = string.Empty;
        // private static bool LastErg = false;
        private static readonly List<string> WriteAccess = new();

        private static DateTime _canWrite_LastCheck = DateTime.Now.Subtract(new TimeSpan(10, 10, 10));

        private static string _canWrite_LastFile = string.Empty;

        private static bool _canWrite_LastResult;

        private static string _LastFilePath = string.Empty;

        #endregion

        #region Delegates

        private delegate bool DoThis(string file1, string file2);

        #endregion

        #region Methods

        public static string CalculateMD5(string filename) {
            if (!FileExists(filename)) { return string.Empty; }
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filename);
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
        }

        public static bool CanWrite(string filename, double tryItForSeconds) {
            if (!CanWriteInDirectory(filename.FilePath())) { return false; }
            var s = DateTime.Now;
            while (true) {
                if (CanWrite(filename)) { return true; }
                if (tryItForSeconds < _canWrite_tryintervall) { return false; }
                if (DateTime.Now.Subtract(s).TotalSeconds > tryItForSeconds) { return false; }
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
            var DirUpper = directory.ToUpper();
            if (WriteAccess.Contains(DirUpper)) { return true; }
            if (NoWriteAccess.Contains(DirUpper)) { return false; }
            try {
                using (var fs = File.Create(Path.Combine(directory, Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose)) { }
                WriteAccess.AddIfNotExists(DirUpper); // Multitasking
                return true;
            } catch {
                NoWriteAccess.AddIfNotExists(DirUpper); // Multitasking
                return false;
            }
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

        public static bool CopyFile(string source, string target, bool toBeSure) => ProcessFile(TryCopyFile, source, target, toBeSure);

        public static bool DeleteDir(string directory, bool toBeSure) => ProcessFile(TryDeleteDir, directory, directory, toBeSure);

        /// <summary>
        ///
        /// </summary>
        /// <param name="filelist"></param>
        /// <returns>True, wenn mindestens eine Datei gelöscht wurde.</returns>
        public static bool DeleteFile(List<string> filelist) {
            for (var Z = 0; Z < filelist.Count; Z++) {
                if (!FileExists(filelist[Z])) { filelist[Z] = string.Empty; }
            }
            filelist = filelist.SortedDistinctList();
            if (filelist.Count == 0) { return false; }
            var del = false;
            foreach (var ThisFile in filelist) {
                if (DeleteFile(ThisFile, false)) { del = true; }
            }
            return del;
        }

        public static bool DeleteFile(string file, bool toBeSure) => !FileExists(file) || ProcessFile(TryDeleteFile, file, file, toBeSure);

        public static bool ExecuteFile(string fileName, string arguments = null, bool waitForExit = false, bool logException = true) {
            try {
                if (string.IsNullOrEmpty(fileName) && string.IsNullOrEmpty(arguments)) { return false; }
                var Processx = arguments == null ? Process.Start(fileName) : Process.Start(fileName, arguments);
                if (waitForExit) {
                    if (Processx == null) { return true; }// Windows 8, DANKE!
                    Processx.WaitForExit();
                    Processx.Dispose();
                }
            } catch (Exception ex) {
                if (logException) { Develop.DebugPrint("ExecuteFile konnte nicht ausgeführt werden:<br>" + ex.Message + "<br>Datei: " + fileName); }
                return false;
            }
            return true;
        }

        public static bool FileExists(string file) => file != null && !string.IsNullOrEmpty(file) && !file.ContainsChars(Constants.Char_PfadSonderZeichen) && File.Exists(file);

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
                return f.LastWriteTimeUtc.ToString(Constants.Format_Date) + "-" + f.Length.ToString();
            } catch {
                if (!mustDo) { return string.Empty; }
                Develop.CheckStackForOverflow();
                Pause(0.5, false);
                return GetFileInfo(filename, mustDo);
            }
        }

        public static List<string> GetFilesWithFileSelector(string defaultpath, bool multi) {
            if (string.IsNullOrEmpty(_LastFilePath)) {
                if (!string.IsNullOrEmpty(defaultpath)) {
                    _LastFilePath = defaultpath;
                }
            }

            using System.Windows.Forms.OpenFileDialog f = new();
            f.CheckFileExists = true;
            f.CheckPathExists = true;
            f.Multiselect = multi;
            f.InitialDirectory = _LastFilePath;
            f.Title = "Datei hinzufügen:";
            f.ShowDialog();
            if (f.FileNames == null) { return null; }

            if (!multi && f.FileNames.Length != 1) { return null; }
            var x = new List<string>();
            x.AddRange(f.FileNames);
            _LastFilePath = f.FileNames[0].FilePath();
            return x;
        }

        public static bool PathExists(string pfad) => pfad.Length >= 3 && Directory.Exists(pfad.CheckPath());

        public static bool RenameFile(string oldName, string newName, bool toBeSure) => ProcessFile(TryRenameFile, oldName, newName, toBeSure);

        public static void SaveToDisk(string dateiName, string text2Save, bool executeAfter, System.Text.Encoding code) {
            try {
                // switch (DateiName.FileType()) {
                //    case enFileFormat.HTML:
                //    case enFileFormat.XMLFile:
                //        File.WriteAllText(DateiName, Text2Save, Encoding.UTF8);
                //        break;
                // case enFileFormat.ProgrammingCode:
                //        File.WriteAllText(DateiName, Text2Save, Encoding.Unicode);
                //        break;
                // default:
                //        File.WriteAllText(DateiName, Text2Save, Encoding.Defxault);
                //        break;
                // }
                File.WriteAllText(dateiName, text2Save, code);
                if (executeAfter) { ExecuteFile(dateiName); }
            } catch (Exception ex) {
                Develop.DebugPrint(ex);
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

        public static string TempFile() => TempFile(string.Empty, string.Empty, string.Empty);

        public static string TempFile(string pfad, string wunschname, string suffix) {
            if (string.IsNullOrEmpty(pfad)) { pfad = Path.GetTempPath(); }
            if (string.IsNullOrEmpty(suffix)) { suffix = "tmp"; }
            if (string.IsNullOrEmpty(wunschname)) { wunschname = UserName() + DateTime.Now.ToString(Constants.Format_Date6); }
            var z = -1;
            pfad = pfad.TrimEnd("\\") + "\\";
            if (!PathExists(pfad)) { Directory.CreateDirectory(pfad); }
            wunschname = wunschname.RemoveChars(Constants.Char_DateiSonderZeichen);
            string filename;
            do {
                z++;
                filename = z > 0 ? pfad + wunschname + "_" + z.ToString(Constants.Format_Integer5) + "." + suffix : pfad + wunschname + "." + suffix;
            } while (FileExists(filename));
            return filename;
        }

        private static bool CanWrite(string file) {
            // Private lassen, das andere CanWrite greift auf diese zu.
            // Aber das andere prüft zusätzlich die Schreibrechte im Verzeichnis
            // http://www.vbarchiv.net/tipps/tipp_1281.html
            if (_canWrite_LastResult) { _canWrite_LastFile = string.Empty; }
            if (DateTime.Now.Subtract(_canWrite_LastCheck).TotalSeconds > _canWrite_tryintervall) { _canWrite_LastFile = string.Empty; }
            if (_canWrite_LastFile != file.ToUpper()) {
                var StartTime = DateTime.Now;
                if (FileExists(file)) {
                    try {
                        // Versuch, Datei EXKLUSIV zu öffnen
                        using (FileStream obFi = new(file, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                            obFi.Close();
                        }
                        _canWrite_LastResult = Convert.ToBoolean(DateTime.Now.Subtract(StartTime).TotalSeconds < 1);
                    } catch {
                        // Bei Fehler ist die Datei in Benutzung
                        _canWrite_LastResult = false;
                    }
                }
                _canWrite_LastCheck = DateTime.Now;
            }
            _canWrite_LastFile = file.ToUpper();
            return _canWrite_LastResult;
        }

        private static bool ProcessFile(DoThis processMethod, string file1, string file2, bool toBeSure) {
            var tries = 0;
            var startTime = DateTime.Now;
            while (!processMethod(file1, file2)) {
                tries++;
                if (tries > 5) {
                    if (!toBeSure) { return false; }
                    if (DateTime.Now.Subtract(startTime).TotalSeconds > 60) { Develop.DebugPrint(enFehlerArt.Fehler, "Befehl konnte nicht ausgeführt werden, " + file1 + " " + file2); }
                }
            }
            return true;
        }

        private static bool TryCopyFile(string source, string target) {
            if (source == target) { return true; }
            if (!FileExists(source)) { return false; }
            if (FileExists(target)) { return false; }
            try {
                File.Copy(source, target);
            } catch (Exception ex) {
                Develop.DebugPrint(enFehlerArt.Info, ex);
                return false;
            }
            return true; // FileExists(target);
        }

        private static bool TryDeleteDir(string pfad, string willBeIgnored) {
            pfad = pfad.CheckPath();
            if (!PathExists(pfad)) { return true; }
            try {
                Directory.Delete(pfad, true);
            } catch (Exception ex) {
                Develop.DebugPrint(enFehlerArt.Info, ex);
            }
            return !PathExists(pfad);
        }

        private static bool TryDeleteFile(string thisFile, string willbeIgnored) {
            // Komisch, manche Dateien können zwar gelöscht werden, die Attribute aber nicht geändert (Berechtigungen?)
            try {
                if (File.GetAttributes(thisFile).HasFlag(FileAttributes.ReadOnly)) {
                    File.SetAttributes(thisFile, FileAttributes.Normal);
                }
            } catch (Exception ex) {
                Develop.DebugPrint(enFehlerArt.Info, ex);
            }
            try {
                CanWrite(thisFile, 0.5);
                File.Delete(thisFile);
            } catch (Exception ex) {
                Develop.DebugPrint(enFehlerArt.Info, ex);
                return false;
            }
            return !FileExists(thisFile);
        }

        private static bool TryRenameFile(string oldName, string newName) {
            if (oldName == newName) { return true; }
            if (!FileExists(oldName)) { return false; }
            if (FileExists(newName)) { return false; }
            try {
                File.Move(oldName, newName);
            } catch (Exception ex) {
                Develop.DebugPrint(enFehlerArt.Info, ex);
                return false;
            }
            return true; // FileExists(newName) && !FileExists(oldName);
        }

        #endregion
    }
}