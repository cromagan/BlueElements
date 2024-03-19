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
using BlueScript.Structures;
using BlueScript.Variables;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static BlueBasics.Converter;

namespace BlueDatabase;

public sealed class RowItem : ICanBeEmpty, IDisposableExtended, IHasKeyName, IHasDatabase, IComparable {

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

    public event EventHandler<DoRowAutomaticEventArgs>? DoSpecialRules;

    public event EventHandler<RowCheckedEventArgs>? RowChecked;

    public event EventHandler<RowEventArgs>? RowGotData;

    #endregion

    #region Properties

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

    public static VariableCollection? CellToVariable(ColumnItem? column, RowItem? row, bool mustbeReadOnly) {
        if (column == null || row == null) { return null; }
        if (column.ScriptType is ScriptType.Nicht_vorhanden or ScriptType.undefiniert) { return null; }

        if (!column.Function.CanBeCheckedByRules()) { return null; }
        //if (!column.SaveContent) { return null; }

        #region ReadOnly bestimmen

        var ro = mustbeReadOnly || !column.Function.CanBeChangedByRules();
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
                vars.Add(new VariableBool(column.KeyName, wert == "+", ro, qi));
                break;

            case ScriptType.List:
                vars.Add(new VariableListString(column.KeyName, wert.SplitAndCutByCrToList(), ro, qi));
                break;

            case ScriptType.Numeral:
                _ = FloatTryParse(wert, out var f);
                vars.Add(new VariableFloat(column.KeyName, f, ro, qi));
                break;

            case ScriptType.String:
                vars.Add(new VariableString(column.KeyName, wert, ro, qi));
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
                vars.Add(new VariableString(column.KeyName, wert, true, qi));
                break;

            case ScriptType.Bool_Readonly:
                vars.Add(new VariableBool(column.KeyName, wert == "+", true, qi));
                break;

            default:
                Develop.DebugPrint(column.ScriptType);
                break;
        }

