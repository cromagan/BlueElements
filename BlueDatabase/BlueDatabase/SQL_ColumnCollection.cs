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

public sealed class SQL_ColumnCollection : ListExt<SQL_ColumnItem> {

    #region Constructors

    public SQL_ColumnCollection(SQL_Database database) : base() {
        Database = database;
        Database.Disposing += Database_Disposing;
    }

    #endregion

    #region Properties

    public SQL_Database? Database { get; private set; }
    public SQL_ColumnItem? SysChapter { get; private set; }

    public SQL_ColumnItem? SysCorrect { get; private set; }

    public SQL_ColumnItem? SysLocked { get; private set; }

    public SQL_ColumnItem? SysRowChangeDate { get; private set; }

    public SQL_ColumnItem? SysRowChanger { get; private set; }

    public SQL_ColumnItem? SysRowCreateDate { get; private set; }

    public SQL_ColumnItem? SysRowCreator { get; private set; }

    #endregion

    #region Indexers

    public new SQL_ColumnItem? this[int index] {
        get {
            if (Database == null) { return null; }
            if (index >= 0 && index < Count) {
                return base[index];
            }

            Database.DevelopWarnung("Spalten-Index nicht gefunden: " + index);
            return null;
        }
    }

    public SQL_ColumnItem? this[string columnName] {
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
            Develop.DebugPrint(FehlerArt.Fehler, "Schl�ssel gleich:  " + oldKey);
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

    [Obsolete("Direkter Aufruf nicht erlaubt!", true)]
    public new SQL_ColumnItem? Add(SQL_ColumnItem column) => null;

    public SQL_ColumnItem Add(string internalName) => Add(NextColumnKey(), internalName, internalName, string.Empty, VarType.Text, string.Empty);

    public SQL_ColumnItem Add(long colKey) => Add(colKey, string.Empty, string.Empty, string.Empty, VarType.Unbekannt, string.Empty);

    public SQL_ColumnItem Add() => Add(NextColumnKey(), string.Empty, string.Empty, string.Empty, VarType.Text, string.Empty);

    public SQL_ColumnItem Add(string internalName, string caption, VarType format) => Add(NextColumnKey(), internalName, caption, string.Empty, format, string.Empty);

    public SQL_ColumnItem Add(string internalName, string caption, VarType format, string quickinfo) => Add(NextColumnKey(), internalName, caption, string.Empty, format, quickinfo);

    public SQL_ColumnItem Add(string internalName, string caption, string suffix, VarType format) => Add(NextColumnKey(), internalName, caption, suffix, format, string.Empty);

    public SQL_ColumnItem Add(long colKey, string internalName, string caption, string suffix, VarType format, string quickinfo) {
        Database.AddPending(DatabaseDataType.AddColumn, colKey, -1, string.Empty, colKey.ToString(), true);
        // Ruft anschlie�en AddFromParser Auf, der die Spalte endg�ltig dazumacht
        var c = SearchByKey(colKey);
        c.Name = internalName;
        c.Caption = caption;
        if (format != VarType.Unbekannt) { c.SetFormat(format); }
        c.Suffix = suffix;
        c.Quickinfo = quickinfo;
        return c;
    }

    public SQL_ColumnItem? Exists(string columnName) {
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
    /// Gib Spalte 0 zur�ck
    /// </summary>
    /// <returns></returns>
    public SQL_ColumnItem? First() => this[0];

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
        Html da = new(Database.Filename.FileNameWithoutSuffix());
        da.AddCaption("Spaltenliste von: " + Database.Caption);
        da.Add("  <Font face=\"Arial\" Size=\"4\">" + Database.Filename + "</h1><br>");
        da.TableBeginn();
        da.RowBeginn();
        da.CellAdd("#");
        da.CellAdd("Name");
        da.CellAdd("Beschriftung");
        da.CellAdd("�berschriften");
        da.CellAdd("Datenformat");
        da.CellAdd("Quickinfo");
        da.CellAdd("Admin-Info");
        da.CellAdd("Tags");
        da.CellAdd("�nderungs-Rechte");
        da.RowEnd();
        var lfdn = 0;
        foreach (var thisSQL_ColumnItem in Database.Column.Where(thisSQL_ColumnItem => thisSQL_ColumnItem != null)) {
            lfdn++;
            da.RowBeginn();
            da.CellAdd(lfdn.ToString());
            da.CellAdd(thisSQL_ColumnItem.Name);
            da.CellAdd(thisSQL_ColumnItem.Caption.Replace("\r", "<br>"));
            da.CellAdd((thisSQL_ColumnItem.Ueberschrift1 + "/" + thisSQL_ColumnItem.Ueberschrift2 + "/" + thisSQL_ColumnItem.Ueberschrift3 + "/").TrimEnd("/"));
            da.CellAdd(thisSQL_ColumnItem.Format.ToString());
            da.CellAdd(thisSQL_ColumnItem.Quickinfo.Replace("\r", "<br>"));
            da.CellAdd(thisSQL_ColumnItem.AdminInfo.Replace("\r", "<br>"));
            da.CellAdd(thisSQL_ColumnItem.Tags.JoinWith("<br>"));
            da.CellAdd(thisSQL_ColumnItem.PermissionGroupsChangeCell.JoinWith("<br>"));
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
        foreach (var thisSQL_ColumnItem in this.Where(thisSQL_ColumnItem => thisSQL_ColumnItem != null)) {
            switch (thisSQL_ColumnItem.Identifier) {
                case "":
                    break;

                case "System: Locked":
                    SysLocked = thisSQL_ColumnItem;
                    break;

                case "System: Creator":
                    SysRowCreator = thisSQL_ColumnItem;
                    break;

                case "System: Changer":
                    SysRowChanger = thisSQL_ColumnItem;
                    break;

                case "System: Date Created":
                    SysRowCreateDate = thisSQL_ColumnItem;
                    break;

                case "System: Correct":
                    SysCorrect = thisSQL_ColumnItem;
                    break;

                case "System: Date Changed":
                    SysRowChangeDate = thisSQL_ColumnItem;
                    break;

                case "System: Chapter":
                    SysChapter = thisSQL_ColumnItem;
                    break;

                default:
                    Develop.DebugPrint(FehlerArt.Fehler, "Unbekannte Kennung: " + thisSQL_ColumnItem.Identifier);
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
                            base[s2].Load("ColumnName", base[s2].Name + "0");
                        }
                        // Evtl. Doppelte Identifier eleminieren
                        if (!string.IsNullOrEmpty(base[s1].Identifier) && string.Equals(base[s1].Identifier, base[s2].Identifier, StringComparison.OrdinalIgnoreCase)) {
                            base[s2].Load("ColumnIdentify", string.Empty);
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
                // Dummy, um nachfoldgnd nicht abfragen zu m�ssen
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

    public SQL_ColumnItem? SearchByKey(long key) {
        try {
            if (Database == null) { return null; }
            if (key < 0) { return null; } // Evtl. Gel�schte Spalte in irgendeiner Order
            return this.FirstOrDefault(thisColumn => thisColumn != null && thisColumn.Key == key);
        } catch {
            return SearchByKey(key); // Sammlung wurde ver�ndert
        }
    }

    public new void Swap(SQL_ColumnItem? column1, SQL_ColumnItem? column2) {
        if (column1 == null || column2 == null) { return; }
        base.Swap(column1, column2);
        column1.Invalidate_ColumAndContent();
        column2.Invalidate_ColumAndContent();
        Database.ColumnArrangements[0].ShowAllColumns(); // Damit die Datenbank mitbekommt, das sich da was ge�ndert hat
    }

    internal static string ParsableColumnKey(SQL_ColumnItem? column) => column == null ? "ColumnKey=?" : ParsableColumnKey(column.Key);

    internal static string ParsableColumnKey(long key) => "ColumnKey=" + key;

    internal void Add(string columnname, SqlBack sql) {
        var x = new SQL_ColumnItem(Database, -1);

        var l = sql.GetStylDataAll(Database.Filename.FileNameWithoutSuffix(), columnname);
        if (l != null && l.Count > 0) {
            foreach (var thisstyle in l) {
                x.Load(thisstyle.Key, thisstyle.Value);
            }
        }

        base.Add(x);
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

    protected override void Dispose(bool disposing) {
        Database.Disposing -= Database_Disposing;
        Database = null;
        base.Dispose(disposing);
    }

    private void AddSystem(string identifier) {
        if (this.Any(thisColumn => thisColumn != null && string.Equals(thisColumn.Identifier, identifier, StringComparison.OrdinalIgnoreCase))) {
            return;
        }
        var c = Add(identifier);
        c.Load("ColumnIdentify", identifier);
        c.ResetSystemToDefault(true);
    }

    private void Database_Disposing(object sender, System.EventArgs e) => Dispose();

    #endregion
}