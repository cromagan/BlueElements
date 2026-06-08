// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueTable.EventArgs;
using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace BlueTable.Classes;

public sealed class RowCollection : IEnumerable<RowItem>, IDisposableExtended, IHasTable {

    #region Fields

    public static readonly ConcurrentDictionary<RowItem, string> FailedRows = [];

    public static readonly InvalidatedRowsManager InvalidatedRowsManager = new();

    private static readonly List<BackgroundWorker> Pendingworker = [];

    private static int _executingchangedrows;

    private readonly ConcurrentDictionary<string, RowItem> _internal = [];
    private volatile int _isDisposedFlag;

    #endregion

    #region Constructors

    public RowCollection(Table table) => Table = table;

    #endregion

    #region Destructors

    ~RowCollection() {
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

    public static int WaitDelay { get; set; }
    public int Count => _internal.Count;

    public bool IsDisposed => _isDisposedFlag == 1;

    public Table? Table {
        get;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == field) { return; }

            field?.DisposingEvent -= _table_Disposing;
            field = value;

            field?.DisposingEvent += _table_Disposing;
        }
    }

    #endregion

    #region Indexers

    /// <summary>
    /// Durchsucht die erste (interne) Spalte der Tabelle nach dem hier angegebenen Primärschlüssel.
    /// </summary>
    /// <returns>Die Zeile, dessen erste Spalte den Primärschlüssel enthält oder - falls nicht gefunden - NULL.</returns>
    public RowItem? this[string primärSchlüssel] {
        get {
            if (primärSchlüssel is not { Length: > 0 }) { return null; }
            if (Table is not { IsDisposed: false }) { return null; }
            if (Table.Column.First is not { IsDisposed: false } c) { return null; }

            if (c.Value_for_Chunk != ChunkType.None) {
                if (!Table.BeSureRowIsLoaded(primärSchlüssel)) { return null; }
            }

            foreach (var thisRow in _internal.Values) {
                if (thisRow is null) { continue; }
                var cellValue = thisRow.CellGetStringCore(c);
                if (string.Equals(cellValue, primärSchlüssel, StringComparison.OrdinalIgnoreCase)) {
                    return thisRow;
                }
            }
            return null;
        }
    }

    public RowItem? this[params FilterItem[] filter] {
        get {
            var f = FilterCollection.CalculateFilteredRows(Table, filter);

            //var parallelQuery = _internal.Values.AsParallel()
            //                                    .Where(thisRow => thisRow is not null)
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
        List<Table> l;
        try {
            l = Table.AllFiles.ToList(); // Explizites ToList() ist oft stabiler als der Spread-Operator bei Multithreading
            l = l.Where(x => x is not null).OrderByDescending(eintrag => eintrag.LastUsedDate).ToList();
        } catch {
            Develop.AbortAppIfStackOverflow();
            ExecuteValueChangedEvent();
            return; // Liste wurde während des Kopierens modifiziert
        }

        // Lock-freie Implementierung mit Interlocked für bessere Performance und Deadlock-Vermeidung
        if (Interlocked.CompareExchange(ref _executingchangedrows, 1, 0) != 0) {
            return; // Bereits in Ausführung
        }

        try {
            if (l.Count == 0) { return; }
            var tim = Stopwatch.StartNew();

            while (NextRowToCeck() is { IsDisposed: false } row) {
                if (row.IsDisposed || row.Table is not { IsDisposed: false } tbl) { break; }

                if (tbl.ChangedScriptMayAffectUser) {
                    if (l.Count > 0 && row.Table == l[0]) {
                        if (Develop.GetUserIdleSeconds() < 1) { break; }
                    } else {
                        if (Develop.GetUserIdleSeconds() < 10) { break; }
                    }
                }

                if (Table.ExecutingScriptThreadsAnyTable.Count > 0) { break; }

                WaitDelay = Pendingworker.Count * 5;
                if (Pendingworker.Count > 2) { break; }

                if (!tbl.CanDoValueChangedScript(true)) { break; }

                var f = tbl.ExternalAbortScriptReasonExtended();
                if (!string.IsNullOrEmpty(f)) { break; }

                if (Table.ExecutingScriptThreadsAnyTable.Count > 0) { break; }
                row.UpdateRow(true, "Allgemeines Update (User Idle)");
                if (Develop.GetUserIdleSeconds() < 1) { break; }
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
                        if (rowToCheck is not null) { return rowToCheck; }
                    } catch (ObjectDisposedException) {
                        // Table disposed während Verwendung - skip
                        continue;
                    }
                }
            }
        } catch { /* NextRowToCeck: Fehler beim Abrufen der nächsten Zeile wird ignoriert */ }

        return null;
    }

    public static OperationResult Remove(ICollection<RowItem>? rows, string comment) {
        if (rows is null || rows.Count == 0) { return OperationResult.SuccessFalse; }

        var did = false;

        Table? tb = null;

        rows = [.. rows.Distinct()];

        foreach (var thisr in rows) {
            tb ??= thisr.Table;

            if (tb != thisr.Table) {
                Develop.DebugError("Tabellen inkonsitent");
                return OperationResult.Failed("Tabellen inkonsitent");
            }
        }

        foreach (var thisRow in rows) {
            var f = Remove(thisRow, comment);
            if (f.IsFailed) { return f; }

            did = did || f.ValueTrue;
        }
        return new OperationResult(did);
    }

    public static OperationResult Remove(RowItem? row, string comment) {
        if (row is not { IsDisposed: false } r) {
            return OperationResult.SuccessFalse;
        }

        var result = r.Table?.ChangeData(TableDataType.Command_RemoveRow, null, r, string.Empty, r.KeyName, Generic.UserName, DateTime.UtcNow, comment);

        return string.IsNullOrEmpty(result) ? OperationResult.SuccessTrue : OperationResult.Failed(result);
    }

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
            if (x > 99999) { Develop.DebugError("Unique ID konnte nicht erzeugt werden"); }

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
            if (!ngr.MatchesTo([.. filter])) { return OperationResult.Failed("RowUnique: InitialValues-Skript hat Zeile verändert, Filter passt nicht mehr"); }
            myRow = ngr;
        } else {
            myRow = r[0];
        }

        return OperationResult.SuccessValue(myRow);
    }

    public OperationResult Clear(string comment) {
        using var fc = new FilterCollection(Table, "rowcol clear");
        return Remove(fc.Rows, comment);
    }

    /// <summary>
    /// Alle Angegebenen Zeilen werden die gleichen Werte erhalten.
    /// </summary>
    /// <param name="rows"></param>
    public OperationResult Combine(ICollection<RowItem> rows) {
        if (rows.Count < 2) { return OperationResult.SuccessFalse; }

        if (Table is not { IsDisposed: false } tb) { return OperationResult.Failed("Tabelle verworfen"); }

        #region Leere Werte befüllen

        foreach (var thisC in tb.Column) {

            #region neuen Wert zum Reinschreiben ermitteln (Wert)

            var wert = string.Empty;
            foreach (var thisR2 in rows) {
                if (thisR2.Table != tb) { return OperationResult.Failed("Tabellen inkonsistent"); }

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

        return OperationResult.SuccessTrue;
    }

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

            if (thisfi.Column?.Table is { IsDisposed: false } tb3 && tb3 != tb2) { return OperationResult.Failed("Tabellen der Spalten im Filter unterschiedlich"); }
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
                Develop.DebugError("Interner Zeilenfehler: " + key);
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
        try {
            Parallel.ForEach(this, row => row.InvalidateCheckData());
        } catch {
            Develop.AbortAppIfStackOverflow();
            InvalidateAllCheckData();
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
        if (rowToCheck is not null) { return rowToCheck; }

        rowToCheck = tb.Row.FirstOrDefault(r => r.NeedsRowInitialization() && !FailedRows.ContainsKey(r) && r.IsMyRow(NewRowTolerance, oldestTo || !tb.MultiUserPossible));
        if (rowToCheck is not null) { return rowToCheck; }

        rowToCheck = tb.Row.FirstOrDefault(r => r.NeedsRowUpdate() && !r.NeedsRowInitialization() && !FailedRows.ContainsKey(r) && r.IsMyRow(15, oldestTo || !tb.MultiUserPossible));
        if (rowToCheck is not null) { return rowToCheck; }

        if (!oldestTo) { return null; }

        if (tb.Column.SysRowState is not { IsDisposed: false } srs) { return null; }
        var datefoundmax = new DateTime(2100, 1, 1, 0, 0, 0, DateTimeKind.Utc);
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

    public OperationResult Remove(FilterItem fi, string comment) {
        //TODO: unbenutzt
        return Remove(FilterCollection.CalculateFilteredRows(Table, fi), comment);
    }

    public OperationResult RemoveObsoleteRows(IEnumerable<RowItem> posssibleObsoelte, HashSet<string> stillused, Reason reason) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return OperationResult.Failed("Tabelle verworfen"); }

        var rowsToRemove = posssibleObsoelte.Where(r => !r.IsDisposed && !stillused.Contains(r.KeyName)).ToList();
        if (rowsToRemove.Count > 0) {
            foreach (var row in rowsToRemove) {
                ExecuteCommand(TableDataType.Command_RemoveRow, row.KeyName, reason, null, null);
            }

            tb.Cell.RemoveOrphans();
        }

        return OperationResult.Success;
    }

    //    var x = (from thisrowitem in _internal.Values where thisrowitem is not null let d = thisrowitem.CellGetDateTime(src) where DateTime.UtcNow.Subtract(d).TotalHours > inHours select thisrowitem).ToList();
    //    //foreach (var thisrowitem in _Internal.Values)
    //    //{
    //    //    if (thisrowitem is not null)
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
    public OperationResult RemoveYoungest(ICollection<RowItem> rows, bool reduceToOne) {
        if (Table is not { IsDisposed: false } tb) { return OperationResult.Failed("Tabelle verworfen"); }

        var l = rows.Distinct().ToList();

        if (l.Count < 2) { return OperationResult.SuccessFalse; }

        #region Jüngste löschen

        var toDel = l[0];

        if (tb.Column.SysRowCreateDate is { } srcd) {
            foreach (var thisR2 in l) {
                if (thisR2.CellGetDateTime(srcd).Subtract(toDel.CellGetDateTime(srcd)).TotalDays < 0) {
                    toDel = thisR2;
                }
            }
        }

        var f = Remove(toDel, "RowCleanUp");
        if (f.IsFailed) { return f; }

        if (reduceToOne) {
            l.Remove(toDel);
            if (l.Count > 1) { RemoveYoungest(l, true); }
        }

        return OperationResult.SuccessTrue;

        #endregion
    }

    public OperationResult UniqueRow(string value, string comment) {
        //TODO: unbenutzt
        if (string.IsNullOrWhiteSpace(value)) { return OperationResult.Failed("Kein Initialwert angekommen"); }

        if (Table is not { IsDisposed: false } tb) { return OperationResult.Failed("Tabelle verworfen"); }

        if (tb.Column.First is not { IsDisposed: false } co) { return OperationResult.Failed("Spalte nicht vorhanden"); }

        using var fic = new FilterCollection(tb, "UnqiueRow");

        var fi = new FilterItem(co, FilterType.Istgleich_GroßKleinEgal, value);

        fic.Add(fi);

        return UniqueRow(fic, comment);
    }

    internal void CopyTo(RowCollection target) {
        if (target.Count > 0) {
            Remove(target.ToList(), "CopyTo - alte Zeilen entfernen");
        }

        if (target.Table is not { IsDisposed: false } tb) { return; }

        foreach (var sourceRow in this) {
            if (sourceRow is not { IsDisposed: false }) { continue; }

            var key = tb.NextRowKey();
            if (string.IsNullOrEmpty(key)) { continue; }

            var targetRow = new RowItem(tb, key);
            if (!target._internal.TryAdd(targetRow.KeyName, targetRow)) { continue; }

            sourceRow.CopyTo(targetRow, tb.Column);
        }
    }

    internal OperationResult ExecuteCommand(TableDataType type, string rowkey, Reason reason, string? user, DateTime? datetimeutc) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return OperationResult.Failed("Tabelle verworfen"); }

        if (type == TableDataType.Command_AddRow) {
            var row = GetByKey(rowkey);

            if (row is { IsDisposed: false }) { return OperationResult.SuccessFalse; }

            row = new RowItem(tb, rowkey);

            if (!_internal.TryAdd(row.KeyName, row)) { return OperationResult.Failed("Hinzufügen fehlgeschlagen."); }

            if (reason.HasFlag(Reason.RaiseEvents)) {
                OnRowAdded(new RowEventArgs(row));
            }

            if (user is not null && datetimeutc is { } dt && reason.HasFlag(Reason.DoRepair)) {
                if (tb.Column.SysRowCreator is { IsDisposed: false } src) { row.CellSetInMemory(src, user); }
                if (tb.Column.SysRowCreateDate is { IsDisposed: false } scd) { row.CellSetInMemory(scd, dt.ToString5()); }
                if (tb.Column.SysLocked is { IsDisposed: false } sl) { row.CellSetInMemory(sl, false.ToPlusMinus()); }
                if (tb.Column.SysCorrect is { IsDisposed: false } sc) { row.CellSetInMemory(sc, true.ToPlusMinus()); }
                if (tb.Column.SysRowKey is { IsDisposed: false } srk) { row.CellSetInMemory(srk, rowkey); }
            }

            if (reason.HasFlag(Reason.LogUndo) && tb.LogUndo) {
                Generic.Pause(0.001, false);
            }
            return OperationResult.SuccessTrue;
        }

        if (type == TableDataType.Command_RemoveRow) {
            var row = GetByKey(rowkey);
            if (row is null) { return OperationResult.Failed("Zeile nicht gefunden!"); }

            if (reason.HasFlag(Reason.RaiseEvents)) { OnRowRemoving(new RowEventArgs(row)); }

            if (reason.HasFlag(Reason.DoRepair)) {
                tb.ExecuteScript(ScriptEventTypes.row_deleting, string.Empty, true, row, null, true, false, 3);
            }

            foreach (var thisColumn in tb.Column) {
                if (thisColumn is not null) {
                    row.CellSetInMemory(thisColumn, string.Empty);
                }
            }

            if (!_internal.TryRemove(row.KeyName, out _)) { return OperationResult.Failed("Löschen nicht erfolgreich"); }

            row.Dispose();

            if (reason.HasFlag(Reason.RaiseEvents)) { OnRowRemoved(new RowEventArgs(row)); }
            return OperationResult.SuccessTrue;
        }

        return OperationResult.Failed("Befehl unbekannt");
    }

    internal void RemoveNullOrEmpty() => _internal.RemoveNullOrEmpty();

    internal void Repair() {
        foreach (var thisR in _internal) {
            thisR.Value.Repair();
        }
    }

    private static void PendingWorker_DoWork(object? sender, DoWorkEventArgs e) {
        if (e.Argument is not RowItem { IsDisposed: false } r) { return; }
        if (Table.ExecutingScriptThreadsAnyTable.Count > 0) { return; }

        r.Table?.ExecuteScript(ScriptEventTypes.value_changed_extra_thread, string.Empty, true, r, null, true, false, 10);
    }

    private static void PendingWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e) {
        if (sender is BackgroundWorker bw) { Pendingworker.Remove(bw); }
    }

    private void _table_Disposing(object? sender, System.EventArgs e) => Dispose();

    private void Dispose(bool disposing) {
        if (Interlocked.CompareExchange(ref _isDisposedFlag, 1, 0) != 0) { return; }

        if (disposing) {
            Table = null;
            foreach (var thisR in _internal) { thisR.Value.Dispose(); }
            RowAdded = null;
            RowChecked = null;
            RowRemoved = null;
            RowRemoving = null;
        }

        try {
            _internal.Clear();
        } catch { }
    }

    /// <summary>
    /// Interne Methode gibt jetzt Tupel zurück statt Exceptions zu werfen
    /// </summary>
    /// <param name="key"></param>
    /// <param name="fc"></param>
    /// <param name="comment"></param>
    /// <returns></returns>
    private OperationResult GenerateAndAddInternal(string key, FilterItem[] fc, string comment) {
        if (Table is not { IsDisposed: false } tb) { return OperationResult.Failed("Tabelle verworfen!"); }

        if (GetByKey(key) is not null) { return OperationResult.Failed("Schlüssel bereits belegt!"); }

        // Sichere Bestimmung des Chunk-Wertes vor der Zeilen-Erstellung
        var chunkvalue = string.Empty;
        List<ColumnItem> orderedColumns = [.. tb.Column];

        if (tb.Column.ChunkValueColumn is { IsDisposed: false } spc) {
            orderedColumns.Remove(spc);
            orderedColumns.Insert(0, spc);
            chunkvalue = FilterCollection.InitValue(spc, true, false, fc) ?? string.Empty;

            // Chunk-Wert validieren bevor wir fortfahren
            if (string.IsNullOrEmpty(chunkvalue)) { return OperationResult.Failed("Chunk-Wert konnte nicht ermittelt werden"); }

            var f = tb.AcquireWriteAccess(TableDataType.Command_AddRow, chunkvalue);
            if (!string.IsNullOrEmpty(f)) { return OperationResult.Failed($"Neue Zeilen nicht möglich: {f}"); }
        }

        var u = Generic.UserName;
        var d = DateTime.UtcNow;

        // Fehlerbehandlung für Zeilen-Erstellung
        var createResult = tb.ChangeData(TableDataType.Command_AddRow, null, null, string.Empty, key, u, d, comment);
        if (!string.IsNullOrEmpty(createResult)) { return OperationResult.FailedRetryable($"Erstellung fehlgeschlagen: {createResult}"); }

        if (GetByKey(key) is not { } nRow) { return OperationResult.FailedRetryable($"Erstellung fehlgeschlagen, Zeile nicht gefunden: {key}"); }

        // Sichere Setzung der Initial-Werte mit Fehlerbehandlung
        var initErrors = new List<string>();

        foreach (var thisColumn in orderedColumns) {
            if (FilterCollection.InitValue(thisColumn, true, false, fc) is { } val && !string.IsNullOrWhiteSpace(val)) {
                try {
                    var cellResult = nRow.CellSet(thisColumn, val, "Initialwert neuer Zeile");
                    if (!string.IsNullOrEmpty(cellResult)) {
                        initErrors.Add($"Spalte {thisColumn.KeyName}: {cellResult}");
                    }
                } catch (Exception ex) {
                    initErrors.Add($"Spalte {thisColumn.KeyName}: Exception - {ex.Message}");
                }
            }
        }

        // Bei kritischen Initialwert-Fehlern Zeile wieder löschen
        if (initErrors.Count > 0) {
            // Kritische Fehler - Zeile wieder entfernen
            Remove(nRow, "Cleanup nach Initialwert-Fehler");
            return OperationResult.FailedRetryable($"Initialwert-Fehler: {string.Join("; ", initErrors)}");
        }

        Develop.Message(ErrorType.DevelopInfo, tb, tb.Caption, ImageCode.PlusZeichen, $"Neue Zeile erstellt: {tb.Caption}\\{nRow.CellFirstString()}", 0);

        tb.ExecuteScript(ScriptEventTypes.InitialValues, string.Empty, true, nRow, null, true, false, 0.1f);

        InvalidatedRowsManager.AddInvalidatedRow(nRow);

        return OperationResult.SuccessValue(nRow);
    }

    private void OnRowAdded(RowEventArgs e) {
        e.Row.RowChecked += OnRowChecked;
        RowAdded?.Invoke(this, e);
    }

    private void OnRowChecked(object? sender, RowPrepareFormulaEventArgs e) => RowChecked?.Invoke(this, e);

    private void OnRowRemoved(RowEventArgs e) => RowRemoved?.Invoke(this, e);

    private void OnRowRemoving(RowEventArgs e) {
        e.Row.RowChecked -= OnRowChecked;
        RowRemoving?.Invoke(this, e);
    }

    #endregion
}