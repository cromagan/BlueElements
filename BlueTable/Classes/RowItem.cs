// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueBasics;
using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueScript.Classes;
using BlueScript.Variables;
using BlueTable.Enums;
using BlueTable.EventArgs;
using BlueTable.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static BlueBasics.ClassesStatic.Converter;
using static BlueTable.Classes.Table;

namespace BlueTable.Classes;

public sealed class RowItem : ICanBeEmpty, IDisposableExtendedWithEvent, IHasKeyName, IHasTable, IEditable {

    #region Fields

    private RowPrepareFormulaEventArgs? _lastCheckedEventArgs;

    #endregion

    #region Constructors

    public RowItem(Table table, string key) {
        Table = table;
        _keyName = key;
    }

    #endregion

    #region Destructors

    ~RowItem() => Dispose(false);

    #endregion

    #region Events

    public event EventHandler? DisposingEvent;

    public event EventHandler<RowPrepareFormulaEventArgs>? RowChecked;

    #endregion

    #region Properties

    public string CaptionForEditor => "Zeile";

    public string ChunkValue {
        get {
            if (Table is not TableChunk) { return string.Empty; }

            if (Table?.Column.ChunkValueColumn is { IsDisposed: false } spc) { return CellGetStringCore(spc); }
            return string.Empty;
        }
    }

    public bool IsDisposed { get; private set; }

    public bool KeyIsCaseSensitive => true;

    private string _keyName;

    public string KeyName {
        get => _keyName;
        set {
            if (IsDisposed) { return; }
            value = value.ToUpperInvariant();
            if (value.Equals(_keyName, StringComparison.OrdinalIgnoreCase)) { return; }

            //if (!ColumNameAllowed(value)) {
            //    Develop.DebugPrint("Spaltenname nicht erlaubt: " + _keyName);
            //    return;
            //}

            if (Table?.Row[value] != null) {
                Develop.DebugPrint("Name existiert bereits!");
                return;
            }

            //if (!IsValidColumnKey(value)) {
            //    Develop.DebugPrint("Spaltenname nicht erlaubt!");
            //    return;
            //}

            var c = ChunkValue;

            Table?.ChangeData(TableDataType.RowKey, null, this, _keyName, value, Generic.UserName, DateTime.UtcNow, string.Empty, c, c);

            //OnPropertyChanged();
            //CheckIfIAmAKeyColumn();
        }
    }

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

    /// <summary>
    /// Wie wichtig ein Update ist. Kleine Zahlen sind wichtiger.
    /// </summary>
    public long UrgencyUpdate {
        get {
            if (NeedsRowInitialization()) { return 0; }
            if (NeedsRowUpdate()) { return 1; }
            if (Table?.Column.SysRowState is { IsDisposed: false } srs) { return CellGetDateTime(srs).Ticks; }
            return long.MaxValue;
        }
    }

    #endregion

    #region Methods

    public static Variable? CellToVariable(ColumnItem? column, RowItem? row, bool readOnly, bool virtualcolumns) {
        if (column is not { ScriptType: not (ScriptType.Nicht_vorhanden or ScriptType.undefiniert) }) { return null; }

        if (!column.SaveContent) {
            if (!virtualcolumns) { return null; }
            readOnly = false;
        }

        if (!column.CanBeCheckedByRules()) { return null; }
        if (!column.CanBeChangedByRules()) { readOnly = true; }

        var value = row?.CellGetString(column) ?? string.Empty;

        return CellToVariable(column.KeyName, column.ScriptType, value, readOnly, "Spalte: " + column.ReadableText());
    }

    public static Variable? CellToVariable(string varname, ScriptType scriptType, string value, bool readOnly, string coment) {
        switch (scriptType) {
            case ScriptType.Bool:
                return new VariableBool(varname, value.FromPlusMinus(), readOnly, coment);

            case ScriptType.List:
                return new VariableListString(varname, [.. value.SplitAndCutByCr()], readOnly, coment);

            case ScriptType.Numeral:
                return new VariableDouble(varname, DoubleParse(value), readOnly, coment);

            case ScriptType.Numeral_Readonly:
                return new VariableDouble(varname, DoubleParse(value), true, coment);

            case ScriptType.String:
                return new VariableString(varname, value, readOnly, coment);

            case ScriptType.String_Readonly:
                return new VariableString(varname, value, true, coment);

            case ScriptType.Bool_Readonly:
                return new VariableBool(varname, value.FromPlusMinus(), true, coment);

            case ScriptType.List_Readonly:
                return new VariableListString(varname, [.. value.SplitAndCutByCr()], true, coment);

            case ScriptType.Nicht_vorhanden:
                return null;

            default:
                Develop.DebugPrint(scriptType);
                return null;
        }
    }

    public string CellFirstString() => Table?.Column.First is not { IsDisposed: false } fc ? string.Empty : CellGetString(fc);

    public bool CellGetBoolean(string columnKey) => CellGetBoolean(Table?.Column[columnKey]);

    public bool CellGetBoolean(ColumnItem? column) => CellGetString(column).FromPlusMinus();

    public Color CellGetColor(string columnKey) => CellGetColor(Table?.Column[columnKey]);

    public Color CellGetColor(ColumnItem? column) => ColorParse(CellGetString(column) ?? string.Empty);

