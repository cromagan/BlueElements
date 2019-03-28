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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BlueBasics;
using BlueBasics.Enums;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;

namespace BlueDatabase
{
    public sealed class RowCollection : IEnumerable<RowItem>
    {
        #region  Variablen-Deklarationen 

        private readonly ConcurrentDictionary<int, RowItem> _Internal = new ConcurrentDictionary<int, RowItem>();

        public readonly Database Database;

        private int _LastRowKey;

        #endregion


        #region  Event-Deklarationen + Delegaten 
        public event EventHandler<RowCheckedEventArgs> RowChecked;

        public event EventHandler<DoRowAutomaticEventArgs> DoSpecialRules;

        public event EventHandler<RowEventArgs> RowRemoving;

        public event EventHandler RowRemoved;

        public event EventHandler<RowEventArgs> RowAdded;
        #endregion


        #region  Construktor + Initialize 


        public void Initialize()
        {
            _LastRowKey = 0;
        }

        public RowCollection(Database cDatabase)
        {
            Database = cDatabase;
            Initialize();
        }

        #endregion


        #region  Properties 

        public RowItem this[FilterItem Filter]
        {
            get
            {
                var d = new FilterCollection(Filter.Database);
                d.Add(Filter);
                return this[d];
            }
        }



        public RowItem this[string ValueOfFirstColumn]
        {
            get
            {
                return this[new FilterItem(Database.Column[0], enFilterType.Istgleich_GroßKleinEgal | enFilterType.MultiRowIgnorieren, ValueOfFirstColumn)];
            }
        }

        public RowItem this[FilterItem Filter1, FilterItem Filter2]
        {
            get
            {
                var d = new FilterCollection(Filter1.Database);
                d.Add(Filter1);
                d.Add(Filter2);
                return this[d];
                //}
            }
        }

        public RowItem this[FilterCollection Filter]
        {
            get
            {
                if (Database == null) { return null; }

                return _Internal.Values.FirstOrDefault(ThisRow => ThisRow != null && ThisRow.MatchesTo(Filter));

                //foreach (var ThisRow in _Internal.Values)
                //{
                //    if (ThisRow != null && ThisRow.MatchesTo(Filter)) { return ThisRow; }
                //}

                //return null;
            }
        }


        #endregion


        internal int NextRowKey()
        {
            do
            {
                if (_LastRowKey == int.MaxValue) { _LastRowKey = 0; }
                _LastRowKey += 1;
            } while (SearchByKey(_LastRowKey) != null);

            return _LastRowKey;
        }



        public void Remove(int Key)
        {
            if (Database.ReadOnly) { return; }

            var e = SearchByKey(Key);
            if (e == null) { return; }

            OnRowRemoving(new RowEventArgs(e));
            foreach (var ThisColumnItem in Database.Column)
            {
                if (ThisColumnItem != null)
                {
                    Database.Cell.Delete(ThisColumnItem, Key);
                }
            }
            if (!_Internal.TryRemove(Key, out e)) { Develop.DebugPrint(enFehlerArt.Fehler, "Remove Failed"); }
            OnRowRemoved();
        }


        public bool Clear()
        {
            return Remove(new FilterCollection(Database));
        }


        public bool Remove(FilterItem Filter)
        {
            var NF = new FilterCollection(Database);
            NF.Add(Filter);
            return Remove(NF);
        }

        public bool Remove(FilterCollection Filter)
        {

            var x = (from thisrowitem in _Internal.Values where thisrowitem != null && thisrowitem.MatchesTo(Filter) select thisrowitem.Key).Select(dummy => (long)dummy).ToList();
            //var x = new List<long>();
            //foreach (var thisrowitem in _Internal.Values)
            //{
            //    if (thisrowitem != null && thisrowitem.MatchesTo(Filter))
            //    {
            //        x.Add(thisrowitem.Key);
            //    }
            //}




            //if (x.Count == 0)
            //{
            //    if (Abfrage) { MessageBox.Show("Keine Zeilen gelöscht.", enImageCode.Information, "OK"); }
            //    return false;
            //}

            //if (Abfrage && MessageBox.Show(x.Count.ToString() + " Zeile(n) <b>löschen</b>?", enImageCode.Frage, "Ja", "Nein") != 0) { return false; }


            foreach (int ThisKey in x)
            {
                Remove(ThisKey);
            }

            return true;

        }

