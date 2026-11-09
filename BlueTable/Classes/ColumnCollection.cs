// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueTable.EventArgs;
using System.Collections;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace BlueTable.Classes;

public sealed class ColumnCollection : IEnumerable<ColumnItem>, IDisposableExtended, IHasTable, IFragmentStoreable {

    #region Enums

    /// <summary>
    /// Pfad-Typen der Spaltensammlung im binären Fragment-Format.
    /// Nur der Sammlung selbst bekannt.
    /// </summary>
    private enum ColumnCollectionFragmentType : byte {

        /// <summary>
        /// Pfad-Segment einer Spalten-Änderung (Key = ColumnKey).
        /// </summary>
        SubColumn = 90,

        /// <summary>
        /// ListKeyObject-Satz: die Spalten folgen als Liste eine Ebene tiefer,
        /// zwischen ihnen je ein Trenner zwei Ebenen unter der Liste.
        /// </summary>
        Columns = 91
    }

    #endregion

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

    public event EventHandler? Disposed;

    public event EventHandler<FragmentChangedEventArgs>? FragmentChanged;

    #endregion

    #region Properties

    public ColumnItem? ChunkValueColumn { get; private set; }

    public int Count => _internal.Count;

    public ColumnItem? First { get; private set; }

    public bool IsDisposed => _isDisposedFlag == 1;

    public ColumnItem? SysCorrect { get; private set; }

    public ColumnItem? SysLocked { get; private set; }

    /// <summary>
    /// Wichtige Spalte auch für Zeilenskripte:
    /// Vor einem Zeilenskript wird der Status der Zeile geloggt. Wird die Zeile während es Skriptes
    /// verändert, wird das Skript abgebrochen
    /// </summary>
    public ColumnItem? SysRowChangeDate { get; private set; }

    public ColumnItem? SysRowChanger { get; private set; }

    public ColumnItem? SysRowChangeComment { get; private set; }

    public ColumnItem? SysRowCreateDate { get; private set; }

    public ColumnItem? SysRowCreator { get; private set; }

    public ColumnItem? SysRowCreateComment { get; private set; }

    public ColumnItem? SysRowKey { get; private set; }

    public ColumnItem? SysRowSortIndex { get; private set; }

    public ColumnItem? SysRowState { get; private set; }

    public Table? Table {
        get;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == field) { return; }

            field?.Disposed -= _table_Disposed;
            field = value;

            field?.Disposed += _table_Disposed;
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

