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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using static BlueBasics.Extensions;

namespace BlueBasics;

public class BackupVerwalter {

    #region Fields

    private Dictionary<string, string> _data = new();

    private List<string>? _deletable;

    #endregion

    #region Properties

    public ReadOnlyCollection<string> Deleteable {
        get {
            if (_deletable == null) { CalculateDeleteable(); }

            return new ReadOnlyCollection<string>(_deletable ?? new List<string>());
        }
    }

    #endregion

    #region Methods

    public void AddData(DateTime dateUTC, string file) {
        var d = dateUTC.ToString(Constants.Format_Date6);

        if (_data.ContainsKey(d)) { return; }

        _data.Add(d, file.ToUpper());

        _deletable = null;
    }

    public void CalculateDeleteable() {
        var von = 0;
        var bis = 1;

        _deletable = new List<string>();

        do {
            von = bis + 1; // Beispiel: 9
            bis *= 2; // Beispiel: 16

            // Beispiel: els = 9 bis < 15 - EVTL. löschbare Schlüssel
            for (var els = von; els < bis; els++) {
                var dt = DateTime.UtcNow.AddDays(-els);
                var dts = dt.ToString(Constants.Format_Date6);

                if (_data.ContainsKey(dts)) {
                    for (var istda = els + 1; istda < bis; istda++) {
                        var dti = DateTime.UtcNow.AddDays(-istda);
                        var dtis = dti.ToString(Constants.Format_Date6);
                        if (_data.ContainsKey(dtis)) {
                            if (_data.TryGetValue(dts, out var k)) {
                                _deletable.Add(k);
                            }
                            break;
                        }
                    }
                }
            }
        }
        while (bis < 2048);
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