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

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using BlueBasics;
using System.IO;
using BlueDatabase.Enums;
using BlueBasics.Enums;

namespace BlueDatabase;

//https://www.c-sharpcorner.com/article/create-a-sql-server-database-dynamically-in-C-Sharp/
//https://www.ictdemy.com/csharp/databases/introduction-to-databases-in-csharp-net
//https://docs.microsoft.com/en-us/troubleshoot/developer/visualstudio/csharp/language-compilers/create-sql-server-database-programmatically
public class SqlBack {

    #region Fields

    private readonly SqlConnection _connection;
    private string _filename;

    #endregion

    #region Constructors

    public SqlBack(string filename, bool create) {
        Develop.StartService();
        //CultureInfo culture = new("de-DE");
        //CultureInfo.DefaultThreadCurrentCulture = culture;
        //CultureInfo.DefaultThreadCurrentUICulture = culture;

        _filename = filename.FilePath() + filename.FileNameWithoutSuffix() + ".mdf";

        if (create && !File.Exists(_filename)) { CreateDatabase(_filename); }

        _connection = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + _filename + ";Integrated Security=True;Trusted_Connection=Yes;");

        var x = ListTables();

        #region Main

        if (!x.Contains("Main")) { CreateTable("Main", new List<string>() { "RK" }); }
        //ExecuteCommand("SET IDENTITY_INSERT Main ON");

        #endregion

        #region Style

        if (!x.Contains("Style")) { CreateTable("Style", new List<string>() { "DBNAME", "COLUMNNAME", "TYPE" }); }

        var colStyle = GetColumnNames("Style");
        if (colStyle == null) { Develop.DebugPrint(FehlerArt.Fehler, "Spaltenfehler"); return; }

        if (!colStyle.Contains("VALUE")) { AddColumn("Style", "VALUE", "VARCHAR(8000)"); }

        #endregion

        #region  Undo

        if (!x.Contains("Undo")) { CreateTable("Undo"); }

        var colUndo = GetColumnNames("Undo");
        if (colUndo == null) { Develop.DebugPrint(FehlerArt.Fehler, "Spaltenfehler"); return; }
        if (!colUndo.Contains("DBNAME")) { AddColumn("Undo", "DBNAME"); }
        if (!colUndo.Contains("COMAND")) { AddColumn("Undo", "COMAND"); }
        if (!colUndo.Contains("COLUMNKEY")) { AddColumn("Undo", "COLUMNKEY"); }
        if (!colUndo.Contains("ROWKEY")) { AddColumn("Undo", "ROWKEY"); }
        if (!colUndo.Contains("PREVIOUSVALUE")) { AddColumn("Undo", "PREVIOUSVALUE", "VARCHAR(8000)"); }
        if (!colUndo.Contains("CHANGEDTO")) { AddColumn("Undo", "CHANGEDTO", "VARCHAR(8000)"); }
        if (!colUndo.Contains("USERNAME")) { AddColumn("Undo", "USERNAME"); }
        if (!colUndo.Contains("DATETIMEUTC")) { AddColumn("Undo", "DATETIMEUTC"); }

        #endregion

