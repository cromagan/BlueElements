#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;

namespace BlueDatabase
{
    public sealed class RowItem : ICanBeEmpty
    {

        #region  Variablen-Deklarationen 

        public readonly Database Database;
        public string TMP_Chapter;
        public int? TMP_Y = null;
        public int? TMP_DrawHeight = null;

        #endregion

        public event EventHandler<RowCheckedEventArgs> RowChecked;
        public event EventHandler<DoRowAutomaticEventArgs> DoSpecialRules;


        #region  Construktor + Initialize 

        private void Initialize()
        {
            TMP_Chapter = string.Empty;
            TMP_Y = null;
            TMP_DrawHeight = null;
        }


        public RowItem(Database cDatabase, int Key)
        {
            Database = cDatabase;
            this.Key = Key;
            Initialize();
        }


        public RowItem(Database cDatabase)
        {
            Database = cDatabase;
            Key = Database.Row.NextRowKey();
            Initialize();
        }

        #endregion


        #region  Properties 
        public int Key { get; }

        #endregion



        #region Cell Get / Set

        #region bool
        public bool CellGetBoolean(string columnName)
        {
            return Database.Cell.GetBoolean(Database.Column[columnName], this);
        }
        public bool CellGetBoolean(ColumnItem column)
        {
            return Database.Cell.GetBoolean(column, this);
        }

        public void CellSet(string columnName, bool value)
        {
            Database.Cell.Set(Database.Column[columnName], this, value, false);
        }
        public void CellSet(string columnName, bool value, bool freezeMode)
        {
            Database.Cell.Set(Database.Column[columnName], this, value, freezeMode);
        }
        public void CellSet(ColumnItem column, bool value)
        {
            Database.Cell.Set(column, this, value, false);
        }
        public void CellSet(ColumnItem column, bool value, bool freezeMode)
        {
            Database.Cell.Set(column, this, value, freezeMode);
        }
        #endregion

        #region string
        public string CellFirstString()
        {
            return Database.Cell.GetString(Database.Column[0], this);
        }
        public string CellGetString(string columnName)
        {
            return Database.Cell.GetString(Database.Column[columnName], this);
        }
        public string CellGetString(ColumnItem column)
        {
            return Database.Cell.GetString(column, this);
        }

        public void CellSet(string columnName, string value)
        {
            Database.Cell.Set(Database.Column[columnName], this, value, false);
        }
        public void CellSet(string columnName, string value, bool freezeMode)
        {
            Database.Cell.Set(Database.Column[columnName], this, value, freezeMode);
        }
        public void CellSet(ColumnItem column, string value)
        {
            Database.Cell.Set(column, this, value, false);
        }
        public void CellSet(ColumnItem column, string value, bool freezeMode)
        {
            Database.Cell.Set(column, this, value, freezeMode);
        }
        #endregion

        #region double
        public double CellGetDouble(string columnName)
        {
            return Database.Cell.GetDouble(Database.Column[columnName], this);
        }
        public double CellGetDouble(ColumnItem column)
        {
            return Database.Cell.GetDouble(column, this);
        }


        public void CellSet(string columnName, double value)
        {
            Database.Cell.Set(Database.Column[columnName], this, value, false);
        }
        public void CellSet(string columnName, double value, bool freezeMode)
        {
            Database.Cell.Set(Database.Column[columnName], this, value, freezeMode);
        }
        public void CellSet(ColumnItem column, double value)
        {
            Database.Cell.Set(column, this, value, false);
        }
        public void CellSet(ColumnItem column, double value, bool freezeMode)
        {
            Database.Cell.Set(column, this, value, freezeMode);
        }
        #endregion

        #region decimal
        public decimal CellGetDecimal(string columnName)
        {
            return Database.Cell.GetDecimal(Database.Column[columnName], this);
        }
        public decimal CellGetDecimal(ColumnItem column)
        {
            return Database.Cell.GetDecimal(column, this);
        }


