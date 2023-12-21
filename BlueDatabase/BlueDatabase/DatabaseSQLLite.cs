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
using BlueBasics;
using BlueBasics.Enums;
using BlueDatabase.Enums;
using System.Linq;
using static BlueDatabase.SqlBackAbstract;
using BlueDatabase.EventArgs;
using System.Runtime.ConstrainedExecution;

namespace BlueDatabase;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class DatabaseSqlLite : Database {

    #region Fields

    /// <summary>
    /// Nicht static, weil verschiedene Datenbankverbindungen möglich sind.
    /// </summary>
    public readonly SqlBackAbstract? SQL;

    #endregion

    #region Constructors

    public DatabaseSqlLite(string tablename) : base(tablename) { }

    #endregion

    #region Properties

    public new static string DatabaseId => nameof(DatabaseSqlLite);

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

    protected override bool MultiUser => true;

    #endregion

    //public static Database? CanProvide(ConnectionInfo ci, bool readOnly, NeedPassword? needPassword) {
    //    if (!DatabaseId.Equals(ci.DatabaseId, StringComparison.OrdinalIgnoreCase)) { return null; }

    //    var sql = ((DatabaseSqlLite?)ci.Provider)?.SQL;
    //    if (sql == null) { return null; }

    //    var at = sql.Tables();
    //    if (at == null || !at.Contains(ci.TableName)) { return null; }
    //    return new DatabaseSqlLite(ci, readOnly);
    //}

    #region Methods

    //        return false;
    //    }
    //}
    public override ConnectionInfo? ConnectionDataOfOtherTable(string tableName, bool checkExists) {
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

                if (thisColumn.LinkedDatabase is Database dbl &&
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
            SQL.LoadColumns(TableName, Row, columns, false);
        } catch {
            Develop.CheckStackForOverflow();
            RefreshColumnsData(columns);
        }

        OnDropMessage(FehlerArt.Info, string.Empty);
    }

    public override (bool didreload, string errormessage) RefreshRowData(IEnumerable<RowItem> row) {
        if (row == null || !row.Any()) { return (false, string.Empty); }

        var l = new List<RowItem>();

        foreach (var thisr in row) {
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
            return RefreshRowData(row, refreshAlways);
        }
    }

    public override void RepairAfterParse() {
        IsInCache = DateTime.UtcNow;
        base.RepairAfterParse();
    }

    public override bool Save() => SQL != null;

    public List<string> SQLLog() => Log;

    internal override bool IsNewRowPossible() {
        if (Column.SysCorrect == null) { return false; }
        return base.IsNewRowPossible();
    }

    internal void LoadFromSqlBack(NeedPassword? needPassword, string freeze, bool readOnly, SqlBackAbstract sql) {
        SQL = sql;

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
                        _ = Column.ExecuteCommand(DatabaseDataType.Command_AddColumnByName, thisCol, Reason.InitialLoad);
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

            RepairColumnArrangements(Reason.InitialLoad);

            #region Alle ZEILENKEYS laden

            var cl = new List<ColumnItem?>();

            if (Column.First() is ColumnItem c1) { cl.Add(c1); }
            if (Column.SysChapter is ColumnItem c2) { cl.Add(c2); }
            if (Column.SysLocked is ColumnItem c3) { cl.Add(c3); }
            if (Column.SysCorrect is ColumnItem c4) { cl.Add(c4); }

            foreach (var thisColumn in Column) {
                thisColumn.IsInCache = null;
            }
            SQL?.LoadColumns(TableName, Row, cl, true);

            #endregion

            _ = SQL?.CloseConnection();

            Cell.RemoveOrphans();
        } catch {
            LoadFromSqlBack(needPassword, freeze, readOnly, sql);
            return;
        }

        RepairAfterParse();
        CheckSysUndoNow(Database.AllFiles);
        if (!string.IsNullOrEmpty(freeze)) { Freeze(freeze); }
        if (ReadOnly) { SetReadOnly(); }

        OnLoaded();
        CreateWatcher();
        GenerateTimer();
        _ = ExecuteScript(ScriptEventTypes.loaded, string.Empty, true, null, null);
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

