// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using BlueDatabase.Interfaces;
using static BlueBasics.IO;

namespace BlueDatabase;

public sealed class ColumnCollection : IEnumerable<ColumnItem>, IDisposableExtended, IHasDatabase {

    #region Fields

    private readonly ConcurrentDictionary<string, ColumnItem> _internal = new();

    #endregion

    #region Constructors

    public ColumnCollection(DatabaseAbstract database) : base() {
        Database = database;
        Database.Disposing += Database_Disposing;
    }

    #endregion

    #region Events

    public event EventHandler<ColumnReasonEventArgs>? ColumnAdded;

    public event EventHandler<ColumnEventArgs>? ColumnInternalChanged;

    public event EventHandler? ColumnRemoved;

    public event EventHandler<ColumnReasonEventArgs>? ColumnRemoving;

    #endregion

    #region Properties

    public int Count => _internal.Count;

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
    //    /// <summary>
    //    /// Gib erste Spalte des ersten Arrangements zurück, die nicht mit "SYS_" beginnt
    //    /// </summary>
    //    /// <returns></returns>

    public bool IsDisposed { get; private set; }

    public ColumnItem? SysChapter { get; private set; }

    public ColumnItem? SysCorrect { get; private set; }

    public ColumnItem? SysLocked { get; private set; }

    /// <summary>
    /// Wichtige Spalte auch für Zeilenskripte:
    /// Vor einem Zeilenskript wird der Status der Zeile geloggt. Wird die Zeile während es Skriptes
    /// verändert, wird das Skript abgebrochen
    /// </summary>
    public ColumnItem? SysRowChangeDate { get; private set; }

    public ColumnItem? SysRowChanger { get; private set; }

    public ColumnItem? SysRowCreateDate { get; private set; }

    public ColumnItem? SysRowCreator { get; private set; }

    public ColumnItem? SysRowState { get; private set; }

    #endregion

    #region Indexers

    public ColumnItem? this[string columnName] {
        get {
            var colum = Exists(columnName);
            if (colum is null) { Database?.DevelopWarnung("Spalte nicht gefunden: " + columnName); }
            return colum;
        }
    }

    #endregion

    #region Methods

    public string Add(ColumnItem column, Reason reason) {
        foreach (var thisc in this) {
            if (thisc.KeyName.Equals(column.KeyName, StringComparison.OrdinalIgnoreCase)) { return "Hinzufügen fehlgeschlagen."; }
        }

        if (!_internal.TryAdd(column.KeyName, column)) { return "Hinzufügen fehlgeschlagen."; }

        GetSystems();

        OnColumnAdded(new ColumnReasonEventArgs(column, reason));
        return string.Empty;
    }

    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    //    foreach (var thisKey in keys) {
    //        Remove(thisKey, comment);
    //    }
    //}
    public ColumnItem? Exists(string? columnName) {
        if (Database == null || Database.IsDisposed || columnName == null || string.IsNullOrEmpty(columnName)) { return null; }
        try {
            columnName = columnName.ToUpper();
            var col = _internal.ContainsKey(columnName) ? _internal[columnName] : null;
            if (col != null && col.IsDisposed) {
                Develop.DebugPrint(FehlerArt.Fehler, "Interner Spaltenfehler, Spalte verworfen: " + columnName);
                return null;
            }
            return col;
        } catch {
            Develop.CheckStackForOverflow();
            return Exists(columnName);
        }
    }

    public ColumnItem? First() {
        // Nicht als Property, weil ansonsten nicht die Function des ENumerators verdeckt wird
        if (Database == null || Database.IsDisposed) { return null; }

        if (Database.ColumnArrangements.Count < 1 || Database.ColumnArrangements[0].Count != Database.Column.Count()) {
            Develop.DebugPrint(FehlerArt.Fehler, "Ansicht 0 fehlerhaft!");
            return null;
        }

        var l = Database?.ColumnArrangements[0]?.FirstOrDefault(thisViewItem => thisViewItem?.Column != null && !thisViewItem.Column.IsDisposed && !thisViewItem.Column.KeyName.StartsWith("SYS_"))?.Column;
        if (l != null) { return l; }

        return Database?.ColumnArrangements[0]?.FirstOrDefault(thisViewItem => thisViewItem?.Column != null && !thisViewItem.Column.IsDisposed)?.Column;
    }

    //public ColumnItem? this[int index] {
    //    get {
    //        if (Database == null || Database.IsDisposed) { return null; }

    //        //var L = new List<string>();

    //        //foreach (var thiscol in this) {
    //        //    L.Add(thiscol.Name);
    //        //}
    //        //L.Sort();

    //        if (index >= 0 && index < _internal.Count) {
    //            return _internal.ElementAt(index).Value;
    //        }

