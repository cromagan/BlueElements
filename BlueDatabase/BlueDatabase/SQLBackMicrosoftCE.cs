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

// https://stackoverflow.com/questions/34002901/need-for-sqlconnection-and-oracleconnection-if-odbcconnection-can-be-used-in-th
#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using BlueBasics;
using BlueBasics.Enums;

namespace BlueDatabase;

//https://www.c-sharpcorner.com/article/create-a-sql-server-database-dynamically-in-C-Sharp/
//https://www.ictdemy.com/csharp/databases/introduction-to-databases-in-csharp-net
//https://docs.microsoft.com/en-us/troubleshoot/developer/visualstudio/csharp/language-compilers/create-sql-server-database-programmatically
public class SQLBackMicrosoftCE : SqlBackAbstract {

    #region Constructors

    public SQLBackMicrosoftCE(SQLBackMicrosoftCE sql, string tablename) : base() {
        if (!IsValidTableName(tablename, false)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Tabellename ungültig: " + tablename);
        }

        Filename = sql.Filename;
        ConnectionString = sql.ConnectionString;
        Connection = sql.Connection;
        RepairAll(tablename.ToUpper());
        DoBackUp();
        //        CompareBackUp(new DateTime(2023, 6, 29));
    }

    public SQLBackMicrosoftCE(string filename, bool create) : base() {
        if (create && !File.Exists(filename)) { _ = CreateDatabase(filename); }

        ConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + filename + ";" +
                           "Integrated Security=True;Trusted_Connection=Yes;";

        Filename = filename;
        Connection = new SqlConnection(ConnectionString);

        RepairAll(string.Empty);
    }

    public SQLBackMicrosoftCE(string filename, bool create, string tablename) : base() {
        if (!IsValidTableName(tablename, false)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Tabellename ungültig: " + tablename);
        }

        if (create && !File.Exists(filename)) { _ = CreateDatabase(filename); }

        ConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + filename + ";" +
                           "Integrated Security=True;Trusted_Connection=Yes;";

        Connection = new SqlConnection(ConnectionString);

        RepairAll(tablename.ToUpper());
    }

    #endregion

    #region Properties

    public override string ColumnPropertyPrimary => "bigint identity(1,1)";
    public override string ColumnTypeDate => "DATE";
    public override int MaxStringLenght => 4000;

    #endregion

    #region Methods

    public override List<string>? AllTables() {
        List<string> tables = new();

        if (Connection == null || !OpenConnection()) { return null; }

        var dt = Connection.GetSchema("Tables");
        foreach (DataRow row in dt.Rows) {
            var tablename = (string)row[2];
            tables.Add(tablename);
        }

        _ = CloseConnection();

        return tables;
    }

    public override int ColumnLenght(string tablename, string columnname) => throw new NotImplementedException();

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
        var dbn = MakeValidTableName(filename.FileNameWithoutSuffix());
        filename = filename.FilePath() + dbn + ".mdf";

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
            _ = myCommand.ExecuteNonQuery();
            //MessageBox.Show("DataBase is Created Successfully", "MyProgram", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ok = true;
        } catch (Exception ex) {
            Develop.DebugPrint("Befehl konnte nicht ausgeführt werden", ex);
            //MessageBox.Show(ex.ToString(false), "MyProgram", MessageBoxButtons.OK, MessageBoxIcon.Information);
        } finally {
            if (myConn.State == ConnectionState.Open) {
                myConn.Close();
            }
        }
        return ok;
    }

    public override string? GenerateRow(string tablename) => throw new NotImplementedException();

    /// <summary>
    /// Gibt die Spaltenname in Grosschreibung zurück
    /// </summary>
    /// <returns></returns>
    public override List<string> GetColumnNames(string tablename) {
        var columns = new List<string>();

        if (!IsValidTableName(tablename, true)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Tabellename ungültig: " + tablename);
            return columns;
        }

        if (!OpenConnection() || Connection == null) { return GetColumnNames(tablename); }

        using var com = Connection.CreateCommand();

        com.CommandText = @"SELECT * FROM " + tablename;

        using var reader = com.ExecuteReader(CommandBehavior.SchemaOnly);

        var schemaTable = reader.GetSchemaTable();
        if (schemaTable == null) { return GetColumnNames(tablename); }

        foreach (DataRow colRow in schemaTable.Rows) {
            columns.Add(colRow.Field<string>("ColumnName").ToUpper());
        }

        _ = CloseConnection();
        return columns;
    }

    public override SqlBackAbstract OtherTable(string tablename) => new SQLBackMicrosoftCE(this, tablename);

    public override string VarChar(int lenght) => "VARCHAR(" + lenght + ")";

    //protected override string CreateTable(string tablename, bool allowSystemTableNames) {
    //    var b = DeleteTable(tablename, allowSystemTableNames);
    //    if (!string.IsNullOrEmpty(b)) { return b; }

    //    return ExecuteCommand(@"CREATE TABLE " + tablename + "(RK " + ColumnPropertyPrimary + " NOT NULL PRIMARY KEY)", true);
    //}

    protected override string CreateTable(string tablename, List<string> keycolumns, bool allowSystemTableNames) {
        if (!IsValidTableName(tablename, allowSystemTableNames)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Tabellenname ungültig: " + tablename);

            return "Tabellenname ungültig:" + tablename;
        }

        var b = DeleteTable(tablename, allowSystemTableNames);
        if (!string.IsNullOrEmpty(b)) { return b; }

        // http://www.sql-server-helper.com/error-messages/msg-8110.aspx

        var t = @"CREATE TABLE " + tablename;

        if (keycolumns.Count > 0) {
            t = t + "(";

            foreach (var thiskey in keycolumns) {
                t += thiskey.ToUpper() + " VARCHAR(255) default '' NOT NULL, ";
            }
            t = t.TrimEnd(", ");
            t += ")";
        }

        b = ExecuteCommand(t, true);

        if (!string.IsNullOrEmpty(b)) { return b; }

        if (keycolumns.Count > 0) {
            t = "ALTER TABLE " + tablename + " ADD CONSTRAINT PK_" + tablename.ToUpper() + " PRIMARY KEY CLUSTERED(" + keycolumns.JoinWith(", ").ToUpper() + ")";

            return ExecuteCommand(t, true);
        }
        return string.Empty;
    }

    protected override string DeleteTable(string tablename, bool allowSystemTableNames) {
        if (!IsValidTableName(tablename, allowSystemTableNames)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Tabellenname ungültig: " + tablename);

            return "Tabellenname ungültig:" + tablename;
        }

        return ExecuteCommand("DROP TABLE IF EXISTS " + tablename, false);
    }

    #endregion
}