        public void CellSet(string columnName, decimal value)
        {
            Database.Cell.Set(Database.Column[columnName], this, value, false);
        }
        public void CellSet(string columnName, decimal value, bool freezeMode)
        {
            Database.Cell.Set(Database.Column[columnName], this, value, freezeMode);
        }
        public void CellSet(ColumnItem column, decimal value)
        {
            Database.Cell.Set(column, this, value, false);
        }
        public void CellSet(ColumnItem column, decimal value, bool freezeMode)
        {
            Database.Cell.Set(column, this, value, freezeMode);
        }
        #endregion

        #region int
        public int CellGetInteger(string columnName)
        {
            return Database.Cell.GetInteger(Database.Column[columnName], this);
        }
        public int CellGetInteger(ColumnItem column)
        {
            return Database.Cell.GetInteger(column, this);
        }

        public void CellSet(string columnName, int value)
        {
            Database.Cell.Set(Database.Column[columnName], this, value, false);
        }
        public void CellSet(string columnName, int value, bool freezeMode)
        {
            Database.Cell.Set(Database.Column[columnName], this, value, freezeMode);
        }
        public void CellSet(ColumnItem column, int value)
        {
            Database.Cell.Set(column, this, value, false);
        }
        public void CellSet(ColumnItem column, int value, bool freezeMode)
        {
            Database.Cell.Set(column, this, value, freezeMode);
        }
        #endregion

        #region Point
        public Point CellGetPoint(string columnName)
        {
            return Database.Cell.GetPoint(Database.Column[columnName], this);
        }
        public Point CellGetPoint(ColumnItem column)
        {
            return Database.Cell.GetPoint(column, this);
        }


        public void CellSet(string columnName, Point value)
        {
            Database.Cell.Set(Database.Column[columnName], this, value, false);
        }
        public void CellSet(string columnName, Point value, bool freezeMode)
        {
            Database.Cell.Set(Database.Column[columnName], this, value, freezeMode);
        }
        public void CellSet(ColumnItem column, Point value)
        {
            Database.Cell.Set(column, this, value, false);
        }
        public void CellSet(ColumnItem column, Point value, bool freezeMode)
        {
            Database.Cell.Set(column, this, value, freezeMode);
        }
        #endregion

        #region List<string>
        public List<string> CellGetList(string columnName)
        {
            return Database.Cell.GetList(Database.Column[columnName], this);
        }
        public List<string> CellGetList(ColumnItem column)
        {
            return Database.Cell.GetList(column, this);
        }

        public void CellSet(string columnName, List<string> value)
        {
            Database.Cell.Set(Database.Column[columnName], this, value, false);
        }
        public void CellSet(string columnName, List<string> value, bool freezeMode)
        {
            Database.Cell.Set(Database.Column[columnName], this, value, freezeMode);
        }
        public void CellSet(ColumnItem column, List<string> value)
        {
            Database.Cell.Set(column, this, value, false);
        }
        public void CellSet(ColumnItem column, List<string> value, bool freezeMode)
        {
            Database.Cell.Set(column, this, value, freezeMode);
        }
        #endregion

        #region DateTime
        public DateTime CellGetDateTime(string columnName)
        {
            return Database.Cell.GetDateTime(Database.Column[columnName], this);
        }
        public DateTime CellGetDateTime(ColumnItem column)
        {
            return Database.Cell.GetDateTime(column, this);
        }


        public void CellSet(string columnName, DateTime value)
        {
            Database.Cell.Set(Database.Column[columnName], this, value, false);
        }
        public void CellSet(string columnName, DateTime value, bool freezeMode)
        {
            Database.Cell.Set(Database.Column[columnName], this, value, freezeMode);
        }
        public void CellSet(ColumnItem column, DateTime value)
        {
            Database.Cell.Set(column, this, value, false);
        }
        public void CellSet(ColumnItem column, DateTime value, bool freezeMode)
        {
            Database.Cell.Set(column, this, value, freezeMode);
        }
        #endregion

