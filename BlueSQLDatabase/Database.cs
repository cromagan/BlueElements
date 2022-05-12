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

namespace BlueSQLDatabase {

    //https://docs.microsoft.com/en-us/troubleshoot/developer/visualstudio/csharp/language-compilers/create-sql-server-database-programmatically
    public class Database {

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