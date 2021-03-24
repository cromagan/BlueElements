#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using static BlueBasics.modAllgemein;

namespace BlueBasics {

    public static class FileOperations {


        private delegate bool DoThis(string file1, string file2);

        //private static string LastCheck = string.Empty;
        //private static bool LastErg = false;

        private static readonly List<string> WriteAccess = new();
        private static readonly List<string> NoWriteAccess = new();


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


        private static bool TryDeleteDir(string Pfad, string WillBeIgnored) {
            Pfad = Pfad.CheckPath();
            if (!PathExists(Pfad)) { return true; }

            try {
                Directory.Delete(Pfad, true);
            } catch (Exception ex) {
                Develop.DebugPrint(enFehlerArt.Info, ex);
            }

            return !PathExists(Pfad);
        }


        /// <summary>
        /// 
        /// </summary>
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


        public static bool DeleteFile(string file, bool toBeSure) {
            if (!FileExists(file)) { return true; }
            return ProcessFile(TryDeleteFile, file, file, toBeSure);
        }
        public static bool RenameFile(string oldName, string newName, bool toBeSure) {
            return ProcessFile(TryRenameFile, oldName, newName, toBeSure);
        }
        public static bool CopyFile(string source, string target, bool toBeSure) {
            return ProcessFile(TryCopyFile, source, target, toBeSure);
        }

        public static bool DeleteDir(string directory, bool toBeSure) {
            return ProcessFile(TryDeleteDir, directory, directory, toBeSure);
        }