        #region color
        public Color CellGetColor(string columnName)
        {
            return Database.Cell.GetColor(Database.Column[columnName], this);
        }
        public Color CellGetColor(ColumnItem column)
        {
            return Database.Cell.GetColor(column, this);
        }

        //public void CellSet(string columnName, Color value)
        //{
        //    Database.Cell.Set(Database.Column[columnName], this, value, false);
        //}
        //public void CellSet(string columnName, Color value, bool freezeMode)
        //{
        //    Database.Cell.Set(Database.Column[columnName], this, value, freezeMode);
        //}
        //public void CellSet(ColumnItem column, Color value)
        //{
        //    Database.Cell.Set(column, this, value, false);
        //}
        //public void CellSet(ColumnItem column, Color value, bool freezeMode)
        //{
        //    Database.Cell.Set(column, this, value, freezeMode);
        //}




        public int CellGetColorBGR(ColumnItem column)
        {
            return Database.Cell.GetColorBGR(column, this);
        }

        #endregion


        #endregion

        public List<string> CellGetValuesReadable(ColumnItem Column, enShortenStyle style)
        {
            return Database.Cell.ValuesReadable(Column, this, style);
        }




        /// <summary>
        /// Führt alle Regeln aus und löst das Ereignis DoSpecialRules aus. Setzt ansonsten keine Änderungen, wie z.B. SysCorrect oder Runden-Befehle.
        /// </summary>
        /// <returns>Gibt Regeln, die einen Fehler verursachen zurück. z.B. SPALTE1|Die Splate darf nicht leer sein.</returns>
        private List<string> DoRules()
        {

            var DoUnfreeze = true;

            if (Database.Cell.Freezed)
            {
                DoUnfreeze = false;
            }
            else
            {
                Database.Cell.Freeze();
            }

            // Dann die Aktionen ausführen und fall es einen Fehler gibt, die Spalten ermitteln
            var ColumnAndErrors = new List<string>();

            foreach (var ThisRule in Database.Rules)
            {
                if (ThisRule != null)
                {
                    if (ThisRule.TrifftZu(this))
                    {
                        var tmpMessage = ThisRule.Execute(this, true);

                        if (!string.IsNullOrEmpty(tmpMessage))
                        {
                            var tmpColumNames = tmpMessage.ReduceToMulti("'#Spalte:*'");
                            if (tmpMessage.Contains("<DELETE>")) { tmpMessage = tmpMessage.Substring(0, tmpMessage.IndexOf("<DELETE>")); }

                            foreach (var t in tmpColumNames)
                            {
                                var Column = Database.Column[t];
                                if (Column != null) { tmpMessage = tmpMessage.Replace("#Spalte:" + Column.Name, Column.ReadableText(), RegexOptions.IgnoreCase); }
                            }


                            if (tmpColumNames.Count > 0)
                            {
                                foreach (var t in tmpColumNames)
                                {
                                    ColumnAndErrors.Add(t + "|" + tmpMessage);
                                }
                            }
                            else
                            {
                                ColumnAndErrors.Add("|" + tmpMessage); // Sie gehören zur Nutzergruppe...
                            }

                        }
                    }
                }
            }

            // Gucken, ob noch ein Fehler da ist, der von einer besonderen anderen Routine kommt. Beispiel Bildzeichen-Liste: Bandart und Einläufe
            var e = new DoRowAutomaticEventArgs(this);
            OnDoSpecialRules(e);


            if (!string.IsNullOrEmpty(e.Feedback) && e.FeedbackColumn == null) { e.FeedbackColumn = Database.Column[0]; }

            if (e.FeedbackColumn != null)
            {
                if (string.IsNullOrEmpty(e.Feedback)) { e.Feedback = "Allgemeiner Fehler in '" + e.FeedbackColumn.ReadableText() + "'."; }
                ColumnAndErrors.Add(e.FeedbackColumn.Name + "|" + e.Feedback);
            }

            if (DoUnfreeze) { Database.Cell.UnFreeze(); }

            return ColumnAndErrors.SortedDistinctList();
        }


