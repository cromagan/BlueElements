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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using BlueDatabase.Interfaces;

namespace BlueDatabase;

#nullable enable

public sealed class FilterCollection : IEnumerable<FilterItem>, IParseable, IHasDatabase, IDisposableExtended, IPropertyChangedFeedback, IReadableText, IEditable, IErrorCheckable {

    #region Fields

    private readonly List<FilterItem> _internal = [];

    //TODO: Kommentar wieder entfernen
    private string _coment;

    private Database? _database;

    private List<RowItem>? _rows;

    #endregion

    #region Constructors

    public FilterCollection(string c) : this(null as Database, c) { }

    public FilterCollection(Database? database, string coment) {
        Database = database;
        _coment = coment;
    }

    /// <summary>
    /// Erstellt einen Filter, der den Zeilenschl�ssel sucht. Mit der SplitColumn als zweiten Filter
    /// </summary>
    public FilterCollection(RowItem r, string coment) {
        _coment = coment;
        if (r.Database is not { } db) {
            Develop.DebugPrint(FehlerArt.Fehler, "Fehler im Filter");
            return;
        }

        Database = db;

        if (db.Column.SplitColumn is { } spc) {
            Add(new FilterItem(spc, FilterType.Istgleich, r.CellGetString(spc)));
        }

        Add(new FilterItem(db, FilterType.RowKey, r.KeyName));
    }

    public FilterCollection(FilterItem? fi, string coment) : this(fi?.Database, coment) {
        if (fi != null) { Add(fi); }
    }

    #endregion

    #region Destructors

    ~FilterCollection() { Dispose(disposing: false); }

    #endregion

    #region Events

    public event EventHandler? DisposingEvent;

    public event EventHandler? PropertyChanged;

    public event EventHandler? PropertyChanging;

    public event EventHandler? RowsChanged;

    #endregion

    #region Properties

    public string CaptionForEditor => "Filter-Sammlung";

    public int Count => IsDisposed ? 0 : _internal.Count;

