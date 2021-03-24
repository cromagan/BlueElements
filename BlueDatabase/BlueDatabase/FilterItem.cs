#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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
    public sealed class FilterItem : IParseable, ICompareKey, IReadableTextWithChanging, ICanBeEmpty {

        #region  Variablen-Deklarationen 


        /// <summary>
        /// Der Edit-Dialog braucht die Datenbank, um mit Texten die Spalte zu suchen.
        /// </summary>
        public readonly Database Database;

        private ColumnItem _Column;
        private enFilterType _FilterType = enFilterType.KeinFilter;

        public string Herkunft = string.Empty;

        #endregion


        #region  Event-Deklarationen + Delegaten 
        public event EventHandler Changed;
        #endregion


        #region  Construktor + Initialize 

        public FilterItem(Database database, enFilterType filterType, string searchValue) : this(database, filterType, new List<string>() { searchValue }) { }
        public FilterItem(Database database, enFilterType filterType, List<string> searchValue) {
            Database = database;
            _FilterType = filterType;
            if (searchValue != null && searchValue.Count > 0) { SearchValue.AddRange(searchValue); }
            SearchValue.Changed += SearchValue_ListOrItemChanged;
        }

        public FilterItem(Database database, string FilterCode) {
            Database = database;
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

        public FilterItem(ColumnItem column, RowItem rowWithValue) : this(column, enFilterType.Istgleich_GroﬂKleinEgal_MultiRowIgnorieren, rowWithValue.CellGetString(column)) { }

        #endregion


        #region  Properties 

        public bool IsParsing { get; private set; }

        public ColumnItem Column {
            get {
                return _Column;
            }
            set {
                _Column = value;
                OnChanged();
            }
        }


        public ListExt<string> SearchValue { get; private set; } = new ListExt<string>();

        public enFilterType FilterType {
            get {
                return _FilterType;
            }
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





        public string CompareKey() {
            return ((int)_FilterType).ToString(Constants.Format_Integer10);
        }

        public string ReadableText() {


            if (_FilterType == enFilterType.KeinFilter) { return "Filter ohne Funktion"; }


            if (_Column == null) { return "Zeilen-Filter"; }






            var nam = _Column.ReadableText();

            if (SearchValue == null || SearchValue.Count < 1) { return "#### Filter-Fehler ####"; }

            if (SearchValue.Count > 1) {
                switch (_FilterType) {
                    case enFilterType.Istgleich:
                    case enFilterType.IstGleich_ODER:
                    case enFilterType.Istgleich_GroﬂKleinEgal:
                    case enFilterType.Istgleich_ODER_GroﬂKleinEgal:
                        return nam + " - eins davon: '" + SearchValue.JoinWith("', '") + "'";

                    case enFilterType.IstGleich_UND:
                    case enFilterType.Istgleich_UND_GroﬂKleinEgal:
                        return nam + " - alle: '" + SearchValue.JoinWith("', '") + "'";

                    default:


                        return nam + ": Spezial-Filter";
                }

            }




            if (_Column == Database.Column.SysCorrect && _FilterType.HasFlag(enFilterType.Istgleich)) {
                if (SearchValue[0].FromPlusMinus()) { return "Fehlerfreie Zeilen"; }
                if (!SearchValue[0].FromPlusMinus()) { return "Fehlerhafte Zeilen"; }
            }


            switch (_FilterType) {
                case enFilterType.Istgleich:
                case enFilterType.Istgleich_GroﬂKleinEgal:
                case enFilterType.Istgleich_ODER_GroﬂKleinEgal:
                case enFilterType.Istgleich_UND_GroﬂKleinEgal:

                    if (string.IsNullOrEmpty(SearchValue[0])) { return nam + " muss leer sein"; }

                    return nam + " = " + LanguageTool.ColumnReplace(SearchValue[0], Column, enShortenStyle.Replaced);

                case enFilterType.Ungleich_MultiRowIgnorieren:
                case enFilterType.Ungleich_MultiRowIgnorieren_UND_GroﬂKleinEgal:
                case enFilterType.Ungleich_MultiRowIgnorieren_GroﬂKleinEgal:
                    if (string.IsNullOrEmpty(SearchValue[0])) { return nam + " muss bef¸llt sein"; }
                    return nam + " <> " + LanguageTool.ColumnReplace(SearchValue[0], Column, enShortenStyle.Replaced);


                case enFilterType.Istgleich_GroﬂKleinEgal_MultiRowIgnorieren:
                case enFilterType.Istgleich_MultiRowIgnorieren:
                    if (SearchValue.Count == 1 && string.IsNullOrEmpty(SearchValue[0])) { return nam + " muss leer sein"; }
                    return "Spezial-Filter";


                case enFilterType.Instr:
                case enFilterType.Instr_GroﬂKleinEgal:
                    if (SearchValue.Count == 0 || string.IsNullOrEmpty(SearchValue[0])) { return "Filter aktuell ohne Funktion"; }

                    return nam + " beinhaltet den Text '" + SearchValue[0] + "'";


                case enFilterType.Berechne:
                case (enFilterType.Berechne | enFilterType.UND):

                    if (SearchValue[0].ToUpper().StartsWith("BTW(VALUE, ")) {
                        var l = SearchValue[0].ToUpper().TrimStart("BTW(VALUE, ");
                        l = l.TrimEnd(")");
                        l = "von " + l.Replace(",", " bis ");

                        return nam + ": " + l;
                    }


                    return nam + ": Spezial-Filter";

                default:
                    return nam + ": Spezial-Filter";

            }
        }

        public QuickImage SymbolForReadableText() {
            return null;
        }

        public bool IsNullOrEmpty() {
            if (_FilterType == enFilterType.KeinFilter) { return true; }
            return false;
        }



        public object Clone() {
            return new FilterItem(Database, ToString());
        }

        public bool IsOk() {
            return string.IsNullOrEmpty(ErrorReason());
        }

        public string ErrorReason() {
            if (_FilterType == enFilterType.KeinFilter) { return "'Kein Filter' angegeben"; }
            return string.Empty;
        }

        private void SearchValue_ListOrItemChanged(object sender, System.EventArgs e) {
            OnChanged();
        }

        public void Changeto(enFilterType type, string searchvalue) {
            SearchValue.ThrowEvents = false;

            SearchValue.Clear();
            SearchValue.Add(searchvalue);
            _FilterType = type;
            SearchValue.ThrowEvents = true;
            OnChanged();
        }
    }
}