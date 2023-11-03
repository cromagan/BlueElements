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
using System.ComponentModel;
using System.Threading;
using BlueBasics;
using BlueBasics.Enums;
using BlueDatabase.Enums;
using System.Linq;
using static BlueDatabase.SqlBackAbstract;

namespace BlueDatabase;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class DatabaseSqlLite : DatabaseAbstract {

    #region Fields

    /// <summary>
    /// Nicht static, weil verschiedene Datenbankverbindungen möglich sind.
    /// </summary>
    public readonly SqlBackAbstract? SQL;

    private static bool _isInTimer;

    /// <summary>
    /// Der Globale Timer, der die Sys_Undo Datenbank abfrägt
    /// </summary>
    private static Timer? _timer;

    /// <summary>
    /// Der Zeitstempel der letzten Abfrage des _timer
    /// </summary>
    private static DateTime _timerTimeStamp = DateTime.UtcNow.AddSeconds(-0.5);

    private readonly string _tablename = string.Empty;
    private bool _undoLoaded;

    #endregion

    #region Constructors

    public DatabaseSqlLite(ConnectionInfo ci, bool readOnly) : this(((DatabaseSqlLite?)ci.Provider)?.SQL, readOnly, ci.MustBeFreezed, ci.TableName) { }

    public DatabaseSqlLite(SqlBackAbstract? sql, bool readOnly, string freezedReason, string tablename) : base(readOnly, freezedReason) {
        if (sql == null) {
            Develop.DebugPrint(FehlerArt.Fehler, "Keine SQL-Verbindung übergeben: " + tablename);
            return;
        }

        SQL = sql;
        sql.RepairAll(tablename);

        _tablename = MakeValidTableName(tablename);

        if (!IsValidTableName(_tablename, false)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Tabellenname ungültig: " + tablename);
        }

        Initialize();
        LoadFromSqlBack(Preselection);

        TryToSetMeTemporaryMaster();
    }

    #endregion

    #region Properties

    public static string DatabaseId => nameof(DatabaseSqlLite);

    public override ConnectionInfo ConnectionData {
        get {
            if (SQL == null) {
                return new(TableName, null, string.Empty, string.Empty, FreezedReason);
            }
            var connectionData = SQL.ConnectionData(TableName, FreezedReason);
            connectionData.Provider = this;
            return connectionData;
        }
    }

    public override string TableName => _tablename;

    public override bool UndoLoaded => _undoLoaded;

    #endregion

    #region Methods

    public static DatabaseAbstract? CanProvide(ConnectionInfo ci, bool readOnly, NeedPassword? needPassword) {
        if (!DatabaseId.Equals(ci.DatabaseId, StringComparison.OrdinalIgnoreCase)) { return null; }

        var sql = ((DatabaseSqlLite?)ci.Provider)?.SQL;
        if (sql == null) { return null; }

        var at = sql.Tables();
        if (at == null || !at.Contains(ci.TableName)) { return null; }
        return new DatabaseSqlLite(ci, readOnly);
    }

    public static void CheckSysUndoNow() {
        if (_isInTimer) { return; }
        _isInTimer = true;

        var fd = DateTime.UtcNow;

        try {
            var done = new List<DatabaseAbstract>();

            foreach (var thisDb in AllFiles) {
                if (!done.Contains(thisDb)) {
                    if (thisDb is DatabaseSqlLite thisDbSqlLite) {
                        if (thisDbSqlLite.SQL != null) {
                            var db = LoadedDatabasesWithThisSql(thisDbSqlLite.SQL);
                            done.AddRange(db);

                            var erg = thisDbSqlLite.SQL.GetLastChanges(db, _timerTimeStamp.AddSeconds(-0.01), fd);
                            if (erg == null) { _isInTimer = false; return; } // Später ein neuer Versuch

                            foreach (var thisdb in db) {
                                thisdb.DoLastChanges(erg);
                                thisdb.TryToSetMeTemporaryMaster();
                            }
                        }
                    }
                }
            }
        } catch {
            _isInTimer = false;
            return;
        }

        _timerTimeStamp = fd;
        _isInTimer = false;
    }

