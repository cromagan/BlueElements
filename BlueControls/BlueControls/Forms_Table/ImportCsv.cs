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

using BlueBasics.Enums;
using BlueControls.Forms;
using BlueTable;
using BlueTable.Interfaces;
using System.ComponentModel;
using static BlueBasics.Extensions;

namespace BlueControls.BlueTableDialogs;

public sealed partial class ImportCsv : FormWithStatusBar, IHasTable {

    #region Fields

    private readonly string _originalImportText;

    #endregion

    #region Constructors

    public ImportCsv(Table? table, string importtext) : base() {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        _originalImportText = importtext.Replace("\r\n", "\r").Trim("\r");
        var ein = _originalImportText.SplitAndCutByCrToList();
        capEinträge.Text = ein.Count + " zum Importieren bereit.";
        Table = table;
    }

    #endregion

    #region Properties

    public Table? Table {
        get;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == field) { return; }

            if (field != null) {
                field.DisposingEvent -= _table_Disposing;
            }
            field = value;

            if (field != null) {
                field.DisposingEvent += _table_Disposing;
            }
        }
    }

    #endregion

    #region Methods

    protected override void OnClosing(CancelEventArgs e) {
        Table = null;
        base.OnClosing(e);
    }

    private void _table_Disposing(object sender, System.EventArgs e) {
        Table = null;
        Close();
    }

    private void Cancel_Click(object sender, System.EventArgs e) => Close();

    private void Fertig_Click(object sender, System.EventArgs e) {
        var tr = string.Empty;
        if (optTabStopp.Checked) {
            tr = "\t";
        } else if (optSemikolon.Checked) {
            tr = ";";
        } else if (optKomma.Checked) {
            tr = ",";
        } else if (optLeerzeichen.Checked) {
            tr = " ";
        } else if (optAndere.Checked) {
            tr = txtAndere.Text;
        }
        if (string.IsNullOrEmpty(tr)) {
            MessageBox.Show("Bitte Trennzeichen angeben.", ImageCode.Information, "OK");
            return;
        }
        var m = "Tabellen-Fehler";

        if (Table is { IsDisposed: false }) {
            m = Table.ImportCsv(_originalImportText, optZeilenZuorden.Checked, tr, chkDoppelteTrennzeichen.Checked, chkTrennzeichenAmAnfang.Checked);
        }

        if (!string.IsNullOrEmpty(m)) {
            MessageBox.Show(m, ImageCode.Information, "OK");
        }
        Close();
    }

    #endregion
}