    public int CellGetColorBgr(ColumnItem? column) {
        var c = CellGetColor(column);
        int colorBlue = c.B;
        int colorGreen = c.G;
        int colorRed = c.R;
        return colorBlue << 16 | colorGreen << 8 | colorRed;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="columnKey"></param>
    /// <returns>DateTime.MinValue bei Fehlern</returns>
    public DateTime CellGetDateTime(string columnKey) => CellGetDateTime(Table?.Column[columnKey]);

    /// <summary>
    ///
    /// </summary>
    /// <returns>DateTime.MinValue bei Fehlern</returns>
    public DateTime CellGetDateTime(ColumnItem? column) {
        var value = CellGetString(column) ?? string.Empty;
        return DateTimeTryParse(value, out var d) ? d : DateTime.MinValue;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="columnKey"></param>
    /// <returns>0 bei Fehlern</returns>
    public double CellGetDouble(string columnKey) => CellGetDouble(Table?.Column[columnKey]);

    /// <summary>
    ///
    /// </summary>
    /// <param name="column"></param>
    /// <returns>0 bei Fehlern</returns>
    public double CellGetDouble(ColumnItem? column) => DoubleParse(CellGetString(column));

    /// <summary>
    ///
    /// </summary>
    /// <param name="columnKey"></param>
    /// <returns>0 bei Fehlern</returns>
    public int CellGetInteger(string columnKey) => CellGetInteger(Table?.Column[columnKey]);

    /// <summary>
    ///
    /// </summary>
    /// <param name="column"></param>
    /// <returns>0 bei Fehlern</returns>
    public int CellGetInteger(ColumnItem? column) => IntParse(CellGetString(column));

    public List<string> CellGetList(ColumnItem? column) {
        if (column?.Table is not { IsDisposed: false }) { return []; }

        if (column.TextFormatingAllowed) {
            return [.. CellGetString(column).SplitAndCutBy("<br>")];
        }

        return [.. CellGetString(column).SplitAndCutByCr()];
    }

    public List<string> CellGetList(string columnKey) => CellGetList(Table?.Column[columnKey]);

    /// <summary>
    ///
    /// </summary>
    /// <param name="column"></param>
    /// <returns>0 bei Fehlern</returns>
    public long CellGetLong(ColumnItem? column) => LongParse(CellGetString(column));

    public Point CellGetPoint(ColumnItem? column) // Main Method
    {
        //TODO: Unused
        var value = CellGetString(column);
        return string.IsNullOrEmpty(value) ? Point.Empty : value.PointParse();
    }

    public string CellGetString(string columnKey) => CellGetString(Table?.Column[columnKey]) ?? string.Empty;

    public string CellGetString(ColumnItem? column) // Main Method
                                                                                                {
        try {
            if (IsDisposed) {
                //Develop.DebugError("Zeile ungültig!<br>" + Table.KeyName);
                return string.Empty;
            }

            if (IsDisposed || Table is not { IsDisposed: false }) {
                Table?.DevelopWarnung("Tabelle ungültig!");
                Develop.DebugError("Tabelle ungültig!");
                return string.Empty;
            }

            if (column is not { IsDisposed: false }) {
                Table?.DevelopWarnung("Spalte ungültig!");
                Develop.DebugError("Spalte ungültig!<br>" + Table?.KeyName);
                return string.Empty;
            }

            //if (column.RelationType == RelationType.CellValues) {
            //    var (lcolumn, lrow, _, _) = LinkedCellData(column, false, false);
            //    if (lcolumn != null && lrow != null) { return lrow.CellGetString(lcolumn); } // Chunks werden NICHT nachgeladen!
            //}

            return CellGetStringCore(column);
        } catch {
            // Manchmal verscwhindwet der vorhandene KeyName?!?
            Develop.AbortAppIfStackOverflow();
            return CellGetString(column);
        }
    }

    public string CellGetStringCore(ColumnItem? column) => Table?.Cell[column, this]?.Value ?? string.Empty;

    public string CellSet(string columnKey, bool value, string comment) => CellSet(Table?.Column[columnKey], value.ToPlusMinus(), comment);

    public string CellSet(ColumnItem column, bool value, string comment) => CellSet(column, value.ToPlusMinus(), comment);

    public string CellSet(string columnKey, string value, string comment) => CellSet(Table?.Column[columnKey], value, comment);

    public string CellSet(string columnKey, double value, string comment) => CellSet(Table?.Column[columnKey], value.ToString1_5(), comment);

    public string CellSet(ColumnItem column, double value, string comment) => CellSet(column, value.ToString1_5(), comment);

    public string CellSet(string columnKey, int value, string comment) => CellSet(Table?.Column[columnKey], value.ToString1(), comment);

    public string CellSet(ColumnItem column, int value, string comment) {
        //TODO: Unused
        return CellSet(column, value.ToString1(), comment);
    }

    public string CellSet(string columnKey, IEnumerable<string>? value, string comment) => CellSet(Table?.Column[columnKey], value != null ? string.Join('\r', value) : null, comment);

    public string CellSet(ColumnItem column, IEnumerable<string>? value, string comment) => CellSet(column, value != null ? string.Join('\r', value) : null, comment);

    public string CellSet(string columnKey, DateTime value, string comment) => CellSet(Table?.Column[columnKey], value.ToString5(), comment);

    public string CellSet(ColumnItem column, DateTime value, string comment) {
        return CellSet(column, value.ToString5(), comment);
    }

    /// <summary>
    /// Lenkt den Wert evtl. auf die verlinkte Zelle um
    /// </summary>
    /// <param name="column"></param>
    /// <param name="value"></param>
    /// <param name="comment"></param>
    /// <returns></returns>
    public string CellSet(ColumnItem? column, string value, string comment) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return "Tabelle ungültig!"; }

        if (tb.IsFreezed) { return "Tabelle eingefroren!"; }

        if (column is not { IsDisposed: false }) { return "Spalte ungültig!"; }

        if (tb != Table || tb != column.Table) { return "Tabelle ungültig!"; }

        if (column.RelationType == RelationType.CellValues) {
            var (lcolumn, lrow, _, _) = LinkedCellData(column, true, !string.IsNullOrEmpty(value));

            //return db.ChangeData(TableDataType.Value_withoutSizeData, lcolumn, lrow, string.Empty, value, UserName, DateTime.UtcNow, string.Empty);
            lrow?.CellSet(lcolumn, value, "Verlinkung der Tabelle " + tb.Caption + " (" + comment + ")");
            return string.Empty;
        }

        value = column.AutoCorrect(value, true);
        var oldValue = CellGetStringCore(column);
        if (value == oldValue) { return string.Empty; }

        var uniqueError = CheckUniqueValueConstraint(column, value);
        if (!string.IsNullOrEmpty(uniqueError)) { return uniqueError; }

        column.UcaseNamesSortedByLength = null;

        if (!column.SaveContent) {
            return CellSetInternal(column, value, Reason.NoUndo_NoInvalidate);
        }

        var newChunkValue = ChunkValue;
        var oldChunkValue = newChunkValue;

        if (column == tb.Column.ChunkValueColumn) {
            newChunkValue = value;
        }

        if (tb.ChangeData(TableDataType.UTF8Value_withoutSizeData, column, this, oldValue, value, Generic.UserName, DateTime.UtcNow, comment, oldChunkValue, newChunkValue) is { Length: > 0 } message) { return message; }

        if (value != CellGetStringCore(column)) { return "Nachprüfung fehlgeschlagen"; }

        DoSpecialFormats(column, oldValue, value);

        return string.Empty;
    }

