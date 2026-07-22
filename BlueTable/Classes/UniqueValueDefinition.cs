// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Collections.ObjectModel;

namespace BlueTable.Classes;

public sealed class UniqueValueDefinition : ParseableItem, IParseable, IEditable, IHasTable, IEquatable<UniqueValueDefinition>, IReadableTextWithKey, IErrorCheckable {

    #region Fields

    private readonly List<ColumnItem> _internal = [];

    #endregion

    #region Constructors

    public UniqueValueDefinition(Table table, string toParse) {
        Table = table;
        this.Parse(toParse);
        KeyName = RebuildKeyName();
    }

    public UniqueValueDefinition(Table table, List<ColumnItem> columns) {
        Table = table;
        foreach (var thisColumn in columns) {
            if (thisColumn is { IsDisposed: false }) { _internal.Add(thisColumn); }
        }
        SortColumnsBySaveOrder();
        KeyName = RebuildKeyName();
    }

    #endregion

    #region Properties

    public string CaptionForEditor => "Unique-Wert-Definition";

    public ReadOnlyCollection<ColumnItem> KeyColumns => _internal.AsReadOnly();
    public string KeyName { get; private set; }

    public string QuickInfo => string.Empty;
    public Table Table { get; }

    #endregion

    #region Methods

    public bool Equals(UniqueValueDefinition? other) {
        if (other is null) { return false; }
        return string.Equals(KeyName, other.KeyName, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj) => Equals(obj as UniqueValueDefinition);

    public string ErrorReason() {
        if (Table is not { IsDisposed: false }) { return "Tabelle verworfen"; }
        if (_internal.Count == 0) { return "Mindestens eine Spalte muss ausgewählt sein."; }

        var linkedColumns = _internal.Where(c => c.RelationType != RelationType.None).ToList();
        if (linkedColumns.Count > 0) {
            return "Verlinkte Spalten können nicht in einer Unique-Definition sein: " + string.Join(", ", linkedColumns.Select(c => c.KeyName));
        }

        return string.Empty;
    }

    string IEditable.IsNowEditable() {
        if (Table is not { IsDisposed: false } tb) { return "Tabelle verworfen."; }
        return tb.IsValueEditable(TableDataType.UniqueValues, string.Empty);
    }

    public List<string> ParseableItems() {
        List<string> result = [];
        result.ParseableAdd("Columns", _internal, true);
        return result;
    }

    public override void ParseFinished(string parsed) {
        base.ParseFinished(parsed);
        SortColumnsBySaveOrder();
        KeyName = RebuildKeyName();
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "identifier":
                if (value != "UniqueValueDefinition") { Develop.DebugError("Identifier fehlerhaft: " + value); }
                return true;

            case "column":
            case "columnkey":
            case "columnname":
                if (Table.Column[value] is { } c) { _internal.Add(c); }
                return true;

            case "columns":
                var cols = value.FromNonCritical().SplitBy("|");
                foreach (var thisc in cols) {
                    if (Table.Column[thisc] is { } c2) { _internal.Add(c2); }
                }
                return true;
        }
        return false;
    }

    public string ReadableText() => _internal.Count == 0 ? "(leer)" : string.Join(";", _internal.Select(x => x.Caption));

    /// <summary>
    /// Übernimmt alle Werte von <paramref name="other"/>. Wird beim Recycling in
    /// <see cref="Table.SetValueInternal"/> (case UniqueValues) verwendet:
    /// Existierende Instanzen behalten ihre Identität, nur die Spalten und der
    /// KeyName werden aktualisiert.
    /// </summary>
    internal void UpdateFrom(UniqueValueDefinition other) {
        _internal.Clear();
        _internal.AddRange(other._internal);
        KeyName = other.KeyName;
    }

    public void Repair() {
        if (_internal.Count == 0) { return; }
        if (Table is not { IsDisposed: false } tb) { return; }
        if (!string.IsNullOrEmpty(tb.IsValueEditable(TableDataType.UniqueValues, TableChunk.Chunk_Master))) { return; }

        _internal.RemoveAll(c => c is not { IsDisposed: false });

        if (tb.Column.ChunkValueColumn is { } cvc && !_internal.Contains(cvc)) {
            _internal.Add(cvc);
        }

        // Muss hier gemacht werden!
        // Bei den Spaltenprüfungen hat man nur noch die Möglichkeit, die Verlinkung zu löschen.
        _internal.RemoveAll(c => c.RelationType != RelationType.None);

        SortColumnsBySaveOrder();
        KeyName = RebuildKeyName();
    }

    public QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Schloss, 16);

    public override string ToString() => ParseableItems().FinishParseable();

    private string RebuildKeyName() => _internal.Count == 0
            ? "LEER"
            : string.Join(";", _internal.Select(x => x.KeyName)).ToUpperInvariant();

    /// <summary>
    /// Sortiert die Spalten in der Reihenfolge, in der sie auch gespeichert werden
    /// (Ansicht 1 / Index 1, danach alphabetisch). Siehe <see cref="Table.ColumnsInSaveOrder" />.
    /// </summary>
    private void SortColumnsBySaveOrder() {
        if (Table is not { IsDisposed: false }) { return; }

        var saveOrder = Table.ColumnsInSaveOrder();
        if (saveOrder.Count == 0) { return; }

        _internal.Sort((a, b) => saveOrder.IndexOf(a).CompareTo(saveOrder.IndexOf(b)));
    }

    #endregion
}