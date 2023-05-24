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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using BlueDatabase.Interfaces;

namespace BlueDatabase;

public sealed class RowCollection : IEnumerable<RowItem>, IDisposableExtended, IHasDatabase {

    #region Fields

    private readonly ConcurrentDictionary<long, RowItem> _internal = new();
    private readonly List<long> _pendingChangedBackgroundRow = new();
    private readonly List<long> _pendingChangedRows = new();
    private readonly List<BackgroundWorker> _pendingworker = new();
    private bool _executingbackgroundworks;
    private bool _executingchangedrows;
    private bool _throwEvents = true;

    #endregion

    #region Constructors

    public RowCollection(DatabaseAbstract database) {
        Database = database;
        Database.Disposing += Database_Disposing;
        //Initialize();
    }

    #endregion

    #region Destructors

    // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    ~RowCollection() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(false);
    }

    #endregion

    #region Events

    public event EventHandler<DoRowAutomaticEventArgs>? DoSpecialRules;

    public event EventHandler<RowEventArgs>? RowAdded;

    public event EventHandler<RowCheckedEventArgs>? RowChecked;

    public event EventHandler? RowRemoved;

    public event EventHandler<RowEventArgs>? RowRemoving;

    #endregion

    #region Properties

    public int Count => _internal.Count;

    public DatabaseAbstract? Database { get; private set; }

    public bool IsDisposed { get; private set; }

    public bool ThrowEvents {
        get => !IsDisposed && _throwEvents;
        set {
            if (_throwEvents == value) { Develop.DebugPrint(FehlerArt.Fehler, "Set ThrowEvents-Fehler! " + value.ToPlusMinus()); }
            _throwEvents = value;
        }
    }

    public long VisibleRowCount { get; private set; }

    #endregion

    #region Indexers

    public RowItem? this[string primärSchlüssel] {
        get {
            if (Database?.Column.First() is ColumnItem c) {
                return this[new FilterItem(c,
                    FilterType.Istgleich_GroßKleinEgal | FilterType.MultiRowIgnorieren,
                    primärSchlüssel)];
            }
            return null;
        }
    }

    /// <summary>
    /// Durchsucht die erste (interne) Spalte der Datenbank nach dem hier angegebenen Prmärschlüssel.
    /// </summary>
    /// <param name="primärSchlüssel">Der Primärschlüssel, nach dem gesucht werden soll. Groß/Kleinschreibung wird ignoriert.</param>
    /// <returns>Die Zeile, dessen erste Spalte den Primärschlüssel enthält oder - falls nicht gefunden - NULL.</returns>
    public RowItem? this[params FilterItem[] filter] {
        get {
            //if (filter == null || filter.Length == 0) {
            //    Develop.DebugPrint("Kein Filter angekommen!");
            //    return null;
            //}

            return _internal.Values.FirstOrDefault(thisRow => thisRow != null && thisRow.MatchesTo(filter));

            //FilterCollection d = new(filter[0].Database);
            //d.AddRange(filter);
            //return this[d];
        }
    }

    #endregion

    #region Methods

    public static string UniqueKeyValue() {
        var x = 9999;
        do {
            x += 1;
            if (x > 99999) { Develop.DebugPrint(FehlerArt.Fehler, "Unique ID konnte nicht erzeugt werden"); }

            var unique = ("X" + DateTime.UtcNow.ToString("mm.fff") + x.ToString(Constants.Format_Integer5)).RemoveChars(Constants.Char_DateiSonderZeichen + ".");
            var ok = true;

            foreach (var thisfile in DatabaseAbstract.AllFiles) {
                var row = thisfile.Row[unique];
                if (row != null && !row.IsDisposed) { ok = false; break; }
            }

            if (ok) { return unique; }
        } while (true);
    }

    public void AddRowWithChangedValue(long rowkey) => _ = _pendingChangedRows.AddIfNotExists(rowkey);

    /// <summary>
    /// Gibt einen Zeilenschlüssel zurück, der bei allen aktuell geladenen Datenbanken einzigartig ist.
    /// </summary>
    /// <returns></returns>
    public List<RowData> AllRows() {
        var sortedRows = new List<RowData>();
        foreach (var thisRowItem in this) {
            if (thisRowItem != null) {
                sortedRows.Add(new RowData(thisRowItem));
            }
        }
        return sortedRows;
    }

    public List<RowItem> CalculateFilteredRows(ICollection<FilterItem>? filter) {
        if (Database == null || Database.IsDisposed) { return new List<RowItem>(); }

        Database.RefreshColumnsData(filter);

        ConcurrentBag<RowItem> tmpVisibleRows = new();

        try {
            _ = Parallel.ForEach(Database.Row, thisRowItem => {
                if (thisRowItem != null) {
                    if (thisRowItem.MatchesTo(filter)) {
                        tmpVisibleRows.Add(thisRowItem);
                    }
                }
            });
        } catch {
            return CalculateFilteredRows(filter);
        }

        var l = new List<RowItem>();
        l.AddRange(tmpVisibleRows);
        return l;
    }

    /// <summary>
    /// Gibt die mit dieser Kombination sichtbaren Zeilen zurück. Ohne Sortierung. Jede Zeile kann maximal einmal vorkommen.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public List<RowData> CalculateSortedRows(ICollection<FilterItem>? filter, RowSortDefinition? rowSortDefinition, List<RowItem>? pinnedRows, List<RowData>? reUseMe) => CalculateSortedRows(CalculateFilteredRows(filter), rowSortDefinition, pinnedRows, reUseMe);

    public List<RowData> CalculateSortedRows(List<RowItem> filteredRows, RowSortDefinition? rowSortDefinition, List<RowItem>? pinnedRows, List<RowData>? reUseMe) {
        var db = Database;
        if (db == null || db.IsDisposed) { return new List<RowData>(); }

        VisibleRowCount = 0;

        #region Ermitteln, ob mindestens eine Überschrift vorhanden ist (capName)

        var capName = pinnedRows != null && pinnedRows.Count > 0;
        if (!capName) {
            foreach (var thisRow in filteredRows) {
                if (thisRow.Database != null && !thisRow.CellIsNullOrEmpty(thisRow.Database.Column.SysChapter)) {
                    capName = true;
                    break;
                }
            }
        }

        #endregion

        var l = new List<ColumnItem>();
        if (rowSortDefinition != null) { l.AddRange(rowSortDefinition.Columns); }
        if (db.Column.SysChapter != null) { _ = l.AddIfNotExists(db.Column.SysChapter); }

        db.RefreshColumnsData(l);

        #region _Angepinnten Zeilen erstellen (_pinnedData)

        List<RowData> pinnedData = new();
        var lockMe = new object();
        if (pinnedRows != null) {
            _ = Parallel.ForEach(pinnedRows, thisRow => {
                var rd = reUseMe.Get(thisRow, "Angepinnt") ?? new RowData(thisRow, "Angepinnt");
                rd.PinStateSortAddition = "1";
                rd.MarkYellow = true;
                rd.AdditionalSort = rowSortDefinition == null ? thisRow.CompareKey(null) : thisRow.CompareKey(rowSortDefinition.Columns);

                lock (lockMe) {
                    VisibleRowCount++;
                    pinnedData.Add(rd);
                }
            });
        }

        #endregion

        #region Gefiltere Zeilen erstellen (_rowData)

        List<RowData> rowData = new();
        _ = Parallel.ForEach(filteredRows, thisRow => {
            var adk = rowSortDefinition == null ? thisRow.CompareKey(null) : thisRow.CompareKey(rowSortDefinition.Columns);

            var markYellow = pinnedRows != null && pinnedRows.Contains(thisRow);
            var added = markYellow;

            var caps = thisRow.CellGetList(thisRow.Database?.Column.SysChapter);

            if (caps.Count > 0) {
                if (caps.Contains(string.Empty)) {
                    _ = caps.Remove(string.Empty);
                    caps.Add("-?-");
                }
            }

            if (caps.Count == 0 && capName) { caps.Add("Weitere Zeilen"); }
            if (caps.Count == 0) { caps.Add(string.Empty); }

            foreach (var thisCap in caps) {
                var rd = reUseMe.Get(thisRow, thisCap) ?? new RowData(thisRow, thisCap);

                rd.PinStateSortAddition = "2";
                rd.MarkYellow = markYellow;
                rd.AdditionalSort = adk;
                lock (lockMe) {
                    rowData.Add(rd);
                    if (!added) { VisibleRowCount++; added = true; }
                }
            }
        });

        #endregion

        pinnedData.Sort();
        rowData.Sort();

        if (rowSortDefinition != null && rowSortDefinition.Reverse) { rowData.Reverse(); }

        rowData.InsertRange(0, pinnedData);
        return rowData;
    }

    public List<RowItem> CalculateVisibleRows(ICollection<FilterItem>? filter, ICollection<RowItem>? pinnedRows) {
        List<RowItem> tmpVisibleRows = new();
        if (Database == null || Database.IsDisposed) { return tmpVisibleRows; }

        pinnedRows ??= new List<RowItem>();

        var lockMe = new object();
        _ = Parallel.ForEach(Database.Row, thisRowItem => {
            if (thisRowItem != null) {
                if (thisRowItem.MatchesTo(filter) || pinnedRows.Contains(thisRowItem)) {
                    lock (lockMe) { tmpVisibleRows.Add(thisRowItem); }
                }
            }
        });

        //foreach (var thisRowItem in Database.Row) {
        //    if (thisRowItem != null) {
        //        if (thisRowItem.MatchesTo(filter) || pinnedRows.Contains(thisRowItem)) {
        //            _tmpVisibleRows.GenerateAndAdd(thisRowItem);
        //        }
        //    }
        //}

        return tmpVisibleRows;
    }

    public bool Clear(string comment) => Remove(new FilterCollection(Database), null, comment);

    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void ExecuteExtraThread() {
        if (_pendingChangedBackgroundRow.Count == 0) { return; }
        if (Database == null || Database.IsDisposed) { return; }
        if (_executingbackgroundworks) { return; }
        _executingbackgroundworks = true;

        var ev = Database.EventScript.Get(EventTypes.value_changed_extra_thread);
        var ok = false;
        if (ev.Count == 1) { ok = ev[0].IsOk(); }

        if (!ok) {
            _pendingChangedBackgroundRow.Clear();
            _executingbackgroundworks = false;
            return;
        }

        try {
            while (_pendingworker.Count < 5) {
                if (Database == null || Database.IsDisposed || IsDisposed) { break; }
                if (!Database.LogUndo) { break; }
                if (_pendingChangedBackgroundRow.Count == 0) { break; }

                var key = _pendingChangedBackgroundRow.First();
                _ = _pendingChangedBackgroundRow.Remove(key);

                var r = SearchByKey(key);

                if (r != null && !r.IsDisposed) {
                    var l = new BackgroundWorker();
                    l.WorkerReportsProgress = true;
                    l.RunWorkerCompleted += PendingWorker_RunWorkerCompleted;
                    l.DoWork += PendingWorker_DoWork;

                    _pendingworker.Add(l);
                    l.RunWorkerAsync(key);

                    Database.OnDropMessage(FehlerArt.Info, "Hintergrund-Skript wird ausgeführt: " + r.CellFirstString());
                }
            }
        } catch { }

        _executingbackgroundworks = false;
    }

    public string ExecuteScript(EventTypes? eventname, string scriptname, FilterCollection? filter, List<RowItem>? pinned, bool fullCheck, bool changevalues) {
        var m = DatabaseAbstract.EditableErrorReason(Database, EditableErrorReason.EditGeneral);
        if (!string.IsNullOrEmpty(m) || Database == null) { return m; }

        var rows = CalculateVisibleRows(filter, pinned);
        if (rows.Count == 0) { return "Keine Zeilen angekommen."; }

        Database.RefreshRowData(rows, false, null);

        var txt = "Skript wird ausgeführt: " + scriptname;

        Database.OnProgressbarInfo(new ProgressbarEventArgs(txt, 0, rows.Count, true, false));

        var all = rows.Count;
        while (rows.Count > 0) {
            Database.OnProgressbarInfo(new ProgressbarEventArgs(txt, all - rows.Count, all, false, false));

            var scx = rows[0].ExecuteScript(eventname, scriptname, true, fullCheck, changevalues, 0);

            if (!scx.AllOk) {
                var w = rows[0].CellFirstString();
                rows.Clear();
                Database.OnProgressbarInfo(new ProgressbarEventArgs(txt, rows.Count, rows.Count, false, true));
                Database.OnDropMessage(FehlerArt.Warnung, "Skript fehlerhaft bei " + w);
                return "Skript fehlerhaft bei " + w + "\r\n" + scx.Protocol[0];
            }
            if (scx.AllOk) { rows.RemoveAt(0); }
        }
        Database.OnProgressbarInfo(new ProgressbarEventArgs(txt, rows.Count, rows.Count, false, true));
        return string.Empty;
    }

    public void ExecuteValueChanged() {
        if (_pendingChangedRows.Count == 0) { return; }
        if (_executingchangedrows) { return; }
        _executingchangedrows = true;

        while (_pendingChangedRows.Count > 0) {
            if (IsDisposed) { return; }

            try {
                var key = _pendingChangedRows[0];

                var r = SearchByKey(key);
                _pendingChangedRows.RemoveAt(0);

                if (r != null && !r.IsDisposed) {
                    _ = r.ExecuteScript(EventTypes.value_changed, string.Empty, true, true, true, 2);
                    r.InvalidateCheckData();
                    r.CheckRowDataIfNeeded();
                }

                _ = _pendingChangedRows.Remove(key); // Evtl.duch das Script erneut hinzugekommen.
                _pendingChangedBackgroundRow.Add(key);
            } catch { }
        }

        _executingchangedrows = false;
    }

    //                var x = dbl.RefreshRowData(key, false, null);
    //                if (!string.IsNullOrEmpty(x.Item2)) {
    //                    Database.OnDropMessage(FehlerArt.Fehler, x.Item2);
    //                    return x.Item2;
    //                }
    //            }
    //        }
    //    }
    //    return string.Empty;
    //}
    /// <summary>
    /// Füllt die Liste row auf, bis sie 100 Einträge enthält.
    /// </summary>
    /// <param name="row"></param>
    /// <param name="sortedRows"></param>
    /// <returns>Gibt false zurück, wenn ALLE Zeilen dadurch geladen sind.</returns>
    public bool FillUp100(List<RowItem> row, List<RowItem>? sortedRows) {
        if (row.Count is > 99 or 0) { return false; }

        if (Database is not DatabaseAbstract db) { return false; }
        if (db.IsDisposed) { return false; }

        sortedRows ??= new List<RowItem>();

        sortedRows.AddRange(db.Row.CalculateFilteredRows(null)); // ALLE Zeilen hinzufügen, nicht dass der Filter auf nein paar beschränkt ist und mehr laden könnte.

        if (sortedRows.Count == 0) { return false; } // Komisch, dürfte nie passieren

        var r = new List<RowItem>();
        r.AddRange(row);

        foreach (var thisRow in r) {
            var all = FillUp(row, thisRow, sortedRows, (100 / r.Count) + 1);
            if (all) {
                //DoLinkedDatabase(row);
                return true;
            }
        }
        //DoLinkedDatabase(row);
        return false;
    }

    //                    if (LongTryParse(s, out var v)) { key.Add(v); }
    //                }
    public RowItem? First() => _internal.Values.FirstOrDefault(thisRowItem => thisRowItem != null && !thisRowItem.IsDisposed);

    //                foreach (var thisRow in row) {
    //                    var s = Database.Cell.GetStringBehindLinkedValue(thisColumn, thisRow);
    /// <summary>
    ///
    /// </summary>
    /// <param name="key"></param>
    /// <param name="valueOfCellInFirstColumn"></param>
    /// <param name="runScriptOfNewRow"></param>
    /// <param name="fullprocessing">Sollen der Zeilenersteller, das Datum und die Initalwerte geschrieben werden?</param>
    /// <param name="comment"></param>
    /// <returns></returns>
    public RowItem? GenerateAndAdd(long key, string valueOfCellInFirstColumn, bool runScriptOfNewRow, bool fullprocessing, string comment) {
        var db = Database;
        if (db == null || db.IsDisposed) { return null; }

        var item = SearchByKey(key);
        if (item != null) {
            Develop.DebugPrint(FehlerArt.Fehler, "Schlüssel belegt!");
            return null;
        }
        _ = db.ChangeData(DatabaseDataType.Comand_AddRow, null, null, string.Empty, key.ToString(), comment);
        item = SearchByKey(key);
        if (item == null) {
            Develop.DebugPrint(FehlerArt.Fehler, "Erstellung fehlgeschlagen.");
            return null;
        }

        if (fullprocessing) {
            db.Cell.SystemSet(db.Column.SysRowCreator, item, Generic.UserName);
            db.Cell.SystemSet(db.Column.SysRowCreateDate, item, DateTime.UtcNow.ToString(Constants.Format_Date5));

            // Dann die Inital-Werte reinschreiben
            foreach (var thisColum in db.Column) {
                if (thisColum != null && !string.IsNullOrEmpty(thisColum.CellInitValue)) {
                    item.CellSet(thisColum, thisColum.CellInitValue);
                }
            }
        }

        if (!string.IsNullOrEmpty(valueOfCellInFirstColumn)) {
            item.CellSet(db.Column.First(), valueOfCellInFirstColumn);
        }

        if (runScriptOfNewRow) {
            _ = item.ExecuteScript(EventTypes.new_row, string.Empty, true, true, true, 0.1f);
        }

        return item;
    }

    //                var key = new List<long>();
    /// <summary>
    /// Erstellt eine neue Spalte mit den aus den Filterkriterien. nur Fiter IstGleich wird unterstützt.
    /// Schägt irgendetwas fehl, wird NULL zurückgegeben.
    /// Ist ein Filter mehrfach vorhanden, erhält die Zelle den LETZTEN Wert.
    /// Am Schluß wird noch das Skript ausgeführt.
    /// </summary>
    /// <param name="fi"></param>
    /// <param name="comment"></param>
    /// <returns></returns>
    public RowItem? GenerateAndAdd(List<FilterItem> fi, string comment) {
        if (Database == null || Database.IsDisposed) { return null; }

        IReadOnlyCollection<string>? first = null;

        foreach (var thisfi in fi) {
            if (Database.Column.First() == thisfi.Column) {
                if (first != null) { return null; }
                first = thisfi.SearchValue;
            }
            if (thisfi.FilterType != FilterType.Istgleich) { return null; }
            if (thisfi.Column == null) { return null; }
            if (thisfi.Column.Database != Database) { return null; }
        }

        if (first == null) { return null; }

        var row = GenerateAndAdd(NextRowKey(), first.JoinWithCr(), false, true, comment);

        if (row == null || row.IsDisposed) { return null; }

        foreach (var thisfi in fi) {
            row.CellSet(thisfi.Column, thisfi.SearchValue.ToList());
        }

        _ = row.ExecuteScript(EventTypes.new_row, string.Empty, false, false, true, 1);

        return row;
    }

    //    foreach (var thisColumn in Database.Column) {
    //        if (thisColumn.LinkedDatabase is DatabaseAbstract dbl) {
    //            if (!done.Contains(dbl)) {
    //                done.Add(dbl);
    public RowItem? GenerateAndAdd(string valueOfCellInFirstColumn, string comment) => GenerateAndAdd(NextRowKey(), valueOfCellInFirstColumn, true, true, comment);

    //    List<DatabaseAbstract> done = new();
    public IEnumerator<RowItem> GetEnumerator() => _internal.Values.GetEnumerator();

    //    if (Database is not DatabaseAbstract db) { return "Verlinkung zur Datenbank fehlhlerhaft"; }
    //    if (db.IsDisposed) { return "Datenbank verworfen"; }
    //foreach (var ThisRowItem in _Internal.Values)//{//    if (ThisRowItem != null) { return ThisRowItem; }//}//return null;
    IEnumerator IEnumerable.GetEnumerator() => IEnumerable_GetEnumerator();

    public bool HasPendingWorker() {
        if (_pendingworker.Count > 0) { return true; }
        return false;
    }

    //public string DoLinkedDatabase(List<RowItem> row) {
    //    if (row.Count == 0) { return string.Empty; }
    public void InvalidateAllCheckData() {
        foreach (var thisRow in this) {
            thisRow.InvalidateCheckData();
        }
    }

    public bool Remove(long key, string comment) {
        var r = SearchByKey(key);

        if (r == null || r.IsDisposed) { return false; }
        return string.IsNullOrEmpty(Database?.ChangeData(DatabaseDataType.Comand_RemoveRow, null, r, string.Empty, key.ToString(), comment));
    }

    public bool Remove(FilterItem filter, List<RowItem>? pinned, string comment) {
        FilterCollection nf = new(Database) { filter };
        return Remove(nf, pinned, comment);
    }

    public bool Remove(FilterCollection? filter, List<RowItem>? pinned, string comment) {
        var keys = (from thisrowitem in _internal.Values where thisrowitem != null && thisrowitem.MatchesTo(filter) select thisrowitem.Key).Select(dummy => dummy).ToList();
        var did = false;

        foreach (var thisKey in keys) {
            if (Remove(thisKey, comment)) { did = true; }
        }

        if (pinned != null && pinned.Count > 0) {
            foreach (var thisr in pinned) {
                if (Remove(thisr.Key, comment)) { did = true; }
            }
        }

        return did;
    }

    public bool Remove(RowItem? row, string comment) => row != null && Remove(row.Key, comment);

    public bool RemoveOlderThan(float inHours, string comment) {
        var x = (from thisrowitem in _internal.Values where thisrowitem != null let d = thisrowitem.CellGetDateTime(Database?.Column.SysRowCreateDate) where DateTime.Now.Subtract(d).TotalHours > inHours select thisrowitem.Key).Select(dummy => dummy).ToList();
        //foreach (var thisrowitem in _Internal.Values)
        //{
        //    if (thisrowitem != null)
        //    {
        //        var D = thisrowitem.CellGetDateTime(Database.Column.SysRowCreateDate());
        //        if (DateTime.Now.Subtract(D).TotalHours > InHours) { x.GenerateAndAdd(thisrowitem.Key); }
        //    }
        //}
        if (x.Count == 0) { return false; }
        foreach (var thisKey in x) {
            _ = Remove(thisKey, comment);
        }
        return true;
    }

    public RowItem? SearchByKey(long? key) {
        if (Database == null || Database.IsDisposed || key == null || key < 0) { return null; }
        try {
            var r = _internal.ContainsKey((long)key) ? _internal[(long)key] : null;
            if (r != null && r.IsDisposed) {
                Develop.DebugPrint(FehlerArt.Fehler, "Interner Zeilenfehler: " + key);
                return null;
            }

            return r;
        } catch {
            return SearchByKey(key);
        }
    }

    internal static List<RowItem> MatchesTo(FilterItem filterItem) {
        List<RowItem> l = new();

        if (filterItem.Database == null) { return l; }

        l.AddRange(filterItem.Database.Row.Where(thisRow => thisRow.MatchesTo(filterItem)));
        return l;
    }

    internal static List<RowItem> MatchesTo(List<FilterItem>? filterItem) {
        List<RowItem> l = new();

        if (filterItem == null || filterItem.Count < 1) { return l; }
        var db = filterItem[0].Database;
        if (db == null || db.IsDisposed) { return l; }

        l.AddRange(db.Row.Where(thisRow => thisRow.MatchesTo(filterItem)));
        return l;
    }

    internal void CloneFrom(DatabaseAbstract sourceDatabase) {
        // Zeilen, die zu viel sind, löschen
        foreach (var thisRow in this) {
            var l = sourceDatabase.Row.SearchByKey(thisRow.Key);
            if (l == null) { _ = Remove(thisRow, "Clone - Zeile zuviel"); }
        }

        // Zeilen erzeugen und Format übertragen
        foreach (var thisRow in sourceDatabase.Row) {
            var l = SearchByKey(thisRow.Key) ?? GenerateAndAdd(thisRow.Key, string.Empty, false, false, "Clone - Zeile fehlt");
            l?.CloneFrom(thisRow, true);
        }

        if (sourceDatabase.Row.Count != Count) {
            Develop.DebugPrint(FehlerArt.Fehler, "Clone Fehlgeschlagen");
        }
    }

    internal bool NeedDataCheck(long key) => _pendingChangedRows.Contains(key);

    //        default:
    //            if (type.ToString(false) == ((int)type).ToString(false)) {
    //                Develop.DebugPrint(enFehlerArt.Info, "Laden von Datentyp '" + type + "' nicht definiert.<br>Wert: " + value + "<br>Datei: " + Database.Filename);
    //            } else {
    //                return "Interner Fehler: Für den Datentyp  '" + type + "'  wurde keine Laderegel definiert.";
    //            }
    //            break;
    //    }
    //    return string.Empty;
    //}
    internal long NextRowKey() {
        var tmp = 0;
        long key;

        do {
            key = Generic.GetUniqueKey(tmp, "row");
            tmp++;
        } while (SearchByKey(key) != null);
        return key;
    }

    //internal string SetValueInternal(enDatabaseDataType type, string value) {
    //    switch (type) {
    //        case enDatabaseDataType.LastRowKey:
    //            //_LastRowKey = LongParse(value);
    //            break;
    internal void OnRowRemoving(RowEventArgs e) {
        e.Row.RowChecked -= OnRowChecked;
        e.Row.DoSpecialRules -= OnDoSpecialRules;

        if (!_throwEvents) { return; }
        RowRemoving?.Invoke(this, e);
    }

    internal void RemoveNullOrEmpty() => _internal.RemoveNullOrEmpty();

    internal string SetValueInternal(DatabaseDataType type, long? rowkey, RowItem? row, bool isLoading) {
        if (rowkey is null or < 0) { return "Schlüsselfehler"; }

        var db = Database;
        if (db == null || db.IsDisposed) { return "Datenbank verworfen"; }

        if (type == DatabaseDataType.Comand_AddRow) {
            var c = new RowItem(db, (long)rowkey);

            return Add(c);
        }

        if (type == DatabaseDataType.Comand_RemoveRow) {
            //var row = SearchByKey(key);
            if (row == null || row.IsDisposed) { return "Zeile nicht vorhanden"; }

            OnRowRemoving(new RowEventArgs(row));
            foreach (var thisColumnItem in db.Column) {
                if (thisColumnItem != null) {
                    db.Cell.Delete(thisColumnItem, row.Key);
                }
            }

            if (!_internal.TryRemove(row.Key, out _)) { return "Löschen nicht erfolgreich"; }
            OnRowRemoved();
            return string.Empty;
        }

        return "Befehl unbekannt";
    }

    /// <summary>
    /// Fügt eine Zeile hinzu, die im System fest verankert ist.
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>
    private string Add(RowItem row) {
        if (!_internal.TryAdd(row.Key, row)) { return "Hinzufügen fehlgeschlagen."; }
        OnRowAdded(new RowEventArgs(row));
        return string.Empty;
    }

    private void Database_Disposing(object sender, System.EventArgs e) => Dispose();

    private void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
            }
            if (Database != null && !Database.IsDisposed) { Database.Disposing -= Database_Disposing; }
            Database = null;
            _throwEvents = false;
            _internal.Clear();
            // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
            // TODO: Große Felder auf NULL setzen
            IsDisposed = true;
        }
    }

    /// <summary>
    /// Füllt die Liste row um count Einträge auf. Ausgehend von thisrow
    /// </summary>
    /// <param name="row"></param>
    /// <param name="thisrow"></param>
    /// <param name="sortedRows"></param>
    /// <param name="count"></param>
    /// <returns>Gibt false zurück, wenn ALLE Zeilen dadurch geladen sind.</returns>
    private bool FillUp(List<RowItem> row, RowItem thisrow, List<RowItem> sortedRows, int count) {
        var num = sortedRows.IndexOf(thisrow);
        if (num == -1) { return false; } // Wie bitte?

        var c = 1;

        while (count > 0) {
            var n1 = num - c;

            if (n1 >= 0) {
                if (sortedRows[n1].IsInCache == null && !row.Contains(sortedRows[n1])) {
                    row.Add(sortedRows[n1]);
                    count--;
                }
            }

            var n2 = num + c;

            if (n2 < sortedRows.Count) {
                if (sortedRows[n2].IsInCache == null && !row.Contains(sortedRows[n2])) {
                    row.Add(sortedRows[n2]);
                    count--;
                }
            } else {
                return true;
            }
            c++;
        }

        return false;
    }

    //internal void Repair() {
    //    foreach (var ThisRowItem in _Internal.Values) {
    //        if (ThisRowItem != null) {
    //            //ThisRowItem.Repair();
    //            _LastRowKey = Math.Max(_LastRowKey, ThisRowItem.Key); // Die Letzte ID ermitteln,falls der gleadene Wert fehlerhaft ist
    //        }
    //    }
    //}
    private IEnumerator IEnumerable_GetEnumerator() => _internal.Values.GetEnumerator();

    private void OnDoSpecialRules(object sender, DoRowAutomaticEventArgs e) {
        if (!_throwEvents) { return; }
        DoSpecialRules?.Invoke(this, e);
    }

    private void OnRowAdded(RowEventArgs e) {
        e.Row.RowChecked += OnRowChecked;
        e.Row.DoSpecialRules += OnDoSpecialRules;

        if (!_throwEvents) { return; }
        RowAdded?.Invoke(this, e);
    }

    private void OnRowChecked(object sender, RowCheckedEventArgs e) {
        if (!_throwEvents) { return; }
        RowChecked?.Invoke(this, e);
    }

    private void OnRowRemoved() {
        if (!_throwEvents) { return; }
        RowRemoved?.Invoke(this, System.EventArgs.Empty);
    }

    private void PendingWorker_DoWork(object sender, DoWorkEventArgs e) {
        var rk = (long)e.Argument;

        var r = SearchByKey(rk);
        if (r == null || r.IsDisposed) { return; }

        _ = r.ExecuteScript(EventTypes.value_changed_extra_thread, string.Empty, false, false, false, 5);
    }

    private void PendingWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) => _pendingworker.Remove((BackgroundWorker)sender);

    #endregion
}