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
using BlueScript;
using BlueScript.Structures;
using BlueScript.Variables;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static BlueBasics.Converter;

namespace BlueDatabase;

public sealed class RowItem : ICanBeEmpty, IDisposableExtended, IHasKeyName, IHasDatabase, IComparable, IEditable {

    #region Fields

    public RowCheckedEventArgs? LastCheckedEventArgs;
    public string? LastCheckedMessage;
    public List<string>? LastCheckedRowFeedback;
    private Database? _database;
    private DateTime? _isInCache;
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

    public DateTime? IsInCache {
        get => _isInCache;
        set {
            _isInCache = value;
            OnRowGotData(new RowEventArgs(this));
        }
    }

    public string KeyName { get; private set; }

    public string QuickInfo {
        get {
            if (_tmpQuickInfo != null) { return _tmpQuickInfo; }
            GenerateQuickInfo();
            return _tmpQuickInfo!;
        }
    }

    #endregion

    #region Methods

    public static Variable? CellToVariable(ColumnItem? column, RowItem? row, bool mustbeReadOnly, bool virtualcolumns) {
        if (column == null) { return null; }
        if (column.ScriptType is ScriptType.Nicht_vorhanden or ScriptType.undefiniert) { return null; }

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

    public static DateTime TimeCodeToUTCDateTime(long timeCode) {
        long originalTicks = (timeCode * 5000) + new DateTime(2024, 1, 1).Ticks;
        return new DateTime(originalTicks, DateTimeKind.Utc);
    }

    public static long TimeCodeUTCNow() {
        var t = DateTime.UtcNow.Ticks - new DateTime(2024, 1, 1).Ticks;
        return t / 5000;
    }

    public string CellFirstString() => Database?.Column.First() is not ColumnItem fc ? string.Empty : CellGetString(fc);

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
        if (Database is not Database db || db.IsDisposed || column.IsDisposed) { return string.Empty; }
        return Database.Cell.GetString(column, this);
    }

    public List<string> CellGetValuesReadable(ColumnItem column, ShortenStyle style) => Database?.Cell.ValuesReadable(column, this, style) ?? [];

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
        if (IsDisposed || Database is not Database db || db.IsDisposed) {
            LastCheckedMessage = "Datenbank verworfen";
            return;
        }

        if (!string.IsNullOrEmpty(Database.ScriptNeedFix)) {
            LastCheckedMessage = "Skripte fehlerhaft";
            return;
        }

        if (LastCheckedEventArgs != null) { return; }

        //_ = Database.RefreshRowData(this, false);

        //if (IsInCache == null) { Develop.DebugPrint(FehlerArt.Fehler, "Refresh-Fehler"); }

        var sef = ExecuteScript(ScriptEventTypes.prepare_formula, string.Empty, false, false, true, 0, null, true, false);

        LastCheckedMessage = "<b><u>" + CellFirstString() + "</b></u><br><br>";

        List<string> cols = [];

        var tmp = LastCheckedRowFeedback;
        if (tmp != null && tmp.Count > 0) {
            foreach (var thiss in tmp) {
                _ = cols.AddIfNotExists(thiss);
                var t = thiss.SplitBy("|");
                var thisc = db?.Column[t[0]];
                if (thisc != null) {
                    LastCheckedMessage = LastCheckedMessage + "<b>" + thisc.ReadableText() + ":</b> " + t[1] + "<br><hr><br>";
                }
            }
        }

        if (cols.Count == 0) {
            LastCheckedMessage += "Diese Zeile ist fehlerfrei.";
        }

        if (db?.Column.SysCorrect != null) {
            if (IsNullOrEmpty(db.Column.SysCorrect) || cols.Count == 0 != CellGetBoolean(db.Column.SysCorrect)) {
                CellSet(db.Column.SysCorrect, cols.Count == 0, "Fehlerprüfung");
            }
        }

        LastCheckedEventArgs = new RowCheckedEventArgs(this, cols, sef.Variables);

