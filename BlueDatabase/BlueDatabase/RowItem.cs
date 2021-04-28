#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2021 Christian Peter
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
#endregion

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using BlueScript;
using Skript.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;

namespace BlueDatabase {
    public sealed class RowItem : ICanBeEmpty {

        #region  Variablen-Deklarationen 

        public readonly Database Database;
        public string TMP_Chapter;
        public bool TMP_Expanded;
        public Rectangle? TMP_CaptionPos;
        public int? TMP_Y = null;
        public int? TMP_DrawHeight = null;

        private string? _tmpQuickInfo = null;

        #endregion

        public event EventHandler<RowCheckedEventArgs> RowChecked;
        public event EventHandler<DoRowAutomaticEventArgs> DoSpecialRules;

        #region  Construktor + Initialize 

        public RowItem(Database database, int key) {
            Database = database;
            Key = key;
            TMP_Chapter = string.Empty;
            TMP_Y = null;
            TMP_DrawHeight = null;
            TMP_Expanded = true;
            TMP_CaptionPos = null;
            _tmpQuickInfo = null;

            Database.Cell.CellValueChanged += Cell_CellValueChanged;
        }

        public RowItem(Database database) : this(database, database.Row.NextRowKey()) { }

        #endregion

        #region  Properties 
        public int Key { get; }

        public string QuickInfo {
            get {
                if (_tmpQuickInfo != null) { return _tmpQuickInfo; }
                GenerateQuickInfo();
                return _tmpQuickInfo;

            }

        }

        private void GenerateQuickInfo() {
            if (string.IsNullOrEmpty(Database.ZeilenQuickInfo)) { _tmpQuickInfo = string.Empty; return; }
            _tmpQuickInfo = ReplaceVariables(Database.ZeilenQuickInfo, true, false);
        }

        #endregion

        #region Cell Get / Set

        #region bool
        public bool CellGetBoolean(string columnName) {
            return Database.Cell.GetBoolean(Database.Column[columnName], this);
        }
        public bool CellGetBoolean(ColumnItem column) {
            return Database.Cell.GetBoolean(column, this);
        }

        public void CellSet(string columnName, bool value) {
            Database.Cell.Set(Database.Column[columnName], this, value);
        }

        public void CellSet(ColumnItem column, bool value) {
            Database.Cell.Set(column, this, value);
        }
        #endregion

        #region string
        public string CellFirstString() {
            return Database.Cell.GetString(Database.Column[0], this);
        }
        public string CellGetString(string columnName) {
            return Database.Cell.GetString(Database.Column[columnName], this);
        }
        public string CellGetString(ColumnItem column) {
            return Database.Cell.GetString(column, this);
        }

        public void CellSet(string columnName, string value) {
            Database.Cell.Set(Database.Column[columnName], this, value);
        }

        public void CellSet(ColumnItem column, string value) {
            Database.Cell.Set(column, this, value);
        }
        #endregion

        #region double
        public double CellGetDouble(string columnName) {
            return Database.Cell.GetDouble(Database.Column[columnName], this);
        }
        public double CellGetDouble(ColumnItem column) {
            return Database.Cell.GetDouble(column, this);
        }

        public void CellSet(string columnName, double value) {
            Database.Cell.Set(Database.Column[columnName], this, value);
        }

        public void CellSet(ColumnItem column, double value) {
            Database.Cell.Set(column, this, value);
        }
        #endregion

        #region decimal
        public decimal CellGetDecimal(string columnName) {
            return Database.Cell.GetDecimal(Database.Column[columnName], this);
        }
        public decimal CellGetDecimal(ColumnItem column) {
            return Database.Cell.GetDecimal(column, this);
        }

        public void CellSet(string columnName, decimal value) {
            Database.Cell.Set(Database.Column[columnName], this, value);
        }
        public void CellSet(ColumnItem column, decimal value) {
            Database.Cell.Set(column, this, value);
        }
        #endregion

        #region int
        public int CellGetInteger(string columnName) {
            return Database.Cell.GetInteger(Database.Column[columnName], this);
        }
        public int CellGetInteger(ColumnItem column) {
            return Database.Cell.GetInteger(column, this);
        }

