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

// https://stackoverflow.com/questions/34002901/need-for-sqlconnection-and-oracleconnection-if-odbcconnection-can-be-used-in-th
#nullable enable

using BlueBasics;
using BlueBasics.Enums;
using BlueDatabase.AdditionalScriptComands;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices.ComTypes;
using static BlueBasics.Converter;
using static BlueBasics.Extensions;

namespace BlueDatabase;

//https://www.c-sharpcorner.com/article/create-a-sql-server-database-dynamically-in-C-Sharp/
//https://www.ictdemy.com/csharp/databases/introduction-to-databases-in-csharp-net
//https://docs.microsoft.com/en-us/troubleshoot/developer/visualstudio/csharp/language-compilers/create-sql-server-database-programmatically
public abstract class SQLBackAbstract {

    #region Fields

    public const string SYS_STYLE = "SYS_STYLE";
    public const string SYS_UNDO = "SYS_UNDO";
    public static List<SQLBackAbstract> ConnectedSQLBack = new();

    //public static List<SQLBackAbstract>? PossibleSQLBacks;
    protected DbConnection? _connection;

    private object getRow = new object();
    private object openclose = new object();

    #endregion

    #region Constructors

    public SQLBackAbstract() {
        //GetSQLBacks();
        ConnectedSQLBack.Add(this);
    }

    #endregion

    #region Properties

    public bool ConnectionOk => _connection != null;
    public string ConnectionString { get; protected set; }

    public string Date { get; } = "DATE";

    /// <summary>
    /// Falls die Datenbank von einer lokalen Datei geladen wurde, ist hier der Dateiname enthalten.
    /// </summary>
    public string Filename { get; protected set; } = string.Empty;

    public abstract int MaxStringLenght { get; }
    public abstract string Primary { get; }
    public abstract string VarChar255 { get; }
    public abstract string VarChar4000 { get; }

    #endregion

    #region Methods

    public static bool IsValidTableName(string tablename) {
        var t = tablename.ToUpper();

        if (t.StartsWith("SYS_")) { return false; }

        if (!t.ContainsOnlyChars(Constants.Char_AZ + Constants.Char_Numerals + "_")) { return false; }

        return true;
    }

    public static string MakeValidTableName(string tablename) {
        var tmp = tablename.RemoveChars(Constants.Char_PfadSonderZeichen); // sonst stürzt FileNameWithoutSuffix ab
        tmp = tmp.FileNameWithoutSuffix().ToLower().Replace(" ", "_").Replace("-", "_");
        tmp = tmp.StarkeVereinfachung("_").ToUpper();

        while (tmp.Contains("__")) {
            tmp = tmp.Replace("__", "_");
        }

        return tmp;
    }

    /// <summary>
    /// Erstellt eine Spalte nur bei Bedarf.
    /// Aber der Key wird aktualisiert.
    /// </summary>
    /// <param name="tablename"></param>
    /// <param name="columnName"></param>
    /// <param name="columnKey"></param>
    public void AddColumnToMain(string tablename, string columnName, long columnKey) {
        columnName = columnName.ToUpper();

        var colMain = GetColumnNames(tablename.ToUpper());
        if (colMain == null) { Develop.DebugPrint(FehlerArt.Fehler, "Spaltenfehler"); return; }
        if (!colMain.Contains(columnName)) { AddColumn(tablename.ToUpper(), columnName, VarChar4000, true); }

        SetStyleData(tablename, DatabaseDataType.ColumnKey, columnName.ToUpper(), columnKey.ToString());
    }

    public bool AddUndo(string tablename, DatabaseDataType comand, long? columnKey, long? rowKey, string previousValue, string changedTo, string userName) {
        if (!OpenConnection()) { return false; }

        var ck = columnKey != null && columnKey > -1 ? columnKey.ToString() : string.Empty;
        var rk = rowKey != null && rowKey > -1 ? rowKey.ToString() : string.Empty;

        var cmdString = "INSERT INTO " + SYS_UNDO +
            " (TABLENAME, COMAND, COLUMNKEY, ROWKEY, PREVIOUSVALUE, CHANGEDTO, USERNAME, TIMECODEUTC) VALUES (" +
             DBVAL(tablename.ToUpper()) + "," +
             DBVAL(comand.ToString()) + "," +
             DBVAL(ck) + "," +
             DBVAL(rk) + "," +
             DBVAL(previousValue) + "," +
             DBVAL(changedTo) + "," +
             DBVAL(userName) + "," +
             //DBVAL(dt.ToString(Constants.Format_Date)) + "," +
             DBVAL(DateTime.UtcNow) + ")";

        using var comm = _connection.CreateCommand();
        comm.CommandText = cmdString;

        return ExecuteCommand(comm);
    }

