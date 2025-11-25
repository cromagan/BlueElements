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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static BlueBasics.IO;

namespace BlueBasics;
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

public class BackupVerwalter {

    #region Fields

    private readonly Dictionary<string, string> _data = [];

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

            return new ReadOnlyCollection<string>(_deletable ?? []);
        }
    }

    #endregion

    #region Methods

    public void AddData(DateTime dateUtc, string file) {
        var d = dateUtc.ToString6();
        if (_data.ContainsKey(d)) { return; }
        _data.Add(d, file.ToUpperInvariant());
        _deletable = null;
    }

    /// <summary>
    /// Macht eine Reinigung des Verzeichnisses -ausschlaggebend ist LastWriteTimeUtc
    /// </summary>
    /// <param name="path">Das Verzeichnis, das bereinigt werden soll</param>
    /// <param name="search">Der Dateipattern, nach dem gesucht werden soll z. B. table_20*.bdb</param>
    /// <returns></returns>
    public string CleanUpDirectory(string path, string search) {
        if (string.IsNullOrEmpty(path)) { return "Kein Verzeichnis angebeben."; }
        if (string.IsNullOrEmpty(search)) { return "Kein Suchpattern angebeben."; }

        var fix = GetFiles(path, search, System.IO.SearchOption.TopDirectoryOnly);

        if (fix.Length == 0) { return string.Empty; }
        try {
            foreach (var thisF in fix) {
                var fi = GetFileInfo(thisF);
                AddData(fi?.LastWriteTimeUtc ?? DateTime.UtcNow, thisF);
            }

            foreach (var thisF in Deleteable) {
                DeleteFile(thisF, false);
            }

            return string.Empty;
        } catch {
            return "Fehler beim Ausführen";
        }
    }

    private void CalculateDeleteable(int multi, int maxfiles) {
        var von = 0;
        var bis = multi;

        var filc = 0;

        _deletable = [];

        do {
            var dtx = DateTime.UtcNow.AddDays(-von);
            if (FileOf(dtx) != null) { filc++; }
            von++; // Beispiel: 8 -> 9

            // Beispiel: els = 9 bis < 15 - EVTL. löschbare Schlüssel
            for (var els = von; els < bis; els++) {
                var dt = DateTime.UtcNow.AddDays(-els);

                if (FileOf(dt) is { } f) {
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
        var dts = date.ToString6();
        return !_data.ContainsKey(dts) ? null : !_data.TryGetValue(dts, out var file) ? null : file;
    }

    private bool IsThereALaterVersion(int myAge, int maxAge) {
        for (var istda = myAge + 1; istda < maxAge; istda++) {
            var dti = DateTime.UtcNow.AddDays(-istda);
            if (FileOf(dti) != null) { return true; }
        }

        return false;
    }

    #endregion
}