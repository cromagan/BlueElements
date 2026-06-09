// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Collections.ObjectModel;
using static BlueBasics.ClassesStatic.Constants;
using static BlueBasics.ClassesStatic.Converter;

namespace BlueTable.Classes;

public sealed class FilterItem : IReadableText, IParseable, ICanBeEmpty, IErrorCheckable, IHasTable, IEquatable<FilterItem> {

    #region Fields

    private string? _pendingColumnName;
    private string? _pendingTableName;

    #endregion

    #region Constructors

    public FilterItem(Table? tb, FilterType filterType, string searchValue) : this(tb, filterType, [searchValue]) { }

    /// <summary>
    /// Ein AlwaysFalse Filter
    /// </summary>
    public FilterItem(Table? table, string origin) {
        Table = table;
        FilterType = FilterType.AlwaysFalse;
        Column = null;
        Origin = origin;
        SearchValue = new List<string>().AsReadOnly();
    }

    public FilterItem(string filterCode, Table? table) {
        // Table?, weil always False keine Tabelle braucht
        Table = table;
        Origin = string.Empty;

        SearchValue = new List<string>().AsReadOnly();

        this.Parse(filterCode);
    }

    public FilterItem(ColumnItem column, double from, double to) : this(column, FilterType.Between | FilterType.UND, from.ToString1_5() + "|" + to.ToString1_5()) { }

    /// <summary>
    /// Bei diesem Construktor muss der Tag 'Table' vorkommen!
    /// </summary>
    public FilterItem(ColumnItem column, FilterType filterType, string searchValue) : this(column, filterType, [searchValue], string.Empty) { }

    public FilterItem(ColumnItem column, FilterType filterType, string searchValue, string origin) : this(column, filterType, [searchValue], origin) { }

    public FilterItem(ColumnItem column, FilterType filterType, IList<string> searchValue) : this(column, filterType, searchValue, string.Empty) { }

    public FilterItem(ColumnItem column, FilterType filterType, IList<string>? searchValue, string origin) {
        Table = column.Table;
        Column = column;
        FilterType = filterType;
        Origin = origin;

        SearchValue = searchValue is { Count: > 0 } ? new ReadOnlyCollection<string>(searchValue) : EmptyReadOnly;
    }

    public FilterItem(Table? table, ColumnItem? column, FilterType filterType, IList<string>? searchValue, string origin) {
        Table = table ?? column?.Table;

        Column = column;

        FilterType = filterType;

        Origin = origin;

        // Setze SearchValue
        SearchValue = searchValue is { Count: > 0 } ? new ReadOnlyCollection<string>(searchValue) : EmptyReadOnly;
    }

    private FilterItem(Table? table, FilterType filterType, IList<string>? searchValue) {
        //Table?, weil AlwaysFalse keine Angaben braucht
        Table = table;

        FilterType = filterType;
        Origin = string.Empty;

        SearchValue = searchValue is { Count: > 0 } ? new ReadOnlyCollection<string>(searchValue) : new List<string>().AsReadOnly();
    }

    #endregion

    #region Properties

    public ColumnItem? Column {
        get {
            if (_pendingColumnName is not null) {
                var name = _pendingColumnName;
                _pendingColumnName = null;
                Column = Table?.Column[name];
            }
            return field;
        }
        private set { field = value; }
    }

    public FilterType FilterType { get; private set; }

    public string Origin { get; private set; }

    public string QuickInfo => ReadableText();

    public ReadOnlyCollection<string> SearchValue { get; private set; }

