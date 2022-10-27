// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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
using System;
using System.Collections.Generic;
using static BlueBasics.Converter;

namespace BlueDatabase;

public sealed class FilterItem : IParseable, IReadableTextWithChanging, ICanBeEmpty, IDisposable {

    #region Fields

    public string Herkunft = string.Empty;

    private ColumnItem? _column;

    private bool _disposedValue;
    private FilterType _filterType = FilterType.KeinFilter;

    #endregion

    #region Constructors

    public FilterItem(DatabaseAbstract? database, FilterType filterType, string searchValue) : this(database, filterType, new List<string> { searchValue }) { }

    public FilterItem(DatabaseAbstract? database, FilterType filterType, IReadOnlyCollection<string>? searchValue) {
        Database = database;
        if (Database != null) {
            Database.Disposing += Database_Disposing;
        }

        _filterType = filterType;
        if (searchValue != null && searchValue.Count > 0) { SearchValue.AddRange(searchValue); }
        SearchValue.Changed += SearchValue_ListOrItemChanged;
    }

    public FilterItem(DatabaseAbstract database, string filterCode) {
        Database = database;
        if (Database != null) {
            Database.Disposing += Database_Disposing;
        }

        Parse(filterCode);
        SearchValue.Changed += SearchValue_ListOrItemChanged;
    }

    /// <summary>
    /// Bei diesem Construktor mus der Tag database vorkommen!
    /// </summary>
    /// <param name="filterCode"></param>
    public FilterItem(string filterCode) {
        Parse(filterCode);
        SearchValue.Changed += SearchValue_ListOrItemChanged;
    }

    public FilterItem(ColumnItem? column, FilterType filterType, string searchValue) : this(column, filterType, new List<string> { searchValue }, string.Empty) { }

    public FilterItem(ColumnItem? column, FilterType filterType, string searchValue, string tag) : this(column, filterType, new List<string> { searchValue }, tag) { }

    public FilterItem(ColumnItem? column, FilterType filterType, List<string> searchValue) : this(column, filterType, searchValue, string.Empty) { }

    public FilterItem(ColumnItem? column, FilterType filterType, IReadOnlyCollection<string>? searchValue, string herkunft) {
        if (column == null) {
            Develop.DebugPrint(FehlerArt.Fehler, "Spalte nicht vorhanden.");
            return;
        }
        Database = column.Database;
        _column = column;
        _filterType = filterType;
        Herkunft = herkunft;
        if (searchValue != null && searchValue.Count > 0) { SearchValue.AddRange(searchValue); }
        SearchValue.Changed += SearchValue_ListOrItemChanged;
    }

    public FilterItem(ColumnItem? column, RowItem rowWithValue) : this(column, FilterType.Istgleich_GroﬂKleinEgal_MultiRowIgnorieren, rowWithValue.CellGetString(column)) { }

    #endregion

    #region Events

    public event EventHandler Changed;

    #endregion

    #region Properties

    public ColumnItem? Column {
        get => _column;
        set {
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
            _filterType = value;
            OnChanged();
        }
    }

    public bool IsParsing { get; private set; }
    public ListExt<string> SearchValue { get; } = new();

    #endregion

    #region Methods

    public void Changeto(FilterType type, string searchvalue) {
        SearchValue.ThrowEvents = false;
        SearchValue.Clear();
        SearchValue.Add(searchvalue);
        _filterType = type;
        SearchValue.ThrowEvents = true;
        OnChanged();
    }

    //public object Clone() => new FilterItem(Database, ToString());

    //public string CompareKey() => ((int)_filterType).ToString(Constants.Format_Integer10);