    public override List<ConnectionInfo>? AllAvailableTables(List<DatabaseAbstract>? allreadychecked, string mustBeFreezed) {
        if (allreadychecked != null) {
            foreach (var thisa in allreadychecked) {
                if (thisa is DatabaseSqlLite db) {
                    if (db.SQL?.ConnectionString == SQL?.ConnectionString) { return null; }
                }
            }
        }

        var tb = SQL?.Tables();
        var l = new List<ConnectionInfo>();

        if (tb == null) {
            Develop.DebugPrint(FehlerArt.Fehler, "Verbindung zur Datenbank gescheitert.");
            return l;
        }

        foreach (var thistb in tb) {
            var t = ConnectionDataOfOtherTable(thistb, false, mustBeFreezed);
            if (t != null) { l.Add(t); }
        }

        return l;
    }

    //        return false;
    //    }
    //}
    public override ConnectionInfo? ConnectionDataOfOtherTable(string tableName, bool checkExists, string mustBeeFreezed) {
        if (checkExists) {
            var t = AllAvailableTables(null, mustBeeFreezed);

            if (t != null) {
                foreach (var thisT in t) {
                    if (string.Equals(tableName, thisT.TableName, StringComparison.InvariantCultureIgnoreCase)) {
                        return thisT;
                    }
                }
            }

            return null;
        }

        var connectionData = SQL?.ConnectionData(tableName, mustBeeFreezed);
        if (connectionData != null) {
            connectionData.Provider = this;
            return connectionData;
        }
        return null;
    }

    public override string EditableErrorReason(EditableErrorReasonType mode) {
        var m = base.EditableErrorReason(mode);
        if (!string.IsNullOrEmpty(m)) { return m; }

        if (SQL == null) { return "Keine SQL-Verbindung vorhanden"; }

        if (mode is EditableErrorReasonType.OnlyRead or EditableErrorReasonType.Load) { return string.Empty; }

        if (ReadOnly) { return "Datenbank schreibgeschützt!"; } // Immer abfragen, da Änderungen direkt gespeichert werden

        return string.Empty;
    }

    public override void GetUndoCache() {
        if (UndoLoaded) { return; }
        if (SQL == null) { return; }

        var undos = SQL.GetLastChanges(new List<DatabaseSqlLite> { this }, new DateTime(2000, 1, 1), new DateTime(2100, 1, 1));

        Undo.Clear();
        Undo.AddRange(undos);

        _undoLoaded = true;
    }

    public override string? NextRowKey() {
        if (!Row.IsNewRowPossible()) {
            Develop.DebugPrint(FehlerArt.Fehler, "Systemspalte Correct fehlt!");
        }
        return SQL?.GenerateRow(TableName);
    }

    public override void RefreshColumnsData(List<ColumnItem> columns) {
        if (columns.Count == 0) { return; }

        if (SQL == null) {
            Develop.DebugPrint(FehlerArt.Fehler, "SQL Verbindung verworfen");
            return;
        }

        var need = false;
        foreach (var thisColumn in columns) {
            if (thisColumn != null && thisColumn.IsInCache == null) {
                need = true;

                if (thisColumn.LinkedDatabase is DatabaseAbstract dbl &&
                     dbl.Column.Exists(thisColumn.LinkedCell_ColumnNameOfLinkedDatabase) is ColumnItem col) {
                    dbl.RefreshColumnsData(col);
                }
            }
        }

        if (!need) { return; }

        //foreach (var thisColumn in Column) {
        //    if (thisColumn.IsInCache == null && !columns.Contains(thisColumn)) {
        //        columns.Add(thisColumn);
        //        if (columns.Count > 2) { break; }
        //    }
        //}

        OnDropMessage(FehlerArt.Info, "Lade " + columns.Count + " Spalte(n) der Datenbank '" + TableName + "' nach.");

        try {
            SQL.LoadColumns(TableName, Row, columns, false, Preselection);
        } catch {
            Develop.CheckStackForOverflow();
            RefreshColumnsData(columns);
        }

        OnDropMessage(FehlerArt.Info, string.Empty);
    }

