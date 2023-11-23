// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using BlueDatabase.Interfaces;
using static BlueBasics.Converter;
using static BlueBasics.Generic;

namespace BlueDatabase;

public sealed class CellCollection : ConcurrentDictionary<string, CellItem>, IDisposableExtended, IHasDatabase {

    #region Fields

    public static ConcurrentDictionary<string, Size> Sizes = new();

    #endregion

    #region Constructors

    public CellCollection(DatabaseAbstract database) : base() {
        Database = database;
        Database.DisposingEvent += _database_Disposing;
        //       Cell = New Dictionary(Of String, CellItem)
        Initialize();
    }

    #endregion

    #region Destructors

    // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    ~CellCollection() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(false);
    }

    #endregion

    #region Events

    public event EventHandler<CellChangedEventArgs>? CellValueChanged;

    #endregion

    #region Properties

    public DatabaseAbstract? Database { get; private set; }
    public bool IsDisposed { get; private set; }

    #endregion

    //public static string AutomaticInitalValue(ColumnItem? column, RowItem? row) {
    //    if (column == null || row == null) { return string.Empty; }

    //    if (column.Format is DataFormat.Verknüpfung_zu_anderer_Datenbank) {
    //        var (lcolumn, lrow, _) = LinkedCellData(column, row, true, true);
    //        return AutomaticInitalValue(lcolumn, lrow);
    //    }

    //    if (column.VorschlagsColumn < 0) { return string.Empty; }
    //    var cc = column.Database.Column.SearchByKey(column.VorschlagsColumn);
    //    if (cc == null) { return string.Empty; }
    //    FilterCollection f = new(column.Database)
    //    {
    //        new FilterItem(cc, FilterType.Istgleich_GroßKleinEgal, row.CellGetString(cc)),
    //        new FilterItem(column, FilterType.Ungleich_MultiRowIgnorieren, string.Empty)
    //    };
    //    var rows = column.Database.Row.RowsFiltered(f);
    //    rows.Remove(row);
    //    return rows.Count == 0 ? string.Empty : rows[0].CellGetString(column);
    //}

    #region Methods

    /// <summary>
    /// Gibt einen Fehlergrund zurück, ob die Zelle bearbeitet werden kann.
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <param name="mode"></param>
    /// <param name="checkUserRights">Ob vom Benutzer aktiv das Feld bearbeitet werden soll. false bei Internen Prozessen angeben.</param>
    /// <param name="checkEditmode">Ob gewünscht wird, das die intern Programmierte Routine geprüft werden soll. Nur in Datenbankansicht empfohlen.</param>
    /// <param name="repairallowed"></param>
    /// <param name="ignoreLinked"></param>
    /// <returns></returns>
    public static string EditableErrorReason(ColumnItem? column, RowItem? row, EditableErrorReasonType mode, bool checkUserRights, bool checkEditmode, bool repairallowed, bool ignoreLinked) {
        if (mode == EditableErrorReasonType.OnlyRead) {
            if (column == null || row == null) { return string.Empty; }
        }

        if (column?.Database is not DatabaseAbstract db || db.IsDisposed) { return "Es ist keine Spalte ausgewählt."; }

        if (row != null && row.IsDisposed) { return "Die Zeile wurde verworfen."; }

        var f = column.EditableErrorReason(mode, checkEditmode);
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (column.Format is DataFormat.Verknüpfung_zu_anderer_Datenbank) {
            if (ignoreLinked) { return string.Empty; }
            //repairallowed = repairallowed && mode is EditableErrorReasonType.EditAcut or EditableErrorReasonType.EditCurrently;

            var (lcolumn, lrow, info, canrepair) = LinkedCellData(column, row, repairallowed, mode is EditableErrorReasonType.EditAcut or EditableErrorReasonType.EditCurrently);

            if (!string.IsNullOrEmpty(info) && !canrepair) { return info; }

            if (lcolumn?.Database is not DatabaseAbstract db2 || db2.IsDisposed) { return "Verknüpfte Datenbank verworfen."; }

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
                return "Neue Zeilen müssen mit der ersten Spalte beginnen.";
            }

            if (checkUserRights && !db.PermissionCheck(db.PermissionGroupsNewRow, null)) {
                return "Sie haben nicht die nötigen Rechte, um neue Zeilen anzulegen.";
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
            return "Sie haben nicht die nötigen Rechte, um diesen Wert zu ändern.";
        }

        return string.Empty;
    }

    public static (FilterCollection? fc, string info) GetFilterFromLinkedCellData(DatabaseAbstract? linkedDatabase, ColumnItem column, RowItem? row) {
        if (linkedDatabase == null || linkedDatabase.IsDisposed) { return (null, "Verlinkte Datenbank verworfen."); }
        if (column.Database is not DatabaseAbstract db || db.IsDisposed || column.IsDisposed) { return (null, "Datenbank verworfen."); }

        var fi = new List<FilterItem>();

        foreach (var thisFi in column.LinkedCellFilter) {
            if (!thisFi.Contains("|")) { return (null, "Veraltetes Filterformat"); }

            var x = thisFi.SplitBy("|");
            var c = linkedDatabase?.Column.Exists(x[0]);
            if (c == null) { return (null, "Eine Spalte, nach der gefiltert werden soll, existiert nicht."); }

            if (x[1] != "=") { return (null, "Nur 'Gleich'-Filter wird unterstützt."); }

            var value = x[2].FromNonCritical();
            if (string.IsNullOrEmpty(value)) { return (null, "Leere Suchwerte werden nicht unterstützt."); }

            if (row != null && !row.IsDisposed) {
                // Es kann auch sein, dass nur mit Texten anstelle von Variablen gearbeitet wird,
                // und auch diese abgefragt werden
                value = row.ReplaceVariables(value, false, true);
            }

            fi.Add(new FilterItem(c, FilterType.Istgleich, value));
        }

        if (fi.Count == 0 && column.Format != DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems) { return (null, "Keine gültigen Suchkriterien definiert."); }

        var fc = new FilterCollection(fi[0].Database);
        fc.AddIfNotExists(fi);

        return (fc, string.Empty);
    }

    public static string KeyOfCell(string colname, string rowKey) => colname.ToUpper() + "|" + rowKey;

    public static string KeyOfCell(ColumnItem? column, RowItem? row) {
        // Alte verweise eleminieren.
        column = column?.Database?.Column.Exists(column.KeyName);
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
    /// <param name="repairLinkedValue">Repariert - wenn möglich - denn Zellenbezug und schreibt ihn im Hintergrund in die Zelle.</param>
    /// <param name="addRowIfNotExists"></param>
    /// <returns></returns>

    public static (ColumnItem? column, RowItem? row, string info, bool canrepair) LinkedCellData(ColumnItem? column, RowItem? row, bool repairLinkedValue, bool addRowIfNotExists) {
        if (column?.Database is not DatabaseAbstract db || db.IsDisposed) { return (null, null, "Interner Spaltenfehler.", false); }

        if (column.Format is not DataFormat.Verknüpfung_zu_anderer_Datenbank) { return (null, null, "Format ist nicht LinkedCell.", false); }

        var linkedDatabase = column.LinkedDatabase;
        if (linkedDatabase == null || linkedDatabase.IsDisposed) { return (null, null, "Verlinkte Datenbank nicht gefunden.", false); }

        if (repairLinkedValue) { return RepairLinkedCellValue(linkedDatabase, column, row, addRowIfNotExists); }

        var key = db.Cell.GetStringCore(column, row);
        if (string.IsNullOrEmpty(key)) {
            return (linkedDatabase.Column.Exists(column.LinkedCell_ColumnNameOfLinkedDatabase), null, "Keine Verlinkung vorhanden.", false);
        }

        if (key.Contains("|")) { return (null, null, "Falsches Format", false); }

        var linkedColumn = linkedDatabase.Column.Exists(column.LinkedCell_ColumnNameOfLinkedDatabase); // linkedDatabase.Column.SearchByKey(LongParse(v[0]));

        if (linkedColumn != null) {
            linkedDatabase.RefreshColumnsData(linkedColumn);
        }

        var linkedRow = linkedDatabase.Row.SearchByKey(key);

        return (linkedColumn, linkedRow, string.Empty, false);
    }

    public void DataOfCellKey(string cellKey, out ColumnItem? column, out RowItem? row) {
        if (string.IsNullOrEmpty(cellKey)) {
            column = null;
            row = null;
            return;
        }
        var cd = cellKey.SplitBy("|");
        if (cd.GetUpperBound(0) != 1) { Develop.DebugPrint(FehlerArt.Fehler, "Falscher CellKey übergeben: " + cellKey); }
        column = Database?.Column.Exists(cd[0]);
        row = Database?.Row.SearchByKey(cd[1]);
    }

    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Rechnet Zellbezüge und besondere Spalten neu durch, falls sich der Wert geändert hat.
    /// </summary>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <param name="previewsValue"></param>
    /// <param name="doAlways">Auch wenn der PreviewsValue gleich dem CurrentValue ist, wird die Routine durchberechnet</param>

    public void DoSpecialFormats(ColumnItem? column, RowItem? row, string previewsValue, bool doAlways) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }
        if (row == null || row.IsDisposed) { return; }

        if (column == null || column.IsDisposed) {
            db.DevelopWarnung("Spalte ungültig!");
            Develop.DebugPrint(FehlerArt.Fehler, "Spalte ungültig!<br>" + db.TableName);
            return;
        }

        var currentValue = GetString(column, row);

        switch (column.Format) {
            case DataFormat.RelationText:
                if (doAlways || currentValue != previewsValue) {
                    RepairRelationText(column, row, previewsValue);
                    //SetSameValueOfKey(column, row, currentValue);
                }
                break;

            case DataFormat.Verknüpfung_zu_anderer_Datenbank:
                if (doAlways) {
                    _ = LinkedCellData(column, row, true, false); // Repariert auch Cellbezüge
                }
                break;
        }

        if (!doAlways && currentValue == previewsValue) { return; }
        //column.CheckIfIAmAKeyColumn();

        if (!string.IsNullOrEmpty(column.Am_A_Key_For_Other_Column)) {
            foreach (var thisC in db.Column) {
                if (thisC.Format == DataFormat.Verknüpfung_zu_anderer_Datenbank) {
                    _ = LinkedCellData(thisC, row, true, false);
                }
            }

            //SetSameValueOfKey(column, row, currentValue);
        }

        if (column.IsFirst()) {
            foreach (var thisColumnItem in db.Column) {
                if (thisColumnItem != null) {
                    switch (thisColumnItem.Format) {
                        //case DataFormat.Relation:
                        //    RelationNameChanged(ThisColumnItem, PreviewsValue, CurrentValue);
                        //    break;
                        case DataFormat.RelationText:
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

    public void DoSystemColumns(DatabaseAbstract db, ColumnItem column, RowItem row, string user, DateTime datetimeutc, Reason reason) {

        if(reason == Reason.InitialLoad) { return; }

        // Die unterschiedlichen Reasons in der Routine beachten!
        if (db.Column.SysRowChanger is ColumnItem src && src != column) { SetValueInternal(src, row, user, Reason.SystemSet); }
        if (db.Column.SysRowChangeDate is ColumnItem scd && scd != column) { SetValueInternal(scd, row, datetimeutc.ToString(Constants.Format_Date5, CultureInfo.InvariantCulture), Reason.SystemSet); }

        if (column.ScriptType != ScriptType.Nicht_vorhanden) {
            if (db.Column.SysRowState is ColumnItem srs && srs != column) { SetValueInternal(srs, row, string.Empty, reason); }
        }
    }

    public bool GetBoolean(ColumnItem? column, RowItem? row) => GetString(column, row).FromPlusMinus();// Main Method

    public Color GetColor(ColumnItem? column, RowItem? row) => Color.FromArgb(GetInteger(column, row)); // Main Method

    public int GetColorBgr(ColumnItem? column, RowItem? row) {
        var c = GetColor(column, row);
        int colorBlue = c.B;
        int colorGreen = c.G;
        int colorRed = c.R;
        return (colorBlue << 16) | (colorGreen << 8) | colorRed;
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>DateTime.MinValue bei Fehlern</returns>
    public DateTime GetDateTime(ColumnItem? column, RowItem? row) {
        var @string = GetString(column, row);
        return string.IsNullOrEmpty(@string) ? default : DateTimeTryParse(@string, out var d) ? d : DateTime.MinValue;
    }

    public double GetDouble(ColumnItem? column, RowItem? row) {
        var x = GetString(column, row);
        return string.IsNullOrEmpty(x) ? 0 : DoubleParse(x);
    }

    public int GetInteger(ColumnItem? column, RowItem? row) {
        var x = GetString(column, row);
        return string.IsNullOrEmpty(x) ? 0 : IntParse(x);
    }

    public List<string> GetList(ColumnItem? column, RowItem? row) => GetString(column, row).SplitAndCutByCrToList();// Main Method

    public Point GetPoint(ColumnItem? column, RowItem? row) // Main Method
    {
        var @string = GetString(column, row);
        return string.IsNullOrEmpty(@string) ? Point.Empty : @string.PointParse();
    }

    public Size? GetSizeOfCellContent(ColumnItem column, RowItem row) {
        var txt = row.CellGetString(column);
        if (string.IsNullOrEmpty(txt)) { return null; }

        var key = TextSizeKey(column, txt);

        if (key == null) { return null; }

        if (Sizes.ContainsKey(key)) { return Sizes[key]; }

        return null;
    }

    public string GetString(ColumnItem? column, RowItem? row) // Main Method
    {
        try {
            if (Database is not DatabaseAbstract db || db.IsDisposed) {
                Database?.DevelopWarnung("Datenbank ungültig!");
                Develop.DebugPrint(FehlerArt.Fehler, "Datenbank ungültig!");
                return string.Empty;
            }

            if (column == null || column.IsDisposed) {
                Database?.DevelopWarnung("Spalte ungültig!");
                Develop.DebugPrint(FehlerArt.Fehler, "Spalte ungültig!<br>" + Database?.TableName);
                return string.Empty;
            }
            if (row == null || row.IsDisposed) {
                Develop.DebugPrint(FehlerArt.Fehler, "Zeile ungültig!<br>" + Database.TableName);
                return string.Empty;
            }

            if (column.Format is DataFormat.Verknüpfung_zu_anderer_Datenbank) {
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

    public void Initialize() => Clear();

    public bool IsInCache(ColumnItem? column, RowItem? row) {
        if (column == null || column.IsDisposed) { return false; }
        if (row == null || row.IsDisposed) { return false; }
        if (column.Database is not DatabaseAbstract db || db.IsDisposed) { return false; }
        return column.IsInCache != null || row.IsInCache != null;
    }

    public bool IsNullOrEmpty(ColumnItem? column, RowItem? row) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return true; }
        if (column == null || column.IsDisposed) { return true; }
        if (row == null || row.IsDisposed) { return true; }
        if (column.Format is DataFormat.Verknüpfung_zu_anderer_Datenbank) {
            var (lcolumn, lrow, _, _) = LinkedCellData(column, row, false, false);
            return lcolumn == null || lrow == null || lrow.CellIsNullOrEmpty(lcolumn);
        }

        return string.IsNullOrEmpty(GetStringCore(column, row));
    }

    public bool MatchesTo(ColumnItem? column, RowItem? row, FilterItem fi) {
        //Grundlegendes zu UND und ODER:
        //Ein Filter kann mehrere Werte haben, diese müssen ein Attribut UND oder ODER haben.
        //Bei UND müssen alle Werte des Filters im Multiline vorkommen.
        //Bei ODER muss ein Wert des Filters im Multiline vorkommen.
        //Beispiel: UND-Filter mit C & D
        //Wenn die Zelle       A B C D hat, trifft der UND-Filter zwar nicht bei den ersten beiden zu, aber bei den letzten.
        //Um genau zu sein C:  - - + -
        //                 D:  - - - +
        //Deswegen muss beim einem UND-Filter nur EINER der Zellenwerte zutreffen.
        //if (Filter.FilterType == enFilterType.KeinFilter) { Develop.DebugPrint(enFehlerArt.Fehler, "Kein Filter angegeben: " + ToString()); }
        try {
            var typ = fi.FilterType;
            // Oder-Flag ermitteln --------------------------------------------
            var oder = typ.HasFlag(FilterType.ODER);
            if (oder) { typ ^= FilterType.ODER; }
            // Und-Flag Ermitteln --------------------------------------------
            var und = typ.HasFlag(FilterType.UND);
            if (und) { typ ^= FilterType.UND; }
            if (fi.SearchValue.Count < 2) {
                oder = true;
                und = false; // Wenn nur EIN Eintrag gecheckt wird, ist es EGAL, ob UND oder ODER.
            }
            //if (Und && Oder) { Develop.DebugPrint(enFehlerArt.Fehler, "Filter-Anweisung erwartet ein 'Und' oder 'Oder': " + ToString()); }
            // Tatsächlichen String ermitteln --------------------------------------------
            var txt = string.Empty;
            var fColumn = column;
            if (column != null && column.Format is DataFormat.Verknüpfung_zu_anderer_Datenbank) {
                var (columnItem, rowItem, _, _) = LinkedCellData(column, row, false, false);
                if (columnItem != null && rowItem != null) {
                    txt = rowItem.CellGetString(columnItem);
                    fColumn = columnItem;
                }
            } else {
                txt = GetStringCore(column, row);
            }

            if (typ.HasFlag(FilterType.Instr)) { txt = LanguageTool.ColumnReplace(txt, fColumn, ShortenStyle.Both); }
            // Multiline-Typ ermitteln  --------------------------------------------
            var tmpMultiLine = false;
            if (column != null && !column.IsDisposed) { tmpMultiLine = column.MultiLine; }
            if (typ.HasFlag(FilterType.MultiRowIgnorieren)) {
                tmpMultiLine = false;
                typ ^= FilterType.MultiRowIgnorieren;
            }
            if (tmpMultiLine && !txt.Contains("\r")) { tmpMultiLine = false; } // Zeilen mit nur einem Eintrag können ohne Multiline behandel werden.
                                                                               //if (Typ == enFilterType.KeinFilter)
                                                                               //{
                                                                               //    Develop.DebugPrint(enFehlerArt.Fehler, "'Kein Filter' wurde übergeben: " + ToString());
                                                                               //}
            if (!tmpMultiLine) {
                var bedingungErfüllt = false;
                foreach (var t in fi.SearchValue) {
                    bedingungErfüllt = CompareValues(txt, t, typ);
                    if (oder && bedingungErfüllt) { return true; }
                    if (und && bedingungErfüllt == false) { return false; } // Bei diesem UND hier müssen allezutreffen, deshalb kann getrost bei einem False dieses zurückgegeben werden.
                }
                return bedingungErfüllt;
            }
            List<string> vorhandenWerte = new(txt.SplitAndCutByCr());
            if (vorhandenWerte.Count == 0) // Um den Filter, der nach 'Leere' Sucht, zu befriediegen
            {
                vorhandenWerte.Add("");
            }
            // Diese Reihenfolge der For Next ist unglaublich wichtig:
            // Sind wenigere VORHANDEN vorhanden als FilterWerte, dann durchsucht diese Routine zu wenig Einträge,
            // bevor sie bei einem UND ein False zurückgibt
            foreach (var t1 in fi.SearchValue) {
                var bedingungErfüllt = false;
                foreach (var t in vorhandenWerte) {
                    bedingungErfüllt = CompareValues(t, t1, typ);
                    if (oder && bedingungErfüllt) { return true; }// Irgendein vorhandener Value trifft zu!!! Super!!!
                    if (und && bedingungErfüllt) { break; }// Irgend ein vorhandener Value trifft zu, restliche Prüfung uninteresant
                }
                if (und && !bedingungErfüllt) // Einzelne UND konnte nicht erfüllt werden...
                {
                    return false;
                }
            }
            if (und) { return true; } // alle "Und" stimmen!
            return false; // Gar kein "Oder" trifft zu...
        } catch (Exception ex) {
            Develop.DebugPrint("Unerwarteter Filter-Fehler", ex);
            Pause(1, true);
            Develop.CheckStackForOverflow();
            return MatchesTo(column, row, fi);
        }
    }

    public string Set(ColumnItem? column, RowItem? row, string value) // Main Method
    {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return "Datenbank ungültig!"; }

        if (column == null || column.IsDisposed) { return "Spalte ungültig!"; }

        if (row == null || row.IsDisposed) { return "Zeile ungültig!"; }

        if (!string.IsNullOrEmpty(db.FreezedReason)) { return "Datenbank eingefroren!"; }

        if (column.Format is DataFormat.Verknüpfung_zu_anderer_Datenbank) {
            var (lcolumn, lrow, _, _) = LinkedCellData(column, row, true, !string.IsNullOrEmpty(value));

            //return db.ChangeData(DatabaseDataType.Value_withoutSizeData, lcolumn, lrow, string.Empty, value, UserName, DateTime.UtcNow, string.Empty);
            lrow?.CellSet(lcolumn, value);
            return string.Empty;
        }

        value = column.AutoCorrect(value, true);
        var oldValue = GetStringCore(column, row);
        if (value == oldValue) { return string.Empty; }

        column.UcaseNamesSortedByLenght = null;

        var message = db.ChangeData(DatabaseDataType.Value_withoutSizeData, column, row, oldValue, value, UserName, DateTime.UtcNow, string.Empty);

        if (!string.IsNullOrEmpty(message)) { return message; }

        if (value != GetStringCore(column, row)) { return "Nachprüfung fehlgeschlagen"; }

        DoSpecialFormats(column, row, oldValue, false);

        //DoSystemColumns(db, column, row, UserName, DateTime.UtcNow, Reason.SystemSet);

        return string.Empty;
    }

    public void Set(ColumnItem? column, RowItem? row, bool value) => Set(column, row, value.ToPlusMinus());

    public void Set(ColumnItem? column, RowItem? row, DateTime value) => Set(column, row, value.ToString(Constants.Format_Date5, CultureInfo.InvariantCulture));

    public void Set(ColumnItem? column, RowItem? row, List<string>? value) => Set(column, row, value.JoinWithCr());

    public void Set(ColumnItem? column, RowItem? row, Point value) => Set(column, row, value.ToString());

    public void Set(ColumnItem? column, RowItem? row, int value) => Set(column, row, value.ToString());

    public void Set(ColumnItem? column, RowItem? row, double value) => Set(column, row, value.ToString(Constants.Format_Float1));

    public void SetSizeOfCellContent(ColumnItem column, RowItem row, Size contentSize) {
        var key = TextSizeKey(column, row.CellGetString(column));

        if (key == null) { return; }

        if (Sizes.ContainsKey(key)) {
            Sizes[key] = contentSize;
            return;
        }

        _ = Sizes.TryAdd(key, contentSize);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <param name="value"></param>
    /// <param name="reason"></param>
    public string SetValueInternal(ColumnItem column, RowItem row, string value, Reason reason) {
        if (column.Database is not DatabaseAbstract db || db.IsDisposed) { return "Datenbank ungültig"; }

        db.RefreshCellData(column, row, reason);

        var cellKey = KeyOfCell(column, row);

        if (ContainsKey(cellKey)) {
            var c = this[cellKey];
            c.Value = value; // Auf jeden Fall setzen. Auch falls es nachher entfernt wird, so ist es sicher leer
            if (string.IsNullOrEmpty(value)) {
                if (!TryRemove(cellKey, out _)) {
                    Develop.CheckStackForOverflow();
                    return SetValueInternal(column, row, value, reason);
                }
            }
        } else {
            if (!string.IsNullOrEmpty(value)) {
                if (!TryAdd(cellKey, new CellItem(value))) {
                    Develop.CheckStackForOverflow();
                    return SetValueInternal(column, row, value, reason);
                }
            }
        }

        if (reason == Reason.SetCommand) {
            if (column.ScriptType != ScriptType.Nicht_vorhanden) {
                Database?.Row.AddRowWithChangedValue(row);
            }
        }

        if (reason is Reason.SetCommand or Reason.UpdateChanges) {
            column.Invalidate_ContentWidth();
            row.InvalidateCheckData();
            OnCellValueChanged(new CellChangedEventArgs(column, row, reason));
        }

        return string.Empty;
    }

    public List<string> ValuesReadable(ColumnItem column, RowItem row, ShortenStyle style) => CellItem.ValuesReadable(column, row, style) ?? new List<string>();

    internal static List<RowItem?> ConnectedRowsOfRelations(string completeRelationText, RowItem? row) {
        List<RowItem?> allRows = new();
        if (row?.Database?.Column.First() == null || row.Database.IsDisposed) { return allRows; }

        var names = row.Database.Column.First()?.GetUcaseNamesSortedByLenght();
        var relationTextLine = completeRelationText.ToUpper().SplitAndCutByCr();
        foreach (var thisTextLine in relationTextLine) {
            var tmp = thisTextLine;
            List<RowItem?> r = new();
            if (names != null)
                for (var z = names.Count - 1; z > -1; z--) {
                    if (tmp.IndexOfWord(names[z], 0, RegexOptions.IgnoreCase) > -1) {
                        r.Add(row.Database.Row[names[z]]);
                        tmp = tmp.Replace(names[z], string.Empty);
                    }
                }

            if (r.Count == 1 || r.Contains(row)) { _ = allRows.AddIfNotExists(r); } // Bei mehr als einer verknüpften Reihe MUSS die die eigene Reihe dabei sein.
        }
        return allRows;
    }

    internal bool ChangeColumnName(string oldName, string newName) {
        oldName = oldName.ToUpper() + "|";
        newName = newName.ToUpper() + "|";

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

    internal string GetStringCore(ColumnItem? column, RowItem? row) {
        if (column == null || column.IsDisposed) { return string.Empty; }
        if (row == null || row.IsDisposed) { return string.Empty; }
        if (column.Database is not DatabaseAbstract db || db.IsDisposed) { return string.Empty; }

        db.RefreshCellData(column, row, Reason.SetCommand);

        var cellKey = KeyOfCell(column, row);

        return ContainsKey(cellKey) ? this[cellKey].Value : string.Empty;
    }

    internal void InvalidateAllSizes() {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }
        foreach (var thisColumn in db.Column) {
            thisColumn.Invalidate_ColumAndContent();
        }
    }

    internal void OnCellValueChanged(CellChangedEventArgs e) {
        e.Column.UcaseNamesSortedByLenght = null;
        CellValueChanged?.Invoke(this, e);
    }

    internal void RemoveOrphans() {
        try {
            List<string> removeKeys = new();

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
            Develop.CheckStackForOverflow(); // Um Rauszufinden, ob endlos-Schleifen öfters  vorkommen. Zuletzt 24.11.2020
        }
    }

    private static bool CompareValues(string istValue, string filterValue, FilterType typ) {
        // if (Column.Format == DataFormat.LinkedCell) { Develop.DebugPrint(enFehlerArt.Fehler, "Falscher Fremdzellenzugriff"); }
        if (typ.HasFlag(FilterType.GroßKleinEgal)) {
            istValue = istValue.ToUpper();
            filterValue = filterValue.ToUpper();
            typ ^= FilterType.GroßKleinEgal;
        }

        switch (typ) {
            case FilterType.Istgleich:
                return istValue == filterValue;

            case (FilterType)2: // Ungleich
                return istValue != filterValue;

            case FilterType.Instr:
                return istValue.Contains(filterValue);

            case FilterType.Between:
                if (!istValue.IsNumeral()) { return false; }
                _ = DoubleTryParse(istValue, out var ival);
                var fval = filterValue.SplitAndCutBy("|");

                _ = DoubleTryParse(fval[0], out var minv);
                if (ival < minv) { return false; }
                _ = DoubleTryParse(fval[1], out var maxv);
                if (ival > maxv) { return false; }
                //if (DoubleParse)
                //if (string.IsNullOrEmpty(IstValue)) { return false; }
                //if (!FilterValue.ToUpper().Contains("VALUE")) { return false; }
                //var d = modErgebnis.Ergebnis(FilterValue.Replace("VALUE", IstValue.Replace(",", "."), RegexOptions.IgnoreCase));
                //if (d == null) { return false; }
                //return d == -1;
                return true;

            case FilterType.KeinFilter:
                Develop.DebugPrint("Kein Filter!");
                return true;

            case FilterType.BeginntMit:
                return istValue.StartsWith(filterValue);

            case FilterType.AlwaysFalse:
                return false;

            default:
                Develop.DebugPrint(typ);
                return false;
        }
    }

    private static void MakeNewRelations(ColumnItem? column, RowItem? row, ICollection<string> oldBz, List<string> newBz) {
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
                        thisRow.CellSet(column, ex.SortedDistinctList());
                    }
                }
            }
        }
    }

    private static (ColumnItem? column, RowItem? row, string info, bool canrepair) RepairLinkedCellValue(DatabaseAbstract linkedDatabase, ColumnItem column, RowItem? row, bool addRowIfNotExists) {
        RowItem? targetRow = null;
        ColumnItem? targetColumn = null;
        var cr = false;

        // Repair nicht mehr erlauben, ergibt rekursieve Schleife, wir sind hier ja schon im repair
        var editableError = EditableErrorReason(column, row, EditableErrorReasonType.EditAcut, false, false, false, true);

        var db = column.Database;
        if (db == null || db.IsDisposed) { return Ergebnis("Verknüpfte Datenbank verworfen."); }

        if (!string.IsNullOrEmpty(editableError)) { return Ergebnis(editableError); }

        if (row == null || row.IsDisposed) { return Ergebnis("Keine Zeile zum finden des Zeilenschlüssels angegeben."); }

        targetColumn = linkedDatabase.Column.Exists(column.LinkedCell_ColumnNameOfLinkedDatabase);
        if (targetColumn == null) { return Ergebnis("Die Spalte ist in der Zieldatenbank nicht vorhanden."); }

        var (fc, info) = GetFilterFromLinkedCellData(linkedDatabase, column, row);
        if (!string.IsNullOrEmpty(info)) { return Ergebnis(info); }
        if (fc == null || fc.Count == 0) { return Ergebnis("Filter konnten nicht generiert werden: " + info); }

        var r = fc.Rows;
        switch (r.Count) {
            case > 1:
                return Ergebnis("Suchergebnis liefert mehrere Ergebnisse.");

            case 1:
                targetRow = r[0];
                break;

            default: {
                    if (addRowIfNotExists) {
                        targetRow = RowCollection.GenerateAndAdd(fc, "LinkedCell aus " + db.TableName);
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
                var newvalue = targetRow?.KeyName ?? string.Empty;
                if (targetColumn == null || !string.IsNullOrEmpty(fehler)) { newvalue = string.Empty; }
                //Nicht CellSet! Damit wird der Wert der Ziel-Datenbank verändert
                //row.CellSet(column, targetRow.KeyName);
                //  db.Cell.SetValue(column, row, targetRow.KeyName, UserName, DateTime.UtcNow, false);

                if (oldvalue != newvalue) {
                    fehler = db.ChangeData(DatabaseDataType.Value_withoutSizeData, column, row, oldvalue, newvalue, UserName, DateTime.UtcNow, string.Empty);
                }
            } else {
                if (string.IsNullOrEmpty(fehler)) { fehler = "Datenbankfehler"; }
            }
            return (targetColumn, targetRow, fehler, cr);
        }

        #endregion
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
            if (completeRelationText.ToUpper().Contains(names[z])) {
                var r = dbtmp.Row[names[z]];
                if (r != null && !r.IsDisposed) { DoReplace(names[z], r.KeyName); }
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
            if (Database != null) { Database.DisposingEvent -= _database_Disposing; }
            Database = null;
            Clear();
            IsDisposed = true;
        }
    }

    private void RelationTextNameChanged(ColumnItem columnToRepair, string rowKey, string oldValue, string newValue) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }

        if (string.IsNullOrEmpty(newValue)) { return; }
        foreach (var thisRowItem in db.Row) {
            if (thisRowItem != null) {
                if (!thisRowItem.CellIsNullOrEmpty(columnToRepair)) {
                    var t = thisRowItem.CellGetString(columnToRepair);
                    if (!string.IsNullOrEmpty(oldValue) && t.ToUpper().Contains(oldValue.ToUpper())) {
                        t = ChangeTextToRowId(t, oldValue, newValue, rowKey);
                        t = ChangeTextFromRowId(t);
                        var t2 = t.SplitAndCutByCrToList().SortedDistinctList();
                        thisRowItem.CellSet(columnToRepair, t2);
                    }
                    if (t.ToUpper().Contains(newValue.ToUpper())) {
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
            Set(column, row, currentString);
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
                        if (x.Contains(row)) {
                            _ = ex.Remove(t);
                        } else {
                            _ = ex.Remove(t.ReplaceWord(thisRow.CellFirstString(), row.CellFirstString(), RegexOptions.IgnoreCase));
                        }
                        thisRow.CellSet(column, ex.SortedDistinctList());
                    }
                }
            }
        }
        MakeNewRelations(column, row, oldBz, newBz);
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
    /// <summary>
    /// Ändert die anderen Zeilen dieser Spalte, so dass der verknüpfte Text bei dieser und den anderen Spalten gleich ist ab.
    /// </summary>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <param name="previewsValue"></param>
    private string? TextSizeKey(ColumnItem? column, string text) {
        if (column?.Database is not DatabaseAbstract db || db.IsDisposed || column.IsDisposed) { return null; }
        return db.TableName + "|" + column.KeyName + "|" + text;
    }

    #endregion
}