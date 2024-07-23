// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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
using System.Drawing;
using System.Text.RegularExpressions;
using static BlueBasics.Generic;

namespace BlueDatabase;

public sealed class CellCollection : ConcurrentDictionary<string, CellItem>, IDisposableExtended, IHasDatabase {

    #region Fields

    private static readonly ConcurrentDictionary<string, Size> Sizes = [];

    private Database? _database;

    #endregion

    #region Constructors

    public CellCollection(Database database) : base() => Database = database;

    #endregion

    #region Destructors

    // TODO: Finalizer nur �berschreiben, wenn "Dispose(bool disposing)" Code f�r die Freigabe nicht verwalteter Ressourcen enth�lt
    ~CellCollection() {
        // �ndern Sie diesen Code nicht. F�gen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(false);
    }

    #endregion

    #region Events

    public event EventHandler<CellChangedEventArgs>? CellValueChanged;

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
    /// Gibt die Zelle zur�ck.
    /// Da leere Zellen nicht gespeichert werden, kann null zu�ck gebeben werden, obwohl die Zelle an sich g�ltig ist.
    /// </summary>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <returns></returns>
    public CellItem? this[ColumnItem? column, RowItem? row] {
        get {
            if (IsDisposed || Database is not Database db || db.IsDisposed) { return null; }
            if (column == null || column.IsDisposed) { return null; }
            if (row == null || row.IsDisposed) { return null; }

            if (column.Database != row.Database || column.Database != Database) { return null; }

            db.RefreshCellData(column, row, Reason.SetCommand);

            var cellKey = KeyOfCell(column, row);
            return ContainsKey(cellKey) ? this[cellKey] : null;
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gibt einen Fehlergrund zur�ck, ob die Zelle bearbeitet werden kann.
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <param name="mode"></param>
    /// <param name="checkUserRights">Ob vom Benutzer aktiv das Feld bearbeitet werden soll. false bei internen Prozessen angeben.</param>
    /// <param name="checkEditmode">Ob gew�nscht wird, dass die intern programmierte Routine gepr�ft werden soll. Nur in Datenbankansicht empfohlen.</param>
    /// <param name="repairallowed"></param>
    /// <param name="ignoreLinked"></param>
    /// <returns></returns>
    public static string EditableErrorReason(ColumnItem? column, RowItem? row, EditableErrorReasonType mode, bool checkUserRights, bool checkEditmode, bool repairallowed, bool ignoreLinked) {
        if (mode == EditableErrorReasonType.OnlyRead) {
            if (column == null || row == null) { return string.Empty; }
        }

        if (column?.Database is not Database db || db.IsDisposed) { return "Es ist keine Spalte ausgew�hlt."; }

        if (row != null && row.IsDisposed) { return "Die Zeile wurde verworfen."; }

        if (column.Function == ColumnFunction.Virtuelle_Spalte) { return "Virtuelle Spalten k�nnen nicht bearbeitet werden."; }

        var f = column.EditableErrorReason(mode, checkEditmode);
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (column.Function is ColumnFunction.Verkn�pfung_zu_anderer_Datenbank or ColumnFunction.Verkn�pfung_zu_anderer_Datenbank2) {
            if (ignoreLinked) { return string.Empty; }
            //repairallowed = repairallowed && mode is EditableErrorReasonType.EditAcut or EditableErrorReasonType.EditCurrently;

            var (lcolumn, lrow, info, canrepair) = LinkedCellData(column, row, repairallowed, mode is EditableErrorReasonType.EditAcut or EditableErrorReasonType.EditCurrently);

            if (!string.IsNullOrEmpty(info) && !canrepair) { return info; }

            if (lcolumn?.Database is not Database db2 || db2.IsDisposed) { return "Verkn�pfte Datenbank verworfen."; }

            db2.PowerEdit = db.PowerEdit;

            if (lrow != null) {
                var tmp = EditableErrorReason(lcolumn, lrow, mode, checkUserRights, checkEditmode, false, false);
                return string.IsNullOrEmpty(tmp)
                    ? string.Empty
                    : "Die verlinkte Zelle kann nicht bearbeitet werden: " + tmp;
            }

            if (canrepair) { return lcolumn.EditableErrorReason(mode, checkEditmode); }

            return "Allgemeiner Fehler.";
        }

        if (row == null) {
            if (!column.IsFirst()) {
                return "Neue Zeilen m�ssen mit der ersten Spalte beginnen.";
            }

            if (checkUserRights && !db.PermissionCheck(db.PermissionGroupsNewRow, null)) {
                return "Sie haben nicht die n�tigen Rechte, um neue Zeilen anzulegen.";
            }
        } else {
            //if (row.IsDisposed) { return "Die Zeile wurde verworfen."; }
            if (row.Database != db) {
                return "Interner Fehler: Bezug der Datenbank zur Zeile ist fehlerhaft.";
            }

            if (db.Column.SysLocked != null) {
                if (db.PowerEdit.Subtract(DateTime.UtcNow).TotalSeconds < 0) {
                    db.RefreshColumnsData(db.Column.SysLocked);
                    if (column != db.Column.SysLocked && row.CellGetBoolean(db.Column.SysLocked) && !column.EditAllowedDespiteLock) {
                        return "Da die Zeile als abgeschlossen markiert ist, kann die Zelle nicht bearbeitet werden.";
                    }
                }
            }
        }

        if (checkUserRights && !db.PermissionCheck(column.PermissionGroupsChangeCell, row)) {
            return "Sie haben nicht die n�tigen Rechte, um diesen Wert zu �ndern.";
        }

        return string.Empty;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="varcol">Wird eine Collection angegeben, werden zuerst diese Werte benutzt - falls vorhanden - anstelle des Wertes in der Zeile </param>
    /// <returns></returns>
    public static (FilterCollection? fc, string info) GetFilterFromLinkedCellData(Database? linkedDatabase, ColumnItem column, RowItem? row, VariableCollection? varcol) {
        if (linkedDatabase == null || linkedDatabase.IsDisposed) { return (null, "Verlinkte Datenbank verworfen."); }
        if (column.Database is not Database db || db.IsDisposed || column.IsDisposed) { return (null, "Datenbank verworfen."); }

        var fi = new List<FilterItem>();

        foreach (var thisFi in column.LinkedCellFilter) {
            if (!thisFi.Contains("|")) { return (null, "Veraltetes Filterformat"); }

            var x = thisFi.SplitBy("|");
            var c = linkedDatabase?.Column[x[0]];
            if (c == null) { return (null, "Eine Spalte, nach der gefiltert werden soll, existiert nicht."); }

            if (x[1] != "=") { return (null, "Nur 'Gleich'-Filter wird unterst�tzt."); }

            var value = x[2].FromNonCritical();
            if (string.IsNullOrEmpty(value)) { return (null, "Leere Suchwerte werden nicht unterst�tzt."); }

            if (row != null && !row.IsDisposed) {
                // Es kann auch sein, dass nur mit Texten anstelle von Variablen gearbeitet wird,
                // und auch diese abgefragt werden
                value = row.ReplaceVariables(value, false, true, varcol);
            }

            fi.Add(new FilterItem(c, FilterType.Istgleich, value));
        }

        if (fi.Count == 0 && column.Function != ColumnFunction.Werte_aus_anderer_Datenbank_als_DropDownItems) { return (null, "Keine g�ltigen Suchkriterien definiert."); }

        var fc = new FilterCollection(linkedDatabase, "cell get filter");
        fc.AddIfNotExists(fi);

        return (fc, string.Empty);
    }

    /// <summary>
    /// Diese Routine erstellt den umgekehrten Linked-Cell-Filter:
    /// Es versucht rauszufinden, welche Zeile in der Datenbank von mycolumn von der Zeile linkedrow bef�llt werden.
    /// </summary>
    /// <param name="mycolumn"></param>
    /// <param name="linkedcolumn"></param>
    /// <param name="linkedrow"></param>
    /// <returns></returns>

    public static (FilterCollection? fc, string info) GetFilterReverse(ColumnItem mycolumn, ColumnItem linkedcolumn, RowItem linkedrow) {
        if (linkedcolumn.Database is not Database ldb || ldb.IsDisposed || linkedcolumn.IsDisposed) { return (null, "Datenbank verworfen."); }

        if (mycolumn.Function != ColumnFunction.Verkn�pfung_zu_anderer_Datenbank2) { return (null, "Falsches Format."); }

        if (mycolumn.Database is not Database db || db.IsDisposed) { return (null, "Datenbank verworfen."); }

        var fi = new List<FilterItem>();

        foreach (var thisFi in mycolumn.LinkedCellFilter) {
            if (!thisFi.Contains("|")) { return (null, "Veraltetes Filterformat"); }

            var x = thisFi.SplitBy("|");
            var c = ldb.Column[x[0]];
            if (c == null) { return (null, "Eine Spalte, nach der gefiltert werden soll, existiert nicht."); }

            if (x[1] != "=") { return (null, "Nur 'Gleich'-Filter wird unterst�tzt."); }

            var value = x[2].FromNonCritical().ToUpperInvariant();
            if (string.IsNullOrEmpty(value)) { return (null, "Leere Suchwerte werden nicht unterst�tzt."); }

            foreach (var thisColumn in db.Column) {
                if (value.Contains("~" + thisColumn.KeyName.ToUpperInvariant() + "~")) {
                    fi.Add(new FilterItem(thisColumn, FilterType.Istgleich_MultiRowIgnorieren, linkedrow.CellGetString(c)));
                }
            }
        }

        //if (fi.Count == 0 && column.Function != ColumnFunction.Werte_aus_anderer_Datenbank_als_DropDownItems) { return (null, "Keine g�ltigen Suchkriterien definiert."); }

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

    public static bool IsInCache(ColumnItem? column, RowItem? row) {
        if (column == null || column.IsDisposed) { return false; }
        if (row == null || row.IsDisposed) { return false; }
        if (column.Database is not Database db || db.IsDisposed) { return false; }
        return column.IsInCache != null || row.IsInCache != null;
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

    /// <summary>
    ///
    /// </summary>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <param name="repairLinkedValue">Repariert - wenn m�glich - denn Zellenbezug und schreibt ihn im Hintergrund in die Zelle.</param>
    /// <param name="addRowIfNotExists"></param>
    /// <returns></returns>

    public static (ColumnItem? column, RowItem? row, string info, bool canrepair) LinkedCellData(ColumnItem? column, RowItem? row, bool repairLinkedValue, bool addRowIfNotExists) {
        if (column?.Database is not Database db || db.IsDisposed) { return (null, null, "Interner Spaltenfehler.", false); }

        if (column.Function is not ColumnFunction.Verkn�pfung_zu_anderer_Datenbank and not ColumnFunction.Verkn�pfung_zu_anderer_Datenbank2) { return (null, null, "Format ist nicht LinkedCell.", false); }

        var linkedDatabase = column.LinkedDatabase;
        if (linkedDatabase == null || linkedDatabase.IsDisposed) { return (null, null, "Verlinkte Datenbank nicht gefunden.", false); }

        if (repairLinkedValue) { return RepairLinkedCellValue(linkedDatabase, column, row, addRowIfNotExists); }

        var key = db.Cell.GetStringCore(column, row);
        if (string.IsNullOrEmpty(key)) {
            return (linkedDatabase.Column[column.LinkedCell_ColumnNameOfLinkedDatabase], null, "Keine Verlinkung vorhanden.", false);
        }

        if (column.Function == ColumnFunction.Verkn�pfung_zu_anderer_Datenbank2) { return (null, null, "Format ist nicht LinkedCell.", false); }

        if (key.Contains("|")) { return (null, null, "Falsches Format", false); }

        var linkedColumn = linkedDatabase.Column[column.LinkedCell_ColumnNameOfLinkedDatabase];

        if (linkedColumn != null) {
            linkedDatabase.RefreshColumnsData(linkedColumn);
        }

        var linkedRow = linkedDatabase.Row.SearchByKey(key);

        return (linkedColumn, linkedRow, string.Empty, false);
    }

    public static (ColumnItem? column, RowItem? row, string info, bool canrepair) RepairLinkedCellValue(Database linkedDatabase, ColumnItem column, RowItem? row, bool addRowIfNotExists) {
        RowItem? targetRow = null;
        ColumnItem? targetColumn = null;
        var cr = false;

        // Repair nicht mehr erlauben, ergibt rekursieve Schleife, wir sind hier ja schon im repair
        var editableError = EditableErrorReason(column, row, EditableErrorReasonType.EditAcut, false, false, false, true);

        var db = column.Database;
        if (db == null || db.IsDisposed) { return Ergebnis("Verkn�pfte Datenbank verworfen."); }

        if (!string.IsNullOrEmpty(editableError)) { return Ergebnis(editableError); }

        if (row == null || row.IsDisposed) { return Ergebnis("Keine Zeile zum finden des Zeilenschl�ssels angegeben."); }

        targetColumn = linkedDatabase.Column[column.LinkedCell_ColumnNameOfLinkedDatabase];
        if (targetColumn == null) { return Ergebnis("Die Spalte ist in der Zieldatenbank nicht vorhanden."); }

        var (fc, info) = GetFilterFromLinkedCellData(linkedDatabase, column, row, null);
        if (!string.IsNullOrEmpty(info)) { return Ergebnis(info); }
        if (fc == null || fc.Count == 0) { return Ergebnis("Filter konnten nicht generiert werden"); }

        var rows = fc.Rows;
        switch (rows.Count) {
            case > 1:
                return Ergebnis("Suchergebnis liefert mehrere Ergebnisse.");

            case 1:
                targetRow = rows[0];
                break;

            default: {
                    if (addRowIfNotExists) {
                        targetRow = RowCollection.GenerateAndAdd(fc, "LinkedCell aus " + db.TableName).newrow;
                    } else {
                        cr = true;
                    }
                    break;
                }
        }

        return targetRow == null ? Ergebnis("Die Zeile ist in der Zieldatenbank nicht vorhanden.") : Ergebnis(string.Empty);

        #region Subroutine Ergebnis

        (ColumnItem? column, RowItem? row, string info, bool canrepair) Ergebnis(string fehler) {
            if (db != null && column != null && row != null) {
                var oldvalue = db.Cell.GetStringCore(column, row);

                var newvalue = string.Empty;

                if (targetRow != null && targetColumn != null && string.IsNullOrEmpty(fehler)) {
                    if (column.Function == ColumnFunction.Verkn�pfung_zu_anderer_Datenbank) { newvalue = targetRow.KeyName; }

                    if (column.Function == ColumnFunction.Verkn�pfung_zu_anderer_Datenbank2) { newvalue = targetRow.CellGetString(targetColumn); }
                }

                //Nicht CellSet! Damit wird der Wert der Ziel-Datenbank ver�ndert
                //row.CellSet(column, targetRow.KeyName);
                //  db.Cell.SetValue(column, row, targetRow.KeyName, UserName, DateTime.UtcNow, false);

                if (oldvalue != newvalue) {
                    fehler = db.ChangeData(DatabaseDataType.Value_withoutSizeData, column, row, oldvalue, newvalue, UserName, DateTime.UtcNow, "Automatische Reparatur");
                }
            } else {
                if (string.IsNullOrEmpty(fehler)) { fehler = "Datenbankfehler"; }
            }
            return (targetColumn, targetRow, fehler, cr);
        }

        #endregion
    }

    public void DataOfCellKey(string cellKey, out ColumnItem? column, out RowItem? row) {
        if (string.IsNullOrEmpty(cellKey)) {
            column = null;
            row = null;
            return;
        }
        var cd = cellKey.SplitBy("|");
        if (cd.GetUpperBound(0) != 1) { Develop.DebugPrint(FehlerArt.Fehler, "Falscher CellKey �bergeben: " + cellKey); }
        column = Database?.Column[cd[0]];
        row = Database?.Row.SearchByKey(cd[1]);
    }

    public void Dispose() {
        // �ndern Sie diesen Code nicht. F�gen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Database = null;
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Rechnet Zellbez�ge und besondere Spalten neu durch, falls sich der Wert ge�ndert hat.
    /// </summary>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <param name="previewsValue"></param>
    /// <param name="doAlways">Auch wenn der PreviewsValue gleich dem CurrentValue ist, wird die Routine durchberechnet</param>

    public void DoSpecialFormats(ColumnItem? column, RowItem? row, string previewsValue, bool doAlways) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }
        if (row == null || row.IsDisposed) { return; }

        if (column == null || column.IsDisposed) {
            db.DevelopWarnung("Spalte ung�ltig!");
            Develop.DebugPrint(FehlerArt.Fehler, "Spalte ung�ltig!<br>" + db.TableName);
            return;
        }

        var currentValue = GetString(column, row);

        switch (column.Function) {
            case ColumnFunction.RelationText:
                if (doAlways || currentValue != previewsValue) {
                    RepairRelationText(column, row, previewsValue);
                    //SetSameValueOfKey(column, row, currentValue);
                }
                break;

            case ColumnFunction.Verkn�pfung_zu_anderer_Datenbank2:
            case ColumnFunction.Verkn�pfung_zu_anderer_Datenbank:
                if (doAlways) {
                    _ = LinkedCellData(column, row, true, false); // Repariert auch Cellbez�ge
                }
                break;
        }

        if (!doAlways && currentValue == previewsValue) { return; }

        if (!string.IsNullOrEmpty(column.Am_A_Key_For_Other_Column)) {
            foreach (var thisC in db.Column) {
                if (thisC.Function is ColumnFunction.Verkn�pfung_zu_anderer_Datenbank or ColumnFunction.Verkn�pfung_zu_anderer_Datenbank2) {
                    _ = LinkedCellData(thisC, row, true, false);
                }
            }
        }

        if (column.IsFirst()) {
            foreach (var thisColumnItem in db.Column) {
                if (thisColumnItem != null) {
                    switch (thisColumnItem.Function) {
                        //case DataFormat.Relation:
                        //    RelationNameChanged(ThisColumnItem, PreviewsValue, CurrentValue);
                        //    break;
                        case ColumnFunction.RelationText:
                            RelationTextNameChanged(thisColumnItem, row.KeyName, previewsValue, currentValue);
                            break;
                    }
                }
            }
        }
        //if (column.KeyColumnKey > -1) {
        //    ChangeValueOfKey(currentValue, column, row.KeyName);
        //}
    }

    public void DoSystemColumns(Database db, ColumnItem column, RowItem row, string user, DateTime datetimeutc, Reason reason) {
        if (reason == Reason.NoUndo_NoInvalidate) { return; }

        // Die unterschiedlichen Reasons in der Routine beachten!
        if (db.Column.SysRowChanger is ColumnItem src && src != column) { SetValueInternal(src, row, user, Reason.NoUndo_NoInvalidate); }
        if (db.Column.SysRowChangeDate is ColumnItem scd && scd != column) { SetValueInternal(scd, row, datetimeutc.ToString5(), Reason.NoUndo_NoInvalidate); }

        RowCollection.FailedRows.Remove(row);

        if (column.ScriptType != ScriptType.Nicht_vorhanden) {
            if (db.Column.SysRowState is ColumnItem srs && srs != column) {
                RowCollection.WaitDelay = 0;
                if (column.Function == ColumnFunction.Schl�sselspalte) {
                    SetValueInternal(srs, row, string.Empty, reason);
                } else {
                    if (row.CellGetLong(srs) > 0) {
                        // wichitg, nur wenn >0, weil leere ZEilen dann nicht mehr kompett durchgerechnet werden
                        SetValueInternal(srs, row, "0", reason);
                    }
                }
            }
        }
    }

    public Size? GetSizeOfCellContent(ColumnItem column, RowItem row) {
        var txt = GetString(column, row);
        if (string.IsNullOrEmpty(txt)) { return null; }

        var key = TextSizeKey(column, txt);

        if (key == null) { return null; }

        if (Sizes.ContainsKey(key)) { return Sizes[key]; }

        return null;
    }

    public string GetString(ColumnItem? column, RowItem? row) // Main Method
    {
        try {
            if (IsDisposed || Database is not Database db || db.IsDisposed) {
                Database?.DevelopWarnung("Datenbank ung�ltig!");
                Develop.DebugPrint(FehlerArt.Fehler, "Datenbank ung�ltig!");
                return string.Empty;
            }

            if (column == null || column.IsDisposed) {
                Database?.DevelopWarnung("Spalte ung�ltig!");
                Develop.DebugPrint(FehlerArt.Fehler, "Spalte ung�ltig!<br>" + Database?.TableName);
                return string.Empty;
            }
            if (row == null || row.IsDisposed) {
                Develop.DebugPrint(FehlerArt.Fehler, "Zeile ung�ltig!<br>" + Database.TableName);
                return string.Empty;
            }

            if (column.Function is ColumnFunction.Verkn�pfung_zu_anderer_Datenbank) {
                var (lcolumn, lrow, _, _) = LinkedCellData(column, row, false, false);
                return lcolumn != null && lrow != null ? lrow.CellGetString(lcolumn) : string.Empty;
            }

            return GetStringCore(column, row);
        } catch {
            // Manchmal verscwhindwet der vorhandene KeyName?!?
            Develop.CheckStackForOverflow();
            return GetString(column, row);
        }
    }

    public bool IsNullOrEmpty(ColumnItem? column, RowItem? row) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return true; }
        if (column == null || column.IsDisposed) { return true; }
        if (row == null || row.IsDisposed) { return true; }
        if (column.Function is ColumnFunction.Verkn�pfung_zu_anderer_Datenbank) {
            var (lcolumn, lrow, _, _) = LinkedCellData(column, row, false, false);
            return lcolumn == null || lrow == null || lrow.CellIsNullOrEmpty(lcolumn);
        }

        return string.IsNullOrEmpty(GetStringCore(column, row));
    }

    public string Set(ColumnItem? column, RowItem? row, string value, string comment) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return "Datenbank ung�ltig!"; }

        if (column == null || column.IsDisposed) { return "Spalte ung�ltig!"; }

        if (row == null || row.IsDisposed) { return "Zeile ung�ltig!"; }

        if (db != row.Database || db != column.Database) { return "Datenbank ung�ltig!"; }

        if (column.Function == ColumnFunction.Virtuelle_Spalte) {
            return SetValueInternal(column, row, value, Reason.NoUndo_NoInvalidate);
        }

        if (!string.IsNullOrEmpty(db.FreezedReason)) { return "Datenbank eingefroren!"; }

        if (column.Function is ColumnFunction.Verkn�pfung_zu_anderer_Datenbank
                            or ColumnFunction.Verkn�pfung_zu_anderer_Datenbank2) {
            var (lcolumn, lrow, _, _) = LinkedCellData(column, row, true, !string.IsNullOrEmpty(value));

            //return db.ChangeData(DatabaseDataType.Value_withoutSizeData, lcolumn, lrow, string.Empty, value, UserName, DateTime.UtcNow, string.Empty);
            lrow?.CellSet(lcolumn, value, "Verlinkung der Datenbank " + db.Caption + " (" + comment + ")");
            return string.Empty;
        }

        value = column.AutoCorrect(value, true);
        var oldValue = GetStringCore(column, row);
        if (value == oldValue) { return string.Empty; }

        column.UcaseNamesSortedByLenght = null;

        var message = db.ChangeData(DatabaseDataType.Value_withoutSizeData, column, row, oldValue, value, UserName, DateTime.UtcNow, comment);

        if (!string.IsNullOrEmpty(message)) { return message; }

        if (value != GetStringCore(column, row)) { return "Nachpr�fung fehlgeschlagen"; }

        DoSpecialFormats(column, row, oldValue, false);

        //DoSystemColumns(db, column, row, UserName, DateTime.UtcNow, Reason.SystemSet);

        return string.Empty;
    }

