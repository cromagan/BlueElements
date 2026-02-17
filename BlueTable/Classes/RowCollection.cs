// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueBasics;
using BlueBasics.Classes;
using BlueBasics.ClassesStatic;
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
using System.Globalization;
using System.Linq;
using System.Threading;

namespace BlueTable.Classes;

public sealed class RowCollection : IEnumerable<RowItem>, IDisposableExtended, IHasTable {

    #region Fields

    public static readonly ConcurrentDictionary<RowItem, string> FailedRows = [];
    public static readonly InvalidatedRowsManager InvalidatedRowsManager = new();
    public static int WaitDelay;
    private static readonly List<BackgroundWorker> Pendingworker = [];
    private static int _executingchangedrows;

    private readonly ConcurrentDictionary<string, RowItem> _internal = [];

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

    public event EventHandler<RowPrepareFormulaEventArgs>? RowChecked;

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
        get;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == field) { return; }

            if (field != null) {
                field.DisposingEvent -= _table_Disposing;
            }
            field = value;

            if (field != null) {
                field.DisposingEvent += _table_Disposing;
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
        if (row.IsDisposed || row.Table is not { IsDisposed: false } tb) { return; }
        if (!tb.IsThisScriptBroken(ScriptEventTypes.value_changed_extra_thread, false)) { return; }
        if (!tb.IsRowScriptPossible()) { return; }
        var l = new BackgroundWorker {
            WorkerReportsProgress = true
        };
        l.RunWorkerCompleted += PendingWorker_RunWorkerCompleted;
        l.DoWork += PendingWorker_DoWork;

        Pendingworker.Add(l);
        l.RunWorkerAsync(row);

        Develop.Message(ErrorType.Info, tb, "Table", ImageCode.Blitz, "Hintergrund-Skript wird ausgeführt: " + row.CellFirstString(), 0);
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
                    if (DateTime.UtcNow.Subtract(Develop.LastUserActionUtc).TotalSeconds < 10) { break; }
                }

                if (Table.ExecutingScriptThreadsAnyTable.Count > 0) { break; }

                WaitDelay = Pendingworker.Count * 5;
                if (Pendingworker.Count > 2) { break; }

                if (!tbl.CanDoValueChangedScript(true)) { break; }

                var f = tbl.ExternalAbortScriptReasonExtended();
                if (!string.IsNullOrEmpty(f)) { return; }

                Develop.SetUserDidSomething();
                if (Table.ExecutingScriptThreadsAnyTable.Count > 0) { break; }
                row.UpdateRow(true, "Allgemeines Update (User Idle)");
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
    public static void GetUniques(ColumnItem column, ICollection<RowItem> rows, out List<string> unique, out List<string> notUnique) {
        unique = [];
        notUnique = [];
        HashSet<string> uniqueSet = [];

        foreach (var thisRow in rows) {
            if (thisRow is { IsDisposed: false }) {
                var values = column.MultiLine ? thisRow.CellGetList(column) : [thisRow.CellGetString(column)];
                foreach (var value in values) {
                    if (!uniqueSet.Add(value)) {
                        notUnique.AddIfNotExists(value);
                    } else {
                        unique.Add(value);
                    }
                }
            }
        }
        unique = [.. unique.Except(notUnique)];
    }

    public static bool HasPendingWorker() => Pendingworker.Count > 0;

    /// <summary>
    /// Prüft alle Tabellen im Speicher und gibt die dringenste Update-Aufgabe aller Tabellen zurück.
    /// </summary>
    /// <returns></returns>
    public static RowItem? NextRowToCeck() {
        try {
            List<Table> allfiles = [.. Table.AllFiles];

            if (allfiles.Count == 0) { return null; }

            if (Constants.GlobalRnd.Next(10) == 1) {
                allfiles.Shuffle();
            } else {
                try {
                    allfiles = [.. allfiles.OrderByDescending(eintrag => eintrag?.LastUsedDate ?? DateTime.MinValue)];
                } catch {
                    return null;
                }
            }

            foreach (var thisTb in allfiles) {
                if (thisTb is { IsDisposed: false } tb) {
                    try {
                        if (!tb.CanDoValueChangedScript(false)) { continue; }

                        var rowToCheck = tb.Row?.NextRowToCheck(false);
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

    public static bool Remove(ICollection<RowItem>? rows, string comment) {
        if (rows == null || rows.Count == 0) { return false; }

        var did = false;

        Table? tb = null;

        rows = [.. rows.Distinct()];

        foreach (var thisr in rows) {
            tb ??= thisr.Table;

            if (tb != thisr.Table) {
                Develop.DebugPrint(ErrorType.Error, "Tabellen inkonsitent");
                return false;
            }
        }

        foreach (var thisRow in rows) {
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
        List<Table> allfiles = [.. Table.AllFiles];

        foreach (var thisTb in allfiles) {
            if (thisTb is { IsDisposed: false } tb) {
                if (!tb.CanDoValueChangedScript(false)) { continue; }
                tb.LoadTableRows(true, 30);
                r.AddRange(tb.Row);
            }
        }

        return [.. r.OrderBy(eintrag => eintrag.UrgencyUpdate)];
    }

    public static string UniqueKeyValue() {
        var x = 9999;
        do {
            x += 1;
            if (x > 99999) { Develop.DebugPrint(ErrorType.Error, "Unique ID konnte nicht erzeugt werden"); }

            var unique = ("X" + DateTime.UtcNow.ToString("mm.fff", CultureInfo.InvariantCulture) + x.ToString5()).RemoveChars(Constants.Char_DateiSonderZeichen + ".");
            var ok = true;

            foreach (var thisfile in Table.AllFiles) {
                var row = thisfile.Row[unique];
                if (row is { IsDisposed: false }) { ok = false; break; }
            }

            if (ok) { return unique; }
        } while (true);
    }

    public static OperationResult UniqueRow(FilterCollection filter, string coment) {
        if (filter.Table is not { IsDisposed: false } tb) { return OperationResult.Failed("Tabelle verworfen"); }

        if (filter.Count < 1) { return OperationResult.Failed("Kein Filter angekommen."); }

        var r = filter.Rows;

        if (r.Count > 10) {
            return OperationResult.Failed($"RowUnique gescheitert, da bereits zu viele Zeilen vorhanden sind: {filter.ReadableText()}");
        }

        if (r.Count == 0) {
            if (!tb.IsThisScriptBroken(ScriptEventTypes.InitialValues, true)) { return OperationResult.Failed($"In der Tabelle '{tb.Caption}' sind die Skripte defekt"); }

            if (filter.HasFilterToLinkedCell()) { return OperationResult.Failed($"Es kann keine neue Zeile mit einen Zeiger auf LinkedCell erstellt werden."); }
        }

        if (r.Count > 1) {
            if (!tb.IsThisScriptBroken(ScriptEventTypes.row_deleting, true)) { return OperationResult.Failed($"In der Tabelle '{tb.Caption}' sind die Skripte defekt"); }

            tb.Row.Combine(r);
            tb.Row.RemoveYoungest(r, true);

            r = filter.Rows;
            if (r.Count != 1) {
                return OperationResult.FailedRetryable($"RowUnique gescheitert, da bereits zu viele Zeilen vorhanden sind: {filter.ReadableText()}");
            }
            if (tb.Column.SysRowState is { IsDisposed: false } srs) {
                r[0].CellSet(srs, string.Empty, "'UniqueRow' Aufräumen mehrerer Zeilen.");
            }
        }

        RowItem? myRow;

        if (r.Count == 0) {
            if (filter.HasFilterToLinkedCell()) { return OperationResult.Failed($"Es kann keine neue Zeile mit einen Zeiger auf LinkedCell erstellt werden."); }
            var rg = tb.Row.GenerateAndAdd([.. filter], coment);
            if (rg.Value is not RowItem ngr) { return rg; }
            myRow = ngr;
        } else {
            myRow = r[0];
        }

        // REPARIERT: Finale Validierung dass die Zeile auch wirklich den Filtern entspricht
        if (!myRow.MatchesTo([.. filter])) {
            return OperationResult.Failed("RowUnique mit falschen Werten initialisiert");
        }

        return OperationResult.SuccessValue(myRow);
    }

    public bool Clear(string comment) {
        using var fc = new FilterCollection(Table, "rowcol clear");
        return Remove(fc.Rows, comment);
    }

    /// <summary>
    /// Alle Angegebenen Zeilen werden die gleichen Werte erhalten.
    /// </summary>
    /// <param name="rows"></param>
    public void Combine(ICollection<RowItem> rows) {
        if (rows.Count < 2) { return; }

        if (Table is not { IsDisposed: false } tb) { return; }

        #region Leere Werte befüllen

        foreach (var thisC in tb.Column) {

            #region neuen Wert zum Reinschreiben ermitteln (Wert)

            var wert = string.Empty;
            foreach (var thisR2 in rows) {
                if (thisR2.Table != tb) { return; }

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
    public OperationResult GenerateAndAdd(FilterItem[] filter, string comment) {
        Table? tb2 = null;

        foreach (var thisfi in filter) {
            if (thisfi.Table is not { IsDisposed: false } tb1) { return OperationResult.Failed("Tabelle eines Filters nicht angegeben"); }
            tb2 ??= tb1;

            if (thisfi.Column?.Table is { } tb3 && tb3 != tb2) { return OperationResult.Failed("Tabellen der Spalten im Filter unterschiedlich"); }
        }

        if (tb2 is not { IsDisposed: false }) { return OperationResult.Failed("Tabellen verworfen"); }

        var s = tb2.NextRowKey();
        if (string.IsNullOrEmpty(s)) { return OperationResult.FailedRetryable("Fehler beim Zeilenschlüssel erstellen, Systeminterner Fehler"); }

        var chunkval = string.Empty;
        foreach (var thisColum in tb2.Column) {
            if (thisColum.IsFirst || thisColum.Value_for_Chunk != ChunkType.None) {
                if (FilterCollection.InitValue(thisColum, true, false, filter) is not { } inval || string.IsNullOrWhiteSpace(inval)) {
                    return OperationResult.Failed($"Initialwert der Spalte '{thisColum.KeyName}' der Tabelle '{tb2.KeyName}' fehlt.");
                }

                if (thisColum.Value_for_Chunk != ChunkType.None) {
                    chunkval = inval;
                    if (!tb2.BeSureRowIsLoaded(inval)) {
                        return OperationResult.FailedRetryable("Chunk konnte nicht geladen werden.");
                    }
                }
            }
        }

        //if (db2.Column.First is not { IsDisposed: false }) { return (null, "Tabelle hat keine erste Spalte, Systeminterner Fehler", false); }

        var m = tb2.IsNowNewRowPossible(chunkval, false);
        if (!string.IsNullOrEmpty(m)) { return OperationResult.Failed($"In der Tabelle sind keine neuen Zeilen möglich: {m}"); }

        return GenerateAndAddInternal(s, filter, comment);
    }

    public RowItem? GenerateAndAdd(string valueOfCellInFirstColumn, string comment) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return null; }

        if (tb.Column.First is not { IsDisposed: false } cf) { return null; }

        return GenerateAndAdd([new FilterItem(cf, FilterType.Istgleich, valueOfCellInFirstColumn)], comment).Value as RowItem;
    }

    public RowItem? GetByKey(string? key) {
        if (Table is not { IsDisposed: false } || string.IsNullOrWhiteSpace(key)) { return null; }
        try {
            var r = _internal.TryGetValue(key, out var value) ? value : null;
            if (r is { IsDisposed: true }) {
                Develop.DebugPrint(ErrorType.Error, "Interner Zeilenfehler: " + key);
                return null;
            }

            return r;
        } catch {
            Develop.AbortAppIfStackOverflow();
            return GetByKey(key);
        }
    }

    public IEnumerator<RowItem> GetEnumerator() => _internal.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _internal.Values.GetEnumerator();

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
        if (Table is not { IsDisposed: false } tb) { return null; }

        if (!tb.CanDoValueChangedScript(false)) { return null; }

        var rowToCheck = tb.Row.FirstOrDefault(r => r.NeedsRowUpdate() && !FailedRows.ContainsKey(r) && r.IsMyRow(0.5, false));
        if (rowToCheck != null) { return rowToCheck; }

        rowToCheck = tb.Row.FirstOrDefault(r => r.NeedsRowInitialization() && !FailedRows.ContainsKey(r) && r.IsMyRow(NewRowTolerance, oldestTo || !tb.MultiUserPossible));
        if (rowToCheck != null) { return rowToCheck; }

        rowToCheck = tb.Row.FirstOrDefault(r => r.NeedsRowUpdate() && !r.NeedsRowInitialization() && !FailedRows.ContainsKey(r) && r.IsMyRow(15, oldestTo || !tb.MultiUserPossible));
        if (rowToCheck != null) { return rowToCheck; }

        if (!oldestTo) { return null; }

        if (tb.Column.SysRowState is not { IsDisposed: false } srs) { return null; }
        var datefoundmax = new DateTime(2100, 1, 1);
        RowItem? foundrow = null;

        foreach (var thisRow in tb.Row) {
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
    //        Remove(thisKey, comment);
    //    }
    //    return true;
    //}

    /// <summary>
    /// Löscht die Jüngste Zeile
    /// </summary>
    /// <param name="rows"></param>
    /// <param name="reduceToOne"></param>
    public void RemoveYoungest(ICollection<RowItem> rows, bool reduceToOne) {
        if (Table is not { IsDisposed: false } tb) { return; }

        var l = rows.Distinct().ToList();

        if (l.Count < 2) { return; }

        #region Jüngste löschen

        var toDel = l.First();

        foreach (var thisR2 in l) {
            if (thisR2.CellGetDateTime(tb.Column.SysRowCreateDate).Subtract(toDel.CellGetDateTime(tb.Column.SysRowCreateDate)).TotalDays < 0) {
                toDel = thisR2;
            }
        }

        Remove(toDel, "RowCleanUp");

        if (reduceToOne) {
            l.Remove(toDel);
            if (l.Count > 1) { RemoveYoungest(l, true); }
        }

        #endregion
    }

    public OperationResult UniqueRow(string value, string comment) {
        if (string.IsNullOrWhiteSpace(value)) { return OperationResult.Failed("Kein Initialwert angekommen"); }

        if (Table is not { IsDisposed: false } tb) { return OperationResult.Failed("Tabelle verworfen"); }

        if (tb.Column.First is not { IsDisposed: false } co) { return OperationResult.Failed("Spalte nicht vorhanden"); }

        using var fic = new FilterCollection(tb, "UnqiueRow");

        var fi = new FilterItem(co, FilterType.Istgleich_GroßKleinEgal, value);

        fic.Add(fi);

        return UniqueRow(fic, comment);
    }

    internal static List<RowItem> MatchesTo(FilterItem fi) {
        List<RowItem> l = [];

        if (fi.Table is not { IsDisposed: false } tb) { return l; }

        l.AddRange(tb.Row.Where(thisRow => thisRow.MatchesTo(fi)));
        return l;
    }

    internal string ExecuteCommand(TableDataType type, string rowkey, Reason reason, string? user, DateTime? datetimeutc) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return "Tabelle verworfen"; }

        if (type == TableDataType.Command_AddRow) {
            var row = GetByKey(rowkey);
            if (row is { IsDisposed: false }) { return string.Empty; } // "Zeile " + rowkey+ " bereits vorhanden!";

            row = new RowItem(tb, rowkey);
            var f = Add(row, reason);

            if (string.IsNullOrEmpty(f) && user != null && datetimeutc is { } dt) {
                if (tb.Column.SysRowCreator is { IsDisposed: false } src) { row.SetValueInternal(src, user, reason); }
                if (tb.Column.SysRowCreateDate is { IsDisposed: false } scd) { row.SetValueInternal(scd, dt.ToString5(), reason); }
                if (tb.Column.SysLocked is { IsDisposed: false } sl) { row.SetValueInternal(sl, false.ToPlusMinus(), reason); }
                if (tb.Column.SysCorrect is { IsDisposed: false } sc) { row.SetValueInternal(sc, true.ToPlusMinus(), reason); }
            }

            if (reason == Reason.SetCommand && tb.LogUndo) {
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
                row.ExecuteScript(ScriptEventTypes.row_deleting, string.Empty, true, 3, null, true, false);
            }

            foreach (var thisColumn in tb.Column) {
                if (thisColumn != null) {
                    row.SetValueInternal(thisColumn, string.Empty, Reason.NoUndo_NoInvalidate);
                }
            }

            if (!_internal.TryRemove(row.KeyName, out _)) { return "Löschen nicht erfolgreich"; }

            row.Dispose();

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
    //        if (l == null) { Remove(thisRow, "Clone - Zeile zuviel"); }
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

    //    var f = tb.EditableErrorReason(EditableErrorReasonType.EditNormaly);
    //    if (!string.IsNullOrEmpty(f)) {
    //        Develop.DebugPrint(ErrorType.Error, "Neue Zeilen nicht möglich: " + f);
    //        throw new Exception();
    //    }
    private static void PendingWorker_DoWork(object sender, DoWorkEventArgs e) {
        if (e.Argument is not RowItem { IsDisposed: false } r) { return; }
        if (Table.ExecutingScriptThreadsAnyTable.Count > 0) { return; }

        r.ExecuteScript(ScriptEventTypes.value_changed_extra_thread, string.Empty, true, 10, null, true, false);
    }

    //internal void CloneFrom(Table sourceTable) {
    //    if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }
    private static void PendingWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) => Pendingworker.Remove((BackgroundWorker)sender);

    private void _table_Disposing(object sender, System.EventArgs e) => Dispose();

    /// <summary>
    /// Fügt eine Zeile hinzu, die im System fest verankert ist.
    /// </summary>
    /// <param name="row"></param>
    /// <param name="reason"></param>
    /// <returns>Einen Text, warum das Hinzufügen fehlgeschlagen ist</returns>
    private string Add(RowItem row, Reason reason) {
        if (!_internal.TryAdd(row.KeyName, row)) { return "Hinzufügen fehlgeschlagen."; }

        if (reason != Reason.NoUndo_NoInvalidate) {
            OnRowAdded(new RowEventArgs(row));
            //if (Table?.Column.SysRowState != null) {
            //    InvalidatedRowsManager.AddInvalidatedRow(row);
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
    private OperationResult GenerateAndAddInternal(string key, FilterItem[] fc, string comment) {
        if (Table is not { IsDisposed: false } tb) { return OperationResult.Failed("Tabelle verworfen!"); }

        if (!tb.IsEditable(false)) { return OperationResult.Failed($"Neue Zeilen nicht möglich: {tb.IsNotEditableReason(false)}"); }

        if (GetByKey(key) != null) { return OperationResult.Failed("Schlüssel bereits belegt!"); }

        // REPARIERT: Sichere Bestimmung des Chunk-Wertes vor der Zeilen-Erstellung
        var chunkvalue = string.Empty;
        List<ColumnItem> orderedColumns = [.. tb.Column];

        if (tb.Column.ChunkValueColumn is { IsDisposed: false } spc) {
            orderedColumns.Remove(spc);
            orderedColumns.Insert(0, spc);
            chunkvalue = FilterCollection.InitValue(spc, true, false, fc) ?? string.Empty;

            // Chunk-Wert validieren bevor wir fortfahren
            if (string.IsNullOrEmpty(chunkvalue)) { return OperationResult.Failed("Chunk-Wert konnte nicht ermittelt werden"); }
        }

        var u = Generic.UserName;
        var d = DateTime.UtcNow;

        // REPARIERT: Fehlerbehandlung für Zeilen-Erstellung
        var createResult = tb.ChangeData(TableDataType.Command_AddRow, null, null, string.Empty, key, u, d, comment, string.Empty, chunkvalue);
        if (!string.IsNullOrEmpty(createResult)) { return OperationResult.FailedRetryable($"Erstellung fehlgeschlagen: {createResult}"); }

        if (GetByKey(key) is not { } nRow) { return OperationResult.FailedRetryable($"Erstellung fehlgeschlagen, Zeile nicht gefunden: {key}"); }

        // REPARIERT: Sichere Setzung der Initial-Werte mit Fehlerbehandlung
        var initErrors = new List<string>();

        foreach (var thisColumn in orderedColumns) {
            if (FilterCollection.InitValue(thisColumn, true, false, fc) is { } val && !string.IsNullOrWhiteSpace(val)) {
                try {
                    var cellResult = nRow.Set(thisColumn, val, "Initialwert neuer Zeile");
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
            Remove(nRow, "Cleanup nach Initialwert-Fehler");
            return OperationResult.FailedRetryable($"Initialwert-Fehler: {string.Join("; ", initErrors)}");
        }

        Develop.Message(ErrorType.DevelopInfo, tb, tb.Caption, ImageCode.PlusZeichen, $"Neue Zeile erstellt: {tb.Caption}\\{nRow.CellFirstString()}", 0);

        nRow.ExecuteScript(ScriptEventTypes.InitialValues, string.Empty, true, 0.1f, null, true, false);

        InvalidatedRowsManager.AddInvalidatedRow(nRow);

        //if (scriptResult.Failed) {
        //    // Script-Fehler sind nicht kritisch, aber loggen
        //    return (nRow, $"InitialValues-Skript fehlgeschlagen für Zeile {key}: {scriptResult.FailedReason}", true);
        //}

        return OperationResult.SuccessValue(nRow);
    }

    private void OnRowAdded(RowEventArgs e) {
        e.Row.RowChecked += OnRowChecked;
        RowAdded?.Invoke(this, e);
    }

    private void OnRowChecked(object sender, RowPrepareFormulaEventArgs e) => RowChecked?.Invoke(this, e);

    private void OnRowRemoved(RowEventArgs e) => RowRemoved?.Invoke(this, e);

    private void OnRowRemoving(RowEventArgs e) {
        e.Row.RowChecked -= OnRowChecked;
        RowRemoving?.Invoke(this, e);
    }

    #endregion
}