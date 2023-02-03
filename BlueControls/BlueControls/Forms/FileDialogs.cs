using BlueBasics;
using BlueBasics.Enums;
using System.Collections.Generic;
using static BlueBasics.IO;

namespace BlueControls.Forms;

public static class FileDialogs {

    #region Methods

    //#region FileDialogs
    //public static void DeleteDir(string Pfad, bool Meldungen = true)
    //{
    //    int ButtonNumber = 0;
    //    Pfad = Pfad.CheckPath();
    //    if (!DirectoryExists(Pfad)) { return; }
    //    if (Meldungen)
    //    {
    //        ButtonNumber = MessageBox.Show("Soll der Ordner \"" + Pfad + "\"<br>und dessen Inhalt wirklich <b>gelöscht</b> werden?\"", ImageCode.Warnung, "Ja - löschen", "Nein - abbrechen");
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
    //                MessageBox.Show("Ordner konnte <b>nicht</b> gelöscht werden:<br>" + ex.Message, ImageCode.Kritisch, "OK");
    //            }
    //            Develop.DebugPrint("Ordner " + Pfad + " konnte nicht gelöscht werden.<br>" + ex.Message);
    //        }
    //    }
    //    else
    //    {
    //        if (Meldungen)
    //        {
    //            MessageBox.Show("Ordner <b>nicht</b> gelöscht!", ImageCode.Information, "OK");
    //        }
    //    }
    //}
    /// <summary>
    ///
    /// </summary>
    /// <param name="filelist"></param>
    /// <param name="meldungen"></param>
    /// <returns>True, wenn mindestens eine DAtei gelöscht wurde.</returns>
    public static bool DeleteFile(List<string> filelist, bool meldungen) {
        var buttonNumber = 0;
        for (var z = 0; z < filelist.Count; z++) {
            if (!FileExists(filelist[z])) { filelist[z] = string.Empty; }
        }
        filelist = filelist.SortedDistinctList();
        if (filelist.Count == 0) { return false; }
        if (meldungen) {
            buttonNumber = filelist.Count == 1
                ? MessageBox.Show("Soll die Datei<br>\"" + filelist[0] + "\"<br>wirklich <b>gelöscht</b> werden?\"", ImageCode.Warnung, "Ja - löschen", "Nein - abbrechen")
                : MessageBox.Show("Sollen wirklich " + filelist.Count + " Dateien<br><b>gelöscht</b> werden?\"", ImageCode.Warnung, "Ja - löschen", "Nein - abbrechen");
        }
        return buttonNumber == 0 && IO.DeleteFile(filelist);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="file"></param>
    /// <param name="Meldungen"></param>
    /// <returns>True, wenn mindestens eine DAtei gelöscht wurde.</returns>
    public static bool DeleteFile(string file, bool rückfrage) {
        List<string> f = new()
        {
            file
        };
        return DeleteFile(f, rückfrage);
    }

    #endregion

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