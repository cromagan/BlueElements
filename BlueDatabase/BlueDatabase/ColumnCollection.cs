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
using static BlueBasics.IO;

namespace BlueDatabase;

public sealed class ColumnCollection : ListExt<ColumnItem> {

    #region Constructors

    public ColumnCollection(DatabaseAbstract database) : base() {
        Database = database;
        Database.Disposing += Database_Disposing;
    }

    #endregion

    #region Properties

    public DatabaseAbstract? Database { get; private set; }
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

    public static string ChangeKeysInString(string originalString, int oldKey, int newKey) {
        var o = ParsableColumnKey(oldKey);
        if (!originalString.Contains(o)) { return originalString; }
        var n = ParsableColumnKey(newKey);
        if (oldKey == newKey) {
            Develop.DebugPrint(FehlerArt.Fehler, "Schlüssel gleich:  " + oldKey);
            return originalString;
        }
        originalString = originalString.Replace(o + "}", n + "}");
        originalString = originalString.Replace(o + ",", n + ",");
        originalString = originalString.Replace(o + " ", n + " ");
        if (originalString.EndsWith(o)) { originalString = originalString.TrimEnd(o) + n; }
        if (originalString.Contains(o)) {
            Develop.DebugPrint(FehlerArt.Fehler, "String nicht ersetzt: " + originalString);
            return originalString;
        }
        return originalString;
    }

    //[Obsolete("Direkter Aufruf nicht erlaubt!", true)]
    //public new ColumnItem? Add(ColumnItem column) => null;

    public ColumnItem Add(string internalName) => Add(NextColumnKey(), internalName, internalName, string.Empty, VarType.Text, string.Empty);

    public ColumnItem Add(long colKey) => Add(colKey, Freename(string.Empty), string.Empty, string.Empty, VarType.Unbekannt, string.Empty);

    public ColumnItem Add() => Add(NextColumnKey(), Freename(string.Empty), string.Empty, string.Empty, VarType.Text, string.Empty);

    public ColumnItem Add(string internalName, string caption, VarType format) => Add(NextColumnKey(), internalName, caption, string.Empty, format, string.Empty);

    public ColumnItem Add(string internalName, string caption, VarType format, string quickinfo) => Add(NextColumnKey(), internalName, caption, string.Empty, format, quickinfo);

    public ColumnItem Add(string internalName, string caption, string suffix, VarType format) => Add(NextColumnKey(), internalName, caption, suffix, format, string.Empty);

    public ColumnItem Add(long colKey, string internalName, string caption, string suffix, VarType format, string quickinfo) {
        if (!ColumnItem.IsValidColumnName(internalName)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Spaltenname nicht erlaubt!");
        }

        var c = SearchByKey(colKey);

        //var addColumnKey = LongParse(value);
        if (c == null) {
            c=new ColumnItem(Database, internalName, colKey);
            base.Add(c);
            Database.ChangeData(DatabaseDataType.Comand_ColumnAdded, c, null, string.Empty, c.Name);
        }

        //var c = SearchByKey(colKey) ?? Exists(internalName);
        //Database.ChangeData(DatabaseDataType.AddColumnNameInfo, c, null, string.Empty, internalName.ToUpper());
        //c = SearchByKey(colKey) ?? Exists(internalName);
        //Database.ChangeData(DatabaseDataType.AddColumnKeyInfo, c, null, string.Empty, colKey.ToString());

        //// Ruft anschließen AddFromParser Auf, der die Spalte endgültig dazumacht

        //c = SearchByKey(colKey) ?? Exists(internalName);

        c.Name = internalName;
        c.Key = colKey;
        c.Caption = caption;

        if (format != VarType.Unbekannt) { c.SetFormat(format); }
        c.Suffix = suffix;
        c.Quickinfo = quickinfo;
        return c;
    }

