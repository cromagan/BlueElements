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

public sealed class FilterCollection : ObservableCollection<FilterItem>, IParseable, IHasDatabase, IDisposableExtended, IChangedFeedback {

    #region Constructors

    public FilterCollection(DatabaseAbstract? database) {
        Database = database;
        if (Database != null && !Database.IsDisposed) {
            Database.Disposing += Database_Disposing;
        }
    }

    public FilterCollection(DatabaseAbstract? database, string toParse) : this(database) => Parse(toParse);

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
            if (Database == null || Database.IsDisposed) { return; }

            var f = this[null];
            if (f != null) {
                if (string.Equals(f.SearchValue[0], value, StringComparison.OrdinalIgnoreCase)) { return; }
                f.Changeto(f.FilterType, value);
                return;
            }

            FilterItem fi = new(Database, FilterType.Instr_UND_GroßKleinEgal, value);
            Add(fi);
        }
    }

    #endregion

    #region Indexers

    public FilterItem? this[ColumnItem? column] => this
        .Where(thisFilterItem => thisFilterItem != null && thisFilterItem.FilterType != FilterType.KeinFilter)
        .FirstOrDefault(thisFilterItem => thisFilterItem.Column == column);

    #endregion

    #region Methods

    public new void Add(FilterItem filter) {
        if (IsDisposed) { return; }
        filter.Changed += Filter_Changed;
        base.Add(filter);
        OnChanged();
    }

    public void Add(FilterType filterType, string filterBy) {
        if (Database == null || Database.IsDisposed) { return; }
        AddIfNotExists(new FilterItem(Database, filterType, filterBy));
    }

    public void Add(FilterType filterType, List<string> filterBy) {
        if (Database == null || Database.IsDisposed) { return; }
        AddIfNotExists(new FilterItem(Database, filterType, filterBy));
    }

    public void Add(string columnName, FilterType filterType, string filterBy) {
        if (Database == null || Database.IsDisposed) { return; }

        Add(Database?.Column.Exists(columnName), filterType, filterBy);
    }

    public void Add(string columnName, FilterType filterType, List<string> filterBy) {
        if (Database == null || Database.IsDisposed) { return; }
        Add(Database?.Column.Exists(columnName), filterType, filterBy);
    }

    public void Add(ColumnItem? column, FilterType filterType, List<string> filterBy) {
        if (column?.Database == null || column.Database.IsDisposed) { return; }

        AddIfNotExists(new FilterItem(column, filterType, filterBy));
    }

    public void Add(ColumnItem? column, FilterType filterType, string filterBy) {
        if (column?.Database == null || column.Database.IsDisposed) { return; }
        AddIfNotExists(new FilterItem(column, filterType, filterBy));
    }

    public void AddIfNotExists(FilterItem fi) {
        if (Exists(fi)) { return; }
        Add(fi);
    }

    public void AddIfNotExists(FilterCollection filterItem) {
        foreach (var thisFilter in filterItem) {
            AddIfNotExists(thisFilter);
        }
    }

    // Dieser Code wird hinzugefügt, um das Dispose-Muster richtig zu implementieren.
    public void Dispose() =>
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
        Dispose(true);// TODO: Auskommentierung der folgenden Zeile aufheben, wenn der Finalizer weiter oben überschrieben wird.//GC.SuppressFinalize(this);

    void IDisposable.Dispose() =>
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(disposing: true);//GC.SuppressFinalize(this);

    public bool Exists(FilterItem filterItem) {
        foreach (var thisFilter in this) {
            if (filterItem.Equals(thisFilter)) { return true; }
        }
        return false;
    }

    public bool IsRowFilterActiv() => this[null] != null;

    public bool MayHasRowFilter(ColumnItem? column) => column != null && !column.IgnoreAtRowFilter && IsRowFilterActiv();

    public void OnChanged() => Changed?.Invoke(this, System.EventArgs.Empty);

    public void Parse(string toParse) {
        // Initialize();
        foreach (var pair in toParse.GetAllTags()) {
            switch (pair.Key) {
                case "filter":
                    if (Database != null && !Database.IsDisposed) {
                        AddIfNotExists(new FilterItem(Database, pair.Value.FromNonCritical()));
                    }

                    break;

                default:
                    Develop.DebugPrint(FehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                    break;
            }
        }
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
    /// <param name="filterItem"></param>
    public new bool Remove(FilterItem filterItem) {
        filterItem.Changed -= Filter_Changed;
        return base.Remove(filterItem);
    }

    public void Remove(FilterItem filterItem, bool throwChangedEvent) {
        if (IsDisposed) { return; }
        if (Remove(filterItem)) {
            if (throwChangedEvent) {
                OnChanged();
            }
        }
    }

    public void Remove_RowFilter() => Remove(null as ColumnItem);

    public void RemoveOtherAndAddIfNotExists(ColumnItem column, FilterType filterType, string filterBy, string herkunft) => RemoveOtherAndAddIfNotExists(new(column, filterType, filterBy, herkunft));

    public void RemoveOtherAndAddIfNotExists(string columnName, FilterType filterType, string filterBy, string herkunft) {
        var column = Database?.Column.Exists(columnName);
        if (column == null || column.IsDisposed) { Develop.DebugPrint(FehlerArt.Fehler, "Spalte '" + columnName + "' nicht vorhanden."); return; }
        RemoveOtherAndAddIfNotExists(column, filterType, filterBy, herkunft);
    }

    public void RemoveOtherAndAddIfNotExists(FilterItem filterItem) {
        if (Exists(filterItem)) { return; }
        Remove(filterItem.Column);
        if (Exists(filterItem)) { return; } // Falls ein Event ausgelöst wurde, und es nun doch schon das ist
        Add(filterItem);
    }

    public void RemoveOtherAndAddIfNotExists(string columnName, FilterType filterType, List<string>? filterBy, string herkunft) {
        var column = Database?.Column.Exists(columnName);
        if (column == null || column.IsDisposed) { Develop.DebugPrint(FehlerArt.Fehler, "Spalte '" + columnName + "' nicht vorhanden."); return; }
        RemoveOtherAndAddIfNotExists(new(column, filterType, filterBy, herkunft));
    }

    public void RemoveRange(List<FilterItem> filter) {
        if (IsDisposed) { return; }
        var did = false;
        foreach (var thisItem in filter) {
            if (Contains(thisItem)) {
                did = true;

                Remove(thisItem, false);
            }
        }

        if (did) { OnChanged(); }
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

    private void Database_Disposing(object sender, System.EventArgs e) => Dispose();

    private void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                //base.Dispose(disposing);
                if (Database != null) {
                    Database.Disposing -= Database_Disposing;
                    Database = null;
                }
            }

            // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
            // TODO: Große Felder auf NULL setzen
            IsDisposed = true;
        }
    }

    private void Filter_Changed(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        OnChanged();
    }

    #endregion
}