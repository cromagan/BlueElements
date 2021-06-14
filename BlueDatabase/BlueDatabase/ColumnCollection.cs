// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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

using BlueBasics;
using BlueBasics.Enums;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using static BlueBasics.FileOperations;

namespace BlueDatabase {

    public sealed class ColumnCollection : ListExt<ColumnItem> {

        #region Fields

        private int _LastColumnKey;

        #endregion

        #region Constructors

        public ColumnCollection(Database database) {
            Database = database;
            Database.Disposing += Database_Disposing;
            Initialize();
        }

        #endregion

        #region Properties

        public Database Database { get; private set; }
        public ColumnItem SysChapter { get; private set; }

        public ColumnItem SysCorrect { get; private set; }

        public ColumnItem SysLocked { get; private set; }

        public ColumnItem SysRowChangeDate { get; private set; }

        public ColumnItem SysRowChanger { get; private set; }

        public ColumnItem SysRowCreateDate { get; private set; }

        public ColumnItem SysRowCreator { get; private set; }

        internal int NextColumnKey {
            get {
                do {
                    if (_LastColumnKey == int.MaxValue) { _LastColumnKey = 0; }
                    _LastColumnKey++;
                    if (SearchByKey(_LastColumnKey) == null) { return _LastColumnKey; }
                } while (true);
            }
        }

        #endregion

        #region Indexers

        public new ColumnItem this[int index] {
            get {
                if (Database == null) { return null; }
                Database.BlockReload(false);
                if (index < 0 || index >= Count) {
                    Database.DevelopWarnung("Spalten-Index nicht gefunden: " + index.ToString());
                    return null;
                }
                return base[index];
            }
        }

        public ColumnItem this[string columnName] {
            get {
                Database.BlockReload(false);
                var colum = Exists(columnName);
                if (colum is null) { Database.DevelopWarnung("Spalte nicht gefunden: " + columnName); }
                return colum;
            }
        }

        #endregion

        #region Methods

        public static string ChangeKeysInString(string OriginalString, int OldKey, int NewKey) {
            var o = ParsableColumnKey(OldKey);
            if (!OriginalString.Contains(o)) { return OriginalString; }
            var n = ParsableColumnKey(NewKey);
            if (OldKey == NewKey) {
                Develop.DebugPrint(enFehlerArt.Fehler, "Schlüssel gleich:  " + OldKey);
                return OriginalString;
            }
            OriginalString = OriginalString.Replace(o + "}", n + "}");
            OriginalString = OriginalString.Replace(o + ",", n + ",");
            OriginalString = OriginalString.Replace(o + " ", n + " ");
            if (OriginalString.EndsWith(o)) { OriginalString = OriginalString.TrimEnd(o) + n; }
            if (OriginalString.Contains(o)) {
                Develop.DebugPrint(enFehlerArt.Fehler, "String nicht ersetzt: " + OriginalString);
                return OriginalString;
            }
            return OriginalString;
        }

        [Obsolete]
        public new ColumnItem Add(ColumnItem column) {
            Develop.DebugPrint(enFehlerArt.Fehler, "Direkter Aufruf nicht erlaubt!");
            return null;
        }

        public ColumnItem Add(string internalName) => Add(NextColumnKey, internalName, internalName, string.Empty, enDataFormat.Text);

        public ColumnItem Add(int colKey) => Add(colKey, string.Empty, string.Empty, string.Empty, enDataFormat.Text);

        public ColumnItem Add() => Add(NextColumnKey, string.Empty, string.Empty, string.Empty, enDataFormat.Text);

        public ColumnItem Add(string internalName, string caption, enDataFormat format) => Add(NextColumnKey, internalName, caption, string.Empty, format);

        public ColumnItem Add(string internalName, string caption, string suffix, enDataFormat format) => Add(NextColumnKey, internalName, caption, suffix, format);

        public ColumnItem Add(int colKey, string internalName, string caption, string suffix, enDataFormat format) {
            Database.AddPending(enDatabaseDataType.AddColumn, colKey, -1, string.Empty, colKey.ToString(), true);
            // Ruft anschließen AddFromParserAuf, der die Spalte endgülrig dazumacht
            var c = SearchByKey(colKey);
            c.Name = internalName;
            c.Caption = caption;
            c.Format = format;
            c.Suffix = suffix;
            return c;
        }

