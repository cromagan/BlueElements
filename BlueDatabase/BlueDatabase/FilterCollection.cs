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
using System.Collections.Generic;

namespace BlueDatabase {
    public sealed class FilterCollection : ListExt<FilterItem>, IParseable {

        #region  Variablen-Deklarationen 
        public Database Database { get; private set; }
        #endregion

        public bool IsParsing { get; private set; }

        #region  Construktor + Initialize 
        public FilterCollection(Database database) {
            Database = database;
            Database.Disposing += Database_Disposing;
        }
        private void Database_Disposing(object sender, System.EventArgs e) => Dispose();
        public FilterCollection(Database database, string toParse) : this(database) => Parse(toParse);
        #endregion


        #region Properties
        public FilterItem this[ColumnItem column] {
            get {
                foreach (var ThisFilterItem in this) {
                    if (ThisFilterItem != null && ThisFilterItem.FilterType != enFilterType.KeinFilter) {
                        if (ThisFilterItem.Column == column) { return ThisFilterItem; }
                    }
                }
                return null;
            }
        }
        #endregion

        public void Remove(ColumnItem column) {
            List<FilterItem> toDel = new();
            foreach (var thisFilter in this) {
                if (thisFilter.Column == column) { toDel.Add(thisFilter); }
            }
            if (toDel.Count == 0) { return; }
            RemoveRange(toDel);
        }

        public void Remove(string columnName) {
            var tmp = Database.Column[columnName];
            if (tmp == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Spalte '" + columnName + "' nicht vorhanden."); }
            Remove(tmp);
        }

        public void Remove_RowFilter() => Remove((ColumnItem)null);
        public void Add(enFilterType filterType, string filterBy) => AddIfNotExists(new FilterItem(Database, filterType, filterBy));
        public void Add(enFilterType filterType, List<string> filterBy) => AddIfNotExists(new FilterItem(Database, filterType, filterBy));
        public void Add(string columnName, enFilterType filterType, string filterBy) => Add(Database.Column[columnName], filterType, filterBy);
        public void Add(string columnName, enFilterType filterType, List<string> filterBy) => Add(Database.Column[columnName], filterType, filterBy);
        public void Add(ColumnItem column, enFilterType filterType, List<string> filterBy) => AddIfNotExists(new FilterItem(column, filterType, filterBy));
        public void Add(ColumnItem column, enFilterType filterType, string filterBy) => AddIfNotExists(new FilterItem(column, filterType, filterBy));
        private void AddIfNotExists(FilterItem filterItem) {
            if (Exists(filterItem)) { return; }
            Add(filterItem);
        }

        public bool IsRowFilterActiv() => this[null] != null;
        public string RowFilterText {
            get {
                var f = this[null];
                return f != null ? f.SearchValue[0] : string.Empty;
            }
            set {
                var f = this[null];
                if (f != null) {
                    if (f.SearchValue[0].ToLower() == value.ToLower()) { return; }
                    f.SearchValue[0] = value;
                    return;
                }
                FilterItem fi = new(Database, enFilterType.Instr_UND_GroßKleinEgal, value);
                Add(fi);
            }
        }

        public bool MayHasRowFilter(ColumnItem Column) => !Column.IgnoreAtRowFilter && IsRowFilterActiv();
        public void Parse(string ToParse) {
            IsParsing = true;
            ThrowEvents = false;
            // Initialize();
            foreach (var pair in ToParse.GetAllTags()) {
                switch (pair.Key) {

                    case "filter":
                        AddIfNotExists(new FilterItem(Database, pair.Value));
                        break;
                    default:
                        Develop.DebugPrint(enFehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                        break;
                }
            }
            ThrowEvents = true;
            IsParsing = false;
        }

        public override string ToString() {
            var w = "{";
            foreach (var ThisFilterItem in this) {
                if (ThisFilterItem != null) {
                    w = w + "Filter=" + ThisFilterItem + ", ";
                }
            }
            return w.TrimEnd(", ") + "}";
        }

        public void RemoveOtherAndAddIfNotExists(ColumnItem column, enFilterType filterType, string filterBy, string herkunft) => RemoveOtherAndAddIfNotExists(new FilterItem(column, filterType, filterBy, herkunft));
        public void RemoveOtherAndAddIfNotExists(string columName, enFilterType filterType, string filterBy, string herkunft) {
            var column = Database.Column[columName];
            if (column == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Spalte '" + columName + "' nicht vorhanden."); }
            RemoveOtherAndAddIfNotExists(column, filterType, filterBy, herkunft);
        }

        public void RemoveOtherAndAddIfNotExists(FilterItem filterItem) {
            if (Exists(filterItem)) { return; }
            Remove(filterItem.Column);
            if (Exists(filterItem)) { return; } // Falls ein Event ausgelöst wurde, und es nun doch schon das ist
            Add(filterItem);
        }

        public bool Exists(FilterItem filterItem) {
            foreach (var thisFilter in this) {
                if (thisFilter.FilterType == filterItem.FilterType) {
                    if (thisFilter.Column == filterItem.Column) {
                        if (thisFilter.Herkunft == filterItem.Herkunft) {
                            if (thisFilter.SearchValue.JoinWithCr() == filterItem.SearchValue.JoinWithCr()) {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public void RemoveOtherAndAddIfNotExists(string columName, enFilterType filterType, List<string> filterBy, string herkunft) {
            var tmp = Database.Column[columName];
            if (tmp == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Spalte '" + columName + "' nicht vorhanden."); }
            RemoveOtherAndAddIfNotExists(new FilterItem(tmp, filterType, filterBy, herkunft));
        }
        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            if (Database != null) {
                Database.Disposing += Database_Disposing;
                Database = null;
            }
        }
    }
}