        /// <summary>
        /// Führt Regeln aus, löst Ereignisses, setzt SysCorrect und auch die initalwerte der Zellen.
        /// Z.b: Runden, Großschreibung wird nur bei einem FullCheck korrigiert, das wird normalerweise vor dem Setzen bei CellSet bereits korrigiert.
        /// </summary>
        /// <param name="doFemdZelleInvalidate">bei verlinkten Zellen wird der verlinkung geprüft und erneuert.</param>
        /// <param name="fullCheck">Runden, Großschreibung, etc. wird ebenfalls durchgefphrt</param>
        /// <param name="tryforsceonds"></param>
        /// <returns></returns>
        public Tuple<bool, string> DoAutomatic(bool doFemdZelleInvalidate, bool fullCheck, float tryforsceonds)
        {

            if (Database.ReadOnly) { return new Tuple<bool, string>(false, "Automatische Prozesse nicht möglich, da die Datenbank schreibgeschützt ist"); }


            var t = DateTime.Now;
            do
            {
                var erg = DoAutomatic(doFemdZelleInvalidate, fullCheck);
                if (erg.Item1) { return erg; }

                if (DateTime.Now.Subtract(t).TotalSeconds > tryforsceonds) { return erg; }
            } while (true);
        }

        /// <summary>
        /// Führt Regeln aus, löst Ereignisses, setzt SysCorrect und auch die initalwerte der Zellen.
        /// Z.b: Runden, Großschreibung wird nur bei einem FullCheck korrigiert, das wird normalerweise vor dem Setzen bei CellSet bereits korrigiert.
        /// </summary>
        /// <param name="doFemdZelleInvalidate">bei verlinkten Zellen wird der verlinkung geprüft und erneuert.</param>
        /// <param name="fullCheck">Runden, Großschreibung, etc. wird ebenfalls durchgefphrt</param>
        public Tuple<bool, string> DoAutomatic(bool doFemdZelleInvalidate, bool fullCheck)
        {

            if (Database.ReadOnly) { return new Tuple<bool, string>(false, "Automatische Prozesse nicht möglich, da die Datenbank schreibgeschützt ist"); }

            var feh = Database.ErrorReason(enErrorReason.EditAcut);

            if (!string.IsNullOrEmpty(feh))
            {
                //Develop.DebugPrint(enFehlerArt.Warnung, "Automatik nicht möglich: " + feh);
                return new Tuple<bool, string>(false, feh);
            }

            // Zuerst die Aktionen ausführen und falls es einen Fehler gibt, die Spalten und Fehler auch ermitteln
            var cols = DoRules();

            // Dann die Abschließenden Korrekturen vornehmen
            foreach (var ThisColum in Database.Column)
            {
                if (ThisColum != null)
                {

                    if (fullCheck)
                    {
                        var x = CellGetString(ThisColum);
                        var x2 = ThisColum.AutoCorrect(x);

                        if (ThisColum.Format != enDataFormat.LinkedCell && x != x2)
                        {
                            Database.Cell.Set(ThisColum, this, x2, false);
                        }
                        else
                        {
                            if (!ThisColum.IsFirst())
                            {

                                Database.Cell.DoSpecialFormats(ThisColum, Key, CellGetString(ThisColum), false, true);
                            }
                        }
                        CellCollection.Invalidate_CellContentSize(ThisColum, this);
                        ThisColum.Invalidate_TmpColumnContentWidth();
                        doFemdZelleInvalidate = false; // Hier ja schon bei jedem gemacht
                    }

                    if (doFemdZelleInvalidate && ThisColum.LinkedDatabase() != null)
                    {
                        CellCollection.Invalidate_CellContentSize(ThisColum, this);
                        ThisColum.Invalidate_TmpColumnContentWidth();
                    }
                }
            }

            TMP_Y = null;
            TMP_DrawHeight = null;


            if (Database.Column.SysCorrect.SaveContent)
            {
                if (IsNullOrEmpty(Database.Column.SysCorrect) || Convert.ToBoolean(cols.Count == 0) != CellGetBoolean(Database.Column.SysCorrect)) { CellSet(Database.Column.SysCorrect, Convert.ToBoolean(cols.Count == 0)); }
            }
            OnRowChecked(new RowCheckedEventArgs(this, cols));


            var _Info = new List<string>();

            foreach (var ThisString in cols)
            {
                var X = ThisString.SplitBy("|");
                _Info.AddIfNotExists(X[1]);
            }

            var _InfoTXT = "<b><u>" + CellGetString(Database.Column[0]) + "</b></u><br><br>";

            if (cols.Count == 0)
            {
                _InfoTXT += "Diese Zeile ist fehlerfrei.";
            }
            else
            {
                _InfoTXT += _Info.JoinWith("<br><hr><br>");
            }

            //MessageBox.Show(_InfoTXT, enImageCode.Information, "OK");

            return new Tuple<bool, string>(true, _InfoTXT); ;

        }



