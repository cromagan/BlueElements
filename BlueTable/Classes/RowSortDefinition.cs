// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Collections.ObjectModel;
using System.IO;

namespace BlueTable.Classes;

public sealed class RowSortDefinition : IParseable, IEditable, IHasTable, IEquatable<RowSortDefinition>, IFragmentStoreable {

    #region Enums

    /// <summary>
    /// Fragment-Typen der Sortier-Definition im binären Fragment-Format.
    /// Nur der Definition selbst bekannt.
    /// </summary>
    private enum SortDefinitionFragmentType : byte {

        /// <summary>
        /// Key-Name einer Sortier-Spalte (Blatt-Wert; die Spalte wird nur
        /// aufgelöst, nicht neu angelegt).
        /// </summary>
        ColumnKeyName = 1,

        /// <summary>
        /// Absteigend sortieren.
        /// </summary>
        Reverse = 2,

        /// <summary>
        /// Komplette Spaltenliste (ListSet; Content = CR-getrennte ColumnNames).
        /// </summary>
        ColumnsList = 3
    }

    #endregion

    #region Fields

    private readonly List<ColumnViewItem> _internal = [];

    #endregion

    #region Constructors

    public RowSortDefinition(Table table, string toParse) {
        Table = table;
        this.Parse(toParse);
    }

    public RowSortDefinition(Table table, ColumnViewItem? colum, bool reverse) {
        Table = table;
        Reverse = reverse;

        if (colum is { IsDisposed: false }) { _internal.Add(colum); }
    }

    public RowSortDefinition(Table table, ColumnItem? colum, bool reverse) : this(table, colum is { IsDisposed: false } ? new ColumnViewItem(colum) : null, reverse) { }

    public RowSortDefinition(Table table, List<ColumnItem> column, bool reverse) {
        Table = table;
        Reverse = reverse;

        foreach (var thisColumn in column) {
            if (thisColumn is { IsDisposed: false } c) { _internal.Add(new ColumnViewItem(c)); }
        }
    }

    #endregion

    #region Properties

    public string CaptionForEditor => "Sortierung";

    /// <summary>
    /// Es wird absteigend sortiert, der größte Wert kommt zuerst.
    /// </summary>
    public bool Reverse {
        get;
        private set {
            if (field == value) { return; }
            field = value;
            OnFragmentChanged(new FragmentChangedEventArgs(this, string.Empty, SortDefinitionFragmentType.Reverse, value));
        }
    }

    public Table Table { get; }

    /// <summary>
    /// Alle Spalten der Sortierung — echte und virtuelle Spalten einheitlich.
    /// </summary>
    public ReadOnlyCollection<ColumnViewItem> SortColumns => _internal.AsReadOnly();

    /// <summary>
    /// Die echten Spalten der Sortierung. Virtuelle Spalten sind nicht enthalten.
    /// </summary>
    public ReadOnlyCollection<ColumnItem> UsedColumns => _internal.Select(thisColumn => thisColumn.Column).OfType<ColumnItem>().ToList().AsReadOnly();

    #endregion

    #region Methods

    public bool Equals(RowSortDefinition? other) {
        if (other is null) { return false; }
        return Reverse == other.Reverse &&
               _internal.Select(x => x.ColumnName).SequenceEqual(other._internal.Select(x => x.ColumnName));
    }