    public bool CloseConnection() {
        lock (openclose) {
            if (_connection.State == ConnectionState.Open) { _connection.Close(); }

            return _connection.State == ConnectionState.Closed;
        }
    }

    /// <summary>
    /// Provider ist immer NULL!
    /// </summary>
    /// <param name="tablename"></param>
    /// <returns></returns>
    public ConnectionInfo ConnectionData(string tablename, bool lite) {
        var z = lite ? " LITE" : string.Empty;
        return new ConnectionInfo(tablename, null, ConnectionString + z, string.Empty);
    }

    /// <summary>
    /// Gibt die Spaltenname in Grosschreibung zurück
    /// </summary>
    /// <returns></returns>
    public abstract List<string>? GetColumnNames(string tablename);

    public string? GetStyleData(string tablename, string type, string columnName) {
        try {
            if (!OpenConnection()) { return null; }

            using var q = _connection.CreateCommand();

            if (string.IsNullOrEmpty(columnName)) { columnName = "~Database~"; }

            q.CommandText = @"select VALUE, PART from " + SYS_STYLE + " " +
                            "where TABLENAME = " + DBVAL(tablename.ToUpper()) + " " +
                            "and TYPE = " + DBVAL(type) + " " +
                            "and COLUMNNAME = " + DBVAL(columnName.ToUpper()) + " " +
                            "ORDER BY PART ASC";

            if (!OpenConnection()) { return null; }
            using var reader = q.ExecuteReader();
            var value = string.Empty;
            while (reader.Read()) {
                value += reader[0].ToString();
            }

            CloseConnection();    // Nix vorhanden!
            return value;
        } catch {
            return GetStyleData(tablename, type, columnName);
        }
    }

    /// <summary>
    /// Wird kein Spaltenname angegeben, werden die Eigenschaften der Datenbank zurück gegeben.
    /// Achtung, die Connection wird nicht geschlossen!
    /// </summary>
    /// <param name="columnName"></param>
    /// <returns></returns>
    public Dictionary<string, string> GetStyleDataAll(string tablename, string columnName) {
        var l = new Dictionary<string, string>();

        if (!OpenConnection()) { return l; }

        using var q = _connection.CreateCommand();

        q.CommandText = @"select TYPE, PART, VALUE from " + SYS_STYLE + " " +
                        "where TABLENAME = " + DBVAL(tablename.ToUpper()) + " " +
                        "and COLUMNNAME = " + DBVAL(columnName.ToUpper()) + " " +
                        "ORDER BY PART ASC";

        using var reader = q.ExecuteReader();

        while (reader.Read()) {
            var key = reader[0].ToString();
            var value = reader[2].ToString();

            if (reader[1].ToString() != "001") {
                l[key] = l[key] + value;
            } else {
                l.Add(key, value);
            }
        }

        //CloseConnection();    // Nix vorhanden!
        return l;
    }

    //[Obsolete]
    //public async void LoadAllCells(string tablename, RowCollection row) {
    //    if (!OpenConnection()) { return; }

    //    //DataTable dataTable = new DataTable();
    //    //using var cmd = _connection.CreateCommand();
    //    //cmd.CommandText =@"select * from " + tablename.ToUpper();
    //    //DbDataAdapter da = DbProviderFactories.GetFactory(_connection).CreateDataAdapter();
    //    //da.SelectCommand = cmd;
    //    //da.Fill(dataTable);
    //    //da.Dispose();

    //    var allcols = GetColumnNames(tablename);
    //    allcols.Remove("RK");

    //    var com = "SELECT RK, ";

    //    foreach (var thiscolumn in row.Database.Column) {
    //        if (!allcols.Contains(thiscolumn.Name.ToUpper())) {
    //            Develop.DebugPrint(FehlerArt.Fehler, "Spalte nicht auf dem Server vorhanden: " + thiscolumn.Name);
    //        }
    //        allcols.Remove(thiscolumn.Name.ToUpper());

