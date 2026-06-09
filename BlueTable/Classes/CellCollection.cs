// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Collections.Concurrent;
using System.Threading;

namespace BlueTable.Classes;

public sealed class CellCollection : IDisposableExtended, IHasTable {

    #region Fields

    private readonly ConcurrentDictionary<(ColumnItem column, RowItem row), CellItem> _internal = new();
    private volatile int _isDisposedFlag;

    #endregion

    #region Constructors

    public CellCollection(Table table) => Table = table;

    #endregion

    #region Destructors

    ~CellCollection() {
        Dispose(false);
    }

    #endregion

    #region Properties

    public int Count => _internal.Count;

    public bool IsDisposed => _isDisposedFlag == 1;

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

    /// <summary>
    /// Gibt die Zelle zurück.
    /// Da leere Zellen nicht gespeichert werden, kann null zuück gebeben werden, obwohl die Zelle an sich gültig ist.
    /// </summary>
    public CellItem? this[ColumnItem? column, RowItem? row] {
        get {
            if (IsDisposed || Table is not { IsDisposed: false } tb) { return null; }
            if (column is not { IsDisposed: false }) { return null; }
            if (row is not { IsDisposed: false }) { return null; }

            if (column.Table != row.Table || column.Table != tb) { return null; }

            return _internal.TryGetValue((column, row), out var cell) ? cell : null;
        }
    }

    #endregion

    #region Methods

    /// <summary>
    ///
    /// </summary>
    /// <param name="inputRow"></param>
    /// <param name="varcol">Wird eine Collection angegeben, werden zuerst diese Werte benutzt - falls vorhanden - anstelle des Wertes in der Zeile </param>
    /// <param name="linkedTable"></param>
    /// <param name="inputColumn"></param>
    /// <returns></returns>
    public static OperationResult GetFilterFromLinkedCellData(Table? linkedTable, ColumnItem inputColumn, RowItem? inputRow, VariableCollection? varcol) {
        if (linkedTable is not { IsDisposed: false }) { return OperationResult.Failed("Verlinkte Tabelle verworfen."); }
        if (inputColumn.Table is not { IsDisposed: false } || inputColumn.IsDisposed) { return OperationResult.Failed("Tabelle verworfen."); }

        return TryBuildLinkedCellFilterItems(linkedTable, inputColumn, inputRow, varcol);
    }

    public static (FilterCollection? fc, string info) GetFilterReverse(ColumnItem mycolumn, ColumnItem linkedcolumn, RowItem linkedrow) {
        if (linkedcolumn.Table is not { IsDisposed: false } ltb || linkedcolumn.IsDisposed) { return (null, "Tabelle verworfen."); }

        if (mycolumn.RelationType != RelationType.CellValues) { return (null, "Falsches Format."); }

        if (mycolumn.Table is not { IsDisposed: false } tb) { return (null, "Tabelle verworfen."); }

        var fc = new FilterCollection(tb, "cell get reverse filter");

        foreach (var thisFi in mycolumn.LinkedCellFilter) {
            if (!thisFi.Contains('|')) { return (null, "Veraltetes Filterformat"); }

            var x = thisFi.SplitBy("|");
            var c = ltb.Column[x[0]];
            if (c is null) { return (null, "Eine Spalte, nach der gefiltert werden soll, existiert nicht."); }

            if (x[1] != "=") { return (null, "Nur 'Gleich'-Filter wird unterstützt."); }

            var value = x[2].FromNonCritical().ToUpperInvariant();
            if (string.IsNullOrEmpty(value)) { return (null, "Leere Suchwerte werden nicht unterstützt."); }

            foreach (var thisColumn in tb.Column) {
                if (value.Contains($"~{thisColumn.KeyName}~", StringComparison.OrdinalIgnoreCase)) {
                    var l = linkedrow.CellGetList(c);
                    if (l.Count == 0) { l.Add(string.Empty); }
                    fc.Add(new FilterItem(thisColumn, FilterType.Istgleich_ODER_GroßKleinEgal, l));
                }
            }
        }

        var er = fc.ErrorReason();
        if (!string.IsNullOrEmpty(er)) {
            fc.Dispose();
            return (null, er);
        }

        if (fc.Count == 0) {
            fc.Add(new FilterItem(tb, "Reverse Filter"));
        }

        return (fc, string.Empty);
    }