        public ColumnItem AddACloneFrom(ColumnItem source) {
            var c = Add();
            c.Caption = source.Caption;
            c.CaptionBitmap = source.CaptionBitmap;
            c.Format = source.Format;
            c.LineLeft = source.LineLeft;
            c.LineRight = source.LineRight;
            c.MultiLine = source.MultiLine;
            c.Quickinfo = source.Quickinfo;
            c.ForeColor = source.ForeColor;
            c.BackColor = source.BackColor;
            c.EditTrotzSperreErlaubt = source.EditTrotzSperreErlaubt;
            c.EditType = source.EditType;
            c.Identifier = source.Identifier;
            c.PermissionGroups_ChangeCell.Clear();
            c.PermissionGroups_ChangeCell.AddRange(source.PermissionGroups_ChangeCell);
            c.Tags.Clear();
            c.Tags.AddRange(source.Tags);
            c.AllowedChars = source.AllowedChars;
            c.AdminInfo = source.AdminInfo;
            c.FilterOptions = source.FilterOptions;
            //c.AutoFilterErlaubt = Source.AutoFilterErlaubt;
            //c.AutofilterTextFilterErlaubt = Source.AutofilterTextFilterErlaubt;
            //c.AutoFilterErweitertErlaubt = Source.AutoFilterErweitertErlaubt;
            c.IgnoreAtRowFilter = source.IgnoreAtRowFilter;
            c.DropdownBearbeitungErlaubt = source.DropdownBearbeitungErlaubt;
            c.DropdownAllesAbwählenErlaubt = source.DropdownAllesAbwählenErlaubt;
            c.TextBearbeitungErlaubt = source.TextBearbeitungErlaubt;
            c.SpellCheckingEnabled = source.SpellCheckingEnabled;
            c.DropdownWerteAndererZellenAnzeigen = source.DropdownWerteAndererZellenAnzeigen;
            c.AfterEdit_QuickSortRemoveDouble = source.AfterEdit_QuickSortRemoveDouble;
            c.AfterEdit_Runden = source.AfterEdit_Runden;
            c.AfterEdit_DoUCase = source.AfterEdit_DoUCase;
            c.AfterEdit_AutoCorrect = source.AfterEdit_AutoCorrect;
            c.AutoRemove = source.AutoRemove;
            c.SaveContent = source.SaveContent;
            c.CellInitValue = source.CellInitValue;
            c.AutoFilterJoker = source.AutoFilterJoker;
            c.KeyColumnKey = source.KeyColumnKey;
            c.LinkedCell_RowKey = source.LinkedCell_RowKey;
            c.LinkedCell_ColumnKey = source.LinkedCell_ColumnKey;
            c.LinkedCell_ColumnValueFoundIn = source.LinkedCell_ColumnValueFoundIn;
            c.LinkedCell_ColumnValueAdd = source.LinkedCell_ColumnValueAdd;
            //c.ZellenZusammenfassen = Source.ZellenZusammenfassen;
            c.DropdownKey = source.DropdownKey;
            c.VorschlagsColumn = source.VorschlagsColumn;
            c.Align = source.Align;
            c.SortMask = source.SortMask;
            c.DropDownItems.Clear();
            c.DropDownItems.AddRange(source.DropDownItems);
            c.OpticalReplace.Clear();
            c.OpticalReplace.AddRange(source.OpticalReplace);
            c.AfterEdit_AutoReplace.Clear();
            c.AfterEdit_AutoReplace.AddRange(source.OpticalReplace);
            c.Regex.Clear();
            c.Regex.AddRange(source.Regex);
            //c.CompactView = Source.CompactView;
            c.ShowUndo = source.ShowUndo;
            c.ShowMultiLineInOneLine = source.ShowMultiLineInOneLine;
            c.Ueberschrift1 = source.Ueberschrift1;
            c.Ueberschrift2 = source.Ueberschrift2;
            c.Ueberschrift3 = source.Ueberschrift3;
            c.Suffix = source.Suffix;
            //c.Intelligenter_Multifilter = Source.Intelligenter_Multifilter;
            c.DauerFilterPos = new Point(source.DauerFilterPos.X, source.DauerFilterPos.Y);
            c.LinkedKeyKennung = source.LinkedKeyKennung;
            c.LinkedDatabaseFile = source.LinkedDatabaseFile;
            c.BildTextVerhalten = source.BildTextVerhalten;
            c.BildCode_ConstantHeight = source.BildCode_ConstantHeight;
            c.BestFile_StandardSuffix = source.BestFile_StandardSuffix;
            c.BestFile_StandardFolder = source.BestFile_StandardFolder;
            c.Prefix = source.Prefix;
            return c;
        }

