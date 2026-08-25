// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueControls.TableElements;
using BlueTable.ClassesStatic;
using BlueTable.ColumnFormats;

namespace BlueControls.ControlStrategies;

/// <summary>
/// Strategy, die eine <see cref="TableView" /> mit bearbeitbaren CSV-Daten anzeigt.
/// <see cref="Columns" /> enthält die Spaltenbeschriftungen; die internen
/// Spalten-Schlüssel sind "Column_" + laufende Nummer. Der Value ist CSV-serialisiert:
/// Spalten getrennt mit ";", Zeilen getrennt mit CR.
/// Beginnt eine Zeile (erste Spalte) mit "##", wird der Text als Überschrift in die
/// Kapitel-Spalte geschrieben. Das Kontextmenü einer Zeile wird durch die Skripte
/// "Zeile löschen" und "Überschrift hinzufügen" ersetzt. Bei
/// <see cref="ControlStrategy.AutoSort" /> == false werden Zeilennummern über die
/// Systemspalte SYS_ROWSORTINDEX eingeblendet.
/// </summary>
public class TableControlStrategy : ControlStrategy {

    #region Fields

    private const string _addHeadingScriptKey = "Überschrift hinzufügen";

    private const string _chapterColumnKey = "Ueberschrift";

    private const string _columnsKey = "columns";

    private const string _deleteRowScriptKey = "Zeile löschen";

    private TableView? _control;