    /// <summary>
    /// Gibt einen Fehlergrund zurück, ob die Zelle bearbeitet werden kann.
    /// </summary>
    public static string IsCellEditable(ColumnItem? column, RowItem? row, string? newChunkValue) {
        if (column?.Table is not { IsDisposed: false } tb) { return "Es ist keine Spalte ausgewählt."; }

        if (row is { IsDisposed: true }) { return "Die Zeile wurde verworfen."; }

        var oldChunk = newChunkValue;

        if (!column.EditableWithTextInput && !column.EditableWithDropdown && !tb.PowerEdit) {
            return "Die Inhalte dieser Spalte können nicht manuell bearbeitet werden, da keine Bearbeitungsmethode erlaubt ist.";
        }

        if (ColumnItem.UserEditDialogTypeInTable(column, false, true) == EditTypeTable.None) {
            return "Interner Programm-Fehler: Es ist keine Bearbeitungsmethode für die Spalte definiert.";
        }

        if (row is null) {
            if (tb.Column.First is not { IsDisposed: false } firstcol || firstcol != column) {
                return "Neue Zeilen müssen mit der ersten Spalte beginnen.";
            }

            if (!tb.PermissionCheck(tb.PermissionGroupsNewRow, null)) {
                return "Sie haben nicht die nötigen Rechte, um neue Zeilen anzulegen.";
            }

            if (tb.Column.ChunkValueColumn is { } cvc && newChunkValue is not null) {
                if (cvc != tb.Column.First && string.IsNullOrEmpty(newChunkValue)) { return "Chunk-Wert fehlt."; }
            }
        } else {
            if (!tb.PowerEdit && tb.Column.SysLocked is not null) {
                if (column != tb.Column.SysLocked && row.CellGetBoolean(tb.Column.SysLocked) && !column.EditAllowedDespiteLock) {
                    return "Da die Zeile als abgeschlossen markiert ist, kann die Zelle nicht bearbeitet werden.";
                }
            }
            oldChunk = row.ChunkValue;
        }

        if (!tb.PermissionCheck(column.PermissionGroupsChangeCell, row)) {
            return "Sie haben nicht die nötigen Rechte, um diesen Wert zu ändern.";
        }

        var f = tb.IsGenericEditable(false);
        if (!string.IsNullOrEmpty(f)) { return $"Tabellensperre: {f}"; }

        if (column.RelationType == RelationType.CellValues) {
            if (row is null) { return "Verlinkungs-Fehler"; }

            var (lcolumn, lrow, info, canrepair) = row.LinkedCellData(column, false, false);

            if (!string.IsNullOrEmpty(info) && !canrepair) { return info; }

            if (lcolumn?.Table is not { IsDisposed: false } tb2) { return "Verknüpfte Tabelle verworfen."; }

            tb2.PowerEdit = tb.PowerEdit;

            if (lrow is not null) {
                var tmp = IsCellEditable(lcolumn, lrow, lrow.ChunkValue);
                return !string.IsNullOrEmpty(tmp) ? "Die verlinkte Zelle kann nicht bearbeitet werden: " + tmp : string.Empty;
            }

            if (canrepair) { return string.Empty; }

            return "Allgemeiner Fehler.";
        }

        if (row is null && tb.Column.ChunkValueColumn == tb.Column.First && newChunkValue is null) {
            // Es soll eine neue Zeile erstellt werden, und die erste Spalte ist die Chunk-Spalte.
            // Wir wissen nicht, was das Ziel ist.
            return string.Empty;
        }

        if (oldChunk != newChunkValue) {
            if (tb.IsValueEditable(TableDataType.UTF8Value_withoutSizeData, oldChunk) is { Length: > 0 } aadc) { return aadc; }
        }

        return tb.IsValueEditable(TableDataType.UTF8Value_withoutSizeData, newChunkValue);
    }

    /// <summary>
    /// Erstellt den String-Key für Serialisierung und Undo.
    /// </summary>
    public static string KeyOfCell(string colname, string rowKey) => colname.ToUpperInvariant() + "|" + rowKey;

    /// <summary>
    /// Erstellt den String-Key für Serialisierung und Undo.
    /// Re-resolved Column und Row, um veraltete Referenzen zu eliminieren.
    /// </summary>
    public static string KeyOfCell(ColumnItem? column, RowItem? row) {
        column = column?.Table?.Column[column.KeyName];
        row = row?.Table?.Row.GetByKey(row.KeyName);

        if (column is not null && row is not null) { return KeyOfCell(column.KeyName, row.KeyName); }
        if (column is null && row is not null) { return KeyOfCell(string.Empty, row.KeyName); }
        if (column is not null) { return KeyOfCell(column.KeyName, string.Empty); }

        return string.Empty;
    }

    /// <summary>
    /// Prüft die Konfiguration des Verlinkungs-Filters einer Spalte, ohne eine FilterCollection zu erzeugen
    /// oder Zeilen der verlinkten Tabelle zu laden. Reine Strukturkontrolle: Format, Spaltenexistenz, AutoCorrect.
    /// </summary>
    public static OperationResult ValidateLinkedCellFilterConfig(Table? linkedTable, ColumnItem inputColumn) {
        if (linkedTable is not { IsDisposed: false }) { return OperationResult.Failed("Verlinkte Tabelle verworfen."); }
        if (inputColumn.IsDisposed) { return OperationResult.Failed("Spalte verworfen."); }

        var build = TryBuildLinkedCellFilterItems(linkedTable, inputColumn, null, null);
        if (build.IsFailed) { return build; }

        if (build.Value is not FilterCollection fc) { return OperationResult.FailedInternalError; }

        foreach (var filter in fc) {
            foreach (var sv in filter.SearchValue) {
                if (!sv.Contains('~')) { continue; }
                var match = false;
                foreach (var thisC in linkedTable.Column) {
                    if (sv == $"~{thisC.KeyName}~") {
                        match = true;
                        break;
                    }
                }
                if (!match) {
                    return OperationResult.Failed($"Suchwert '{sv}' ist ungültig. Erlaubt ist nur ~Spalte~ oder ein einfacher Wert.");
                }
            }
        }

        fc.Dispose();
        return OperationResult.Success;
    }

