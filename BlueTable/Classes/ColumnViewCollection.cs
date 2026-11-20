// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

namespace BlueTable.Classes;

/// <summary>
/// Ansichten-Sammlung einer Tabelle (Spaltenanordnung). Erbt Schlüssel-Ablage
/// und das Fragment-Gerüst von KeyedItemCollection; die
/// Reihenfolge der persistenten Einträge führt sie selbst (_internal), die
/// on-demand virtuellen Spalten liegen separat (_onDemand) und werden nicht
/// serialisiert.
/// </summary>
public sealed class ColumnViewCollection : KeyedItemCollection<ColumnViewItem>, IParseable, ICloneable, IHasTable, IReadableTextWithKey, IEditable {

    #region Enums

    /// <summary>
    /// Empfänger-Typen der Ansichts-Eigenschaften im binären Fragment-Format;
    /// Pfad-, Listen- und Journal-Blätter stammen aus dem geschützten Zahlraum
    /// der Basis (FragmentTagType ab 200).
    /// </summary>
    private enum ViewFragmentType : byte {

        ShowHead = 21,

        StartCollapsed = 22,

        ChaptersUnique = 23,

        FilterRows = 24,

        ChapterColumn = 25,

        QuickInfo = 26,

        ColumnHeaderMode = 27,

        ScaleToFit = 28,

        PinAnzeigen = 29,

        AnsichtAnzeigen = 30,

        AehnlicheAnzeigen = 31,

        CollapseButtonAnzeigen = 32,

        ContextmenuScripts = 33,

        ExecuteableScripts = 34,

        ColumnsShowAlwaysFilter = 35,

        PermissionGroups = 36,

        /// <summary>
        /// Name der Ansicht; erster Satz des Blocks, die Empfänger-Fabrik baut das
        /// Objekt daraus.
        /// </summary>
        ViewName = 37
    }

    #endregion

    /// <summary>
    /// Verständlicher Name eines Empfänger-Typs im Ansicht-Pfad: Der Listen- oder
    /// Nav-Satz (ItemPath) leitet den Zahlraum des Ansichts-Eintrags ein, sonst
    /// eigener Raum; unbekannte Werte liefern die Zahl.
    /// </summary>
    public static string GetNameOf(ReadOnlySpan<FragmentRecord> path) {
        if (path.Length == 1) { return ((FragmentTagType)path[0].ValueId).ToString(); }

        return path[0].TagType is FragmentTagType.ItemPath or FragmentTagType.ItemList
            ? ColumnViewItem.GetNameOf(path[1..])
            : ((ViewFragmentType)path[^1].LeafType).ToString();
    }

    #region Fields

    /// <summary>
    /// Reihenfolge-Träger der persistenten Einträge; parallel dazu liegt die
    /// Schlüssel-Ablage der Basis (Internal).
    /// </summary>
    private readonly List<ColumnViewItem> _internal = [];
    private readonly List<ColumnViewItem> _onDemand = [];
    private readonly List<string> _permissionGroups_show = [];

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

