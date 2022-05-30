// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueDatabase;
using BlueDatabase.EventArgs;

namespace BlueControls.BlueDatabaseDialogs;

public sealed partial class Search : Form {

    #region Fields

    private readonly Table _blueTable;
    private ColumnItem? _col;
    private RowData? _row;

    #endregion

    #region Constructors

    public Search(Table table) {
        // Dieser Aufruf ist f�r den Designer erforderlich.
        InitializeComponent();
        // F�gen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        _blueTable = table;
        _blueTable.CursorPosChanged += CursorPosChanged;
        CursorPosChanged(_blueTable, new CellExtEventArgs(_blueTable.CursorPosColumn, _blueTable.CursorPosRow));
    }

    #endregion

    #region Methods

    protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e) {
        base.OnFormClosing(e);
        _blueTable.CursorPosChanged -= CursorPosChanged;
    }

    private void btnSuchInCell_Click(object? sender, System.EventArgs e) {
        var suchtT = SuchText();
        if (string.IsNullOrEmpty(suchtT)) { return; }
        Table.SearchNextText(suchtT, _blueTable, _col, _blueTable.CursorPosRow, out var found, out var gefRow, btnAehnliches.Checked);
        if (found == null) {
            MessageBox.Show("Text nicht gefunden", ImageCode.Information, "OK");
            return;
        }
        _blueTable.CursorPos_Set(found, gefRow, true);
        txbSuchText.Focus();
    }

    private void btnSuchSpalte_Click(object sender, System.EventArgs e) {
        if (_blueTable.Design == BlueTableAppearance.OnlyMainColumnWithoutHead) {
            MessageBox.Show("In dieser Ansicht nicht m�glich", ImageCode.Information, "OK");
            return;
        }
        var searchT = SuchText();
        if (string.IsNullOrEmpty(searchT)) { return; }
        var found = _col;
        var ca = _blueTable?.CurrentArrangement;
        if (found == null) { found = _blueTable.Database.Column.SysLocked; }
        var columnStarted = _col;
        do {
            found = ca.NextVisible(found);
            if (found == null) { found = ca[0].Column; }
            var ist1 = found.ReadableText().ToLower();
            if (!string.IsNullOrEmpty(ist1)) {
                // Allgemeine Pr�fung
                if (ist1.Contains(searchT.ToLower())) { break; }
                if (btnAehnliches.Checked) {
                    var ist3 = ist1.StarkeVereinfachung(" ,");
                    var searchTxt3 = searchT.StarkeVereinfachung(" ,");
                    if (!string.IsNullOrEmpty(ist3) && ist3.ToLower().Contains(searchTxt3.ToLower())) {
                        break;
                    }
                }
            }
            if (columnStarted == found) {
                found = null;
                break;
            }
        } while (true);
        if (found == null) {
            MessageBox.Show("Text in den Spalten nicht gefunden.", ImageCode.Information, "OK");
            return;
        }
        _blueTable.CursorPos_Set(found, _row, true);
        txbSuchText.Focus();
    }

    private void CursorPosChanged(object sender, CellExtEventArgs e) {
        _row = e.RowData;
        _col = e.Column;
    }

    private void Search_Load(object sender, System.EventArgs e) => txbSuchText.Focus();

    private string SuchText() {
        var suchtT = txbSuchText.Text.Trim();
        if (string.IsNullOrEmpty(suchtT)) {
            MessageBox.Show("Bitte Text zum Suchen eingeben.", ImageCode.Information, "OK");
            return string.Empty;
        }
        return suchtT.Replace(";cr;", "\r").Replace(";tab;", "\t").ToLower();
    }

    private void txbSuchText_Enter(object sender, System.EventArgs e) => btnSuchInCell_Click(null, System.EventArgs.Empty);

    private void txbSuchText_TextChanged(object sender, System.EventArgs e) { }

    #endregion
}