        return vars;
    }

    public string CellFirstString() => Database?.Column.First() is not ColumnItem fc ? string.Empty : CellGetString(fc);

    public bool CellGetBoolean(string columnName) => CellGetBoolean(Database?.Column[columnName]);

    public bool CellGetBoolean(ColumnItem? column) => Database?.Cell.GetString(column, this).FromPlusMinus() ?? default;// Main Method

    public Color CellGetColor(string columnName) => CellGetColor(Database?.Column[columnName]);

    public Color CellGetColor(ColumnItem? column) => Color.FromArgb(CellGetInteger(column)); // Main Method

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

    public List<string> CellGetList(ColumnItem? column) => Database?.Cell.GetString(column, this).SplitAndCutByCrToList() ?? [];// Main Method

    public List<string> CellGetList(string columnName) => CellGetList(Database?.Column[columnName]);

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

    public IEnumerable<string> CellGetValuesReadable(ColumnItem column, ShortenStyle style) => Database?.Cell.ValuesReadable(column, this, style) ?? [];

    public bool CellIsNullOrEmpty(string columnName) => Database?.Cell.IsNullOrEmpty(Database?.Column[columnName], this) ?? default;

    public bool CellIsNullOrEmpty(ColumnItem? column) => Database?.Cell.IsNullOrEmpty(column, this) ?? default;

    public void CellSet(string columnName, bool value, string comment) => Database?.Cell.Set(Database?.Column[columnName], this, value.ToPlusMinus(), comment);

    public void CellSet(ColumnItem column, bool value, string comment) => Database?.Cell.Set(column, this, value.ToPlusMinus(), comment);

    public void CellSet(string columnName, string value, string comment) => Database?.Cell.Set(Database?.Column[columnName], this, value, comment);

    public void CellSet(ColumnItem? column, string value, string comment) => Database?.Cell.Set(column, this, value, comment);

    public void CellSet(string columnName, double value, string comment) => Database?.Cell.Set(Database?.Column[columnName], this, value.ToString(Constants.Format_Float1, CultureInfo.InvariantCulture), comment);

    public void CellSet(ColumnItem column, double value, string comment) => Database?.Cell.Set(column, this, value.ToString(Constants.Format_Float1, CultureInfo.InvariantCulture), comment);

    public void CellSet(string columnName, int value, string comment) => Database?.Cell.Set(Database?.Column[columnName], this, value.ToString(), comment);

    public void CellSet(ColumnItem column, int value, string comment) => Database?.Cell.Set(column, this, value.ToString(), comment);

    public void CellSet(string columnName, Point value, string comment) => Database?.Cell.Set(Database?.Column[columnName], this, value.ToString(), comment);

    public void CellSet(ColumnItem column, Point value, string comment) => Database?.Cell.Set(column, this, value.ToString(), comment);

    public void CellSet(string columnName, List<string>? value, string comment) => Database?.Cell.Set(Database?.Column[columnName], this, value.JoinWithCr(), comment);

    public void CellSet(ColumnItem column, List<string>? value, string comment) => Database?.Cell.Set(column, this, value.JoinWithCr(), comment);

    public void CellSet(string columnName, DateTime value, string comment) => Database?.Cell.Set(Database?.Column[columnName], this, value.ToString(Constants.Format_Date5, CultureInfo.InvariantCulture), comment);

    public void CellSet(ColumnItem column, DateTime value, string comment) => Database?.Cell.Set(column, this, value.ToString(Constants.Format_Date5, CultureInfo.InvariantCulture), comment);

    public void CheckRowDataIfNeeded() {
        if (IsDisposed || Database is not Database db || db.IsDisposed) {
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

        var sef = ExecuteScript(ScriptEventTypes.prepare_formula, string.Empty, false, false, true, 0, null);

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

        var ColsToRefresh = new List<ColumnItem>();
        if (db.SortDefinition?.Columns is List<ColumnItem> lc) { ColsToRefresh.AddRange(lc); }
        if (db.Column.SysChapter is ColumnItem csc) { _ = ColsToRefresh.AddIfNotExists(csc); }
        if (db.Column.First() is ColumnItem cf) { _ = ColsToRefresh.AddIfNotExists(cf); }

        db.RefreshColumnsData(ColsToRefresh);

        return CompareKey(ColsToRefresh);
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
    /// <param name="changevalues"></param>
    /// <param name="tryforsceonds"></param>
    /// <param name="eventname"></param>
    /// <returns>checkPerformed  = ob das Skript gestartet werden konnte und beendet wurde, error = warum das fehlgeschlagen ist, script dort sind die Skriptfehler gespeichert</returns>
    public ScriptEndedFeedback ExecuteScript(ScriptEventTypes? eventname, string scriptname, bool doFemdZelleInvalidate, bool fullCheck, bool changevalues, float tryforsceonds, List<string>? attributes) {
        var m = Database.EditableErrorReason(Database, EditableErrorReasonType.EditAcut);
        if (!string.IsNullOrEmpty(m) || Database is null) { return new ScriptEndedFeedback("Automatische Prozesse nicht möglich: " + m, false, false, "Allgemein"); }

        var t = DateTime.UtcNow;
        do {
            var erg = ExecuteScript(eventname, scriptname, doFemdZelleInvalidate, fullCheck, changevalues, attributes);
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

        if (!Database.Cell.MatchesTo(fi.Column, this, fi)) { return false; }

        return true;
    }

    public bool MatchesTo(List<FilterItem>? fi) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return false; }
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
                                     Database.HasValueChangedScript();

    /// <summary>
    ///
    /// </summary>
    /// <param name="txt"></param>
    /// <param name="replacedvalue">Wenn true, wird der Wert durch den leserlichen Zelleninhalt ersetzt. Bei False durch den Origial Zelleninhalt</param>
    /// <param name="removeLineBreaks"></param>
    /// <returns></returns>
    public string ReplaceVariables(string txt, bool replacedvalue, bool removeLineBreaks) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return txt; }

        var erg = txt;
        // Variablen ersetzen
        foreach (var column in db.Column) {
            if (!erg.Contains("~")) { return erg; }

            if (column != null) {
                if (erg.ToUpper().Contains("~" + column.KeyName.ToUpper())) {
                    var replacewith = CellGetString(column);
                    if (replacedvalue) { replacewith = CellItem.ValueReadable(replacewith, ShortenStyle.Replaced, BildTextVerhalten.Nur_Text, removeLineBreaks, column.Prefix, column.Suffix, column.DoOpticalTranslation, column.OpticalReplace); }
                    if (removeLineBreaks && !replacedvalue) {
                        replacewith = replacewith.Replace("\r\n", " ");
                        replacewith = replacewith.Replace("\r", " ");
                    }

                    erg = erg.Replace("~" + column.KeyName.ToUpper() + "~", replacewith, RegexOptions.IgnoreCase);
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

            default:
                Develop.DebugPrint("Typ nicht erkannt: " + columnVar.MyClassId);
                break;
        }
    }

    internal bool AmIChanger() {
        if (IsDisposed) { return false; }
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return false; }

        return db.Column.SysRowChanger is ColumnItem src && CellGetString(src).Equals(Generic.UserName, StringComparison.OrdinalIgnoreCase);
    }

    internal double RowChangedXMinutesAgo() {
        if (IsDisposed) { return -1; }
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return -1; }

        if (db.Column.SysRowChangeDate is not ColumnItem src) { return -1; }

        var v = CellGetDateTime(src);
        if (v == DateTime.MinValue) { return -1; }

        return DateTime.UtcNow.Subtract(v).TotalMinutes;
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
            LastCheckedEventArgs = null;
            LastCheckedMessage = null;
            LastCheckedRowFeedback?.Clear();
            LastCheckedRowFeedback = null;

            IsDisposed = true;
        }
    }

    private ScriptEndedFeedback ExecuteScript(ScriptEventTypes? eventname, string scriptname, bool doFemdZelleInvalidate, bool fullCheck, bool changevalues, List<string>? attributes) {
        var m = Database.EditableErrorReason(Database, EditableErrorReasonType.EditAcut);
        if (!string.IsNullOrEmpty(m) || Database is not Database db || db.IsDisposed) { return new ScriptEndedFeedback("Automatische Prozesse nicht möglich: " + m, false, false, "Allgemein"); }

        var feh = db.EditableErrorReason(EditableErrorReasonType.EditAcut);
        if (!string.IsNullOrEmpty(feh)) { return new ScriptEndedFeedback(feh, true, false, "Allgemein"); }

        // Zuerst die Aktionen ausführen und falls es einen Fehler gibt, die Spalten und Fehler auch ermitteln
        var script = db.ExecuteScript(eventname, scriptname, changevalues, this, attributes);

        if (!script.AllOk) {
            //db.OnScriptError(new RowScriptCancelEventArgs(this, script.ProtocolText, script.ScriptHasSystaxError));
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
                CellSet(srs, db.EventScriptVersion, "NACH Skript 'value_changed'"); // Nicht System set, diese Änderung muss geloggt werden
            } else {
                var l = db.EventScript.Get(ScriptEventTypes.value_changed);
                if (l.Count == 1 && l[0].KeyName == scriptname) {
                    CellSet(srs, db.EventScriptVersion, "NACH Skript 'value_changed' (" + scriptname + ")"); // Nicht System set, diese Änderung muss geloggt werden
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
                    if (thisColum.Function is not ColumnFunction.Verknüpfung_zu_anderer_Datenbank && x != x2) {
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

        _tmpQuickInfo = ReplaceVariables(Database.ZeilenQuickInfo, true, false);
    }

    private void OnDoSpecialRules(DoRowAutomaticEventArgs e) => DoSpecialRules?.Invoke(this, e);

    private void OnRowChecked(RowCheckedEventArgs e) => RowChecked?.Invoke(this, e);

    private void OnRowGotData(RowEventArgs e) => RowGotData?.Invoke(this, e);

    private bool RowFilterMatch(string searchText) {
        if (string.IsNullOrEmpty(searchText)) { return true; }
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return false; }

        searchText = searchText.ToUpper();
        foreach (var thisColumnItem in db.Column) {
            {
                if (!thisColumnItem.IgnoreAtRowFilter) {
                    var txt = CellGetString(thisColumnItem);
                    txt = LanguageTool.PrepaireText(txt, ShortenStyle.Both, thisColumnItem.Prefix, thisColumnItem.Suffix, thisColumnItem.DoOpticalTranslation, thisColumnItem.OpticalReplace);
                    if (!string.IsNullOrEmpty(txt) && txt.ToUpper().Contains(searchText)) { return true; }
                }
            }
        }
        return false;
    }

    #endregion
}