        public void CellSet(string columnName, int value) {
            Database.Cell.Set(Database.Column[columnName], this, value);
        }

        public void CellSet(ColumnItem column, int value) {
            Database.Cell.Set(column, this, value);
        }
        #endregion

        #region Point
        public Point CellGetPoint(string columnName) {
            return Database.Cell.GetPoint(Database.Column[columnName], this);
        }
        public Point CellGetPoint(ColumnItem column) {
            return Database.Cell.GetPoint(column, this);
        }

        public void CellSet(string columnName, Point value) {
            Database.Cell.Set(Database.Column[columnName], this, value);
        }

        public void CellSet(ColumnItem column, Point value) {
            Database.Cell.Set(column, this, value);
        }
        #endregion

        #region List<string>
        public List<string> CellGetList(string columnName) {
            return Database.Cell.GetList(Database.Column[columnName], this);
        }
        public List<string> CellGetList(ColumnItem column) {
            return Database.Cell.GetList(column, this);
        }

        public void CellSet(string columnName, List<string> value) {
            Database.Cell.Set(Database.Column[columnName], this, value);
        }

        public void CellSet(ColumnItem column, List<string> value) {
            Database.Cell.Set(column, this, value);
        }
        #endregion

        #region DateTime
        public DateTime CellGetDateTime(string columnName) {
            return Database.Cell.GetDateTime(Database.Column[columnName], this);
        }
        public DateTime CellGetDateTime(ColumnItem column) {
            return Database.Cell.GetDateTime(column, this);
        }

        public void CellSet(string columnName, DateTime value) {
            Database.Cell.Set(Database.Column[columnName], this, value);
        }

        public void CellSet(ColumnItem column, DateTime value) {
            Database.Cell.Set(column, this, value);
        }
        #endregion

        #region color
        public Color CellGetColor(string columnName) {
            return Database.Cell.GetColor(Database.Column[columnName], this);
        }
        public Color CellGetColor(ColumnItem column) {
            return Database.Cell.GetColor(column, this);
        }

        //public void CellSet(string columnName, Color value)
        //{
        //    Database.Cell.Set(Database.Column[columnName], this, value, false);
        //}
        //public void CellSet(string columnName, Color value)
        //{
        //    Database.Cell.Set(Database.Column[columnName], this, value);
        //}
        //public void CellSet(ColumnItem column, Color value)
        //{
        //    Database.Cell.Set(column, this, value, false);
        //}
        //public void CellSet(ColumnItem column, Color value)
        //{
        //    Database.Cell.Set(column, this, value);
        //}




        public int CellGetColorBGR(ColumnItem column) {
            return Database.Cell.GetColorBGR(column, this);
        }

        #endregion


        #endregion

        public List<string> CellGetValuesReadable(ColumnItem Column, enShortenStyle style) {
            return Database.Cell.ValuesReadable(Column, this, style);
        }

        private void Cell_CellValueChanged(object sender, CellEventArgs e) {
            if (e.Row != this) { return; }
            _tmpQuickInfo = null;
        }

        private void VariableToCell(ColumnItem thisCol, List<Variable> vars) {
            var s = vars.Get(thisCol.Name);

            if (s == null) { return; }
            if (s.Readonly) { return; }

            if (thisCol.MultiLine) {
                CellSet(thisCol, s.ValueString);
                return;
            }

            switch (thisCol.Format) {
                case enDataFormat.Bit:
                    if (s.ValueString.ToLower() == "true") {
                        CellSet(thisCol, true);
                    } else {
                        CellSet(thisCol, false);
                    }
                    return;

                case enDataFormat.Ganzzahl:
                case enDataFormat.Gleitkommazahl:
                    CellSet(thisCol, s.ValueString);
                    return;

                case enDataFormat.Text:
                case enDataFormat.BildCode:
                case enDataFormat.Datum_und_Uhrzeit:
                case enDataFormat.FarbeInteger:
                case enDataFormat.RelationText:
                case enDataFormat.Schrift:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.Link_To_Filesystem:
                case enDataFormat.LinkedCell:
                case enDataFormat.Columns_für_LinkedCellDropdown:
                case enDataFormat.Values_für_LinkedCellDropdown:
                    CellSet(thisCol, s.ValueString);
                    return;

            }
        }


