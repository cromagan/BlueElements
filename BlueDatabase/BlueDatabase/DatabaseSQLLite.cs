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
using System.Threading;
using static BlueBasics.Converter;

namespace BlueDatabase;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class DatabaseSQLLite : DatabaseAbstract {

    #region Fields

    /// <summary>
    /// Nicht static, weil verschiedene Datenbankverbindngen möglich sind.
    /// </summary>
    public readonly SQLBackAbstract _sql;

    private static bool _isInTimer = false;

    /// <summary>
    /// Der Globale Timer, der die Sys_Undo Datenbank abfrägt
    /// </summary>
    private static Timer? _timer = null;

    /// <summary>
    /// Der Zeitstempel der letzten Abfrage des _timer
    /// </summary>
    private static DateTime _timerTimeStamp = DateTime.UtcNow.AddMinutes(-5);

    #endregion

    //private bool _checkedAndReloadNeed;

    //private DateTime _lastCheck = DateTime.Now;

    //private int _loadingCount = 0;

    //private bool _isLoading = false;

    #region Constructors

    public DatabaseSQLLite(SQLBackAbstract sql, bool readOnly, string tablename) : base(tablename, readOnly) {
        AllFiles.Add(this);

        _sql = sql;

        Develop.StartService();

        Initialize();
        //_checkedAndReloadNeed = true;

        if (sql != null) {
            //DropConstructorMessage?.Invoke(this, new MessageEventArgs(enFehlerArt.Info, "Lade Datenbank aus Dateisystem: \r\n" + tablename.FileNameWithoutSuffix()));
            LoadFromSQLBack();
        }
    }

    #endregion

    #region Properties

    public override ConnectionInfo ConnectionData {
        get {
            var connectionData = _sql.ConnectionData(TableName, true);
            connectionData.Provider = this;
            return connectionData;
        }
    }

    #endregion

    //public override string Filename => _sql.Filename;
    //public override bool IsLoading { get => _isLoading; }

    //public override bool ReloadNeeded => false;

    //public override bool ReloadNeededSoft => false;

    //public override bool ReloadNeeded {
    //    get {
    //        if (string.IsNullOrEmpty(TableName)) { return false; }
    //        if (_checkedAndReloadNeed) { return true; }
    //        _lastCheck = DateTime.Now;

    //        if (_sql.GetStyleData(TableName, DatabaseDataType.TimeCode.ToString(), string.Empty) != TimeCode) {
    //            _checkedAndReloadNeed = true;
    //            return true;
    //        }

    //        return false;
    //    }
    //}

    //public override bool ReloadNeededSoft {
    //    get {
    //        if (string.IsNullOrEmpty(TableName)) { return false; }
    //        if (_checkedAndReloadNeed) { return true; }

    //        if (DateTime.Now.Subtract(_lastCheck).TotalSeconds > 20) {
    //            return ReloadNeeded;
    //        }

    //        return false;
    //    }
    //}

    #region Methods

