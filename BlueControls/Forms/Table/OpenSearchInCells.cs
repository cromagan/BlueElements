// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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
using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionList.TableItems;
using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueTable.Classes;
using BlueTable.Interfaces;
using System.Windows.Forms;
using Form = BlueControls.Forms.Form;

namespace BlueControls.BlueTableDialogs;

public sealed partial class OpenSearchInCells : Form, IUniqueWindow, IHasTable {

    #region Fields

    private ColumnViewItem? _col;
    private RowListItem? _row;
    private TableView? _tableView;

    #endregion

    #region Constructors

    public OpenSearchInCells() {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
    }

    #endregion

    #region Properties

    public object? Object {
        get => _tableView;

        set {
            var newt = value as TableView;

            if (_tableView != null) {
                _tableView.SelectedCellChanged -= SelectedCellChanged;
                _tableView.TableChanged -= TableChanged;
            }

            _tableView = newt;

            if (_tableView != null) {
                _tableView.SelectedCellChanged += SelectedCellChanged;
                _tableView.TableChanged += TableChanged;
                SelectedCellChanged(_tableView, new CellExtEventArgs(_tableView.CursorPosColumn, _tableView.CursorPosRow));
            }
        }
    }

    public Table? Table => _tableView?.Table;

    #endregion

    #region Methods

    protected override void OnFormClosing(FormClosingEventArgs e) {
        base.OnFormClosing(e);
        Object = null;
    }

    private void btnSuchInCell_Click(object? sender, System.EventArgs e) {
        var suchtT = SuchText();
        if (string.IsNullOrEmpty(suchtT)) { return; }
        TableView.SearchNextText(suchtT, _tableView, _col, _tableView.CursorPosRow, out var found, out var gefRow, btnAehnliches.Checked);
        if (found == null) {
            Forms.MessageBox.Show("Text nicht gefunden", ImageCode.Information, "OK");
            return;
        }
        _tableView.CursorPos_Set(found, gefRow, true);
        txbSuchText.Focus();
    }

    private void btnSuchSpalte_Click(object sender, System.EventArgs e) {
        //if (_table.Design == BlueTableAppearance.OnlyMainColumnWithoutHead) {
        //    BlueControls.Forms.MessageBox.Show("In dieser Ansicht nicht möglich", ImageCode.Information, "OK");
        //    return;
        //}
        var searchT = SuchText();
        if (string.IsNullOrEmpty(searchT)) { return; }
        var found = _col;

        if (_tableView.CurrentArrangement is not { IsDisposed: false } ca) {
            Forms.MessageBox.Show("Ansicht-Fehler", ImageCode.Information, "OK");
            return;
        }

        found ??= ca.Last();
        var columnStarted = _col;

        do {
            found = ca.NextVisible(found) ?? ca.First();

            if (found?.Column is not { IsDisposed: false } c) {
                Forms.MessageBox.Show("Ansicht-Fehler", ImageCode.Information, "OK");
                return;
            }

            var ist1 = c.ReadableText().ToLowerInvariant() + " (" + c.KeyName.ToLowerInvariant() + ")";
            if (!string.IsNullOrEmpty(ist1)) {
                // Allgemeine Prüfung
                if (ist1.Contains(searchT.ToLowerInvariant())) { break; }
                if (btnAehnliches.Checked) {
                    var ist3 = ist1.StarkeVereinfachung(" ,", true);
                    var searchTxt3 = searchT.StarkeVereinfachung(" ,", true);
                    if (!string.IsNullOrEmpty(ist3) && ist3.ContainsIgnoreCase(searchTxt3)) {
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
            Forms.MessageBox.Show("Text in den Spalten nicht gefunden.", ImageCode.Information, "OK");
            return;
        }
        _tableView.CursorPos_Set(found, _row, true);
        txbSuchText.Focus();
    }

    private void Search_Load(object sender, System.EventArgs e) => txbSuchText.Focus();

    private void SelectedCellChanged(object sender, CellExtEventArgs e) {
        _row = e.RowData;
        _col = e.ColumnView;
    }

    private string SuchText() {
        var suchtT = txbSuchText.Text.Trim();
        if (string.IsNullOrEmpty(suchtT)) {
            Forms.MessageBox.Show("Bitte Text zum Suchen eingeben.", ImageCode.Information, "OK");
            return string.Empty;
        }
        return suchtT.Replace(";cr;", "\r").Replace(";tab;", "\t").ToLowerInvariant();
    }

    private void TableChanged(object sender, System.EventArgs e) => Close();

    private void txbSuchText_Enter(object sender, System.EventArgs e) => btnSuchInCell_Click(null, System.EventArgs.Empty);

    private void txbSuchText_TextChanged(object sender, System.EventArgs e) { }

    #endregion
}