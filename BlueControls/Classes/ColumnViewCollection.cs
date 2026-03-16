// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueBasics;
using BlueBasics.Classes;
using BlueBasics.Interfaces;
using BlueControls.Interfaces;
using BlueTable.Classes;
using BlueTable.Enums;
using BlueTable.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using static BlueBasics.ClassesStatic.Constants;
using static BlueBasics.ClassesStatic.Converter;

namespace BlueControls.Classes;

public sealed class ColumnViewCollection : IEnumerable<ColumnViewItem>, IParseable, ICloneable, IDisposableExtended, IHasTable, IReadableTextWithKey, IEditable, IStyleable {

    #region Fields

    private readonly List<ColumnViewItem> _internal = [];
    private readonly List<string> _permissionGroups_show = [];

    private bool _invalidated = true;

    #endregion

    #region Constructors

    public ColumnViewCollection(Table? table, string toParse) {
        Table = table;
        KeyName = string.Empty;
        this.Parse(toParse);
    }

    public ColumnViewCollection(Table table, string toParse, string newname) : this(table, toParse) => KeyName = newname;

    #endregion

    #region Events

    public event EventHandler? StyleChanged;

    #endregion

    #region Properties

    public ReadOnlyCollection<string> Ausführbare_Skripte { get; set; } = new List<string>().AsReadOnly();
    public string CaptionForEditor => "Spaltenanordnung";

    public ColumnItem? ColumnForChapter { get; internal set; }

    /// <summary>
    /// Controll gibt an, dass es sich um Koordinten auf Controll ebene handel (nicht Canvas)
    /// Es muss mit Control-Koordinaten gearbeitet werden, da verschiedene Zoom-Stufen anderen Spaltenbreiten haben können
    /// </summary>
    public int ControlColumnsPermanentWidth { get; private set; }

    /// <summary>
    /// Controll gibt an, dass es sich um Koordinten auf Controll ebene handel (nicht Canvas)
    /// Es muss mit Control-Koordinaten gearbeitet werden, da verschiedene Zoom-Stufen anderen Spaltenbreiten haben können
    /// </summary>
    public int ControlColumnsWidth { get; private set; }

    public int Count => _internal.Count;
    public ReadOnlyCollection<string> Filter_immer_Anzeigen { get; set; } = new List<string>().AsReadOnly();
    public int FilterRows { get; internal set; } = 1;
    public bool IsDisposed { get; private set; }
    public bool KeyIsCaseSensitive => false;
    public string KeyName { get; set; }

    public ReadOnlyCollection<string> Kontextmenu_Skripte { get; set; } = new List<string>().AsReadOnly();

    public ReadOnlyCollection<string> PermissionGroups_Show {
        get => new(_permissionGroups_show);
        set {
            var l = value.SortedDistinctList();

            if (l.IsDifferentTo(_permissionGroups_show)) {
                _permissionGroups_show.Clear();
                _permissionGroups_show.AddRange(l);
            }
        }
    }

    public string QuickInfo { get; set; } = string.Empty;