    public override List<ConnectionInfo>? AllAvailableTables(List<DatabaseAbstract>? allreadychecked) {
        if (allreadychecked != null) {
            foreach (var thisa in allreadychecked) {
                if (thisa is DatabaseSQLLite db) {
                    if (db._sql == _sql) { return null; }
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

    public override ConnectionInfo? ConnectionDataOfOtherTable(string tableName, bool checkExists) {
        if (checkExists) {
            var t = AllAvailableTables();

            foreach (var thisT in t) {
                if (string.Equals(tableName, thisT.TableName, StringComparison.InvariantCultureIgnoreCase)) {
                    return thisT;
                }
            }
            return null;
        }

        var connectionData = _sql.ConnectionData(tableName, true);
        connectionData.Provider = this;
        return connectionData;
    }

    //public override void Load_Reload() {
    //    if (!ReloadNeeded) { return; }

    //    LoadFromSQLBack();
    //}

    public override void RefreshColumnsData(List<ColumnItem>? columns) {
        if (columns == null || columns.Count == 0) { return; }
        if (columns.Count == 1 && columns[0].IsInCache) { return; }

        try {
            _sql.LoadColumns(TableName, columns);
        } catch {
            RefreshColumnsData(columns);
        }
    }

    public override bool RefreshRowData(List<RowItem> rows, bool refreshAlways) {
        if (rows == null || rows.Count == 0) { return false; }

        var l = new ListExt<RowItem>();

        foreach (var thisr in rows) {
            if (refreshAlways || !thisr.IsInCache) {
                l.AddIfNotExists(thisr);
            }
        }

        if (l.Count == 0) { return false; }

        try {
            _sql.LoadRow(TableName, l);
        } catch {
            return RefreshRowData(rows, refreshAlways);
        }

        return true;
    }

    public override bool Save() => _sql.ConnectionOk;

    public override string UndoText(ColumnItem? column, RowItem? row) => string.Empty;

    //public override void UnlockHard() { }

    //public override void WaitEditable() { }

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
                Enum.TryParse(thisstyle.Key, out DatabaseDataType t);
                if (t != (DatabaseDataType)0) {
                    column.SetValueInternal(t, thisstyle.Value, true);
                }
            }
        }
    }

    protected override void AddUndo(string tableName, DatabaseDataType comand, long? columnKey, long? rowKey, string previousValue, string changedTo, string userName) {
        _sql.AddUndo(tableName, comand, columnKey, rowKey, previousValue, changedTo, UserName);
    }

    protected override void SetUserDidSomething() { }

    protected override string SetValueInternal(DatabaseDataType type, string value, long? columnkey, long? rowkey, int width, int height, bool isLoading) {
        if (!isLoading && !ReadOnly) {
            var c = Column.SearchByKey(columnkey);
            if (type == DatabaseDataType.ColumnName) {
                _sql?.RenameColumn(TableName, c?.Name.ToUpper(), value.ToUpper());
            }
            _sql?.SetValueInternal(TableName, type, value, c?.Name, columnkey, rowkey, -1, -1, isLoading);
        }

        return base.SetValueInternal(type, value, columnkey, rowkey, width, height, isLoading);
    }

    //protected override string SpecialErrorReason(ErrorReason mode) => string.Empty;

    private static void CheckSysUndo(object state) {
        if (DateTime.UtcNow.Subtract(_timerTimeStamp).TotalSeconds < 180) { return; }

        //foreach (var thisDB in AllFiles) {
        //    if (thisDB.IsLoadingx) { return; }
        //}

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
                if (thidDBSQLLIte._sql == sql) {
                    oo.Add(thidDBSQLLIte);
                }
            }
        }

