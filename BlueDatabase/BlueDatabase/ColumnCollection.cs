#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2019 Christian Peter
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

using System;
using System.Collections.Generic;
using BlueBasics;
using BlueDatabase.Enums;
using static BlueBasics.FileOperations;
using BlueBasics.Enums;

namespace BlueDatabase
{
    public sealed class ColumnCollection : ListExt<ColumnItem>
    {



        #region  Variablen-Deklarationen 

        public readonly Database Database;

        private int _LastColumnKey;


        #endregion


        #region  Event-Deklarationen + Delegaten 

        #endregion


        #region  Construktor + Initialize 

        public void Initialize()
        {
            _LastColumnKey = 0;
            Clear();
            ResetSystems();
        }

        public ColumnCollection(Database cDatabase)
        {
            Database = cDatabase;
            Initialize();
        }

        #endregion


        #region  Properties 

        public new ColumnItem this[int Index]
        {
            get
            {
                if (Database == null) { return null; }

                if (Index < 0 || Index >= Count) { return null; }
                return base[Index];
            }
        }



        public ColumnItem SearchByKey(int Key)
        {
            if (Database == null) { return null; }
            if (Key < 0) { return null; } // Evtl. Gelöschte Spalte in irgendeiner Order

            foreach (var ThisColumn in this)
            {
                if (ThisColumn != null && ThisColumn.Key == Key) { return ThisColumn; }
            }

            return null;
        }


        public ColumnItem this[string ColumnName]
        {
            get
            {
                if (Database == null) { return null; }
                if (string.IsNullOrEmpty(ColumnName)) { return null; }// Evtl. Gelöschte Spalte in irgendeiner Order

                ColumnName = ColumnName.ToUpper();

                foreach (var ThisColumn in this)
                {
                    if (ThisColumn != null && ThisColumn.Name == ColumnName) { return ThisColumn; }
                }

                return null;
            }
        }


        #endregion

        public void ResetSystems()
        {
            SysLocked = null;
            SysRowCreateDate = null;
            SysRowCreator = null;
            SysCorrect = null;
            SysRowChanger = null;
            SysRowChangeDate = null;
            SysChapter = null;
        }


        internal string Load_310(enDatabaseDataType Art, string Wert)
        {

            switch (Art)
            {


                case enDatabaseDataType.LastColumnKey:
                    _LastColumnKey = int.Parse(Wert);
                    break;

                default:

                    if (Art.ToString() == Convert.ToInt32(Art).ToString())
                    {
                        Develop.DebugPrint(enFehlerArt.Info, "Laden von Datentyp '" + Art + "' nicht definiert.<br>Wert: " + Wert + "<br>Datei: " + Database.Filename);
                    }
                    else
                    {
                        return "Interner Fehler: Für den Datentyp  '" + Art + "'  wurde keine Laderegel definiert.";
                    }
                    break;
            }

            return "";
        }



        public ColumnItem SysChapter { get; private set; }


        public ColumnItem SysCorrect { get; private set; }


        public ColumnItem SysLocked { get; private set; }



        public ColumnItem SysRowChangeDate { get; private set; }



        public ColumnItem SysRowChanger { get; private set; }

        public ColumnItem SysRowCreateDate { get; private set; }


        public ColumnItem SysRowCreator { get; private set; }




        public void Swap(ColumnItem Column1, ColumnItem Column2)
        {

            if (Column1 == Column2 || Column1 == null || Column2 == null) { return; }

            var nr1 = IndexOf(Column1);
            var nr2 = IndexOf(Column2);

            base.Swap(nr1, nr2);

            Column1.Invalidate_ColumAndContent();
            Column2.Invalidate_ColumAndContent();

            Database.ColumnArrangements[0].ShowAllColumns(Database); // Damit die Datenbank mitbekommt, das sich da was geändert hat
        }


        private void AddSystems(string Kennung)
        {

            foreach (var ThisColumn in this)
            {
                if (ThisColumn != null && ThisColumn.Identifier.ToUpper() == Kennung.ToUpper()) { return; }
            }

            var c = new ColumnItem(Database, true);
            c.Load(enDatabaseDataType.co_Identifier, Kennung);

        }


        internal void SaveToByteList(List<byte> List)
        {

            Database.SaveToByteList(List, enDatabaseDataType.LastColumnKey, _LastColumnKey.ToString());


            for (var ColumnCount = 0 ; ColumnCount < Count ; ColumnCount++)
            {
                if (this[ColumnCount] != null && !string.IsNullOrEmpty(this[ColumnCount].Name))
                {
                    this[ColumnCount].SaveToByteList(ref List);
                }
            }
        }


