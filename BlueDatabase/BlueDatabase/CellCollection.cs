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
using BlueScript.Variables;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static BlueBasics.Generic;

namespace BlueDatabase;

public sealed class CellCollection : ConcurrentDictionary<string, CellItem>, IDisposableExtended, IHasDatabase {

    #region Fields

    private Database? _database;

    #endregion

    #region Constructors

    public CellCollection(Database database) : base() => Database = database;

    #endregion

    #region Destructors

    // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    ~CellCollection() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(false);
    }

    #endregion

    #region Events

    public event EventHandler<CellEventArgs>? CellValueChanged;

    #endregion

    #region Properties

    public Database? Database {
        get => _database;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == _database) { return; }

            if (_database != null) {
                _database.DisposingEvent -= _database_Disposing;
            }
            _database = value;

            if (_database != null) {
                _database.DisposingEvent += _database_Disposing;
            }
        }
    }

    public bool IsDisposed { get; private set; }

    #endregion

    #region Indexers

    /// <summary>
    /// Gibt die Zelle zurück.
    /// Da leere Zellen nicht gespeichert werden, kann null zuück gebeben werden, obwohl die Zelle an sich gültig ist.
    /// </summary>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <returns></returns>
    public CellItem? this[ColumnItem? column, RowItem? row] {
        get {
            if (IsDisposed || Database is not { IsDisposed: false } db) { return null; }
            if (column is not { IsDisposed: false }) { return null; }
            if (row is not { IsDisposed: false }) { return null; }

            if (column.Database != row.Database || column.Database != db) { return null; }

            var cellKey = KeyOfCell(column, row);
            return ContainsKey(cellKey) ? this[cellKey] : null;
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gibt einen Fehlergrund zurück, ob die Zelle bearbeitet werden kann.
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <param name="mode"></param>
    /// <param name="checkUserRights">Ob vom Benutzer aktiv das Feld bearbeitet werden soll. false bei internen Prozessen angeben.</param>
    /// <param name="checkEditmode">Ob gewünscht wird, dass die intern programmierte Routine geprüft werden soll. Nur in Datenbankansicht empfohlen.</param>
    /// <param name="repairallowed"></param>
    /// <param name="ignoreLinked"></param>
    /// <returns></returns>
    public static string EditableErrorReason(string oldChunkValue, string newChunkValue, ColumnItem? column, RowItem? row, EditableErrorReasonType mode, bool checkUserRights, bool checkEditmode, bool repairallowed, bool ignoreLinked) {
        if (mode == EditableErrorReasonType.OnlyRead) {
            if (column == null || row == null) { return string.Empty; }
        }

        if (column?.Database is not { IsDisposed: false } db) { return "Es ist keine Spalte ausgewählt."; }

        if (row is { IsDisposed: true }) { return "Die Zeile wurde verworfen."; }

        var f = column.EditableErrorReason(mode, checkEditmode);
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (column.RelationType == RelationType.CellValues) {
            if (ignoreLinked) { return string.Empty; }
            //repairallowed = repairallowed && mode is EditableErrorReasonType.EditAcut or EditableErrorReasonType.EditCurrently;

            var (lcolumn, lrow, info, canrepair) = LinkedCellData(column, row, repairallowed, mode is EditableErrorReasonType.EditAcut or EditableErrorReasonType.EditCurrently);

            if (!string.IsNullOrEmpty(info) && !canrepair) { return info; }

            if (lcolumn?.Database is not { IsDisposed: false } db2) { return "Verknüpfte Datenbank verworfen."; }

            db2.PowerEdit = db.PowerEdit;

            if (lrow != null) {
                var chunkval = lrow.ChunkValue;
                var tmp = EditableErrorReason(chunkval, chunkval, lcolumn, lrow, mode, checkUserRights, checkEditmode, false, false);
                return string.IsNullOrEmpty(tmp)
                    ? string.Empty
                    : "Die verlinkte Zelle kann nicht bearbeitet werden: " + tmp;
            }

            if (canrepair) { return lcolumn.EditableErrorReason(mode, checkEditmode); }

            return "Allgemeiner Fehler.";
        }

        if (db.Column.ChunkValueColumn is { IsDisposed: false } spc) {
            //if (string.IsNullOrEmpty(newChunkValue)) {
            //    return "Bei Split-Datenbanken muss ein Chunk-Wert in der Split-Spalte sein.";
            //}

            if (column == spc) {
                var f1 = db.IsValueEditable(DatabaseDataType.UTF8Value_withoutSizeData, oldChunkValue, mode);
                if (!string.IsNullOrEmpty(f1)) { return f1; }
            }

            var f2 = db.IsValueEditable(DatabaseDataType.UTF8Value_withoutSizeData, newChunkValue, mode);
            if (!string.IsNullOrEmpty(f2)) { return f2; }
        }

        if (row == null) {
            if (db.Column.First() is not { IsDisposed: false } firstcol || firstcol != column) {
                return "Neue Zeilen müssen mit der ersten Spalte beginnen.";
            }

            if (checkUserRights && !db.PermissionCheck(db.PermissionGroupsNewRow, null)) {
                return "Sie haben nicht die nötigen Rechte, um neue Zeilen anzulegen.";
            }

            if (db.Column.ChunkValueColumn is { IsDisposed: false }) {
                if (string.IsNullOrEmpty(newChunkValue) && mode != EditableErrorReasonType.EditNormaly) {
                    return "Bei Split-Datenbanken muss ein Chunk-Wert in der Split-Spalte sein.";
                }
            }

            return string.Empty;
        }

        if (db.Column.SysLocked != null) {
            if (!db.PowerEdit) {
                if (column != db.Column.SysLocked && row.CellGetBoolean(db.Column.SysLocked) && !column.EditAllowedDespiteLock) {
                    return "Da die Zeile als abgeschlossen markiert ist, kann die Zelle nicht bearbeitet werden.";
                }
            }
        }

        if (checkUserRights && !db.PermissionCheck(column.PermissionGroupsChangeCell, row)) {
            return "Sie haben nicht die nötigen Rechte, um diesen Wert zu ändern.";
        }
        return string.Empty;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="varcol">Wird eine Collection angegeben, werden zuerst diese Werte benutzt - falls vorhanden - anstelle des Wertes in der Zeile </param>
    /// <returns></returns>
    public static (FilterCollection? fc, string info) GetFilterFromLinkedCellData(Database? linkedDatabase, ColumnItem inputColumn, RowItem? inputRow, VariableCollection? varcol) {
        if (linkedDatabase is not { IsDisposed: false }) { return (null, "Verlinkte Datenbank verworfen."); }
        if (inputColumn.Database is not { IsDisposed: false } || inputColumn.IsDisposed) { return (null, "Datenbank verworfen."); }

        var fi = new List<FilterItem>();

        foreach (var thisFi in inputColumn.LinkedCellFilter) {
            if (!thisFi.Contains("|")) { return (null, "Veraltetes Filterformat"); }

            var x = thisFi.SplitBy("|");
            var c = linkedDatabase.Column[x[0]];
            if (c == null) { return (null, $"Die Spalte {x[0]}, nach der gefiltert werden soll, existiert nicht."); }

            if (x[1] != "=") { return (null, "Nur 'Gleich'-Filter wird unterstützt."); }

            var value = x[2].FromNonCritical();
            if (string.IsNullOrEmpty(value)) { return (null, "Leere Suchwerte werden nicht unterstützt."); }

            if (inputRow is { IsDisposed: false }) {
                // Es kann auch sein, dass nur mit Texten anstelle von Variablen gearbeitet wird,
                // und auch diese abgefragt werden
                value = inputRow.ReplaceVariables(value, true, varcol);
            }

            if (value != c.AutoCorrect(value, true)) { return (null, "Wert kann nicht gesetzt werden."); }

            fi.Add(new FilterItem(c, FilterType.Istgleich, value));
        }

        if (fi.Count == 0 && inputColumn.RelationType != RelationType.DropDownValues) { return (null, "Keine gültigen Suchkriterien definiert."); }

        if (linkedDatabase.Column.ChunkValueColumn is { IsDisposed: false } cvc) {
            if (string.IsNullOrEmpty(FilterCollection.InitValue(cvc, true, fi.ToArray()))) return (null, "Filter des Chunk-Wertes fehlt.");
        }

        var fc = new FilterCollection(linkedDatabase, "cell get filter");
        fc.AddIfNotExists(fi);

        return (fc, string.Empty);
    }

    /// <summary>
    /// Diese Routine erstellt den umgekehrten Linked-Cell-Filter:
    /// Es versucht rauszufinden, welche Zeile in der Datenbank von mycolumn von der Zeile linkedrow befüllt werden.
    /// </summary>
    /// <param name="mycolumn"></param>
    /// <param name="linkedcolumn"></param>
    /// <param name="linkedrow"></param>
    /// <returns></returns>

    public static (FilterCollection? fc, string info) GetFilterReverse(ColumnItem mycolumn, ColumnItem linkedcolumn, RowItem linkedrow) {
        if (linkedcolumn.Database is not { IsDisposed: false } ldb || linkedcolumn.IsDisposed) { return (null, "Datenbank verworfen."); }

        if (mycolumn.RelationType != RelationType.CellValues) { return (null, "Falsches Format."); }

        if (mycolumn.Database is not { IsDisposed: false } db) { return (null, "Datenbank verworfen."); }

        var fi = new List<FilterItem>();

        foreach (var thisFi in mycolumn.LinkedCellFilter) {
            if (!thisFi.Contains("|")) { return (null, "Veraltetes Filterformat"); }

            var x = thisFi.SplitBy("|");
            var c = ldb.Column[x[0]];
            if (c == null) { return (null, "Eine Spalte, nach der gefiltert werden soll, existiert nicht."); }

            if (x[1] != "=") { return (null, "Nur 'Gleich'-Filter wird unterstützt."); }

            var value = x[2].FromNonCritical().ToUpperInvariant();
            if (string.IsNullOrEmpty(value)) { return (null, "Leere Suchwerte werden nicht unterstützt."); }

            foreach (var thisColumn in db.Column) {
                if (value.Contains("~" + thisColumn.KeyName.ToUpperInvariant() + "~")) {
                    fi.Add(new FilterItem(thisColumn, FilterType.Istgleich_ODER_GroßKleinEgal, linkedrow.CellGetList(c)));
                }
            }
        }

        //if (fi.Count == 0 && column.Function != ColumnFunction.Werte_aus_anderer_Datenbank_als_DropDownItemsx) { return (null, "Keine gültigen Suchkriterien definiert."); }

        var fc = new FilterCollection(db, "cell get reverse filter");
        fc.AddIfNotExists(fi);

        if (fc.Count == 0) {
            fc.Add(new FilterItem(db, "Reverse Filter"));
        }

        return (fc, string.Empty);

        //var columns = new List<ColumnItem>();

        //foreach (var thisFi in column.LinkedCellFilter) {
        //    if (!thisFi.Contains("|")) { return (null, "Veraltetes Filterformat"); }

        //    var x = thisFi.SplitBy("|");
        //    var value = x[2].FromNonCritical().ToUpperInvariant();

        //    foreach (var thisColumn in db.Column) {
        //        if (value.Contains("~" + thisColumn.KeyName.ToUpperInvariant() + "~")) {
        //            columns.AddIfNotExists(thisColumn);
        //        }
        //    }
        //}

        //var fc = new FilterCollection(db, "cell reverse get filter");

        //foreach (var thisColumn in columns) {
        //    var fi = new FilterItem(row, thisColumn);
        //    fc.AddIfNotExists(fi);
        //}

        //if (fc.Count == 0) {
        //    fc.Add(new FilterItem(db, "Reverse Filter"));
        //}

        //return (fc, string.Empty);
    }

    public static string KeyOfCell(string colname, string rowKey) => colname.ToUpperInvariant() + "|" + rowKey;

    public static string KeyOfCell(ColumnItem? column, RowItem? row) {
        // Alte verweise eleminieren.
        column = column?.Database?.Column[column.KeyName];
        row = row?.Database?.Row.SearchByKey(row.KeyName);

        if (column != null && row != null) { return KeyOfCell(column.KeyName, row.KeyName); }
        if (column == null && row != null) { return KeyOfCell(string.Empty, row.KeyName); }
        if (column != null && row == null) { return KeyOfCell(column.KeyName, string.Empty); }

        return string.Empty;
    }

    public static (ColumnItem? column, RowItem? row, string info, bool canrepair) LinkedCellData(ColumnItem? inputColumn, RowItem? inputRow, bool repairallowed, bool addRowIfNotExists) {
        if (inputColumn?.Database is not { IsDisposed: false } db) { return (null, null, "Eigene Datenbank verworfen.", false); }
        if (inputColumn.RelationType != RelationType.CellValues) { return (null, null, "Spalte ist nicht verlinkt.", false); }
        if (inputColumn.Value_for_Chunk != ChunkType.None) { return (null, null, "Verlinkte Spalte darf keine Split-Spalte sein.", false); }
        if (inputColumn.LinkedDatabase is not { IsDisposed: false } linkedDatabase) { return (null, null, "Verknüpfte Datenbank verworfen.", false); }
        if (inputRow is not { IsDisposed: false }) { return (null, null, "Keine Zeile zum finden des Zeilenschlüssels angegeben.", false); }

        if (linkedDatabase.Column[inputColumn.ColumnNameOfLinkedDatabase] is not { IsDisposed: false } targetColumn) { return (null, null, "Die Spalte ist in der Zieldatenbank nicht vorhanden.", false); }
        if (targetColumn.Value_for_Chunk != ChunkType.None) { return (null, null, "Verlinkungen auf Chunk-Spalten nicht möglich.", false); }

        var (fc, info) = GetFilterFromLinkedCellData(linkedDatabase, inputColumn, inputRow, null);
        if (!string.IsNullOrEmpty(info)) { return (targetColumn, null, info, false); }
        if (fc is not { Count: not 0 }) { return (targetColumn, null, "Filter konnten nicht generiert werden", false); }

        RowItem? targetRow = null;
        var rows = fc.Rows;
        switch (rows.Count) {
            case > 1:
                return (targetColumn, null, "Suchergebnis liefert mehrere Ergebnisse.", false);

            case 1:
                targetRow = rows[0];
                break;

            default: {
                    if (addRowIfNotExists) {
                        var (newrow, message, _) = db.Row.GenerateAndAdd(fc.ToArray(), "LinkedCell aus " + db.TableName);
                        if (!string.IsNullOrEmpty(message)) { return (targetColumn, null, message, false); }
                        targetRow = newrow;
                    }
                    break;
                }
        }

        if (targetRow != null) {
            if (targetColumn != null && inputColumn != null) {
                if (inputRow != null && repairallowed && inputColumn.RelationType == RelationType.CellValues) {
                    var oldvalue = db.Cell.GetStringCore(inputColumn, inputRow);
                    var newvalue = targetRow.CellGetString(targetColumn);

                    if (oldvalue != newvalue) {
                        var chunkValue = inputRow.ChunkValue;

                        var editableError = EditableErrorReason(chunkValue, chunkValue, inputColumn, inputRow, EditableErrorReasonType.EditAcut, false, false, false, true);

                        if (!string.IsNullOrEmpty(editableError)) { return (targetColumn, targetRow, editableError, false); }
                        //Nicht CellSet! Damit wird der Wert der Ziel-Datenbank verändert
                        //row.CellSet(column, targetRow.KeyName);
                        //  db.Cell.SetValue(column, row, targetRow.KeyName, UserName, DateTime.UtcNow, false);

                        var fehler = db.ChangeData(DatabaseDataType.UTF8Value_withoutSizeData, inputColumn, inputRow, oldvalue, newvalue, UserName, DateTime.UtcNow, "Automatische Reparatur", string.Empty, chunkValue);
                        if (!string.IsNullOrEmpty(fehler)) { return (targetColumn, targetRow, fehler, false); }
                    }
                }
                targetColumn.AddSystemInfo("Links to me", db, inputColumn.KeyName);
            }
        }

        return (targetColumn, targetRow, string.Empty, true);
    }

    public void DataOfCellKey(string cellKey, out ColumnItem? column, out RowItem? row) {
        if (string.IsNullOrEmpty(cellKey)) {
            column = null;
            row = null;
            return;
        }
        var cd = cellKey.SplitBy("|");
        if (cd.GetUpperBound(0) != 1) { Develop.DebugPrint(ErrorType.Error, "Falscher CellKey übergeben: " + cellKey); }
        column = Database?.Column[cd[0]];
        row = Database?.Row.SearchByKey(cd[1]);
    }

    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Database = null;
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public string GetString(ColumnItem? column, RowItem? row) // Main Method
    {
        try {
            if (IsDisposed || Database is not { IsDisposed: false }) {
                Database?.DevelopWarnung("Datenbank ungültig!");
                Develop.DebugPrint(ErrorType.Error, "Datenbank ungültig!");
                return string.Empty;
            }

            if (column is not { IsDisposed: false }) {
                Database?.DevelopWarnung("Spalte ungültig!");
                Develop.DebugPrint(ErrorType.Error, "Spalte ungültig!<br>" + Database?.TableName);
                return string.Empty;
            }

            if (row is not { IsDisposed: false }) {
                Develop.DebugPrint(ErrorType.Error, "Zeile ungültig!<br>" + Database.TableName);
                return string.Empty;
            }

            //if (column.Function is ColumnFunction.Verknüpfung_zu_anderer_Datenbank) {
            //    var (lcolumn, lrow, _, _) = LinkedCellData(column, row, false, false);
            //    return lcolumn != null && lrow != null ? lrow.CellGetString(lcolumn) : string.Empty;
            //}

            return GetStringCore(column, row);
        } catch {
            // Manchmal verscwhindwet der vorhandene KeyName?!?
            Develop.CheckStackOverflow();
            return GetString(column, row);
        }
    }

    public bool IsNullOrEmpty(ColumnItem? column, RowItem? row) {
        if (IsDisposed || Database is not { IsDisposed: false }) { return true; }
        if (column is not { IsDisposed: false }) { return true; }
        if (row is not { IsDisposed: false }) { return true; }
        //if (column.Function is ColumnFunction.Verknüpfung_zu_anderer_Datenbank) {
        //    var (lcolumn, lrow, _, _) = LinkedCellData(column, row, false, false);
        //    return lcolumn == null || lrow == null || lrow.CellIsNullOrEmpty(lcolumn);
        //}

        return string.IsNullOrEmpty(GetStringCore(column, row));
    }

    public string Set(ColumnItem? column, RowItem? row, string value, string comment) {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return "Datenbank ungültig!"; }

        if (!string.IsNullOrEmpty(db.FreezedReason)) { return "Datenbank eingefroren!"; }

        if (column is not { IsDisposed: false }) { return "Spalte ungültig!"; }

        if (row is not { IsDisposed: false }) { return "Zeile ungültig!"; }

        if (db != row.Database || db != column.Database) { return "Datenbank ungültig!"; }

        if (column.RelationType == RelationType.CellValues) {
            var (lcolumn, lrow, _, _) = LinkedCellData(column, row, true, !string.IsNullOrEmpty(value));

            //return db.ChangeData(DatabaseDataType.Value_withoutSizeData, lcolumn, lrow, string.Empty, value, UserName, DateTime.UtcNow, string.Empty);
            lrow?.CellSet(lcolumn, value, "Verlinkung der Datenbank " + db.Caption + " (" + comment + ")");
            return string.Empty;
        }

        value = column.AutoCorrect(value, true);
        var oldValue = GetStringCore(column, row);
        if (value == oldValue) { return string.Empty; }

        column.UcaseNamesSortedByLenght = null;

        if (!column.SaveContent) {
            return row.SetValueInternal(column, value, Reason.NoUndo_NoInvalidate);
        }

        var newChunkValue = row.ChunkValue;
        var oldChunkValue = newChunkValue;

        if (column == db.Column.ChunkValueColumn) {
            newChunkValue = value;
        }

        var message = db.ChangeData(DatabaseDataType.UTF8Value_withoutSizeData, column, row, oldValue, value, UserName, DateTime.UtcNow, comment, oldChunkValue, newChunkValue);

        if (!string.IsNullOrEmpty(message)) { return message; }

        if (value != GetStringCore(column, row)) { return "Nachprüfung fehlgeschlagen"; }

        DoSpecialFormats(column, row, oldValue, value);

        return string.Empty;
    }

    internal bool ChangeColumnName(string oldName, string newName) {
        oldName = oldName.ToUpperInvariant() + "|";
        newName = newName.ToUpperInvariant() + "|";

        if (oldName == newName) { return true; }

        var keys = new List<string>();
        foreach (var thispair in this) {
            if (thispair.Key.StartsWith(oldName)) {
                keys.Add(thispair.Key);
            }
        }

        foreach (var thisk in keys) {
            if (!TryRemove(thisk, out var ci)) { return false; }

            var newk = newName + thisk.TrimStart(oldName);
            if (!TryAdd(newk, ci)) { return false; }
        }

        return true;
    }

    internal string CompareKey(ColumnItem column, RowItem row) => GetStringCore(column, row).CompareKey(column.SortType);

    internal string GetStringCore(ColumnItem? column, RowItem? row) => this[column, row]?.Value ?? string.Empty;

    internal void InvalidateAllSizes() {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return; }
        foreach (var thisColumn in db.Column) {
            thisColumn.Invalidate_ColumAndContent();
        }
    }

    internal void OnCellValueChanged(CellEventArgs e) {
        if (IsDisposed || Database is not { IsDisposed: false }) { return; }
        e.Column.UcaseNamesSortedByLenght = null;
        CellValueChanged?.Invoke(this, e);
    }

    internal void RemoveOrphans() {
        try {
            List<string> removeKeys = [];

            foreach (var pair in this) {
                if (!string.IsNullOrEmpty(pair.Value.Value)) {
                    DataOfCellKey(pair.Key, out var column, out var row);
                    if (column == null || row == null) {
                        removeKeys.Add(pair.Key);
                    }
                }
            }

            if (removeKeys.Count == 0) { return; }

            foreach (var thisKey in removeKeys) {
                if (!TryRemove(thisKey, out _)) {
                    Develop.CheckStackOverflow();
                    RemoveOrphans();
                    return;
                }
            }
        } catch {
            Develop.CheckStackOverflow(); // Um Rauszufinden, ob endlos-Schleifen öfters  vorkommen. Zuletzt 24.11.2020
        }
    }

    private static List<RowItem?> ConnectedRowsOfRelations(string completeRelationText, RowItem? row) {
        List<RowItem?> allRows = [];
        if (row?.Database?.Column.First() == null || row.Database.IsDisposed) { return allRows; }

        var names = row.Database.Column.First()?.GetUcaseNamesSortedByLenght();
        var relationTextLine = completeRelationText.ToUpperInvariant().SplitAndCutByCr();
        foreach (var thisTextLine in relationTextLine) {
            var tmp = thisTextLine;
            List<RowItem?> r = [];
            if (names != null) {
                for (var z = names.Count - 1; z > -1; z--) {
                    if (tmp.IndexOfWord(names[z], 0, RegexOptions.IgnoreCase) > -1) {
                        r.Add(row.Database.Row[names[z]]);
                        tmp = tmp.Replace(names[z], string.Empty);
                    }
                }
            }

            if (r.Count == 1 || r.Contains(row)) { _ = allRows.AddIfNotExists(r); } // Bei mehr als einer verknüpften Reihe MUSS die die eigene Reihe dabei sein.
        }
        return allRows;
    }

    private static void MakeNewRelations(ColumnItem? column, RowItem? row, ICollection<string> oldBz, IEnumerable<string> newBz) {
        if (row is not { IsDisposed: false }) { return; }
        if (column is not { IsDisposed: false }) { return; }

        //// Dann die neuen Erstellen
        foreach (var t in newBz) {
            if (!oldBz.Contains(t)) {
                var x = ConnectedRowsOfRelations(t, row);
                foreach (var thisRow in x) {
                    if (thisRow != null && thisRow != row) {
                        var ex = thisRow.CellGetList(column);
                        if (x.Contains(row)) {
                            ex.Add(t);
                        } else {
                            ex.Add(t.ReplaceWord(thisRow.CellFirstString(), row.CellFirstString(), RegexOptions.IgnoreCase));
                        }
                        thisRow.CellSet(column, ex.SortedDistinctList(), "Automatische Beziehungen von '" + row.CellFirstString() + "'");
                    }
                }
            }
        }
    }

    //    var ownRow = _database.Row.SearchByKey(rowKey);
    //    var rows = keyc.Format == DataFormat.RelationText
    //        ? ConnectedRowsOfRelations(ownRow.CellGetString(keyc), ownRow)
    //        : RowCollection.MatchesTo(new FilterItem(keyc, FilterType.Istgleich_GroßKleinEgal, ownRow.CellGetString(keyc)));
    //    rows.Remove(ownRow);
    //    if (rows.Count < 1) { return; }
    //    foreach (var thisRow in rows) {
    //        thisRow.CellSet(column, currentvalue);
    //    }
    //}

    private void _database_Disposing(object sender, System.EventArgs e) => Dispose();

    private string ChangeTextFromRowId(string completeRelationText) {
        var dbtmp = Database;
        if (dbtmp is not { IsDisposed: false }) { return completeRelationText; }

        foreach (var rowItem in dbtmp.Row) {
            if (rowItem != null) {
                completeRelationText = completeRelationText.Replace("/@X" + rowItem.KeyName + "X@/", rowItem.CellFirstString());
            }
        }
        return completeRelationText;
    }

    private string ChangeTextToRowId(string completeRelationText, string oldValue, string newValue, string keyOfCHangedRow) {
        var dbtmp = Database;
        if (dbtmp is not { IsDisposed: false }) { return completeRelationText; }

        var c = dbtmp.Column.First();
        if (c == null) { return completeRelationText; }

        var names = c.GetUcaseNamesSortedByLenght();
        var didOld = false;
        var didNew = false;
        for (var z = names.Count - 1; z > -1; z--) {
            if (!didOld && names[z].Length <= oldValue.Length) {
                didOld = true;
                DoReplace(oldValue, keyOfCHangedRow);
            }
            if (!didNew && names[z].Length <= newValue.Length) {
                didNew = true;
                DoReplace(newValue, keyOfCHangedRow);
            }
            if (completeRelationText.ToUpperInvariant().Contains(names[z])) {
                var r = dbtmp.Row[names[z]];
                if (r is { IsDisposed: false }) { DoReplace(names[z], r.KeyName); }
            }
        }
        if (string.IsNullOrEmpty(newValue)) { return completeRelationText; }
        // Nochmal am Schluss, wenn die Wörter alle lang sind, und die nicht mehr zum ZUg kommen.
        if (oldValue.Length > newValue.Length) {
            DoReplace(oldValue, keyOfCHangedRow);
            DoReplace(newValue, keyOfCHangedRow);
        } else {
            DoReplace(newValue, keyOfCHangedRow);
            DoReplace(oldValue, keyOfCHangedRow);
        }
        return completeRelationText;
        void DoReplace(string name, string key) {
            if (!string.IsNullOrEmpty(name)) {
                completeRelationText = completeRelationText.Replace(name, "/@X" + key + "X@/", RegexOptions.IgnoreCase);
            }
        }
    }

    private void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
            }
            // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
            // TODO: Große Felder auf NULL setzen
            Database = null;
            Clear();
            IsDisposed = true;
        }
    }

    private void DoSpecialFormats(ColumnItem column, RowItem row, string previewsValue, string currentValue) {
        if (currentValue == previewsValue) { return; }

        if (column.Database is not { IsDisposed: false } db) { return; }

        if (column.Relationship_to_First) { RepairRelationText(column, row, previewsValue); }

        if (!string.IsNullOrEmpty(column.Am_A_Key_For_Other_Column)) {
            foreach (var thisColumn in db.Column) {
                if (thisColumn.RelationType == RelationType.CellValues) {
                    _ = LinkedCellData(thisColumn, row, true, false);
                }
            }
        }

        if (db.Column.First() is { IsDisposed: false } c && c == column) {
            foreach (var thisColumn in db.Column) {
                if (column.Relationship_to_First) {
                    RelationTextNameChanged(thisColumn, row.KeyName, previewsValue, currentValue);
                }
            }
        }
    }

    private void RelationTextNameChanged(ColumnItem columnToRepair, string rowKey, string oldValue, string newValue) {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return; }

        if (string.IsNullOrEmpty(newValue)) { return; }
        foreach (var thisRowItem in db.Row) {
            if (thisRowItem != null) {
                if (!thisRowItem.CellIsNullOrEmpty(columnToRepair)) {
                    var t = thisRowItem.CellGetString(columnToRepair);
                    if (!string.IsNullOrEmpty(oldValue) && t.ToUpperInvariant().Contains(oldValue.ToUpperInvariant())) {
                        t = ChangeTextToRowId(t, oldValue, newValue, rowKey);
                        t = ChangeTextFromRowId(t);
                        var t2 = t.SplitAndCutByCrToList().SortedDistinctList();
                        thisRowItem.CellSet(columnToRepair, t2, "Automatische Beziehungen, Namensänderung: " + oldValue + " -> " + newValue);
                    }
                    if (t.ToUpperInvariant().Contains(newValue.ToUpperInvariant())) {
                        MakeNewRelations(columnToRepair, thisRowItem, [], t.SplitAndCutByCrToList());
                    }
                }
            }
        }
    }

    private void RepairRelationText(ColumnItem column, RowItem row, string previewsValue) {
        var currentString = GetString(column, row);
        currentString = ChangeTextToRowId(currentString, string.Empty, string.Empty, string.Empty);
        currentString = ChangeTextFromRowId(currentString);
        if (currentString != GetString(column, row)) {
            _ = Set(column, row, currentString, "Bezugstextänderung");
            return;
        }
        var oldBz = new List<string>(previewsValue.SplitAndCutByCr()).SortedDistinctList();
        var newBz = new List<string>(currentString.SplitAndCutByCr()).SortedDistinctList();
        // Zuerst Beziehungen LÖSCHEN
        foreach (var t in oldBz) {
            if (!newBz.Contains(t)) {
                var x = ConnectedRowsOfRelations(t, row);
                foreach (var thisRow in x) {
                    if (thisRow != null && thisRow != row) {
                        var ex = thisRow.CellGetList(column);
                        _ = x.Contains(row)
                            ? ex.Remove(t)
                            : ex.Remove(t.ReplaceWord(thisRow.CellFirstString(), row.CellFirstString(), RegexOptions.IgnoreCase));
                        thisRow.CellSet(column, ex.SortedDistinctList(), "Bezugstextänderung / Löschung");
                    }
                }
            }
        }
        MakeNewRelations(column, row, oldBz, newBz);
    }

    #endregion
}