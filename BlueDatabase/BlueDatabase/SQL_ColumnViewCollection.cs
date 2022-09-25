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

public sealed class SQL_ColumnViewCollection : ListExt<SQL_ColumnViewItem>, IParseable {
    //NICHT IReadableText, das gibt zu viele Probleme (Dropdownboxen)

    #region Fields

    private string _name;

    #endregion

    #region Constructors

    public SQL_ColumnViewCollection(SQL_Database database, string code) {
        Database = database;
        Database.Disposing += Database_Disposing;
        Parse(code);
    }

    public SQL_ColumnViewCollection(SQL_Database database, string code, string newname) {
        Database = database;
        Database.Disposing += Database_Disposing;
        Parse(code);
        _name = newname;
    }

    #endregion

    #region Properties

    public SQL_Database? Database { get; private set; }
    public bool IsParsing { get; private set; }

    public string Name {
        get => _name;
        set {
            if (_name == value) { return; }
            _name = value;
            OnChanged();
        }
    }

    public ListExt<string> PermissionGroups_Show { get; } = new();

    #endregion

    #region Indexers

    public SQL_ColumnViewItem? this[SQL_ColumnItem? vColumn] {
        get {
            if (vColumn == null) { return null; }

            return this.FirstOrDefault(thisViewItem => thisViewItem != null && thisViewItem.Column == vColumn);
        }
    }

    #endregion

    #region Methods

    public void Add(SQL_ColumnItem? column, bool permanent) {
        if (permanent) {
            Add(new SQL_ColumnViewItem(column, ViewType.PermanentColumn, this));
        } else {
            Add(new SQL_ColumnViewItem(column, ViewType.Column, this));
        }
    }

    public void Hide(string columnName) {
        foreach (var thisViewItem in this.Where(thisViewItem => thisViewItem != null && (thisViewItem.Column == null || string.Equals(thisViewItem.Column.Name, columnName, StringComparison.OrdinalIgnoreCase)))) {
            Remove(thisViewItem);
            Hide(columnName);
            return;
        }
    }

    public void HideSystemColumns() {
        foreach (var thisViewItem in this.Where(thisViewItem => thisViewItem != null && (thisViewItem.Column == null || !string.IsNullOrEmpty(thisViewItem.Column.Identifier)))) {
            Remove(thisViewItem);
            HideSystemColumns();
            return;
        }
    }

    public void Insert(int index, SQL_ColumnItem? column) => Insert(index, new SQL_ColumnViewItem(column, ViewType.Column, this));

    public void Invalidate_DrawWithOfAllItems() {
        foreach (var thisViewItem in this) {
            thisViewItem?.Invalidate_DrawWidth();
        }
    }

    public List<SQL_ColumnItem?> ListOfUsedColumn() {
        List<SQL_ColumnItem?> colList = new();
        foreach (var t in this) {
            if (t != null) { colList.Add(t.Column); }
        }
        return colList;
    }

    public SQL_ColumnItem? NextVisible(SQL_ColumnItem? column) {
        var viewItemNo = 0;
        var found = false;
        do {
            if (viewItemNo >= Count) { return null; }
            if (this[viewItemNo] != null && this[viewItemNo].Column != null) {
                if (found) { return this[viewItemNo].Column; }
                if (this[viewItemNo].Column == column) { found = true; }
            }
            viewItemNo++;
        } while (true);
    }

    public SQL_ColumnViewItem? NextVisible(SQL_ColumnViewItem viewItem) {
        var viewItemNo = IndexOf(viewItem);
        if (viewItemNo < 0) { return null; }
        do {
            viewItemNo++;
            if (viewItemNo >= Count) { return null; }
            if (this[viewItemNo] != null && this[viewItemNo].Column != null) { return this[viewItemNo]; }
        } while (true);
    }

    public override void OnChanged() {
        if (IsParsing) { Develop.DebugPrint(FehlerArt.Warnung, "Falscher Parsing Zugriff!"); return; }
        base.OnChanged();
        //Changed?.Invoke(this, System.EventArgs.Empty);
    }