    //        com = com + thiscolumn.Name.ToUpper() + ", ";
    //    }

    //    com = com.TrimEnd(", ");

    //    com = com + " FROM " + tablename.ToUpper();

    //    if (allcols.Count > 0) {
    //        Develop.DebugPrint(FehlerArt.Fehler, "Zusätzliche Spalten dem Server vorhanden: " + allcols.JoinWith(", "));
    //    }

    //    OpenConnection();

    //    using var command = _connection.CreateCommand();
    //    command.CommandText = com; //@"select * from " + tablename.ToUpper();

    //    using var reader = await command.ExecuteReaderAsync();

    //    row.Clear();
    //    var ti = DateTime.Now;

    //    while (await reader.ReadAsync()) {
    //        var rk = LongParse(reader[0].ToString());
    //        var r = row.GenerateAndAdd(rk, string.Empty, false, false);

    //        for (var z = 1; z < reader.FieldCount; z++) {
    //            row.Database.Cell.SetValueInternal(row.Database.Column[z - 1].Key, r, reader[z].ToString(), -1, -1);
    //        }
    //    }

    //    Develop.DebugPrint(FehlerArt.Info, "Datenbank Ladezeit: " + row.Database.TableName + " - " + DateTime.Now.Subtract(ti).TotalSeconds.ToString() + " Sekunden");

    //    CloseConnection();
    //}

    public async void LoadAllRowKeys(string tablename, RowCollection row) {
        try {
            if (!OpenConnection()) { return; }

            var com = "SELECT RK ";
            com = com + " FROM " + tablename.ToUpper();

            OpenConnection();

            using var command = _connection.CreateCommand();
            command.CommandText = com;

            using var reader = await command.ExecuteReaderAsync();

            row.Clear();

            while (await reader.ReadAsync()) {
                var rk = LongParse(reader[0].ToString());
                var r = row.GenerateAndAdd(rk, string.Empty, false, false);
            }

            CloseConnection();
        } catch {
            LoadAllRowKeys(tablename, row);
        }
    }

    public void LoadRow(string tablename, List<RowItem> row) {
        try {
            CloseConnection(); // Um ORA-01000: MAXIMALE ANZAHL OFFENER CURSOR ÜBERSCHRITTEN zu vermeiden
            if (row == null || row.Count == 0 || row[0] is null || row[0].Database is null) { return; }
            if (!OpenConnection()) { return; }

            if (!row[0].Database.IsLoading) { Develop.DebugPrint("Loading falsch"); }

            lock (getRow) {
                if (!OpenConnection()) { return; }

                var com = "SELECT RK, ";

                foreach (var thiscolumn in row[0].Database.Column) {
                    com = com + thiscolumn.Name.ToUpper() + ", ";
                }

                com = com.TrimEnd(", ");

                com = com + " FROM " + tablename.ToUpper() + " WHERE ";

                foreach (var thisr in row) {
                    com = com + "RK = " + DBVAL(thisr.Key) + " OR ";
                }

                com = com.TrimEnd(" OR ");

                OpenConnection();

                using var command = _connection.CreateCommand();
                command.CommandText = com;

                using var reader = command.ExecuteReader();

                while (reader.Read()) {
                    var rk = LongParse(reader[0].ToString());
                    var r = row[0].Database.Row.SearchByKey(rk);
                    if (r == null) { r = row[0].Database.Row.GenerateAndAdd(rk, string.Empty, false, false); }

                    for (var z = 1; z < reader.FieldCount; z++) {
                        r.Database.Cell.SetValueInternal(r.Database.Column[z - 1].Key, r.Key, reader[z].ToString(), -1, -1);
                    }

                    r.RowInCache = true;
                    //if (!r.RowInChache()) {
                    //    Develop.DebugPrint(FehlerArt.Warnung, "Zeile ohne Zeitstempel, repariert! + " + com);
                    //    r.CellSet(r.Database.Column.SysRowChangeDate, DateTime.UtcNow.ToString(Constants.Format_Date5));
                    //}
                }
                CloseConnection();
            }
        } catch {
            CloseConnection();
            LoadRow(tablename, row);
        }
    }

