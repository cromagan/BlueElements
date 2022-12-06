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

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using static BlueBasics.Converter;
using static BlueBasics.Generic;

#nullable enable

namespace BlueDatabase;

public sealed class CellCollection : ConcurrentDictionary<string, CellItem>, IDisposableExtended {

    #region Fields

    private DatabaseAbstract? _database;

    #endregion

    #region Constructors

    public CellCollection(DatabaseAbstract database) : base() {
        _database = database;
        _database.Disposing += _database_Disposing;
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

    public event EventHandler<CellEventArgs> CellValueChanged;

    #endregion

    #region Properties

    public bool IsDisposed { get; private set; }

    #endregion

    #region Methods

    public static string AutomaticInitalValue(ColumnItem? column, RowItem? row) {
        if (column == null || row == null) { return string.Empty; }

        if (column.Format is DataFormat.Verknüpfung_zu_anderer_Datenbank) {
            var (lcolumn, lrow, _) = LinkedCellData(column, row, true, true);
            return AutomaticInitalValue(lcolumn, lrow);
        }

        if (column.VorschlagsColumn < 0) { return string.Empty; }
        var cc = column.Database.Column.SearchByKey(column.VorschlagsColumn);
        if (cc == null) { return string.Empty; }
        FilterCollection f = new(column.Database)
        {
            new FilterItem(cc, FilterType.Istgleich_GroßKleinEgal, row.CellGetString(cc)),
            new FilterItem(column, FilterType.Ungleich_MultiRowIgnorieren, string.Empty)
        };
        var rows = column.Database.Row.CalculateFilteredRows(f);
        rows.Remove(row);
        return rows.Count == 0 ? string.Empty : rows[0].CellGetString(column);
    }

    /// <summary>
    /// Gibt einen Fehlergrund zurück, ob die Zelle bearbeitet werden kann.
    /// Optional zusätzlich mit den Dateirechten.
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    public static string ErrorReason(ColumnItem? column, RowItem? row, ErrorReason mode) {
        if (mode == BlueBasics.Enums.ErrorReason.OnlyRead) { return string.Empty; }
        if (column == null) { return LanguageTool.DoTranslate("Es ist keine Spalte ausgewählt."); }
        var tmpf = column.Database.ErrorReason(mode);
        if (!string.IsNullOrEmpty(tmpf)) { return LanguageTool.DoTranslate(tmpf); }
        if (!column.SaveContent) { return LanguageTool.DoTranslate("Der Spalteninhalt wird nicht gespeichert."); }

        if (column.Format is DataFormat.Verknüpfung_zu_anderer_Datenbank) {
            var (lcolumn, lrow, info) = LinkedCellData(column, row, true, mode == BlueBasics.Enums.ErrorReason.EditAcut);

            if (!string.IsNullOrEmpty(info)) { return LanguageTool.DoTranslate(info); }

            if (lcolumn != null && lrow != null) {
                lcolumn.Database.PowerEdit = column.Database.PowerEdit;
                var tmp = ErrorReason(lcolumn, lrow, mode);
                return string.IsNullOrEmpty(tmp)
                    ? string.Empty
                    : LanguageTool.DoTranslate("Die verlinkte Zelle kann nicht bearbeitet werden: ") + tmp;
            }
            return LanguageTool.DoTranslate("Allgemeiner Fehler.");
        }

        if (row != null) {
            if (row.Database != column.Database) { return LanguageTool.DoTranslate("Interner Fehler: Bezug der Datenbank zur Zeile ist fehlerhaft."); }

            if (column.Database.PowerEdit.Subtract(DateTime.Now).TotalSeconds < 0) {
                if (column != column.Database.Column.SysLocked && row.CellGetBoolean(column.Database.Column.SysLocked) && !column.EditAllowedDespiteLock) { return LanguageTool.DoTranslate("Da die Zeile als abgeschlossen markiert ist, kann die Zelle nicht bearbeitet werden."); }
            }
        } else {
            //Auf neue Zeile wird geprüft
            if (!column.IsFirst()) { return LanguageTool.DoTranslate("Neue Zeilen müssen mit der ersten Spalte beginnen."); }
        }
        if (!column.TextBearbeitungErlaubt && !column.DropdownBearbeitungErlaubt) {
            return LanguageTool.DoTranslate("Die Inhalte dieser Spalte können nicht manuell bearbeitet werden, da keine Bearbeitungsmethode erlaubt ist.");
        }
        if (ColumnItem.UserEditDialogTypeInTable(column.Format, false, true, column.MultiLine) == EditTypeTable.None) {
            return "Interner Programm-Fehler: Es ist keine Bearbeitungsmethode für den Typ des Spalteninhalts '" + column.Format + "' definiert.";
        }
        //foreach (var ThisRule in Column.Database.Rules) {
        //    if (ThisRule != null) {
        //        if (ThisRule.WillAlwaysCellOverride(Column)) { return LanguageTool.DoTranslate("Diese Zelle wird von automatischen Regeln befüllt."); }
        //        if (ThisRule.BlockEditing(Column, Row)) { return LanguageTool.DoTranslate("Eine Regel sperrt diese Zelle."); }
        //    }
        //}
        return !column.Database.PermissionCheck(column.PermissionGroupsChangeCell, row)
            ? LanguageTool.DoTranslate("Sie haben nicht die nötigen Rechte, um diesen Wert zu ändern.")
            : string.Empty;
    }

    public static (List<FilterItem> filter, string info) GetFilterFromLinkedCellData(DatabaseAbstract linkedDatabase, ColumnItem column, RowItem? row) {
        var fi = new List<FilterItem>();

        for (var z = 0; z < Math.Min(column.LinkedCellFilter.Count, linkedDatabase.Column.Count); z++) {
            if (IntTryParse(column.LinkedCellFilter[z], out var key)) {
                var c = column.Database.Column.SearchByKey(key);
                if (c == null) { return (fi, "Eine Spalte, aus der der Zeilenschlüssel kommen soll, existiert nicht."); }
                var value = row.CellGetString(c);
                if (string.IsNullOrEmpty(value)) { return (fi, "Leere Suchwerte werden nicht unterstützt."); }
                fi.Add(new FilterItem(linkedDatabase.Column[z], FilterType.Istgleich, value));
            } else if (!string.IsNullOrEmpty(column.LinkedCellFilter[z]) && column.LinkedCellFilter[z].StartsWith("@")) {
                fi.Add(new FilterItem(linkedDatabase.Column[z], FilterType.Istgleich, column.LinkedCellFilter[z].Substring(1)));
            }
        }

        if (fi.Count == 0 && column.Format != DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems) { return (null, "Keine gültigen Suchkriterien definiert."); }

        return (fi, string.Empty);
    }

    public static string KeyOfCell(long colKey, long rowKey) => colKey + "|" + rowKey;

    public static string KeyOfCell(ColumnItem? column, RowItem? row) {
        // Alte verweise eleminieren.
        column = column?.Database?.Column.SearchByKey(column.Key);
        row = row?.Database?.Row.SearchByKey(row.Key);

        if (column != null && row != null) { return KeyOfCell(column.Key, row.Key); }
        if (column == null && row != null) { return KeyOfCell(-1, row.Key); }
        if (column != null && row == null) { return KeyOfCell(column.Key, -1); }

        return string.Empty;
    }

    public static (ColumnItem? column, RowItem? row, string info) LinkedCellData(string column, RowItem? row, bool repairLinkedValue, bool addRowIfNotExists) {
        if (row == null) { return (null, null, "Keine Zeile angegeben."); }

        var col = row.Database?.Column[column];
        return LinkedCellData(col, row, repairLinkedValue, addRowIfNotExists);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <param name="repairLinkedValue">Repariert - wenn möglich - denn Zellenbezug und schreibt ihn im Hintergrund in die Zelle.</param>
    /// <param name="addRowIfNotExists"></param>
    /// <returns></returns>
    public static (ColumnItem? column, RowItem? row, string info) LinkedCellData(ColumnItem? column, RowItem? row, bool repairLinkedValue, bool addRowIfNotExists) {
        if (column == null) { return (null, null, "Interner Spaltenfehler."); }

        if (column.Format is not DataFormat.Verknüpfung_zu_anderer_Datenbank) { return ((ColumnItem?)null, (RowItem?)null, "Format ist nicht LinkedCell."); }

        var linkedDatabase = column.LinkedDatabase;
        if (linkedDatabase == null) { return (null, null, "Verlinkte Datenbank nicht gefunden."); }

        //var skriptgesteuert = column.LinkedCell_RowKeyIsInColumn == -9999 && column.Format == DataFormat.Verknüpfung_zu_anderer_Datenbank_Skriptgesteuert;

        if (repairLinkedValue) { return RepairLinkedCellValue(linkedDatabase, column, row, addRowIfNotExists); }

        var key = column.Database.Cell.GetStringBehindLinkedValue(column, row);
        if (string.IsNullOrEmpty(key)) {
            return (linkedDatabase.Column.SearchByKey(column.LinkedCell_ColumnKeyOfLinkedDatabase), null, "Keine Verlinkung vorhanden.");
        }

        var v = key.SplitAndCutBy("|");
        if (v.Length != 2) { return RepairLinkedCellValue(linkedDatabase, column, row, addRowIfNotExists); }
        var linkedColumn = linkedDatabase.Column.SearchByKey(LongParse(v[0]));
        var linkedRow = linkedDatabase.Row.SearchByKey(LongParse(v[1]));

        if (KeyOfCell(linkedColumn, linkedRow) == key) { return (linkedColumn, linkedRow, string.Empty); }

        return RepairLinkedCellValue(linkedDatabase, column, row, addRowIfNotExists);
    }

    /// <summary>
    ///  Gibt zurück, ob die Zelle bearbeitet werden kann.
    ///  Optional zusätzlich mit den Dateirechten.
    /// </summary>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    public static bool UserEditPossible(ColumnItem? column, RowItem? row, ErrorReason mode) => string.IsNullOrEmpty(ErrorReason(column, row, mode));

    public void DataOfCellKey(string cellKey, out ColumnItem? column, out RowItem? row) {
        if (string.IsNullOrEmpty(cellKey)) {
            column = null;
            row = null;
            return;
        }
        var cd = cellKey.SplitAndCutBy("|");
        if (cd.GetUpperBound(0) != 1) { Develop.DebugPrint(FehlerArt.Fehler, "Falscher CellKey übergeben: " + cellKey); }
        column = _database?.Column.SearchByKey(LongParse(cd[0]));
        row = _database?.Row.SearchByKey(LongParse(cd[1]));
    }

    public void Delete(ColumnItem column, long rowKey) {
        var cellKey = KeyOfCell(column.Key, rowKey);
        if (!ContainsKey(cellKey)) { return; }

        if (!TryRemove(cellKey, out _)) {
            Develop.CheckStackForOverflow();
            Delete(column, rowKey);
        }
    }

    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Rechnet Cellbezüge und besondere Spalten neu durch, falls sich der Wert geändert hat.
    /// </summary>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <param name="previewsValue"></param>
    /// <param name="doAlways">Auch wenn der PreviewsValue gleich dem CurrentValue ist, wird die Routine durchberechnet</param>
    public void DoSpecialFormats(ColumnItem? column, RowItem? row, string previewsValue, bool doAlways) {
        if (_database == null || _database.IsDisposed) { return; }
        if (row == null) { return; }
        if (column == null) { _database?.DevelopWarnung("Spalte ungültig!"); Develop.DebugPrint(FehlerArt.Fehler, "Spalte ungültig!<br>" + _database.ConnectionData.TableName); }

        var currentValue = GetString(column, row);

        switch (column!.Format) {
            case DataFormat.RelationText:

                if (doAlways || currentValue != previewsValue) {
                    RepairRelationText(column, row, previewsValue);
                    SetSameValueOfKey(column, row, currentValue);
                }
                break;

            case DataFormat.Verknüpfung_zu_anderer_Datenbank:
                if (doAlways) {
                    LinkedCellData(column, row, true, false); // Repariert auch Cellbezüge
                }
                break;
        }

        if (!doAlways && currentValue == previewsValue) { return; }

        if (!string.IsNullOrEmpty(column.Am_A_Key_For_Other_Column)) {
            SetSameValueOfKey(column, row, currentValue);
        }
        if (column.IsFirst()) {
            foreach (var thisColumnItem in _database.Column.Where(thisColumnItem => thisColumnItem != null)) {
                switch (thisColumnItem.Format) {
                    //case DataFormat.Relation:
                    //    RelationNameChanged(ThisColumnItem, PreviewsValue, CurrentValue);
                    //    break;
                    case DataFormat.RelationText:
                        RelationTextNameChanged(thisColumnItem, row.Key, previewsValue, currentValue);
                        break;
                }
            }
        }
        if (column.KeyColumnKey > -1) {
            ChangeValueOfKey(currentValue, column, row.Key);
        }
    }

    public bool GetBoolean(string columnName, RowItem? row) => GetBoolean(_database?.Column[columnName], row);

    public bool GetBoolean(ColumnItem? column, RowItem? row) => GetString(column, row).FromPlusMinus();// Main Method

    public Color GetColor(string columnName, RowItem? row) => GetColor(_database?.Column[columnName], row);

    public Color GetColor(ColumnItem? column, RowItem? row) => Color.FromArgb(GetInteger(column, row)); // Main Method

    public int GetColorBgr(ColumnItem? column, RowItem? row) {
        var c = GetColor(column, row);
        int colorBlue = c.B;
        int colorGreen = c.G;
        int colorRed = c.R;
        return (colorBlue << 16) | (colorGreen << 8) | colorRed;
    }

    public DateTime GetDateTime(string columnName, RowItem? row) => GetDateTime(_database?.Column[columnName], row);

    public DateTime GetDateTime(ColumnItem? column, RowItem? row) // Main Method
    {
        var @string = GetString(column, row);
        return string.IsNullOrEmpty(@string) ? default : DateTimeTryParse(@string, out var d) ? d : default;
    }

    public double GetDouble(string columnName, RowItem? row) => GetDouble(_database?.Column[columnName], row);

    public double GetDouble(ColumnItem? column, RowItem? row) // Main Method
    {
        var x = GetString(column, row);
        return string.IsNullOrEmpty(x) ? 0 : DoubleParse(x);
    }

    public int GetInteger(string columnName, RowItem? row) => GetInteger(_database?.Column[columnName], row);

    public int GetInteger(ColumnItem? column, RowItem? row) // Main Method
    {
        var x = GetString(column, row);
        return string.IsNullOrEmpty(x) ? 0 : IntParse(x);
    }

    public List<string> GetList(string columnName, RowItem? row) => GetList(_database?.Column[columnName], row);

    public List<string> GetList(ColumnItem? column, RowItem? row) => GetString(column, row).SplitAndCutByCrToList();// Main Method

    public Point GetPoint(string columnName, RowItem? row) => GetPoint(_database?.Column[columnName], row);

    public Point GetPoint(ColumnItem? column, RowItem? row) // Main Method
    {
        var @string = GetString(column, row);
        return string.IsNullOrEmpty(@string) ? Point.Empty : @string.PointParse();
    }

    public Size GetSizeOfCellContent(ColumnItem column, RowItem row) {
        var cellKey = KeyOfCell(column, row);
        if (column.Database != null && ContainsKey(cellKey)) {
            return column.Database.Cell[cellKey].Size;
        }

        return Size.Empty;
    }

    public string GetString(string columnName, RowItem? row) => GetString(_database?.Column[columnName], row);

    public string GetString(ColumnItem? column, RowItem? row) // Main Method
    {
        try {
            if (column == null) { _database?.DevelopWarnung("Spalte ungültig!"); Develop.DebugPrint(FehlerArt.Fehler, "Spalte ungültig!<br>" + _database.ConnectionData.TableName); }
            if (row == null) { Develop.DebugPrint(FehlerArt.Fehler, "Zeile ungültig!<br>" + _database.ConnectionData.TableName); }
            if (column.Format is DataFormat.Verknüpfung_zu_anderer_Datenbank) {
                var (lcolumn, lrow, _) = LinkedCellData(column, row, false, false);
                return lcolumn != null && lrow != null ? lrow.CellGetString(lcolumn) : string.Empty;
            }
            var cellKey = KeyOfCell(column, row);

            if (!column.Loaded) {
                _database.RefreshRowData(row, false);
            }

            return !ContainsKey(cellKey) ? string.Empty : this[cellKey].Value;
        } catch {
            // Manchmal verscwhindwet der vorhandene Key?!?
            return GetString(column, row);
        }
    }

    public string GetStringBehindLinkedValue(ColumnItem? column, RowItem? row) {
        if (column == null || row == null) { return string.Empty; }
        var cellKey = KeyOfCell(column, row);
        return !ContainsKey(cellKey) ? string.Empty : this[cellKey].Value;
    }

    public void Initialize() => Clear();

    public bool IsNullOrEmpty(ColumnItem? column, RowItem? row) {
        if (column == null) { return true; }
        if (row == null) { return true; }
        if (column.Format is DataFormat.Verknüpfung_zu_anderer_Datenbank) {
            var (lcolumn, lrow, _) = LinkedCellData(column, row, false, false);
            return lcolumn == null || lrow == null || lrow.CellIsNullOrEmpty(lcolumn);
        }
        var cellKey = KeyOfCell(column.Key, row.Key);
        return !ContainsKey(cellKey) || string.IsNullOrEmpty(this[cellKey].Value);
    }

    public bool IsNullOrEmpty(string columnName, RowItem? row) => IsNullOrEmpty(_database.Column[columnName], row);

    public bool IsNullOrEmpty(string cellKey) {
        DataOfCellKey(cellKey, out var column, out var row);
        return IsNullOrEmpty(column, row);
    }

    public bool MatchesTo(ColumnItem? column, RowItem? row, FilterItem filter) {
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
            var typ = filter.FilterType;
            // Oder-Flag ermitteln --------------------------------------------
            var oder = typ.HasFlag(FilterType.ODER);
            if (oder) { typ ^= FilterType.ODER; }
            // Und-Flag Ermitteln --------------------------------------------
            var und = typ.HasFlag(FilterType.UND);
            if (und) { typ ^= FilterType.UND; }
            if (filter.SearchValue.Count < 2) {
                oder = true;
                und = false; // Wenn nur EIN Eintrag gecheckt wird, ist es EGAL, ob UND oder ODER.
            }
            //if (Und && Oder) { Develop.DebugPrint(enFehlerArt.Fehler, "Filter-Anweisung erwartet ein 'Und' oder 'Oder': " + ToString()); }
            // Tatsächlichen String ermitteln --------------------------------------------
            var @string = string.Empty;
            var fColumn = column;
            if (column.Format is DataFormat.Verknüpfung_zu_anderer_Datenbank) {
                var (columnItem, rowItem, _) = LinkedCellData(column, row, false, false);
                if (columnItem != null && rowItem != null) {
                    @string = rowItem.CellGetString(columnItem);
                    fColumn = columnItem;
                }
            } else {
                var cellKey = KeyOfCell(column, row);
                @string = ContainsKey(cellKey) ? this[cellKey].Value : string.Empty;
            }
            if (@string is null) { @string = string.Empty; }
            if (typ.HasFlag(FilterType.Instr)) { @string = LanguageTool.ColumnReplace(@string, fColumn, ShortenStyle.Both); }
            // Multiline-Typ ermitteln  --------------------------------------------
            var tmpMultiLine = false;
            if (column != null) { tmpMultiLine = column.MultiLine; }
            if (typ.HasFlag(FilterType.MultiRowIgnorieren)) {
                tmpMultiLine = false;
                typ ^= FilterType.MultiRowIgnorieren;
            }
            if (tmpMultiLine && !@string.Contains("\r")) { tmpMultiLine = false; } // Zeilen mit nur einem Eintrag können ohne Multiline behandel werden.
            //if (Typ == enFilterType.KeinFilter)
            //{
            //    Develop.DebugPrint(enFehlerArt.Fehler, "'Kein Filter' wurde übergeben: " + ToString());
            //}
            if (!tmpMultiLine) {
                var bedingungErfüllt = false;
                foreach (var t in filter.SearchValue) {
                    bedingungErfüllt = CompareValues(@string, t, typ);
                    if (oder && bedingungErfüllt) { return true; }
                    if (und && bedingungErfüllt == false) { return false; } // Bei diesem UND hier müssen allezutreffen, deshalb kann getrost bei einem False dieses zurückgegeben werden.
                }
                return bedingungErfüllt;
            }
            List<string> vorhandenWerte = new(@string.SplitAndCutByCr());
            if (vorhandenWerte.Count == 0) // Um den Filter, der nach 'Leere' Sucht, zu befriediegen
            {
                vorhandenWerte.Add("");
            }
            // Diese Reihenfolge der For Next ist unglaublich wichtig:
            // Sind wenigere VORHANDEN vorhanden als FilterWerte, dann durchsucht diese Routine zu wenig Einträge,
            // bevor sie bei einem UND ein False zurückgibt
            foreach (var t1 in filter.SearchValue) {
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
            Develop.DebugPrint(ex);
            Pause(1, true);
            return MatchesTo(column, row, filter);
        }
    }

    public void Set(string columnName, RowItem? row, string value) => Set(_database?.Column[columnName], row, value);

    public void Set(ColumnItem? column, RowItem? row, string value) // Main Method
    {
        _database.BlockReload(false);
        if (column == null) { _database?.DevelopWarnung("Spalte ungültig!"); Develop.DebugPrint(FehlerArt.Fehler, "Spalte ungültig!<br>" + _database.ConnectionData.TableName); }
        if (row == null) { Develop.DebugPrint(FehlerArt.Fehler, "Zeile ungültig!!<br>" + _database.ConnectionData.TableName); }
        if (column.Format is DataFormat.Verknüpfung_zu_anderer_Datenbank) {
            var (lcolumn, lrow, _) = LinkedCellData(column, row, true, !string.IsNullOrEmpty(value));
            lrow?.CellSet(lcolumn, value);
            return;
        }
        SetValueBehindLinkedValue(column, row, value, true);
    }

    public void Set(string columnName, RowItem? row, bool value) => Set(_database?.Column[columnName], row, value.ToPlusMinus());

    public void Set(ColumnItem? column, RowItem? row, bool value) => Set(column, row, value.ToPlusMinus());

    public void Set(string columnName, RowItem? row, DateTime value) => Set(_database?.Column[columnName], row, value.ToString(Constants.Format_Date5));

    public void Set(ColumnItem? column, RowItem? row, DateTime value) => Set(column, row, value.ToString(Constants.Format_Date5));

    public void Set(string columnName, RowItem? row, List<string>? value) => Set(_database?.Column[columnName], row, value);

    public void Set(ColumnItem? column, RowItem? row, List<string>? value) => Set(column, row, value.JoinWithCr());

    public void Set(string columnName, RowItem? row, Point value) => Set(_database?.Column[columnName], row, value);

    public void Set(ColumnItem? column, RowItem? row, Point value) => Set(column, row, value.ToString());

    public void Set(string columnName, RowItem? row, int value) => Set(_database?.Column[columnName], row, value.ToString());

    // Main Method// {X=253,Y=194} MUSS ES SEIN, prüfen
    public void Set(ColumnItem? column, RowItem? row, int value) => Set(column, row, value.ToString());

    public void Set(string columnName, RowItem? row, double value) => Set(_database?.Column[columnName], row, value.ToString(Constants.Format_Float1));

    public void Set(ColumnItem? column, RowItem? row, double value) => Set(column, row, value.ToString(Constants.Format_Float1));

    /// <summary>
    ///
    /// </summary>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <param name="contentSize">Wird im Scale der Datenbank gespeichert, da es ja Benutzerübergreifend ist</param>
    public void SetSizeOfCellContent(ColumnItem? column, RowItem? row, Size contentSize) {
        var cellKey = KeyOfCell(column, row);
        if (!ContainsKey(cellKey)) { return; }
        this[cellKey].Size = contentSize;
    }

    public string SetValueInternal(long columnkey, long rowkey, string value, int width, int height, bool isLoading) {
        if (rowkey < 0) { return "Row konnte nicht generiert werden."; }
        if (columnkey < 0) { return "Column konnte nicht generiert werden."; }

        var cellKey = KeyOfCell(columnkey, rowkey);

        if (ContainsKey(cellKey)) {
            var c = this[cellKey];
            c.Value = value; // Auf jeden Fall setzen. Auch falls es nachher entfernt wird, so ist es sicher leer
            c.Size = width > 0 ? new Size(width, height) : Size.Empty;
            if (string.IsNullOrEmpty(value)) {
                if (!TryRemove(cellKey, out _)) {
                    Develop.CheckStackForOverflow();
                    return SetValueInternal(columnkey, rowkey, value, width, height, isLoading);
                }
            }
        } else {
            if (!string.IsNullOrEmpty(value)) {
                if (!TryAdd(cellKey, new CellItem(value, width, height))) {
                    Develop.CheckStackForOverflow();
                    return SetValueInternal(columnkey, rowkey, value, width, height, isLoading);
                }
            }
        }

        return string.Empty;
    }

    public List<string> ValuesReadable(ColumnItem? column, RowItem row, ShortenStyle style) => CellItem.ValuesReadable(column, row, style);

    internal static List<RowItem?> ConnectedRowsOfRelations(string completeRelationText, RowItem? row) {
        List<RowItem?> allRows = new();
        var names = row.Database.Column[0].GetUcaseNamesSortedByLenght();
        var relationTextLine = completeRelationText.ToUpper().SplitAndCutByCr();
        foreach (var thisTextLine in relationTextLine) {
            var tmp = thisTextLine;
            List<RowItem?> r = new();
            for (var z = names.Count - 1; z > -1; z--) {
                if (tmp.IndexOfWord(names[z], 0, RegexOptions.IgnoreCase) > -1) {
                    r.Add(row.Database.Row[names[z]]);
                    tmp = tmp.Replace(names[z], string.Empty);
                }
            }
            if (r.Count == 1 || r.Contains(row)) { allRows.AddIfNotExists(r); } // Bei mehr als einer verknüpften Reihe MUSS die die eigene Reihe dabei sein.
        }
        return allRows;
    }

    internal static void Invalidate_CellContentSize(ColumnItem? column, RowItem? row) {
        var cellKey = KeyOfCell(column, row);
        if (column.Database.Cell.ContainsKey(cellKey)) {
            column.Database.Cell[cellKey].InvalidateSize();
        }
    }

    internal string CompareKey(ColumnItem? column, RowItem? row) => GetString(column, row).CompareKey(column.SortType);

    internal Size ContentSizeToSave(KeyValuePair<string, CellItem> vCell, ColumnItem column) {
        if (column.Format.SaveSizeData()) {
            if (vCell.Value.Size.Height > 4 &&
                vCell.Value.Size.Height < 65025 &&
                vCell.Value.Size.Width > 4 &&
                vCell.Value.Size.Width < 65025) { return vCell.Value.Size; }
        }
        return Size.Empty;
    }

    internal void InvalidateAllSizes() {
        foreach (var thisColumn in _database.Column) {
            thisColumn.Invalidate_ColumAndContent();
        }
    }

    internal void OnCellValueChanged(CellEventArgs e) {
        e.Column.UcaseNamesSortedByLenght = null;
        CellValueChanged?.Invoke(this, e);
    }

    internal void RemoveOrphans() {
        try {
            List<string?> removeKeys = new();

            foreach (var pair in this.Where(pair => !string.IsNullOrEmpty(pair.Value.Value))) {
                DataOfCellKey(pair.Key, out var column, out var row);
                if (column == null || row == null) { removeKeys.Add(pair.Key); }
            }

            if (removeKeys.Count == 0) { return; }

            foreach (var thisKey in removeKeys) {
                if (!TryRemove(thisKey, out _)) {
                    Develop.CheckStackForOverflow();
                    RemoveOrphans();
                    return;
                }
            }

            return;
        } catch {
            Develop.CheckStackForOverflow(); // Um Rauszufinden, ob endlos-Schleifen öfters  vorkommen. Zuletzt 24.11.2020
            return;
        }
    }

    /// <summary>
    /// Diese Routine setzt dern Wert direkt in die Zelle. Verlinkete Zellen werden nicht weiter verfolgt.
    /// Deswegen kann nur mit dieser Routine der Cell-Link eingefügt werden.
    /// Die Routine blockt zwar den Reload, aber eine endgültige Prüfung verhindet ein Ändern, wenn sich der Wert nicht verändert hat
    /// </summary>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <param name="value"></param>
    /// <param name="changeSysColumns"></param>
    internal void SetValueBehindLinkedValue(ColumnItem column, RowItem row, string value, bool changeSysColumns) {
        if (_database == null) { return; }

        _database.BlockReload(false);
        if (column == null) {
            _database?.DevelopWarnung("Spalte ungültig!");
            Develop.DebugPrint(FehlerArt.Fehler, "Spalte ungültig!<br>" + _database.ConnectionData.TableName);
            return;
        }
        if (row == null) {
            _database?.DevelopWarnung("Zeile ungültig!!");
            Develop.DebugPrint(FehlerArt.Fehler, "Zeile ungültig!!<br>" + _database.ConnectionData.TableName);
            return;
        }

        if (column?.Format is not DataFormat.Verknüpfung_zu_anderer_Datenbank) { value = column?.AutoCorrect(value); }

        var cellKey = KeyOfCell(column, row);
        var oldValue = string.Empty;
        if (ContainsKey(cellKey)) { oldValue = this[cellKey].Value; }
        if (value == oldValue) { return; }

        _database?.WaitEditable();
        _database?.ChangeData(DatabaseDataType.Value_withoutSizeData, column.Key, row.Key, oldValue, value);
        column.UcaseNamesSortedByLenght = null;

        if (changeSysColumns) {
            DoSpecialFormats(column, row, oldValue, false);
            SystemSet(_database?.Column.SysRowChanger, row, _database?.UserName);
            SystemSet(_database?.Column.SysRowChangeDate, row, DateTime.UtcNow.ToString(Constants.Format_Date5));
            //column.TimeCode = tc;
        }

        Invalidate_CellContentSize(column, row);
        column.Invalidate_ContentWidth();
        OnCellValueChanged(new CellEventArgs(column, row));
    }

    internal void SystemSet(ColumnItem? column, RowItem? row, string value) {
        if (column == null) { _database?.DevelopWarnung("Spalte ungültig!"); Develop.DebugPrint(FehlerArt.Fehler, "Spalte ungültig!<br>" + _database.ConnectionData.TableName); }
        if (row == null) { Develop.DebugPrint(FehlerArt.Fehler, "Zeile ungültig!<br>" + _database.ConnectionData.TableName); }
        if (!column.IsSystemColumn()) { Develop.DebugPrint(FehlerArt.Fehler, "SystemSet nur bei System-Spalten möglich: " + ToString()); }
        if (!column.SaveContent) { return; }
        var cellKey = KeyOfCell(column, row);
        var @string = string.Empty;
        if (ContainsKey(cellKey)) {
            @string = this[cellKey].Value;
        } else {
            if (!TryAdd(cellKey, new CellItem(string.Empty, 0, 0))) {
                Develop.CheckStackForOverflow();
                SystemSet(column, row, value);
                return;
            }
        }
        if (value == @string) { return; }
        _database.ChangeData(DatabaseDataType.Value_withoutSizeData, column.Key, row.Key, @string, value);
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
                return Convert.ToBoolean(istValue == filterValue);

            case (FilterType)2: // Ungleich
                return Convert.ToBoolean(istValue != filterValue);

            case FilterType.Instr:
                return istValue.Contains(filterValue);

            case FilterType.Between:
                if (!istValue.IsNumeral()) { return false; }
                DoubleTryParse(istValue, out var ival);
                var fval = filterValue.SplitAndCutBy("|");

                DoubleTryParse(fval[0], out var minv);
                if (ival < minv) { return false; }
                DoubleTryParse(fval[1], out var maxv);
                if (ival > maxv) { return false; }
                //if (DoubleParse)
                //if (string.IsNullOrEmpty(IstValue)) { return false; }
                //if (!FilterValue.ToUpper().Contains("VALUE")) { return false; }
                //var d = modErgebnis.Ergebnis(FilterValue.Replace("VALUE", IstValue.Replace(",", "."), RegexOptions.IgnoreCase));
                //if (d == null) { return false; }
                //return Convert.ToBoolean(d == -1);
                return true;

            case FilterType.KeinFilter:
                Develop.DebugPrint("Kein Filter!");
                return true;

            case FilterType.BeginntMit:
                return istValue.StartsWith(filterValue);

            default: {
                    Develop.DebugPrint(typ);
                    return false;
                }
        }
    }

    private static void MakeNewRelations(ColumnItem? column, RowItem? row, ICollection<string> oldBz, List<string> newBz) {
        //Develop.CheckStackForOverflow();
        //// Dann die neuen Erstellen
        foreach (var t in newBz) {
            if (!oldBz.Contains(t)) {
                var x = ConnectedRowsOfRelations(t, row);
                foreach (var thisRow in x) {
                    if (thisRow != row) {
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

    private static (ColumnItem? column, RowItem? row, string info) RepairLinkedCellValue(DatabaseAbstract linkedDatabase, ColumnItem column, RowItem? row, bool addRowIfNotExists) {
        RowItem? targetRow = null;

        /// Spalte aus der Ziel-Datenbank ermitteln
        var targetColumn = linkedDatabase.Column.SearchByKey(column.LinkedCell_ColumnKeyOfLinkedDatabase);
        if (targetColumn == null) { return Ergebnis("Die Spalte ist in der Zieldatenbank nicht vorhanden."); }

        /// Zeile aus der Ziel-Datenbank ermitteln
        if (row == null) { return Ergebnis("Keine Zeile zum finden des Zeilenschlüssels angegeben."); }

        //if (column.Format != DataFormat.Verknüpfung_zu_anderer_Datenbank) {
        //    var linkedCellRowIsInColumn = column.Database?.Column.SearchByKey(column.LinkedCell_RowKeyIsInColumn);
        //    if (linkedCellRowIsInColumn == null) { return Ergebnis("Die Spalte, aus der der Zeilenschlüssel kommen soll, existiert nicht."); }
        //    if (row.CellIsNullOrEmpty(linkedCellRowIsInColumn)) { return Ergebnis("Kein Zeilenschlüssel angegeben."); }

        //    targetRow = linkedDatabase.Row[row.CellGetString(linkedCellRowIsInColumn)];
        //    if (targetRow == null && addRowIfNotExists) {
        //        targetRow = linkedDatabase.Row.GenerateAndAdd(row.CellGetString(linkedCellRowIsInColumn));
        //    }
        //} else {
        var (filter, info) = GetFilterFromLinkedCellData(linkedDatabase, column, row);
        if (!string.IsNullOrEmpty(info)) { return Ergebnis(info); }

        var r = linkedDatabase.Row.CalculateFilteredRows(filter);
        if (r.Count > 1) { return Ergebnis("Suchergebnis liefert mehrere Ergebnisse."); }

        if (r.Count == 1) {
            targetRow = r[0];
        } else {
            if (addRowIfNotExists) {
                targetRow = linkedDatabase.Row.GenerateAndAdd(filter);
            }
        }
        //   }

        return targetRow == null ? Ergebnis("Die Zeile ist in der Zieldatenbank nicht vorhanden.") : Ergebnis(string.Empty);

        #region Subroutine Ergebnis

        (ColumnItem? column, RowItem? row, string info) Ergebnis(string fehler) {
            column.Database?.BlockReload(false);
            if (targetColumn != null && targetRow != null && string.IsNullOrEmpty(fehler)) {
                column.Database?.Cell.SetValueBehindLinkedValue(column, row, KeyOfCell(targetColumn.Key, targetRow.Key), true);
                return (targetColumn, targetRow, fehler);
            }

            if (column != null && row != null) { column.Database?.Cell.SetValueBehindLinkedValue(column, row, string.Empty, true); }
            return (targetColumn, targetRow, fehler);
        }

        #endregion
    }

    private void _database_Disposing(object sender, System.EventArgs e) => Dispose();

    private string ChangeTextFromRowId(string completeRelationText) {
        foreach (var rowItem in _database.Row) {
            if (rowItem != null) {
                completeRelationText = completeRelationText.Replace("/@X" + rowItem.Key + "X@/", rowItem.CellFirstString());
            }
        }
        return completeRelationText;
    }

    private string ChangeTextToRowId(string completeRelationText, string oldValue, string newValue, long keyOfCHangedRow) {
        var names = _database.Column[0].GetUcaseNamesSortedByLenght();
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
                DoReplace(names[z], _database.Row[names[z]].Key);
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
        void DoReplace(string name, long key) {
            if (!string.IsNullOrEmpty(name)) {
                completeRelationText = completeRelationText.Replace(name, "/@X" + key + "X@/", RegexOptions.IgnoreCase);
            }
        }
    }

    /// <summary>
    /// Ändert bei allen Zeilen - die den gleichen Key (KeyColumn, Relation) benutzen wie diese Zeile - den Inhalt der Zellen ab um diese gleich zu halten.
    /// </summary>
    /// <param name="currentvalue"></param>
    /// <param name="column"></param>
    /// <param name="rowKey"></param>
    private void ChangeValueOfKey(string currentvalue, ColumnItem? column, long rowKey) {
        var keyc = _database.Column.SearchByKey(column.KeyColumnKey); // Schlüsselspalte für diese Spalte bestimmen
        if (keyc is null) { return; }

        var ownRow = _database.Row.SearchByKey(rowKey);
        var rows = keyc.Format == DataFormat.RelationText
            ? ConnectedRowsOfRelations(ownRow.CellGetString(keyc), ownRow)
            : RowCollection.MatchesTo(new FilterItem(keyc, FilterType.Istgleich_GroßKleinEgal, ownRow.CellGetString(keyc)));
        rows.Remove(ownRow);
        if (rows.Count < 1) { return; }
        foreach (var thisRow in rows) {
            thisRow.CellSet(column, currentvalue);
        }
    }

    private void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
            }
            // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
            // TODO: Große Felder auf NULL setzen
            _database.Disposing -= _database_Disposing;
            _database = null;
            Clear();
            IsDisposed = true;
        }
    }

    private void RelationTextNameChanged(ColumnItem? columnToRepair, long rowKey, string oldValue, string newValue) {
        if (string.IsNullOrEmpty(newValue)) { return; }
        foreach (var thisRowItem in _database.Row) {
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

    /// <summary>
    /// Ändert die anderen Zeilen dieser Spalte, so dass der verknüpfte Text bei dieser und den anderen Spalten gleich ist ab.
    /// </summary>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <param name="previewsValue"></param>
    private void RepairRelationText(ColumnItem? column, RowItem? row, string previewsValue) {
        var currentString = GetString(column, row);
        currentString = ChangeTextToRowId(currentString, string.Empty, string.Empty, -1);
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
                            ex.Remove(t);
                        } else {
                            ex.Remove(t.ReplaceWord(thisRow.CellFirstString(), row.CellFirstString(), RegexOptions.IgnoreCase));
                        }
                        thisRow.CellSet(column, ex.SortedDistinctList());
                    }
                }
            }
        }
        MakeNewRelations(column, row, oldBz, newBz);
    }