        public void GetSystems()
        {

            ResetSystems();

            foreach (var ThisColumnItem in this)
            {
                if (ThisColumnItem != null)
                {
                    switch (ThisColumnItem.Identifier)
                    {
                        case "": break;
                        case "System: Locked": SysLocked = ThisColumnItem; break;
                        case "System: Creator": SysRowCreator = ThisColumnItem; break;
                        case "System: Changer": SysRowChanger = ThisColumnItem; break;
                        case "System: Date Created": SysRowCreateDate = ThisColumnItem; break;
                        case "System: Correct": SysCorrect = ThisColumnItem; break;
                        case "System: Date Changed": SysRowChangeDate = ThisColumnItem; break;
                        case "System: Chapter": SysChapter = ThisColumnItem; break;
                        default:
                            Develop.DebugPrint(enFehlerArt.Fehler, "Unbekannte Kennung: " + ThisColumnItem.Identifier);
                            break;
                    }


                }
            }

        }


        public void Repair()
        {
            var ColN = 0;
            var w = new List<string>
            {
                "System: Chapter",
                "System: Date Changed",
                "System: Changer",
                "System: Date Created",
                "System: Creator",
                "System: Correct",
                "System: Locked"
            };


            // Die Letzte ID ermitteln,falls der gleadene Wert fehlerhaft ist
            foreach (var ThisColumnItem in this)
            {

                if (ThisColumnItem != null)
                {
                    _LastColumnKey = Math.Max(_LastColumnKey, ThisColumnItem.Key);
                }
            }


            foreach (var thisstring in w)
            {
                AddSystems(thisstring);
            }

            GetSystems();


            for (var s1 = 0 ; s1 < Count ; s1++)
            {
                if (base[s1] != null)
                {

                    for (var s2 = s1 + 1 ; s2 < Count ; s2++)
                    {
                        if (base[s2] != null)
                        {

                            // Evtl. Doppelte Namen einzigartig machen
                            if (base[s1].Name.ToUpper() == base[s2].Name.ToUpper())
                            {
                                base[s2].Load(enDatabaseDataType.co_Name, base[s2].Name + "0");
                            }

                            // Evtl. Doppelte Identifier eleminieren
                            if (!string.IsNullOrEmpty(base[s1].Identifier) && base[s1].Identifier.ToUpper() == base[s2].Identifier.ToUpper())
                            {
                                base[s2].Load(enDatabaseDataType.co_Identifier, string.Empty);
                            }

                        }
                    }

                    //base[s1].Repair();

                }
            }


            // Reihengolge reparieren
            ColN = -1;
            do
            {
                ColN += 1;

                if (ColN + 2 > Count) { break; }

                if (this[ColN] == null)
                {
                    base.Swap(ColN, ColN + 1);
                    ColN = -1;
                }
                else if (this[ColN + 1] == null)
                {
                    // Dummy, um nachfoldgnd nicht abfragen zu müssen

                }
                else if (!string.IsNullOrEmpty(this[ColN].Identifier) && string.IsNullOrEmpty(this[ColN + 1].Identifier))
                {
                    base.Swap(ColN, ColN + 1);
                    ColN = -1;

                }
                else if (!string.IsNullOrEmpty(this[ColN].Identifier) && !string.IsNullOrEmpty(this[ColN + 1].Identifier))
                {

                    if (w.IndexOf(this[ColN].Identifier) > w.IndexOf(this[ColN + 1].Identifier))
                    {
                        base.Swap(ColN, ColN + 1);
                        ColN = -1;
                    }
                }
            } while (true);
        }


        internal int NextColumnKey()
        {

            do
            {
                if (_LastColumnKey == int.MaxValue) { _LastColumnKey = 0; }
                _LastColumnKey += 1;
                if (SearchByKey(_LastColumnKey) == null) { return _LastColumnKey; }
            } while (true);
        }


        internal static string ParsableColumnKey(ColumnItem Column)
        {
            if (Column == null) { return "ColumnKey=?"; }
            return ParsableColumnKey(Column.Key);
        }


        internal static string ParsableColumnKey(int Key)
        {
            return "ColumnKey=" + Key;
        }



