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
using System.Threading.Tasks;

namespace BlueDatabase;

public sealed class SQL_RowCollection : IEnumerable<SQL_RowItem>, IDisposable {

    #region Fields

    private readonly ConcurrentDictionary<long, SQL_RowItem?> _internal = new();

    private bool _disposedValue;

    #endregion

    #region Constructors

    public SQL_RowCollection(SQL_Database database) {
        Database = database;
        Database.Disposing += Database_Disposing;
        //Initialize();
    }

    #endregion

    #region Destructors

    // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    ~SQL_RowCollection() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(false);
    }

    #endregion

    #region Events

    public event EventHandler<SQL_DoRowAutomaticEventArgs> DoSpecialRules;

    public event EventHandler<SQL_RowEventArgs> RowAdded;

    public event EventHandler<SQL_RowCheckedEventArgs> RowChecked;

    public event EventHandler RowRemoved;

    public event EventHandler<SQL_RowEventArgs> RowRemoving;

    #endregion

    #region Properties

    public int Count => _internal.Count;
    public SQL_Database? Database { get; private set; }

    public long VisibleRowCount { get; private set; }

    #endregion

    #region Indexers

    /// <summary>
    /// Durchsucht die erste (interne) Spalte der Datenbank nach dem hier angegebenen Prmärschlüssel.
    /// </summary>
    /// <param name="primärSchlüssel">Der Primärschlüssel, nach dem gesucht werden soll. Groß/Kleinschreibung wird ignoriert.</param>
    /// <returns>Die Zeile, dessen erste Spalte den Primärschlüssel enthält oder - falls nicht gefunden - NULL.</returns>
    public SQL_RowItem this[string primärSchlüssel] => this[new SQL_FilterItem(Database.Column[0], FilterType.Istgleich_GroßKleinEgal | FilterType.MultiRowIgnorieren, primärSchlüssel)];

    public SQL_RowItem? this[params SQL_FilterItem[]? filter] {
        get {
            if (filter == null || filter.Length == 0) {
                Develop.DebugPrint("Kein Filter angekommen!");
                return null;
            }
            SQL_FilterCollection d = new(filter[0].Database);
            d.AddRange(filter);
            return this[d];
        }
    }

    public SQL_RowItem this[SQL_FilterCollection? filter] => Database == null ? null : _internal.Values.FirstOrDefault(thisRow => thisRow != null && thisRow.MatchesTo(filter));

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
            if (x > 99999) { Develop.DebugPrint(FehlerArt.Fehler, "Unique ID konnte nicht erzeugt werden"); }

            var unique = ("X" + DateTime.Now.ToString("mm.fff") + x.ToString(Constants.Format_Integer5)).RemoveChars(Constants.Char_DateiSonderZeichen + ".");
            var ok = true;

            foreach (var thisfile in SQL_Database.AllFiles) {
                var row = thisfile.Row[unique];
                if (row != null) { ok = false; break; }
            }

            if (ok) { return unique; }
        } while (true);
    }

    public void Add(SQL_RowItem row) {
        if (!_internal.TryAdd(row.Key, row)) { Develop.DebugPrint(FehlerArt.Fehler, "Add Failed"); }
        OnRowAdded(new SQL_RowEventArgs(row));
    }

    public SQL_RowItem? Add(string valueOfCellInFirstColumn) => Add(valueOfCellInFirstColumn, true);

    /// <summary>
    /// Erstellt eine neue Spalte mit den aus den Filterkriterien. nur Fiter IstGleich wird unterstützt.
    /// Schägt irgendetwas fehl, wird NULL zurückgegeben.
    /// Ist ein Filter mehrfach vorhanden, erhält die Zelle den LETZTEN Wert.
    /// Am Schluß wird noch das Skript ausgeführt.
    /// </summary>
    /// <param name="fi"></param>
    /// <returns></returns>
    public SQL_RowItem? Add(List<SQL_FilterItem>? fi) {
        List<string> first = null;

        foreach (var thisfi in fi) {
            if (Database.Column.First() == thisfi.Column) {
                if (first != null) { return null; }
                first = thisfi.SearchValue;
            }
            if (thisfi.FilterType != FilterType.Istgleich) { return null; }
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

    public SQL_RowItem? Add(string valueOfCellInFirstColumn, bool runScriptOfNewRow) {
        Develop.DebugPrint_Disposed(Database);

        if (string.IsNullOrEmpty(valueOfCellInFirstColumn)) {
            Develop.DebugPrint("Value = 0");
            return null;
        }

        SQL_RowItem row = new(Database!);
        Add(row);
        row.CellSet(Database!.Column[0], valueOfCellInFirstColumn);
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
    public List<SQL_RowItem> CalculateFilteredRows(List<SQL_FilterItem>? filter) {
        List<SQL_RowItem> tmpVisibleRows = new();
        if (Database == null) { return tmpVisibleRows; }

        try {
            var lockMe = new object();
            Parallel.ForEach(Database.Row, thisSQL_RowItem => {
                if (thisSQL_RowItem != null) {
                    if (thisSQL_RowItem.MatchesTo(filter)) {
                        lock (lockMe) { tmpVisibleRows.Add(thisSQL_RowItem); }
                    }
                }
            });
        } catch {
            return CalculateFilteredRows(filter);
        }

        return tmpVisibleRows;
    }

    public List<SQL_RowData?> CalculateSortedRows(List<SQL_FilterItem>? filter, SQL_RowSortDefinition rowSortDefinition, List<SQL_RowItem>? pinnedRows, List<SQL_RowData>? reUseMe) => CalculateSortedRows(CalculateFilteredRows(filter), rowSortDefinition, pinnedRows, reUseMe);

    public List<SQL_RowData?> CalculateSortedRows(List<SQL_RowItem?> filteredRows, SQL_RowSortDefinition? rowSortDefinition, List<SQL_RowItem>? pinnedRows, List<SQL_RowData>? reUseMe) {
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

        List<SQL_RowData> pinnedData = new();
        if (pinnedRows != null) {
            Parallel.ForEach(pinnedRows, thisRow => {
                var rd = reUseMe.Get(thisRow, "Angepinnt") is SQL_RowData r ? r : new SQL_RowData(thisRow, "Angepinnt");
                rd.PinStateSortAddition = "1";
                rd.MarkYellow = true;
                rd.AdditionalSort = rowSortDefinition == null ? thisRow.CompareKey(null) : thisRow.CompareKey(rowSortDefinition.Columns);

                lock (lockMe) {
                    VisibleRowCount++;
                    pinnedData.Add(rd);
                }
            });
        }

        #endregion

        #region Gefiltere Zeilen erstellen (_SQL_RowData)

        List<SQL_RowData?> SQL_RowData = new();
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
                var rd = reUseMe.Get(thisRow, thisCap) is SQL_RowData r ? r : new SQL_RowData(thisRow, thisCap);

                rd.PinStateSortAddition = "2";
                rd.MarkYellow = markYellow;
                rd.AdditionalSort = adk;
                lock (lockMe) {
                    SQL_RowData.Add(rd);
                    if (!added) { VisibleRowCount++; added = true; }
                }
            }
        });

        #endregion

        pinnedData.Sort();
        SQL_RowData.Sort();

        if (rowSortDefinition != null && rowSortDefinition.Reverse) { SQL_RowData.Reverse(); }

        SQL_RowData.InsertRange(0, pinnedData);
        return SQL_RowData;
    }

    /// <summary>
    /// Gibt die mit dieser Kombination sichtbaren Zeilen zurück. Ohne Sortierung. Jede Zeile kann maximal einmal vorkommen.
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="pinnedRows"></param>
    /// <returns></returns>
    public List<SQL_RowItem> CalculateVisibleRows(List<SQL_FilterItem>? filter, List<SQL_RowItem?>? pinnedRows) {
        List<SQL_RowItem> tmpVisibleRows = new();
        if (Database == null) { return tmpVisibleRows; }

        pinnedRows ??= new List<SQL_RowItem?>();

        var lockMe = new object();
        Parallel.ForEach(Database.Row, thisSQL_RowItem => {
            if (thisSQL_RowItem != null) {
                if (thisSQL_RowItem.MatchesTo(filter) || pinnedRows.Contains(thisSQL_RowItem)) {
                    lock (lockMe) { tmpVisibleRows.Add(thisSQL_RowItem); }
                }
            }
        });

        //foreach (var thisSQL_RowItem in Database.Row) {
        //    if (thisSQL_RowItem != null) {
        //        if (thisSQL_RowItem.MatchesTo(filter) || pinnedRows.Contains(thisSQL_RowItem)) {
        //            _tmpVisibleRows.Add(thisSQL_RowItem);
        //        }
        //    }
        //}

        return tmpVisibleRows;
    }

    public bool Clear() => Remove(new SQL_FilterCollection(Database), null);

    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public string DoAutomatic(SQL_FilterCollection? filter, bool fullCheck, List<SQL_RowItem?>? pinned, string startroutine) {
        if (Database == null || Database.ReadOnly) { return "Datenbank schreibgeschützt."; }
        return DoAutomatic(CalculateVisibleRows(filter, pinned), fullCheck, startroutine);
    }

    public string DoAutomatic(List<SQL_RowItem>? rows, bool fullCheck, string startroutine) {
        if (Database == null || Database.ReadOnly) { return "Datenbank schreibgeschützt."; }

        if (rows == null || rows.Count == 0) { return "Keine Zeilen angekommen."; }

        Database.OnProgressbarInfo(new ProgressbarEventArgs("Datenüberprüfung", 0, rows.Count, true, false));

        var all = rows.Count;
        while (rows.Count > 0) {
            Database.OnProgressbarInfo(new ProgressbarEventArgs("Datenüberprüfung", all - rows.Count, all, false, false));

            var (checkPerformed, _, s) = rows[0].DoAutomatic(true, fullCheck, startroutine);

            if (s != null && !string.IsNullOrEmpty(s.Error)) {
                var w = rows[0].CellFirstString();
                rows.Clear();
                Database.OnDropMessage(FehlerArt.Warnung, "Skript fehlerhaft bei " + w + "\r\n" + s.Error);
                return "Skript fehlerhaft bei " + w + "\r\n" + s.Error;
            }
            if (checkPerformed) { rows.RemoveAt(0); }
        }
        Database.OnProgressbarInfo(new ProgressbarEventArgs("Datenüberprüfung", rows.Count, rows.Count, false, true));
        return string.Empty;
    }

    public SQL_RowItem? First() => _internal.Values.FirstOrDefault(thisSQL_RowItem => thisSQL_RowItem != null);

    public IEnumerator<SQL_RowItem> GetEnumerator() => _internal.Values.GetEnumerator();

    //foreach (var ThisSQL_RowItem in _Internal.Values)//{//    if (ThisSQL_RowItem != null) { return ThisSQL_RowItem; }//}//return null;
    IEnumerator IEnumerable.GetEnumerator() => IEnumerable_GetEnumerator();

    public bool Remove(long key) {
        var e = SearchByKey(key);
        if (e == null) { return false; }
        OnRowRemoving(new SQL_RowEventArgs(e));
        foreach (var thisColumnItem in Database.Column.Where(thisColumnItem => thisColumnItem != null)) {
            Database.Cell.Delete(thisColumnItem, key);
        }
        if (!_internal.TryRemove(key, out _)) { Develop.DebugPrint(FehlerArt.Fehler, "Remove Failed"); }
        OnRowRemoved();
        return true;
    }

    public bool Remove(SQL_FilterItem filter, List<SQL_RowItem?> pinned) {
        SQL_FilterCollection nf = new(Database)
        {
            filter
        };
        return Remove(nf, pinned);
    }

    public bool Remove(SQL_FilterCollection? filter, List<SQL_RowItem>? pinned) {
        var keys = (from thisSQL_RowItem in _internal.Values where thisSQL_RowItem != null && thisSQL_RowItem.MatchesTo(filter) select thisSQL_RowItem.Key).Select(dummy => dummy).ToList();
        var did = keys.Count(thisKey => Remove(thisKey)) > 0;

        did = pinned?.Count(thisRow => Remove(thisRow)) > 0 || did;

        return did;
    }

    public bool Remove(SQL_RowItem? row) => row != null && Remove(row.Key);

    public bool RemoveOlderThan(float inHours) {
        var x = (from thisSQL_RowItem in _internal.Values where thisSQL_RowItem != null let d = thisSQL_RowItem.CellGetDateTime(Database.Column.SysRowCreateDate) where DateTime.Now.Subtract(d).TotalHours > inHours select thisSQL_RowItem.Key).Select(dummy => dummy).ToList();
        //foreach (var thisSQL_RowItem in _Internal.Values)
        //{
        //    if (thisSQL_RowItem != null)
        //    {
        //        var D = thisSQL_RowItem.CellGetDateTime(Database.Column.SysRowCreateDate());
        //        if (DateTime.Now.Subtract(D).TotalHours > InHours) { x.Add(thisSQL_RowItem.Key); }
        //    }
        //}
        if (x.Count == 0) { return false; }
        foreach (var thisKey in x) {
            Remove(thisKey);
        }
        return true;
    }

    public SQL_RowItem? SearchByKey(long key) {
        try {
            return key < 0 ? null : !_internal.ContainsKey(key) ? null : _internal[key];
        } catch {
            return SearchByKey(key);
        }
    }

    internal static List<SQL_RowItem> MatchesTo(SQL_FilterItem? filterItem) {
        List<SQL_RowItem> l = new();

        if (filterItem == null) { return l; }

        l.AddRange(filterItem.Database.Row.Where(thisRow => thisRow.MatchesTo(filterItem)));
        return l;
    }

    internal static List<SQL_RowItem> MatchesTo(List<SQL_FilterItem>? filterItem) {
        List<SQL_RowItem> l = new();

        if (filterItem == null || filterItem.Count < 1) { return l; }

        l.AddRange(filterItem[0].Database.Row.Where(thisRow => thisRow.MatchesTo(filterItem)));
        return l;
    }

    //internal string Load_310(enDatabaseDataType type, string value) {
    //    switch (type) {
    //        case enDatabaseDataType.LastRowKey:
    //            //_LastRowKey = LongParse(value);
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
        var tmp = 0;
        long key;

        do {
            key = Generic.GetUniqueKey(tmp, "row");
            tmp++;
        } while (SearchByKey(key) != null);
        return key;
    }

    internal void OnRowAdded(SQL_RowEventArgs e) {
        e.Row.RowChecked += OnRowChecked;
        e.Row.DoSpecialRules += OnDoSpecialRules;
        RowAdded?.Invoke(this, e);
    }

    internal void OnRowRemoved() => RowRemoved?.Invoke(this, System.EventArgs.Empty);

    internal void OnRowRemoving(SQL_RowEventArgs e) {
        e.Row.RowChecked -= OnRowChecked;
        e.Row.DoSpecialRules -= OnDoSpecialRules;
        RowRemoving?.Invoke(this, e);
    }

    internal void RemoveNullOrEmpty() => _internal.RemoveNullOrEmpty();

    //internal void Repair() {
    //    foreach (var ThisSQL_RowItem in _Internal.Values) {
    //        if (ThisSQL_RowItem != null) {
    //            //ThisSQL_RowItem.Repair();
    //            _LastRowKey = Math.Max(_LastRowKey, ThisSQL_RowItem.Key); // Die Letzte ID ermitteln,falls der gleadene Wert fehlerhaft ist
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

    private void OnDoSpecialRules(object sender, SQL_DoRowAutomaticEventArgs e) => DoSpecialRules?.Invoke(this, e);

    private void OnRowChecked(object sender, SQL_RowCheckedEventArgs e) => RowChecked?.Invoke(this, e);

    #endregion
}