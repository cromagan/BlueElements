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
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using static BlueBasics.Converter;
using static BlueBasics.Extensions;
using static BlueBasics.Generic;

namespace BlueDatabase {

    public sealed class CellCollection : Dictionary<string, CellItem>, IDisposable {

        #region Fields

        private Database _database;
        private bool disposedValue;

        #endregion

        #region Constructors

        public CellCollection(Database database) : base() {
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
            Dispose(disposing: false);
        }

        #endregion

        #region Events

        public event EventHandler<CellEventArgs> CellValueChanged;

        #endregion

        #region Methods

        public static string AutomaticInitalValue(ColumnItem column, RowItem row) {
            if (column == null || row == null) { return string.Empty; }

            if (column.Format == enDataFormat.LinkedCell) {
                (var lcolumn, var lrow) = LinkedCellData(column, row, true, true);
                return AutomaticInitalValue(lcolumn, lrow);
            }

            if (column.VorschlagsColumn < 0) { return string.Empty; }
            var cc = column.Database.Column.SearchByKey(column.VorschlagsColumn);
            if (cc == null) { return string.Empty; }
            FilterCollection F = new(column.Database)
            {
                new FilterItem(cc, enFilterType.Istgleich_GroßKleinEgal, row.CellGetString(cc)),
                new FilterItem(column, enFilterType.Ungleich_MultiRowIgnorieren, string.Empty)
            };
            var rows = column.Database.Row.CalculateFilteredRows(F);
            rows.Remove(row);
            return rows.Count == 0 ? string.Empty : rows[0].CellGetString(column);
        }

        /// <summary>
        /// Gibt einen Fehlergrund zurück, ob die Zelle bearbeitet werden kann.
        /// Optional zusätzlich mit den Dateirechten.
        /// </summary>
        /// <param name="Row"></param>
        /// <param name="DateiRechtePrüfen"></param>
        /// <param name="Column"></param>
        /// <returns></returns>
        public static string ErrorReason(ColumnItem Column, RowItem Row, enErrorReason mode) {
            if (mode == enErrorReason.OnlyRead) { return string.Empty; }
            if (Column == null) { return LanguageTool.DoTranslate("Es ist keine Spalte ausgewählt."); }
            var tmpf = Column.Database.ErrorReason(mode);
            if (!string.IsNullOrEmpty(tmpf)) { return LanguageTool.DoTranslate(tmpf); }
            if (!Column.SaveContent) { return LanguageTool.DoTranslate("Der Spalteninhalt wird nicht gespeichert."); }

            if (Column.Format == enDataFormat.LinkedCell) {
                (var lcolumn, var lrow) = LinkedCellData(Column, Row, true, false);
                if (lcolumn != null && lrow != null) {
                    var tmp = ErrorReason(lcolumn, lrow, mode);
                    return !string.IsNullOrEmpty(tmp)
                        ? LanguageTool.DoTranslate("Die verlinkte Zelle kann nicht bearbeitet werden: ") + tmp
                        : string.Empty;
                }
                if (lcolumn == null) { return LanguageTool.DoTranslate("Die Spalte ist in der Quell-Datenbank nicht vorhanden."); }
                // if (LinkedData.Item2 == null) { return LanguageTool.DoTranslate("Die Zeile ist in der Quell-Datenbank nicht vorhanden."); }
                return LanguageTool.DoTranslate("Die Zeile ist in der Quell-Datenbank nicht vorhanden.");
            }
            if (Row != null) {
                if (Row.Database != Column.Database) { return LanguageTool.DoTranslate("Interner Fehler: Bezug der Datenbank zur Zeile ist fehlerhaft."); }
                if (Column != Column.Database.Column.SysLocked && Row.CellGetBoolean(Column.Database.Column.SysLocked) && !Column.EditTrotzSperreErlaubt) { return LanguageTool.DoTranslate("Da die Zeile als abgeschlossen markiert ist, kann die Zelle nicht bearbeitet werden."); }
            } else {
                //Auf neue Zeile wird geprüft
                if (!Column.IsFirst()) { return LanguageTool.DoTranslate("Neue Zeilen müssen mit der ersten Spalte beginnen."); }
            }
            if (!Column.TextBearbeitungErlaubt && !Column.DropdownBearbeitungErlaubt) {
                return LanguageTool.DoTranslate("Die Inhalte dieser Spalte können nicht manuell bearbeitet werden, da keine Bearbeitungsmethode erlaubt ist.");
            }
            if (ColumnItem.UserEditDialogTypeInTable(Column.Format, false, true, Column.MultiLine) == enEditTypeTable.None) {
                return "Interner Programm-Fehler: Es ist keine Bearbeitungsmethode für den Typ des Spalteninhalts '" + Column.Format + "' definiert.";
            }
            //foreach (var ThisRule in Column.Database.Rules) {
            //    if (ThisRule != null) {
            //        if (ThisRule.WillAlwaysCellOverride(Column)) { return LanguageTool.DoTranslate("Diese Zelle wird von automatischen Regeln befüllt."); }
            //        if (ThisRule.BlockEditing(Column, Row)) { return LanguageTool.DoTranslate("Eine Regel sperrt diese Zelle."); }
            //    }
            //}
            return !Column.Database.PermissionCheck(Column.PermissionGroups_ChangeCell, Row)
                ? LanguageTool.DoTranslate("Sie haben nicht die nötigen Rechte, um diesen Wert zu ändern.")
                : string.Empty;
        }

