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

namespace BlueDatabase;

//public static class SQLExtension {
//    #region Methods

//    public static void AddParameterWithValue(this DbCommand? command, string parameterName, object parameterValue) {
//        // https://stackoverflow.com/questions/21608362/missing-addwithvalue-from-dbcommand-parameters

//        if (command == null) { return; }
//        var parameter = command.CreateParameter();
//        parameter.ParameterName = parameterName;
//        parameter.Value = parameterValue;
//        command.Parameters.Add(parameter);
//    }

//    #endregion
//}

//https://www.c-sharpcorner.com/article/create-a-sql-server-database-dynamically-in-C-Sharp/
//https://www.ictdemy.com/csharp/databases/introduction-to-databases-in-csharp-net
//https://docs.microsoft.com/en-us/troubleshoot/developer/visualstudio/csharp/language-compilers/create-sql-server-database-programmatically
public abstract class SQLBackAbstract {

    #region Fields

    public const string SYS_STYLE = "SYS_STYLE";
    public const string SYS_UNDO = "SYS_UNDO";
    protected DbConnection? _connection;

    #endregion

    #region Constructors

    public SQLBackAbstract() { }

    #endregion

    #region Properties

    public bool ConnectionOk => _connection != null;
    public abstract string ConnectionString { get; protected set; }

    /// <summary>
    /// Falls die Datenbank von einer lokalen Datei geladen wurde, ist hier der Dateiname enthalten.
    /// </summary>
    public string Filename { get; protected set; } = string.Empty;

    public abstract string ID { get; }
    public abstract string Primary { get; }

    public abstract string VarChar255 { get; }

    public abstract string VarChar4000 { get; }

    #endregion

    #region Methods

    public static bool IsValidTableName(string tablename) {
        var t = tablename.ToUpper();

        if (t.StartsWith("SYS_")) {
            return false;
        }

        if (!t.ContainsOnlyChars(Constants.Char_AZ + Constants.Char_Numerals + "_")) {
            return false;
        }

        return true;
    }

    public bool AddUndo(string tablename, DatabaseDataType comand, long columnKey, long rowKey, string previousValue, string changedTo, string userName) {
        if (!OpenConnection()) { return false; }

        var cmdString = "INSERT INTO " + SYS_UNDO +
            " (TABLENAME, COMAND, COLUMNKEY, ROWKEY, PREVIOUSVALUE, CHANGEDTO, USERNAME, DATETIMEUTC) VALUES (" +
            "'" + tablename.ToUpper() + "'," +
            "'" + ((int)comand).ToString() + "'," +
            "'" + columnKey.ToString() + "'," +
            "'" + rowKey.ToString() + "'," +
            "'" + previousValue + "'," +
            "'" + changedTo + "'," +
            "'" + userName + "'," +
            "'" + DateTime.UtcNow.ToString(Constants.Format_Date) + "')";

        using var comm = _connection.CreateCommand();
        comm.CommandText = cmdString;

        return ExecuteCommand(comm);
    }

    public bool CheckIn(string tablename, DatabaseDataType type, string value, ColumnItem? column, RowItem? row, int width, int height) {
        if (!OpenConnection()) { return false; }

        if ((int)type < 100) { column = null; row = null; }

        if (type != DatabaseDataType.ce_Value_withoutSizeData && type != DatabaseDataType.ce_Value_withSizeData &&
            type != DatabaseDataType.ce_UTF8Value_withoutSizeData && type != DatabaseDataType.ce_UTF8Value_withSizeData) {
            row = null;
        }

        if (column == null && row == null) {
            switch (type) {
                case DatabaseDataType.Formatkennung:
                    break;

                case DatabaseDataType.Werbung:
                    break;

                case DatabaseDataType.EOF:
                    break;

                case DatabaseDataType.co_SaveContent:
                    break;

                case DatabaseDataType.CryptionState:
                    break;

                case DatabaseDataType.CryptionTest:
                    break;

                default:
                    return SetStyleData(tablename, type, string.Empty, value);
            }

            CloseConnection();
            return true;
        }

        if (column != null && row == null) {
            switch (type) {
                //case DatabaseDataType.co_EditType: break;
                case DatabaseDataType.co_SaveContent:
                    break;

                case DatabaseDataType.co_ShowUndo:
                    break;

                case DatabaseDataType.ColumnName:
                    break;

                default:
                    if (type == DatabaseDataType.ColumnCaption) { AddColumnToMain(tablename, column.Name); }

                    return SetStyleData(tablename, type, column.Name.ToUpper(), value);
            }
            CloseConnection();
            return true;
        }

        if (column != null && row != null) {
            SetCellValue(tablename, column, row, value);

            CloseConnection();
            return true;
        }

        CloseConnection();
        return false;
    }

