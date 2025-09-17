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
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueTable;
using BlueTable.Enums;
using BlueTable.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.BlueTableDialogs;

public sealed partial class RowCleanUp : FormWithStatusBar, IHasTable {

    #region Fields

    private BlueTable.Table? _table;
    private BlueControls.Controls.TableView? _tableview;

    #endregion

    #region Constructors

    public RowCleanUp(Controls.TableView table) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        TableView = table;

        Table = table.Table;
        //_table.SelectedCellChanged += SelectedCellChanged;
        //SelectedCellChanged(_table, new CellExtEventArgs(_table.CursorPosColumn, _table.CursorPosRow));

        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        //_originalImportText = importtext.Replace("\r\n", "\r").Trim("\r");
        //var ein = _originalImportText.SplitAndCutByCrToList();
        //Eintr.Text = ein.Count + " zum Importieren bereit.";
        //Table = table;

        if (_table is { IsDisposed: false } db) {
            //var lst =  List<AbstractListItem>();
            lstColumns.ItemAddRange(ItemsOf(db.Column, false));
            //cbxColDateiname.Item = lst;
        }

        CheckButtons();
    }

    #endregion

    #region Properties

    public BlueTable.Table? Table {
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

    public BlueControls.Controls.TableView? TableView {
        get => _tableview;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == _tableview) { return; }

            if (_tableview != null) {
                _tableview.VisibleRowsChanged -= _table_VisibleRowsChanged;
            }
            _tableview = value;

            if (_tableview != null) {
                _tableview.VisibleRowsChanged += _table_VisibleRowsChanged;
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

    private void _table_VisibleRowsChanged(object sender, System.EventArgs e) => CheckButtons();

    private void Cancel_Click(object sender, System.EventArgs e) => Close();

    private void CheckButtons() {
        if (_table == null || _tableview == null) {
            txtInfo.Text = "Keine Tabelle gewählt.";
            btnExecute.Enabled = false;
            return;
        }

        var r = _tableview.RowsVisibleUnique().Count;

        if (r == 0) {
            txtInfo.Text = "Keine Zeilen angezeigt.";
            btnExecute.Enabled = false;
            return;
        }

        if (lstColumns.Checked.Count == 0) {
            txtInfo.Text = "Keine Spalten gewählt.";
            btnExecute.Enabled = false;
            return;
        }

        //if (_table.Column[cbxColDateiname.Text] == null) {
        //    txtInfo.Text = "Keine Spalte für Dateinahmen gewählt.";
        //    btnImport.Enabled = false;
        //    return;
        //}

        btnExecute.Enabled = true;

        //if (_files.Count == 1) {
        //    txtInfo.Text = _files[0];
        //    return;
        //}

        txtInfo.Text = r + " angepinnte und gefilterte Zeilen werden berücksichtigt.";
    }

    private void Fertig_Click(object sender, System.EventArgs e) {
        var r = _tableview?.RowsVisibleUnique();

        if (r is not { Count: not 0 }) {
            MessageBox.Show("Keine Zeilen gewählt.", ImageCode.Information, "OK");
            return;
        }

        if (_table is not { IsDisposed: false } db) { return; }
        var columns = new List<ColumnItem>();
        foreach (var column in lstColumns.Checked) {
            if (db.Column[column] is { IsDisposed: false } c) {
                columns.Add(c);
            }
        }

        foreach (var thisR in r) {
            if (!thisR.IsDisposed && db.Row.Contains(thisR)) {

                #region Filtercol erstellen

                var f = new FilterCollection(db, "Dupe Suche");

                foreach (var thisc in columns) {
                    f.Add(new FilterItem(thisc, FilterType.Istgleich_GroßKleinEgal_MultiRowIgnorieren, thisR.CellGetString(thisc)));
                }

                #endregion

                #region Zeilen ermitteln (rows)

                var rows = f.Rows.Intersect(r).ToList();

                #endregion

                if (rows.Count > 1) {
                    if (optFülle.Checked) {
                        db.Row.Combine(rows);
                    } else if (optLöschen.Checked) {
                        db.Row.RemoveYoungest(rows, false);
                    } else {
                        MessageBox.Show("Modus unbekannt.", ImageCode.Information, "OK");
                        return;
                    }
                }
            }
        }
    }

    private void lstColumns_ItemClicked(object sender, AbstractListItemEventArgs e) => CheckButtons();

    #endregion
}