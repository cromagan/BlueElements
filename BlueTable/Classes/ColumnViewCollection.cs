// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace BlueTable.Classes;

public sealed class ColumnViewCollection : IEnumerable<ColumnViewItem>, IParseable, ICloneable, IDisposableExtended, IHasTable, IReadableTextWithKey, IEditable, IFragmentStoreable {

    #region Enums

    /// <summary>
    /// Pfad- und Empfänger-Typen der Ansicht im binären Fragment-Format.
    /// Nur der Ansicht selbst bekannt.
    /// </summary>
    private enum ViewFragmentType : byte {

        /// <summary>
        /// Navigation in einen Ansichts-Eintrag (Key = ColumnName bzw. StorageKey).
        /// </summary>
        SubViewItem = 90,

        ShowHead = 1,

        StartCollapsed = 2,

        ChaptersUnique = 3,

        FilterRows = 4,

        ChapterColumn = 5,

        QuickInfo = 6,

        ColumnHeaderMode = 7,

        ScaleToFit = 8,

        PinAnzeigen = 9,

        AnsichtAnzeigen = 10,

        AehnlicheAnzeigen = 11,

        CollapseButtonAnzeigen = 12,

        ContextmenuScripts = 13,

        ExecuteableScripts = 14,

        ColumnsShowAlwaysFilter = 15,

        PermissionGroups = 16,

        /// <summary>
        /// Name der Ansicht; erster Satz des Blocks, die Empfänger-Fabrik baut das
        /// Objekt daraus.
        /// </summary>
        ViewName = 17
    }

    #endregion

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

    #region Events

    public event EventHandler? Disposed;

    #endregion

    #region Properties

    public bool Ansichtbearbeitung { get; set; }

