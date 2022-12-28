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

    ///// <summary>
    ///// Diese Routine sollte nur bei einem Reload benutzt werden. AddPending wir nicht mehr ausgelöst.
    ///// </summary>
    ///// <param name="column"></param>
    ///// <returns></returns>
    //public void AddFromParser(ColumnItem? column) {
    //    if (column.Database != Database) { Develop.DebugPrint(FehlerArt.Fehler, "Parent-Datenbanken unterschiedlich!"); }
    //    if (Contains(column)) { Develop.DebugPrint(FehlerArt.Fehler, "Spalte bereits vorhanden!"); }
    //    base.GenerateAndAdd(column);
    //}
    /// <summary>
    /// Gib ColumnFirst oder Spalte 0 zurück
    /// </summary>
    /// <returns></returns>
    public ColumnItem? First => Exists(Database?.FirstColumn) ?? this[0];

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

            if (index >= 0 && index < Count) {
                return base[index];
            }

            Database.DevelopWarnung("Spalten-Index nicht gefunden: " + index);
            return null;
        }
    }

    public ColumnItem? this[string columnName] {
        get {
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

    public ColumnItem? Exists(string? columnName) {
        if (Database == null) {
            Develop.DebugPrint(FehlerArt.Fehler, "Database ist null bei " + columnName);
            return null;
        }
        if (columnName == null || string.IsNullOrEmpty(columnName)) {
            //             Develop.DebugPrint(enFehlerArt.Warnung, "Leerer Spaltenname"); Neue Spalten haben noch keinen Namen
            return null;
        }
        columnName = columnName.ToUpper();
        return this.FirstOrDefault(thisColumn => thisColumn != null && thisColumn.Name == columnName);
    }

    public string Freename(string preferedName) {
        preferedName = preferedName.ReduceToChars(Constants.AllowedCharsVariableName);
        if (string.IsNullOrEmpty(preferedName)) { preferedName = "NewColumn"; }

        if (Exists(preferedName) == null) { return preferedName; }

        string testName;
        var nr = 0;
        do {
            nr++;
            testName = preferedName + "_" + nr;
        } while (Exists(testName) != null);
        return testName;
    }

    public ColumnItem? GenerateAndAdd(string internalName, string caption, string suffix, VarType format) => GenerateAndAdd(NextColumnKey(), internalName, caption, suffix, format, string.Empty);

    public ColumnItem? GenerateAndAdd(string internalName, string caption, VarType format, string quickinfo) => GenerateAndAdd(NextColumnKey(), internalName, caption, string.Empty, format, quickinfo);

    public ColumnItem? GenerateAndAdd(string internalName, string caption, VarType format) => GenerateAndAdd(NextColumnKey(), internalName, caption, string.Empty, format, string.Empty);

    public ColumnItem? GenerateAndAdd(long colKey) => GenerateAndAdd(colKey, Freename(string.Empty), string.Empty, string.Empty, VarType.Unbekannt, string.Empty);

    public ColumnItem? GenerateAndAdd(long colKey, string internalName) => GenerateAndAdd(colKey, internalName, string.Empty, string.Empty, VarType.Unbekannt, string.Empty);

    public ColumnItem? GenerateAndAdd() => GenerateAndAdd(NextColumnKey(), Freename(string.Empty), string.Empty, string.Empty, VarType.Text, string.Empty);

    public ColumnItem? GenerateAndAdd(string internalName) => GenerateAndAdd(NextColumnKey(), internalName, internalName, string.Empty, VarType.Text, string.Empty);

    public ColumnItem? GenerateAndAdd(long key, string internalName, string caption, string suffix, VarType format, string quickinfo) {
        if (!ColumnItem.IsValidColumnName(internalName)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Spaltenname nicht erlaubt!");
            return null;
        }

        if (Database == null || Database.IsDisposed) { return null; }

        var item = SearchByKey(key);
        if (item != null) {
            Develop.DebugPrint(FehlerArt.Fehler, "Schlüssel belegt!");
            return null;
        }
        Database.ChangeData(DatabaseDataType.Comand_AddColumn, key, null, string.Empty, key.ToString());
        item = SearchByKey(key);
        if (item == null) {
            Develop.DebugPrint(FehlerArt.Fehler, "Erstellung fehlgeschlagen.");
            return null;
        }

        item.Name = internalName;
        item.Key = key;
        item.Caption = caption;

        if (format != VarType.Unbekannt) { item.SetFormat(format); }
        item.Suffix = suffix;
        item.Quickinfo = quickinfo;
        return item;
    }

    public void GenerateOverView() {
        if (Database == null || Database.IsDisposed) { return; }
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
    /// Setzt die fest vermerkten Spalten zurück und durchsucht die Spalten nach dem Identifierx.
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

        foreach (var thisColumnItem in this.Where(thisColumnItem => thisColumnItem != null && thisColumnItem.IsSystemColumn())) {
            switch (thisColumnItem.Name.ToUpper()) {
                case "SYS_LOCKED":
                    SysLocked = thisColumnItem;
                    break;

                case "SYS_CREATOR":
                    SysRowCreator = thisColumnItem;
                    break;

                case "SYS_CHANGER":
                    SysRowChanger = thisColumnItem;
                    break;

                case "SYS_DATECREATED":
                    SysRowCreateDate = thisColumnItem;
                    break;

                case "SYS_CORRECT":
                    SysCorrect = thisColumnItem;
                    break;

                case "SYS_DATECHANGED":
                    SysRowChangeDate = thisColumnItem;
                    break;

                case "SYS_CHAPTER":
                    SysChapter = thisColumnItem;
                    break;

                default:

                    Develop.DebugPrint(FehlerArt.Fehler, "Unbekannte Kennung: " + thisColumnItem.Name);
                    break;
            }
        }
    }

    public new void Remove(ColumnItem item) {
        if (Database == null || Database.IsDisposed) { return; }
        Database.ChangeData(DatabaseDataType.Comand_RemoveColumn, item.Key, null, string.Empty, item.Key.ToString());
    }

    public void Repair() {
        List<string> w = new()
        {
            "SYS_CHAPTER",
            "SYS_DATECHANGED",
            "SYS_CHANGER",
            "SYS_DATECREATED",
            "SYS_CREATOR",
            "SYS_CORRECT",
            "SYS_LOCKED"
        };

        foreach (var thisstring in w) {
            if (Exists(thisstring) == null) {
                GenerateAndAddSystem(thisstring);
            }
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
                        //// Evtl. Doppelte Identifierx eleminieren
                        //if (!string.IsNullOrEmpty(base[s1].Identifierx) && string.Equals(base[s1].Identifierx, base[s2].Identifierx, StringComparison.OrdinalIgnoreCase)) {
                        //    base[s2].Identifierx = string.Empty;
                        //}
                    }
                }
            }
        }

        // Reihengolge reparieren
        var colN = -1;
        do {
            colN++;
            if (colN + 2 > Count) { break; }

            var thisc = this[colN];

            if (thisc == null) {
                Swap(colN, colN + 1);
                colN = -1;
            } else {
                var thisc1 = this[colN + 1];
                if (thisc1 == null) {
                    // Dummy, um nachfoldgnd nicht abfragen zu müssen
                } else if (thisc.IsSystemColumn() && !thisc1.IsSystemColumn()) {
                    Swap(colN, colN + 1);
                    colN = -1;
                } else if (thisc.IsSystemColumn() && thisc1.IsSystemColumn()) {
                    if (w.IndexOf(thisc.Name) > w.IndexOf(thisc1.Name)) {
                        Swap(colN, colN + 1);
                        colN = -1;
                    }
                }
            }
        } while (true);

        foreach (var thisColumn in this) {
            thisColumn.Repair();
        }
    }

    public ColumnItem? SearchByKey(long? key) {
        try {
            if (Database == null) { return null; }
            if (key == null || key < 0) { return null; } // Evtl. Gelöschte Spalte in irgendeiner Order

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
        foreach (var thisColumn in this) {
            var l = sourceDatabase.Column.Exists(thisColumn.Name);
            if (l == null) { names.Add(thisColumn); }
        }
        foreach (var thisname in names) {
            Remove(thisname);
        }

        // Spalten erzeugen und Format übertragen
        foreach (var thisColumn in sourceDatabase.Column) {
            var l = Exists(thisColumn.Name) ?? GenerateAndAdd(thisColumn.Key, thisColumn.Name, thisColumn.Caption, thisColumn.Suffix, VarType.Unbekannt, thisColumn.Quickinfo);

            if (l != null) {
                l.CloneFrom(thisColumn, true);

                if (l.Name != thisColumn.Name) {
                    Develop.DebugPrint(FehlerArt.Fehler, "Name nicht korrekt!");
                }

                if (l.Key != thisColumn.Key) {
                    Develop.DebugPrint(FehlerArt.Fehler, "Key nicht korrekt!");
                }
            } else {
                Develop.DebugPrint(FehlerArt.Fehler, "Spalte nicht erzeugt!");
            }
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

    //internal string SetValueInternal(enDatabaseDataType type, string value, bool isLoading) {
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
    internal string SetValueInternal(DatabaseDataType type, long? key, bool isLoading) {
        if (Database == null || Database.IsDisposed) { return "Datenbank verworfen!"; }
        if (key is null or < 0) { return "Schlüsselfehler"; }

        if (type == DatabaseDataType.Comand_AddColumn) {
            var c = SearchByKey(key);
            if (c != null) { return "Bereits vorhanden!"; }

            c = new ColumnItem(Database, (long)key);
            Add(c);

            if (!isLoading) {
                Database.RepairColumnArrangements();
                Database.RepairViews();
            }

            return string.Empty;
        }

        if (type == DatabaseDataType.Comand_RemoveColumn) {
            var c = SearchByKey(key);
            if (c == null) { return "Spalte nicht gefunden!"; }
            base.Remove(c);

            if (!isLoading) {
                Database.RepairColumnArrangements();
                Database.RepairViews();
            }

            return string.Empty;
        }

        return "Befehl unbekannt";
    }

    protected override void Dispose(bool disposing) {
        if (Database != null) { Database.Disposing -= Database_Disposing; }

        Database = null;
        base.Dispose(disposing);
    }

    private void Database_Disposing(object sender, System.EventArgs e) => Dispose();

    private void GenerateAndAddSystem(string sysname) {
        var c = Exists(sysname);

        if (sysname == "SYS_DATECHANGED" && c == null) { c = Exists("SYS_CHANGEDATE"); }
        if (sysname == "SYS_DATECREATED" && c == null) { c = Exists("SYS_CREATEDATE"); }

        if (c != null) {
            c.Name = sysname; // Wegen der Namensverbiegung oben...
            c.ResetSystemToDefault(false);
            return;
        }

        c = GenerateAndAdd(sysname.ToUpper());
        c?.ResetSystemToDefault(true);
    }

    #endregion
}