    ///// <summary>
    ///// Diese Routine sollte nur bei einem Reload benutzt werden. AddPending wir nicht mehr ausgelöst.
    ///// </summary>
    ///// <param name="column"></param>
    ///// <returns></returns>
    //public void AddFromParser(ColumnItem? column) {
    //    if (column.Database != Database) { Develop.DebugPrint(FehlerArt.Fehler, "Parent-Datenbanken unterschiedlich!"); }
    //    if (Contains(column)) { Develop.DebugPrint(FehlerArt.Fehler, "Spalte bereits vorhanden!"); }
    //    base.Add(column);
    //}

    public ColumnItem? Exists(string columnName) {
        if (Database == null) {
            Develop.DebugPrint(FehlerArt.Fehler, "Database ist null bei " + columnName);
            return null;
        }
        if (string.IsNullOrEmpty(columnName)) {
            //             Develop.DebugPrint(enFehlerArt.Warnung, "Leerer Spaltenname"); Neue Spalten haben noch keinen Namen
            return null;
        }
        columnName = columnName.ToUpper();
        return this.FirstOrDefault(thisColumn => thisColumn != null && thisColumn.Name == columnName);
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
        string? testName;
        do {
            nr++;
            testName = wunschname + "_" + nr;
        } while (Exists(testName) != null);
        return testName;
    }

    public void GenerateOverView() {
        Html da = new(Database.TableName);
        da.AddCaption("Spaltenliste von: " + Database.Caption);
        da.Add("  <Font face=\"Arial\" Size=\"4\">" + Database.TableName + "</h1><br>");
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
        foreach (var thisColumnItem in Database.Column.Where(thisColumnItem => thisColumnItem != null)) {
            lfdn++;
            da.RowBeginn();
            da.CellAdd(lfdn.ToString());
            da.CellAdd(thisColumnItem.Name);
            da.CellAdd(thisColumnItem.Caption.Replace("\r", "<br>"));
            da.CellAdd((thisColumnItem.CaptionGroup1 + "/" + thisColumnItem.CaptionGroup2 + "/" + thisColumnItem.CaptionGroup3 + "/").TrimEnd("/"));
            da.CellAdd(thisColumnItem.Format.ToString());
            da.CellAdd(thisColumnItem.Quickinfo.Replace("\r", "<br>"));
            da.CellAdd(thisColumnItem.AdminInfo.Replace("\r", "<br>"));
            da.CellAdd(thisColumnItem.Tags.JoinWith("<br>"));
            da.CellAdd(thisColumnItem.PermissionGroupsChangeCell.JoinWith("<br>"));
            da.RowEnd();
        }
        da.TableEnd();
        da.AddFoot();
        da.Save(TempFile("", "Spaltenliste.html"), true);
    }

