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

#nullable enable

using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueDatabase;
using BlueDatabase.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static BlueBasics.Extensions;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.BlueDatabaseDialogs;

public sealed partial class RowCleanUp : FormWithStatusBar, IHasDatabase {

    #region Fields

    private Database? _database;
    private Table? _table;

    #endregion

    #region Constructors

    public RowCleanUp(Table table) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        Table = table;

        Database = table.Database;
        //_table.SelectedCellChanged += SelectedCellChanged;
        //SelectedCellChanged(_table, new CellExtEventArgs(_table.CursorPosColumn, _table.CursorPosRow));

        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        //_originalImportText = importtext.Replace("\r\n", "\r").Trim("\r");
        //var ein = _originalImportText.SplitAndCutByCrToList();
        //Eintr.Text = ein.Count + " zum Importieren bereit.";
        //Database = database;

        if (_database is Database db && !db.IsDisposed) {
            //var lst =  List<AbstractListItem>();
            lstColumns.ItemAddRange(ItemsOf(db.Column, false));
            //cbxColDateiname.Item = lst;
        }

        CheckButtons();
    }

    #endregion

    #region Properties

    public Database? Database {
        get => _database;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == _database) { return; }

            if (_database != null) {
                _database.DisposingEvent -= _database_Disposing;
            }
            _database = value;

            if (_database != null) {
                _database.DisposingEvent += _database_Disposing;
            }
        }
    }

    public Table? Table {
        get => _table;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == _table) { return; }

            if (_table != null) {
                _table.VisibleRowsChanged -= _table_VisibleRowsChanged;
            }
            _table = value;

            if (_table != null) {
                _table.VisibleRowsChanged += _table_VisibleRowsChanged;
            }
        }
    }

    #endregion

    #region Methods

    protected override void OnClosing(CancelEventArgs e) {
        Database = null;
        base.OnClosing(e);
    }

    private void _database_Disposing(object sender, System.EventArgs e) {
        Database = null;
        Close();
    }

    private void _table_VisibleRowsChanged(object sender, System.EventArgs e) {
        CheckButtons();
    }

    private void Cancel_Click(object sender, System.EventArgs e) => Close();

    private void CheckButtons() {
        if (_database == null || _table == null) {
            txtInfo.Text = "Keine Datenbank gewählt.";
            btnExecute.Enabled = false;
            return;
        }

        var r = _table.RowsVisibleUnique()?.Count ?? 0;

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

        //if (_database.Column[cbxColDateiname.Text] == null) {
        //    txtInfo.Text = "Keine Spalte für Dateinahmen gewählt.";
        //    btnImport.Enabled = false;
        //    return;
        //}

        btnExecute.Enabled = true;

        //if (_files.Count == 1) {
        //    txtInfo.Text = _files[0];
        //    return;
        //}

        txtInfo.Text = r.ToString() + " angepinnte und gefilterte Zeilen werden berücksichtigt.";
    }

    private void Fertig_Click(object sender, System.EventArgs e) {
        var r = _table?.RowsVisibleUnique();

        if (r == null || r.Count == 0) {
            MessageBox.Show("Keine Zeilen gewählt.", ImageCode.Information, "OK");
            return;
        }

        if (_database is not Database db || db.IsDisposed) { return; }
        var columns = new List<ColumnItem>();
        foreach (var column in lstColumns.Checked) {
            if (db.Column[column] is ColumnItem c) {
                columns.Add(c);
            }
        }

        foreach (var thisR in r) {
            if (!thisR.IsDisposed && db.Row.Contains(thisR)) {

                #region Filtercol erstellen

                var f = new FilterCollection(db, "Dupe Suche");

                foreach (var thisc in columns) {
                    f.Add(new FilterItem(thisc, BlueDatabase.Enums.FilterType.Istgleich_GroßKleinEgal_MultiRowIgnorieren, thisR.CellGetString(thisc)));
                }

                #endregion

                #region Zeilen ermitteln (rows)

                var rows = f.Rows.Intersect(r).ToList();

                #endregion

                if (rows.Count > 1) {
                    if (optFülle.Checked) {

                        #region Leere Werte befüllen

                        foreach (var thisC in db.Column) {

                            #region neuen Wert zum Reinschreiben ermitteln (Wert)

                            var wert = string.Empty;
                            foreach (var thisR2 in rows) {
                                if (string.IsNullOrEmpty(wert)) { wert = thisR2.CellGetString(thisC); }
                            }

                            #endregion

                            #region Wert in leere Zellen reinscheiben

                            foreach (var thisR2 in rows) {
                                if (string.IsNullOrEmpty(thisR2.CellGetString(thisC))) { thisR2.CellSet(thisC, wert, "Zeilenbereinigungs-Dialog"); }
                            }

                            #endregion
                        }

                        #endregion
                    } else if (optLöschen.Checked) {

                        #region Jüngste löschen

                        var ToDel = rows[0];

                        foreach (var thisR2 in rows) {
                            if (thisR2.CellGetDateTime(db.Column.SysRowCreateDate).Subtract(ToDel.CellGetDateTime(db.Column.SysRowCreateDate)).TotalDays < 0) {
                                ToDel = thisR2;
                            }
                        }

                        db.Row.Remove(ToDel, "RowCleanUp");

                        #endregion
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