    public RowPrepareFormulaEventArgs CheckRow() {
        if (_lastCheckedEventArgs != null) {
            if (_lastCheckedEventArgs.PrepareFormulaFeedback.NeedsScriptFix || !_lastCheckedEventArgs.PrepareFormulaFeedback.Failed) {
                return _lastCheckedEventArgs;
            }
        }

        var sef = Table?.ExecuteScript(ScriptEventTypes.prepare_formula, string.Empty, true, this, null, true, false, 0) ?? new ScriptEndedFeedback("Tabelle verworfen", false, false, "Allgemein");

        Brush? b = null;
        if (sef.Variables.GetByKey("RowColor") is VariableString vs) {
            if (!string.IsNullOrEmpty(vs.ValueString)) {
                if (ColorTryParse(vs.ValueString, out var c)) {
                    b = new SolidBrush(c);
                }
            }
        }

        if (sef.Failed) {
            _lastCheckedEventArgs = new RowPrepareFormulaEventArgs(this, null, sef, $"Das Skript konnte die Zeile nicht durchrechnen: {sef.FailedReason}", b);
            return _lastCheckedEventArgs;
        }

        if (RowCollection.FailedRows.TryGetValue(this, out var reason)) {
            _lastCheckedEventArgs = new RowPrepareFormulaEventArgs(this, null, sef, reason, b);
            return _lastCheckedEventArgs;
        }

        List<string> cols = [];

        var sb = new StringBuilder();

        var tmp = sef.Variables?.GetList("ErrorColumns");
        if (tmp is { Count: > 0 }) {
            foreach (var thiss in tmp) {
                cols.AddIfNotExists(thiss);
                var t = thiss.SplitBy("|");

                if (Table?.Column[t[0]] is { IsDisposed: false } thisc) {
                    sb.Append("<b>");
                    sb.Append(thisc.ReadableText());
                    sb.Append(":</b> ");
                    sb.Append(t[1]);
                    sb.Append("<br><hr><br>");
                }
            }
        } else {
            sb.Append("Diese Zeile ist fehlerfrei.");
        }

        var m = sb.ToString();

        if (Table?.Column.SysCorrect is { IsDisposed: false } sc) {
            if (cols.Count == 0 != CellGetBoolean(sc)) {
                if (string.IsNullOrEmpty(Table.IsValueEditable(TableDataType.UTF8Value_withoutSizeData, ChunkValue))) {
                    CellSet(sc, cols.Count == 0, "Fehlerprüfung");
                }
            }
        }

        _lastCheckedEventArgs = new RowPrepareFormulaEventArgs(this, cols, sef, m, b);

        OnRowChecked(_lastCheckedEventArgs);

        return _lastCheckedEventArgs;
    }

    public string CompareKey(ICollection<ColumnItem> columns) {
        var key = new StringBuilder();

        foreach (var thisColumn in columns) {
            key.Append(CellGetCompareKey(thisColumn) + Constants.FirstSortChar);
        }

        key.Append(Constants.SecondSortChar + KeyName);
        return key.ToString();
    }

    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    //    Develop.DebugError("Falscher Objecttyp!");
    //    return 0;
    //}
    public void DropMessage(ErrorType type, string message) {
        if (IsDisposed) { return; }
        if (Table is not { IsDisposed: false } tb) { return; }
        if (!tb.DropMessages) { return; }

        Develop.Message(type, this, tb.Caption, ImageCode.Zeile, message, 0);
    }

    //public int CompareTo(object obj) {
    //    if (obj is RowItem tobj) {
    //        return string.Compare(CompareKey(), tobj.CompareKey(), StringComparison.OrdinalIgnoreCase);
    //    }
    public string GetQuickInfo() {
        if (Table is not { IsDisposed: false }) { return string.Empty; }
        return ReplaceVariables(Table.RowQuickInfo, false, null);
    }

    public void InvalidateCheckData() {
        RowCollection.FailedRows.TryRemove(this, out _);
        _lastCheckedEventArgs = null;
    }

    public void InvalidateRowState(string comment) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        if (tb.Column.SysRowState is not { IsDisposed: false } srs) { return; }
        //if (db.Column.SysRowChanger is not { IsDisposed: false } src) { return; }
        if (tb.Column.SysRowChangeDate is not { IsDisposed: false } scd) { return; }

        InvalidateCheckData();
        RowCollection.InvalidatedRowsManager.AddInvalidatedRow(this);

        if (string.IsNullOrEmpty(CellGetStringCore(srs)) && IsMyRow(RowCollection.NewRowTolerance, false)) {
            DropMessage(ErrorType.Info, $"Zeile {CellFirstString()} ist bereits invalidiert");
            return;
        }

        CellSet(srs, string.Empty, comment);
        CellSet(scd, DateTime.UtcNow, comment);

