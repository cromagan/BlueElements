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
using BlueScript.Variables;
using BlueTable.Enums;
using BlueTable.EventArgs;
using BlueTable.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static BlueBasics.Generic;

namespace BlueTable;

public sealed class CellCollection : ConcurrentDictionary<string, CellItem>, IDisposableExtended, IHasTable {

    #region Constructors

    public CellCollection(Table table) : base() => Table = table;

    #endregion Constructors

    #region Destructors

    // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    ~CellCollection() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(false);
    }

    #endregion Destructors

    #region Events

    public event EventHandler<CellEventArgs>? CellValueChanged;

    #endregion Events

    #region Properties

    public bool IsDisposed { get; private set; }

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

    #endregion Properties

    #region Indexers

    /// <summary>
    /// Gibt die Zelle zurück.
    /// Da leere Zellen nicht gespeichert werden, kann null zuück gebeben werden, obwohl die Zelle an sich gültig ist.
    /// </summary>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <returns></returns>
    public CellItem? this[ColumnItem? column, RowItem? row] {
        get {
            if (IsDisposed || Table is not { IsDisposed: false } db) { return null; }
            if (column is not { IsDisposed: false }) { return null; }
            if (row is not { IsDisposed: false }) { return null; }

            if (column.Table != row.Table || column.Table != db) { return null; }

            var cellKey = KeyOfCell(column, row);
            return ContainsKey(cellKey) ? this[cellKey] : null;
        }
    }

    #endregion Indexers

    #region Methods

    /// <summary>
    ///
    /// </summary>
    /// <param name="varcol">Wird eine Collection angegeben, werden zuerst diese Werte benutzt - falls vorhanden - anstelle des Wertes in der Zeile </param>
    /// <returns></returns>
    public static (FilterCollection? fc, string info) GetFilterFromLinkedCellData(Table? linkedTable, ColumnItem inputColumn, RowItem? inputRow, VariableCollection? varcol) {
        if (linkedTable is not { IsDisposed: false }) { return (null, "Verlinkte Tabelle verworfen."); }
        if (inputColumn.Table is not { IsDisposed: false } || inputColumn.IsDisposed) { return (null, "Tabelle verworfen."); }

        var fi = new List<FilterItem>();

        foreach (var thisFi in inputColumn.LinkedCellFilter) {
            if (!thisFi.Contains("|")) { return (null, "Veraltetes Filterformat"); }

            var x = thisFi.SplitBy("|");
            var c = linkedTable.Column[x[0]];
            if (c == null) { return (null, $"Die Spalte {x[0]}, nach der gefiltert werden soll, existiert nicht."); }

            if (x[1] != "=") { return (null, "Nur 'Gleich'-Filter wird unterstützt."); }

            var value = x[2].FromNonCritical();
            if (string.IsNullOrEmpty(value)) { return (null, "Leere Suchwerte werden nicht unterstützt."); }

            if (inputRow is { IsDisposed: false }) {
                // Es kann auch sein, dass nur mit Texten anstelle von Variablen gearbeitet wird,
                // und auch diese abgefragt werden
                value = inputRow.ReplaceVariables(value, true, varcol);
            }

            if (value != c.AutoCorrect(value, true)) { return (null, "Wert kann nicht gesetzt werden."); }

            fi.Add(new FilterItem(c, FilterType.Istgleich, value));
        }

        if (fi.Count == 0 && inputColumn.RelationType != RelationType.DropDownValues) { return (null, "Keine gültigen Suchkriterien definiert."); }

        if (linkedTable.Column.ChunkValueColumn is { IsDisposed: false } cvc) {
            if (string.IsNullOrEmpty(FilterCollection.InitValue(cvc, true, false, [.. fi])))
                return (null, "Filter des Chunk-Wertes fehlt.");
        }

        var fc = new FilterCollection(linkedTable, "cell get filter");
        fc.AddIfNotExists(fi);

        return (fc, string.Empty);
    }

    public static (FilterCollection? fc, string info) GetFilterReverse(ColumnItem mycolumn, ColumnItem linkedcolumn, RowItem linkedrow) {
        if (linkedcolumn.Table is not { IsDisposed: false } ldb || linkedcolumn.IsDisposed) { return (null, "Tabelle verworfen."); }

        if (mycolumn.RelationType != RelationType.CellValues) { return (null, "Falsches Format."); }

        if (mycolumn.Table is not { IsDisposed: false } db) { return (null, "Tabelle verworfen."); }

        var fc = new FilterCollection(db, "cell get reverse filter");

        foreach (var thisFi in mycolumn.LinkedCellFilter) {
            if (!thisFi.Contains("|")) { return (null, "Veraltetes Filterformat"); }

            var x = thisFi.SplitBy("|");
            var c = ldb.Column[x[0]];
            if (c == null) { return (null, "Eine Spalte, nach der gefiltert werden soll, existiert nicht."); }

            if (x[1] != "=") { return (null, "Nur 'Gleich'-Filter wird unterstützt."); }

            var value = x[2].FromNonCritical().ToUpperInvariant();
            if (string.IsNullOrEmpty(value)) { return (null, "Leere Suchwerte werden nicht unterstützt."); }

            foreach (var thisColumn in db.Column) {
                if (value.Contains("~" + thisColumn.KeyName.ToUpperInvariant() + "~")) {
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
            fc.Add(new FilterItem(db, "Reverse Filter"));
        }

        return (fc, string.Empty);

        //var columns = new List<ColumnItem>();

        //foreach (var thisFi in column.LinkedCellFilter) {
        //    if (!thisFi.Contains("|")) { return (null, "Veraltetes Filterformat"); }

        //    var x = thisFi.SplitBy("|");
        //    var value = x[2].FromNonCritical().ToUpperInvariant();

        //    foreach (var thisColumn in db.Column) {
        //        if (value.Contains("~" + thisColumn.KeyName.ToUpperInvariant() + "~")) {
        //            columns.AddIfNotExists(thisColumn);
        //        }
        //    }
        //}

        //var fc = new FilterCollection(db, "cell reverse get filter");

        //foreach (var thisColumn in columns) {
        //    var fi = new FilterItem(row, thisColumn);
        //    fc.AddIfNotExists(fi);
        //}

        //if (fc.Count == 0) {
        //    fc.Add(new FilterItem(db, "Reverse Filter"));
        //}

        //return (fc, string.Empty);
    }

    /// <summary>
    /// Gibt einen Fehlergrund zurück, ob die Zelle bearbeitet werden kann.
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <param name="mode"></param>
    /// <param name="checkUserRights">Ob vom Benutzer aktiv das Feld bearbeitet werden soll. false bei internen Prozessen angeben.</param>
    /// <param name="checkEditmode">Ob gewünscht wird, dass die intern programmierte Routine geprüft werden soll. Nur in Tabelleansicht empfohlen.</param>
    /// <param name="repairallowed"></param>
    /// <param name="ignoreLinked"></param>
    /// <returns></returns>
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

        if (row == null) {
            if (tb.Column.First is not { IsDisposed: false } firstcol || firstcol != column) {
                return "Neue Zeilen müssen mit der ersten Spalte beginnen.";
            }

            if (!tb.PermissionCheck(tb.PermissionGroupsNewRow, null)) {
                return "Sie haben nicht die nötigen Rechte, um neue Zeilen anzulegen.";
            }

            if (tb.Column.ChunkValueColumn is { } cvc && newChunkValue != null) {
                if (cvc != tb.Column.First && string.IsNullOrEmpty(newChunkValue)) { return "Chunk-Wert fehlt."; }
            }
        } else {
            if (!tb.PowerEdit && tb.Column.SysLocked != null) {
                if (column != tb.Column.SysLocked && row.CellGetBoolean(tb.Column.SysLocked) && !column.EditAllowedDespiteLock) {
                    return "Da die Zeile als abgeschlossen markiert ist, kann die Zelle nicht bearbeitet werden.";
                }
            }
            oldChunk = row.ChunkValue;
        }

        if (!tb.PermissionCheck(column.PermissionGroupsChangeCell, row)) {
            return "Sie haben nicht die nötigen Rechte, um diesen Wert zu ändern.";
        }

        if (!tb.IsEditable(false)) { return $"Tabellesperre: {tb.IsNotEditableReason(false)}"; }

        if (column.RelationType == RelationType.CellValues) {
            var (lcolumn, lrow, info, canrepair) = row.LinkedCellData(column, false, false);

            if (!string.IsNullOrEmpty(info) && !canrepair) { return info; }

            if (lcolumn?.Table is not { IsDisposed: false } db2) { return "Verknüpfte Tabelle verworfen."; }

            db2.PowerEdit = tb.PowerEdit;

            if (lrow != null) {
                var tmp = IsCellEditable(lcolumn, lrow, lrow.ChunkValue);
                return !string.IsNullOrEmpty(tmp) ? "Die verlinkte Zelle kann nicht bearbeitet werden: " + tmp : string.Empty;
            }

            if (canrepair) { return string.Empty; }

            return "Allgemeiner Fehler.";
        }

        if (row == null && tb.Column.ChunkValueColumn == tb.Column.First && newChunkValue == null) {
            // Es soll eine neue Zeile erstellt werden, und die erste Spalte ist die Chunk-Spalte.
            // Wir wissen nicht, was das Ziel ist.
            return string.Empty;
        }

        if (oldChunk != newChunkValue) {
            var aadc = tb.IsValueEditable(TableDataType.UTF8Value_withoutSizeData, oldChunk);
            if (!string.IsNullOrEmpty(aadc)) { return aadc; }
        }

        return tb.IsValueEditable(TableDataType.UTF8Value_withoutSizeData, newChunkValue);
    }

    /// <summary>
    /// Diese Routine erstellt den umgekehrten Linked-Cell-Filter:
    /// Es versucht rauszufinden, welche Zeile in der Tabelle von mycolumn von der Zeile linkedrow befüllt werden.
    /// </summary>
    /// <param name="mycolumn"></param>
    /// <param name="linkedcolumn"></param>
    /// <param name="linkedrow"></param>
    /// <returns></returns>
    public static string KeyOfCell(string colname, string rowKey) => colname.ToUpperInvariant() + "|" + rowKey;

    public static string KeyOfCell(ColumnItem? column, RowItem? row) {
        // Alte verweise eleminieren.
        column = column?.Table?.Column[column.KeyName];
        row = row?.Table?.Row.GetByKey(row.KeyName);

        if (column != null && row != null) { return KeyOfCell(column.KeyName, row.KeyName); }
        if (column == null && row != null) { return KeyOfCell(string.Empty, row.KeyName); }
        if (column != null && row == null) { return KeyOfCell(column.KeyName, string.Empty); }

        return string.Empty;
    }

    public void DataOfCellKey(string cellKey, out ColumnItem? column, out RowItem? row) {
        if (string.IsNullOrEmpty(cellKey)) {
            column = null;
            row = null;
            return;
        }
        var cd = cellKey.SplitBy("|");
        if (cd.GetUpperBound(0) != 1) { Develop.DebugPrint(ErrorType.Error, "Falscher CellKey übergeben: " + cellKey); }
        column = Table?.Column[cd[0]];
        row = Table?.Row.GetByKey(cd[1]);
    }

    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Table = null;
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    internal bool ChangeColumnName(string oldName, string newName) {
        oldName = oldName.ToUpperInvariant() + "|";
        newName = newName.ToUpperInvariant() + "|";

        if (oldName == newName) { return true; }

        var keys = new List<string>();
        foreach (var thispair in this) {
            if (thispair.Key.StartsWith(oldName)) {
                keys.Add(thispair.Key);
            }
        }

        foreach (var thisk in keys) {
            if (!TryRemove(thisk, out var ci)) { return false; }

            var newk = newName + thisk.TrimStart(oldName);
            if (!TryAdd(newk, ci)) { return false; }
        }

        return true;
    }

    internal string CompareKey(ColumnItem column, RowItem row) => row.CellGetStringCore(column).CompareKey(column.SortType);

    internal void InvalidateAllSizes() {
        if (IsDisposed || Table is not { IsDisposed: false } db) { return; }
        foreach (var thisColumn in db.Column) {
            thisColumn.Invalidate_ColumAndContent();
        }
    }

    internal void OnCellValueChanged(CellEventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false }) { return; }
        e.Column.UcaseNamesSortedByLength = null;
        CellValueChanged?.Invoke(this, e);
    }

    internal void RemoveOrphans() {
        try {
            List<string> removeKeys = [];

            foreach (var pair in this) {
                if (!string.IsNullOrEmpty(pair.Value.Value)) {
                    DataOfCellKey(pair.Key, out var column, out var row);
                    if (column == null || row == null) {
                        removeKeys.Add(pair.Key);
                    }
                }
            }

            if (removeKeys.Count == 0) { return; }

            foreach (var thisKey in removeKeys) {
                if (!TryRemove(thisKey, out _)) {
                    Develop.AbortAppIfStackOverflow();
                    RemoveOrphans();
                    return;
                }
            }
        } catch {
            Develop.AbortAppIfStackOverflow(); // Um Rauszufinden, ob endlos-Schleifen öfters  vorkommen. Zuletzt 24.11.2020
        }
    }

    private void _table_Disposing(object sender, System.EventArgs e) => Dispose();

    private void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
            }
            // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
            // TODO: Große Felder auf NULL setzen
            Table = null;
            Clear();
            IsDisposed = true;
        }
    }

    #endregion Methods
}