    public Database? Database {
        get => _database;
        set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (_database == value) { return; }

            OnChanging();
            UnRegisterDatabaseEvents();

            _database = value;
            _internal.Clear();

            RegisterDatabaseEvents();

            Invalidate_FilteredRows();
            OnPropertyChanged();
        }
    }

    public Type? Editor { get; set; }

    public bool IsDisposed { get; private set; }

    public string RowFilterText {
        get {
            var f = this[null];
            return f?.SearchValue[0] ?? string.Empty;
        }
        set {
            if (IsDisposed || Database is not { IsDisposed: false }) { return; }

            if (this[null] is { } f) {
                if (f.SearchValue.Count == 1 && f.SearchValue[0] == value) { return; }
                Remove(f);
            }

            var fi = new FilterItem(Database, FilterType.Instr_UND_Gro�KleinEgal, value);
            Add(fi);
        }
    }

    public ReadOnlyCollection<RowItem> Rows {
        get {
            if (IsDisposed || Database is not { IsDisposed: false }) { return new List<RowItem>().AsReadOnly(); }
            _rows ??= CalculateFilteredRows(_database, _internal.ToArray());
            return _rows.AsReadOnly();
        }
    }

    public RowItem? RowSingleOrNull {
        get {
            if (IsDisposed || Database is not { IsDisposed: false }) { return null; }
            _rows ??= CalculateFilteredRows(_database, _internal.ToArray());
            return _rows.Count != 1 ? null : _rows[0];
        }
    }

    #endregion

    #region Indexers

    public FilterItem? this[ColumnItem? column] =>
        _internal.Where(thisFilterItem => thisFilterItem != null && thisFilterItem.IsOk())
        .FirstOrDefault(thisFilterItem => thisFilterItem.Column == column);

    public FilterItem? this[int no] {
        get {
            if (no < 0 || no >= _internal.Count) { return null; }
            return _internal[no];
        }
    }

    #endregion

    #region Methods

    public static List<RowItem> CalculateFilteredRows(Database? db, params FilterItem[] filter) {
        if (db == null || db.IsDisposed) { return []; }

        if (db.Column.SplitColumn is { } spc) {
            if (InitValue(spc, true, filter) is { } i) {
                var (_, ok) = db.BeSureRowIsLoaded(i, null);
                if (!ok) { return []; }
            }
        }

        List<RowItem> tmpVisibleRows = [];
        var lockMe = new object();
        try {
            _ = Parallel.ForEach(db.Row, thisRowItem => {
                if (thisRowItem != null) {
                    if (thisRowItem.MatchesTo(filter)) {
                        lock (lockMe) {
                            tmpVisibleRows.Add(thisRowItem);
                        }
                    }
                }
            });
        } catch {
            Develop.CheckStackForOverflow();
            return CalculateFilteredRows(db, filter);
        }

        return tmpVisibleRows;
    }

    /// <summary>
    /// Gibt den Wert zur�ck, der in eine neue Zeile reingeschrieben wird
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    public static string? InitValue(ColumnItem column, bool firstToo, params FilterItem[] filter) {
        if (filter == null || !filter.Any()) { return null; }
        if (column is not { IsDisposed: false }) { return null; }
        if (column.Database is not { IsDisposed: false } db) { return null; }

        if (column.Function is not ColumnFunction.Normal
                           and not ColumnFunction.First
                           and not ColumnFunction.Schl�sselspalte
                           and not ColumnFunction.RelationText
                           and not ColumnFunction.Werte_aus_anderer_Datenbank_als_DropDownItems
                           and not ColumnFunction.Split_Medium
                           and not ColumnFunction.Split_Large
                           and not ColumnFunction.Split_Name) { return null; }

        if (!firstToo && db.Column.First() == column) { return null; }

        if (column == db.Column.SysCorrect ||
            column == db.Column.SysRowChangeDate ||
            column == db.Column.SysRowChanger ||
            column == db.Column.SysRowCreator ||
            column == db.Column.SysRowCreateDate ||
            column == db.Column.SysLocked ||
            column == db.Column.SysRowState) { return null; }

        var fi = filter.Where(thisFilterItem => thisFilterItem != null && thisFilterItem.IsOk())
                              .FirstOrDefault(thisFilterItem => thisFilterItem.Column == column);

        if (fi is not {
            FilterType: not (not FilterType.Istgleich
                and not FilterType.Istgleich_Gro�KleinEgal
                and not FilterType.Istgleich_ODER_Gro�KleinEgal
                and not FilterType.Istgleich_UND_Gro�KleinEgal
                and not FilterType.Instr
                and not FilterType.Instr_Gro�KleinEgal
                and not FilterType.Instr_UND_Gro�KleinEgal
                and not FilterType.Istgleich_MultiRowIgnorieren
                and not FilterType.Istgleich_Gro�KleinEgal_MultiRowIgnorieren)
        }) { return null; }

        return column.AutoCorrect(fi.SearchValue.JoinWithCr(), false);
    }

    public void Add(FilterItem fi) {
        if (IsDisposed) { return; }

        OnChanging();

        AddInternal(fi);
        Invalidate_FilteredRows();
        OnPropertyChanged();
    }

    public void AddIfNotExists(FilterItem fi) {
        if (Exists(fi)) { return; }
        Add(fi);
    }

    public void AddIfNotExists(List<FilterItem> filterItems) {
        if (IsDisposed || !filterItems.Any()) { return; }

        var newItems = filterItems.Where(item => !Exists(item)).ToList();

        if (newItems.Any()) {
            OnChanging();
            AddInternal(newItems);
            Invalidate_FilteredRows();
            OnPropertyChanged();
        }
    }

    public void ChangeTo(FilterItem? fi) {
        if (fi != null && fi.Database == _database && _internal.Count == 1 && Exists(fi)) { return; }
        if (fi == null && _database == null && _internal.Count == 0) { return; }

        OnChanging();

        if (_database != fi?.Database) {
            UnRegisterDatabaseEvents();
            _database = fi?.Database;
            RegisterDatabaseEvents();
        }

        _internal.Clear();

        if (fi != null) {
            AddInternal(fi);
        }
        Invalidate_FilteredRows();

        OnPropertyChanged();
    }

    //public void AddIfNotExists(FilterCollection fc) => AddIfNotExists(fc.ToList());
    /// <summary>
    /// Effizente Methode um wenige Events auszul�sen
    /// </summary>
    /// <param name="fc"></param>
    public void ChangeTo(FilterCollection? fc) {
        if (!IsDifferentTo(fc)) { return; }

        OnChanging();

        UnRegisterDatabaseEvents();
        _database = fc?.Database;
        RegisterDatabaseEvents();

        _internal.Clear();

        if (fc != null) {
            var reallydifferent = fc.ParseableItems().FinishParseable() != ParseableItems().FinishParseable();
            foreach (var thisf in fc) {
                if (!Exists(thisf) && thisf.IsOk()) { AddInternal(thisf); }
            }

            if (reallydifferent) {
                if (fc._rows != null) {
                    _rows = [];
                    _rows.AddRange(fc.Rows);
                    OnRowsChanged();
                } else {
                    Invalidate_FilteredRows();
                }
            }
        } else {
            if (Count > 0) {
                Invalidate_FilteredRows();
            }
        }

        OnPropertyChanged();
    }

    public void Clear() {
        if (IsDisposed) { return; }
        if (_internal.Count == 0) { return; }

        OnChanging();
        _internal.Clear();
        Invalidate_FilteredRows();
        OnPropertyChanged();
    }

    /// <summary>
    /// Klont die Filter Collection. Auch alle Filter werden ein Klon. Vorberechnete Zeilen werden weitergegeben.
    /// </summary>
    /// <returns></returns>
    public object Clone(string c2) {
        var fc = new FilterCollection(Database, "Clone " + c2);

        fc.ChangeTo(this);

        return fc;
    }

    public bool Contains(FilterItem filter) => _internal.Contains(filter);

    public void Dispose() {
        // �ndern Sie diesen Code nicht. F�gen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public string ErrorReason() {
        foreach (var thisf in this) {
            var f = thisf.ErrorReason();

            if (!string.IsNullOrEmpty(f)) { return f; }

            if (_database != thisf.Database) {
                return "Filter haben unterschiedliche Datenbanken";
            }
        }

        return string.Empty;
    }

    IEnumerator IEnumerable.GetEnumerator() => _internal.GetEnumerator();

    public IEnumerator<FilterItem> GetEnumerator() => _internal.GetEnumerator();

    public bool HasAlwaysFalse() {
        foreach (var thisFi in this) {
            if (thisFi.FilterType == FilterType.AlwaysFalse) { return true; }
        }
        return false;
    }

    public void Invalidate_FilteredRows() {
        if (_rows == null) { return; }
        _rows = null;
        OnRowsChanged();
    }

    public bool IsDifferentTo(FilterCollection? fc) {
        if (IsDisposed) { return false; }
        if (fc == this) { return false; }

        if (fc is not { IsDisposed: false }) { return true; }

        if (fc.Count != Count) { return true; }

        if (_database != fc.Database) { return true; }

        foreach (var thisf in this) {
            if (!fc.Contains(thisf)) { return true; }
        }

        // Zweite Schleife obosolet, wenn alle vorhanden sind und Count gleich.

        return false;
    }

    public bool IsRowFilterActiv() {
        var fi = this[null];

        return fi is { FilterType: FilterType.Instr or FilterType.Instr_UND_Gro�KleinEgal or FilterType.Instr_Gro�KleinEgal };
    }

    public bool MayHaveRowFilter(ColumnItem? column) => column is { IgnoreAtRowFilter: false } && IsRowFilterActiv();

    public FilterCollection Normalized() {
        var tmp = new List<FilterItem>();

        foreach (var thisf in _internal) {
            tmp.Add(thisf.Normalized());
        }

        var fcn = new FilterCollection(Database, "Normalize");
        foreach (var thisf in tmp.OrderBy(e => e.ReadableText()).ToList()) {
            fcn.Add(thisf);
        }
        return fcn;
    }

    public void OnPropertyChanged() {
        if (IsDisposed) { return; }
        PropertyChanged?.Invoke(this, System.EventArgs.Empty);
    }

    public void OnRowsChanged() {
        if (IsDisposed) { return; }
        //if(_rows == null) { return;}
        RowsChanged?.Invoke(this, System.EventArgs.Empty);
    }

    public List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [];

        foreach (var thisFilterItem in _internal) {
            if (thisFilterItem is { } && thisFilterItem.IsOk()) {
                result.ParseableAdd("Filter", thisFilterItem as IStringable);
            }
        }
        return result;
    }

    public void ParseFinished(string parsed) { }

    public bool ParseThis(string key, string value) {
        switch (key) {
            case "filter":

                var fi = new FilterItem(value.FromNonCritical(), Database);
                if (!Exists(fi)) { AddInternal(fi); }

                return true;
        }
        return false;
    }

    public string ReadableText() {
        if (Count <= 0) { return "Kein Filter"; }

        var f = string.Empty;

        foreach (var thisf in _internal) {
            f += thisf.ReadableText() + "\r\n";
        }

        return f.TrimEnd("\r\n");
    }

    public void Remove(ColumnItem? column) {
        var toDel = _internal.Where(thisFilter => thisFilter.Column == column).ToList();
        if (toDel.Count == 0) { return; }
        RemoveRange(toDel);
    }

    public void Remove(FilterItem fi) {
        if (IsDisposed) { return; }
        if (!_internal.Contains(fi)) { return; }

        OnChanging();
        _internal.Remove(fi);
        Invalidate_FilteredRows();
        OnPropertyChanged();
    }

    public void Remove_RowFilter() => Remove(null as ColumnItem);

    public void RemoveOtherAndAdd(FilterItem fi, string newOrigin) {
        var fin = fi;
        if (fi.Origin != newOrigin) {
            fin = new FilterItem(fi.Database, fi.Column, fi.FilterType, fi.SearchValue, newOrigin);
        }

        RemoveOtherAndAdd(fin);
    }

    /// <summary>
    /// �ndert einen Filter mit der gleichen Spalte auf diesen Filter ab. Perfekt um so wenig Events wie m�glich auszul�sen
    /// </summary>
    public void RemoveOtherAndAdd(FilterItem? fi) {
        if (fi == null) { return; }
        if (IsDisposed) { return; }
        if (Exists(fi)) { return; }
        if (!fi.IsOk()) {
            Develop.DebugPrint(FehlerArt.Fehler, "Filter Fehler!");
            return;
        }

        var existingColumnFilter = _internal.Where(thisFilter => thisFilter.Column == fi.Column);

        OnChanging();
        foreach (var thisItem in existingColumnFilter) {
            _internal.Remove(thisItem);
        }
        AddInternal(fi);
        Invalidate_FilteredRows();
        OnPropertyChanged();
    }

    /// <summary>
    /// �ndert einen Filter mit der gleichen Spalte auf diesen Filter ab. Perfekt um so wenig Events wie m�glich auszul�sen
    /// </summary>
    public void RemoveOtherAndAdd(FilterCollection? fc, string newOrigin) {
        if (fc == null) { return; }

        foreach (var thisFi in fc) {
            RemoveOtherAndAdd(thisFi, newOrigin);
        }
    }

    public void RemoveRange(string origin) {
        var l = new List<FilterItem>();

        foreach (var thisItem in _internal) {
            if (thisItem.Origin.Equals(origin, StringComparison.OrdinalIgnoreCase)) {
                l.Add(thisItem);
            }
        }
        RemoveRange(l);
    }

    public void RemoveRange(List<FilterItem> fi) {
        if (IsDisposed) { return; }

        if (fi.Count == 0) { return; }

        var did = false;
        foreach (var thisItem in fi) {
            if (_internal.Contains(thisItem)) {
                if (!did) {
                    OnChanging();
                    did = true;
                }
                _internal.Remove(thisItem);
            }
        }

        if (did) {
            Invalidate_FilteredRows();
            OnPropertyChanged();
        }
    }

    public QuickImage SymbolForReadableText() {
        switch (Count) {
            case 0:
                return QuickImage.Get(ImageCode.Kreuz, 16);

            case 1:
                return QuickImage.Get(ImageCode.Trichter, 16);

            default:
                return QuickImage.Get(ImageCode.Trichter, 16, Color.Red, Color.Transparent);
        }
    }

    public List<FilterItem> ToList() => _internal;

    public override string ToString() => ParseableItems().FinishParseable();

    private void _Database_CellValueChanged(object sender, CellEventArgs e) {
        if (_rows == null) { return; }
        if (e.Row.IsDisposed || e.Column.IsDisposed) { return; }

        if (e.Row.MatchesTo(_internal.ToArray()) != _rows.Contains(e.Row)) {
            Invalidate_FilteredRows();
        }

        //if ((this[e.Column] != null) ||
        //     MayHasRowFilter(e.Column)
        // ) {
        //    Invalidate_FilteredRows();
        //}
    }

    private void _database_Disposing(object sender, System.EventArgs e) => Dispose();

    private void _database_Loaded(object sender, System.EventArgs e) {
        Invalidate_FilteredRows();
    }

    /// <summary>
    /// L�st keine Ereignisse aus
    /// </summary>
    /// <param name="fi"></param>
    private void AddInternal(FilterItem fi) {
        if (_internal.Count > 0) {
            foreach (var internalFilter in _internal) {
                if (fi.FilterType != FilterType.AlwaysFalse && internalFilter.FilterType != FilterType.AlwaysFalse) {
                    if (internalFilter.Database != fi.Database) {
                        fi = new FilterItem(null, "AddInternal");
                        break;
                    }
                }
            }
        }

        if (fi.Database != Database && fi.FilterType != FilterType.AlwaysFalse) {
            fi = new FilterItem(null, "AddInternal");
        }

        _internal.Add(fi);
        Invalidate_FilteredRows();
    }

    private void AddInternal(List<FilterItem> fi) {
        foreach (var thisfio in fi) {
            AddInternal(thisfio);
        }
    }

    private void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                // Verwaltete Ressourcen (Instanzen von Klassen, Lists, Tasks,...)
                OnDisposingEvent();
                Database = null;

                Invalidate_FilteredRows();

                //foreach (var thisf in _internal) {
                //    thisf.Dispose();
                //}
            }
            // Nicht verwaltete Ressourcen (Bitmap, Datenbankverbindungen, ...)
            _internal.Clear();
            IsDisposed = true;
        }
    }

    private bool Exists(FilterItem fi) {
        foreach (var thisFilter in _internal) {
            if (fi.Equals(thisFilter)) { return true; }
        }

        return false;
    }

    private void OnChanging() {
        if (IsDisposed) { return; }
        PropertyChanging?.Invoke(this, System.EventArgs.Empty);
    }

    private void OnDisposingEvent() => DisposingEvent?.Invoke(this, System.EventArgs.Empty);

    private void RegisterDatabaseEvents() {
        if (_database != null) {
            _database.Loaded += _database_Loaded;
            _database.DisposingEvent += _database_Disposing;
            _database.Row.RowRemoved += Row_RowRemoved;
            //_database.Row.RowRemoved += Row_RowRemoving;
            _database.Row.RowAdded += Row_Added;
            _database.Cell.CellValueChanged += _Database_CellValueChanged;
        }
    }

    private void Row_Added(object sender, RowEventArgs e) {
        if (IsDisposed || Database is not { IsDisposed: false }) { return; }
        if (_rows == null) { return; }
        if (e.Row.MatchesTo(_internal.ToArray())) { Invalidate_FilteredRows(); }
    }

    private void Row_RowRemoved(object sender, RowEventArgs e) {
        if (IsDisposed || Database is not { IsDisposed: false }) { return; }
        if (_rows == null) { return; }
        if (_rows.Contains(e.Row)) { Invalidate_FilteredRows(); }
    }

    private void UnRegisterDatabaseEvents() {
        if (_database != null) {
            _database.Loaded += _database_Loaded;
            _database.DisposingEvent -= _database_Disposing;
            _database.Row.RowRemoved -= Row_RowRemoved;
            _database.Row.RowAdded -= Row_Added;
            _database.Cell.CellValueChanged -= _Database_CellValueChanged;
        }
    }

    #endregion
}