        public static string ChangeKeysInString(string OriginalString, int OldKey, int NewKey)
        {

            var o = ParsableColumnKey(OldKey);
            if (!OriginalString.Contains(o)) { return OriginalString; }

            var n = ParsableColumnKey(NewKey);


            if (OldKey == NewKey)
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "Schlüssel gleich:  " + OldKey);
                return OriginalString;
            }


            OriginalString = OriginalString.Replace(o + "}", n + "}");
            OriginalString = OriginalString.Replace(o + ",", n + ",");
            OriginalString = OriginalString.Replace(o + " ", n + " ");

            if (OriginalString.EndsWith(o)) { OriginalString = OriginalString.TrimEnd(o) + n; }


            if (OriginalString.Contains(o))
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "String nicht ersetzt: " + OriginalString);
                return OriginalString;
            }

            return OriginalString;

        }

        internal string ChangeKeysToNames(string OriginalString)
        {

            foreach (var ThisColumn in this)
            {
                if (ThisColumn != null)
                {

                    OriginalString = OriginalString.Replace(ParsableColumnKey(ThisColumn) + ",", "ColumnName=" + ThisColumn.Name.ToNonCritical() + ",");
                    OriginalString = OriginalString.Replace(ParsableColumnKey(ThisColumn) + "}", "ColumnName=" + ThisColumn.Name.ToNonCritical() + "}");
                    OriginalString = OriginalString.Replace(ParsableColumnKey(ThisColumn) + " ", "ColumnName=" + ThisColumn.Name.ToNonCritical() + " ");
                }
            }

            return OriginalString;

        }

        public void GenerateOverView()
        {
            var da = new List<string>();
            modAllgemein.HTML_AddHead(da, Database.Filename.FileNameWithoutSuffix());
            da.Add("  <Font face=\"Arial\" Size=\"7\">Spaltenliste von: " + Database.Caption + "</h1><br>");
            da.Add("  <Font face=\"Arial\" Size=\"4\">" + Database.Filename + "</h1><br>");

            da.Add("  <Font face=\"Arial\" Size=\"2\"><table border=\"1\" BORDERCOLOR=\"#aaaaaa\" cellspacing=\"0\" cellpadding=\"0\" align=\"left\">");

            da.Add("       <tr bgcolor=\"#dddddd\"BORDERCOLOR=\"#cccccc\" > ");
            da.Add("              <th align=left width =\"1%\">Name</th>");
            da.Add("              <th align=left>Beschriftung</th>");
            da.Add("              <th align=left>Datenformat</th>");
            da.Add("              <th align=left>Quickinfo</th>");
            da.Add("              <th align=left>Admin-Info</th>");
            da.Add("              <th align=left>Tags</th>");
            da.Add("              <th align=left>Änderungs-Rechte</th>");
            da.Add("      </tr>");



            foreach (var ThisColumnItem in Database.Column)
            {
                if (ThisColumnItem != null)
                {

                    da.Add("       <tr BORDERCOLOR=\"#cccccc\">");
                    da.Add("              <th align=left>" + ThisColumnItem.Name + "</th>");
                    da.Add("              <th align=left>" + ThisColumnItem.Caption.Replace("\r", "<br>") + "</th>");
                    da.Add("              <th align=left>" + ThisColumnItem.Format + "</th>");
                    da.Add("              <th align=left>" + ThisColumnItem.Quickinfo.Replace("\r", "<br>") + "</th>");
                    da.Add("              <th align=left>" + ThisColumnItem.AdminInfo.Replace("\r", "<br>") + "</th>");
                    da.Add("              <th align=left>" + ThisColumnItem.Tags.JoinWith("<br>") + "</th>");
                    da.Add("              <th align=left>" + ThisColumnItem.PermissionGroups_ChangeCell.JoinWith("<br>") + "</th>");
                    da.Add("      </tr>");
                }
            }

            da.Add("    </table>");
            modAllgemein.HTML_AddFoot(da);

            da.Save(TempFile("", "Spaltenliste.html"), true);
        }







        public string Freename(string Wunschname)
        {
            var nr = 0;

            Wunschname = Wunschname.ReduceToChars(ColumnItem.AllowedCharsInternalName);
            if (string.IsNullOrEmpty(Wunschname)) { Wunschname = "NewColumn"; }

            if (this[Wunschname] == null) { return Wunschname; }
            string TestName = null;

            do
            {
                nr += 1;
                TestName = Wunschname + "_" + nr;
            } while (this[TestName] != null);

            return TestName;
        }
    }
}