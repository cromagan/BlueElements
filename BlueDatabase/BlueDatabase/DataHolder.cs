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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;


namespace BlueDatabase {
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class DataHolder {

        public readonly Database InternalDatabase = null;


        public readonly DataHolder Parent = null;
        public readonly string Typ = string.Empty;
        public readonly string ID = string.Empty;

        public abstract string MyDefaultFileName();
        public abstract string MyDefaultSubTyp();

        public abstract bool UseExtraFile();

        //private List<(ColumnItem, ListExt<object>)> syncs = new List<(ColumnItem, ListExt<object>)>();



        //public void Sync<t>(string columnname, ListExt<object> data) {
        //    var c = Column(columnname, string.Empty);

        //    syncs.Add((c, data));

        //    data.Changed += Data_Changed;

        //}


        public void SetData(string feldname, bool editable, string quickinfo) {
            ColumnItem c = Column(feldname, string.Empty);

            //c.Caption = dataName;
            //c.Format = enDataFormat.Text;
            //c.MultiLine = true;
            c.TextBearbeitungErlaubt = editable;
            if (editable) {
                c.PermissionGroups_ChangeCell.AddIfNotExists("#Everybody");
            } else {
                c.PermissionGroups_ChangeCell.Remove("#Everybody");
            }
            c.Quickinfo = quickinfo;



        }


        /// <summary>
        /// Speichert die Daten in dem Parent ab
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="id"></param>
        public DataHolder(DataHolder parent, string id) {

            if (UseExtraFile()) {
                Develop.DebugPrint(enFehlerArt.Fehler, "Falsche Routine!");
            }

            InternalDatabase = parent.InternalDatabase;
            Typ = parent.Row().CellFirstString() + "/" + MyDefaultSubTyp();
            ID = id;
        }

        /// <summary>
        /// Erzeugt eine eigenständige Datei. Der Typ wird dabei immer auf MAIN gesetzt
        /// </summary>
        /// <param name="id"></param>
        public DataHolder(string id) {

            if (!UseExtraFile()) {
                Develop.DebugPrint(enFehlerArt.Fehler, "Falsche Routine!");
            }

            ID = id;
            Typ = "MAIN";

            string filename = MyDefaultFileName();
            InternalDatabase = (Database)Database.GetByFilename(filename, false);

            Parent = null;


            if (InternalDatabase == null) {
                InternalDatabase = new Database(filename, false, true);

                if (InternalDatabase.Column.Exists("ID") == null) {


                    InternalDatabase.Column.Add("ID", "ID", enDataFormat.Text);
                    InternalDatabase.RepairAfterParse();
                    InternalDatabase.UndoCount = 50;
                    InternalDatabase.Column.SysCorrect.SaveContent = false;
                    InternalDatabase.Column.SysLocked.SaveContent = false;
                    InternalDatabase.Column.SysRowChangeDate.SaveContent = false;
                    InternalDatabase.Column.SysRowCreateDate.SaveContent = false;
                    InternalDatabase.Column.SysRowCreator.SaveContent = false;
                    InternalDatabase.Column.SysCorrect.ShowUndo = false;
                    InternalDatabase.Column.SysLocked.ShowUndo = false;
                    InternalDatabase.Column.SysRowChangeDate.ShowUndo = false;
                    InternalDatabase.Column.SysRowCreateDate.ShowUndo = false;
                    InternalDatabase.Column.SysRowCreator.ShowUndo = false;

                    InternalDatabase.ColumnArrangements[1].ShowAllColumns(InternalDatabase);
                    InternalDatabase.ColumnArrangements[1].HideSystemColumns();
                    //InternalDatabase.ColumnArrangements[1].Remove(t.ColumnArrangements[1][t.Column["dummy"]]);
                    //InternalDatabase.ColumnArrangements[1].Remove(t.ColumnArrangements[1][t.Column["GebindeMatNr"]]);
                    //InternalDatabase.ColumnArrangements[1].Remove(t.ColumnArrangements[1][t.Column["BehaelterMatNr"]]);

                    //t.ZeilenQuickInfo = "<b><u>Gebinde &Nummer; in Beleg &Beleg;</b></u><br>" +
                    //                    "<b>Nummer: </b> &GebindeMatNr; <br> " +
                    //                    "<b>Art: </b> &Art; &Formation; (&Laenge; x &Breite; mm) H= &Hoehe; mm<br><br>" +
                    //                    "<b><u>Zugehöriger Behälter:</b></u><br>" +
                    //                    "<b>Nummer: </b> &BehaelterMatNr; <br> " +
                    //                    "<b>Volumen: </b> &Volumen;";

                    InternalDatabase.SortDefinition = new RowSortDefinition(InternalDatabase, new List<string>() { "ID" }, false);

                }

            }
            InternalDatabase.Caption = id;
            InternalDatabase.ReloadDelaySecond = 240;
            InternalDatabase.AutoDeleteBAK = true;
        }

        public void Save(bool mustsave) {
            InternalDatabase.Save(mustsave);
        }

        public static string ColumnName(string originalName) {

            originalName = originalName.ToUpper().StarkeVereinfachung(" -_\\*/()");

            originalName = originalName.Replace(" ", "_");
            originalName = originalName.Replace("-", "_");
            originalName = originalName.Replace("(", "_");
            originalName = originalName.Replace(")", "_");
            originalName = originalName.Replace("\\", "_");
            originalName = originalName.Replace("/", "_");
            originalName = originalName.Replace("__", "_");
            originalName = originalName.Replace("*", "_X_");

            return originalName;


        }


        public ColumnItem Column(string dataName, string message) {

            string nd = ColumnName(dataName);



            ColumnItem c = InternalDatabase.Column.Exists(nd);
            if (c == null) {
                c = InternalDatabase.Column.Add(nd);
                c.Caption = dataName;
                c.Format = enDataFormat.Text;
                c.MultiLine = true;
                c.TextBearbeitungErlaubt = true;
                c.PermissionGroups_ChangeCell.Add("#Everybody");
                c.Ueberschrift1 = Typ;


                if (!string.IsNullOrEmpty(message)) {
                    c.Caption = "!!!" + c.Caption;
                    c.Quickinfo = message;
                    Develop.DebugPrint(enFehlerArt.Warnung, "Erzeugungsfehler: " + message);
                }

                InternalDatabase.ColumnArrangements[1].ShowAllColumns(InternalDatabase);
                InternalDatabase.ColumnArrangements[1].HideSystemColumns();


            }

            return c;
        }

        public RowItem Row() {
            string rn = Typ + "/" + ID;

            RowItem r = InternalDatabase.Row[rn];

            if (r == null) {
                r = InternalDatabase.Row.Add(rn);
                r.CellSet(InternalDatabase.Column.SysChapter, Typ);
            }

            return r;
        }

        public void Set(string dataName, string value) {
            Row().CellSet(Column(dataName, string.Empty), value);
        }

        public void Set(string dataName, List<string> value) {
            Row().CellSet(Column(dataName, string.Empty), value);
        }

        public void SetSynchronizedFiles<t>(string dataName, string value, ref t synchronData) where t : DataHolder {
            Row().CellSet(Column(dataName, string.Empty), value);


            if (synchronData == null || synchronData.ID.ToUpper() != value.ToUpper()) {

                synchronData = (t)System.Activator.CreateInstance(typeof(t), value);
            }
        }


        //public void SetSynchronizedFiles<t>(string dataName, List<string> value, List<t> synchronData) where t : DataHolder {
        //    Row().CellSet(Column(dataName, string.Empty), value);

        //    var oldl = new List<t>();
        //    oldl.AddRange(synchronData);
        //    synchronData.Clear();

        //    foreach (var thisID in value) {
        //        var v = oldl.GetByID(thisID);

        //        if (v == null) {
        //            //var obj = new object[2];
        //            //obj[0] = filename + thisc + ".mdd";
        //            //obj[1] = dataName + "-Data";

        //            // (string filename, string id) {
        //            v = (t)System.Activator.CreateInstance(typeof(t), thisID);
        //        }
        //        synchronData.Add(v);

        //    }
        //}




        public void Register<t>(out ListExt<t> data, bool separateFiles) where t : DataHolder {

            data = new ListExt<t>();

            string name = data.GetType().ToString();

            List<string> IDS = GetList(name);

            foreach (string thisID in IDS) {
                if (!separateFiles) {

                    t v = (t)System.Activator.CreateInstance(typeof(t), this, thisID.ToUpper());
                    data.Add(v);
                } else {
                    t v = (t)System.Activator.CreateInstance(typeof(t), thisID.ToUpper());
                    data.Add(v);
                }

            }

            data.Changed += Data_Changed;

        }

        private void Data_Changed(object sender, System.EventArgs e) {

            List<string> IDS = new List<string>();
            if (sender is IEnumerable enumerable) {

                foreach (object thisd in enumerable) {
                    if (thisd is DataHolder dh) {
                        IDS.Add(dh.ID);
                    }
                }
            } else {
                Develop.DebugPrint(enFehlerArt.Fehler, "Falscher Typ");
            }

            string name = sender.GetType().ToString();
            Set(name, IDS);
        }








        ///// <summary>
        ///// Setz die Synchronen Daten in dieser Datei mit ab
        ///// </summary>
        ///// <typeparam name="t"></typeparam>
        ///// <param name="dataName"></param>
        ///// <param name="value"></param>
        ///// <param name="synchronData"></param>
        //public void SetSynchronized<t>(string dataName, List<string> value, List<t> synchronData) where t : DataHolder {

        //    var oldSync = new List<t>();
        //    oldSync.AddRange(synchronData);

        //    #region Alte Werte aus der Datenbank löschen
        //    var oldval = GetList(dataName);
        //    oldval.RemoveRange(value);

        //    foreach (var toremove in oldval) {
        //        var rem = oldSync.GetByID(toremove);
        //        var r = rem.Row(); //ja, ein bisschen blöd, evtl. wir die Zeile ersr hiermit erzeugt
        //        InternalDatabase.Row.Remove(r);
        //    }

        //    #endregion

        //    Row().CellSet(Column(dataName, string.Empty), value);

        //    #region die Daten Synchron halten
        //    synchronData.Clear();

        //    foreach (var thisID in value) {
        //        var v = oldSync.GetByID(thisID);

        //        if (v == null) {
        //            //var obj = new object[2];
        //            //obj[0] = this;
        //            //obj[1] = dataName + "-Data";
        //            //obj[2] = thisc.ToUpper();

        //            v = (t)System.Activator.CreateInstance(typeof(t), this, thisID.ToUpper()); ;
        //        }
        //        synchronData.Add(v);
        //    }
        //    #endregion
        //}



        public void Set(string dataName, decimal value) {
            Row().CellSet(Column(dataName, string.Empty), value);
        }

        public void Set(string dataName, double value) {
            Row().CellSet(Column(dataName, string.Empty), value);
        }

        public void Set(string dataName, int value) {
            Row().CellSet(Column(dataName, string.Empty), value);
        }



        public string GetString(string dataName) {
            return Row().CellGetString(Column(dataName, string.Empty));
        }

        public List<string> GetList(string dataName) {
            return Row().CellGetList(Column(dataName, string.Empty));
        }


        public decimal GetDecimal(string dataName) {
            return Row().CellGetDecimal(Column(dataName, string.Empty));
        }


        public double GetDouble(string dataName) {
            return Row().CellGetDouble(Column(dataName, string.Empty));
        }

        public int GetInt(string dataName) {
            return Row().CellGetInteger(Column(dataName, string.Empty));
        }




        public string Erstelldatum {
            get => InternalDatabase.CreateDate;
            set { InternalDatabase.CreateDate = value; }
        }



        public string Ersteller {
            get => InternalDatabase.Creator;
            set { InternalDatabase.Creator = value; }
        }




    }


    public static class DataHolderExtensions {
        public static t GetByID<t>(this List<t> items, string id) where t : DataHolder {
            foreach (t thisit in items) {
                if (thisit.ID.ToUpper() == id.ToUpper()) { return thisit; }
            }
            return null;

        }

    }


}
