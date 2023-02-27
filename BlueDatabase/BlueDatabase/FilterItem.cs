// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using BlueDatabase.Interfaces;
using static BlueBasics.Converter;

namespace BlueDatabase;

public sealed class FilterItem : IReadableTextWithChangingAndKey, IParseable, IReadableTextWithChanging, ICanBeEmpty, IErrorCheckable, IHasDatabase, IHasKeyName, IDisposableExtended {

    #region Fields

    public string Herkunft = string.Empty;

    private ColumnItem? _column;

    private FilterType _filterType = FilterType.KeinFilter;

    #endregion

    #region Constructors

    public FilterItem(DatabaseAbstract database, FilterType filterType, string searchValue) : this(database, filterType, new List<string> { searchValue }) { }

    public FilterItem(DatabaseAbstract database, FilterType filterType, IList<string>? searchValue) {
        Database = database;
        KeyName = Generic.UniqueInternal();
        if (Database != null && !Database.IsDisposed) {
            Database.Disposing += Database_Disposing;
        }

        _filterType = filterType;
        if (searchValue != null && searchValue.Count > 0) {
            SearchValue = new ReadOnlyCollection<string>(searchValue);
        } else {
            SearchValue = new ReadOnlyCollection<string>(new List<string>());
        }

        _column?.RefreshColumnsData();
    }

    public FilterItem(DatabaseAbstract database, string filterCode) {
        Database = database;
        if (Database != null && !Database.IsDisposed) {
            Database.Disposing += Database_Disposing;
        }
        KeyName = Generic.UniqueInternal();

        SearchValue = new ReadOnlyCollection<string>(new List<string>());

        Parse(filterCode);

        _column?.RefreshColumnsData();
    }

    /// <summary>
    /// Bei diesem Construktor mus der Tag database vorkommen!
    /// </summary>
    /// <param name="filterCode"></param>

    public FilterItem(string filterCode) {
        KeyName = Generic.UniqueInternal();
        SearchValue = new ReadOnlyCollection<string>(new List<string>());
        Parse(filterCode);
        _column?.RefreshColumnsData();
    }

    public FilterItem(ColumnItem column, FilterType filterType, string searchValue) : this(column, filterType, new List<string> { searchValue }, string.Empty) { }

    public FilterItem(ColumnItem column, FilterType filterType, string searchValue, string tag) : this(column, filterType, new List<string> { searchValue }, tag) { }

    public FilterItem(ColumnItem column, FilterType filterType, List<string> searchValue) : this(column, filterType, searchValue, string.Empty) { }

    public FilterItem(ColumnItem column, FilterType filterType, IList<string>? searchValue, string herkunft) {
        KeyName = Generic.UniqueInternal();
        Database = column.Database;
        _column = column;
        _filterType = filterType;
        Herkunft = herkunft;

        _column?.RefreshColumnsData();

        if (searchValue != null && searchValue.Count > 0) {
            SearchValue = new ReadOnlyCollection<string>(searchValue);
        } else {
            SearchValue = new ReadOnlyCollection<string>(new List<string>());
        }
    }

    public FilterItem(ColumnItem column, RowItem rowWithValue) : this(column, FilterType.Istgleich_Gro�KleinEgal_MultiRowIgnorieren, rowWithValue.CellGetString(column)) { }

    #endregion

    #region Events

    public event EventHandler? Changed;

    #endregion

    #region Properties

    public ColumnItem? Column {
        get => _column;
        set {
            if (value == _column) { return; }
            _column = value;
            OnChanged();
        }
    }

    /// <summary>
    /// Der Edit-Dialog braucht die Datenbank, um mit Texten die Spalte zu suchen.
    /// </summary>
    public DatabaseAbstract? Database { get; private set; }

    public FilterType FilterType {
        get => _filterType;
        set {
            if (value == _filterType) { return; }
            _filterType = value;
            OnChanged();
        }
    }

    public bool IsDisposed { get; private set; }
    public string KeyName { get; private set; }
    public ReadOnlyCollection<string> SearchValue { get; private set; }

    #endregion

    #region Methods

    public void Changeto(FilterType type, IEnumerable<string> searchvalue) {
        var l = new List<string>();
        l.AddRange(searchvalue);

        SearchValue = new ReadOnlyCollection<string>(l.SortedDistinctList());

        _filterType = type;
        OnChanged();
    }

    public void Changeto(FilterType type, string searchvalue) {
        SearchValue = new ReadOnlyCollection<string>(new List<string> { searchvalue });

        _filterType = type;
        OnChanged();
    }

    public void Dispose() {
        IsDisposed = true;

        Column = null;
        if (Database != null && !Database.IsDisposed) {
            if (Database != null && !Database.IsDisposed) { Database.Disposing -= Database_Disposing; }
            Database = null;
        }
    }

    public string ErrorReason() {
        if (_filterType == FilterType.KeinFilter) { return "'Kein Filter' angegeben"; }

        if (_filterType.HasFlag(FilterType.Instr)) {
            if (SearchValue == null || SearchValue.Count == 0) { return "Instr-Filter ohne Suchtext"; }
            foreach (var thisV in SearchValue) {
                if (string.IsNullOrEmpty(thisV)) { return "Instr-Filter ohne Suchtext"; }
            }
        }

        if (_column == null && Database == null) { return "Weder Spalte noch Datenbank angegeben"; }

        if (Database != null && Database.IsDisposed) { return "Datenbank verworfen"; }

        if (Column == null && !_filterType.HasFlag(FilterType.Instr)) { return "Fehlerhafter Zeilenfilter"; }

        return string.Empty;
    }

    public bool IsNullOrEmpty() => _filterType == FilterType.KeinFilter;