    // // TODO: Finalizer nur ¸berschreiben, wenn "Dispose(bool disposing)" Code f¸r die Freigabe nicht verwalteter Ressourcen enth‰lt
    // ~FilterItem()
    // {
    //     // ƒndern Sie diesen Code nicht. F¸gen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
    //     Dispose(disposing: false);
    // }
    public void Dispose() {
        // ƒndern Sie diesen Code nicht. F¸gen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public string ErrorReason() => _filterType == FilterType.KeinFilter ? "'Kein Filter' angegeben" : string.Empty;

    public bool IsNullOrEmpty() => _filterType == FilterType.KeinFilter;

    public bool IsOk() => string.IsNullOrEmpty(ErrorReason());

    public void OnChanged() {
        if (IsParsing) { Develop.DebugPrint(FehlerArt.Warnung, "Falscher Parsing Zugriff!"); return; }
        Changed?.Invoke(this, System.EventArgs.Empty);
    }

    public void Parse(string toParse) {
        IsParsing = true;
        foreach (var pair in toParse.GetAllTags()) {
            switch (pair.Key) {
                case "identifier":
                    if (pair.Value != "Filter") {
                        Develop.DebugPrint(FehlerArt.Fehler, "Identifier fehlerhaft: " + pair.Value);
                    }
                    break;

                case "database":
                    if (Database != null) { Database.Disposing -= Database_Disposing; }
                    Database = DatabaseAbstract.GetByID(new ConnectionInfo( pair.Value.FromNonCritical()));
                    Database.Disposing += Database_Disposing;
                    break;

                case "type":
                    _filterType = (FilterType)IntParse(pair.Value);
                    break;

                case "column":
                    _column = Database.Column[pair.Value];
                    break;

                case "columnkey":
                    _column = Database.Column.SearchByKey(LongParse(pair.Value));
                    break;

                case "value":
                    SearchValue.AddIfNotExists(pair.Value.FromNonCritical());
                    break;

                case "herkunft":
                    Herkunft = pair.Value.FromNonCritical();
                    break;

                default:
                    Develop.DebugPrint(FehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                    break;
            }
        }
        if (toParse.Contains(", Value=}") || toParse.Contains(", Value=,")) { SearchValue.AddIfNotExists(""); }

        IsParsing = false;
    }

    public string ReadableText() {
        if (_filterType == FilterType.KeinFilter) { return "Filter ohne Funktion"; }
        if (_column == null) { return "Zeilen-Filter"; }
        var nam = _column.ReadableText();
        if (SearchValue == null || SearchValue.Count < 1) { return "#### Filter-Fehler ####"; }
        if (SearchValue.Count > 1) {
            return _filterType switch {
                FilterType.Istgleich or FilterType.IstGleich_ODER or FilterType.Istgleich_GroﬂKleinEgal or FilterType.Istgleich_ODER_GroﬂKleinEgal => nam + " - eins davon: '" + SearchValue.JoinWith("', '") + "'",
                FilterType.IstGleich_UND or FilterType.Istgleich_UND_GroﬂKleinEgal => nam + " - alle: '" + SearchValue.JoinWith("', '") + "'",
                _ => nam + ": Spezial-Filter"
            };
        }
        if (_column == Database.Column.SysCorrect && _filterType.HasFlag(FilterType.Istgleich)) {
            if (SearchValue[0].FromPlusMinus()) { return "Fehlerfreie Zeilen"; }
            if (!SearchValue[0].FromPlusMinus()) { return "Fehlerhafte Zeilen"; }
        }
        switch (_filterType) {
            case FilterType.Istgleich:

            case FilterType.Istgleich_GroﬂKleinEgal:

            case FilterType.Istgleich_ODER_GroﬂKleinEgal:

            case FilterType.Istgleich_UND_GroﬂKleinEgal:
                if (string.IsNullOrEmpty(SearchValue[0])) { return nam + " muss leer sein"; }
                return nam + " = " + LanguageTool.ColumnReplace(SearchValue[0], Column, ShortenStyle.Replaced);

            case FilterType.Ungleich_MultiRowIgnorieren:

            case FilterType.Ungleich_MultiRowIgnorieren_UND_GroﬂKleinEgal:

            case FilterType.Ungleich_MultiRowIgnorieren_GroﬂKleinEgal:
                if (string.IsNullOrEmpty(SearchValue[0])) { return nam + " muss bef¸llt sein"; }
                return nam + " <> " + LanguageTool.ColumnReplace(SearchValue[0], Column, ShortenStyle.Replaced);

            case FilterType.Istgleich_GroﬂKleinEgal_MultiRowIgnorieren:

            case FilterType.Istgleich_MultiRowIgnorieren:
                if (SearchValue.Count == 1 && string.IsNullOrEmpty(SearchValue[0])) { return nam + " muss leer sein"; }
                return "Spezial-Filter";

            case FilterType.Instr:

            case FilterType.Instr_GroﬂKleinEgal:
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

    public string ToString(bool withdatabaseTag) {
        try {
            if (!IsOk()) { return string.Empty; }
            var result = "{Type=" + (int)_filterType;

            if (Database != null && withdatabaseTag) { result = result + ", Database=" + Database.ConnectionData.DatabaseID; }

            if (_column != null) { result = result + ", " + _column.ParsableColumnKey(); }
            foreach (var t in SearchValue) {
                result = result + ", Value=" + t.ToNonCritical();
            }
            if (!string.IsNullOrEmpty(Herkunft)) { result = result + ", Herkunft=" + Herkunft.ToNonCritical(); }
            return result + "}";
        } catch {
            return ToString(withdatabaseTag);
        }
    }

    public override string ToString() => ToString(false);

    private void Database_Disposing(object sender, System.EventArgs e) => Dispose();

    private void Dispose(bool disposing) {
        if (!_disposedValue) {
            if (disposing) {
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
            }
            Column = null;
            if (Database != null) {
                Database.Disposing -= Database_Disposing;
                Database = null;
            }
            // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer ¸berschreiben
            // TODO: Groﬂe Felder auf NULL setzen
            _disposedValue = true;
        }
    }

    private void SearchValue_ListOrItemChanged(object sender, System.EventArgs e) => OnChanged();

    #endregion
}