        DropMessage(ErrorType.Info, $"Zeile {CellFirstString()} invalidiert");
    }

    string IEditable.IsNowEditable() {
        if (IsDisposed) { return "Zeile verworfen"; }
        if (Table is not { IsDisposed: false } tb) { return "Tabelle verworfen"; }
        return tb.GrantWriteAccess(TableDataType.UTF8Value_withoutSizeData, ChunkValue);
    }

    public bool IsNullOrEmpty() => IsDisposed || Table is not { IsDisposed: false } tb ||
                                   tb.Column.All(thisColumnItem => thisColumnItem != null && string.IsNullOrEmpty(CellGetStringCore(thisColumnItem)));

    public (ColumnItem? column, RowItem? row, string info, bool canrepair) LinkedCellData(ColumnItem? inputColumn, bool repairallowed, bool addRowIfNotExists) {
        if (inputColumn?.Table is not { IsDisposed: false } tb) { return (null, null, "Eigene Tabelle verworfen.", false); }
        if (inputColumn.RelationType != RelationType.CellValues) { return (null, null, "Spalte ist nicht verlinkt.", false); }
        if (inputColumn.Value_for_Chunk != ChunkType.None) { return (null, null, "Verlinkte Spalte darf keine ChunkValue-Spalte sein.", false); }
        if (inputColumn.LinkedTable is not { IsDisposed: false } linkedTable) { return (null, null, "Verknüpfte Tabelle verworfen.", false); }
        if (IsDisposed) { return (null, null, "Keine Zeile zum Finden des Zeilenschlüssels angegeben.", false); }

        if (linkedTable.Column[inputColumn.ColumnKeyOfLinkedTable] is not { IsDisposed: false } targetColumn) { return (null, null, "Die Spalte ist in der Zieltabelle nicht vorhanden.", false); }
        if (targetColumn.Value_for_Chunk != ChunkType.None) { return (null, null, "Verlinkungen auf Chunk-Spalten nicht möglich.", false); }

        var result = CellCollection.GetFilterFromLinkedCellData(linkedTable, inputColumn, this, null);
        if (result.IsFailed || result.Value is not FilterCollection { } fc) { return (targetColumn, null, result.FailedReason, false); }
        if (fc is not { Count: not 0 }) { return (targetColumn, null, "Filter konnten nicht generiert werden", false); }

        if (linkedTable is TableChunk tbc) {
            if (!repairallowed && !tbc.ChunkIsLoaded(fc.ChunkVal)) { return (targetColumn, null, "Chunk nicht geladen", true); }
        }

        RowItem? targetRow = null;
        var rows = fc.Rows;
        switch (rows.Count) {
            case > 1:
                return (targetColumn, null, "Suchergebnis liefert mehrere Ergebnisse.", false);

            case 1:
                targetRow = rows[0];
                break;

            default: {
                    if (addRowIfNotExists) {
                        var r = linkedTable.Row.GenerateAndAdd([.. fc], "LinkedCell aus " + tb.KeyName);
                        if (r.Value is not RowItem newrow) { return (targetColumn, null, r.FailedReason, r.IsRetryable); }
                        targetRow = newrow;
                    }
                    break;
                }
        }

        if (targetRow != null) {
            if (repairallowed && inputColumn.RelationType == RelationType.CellValues) {
                var oldvalue = CellGetStringCore(inputColumn);
                var newvalue = targetRow.CellGetString(targetColumn);

                if (oldvalue != newvalue) {
                    var chunkValue = ChunkValue;
                    var f = GrantWriteAccess(inputColumn, this, chunkValue, 2, true);
                    if (!string.IsNullOrEmpty(f)) { return (targetColumn, targetRow, f, false); }

                    //Nicht CellSet! Damit wird der Wert der Ziel-Tabelle verändert
                    //row.CellSet(column, targetRow.KeyName);
                    //  db.Cell.SetValue(column, row, targetRow.KeyName, UserName, DateTime.UtcNow, false);

                    var fehler = tb.ChangeData(TableDataType.UTF8Value_withoutSizeData, inputColumn, this, oldvalue, newvalue, Generic.UserName, DateTime.UtcNow, "Automatische Reparatur", string.Empty, chunkValue);
                    if (!string.IsNullOrEmpty(fehler)) { return (targetColumn, targetRow, fehler, false); }
                }
            }
            targetColumn.AddSystemInfo("Links to me", tb, inputColumn.KeyName);
        }

        return (targetColumn, targetRow, string.Empty, true);
    }

    public bool MatchesTo(FilterItem fi) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return false; }

        if (fi.Table != tb) { return false; }

        if (fi.FilterType == FilterType.AlwaysFalse) { return false; }

        if (!fi.IsOk()) { return false; }

        if (fi.FilterType == FilterType.RowKey) {
            if (fi.SearchValue.Count != 1) { return false; }
            return KeyName == fi.SearchValue[0];
        }

        if (fi.Column == null) { return fi.SearchValue.All(RowFilterMatch); }

        return MatchesTo(fi.Column, fi.FilterType, fi.SearchValue);
    }

    public bool MatchesTo(params FilterItem[]? filter) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return false; }

        if (filter is not { Length: not 0 }) { return Table.Column.ChunkValueColumn == null || tb.PowerEdit; }

        if (Table.Column.ChunkValueColumn is { IsDisposed: false } cvc && !Table.PowerEdit) {
            var found = false;
            foreach (var thisF in filter) {
                if (thisF.Column == cvc) { found = true; break; }
            }
            if (!found) { return false; }
        }

        if (filter.Length == 1) { return MatchesTo(filter[0]); }

        var ok = true;

        Parallel.ForEach(filter, (thisFilter, state) => {
            if (!MatchesTo(thisFilter)) {
                ok = false;
                state.Break();
            }
        });

        return ok;
    }

    /// <summary>
    /// Gibt true zurück, wenn eine Zeile initialisiert werden muss.
    /// Achtung: KURZE ValueChanged Skripte werden wie ein Update behandelt
    /// </summary>
    /// <returns></returns>
    public bool NeedsRowInitialization() {
        if (Table is not { IsDisposed: false } tb) { return false; }

        if (!tb.ChangedScriptMayAffectUser) { return NeedsRowUpdate(); }

        if (!tb.HasValueChangedScript) { return false; }

        if (tb.Column.SysRowState is not { IsDisposed: false } srs) { return false; }

        var dts = CellGetString(srs);

        return string.IsNullOrEmpty(dts) || dts == "0";
    }

    /// <summary>
    /// Gibt true zurück, wenn eine Zeile aktualisiert oder initialisiert werden muss.
    /// </summary>
    /// <returns></returns>
    public bool NeedsRowUpdate() {
        if (Table is not { IsDisposed: false } tb) { return false; }
        if (!tb.HasValueChangedScript) { return false; }

        if (tb.Column.SysRowState is not { IsDisposed: false } srs) { return false; }

        var dts = CellGetString(srs);
        if (string.IsNullOrEmpty(dts)) { return true; }

        if (!DateTimeTryParse(dts, out var dt)) { return true; }

        return dt < Table.EventScriptVersion;
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>Empty, wenn alles in Ordung ist. Ansonten ein Grund.</returns>
    public string RepairAllLinks() {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return "Tabelle verworfen"; }

        foreach (var thisColumn in tb.Column) {
            if (thisColumn.RelationType == RelationType.CellValues) {
                LinkedCellData(thisColumn, true, false);

                //if (!string.IsNullOrEmpty(info) && !canrepair) { return false; }
            }
        }
        return string.Empty;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="txt"></param>
    /// <param name="removeLineBreaks"></param>
    /// <param name="varcol">Wir eine Collection angegeben, werden zuerst diese Werte benutzt - falls vorhanden - anstelle des Wertes in der Zeile </param>
    /// <returns></returns>
    public string ReplaceVariables(string txt, bool removeLineBreaks, VariableCollection? varcol) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return txt; }

        var erg = txt;

        if (varcol != null) {
            foreach (var vari in varcol) {
                if (!erg.Contains('~')) { return erg; }

                if (vari != null) {
                    if (erg.Contains($"~{vari.KeyName}", StringComparison.OrdinalIgnoreCase)) {
                        var replacewith = vari.SearchValue;

                        //if (vari is VariableString vs) { replacewith =  vs.v}

                        if (removeLineBreaks) {
                            replacewith = replacewith.Replace("\r\n", " ");
                            replacewith = replacewith.Replace('\r', ' ');
                        }

                        erg = erg.Replace($"~{vari.KeyName}~", replacewith, RegexOptions.IgnoreCase);
                    }
                }
            }
        }

        // Variablen ersetzen
        foreach (var column in tb.Column) {
            if (!erg.Contains('~')) { return erg; }

            if (column is { } && column.RelationType != RelationType.CellValues) {
                if (erg.Contains($"~{column.KeyName}", StringComparison.OrdinalIgnoreCase)) {
                    var replacewith = CellGetString(column);
                    //if (readableValue) { replacewith = CellItem.ValueReadable(replacewith, ShortenStyle.Replaced, BildTextVerhalten.Nur_Text, removeLineBreaks, column.Prefix, column.Suffix, column.DoOpticalTranslation, column.OpticalReplace); }

                    //if (varcol != null) {
                    //    if (varcol.Get(column.KeyName) is Variable v) { replacewith = v.SearchValue; }
                    //}

                    if (removeLineBreaks) {
                        replacewith = replacewith.Replace("\r\n", " ");
                        replacewith = replacewith.Replace('\r', ' ');
                    }

                    erg = erg.Replace("~" + column.KeyName + "~", replacewith, RegexOptions.IgnoreCase);
                }
            }
        }
        return erg;
    }

    /// <summary>
    /// Erstellt einen eindeutigen String-Stempel für die aktuelle Zeile durch Verkettung aller relevanten Spaltenwerte.
    /// Ignoriert dabei System-Spalten und Spalten mit ungültigen ScriptTypes.
    /// </summary>
    /// <returns>
    /// Ein String mit allen Spaltenwerten getrennt durch "|", oder ein leerer String wenn die Zeile oder Tabelle disposed ist.
    /// </returns>
    public string RowStamp() {
        // Frühe Rückgabe wenn Objekt disposed oder Tabelle ungültig ist
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return string.Empty; }

        // Null-Prüfung für Column-Collection
        if (tb.Column == null) { return string.Empty; }

        var resultBuilder = new StringBuilder();

        // LINQ-basierte Filterung und Verarbeitung der Spalten
        var validColumns = tb.Column
            .Where(column => column is { IsDisposed: false })
            .Where(column => column.ScriptType is not (ScriptType.Nicht_vorhanden or ScriptType.undefiniert))
            .Where(column => column != tb.Column.SysRowState);

        // Spaltenwerte sammeln und verketten
        foreach (var column in validColumns) {
            resultBuilder.Append(CellGetString(column)).Append('|');
        }

        return resultBuilder.ToString();
    }

    public ScriptEndedFeedback UpdateRow(bool extendedAllowed, string reason) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return new ScriptEndedFeedback("Tabelle verworfen", false, false, "Allgemein"); }

        if (!tb.CanDoValueChangedScript(true)) { return new ScriptEndedFeedback("Skripte fehlerhaft!", false, true, "Allgemein"); }

        WaitScriptsDone();

        if (tb.Column.SysRowState is not { IsDisposed: false } srs) {
            return new ScriptEndedFeedback([], RepairAllLinks());
        }

        var mustBeExtended = NeedsRowInitialization();

        if (!extendedAllowed && mustBeExtended) { return new ScriptEndedFeedback("Interner Fehler", false, false, "Allgemein"); }

        try {
            DropMessage(ErrorType.Info, $"Aktualisiere Zeile: {CellFirstString()} der Tabelle {tb.Caption} ({reason})");

            if (extendedAllowed) {
                RowCollection.InvalidatedRowsManager.MarkAsProcessed(this);
            }

            var ok = tb.ExecuteScript(ScriptEventTypes.value_changed, string.Empty, true, this, null, true, mustBeExtended, 2);

            if (ok.Failed) { return ok; }

            var reas = RepairAllLinks();

            if (!string.IsNullOrEmpty(reas)) {
                return new ScriptEndedFeedback([], reas);
            }

            CellSet(srs, DateTime.UtcNow, "Erfolgreiche Datenüberprüfung"); // Nicht System set, diese Änderung muss geloggt werden

            InvalidateCheckData();
            RowCollection.AddBackgroundWorker(this);
            tb.OnInvalidateView();
            return ok;
        } catch {
            return new ScriptEndedFeedback("Interner Fehler", false, true, "Allgemein");
        }
    }

    public void VariableToCell(ColumnItem? column, VariableCollection vars, string scriptname) {
        if (Table is not { IsDisposed: false } tb || column == null) { return; }

        if (!string.IsNullOrEmpty(tb.GrantWriteAccess(TableDataType.UTF8Value_withoutSizeData, ChunkValue))) { return; }

        var columnVar = vars.GetByKey(column.KeyName);
        if (columnVar is not { ReadOnly: false }) { return; }
        if (!column.CanBeChangedByRules()) { return; }

        CellSet(column, columnVar.ValueForCell, $"Skript '{scriptname}'");
    }

    internal static bool CompareValues(string istValue, string filterValue, FilterType typ) {
        var comparisonType = typ.HasFlag(FilterType.GroßKleinEgal) ? StringComparison.OrdinalIgnoreCase
                                                                   : StringComparison.Ordinal;

        // Entfernen des GroßKleinEgal-Flags, da es nicht mehr benötigt wird
        typ &= ~FilterType.GroßKleinEgal;

        switch (typ) {
            case FilterType.Istgleich:
                return string.Equals(istValue, filterValue, comparisonType);

            case (FilterType)2: // Ungleich
                return !string.Equals(istValue, filterValue, comparisonType);

            case FilterType.Instr:
                return istValue.Contains(filterValue, comparisonType);

            case FilterType.Between:
                var rangeParts = filterValue.Split(['|'], StringSplitOptions.RemoveEmptyEntries);
                if (rangeParts.Length != 2) {
                    return false;
                }

                // Wenn kein Datum, dann als numerischen Wert behandeln
                if (DoubleTryParse(istValue, out var numericValue)) {
                    if (!DoubleTryParse(rangeParts[0], out var minNumeric) || !DoubleTryParse(rangeParts[1], out var maxNumeric)) {
                        return false; // Mindestens einer der Werte ist keine gültige Zahl
                    }
                    return numericValue >= minNumeric && numericValue <= maxNumeric;
                }

                if (DateTimeTryParse(istValue, out var dateValue)) {
                    if (!DateTimeTryParse(rangeParts[0], out var minDate) || !DateTimeTryParse(rangeParts[1], out var maxDate)) {
                        return false; // Mindestens einer der Bereichswerte ist kein gültiges Datum
                    }
                    return dateValue >= minDate && dateValue <= maxDate;
                }

                return false; // Weder Datum noch numerischer Wert

            case FilterType.BeginntMit:
                return istValue.StartsWith(filterValue, comparisonType);

            case FilterType.AlwaysFalse:
                return false;

            default:
                Develop.DebugPrint(typ);
                return false;
        }
    }

    internal bool CompareValues(ColumnItem column, string filterValue, FilterType typ) => CompareValues(CellGetStringCore(column) ?? string.Empty, filterValue, typ);

    internal void DoSystemColumns(Table tb, ColumnItem column, string user, DateTime datetimeutc, Reason reason) {
        if (!reason.HasFlag(Reason.DoRepair)) { return; }

        if (column.RelationType == RelationType.CellValues) { return; }

        if (tb.Column.SysRowChanger is { IsDisposed: false } src && src != column) { CellSetInternal(src, user, Reason.NoUndo_NoInvalidate); }
        if (tb.Column.SysRowChangeDate is { IsDisposed: false } scd && scd != column) { CellSetInternal(scd, datetimeutc.ToString5(), Reason.NoUndo_NoInvalidate); }

        if (tb.Column.SysRowState is { IsDisposed: false } srs && srs != column && column.SaveContent) {
            InvalidateCheckData();

            if (column.ScriptType != ScriptType.Nicht_vorhanden || column.IsKeyColumn) {
                RowCollection.WaitDelay = 0;
                if (column.IsKeyColumn) {
                    CellSetInternal(srs, string.Empty, reason);
                } else {
                    if (!string.IsNullOrEmpty(CellGetString(srs))) {
                        CellSetInternal(srs, "01.01.1900", reason);
                    }
                }
            }
        }
    }

    internal bool IsMyRow(double maxminutes, bool mastertoo) {
        if (Table is not { IsDisposed: false } tb) { return false; }
        if (mastertoo && !tb.MultiUserPossible) { return true; }
        if (tb.Column.SysRowChanger is not { IsDisposed: false } src) { return false; }
        if (tb.Column.SysRowChangeDate is not { IsDisposed: false } srcd) { return false; }

        var t = DateTime.UtcNow.Subtract(CellGetDateTime(srcd));
        if (mastertoo && tb.AmITemporaryMaster(MasterTry, MasterUntil, true) && t.TotalMinutes > MyRowLost) { return true; }

        if (!string.Equals(CellGetString(src), Generic.UserName, StringComparison.OrdinalIgnoreCase)) { return false; }

        return t.TotalSeconds > 3 && t.TotalMinutes < maxminutes; // 3 Sekunde deswegen, weil machne Routinen gleich die Prüfung machen und ansonsten die Routine reingrätscht
    }

    internal void Repair() {
        if (Table is not { IsDisposed: false } tb) { return; }

        if (tb.Column.SysCorrect is { IsDisposed: false } sc) {
            if (string.IsNullOrEmpty(CellGetStringCore(sc))) {
                CellSetInternal(sc, true.ToPlusMinus(), Reason.NoUndo_NoInvalidate);
            }
        }

        if (tb.Column.SysLocked is { IsDisposed: false } sl) {
            if (string.IsNullOrEmpty(CellGetStringCore(sl))) {
                CellSetInternal(sl, false.ToPlusMinus(), Reason.NoUndo_NoInvalidate);
            }
        }

        if (tb.Column.SysRowKey is { IsDisposed: false } srk) {
            if (string.IsNullOrEmpty(CellGetStringCore(srk))) {
                CellSetInternal(srk, KeyName, Reason.NoUndo_NoInvalidate);
            }
        }
    }

    /// <summary>
    /// Setzt den Wert ohne Undo Logging
    /// </summary>
    /// <param name="column"></param>
    /// <param name="value"></param>
    /// <param name="reason"></param>
    internal string CellSetInternal(ColumnItem column, string value, Reason reason) {
        var tries = 0;
        var startTime = DateTime.UtcNow;

        while (true) {
            tries++; // Inkrementiere bei JEDEM Durchlauf, nicht nur bei Failures

            if (IsDisposed || column.Table is not { IsDisposed: false } tb) { return "Tabelle ungültig"; }
            // Timeout-Prüfung ODER tries-Limit - doppelte Sicherheit
            if (DateTime.UtcNow.Subtract(startTime).TotalSeconds > 20 || tries > 100) {
                return "Timeout: Wert konnte nicht gesetzt werden.";
            }

            var cellKey = CellCollection.KeyOfCell(column, this);

            if (tb.Cell.TryGetValue(cellKey, out var c)) {
                c.Value = value; // Auf jeden Fall setzen. Auch falls es nachher entfernt wird, so ist es sicher leer
                if (string.IsNullOrEmpty(value)) {
                    if (!tb.Cell.TryRemove(cellKey, out _)) {
                        // Exponential backoff: Wartezeit verdoppelt sich mit jedem Versuch
                        Thread.Sleep(Math.Min(tries * 10, 200)); // Max 200ms Wartezeit
                        continue;
                    }
                }
            } else {
                if (!string.IsNullOrEmpty(value)) {
                    if (!tb.Cell.TryAdd(cellKey, new CellItem(value))) {
                        // Exponential backoff: Wartezeit verdoppelt sich mit jedem Versuch
                        Thread.Sleep(Math.Min(tries * 10, 200)); // Max 200ms Wartezeit
                        continue;
                    }
                }
            }

            if (reason.HasFlag(Reason.LogUndo)) {
                if (column.IsKeyColumn) {
                    if (tb.Column.SysRowState is { IsDisposed: false } srs) {
                        CellSetInternal(srs, string.Empty, reason);
                    }
                }

                InvalidateCheckData();
                tb.Cell.OnCellValueChanged(new CellEventArgs(column, this));
            }

            return string.Empty;
        }
    }

    private static List<RowItem> ConnectedRowsOfRelations(string completeRelationText, RowItem? row) {
        List<RowItem> allRows = [];
        if (row?.Table?.Column.First == null || row.Table.IsDisposed) { return allRows; }

        var searchData = row.Table.Column.First?.GetCellContentsSortedByLength();
        if (searchData == null || searchData.Count == 0) { return allRows; }

        var relationTextLine = completeRelationText.ToUpperInvariant().SplitAndCutByCr();

        foreach (var thisTextLine in relationTextLine) {
            var tmp = thisTextLine;
            List<RowItem> rows = [];

            foreach (var (name, foundRow) in searchData) {
                if (tmp.IndexOfWord(name, 0, RegexOptions.IgnoreCase) > -1) {
                    rows.Add(foundRow);
                    tmp = tmp.Replace(name, string.Empty);
                }
            }

            if (rows.Count == 1 || rows.Contains(row)) {
                allRows.AddIfNotExists(rows);
            }
        }

        return allRows;
    }

    private static void MakeNewRelations(ColumnItem? column, RowItem? row, List<string> oldBz, IEnumerable<string> newBz) {
        if (row is not { IsDisposed: false }) { return; }
        if (column is not { IsDisposed: false }) { return; }

        //// Dann die neuen Erstellen
        foreach (var t in newBz) {
            if (!oldBz.Contains(t)) {
                var x = ConnectedRowsOfRelations(t, row);
                foreach (var thisRow in x) {
                    if (thisRow != null && thisRow != row) {
                        var ex = thisRow.CellGetList(column);
                        if (x.Contains(row)) {
                            ex.Add(t);
                        } else {
                            ex.Add(t.ReplaceWord(thisRow.CellFirstString(), row.CellFirstString(), RegexOptions.IgnoreCase));
                        }
                        thisRow.CellSet(column, ex.SortedDistinctList(), "Automatische Beziehungen von '" + row.CellFirstString() + "'");
                    }
                }
            }
        }
    }

    private void _table_Disposing(object? sender, System.EventArgs e) => Dispose();

    private string CellGetCompareKey(ColumnItem column) => CellCollection.CompareKey(column, this) ?? string.Empty;

    private string ChangeTextFromRowId(string completeRelationText) {
        if (Table is not { IsDisposed: false } tb) { return completeRelationText; }

        foreach (var rowItem in tb.Row) {
            if (rowItem != null) {
                completeRelationText = completeRelationText.Replace("/@X" + rowItem.KeyName + "X@/", rowItem.CellFirstString());
            }
        }
        return completeRelationText;
    }

    private string ChangeTextToRowId(string completeRelationText, string oldValue, string newValue, string keyOfCHangedRow) {
        if (Table is not { IsDisposed: false } tb) { return completeRelationText; }

        var c = tb.Column.First;
        if (c == null) { return completeRelationText; }

        var searchData = c.GetCellContentsSortedByLength();
        var didOld = false;
        var didNew = false;

        for (var z = searchData.Count - 1; z > -1; z--) {
            if (!didOld && searchData[z].value.Length <= oldValue.Length) {
                didOld = true;
                DoReplace(oldValue, keyOfCHangedRow);
            }
            if (!didNew && searchData[z].value.Length <= newValue.Length) {
                didNew = true;
                DoReplace(newValue, keyOfCHangedRow);
            }
            if (completeRelationText.Contains(searchData[z].value, StringComparison.OrdinalIgnoreCase)) {
                var r = searchData[z].row;
                if (r is { IsDisposed: false }) {
                    DoReplace(searchData[z].value, r.KeyName);
                }
            }
        }
        if (string.IsNullOrEmpty(newValue)) { return completeRelationText; }
        // Nochmal am Schluss, wenn die Wörter alle lang sind, und die nicht mehr zum ZUg kommen.
        if (oldValue.Length > newValue.Length) {
            DoReplace(oldValue, keyOfCHangedRow);
            DoReplace(newValue, keyOfCHangedRow);
        } else {
            DoReplace(newValue, keyOfCHangedRow);
            DoReplace(oldValue, keyOfCHangedRow);
        }
        return completeRelationText;
        void DoReplace(string name, string key) {
            if (!string.IsNullOrEmpty(name)) {
                completeRelationText = completeRelationText.Replace(name, "/@X" + key + "X@/", RegexOptions.IgnoreCase);
            }
        }
    }

    private string CheckUniqueValueConstraint(ColumnItem column, string newValue) {
        if (Table is not { IsDisposed: false } tb) { return string.Empty; }

        foreach (var uvd in tb.UniqueValues) {
            if (uvd.KeyColumns.All(c => c.KeyName != column.KeyName)) { continue; }

            var thisRowValues = uvd.KeyColumns.Select(c =>
                c.KeyName == column.KeyName ? newValue : CellGetStringCore(c)).ToList();

            foreach (var otherRow in tb.Row) {
                if (otherRow == this || otherRow is not { IsDisposed: false }) { continue; }

                var match = true;
                for (var i = 0; i < uvd.KeyColumns.Count; i++) {
                    var otherValue = otherRow.CellGetStringCore(uvd.KeyColumns[i]);
                    if (otherValue != thisRowValues[i]) {
                        match = false;
                        break;
                    }
                }

                if (match) {
                    return "Unique-Wert-Verletzung: Die Kombination aus " + string.Join(", ", uvd.KeyColumns.Select(c => c.Caption)) + " muss einzigartig sein.";
                }
            }
        }

        return string.Empty;
    }

    private void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                // Verwaltete Ressourcen (Instanzen von Klassen, Lists, Tasks,...)
                Table = null;
            }
            // Nicht verwaltete Ressourcen (Bitmap, Tabellenverbindungen, ...)
            InvalidateCheckData();
            OnDisposingEvent();
            IsDisposed = true;
        }
    }

    private void DoSpecialFormats(ColumnItem column, string previewsValue, string currentValue) {
        if (currentValue == previewsValue) { return; }

        if (column.Table is not { IsDisposed: false } tb) { return; }

        if (column.Relationship_to_First) { RepairRelationText(column, previewsValue); }

        if (column.Am_A_Key_For.Count > 0) {
            foreach (var thisColumn in tb.Column) {
                if (thisColumn.RelationType == RelationType.CellValues) {
                    LinkedCellData(thisColumn, true, false);
                }
            }
        }

        if (tb.Column.First is { IsDisposed: false } c && c == column) {
            foreach (var thisColumn in tb.Column) {
                if (column.Relationship_to_First) {
                    RelationTextNameChanged(thisColumn, KeyName, previewsValue, currentValue);
                }
            }
        }
    }

    private bool MatchesTo(ColumnItem column, FilterType filtertyp, ReadOnlyCollection<string> searchvalue) {
        //Grundlegendes zu UND und ODER:
        //Ein Filter kann mehrere Werte haben, diese müssen ein Attribut UND oder ODER haben.
        //Bei UND müssen alle Werte des Filters im Multiline vorkommen.
        //Bei ODER muss ein Wert des Filters im Multiline vorkommen.
        //Beispiel: UND-Filter mit C & D
        //Wenn die Zelle       A B C D hat, trifft der UND-Filter zwar nicht bei den ersten beiden zu, aber bei den letzten.
        //Um genau zu sein C:  - - + -
        //                 D:  - - - +
        //Deswegen muss beim einem UND-Filter nur EINER der Zellenwerte zutreffen.
        //if (Filter.FilterType == enFilterType.KeinFilter) { Develop.DebugError("Kein Filter angegeben: " + ToString()); }

        try {
            var typ = filtertyp;
            // Oder-Flag ermitteln --------------------------------------------
            var oder = typ.HasFlag(FilterType.ODER);
            if (oder) { typ ^= FilterType.ODER; }
            // Und-Flag Ermitteln --------------------------------------------
            var und = typ.HasFlag(FilterType.UND);
            if (und) { typ ^= FilterType.UND; }
            if (searchvalue.Count < 2) {
                oder = true;
                und = false; // Wenn nur EIN Eintrag gecheckt wird, ist es EGAL, ob UND oder ODER.
            }
            //if (Und && Oder) { Develop.DebugError("Filter-Anweisung erwartet ein 'Und' oder 'Oder': " + ToString()); }
            // Tatsächlichen String ermitteln --------------------------------------------

            if (!column.SaveContent) { CheckRow(); }

            var txt = CellGetStringCore(column);

            if (typ.HasFlag(FilterType.Instr)) { txt = LanguageTool.PrepaireText(txt, ShortenStyle.Both, string.Empty, string.Empty, column.DoOpticalTranslation, null); }
            // Multiline-Typ ermitteln  --------------------------------------------
            var tmpMultiLine = column.MultiLine;
            if (typ.HasFlag(FilterType.MultiRowIgnorieren)) {
                tmpMultiLine = false;
                typ ^= FilterType.MultiRowIgnorieren;
            }
            if (tmpMultiLine && !txt.Contains('\r')) { tmpMultiLine = false; } // Zeilen mit nur einem Eintrag können ohne Multiline behandelt werden.

            if (!tmpMultiLine) {
                var bedingungErfüllt = false;
                foreach (var t in searchvalue) {
                    bedingungErfüllt = CompareValues(txt, t, typ);
                    if (oder && bedingungErfüllt) { return true; }
                    if (und && !bedingungErfüllt) { return false; } // Bei diesem UND hier müssen allezutreffen, deshalb kann getrost bei einem False dieses zurückgegeben werden.
                }
                return bedingungErfüllt;
            }
            List<string> vorhandenWerte = [.. txt.SplitAndCutByCr()];
            if (vorhandenWerte.Count == 0) { vorhandenWerte.Add(string.Empty); }// Um den Filter, der nach 'Leere' Sucht, zu befriediegen

            // Diese Reihenfolge der For Next ist unglaublich wichtig:
            // Sind wenigere VORHANDEN vorhanden als FilterWerte, dann durchsucht diese Routine zu wenig Einträge,
            // bevor sie bei einem UND ein False zurückgibt
            foreach (var t1 in searchvalue) {
                var bedingungErfüllt = false;
                foreach (var t in vorhandenWerte) {
                    bedingungErfüllt = CompareValues(t, t1, typ);
                    if (oder && bedingungErfüllt) { return true; }// Irgendein vorhandener Value trifft zu!!! Super!!!
                    if (und && bedingungErfüllt) { break; }// Irgend ein vorhandener Value trifft zu, restliche Prüfung uninteresant
                }
                if (und && !bedingungErfüllt) // Einzelne UND konnte nicht erfüllt werden...
                {
                    return false;
                }
            }
            if (und) { return true; } // alle "Und" stimmen!
            return false; // Gar kein "Oder" trifft zu...
        } catch (Exception ex) {
            Develop.DebugPrint("Unerwarteter Filter-Fehler", ex);
            Generic.Pause(0.1, true);
            Develop.AbortAppIfStackOverflow();
            return MatchesTo(column, filtertyp, searchvalue);
        }
    }

    private void OnDisposingEvent() => DisposingEvent?.Invoke(this, System.EventArgs.Empty);

    private void OnRowChecked(RowPrepareFormulaEventArgs e) => RowChecked?.Invoke(this, e);

    private void RelationTextNameChanged(ColumnItem columnToRepair, string rowKey, string oldValue, string newValue) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        if (string.IsNullOrEmpty(newValue)) { return; }
        foreach (var thisRowItem in tb.Row) {
            var t = thisRowItem.CellGetString(columnToRepair);
            if (!string.IsNullOrEmpty(t)) {
                if (!string.IsNullOrEmpty(oldValue) && t.Contains(oldValue, StringComparison.OrdinalIgnoreCase)) {
                    t = ChangeTextToRowId(t, oldValue, newValue, rowKey);
                    t = ChangeTextFromRowId(t);
                    var t2 = t.SplitAndCutByCr().SortedDistinctList();
                    thisRowItem.CellSet(columnToRepair, t2, "Automatische Beziehungen, Namensänderung: " + oldValue + " -> " + newValue);
                }
                if (t.Contains(newValue, StringComparison.OrdinalIgnoreCase)) {
                    MakeNewRelations(columnToRepair, thisRowItem, [], [.. t.SplitAndCutByCr()]);
                }
            }
        }
    }

    private void RepairRelationText(ColumnItem column, string previewsValue) {
        var currentString = CellGetString(column);
        currentString = ChangeTextToRowId(currentString, string.Empty, string.Empty, string.Empty);
        currentString = ChangeTextFromRowId(currentString);
        if (currentString != CellGetString(column)) {
            CellSet(column, currentString, "Bezugstextänderung");
            return;
        }
        var oldBz = new List<string>(previewsValue.SplitAndCutByCr()).SortedDistinctList();
        var newBz = new List<string>(currentString.SplitAndCutByCr()).SortedDistinctList();
        // Zuerst Beziehungen LÖSCHEN
        foreach (var t in oldBz) {
            if (!newBz.Contains(t)) {
                var x = ConnectedRowsOfRelations(t, this);
                foreach (var thisRow in x) {
                    if (thisRow != this) {
                        var ex = thisRow.CellGetList(column);
                        _ = x.Contains(this)
                            ? ex.Remove(t)
                            : ex.Remove(t.ReplaceWord(thisRow.CellFirstString(), CellFirstString(), RegexOptions.IgnoreCase));
                        thisRow.CellSet(column, ex.SortedDistinctList(), "Bezugstextänderung / Löschung");
                    }
                }
            }
        }
        MakeNewRelations(column, this, oldBz, newBz);
    }

    private bool RowFilterMatch(string searchText) {
        if (string.IsNullOrEmpty(searchText)) { return true; }
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return false; }

        searchText = searchText.ToUpperInvariant();
        foreach (var thisColumnItem in tb.Column) {
            if (!thisColumnItem.IgnoreAtRowFilter) {
                var txt = CellGetString(thisColumnItem);
                txt = LanguageTool.PrepaireText(txt, ShortenStyle.Both, string.Empty, string.Empty, thisColumnItem.DoOpticalTranslation, null);
                if (!string.IsNullOrEmpty(txt) && txt.Contains(searchText, StringComparison.OrdinalIgnoreCase)) { return true; }
            }
        }
        return false;
    }

    internal string SetValueInternal(TableDataType type, string value) {
        if (type.IsObsolete()) { return string.Empty; }

        switch (type) {
            case TableDataType.RowKey:
                var oldKey = _keyName;
                _keyName = value.ToUpperInvariant();
                var f = Table?.Row.ChangeKey(oldKey, _keyName) ?? "Tabelle verworfen";

                if (!string.IsNullOrEmpty(f)) {
                    var reason = $"Schwerer Rowkey Umbenennungsfehler, {f}";
                    Table?.Freeze(reason);
                    return reason;
                }
                break;

            default:
                if (!string.Equals(type.ToString(), ((int)type).ToString1(), StringComparison.Ordinal)) {
                    return "Interner Fehler: Für den Datentyp '" + type + "' wurde keine Laderegel definiert.";
                }
                break;
        }
        return string.Empty;
    }

    #endregion
}