    private bool _isConvertingHeadings;

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
    /// Berechnet die benötigte Größe anhand der Tabelleninhalte: Die Tabelle
    /// wird so groß, dass bevorzugt alle Zeilen (inkl. neuer Zeile) und alle
    /// Spalten sichtbar sind. Die TableView begrenzt das Ergebnis auf den
    /// verfügbaren Platz.
    /// </summary>
    public override Size CalculateRequiredSize(int minWidth, int minHeight) {
        var size = base.CalculateRequiredSize(minWidth, minHeight);

        if (_control is not { IsDisposed: false } tv || _table is not { IsDisposed: false }) { return size; }
        if (tv.CurrentArrangement is not { } ca) { return size; }

        // Breite: Spalten mit sehr großer verfügbarer Breite berechnen, damit
        // ScaleToFit nicht verkleinert und die natürlichen Inhaltsbreiten übrig
        // bleiben. Zeilennummerierung und Pin-Spalte sind als permanente
        // Spalten enthalten; Reserve für den senkrechten Scrollbalken.
        ca.Invalidated = true;
        ca.ComputeAllColumnPositions(int.MaxValue / 4, tv.Zoom);
        var width = ca.ControlColumnsPermanentWidth() + ZoomPad.SliderSize;

        // Höhe: Spaltenköpfe oben, alle Zeilen — plus Reserve für eine neue
        // Zeile und den waagerechten Scrollbalken.
        var items = tv.SortedViewItems;
        var canvasBottom = 0;

        if (items is { Count: > 0 }) {
            canvasBottom = items.Max(i => i.CanvasPosition.Bottom);

            // Das Neue-Zeile-Element fehlt (z. B. Rechte fehlen): Höhen-Reserve
            // über die letzte vorhandene Zeile ergänzen.
            if (items.TrueForAll(i => i is not NewRowTableElement)
                && items.FindLast(i => i is RowTableElement) is { } lastRow) {
                canvasBottom += lastRow.CanvasPosition.Height;
            }
        }

        var height = canvasBottom.CanvasToControl(tv.Zoom) + ZoomPad.SliderSize;

        return new Size(Math.Max(size.Width, width), Math.Max(size.Height, height));
    }

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
        Value = ExportCurrentValue(tb);
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
        if (!_suppressEvents && ExportCurrentValue(tb) == value) { return; }
        LoadCsvIntoTable(value);
    }

    /// <summary>
    /// Serialisiert die Tabelle als CSV. Zeilen mit Kapitel schreiben "## Kapitel"
    /// in die erste Spalte; die Kapitel-Spalte selbst ist nicht Teil des Value.
    /// </summary>
    private static string ExportCurrentValue(Table tb) {
        var columns = tb.ColumnsInSaveOrder().Where(c => c.SaveContent).ToList();
        if (columns.Count == 0) { return string.Empty; }

        var chapter = tb.Column[_chapterColumnKey];
        var lines = new List<string>();

        foreach (var row in tb.RowsInSaveOrder()) {
            var heading = chapter is { IsDisposed: false } ch ? row.CellGetString(ch).Trim() : string.Empty;

            var fields = new List<string>();
            for (var i = 0; i < columns.Count; i++) {
                var cellValue = row.CellGetString(columns[i]);
                if (i == 0 && heading.Length > 0) { cellValue = "## " + heading; }
                fields.Add(CsvHelper.EscapeCSVField(cellValue, ';'));
            }
            lines.Add(string.Join(';', fields));
        }

        return string.Join('\r', lines);
    }

    private void BuildArrangement() {
        if (_table is not { IsDisposed: false } tb) { return; }

        var tcvc = ColumnViewCollection.ParseAll(tb);
        while (tcvc.Count < 2) { tcvc.Add(new ColumnViewCollection(tb, string.Empty)); }

        var view = tcvc[1];
        view.RemoveAll();
        view.KeyName = "CSV-Ansicht";
        view.Kontextmenu_Skripte = new[] { _deleteRowScriptKey, _addHeadingScriptKey }.AsReadOnly();
        view.ColumnForChapter = tb.Column[_chapterColumnKey];
        view.ScaleToFit = ScaleToFitMode.Aggressiv;

        // Zeilennummern-Spalte nur, wenn SysRowSortIndex aktiv ist (!AutoSort).
        if (tb.Column.SysRowSortIndex is { IsDisposed: false }) {
            view.Add(new NumberColumnItem());
        }

        // Die Kapitel-Spalte erscheint nur als Überschriften-Zeile, niemals als Datenspalte.
        view.ShowColumns(tb.Column.Where(c => !c.IsSystemColumn() && c.KeyName != _chapterColumnKey).Select(c => c.KeyName).ToArray());
        if (tb.Column[_chapterColumnKey] is { } chapterCol && view[chapterCol] is { } chapterView) { view.Remove(chapterView); }
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

        // Kapitel-Spalte für ##-Überschriften. SaveContent = false: der Wert wird
        // beim Export als "## "-Präfix der ersten Spalte serialisiert, nicht als eigenes Feld.
        var chapterColumn = _table.Column.GenerateAndAdd(_chapterColumnKey, "Überschrift", TextOneLineColumnFormat.Instance);
        if (chapterColumn is { IsDisposed: false }) { chapterColumn.SaveContent = false; }

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

        // Systemspalten sind Laufzeit-Hilfsspalten und gehören nicht in den
        // CSV-Value. Die eingebettete Tabelle wird nie gefiltert.
        foreach (var c in _table.Column) {
            if (c.IsSystemColumn()) { c.SaveContent = false; }
            c.FilterOptions = FilterOptions.None;
        }

        CreateEventScripts();
        BuildArrangement();

        _lastColumns = [.. Columns];
        _lastAutoSort = AutoSort;

        if (_table is { IsDisposed: false } newTb) {
            newTb.CellValueChanged += Table_ContentChanged;
            newTb.Row.RowAdded += Table_ContentChanged;
            newTb.Row.RowRemoved += Table_ContentChanged;
        }
    }

    private void Control_LostFocus(object? sender, System.EventArgs e) {
        if (_control is not { IsDisposed: false } c) { return; }
        if (!c.IsHandleCreated) { OnLostFocus(); return; }

        // LostFocus feuert auch mitten in der Fokus-Übergabe an Kind-Controls
        // (z. B. das Inline-Edit der Tabelle) — bevor das Kind den Fokus erhalten
        // hat. Die Prüfung daher verzögert ausführen.
        _ = c.BeginInvoke(new System.Action(Control_LostFocusDeferred));
    }

    private void Control_LostFocusDeferred() {
        if (_control is { IsDisposed: false, ContainsFocus: false }) { OnLostFocus(); }
    }

    /// <summary>
    /// Verschiebt Zeilen, deren erste Spalte mit ## beginnt, in die Kapitel-Spalte.
    /// "##Text" und "## Text" werden gleichermaßen erkannt; die Überschrift ist
    /// der restliche Text ohne führende Leerzeichen.
    /// </summary>
    private void ConvertHeadingRows() {
        if (_table is not { IsDisposed: false } tb) { return; }
        if (tb.Column.First is not { IsDisposed: false } first) { return; }
        if (tb.Column[_chapterColumnKey] is not { IsDisposed: false } chapter) { return; }

        _isConvertingHeadings = true;
        try {
            foreach (var row in tb.Row) {
                if (row is not { IsDisposed: false }) { continue; }

                var text = row.CellGetString(first).Trim();
                if (!text.StartsWith("##", StringComparison.Ordinal)) { continue; }

                var caption = text[2..].Trim();
                row.CellSet(first, string.Empty, "Überschrift");
                row.CellSet(chapter, caption, "Überschrift");
            }
        } finally {
            _isConvertingHeadings = false;
        }
    }

    private void CreateEventScripts() {
        if (_table is not { IsDisposed: false } tb) { return; }

        var deleteScript = new TableScriptDescription(
            tb,
            _deleteRowScriptKey,
            "rowdelete(CurrentRow);\r\nShortSuccessMessage = \"Zeile gelöscht\";",
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

        // Macht aus der angeklickten Zeile eine Überschrift: weist der ersten
        // Spalte "## <Text>" zu — ConvertHeadingRows verschiebt sie in die
        // Kapitel-Spalte. Die Millisekunden machen den Text eindeutig.
        TableScriptDescription? headingScript = null;
        if (tb.Column.First is { IsDisposed: false } first) {
            headingScript = new TableScriptDescription(
                tb,
                _addHeadingScriptKey,
                first.KeyName + " = \"## Überschrift \"+datetimeutcnow(\"fff\");\r\n" +
                "ShortSuccessMessage = \"Überschrift hinzugefügt\";",
                "Textfeld|16",
                "Macht aus der Zeile eine Überschrift.",
                string.Empty,
                new[] { Everybody }.AsReadOnly(),
                ScriptEventTypes.Ohne_Auslöser,
                true,
                false,
                string.Empty,
                null,
                0,
                0);
        }

        tb.EventScript = headingScript is null
            ? new[] { deleteScript }.AsReadOnly()
            : new[] { deleteScript, headingScript }.AsReadOnly();
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

            // "## "-Zeilen aus dem CSV in die Kapitel-Spalte übernehmen.
            ConvertHeadingRows();
        } finally {
            _suppressEvents = false;
        }
    }

    private void Table_ContentChanged(object? sender, System.EventArgs e) {
        // ##-Eingaben in die Kapitel-Spalte umleiten, dann den Value aktualisieren.
        if (!_suppressEvents && !_isConvertingHeadings) { ConvertHeadingRows(); }
        ForceWriteBackValue();
    }

    #endregion
}