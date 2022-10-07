﻿// Authors:
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
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace BlueDatabase;

//https://www.c-sharpcorner.com/article/create-a-sql-server-database-dynamically-in-C-Sharp/
//https://www.ictdemy.com/csharp/databases/introduction-to-databases-in-csharp-net
//https://docs.microsoft.com/en-us/troubleshoot/developer/visualstudio/csharp/language-compilers/create-sql-server-database-programmatically
public class SQLBackMicrosoftCE : SQLBackAbstract {

    #region Constructors

    public SQLBackMicrosoftCE(SQLBackMicrosoftCE sql, string tablename) : base() {
        _connection = sql._connection;
        RepairAll(tablename.ToUpper());
    }

    public SQLBackMicrosoftCE(string filename, bool create, string tablename) : base() {
        if (create && !File.Exists(filename)) { CreateDatabase(filename); }
        _connection = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + filename + ";Integrated Security=True;Trusted_Connection=Yes;");

        RepairAll(tablename.ToUpper());
    }

    #endregion

    #region Properties

    public override string Primary => "bigint identity(1,1) NOT NULL";
    public override string VarChar255 => "VARCHAR(255)";
    public override string VarChar4000 => "VARCHAR(4000)";

    #endregion

    #region Methods

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
        var dbn = filename.FileNameWithoutSuffix().Replace(" ", "_");
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

    public override List<string> ListTables() {
        List<string> tables = new();

        OpenConnection();

        var dt = _connection.GetSchema("Tables");
        foreach (DataRow row in dt.Rows) {
            var tablename = (string)row[2];
            tables.Add(tablename);
        }

        CloseConnection();

        return tables;
    }

    public override SQLBackAbstract OtherTable(string tablename) {
        return new SQLBackMicrosoftCE(this, tablename);
    }

    protected override bool CreateTable(string name) {
        if (!ExecuteCommand("DROP TABLE IF EXISTS " + name)) { return false; }
        return ExecuteCommand(@"CREATE TABLE " + name + "(RK " + Primary + " NOT NULL PRIMARY KEY)");
    }

    protected override bool CreateTable(string name, List<string> keycolumns) {
        if (!ExecuteCommand("DROP TABLE IF EXISTS " + name)) { return false; }
        // http://www.sql-server-helper.com/error-messages/msg-8110.aspx

        var t = @"CREATE TABLE " + name + "(";

        foreach (var thiskey in keycolumns) {
            t += thiskey.ToUpper() + " VARCHAR(255) default '' NOT NULL, ";
        }
        t = t.TrimEnd(", ");
        t += ")";

        if (!ExecuteCommand(t)) { return false; }
        //if (!ExecuteCommand("SET IDENTITY_INSERT " + name + " ON")) { return false; }

        t = "ALTER TABLE " + name + " ADD CONSTRAINT (PK_" + name.ToUpper() + " PRIMARY KEY CLUSTERED(" + keycolumns.JoinWith(", ").ToUpper() + "))";

        return ExecuteCommand(t);
    }

    #endregion
}