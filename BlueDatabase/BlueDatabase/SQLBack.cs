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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
using BlueBasics;
using System.IO;
using System.Globalization;
using System.Windows.Forms;
using BlueDatabase.Enums;
using System.Drawing;

namespace BlueDatabase;
//https://www.c-sharpcorner.com/article/create-a-sql-server-database-dynamically-in-C-Sharp/
//https://www.ictdemy.com/csharp/databases/introduction-to-databases-in-csharp-net

//https://docs.microsoft.com/en-us/troubleshoot/developer/visualstudio/csharp/language-compilers/create-sql-server-database-programmatically
public class SQLBack {

    #region Fields

    private SqlConnection _connection;
    private string _filename;

    #endregion

    #region Constructors

    public SQLBack(string filename, bool create) {
        Develop.StartService();
        //CultureInfo culture = new("de-DE");
        //CultureInfo.DefaultThreadCurrentCulture = culture;
        //CultureInfo.DefaultThreadCurrentUICulture = culture;

        if (create && !File.Exists(filename)) { CreateDatabase(filename); }

        _connection = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + filename + ";Integrated Security=True;Trusted_Connection=Yes;");

        _connection.Open();

        var x = ListTables();
        if (!x.Contains("Main")) { CreateTable("Main"); }
        if (!x.Contains("Style")) { CreateTable("Style", new List<string>() { "DBNAME", "TYPE" }); }
        if (!x.Contains("Undo")) { CreateTable("Undo"); }

        // DatabaseDataType comand, long columnKey, long rowKey, string previousValue, string changedTo, string userName
        var colUndo = GetColumnNames("Undo");
        if (!colUndo.Contains("DBNAME")) { AddColumn("Undo", "DBNAME"); }
        if (!colUndo.Contains("COMAND")) { AddColumn("Undo", "COMAND"); }
        if (!colUndo.Contains("COLUMNKEY")) { AddColumn("Undo", "COLUMNKEY"); }
        if (!colUndo.Contains("ROWKEY")) { AddColumn("Undo", "ROWKEY"); }
        if (!colUndo.Contains("PREVIOUSVALUE")) { AddColumn("Undo", "PREVIOUSVALUE"); }
        if (!colUndo.Contains("CHANGEDTO")) { AddColumn("Undo", "CHANGEDTO"); }
        if (!colUndo.Contains("USERNAME")) { AddColumn("Undo", "USERNAME"); }
        if (!colUndo.Contains("DATETIMEUTC")) { AddColumn("Undo", "DATETIMEUTC"); }
        _connection.Close();

        //Cell = new CellCollection(this);

        //Row = new RowCollection(this);
        //Row.RowRemoving += Row_RowRemoving;
        //Row.RowAdded += Row_RowAdded;

        //Column = new ColumnCollection(this);
        //Column.ItemRemoving += Column_ItemRemoving;
        //Column.ItemRemoved += Column_ItemRemoved;
        //Column.ItemAdded += Column_ItemAdded;

        //Works = new ListExt<WorkItem>();
        //_filesAfterLoadingLCase = new List<string>();
        //ColumnArrangements.Changed += ColumnArrangements_ListOrItemChanged;
        //Layouts.Changed += Layouts_ListOrItemChanged;
        //Layouts.ItemSeted += Layouts_ItemSeted;
        //Views.Changed += Views_ListOrItemChanged;
        //PermissionGroupsNewRow.Changed += PermissionGroups_NewRow_ListOrItemChanged;
        //Tags.Changed += DatabaseTags_ListOrItemChanged;
        //Export.Changed += Export_ListOrItemChanged;
        //DatenbankAdmin.Changed += DatabaseAdmin_ListOrItemChanged;

        //Initialize();

        //UserGroup = "#Administrator";
        //if (!string.IsNullOrEmpty(filename)) {
        //    //DropConstructorMessage?.Invoke(this, new MessageEventArgs(enFehlerArt.Info, "Lade Datenbank aus Dateisystem: \r\n" + filename.FileNameWithoutSuffix()));
        //    Load(filename, create);
        //} else if (stream != null) {
        //    LoadFromStream(stream);
        //} else {
        //    RepairAfterParse();
        //}
        //QuickImage.NeedImage += QuickImage_NeedImage;
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

    public void Checkin(DatabaseDataType comand, long columnKey, long rowKey, string previousValue, string changedTo, string userName) {
        throw new NotImplementedException();
    }