        internal string Load_310(enDatabaseDataType Art, string Wert)
        {
            switch (Art)
            {


                case enDatabaseDataType.LastRowKey:
                    _LastRowKey = int.Parse(Wert);

                    break;
                default:

                    if (Art.ToString() == ((int)Art).ToString())
                    {
                        Develop.DebugPrint(enFehlerArt.Info, "Laden von Datentyp '" + Art + "' nicht definiert.<br>Wert: " + Wert + "<br>Datei: " + Database.Filename);
                    }
                    else
                    {
                        return "Interner Fehler: Für den Datentyp  '" + Art + "'  wurde keine Laderegel definiert.";
                    }

                    break;
            }

            return "";

        }


        public RowItem Add(RowItem Row)
        {
            if (!_Internal.TryAdd(Row.Key, Row)) { Develop.DebugPrint(enFehlerArt.Fehler, "Add Failed"); }

            OnRowAdded(new RowEventArgs(Row));
            return Row;
        }

        private void OnRowChecked(object sender, RowCheckedEventArgs e)
        {
            RowChecked?.Invoke(this, e);
        }


        private void OnDoSpecialRules(object sender, DoRowAutomaticEventArgs e)
        {
            DoSpecialRules?.Invoke(this, e);
        }

        public RowItem Add(string ValueOfCellInFirstColumn)
        {
            if (string.IsNullOrEmpty(ValueOfCellInFirstColumn))
            {
                Develop.DebugPrint("Value = 0");
                return null;
            }


            var Row = new RowItem(Database);

            Add(Row);

            Row.CellSet(Database.Column[0], ValueOfCellInFirstColumn);

            return Row;
        }


        public void DoAutomatic(FilterCollection Filter)
        {
            foreach (var ThisRowItem in _Internal)
            {
                if (ThisRowItem.Value != null)
                {
                    //eProgressbar("Datenüberprüfung...", rc, _Internal.Count - 1, null);
                    if (ThisRowItem.Value.MatchesTo(Filter))
                    {
                        ThisRowItem.Value.DoAutomatic(false, true);
                    }
                }
            }
            //P?.Close();
        }




        public void Remove(RowItem Row)
        {
            if (Row == null) { return; }
            Remove(Row.Key);
        }


        internal void Repair()
        {

            foreach (var ThisRowItem in _Internal.Values)
            {
                if (ThisRowItem != null)
                {
                    ThisRowItem.Repair(false);
                    _LastRowKey = Math.Max(_LastRowKey, ThisRowItem.Key); // Die Letzte ID ermitteln,falls der gleadene Wert fehlerhaft ist
                }

            }
        }




        internal void SaveToByteList(List<byte> l)
        {
            Database.SaveToByteList(l, enDatabaseDataType.LastRowKey, _LastRowKey.ToString());
        }

        internal bool ContainsKey(int RowKey)
        {
            return _Internal.ContainsKey(RowKey);
        }

        public RowItem SearchByKey(int Key)
        {
            try
            {
                if (Key < 0) { return null; }
                if (!_Internal.ContainsKey(Key)) { return null; }
                return _Internal[Key];
            }
            catch (Exception ex)
            {
                Develop.DebugPrint(ex);
                return SearchByKey(Key);
            }


        }

        public RowItem First()
        {
            return _Internal.Values.FirstOrDefault(ThisRowItem => ThisRowItem != null);

            //foreach (var ThisRowItem in _Internal.Values)
            //{
            //    if (ThisRowItem != null) { return ThisRowItem; }
            //}

            //return null;
        }