    /// <summary>
    /// Setzt die fest vermerkten Spalten zurück und durchsucht die Spalten nach dem Identifier.
    /// Es werden nur die gefunden Spalten gemerkt - keine neuen erstellt!
    /// </summary>
    public void GetSystems() {
        SysLocked = null;
        SysRowCreateDate = null;
        SysRowCreator = null;
        SysCorrect = null;
        SysRowChanger = null;
        SysRowChangeDate = null;
        SysChapter = null;

        foreach (var thisColumnItem in this.Where(thisColumnItem => thisColumnItem != null)) {
            switch (thisColumnItem.Identifier) {
                case "":
                    break;

                case "System: Locked":
                    SysLocked = thisColumnItem;
                    break;

                case "System: Creator":
                    SysRowCreator = thisColumnItem;
                    break;

                case "System: Changer":
                    SysRowChanger = thisColumnItem;
                    break;

                case "System: Date Created":
                    SysRowCreateDate = thisColumnItem;
                    break;

                case "System: Correct":
                    SysCorrect = thisColumnItem;
                    break;

                case "System: Date Changed":
                    SysRowChangeDate = thisColumnItem;
                    break;

                case "System: Chapter":
                    SysChapter = thisColumnItem;
                    break;

                default:
                    Develop.DebugPrint(FehlerArt.Fehler, "Unbekannte Kennung: " + thisColumnItem.Identifier);
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
                        if (string.Equals(base[s1].Name, base[s2].Name, StringComparison.OrdinalIgnoreCase)) {
                            base[s2].Name = base[s2].Name + "0";
                        }
                        // Evtl. Doppelte Identifier eleminieren
                        if (!string.IsNullOrEmpty(base[s1].Identifier) && string.Equals(base[s1].Identifier, base[s2].Identifier, StringComparison.OrdinalIgnoreCase)) {
                            base[s2].Identifier = string.Empty;
                        }
                    }
                }
            }
        }

        // Reihengolge reparieren
        var colN = -1;
        do {
            colN++;
            if (colN + 2 > Count) { break; }
            if (this[colN] == null) {
                Swap(colN, colN + 1);
                colN = -1;
            } else if (this[colN + 1] == null) {
                // Dummy, um nachfoldgnd nicht abfragen zu müssen
            } else if (!string.IsNullOrEmpty(this[colN].Identifier) && string.IsNullOrEmpty(this[colN + 1].Identifier)) {
                Swap(colN, colN + 1);
                colN = -1;
            } else if (!string.IsNullOrEmpty(this[colN].Identifier) && !string.IsNullOrEmpty(this[colN + 1].Identifier)) {
                if (w.IndexOf(this[colN].Identifier) > w.IndexOf(this[colN + 1].Identifier)) {
                    Swap(colN, colN + 1);
                    colN = -1;
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
            return this.FirstOrDefault(thisColumn => thisColumn != null && thisColumn.Key == key);
        } catch {
            return SearchByKey(key); // Sammlung wurde verändert
        }
    }

    //public new void Swap(int index1, int index2) {
    //    if (column1 == null || column2 == null) { return; }
    //    base.Swap(index1, column2);
    //    column1.Invalidate_ColumAndContentx();
    //    column2.Invalidate_ColumAndContentx();
    //    //Database.ColumnArrangements[0].ShowAllColumns(); // Damit die Datenbank mitbekommt, das sich da was geändert hat
    //}

    internal static string ParsableColumnKey(ColumnItem? column) => column == null ? "ColumnKey=?" : ParsableColumnKey(column.Key);

    internal static string ParsableColumnKey(long key) => "ColumnKey=" + key;

    internal void CloneFrom(DatabaseAbstract sourceDatabase) {
        // Spalten, die zu viel sind, löschen
        var names = new List<ColumnItem>();
        foreach (var ThisColumn in this) {
            var l = sourceDatabase.Column.Exists(ThisColumn.Name);
            if (l == null) { names.Add(ThisColumn); }
        }
        foreach (var thisname in names) {
            Remove(thisname);
        }

        // Spalten erzeugen und Format übertragen
        foreach (var ThisColumn in sourceDatabase.Column) {
            var l = Exists(ThisColumn.Name);
            if (l == null) {
                l = Add(ThisColumn.Key, ThisColumn.Name, ThisColumn.Caption, ThisColumn.Suffix, VarType.Unbekannt, ThisColumn.Quickinfo);
            }
            l.CloneFrom(ThisColumn, true);
        }
    }

    internal long NextColumnKey() {
        var tmp = 0;
        long key;

        do {
            key = Generic.GetUniqueKey(tmp, "column");
            tmp++;
        } while (SearchByKey(key) != null);
        return key;
    }

    //internal string SetValueInternal(enDatabaseDataType type, string value) {
    //    switch (type) {
    //        case enDatabaseDataType.LastColumnKey:
    //            _LastColumnKey = LongParse(value);
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

    protected override void Dispose(bool disposing) {
        Database.Disposing -= Database_Disposing;
        Database = null;
        base.Dispose(disposing);
    }

    private void AddSystem(string identifier) {
        if (this.Any(thisColumn => thisColumn != null && string.Equals(thisColumn.Identifier, identifier, StringComparison.OrdinalIgnoreCase))) {
            return;
        }

        var nam = identifier.ToUpper();
        nam = nam.Replace("SYSTEM: ", "SYS_");
        nam = nam.Replace(" ", "");

        var c = Add(nam);
        c.Identifier = identifier;
        //c.SetValueInternal(DatabaseDataType.ColumnIdentify, identifier);
        c.ResetSystemToDefault(true);
    }

    private void Database_Disposing(object sender, System.EventArgs e) => Dispose();

    #endregion
}