    public bool CreateDatabase(string filename) {
        //var myConn = new SqlConnection("Server=localhost;Integrated security=SSPI;database=master");
        // var myConn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\Database1.mdf;Integrated Security=True");

        //var myConn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;Integrated Security=True");

        //var myConn = new SqlConnection("Integrated Security=SSPI;Initial Catalog=;Data Source=localhost;");

        // var myConn = new SqlConnection("Server=(local)\\netsdk;uid=sa;pwd=;database=master");

        var myConn = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;Integrated security=SSPI;database=master");
        var dbn = filename.FileNameWithoutSuffix();

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
        } catch (System.Exception ex) {
            MessageBox.Show(ex.ToString(), "MyProgram", MessageBoxButtons.OK, MessageBoxIcon.Information);
        } finally {
            if (myConn.State == ConnectionState.Open) {
                myConn.Close();
            }
        }
        return ok;
    }

    public List<string> GetColumnNames(string tableName) {
        List<string> columns = new List<string>();

        using var com = new SqlCommand(@"SELECT * FROM " + tableName, _connection);
        using var reader = com.ExecuteReader(CommandBehavior.SchemaOnly);
        //listBoxColumns.Items.Clear();
        DataTable schemaTable = reader.GetSchemaTable();
        foreach (DataRow colRow in schemaTable.Rows) {
            columns.Add(colRow.Field<String>("ColumnName"));
        }

        ////string strConnect = ".........";
        ////using (SqlConnection con = new SqlConnection(strConnect)) {
        ////    con.Open();
        //using var com = new SqlCommand(@"SELECT COLUMN_NAME
        //                         FROM INFORMATION_SCHEMA.COLUMNS
        //                         WHERE TABLE_NAME = @yourtablename", _connection);
        //com.Parameters.AddWithValue("@yourtableName", tableName);
        //using var reader = com.ExecuteReader();

        //var schemaTable = reader.GetSchemaTable();
        //foreach (DataRow colRow in schemaTable.Rows)
        //    listBoxColumns.Items.Add(colRow.Field<String>("ColumnName"));

        //var tmp = reader["COLUMN_NAME"];

        //columns.Add(tmp.ToString());

        //}
        return columns;
    }

    public List<string> ListTables() {
        List<string> tables = new();
        //_connection.Open();
        DataTable dt = _connection.GetSchema("Tables");
        foreach (DataRow row in dt.Rows) {
            string tablename = (string)row[2];
            tables.Add(tablename);
        }
        //_connection.Close();
        return tables;
    }

    private void AddColumn(string table, string column) {
        //using (DbConnection connection = new SqlConnection("Your connection string")) {
        //    connection.Open();
        using var command = new SqlCommand("alter table " + table + " add " + column + " VARCHAR(255) default '' NOT NULL", _connection);
        command.ExecuteNonQuery();
        //}
    }

    //public List<string> ListColumns(string oftable) {
    //    try {
    //        var dt = _connection.GetSchema(oftable);

    //        foreach (var thisol in dt.Columns) {
    //            thisol.

    //        }

    //        //using var cmd = new SqlCommand();
    //        //cmd.Connection = _connection;

    //        //   https://zetcode.com/csharp/sqlserver/
    //        //SELECT *
    //        ////  FROM INFORMATION_SCHEMA.COLUMNS
    //        //   WHERE TABLE_NAME = N'Customers'
    //        //cmd.CommandText = "DROP TABLE IF EXISTS " + name;
    //        //cmd.ExecuteNonQuery();

    //        ////cmd.CommandText = @"CREATE TABLE cars(
    //        ////    id int identity(1,1) NOT NULL PRIMARY KEY,
    //        ////    name VARCHAR(255) NOT NULL,
    //        ////    price INT
    //        ////)";

    //        //cmd.CommandText = @"CREATE TABLE " + name + "(" +
    //        //                  "id int identity(1,1) NOT NULL PRIMARY KEY," +
    //        //                  ")";
    //        //cmd.ExecuteNonQuery();
    //        //return true;
    //    } catch {
    //        //return false;
    //    }

    //    //List<string> tables = new();
    //    ////_connection.Open();
    //    //DataTable dt = _connection.GetSchema("Tables");
    //    //foreach (DataRow row in dt.Rows) {
    //    //    string tablename = (string)row[2];
    //    //    tables.Add(tablename);
    //    //}
    //    ////_connection.Close();
    //    //return tables;
    //}

    private bool CreateTable(string name) {
        try {
            using var cmd = new SqlCommand();
            cmd.Connection = _connection;

            //   https://zetcode.com/csharp/sqlserver/

            cmd.CommandText = "DROP TABLE IF EXISTS " + name;
            cmd.ExecuteNonQuery();

            //cmd.CommandText = @"CREATE TABLE cars(
            //    id int identity(1,1) NOT NULL PRIMARY KEY,
            //    name VARCHAR(255) NOT NULL,
            //    price INT
            //)";

            cmd.CommandText = @"CREATE TABLE " + name + "(" +
                              "id int identity(1,1) NOT NULL PRIMARY KEY," +
                              ")";
            cmd.ExecuteNonQuery();
            return true;
        } catch {
            return false;
        }
    }

    private bool CreateTable(string name, List<string> keycolumns) {
        try {
            using var cmd = new SqlCommand();
            cmd.Connection = _connection;

            //   https://zetcode.com/csharp/sqlserver/

            cmd.CommandText = "DROP TABLE IF EXISTS " + name;
            cmd.ExecuteNonQuery();

            cmd.CommandText = @"CREATE TABLE " + name + "(";

            foreach (var thiskey in keycolumns) {
                cmd.CommandText += thiskey + " VARCHAR(255) default '' NOT NULL,";
            }
            cmd.CommandText += ")";

            cmd.ExecuteNonQuery();
            return true;
        } catch {
            return false;
        }
    }

    #endregion
}