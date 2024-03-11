// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using BlueDatabase.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using static BlueBasics.Constants;

namespace BlueDatabase;

public sealed class ColumnViewCollection : IEnumerable<ColumnViewItem>, IParseable, ICloneable, IDisposableExtended, IHasDatabase, IHasKeyName, IReadableTextWithKey {

    #region Fields

    public bool ShowHead = true;
    private readonly List<ColumnViewItem> _internal = [];
    private readonly List<string> _permissionGroups_Show = [];
    private Database? _database;
    private int? _headSize;

    #endregion

    #region Constructors

    public ColumnViewCollection(Database? database, string toParse) {
        Database = database;
        KeyName = string.Empty;
        this.Parse(toParse);
    }

    public ColumnViewCollection(Database database, string toParse, string newname) : this(database, toParse) => KeyName = newname;

    #endregion

    #region Properties

    public int Count => _internal.Count;

    public Database? Database {
        get => _database;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == _database) { return; }

            if (_database != null) {
                _database.DisposingEvent -= _database_Disposing;
            }
            _database = value;

            if (_database != null) {
                _database.DisposingEvent += _database_Disposing;
            }
        }
    }

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

    public string QuickInfo => string.Empty;

    #endregion

    #region Indexers

    public ColumnViewItem? this[int index] => _internal[index];

    public ColumnViewItem? this[ColumnItem? vColumn] {
        get {
            return vColumn == null ? null : _internal.FirstOrDefault(thisViewItem => thisViewItem != null && thisViewItem.Column == vColumn);
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Static, um klar zumachen, dass die Collection nicht direkt bearbeitet werden kann.
    /// </summary>
    /// <param name="ca"></param>
    /// <param name="number"></param>
    public static void Repair(ColumnViewCollection ca, int number) {
        if (ca.Database is not Database db || db.IsDisposed) { return; }

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
        tmp.RemoveString(Administrator, false);
        //tmp.RemoveNullOrEmpty();

        switch (number) {
            case 0:
                if (string.IsNullOrEmpty(ca.KeyName)) { ca.KeyName = "Alle Spalten"; }
                ca.ShowAllColumns();
                break;

            case 1:
                if (string.IsNullOrEmpty(ca.KeyName)) { ca.KeyName = "Standard"; }
                _ = tmp.AddIfNotExists(Everybody);
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

    public ColumnItem? ColumnOnCoordinate(int xpos, Rectangle displayRectangleWithoutSlider, int pix16, Font cellFont) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return null; }

        foreach (var thisViewItem in this) {
            if (thisViewItem?.Column != null) {
                if (xpos >= thisViewItem.OrderTmpSpalteX1 && xpos <= thisViewItem.OrderTmpSpalteX1 + thisViewItem.DrawWidth(displayRectangleWithoutSlider, pix16, cellFont)) {
                    return thisViewItem.Column;
                }
            }
        }
        return null;
    }

    public void Dispose() => Dispose(true);

    public void Dispose(bool disposing) {
        IsDisposed = true;
        if (disposing) {
            // TODO: verwalteten Zustand (verwaltete Objekte) entsorgen.
        }
        // TODO: nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer weiter unten überschreiben.
        // TODO: große Felder auf Null setzen.
        //PermissionGroups_Show.Changed -= _PermissionGroups_Show_ListOrItemChanged;
        //PermissionGroups_Show.Clear();
        Database = null;
        //base.Dispose(disposing);
    }

    //NICHT IReadableText, das gibt zu viele Probleme (Dropdownboxen)
    public ColumnViewItem? First() => _internal.FirstOrDefault(thisViewItem => thisViewItem?.Column != null);

    public IEnumerator<ColumnViewItem> GetEnumerator() => ((IEnumerable<ColumnViewItem>)_internal).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_internal).GetEnumerator();

    public int HeadSize(Font columnFont) {
        if (_headSize != null) { return (int)_headSize; }

        if (!ShowHead || Count - 1 < 0) {
            _headSize = 0;
            return 0;
        }
        _headSize = 16;
        foreach (var thisViewItem in this) {
            if (thisViewItem?.Column != null) {
                _headSize = Math.Max((int)_headSize, (int)thisViewItem.Column.ColumnHead_Size(columnFont).Height);
            }
        }

        _headSize += 8;
        _headSize += AutoFilterSize;
        return (int)_headSize;
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

    public int IndexOf(ColumnViewItem? columnViewItem) => columnViewItem == null ? -1 : _internal.IndexOf(columnViewItem);

    public void Invalidate_DrawWithOfAllItems() {
        foreach (var thisViewItem in _internal) {
            thisViewItem?.Invalidate_DrawWidth();
        }
    }

    public void Invalidate_HeadSize() => _headSize = null;

    public ColumnViewItem? Last() => _internal.Last(thisViewItem => thisViewItem?.Column != null);

    public List<ColumnItem> ListOfUsedColumn() {
        List<ColumnItem> colList = [];
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

            case "showhead":
                ShowHead = value.FromPlusMinus();
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

    public string ReadableText() => KeyName;

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

    public IEnumerable<ColumnItem> ReducedColumns() {
        var x = new List<ColumnItem>();
        foreach (var thiscol in _internal) {
            if (thiscol?.Column != null && thiscol.TmpReduced) { x.Add(thiscol.Column); }
        }
        return x;
    }

    public void RemoveAt(int z) => _internal.RemoveAt(z);

    public void ShowAllColumns() {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }

        foreach (var thisColumn in db.Column) {
            if (this[thisColumn] == null && !thisColumn.IsDisposed) {
                Add(new ColumnViewItem(thisColumn, ViewType.Column, this));
            }
        }
    }

    public void ShowColumns(params string[] columnnames) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }

        foreach (var thisColumn in columnnames) {
            var c = Database?.Column[thisColumn];

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

    public QuickImage? SymbolForReadableText() => null;

    public override string ToString() {
        if (IsDisposed) { return string.Empty; }

        var result = new List<string>();
        result.ParseableAdd("Name", (IHasKeyName)this);
        result.ParseableAdd("ShowHead", ShowHead);
        result.ParseableAdd("Columns", "Column", _internal);

        var tmp = PermissionGroups_Show.SortedDistinctList();
        tmp.RemoveString(Administrator, false);
        result.ParseableAdd("Permissiongroups", tmp);

        return result.Parseable();
    }

    private void _database_Disposing(object sender, System.EventArgs e) => Dispose();

    private void Add(ColumnViewItem columnViewItem) => _internal.Add(columnViewItem);

    private void Remove(ColumnViewItem columnViewItem) {
        if (_internal.Remove(columnViewItem)) {
            //columnViewItem.Changed -= ColumnViewItem_Changed;
        }
    }

    #endregion
}