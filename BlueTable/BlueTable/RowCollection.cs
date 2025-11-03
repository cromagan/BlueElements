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

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueTable.Enums;
using BlueTable.EventArgs;
using BlueTable.Interfaces;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace BlueTable;

public sealed class RowCollection : IEnumerable<RowItem>, IDisposableExtended, IHasTable {

    #region Fields

    public static readonly ConcurrentDictionary<RowItem, string> FailedRows = [];
    public static readonly InvalidatedRowsManager InvalidatedRowsManager = new InvalidatedRowsManager();
    public static int WaitDelay;
    private static readonly List<BackgroundWorker> Pendingworker = [];
    private static int _executingchangedrows;

    private readonly ConcurrentDictionary<string, RowItem> _internal = [];
    private Table? _table;

    #endregion

    #region Constructors

    public RowCollection(Table table) => Table = table;

    #endregion

    #region Destructors

    // TODO: Override a finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources.
    ~RowCollection() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(false);
    }

    #endregion

    #region Events

    public event EventHandler<RowEventArgs>? RowAdded;

    public event EventHandler<RowCheckedEventArgs>? RowChecked;

    public event EventHandler<RowEventArgs>? RowRemoved;

    public event EventHandler<RowEventArgs>? RowRemoving;

    #endregion

    #region Properties

    /// <summary>
    /// Wert in Minuten.
    /// Gibt an, wieviel Minuten maximal vergangen sein dürfen, um eine Zeile direkt zu INITIALISIEREN
    /// </summary>
    public static double NewRowTolerance { get; set; } = 1;

    public int Count => _internal.Count;

    public bool IsDisposed { get; private set; }

    public Table? Table {
        get => _table;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == _table) { return; }

            if (_table != null) {
                _table.DisposingEvent -= _table_Disposing;
            }
            _table = value;

            if (_table != null) {
                _table.DisposingEvent += _table_Disposing;
            }
        }
    }

    #endregion

    #region Indexers

    /// <summary>
    /// Durchsucht die erste (interne) Spalte der Tabelle nach dem hier angegebenen Prmärschlüssel.
    /// </summary>
    /// <returns>Die Zeile, dessen erste Spalte den Primärschlüssel enthält oder - falls nicht gefunden - NULL.</returns>
    public RowItem? this[string primärSchlüssel] {
        get {
            if (Table?.Column.First is { IsDisposed: false } c) {
                if (c.Value_for_Chunk != ChunkType.None) {
                    var ok = Table.BeSureRowIsLoaded(primärSchlüssel);
                    if (!ok) { return null; }
                }

                var parallelQuery = _internal.Values.AsParallel()
                                    .Where(thisRow => thisRow != null)
                                    .FirstOrDefault(thisRow => thisRow.CompareValues(c, primärSchlüssel, FilterType.Istgleich_GroßKleinEgal));

                return parallelQuery;
            }
            return null;
        }
    }

    public RowItem? this[params FilterItem[] filter] {
        get {
            var f = FilterCollection.CalculateFilteredRows(Table, filter);

            //var parallelQuery = _internal.Values.AsParallel()
            //                                    .Where(thisRow => thisRow != null)
            //                                    .FirstOrDefault(thisRow => thisRow.MatchesTo(filter));

            //return parallelQuery;
            return f.Count == 0 ? null : f[0];
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Durchsucht  Tabelle mit dem angegeben Filter..
    /// </summary>
    /// <returns>Die Zeile, dessen Filter zutrifft - falls nicht gefunden - NULL.</returns>
    public static void AddBackgroundWorker(RowItem row) {
        if (row.IsDisposed || row.Table is not { IsDisposed: false } db) { return; }
        if (!db.IsThisScriptBroken(ScriptEventTypes.value_changed_extra_thread, false)) { return; }
        if (!db.IsRowScriptPossible()) { return; }
        var l = new BackgroundWorker {
            WorkerReportsProgress = true
        };
        l.RunWorkerCompleted += PendingWorker_RunWorkerCompleted;
        l.DoWork += PendingWorker_DoWork;

        Pendingworker.Add(l);
        l.RunWorkerAsync(row);

        Develop.Message?.Invoke(ErrorType.Info, db, "Table", ImageCode.Blitz, "Hintergrund-Skript wird ausgeführt: " + row.CellFirstString(), 0);
    }

    public static void ExecuteValueChangedEvent() {
        // Lock-freie Implementierung mit Interlocked für bessere Performance und Deadlock-Vermeidung
        if (Interlocked.CompareExchange(ref _executingchangedrows, 1, 0) != 0) {
            return; // Bereits in Ausführung
        }

        try {
            List<Table> l = [.. Table.AllFiles];
            if (l.Count == 0) { return; }
            try {
                l = [.. l.OrderByDescending(eintrag => eintrag.LastUsedDate)];
            } catch { return; }

            var tim = Stopwatch.StartNew();

            while (NextRowToCeck() is { IsDisposed: false } row) {
                if (row.IsDisposed || row.Table is not { IsDisposed: false } tbl) { break; }

                if (row.Table == l[0]) {
                    if (DateTime.UtcNow.Subtract(Develop.LastUserActionUtc).TotalSeconds < 1) { break; }
                } else {
                    if (DateTime.UtcNow.Subtract(Develop.LastUserActionUtc).TotalSeconds < 5 + WaitDelay) { break; }
                }

                if (Table.ExecutingScriptThreadsAnyTable.Count > 0) { break; }

                WaitDelay = Pendingworker.Count * 5;
                if (Pendingworker.Count > 2) { break; }

                if (!tbl.CanDoValueChangedScript(true)) { break; }

                var f = tbl.ExternalAbortScriptReasonExtended();
                if (!string.IsNullOrEmpty(f)) { return; }

                Develop.SetUserDidSomething();
                if (Table.ExecutingScriptThreadsAnyTable.Count > 0) { break; }
                _ = row.UpdateRow(true, "Allgemeines Update (User Idle)");
                Develop.SetUserDidSomething();
                if (tim.ElapsedMilliseconds > 30 * 1000) { break; }
            }

            WaitDelay = Math.Min(WaitDelay + 5, 100);
        } finally {
            // Garantierte Freigabe des Flags auch bei Exceptions
            Interlocked.Exchange(ref _executingchangedrows, 0);
        }
    }

    /// <summary>
    /// Sucht dopplete Einträge in der angegebenen Spalte. Dabei werden Multiline-Einträge auggesplittet.
    /// </summary>
    /// <param name="column"></param>
    /// <param name="rows"></param>
    /// <param name="unique"></param>
    /// <param name="notUnique"></param>
    public static void GetUniques(ColumnItem column, List<RowItem> rows, out List<string> unique, out List<string> notUnique) {
        unique = [];
        notUnique = [];
        HashSet<string> uniqueSet = [];

        foreach (var thisRow in rows) {
            if (thisRow is { IsDisposed: false }) {
                var values = column.MultiLine ? thisRow.CellGetList(column) : [thisRow.CellGetString(column)];
                foreach (var value in values) {
                    if (!uniqueSet.Add(value)) {
                        _ = notUnique.AddIfNotExists(value);
                    } else {
                        unique.Add(value);
                    }
                }
            }
        }
        unique = [.. unique.Except(notUnique)];
    }

    /// <summary>
    /// Prüft alle Tabellen im Speicher und gibt die dringenste Update-Aufgabe aller Tabellen zurück.
    /// </summary>
    /// <returns></returns>
    public static RowItem? NextRowToCeck() {
        try {
            List<Table> l = [.. Table.AllFiles];

            if (l.Count == 0) { return null; }

            if (Constants.GlobalRnd.Next(10) == 1) {
                l.Shuffle();
            } else {
                try {
                    l = [.. l.OrderByDescending(eintrag => eintrag?.LastUsedDate ?? DateTime.MinValue)];
                } catch {
                    return null;
                }
            }

            foreach (var thisDb in l) {
                if (thisDb is { IsDisposed: false } db) {
                    try {
                        if (!db.CanDoValueChangedScript(false)) { continue; }

                        var rowToCheck = db.Row?.NextRowToCheck(false);
                        if (rowToCheck != null) { return rowToCheck; }
                    } catch (ObjectDisposedException) {
                        // Table disposed während Verwendung - skip
                        continue;
                    }
                }
            }
        } catch { }

        return null;
    }

    public static bool Remove(FilterCollection? fc, List<RowItem>? pinned, string comment) {
        var allrows = new List<RowItem>();
        if (fc?.Rows is { Count: > 0 } rows) { allrows.AddRange(rows); }
        if (pinned is { Count: > 0 }) { allrows.AddRange(pinned); }

        return Remove(allrows, comment);
    }

    public static bool Remove(List<RowItem>? allrows, string comment) {
        if (allrows == null || allrows.Count == 0) { return false; }

        var did = false;

        Table? db = null;

        allrows = [.. allrows.Distinct()];

        foreach (var thisr in allrows) {
            db ??= thisr.Table;

            if (db != thisr.Table) {
                Develop.DebugPrint(ErrorType.Error, "Tabellen inkonsitent");
                return false;
            }
        }

        foreach (var thisRow in allrows) {
            if (Remove(thisRow, comment)) { did = true; }
        }

        return did;
    }

    public static bool Remove(RowItem? row, string comment) => row is { IsDisposed: false } r
                                                            && string.IsNullOrEmpty(r.Table?.ChangeData(TableDataType.Command_RemoveRow, null, r, string.Empty, r.KeyName, Generic.UserName, DateTime.UtcNow, comment, string.Empty, r.ChunkValue));

    /// <summary>
    /// Prüft alle Tabellen im Speicher und gibt die dringenste Update-Aufgabe aller Tabellen zurück.
    /// </summary>
    /// <returns></returns>
    public static List<RowItem> RowListToCheck() {
        var r = new List<RowItem>();
        List<Table> l = [.. Table.AllFiles];

        foreach (var thisDb in l) {
            if (thisDb is { IsDisposed: false } db) {
                if (!db.CanDoValueChangedScript(false)) { continue; }
                _ = db.BeSureAllDataLoaded(30);
                r.AddRange(db.Row);
            }
        }

        return [.. r.OrderBy(eintrag => eintrag.UrgencyUpdate)];
    }

    public static string UniqueKeyValue() {
        var x = 9999;
        do {
            x += 1;
            if (x > 99999) { Develop.DebugPrint(ErrorType.Error, "Unique ID konnte nicht erzeugt werden"); }

            var unique = ("X" + DateTime.UtcNow.ToString("mm.fff") + x.ToStringInt5()).RemoveChars(Constants.Char_DateiSonderZeichen + ".");
            var ok = true;

            foreach (var thisfile in Table.AllFiles) {
                var row = thisfile.Row[unique];
                if (row is { IsDisposed: false }) { ok = false; break; }
            }

            if (ok) { return unique; }
        } while (true);
    }

    public static (RowItem? newrow, string message, bool stoptrying) UniqueRow(FilterCollection filter, string coment) {
        if (filter.Table is not { IsDisposed: false } db) { return (null, "Tabelle verworfen", true); }

        if (filter.Count < 1) { return (null, "Kein Filter angekommen.", true); }

        var r = filter.Rows;

        if (r.Count > 10) {
            return (null, "RowUnique gescheitert, da bereits zu viele Zeilen vorhanden sind: " + filter.ReadableText(), true);
        }

        if (r.Count == 0) {
            if (!db.IsThisScriptBroken(ScriptEventTypes.InitialValues, true)) { return (null, $"In der Tabelle '{db.Caption}' sind die Skripte defekt", true); }
        }

        if (r.Count > 1) {
            if (!db.IsThisScriptBroken(ScriptEventTypes.row_deleting, true)) { return (null, $"In der Tabelle '{db.Caption}' sind die Skripte defekt", true); }

            db.Row.Combine(r);
            db.Row.RemoveYoungest(r, true);

            r = filter.Rows;
            if (r.Count != 1) {
                return (null, "RowUnique gescheitert, Aufräumen fehlgeschlagen: " + filter.ReadableText(), false);
            }
            if (db.Column.SysRowState is { IsDisposed: false } srs) {
                r[0].CellSet(srs, string.Empty, "'UniqueRow' Aufräumen mehrerer Zeilen.");
            }
        }

        RowItem? myRow;

        if (r.Count == 0) {
            var (newrow, message, stoptrying) = db.Row.GenerateAndAdd([.. filter], coment);
            if (newrow == null) { return (null, "Neue Zeile konnte nicht erstellt werden: " + message, stoptrying); }
            myRow = newrow;
        } else {
            myRow = r[0];
        }

        // REPARIERT: Finale Validierung dass die Zeile auch wirklich den Filtern entspricht
        if (!myRow.MatchesTo([.. filter])) {
            return (myRow, "RowUnique mit falschen Werten initialisiert", true);
        }

        return (myRow, string.Empty, true);
    }

    public bool Clear(string comment) {
        using var fc = new FilterCollection(Table, "rowcol clear");
        return Remove(fc, null, comment);
    }

    /// <summary>
    /// Alle Angegebenen Zeilen werden die gleichen Werte erhalten.
    /// </summary>
    /// <param name="rows"></param>
    public void Combine(ICollection<RowItem> rows) {
        if (rows.Count < 2) { return; }

        if (Table is not { IsDisposed: false } db) { return; }

        #region Leere Werte befüllen

        foreach (var thisC in db.Column) {

            #region neuen Wert zum Reinschreiben ermitteln (Wert)

            var wert = string.Empty;
            foreach (var thisR2 in rows) {
                if (thisR2.Table != db) { return; }

                if (string.IsNullOrEmpty(wert)) { wert = thisR2.CellGetString(thisC); }
            }

            #endregion

            #region Wert in leere Zellen reinscheiben

            foreach (var thisR2 in rows) {
                if (string.IsNullOrEmpty(thisR2.CellGetString(thisC))) { thisR2.CellSet(thisC, wert, "Zeilenbereinigungs-Dialog"); }
            }

            #endregion
        }

        #endregion
    }

    ///// <summary>
    ///// Gibt einen Zeilenschlüssel zurück, der bei allen aktuell geladenen Tabellen einzigartig ist.
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
    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public string ExecuteScript(ScriptEventTypes? eventname, string scriptname, List<RowItem> rows) {
        if (Table is not { IsDisposed: false } tb) { return "Tabelle verworfen"; }

        if (!tb.IsEditable(false)) { return tb.IsNotEditableReason(false); }

        if (rows.Count == 0) { return "Keine Zeilen angekommen."; }

        var txt = "Skript wird ausgeführt: " + scriptname;

        Table.OnProgressbarInfo(new ProgressbarEventArgs(txt, 0, rows.Count, true, false));

        var all = rows.Count;
        var start = DateTime.UtcNow;
        while (rows.Count > 0) {
            Table.OnProgressbarInfo(new ProgressbarEventArgs(txt, all - rows.Count, all, false, false));

            var scx = rows[0].ExecuteScript(eventname, scriptname, true, 0, null, true, true);

            if (scx.Failed) {
                var w = rows[0].CellFirstString();
                rows.Clear();
                Table.OnProgressbarInfo(new ProgressbarEventArgs(txt, rows.Count, rows.Count, false, true));
                Develop.Message?.Invoke(ErrorType.Warning, tb, "Table", ImageCode.Skript, "Skript fehlerhaft bei " + w, 0);
                return "Skript fehlerhaft bei " + w + "\r\n" + scx.Protocol[0];
            }

            rows.RemoveAt(0);

            if (DateTime.UtcNow.Subtract(start).TotalMinutes > 1) {
                if (tb is TableFile tbf) { tbf.Save(true); }

                start = DateTime.UtcNow;
            }
        }
        if (tb is TableFile tbf2) { tbf2.Save(true); }
        Table.OnProgressbarInfo(new ProgressbarEventArgs(txt, rows.Count, rows.Count, false, true));
        return string.Empty;
    }

    public RowItem? First() => _internal.Values.FirstOrDefault(thisRowItem => thisRowItem is { IsDisposed: false });

    /// <summary>
    /// Erstellt eine neue Zeile mit den aus den Filterkriterien. Nur Filter IstGleich wird unterstützt.
    /// Schägt irgendetwas fehl, wird NULL zurückgegeben.
    /// Ist ein Filter mehrfach vorhanden, erhält die Zelle den LETZTEN Wert.
    /// Am Schluss wird noch das Skript ausgeführt.
    /// REPARIERT: Exception-Handling und sichere Initialwert-Setzung
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="comment"></param>
    /// <returns></returns>
    public (RowItem? newrow, string message, bool stoptrying) GenerateAndAdd(FilterItem[] filter, string comment) {
        Table? tb2 = null;

        foreach (var thisfi in filter) {
            if (thisfi.Table is not { IsDisposed: false } tb1) { return (null, "Tabelle eines Filters nicht angegeben", true); }
            tb2 ??= tb1;

            if (thisfi.Column?.Table is { } tb3 && tb3 != tb2) { return (null, "Tabellen der Spalten im Filter unterschiedlich", true); }
        }

        if (tb2 is not { IsDisposed: false }) { return (null, "Tabellen verworfen", true); }

        var s = tb2.NextRowKey();
        if (string.IsNullOrEmpty(s)) { return (null, "Fehler beim Zeilenschlüssel erstellen, Systeminterner Fehler", false); }

        var chunkval = string.Empty;
        foreach (var thisColum in tb2.Column) {
            if (thisColum.IsFirst || thisColum.Value_for_Chunk != ChunkType.None) {
                var inval = FilterCollection.InitValue(thisColum, true, false, filter);
                if (inval is not { } || string.IsNullOrWhiteSpace(inval)) {
                    return (null, "Initalwert fehlt.", false);
                }

                if (thisColum.Value_for_Chunk != ChunkType.None) {
                    chunkval = inval;
                    if (!tb2.BeSureRowIsLoaded(inval)) {
                        return (null, "Chunk konnte nicht geladen werden.", false);
                    }
                }
            }
        }

        //if (db2.Column.First is not { IsDisposed: false }) { return (null, "Tabelle hat keine erste Spalte, Systeminterner Fehler", false); }

        var m = tb2.IsNowNewRowPossible(chunkval, false);
        if (!string.IsNullOrEmpty(m)) { return (null, $"In der Tabelle sind keine neuen Zeilen möglich: {m}", true); }

        return GenerateAndAddInternal(s, filter, comment);
    }

    public RowItem? GenerateAndAdd(string valueOfCellInFirstColumn, string comment) {
        if (IsDisposed || Table is not { IsDisposed: false } db) { return null; }

        if (db.Column.First is not { IsDisposed: false } cf) { return null; }

        return GenerateAndAdd([new FilterItem(cf, FilterType.Istgleich, valueOfCellInFirstColumn)], comment).newrow;
    }

    public RowItem? GetByKey(string? key) {
        if (Table is not { IsDisposed: false } || key == null || string.IsNullOrWhiteSpace(key)) { return null; }
        try {
            var r = _internal.TryGetValue(key, out var value) ? value : null;
            if (r is { IsDisposed: true }) {
                Develop.DebugPrint(ErrorType.Error, "Interner Zeilenfehler: " + key);
                return null;
            }

            return r;
        } catch {
            Develop.CheckStackOverflow();
            return GetByKey(key);
        }
    }

    public IEnumerator<RowItem> GetEnumerator() => _internal.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _internal.Values.GetEnumerator();

    public bool HasPendingWorker() => Pendingworker.Count > 0;

    public void InvalidateAllCheckData() {
        foreach (var thisRow in this) {
            thisRow.InvalidateCheckData();
        }

        // Thread-sicherer Ansatz: Snapshot erstellen und dann versuchen zu entfernen
        // ToArray() erstellt eine Kopie zum Zeitpunkt des Aufrufs
        // TryRemove ist atomar und gibt einfach false zurück wenn Key nicht mehr existiert
        foreach (var kvp in FailedRows.ToArray()) {
            if (kvp.Key.Table == Table) {
                FailedRows.TryRemove(kvp.Key, out _);
            }
        }
    }

    /// <summary>
    /// Gibt Zeilen Zurück, die ein Update benötigen.
    /// wenn oldestTo=True ist, wird nach den dringenen Updates die älteste Zeile zurückgegeben, Benutzerunabhängig
    /// </summary>
    /// <param name="oldestTo"></param>
    /// <returns></returns>
    public RowItem? NextRowToCheck(bool oldestTo) {
        if (Table is not { IsDisposed: false } tbl) { return null; }

        if (!tbl.CanDoValueChangedScript(false)) { return null; }

        var rowToCheck = tbl.Row.FirstOrDefault(r => r.NeedsRowUpdate() && !FailedRows.ContainsKey(r) && r.IsMyRow(0.5, false));
        if (rowToCheck != null) { return rowToCheck; }

        rowToCheck = tbl.Row.FirstOrDefault(r => r.NeedsRowInitialization() && !FailedRows.ContainsKey(r) && r.IsMyRow(NewRowTolerance, oldestTo || !tbl.MultiUserPossible));
        if (rowToCheck != null) { return rowToCheck; }

        rowToCheck = tbl.Row.FirstOrDefault(r => r.NeedsRowUpdate() && !r.NeedsRowInitialization() && !FailedRows.ContainsKey(r) && r.IsMyRow(15, oldestTo || !tbl.MultiUserPossible));
        if (rowToCheck != null) { return rowToCheck; }

        if (!oldestTo) { return null; }

        if (tbl.Column.SysRowState is not { IsDisposed: false } srs) { return null; }
        var datefoundmax = new DateTime(2100, 1, 1);
        RowItem? foundrow = null;

        foreach (var thisRow in tbl.Row) {
            var dateofmyrow = thisRow.CellGetDateTime(srs);
            if (dateofmyrow < datefoundmax && !FailedRows.ContainsKey(thisRow) && thisRow.IsMyRow(15, true)) {
                datefoundmax = dateofmyrow;
                foundrow = thisRow;
            }
        }

        return foundrow;
    }

    public bool Remove(FilterItem fi, string comment) => Remove(FilterCollection.CalculateFilteredRows(Table, fi), comment);

    //public bool RemoveOlderThan(float inHours, string comment) {
    //    if (Table?.Column.SysRowCreateDate is not { IsDisposed: false } src) { return false; }

    //    var x = (from thisrowitem in _internal.Values where thisrowitem != null let d = thisrowitem.CellGetDateTime(src) where DateTime.UtcNow.Subtract(d).TotalHours > inHours select thisrowitem).ToList();
    //    //foreach (var thisrowitem in _Internal.Values)
    //    //{
    //    //    if (thisrowitem != null)
    //    //    {
    //    //        var D = thisrowitem.CellGetDateTime(table.Column.SysRowCreateDate());
    //    //        if (DateTime.UtcNow.Subtract(D).TotalHours > InHours) { x.GenerateAndAdd(thisrowitem.KeyName); }
    //    //    }
    //    //}
    //    if (x.Count == 0) { return false; }
    //    foreach (var thisKey in x) {
    //        _ = Remove(thisKey, comment);
    //    }
    //    return true;
    //}

    /// <summary>
    /// Löscht die Jüngste Zeile
    /// </summary>
    /// <param name="rows"></param>
    /// <param name="reduceToOne"></param>
    public void RemoveYoungest(ICollection<RowItem> rows, bool reduceToOne) {
        if (Table is not { IsDisposed: false } db) { return; }

        var l = rows.Distinct().ToList();

        if (l.Count < 2) { return; }

        #region Jüngste löschen

        var toDel = l.First();

        foreach (var thisR2 in l) {
            if (thisR2.CellGetDateTime(db.Column.SysRowCreateDate).Subtract(toDel.CellGetDateTime(db.Column.SysRowCreateDate)).TotalDays < 0) {
                toDel = thisR2;
            }
        }

        _ = Remove(toDel, "RowCleanUp");

        if (reduceToOne) {
            _ = l.Remove(toDel);
            if (l.Count > 1) { RemoveYoungest(l, true); }
        }

        #endregion
    }

    public (RowItem? newrow, string message, bool stoptrying) UniqueRow(string value, string comment) {
        // Used: Only BZL
        if (string.IsNullOrWhiteSpace(value)) { return (null, "Kein Initialwert angekommen", true); }

        if (Table is not { IsDisposed: false } db) { return (null, "Tabelle verworfen", true); }

        if (db.Column.First is not { IsDisposed: false } co) { return (null, "Spalte nicht vorhanden", true); }

        using var fic = new FilterCollection(db, "UnqiueRow");

        var fi = new FilterItem(co, FilterType.Istgleich_GroßKleinEgal, value);

        fic.Add(fi);

        return UniqueRow(fic, comment);
    }

    internal static List<RowItem> MatchesTo(FilterItem fi) {
        List<RowItem> l = [];

        if (fi.Table is not { IsDisposed: false } db) { return l; }

        l.AddRange(db.Row.Where(thisRow => thisRow.MatchesTo(fi)));
        return l;
    }

    internal string ExecuteCommand(TableDataType type, string rowkey, Reason reason, string? user, DateTime? datetimeutc) {
        if (IsDisposed || Table is not { IsDisposed: false } db) { return "Tabelle verworfen"; }

        if (type == TableDataType.Command_AddRow) {
            var row = GetByKey(rowkey);
            if (row is { IsDisposed: false }) { return string.Empty; } // "Zeile " + rowkey+ " bereits vorhanden!";

            row = new RowItem(db, rowkey);
            var f = Add(row, reason);

            if (string.IsNullOrEmpty(f) && user != null && datetimeutc is { } dt) {
                if (db.Column.SysRowCreator is { IsDisposed: false } src) { _ = row.SetValueInternal(src, user, reason); }
                if (db.Column.SysRowCreateDate is { IsDisposed: false } scd) { _ = row.SetValueInternal(scd, dt.ToString5(), reason); }
                if (db.Column.SysLocked is { IsDisposed: false } sl) { _ = row.SetValueInternal(sl, false.ToPlusMinus(), reason); }
                if (db.Column.SysCorrect is { IsDisposed: false } sc) { _ = row.SetValueInternal(sc, true.ToPlusMinus(), reason); }
            }

            if (reason == Reason.SetCommand && db.LogUndo) {
                Generic.Pause(0.001, false); // um in den Logs den Zeitstempel richtig zu haben
            }
            return f;
        }

        if (type == TableDataType.Command_RemoveRow) {
            var row = GetByKey(rowkey);
            if (row == null) { return "Zeile nicht gefunden!"; }

            if (reason != Reason.NoUndo_NoInvalidate) {
                OnRowRemoving(new RowEventArgs(row));
            }

            if (reason == Reason.SetCommand) {
                _ = row.ExecuteScript(ScriptEventTypes.row_deleting, string.Empty, true, 3, null, true, false);
            }

            foreach (var thisColumn in db.Column) {
                if (thisColumn != null) {
                    _ = row.SetValueInternal(thisColumn, string.Empty, Reason.NoUndo_NoInvalidate);
                }
            }

            if (!_internal.TryRemove(row.KeyName, out _)) { return "Löschen nicht erfolgreich"; }

            if (reason != Reason.NoUndo_NoInvalidate) {
                OnRowRemoved(new RowEventArgs(row));
            }
            return string.Empty;
        }

        return "Befehl unbekannt";
    }

    //    if (sourceTable.Row.Count != Count) {
    //        Develop.DebugPrint(ErrorType.Error, "Clone Fehlgeschlagen");
    //    }
    //}
    internal void RemoveNullOrEmpty() => _internal.RemoveNullOrEmpty();

    //    // Zeilen erzeugen und Format übertragen
    //    foreach (var thisRow in sourceTable.Row) {
    //        var l = GetByKey(thisRow.KeyName) ?? GenerateAndAdd(thisRow.KeyName, string.Empty, null, false, "Clone - Zeile fehlt");
    //        l.CloneFrom(thisRow, true);
    //    }
    internal void Repair() {
        foreach (var thisR in _internal) {
            thisR.Value.Repair();
        }
    }

    //    // Zeilen, die zu viel sind, löschen
    //    foreach (var thisRow in this) {
    //        var l = sourceTable.Row.GetByKey(thisRow.KeyName);
    //        if (l == null) { _ = Remove(thisRow, "Clone - Zeile zuviel"); }
    //    }
    //private static RowItem? OlderState(RowItem? row1, RowItem? row2) {
    //    if (row1 == null) { return row2; }
    //    if (row2 == null) { return row1; }

    //    if (row1.Table?.Column.SysRowState is not { IsDisposed: false } srs1 ||
    //        row2.Table?.Column.SysRowState is not { IsDisposed: false } srs2) {
    //        return Constants.GlobalRnd.Next(2) == 0 ? row1 : row2;
    //    }

    //    return row1.CellGetDateTime(srs1) < row2.CellGetDateTime(srs2) ? row1 : row2;
    //}

    //    var f = db.EditableErrorReason(EditableErrorReasonType.EditNormaly);
    //    if (!string.IsNullOrEmpty(f)) {
    //        Develop.DebugPrint(ErrorType.Error, "Neue Zeilen nicht möglich: " + f);
    //        throw new Exception();
    //    }
    private static void PendingWorker_DoWork(object sender, DoWorkEventArgs e) {
        if (e.Argument is not RowItem { IsDisposed: false } r) { return; }
        if (Table.ExecutingScriptThreadsAnyTable.Count > 0) { return; }

        _ = r.ExecuteScript(ScriptEventTypes.value_changed_extra_thread, string.Empty, true, 10, null, true, false);
    }

    //internal void CloneFrom(Table sourceTable) {
    //    if (IsDisposed || Table is not { IsDisposed: false } db) { return; }
    private static void PendingWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) => Pendingworker.Remove((BackgroundWorker)sender);

    private void _table_Disposing(object sender, System.EventArgs e) => Dispose();

    /// <summary>
    /// Fügt eine Zeile hinzu, die im System fest verankert ist.
    /// </summary>
    /// <param name="row"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    private string Add(RowItem row, Reason reason) {
        if (!_internal.TryAdd(row.KeyName, row)) { return "Hinzufügen fehlgeschlagen."; }

        if (reason != Reason.NoUndo_NoInvalidate) {
            OnRowAdded(new RowEventArgs(row));
            //if (Table?.Column.SysRowState != null) {
            //    _ = InvalidatedRowsManager.AddInvalidatedRow(row);
            //}
        }

        return string.Empty;
    }

    private void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                Table = null;
                foreach (var thisR in _internal) { thisR.Value.Dispose(); }
            }

            _internal.Clear();

            IsDisposed = true;
        }
    }

    /// <summary>
    /// REPARIERT: Interne Methode gibt jetzt Tupel zurück statt Exceptions zu werfen
    /// </summary>
    /// <param name="key"></param>
    /// <param name="fc"></param>
    /// <param name="comment"></param>
    /// <returns></returns>
    private (RowItem? newrow, string message, bool stoptrying) GenerateAndAddInternal(string key, FilterItem[] fc, string comment) {
        if (Table is not { IsDisposed: false } tb) { return (null, "Tabelle verworfen!", true); }

        if (!tb.IsEditable(false)) { return (null, "Neue Zeilen nicht möglich: " + tb.IsNotEditableReason, true); }

        var item = GetByKey(key);
        if (item != null) { return (null, "Schlüssel bereits belegt!", true); }

        // REPARIERT: Sichere Bestimmung des Chunk-Wertes vor der Zeilen-Erstellung
        var chunkvalue = string.Empty;
        List<ColumnItem> orderedColumns = [.. tb.Column];

        if (tb.Column.ChunkValueColumn is { IsDisposed: false } spc) {
            _ = orderedColumns.Remove(spc);
            orderedColumns.Insert(0, spc);
            chunkvalue = FilterCollection.InitValue(spc, true, false, fc) ?? string.Empty;

            // Chunk-Wert validieren bevor wir fortfahren
            if (string.IsNullOrEmpty(chunkvalue)) { return (null, "Chunk-Wert konnte nicht ermittelt werden", true); }
        }

        var u = Generic.UserName;
        var d = DateTime.UtcNow;

        // REPARIERT: Fehlerbehandlung für Zeilen-Erstellung
        var createResult = tb.ChangeData(TableDataType.Command_AddRow, null, null, string.Empty, key, u, d, comment, string.Empty, chunkvalue);
        if (!string.IsNullOrEmpty(createResult)) { return (null, $"Erstellung fehlgeschlagen: {createResult}", false); }

        item = GetByKey(key);
        if (item == null) { return (null, $"Erstellung fehlgeschlagen, Zeile nicht gefunden: {key}", false); }

        // REPARIERT: Sichere Setzung der Initial-Werte mit Fehlerbehandlung
        var initErrors = new List<string>();

        foreach (var thisColumn in orderedColumns) {
            var val = FilterCollection.InitValue(thisColumn, true, false, fc);
            if (val is { } && !string.IsNullOrWhiteSpace(val)) {
                try {
                    var cellResult = item.Table?.Cell.Set(thisColumn, item, val, "Initialwert neuer Zeile");
                    if (!string.IsNullOrEmpty(cellResult)) {
                        initErrors.Add($"Spalte {thisColumn.KeyName}: {cellResult}");
                    }
                } catch (Exception ex) {
                    initErrors.Add($"Spalte {thisColumn.KeyName}: Exception - {ex.Message}");
                }
            }
        }

        // REPARIERT: Bei kritischen Initialwert-Fehlern Zeile wieder löschen
        if (initErrors.Count > 0) {
            // Kritische Fehler - Zeile wieder entfernen
            _ = Remove(item, "Cleanup nach Initialwert-Fehler");
            return (null, $"Initialwert-Fehler: {string.Join("; ", initErrors)}", false);
        }

        Develop.Message?.Invoke(ErrorType.DevelopInfo, tb, tb.Caption, ImageCode.PlusZeichen, $"Neue Zeile erstellt: {tb.Caption}\\{item.CellFirstString()}", 0);

        var scriptResult = item.ExecuteScript(ScriptEventTypes.InitialValues, string.Empty, true, 0.1f, null, true, false);

        InvalidatedRowsManager.AddInvalidatedRow(item);

        if (scriptResult.Failed) {
            // Script-Fehler sind nicht kritisch, aber loggen
            return (item, $"InitialValues-Skript fehlgeschlagen für Zeile {key}: {scriptResult.FailedReason}", true);
        }

        return (item, string.Empty, true);
    }

    private void OnRowAdded(RowEventArgs e) {
        e.Row.RowChecked += OnRowChecked;
        RowAdded?.Invoke(this, e);
    }

    private void OnRowChecked(object sender, RowCheckedEventArgs e) => RowChecked?.Invoke(this, e);

    private void OnRowRemoved(RowEventArgs e) => RowRemoved?.Invoke(this, e);

    private void OnRowRemoving(RowEventArgs e) {
        e.Row.RowChecked -= OnRowChecked;
        RowRemoving?.Invoke(this, e);
    }

    #endregion
}