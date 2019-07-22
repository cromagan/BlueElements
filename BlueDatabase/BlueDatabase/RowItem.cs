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
        public event EventHandler<KeyChangedEventArgs> KeyChanged;


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
        public int Key { get; private set; }

        #endregion


        #region +++ get / set ++

        #region Get/Set String


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

        #region Get/Set Boolean


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

        #region Get/Set List<String>


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

        #region Get/Set Double


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

        #region Get/Set Integer


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

        #region Get/Set DateTime


        public DateTime CellGetDate(string columnName)
        {
            return Database.Cell.GetDate(Database.Column[columnName], this);
        }

        public DateTime CellGetDate(ColumnItem column)
        {
            return Database.Cell.GetDate(column, this);
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

        #region Get/Set Point


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

        #endregion


        /// <summary>
        /// F�hrt alle Regeln aus und l�st das Ereignis DoSpecialRules aus. Setzt ansonsten keine �nderungen, wie z.B. SysCorrect oder Runden-Befehle.
        /// </summary>
        /// <returns>Gibt Regeln, die einen Fehler verursachen zur�ck. z.B. SPALTE1|Die Splate darf nicht leer sein.</returns>
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

            // Dann die Aktionen ausf�hren und fall es einen Fehler gibt, die Spalten ermitteln
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
                                ColumnAndErrors.Add("|" + tmpMessage); // Sie geh�ren zur Nutzergruppe...
                            }

                        }
                    }
                }
            }

            // Gucken, ob noch ein Fehler da ist, der von einer besonderen anderen Routine kommt. Beispiel Bildzeichen-Liste: Bandart und Einl�ufe
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
        /// F�hrt Regeln aus, l�st Ereignisses, setzt SysCorrect und auch die initalwerte der Zellen.
        /// Z.b: Runden, Gro�schreibung wird nur bei einem FullCheck korrigiert, das wird normalerweise vor dem Setzen bei CellSet bereits korrigiert.
        /// </summary>
        /// <param name="IsNewRow"></param>
        /// <param name="DoFemdZelleInvalidate"></param>
        public string DoAutomatic(bool DoFemdZelleInvalidate, bool FullCheck)
        {
            // Zuerst die Aktionen ausf�hren und falls es einen Fehler gibt, die Spalten und Fehler auch ermitteln
            var cols = DoRules();

            // Dann die Abschlie�enden Korrekturen vornehmen
            foreach (var ThisColum in Database.Column)
            {
                if (ThisColum != null)
                {

                    if (FullCheck)
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
                        DoFemdZelleInvalidate = false; // Hier ja schon bei jedem gemacht
                    }

                    if (DoFemdZelleInvalidate && ThisColum.LinkedDatabase() != null)
                    {
                        CellCollection.Invalidate_CellContentSize(ThisColum, this);
                        ThisColum.Invalidate_TmpColumnContentWidth();
                    }
                }
            }

            TMP_Y = null;
            TMP_DrawHeight = null;

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
        /// �berpr�ft auf ung�ltige Werte in einer Zelle und korrigiert diese. Es werden keine Regeln ausgel�st.
        /// </summary>
        internal void Repair()
        {
            if (Database.Column.SysCorrect == null) { Database.Column.GetSystems(); }

            if (Key < 0) { Develop.DebugPrint(enFehlerArt.Fehler, "Key < 0"); }


            //if (CellIsNullOrEmpty(Database.Column.SysLocked))
            //{
            //    Database.Cell.Set(Database.Column.SysLocked, this, false.ToPlusMinus(), false);
            //}


            //if (CellIsNullOrEmpty(Database.Column.SysCorrect))
            //{
            //    Database.Cell.Set(Database.Column.SysCorrect, this, true.ToPlusMinus(), false);
            //}


            //if (CellIsNullOrEmpty(Database.Column.SysRowChangeDate))
            //{
            //    Database.Cell.Set(Database.Column.SysRowChangeDate, this, DateTime.Now.ToString(), false);
            //}
        }

        internal void ChangeKeyTo(int newKey)
        {
            if (newKey == Key) { return; }
            var Ok = Key;

            Key = newKey;
            OnKeyChanged(Ok,newKey);

        }

        private void OnKeyChanged(int ok, int newKey)
        {
            KeyChanged?.Invoke(this, new KeyChangedEventArgs(ok, newKey));
        }

        public bool MatchesTo(FilterItem Filter)
        {
            if (Filter != null)
            {

                if (Filter.Column == null)
                {
                    if (!Convert.ToBoolean(Filter.FilterType & enFilterType.Gro�KleinEgal)) { Filter.FilterType |= enFilterType.Gro�KleinEgal; }

                    if (Filter.FilterType != enFilterType.Instr_Gro�KleinEgal && Filter.FilterType != enFilterType.Instr_UND_Gro�KleinEgal) { Develop.DebugPrint(enFehlerArt.Fehler, "Zeilenfilter nur mit Instr m�glich!"); }
                    if (Filter.SearchValue.Count < 1) { Develop.DebugPrint(enFehlerArt.Fehler, "Zeilenfilter nur mit mindestens einem Wert m�glich"); }

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

        public bool MatchesTo(FilterCollection filter)
        {
            if (Database == null) { return false; }

            if (filter == null || filter.Count() == 0) { return true; }

            foreach (var ThisFilter in filter)
            {
                if (!MatchesTo(ThisFilter)) { return false; }
            }

            return true;
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
                        _String = LanguageTool.ColumnReplace(_String, ThisColumnItem, enShortenStyle.Both);
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
            return CellCollection.IsNullOrEmpty(Database.Column[ColumnName], this);
        }

        public string CellFirstString()
        {
            return Database.Cell.GetString(Database.Column[0], this);
        }





















        public bool CellIsNullOrEmpty(string ColumnName)
        {
            return CellCollection.IsNullOrEmpty(Database.Column[ColumnName], this);
        }
        public bool CellIsNullOrEmpty(ColumnItem Column)
        {
            return CellCollection.IsNullOrEmpty(Column, this);
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
        /// Erstellt einen Sortierf�higen String eine Zeile
        /// </summary>
        /// <param name="vColumns">Nur diese Spalten in deser Reihenfolge werden ber�cksichtigt</param>
        /// <returns>Den String mit dem abschlu� <<>key<>> und dessen Key.</returns>
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