    /// <summary>
    /// Der Edit-Dialog braucht die Tabelle, um mit Texten die Spalte zu suchen.
    /// </summary>
    public Table? Table {
        get {
            if (_pendingTableName is not null) {
                var name = _pendingTableName;
                _pendingTableName = null;
                Table = Table.Get(name, null);
            }
            return field;
        }
        private set {
            if (value?.IsDisposed ?? true) { value = null; }
            if (value == field) { return; }

            field?.DisposingEvent -= _table_Disposing;
            field = value;

            field?.DisposingEvent += _table_Disposing;
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Prüft, ob der Filter identisch ist. Die Herkunft wird dabei ignoriert.
    /// </summary>
    /// <param name="thisFilter"></param>
    /// <returns></returns>
    public bool Equals(FilterItem? thisFilter) => thisFilter?.FilterType == FilterType &&
                                                   thisFilter.Column == Column &&
                                                   thisFilter.Origin == Origin &&
                                                   string.Join('\r', thisFilter.SearchValue) == string.Join('\r', SearchValue);

    public override bool Equals(object? obj) => Equals(obj as FilterItem);

    public string ErrorReason() {
        if (FilterType == FilterType.AlwaysFalse) { return string.Empty; }

        if (Table is null) { return "Keine Tabelle angegeben"; }

        if (Table.IsDisposed) { return "Tabelle verworfen"; }

        if (FilterType is FilterType.GroßKleinEgal or FilterType.UND or FilterType.ODER or FilterType.MultiRowIgnorieren) { return "Fehlerhafter Filter"; }

        if (Column is not null && Column.Table != Table) { return "Tabellen inkonsistent"; }

        if (SearchValue.Count == 0) { return "Kein Suchtext vorhanden"; }

        if (FilterType == FilterType.RowKey) {
            if (Column is not null) { return "RowKey suche mit Spaltenangabe"; }
            if (SearchValue.Count != 1) { return "RowKey mit ungültiger Suche"; }
            return string.Empty;
        }

        if (Column is null && !FilterType.HasFlag(FilterType.Instr_GroßKleinEgal)) { return "Fehlerhafter Zeilenfilter"; }

        if (FilterType.HasFlag(FilterType.Instr) && SearchValue.Any(string.IsNullOrEmpty)) { return "Instr-Filter ohne Suchtext"; }

        if (Column?.Value_for_Chunk is not null && Column.Value_for_Chunk != ChunkType.None) {
            if (SearchValue.Count == 0) { return "Split-Spalte mit ungültiger Suche"; }
            if (FilterType is not FilterType.Istgleich
                and not FilterType.Istgleich_GroßKleinEgal
                and not FilterType.Istgleich_MultiRowIgnorieren
                and not FilterType.Istgleich_ODER_GroßKleinEgal
                and not FilterType.Istgleich_UND_GroßKleinEgal
                and not FilterType.Istgleich_GroßKleinEgal_MultiRowIgnorieren) { return "Falscher Typ"; }
        }

        foreach (var thisV in SearchValue) {
            if (thisV.Contains('~')) { return $"Unaufgelöste Variable {thisV}"; }
        }
        return string.Empty;
    }

    public override int GetHashCode() => HashCode.Combine(FilterType, Column, Origin, string.Join('\r', SearchValue));

    public bool IsNullOrEmpty() => !this.IsOk();

    public List<string> ParseableItems() {
        try {
            // Für FlexiForFilter werden auch "ungültige" Filter benötigt
            // z.B. Instr ohn Text
            //if (!this.IsOk()) { return string.Empty; }

            List<string> result = [];
            result.ParseableAdd("Type", FilterType);
            result.ParseableAdd("Table", Table);
            result.ParseableAdd("ColumnName", Column);
            result.ParseableAdd("Values", SearchValue, false);
            result.ParseableAdd("Origin", Origin);
            return result;
        } catch {
            Develop.AbortAppIfStackOverflow();
            return ParseableItems();
        }
    }

    public void ParseFinished(string parsed) {
        if (parsed.Contains(", Value=}") || parsed.Contains(", Value=,")) { SearchValue.AddIfNotExists(string.Empty); }
    }

    public bool ParseThis(string key, string value) {
        switch (key) {
            case "identifier":
                if (value != "Filter") {
                    Develop.DebugError("Identifier fehlerhaft: " + value);
                }
                return true;

            case "database":
            case "table":
                _pendingTableName = value.FromNonCritical();
                return true;

            case "type":
                FilterType = (FilterType)IntParse(value);
                return true;

            case "columnkey":
            case "columnname":
            case "column":
                _pendingColumnName = value.FromNonCritical();
                return true;

            case "values":

                SearchValue = string.IsNullOrEmpty(value)
                    ? new List<string> { string.Empty }.AsReadOnly()
                    : value.SplitBy("|").FromNonCritical().ToList().AsReadOnly();

                return true;

            case "origin":
            case "herkunft":
                Origin = value.FromNonCritical();
                return true;

            case "id":
                return true;
        }

        return false;
    }

    public string ReadableText() {
        // Bei Nich OK schön en Text zurück geben für FlexiControlForFilter
        if (!this.IsOk()) { return "Filter ohne Funktion"; }

        if (FilterType == FilterType.AlwaysFalse) {
            return "Kein Zeilen-Anzeige";
        }

        if (FilterType == FilterType.RowKey) {
            return SearchValue.Count == 0 ? "Row-Key: ?" : "Row-Key: " + SearchValue[0];
        }

        if (Column is null) {
            return SearchValue.Count == 0 ? "Zeilen-Filter" : "Zeilen-Filter: " + SearchValue[0];
        }
        var nam = Column.ReadableText();

        if (SearchValue.Count > 1) {
            switch (FilterType) {
                case FilterType.Istgleich or FilterType.IstGleich_ODER or FilterType.Istgleich_GroßKleinEgal
                    or FilterType.Istgleich_ODER_GroßKleinEgal:
                    return $"{nam} - eins davon: '{string.Join("', '", SearchValue)}'";

                case FilterType.IstGleich_UND or FilterType.Istgleich_UND_GroßKleinEgal:
                    return $"{nam} - alle: '{string.Join("', '", SearchValue)}'";

                default:
                    return $"{nam}: Spezial-Filter";
            }
        }

        if (Column == Table?.Column.SysCorrect && FilterType.HasFlag(FilterType.Istgleich)) {
            if (SearchValue[0].FromPlusMinus()) { return "Fehlerfreie Zeilen"; }
            if (!SearchValue[0].FromPlusMinus()) { return "Fehlerhafte Zeilen"; }
        }

        switch (FilterType) {
            case FilterType.Istgleich:

            case FilterType.Istgleich_GroßKleinEgal:

            case FilterType.Istgleich_ODER_GroßKleinEgal:

            case FilterType.Istgleich_UND_GroßKleinEgal:
                if (string.IsNullOrEmpty(SearchValue[0])) { return nam + " ist 'leer'"; }

                if (Column is null) { return "Unbekannter Filter"; }
                return nam + " = " + LanguageTool.PrepaireText(SearchValue[0], ShortenStyle.Replaced, string.Empty, string.Empty, Column.DoOpticalTranslation, null);

            case FilterType.Ungleich_MultiRowIgnorieren:

            case FilterType.Ungleich_MultiRowIgnorieren_UND_GroßKleinEgal:

            case FilterType.Ungleich_MultiRowIgnorieren_GroßKleinEgal:
                if (string.IsNullOrEmpty(SearchValue[0])) { return nam + " ist 'befüllt'"; }
                if (Column is null) { return "Unbekannter Filter"; }
                return nam + " ist nicht '" + LanguageTool.PrepaireText(SearchValue[0], ShortenStyle.Replaced, string.Empty, string.Empty, Column.DoOpticalTranslation, null) + "'";

            case FilterType.Istgleich_GroßKleinEgal_MultiRowIgnorieren:

            case FilterType.Istgleich_MultiRowIgnorieren:
                if (SearchValue.Count == 1 && string.IsNullOrEmpty(SearchValue[0])) { return nam + " muss leer sein"; }
                return "Der ganze Zelleninhalt von " + nam + " muss genau '" + SearchValue[0] + "' sein.";

            case FilterType.Instr:

            case FilterType.Instr_GroßKleinEgal:
                if (SearchValue.Count == 0 || string.IsNullOrEmpty(SearchValue[0])) { return "Filter aktuell ohne Funktion"; }
                return nam + " beinhaltet den Text '" + SearchValue[0] + "'";

            case FilterType.Between:

            case FilterType.Between | FilterType.UND:
                return nam + ": von " + SearchValue[0].Replace("|", " bis ");

            case FilterType.BeginntMit:
            case FilterType.BeginntMit_GroßKleinEgal:
                return nam + " beginnt mit '" + SearchValue[0] + "'";

            case FilterType.AlwaysFalse:
                return "Immer FALSCH";

            case FilterType.RowKey:
                return "Spezielle Zeile";

            default:
                return nam + ": Spezial-Filter";
        }
    }

    public QuickImage? SymbolForReadableText() => null;

    public override string ToString() => ParseableItems().FinishParseable();

    internal FilterItem Normalized() {
        var tb = Table ?? Column?.Table;

        var f = FilterType;

        if (SearchValue.Count < 2) {
            if (f.HasFlag(FilterType.ODER)) { f ^= FilterType.ODER; }
            if (f.HasFlag(FilterType.UND)) { f ^= FilterType.UND; }
        }

        return new FilterItem(tb, Column, f, SearchValue.SortedDistinctList().AsReadOnly(), "Normalize");
    }

    private void _table_Disposing(object? sender, System.EventArgs e) => Table = null;

    #endregion
}