        public IEnumerator<RowItem> GetEnumerator()
        {
            return _Internal.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return IEnumerable_GetEnumerator();
        }
        private IEnumerator IEnumerable_GetEnumerator()
        {
            return _Internal.Values.GetEnumerator();
        }


        public int Count()
        {
            return _Internal.Count;
        }

        internal void RemoveNullOrEmpty()
        {
            _Internal.RemoveNullOrEmpty();
        }

        internal void OnRowAdded(RowEventArgs e)
        {
            e.Row.RowChecked += OnRowChecked;
            e.Row.DoSpecialRules += OnDoSpecialRules;
            RowAdded?.Invoke(this, e);
        }

        internal void OnRowRemoving(RowEventArgs e)
        {
            e.Row.RowChecked -= OnRowChecked;
            e.Row.DoSpecialRules -= OnDoSpecialRules;
            RowRemoving?.Invoke(this, e);
        }

        internal void OnRowRemoved()
        {
            RowRemoved?.Invoke(this, System.EventArgs.Empty);
        }

        public bool RemoveOlderThan(float InHours)
        {

            var x = (from thisrowitem in _Internal.Values where thisrowitem != null let D = thisrowitem.CellGetDate(Database.Column.SysRowCreateDate) where DateTime.Now.Subtract(D).TotalHours > InHours select thisrowitem.Key).Select(dummy => (long)dummy).ToList();

            //foreach (var thisrowitem in _Internal.Values)
            //{
            //    if (thisrowitem != null)
            //    {
            //        var D = thisrowitem.CellGetDate(Database.Column.SysRowCreateDate());
            //        if (DateTime.Now.Subtract(D).TotalHours > InHours) { x.Add(thisrowitem.Key); }
            //    }
            //}




            if (x.Count == 0) { return false; }



            foreach (int ThisKey in x)
            {
                Remove(ThisKey);
            }

            return true;
        }


        public List<RowItem> CalculateSortedRows(FilterCollection Filter, RowSortDefinition tmpSortDefinition)
        {
            var TMP = new List<string>();
            var _tmpSortedRows = new List<RowItem>();


            foreach (var ThisRowItem in Database.Row)
            {
                if (ThisRowItem != null)
                {
                    if (ThisRowItem.MatchesTo(Filter))
                    {
                        if (tmpSortDefinition == null)
                        {
                            TMP.Add(ThisRowItem.CompareKey(null));
                        }
                        else
                        {
                            TMP.Add(ThisRowItem.CompareKey(tmpSortDefinition.Columns));
                        }
                    }
                }
            }


            TMP.Sort();


            var cc = 0;
            if (tmpSortDefinition == null || !tmpSortDefinition.Reverse)
            {
                foreach (var t in TMP)
                {
                    if (!string.IsNullOrEmpty(t))
                    {
                        cc = t.IndexOf(Constants.SecondSortChar + "<key>");
                        _tmpSortedRows.Add(Database.Row.SearchByKey(int.Parse(t.Substring(cc + 6))));
                    }
                }
            }
            else
            {
                for (var z = TMP.Count - 1 ; z > -1 ; z--)
                {
                    if (!string.IsNullOrEmpty(TMP[z]))
                    {
                        cc = TMP[z].IndexOf(Constants.SecondSortChar + "<key>");
                        _tmpSortedRows.Add(Database.Row.SearchByKey(int.Parse(TMP[z].Substring(cc + 6))));
                    }
                }

            }
            return _tmpSortedRows;
        }

        internal static List<RowItem> MatchesTo(FilterItem FilterItem)
        {

            var l = new List<RowItem>();
            foreach (var ThisRow in FilterItem.Database.Row)
            {
                if (ThisRow.MatchesTo(FilterItem))
                {
                    l.Add(ThisRow);
                }
            }
            return l;
        }
    }
}