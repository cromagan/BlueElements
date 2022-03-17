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

#nullable enable

using BlueBasics;
using BlueBasics.Enums;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static BlueBasics.FileOperations;

namespace BlueDatabase {

    public sealed class ColumnCollection : ListExt<ColumnItem> {

        #region Constructors

        public ColumnCollection(Database database) : base() {
            Database = database;
            Database.Disposing += Database_Disposing;
        }

        #endregion

        #region Properties

        public Database? Database { get; private set; }
        public ColumnItem? SysChapter { get; private set; }

        public ColumnItem? SysCorrect { get; private set; }

        public ColumnItem? SysLocked { get; private set; }

        public ColumnItem? SysRowChangeDate { get; private set; }

        public ColumnItem? SysRowChanger { get; private set; }

        public ColumnItem? SysRowCreateDate { get; private set; }

        public ColumnItem? SysRowCreator { get; private set; }

        #endregion

        #region Indexers

        public new ColumnItem? this[int index] {
            get {
                if (Database == null) { return null; }
                Database.BlockReload(false);
                if (index >= 0 && index < Count) {
                    return base[index];
                }

                Database.DevelopWarnung("Spalten-Index nicht gefunden: " + index);
                return null;
            }
        }

        public ColumnItem? this[string columnName] {
            get {
                Database?.BlockReload(false);
                var colum = Exists(columnName);
                if (colum is null) { Database?.DevelopWarnung("Spalte nicht gefunden: " + columnName); }
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

        [Obsolete("Direkter Aufruf nicht erlaubt!", true)]
        public new ColumnItem? Add(ColumnItem column) => null;

        public ColumnItem Add(string internalName) => Add(NextColumnKey(), internalName, internalName, string.Empty, enVarType.Text, string.Empty);

        public ColumnItem Add(long colKey) => Add(colKey, string.Empty, string.Empty, string.Empty, enVarType.Text, string.Empty);

        public ColumnItem Add() => Add(NextColumnKey(), string.Empty, string.Empty, string.Empty, enVarType.Text, string.Empty);

        public ColumnItem Add(string internalName, string caption, enVarType format) => Add(NextColumnKey(), internalName, caption, string.Empty, format, string.Empty);

        public ColumnItem Add(string internalName, string caption, enVarType format, string quickinfo) => Add(NextColumnKey(), internalName, caption, string.Empty, format, quickinfo);

        public ColumnItem Add(string internalName, string caption, string suffix, enVarType format) => Add(NextColumnKey(), internalName, caption, suffix, format, string.Empty);

        public ColumnItem Add(long colKey, string internalName, string caption, string suffix, enVarType format, string quickinfo) {
            Database.AddPending(DatabaseDataType.AddColumn, colKey, -1, string.Empty, colKey.ToString(), true);
            // Ruft anschließen AddFromParserAuf, der die Spalte endgülrig dazumacht
            var c = SearchByKey(colKey);
            c.Name = internalName;
            c.Caption = caption;
            c.SetFormat(format);
            c.Suffix = suffix;
            c.Quickinfo = quickinfo;
            return c;
        }

        /// <summary>
        /// Diese Routine sollte nur bei einem Reload benutzt werden. AddPending wir nicht mehr ausgelöst.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public void AddFromParser(ColumnItem? column) {
            if (column.Database != Database) { Develop.DebugPrint(enFehlerArt.Fehler, "Parent-Datenbanken unterschiedlich!"); }
            if (Contains(column)) { Develop.DebugPrint(enFehlerArt.Fehler, "Spalte bereits vorhanden!"); }
            base.Add(column);
        }

        public ColumnItem? Exists(string columnName) {
            if (Database == null) {
                Develop.DebugPrint(enFehlerArt.Fehler, "Database ist null bei " + columnName);
                return null;
            }
            if (string.IsNullOrEmpty(columnName)) {
                //             Develop.DebugPrint(enFehlerArt.Warnung, "Leerer Spaltenname"); Neue Spalten haben noch keinen Namen
                return null;
            }
            columnName = columnName.ToUpper();
            return this.FirstOrDefault(ThisColumn => ThisColumn != null && ThisColumn.Name == columnName);
        }

        /// <summary>
        /// Gib Spalte 0 zurück
        /// </summary>
        /// <returns></returns>
        public ColumnItem? First() => this[0];

        public string Freename(string wunschname) {
            var nr = 0;
            wunschname = wunschname.ReduceToChars(Constants.AllowedCharsVariableName);
            if (string.IsNullOrEmpty(wunschname)) { wunschname = "NewColumn"; }
            if (Exists(wunschname) == null) { return wunschname; }
            string? TestName;
            do {
                nr++;
                TestName = wunschname + "_" + nr;
            } while (Exists(TestName) != null);
            return TestName;
        }

        public void GenerateOverView() {
            Html da = new(Database.Filename.FileNameWithoutSuffix());
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
            foreach (var ThisColumnItem in Database.Column.Where(ThisColumnItem => ThisColumnItem != null)) {
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
                da.CellAdd(ThisColumnItem.PermissionGroupsChangeCell.JoinWith("<br>"));
                da.RowEnd();
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
            foreach (var ThisColumnItem in this.Where(ThisColumnItem => ThisColumnItem != null)) {
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

            foreach (var thisstring in w) {
                AddSystem(thisstring);
            }
            GetSystems();
            for (var s1 = 0; s1 < Count; s1++) {
                if (base[s1] != null) {
                    for (var s2 = s1 + 1; s2 < Count; s2++) {
                        if (base[s2] != null) {
                            // Evtl. Doppelte Namen einzigartig machen
                            if (string.Equals(base[s1].Name, base[s2].Name, StringComparison.CurrentCultureIgnoreCase)) {
                                base[s2].Load(DatabaseDataType.co_Name, base[s2].Name + "0");
                            }
                            // Evtl. Doppelte Identifier eleminieren
                            if (!string.IsNullOrEmpty(base[s1].Identifier) && string.Equals(base[s1].Identifier, base[s2].Identifier, StringComparison.CurrentCultureIgnoreCase)) {
                                base[s2].Load(DatabaseDataType.co_Identifier, string.Empty);
                            }
                        }
                    }
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

            foreach (var thisColumn in this) {
                thisColumn.Repair();
            }
        }

        public ColumnItem? SearchByKey(long key) {
            try {
                if (Database == null) { return null; }
                if (key < 0) { return null; } // Evtl. Gelöschte Spalte in irgendeiner Order
                Database.BlockReload(false);
                return this.FirstOrDefault(ThisColumn => ThisColumn != null && ThisColumn.Key == key);
            } catch {
                return SearchByKey(key); // Sammlung wurde verändert
            }
        }

        public new void Swap(ColumnItem? column1, ColumnItem? column2) {
            if (column1 == null || column2 == null) { return; }
            base.Swap(column1, column2);
            column1.Invalidate_ColumAndContent();
            column2.Invalidate_ColumAndContent();
            Database.ColumnArrangements[0].ShowAllColumns(); // Damit die Datenbank mitbekommt, das sich da was geändert hat
        }

        internal static string ParsableColumnKey(ColumnItem Column) => Column == null ? "ColumnKey=?" : ParsableColumnKey(Column.Key);

        internal static string ParsableColumnKey(long Key) => "ColumnKey=" + Key;

        internal long NextColumnKey() {
            var s = Generic.UserName() + "\r\n" + Thread.CurrentThread.ManagedThreadId + "\r\n" + Environment.MachineName;
            var tmp = 0;
            long key;

            do {
                tmp++;
                key = (s.GetHashCode() * 100000000) + tmp;
                if (key < 0) { key *= -1; }
            } while (SearchByKey(key) != null);
            return key;
        }

        //internal string Load_310(enDatabaseDataType type, string value) {
        //    switch (type) {
        //        case enDatabaseDataType.LastColumnKey:
        //            _LastColumnKey = IntParse(value);
        //            break;

        //        default:
        //            if (type.ToString() == ((int)type).ToString()) {
        //                Develop.DebugPrint(enFehlerArt.Info, "Laden von Datentyp '" + type + "' nicht definiert.<br>Wert: " + value + "<br>Datei: " + Database.Filename);
        //            } else {
        //                return "Interner Fehler: Für den Datentyp  '" + type + "'  wurde keine Laderegel definiert.";
        //            }
        //            break;
        //    }
        //    return string.Empty;
        //}

        internal void SaveToByteList(List<byte> List) {
            //Database.SaveToByteList(List, enDatabaseDataType.LastColumnKey, _LastColumnKey.ToString());
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
            if (this.Any(ThisColumn => ThisColumn != null && string.Equals(ThisColumn.Identifier, identifier, StringComparison.CurrentCultureIgnoreCase))) {
                return;
            }
            var c = Add(identifier);
            c.Load(DatabaseDataType.co_Identifier, identifier);
            c.ResetSystemToDefault(true);
        }

        private void Database_Disposing(object sender, System.EventArgs e) => Dispose();

        #endregion
    }
}