    public override (bool didreload, string errormessage) RefreshRowData(List<RowItem> rows, bool refreshAlways) {
        if (rows == null || rows.Count == 0) { return (false, string.Empty); }

        var l = new List<RowItem>();

        foreach (var thisr in rows) {
            if (refreshAlways || thisr.IsInCache == null) {
                l.Add(thisr);
            }
        }

        if (l.Count == 0) { return (false, string.Empty); }
        l = l.Distinct().ToList();

        if (l.Count < 50) {
            foreach (var thisr in Row) {
                if (thisr.IsInCache == null && !l.Contains(thisr)) {
                    l.Add(thisr);
                    if (l.Count >= 100) { break; }
                }
            }
        }

        //Row.DoLinkedDatabase(rows);
        //Develop.CheckStackForOverflow();
        OnDropMessage(FehlerArt.Info, "Lade " + l.Count + " Zeile(n) der Datenbank '" + TableName + "' nach.");

        if (SQL == null) { return (false, "SQL Verbindung fehlerhaft"); }

        try {
            return (true, SQL.LoadRow(TableName, l, refreshAlways));
        } catch {
            Develop.CheckStackForOverflow();
            return RefreshRowData(rows, refreshAlways);
        }
    }

    //    LoadFromSQLBack();
    //}
    public override bool Save() => SQL != null;

    public List<string> SQLLog() => Log;

    internal override bool IsNewRowPossible() {
        if (Column.SysCorrect == null) { return false; }
        return base.IsNewRowPossible();
    }

    internal override string SetValueInternal(DatabaseDataType type, string value, ColumnItem? column, RowItem? row, Reason reason, string user, DateTime datetimeutc) {
        if (IsDisposed) { return "Datenbank verworfen!"; }

        if (type.IsObsolete()) { return string.Empty; }

        if (reason != Reason.LoadReload) {
            if (ReadOnly) { return "Datenbank schreibgeschützt!"; } // Sicherheitshalber!
            _ = SQL?.SetValueInternal(this, type, value, column, row, user, datetimeutc);
        }

        return base.SetValueInternal(type, value, column, row, reason, user, datetimeutc);
    }

    protected override void AddUndo(DatabaseDataType type, ColumnItem? column, RowItem? row, string previousValue, string changedTo, string userName, DateTime datetimeutc, string comment) {
        base.AddUndo(type, column, row, previousValue, changedTo, userName, datetimeutc, comment);

        if (IsDisposed) { return; }
        if (type.IsObsolete()) { return; }
        if (ReadOnly) { return; } // Sicherheitshalber!

        if (type == DatabaseDataType.SystemValue) { return; }

        var columnName = column?.KeyName ?? string.Empty;
        var rowkey = row?.KeyName ?? string.Empty;

        var err = SQL?.AddUndo(TableName, type, columnName, rowkey, previousValue, changedTo, userName, datetimeutc, comment);
        if (!string.IsNullOrEmpty(err)) {
            Develop.CheckStackForOverflow();
            AddUndo(type, column, row, previousValue, changedTo, userName, datetimeutc, comment);
        }
    }

    private static void CheckSysUndo(object state) {
        if (DateTime.UtcNow.Subtract(_timerTimeStamp).TotalSeconds < 180) { return; }
        if (DateTime.UtcNow.Subtract(LastLoadUtc).TotalSeconds < 5) { return; }

        if (CriticalState()) { return; }
        CheckSysUndoNow();
    }

    private static List<DatabaseSqlLite> LoadedDatabasesWithThisSql(SqlBackAbstract sql) {
        var oo = new List<DatabaseSqlLite>();
        foreach (var thisDb in AllFiles) {
            if (thisDb is DatabaseSqlLite thidDbsqllIte) {
                if (sql.ConnectionString == thidDbsqllIte.SQL?.ConnectionString) {
                    oo.Add(thidDbsqllIte);
                }
            }
        }

        return oo;
    }