    /// <summary>
    /// Ansichten-Button (gespeicherte Ansichten) in der Filterleiste anzeigen.
    /// </summary>
    public bool Ansicht_anzeigen {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnFragmentChanged(new FragmentChangedEventArgs(this, string.Empty, ViewFragmentType.AnsichtAnzeigen, value));
        }
    } = true;

    /// <summary>
    /// Ähnlichkeits-Spalte und Ähnlich-Button der Ähnlichen-Zeilen-Suche anzeigen.
    /// </summary>
    public bool Ähnliche_anzeigen {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnFragmentChanged(new FragmentChangedEventArgs(this, string.Empty, ViewFragmentType.AehnlicheAnzeigen, value));
        }
    } = true;

    public ReadOnlyCollection<string> Ausführbare_Skripte {
        get;
        set {
            if (!value.IsDifferentTo(field)) { return; }
            field = value;
            OnFragmentChanged(new FragmentChangedEventArgs(this, string.Empty, ViewFragmentType.ExecuteableScripts, value));
        }
    } = new List<string>().AsReadOnly();

    public string CaptionForEditor => "Spaltenanordnung";

    public ColumnItem? ColumnForChapter {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnFragmentChanged(new FragmentChangedEventArgs(this, string.Empty, ViewFragmentType.ChapterColumn, value?.KeyName ?? string.Empty));
        }
    }

    /// <summary>
    /// Kapitel einzigartig anzeigen: Kapitel werden zusammengefasst und nur einmal angezeigt.
    /// Ist false, erscheinen die Zeilen in der Reihenfolge der aktuellen Sortierung
    /// und Kapitelüberschriften können sich wiederholen.
    /// </summary>
    public bool ChaptersUnique {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnFragmentChanged(new FragmentChangedEventArgs(this, string.Empty, ViewFragmentType.ChaptersUnique, value));
        }
    } = true;

    /// <summary>
    /// Kapitel-Button (alle Kapitel auf-/zuklappen) in der Filterleiste anzeigen.
    /// </summary>
    public bool CollapseButton_anzeigen {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnFragmentChanged(new FragmentChangedEventArgs(this, string.Empty, ViewFragmentType.CollapseButtonAnzeigen, value));
        }
    } = true;

    public ColumnHeaderMode ColumnHeaderMode {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnFragmentChanged(new FragmentChangedEventArgs(this, string.Empty, ViewFragmentType.ColumnHeaderMode, (int)value));
        }
    }

    public int Count => _internal.Count;

    public ReadOnlyCollection<string> Filter_immer_Anzeigen {
        get;
        set {
            if (!value.IsDifferentTo(field)) { return; }
            field = value;
            OnFragmentChanged(new FragmentChangedEventArgs(this, string.Empty, ViewFragmentType.ColumnsShowAlwaysFilter, value));
        }
    } = new List<string>().AsReadOnly();

    public int FilterRows {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnFragmentChanged(new FragmentChangedEventArgs(this, string.Empty, ViewFragmentType.FilterRows, value));
        }
    } = 1;

    public bool Invalidated { get; set; } = true;

    public bool IsDisposed => _isDisposedFlag == 1;

    public string KeyName { get; set; }

    public ReadOnlyCollection<string> Kontextmenu_Skripte {
        get;
        set {
            if (!value.IsDifferentTo(field)) { return; }
            field = value;
            OnFragmentChanged(new FragmentChangedEventArgs(this, string.Empty, ViewFragmentType.ContextmenuScripts, value));
        }
    } = new List<string>().AsReadOnly();

    public ReadOnlyCollection<string> PermissionGroups_Show {
        get => new(_permissionGroups_show);
        set {
            var l = value.SortedDistinctList();

            if (l.IsDifferentTo(_permissionGroups_show)) {
                _permissionGroups_show.Clear();
                _permissionGroups_show.AddRange(l);
                OnFragmentChanged(new FragmentChangedEventArgs(this, string.Empty, ViewFragmentType.PermissionGroups, l));
            }
        }
    }

    /// <summary>
    /// Pin-Spalte und Pin-Buttons in der Filterleiste anzeigen.
    /// </summary>
    public bool Pin_anzeigen {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnFragmentChanged(new FragmentChangedEventArgs(this, string.Empty, ViewFragmentType.PinAnzeigen, value));
        }
    } = true;

    public string QuickInfo {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnFragmentChanged(new FragmentChangedEventArgs(this, string.Empty, ViewFragmentType.QuickInfo, value));
        }
    } = string.Empty;

    /// <summary>
    /// Alle für die Darstellung relevante Spalten: die persistenten
    /// (_internal) in ihrer gespeicherten Reihenfolge plus die
    /// on-demand virtuellen (_onDemand) an ihrer Standardposition
    /// (Pin vorne, Ähnlichkeits-Spalte direkt dahinter, Hinzufügen
    /// hinten). Editier-Operationen (Move/Remove/IndexOf) und die
    /// Serialisierung arbeiten weiterhin ausschließlich auf den
    /// persistenten Spalten.
    /// </summary>
    public IEnumerable<ColumnViewItem> RenderingItems {
        get {
            var pin = _onDemand.OfType<PinColumnItem>().FirstOrDefault();
            var add = _onDemand.OfType<AddColumnItem>().FirstOrDefault();
            var similarity = _onDemand.OfType<SimilarityColumnItem>().FirstOrDefault();

            IEnumerable<ColumnViewItem> result = _internal;
            if (add is not null) { result = result.Append(add); }
            if (similarity is not null) { result = result.Prepend(similarity); }
            if (pin is not null) { result = result.Prepend(pin); }
            return result;
        }
    }

    public ScaleToFitMode ScaleToFit {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnFragmentChanged(new FragmentChangedEventArgs(this, string.Empty, ViewFragmentType.ScaleToFit, (int)value));
        }
    } = ScaleToFitMode.Normal;

    [DefaultValue(Win11)]
    public string SheetStyle { get; set; } = Win11;

    public bool ShowHead {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnFragmentChanged(new FragmentChangedEventArgs(this, string.Empty, ViewFragmentType.ShowHead, value));
        }
    } = true;

    /// <summary>
    /// Wenn true, klappt die TableView beim Umschalten auf diese Ansicht
    /// alle Kapitelüberschriften ein.
    /// </summary>
    public bool StartCollapsed {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnFragmentChanged(new FragmentChangedEventArgs(this, string.Empty, ViewFragmentType.StartCollapsed, value));
        }
    }

    public Table? Table {
        get;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == field) { return; }

            field?.Disposed -= _table_Disposed;
            field = value;

            field?.Disposed += _table_Disposed;
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

    /// <summary>
    /// Liefert die Spalte mit dem angegebenen Namen (Spaltenschlüssel bzw.
    /// StorageKey virtueller Spalten), inklusive der on-demand Spalten.
    /// </summary>
    public ColumnViewItem? this[string? columnName] {
        get {
            if (string.IsNullOrEmpty(columnName)) { return null; }
            foreach (var thisViewItem in _internal.Concat(_onDemand)) {
                if (thisViewItem is not null && string.Equals(thisViewItem.ColumnName, columnName, StringComparison.OrdinalIgnoreCase)) { return thisViewItem; }
            }
            return null;
        }
    }

    #endregion

    #region Methods

    public static List<ColumnViewCollection> ParseAll(Table tb) {
        var tcvc = new List<ColumnViewCollection>();

        foreach (var item in tb.ColumnArrangements) {
            var c = new ColumnViewCollection(tb, item.ParseableItems().FinishParseable());
            // Persistente Hinzufügen-Spalten auflösen — sie werden nur on-demand geführt.
            foreach (var add in c.OfType<AddColumnItem>().ToList()) { c.Remove(add); }
            tcvc.Add(c);
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
        result.ParseableAdd("ChaptersUnique", ChaptersUnique);
        result.ParseableAdd("ScaleToFit", ScaleToFit);
        result.ParseableAdd("PinAnzeigen", Pin_anzeigen);
        result.ParseableAdd("AnsichtAnzeigen", Ansicht_anzeigen);
        result.ParseableAdd("AehnlicheAnzeigen", Ähnliche_anzeigen);
        result.ParseableAdd("CollapseButtonAnzeigen", CollapseButton_anzeigen);

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

    public bool ParseFinished() {
        if (FilterRows < 0) { FilterRows = 0; }
        if (FilterRows > 10) { FilterRows = 10; }
        Invalidated = true;
        return true;
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
                if (ColumnViewItem.Create(Table, value.FromNonCritical()) is { } cvi) { Add(cvi); }
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

            case "chaptersunique":
                ChaptersUnique = value.FromPlusMinus();
                return true;

            case "pinanzeigen":
                Pin_anzeigen = value.FromPlusMinus();
                return true;

            case "ansichtanzeigen":
                Ansicht_anzeigen = value.FromPlusMinus();
                return true;

            case "aehnlicheanzeigen":
                Ähnliche_anzeigen = value.FromPlusMinus();
                return true;

            case "collapsebuttonanzeigen":
                CollapseButton_anzeigen = value.FromPlusMinus();
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
    /// Stellt die on-demand virtuellen Spalten (Pin, Hinzufügen,
    /// Ähnlichkeits-Suche) gemäß den übergebenen Bedingungen sicherhaft ein.
    /// Die Hinzufügen-Spalte wird ausschließlich on-demand geführt —
    /// persistente Einträge in _internal werden entfernt, sobald sie nicht
    /// mehr benötigt werden. Die Pin-Spalte bleibt auch als persistenter
    /// Eintrag erhalten. On-demand-Spalten landen in _onDemand und werden
    /// nicht serialisiert. Die Number-Spalte hat keinen on-demand-Trigger
    /// und erscheint ausschließlich als VIR_NUMBER.
    /// </summary>
    public void ReconcileVirtualColumns(bool needPin, bool needAdd, bool needSimilarity) {
        var changed = false;

        for (var z = _onDemand.Count - 1; z >= 0; z--) {
            var v = _onDemand[z];

            var want = v switch {
                PinColumnItem => needPin,
                AddColumnItem => needAdd,
                SimilarityColumnItem => needSimilarity,
                _ => false
            };

            if (!want) {
                v.PropertyChanged -= ColumnViewItem_PropertyChanged;
                _onDemand.RemoveAt(z);
                changed = true;
            }
        }

        // Persistente Hinzufügen-Spalten ("Neue Spalte") auflösen — sie werden
        // ausschließlich on-demand bereitgestellt und dürfen nicht in der
        // Ansicht gespeichert werden.
        foreach (var add in _internal.OfType<AddColumnItem>().ToList()) {
            Remove(add);
            changed = true;
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

        if (needSimilarity && !_internal.OfType<SimilarityColumnItem>().Any() && !_onDemand.OfType<SimilarityColumnItem>().Any()) {
            var s = new SimilarityColumnItem();
            s.PropertyChanged += ColumnViewItem_PropertyChanged;
            _onDemand.Add(s);
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
            var thisViewItem = _internal[z];
            if (thisViewItem is null) {
                _internal.RemoveAt(z);
                z--;
                continue;
            }
            if (thisViewItem.StorageKey is not null) { continue; }
            if (thisViewItem.Column is { IsDisposed: false } && tb.Column.Contains(thisViewItem.Column)) { continue; }

            // Verwaisten Verweis zuerst neu auflösen: Ein kurzzeitig nicht
            // auflösbarer Schlüssel (Reload, Umbenennung) darf die Spalte
            // nicht dauerhaft aus der Ansicht entfernen.
            if (thisViewItem.RepairColumnReference()) { continue; }

            // Während eines Reloads sind gültige Schlüssel kurzzeitig nicht
            // auflösbar, weil die Spalten nacheinander neu eingespielt werden.
            // Der Eintrag bleibt erhalten und wird nach dem Reload bereinigt.
            if (tb.IsDataReloading) { continue; }

            _internal.Remove(thisViewItem);
            z--;
        }

        #endregion

        var tmp = Table.RepairUserGroups(PermissionGroups_Show);
        tmp.RemoveString(Administrator, false);

        switch (number) {
            case 0:
                if (string.IsNullOrEmpty(KeyName)) { KeyName = "Alle Spalten"; }
                // Ansicht 0 zeigt ausschließlich alle echten Spalten.
                // Virtuelle (persistente) Spalten — Pin, Hinzufügen —
                // sind hier nicht erlaubt und werden entfernt.
                for (var z = _internal.Count - 1; z >= 0; z--) {
                    if (_internal[z] is { StorageKey: not null } v) { Remove(v); }
                }
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

    private void _table_Disposed(object? sender, System.EventArgs e) => Dispose();

    private void ColumnViewItem_PropertyChanged(object? sender, PropertyChangedEventArgs e) => Invalidated = true;

    /// <summary>
    /// Liefert den Ansichts-Eintrag zum Schlüssel des ersten Blocksatzes
    /// (ColumnName bzw. StorageKey), inklusive der on-demand Spalten; fehlt er,
    /// wird er erzeugt und angehängt. Ein bereits vorhandener Eintrag wandert an
    /// seine Position gemäß Block — die Datei-Reihenfolge ist maßgeblich.
    /// </summary>
    private ColumnViewItem? GetViewItemCreateOrGet(FragmentRecord first) {
        var keyName = first.StringContent;

        if (this[keyName] is { } existing) {
            _ = _internal.Remove(existing);
            Add(existing);
            return existing;
        }

        ColumnViewItem? newItem = null;

        if (keyName.StartsWith("VIR_", StringComparison.OrdinalIgnoreCase)) {
            newItem = ParseableItem.NewByTypeName<ColumnViewItem>(keyName);
        } else if (Table?.Column[keyName] is { IsDisposed: false } column) {
            newItem = new ColumnViewItem(column);
        }

        if (newItem is null) { return null; }

        Add(newItem);
        return newItem;
    }

    private void Dispose(bool disposing) {
        if (Interlocked.CompareExchange(ref _isDisposedFlag, 1, 0) != 0) { return; }

        if (disposing) {
            OnDisposed();
            Disposed = null;
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

    private void OnDisposed() => Disposed?.Invoke(this, System.EventArgs.Empty);

    #endregion

    #region IFragmentStoreable

    /// <summary>
    /// Bubbling-Event für Fragment-Änderungen (Pendant zu OnPropertyChangedExt).
    /// </summary>
    public event EventHandler<FragmentChangedEventArgs>? FragmentChanged;


    /// <summary>
    /// Schreibt die Ansicht: eigene Werte nur wenn nicht im Standard, danach die
    /// persistenten Ansichts-Einträge als Sub*-Sätze (Key = ColumnName) mit
    /// eigenem Block eine Ebene tiefer.
    /// </summary>
    public bool SerializeCore(Stream stream, int depth) {
        if (IsDisposed) { return true; }

        var allok = true;

        // Der Name ist der erste Satz: Die Empfänger-Fabrik baut das Objekt daraus
        allok &= FragmentRecord.Append(stream, depth, ViewFragmentType.ViewName, KeyName);
        allok &= FragmentRecord.Append(stream, depth, ViewFragmentType.ShowHead, ShowHead);
        allok &= FragmentRecord.Append(stream, depth, ViewFragmentType.StartCollapsed, StartCollapsed);
        allok &= FragmentRecord.Append(stream, depth, ViewFragmentType.ChaptersUnique, ChaptersUnique);
        allok &= FragmentRecord.Append(stream, depth, ViewFragmentType.FilterRows, FilterRows);

        if (ColumnForChapter is { IsDisposed: false } chapter) {
            allok &= FragmentRecord.Append(stream, depth, ViewFragmentType.ChapterColumn, chapter.KeyName);
        }


        allok &= FragmentRecord.Append(stream, depth, ViewFragmentType.QuickInfo, QuickInfo); 
        allok &= FragmentRecord.Append(stream, depth, ViewFragmentType.ColumnHeaderMode, (int)ColumnHeaderMode);
        allok &= FragmentRecord.Append(stream, depth, ViewFragmentType.ScaleToFit, (int)ScaleToFit);
        allok &= FragmentRecord.Append(stream, depth, ViewFragmentType.PinAnzeigen, Pin_anzeigen);
        allok &= FragmentRecord.Append(stream, depth, ViewFragmentType.AnsichtAnzeigen, Ansicht_anzeigen);
        allok &= FragmentRecord.Append(stream, depth, ViewFragmentType.AehnlicheAnzeigen, Ähnliche_anzeigen);
        allok &= FragmentRecord.Append(stream, depth, ViewFragmentType.CollapseButtonAnzeigen, CollapseButton_anzeigen);
        allok &= FragmentRecord.Append(stream, depth, ViewFragmentType.ContextmenuScripts, Kontextmenu_Skripte);
        allok &= FragmentRecord.Append(stream, depth, ViewFragmentType.ExecuteableScripts, Ausführbare_Skripte);
        allok &= FragmentRecord.Append(stream, depth, ViewFragmentType.ColumnsShowAlwaysFilter, Filter_immer_Anzeigen);

        var tmp = PermissionGroups_Show.SortedDistinctList();
        tmp.RemoveString(Administrator, false);

        allok &= FragmentRecord.Append(stream, depth, ViewFragmentType.PermissionGroups, tmp);

        allok &= FragmentRecord.Append(stream, depth, ViewFragmentType.SubViewItem, _internal);

        return allok;
    }

    /// <summary>
    /// Verarbeitet den Ansicht-Block: eigene Werte im Switch, Ansichts-Einträge über
    /// den SubViewItem-Satz suchen bzw. erzeugen und deren Block eine Ebene tiefer lesen.
    /// </summary>
    public bool DeserializeThis(Stream stream, FragmentRecord record) {
        switch ((ViewFragmentType)record.LeafType) {
            case ViewFragmentType.ShowHead:
                ShowHead = record.BoolContent;
                break;

            case ViewFragmentType.StartCollapsed:
                StartCollapsed = record.BoolContent;
                break;

            case ViewFragmentType.ChaptersUnique:
                ChaptersUnique = record.BoolContent;
                break;

            case ViewFragmentType.FilterRows:
                FilterRows = record.IntContent;
                break;

            case ViewFragmentType.ChapterColumn:
                ColumnForChapter = Table?.Column[record.StringContent];
                break;

            case ViewFragmentType.ViewName:
                // Name steckt im Objekt; die Empfänger-Fabrik hat ihn übernommen
                break;

            case ViewFragmentType.QuickInfo:
                QuickInfo = record.StringContent;
                break;

            case ViewFragmentType.ColumnHeaderMode:
                ColumnHeaderMode = (ColumnHeaderMode)record.IntContent;
                break;

            case ViewFragmentType.ScaleToFit:
                ScaleToFit = (ScaleToFitMode)record.IntContent;
                break;

            case ViewFragmentType.PinAnzeigen:
                Pin_anzeigen = record.BoolContent;
                break;

            case ViewFragmentType.AnsichtAnzeigen:
                Ansicht_anzeigen = record.BoolContent;
                break;

            case ViewFragmentType.AehnlicheAnzeigen:
                Ähnliche_anzeigen = record.BoolContent;
                break;

            case ViewFragmentType.CollapseButtonAnzeigen:
                CollapseButton_anzeigen = record.BoolContent;
                break;

            case ViewFragmentType.ContextmenuScripts:
                Kontextmenu_Skripte = [.. record.ListContent];
                break;

            case ViewFragmentType.ExecuteableScripts:
                Ausführbare_Skripte = [.. record.ListContent];
                break;

            case ViewFragmentType.ColumnsShowAlwaysFilter:
                Filter_immer_Anzeigen = [.. record.ListContent];
                break;

            case ViewFragmentType.PermissionGroups:
                PermissionGroups_Show = [.. record.ListContent];
                break;

            case ViewFragmentType.SubViewItem:
                // Ansichts-Einträge als Liste: Items liegen eine Ebene unter dem
                // ListKeyObject-Satz, je Trenner leitet ein neues Item ein
                FragmentStoreableExtension.DeserializeList(stream, record.Depth, GetViewItemCreateOrGet);

                break;
            default:
                // Unbekannter Blatt-Typ — Satz überspringen
                break;
        }

        return true;
    }

    /// <summary>
    /// Löst das FragmentChanged-Event mit den fertigen Argumenten aus (Pendant zu OnPropertyChangedExt).
    /// </summary>
    public void OnFragmentChanged(FragmentChangedEventArgs e) {
        if (IsDisposed) { return; }

        FragmentChanged?.Invoke(this, e);
    }

    #endregion
}