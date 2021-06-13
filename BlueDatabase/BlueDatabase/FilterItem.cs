#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2021 Christian Peter
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
#endregion
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;

namespace BlueDatabase {
    public sealed class FilterItem : IParseable, ICompareKey, IReadableTextWithChanging, ICanBeEmpty, IDisposable {

        #region  Variablen-Deklarationen 
        /// <summary>
        /// Der Edit-Dialog braucht die Datenbank, um mit Texten die Spalte zu suchen.
        /// </summary>
        public Database Database { get; private set; }
        private ColumnItem _Column;
        private enFilterType _FilterType = enFilterType.KeinFilter;
        public string Herkunft = string.Empty;
        private bool disposedValue;
        #endregion


        #region  Event-Deklarationen + Delegaten 
        public event EventHandler Changed;
        #endregion


        #region  Construktor + Initialize 
        public FilterItem(Database database, enFilterType filterType, string searchValue) : this(database, filterType, new List<string>() { searchValue }) { }
        public FilterItem(Database database, enFilterType filterType, List<string> searchValue) {
            Database = database;
            Database.Disposing += Database_Disposing;
            _FilterType = filterType;
            if (searchValue != null && searchValue.Count > 0) { SearchValue.AddRange(searchValue); }
            SearchValue.Changed += SearchValue_ListOrItemChanged;
        }
        public FilterItem(Database database, string FilterCode) {
            Database = database;
            Database.Disposing += Database_Disposing;
            Parse(FilterCode);
            SearchValue.Changed += SearchValue_ListOrItemChanged;
        }
        public FilterItem(ColumnItem column, enFilterType filterType, string searchValue) : this(column, filterType, new List<string>() { searchValue }, string.Empty) { }
        public FilterItem(ColumnItem column, enFilterType filterType, string searchValue, string tag) : this(column, filterType, new List<string>() { searchValue }, tag) { }
        public FilterItem(ColumnItem column, enFilterType filterType, List<string> searchValue) : this(column, filterType, searchValue, string.Empty) { }
        public FilterItem(ColumnItem column, enFilterType filterType, List<string> searchValue, string herkunft) {
            if (column == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Spalte nicht vorhanden."); }
            Database = column.Database;
            _Column = column;
            _FilterType = filterType;
            Herkunft = herkunft;
            if (searchValue != null && searchValue.Count > 0) { SearchValue.AddRange(searchValue); }
            SearchValue.Changed += SearchValue_ListOrItemChanged;
        }
        public FilterItem(ColumnItem column, RowItem rowWithValue) : this(column, enFilterType.Istgleich_GroßKleinEgal_MultiRowIgnorieren, rowWithValue.CellGetString(column)) { }
        #endregion


        #region  Properties 
        public bool IsParsing { get; private set; }
        public ColumnItem Column {
            get => _Column;
            set {
                _Column = value;
                OnChanged();
            }
        }
        public ListExt<string> SearchValue { get; private set; } = new ListExt<string>();
        public enFilterType FilterType {
            get => _FilterType;
            set {
                _FilterType = value;
                OnChanged();
            }
        }
        public void OnChanged() {
            if (IsParsing) { Develop.DebugPrint(enFehlerArt.Warnung, "Falscher Parsing Zugriff!"); return; }
            Changed?.Invoke(this, System.EventArgs.Empty);
        }
        #endregion

        public override string ToString() {
            if (!IsOk()) { return string.Empty; }
            var Result = "{Type=" + (int)_FilterType;
            if (_Column != null) { Result = Result + ", " + _Column.ParsableColumnKey(); }
            foreach (var t in SearchValue) {
                Result = Result + ", Value=" + t.ToNonCritical();
            }
            if (!string.IsNullOrEmpty(Herkunft)) { Result = Result + ", Herkunft=" + Herkunft.ToNonCritical(); }
            return Result + "}";
        }
        public void Parse(string ToParse) {
            IsParsing = true;
            foreach (var pair in ToParse.GetAllTags()) {
                switch (pair.Key) {

                    case "identifier":
                        if (pair.Value != "Filter") {
                            Develop.DebugPrint(enFehlerArt.Fehler, "Identifier fehlerhaft: " + pair.Value);
                        }
                        break;

                    case "type":
                        _FilterType = (enFilterType)int.Parse(pair.Value);
                        break;

                    case "column":
                        _Column = Database.Column[pair.Value];
                        break;

                    case "columnkey":
                        _Column = Database.Column.SearchByKey(int.Parse(pair.Value));
                        break;

                    case "value":
                        SearchValue.Add(pair.Value.FromNonCritical());
                        break;

                    case "herkunft":
                        Herkunft = pair.Value.FromNonCritical();
                        break;
                    default:
                        Develop.DebugPrint(enFehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                        break;
                }
            }
            if (ToParse.Contains(", Value=}") || ToParse.Contains(", Value=,")) { SearchValue.Add(""); }
            IsParsing = false;
        }
        public string CompareKey() => ((int)_FilterType).ToString(Constants.Format_Integer10);
        public string ReadableText() {
            if (_FilterType == enFilterType.KeinFilter) { return "Filter ohne Funktion"; }
            if (_Column == null) { return "Zeilen-Filter"; }
            var nam = _Column.ReadableText();
            if (SearchValue == null || SearchValue.Count < 1) { return "#### Filter-Fehler ####"; }
            if (SearchValue.Count > 1) {
                return _FilterType switch {
                    enFilterType.Istgleich or enFilterType.IstGleich_ODER or enFilterType.Istgleich_GroßKleinEgal or enFilterType.Istgleich_ODER_GroßKleinEgal => nam + " - eins davon: '" + SearchValue.JoinWith("', '") + "'",
                    enFilterType.IstGleich_UND or enFilterType.Istgleich_UND_GroßKleinEgal => nam + " - alle: '" + SearchValue.JoinWith("', '") + "'",
                    _ => nam + ": Spezial-Filter",
                };
            }
            if (_Column == Database.Column.SysCorrect && _FilterType.HasFlag(enFilterType.Istgleich)) {
                if (SearchValue[0].FromPlusMinus()) { return "Fehlerfreie Zeilen"; }
                if (!SearchValue[0].FromPlusMinus()) { return "Fehlerhafte Zeilen"; }
            }
            switch (_FilterType) {

                case enFilterType.Istgleich:

                case enFilterType.Istgleich_GroßKleinEgal:

                case enFilterType.Istgleich_ODER_GroßKleinEgal:

                case enFilterType.Istgleich_UND_GroßKleinEgal:
                    if (string.IsNullOrEmpty(SearchValue[0])) { return nam + " muss leer sein"; }
                    return nam + " = " + LanguageTool.ColumnReplace(SearchValue[0], Column, enShortenStyle.Replaced);

                case enFilterType.Ungleich_MultiRowIgnorieren:

                case enFilterType.Ungleich_MultiRowIgnorieren_UND_GroßKleinEgal:

                case enFilterType.Ungleich_MultiRowIgnorieren_GroßKleinEgal:
                    if (string.IsNullOrEmpty(SearchValue[0])) { return nam + " muss befüllt sein"; }
                    return nam + " <> " + LanguageTool.ColumnReplace(SearchValue[0], Column, enShortenStyle.Replaced);

                case enFilterType.Istgleich_GroßKleinEgal_MultiRowIgnorieren:

                case enFilterType.Istgleich_MultiRowIgnorieren:
                    if (SearchValue.Count == 1 && string.IsNullOrEmpty(SearchValue[0])) { return nam + " muss leer sein"; }
                    return "Spezial-Filter";

                case enFilterType.Instr:

                case enFilterType.Instr_GroßKleinEgal:
                    if (SearchValue.Count == 0 || string.IsNullOrEmpty(SearchValue[0])) { return "Filter aktuell ohne Funktion"; }
                    return nam + " beinhaltet den Text '" + SearchValue[0] + "'";

                case enFilterType.Between:

                case enFilterType.Between | enFilterType.UND:
                    return nam + ": von " + SearchValue[0].Replace("|", " bis ");
                default:
                    return nam + ": Spezial-Filter";
            }
        }
        public QuickImage SymbolForReadableText() => null;
        public bool IsNullOrEmpty() => _FilterType == enFilterType.KeinFilter;
        public object Clone() => new FilterItem(Database, ToString());
        public bool IsOk() => string.IsNullOrEmpty(ErrorReason());
        public string ErrorReason() => _FilterType == enFilterType.KeinFilter ? "'Kein Filter' angegeben" : string.Empty;
        private void SearchValue_ListOrItemChanged(object sender, System.EventArgs e) => OnChanged();
        public void Changeto(enFilterType type, string searchvalue) {
            SearchValue.ThrowEvents = false;
            SearchValue.Clear();
            SearchValue.Add(searchvalue);
            _FilterType = type;
            SearchValue.ThrowEvents = true;
            OnChanged();
        }
        private void Database_Disposing(object sender, System.EventArgs e) => Dispose();
        private void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
                }
                Column = null;
                if (Database != null) {
                    Database.Disposing -= Database_Disposing;
                    Database = null;
                }
                // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
                // TODO: Große Felder auf NULL setzen
                disposedValue = true;
            }
        }
        // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
        // ~FilterItem()
        // {
        //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        //     Dispose(disposing: false);
        // }
        public void Dispose() {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}