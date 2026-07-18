// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;

namespace BlueTable.Classes;

public sealed class ColumnViewCollection : IEnumerable<ColumnViewItem>, IParseable, ICloneable, IDisposableExtended, IHasTable, IReadableTextWithKey, IEditable {

    #region Fields

    private readonly List<ColumnViewItem> _internal = [];
    private readonly List<ColumnViewItem> _onDemand = [];
    private readonly List<string> _permissionGroups_show = [];
    private volatile int _isDisposedFlag;

    #endregion

    #region Constructors

    public ColumnViewCollection(Table? table, string toParse) {
        Table = table;
        KeyName = string.Empty;
        this.Parse(toParse);
    }

    public ColumnViewCollection(Table table, string toParse, string newname) : this(table, toParse) => KeyName = newname;

    #endregion

    #region Properties

    public bool Ansichtbearbeitung { get; set; }

    public ReadOnlyCollection<string> Ausführbare_Skripte { get; set; } = new List<string>().AsReadOnly();
    public string CaptionForEditor => "Spaltenanordnung";

    public ColumnItem? ColumnForChapter { get; set; }
    public ColumnHeaderMode ColumnHeaderMode { get; set; }
    public int Count => _internal.Count;

    public ReadOnlyCollection<string> Filter_immer_Anzeigen { get; set; } = new List<string>().AsReadOnly();

    public int FilterRows { get; set; } = 1;

    public bool Invalidated { get; set; } = true;

    public bool IsDisposed => _isDisposedFlag == 1;

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

    /// <summary>
    /// Alle für die Darstellung relevante Spalten: die persistenten
    /// (<see cref="_internal" />) in ihrer gespeicherten Reihenfolge plus die
    /// on-demand virtuellen (<see cref="_onDemand" />) an ihrer Standardposition
    /// (Pin vorne, Hinzufügen hinten). Editier-Operationen
    /// (Move/Remove/IndexOf) und die Serialisierung arbeiten weiterhin
    /// ausschließlich auf den persistenten Spalten.
    /// </summary>
    public IEnumerable<ColumnViewItem> RenderingItems {
        get {
            var pin = _onDemand.FirstOrDefault(v => v is PinColumnItem);
            var add = _onDemand.FirstOrDefault(v => v is AddColumnItem);

            IEnumerable<ColumnViewItem> result = _internal;
            if (add is not null) { result = result.Append(add); }
            if (pin is not null) { result = result.Prepend(pin); }
            return result;
        }
    }

    public ScaleToFitMode ScaleToFit { get; set; } = ScaleToFitMode.Normal;

    [DefaultValue(Win11)]
    public string SheetStyle { get; set; } = Win11;

    public bool ShowHead { get; set; } = true;

    /// <summary>
    /// Wenn true, klappt die TableView beim Umschalten auf diese Ansicht
    /// alle Kapitelüberschriften ein.
    /// </summary>
    public bool StartCollapsed { get; set; }

    public Table? Table {
        get;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == field) { return; }

            field?.DisposingEvent -= _table_Disposing;
            field = value;

