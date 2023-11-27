// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;

namespace BlueBasics;

public class BackupVerwalter {

    #region Fields

    private readonly Dictionary<string, string> _data = new();

    private readonly int _maxfiles;
    private readonly int _multi;
    private List<string>? _deletable;

    #endregion

    #region Constructors

    public BackupVerwalter(int multi, int maxfiles) {
        _multi = multi;
        _maxfiles = maxfiles;
    }

    #endregion

    #region Properties

    public ReadOnlyCollection<string> Deleteable {
        get {
            if (_deletable == null) { CalculateDeleteable(_multi, _maxfiles); }

            return new ReadOnlyCollection<string>(_deletable ?? new());
        }
    }

    #endregion

    #region Methods

    public void AddData(DateTime dateUtc, string file) {
        var d = dateUtc.ToString(Constants.Format_Date6, CultureInfo.InvariantCulture);
        if (_data.ContainsKey(d)) { return; }
        _data.Add(d, file.ToUpper());
        _deletable = null;
    }

    /// <summary>
    /// Macht eine Reinigung des Verzeichnisses -a usschlaggebend ist LastWriteTimeUtc
    /// </summary>
    /// <param name="path">Das Verzeichnis, das bereinigt werden soll</param>
    /// <param name="search">Der Dateipattern, nach dem gesucht werden soll z.b. table_20*.bdb</param>
    /// <returns></returns>
    public string CleanUpDirectory(string path, string search) {
        if (string.IsNullOrEmpty(path)) { return "Kein Verzeichniss angebeben."; }
        if (string.IsNullOrEmpty(search)) { return "Kein Suchpattern angebeben."; }


        var fix = Directory.GetFiles(path, search, SearchOption.TopDirectoryOnly);

        if (fix.Length == 0) { return string.Empty; }
        try {
            foreach (var thisF in fix) {
                var fi = new FileInfo(thisF);
                AddData(fi.LastWriteTimeUtc, thisF);
            }

            foreach (var thisF in Deleteable) {
                IO.DeleteFile(thisF, false);
            }

            return string.Empty;
        } catch {
            return "Fehler beim Ausführen";
        }
    }

    private void CalculateDeleteable(int multi, int maxfiles) {
        var von = 0;
        var bis = multi;

        int filc = 0;

        _deletable = new List<string>();

        do {
            var dtx = DateTime.UtcNow.AddDays(-von);
            if (FileOf(dtx) != null) { filc++; }
            von++; // Beispiel: 8 -> 9

            // Beispiel: els = 9 bis < 15 - EVTL. löschbare Schlüssel
            for (var els = von; els < bis; els++) {
                var dt = DateTime.UtcNow.AddDays(-els);

                if (FileOf(dt) is string f) {
                    if (filc > maxfiles || IsThereALaterVersion(els, bis)) {
                        _deletable.Add(f);
                    } else {
                        filc++;
                    }
                }
            }

            von = bis; // Beispiel: 8
            bis *= multi; // Beispiel: 16
        } while (bis < 2048);
    }

    private string? FileOf(DateTime date) {
        var dts = date.ToString(Constants.Format_Date6, CultureInfo.InvariantCulture);
        if (!_data.ContainsKey(dts)) { return null; }
        if (!_data.TryGetValue(dts, out var file)) { return null; }
        return file;
    }

    private bool IsThereALaterVersion(int myAge, int maxAge) {
        for (var istda = myAge + 1; istda < maxAge; istda++) {
            var dti = DateTime.UtcNow.AddDays(-istda);
            if (FileOf(dti) != null) { return true; }
        }

        return false;
    }

    #endregion

    /*  [1]
     *  [2]
     *   3  löschen, wenn 4 vorhanden
     *  [4]
     *   5  löschen, wenn 6, 7 oder 8 vorhanden
     *   6  löschen, wenn 7 oder 8 vorhanden
     *   7  löschen, wenn 8 vorhanden
     *  [8]
     *   9
     *   ...
     */
}