        /// <summary>
        /// Diese Routine konvertiert den Inhalt der Zelle in eine vom Skript lesbaren Variable
        /// </summary>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public static Variable CellToVariable(ColumnItem column, RowItem row) {
            if (!column.Format.CanBeCheckedByRules()) { return null; }


            var ro = !column.Format.CanBeChangedByRules();

            if (column == column.Database.Column.SysCorrect) { ro = true; }
            if (column == column.Database.Column.SysRowChanger) { ro = true; }
            if (column == column.Database.Column.SysRowChangeDate) { ro = true; }



            if (column.MultiLine) {
                return new Variable(column.Name, row.CellGetString(column), enVariableDataType.List, ro, false, "Spalte: " + column.ReadableText() + "\r\nMehrzeilige Spalten können nur als Liste bearbeitet werdern.");
            }

            switch (column.Format) {
                case enDataFormat.Bit:
                    if (row.CellGetString(column) == "+") {
                        return new Variable(column.Name, "true", enVariableDataType.Bool, ro, false, "Spalte: " + column.ReadableText());
                    } else {
                        return new Variable(column.Name, "false", enVariableDataType.Bool, ro, false, "Spalte: " + column.ReadableText());
                    }

                case enDataFormat.Ganzzahl:
                case enDataFormat.Gleitkommazahl:
                    return new Variable(column.Name, row.CellGetString(column), enVariableDataType.Numeral, ro, false, "Spalte: " + column.ReadableText());

                default:
                    return new Variable(column.Name, row.CellGetString(column), enVariableDataType.String, ro, false, "Spalte: " + column.ReadableText());

            }
        }

        /// <summary>
        /// Führt alle Regeln aus und löst das Ereignis DoSpecialRules aus. Setzt ansonsten keine Änderungen, wie z.B. SysCorrect oder Runden-Befehle.
        /// </summary>
        /// <returns>Gibt Regeln, die einen Fehler verursachen zurück. z.B. SPALTE1|Die Splate darf nicht leer sein.</returns>
        private BlueScript.Script DoRules(bool onlyTest, string startRoutine) {
            try {
                var vars = new List<Variable>
                {
                new Variable("Startroutine", startRoutine, enVariableDataType.String, true, false, "ACHTUNG: Keinesfalls dürfen Startroutinenabhängig Werte verändert werden.\r\nMögliche Werte: new row, value changed, script testing, manual check, to be sure")
                };

                #region Variablen für Skript erstellen
                foreach (var thisCol in Database.Column) {
                    var v = CellToVariable(thisCol, this);
                    if (v != null) { vars.Add(v); }
                }


                vars.Add(new Variable("User", modAllgemein.UserName(), enVariableDataType.String, true, false, "ACHTUNG: Keinesfalls dürfen benutzerabhängig Werte verändert werden."));


                vars.Add(new Variable("Usergroup", Database.UserGroup, enVariableDataType.String, true, false, "ACHTUNG: Keinesfalls dürfen gruppenabhängig Werte verändert werden."));




                if (Database.IsAdministrator()) {
                    vars.Add(new Variable("Administrator", "true", enVariableDataType.Bool, true, false, "ACHTUNG: Keinesfalls dürfen gruppenabhängig Werte verändert werden.\r\nDiese Variable gibt zurück, ob der Benutzer Admin für diese Datenbank ist."));
                } else {
                    vars.Add(new Variable("Administrator", "false", enVariableDataType.Bool, true, false, "ACHTUNG: Keinesfalls dürfen gruppenabhängig Werte verändert werden.\r\nDiese Variable gibt zurück, ob der Benutzer Admin für diese Datenbank ist."));
                }



                //if (Database.ReadOnly) {
                //    vars.Add(new Variable("ReadOnly", "true", enVariableDataType.Bool, true, false, "Gibt an, ob die Datenbank schreibgeschützt));
                //}
                //else {
                //    vars.Add(new Variable("ReadOnly", "false", enVariableDataType.Bool, true, false));
                //}

                vars.Add(new Variable("Filename", Database.Filename, enVariableDataType.String, true, true, string.Empty));

                #endregion

                var script = new BlueScript.Script(vars) {
                    ScriptText = Database.RulesScript
                };

                script.Parse();

                if (!onlyTest) {

                    if (!string.IsNullOrEmpty(script.Error)) {
                        return script;
                    }

                    foreach (var thisCol in Database.Column) {
                        VariableToCell(thisCol, vars);
                    }

                    // Gucken, ob noch ein Fehler da ist, der von einer besonderen anderen Routine kommt. Beispiel Bildzeichen-Liste: Bandart und Einläufe
                    var e = new DoRowAutomaticEventArgs(this);
                    OnDoSpecialRules(e);
                }

                return script;

            } catch {
                return DoRules(onlyTest, startRoutine);
            }
        }

