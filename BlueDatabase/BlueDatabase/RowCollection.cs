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
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlueDatabase {

    public sealed class RowCollection : IEnumerable<RowItem>, IDisposable {

        #region Fields

        private readonly ConcurrentDictionary<long, RowItem?> _internal = new();

        private bool _disposedValue;

        #endregion

        #region Constructors

        public RowCollection(Database database) {
            Database = database;
            Database.Disposing += Database_Disposing;
            //Initialize();
        }

        #endregion

        #region Destructors

        // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
        ~RowCollection() {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(false);
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

        public int Count => _internal.Count;
        public Database? Database { get; private set; }

        public long VisibleRowCount { get; private set; }

        #endregion

        #region Indexers

        /// <summary>
        /// Durchsucht die erste (interne) Spalte der Datenbank nach dem hier angegebenen Prmärschlüssel.
        /// </summary>
        /// <param name="primärSchlüssel">Der Primärschlüssel, nach dem gesucht werden soll. Groß/Kleinschreibung wird ignoriert.</param>
        /// <returns>Die Zeile, dessen erste Spalte den Primärschlüssel enthält oder - falls nicht gefunden - NULL.</returns>
        public RowItem this[string primärSchlüssel] => this[new FilterItem(Database.Column[0], enFilterType.Istgleich_GroßKleinEgal | enFilterType.MultiRowIgnorieren, primärSchlüssel)];

        public RowItem? this[params FilterItem[]? filter] {
            get {
                if (filter == null || filter.Length == 0) {
                    Develop.DebugPrint("Kein Filter angekommen!");
                    return null;
                }
                FilterCollection d = new(filter[0].Database);
                d.AddRange(filter);
                return this[d];
            }
        }

        public RowItem this[FilterCollection? filter] => Database == null ? null : _internal.Values.FirstOrDefault(thisRow => thisRow != null && thisRow.MatchesTo(filter));

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

                foreach (var thisfile in BlueBasics.MultiUserFile.MultiUserFile.AllFiles) {
                    if (thisfile is Database db) {
                        var row = db.Row[unique];
                        if (row != null) { ok = false; break; }
                    }
                }

                if (ok) { return unique; }
            } while (true);
        }

        public void Add(RowItem? row) {
            if (!_internal.TryAdd(row.Key, row)) { Develop.DebugPrint(enFehlerArt.Fehler, "Add Failed"); }
            OnRowAdded(new RowEventArgs(row));
        }

        public RowItem? Add(string valueOfCellInFirstColumn) => Add(valueOfCellInFirstColumn, true);

        /// <summary>
        /// Erstellt eine neue Spalte mit den aus den Filterkriterien. nur Fiter IstGleich wird unterstützt.
        /// Schägt irgendetwas fehl, wird NULL zurückgegeben.
        /// Ist ein Filter mehrfach vorhanden, erhält die Zelle den LETZTEN Wert.
        /// Am Schluß wird noch das Skript ausgeführt.
        /// </summary>
        /// <param name="fi"></param>
        /// <returns></returns>
        public RowItem? Add(List<FilterItem>? fi) {
            List<string> first = null;

            foreach (var thisfi in fi) {
                if (Database.Column.First() == thisfi.Column) {
                    if (first != null) { return null; }
                    first = thisfi.SearchValue;
                }
                if (thisfi.FilterType != enFilterType.Istgleich) { return null; }
                if (thisfi.Column == null) { return null; }
                if (thisfi.Column.Database != Database) { return null; }
            }

            if (first == null) { return null; }

            var row = Add(first.JoinWithCr(), false);

            if (row == null) { return null; }

            foreach (var thisfi in fi) {
                row.CellSet(thisfi.Column, thisfi.SearchValue);
            }

            row.DoAutomatic(false, false, 1, "new row");

            return row;
        }

        public RowItem? Add(string valueOfCellInFirstColumn, bool runScriptOfNewRow) {
            if (string.IsNullOrEmpty(valueOfCellInFirstColumn)) {
                Develop.DebugPrint("Value = 0");
                return null;
            }
            RowItem row = new(Database);
            Add(row);
            row.CellSet(Database.Column[0], valueOfCellInFirstColumn);
            Database.Cell.SystemSet(Database.Column.SysRowCreator, row, Database.UserName);
            Database.Cell.SystemSet(Database.Column.SysRowCreateDate, row, DateTime.Now.ToString(Constants.Format_Date5));
            // Dann die Inital-Werte reinschreiben
            foreach (var thisColum in Database.Column.Where(thisColum => thisColum != null && !string.IsNullOrEmpty(thisColum.CellInitValue))) {
                row.CellSet(thisColum, thisColum.CellInitValue);
            }

            if (runScriptOfNewRow) { row.DoAutomatic(false, false, 1, "new row"); }

            return row;
        }

        /// <summary>
        /// Gibt die mit dieser Kombination sichtbaren Zeilen zurück. Ohne Sortierung. Jede Zeile kann maximal einmal vorkommen.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="pinnedRows"></param>
        /// <returns></returns>
        public List<RowItem> CalculateFilteredRows(List<FilterItem>? filter) {
            List<RowItem> tmpVisibleRows = new();
            if (Database == null) { return tmpVisibleRows; }

            try {
                var lockMe = new object();
                Parallel.ForEach(Database.Row, thisRowItem => {
                    if (thisRowItem != null) {
                        if (thisRowItem.MatchesTo(filter)) {
                            lock (lockMe) { tmpVisibleRows.Add(thisRowItem); }
                        }
                    }
                });
            } catch {
                return CalculateFilteredRows(filter);
            }

            return tmpVisibleRows;
        }

        public List<RowData?> CalculateSortedRows(List<FilterItem>? filter, RowSortDefinition rowSortDefinition, List<RowItem>? pinnedRows, List<RowData>? reUseMe) => CalculateSortedRows(CalculateFilteredRows(filter), rowSortDefinition, pinnedRows, reUseMe);

        public List<RowData?> CalculateSortedRows(List<RowItem?> filteredRows, RowSortDefinition rowSortDefinition, List<RowItem>? pinnedRows, List<RowData>? reUseMe) {
            var lockMe = new object();
            VisibleRowCount = 0;

            #region Ermitteln, ob mindestens eine Überschruft vorhanden ist (capName)

            var capName = pinnedRows != null && pinnedRows.Count > 0;
            if (!capName) {
                if (filteredRows.Any(thisRow => !thisRow.CellIsNullOrEmpty(thisRow.Database.Column.SysChapter))) {
                    capName = true;
                }
            }

            #endregion

            #region _Angepinnten Zeilen erstellen (_pinnedData)

            List<RowData> pinnedData = new();
            if (pinnedRows != null) {
                Parallel.ForEach(pinnedRows, thisRow => {
                    var rd = reUseMe.Get(thisRow, "Angepinnt") is RowData r ? r : new RowData(thisRow, "Angepinnt");
                    rd.AdditinalSort = "1";
                    rd.MarkYellow = true;
                    rd.AdditionalSort = rowSortDefinition == null ? thisRow.CompareKey(null) : thisRow.CompareKey(rowSortDefinition.Columns);

                    lock (lockMe) {
                        VisibleRowCount++;
                        pinnedData.Add(rd);
                    }
                });
            }

            #endregion

            #region Gefiltere Zeilen erstellen (_rowData)

            List<RowData?> rowData = new();
            Parallel.ForEach(filteredRows, thisRow => {
                var adk = rowSortDefinition == null ? thisRow.CompareKey(null) : thisRow.CompareKey(rowSortDefinition.Columns);

                var markYellow = pinnedRows != null && pinnedRows.Contains(thisRow);
                var added = markYellow;

                var caps = thisRow.CellGetList(thisRow.Database.Column.SysChapter);

                if (caps.Count > 0) {
                    if (caps.Contains(string.Empty)) {
                        caps.Remove(string.Empty);
                        caps.Add("-?-");
                    }
                }

                if (caps.Count == 0 && capName) { caps.Add("Weitere Zeilen"); }
                if (caps.Count == 0) { caps.Add(string.Empty); }

                foreach (var thisCap in caps) {
                    var rd = reUseMe.Get(thisRow, thisCap) is RowData r ? r : new RowData(thisRow, thisCap);

                    rd.AdditinalSort = "2";
                    rd.MarkYellow = markYellow;
                    rd.AdditionalSort = adk;
                    lock (lockMe) {
                        rowData.Add(rd);
                        if (!added) { VisibleRowCount++; added = true; }
                    }
                }
            });

            #endregion

            pinnedData.Sort();
            rowData.Sort();

            if (rowSortDefinition != null && rowSortDefinition.Reverse) { rowData.Reverse(); }

            rowData.InsertRange(0, pinnedData);
            return rowData;
        }

        /// <summary>
        /// Gibt die mit dieser Kombination sichtbaren Zeilen zurück. Ohne Sortierung. Jede Zeile kann maximal einmal vorkommen.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="pinnedRows"></param>
        /// <returns></returns>
        public List<RowItem> CalculateVisibleRows(List<FilterItem>? filter, List<RowItem>? pinnedRows) {
            List<RowItem> tmpVisibleRows = new();
            if (pinnedRows == null) { pinnedRows = new List<RowItem>(); }

            var lockMe = new object();
            Parallel.ForEach(Database.Row, thisRowItem => {
                if (thisRowItem != null) {
                    if (thisRowItem.MatchesTo(filter) || pinnedRows.Contains(thisRowItem)) {
                        lock (lockMe) { tmpVisibleRows.Add(thisRowItem); }
                    }
                }
            });

            //foreach (var thisRowItem in Database.Row) {
            //    if (thisRowItem != null) {
            //        if (thisRowItem.MatchesTo(filter) || pinnedRows.Contains(thisRowItem)) {
            //            _tmpVisibleRows.Add(thisRowItem);
            //        }
            //    }
            //}

            return tmpVisibleRows;
        }

        public bool Clear() => Remove(new FilterCollection(Database), null);

        public void Dispose() {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void DoAutomatic(FilterCollection? filter, bool fullCheck, List<RowItem?> pinned, string startroutine) {
            if (Database.ReadOnly) { return; }
            DoAutomatic(CalculateVisibleRows(filter, pinned), fullCheck, startroutine);
        }

        public void DoAutomatic(List<RowItem>? rows, bool fullCheck, string startroutine) {
            if (Database == null || Database.ReadOnly) { return; }

            if (rows == null || rows.Count == 0) { return; }

            Database.OnProgressbarInfo(new ProgressbarEventArgs("Datenüberprüfung", 0, rows.Count, true, false));

            var all = rows.Count;
            while (rows.Count > 0) {
                Database.OnProgressbarInfo(new ProgressbarEventArgs("Datenüberprüfung", all - rows.Count, all, false, false));

                var (checkPerformed, _, s) = rows[0].DoAutomatic(true, fullCheck, startroutine);

                if (s != null && !string.IsNullOrEmpty(s.Error)) {
                    var w = rows[0].CellFirstString();
                    rows.Clear();
                    Database.OnDropMessage(enFehlerArt.Warnung, "Skript fehlerhaft bei " + w + "\r\n" + s.Error);
                    break;
                }
                if (checkPerformed) { rows.RemoveAt(0); }
            }
            Database.OnProgressbarInfo(new ProgressbarEventArgs("Datenüberprüfung", rows.Count, rows.Count, false, true));
        }

        public RowItem? First() => _internal.Values.FirstOrDefault(thisRowItem => thisRowItem != null);

        public IEnumerator<RowItem> GetEnumerator() => _internal.Values.GetEnumerator();

        //foreach (var ThisRowItem in _Internal.Values)//{//    if (ThisRowItem != null) { return ThisRowItem; }//}//return null;
        IEnumerator IEnumerable.GetEnumerator() => IEnumerable_GetEnumerator();

        public bool Remove(long key) {
            var e = SearchByKey(key);
            if (e == null) { return false; }
            OnRowRemoving(new RowEventArgs(e));
            foreach (var thisColumnItem in Database.Column.Where(thisColumnItem => thisColumnItem != null)) {
                Database.Cell.Delete(thisColumnItem, key);
            }
            if (!_internal.TryRemove(key, out _)) { Develop.DebugPrint(enFehlerArt.Fehler, "Remove Failed"); }
            OnRowRemoved();
            return true;
        }

        public bool Remove(FilterItem filter, List<RowItem?> pinned) {
            FilterCollection nf = new(Database)
            {
                filter
            };
            return Remove(nf, pinned);
        }

        public bool Remove(FilterCollection? filter, List<RowItem>? pinned) {
            var keys = (from thisrowitem in _internal.Values where thisrowitem != null && thisrowitem.MatchesTo(filter) select thisrowitem.Key).Select(dummy => dummy).ToList();
            var did = keys.Count(thisKey => Remove(thisKey)) > 0;

            did = pinned?.Count(thisRow => Remove(thisRow)) > 0 || did;

            return did;
        }

        public bool Remove(RowItem? row) => row != null && Remove(row.Key);

        public bool RemoveOlderThan(float inHours) {
            var x = (from thisrowitem in _internal.Values where thisrowitem != null let d = thisrowitem.CellGetDateTime(Database.Column.SysRowCreateDate) where DateTime.Now.Subtract(d).TotalHours > inHours select thisrowitem.Key).Select(dummy => dummy).ToList();
            //foreach (var thisrowitem in _Internal.Values)
            //{
            //    if (thisrowitem != null)
            //    {
            //        var D = thisrowitem.CellGetDateTime(Database.Column.SysRowCreateDate());
            //        if (DateTime.Now.Subtract(D).TotalHours > InHours) { x.Add(thisrowitem.Key); }
            //    }
            //}
            if (x.Count == 0) { return false; }
            foreach (var thisKey in x) {
                Remove(thisKey);
            }
            return true;
        }

        public RowItem? SearchByKey(long key) {
            try {
                return key < 0 ? null : !_internal.ContainsKey(key) ? null : _internal[key];
            } catch {
                return SearchByKey(key);
            }
        }

        internal static List<RowItem?> MatchesTo(FilterItem filterItem) {
            List<RowItem?> l = new();

            if (filterItem == null) { return l; }

            l.AddRange(filterItem.Database.Row.Where(thisRow => thisRow.MatchesTo(filterItem)));
            return l;
        }

        internal static List<RowItem> MatchesTo(List<FilterItem>? filterItem) {
            List<RowItem> l = new();

            if (filterItem == null || filterItem.Count < 1) { return l; }

            l.AddRange(filterItem[0].Database.Row.Where(thisRow => thisRow.MatchesTo(filterItem)));
            return l;
        }

        //internal string Load_310(enDatabaseDataType type, string value) {
        //    switch (type) {
        //        case enDatabaseDataType.LastRowKey:
        //            //_LastRowKey = long.Parse(value);
        //            break;

        //        default:
        //            if (type.ToString() == ((int)type).ToString()) {
        //                Develop.DebugPrint(enFehlerArt.Info, "Laden von Datentyp '" + type + "' nicht definiert.<br>Wert: " + value + "<br>Datei: " + Database.Filename);
        //            } else {
        //                return "Interner Fehler: Für den Datentyp  '" + type + "'  wurde keine Laderegel definiert.";
        //            }
        //            break;
        //    }
        //    return string.Empty;
        //}

        internal long NextRowKey() {
            var x = DateTime.UtcNow.AddYears(-2020).Ticks;

            var s = Generic.UserName() + "\r\n" + Thread.CurrentThread.ManagedThreadId + "\r\n" + Environment.MachineName;

            var tmp = 0;
            long key;

            do {
                tmp++;
                key = x + (s.GetHashCode() * 100000000) + tmp;
                if (key < 0) { key *= -1; }
            } while (SearchByKey(key) != null);
            return key;
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

        internal void RemoveNullOrEmpty() => _internal.RemoveNullOrEmpty();

        //internal void Repair() {
        //    foreach (var ThisRowItem in _Internal.Values) {
        //        if (ThisRowItem != null) {
        //            //ThisRowItem.Repair();
        //            _LastRowKey = Math.Max(_LastRowKey, ThisRowItem.Key); // Die Letzte ID ermitteln,falls der gleadene Wert fehlerhaft ist
        //        }
        //    }
        //}

        //internal void SaveToByteList(List<byte> l) => Database.SaveToByteList(l, enDatabaseDataType.LastRowKey, _LastRowKey.ToString());

        private void Database_Disposing(object sender, System.EventArgs e) => Dispose();

        private void Dispose(bool disposing) {
            if (!_disposedValue) {
                if (disposing) {
                    // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
                }
                Database.Disposing -= Database_Disposing;
                Database = null;
                _internal.Clear();
                // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
                // TODO: Große Felder auf NULL setzen
                _disposedValue = true;
            }
        }

        private IEnumerator IEnumerable_GetEnumerator() => _internal.Values.GetEnumerator();

        private void OnDoSpecialRules(object sender, DoRowAutomaticEventArgs e) => DoSpecialRules?.Invoke(this, e);

        private void OnRowChecked(object sender, RowCheckedEventArgs e) => RowChecked?.Invoke(this, e);

        #endregion
    }
}