// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using BlueControls;
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueDatabase.Enums;
using BlueDatabase.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static BlueBasics.Constants;

namespace BlueDatabase;

public sealed class ColumnViewCollection : IEnumerable<ColumnViewItem>, IParseable, ICloneable, IDisposableExtended, IHasDatabase, IReadableTextWithKey, IEditable, IStyleable {

    #region Fields

    public bool ShowHead = true;

    private readonly List<ColumnViewItem> _internal = [];

    private readonly List<string> _permissionGroups_Show = [];

    private int _clientWidth = 16;

    private Database? _database;

    private int? _headSize;

    private string _sheetStyle = Win11;

    #endregion

    #region Constructors

    public ColumnViewCollection(Database? database, string toParse) {
        Database = database;
        KeyName = string.Empty;
        this.Parse(toParse);
    }

    public ColumnViewCollection(Database database, string toParse, string newname) : this(database, toParse) => KeyName = newname;

    #endregion

    #region Events

    public event EventHandler? StyleChanged;

    #endregion

    #region Properties

    public string CaptionForEditor => "Spaltenanordnung";

    public int ClientWidth {
        get => _clientWidth;
        set {
            if (_clientWidth == value) { return; }

            _clientWidth = value;
            OnStyleChanged();
        }
    }

    public string ColumnQuickInfo => string.Empty;

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

    public Type? Editor { get; set; }

    public BlueFont Font_RowChapter { get; internal set; } = BlueFont.DefaultFont;

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

    public int RowChapterHeight { get; internal set; } = 20;

    public string SheetStyle {
        get => _sheetStyle;
        set {
            if (IsDisposed) { return; }
            if (_sheetStyle == value) { return; }
            _sheetStyle = value;
            OnStyleChanged();
        }
    }

    public int WiederHolungsSpaltenWidth { get; private set; }

    #endregion

    #region Indexers

    public ColumnViewItem? this[int index] => index >= 0 && index < _internal.Count ? _internal[index] : null;

    public ColumnViewItem? this[ColumnItem? column] => column == null ? null : _internal.FirstOrDefault(thisViewItem => thisViewItem != null && thisViewItem.Column == column);

    #endregion

    #region Methods

    public static List<ColumnViewCollection> ParseAll(Database db) {
        var tcvc = new List<ColumnViewCollection>();
        List<string> ca = [.. db.ColumnArrangements.SplitAndCutByCr()];
        foreach (var t in ca) {
            tcvc.Add(new ColumnViewCollection(db, t));
        }

        if (tcvc.Count < 2) { tcvc.Add(new ColumnViewCollection(db, string.Empty)); }

        if (tcvc.Count < 2) { tcvc.Add(new ColumnViewCollection(db, string.Empty)); }

        foreach (var thisC in tcvc) {
            thisC.Editor = typeof(ColumnArrangementPadEditor);
        }

        return tcvc;
    }

    /// <summary>
    /// Static, um klar zumachen, dass die Collection nicht direkt bearbeitet werden kann.
    /// </summary>
    /// <param name="ca"></param>
    /// <param name="number"></param>
    public static void Repair(ColumnViewCollection ca, int number) {
        if (ca.Database is not { IsDisposed: false } db) { return; }

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

        ca.PermissionGroups_Show = tmp.AsReadOnly();

        if (string.IsNullOrEmpty(ca.KeyName)) { ca.KeyName = "Ansicht " + number; }

        //foreach (var thisView in ca) {
        //    thisView.Repair();
        //}
    }

    public void Add(ColumnItem column) {
        if (column is not { IsDisposed: false }) { return; }

        Add(new ColumnViewItem(column, this));
    }

    public void Add(ColumnViewItem columnViewItem) => _internal.Add(columnViewItem);

    public object Clone() => new ColumnViewCollection(Database, ParseableItems().FinishParseable());

