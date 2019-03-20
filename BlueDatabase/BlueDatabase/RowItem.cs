#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2019 Christian Peter
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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueDatabase.EventArgs;
using System.Text.RegularExpressions;
using BlueDatabase.Enums;

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

        public string CellFirstString()
        {
            return Database.Cell.GetString(0, this);
        }

        public List<string> CellGetList(string ColumnName)
        {
            return Database.Cell.GetList(Database.Column[ColumnName], this);
        }

        public string CellBestFile(string ColumnName)
        {
            return Database.Cell.BestFile(Database.Column[ColumnName], this);
        }

        #endregion


        #region  Properties 
        public int Key { get; }

        #endregion











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
                    if (ThisRule.TrifftZu(this, null))
                    {
                        var tmpMessage = ThisRule.Execute(this, null, true);

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
        /// Z.b: Runden, Großschreibung wird nicht korrigiert, das wird vor dem Setzen bei CellSet bereits korrigiert.
        /// </summary>
        /// <param name="IsNewRow"></param>
        /// <param name="DoFemdZelleInvalidate"></param>
        /// <param name="ShowMessageBox"></param>
        public string DoAutomatic(bool IsNewRow, bool DoFemdZelleInvalidate)
        {
            // Zuerst die Aktionen ausführen und falls es einen Fehler gibt, die Spalten und Fehler auch ermitteln
            var cols = DoRules();


            // Dann die Abschließenden Korrekturen vornehmen
            foreach (var ThisColum in Database.Column)
            {
                if (ThisColum != null)
                {
                    if (IsNewRow && !string.IsNullOrEmpty(ThisColum.CellInitValue)) { CellSet(ThisColum, ThisColum.CellInitValue); }

                    if (DoFemdZelleInvalidate && ThisColum.LinkedDatabase() != null)
                    {
                        CellCollection.Invalidate_CellContentSize(ThisColum, this);
                        ThisColum.Invalidate_TmpColumnContentWidth();
                    }
                }
            }

            if (Convert.ToBoolean(cols.Count == 0) != Database.Cell.GetBoolean(Database.Column.SysCorrect, this)) { CellSet(Database.Column.SysCorrect, Convert.ToBoolean(cols.Count == 0)); }

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
                _InfoTXT = _InfoTXT + "Diese Zeile ist fehlerfrei.";
            }
            else
            {
                _InfoTXT = _InfoTXT + _Info.JoinWith("<br><hr><br>");
            }

            //MessageBox.Show(_InfoTXT, enImageCode.Information, "OK");

            return _InfoTXT;

        }



        /// <summary>
        /// Überprüft auf ungültige Werte in einer Zelle und korrigiert diese. Es werden keine Regeln ausgelöst.
        /// </summary>
        /// <param name="IsNewRow">Settzt bei True den Ersteller und das Erstelldatum.</param>
        internal void Repair(bool IsNewRow)
        {
            if (Database.Column.SysCorrect == null) { Database.Column.GetSystems(); }

            if (Key < 0) { Develop.DebugPrint(enFehlerArt.Fehler, "Key < 0"); }


            if (CellIsNullOrEmpty(Database.Column.SysLocked))
            {
                Database.Cell.SystemSet(Database.Column.SysLocked, this, false.ToPlusMinus(), false);
            }


            if (CellIsNullOrEmpty(Database.Column.SysCorrect))
            {
                Database.Cell.SystemSet(Database.Column.SysCorrect, this, true.ToPlusMinus(), false);
            }


            if (CellIsNullOrEmpty(Database.Column.SysRowChangeDate))
            {
                Database.Cell.SystemSet(Database.Column.SysRowChangeDate, this, DateTime.Now.ToString(), false);
            }


            if (IsNewRow)
            {
                Database.Cell.SystemSet(Database.Column.SysRowCreator, this, Database.UserName, false);
                Database.Cell.SystemSet(Database.Column.SysRowCreateDate, this, DateTime.Now.ToString(), false);
                DoAutomatic(true, false);
            }
        }

        public bool MatchesTo(FilterItem Filter)
        {
            if (Filter != null)
            {

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

        public bool MatchesTo(FilterCollection Filter)
        {
            if (Database == null) { return false; }

            if (Filter == null || Filter.Count() == 0) { return true; }

            foreach (var ThisFilter in Filter)
            {
                if (!MatchesTo(ThisFilter)) { return false; }
            }

            return true;
        }
        public void CellSet(ColumnItem Column, bool Value)
        {
            Database.Cell.Set(Column, this, Value);
        }
        public void CellSet(ColumnItem Column, bool Value, bool FreezeMode)
        {
            Database.Cell.Set(Column, this, Value, FreezeMode);
        }

        internal void CellSet(ColumnItem Column, double Value, bool FreezeMode)
        {
            Database.Cell.Set(Column, this, Value, FreezeMode);
        }
        internal void CellSet(ColumnItem Column, double Value)
        {
            Database.Cell.Set(Column, this, Value);
        }
        public void CellSet(ColumnItem Column, int Value, bool FreezeMode)
        {
            Database.Cell.Set(Column, this, Value, FreezeMode);
        }
        public void CellSet(ColumnItem Column, int Value)
        {
            Database.Cell.Set(Column, this, Value);
        }

        public void CellSet(string ColumnName, Point Value)
        {
            Database.Cell.Set(ColumnName, this, Value);
        }

        private bool RowFilterMatch(string Search)
        {

            if (string.IsNullOrEmpty(Search)) { return true; }

            Search = Search.ToUpper();


            foreach (var ThisColumnItem in Database.Column)
            {
                {
                    if (!ThisColumnItem.IgnoreAtRowFilter)
                    {
                        var _String = CellGetString(ThisColumnItem);
                        _String = ColumnItem.ColumnReplace(_String, ThisColumnItem, enShortenStyle.Both);
                        if (!string.IsNullOrEmpty(_String) && _String.ToUpper().Contains(Search)) { return true; }
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

        public bool IsNullOrEmpty(string ColumnName)
        {
            return Database.Cell.IsNullOrEmpty(Database.Column[ColumnName], this);
        }




        public int CellGetInteger(string ColumnName)
        {
            return Database.Cell.GetInteger(ColumnName, this);
        }

        public string CellGetString(string ColumnName)
        {
            return Database.Cell.GetString(ColumnName, this);
        }

        public string CellGetValueCompleteReadable(ColumnItem Column, enShortenStyle style)
        {
            return Database.Cell.GetValueCompleteReadable(Column, this, style);
        }


        //public string CellGetStringForExport(string ColumnName)
        //{
        //    return Database.Cell.GetStringForExport(ColumnName, this);
        //}


        public void CellSet(string ColumnName, string Value)
        {
            Database.Cell.Set(ColumnName, this, Value);
        }

        public void CellSet(string ColumnName, string Value, bool FreezeMode)
        {
            Database.Cell.Set(ColumnName, this, Value, FreezeMode);
        }

        public string[] CellGetArray(string ColumnName)
        {
            return Database.Cell.GetArray(Database.Column[ColumnName], this);
        }

        public List<string> CellGetList(ColumnItem Column)
        {
            return Database.Cell.GetList(Column, this);
        }

        public double CellGetDouble(string ColumnName)
        {
            return Database.Cell.GetDouble(Database.Column[ColumnName], this);
        }

        public bool CellGetBoolean(ColumnItem Column)
        {
            return Database.Cell.GetBoolean(Column, this);
        }

        public string CellGetString(ColumnItem Column)
        {
            return Database.Cell.GetString(Column, this);
        }

        public bool CellGetBoolean(string ColumnName)
        {
            return Database.Cell.GetBoolean(Database.Column[ColumnName], this);
        }

        public void CellSet(string ColumnName, bool Value)
        {
            Database.Cell.Set(Database.Column[ColumnName], this, Value);
        }

        public void CellSet(string ColumnName, List<string> Value)
        {
            Database.Cell.Set(Database.Column[ColumnName], this, Value);

        }

        public void CellSet(string ColumnName, List<string> Value, bool FreezeMode)
        {
            Database.Cell.Set(Database.Column[ColumnName], this, Value, FreezeMode);

        }

        public void CellSet(ColumnItem Column, List<string> Value)
        {
            Database.Cell.Set(Column, this, Value);
        }
        public void CellSet(ColumnItem Column, List<string> Value, bool FreezeMode)
        {
            Database.Cell.Set(Column, this, Value, FreezeMode);
        }
        public void CellSet(ColumnItem Column, string Value)
        {
            Database.Cell.Set(Column, this, Value);
        }

        public void CellSet(ColumnItem Column, string Value, bool FreezeMode)
        {
            Database.Cell.Set(Column, this, Value, FreezeMode);
        }

        public void CellSet(string ColumnName, DateTime Value)
        {
            Database.Cell.Set(Database.Column[ColumnName], this, Value);
        }

        public void CellSet(string ColumnName, int Value)
        {
            Database.Cell.Set(Database.Column[ColumnName], this, Value);
        }

        public bool CellIsNullOrEmpty(string ColumnName)
        {
            return Database.Cell.IsNullOrEmpty(Database.Column[ColumnName], this);
        }
        public bool CellIsNullOrEmpty(ColumnItem Column)
        {
            return Database.Cell.IsNullOrEmpty(Column, this);
        }


        public string CellGetBestFile(string ColumnName)
        {
            return Database.Cell.BestFile(Database.Column[ColumnName], this);
        }

        public string CellGetBestFile(ColumnItem Column)
        {
            return Database.Cell.BestFile(Column, this);
        }

        public double CellGetDouble(ColumnItem Column)
        {
            return Database.Cell.GetDouble(Column, this);
        }

        public decimal CellGetDecimal(string ColumnName)
        {
            return Database.Cell.GetDecimal(ColumnName, this);
        }

        public void CellSet(string ColumnName, double Value)
        {
            Database.Cell.Set(ColumnName, this, Value);
        }

        public Point CellGetPoint(ColumnItem Column)
        {
            return Database.Cell.GetPoint(Column, this);
        }
        public Point CellGetPoint(string ColumnName)
        {
            return Database.Cell.GetPoint(Database.Column[ColumnName], this);
        }


        public string CellBestFile(ColumnItem Column)
        {
            return Database.Cell.BestFile(Column, this);
        }

        public bool CellIsNullOrEmpty(int ColumnIndex)
        {
            return Database.Cell.IsNullOrEmpty(ColumnIndex, this);

        }

        public Bitmap CellGetBitmap(ColumnItem Column)
        {
            return Database.Cell.GetBitmap(Column, this);
        }

        public int CellGetColorBGR(ColumnItem Column)
        {
            return Database.Cell.GetColorBGR(Column, this);
        }

        public int CellGetInteger(ColumnItem Column)
        {
            return Database.Cell.GetInteger(Column, this);
        }

        public Color CellGetColor(ColumnItem Column)
        {
            return Database.Cell.GetColor(Column, this);
        }

        //public BlueFont CellGetBlueFont(ColumnItem Column)
        //{
        //    return Database.Cell.GetBlueFont(Column, this);
        //}

        public DateTime CellGetDate(string ColumnName)
        {
            return Database.Cell.GetDate(ColumnName, this);
        }

        public DateTime CellGetDate(ColumnItem Column)
        {
            return Database.Cell.GetDate(Column, this);
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
        /// Erstellt einen Sortierfähigen String eine Zeile
        /// </summary>
        /// <param name="vColumns">Nur diese Spalten in deser Reihenfolge werden berücksichtigt</param>
        /// <returns>Den String mit dem abschluß <<>key<>> und dessen Key.</returns>
        public string CompareKey(List<ColumnItem> vColumns)
        {
            var r = new StringBuilder();

            if (vColumns != null)
            {
                foreach (var t in vColumns)
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
