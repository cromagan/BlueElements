using BlueBasics;
using BlueBasics.Enums;
using System.Collections.Generic;
using static BlueBasics.FileOperations;

namespace BlueControls.Forms {
    public static class FileDialogs {


        //#region FileDialogs

        //public static void DeleteDir(string Pfad, bool Meldungen = true)
        //{
        //    int ButtonNumber = 0;

        //    Pfad = Pfad.CheckPath();

        //    if (!PathExists(Pfad)) { return; }


        //    if (Meldungen)
        //    {
        //        ButtonNumber = MessageBox.Show("Soll der Ordner \"" + Pfad + "\"<br>und dessen Inhalt wirklich <b>gelöscht</b> werden?\"", enImageCode.Warnung, "Ja - löschen", "Nein - abbrechen");
        //    }
        //    else
        //    {
        //        ButtonNumber = 0;
        //    }

        //    if (ButtonNumber == 0)
        //    {

        //        try
        //        {
        //            Directory.Delete(Pfad, true);
        //        }
        //        catch (Exception ex)
        //        {
        //            if (Meldungen)
        //            {
        //                MessageBox.Show("Ordner konnte <b>nicht</b> gelöscht werden:<br>" + ex.Message, enImageCode.Kritisch, "OK");
        //            }
        //            Develop.DebugPrint("Ordner " + Pfad + " konnte nicht gelöscht werden.<br>" + ex.Message);
        //        }
        //    }
        //    else
        //    {
        //        if (Meldungen)
        //        {
        //            MessageBox.Show("Ordner <b>nicht</b> gelöscht!", enImageCode.Information, "OK");
        //        }
        //    }
        //}


        /// <summary>
        /// 
        /// </summary>
        /// <param name="Filelist"></param>
        /// <param name="Meldungen"></param>
        /// <returns>True, wenn mindestens eine DAtei gelöscht wurde.</returns>
        public static bool DeleteFile(List<string> Filelist, bool Meldungen) {
            var ButtonNumber = 0;

            for (var Z = 0; Z < Filelist.Count; Z++) {
                if (!FileExists(Filelist[Z])) { Filelist[Z] = ""; }
            }

            Filelist = Filelist.SortedDistinctList();

            if (Filelist.Count == 0) { return false; }


            if (Meldungen) {
                if (Filelist.Count == 1) {
                    ButtonNumber = MessageBox.Show("Soll die Datei<br>\"" + Filelist[0] + "\"<br>wirklich <b>gelöscht</b> werden?\"", enImageCode.Warnung, "Ja - löschen", "Nein - abbrechen");
                } else {
                    ButtonNumber = MessageBox.Show("Sollen wirklich " + Filelist.Count + " Dateien<br><b>gelöscht</b> werden?\"", enImageCode.Warnung, "Ja - löschen", "Nein - abbrechen");
                }

            }

            if (ButtonNumber == 0) {
                return FileOperations.DeleteFile(Filelist);
            }
            return false; //nein geklickt
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Filelist"></param>
        /// <param name="Meldungen"></param>
        /// <returns>True, wenn mindestens eine DAtei gelöscht wurde.</returns>
        public static bool DeleteFile(string File, bool Rückfrage) {
            var f = new List<string>
            {
                File
            };
            return DeleteFile(f, Rückfrage);
        }
        //public static string RenameFile(string OldN, string NewN)
        //{

        //    if (OldN == NewN) { return string.Empty; }


        //    DateTime StartTime = DateTime.Now;
        //    do
        //    {
        //        if (!FileExists(OldN)) { return "Quelldatei existiert nicht: " + OldN; }
        //        if (FileExists(NewN)) { return "Zieldatei existiert bereits: " + NewN; }


        //        try
        //        {
        //            File.Move(OldN, NewN);
        //            return string.Empty;
        //        }
        //        catch (Exception ex)
        //        {
        //            if (DateTime.Now.Subtract(StartTime).TotalSeconds > 30)
        //            {
        //                Develop.DebugPrint("Datei konnte nicht umbenannt werden: " + OldN + "<br>" + ex.Message);
        //                return "Datei konnte nicht umbenannt werden: " + OldN + "<br>" + ex.Message;
        //            }
        //        }


        //    } while (true);
        //}
        //public static string CopyFile(string OldN, string NewN)
        //{
        //    if (OldN == NewN) { return string.Empty; }

        //    DateTime StartTime = DateTime.Now;
        //    do
        //    {
        //        if (!FileExists(OldN)) { return "Quelldatei existiert nicht: " + OldN; }
        //        if (FileExists(NewN)) { return "Zieldatei existiert bereits: " + NewN; }


        //        try
        //        {
        //            File.Copy(OldN, NewN);
        //            return string.Empty;
        //        }
        //        catch (Exception ex)
        //        {
        //            if (DateTime.Now.Subtract(StartTime).TotalSeconds > 30)
        //            {
        //                Develop.DebugPrint("Datei konnte nicht kopiert werden: " + OldN + "<br>" + ex.Message);
        //                return "Datei konnte nicht kopiert werden: " + OldN + "<br>" + ex.Message;
        //            }
        //        }


        //    } while (true);
        //}

        //#endregion

    }
}
