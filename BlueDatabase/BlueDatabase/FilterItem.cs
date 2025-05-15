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
using BlueDatabase.Enums;
using BlueDatabase.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static BlueBasics.Constants;
using static BlueBasics.Converter;

namespace BlueDatabase;

public sealed class FilterItem : IReadableText, IParseable, ICanBeEmpty, IErrorCheckable, IHasDatabase {

    #region Fields

    private Database? _database;

    #endregion

    #region Constructors

    public FilterItem(Database? db, FilterType filterType, string searchValue) : this(db, filterType, [searchValue]) { }

    /// <summary>
    /// Ein AlwaysFalse Filter
    /// </summary>
    public FilterItem(Database? database, string origin) {
        Database = database;
        FilterType = FilterType.AlwaysFalse;
        Column = null;
        Origin = origin;
        SearchValue = new List<string>().AsReadOnly();
    }

    public FilterItem(string filterCode, Database? database) {
        // Database?, weil always False keine Datenbank braucht
        Database = database;
        Origin = string.Empty;

        SearchValue = new List<string>().AsReadOnly();

        this.Parse(filterCode);
    }

    public FilterItem(ColumnItem column, double from, double to) : this(column, FilterType.Between | FilterType.UND, from.ToStringFloat5() + "|" + to.ToStringFloat5()) { }

    /// <summary>
    /// Bei diesem Construktor muss der Tag 'Database' vorkommen!
    /// </summary>
    public FilterItem(ColumnItem column, FilterType filterType, string searchValue) : this(column, filterType, [searchValue], string.Empty) { }

    public FilterItem(ColumnItem column, FilterType filterType, string searchValue, string origin) : this(column, filterType, [searchValue], origin) { }

    public FilterItem(ColumnItem column, FilterType filterType, IList<string> searchValue) : this(column, filterType, searchValue, string.Empty) { }

    public FilterItem(ColumnItem column, FilterType filterType, IList<string>? searchValue, string origin) {
        Database = column.Database;
        Column = column;
        FilterType = filterType;
        Origin = origin;

        SearchValue = searchValue is { Count: > 0 } ? new ReadOnlyCollection<string>(searchValue) : EmptyReadOnly;
    }

    public FilterItem(Database? database, ColumnItem? column, FilterType filterType, IList<string>? searchValue, string origin) {
        Database = database ?? column?.Database;

        Column = column;

        FilterType = filterType;

        Origin = origin;

        // Setze SearchValue
        SearchValue = searchValue is { Count: > 0 } ? new ReadOnlyCollection<string>(searchValue) : EmptyReadOnly;
    }

    private FilterItem(Database? database, FilterType filterType, IList<string>? searchValue) {
        //Database?, weil AlwaysFalse keine Angaben braucht
        Database = database;

        FilterType = filterType;
        Origin = string.Empty;

        SearchValue = searchValue is { Count: > 0 } ? new ReadOnlyCollection<string>(searchValue) : new List<string>().AsReadOnly();
    }

    #endregion

    #region Properties

    public ColumnItem? Column { get; private set; }

    /// <summary>
    /// Der Edit-Dialog braucht die Datenbank, um mit Texten die Spalte zu suchen.
    /// </summary>
    public Database? Database {
        get => _database;
        private set {
            if (value?.IsDisposed ?? true) { value = null; }
            if (value == _database) { return; }

            if (_database != null) {
                _database.DisposingEvent -= _database_Disposing;
            }
            _database = value;

            if (_database != null) {
                _database.DisposingEvent += _database_Disposing;
            }
        }
    }

    public FilterType FilterType { get; private set; }

    public string Origin { get; private set; }

    public string QuickInfo => ReadableText();

    public ReadOnlyCollection<string> SearchValue { get; private set; }

    #endregion

    #region Methods

    public bool Equals(FilterItem? thisFilter) => thisFilter != null &&
                                                  thisFilter.FilterType == FilterType &&
                                                  thisFilter.Column == Column &&
                                                  thisFilter.Origin == Origin &&
                                                  thisFilter.SearchValue.JoinWithCr() == SearchValue.JoinWithCr();

    public string ErrorReason() {
        if (_database == null) { return "Keine Datenbank angegeben"; }

        if (_database.IsDisposed) { return "Datenbank verworfen"; }

        if (FilterType == FilterType.AlwaysFalse) { return string.Empty; }

        if (FilterType is FilterType.GroßKleinEgal or FilterType.UND or FilterType.ODER or FilterType.MultiRowIgnorieren) { return "Fehlerhafter Filter"; }

        if (Column != null && Column?.Database != Database) { return "Datenbanken inkonsistent"; }

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
            if (SearchValue.Count != 1) { return "Split-Spalte mit ungültiger Suche"; }
            if (FilterType is not FilterType.Istgleich
                and not FilterType.Istgleich_GroßKleinEgal
                and not FilterType.Istgleich_MultiRowIgnorieren
                and not FilterType.Istgleich_ODER_GroßKleinEgal
                and not FilterType.Istgleich_UND_GroßKleinEgal) { return "Falscher Typ"; }
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
            result.ParseableAdd("Database", Database);
            result.ParseableAdd("ColumnName", Column);
            result.ParseableAdd("Values", SearchValue, false);
            result.ParseableAdd("Origin", Origin);
            return result;
        } catch {
            Develop.CheckStackOverflow();
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
                Database = Database.Get(value.FromNonCritical(), false, null);
                return true;

            case "type":
                FilterType = (FilterType)IntParse(value);
                return true;

            case "columnname":
            case "column":
                Column = Database?.Column[value];
                return true;

            case "columnkey":
                //_column = Database.Column.SearchByKey(LongParse(pair.Value));
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

        if (Column == Database?.Column.SysCorrect && FilterType.HasFlag(FilterType.Istgleich)) {
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
        var db = _database ?? Column?.Database;

        var f = FilterType;

        if (SearchValue.Count < 2) {
            if (f.HasFlag(FilterType.ODER)) { f ^= FilterType.ODER; }
            if (f.HasFlag(FilterType.UND)) { f ^= FilterType.UND; }
        }

        return new FilterItem(db, Column, f, SearchValue.SortedDistinctList().AsReadOnly(), "Normalize");
    }

    private void _database_Disposing(object sender, System.EventArgs e) => Database = null;

    #endregion
}