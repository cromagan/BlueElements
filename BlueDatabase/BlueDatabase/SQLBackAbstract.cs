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
using System.Drawing;
using System.IO;
using static BlueBasics.Converter;

namespace BlueDatabase;

public static class SQLExtension {

    #region Methods


    public static void AddParameterWithValue(this DbCommand? command, string parameterName, object parameterValue) {
        // https://stackoverflow.com/questions/21608362/missing-addwithvalue-from-dbcommand-parameters

        if (command == null) { return; }
        var parameter = command.CreateParameter();
        parameter.ParameterName = parameterName;
        parameter.Value = parameterValue;
        command.Parameters.Add(parameter);
    }

    #endregion
}

//https://www.c-sharpcorner.com/article/create-a-sql-server-database-dynamically-in-C-Sharp/
//https://www.ictdemy.com/csharp/databases/introduction-to-databases-in-csharp-net
//https://docs.microsoft.com/en-us/troubleshoot/developer/visualstudio/csharp/language-compilers/create-sql-server-database-programmatically
public abstract class SQLBackAbstract {

    #region Fields

    protected readonly DbConnection _connection;
    private string _filename;

    public abstract string VarChar4000 { get; }
    public abstract string VarChar255 { get; }

    #endregion

    #region Constructors

    public SQLBackAbstract(string filename, bool create) {
        Develop.StartService();

        _connection = CreateConnection(filename, create);
        _filename = filename;

        var x = ListTables();

        #region Main

        if (!x.Contains("MAIN")) { CreateTable("MAIN", new List<string>() { "RK" }); }

        #endregion

        #region Style

        if (!x.Contains("Style")) { CreateTable("Style", new List<string>() { "DBNAME", "COLUMNNAME", "TYPE" }); }

        var colStyle = GetColumnNames("Style");
        if (colStyle == null) { Develop.DebugPrint(FehlerArt.Fehler, "Spaltenfehler"); return; }

        if (!colStyle.Contains("VALUE")) { AddColumn("Style", "VALUE", VarChar4000); }

        #endregion

        #region  Undo

        if (!x.Contains("Undo")) { CreateTable("Undo"); }

        var colUndo = GetColumnNames("Undo");
        if (colUndo == null) { Develop.DebugPrint(FehlerArt.Fehler, "Spaltenfehler"); return; }
        if (!colUndo.Contains("DBNAME")) { AddColumn("Undo", "DBNAME"); }
        if (!colUndo.Contains("COMAND")) { AddColumn("Undo", "COMAND"); }
        if (!colUndo.Contains("COLUMNKEY")) { AddColumn("Undo", "COLUMNKEY"); }
        if (!colUndo.Contains("ROWKEY")) { AddColumn("Undo", "ROWKEY"); }
        if (!colUndo.Contains("PREVIOUSVALUE")) { AddColumn("Undo", "PREVIOUSVALUE", VarChar4000); }
        if (!colUndo.Contains("CHANGEDTO")) { AddColumn("Undo", "CHANGEDTO", VarChar4000); }
        if (!colUndo.Contains("USERNAME")) { AddColumn("Undo", "USERNAME"); }
        if (!colUndo.Contains("DATETIMEUTC")) { AddColumn("Undo", "DATETIMEUTC"); }

        #endregion

        CloseConnection();
    }

    protected abstract bool CreateTable(string v);
    protected abstract bool CreateTable(string v, List<string> list);

    #endregion

    #region Properties

    /// <summary>
    /// Load oder SaveAsAndChangeTo benutzen
    /// </summary>
    public string Filename {
        get => _filename;
        private set {
            if (string.IsNullOrEmpty(value)) {
                _filename = string.Empty;
            } else {
                var tmp = Path.GetFullPath(value);
                _filename = tmp;
            }
        }
    }

    #endregion

    #region Methods

