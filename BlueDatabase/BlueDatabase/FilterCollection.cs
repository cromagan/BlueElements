// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using BlueDatabase.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BlueDatabase;

public sealed class FilterCollection : IEnumerable<FilterItem>, IParseable, IHasDatabase, IDisposableExtended, INotifyPropertyChanged, IReadableText, IEditable, IErrorCheckable {

    #region Fields

    //TODO: Kommentar wieder entfernen
    private readonly string _coment;

    private readonly List<FilterItem> _internal = [];

    private Database? _database;

    private List<RowItem>? _rows;

    #endregion

    #region Constructors

    public FilterCollection(string c) : this(null as Database, c) { }

    public FilterCollection(Database? database, string coment) {
        Database = database;
        _coment = coment;
    }

    /// <summary>
    /// Erstellt einen Filter, der den Zeilenschlüssel sucht. Mit der SplitColumn als zweiten Filter
    /// </summary>
    public FilterCollection(RowItem r, string coment) {
        _coment = coment;
        if (r.Database is not { IsDisposed: false } db) {
            Develop.DebugPrint(ErrorType.Error, "Fehler im Filter");
            return;
        }

        Database = db;

        if (db.Column.ChunkValueColumn is { IsDisposed: false } spc) {
            Add(new FilterItem(spc, FilterType.Istgleich, r.CellGetString(spc)));
        }

        Add(new FilterItem(db, FilterType.RowKey, r.KeyName));
    }

    public FilterCollection(FilterItem? fi, string coment) : this(fi?.Database, coment) {
        if (fi != null) { Add(fi); }
    }

    /// <summary>
    /// Erstellt eine FilterCollection aus einer vollständigen SQL-SELECT-Anweisung
    /// Beispiel: "SELECT * FROM Kunden WHERE Name = 'Mueller' AND Status = 'Aktiv'"
    /// </summary>
    /// <param name="sql">Die vollständige SQL-SELECT-Anweisung</param>
    /// <param name="comment">Kommentar für den Filter</param>
    public FilterCollection(string sql, string comment) {
        _coment = comment;

        if (string.IsNullOrWhiteSpace(sql)) {
            Database = null;
            return;
        }

        try {
            // SQL normalisieren und in Großbuchstaben konvertieren
            (var normalizedSql, var error) = sql.NormalizedText(true, true, true, false, ' ');

            if (!string.IsNullOrEmpty(error)) {
                Develop.DebugPrint(ErrorType.Error, "Fehler beim Normalisieren der SQL-Anweisung: " + error);
                Database = null;
                return;
            }

            // Datenbank anhand des Tabellennamens finden
            Database = GetTableFromSql(normalizedSql);
            if (Database == null) {
                Develop.DebugPrint(ErrorType.Warning, "Tabelle nicht gefunden in SQL: " + sql);
                return;
            }

            // WHERE-Klausel extrahieren und parsen
            var whereClause = ExtractWhereClauseFromSql(normalizedSql);
            if (!string.IsNullOrWhiteSpace(whereClause)) {
                ParseSqlWhereClause(whereClause);
            }
        } catch (Exception ex) {
            Develop.DebugPrint(ErrorType.Error, "Fehler beim Parsen der SQL-Anweisung: " + ex.Message);
            Database = null;
        }
    }

    #endregion

    #region Destructors

    ~FilterCollection() { Dispose(disposing: false); }

    #endregion

    #region Events

    public event EventHandler? DisposingEvent;

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler? PropertyChanging;

    public event EventHandler? RowsChanged;

    #endregion

    #region Properties

    public string CaptionForEditor => "Filter-Sammlung";

    public string ChunkVal {
        get {
            if (Database?.Column.ChunkValueColumn is not { IsDisposed: false } cvc) { return string.Empty; }
            return InitValue(cvc, true, this.ToArray()) ?? string.Empty;
        }
    }

    public int Count => IsDisposed ? 0 : _internal.Count;