    private void DoLastChanges(List<UndoItem>? data) {
        if (data == null) { return; }
        if (IsDisposed) { return; }
        if (!string.IsNullOrEmpty(FreezedReason)) { return; }

        if (IsInCache == null) {
            Develop.DebugPrint(FehlerArt.Fehler, "Datenbank noch nicht korrekt geladen!");
            return;
        }

        try {
            var rk = new List<string>();
            var cek = new List<string>();

            foreach (var thisWork in data) {
                if (TableName == thisWork.TableName && thisWork.DateTimeUtc > IsInCache) {
                    Undo.Add(thisWork);

                    if (thisWork.Comand.IsObsolete()) {
                        // Nix tun
                    } else if (thisWork.Comand.IsCommand()) {

                        #region Befehle

                        switch (thisWork.Comand) {
                            case DatabaseDataType.Comand_RemoveColumn:
                                _ = Column.SetValueInternal(thisWork.Comand, Reason.LoadReload, thisWork.ColName);
                                break;

                            //case DatabaseDataType.Comand_AddColumnByKey:
                            //case DatabaseDataType.Comand_AddColumn:
                            //    _ = Column.SetValueInternal(t, true, columnname);
                            //    var c = Column.SearchByKey(LongParse(columnkey));
                            //    var name = _sql.GetLastColumnName(TableName, c.KeyName);
                            //    _ = SetValueInternal(DatabaseDataType.ColumnName, name, c.Name, null, true);
                            //    c.RefreshColumnsData(); // muss sein, alternativ alle geladenen Zeilen neu laden
                            //    break;

                            case DatabaseDataType.Comand_AddColumnByName:
                                _ = Column.SetValueInternal(thisWork.Comand, Reason.LoadReload, thisWork.ChangedTo); // ColumName kann nicht benutzt werden, da beim erstellen der SYS_Undo keine Spalte bekannt ist und nicht gespeichert wird
                                var c2 = Column.Exists(thisWork.ChangedTo);
                                //var columnname = _sql.GetLastColumnName(TableName, c.KeyName);
                                //_ = SetValueInternal(DatabaseDataType.ColumnKey, columnkey, c2.Name, null, true);
                                c2?.RefreshColumnsData(); // muss sein, alternativ alle geladenen Zeilen neu laden
                                break;

                            case DatabaseDataType.Comand_AddRow:
                                _ = Row.SetValueInternal(thisWork.Comand, thisWork.ChangedTo, null, Reason.LoadReload); // RowKey kann nicht benutzt werden, da beim erstellen der SYS_Undo keine Zeile bekannt ist und nicht gespeichert wird
                                _ = rk.AddIfNotExists(thisWork.ChangedTo); // Nachher auch laden
                                break;

                            case DatabaseDataType.Comand_RemoveRow:
                                var r = Row.SearchByKey(thisWork.RowKey);
                                _ = Row.SetValueInternal(thisWork.Comand, r?.KeyName, r, Reason.LoadReload);
                                break;

                            default:
                                Develop.DebugPrint(thisWork.Comand);
                                break;
                        }

                        #endregion
                    } else if (!string.IsNullOrEmpty(thisWork.RowKey)) {

                        #region Zeilen zum neu Einlesen merken uns Spaltenbreite invalidieren

                        var c = Column.Exists(thisWork.ColName);
                        var r = Row.SearchByKey(thisWork.RowKey);
                        if (r != null && c != null) {
                            // Kann sein, dass der Benutzer hier ja schon eine Zeile oder so gelöscht hat,
                            // aber anderer PC hat bei der noch vorhandenen Zeile eine Änderung
                            if (thisWork.DateTimeUtc > r.IsInCache || thisWork.DateTimeUtc > c.IsInCache) {
                                _ = rk.AddIfNotExists(r.KeyName);
                                _ = cek.AddIfNotExists(CellCollection.KeyOfCell(c, r));
                                c.Invalidate_ContentWidth();
                            }
                        }

                        #endregion
                    } else if (thisWork.Comand.IsDatabaseTag()) {

                        #region Datenbank-Styles

                        var v = SQL?.GetStyleData(thisWork.TableName, thisWork.Comand, DatabaseProperty, SysStyle);
                        if (v != null) { _ = SetValueInternal(thisWork.Comand, v, null, null, Reason.LoadReload, Generic.UserName, DateTime.UtcNow); }

                        #endregion
                    } else if (thisWork.Comand.IsColumnTag()) {

                        #region Spalten-Styles

                        var c = Column.Exists(thisWork.ColName);
                        if (c != null && !c.IsDisposed) {
                            var v = SQL?.GetStyleData(thisWork.TableName, thisWork.Comand, c.KeyName, SysStyle);
                            if (v != null) { _ = SetValueInternal(thisWork.Comand, v, c, null, Reason.LoadReload, Generic.UserName, DateTime.UtcNow); }
                        }

                        #endregion
                    }
                }
            }

            var (_, errormessage) = RefreshRowData(rk, true);
            if (!string.IsNullOrEmpty(errormessage)) {
                OnDropMessage(FehlerArt.Fehler, errormessage);
            }

            foreach (var thisc in cek) {
                Cell.DataOfCellKey(thisc, out var c, out var r);
                if (c != null && r != null) {
                    Cell.OnCellValueChanged(new EventArgs.CellChangedEventArgs(c, r, Reason.LoadReload));
                }
            }

            OnInvalidateView();
        } catch {
            Develop.CheckStackForOverflow();
            DoLastChanges(data);
        }

        //if (_loadingCount < 1) { Develop.DebugPrint("Loading <0!"); }
        //_loadingCount--;
    }