            field?.DisposingEvent += _table_Disposing;
        }
    }

    #endregion

    #region Indexers

    public ColumnViewItem? this[int index] => index >= 0 && index < _internal.Count ? _internal[index] : null;

    public ColumnViewItem? this[ColumnItem? column] {
        get {
            if (column is null) { return null; }
            foreach (var thisViewItem in _internal) {
                if (thisViewItem is not null && thisViewItem.Column == column) { return thisViewItem; }
            }
            return null;
        }
    }

    #endregion

    #region Methods

    public static List<ColumnViewCollection> ParseAll(Table tb) {
        var tcvc = new List<ColumnViewCollection>();

        foreach (var item in tb.ColumnArrangements) {
            tcvc.Add(new ColumnViewCollection(tb, item.ParseableItems().FinishParseable()));
        }

        if (tcvc.Count < 2) { tcvc.Add(new ColumnViewCollection(tb, string.Empty)); }

        if (tcvc.Count < 2) { tcvc.Add(new ColumnViewCollection(tb, string.Empty)); }

        return tcvc;
    }

    public void Add(ColumnViewItem columnViewItem) => Add(columnViewItem, null);

    public void Add(ColumnViewItem columnViewItem, ColumnItem? insertAfterColumn) {
        columnViewItem.PropertyChanged += ColumnViewItem_PropertyChanged;

        var insertIndex = _internal.Count;
        if (insertAfterColumn is not null && this[insertAfterColumn] is { } afterViewItem) {
            insertIndex = _internal.IndexOf(afterViewItem) + 1;
        }

        _internal.Insert(insertIndex, columnViewItem);
        Invalidated = true;
    }

    public object Clone() => new ColumnViewCollection(Table, ParseableItems().FinishParseable());

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public ColumnViewItem? First() => _internal.Find(thisViewItem => thisViewItem?.Column is not null);

    public IEnumerator<ColumnViewItem> GetEnumerator() => ((IEnumerable<ColumnViewItem>)_internal).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_internal).GetEnumerator();

    public void HideSystemColumns() {
        foreach (var thisViewItem in this) {
            if (thisViewItem is not null && (thisViewItem.Column is null || thisViewItem.Column.IsSystemColumn())) {
                Remove(thisViewItem);
                HideSystemColumns();
                return;
            }
        }
    }

    public int IndexOf(ColumnViewItem? columnViewItem) => columnViewItem is null ? -1 : _internal.IndexOf(columnViewItem);

    public string IsNowEditable() {
        if (Table is not { IsDisposed: false } tb) { return "Tabelle verworfen"; }

        return tb.IsValueEditable(TableDataType.ColumnArrangement, string.Empty);
    }

    public ColumnViewItem? Last() => _internal.Last(thisViewItem => thisViewItem?.Column is not null);

    public List<ColumnItem> ListOfUsedColumn() {
        List<ColumnItem> colList = [];
        foreach (var t in _internal) {
            if (t?.Column is not null) { colList.Add(t.Column); }
        }
        return colList;
    }

    /// <summary>
    /// Verschiebt ein Element von <paramref name="oldIndex"/> nach <paramref name="newIndex"/>.
    /// Alle dazwischenliegenden Elemente werden entsprechend verschoben.
    /// </summary>
    public void Move(int oldIndex, int newIndex) {
        if (oldIndex < 0 || newIndex < 0 || oldIndex >= _internal.Count) { return; }
        if (newIndex > _internal.Count) { newIndex = _internal.Count; }
        if (oldIndex == newIndex) { return; }

        var item = _internal[oldIndex];
        _internal.RemoveAt(oldIndex);
        if (newIndex > oldIndex) { newIndex--; }
        _internal.Insert(newIndex, item);
        Invalidated = true;
    }

    public ColumnViewItem? NextVisible(ColumnViewItem? viewItem) {
        if (viewItem is null) { return null; }

        var viewItemNo = _internal.IndexOf(viewItem);

        if (viewItemNo < 0) { return null; }
        do {
            viewItemNo++;
            if (viewItemNo >= _internal.Count) { return null; }
            if (_internal[viewItemNo]?.Column is not null) { return _internal[viewItemNo]; }
        } while (true);
    }

    public List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [];

        result.ParseableAdd("Name", this as IHasKeyName);
        result.ParseableAdd("ShowHead", ShowHead);
        result.ParseableAdd("StartCollapsed", StartCollapsed);
        result.ParseableAdd("ScaleToFit", ScaleToFit);

        result.ParseableAdd("FilterRows", FilterRows);
        result.ParseableAdd("ChapterColumn", ColumnForChapter?.KeyName ?? string.Empty);
        result.ParseableAdd("QuickInfo", QuickInfo);
        result.ParseableAdd("ColumnHeaderMode", (int)ColumnHeaderMode);
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
        Invalidated = true;
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
                Add(ColumnViewItem.Create(Table, value.FromNonCritical()));
                return true;

            case "chaptercolumn":
                ColumnForChapter = Table?.Column[value.FromNonCritical()];
                return true;

            case "rowsortdefinition":
                return true;

            case "filter":
                return true;

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
                //if (value.GetAllTags() is { } x) {
                //    foreach (var pair2 in x) {
                //        AddParsed(ColumnViewItem.CreateFromParsed(Table, pair2.Value.FromNonCritical()));
                //    }
                //}
                return true;

            case "permissiongroup":
                _permissionGroups_show.Add(value);
                return true;

            case "permissiongroups":
                _permissionGroups_show.Clear();
                _permissionGroups_show.AddRange(value.FromNonCritical().SplitAndCutBy("|"));
                return true;

            case "showhead":
                ShowHead = value.FromPlusMinus();
                return true;

            case "startcollapsed":
                StartCollapsed = value.FromPlusMinus();
                return true;

            case "fillwidth":
                return true;

            case "scaletofit":
                ScaleToFit = (ScaleToFitMode)IntParse(value);
                return true;

            //case "vir_pin":
            //case "vir_number":
            //case "vir_add":
            //    if (ColumnViewItem.CreateVirtual(key) is { } v) {
            //        v.Parse(value.FromNonCritical());
            //        AddParsed(v);
            //    }
            //    return true;

            case "columnheadermode":
                ColumnHeaderMode = (ColumnHeaderMode)IntParse(value);
                return true;
        }

        return false;
    }

    public ColumnViewItem? PreviousVisible(ColumnViewItem? viewItem) {
        if (viewItem is null) { return null; }

        var viewItemNo = _internal.IndexOf(viewItem);

        do {
            viewItemNo--;
            if (viewItemNo < 0) { return null; }
            if (_internal[viewItemNo]?.Column is not null) { return _internal[viewItemNo]; }
        } while (true);
    }

    public string ReadableText() => KeyName;

    /// <summary>
    /// Stellt die on-demand virtuellen Spalten (Pin, Hinzufügen) gemäß den
    /// übergebenen Bedingungen sicherhaft ein. Persistente virtuelle Spalten
    /// (über ihren VIR_-Schlüssel in <see cref="_internal" />) werden NICHT
    /// angerührt — sie bleiben unabhängig vom Bedarf erhalten. On-demand-
    /// Spalten landen in <see cref="_onDemand" /> und werden nicht
    /// serialisiert. Die Number-Spalte hat keinen on-demand-Trigger und
    /// erscheint ausschließlich als VIR_NUMBER.
    /// </summary>
    public void ReconcileVirtualColumns(bool needPin, bool needAdd) {
        var changed = false;

        for (var z = _onDemand.Count - 1; z >= 0; z--) {
            var v = _onDemand[z];

            var want = v switch {
                PinColumnItem => needPin,
                AddColumnItem => needAdd,
                _ => false
            };

            if (!want) {
                v.PropertyChanged -= ColumnViewItem_PropertyChanged;
                _onDemand.RemoveAt(z);
                changed = true;
            }
        }

        if (needPin && !_internal.OfType<PinColumnItem>().Any() && !_onDemand.OfType<PinColumnItem>().Any()) {
            var p = new PinColumnItem {
                Permanent = true
            };
            p.PropertyChanged += ColumnViewItem_PropertyChanged;
            _onDemand.Add(p);
            changed = true;
        }

        if (needAdd && !_internal.OfType<AddColumnItem>().Any() && !_onDemand.OfType<AddColumnItem>().Any()) {
            var a = new AddColumnItem();
            a.PropertyChanged += ColumnViewItem_PropertyChanged;
            _onDemand.Add(a);
            changed = true;
        }

        if (changed) { Invalidated = true; }
    }

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

    public void Remove(ColumnViewItem? columnViewItem) {
        if (columnViewItem is null || !_internal.Contains(columnViewItem)) { return; }
        columnViewItem.PropertyChanged -= ColumnViewItem_PropertyChanged;
        _internal.Remove(columnViewItem);
        Invalidated = true;
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
            if (_internal[z]?.StorageKey is not null) { continue; }
            if (_internal[z]?.Column is null || !tb.Column.Contains(_internal[z]?.Column)) {
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
            if (this[thisColumn] is null && !thisColumn.IsDisposed && !thisColumn.IsSystemColumn()) {
                Add(new ColumnViewItem(thisColumn));
            }
        }

        foreach (var thisColumn in tb.Column) {
            if (this[thisColumn] is null && !thisColumn.IsDisposed && thisColumn.IsSystemColumn()) {
                Add(new ColumnViewItem(thisColumn));
            }
        }
    }

    public void ShowColumns(params string[] columnKeys) {
        if (IsDisposed || Table is not { IsDisposed: false }) { return; }

        foreach (var thisColumnKey in columnKeys) {
            var thisColumn = Table?.Column[thisColumnKey];

            if (thisColumn is not null && this[thisColumn] is null && !thisColumn.IsDisposed) {
                Add(new ColumnViewItem(thisColumn));
            }
        }
    }

    public void Swap(int index1, int index2) {
        if (index1 < 0 || index2 < 0 || index1 >= _internal.Count || index2 >= _internal.Count) { return; }
        if (index1 == index2) { return; }
        (_internal[index1], _internal[index2]) = (_internal[index2], _internal[index1]);
        Invalidated = true;
    }

    public QuickImage? SymbolForReadableText() => null;

    public override string ToString() => ParseableItems().FinishParseable();

    private void _table_Disposing(object? sender, System.EventArgs e) => Dispose();

    private void ColumnViewItem_PropertyChanged(object? sender, PropertyChangedEventArgs e) => Invalidated = true;

    private void Dispose(bool disposing) {
        if (Interlocked.CompareExchange(ref _isDisposedFlag, 1, 0) != 0) { return; }

        if (disposing) {
            Table = null;
            foreach (var item in _internal) {
                item.Dispose();
            }
            foreach (var item in _onDemand) {
                item.Dispose();
            }
        }
        _internal.Clear();
        _onDemand.Clear();
        Table = null;
    }

    #endregion
}