    public void Parse(string toParse) {
        IsParsing = true;
        ThrowEvents = false;
        PermissionGroups_Show.ThrowEvents = false;
        Initialize();
        foreach (var pair in toParse.GetAllTags()) {
            switch (pair.Key) {
                case "name":
                    _name = pair.Value;
                    break;

                case "columndata":
                    Add(new SQL_ColumnViewItem(Database, pair.Value, this));
                    break;

                case "permissiongroup":
                    PermissionGroups_Show.Add(pair.Value);
                    break;

                default:
                    Develop.DebugPrint(FehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                    break;
            }
        }
        PermissionGroups_Show.ThrowEvents = true;
        ThrowEvents = true;
        IsParsing = false;
    }

    public SQL_ColumnItem? PreviousVisible(SQL_ColumnItem? column) {
        var viewItemNo = Count - 1;
        var found = false;
        do {
            if (viewItemNo < 0) { return null; }
            if (this[viewItemNo] != null && this[viewItemNo].Column != null) {
                if (found) { return this[viewItemNo].Column; }
                if (this[viewItemNo].Column == column) { found = true; }
            }
            viewItemNo--;
        } while (true);
    }

    public SQL_ColumnViewItem? PreviousVisible(SQL_ColumnViewItem viewItem) {
        var viewItemNo = IndexOf(viewItem);
        do {
            viewItemNo--;
            if (viewItemNo < 0) { return null; }
            if (this[viewItemNo] != null && this[viewItemNo].Column != null) { return this[viewItemNo]; }
        } while (true);
    }

    public void ShowAllColumns() {
        if (Database.IsParsing) { return; }
        var ok = true;
        for (var z = 0; z < Database.Column.Count; z++) {
            if (z >= Count) {
                ok = false;
                break;
            }
            if (this[z].Column != Database.Column[z]) {
                ok = false;
                break;
            }
        }
        if (Count > 0 && this[0].ViewType != ViewType.PermanentColumn) { ok = false; }
        if (ok) { return; }
        Clear();
        foreach (var thisSQL_ColumnItem in Database.Column.Where(thisSQL_ColumnItem => thisSQL_ColumnItem != null)) {
            Add(new SQL_ColumnViewItem(thisSQL_ColumnItem, ViewType.Column, this));
        }
        if (Count > 0) { this[0].ViewType = ViewType.PermanentColumn; }
    }

    public new void Swap(SQL_ColumnViewItem? viewItem1, SQL_ColumnViewItem? viewItem2) {
        if (viewItem1 == null || viewItem2 == null) { return; }
        base.Swap(viewItem1, viewItem2);
        if (viewItem2.ViewType != ViewType.PermanentColumn) { viewItem1.ViewType = ViewType.Column; }
    }

    public override string ToString() {
        Develop.DebugPrint_Disposed(IsDisposed);
        var result = "{Name=" + _name.ToNonCritical();
        foreach (var thisViewItem in this) {
            if (thisViewItem != null) {
                result = result + ", Columndata=" + thisViewItem;
            }
        }
        var tmp = PermissionGroups_Show.SortedDistinctList();
        tmp.RemoveString("#Administrator", false);
        foreach (var t in tmp) {
            if (!string.IsNullOrEmpty(t)) {
                result = result + ", Permissiongroup=" + t;
            }
        }
        return result + "}";
    }

    internal void Repair(int number, bool isTableView) {

        #region Ungültige Spalten entfernen

        for (var z = 0; z < Count; z++) {
            if (this[z].Column == null || !Database.Column.Contains(this[z].Column)) {
                this[z] = null;
            }
        }
        this.RemoveNull();

        #endregion

        var tmp = PermissionGroups_Show.SortedDistinctList();
        tmp.RemoveString("#Administrator", false);
        tmp.RemoveNullOrEmpty();

        switch (number) {
            case 0:
                if (isTableView) {
                    if (string.IsNullOrEmpty(Name)) { Name = "Alle Spalten"; }
                    ShowAllColumns();
                } else {
                    if (string.IsNullOrEmpty(Name)) { Name = "Kopf"; }
                    if (PermissionGroups_Show.Count > 0) { PermissionGroups_Show.Clear(); }
                }
                break;

            case 1:
                if (string.IsNullOrEmpty(Name)) { Name = "Standard"; }
                tmp.AddIfNotExists("#Everybody");
                break;
        }

        tmp = tmp.SortedDistinctList();

        if (PermissionGroups_Show.IsDifferentTo(tmp)) {
            PermissionGroups_Show.Clear();
            PermissionGroups_Show.AddRange(tmp);
        }

        if (string.IsNullOrEmpty(Name)) { Name = "Ansicht " + number; }
    }

    protected override void Dispose(bool disposing) {
        PermissionGroups_Show.Changed -= _PermissionGroups_Show_ListOrItemChanged;
        PermissionGroups_Show.Clear();
        Database.Disposing += Database_Disposing;
        Database = null;
        base.Dispose(disposing);
    }

    private void _PermissionGroups_Show_ListOrItemChanged(object sender, System.EventArgs e) => OnChanged();

    private void Database_Disposing(object sender, System.EventArgs e) => Dispose();

    private void Initialize() {
        _name = string.Empty;
        PermissionGroups_Show.Clear();
        PermissionGroups_Show.Changed += _PermissionGroups_Show_ListOrItemChanged;
    }

    #endregion
}