        public (bool didSuccesfullyCheck, string error, BlueScript.Script script) DoAutomatic(bool onlyTest, string startroutine) {
            return DoAutomatic(false, false, onlyTest, startroutine);
        }

        /// <summary>
        /// Führt Regeln aus, löst Ereignisses, setzt SysCorrect und auch die initalwerte der Zellen.
        /// Z.b: Runden, Großschreibung wird nur bei einem FullCheck korrigiert, das wird normalerweise vor dem Setzen bei CellSet bereits korrigiert.
        /// </summary>
        /// <param name="doFemdZelleInvalidate">bei verlinkten Zellen wird der verlinkung geprüft und erneuert.</param>
        /// <param name="fullCheck">Runden, Großschreibung, etc. wird ebenfalls durchgefphrt</param>
        /// <param name="tryforsceonds"></param>
        /// <returns></returns>
        public (bool didSuccesfullyCheck, string error, BlueScript.Script script) DoAutomatic(bool doFemdZelleInvalidate, bool fullCheck, float tryforsceonds, string startroutine) {
            if (Database.ReadOnly) { return (false, "Automatische Prozesse nicht möglich, da die Datenbank schreibgeschützt ist", null); }

            var t = DateTime.Now;
            do {
                var erg = DoAutomatic(doFemdZelleInvalidate, fullCheck, false, startroutine);
                if (erg.didSuccesfullyCheck) { return erg; }

                if (DateTime.Now.Subtract(t).TotalSeconds > tryforsceonds) { return erg; }
            } while (true);
        }

        /// <summary>
        /// Führt Regeln aus, löst Ereignisses, setzt SysCorrect und auch die initalwerte der Zellen.
        /// Z.b: Runden, Großschreibung wird nur bei einem FullCheck korrigiert, das wird normalerweise vor dem Setzen bei CellSet bereits korrigiert.
        /// </summary>
        /// <param name="doFemdZelleInvalidate">bei verlinkten Zellen wird der verlinkung geprüft und erneuert.</param>
        /// <param name="fullCheck">Runden, Großschreibung, etc. wird ebenfalls durchgeführt</param>
        public (bool didSuccesfullyCheck, string error, BlueScript.Script skript) DoAutomatic(bool doFemdZelleInvalidate, bool fullCheck, bool onlyTest, string startroutine) {

            if (Database.ReadOnly) { return (false, "Automatische Prozesse nicht möglich, da die Datenbank schreibgeschützt ist", null); }

            var feh = Database.ErrorReason(enErrorReason.EditAcut);

            if (!string.IsNullOrEmpty(feh)) { return (false, feh, null); }

            // Zuerst die Aktionen ausführen und falls es einen Fehler gibt, die Spalten und Fehler auch ermitteln
            var script = DoRules(onlyTest, startroutine);

            if (onlyTest) { return (true, string.Empty, script); }

            /// didSuccesfullyCheck geht von Dateisystemfehlern aus
            if (!string.IsNullOrEmpty(script.Error)) { return (true, "<b>Das Skript ist fehlerhaft:</b>\r\n" + "Zeile: " + script.Line.ToString() + "\r\n" + script.Error + "\r\n" + script.ErrorCode, script); }


            // Dann die Abschließenden Korrekturen vornehmen
            foreach (var ThisColum in Database.Column) {
                if (ThisColum != null) {

                    if (fullCheck) {
                        var x = CellGetString(ThisColum);
                        var x2 = ThisColum.AutoCorrect(x);

                        if (ThisColum.Format != enDataFormat.LinkedCell && x != x2) {
                            Database.Cell.Set(ThisColum, this, x2);
                        } else {
                            if (!ThisColum.IsFirst()) {

                                Database.Cell.DoSpecialFormats(ThisColum, Key, CellGetString(ThisColum), false);
                            }
                        }
                        CellCollection.Invalidate_CellContentSize(ThisColum, this);
                        ThisColum.Invalidate_TmpColumnContentWidth();
                        doFemdZelleInvalidate = false; // Hier ja schon bei jedem gemacht
                    }

                    if (doFemdZelleInvalidate && ThisColum.LinkedDatabase() != null) {
                        CellCollection.Invalidate_CellContentSize(ThisColum, this);
                        ThisColum.Invalidate_TmpColumnContentWidth();
                    }
                }
            }

            var cols = new List<string>();
            //var _Info = new List<string>();
            var _InfoTXT = "<b><u>" + CellGetString(Database.Column[0]) + "</b></u><br><br>";
            foreach (var thisc in Database.Column) {

                var n = thisc.Name + "_error";

                var va = script.Variablen.GetSystem(n);
                if (va != null) {
                    cols.Add(thisc.Name + "|" + va.ValueString);
                    _InfoTXT = _InfoTXT + "<b>" + thisc.ReadableText() + ":</b> " + va.ValueString + "<br><hr><br>";
                }
            }

            if (cols.Count == 0) {
                _InfoTXT += "Diese Zeile ist fehlerfrei.";
            }


            if (Database.Column.SysCorrect.SaveContent) {
                if (IsNullOrEmpty(Database.Column.SysCorrect) || cols.Count == 0 != CellGetBoolean(Database.Column.SysCorrect)) { CellSet(Database.Column.SysCorrect, cols.Count == 0); }
            }
            OnRowChecked(new RowCheckedEventArgs(this, cols));

            return (true, _InfoTXT, script);
        }