    /// <summary>
    /// Gibt die Spaltenname in Grosschreibung zurück
    /// </summary>
    /// <returns></returns>
    public List<string>? GetColumnNames(string tablename) {
        if (!OpenConnection()) { return null; }

        var columns = new List<string>();

        using var com = _connection.CreateCommand();
        com.CommandText = @"SELECT * FROM " + tablename;

        using var reader = com.ExecuteReader(CommandBehavior.SchemaOnly);

        var schemaTable = reader.GetSchemaTable();
        if (schemaTable == null) { return null; }

        foreach (DataRow colRow in schemaTable.Rows) {
            columns.Add(colRow.Field<string>("ColumnName").ToUpper());
        }

        CloseConnection();
        return columns;
    }

    /// <summary>
    /// Wird kein Spaltenname angegeben, werden die Eigenschaften der Datenbank zurück gegeben.
    /// </summary>
    /// <param name="columnName"></param>
    /// <returns></returns>
    public Dictionary<string, string> GetStylDataAll(string tablename, string columnName) {
        var l = new Dictionary<string, string>();

        if (!OpenConnection()) { return l; }

        using var q = _connection.CreateCommand();
        // (DBNAME, TYPE, COLUMNNAME, VALUE)
        q.CommandText = @"select TYPE, VALUE from " + SYS_STYLE + " " +
                        "where TABLENAME = '" + tablename.ToUpper() + "' " +
                        "and COLUMNNAME = '" + columnName.ToUpper() + "'";

        using var reader = q.ExecuteReader();

        while (reader.Read()) {
            // you may want to check if value is NULL: reader.IsDBNull(0)
            l.Add(reader[0].ToString(), reader[1].ToString());
        }

        CloseConnection();    // Nix vorhanden!
        return l;
    }