    public bool AddUndo(string dbname, DatabaseDataType comand, long columnKey, long rowKey, string previousValue, string changedTo, string userName) {
        if (!OpenConnection()) { return false; }

        var cmdString = "INSERT INTO Undo (DBNAME, COMAND, COLUMNKEY, ROWKEY, PREVIOUSVALUE, CHANGEDTO, USERNAME, DATETIMEUTC) VALUES (@DBNAME, @COMAND, @COLUMNKEY, @ROWKEY, @PREVIOUSVALUE ,@CHANGEDTO, @USERNAME, @DATETIMEUTC)";
        using var comm = _connection.CreateCommand();
        comm.CommandText = cmdString;
        comm.AddParameterWithValue("@DBNAME", dbname.ToUpper());
        comm.AddParameterWithValue("@COMAND", ((int)comand).ToString());
        comm.AddParameterWithValue("@COLUMNKEY", columnKey.ToString());
        comm.AddParameterWithValue("@ROWKEY", rowKey.ToString());
        comm.AddParameterWithValue("@PREVIOUSVALUE", previousValue);
        comm.AddParameterWithValue("@CHANGEDTO", changedTo);
        comm.AddParameterWithValue("@USERNAME", userName);
        comm.AddParameterWithValue("@DATETIMEUTC", DateTime.UtcNow.ToString(Constants.Format_Date));

        return ExecuteCommand(comm);
    }