        private static bool TryDeleteFile(string ThisFile, string WillbeIgnored) {
            // Komisch, manche Dateien können zwar gelöscht werden, die Attribute aber nicht geändert (Berechtigungen?)
            try {

                if (File.GetAttributes(ThisFile).HasFlag(FileAttributes.ReadOnly)) {
                    File.SetAttributes(ThisFile, FileAttributes.Normal);
                }

            } catch (Exception ex) {
                Develop.DebugPrint(enFehlerArt.Info, ex);
            }

            try {
                CanWrite(ThisFile, 0.5);
                File.Delete(ThisFile);
            } catch (Exception ex) {
                Develop.DebugPrint(enFehlerArt.Info, ex);
                return false;
            }

            return !FileExists(ThisFile);
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


        public static bool FileExists(string file) {
            if (file == null) { return false; }
            if (string.IsNullOrEmpty(file)) { return false; }
            if (file.ContainsChars(Constants.Char_PfadSonderZeichen)) { return false; }
            return File.Exists(file);
        }

        //public static bool CanWriteInDirectory(string DirectoryPath)
        //{
        //    if (string.IsNullOrEmpty(DirectoryPath)) { return false; }

        //    var AccessRight = FileSystemRights.CreateFiles;

        //    var Allow = false;
        //    var Deny = false;

        //    try
        //    {
        //        var rules = Directory.GetAccessControl(DirectoryPath).GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier));
        //        var identity = WindowsIdentity.GetCurrent();

        //        foreach (FileSystemAccessRule rule in rules)
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

        //    return Allow && !Deny;
        //}


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


        private static DateTime _canWrite_LastCheck = DateTime.Now.Subtract(new TimeSpan(10, 10, 10));
        private static bool _canWrite_LastResult;
        private static string _canWrite_LastFile = string.Empty;
        public static int _canWrite_tryintervall = 10;
        public static bool CanWrite(string filename, double tryItForSeconds) {
            if (!CanWriteInDirectory(filename.FilePath())) { return false; }

            var s = DateTime.Now;
            do {
                if (CanWrite(filename)) { return true; }
                if (tryItForSeconds < _canWrite_tryintervall) { return false; }

                if (DateTime.Now.Subtract(s).TotalSeconds > tryItForSeconds) { return false; }
            } while (true);
        }


        private static bool CanWrite(string file) {
            //Private lassen, das andere CanWrite greift auf diese zu.
            //Aber das andere prüft zusätzlich die Schreibrechte im Verzeichnis
            //http://www.vbarchiv.net/tipps/tipp_1281.html

            if (_canWrite_LastResult) { _canWrite_LastFile = string.Empty; }
            if (DateTime.Now.Subtract(_canWrite_LastCheck).TotalSeconds > _canWrite_tryintervall) { _canWrite_LastFile = string.Empty; }




            if (_canWrite_LastFile != file.ToUpper()) {

                var StartTime = DateTime.Now;

                if (FileExists(file)) {
                    try {
                        // Versuch, Datei EXKLUSIV zu öffnen
                        using (var obFi = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read)) {
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

        public static bool PathExists(string Pfad) {
            if (Pfad.Length < 3) { return false; }
            return Directory.Exists(Pfad.CheckPath());
        }


        public static string TempFile(string NewPath, string Filename) {
            var dn = Filename.FileNameWithoutSuffix();
            var ds = Filename.FileSuffix();
            return TempFile(NewPath, dn, ds);
        }

        public static string TempFile(string FullName) {
            var dp = FullName.FilePath();
            var dn = FullName.FileNameWithoutSuffix();
            var ds = FullName.FileSuffix();
            return TempFile(dp, dn, ds);
        }


        public static string TempFile() {
            return TempFile("", "", "");
        }

        public static string TempFile(string Pfad, string Wunschname, string Suffix) {
            if (string.IsNullOrEmpty(Pfad)) { Pfad = Path.GetTempPath(); }
            if (string.IsNullOrEmpty(Suffix)) { Suffix = "tmp"; }
            if (string.IsNullOrEmpty(Wunschname)) { Wunschname = UserName() + DateTime.Now.ToString(Constants.Format_Date6); }

            var z = -1;
            Pfad = Pfad.TrimEnd("\\") + "\\";

            if (!PathExists(Pfad)) { Directory.CreateDirectory(Pfad); }

            Wunschname = Wunschname.RemoveChars(Constants.Char_DateiSonderZeichen);

            string filename;

            do {
                z++;
                if (z > 0) {
                    filename = Pfad + Wunschname + "_" + z.ToString(Constants.Format_Integer5) + "." + Suffix;
                } else {
                    filename = Pfad + Wunschname + "." + Suffix;
                }
            } while (FileExists(filename));

            return filename;
        }


        public static string CalculateMD5(string filename) {

            if (!FileExists(filename)) { return string.Empty; }

            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filename);
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }



        public static bool ExecuteFile(string FileName, string Arguments = null, bool WaitForExit = false, bool LogException = true) {
            try {
                if (string.IsNullOrEmpty(FileName) && string.IsNullOrEmpty(Arguments)) { return false; }

                Process Processx = null;

                if (Arguments == null) {
                    Processx = Process.Start(FileName);
                } else {
                    Processx = Process.Start(FileName, Arguments);
                }

                if (WaitForExit) {
                    if (Processx == null) { return true; }// Windows 8, DANKE!

                    Processx.WaitForExit();
                    Processx.Dispose();
                }
            } catch (Exception ex) {
                if (LogException) { Develop.DebugPrint("ExecuteFile konnte nicht ausgeführt werden:<br>" + ex.Message + "<br>Datei: " + FileName); }
                return false;
            }


            return true;
        }




        public static string ChecksumFileName(string name) {

            name = name.Replace("\\", "}");
            name = name.Replace("/", "}");
            name = name.Replace(":", "}");
            name = name.Replace("?", "}");

            name = name.Replace("\r", "");

            if (name.Length < 100) { return name; }

            var nn = "";

            for (var z = 0; z <= name.Length - 21; z++) {
                nn += name.Substring(z, 1);
            }
            nn += name.Substring(name.Length - 20);


            return nn;
        }
     

        public static void SaveToDisk(string DateiName, string Text2Save, bool ExecuteAfter, System.Text.Encoding code) {

            try {
                //switch (DateiName.FileType()) {
                //    case enFileFormat.HTML:
                //    case enFileFormat.XMLFile:
                //        File.WriteAllText(DateiName, Text2Save, Encoding.UTF8);
                //        break;

                //    case enFileFormat.ProgrammingCode:
                //        File.WriteAllText(DateiName, Text2Save, Encoding.Unicode);
                //        break;

                //    default:
                //        File.WriteAllText(DateiName, Text2Save, Encoding.Defxault);
                //        break;
                //}
                File.WriteAllText(DateiName, Text2Save, code);
                if (ExecuteAfter) { ExecuteFile(DateiName); }
            } catch (Exception ex) {
                Develop.DebugPrint(ex);
            }



        }

        //public static string LoadFromDisk(string DateiName) {


        //    switch (DateiName.FileSuffix()) {
        //        case "XML":
        //            return File.ReadAllText(DateiName, Encoding.UTF8);
        //        default:
        //            return File.ReadAllText(DateiName, Encoding.Defxault);
        //    }


        //}


        public static string LoadFromDiskUTF8(string DateiName) {
                    return File.ReadAllText(DateiName, Encoding.UTF8);
        }

        public static string LoadFromDiskLatin(string DateiName) {
            return File.ReadAllText(DateiName, Encoding.Latin1);
        }


        public static string GetFileInfo(string filename, bool mustDo) {
            try {
                var f = new FileInfo(filename);
                return f.LastWriteTimeUtc.ToString(Constants.Format_Date) + "-" + f.Length.ToString();
            } catch {
                if (!mustDo) { return string.Empty; }
                Develop.CheckStackForOverflow();
                Pause(0.5, false);
                return GetFileInfo(filename, mustDo);
            }
        }



    }
}