        public bool MatchesTo(FilterItem Filter) {
            if (Filter != null) {

                if (Filter.FilterType == enFilterType.KeinFilter || Filter.FilterType == enFilterType.GroßKleinEgal) { return true; } // Filter ohne Funktion

                if (Filter.Column == null) {
                    if (!Convert.ToBoolean(Filter.FilterType & enFilterType.GroßKleinEgal)) { Filter.FilterType |= enFilterType.GroßKleinEgal; }

                    if (Filter.FilterType != enFilterType.Instr_GroßKleinEgal && Filter.FilterType != enFilterType.Instr_UND_GroßKleinEgal) { Develop.DebugPrint(enFehlerArt.Fehler, "Zeilenfilter nur mit Instr möglich!"); }
                    if (Filter.SearchValue.Count < 1) { Develop.DebugPrint(enFehlerArt.Fehler, "Zeilenfilter nur mit mindestens einem Wert möglich"); }

                    foreach (var t in Filter.SearchValue) {
                        if (!RowFilterMatch(t)) { return false; }
                    }
                } else {
                    if (!Database.Cell.MatchesTo(Filter.Column, this, Filter)) { return false; }
                }
            }

            return true;
        }

        public bool MatchesTo(List<FilterItem> filter) {
            if (Database == null) { return false; }

            if (filter == null || filter.Count == 0) { return true; }

            foreach (var ThisFilter in filter) {
                if (!MatchesTo(ThisFilter)) { return false; }
            }

            return true;
        }

        private bool RowFilterMatch(string searchText) {
            if (string.IsNullOrEmpty(searchText)) { return true; }

            searchText = searchText.ToUpper();

            foreach (var ThisColumnItem in Database.Column) {
                {
                    if (!ThisColumnItem.IgnoreAtRowFilter) {
                        var _String = CellGetString(ThisColumnItem);
                        _String = LanguageTool.ColumnReplace(_String, ThisColumnItem, enShortenStyle.Both);
                        if (!string.IsNullOrEmpty(_String) && _String.ToUpper().Contains(searchText)) { return true; }
                    }
                }

            }
            return false;
        }

        public bool IsNullOrEmpty() {

            foreach (var ThisColumnItem in Database.Column) {
                if (ThisColumnItem != null) {
                    if (!CellIsNullOrEmpty(ThisColumnItem)) { return false; }
                }
            }

            return true;
        }