    public bool IsOk() => string.IsNullOrEmpty(ErrorReason());

    public void OnChanged() => Changed?.Invoke(this, System.EventArgs.Empty);

    public void Parse(string toParse) {
        foreach (var pair in toParse.GetAllTags()) {
            switch (pair.Key) {
                case "identifier":
                    if (pair.Value != "Filter") {
                        Develop.DebugPrint(FehlerArt.Fehler, "Identifier fehlerhaft: " + pair.Value);
                    }
                    break;

                case "database":
                    if (Database != null && !Database.IsDisposed) { Database.Disposing -= Database_Disposing; }
                    Database = DatabaseAbstract.GetById(new ConnectionInfo(pair.Value.FromNonCritical(), null), null);

                    if (Database != null && !Database.IsDisposed) { Database.Disposing += Database_Disposing; }

                    break;

                case "type":
                    _filterType = (FilterType)IntParse(pair.Value);
                    break;

                case "columnname":
                case "column":
                    _column = Database?.Column.Exists(pair.Value);
                    break;

                case "columnkey":
                    //_column = Database.Column.SearchByKey(LongParse(pair.Value));
                    break;

                case "value":
                    var l = new List<string>();
                    if (SearchValue != null) {
                        l.AddRange(SearchValue);
                    }

                    l.Add(pair.Value.FromNonCritical());
                    SearchValue = new ReadOnlyCollection<string>(l);
                    break;

                case "herkunft":
                    Herkunft = pair.Value.FromNonCritical();
                    break;

                case "id":
                    KeyName = pair.Value.FromNonCritical();
                    break;

                default:
                    Develop.DebugPrint(FehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                    break;
            }
        }
        if (toParse.Contains(", Value=}") || toParse.Contains(", Value=,")) { _ = SearchValue.AddIfNotExists(""); }
    }

    public string ReadableText() {
        if (_filterType == FilterType.KeinFilter) { return "Filter ohne Funktion"; }
        if (_column == null) { return "Zeilen-Filter"; }
        var nam = _column.ReadableText();
        if (SearchValue == null || SearchValue.Count < 1) { return "#### Filter-Fehler ####"; }
        if (SearchValue.Count > 1) {
            return _filterType switch {
                FilterType.Istgleich or FilterType.IstGleich_ODER or FilterType.Istgleich_Gro�KleinEgal or FilterType.Istgleich_ODER_Gro�KleinEgal => nam + " - eins davon: '" + SearchValue.JoinWith("', '") + "'",
                FilterType.IstGleich_UND or FilterType.Istgleich_UND_Gro�KleinEgal => nam + " - alle: '" + SearchValue.JoinWith("', '") + "'",
                _ => nam + ": Spezial-Filter"
            };
        }
        if (_column == Database?.Column.SysCorrect && _filterType.HasFlag(FilterType.Istgleich)) {
            if (SearchValue[0].FromPlusMinus()) { return "Fehlerfreie Zeilen"; }
            if (!SearchValue[0].FromPlusMinus()) { return "Fehlerhafte Zeilen"; }
        }
        switch (_filterType) {
            case FilterType.Istgleich:

            case FilterType.Istgleich_Gro�KleinEgal:

            case FilterType.Istgleich_ODER_Gro�KleinEgal:

            case FilterType.Istgleich_UND_Gro�KleinEgal:
                if (string.IsNullOrEmpty(SearchValue[0])) { return nam + " muss leer sein"; }
                return nam + " = " + LanguageTool.ColumnReplace(SearchValue[0], Column, ShortenStyle.Replaced);

            case FilterType.Ungleich_MultiRowIgnorieren:

            case FilterType.Ungleich_MultiRowIgnorieren_UND_Gro�KleinEgal:

            case FilterType.Ungleich_MultiRowIgnorieren_Gro�KleinEgal:
                if (string.IsNullOrEmpty(SearchValue[0])) { return nam + " muss bef�llt sein"; }
                return nam + " <> " + LanguageTool.ColumnReplace(SearchValue[0], Column, ShortenStyle.Replaced);

            case FilterType.Istgleich_Gro�KleinEgal_MultiRowIgnorieren:

            case FilterType.Istgleich_MultiRowIgnorieren:
                if (SearchValue.Count == 1 && string.IsNullOrEmpty(SearchValue[0])) { return nam + " muss leer sein"; }
                return "Spezial-Filter";

            case FilterType.Instr:

            case FilterType.Instr_Gro�KleinEgal:
                if (SearchValue.Count == 0 || string.IsNullOrEmpty(SearchValue[0])) { return "Filter aktuell ohne Funktion"; }
                return nam + " beinhaltet den Text '" + SearchValue[0] + "'";

            case FilterType.Between:

            case FilterType.Between | FilterType.UND:
                return nam + ": von " + SearchValue[0].Replace("|", " bis ");

            default:
                return nam + ": Spezial-Filter";
        }
    }

    public QuickImage? SymbolForReadableText() => null;

    public override string ToString() {
        if (IsDisposed) { return string.Empty; }

        try {
            if (!IsOk()) { return string.Empty; }

            var result = new List<string>();
            result.ParseableAdd("ID", KeyName);
            result.ParseableAdd("Type", _filterType);

            result.ParseableAdd("Database", Database);
            result.ParseableAdd("ColumnName", _column);
            foreach (var t in SearchValue) {
                result.ParseableAdd("Value", t);
            }
            result.ParseableAdd("Herkunft", Herkunft);
            return result.Parseable();
        } catch {
            return ToString();
        }
    }

    private void Database_Disposing(object sender, System.EventArgs e) => Dispose();

    #endregion
}