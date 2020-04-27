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

using System;
using System.Collections;
using System.Collections.Generic;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;

namespace BlueDatabase
{
    public sealed class FilterCollection : ListExt<FilterItem>, IParseable
    {
        #region  Variablen-Deklarationen 

        //private readonly ListExt<FilterItem> _Internal = new ListExt<FilterItem>();
        public readonly Database Database;

        #endregion


        public bool IsParsing { get; private set; }

        #region  Event-Deklarationen + Delegaten 
        public event EventHandler Changed;
        #endregion


        #region  Construktor + Initialize 


        public FilterCollection(Database database)
        {
            Database = database;
        }

        public FilterCollection(Database database, string filterCodes)
        {
            Database = database;

            Parse(filterCodes);
        }

        #endregion



        protected override void OnListOrItemChanged()
        {
            base.OnListOrItemChanged();
            OnChanged();
        }







        public void Delete(ColumnItem column)
        {

            var Again = true;

            while (Again)
            {
                Again = false;


                foreach (var ThisFilterItem in this)
                {
                    if (ThisFilterItem != null)
                    {
                        if (ThisFilterItem.Column == column)
                        {
                            Remove(ThisFilterItem);
                            Again = true;
                            break;
                        }
                    }
                }

            }

        }

        public void Delete(string ColumnName)
        {

            var tmp = Database.Column[ColumnName];
            if (tmp == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Spalte '" + ColumnName + "' nicht vorhanden."); }

            Delete(tmp);
        }

        public void Delete_RowFilter()
        {
            Delete((ColumnItem)null);
        }



        public void Add(enFilterType filterType, string filterBy)
        {
            Add(new FilterItem(Database, filterType, filterBy));
        }

        public void Add(enFilterType filterType, List<string> filterBy)
        {
            Add(new FilterItem(Database, filterType, filterBy));
        }



        public void Add(string columnName, enFilterType filterType, string filterBy)
        {
            Add(Database.Column[columnName], filterType, filterBy);
        }

        public void Add(string columnName, enFilterType filterType, List<string> filterBy)
        {

            Add(Database.Column[columnName], filterType, filterBy);
        }

        public void Add(ColumnItem column, enFilterType filterType, List<string> filterBy)
        {
            Add(new FilterItem(column, filterType, filterBy));
        }


        public void Add(ColumnItem column, enFilterType filterType, string filterBy)
        {
            Add(new FilterItem(column, filterType, filterBy));
        }


        protected override void OnItemAdded(FilterItem item)
        {
            base.OnItemAdded(item);

            if (item.FilterType == enFilterType.KeinFilter)
            {
                Develop.DebugPrint("Kein Filter!");
                return;
            }



            foreach (var ThisFilterItem in this)
            {
                if (ThisFilterItem.Column == item.Column && item != ThisFilterItem)
                {
                    Develop.DebugPrint(enFehlerArt.Warnung, "Doppelter Filter!");
                }
            }


            if (item.SearchValue != null && item.SearchValue.Count >1)
            {
                if (!item.FilterType.HasFlag(enFilterType.UND ) && !item.FilterType.HasFlag(enFilterType.ODER))
                {
                    Develop.DebugPrint(enFehlerArt.Warnung, "UND/ODER fehlt");
                }
            }

            if (item.FilterType.HasFlag(enFilterType.UND) && item.FilterType.HasFlag(enFilterType.ODER))
            {
                Develop.DebugPrint(enFehlerArt.Warnung, "UND/ODER zu viel");
            }


            //var Oder1 = false;
            //var Und1 = false;
            //var TMPFilter1 = filterType;

                //var Oder2 = false;
                //var Und2 = false;

                //if (Convert.ToBoolean(TMPFilter1 & enFilterType.UND))
                //{
                //    Und1 = true;
                //    TMPFilter1 = TMPFilter1 ^ enFilterType.UND;
                //}
                //if (Convert.ToBoolean(TMPFilter1 & enFilterType.ODER))
                //{
                //    Oder1 = true;
                //    TMPFilter1 = TMPFilter1 ^ enFilterType.ODER;
                //}


                //foreach (var ThisFilterItem in this)
                //{
                //    if (ThisFilterItem != null)
                //    {


                //        if (ThisFilterItem.Column == Column)
                //        {
                //            var TMPFilter2 = ThisFilterItem.filterType;
                //            if (Convert.ToBoolean(TMPFilter2 & enFilterType.UND))
                //            {
                //                Und2 = true;
                //                TMPFilter2 = TMPFilter2 ^ enFilterType.UND;
                //            }
                //            if (Convert.ToBoolean(TMPFilter2 & enFilterType.ODER))
                //            {
                //                Oder2 = true;
                //                TMPFilter2 = TMPFilter2 ^ enFilterType.ODER;
                //            }

                //            if (TMPFilter1 == TMPFilter2)
                //            {
                //                if (ThisFilterItem.SearchValue.Contains(FilterValue) && Und1 == Und2 && Oder1 == Oder2) { return; }// Filter genau so schon im gebrauch

                //                if (Und1 == false && Und2 == false && Oder1 == false && Oder2 == false) { Develop.DebugPrint(enFehlerArt.Fehler, "Unbekannte Vergleichmethode!"); }
                //                if (Und1 != Und2) { Und1 = true; }
                //                if (Oder1 != Oder2) { Oder1 = true; }
                //                if (Und1 == Oder1) { Develop.DebugPrint(enFehlerArt.Fehler, "Doppelte Vergleichmethode!"); }

                //                if (Und1) { ThisFilterItem.filterType = TMPFilter1 | enFilterType.UND; }
                //                if (Oder1) { ThisFilterItem.filterType = TMPFilter1 | enFilterType.ODER; }
                //                ThisFilterItem.SearchValue.Add(FilterValue); // KEIN Cutend, es kann ja sein, daß nach LEEREN auch gesucht wird!

                //                return;
                //            }
                //        }

                //    }
                //}


                //if (Column == null)
                //{
                //    base.Add(new FilterItem(Database, filterType, FilterValue));
                //}
                //else
                //{
                //    base.Add(new FilterItem(Column, filterType, FilterValue));
                //}





        }


        public bool IsRowFilterActiv()
        {

            foreach (var ThisFilterItem in this)
            {
                if (ThisFilterItem != null && ThisFilterItem.Column == null && ThisFilterItem.FilterType != enFilterType.KeinFilter) { return true; }
            }

            return false;
        }

        public string RowFilterText()
        {

            foreach (var ThisFilterItem in this)
            {
                if (ThisFilterItem != null && ThisFilterItem.Column == null) { return ThisFilterItem.SearchValue[0]; }
            }

            return string.Empty;
        }






        public bool Uses(ColumnItem Column)
        {

            foreach (var ThisFilterItem in this)
            {
                if (ThisFilterItem != null && ThisFilterItem.FilterType != enFilterType.KeinFilter)
                {
                    if (ThisFilterItem.Column == Column) { return true; }
                }
            }

            return false;
        }


        public bool MayHasRowFilter(ColumnItem Column)
        {

            if (Column.IgnoreAtRowFilter) { return false; }
            return IsRowFilterActiv();
        }


        public void Parse(string ToParse)
        {

            IsParsing = true;
            ThrowEvents = false;
            // Initialize();

            foreach (var pair in ToParse.GetAllTags())
            {
                switch (pair.Key)
                {
                    case "filter":
                        Add(new FilterItem(Database, pair.Value));
                        break;
                    default:
                        Develop.DebugPrint(enFehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                        break;
                }
            }

            ThrowEvents = true;
            IsParsing = false;

        }

        public override string ToString()
        {
            var w = "{";

            foreach (var ThisFilterItem in this)
            {
                if (ThisFilterItem != null)
                {
                    w = w + "Filter=" + ThisFilterItem + ", ";
                }
            }

            return w.TrimEnd(", ") + "}";
        }

        //public IEnumerator<FilterItem> GetEnumerator()
        //{
        //    return _Internal.GetEnumerator();
        //}

        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    return _Internal.GetEnumerator();
        //}

        public void OnChanged()
        {
            if (IsParsing) { Develop.DebugPrint(enFehlerArt.Warnung, "Falscher Parsing Zugriff!"); return; }
            Changed?.Invoke(this, System.EventArgs.Empty);
        }

        public void DeleteOtherAndAddIfNotExists(string columName, enFilterType filterType, string filterBy)
        {

            var tmp = Database.Column[columName];
            if (tmp == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Spalte '" + columName + "' nicht vorhanden."); }


            DeleteOtherAndAddIfNotExists(new FilterItem(tmp, filterType, filterBy));
        }

        public void DeleteOtherAndAddIfNotExists(FilterItem filterItem)
        {

            if (Exists(filterItem)) { return; }

            Delete(filterItem.Column);

            if (Exists(filterItem)) { return; } // Falls ein Event ausgelöst wurde, und es nun doch schon das ist

            Add(filterItem);

        }

        public bool Exists(FilterItem filterItem)
        {
            foreach (var thisFilter in this)
            {
                if (thisFilter.FilterType == filterItem.FilterType)
                {
                    if (thisFilter.Column == filterItem.Column)
                    {

                        if (thisFilter.SearchValue.JoinWithCr() == filterItem.SearchValue.JoinWithCr())
                        {
                            return true;
                        }

                    }

                }
            }

            return false;
        }

        public void DeleteOtherAndAddIfNotExists(string columName, enFilterType filterType, List<string> filterBy)
        {

            var tmp = Database.Column[columName];
            if (tmp == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Spalte '" + columName + "' nicht vorhanden."); }

            DeleteOtherAndAddIfNotExists(new FilterItem(tmp, filterType, filterBy));
        }
    }
}