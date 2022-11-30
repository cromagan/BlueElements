// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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
using System.Data.SqlTypes;
using System.Threading;
using static BlueBasics.Converter;

namespace BlueDatabase;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class DatabaseSQLLite : DatabaseAbstract {

    #region Fields

    /// <summary>
    /// Nicht static, weil verschiedene Datenbankverbindngen m�glich sind.
    /// </summary>
    public readonly SQLBackAbstract _sql;

    private static bool _isInTimer = false;

    /// <summary>
    /// Der Globale Timer, der die Sys_Undo Datenbank abfr�gt
    /// </summary>
    private static System.Threading.Timer? _timer = null;

    /// <summary>
    /// Der Zeitstempel der letzten Abfrage des _timer
    /// </summary>
    private static DateTime _timerTimeStamp = DateTime.UtcNow.AddMinutes(-5);

    private bool _checkedAndReloadNeed;

    private DateTime _lastCheck = DateTime.Now;

    #endregion

    #region Constructors

    public DatabaseSQLLite(SQLBackAbstract sql, bool readOnly, string tablename) : base(tablename, readOnly) {
        AllFiles.Add(this);

        _sql = sql;

        Develop.StartService();

        Initialize();
        _checkedAndReloadNeed = true;

        if (sql != null) {
            //DropConstructorMessage?.Invoke(this, new MessageEventArgs(enFehlerArt.Info, "Lade Datenbank aus Dateisystem: \r\n" + tablename.FileNameWithoutSuffix()));
            LoadFromSQLBack();
        }
        RepairAfterParse();

        GenerateTimer();
    }

    #endregion

    #region Properties

    public override ConnectionInfo ConnectionData {
        get {
            var ConnectionData = _sql.ConnectionData(TableName, true);
            ConnectionData.Provider = this;
            return ConnectionData;
        }
    }

    public override string Filename => _sql.Filename;

    public override bool IsLoading { get; protected set; }

    public override bool ReloadNeeded {
        get {
            if (string.IsNullOrEmpty(TableName)) { return false; }
            if (_checkedAndReloadNeed) { return true; }
            _lastCheck = DateTime.Now;

            if (_sql.GetStyleData(TableName, DatabaseDataType.TimeCode.ToString(), string.Empty) != TimeCode) {
                _checkedAndReloadNeed = true;
                return true;
            }

            return false;
        }
    }

    public override bool ReloadNeededSoft {
        get {
            if (string.IsNullOrEmpty(TableName)) { return false; }
            if (_checkedAndReloadNeed) { return true; }

            if (DateTime.Now.Subtract(_lastCheck).TotalSeconds > 20) {
                return ReloadNeeded;
            }

            return false;
        }
    }

    #endregion

    #region Methods

