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
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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

    public bool AddUndo(string tablename, DatabaseDataType comand, ColumnItem? column, RowItem? row, string previousValue, string changedTo, string userName) {
        if (!OpenConnection()) { return false; }

        var n = (column?.Name) ?? "~Database~";
        var rk = (row?.Key.ToString()) ?? string.Empty;

        var cmdString = "INSERT INTO " + SYS_UNDO +
            " (TABLENAME, COMAND, COLUMNNAME, ROWKEY, PREVIOUSVALUE, CHANGEDTO, USERNAME, DATETIMEUTC) VALUES (" +
             DBVAL(tablename.ToUpper()) + "," +
             DBVAL(comand.ToString()) + "," +
             DBVAL(n) + "," +
             DBVAL(rk) + "," +
             DBVAL(previousValue) + "," +
             DBVAL(changedTo) + "," +
             DBVAL(userName) + "," +
             DBVAL(DateTime.UtcNow.ToString(Constants.Format_Date)) + ")";

        using var comm = _connection.CreateCommand();
        comm.CommandText = cmdString;

        return ExecuteCommand(comm);
    }

    public bool CheckIn(string tablename, DatabaseDataType type, string value, ColumnItem? column, RowItem? row, int width, int height) {
        if (!OpenConnection()) { return false; }

        #region Ignorieren

        switch (type) {
            case DatabaseDataType.Formatkennung:
            case DatabaseDataType.Werbung:
            case DatabaseDataType.CryptionState:
            case DatabaseDataType.CryptionTest:
                break;

            case DatabaseDataType.SaveContent:
                break;

            case DatabaseDataType.EOF:
                break;

            default:
                break;
        }

        #endregion

        #region Datenbank Eigenschaften

        if (type.IsDatabaseTag()) {
            return SetStyleData(tablename, type, string.Empty, value);
        }

        #endregion

        #region Spalten Eigenschaften

        if (type.IsColumnTag()) {
            if (type == DatabaseDataType.ColumnName) { RenameColumn(tablename, column.Name.ToUpper(), value.ToUpper()); }
            return SetStyleData(tablename, type, column.Name.ToUpper(), value);
        }

        #endregion

        #region Zellen-Wert

        if (type.IsCellValue()) {
            SetCellValue(tablename, column, row, value);

            CloseConnection();
            return true;
        }

        #endregion

        #region Befehle

        if (type.IsCommand()) {
            switch (type) {
                //case DatabaseDataType.AddColumnKeyInfo:
                //    // Ignoreieren, macht AddColumnNameInfo
                //    return true;

                case DatabaseDataType.Comand_ColumnAdded:
                    AddColumnToMain(tablename, column.Name, column.Key);
                    return true;

                case DatabaseDataType.Comand_RemovingColumn:
                    RemoveColumn(tablename, column.Name);
                    return true;

                case DatabaseDataType.Comand_RemovingRow:
                    RemoveRow(tablename, row.Key);
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

    public bool CloseConnection() {
        if (_connection.State == ConnectionState.Open) { _connection.Close(); }

        return _connection.State == ConnectionState.Closed;
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
        if (!OpenConnection()) { return null; }

        using var q = _connection.CreateCommand();

        if (string.IsNullOrEmpty(columnName)) { columnName = "~Database~"; }

        q.CommandText = @"select VALUE, PART from " + SYS_STYLE + " " +
                        "where TABLENAME = " + DBVAL(tablename.ToUpper()) + " " +
                        "and TYPE = " + DBVAL(type) + " " +
                        "and COLUMNNAME = " + DBVAL(columnName.ToUpper()) + " "+
                        "ORDER BY PART ASC";

        using var reader = q.ExecuteReader();
        var value = string.Empty;
        while (reader.Read()) {
            value += reader[0].ToString();
        }

        //if (reader.Read()) {
        //    // you may want to check if value is NULL: reader.IsDBNull(0)
        //    var value = reader[0].ToString();

        //    //if (reader.Read()) {
        //    //    // Doppelter Wert?!?Ersten Wert zurückgeben, um unendliche erweiterungen zu erhindern
        //    //    Develop.DebugPrint(tablename.ToUpper() + " " + type + " " + columnName + " doppelt in Style vorhanden!");
        //    //}

        //    CloseConnection();
        //    return value;
        //}

        CloseConnection();    // Nix vorhanden!
        return value;
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
                l[key] =  l[key]+value;
            } else {
                l.Add(key, value);
            }
        }

        //CloseConnection();    // Nix vorhanden!
        return l;
    }


    public async void LoadAllRows(string tablename, RowCollection row) {
        if (!OpenConnection()) { return; }



        //var allcols = GetColumnNames(tablename);
        //allcols.Remove("RK");

        var com = "SELECT RK ";

        //foreach (var thiscolumn in row.Database.Column) {
        //    if (!allcols.Contains(thiscolumn.Name.ToUpper())) {
        //        Develop.DebugPrint(FehlerArt.Fehler, "Spalte nicht auf dem Server vorhanden: " + thiscolumn.Name);
        //    }
        //    allcols.Remove(thiscolumn.Name.ToUpper());

        //    com = com + thiscolumn.Name.ToUpper() + ", ";
        //}

        //com = com.TrimEnd(", ");

        com = com + " FROM " + tablename.ToUpper();

        //if (allcols.Count>0) {
        //    Develop.DebugPrint(FehlerArt.Fehler, "Zusätzliche Spalten dem Server vorhanden: " + allcols.JoinWith(", "));
        //}

        OpenConnection();

        using var command = _connection.CreateCommand();
        command.CommandText = com; //@"select * from " + tablename.ToUpper();

        using var reader = await command.ExecuteReaderAsync();



        row.Clear();
        //var ti = DateTime.Now;

        while (await reader.ReadAsync()) {
            var rk = LongParse(reader[0].ToString());
            var r = new RowItem(row.Database, rk);
            row.Add(r);

            //for (var z = 1; z < reader.FieldCount; z++) {
            //    row.Database.Cell.SetValueInternal(row.Database.Column[z - 1], r, reader[z].ToString(), -1, -1);
            //}
        }

        //Develop.DebugPrint(FehlerArt.Info, "Datenbank Ladezeit: " + row.Database.TableName + " - " + DateTime.Now.Subtract(ti).TotalSeconds.ToString() + " Sekunden");

        CloseConnection();
    }


    public async void LoadAllCells(string tablename, RowCollection row) {
        if (!OpenConnection()) { return; }


        //DataTable dataTable = new DataTable();
        //using var cmd = _connection.CreateCommand();
        //cmd.CommandText =@"select * from " + tablename.ToUpper();
        //DbDataAdapter da = DbProviderFactories.GetFactory(_connection).CreateDataAdapter();
        //da.SelectCommand = cmd;
        //da.Fill(dataTable);
        //da.Dispose();



        var allcols = GetColumnNames(tablename);
        allcols.Remove("RK");

        var com = "SELECT RK, ";

        foreach (var thiscolumn in row.Database.Column) {
            if (!allcols.Contains(thiscolumn.Name.ToUpper())) {
                Develop.DebugPrint(FehlerArt.Fehler, "Spalte nicht auf dem Server vorhanden: " + thiscolumn.Name);
            }
            allcols.Remove(thiscolumn.Name.ToUpper());

            com = com + thiscolumn.Name.ToUpper() + ", ";
        }

        com = com.TrimEnd(", ");

        com = com + " FROM " + tablename.ToUpper();

        if (allcols.Count>0) {
            Develop.DebugPrint(FehlerArt.Fehler, "Zusätzliche Spalten dem Server vorhanden: " + allcols.JoinWith(", "));
        }

        OpenConnection();

        using var command = _connection.CreateCommand();
        command.CommandText = com; //@"select * from " + tablename.ToUpper();

        using var reader = await command.ExecuteReaderAsync();



        row.Clear();
        var ti = DateTime.Now;

        while (await reader.ReadAsync()) {
            var rk = LongParse(reader[0].ToString());
            var r = new RowItem(row.Database, rk);
            row.Add(r);

            for (var z = 1; z < reader.FieldCount; z++) {
                row.Database.Cell.SetValueInternal(row.Database.Column[z - 1], r, reader[z].ToString(), -1, -1);
            }
        }

        Develop.DebugPrint(FehlerArt.Info, "Datenbank Ladezeit: " + row.Database.TableName + " - " + DateTime.Now.Subtract(ti).TotalSeconds.ToString() + " Sekunden");

        CloseConnection();
    }

    public abstract SQLBackAbstract OtherTable(string tablename);

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
        if (!colUndo.Contains("COLUMNNAME")) { AddColumn(SYS_UNDO, "COLUMNNAME", false); }
        if (!colUndo.Contains("ROWKEY")) { AddColumn(SYS_UNDO, "ROWKEY", true); }
        if (!colUndo.Contains("PREVIOUSVALUE")) { AddColumn(SYS_UNDO, "PREVIOUSVALUE", VarChar4000, true); }
        if (!colUndo.Contains("CHANGEDTO")) { AddColumn(SYS_UNDO, "CHANGEDTO", VarChar4000, true); }
        if (!colUndo.Contains("USERNAME")) { AddColumn(SYS_UNDO, "USERNAME", false); }
        if (!colUndo.Contains("DATETIMEUTC")) { AddColumn(SYS_UNDO, "DATETIMEUTC", false); }

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

    internal SQLBackAbstract? HandleMe(ConnectionInfo ci) {
        if (ci is null) { return null; }

        //var s = connectionID.SplitBy("|");
        //if (s.Count() != 2) { return null; }

        foreach (var thisK in ConnectedSQLBack) {
            if (thisK.ConnectionOk && thisK.ConnectionString == ci.DatabaseID) {
                return thisK.OtherTable(ci.TableName);
            }

            if (thisK.ConnectionOk && thisK.ConnectionString  + " LITE" == ci.DatabaseID) {
                return thisK.OtherTable(ci.TableName);
            }

        }

        return null;
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

    public bool OpenConnection() {
        if (_connection == null) { return false; }

        if (_connection.State == ConnectionState.Closed) {
            _connection.Open();
        }

        return _connection.State == ConnectionState.Open;
    }

    private void AddColumn(string tablename, string column, bool nullable) => AddColumn(tablename, column, VarChar255, nullable);

    private void AddColumn(string tablename, string column, string type, bool nullable) {
        var n = " NOT NULL";
        if (nullable) { n = string.Empty; }

        ExecuteCommand("alter table " + tablename.ToUpper() + " add " + column + " " + type + " default ''" + n);
    }

    private string DBVAL(long original) {
        return DBVAL(original.ToString());
    }

    private string DBVAL(string original) {
        original = original.CutToUTF8Length(MaxStringLenght);
        original =  original.Replace("'", "''");
        return "'" + original  + "'";
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

    private string? GetCellValue(string tablename, ColumnItem column, RowItem row) {
        if (!OpenConnection()) { return null; }

        using var q = _connection.CreateCommand();

        q.CommandText = @"select " + column.Name.ToUpper() + " from " + tablename.ToUpper() + " " +
                        "where RK = " + DBVAL(row.Key);

        ////command.AddParameterWithValue("" + DBVAL(columnName.ToUpper()) , column.Name.ToUpper());
        //command.AddParameterWithValue("" + DBVAL(row.Key)", row.Key.ToString());

        using var reader = q.ExecuteReader();
        if (reader.Read()) {
            // you may want to check if value is NULL: reader.IsDBNull(0)
            var value = reader[0].ToString();

            if (reader.Read()) {
                // Doppelter Wert?!?Ersten Wert zurückgeben, um unendliche erweiterungen zu erhindern
                Develop.DebugPrint(column.Key.ToString() + " doppelt in " + tablename.ToUpper() + " vorhanden!");
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

    private void RenameColumn(string tablename, string oldname, string newname) {
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

    /// <summary>
    /// Gibt TRUE zurück, wenn alles ok ist.
    /// Entweder der Wert gesetzt wurde, der Wert aktuell ist oder der Wert unwichtig ist.
    /// </summary>
    /// <param name="tablename"></param>
    /// <param name="type"></param>
    /// <param name="columnName"></param>
    /// <param name="newValue"></param>
    /// <returns></returns>
    private bool SetCellValue(string tablename, ColumnItem column, RowItem row, string newValue) {
        var isVal = GetCellValue(tablename, column, row);

        string cmdString;

        if (isVal is null) {
            cmdString = "INSERT INTO " + tablename.ToUpper() + " (RK, " + column.Name.ToUpper() + " ) VALUES (" + DBVAL(row.Key) + ", " + DBVAL(newValue) + " )";
        } else if (isVal != newValue) {
            cmdString = "UPDATE " + tablename.ToUpper() + " SET " + column.Name.ToUpper() + " = " + DBVAL(newValue) + " WHERE RK = " + DBVAL(row.Key);
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

    internal async void  LoadColumns(string tablename, ListExt<ColumnItem> columns) {
        if (columns == null || columns.Count ==0) { return; }
        if (!OpenConnection()) { return; }

        var com = "SELECT RK, ";

        foreach (var thiscolumn in columns) {
            com = com + thiscolumn.Name.ToUpper() + ", ";
        }
        com = com.TrimEnd(", ");
        com = com + " FROM " + tablename.ToUpper();

        OpenConnection();

        using var command = _connection.CreateCommand();
        command.CommandText = com; 

        using var reader = await command.ExecuteReaderAsync();


        while (await reader.ReadAsync()) {
            var rk = LongParse(reader[0].ToString());
            var row = columns[0].Database.Row.SearchByKey(rk);

            if(row == null) {
                row = new RowItem(columns[0].Database, rk);
                columns[0].Database.Row.Add(row);
            }



            for (var z = 1; z < reader.FieldCount; z++) {
                row.Database.Cell.SetValueInternal(columns[z - 1], row, reader[z].ToString(), -1, -1);
            }
        }

        foreach (var thiscolumn in columns) {
            var val = GetStyleData(tablename, DatabaseDataType.ColumnTimeCode.ToString(), thiscolumn.Name);
            thiscolumn.TimeCode = val;
        }




        //Develop.DebugPrint(FehlerArt.Info, "Datenbank Ladezeit: " + row.Database.TableName + " - " + DateTime.Now.Subtract(ti).TotalSeconds.ToString() + " Sekunden");

        CloseConnection();




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