    //        Database.DevelopWarnung("Spalten-Index nicht gefunden: " + index);
    //        return null;
    //    }
    //}
    //public static string ChangeKeysInString(string originalString, int oldKey, int newKey) {
    //    var o = ParsableColumnKey(oldKey);
    //    if (!originalString.Contains(o)) { return originalString; }
    //    var n = ParsableColumnKey(newKey);
    //    if (oldKey == newKey) {
    //        Develop.DebugPrint(FehlerArt.Fehler, "Schlüssel gleich:  " + oldKey);
    //        return originalString;
    //    }
    //    originalString = originalString.Replace(o + "}", n + "}");
    //    originalString = originalString.Replace(o + ",", n + ",");
    //    originalString = originalString.Replace(o + " ", n + " ");
    //    if (originalString.EndsWith(o)) { originalString = originalString.TrimEnd(o) + n; }
    //    if (originalString.Contains(o)) {
    //        Develop.DebugPrint(FehlerArt.Fehler, "String nicht ersetzt: " + originalString);
    //        return originalString;
    //    }
    //    return originalString;
    //}
    //public void Clear(string comment) {
    //    var name = (from thiscolumnitem in _internal.Values where thiscolumnitem != null select thiscolumnitem.Key).Select(dummy => dummy).ToList();
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

    public ColumnItem? GenerateAndAdd(string internalName, string caption, string suffix, IColumnInputFormat format) => GenerateAndAdd(internalName, caption, suffix, format, string.Empty);

    public ColumnItem? GenerateAndAdd(string internalName, string caption, IColumnInputFormat format, string quickinfo) => GenerateAndAdd(internalName, caption, string.Empty, format, quickinfo);

    public ColumnItem? GenerateAndAdd(string internalName, string caption, IColumnInputFormat format) => GenerateAndAdd(internalName, caption, string.Empty, format, string.Empty);

    public ColumnItem? GenerateAndAdd(string internalName) => GenerateAndAdd(internalName, string.Empty, string.Empty, null, string.Empty);

    public ColumnItem? GenerateAndAdd() => GenerateAndAdd(Freename(string.Empty), string.Empty, string.Empty, null, string.Empty);

    public ColumnItem? GenerateAndAdd(string internalName, string caption, string suffix, IColumnInputFormat? format, string quickinfo) {
        if (!ColumnItem.IsValidColumnName(internalName)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Spaltenname nicht erlaubt!");
            return null;
        }

        if (Database == null || Database.IsDisposed) { return null; }

        //var item = SearchByKey(key);
        //if (item != null) {
        //    Develop.DebugPrint(FehlerArt.Fehler, "Schlüssel belegt!");
        //    return null;
        //}
        _ = Database?.ChangeData(DatabaseDataType.Comand_AddColumnByName, null, null, string.Empty, internalName, string.Empty, Generic.UserName, DateTime.UtcNow);
        var item = Exists(internalName);
        if (item == null) {
            Develop.DebugPrint(FehlerArt.Fehler, "Erstellung fehlgeschlagen.");
            return null;
        }

        item.KeyName = internalName;
        item.Caption = caption;

        item.GetStyleFrom(format);
        item.Suffix = suffix;
        item.Quickinfo = quickinfo;
        return item;
    }

    public void GenerateAndAddSystem() {
        string[] w = {
            "SYS_ROWSTATE",
            "SYS_CHAPTER",
            "SYS_DATECHANGED",
            "SYS_CHANGER",
            "SYS_DATECREATED",
            "SYS_CREATOR",
            "SYS_CORRECT",
            "SYS_LOCKED",
        };
        GenerateAndAddSystem(w);
    }

    public void GenerateAndAddSystem(params string[] sysnames) {
        foreach (var thisstring in sysnames) {
            if (Exists(thisstring) == null) {
                GenerateAndAddSystem(thisstring);
            }
        }
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
        foreach (var thisColumnItem in Database.ColumnArrangements[0]) {
            if (thisColumnItem?.Column != null) {
                lfdn++;
                da.RowBeginn();
                da.CellAdd(lfdn.ToString());
                da.CellAdd(thisColumnItem.Column.KeyName);
                da.CellAdd(thisColumnItem.Column.Caption.Replace("\r", "<br>"));
                da.CellAdd((thisColumnItem.Column.CaptionGroup1 + "/" + thisColumnItem.Column.CaptionGroup2 + "/" + thisColumnItem.Column.CaptionGroup3 + "/").TrimEnd("/"));
                var name = "{" + thisColumnItem.Column.Format.ToString() + "}";
                foreach (var thisFormat in FormatHolder.AllFormats) {
                    if (thisFormat.IsFormatIdenticalSoft(thisColumnItem.Column)) { name = thisFormat.Name; }
                }
                da.CellAdd(name + " (" + thisColumnItem.Column.MaxCellLenght + " Char)");

                da.CellAdd(thisColumnItem.Column.Quickinfo.Replace("\r", "<br>"));
                da.CellAdd(thisColumnItem.Column.AdminInfo.Replace("\r", "<br>"));
                da.CellAdd(thisColumnItem.Column.Tags.JoinWith("<br>"));
                da.CellAdd(thisColumnItem.Column.PermissionGroupsChangeCell.JoinWith("<br>"));
                da.RowEnd();
            }
        }

        da.TableEnd();
        da.AddFoot();
        da.Save(TempFile("", "Spaltenliste.html"), true);
    }