    public void SetSizeOfCellContent(ColumnItem column, RowItem row, Size contentSize) {
        var key = TextSizeKey(column, row.CellGetString(column));

        if (key == null) { return; }

        if (Sizes.ContainsKey(key)) {
            Sizes[key] = contentSize;
            return;
        }

        _ = Sizes.TryAdd(key, contentSize);
    }

    public List<string> ValuesReadable(ColumnItem column, RowItem row, ShortenStyle style) => CellItem.ValuesReadable(column, row, style) ?? [];

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

    internal string CompareKey(ColumnItem column, RowItem row) => GetString(column, row).CompareKey(column.SortType);

    internal string GetStringCore(ColumnItem? column, RowItem? row) => this[column, row]?.Value ?? string.Empty;

    internal void InvalidateAllSizes() {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }
        foreach (var thisColumn in db.Column) {
            thisColumn.Invalidate_ColumAndContent();
        }
    }

    internal void OnCellValueChanged(CellChangedEventArgs e) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }
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
                    Develop.CheckStackForOverflow();
                    RemoveOrphans();
                    return;
                }
            }
        } catch {
            Develop.CheckStackForOverflow(); // Um Rauszufinden, ob endlos-Schleifen �fters  vorkommen. Zuletzt 24.11.2020
        }
    }

    /// <summary>
    /// Setzt den Wert ohne Undo Logging
    /// </summary>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <param name="value"></param>
    /// <param name="reason"></param>
    internal string SetValueInternal(ColumnItem column, RowItem row, string value, Reason reason) {
        var tries = 0;

        while (true) {
            if (IsDisposed || column.Database is not Database db || db.IsDisposed) { return "Datenbank ung�ltig"; }
            if (tries > 100) { return "Wert konnte nicht gesetzt werden."; }

            db.RefreshCellData(column, row, reason);

            var cellKey = KeyOfCell(column, row);

            if (ContainsKey(cellKey)) {
                var c = this[cellKey];
                c.Value = value; // Auf jeden Fall setzen. Auch falls es nachher entfernt wird, so ist es sicher leer
                if (string.IsNullOrEmpty(value)) {
                    if (!TryRemove(cellKey, out _)) {
                        tries++;
                        continue;
                    }
                }
            } else {
                if (!string.IsNullOrEmpty(value)) {
                    if (!TryAdd(cellKey, new CellItem(value))) {
                        tries++;
                        continue;
                    }
                }
            }

            if (reason == Reason.SetCommand) {
                if (column.ScriptType != ScriptType.Nicht_vorhanden) {
                    if (column.Function == ColumnFunction.Schl�sselspalte) {
                        if (db.Column.SysRowState is ColumnItem srs) {
                            SetValueInternal(srs, row, string.Empty, reason);
                        }
                    }
                }
            }

            if (reason is Reason.SetCommand) {
                column.Invalidate_ContentWidth();
                row.InvalidateCheckData();
                OnCellValueChanged(new CellChangedEventArgs(column, row, reason));
            }

            return string.Empty;
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
            if (names != null)
                for (var z = names.Count - 1; z > -1; z--) {
                    if (tmp.IndexOfWord(names[z], 0, RegexOptions.IgnoreCase) > -1) {
                        r.Add(row.Database.Row[names[z]]);
                        tmp = tmp.Replace(names[z], string.Empty);
                    }
                }

            if (r.Count == 1 || r.Contains(row)) { _ = allRows.AddIfNotExists(r); } // Bei mehr als einer verkn�pften Reihe MUSS die die eigene Reihe dabei sein.
        }
        return allRows;
    }

    private static void MakeNewRelations(ColumnItem? column, RowItem? row, ICollection<string> oldBz, IEnumerable<string> newBz) {
        if (row == null || row.IsDisposed) { return; }
        if (column == null || column.IsDisposed) { return; }

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
    //        : RowCollection.MatchesTo(new FilterItem(keyc, FilterType.Istgleich_Gro�KleinEgal, ownRow.CellGetString(keyc)));
    //    rows.Remove(ownRow);
    //    if (rows.Count < 1) { return; }
    //    foreach (var thisRow in rows) {
    //        thisRow.CellSet(column, currentvalue);
    //    }
    //}
    /// <summary>
    /// �ndert die anderen Zeilen dieser Spalte, so dass der verkn�pfte Text bei dieser und den anderen Spalten gleich ist ab.
    /// </summary>
    /// <param name="column"></param>
    /// <param name="text"></param>
    private static string? TextSizeKey(ColumnItem? column, string text) {
        if (column?.Database is not Database db || db.IsDisposed || column.IsDisposed) { return null; }
        return db.TableName + "|" + column.KeyName + "|" + text;
    }

    private void _database_Disposing(object sender, System.EventArgs e) => Dispose();

    private string ChangeTextFromRowId(string completeRelationText) {
        var dbtmp = Database;
        if (dbtmp == null || dbtmp.IsDisposed) { return completeRelationText; }

        foreach (var rowItem in dbtmp.Row) {
            if (rowItem != null) {
                completeRelationText = completeRelationText.Replace("/@X" + rowItem.KeyName + "X@/", rowItem.CellFirstString());
            }
        }
        return completeRelationText;
    }

    private string ChangeTextToRowId(string completeRelationText, string oldValue, string newValue, string keyOfCHangedRow) {
        var dbtmp = Database;
        if (dbtmp == null || dbtmp.IsDisposed) { return completeRelationText; }

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
                if (r != null && !r.IsDisposed) { DoReplace(names[z], r.KeyName); }
            }
        }
        if (string.IsNullOrEmpty(newValue)) { return completeRelationText; }
        // Nochmal am Schluss, wenn die W�rter alle lang sind, und die nicht mehr zum ZUg kommen.
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
            // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer �berschreiben
            // TODO: Gro�e Felder auf NULL setzen
            Database = null;
            Clear();
            IsDisposed = true;
        }
    }

    private void RelationTextNameChanged(ColumnItem columnToRepair, string rowKey, string oldValue, string newValue) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }

        if (string.IsNullOrEmpty(newValue)) { return; }
        foreach (var thisRowItem in db.Row) {
            if (thisRowItem != null) {
                if (!thisRowItem.CellIsNullOrEmpty(columnToRepair)) {
                    var t = thisRowItem.CellGetString(columnToRepair);
                    if (!string.IsNullOrEmpty(oldValue) && t.ToUpperInvariant().Contains(oldValue.ToUpperInvariant())) {
                        t = ChangeTextToRowId(t, oldValue, newValue, rowKey);
                        t = ChangeTextFromRowId(t);
                        var t2 = t.SplitAndCutByCrToList().SortedDistinctList();
                        thisRowItem.CellSet(columnToRepair, t2, "Automatische Beziehungen, Namens�nderung: " + oldValue + " -> " + newValue);
                    }
                    if (t.ToUpperInvariant().Contains(newValue.ToUpperInvariant())) {
                        MakeNewRelations(columnToRepair, thisRowItem, new List<string>(), t.SplitAndCutByCrToList());
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
            Set(column, row, currentString, "Bezugstext�nderung");
            return;
        }
        var oldBz = new List<string>(previewsValue.SplitAndCutByCr()).SortedDistinctList();
        var newBz = new List<string>(currentString.SplitAndCutByCr()).SortedDistinctList();
        // Zuerst Beziehungen L�SCHEN
        foreach (var t in oldBz) {
            if (!newBz.Contains(t)) {
                var x = ConnectedRowsOfRelations(t, row);
                foreach (var thisRow in x) {
                    if (thisRow != null && thisRow != row) {
                        var ex = thisRow.CellGetList(column);
                        if (x.Contains(row)) {
                            _ = ex.Remove(t);
                        } else {
                            _ = ex.Remove(t.ReplaceWord(thisRow.CellFirstString(), row.CellFirstString(), RegexOptions.IgnoreCase));
                        }
                        thisRow.CellSet(column, ex.SortedDistinctList(), "Bezugstext�nderung / L�schung");
                    }
                }
            }
        }
        MakeNewRelations(column, row, oldBz, newBz);
    }

    #endregion
}