        /// <summary>
        /// Diese Routine sollte nur bei einem Reload benutzt werden. AddPending wir nicht mehr ausgelöst.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public void AddFromParser(ColumnItem column) {
            if (column.Database != Database) { Develop.DebugPrint(enFehlerArt.Fehler, "Parent-Datenbanken unterschiedlich!"); }
            if (Contains(column)) { Develop.DebugPrint(enFehlerArt.Fehler, "Spalte bereits vorhanden!"); }
            base.Add(column);
        }

        public ColumnItem Exists(string columnName) {
            if (Database == null) {
                Develop.DebugPrint(enFehlerArt.Fehler, "Database ist null bei " + columnName);
                return null;
            }
            if (string.IsNullOrEmpty(columnName)) {
                //             Develop.DebugPrint(enFehlerArt.Warnung, "Leerer Spaltenname"); Neue Spalten haben noch keinen Namen
                return null;
            }
            columnName = columnName.ToUpper();
            foreach (var ThisColumn in this) {
                if (ThisColumn != null && ThisColumn.Name == columnName) { return ThisColumn; }
            }
            return null;
        }

        public string Freename(string wunschname) {
            var nr = 0;
            wunschname = wunschname.ReduceToChars(Constants.AllowedCharsVariableName);
            if (string.IsNullOrEmpty(wunschname)) { wunschname = "NewColumn"; }
            if (Exists(wunschname) == null) { return wunschname; }
            string TestName;
            do {
                nr++;
                TestName = wunschname + "_" + nr;
            } while (Exists(TestName) != null);
            return TestName;
        }

