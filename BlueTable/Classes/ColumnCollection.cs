// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using BlueTable.Enums;
using BlueTable.EventArgs;
using BlueTable.Interfaces;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using static BlueBasics.IO;

namespace BlueTable;

public sealed class ColumnCollection : IEnumerable<ColumnItem>, IDisposableExtended, IHasTable {

    #region Fields

    private readonly ConcurrentDictionary<string, ColumnItem> _internal = [];

    #endregion

    #region Constructors

    public ColumnCollection(Table table) : base() => Table = table;

    #endregion

    #region Events

    public event EventHandler<ColumnEventArgs>? ColumnAdded;

    public event EventHandler<ColumnEventArgs>? ColumnDisposed;

    public event EventHandler<ColumnEventArgs>? ColumnPropertyChanged;

    public event EventHandler? ColumnRemoved;

    public event EventHandler<ColumnEventArgs>? ColumnRemoving;

    #endregion

    #region Properties

    public ColumnItem? ChunkValueColumn { get; private set; }
    public int Count => _internal.Count;

    public ColumnItem? First { get; private set; }

    ///// <summary>
    ///// Diese Routine sollte nur bei einem Reload benutzt werden. AddPending wir nicht mehr ausgelöst.
    ///// </summary>
    ///// <param name="column"></param>
    ///// <returns></returns>
    //public void AddFromParser(ColumnItem? column) {
    //    if (column.Table != Table) { Develop.DebugPrint(ErrorType.Error, "Parent-Tabellen unterschiedlich!"); }
    //    if (Contains(column)) { Develop.DebugPrint(ErrorType.Error, "Spalte bereits vorhanden!"); }
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

    public ColumnItem? SysRowColor { get; private set; }

    public ColumnItem? SysRowCreateDate { get; private set; }

    public ColumnItem? SysRowCreator { get; private set; }

    public ColumnItem? SysRowState { get; private set; }

    public Table? Table {
        get;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == field) { return; }

            if (field != null) {
                field.DisposingEvent -= _table_Disposing;
            }
            field = value;

