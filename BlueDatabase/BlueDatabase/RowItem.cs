// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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
using BlueScript;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BlueScript.Variables;
using static BlueBasics.Converter;

namespace BlueDatabase {

    public sealed class RowItem : ICanBeEmpty, IDisposable {

        #region Fields

        private bool _disposedValue;
        private string? _tmpQuickInfo;

        #endregion

        #region Constructors

        public RowItem(Database? database, long key) {
            Database = database;
            Key = key;
            _tmpQuickInfo = null;
            if (Database != null) {
                Database.Cell.CellValueChanged += Cell_CellValueChanged;
                Database.Disposing += Database_Disposing;
            }
        }

        public RowItem(Database database) : this(database, database.Row.NextRowKey()) { }

        #endregion

        #region Destructors

        // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
        ~RowItem() {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(false);
        }

        #endregion

        #region Events

        public event EventHandler<DoRowAutomaticEventArgs> DoSpecialRules;

        public event EventHandler<RowCheckedEventArgs> RowChecked;

        #endregion

        #region Properties

        /// <summary>
        /// Sehr rudimentäre Angabe!
        /// </summary>
        public static bool DoingScript { get; private set; }

        public Database? Database { get; private set; }
        public long Key { get; }

        public string QuickInfo {
            get {
                if (_tmpQuickInfo != null) { return _tmpQuickInfo; }
                GenerateQuickInfo();
                return _tmpQuickInfo!;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Diese Routine konvertiert den Inhalt der Zelle in eine vom Skript lesbaren Variable
        /// </summary>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public static List<Variable>? CellToVariable(ColumnItem? column, RowItem? row) {
            if (column == null || row == null) { return null; }
            if (!column.Format.CanBeCheckedByRules()) { return null; }
            if (!column.SaveContent) { return null; }

            #region ReadOnly bestimmen

            var ro = !column.Format.CanBeChangedByRules();
            //if (column == column.Database.Column.SysCorrect) { ro = true; }
            //if (column == column.Database.Column.SysRowChanger) { ro = true; }
            //if (column == column.Database.Column.SysRowChangeDate) { ro = true; }

            #endregion

            if (column.MultiLine && column.Format == BlueBasics.Enums.DataFormat.Link_To_Filesystem) { return null; }
            if (column.ScriptType == ScriptType.Nicht_vorhanden) { return null; }

            var wert = row.CellGetString(column);
            var qi = "Spalte: " + column.ReadableText();

            var vars = new List<Variable>();

            switch (column.Format) {
                //case enDataFormat.Verknüpfung_zu_anderer_Datenbank:
                //    //if (column.LinkedCell_RowKeyIsInColumn == -9999) {
                //    wert = string.Empty; // Beim Skript-Start ist dieser Wert immer leer, da die Verlinkung erst erstellt werden muss.
                //    //vars.Add(new Variable(column.Name + "_link", string.Empty, VariableDataType.String, true, true, "Dieser Wert kann nur mit SetLink verändert werden.\r\nBeim Skript-Start ist dieser Wert immer leer, da die Verlinkung erst erstellt werden muss."));
                //    //} else {
                //    //    qi = "Spalte: " + column.ReadableText() + "\r\nDer Inhalt wird zur Startzeit des Skripts festgelegt.";
                //    //    ro = true;
                //    //}
                //    break;

                case BlueBasics.Enums.DataFormat.Link_To_Filesystem:
                    qi = "Spalte: " + column.ReadableText() + "\r\nFalls die Datei auf der Festplatte existiert, wird eine weitere\r\nVariable erzeugt: " + column.Name + "_FileName";
                    var f = column.Database.Cell.BestFile(column, row);
                    if (f.FileType() == enFileFormat.Image && FileOperations.FileExists(f)) {
                        vars.Add(new VariableString(column.Name + "_FileName", f, true, false, "Spalte: " + column.ReadableText() + "\r\nEnthält den vollen Dateinamen der Datei der zugehörigen Zelle.\r\nDie Existenz der Datei wurde geprüft und die Datei existert.\r\nAuf die Datei kann evtl. mit LoadImage zugegriffen werden."));
                    }
                    break;

                    //case enDataFormat.Columns_für_LinkedCellDropdown:
                    //    if (IntTryParse(wert, out var colKey)) {
                    //        var c = column.LinkedDatabase().Column.SearchByKey(colKey);
                    //        if (c != null) { wert = c.Name; }
                    //    }
                    //    break;
            }

            switch (column.ScriptType) {
                case ScriptType.Bool:
                    vars.Add(new VariableBool(column.Name, wert == "+", ro, false, qi));

                    break;

                case ScriptType.List:
                    vars.Add(new VariableListString(column.Name, wert.SplitAndCutByCrToList(), ro, false, qi));
                    break;

                case ScriptType.Numeral:
                    FloatTryParse(wert, out var f);
                    vars.Add(new VariableFloat(column.Name, f, ro, false, qi));
                    break;

                case ScriptType.String:
                    vars.Add(new VariableString(column.Name, wert, ro, false, qi));
                    break;

                case ScriptType.String_Readonly:
                    vars.Add(new VariableString(column.Name, wert, true, false, qi));
                    break;

                default:
                    Develop.DebugPrint(column.ScriptType);
                    break;
            }

            return vars;
        }

        public string CellFirstString() => Database.Cell.GetString(Database.Column[0], this);

        public bool CellGetBoolean(string columnName) => Database.Cell.GetBoolean(Database.Column[columnName], this);

        public bool CellGetBoolean(ColumnItem? column) => Database.Cell.GetBoolean(column, this);

        public Color CellGetColor(string columnName) => Database.Cell.GetColor(Database.Column[columnName], this);

        public Color CellGetColor(ColumnItem? column) => Database.Cell.GetColor(column, this);

        public int CellGetColorBgr(ColumnItem? column) => Database.Cell.GetColorBgr(column, this);

        public string CellGetCompareKey(ColumnItem? column) => Database.Cell.CompareKey(column, this);

        public DateTime CellGetDateTime(string columnName) => Database.Cell.GetDateTime(Database.Column[columnName], this);

        public DateTime CellGetDateTime(ColumnItem? column) => Database.Cell.GetDateTime(column, this);

        public double CellGetDouble(string columnName) => Database.Cell.GetDouble(Database.Column[columnName], this);

        public double CellGetDouble(ColumnItem? column) => Database.Cell.GetDouble(column, this);

        public int CellGetInteger(string columnName) => Database.Cell.GetInteger(Database.Column[columnName], this);

        public int CellGetInteger(ColumnItem? column) => Database.Cell.GetInteger(column, this);

        public List<string?> CellGetList(string columnName) => Database.Cell.GetList(Database.Column[columnName], this);

        public List<string?> CellGetList(ColumnItem? column) => Database.Cell.GetList(column, this);

        public Point CellGetPoint(string columnName) => Database.Cell.GetPoint(Database.Column[columnName], this);

        public Point CellGetPoint(ColumnItem? column) => Database.Cell.GetPoint(column, this);

        public string CellGetString(string columnName) => Database.Cell.GetString(Database.Column[columnName], this);

        public string CellGetString(ColumnItem? column) => Database.Cell.GetString(column, this);

        public List<string> CellGetValuesReadable(ColumnItem? column, ShortenStyle style) => Database.Cell.ValuesReadable(column, this, style);

        public bool CellIsNullOrEmpty(string columnName) => Database.Cell.IsNullOrEmpty(Database.Column[columnName], this);

        public bool CellIsNullOrEmpty(ColumnItem? column) => Database.Cell.IsNullOrEmpty(column, this);

        public void CellSet(string columnName, bool value) => Database.Cell.Set(Database.Column[columnName], this, value);

        public void CellSet(ColumnItem? column, bool value) => Database.Cell.Set(column, this, value);

        public void CellSet(string columnName, string value) => Database.Cell.Set(Database.Column[columnName], this, value);

        public void CellSet(ColumnItem? column, string value) => Database.Cell.Set(column, this, value);

        public void CellSet(string columnName, double value) => Database.Cell.Set(Database.Column[columnName], this, value);

        public void CellSet(ColumnItem? column, double value) => Database.Cell.Set(column, this, value);

        public void CellSet(string columnName, int value) => Database.Cell.Set(Database.Column[columnName], this, value);

        public void CellSet(ColumnItem? column, int value) => Database.Cell.Set(column, this, value);

        public void CellSet(string columnName, Point value) => Database.Cell.Set(Database.Column[columnName], this, value);

        public void CellSet(ColumnItem? column, Point value) => Database.Cell.Set(column, this, value);

        public void CellSet(string columnName, List<string?>? value) => Database.Cell.Set(Database.Column[columnName], this, value);

        public void CellSet(ColumnItem? column, List<string?>? value) => Database.Cell.Set(column, this, value);

        public void CellSet(string columnName, DateTime value) => Database.Cell.Set(Database.Column[columnName], this, value);

        public void CellSet(ColumnItem? column, DateTime value) => Database.Cell.Set(column, this, value);

        /// <summary>
        /// Erstellt einen sortierfähigen String eine Zeile mit der Standard sortierung
        /// </summary>
        /// <param name="columns">Nur diese Spalten in deser Reihenfolge werden berücksichtigt</param>
        /// <returns>Den String mit dem abschluß <<>key<>> und dessen Key.</returns>
        public string CompareKey() => CompareKey(Database.SortDefinition?.Columns);

        /// <summary>
        /// Erstellt einen Sortierfähigen String eine Zeile
        /// </summary>
        /// <param name="columns">Nur diese Spalten in deser Reihenfolge werden berücksichtigt</param>
        /// <returns>Den String mit dem abschluß <<>key<>> und dessen Key.</returns>
        public string CompareKey(List<ColumnItem>? columns) {
            StringBuilder r = new();
            if (columns != null) {
                foreach (var t in columns) {
                    r.Append(CellGetCompareKey(t) + Constants.FirstSortChar);
                }
            }
            r.Append(Constants.SecondSortChar + "<key>" + Key);
            return r.ToString();
        }

        public void Dispose() {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public (bool checkPerformed, string error, Script? script) DoAutomatic(string startroutine) => DoAutomatic(false, false, startroutine);

        /// <summary>
        /// Führt Regeln aus, löst Ereignisses, setzt SysCorrect und auch die initalwerte der Zellen.
        /// Z.b: Runden, Großschreibung wird nur bei einem FullCheck korrigiert, das wird normalerweise vor dem Setzen bei CellSet bereits korrigiert.
        /// </summary>
        /// <param name="doFemdZelleInvalidate">bei verlinkten Zellen wird der verlinkung geprüft und erneuert.</param>
        /// <param name="fullCheck">Runden, Großschreibung, etc. wird ebenfalls durchgefphrt</param>
        /// <param name="tryforsceonds"></param>
        /// <returns>checkPerformed  = ob das Skript gestartet werden konnte und beendet wurde, error = warum das fehlgeschlagen ist, script dort sind die Skriptfehler gespeichert</returns>
        public (bool checkPerformed, string error, Script? script) DoAutomatic(bool doFemdZelleInvalidate, bool fullCheck, float tryforsceonds, string startroutine) {
            if (Database == null || Database.ReadOnly) { return (false, "Automatische Prozesse nicht möglich, da die Datenbank schreibgeschützt ist", null); }
            var t = DateTime.Now;
            do {
                var erg = DoAutomatic(doFemdZelleInvalidate, fullCheck, startroutine);
                if (erg.checkPerformed) { return erg; }
                if (DateTime.Now.Subtract(t).TotalSeconds > tryforsceonds) { return erg; }
            } while (true);
        }

        /// <summary>
        /// Führt Regeln aus, löst Ereignisses, setzt SysCorrect und auch die initalwerte der Zellen.
        /// Z.b: Runden, Großschreibung wird nur bei einem FullCheck korrigiert, das wird normalerweise vor dem Setzen bei CellSet bereits korrigiert.
        /// </summary>
        /// <param name="doFemdZelleInvalidate">bei verlinkten Zellen wird der verlinkung geprüft und erneuert.</param>
        /// <param name="fullCheck">Runden, Großschreibung, etc. wird ebenfalls durchgeführt</param>
        /// <returns>checkPerformed  = ob das Skript gestartet werden konnte und beendet wurde, error = warum das fehlgeschlagen ist, script dort sind die Skriptfehler gespeichert</returns>
        public (bool checkPerformed, string error, Script? skript) DoAutomatic(bool doFemdZelleInvalidate, bool fullCheck, string startroutine) {
            if (Database == null || Database.ReadOnly) { return (false, "Automatische Prozesse nicht möglich, da die Datenbank schreibgeschützt ist", null); }

            var feh = Database.ErrorReason(enErrorReason.EditAcut);
            if (!string.IsNullOrEmpty(feh)) { return (false, feh, null); }

            // Zuerst die Aktionen ausführen und falls es einen Fehler gibt, die Spalten und Fehler auch ermitteln
            DoingScript = true;
            var script = DoRules(startroutine);
            DoingScript = false;

            if (startroutine == "script testing") { return (true, string.Empty, script); }

            // checkPerformed geht von Dateisystemfehlern aus
            if (!string.IsNullOrEmpty(script.Error)) {
                Database.OnScriptError(new RowCancelEventArgs(this, "Zeile: " + script.Line + "\r\n" + script.Error + "\r\n" + script.ErrorCode));
                return (true, "<b>Das Skript ist fehlerhaft:</b>\r\n" + "Zeile: " + script.Line + "\r\n" + script.Error + "\r\n" + script.ErrorCode, script);
            }

            if (script?.Variables == null || !((VariableBool)script.Variables.GetSystem("CellChangesEnabled")).ValueBool) { return (true, string.Empty, script); }

            // Dann die Abschließenden Korrekturen vornehmen
            foreach (var thisColum in Database.Column.Where(thisColum => thisColum != null)) {
                if (fullCheck) {
                    var x = CellGetString(thisColum);
                    var x2 = thisColum.AutoCorrect(x);
                    if (thisColum.Format is not BlueBasics.Enums.DataFormat.Verknüpfung_zu_anderer_Datenbank && x != x2) {
                        Database.Cell.Set(thisColum, this, x2);
                    } else {
                        if (!thisColum.IsFirst()) {
                            Database.Cell.DoSpecialFormats(thisColum, Key, CellGetString(thisColum), false);
                        }
                    }
                    CellCollection.Invalidate_CellContentSize(thisColum, this);
                    thisColum.Invalidate_TmpColumnContentWidth();
                    doFemdZelleInvalidate = false; // Hier ja schon bei jedem gemacht
                }
                if (doFemdZelleInvalidate && thisColum.LinkedDatabase != null) {
                    CellCollection.Invalidate_CellContentSize(thisColum, this);
                    thisColum.Invalidate_TmpColumnContentWidth();
                }
            }

            List<string> cols = new();
            var infoTxt = "<b><u>" + CellGetString(Database.Column[0]) + "</b></u><br><br>";

            var fs = script.Feedback.SplitAndCutByCrToList().SortedDistinctList();
            foreach (var thiss in fs) {
                cols.AddIfNotExists(thiss);
                var t = thiss.SplitBy("|");
                var thisc = Database.Column[t[0]];
                if (thisc != null) {
                    infoTxt = infoTxt + "<b>" + thisc.ReadableText() + ":</b> " + t[1] + "<br><hr><br>";
                }
            }

            //foreach (var thisc in Database.Column) {
            //    var n = thisc.Name + "_error";
            //    var va = BlueScript.Variablen.GetSystem(n);
            //    if (va != null) {
            //        cols.Add(thisc.Name + "|" + va.ValueString);
            //        _InfoTXT = _InfoTXT + "<b>" + thisc.ReadableText() + ":</b> " + va.ValueString + "<br><hr><br>";
            //    }
            //}
            if (cols.Count == 0) {
                infoTxt += "Diese Zeile ist fehlerfrei.";
            }
            if (Database.Column.SysCorrect.SaveContent) {
                if (IsNullOrEmpty(Database.Column.SysCorrect) || cols.Count == 0 != CellGetBoolean(Database.Column.SysCorrect)) {
                    CellSet(Database.Column.SysCorrect, cols.Count == 0);
                }
            }
            OnRowChecked(new RowCheckedEventArgs(this, cols));
            return (true, infoTxt, script);
        }

        public bool IsNullOrEmpty() => Database.Column.All(thisColumnItem => thisColumnItem != null && CellIsNullOrEmpty(thisColumnItem));

        public bool IsNullOrEmpty(ColumnItem? column) => Database.Cell.IsNullOrEmpty(column, this);

        public bool IsNullOrEmpty(string columnName) => Database.Cell.IsNullOrEmpty(Database.Column[columnName], this);

        public bool MatchesTo(FilterItem? filter) {
            if (Database == null) { return false; }

            if (filter != null) {
                if (filter.FilterType is FilterType.KeinFilter or FilterType.GroßKleinEgal) { return true; } // Filter ohne Funktion
                if (filter.Column == null) {
                    if (!filter.FilterType.HasFlag(FilterType.GroßKleinEgal)) { filter.FilterType |= FilterType.GroßKleinEgal; }
                    if (filter.FilterType is not FilterType.Instr_GroßKleinEgal and not FilterType.Instr_UND_GroßKleinEgal) { Develop.DebugPrint(enFehlerArt.Fehler, "Zeilenfilter nur mit Instr möglich!"); }
                    if (filter.SearchValue.Count < 1) { Develop.DebugPrint(enFehlerArt.Fehler, "Zeilenfilter nur mit mindestens einem Wert möglich"); }

                    return filter.SearchValue.All(t => RowFilterMatch(t));
                }

                if (!Database.Cell.MatchesTo(filter.Column, this, filter)) { return false; }
            }
            return true;
        }

        public bool MatchesTo(List<FilterItem>? filter) {
            if (Database == null) { return false; }
            if (filter == null || filter.Count == 0) { return true; }
            foreach (var thisFilter in filter) {
                if (thisFilter.Database != filter[0].Database) { Develop.DebugPrint_NichtImplementiert(); }

                if (!MatchesTo(thisFilter)) { return false; }
            }
            return true;
        }

        /// <summary>
        /// Ersetzt Spaltennamen mit dem dementsprechenden Wert der Zelle. Format: &Spaltenname; oder &Spaltenname(L,8);
        /// </summary>
        /// <param name="formel"></param>
        /// <param name="fulltext">Bei TRUE wird der Text so zurückgegeben, wie er in der Zelle angezeigt werden würde: Mit Suffix und Ersetzungen. Zeilenumbrüche werden eleminiert!</param>
        /// <returns></returns>
        public string ReplaceVariables(string formel, bool fulltext, bool removeLineBreaks) {
            if (Database == null) { return formel; }

            var erg = formel;
            // Variablen ersetzen
            foreach (var thisColumnItem in Database.Column) {
                if (!erg.Contains("~")) { return erg; }
                if (thisColumnItem != null) {
                    var txt = CellGetString(thisColumnItem);
                    if (fulltext) { txt = CellItem.ValueReadable(thisColumnItem, txt, ShortenStyle.Replaced, BildTextVerhalten.Nur_Text, removeLineBreaks); }
                    if (removeLineBreaks && !fulltext) {
                        txt = txt.Replace("\r\n", " ");
                        txt = txt.Replace("\r", " ");
                    }

                    erg = erg.Replace("~" + thisColumnItem.Name.ToUpper() + "~", txt, RegexOptions.IgnoreCase);
                    while (erg.ToUpper().Contains("~" + thisColumnItem.Name.ToUpper() + "(")) {
                        var x = erg.ToUpper().IndexOf("~" + thisColumnItem.Name.ToUpper() + "(");
                        var x2 = erg.IndexOf(")", x);
                        if (x2 < x) { return erg; }
                        var ww = erg.Substring(x + thisColumnItem.Name.Length + 2, x2 - x - thisColumnItem.Name.Length - 2);
                        ww = ww.Replace(" ", string.Empty).ToUpper();
                        var vals = ww.SplitAndCutBy(",");
                        if (vals.Length != 2) { return formel; }
                        if (vals[0] != "L") { return formel; }
                        if (!IntTryParse(vals[1], out var stellen)) { return formel; }
                        var newW = txt.Substring(0, Math.Min(stellen, txt.Length));
                        erg = erg.Replace(erg.Substring(x, x2 - x + 1), newW);
                    }
                }
            }
            return erg;
        }

        internal void OnDoSpecialRules(DoRowAutomaticEventArgs e) => DoSpecialRules?.Invoke(this, e);

        internal void OnRowChecked(RowCheckedEventArgs e) => RowChecked?.Invoke(this, e);

        private void Cell_CellValueChanged(object sender, CellEventArgs e) {
            if (e.Row != this) { return; }
            _tmpQuickInfo = null;
        }

        private void Database_Disposing(object sender, System.EventArgs e) => Dispose();

        private void Dispose(bool disposing) {
            if (!_disposedValue) {
                if (disposing) {
                    // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
                }
                // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
                // TODO: Große Felder auf NULL setzen
                Database.Cell.CellValueChanged -= Cell_CellValueChanged;
                Database.Disposing -= Database_Disposing;
                Database = null;
                _tmpQuickInfo = null;
                _disposedValue = true;
            }
        }

        /// <summary>
        /// Führt alle Regeln aus und löst das Ereignis DoSpecialRules aus. Setzt ansonsten keine Änderungen, wie z.B. SysCorrect oder Runden-Befehle.
        /// </summary>
        /// <returns>Gibt Regeln, die einen Fehler verursachen zurück. z.B. SPALTE1|Die Splate darf nicht leer sein.</returns>
        private Script DoRules(string startRoutine) {
            try {
                List<Variable> vars = new()
                {
                    new VariableString("Startroutine", startRoutine, true, false, "ACHTUNG: Keinesfalls dürfen Startroutinenabhängig Werte verändert werden.\r\nMögliche Werte:\r\nnew row\r\nvalue changed\r\nscript testing\r\nmanual check\r\nto be sure\r\nimport\r\nexport\r\nscript"),
                    new VariableBool("CellChangesEnabled", true, true, true, "Nur wenn TRUE werden nach dem Skript die Änderungen\r\nin die Datenbank aufgenommen.\r\nKann mit DisableCellChanges umgesetzt werden.")
                };

                #region Variablen für Skript erstellen

                foreach (var thisCol in Database.Column) {
                    var v = CellToVariable(thisCol, this);
                    if (v != null) { vars.AddRange(v); }
                }
                vars.Add(new VariableString("User", Generic.UserName(), true, false, "ACHTUNG: Keinesfalls dürfen benutzerabhängig Werte verändert werden."));
                vars.Add(new VariableString("Usergroup", Database.UserGroup, true, false, "ACHTUNG: Keinesfalls dürfen gruppenabhängig Werte verändert werden."));
                vars.Add(new VariableBool("Administrator", Database.IsAdministrator(), true, false, "ACHTUNG: Keinesfalls dürfen gruppenabhängig Werte verändert werden.\r\nDiese Variable gibt zurück, ob der Benutzer Admin für diese Datenbank ist."));
                vars.Add(new VariableString("Filename", Database.Filename, true, true, string.Empty));

                #endregion Variablen für Skript erstellen

                #region Script ausführen

                Script sc = new(vars, Database.AdditionaFilesPfadWhole()) {
                    ScriptText = Database.RulesScript
                };
                sc.Parse();

                #endregion

                if (startRoutine != "script testing" && ((VariableBool)vars.GetSystem("CellChangesEnabled")).ValueBool) {

                    #region Variablen zurückschreiben und Special Rules ausführen

                    if (!string.IsNullOrEmpty(sc.Error)) { return sc; }
                    foreach (var thisCol in Database.Column) { VariableToCell(thisCol, vars); }
                    // Gucken, ob noch ein Fehler da ist, der von einer besonderen anderen Routine kommt. Beispiel Bildzeichen-Liste: Bandart und Einläufe
                    DoRowAutomaticEventArgs e = new(this);
                    OnDoSpecialRules(e);

                    #endregion
                }
                return sc;
            } catch {
                return DoRules(startRoutine);
            }
        }

        private void GenerateQuickInfo() {
            if (string.IsNullOrEmpty(Database.ZeilenQuickInfo)) { _tmpQuickInfo = string.Empty; return; }
            _tmpQuickInfo = ReplaceVariables(Database.ZeilenQuickInfo, true, false);
        }

        private bool RowFilterMatch(string searchText) {
            if (string.IsNullOrEmpty(searchText)) { return true; }
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

        private void VariableToCell(ColumnItem? column, List<Variable> vars) {
            if (Database == null || Database.ReadOnly) { return; }

            var columnVar = vars.Get(column.Name);
            if (columnVar == null || columnVar.Readonly) { return; }
            if (!column.SaveContent || !column.Format.CanBeChangedByRules()) { return; }

            //if (column.Format == enDataFormat.Verknüpfung_zu_anderer_Datenbank) {
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
        }

        #endregion
    }
}