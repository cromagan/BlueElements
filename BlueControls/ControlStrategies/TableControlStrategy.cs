// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueTable.ClassesStatic;
using BlueTable.ColumnFormats;

namespace BlueControls.ControlStrategies;

/// <summary>
/// Strategy, die eine <see cref="TableView" /> mit bearbeitbaren CSV-Daten anzeigt.
/// <see cref="ControlStrategy.StrategyParameter" /> enthält die Spaltennamen,
/// getrennt mit ";" (z. B. "Spalte1;Spalte2;Spalte3"). Der Value ist CSV-serialisiert:
/// Spalten getrennt mit ";", Zeilen getrennt mit CR.
/// Bei <see cref="ControlStrategy.AutoSort" /> == false werden Zeilennummern
/// über die Systemspalte SYS_ROWSORTINDEX eingeblendet.
/// </summary>
public class TableControlStrategy : ControlStrategy {

    #region Fields

    private TableView? _control;
    private bool _lastAutoSort = true;
    private string _lastColumns = "\u0001";
    private string _pendingValue = string.Empty;
    private bool _suppressEvents;
    private Table? _table;

    #endregion

    #region Properties

    public static string ClassId => "Table";

    public override System.Windows.Forms.Control? Control => _control;

    public override string KeyName => ClassId;

    #endregion

    #region Methods

    public override void CreateControl() => _control = new TableView();

    public override void SubscribeEvents() {
        if (_table is { IsDisposed: false } tb) {
            tb.CellValueChanged += Table_ContentChanged;
            tb.Row.RowAdded += Table_ContentChanged;
            tb.Row.RowRemoved += Table_ContentChanged;
        }
        _control?.LostFocus += Control_LostFocus;
    }

    public override void UnsubscribeEvents() {
        if (_table is { IsDisposed: false } tb) {
            tb.CellValueChanged -= Table_ContentChanged;
            tb.Row.RowAdded -= Table_ContentChanged;
            tb.Row.RowRemoved -= Table_ContentChanged;
        }
        _control?.LostFocus -= Control_LostFocus;
    }

