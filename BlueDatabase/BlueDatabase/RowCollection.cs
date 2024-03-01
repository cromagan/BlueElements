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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using BlueDatabase.Interfaces;

namespace BlueDatabase;

public sealed class RowCollection : IEnumerable<RowItem>, IDisposableExtended, IHasDatabase {

    #region Fields

    private readonly ConcurrentDictionary<string, RowItem> _internal = [];
    private readonly List<string> _pendingChangedBackgroundRow = [];
    private readonly List<string> _pendingChangedRows = [];
    private readonly List<BackgroundWorker> _pendingworker = [];
    private Database? _database;
    private bool _executingbackgroundworks;
    private bool _executingchangedrows;

    #endregion

    #region Constructors

    public RowCollection(Database database) => Database = database;

    #endregion

    #region Destructors

    // TODO: Override a finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources.
    ~RowCollection() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(false);
    }

    #endregion

    #region Events

    public event EventHandler<DoRowAutomaticEventArgs>? DoSpecialRules;

    public event EventHandler<RowChangedEventArgs>? RowAdded;

    public event EventHandler<RowCheckedEventArgs>? RowChecked;

    public event EventHandler<RowEventArgs>? RowGotData;

    public event EventHandler? RowRemoved;

    public event EventHandler<RowChangedEventArgs>? RowRemoving;

    #endregion

    #region Properties

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

    public bool IsDisposed { get; private set; }

    #endregion

    #region Indexers

    /// <summary>
    /// Durchsucht die erste (interne) Spalte der Datenbank nach dem hier angegebenen Prmärschlüssel.
    /// </summary>
    /// <returns>Die Zeile, dessen erste Spalte den Primärschlüssel enthält oder - falls nicht gefunden - NULL.</returns>
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
    /// Durchsucht  Datenbank mit dem angegeben Filter..
    /// </summary>
    /// <returns>Die Zeile, dessen Filter zutrifft - falls nicht gefunden - NULL.</returns>

    public RowItem? this[params FilterItem[] filter] {
        get {
            //if (filter == null || filter.Length == 0) {
            //    Develop.DebugPrint("Kein Filter angekommen!");
            //    return null;
            //}

            return _internal.Values.FirstOrDefault(thisRow => thisRow != null && thisRow.MatchesTo(filter.ToList()));

            //FilterCollection d = new(filter[0].database);
            //d.AddRange(filter);
            //return this[d];
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Erstellt eine neue Spalte mit den aus den Filterkriterien. Nur Filter IstGleich wird unterstützt.
    /// Schägt irgendetwas fehl, wird NULL zurückgegeben.
    /// Ist ein Filter mehrfach vorhanden, erhält die Zelle den LETZTEN Wert.
    /// Am Schluß wird noch das Skript ausgeführt.
    /// </summary>
    /// <param name="fc"></param>
    /// <param name="comment"></param>
    /// <returns></returns>
    public static RowItem? GenerateAndAdd(FilterCollection fc, string comment) {
        IReadOnlyCollection<string>? first = null;

        Database? db2 = null;

        foreach (var thisfi in fc) {
            if (thisfi.FilterType is not FilterType.Istgleich
                and not FilterType.Istgleich_GroßKleinEgal
                 and not FilterType.Istgleich_ODER_GroßKleinEgal) { return null; }
            if (thisfi.Column == null) { return null; }
            if (thisfi.Database is not Database db1 || db1.IsDisposed) { return null; }
            db2 ??= db1;

            if (db1.Column.First() == thisfi.Column) {
                if (first != null) { return null; }
                first = thisfi.SearchValue;
            }

            if (thisfi.Column.Database != db2) { return null; }
        }

        if (db2 == null || db2.IsDisposed) { return null; }

        if (!db2.Row.IsNewRowPossible()) { return null; }

        if (first == null) { return null; }

        var s = db2.NextRowKey();
        if (s == null || string.IsNullOrEmpty(s)) { return null; }

        var row = db2.Row.GenerateAndAdd(s, first.JoinWithCr(), false, true, comment);

        foreach (var thisfi in fc) {
            if (thisfi.Column is ColumnItem c) {
                row.CellSet(c, thisfi.SearchValue.ToList());
            }
        }

        _ = row.ExecuteScript(ScriptEventTypes.new_row, string.Empty, false, false, true, 1, null);

        return row;
    }

    public static string UniqueKeyValue() {
        var x = 9999;
        do {
            x += 1;
            if (x > 99999) { Develop.DebugPrint(FehlerArt.Fehler, "Unique ID konnte nicht erzeugt werden"); }

            var unique = ("X" + DateTime.UtcNow.ToString("mm.fff") + x.ToString(Constants.Format_Integer5)).RemoveChars(Constants.Char_DateiSonderZeichen + ".");
            var ok = true;

            foreach (var thisfile in Database.AllFiles) {
                var row = thisfile.Row[unique];
                if (row != null && !row.IsDisposed) { ok = false; break; }
            }

            if (ok) { return unique; }
        } while (true);
    }

    public void AddRowWithChangedValue(RowItem? row) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }
        if (row == null || row.IsDisposed) { return; }

        if (!db.IsRowScriptPossible(true)) { return; }

        if (!row.NeedsUpdate()) { return; }

        var isMyRow = row.AmIChanger();
        var age = row.RowChangedXMinutesAgo();

        if (isMyRow && age >= 0 && age < 55) {
            _ = _pendingChangedRows.AddIfNotExists(row.KeyName);
            return;
        }

        if (db.AmITemporaryMaster(false) && age is < 0 or > 80) {
            _ = _pendingChangedRows.AddIfNotExists(row.KeyName);
        }
    }

    ///// <summary>
    ///// Gibt einen Zeilenschlüssel zurück, der bei allen aktuell geladenen Datenbanken einzigartig ist.
    ///// </summary>
    ///// <returns></returns>
    //public List<RowData> AllRows() {
    //    var sortedRows = new List<RowData>();
    //    foreach (var thisRowItem in this) {
    //        if (thisRowItem != null) {
    //            sortedRows.Add(new RowData(thisRowItem));
    //        }
    //    }
    //    return sortedRows;
    //}

    public bool Clear(string comment) => Remove(new FilterCollection(Database, "rowcol clear"), null, comment);

    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // TODO: Override a finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources.

    public void ExecuteExtraThread() {
        if (_pendingChangedRows.Count > 0) { return; }
        if (_pendingChangedBackgroundRow.Count == 0) { return; }
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }
        if (!Database.IsRowScriptPossible(true)) { return; }

        if (_executingbackgroundworks) { return; }
        _executingbackgroundworks = true;

        var ev = Database.EventScript.Get(ScriptEventTypes.value_changed_extra_thread);
        var ok = false;
        if (ev.Count == 1) { ok = ev[0].IsOk(); }

        if (!ok) {
            _pendingChangedBackgroundRow.Clear();
            _executingbackgroundworks = false;
            return;
        }

        try {
            while (_pendingworker.Count < 5) {
                if (db.IsDisposed || IsDisposed) { break; }
                if (!Database.IsRowScriptPossible(true)) { break; }
                if (!Database.LogUndo) { break; }
                if (_pendingChangedBackgroundRow.Count == 0) { break; }
                if (_pendingChangedRows.Count > 0) { break; }
                if (!Database.IsRowScriptPossible(true)) { break; }

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

    public string ExecuteScript(ScriptEventTypes? eventname, string scriptname, List<RowItem> rows) {
        var m = Database.EditableErrorReason(Database, EditableErrorReasonType.EditCurrently);
        if (!string.IsNullOrEmpty(m) || Database is not Database db || db.IsDisposed) { return m; }

        if (rows.Count == 0) { return "Keine Zeilen angekommen."; }

        Database.RefreshRowData(rows);

        var txt = "Skript wird ausgeführt: " + scriptname;

        Database.OnProgressbarInfo(new ProgressbarEventArgs(txt, 0, rows.Count, true, false));

        var all = rows.Count;
        var start = DateTime.UtcNow;
        while (rows.Count > 0) {
            Database.OnProgressbarInfo(new ProgressbarEventArgs(txt, all - rows.Count, all, false, false));

            var scx = rows[0].ExecuteScript(eventname, scriptname, true, true, true, 0, null);

            if (!scx.AllOk) {
                var w = rows[0].CellFirstString();
                rows.Clear();
                Database.OnProgressbarInfo(new ProgressbarEventArgs(txt, rows.Count, rows.Count, false, true));
                Database.OnDropMessage(FehlerArt.Warnung, "Skript fehlerhaft bei " + w);
                return "Skript fehlerhaft bei " + w + "\r\n" + scx.Protocol[0];
            }

            rows.RemoveAt(0);

            if (DateTime.UtcNow.Subtract(start).TotalMinutes > 1) {
                db.Save();
                start = DateTime.UtcNow;
            }
        }
        db.Save();
        Database.OnProgressbarInfo(new ProgressbarEventArgs(txt, rows.Count, rows.Count, false, true));
        return string.Empty;
    }

    public void ExecuteValueChangedEvent(bool ignoreUserAction) {
        if (_pendingChangedRows.Count == 0) { return; }
        if (!ignoreUserAction && DateTime.UtcNow.Subtract(Develop.LastUserActionUtc).TotalSeconds < 6) { return; }

        if (_executingchangedrows) { return; }
        _executingchangedrows = true;

        while (_pendingChangedRows.Count > 0) {
            if (IsDisposed) { break; }
            if (IsDisposed || Database is not Database db || db.IsDisposed) { break; }

            if (!Database.IsRowScriptPossible(true)) { break; }

            var e = new CancelReasonEventArgs();
            Database.OnCanDoScript(e);
            if (e.Cancel) { break; }

            try {
                var key = _pendingChangedRows[0];

                var r = SearchByKey(key);
                if (r != null && !r.IsDisposed) {
                    var ok = r.ExecuteScript(ScriptEventTypes.value_changed, string.Empty, true, true, true, 2, null);

                    if (!ok.AllOk) { break; }

                    r.InvalidateCheckData();
                    r.CheckRowDataIfNeeded();
                    _pendingChangedBackgroundRow.Add(key);
                }

                _ = _pendingChangedRows.Remove(key);
                Database?.OnInvalidateView();
            } catch { }
        }

        _executingchangedrows = false;
    }

    //                    if (LongTryParse(s, out var v)) { key.Add(v); }
    //                }
    public RowItem? First() => _internal.Values.FirstOrDefault(thisRowItem => thisRowItem != null && !thisRowItem.IsDisposed);

    public RowItem? GenerateAndAdd(string valueOfCellInFirstColumn, string comment) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return null; }
        if (!Database.Row.IsNewRowPossible()) { return null; }

        var s = Database.NextRowKey();
        if (s == null || string.IsNullOrEmpty(s)) { return null; }

        return GenerateAndAdd(s, valueOfCellInFirstColumn, true, true, comment);
    }

    //                foreach (var thisRow in rowsToExpand) {
    //                    var s = database.Cell.GetStringBehindLinkedValue(thisColumn, thisRow);
    /// <summary>
    ///
    /// </summary>
    /// <param name="key"></param>
    /// <param name="valueOfCellInFirstColumn"></param>
    /// <param name="runScriptOfNewRow"></param>
    /// <param name="fullprocessing">Sollen der Zeilenersteller, das Datum und die Initalwerte geschrieben werden?</param>
    /// <param name="comment"></param>
    /// <returns></returns>
    public RowItem GenerateAndAdd(string key, string valueOfCellInFirstColumn, bool runScriptOfNewRow, bool fullprocessing, string comment) {
        var db = Database;
        if (db == null || db.IsDisposed) {
            Develop.DebugPrint(FehlerArt.Fehler, "Datenbank verworfen!");
            throw new Exception();
        }

        if (!IsNewRowPossible()) {
            Develop.DebugPrint(FehlerArt.Fehler, "Neue Zeilen nicht möglich");
            throw new Exception();
        }

        var item = SearchByKey(key);
        if (item != null) {
            Develop.DebugPrint(FehlerArt.Fehler, "Schlüssel belegt!");
            throw new Exception();
        }

        var u = Generic.UserName;
        var d = DateTime.UtcNow;

        var s = db.ChangeData(DatabaseDataType.Command_AddRow, null, null, string.Empty, key, u, d, comment);
        if (!string.IsNullOrEmpty(s)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Erstellung fehlgeschlagen: " + s);
            throw new Exception();
        }

        item = SearchByKey(key);
        if (item == null) {
            Develop.DebugPrint(FehlerArt.Fehler, "Erstellung fehlgeschlagen, ID Fehler");
            throw new Exception();
        }

        if (fullprocessing) {
            if (db.Column.SysRowCreator is ColumnItem src) { item.CellSet(src, u); }
            if (db.Column.SysRowCreateDate is ColumnItem scd) { item.CellSet(scd, d.ToString(Constants.Format_Date5, CultureInfo.InvariantCulture)); }

            //// Dann die Inital-Werte reinschreiben
            //foreach (var thisColum in db.Column) {
            //    if (thisColum != null && !string.IsNullOrEmpty(thisColum.CellInitValue)) {
            //        item.CellSet(thisColum, thisColum.CellInitValue);
            //    }
            //}
        }

        if (!string.IsNullOrEmpty(valueOfCellInFirstColumn)) {
            item.CellSet(db.Column.First(), valueOfCellInFirstColumn);
        } else {
            Develop.DebugPrint(FehlerArt.Warnung, "Null!");
        }

        if (item.CellFirstString() != db.Column?.First()?.AutoCorrect(valueOfCellInFirstColumn, true)) {
            Develop.DebugPrint(FehlerArt.Warnung, "Fehler!!");
        }

        if (runScriptOfNewRow) {
            _ = item.ExecuteScript(ScriptEventTypes.new_row, string.Empty, true, true, true, 0.1f, null);
        }

        return item;
    }

    //    List<Database> done = new();
    public IEnumerator<RowItem> GetEnumerator() => _internal.Values.GetEnumerator();

    //    if (database is not Database db) { return "Verlinkung zur Datenbank fehlhlerhaft"; }
    //    if (db.IsDisposed) { return "Datenbank verworfen"; }
    //foreach (var ThisRowItem in _Internal.Values)//{//    if (ThisRowItem != null) { return ThisRowItem; }//}//return null;
    IEnumerator IEnumerable.GetEnumerator() => _internal.Values.GetEnumerator();

    public bool HasPendingWorker() => _pendingworker.Count > 0;

    //public string DoLinkedDatabase(List<RowItem> rowsToExpand) {
    //    if (rowsToExpand.Count == 0) { return string.Empty; }
    public void InvalidateAllCheckData() {
        foreach (var thisRow in this) {
            thisRow.InvalidateCheckData();
        }
    }

    public bool IsNewRowPossible() {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return false; }

        return Database.IsNewRowPossible();
    }

    public bool Remove(string key, string comment) {
        var r = SearchByKey(key);

        if (r == null || r.IsDisposed) { return false; }
        return string.IsNullOrEmpty(Database?.ChangeData(DatabaseDataType.Command_RemoveRow, null, r, string.Empty, key, Generic.UserName, DateTime.UtcNow, comment));
    }

    public bool Remove(FilterItem fi, List<RowItem>? pinned, string comment) {
        using FilterCollection fc = new(Database, "renome row") { fi };
        return Remove(fc, pinned, comment);
    }

    public bool Remove(FilterCollection? fc, List<RowItem>? pinned, string comment) {
        var keys = (from thisrowitem in _internal.Values where thisrowitem != null && thisrowitem.MatchesTo(fc.ToList()) select thisrowitem.KeyName).Select(dummy => dummy).ToList();
        var did = false;

        foreach (var thisKey in keys) {
            if (Remove(thisKey, comment)) { did = true; }
        }

        if (pinned != null && pinned.Count > 0) {
            foreach (var thisr in pinned) {
                if (Remove(thisr.KeyName, comment)) { did = true; }
            }
        }

        return did;
    }

    public bool Remove(RowItem? row, string comment) => row != null && Remove(row.KeyName, comment);

    public bool RemoveOlderThan(float inHours, string comment) {
        if (Database?.Column.SysRowCreateDate is not ColumnItem src) { return false; }

        var x = (from thisrowitem in _internal.Values where thisrowitem != null let d = thisrowitem.CellGetDateTime(src) where DateTime.UtcNow.Subtract(d).TotalHours > inHours select thisrowitem.KeyName).Select(dummy => dummy).ToList();
        //foreach (var thisrowitem in _Internal.Values)
        //{
        //    if (thisrowitem != null)
        //    {
        //        var D = thisrowitem.CellGetDateTime(database.Column.SysRowCreateDate());
        //        if (DateTime.UtcNow.Subtract(D).TotalHours > InHours) { x.GenerateAndAdd(thisrowitem.KeyName); }
        //    }
        //}
        if (x.Count == 0) { return false; }
        foreach (var thisKey in x) {
            _ = Remove(thisKey, comment);
        }
        return true;
    }

    public RowItem? SearchByKey(string? key) {
        if (Database is not Database db || db.IsDisposed || key == null || string.IsNullOrWhiteSpace(key)) { return null; }
        try {
            var r = _internal.ContainsKey(key) ? _internal[key] : null;
            if (r != null && r.IsDisposed) {
                Develop.DebugPrint(FehlerArt.Fehler, "Interner Zeilenfehler: " + key);
                return null;
            }

            return r;
        } catch {
            Develop.CheckStackForOverflow();
            return SearchByKey(key);
        }
    }

    internal static List<RowItem> MatchesTo(FilterItem fi) {
        List<RowItem> l = [];

        if (fi.Database is not Database db || db.IsDisposed) { return l; }

        l.AddRange(db.Row.Where(thisRow => thisRow.MatchesTo(fi)));
        return l;
    }

    internal void AddRowsForValueChangedEvent() {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }
        if (!db.IsRowScriptPossible(true)) { return; }

        var x = DateTime.UtcNow;

        try {
            foreach (var thisRow in this) {
                if (thisRow.IsInCache != null) {
                    AddRowWithChangedValue(thisRow);
                }
                if (DateTime.UtcNow.Subtract(x).TotalSeconds > 5) { break; }
            }
        } catch { }
    }

    internal void CloneFrom(Database sourceDatabase) {
        if (IsNewRowPossible()) {
            Develop.DebugPrint(FehlerArt.Fehler, "Neue Zeilen nicht erlaubt");
        }

        // Zeilen, die zu viel sind, löschen
        foreach (var thisRow in this) {
            var l = sourceDatabase.Row.SearchByKey(thisRow.KeyName);
            if (l == null) { _ = Remove(thisRow, "Clone - Zeile zuviel"); }
        }

        // Zeilen erzeugen und Format übertragen
        foreach (var thisRow in sourceDatabase.Row) {
            var l = SearchByKey(thisRow.KeyName) ?? GenerateAndAdd(thisRow.KeyName, string.Empty, false, false, "Clone - Zeile fehlt");
            l.CloneFrom(thisRow, true);
        }

        if (sourceDatabase.Row.Count != Count) {
            Develop.DebugPrint(FehlerArt.Fehler, "Clone Fehlgeschlagen");
        }
    }

    internal string ExecuteCommand(DatabaseDataType type, string rowkey, Reason reason) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return "Datenbank verworfen"; }

        if (type == DatabaseDataType.Command_AddRow) {
            var row = SearchByKey(rowkey);
            if (row != null && !row.IsDisposed) { return string.Empty; } // "Zeile " + rowkey+ " bereits vorhanden!";

            var c = new RowItem(db, rowkey);
            var f = Add(c, reason);
            if (reason == Reason.SetCommand && db.LogUndo) {
                Generic.Pause(0.001, false); // um in den Logs den Zeitstempel richtig zu haben
            }
            return f;
        }

        if (type == DatabaseDataType.Command_RemoveRow) {
            var row = SearchByKey(rowkey);
            if (row == null) { return "Zeile nicht gefunden!"; }

            OnRowRemoving(new RowChangedEventArgs(row, reason));
            foreach (var thisColumn in db.Column) {
                if (thisColumn != null) {
                    db.Cell.SetValueInternal(thisColumn, row, string.Empty, Reason.AdditionalWorkAfterCommand);
                }
            }

            if (!_internal.TryRemove(row.KeyName, out _)) { return "Löschen nicht erfolgreich"; }
            OnRowRemoved();
            return string.Empty;
        }

        return "Befehl unbekannt";
    }

    internal void RemoveNullOrEmpty() => _internal.RemoveNullOrEmpty();

    private void _database_Disposing(object sender, System.EventArgs e) => Dispose();

    /// <summary>
    /// Fügt eine Zeile hinzu, die im System fest verankert ist.
    /// </summary>
    /// <param name="row"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    private string Add(RowItem row, Reason reason) {
        if (!_internal.TryAdd(row.KeyName, row)) { return "Hinzufügen fehlgeschlagen."; }
        OnRowAdded(new RowChangedEventArgs(row, reason));
        return string.Empty;
    }

    private void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                Database = null;
                foreach (var thisR in _internal) { thisR.Value.Dispose(); }
            }

            _internal.Clear();
            _pendingChangedBackgroundRow.Clear();
            _pendingChangedRows.Clear();

            IsDisposed = true;
        }
    }

    private void OnDoSpecialRules(object sender, DoRowAutomaticEventArgs e) => DoSpecialRules?.Invoke(this, e);

    //internal void Repair() {
    //    foreach (var ThisRowItem in _Internal.Values) {
    //        if (ThisRowItem != null) {
    //            //ThisRowItem.Repair();
    //            _LastRowKey = Math.Max(_LastRowKey, ThisRowItem.KeyName); // Die Letzte ID ermitteln,falls der gleadene Wert fehlerhaft ist
    //        }
    //    }
    //}
    //private IEnumerator IEnumerable_GetEnumerator() => _internal.Values.GetEnumerator();
    private void OnRowAdded(RowChangedEventArgs e) {
        e.Row.RowChecked += OnRowChecked;
        e.Row.DoSpecialRules += OnDoSpecialRules;
        e.Row.RowGotData += OnRowGotData;

        RowAdded?.Invoke(this, e);
    }

    private void OnRowChecked(object sender, RowCheckedEventArgs e) => RowChecked?.Invoke(this, e);

    private void OnRowGotData(object sender, RowEventArgs e) => RowGotData?.Invoke(this, e);

    private void OnRowRemoved() => RowRemoved?.Invoke(this, System.EventArgs.Empty);

    private void OnRowRemoving(RowChangedEventArgs e) {
        e.Row.RowChecked -= OnRowChecked;
        e.Row.DoSpecialRules -= OnDoSpecialRules;
        e.Row.RowGotData -= OnRowGotData;
        RowRemoving?.Invoke(this, e);
    }

    private void PendingWorker_DoWork(object sender, DoWorkEventArgs e) {
        var rk = (string)e.Argument;
        var r = SearchByKey(rk);
        if (r == null || r.IsDisposed) { return; }
        var _ = r.ExecuteScript(ScriptEventTypes.value_changed_extra_thread, string.Empty, false, false, false, 10, null);
    }

    private void PendingWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) => _pendingworker.Remove((BackgroundWorker)sender);

    #endregion
}