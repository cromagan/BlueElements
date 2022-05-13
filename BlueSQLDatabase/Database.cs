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
using BlueBasics;
using BlueBasics.DatabaseInterfaces;
using System.IO;
using System.Globalization;

namespace BlueSQLDatabase {

    //https://docs.microsoft.com/en-us/troubleshoot/developer/visualstudio/csharp/language-compilers/create-sql-server-database-programmatically
    public class Database : IDatabase {

        #region Fields

        private SqlConnection _connection;
        private string _filename;

        #endregion

        #region Constructors

        private Database(string filename, bool readOnly, bool create) {
            CultureInfo culture = new("de-DE");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            ReadOnly = readOnly;

            if (create && !File.Exists(filename)) { CreateDatabase(filename); }

            //string connetionString;
            //SqlConnection cnn;
            //connetionString = @"Data Source=WIN-50GP30FGO75;Initial Catalog=Demodb;User ID=sa;Password=demol23";
            //_connection = new SqlConnection(connetionString);
            //_connection.Open();
            //MessageBox.Show("Connection Open  !");
            //cnn.Close();

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

        public bool ReadOnly { get; private set; }

        #endregion

        #region Methods

        public bool CreateDatabase(string filename) {
            var myConn = new SqlConnection("Server=localhost;Integrated security=SSPI;database=master");

            var str = "CREATE DATABASE MyDatabase ON PRIMARY " +
             "(NAME = MyDatabase_Data, " +
             "FILENAME = '" + filename + "', " +
             "SIZE = 2MB, MAXSIZE = 512MB, FILEGROWTH = 1MB)";
            //"LOG ON (NAME = MyDatabase_Log, " +
            //"FILENAME = 'C:\\MyDatabaseLog.ldf', " +
            //"SIZE = 1MB, " +
            //"MAXSIZE = 5MB, " +
            //"FILEGROWTH = 10%)";

            var myCommand = new SqlCommand(str, myConn);
            var ok = false;
            try {
                myConn.Open();
                myCommand.ExecuteNonQuery();
                //MessageBox.Show("DataBase is Created Successfully", "MyProgram", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ok = true;
            } catch (System.Exception ex) {
                //MessageBox.Show(ex.ToString(), "MyProgram", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } finally {
                if (myConn.State == ConnectionState.Open) {
                    myConn.Close();
                }
            }
            return ok;
        }

        #endregion
    }
}