            if (field != null) {
                field.DisposingEvent += _table_Disposing;
            }
        }
    }

    #endregion

    #region Indexers

    public ColumnItem? this[string keyName] {
        get {
            if (IsDisposed || Table is not { IsDisposed: false } || string.IsNullOrEmpty(keyName)) { return null; }

            try {
                keyName = keyName.ToUpperInvariant();
                var col = _internal.TryGetValue(keyName, out var value) ? value : null;
                if (col is { IsDisposed: true }) {
                    Develop.DebugPrint(ErrorType.Error, "Interner Spaltenfehler, Spalte verworfen: " + keyName);
                    return null;
                }
                return col;
            } catch {
                Develop.AbortAppIfStackOverflow();
                return this[keyName];
            }
        }
    }

    #endregion

    #region Methods

    public void Dispose() => Dispose(true);

    //public ColumnItem? First() {
    //    // Nicht als Property, weil ansonsten nicht die Function des ENumerators verdeckt wird
    //    if (IsDisposed || Table is not { IsDisposed: false } db) { return null; }

    //    if (db.Column.Count == 0) { return null; }

    //    foreach (var thisC in db.Column) {
    //        if (thisC is { IsDisposed: false, IsFirst: true }) { return thisC; }
    //    }

    //    if (string.IsNullOrEmpty(db.ColumnArrangements)) { return null; }

    //    var i = db.ColumnArrangements.IndexOf("ColumnName[J]", StringComparison.Ordinal);

    //    if (i < 5) { return null; }

    //    var en = db.ColumnArrangements.IndexOf("[", i + 12, StringComparison.Ordinal);

    //    if (en <= i) { return null; }

    //    var n = db.ColumnArrangements.Substring(i + 13, en - i - 13);

    //    if (n.StartsWith("SYS_")) { return null; }

    //    //var l = db.ColumnArrangements[0]?.FirstOrDefault(thisViewItem => thisViewItem?.Column is { IsDisposed: false } && !thisViewItem.Column.KeyName.StartsWith("SYS_"))?.Column;
    //    //if (l != null) { return l; }

    //    //return db.ColumnArrangements[0]?.FirstOrDefault(thisViewItem => thisViewItem?.Column is { IsDisposed: false })?.Column;

    //    return this[n];
    //}

    public ColumnItem? GenerateAndAdd(string keyName, string caption, IColumnInputFormat format) => GenerateAndAdd(keyName, caption, format, string.Empty);

    public ColumnItem? GenerateAndAdd(string keyName) => GenerateAndAdd(keyName, string.Empty, null, string.Empty);

    public ColumnItem? GenerateAndAdd() => GenerateAndAdd(Freename(string.Empty), string.Empty, null, string.Empty);

    public ColumnItem? GenerateAndAdd(string keyName, string caption, IColumnInputFormat? format, string quickinfo) {
        if (!ColumnItem.IsValidColumnName(keyName)) {
            Develop.DebugPrint(ErrorType.Error, "Spaltenname nicht erlaubt!");
            return null;
        }

        if (IsDisposed || Table is not { IsDisposed: false }) { return null; }

        //var item = GetByKey(key);
        //if (item != null) {
        //    Develop.DebugPrint(ErrorType.Error, "Schlüssel belegt!");
        //    return null;
        //}

        Table?.ChangeData(TableDataType.Command_AddColumnByName, null, string.Empty, keyName);
        var item = this[keyName];
        if (item == null) {
            Develop.DebugPrint(ErrorType.Error, "Erstellung fehlgeschlagen.");
            return null;
        }

        item.KeyName = keyName;
        item.Caption = caption;

        item.GetStyleFrom(format);
        item.ColumnQuickInfo = quickinfo;
        return item;
    }

    public void GenerateAndAddSystem() {
        string[] w = [
            "SYS_ROWSTATE",
            "SYS_CHAPTER",
            "SYS_ROWCOLOR",
            "SYS_DATECHANGED",
            "SYS_CHANGER",
            "SYS_DATECREATED",
            "SYS_CREATOR",
            "SYS_CORRECT",
            "SYS_LOCKED"
        ];
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
        if (IsDisposed || Table is not { IsDisposed: false } db) { return; }
        Html da = new(db.KeyName);
        da.AddCaption("Spaltenliste von: " + db.Caption);
        da.Add("  <Font face=\"Arial\" Size=\"4\">" + db.KeyName + "</h1><br>");
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
        foreach (var thisColumnItem in this) {
            if (thisColumnItem != null) {
                lfdn++;
                da.RowBeginn();
                da.CellAdd(lfdn.ToString());
                da.CellAdd(thisColumnItem.KeyName);
                da.CellAdd(thisColumnItem.Caption.Replace("\r", "<br>"));
                da.CellAdd((thisColumnItem.CaptionGroup1 + "/" + thisColumnItem.CaptionGroup2 + "/" + thisColumnItem.CaptionGroup3 + "/").TrimEnd("/"));
                var name = string.Empty;
                foreach (var thisFormat in FormatHolder.AllFormats) {
                    if (thisFormat.IsFormatIdenticalSoft(thisColumnItem)) { name = thisFormat.Name; }
                }
                da.CellAdd(name + " (" + thisColumnItem.MaxCellLength + " Char)");
                da.CellAdd(thisColumnItem.ColumnQuickInfo.Replace("\r", "<br>"));
                da.CellAdd(thisColumnItem.AdminInfo.Replace("\r", "<br>"));
                da.CellAdd(thisColumnItem.ColumnTags.JoinWith("<br>"));
                da.CellAdd(thisColumnItem.PermissionGroupsChangeCell.JoinWith("<br>"));
                da.RowEnd();
            }
        }

        da.TableEnd();
        da.AddFoot();
        da.Save(TempFile(string.Empty, "Spaltenliste.html"), true);
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
        SysRowColor = null;
        ChunkValueColumn = null;
        First = null;

        foreach (var thisColumnItem in this) {
            if (thisColumnItem != null) {
                if (thisColumnItem.Value_for_Chunk != ChunkType.None && Table is TableChunk) { ChunkValueColumn = thisColumnItem; }
                if (thisColumnItem.IsFirst) { First = thisColumnItem; }

                if (thisColumnItem.IsSystemColumn()) {
                    switch (thisColumnItem.KeyName.ToUpperInvariant()) {
                        case "SYS_ROWCOLOR":
                            SysRowColor = thisColumnItem;
                            break;

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

                            Develop.DebugPrint(ErrorType.Error, "Unbekannte Kennung: " + thisColumnItem.KeyName);
                            break;
                    }
                }
            }
        }
    }

    public bool Remove(ColumnItem column, string comment) => !column.IsDisposed
                                                            && string.IsNullOrEmpty(Table?.ChangeData(TableDataType.Command_RemoveColumn, column, null, string.Empty, column.KeyName, Generic.UserName, DateTime.UtcNow, comment, string.Empty, string.Empty));

    //    Table.DevelopWarnung("Spalten-Index nicht gefunden: " + column.Caption);
    //    return -1;
    //}
    public void Repair() {
        if (Table is not { IsDisposed: false } tb) { return; }
        if (!tb.IsEditable(false)) { return; }

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

    //    for (var index = 0; index < _internal.Count; index++) {
    //        if (column == _internal.ElementAt(index).Value) { return index; }
    //    }
    internal string ChangeName(string oldName, string newName) {
        if (oldName == newName) { return string.Empty; }
        if (IsDisposed || Table is not { IsDisposed: false }) { return "Tabelle verworfen"; }

        var ok = _internal.TryRemove(oldName.ToUpperInvariant(), out var vcol);
        if (!ok) { return "Entfernen fehlgeschlagen"; }

        ok = _internal.TryAdd(newName.ToUpperInvariant(), vcol);
        if (!ok) { return "Hinzufügen fehlgeschlagen"; }

        ok = Table.Cell.ChangeColumnName(oldName, newName);
        if (!ok) { return "Namensänderung fehlgeschlagen"; }
        //Table?.RepairColumnArrangements(Reason.SetCommand);
        return string.Empty;
    }

    internal void CloneFrom(Table sourceTable) {
        // Spalten, die zu viel sind, löschen
        var names = new List<ColumnItem>();
        foreach (var thisColumn in this) {
            var l = sourceTable.Column[thisColumn.KeyName];
            if (l == null) { names.Add(thisColumn); }
        }
        foreach (var thisname in names) {
            Remove(thisname, "Clone - Spalte zu viel");
        }

        // Spalten erzeugen und Format übertragen
        foreach (var thisColumn in sourceTable.Column) {
            var l = this[thisColumn.KeyName] ??
                GenerateAndAdd(thisColumn.KeyName, thisColumn.Caption, null, thisColumn.ColumnQuickInfo);

            if (l != null) {
                l.CloneFrom(thisColumn, true);

                if (l.KeyName != thisColumn.KeyName) {
                    Develop.DebugPrint(ErrorType.Error, "Name nicht korrekt!");
                }

                //if (l.KeyName != thisColumn.KeyName) {
                //    Develop.DebugPrint(ErrorType.Error, "KeyName nicht korrekt!");
                //}
            } else {
                Develop.DebugPrint(ErrorType.Error, "Spalte nicht erzeugt!");
            }
        }
    }

    internal string ExecuteCommand(TableDataType type, string name, Reason reason) {
        if (IsDisposed || Table is not { IsDisposed: false } db) { return "Tabelle verworfen!"; }

        if (type == TableDataType.Command_AddColumnByName) {
            var c = this[name];
            if (c is { IsDisposed: false }) { return string.Empty; }//"Spalte " + name + " bereits vorhanden!"

            c = new ColumnItem(Table, name);
            var f = Add(c);
            if (!string.IsNullOrEmpty(f)) { return f; }

            if (reason == Reason.SetCommand && db.LogUndo) {
                Generic.Pause(0.001, false); // um in den Logs den Zeitstempel richtig zu haben
            }

            //if (reason is not Reason.NoUndo_NoInvalidate and not Reason.UpdateChanges) {
            //    // Wichtig! NICHT bei LoadReload - da werden ja noch weitere Spalten erstellt
            //    Table.RepairColumnArrangements(reason);
            //}

            return string.Empty;
        }

        if (type == TableDataType.Command_RemoveColumn) {
            var c = this[name];
            if (c == null) { return "Spalte nicht gefunden!"; }

            OnColumnRemoving(new ColumnEventArgs(c));
            if (!_internal.TryRemove(name.ToUpperInvariant(), out _)) { return "Löschen nicht erfolgreich"; }
            OnColumnRemoved();

            //if (reason is not Reason.NoUndo_NoInvalidate and not Reason.UpdateChanges) {
            //    // Wichtig! Nicht bei LoadReload, da werden evtl. noch weitere Spalten modifiziert

            //    Table.RepairColumnArrangements(reason);
            //    //Table.RepairViews();
            //}
            c.Dispose();
            GetSystems();

            return string.Empty;
        }

        return "Befehl unbekannt";
    }

    private void _table_Disposing(object sender, System.EventArgs e) => Dispose();

    private string Add(ColumnItem column) {
        if (this[column.KeyName] != null) { return "Hinzufügen fehlgeschlagen."; }

        if (!_internal.TryAdd(column.KeyName.ToUpperInvariant(), column)) { return "Hinzufügen fehlgeschlagen."; }

        GetSystems();

        OnColumnAdded(new ColumnEventArgs(column));
        return string.Empty;
    }

    //public ColumnItem? this[int index] {
    //    get {
    //        if (Table is not Table db || Table.IsDisposed) { return null; }

    //        //var L = new List<string>();

    //        //foreach (var thiscol in this) {
    //        //    L.Add(thiscol.Name);
    //        //}
    //        //L.Sort();

    private void Column_DisposingEvent(object sender, System.EventArgs e) {
        if (sender is ColumnItem c) {
            c.DisposingEvent -= Column_DisposingEvent;
            _internal.TryRemove(c.KeyName.ToUpperInvariant(), out _);
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
                // Alle externen Event-Handler abmelden
                ColumnAdded = null;
                ColumnDisposed = null;
                ColumnPropertyChanged = null;
                ColumnRemoved = null;
                ColumnRemoving = null;

                // Table Events sicher abmelden
                Table = null;

                // Alle Columns dispose und deren Events abmelden
                foreach (var kvp in _internal) {
                    if (kvp.Value != null) {
                        kvp.Value.PropertyChanged -= OnColumnPropertyChanged;
                        kvp.Value.DisposingEvent -= Column_DisposingEvent;
                        kvp.Value.Dispose();
                    }
                }
            }

            _internal.Clear();
            IsDisposed = true;
        }
    }

    //        Table.DevelopWarnung("Spalten-Index nicht gefunden: " + index);
    //        return null;
    //    }
    //}
    //public static string ChangeKeysInString(string originalString, int oldKey, int newKey) {
    //    var o = ParsableColumnKey(oldKey);
    //    if (!originalString.Contains(o)) { return originalString; }
    //    var n = ParsableColumnKey(newKey);
    //    if (oldKey == newKey) {
    //        Develop.DebugPrint(ErrorType.Error, "Schlüssel gleich:  " + oldKey);
    //        return originalString;
    //    }
    //    originalString = originalString.Replace(o + "}", n + "}");
    //    originalString = originalString.Replace(o + ",", n + ",");
    //    originalString = originalString.Replace(o + " ", n + " ");
    //    if (originalString.EndsWith(o)) { originalString = originalString.TrimEnd(o) + n; }
    //    if (originalString.Contains(o)) {
    //        Develop.DebugPrint(ErrorType.Error, "String nicht ersetzt: " + originalString);
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

        if (c is { IsDisposed: false }) {
            c.KeyName = sysname; // Wegen der Namensverbiegung oben...
            c.ResetSystemToDefault(false);
            return;
        }

        c = GenerateAndAdd(sysname.ToUpperInvariant());
        c?.ResetSystemToDefault(true);
    }

    private IEnumerator IEnumerable_GetEnumerator() => _internal.Values.GetEnumerator();

    private void OnColumnAdded(ColumnEventArgs e) {
        e.Column.PropertyChanged += OnColumnPropertyChanged;
        e.Column.DisposingEvent += Column_DisposingEvent;
        ColumnAdded?.Invoke(this, e);
    }

    private void OnColumnDisposed(ColumnEventArgs e) => ColumnDisposed?.Invoke(this, e);

    private void OnColumnPropertyChanged(object sender, System.EventArgs e) => ColumnPropertyChanged?.Invoke(this, new ColumnEventArgs((ColumnItem)sender));

    private void OnColumnRemoved() => ColumnRemoved?.Invoke(this, System.EventArgs.Empty);

    private void OnColumnRemoving(ColumnEventArgs e) {
        e.Column.PropertyChanged -= OnColumnPropertyChanged;
        e.Column.DisposingEvent -= Column_DisposingEvent;
        ColumnRemoving?.Invoke(this, e);
    }

    #endregion
}