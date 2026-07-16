// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.EventArgs;
using BlueTable.Interfaces;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.BlueTableDialogs;

public sealed partial class RowCleanUp : FormWithStatusBar, IHasTable {

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

        if (Table is { IsDisposed: false } tb) {
            //var lst =  List<AbstractListItem>();
            lstColumns.ItemAddRange(ItemsOf(tb.Column));
            //cbxColDateiname.Item = lst;
        }

        CheckButtons();
    }

    #endregion

    #region Properties

    public Table? Table {
        get;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == field) { return; }

            field?.DisposingEvent -= _table_Disposing;
            field = value;

            field?.DisposingEvent += _table_Disposing;
        }
    }

    public Controls.TableView? TableView {
        get;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == field) { return; }

            field?.VisibleRowsChanged -= _table_VisibleRowsChanged;
            field = value;

            field?.VisibleRowsChanged += _table_VisibleRowsChanged;
        }
    }

    #endregion

    #region Methods

    protected override void OnClosing(CancelEventArgs e) {
        Table = null;
        base.OnClosing(e);
    }

    private void _table_Disposing(object? sender, System.EventArgs e) {
        Table = null;
        Close();
    }

    private void _table_VisibleRowsChanged(object? sender, System.EventArgs e) => CheckButtons();

    private void Cancel_Click(object sender, System.EventArgs e) => Close();

    private void CheckButtons() {
        if (Table is null || TableView is null) {
            txtInfo.Text = "Keine Tabelle gewählt.";
            btnExecute.Enabled = false;
            return;
        }

        var r = TableView.RowsVisibleUnique().Count;

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

        //if (_table.Column[cbxColDateiname.Text] is null) {
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
        var r = TableView?.RowsVisibleUnique();

        if (r is not { Count: not 0 }) {
            MessageBox.Show("Keine Zeilen gewählt.", ImageCode.Information, "OK");
            return;
        }

        if (Table is not { IsDisposed: false } tb) { return; }
        var columns = new List<ColumnItem>();
        foreach (var column in lstColumns.Checked) {
            if (tb.Column[column] is { IsDisposed: false } c) {
                columns.Add(c);
            }
        }

        if (tb.Column.ChunkValueColumn is { } chk && !columns.Contains(chk)) {
            MessageBox.Show($"Chunk-Spalte '{chk.Caption}' muss mit gewählt werden.", ImageCode.Information, "OK");
            return;
        }

        var error = string.Empty;

        // Combine/RemoveYoungest feuern pro Duplikat-Gruppe mehrere Events
        // (CellSet + Remove). Bei vielen Gruppen baut sich die UI wiederholt
        // auf. SuppressEvents bündelt alles, ResumeEvents macht am Ende einen
        // einzigen Aufbau. Die frühen `return` im Loop werden über finally
        // sicher wieder freigegeben.
        tb.SuppressEvents();
        try {
            foreach (var thisR in r) {
                if (!thisR.IsDisposed && tb.Row.Contains(thisR)) {

                    #region Filtercol erstellen

                    var f = new FilterCollection(tb, "Dupe Suche");

                    foreach (var thisc in columns) {
                        f.Add(new FilterItem(thisc, FilterType.Istgleich_GroßKleinEgal_MultiRowIgnorieren, thisR.CellGetString(thisc)));
                    }

                    #endregion

                    #region Zeilen ermitteln (rows)

                    var rows = f.Rows.Intersect(r).ToList();

                    #endregion

                    if (rows.Count > 1) {
                        if (optFülle.Checked) {
                            error = tb.Row.Combine(rows).FailedReason;
                        } else if (optLöschen.Checked) {
                            error = tb.Row.RemoveYoungest(rows, false).FailedReason;
                        } else {
                            MessageBox.Show("Modus unbekannt.", ImageCode.Information, "OK");
                            return;
                        }
                    }

                    if (!string.IsNullOrEmpty(error)) {
                        MessageBox.Show($"Abbruch:\r\n{error}", ImageCode.Information, "OK");
                        return;
                    }
                }
            }
        } finally {
            tb.ResumeEvents();
        }
    }

    private void lstColumns_ItemClicked(object sender, AbstractListItemEventArgs e) => CheckButtons();

    #endregion
}