        public static string KeyOfCell(long ColKey, long RowKey) => ColKey + "|" + RowKey;

        public static string KeyOfCell(ColumnItem Column, RowItem Row) {
            // Alte verweise eleminieren.
            if (Column != null) { Column = Column.Database.Column.SearchByKey(Column.Key); }
            if (Row != null) { Row = Row.Database.Row.SearchByKey(Row.Key); }
            return Column == null && Row == null
                ? string.Empty
                : Column == null ? KeyOfCell(-1, Row.Key) : Row == null ? KeyOfCell(Column.Key, -1) : KeyOfCell(Column.Key, Row.Key);
        }

        public static (ColumnItem column, RowItem row) LinkedCellData(string column, RowItem row, bool repairLinkedValue, bool addRowIfNotExists) {
            if (row == null) { return (null, null); }

            var col = row.Database.Column[column];
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
        public static (ColumnItem column, RowItem row) LinkedCellData(ColumnItem column, RowItem row, bool repairLinkedValue, bool addRowIfNotExists) {
            if (column.Format != enDataFormat.LinkedCell) { return (null, null); }

            var LinkedDatabase = column.LinkedDatabase();
            if (LinkedDatabase == null) { return (null, null); }

            var skriptgesteuert = column.LinkedCell_RowKey == -9999;

            if (repairLinkedValue && !skriptgesteuert) { return RepairLinkedCellValue(LinkedDatabase, column, row, addRowIfNotExists); }

            var Key = column.Database.Cell.GetStringBehindLinkedValue(column, row);
            if (string.IsNullOrEmpty(Key)) {
                return (LinkedDatabase.Column.SearchByKey(column.LinkedCell_ColumnKey), null);
            }

            var V = Key.SplitAndCutBy("|");
            if (V.Length != 2) { return skriptgesteuert ? (LinkedDatabase.Column.SearchByKey(column.LinkedCell_ColumnKey), null) : RepairLinkedCellValue(LinkedDatabase, column, row, addRowIfNotExists); }
            var LinkedColumn = LinkedDatabase.Column.SearchByKey(long.Parse(V[0]));
            var LinkedRow = LinkedDatabase.Row.SearchByKey(long.Parse(V[1]));

            if (KeyOfCell(LinkedColumn, LinkedRow) == Key) { return (LinkedColumn, LinkedRow); }

            return skriptgesteuert ? (LinkedDatabase.Column.SearchByKey(column.LinkedCell_ColumnKey), null) : RepairLinkedCellValue(LinkedDatabase, column, row, addRowIfNotExists);
        }

        /// <summary>
        ///  Gibt zurück, ob die Zelle bearbeitet werden kann.
        ///  Optional zusätzlich mit den Dateirechten.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <param name="DateiRechtePrüfen"></param>
        /// <returns></returns>
        public static bool UserEditPossible(ColumnItem column, RowItem row, enErrorReason mode) => string.IsNullOrEmpty(ErrorReason(column, row, mode));

        /// <summary>
        /// Gibt einen Datainamen/Pfad zurück, der sich aus dem Standard Angaben der Spalte und den Zelleninhalt zusammensetzt.
        /// Keine Garantie, dass die Datei auch existiert.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public string BestFile(string columnName, RowItem row) => BestFile(_database.Column[columnName], row);

        /// <summary>
        /// Gibt einen Datainamen/Pfad zurück, der sich aus dem Standard Angaben der Spalte und den Zelleninhalt zusammensetzt.
        /// Keine Garantie, dass die Datei auch existiert.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public string BestFile(ColumnItem column, RowItem row) => column.BestFile(GetString(column, row), false);

        public void DataOfCellKey(string CellKey, out ColumnItem Column, out RowItem Row) {
            if (string.IsNullOrEmpty(CellKey)) {
                Column = null;
                Row = null;
                return;
            }
            var cd = CellKey.SplitAndCutBy("|");
            if (cd.GetUpperBound(0) != 1) { Develop.DebugPrint(enFehlerArt.Fehler, "Falscher CellKey übergeben: " + CellKey); }
            Column = _database.Column.SearchByKey(long.Parse(cd[0]));
            Row = _database.Row.SearchByKey(long.Parse(cd[1]));
        }

        public void Delete(ColumnItem column, long rowKey) {
            var CellKey = KeyOfCell(column.Key, rowKey);
            if (!ContainsKey(CellKey)) { return; }
            //           var Inhalt = _cells[CellKey].Value;
            Remove(CellKey);
            //  DoSpecialFormats(Column, RowKey, Inhalt, false, false, true);
        }

        public void Dispose() {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="column"></param>
        /// <param name="RowKey"></param>
        /// <param name="previewsValue"></param>
        /// <param name="doAlways">Auch wenn der PreviewsValue gleich dem CurrentValue ist, wird die Routine durchberechnet</param>
        public void DoSpecialFormats(ColumnItem column, long RowKey, string previewsValue, bool doAlways) {
            if (column == null) { _database?.DevelopWarnung("Spalte ungültig!"); Develop.DebugPrint(enFehlerArt.Fehler, "Spalte ungültig!<br>" + _database.Filename); }
            var CurrentValue = GetString(column, _database.Row.SearchByKey(RowKey));
            if (!doAlways && CurrentValue == previewsValue) { return; }
            switch (column.Format) {
                case enDataFormat.RelationText:
                    RepairRelationText(column, _database.Row.SearchByKey(RowKey), previewsValue);
                    SetSameValueOfKey(column, RowKey, CurrentValue);
                    break;

                case enDataFormat.LinkedCell:
                    if (doAlways) {
                        LinkedCellData(column, _database.Row.SearchByKey(RowKey), true, false); // Repariert auch Cellbezüge
                    }
                    break;
            }
            if (!string.IsNullOrEmpty(column.I_Am_A_Key_For_Other_Column)) {
                SetSameValueOfKey(column, RowKey, CurrentValue);
            }
            if (column.IsFirst()) {
                foreach (var ThisColumnItem in _database.Column) {
                    if (ThisColumnItem != null) {
                        switch (ThisColumnItem.Format) {
                            //case enDataFormat.Relation:
                            //    RelationNameChanged(ThisColumnItem, PreviewsValue, CurrentValue);
                            //    break;
                            case enDataFormat.RelationText:
                                RelationTextNameChanged(ThisColumnItem, RowKey, previewsValue, CurrentValue);
                                break;
                        }
                    }
                }
            }
            if (column.KeyColumnKey > -1) {
                ChangeValueOfKey(CurrentValue, column, RowKey);
            }
        }

        public bool GetBoolean(string columnName, RowItem row) => GetBoolean(_database.Column[columnName], row);

        public bool GetBoolean(ColumnItem column, RowItem row) => GetString(column, row).FromPlusMinus();// Main Method

        public Color GetColor(string columnName, RowItem row) => GetColor(_database.Column[columnName], row);

        public Color GetColor(ColumnItem column, RowItem row) => Color.FromArgb(GetInteger(column, row)); // Main Method

        public int GetColorBGR(ColumnItem Column, RowItem Row) {
            var c = GetColor(Column, Row);
            int colorBlue = c.B;
            int colorGreen = c.G;
            int colorRed = c.R;
            return (colorBlue << 16) | (colorGreen << 8) | colorRed;
        }

        public DateTime GetDateTime(string columnName, RowItem row) => GetDateTime(_database.Column[columnName], row);

        public DateTime GetDateTime(ColumnItem column, RowItem row) // Main Method
        {
            var _String = GetString(column, row);
            return string.IsNullOrEmpty(_String) ? default : DateTimeTryParse(_String, out var d) ? d : default;
        }

        public double GetDouble(string columnName, RowItem row) => GetDouble(_database.Column[columnName], row);

        public double GetDouble(ColumnItem column, RowItem row) // Main Method
        {
            var x = GetString(column, row);
            return string.IsNullOrEmpty(x) ? 0 : double.Parse(x);
        }

        public int GetInteger(string columnName, RowItem row) => GetInteger(_database.Column[columnName], row);

        public int GetInteger(ColumnItem column, RowItem row) // Main Method
        {
            var x = GetString(column, row);
            return string.IsNullOrEmpty(x) ? 0 : int.Parse(x);
        }

        public List<string> GetList(string columnName, RowItem row) => GetList(_database.Column[columnName], row);

        public List<string> GetList(ColumnItem column, RowItem row) => GetString(column, row).SplitAndCutByCRToList();// Main Method

        public Point GetPoint(string columnName, RowItem row) => GetPoint(_database.Column[columnName], row);

        public Point GetPoint(ColumnItem column, RowItem row) // Main Method
        {
            var _String = GetString(column, row);
            return string.IsNullOrEmpty(_String) ? Point.Empty : Extensions.PointParse(_String);
        }

        public Size GetSizeOfCellContent(ColumnItem Column, RowItem Row) {
            var CellKey = KeyOfCell(Column, Row);
            return ContainsKey(CellKey) ? Column.Database.Cell[CellKey].Size : Size.Empty;
        }

        public string GetString(string columnName, RowItem row) => GetString(_database.Column[columnName], row);

        public string GetString(ColumnItem column, RowItem row) // Main Method
        {
            try {
                if (column == null) { _database?.DevelopWarnung("Spalte ungültig!"); Develop.DebugPrint(enFehlerArt.Fehler, "Spalte ungültig!<br>" + _database.Filename); }
                if (row == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Zeile ungültig!<br>" + _database.Filename); }
                if (column.Format == enDataFormat.LinkedCell) {
                    (var lcolumn, var lrow) = LinkedCellData(column, row, false, false);
                    return lcolumn != null && lrow != null ? lrow.CellGetString(lcolumn) : string.Empty;
                }
                var CellKey = KeyOfCell(column, row);
                return !ContainsKey(CellKey) ? string.Empty : this[CellKey].Value is string s ? s : string.Empty;
            } catch {
                // Manchmal verscwhindwet der vorhandene Key?!?
                return GetString(column, row);
            }
        }

        public string GetStringBehindLinkedValue(ColumnItem Column, RowItem Row) {
            if (Column == null || Row == null) { return string.Empty; }
            var CellKey = KeyOfCell(Column, Row);
            return !ContainsKey(CellKey) ? string.Empty : this[CellKey].Value;
        }

        public void Initialize() => Clear();

        public bool IsNullOrEmpty(ColumnItem Column, RowItem Row) {
            if (Column == null) { return true; }
            if (Row == null) { return true; }
            if (Column.Format == enDataFormat.LinkedCell) {
                (var lcolumn, var lrow) = LinkedCellData(Column, Row, false, false);
                return lcolumn == null || lrow == null || lrow.CellIsNullOrEmpty(lcolumn);
            }
            var CellKey = KeyOfCell(Column.Key, Row.Key);
            return !ContainsKey(CellKey) || string.IsNullOrEmpty(this[CellKey].Value);
        }

        public bool IsNullOrEmpty(string ColumnName, RowItem Row) => IsNullOrEmpty(_database.Column[ColumnName], Row);

        public bool IsNullOrEmpty(string CellKey) {
            DataOfCellKey(CellKey, out var Column, out var Row);
            return IsNullOrEmpty(Column, Row);
        }

        public bool MatchesTo(ColumnItem column, RowItem row, FilterItem filter) {
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
                var Typ = filter.FilterType;
                // Oder-Flag ermitteln --------------------------------------------
                var Oder = Typ.HasFlag(enFilterType.ODER);
                if (Oder) { Typ ^= enFilterType.ODER; }
                // Und-Flag Ermitteln --------------------------------------------
                var Und = Typ.HasFlag(enFilterType.UND);
                if (Und) { Typ ^= enFilterType.UND; }
                if (filter.SearchValue.Count < 2) {
                    Oder = true;
                    Und = false; // Wenn nur EIN Eintrag gecheckt wird, ist es EGAL, ob UND oder ODER.
                }
                //if (Und && Oder) { Develop.DebugPrint(enFehlerArt.Fehler, "Filter-Anweisung erwartet ein 'Und' oder 'Oder': " + ToString()); }
                // Tatsächlichen String ermitteln --------------------------------------------
                var _String = string.Empty;
                var fColumn = column;
                if (column.Format == enDataFormat.LinkedCell) {
                    var LinkedData = LinkedCellData(column, row, false, false);
                    if (LinkedData.column != null && LinkedData.row != null) {
                        _String = LinkedData.row.CellGetString(LinkedData.column);
                        fColumn = LinkedData.column;
                    }
                } else {
                    var CellKey = KeyOfCell(column, row);
                    _String = ContainsKey(CellKey) ? this[CellKey].Value : string.Empty;
                }
                if (_String is null) { _String = string.Empty; }
                if (Typ.HasFlag(enFilterType.Instr)) { _String = LanguageTool.ColumnReplace(_String, fColumn, enShortenStyle.Both); }
                // Multiline-Typ ermitteln  --------------------------------------------
                var TMPMultiLine = false;
                if (column != null) { TMPMultiLine = column.MultiLine; }
                if (Typ.HasFlag(enFilterType.MultiRowIgnorieren)) {
                    TMPMultiLine = false;
                    Typ ^= enFilterType.MultiRowIgnorieren;
                }
                if (TMPMultiLine && !_String.Contains("\r")) { TMPMultiLine = false; } // Zeilen mit nur einem Eintrag können ohne Multiline behandel werden.
                //if (Typ == enFilterType.KeinFilter)
                //{
                //    Develop.DebugPrint(enFehlerArt.Fehler, "'Kein Filter' wurde übergeben: " + ToString());
                //}
                if (!TMPMultiLine) {
                    var BedingungErfüllt = false;
                    for (var FiltNachNr = 0; FiltNachNr < filter.SearchValue.Count; FiltNachNr++) {
                        BedingungErfüllt = CompareValues(_String, filter.SearchValue[FiltNachNr], Typ);
                        if (Oder && BedingungErfüllt) { return true; }
                        if (Und && BedingungErfüllt == false) { return false; } // Bei diesem UND hier müssen allezutreffen, deshalb kann getrost bei einem False dieses zurückgegeben werden.
                    }
                    return BedingungErfüllt;
                }
                List<string> VorhandenWerte = new(_String.SplitAndCutByCR());
                if (VorhandenWerte.Count == 0) // Um den Filter, der nach 'Leere' Sucht, zu befriediegen
                {
                    VorhandenWerte.Add("");
                }
                // Diese Reihenfolge der For Next ist unglaublich wichtig:
                // Sind wenigere VORHANDEN vorhanden als FilterWerte, dann durchsucht diese Routine zu wenig Einträge,
                // bevor sie bei einem UND ein False zurückgibt
                for (var FiltNachNr = 0; FiltNachNr < filter.SearchValue.Count; FiltNachNr++) {
                    var BedingungErfüllt = false;
                    foreach (var t in VorhandenWerte) {
                        BedingungErfüllt = CompareValues(t, filter.SearchValue[FiltNachNr], Typ);
                        if (Oder && BedingungErfüllt) { return true; }// Irgendein vorhandener Value trifft zu!!! Super!!!
                        if (Und && BedingungErfüllt) { break; }// Irgend ein vorhandener Value trifft zu, restliche Prüfung uninteresant
                    }
                    if (Und && !BedingungErfüllt) // Einzelne UND konnte nicht erfüllt werden...
                    {
                        return false;
                    }
                }
                if (Und) { return true; } // alle "Und" stimmen!
                return false; // Gar kein "Oder" trifft zu...
            } catch (Exception ex) {
                Develop.DebugPrint(ex);
                Pause(1, true);
                return MatchesTo(column, row, filter);
            }
        }

        public void Set(string columnName, RowItem row, string value) => Set(_database.Column[columnName], row, value);

        public void Set(ColumnItem column, RowItem row, string value) // Main Method
        {
            _database.BlockReload(false);
            if (column == null) { _database?.DevelopWarnung("Spalte ungültig!"); Develop.DebugPrint(enFehlerArt.Fehler, "Spalte ungültig!<br>" + _database.Filename); }
            if (row == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Zeile ungültig!!<br>" + _database.Filename); }
            if (column.Format == enDataFormat.LinkedCell) {
                (var lcolumn, var lrow) = LinkedCellData(column, row, true, !string.IsNullOrEmpty(value));
                lrow?.CellSet(lcolumn, value);
                return;
            }
            SetValueBehindLinkedValue(column, row, value);
        }

        public void Set(string columnName, RowItem row, bool value) => Set(_database.Column[columnName], row, value.ToPlusMinus());

        public void Set(ColumnItem column, RowItem row, bool value) => Set(column, row, value.ToPlusMinus());

        public void Set(string columnName, RowItem row, DateTime value) => Set(_database.Column[columnName], row, value.ToString(Constants.Format_Date5));

        public void Set(ColumnItem column, RowItem row, DateTime value) => Set(column, row, value.ToString(Constants.Format_Date5));

        public void Set(string columnName, RowItem row, List<string> value) => Set(_database.Column[columnName], row, value);

        public void Set(ColumnItem column, RowItem row, List<string> value) => Set(column, row, value.JoinWithCr());

        public void Set(string columnName, RowItem row, Point value) => Set(_database.Column[columnName], row, value);

        public void Set(ColumnItem column, RowItem row, Point value) => Set(column, row, value.ToString());      // Main Method// {X=253,Y=194} MUSS ES SEIN, prüfen

        public void Set(string columnName, RowItem row, int value) => Set(_database.Column[columnName], row, value.ToString());

        public void Set(ColumnItem column, RowItem row, int value) => Set(column, row, value.ToString());

        public void Set(string columnName, RowItem row, double value) => Set(_database.Column[columnName], row, value.ToString());

        public void Set(ColumnItem column, RowItem row, double value) => Set(column, row, value.ToString());

        /// <summary>
        ///
        /// </summary>
        /// <param name="Column"></param>
        /// <param name="Row"></param>
        /// <param name="ContentSize">Wird im Scale der Datenbank gespeichert, da es ja Benutzerübergreifend ist</param>
        public void SetSizeOfCellContent(ColumnItem Column, RowItem Row, Size ContentSize) {
            var CellKey = KeyOfCell(Column, Row);
            if (!ContainsKey(CellKey)) { return; }
            this[CellKey].Size = ContentSize;
        }

        public List<string> ValuesReadable(ColumnItem Column, RowItem Row, enShortenStyle style) => CellItem.ValuesReadable(Column, Row, style);

        internal static List<RowItem> ConnectedRowsOfRelations(string CompleteRelationText, RowItem Row) {
            List<RowItem> AllRows = new();
            var Names = Row.Database.Column[0].GetUcaseNamesSortedByLenght();
            var RelationTextLine = CompleteRelationText.ToUpper().SplitAndCutByCR();
            foreach (var thisTextLine in RelationTextLine) {
                var tmp = thisTextLine;
                List<RowItem> R = new();
                for (var Z = Names.Count - 1; Z > -1; Z--) {
                    if (tmp.IndexOfWord(Names[Z], 0, RegexOptions.IgnoreCase) > -1) {
                        R.Add(Row.Database.Row[Names[Z]]);
                        tmp = tmp.Replace(Names[Z], string.Empty);
                    }
                }
                if (R.Count == 1 || R.Contains(Row)) { AllRows.AddIfNotExists(R); } // Bei mehr als einer verknüpften Reihe MUSS die die eigene Reihe dabei sein.
            }
            return AllRows;
        }

        internal static void Invalidate_CellContentSize(ColumnItem Column, RowItem Row) {
            var CellKey = KeyOfCell(Column, Row);
            if (Column.Database.Cell.ContainsKey(CellKey)) {
                Column.Database.Cell[CellKey].InvalidateSize();
            }
        }

        internal string CompareKey(ColumnItem Column, RowItem Row) => GetString(Column, Row).CompareKey(Column.SortType);

        internal Size ContentSizeToSave(KeyValuePair<string, CellItem> vCell, ColumnItem Column) {
            if (Column.Format.SaveSizeData()) {
                if (vCell.Value.Size.Height > 4 &&
                    vCell.Value.Size.Height < 65025 &&
                    vCell.Value.Size.Width > 4 &&
                    vCell.Value.Size.Width < 65025) { return vCell.Value.Size; }
            }
            return Size.Empty;
        }

        internal void InvalidateAllSizes() {
            foreach (var ThisColumn in _database.Column) {
                ThisColumn.Invalidate_ColumAndContent();
            }
        }

        internal void Load_310(ColumnItem column, RowItem row, string value, int width, int height) {
            if (row == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Row konnte nicht generiert werden."); }
            if (column == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Column konnte nicht generiert werden."); }
            var CellKey = KeyOfCell(column, row);
            if (ContainsKey(CellKey)) {
                var c = this[CellKey];
                c.Value = value; // Auf jeden Fall setzen. Auch falls es nachher entfernt wird, so ist es sicher leer
                c.Size = width > 0 ? new Size(width, height) : Size.Empty;
                if (string.IsNullOrEmpty(value)) { Remove(CellKey); }
            } else {
                Add(CellKey, new CellItem(value, width, height));
            }
        }

        internal void OnCellValueChanged(CellEventArgs e) {
            e.Column._UcaseNamesSortedByLenght = null;
            CellValueChanged?.Invoke(this, e);
        }

        internal bool RemoveOrphans() {
            try {
                List<string> RemoveKeys = new();
                foreach (var pair in this) {
                    if (!string.IsNullOrEmpty(pair.Value.Value)) {
                        DataOfCellKey(pair.Key, out var Column, out var Row);
                        if (Column == null || Row == null) { RemoveKeys.Add(pair.Key); }
                    }
                }
                if (RemoveKeys.Count == 0) { return false; }
                foreach (var ThisKey in RemoveKeys) {
                    Remove(ThisKey);
                }
                return true;
            } catch {
                Develop.CheckStackForOverflow(); // Um Rauszufinden, ob endlos-Schleifen öfters  vorkommen. Zuletzt 24.11.2020
                return RemoveOrphans();
            }
        }

        internal void SaveToByteList(ref List<byte> l) {
            RemoveOrphans();
            foreach (var ThisString in this) {
                _database.SaveToByteList(l, ThisString);
            }
        }

        /// <summary>
        /// Diese Routine setzt dern Wert direkt in die Zelle. Verlinkete Zellen werden nicht weiter verfolgt.
        /// Deswegenkan nur mit dieser Routine der Cell-Link eingefügt werden.
        /// Die Routine blockt zwar den Reload, aber eine endgültige prüfung verhindet ein Ändern, wenn sich der Wert nicht verändert hat
        /// </summary>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <param name="value"></param>
        internal void SetValueBehindLinkedValue(ColumnItem column, RowItem row, string value) {
            _database.BlockReload(false);
            if (column == null || _database.Column.SearchByKey(column.Key) == null) {
                _database?.DevelopWarnung("Spalte ungültig!");
                Develop.DebugPrint(enFehlerArt.Fehler, "Spalte ungültig!<br>" + _database.Filename);
            }
            if (row == null || _database.Row.SearchByKey(row.Key) == null) {
                _database?.DevelopWarnung("Zeile ungültig!!");
                Develop.DebugPrint(enFehlerArt.Fehler, "Zeile ungültig!!<br>" + _database.Filename);
            }

            if (column.Format != enDataFormat.LinkedCell) { value = column.AutoCorrect(value); }

            var CellKey = KeyOfCell(column.Key, row.Key);
            var OldValue = string.Empty;
            if (ContainsKey(CellKey)) { OldValue = this[CellKey].Value; }
            if (value == OldValue) { return; }

            _database.WaitEditable();
            _database.AddPending(enDatabaseDataType.ce_Value_withoutSizeData, column.Key, row.Key, OldValue, value, true);
            column._UcaseNamesSortedByLenght = null;
            DoSpecialFormats(column, row.Key, OldValue, false);
            SystemSet(_database.Column.SysRowChanger, row, _database.UserName);
            SystemSet(_database.Column.SysRowChangeDate, row, DateTime.Now.ToString(Constants.Format_Date5));
            Invalidate_CellContentSize(column, row);
            column.Invalidate_TmpColumnContentWidth();
            OnCellValueChanged(new CellEventArgs(column, row));
        }

        internal void SystemSet(ColumnItem Column, RowItem Row, string Value) {
            if (Column == null) { _database?.DevelopWarnung("Spalte ungültig!"); Develop.DebugPrint(enFehlerArt.Fehler, "Spalte ungültig!<br>" + _database.Filename); }
            if (Row == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Zeile ungültig!<br>" + _database.Filename); }
            if (string.IsNullOrEmpty(Column.Identifier)) { Develop.DebugPrint(enFehlerArt.Fehler, "SystemSet nur bei System-Spalten möglich: " + ToString()); }
            if (!Column.SaveContent) { return; }
            var CellKey = KeyOfCell(Column, Row);
            var _String = string.Empty;
            if (ContainsKey(CellKey)) {
                _String = this[CellKey].Value;
            } else {
                Add(CellKey, new CellItem(string.Empty, 0, 0));
            }
            if (Value == _String) { return; }
            _database.AddPending(enDatabaseDataType.ce_Value_withoutSizeData, Column.Key, Row.Key, _String, Value, true);
        }

        private static (ColumnItem column, RowItem row) RepairLinkedCellValue(Database linkedDatabase, ColumnItem column, RowItem row, bool addRowIfNotExists) {
            ColumnItem targetColumn;
            RowItem targetRow = null;

            /// Spalte aus der Ziel-Datenbank ermitteln
            targetColumn = linkedDatabase.Column.SearchByKey(column.LinkedCell_ColumnKey);
            if (targetColumn == null) { return Ergebnis("Die Spalte ist in der Zieldatenbank nicht vorhanden."); }

            /// Zeile aus der Ziel-Datenbank ermitteln
            if (row == null) { return Ergebnis("Keine Zeile zum finden des Zeilenschlüssels angegeben."); }
            var LinkedCell_RowColumn = column.Database.Column.SearchByKey(column.LinkedCell_RowKey);
            if (LinkedCell_RowColumn == null) { return Ergebnis("Die Spalte, aus der der Zeilenschlüssel kommen soll, existiert nicht."); }
            if (row.CellIsNullOrEmpty(LinkedCell_RowColumn)) { return Ergebnis("Kein Zeilenschlüssel angegeben."); }

            targetRow = linkedDatabase.Row[row.CellGetString(LinkedCell_RowColumn)];
            if (targetRow == null && addRowIfNotExists) {
                targetRow = linkedDatabase.Row.Add(row.CellGetString(LinkedCell_RowColumn));
            }
            return targetRow == null ? Ergebnis("Die Zeile ist in der Zieldatenbank nicht vorhanden.") : Ergebnis(string.Empty);

            #region Subroutine Ergebnis

            (ColumnItem column, RowItem row) Ergebnis(string fehler) {
                column.Database.BlockReload(false);
                if (string.IsNullOrEmpty(fehler)) {
                    column.Database.Cell.SetValueBehindLinkedValue(column, row, KeyOfCell(targetColumn.Key, targetRow.Key));
                    return (targetColumn, targetRow);
                } else {
                    if (column != null && row != null) { column.Database.Cell.SetValueBehindLinkedValue(column, row, string.Empty); }
                    return (targetColumn, targetRow);
                }
            }

            #endregion
        }

        private void _database_Disposing(object sender, System.EventArgs e) => Dispose();

        private string ChangeTextFromRowId(string CompleteRelationText) {
            foreach (var RowItem in _database.Row) {
                if (RowItem != null) {
                    CompleteRelationText = CompleteRelationText.Replace("/@X" + RowItem.Key.ToString() + "X@/", RowItem.CellFirstString());
                }
            }
            return CompleteRelationText;
        }

        private string ChangeTextToRowId(string CompleteRelationText, string OldValue, string NewValue, long KeyOfCHangedRow) {
            var Names = _database.Column[0].GetUcaseNamesSortedByLenght();
            var DidOld = false;
            var DidNew = false;
            for (var Z = Names.Count - 1; Z > -1; Z--) {
                if (!DidOld && Names[Z].Length <= OldValue.Length) {
                    DidOld = true;
                    DoReplace(OldValue, KeyOfCHangedRow);
                }
                if (!DidNew && Names[Z].Length <= NewValue.Length) {
                    DidNew = true;
                    DoReplace(NewValue, KeyOfCHangedRow);
                }
                if (CompleteRelationText.ToUpper().Contains(Names[Z])) {
                    DoReplace(Names[Z], _database.Row[Names[Z]].Key);
                }
            }
            if (string.IsNullOrEmpty(NewValue)) { return CompleteRelationText; }
            // Nochmal am Schluss, wenn die Wörter alle lang sind, und die nicht mehr zum ZUg kommen.
            if (OldValue.Length > NewValue.Length) {
                DoReplace(OldValue, KeyOfCHangedRow);
                DoReplace(NewValue, KeyOfCHangedRow);
            } else {
                DoReplace(NewValue, KeyOfCHangedRow);
                DoReplace(OldValue, KeyOfCHangedRow);
            }
            return CompleteRelationText;
            void DoReplace(string Name, long Key) {
                if (!string.IsNullOrEmpty(Name)) {
                    CompleteRelationText = CompleteRelationText.Replace(Name, "/@X" + Key.ToString() + "X@/", RegexOptions.IgnoreCase);
                }
            }
        }

        /// <summary>
        /// Ändert bei allen Zeilen - die den gleichen Key (KeyColumn, Relation) benutzen wie diese Zeile - den Inhalt der Zellen ab um diese gleich zu halten.
        /// </summary>
        /// <param name="currentvalue"></param>
        /// <param name="column"></param>
        /// <param name="rowKey"></param>
        private void ChangeValueOfKey(string currentvalue, ColumnItem column, long rowKey) {
            var keyc = _database.Column.SearchByKey(column.KeyColumnKey); // Schlüsselspalte für diese Spalte bestimmen
            if (keyc is null) { return; }
            List<RowItem> Rows;
            var ownRow = _database.Row.SearchByKey(rowKey);
            Rows = keyc.Format == enDataFormat.RelationText
                ? ConnectedRowsOfRelations(ownRow.CellGetString(keyc), ownRow)
                : RowCollection.MatchesTo(new FilterItem(keyc, enFilterType.Istgleich_GroßKleinEgal, ownRow.CellGetString(keyc)));
            Rows.Remove(ownRow);
            if (Rows.Count < 1) { return; }
            foreach (var thisRow in Rows) {
                thisRow.CellSet(column, currentvalue);
            }
        }

        private bool CompareValues(string IstValue, string FilterValue, enFilterType Typ) {
            // if (Column.Format == enDataFormat.LinkedCell) { Develop.DebugPrint(enFehlerArt.Fehler, "Falscher Fremdzellenzugriff"); }
            if (Typ.HasFlag(enFilterType.GroßKleinEgal)) {
                IstValue = IstValue.ToUpper();
                FilterValue = FilterValue.ToUpper();
                Typ ^= enFilterType.GroßKleinEgal;
            }
            switch (Typ) {
                case enFilterType.Istgleich:
                    return Convert.ToBoolean(IstValue == FilterValue);

                case (enFilterType)2: // Ungleich
                    return Convert.ToBoolean(IstValue != FilterValue);

                case enFilterType.Instr:
                    return IstValue.Contains(FilterValue);

                case enFilterType.Between:
                    if (!IstValue.IsNumeral()) { return false; }
                    var ival = DoubleParse(IstValue);
                    var fval = FilterValue.SplitAndCutBy("|");
                    if (ival < DoubleParse(fval[0])) { return false; }
                    if (ival > DoubleParse(fval[1])) { return false; }
                    //if (double.Parse)
                    //if (string.IsNullOrEmpty(IstValue)) { return false; }
                    //if (!FilterValue.ToUpper().Contains("VALUE")) { return false; }
                    //var d = modErgebnis.Ergebnis(FilterValue.Replace("VALUE", IstValue.Replace(",", "."), RegexOptions.IgnoreCase));
                    //if (d == null) { return false; }
                    //return Convert.ToBoolean(d == -1);
                    return true;

                case enFilterType.KeinFilter:
                    Develop.DebugPrint("Kein Filter!");
                    return true;

                case enFilterType.BeginntMit:
                    return IstValue.StartsWith(FilterValue);

                default: {
                        Develop.DebugPrint(Typ);
                        return false;
                    }
            }
        }

        private void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
                }
                // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
                // TODO: Große Felder auf NULL setzen
                _database.Disposing -= _database_Disposing;
                _database = null;
                Clear();
                disposedValue = true;
            }
        }

        private void MakeNewRelations(ColumnItem Column, RowItem Row, List<string> OldBZ, List<string> NewBZ) {
            //Develop.CheckStackForOverflow();
            //// Dann die neuen Erstellen
            foreach (var t in NewBZ) {
                if (!OldBZ.Contains(t)) {
                    var X = ConnectedRowsOfRelations(t, Row);
                    foreach (var ThisRow in X) {
                        if (ThisRow != Row) {
                            var ex = ThisRow.CellGetList(Column);
                            if (X.Contains(Row)) {
                                ex.Add(t);
                            } else {
                                ex.Add(t.ReplaceWord(ThisRow.CellFirstString(), Row.CellFirstString(), RegexOptions.IgnoreCase));
                            }
                            ThisRow.CellSet(Column, ex.SortedDistinctList());
                        }
                    }
                }
            }
        }

        private void RelationTextNameChanged(ColumnItem ColumnToRepair, long RowKey, string OldValue, string NewValue) {
            if (string.IsNullOrEmpty(NewValue)) { return; }
            foreach (var ThisRowItem in _database.Row) {
                if (ThisRowItem != null) {
                    if (!ThisRowItem.CellIsNullOrEmpty(ColumnToRepair)) {
                        var t = ThisRowItem.CellGetString(ColumnToRepair);
                        if (!string.IsNullOrEmpty(OldValue) && t.ToUpper().Contains(OldValue.ToUpper())) {
                            t = ChangeTextToRowId(t, OldValue, NewValue, RowKey);
                            t = ChangeTextFromRowId(t);
                            var t2 = t.SplitAndCutByCRToList().SortedDistinctList();
                            ThisRowItem.CellSet(ColumnToRepair, t2);
                        }
                        if (t.ToUpper().Contains(NewValue.ToUpper())) {
                            MakeNewRelations(ColumnToRepair, ThisRowItem, new List<string>(), t.SplitAndCutByCRToList());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Ändert die anderen Zeilen dieser Spalte, so dass der verknüpfte Text bei dieser und den anderen Spalten gleich ist ab.
        /// </summary>
        /// <param name="Column"></param>
        /// <param name="Row"></param>
        /// <param name="PreviewsValue"></param>
        private void RepairRelationText(ColumnItem Column, RowItem Row, string PreviewsValue) {
            var CurrentString = GetString(Column, Row);
            CurrentString = ChangeTextToRowId(CurrentString, string.Empty, string.Empty, -1);
            CurrentString = ChangeTextFromRowId(CurrentString);
            if (CurrentString != GetString(Column, Row)) {
                Set(Column, Row, CurrentString);
                return;
            }
            var OldBZ = new List<string>(PreviewsValue.SplitAndCutByCR()).SortedDistinctList();
            var NewBZ = new List<string>(CurrentString.SplitAndCutByCR()).SortedDistinctList();
            // Zuerst Beziehungen LÖSCHEN
            foreach (var t in OldBZ) {
                if (!NewBZ.Contains(t)) {
                    var X = ConnectedRowsOfRelations(t, Row);
                    foreach (var ThisRow in X) {
                        if (ThisRow != null && ThisRow != Row) {
                            var ex = ThisRow.CellGetList(Column);
                            if (X.Contains(Row)) {
                                ex.Remove(t);
                            } else {
                                ex.Remove(t.ReplaceWord(ThisRow.CellFirstString(), Row.CellFirstString(), RegexOptions.IgnoreCase));
                            }
                            ThisRow.CellSet(Column, ex.SortedDistinctList());
                        }
                    }
                }
            }
            MakeNewRelations(Column, Row, OldBZ, NewBZ);
        }

        /// <summary>
        /// Ändert bei allen anderen Spalten den Inhalt der Zelle ab (um diese gleich zuhalten), wenn diese Spalte der Key für die anderen ist.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="rowKey"></param>
        /// <param name="currentvalue"></param>
        private void SetSameValueOfKey(ColumnItem column, long rowKey, string currentvalue) {
            List<RowItem> Rows = null;
            var ownRow = _database.Row.SearchByKey(rowKey);
            foreach (var ThisColumn in _database.Column) {
                if (ThisColumn.LinkedCell_RowKey == column.Key) {
                    LinkedCellData(ThisColumn, ownRow, true, false); // Repariert auch Zellbezüge
                }
                if (ThisColumn.KeyColumnKey == column.Key) {
                    if (Rows == null) {
                        Rows = column.Format == enDataFormat.RelationText
                            ? ConnectedRowsOfRelations(currentvalue, ownRow)
                            : RowCollection.MatchesTo(new FilterItem(column, enFilterType.Istgleich_GroßKleinEgal, currentvalue));
                        Rows.Remove(ownRow);
                    }
                    if (Rows.Count < 1) {
                        ownRow.CellSet(ThisColumn, string.Empty);
                    } else {
                        ownRow.CellSet(ThisColumn, Rows[0].CellGetString(ThisColumn));
                    }
                }
            }
        }

        #endregion
    }
}