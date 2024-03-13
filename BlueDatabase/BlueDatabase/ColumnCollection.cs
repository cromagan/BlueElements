// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using BlueDatabase.Interfaces;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static BlueBasics.IO;

namespace BlueDatabase;

public sealed class ColumnCollection : IEnumerable<ColumnItem>, IDisposableExtended, IHasDatabase {

    #region Fields

    private readonly ConcurrentDictionary<string, ColumnItem> _internal = [];

    private Database? _database;

    #endregion

    #region Constructors

    public ColumnCollection(Database database) : base() => Database = database;

    #endregion

    #region Events

    public event EventHandler<ColumnEventArgs>? ColumnAdded;

    public event EventHandler<ColumnEventArgs>? ColumnDisposed;

    public event EventHandler<ColumnEventArgs>? ColumnInternalChanged;

    public event EventHandler? ColumnRemoved;

    public event EventHandler<ColumnEventArgs>? ColumnRemoving;

    #endregion

    #region Properties

    public int Count => _internal.Count;

    public Database? Database {
        get => _database;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == _database) { return; }

            if (_database != null) {
                _database.DisposingEvent -= _database_Disposing;
            }
            _database = value;

            if (_database != null) {
                _database.DisposingEvent += _database_Disposing;
            }
        }
    }

    ///// <summary>
    ///// Diese Routine sollte nur bei einem Reload benutzt werden. AddPending wir nicht mehr ausgel�st.
    ///// </summary>
    ///// <param name="column"></param>
    ///// <returns></returns>
    //public void AddFromParser(ColumnItem? column) {
    //    if (column.Database != Database) { Develop.DebugPrint(FehlerArt.Fehler, "Parent-Datenbanken unterschiedlich!"); }
    //    if (Contains(column)) { Develop.DebugPrint(FehlerArt.Fehler, "Spalte bereits vorhanden!"); }
    //    base.GenerateAndAdd(column);
    //}
    //    /// <summary>
    //    /// Gib erste Spalte des ersten Arrangements zur�ck, die nicht mit "SYS_" beginnt
    //    /// </summary>
    //    /// <returns></returns>

    public bool IsDisposed { get; private set; }

    public ColumnItem? SysChapter { get; private set; }

    public ColumnItem? SysCorrect { get; private set; }

    public ColumnItem? SysLocked { get; private set; }

    /// <summary>
    /// Wichtige Spalte auch f�r Zeilenskripte:
    /// Vor einem Zeilenskript wird der Status der Zeile geloggt. Wird die Zeile w�hrend es Skriptes
    /// ver�ndert, wird das Skript abgebrochen
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
            if (IsDisposed || Database is not Database db || db.IsDisposed || columnName == null || string.IsNullOrEmpty(columnName)) { return null; }

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
                return this[columnName];
            }
        }
    }

    #endregion

    #region Methods

    public void Dispose() => Dispose(true);

    public ColumnItem? First() {
        // Nicht als Property, weil ansonsten nicht die Function des ENumerators verdeckt wird
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return null; }

        if (db.ColumnArrangements.Count < 1 || db.ColumnArrangements[0].Count != db.Column.Count()) {
            //Develop.DebugPrint(FehlerArt.Fehler, "Ansicht 0 fehlerhaft!");
            return null;
        }

        var l = db.ColumnArrangements[0]?.FirstOrDefault(thisViewItem => thisViewItem?.Column != null && !thisViewItem.Column.IsDisposed && !thisViewItem.Column.KeyName.StartsWith("SYS_"))?.Column;
        if (l != null) { return l; }

        return db.ColumnArrangements[0]?.FirstOrDefault(thisViewItem => thisViewItem?.Column != null && !thisViewItem.Column.IsDisposed)?.Column;
    }

    public ColumnItem? GenerateAndAdd(string internalName, string caption, IColumnInputFormat format, string quickinfo) => GenerateAndAdd(internalName, caption, string.Empty, format, quickinfo);

    public ColumnItem? GenerateAndAdd(string internalName, string caption, IColumnInputFormat format) => GenerateAndAdd(internalName, caption, string.Empty, format, string.Empty);

    public ColumnItem? GenerateAndAdd(string internalName) => GenerateAndAdd(internalName, string.Empty, string.Empty, null, string.Empty);

    public ColumnItem? GenerateAndAdd() => GenerateAndAdd(Freename(string.Empty), string.Empty, string.Empty, null, string.Empty);

    public ColumnItem? GenerateAndAdd(string internalName, string caption, string suffix, IColumnInputFormat? format, string quickinfo) {
        if (!ColumnItem.IsValidColumnName(internalName)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Spaltenname nicht erlaubt!");
            return null;
        }

        if (IsDisposed || Database is not Database db || db.IsDisposed) { return null; }

        //var item = SearchByKey(key);
        //if (item != null) {
        //    Develop.DebugPrint(FehlerArt.Fehler, "Schl�ssel belegt!");
        //    return null;
        //}
        _ = Database?.ChangeData(DatabaseDataType.Command_AddColumnByName, null, null, string.Empty, internalName, Generic.UserName, DateTime.UtcNow, string.Empty);
        var item = this[internalName];
        if (item == null) {
            Develop.DebugPrint(FehlerArt.Fehler, "Erstellung fehlgeschlagen.");
            return null;
        }

        item.KeyName = internalName;
        item.Caption = caption;

        item.GetStyleFrom(format);
        item.Suffix = suffix;
        item.QuickInfo = quickinfo;
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
            "SYS_LOCKED"
        };
        GenerateAndAddSystem(w);
    }

    public void GenerateAndAddSystem(params string[] sysnames) {
        foreach (var thisstring in sysnames) {
            if (this[thisstring] == null) {
                GenerateAndAddSystem(thisstring);
            }
        }
    }

    public void GenerateOverView() {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }
        Html da = new(Database.TableName);
        da.AddCaption("Spaltenliste von: " + Database.Caption);
        da.Add("  <Font face=\"Arial\" Size=\"4\">" + Database.TableName + "</h1><br>");
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
        foreach (var thisColumnItem in db.ColumnArrangements[0]) {
            if (thisColumnItem?.Column != null) {
                lfdn++;
                da.RowBeginn();
                da.CellAdd(lfdn.ToString());
                da.CellAdd(thisColumnItem.Column.KeyName);
                da.CellAdd(thisColumnItem.Column.Caption.Replace("\r", "<br>"));
                da.CellAdd((thisColumnItem.Column.CaptionGroup1 + "/" + thisColumnItem.Column.CaptionGroup2 + "/" + thisColumnItem.Column.CaptionGroup3 + "/").TrimEnd("/"));
                var name = "{" + thisColumnItem.Column.Function + "}";
                foreach (var thisFormat in FormatHolder.AllFormats) {
                    if (thisFormat.IsFormatIdenticalSoft(thisColumnItem.Column)) { name = thisFormat.Name; }
                }
                da.CellAdd(name + " (" + thisColumnItem.Column.MaxCellLenght + " Char)");

                da.CellAdd(thisColumnItem.Column.QuickInfo.Replace("\r", "<br>"));
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

    public bool Remove(ColumnItem column, string comment) {
        if (column.IsDisposed) { return false; }
        return string.IsNullOrEmpty(Database?.ChangeData(DatabaseDataType.Command_RemoveColumn, column, null, string.Empty, column.KeyName, Generic.UserName, DateTime.UtcNow, comment));
    }

    //    Database.DevelopWarnung("Spalten-Index nicht gefunden: " + column.Caption);
    //    return -1;
    //}
    public void Repair() {
        GetSystems();
        if (!string.IsNullOrEmpty(Database.EditableErrorReason(Database, EditableErrorReasonType.EditAcut)) || Database is null) { return; }

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
        //            // Dummy, um nachfoldgnd nicht abfragen zu m�ssen
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

    //    for (var index = 0; index < _internal.Count; index++) {
    //        if (column == _internal.ElementAt(index).Value) { return index; }
    //    }
    internal bool ChangeName(string oldName, string newName) {
        if (oldName == newName) { return true; }
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return false; }

        var ok = _internal.TryRemove(oldName.ToUpper(), out var vcol);
        if (!ok) { return false; }

        ok = _internal.TryAdd(newName.ToUpper(), vcol);
        if (!ok) { return false; }

        ok = Database.Cell.ChangeColumnName(oldName, newName);
        if (!ok) { return false; }
        Database?.RepairColumnArrangements(Reason.SetCommand);
        //Database?.RepairViews();
        return true;
    }

    /// <summary>
    /// Setzt die fest vermerkten Spalten zur�ck und durchsucht die Spalten nach dem Identifier.
    /// Es werden nur die gefunden Spalten gemerkt - keine neuen erstellt!
    /// </summary>
    //public int IndexOf(ColumnItem? column) {
    //    if (column == null || column.IsDisposed || Database?.Column == null || Database.IsDisposed) { return -1; }
    internal void CloneFrom(Database sourceDatabase) {
        // Spalten, die zu viel sind, l�schen
        var names = new List<ColumnItem>();
        foreach (var thisColumn in this) {
            var l = sourceDatabase.Column[thisColumn.KeyName];
            if (l == null) { names.Add(thisColumn); }
        }
        foreach (var thisname in names) {
            Remove(thisname, "Clone - Spalte zu viel");
        }

        // Spalten erzeugen und Format �bertragen
        foreach (var thisColumn in sourceDatabase.Column) {
            var l = this[thisColumn.KeyName] ??
                GenerateAndAdd(thisColumn.KeyName, thisColumn.Caption, thisColumn.Suffix, null, thisColumn.QuickInfo);

            if (l != null) {
                l.CloneFrom(thisColumn, true);

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

    internal string ExecuteCommand(DatabaseDataType type, string name, Reason reason) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return "Datenbank verworfen!"; }

        if (type == DatabaseDataType.Command_AddColumnByName) {
            var c = this[name];
            if (c != null && !c.IsDisposed) { return string.Empty; }//"Spalte " + name + " bereits vorhanden!"

            c = new ColumnItem(Database, name);
            var f = Add(c);
            if (!string.IsNullOrEmpty(f)) { return f; }

            if (reason == Reason.SetCommand) {
                Generic.Pause(0.001, false); // um in den Logs den Zeitstempel richtig zu haben
            }

            if (reason is not Reason.InitialLoad and not Reason.UpdateChanges) {
                // Wichtig! NICHT bei LoadReload - da werden ja noch weitere Spalten erstellt
                Database.RepairColumnArrangements(reason);
            }

            return string.Empty;
        }

        if (type == DatabaseDataType.Command_RemoveColumn) {
            var c = this[name];
            if (c == null) { return "Spalte nicht gefunden!"; }

            OnColumnRemoving(new ColumnEventArgs(c));
            if (!_internal.TryRemove(name.ToUpper(), out _)) { return "L�schen nicht erfolgreich"; }
            OnColumnRemoved();

            if (reason is not Reason.InitialLoad and not Reason.UpdateChanges) {
                // Wichtig! Nicht bei LoadReload, da werden evtl. noch weitere Spalten modifiziert

                Database.RepairColumnArrangements(reason);
                //Database.RepairViews();
            }
            c.Dispose();
            GetSystems();

            return string.Empty;
        }

        return "Befehl unbekannt";
    }

    internal bool HasKeyColumns() {
        foreach (var thisColumn in _internal) {
            if (thisColumn.Value.Function == ColumnFunction.Schl�sselspalte_NurDatenpr�fung) {
                return true;
            }
        }
        return false;
    }

    private void _database_Disposing(object sender, System.EventArgs e) => Dispose();

    private string Add(ColumnItem column) {
        if (this[column.KeyName] != null) { return "Hinzuf�gen fehlgeschlagen."; }

        if (!_internal.TryAdd(column.KeyName, column)) { return "Hinzuf�gen fehlgeschlagen."; }

        GetSystems();

        OnColumnAdded(new ColumnEventArgs(column));
        return string.Empty;
    }

    //public ColumnItem? this[int index] {
    //    get {
    //        if (Database is not Database db || Database.IsDisposed) { return null; }

    //        //var L = new List<string>();

    //        //foreach (var thiscol in this) {
    //        //    L.Add(thiscol.Name);
    //        //}
    //        //L.Sort();

    private void Column_DisposingEvent(object sender, System.EventArgs e) {
        if (sender is ColumnItem c) {
            c.DisposingEvent -= Column_DisposingEvent;
            _internal.TryRemove(c.KeyName.ToUpper(), out _);
            OnColumnDisposed(new ColumnEventArgs(c));
            //Remove(c, "Disposing");
        }
    }

    //        if (index >= 0 && index < _internal.Count) {
    //            return _internal.ElementAt(index).Value;
    //        }
    private void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
                Database = null;
                foreach (var thisc in _internal) { thisc.Value.Dispose(); }
            }

            _internal.Clear();
            // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer �berschreiben
            // TODO: Gro�e Felder auf NULL setzen
            IsDisposed = true;
        }
    }

    //        Database.DevelopWarnung("Spalten-Index nicht gefunden: " + index);
    //        return null;
    //    }
    //}
    //public static string ChangeKeysInString(string originalString, int oldKey, int newKey) {
    //    var o = ParsableColumnKey(oldKey);
    //    if (!originalString.Contains(o)) { return originalString; }
    //    var n = ParsableColumnKey(newKey);
    //    if (oldKey == newKey) {
    //        Develop.DebugPrint(FehlerArt.Fehler, "Schl�ssel gleich:  " + oldKey);
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
    private string Freename(string preferedName) {
        preferedName = preferedName.ReduceToChars(Constants.AllowedCharsVariableName);
        if (string.IsNullOrEmpty(preferedName)) { preferedName = "NewColumn"; }

        if (this[preferedName] == null) { return preferedName; }

        string testName;
        var nr = 0;
        do {
            nr++;
            testName = preferedName + "_" + nr;
        } while (this[testName] != null);
        return testName;
    }

    private void GenerateAndAddSystem(string sysname) {
        var c = this[sysname];

        if (sysname == "SYS_DATECHANGED" && c == null) { c = this["SYS_CHANGEDATE"]; }
        if (sysname == "SYS_DATECREATED" && c == null) { c = this["SYS_CREATEDATE"]; }

        if (c != null && !c.IsDisposed) {
            c.KeyName = sysname; // Wegen der Namensverbiegung oben...
            c.ResetSystemToDefault(false);
            return;
        }

        c = GenerateAndAdd(sysname.ToUpper());
        c?.ResetSystemToDefault(true);
    }

    private IEnumerator IEnumerable_GetEnumerator() => _internal.Values.GetEnumerator();

    private void OnColumnAdded(ColumnEventArgs e) {
        e.Column.PropertyChanged += OnColumnPropertyChanged;
        e.Column.DisposingEvent += Column_DisposingEvent;
        ColumnAdded?.Invoke(this, e);
    }

    private void OnColumnDisposed(ColumnEventArgs e) => ColumnDisposed?.Invoke(this, e);

    private void OnColumnPropertyChanged(object sender, System.EventArgs e) => ColumnInternalChanged?.Invoke(this, new ColumnEventArgs((ColumnItem)sender));

    private void OnColumnRemoved() => ColumnRemoved?.Invoke(this, System.EventArgs.Empty);

    private void OnColumnRemoving(ColumnEventArgs e) {
        e.Column.PropertyChanged -= OnColumnPropertyChanged;
        e.Column.DisposingEvent -= Column_DisposingEvent;
        ColumnRemoving?.Invoke(this, e);
    }

    #endregion
}