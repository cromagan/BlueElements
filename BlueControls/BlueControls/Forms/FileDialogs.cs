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

using System.Collections.Generic;
using BlueBasics;
using BlueBasics.Enums;
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
        List<string> f =
        [
            file
        ];
        return DeleteFile(f, rückfrage);
    }

    #endregion
}