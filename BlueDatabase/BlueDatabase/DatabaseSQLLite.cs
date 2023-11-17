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

    #endregion

    #region Constructors

    public DatabaseSqlLite(ConnectionInfo ci, bool readOnly) : this(ci.TableName, readOnly, ci.MustBeFreezed, ((DatabaseSqlLite?)ci.Provider)?.SQL) { }

    public DatabaseSqlLite(string tablename, bool readOnly, string freezedReason, SqlBackAbstract? sql) : base(tablename, readOnly, freezedReason) {
        if (sql == null) {
            Develop.DebugPrint(FehlerArt.Fehler, "Keine SQL-Verbindung übergeben: " + tablename);
            return;
        }

        SQL = sql;
        sql.RepairAll(tablename);

        TableName = MakeValidTableName(tablename);

        if (!IsValidTableName(TableName, false)) {
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

    protected override bool DoCellChanges => false;

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

    public override List<UndoItem>? GetLastChanges(IEnumerable<DatabaseAbstract> db, DateTime fromUTC, DateTime toUTC) => SQL?.GetLastChanges(db, fromUTC, toUTC);

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

    public override (bool didreload, string errormessage) RefreshRowData(IEnumerable<RowItem> rows, bool refreshAlways) {
        if (rows == null || !rows.Any()) { return (false, string.Empty); }

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

    public override void RepairAfterParse() {
        IsInCache = DateTime.UtcNow;
        base.RepairAfterParse();
    }

    //    LoadFromSQLBack();
    //}
    public override bool Save() => SQL != null;

    public List<string> SQLLog() => Log;

    internal override bool IsNewRowPossible() {
        if (Column.SysCorrect == null) { return false; }
        return base.IsNewRowPossible();
    }

    internal override string SetValueInternal(DatabaseDataType type, string value, ColumnItem? column, RowItem? row, Reason reason, string user, DateTime datetimeutc, string comment) {
        if (IsDisposed) { return "Datenbank verworfen!"; }

        if (type.IsObsolete()) { return string.Empty; }

        if (reason != Reason.LoadReload && type != DatabaseDataType.SystemValue) {
            if (ReadOnly) { return "Datenbank schreibgeschützt!"; } // Sicherheitshalber!
            _ = SQL?.SetValueInternal(this, type, value, column, row, user, datetimeutc);
        }

        return base.SetValueInternal(type, value, column, row, reason, user, datetimeutc, comment);
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

    protected override IEnumerable<DatabaseAbstract> LoadedDatabasesWithSameServer() {
        var oo = new List<DatabaseSqlLite>();

        if (SQL == null) { return oo; }

        foreach (var thisDb in AllFiles) {
            if (thisDb is DatabaseSqlLite thidDbsqllIte) {
                if (SQL.ConnectionString == thidDbsqllIte.SQL?.ConnectionString) {
                    oo.Add(thidDbsqllIte);
                }
            }
        }

        return oo;
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

            var cl = new List<ColumnItem?>();

            if (Column.First() is ColumnItem c1) { cl.Add(c1); }
            if (Column.SysChapter is ColumnItem c2) { cl.Add(c2); }
            if (Column.SysLocked is ColumnItem c3) { cl.Add(c3); }
            if (Column.SysCorrect is ColumnItem c4) { cl.Add(c4); }

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