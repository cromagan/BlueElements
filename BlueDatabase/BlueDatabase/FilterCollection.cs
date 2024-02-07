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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

public sealed class FilterCollection : IEnumerable<FilterItem>, IParseable, IHasDatabase, IDisposableExtended, IChangedFeedback {

    #region Fields

    private readonly List<FilterItem> _internal = [];

    //TODO: Komentar wieder entfernen
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

    public FilterCollection(FilterItem? fi, string coment) : this(fi?.Database, coment) {
        if (fi != null) { Add(fi); }
    }

    #endregion

    #region Destructors

    ~FilterCollection() { Dispose(disposing: false); }

    #endregion

    #region Events

    public event EventHandler? Changed;

    public event EventHandler? Changing;

    public event EventHandler? DisposingEvent;

    public event EventHandler? RowsChanged;

    #endregion

    #region Properties

    public int Count => IsDisposed ? 0 : _internal.Count;

    public Database? Database {
        get => _database;
        set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (_database == value) { return; }

            OnChanging();
            UnRegisterDatabaseEvents();

            _database = null;
            _internal.Clear();

            _database = value;

            RegisterDatabaseEvents();

            Invalidate_FilteredRows();
            OnChanged();
        }
    }

    public bool IsDisposed { get; private set; }

    public string RowFilterText {
        get {
            var f = this[null];
            return f?.SearchValue[0] ?? string.Empty;
        }
        set {
            if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }

            var f = this[null];
            if (f != null) {
                if (string.Equals(f.SearchValue[0], value, StringComparison.OrdinalIgnoreCase)) { return; }
                f.Changeto(f.FilterType, value);
                return;
            }

            var fi = new FilterItem(Database, FilterType.Instr_UND_GroßKleinEgal, value);
            Add(fi);
        }
    }

    public ReadOnlyCollection<RowItem> Rows {
        get {
            if (IsDisposed || Database is not Database db || db.IsDisposed) { return new List<RowItem>().AsReadOnly(); }
            _rows ??= CalculateFilteredRows();
            return _rows.AsReadOnly();
        }
    }

    public RowItem? RowSingleOrNull {
        get {
            if (IsDisposed || Database is not Database db || db.IsDisposed) { return null; }
            _rows ??= CalculateFilteredRows();
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
            return _internal[0];
        }
    }

    #endregion

    #region Methods

    public void Add(FilterItem fi) {
        if (IsDisposed) { return; }

        if (fi.Database != Database && fi.FilterType != FilterType.AlwaysFalse) { Develop.DebugPrint(FehlerArt.Fehler, "Filter Fehler!"); }
        if (!fi.IsOk()) { Develop.DebugPrint(FehlerArt.Fehler, "Filter Fehler!"); }

        OnChanging();

        AddAndRegisterEvents(fi);
        Invalidate_FilteredRows();
        OnChanged();
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
            AddAndRegisterEvents(newItems);
            Invalidate_FilteredRows();
            OnChanged();
        }
    }

    public void AddIfNotExists(FilterCollection fc) => AddIfNotExists(fc.ToList());

    public void ChangeTo(FilterItem? fi) {
        if (fi != null && fi.Database == _database && _internal.Count == 1 && Exists(fi)) { return; }
        if (fi == null && _database == null && _internal.Count == 0) { return; }

        OnChanging();

        UnRegisterDatabaseEvents();
        _database = fi?.Database;
        RegisterDatabaseEvents();

        UnRegisterEvents(_internal);
        _internal.Clear();

        if (fi != null) {
            AddAndRegisterEvents(fi);
        }
        Invalidate_FilteredRows();

        OnChanged();
    }

    /// <summary>
    /// Effizente Methode um wenige Events auszulösen
    /// </summary>
    /// <param name="fc"></param>
    public void ChangeTo(FilterCollection? fc) {
        if (!IsDifferentTo(fc)) { return; }

        OnChanging();

        UnRegisterDatabaseEvents();
        _database = fc?.Database;
        RegisterDatabaseEvents();

        UnRegisterEvents(_internal);
        _internal.Clear();

        if (fc != null) {
            foreach (var thisf in fc) {
                if (!Exists(thisf) && thisf.IsOk() && thisf.Clone() is FilterItem nfi) { AddAndRegisterEvents(nfi); }
            }

            _rows = [];
            _rows.AddRange(fc.Rows);
            OnRowsChanged();
        } else {
            Invalidate_FilteredRows();
        }

        OnChanged();
    }

    public void Clear() {
        if (IsDisposed) { return; }
        if (_internal.Count == 0) { return; }
        OnChanging();

        UnRegisterEvents(_internal);
        foreach (var thisF in _internal) {
            thisF.Dispose();
        }

        _internal.Clear();
        Invalidate_FilteredRows();
        OnChanged();
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
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    IEnumerator IEnumerable.GetEnumerator() => _internal.GetEnumerator();

    public IEnumerator<FilterItem> GetEnumerator() => _internal.GetEnumerator();

    public bool IsDifferentTo(FilterCollection? fc) {
        if (IsDisposed) { return false; }
        if (fc == this) { return false; }

        if (fc != null && fc.IsDisposed) { fc = null; }

        if (fc == null) { return true; }

        if (fc.Count != Count) { return true; }

        if (_database != fc.Database) { return true; }

        foreach (var thisf in this) {
            if (!fc.Contains(thisf)) { return true; }
        }

        // Zweite Schleife obosolet, wenn alle vorhanden sind und Count gleich.

        return false;
    }

    public bool IsRowFilterActiv() => this[null] != null;

    public bool MayHaveRowFilter(ColumnItem? column) => column != null && !column.IgnoreAtRowFilter && IsRowFilterActiv();

    public void OnChanged() {
        if (IsDisposed) { return; }
        Changed?.Invoke(this, System.EventArgs.Empty);
    }

    public void OnRowsChanged() {
        if (IsDisposed) { return; }
        //if(_rows == null) { return;}
        RowsChanged?.Invoke(this, System.EventArgs.Empty);
    }

    public void ParseFinished(string parsed) { }

    public bool ParseThis(string key, string value) {
        switch (key) {
            case "filter":

                var fi = new FilterItem(value.FromNonCritical(), Database);
                if (!Exists(fi)) { AddAndRegisterEvents(fi); }

                return true;
        }
        return false;
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
        UnRegisterEvents(fi);
        _internal.Remove(fi);
        Invalidate_FilteredRows();
        OnChanged();
    }

    public void Remove_RowFilter() => Remove(null as ColumnItem);

    public void RemoveOtherAndAdd(FilterItem fi, string newOrigin) {
        if (fi.Clone() is FilterItem fi2) {
            fi2.Origin = newOrigin;
            RemoveOtherAndAdd(fi2);
        }
    }

    /// <summary>
    /// Ändert einen Filter mit der gleichen Spalte auf diesen Filter ab. Perfekt um so wenig Events wie möglich auszulösen
    /// </summary>
    public void RemoveOtherAndAdd(FilterItem fi) {
        if (IsDisposed) { return; }
        if (Exists(fi)) { return; }
        if (!fi.IsOk()) {
            Develop.DebugPrint(FehlerArt.Fehler, "Filter Fehler!");
            return;
        }

        var existingColumnFilter = _internal.Where(thisFilter => thisFilter.Column == fi.Column).ToList();

        if (existingColumnFilter.Count == 0) {
            Add(fi);
            return;
        }

        if (existingColumnFilter.Count == 1) {
            existingColumnFilter[0].Changeto(fi.FilterType, fi.SearchValue);
            return;
        }

        OnChanging();
        foreach (var thisItem in existingColumnFilter) {
            UnRegisterEvents(thisItem);
            _internal.Remove(thisItem);
        }
        AddAndRegisterEvents(fi);
        Invalidate_FilteredRows();
        OnChanged();
    }

    /// <summary>
    /// Ändert einen Filter mit der gleichen Spalte auf diesen Filter ab. Perfekt um so wenig Events wie möglich auszulösen
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
                UnRegisterEvents(thisItem);
                _internal.Remove(thisItem);
            }
        }

        if (did) {
            Invalidate_FilteredRows();
            OnChanged();
        }
    }

    public List<FilterItem> ToList() => _internal;

    public override string ToString() {
        if (IsDisposed) { return string.Empty; }
        List<string> result = [];

        foreach (var thisFilterItem in _internal) {
            if (thisFilterItem != null && !thisFilterItem.IsDisposed && thisFilterItem.IsOk()) {
                result.ParseableAdd("Filter", thisFilterItem as IStringable);
            }
        }
        return result.Parseable();
    }

    private void _Database_CellValueChanged(object sender, CellChangedEventArgs e) {
        if (_rows == null) { return; }
        if (e.Row.IsDisposed || e.Column.IsDisposed) { return; }

        if (e.Row.MatchesTo(_internal) != _rows.Contains(e.Row)) {
            Invalidate_FilteredRows();
        }

        //if ((this[e.Column] != null) ||
        //     MayHasRowFilter(e.Column)
        // ) {
        //    Invalidate_FilteredRows();
        //}
    }

    private void _database_Disposing(object sender, System.EventArgs e) => Dispose();

    /// <summary>
    /// Löst keine Ereignisse aus
    /// </summary>
    /// <param name="fi"></param>
    private void AddAndRegisterEvents(FilterItem fi) {
        if (_internal.Count > 0 && _internal[0].Database != fi.Database) {
            Develop.DebugPrint(FehlerArt.Fehler, "Datenbanken unterschiedlich");
        }

        if (fi.Database != Database && fi.FilterType != FilterType.AlwaysFalse) { Develop.DebugPrint(FehlerArt.Fehler, "Filter Fehler!"); }

        if (fi.Parent != null) { Develop.DebugPrint(FehlerArt.Fehler, "Doppelte Filterverwendung!"); }
        fi.Parent = this;

        fi.Changing += Filter_Changing;
        fi.Changed += Filter_Changed;
        _internal.Add(fi);
        Invalidate_FilteredRows();
    }

    private void AddAndRegisterEvents(List<FilterItem> fi) {
        foreach (var thisfio in fi) {
            AddAndRegisterEvents(thisfio);
        }
    }

    private List<RowItem> CalculateFilteredRows() {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return []; }
        db.RefreshColumnsData(_internal);

        List<RowItem> tmpVisibleRows = [];
        var lockMe = new object();
        try {
            _ = Parallel.ForEach(Database.Row, thisRowItem => {
                if (thisRowItem != null) {
                    if (thisRowItem.MatchesTo(_internal)) {
                        lock (lockMe) {
                            tmpVisibleRows.Add(thisRowItem);
                        }
                    }
                }
            });
        } catch {
            Develop.CheckStackForOverflow();
            return CalculateFilteredRows();
        }

        return tmpVisibleRows;
    }

    private void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                // Verwaltete Ressourcen (Instanzen von Klassen, Lists, Tasks,...)
                OnDisposingEvent();
                Database = null;

                Invalidate_FilteredRows();

                foreach (var thisf in _internal) {
                    thisf.Dispose();
                }
                _internal.Clear();
            }

            // Nicht verwaltete Ressourcen (Bitmap, Datenbankverbindungen, ...)
            IsDisposed = true;
        }
    }

    private bool Exists(FilterItem fi) {
        foreach (var thisFilter in _internal) {
            if (fi.Equals(thisFilter)) { return true; }
        }

        return false;
    }

    private void Filter_Changed(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        Invalidate_FilteredRows();
        OnChanged();
    }

    private void Filter_Changing(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        OnChanging();
    }

    private void Invalidate_FilteredRows() {
        if (_rows == null) { return; }
        _rows = null;
        OnRowsChanged();
    }

    private void OnChanging() {
        if (IsDisposed) { return; }
        Changing?.Invoke(this, System.EventArgs.Empty);
    }

    private void OnDisposingEvent() => DisposingEvent?.Invoke(this, System.EventArgs.Empty);

    private void RegisterDatabaseEvents() {
        if (_database != null) {
            _database.DisposingEvent += _database_Disposing;
            _database.Row.RowRemoving += Row_RowRemoving;
            _database.Row.RowAdded += Row_Added;
            _database.Cell.CellValueChanged += _Database_CellValueChanged;
        }
    }

    private void Row_Added(object sender, RowChangedEventArgs e) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }
        if (_rows == null) { return; }
        if (e.Row.MatchesTo(_internal)) { Invalidate_FilteredRows(); }
    }

    private void Row_RowRemoving(object sender, RowEventArgs e) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }
        if (_rows == null) { return; }
        if (_rows.Contains(e.Row)) { Invalidate_FilteredRows(); }
    }

    private void UnRegisterDatabaseEvents() {
        if (_database != null) {
            _database.DisposingEvent -= _database_Disposing;
            _database.Row.RowRemoving -= Row_RowRemoving;
            _database.Row.RowAdded -= Row_Added;
            _database.Cell.CellValueChanged -= _Database_CellValueChanged;
        }
    }

    private void UnRegisterEvents(List<FilterItem> fi) {
        foreach (var thisfi in fi) {
            UnRegisterEvents(thisfi);
        }
    }

    private void UnRegisterEvents(FilterItem fi) {
        fi.Changing += Filter_Changing;
        fi.Changed += Filter_Changed;
    }

    #endregion
}