    protected override void ApplyStyle() {
        if (_control is null) { return; }

        // Bei bestehender Tabelle: aktuellen Wert sichern, bevor evtl. neu aufgebaut wird.
        if (_table is { IsDisposed: false } tb && !_suppressEvents) {
            _pendingValue = CsvHelper.ExportCSV(tb, ';', false);
        }

        // Tabelle neu aufbauen, wenn sich Spalten oder Sortiermodus geändert hat.
        if (_table is null or { IsDisposed: true }
            || _lastAutoSort != AutoSort) {
            BuildTable();
        }

        if (_control.Table != _table && _table is { IsDisposed: false }) {
            _control.Table = _table;
            _control.Arrangement = string.Empty;
        }
        _control.Zoom = Zoom;
        LoadCsvIntoTable(_pendingValue);
        _control.QuickInfo = QuickInfo;
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            if (_table is { IsDisposed: false } tb) {
                tb.CellValueChanged -= Table_ContentChanged;
                tb.Row.RowAdded -= Table_ContentChanged;
                tb.Row.RowRemoved -= Table_ContentChanged;
            }
            if (_table is { IsDisposed: false }) { _table.Dispose(); }
            _table = null;
        }
        base.Dispose(disposing);
    }

    protected override void SetValueToControlInternal(string value) {
        _pendingValue = value;
        if (_table is not { IsDisposed: false } tb) { return; }
        // Durch Benutzereingabe ausgelöste Value-Änderungen nicht neu laden,
        // die Tabelle enthält den Wert bereits.
        if (!_suppressEvents && CsvHelper.ExportCSV(tb, ';', false) == value) { return; }
        LoadCsvIntoTable(value);
    }

    private void BuildArrangement() {
        if (_table is not { IsDisposed: false } tb) { return; }

        var tcvc = ColumnViewCollection.ParseAll(tb);
        while (tcvc.Count < 2) { tcvc.Add(new ColumnViewCollection(tb, string.Empty)); }

        var view = tcvc[1];
        view.RemoveAll();
        view.KeyName = "CSV-Ansicht";

        // Zeilennummern-Spalte nur, wenn SysRowSortIndex aktiv ist (!AutoSort).
        if (tb.Column.SysRowSortIndex is { IsDisposed: false }) {
            view.Add(new NumberColumnItem());
        }

        view.ShowColumns(tb.Column.Where(c => !c.IsSystemColumn()).Select(c => c.KeyName).ToArray());
        view.Repair(1);

        tb.ColumnArrangements = tcvc.AsReadOnly();
    }

    private void BuildTable() {
        if (_table is { IsDisposed: false } oldTb) {
            oldTb.CellValueChanged -= Table_ContentChanged;
            oldTb.Row.RowAdded -= Table_ContentChanged;
            oldTb.Row.RowRemoved -= Table_ContentChanged;
        }
        if (_table is { IsDisposed: false }) { _table.Dispose(); }

        _table = Table.Get();
        _table.LogUndo = false;
        _table.DropMessages = false;
        // Tabelle für jeden Benutzer voll editierbar machen, damit die TableView
        // den Hinzufügen-Button anbietet und Zellen bearbeitet werden können.
        _table.TableAdmin = new[] { Everybody }.AsReadOnly();
        _table.PermissionGroupsNewRow = new[] { Everybody }.AsReadOnly();

        // Spalten aus StrategyParameter ("Spalte1;Spalte2;Spalte3") erzeugen.
        var added = false;
        foreach (var raw in StrategyParameter.SplitBy(";")) {
            var name = raw.Trim();
            if (string.IsNullOrEmpty(name)) { continue; }
            var c = _table.Column.GenerateAndAdd(name, name, TextOneLineColumnFormat.Instance);
            if (!added && c is { IsDisposed: false }) {
                c.IsFirst = true;
                added = true;
            }
        }

        // Fallback: mindestens eine Spalte, falls StrategyParameter leer.
        if (!added) {
            var c = _table.Column.GenerateAndAdd("Wert", "Wert", TextOneLineColumnFormat.Instance);
            if (c is { IsDisposed: false }) { c.IsFirst = true; }
        }

        _table.RepairAfterParse();

        // Sortiermodus: bei AutoSort alphabetisch nach erster Spalte,
        // sonst Zeilennummern über SYS_ROWSORTINDEX.
        if (AutoSort) {
            _table.DisableCustomSort();
            if (_table.Column.First is { IsDisposed: false } first) {
                _table.SortDefinition = new RowSortDefinition(_table, first, false);
            }
        } else {
            _table.EnableCustomSort();
        }

        BuildArrangement();

        _lastColumns = StrategyParameter;
        _lastAutoSort = AutoSort;

        if (_table is { IsDisposed: false } newTb) {
            newTb.CellValueChanged += Table_ContentChanged;
            newTb.Row.RowAdded += Table_ContentChanged;
            newTb.Row.RowRemoved += Table_ContentChanged;
        }
    }

    private void Control_LostFocus(object? sender, System.EventArgs e) => OnLostFocus();

    private void LoadCsvIntoTable(string value) {
        if (_table is not { IsDisposed: false } tb) { return; }

        _suppressEvents = true;
        try {
            var existing = tb.Row.ToList();
            if (existing.Count > 0) {
                _ = RowCollection.Remove(existing, "CSV neu geladen");
            }

            if (!string.IsNullOrEmpty(value)) {
                // CsvHelper.ImportCsv erwartet einen Header mit Spaltennamen.
                // Die Spaltenstruktur ist durch BuildTable() fest vorgegeben,
                // daher wird der Header aus den Tabellenspalten synthetisiert.
                var headerFields = tb.ColumnsInSaveOrder()
                    .Where(c => c.SaveContent)
                    .Select(c => CsvHelper.EscapeCSVField(c.KeyName, ';'));
                var header = string.Join(';', headerFields);
                _ = CsvHelper.ImportCsv(tb, header + "\r" + value, false, ';');
            }

            // Bei !AutoSort: fortlaufende Zeilennummern vergeben.
            if (!AutoSort && tb.Column.SysRowSortIndex is { IsDisposed: false }) {
                tb.RenumberRows(tb.Row, "CSV neu nummeriert");
            }
        } finally {
            _suppressEvents = false;
        }
    }

    private void Table_ContentChanged(object? sender, System.EventArgs e) {
        if (_suppressEvents) { return; }
        if (_table is not { IsDisposed: false } tb) { return; }
        OnValueChanged(CsvHelper.ExportCSV(tb, ';', false));
    }

    #endregion
}