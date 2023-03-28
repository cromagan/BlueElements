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
using System.Linq;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using BlueDatabase.Interfaces;

namespace BlueDatabase;

public sealed class ColumnViewCollection : List<ColumnViewItem?>, IParseable, ICloneable, IDisposableExtended, IHasDatabase, IHasKeyName {
    //NICHT IReadableText, das gibt zu viele Probleme (Dropdownboxen)

    #region Fields

    private readonly List<string> _permissionGroups_Show = new();
    private string _name;

    #endregion

    #region Constructors

    public ColumnViewCollection(DatabaseAbstract database, string toParse) {
        Database = database;
        Database.Disposing += Database_Disposing;
        _name = string.Empty;
        Parse(toParse);
    }

    public ColumnViewCollection(DatabaseAbstract database, string toParse, string newname) : this(database, toParse) => _name = newname;

    #endregion

    #region Events

    public event EventHandler? Changed;

    #endregion

    #region Properties

    public DatabaseAbstract? Database { get; private set; }

    public bool IsDisposed { get; private set; }
    public string KeyName => _name;

    public string Name {
        get => _name;
        set {
            if (_name == value) { return; }
            _name = value;
            OnChanged();
        }
    }

    public ReadOnlyCollection<string> PermissionGroups_Show {
        get => new(_permissionGroups_Show);
        set {
            var l = value.SortedDistinctList();

            if (l.IsDifferentTo(_permissionGroups_Show)) {
                _permissionGroups_Show.Clear();
                _permissionGroups_Show.AddRange(l);
                OnChanged();
            }
        }
    }

    #endregion

    #region Indexers

    public ColumnViewItem? this[ColumnItem? vColumn] {
        get {
            if (vColumn == null) { return null; }

            return this.FirstOrDefault(thisViewItem => thisViewItem != null && thisViewItem.Column == vColumn);
        }
    }

    #endregion

    #region Methods

    public void Add(ColumnItem? column, bool permanent) {
        if (column == null) { return; }

        Add(permanent
            ? new ColumnViewItem(column, ViewType.PermanentColumn, this)
            : new ColumnViewItem(column, ViewType.Column, this));
    }

    public new void Add(ColumnViewItem item) {
        base.Add(item);
        OnChanged();
    }

    public object Clone() => new ColumnViewCollection(Database, ToString());

    public void Dispose() => Dispose(true);

    public void Hide(string columnName) {
        foreach (var thisViewItem in this) {
            if (thisViewItem != null && (thisViewItem.Column == null || string.Equals(thisViewItem.Column.Name, columnName, StringComparison.OrdinalIgnoreCase))) {
                Remove(thisViewItem);
                Hide(columnName);
                return;
            }
        }
    }

    public void HideSystemColumns() {
        foreach (var thisViewItem in this) {
            if (thisViewItem != null && (thisViewItem.Column == null || thisViewItem.Column.IsSystemColumn())) {
                Remove(thisViewItem);
                HideSystemColumns();
                return;
            }
        }
    }

    public void Invalidate_DrawWithOfAllItems() {
        foreach (var thisViewItem in this) {
            thisViewItem?.Invalidate_DrawWidth();
        }
    }

    public List<ColumnItem> ListOfUsedColumn() {
        List<ColumnItem> colList = new();
        foreach (var t in this) {
            if (t?.Column != null) { colList.Add(t.Column); }
        }
        return colList;
    }

