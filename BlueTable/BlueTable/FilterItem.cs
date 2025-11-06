// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

#nullable enable

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueTable.Enums;
using BlueTable.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static BlueBasics.Constants;
using static BlueBasics.Converter;

namespace BlueTable;

public sealed class FilterItem : IReadableText, IParseable, ICanBeEmpty, IErrorCheckable, IHasTable {

    #region Fields

    private Table? _table;

    #endregion

    #region Constructors

    public FilterItem(Table? db, FilterType filterType, string searchValue) : this(db, filterType, [searchValue]) { }

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

    public FilterItem(ColumnItem column, double from, double to) : this(column, FilterType.Between | FilterType.UND, from.ToStringFloat5() + "|" + to.ToStringFloat5()) { }

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

    /// <summary>
    /// Erstellt ein FilterItem aus einer SQL-WHERE-Bedingung
    /// Beispiel: "Name = 'Mueller'" oder "Age BETWEEN 18 AND 65"
    /// </summary>
    /// <param name="table">Die Tabelle für die Spaltenreferenzen</param>
    /// <param name="whereCondition">Die SQL-WHERE-Bedingung</param>
    public FilterItem(Table? table, string whereCondition, string origin) {
        Table = table;
        SearchValue = new List<string>().AsReadOnly();
        Origin = origin;

        if (string.IsNullOrWhiteSpace(whereCondition) || table == null) {
            FilterType = FilterType.AlwaysFalse;
            Column = null;
            return;
        }

        try {
            ParseWhereCondition(whereCondition.Trim());
        } catch {
            FilterType = FilterType.AlwaysFalse;
            Column = null;
            SearchValue = new List<string>().AsReadOnly();
        }
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

    public ColumnItem? Column { get; private set; }

    public FilterType FilterType { get; private set; }

    public string Origin { get; private set; }

    public string QuickInfo => ReadableText();

    public ReadOnlyCollection<string> SearchValue { get; private set; }

    /// <summary>
    /// Der Edit-Dialog braucht die Tabelle, um mit Texten die Spalte zu suchen.
    /// </summary>
    public Table? Table {
        get => _table;
        private set {
            if (value?.IsDisposed ?? true) { value = null; }
            if (value == _table) { return; }

            if (_table != null) {
                _table.DisposingEvent -= _table_Disposing;
            }
            _table = value;

            if (_table != null) {
                _table.DisposingEvent += _table_Disposing;
            }
        }
    }

    public string Where {
        get {
            if (!this.IsOk()) { return string.Empty; }

            if (FilterType == FilterType.AlwaysFalse) { return "1 = 0"; }

            if (FilterType == FilterType.RowKey) {
                if (SearchValue.Count == 1 && !string.IsNullOrEmpty(SearchValue[0])) {
                    return $"RowID = '{EscapeSqlValue(SearchValue[0])}'";
                }
                return string.Empty;
            }

            if (Column == null) { return string.Empty; } // Row-Filter ist kein SQL-Filter

            var columnName = Column.KeyName;
            var caseSensitive = !FilterType.HasFlag(FilterType.GroßKleinEgal);

            switch (FilterType) {
                case FilterType.Istgleich:
                case FilterType.Istgleich_MultiRowIgnorieren:
                    if (SearchValue.Count == 1) {
                        if (string.IsNullOrEmpty(SearchValue[0])) {
                            return $"{columnName} IS NULL";
                        }
                        return $"{columnName} = '{EscapeSqlValue(SearchValue[0])}'";
                    }
                    break;

                case FilterType.Istgleich_GroßKleinEgal:
                case FilterType.Istgleich_GroßKleinEgal_MultiRowIgnorieren:
                    if (SearchValue.Count == 1) {
                        if (string.IsNullOrEmpty(SearchValue[0])) {
                            return $"{columnName} IS NULL";
                        }
                        return $"UPPER({columnName}) = UPPER('{EscapeSqlValue(SearchValue[0])}')";
                    }
                    break;

                case FilterType.IstGleich_ODER:
                    if (SearchValue.Count > 1) {
                        var values = SearchValue.Select(v => $"'{EscapeSqlValue(v)}'").ToList();
                        return $"{columnName} IN ({string.Join(", ", values)})";
                    } else if (SearchValue.Count == 1) {
                        if (string.IsNullOrEmpty(SearchValue[0])) {
                            return $"{columnName} IS NULL";
                        }
                        return $"{columnName} = '{EscapeSqlValue(SearchValue[0])}'";
                    }
                    break;

                case FilterType.Istgleich_ODER_GroßKleinEgal:
                    if (SearchValue.Count > 1) {
                        var values = SearchValue.Select(v => $"UPPER('{EscapeSqlValue(v)}')").ToList();
                        return $"UPPER({columnName}) IN ({string.Join(", ", values)})";
                    } else if (SearchValue.Count == 1) {
                        if (string.IsNullOrEmpty(SearchValue[0])) {
                            return $"{columnName} IS NULL";
                        }
                        return $"UPPER({columnName}) = UPPER('{EscapeSqlValue(SearchValue[0])}')";
                    }
                    break;

                case FilterType.IstGleich_UND:
                    if (SearchValue.Count > 1) {
                        var conditions = SearchValue.Select(v =>
                            string.IsNullOrEmpty(v) ? $"{columnName} IS NULL" : $"{columnName} = '{EscapeSqlValue(v)}'");
                        return $"({string.Join(" AND ", conditions)})";
                    } else if (SearchValue.Count == 1) {
                        if (string.IsNullOrEmpty(SearchValue[0])) {
                            return $"{columnName} IS NULL";
                        }
                        return $"{columnName} = '{EscapeSqlValue(SearchValue[0])}'";
                    }
                    break;

                case FilterType.Istgleich_UND_GroßKleinEgal:
                    if (SearchValue.Count > 1) {
                        var conditions = SearchValue.Select(v =>
                            string.IsNullOrEmpty(v) ? $"{columnName} IS NULL" : $"UPPER({columnName}) = UPPER('{EscapeSqlValue(v)}')");
                        return $"({string.Join(" AND ", conditions)})";
                    } else if (SearchValue.Count == 1) {
                        if (string.IsNullOrEmpty(SearchValue[0])) {
                            return $"{columnName} IS NULL";
                        }
                        return $"UPPER({columnName}) = UPPER('{EscapeSqlValue(SearchValue[0])}')";
                    }
                    break;

                case FilterType.Ungleich_MultiRowIgnorieren:
                    if (SearchValue.Count == 1) {
                        if (string.IsNullOrEmpty(SearchValue[0])) {
                            return $"{columnName} IS NOT NULL";
                        }
                        return $"{columnName} != '{EscapeSqlValue(SearchValue[0])}'";
                    }
                    break;

                case FilterType.Ungleich_MultiRowIgnorieren_GroßKleinEgal:
                case FilterType.Ungleich_MultiRowIgnorieren_UND_GroßKleinEgal:
                    if (SearchValue.Count == 1) {
                        if (string.IsNullOrEmpty(SearchValue[0])) {
                            return $"{columnName} IS NOT NULL";
                        }
                        return $"UPPER({columnName}) != UPPER('{EscapeSqlValue(SearchValue[0])}')";
                    }
                    break;

                case FilterType.Instr:
                    if (SearchValue.Count == 1 && !string.IsNullOrEmpty(SearchValue[0])) {
                        return $"{columnName} LIKE '%{EscapeSqlValue(SearchValue[0])}%'";
                    }
                    break;

                case FilterType.Instr_GroßKleinEgal:
                case FilterType.Instr_UND_GroßKleinEgal:
                    if (SearchValue.Count == 1 && !string.IsNullOrEmpty(SearchValue[0])) {
                        return $"UPPER({columnName}) LIKE UPPER('%{EscapeSqlValue(SearchValue[0])}%')";
                    }
                    break;

                case FilterType.BeginntMit:
                    if (SearchValue.Count == 1 && !string.IsNullOrEmpty(SearchValue[0])) {
                        return $"{columnName} LIKE '{EscapeSqlValue(SearchValue[0])}%'";
                    }
                    break;

                case FilterType.BeginntMit_GroßKleinEgal:
                    if (SearchValue.Count == 1 && !string.IsNullOrEmpty(SearchValue[0])) {
                        return $"UPPER({columnName}) LIKE UPPER('{EscapeSqlValue(SearchValue[0])}%')";
                    }
                    break;

                case FilterType.Between:
                case FilterType.Between | FilterType.UND:
                    if (SearchValue.Count == 1 && SearchValue[0].Contains("|")) {
                        var parts = SearchValue[0].Split('|');
                        if (parts.Length == 2) {
                            return $"{columnName} BETWEEN '{EscapeSqlValue(parts[0])}' AND '{EscapeSqlValue(parts[1])}'";
                        }
                    }
                    break;
            }

            return string.Empty;
        }
    }

    #endregion

    #region Methods

    public bool Equals(FilterItem? thisFilter) => thisFilter != null &&
                                                  thisFilter.FilterType == FilterType &&
                                                  thisFilter.Column == Column &&
                                                  thisFilter.Origin == Origin &&
                                                  thisFilter.SearchValue.JoinWithCr() == SearchValue.JoinWithCr();

    public string ErrorReason() {
        if (FilterType == FilterType.AlwaysFalse) { return string.Empty; }

        if (_table == null) { return "Keine Tabelle angegeben"; }

        if (_table.IsDisposed) { return "Tabelle verworfen"; }

        if (FilterType is FilterType.GroßKleinEgal or FilterType.UND or FilterType.ODER or FilterType.MultiRowIgnorieren) { return "Fehlerhafter Filter"; }

        if (Column != null && Column?.Table != Table) { return "Tabellen inkonsistent"; }

        if (SearchValue.Count == 0) { return "Kein Suchtext vorhanden"; }

        if (FilterType == FilterType.RowKey) {
            if (Column != null) { return "RowKey suche mit Spaltenangabe"; }
            if (SearchValue.Count != 1) { return "RowKey mit ungültiger Suche"; }
            return string.Empty;
        }

        if (Column == null && !FilterType.HasFlag(FilterType.Instr_GroßKleinEgal)) { return "Fehlerhafter Zeilenfilter"; }

        if (FilterType.HasFlag(FilterType.Instr)) {
            foreach (var thisV in SearchValue) {
                if (string.IsNullOrEmpty(thisV)) { return "Instr-Filter ohne Suchtext"; }
            }
        }

        if (Column?.Value_for_Chunk != null && Column.Value_for_Chunk != ChunkType.None) {
            if (SearchValue.Count == 0) { return "Split-Spalte mit ungültiger Suche"; }
            if (FilterType is not FilterType.Istgleich
                and not FilterType.Istgleich_GroßKleinEgal
                and not FilterType.Istgleich_MultiRowIgnorieren
                and not FilterType.Istgleich_ODER_GroßKleinEgal
                and not FilterType.Istgleich_UND_GroßKleinEgal) { return "Falscher Typ"; }
        }

        foreach (var thisV in SearchValue) {
            Develop.AbortAppIfStackOverflow();
            if (thisV.Contains("~")) { return $"Unaufgelöste Variable {thisV}"; }
        }

        return string.Empty;
    }

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
        if (parsed.Contains(", Value=}") || parsed.Contains(", Value=,")) { _ = SearchValue.AddIfNotExists(""); }
    }

