// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueTable.EventArgs;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using static BlueBasics.ClassesStatic.IO;

namespace BlueTable.Classes;

public sealed class ColumnCollection : IEnumerable<ColumnItem>, IDisposableExtended, IHasTable {

    #region Fields

    private readonly ConcurrentDictionary<string, ColumnItem> _internal = new(StringComparer.OrdinalIgnoreCase);
    private volatile int _isDisposedFlag;

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
    //    if (column.Table != Table) { Develop.DebugError("Parent-Tabellen unterschiedlich!"); }
    //    if (Contains(column)) { Develop.DebugError("Spalte bereits vorhanden!"); }
    //    base.GenerateAndAdd(column);
    //}
    //    /// <summary>
    //    /// Gib erste Spalte des ersten Arrangements zurück, die nicht mit "SYS_" beginnt
    //    /// </summary>
    //    /// <returns></returns>
    public bool IsDisposed => _isDisposedFlag == 1;

    public ColumnItem? SysCellNote { get; private set; }
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

    public ColumnItem? SysRowKey { get; private set; }

    public ColumnItem? SysRowState { get; private set; }

    public Table? Table {
        get;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == field) { return; }

            field?.DisposingEvent -= _table_Disposing;
            field = value;

            field?.DisposingEvent += _table_Disposing;
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
                    Develop.DebugError("Interner Spaltenfehler, Spalte verworfen: " + keyName);
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

    public void DisableAllEditing() {
        if (Table is not { IsDisposed: false } tb) { return; }
        foreach (var thisColumn in tb.Column) {
            thisColumn.EditableWithTextInput = false;
            thisColumn.EditableWithDropdown = false;
            thisColumn.EditAllowedDespiteLock = false;
        }
    }

    public void Dispose() => Dispose(true);

    public ColumnItem? GenerateAndAdd(string keyName, string caption, IColumnInputFormat format) => GenerateAndAdd(keyName, caption, format, string.Empty);

    public ColumnItem? GenerateAndAdd(string keyName) => GenerateAndAdd(keyName, string.Empty, null, string.Empty);

    public ColumnItem? GenerateAndAdd() => GenerateAndAdd(Freename(string.Empty), string.Empty, null, string.Empty);

    public ColumnItem? GenerateAndAdd(string keyName, string caption, IColumnInputFormat? format, string quickinfo) {
        if (!ColumnItem.IsValidColumnKey(keyName)) {
            Develop.DebugError("Spaltenname (Schlüssel) nicht erlaubt!");
            return null;
        }

        if (IsDisposed || Table is not { IsDisposed: false }) { return null; }

        Table?.ChangeData(TableDataType.Command_AddColumnByName, null, string.Empty, keyName);
        var item = this[keyName];
        if (item is null) {
            Develop.DebugError("Erstellung fehlgeschlagen.");
            return null;
        }

        item.KeyName = keyName;
        item.Caption = caption;

        item.GetStyleFrom(format);
        item.QuickInfo = quickinfo;
        return item;
    }

    public void GenerateAndAddSystem() {
        string[] w = [
            SystemColumnKeys.RowKey,
            SystemColumnKeys.RowState,
            SystemColumnKeys.DateChanged,
            SystemColumnKeys.Changer,
            SystemColumnKeys.DateCreated,
            SystemColumnKeys.Creator,
            SystemColumnKeys.Correct,
            SystemColumnKeys.Locked
        ];
        GenerateAndAddSystem(w);
    }

    public void GenerateAndAddSystem(params string[] sysnames) {
        foreach (var thisstring in sysnames) {
            if (this[thisstring] is null) {
                GenerateAndAddSystem(thisstring);
            }
        }
    }