    public ColumnItem? NextVisible(ColumnItem? column) {
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

    public ColumnViewItem? NextVisible(ColumnViewItem viewItem) {
        var viewItemNo = IndexOf(viewItem);
        if (viewItemNo < 0) { return null; }
        do {
            viewItemNo++;
            if (viewItemNo >= Count) { return null; }
            if (this[viewItemNo] != null && this[viewItemNo].Column != null) { return this[viewItemNo]; }
        } while (true);
    }

    public void OnChanged() => Changed?.Invoke(this, System.EventArgs.Empty);

    public void Parse(string toParse) {
        foreach (var pair in toParse.GetAllTags()) {
            switch (pair.Key) {
                case "name":
                    _name = pair.Value;
                    break;

                case "columndata":
                    base.Add(new ColumnViewItem(Database, pair.Value, this)); // BAse, um Events zu vermeiden
                    break;

                case "permissiongroup":
                    _permissionGroups_Show.Add(pair.Value);
                    break;

                default:
                    Develop.DebugPrint(FehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                    break;
            }
        }
    }

    public ColumnItem? PreviousVisible(ColumnItem? column) {
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

    public ColumnViewItem? PreviousVisible(ColumnViewItem viewItem) {
        var viewItemNo = IndexOf(viewItem);
        do {
            viewItemNo--;
            if (viewItemNo < 0) { return null; }
            if (this[viewItemNo] != null && this[viewItemNo].Column != null) { return this[viewItemNo]; }
        } while (true);
    }

    public new void Remove(ColumnViewItem item) {
        _ = base.Remove(item);
        OnChanged();
    }

    public void ShowAllColumns() {
        if (Database == null || Database.IsDisposed) { return; }

        foreach (var thisColumn in Database.Column) {
            if (this[thisColumn] == null) {
                Add(new ColumnViewItem(thisColumn, ViewType.Column, this));
            }
        }
    }

    public void ShowColumns(params string[] columnnames) {
        if (Database == null || Database.IsDisposed) { return; }

        foreach (var thisColumn in columnnames) {
            var c = Database?.Column.Exists(thisColumn);

            if (c != null && this[c] == null) {
                Add(new ColumnViewItem(c, ViewType.Column, this));
            }
        }
    }

    public void Swap(int index1, int index2) {
        if (index1 == index2) { return; }

        (base[index1], base[index2]) = (base[index2], base[index1]);

        if (base[index2].ViewType != ViewType.PermanentColumn) { base[index1].ViewType = ViewType.Column; }
        OnChanged();
    }

    //public void Swap(ColumnViewItem? viewItem1, ColumnViewItem? viewItem2) {
    //    if (viewItem1 == null || viewItem2 == null) { return; }

    //    var index1 = IndexOf(viewItem1);
    //    var index2 = IndexOf(viewItem2);

    //    Swap(index1, index2);
    //}

    public override string ToString() {
        if (IsDisposed) { return string.Empty; }
        var result = "{Name=" + _name.ToNonCritical();
        foreach (var thisViewItem in this) {
            if (thisViewItem != null) {
                result = result + ", Columndata=" + thisViewItem;
            }
        }
        var tmp = PermissionGroups_Show.SortedDistinctList();
        tmp.RemoveString(DatabaseAbstract.Administrator, false);
        foreach (var t in tmp) {
            if (!string.IsNullOrEmpty(t)) {
                result = result + ", Permissiongroup=" + t;
            }
        }
        return result + "}";
    }

    internal void Repair(int number) {
        if (Database == null || Database.IsDisposed) { return; }

        #region Ungültige Spalten entfernen

        for (var z = 0; z < Count; z++) {
            if (this[z].Column == null || !Database.Column.Contains(this[z]?.Column)) {
                this[z] = null;
            }
        }
        _ = this.RemoveNull();

        #endregion

        var tmp = PermissionGroups_Show.SortedDistinctList();
        tmp.RemoveString(DatabaseAbstract.Administrator, false);
        tmp.RemoveNullOrEmpty();

        switch (number) {
            case 0:
                if (string.IsNullOrEmpty(Name)) { Name = "Alle Spalten"; }
                ShowAllColumns();
                break;

            case 1:
                if (string.IsNullOrEmpty(Name)) { Name = "Standard"; }
                _ = tmp.AddIfNotExists(DatabaseAbstract.Everybody);
                break;
        }

        PermissionGroups_Show = new ReadOnlyCollection<string>(tmp);

        if (string.IsNullOrEmpty(Name)) { Name = "Ansicht " + number; }
    }

    protected void Dispose(bool disposing) {
        IsDisposed = true;
        //PermissionGroups_Show.Changed -= _PermissionGroups_Show_ListOrItemChanged;
        //PermissionGroups_Show.Clear();
        if (Database != null) { Database.Disposing += Database_Disposing; }
        Database = null;
        //base.Dispose(disposing);
    }

    private void Database_Disposing(object sender, System.EventArgs e) => Dispose();

    #endregion
}