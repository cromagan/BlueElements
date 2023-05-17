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

public sealed class ColumnViewCollection : List<ColumnViewItem>, IParseable, ICloneable, IDisposableExtended, IHasDatabase, IHasKeyName {

    #region Fields

    private readonly List<string> _permissionGroups_Show = new();

    #endregion

    #region Constructors

    public ColumnViewCollection(DatabaseAbstract? database, string toParse) {
        Database = database;
        if (Database != null) {
            Database.Disposing += Database_Disposing;
        }

        KeyName = string.Empty;
        Parse(toParse);
    }

    public ColumnViewCollection(DatabaseAbstract database, string toParse, string newname) : this(database, toParse) => KeyName = newname;

    #endregion

    #region Properties

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

    public ColumnViewItem? this[ColumnItem? vColumn] {
        get {
            if (vColumn == null) { return null; }

            return this.FirstOrDefault(thisViewItem => thisViewItem != null && thisViewItem.Column == vColumn);
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
            if (thisViewItem != null && (thisViewItem.Column == null || string.Equals(thisViewItem.Column.Name, columnName, StringComparison.OrdinalIgnoreCase))) {
                _ = ca.Remove(thisViewItem);
                Hide(columnName, ca);
                return;
            }
        }
    }

    /// <summary>
    /// Static, um klar zumachen, dass die Collection nicht direkt bearbeitet werden kann.
    /// </summary>
    /// <param name="ca"></param>
    public static void HideSystemColumns(ColumnViewCollection ca) {
        foreach (var thisViewItem in ca) {
            if (thisViewItem != null && (thisViewItem.Column == null || thisViewItem.Column.IsSystemColumn())) {
                ca.Remove(thisViewItem);
                HideSystemColumns(ca);
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
        if (ca.Database == null || ca.Database.IsDisposed) { return; }

        #region Ungültige Spalten entfernen

        for (var z = 0; z < ca.Count; z++) {
            if (ca[z].Column == null || !ca.Database.Column.Contains(ca[z]?.Column)) {
                ca.RemoveAt(z);
                z--;
            }
        }
        //_ = ca.RemoveNull();

        #endregion

        var tmp = ca.PermissionGroups_Show.SortedDistinctList();
        tmp.RemoveString(DatabaseAbstract.Administrator, false);
        tmp.RemoveNullOrEmpty();

        switch (number) {
            case 0:
                if (string.IsNullOrEmpty(ca.KeyName)) { ca.KeyName = "Alle Spalten"; }
                ShowAllColumns(ca);
                break;

            case 1:
                if (string.IsNullOrEmpty(ca.KeyName)) { ca.KeyName = "Standard"; }
                _ = tmp.AddIfNotExists(DatabaseAbstract.Everybody);
                break;
        }

        ca.PermissionGroups_Show = new ReadOnlyCollection<string>(tmp);

        if (string.IsNullOrEmpty(ca.KeyName)) { ca.KeyName = "Ansicht " + number; }
    }

    /// <summary>
    /// Static, um klar zumachen, dass die Collection nicht direkt bearbeitet werden kann.
    /// </summary>
    /// <param name="ca"></param>
    public static void ShowAllColumns(ColumnViewCollection ca) {
        if (ca.Database == null || ca.Database.IsDisposed) { return; }

        foreach (var thisColumn in ca.Database.Column) {
            if (ca[thisColumn] == null) {
                ca.Add(new ColumnViewItem(thisColumn, ViewType.Column, ca));
            }
        }
    }

    public void Add(ColumnItem? column, bool permanent) {
        if (column == null) { return; }

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
    public ColumnViewItem? First() {
        return this.FirstOrDefault(thisViewItem => thisViewItem?.Column != null);
    }

    public void Invalidate_DrawWithOfAllItems() {
        foreach (var thisViewItem in this) {
            thisViewItem?.Invalidate_DrawWidth();
        }
    }

    public ColumnViewItem? Last() {
        return this.Last(thisViewItem => thisViewItem?.Column != null);
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
            if (this[viewItemNo]?.Column is ColumnItem c) {
                if (found) { return c; }
                if (c == column) { found = true; }
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
            if (this[viewItemNo]?.Column != null) { return this[viewItemNo]; }
        } while (true);
    }

    public void Parse(string toParse) {
        foreach (var pair in toParse.GetAllTags()) {
            switch (pair.Key) {
                case "name":
                    KeyName = pair.Value;
                    break;

                case "columndata":
                    if (Database != null) {
                        base.Add(new ColumnViewItem(Database, pair.Value, this)); // BAse, um Events zu vermeiden
                    }

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
            if (this[viewItemNo]?.Column is ColumnItem c) {
                if (found) { return c; }
                if (c == column) { found = true; }
            }
            viewItemNo--;
        } while (true);
    }

    public ColumnViewItem? PreviousVisible(ColumnViewItem viewItem) {
        var viewItemNo = IndexOf(viewItem);
        do {
            viewItemNo--;
            if (viewItemNo < 0) { return null; }
            if (this[viewItemNo]?.Column != null) { return this[viewItemNo]; }
        } while (true);
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
    }

    public override string ToString() {
        if (IsDisposed) { return string.Empty; }
        var result = "{Name=" + KeyName.ToNonCritical();
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

    //    Swap(index1, index2);
    //}
    private void Database_Disposing(object sender, System.EventArgs e) => Dispose();

    #endregion
}