        OnRowChecked(LastCheckedEventArgs);
    }

    public void CloneFrom(RowItem source, bool nameAndKeyToo) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }

        var sdb = source.Database;
        if (sdb is null || sdb.IsDisposed) { return; }

        if (nameAndKeyToo) { KeyName = source.KeyName; }

        foreach (var thisColumn in db.Column) {
            var value = sdb.Cell.GetStringCore(sdb.Column[thisColumn.KeyName], source);

            _ = db.ChangeData(DatabaseDataType.Value_withoutSizeData, thisColumn, source, string.Empty, value, Generic.UserName, DateTime.UtcNow, "Zeilen-Klonung");

            //Database.Cell.SetValueBehindLinkedValue(thisColumn, this, sdb.Cell.GetStringBehindLinkedValue(sdb.Column[thisColumn.KeyName], source), false);
        }
    }

    public string CompareKey(List<ColumnItem> columns) {
        StringBuilder r = new();

        foreach (var t in columns) {
            if (t.LinkedDatabase is not Database db || db.IsDisposed) {
                // LinkedDatabase = null - Ansonsten wird beim Sortieren alles immer wieder geladen,
                _ = r.Append(CellGetCompareKey(t) + Constants.FirstSortChar);
            }
        }

        _ = r.Append(Constants.SecondSortChar + KeyName);
        return r.ToString();
    }

    public string CompareKey() {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return string.Empty; }

        var colsToRefresh = new List<ColumnItem>();
        if (db.SortDefinition?.Columns is List<ColumnItem> lc) { colsToRefresh.AddRange(lc); }
        if (db.Column.SysChapter is ColumnItem csc) { _ = colsToRefresh.AddIfNotExists(csc); }
        if (db.Column.First() is ColumnItem cf) { _ = colsToRefresh.AddIfNotExists(cf); }

        db.RefreshColumnsData(colsToRefresh.ToArray());

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
    public ScriptEndedFeedback ExecuteScript(ScriptEventTypes? eventname, string scriptname, bool doFemdZelleInvalidate, bool fullCheck, bool produktivphase, float tryforsceonds, List<string>? attributes, bool dbVariables, bool extended) {
        var m = Database.EditableErrorReason(Database, EditableErrorReasonType.EditAcut);
        if (!string.IsNullOrEmpty(m) || Database is null) { return new ScriptEndedFeedback("Automatische Prozesse nicht möglich: " + m, false, false, "Allgemein"); }

        var t = DateTime.UtcNow;
        do {
            var erg = ExecuteScript(eventname, scriptname, doFemdZelleInvalidate, fullCheck, produktivphase, attributes, dbVariables, extended);
            if (erg.AllOk) { return erg; }
            if (!erg.GiveItAnotherTry || DateTime.UtcNow.Subtract(t).TotalSeconds > tryforsceonds) { return erg; }
        } while (true);
    }

    public string Hash() {
        if (Database is not Database db || db.IsDisposed) { return string.Empty; }

        var thisss = "Database=" + db.Caption + ";File=" + db.Filename + ";";

        foreach (var thisColumnItem in db.Column) {
            if (thisColumnItem.IsDisposed) { return string.Empty; }

            if (thisColumnItem.IsSystemColumn()) { continue; }

            thisss = thisss + thisColumnItem.KeyName + "=" + CellGetString(thisColumnItem) + ";";
        }

        return Generic.GetHashString(thisss);
    }

    public void InvalidateCheckData() {
        LastCheckedRowFeedback = null;
        LastCheckedEventArgs = null;
        LastCheckedMessage = null;
    }

    public bool IsNullOrEmpty() {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return true; }
        return db.Column.All(thisColumnItem => thisColumnItem != null && CellIsNullOrEmpty(thisColumnItem));
    }

    public bool IsNullOrEmpty(ColumnItem? column) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return true; }
        return db.Cell.IsNullOrEmpty(column, this);
    }

    public bool MatchesTo(FilterItem fi) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return false; }

        if (fi.IsDisposed) { return true; }

        if (fi.Database != Database) { return false; }

        if (fi.FilterType == FilterType.AlwaysFalse) { return false; }

        if (!fi.IsOk()) { return false; }

        //fi.Column?.RefreshColumnsData(); // Muss beim Ändern der Colum Property ausgeführt werden

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
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return false; }
        if (filter == null || filter.Length == 0) { return true; }

        //Database.RefreshColumnsData(filter);
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
    /// Alter Egal.
    /// </summary>
    /// <returns></returns>
    public bool NeedsRowInitialization() {
        if (Database is not Database db || db.IsDisposed) { return false; }
        if (db.Column.SysRowState is not ColumnItem srs) { return false; }
        if (!string.IsNullOrEmpty(CellGetString(srs))) { return false; }
        if (db.Column.SysRowChanger is not ColumnItem src) { return false; }
        return string.Equals(CellGetString(src), Generic.UserName, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gibt true zurück, wenn eine Zeile aktualisiert werden muss.
    /// Benutzer und Alter egal.
    /// </summary>
    /// <returns></returns>
    public bool NeedsRowUpdate() => Database?.Column.SysRowState is ColumnItem srs &&
                                         CellGetLong(srs) < Database.EventScriptVersion;

    /// <summary>
    /// Gibt true zurück, wenn eine eigene Zeile aktualisiert werden.
    /// Diese muss jünger als 5 Minuten sein und älter als 3 sekunden.
    /// </summary>
    /// <returns></returns>
    public bool NeedsRowUpdateAfterChange() => NeedsRowUpdate() &&
                                           Database?.Column.SysRowChanger is ColumnItem srcr &&
                                           Database?.Column.SysRowChangeDate is ColumnItem srcd &&
                                           string.Equals(CellGetString(srcr), Generic.UserName, StringComparison.OrdinalIgnoreCase) &&
                                           DateTime.UtcNow.Subtract(CellGetDateTime(srcd)).TotalMinutes < 5 &&
                                           DateTime.UtcNow.Subtract(CellGetDateTime(srcd)).TotalSeconds > 3;

    /// <summary>
    ///
    /// </summary>
    /// <returns>True wenn alles in Ordnung ist</returns>
    public bool RepairAllLinks() {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return false; }

        foreach (var thisColumn in db.Column) {
            if (thisColumn.Function is ColumnFunction.Verknüpfung_zu_anderer_Datenbank or ColumnFunction.Verknüpfung_zu_anderer_Datenbank2) {
                _ = CellCollection.LinkedCellData(thisColumn, this, true, false);

                //if (!string.IsNullOrEmpty(info) && !canrepair) { return false; }
            }
        }
        return true;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="txt"></param>
    /// <param name="readableValue">Wenn true, wird der Wert durch den leserlichen Zelleninhalt ersetzt. Bei False durch den origial Zelleninhalt</param>
    /// <param name="removeLineBreaks"></param>
    /// <param name="varcol">Wir eine Collection angegeben, werden zuerst diese Werte benutzt - falls vorhanden - anstelle des Wertes in der Zeile </param>
    /// <returns></returns>
    public string ReplaceVariables(string txt, bool readableValue, bool removeLineBreaks, VariableCollection? varcol) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return txt; }

        var erg = txt;

        if (varcol != null) {
            foreach (var vari in varcol) {
                if (!erg.Contains("~")) { return erg; }

                if (vari != null) {
                    if (erg.ToUpperInvariant().Contains("~" + vari.KeyName.ToUpperInvariant())) {
                        var replacewith = vari.SearchValue;

                        //if (vari is VariableString vs) { replacewith =  vs.v}

                        if (removeLineBreaks && !readableValue) {
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
                    if (readableValue) { replacewith = CellItem.ValueReadable(replacewith, ShortenStyle.Replaced, BildTextVerhalten.Nur_Text, removeLineBreaks, column.Prefix, column.Suffix, column.DoOpticalTranslation, column.OpticalReplace); }

                    //if (varcol != null) {
                    //    if (varcol.Get(column.KeyName) is Variable v) { replacewith = v.SearchValue; }
                    //}

                    if (removeLineBreaks && !readableValue) {
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
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return string.Empty; }

        var erg = string.Empty;
        foreach (var thisColumn in db.Column) {
            if (thisColumn != null && !thisColumn.IsDisposed) {
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
    public bool UpdateRow(bool onlyIfQuick, bool mustDoFullCheck, bool wichtig) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return false; }

        if (!wichtig && Database.ExecutingScriptAnyDatabase > 0) { return false; }

        if (wichtig) {
            var t = new Stopwatch();
            t.Start();

            while (db.ExecutingScript > 0) {
                if (t.ElapsedMilliseconds > 10000) {
                    break;
                }
            }
        }

        //if (db.ExecutingScript > 0) { return false; }
        if (db.Column.SysRowState is not ColumnItem srs) { return RepairAllLinks(); }

        var large = db.EventScript.Get(ScriptEventTypes.value_changed).Count;
        if (large > 1) { return false; }

        mustDoFullCheck = mustDoFullCheck || (large == 1 && string.IsNullOrEmpty(CellGetString(srs)));

        if (onlyIfQuick && mustDoFullCheck) { return false; }

        try {
            db.OnDropMessage(FehlerArt.Info, $"Aktualisiere Zeile: {CellFirstString()} der Datenbank {db.Caption}");

            var ok = ExecuteScript(ScriptEventTypes.value_changed, string.Empty, true, true, true, 2, null, true, mustDoFullCheck);
            if (!ok.AllOk) { return false; }

            if (!RepairAllLinks()) { return false; }

            CellSet(srs, TimeCodeUTCNow(), "Erfolgreiche Datenüberprüfung"); // Nicht System set, diese Änderung muss geloggt werden

            InvalidateCheckData();
            CheckRowDataIfNeeded();
            RowCollection.AddBackgroundWorker(this);
            db.OnInvalidateView();
            return true;
        } catch {
            return false;
        }
    }

    public void VariableToCell(ColumnItem? column, VariableCollection vars, string scriptname) {
        var m = Database.EditableErrorReason(Database, EditableErrorReasonType.EditAcut);
        if (!string.IsNullOrEmpty(m) || Database is not Database db || db.IsDisposed || column == null) { return; }

        var columnVar = vars.Get(column.KeyName);
        if (columnVar == null || columnVar.ReadOnly) { return; }
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
                if (vr.RowItem is RowItem ro) { r = ro.KeyName; }
                CellSet(column, r, comment);
                break;

            default:
                Develop.DebugPrint("Typ nicht erkannt: " + columnVar.MyClassId);
                break;
        }
    }

    internal static bool CompareValues(string istValue, string filterValue, FilterType typ) {
        StringComparison comparisonType = typ.HasFlag(FilterType.GroßKleinEgal) ? StringComparison.OrdinalIgnoreCase
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
                } else if (DateTimeTryParse(istValue, out var dateValue)) {
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
            LastCheckedEventArgs = null;
            LastCheckedMessage = null;
            LastCheckedRowFeedback?.Clear();
            LastCheckedRowFeedback = null;

            IsDisposed = true;
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="eventname"></param>
    /// <param name="scriptname"></param>
    /// <param name="doFemdZelleInvalidate"></param>
    /// <param name="fullCheck"></param>
    /// <param name="produktivphase"></param>
    /// <param name="attributes"></param>
    /// <param name="dbVariables"></param>
    /// <param name="extended">True, wenn valueChanged im erweiterten Modus aufgerufen wird</param>
    /// <returns></returns>
    private ScriptEndedFeedback ExecuteScript(ScriptEventTypes? eventname, string scriptname, bool doFemdZelleInvalidate, bool fullCheck, bool produktivphase, List<string>? attributes, bool dbVariables, bool extended) {
        var m = Database.EditableErrorReason(Database, EditableErrorReasonType.EditAcut);
        if (!string.IsNullOrEmpty(m) || Database is not Database db || db.IsDisposed) { return new ScriptEndedFeedback("Automatische Prozesse nicht möglich: " + m, false, false, "Allgemein"); }

        var feh = db.EditableErrorReason(EditableErrorReasonType.EditAcut);
        if (!string.IsNullOrEmpty(feh)) { return new ScriptEndedFeedback(feh, true, false, "Allgemein"); }

        // Zuerst die Aktionen ausführen und falls es einen Fehler gibt, die Spalten und Fehler auch ermitteln
        var script = db.ExecuteScript(eventname, scriptname, produktivphase, this, attributes, dbVariables, extended);

        if (!script.AllOk) {
            //db.OnScriptError(new RowScriptCancelEventArgs(this, script.ProtocolText, script.ScriptHasSystaxError));
            if (script.ScriptNeedFix) {
                db.ScriptNeedFix = "Zeile: " + CellFirstString() + "\r\n\r\n" + script.ProtocolText;
            }

            return script;// (true, "<b>Das Skript ist fehlerhaft:</b>\r\nZeile: " + script.Line + "\r\n" + script.Error + "\r\n" + script.ErrorCode, script);
        }

        // Nicht ganz optimal, da ein Script ebenfalls den Flag changevalues hat. Aber hier wird nur auf den Flag eingenangen, ob es eine Testroutine ist oder nicht
        if (eventname is ScriptEventTypes.prepare_formula
            or ScriptEventTypes.value_changed_extra_thread
            or ScriptEventTypes.export
            or ScriptEventTypes.row_deleting) { return script; }

        if (!produktivphase) { return script; }

        // Dann die abschließenden Korrekturen vornehmen
        foreach (var thisColum in db.Column) {
            if (thisColum != null) {
                if (fullCheck) {
                    var x = CellGetString(thisColum);
                    var x2 = thisColum.AutoCorrect(x, true);
                    if (thisColum.Function is not ColumnFunction.Verknüpfung_zu_anderer_Datenbank and not ColumnFunction.Verknüpfung_zu_anderer_Datenbank2 && x != x2) {
                        db.Cell.Set(thisColum, this, x2, "Nach Skript-Korrekturen");
                    } else {
                        if (!thisColum.IsFirst()) {
                            db.Cell.DoSpecialFormats(thisColum, this, CellGetString(thisColum), true);
                        }
                    }
                    doFemdZelleInvalidate = false; // Hier ja schon bei jedem gemacht
                }

                if (doFemdZelleInvalidate && thisColum.LinkedDatabase != null) {
                    thisColum.Invalidate_ContentWidth();
                }
            }
        }

        return script;
    }

    private void GenerateQuickInfo() {
        if (Database is not Database db || db.IsDisposed ||
            string.IsNullOrEmpty(Database.ZeilenQuickInfo)) {
            _tmpQuickInfo = string.Empty;
            return;
        }

        _tmpQuickInfo = ReplaceVariables(Database.ZeilenQuickInfo, true, false, null);
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
            var txt = string.Empty;

            switch (column.Function) {
                case ColumnFunction.Verknüpfung_zu_anderer_Datenbank:
                    var (columnItem, rowItem, _, _) = CellCollection.LinkedCellData(column, this, false, false);
                    if (columnItem != null && rowItem != null) {
                        txt = rowItem.CellGetString(columnItem);
                    }
                    break;

                case ColumnFunction.Virtuelle_Spalte:
                    CheckRowDataIfNeeded();
                    txt = _database?.Cell.GetStringCore(column, this) ?? string.Empty;
                    break;

                default:
                    txt = _database?.Cell.GetStringCore(column, this) ?? string.Empty;
                    break;
            }

            if (typ.HasFlag(FilterType.Instr)) { txt = LanguageTool.PrepaireText(txt, ShortenStyle.Both, column.Prefix, column.Suffix, column.DoOpticalTranslation, column.OpticalReplace); }
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
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return false; }

        searchText = searchText.ToUpperInvariant();
        foreach (var thisColumnItem in db.Column) {
            {
                if (!thisColumnItem.IgnoreAtRowFilter) {
                    var txt = CellGetString(thisColumnItem);
                    txt = LanguageTool.PrepaireText(txt, ShortenStyle.Both, thisColumnItem.Prefix, thisColumnItem.Suffix, thisColumnItem.DoOpticalTranslation, thisColumnItem.OpticalReplace);
                    if (!string.IsNullOrEmpty(txt) && txt.ToUpperInvariant().Contains(searchText)) { return true; }
                }
            }
        }
        return false;
    }

    #endregion
}