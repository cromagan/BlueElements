// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Collections.ObjectModel;
using System.IO;

namespace BlueTable.Classes;

public sealed class UniqueValueDefinition : ParseableItem, IParseable, IEditable, IHasTable, IEquatable<UniqueValueDefinition>, IReadableTextWithKey, IErrorCheckable, IFragmentStoreable {

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

    // GetHashCode MUSS zu Equals passen: Equals vergleicht KeyName mit
    // OrdinalIgnoreCase, also muss der Hash auf der gleichen Normierung basieren.
    // Sonst kaputte Objekte in HashSet/Dictionary (CS0659).
    public override int GetHashCode() => KeyName.GetHashCode(StringComparison.OrdinalIgnoreCase);

    string IEditable.IsNowEditable() {
        if (Table is not { IsDisposed: false } tb) { return "Tabelle verworfen."; }
        return tb.IsValueEditable(TableDataType.UniqueValues, string.Empty);
    }

    public override List<string> ParseableItems() {
        List<string> result = [];
        result.ParseableAdd("Columns", _internal, true);
        return result;
    }

    public override bool ParseFinished() {
        var t = base.ParseFinished();
        SortColumnsBySaveOrder();
        KeyName = RebuildKeyName();
        return t;
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

    public void Repair() {
        if (_internal.Count == 0) { return; }
        if (Table is not { IsDisposed: false } tb) { return; }
        if (!string.IsNullOrEmpty(tb.IsValueEditable(TableDataType.UniqueValues, TableChunk.Chunk_Master))) { return; }

        _internal.RemoveAll(c => c is not { IsDisposed: false });

        var repaired = ColumnCollection.ColumnsIncludingChunk(tb, _internal);
        _internal.Clear();
        _internal.AddRange(repaired);

        // Muss hier gemacht werden!
        // Bei den Spaltenprüfungen hat man nur noch die Möglichkeit, die Verlinkung zu löschen.
        _internal.RemoveAll(c => c is not { IsDisposed: false } || c.RelationType != RelationType.None);

        SortColumnsBySaveOrder();
        KeyName = RebuildKeyName();
    }

    public QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Schloss, 16);

    public override string ToString() => ParseableItems().FinishParseable();

    /// <summary>
    /// Übernimmt alle Werte von <paramref name="other"/>. Wird beim Recycling in
    /// Table.SetValueInternal (case UniqueValues) verwendet:
    /// Existierende Instanzen behalten ihre Identität, nur die Spalten und der
    /// KeyName werden aktualisiert.
    /// </summary>
    internal void UpdateFrom(UniqueValueDefinition other) {
        _internal.Clear();
        _internal.AddRange(other._internal);
        KeyName = other.KeyName;
    }

    private string RebuildKeyName() => _internal.Count == 0
            ? "LEER"
            : string.Join(";", _internal.Select(x => x.KeyName)).ToUpperInvariant();

    /// <summary>
    /// Sortiert die Spalten in der Reihenfolge, in der sie auch gespeichert werden
    /// (Ansicht 1 / Index 1, danach alphabetisch). Siehe Table.ColumnsInSaveOrder.
    /// </summary>
    private void SortColumnsBySaveOrder() {
        if (Table is not { IsDisposed: false }) { return; }

        var saveOrder = Table.ColumnsInSaveOrder();
        if (saveOrder.Count == 0) { return; }

        _internal.Sort((a, b) => saveOrder.IndexOf(a).CompareTo(saveOrder.IndexOf(b)));
    }

    #region IFragmentStoreable

    /// <summary>
    /// Empfänger-Typen der Unique-Wert-Definition im binären Fragment-Format.
    /// Nur der Definition selbst bekannt.
    /// </summary>
    private enum UniqueFragmentType : byte {

        /// <summary>
        /// Spalten der Definition; Content = ColumnKeys, durch „|“ getrennt.
        /// </summary>
        Columns = 1
    }

    /// <summary>
    /// Bubbling-Event für Fragment-Änderungen (Pendant zu OnPropertyChangedExt).
    /// </summary>
    public event EventHandler<FragmentChangedEventArgs>? FragmentChanged;

    /// <summary>
    /// Eine Unique-Wert-Definition hat keine Sub-Items.
    /// </summary>
    public bool SerializeCore(Stream stream, int depth) {
        if (KeyColumns.Count == 0) { return true; }

        List<string> cols = [];
        foreach (var c in KeyColumns) { cols.Add(c.KeyName); }

        return FragmentRecord.Append(stream, depth, UniqueFragmentType.Columns, cols);
    }

    /// <summary>
    /// Verarbeitet den Definitions-Block; der Content der Spalten enthält die
    /// ColumnKeys als ListSet-Einträge. Danach werden Sortierung und KeyName neu gebaut,
    /// damit eine beim Lesen neu erzeugte Definition per KeyName auffindbar bleibt.
    /// </summary>
    public bool DeserializeThis(Stream stream, FragmentRecord record) {
        switch ((UniqueFragmentType)record.LeafType) {
            case UniqueFragmentType.Columns:
                // Erst leeren: Ein wiederverwendetes Objekt bleibt damit beim
                // Wieder-Einspielen des Blocks idempotent.
                _internal.Clear();

                foreach (var thisc in record.ListContent) {
                    if (Table.Column[thisc] is { } c) { _internal.Add(c); }
                }

                SortColumnsBySaveOrder();
                KeyName = RebuildKeyName();
                break;

            default:
                // Unbekannter Blatt-Typ — Satz überspringen
                break;
        }

        return true;
    }

    /// <summary>
    /// Löst das FragmentChanged-Event mit den fertigen Argumenten aus (Pendant zu OnPropertyChangedExt).
    /// </summary>
    public void OnFragmentChanged(FragmentChangedEventArgs e) => FragmentChanged?.Invoke(this, e);

    #endregion

    #endregion
}