    public bool OpenConnection() {
        lock (openclose) {
            if (_connection == null) { return false; }

            if (_connection.State == ConnectionState.Closed) {
                _connection.Open();
            }

            return _connection.State == ConnectionState.Open;
        }
    }

    public abstract SQLBackAbstract OtherTable(string tablename);

    public void RenameColumn(string tablename, string oldname, string newname) {
        //https://www.1keydata.com/sql/alter-table-rename-column.html
        var cmdString = @"ALTER TABLE " + tablename + " RENAME COLUMN " + oldname + " TO " + newname;
        ExecuteCommand(cmdString);

        //if (isVal is null) {
        //    cmdString = "INSERT INTO " + SYS_STYLE + " (TABLENAME, TYPE, COLUMNNAME, VALUE)  VALUES (" + DBVAL( tablename.ToUpper() ) + ", " + DBVAL( type ) + ", " + DBVAL( columnName.ToUpper() ) + ", " + DBVAL( newValue ) + ")";
        //} else if (isVal != newValue) {
        cmdString = "UPDATE " + SYS_STYLE + " SET COLUMNNAME = " + DBVAL(newname) + " WHERE TABLENAME = " + DBVAL(tablename.ToUpper()) + " AND COLUMNNAME = " + DBVAL(oldname.ToUpper());
        //} else {
        //    return true;
        //}

        ExecuteCommand(cmdString);
    }

    public void RepairAll(string tablename) {
        Develop.StartService();

        if (!IsValidTableName(tablename)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Tabellename ungültig: " + tablename);
            return;
        }

        var x = AllTables();

        #region Main

        if (!string.IsNullOrEmpty(tablename)) {
            if (!x.Contains(tablename.ToUpper())) { CreateTable(tablename.ToUpper(), new List<string>() { "RK" }); }
        }

        #endregion

        #region Style

        if (!x.Contains(SYS_STYLE)) { CreateTable(SYS_STYLE, new List<string>() { "TABLENAME", "COLUMNNAME", "TYPE", "PART" }); }

        var colStyle = GetColumnNames(SYS_STYLE);
        if (colStyle == null) { Develop.DebugPrint(FehlerArt.Fehler, "Spaltenfehler"); return; }

        if (!colStyle.Contains("VALUE")) { AddColumn(SYS_STYLE, "VALUE", VarChar4000, true); }

        #endregion

        #region  Undo

        if (!x.Contains(SYS_UNDO)) { CreateTable(SYS_UNDO); }

        var colUndo = GetColumnNames(SYS_UNDO);
        if (colUndo == null) { Develop.DebugPrint(FehlerArt.Fehler, "Spaltenfehler"); return; }
        if (!colUndo.Contains("TABLENAME")) { AddColumn(SYS_UNDO, "TABLENAME", false); }
        if (!colUndo.Contains("COMAND")) { AddColumn(SYS_UNDO, "COMAND", false); }
        //if (!colUndo.Contains("COLUMNNAME")) { AddColumn(SYS_UNDO, "COLUMNNAME", false); }
        if (!colUndo.Contains("COLUMNKEY")) { AddColumn(SYS_UNDO, "COLUMNKEY", true); }
        if (!colUndo.Contains("ROWKEY")) { AddColumn(SYS_UNDO, "ROWKEY", true); }
        if (!colUndo.Contains("PREVIOUSVALUE")) { AddColumn(SYS_UNDO, "PREVIOUSVALUE", VarChar4000, true); }
        if (!colUndo.Contains("CHANGEDTO")) { AddColumn(SYS_UNDO, "CHANGEDTO", VarChar4000, true); }
        if (!colUndo.Contains("USERNAME")) { AddColumn(SYS_UNDO, "USERNAME", false); }
        //if (!colUndo.Contains("DATETIMEUTC")) { AddColumn(SYS_UNDO, "DATETIMEUTC", false); }
        if (!colUndo.Contains("TIMECODEUTC")) { AddColumn(SYS_UNDO, "TIMECODEUTC", Date, false); }

        #endregion

        CloseConnection();
    }

