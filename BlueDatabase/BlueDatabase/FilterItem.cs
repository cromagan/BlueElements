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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using BlueDatabase.Interfaces;
using static BlueBasics.Converter;

namespace BlueDatabase;

public sealed class FilterItem : IReadableTextWithPropertyChangingAndKey, IParseable, ICanBeEmpty, IErrorCheckable, IHasDatabase, ICloneable, IEditable {

    #region Fields

    private ColumnItem? _column;

    private Database? _database;

    private FilterType _filterType = FilterType.AlwaysFalse;

    private string _origin = string.Empty;

    private object? _parent;
    private ReadOnlyCollection<string> _searchValue = new List<string>().AsReadOnly();

    #endregion

    #region Constructors

    public FilterItem(Database? db, FilterType filterType, string searchValue) : this(db, filterType, [searchValue]) { }

    /// <summary>
    /// Ein AlwaysFalse Filter
    /// </summary>
    public FilterItem(Database? database, string origin) {
        Database = database;
        _filterType = FilterType.AlwaysFalse;
        _column = null;
        KeyName = string.Empty;
        _origin = origin;
        SearchValue = new List<string>().AsReadOnly();
    }

    public FilterItem(string filterCode, Database? db) {
        // Database?, weil always False keine Datenbank braucht
        Database = db;

        KeyName = Generic.GetUniqueKey();

        SearchValue = new List<string>().AsReadOnly();

        this.Parse(filterCode);
    }

    public FilterItem(ColumnItem column, double from, double to) : this(column, FilterType.Between | FilterType.UND, from.ToStringFloat5() + "|" + to.ToStringFloat5()) { }

    /// <summary>
    /// Bei diesem Construktor muss der Tag 'Database' vorkommen!
    /// </summary>
    public FilterItem(ColumnItem column, FilterType filterType, string searchValue) : this(column, filterType,
        [searchValue], string.Empty) { }

    public FilterItem(ColumnItem column, FilterType filterType, string searchValue, string origin) : this(column, filterType,
        [searchValue], origin) { }

    public FilterItem(ColumnItem column, FilterType filterType, IList<string> searchValue) : this(column, filterType, searchValue, string.Empty) { }

    public FilterItem(ColumnItem column, FilterType filterType, IList<string>? searchValue, string origin) {
        KeyName = Generic.GetUniqueKey();
        Database = column.Database;
        _column = column;
        _filterType = filterType;
        _origin = origin;

        if (searchValue is { Count: > 0 }) {
            SearchValue = new ReadOnlyCollection<string>(searchValue);
        } else {
            SearchValue = new List<string>().AsReadOnly();
        }
    }

    /// <summary>
    /// Erstellt einen Filter, der den Zeilenschhl�ssel sucht.
    /// </summary>
    /// <param name="row"></param>
    public FilterItem(RowItem row) : this(row.Database, FilterType.RowKey, row.KeyName) { }

    private FilterItem(Database? db, FilterType filterType, IList<string>? searchValue) {
        //Database?, weil AlwaysFalse keine Angaben braucht
        Database = db;
        KeyName = Generic.GetUniqueKey();

        _filterType = filterType;
        if (searchValue is { Count: > 0 }) {
            SearchValue = new ReadOnlyCollection<string>(searchValue);
        } else {
            SearchValue = new List<string>().AsReadOnly();
        }
    }

    #endregion

    #region Events

    public event EventHandler? PropertyChanged;

    public event EventHandler? PropertyChanging;

    #endregion

    #region Properties

    public string CaptionForEditor => "Filter";

    public ColumnItem? Column {
        get => _column;
        set {
            if (value == _column) { return; }
            OnChanging();
            _column = value;
            OnPropertyChanged();
        }
    }

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
            OnPropertyChanged();

