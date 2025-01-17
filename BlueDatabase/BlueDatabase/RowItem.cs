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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using BlueDatabase.Interfaces;
using BlueScript;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueBasics.Converter;

namespace BlueDatabase;

public sealed class RowItem : ICanBeEmpty, IDisposableExtended, IHasKeyName, IHasDatabase, IComparable, IEditable, ICanDropMessages {

    #region Fields

    public RowCheckedEventArgs? LastCheckedEventArgs;

    public string? LastCheckedMessage;

    public List<string>? LastCheckedRowFeedback;

    private Database? _database;

    private string? _tmpQuickInfo;

    #endregion

    #region Constructors

    public RowItem(Database database, string key) {
        Database = database;
        KeyName = key;
        _tmpQuickInfo = null;
    }

    #endregion

    #region Destructors

    ~RowItem() {
        Dispose(false);
    }

    #endregion

    #region Events

    public event EventHandler<MessageEventArgs>? DropMessage;

    public event EventHandler<RowCheckedEventArgs>? RowChecked;

    public event EventHandler<RowEventArgs>? RowGotData;

    #endregion

    #region Properties

    public string CaptionForEditor => "Zeile";

    public Database? Database {
        get => _database;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == _database) { return; }

            if (_database != null) {
                _database.DisposingEvent -= _database_Disposing;
                _database.Cell.CellValueChanged -= Cell_CellValueChanged;
            }
            _database = value;

