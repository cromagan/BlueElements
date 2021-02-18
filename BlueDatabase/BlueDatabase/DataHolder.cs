#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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
#endregion


using BlueBasics;
using BlueBasics.Enums;
using System.Collections.Generic;
using System.ComponentModel;


namespace BlueDatabase {
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class DataHolder {

        readonly Database db = null;


        public readonly DataHolder Parent = null;
        public readonly string Typ = string.Empty;
        public readonly string ID = string.Empty;


        public DataHolder(DataHolder parent, string typ, string id) {
            db = parent.db;
            Typ = typ;
            ID = id;
        }


        public DataHolder(string filename, string id) {

            db = (Database)Database.GetByFilename(filename, false);

            ID = id;
            Typ = "MAIN";
            Parent = null;


            if (db == null) {
                db = new Database(filename, false, true);
                db.Column.Add("ID", "ID", enDataFormat.Text);
                db.RepairAfterParse();
                db.UndoCount = 10;
                db.Column.SysCorrect.SaveContent = false;
                db.Column.SysLocked.SaveContent = false;
                db.Column.SysRowChangeDate.SaveContent = false;
                db.Column.SysRowCreateDate.SaveContent = false;
                db.Column.SysRowCreator.SaveContent = false;
            }

        }


        private ColumnItem Column(string dataName) {

            var nd = (Typ + "*" + dataName).ToUpper().StarkeVereinfachung(" _\\*/");

            nd = nd.Replace(" ", "_");
            nd = nd.Replace("\\", "_");
            nd = nd.Replace("/", "_");
            nd = nd.Replace("__", "_");
            nd = nd.Replace("*", "_X_");




            var c = db.Column.Exists(nd);
            if (c == null) {
                c = db.Column.Add(nd);
                c.Caption = dataName;
                c.Ueberschrift1 = Typ;
            }

            return c;
        }

        private RowItem Row() {
            var rn = Typ + "/" + ID;

            var r = db.Row[rn];

            if (r == null) {
                r = db.Row.Add(rn);
                r.CellSet(db.Column.SysChapter, Typ);
            }

            return r;
        }

        public void Set(string dataName, string value) {
            Row().CellSet(Column(dataName), value);
        }

        public void Set(string dataName, List<string> value) {
            Row().CellSet(Column(dataName), value);
        }


        public void Set(string dataName, decimal value) {
            Row().CellSet(Column(dataName), value);
        }

        public void Set(string dataName, double value) {
            Row().CellSet(Column(dataName), value);
        }

        public void Set(string dataName, int value) {
            Row().CellSet(Column(dataName), value);
        }



        public string GetString(string dataName) {
            return Row().CellGetString(Column(dataName));
        }

        public List<string> GetList(string dataName) {
            return Row().CellGetList(Column(dataName));
        }

        public decimal GetDecimal(string dataName) {
            return Row().CellGetDecimal(Column(dataName));
        }


        public double GetDouble(string dataName) {
            return Row().CellGetDouble(Column(dataName));
        }

        public int GetInt(string dataName) {
            return Row().CellGetInteger(Column(dataName));
        }

    }
}