    /// <summary>
    /// Ansichten-Button (gespeicherte Ansichten) in der Filterleiste anzeigen.
    /// </summary>
    public bool Ansicht_anzeigen {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnFragmentChanged(new FragmentChangedEventArgs(this, ViewFragmentType.AnsichtAnzeigen, value));
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
            OnFragmentChanged(new FragmentChangedEventArgs(this, ViewFragmentType.AehnlicheAnzeigen, value));
        }
    } = true;

    public ReadOnlyCollection<string> Ausführbare_Skripte {
        get;
        set {
            if (!value.IsDifferentTo(field)) { return; }
            field = value;
            OnFragmentChanged(new FragmentChangedEventArgs(this, ViewFragmentType.ExecuteableScripts, value));
        }
    } = new List<string>().AsReadOnly();

    public string CaptionForEditor => "Spaltenanordnung";

    public ColumnItem? ColumnForChapter {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnFragmentChanged(new FragmentChangedEventArgs(this, ViewFragmentType.ChapterColumn, value?.KeyName ?? string.Empty));
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
            OnFragmentChanged(new FragmentChangedEventArgs(this, ViewFragmentType.ChaptersUnique, value));
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
            OnFragmentChanged(new FragmentChangedEventArgs(this, ViewFragmentType.CollapseButtonAnzeigen, value));
        }
    } = true;

    public ColumnHeaderMode ColumnHeaderMode {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnFragmentChanged(new FragmentChangedEventArgs(this, ViewFragmentType.ColumnHeaderMode, (int)value));
        }
    }

    public ReadOnlyCollection<string> Filter_immer_Anzeigen {
        get;
        set {
            if (!value.IsDifferentTo(field)) { return; }
            field = value;
            OnFragmentChanged(new FragmentChangedEventArgs(this, ViewFragmentType.ColumnsShowAlwaysFilter, value));
        }
    } = new List<string>().AsReadOnly();

    public int FilterRows {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnFragmentChanged(new FragmentChangedEventArgs(this, ViewFragmentType.FilterRows, value));
        }
    } = 1;

    public bool Invalidated { get; set; } = true;

    public string KeyName { get; set; }

    public ReadOnlyCollection<string> Kontextmenu_Skripte {
        get;
        set {
            if (!value.IsDifferentTo(field)) { return; }
            field = value;
            OnFragmentChanged(new FragmentChangedEventArgs(this, ViewFragmentType.ContextmenuScripts, value));
        }
    } = new List<string>().AsReadOnly();

    public ReadOnlyCollection<string> PermissionGroups_Show {
        get => new(_permissionGroups_show);
        set {
            var l = value.SortedDistinctList();

            if (l.IsDifferentTo(_permissionGroups_show)) {
                _permissionGroups_show.Clear();
                _permissionGroups_show.AddRange(l);
                OnFragmentChanged(new FragmentChangedEventArgs(this, ViewFragmentType.PermissionGroups, l));
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
            OnFragmentChanged(new FragmentChangedEventArgs(this, ViewFragmentType.PinAnzeigen, value));
        }
    } = true;

    public string QuickInfo {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnFragmentChanged(new FragmentChangedEventArgs(this, ViewFragmentType.QuickInfo, value));
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
            OnFragmentChanged(new FragmentChangedEventArgs(this, ViewFragmentType.ScaleToFit, (int)value));
        }
    } = ScaleToFitMode.Normal;

    [DefaultValue(Win11)]
    public string SheetStyle { get; set; } = Win11;

    public bool ShowHead {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnFragmentChanged(new FragmentChangedEventArgs(this, ViewFragmentType.ShowHead, value));
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
            OnFragmentChanged(new FragmentChangedEventArgs(this, ViewFragmentType.StartCollapsed, value));
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
    /// Liefert den Ansichts-Eintrag zum Schlüssel (ColumnName bzw. StorageKey),
    /// inklusive der on-demand Spalten.
    /// </summary>
    public override ColumnViewItem? GetByKey(string? keyName) {
        if (string.IsNullOrEmpty(keyName)) { return null; }

        foreach (var thisViewItem in _onDemand) {
            if (thisViewItem is not null && string.Equals(thisViewItem.ColumnName, keyName, StringComparison.OrdinalIgnoreCase)) { return thisViewItem; }
        }

        return base.GetByKey(keyName);
    }

    #endregion

    #region Methods

    public static List<ColumnViewCollection> ParseAll(Table tb) {
        var tcvc = new List<ColumnViewCollection>();

        foreach (var item in tb.ColumnArrangements) {
            var c = new ColumnViewCollection(tb, item.ParseableItems().FinishParseable());
            // Persistente Hinzufügen-Spalten auflösen — sie werden nur on-demand geführt.
            foreach (var add in c.OfType<AddColumnItem>().ToList()) { _ = c.RemoveCore(add, false); }
            tcvc.Add(c);
        }

        if (tcvc.Count < 2) { tcvc.Add(new ColumnViewCollection(tb, string.Empty)); }

        if (tcvc.Count < 2) { tcvc.Add(new ColumnViewCollection(tb, string.Empty)); }

        return tcvc;
    }

    /// <summary>
    /// Nimmt einen Ansichts-Eintrag ans Ende auf.
    /// </summary>
    public override bool Add(ColumnViewItem? columnViewItem) => Add(columnViewItem, null);

    /// <summary>
    /// Nimmt einen Ansichts-Eintrag auf: verdrahtet PropertyChanged und das
    /// FragmentChanged-Bubbling des Eintrags und hängt ihn hinter
    /// <paramref name="insertAfterColumn"/> ein; die Aufnahme wird über
    /// FragmentChanged gemeldet, sofern die Events nicht unterdrückt sind.
    /// </summary>
    public bool Add(ColumnViewItem? columnViewItem, ColumnItem? insertAfterColumn) {
        if (columnViewItem is null || IsDisposed) { return false; }

        // Bereits in der Ablage: nur fortführen, wenn es dieselbe Instanz ist (Neupositionierung beim Laden).
        if (Internal.TryGetValue(columnViewItem.KeyName, out var present) && !ReferenceEquals(present, columnViewItem)) { return false; }
        _ = Internal.TryAdd(columnViewItem.KeyName, columnViewItem);

        columnViewItem.PropertyChanged += ColumnViewItem_PropertyChanged;
        this.Reregister(columnViewItem);

        var insertIndex = _internal.Count;
        if (insertAfterColumn is not null && this[insertAfterColumn] is { } afterViewItem) {
            insertIndex = _internal.IndexOf(afterViewItem) + 1;
        }

        _internal.Insert(insertIndex, columnViewItem);
        Invalidated = true;

        var e = new FragmentChangedEventArgs(this, FragmentTagType.ItemAdd, columnViewItem)
            .WithNavigation(FragmentTagType.ItemPath, columnViewItem.KeyName);
        OnFragmentChanged(e);

        return true;
    }

    public object Clone() => new ColumnViewCollection(Table, ParseableItems().FinishParseable());

    public ColumnViewItem? First() => _internal.Find(thisViewItem => thisViewItem?.Column is not null);

    public override IEnumerator<ColumnViewItem> GetEnumerator() => ((IEnumerable<ColumnViewItem>)_internal).GetEnumerator();

    public void HideSystemColumns() {
        foreach (var thisViewItem in this) {
            if (thisViewItem is not null && (thisViewItem.Column is null || thisViewItem.Column.IsSystemColumn())) {
                _ = RemoveCore(thisViewItem, true);
                HideSystemColumns();
                return;
            }
        }
    }

    public int IndexOf(ColumnViewItem? columnViewItem) => columnViewItem is null ? -1 : _internal.IndexOf(columnViewItem);

    public override string IsNowEditable() {
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
                if (ColumnViewItem.Create(Table, value.FromNonCritical()) is { } cvi) {
                    SuppressEvents();
                    try {
                        _ = Add(cvi, null);
                    } finally {
                        ResumeEvents();
                    }
                }
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
            _ = RemoveCore(add, true);
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

    /// <summary>
    /// Entfernt einen Ansichts-Eintrag.
    /// </summary>
    public void Remove(ColumnViewItem? columnViewItem) {
        _ = RemoveCore(columnViewItem, true);
    }

    /// <summary>
    /// Entfernt den Ansichts-Eintrag zum Schlüssel; die Entfernung meldet den
    /// Journal-Block.
    /// </summary>
    public override bool Remove(string keyName) {
        if (IsDisposed || GetByKey(keyName) is not { } item) { return false; }

        return RemoveCore(item, true);
    }

    /// <summary>
    /// Entfernt einen Ansichts-Eintrag stillschweigend möglich (Systempfad);
    /// meldet die Entfernung als Journal-Block mit dem ItemPath-Navigationssatz,
    /// wenn <paramref name="fireEvent"/> gesetzt ist.
    /// </summary>
    private bool RemoveCore(ColumnViewItem? columnViewItem, bool fireEvent) {
        if (columnViewItem is null || !_internal.Contains(columnViewItem)) { return false; }

        columnViewItem.PropertyChanged -= ColumnViewItem_PropertyChanged;
        this.Unregister(columnViewItem);
        _ = Internal.TryRemove(columnViewItem.KeyName, out _);
        _ = _internal.Remove(columnViewItem);
        Invalidated = true;

        if (fireEvent) {
            var e = new FragmentChangedEventArgs(this, FragmentTagType.ItemRemove, string.Empty)
                .WithNavigation(FragmentTagType.ItemPath, columnViewItem.KeyName);
            OnFragmentChanged(e);
        }

        return true;
    }

    /// <summary>
    /// Entfernt alle Ansichts-Einträge.
    /// </summary>
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

            // Reparatur ist ein Systempfad: still entfernen, kein Journal-Eintrag
            _ = RemoveCore(thisViewItem, false);
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
                _ = Add(new ColumnViewItem(thisColumn), null);
            }
        }

        foreach (var thisColumn in tb.Column) {
            if (this[thisColumn] is null && !thisColumn.IsDisposed && thisColumn.IsSystemColumn()) {
                _ = Add(new ColumnViewItem(thisColumn), null);
            }
        }
    }

    public void ShowColumns(params string[] columnKeys) {
        if (IsDisposed || Table is not { IsDisposed: false }) { return; }

        foreach (var thisColumnKey in columnKeys) {
            var thisColumn = Table?.Column[thisColumnKey];

            if (thisColumn is not null && this[thisColumn] is null && !thisColumn.IsDisposed) {
                _ = Add(new ColumnViewItem(thisColumn), null);
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
    /// Übernimmt ein Journal-Add-Blatt: der Eintrag wird anhand des Schlüssels
    /// (Blatt-Inhalt bzw. Nav-Schlüssel) idempotent erzeugt und still angehängt;
    /// seine Eigenschaften tragen die Property-Blätter.
    /// </summary>
    protected override bool ApplyItemAdd(string keyName, string typeName) => GetOrCreateItem(keyName) is not null;

    /// <summary>
    /// Entfernt den benannten Ansichts-Eintrag still (Journal-Replay meldet nicht zurück).
    /// </summary>
    protected override bool ApplyItemRemove(string keyName) {
        if (GetByKey(keyName) is { } gone) { _ = RemoveCore(gone, false); }

        return true;
    }

    /// <summary>
    /// Liefert den Ansichts-Eintrag zum Schlüssel (ColumnName bzw. StorageKey),
    /// inklusive der on-demand Spalten; fehlt er, wird er erzeugt und still
    /// angehängt (Journal kann vor der Aufnahme eintreffen).
    /// </summary>
    protected override ColumnViewItem? GetOrCreateItem(string keyName) {
        if (GetByKey(keyName) is { } existing) { return existing; }

        ColumnViewItem? newItem = null;

        if (keyName.StartsWith("VIR_", StringComparison.OrdinalIgnoreCase)) {
            newItem = ParseableItem.NewByTypeName<ColumnViewItem>(keyName);
        } else if (Table?.Column[keyName] is { IsDisposed: false } column) {
            newItem = new ColumnViewItem(column);
        }

        if (newItem is null) { return null; }

        SuppressEvents();
        try {
            _ = Add(newItem, null);
        } finally {
            ResumeEvents();
        }

        return newItem;
    }

    /// <summary>
    /// Liefert den Ansichts-Eintrag zum Schlüssel des ersten Blocksatzes; ein
    /// bereits vorhandener Eintrag wandert an seine Position gemäß Block — die
    /// Datei-Reihenfolge ist maßgeblich.
    /// </summary>
    protected override ColumnViewItem? CreateItem(FragmentRecord first) {
        if (GetByKey(first.StringContent) is { } existing) {
            _ = _internal.Remove(existing);
            SuppressEvents();
            try {
                _ = Add(existing, null);
            } finally {
                ResumeEvents();
            }
            return existing;
        }

        return GetOrCreateItem(first.StringContent);
    }

    /// <summary>
    /// Räumt die Ansicht ab: on-demand Spalten verwerfen und die Tabelle lösen;
    /// die persistenten Einträge und die Events übernimmt die Basis.
    /// </summary>
    protected override void Dispose(bool disposing) {
        if (IsDisposed) { return; }

        if (disposing) {
            Table = null;
            foreach (var item in _onDemand) {
                item.Dispose();
            }
        }
        _onDemand.Clear();

        base.Dispose(disposing);
    }

    #endregion

    #region IFragmentStoreable

    /// <summary>
    /// Schreibt die Ansicht: eigene Werte nur wenn nicht im Standard, danach die
    /// persistenten Ansichts-Einträge als ItemList-Satz, je Eintrag ein eigener
    /// Block eine Ebene tiefer.
    /// </summary>
    public override bool SerializeCore(Stream stream, int depth) {
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

        allok &= FragmentRecord.Append(stream, depth, FragmentTagType.ItemList, _internal);

        return allok;
    }

    /// <summary>
    /// Verarbeitet die eigenen Werte im Switch; Nav-, Listen- und Journal-Sätze
    /// übernimmt das Gerüst der Basis.
    /// </summary>
    public override bool DeserializeThis(Stream stream, FragmentRecord record) {
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

            default:
                // Nav-, Listen- und übrige Sätze: das Gerüst der Basis
                return base.DeserializeThis(stream, record);
        }

        return true;
    }

    /// <summary>
    /// Übernimmt die Ansicht aus der Quelle: Die Property-Setter melden ihre Änderungen
    /// selbst, die Ansichts-Einträge werden abgeglichen — fehlende entfernt, neue
    /// aufgenommen (beides meldet die Journal-Ereignisse), gemeinsame per Eintrag-Update
    /// beschrieben. Die Spalten werden in der eigenen Tabelle aufgelöst.
    /// Auch schreibgeschützte Ansichten (Legacy-Live-Ansichten) werden beschrieben:
    /// Alle Aufrufer sind Systempfade (Einlesen, Journal, Reparatur, Tabellenkopie).
    /// </summary>
    public override string Update(IFragmentStoreable source) {
        if (IsDisposed) { return "Ansicht ist verworfen"; }
        if (source is not ColumnViewCollection sourceView) { return "Quelltyp ungültig"; }

        ShowHead = sourceView.ShowHead;
        StartCollapsed = sourceView.StartCollapsed;
        ChaptersUnique = sourceView.ChaptersUnique;
        FilterRows = sourceView.FilterRows;
        ColumnForChapter = sourceView.ColumnForChapter is { IsDisposed: false } chapter ? Table?.Column[chapter.KeyName] : null;
        QuickInfo = sourceView.QuickInfo;
        ColumnHeaderMode = sourceView.ColumnHeaderMode;
        ScaleToFit = sourceView.ScaleToFit;
        Pin_anzeigen = sourceView.Pin_anzeigen;
        Ansicht_anzeigen = sourceView.Ansicht_anzeigen;
        Ähnliche_anzeigen = sourceView.Ähnliche_anzeigen;
        CollapseButton_anzeigen = sourceView.CollapseButton_anzeigen;
        Kontextmenu_Skripte = sourceView.Kontextmenu_Skripte;
        Ausführbare_Skripte = sourceView.Ausführbare_Skripte;
        Filter_immer_Anzeigen = sourceView.Filter_immer_Anzeigen;
        PermissionGroups_Show = sourceView.PermissionGroups_Show;

        foreach (var thisItem in _internal.ToList()) {
            if (sourceView[thisItem.KeyName] is not null) { continue; }

            // Remove meldet die Entfernung selbst als Journal-Eintrag.
            Remove(thisItem);
        }

        foreach (var sourceItem in sourceView._internal) {
            if (sourceItem is not { IsDisposed: false }) { continue; }

            if (this[sourceItem.KeyName] is not { } existing) {
                if (ColumnViewItem.Create(Table, sourceItem.ParseableItems().FinishParseable()) is { } newItem) { _ = Add(newItem); }
            } else {
                var error = existing.Update(sourceItem);
                if (error is { Length: > 0 }) { return error; }
            }
        }

        return string.Empty;
    }

    #endregion
}