        CloseConnection();
    }

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

        string cmdString = "INSERT INTO Undo (DBNAME, COMAND, COLUMNKEY, ROWKEY, PREVIOUSVALUE, CHANGEDTO, USERNAME, DATETIMEUTC) VALUES (@DBNAME, @COMAND, @COLUMNKEY, @ROWKEY, @PREVIOUSVALUE ,@CHANGEDTO, @USERNAME, @DATETIMEUTC)";
        using SqlCommand comm = new SqlCommand();
        comm.Connection = _connection;
        comm.CommandText = cmdString;
        comm.Parameters.AddWithValue("@DBNAME", dbname.ToUpper());
        comm.Parameters.AddWithValue("@COMAND", ((int)comand).ToString());
        comm.Parameters.AddWithValue("@COLUMNKEY", columnKey.ToString());
        comm.Parameters.AddWithValue("@ROWKEY", rowKey.ToString());
        comm.Parameters.AddWithValue("@PREVIOUSVALUE", previousValue);
        comm.Parameters.AddWithValue("@CHANGEDTO", changedTo);
        comm.Parameters.AddWithValue("@USERNAME", userName);
        comm.Parameters.AddWithValue("@DATETIMEUTC", DateTime.UtcNow.ToString(Constants.Format_Date));

        return ExecuteCommand(comm);
    }

    public bool CheckIn(string database, DatabaseDataType type, string value, ColumnItem? column, RowItem? row, int width, int height) {
        if (!OpenConnection()) { return false; }

        if (column == null && row == null) {
            switch (type) {
                case DatabaseDataType.Formatkennung: break;
                case DatabaseDataType.Werbung: break;
                case DatabaseDataType.EOF: break;
                case DatabaseDataType.co_SaveContent: break;
                case DatabaseDataType.CryptionState: break;
                case DatabaseDataType.CryptionTest: break;
                default:
                    return SetStyleDate(database, type, string.Empty, value);
            }

            CloseConnection();
            return true;
        }

        if (column != null && row == null) {
            switch (type) {
                //case DatabaseDataType.co_EditType: break;
                case DatabaseDataType.co_SaveContent: break;
                case DatabaseDataType.co_ShowUndo: break;
                case DatabaseDataType.ColumnName: break;

                default:
                    if (type == DatabaseDataType.ColumnCaption) { AddColumnToMain(column.Name); }

                    return SetStyleDate(database, type, column.Name.ToUpper(), value);
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
    /// Dateiendung wird immer mdf benutzt!
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public bool CreateDatabase(string filename) {
        //var myConn = new SqlConnection("Server=localhost;Integrated security=SSPI;database=master");
        //var myConn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\Database1.mdf;Integrated Security=True");
        //var myConn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;Integrated Security=True");
        //var myConn = new SqlConnection("Integrated Security=SSPI;Initial Catalog=;Data Source=localhost;");
        // var myConn = new SqlConnection("Server=(local)\\netsdk;uid=sa;pwd=;database=master");

        var myConn = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;Integrated security=SSPI;database=master");
        var dbn = filename.FileNameWithoutSuffix();
        filename = filename.FilePath() + filename.FileNameWithoutSuffix() + ".mdf";

        var str = "CREATE DATABASE " + dbn + " ON PRIMARY " +
                  "(NAME = " + dbn + "_Data, " +
                  "FILENAME = '" + filename + "', " +
                  "SIZE = 2MB, MAXSIZE = 512MB, FILEGROWTH = 1MB)" +
                  "LOG ON (NAME = " + dbn + "_Log, " +
                  "FILENAME = '" + filename.TrimEnd(".mdf") + "Log.ldf', " +
                  "SIZE = 1MB, " +
                  "MAXSIZE = 50MB, " +
                  "FILEGROWTH = 1MB)";

        var myCommand = new SqlCommand(str, myConn);
        var ok = false;
        try {
            myConn.Open();
            myCommand.ExecuteNonQuery();
            //MessageBox.Show("DataBase is Created Successfully", "MyProgram", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ok = true;
        } catch (Exception ex) {
            Develop.DebugPrint(ex);
            //MessageBox.Show(ex.ToString(), "MyProgram", MessageBoxButtons.OK, MessageBoxIcon.Information);
        } finally {
            if (myConn.State == ConnectionState.Open) {
                myConn.Close();
            }
        }
        return ok;
    }

    /// <summary>
    /// Gibt die Spaltenname in Grosschreibung zurück
    /// </summary>
    /// <param name="table"></param>
    /// <returns></returns>
    public List<string>? GetColumnNames(string table) {
        if (!OpenConnection()) { return null; }

        List<string> columns = new List<string>();

        using var com = new SqlCommand(@"SELECT * FROM " + table, _connection);
        using var reader = com.ExecuteReader(CommandBehavior.SchemaOnly);

        var schemaTable = reader.GetSchemaTable();
        if (schemaTable == null) { return null; }

        foreach (DataRow colRow in schemaTable.Rows) {
            columns.Add(colRow.Field<string>("ColumnName").ToUpper());
        }

        CloseConnection();
        return columns;
    }

    public List<string> ListTables() {
        List<string> tables = new();

        OpenConnection();

        DataTable dt = _connection.GetSchema("Tables");
        foreach (DataRow row in dt.Rows) {
            string tablename = (string)row[2];
            tables.Add(tablename);
        }

        CloseConnection();

        return tables;
    }

    private bool AddColumn(string table, string column) => ExecuteCommand("alter table " + table + " add " + column + " VARCHAR(255) default '' NOT NULL");

    private bool AddColumn(string table, string column, string type) => ExecuteCommand("alter table " + table + " add " + column + " " + type + " default '' NOT NULL");

    private void AddColumnToMain(string columnName) {
        columnName = columnName.ToUpper();

        var colMain = GetColumnNames("Main");
        if (colMain == null) { Develop.DebugPrint(FehlerArt.Fehler, "Spaltenfehler"); return; }
        if (!colMain.Contains(columnName)) { AddColumn("Main", columnName, "VARCHAR(8000)"); }
    }

    private bool CloseConnection() {
        if (_connection.State == ConnectionState.Open) { _connection.Close(); }

        return _connection.State == ConnectionState.Closed;
    }

    private bool CreateTable(string name) {
        if (!ExecuteCommand("DROP TABLE IF EXISTS " + name)) { return false; }
        return ExecuteCommand(@"CREATE TABLE " + name + "(" + "RK bigint identity(1,1) NOT NULL PRIMARY KEY, " + ")");
    }

    private bool CreateTable(string name, List<string> keycolumns) {
        if (!ExecuteCommand("DROP TABLE IF EXISTS " + name)) { return false; }

        // http://www.sql-server-helper.com/error-messages/msg-8110.aspx

        var t = @"CREATE TABLE " + name + "(";

        foreach (var thiskey in keycolumns) {
            t += thiskey.ToUpper() + " VARCHAR(255) default '' NOT NULL, ";
        }
        t += ")";

        if (!ExecuteCommand(t)) { return false; }
        //if (!ExecuteCommand("SET IDENTITY_INSERT " + name + " ON")) { return false; }

        t = "ALTER TABLE " + name + " ADD CONSTRAINT PK_" + name.ToUpper() + " PRIMARY KEY CLUSTERED(" + keycolumns.JoinWith(", ").ToUpper() + ")";

        return ExecuteCommand(t);
    }

    private bool ExecuteCommand(string commandtext) {
        if (!OpenConnection()) { return false; }

        using var command = new SqlCommand(commandtext, _connection);

        return ExecuteCommand(command);
    }

    private bool ExecuteCommand(SqlCommand command) {
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

    private string? GetCellValue(ColumnItem column, RowItem row) {
        if (!OpenConnection()) { return null; }

        using var q = _connection.CreateCommand();

        q.CommandText = @"select " + column.Name.ToUpper() + " from Main " +
                        "where RK = @RK";

        //q.Parameters.AddWithValue("@COLUMNNAME", column.Name.ToUpper());
        q.Parameters.AddWithValue("@RK", row.Key.ToString());

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

        q.Parameters.AddWithValue("@DBNAME", dbname.ToUpper());
        q.Parameters.AddWithValue("@TYPE", type);
        q.Parameters.AddWithValue("@COLUMNNAME", columnName.ToUpper());

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

    private bool OpenConnection() {
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

        using SqlCommand comm = new SqlCommand();
        comm.Connection = _connection;
        comm.CommandText = cmdString;
        //comm.Parameters.AddWithValue("@DBNAME", "Main");
        comm.Parameters.AddWithValue("@RK", row.Key);
        //comm.Parameters.AddWithValue("@COLUMNNAME", column.Name.ToUpper());
        comm.Parameters.AddWithValue("@VALUE", newValue);

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
    private bool SetStyleDate(string dbname, DatabaseDataType type, string columnName, string newValue) {
        var t = type.ToString();

        if (t == ((int)type).ToString()) { return true; }

        if (type == DatabaseDataType.AddColumn) { return true; }

        columnName = columnName.ToUpper();

        var isVal = GetStyleData(dbname, t, columnName);

        string cmdString;

        if (isVal is null) {
            cmdString = "INSERT INTO Style (DBNAME, TYPE, COLUMNNAME, VALUE)  VALUES (@DBNAME, @TYPE, @COLUMNNAME, @VALUE)";
        } else if (isVal != newValue) {
            cmdString = "UPDATE Style SET VALUE = @VALUE WHERE DBNAME = @DBNAME AND TYPE = @TYPE AND COLUMNNAME = @COLUMNNAME";
        } else {
            return true;
        }

        if (!OpenConnection()) { return false; }

        using SqlCommand comm = new SqlCommand();
        comm.Connection = _connection;
        comm.CommandText = cmdString;
        comm.Parameters.AddWithValue("@DBNAME", dbname.ToUpper());
        comm.Parameters.AddWithValue("@TYPE", t);
        comm.Parameters.AddWithValue("@COLUMNNAME", columnName);
        comm.Parameters.AddWithValue("@VALUE", newValue);

        return ExecuteCommand(comm);
    }

    #endregion
}