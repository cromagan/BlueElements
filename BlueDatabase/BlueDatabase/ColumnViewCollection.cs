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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BlueBasics;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using BlueDatabase.Interfaces;

namespace BlueDatabase;

public sealed class ColumnViewCollection : IParseable, ICloneable, IDisposableExtended, IHasDatabase, IHasKeyName, IEnumerable<ColumnViewItem> {

    #region Fields

    private readonly List<ColumnViewItem> _internal = new();

    private readonly List<string> _permissionGroups_Show = new();

    #endregion

    #region Constructors

    public ColumnViewCollection(DatabaseAbstract? database, string toParse) {
        Database = database;
        if (Database != null) {
            Database.Disposing += Database_Disposing;
        }

        KeyName = string.Empty;
        this.Parse(toParse);
    }

    public ColumnViewCollection(DatabaseAbstract database, string toParse, string newname) : this(database, toParse) => KeyName = newname;

    #endregion

    #region Properties

    public int Count => _internal.Count;

    public DatabaseAbstract? Database { get; private set; }

    //public event EventHandler? Changed;
    public bool IsDisposed { get; private set; }

    public string KeyName { get; set; }

    public ReadOnlyCollection<string> PermissionGroups_Show {
        get => new(_permissionGroups_Show);
        set {
            var l = value.SortedDistinctList();

            if (l.IsDifferentTo(_permissionGroups_Show)) {
                _permissionGroups_Show.Clear();
                _permissionGroups_Show.AddRange(l);
            }
        }
    }

    #endregion

    #region Indexers

    public ColumnViewItem? this[int index] => _internal[index];