    private void GenerateTimer() {
        if (_timer != null) { return; }
        _timerTimeStamp = DateTime.UtcNow.AddMinutes(-5);
        _timer = new Timer(CheckSysUndo);
        _ = _timer.Change(10000, 10000);
    }

    private void LoadFromSqlBack(List<FilterItem>? preselection) {
        OnLoading();
        //Develop.DebugPrint(FehlerArt.DevelopInfo, "Loading++");

        try {

            #region Spalten richtig stellen

            var columnsToLoad = SQL?.GetColumnNames(TableName.ToUpper());
            if (columnsToLoad != null) {
                //_ = columnsToLoad.Remove("RK");

                #region Nicht mehr vorhandene Spalten löschen

                var columnsToDelete = new List<ColumnItem>();
                foreach (var thiscol in Column) {
                    if (!columnsToLoad.Contains(thiscol.KeyName.ToUpper())) {
                        columnsToDelete.Add(thiscol);
                    }
                }

                foreach (var thiscol in columnsToDelete) {
                    Column.Remove(thiscol, "Reload, Spalten zu viel");
                }

                #endregion

                #region Spalten erstellen

                foreach (var thisCol in columnsToLoad) {
                    var column = Column.Exists(thisCol);
                    if (column == null || column.IsDisposed) {
                        _ = Column.SetValueInternal(DatabaseDataType.Comand_AddColumnByName, Reason.LoadReload, thisCol);
                        column = Column.Exists(thisCol);

                        if (column == null || column.IsDisposed) {
                            Develop.DebugPrint(FehlerArt.Fehler, "Spaltenname nicht gefunden");
                            return;
                        }
                    }

                    //if (_sql != null) {
                    //    GetColumnAttributesColumn(column, _sql);
                    //}
                }
            }

            Column.GetSystems();

            #endregion

            #endregion

            //#region Datenbank Eigenschaften laden
            SQL?.GetStyleDataAll(this);

            //#endregion

            RepairColumnArrangements(Reason.LoadReload);

            #region Alle ZEILENKEYS laden

            var cl = new List<ColumnItem?>()
                        {Column.First(),
                         Column.SysChapter,
                         Column.SysLocked,
                         Column.SysCorrect};

            foreach (var thisColumn in Column) {
                thisColumn.IsInCache = null;
            }
            SQL?.LoadColumns(TableName, Row, cl, true, preselection);

            #endregion

            _ = SQL?.CloseConnection();

            Cell.RemoveOrphans();
        } catch {
            LoadFromSqlBack(preselection);
            return;
        }

        RepairAfterParse();
        OnLoaded();
        CreateWatcher();
        GenerateTimer();
        _ = ExecuteScript(ScriptEventTypes.loaded, string.Empty, true, null, null);
    }

    #endregion
}