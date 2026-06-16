// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Collections.ObjectModel;

namespace BlueTable.Classes;

public sealed class RowSortDefinition : IParseable, IEditable, IHasTable, IEquatable<RowSortDefinition> {

    #region Fields

    private readonly List<ColumnItem> _internal = [];

    #endregion

    #region Constructors

    public RowSortDefinition(Table table, string toParse) {
        Table = table;
        this.Parse(toParse);
    }

    public RowSortDefinition(Table table, ColumnItem? colum, bool reverse) {
        Table = table;
        Reverse = reverse;

        if (colum is { IsDisposed: false }) { _internal.Add(colum); }
    }

    public RowSortDefinition(Table table, List<ColumnItem> column, bool reverse) {
        Table = table;
        Reverse = reverse;

        foreach (var thisColumn in column) {
            if (thisColumn is { IsDisposed: false } c) { _internal.Add(c); }
        }
    }

    #endregion

    #region Properties

    public string CaptionForEditor => "Sortierung";
    public bool Reverse { get; private set; }
    public Table Table { get; }
    public ReadOnlyCollection<ColumnItem> UsedColumns => _internal.AsReadOnly();

    #endregion

    #region Methods

    public bool Equals(RowSortDefinition? other) {
        if (other is null) { return false; }
        return Reverse == other.Reverse &&
               _internal.Select(x => x.KeyName).SequenceEqual(other._internal.Select(x => x.KeyName));
    }

    public override bool Equals(object? obj) => Equals(obj as RowSortDefinition);

    public override int GetHashCode() {
        unchecked {
            var hash = 17;
            hash = hash * 23 + Reverse.GetHashCode();
            foreach (var item in _internal) {
                hash = hash * 23 + (item.KeyName?.GetHashCode() ?? 0);
            }

            return hash;
        }
    }

    public string IsNowEditable() => string.Empty;

    public List<string> ParseableItems() {
        List<string> result = [];
        result.ParseableAdd("Reverse", Reverse);
        result.ParseableAdd("Columns", _internal, true);
        return result;
    }

    public void ParseFinished(string parsed) { }

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

            case "column":
            case "columnkey":
            case "columnname": // ColumnKey wichtig wegen CopyLayout
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

    public void Repair() {
        if (_internal.Count == 0) { return; }

        if (Table is not { IsDisposed: false } tb) { return; }

        if (!string.IsNullOrEmpty(tb.IsValueEditable(TableDataType.SortDefinition, TableChunkFragments.Chunk_Master))) { return; }

        for (var i = 0; i < _internal.Count; i++) {
            if (_internal[i] is not { IsDisposed: false }) {
                _internal.RemoveAt(i);
                //OnPropertyChanged(string propertyname);
                Repair();
                return;
            }
        }
    }

    public List<RowItem> SortedRows(IEnumerable<RowItem> rows) {
        var sortedList = rows.OrderBy(item => item.CompareKey(_internal)).ToList();

        if (Reverse) { sortedList.Reverse(); }
        return sortedList;
    }

    public override string ToString() => ParseableItems().FinishParseable();

    public bool UsedForRowSort(ColumnItem? column) => _internal.Count != 0 && _internal.Exists(thisColumn => thisColumn == column);

    #endregion
}