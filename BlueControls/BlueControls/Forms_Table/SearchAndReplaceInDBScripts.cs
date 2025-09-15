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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Forms;
using BlueTable;
using System;
using System.Collections.Generic;
using System.Linq;
using static BlueBasics.Constants;
using static BlueBasics.IO;

namespace BlueControls.BlueTableDialogs;

internal sealed partial class SearchAndReplaceInDBScripts : Form {

    #region Fields

    private bool _isWorking;

    #endregion

    #region Constructors

    public SearchAndReplaceInDBScripts() =>
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();

    #endregion

    // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.//_table = table;//_table.SelectedCellChanged += SelectedCellChanged;//SelectedCellChanged(_table, new CellExtEventArgs(_table.CursorPosColumn, _table.CursorPosRow));

    #region Methods

    private void AltNeu_TextChanged(object sender, System.EventArgs e) => Checkbuttons();

    private void btnErsetzen_Click(object sender, System.EventArgs e) {
        if (_isWorking) { return; }
        _isWorking = true;

        var count = 0;

        foreach (var thisDb in Table.AllFiles) {
            if (thisDb is { IsDisposed: false } db && !string.IsNullOrEmpty(db.Filename) && string.IsNullOrEmpty(db.CanWriteMainFile())) {

                foreach (var script in db.EventScript.ToList()) { // ToList() für sichere Iteration
                    var newScriptContent = script.Script.Replace(txbAlt.Text, txbNeu.Text);
                    if (newScriptContent != script.Script) {
                        count++;
                        Table.UpdateScript(script, scriptContent: newScriptContent, failedReason: string.Empty);
                    }
                }
            }
        }

        MessageBox.Show(count + " Skript(e) bearbeitet.", ImageCode.Information, "OK");
        _isWorking = false;
    }

    private void btnSuche_Click(object sender, System.EventArgs e) {
        if (_isWorking) { return; }
        _isWorking = true;

        var l = new List<string>();

        foreach (var thisDb in Table.AllFiles) {
            if (thisDb is { IsDisposed: false } db) {
                foreach (var thiss in db.EventScript) {
                    // Prüfen, ob der Suchtext im Skript vorkommt
                    if (thiss.Script.Contains(txbAlt.Text)) {
                        // Tabellename -> Skriptname zur Liste hinzufügen
                        l.Add(string.Empty);
                        l.Add(string.Empty);
                        l.Add("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
                        l.Add($"{db.Caption} -> {thiss.KeyName}");

                        // Alle Zeilen durchgehen und die mit dem Suchtext zur Liste hinzufügen
                        var lines = thiss.Script.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
                        foreach (var line in lines) {
                            if (line.Contains(txbAlt.Text)) {
                                l.Add(line.Trim());
                            }
                        }
                    }
                }
            }
        }

        l.WriteAllText(TempFile(string.Empty, string.Empty, "txt"), Win1252, true);
        _isWorking = false;
    }

    private void Checkbuttons() {
        var canDo = Generic.IsAdministrator();
        if (string.IsNullOrEmpty(txbAlt.Text)) { canDo = false; }
        btnErsetzen.Enabled = canDo;
        btnSuche.Enabled = canDo;
    }

    #endregion
}