    public Database? Database {
        get => _database;
        set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (_database == value) { return; }

            lock (_internal) {
                if (IsDisposed) { return; }
                OnChanging();
                UnRegisterDatabaseEvents();

                _database = value;
                _internal.Clear();

                RegisterDatabaseEvents();
                Invalidate_FilteredRows();
            }
            OnPropertyChanged();
        }
    }

    public Type? Editor { get; set; }

    public bool IsDisposed { get; private set; }

    public string RowFilterText {
        get {
            var f = this[null];
            return f?.SearchValue[0] ?? string.Empty;
        }
        set {
            if (IsDisposed || Database is not { IsDisposed: false }) { return; }

            if (this[null] is { } f) {
                if (f.SearchValue.Count == 1 && f.SearchValue[0] == value) { return; }
                Remove(f);
            }

            var fi = new FilterItem(Database, FilterType.Instr_UND_GroßKleinEgal, value);
            Add(fi);
        }
    }

    public ReadOnlyCollection<RowItem> Rows {
        get {
            if (IsDisposed || Database is not { IsDisposed: false }) { return new List<RowItem>().AsReadOnly(); }
            _rows ??= CalculateFilteredRows(_database, _internal.ToArray());
            return _rows.AsReadOnly();
        }
    }

    public RowItem? RowSingleOrNull {
        get {
            if (IsDisposed || Database is not { IsDisposed: false }) { return null; }
            _rows ??= CalculateFilteredRows(_database, _internal.ToArray());
            return _rows.Count != 1 ? null : _rows[0];
        }
    }

    public string Where {
        get {
            if (IsDisposed || Count == 0) { return string.Empty; }

            var conditions = new List<string>();

            foreach (var filter in _internal) {
                if (filter?.IsOk() == true) {
                    var condition = filter.Where;
                    if (!string.IsNullOrEmpty(condition)) {
                        conditions.Add(condition);
                    }
                }
            }

            return conditions.Count > 0 ? string.Join(" AND ", conditions) : string.Empty;
        }
        set {
            if (IsDisposed) { return; }

            OnChanging();
            Clear();

            if (!string.IsNullOrWhiteSpace(value) && Database is { IsDisposed: false }) {
                try {
                    ParseSqlWhereClause(value);
                } catch (Exception ex) {
                    Develop.DebugPrint(ErrorType.Error, "Fehler beim Setzen der WHERE-Klausel: " + ex.Message);
                }
            }

            OnPropertyChanged();
        }
    }

    #endregion

    #region Indexers

    public FilterItem? this[ColumnItem? column] =>
        _internal.Where(thisFilterItem => thisFilterItem != null && thisFilterItem.IsOk())
        .FirstOrDefault(thisFilterItem => thisFilterItem.Column == column);

    public FilterItem? this[int no] => no < 0 || no >= _internal.Count ? null : _internal[no];

    #endregion

    #region Methods

    public static List<RowItem> CalculateFilteredRows(Database? db, params FilterItem[] filter) {
        if (db == null || db.IsDisposed) { return []; }

        if (db.Column.ChunkValueColumn is { IsDisposed: false } spc) {
            if (InitValue(spc, true, filter) is { } i) {
                var ok = db.BeSureRowIsLoaded(i);
                if (!ok) { return []; }
            }
        }

        List<RowItem> tmpVisibleRows = [];
        var lockMe = new object();
        var hasError = false;

        try {
            _ = Parallel.ForEach(db.Row, (thisRowItem, state) => {
                try {
                    if (hasError) {
                        state.Break();
                        return;
                    }

                    if (thisRowItem is { IsDisposed: false } && thisRowItem.MatchesTo(filter)) {
                        lock (lockMe) {
                            if (!hasError) { // Double-check nach dem Lock
                                tmpVisibleRows.Add(thisRowItem);
                            }
                        }
                    }
                } catch {
                    hasError = true;
                    state.Break();
                }
            });
        } catch {
            hasError = true;
        }

        if (hasError) {
            Develop.CheckStackOverflow();
            return CalculateFilteredRows(db, filter);
        }

        return tmpVisibleRows;
    }

    /// <summary>
    /// Gibt den Wert zurück, der in eine neue Zeile reingeschrieben wird
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    public static string? InitValue(ColumnItem column, bool firstToo, params FilterItem[] filter) {
        if (filter == null || !filter.Any()) { return null; }
        if (column is not { IsDisposed: false }) { return null; }
        if (column.Database is not { IsDisposed: false } db) { return null; }

        if (!firstToo && db.Column.First == column) { return null; }

        if (column == db.Column.SysCorrect ||
            column == db.Column.SysRowChangeDate ||
            column == db.Column.SysRowChanger ||
            column == db.Column.SysRowCreator ||
            column == db.Column.SysRowCreateDate ||
            column == db.Column.SysLocked ||
            column == db.Column.SysRowState) { return null; }

        var fi = filter.Where(thisFilterItem => thisFilterItem != null && thisFilterItem.IsOk())
                              .FirstOrDefault(thisFilterItem => thisFilterItem.Column == column);

        if (fi is not {
            FilterType: not (not FilterType.Istgleich
                and not FilterType.Istgleich_GroßKleinEgal
                and not FilterType.Istgleich_ODER_GroßKleinEgal
                and not FilterType.Istgleich_UND_GroßKleinEgal
                and not FilterType.Instr
                and not FilterType.Instr_GroßKleinEgal
                and not FilterType.Instr_UND_GroßKleinEgal
                and not FilterType.Istgleich_MultiRowIgnorieren
                and not FilterType.Istgleich_GroßKleinEgal_MultiRowIgnorieren)
        }) { return null; }

        if (!column.MultiLine && fi.SearchValue.Count > 1) { return null; }

        return column.AutoCorrect(fi.SearchValue.JoinWithCr(), false);
    }

    public void Add(FilterItem fi) {
        if (IsDisposed) { return; }

        OnChanging();
        AddInternal(fi);
        Invalidate_FilteredRows();
        OnPropertyChanged("FilterItems");
    }

    public void AddIfNotExists(FilterItem fi) {
        if (Exists(fi)) { return; }
        Add(fi);
    }

    public void AddIfNotExists(List<FilterItem> filterItems) {
        if (IsDisposed || !filterItems.Any()) { return; }

        var newItems = filterItems.Where(item => !Exists(item)).ToList();

        if (newItems.Any()) {
            OnChanging();
            AddInternal(newItems);
            Invalidate_FilteredRows();
            OnPropertyChanged("FilterItems");
        }
    }

    public void ChangeTo(FilterItem? fi) {
        if (fi != null && fi.Database == _database && _internal.Count == 1 && Exists(fi)) { return; }
        if (fi == null && _database == null && _internal.Count == 0) { return; }

        OnChanging();

        if (_database != fi?.Database) {
            UnRegisterDatabaseEvents();
            _database = fi?.Database;
            RegisterDatabaseEvents();
        }

        _internal.Clear();

        if (fi != null) {
            AddInternal(fi);
        }
        Invalidate_FilteredRows();

        OnPropertyChanged("FilterItems");
    }

    /// <summary>
    /// Effizente Methode um wenige Events auszulösen
    /// </summary>
    /// <param name="fc"></param>
    public void ChangeTo(FilterCollection? fc) {
        if (!IsDifferentTo(fc, false)) { return; }

        OnChanging();

        // Datenbankwechsel nur bei Unterschieden durchführen
        if (_database != fc?.Database) {
            UnRegisterDatabaseEvents();
            _database = fc?.Database;
            RegisterDatabaseEvents();
        }

        _internal.Clear();

        if (fc != null) {
            // Vorhandene Filterliste direkt übernehmen statt einzeln zu prüfen und hinzuzufügen
            foreach (var thisf in fc.Where(f => f.IsOk())) {
                AddInternal(thisf);
            }

            // Rows-Übernahme optimieren - bei null einfach ungültig machen
            if (fc._rows != null) {
                _rows = [.. fc.Rows];
                OnRowsChanged();
            } else {
                Invalidate_FilteredRows();
            }
        } else {
            Invalidate_FilteredRows();
        }

        OnPropertyChanged("FilterItems");
    }

    public void Clear() {
        if (IsDisposed) { return; }
        if (_internal.Count == 0) { return; }

        OnChanging();
        _internal.Clear();
        Invalidate_FilteredRows();
        OnPropertyChanged("FilterItems");
    }

    /// <summary>
    /// Klont die Filter Collection. Vorberechnete Zeilen werden weitergegeben.
    /// </summary>
    /// <returns></returns>
    public object Clone(string c2) {
        var fc = new FilterCollection(Database, "Clone " + c2);

        fc.ChangeTo(this);

        return fc;
    }

    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public string ErrorReason() {
        foreach (var thisf in this) {
            var f = thisf.ErrorReason();

            if (!string.IsNullOrEmpty(f)) { return f; }

            if (_database != thisf.Database) {
                return "Filter haben unterschiedliche Datenbanken";
            }
        }

        if (_database?.Column.ChunkValueColumn is { } cvc && this.Count > 0) {
            if (string.IsNullOrEmpty(InitValue(cvc, true, this.ToArray()))) { return "Chunk-Wert Filter fehlt."; }
        }
        return string.Empty;
    }

    /// <summary>
    /// Prüft, ob der Filter sinngemäß vorhanden ist.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public bool Exists(FilterItem filter) {
        foreach (var thisFilter in _internal) {
            if (filter.Equals(thisFilter)) { return true; }
        }

        return false;
    }

    IEnumerator IEnumerable.GetEnumerator() => _internal.GetEnumerator();

    public IEnumerator<FilterItem> GetEnumerator() => _internal.GetEnumerator();

    public bool HasAlwaysFalse() {
        foreach (var thisFi in this) {
            if (thisFi.FilterType == FilterType.AlwaysFalse) { return true; }
        }
        return false;
    }

    public void Invalidate_FilteredRows() {
        if (_rows == null) { return; }
        _rows = null;
        OnRowsChanged();
    }

    public bool IsDifferentTo(FilterCollection? fc, bool ignoreDatabaseTag) {
        if (IsDisposed) { return false; }
        if (fc == this) { return false; }

        if (fc is not { IsDisposed: false }) { return true; }

        if (fc.Count != Count) { return true; }

        if (!ignoreDatabaseTag && _database != fc.Database) { return true; }

        foreach (var thisf in this) {
            if (!fc.Exists(thisf)) { return true; }
        }

        // Zweite Schleife obosolet, wenn alle vorhanden sind und Count gleich.

        return false;
    }

    public string IsNowEditable() => string.Empty;

    public bool IsRowFilterActiv() {
        var fi = this[null];

        return fi is { FilterType: FilterType.Instr or FilterType.Instr_UND_GroßKleinEgal or FilterType.Instr_GroßKleinEgal };
    }

    public bool MayHaveRowFilter(ColumnItem? column) => column is { IsDisposed: false, IgnoreAtRowFilter: false } && IsRowFilterActiv();

    public FilterCollection Normalized() {
        var tmp = new List<FilterItem>();

        foreach (var thisf in _internal) {
            tmp.Add(thisf.Normalized());
        }

        var fcn = new FilterCollection(Database, "Normalize");
        foreach (var thisf in tmp.OrderBy(e => e.ReadableText()).ToList()) {
            fcn.Add(thisf);
        }
        return fcn;
    }

    public void OnRowsChanged() {
        if (IsDisposed) { return; }
        //if(_rows == null) { return;}
        RowsChanged?.Invoke(this, System.EventArgs.Empty);
    }

    public List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [];

        foreach (var thisFilterItem in _internal) {
            if (thisFilterItem is { } && thisFilterItem.IsOk()) {
                result.ParseableAdd("Filter", thisFilterItem);
            }
        }
        return result;
    }

    public void ParseFinished(string parsed) { }

    public bool ParseThis(string key, string value) {
        switch (key) {
            case "filter":

                var fi = new FilterItem(value.FromNonCritical(), Database);
                if (!Exists(fi)) { AddInternal(fi); }

                return true;
        }
        return false;
    }

    public string ReadableText() {
        if (Count <= 0) { return "Kein Filter"; }

        var f = string.Empty;

        foreach (var thisf in _internal) {
            f += thisf.ReadableText() + "\r\n";
        }

        return f.TrimEnd("\r\n");
    }

    public void Remove(ColumnItem? column) {
        var toDel = _internal.Where(thisFilter => thisFilter.Column == column).ToList();
        if (toDel.Count == 0) { return; }
        RemoveRange(toDel);
    }

    public void Remove(FilterType filterType) {
        var toDel = _internal.Where(thisFilter => thisFilter.FilterType.HasFlag(filterType)).ToList();
        if (toDel.Count == 0) { return; }
        RemoveRange(toDel);
    }

    public void Remove(FilterItem filter) {
        if (IsDisposed) { return; }

        var existingColumnFilter = _internal.Where(thisFilter => thisFilter.Equals(filter)).ToList();

        if (existingColumnFilter.Any()) {
            OnChanging();
            foreach (var thisItem in existingColumnFilter) {
                _ = _internal.Remove(thisItem);
            }
            Invalidate_FilteredRows();
            OnPropertyChanged("FilterItems");
        }
    }

    public void Remove_RowFilter() => Remove(null as ColumnItem);

    public void RemoveOtherAndAdd(FilterItem fi, string? newOrigin) {
        var fin = fi;

        newOrigin ??= fi.Origin;

        if (fi.Origin != newOrigin) {
            fin = new FilterItem(fi.Database, fi.Column, fi.FilterType, fi.SearchValue, newOrigin);
        }

        RemoveOtherAndAdd(fin);
    }

    /// <summary>
    /// Ändert einen Filter mit der gleichen Spalte auf diesen Filter ab. Perfekt um so wenig Events wie möglich auszulösen
    /// </summary>
    public void RemoveOtherAndAdd(FilterItem? fi) {
        if (fi == null) { return; }
        if (IsDisposed) { return; }
        if (!fi.IsOk()) {
            Develop.DebugPrint(ErrorType.Error, "Filter Fehler: " + fi.ErrorReason());
            return;
        }

        var existingColumnFilter = _internal.Where(thisFilter => thisFilter.Column == fi.Column).ToList();

        if (existingColumnFilter.Count == 1) {
            if (Exists(fi)) { return; }
        }

        OnChanging();
        foreach (var thisItem in existingColumnFilter) {
            _ = _internal.Remove(thisItem);
        }
        AddInternal(fi);
        Invalidate_FilteredRows();
        OnPropertyChanged("FilterItems");
    }

    public void RemoveOtherAndAdd(FilterCollection? fc, string? newOrigin) {
        if (fc == null || IsDisposed || fc.Count == 0) { return; }

        // Sammle alle Filter, die hinzugefügt werden sollen
        var filtersToAdd = new List<FilterItem>();

        foreach (var thisFi in fc) {
            if (!thisFi.IsOk()) { continue; }

            var fin = newOrigin != null && thisFi.Origin != newOrigin
                ? new FilterItem(thisFi.Database, thisFi.Column, thisFi.FilterType, thisFi.SearchValue, newOrigin)
                : thisFi;

            filtersToAdd.Add(fin);
        }

        if (filtersToAdd.Count == 0) { return; }

        // Prüfe, ob tatsächlich Änderungen erforderlich sind und entferne bestehende Filter
        var needChanges = false;
        var toRemove = new List<FilterItem>();

        foreach (var filterToAdd in filtersToAdd) {
            var existingFilters = _internal.Where(f => f.Column == filterToAdd.Column).ToList();

            if (existingFilters.Count != 1 || !Exists(filterToAdd)) {
                needChanges = true;
                toRemove.AddRange(existingFilters);
            }
        }

        if (!needChanges) { return; }

        OnChanging();

        // Entferne die identifizierten Filter
        foreach (var filter in toRemove) {
            _ = _internal.Remove(filter);
        }

        // Füge neue Filter hinzu
        foreach (var filter in filtersToAdd) {
            AddInternal(filter);
        }

        Invalidate_FilteredRows();
        OnPropertyChanged("FilterItems");
    }

    public void RemoveRange(List<FilterItem> fi) {
        if (IsDisposed) { return; }

        if (fi.Count == 0) { return; }

        var did = false;
        foreach (var thisItem in fi) {
            if (Exists(thisItem)) {
                if (!did) {
                    OnChanging();
                    did = true;
                }
                _internal.Remove(thisItem);
            }
        }

        if (did) {
            Invalidate_FilteredRows();
            OnPropertyChanged("FilterItems");
        }
    }

    //    foreach (var thisItem in _internal) {
    //        if (thisItem.Origin.Equals(origin, StringComparison.OrdinalIgnoreCase)) {
    //            l.Add(thisItem);
    //        }
    //    }
    //    RemoveRange(l);
    //}
    public QuickImage SymbolForReadableText() {
        switch (Count) {
            case 0:
                return QuickImage.Get(ImageCode.Kreuz, 16);

            case 1:
                return QuickImage.Get(ImageCode.Trichter, 16);

            default:
                return QuickImage.Get(ImageCode.Trichter, 16, Color.Red, Color.Transparent);
        }
    }

    //public void RemoveRange(string origin) {
    //    var l = new List<FilterItem>();
    public List<FilterItem> ToList() => _internal;

    public override string ToString() => ParseableItems().FinishParseable();

    /// <summary>
    /// Extrahiert die WHERE-Klausel aus einer bereits normalisierten SQL-SELECT-Anweisung
    /// </summary>
    /// <param name="normalizedSql">Die bereits normalisierte SQL-Anweisung</param>
    /// <returns>Die WHERE-Klausel ohne das Wort "WHERE" oder leerer String</returns>
    private static string ExtractWhereClauseFromSql(string normalizedSql) {
        if (string.IsNullOrWhiteSpace(normalizedSql)) { return string.Empty; }

        // WHERE-Position finden (da bereits uppercase: nur "WHERE")
        var whereIndex = normalizedSql.IndexOf(" WHERE ");

        if (whereIndex == -1) { return string.Empty; }

        // Alles nach WHERE extrahieren
        var whereClause = normalizedSql.Substring(whereIndex + 7); // 7 = " WHERE ".Length

        // Eventuelle ORDER BY, GROUP BY, HAVING entfernen
        var endKeywords = new[] { " ORDER BY ", " GROUP BY ", " HAVING ", " LIMIT " };

        foreach (var keyword in endKeywords) {
            var keywordIndex = whereClause.IndexOf(keyword);
            if (keywordIndex >= 0) {
                whereClause = whereClause.Substring(0, keywordIndex);
            }
        }

        return whereClause.Trim();
    }

    /// <summary>
    /// Extrahiert den Tabellennamen aus einer bereits normalisierten SQL-SELECT-Anweisung und gibt die Datenbank zurück
    /// </summary>
    /// <param name="normalizedSql">Die bereits normalisierte SQL-Anweisung</param>
    /// <returns>Die Datenbank oder null wenn nicht gefunden</returns>
    private static Database? GetTableFromSql(string normalizedSql) {
        if (string.IsNullOrWhiteSpace(normalizedSql)) { return null; }

        // FROM-Klausel suchen
        var fromMatch = System.Text.RegularExpressions.Regex.Match(normalizedSql,
            @"\bFROM\s+([^\s]+)");

        if (fromMatch.Success) {
            var tableName = fromMatch.Groups[1].Value;

            // Eventuelle Anführungszeichen oder eckige Klammern entfernen
            tableName = tableName.Trim('[', ']', '"', '\'');

            var validTableName = Database.MakeValidTableName(tableName);
            return Database.Get(validTableName, false, null);
        }

        return null;
    }

    /// <summary>
    /// Teilt die WHERE-Klausel in einzelne Bedingungen auf
    /// Vereinfachte Implementierung ohne Klammern-Unterstützung
    /// </summary>
    private static List<string> SplitConditions(string whereClause) {
        var conditions = new List<string>();

        // Einfache Aufteilung bei AND (da bereits uppercase: nur " AND ")
        var parts = whereClause.Split(new[] { " AND " }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts) {
            if (!string.IsNullOrWhiteSpace(part)) {
                conditions.Add(part.Trim());
            }
        }

        return conditions;
    }

    private void _Database_CellValueChanged(object sender, CellEventArgs e) {
        if (_rows == null) { return; }
        if (e.Row.IsDisposed || e.Column.IsDisposed) { return; }

        if (e.Row.MatchesTo(_internal.ToArray()) != _rows.Contains(e.Row)) {
            Invalidate_FilteredRows();
        }

        //if ((this[e.Column] != null) ||
        //     MayHasRowFilter(e.Column)
        // ) {
        //    Invalidate_FilteredRows();
        //}
    }

    private void _database_Disposing(object sender, System.EventArgs e) => Dispose();

    private void _database_Loaded(object sender, System.EventArgs e) => Invalidate_FilteredRows();

    /// <summary>
    /// Löst keine Ereignisse aus, rows werden NICHT invalidiert
    /// </summary>
    /// <param name="fi"></param>
    private void AddInternal(FilterItem fi) {
        if (_internal.Count > 0) {
            foreach (var internalFilter in _internal) {
                if (fi.FilterType != FilterType.AlwaysFalse && internalFilter.FilterType != FilterType.AlwaysFalse) {
                    if (internalFilter.Database != fi.Database) {
                        fi = new FilterItem(null, "AddInternal");
                        break;
                    }
                }
            }
        }

        if (fi.Database != Database && fi.FilterType != FilterType.AlwaysFalse) {
            fi = new FilterItem(null, "AddInternal");
        }

        _internal.Add(fi);
    }

    /// <summary>
    /// Es werden keine Events ausgelöst und Zeilen nicht invalidiert
    /// </summary>
    /// <param name="fi"></param>
    private void AddInternal(List<FilterItem> fi) {
        foreach (var thisfio in fi) {
            AddInternal(thisfio);
        }
    }

    private void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                lock (_internal) {
                    OnDisposingEvent();
                    UnRegisterDatabaseEvents();
                    _database = null;
                    Invalidate_FilteredRows();
                }
            }
            _internal.Clear();
            IsDisposed = true;
        }
    }

    private void OnChanging() {
        if (IsDisposed) { return; }
        PropertyChanging?.Invoke(this, System.EventArgs.Empty);
    }

    private void OnDisposingEvent() => DisposingEvent?.Invoke(this, System.EventArgs.Empty);

    private void OnPropertyChanged([CallerMemberName] string propertyName = "unknown") {
        if (IsDisposed) { return; }
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Parst eine bereits normalisierte SQL-WHERE-Klausel und erstellt entsprechende FilterItems
    /// </summary>
    /// <param name="whereClause">Die bereits normalisierte WHERE-Klausel</param>
    private void ParseSqlWhereClause(string whereClause) {
        if (Database is not { IsDisposed: false } db) { return; }

        try {
            // Aufteilen in einzelne Bedingungen (vereinfacht, ohne Klammern-Parsing)
            var conditions = SplitConditions(whereClause);

            foreach (var condition in conditions) {
                var filterItem = new FilterItem(db, condition.Trim());
                if (filterItem.IsOk()) {
                    AddInternal(filterItem);
                }
            }

            Invalidate_FilteredRows();
        } catch (Exception ex) {
            Develop.DebugPrint(ErrorType.Warning, $"Fehler beim Parsen der SQL-WHERE-Klausel: {ex.Message}");
        }
    }

    private void RegisterDatabaseEvents() {
        if (_database != null) {
            _database.Loaded += _database_Loaded;
            _database.DisposingEvent += _database_Disposing;
            _database.Row.RowRemoved += Row_RowRemoved;
            //_database.Row.RowRemoved += Row_RowRemoving;
            _database.Row.RowAdded += Row_Added;
            _database.Cell.CellValueChanged += _Database_CellValueChanged;
        }
    }

    private void Row_Added(object sender, RowEventArgs e) {
        if (IsDisposed || Database is not { IsDisposed: false }) { return; }
        if (_rows == null) { return; }
        if (e.Row.MatchesTo(_internal.ToArray())) { Invalidate_FilteredRows(); }
    }

    private void Row_RowRemoved(object sender, RowEventArgs e) {
        if (IsDisposed || Database is not { IsDisposed: false }) { return; }
        if (_rows == null) { return; }
        if (_rows.Contains(e.Row)) { Invalidate_FilteredRows(); }
    }

    private void UnRegisterDatabaseEvents() {
        if (_database != null) {
            _database.Loaded -= _database_Loaded;
            _database.DisposingEvent -= _database_Disposing;
            _database.Row.RowRemoved -= Row_RowRemoved;
            _database.Row.RowAdded -= Row_Added;
            _database.Cell.CellValueChanged -= _Database_CellValueChanged;
        }
    }

    #endregion
}