    IEnumerator IEnumerable.GetEnumerator() => IEnumerable_GetEnumerator();

    public IEnumerator<ColumnItem> GetEnumerator() => _internal.Values.GetEnumerator();

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
        SysRowState = null;

        foreach (var thisColumnItem in this) {
            if (thisColumnItem != null && thisColumnItem.IsSystemColumn()) {
                switch (thisColumnItem.KeyName.ToUpper()) {
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

                    case "SYS_ROWSTATE":
                        SysRowState = thisColumnItem;
                        break;

                    default:

                        Develop.DebugPrint(FehlerArt.Fehler, "Unbekannte Kennung: " + thisColumnItem.KeyName);
                        break;
                }
            }
        }
    }

    //public int IndexOf(ColumnItem? column) {
    //    if (column == null || column.IsDisposed || Database?.Column == null || Database.IsDisposed) { return -1; }

    //    for (var index = 0; index < _internal.Count; index++) {
    //        if (column == _internal.ElementAt(index).Value) { return index; }
    //    }

    //    Database.DevelopWarnung("Spalten-Index nicht gefunden: " + column.Caption);
    //    return -1;
    //}

    public bool Remove(ColumnItem column, string comment) {
        if (column == null || column.IsDisposed) { return false; }
        return string.IsNullOrEmpty(Database?.ChangeData(DatabaseDataType.Comand_RemoveColumn, column, null, string.Empty, column.KeyName, comment, Generic.UserName, DateTime.UtcNow));
    }

    public void Repair() {
        GetSystems();

        //for (var s1 = 0; s1 < Count; s1++) {
        //    if (this[s1] != null) {
        //        for (var s2 = s1 + 1; s2 < Count; s2++) {
        //            if (this[s2] != null) {
        //                // Evtl. Doppelte Namen einzigartig machen
        //                if (string.Equals(this[s1].Name, this[s2].Name, StringComparison.OrdinalIgnoreCase)) {
        //                    this[s2].Name = this[s2].Name + "0";
        //                }
        //                //// Evtl. Doppelte Identifierx eleminieren
        //                //if (!string.IsNullOrEmpty(base[s1].Identifierx) && string.Equals(base[s1].Identifierx, base[s2].Identifierx, StringComparison.OrdinalIgnoreCase)) {
        //                //    base[s2].Identifierx = string.Empty;
        //                //}
        //            }
        //        }
        //    }
        //}

        //// Reihengolge reparieren
        //var colN = -1;
        //do {
        //    colN++;
        //    if (colN + 2 > Count) { break; }

        //    var thisc = this[colN];

        //    if (thisc == null) {
        //        Swap(colN, colN + 1);
        //        colN = -1;
        //    } else {
        //        var thisc1 = this[colN + 1];
        //        if (thisc1 == null) {
        //            // Dummy, um nachfoldgnd nicht abfragen zu müssen
        //        } else if (thisc.IsSystemColumn() && !thisc1.IsSystemColumn()) {
        //            Swap(colN, colN + 1);
        //            colN = -1;
        //        } else if (thisc.IsSystemColumn() && thisc1.IsSystemColumn()) {
        //            if (w.IndexOf(thisc.Name) > w.IndexOf(thisc1.Name)) {
        //                Swap(colN, colN + 1);
        //                colN = -1;
        //            }
        //        }
        //    }
        //} while (true);

        foreach (var thisColumn in this) {
            thisColumn.Repair();
        }
    }

    internal bool ChangeName(string oldName, string newName) {
        if (oldName == newName) { return true; }
        if (Database == null || Database.IsDisposed) { return false; }

        var ok = _internal.TryRemove(oldName.ToUpper(), out var vcol);
        if (!ok) { return false; }

        ok = _internal.TryAdd(newName.ToUpper(), vcol);
        if (!ok) { return false; }

        ok = Database.Cell.ChangeColumnName(oldName, newName);
        if (!ok) { return false; }
        Database?.RepairColumnArrangements();
        Database?.RepairViews();
        return true;
    }

    internal void CloneFrom(DatabaseAbstract sourceDatabase, bool changeWidth) {
        // Spalten, die zu viel sind, löschen
        var names = new List<ColumnItem>();
        foreach (var thisColumn in this) {
            var l = sourceDatabase.Column.Exists(thisColumn.KeyName);
            if (l == null) { names.Add(thisColumn); }
        }
        foreach (var thisname in names) {
            Remove(thisname, "Clone - Spalte zu viel");
        }

        // Spalten erzeugen und Format übertragen
        foreach (var thisColumn in sourceDatabase.Column) {
            var l = Exists(thisColumn.KeyName) ??
                GenerateAndAdd(thisColumn.KeyName, thisColumn.Caption, thisColumn.Suffix, null, thisColumn.Quickinfo);

            if (l != null) {
                l.CloneFrom(thisColumn, true, changeWidth);

                if (l.KeyName != thisColumn.KeyName) {
                    Develop.DebugPrint(FehlerArt.Fehler, "Name nicht korrekt!");
                }

                //if (l.KeyName != thisColumn.KeyName) {
                //    Develop.DebugPrint(FehlerArt.Fehler, "KeyName nicht korrekt!");
                //}
            } else {
                Develop.DebugPrint(FehlerArt.Fehler, "Spalte nicht erzeugt!");
            }
        }
    }

    internal void OnColumnRemoving(ColumnReasonEventArgs e) {
        e.Column.Changed -= OnColumnChanged;
        ColumnRemoving?.Invoke(this, e);
    }

    internal string SetValueInternal(DatabaseDataType type, Reason reason, string name) {
        if (Database == null || Database.IsDisposed) { return "Datenbank verworfen!"; }
        //if (key is null or < 0) { return "Schlüsselfehler"; }

        //if (type == DatabaseDataType.Comand_AddColumnByKey) {
        //    var c = SearchByKey(key);
        //    if (c != null && !c.IsDisposed) { return "Bereits vorhanden!"; }

        //    c = new ColumnItem(Database, (long)key);
        //    _ = Add(c);

        //    if (!isLoading) {
        //        Database.RepairColumnArrangements();
        //        Database.RepairViews();
        //    }

        //    return string.Empty;
        //}

        if (type == DatabaseDataType.Comand_AddColumnByName) {
            var c = Exists(name);
            if (c != null && !c.IsDisposed) { return "Bereits vorhanden!"; }

            c = new ColumnItem(Database, name);
            _ = Add(c, reason);

            if (reason != Reason.LoadReload) {
                Database.RepairColumnArrangements();
                Database.RepairViews();
            }

            return string.Empty;
        }

        if (type == DatabaseDataType.Comand_RemoveColumn) {
            var c = Exists(name);
            if (c == null) { return "Spalte nicht gefunden!"; }

            OnColumnRemoving(new ColumnReasonEventArgs(c, reason));
            if (!_internal.TryRemove(name.ToUpper(), out _)) { return "Löschen nicht erfolgreich"; }
            OnColumnRemoved();

            if (reason != Reason.LoadReload) {
                Database.RepairColumnArrangements();
                Database.RepairViews();
            }

            return string.Empty;
        }

        return "Befehl unbekannt";
    }

    private void Database_Disposing(object sender, System.EventArgs e) => Dispose();

    private void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
            }
            if (Database != null && !Database.IsDisposed) { Database.Disposing -= Database_Disposing; }
            Database = null;
            _internal.Clear();
            // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
            // TODO: Große Felder auf NULL setzen
            IsDisposed = true;
        }
    }

    private void GenerateAndAddSystem(string sysname) {
        var c = Exists(sysname);

        if (sysname == "SYS_DATECHANGED" && c == null) { c = Exists("SYS_CHANGEDATE"); }
        if (sysname == "SYS_DATECREATED" && c == null) { c = Exists("SYS_CREATEDATE"); }

        if (c != null && !c.IsDisposed) {
            c.KeyName = sysname; // Wegen der Namensverbiegung oben...
            c.ResetSystemToDefault(false);
            return;
        }

        c = GenerateAndAdd(sysname.ToUpper());
        c?.ResetSystemToDefault(true);
    }

    private IEnumerator IEnumerable_GetEnumerator() => _internal.Values.GetEnumerator();

    private void OnColumnAdded(ColumnReasonEventArgs e) {
        e.Column.Changed += OnColumnChanged;
        ColumnAdded?.Invoke(this, e);
    }

    private void OnColumnChanged(object sender, System.EventArgs e) => ColumnInternalChanged?.Invoke(this, new ColumnEventArgs((ColumnItem)sender));

    private void OnColumnRemoved() => ColumnRemoved?.Invoke(this, System.EventArgs.Empty);

    #endregion
}