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

using BlueBasics;
using BlueBasics.Enums;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using static BlueBasics.Converter;

namespace BlueDatabase;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class DatabaseSQLLite : DatabaseAbstract {

    #region Fields

    /// <summary>
    /// Nicht static, weil verschiedene Datenbankverbindngen möglich sind.
    /// </summary>
    public readonly SQLBackAbstract? _sql;

    private static bool _isInTimer;

    /// <summary>
    /// Der Globale Timer, der die Sys_Undo Datenbank abfrägt
    /// </summary>
    private static System.Threading.Timer? _timer;

    /// <summary>
    /// Der Zeitstempel der letzten Abfrage des _timer
    /// </summary>
    private static DateTime _timerTimeStamp = DateTime.UtcNow.AddSeconds(-0.5);

    #endregion

    #region Constructors

    public DatabaseSQLLite(ConnectionInfo ci) : this(((DatabaseSQLLite?)ci.Provider)?._sql, false, ci.TableName) { }

    public DatabaseSQLLite(SQLBackAbstract? sql, bool readOnly, string tablename) : base(tablename, readOnly) {
        if (sql == null) {
            Develop.DebugPrint(FehlerArt.Fehler, "Keine SQL_Verbindung übergeben.");
            return;
        }

        _sql = sql.OtherTable(tablename);

        Initialize();

        if (sql != null) {
            LoadFromSQLBack();
        }
    }

    #endregion

    #region Properties

    public static string DatabaseId => typeof(DatabaseSQLLite).Name;

    //private bool _isLoading = false;
    public override ConnectionInfo ConnectionData {
        get {
            var connectionData = _sql.ConnectionData(TableName);
            connectionData.Provider = this;
            return connectionData;
        }
    }

    #endregion

    #region Methods

    public override List<ConnectionInfo>? AllAvailableTables(List<DatabaseAbstract>? allreadychecked) {
        if (allreadychecked != null) {
            foreach (var thisa in allreadychecked) {
                if (thisa is DatabaseSQLLite db) {
                    if (db._sql.ConnectionString == _sql.ConnectionString) { return null; }
                }
            }
        }

        var tb = _sql.Tables();

        var l = new List<ConnectionInfo>();

        foreach (var thistb in tb) {
            l.Add(ConnectionDataOfOtherTable(thistb, false));
        }

        return l;
    }

    //        return false;
    //    }
    //}
    public override ConnectionInfo? ConnectionDataOfOtherTable(string tableName, bool checkExists) {
        if (checkExists) {
            var t = AllAvailableTables(null);

            foreach (var thisT in t) {
                if (string.Equals(tableName, thisT.TableName, StringComparison.InvariantCultureIgnoreCase)) {
                    return thisT;
                }
            }
            return null;
        }

        var connectionData = _sql.ConnectionData(tableName);
        connectionData.Provider = this;
        return connectionData;
    }

    public override void RefreshColumnsData(List<ColumnItem?>? columns) {
        if (columns == null || columns.Count == 0) { return; }
        var need = false;
        foreach (var thisColumn in columns) {
            if (thisColumn != null && thisColumn.IsInCache == null) {
                need = true;
            }
        }

        if (!need) { return; }

        OnDropMessage(FehlerArt.Info, "Lade " + columns.Count + " Spalte(n) nach.");

        try {
            _sql.LoadColumns(TableName, columns);
        } catch {
            RefreshColumnsData(columns);
        }

        OnDropMessage(FehlerArt.Info, string.Empty);
    }

    //    LoadFromSQLBack();
    //}

    public override bool RefreshRowData(List<RowItem> rows, bool refreshAlways) {
        if (rows == null || rows.Count == 0) { return false; }

        var l = new ListExt<RowItem>();

        foreach (var thisr in rows) {
            if (refreshAlways || thisr.IsInCache == null) {
                _ = l.AddIfNotExists(thisr);
            }
        }

        if (l.Count == 0) { return false; }

        try {
            _sql.LoadRow(TableName, l, refreshAlways);
        } catch {
            return RefreshRowData(rows, refreshAlways);
        }

        return true;
    }

    public override bool Save() => _sql.ConnectionOk;

    public override string UndoText(ColumnItem? column, RowItem? row) => string.Empty;