    /// <summary>
    /// Ändert bei allen anderen Spalten den Inhalt der Zelle ab (um diese gleich zuhalten), wenn diese Spalte der Key für die anderen ist.
    /// </summary>
    /// <param name="column"></param>
    /// <param name="ownRow"></param>
    /// <param name="currentvalue"></param>
    private void SetSameValueOfKey(ColumnItem? column, RowItem ownRow, string currentvalue) {
        if (_database == null || _database.IsDisposed) { return; }
        if (column == null) { return; }
        List<RowItem>? rows = null;

        foreach (var thisColumn in _database.Column) {
            //if (thisColumn.LinkedCell_RowKeyIsInColumn == column.Key) {
            //    LinkedCellData(thisColumn, ownRow, true, false); // Repariert auch Zellbezüge
            //}

            if (thisColumn.Format == DataFormat.Verknüpfung_zu_anderer_Datenbank) {
                foreach (var thisV in thisColumn.LinkedCellFilter) {
                    if (IntTryParse(thisV, out var key)) {
                        if (key == column.Key) { LinkedCellData(thisColumn, ownRow, true, false); break; }
                    }
                }
            }

            if (thisColumn.KeyColumnKey == column.Key) {
                if (rows == null) {
                    rows = column.Format == DataFormat.RelationText
                        ? ConnectedRowsOfRelations(currentvalue, ownRow)
                        : RowCollection.MatchesTo(new FilterItem(column, FilterType.Istgleich_GroßKleinEgal, currentvalue));
                    rows.Remove(ownRow);
                }
                if (rows.Count < 1) {
                    ownRow.CellSet(thisColumn, string.Empty);
                } else {
                    ownRow.CellSet(thisColumn, rows[0].CellGetString(thisColumn));
                }
            }
        }
    }

    #endregion
}