        ///// <summary>
        ///// Überprüft auf ungültige Werte in einer Zelle und korrigiert diese. Es werden keine Regeln ausgelöst.
        ///// </summary>
        //internal void Repair()
        //{
        //    if (Database.Column.SysCorrect == null) { Database.Column.GetSystems(); }

        //    if (Key < 0) { Develop.DebugPrint(enFehlerArt.Fehler, "Key < 0"); }

        //    if (Database.ReadOnly) { return; }

        //    if (CellIsNullOrEmpty(Database.Column.SysLocked))
        //    {
        //        Database.Cell.SystemSet(Database.Column.SysLocked, this, false.ToPlusMinus(), false);
        //    }


        //    if (CellIsNullOrEmpty(Database.Column.SysCorrect))
        //    {
        //        Database.Cell.SystemSet(Database.Column.SysCorrect, this, true.ToPlusMinus(), false);
        //    }


        //    if (CellIsNullOrEmpty(Database.Column.SysRowChangeDate))
        //    {
        //        Database.Cell.SystemSet(Database.Column.SysRowChangeDate, this, DateTime.Now.ToString(Constants.Format_Date5), false);
        //    }
        //}

        public bool MatchesTo(FilterItem Filter)
        {
            if (Filter != null)
            {


                if (Filter.FilterType == enFilterType.KeinFilter || Filter.FilterType == enFilterType.GroßKleinEgal) { return true; } // Filter ohne Funktion

                if (Filter.Column == null)
                {
                    if (!Convert.ToBoolean(Filter.FilterType & enFilterType.GroßKleinEgal)) { Filter.FilterType |= enFilterType.GroßKleinEgal; }

                    if (Filter.FilterType != enFilterType.Instr_GroßKleinEgal && Filter.FilterType != enFilterType.Instr_UND_GroßKleinEgal) { Develop.DebugPrint(enFehlerArt.Fehler, "Zeilenfilter nur mit Instr möglich!"); }
                    if (Filter.SearchValue.Count < 1) { Develop.DebugPrint(enFehlerArt.Fehler, "Zeilenfilter nur mit mindestens einem Wert möglich"); }

                    foreach (var t in Filter.SearchValue)
                    {
                        if (!RowFilterMatch(t)) { return false; }
                    }
                }
                else
                {
                    if (!Database.Cell.MatchesTo(Filter.Column, this, Filter)) { return false; }
                }
            }

            return true;
        }

        public bool MatchesTo(List<FilterItem> filter)
        {
            if (Database == null) { return false; }

            if (filter == null || filter.Count == 0) { return true; }

            foreach (var ThisFilter in filter)
            {
                if (!MatchesTo(ThisFilter)) { return false; }
            }

            return true;
        }






        private bool RowFilterMatch(string searchText)
        {
            if (string.IsNullOrEmpty(searchText)) { return true; }

            searchText = searchText.ToUpper();

            foreach (var ThisColumnItem in Database.Column)
            {
                {
                    if (!ThisColumnItem.IgnoreAtRowFilter)
                    {
                        var _String = CellGetString(ThisColumnItem);
                        _String = LanguageTool.ColumnReplace(_String, ThisColumnItem, enShortenStyle.Both, false);
                        if (!string.IsNullOrEmpty(_String) && _String.ToUpper().Contains(searchText)) { return true; }
                    }
                }

            }
            return false;
        }


