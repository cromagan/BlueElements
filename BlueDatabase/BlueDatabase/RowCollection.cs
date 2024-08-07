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

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using BlueDatabase.Interfaces;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BlueDatabase;

public sealed class RowCollection : IEnumerable<RowItem>, IDisposableExtended, IHasDatabase {

    #region Fields

    public static readonly List<RowItem> DidRows = new();
    public static readonly List<RowItem> FailedRows = new();
    public static readonly List<RowItem> InvalidatedRows = new();
    public static int WaitDelay = 0;
    private static readonly List<BackgroundWorker> Pendingworker = [];
    private static bool _executingchangedrows;
    private static object _executingchangedrowslock = new();
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

    public event EventHandler<RowChangedEventArgs>? RowAdded;

    public event EventHandler<RowCheckedEventArgs>? RowChecked;

    public event EventHandler<RowEventArgs>? RowGotData;

    public event EventHandler<RowEventArgs>? RowRemoved;

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

    /// <summary>
    /// Durchsucht  Datenbank mit dem angegeben Filter..
    /// </summary>
    /// <returns>Die Zeile, dessen Filter zutrifft - falls nicht gefunden - NULL.</returns>

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

    public static void AddBackgroundWorker(RowItem row) {
        if (row.IsDisposed || row.Database is not Database db || db.IsDisposed) { return; }
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

    public static void DoAllInvalidatedRows(RowItem? masterRow) {
        if (Database.ExecutingScriptAnyDatabase != 0 || DidRows.Count > 0) { return; }

        var ra = 0;
        var n = 0;

        DidRows.Clear();
        try {
            while (InvalidatedRows.Count > 0) {
                n++;
                var r = InvalidatedRows[0];
                InvalidatedRows.RemoveAt(0);

                if (InvalidatedRows.Count > ra) {
                    masterRow?.OnDropMessage(BlueBasics.Enums.FehlerArt.Info, $"{InvalidatedRows.Count - ra} neue Einträge zum Abarbeiten ({InvalidatedRows.Count + DidRows.Count} insgesamt)");
                    ra = InvalidatedRows.Count;
                }

                if (r != null && !r.IsDisposed && r.Database != null && !r.Database.IsDisposed && !DidRows.Contains(r)) {
                    DidRows.Add(r);
                    if (masterRow?.Database != null) {
                        r.UpdateRow(false, true, true, "Update von " + masterRow.CellFirstString());
                        masterRow.OnDropMessage(BlueBasics.Enums.FehlerArt.Info, $"Nr. {n.ToStringInt2()} von {InvalidatedRows.Count + DidRows.Count}: Aktualisiere {r.Database.Caption} / {r.CellFirstString()}");
                    } else {
                        r.UpdateRow(false, true, true, "Normales Update");
                    }
                }
            }
        } catch { }

        DidRows.Clear();
        masterRow?.OnDropMessage(BlueBasics.Enums.FehlerArt.Info, "Updates abgearbeitet");
    }

    public static void ExecuteValueChangedEvent() {
        List<Database> l = [.. Database.AllFiles];
        if (l.Count == 0) { return; }
        try {
            l = l.OrderByDescending(eintrag => eintrag.LastUsedDate).ToList();
        } catch { return; }

        lock (_executingchangedrowslock) {
            if (_executingchangedrows) { return; }
            _executingchangedrows = true;
        }

        var tim = Stopwatch.StartNew();

        while (NextRowToCeck(false) is RowItem row) {
            if (row.IsDisposed || row.Database is not Database db || db.IsDisposed) { break; }

            if (row.Database != l[0]) {
                if (DateTime.UtcNow.Subtract(Develop.LastUserActionUtc).TotalSeconds < 3 + WaitDelay) { break; }
            }

            if (Database.ExecutingScriptAnyDatabase > 0) { break; }

            WaitDelay = Pendingworker.Count * 5;
            if (Pendingworker.Count > 2) { break; }

            if (!db.CanDoValueChangedScript()) { break; }

            var e = new CancelReasonEventArgs();
            db.OnCanDoScript(e);
            if (e.Cancel) { break; }

            Develop.SetUserDidSomething();
            row.UpdateRow(false, false, false, "Allgemeines Updates");
            Develop.SetUserDidSomething();
            if (tim.ElapsedMilliseconds > 30000) { break; }
        }

        lock (_executingchangedrowslock) {
            _executingchangedrows = false;
        }
    }

    /// <summary>
    /// Erstellt eine neue Spalte mit den aus den Filterkriterien. Nur Filter IstGleich wird unterstützt.
    /// Schägt irgendetwas fehl, wird NULL zurückgegeben.
    /// Ist ein Filter mehrfach vorhanden, erhält die Zelle den LETZTEN Wert.
    /// Am Schluß wird noch das Skript ausgeführt.
    /// </summary>
    /// <param name="fc"></param>
    /// <param name="comment"></param>
    /// <returns></returns>
    public static (RowItem? newrow, string message) GenerateAndAdd(FilterCollection fc, string comment) {
        IReadOnlyCollection<string>? first = null;

        Database? db2 = null;

        foreach (var thisfi in fc) {
            if (thisfi.FilterType is not FilterType.Istgleich
                and not FilterType.Istgleich_GroßKleinEgal
                 and not FilterType.Istgleich_ODER_GroßKleinEgal) { return (null, "Filtertyp wird nicht unterstützt"); }
            if (thisfi.Column == null) { return (null, "Leere Spalte angekommen"); }
            if (thisfi.Database is not Database db1 || db1.IsDisposed) { return (null, "Datenbanken unterschiedlich"); }
            db2 ??= db1;

            if (db1.Column.First() == thisfi.Column) {
                if (first != null) { return (null, "Datenbank hat keine erste Spalte, Systeminterner Fehler"); }
                first = thisfi.SearchValue;
            }

            if (thisfi.Column.Database != db2) { return (null, "Spalten-Datenbanken unterschiedlich"); }
        }

        if (db2 == null || db2.IsDisposed) { return (null, "Datenbanken verworfen"); }

        if (!db2.Row.IsNewRowPossible()) { return (null, "In der Datenbank sind keine neuen Zeilen möglich"); }

        if (first == null) { return (null, "Der Wert für die erste Spalte fehlt"); }

        var s = db2.NextRowKey();
        if (string.IsNullOrEmpty(s)) { return (null, "Fehler beim Zeilenschlüssel erstellen, Systeminterner Fehler"); }

        return (db2.Row.GenerateAndAdd(s, first.JoinWithCr(), fc, true, comment), string.Empty);

        //foreach (var thisfi in fc) {
        //    if (thisfi.Column is ColumnItem c) {
        //        row.CellSet(c, thisfi.SearchValue.ToList());
        //    }
        //}

        //_ = row.ExecuteScript(ScriptEventTypes.new_row, string.Empty, false, false, true, 1, null);

        //return row;
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
            if (thisRow != null && !thisRow.IsDisposed) {
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
            if (thisDb is Database db && !db.IsDisposed) {
                if (!db.CanDoValueChangedScript()) { continue; }

                var rowToCheck = db.Row.NextRowToCheck(false);
                if (rowToCheck != null) { return rowToCheck; }

                if (oldestTo) {
                    var tmpo = db.Row.NextRowToCheck(true);
                    oldrow = RowCollection.OlderState(tmpo, oldrow);
                }
            }
        }

        if (oldrow != null) { return oldrow; }

        WaitDelay = Math.Min(WaitDelay + 5, 100);
        return null;
    }

    public static RowItem? OlderState(RowItem? row1, RowItem? row2) {
        if (row1 == null) { return row2; }
        if (row2 == null) { return row1; }

        if (row1.Database?.Column.SysRowState is not ColumnItem srs1 ||
            row2.Database?.Column.SysRowState is not ColumnItem srs2) {
            return Constants.GlobalRnd.Next(2) == 0 ? row1 : row2;
        }

        return row1.CellGetDateTime(srs1) < row2.CellGetDateTime(srs2) ? row1 : row2;
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
                if (row != null && !row.IsDisposed) { ok = false; break; }
            }

            if (ok) { return unique; }
        } while (true);
    }

    public static (RowItem? newrow, string message) UniqueRow(FilterCollection filter, string coment) {
        if (filter.Database is not Database db || db.IsDisposed) { return (null, "Datenbank verworfen"); }

        if (filter.Count < 1) { return (null, "Kein Filter angekommen."); }

        var r = filter.Rows;

        if (r.Count > 5) {
            return (null, "RowUnique gescheitert, da bereits zu viele Zeilen vorhanden sind: " + filter.ReadableText());
        }

        if (r.Count > 1) {
            r[0].Database?.Row.Combine(r);
            r[0].Database?.Row.RemoveYoungest(r, true);
            r = filter.Rows;
            if (r.Count != 1) {
                return (null, "RowUnique gescheitert, Aufräumen fehlgeschlagen: " + filter.ReadableText());
            }
            InvalidatedRows.AddIfNotExists(r[0]);
        }

        RowItem? myRow;

        if (r.Count == 0) {
            var (newrow, message) = RowCollection.GenerateAndAdd(filter, coment);
            if (newrow == null) { return (null, "Neue Zeile konnte nicht erstellt werden: " + message); }
            myRow = newrow;
            RowCollection.InvalidatedRows.AddIfNotExists(newrow);
        } else {
            myRow = r[0];
        }

        return (myRow, string.Empty);
    }

    public (List<RowData> rows, long visiblerowcount) CalculateSortedRows(IEnumerable<RowItem> filteredRows, IEnumerable<RowItem>? pinnedRows, RowSortDefinition? sortused) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return ([], 0); }

        var vrc = 0;

        #region Ermitteln, ob mindestens eine Überschrift vorhanden ist (capName)

        var capName = pinnedRows != null && pinnedRows.Any();
        if (!capName && db.Column.SysChapter is ColumnItem cap) {
            foreach (var thisRow in filteredRows) {
                if (thisRow.Database != null && !thisRow.CellIsNullOrEmpty(cap)) {
                    capName = true;
                    break;
                }
            }
        }

        #endregion

        #region Refresh

        var colsToRefresh = new List<ColumnItem>();
        var reverse = false;
        if (sortused is RowSortDefinition rsd) { colsToRefresh.AddRange(rsd.Columns); reverse = rsd.Reverse; }
        if (db.Column.SysChapter is ColumnItem csc) { _ = colsToRefresh.AddIfNotExists(csc); }
        if (db.Column.First() is ColumnItem cf) { _ = colsToRefresh.AddIfNotExists(cf); }

        db.RefreshColumnsData(colsToRefresh.ToArray());

        #endregion

        var lockMe = new object();

        #region _Angepinnten Zeilen erstellen (_pinnedData)

        List<RowData> pinnedData = [];

        if (pinnedRows != null) {
            _ = Parallel.ForEach(pinnedRows, thisRow => {
                var rd = new RowData(thisRow, "Angepinnt");
                rd.PinStateSortAddition = "1";
                rd.MarkYellow = true;
                rd.AdditionalSort = thisRow.CompareKey(colsToRefresh);

                lock (lockMe) {
                    vrc++;
                    pinnedData.Add(rd);
                }
            });
        }

        #endregion

        #region Gefiltere Zeilen erstellen (_rowData)

        List<RowData> rowData = [];
        _ = Parallel.ForEach(filteredRows, thisRow => {
            var adk = thisRow.CompareKey(colsToRefresh);

            var markYellow = pinnedRows != null && pinnedRows.Contains(thisRow);
            var added = markYellow;

            List<string> caps;
            if (db.Column.SysChapter is ColumnItem sc) {
                caps = thisRow.CellGetList(sc);
            } else {
                caps = [];
            }

            if (caps.Count > 0) {
                if (caps.Contains(string.Empty)) {
                    _ = caps.Remove(string.Empty);
                    caps.Add("-?-");
                }
            }

            if (caps.Count == 0 && capName) { caps.Add("Weitere Zeilen"); }
            if (caps.Count == 0) { caps.Add(string.Empty); }

            foreach (var thisCap in caps) {
                var rd = new RowData(thisRow, thisCap);

                rd.PinStateSortAddition = "2";
                rd.MarkYellow = markYellow;
                rd.AdditionalSort = adk;
                lock (lockMe) {
                    rowData.Add(rd);
                    if (!added) { vrc++; added = true; }
                }
            }
        });

        #endregion

        pinnedData.Sort();
        rowData.Sort();

        if (reverse) { rowData.Reverse(); }

        rowData.InsertRange(0, pinnedData);
        return (rowData, vrc);
    }

    public bool Clear(string comment) => Remove(new FilterCollection(Database, "rowcol clear"), null, comment);

    /// <summary>
    /// Alle Angegebenen Zeilen werden die gleichen Werte erhalten.
    /// </summary>
    /// <param name="rows"></param>
    public void Combine(ICollection<RowItem> rows) {
        if (rows.Count < 2) { return; }

        if (Database is not Database db || db.IsDisposed) { return; }

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
        if (!string.IsNullOrEmpty(m) || Database is not Database db || db.IsDisposed) { return m; }

        if (rows.Count == 0) { return "Keine Zeilen angekommen."; }

        Database.RefreshRowData(rows);

        var txt = "Skript wird ausgeführt: " + scriptname;

        Database.OnProgressbarInfo(new ProgressbarEventArgs(txt, 0, rows.Count, true, false));

        var all = rows.Count;
        var start = DateTime.UtcNow;
        while (rows.Count > 0) {
            Database.OnProgressbarInfo(new ProgressbarEventArgs(txt, all - rows.Count, all, false, false));

            var scx = rows[0].ExecuteScript(eventname, scriptname, true, true, true, 0, null, true, false);

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
    public RowItem? First() => _internal.Values.FirstOrDefault(thisRowItem => thisRowItem != null && !thisRowItem.IsDisposed);

    public RowItem? GenerateAndAdd(string valueOfCellInFirstColumn, FilterCollection? fc, string comment) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return null; }
        if (!Database.Row.IsNewRowPossible()) { return null; }

        var s = Database.NextRowKey();
        if (string.IsNullOrEmpty(s)) { return null; }

        return GenerateAndAdd(s, valueOfCellInFirstColumn, fc, true, comment);
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
    public RowItem GenerateAndAdd(string key, string valueOfCellInFirstColumn, FilterCollection? fc, bool fullprocessing, string comment) {
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
            if (db.Column.SysRowCreator is ColumnItem src) { item.CellSet(src, u, "Initialwert neuer Zeile"); }
            if (db.Column.SysRowCreateDate is ColumnItem scd) { item.CellSet(scd, d.ToString5(), "Initialwert neuer Zeile"); }

            // Dann die Inital-Werte reinschreiben
            if (fc != null) {
                foreach (var thisColum in db.Column) {
                    var val = fc.InitValue(thisColum, string.IsNullOrWhiteSpace(valueOfCellInFirstColumn));
                    if (!string.IsNullOrWhiteSpace(val)) {
                        item.CellSet(thisColum, val, "Initialwert neuer Zeile");
                    }
                }
            }
        }

        if (!string.IsNullOrEmpty(valueOfCellInFirstColumn)) {
            item.CellSet(db.Column.First(), valueOfCellInFirstColumn, "Initialwert neuer Zeile");
        } else {
            Develop.DebugPrint(FehlerArt.Warnung, "Null!");
        }

        if (item.CellFirstString() != db.Column.First()?.AutoCorrect(valueOfCellInFirstColumn, true)) {
            Develop.DebugPrint(FehlerArt.Warnung, "Fehler!!");
        }

        _ = item.ExecuteScript(ScriptEventTypes.InitialValues, string.Empty, true, true, true, 0.1f, null, true, false);
        //if (db.Column.HasKeyColumns()) {
        //    _ = item.ExecuteScript(ScriptEventTypes.keyvalue_changed, string.Empty, true, true, true, 0.1f, null, true);
        //}
        //_ = item.ExecuteScript(ScriptEventTypes.value_changed, string.Empty, true, true, true, 0.1f, null, true);
        //_ = item.ExecuteScript(ScriptEventTypes.prepare_formula, string.Empty, false, false, true, 0.1f, null, true);

        return item;
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

    public bool IsNewRowPossible() {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return false; }

        return Database.IsNewRowPossible();
    }

    /// <summary>
    /// Gibt Zeilen Zurück, die ein Update benötigen.
    /// wenn oldestTo=True ist, wird nach den dringenen Updates die älteste Zeile zurückgegeben, Benutzerunabhängig
    /// </summary>
    /// <param name="oldestTo"></param>
    /// <returns></returns>
    public RowItem? NextRowToCheck(bool oldestTo) {
        if (Database is not Database db || db.IsDisposed) { return null; }

        if (!db.CanDoValueChangedScript()) { return null; }

        var rowToCheck = db.Row.FirstOrDefault(r => r.NeedsRowInitialization());
        if (rowToCheck != null) { return rowToCheck; }

        rowToCheck = db.Row.FirstOrDefault(r => r.NeedsRowUpdateAfterChange());
        if (rowToCheck != null) { return rowToCheck; }

        if (db.AmITemporaryMaster(5, 55)) {
            rowToCheck = db.Row.FirstOrDefault(r => r.NeedsRowUpdate(false));
            if (rowToCheck != null) { return rowToCheck; }
        }

        if (!oldestTo) { return null; }

        if (db.Column.SysRowState is not ColumnItem srs) { return null; }
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
        var keys = (from thisrowitem in _internal.Values where thisrowitem != null && thisrowitem.MatchesTo(fc.ToArray()) select thisrowitem.KeyName).Select(dummy => dummy).ToList();
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

    /// <summary>
    /// Löscht die Jüngste Zeile
    /// </summary>
    /// <param name="rows"></param>
    /// <param name="reduceToOne"></param>
    public void RemoveYoungest(ICollection<RowItem> rows, bool reduceToOne) {
        if (Database is not Database db || db.IsDisposed) { return; }

        var l = new List<RowItem>();
        l = rows.Distinct().ToList();

        if (l.Count < 2) { return; }

        #region Jüngste löschen

        var ToDel = l.First();

        foreach (var thisR2 in l) {
            if (thisR2.CellGetDateTime(db.Column.SysRowCreateDate).Subtract(ToDel.CellGetDateTime(db.Column.SysRowCreateDate)).TotalDays < 0) {
                ToDel = thisR2;
            }
        }

        db.Row.Remove(ToDel, "RowCleanUp");

        if (reduceToOne) {
            l.Remove(ToDel);
            if (l.Count > 1) { RemoveYoungest(l, true); }
        }

        #endregion
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

    public (RowItem? newrow, string message) UniqueRow(string value, string comment) {
        if (string.IsNullOrWhiteSpace(value)) { return (null, "Kein Initialwert angekommen"); }

        if (Database is not Database db || db.IsDisposed) { return (null, "Datenbank verworfen"); }

        if (db.Column.First() is not ColumnItem co) { return (null, "Spalte nicht vorhanden"); }

        using var fic = new FilterCollection(db, "UnqiueRow");

        var fi = new FilterItem(co, FilterType.Istgleich_GroßKleinEgal, value);

        fic.Add(fi);

        return UniqueRow(fic, comment);
    }

    internal static List<RowItem> MatchesTo(FilterItem fi) {
        List<RowItem> l = [];

        if (fi.Database is not Database db || db.IsDisposed) { return l; }

        l.AddRange(db.Row.Where(thisRow => thisRow.MatchesTo(fi)));
        return l;
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
            var l = SearchByKey(thisRow.KeyName) ?? GenerateAndAdd(thisRow.KeyName, string.Empty, null, false, "Clone - Zeile fehlt");
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

            if (reason == Reason.SetCommand) {
                row.ExecuteScript(ScriptEventTypes.row_deleting, string.Empty, false, false, true, 3, null, true, false);
            }

            foreach (var thisColumn in db.Column) {
                if (thisColumn != null) {
                    db.Cell.SetValueInternal(thisColumn, row, string.Empty, Reason.AdditionalWorkAfterCommand);
                }
            }

            if (!_internal.TryRemove(row.KeyName, out _)) { return "Löschen nicht erfolgreich"; }
            OnRowRemoved(new RowChangedEventArgs(row, reason));
            return string.Empty;
        }

        return "Befehl unbekannt";
    }

    internal void RemoveNullOrEmpty() => _internal.RemoveNullOrEmpty();

    private static void PendingWorker_DoWork(object sender, DoWorkEventArgs e) {
        if (e.Argument is not RowItem r || r.IsDisposed) { return; }
        _ = r.ExecuteScript(ScriptEventTypes.value_changed_extra_thread, string.Empty, false, false, false, 10, null, true, false);
    }

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
        OnRowAdded(new RowChangedEventArgs(row, reason));

        if (reason is not Reason.NoUndo_NoInvalidate and not Reason.UpdateChanges) {
            RowCollection.InvalidatedRows.Add(row);
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

    private void OnRowAdded(RowChangedEventArgs e) {
        e.Row.RowChecked += OnRowChecked;
        e.Row.RowGotData += OnRowGotData;

        RowAdded?.Invoke(this, e);
    }

    private void OnRowChecked(object sender, RowCheckedEventArgs e) => RowChecked?.Invoke(this, e);

    private void OnRowGotData(object sender, RowEventArgs e) => RowGotData?.Invoke(this, e);

    private void OnRowRemoved(RowChangedEventArgs e) => RowRemoved?.Invoke(this, e);

    private void OnRowRemoving(RowChangedEventArgs e) {
        e.Row.RowChecked -= OnRowChecked;
        e.Row.RowGotData -= OnRowGotData;
        RowRemoving?.Invoke(this, e);
    }

    #endregion
}