        public void GenerateOverView() {
            HTML da = new(Database.Filename.FileNameWithoutSuffix());
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
            foreach (var ThisColumnItem in Database.Column) {
                if (ThisColumnItem != null) {
                    lfdn++;
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

        public void GetSystems() {
            SysLocked = null;
            SysRowCreateDate = null;
            SysRowCreator = null;
            SysCorrect = null;
            SysRowChanger = null;
            SysRowChangeDate = null;
            SysChapter = null;
            foreach (var ThisColumnItem in this) {
                if (ThisColumnItem != null) {
                    switch (ThisColumnItem.Identifier) {
                        case "":
                            break;

                        case "System: Locked":
                            SysLocked = ThisColumnItem;
                            break;

                        case "System: Creator":
                            SysRowCreator = ThisColumnItem;
                            break;

                        case "System: Changer":
                            SysRowChanger = ThisColumnItem;
                            break;

                        case "System: Date Created":
                            SysRowCreateDate = ThisColumnItem;
                            break;

                        case "System: Correct":
                            SysCorrect = ThisColumnItem;
                            break;

                        case "System: Date Changed":
                            SysRowChangeDate = ThisColumnItem;
                            break;

                        case "System: Chapter":
                            SysChapter = ThisColumnItem;
                            break;

                        default:
                            Develop.DebugPrint(enFehlerArt.Fehler, "Unbekannte Kennung: " + ThisColumnItem.Identifier);
                            break;
                    }
                }
            }
        }

        public void Initialize() {
        }

        public void Repair() {
            List<string> w = new()
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
            foreach (var ThisColumnItem in this) {
                if (ThisColumnItem != null) {
                    _LastColumnKey = Math.Max(_LastColumnKey, ThisColumnItem.Key);
                    ThisColumnItem.CheckIfIAmAKeyColumn();
                }
            }
            foreach (var thisstring in w) {
                AddSystem(thisstring);
            }
            GetSystems();
            for (var s1 = 0; s1 < Count; s1++) {
                if (base[s1] != null) {
                    for (var s2 = s1 + 1; s2 < Count; s2++) {
                        if (base[s2] != null) {
                            // Evtl. Doppelte Namen einzigartig machen
                            if (base[s1].Name.ToUpper() == base[s2].Name.ToUpper()) {
                                base[s2].Load(enDatabaseDataType.co_Name, base[s2].Name + "0");
                            }
                            // Evtl. Doppelte Identifier eleminieren
                            if (!string.IsNullOrEmpty(base[s1].Identifier) && base[s1].Identifier.ToUpper() == base[s2].Identifier.ToUpper()) {
                                base[s2].Load(enDatabaseDataType.co_Identifier, string.Empty);
                            }
                        }
                    }
                    //base[s1].Repair();
                }
            }
            // Reihengolge reparieren
            var ColN = -1;
            do {
                ColN++;
                if (ColN + 2 > Count) { break; }
                if (this[ColN] == null) {
                    Swap(ColN, ColN + 1);
                    ColN = -1;
                } else if (this[ColN + 1] == null) {
                    // Dummy, um nachfoldgnd nicht abfragen zu müssen
                } else if (!string.IsNullOrEmpty(this[ColN].Identifier) && string.IsNullOrEmpty(this[ColN + 1].Identifier)) {
                    Swap(ColN, ColN + 1);
                    ColN = -1;
                } else if (!string.IsNullOrEmpty(this[ColN].Identifier) && !string.IsNullOrEmpty(this[ColN + 1].Identifier)) {
                    if (w.IndexOf(this[ColN].Identifier) > w.IndexOf(this[ColN + 1].Identifier)) {
                        Swap(ColN, ColN + 1);
                        ColN = -1;
                    }
                }
            } while (true);
        }

        public ColumnItem SearchByKey(int key) {
            try {
                if (Database == null) { return null; }
                if (key < 0) { return null; } // Evtl. Gelöschte Spalte in irgendeiner Order
                Database.BlockReload(false);
                foreach (var ThisColumn in this) {
                    if (ThisColumn != null && ThisColumn.Key == key) {
                        return ThisColumn;
                    }
                }
                //// Beim Parsen und erstellen werden die Spalten ja erstellt
                //if (!Database.IsParsing) { Database.DevelopWarnung("Spalten-Key nicht gefunden: " + key.ToString()); }
                return null;
            } catch {
                return SearchByKey(key); // Sammlung wurde verändert
            }
        }

        public new void Swap(ColumnItem column1, ColumnItem column2) {
            if (column1 == null || column2 == null) { return; }
            base.Swap(column1, column2);
            column1.Invalidate_ColumAndContent();
            column2.Invalidate_ColumAndContent();
            Database.ColumnArrangements[0].ShowAllColumns(); // Damit die Datenbank mitbekommt, das sich da was geändert hat
        }

        internal static string ParsableColumnKey(ColumnItem Column) => Column == null ? "ColumnKey=?" : ParsableColumnKey(Column.Key);

        internal static string ParsableColumnKey(int Key) => "ColumnKey=" + Key;

        internal string Load_310(enDatabaseDataType Art, string Wert) {
            switch (Art) {
                case enDatabaseDataType.LastColumnKey:
                    _LastColumnKey = int.Parse(Wert);
                    break;

                default:
                    if (Art.ToString() == ((int)Art).ToString()) {
                        Develop.DebugPrint(enFehlerArt.Info, "Laden von Datentyp '" + Art + "' nicht definiert.<br>Wert: " + Wert + "<br>Datei: " + Database.Filename);
                    } else {
                        return "Interner Fehler: Für den Datentyp  '" + Art + "'  wurde keine Laderegel definiert.";
                    }
                    break;
            }
            return "";
        }

        internal void SaveToByteList(List<byte> List) {
            Database.SaveToByteList(List, enDatabaseDataType.LastColumnKey, _LastColumnKey.ToString());
            for (var ColumnCount = 0; ColumnCount < Count; ColumnCount++) {
                if (this[ColumnCount] != null && !string.IsNullOrEmpty(this[ColumnCount].Name)) {
                    this[ColumnCount].SaveToByteList(ref List);
                }
            }
        }

        protected override void Dispose(bool disposing) {
            Database.Disposing -= Database_Disposing;
            Database = null;
            base.Dispose(disposing);
        }

        private void AddSystem(string identifier) {
            foreach (var ThisColumn in this) {
                if (ThisColumn != null && ThisColumn.Identifier.ToUpper() == identifier.ToUpper()) { return; }
            }
            var c = Add(identifier);
            c.Load(enDatabaseDataType.co_Identifier, identifier);
            c.ResetSystemToDefault(true);
        }

        private void Database_Disposing(object sender, System.EventArgs e) => Dispose();

        #endregion
    }
}