    public void LoadAllCells(string tablename, RowCollection row) {
        if (!OpenConnection()) { return; }

        using var q = _connection.CreateCommand();

        q.CommandText = @"select * from " + tablename.ToUpper();

        using var reader = q.ExecuteReader();

        row.Clear();

        while (reader.Read()) {
            var rk = LongParse(reader[0].ToString());
            var r = new RowItem(row.Database, rk);
            row.Add(r);

            for (var z = 1; z < reader.FieldCount; z++) {
                row.Database.Cell.Load_310(row.Database.Column[z - 1], r, reader[z].ToString(), -1, -1);
            }

            //var n = 0;
            //foreach (var thiss in reader) {
            //    n++;
            //    row.Database.Cell.Load_310(row.Database.Column[n], r, thiss.ToString(), -1, -1);
            //    //row.Database.Cell.SetValueBehindLinkedValue(row.Database.Column[n], r, thiss.ToString());
            //    //r.CellSet(row.Database.Column[n], thiss.ToString());
            //}

            ////l.Add(reader[0].ToString(), reader[1].ToString());
        }

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

        if (!x.Contains(SYS_STYLE)) { CreateTable(SYS_STYLE, new List<string>() { "TABLENAME", "COLUMNNAME", "TYPE" }); }

        var colStyle = GetColumnNames(SYS_STYLE);
        if (colStyle == null) { Develop.DebugPrint(FehlerArt.Fehler, "Spaltenfehler"); return; }

        if (!colStyle.Contains("VALUE")) { AddColumn(SYS_STYLE, "VALUE", VarChar4000); }

        #endregion

        #region  Undo

        if (!x.Contains(SYS_UNDO)) { CreateTable(SYS_UNDO); }

        var colUndo = GetColumnNames(SYS_UNDO);
        if (colUndo == null) { Develop.DebugPrint(FehlerArt.Fehler, "Spaltenfehler"); return; }
        if (!colUndo.Contains("TABLENAME")) { AddColumn(SYS_UNDO, "TABLENAME"); }
        if (!colUndo.Contains("COMAND")) { AddColumn(SYS_UNDO, "COMAND"); }
        if (!colUndo.Contains("COLUMNKEY")) { AddColumn(SYS_UNDO, "COLUMNKEY"); }
        if (!colUndo.Contains("ROWKEY")) { AddColumn(SYS_UNDO, "ROWKEY"); }
        if (!colUndo.Contains("PREVIOUSVALUE")) { AddColumn(SYS_UNDO, "PREVIOUSVALUE", VarChar4000); }
        if (!colUndo.Contains("CHANGEDTO")) { AddColumn(SYS_UNDO, "CHANGEDTO", VarChar4000); }
        if (!colUndo.Contains("USERNAME")) { AddColumn(SYS_UNDO, "USERNAME"); }
        if (!colUndo.Contains("DATETIMEUTC")) { AddColumn(SYS_UNDO, "DATETIMEUTC"); }

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
        var t = type.ToString();

        if (t == ((int)type).ToString()) { return true; }

        if (type == DatabaseDataType.AddColumn) { return true; } // enthält zwar den Key, aber Wertlos, wenn der Spaltenname noch nicht bekannt ist...
        if (type == DatabaseDataType.AutoExport) { return true; }
        if (type == DatabaseDataType.UndoInOne) { return true; }

        return SetStyleData(tablename, t, columnName, newValue);
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
    public bool SetStyleData(string tablename, string type, string columnName, string newValue) {
        columnName = columnName.ToUpper();

        var isVal = GetStyleData(tablename, type, columnName);

        if (string.IsNullOrEmpty(columnName)) { columnName = "~Database~"; }

        string cmdString;

        if (isVal is null) {
            cmdString = "INSERT INTO " + SYS_STYLE + " (TABLENAME, TYPE, COLUMNNAME, VALUE)  VALUES ('" + tablename.ToUpper() + "', '" + type + "', '" + columnName.ToUpper() + "', '" + newValue + "')";
        } else if (isVal != newValue) {
            cmdString = "UPDATE " + SYS_STYLE + " SET VALUE = '" + newValue + "' WHERE TABLENAME = '" + tablename.ToUpper() + "' AND TYPE = '" + type + "' AND COLUMNNAME = '" + columnName.ToUpper() + "'";
        } else {
            return true;
        }

        if (!OpenConnection()) { return false; }

        return ExecuteCommand(cmdString);
    }

    public List<string> Tables() {
        var l = AllTables();

        l.Remove(SYS_STYLE);
        l.Remove(SYS_UNDO);
        return l;
    }

    protected abstract List<string> AllTables();

    protected bool CloseConnection() {
        if (_connection.State == ConnectionState.Open) { _connection.Close(); }

        return _connection.State == ConnectionState.Closed;
    }

    protected abstract bool CreateTable(string tablename);

    protected abstract bool CreateTable(string tablename, List<string> keycolumns);

    protected bool ExecuteCommand(string commandtext) {
        if (!OpenConnection()) { return false; }

        using var command = _connection.CreateCommand();
        command.CommandText = commandtext;

        return ExecuteCommand(command);
    }

    protected bool OpenConnection() {
        if (_connection.State == ConnectionState.Closed) {
            _connection.Open();
        }

        return _connection.State == ConnectionState.Open;
    }

    private bool AddColumn(string tablename, string column) => ExecuteCommand("alter table " + tablename.ToUpper() + " add " + column + " " + VarChar255 + " default '' NOT NULL");

    private bool AddColumn(string tablename, string column, string type) => ExecuteCommand("alter table " + tablename.ToUpper() + " add " + column + " " + type + " default '' NOT NULL");

    private void AddColumnToMain(string tablename, string columnName) {
        columnName = columnName.ToUpper();

        var colMain = GetColumnNames(tablename.ToUpper());
        if (colMain == null) { Develop.DebugPrint(FehlerArt.Fehler, "Spaltenfehler"); return; }
        if (!colMain.Contains(columnName)) { AddColumn(tablename.ToUpper(), columnName, VarChar4000); }
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
                        "where RK = '" + row.Key + "'";

        ////q.AddParameterWithValue("'"+columnName.ToUpper()+"'", column.Name.ToUpper());
        //q.AddParameterWithValue("'"+row.Key+"'", row.Key.ToString());

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

    private string? GetStyleData(string tablename, string type, string columnName) {
        if (!OpenConnection()) { return null; }

        using var q = _connection.CreateCommand();

        if (string.IsNullOrEmpty(columnName)) { columnName = "~Database~"; }

        q.CommandText = @"select VALUE from " + SYS_STYLE + " " +
                        "where TABLENAME = '" + tablename.ToUpper() + "' " +
                        "and TYPE = '" + type + "' " +
                        "and COLUMNNAME = '" + columnName.ToUpper() + "' ";

        using var reader = q.ExecuteReader();
        if (reader.Read()) {
            // you may want to check if value is NULL: reader.IsDBNull(0)
            var value = reader[0].ToString();

            if (reader.Read()) {
                // Doppelter Wert?!?Ersten Wert zurückgeben, um unendliche erweiterungen zu erhindern
                Develop.DebugPrint(tablename.ToUpper() + " " + type + " " + columnName + " doppelt in Style vorhanden!");
            }

            CloseConnection();
            return value;
        }

        CloseConnection();    // Nix vorhanden!
        return null;
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
            cmdString = "INSERT INTO " + tablename.ToUpper() + " (RK, " + column.Name.ToUpper() + " ) VALUES ('" + row.Key + "', '" + newValue + "' )";
        } else if (isVal != newValue) {
            cmdString = "UPDATE " + tablename.ToUpper() + " SET " + column.Name.ToUpper() + " = '" + newValue + "' WHERE RK = '" + row.Key + "'";
        } else {
            return true;
        }

        if (!OpenConnection()) { return false; }

        return ExecuteCommand(cmdString);
    }

    #endregion
}