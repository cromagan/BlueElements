// http://openbook.galileo-press.de/visualbasic_2008/vb2008_03_klassendesign_015.htm#mjb06b32f7141ae42e9e38c96b77a2b713

using System;
using System.Collections.Generic;
using System.IO;
using static BlueBasics.modAllgemein;
using BlueBasics.Enums;

namespace BlueBasics
{

    public static class FileOperations
    {
        private static DateTime CanWrite_LastCheck = DateTime.Now.Subtract(new TimeSpan(10, 10, 10));
        private static bool CanWrite_LastResult;
        private static string CanWrite_LastFile = string.Empty;



        public static void DeleteDir(string Pfad)
        {
            Pfad = Pfad.CheckPath();

            if (!PathExists(Pfad)) { return; }




            try
            {
                Directory.Delete(Pfad, true);
            }
            catch (Exception ex)
            {

                Develop.DebugPrint("Ordner " + Pfad + " konnte nicht gelöscht werden.<br>" + ex.Message);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="Filelist"></param>
        /// <param name="Meldungen"></param>
        /// <returns>True, wenn mindestens eine DAtei gelöscht wurde.</returns>
        public static bool DeleteFile(List<string> Filelist)
        {
            for (var Z = 0 ; Z < Filelist.Count ; Z++)
            {
                if (!FileExists(Filelist[Z])) { Filelist[Z] = ""; }
            }

            Filelist = Filelist.SortedDistinctList();

            if (Filelist.Count == 0) { return false; }


            var del = false;


            foreach (var ThisFile in Filelist)
            {
                if (!FileExists(ThisFile)) { return true; }
                // Komisch, manche Dateien können zwar gelöscht werden, die Attribute aber nicht geändert (Berechtigungen?)
                try
                {
                    File.SetAttributes(ThisFile, FileAttributes.Normal);
                }
                catch (Exception ex)
                {
                    Develop.DebugPrint(ex);
                }

                try
                {
                    if (CanWrite(ThisFile, 0.5))
                    {
                        File.Delete(ThisFile);
                        del = true;
                    }
                    else
                    {
                        Develop.DebugPrint(enFehlerArt.Info, "Die Datei '" + ThisFile + "' konnte nicht gelöscht werden.\r\nNicht 'FileIsWriteable'");
                    }
                }
                catch (Exception ex)
                {
                    Develop.DebugPrint(ex);
                }
            }

            return del;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="File"></param>
        /// <returns>True, wenn mindestens eine DAtei gelöscht wurde.</returns>
        public static bool DeleteFile(string File)
        {
            var f = new List<string>();
            f.Add(File);
            return DeleteFile(f);
        }
        public static string RenameFile(string OldN, string NewN)
        {

            if (OldN == NewN) { return string.Empty; }


            var StartTime = DateTime.Now;
            do
            {
                if (!FileExists(OldN)) { return "Quelldatei existiert nicht: " + OldN; }
                if (FileExists(NewN)) { return "Zieldatei existiert bereits: " + NewN; }


                try
                {
                    File.Move(OldN, NewN);
                    return string.Empty;
                }
                catch (Exception ex)
                {
                    if (DateTime.Now.Subtract(StartTime).TotalSeconds > 30)
                    {
                        Develop.DebugPrint("Datei konnte nicht umbenannt werden: " + OldN + "<br>" + ex.Message);
                        return "Datei konnte nicht umbenannt werden: " + OldN + "<br>" + ex.Message;
                    }
                }


            } while (true);
        }
        public static string CopyFile(string OldN, string NewN)
        {
            if (OldN == NewN) { return string.Empty; }

            var StartTime = DateTime.Now;
            do
            {
                if (!FileExists(OldN)) { return "Quelldatei existiert nicht: " + OldN; }
                if (FileExists(NewN)) { return "Zieldatei existiert bereits: " + NewN; }


                try
                {
                    File.Copy(OldN, NewN);
                    return string.Empty;
                }
                catch (Exception ex)
                {
                    if (DateTime.Now.Subtract(StartTime).TotalSeconds > 30)
                    {
                        Develop.DebugPrint("Datei konnte nicht kopiert werden: " + OldN + "<br>" + ex.Message);
                        return "Datei konnte nicht kopiert werden: " + OldN + "<br>" + ex.Message;
                    }
                }


            } while (true);
        }


        public static bool FileExists(string Datei)
        {
            if (string.IsNullOrEmpty(Datei)) { return false; }
            if (Datei.ContainsChars(Constants.Char_PfadSonderZeichen)) { return false; }
            return File.Exists(Datei);
        }

        public static bool CanWriteInDirectory(string Directory)
        {

            if (string.IsNullOrEmpty(Directory)) { return false; }

            var di = new DirectoryInfo(Directory);

            return di.IsWriteable();
       }

        public static bool CanWrite(string Datei, double TryItForSeconds)
        {
            if (!CanWriteInDirectory(Datei.FilePath())) { return false; }
            var s = DateTime.Now;
            do
            {
                //  If Not FileExists(Datei) Then Return False ' Irgend ein Prozess hat die wohl gelöscht...
                if (CanWrite(Datei)) { return true; }
                if (DateTime.Now.Subtract(s).TotalSeconds > TryItForSeconds) { return false; }
            } while (true);
        }


        private static bool CanWrite(string sFile)
        {
            //Private lassen, das andere CanWrite greift auf diese zu.
            //Aber das andere prüft zusätzlich die Schreibrechte im Verzeichnis
            //http://www.vbarchiv.net/tipps/tipp_1281.html

            if (CanWrite_LastResult) { CanWrite_LastFile = string.Empty; }
            if (DateTime.Now.Subtract(CanWrite_LastCheck).TotalSeconds > 10) { CanWrite_LastFile = string.Empty; }
            if (CanWrite_LastFile != sFile.ToUpper())
            {

                var StartTime = DateTime.Now;

                if (FileExists(sFile))
                {
                    try
                    {
                        // Versuch, Datei EXKLUSIV zu öffnen
                        using (var obFi = new FileStream(sFile, FileMode.Open, FileAccess.Read, FileShare.None))
                        {
                            obFi.Close();
                        }

                        CanWrite_LastResult = Convert.ToBoolean(DateTime.Now.Subtract(StartTime).TotalSeconds < 1);

                    }
                    catch
                    {
                        // Bei Fehler ist die Datei in Benutzung
                        CanWrite_LastResult = false;
                    }
                }

                CanWrite_LastCheck = DateTime.Now;
            }

            CanWrite_LastFile = sFile.ToUpper();

            return CanWrite_LastResult;
        }

        public static bool PathExists(string Pfad)
        {
            if (Pfad.Length < 3) { return false; }
            return Directory.Exists(Pfad.CheckPath());
        }


        public static string TempFile(string NewPath, string Filename)
        {

            // Dim dp As String = DateiPfad(FullName)
            var dn = Filename.FileNameWithoutSuffix();
            var ds = Filename.FileSuffix();

            // If Not String.IsNullOrEmpty(ds) Then dn = Mid(dn, 1, dn.Length - ds.Length - 1)


            return TempFile(NewPath, dn, ds);
        }

        public static string TempFile(string FullName)
        {

            var dp = FullName.FilePath();
            var dn = FullName.FileNameWithoutSuffix();
            var ds = FullName.FileSuffix();

            // If Not String.IsNullOrEmpty(ds) Then dn = Mid(dn, 1, dn.Length - ds.Length - 1)
            return TempFile(dp, dn, ds);
        }



        public static string TempFile()
        {
            return TempFile("", "", "");
        }



        public static string TempFile(string Pfad, string Wunschname, string Suffix)
        {

            if (string.IsNullOrEmpty(Pfad)) { Pfad = Path.GetTempPath(); }
            if (string.IsNullOrEmpty(Suffix)) { Suffix = "tmp"; }
            if (string.IsNullOrEmpty(Wunschname)) { Wunschname = UserName() + DateTime.Now.ToShortTimeString(); }


            var z = -1;


            Pfad = Pfad.TrimEnd("\\") + "\\";

            //if (Pfad.Substring(Pfad.Length - 1) != "\\")
            //{
            //    Pfad = Pfad + "\\";
            //}

            if (!PathExists(Pfad)) { Directory.CreateDirectory(Pfad); }


            Wunschname = Wunschname.RemoveChars(Constants.Char_DateiSonderZeichen);

            do
            {
                z += 1;
                string k = null;
                if (z > 0)
                {
                    k = Pfad + Wunschname + "_" + z.Nummer(5) + "." + Suffix;
                }
                else
                {
                    k = Pfad + Wunschname + "." + Suffix;
                }
                if (!FileExists(k)) { return k; }
            } while (true);

        }




    }
}