    public override List<ConnectionInfo>? AllAvailableTables(List<DatabaseAbstract>? allreadychecked) {
        if (allreadychecked != null) {
            foreach (var thisa in allreadychecked) {
                if (thisa is DatabaseSQL db) {
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

    public override void BlockReload(bool crashIsCurrentlyLoading) { }

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

        var ConnectionData = _sql.ConnectionData(tableName, true);
        ConnectionData.Provider = this;
        return ConnectionData;
    }

    public override void Load_Reload() {
        if (!ReloadNeeded) { return; }

        LoadFromSQLBack();
    }

    public override void RefreshColumnsData(List<ColumnItem>? columns) {
        if (columns == null || columns.Count == 0) { return; }
        //if(!ReloadNeeded) { return; }

        _sql.OpenConnection();
        var l = new ListExt<ColumnItem>();

        foreach (var thisc in columns) {
            if (string.IsNullOrEmpty(thisc.TimeCode) || _sql.GetStyleData(TableName, DatabaseDataType.ColumnTimeCode.ToString(), thisc.Name) != thisc.TimeCode) {
                l.AddIfNotExists(thisc);
            }
        }

        _sql.LoadColumns(TableName, l);
        _sql.CloseConnection();
    }

    public override bool RefreshRowData(List<RowItem> rows, bool refreshAlways) {
        if (rows == null || rows.Count == 0) { return false; }

        var l = new ListExt<RowItem>();

        foreach (var thisr in rows) {
            var cellKey = CellCollection.KeyOfCell(Column.SysRowChangeDate, thisr);

            if (refreshAlways || !Cell.ContainsKey(cellKey)) {
                l.AddIfNotExists(thisr);
            }
        }

        if (l.Count == 0) { return false; }

        _sql.OpenConnection();
        _sql.LoadRow(TableName, l);
        _sql.CloseConnection();
        return true;
    }

    public override bool Save(bool mustSave) => _sql.ConnectionOk;

    public override string UndoText(ColumnItem? column, RowItem? row) => string.Empty;

    public override void UnlockHard() { }

    public override void WaitEditable() { }

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
                    column.SetValueInternal(t, thisstyle.Value);
                }
            }
        }
    }

    protected override void AddUndo(string tableName, DatabaseDataType comand, ColumnItem? column, RowItem? row, string previousValue, string changedTo, string userName) {
        _sql.AddUndo(tableName, comand, column, row, previousValue, changedTo, UserName);
    }

    protected override void SetUserDidSomething() { }

    protected override string SpecialErrorReason(ErrorReason mode) => string.Empty;

    protected override void StoreValueToHardDisk(DatabaseDataType type, ColumnItem? column, RowItem? row, string value) {
        _sql?.CheckIn(TableName, type, value, column, row, -1, -1);
        //return base.SetValueInternal(type, value, column, row, width, height);
    }

    private static void CheckSysUndo(object state) {
        if (DateTime.UtcNow.Subtract(_timerTimeStamp).TotalSeconds < 30) { return; }

        if (_isInTimer) { return; }
        _isInTimer = true;

        var fd = DateTime.UtcNow;

        var done = new List<DatabaseAbstract>();

        foreach (var thisDB in AllFiles) {
            if (!done.Contains(thisDB)) {
                if (thisDB is DatabaseSQLLite thisDBSqlLite) {
                    var db = LoadedDatabasesWithThisSQL(thisDBSqlLite._sql);
                    done.AddRange(db);

                    var erg = thisDBSqlLite._sql.GetRowData(db, _timerTimeStamp.AddSeconds(-2), fd);

                    foreach (var thisdb in db) {
                        thisdb.RefreshRows(erg);
                    }
                }
            }
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

    private void GenerateTimer() {
        if (_timer != null) { return; }
        _timerTimeStamp = DateTime.UtcNow.AddMinutes(-5);
        _timer = new System.Threading.Timer(CheckSysUndo);
        _timer.Change(5000, 5000);
    }

    private void LoadFromSQLBack() {
        var onlyReload = false;

        OnLoading(this, new BlueBasics.EventArgs.LoadingEventArgs(onlyReload));
        IsLoading = true;
        Column.ThrowEvents = false;
        Row.ThrowEvents = false;

        #region Spalten richtig stellen

        var columnsToLoad = _sql.GetColumnNames(TableName.ToUpper());
        columnsToLoad.Remove("RK");

        #region Nicht mehr vorhandene Spalten l�schen

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
            var colum = Column.Exists(thisCol);
            if (colum == null) {
                colum = new ColumnItem(this, thisCol, Column.NextColumnKey());
                Column.Add(colum);
            }
            GetColumnAttributesColumn(colum, _sql);
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
                    SetValueInternal(t, thisstyle.Value, null, null, -1, -1);
                }
            }
        }

        #endregion

        #region  Alle ZEILEN laden

        _sql.LoadAllRows(TableName, Row);

        foreach (var thisColumn in Column) {
            thisColumn._timecode = string.Empty;
        }

        #endregion

        _sql.CloseConnection();

        Row.RemoveNullOrEmpty();
        Cell.RemoveOrphans();

        _checkedAndReloadNeed = false;
        Column.ThrowEvents = true;
        Row.ThrowEvents = true;
        IsLoading = false;
        OnLoaded(this, new BlueBasics.EventArgs.LoadedEventArgs(onlyReload));
        //RepairAfterParse();
    }

    private void RefreshRows(List<(string tablename, string comand, string columnname, string rowid)>? data) {
        if (data == null) { return; }

        var rk = new List<long>();

        foreach (var thisData in data) {
            if (TableName == thisData.tablename) {
                if (!string.IsNullOrEmpty(thisData.rowid)) {
                    rk.AddIfNotExists(LongParse(thisData.rowid));
                }

                //Enum.TryParse(thisData.comand, out DatabaseDataType t);
                //if (t != (DatabaseDataType)0) {
                //    column.SetValueInternal(t, thisstyle.Value);
                //}
            }
        }

        RefreshRowData(rk, true);
    }

    #endregion
}