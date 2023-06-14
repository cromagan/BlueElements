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
using static BlueBasics.Converter;
using System.Linq;

namespace BlueDatabase;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class DatabaseSqlLite : DatabaseAbstract {

    #region Fields

    private static bool _isInTimer;

    /// <summary>
    /// Der Globale Timer, der die Sys_Undo Datenbank abfrägt
    /// </summary>
    private static Timer? _timer;

    /// <summary>
    /// Der Zeitstempel der letzten Abfrage des _timer
    /// </summary>
    private static DateTime _timerTimeStamp = DateTime.UtcNow.AddSeconds(-0.5);

    /// <summary>
    /// Nicht static, weil verschiedene Datenbankverbindngen möglich sind.
    /// </summary>
    private readonly SqlBackAbstract? _sql;

    private readonly string _tablename = string.Empty;

    #endregion

    #region Constructors

    public DatabaseSqlLite(ConnectionInfo ci) : this(((DatabaseSqlLite?)ci.Provider)?._sql, false, ci.TableName) { }

    public DatabaseSqlLite(SqlBackAbstract? sql, bool readOnly, string tablename) : base(readOnly) {
        if (sql == null) {
            Develop.DebugPrint(FehlerArt.Fehler, "Keine SQL-Verbindung übergeben: " + tablename);
            return;
        }

        _sql = sql.OtherTable(tablename);

        _tablename = SqlBackAbstract.MakeValidTableName(tablename);

        if (!SqlBackAbstract.IsValidTableName(_tablename, false)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Tabellenname ungültig: " + tablename);
        }

        Initialize();
        LoadFromSqlBack();
    }

    #endregion

    #region Properties

    public static string DatabaseId => typeof(DatabaseSqlLite).Name;

    public override ConnectionInfo ConnectionData {
        get {
            if (_sql == null) {
                return new(TableName, null, string.Empty, string.Empty);
            }
            var connectionData = _sql.ConnectionData(TableName);
            connectionData.Provider = this;
            return connectionData;
        }
    }

    public override string TableName => _tablename;

    #endregion

    #region Methods

    public static DatabaseAbstract? CanProvide(ConnectionInfo ci, NeedPassword? needPassword) {
        if (!DatabaseId.Equals(ci.DatabaseID, StringComparison.OrdinalIgnoreCase)) { return null; }

        var sql = ((DatabaseSqlLite?)ci.Provider)?._sql;
        if (sql == null) { return null; }

        var at = sql.Tables();
        if (at == null || !at.Contains(ci.TableName)) { return null; }
        return new DatabaseSqlLite(ci);
    }

    public override List<ConnectionInfo>? AllAvailableTables(List<DatabaseAbstract>? allreadychecked) {
        if (allreadychecked != null) {
            foreach (var thisa in allreadychecked) {
                if (thisa is DatabaseSqlLite db) {
                    if (db._sql?.ConnectionString == _sql?.ConnectionString) { return null; }
                }
            }
        }

        var tb = _sql?.Tables();
        var l = new List<ConnectionInfo>();

        if (tb == null) {
            Develop.DebugPrint(FehlerArt.Fehler, "Verbindung zur Datenbank gescheitert.");
            return l;
        }

        if (tb != null)
            foreach (var thistb in tb) {
                var t = ConnectionDataOfOtherTable(thistb, false);
                if (t != null) { l.Add(t); }
            }

        return l;
    }

    //        return false;
    //    }
    //}
    public override ConnectionInfo? ConnectionDataOfOtherTable(string tableName, bool checkExists) {
        if (checkExists) {
            var t = AllAvailableTables(null);

            if (t != null) {
                foreach (var thisT in t) {
                    if (string.Equals(tableName, thisT.TableName, StringComparison.InvariantCultureIgnoreCase)) {
                        return thisT;
                    }
                }
            }

            return null;
        }

        var connectionData = _sql?.ConnectionData(tableName);
        if (connectionData != null) {
            connectionData.Provider = this;
            return connectionData;
        }
        return null;
    }

    public override string EditableErrorReason(EditableErrorReasonType mode) {
        var m = base.EditableErrorReason(mode);
        if (!string.IsNullOrEmpty(m)) { return m; }

        if (_sql == null) { return "Keine SQL-Verbindung vorhanden"; }

        if (mode is EditableErrorReasonType.OnlyRead or EditableErrorReasonType.Load) { return string.Empty; }

        if (ReadOnly) { return "Datenbank schreibgeschützt!"; } // Immer abfragen, da Änderungen direkt gespeichert werden

        return string.Empty;
    }

