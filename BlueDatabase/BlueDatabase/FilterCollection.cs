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
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using BlueDatabase.Interfaces;

namespace BlueDatabase;

public sealed class FilterCollection : ObservableCollection<FilterItem>, IParseable, IHasDatabase, IDisposableExtended, IChangedFeedback, ICloneable {

    #region Fields

    private List<RowItem>? _rows;

    #endregion

    #region Constructors

    public FilterCollection(DatabaseAbstract? database) {
        Database = database;
        if (Database != null && !Database.IsDisposed) {
            Database.Disposing += Database_Disposing;
            Database.Row.RowRemoving += Row_RowRemoving;
        }
    }

    public FilterCollection(DatabaseAbstract? database, string toParse) : this(database) => this.Parse(toParse);

    public FilterCollection(FilterItem fi) : this(fi.Database) => Add(fi);

    #endregion

    #region Events

    // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    // ~FilterCollection()
    // {
    //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
    //     Dispose(disposing: false);
    // }
    public event EventHandler? Changed;

    #endregion

    #region Properties

    public DatabaseAbstract? Database { get; private set; }

    public bool IsDisposed { get; private set; }

    public string RowFilterText {
        get {
            var f = this[null];
            return f?.SearchValue[0] ?? string.Empty;
        }
        set {
            if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }

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
            if (Database is not DatabaseAbstract db || db.IsDisposed) { return new List<RowItem>().AsReadOnly(); }
            if (_rows == null) { _rows = CalculateFilteredRows(); }
            return _rows.AsReadOnly();
        }
    }

    public RowItem? RowSingleOrNull {
        get {
            if (Database is not DatabaseAbstract db || db.IsDisposed) { return null; }
            if (_rows == null) { _rows = CalculateFilteredRows(); }
            if (_rows.Count != 1) { return null; }
            return _rows[0];
        }
    }

    #endregion

    #region Indexers

    public FilterItem? this[ColumnItem? column] => this
        .Where(thisFilterItem => thisFilterItem != null && thisFilterItem.FilterType != FilterType.KeinFilter)
        .FirstOrDefault(thisFilterItem => thisFilterItem.Column == column);

    #endregion

    #region Methods

    public new void Add(FilterItem fi) {
        if (IsDisposed) { return; }

        if (fi.Database != Database) { Develop.DebugPrint(FehlerArt.Fehler, "Filter Fehler!"); }

        fi.Changed += Filter_Changed;
        base.Add(fi);
        Invalidate_FilteredRows();
        OnChanged();
    }

    public void Add(FilterType filterType, string filterBy) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }
        AddIfNotExists(new FilterItem(Database, filterType, filterBy));
    }

    public void Add(FilterType filterType, List<string> filterBy) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }
        AddIfNotExists(new FilterItem(Database, filterType, filterBy));
    }

    public void Add(string columnName, FilterType filterType, string filterBy) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }

        Add(Database?.Column.Exists(columnName), filterType, filterBy);
    }

    public void Add(string columnName, FilterType filterType, List<string> filterBy) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }
        Add(Database?.Column.Exists(columnName), filterType, filterBy);
    }

    public void Add(ColumnItem? column, FilterType filterType, List<string> filterBy) {
        if (column?.Database is not DatabaseAbstract db || db.IsDisposed) { return; }

        AddIfNotExists(new FilterItem(column, filterType, filterBy));
    }

    public void Add(ColumnItem? column, FilterType filterType, string filterBy) {
        if (column?.Database is not DatabaseAbstract db || db.IsDisposed) { return; }
        AddIfNotExists(new FilterItem(column, filterType, filterBy));
    }

    public void AddIfNotExists(FilterItem fi) {
        if (Exists(fi)) { return; }
        Add(fi);
    }

    public void AddIfNotExists(List<FilterItem> fi) {
        foreach (var thisFilter in fi) {
            AddIfNotExists(thisFilter);
        }
    }

    public void AddIfNotExists(FilterCollection fc) {
        foreach (var thisFilter in fc) {
            AddIfNotExists(thisFilter);
        }
    }

    public object Clone() {
        var x = new FilterCollection(Database);
        foreach (var thisf in this) {
            x.Add(thisf);
        }

        return x;
    }

    // Dieser Code wird hinzugefügt, um das Dispose-Muster richtig zu implementieren.
    public void Dispose() =>
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
        Dispose(true);

    void IDisposable.Dispose() =>
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(disposing: true);

    public bool Exists(FilterItem fi) {
        foreach (var thisFilter in this) {
            if (fi.Equals(thisFilter)) { return true; }
        }

        return false;
    }

    public void Invalidate_FilteredRows() => _rows = null;

    //GC.SuppressFinalize(this);
    public bool IsRowFilterActiv() => this[null] != null;

    // TODO: Auskommentierung der folgenden Zeile aufheben, wenn der Finalizer weiter oben überschrieben wird.//GC.SuppressFinalize(this);
    public bool MayHasRowFilter(ColumnItem? column) => column != null && !column.IgnoreAtRowFilter && IsRowFilterActiv();

    public void OnChanged() => Changed?.Invoke(this, System.EventArgs.Empty);

    public void ParseFinished(string parsed) { }

    public bool ParseThis(string key, string value) {
        switch (key) {
            case "filter":
                if (Database != null && !Database.IsDisposed) {
                    AddIfNotExists(new FilterItem(Database, value.FromNonCritical()));
                }

                return true;
        }
        return false;
    }

    public void Remove(ColumnItem? column) {
        var toDel = this.Where(thisFilter => thisFilter.Column == column).ToList();
        if (toDel.Count == 0) { return; }
        RemoveRange(toDel);
    }

    public void Remove(string columnName) {
        var tmp = Database?.Column.Exists(columnName);
        if (tmp == null) { Develop.DebugPrint(FehlerArt.Fehler, "Spalte '" + columnName + "' nicht vorhanden."); }
        Remove(tmp);
    }

    /// <summary>
    /// Wirft niemals das Event Changed ab - CollectionChanged aber schon
    /// </summary>
    /// <param name="fi"></param>
    public new bool Remove(FilterItem fi) {
        fi.Changed -= Filter_Changed;
        return base.Remove(fi);
    }

    public void Remove(FilterItem fi, bool throwChangedEvent) {
        if (IsDisposed) { return; }
        if (Remove(fi)) {
            Invalidate_FilteredRows();
            if (throwChangedEvent) {
                OnChanged();
            }
        }
    }

    public void Remove_RowFilter() => Remove(null as ColumnItem);

    public void RemoveOtherAndAddIfNotExists(ColumnItem column, FilterType filterType, string filterBy, string herkunft) => RemoveOtherAndAddIfNotExists(new FilterItem(column, filterType, filterBy, herkunft));

    public void RemoveOtherAndAddIfNotExists(string columnName, FilterType filterType, string filterBy, string herkunft) {
        var column = Database?.Column.Exists(columnName);
        if (column == null || column.IsDisposed) { Develop.DebugPrint(FehlerArt.Fehler, "Spalte '" + columnName + "' nicht vorhanden."); return; }
        RemoveOtherAndAddIfNotExists(column, filterType, filterBy, herkunft);
    }

    public void RemoveOtherAndAddIfNotExists(FilterCollection? fc) {
        if (fc == null) { return; }

        foreach (var thisFi in fc) {
            RemoveOtherAndAddIfNotExists(thisFi);
        }
    }

    public void RemoveOtherAndAddIfNotExists(FilterItem fi) {
        if (Exists(fi)) { return; }
        Remove(fi.Column);
        if (Exists(fi)) { return; } // Falls ein Event ausgelöst wurde, und es nun doch schon das ist
        Add(fi);
    }

    public void RemoveOtherAndAddIfNotExists(string columnName, FilterType filterType, List<string>? filterBy, string herkunft) {
        var column = Database?.Column.Exists(columnName);
        if (column == null || column.IsDisposed) { Develop.DebugPrint(FehlerArt.Fehler, "Spalte '" + columnName + "' nicht vorhanden."); return; }
        RemoveOtherAndAddIfNotExists(new FilterItem(column, filterType, filterBy, herkunft));
    }

    public void RemoveRange(string herkunft) {
        var l = new List<FilterItem>();

        foreach (var thisItem in this) {
            if (thisItem.Herkunft.Equals(herkunft, StringComparison.OrdinalIgnoreCase)) {
                l.Add(thisItem);
            }
        }
        RemoveRange(l);
    }

    public void RemoveRange(List<FilterItem> fi) {
        if (IsDisposed) { return; }
        var did = false;
        foreach (var thisItem in fi) {
            if (Contains(thisItem)) {
                did = true;

                Remove(thisItem, false);
            }
        }

        if (did) {
            OnChanged();
        }
    }

    public override string ToString() {
        if (IsDisposed) { return string.Empty; }
        List<string> result = new();

        foreach (var thisFilterItem in this) {
            if (thisFilterItem != null && !thisFilterItem.IsDisposed && thisFilterItem.IsOk()) {
                result.ParseableAdd("Filter", thisFilterItem as IStringable);
            }
        }
        return result.Parseable();
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
        // Wegen Remove
        Invalidate_FilteredRows();
        base.OnCollectionChanged(e);
        OnChanged();
    }

    private List<RowItem> CalculateFilteredRows() {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return new List<RowItem>(); }
        db.RefreshColumnsData(this);

        List<RowItem> tmpVisibleRows = new();
        var lockMe = new object();
        try {
            _ = Parallel.ForEach(Database.Row, thisRowItem => {
                if (thisRowItem != null) {
                    if (thisRowItem.MatchesTo(this)) {
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

    private void Database_Disposing(object sender, System.EventArgs e) => Dispose();

    private void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                //base.Dispose(disposing);
                if (Database != null) {
                    Database.Disposing -= Database_Disposing;
                    Database.Row.RowRemoving -= Row_RowRemoving;
                    Database = null;
                }
                _rows = null;
            }

            // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
            // TODO: Große Felder auf NULL setzen
            IsDisposed = true;
        }
    }

    private void Filter_Changed(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        Invalidate_FilteredRows();
        OnChanged();
    }

    private void Row_RowRemoving(object sender, RowReasonEventArgs e) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }
        if (_rows == null) { return; }
        if (_rows.Contains(e.Row)) { _rows = null; }
    }

    #endregion
}