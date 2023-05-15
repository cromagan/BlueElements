// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

    public DateTime? IsInCache = null;
    public RowCheckedEventArgs? LastCheckedEventArgs;
    public string? LastCheckedMessage;
    public List<string>? LastCheckedRowFeedback;
    private string? _tmpQuickInfo;

    #endregion

    #region Constructors

    public RowItem(DatabaseAbstract database, long key) {
        Database = database;
        Key = key;
        _tmpQuickInfo = null;
        if (Database != null && !Database.IsDisposed) {
            Database.Cell.CellValueChanged += Cell_CellValueChanged;
            Database.Disposing += Database_Disposing;
        }
    }

    public RowItem(DatabaseAbstract database) : this(database, database.Row.NextRowKey()) { }

    #endregion

    #region Destructors

    // TODO: Finalizer nur �berschreiben, wenn "Dispose(bool disposing)" Code f�r die Freigabe nicht verwalteter Ressourcen enth�lt
    ~RowItem() {
        // �ndern Sie diesen Code nicht. F�gen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(false);
    }

    #endregion

    #region Events

    public event EventHandler<DoRowAutomaticEventArgs>? DoSpecialRules;

    public event EventHandler<RowCheckedEventArgs>? RowChecked;

    #endregion

    #region Properties

    /// <summary>
    /// Sehr rudiment�re Angabe!
    /// </summary>
    public static bool DoingScript { get; private set; }

    public DatabaseAbstract? Database { get; private set; }
    public bool IsDisposed { get; private set; }
    public long Key { get; private set; }

    public string KeyName => Key.ToString();

    public string QuickInfo {
        get {
            if (_tmpQuickInfo != null) { return _tmpQuickInfo; }
            GenerateQuickInfo();
            return _tmpQuickInfo!;
        }
    }

    #endregion

    /// <summary>
    /// Diese Routine konvertiert den Inhalt der Zelle in eine vom Skript lesbaren Variable
    /// </summary>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <returns></returns>

    #region Methods

    public static VariableCollection CellToVariable(ColumnItem? column, RowItem? row) {
        if (column == null || row == null) { return null; }
        if (!column.Format.CanBeCheckedByRules()) { return null; }
        if (!column.SaveContent) { return null; }

        #region ReadOnly bestimmen

        var ro = !column.Format.CanBeChangedByRules();
        //if (column == column.Database.Column.SysCorrect) { ro = true; }
        //if (column == column.Database.Column.SysRowChanger) { ro = true; }
        //if (column == column.Database.Column.SysRowChangeDate) { ro = true; }

        #endregion

        //if (column.MultiLine && column.Format == DataFormat.Link_To_Filesystem) { return null; }
        if (column.ScriptType == ScriptType.Nicht_vorhanden) { return null; }

        var wert = row.CellGetString(column);
        var qi = "Spalte: " + column.ReadableText();

        var vars = new VariableCollection();

        //switch (column.Format) {
        //    //case DataFormat.Verkn�pfung_zu_anderer_Datenbank:
        //    //    //if (column.LinkedCell_RowKeyIsInColumn == -9999) {
        //    //    wert = string.Empty; // Beim Skript-Start ist dieser Wert immer leer, da die Verlinkung erst erstellt werden muss.
        //    //    //vars.GenerateAndAdd(new Variable(column.Name + "_link", string.Empty, VariableDataType.String, true, true, "Dieser Wert kann nur mit SetLink ver�ndert werden.\r\nBeim Skript-Start ist dieser Wert immer leer, da die Verlinkung erst erstellt werden muss."));
        //    //    //} else {
        //    //    //    qi = "Spalte: " + column.ReadableText() + "\r\nDer Inhalt wird zur Startzeit des Skripts festgelegt.";
        //    //    //    ro = true;
        //    //    //}
        //    //    break;

        //    //case DataFormat.Link_To_Filesystem:
        //    //    qi = "Spalte: " + column.ReadableText() + "\r\nFalls die Datei auf der Festplatte existiert, wird eine weitere\r\nVariable erzeugt: " + column.Name + "_FileName";
        //    //    var f = column.Database.Cell.BestFile(column, row);
        //    //    if (f.FileType() == FileFormat.Image && IO.FileExists(f)) {
        //    //        vars.GenerateAndAdd(new VariableString(column.Name + "_FileName", f, true, false, "Spalte: " + column.ReadableText() + "\r\nEnth�lt den vollen Dateinamen der Datei der zugeh�rigen Zelle.\r\nDie Existenz der Datei wurde gepr�ft und die Datei existert.\r\nAuf die Datei kann evtl. mit LoadImage zugegriffen werden."));
        //    //    }
        //    //    break;

        //    //case DataFormat.Columns_f�r_LinkedCellDropdown:
        //    //    if (IntTryParse(wert, out var colKey)) {
        //    //        var c = column.LinkedDatabase().Column.SearchByKey(colKey);
        //    //        if (c != null) { wert = c.Name; }
        //    //    }
        //    //    break;
        //}

        switch (column.ScriptType) {
            case ScriptType.Bool:
                vars.Add(new VariableBool(column.Name, wert == "+", ro, false, qi));
                break;

            case ScriptType.List:
                vars.Add(new VariableListString(column.Name, wert.SplitAndCutByCrToList(), ro, false, qi));
                break;

            case ScriptType.Numeral:
                _ = FloatTryParse(wert, out var f);
                vars.Add(new VariableFloat(column.Name, f, ro, false, qi));
                break;

            case ScriptType.String:
                vars.Add(new VariableString(column.Name, wert, ro, false, qi));
                break;

            case ScriptType.DateTime:
                qi += "\r\nFalls die Zelle keinen g�ltiges Datum enth�lt, wird 01.01.0001 als Datum verwendet.";
                if (DateTimeTryParse(wert, out var d)) {
                    vars.Add(new VariableDateTime(column.Name, d, ro, false, qi));
                } else {
                    vars.Add(new VariableDateTime(column.Name, new DateTime(1, 1, 1), ro, false, qi));
                }
                break;

            case ScriptType.String_Readonly:
                vars.Add(new VariableString(column.Name, wert, true, false, qi));
                break;

            case ScriptType.Bool_Readonly:
                vars.Add(new VariableBool(column.Name, wert == "+", true, false, qi));
                break;

            default:
                Develop.DebugPrint(column.ScriptType);
                break;
        }

        return vars;
    }

    public string CellFirstString() {
        var fc = Database?.Column.First();
        if (fc == null) { return string.Empty; }

        return CellGetString(fc);
    }

    public bool CellGetBoolean(string columnName) => Database?.Cell.GetBoolean(Database?.Column[columnName], this) ?? default;

    public bool CellGetBoolean(ColumnItem? column) => Database?.Cell.GetBoolean(column, this) ?? default;

    public Color CellGetColor(string columnName) => Database?.Cell.GetColor(Database?.Column[columnName], this) ?? default;

    public Color CellGetColor(ColumnItem? column) => Database?.Cell.GetColor(column, this) ?? default;

    public int CellGetColorBgr(ColumnItem? column) => Database?.Cell.GetColorBgr(column, this) ?? 0;

    public string CellGetCompareKey(ColumnItem column) => Database?.Cell.CompareKey(column, this) ?? string.Empty;

    public DateTime CellGetDateTime(string columnName) => Database?.Cell.GetDateTime(Database?.Column[columnName], this) ?? default;

    public DateTime CellGetDateTime(ColumnItem? column) => Database?.Cell.GetDateTime(column, this) ?? default;

    public double CellGetDouble(string columnName) => Database?.Cell.GetDouble(Database?.Column[columnName], this) ?? default;

    public double CellGetDouble(ColumnItem? column) => Database?.Cell.GetDouble(column, this) ?? default;

    public int CellGetInteger(string columnName) => Database?.Cell.GetInteger(Database?.Column[columnName], this) ?? default;

    public int CellGetInteger(ColumnItem? column) => Database?.Cell.GetInteger(column, this) ?? default;

    public List<string> CellGetList(string columnName) => Database?.Cell.GetList(Database?.Column[columnName], this) ?? new List<string>();

    public List<string> CellGetList(ColumnItem? column) => Database?.Cell.GetList(column, this) ?? new List<string>();

    public Point CellGetPoint(string columnName) => Database?.Cell.GetPoint(Database?.Column[columnName], this) ?? Point.Empty;

    public Point CellGetPoint(ColumnItem? column) => Database?.Cell.GetPoint(column, this) ?? Point.Empty;

    public string CellGetString(string columnName) => Database?.Cell.GetString(Database?.Column[columnName], this) ?? string.Empty;

    public string CellGetString(ColumnItem? column) {
        if (Database == null || Database.IsDisposed || column == null) { return string.Empty; }
        return Database.Cell.GetString(column, this);
    }

    public List<string> CellGetValuesReadable(ColumnItem? column, ShortenStyle style) => Database?.Cell.ValuesReadable(column, this, style) ?? new List<string>();

    public bool CellIsNullOrEmpty(string columnName) => Database?.Cell.IsNullOrEmpty(Database?.Column[columnName], this) ?? default;

    public bool CellIsNullOrEmpty(ColumnItem? column) => Database?.Cell.IsNullOrEmpty(column, this) ?? default;

    public void CellSet(string columnName, bool value) => Database?.Cell.Set(Database?.Column[columnName], this, value);

    public void CellSet(ColumnItem? column, bool value) => Database?.Cell.Set(column, this, value);

    public void CellSet(string columnName, string value) => Database?.Cell.Set(Database?.Column[columnName], this, value);

    public void CellSet(ColumnItem? column, string value) => Database?.Cell.Set(column, this, value);

    public void CellSet(string columnName, double value) => Database?.Cell.Set(Database?.Column[columnName], this, value);

    public void CellSet(ColumnItem? column, double value) => Database?.Cell.Set(column, this, value);

    public void CellSet(string columnName, int value) => Database?.Cell.Set(Database?.Column[columnName], this, value);

    public void CellSet(ColumnItem? column, int value) => Database?.Cell.Set(column, this, value);

    public void CellSet(string columnName, Point value) => Database?.Cell.Set(Database?.Column[columnName], this, value);

    public void CellSet(ColumnItem? column, Point value) => Database?.Cell.Set(column, this, value);

    public void CellSet(string columnName, List<string>? value) => Database?.Cell.Set(Database?.Column[columnName], this, value);

    public void CellSet(ColumnItem? column, List<string>? value) => Database?.Cell.Set(column, this, value);

    public void CellSet(string columnName, DateTime value) => Database?.Cell.Set(Database?.Column[columnName], this, value);

    public void CellSet(ColumnItem? column, DateTime value) => Database?.Cell.Set(column, this, value);

    public void CheckRowDataIfNeeded() {
        if (Database is null || Database.IsDisposed) {
            LastCheckedMessage = "Datenbank verworfen";
            return;
        }

        if (LastCheckedEventArgs != null) { return; }

        var sef = ExecuteScript(EventTypes.prepare_formula, string.Empty, false, false, true, 0);

        LastCheckedMessage = "<b><u>" + CellFirstString() + "</b></u><br><br>";

        List<string> cols = new();

        var tmp = LastCheckedRowFeedback;
        if (tmp != null && tmp.Count > 0) {
            foreach (var thiss in tmp) {
                _ = cols.AddIfNotExists(thiss);
                var t = thiss.SplitBy("|");
                var thisc = Database?.Column.Exists(t[0]);
                if (thisc != null) {
                    LastCheckedMessage = LastCheckedMessage + "<b>" + thisc.ReadableText() + ":</b> " + t[1] + "<br><hr><br>";
                }
            }
        }

        if (cols.Count == 0) {
            LastCheckedMessage += "Diese Zeile ist fehlerfrei.";
        }

        if (Database?.Column.SysCorrect != null && Database.Column.SysCorrect.SaveContent) {
            if (IsNullOrEmpty(Database.Column.SysCorrect) || cols.Count == 0 != CellGetBoolean(Database.Column.SysCorrect)) {
                CellSet(Database.Column.SysCorrect, cols.Count == 0);
            }
        }

        LastCheckedEventArgs = new RowCheckedEventArgs(this, cols, sef.Variables);

        OnRowChecked(LastCheckedEventArgs);
    }

    public void CloneFrom(RowItem source, bool nameAndKeyToo) {
        if (Database is null || Database.IsDisposed) { return; }

        var sdb = source.Database;
        if (sdb is null || sdb.IsDisposed) { return; }

        if (nameAndKeyToo) { Key = source.Key; }

        foreach (var thisColumn in Database.Column) {
            var value = sdb.Cell.GetStringBehindLinkedValue(sdb.Column[thisColumn.Name], source);

            _ = Database.ChangeData(DatabaseDataType.Value_withoutSizeData, thisColumn, source, string.Empty, value, string.Empty);

            //Database.Cell.SetValueBehindLinkedValue(thisColumn, this, sdb.Cell.GetStringBehindLinkedValue(sdb.Column[thisColumn.Name], source), false);
        }
    }

    public string CompareKey(List<ColumnItem>? columns) {
        StringBuilder r = new();
        if (columns != null) {
            foreach (var t in columns) {
                if (t.LinkedDatabase == null) {
                    // LinkedDatabase = null - Ansonsten wird beim Sortieren alles immer wieder geladen,
                    _ = r.Append(CellGetCompareKey(t) + Constants.FirstSortChar);
                }
            }
        }
        _ = r.Append(Constants.SecondSortChar + Key);
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
        // �ndern Sie diesen Code nicht. F�gen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// F�hrt Regeln aus, l�st Ereignisses, setzt SysCorrect und auch die initalwerte der Zellen.
    /// Z.b: Runden, Gro�schreibung wird nur bei einem FullCheck korrigiert, das wird normalerweise vor dem Setzen bei CellSet bereits korrigiert.
    /// </summary>
    /// <param name="scriptname"></param>
    /// <param name="doFemdZelleInvalidate">bei verlinkten Zellen wird der verlinkung gepr�ft und erneuert.</param>
    /// <param name="fullCheck">Runden, Gro�schreibung, etc. wird ebenfalls durchgefphrt</param>
    /// <param name="changevalues"></param>
    /// <param name="tryforsceonds"></param>
    /// <param name="eventname"></param>
    /// <returns>checkPerformed  = ob das Skript gestartet werden konnte und beendet wurde, error = warum das fehlgeschlagen ist, script dort sind die Skriptfehler gespeichert</returns>
    public ScriptEndedFeedback ExecuteScript(EventTypes? eventname, string scriptname, bool doFemdZelleInvalidate, bool fullCheck, bool changevalues, float tryforsceonds) {
        var m = DatabaseAbstract.EditableErrorReason(Database, EditableErrorReason.EditAcut);
        if (!string.IsNullOrEmpty(m) || Database == null) { return new ScriptEndedFeedback("Automatische Prozesse nicht m�glich: " + m, false, "Allgemein"); }

        var t = DateTime.Now;
        do {
            var erg = ExecuteScript(eventname, scriptname, doFemdZelleInvalidate, fullCheck, changevalues);
            if (erg.AllOk) { return erg; }
            if (!erg.GiveItAnotherTry || DateTime.Now.Subtract(t).TotalSeconds > tryforsceonds) { return erg; }
        } while (true);
    }

    public void InvalidateCheckData() {
        LastCheckedRowFeedback = null;
        LastCheckedEventArgs = null;
        LastCheckedMessage = null;
    }

    public bool IsNullOrEmpty() {
        if (Database == null || Database.IsDisposed) { return true; }
        return Database.Column.All(thisColumnItem => thisColumnItem != null && CellIsNullOrEmpty(thisColumnItem));
    }

    public bool IsNullOrEmpty(ColumnItem? column) {
        if (Database == null || Database.IsDisposed) { return true; }
        return Database.Cell.IsNullOrEmpty(column, this);
    }

    public bool MatchesTo(FilterItem filter) {
        if (Database == null || Database.IsDisposed) { return false; }

        if (filter.IsDisposed) { return true; }

        if (filter.Database != Database) { return false; }

        filter.Column?.RefreshColumnsData();

        if (filter.FilterType is FilterType.KeinFilter or FilterType.Gro�KleinEgal) { return true; } // Filter ohne Funktion
        if (filter.Column == null) {
            if (!filter.FilterType.HasFlag(FilterType.Gro�KleinEgal)) { filter.FilterType |= FilterType.Gro�KleinEgal; }
            if (filter.FilterType is not FilterType.Instr_Gro�KleinEgal and not FilterType.Instr_UND_Gro�KleinEgal) { Develop.DebugPrint(FehlerArt.Fehler, "Zeilenfilter nur mit Instr m�glich!"); }
            if (filter.SearchValue.Count < 1) { Develop.DebugPrint(FehlerArt.Fehler, "Zeilenfilter nur mit mindestens einem Wert m�glich"); }

            return filter.SearchValue.All(RowFilterMatch);
        }

        if (!Database.Cell.MatchesTo(filter.Column, this, filter)) { return false; }

        return true;
    }

    public bool MatchesTo(ICollection<FilterItem>? filter) {
        if (Database == null || Database.IsDisposed) { return false; }
        if (filter == null || filter.Count == 0) { return true; }

        Database.RefreshColumnsData(filter);

        foreach (var thisFilter in filter) {
            //if (thisFilter.Database != filter[0].Database) { Develop.DebugPrint_NichtImplementiert(); }

            if (!MatchesTo(thisFilter)) { return false; }
        }
        return true;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="txt"></param>
    /// <param name="replacedvalue">Wenn true, wird der Wert durch den leserlichen Zelleninhalt ersetzt. Bei False durch den Origial Zelleninhalt</param>
    /// <param name="removeLineBreaks"></param>
    /// <returns></returns>
    public string ReplaceVariables(string txt, bool replacedvalue, bool removeLineBreaks) {
        if (Database == null || Database.IsDisposed) { return txt; }

        var erg = txt;
        // Variablen ersetzen
        foreach (var thisColumnItem in Database.Column) {
            if (!erg.Contains("~")) { return erg; }

            if (thisColumnItem != null) {
                if (erg.ToUpper().Contains("~" + thisColumnItem.Name.ToUpper())) {
                    var replacewith = CellGetString(thisColumnItem);
                    if (replacedvalue) { replacewith = CellItem.ValueReadable(thisColumnItem, replacewith, ShortenStyle.Replaced, BildTextVerhalten.Nur_Text, removeLineBreaks); }
                    if (removeLineBreaks && !replacedvalue) {
                        replacewith = replacewith.Replace("\r\n", " ");
                        replacewith = replacewith.Replace("\r", " ");
                    }

                    erg = erg.Replace("~" + thisColumnItem.Name.ToUpper() + "~", replacewith, RegexOptions.IgnoreCase);
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

    public void VariableToCell(ColumnItem? column, VariableCollection vars) {
        var m = DatabaseAbstract.EditableErrorReason(Database, EditableErrorReason.EditAcut);
        if (string.IsNullOrEmpty(m) || Database == null || column == null) { return; }

        var columnVar = vars.Get(column.Name);
        if (columnVar == null || columnVar.ReadOnly) { return; }
        if (!column.SaveContent || !column.Format.CanBeChangedByRules()) { return; }

        //if (column.Format == DataFormat.Verkn�pfung_zu_anderer_Datenbank) {
        //    var columnLinkVar = vars.GetSystem(column.Name + "_Link");
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
        if (columnVar is VariableDateTime vd) {
            var x = vd.ValueDate.ToString(Constants.Format_Date7);
            x = x.TrimEnd(".000");
            x = x.TrimEnd(".0");
            x = x.TrimEnd("00:00:00");
            x = x.TrimEnd(" ");
            CellSet(column, x);
            return;
        }

        Develop.DebugPrint("Typ nicht erkannt: " + columnVar.MyClassId);
    }

    internal bool NeedDataCheck() {
        if (Database == null || Database.IsDisposed) { return false; }
        return Database.Row.NeedDataCheck(Key);
    }

    internal void OnDoSpecialRules(DoRowAutomaticEventArgs e) => DoSpecialRules?.Invoke(this, e);

    internal void OnRowChecked(RowCheckedEventArgs e) => RowChecked?.Invoke(this, e);

    private void Cell_CellValueChanged(object sender, CellEventArgs e) {
        if (e.Row != this) { return; }
        _tmpQuickInfo = null;
    }

    private void Database_Disposing(object sender, System.EventArgs e) => Dispose();

    private void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
            }
            // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer �berschreiben
            // TODO: Gro�e Felder auf NULL setzen

            if (Database != null && !Database.IsDisposed) {
                Database.Cell.CellValueChanged -= Cell_CellValueChanged;
                Database.Disposing -= Database_Disposing;
            }
            Database = null;
            _tmpQuickInfo = null;
            IsDisposed = true;
        }
    }

    private ScriptEndedFeedback ExecuteScript(EventTypes? eventname, string scriptname, bool doFemdZelleInvalidate, bool fullCheck, bool changevalues) {
        var m = DatabaseAbstract.EditableErrorReason(Database, EditableErrorReason.EditAcut);
        if (!string.IsNullOrEmpty(m) || Database == null) { return new ScriptEndedFeedback("Automatische Prozesse nicht m�glich: " + m, false, "Allgemein"); }

        var feh = Database.EditableErrorReason(EditableErrorReason.EditAcut);
        if (!string.IsNullOrEmpty(feh)) { return new ScriptEndedFeedback(feh, true, "Allgemein"); }

        // Zuerst die Aktionen ausf�hren und falls es einen Fehler gibt, die Spalten und Fehler auch ermitteln
        DoingScript = true;
        var script = Database.ExecuteScript(eventname, scriptname, changevalues, this);

        if (!script.AllOk) {
            Database.OnScriptError(new RowCancelEventArgs(this, script.ProtocolText));
            DoingScript = false;
            return script;// (true, "<b>Das Skript ist fehlerhaft:</b>\r\n" + "Zeile: " + script.Line + "\r\n" + script.Error + "\r\n" + script.ErrorCode, script);
        }

        if (changevalues) {
            // Gucken, ob noch ein Fehler da ist, der von einer besonderen anderen Routine kommt. Beispiel Bildzeichen-Liste: Bandart und Einl�ufe
            DoRowAutomaticEventArgs e = new(this, eventname);
            OnDoSpecialRules(e);
        }

        DoingScript = false;

        if (!changevalues) { return new ScriptEndedFeedback(string.Empty, false, "Allgemein"); }

        // checkPerformed geht von Dateisystemfehlern aus

        // Dann die abschlie�enden Korrekturen vornehmen
        foreach (var thisColum in Database.Column) {
            if (thisColum != null) {
                if (fullCheck) {
                    var x = CellGetString(thisColum);
                    var x2 = thisColum.AutoCorrect(x, true);
                    if (thisColum.Format is not DataFormat.Verkn�pfung_zu_anderer_Datenbank && x != x2) {
                        Database.Cell.Set(thisColum, this, x2);
                    } else {
                        if (!thisColum.IsFirst()) {
                            Database.Cell.DoSpecialFormats(thisColum, this, CellGetString(thisColum), true);
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
        if (Database == null || Database.IsDisposed ||
            string.IsNullOrEmpty(Database.ZeilenQuickInfo)) {
            _tmpQuickInfo = string.Empty;
            return;
        }

        _tmpQuickInfo = ReplaceVariables(Database.ZeilenQuickInfo, true, false);
    }

    private bool RowFilterMatch(string searchText) {
        if (string.IsNullOrEmpty(searchText)) { return true; }
        if (Database == null || Database.IsDisposed) { return false; }

        searchText = searchText.ToUpper();
        foreach (var thisColumnItem in Database.Column) {
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