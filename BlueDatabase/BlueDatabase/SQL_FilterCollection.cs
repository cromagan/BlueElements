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
using System.Linq;

namespace BlueDatabase;

public sealed class SQL_FilterCollection : ListExt<SQL_FilterItem>, IParseable {

    #region Constructors

    public SQL_FilterCollection(SQL_Database? database) {
        Database = database;
        if (Database != null) { Database.Disposing += Database_Disposing; }
    }

    public SQL_FilterCollection(SQL_Database? database, string toParse) : this(database) {
        Parse(toParse);
    }

    #endregion

    #region Properties

    public SQL_Database? Database { get; private set; }

    public bool IsParsing { get; private set; }

    public string RowFilterText {
        get {
            var f = this[null];
            return f != null ? f.SearchValue[0] : string.Empty;
        }
        set {
            var f = this[null];
            if (f != null) {
                if (string.Equals(f.SearchValue[0], value, StringComparison.OrdinalIgnoreCase)) { return; }
                f.SearchValue[0] = value;
                return;
            }
            SQL_FilterItem fi = new(Database, FilterType.Instr_UND_GroßKleinEgal, value);
            Add(fi);
        }
    }

    #endregion

    #region Indexers

    public SQL_FilterItem? this[SQL_ColumnItem? column] => this.Where(thisSQL_FilterItem => thisSQL_FilterItem != null && thisSQL_FilterItem.FilterType != FilterType.KeinFilter).FirstOrDefault(thisSQL_FilterItem => thisSQL_FilterItem.Column == column);

    #endregion

    #region Methods

    public void Add(FilterType filterType, string filterBy) => AddIfNotExists(new SQL_FilterItem(Database, filterType, filterBy));

    public void Add(FilterType filterType, List<string> filterBy) => AddIfNotExists(new SQL_FilterItem(Database, filterType, filterBy));

    public void Add(string columnName, FilterType filterType, string filterBy) => Add(Database?.Column[columnName], filterType, filterBy);

    public void Add(string columnName, FilterType filterType, List<string> filterBy) => Add(Database?.Column[columnName], filterType, filterBy);

    public void Add(SQL_ColumnItem? column, FilterType filterType, List<string> filterBy) => AddIfNotExists(new SQL_FilterItem(column, filterType, filterBy));

    public void Add(SQL_ColumnItem? column, FilterType filterType, string filterBy) => AddIfNotExists(new SQL_FilterItem(column, filterType, filterBy));

    public bool Exists(SQL_FilterItem SQL_FilterItem) {
        foreach (var thisFilter in this) {
            if (thisFilter.FilterType == SQL_FilterItem.FilterType) {
                if (thisFilter.Column == SQL_FilterItem.Column) {
                    if (thisFilter.Herkunft == SQL_FilterItem.Herkunft) {
                        if (thisFilter.SearchValue.JoinWithCr() == SQL_FilterItem.SearchValue.JoinWithCr()) {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    public bool IsRowFilterActiv() => this[null] != null;

    public bool MayHasRowFilter(SQL_ColumnItem? column) => !column.IgnoreAtRowFilter && IsRowFilterActiv();

    public void Parse(string toParse) {
        IsParsing = true;
        ThrowEvents = false;
        // Initialize();
        foreach (var pair in toParse.GetAllTags()) {
            switch (pair.Key) {
                case "filter":
                    AddIfNotExists(new SQL_FilterItem(Database, pair.Value));
                    break;

                default:
                    Develop.DebugPrint(FehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                    break;
            }
        }
        ThrowEvents = true;
        IsParsing = false;
    }

    public void Remove(SQL_ColumnItem? column) {
        var toDel = this.Where(thisFilter => thisFilter.Column == column).ToList();
        if (toDel.Count == 0) { return; }
        RemoveRange(toDel);
    }

    public void Remove(string columnName) {
        var tmp = Database.Column[columnName];
        if (tmp == null) { Develop.DebugPrint(FehlerArt.Fehler, "Spalte '" + columnName + "' nicht vorhanden."); }
        Remove(tmp);
    }

    public void Remove_RowFilter() => Remove((SQL_ColumnItem)null);

    public void RemoveOtherAndAddIfNotExists(SQL_ColumnItem? column, FilterType filterType, string filterBy, string herkunft) => RemoveOtherAndAddIfNotExists(new SQL_FilterItem(column, filterType, filterBy, herkunft));

    public void RemoveOtherAndAddIfNotExists(string columName, FilterType filterType, string filterBy, string herkunft) {
        var column = Database.Column[columName];
        if (column == null) { Develop.DebugPrint(FehlerArt.Fehler, "Spalte '" + columName + "' nicht vorhanden."); }
        RemoveOtherAndAddIfNotExists(column, filterType, filterBy, herkunft);
    }

    public void RemoveOtherAndAddIfNotExists(SQL_FilterItem SQL_FilterItem) {
        if (Exists(SQL_FilterItem)) { return; }
        Remove(SQL_FilterItem.Column);
        if (Exists(SQL_FilterItem)) { return; } // Falls ein Event ausgelöst wurde, und es nun doch schon das ist
        Add(SQL_FilterItem);
    }

    public void RemoveOtherAndAddIfNotExists(string columName, FilterType filterType, List<string>? filterBy, string herkunft) {
        var tmp = Database.Column[columName];
        if (tmp == null) { Develop.DebugPrint(FehlerArt.Fehler, "Spalte '" + columName + "' nicht vorhanden."); }
        RemoveOtherAndAddIfNotExists(new SQL_FilterItem(tmp, filterType, filterBy, herkunft));
    }

    public override string ToString() {
        var w = "{";
        foreach (var thisSQL_FilterItem in this) {
            if (thisSQL_FilterItem != null) {
                w = w + "Filter=" + thisSQL_FilterItem + ", ";
            }
        }
        return w.TrimEnd(", ") + "}";
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        if (Database != null) {
            Database.Disposing += Database_Disposing;
            Database = null;
        }
    }

    //private void AddIfNotExists(SQL_FilterItem SQL_FilterItem) {
    //    if (Exists(SQL_FilterItem)) { return; }
    //    Add(SQL_FilterItem);
    //}

    private void Database_Disposing(object sender, System.EventArgs e) => Dispose();

    #endregion
}