    [DefaultValue(Win11)]
    public string SheetStyle {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            OnStyleChanged();
        }
    } = Win11;

    public bool ShowHead { get; set; } = true;

    public Table? Table {
        get;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == field) { return; }

            if (field != null) {
                field.DisposingEvent -= _table_Disposing;
            }
            field = value;

            if (field != null) {
                field.DisposingEvent += _table_Disposing;
            }
        }
    }

    #endregion

    #region Indexers

    public ColumnViewItem? this[int index] => index >= 0 && index < _internal.Count ? _internal[index] : null;

    public ColumnViewItem? this[ColumnItem? column] => column == null ? null : _internal.Find(thisViewItem => thisViewItem != null && thisViewItem.Column == column);

    #endregion

    #region Methods

    public static List<ColumnViewCollection> ParseAll(Table tb) {
        var tcvc = new List<ColumnViewCollection>();
        List<string> ca = [.. tb.ColumnArrangements.SplitAndCutByCr()];
        foreach (var t in ca) {
            tcvc.Add(new ColumnViewCollection(tb, t));
        }

        if (tcvc.Count < 2) { tcvc.Add(new ColumnViewCollection(tb, string.Empty)); }

        if (tcvc.Count < 2) { tcvc.Add(new ColumnViewCollection(tb, string.Empty)); }

        return tcvc;
    }

    public void Add(ColumnItem column) {
        if (column is not { IsDisposed: false }) { return; }

        Add(new ColumnViewItem(column));
    }

    public void Add(ColumnViewItem columnViewItem) {
        columnViewItem.PropertyChanged += ColumnViewItem_PropertyChanged;
        _internal.Add(columnViewItem);
    }

    public object Clone() => new ColumnViewCollection(Table, ParseableItems().FinishParseable());

    public void ComputeAllColumnPositions(int tableviewWith, float zoom) {
        if (!_invalidated) { return; }
        ControlColumnsPermanentWidth = 0;
        ControlColumnsWidth = 0;
        if (IsDisposed) { return; }

        _invalidated = false;
        var maxX = 0;

        foreach (var thisViewItem in this) {
            thisViewItem.ComputeLocation(this, maxX, tableviewWith, zoom);

            maxX = thisViewItem.ControlColumnRight(0);

            if (thisViewItem.Permanent) {
                ControlColumnsPermanentWidth = Math.Max(maxX, ControlColumnsPermanentWidth);
            }

            ControlColumnsWidth = Math.Max(maxX, ControlColumnsWidth);
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
        Table = null;
        //base.Dispose(disposing);
    }

    //NICHT IReadableText, das gibt zu viele Probleme (Dropdownboxen)
    public ColumnViewItem? First() => _internal.Find(thisViewItem => thisViewItem?.Column != null);

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

    public int IndexOf(ColumnViewItem? columnViewItem) => columnViewItem == null ? -1 : _internal.IndexOf(columnViewItem);

    public void Invalidate() => _invalidated = true;

    public string IsNowEditable() {
        if (Table is not { IsDisposed: false } tb) { return "Tabelle verworfen"; }

        return tb.GrantWriteAccess(TableDataType.ColumnArrangement, TableChunk.Chunk_Master);
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
        result.ParseableAdd("FilterRows", FilterRows);
        result.ParseableAdd("ChapterColumn", ColumnForChapter?.KeyName ?? string.Empty);
        result.ParseableAdd("QuickInfo", QuickInfo);
        result.ParseableAdd("Column", _internal);
        result.ParseableAdd("ContextmenuScripts", Kontextmenu_Skripte, true);
        result.ParseableAdd("ExecuteableScripts", Ausführbare_Skripte, true);
        result.ParseableAdd("ColumnsShowAlwaysFilter", Filter_immer_Anzeigen, true);

        var tmp = PermissionGroups_Show.SortedDistinctList();
        tmp.RemoveString(Administrator, false);
        result.ParseableAdd("Permissiongroups", tmp, true);

        return result;
    }

    public void ParseFinished(string parsed) {
        if (FilterRows < 0) { FilterRows = 0; }
        if (FilterRows > 10) { FilterRows = 10; }
    }

    public bool ParseThis(string key, string value) {
        switch (key) {
            case "name":
                KeyName = value;
                return true;

            case "filterrows":
                FilterRows = IntParse(value);
                return true;

            case "column":
            case "columndata":
                Add(new ColumnViewItem(Table, value.FromNonCritical())); // Base, um Events zu vermeiden
                return true;

            case "chaptercolumn":
                ColumnForChapter = Table?.Column[value.FromNonCritical()];
                return true;

            case "rowsortdefinition":
                return true; // TODO Entfernen: 18.01.2026

            case "filter":
                return true; // TODO Entfernen: 18.01.2026

            case "contextmenuscripts":
                Kontextmenu_Skripte = value.FromNonCritical().SplitAndCutBy("|").ToList().AsReadOnly();
                return true;

            case "executeablescripts":
                Ausführbare_Skripte = value.FromNonCritical().SplitAndCutBy("|").ToList().AsReadOnly();
                return true;

            case "columnsshowalwaysfilter":
                Filter_immer_Anzeigen = value.FromNonCritical().SplitAndCutBy("|").ToList().AsReadOnly();
                return true;

            case "quickinfo":
                QuickInfo = value.FromNonCritical();
                return true;

            case "columns":
                if (value.GetAllTags() is { } x) {
                    foreach (var pair2 in x) {
                        Add(new ColumnViewItem(Table, pair2.Value.FromNonCritical())); // BAse, um Events zu vermeiden
                    }
                }
                return true;

            case "permissiongroup":
                _permissionGroups_show.Add(value);
                return true;

            case "permissiongroups":
                _permissionGroups_show.Clear();
                _permissionGroups_show.AddRange(value.FromNonCritical().SplitByCr());
                return true;

            case "showhead":
                ShowHead = value.FromPlusMinus();
                return true;
        }

        return false;
    }

    public ColumnViewItem? PreviousVisible(ColumnViewItem? viewItem) {
        if (viewItem == null) { return null; }

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
                thiscv.IsExpanded = !columns.Contains(ci.KeyName);
            }
        }
    }

    public List<ColumnItem> ReducedColumns() {
        var x = new List<ColumnItem>();
        foreach (var thiscol in _internal) {
            if (thiscol is { Column: not null, IsExpanded: false }) { x.Add(thiscol.Column); }
        }
        return x;
    }

    public void RemoveAll() {
        List<ColumnViewItem> l = [.. _internal];

        foreach (var thiscol in l) {
            Remove(thiscol);
        }
    }

    public void Repair(int number) {
        if (Table is not { IsDisposed: false } tb) { return; }

        #region Ungültige Spalten entfernen

        for (var z = 0; z < _internal.Count; z++) {
            if (_internal[z]?.Column == null || !tb.Column.Contains(_internal[z]?.Column)) {
                _internal.Remove(_internal[z]);
                z--;
            }
        }

        #endregion

        var tmp = Table.RepairUserGroups(PermissionGroups_Show);
        tmp.RemoveString(Administrator, false);

        switch (number) {
            case 0:
                if (string.IsNullOrEmpty(KeyName)) { KeyName = "Alle Spalten"; }
                ShowAllColumns();
                break;

            case 1:
                if (string.IsNullOrEmpty(KeyName)) { KeyName = "Standard"; }
                tmp.AddIfNotExists(Everybody);
                break;
        }

        PermissionGroups_Show = tmp.AsReadOnly();

        if (string.IsNullOrEmpty(KeyName)) { KeyName = "Ansicht " + number; }
    }

    public void ShowAllColumns() {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        foreach (var thisColumn in tb.Column) {
            if (this[thisColumn] == null && !thisColumn.IsDisposed && !thisColumn.IsSystemColumn()) {
                Add(new ColumnViewItem(thisColumn));
            }
        }

        foreach (var thisColumn in tb.Column) {
            if (this[thisColumn] == null && !thisColumn.IsDisposed && thisColumn.IsSystemColumn()) {
                Add(new ColumnViewItem(thisColumn));
            }
        }
    }

    public void ShowColumns(params string[] columnnames) {
        if (IsDisposed || Table is not { IsDisposed: false }) { return; }

        foreach (var thisColumnName in columnnames) {
            var thisColumn = Table?.Column[thisColumnName];

            if (thisColumn != null && this[thisColumn] == null && this[thisColumn] == null && !thisColumn.IsDisposed) {
                Add(new ColumnViewItem(thisColumn));
            }
        }
    }

    public QuickImage? SymbolForReadableText() => null;

    //    if (_internal[index2].ViewType != ViewType.PermanentColumn) { _internal[index1].ViewType = ViewType.Column; }
    //}
    public override string ToString() => ParseableItems().FinishParseable();

    //    (_internal[index1], _internal[index2]) = (_internal[index2], _internal[index1]);
    private void _table_Disposing(object sender, System.EventArgs e) => Dispose();

    private void ColumnViewItem_PropertyChanged(object sender, PropertyChangedEventArgs e) => Invalidate();

    private void OnStyleChanged() {
        StyleChanged?.Invoke(this, System.EventArgs.Empty);
        Invalidate();
    }

    private void Remove(ColumnViewItem? columnViewItem) {
        if (columnViewItem == null || !_internal.Contains(columnViewItem)) { return; }
        columnViewItem.PropertyChanged -= ColumnViewItem_PropertyChanged;
        Invalidate();
        _internal.Remove(columnViewItem);
    }

    #endregion
}