    /// <summary>
    /// Liest die Spaltenattribute aus der Style-Datenbank und schreibt sie in die Spalte
    /// Achtung: Die Connection wird nicht geschlossen!
    /// </summary>
    /// <param name="column"></param>
    /// <param name="sql"></param>

    internal void GetColumnAttributesColumn(ColumnItem column, SQLBackAbstract sql) {
        var l = sql.GetStyleDataAll(TableName.FileNameWithoutSuffix(), column.Name);
        if (l != null && l.Count > 0) {
            foreach (var thisstyle in l) {
                _ = Enum.TryParse(thisstyle.Key, out DatabaseDataType t);
                if (!t.IsObsolete()) {
                    _ = column.SetValueInternal(t, thisstyle.Value, true);
                }
            }
        }
    }

    internal override string SetValueInternal(DatabaseDataType type, string value, string? columnName, long? rowkey, bool isLoading) {
        if (IsDisposed) { return "Datenbank verworfen!"; }

        if (type.IsObsolete()) { return string.Empty; }

        if (!isLoading && !ReadOnly) {
            var c = Column.Exists(columnName);

            _ = (_sql?.SetValueInternal(TableName, type, value, c?.Name, rowkey, isLoading));
        }

        return base.SetValueInternal(type, value, columnName, rowkey, isLoading);
    }

    protected override void AddUndo(string tableName, DatabaseDataType comand, string? columnName, long? rowKey, string previousValue, string changedTo, string userName, string comment) {
        //var ck = Column.Exists(columnName)?.Key ?? -1;
        _ = _sql.AddUndo(tableName, comand, columnName, rowKey, previousValue, changedTo, UserName, comment);
    }

    protected override void SetUserDidSomething() { }

    //protected override string SpecialErrorReason(ErrorReason mode) => string.Empty;