    public bool CheckIn(string database, DatabaseDataType type, string value, SQL_ColumnItem? column, SQL_RowItem? row, int width, int height) {
        if (!OpenConnection()) { return false; }

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
                    return SetStyleData(database, type, string.Empty, value);
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
                    if (type == DatabaseDataType.ColumnCaption) { AddColumnToMain(column.Name); }

                    return SetStyleData(database, type, column.Name.ToUpper(), value);
            }
            CloseConnection();
            return true;
        }

        if (column != null && row != null) {
            SetCellValue(column, row, value);

            CloseConnection();
            return true;
        }

        CloseConnection();
        return false;
    }

    public bool CheckIn(string database, DatabaseDataType type, string value, ColumnItem? column, RowItem? row, int width, int height) {
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
                    return SetStyleData(database, type, string.Empty, value);
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
                    if (type == DatabaseDataType.ColumnCaption) { AddColumnToMain(column.Name); }

                    return SetStyleData(database, type, column.Name.ToUpper(), value);
            }
            CloseConnection();
            return true;
        }

        if (column != null && row != null) {
            SetCellValue(column, row, value);

            CloseConnection();
            return true;
        }

        CloseConnection();
        return false;
    }

    /// <summary>
    /// Gibt die Spaltenname in Grosschreibung zurück
    /// </summary>
    /// <param name="table"></param>
    /// <returns></returns>
    public List<string>? GetColumnNames(string table) {
        if (!OpenConnection()) { return null; }

        var columns = new List<string>();

        using var com = _connection.CreateCommand();
        com.CommandText = @"SELECT * FROM " + table;

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
    /// <param name="dbname"></param>
    /// <param name="columnName"></param>
    /// <returns></returns>
    public Dictionary<string, string> GetStylDataAll(string dbname, string columnName) {
        var l = new Dictionary<string, string>();

        if (!OpenConnection()) { return l; }

        using var q = _connection.CreateCommand();
        // (DBNAME, TYPE, COLUMNNAME, VALUE)
        q.CommandText = @"select TYPE, VALUE from Style " +
                        "where DBNAME = @DBNAME " +
                        "and COLUMNNAME = @COLUMNNAME";

        q.AddParameterWithValue("@DBNAME", dbname.ToUpper());
        q.AddParameterWithValue("@COLUMNNAME", columnName.ToUpper());

        using var reader = q.ExecuteReader();

        while (reader.Read()) {
            // you may want to check if value is NULL: reader.IsDBNull(0)
            l.Add(reader[0].ToString(), reader[1].ToString());
        }

        CloseConnection();    // Nix vorhanden!
        return l;
    }

    public abstract List<string> ListTables();

    public void LoadAllCells(string fileNameWithoutSuffix, SQL_RowCollection row) {
        if (!OpenConnection()) { return; }

        using var q = _connection.CreateCommand();

        q.CommandText = @"select * from Main";

        using var reader = q.ExecuteReader();

        row.Clear();

        while (reader.Read()) {
            var rk = LongParse(reader[0].ToString());
            var r = new SQL_RowItem(row.Database, rk);
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

    /// <summary>
    /// Gibt TRUE zurück, wenn alles ok ist.
    /// Entweder der Wert gesetzt wurde, der Wert aktuell ist oder der Wert unwichtig ist.
    /// </summary>
    /// <param name="dbname"></param>
    /// <param name="type"></param>
    /// <param name="columnName"></param>
    /// <param name="newValue"></param>
    /// <returns></returns>
    public bool SetStyleData(string dbname, DatabaseDataType type, string columnName, string newValue) {
        var t = type.ToString();

        if (t == ((int)type).ToString()) { return true; }

        if (type == DatabaseDataType.AddColumn) { return true; } // enthält zwar den Key, aber Wertlos, wenn der Spaltenname noch nicht bekannt ist...
        if (type == DatabaseDataType.AutoExport) { return true; }
        if (type == DatabaseDataType.UndoInOne) { return true; }

        return SetStyleData(dbname, t, columnName, newValue);
    }

    /// <summary>
    /// Gibt TRUE zurück, wenn alles ok ist.
    /// Entweder der Wert gesetzt wurde, der Wert aktuell ist oder der Wert unwichtig ist.
    /// </summary>
    /// <param name="dbname"></param>
    /// <param name="type"></param>
    /// <param name="columnName"></param>
    /// <param name="newValue"></param>
    /// <returns></returns>
    public bool SetStyleData(string dbname, string type, string columnName, string newValue) {
        columnName = columnName.ToUpper();

        var isVal = GetStyleData(dbname, type, columnName);

        string cmdString;

        if (isVal is null) {
            cmdString = "INSERT INTO Style (DBNAME, TYPE, COLUMNNAME, VALUE)  VALUES (@DBNAME, @TYPE, @COLUMNNAME, @VALUE)";
        } else if (isVal != newValue) {
            cmdString = "UPDATE Style SET VALUE = @VALUE WHERE DBNAME = @DBNAME AND TYPE = @TYPE AND COLUMNNAME = @COLUMNNAME";
        } else {
            return true;
        }

        if (!OpenConnection()) { return false; }

        using var comm = _connection.CreateCommand();
        comm.CommandText = cmdString;
        comm.AddParameterWithValue("@DBNAME", dbname.ToUpper());
        comm.AddParameterWithValue("@TYPE", type);
        comm.AddParameterWithValue("@COLUMNNAME", columnName);
        comm.AddParameterWithValue("@VALUE", newValue);

        return ExecuteCommand(comm);
    }

    protected abstract DbConnection CreateConnection(string filename, bool create);

    private bool AddColumn(string table, string column) => ExecuteCommand("alter table " + table + " add " + column + " " + VarChar255  + " default '' NOT NULL");

    private bool AddColumn(string table, string column, string type) => ExecuteCommand("alter table " + table + " add " + column + " " + type + " default '' NOT NULL");

    private void AddColumnToMain(string columnName) {
        columnName = columnName.ToUpper();

        var colMain = GetColumnNames("Main");
        if (colMain == null) { Develop.DebugPrint(FehlerArt.Fehler, "Spaltenfehler"); return; }
        if (!colMain.Contains(columnName)) { AddColumn("Main", columnName, VarChar4000); }
    }

    protected bool CloseConnection() {
        if (_connection.State == ConnectionState.Open) { _connection.Close(); }

        return _connection.State == ConnectionState.Closed;
    }


    protected bool ExecuteCommand(string commandtext) {
        if (!OpenConnection()) { return false; }

        using var command = _connection.CreateCommand();
        command.CommandText = commandtext;

        return ExecuteCommand(command);
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

    private string? GetCellValue(SQL_ColumnItem column, SQL_RowItem row) {
        if (!OpenConnection()) { return null; }

        using var q = _connection.CreateCommand();

        q.CommandText = @"select " + column.Name.ToUpper() + " from Main " +
                        "where RK = @RK";

        //q.AddParameterWithValue("@COLUMNNAME", column.Name.ToUpper());
        q.AddParameterWithValue("@RK", row.Key.ToString());

        using var reader = q.ExecuteReader();
        if (reader.Read()) {
            // you may want to check if value is NULL: reader.IsDBNull(0)
            var value = reader[0].ToString();

            if (reader.Read()) {
                // Doppelter Wert?!?Ersten Wert zurückgeben, um unendliche erweiterungen zu erhindern
                Develop.DebugPrint(column.Key.ToString() + " doppelt in MAIN vorhanden!");
            }

            CloseConnection();
            return value;
        }

        CloseConnection();    // Nix vorhanden!
        return null;
    }

    private string? GetCellValue(ColumnItem column, RowItem row) {
        if (!OpenConnection()) { return null; }

        using var q = _connection.CreateCommand();

        q.CommandText = @"select " + column.Name.ToUpper() + " from Main " +
                        "where RK = @RK";

        //q.AddParameterWithValue("@COLUMNNAME", column.Name.ToUpper());
        q.AddParameterWithValue("@RK", row.Key.ToString());

        using var reader = q.ExecuteReader();
        if (reader.Read()) {
            // you may want to check if value is NULL: reader.IsDBNull(0)
            var value = reader[0].ToString();

            if (reader.Read()) {
                // Doppelter Wert?!?Ersten Wert zurückgeben, um unendliche erweiterungen zu erhindern
                Develop.DebugPrint(column.Key.ToString() + " doppelt in MAIN vorhanden!");
            }

            CloseConnection();
            return value;
        }

        CloseConnection();    // Nix vorhanden!
        return null;
    }

    private string? GetStyleData(string dbname, string type, string columnName) {
        if (!OpenConnection()) { return null; }

        using var q = _connection.CreateCommand();

        q.CommandText = @"select VALUE from Style " +
                        "where DBNAME = @DBNAME " +
                        "and TYPE = @TYPE " +
                        "and COLUMNNAME = @COLUMNNAME";

        q.AddParameterWithValue("@DBNAME", dbname.ToUpper());
        q.AddParameterWithValue("@TYPE", type);
        q.AddParameterWithValue("@COLUMNNAME", columnName.ToUpper());

        using var reader = q.ExecuteReader();
        if (reader.Read()) {
            // you may want to check if value is NULL: reader.IsDBNull(0)
            var value = reader[0].ToString();

            if (reader.Read()) {
                // Doppelter Wert?!?Ersten Wert zurückgeben, um unendliche erweiterungen zu erhindern
                Develop.DebugPrint(dbname + " " + type + " " + columnName + " doppelt in Style vorhanden!");
            }

            CloseConnection();
            return value;
        }

        CloseConnection();    // Nix vorhanden!
        return null;
    }

    protected bool OpenConnection() {
        if (_connection.State == ConnectionState.Closed) {
            _connection.Open();
        }

        return _connection.State == ConnectionState.Open;
    }

    /// <summary>
    /// Gibt TRUE zurück, wenn alles ok ist.
    /// Entweder der Wert gesetzt wurde, der Wert aktuell ist oder der Wert unwichtig ist.
    /// </summary>
    /// <param name="dbname"></param>
    /// <param name="type"></param>
    /// <param name="columnName"></param>
    /// <param name="newValue"></param>
    /// <returns></returns>
    private bool SetCellValue(SQL_ColumnItem column, SQL_RowItem row, string newValue) {
        var isVal = GetCellValue(column, row);

        string cmdString;

        if (isVal is null) {
            cmdString = "INSERT INTO Main (RK, " + column.Name.ToUpper() + " ) VALUES (@RK, @VALUE )";
        } else if (isVal != newValue) {
            cmdString = "UPDATE Main SET " + column.Name.ToUpper() + " = @VALUE WHERE RK = @RK";
        } else {
            return true;
        }

        if (!OpenConnection()) { return false; }

        using var comm = _connection.CreateCommand();
        comm.CommandText = cmdString;
        comm.AddParameterWithValue("@RK", row.Key);
        comm.AddParameterWithValue("@VALUE", newValue);

        return ExecuteCommand(comm);
    }

    /// <summary>
    /// Gibt TRUE zurück, wenn alles ok ist.
    /// Entweder der Wert gesetzt wurde, der Wert aktuell ist oder der Wert unwichtig ist.
    /// </summary>
    /// <param name="dbname"></param>
    /// <param name="type"></param>
    /// <param name="columnName"></param>
    /// <param name="newValue"></param>
    /// <returns></returns>
    private bool SetCellValue(ColumnItem column, RowItem row, string newValue) {
        var isVal = GetCellValue(column, row);

        string cmdString;

        if (isVal is null) {
            cmdString = "INSERT INTO Main (RK, " + column.Name.ToUpper() + " ) VALUES (@RK, @VALUE )";
        } else if (isVal != newValue) {
            cmdString = "UPDATE Main SET " + column.Name.ToUpper() + " = @VALUE WHERE RK = @RK";
        } else {
            return true;
        }

        if (!OpenConnection()) { return false; }

        using var comm = _connection.CreateCommand();
        comm.CommandText = cmdString;
        comm.AddParameterWithValue("@RK", row.Key);
        comm.AddParameterWithValue("@VALUE", newValue);

        return ExecuteCommand(comm);
    }

    #endregion
}