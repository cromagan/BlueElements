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
/// Im Value markiert eine "## Text"-Zeile ein Kapitel: Beim Import wird der Text
/// in die Kapitel-Spalte der folgenden Zeilen geschrieben, beim Export wird bei
/// einem Wechsel der Kapitel-Spalte eine eigene "## Text"-Zeile ausgegeben.
/// Das Kontextmenü einer Zeile wird durch die Skripte
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
    /// Berechnet den benötigten Bereich anhand der Tabelleninhalte: Die
    /// Tabelle wird so groß, dass bevorzugt alle Zeilen (inkl. neuer Zeile)
    /// und alle Spalten sichtbar sind — inklusive der Spaltenköpfe, sie sind
    /// Teil der View-Items. Der Bereich beginnt um die Höhe der Kopfleisten
    /// über der Zelllinie, damit die Spaltenköpfe über der Zelle liegen;
    /// unterhalb der Linie wird mindestens die volle Zellhöhe aufgefüllt. Die
    /// TableView begrenzt das Ergebnis auf den verfügbaren Platz.
    /// </summary>
    public override Rectangle CalculateRequiredBounds(Rectangle bounds) {
        var required = base.CalculateRequiredBounds(bounds);

        if (_control is not { IsDisposed: false } tv || _table is not { IsDisposed: false }) { return required; }
        if (tv.CurrentArrangement is not { } ca) { return required; }

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

        // Kopfleisten über die Zelllinie; unterhalb der Linie mindestens die
        // volle Zellhöhe auffüllen.
        var shift = tv.RowsAreaTop();
        return new Rectangle(required.X, required.Y - shift,
            Math.Max(required.Width, width), Math.Max(required.Height + shift, height));
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

    /// <summary>
    /// Entfernt alle Zeilen und setzt die Scroll-Position der Ansicht zurück.
    /// </summary>
    public override void Reset() {
        base.Reset();
        if (_control is { IsDisposed: false } tv) {
            tv.OffsetX = 0;
            tv.OffsetY = 0;
        }

        if (_table is not { IsDisposed: false } tb) { return; }

        _suppressEvents = true;
        try {
            var existing = tb.Row.ToList();
            if (existing.Count > 0) { _ = RowCollection.Remove(existing, "Reset"); }
        } finally {
            _suppressEvents = false;
        }
    }

    /// <summary>
    /// Idempotent: laufende Abos werden zuerst abgemeldet, damit das erneute
    /// Abonnieren nach einem BuildTable innerhalb des Value-Setters keine
    /// Doppel-Subskription erzeugt.
    /// </summary>
    public override void SubscribeEvents() {
        UnsubscribeEvents();
        if (_table is { IsDisposed: false } tb) {
            tb.CellValueChanged += Table_ContentChanged;
            tb.Row.RowAdded += Table_ContentChanged;
            tb.Row.RowRemoved += Table_ContentChanged;
        }
        _control?.LostFocus += Control_LostFocus;
    }

    public override QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Tabelle);

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
        // Die Spaltenreihenfolge ist durch die Strategie fest — kein Verschieben.
        _control.ColumnMoveAllowed = false;
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
    /// Die Spalten, die den CSV-Feldern des Value entsprechen: die echten
    /// Datenspalten in Speicherreihenfolge. Systemspalten (z. B. Zeilennummern)
    /// und die Kapitel-Spalte gehören nicht in den Value. Der Schlüssel-Vergleich
    /// muss Groß/Klein-ignorierend sein, KeyName liefert Großbuchstaben.
    /// </summary>
    private static List<ColumnItem> CsvColumns(Table tb) =>
        tb.ColumnsInSaveOrder().Where(c => !c.IsSystemColumn() && !string.Equals(c.KeyName, _chapterColumnKey, StringComparison.OrdinalIgnoreCase)).ToList();

    /// <summary>
    /// Serialisiert die Tabelle als CSV. Ändert sich die Kapitel-Spalte gegenüber
    /// der vorherigen Zeile, wird vor der Zeile eine eigene "## Kapitel"-Zeile
    /// ausgegeben; die Kapitel-Spalte selbst ist nicht Teil des Value.
    /// </summary>
    private static string ExportCurrentValue(Table tb) {
        var columns = CsvColumns(tb);
        if (columns.Count == 0) { return string.Empty; }

        var chapter = tb.Column[_chapterColumnKey];
        var lines = new List<string>();
        var lastChapter = string.Empty;

        foreach (var row in tb.RowsInSaveOrder()) {
            var heading = chapter is { IsDisposed: false } ch ? row.CellGetString(ch).Trim() : string.Empty;

            if (heading != lastChapter) {
                lastChapter = heading;
                // Kapitel-Zeile roh ausgeben: CSV-Escaping würde die Zeile
                // bei Sonderzeichen quoten und beim Import als Datenzeile
                // gelesen werden.
                if (heading is { Length: > 0 }) { lines.Add("## " + heading); }
            }

            var fields = new List<string>();
            foreach (var c in columns) {
                fields.Add(CsvHelper.EscapeCSVField(row.CellGetString(c), ';'));
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

        if (AutoSort) {
            view.ColumnForChapter = null;
            view.Kontextmenu_Skripte = new[] { _deleteRowScriptKey }.AsReadOnly();
        } else {
            view.ColumnForChapter = tb.Column[_chapterColumnKey];
            view.Kontextmenu_Skripte = new[] { _deleteRowScriptKey, _addHeadingScriptKey }.AsReadOnly();
        }

        view.ScaleToFit = ScaleToFitMode.Maximum;

        // Zeilennummern bei !AutoSort über die echte Systemspalte SYS_ROWSORTINDEX
        // anzeigen — nicht über die virtuelle NumberColumnItem.
        if (tb.Column.SysRowSortIndex is { IsDisposed: false } sortCol) {
            // Feste Spaltenbreite, gemessen am Text "888": unabhängig von der Zeilenzahl.
            if (_control is { IsDisposed: false } tv) {
                sortCol.FixedColumnWidth = TableView.RendererOf(sortCol, tv.SheetStyle).ContentSize("888", sortCol.DoOpticalTranslation).Width;
            }
            view.Add(new ColumnViewItem(sortCol));
        }

        // Die Kapitel-Spalte erscheint nur als Überschriften-Zeile, niemals als Datenspalte.
        view.ShowColumns(tb.Column.Where(c => !c.IsSystemColumn() && !string.Equals(c.KeyName, _chapterColumnKey, StringComparison.OrdinalIgnoreCase)).Select(c => c.KeyName).ToArray());
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

        // Kapitel-Spalte. SaveContent muss true bleiben, sonst gibt es für
        // Skripte keine beschreibbare Variable (CellToVariable liefert bei
        // !SaveContent null). Der Export schließt die Spalte über den
        // Schlüssel aus, nicht über SaveContent.
        _ = _table.Column.GenerateAndAdd(_chapterColumnKey, "Überschrift", TextOneLineColumnFormat.Instance);

        // Systemspalten für Zeilen-Skripte (rowdelete) bereitstellen.
        _table.Column.GenerateAndAddSystem(SystemColumnKeys.RowState, SystemColumnKeys.DateChanged);

        _table.RepairAfterParse();

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

        SubscribeEvents();
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
        if (_control is not { IsDisposed: false } c) { return; }
        if (c.ContainsFocus) { return; }

        // Fokusverlust an eine mit diesem Control verbundene FloatingForm (z. B.
        // das Kontextmenü): kein echter Fokusverlust, das Edit bleibt geöffnet.
        if (FloatingForm.IsShowing(c)) { return; }

        OnLostFocus();
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

        // Macht aus der angeklickten Zeile den Anfang eines neuen Kapitels:
        // die Kapitel-Spalte erhält einen eindeutigen Namen (Zeitstempel), damit
        // er nicht mit einem bestehenden Kapitel zusammenfällt. Umbenennen per
        // Doppelklick auf die Kapitel-Zeile.
        var headingScript = new TableScriptDescription(
            tb,
            _addHeadingScriptKey,
            _chapterColumnKey + " = \"Überschrift \"+datetimeutcnow(\"fff\");\r\n" +
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

        tb.EventScript = new[] { deleteScript, headingScript }.AsReadOnly();
    }

    private void LoadCsvIntoTable(string value) {
        if (_table is not { IsDisposed: false } tb) { return; }

        _suppressEvents = true;
        try {
            var existing = tb.Row.ToList();
            if (existing.Count > 0) {
                _ = RowCollection.Remove(existing, "CSV neu geladen");
            }

            var no = 0;
            var currentChapter = string.Empty;
            var columns = CsvColumns(tb);

            if (value is { Length: > 0 }) {
                foreach (var line in value.Replace("\r\n", "\r").SplitAndCutByCr()) {
                    if (line.StartsWith("##", StringComparison.Ordinal) && _table.Column[_chapterColumnKey] is not null) {
                        currentChapter = line[2..].Trim();
                        continue;
                    }

                    var row = _table.Row.GenerateAndAdd("-", "CSV Import") ?? throw Develop.DebugError("Interner Fehler");
                    no++;

                    if (_table.Column.SysRowSortIndex is { } c) { row.CellSet(c, no, "CSV Import"); }
                    if (_table.Column[_chapterColumnKey] is { } cx) { row.CellSet(cx, currentChapter, "CSV Import"); }

                    // Felder in der Speicherreihenfolge der Spalten setzen. Mehr Felder
                    // als Spalten: der Rest landet inklusive Semikolons in der letzten Spalte.
                    var fields = CsvHelper.ParseCSVLine(line, ';').ToList();
                    for (var i = 0; i < fields.Count && i < columns.Count; i++) {
                        var field = i == columns.Count - 1 && fields.Count > columns.Count ? string.Join(';', fields.Skip(i)) : fields[i];
                        row.CellSet(columns[i], field, "CSV Import");
                    }
                }
            }
        } finally {
            _suppressEvents = false;
        }
    }

    private void Table_ContentChanged(object? sender, System.EventArgs e) {
        // Während des CSV-Imports keine Rückreaktion: ForceWriteBackValue würde
        // einen verschachtelten LoadCsvIntoTable auslösen und die Zeilen entfernen,
        // die der äußere Import gerade füllt.
        if (_suppressEvents) { return; }
        ForceWriteBackValue();
    }

    #endregion
}