    private static void CheckSysUndo(object state) {
        if (DateTime.UtcNow.Subtract(_timerTimeStamp).TotalSeconds < 180) { return; }

        if (CriticalState()) { return; }

        if (_isInTimer) { return; }
        _isInTimer = true;

        var fd = DateTime.UtcNow;

        try {
            var done = new List<DatabaseAbstract>();

            foreach (var thisDB in AllFiles) {
                if (!done.Contains(thisDB)) {
                    if (thisDB is DatabaseSQLLite thisDBSqlLite) {
                        var db = LoadedDatabasesWithThisSQL(thisDBSqlLite._sql);
                        done.AddRange(db);

                        var erg = thisDBSqlLite._sql.GetLastChanges(db, _timerTimeStamp.AddSeconds(-0.01), fd);
                        if (erg == null) { _isInTimer = false; return; } // Später ein neuer Versuch

                        foreach (var thisdb in db) {
                            thisdb.DoLastChanges(erg);
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

    private static List<DatabaseSQLLite> LoadedDatabasesWithThisSQL(SQLBackAbstract sql) {
        var oo = new List<DatabaseSQLLite>();
        foreach (var thisDb in AllFiles) {
            if (thisDb is DatabaseSQLLite thidDBSQLLIte) {
                if (sql.ConnectionString == thidDBSQLLIte._sql.ConnectionString) {
                    oo.Add(thidDBSQLLIte);
                }
            }
        }

        return oo;
    }

    private void DoLastChanges(List<(string tablename, string comand, string columnname, string rowkey, DateTime timecode)>? data) {
        if (data == null) { return; }
        if (IsDisposed) { return; }

        if (IsInCache == null) {
            Develop.DebugPrint(FehlerArt.Fehler, "Datenbank noch nicht korrekt geladen!");
            return;
        }

        try {
            var rk = new List<long>();

            foreach (var (tablename, comand, columnname, rowkey, timecode) in data) {
                if (TableName == tablename && timecode > IsInCache) {
                    _ = Enum.TryParse(comand, out DatabaseDataType t);

                    if (t.IsObsolete()) {
                        // Nix tun
                    }
                     //else if (t == DatabaseDataType.ColumnName) {
                     //    #region Sonderbehandlung: ColumnName

                     //    // Sonderbehandlung!
                     //    var c = Column.SearchByKey(LongParse(columnkey));

                     //    if (c != null) {
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
                                _ = Column.SetValueInternal(t, true, columnname);
                                var c2 = Column.Exists(columnname);
                                //var columnname = _sql.GetLastColumnName(TableName, c.Key);
                                //_ = SetValueInternal(DatabaseDataType.ColumnKey, columnkey, c2.Name, null, true);
                                c2.RefreshColumnsData(); // muss sein, alternativ alle geladenen Zeilen neu laden
                                break;

                            case DatabaseDataType.Comand_AddRow:
                                _ = Row.SetValueInternal(t, LongParse(rowkey), true);
                                _ = rk.AddIfNotExists(LongParse(rowkey)); // Nachher auch laden
                                break;

                            case DatabaseDataType.Comand_RemoveRow:
                                _ = Row.SetValueInternal(t, LongParse(rowkey), true);
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
                        //if (r == null) { Develop.DebugPrint(FehlerArt.Fehler, "Zeile nicht gefunden"); }
                        //if (c == null) { Develop.DebugPrint(FehlerArt.Fehler, "Spalte nicht gefunden"); }
                        if (r != null && c != null) {
                            // Kann sein, dass der Bentzer hier ja schon eine Zeile oder so geklscht hat
                            // hier geklscht, aber anderer PC mat bei der noch vorhanden Zeile eine Änderung
                            if (timecode > r.IsInCache || timecode > c.IsInCache) {
                                _ = rk.AddIfNotExists(r.Key);
                                c?.Invalidate_ContentWidth();
                            }
                        }

                        #endregion
                    } else if (t.IsDatabaseTag()) {

                        #region Datenbank-Styles

                        var v = _sql.GetStyleData(tablename, comand, "~DATABASE~");
                        if (v != null) { _ = SetValueInternal(t, v, null, null, true); }

                        #endregion
                    } else if (t.IsColumnTag()) {

                        #region Spalten-Styles

                        var c = Column.Exists(columnname);
                        if (c != null) {
                            var v = _sql.GetStyleData(tablename, comand, c.Name);
                            if (v != null) { _ = SetValueInternal(t, v, c.Name, null, true); }
                        }

                        #endregion
                    }
                }
            }

            _ = RefreshRowData(rk, true);
        } catch {
            DoLastChanges(data);
        }

        //if (_loadingCount < 1) { Develop.DebugPrint("Loading <0!"); }
        //_loadingCount--;
    }

    private void GenerateTimer() {
        if (_timer != null) { return; }
        _timerTimeStamp = DateTime.UtcNow.AddMinutes(-5);
        _timer = new System.Threading.Timer(CheckSysUndo);
        _ = _timer.Change(10000, 10000);
    }

    private void LoadFromSQLBack() {
        OnLoading();
        //Develop.DebugPrint(FehlerArt.DevelopInfo, "Loading++");

        try {
            Column.ThrowEvents = false;
            Row.ThrowEvents = false;

            #region Spalten richtig stellen

            var columnsToLoad = _sql.GetColumnNames(TableName.ToUpper());
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
                if (column == null) {
                    //var ck = Column.NextColumnKey();
                    _ = Column.SetValueInternal(DatabaseDataType.Comand_AddColumnByName, true, thisCol);
                    var co = Column.Exists(thisCol);
                    //_ = SetValueInternal(DatabaseDataType.ColumnKey, ck.ToString(), co?.Name, null, true);
                    column = Column.Exists(thisCol);
                    if (column == null) { Develop.DebugPrint(FehlerArt.Fehler, "Spaltenname nicht gefunden"); return; }
                    //column = Column.SearchByKey(ck);

                    //column = new ColumnItem(this, thisCol, Column.NextColumnKey()); // Column.GenerateAndAdd(Column.NextColumnKey(), thisCol);
                }
                if (column == null) { Develop.DebugPrint(FehlerArt.Fehler, "Spalte nicht gefunden"); return; }
                GetColumnAttributesColumn(column, _sql);
            }

            Column.GetSystems();

            #endregion

            #endregion

            #region Datenbank Eigenschaften laden

            var l = _sql.GetStyleDataAll(TableName, "~DATABASE~");
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

            _sql.LoadAllRowKeys(TableName, Row);

            foreach (var thisColumn in Column) {
                thisColumn.IsInCache = null;
            }

            #endregion

            _ = _sql.CloseConnection();

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
            LoadFromSQLBack();
            return;
        }

        RepairAfterParse();
        OnLoaded();
        CreateWatcher();
        GenerateTimer();
    }

    #endregion
}