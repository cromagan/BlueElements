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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

    public static readonly List<RowItem> DidRows = [];
    public static readonly List<RowItem> FailedRows = [];
    public static readonly List<RowItem> InvalidatedRows = [];
    public static int WaitDelay;
    private static readonly object Executingchangedrowslock = new();
    private static readonly List<BackgroundWorker> Pendingworker = [];
    private static bool _executingchangedrows;
    private readonly ConcurrentDictionary<string, RowItem> _internal = [];
    private Database? _database;

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

    public event EventHandler<RowEventArgs>? RowAdded;

    public event EventHandler<RowCheckedEventArgs>? RowChecked;

    public event EventHandler<RowEventArgs>? RowGotData;

    public event EventHandler<RowEventArgs>? RowRemoved;

    public event EventHandler<RowEventArgs>? RowRemoving;

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
            if (Database?.Column.First() is { IsDisposed: false } c) {
                //foreach(var thisRow in _internal.Values) {
                //    if(thisRow != null && !thisRow.IsDisposed) {
                //       var  txt = _database?.Cell.GetStringCore(c, thisRow) ?? string.Empty;
                //        if (RowItem.CompareValues(txt, primärSchlüssel, FilterType.Istgleich_GroßKleinEgal)) {  return thisRow}

                //    }

                //}

                //var result = _internal.Values.AsParallel()
                //    .Where(thisRow => thisRow != null && !thisRow.IsDisposed)
                //    .Select(thisRow => new {
                //        Row = thisRow,
                //        Text = _database?.Cell.GetStringCore(c, thisRow) ?? string.Empty
                //    })
                //    .FirstOrDefault(x => RowItem.CompareValues(x.Text, primärSchlüssel, FilterType.Istgleich_GroßKleinEgal)).Row;

                //return result;

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
            var parallelQuery = _internal.Values.AsParallel()
                                                .Where(thisRow => thisRow != null)
                                                .FirstOrDefault(thisRow => thisRow.MatchesTo(filter));

            return parallelQuery;
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Durchsucht  Datenbank mit dem angegeben Filter..
    /// </summary>
    /// <returns>Die Zeile, dessen Filter zutrifft - falls nicht gefunden - NULL.</returns>
    public static void AddBackgroundWorker(RowItem row) {
        if (row.IsDisposed || row.Database is not { IsDisposed: false } db) { return; }
        if (db.EventScript.Get(ScriptEventTypes.value_changed_extra_thread).Count != 1) { return; }
        if (!db.IsRowScriptPossible(true)) { return; }

        var l = new BackgroundWorker();
        l.WorkerReportsProgress = true;
        l.RunWorkerCompleted += PendingWorker_RunWorkerCompleted;
        l.DoWork += PendingWorker_DoWork;

        Pendingworker.Add(l);
        l.RunWorkerAsync(row);

        db.OnDropMessage(FehlerArt.Info, "Hintergrund-Skript wird ausgeführt: " + row.CellFirstString());
    }

    public static void DoAllInvalidatedRows(RowItem? masterRow, bool extendedAllowed) {
        var t = Stopwatch.StartNew();

        do {
            if (DidRows.Count > 0) { return; }
            if (InvalidatedRows.Count == 0) { return; }
            if (t.Elapsed.TotalMinutes > 5) { return; }
            Generic.Pause(1, true);
        } while (Database.ExecutingScriptAnyDatabase.Count > 0);

        var ra = 0;
        var n = 0;

        //DidRows.Clear();

        while (InvalidatedRows.Count > 0) {
            if (InvalidatedRows.Count > ra) {
                masterRow?.OnDropMessage(FehlerArt.Info, $"{InvalidatedRows.Count - ra} neue Einträge zum Abarbeiten ({InvalidatedRows.Count + DidRows.Count} insgesamt)");
                ra = InvalidatedRows.Count;
            }

            n++;
            RowItem? r = null;

            try {
                r = InvalidatedRows[0];
                InvalidatedRows.RemoveAt(0);
            } catch { }

            if (r is { IsDisposed: false, Database.IsDisposed: false } && !DidRows.Contains(r)) {
                DidRows.Add(r);
                if (r.NeedsRowUpdate(false, true)) {
                    if (masterRow?.Database != null) {
                        r.UpdateRow(extendedAllowed, true, "Update von " + masterRow.CellFirstString());
                        masterRow.OnDropMessage(FehlerArt.Info, $"Nr. {n} von {InvalidatedRows.Count + DidRows.Count}: Aktualisiere {r.Database.Caption} / {r.CellFirstString()}");
                    } else {
                        r.UpdateRow(extendedAllowed, true, "Normales Update");
                    }
                }
            }
        }
        DidRows.Clear();
        masterRow?.OnDropMessage(FehlerArt.Info, "Updates abgearbeitet");
    }

    public static void ExecuteValueChangedEvent() {
        List<Database> l = [.. Database.AllFiles];
        if (l.Count == 0) { return; }
        try {
            l = l.OrderByDescending(eintrag => eintrag.LastUsedDate).ToList();
        } catch { return; }

        lock (Executingchangedrowslock) {
            if (_executingchangedrows) { return; }
            _executingchangedrows = true;
        }

        var tim = Stopwatch.StartNew();

        while (NextRowToCeck(false) is { IsDisposed: false } row) {
            if (row.IsDisposed || row.Database is not { IsDisposed: false } db) { break; }

            if (row.Database != l[0]) {
                if (DateTime.UtcNow.Subtract(Develop.LastUserActionUtc).TotalSeconds < 3 + WaitDelay) { break; }
            }

            if (Database.ExecutingScriptAnyDatabase.Count > 0) { break; }

            WaitDelay = Pendingworker.Count * 5;
            if (Pendingworker.Count > 2) { break; }

            if (!db.CanDoValueChangedScript()) { break; }

            var e = new CancelReasonEventArgs();
            db.OnCanDoScript(e);
            if (e.Cancel) { break; }

            Develop.SetUserDidSomething();
            row.UpdateRow(true, false, "Allgemeines Updates");
            Develop.SetUserDidSomething();
            if (tim.ElapsedMilliseconds > 30000) { break; }
        }

        lock (Executingchangedrowslock) {
            _executingchangedrows = false;
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
        unique = unique.Except(notUnique).ToList();
    }

    /// <summary>
    /// Prüft alle Datenbanken im Speicher und gibt die dringenste Update-Aufgabe aller Datenbanken zurück.
    /// </summary>
    /// <returns></returns>
    public static RowItem? NextRowToCeck(bool oldestTo) {
        List<Database> l = [.. Database.AllFiles];

        if (Constants.GlobalRnd.Next(10) == 1) {
            l.Shuffle();
        } else {
            l = l.OrderByDescending(eintrag => eintrag.LastUsedDate).ToList();
        }

        RowItem? oldrow = null;

        foreach (var thisDb in l) {
            if (thisDb is { IsDisposed: false } db) {
                if (!db.CanDoValueChangedScript()) { continue; }

                var rowToCheck = db.Row.NextRowToCheck(false);
                if (rowToCheck != null) { return rowToCheck; }

                if (oldestTo) {
                    var tmpo = db.Row.NextRowToCheck(true);
                    oldrow = OlderState(tmpo, oldrow);
                }
            }
        }

        if (oldrow != null) { return oldrow; }

        WaitDelay = Math.Min(WaitDelay + 5, 100);
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

        Database? db = null;

        allrows = allrows.Distinct().ToList();

        foreach (var thisr in allrows) {
            db ??= thisr.Database;

            if (db != thisr.Database) {
                Develop.DebugPrint(FehlerArt.Fehler, "Datenbanken inkonsitent");
                return false;
            }
        }

        foreach (var thisRow in allrows) {
            if (Remove(thisRow, comment)) { did = true; }
        }

        return did;
    }

    public static bool Remove(RowItem? row, string comment) {
        if (row is not { IsDisposed: false } r) { return false; }
        return string.IsNullOrEmpty(r.Database?.ChangeData(DatabaseDataType.Command_RemoveRow, null, r, string.Empty, r.KeyName, Generic.UserName, DateTime.UtcNow, comment, DatabaseChunk.GetChunkValue(r)));
    }

    /// <summary>
    /// Prüft alle Datenbanken im Speicher und gibt die dringenste Update-Aufgabe aller Datenbanken zurück.
    /// </summary>
    /// <returns></returns>
    public static List<RowItem> RowListToCheck() {
        var r = new List<RowItem>();
        List<Database> l = [.. Database.AllFiles];

        foreach (var thisDb in l) {
            if (thisDb is { IsDisposed: false } db) {
                if (!db.CanDoValueChangedScript()) { continue; }
                r.AddRange(db.Row);
            }
        }

        return r.OrderBy(eintrag => eintrag.UrgencyUpdate).ToList();
    }

    public static string UniqueKeyValue() {
        var x = 9999;
        do {
            x += 1;
            if (x > 99999) { Develop.DebugPrint(FehlerArt.Fehler, "Unique ID konnte nicht erzeugt werden"); }

            var unique = ("X" + DateTime.UtcNow.ToString("mm.fff") + x.ToStringInt5()).RemoveChars(Constants.Char_DateiSonderZeichen + ".");
            var ok = true;

            foreach (var thisfile in Database.AllFiles) {
                var row = thisfile.Row[unique];
                if (row is { IsDisposed: false }) { ok = false; break; }
            }

            if (ok) { return unique; }
        } while (true);
    }

    public static (RowItem? newrow, string message, bool stoptrying) UniqueRow(FilterCollection filter, string coment) {
        if (filter.Database is not { IsDisposed: false } db) { return (null, "Datenbank verworfen", true); }

        if (filter.Count < 1) { return (null, "Kein Filter angekommen.", true); }

        var r = filter.Rows;

        if (r.Count > 5) {
            return (null, "RowUnique gescheitert, da bereits zu viele Zeilen vorhanden sind: " + filter.ReadableText(), true);
        }

        if (r.Count > 1) {
            r[0].Database?.Row.Combine(r);
            r[0].Database?.Row.RemoveYoungest(r, true);
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
            var (newrow, message, stoptrying) = db.Row.GenerateAndAdd(filter.ToList(), coment);
            if (newrow == null) { return (null, "Neue Zeile konnte nicht erstellt werden: " + message, stoptrying); }
            myRow = newrow;
        } else {
            myRow = r[0];
        }

        return (myRow, string.Empty, false);
    }

    public bool Clear(string comment) {
        using var fc = new FilterCollection(Database, "rowcol clear");
        return Remove(fc, null, comment);
    }

    /// <summary>
    /// Alle Angegebenen Zeilen werden die gleichen Werte erhalten.
    /// </summary>
    /// <param name="rows"></param>
    public void Combine(ICollection<RowItem> rows) {
        if (rows.Count < 2) { return; }

        if (Database is not { IsDisposed: false } db) { return; }

        #region Leere Werte befüllen

        foreach (var thisC in db.Column) {

            #region neuen Wert zum Reinschreiben ermitteln (Wert)

            var wert = string.Empty;
            foreach (var thisR2 in rows) {
                if (thisR2.Database != db) { return; }

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
    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public string ExecuteScript(ScriptEventTypes? eventname, string scriptname, List<RowItem> rows) {
        var m = Database.EditableErrorReason(Database, EditableErrorReasonType.EditCurrently);
        if (!string.IsNullOrEmpty(m) || Database is not { IsDisposed: false } db) { return m; }

        if (rows.Count == 0) { return "Keine Zeilen angekommen."; }

        var txt = "Skript wird ausgeführt: " + scriptname;

        Database.OnProgressbarInfo(new ProgressbarEventArgs(txt, 0, rows.Count, true, false));

        var all = rows.Count;
        var start = DateTime.UtcNow;
        while (rows.Count > 0) {
            Database.OnProgressbarInfo(new ProgressbarEventArgs(txt, all - rows.Count, all, false, false));

            var scx = rows[0].ExecuteScript(eventname, scriptname, true, 0, null, true, false);

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

    // TODO: Override a finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources.
    public RowItem? First() => _internal.Values.FirstOrDefault(thisRowItem => thisRowItem is { IsDisposed: false });

    /// <summary>
    /// Erstellt eine neue Zeile mit den aus den Filterkriterien. Nur Filter IstGleich wird unterstützt.
    /// Schägt irgendetwas fehl, wird NULL zurückgegeben.
    /// Ist ein Filter mehrfach vorhanden, erhält die Zelle den LETZTEN Wert.
    /// Am Schluss wird noch das Skript ausgeführt.
    /// </summary>
    /// <param name="fc"></param>
    /// <param name="comment"></param>
    /// <returns></returns>
    public (RowItem? newrow, string message, bool stoptrying) GenerateAndAdd(List<FilterItem> filter, string comment) {
        Database? db2 = null;

        foreach (var thisfi in filter) {
            if (thisfi.FilterType is not FilterType.Istgleich
                and not FilterType.Istgleich_GroßKleinEgal
                and not FilterType.Istgleich_ODER_GroßKleinEgal) { return (null, "Filtertyp wird nicht unterstützt", true); }

            if (thisfi.Column == null) { return (null, "Leere Spalte angekommen", true); }
            if (thisfi.Database is not { IsDisposed: false } db1) { return (null, "Datenbanken unterschiedlich", true); }
            db2 ??= db1;

            if (thisfi.Column.Database != db2) { return (null, "Spalten-Datenbanken unterschiedlich", true); }
        }

        if (db2 is not { IsDisposed: false }) { return (null, "Datenbanken verworfen", true); }

        if (db2.Column.First() is not { }) { return (null, "Datenbank hat keine erste Spalte, Systeminterner Fehler", false); }

        var f = db2.EditableErrorReason(EditableErrorReasonType.EditNormaly);
        if (!string.IsNullOrEmpty(f)) { return (null, "In der Datenbank sind keine neuen Zeilen möglich: " + f, true); }

        var s = db2.NextRowKey();
        if (string.IsNullOrEmpty(s)) { return (null, "Fehler beim Zeilenschlüssel erstellen, Systeminterner Fehler", false); }

        foreach (var thisColum in db2.Column) {
            if (thisColum.Function == ColumnFunction.First || thisColum == db2.Column.SplitColumn) {
                var inval = FilterCollection.InitValue(filter, thisColum, true);
                if (inval == null || string.IsNullOrWhiteSpace(inval)) {
                    return (null, "Initalwert fehlt.", false);
                }
            }
        }

        return (db2.Row.GenerateAndAddInternal(s, filter, comment), string.Empty, false);
    }

    public RowItem? GenerateAndAdd(string valueOfCellInFirstColumn, string comment) {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return null; }

        if (db.Column.First() is not { } cf) { return null; }

        return GenerateAndAdd([new FilterItem(cf, FilterType.Istgleich, valueOfCellInFirstColumn)], comment).newrow;
    }

    //    List<Database> done = new();
    public IEnumerator<RowItem> GetEnumerator() => _internal.Values.GetEnumerator();

    //    if (database is not Database db) { return "Verlinkung zur Datenbank fehlhlerhaft"; }
    //    if (db.IsDisposed) { return "Datenbank verworfen"; }
    //foreach (var ThisRowItem in _Internal.Values)//{//    if (ThisRowItem != null) { return ThisRowItem; }//}//return null;
    IEnumerator IEnumerable.GetEnumerator() => _internal.Values.GetEnumerator();

    public bool HasPendingWorker() => Pendingworker.Count > 0;

    //public string DoLinkedDatabase(List<RowItem> rowsToExpand) {
    //    if (rowsToExpand.Count == 0) { return string.Empty; }
    public void InvalidateAllCheckData() {
        foreach (var thisRow in this) {
            thisRow.InvalidateCheckData();
        }
    }

    /// <summary>
    /// Gibt Zeilen Zurück, die ein Update benötigen.
    /// wenn oldestTo=True ist, wird nach den dringenen Updates die älteste Zeile zurückgegeben, Benutzerunabhängig
    /// </summary>
    /// <param name="oldestTo"></param>
    /// <returns></returns>
    public RowItem? NextRowToCheck(bool oldestTo) {
        if (Database is not { IsDisposed: false } db) { return null; }

        if (!db.CanDoValueChangedScript()) { return null; }

        var rowToCheck = db.Row.FirstOrDefault(r => r.NeedsRowInitialization());
        if (rowToCheck != null) { return rowToCheck; }

        rowToCheck = db.Row.FirstOrDefault(r => r.NeedsRowUpdateAfterChange());
        if (rowToCheck != null) { return rowToCheck; }

        if (db.AmITemporaryMaster(5, 55)) {
            rowToCheck = db.Row.FirstOrDefault(r => r.NeedsRowUpdate(false, oldestTo));
            if (rowToCheck != null) { return rowToCheck; }
        }

        if (!oldestTo) { return null; }

        if (db.Column.SysRowState is not { IsDisposed: false } srs) { return null; }
        var datefoundmax = new DateTime(2100, 1, 1);
        RowItem? foundrow = null;

        foreach (var thisRow in db.Row) {
            var dateofmyrow = thisRow.CellGetDateTime(srs);
            if (dateofmyrow < datefoundmax && !FailedRows.Contains(thisRow)) {
                datefoundmax = dateofmyrow;
                foundrow = thisRow;
            }
        }

        return foundrow;
    }

    public bool Remove(FilterItem fi, string comment) => Remove(FilterCollection.CalculateFilteredRows(Database, fi), comment);

    public bool RemoveOlderThan(float inHours, string comment) {
        if (Database?.Column.SysRowCreateDate is not { IsDisposed: false } src) { return false; }

        var x = (from thisrowitem in _internal.Values where thisrowitem != null let d = thisrowitem.CellGetDateTime(src) where DateTime.UtcNow.Subtract(d).TotalHours > inHours select thisrowitem).ToList();
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

    /// <summary>
    /// Löscht die Jüngste Zeile
    /// </summary>
    /// <param name="rows"></param>
    /// <param name="reduceToOne"></param>
    public void RemoveYoungest(ICollection<RowItem> rows, bool reduceToOne) {
        if (Database is not { IsDisposed: false } db) { return; }

        var l = rows.Distinct().ToList();

        if (l.Count < 2) { return; }

        #region Jüngste löschen

        var toDel = l.First();

        foreach (var thisR2 in l) {
            if (thisR2.CellGetDateTime(db.Column.SysRowCreateDate).Subtract(toDel.CellGetDateTime(db.Column.SysRowCreateDate)).TotalDays < 0) {
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

    public RowItem? SearchByKey(string? key) {
        if (Database is not { IsDisposed: false } || key == null || string.IsNullOrWhiteSpace(key)) { return null; }
        try {
            var r = _internal.TryGetValue(key, out var value) ? value : null;
            if (r is { IsDisposed: true }) {
                Develop.DebugPrint(FehlerArt.Fehler, "Interner Zeilenfehler: " + key);
                return null;
            }

            return r;
        } catch {
            Develop.CheckStackForOverflow();
            return SearchByKey(key);
        }
    }

    public (RowItem? newrow, string message, bool stoptrying) UniqueRow(string value, string comment) {
        // Used: Only BZL
        if (string.IsNullOrWhiteSpace(value)) { return (null, "Kein Initialwert angekommen", true); }

        if (Database is not { IsDisposed: false } db) { return (null, "Datenbank verworfen", true); }

        if (db.Column.First() is not { IsDisposed: false } co) { return (null, "Spalte nicht vorhanden", true); }

        using var fic = new FilterCollection(db, "UnqiueRow");

        var fi = new FilterItem(co, FilterType.Istgleich_GroßKleinEgal, value);

        fic.Add(fi);

        return UniqueRow(fic, comment);
    }

    internal static List<RowItem> MatchesTo(FilterItem fi) {
        List<RowItem> l = [];

        if (fi.Database is not { IsDisposed: false } db) { return l; }

        l.AddRange(db.Row.Where(thisRow => thisRow.MatchesTo(fi)));
        return l;
    }

    internal string ExecuteCommand(DatabaseDataType type, string rowkey, Reason reason, string? user, DateTime? datetimeutc) {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return "Datenbank verworfen"; }

        if (type == DatabaseDataType.Command_AddRow) {
            var row = SearchByKey(rowkey);
            if (row is { IsDisposed: false }) { return string.Empty; } // "Zeile " + rowkey+ " bereits vorhanden!";

            row = new RowItem(db, rowkey);
            var f = Add(row, reason);

            if (string.IsNullOrEmpty(f) && user != null && datetimeutc is { } dt) {
                if (db.Column.SysRowCreator is { IsDisposed: false } src) { row.SetValueInternal(src, user, reason); }
                if (db.Column.SysRowCreateDate is { IsDisposed: false } scd) { row.SetValueInternal(scd, dt.ToString5(), reason); }
                if (db.Column.SysLocked is { IsDisposed: false } sl) { row.SetValueInternal(sl, false.ToPlusMinus(), reason); }
                if (db.Column.SysCorrect is { IsDisposed: false } sc) { row.SetValueInternal(sc, true.ToPlusMinus(), reason); }
            }

            if (reason == Reason.SetCommand && db.LogUndo) {
                Generic.Pause(0.001, false); // um in den Logs den Zeitstempel richtig zu haben
            }
            return f;
        }

        if (type == DatabaseDataType.Command_RemoveRow) {
            var row = SearchByKey(rowkey);
            if (row == null) { return "Zeile nicht gefunden!"; }

            if (reason != Reason.NoUndo_NoInvalidate) {
                OnRowRemoving(new RowEventArgs(row));
            }

            if (reason == Reason.SetCommand) {
                row.ExecuteScript(ScriptEventTypes.row_deleting, string.Empty, true, 3, null, true, false);
            }

            foreach (var thisColumn in db.Column) {
                if (thisColumn != null) {
                    row.SetValueInternal(thisColumn, string.Empty, Reason.NoUndo_NoInvalidate);
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

    //    if (sourceDatabase.Row.Count != Count) {
    //        Develop.DebugPrint(FehlerArt.Fehler, "Clone Fehlgeschlagen");
    //    }
    //}
    internal void RemoveNullOrEmpty() => _internal.RemoveNullOrEmpty();

    //    // Zeilen erzeugen und Format übertragen
    //    foreach (var thisRow in sourceDatabase.Row) {
    //        var l = SearchByKey(thisRow.KeyName) ?? GenerateAndAdd(thisRow.KeyName, string.Empty, null, false, "Clone - Zeile fehlt");
    //        l.CloneFrom(thisRow, true);
    //    }
    internal void Repair() {
        foreach (var thisR in _internal) {
            thisR.Value.Repair();
        }
    }

    //    // Zeilen, die zu viel sind, löschen
    //    foreach (var thisRow in this) {
    //        var l = sourceDatabase.Row.SearchByKey(thisRow.KeyName);
    //        if (l == null) { _ = Remove(thisRow, "Clone - Zeile zuviel"); }
    //    }
    private static RowItem? OlderState(RowItem? row1, RowItem? row2) {
        if (row1 == null) { return row2; }
        if (row2 == null) { return row1; }

        if (row1.Database?.Column.SysRowState is not { IsDisposed: false } srs1 ||
            row2.Database?.Column.SysRowState is not { IsDisposed: false } srs2) {
            return Constants.GlobalRnd.Next(2) == 0 ? row1 : row2;
        }

        return row1.CellGetDateTime(srs1) < row2.CellGetDateTime(srs2) ? row1 : row2;
    }

    //    var f = db.EditableErrorReason(EditableErrorReasonType.EditNormaly);
    //    if (!string.IsNullOrEmpty(f)) {
    //        Develop.DebugPrint(FehlerArt.Fehler, "Neue Zeilen nicht möglich: " + f);
    //        throw new Exception();
    //    }
    private static void PendingWorker_DoWork(object sender, DoWorkEventArgs e) {
        if (e.Argument is not RowItem { IsDisposed: false } r) { return; }
        _ = r.ExecuteScript(ScriptEventTypes.value_changed_extra_thread, string.Empty, true, 10, null, true, false);
    }

    //internal void CloneFrom(Database sourceDatabase) {
    //    if (IsDisposed || Database is not { IsDisposed: false } db) { return; }
    private static void PendingWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) => Pendingworker.Remove((BackgroundWorker)sender);

    private void _database_Disposing(object sender, System.EventArgs e) => Dispose();

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
            if (Database?.Column.SysRowState != null) {
                InvalidatedRows.AddIfNotExists(row);
            }
        }

        return string.Empty;
    }

    private void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                Database = null;
                foreach (var thisR in _internal) { thisR.Value.Dispose(); }
            }

            _internal.Clear();

            IsDisposed = true;
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="key"></param>
    /// <param name="fc"></param>
    /// <param name="comment"></param>
    /// <returns></returns>
    private RowItem GenerateAndAddInternal(string key, List<FilterItem> fc, string comment) {
        if (Database is not { IsDisposed: false } db) {
            Develop.DebugPrint(FehlerArt.Fehler, "Datenbank verworfen!");
            throw new Exception();
        }

        var f = db.EditableErrorReason(EditableErrorReasonType.EditNormaly);

        if (!string.IsNullOrEmpty(f)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Neue Zeilen nicht möglich: " + f);
            throw new Exception();
        }

        var item = SearchByKey(key);
        if (item != null) {
            Develop.DebugPrint(FehlerArt.Fehler, "Schlüssel belegt!");
            throw new Exception();
        }

        var u = Generic.UserName;
        var d = DateTime.UtcNow;

        var s = db.ChangeData(DatabaseDataType.Command_AddRow, null, null, string.Empty, key, u, d, comment, string.Empty);
        if (!string.IsNullOrEmpty(s)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Erstellung fehlgeschlagen: " + s);
            throw new Exception();
        }

        item = SearchByKey(key);
        if (item == null) {
            Develop.DebugPrint(FehlerArt.Fehler, "Erstellung fehlgeschlagen, ID Fehler");
            throw new Exception();
        }

        // Dann die Inital-Werte reinschreiben

        List<ColumnItem> l = [.. db.Column];
        if (db.Column.SplitColumn is { } spc) {
            l.Remove(spc);
            l.Insert(0, spc);
        }

        foreach (var thisColum in l) {
            var val = FilterCollection.InitValue(fc, thisColum, true);
            if (!string.IsNullOrWhiteSpace(val)) {
                item.CellSet(thisColum, val, "Initialwert neuer Zeile");
            }
        }

        _ = item.ExecuteScript(ScriptEventTypes.InitialValues, string.Empty, true, 0.1f, null, true, false);

        return item;
    }

    private void OnRowAdded(RowEventArgs e) {
        e.Row.RowChecked += OnRowChecked;
        e.Row.RowGotData += OnRowGotData;

        RowAdded?.Invoke(this, e);
    }

    private void OnRowChecked(object sender, RowCheckedEventArgs e) => RowChecked?.Invoke(this, e);

    private void OnRowGotData(object sender, RowEventArgs e) => RowGotData?.Invoke(this, e);

    private void OnRowRemoved(RowEventArgs e) => RowRemoved?.Invoke(this, e);

    private void OnRowRemoving(RowEventArgs e) {
        e.Row.RowChecked -= OnRowChecked;
        e.Row.RowGotData -= OnRowGotData;
        RowRemoving?.Invoke(this, e);
    }

    #endregion
}