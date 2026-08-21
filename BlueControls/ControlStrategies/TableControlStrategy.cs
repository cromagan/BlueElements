// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueTable.ClassesStatic;
using BlueTable.ColumnFormats;

namespace BlueControls.ControlStrategies;

/// <summary>
/// Strategy, die eine <see cref="TableView" /> mit bearbeitbaren CSV-Daten anzeigt.
/// <see cref="Columns" /> enthält die Spaltenbeschriftungen; die internen
/// Spalten-Schlüssel sind "Column_" + laufende Nummer. Der Value ist CSV-serialisiert:
/// Spalten getrennt mit ";", Zeilen getrennt mit CR.
/// Das Kontextmenü einer Zeile wird durch das Skript "Zeile löschen" ersetzt.
/// Bei <see cref="ControlStrategy.AutoSort" /> == false werden Zeilennummern
/// über die Systemspalte SYS_ROWSORTINDEX eingeblendet.
/// </summary>
public class TableControlStrategy : ControlStrategy {

    #region Fields

    private const string _columnsKey = "columns";

    private const string _deleteRowScriptKey = "Zeile löschen";

    private TableView? _control;

    private bool _lastAutoSort = true;

    private List<string> _lastColumns = [];

    private bool _suppressEvents;

    private Table? _table;

    #endregion

    #region Properties

    public static string ClassId => "Table";

    /// <summary>
    /// Die Spaltenbeschriftungen der eingebetteten Tabelle.
    /// </summary>
    public List<string> Columns {
        get;
        set {
            if (field == value) { return; }
            field = value;
            ControlStrategyParameter.Set(_columnsKey, value);

            if (IsEventsSuppressed) { return; }
            OnDoUpdateSideOptionMenu();
            ApplyStyle();
        }
    } = [];

    public override string Description => "Zeigt eine kleine Tabelle mit eigenen Spalten. Die Spalten werden in einer Liste verwaltet, Zeilen lassen sich über das Kontextmenü löschen.";

    public override string KeyName => ClassId;

    protected override System.Windows.Forms.Control? ControlCore => _control;

    #endregion

    #region Methods

    /// <summary>
    /// Option der Strategie: die Spaltenbeschriftungen der eingebetteten Tabelle.
    /// </summary>
    public override List<GenericControl> GetProperties(int widthOfControl) {
        var columnEditor = new FlexiControlForProperty<List<string>>(
            () => Columns, "Spalten", 6, null, CheckBehavior.AllSelected, AddType.Text, false) {
            RemoveAllowed = true,
            MoveAllowed = true
        };

        return [.. base.GetProperties(widthOfControl), columnEditor];
    }

    public override string ReadableText() => "Tabellenansicht";

    public override void SubscribeEvents() {
        if (_table is { IsDisposed: false } tb) {
            tb.CellValueChanged += Table_ContentChanged;
            tb.Row.RowAdded += Table_ContentChanged;
            tb.Row.RowRemoved += Table_ContentChanged;
        }
        _control?.LostFocus += Control_LostFocus;
    }

    public override QuickImage? SymbolForReadableText() => QuickImage.Get(BlueBasics.Enums.ImageCode.Tabelle);

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

        // Tabelle neu aufbauen, wenn sich Spalten oder Sortiermodus geändert hat.
        if (_table is null or { IsDisposed: true }
            || _lastAutoSort != AutoSort
            || Columns.IsDifferentTo(_lastColumns)) {
            BuildTable();
        }

        if (_control.Table != _table && _table is { IsDisposed: false }) {
            _control.Table = _table;
            _control.Arrangement = string.Empty;
        }
        _control.Zoom = Zoom;

        _control.QuickInfo = QuickInfo;
    }

    protected override void CreateControlCore() => _control = new TableView();

    protected override void Dispose(bool disposing) {
        if (disposing) {
            base.Dispose(disposing);

            if (_table is { IsDisposed: false } tb) {
                tb.CellValueChanged -= Table_ContentChanged;
                tb.Row.RowAdded -= Table_ContentChanged;
                tb.Row.RowRemoved -= Table_ContentChanged;
            }
            if (_table is { IsDisposed: false }) { _table.Dispose(); }
            _table = null;
        }
    }

    protected override void ForceWriteBackValue() {
        if (_table is not { IsDisposed: false } tb) { return; }
        Value = CsvHelper.ExportCSV(tb, ';', false);
    }

    protected override void ReadParameters(JsonObject json) {
        base.ReadParameters(json);
        Columns = json.GetListString(_columnsKey, Columns);
    }

    protected override void SetValueToControlInternal(string value) {
        // Value kann vor dem ersten ApplyStyle gesetzt werden (FlexiControl-Reihenfolge):
        // dann die Tabelle hier on-demand aufbauen, sonst würde der Wert verworfen.
        if (_table is not { IsDisposed: false }) { ApplyStyle(); }
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
        view.Kontextmenu_Skripte = new[] { _deleteRowScriptKey }.AsReadOnly();

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

        // Spalten erzeugen: der Benutzer-Text wird zur Caption,
        // der interne Schlüssel ist "Column_" + laufende Nummer.
        ColumnItem? firstColumn = null;
        var nr = 0;
        foreach (var caption in Columns) {
            if (caption.Trim() is not { Length: > 0 } cap) { continue; }
            nr++;
            var c = _table.Column.GenerateAndAdd("Column_" + nr.ToString1(), cap, TextOneLineColumnFormat.Instance);
            if (firstColumn is null && c is { IsDisposed: false }) { firstColumn = c; }
        }

        // Fallback: mindestens eine Spalte, falls Columns leer.
        firstColumn ??= _table.Column.GenerateAndAdd("Wert", "Wert", TextOneLineColumnFormat.Instance);
        if (firstColumn is { IsDisposed: false }) { firstColumn.IsFirst = true; }

        // Systemspalten für Zeilen-Skripte (rowdelete) bereitstellen.
        _table.Column.GenerateAndAddSystem(SystemColumnKeys.RowState, SystemColumnKeys.DateChanged);

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

        // Systemspalten sind Laufzeit-Hilfsspalten und gehören nicht in den CSV-Value.
        foreach (var c in _table.Column.Where(c => c.IsSystemColumn())) {
            c.SaveContent = false;
        }

        CreateDeleteRowScript();
        BuildArrangement();

        _lastColumns = [.. Columns];
        _lastAutoSort = AutoSort;

        if (_table is { IsDisposed: false } newTb) {
            newTb.CellValueChanged += Table_ContentChanged;
            newTb.Row.RowAdded += Table_ContentChanged;
            newTb.Row.RowRemoved += Table_ContentChanged;
        }
    }

    private void Control_LostFocus(object? sender, System.EventArgs e) => OnLostFocus();

    private void CreateDeleteRowScript() {
        if (_table is not { IsDisposed: false } tb) { return; }

        var script = new TableScriptDescription(
            tb,
            _deleteRowScriptKey,
            "rowdelete(CurrentRow);",
            "Zeile|16|||||||||Kreuz",
            "Löscht die Zeile, auf die geklickt wurde.",
            string.Empty,
            new[] { Everybody }.AsReadOnly(),
            ScriptEventTypes.Ohne_Auslöser,
            true,
            true,
            string.Empty,
            null,
            0,
            0);

        tb.EventScript = new[] { script }.AsReadOnly();
    }

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

    private void Table_ContentChanged(object? sender, System.EventArgs e) => ForceWriteBackValue();

    #endregion
}