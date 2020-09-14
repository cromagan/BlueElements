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
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

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


        /// <summary>
        /// Durchsucht die erste (interne) Spalte der Datenbank nach dem hier angegebenen Prmärschlüssel.
        /// </summary>
        /// <param name="primärSchlüssel">Der Primärschlüssel, nach dem gesucht werden soll. Groß/Kleinschreibung wird ignoriert.</param>
        /// <returns>Die Zeile, dessen erste Spalte den Primärschlüssel enthält oder - falls nicht gefunden - NULL.</returns>
        public RowItem this[string primärSchlüssel]
        {
            get
            {
                return this[new FilterItem(Database.Column[0], enFilterType.Istgleich_GroßKleinEgal | enFilterType.MultiRowIgnorieren, primärSchlüssel)];
            }
        }

        public RowItem this[FilterItem filter]
        {
            get
            {
                var d = new FilterCollection(filter.Database)
                {
                    filter
                };
                return this[d];
            }
        }


        public RowItem this[FilterItem filter1, FilterItem filter2]
        {
            get
            {
                var d = new FilterCollection(filter1.Database)
                {
                    filter1,
                    filter2
                };
                return this[d];
            }
        }

        public RowItem this[FilterCollection filter]
        {
            get
            {
                if (Database == null) { return null; }
                return _Internal.Values.FirstOrDefault(ThisRow => ThisRow != null && ThisRow.MatchesTo(filter));
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
            if (!_Internal.TryRemove(Key, out _)) { Develop.DebugPrint(enFehlerArt.Fehler, "Remove Failed"); }
            OnRowRemoved();
        }


        public bool Clear()
        {
            return Remove(new FilterCollection(Database));
        }


        public bool Remove(FilterItem Filter)
        {
            var NF = new FilterCollection(Database)
            {
                Filter
            };
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


            Database.Cell.SystemSet(Database.Column.SysRowCreator, Row, Database.UserName, false);
            Database.Cell.SystemSet(Database.Column.SysRowCreateDate, Row, DateTime.Now.ToString(Constants.Format_Date5), false);

            // Dann die Inital-Werte reinschreiben
            foreach (var ThisColum in Database.Column)
            {
                if (ThisColum != null && !string.IsNullOrEmpty(ThisColum.CellInitValue)) { Row.CellSet(ThisColum, ThisColum.CellInitValue); }
            }

            Row.DoAutomatic(false, false, 1);


            return Row;
        }


        public void DoAutomatic(FilterCollection filter, bool fullCheck, List<RowItem> pinned)
        {
            if (Database.ReadOnly) { return; }

            var x = CalculateSortedRows(filter, null, pinned);


            if (x.Count() == 0) { return; }
            Database.OnProgressbarInfo(new ProgressbarEventArgs("Datenüberprüfung", 0, x.Count(), true, false));

            var all = x.Count;

            while (x.Count > 0)
            {

                Database.OnProgressbarInfo(new ProgressbarEventArgs("Datenüberprüfung", all - x.Count(), all, false, false));
                if (x[0].DoAutomatic(true, fullCheck).Item1)
                {
                    x.RemoveAt(0);
                }

            }

            //foreach (var ThisRowItem in x)
            //{
            //    c++;
            //    Database.OnProgressbarInfo(new ProgressbarEventArgs("Datenüberprüfung", c, x.Count(), false, false));
            //    ThisRowItem.DoAutomatic(true, fullCheck);
            //}

            Database.OnProgressbarInfo(new ProgressbarEventArgs("Datenüberprüfung", x.Count(), x.Count(), false, true));
        }




        public void Remove(RowItem Row)
        {

            //if (Database.InvokeRequired)
            //{
            //    Database.Invoke(new Action(() => Database.Row.Remove(Row)));
            //    return;
            //}

            if (Row == null) { return; }
            Remove(Row.Key);
        }


        internal void Repair()
        {

            foreach (var ThisRowItem in _Internal.Values)
            {
                if (ThisRowItem != null)
                {
                    //ThisRowItem.Repair();
                    _LastRowKey = Math.Max(_LastRowKey, ThisRowItem.Key); // Die Letzte ID ermitteln,falls der gleadene Wert fehlerhaft ist
                }

            }
        }




        internal void SaveToByteList(List<byte> l)
        {
            Database.SaveToByteList(l, enDatabaseDataType.LastRowKey, _LastRowKey.ToString());
        }

        public RowItem SearchByKey(int Key)
        {
            try
            {
                if (Key < 0) { return null; }
                if (!_Internal.ContainsKey(Key)) { return null; }
                return _Internal[Key];
            }
            catch
            {
                // Develop.DebugPrint(ex);
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

            var x = (from thisrowitem in _Internal.Values where thisrowitem != null let D = thisrowitem.CellGetDateTime(Database.Column.SysRowCreateDate) where DateTime.Now.Subtract(D).TotalHours > InHours select thisrowitem.Key).Select(dummy => (long)dummy).ToList();

            //foreach (var thisrowitem in _Internal.Values)
            //{
            //    if (thisrowitem != null)
            //    {
            //        var D = thisrowitem.CellGetDateTime(Database.Column.SysRowCreateDate());
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


        public List<RowItem> CalculateSortedRows(FilterCollection Filter, RowSortDefinition rowSortDefinition, List<RowItem> pinnedRows)
        {
            var TMP = new List<string>();
            var _tmpSortedRows = new List<RowItem>();

            if (pinnedRows == null) { pinnedRows = new List<RowItem>(); }

            foreach (var ThisRowItem in Database.Row)
            {
                if (ThisRowItem != null)
                {
                    if (ThisRowItem.MatchesTo(Filter) && !pinnedRows.Contains(ThisRowItem))
                    {
                        if (rowSortDefinition == null)
                        {
                            TMP.Add(ThisRowItem.CompareKey(null));
                        }
                        else
                        {
                            TMP.Add(ThisRowItem.CompareKey(rowSortDefinition.Columns));
                        }
                    }
                }
            }


            TMP.Sort();


            var cc = 0;
            if (rowSortDefinition == null || !rowSortDefinition.Reverse)
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
                for (var z = TMP.Count - 1; z > -1; z--)
                {
                    if (!string.IsNullOrEmpty(TMP[z]))
                    {
                        cc = TMP[z].IndexOf(Constants.SecondSortChar + "<key>");
                        _tmpSortedRows.Add(Database.Row.SearchByKey(int.Parse(TMP[z].Substring(cc + 6))));
                    }
                }

            }

            var newPinned = new List<RowItem>();

            foreach (var thisPinned in pinnedRows)
            {
                if (Database.Row.Contains(thisPinned))
                {
                    newPinned.Add(thisPinned);
                }
            }


            _tmpSortedRows.InsertRange(0, newPinned);
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