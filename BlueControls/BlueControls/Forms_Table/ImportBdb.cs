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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.BlueTableDialogs;

public sealed partial class ImportBdb : FormWithStatusBar, IHasTable {

    #region Fields

    private Table? _table;

    private List<string>? _files;

    #endregion

    #region Constructors

    public ImportBdb(Table? table) : base() {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        //_originalImportText = importtext.Replace("\r\n", "\r").Trim("\r");
        //var ein = _originalImportText.SplitAndCutByCrToList();
        //Eintr.Text = ein.Count + " zum Importieren bereit.";
        Table = table;

        if (table != null) {
            //var lst =  List<AbstractListItem>();
            cbxColDateiname.ItemAddRange(ItemsOf(table.Column, false));
            //cbxColDateiname.Item = lst;
        }

        CheckButtons();
    }

    #endregion

    #region Properties

    public Table? Table {
        get => _table;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == _table) { return; }

            if (_table != null) {
                _table.DisposingEvent -= _table_Disposing;
            }
            _table = value;

            if (_table != null) {
                _table.DisposingEvent += _table_Disposing;
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

    private void btnChoseTable_Click(object sender, System.EventArgs e) => _ = LoadTab.ShowDialog();

    private void Cancel_Click(object sender, System.EventArgs e) => Close();

    private void cbxColDateiname_TextChanged(object sender, System.EventArgs e) => CheckButtons();

    private void CheckButtons() {
        if (_table == null) {
            txtInfo.Text = "Keine Tabelle gewählt.";
            btnImport.Enabled = false;
            return;
        }

        if (_files is not { Count: not 0 }) {
            txtInfo.Text = "Keine Datei(en) zum Importieren gewählt.";
            btnImport.Enabled = false;
            return;
        }

        if (_table.Column[cbxColDateiname.Text] == null) {
            txtInfo.Text = "Keine Spalte für Dateinahmen gewählt.";
            btnImport.Enabled = false;
            return;
        }

        btnImport.Enabled = true;

        if (_files.Count == 1) {
            txtInfo.Text = _files[0];
            return;
        }

        txtInfo.Text = _files.Count + " Dateien.";
    }

    private void Fertig_Click(object sender, System.EventArgs e) {
        //var TR = string.Empty;
        //if (TabStopp.Checked) {
        //    TR = "\t";
        //} else if (Semikolon.Checked) {
        //    TR = ";";
        //} else if (Komma.Checked) {
        //    TR = ",";
        //} else if (Leerzeichen.Checked) {
        //    TR = " ";
        //} else if (Andere.Checked) {
        //    TR = aTXT.Text;
        //}
        //if (string.IsNullOrEmpty(TR)) {
        //    MessageBox.Show("Bitte Trennzeichen angeben.", ImageCode.Information, "OK");
        //    return;
        //}

        if (_files is not { Count: not 0 }) { return; }
        if (_table == null) { return; }

        var m = "Tabellen-Fehler";

        if (Table is  TableFile { IsDisposed: false } tbf) {
            m = tbf.ImportBdb(_files, _table.Column[cbxColDateiname.Text], btnDateienlöschen.Checked);
        }

        if (!string.IsNullOrEmpty(m)) {
            MessageBox.Show(m, ImageCode.Information, "OK");
        }
        Close();
    }

    private void LoadTab_FileOk(object sender, CancelEventArgs e) {
        _files = LoadTab.FileNames.ToList();
        CheckButtons();
    }

    #endregion
}