    /// <summary>
    /// Liefert alle nicht verworfenen Spalten, die in mindestens einer Unique-Definition der Tabelle enthalten sind.
    /// </summary>
    public HashSet<ColumnItem> ColumnsUsedInUniqueDefinition() {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return []; }
        return [.. tb.UniqueValues.SelectMany(uvd => uvd.KeyColumns).Where(kc => kc is { IsDisposed: false })];
    }

    /// <summary>
    /// Verarbeitet den Spalten-Block: Der ColumnList-Satz liest die Spalten als
    /// Liste; je Segment benennt der ColumnKey-Satz die Spalte.
    /// </summary>
    public bool DeserializeThis(Stream stream, FragmentRecord record) {
        if (IsDisposed) { return false; }

        switch ((ColumnCollectionFragmentType)record.LeafType) {
            case ColumnCollectionFragmentType.Columns:
                // Spalten als Liste: je Segment eine Spalte (ColumnKey-Satz zuerst)
                return FragmentStoreableExtension.DeserializeList(stream, record.Depth, ColumnCreateOrGet) is not null;

            default:
                // Unbekannter Blatt-Typ — Satz überspringen
                return true;
        }
    }

    /// <summary>
    /// Liefert die Spalte zum ColumnKey des ersten Segment-Satzes; fehlende Spalten
    /// werden erzeugt. Der Key geht an die Nachbereitung (verwaiste Spalten entfernen).
    /// </summary>
    private ColumnItem? ColumnCreateOrGet(FragmentRecord first) {
        Table?.FragmentLoadedColumns.Add(first.StringContent);

        if (this[first.StringContent] is { IsDisposed: false } column) { return column; }

        ExecuteCommand(TableDataType.Command_AddColumnByName, first.StringContent, ChangeFlags.IgnoreFreeze);
        return this[first.StringContent];
    }

    public void DisableAllEditing() {
        if (Table is not { IsDisposed: false } tb) { return; }
        foreach (var thisColumn in tb.Column) {
            thisColumn.DisableAllEditing();
        }
    }

    public void Dispose() => Dispose(true);

    public ColumnItem? GenerateAndAdd(string keyName, string caption, IColumnInputFormat format) => GenerateAndAdd(keyName, caption, format, string.Empty);

    public ColumnItem? GenerateAndAdd(string keyName) => GenerateAndAdd(keyName, string.Empty, null, string.Empty);

    public ColumnItem? GenerateAndAdd() => GenerateAndAdd(Freename(string.Empty), string.Empty, null, string.Empty);

    public ColumnItem? GenerateAndAdd(string keyName, string caption, IColumnInputFormat? format, string quickinfo) {
        if (!ColumnItem.IsValidColumnKey(keyName)) {
            Develop.DebugError($"Spaltenname (Schlüssel) nicht erlaubt: '{keyName}'");
            return null;
        }

        if (IsDisposed || Table is not { IsDisposed: false }) { return null; }

        var result = Table?.ChangeData(TableDataType.Command_AddColumnByName, null, string.Empty, keyName);
        var item = this[keyName];
        if (item is null) {
            Develop.DebugError($"Erstellung fehlgeschlagen: '{keyName}' – ChangeData: '{result}'");
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
            SystemColumnKeys.ChangeComment,
            SystemColumnKeys.DateCreated,
            SystemColumnKeys.Creator,
            SystemColumnKeys.CreateComment,
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

    IEnumerator IEnumerable.GetEnumerator() => IEnumerable_GetEnumerator();

    public IEnumerator<ColumnItem> GetEnumerator() => _internal.Values.GetEnumerator();

    public void GetSystems() {
        SysLocked = null;
        SysRowCreateDate = null;
        SysRowCreator = null;
        SysRowCreateComment = null;
        SysCorrect = null;
        SysRowChanger = null;
        SysRowChangeDate = null;
        SysRowChangeComment = null;
        SysRowKey = null;
        SysRowSortIndex = null;
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

                        case SystemColumnKeys.CreateComment:
                            SysRowCreateComment = thisColumnItem;
                            break;

                        case SystemColumnKeys.Correct:
                            SysCorrect = thisColumnItem;
                            break;

                        case SystemColumnKeys.DateChanged:
                            SysRowChangeDate = thisColumnItem;
                            break;

                        case SystemColumnKeys.ChangeComment:
                            SysRowChangeComment = thisColumnItem;
                            break;

                        case SystemColumnKeys.Chapter_Obsolete: // TODO: Entfernen, 09.01.2026
                            break;

                        case SystemColumnKeys.RowState:
                            SysRowState = thisColumnItem;
                            break;

                        case SystemColumnKeys.RowKey:
                            SysRowKey = thisColumnItem;
                            break;

                        case SystemColumnKeys.RowSortIndex:
                            SysRowSortIndex = thisColumnItem;
                            break;

                        default:

                            Develop.DebugError("Unbekannte Kennung: " + thisColumnItem.KeyName);
                            break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Löst das FragmentChanged-Event mit den fertigen Argumenten aus (Pendant zu OnPropertyChangedExt).
    /// </summary>
    public void OnFragmentChanged(FragmentChangedEventArgs e) {
        if (IsDisposed) { return; }

        FragmentChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Reicht eine Spalten-Änderung mit dem eigenen Pfad-Segment (SubColumn, ColumnKey der Spalte) eine Ebene höher.
    /// </summary>
    public void ChildFragmentChanged(object? sender, FragmentChangedEventArgs e) {
        OnFragmentChanged(new FragmentChangedEventArgs(this, FragmentChangedEventArgs.Crumbs(ColumnCollectionFragmentType.SubColumn, FragmentChangedEventArgs.CrumbKey(sender), e.RelativePath), e.TypeSelector, e.ContentType, e.Content) { ListContent = e.ListContent });
    }

    public bool Remove(ColumnItem column, string comment) => !column.IsDisposed
                                                                                && string.IsNullOrEmpty(Table?.ChangeData(TableDataType.Command_RemoveColumn, column, null, string.Empty, column.KeyName, UserName, DateTime.UtcNow, comment, ChangeFlags.UserCommand));

    public void RemoveObsoleteColumns(IEnumerable<ColumnItem> posssibleObsoelte, HashSet<string> stillUsed, ChangeFlags reason) {
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

    /// <summary>
    /// Schreibt alle Spalten als Liste in der Speicherreihenfolge: ein ColumnList-
    /// Satz, darunter je Spalte der Spalten-Block eine Ebene tiefer (ColumnKey zuerst).
    /// </summary>
    public bool SerializeCore(Stream stream, int depth) {
        if (IsDisposed) { return true; }

        // SaveOrder statt Collection-Enumeration: Die Enumerationsordnung des
        // ConcurrentDictionary ist hash-basiert und instabil; gespeichert wird
        // in der Classic-Reihenfolge (Ansicht 1, danach alphabetisch).
        var saveColumns = Table is { IsDisposed: false } st ? st.ColumnsInSaveOrder() : [.. this];
        saveColumns.RemoveAll(column => column is not { IsDisposed: false });

        return FragmentRecord.Append(stream, depth, ColumnCollectionFragmentType.Columns, saveColumns);
    }

    /// <summary>
    /// Ergänzt die Chunk-Spalte zur Spaltenliste, falls vorhanden und nicht enthalten.
    /// Ohne die Chunk-Spalte liefern Duplikat-/Unique-Suchen bei gechunkten Tabellen keine Treffer.
    /// </summary>
    internal static List<ColumnItem> ColumnsIncludingChunk(Table tb, IEnumerable<ColumnItem> columns) {
        if (tb.Column.ChunkValueColumn is { IsDisposed: false } chk && !columns.Contains(chk)) {
            return [.. columns, chk];
        }
        return columns.ToList();
    }

    /// <summary>
    /// Verkettet die Zellwerte einer Zeile über alle gegebenen Spalten mit dem gegebenen Trennzeichen.
    /// </summary>
    internal static string ValuesKey(RowItem row, IEnumerable<ColumnItem> columns, char separator) {
        var sb = new System.Text.StringBuilder();
        foreach (var column in columns) {
            sb.Append(row.CellGetStringCore(column)).Append(separator);
        }
        return sb.ToString();
    }

    internal string ChangeKey(string oldKey, string newKey) {
        if (oldKey == newKey) { return string.Empty; }
        if (IsDisposed || Table is not { IsDisposed: false }) { return "Tabelle verworfen"; }

        // Zielspalte existiert bereits: Beim Nachspielen von Fragmenten möglich, wenn
        // der Hauptfile-Stand die Umbenennung schon enthält, das Fragment die Kette
        // inklusive Neu-Anlage der Quellspalte aber erneut abspielt. Die bestehende
        // Zielspalte ist die gültige; die Quellspalte ist ein leeres
        // Wiedereinspiel-Artefakt (Zelldaten liegen in den Zeilen, nicht in der
        // Spalte) und wird entfernt. Damit ist die Umbenennung idempotent.
        if (this[newKey] is { IsDisposed: false }) {
            if (_internal.TryRemove(oldKey.ToUpperInvariant(), out var artifact) && artifact is { IsDisposed: false }) {
                artifact.Dispose();
            }
            return string.Empty;
        }

        var ok = _internal.TryRemove(oldKey.ToUpperInvariant(), out var value);
        if (!ok || value is null) { return string.Empty; }

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

        // SaveOrder statt Collection-Enumeration: Fügt das Ziel in derselben
        // Reihenfolge ein wie ein Ladevorgang, damit die (Hash-basierte)
        // Enumerationsordnung der ConcurrentDictionary identisch bleibt.
        var sourceColumns = Table is { IsDisposed: false } st ? st.ColumnsInSaveOrder() : [.. this];

        foreach (var sourceColumn in sourceColumns) {
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

    internal string ExecuteCommand(TableDataType type, string name, ChangeFlags reason) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return "Tabelle verworfen!"; }

        if (!reason.HasFlag(ChangeFlags.IgnoreFreeze)) {
            if (tb.IsValueEditable(type, string.Empty) is { Length: > 0 } f) { return f; }
        }

        if (type == TableDataType.Command_AddColumnByName) {
            var column = this[name];
            if (column is { IsDisposed: false }) { return string.Empty; }//"Spalte " + name + " bereits vorhanden!"

            column = new ColumnItem(Table, name);

            if (this[column.KeyName] is not null) { return "Hinzufügen fehlgeschlagen."; }
            if (!_internal.TryAdd(column.KeyName.ToUpperInvariant(), column)) { return "Hinzufügen fehlgeschlagen."; }

            // Spalte als Fragment-Kind verdrahten (Crumb = SubColumn, Key = ColumnKey)
            this.Register(column);

            GetSystems();

            if (reason.HasFlag(ChangeFlags.RaiseEvents)) { OnColumnAdded(new ColumnEventArgs(column)); }

            return string.Empty;
        }

        if (type == TableDataType.Command_RemoveColumn) {
            var c = this[name];
            if (c is null) { return "Spalte nicht gefunden!"; }

            if (reason.HasFlag(ChangeFlags.RaiseEvents)) { OnColumnRemoving(new ColumnEventArgs(c)); }
            if (!_internal.TryRemove(name.ToUpperInvariant(), out _)) { return "Löschen nicht erfolgreich"; }

            this.Unregister(c);

            if (reason.HasFlag(ChangeFlags.RaiseEvents)) { OnColumnRemoved(); }

            c.Dispose();
            GetSystems();

            return string.Empty;
        }

        return "Befehl unbekannt";
    }

    private void _table_Disposed(object? sender, System.EventArgs e) => Dispose();

    private void Column_Disposed(object? sender, System.EventArgs e) {
        if (sender is ColumnItem c) {
            c.Disposed -= Column_Disposed;
            this.Unregister(c);
            _internal.TryRemove(c.KeyName.ToUpperInvariant(), out _);
            OnColumnDisposed(new ColumnEventArgs(c));
            //Remove(c, "Disposing");
        }
    }

    private void Dispose(bool disposing) {
        if (Interlocked.CompareExchange(ref _isDisposedFlag, 1, 0) != 0) { return; }

        if (disposing) {
            OnDisposed();
            Disposed = null;
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
        preferedName = preferedName.ReduceToChars(AllowedCharsVariableName);
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
        e.Column.Disposed += Column_Disposed;
        ColumnAdded?.Invoke(this, e);
    }

    private void OnColumnDisposed(ColumnEventArgs e) => ColumnDisposed?.Invoke(this, e);

    private void OnColumnPropertyChanged(object? sender, System.EventArgs e) {
        if (sender is ColumnItem ci) { ColumnPropertyChanged?.Invoke(this, new ColumnEventArgs(ci)); }
    }

    private void OnColumnRemoved() => ColumnRemoved?.Invoke(this, System.EventArgs.Empty);

    private void OnColumnRemoving(ColumnEventArgs e) {
        e.Column.PropertyChanged -= OnColumnPropertyChanged;
        e.Column.Disposed -= Column_Disposed;
        ColumnRemoving?.Invoke(this, e);
    }

    private void OnDisposed() => Disposed?.Invoke(this, System.EventArgs.Empty);

    #endregion
}