        public bool IsNullOrEmpty()
        {

            foreach (var ThisColumnItem in Database.Column)
            {
                if (ThisColumnItem != null)
                {
                    if (!CellIsNullOrEmpty(ThisColumnItem)) { return false; }
                }
            }

            return true;
        }


        public bool IsNullOrEmpty(ColumnItem column)
        {
            return Database.Cell.IsNullOrEmpty(column, this);
        }

        public bool IsNullOrEmpty(string columnName)
        {
            return Database.Cell.IsNullOrEmpty(Database.Column[columnName], this);
        }





        public bool CellIsNullOrEmpty(string columnName)
        {
            return Database.Cell.IsNullOrEmpty(Database.Column[columnName], this);
        }
        public bool CellIsNullOrEmpty(ColumnItem column)
        {
            return Database.Cell.IsNullOrEmpty(column, this);
        }





        internal void OnRowChecked(RowCheckedEventArgs e)
        {
            RowChecked?.Invoke(this, e);
        }


        internal void OnDoSpecialRules(DoRowAutomaticEventArgs e)
        {
            DoSpecialRules?.Invoke(this, e);
        }


        /// <summary>
        /// Ersetzt Spaltennamen mit dem dementsprechenden Wert der Zelle. Format: &Spaltenname; Leere ZEllen werden mit 0 ersetzt.
        /// </summary>
        /// <param name="formel"></param>
        /// <returns></returns>
        public string ReplaceVariablesForMath(string formel)
        {

            // Variablen ersetzen
            foreach (var thisColumnItem in Database.Column)
            {
                if (thisColumnItem != null)
                {
                    var w = "0";
                    if (!CellIsNullOrEmpty(thisColumnItem)) { w = CellGetString(thisColumnItem); }
                    formel = formel.Replace("&" + thisColumnItem.Name.ToUpper() + ";", w);
                }

            }

            return formel;
        }

        /// <summary>
        /// Ersetzt Spaltennamen mit dem dementsprechenden Wert der Zelle. Format: &Spaltenname; oder &Spaltenname(L,8);
        /// </summary>
        /// <param name="formel"></param>
        /// <returns></returns>
        public string ReplaceVariables(string formel)
        {

            var erg = formel;

            // Variablen ersetzen
            foreach (var thisColumnItem in Database.Column)
            {
                if (thisColumnItem != null)
                {
                    var w = CellGetString(thisColumnItem);
                    erg = erg.Replace("&" + thisColumnItem.Name.ToUpper() + ";", w, RegexOptions.IgnoreCase);


                    while (erg.ToUpper().Contains("&" + thisColumnItem.Name.ToUpper() + "("))
                    {
                        var x = erg.ToUpper().IndexOf("&" + thisColumnItem.Name.ToUpper() + "(");

                        var x2 = erg.IndexOf(")", x);
                        if (x2 < x) { return erg; }

                        var ww = erg.Substring(x + thisColumnItem.Name.Length + 2, x2 - x - thisColumnItem.Name.Length - 2);
                        ww = ww.Replace(" ", string.Empty).ToUpper();
                        var vals = ww.SplitBy(",");
                        if (vals.Length != 2) { return formel; }
                        if (vals[0] != "L") { return formel; }
                        if (!int.TryParse(vals[1], out var Stellen)) { return formel; }

                        var newW = w.Substring(0, Math.Min(Stellen, w.Length));
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
        public string CompareKey(List<ColumnItem> columns)
        {
            var r = new StringBuilder();

            if (columns != null)
            {
                foreach (var t in columns)
                {
                    if (t != null)
                    {
                        r.Append(Database.Cell.CompareKey(t, this) + Constants.FirstSortChar);
                    }
                }
            }

            r.Append(Constants.SecondSortChar + "<key>" + Key);
            return r.ToString();
        }

    }
}