    public void DataOfCellKey(string cellKey, out ColumnItem? column, out RowItem? row) {
        if (string.IsNullOrEmpty(cellKey)) {
            column = null;
            row = null;
            return;
        }
        var cd = cellKey.SplitBy("|");
        if (cd.GetUpperBound(0) != 1) { Develop.DebugError("Falscher CellKey übergeben: " + cellKey); }
        column = Table?.Column[cd[0]];
        row = Table?.Row.GetByKey(cd[1]);
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    internal static string CompareKey(ColumnItem column, RowItem row) => row.CellGetStringCore(column).CompareKey(column.SortType);

    internal void Clear() => _internal.Clear();

    internal void InvalidateAllSizes() {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }
        foreach (var thisColumn in tb.Column) {
            thisColumn.Invalidate_ColumAndContent();
        }
    }

    internal void RemoveOrphans() {
        try {
            List<(ColumnItem column, RowItem row)> removeKeys = [];

            foreach (var pair in _internal) {
                if (!string.IsNullOrEmpty(pair.Value.Value)) {
                    if (pair.Key.column.IsDisposed || pair.Key.row.IsDisposed) {
                        removeKeys.Add(pair.Key);
                    }
                }
            }

            if (removeKeys.Count == 0) { return; }

            foreach (var thisKey in removeKeys) {
                if (!_internal.TryRemove(thisKey, out _)) {
                    Develop.AbortAppIfStackOverflow();
                    RemoveOrphans();
                    return;
                }
            }
        } catch {
            Develop.AbortAppIfStackOverflow(); // Um Rauszufinden, ob endlos-Schleifen öfters  vorkommen. Zuletzt 24.11.2020
        }
    }

    internal bool TryAddCell(ColumnItem column, RowItem row, CellItem cell) => _internal.TryAdd((column, row), cell);

    internal bool TryGetCell(ColumnItem column, RowItem row, out CellItem? cell) => _internal.TryGetValue((column, row), out cell);

    internal bool TryRemove(ColumnItem column, RowItem row) => _internal.TryRemove((column, row), out _);

    private static OperationResult TryBuildLinkedCellFilterItems(Table linkedTable, ColumnItem inputColumn, RowItem? inputRow, VariableCollection? varcol) {
        var fi = new FilterCollection(linkedTable, "linked cell filter");
        try {
            foreach (var thisFi in inputColumn.LinkedCellFilter) {
                if (!thisFi.Contains('|')) { return OperationResult.Failed("Veraltetes Filterformat"); }

                var x = thisFi.SplitBy("|");
                var c = linkedTable.Column[x[0]];
                if (c is null) { return OperationResult.Failed($"Die Spalte {x[0]}, nach der gefiltert werden soll, existiert nicht."); }

                if (x[1] != "=") { return OperationResult.Failed("Nur 'Gleich'-Filter wird unterstützt."); }

                var value = x[2].FromNonCritical();
                if (string.IsNullOrEmpty(value)) { return OperationResult.Failed("Leere Suchwerte werden nicht unterstützt."); }

                if (inputRow is not null) {
                    // DropdownValues hat nie eine Zeile!
                    value = inputRow.ReplaceVariables(value, true, varcol);
                    if (value.Contains('~')) { return OperationResult.Failed("Eine Variable konnte nicht aufgelöst werden."); }
                    if (value != c.AutoCorrect(value, true)) { return OperationResult.Failed("Wert kann nicht gesetzt werden."); }
                }

                if (value.Contains('\r')) { return OperationResult.Failed("Eine wurde mit einer Leerzeile aufgelöst."); }

                fi.Add(new FilterItem(c, FilterType.Istgleich, value));
            }

            if (fi.Count == 0 && inputColumn.RelationType != RelationType.DropDownValues) { return OperationResult.Failed("Keine gültigen Suchkriterien definiert."); }

            if (linkedTable.Column.ChunkValueColumn is { IsDisposed: false } cvc) {
                if (FilterCollection.InitValue(cvc, true, false, [.. fi]) is null) {
                    return OperationResult.Failed($"Im Verlinkungs-Filter der Spalte '{inputColumn.Caption}' fehlt die Filterung der Chunk-Spalte '{cvc.Caption}'.");
                }
            }

            return OperationResult.SuccessValue(fi);
        } catch (Exception ex) {
            fi.Dispose();
            return OperationResult.Failed(ex);
        }
    }

    private void _table_Disposing(object? sender, System.EventArgs e) => Dispose();

    private void Dispose(bool disposing) {
        if (Interlocked.CompareExchange(ref _isDisposedFlag, 1, 0) != 0) { return; }

        if (disposing) {
        }
        Table = null;
        _internal.Clear();
    }

    #endregion
}