    public override bool Equals(object? obj) => Equals(obj as RowSortDefinition);

    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(Reverse);
        foreach (var item in _internal) {
            hash.Add(item.ColumnName);
        }
        return hash.ToHashCode();
    }

    public string IsNowEditable() => string.Empty;

    public List<string> ParseableItems() {
        List<string> result = [];
        result.ParseableAdd("Reverse", Reverse);
        result.ParseableAdd("Columns", _internal.Select(thisColumn => thisColumn.ColumnName ?? string.Empty), true);
        return result;
    }

    public bool ParseFinished() => true;

    public bool ParseThis(string key, string value) {
        switch (key) {
            case "identifier":
                if (value != "SortDefinition") { Develop.DebugError("Identifier fehlerhaft: " + value); }
                return true;

            case "direction":
                Reverse = value == "Z-A";
                return true;

            case "reverse":
                Reverse = value.FromPlusMinus();
                return true;

            case "similarrows": // Obsolet: Ähnlichkeits-Sortierung ist eine normale Spaltensortierung
                return true;

            case "column":
            case "columnkey":
            case "columnname": // ColumnKey wichtig wegen CopyLayout
                if (SortedColumnByName(value) is { } c) { _internal.Add(c); }
                return true;

            case "columns":
                var cols = value.FromNonCritical().SplitBy("|");
                foreach (var thisc in cols) {
                    if (SortedColumnByName(thisc) is { } c2) { _internal.Add(c2); }
                }
                return true;
        }

        return false;
    }

    public void Repair() {
        if (_internal.Count == 0) { return; }
        if (Table is not { IsDisposed: false } tb) { return; }
        if (!string.IsNullOrEmpty(tb.IsValueEditable(TableDataType.SortDefinition, TableChunk.Chunk_Master))) { return; }

        // TODO: ggf. OnPropertyChanged(string propertyname) feuern, wenn Spalten entfernt werden.
        _internal.RemoveAll(IsSortColumnInvalid);
    }

    public List<RowItem> SortedRows(IEnumerable<RowItem> rows) {
        var sortedList = rows.OrderBy(item => item.CompareKey(UsedColumns)).ToList();

        if (Reverse) { sortedList.Reverse(); }
        return sortedList;
    }

    public override string ToString() => ParseableItems().FinishParseable();

    /// <summary>
    /// True, wenn die Spalte an der Sortierung beteiligt ist. Echte Spalten
    /// werden über ihre ColumnItem-Referenz erkannt, virtuelle über ihren Namen.
    /// </summary>
    public bool UsedForRowSort(ColumnItem? column) => _internal.Count != 0 && column is not null && _internal.Exists(thisColumn => thisColumn.Column == column);

    /// <summary>
    /// True, wenn die Spalte an der Sortierung beteiligt ist.
    /// </summary>
    public bool UsedForRowSort(ColumnViewItem? column) => _internal.Count != 0 && column is not null && _internal.Exists(thisColumn => SameSortColumn(thisColumn, column));

    private static bool IsSortColumnInvalid(ColumnViewItem c) =>
        c is not { IsDisposed: false }
        || c.Column is { IsDisposed: true }
        || (c.Column is null && string.IsNullOrEmpty(c.StorageKey));

    private static bool SameSortColumn(ColumnViewItem? a, ColumnViewItem? b) {
        if (a is null || b is null) { return false; }
        if (a.Column is not null || b.Column is not null) { return a.Column == b.Column; }
        return string.Equals(a.ColumnName, b.ColumnName, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Löst einen Spaltennamen zur Sortierspalte auf. Virtuelle Spalten
    /// (VIR_-Präfix) werden generisch über ihren Typnamen erzeugt — derselbe
    /// Mechanismus wie in ColumnViewItem.Create.
    /// </summary>
    private ColumnViewItem? SortedColumnByName(string name) {
        if (string.IsNullOrEmpty(name)) { return null; }

        if (name.StartsWith("VIR_", StringComparison.OrdinalIgnoreCase)) { return ParseableItem.NewByTypeName<ColumnViewItem>(name); }

        if (Table.Column[name] is not { IsDisposed: false } c) { return null; }
        return new ColumnViewItem(c);
    }

    #endregion

    #region IFragmentStoreable

    /// <summary>
    /// Bubbling-Event für Fragment-Änderungen (Pendant zu OnPropertyChangedExt).
    /// </summary>
    public event EventHandler<FragmentChangedEventArgs>? FragmentChanged;

    /// <summary>
    /// Schreibt die Sortier-Definition: Reverse nur wenn gesetzt, danach die
    /// Sortier-Spalten als Blatt-Sätze (Content = ColumnName).
    /// </summary>
    public bool SerializeCore(Stream stream, int depth) {
        var allok = true;

        allok &= FragmentRecord.Append(stream, depth, SortDefinitionFragmentType.Reverse, Reverse);

        foreach (var thisColumn in _internal) {
            allok &= FragmentRecord.Append(stream, depth, SortDefinitionFragmentType.ColumnKeyName, thisColumn.ColumnName ?? string.Empty);
        }

        return allok;
    }

    /// <summary>
    /// Verarbeitet den Sortier-Definitions-Block: Reverse im Switch, Sortier-Spalten
    /// über den ColumnKeyName-Satz nur auflösen — fehlende Spalten werden
    /// übersprungen, nicht neu angelegt. ColumnsList setzt die komplette Liste neu
    /// (ListSet; Replay mehrerer Blöcke bleibt damit deterministisch).
    /// </summary>
    public bool DeserializeThis(Stream stream, FragmentRecord record) {
        switch ((SortDefinitionFragmentType)record.LeafType) {
            case SortDefinitionFragmentType.Reverse:
                Reverse = record.BoolContent;
                break;

            case SortDefinitionFragmentType.ColumnKeyName:
                if (SortedColumnByName(record.StringContent) is { } target) { _internal.Add(target); }
                break;

            case SortDefinitionFragmentType.ColumnsList:
                _internal.Clear();
                foreach (var thisColumn in record.ListContent) {
                    if (SortedColumnByName(thisColumn) is { } newTarget) { _internal.Add(newTarget); }
                }
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
}