    public ColumnViewItem? this[ColumnItem? vColumn] {
        get {
            if (vColumn == null) { return null; }

            return _internal.FirstOrDefault(thisViewItem => thisViewItem != null && thisViewItem.Column == vColumn);
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Static, um klar zumachen, dass die Collection nicht direkt bearbeitet werden kann.
    /// </summary>
    /// <param name="columnName"></param>
    /// <param name="ca"></param>
    public static void Hide(string columnName, ColumnViewCollection ca) {
        foreach (var thisViewItem in ca) {
            if (thisViewItem != null && (thisViewItem.Column == null || string.Equals(thisViewItem.Column.KeyName, columnName, StringComparison.OrdinalIgnoreCase))) {
                ca.Remove(thisViewItem);
                Hide(columnName, ca);
                return;
            }
        }
    }

    /// <summary>
    /// Static, um klar zumachen, dass die Collection nicht direkt bearbeitet werden kann.
    /// </summary>
    /// <param name="ca"></param>
    /// <param name="number"></param>
    public static void Repair(ColumnViewCollection ca, int number) {
        if (ca.Database is not DatabaseAbstract db || db.IsDisposed) { return; }

        #region Ungültige Spalten entfernen

        for (var z = 0; z < ca.Count; z++) {
            if (ca[z]?.Column == null || !db.Column.Contains(ca[z]?.Column)) {
                ca.RemoveAt(z);
                z--;
            }
        }
        //_ = ca.RemoveNull();

        #endregion

        var tmp = ca.PermissionGroups_Show.SortedDistinctList();
        tmp.RemoveString(Constants.Administrator, false);
        //tmp.RemoveNullOrEmpty();

        switch (number) {
            case 0:
                if (string.IsNullOrEmpty(ca.KeyName)) { ca.KeyName = "Alle Spalten"; }
                ca.ShowAllColumns();
                break;

            case 1:
                if (string.IsNullOrEmpty(ca.KeyName)) { ca.KeyName = "Standard"; }
                _ = tmp.AddIfNotExists(Constants.Everybody);
                break;
        }

        ca.PermissionGroups_Show = new ReadOnlyCollection<string>(tmp);

        if (string.IsNullOrEmpty(ca.KeyName)) { ca.KeyName = "Ansicht " + number; }
    }

    public void Add(ColumnItem? column, bool permanent) {
        if (column == null || column.IsDisposed) { return; }

        Add(permanent
            ? new ColumnViewItem(column, ViewType.PermanentColumn, this)
            : new ColumnViewItem(column, ViewType.Column, this));
    }

    public object Clone() => new ColumnViewCollection(Database, ToString());

    public void Dispose() => Dispose(true);

    public void Dispose(bool disposing) {
        IsDisposed = true;
        //PermissionGroups_Show.Changed -= _PermissionGroups_Show_ListOrItemChanged;
        //PermissionGroups_Show.Clear();
        if (Database != null) { Database.Disposing += Database_Disposing; }
        Database = null;
        //base.Dispose(disposing);
    }

    //NICHT IReadableText, das gibt zu viele Probleme (Dropdownboxen)
    public ColumnViewItem? First() => _internal.FirstOrDefault(thisViewItem => thisViewItem?.Column != null);

    public IEnumerator<ColumnViewItem> GetEnumerator() => ((IEnumerable<ColumnViewItem>)_internal).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_internal).GetEnumerator();

    public void HideSystemColumns() {
        foreach (var thisViewItem in this) {
            if (thisViewItem != null && (thisViewItem.Column == null || thisViewItem.Column.IsSystemColumn())) {
                Remove(thisViewItem);
                HideSystemColumns();
                return;
            }
        }
    }

    public int IndexOf(ColumnViewItem? columnViewItem) {
        if (columnViewItem == null) { return -1; }
        return _internal.IndexOf(columnViewItem);
    }

    public void Invalidate_DrawWithOfAllItems() {
        foreach (var thisViewItem in _internal) {
            thisViewItem?.Invalidate_DrawWidth();
        }
    }

    public ColumnViewItem? Last() => _internal.Last(thisViewItem => thisViewItem?.Column != null);

    public List<ColumnItem> ListOfUsedColumn() {
        List<ColumnItem> colList = new();
        foreach (var t in _internal) {
            if (t?.Column != null) { colList.Add(t.Column); }
        }
        return colList;
    }

    public ColumnItem? NextVisible(ColumnItem? column) {
        var viewItemNo = 0;
        var found = false;
        do {
            if (viewItemNo >= _internal.Count) { return null; }
            if (_internal[viewItemNo]?.Column is ColumnItem c) {
                if (found) { return c; }
                if (c == column) { found = true; }
            }
            viewItemNo++;
        } while (true);
    }

    public ColumnViewItem? NextVisible(ColumnViewItem viewItem) {
        var viewItemNo = _internal.IndexOf(viewItem);
        if (viewItemNo < 0) { return null; }
        do {
            viewItemNo++;
            if (viewItemNo >= _internal.Count) { return null; }
            if (_internal[viewItemNo]?.Column != null) { return _internal[viewItemNo]; }
        } while (true);
    }

    public void ParseFinished(string parsed) { }

    public bool ParseThis(string key, string value) {
        switch (key) {
            case "name":
                KeyName = value;
                return true;

            case "columndata":

                _internal.Add(new ColumnViewItem(this, value)); // BAse, um Events zu vermeiden

                return true;

            case "columns":

                foreach (var pair2 in value.GetAllTags()) {
                    _internal.Add(new ColumnViewItem(this, pair2.Value.FromNonCritical())); // BAse, um Events zu vermeiden
                }

                return true;

            case "permissiongroup":
                _permissionGroups_Show.Add(value);
                return true;

            case "permissiongroups":
                _permissionGroups_Show.Clear();
                _permissionGroups_Show.AddRange(value.FromNonCritical().SplitByCrToList());
                return true;
        }

        return false;
    }

    public ColumnItem? PreviousVisible(ColumnItem? column) {
        var viewItemNo = _internal.Count - 1;
        var found = false;
        do {
            if (viewItemNo < 0) { return null; }
            if (_internal[viewItemNo]?.Column is ColumnItem c) {
                if (found) { return c; }
                if (c == column) { found = true; }
            }
            viewItemNo--;
        } while (true);
    }

    public ColumnViewItem? PreviousVisible(ColumnViewItem viewItem) {
        var viewItemNo = _internal.IndexOf(viewItem);
        do {
            viewItemNo--;
            if (viewItemNo < 0) { return null; }
            if (_internal[viewItemNo]?.Column != null) { return _internal[viewItemNo]; }
        } while (true);
    }

    /// <summary>
    /// Klappe alle Spalten ein, die in der ColumnListe vorhanden sind.
    /// Alle anderen werden ausgeklappt.
    /// </summary>
    /// <param name="columns"></param>
    public void Reduce(string[] columns) {
        foreach (var thiscv in _internal) {
            if (thiscv?.Column is ColumnItem ci) {
                thiscv.TmpReduced = columns.Contains(ci.KeyName);
            }
        }
    }

    public List<ColumnItem> ReducedColumns() {
        var x = new List<ColumnItem>();
        foreach (var thiscol in _internal) {
            if (thiscol?.Column != null && thiscol.TmpReduced) { x.Add(thiscol.Column); }
        }
        return x;
    }

    public void RemoveAt(int z) => _internal.RemoveAt(z);

    public void ShowAllColumns() {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }

        foreach (var thisColumn in db.Column) {
            if (this[thisColumn] == null && !thisColumn.IsDisposed) {
                Add(new ColumnViewItem(thisColumn, ViewType.Column, this));
            }
        }
    }

    public void ShowColumns(params string[] columnnames) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }

        foreach (var thisColumn in columnnames) {
            var c = Database?.Column.Exists(thisColumn);

            if (c != null && this[c] == null && this[c] == null && !c.IsDisposed) {
                Add(new ColumnViewItem(c, ViewType.Column, this));
            }
        }
    }

    public void Swap(int index1, int index2) {
        if (index1 == index2) { return; }

        (_internal[index1], _internal[index2]) = (_internal[index2], _internal[index1]);

        if (_internal[index2].ViewType != ViewType.PermanentColumn) { _internal[index1].ViewType = ViewType.Column; }
    }

    public override string ToString() {
        if (IsDisposed) { return string.Empty; }

        var result = new List<string>();
        result.ParseableAdd("Name", (IHasKeyName)this);
        result.ParseableAdd("Columns", "Column", _internal);

        var tmp = PermissionGroups_Show.SortedDistinctList();
        tmp.RemoveString(Constants.Administrator, false);
        result.ParseableAdd("Permissiongroups", tmp);

        return result.Parseable();
    }

    private void Add(ColumnViewItem columnViewItem) => _internal.Add(columnViewItem);

    private void Database_Disposing(object sender, System.EventArgs e) => Dispose();

    private void Remove(ColumnViewItem columnViewItem) {
        if (_internal.Remove(columnViewItem)) {
            //columnViewItem.Changed -= ColumnViewItem_Changed;
        }
    }

    #endregion
}