    public override void RefreshColumnsData(List<ColumnItem>? columns) {
        if (columns == null || columns.Count == 0) { return; }

        if (_sql == null) {
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

        OnDropMessage(FehlerArt.Info, "Lade " + columns.Count + " Spalte(n) der Datenbank '" + TableName + "' nach.");

        try {
            _sql.LoadColumns(TableName, columns);
        } catch {
            RefreshColumnsData(columns);
        }

        OnDropMessage(FehlerArt.Info, string.Empty);
    }

    //    LoadFromSQLBack();
    //}

    public override (bool didreload, string errormessage) RefreshRowData(List<RowItem> rows, bool refreshAlways, List<RowItem>? sortedRows) {
        if (rows == null || rows.Count == 0) { return (false, string.Empty); }

        var l = new List<RowItem>();

        foreach (var thisr in rows) {
            if (refreshAlways || thisr.IsInCache == null) {
                l.Add(thisr);
            }
        }

        if (l.Count == 0) { return (false, string.Empty); }
        l = l.Distinct().ToList();

        //Row.DoLinkedDatabase(rows);

        OnDropMessage(FehlerArt.Info, "Lade " + l.Count + " Zeile(n) der Datenbank '" + TableName + "' nach.");

        if (_sql == null) { return (false, "SQL Verbindung fehlerhaft"); }

        try {
            return (true, _sql.LoadRow(TableName, l, refreshAlways, sortedRows, 1));
        } catch {
            return RefreshRowData(rows, refreshAlways, sortedRows);
        }
    }

    public override bool Save() => _sql != null && _sql.ConnectionOk;

    public override string UndoText(ColumnItem? column, RowItem? row) => string.Empty;

    /// <summary>
    /// Liest die Spaltenattribute aus der Style-Datenbank und schreibt sie in die Spalte
    /// Achtung: Die Connection wird nicht geschlossen!
    /// </summary>
    /// <param name="column"></param>
    /// <param name="sql"></param>

    internal void GetColumnAttributesColumn(ColumnItem column, SqlBackAbstract sql) {
        var l = sql.GetStyleDataAll(TableName.FileNameWithoutSuffix(), column.Name);

        if (l == null) {
            Develop.DebugPrint(FehlerArt.Fehler, "Datenbank Fehler");
            return;
        }

        if (l != null && l.Count > 0) {
            foreach (var thisstyle in l) {
                _ = Enum.TryParse(thisstyle.Key, out DatabaseDataType t);
                if (!t.IsObsolete()) {
                    _ = column.SetValueInternal(t, thisstyle.Value, true);
                }
            }
        }
    }

    internal override string SetValueInternal(DatabaseDataType type, string value, ColumnItem? column, RowItem? row, bool isLoading) {
        if (IsDisposed) { return "Datenbank verworfen!"; }

        if (type.IsObsolete()) { return string.Empty; }

        if (ReadOnly) { return "Datenbank schreibgeschützt!"; } // Sicherheitshalber!

        if (!isLoading) {
            _ = _sql?.SetValueInternal(TableName, type, value, column?.Name, row?.Key);
        }

        return base.SetValueInternal(type, value, column, row, isLoading);
    }

    protected override void AddUndo(string tableName, DatabaseDataType type, ColumnItem? column, RowItem? row, string previousValue, string changedTo, string userName, string comment) {
        if (IsDisposed) { return; }
        if (type.IsObsolete()) { return; }
        if (ReadOnly) { return; } // Sicherheitshalber!

        var columnName = column?.Name ?? string.Empty;
        var rowkey = row?.Key ?? -1;

        var err = _sql?.AddUndo(tableName, type, columnName, rowkey, previousValue, changedTo, comment);
        if (!string.IsNullOrEmpty(err)) {
            Develop.CheckStackForOverflow();
            AddUndo(tableName, type, column, row, previousValue, changedTo, userName, comment);
        }
    }

    private static void CheckSysUndo(object state) {
        if (DateTime.UtcNow.Subtract(_timerTimeStamp).TotalSeconds < 180) { return; }
        if (DateTime.UtcNow.Subtract(SqlBackAbstract.LastLoadUtc).TotalSeconds < 5) { return; }

        if (CriticalState()) { return; }

        if (_isInTimer) { return; }
        _isInTimer = true;

        var fd = DateTime.UtcNow;

        try {
            var done = new List<DatabaseAbstract>();

            foreach (var thisDb in AllFiles) {
                if (!done.Contains(thisDb)) {
                    if (thisDb is DatabaseSqlLite thisDbSqlLite) {
                        if (thisDbSqlLite._sql != null) {
                            var db = LoadedDatabasesWithThisSql(thisDbSqlLite._sql);
                            done.AddRange(db);

                            var erg = thisDbSqlLite._sql.GetLastChanges(db, _timerTimeStamp.AddSeconds(-0.01), fd);
                            if (erg == null) { _isInTimer = false; return; } // Später ein neuer Versuch

                            foreach (var thisdb in db) {
                                thisdb.DoLastChanges(erg);
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

    private static List<DatabaseSqlLite> LoadedDatabasesWithThisSql(SqlBackAbstract sql) {
        var oo = new List<DatabaseSqlLite>();
        foreach (var thisDb in AllFiles) {
            if (thisDb is DatabaseSqlLite thidDbsqllIte) {
                if (sql.ConnectionString == thidDbsqllIte._sql?.ConnectionString) {
                    oo.Add(thidDbsqllIte);
                }
            }
        }

        return oo;
    }

    private void DoLastChanges(List<(string tablename, string comand, string columnname, string rowkey, string newValue, DateTime timecode)>? data) {
        if (data == null) { return; }
        if (IsDisposed) { return; }

        if (IsInCache == null) {
            Develop.DebugPrint(FehlerArt.Fehler, "Datenbank noch nicht korrekt geladen!");
            return;
        }

        try {
            var rk = new List<long>();

            foreach (var (tablename, comand, columnname, rowkey, newvalue, timecode) in data) {
                if (TableName == tablename && timecode > IsInCache) {
                    _ = Enum.TryParse(comand, out DatabaseDataType t);

                    if (t.IsObsolete()) {
                        // Nix tun
                    }
                     //else if (t == DatabaseDataType.ColumnName) {
                     //    #region Sonderbehandlung: ColumnName

                     //    // Sonderbehandlung!
                     //    var c = Column.SearchByKey(LongParse(columnkey));

                     //    if (c != null && !c.IsDisposed) {
                     //        var newn = _sql.GetLastColumnName(tablename, c.Key);
                     //        c.Name = newn;
                     //    }

                     //    #endregion
                     else if (t.IsCommand()) {

                        #region Befehle

                        switch (t) {
                            case DatabaseDataType.Comand_RemoveColumn:
                                _ = Column.SetValueInternal(t, true, columnname);
                                break;

                            //case DatabaseDataType.Comand_AddColumnByKey:
                            //case DatabaseDataType.Comand_AddColumn:
                            //    _ = Column.SetValueInternal(t, true, columnname);
                            //    var c = Column.SearchByKey(LongParse(columnkey));
                            //    var name = _sql.GetLastColumnName(TableName, c.Key);
                            //    _ = SetValueInternal(DatabaseDataType.ColumnName, name, c.Name, null, true);
                            //    c.RefreshColumnsData(); // muss sein, alternativ alle geladenen Zeilen neu laden
                            //    break;

                            case DatabaseDataType.Comand_AddColumnByName:
                                _ = Column.SetValueInternal(t, true, newvalue); // ColumName kann nicht benutzt werden, da beim erstellen der SYS_Undo keine Spalte bekannt ist und nicht gespeichert wird
                                var c2 = Column.Exists(newvalue);
                                //var columnname = _sql.GetLastColumnName(TableName, c.Key);
                                //_ = SetValueInternal(DatabaseDataType.ColumnKey, columnkey, c2.Name, null, true);
                                c2?.RefreshColumnsData(); // muss sein, alternativ alle geladenen Zeilen neu laden
                                break;

                            case DatabaseDataType.Comand_AddRow:
                                _ = Row.SetValueInternal(t, LongParse(newvalue), null, true); // RowKey kann nicht benutzt werden, da beim erstellen der SYS_Undo keine Zeile bekannt ist und nicht gespeichert wird
                                _ = rk.AddIfNotExists(LongParse(newvalue)); // Nachher auch laden
                                break;

                            case DatabaseDataType.Comand_RemoveRow:
                                var r = Row.SearchByKey(LongParse(rowkey));
                                _ = Row.SetValueInternal(t, r?.Key, r, true);
                                break;

                            default:
                                Develop.DebugPrint(t);
                                break;
                        }

                        #endregion
                    } else if (!string.IsNullOrEmpty(rowkey)) {

                        #region Zeilen zum neu Einlesen merken uns Spaltenbreite invalidierne

                        var c = Column.Exists(columnname);
                        var r = Row.SearchByKey(LongParse(rowkey));
                        //if (r== null || r.IsDisposed) { Develop.DebugPrint(FehlerArt.Fehler, "Zeile nicht gefunden"); }
                        //if (c == null) { Develop.DebugPrint(FehlerArt.Fehler, "Spalte nicht gefunden"); }
                        if (r != null && c != null) {
                            // Kann sein, dass der Bentzer hier ja schon eine Zeile oder so geklscht hat
                            // hier geklscht, aber anderer PC mat bei der noch vorhanden Zeile eine Änderung
                            if (timecode > r.IsInCache || timecode > c.IsInCache) {
                                _ = rk.AddIfNotExists(r.Key);
                                c.Invalidate_ContentWidth();
                            }
                        }

                        #endregion
                    } else if (t.IsDatabaseTag()) {

                        #region Datenbank-Styles

                        var v = _sql?.GetStyleData(tablename, comand, "~DATABASE~");
                        if (v != null) { _ = SetValueInternal(t, v, null, null, true); }

                        #endregion
                    } else if (t.IsColumnTag()) {

                        #region Spalten-Styles

                        var c = Column.Exists(columnname);
                        if (c != null && !c.IsDisposed) {
                            var v = _sql?.GetStyleData(tablename, comand, c.Name);
                            if (v != null) { _ = SetValueInternal(t, v, c, null, true); }
                        }

                        #endregion
                    }
                }
            }

            var (_, errormessage) = RefreshRowData(rk, true, null);
            if (!string.IsNullOrEmpty(errormessage)) {
                OnDropMessage(FehlerArt.Fehler, errormessage);
            }
        } catch {
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

    private void LoadFromSqlBack() {
        OnLoading();
        //Develop.DebugPrint(FehlerArt.DevelopInfo, "Loading++");

        try {
            Column.ThrowEvents = false;
            Row.ThrowEvents = false;

            #region Spalten richtig stellen

            var columnsToLoad = _sql?.GetColumnNames(TableName.ToUpper());
            if (columnsToLoad != null) {
                _ = columnsToLoad.Remove("RK");

                #region Nicht mehr vorhandene Spalten löschen

                var columnsToDelete = new List<ColumnItem>();
                foreach (var thiscol in Column) {
                    if (!columnsToLoad.Contains(thiscol.Name.ToUpper())) {
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
                        _ = Column.SetValueInternal(DatabaseDataType.Comand_AddColumnByName, true, thisCol);
                        column = Column.Exists(thisCol);
                        if (Column == null || Column.IsDisposed) {
                            Develop.DebugPrint(FehlerArt.Fehler, "Spaltenname nicht gefunden");
                            return;
                        }
                    }

                    if (_sql != null) GetColumnAttributesColumn(column, _sql);
                }
            }

            Column.GetSystems();

            #endregion

            #endregion

            #region Datenbank Eigenschaften laden

            var l = _sql?.GetStyleDataAll(TableName, "~DATABASE~");

            if (l == null) {
                Develop.DebugPrint(FehlerArt.Fehler, "Datenbank Fehler");
                return;
            }

            if (l != null && l.Count > 0) {
                foreach (var thisstyle in l) {
                    _ = Enum.TryParse(thisstyle.Key, out DatabaseDataType t);
                    if (!t.IsObsolete()) {
                        _ = SetValueInternal(t, thisstyle.Value, null, null, true);
                    }
                }
            }

            #endregion

            #region  Alle ZEILEN laden

            _sql?.LoadAllRowKeys(TableName, Row);

            foreach (var thisColumn in Column) {
                thisColumn.IsInCache = null;
            }

            #endregion

            _ = _sql?.CloseConnection();

            //Row.RemoveNullOrEmpty();
            Cell.RemoveOrphans();

            //_checkedAndReloadNeed = false;
            Column.ThrowEvents = true;
            Row.ThrowEvents = true;
        } catch {
            //if (_loadingCount < 1) { Develop.DebugPrint("Loading <0!"); }
            //_loadingCount--;
            Column.ThrowEvents = true;
            Row.ThrowEvents = true;
            LoadFromSqlBack();
            return;
        }

        RepairAfterParse();
        OnLoaded();
        CreateWatcher();
        GenerateTimer();
        _ = ExecuteScript(EventTypes.database_loaded, string.Empty, true, null);
    }

    #endregion
}