            if (_database != null) {
                _database.DisposingEvent += _database_Disposing;
                _database.Cell.CellValueChanged += Cell_CellValueChanged;
            }
        }
    }

    public Type? Editor { get; set; }

    public bool IsDisposed { get; private set; }

    public string KeyName { get; private set; }

    public string QuickInfo {
        get {
            if (_tmpQuickInfo != null) { return _tmpQuickInfo; }
            GenerateQuickInfo();
            return _tmpQuickInfo!;
        }
    }

    /// <summary>
    /// Wie wichtig ein Update ist. Kleine Zahlen sind wichtiger.
    /// </summary>
    public long UrgencyUpdate {
        get {
            if (NeedsRowInitialization()) { return 0; }
            if (NeedsRowUpdateAfterChange()) { return 1; }
            if (NeedsRowUpdate(false, true)) { return 2; }

            if (Database?.Column.SysRowState is not { IsDisposed: false } srs) { return long.MaxValue; }

            return CellGetDateTime(srs).Ticks;
        }
    }

    #endregion

    #region Methods

    public static Variable? CellToVariable(ColumnItem? column, RowItem? row, bool mustbeReadOnly, bool virtualcolumns) {
        if (column is not { ScriptType: not (ScriptType.Nicht_vorhanden or ScriptType.undefiniert) }) { return null; }

        if (column.Function == ColumnFunction.Virtuelle_Spalte) {
            if (!virtualcolumns) { return null; }
            mustbeReadOnly = false;
        }

        if (!column.Function.CanBeCheckedByRules()) { return null; }
        //if (!column.SaveContent) { return null; }

        #region ReadOnly bestimmen

        var ro = mustbeReadOnly || !column.Function.CanBeChangedByRules();
        //if (column == column.Database.Column.SysCorrect) { ro = true; }
        //if (column == column.Database.Column.SysRowChanger) { ro = true; }
        //if (column == column.Database.Column.SysRowChangeDate) { ro = true; }

        #endregion

        var wert = row?.CellGetString(column) ?? string.Empty;

        var qi = "Spalte: " + column.ReadableText();

        switch (column.ScriptType) {
            case ScriptType.Bool:
                return new VariableBool(column.KeyName, wert == "+", ro, qi);

            case ScriptType.List:
                return new VariableListString(column.KeyName, wert.SplitAndCutByCrToList(), ro, qi);

            case ScriptType.Numeral:
                _ = FloatTryParse(wert, out var f);
                return new VariableFloat(column.KeyName, f, ro, qi);

            case ScriptType.String:
                return new VariableString(column.KeyName, wert, ro, qi);

            case ScriptType.String_Readonly:
                return new VariableString(column.KeyName, wert, true, qi);

            case ScriptType.Bool_Readonly:
                return new VariableBool(column.KeyName, wert == "+", true, qi);

            case ScriptType.List_Readonly:
                return new VariableListString(column.KeyName, wert.SplitAndCutByCrToList(), true, qi);

            case ScriptType.Row:
                return new VariableRowItem(column.KeyName, row?.Database?.Row.SearchByKey(wert), ro, qi);

            default:
                Develop.DebugPrint(column.ScriptType);
                return null;
        }
    }

    public string CellFirstString() => Database?.Column.First() is not { IsDisposed: false } fc ? string.Empty : CellGetString(fc);

    public bool CellGetBoolean(string columnName) => CellGetBoolean(Database?.Column[columnName]);

    public bool CellGetBoolean(ColumnItem? column) => Database?.Cell.GetString(column, this).FromPlusMinus() ?? default;

    public Color CellGetColor(string columnName) => CellGetColor(Database?.Column[columnName]);

    public Color CellGetColor(ColumnItem? column) => Color.FromArgb(CellGetInteger(column));

    public int CellGetColorBgr(ColumnItem? column) {
        var c = CellGetColor(column);
        int colorBlue = c.B;
        int colorGreen = c.G;
        int colorRed = c.R;
        return (colorBlue << 16) | (colorGreen << 8) | colorRed;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="columnName"></param>
    /// <returns>DateTime.MinValue bei Fehlern</returns>
    public DateTime CellGetDateTime(string columnName) => CellGetDateTime(Database?.Column[columnName]);

    /// <summary>
    ///
    /// </summary>
    /// <returns>DateTime.MinValue bei Fehlern</returns>
    public DateTime CellGetDateTime(ColumnItem? column) {
        var value = Database?.Cell.GetString(column, this);
        if (value == null) { return DateTime.MinValue; }
        return string.IsNullOrEmpty(value) ? default : DateTimeTryParse(value, out var d) ? d : DateTime.MinValue;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="column"></param>
    /// <returns>0 bei Fehlern</returns>
    public double CellGetDouble(string columnName) => CellGetDouble(Database?.Column[columnName]);

    /// <summary>
    ///
    /// </summary>
    /// <param name="column"></param>
    /// <returns>0 bei Fehlern</returns>
    public double CellGetDouble(ColumnItem? column) => DoubleParse(Database?.Cell.GetString(column, this));

    /// <summary>
    ///
    /// </summary>
    /// <param name="column"></param>
    /// <returns>0 bei Fehlern</returns>
    public int CellGetInteger(string columnName) => CellGetInteger(Database?.Column[columnName]);

    /// <summary>
    ///
    /// </summary>
    /// <param name="column"></param>
    /// <returns>0 bei Fehlern</returns>
    public int CellGetInteger(ColumnItem? column) => IntParse(Database?.Cell.GetString(column, this));

    public List<string> CellGetList(ColumnItem? column) => Database?.Cell.GetString(column, this).SplitAndCutByCrToList() ?? [];

    public List<string> CellGetList(string columnName) => CellGetList(Database?.Column[columnName]);

    /// <summary>
    ///
    /// </summary>
    /// <param name="column"></param>
    /// <returns>0 bei Fehlern</returns>
    public long CellGetLong(ColumnItem? column) => LongParse(Database?.Cell.GetString(column, this));

    public Point CellGetPoint(ColumnItem? column) // Main Method
    {
        var value = Database?.Cell.GetString(column, this);
        return string.IsNullOrEmpty(value) ? Point.Empty : value.PointParse();
    }

    public Point CellGetPoint(string columnName) => CellGetPoint(Database?.Column[columnName]);

    public string CellGetString(string columnName) => Database?.Cell.GetString(Database?.Column[columnName], this) ?? string.Empty;

    public string CellGetString(ColumnItem column) {
        if (Database is not { IsDisposed: false } || column.IsDisposed) { return string.Empty; }
        return Database.Cell.GetString(column, this);
    }

    //public List<string> CellGetValuesReadable(ColumnItem column, ShortenStyle style) => Database?.Cell.ValuesReadable(column, this, style) ?? [];

    public bool CellIsNullOrEmpty(string columnName) => Database?.Cell.IsNullOrEmpty(Database?.Column[columnName], this) ?? true;

    public bool CellIsNullOrEmpty(ColumnItem? column) => Database?.Cell.IsNullOrEmpty(column, this) ?? true;

    public void CellSet(string columnName, bool value, string comment) => Database?.Cell.Set(Database?.Column[columnName], this, value.ToPlusMinus(), comment);

    public void CellSet(ColumnItem column, bool value, string comment) => Database?.Cell.Set(column, this, value.ToPlusMinus(), comment);

    public void CellSet(string columnName, string value, string comment) => Database?.Cell.Set(Database?.Column[columnName], this, value, comment);

    public void CellSet(ColumnItem? column, string value, string comment) => Database?.Cell.Set(column, this, value, comment);

    public void CellSet(string columnName, double value, string comment) => Database?.Cell.Set(Database?.Column[columnName], this, value.ToStringFloat5(), comment);

    public void CellSet(ColumnItem column, double value, string comment) => Database?.Cell.Set(column, this, value.ToStringFloat5(), comment);

    public void CellSet(string columnName, int value, string comment) => Database?.Cell.Set(Database?.Column[columnName], this, value.ToString(), comment);

    public void CellSet(ColumnItem column, int value, string comment) => Database?.Cell.Set(column, this, value.ToString(), comment);

    public void CellSet(string columnName, Point value, string comment) => Database?.Cell.Set(Database?.Column[columnName], this, value.ToString(), comment);

    public void CellSet(ColumnItem column, Point value, string comment) => Database?.Cell.Set(column, this, value.ToString(), comment);

    public void CellSet(string columnName, List<string>? value, string comment) => Database?.Cell.Set(Database?.Column[columnName], this, value.JoinWithCr(), comment);

    public void CellSet(ColumnItem column, List<string>? value, string comment) => Database?.Cell.Set(column, this, value.JoinWithCr(), comment);

    public void CellSet(string columnName, DateTime value, string comment) => Database?.Cell.Set(Database?.Column[columnName], this, value.ToString5(), comment);

    public void CellSet(ColumnItem column, DateTime value, string comment) => Database?.Cell.Set(column, this, value.ToString5(), comment);

    public void CheckRowDataIfNeeded() {
        if (IsDisposed || Database is not { IsDisposed: false } db) {
            LastCheckedMessage = "Datenbank verworfen";
            return;
        }

        if (!string.IsNullOrEmpty(Database.ScriptNeedFix)) {
            LastCheckedMessage = "Skripte fehlerhaft und müssen repariert werden.";
            return;
        }

        if (RowCollection.FailedRows.Contains(this)) {
            LastCheckedMessage = "Das Skript konnte die Zeile nicht durchrechnen.";
            return;
        }
        if (LastCheckedEventArgs != null) { return; }

        var sef = ExecuteScript(ScriptEventTypes.prepare_formula, string.Empty, true, 0, null, true, false);

        LastCheckedMessage = "<b><u>" + CellFirstString() + "</b></u><br><br>";

        if (!sef.AllOk) {
            LastCheckedMessage += "Das Skript enthält Fehler und muss repariert werden.";
            return;
        }
        if (!sef.Successful) {
            LastCheckedMessage += "Das Skript konnte die Zeile nicht durchrechnen: " + sef.NotSuccessfulReason;
            return;
        }

        List<string> cols = [];

        var tmp = LastCheckedRowFeedback;
        if (tmp is { Count: > 0 }) {
            foreach (var thiss in tmp) {
                _ = cols.AddIfNotExists(thiss);
                var t = thiss.SplitBy("|");
                var thisc = db?.Column[t[0]];
                if (thisc != null) {
                    LastCheckedMessage = LastCheckedMessage + "<b>" + thisc.ReadableText() + ":</b> " + t[1] + "<br><hr><br>";
                }
            }
        } else {
            LastCheckedMessage += "Diese Zeile ist fehlerfrei.";
        }

        if (db?.Column.SysCorrect is { IsDisposed: false } sc) {
            if (IsNullOrEmpty(sc) || (cols.Count == 0) != CellGetBoolean(db.Column.SysCorrect)) {
                CellSet(db.Column.SysCorrect, cols.Count == 0, "Fehlerprüfung");

                var erg2 = ExecuteScript(ScriptEventTypes.correct_changed, string.Empty, true, 3, null, true, false);

                if (!erg2.AllOk) {
                    LastCheckedMessage += "Das Skripte enthalten Fehler und müssen repariert werden.";
                }
            }
        }

        LastCheckedEventArgs = new RowCheckedEventArgs(this, cols, sef.Variables);

        OnRowChecked(LastCheckedEventArgs);
    }

    //public void CloneFrom(RowItem source, bool nameAndKeyToo) {
    //    if (IsDisposed || Database is not { IsDisposed: false } db) { return; }

    //    var sdb = source.Database;
    //    if (sdb is null || sdb.IsDisposed) { return; }

    //    if (nameAndKeyToo) { KeyName = source.KeyName; }

    //    foreach (var thisColumn in db.Column) {
    //        var value = sdb.Cell.GetStringCore(sdb.Column[thisColumn.KeyName], source);

    //        _ = db.ChangeData(DatabaseDataType.Value_withoutSizeData, thisColumn, source, string.Empty, value, Generic.UserName, DateTime.UtcNow, "Zeilen-Klonung", );

    //        //Database.Cell.SetValueBehindLinkedValue(thisColumn, this, sdb.Cell.GetStringBehindLinkedValue(sdb.Column[thisColumn.KeyName], source), false);
    //    }
    //}

    public string CompareKey(List<ColumnItem> columns) {
        StringBuilder r = new();

        foreach (var t in columns) {
            if (t.LinkedDatabase is not { IsDisposed: false }) {
                // LinkedDatabase = null - Ansonsten wird beim Sortieren alles immer wieder geladen,
                _ = r.Append(CellGetCompareKey(t) + Constants.FirstSortChar);
            }
        }

        _ = r.Append(Constants.SecondSortChar + KeyName);
        return r.ToString();
    }

    public string CompareKey() {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return string.Empty; }

        var colsToRefresh = new List<ColumnItem>();
        if (db.SortDefinition?.Columns is { } lc) { colsToRefresh.AddRange(lc); }
        if (db.Column.SysChapter is { IsDisposed: false } csc) { _ = colsToRefresh.AddIfNotExists(csc); }
        if (db.Column.First() is { IsDisposed: false } cf) { _ = colsToRefresh.AddIfNotExists(cf); }

        return CompareKey(colsToRefresh);
    }

    public int CompareTo(object obj) {
        if (obj is RowItem tobj) {
            return string.Compare(CompareKey(), tobj.CompareKey(), StringComparison.OrdinalIgnoreCase);
        }

        Develop.DebugPrint(FehlerArt.Fehler, "Falscher Objecttyp!");
        return 0;
    }

    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Führt Regeln aus, löst Ereignisses, setzt SysCorrect und auch die initalwerte der Zellen.
    /// Z.b: Runden, Großschreibung wird nur bei einem FullCheck korrigiert, das wird normalerweise vor dem Setzen bei CellSet bereits korrigiert.
    /// </summary>
    /// <param name="scriptname"></param>
    /// <param name="doFemdZelleInvalidate">bei verlinkten Zellen wird der verlinkung geprüft und erneuert.</param>
    /// <param name="fullCheck">Runden, Großschreibung, etc. wird ebenfalls durchgefphrt</param>
    /// <param name="produktivphase"></param>
    /// <param name="tryforsceonds"></param>
    /// <param name="eventname"></param>
    /// <param name="extended">True, wenn valueChanged im erweiterten Modus aufgerufen wird</param>
    /// <returns>checkPerformed  = ob das Skript gestartet werden konnte und beendet wurde, error = warum das fehlgeschlagen ist, script dort sind die Skriptfehler gespeichert</returns>
    public ScriptEndedFeedback ExecuteScript(ScriptEventTypes? eventname, string scriptname, bool produktivphase, float tryforsceonds, List<string>? attributes, bool dbVariables, bool extended) {
        var m = Database.EditableErrorReason(Database, EditableErrorReasonType.EditAcut);
        if (!string.IsNullOrEmpty(m) || Database is not { } db) { return new ScriptEndedFeedback("Automatische Prozesse nicht möglich: " + m, false, false, "Allgemein"); }

        var t = DateTime.UtcNow;
        do {
            var erg = db.ExecuteScript(eventname, scriptname, produktivphase, this, attributes, dbVariables, extended);
            if (erg.AllOk) { return erg; }
            if (!erg.GiveItAnotherTry || DateTime.UtcNow.Subtract(t).TotalSeconds > tryforsceonds) { return erg; }
        } while (true);
    }

    public string Hash() {
        if (Database is not { IsDisposed: false } db) { return string.Empty; }

        var thisss = "Database=" + db.Caption + ";File=" + db.Filename + ";";

        foreach (var thisColumnItem in db.Column) {
            if (thisColumnItem.IsDisposed) { return string.Empty; }

            if (thisColumnItem.IsSystemColumn()) { continue; }

            thisss = thisss + thisColumnItem.KeyName + "=" + CellGetString(thisColumnItem) + ";";
        }

        return thisss.GetHashString();
    }

    public void InvalidateCheckData() {
        LastCheckedRowFeedback?.Clear();
        LastCheckedRowFeedback = null;
        LastCheckedEventArgs = null;
        LastCheckedMessage = null;
    }

    public void InvalidateRowState(string comment) {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return; }

        if (db.Column.SysRowState is { IsDisposed: false } srs) {
            CellSet(srs, string.Empty, comment);
        }

        if (db.Column.SysRowChangeDate is { IsDisposed: false } scd) {
            CellSet(scd, DateTime.UtcNow, comment);
        }
        RowCollection.InvalidatedRows.AddIfNotExists(this);

        //DoSystemColumns(db, null, Generic.UserName, DateTime.UtcNow, Reason.SetCommand);

        //q
        //CellSet(srs, string.Empty,comment);
    }

    public bool IsNullOrEmpty() {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return true; }
        return db.Column.All(thisColumnItem => thisColumnItem != null && CellIsNullOrEmpty(thisColumnItem));
    }

    public bool IsNullOrEmpty(ColumnItem? column) {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return true; }
        return db.Cell.IsNullOrEmpty(column, this);
    }

    public bool MatchesTo(FilterItem fi) {
        if (IsDisposed || Database is not { IsDisposed: false }) { return false; }

        if (fi.Database != Database) { return false; }

        if (fi.FilterType == FilterType.AlwaysFalse) { return false; }

        if (!fi.IsOk()) { return false; }

        if (fi.FilterType == FilterType.RowKey) {
            if (fi.SearchValue.Count != 1) { return false; }
            return KeyName == fi.SearchValue[0];
        }

        if (fi.Column == null) {
            if (!fi.FilterType.HasFlag(FilterType.GroßKleinEgal)) { fi.FilterType |= FilterType.GroßKleinEgal; }
            if (fi.FilterType is not FilterType.Instr_GroßKleinEgal and not FilterType.Instr_UND_GroßKleinEgal) { Develop.DebugPrint(FehlerArt.Fehler, "Zeilenfilter nur mit Instr möglich!"); }
            if (fi.SearchValue.Count < 1) { Develop.DebugPrint(FehlerArt.Fehler, "Zeilenfilter nur mit mindestens einem Wert möglich"); }

            return fi.SearchValue.All(RowFilterMatch);
        }

        if (!MatchesTo(fi.Column, fi.FilterType, fi.SearchValue)) { return false; }

        return true;
    }

    public bool MatchesTo(params FilterItem[]? filter) {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return false; }

        if (filter is not { Length: not 0 }) { return Database.Column.SplitColumn == null || db.PowerEdit; }

        if (Database.Column.SplitColumn is { } csp && !Database.PowerEdit) {
            var found = false;
            foreach (var thisF in filter) {
                if (thisF.Column == csp) { found = true; break; }
            }
            if (!found) { return false; }
        }

        if (filter.Length == 1) { return MatchesTo(filter[0]); }

        var ok = true;

        _ = Parallel.ForEach(filter, (thisFilter, state) => {
            if (!MatchesTo(thisFilter)) {
                ok = false;
                state.Break();
            }
        });

        return ok;
    }

    /// <summary>
    /// Gibt True zurück, wenn eine eigene Zeile initialisiert werden muss.
    /// Alter kleiner 5 Minuten.
    /// </summary>
    /// <returns></returns>
    public bool NeedsRowInitialization() {
        if (Database is not { IsDisposed: false } db) { return false; }
        if (db.Column.SysRowState is not { IsDisposed: false } srs) { return false; }
        if (db.Column.SysRowChanger is not { IsDisposed: false } src) { return false; }
        if (db.Column.SysRowChangeDate is not { IsDisposed: false } srcd) { return false; }

        if (RowCollection.FailedRows.Contains(this)) { return false; }

        if (!string.IsNullOrEmpty(CellGetString(srs))) { return false; }

        if (!string.Equals(CellGetString(src), Generic.UserName, StringComparison.OrdinalIgnoreCase)) { return false; }

        var t = DateTime.UtcNow.Subtract(CellGetDateTime(srcd));
        return t is { TotalSeconds: > 1, TotalMinutes: < 5 }; // 1 Sekunde deswegen, weil machne Routinen gleich die Prüfung machen und ansonsten die Routine reingrätscht
    }

    /// <summary>
    /// Gibt true zurück, wenn eine Zeile aktualisiert werden muss.
    /// Benutzer und Alter egal.
    /// </summary>
    /// <returns></returns>
    public bool NeedsRowUpdate(bool ignoreFailed, bool extendedAllowed) {
        if (Database is not { IsDisposed: false } db) { return false; }
        if (db.Column.SysRowState is not { IsDisposed: false } srs) { return false; }

        if (!extendedAllowed && string.IsNullOrEmpty(CellGetString(srs))) { return false; }

        if (CellGetDateTime(srs) >= Database.EventScriptVersion) { return false; }
        return ignoreFailed || !RowCollection.FailedRows.Contains(this);
    }

    /// <summary>
    /// Gibt true zurück, wenn eine eigene Zeile aktualisiert werden muss.
    /// Diese muss jünger als 20 Minuten sein und älter als 3 sekunden.
    /// </summary>
    /// <returns></returns>
    public bool NeedsRowUpdateAfterChange() {
        if (Database is not { IsDisposed: false } db) { return false; }
        //if (db.Column.SysRowState is not ColumnItem srs) { return false; }
        if (db.Column.SysRowChanger is not { IsDisposed: false } src) { return false; }
        if (db.Column.SysRowChangeDate is not { IsDisposed: false } srcd) { return false; }

        //if (string.IsNullOrEmpty(CellGetString(srs))) { return false; }  // Zu krass! Die muss komplett initialisiert werden! Wird vorher schon abgefragt

        if (!string.Equals(CellGetString(src), Generic.UserName, StringComparison.OrdinalIgnoreCase)) { return false; }

        var t = DateTime.UtcNow.Subtract(CellGetDateTime(srcd));
        return NeedsRowUpdate(false, false) && t is { TotalMinutes: < 20, TotalSeconds: > 3 };
    }

    public void OnDropMessage(FehlerArt type, string message) {
        if (IsDisposed) { return; }
        DropMessage?.Invoke(this, new MessageEventArgs(type, message));
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>True wenn alles in Ordnung ist</returns>
    public string RepairAllLinks() {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return "Datenbank verworfen"; }

        foreach (var thisColumn in db.Column) {
            if (thisColumn.Function == ColumnFunction.Verknüpfung_zu_anderer_Datenbank) {
                _ = CellCollection.LinkedCellData(thisColumn, this, true, false);

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
        if (IsDisposed || Database is not { IsDisposed: false } db) { return txt; }

        var erg = txt;

        if (varcol != null) {
            foreach (var vari in varcol) {
                if (!erg.Contains("~")) { return erg; }

                if (vari != null) {
                    if (erg.ToUpperInvariant().Contains("~" + vari.KeyName.ToUpperInvariant())) {
                        var replacewith = vari.SearchValue;

                        //if (vari is VariableString vs) { replacewith =  vs.v}

                        if (removeLineBreaks) {
                            replacewith = replacewith.Replace("\r\n", " ");
                            replacewith = replacewith.Replace("\r", " ");
                        }

                        erg = erg.Replace("~" + vari.KeyName.ToUpperInvariant() + "~", replacewith, RegexOptions.IgnoreCase);
                    }
                }
            }
        }

        // Variablen ersetzen
        foreach (var column in db.Column) {
            if (!erg.Contains("~")) { return erg; }

            if (column != null) {
                if (erg.ToUpperInvariant().Contains("~" + column.KeyName.ToUpperInvariant())) {
                    var replacewith = CellGetString(column);
                    //if (readableValue) { replacewith = CellItem.ValueReadable(replacewith, ShortenStyle.Replaced, BildTextVerhalten.Nur_Text, removeLineBreaks, column.Prefix, column.Suffix, column.DoOpticalTranslation, column.OpticalReplace); }

                    //if (varcol != null) {
                    //    if (varcol.Get(column.KeyName) is Variable v) { replacewith = v.SearchValue; }
                    //}

                    if (removeLineBreaks) {
                        replacewith = replacewith.Replace("\r\n", " ");
                        replacewith = replacewith.Replace("\r", " ");
                    }

                    erg = erg.Replace("~" + column.KeyName.ToUpperInvariant() + "~", replacewith, RegexOptions.IgnoreCase);
                }
            }
        }
        return erg;
    }

    public string RowStamp() {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return string.Empty; }

        var erg = string.Empty;
        foreach (var thisColumn in db.Column) {
            if (thisColumn is { IsDisposed: false }) {
                if (thisColumn.ScriptType is ScriptType.Nicht_vorhanden or ScriptType.undefiniert) { continue; }

                erg += CellGetString(thisColumn) + "|";
            }
        }
        return erg;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="onlyIfQuick"></param>
    /// <returns>Wenn alles in Ordung ist</returns>
    public ScriptEndedFeedback UpdateRow(bool extendedAllowed, bool important, string reason) {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return new ScriptEndedFeedback("Datenbank verworfen", false, false, "Allgemein"); }

        if (!important && Database.ExecutingScriptAnyDatabase.Count > 0) { return new ScriptEndedFeedback("Andere Skripte werden ausgeführt", false, false, "Allgemein"); }

        if (important) {
            var tim = Stopwatch.StartNew();

            while (db.ExecutingScript.Count > 0) {
                if (tim.Elapsed.TotalSeconds > 10) {
                    break;
                }
            }
        }

        if (db.Column.SysRowState is not { IsDisposed: false } srs) {
            return new ScriptEndedFeedback(new VariableCollection(), RepairAllLinks());
        }

        var hasScript = db.EventScript.Get(ScriptEventTypes.value_changed).Count;
        if (hasScript > 1) { return new ScriptEndedFeedback("Skripte fehlerhaft!", false, true, "Allgemein"); }

        var mustBeExtended = string.IsNullOrEmpty(CellGetString(srs)) || CellGetString(srs) == "0";

        if (!extendedAllowed && mustBeExtended) { return new ScriptEndedFeedback("Interner Fehler", false, false, "Allgemein"); }

        try {
            db.OnDropMessage(FehlerArt.Info, $"Aktualisiere Zeile: {CellFirstString()} der Datenbank {db.Caption} ({reason})");
            OnDropMessage(FehlerArt.Info, $"Aktualisiere ({reason})");

            if (extendedAllowed) {
                RowCollection.InvalidatedRows.Remove(this);
            }

            var ok = ExecuteScript(ScriptEventTypes.value_changed, string.Empty, true, 2, null, true, mustBeExtended);
            if (!ok.Successful) {
                db.OnDropMessage(FehlerArt.Info, $"Fehlgeschlagen: {CellFirstString()} der Datenbank {db.Caption} ({reason})");
                OnDropMessage(FehlerArt.Info, $"Fehlgeschlagen ({reason})");
                LastCheckedMessage = "Konnte intern nicht berechnet werden. Administrator verständigen." + ok.NotSuccessfulReason;

                RowCollection.FailedRows.AddIfNotExists(this);
                return ok;
            }

            if (!ok.AllOk) {
                //LastCheckedMessage = "Das Skript ist fehlerhaft. Administrator verständigen.\r\n\r\n" + ok.ProtocolText;
                return ok;
            }

            var reas = RepairAllLinks();

            if (!string.IsNullOrEmpty(reas)) {
                return new ScriptEndedFeedback(new VariableCollection(), reas);
            }

            CellSet(srs, DateTime.UtcNow, "Erfolgreiche Datenüberprüfung"); // Nicht System set, diese Änderung muss geloggt werden

            RowCollection.FailedRows.Remove(this);

            InvalidateCheckData();
            CheckRowDataIfNeeded();
            RowCollection.AddBackgroundWorker(this);
            db.OnInvalidateView();
            return ok;
        } catch {
            return new ScriptEndedFeedback("Interner Fehler", false, true, "Allgemein");
        }
    }

    public void VariableToCell(ColumnItem? column, VariableCollection vars, string scriptname) {
        var m = Database.EditableErrorReason(Database, EditableErrorReasonType.EditAcut);
        if (!string.IsNullOrEmpty(m) || Database is not { IsDisposed: false } || column == null) { return; }

        var columnVar = vars.Get(column.KeyName);
        if (columnVar is not { ReadOnly: false }) { return; }
        if (!column.Function.CanBeChangedByRules()) { return; }

        var comment = "Skript '" + scriptname + "'";

        switch (columnVar) {
            case VariableFloat vf:
                CellSet(column, vf.ValueNum, comment);
                break;

            case VariableListString vl:
                CellSet(column, vl.ValueList, comment);
                break;

            case VariableBool vb:
                CellSet(column, vb.ValueBool, comment);
                break;

            case VariableString vs:
                CellSet(column, vs.ValueString, comment);
                break;

            case VariableRowItem vr:
                var r = string.Empty;
                if (vr.RowItem is { IsDisposed: false } ro) { r = ro.KeyName; }
                CellSet(column, r, comment);
                break;

            default:
                Develop.DebugPrint("Typ nicht erkannt: " + columnVar.MyClassId);
                break;
        }
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
                return istValue.IndexOf(filterValue, comparisonType) >= 0;

            case FilterType.Between:
                var rangeParts = filterValue.Split(['|'], StringSplitOptions.RemoveEmptyEntries);
                if (rangeParts.Length != 2)
                    return false;

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

    internal bool CompareValues(ColumnItem column, string filterValue, FilterType typ) => CompareValues(_database?.Cell.GetStringCore(column, this) ?? string.Empty, filterValue, typ);

    internal void DoSystemColumns(Database db, ColumnItem column, string user, DateTime datetimeutc, Reason reason) {
        if (reason == Reason.NoUndo_NoInvalidate) { return; }

        // Die unterschiedlichen Reasons in der Routine beachten!
        if (db.Column.SysRowChanger is { IsDisposed: false } src && src != column) { SetValueInternal(src, user, Reason.NoUndo_NoInvalidate); }
        if (db.Column.SysRowChangeDate is { IsDisposed: false } scd && scd != column) { SetValueInternal(scd, datetimeutc.ToString5(), Reason.NoUndo_NoInvalidate); }

        if (db.Column.SysRowState is { IsDisposed: false } srs && srs != column) {
            RowCollection.FailedRows.Remove(this);

            if (column.ScriptType != ScriptType.Nicht_vorhanden) {
                //if (reason != Reason.UpdateChanges)
                //{
                //    RowCollection.InvalidatedRows.AddIfNotExists(this);
                //}

                RowCollection.WaitDelay = 0;

                if (column.Function is ColumnFunction.Schlüsselspalte or ColumnFunction.First) {
                    SetValueInternal(srs, string.Empty, reason);
                } else {
                    if (!string.IsNullOrEmpty(CellGetString(srs))) {
                        SetValueInternal(srs, "01.01.1900", reason);
                    }
                }
            }
        }
    }

    internal void Repair() {
        if (Database is not { IsDisposed: false } db) { return; }

        if (db.Column.SysCorrect is { IsDisposed: false } sc) {
            if (string.IsNullOrEmpty(db.Cell.GetStringCore(sc, this))) {
                SetValueInternal(sc, true.ToPlusMinus(), Reason.NoUndo_NoInvalidate);
            }
        }

        if (db.Column.SysLocked is { IsDisposed: false } sl) {
            if (string.IsNullOrEmpty(db.Cell.GetStringCore(sl, this))) {
                SetValueInternal(sl, false.ToPlusMinus(), Reason.NoUndo_NoInvalidate);
            }
        }
    }

    /// <summary>
    /// Setzt den Wert ohne Undo Logging
    /// </summary>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <param name="value"></param>
    /// <param name="reason"></param>
    internal string SetValueInternal(ColumnItem column, string value, Reason reason) {
        var tries = 0;

        while (true) {
            if (IsDisposed || column.Database is not { IsDisposed: false } db) { return "Datenbank ungültig"; }
            if (tries > 100) { return "Wert konnte nicht gesetzt werden."; }

            var cellKey = CellCollection.KeyOfCell(column, this);

            if (db.Cell.TryGetValue(cellKey, out var c)) {
                c.Value = value; // Auf jeden Fall setzen. Auch falls es nachher entfernt wird, so ist es sicher leer
                if (string.IsNullOrEmpty(value)) {
                    if (!db.Cell.TryRemove(cellKey, out _)) {
                        tries++;
                        continue;
                    }
                }
            } else {
                if (!string.IsNullOrEmpty(value)) {
                    if (!db.Cell.TryAdd(cellKey, new CellItem(value))) {
                        tries++;
                        continue;
                    }
                }
            }

            if (reason == Reason.SetCommand) {
                if (column.ScriptType != ScriptType.Nicht_vorhanden) {
                    if (column.Function is ColumnFunction.Schlüsselspalte or ColumnFunction.First) {
                        if (db.Column.SysRowState is { IsDisposed: false } srs) {
                            SetValueInternal(srs, string.Empty, reason);
                        }
                    }
                }

                //column.Invalidate_ContentWidth();
                InvalidateCheckData();
                db.Cell.OnCellValueChanged(new CellEventArgs(column, this));
            }

            return string.Empty;
        }
    }

    private void _database_Disposing(object sender, System.EventArgs e) => Dispose();

    private void Cell_CellValueChanged(object sender, CellEventArgs e) {
        if (e.Row != this) { return; }
        _tmpQuickInfo = null;
    }

    private string CellGetCompareKey(ColumnItem column) => Database?.Cell.CompareKey(column, this) ?? string.Empty;

    private void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                // Verwaltete Ressourcen (Instanzen von Klassen, Lists, Tasks,...)
                Database = null;
            }
            // Nicht verwaltete Ressourcen (Bitmap, Datenbankverbindungen, ...)
            _tmpQuickInfo = null;
            InvalidateCheckData();

            IsDisposed = true;
        }
    }

    private void GenerateQuickInfo() {
        if (Database is not { IsDisposed: false } ||
            string.IsNullOrEmpty(Database.ZeilenQuickInfo)) {
            _tmpQuickInfo = string.Empty;
            return;
        }

        _tmpQuickInfo = ReplaceVariables(Database.ZeilenQuickInfo, false, null);
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
        //if (Filter.FilterType == enFilterType.KeinFilter) { Develop.DebugPrint(enFehlerArt.Fehler, "Kein Filter angegeben: " + ToString()); }

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
            //if (Und && Oder) { Develop.DebugPrint(enFehlerArt.Fehler, "Filter-Anweisung erwartet ein 'Und' oder 'Oder': " + ToString()); }
            // Tatsächlichen String ermitteln --------------------------------------------
            string txt;

            switch (column.Function) {
                //case ColumnFunction.Verknüpfung_zu_anderer_Datenbank:
                //    var (columnItem, rowItem, _, _) = CellCollection.LinkedCellData(column, this, false, false);
                //    if (columnItem != null && rowItem != null) {
                //        txt = rowItem.CellGetString(columnItem);
                //    }
                //    break;

                case ColumnFunction.Virtuelle_Spalte:
                    CheckRowDataIfNeeded();
                    txt = _database?.Cell.GetStringCore(column, this) ?? string.Empty;
                    break;

                default:
                    txt = _database?.Cell.GetStringCore(column, this) ?? string.Empty;
                    break;
            }

            if (typ.HasFlag(FilterType.Instr)) { txt = LanguageTool.PrepaireText(txt, ShortenStyle.Both, string.Empty, string.Empty, column.DoOpticalTranslation, null); }
            // Multiline-Typ ermitteln  --------------------------------------------
            var tmpMultiLine = column.MultiLine;
            if (typ.HasFlag(FilterType.MultiRowIgnorieren)) {
                tmpMultiLine = false;
                typ ^= FilterType.MultiRowIgnorieren;
            }
            if (tmpMultiLine && !txt.Contains("\r")) { tmpMultiLine = false; } // Zeilen mit nur einem Eintrag können ohne Multiline behandelt werden.

            if (!tmpMultiLine) {
                var bedingungErfüllt = false;
                foreach (var t in searchvalue) {
                    bedingungErfüllt = CompareValues(txt, t, typ);
                    if (oder && bedingungErfüllt) { return true; }
                    if (und && bedingungErfüllt == false) { return false; } // Bei diesem UND hier müssen allezutreffen, deshalb kann getrost bei einem False dieses zurückgegeben werden.
                }
                return bedingungErfüllt;
            }
            List<string> vorhandenWerte = [.. txt.SplitAndCutByCr()];
            if (vorhandenWerte.Count == 0) { vorhandenWerte.Add(""); }// Um den Filter, der nach 'Leere' Sucht, zu befriediegen

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
            Develop.CheckStackForOverflow();
            return MatchesTo(column, filtertyp, searchvalue);
        }
    }

    private void OnRowChecked(RowCheckedEventArgs e) => RowChecked?.Invoke(this, e);

    private void OnRowGotData(RowEventArgs e) => RowGotData?.Invoke(this, e);

    private bool RowFilterMatch(string searchText) {
        if (string.IsNullOrEmpty(searchText)) { return true; }
        if (IsDisposed || Database is not { IsDisposed: false } db) { return false; }

        searchText = searchText.ToUpperInvariant();
        foreach (var thisColumnItem in db.Column) {
            {
                if (!thisColumnItem.IgnoreAtRowFilter) {
                    var txt = CellGetString(thisColumnItem);
                    txt = LanguageTool.PrepaireText(txt, ShortenStyle.Both, string.Empty, string.Empty, thisColumnItem.DoOpticalTranslation, null);
                    if (!string.IsNullOrEmpty(txt) && txt.ToUpperInvariant().Contains(searchText)) { return true; }
                }
            }
        }
        return false;
    }

    #endregion
}