    public void GenerateOverView() {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }
        var da = new Html(tb.KeyName);
        da.AddCaption("Spaltenliste von: " + tb.Caption);
        da.Add("  <Font face=\"Arial\" Size=\"4\">" + tb.KeyName + "</h1><br>");
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
            if (thisColumnItem is not null) {
                lfdn++;
                da.RowBeginn();
                da.CellAdd(lfdn.ToString1());
                da.CellAdd(thisColumnItem.KeyName);
                da.CellAdd(thisColumnItem.Caption.Replace("\r", "<br>"));
                da.CellAdd((thisColumnItem.CaptionGroup1 + "/" + thisColumnItem.CaptionGroup2 + "/" + thisColumnItem.CaptionGroup3 + "/").TrimEnd('/'));
                var name = string.Empty;
                foreach (var thisFormat in FormatHolder.AllFormats.Instances) {
                    if (thisFormat.IsFormatIdenticalSoft(thisColumnItem)) { name = thisFormat.KeyName; }
                }
                da.CellAdd(name + " (" + thisColumnItem.MaxCellLength + " Char)");
                da.CellAdd(thisColumnItem.QuickInfo.Replace("\r", "<br>"));
                da.CellAdd(thisColumnItem.AdminInfo.Replace("\r", "<br>"));
                da.CellAdd(string.Join("<br>", thisColumnItem.ColumnTags));
                da.CellAdd(string.Join("<br>", thisColumnItem.PermissionGroupsChangeCell));
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
        SysCellNote = null;
        SysLocked = null;
        SysRowCreateDate = null;
        SysRowCreator = null;
        SysCorrect = null;
        SysRowChanger = null;
        SysRowChangeDate = null;
        SysRowKey = null;
        SysRowState = null;
        ChunkValueColumn = null;
        First = null;

        foreach (var thisColumnItem in this) {
            if (thisColumnItem is not null) {
                if (thisColumnItem.Value_for_Chunk != ChunkType.None && Table is TableChunk) { ChunkValueColumn = thisColumnItem; }
                if (thisColumnItem.IsFirst) { First = thisColumnItem; }

                if (thisColumnItem.IsSystemColumn()) {
                    switch (thisColumnItem.KeyName.ToUpperInvariant()) {
                        case SystemColumnKeys.RowColor_Obsolete: // TODO: Entfernen, 12.01.2026
                            break;

                        case SystemColumnKeys.CellNote:
                            SysCellNote = thisColumnItem;
                            break;

                        case SystemColumnKeys.Locked:
                            SysLocked = thisColumnItem;
                            break;

                        case SystemColumnKeys.Creator:
                            SysRowCreator = thisColumnItem;
                            break;

                        case SystemColumnKeys.Changer:
                            SysRowChanger = thisColumnItem;
                            break;

                        case SystemColumnKeys.DateCreated:
                            SysRowCreateDate = thisColumnItem;
                            break;

                        case SystemColumnKeys.Correct:
                            SysCorrect = thisColumnItem;
                            break;

                        case SystemColumnKeys.DateChanged:
                            SysRowChangeDate = thisColumnItem;
                            break;

                        case SystemColumnKeys.Chapter_Obsolete: // TODO: Entfernen, 09.01.2026
                            break;

                        case SystemColumnKeys.RowState:
                            SysRowState = thisColumnItem;
                            break;

                        case SystemColumnKeys.RowKey:
                            SysRowKey = thisColumnItem;
                            break;

                        default:

                            Develop.DebugError("Unbekannte Kennung: " + thisColumnItem.KeyName);
                            break;
                    }
                }
            }
        }
    }

    public bool Remove(ColumnItem column, string comment) => !column.IsDisposed
                                                            && string.IsNullOrEmpty(Table?.ChangeData(TableDataType.Command_RemoveColumn, column, null, string.Empty, column.KeyName, Generic.UserName, DateTime.UtcNow, comment));

    public void RemoveObsoleteColumns(IEnumerable<ColumnItem> posssibleObsoelte, HashSet<string> stillUsed, Reason reason) {
        if (IsDisposed || Table is not { IsDisposed: false }) { return; }

        var colsToRemove = posssibleObsoelte.Where(c => !c.IsDisposed && !stillUsed.Contains(c.KeyName)).ToList();
        if (colsToRemove.Count > 0) {
            foreach (var col in colsToRemove) {
                ExecuteCommand(TableDataType.Command_RemoveColumn, col.KeyName, reason);
            }
        }
    }

    //    Table.DevelopWarnung("Spalten-Index nicht gefunden: " + column.Caption);
    //    return -1;
    //}
    public void Repair() {
        if (Table is not { IsDisposed: false } tb) { return; }
        if (!string.IsNullOrEmpty(tb.IsValueEditable(TableDataType.ColumnArrangement, TableChunk.Chunk_Master))) { return; }

        GetSystems();
        //for (var s1 = 0; s1 < Count; s1++) {
        //    if (this[s1] is not null) {
        //        for (var s2 = s1 + 1; s2 < Count; s2++) {
        //            if (this[s2] is not null) {
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

        //    if (thisc is null) {
        //        Swap(colN, colN + 1);
        //        colN = -1;
        //    } else {
        //        var thisc1 = this[colN + 1];
        //        if (thisc1 is null) {
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

    internal string ChangeKey(string oldKey, string newKey) {
        if (oldKey == newKey) { return string.Empty; }
        if (IsDisposed || Table is not { IsDisposed: false }) { return "Tabelle verworfen"; }

        var ok = _internal.TryRemove(oldKey.ToUpperInvariant(), out var value);
        if (!ok || value is null) { return "Entfernen fehlgeschlagen"; }

        ok = _internal.TryAdd(newKey.ToUpperInvariant(), value);
        if (!ok) { return "Hinzufügen fehlgeschlagen"; }

        return string.Empty;
    }

    internal void CopyTo(ColumnCollection target) {
        var names = new List<ColumnItem>();
        foreach (var targetColumn in target) {
            if (this[targetColumn.KeyName] is null) { names.Add(targetColumn); }
        }
        foreach (var n in names) {
            target.Remove(n, "CopyTo - Spalte zu viel");
        }

        foreach (var sourceColumn in this) {
            var l = target[sourceColumn.KeyName] ??
                target.GenerateAndAdd(sourceColumn.KeyName, sourceColumn.Caption, null, sourceColumn.QuickInfo);

            if (l is not null) {
                sourceColumn.CopyTo(l, true);

                if (l.KeyName != sourceColumn.KeyName) {
                    Develop.DebugError("Name nicht korrekt!");
                }
            } else {
                Develop.DebugError("Spalte nicht erzeugt!");
            }
        }
    }

    internal string ExecuteCommand(TableDataType type, string name, Reason reason) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return "Tabelle verworfen!"; }