    public bool ParseThis(string key, string value) {
        switch (key) {
            case "identifier":
                if (value != "Filter") {
                    Develop.DebugPrint(ErrorType.Error, "Identifier fehlerhaft: " + value);
                }
                return true;

            case "database":
            case "table":
                Table = Table.Get(value.FromNonCritical(), null, false);
                return true;

            case "type":
                FilterType = (FilterType)IntParse(value);
                return true;

            case "columnname":
            case "column":
                Column = Table?.Column[value];
                return true;

            case "columnkey":
                //_column = Table.Column.SearchByKey(LongParse(pair.Value));
                return true;

            case "values":

                SearchValue = string.IsNullOrEmpty(value)
                    ? new List<string> { string.Empty }.AsReadOnly()
                    : value.SplitBy("|").ToList().FromNonCritical().AsReadOnly();

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
            return "Immer FALSCH";
        }

        if (FilterType == FilterType.RowKey) {
            return SearchValue.Count == 0 ? "Row-Key: ?" : "Row-Key: " + SearchValue[0];
        }

        if (Column == null) {
            return SearchValue.Count == 0 ? "Zeilen-Filter" : "Zeilen-Filter: " + SearchValue[0];
        }
        var nam = Column.ReadableText();

        if (SearchValue.Count > 1) {
            switch (FilterType) {
                case FilterType.Istgleich or FilterType.IstGleich_ODER or FilterType.Istgleich_GroßKleinEgal
                    or FilterType.Istgleich_ODER_GroßKleinEgal:
                    return nam + " - eins davon: '" + SearchValue.JoinWith("', '") + "'";

                case FilterType.IstGleich_UND or FilterType.Istgleich_UND_GroßKleinEgal:
                    return nam + " - alle: '" + SearchValue.JoinWith("', '") + "'";

                default:
                    return nam + ": Spezial-Filter";
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
                if (string.IsNullOrEmpty(SearchValue[0])) { return nam + " muss leer sein"; }

                if (Column == null) { return "Unbekannter Filter"; }
                return nam + " = " + LanguageTool.PrepaireText(SearchValue[0], ShortenStyle.Replaced, string.Empty, string.Empty, Column.DoOpticalTranslation, null);

            case FilterType.Ungleich_MultiRowIgnorieren:

            case FilterType.Ungleich_MultiRowIgnorieren_UND_GroßKleinEgal:

            case FilterType.Ungleich_MultiRowIgnorieren_GroßKleinEgal:
                if (string.IsNullOrEmpty(SearchValue[0])) { return nam + " muss befüllt sein"; }
                if (Column == null) { return "Unbekannter Filter"; }
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
        var db = _table ?? Column?.Table;

        var f = FilterType;

        if (SearchValue.Count < 2) {
            if (f.HasFlag(FilterType.ODER)) { f ^= FilterType.ODER; }
            if (f.HasFlag(FilterType.UND)) { f ^= FilterType.UND; }
        }

        return new FilterItem(db, Column, f, SearchValue.SortedDistinctList().AsReadOnly(), "Normalize");
    }

    private static string EscapeSqlValue(string value) {
        if (string.IsNullOrEmpty(value)) { return value; }

        // Einfacher SQL-Escape: Verdopple einfache Anführungszeichen
        return value.Replace("'", "''");
    }

    /// <summary>
    /// Parst die Werte einer IN-Klausel
    /// </summary>
    private static List<string> ParseInValues(string valuesString) {
        var values = new List<string>();

        // Einfache Implementierung: Split bei Komma und entferne Anführungszeichen
        var parts = valuesString.Split(',');

        foreach (var part in parts) {
            var trimmed = part.Trim();
            if (trimmed.StartsWith("'") && trimmed.EndsWith("'") && trimmed.Length >= 2) {
                // Entferne Anführungszeichen
                values.Add(trimmed.Substring(1, trimmed.Length - 2));
            } else {
                // Ohne Anführungszeichen
                values.Add(trimmed);
            }
        }

        return values;
    }

    private void _table_Disposing(object sender, System.EventArgs e) => Table = null;

    /// <summary>
    /// Parst eine einzelne SQL-WHERE-Bedingung und setzt die entsprechenden Properties
    /// </summary>
    private void ParseWhereCondition(string condition) {
        if (string.IsNullOrWhiteSpace(condition) || Table == null) {
            FilterType = FilterType.AlwaysFalse;
            return;
        }

        // IS NULL / IS NOT NULL
        if (condition.EndsWith(" IS NULL", System.StringComparison.OrdinalIgnoreCase)) {
            var columnName = condition.Substring(0, condition.Length - 8).Trim();
            Column = Table.Column[columnName];
            if (Column != null) {
                FilterType = FilterType.Istgleich;
                SearchValue = new List<string> { string.Empty }.AsReadOnly();
                return;
            }
        }

        if (condition.EndsWith(" IS NOT NULL", System.StringComparison.OrdinalIgnoreCase)) {
            var columnName = condition.Substring(0, condition.Length - 12).Trim();
            Column = Table.Column[columnName];
            if (Column != null) {
                FilterType = FilterType.Ungleich_MultiRowIgnorieren;
                SearchValue = new List<string> { string.Empty }.AsReadOnly();
                return;
            }
        }

        // BETWEEN
        var betweenMatch = System.Text.RegularExpressions.Regex.Match(condition,
            @"(\w+)\s+BETWEEN\s+'([^']+)'\s+AND\s+'([^']+)'",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (betweenMatch.Success) {
            var columnName = betweenMatch.Groups[1].Value;
            var value1 = betweenMatch.Groups[2].Value;
            var value2 = betweenMatch.Groups[3].Value;
            Column = Table.Column[columnName];
            if (Column != null) {
                FilterType = FilterType.Between | FilterType.UND;
                SearchValue = new List<string> { $"{value1}|{value2}" }.AsReadOnly();
                return;
            }
        }

        // LIKE
        var likeMatch = System.Text.RegularExpressions.Regex.Match(condition,
            @"(\w+)\s+LIKE\s+'([^']+)'",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (likeMatch.Success) {
            var columnName = likeMatch.Groups[1].Value;
            var value = likeMatch.Groups[2].Value;
            Column = Table.Column[columnName];

            if (Column != null) {
                // Bestimme FilterType basierend auf Wildcards
                if (value.StartsWith("%") && value.EndsWith("%")) {
                    // %value% -> INSTR
                    var searchValue = value.Substring(1, value.Length - 2);
                    FilterType = FilterType.Instr_GroßKleinEgal;
                    SearchValue = new List<string> { searchValue }.AsReadOnly();
                } else if (value.EndsWith("%")) {
                    // value% -> BeginntMit
                    var searchValue = value.Substring(0, value.Length - 1);
                    FilterType = FilterType.BeginntMit_GroßKleinEgal;
                    SearchValue = new List<string> { searchValue }.AsReadOnly();
                } else {
                    // Exakte Übereinstimmung
                    FilterType = FilterType.Istgleich_GroßKleinEgal;
                    SearchValue = new List<string> { value }.AsReadOnly();
                }
                return;
            }
        }

        // IN
        var inMatch = System.Text.RegularExpressions.Regex.Match(condition,
            @"(\w+)\s+IN\s*\(([^)]+)\)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (inMatch.Success) {
            var columnName = inMatch.Groups[1].Value;
            var valuesString = inMatch.Groups[2].Value;
            Column = Table.Column[columnName];

            if (Column != null) {
                // Parse die Werte in der IN-Klausel
                var values = ParseInValues(valuesString);
                if (values.Count > 0) {
                    FilterType = FilterType.Istgleich_ODER_GroßKleinEgal;
                    SearchValue = new ReadOnlyCollection<string>(values);
                    return;
                }
            }
        }

        // Einfache Gleichheit: column = 'value'
        var equalMatch = System.Text.RegularExpressions.Regex.Match(condition,
            @"(\w+)\s*=\s*'([^']*)'",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (equalMatch.Success) {
            var columnName = equalMatch.Groups[1].Value;
            var value = equalMatch.Groups[2].Value;
            Column = Table.Column[columnName];
            if (Column != null) {
                FilterType = FilterType.Istgleich_GroßKleinEgal;
                SearchValue = new List<string> { value }.AsReadOnly();
                return;
            }
        }

        // Einfache Gleichheit ohne Anführungszeichen: column = value
        var equalMatchNoQuotes = System.Text.RegularExpressions.Regex.Match(condition,
            @"(\w+)\s*=\s*([^'\s]+)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (equalMatchNoQuotes.Success) {
            var columnName = equalMatchNoQuotes.Groups[1].Value;
            var value = equalMatchNoQuotes.Groups[2].Value;
            Column = Table.Column[columnName];
            if (Column != null) {
                FilterType = FilterType.Istgleich_GroßKleinEgal;
                SearchValue = new List<string> { value }.AsReadOnly();
                return;
            }
        }

        // Wenn nichts geparst werden konnte
        FilterType = FilterType.AlwaysFalse;
        Column = null;
    }

    #endregion
}