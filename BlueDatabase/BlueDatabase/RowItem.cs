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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using BlueDatabase.Interfaces;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueBasics.Converter;

namespace BlueDatabase;

public sealed class RowItem : ICanBeEmpty, IDisposableExtended, IHasKeyName, IHasDatabase, IComparable {

    #region Fields

    public RowCheckedEventArgs? LastCheckedEventArgs;
    public string? LastCheckedMessage;
    public List<string>? LastCheckedRowFeedback;
    private DateTime? _isInCache;
    private string? _tmpQuickInfo;

    #endregion

    #region Constructors

    public RowItem(DatabaseAbstract database, string key) {
        Database = database;
        KeyName = key;
        _tmpQuickInfo = null;
        if (Database != null && !Database.IsDisposed) {
            Database.Cell.CellValueChanged += Cell_CellValueChanged;
            Database.DisposingEvent += Database_Disposing;
        }
    }

    #endregion

    #region Destructors

    // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    ~RowItem() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(false);
    }

    #endregion

    #region Events

    public event EventHandler<DoRowAutomaticEventArgs>? DoSpecialRules;

    public event EventHandler<RowCheckedEventArgs>? RowChecked;

    public event EventHandler<RowEventArgs>? RowGotData;

    #endregion

    #region Properties

    public DatabaseAbstract? Database { get; private set; }

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

    public static VariableCollection? CellToVariable(ColumnItem? column, RowItem? row) {
        if (column == null || row == null) { return null; }
        if (column.ScriptType is ScriptType.Nicht_vorhanden or ScriptType.undefiniert) { return null; }

        if (!column.Format.CanBeCheckedByRules()) { return null; }
        //if (!column.SaveContent) { return null; }

        #region ReadOnly bestimmen

        var ro = !column.Format.CanBeChangedByRules();
        //if (column == column.Database.Column.SysCorrect) { ro = true; }
        //if (column == column.Database.Column.SysRowChanger) { ro = true; }
        //if (column == column.Database.Column.SysRowChangeDate) { ro = true; }

        #endregion

        var wert = row.CellGetString(column);
        var qi = "Spalte: " + column.ReadableText();

        var vars = new VariableCollection();

        //switch (column.Format) {
        //    //case DataFormat.Verknüpfung_zu_anderer_Datenbank:
        //    //    //if (column.LinkedCell_RowKeyIsInColumn == -9999) {
        //    //    wert = string.Empty; // Beim Skript-Start ist dieser Wert immer leer, da die Verlinkung erst erstellt werden muss.
        //    //    //vars.GenerateAndAdd(new Variable(Column.KeyName + "_link", string.Empty, VariableDataType.String, true, true, "Dieser Wert kann nur mit SetLink verändert werden.\r\nBeim Skript-Start ist dieser Wert immer leer, da die Verlinkung erst erstellt werden muss."));
        //    //    //} else {
        //    //    //    qi = "Spalte: " + column.ReadableText() + "\r\nDer Inhalt wird zur Startzeit des Skripts festgelegt.";
        //    //    //    ro = true;
        //    //    //}
        //    //    break;

        //    //case DataFormat.Link_To_Filesystem:
        //    //    qi = "Spalte: " + column.ReadableText() + "\r\nFalls die Datei auf der Festplatte existiert, wird eine weitere\r\nVariable erzeugt: " + Column.KeyName + "_FileName";
        //    //    var f = column.Database.Cell.BestFile(column, row);
        //    //    if (f.FileType() == FileFormat.Image && IO.FileExists(f)) {
        //    //        vars.GenerateAndAdd(new VariableString(Column.KeyName + "_FileName", f, true, false, "Spalte: " + column.ReadableText() + "\r\nEnthält den vollen Dateinamen der Datei der zugehörigen Zelle.\r\nDie Existenz der Datei wurde geprüft und die Datei existert.\r\nAuf die Datei kann evtl. mit LoadImage zugegriffen werden."));
        //    //    }
        //    //    break;

        //    //case DataFormat.Columns_für_LinkedCellDropdown:
        //    //    if (IntTryParse(wert, out var colKey)) {
        //    //        var c = column.LinkedDatabase().Column.SearchByKey(colKey);
        //    //        if (c != null && !c.IsDisposed) { wert = c.Name; }
        //    //    }
        //    //    break;
        //}

        switch (column.ScriptType) {
            case ScriptType.Bool:
                vars.Add(new VariableBool(column.KeyName, wert == "+", ro, false, qi));
                break;

            case ScriptType.List:
                vars.Add(new VariableListString(column.KeyName, wert.SplitAndCutByCrToList(), ro, false, qi));
                break;

            case ScriptType.Numeral:
                _ = FloatTryParse(wert, out var f);
                vars.Add(new VariableFloat(column.KeyName, f, ro, false, qi));
                break;

            case ScriptType.String:
                vars.Add(new VariableString(column.KeyName, wert, ro, false, qi));
                break;

            //case ScriptType.DateTime:
            //    qi += "\r\nFalls die Zelle keinen gültiges Datum enthält, wird 01.01.0001 als Datum verwendet.";
            //    if (DateTimeTryParse(wert, out var d)) {
            //        vars.Add(new VariableDateTime(column.KeyName, d, ro, false, qi));
            //    } else {
            //        vars.Add(new VariableDateTime(column.KeyName, new DateTime(1, 1, 1), ro, false, qi));
            //    }
            //    break;

            case ScriptType.String_Readonly:
                vars.Add(new VariableString(column.KeyName, wert, true, false, qi));
                break;

            case ScriptType.Bool_Readonly:
                vars.Add(new VariableBool(column.KeyName, wert == "+", true, false, qi));
                break;

            default:
                Develop.DebugPrint(column.ScriptType);
                break;
        }

        return vars;
    }

    public string CellFirstString() => Database?.Column.First() is not ColumnItem fc ? string.Empty : CellGetString(fc);

    public bool CellGetBoolean(string columnName) => Database?.Cell.GetBoolean(Database?.Column[columnName], this) ?? default;

    public bool CellGetBoolean(ColumnItem? column) => Database?.Cell.GetBoolean(column, this) ?? default;

    public Color CellGetColor(string columnName) => Database?.Cell.GetColor(Database?.Column[columnName], this) ?? default;

    public Color CellGetColor(ColumnItem? column) => Database?.Cell.GetColor(column, this) ?? default;

    public int CellGetColorBgr(ColumnItem? column) => Database?.Cell.GetColorBgr(column, this) ?? 0;

    /// <summary>
    ///
    /// </summary>
    /// <param name="columnName"></param>
    /// <returns>DateTime.MinValue bei Fehlern</returns>
    public DateTime CellGetDateTime(string columnName) => Database?.Cell.GetDateTime(Database?.Column[columnName], this) ?? DateTime.MinValue;

    /// <summary>
    ///
    /// </summary>
    /// <param name="column"></param>
    /// <returns>DateTime.MinValue bei Fehlern</returns>
    public DateTime CellGetDateTime(ColumnItem? column) => Database?.Cell.GetDateTime(column, this) ?? DateTime.MinValue;

    public double CellGetDouble(string columnName) => Database?.Cell.GetDouble(Database?.Column[columnName], this) ?? default;

    public double CellGetDouble(ColumnItem column) => Database?.Cell.GetDouble(column, this) ?? default;

    public int CellGetInteger(string columnName) => Database?.Cell.GetInteger(Database?.Column[columnName], this) ?? default;

    public int CellGetInteger(ColumnItem column) => Database?.Cell.GetInteger(column, this) ?? default;

    public List<string> CellGetList(string columnName) => Database?.Cell.GetList(Database?.Column[columnName], this) ?? [];

    public List<string> CellGetList(ColumnItem column) => Database?.Cell.GetList(column, this) ?? [];

    public Point CellGetPoint(string columnName) => Database?.Cell.GetPoint(Database?.Column[columnName], this) ?? Point.Empty;

    public Point CellGetPoint(ColumnItem column) => Database?.Cell.GetPoint(column, this) ?? Point.Empty;

    public string CellGetString(string columnName) => Database?.Cell.GetString(Database?.Column[columnName], this) ?? string.Empty;

    public string CellGetString(ColumnItem column) {
        if (Database is not DatabaseAbstract db || db.IsDisposed || column.IsDisposed) { return string.Empty; }
        return Database.Cell.GetString(column, this);
    }

    public IEnumerable<string> CellGetValuesReadable(ColumnItem column, ShortenStyle style) => Database?.Cell.ValuesReadable(column, this, style) ?? [];

    public bool CellIsNullOrEmpty(string columnName) => Database?.Cell.IsNullOrEmpty(Database?.Column.Exists(columnName), this) ?? default;

    public bool CellIsNullOrEmpty(ColumnItem? column) => Database?.Cell.IsNullOrEmpty(column, this) ?? default;

    public void CellSet(string columnName, bool value) => Database?.Cell.Set(Database?.Column[columnName], this, value);

    public void CellSet(ColumnItem column, bool value) => Database?.Cell.Set(column, this, value);

    public void CellSet(string columnName, string value) => Database?.Cell.Set(Database?.Column[columnName], this, value);

    public void CellSet(ColumnItem? column, string value) => Database?.Cell.Set(column, this, value);

    public void CellSet(string columnName, double value) => Database?.Cell.Set(Database?.Column[columnName], this, value);

    public void CellSet(ColumnItem column, double value) => Database?.Cell.Set(column, this, value);

    public void CellSet(string columnName, int value) => Database?.Cell.Set(Database?.Column[columnName], this, value);

    public void CellSet(ColumnItem column, int value) => Database?.Cell.Set(column, this, value);

    public void CellSet(string columnName, Point value) => Database?.Cell.Set(Database?.Column[columnName], this, value);

    public void CellSet(ColumnItem column, Point value) => Database?.Cell.Set(column, this, value);

    public void CellSet(string columnName, List<string>? value) => Database?.Cell.Set(Database?.Column[columnName], this, value);

    public void CellSet(ColumnItem column, List<string>? value) => Database?.Cell.Set(column, this, value);

    public void CellSet(string columnName, DateTime value) => Database?.Cell.Set(Database?.Column[columnName], this, value);

    public void CellSet(ColumnItem column, DateTime value) => Database?.Cell.Set(column, this, value);

    public void CheckRowDataIfNeeded() {
        if (Database is not DatabaseAbstract db || db.IsDisposed) {
            LastCheckedMessage = "Datenbank verworfen";
            return;
        }

        if (!db.IsRowScriptPossible(true)) {
            LastCheckedMessage = "Zeilenprüfung deaktiviert";
            return;
        }

        if (LastCheckedEventArgs != null) { return; }

        //_ = Database.RefreshRowData(this, false);

        //if (IsInCache == null) { Develop.DebugPrint(FehlerArt.Fehler, "Refresh-Fehler"); }

        var sef = ExecuteScript(ScriptEventTypes.prepare_formula, string.Empty, false, false, true, 0);

        LastCheckedMessage = "<b><u>" + CellFirstString() + "</b></u><br><br>";

        List<string> cols = [];

        var tmp = LastCheckedRowFeedback;
        if (tmp != null && tmp.Count > 0) {
            foreach (var thiss in tmp) {
                _ = cols.AddIfNotExists(thiss);
                var t = thiss.SplitBy("|");
                var thisc = db?.Column.Exists(t[0]);
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
                CellSet(db.Column.SysCorrect, cols.Count == 0);
            }
        }

        LastCheckedEventArgs = new RowCheckedEventArgs(this, cols, sef.Variables);

        OnRowChecked(LastCheckedEventArgs);
    }

    public void CloneFrom(RowItem source, bool nameAndKeyToo) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }

        var sdb = source.Database;
        if (sdb is null || sdb.IsDisposed) { return; }

        if (nameAndKeyToo) { KeyName = source.KeyName; }

        foreach (var thisColumn in db.Column) {
            var value = sdb.Cell.GetStringCore(sdb.Column[thisColumn.KeyName], source);

            _ = db.ChangeData(DatabaseDataType.Value_withoutSizeData, thisColumn, source, string.Empty, value, Generic.UserName, DateTime.UtcNow, string.Empty);

            //Database.Cell.SetValueBehindLinkedValue(thisColumn, this, sdb.Cell.GetStringBehindLinkedValue(sdb.Column[thisColumn.KeyName], source), false);
        }
    }

    public string CompareKey(List<ColumnItem>? columns) {
        StringBuilder r = new();
        if (columns != null) {
            foreach (var t in columns) {
                if (t.LinkedDatabase is not DatabaseAbstract db || db.IsDisposed) {
                    // LinkedDatabase = null - Ansonsten wird beim Sortieren alles immer wieder geladen,
                    _ = r.Append(CellGetCompareKey(t) + Constants.FirstSortChar);
                }
            }
        }
        _ = r.Append(Constants.SecondSortChar + KeyName);
        return r.ToString();
    }

    public string CompareKey() => CompareKey(Database?.SortDefinition?.Columns);

    public int CompareTo(object obj) {
        if (obj is RowItem tobj) {
            return string.Compare(CompareKey(), tobj.CompareKey(), StringComparison.OrdinalIgnoreCase);
        }

        Develop.DebugPrint(FehlerArt.Fehler, "Falscher Objecttyp!");
        return 0;
    }

    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
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
    /// <param name="changevalues"></param>
    /// <param name="tryforsceonds"></param>
    /// <param name="eventname"></param>
    /// <returns>checkPerformed  = ob das Skript gestartet werden konnte und beendet wurde, error = warum das fehlgeschlagen ist, script dort sind die Skriptfehler gespeichert</returns>
    public ScriptEndedFeedback ExecuteScript(ScriptEventTypes? eventname, string scriptname, bool doFemdZelleInvalidate, bool fullCheck, bool changevalues, float tryforsceonds) {
        var m = DatabaseAbstract.EditableErrorReason(Database, EditableErrorReasonType.EditAcut);
        if (!string.IsNullOrEmpty(m) || Database is null) { return new ScriptEndedFeedback("Automatische Prozesse nicht möglich: " + m, false, false, "Allgemein"); }

        var t = DateTime.UtcNow;
        do {
            var erg = ExecuteScript(eventname, scriptname, doFemdZelleInvalidate, fullCheck, changevalues);
            if (erg.AllOk) { return erg; }
            if (!erg.GiveItAnotherTry || DateTime.UtcNow.Subtract(t).TotalSeconds > tryforsceonds) { return erg; }
        } while (true);
    }

    public void InvalidateCheckData() {
        LastCheckedRowFeedback = null;
        LastCheckedEventArgs = null;
        LastCheckedMessage = null;
    }

    public bool IsNullOrEmpty() {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return true; }
        return db.Column.All(thisColumnItem => thisColumnItem != null && CellIsNullOrEmpty(thisColumnItem));
    }

    public bool IsNullOrEmpty(ColumnItem? column) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return true; }
        return db.Cell.IsNullOrEmpty(column, this);
    }

    public bool MatchesTo(FilterItem fi) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return false; }

        if (fi.IsDisposed) { return true; }

        if (fi.Database != Database) { return false; }

        if (fi.FilterType == FilterType.AlwaysFalse) { return false; }

        fi.Column?.RefreshColumnsData();

        if (fi.FilterType is FilterType.KeinFilter or FilterType.GroßKleinEgal) { return true; } // Filter ohne Funktion
        if (fi.Column == null) {
            if (!fi.FilterType.HasFlag(FilterType.GroßKleinEgal)) { fi.FilterType |= FilterType.GroßKleinEgal; }
            if (fi.FilterType is not FilterType.Instr_GroßKleinEgal and not FilterType.Instr_UND_GroßKleinEgal) { Develop.DebugPrint(FehlerArt.Fehler, "Zeilenfilter nur mit Instr möglich!"); }
            if (fi.SearchValue.Count < 1) { Develop.DebugPrint(FehlerArt.Fehler, "Zeilenfilter nur mit mindestens einem Wert möglich"); }

            return fi.SearchValue.All(RowFilterMatch);
        }

        if (!Database.Cell.MatchesTo(fi.Column, this, fi)) { return false; }

        return true;
    }

    public bool MatchesTo(List<FilterItem>? fi) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return false; }
        if (fi == null || fi.Count == 0) { return true; }

        Database.RefreshColumnsData(fi);

        var ok = true;

        _ = Parallel.ForEach(fi, (thisFilter, state) => {
            if (!MatchesTo(thisFilter)) {
                ok = false;
                state.Break();
            }
        });

        return ok;

        //foreach (var thisFilter in filter) {
        //    if (!MatchesTo(thisFilter)) { return false; }
        //}
        //return true;
    }

    public bool NeedsUpdate() => Database?.Column.SysRowState is ColumnItem srs &&
                                     CellGetString(srs) != Database.EventScriptVersion &&
                                     Database.IsRowScriptPossible(true);

    /// <summary>
    ///
    /// </summary>
    /// <param name="txt"></param>
    /// <param name="replacedvalue">Wenn true, wird der Wert durch den leserlichen Zelleninhalt ersetzt. Bei False durch den Origial Zelleninhalt</param>
    /// <param name="removeLineBreaks"></param>
    /// <returns></returns>
    public string ReplaceVariables(string txt, bool replacedvalue, bool removeLineBreaks) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return txt; }

        var erg = txt;
        // Variablen ersetzen
        foreach (var thisColumnItem in db.Column) {
            if (!erg.Contains("~")) { return erg; }

            if (thisColumnItem != null) {
                if (erg.ToUpper().Contains("~" + thisColumnItem.KeyName.ToUpper())) {
                    var replacewith = CellGetString(thisColumnItem);
                    if (replacedvalue) { replacewith = CellItem.ValueReadable(thisColumnItem, replacewith, ShortenStyle.Replaced, BildTextVerhalten.Nur_Text, removeLineBreaks); }
                    if (removeLineBreaks && !replacedvalue) {
                        replacewith = replacewith.Replace("\r\n", " ");
                        replacewith = replacewith.Replace("\r", " ");
                    }

                    erg = erg.Replace("~" + thisColumnItem.KeyName.ToUpper() + "~", replacewith, RegexOptions.IgnoreCase);
                    //while (erg.ToUpper().Contains("~" + thisColumnItem.Name.ToUpper() + "(")) {
                    //    var x = erg.IndexOf("~" + thisColumnItem.Name.ToUpper() + "(", StringComparison.OrdinalIgnoreCase);
                    //    var x2 = erg.IndexOf(")", x, StringComparison.Ordinal);
                    //    if (x2 < x) { return erg; }
                    //    var ww = erg.Substring(x + thisColumnItem.Name.Length + 2, x2 - x - thisColumnItem.Name.Length - 2);
                    //    ww = ww.Replace(" ", string.Empty).ToUpper();
                    //    var vals = ww.SplitAndCutBy(",");
                    //    if (vals.Length != 2) { return txt; }
                    //    if (vals[0] != "L") { return txt; }
                    //    if (!IntTryParse(vals[1], out var stellen)) { return txt; }
                    //    var newW = replacewith.Substring(0, Math.Min(stellen, replacewith.Length));
                    //    erg = erg.Replace(erg.Substring(x, x2 - x + 1), newW);
                    //}
                }
            }
        }
        return erg;
    }

    public string RowStamp() {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return string.Empty; }

        var erg = string.Empty;
        foreach (var thisColumn in db.Column) {
            if (thisColumn != null && !thisColumn.IsDisposed) {
                if (thisColumn.ScriptType is ScriptType.Nicht_vorhanden or ScriptType.undefiniert) { continue; }

                erg += CellGetString(thisColumn) + "|";
            }
        }
        return erg;
    }

    public void VariableToCell(ColumnItem? column, VariableCollection vars) {
        var m = DatabaseAbstract.EditableErrorReason(Database, EditableErrorReasonType.EditAcut);
        if (!string.IsNullOrEmpty(m) || Database is not DatabaseAbstract db || db.IsDisposed || column == null) { return; }

        var columnVar = vars.Get(column.KeyName);
        if (columnVar == null || columnVar.ReadOnly) { return; }
        if (!column.Format.CanBeChangedByRules()) { return; }

        //if (column.Format == DataFormat.Verknüpfung_zu_anderer_Datenbank) {
        //    var columnLinkVar = vars.GetSystem(Column.KeyName + "_Link");
        //    if (columnLinkVar != null) {
        //        column.Database.Cell.SetValueBehindLinkedValue(column, this, columnLinkVar.ValueString);
        //    }
        //}

        if (columnVar is VariableFloat vf) {
            CellSet(column, vf.ValueNum);
            return;
        }

        if (columnVar is VariableListString vl) {
            CellSet(column, vl.ValueList);
            return;
        }

        if (columnVar is VariableBool vb) {
            CellSet(column, vb.ValueBool);
            return;
        }
        if (columnVar is VariableString vs) {
            CellSet(column, vs.ValueString);
            return;
        }
        //if (columnVar is VariableDateTime vd) {
        //    var x = vd.ValueDate.ToString(Constants.Format_Date9, CultureInfo.InvariantCulture);
        //    x = x.TrimEnd(".000");
        //    x = x.TrimEnd(".0");
        //    x = x.TrimEnd("00:00:00");
        //    x = x.TrimEnd(" ");
        //    CellSet(column, x);
        //    return;
        //}

        Develop.DebugPrint("Typ nicht erkannt: " + columnVar.MyClassId);
    }

    internal bool AmIChanger() {
        if (IsDisposed) { return false; }
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return false; }

        return db.Column.SysRowChanger is ColumnItem src && CellGetString(src).Equals(Generic.UserName, StringComparison.OrdinalIgnoreCase);
    }

    internal double RowChangedXMinutesAgo() {
        if (IsDisposed) { return -1; }
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return -1; }

        if (db.Column.SysRowChangeDate is not ColumnItem src) { return -1; }

        var v = CellGetDateTime(src);
        if (v == DateTime.MinValue) { return -1; }

        return DateTime.UtcNow.Subtract(v).TotalMinutes;
    }

    private void Cell_CellValueChanged(object sender, CellEventArgs e) {
        if (e.Row != this) { return; }
        _tmpQuickInfo = null;
    }

    private string CellGetCompareKey(ColumnItem column) => Database?.Cell.CompareKey(column, this) ?? string.Empty;

    private void Database_Disposing(object sender, System.EventArgs e) => Dispose();

    private void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
            }
            // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
            // TODO: Große Felder auf NULL setzen

            if (Database != null && !Database.IsDisposed) {
                Database.Cell.CellValueChanged -= Cell_CellValueChanged;
                Database.DisposingEvent -= Database_Disposing;
            }
            Database = null;
            _tmpQuickInfo = null;
            IsDisposed = true;
        }
    }

    private ScriptEndedFeedback ExecuteScript(ScriptEventTypes? eventname, string scriptname, bool doFemdZelleInvalidate, bool fullCheck, bool changevalues) {
        var m = DatabaseAbstract.EditableErrorReason(Database, EditableErrorReasonType.EditAcut);
        if (!string.IsNullOrEmpty(m) || Database is not DatabaseAbstract db) { return new ScriptEndedFeedback("Automatische Prozesse nicht möglich: " + m, false, false, "Allgemein"); }

        var feh = db.EditableErrorReason(EditableErrorReasonType.EditAcut);
        if (!string.IsNullOrEmpty(feh)) { return new ScriptEndedFeedback(feh, true, false, "Allgemein"); }

        // Zuerst die Aktionen ausführen und falls es einen Fehler gibt, die Spalten und Fehler auch ermitteln
        var script = db.ExecuteScript(eventname, scriptname, changevalues, this, null);

        if (!script.AllOk) {
            db.OnScriptError(new RowScriptCancelEventArgs(this, script.ProtocolText, script.ScriptHasSystaxError));
            if (script.ScriptHasSystaxError) {
                db.EventScriptErrorMessage = "Zeile: " + CellFirstString() + "\r\n\r\n" + script.ProtocolText;
            }

            return script;// (true, "<b>Das Skript ist fehlerhaft:</b>\r\n" + "Zeile: " + script.Line + "\r\n" + script.Error + "\r\n" + script.ErrorCode, script);
        }

        if (changevalues && db.Column.SysRowState is ColumnItem srs) {
            // Gucken, ob noch ein Fehler da ist, der von einer besonderen anderen Routine kommt. Beispiel Bildzeichen-Liste: Bandart und Einläufe
            DoRowAutomaticEventArgs e = new(this);
            OnDoSpecialRules(e);

            if (eventname is ScriptEventTypes.value_changed) {
                CellSet(srs, db.EventScriptVersion); // Nicht System set, diese Änderung muss geloggt werden
            } else {
                var l = db.EventScript.Get(ScriptEventTypes.value_changed);
                if (l.Count == 1 && l[0].KeyName == scriptname) {
                    CellSet(srs, db.EventScriptVersion); // Nicht System set, diese Änderung muss geloggt werden
                }
            }
        }

        if (!changevalues) { return new ScriptEndedFeedback(string.Empty, false, false, "Allgemein"); }

        // checkPerformed geht von Dateisystemfehlern aus

        // Dann die abschließenden Korrekturen vornehmen
        foreach (var thisColum in db.Column) {
            if (thisColum != null) {
                if (fullCheck) {
                    var x = CellGetString(thisColum);
                    var x2 = thisColum.AutoCorrect(x, true);
                    if (thisColum.Format is not DataFormat.Verknüpfung_zu_anderer_Datenbank && x != x2) {
                        db.Cell.Set(thisColum, this, x2);
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
        if (Database is not DatabaseAbstract db || db.IsDisposed ||
            string.IsNullOrEmpty(Database.ZeilenQuickInfo)) {
            _tmpQuickInfo = string.Empty;
            return;
        }

        _tmpQuickInfo = ReplaceVariables(Database.ZeilenQuickInfo, true, false);
    }

    private void OnDoSpecialRules(DoRowAutomaticEventArgs e) => DoSpecialRules?.Invoke(this, e);

    private void OnRowChecked(RowCheckedEventArgs e) => RowChecked?.Invoke(this, e);

    private void OnRowGotData(RowEventArgs e) => RowGotData?.Invoke(this, e);

    private bool RowFilterMatch(string searchText) {
        if (string.IsNullOrEmpty(searchText)) { return true; }
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return false; }

        searchText = searchText.ToUpper();
        foreach (var thisColumnItem in db.Column) {
            {
                if (!thisColumnItem.IgnoreAtRowFilter) {
                    var @string = CellGetString(thisColumnItem);
                    @string = LanguageTool.ColumnReplace(@string, thisColumnItem, ShortenStyle.Both);
                    if (!string.IsNullOrEmpty(@string) && @string.ToUpper().Contains(searchText)) { return true; }
                }
            }
        }
        return false;
    }

    #endregion
}