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

                    if (Art.ToString() == ((int)Art).ToString())
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

            var c = Database.Column.Add();
            c.Load(enDatabaseDataType.co_Identifier, Kennung);
            c.StandardWerteNachKennung(true);

        }


        internal void SaveToByteList(List<byte> List)
        {

            Database.SaveToByteList(List, enDatabaseDataType.LastColumnKey, _LastColumnKey.ToString());


            for (var ColumnCount = 0; ColumnCount < Count; ColumnCount++)
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
            // Den Wert Am I a Key Column ermitteln
            foreach (var ThisColumnItem in this)
            {

                if (ThisColumnItem != null)
                {
                    _LastColumnKey = Math.Max(_LastColumnKey, ThisColumnItem.Key);
                    ThisColumnItem.CheckIfIAmAKeyColumn();
                }
            }


            foreach (var thisstring in w)
            {
                AddSystems(thisstring);
            }

            GetSystems();


            for (var s1 = 0; s1 < Count; s1++)
            {
                if (base[s1] != null)
                {

                    for (var s2 = s1 + 1; s2 < Count; s2++)
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

        public ColumnItem AddACloneFrom(ColumnItem Source)
        {

            var c = Add(string.Empty);

            c.Caption = Source.Caption;
            c.CaptionBitmap = Source.CaptionBitmap;

            c.Format = Source.Format;
            c.LineLeft = Source.LineLeft;
            c.LineRight = Source.LineRight;
            c.MultiLine = Source.MultiLine;
            c.Quickinfo = Source.Quickinfo;
            c.ForeColor = Source.ForeColor;
            c.BackColor = Source.BackColor;

            c.EditTrotzSperreErlaubt = Source.EditTrotzSperreErlaubt;


            c.EditType = Source.EditType;
            c.Identifier = Source.Identifier;

            c.PermissionGroups_ChangeCell.Clear();
            c.PermissionGroups_ChangeCell.AddRange(Source.PermissionGroups_ChangeCell);

            c.Tags.Clear();
            c.Tags.AddRange(Source.Tags);


            c.AllowedChars = Source.AllowedChars;
            c.AdminInfo = Source.AdminInfo;
            c.AutoFilterErlaubt = Source.AutoFilterErlaubt;
            c.AutofilterTextFilterErlaubt = Source.AutofilterTextFilterErlaubt;
            c.AutoFilterErweitertErlaubt = Source.AutoFilterErweitertErlaubt;
            c.IgnoreAtRowFilter = Source.IgnoreAtRowFilter;
            c.DropdownBearbeitungErlaubt = Source.DropdownBearbeitungErlaubt;
            c.DropdownAllesAbwählenErlaubt = Source.DropdownAllesAbwählenErlaubt;
            c.TextBearbeitungErlaubt = Source.TextBearbeitungErlaubt;
            c.SpellCheckingEnabled = Source.SpellCheckingEnabled;
            c.DropdownWerteAndererZellenAnzeigen = Source.DropdownWerteAndererZellenAnzeigen;
            c.AfterEdit_QuickSortRemoveDouble = Source.AfterEdit_QuickSortRemoveDouble;

            c.AfterEdit_Runden = Source.AfterEdit_Runden;
            c.AfterEdit_DoUCase = Source.AfterEdit_DoUCase;
            c.AfterEdit_AutoCorrect = Source.AfterEdit_AutoCorrect;
            c.AutoRemove = Source.AutoRemove;
            c.SaveContent = Source.SaveContent;
            c.CellInitValue = Source.CellInitValue;
            c.AutoFilterJoker = Source.AutoFilterJoker;
            c.KeyColumnKey = Source.KeyColumnKey;
            c.LinkedCell_RowKey = Source.LinkedCell_RowKey;
            c.LinkedCell_ColumnKey = Source.LinkedCell_ColumnKey;
            c.LinkedCell_ColumnValueFoundIn = Source.LinkedCell_ColumnValueFoundIn;
            c.LinkedCell_ColumnValueAdd = Source.LinkedCell_ColumnValueAdd;
            c.ZellenZusammenfassen = Source.ZellenZusammenfassen;
            c.DropdownKey = Source.DropdownKey;
            c.VorschlagsColumn = Source.VorschlagsColumn;
            c.Align = Source.Align;
            c.SortMask = Source.SortMask;


            c.DropDownItems.Clear();
            c.DropDownItems.AddRange(Source.DropDownItems);

            c.Replacer.Clear();
            c.Replacer.AddRange(Source.Replacer);

            c.Regex.Clear();
            c.Regex.AddRange(Source.Regex);


            c.CompactView = Source.CompactView;
            c.ShowUndo = Source.ShowUndo;
            c.ShowMultiLineInOneLine = Source.ShowMultiLineInOneLine;

            c.Ueberschrift1 = Source.Ueberschrift1;
            c.Ueberschrift2 = Source.Ueberschrift2;
            c.Ueberschrift3 = Source.Ueberschrift3;

            c.Suffix = Source.Suffix;

            c.LinkedKeyKennung = Source.LinkedKeyKennung;
            c.LinkedDatabaseFile = Source.LinkedDatabaseFile;
            c.BildCode_ImageNotFound = Source.BildCode_ImageNotFound;
            c.BildCode_ConstantHeight = Source.BildCode_ConstantHeight;
            c.BestFile_StandardSuffix = Source.BestFile_StandardSuffix;
            c.BestFile_StandardFolder = Source.BestFile_StandardFolder;

            c.Prefix = Source.Prefix;

            return c;


        }

        //internal string ChangeKeysToNames(string OriginalString)
        //{

        //    foreach (var ThisColumn in this)
        //    {
        //        if (ThisColumn != null)
        //        {

        //            OriginalString = OriginalString.Replace(ParsableColumnKey(ThisColumn) + ",", "ColumnName=" + ThisColumn.Name.ToNonCritical() + ",");
        //            OriginalString = OriginalString.Replace(ParsableColumnKey(ThisColumn) + "}", "ColumnName=" + ThisColumn.Name.ToNonCritical() + "}");
        //            OriginalString = OriginalString.Replace(ParsableColumnKey(ThisColumn) + " ", "ColumnName=" + ThisColumn.Name.ToNonCritical() + " ");
        //        }
        //    }

        //    return OriginalString;

        //}

        public void GenerateOverView()
        {
            var da = new HTML(Database.Filename.FileNameWithoutSuffix());
            da.AddCaption("Spaltenliste von: " + Database.Caption);
            da.Add("  <Font face=\"Arial\" Size=\"4\">" + Database.Filename + "</h1><br>");

            da.TableBeginn();
            da.RowBeginn();
            da.CellAdd("#");
            da.CellAdd("Name");
            da.CellAdd("Beschriftung");
            da.CellAdd("Überschriften");
            da.CellAdd("Datenformat");
            da.CellAdd("Quickinfo");
            da.CellAdd("Admin-Info");
            da.CellAdd("Tags");
            da.CellAdd("Änderungs-Rechte");

            da.RowEnd();


            var lfdn = 0;
            foreach (var ThisColumnItem in Database.Column)
            {

                if (ThisColumnItem != null)
                {
                    lfdn += 1;
                    da.RowBeginn();
                    da.CellAdd(lfdn.ToString());
                    da.CellAdd(ThisColumnItem.Name);
                    da.CellAdd(ThisColumnItem.Caption.Replace("\r", "<br>"));
                    da.CellAdd((ThisColumnItem.Ueberschrift1 + "/" + ThisColumnItem.Ueberschrift2 + "/" + ThisColumnItem.Ueberschrift3 + "/").TrimEnd("/"));
                    da.CellAdd(ThisColumnItem.Format.ToString());
                    da.CellAdd(ThisColumnItem.Quickinfo.Replace("\r", "<br>"));
                    da.CellAdd(ThisColumnItem.AdminInfo.Replace("\r", "<br>"));
                    da.CellAdd(ThisColumnItem.Tags.JoinWith("<br>"));
                    da.CellAdd(ThisColumnItem.PermissionGroups_ChangeCell.JoinWith("<br>"));
                    da.RowEnd();
                }
            }

            da.TableEnd();
            da.AddFoot();

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

        [Obsolete]
        public new ColumnItem Add(ColumnItem column)
        {

            Develop.DebugPrint(enFehlerArt.Fehler, "Direkter Aufruf nicht erlaubt!");
            return null;
        }


        /// <summary>
        /// Diese Routine sollte nur bei einem Reload benutzt werden. AddPending wir nicht mehr ausgelöst.
        /// </summary>
        /// <param name="comand">AddColumn muss benutzt werden. Ein Sicherheitsfaktor, um zu zeigen, dass das AddPending bereits erledigt wurde.</param>
        /// <param name="column"></param>
        /// <returns></returns>
        public ColumnItem Add(enDatabaseDataType comand, ColumnItem column)
        {

            if (comand != enDatabaseDataType.AddColumn)
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "Parent-Datenbanken unterschiedlich!");
            }

            if (column.Database != Database)
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "Parent-Datenbanken unterschiedlich!");
            }

            if (base.Contains(column))
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "Spalte bereits vorhanden!");
            }



            base.Add(column);

            return column;
        }


        /// <summary>
        /// Diese Routine sollte nur bei einem Load oder Reload benutzt werden.
        /// </summary>
        /// <param name="colKey">Ein bereits bekannter einmaliger Schlüssel mit einer Wert >= 0</param>
        /// <returns></returns>
        internal ColumnItem Add(int colKey)
        {
            Database.AddPending(enDatabaseDataType.AddColumn, colKey, -1, "", colKey.ToString(), true);
            return SearchByKey(colKey);
        }


        public ColumnItem Add()
        {
            return Add(Database.Column.NextColumnKey());
        }



        public ColumnItem Add(string internalName, string caption, enDataFormat format)
        {
            var c = Add();
            c.Name = internalName;

            c.Caption = caption;
            c.Format = format;

            return c;


        }

        public ColumnItem Add(string internalName, string caption, string suffix, enDataFormat format)
        {
            var c = Add();
            c.Name = internalName;
            c.Caption = caption;
            c.Format = format;
            c.Suffix = suffix;
            return c;
        }

        public ColumnItem Add(string internalName)
        {
            var c = Add();
            c.Name = internalName;
            return c;
        }

    }
}