    /// <summary>
    /// Gibt TRUE zurück, wenn alles ok ist.
    /// Entweder der Wert gesetzt wurde, der Wert aktuell ist oder der Wert unwichtig ist.
    /// </summary>
    /// <param name="tablename"></param>
    /// <param name="type"></param>
    /// <param name="columnName"></param>
    /// <param name="newValue"></param>
    /// <returns></returns>
    public bool SetStyleData(string tablename, DatabaseDataType type, string columnName, string newValue) {
        if (type.Nameless()) { return true; }
        //command
        //if (type == DatabaseDataType.AddColumnKeyInfo) { return true; } // enthält zwar den Key, aber Wertlos, wenn der Spaltenname noch nicht bekannt ist...
        //if (type == DatabaseDataType.AutoExport) { return true; }
        if (type == DatabaseDataType.UndoInOne) { return true; }
        var c = 0;
        var ok = true;
        var utf8 = newValue;
        var ty = type.ToString();

        columnName = columnName.ToUpper();
        if (string.IsNullOrEmpty(columnName)) { columnName = "~Database~"; }

        var isVal = GetStyleData(tablename, ty, columnName);
        if (isVal == null) { return false; }
        if (isVal == newValue) { return true; }

        ExecuteCommand("DELETE FROM " + SYS_STYLE +
                       " WHERE TABLENAME = " + DBVAL(tablename.ToUpper()) +
                       " AND COLUMNNAME = " + DBVAL(columnName.ToUpper()) +
                       " AND TYPE = " + DBVAL(type.ToString()));

        do {
            c++;
            var tmp = utf8.CutToUTF8Length(MaxStringLenght);

            var ok2 = SetStyleData(tablename, ty, columnName, tmp, c);
            if (!ok2) { ok = false; }

            if (tmp == utf8) { return ok; }

            utf8 = utf8.Substring(tmp.Length);
        } while (true);
    }

    /// <summary>
    /// Führt eine eine Änderung im BackEnd aus
    /// </summary>
    /// <param name="tablename"></param>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public bool SetValueInternal(string tablename, DatabaseDataType type, string value, string? columname, long? columnkey, long? rowkey, int width, int height) {

        #region Ignorieren

        switch (type) {
            case DatabaseDataType.Formatkennung:
            case DatabaseDataType.Werbung:
            case DatabaseDataType.CryptionState:
            case DatabaseDataType.CryptionTest:
                return true;

            case DatabaseDataType.SaveContent:
                return true;

            case DatabaseDataType.EOF:
                return true;

                //default:
                //    break;
        }

        #endregion

        if (!OpenConnection()) { return false; }

        #region Datenbank Eigenschaften

        if (type.IsDatabaseTag()) {
            return SetStyleData(tablename, type, string.Empty, value);
        }

        #endregion

        #region Spalten Eigenschaften

        if (type.IsColumnTag()) {
            return SetStyleData(tablename, type, columname.ToUpper(), value);
        }

        #endregion

        #region Zellen-Wert

        if (type.IsCellValue()) {
            SetCellValue(tablename, columname, (long)rowkey, value);

            CloseConnection();
            return true;
        }

        #endregion

        #region Befehle

        if (type.IsCommand()) {
            switch (type) {
                case DatabaseDataType.Comand_AddColumn:
                    AddColumnToMain(tablename, ColumnItem.TmpNewDummy, (long)columnkey);
                    return true;

                case DatabaseDataType.Comand_RemoveColumn:
                    RemoveColumn(tablename, columname);
                    return true;

                case DatabaseDataType.Comand_RemoveRow:
                    RemoveRow(tablename, LongParse(value));
                    return true;

                case DatabaseDataType.Comand_AddRow:
                    AddRow(tablename, LongParse(value));
                    return true;

                default:
                    Develop.DebugPrint(FehlerArt.Fehler, type.ToString() + " nicht definiert!");
                    return false;
            }
        }

        #endregion

