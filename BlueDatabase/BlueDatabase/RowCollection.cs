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

using BlueBasics;
using BlueBasics.Enums;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace BlueDatabase {

    public sealed class RowCollection : IEnumerable<RowItem>, IDisposable {

        #region Fields

        private readonly ConcurrentDictionary<int, RowItem> _Internal = new();
        private int _LastRowKey;
        private bool disposedValue;

        #endregion

        #region Constructors

        public RowCollection(Database database) {
            Database = database;
            Database.Disposing += Database_Disposing;
            Initialize();
        }

        #endregion

        #region Destructors

        // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
        ~RowCollection() {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: false);
        }

        #endregion

        #region Events

        public event EventHandler<DoRowAutomaticEventArgs> DoSpecialRules;

        public event EventHandler<RowEventArgs> RowAdded;

        public event EventHandler<RowCheckedEventArgs> RowChecked;

        public event EventHandler RowRemoved;

        public event EventHandler<RowEventArgs> RowRemoving;

        #endregion

        #region Properties

        public int Count => _Internal.Count;
        public Database Database { get; private set; }

        #endregion

        #region Indexers

        /// <summary>
        /// Durchsucht die erste (interne) Spalte der Datenbank nach dem hier angegebenen Prmärschlüssel.
        /// </summary>
        /// <param name="primärSchlüssel">Der Primärschlüssel, nach dem gesucht werden soll. Groß/Kleinschreibung wird ignoriert.</param>
        /// <returns>Die Zeile, dessen erste Spalte den Primärschlüssel enthält oder - falls nicht gefunden - NULL.</returns>
        public RowItem this[string primärSchlüssel] => this[new FilterItem(Database.Column[0], enFilterType.Istgleich_GroßKleinEgal | enFilterType.MultiRowIgnorieren, primärSchlüssel)];

        public RowItem this[params FilterItem[] filter] {
            get {
                if (filter == null || filter.Count() == 0) {
                    Develop.DebugPrint("Kein Filter angekommen!");
                    return null;
                }
                FilterCollection d = new(filter[0].Database);
                d.AddRange(filter);
                return this[d];
            }
        }

        public RowItem this[FilterCollection filter] => Database == null ? null : _Internal.Values.FirstOrDefault(ThisRow => ThisRow != null && ThisRow.MatchesTo(filter));

        #endregion

        #region Methods

        /// <summary>
        /// Gibt einen Zeilenschlüssel zurück, der bei allen aktuell geladenen Datenbanken einzigartig ist.
        /// </summary>
        /// <returns></returns>
        public static string UniqueKeyValue() {
            var x = 9999;
            do {
                x += 1;
                if (x > 99999) { Develop.DebugPrint(enFehlerArt.Fehler, "Unique ID konnte nicht erzeugt werden"); }

                var unique = ("X" + DateTime.Now.ToString("mm.fff") + x.ToString(Constants.Format_Integer5)).RemoveChars(Constants.Char_DateiSonderZeichen + ".");
                var ok = true;

                foreach (var thisfile in BlueBasics.MultiUserFile.clsMultiUserFile.AllFiles) {
                    if (thisfile is Database db) {
                        var row = db.Row[unique];
                        if (row != null) { ok = false; break; }
                    }
                }

                if (ok) { return unique; }
            } while (true);
        }

        public void Add(RowItem Row) {
            if (!_Internal.TryAdd(Row.Key, Row)) { Develop.DebugPrint(enFehlerArt.Fehler, "Add Failed"); }
            OnRowAdded(new RowEventArgs(Row));
        }

        public RowItem Add(string ValueOfCellInFirstColumn) {
            if (string.IsNullOrEmpty(ValueOfCellInFirstColumn)) {
                Develop.DebugPrint("Value = 0");
                return null;
            }
            RowItem Row = new(Database);
            Add(Row);
            Row.CellSet(Database.Column[0], ValueOfCellInFirstColumn);
            Database.Cell.SystemSet(Database.Column.SysRowCreator, Row, Database.UserName);
            Database.Cell.SystemSet(Database.Column.SysRowCreateDate, Row, DateTime.Now.ToString(Constants.Format_Date5));
            // Dann die Inital-Werte reinschreiben
            foreach (var ThisColum in Database.Column) {
                if (ThisColum != null && !string.IsNullOrEmpty(ThisColum.CellInitValue)) { Row.CellSet(ThisColum, ThisColum.CellInitValue); }
            }
            Row.DoAutomatic(false, false, 1, "new row");
            return Row;
        }

        public List<RowItem> CalculateSortedRows(List<FilterItem> filter, RowSortDefinition rowSortDefinition, List<RowItem> pinnedRows) {
            List<string> TMP = new();
            List<RowItem> _tmpSortedRows = new();
            if (pinnedRows == null) { pinnedRows = new List<RowItem>(); }

            foreach (var ThisRowItem in Database.Row) {
                if (ThisRowItem != null) {
                    if (ThisRowItem.MatchesTo(filter) && !pinnedRows.Contains(ThisRowItem)) {
                        if (rowSortDefinition == null) {
                            TMP.Add(ThisRowItem.CompareKey(null));
                        } else {
                            TMP.Add(ThisRowItem.CompareKey(rowSortDefinition.Columns));
                        }
                    }
                }
            }
            TMP.Sort();

            int cc;
            if (rowSortDefinition == null || !rowSortDefinition.Reverse) {
                foreach (var t in TMP) {
                    if (!string.IsNullOrEmpty(t)) {
                        cc = t.IndexOf(Constants.SecondSortChar + "<key>");
                        _tmpSortedRows.Add(Database.Row.SearchByKey(int.Parse(t.Substring(cc + 6))));
                    }
                }
            } else {
                for (var z = TMP.Count - 1; z > -1; z--) {
                    if (!string.IsNullOrEmpty(TMP[z])) {
                        cc = TMP[z].IndexOf(Constants.SecondSortChar + "<key>");
                        _tmpSortedRows.Add(Database.Row.SearchByKey(int.Parse(TMP[z].Substring(cc + 6))));
                    }
                }
            }

            List<RowItem> newPinned = new();
            foreach (var thisPinned in pinnedRows) {
                if (Database.Row.Contains(thisPinned)) {
                    newPinned.Add(thisPinned);
                }
            }

            _tmpSortedRows.InsertRange(0, newPinned);
            return _tmpSortedRows;
        }

        public bool Clear() => Remove(new FilterCollection(Database));

        public void Dispose() {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void DoAutomatic(FilterCollection filter, bool fullCheck, List<RowItem> pinned, string startroutine) {
            if (Database.ReadOnly) { return; }
            DoAutomatic(CalculateSortedRows(filter, null, pinned), fullCheck, startroutine);
        }

        public void DoAutomatic(List<RowItem> x, bool fullCheck, string startroutine) {
            if (Database.ReadOnly) { return; }

            if (x == null || x.Count() == 0) { return; }

            Database.OnProgressbarInfo(new ProgressbarEventArgs("Datenüberprüfung", 0, x.Count(), true, false));

            var all = x.Count;
            while (x.Count > 0) {
                Database.OnProgressbarInfo(new ProgressbarEventArgs("Datenüberprüfung", all - x.Count(), all, false, false));

                (var checkPerformed, var _, var skript) = x[0].DoAutomatic(true, fullCheck, false, startroutine);

                if (skript != null && !string.IsNullOrEmpty(skript.Error)) {
                    var w = x[0].CellFirstString();
                    x.Clear();
                    Database.OnDropMessage("Skript fehlerhaft bei " + w + "\r\n" + skript.Error);
                    break;
                }
                if (checkPerformed) { x.RemoveAt(0); }
            }
            Database.OnProgressbarInfo(new ProgressbarEventArgs("Datenüberprüfung", x.Count(), x.Count(), false, true));
        }

        public RowItem First() => _Internal.Values.FirstOrDefault(ThisRowItem => ThisRowItem != null);

        public IEnumerator<RowItem> GetEnumerator() => _Internal.Values.GetEnumerator();

        //foreach (var ThisRowItem in _Internal.Values)//{//    if (ThisRowItem != null) { return ThisRowItem; }//}//return null;
        IEnumerator IEnumerable.GetEnumerator() => IEnumerable_GetEnumerator();

        public void Initialize() => _LastRowKey = 0;

        public void Remove(int Key) {
            var e = SearchByKey(Key);
            if (e == null) { return; }
            OnRowRemoving(new RowEventArgs(e));
            foreach (var ThisColumnItem in Database.Column) {
                if (ThisColumnItem != null) {
                    Database.Cell.Delete(ThisColumnItem, Key);
                }
            }
            if (!_Internal.TryRemove(Key, out _)) { Develop.DebugPrint(enFehlerArt.Fehler, "Remove Failed"); }
            OnRowRemoved();
        }

        public bool Remove(FilterItem Filter) {
            FilterCollection NF = new(Database)
            {
                Filter
            };
            return Remove(NF);
        }

        public bool Remove(FilterCollection Filter) {
            var x = (from thisrowitem in _Internal.Values where thisrowitem != null && thisrowitem.MatchesTo(Filter) select thisrowitem.Key).Select(dummy => (long)dummy).ToList();
            foreach (int ThisKey in x) {
                Remove(ThisKey);
            }
            return true;
        }

        public void Remove(RowItem Row) {
            //if (Database.InvokeRequired)
            //{
            //    Database.Invoke(new Action(() => Database.Row.Remove(Row)));
            //    return;
            //}
            if (Row == null) { return; }
            Remove(Row.Key);
        }

        public bool RemoveOlderThan(float InHours) {
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
            foreach (int ThisKey in x) {
                Remove(ThisKey);
            }
            return true;
        }

        public RowItem SearchByKey(int Key) {
            try {
                return Key < 0 ? null : !_Internal.ContainsKey(Key) ? null : _Internal[Key];
            } catch {
                // Develop.DebugPrint(ex);
                return SearchByKey(Key);
            }
        }

        internal static List<RowItem> MatchesTo(FilterItem filterItem) {
            List<RowItem> l = new();

            if (filterItem == null) { return l; }

            foreach (var ThisRow in filterItem.Database.Row) {
                if (ThisRow.MatchesTo(filterItem)) {
                    l.Add(ThisRow);
                }
            }
            return l;
        }

        internal static List<RowItem> MatchesTo(List<FilterItem> filterItem) {
            List<RowItem> l = new();

            if (filterItem == null || filterItem.Count < 1) { return l; }

            foreach (var ThisRow in filterItem[0].Database.Row) {
                if (ThisRow.MatchesTo(filterItem)) {
                    l.Add(ThisRow);
                }
            }
            return l;
        }

        internal string Load_310(enDatabaseDataType Art, string Wert) {
            switch (Art) {
                case enDatabaseDataType.LastRowKey:
                    _LastRowKey = int.Parse(Wert);
                    break;

                default:
                    if (Art.ToString() == ((int)Art).ToString()) {
                        Develop.DebugPrint(enFehlerArt.Info, "Laden von Datentyp '" + Art + "' nicht definiert.<br>Wert: " + Wert + "<br>Datei: " + Database.Filename);
                    } else {
                        return "Interner Fehler: Für den Datentyp  '" + Art + "'  wurde keine Laderegel definiert.";
                    }
                    break;
            }
            return "";
        }

        internal int NextRowKey() {
            do {
                if (_LastRowKey == int.MaxValue) { _LastRowKey = 0; }
                _LastRowKey++;
            } while (SearchByKey(_LastRowKey) != null);
            return _LastRowKey;
        }

        internal void OnRowAdded(RowEventArgs e) {
            e.Row.RowChecked += OnRowChecked;
            e.Row.DoSpecialRules += OnDoSpecialRules;
            RowAdded?.Invoke(this, e);
        }

        internal void OnRowRemoved() => RowRemoved?.Invoke(this, System.EventArgs.Empty);

        internal void OnRowRemoving(RowEventArgs e) {
            e.Row.RowChecked -= OnRowChecked;
            e.Row.DoSpecialRules -= OnDoSpecialRules;
            RowRemoving?.Invoke(this, e);
        }

        internal void RemoveNullOrEmpty() => _Internal.RemoveNullOrEmpty();

        internal void Repair() {
            foreach (var ThisRowItem in _Internal.Values) {
                if (ThisRowItem != null) {
                    //ThisRowItem.Repair();
                    _LastRowKey = Math.Max(_LastRowKey, ThisRowItem.Key); // Die Letzte ID ermitteln,falls der gleadene Wert fehlerhaft ist
                }
            }
        }

        internal void SaveToByteList(List<byte> l) => Database.SaveToByteList(l, enDatabaseDataType.LastRowKey, _LastRowKey.ToString());

        private void Database_Disposing(object sender, System.EventArgs e) => Dispose();

        private void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
                }
                Database.Disposing -= Database_Disposing;
                Database = null;
                _Internal.Clear();
                // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
                // TODO: Große Felder auf NULL setzen
                disposedValue = true;
            }
        }

        private IEnumerator IEnumerable_GetEnumerator() => _Internal.Values.GetEnumerator();

        private void OnDoSpecialRules(object sender, DoRowAutomaticEventArgs e) => DoSpecialRules?.Invoke(this, e);

        private void OnRowChecked(object sender, RowCheckedEventArgs e) => RowChecked?.Invoke(this, e);

        #endregion
    }
}