        if (!reason.HasFlag(Reason.IgnoreFreeze)) {
            if (tb.AcquireWriteAccess(type) is { Length: > 0 } f) { return f; }
        }

        if (type == TableDataType.Command_AddColumnByName) {
            var column = this[name];
            if (column is { IsDisposed: false }) { return string.Empty; }//"Spalte " + name + " bereits vorhanden!"

            column = new ColumnItem(Table, name);

            if (this[column.KeyName] is not null) { return "Hinzufügen fehlgeschlagen."; }
            if (!_internal.TryAdd(column.KeyName.ToUpperInvariant(), column)) { return "Hinzufügen fehlgeschlagen."; }

            GetSystems();

            if (reason.HasFlag(Reason.RaiseEvents)) { OnColumnAdded(new ColumnEventArgs(column)); }

            if (reason.HasFlag(Reason.LogUndo) && tb.LogUndo) {
                Generic.Pause(0.001, false); // um in den Logs den Zeitstempel richtig zu haben
            }

            return string.Empty;
        }

        if (type == TableDataType.Command_RemoveColumn) {
            var c = this[name];
            if (c is null) { return "Spalte nicht gefunden!"; }

            if (reason.HasFlag(Reason.RaiseEvents)) { OnColumnRemoving(new ColumnEventArgs(c)); }
            if (!_internal.TryRemove(name.ToUpperInvariant(), out _)) { return "Löschen nicht erfolgreich"; }
            if (reason.HasFlag(Reason.RaiseEvents)) { OnColumnRemoved(); }

            c.Dispose();
            GetSystems();

            return string.Empty;
        }

        return "Befehl unbekannt";
    }

    private void _table_Disposing(object? sender, System.EventArgs e) => Dispose();

    private void Column_DisposingEvent(object? sender, System.EventArgs e) {
        if (sender is ColumnItem c) {
            c.DisposingEvent -= Column_DisposingEvent;
            _internal.TryRemove(c.KeyName.ToUpperInvariant(), out _);
            OnColumnDisposed(new ColumnEventArgs(c));
            //Remove(c, "Disposing");
        }
    }

    private void Dispose(bool disposing) {
        if (Interlocked.CompareExchange(ref _isDisposedFlag, 1, 0) != 0) { return; }

        if (disposing) {
            ColumnAdded = null;
            ColumnDisposed = null;
            ColumnPropertyChanged = null;
            ColumnRemoved = null;
            ColumnRemoving = null;

            Table = null;

            foreach (var kvp in _internal) {
                kvp.Value?.Dispose();
            }
        }

        _internal.Clear();
    }

    private string Freename(string preferedName) {
        preferedName = preferedName.ReduceToChars(Constants.AllowedCharsVariableName);
        if (string.IsNullOrEmpty(preferedName)) { preferedName = "NewColumn"; }

        if (this[preferedName] is null) { return preferedName; }

        string testName;
        var nr = 0;
        do {
            nr++;
            testName = preferedName + "_" + nr;
        } while (this[testName] is not null);
        return testName;
    }

    private void GenerateAndAddSystem(string sysname) {
        var c = this[sysname];

        if (sysname == SystemColumnKeys.DateChanged && c is null) { c = this[SystemColumnKeys.ChangeDate_Alt]; }
        if (sysname == SystemColumnKeys.DateCreated && c is null) { c = this[SystemColumnKeys.CreateDate_Alt]; }

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

    private void OnColumnPropertyChanged(object? sender, System.EventArgs e) {
        if (sender is ColumnItem ci) { ColumnPropertyChanged?.Invoke(this, new ColumnEventArgs(ci)); }
    }

    private void OnColumnRemoved() => ColumnRemoved?.Invoke(this, System.EventArgs.Empty);

    private void OnColumnRemoving(ColumnEventArgs e) {
        e.Column.PropertyChanged -= OnColumnPropertyChanged;
        e.Column.DisposingEvent -= Column_DisposingEvent;
        ColumnRemoving?.Invoke(this, e);
    }

    #endregion
}