        CloseConnection();
        return false;
    }

    /// <summary>
    /// Gibt alle verfügbaren Tabellen - außer die Systemtabellen - zurück
    /// </summary>
    /// <returns></returns>
    public List<string> Tables() {
        var l = AllTables();

        l.Remove(SYS_STYLE);
        l.Remove(SYS_UNDO);
        return l;
    }

    internal static string MakeValidColumnName(string columnname) {
        return columnname.ToUpper().Replace(" ", "_").ReduceToChars(Constants.AllowedCharsVariableName);
    }

    internal List<(string tablename, string comand, string columnkey, string rowkey)>? GetLastChanges(List<DatabaseSQLLite> db, DateTime fromDate, DateTime toDate) {
        if (!OpenConnection()) { return null; }

        try {
            using var q = _connection.CreateCommand();

            q.CommandText = @"select TABLENAME, COMAND, COLUMNKEY, ROWKEY from " + SYS_UNDO + " ";

            // nur bestimmte Tabellen
            q.CommandText += "WHERE (";
            foreach (var thisdb in db) {
                q.CommandText += "TABLENAME=" + DBVAL(thisdb.TableName.ToUpper()) + " OR ";
            }
            q.CommandText = q.CommandText.TrimEnd(" OR ") + ") AND ";

            // Zeit einbeziehen
            q.CommandText += "(TIMECODEUTC BETWEEN " + DBVAL(fromDate)
                             + " AND " + DBVAL(toDate) + ")";

            // Sortierung nach Tabellen
            q.CommandText += " ORDER BY TIMECODEUTC ASC";

            var fb = new List<(string tablename, string comand, string columnname, string rowid)>();

            if (!OpenConnection()) { return null; }
            using var reader = q.ExecuteReader();
            var value = string.Empty;
            while (reader.Read()) {
                fb.Add((reader[0].ToString(), reader[1].ToString(), reader[2].ToString(), reader[3].ToString()));
            }

            CloseConnection();
            return fb;
        } catch { return null; }
    }

    internal string? GetLastColumnName(string tablename, long key) {
        if (!OpenConnection()) { return null; }

        try {
            using var q = _connection.CreateCommand();

            q.CommandText = @"select CHANGEDTO from " + SYS_UNDO + " ";
            q.CommandText += "WHERE TABLENAME=" + DBVAL(tablename.ToUpper()) + " AND ";
            q.CommandText += "COMAND=" + DBVAL(DatabaseDataType.ColumnName.ToString()) + " AND ";
            q.CommandText += "COLUMNKEY=" + DBVAL(key);
            q.CommandText += " ORDER BY TIMECODEUTC DESC";

            if (!OpenConnection()) { return null; }
            using var reader = q.ExecuteReader();
            var value = string.Empty;
            while (reader.Read()) {
                value = reader[0].ToString();
                break;
            }

            CloseConnection();
            return value;
        } catch { return GetLastColumnName(tablename, key); }
    }

    internal SQLBackAbstract? HandleMe(ConnectionInfo ci) {
        if (ci is null) { return null; }

        //var s = connectionID.SplitBy("|");
        //if (s.Count() != 2) { return null; }

        foreach (var thisK in ConnectedSQLBack) {
            if (thisK.ConnectionOk && thisK.ConnectionString == ci.DatabaseID) {
                return thisK.OtherTable(ci.TableName);
            }

            if (thisK.ConnectionOk && thisK.ConnectionString + " LITE" == ci.DatabaseID) {
                return thisK.OtherTable(ci.TableName);
            }
        }

        return null;
    }

    internal void LoadColumns(string tablename, ListExt<ColumnItem> columns) {
        try {
            if (columns == null || columns.Count == 0 || columns[0] is null || columns[0].Database is null) { return; }
            if (columns.Count == 1 && columns[0].Loaded) { return; }

            if (!columns[0].Database.IsLoading) { Develop.DebugPrint("Loading falsch"); }

            var com = "SELECT RK, ";

            foreach (var thiscolumn in columns) {
                com = com + thiscolumn.Name.ToUpper() + ", ";
            }
            com = com.TrimEnd(", ");
            com = com + " FROM " + tablename.ToUpper();

            if (!OpenConnection()) { return; }

            using var command = _connection.CreateCommand();
            command.CommandText = com;

            using var reader = command.ExecuteReader();

            while (reader.Read()) {
                var rk = LongParse(reader[0].ToString());
                var row = columns[0].Database.Row.SearchByKey(rk);

                if (row == null) {
                    row = columns[0].Database.Row.GenerateAndAdd(rk, string.Empty, false, false);
                }

                for (var z = 1; z < reader.FieldCount; z++) {
                    row.Database.Cell.SetValueInternal(columns[z - 1].Key, row.Key, reader[z].ToString(), -1, -1);
                }
            }

            foreach (var thiscolumn in columns) {
                //var val = GetStyleData(tablename, DatabaseDataType.ColumnTimeCode.ToString(), thiscolumn.Name);

                thiscolumn.Loaded = true;

                //if (string.IsNullOrEmpty(val)) {
                //    Develop.DebugPrint(FehlerArt.Warnung, "Spalte ohne Zeitstempel, repariert! + " + com);
                //    thiscolumn.TimeCode = DateTime.UtcNow.ToString(Constants.Format_Date);
                //    //val =
                //    //SetStyleData(tablename, DatabaseDataType.ColumnTimeCode, thiscolumn.Name, val);
                //}
            }

            //Develop.DebugPrint(FehlerArt.Info, "Datenbank Ladezeit: " + row.Database.TableName + " - " + DateTime.Now.Subtract(ti).TotalSeconds.ToString() + " Sekunden");

            CloseConnection();
        } catch {
            CloseConnection();
            LoadColumns(tablename, columns);
        }
    }

    /// <summary>
    /// Gibt alle verfügbaren Tabellen - einschließlich der Systemtabellen - zurück
    /// </summary>
    /// <returns></returns>
    protected abstract List<string> AllTables();

    protected abstract bool CreateTable(string tablename);

    protected abstract bool CreateTable(string tablename, List<string> keycolumns);

    protected bool ExecuteCommand(string commandtext) {
        if (!OpenConnection()) { return false; }

        using var command = _connection.CreateCommand();
        command.CommandText = commandtext;

        return ExecuteCommand(command);
    }

    private void AddColumn(string tablename, string column, bool nullable) => AddColumn(tablename, column, VarChar255, nullable);

    private void AddColumn(string tablename, string column, string type, bool nullable) {
        if (string.IsNullOrEmpty(column)) {
            Develop.DebugPrint(FehlerArt.Warnung, "Spalte ohne Namen!");
            return;
        }

        var n = " NOT NULL";
        if (nullable) { n = string.Empty; }

        ExecuteCommand("alter table " + tablename.ToUpper() + " add " + column + " " + type + " default ''" + n);
    }

    private void AddRow(string tablename, long key) {
        ExecuteCommand("INSERT INTO " + tablename.ToUpper() + " (RK) VALUES (" + DBVAL(key) + ")");
        //ExecuteCommand("DELETE FROM  " + tablename.ToUpper() + " WHERE RK = " + DBVAL(key.ToString()));
        //ExecuteCommand("DELETE FROM " + SYS_STYLE + " WHERE TABLENAME = " + DBVAL( tablename.ToUpper() ) + " AND COLUMNNAME = " + DBVAL( column.ToUpper() ));
    }

    private string DBVAL(long original) {
        return DBVAL(original.ToString());
    }

    private string DBVAL(DateTime date) {
        return "to_timestamp(" +
               DBVAL(date.ToString(Constants.Format_Date9)) + ", " +
               DBVAL(Constants.Format_Date9.ToUpper().Replace(":MM:", ":MI:").Replace("HH:", "HH24:").Replace(".FFF", ".FF3")) +
                    ")";
    }

    private string DBVAL(string original) {
        original = original.CutToUTF8Length(MaxStringLenght);
        original = original.Replace("'", "''");
        return "'" + original + "'";
    }

    private bool ExecuteCommand(DbCommand command) {
        if (!OpenConnection()) { return false; }

        try {
            command.ExecuteNonQuery();
            return true;
        } catch (Exception ex) {
            Develop.DebugPrint(ex);
            return false;
        } finally {
            CloseConnection();
        }
    }

    private string? GetCellValue(string tablename, string columnname, long rowkey) {
        if (!OpenConnection()) { return null; }

        using var q = _connection.CreateCommand();

        q.CommandText = @"select " + columnname.ToUpper() + " from " + tablename.ToUpper() + " " +
                        "where RK = " + DBVAL(rowkey);

        ////command.AddParameterWithValue("" + DBVAL(columnName.ToUpper()) , column.Name.ToUpper());
        //command.AddParameterWithValue("" + DBVAL(row.Key)", row.Key.ToString());

        using var reader = q.ExecuteReader();
        if (reader.Read()) {
            // you may want to check if value is NULL: reader.IsDBNull(0)
            var value = reader[0].ToString();

            if (reader.Read()) {
                // Doppelter Wert?!?Ersten Wert zurückgeben, um unendliche erweiterungen zu erhindern
                Develop.DebugPrint(columnname.ToString() + " doppelt in " + tablename.ToUpper() + " vorhanden!");
            }

            CloseConnection();
            return value;
        }

        CloseConnection();    // Nix vorhanden!
        return null;
    }

    private void RemoveColumn(string tablename, string column) {
        ExecuteCommand("alter table " + tablename.ToUpper() + " drop column " + column.ToUpper());
        ExecuteCommand("DELETE FROM " + SYS_STYLE + " WHERE TABLENAME = " + DBVAL(tablename.ToUpper()) + " AND COLUMNNAME = " + DBVAL(column.ToUpper()));
    }

    private void RemoveRow(string tablename, long key) {
        ExecuteCommand("DELETE FROM  " + tablename.ToUpper() + " WHERE RK = " + DBVAL(key.ToString()));
        //ExecuteCommand("DELETE FROM " + SYS_STYLE + " WHERE TABLENAME = " + DBVAL( tablename.ToUpper() ) + " AND COLUMNNAME = " + DBVAL( column.ToUpper() ));
    }

    /// <summary>
    /// Gibt TRUE zurück, wenn alles ok ist.
    /// Entweder der Wert gesetzt wurde, der Wert aktuell ist oder der Wert unwichtig ist.
    /// </summary>
    /// <param name="tablename"></param>
    /// <param name="type"></param>
    /// <param name="columnName"></param>
    /// <param name="newValue"></param>
    /// <returns></returns>
    private bool SetCellValue(string tablename, string columnname, long rowkey, string newValue) {
        var isVal = GetCellValue(tablename, columnname, rowkey);

        string cmdString;

        if (isVal is null) {
            cmdString = "INSERT INTO " + tablename.ToUpper() + " (RK, " + columnname.ToUpper() + " ) VALUES (" + DBVAL(rowkey) + ", " + DBVAL(newValue) + " )";
        } else if (isVal != newValue) {
            cmdString = "UPDATE " + tablename.ToUpper() + " SET " + columnname.ToUpper() + " = " + DBVAL(newValue) + " WHERE RK = " + DBVAL(rowkey);
        } else {
            return true;
        }

        if (!OpenConnection()) { return false; }

        return ExecuteCommand(cmdString);
    }

    /// <summary>
    /// Gibt TRUE zurück, wenn alles ok ist.
    /// Entweder der Wert gesetzt wurde oder der Wert aktuell ist.
    /// </summary>
    /// <param name="tablename"></param>
    /// <param name="type"></param>
    /// <param name="columnName"></param>
    /// <param name="newValue"></param>
    /// <returns></returns>
    private bool SetStyleData(string tablename, string type, string columnName, string newValue, int part) {
        string cmdString;

        //if (isVal is null) {
        cmdString = "INSERT INTO " + SYS_STYLE + " (TABLENAME, TYPE, COLUMNNAME, VALUE, PART)  VALUES (" + DBVAL(tablename.ToUpper()) + ", " + DBVAL(type) + ", " + DBVAL(columnName.ToUpper()) + ", " + DBVAL(newValue) + ", " + DBVAL(part.ToString(Constants.Format_Integer3)) + ")";
        //} else if (isVal != newValue) {
        //    cmdString = "UPDATE " + SYS_STYLE + " SET VALUE = " + DBVAL( newValue ) + " WHERE TABLENAME = " + DBVAL( tablename.ToUpper() ) + " AND TYPE = " + DBVAL( type ) + " AND COLUMNNAME = " + DBVAL( columnName.ToUpper() );
        //} else {
        //    return true;
        //}

        if (!OpenConnection()) { return false; }

        return ExecuteCommand(cmdString);
    }

    #endregion

    /// <summary>
    /// Gibt TRUE zurück, wenn alles ok ist.
    /// Entweder der Wert gesetzt wurde, der Wert aktuell ist oder der Wert unwichtig ist.
    /// </summary>
    /// <param name="tablename"></param>
    /// <param name="type"></param>
    /// <param name="columnName"></param>
    /// <param name="newValue"></param>
    /// <returns></returns>
}