            if (_database != null) {
                _database.DisposingEvent += _database_Disposing;
            }
        }
    }

    public Type? Editor { get; set; }

    public FilterType FilterType {
        get => _filterType;
        set {
            if (value == _filterType) { return; }
            OnChanging();
            _filterType = value;
            OnPropertyChanged();
        }
    }

    public string KeyName { get; private set; }

    public string Origin {
        get => _origin;
        set {
            if (value == _origin) { return; }
            OnChanging();
            _origin = value;
            OnPropertyChanged();
        }
    }

    public object? Parent {
        get => _parent;
        set {
            if (_parent == value) { return; }
            _parent = value;
            OnPropertyChanged();
        }
    }

    public string QuickInfo => ReadableText();

    public ReadOnlyCollection<string> SearchValue {
        get => _searchValue;
        set {
            if (!value.IsDifferentTo(_searchValue)) { return; }
            OnChanging();
            _searchValue = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Methods

    public void Changeto(FilterType type, IEnumerable<string> searchvalue) {
        var svl = searchvalue.ToList();
        if (type == _filterType && !svl.IsDifferentTo(_searchValue)) { return; }
        OnChanging();
        _filterType = type;
        _searchValue = svl.AsReadOnly();
        OnPropertyChanged();
    }

    public void Changeto(FilterType type, string searchvalue) => Changeto(type, (List<string>)[searchvalue]);

    public object Clone() {
        var fi = new FilterItem(Database, _filterType, _searchValue);
        fi.Column = _column;
        fi.Origin = _origin;
        fi.KeyName = KeyName;
        fi.Database = Database;
        return fi;
    }

    public bool Equals(FilterItem? thisFilter) => thisFilter != null &&
                                                  thisFilter.FilterType == _filterType &&
                                                  thisFilter.Column == _column &&
                                                  thisFilter._origin == _origin &&
                                                  thisFilter.SearchValue.JoinWithCr() == _searchValue.JoinWithCr();

    public string ErrorReason() {
        if (Database == null) { return "Keine Datenbank angegeben"; }

        if (Database.IsDisposed) { return "Datenbank verworfen"; }
        //if (IsDisposed) { return "Filter verworfen"; }

        //if (_filterType == FilterType.KeinFilter) { return "'Kein Filter' angegeben"; }

        if (_filterType == FilterType.AlwaysFalse) { return string.Empty; }

        if (_filterType is FilterType.Gro�KleinEgal or FilterType.UND or FilterType.ODER or FilterType.MultiRowIgnorieren) { return "Fehlerhafter Filter"; }

        if (_column != null && _column?.Database != Database) { return "Datenbanken inkonsistent"; }

        if (SearchValue.Count == 0) { return "Kein Suchtext vorhanden"; }

        if (_filterType == FilterType.RowKey) {
            if (_column != null) { return "RowKey suche mit Spaltenangabe"; }
            if (SearchValue.Count != 1) { return "RowKey mit ung�ltiger Suche"; }
            return string.Empty;
        }

        if (_column == null && !_filterType.HasFlag(FilterType.Instr)) { return "Fehlerhafter Zeilenfilter"; }

        if (_filterType.HasFlag(FilterType.Instr)) {
            foreach (var thisV in SearchValue) {
                if (string.IsNullOrEmpty(thisV)) { return "Instr-Filter ohne Suchtext"; }
            }
        }

        if (_column != null && _column == _column.Database?.Column.SplitColumn) {
            if (SearchValue.Count != 1) { return "Split-Spalte mit ung�ltiger Suche"; }
            if (_filterType is not FilterType.Istgleich
                and not FilterType.Istgleich_Gro�KleinEgal
                and not FilterType.Istgleich_MultiRowIgnorieren
                and not FilterType.Istgleich_ODER_Gro�KleinEgal
                and not FilterType.Istgleich_UND_Gro�KleinEgal) { return "Falscher Typ"; }
        }

        return string.Empty;
    }

    public bool IsNullOrEmpty() => !this.IsOk();

    public void Normalize() {
        _searchValue = _searchValue.SortedDistinctList().AsReadOnly();

        KeyName = "Normalized";

        _origin = "Normalize";

        if (_column != null) { _database = _column.Database; }

        if (_searchValue.Count < 2) {
            if (_filterType.HasFlag(FilterType.ODER)) { _filterType ^= FilterType.ODER; }
            if (_filterType.HasFlag(FilterType.UND)) { _filterType ^= FilterType.UND; }
        }
    }

    public void OnChanging() {
        PropertyChanging?.Invoke(this, System.EventArgs.Empty);
    }

    public void OnPropertyChanged() {
        PropertyChanged?.Invoke(this, System.EventArgs.Empty);
    }

    public List<string> ParseableItems() {
        try {
            // F�r FlexiForFilter werden auch "ung�ltige" Filter ben�tigt
            // z.B. Instr ohn Text
            //if (!this.IsOk()) { return string.Empty; }

            List<string> result = [];
            result.ParseableAdd("ID", KeyName);
            result.ParseableAdd("Type", _filterType);
            result.ParseableAdd("Database", Database);
            result.ParseableAdd("ColumnName", _column);
            result.ParseableAdd("Values", _searchValue, false);
            result.ParseableAdd("Origin", _origin);
            return result;
        } catch {
            Develop.CheckStackForOverflow();
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
                    Develop.DebugPrint(FehlerArt.Fehler, "Identifier fehlerhaft: " + value);
                }
                return true;

            case "database":
                Database = Database.Get(value.FromNonCritical(), false, null);
                return true;

            case "type":
                _filterType = (FilterType)IntParse(value);
                return true;

            case "columnname":
            case "column":
                _column = Database?.Column[value];
                return true;

            case "columnkey":
                //_column = Database.Column.SearchByKey(LongParse(pair.Value));
                return true;

            case "values":

                if (string.IsNullOrEmpty(value)) {
                    _searchValue = new List<string> { string.Empty }.AsReadOnly();
                } else {
                    _searchValue = value.SplitBy("|").ToList().FromNonCritical().AsReadOnly();
                }

                return true;

            case "origin":
            case "herkunft":
                _origin = value.FromNonCritical();
                return true;

            case "id":
                KeyName = value.FromNonCritical();
                return true;
        }

        return false;
    }

    public string ReadableText() {
        // Bei Nich OK sch�n en Text zur�ck geben f�r FlexiControlForFilter
        if (!this.IsOk()) { return "Filter ohne Funktion"; }

        if (_filterType == FilterType.AlwaysFalse) {
            return "Immer FALSCH";
        }

        if (_column == null) {
            if (SearchValue.Count == 0) {
                return "Zeilen-Filter";
            }

            return "Zeilen-Filter: " + SearchValue[0];
        }
        var nam = _column.ReadableText();

        if (SearchValue.Count > 1) {
            switch (_filterType) {
                case FilterType.Istgleich or FilterType.IstGleich_ODER or FilterType.Istgleich_Gro�KleinEgal
                    or FilterType.Istgleich_ODER_Gro�KleinEgal:
                    return nam + " - eins davon: '" + SearchValue.JoinWith("', '") + "'";

                case FilterType.IstGleich_UND or FilterType.Istgleich_UND_Gro�KleinEgal:
                    return nam + " - alle: '" + SearchValue.JoinWith("', '") + "'";

                default:
                    return nam + ": Spezial-Filter";
            }
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

                if (Column == null) { return "Unbekannter Filter"; }
                return nam + " = " + LanguageTool.PrepaireText(SearchValue[0], ShortenStyle.Replaced, string.Empty, string.Empty, Column.DoOpticalTranslation, null);

            case FilterType.Ungleich_MultiRowIgnorieren:

            case FilterType.Ungleich_MultiRowIgnorieren_UND_Gro�KleinEgal:

            case FilterType.Ungleich_MultiRowIgnorieren_Gro�KleinEgal:
                if (string.IsNullOrEmpty(SearchValue[0])) { return nam + " muss bef�llt sein"; }
                if (Column == null) { return "Unbekannter Filter"; }
                return nam + " ist nicht '" + LanguageTool.PrepaireText(SearchValue[0], ShortenStyle.Replaced, string.Empty, string.Empty, Column.DoOpticalTranslation, null) + "'";

            case FilterType.Istgleich_Gro�KleinEgal_MultiRowIgnorieren:

            case FilterType.Istgleich_MultiRowIgnorieren:
                if (SearchValue.Count == 1 && string.IsNullOrEmpty(SearchValue[0])) { return nam + " muss leer sein"; }
                return "Der ganze Zelleninhalt von " + nam + " muss genau '" + SearchValue[0] + "' sein.";

            case FilterType.Instr:

            case FilterType.Instr_Gro�KleinEgal:
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

    private void _database_Disposing(object sender, System.EventArgs e) => Database = null;

    #endregion
}