        return oo;
    }

    private void DoLastChanges(List<(string tablename, string comand, string columnkey, string rowkey)>? data) {
        if (data == null) { return; }

        //_loadingCount++;

        try {
            var rk = new List<long>();

            foreach (var (tablename, comand, columnkey, rowkey) in data) {
                if (TableName == tablename) {
                    Enum.TryParse(comand, out DatabaseDataType t);

                    if (t == (DatabaseDataType)0) {
                        // Nix tun
                    } else if (t == DatabaseDataType.ColumnName) {

                        #region Sonderbehandlung: ColumnName

                        // Sonderbehandlung!
                        var c = Column.SearchByKey(LongParse(columnkey));

                        if (c != null) {
                            var newn = _sql.GetLastColumnName(tablename, c.Key);
                            if (c.Name != newn) {
                                _sql.RenameColumn(tablename, c.Name, newn);
                            }
                        }

                        #endregion
                    } else if (t.IsCommand()) {

                        #region Befehle

                        switch (t) {
                            case DatabaseDataType.Comand_RemoveColumn:
                                Column.SetValueInternal(t, LongParse(columnkey), true);
                                break;

                            case DatabaseDataType.Comand_AddColumn:
                                Column.SetValueInternal(t, LongParse(columnkey), true);
                                var c = Column.SearchByKey(LongParse(columnkey));
                                var name = _sql.GetLastColumnName(TableName, c.Key);
                                SetValueInternal(DatabaseDataType.ColumnName, name, c.Key, null, -1, -1, true);
                                c.RefreshColumnsData(); // muss sein, alternativ alle geladenen Zeilen neu laden
                                break;

                            case DatabaseDataType.Comand_AddRow:
                                Row.SetValueInternal(t, LongParse(rowkey), true);
                                rk.AddIfNotExists(LongParse(rowkey)); // Nachher auch laden
                                break;

                            case DatabaseDataType.Comand_RemoveRow:
                                Row.SetValueInternal(t, LongParse(rowkey), true);
                                break;

                            default:
                                Develop.DebugPrint(t);
                                break;
                        }

                        #endregion
                    } else if (!string.IsNullOrEmpty(rowkey)) {

                        #region Zeilen zum neu Einlesen merken uns Spaltenbreite invalidierne

                        rk.AddIfNotExists(LongParse(rowkey));

                        var c = Column.SearchByKey(LongParse(columnkey));
                        c.Invalidate_ContentWidth();

                        #endregion
                    } else if (t.IsDatabaseTag()) {

                        #region Datenbank-Styles

                        var v = _sql.GetStyleData(tablename, comand, "~DATABASE~");
                        if (v != null) { SetValueInternal(t, v, null, null, -1, -1, true); }

                        #endregion
                    } else if (t.IsColumnTag()) {

                        #region Spalten-Styles

                        var c = Column.SearchByKey(LongParse(columnkey));
                        if (c != null) {
                            var v = _sql.GetStyleData(tablename, comand, c.Name);
                            if (v != null) { SetValueInternal(t, v, c.Key, null, -1, -1, true); }
                        }

                        #endregion
                    }
                }
            }

            RefreshRowData(rk, true);
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
        _timer.Change(10000, 10000);
    }

    private void LoadFromSQLBack() {
        OnLoading();
        //Develop.DebugPrint(FehlerArt.DevelopInfo, "Loading++");

        try {
            Column.ThrowEvents = false;
            Row.ThrowEvents = false;

            #region Spalten richtig stellen

            var columnsToLoad = _sql.GetColumnNames(TableName.ToUpper());
            columnsToLoad.Remove("RK");

            #region Nicht mehr vorhandene Spalten löschen

            var columnsToDelete = new List<ColumnItem>();
            foreach (var thiscol in Column) {
                if (!columnsToLoad.Contains(thiscol.Name.ToUpper())) {
                    columnsToDelete.Add(thiscol);
                }
            }

            foreach (var thiscol in columnsToDelete) {
                Column.Remove(thiscol);
            }

            #endregion

            #region Spalten erstellen

            foreach (var thisCol in columnsToLoad) {
                var column = Column.Exists(thisCol);
                if (column == null) {
                    var ck = Column.NextColumnKey();
                    Column.SetValueInternal(DatabaseDataType.Comand_AddColumn, ck, true);
                    SetValueInternal(DatabaseDataType.ColumnName, thisCol, ck, null, -1, -1, true);
                    column = Column.Exists(thisCol);
                    if (column == null) { Develop.DebugPrint(FehlerArt.Fehler, "Spaltenname nicht gefunden"); return; }
                    column = Column.SearchByKey(ck);

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
                    Enum.TryParse(thisstyle.Key, out DatabaseDataType t);
                    if (t != (DatabaseDataType)0) {
                        SetValueInternal(t, thisstyle.Value, null, null, -1, -1, true);
                    }
                }
            }

            #endregion

            #region  Alle ZEILEN laden

            _sql.LoadAllRowKeys(TableName, Row);

            foreach (var thisColumn in Column) {
                thisColumn.IsInCache = false;// string.Empty;
            }

            #endregion

            _sql.CloseConnection();

            Row.RemoveNullOrEmpty();
            Cell.RemoveOrphans();

            //_checkedAndReloadNeed = false;
            Column.ThrowEvents = true;
            Row.ThrowEvents = true;
        } catch {
            //if (_loadingCount < 1) { Develop.DebugPrint("Loading <0!"); }
            //_loadingCount--;
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