    protected override List<ConnectionInfo>? AllAvailableTables(List<Database>? allreadychecked, string mustBeFreezed) {
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

    protected override void DoWorkAfterLastChanges(List<string>? files, List<ColumnItem> columnsAdded, List<RowItem> rowsAdded, DateTime starttimeUtc) {
        // NICHT BASE! SOnst wir alles auf im Cache gesetzt

        RefreshColumnsData(columnsAdded); // muss sein, alternativ alle geladenen Zeilen neu laden

        var rows = new List<RowItem>();
        rows.AddRange(rowsAdded);

        foreach (var thisc in cellschanged) {
            Cell.DataOfCellKey(thisc, out var c, out var r);
            if (c != null && r != null) {
                rows.AddIfNotExists(r);
            }
        }

        RefreshRowData(rows, true);

        //if (c != null) { colums.AddIfNotExists(c); }
        //if (r != null) { rows.AddIfNotExists(r); }
        //if (c != null && r != null) { _ = cells.AddIfNotExists(CellCollection.KeyOfCell(c, r)); }

        //        if (thisWork.Command.IsObsolete()) {
        //            // Nix tun
        //        } else if (thisWork.Command.IsCommand()) {
        //            #region Befehle

        //            switch (thisWork.Command) {
        //                case DatabaseDataType.Command_RemoveColumn:
        //                    _ = Column.ExecuteCommand(thisWork.Command, thisWork.ColName);
        //                    break;

        //                case DatabaseDataType.Command_AddColumnByName:
        //                    _ = Column.ExecuteCommand(thisWork.Command, thisWork.ChangedTo); // ColumName kann nicht benutzt werden, da beim erstellen der SYS_Undo keine Spalte bekannt ist und nicht gespeichert wird
        //                    var c2 = Column.Exists(thisWork.ChangedTo);
        //                    //var columnname = _sql.GetLastColumnName(TableName, c.KeyName);
        //                    //_ = SetValueInternal(DatabaseDataType.ColumnKey, columnkey, c2.Name, null, true);
        //                    c2?.RefreshColumnsData(); // muss sein, alternativ alle geladenen Zeilen neu laden
        //                    break;

        //                case DatabaseDataType.Command_AddRow:
        //                    _ = Row.ExecuteCommand(thisWork.Command, thisWork.ChangedTo, null); // RowKey kann nicht benutzt werden, da beim erstellen der SYS_Undo keine Zeile bekannt ist und nicht gespeichert wird
        //                    _ = rk.AddIfNotExists(thisWork.ChangedTo); // Nachher auch laden
        //                    break;

        //                case DatabaseDataType.Command_RemoveRow:
        //                    if (Row.SearchByKey(thisWork.RowKey) is RowItem r) {
        //                        _ = Row.ExecuteCommand(thisWork.Command, r.KeyName, r);
        //                    }
        //                    break;

        //                default:
        //                    Develop.DebugPrint(thisWork.Command);
        //                    break;
        //            }

        //            #endregion
        //        } else if (!string.IsNullOrEmpty(thisWork.RowKey)) {
        //            #region Zeilen zum neu Einlesen merken uns Spaltenbreite invalidieren

        //            var c = Column.Exists(thisWork.ColName);
        //            var r = Row.SearchByKey(thisWork.RowKey);
        //            if (r != null && c != null) {
        //                // Kann sein, dass der Benutzer hier ja schon eine Zeile oder so gelöscht hat,
        //                // aber anderer PC hat bei der noch vorhandenen Zeile eine Änderung

        //                if (DoCellChanges) {
        //                    _ = SetValueInternal(thisWork.Command, c, r, thisWork.ChangedTo);
        //                } else {
        //                    if (thisWork.DateTimeUtc > r.IsInCache || thisWork.DateTimeUtc > c.IsInCache) {
        //                        _ = rk.AddIfNotExists(r.KeyName);
        //                        _ = cek.AddIfNotExists(CellCollection.KeyOfCell(c, r));
        //                    }
        //                }
        //                c.Invalidate_ContentWidth();
        //            }

        //            #endregion
        //        } else if (thisWork.Command.IsDatabaseTag()) {
        //            #region Datenbank-Styles

        //            //var v = SQL?.GetStyleData(thisWork.TableName, thisWork.Command, DatabaseProperty, SysStyle);
        //            _ = SetValueInternal(thisWork.Command, null, null, thisWork.ChangedTo);

        //            #endregion
        //        } else if (thisWork.Command.IsColumnTag()) {
        //            #region Spalten-Styles

        //            var column = Column.Exists(thisWork.ColName);
        //            if (column != null && !column.IsDisposed) {
        //                //var v = SQL?.GetStyleData(thisWork.TableName, thisWork.Command, c.KeyName, SysStyle);
        //                _ = column.SetValueInternal(thisWork.Command, thisWork.ChangedTo);
        //            }

        //            #endregion
        //        }
        //    }
        //}

        //if (!DoCellChanges) {
        //    var (_, errormessage) = RefreshRowData(rk, true);
        //    if (!string.IsNullOrEmpty(errormessage)) {
        //        OnDropMessage(FehlerArt.Fehler, errormessage);
        //    }
        //}

        //foreach (var thisc in cells) {
        //    Cell.DataOfCellKey(thisc, out var c, out var r);
        //    if (c != null && r != null) {
        //        Cell.OnCellValueChanged(new CellEventArgs(c, r));
        //    }
        //}
    }

    protected override (List<UndoItem>? Changes, List<string>? Files) GetLastChanges(ICollection<Database> db, DateTime fromUtc, DateTime toUtc) => (SQL?.GetLastChanges(db, fromUtc, toUtc), null);

    protected override List<Database> LoadedDatabasesWithSameServer() {
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

    protected override string WriteValueToDiscOrServer(DatabaseDataType type, string value, ColumnItem? column, RowItem? row, string user, DateTime datetimeutc, string comment) {
        var f = base.WriteValueToDiscOrServer(type, value, column, row, user, datetimeutc, comment);
        if (!string.IsNullOrEmpty(f)) { return f; }
        HasPendingChanges = false; // Datenbank kann keine Pendungs haben

        if (type == DatabaseDataType.UndoInOne) { return string.empty; }

        if (ReadOnly) { return "Datenbank schreibgeschützt!"; } // Sicherheitshalber!
        if (SQL == null) { return "SQL-Verbindung verloren!"; }
        return SQL.WriteValueToServer(this, type, value, column, row, user, datetimeutc);
    }

    #endregion
}