    public void ComputeAllColumnPositions() {
        foreach (var thisViewItem in this) {
            thisViewItem.X = null;
        }

        if (IsDisposed) { return; }

        WiederHolungsSpaltenWidth = 0;

        var wdh = true;
        var maxX = 0;

        foreach (var thisViewItem in this) {
            if (thisViewItem?.Column != null) {
                if (thisViewItem.ViewType != ViewType.PermanentColumn) { wdh = false; }

                thisViewItem.X = maxX;

                maxX += thisViewItem.DrawWidth();
                if (wdh) {
                    WiederHolungsSpaltenWidth = Math.Max(maxX, WiederHolungsSpaltenWidth);
                }
            }
        }
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

    public int HeadSize() {
        if (_headSize != null) { return (int)_headSize; }

        if (!ShowHead || Count - 1 < 0) {
            _headSize = 0;
            return 0;
        }
        _headSize = 16;

        foreach (var thisViewItem in this) {
            if (thisViewItem?.Column != null) {
                _headSize = Math.Max((int)_headSize, (int)thisViewItem.ColumnHead_Size().Height);
            }
        }

        _headSize += 8;
        _headSize += ColumnViewItem.AutoFilterSize;
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

    public void Invalidate_ContentWidthOfAllItems() {
        foreach (var thisViewItem in _internal) {
            thisViewItem?.Invalidate_ContentWidth();
        }
    }

    public void Invalidate_HeadSize() {
        _headSize = null;

        foreach (var thisViewItem in this) {
            thisViewItem.Invalidate_Head();
        }
    }

    public void Invalidate_XOfAllItems() {
        foreach (var thisViewItem in _internal) {
            thisViewItem?.Invalidate_X();
        }
    }

    public string IsNowEditable() {
        if (Database is not { IsDisposed: false } db) { return "Datenbank verworfen"; }

        return db.GrantWriteAccess(DatabaseDataType.ColumnArrangement, DatabaseChunk.Chunk_Master);
    }

    public ColumnViewItem? Last() => _internal.Last(thisViewItem => thisViewItem?.Column != null);

    public List<ColumnItem> ListOfUsedColumn() {
        List<ColumnItem> colList = [];
        foreach (var t in _internal) {
            if (t?.Column != null) { colList.Add(t.Column); }
        }
        return colList;
    }

    public ColumnViewItem? NextVisible(ColumnViewItem? viewItem) {
        if (viewItem == null) { return null; }
        var viewItemNo = _internal.IndexOf(viewItem);
        if (viewItemNo < 0) { return null; }
        do {
            viewItemNo++;
            if (viewItemNo >= _internal.Count) { return null; }
            if (_internal[viewItemNo]?.Column != null) { return _internal[viewItemNo]; }
        } while (true);
    }

    public List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [];

        result.ParseableAdd("Name", this as IHasKeyName);
        result.ParseableAdd("ShowHead", ShowHead);
        result.ParseableAdd("Column", _internal);

        var tmp = PermissionGroups_Show.SortedDistinctList();
        tmp.RemoveString(Administrator, false);
        result.ParseableAdd("Permissiongroups", tmp, true);

        return result;
    }

    public void ParseFinished(string parsed) { }

    public bool ParseThis(string key, string value) {
        switch (key) {
            case "name":
                KeyName = value;
                return true;

            case "column":
            case "columndata":

                _internal.Add(new ColumnViewItem(this, value.FromNonCritical())); // BAse, um Events zu vermeiden

                return true;

            case "columns":
                if (value.GetAllTags() is { } x) {
                    foreach (var pair2 in x) {
                        _internal.Add(new ColumnViewItem(this, pair2.Value.FromNonCritical())); // BAse, um Events zu vermeiden
                    }
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
            if (thiscv?.Column is { IsDisposed: false } ci) {
                thiscv.Reduced = columns.Contains(ci.KeyName);
            }
        }
    }

    public List<ColumnItem> ReducedColumns() {
        var x = new List<ColumnItem>();
        foreach (var thiscol in _internal) {
            if (thiscol is { Column: not null, Reduced: true }) { x.Add(thiscol.Column); }
        }
        return x;
    }

    public void RemoveAll() => _internal.Clear();

    public void RemoveAt(int z) => _internal.RemoveAt(z);

    public void ShowAllColumns() {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return; }

        foreach (var thisColumn in db.Column) {
            if (this[thisColumn] == null && !thisColumn.IsDisposed && !thisColumn.IsSystemColumn()) {
                Add(new ColumnViewItem(thisColumn, this));
            }
        }

        foreach (var thisColumn in db.Column) {
            if (this[thisColumn] == null && !thisColumn.IsDisposed && thisColumn.IsSystemColumn()) {
                Add(new ColumnViewItem(thisColumn, this));
            }
        }
    }

    public void ShowColumns(params string[] columnnames) {
        if (IsDisposed || Database is not { IsDisposed: false }) { return; }

        foreach (var thisColumnName in columnnames) {
            var thisColumn = Database?.Column[thisColumnName];

            if (thisColumn != null && this[thisColumn] == null && this[thisColumn] == null && !thisColumn.IsDisposed) {
                Add(new ColumnViewItem(thisColumn, this));
            }
        }
    }

    public void Swap(int index1, int index2) {
        if (index1 == index2) { return; }

        (_internal[index1], _internal[index2]) = (_internal[index2], _internal[index1]);

        if (_internal[index2].ViewType != ViewType.PermanentColumn) { _internal[index1].ViewType = ViewType.Column; }
    }

    public QuickImage? SymbolForReadableText() => null;

    public override string ToString() => ParseableItems().FinishParseable();

    private void _database_Disposing(object sender, System.EventArgs e) => Dispose();

    private void OnStyleChanged() {
        Invalidate_HeadSize();
        Font_RowChapter = Skin.GetBlueFont(_sheetStyle, PadStyles.Überschrift);
        RowChapterHeight = (int)Font_RowChapter.CharHeight + 1;
        StyleChanged?.Invoke(this, System.EventArgs.Empty);
    }

    private void Remove(ColumnViewItem columnViewItem) {
        if (_internal.Remove(columnViewItem)) {
            //columnViewItem.Changed -= ColumnViewItem_Changed;
        }
    }

    #endregion
}