        public bool IsNullOrEmpty(ColumnItem column) {
            return Database.Cell.IsNullOrEmpty(column, this);
        }

        public bool IsNullOrEmpty(string columnName) {
            return Database.Cell.IsNullOrEmpty(Database.Column[columnName], this);
        }

        public bool CellIsNullOrEmpty(string columnName) {
            return Database.Cell.IsNullOrEmpty(Database.Column[columnName], this);
        }
        public bool CellIsNullOrEmpty(ColumnItem column) {
            return Database.Cell.IsNullOrEmpty(column, this);
        }

        internal void OnRowChecked(RowCheckedEventArgs e) {
            RowChecked?.Invoke(this, e);
        }

        internal void OnDoSpecialRules(DoRowAutomaticEventArgs e) {
            DoSpecialRules?.Invoke(this, e);
        }

        /// <summary>
        /// Ersetzt Spaltennamen mit dem dementsprechenden Wert der Zelle. Format: &Spaltenname; oder &Spaltenname(L,8);
        /// </summary>
        /// <param name="formel"></param>
        /// <param name="fulltext">Bei TRUE wird der Text so zurückgegeben, wie er in der Zelle angezeigt werden würde: Mit Suffix und Ersetzungen. Zeilenumbrüche werden eleminiert!</param>
        /// <returns></returns>
        public string ReplaceVariables(string formel, bool fulltext, bool removeLineBreaks) {
            var erg = formel;

            // Variablen ersetzen
            foreach (var thisColumnItem in Database.Column) {
                if (!erg.Contains("&")) { return erg; }

                if (thisColumnItem != null) {
                    var txt = CellGetString(thisColumnItem);

                    if (fulltext) { txt = CellItem.ValueReadable(thisColumnItem, txt, enShortenStyle.Replaced, enBildTextVerhalten.Nur_Text, removeLineBreaks); }


                    if (removeLineBreaks && !fulltext) {
                        txt = txt.Replace("\r\n", " ");
                        txt = txt.Replace("\r", " ");
                    }

                    erg = erg.Replace("&" + thisColumnItem.Name.ToUpper() + ";", txt, RegexOptions.IgnoreCase);


                    while (erg.ToUpper().Contains("&" + thisColumnItem.Name.ToUpper() + "(")) {
                        var x = erg.ToUpper().IndexOf("&" + thisColumnItem.Name.ToUpper() + "(");

                        var x2 = erg.IndexOf(")", x);
                        if (x2 < x) { return erg; }

                        var ww = erg.Substring(x + thisColumnItem.Name.Length + 2, x2 - x - thisColumnItem.Name.Length - 2);
                        ww = ww.Replace(" ", string.Empty).ToUpper();
                        var vals = ww.SplitBy(",");
                        if (vals.Length != 2) { return formel; }
                        if (vals[0] != "L") { return formel; }
                        if (!int.TryParse(vals[1], out var Stellen)) { return formel; }

                        var newW = txt.Substring(0, Math.Min(Stellen, txt.Length));
                        erg = erg.Replace(erg.Substring(x, x2 - x + 1), newW);
                    }
                }
            }

            return erg;
        }

        /// <summary>
        /// Erstellt einen Sortierfähigen String eine Zeile
        /// </summary>
        /// <param name="columns">Nur diese Spalten in deser Reihenfolge werden berücksichtigt</param>
        /// <returns>Den String mit dem abschluß <<>key<>> und dessen Key.</returns>
        public string CompareKey(List<ColumnItem> columns) {
            var r = new StringBuilder();

            if (columns != null) {
                foreach (var t in columns) {
                    if (t != null) {
                        r.Append(Database.Cell.CompareKey(t, this) + Constants.FirstSortChar);
                    }
                }
            }

            r.Append(Constants.SecondSortChar + "<key>" + Key);
            return r.ToString();
        }

        public string CaptionReadable() {
            var c = CellGetString(Database.Column.SysChapter);

            if (string.IsNullOrEmpty(c)) {
                return "- ohne " + Database.Column.SysChapter.Caption + " -";
            } else {
                return c.Replace("\r", ", ");
            }

        }
    }
}