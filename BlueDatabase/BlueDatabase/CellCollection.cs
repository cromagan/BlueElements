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
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using static BlueBasics.Extensions;
using static BlueBasics.modAllgemein;
using static BlueBasics.modConverter;

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

        // TODO: Finalizer nur �berschreiben, wenn "Dispose(bool disposing)" Code f�r die Freigabe nicht verwalteter Ressourcen enth�lt
        ~CellCollection() {
            // �ndern Sie diesen Code nicht. F�gen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
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
                new FilterItem(cc, enFilterType.Istgleich_Gro�KleinEgal, row.CellGetString(cc)),
                new FilterItem(column, enFilterType.Ungleich_MultiRowIgnorieren, string.Empty)
            };
            var rows = column.Database.Row.CalculateSortedRows(F, null, null);
            rows.Remove(row);
            return rows.Count == 0 ? string.Empty : rows[0].CellGetString(column);
        }

        /// <summary>
        /// Gibt einen Fehlergrund zur�ck, ob die Zelle bearbeitet werden kann.
        /// Optional zus�tzlich mit den Dateirechten.
        /// </summary>
        /// <param name="Row"></param>
        /// <param name="DateiRechtePr�fen"></param>
        /// <param name="Column"></param>
        /// <returns></returns>
        public static string ErrorReason(ColumnItem Column, RowItem Row, enErrorReason mode) {
            if (mode == enErrorReason.OnlyRead) { return string.Empty; }
            if (Column == null) { return LanguageTool.DoTranslate("Es ist keine Spalte ausgew�hlt."); }
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
                //Auf neue Zeile wird gepr�ft
                if (!Column.IsFirst()) { return LanguageTool.DoTranslate("Neue Zeilen m�ssen mit der ersten Spalte beginnen."); }
            }
            if (!Column.TextBearbeitungErlaubt && !Column.DropdownBearbeitungErlaubt) {
                return LanguageTool.DoTranslate("Die Inhalte dieser Spalte k�nnen nicht manuell bearbeitet werden, da keine Bearbeitungsmethode erlaubt ist.");
            }
            if (ColumnItem.UserEditDialogTypeInTable(Column.Format, false, true, Column.MultiLine) == enEditTypeTable.None) {
                return "Interner Programm-Fehler: Es ist keine Bearbeitungsmethode f�r den Typ des Spalteninhalts '" + Column.Format + "' definiert.";
            }
            //foreach (var ThisRule in Column.Database.Rules) {
            //    if (ThisRule != null) {
            //        if (ThisRule.WillAlwaysCellOverride(Column)) { return LanguageTool.DoTranslate("Diese Zelle wird von automatischen Regeln bef�llt."); }
            //        if (ThisRule.BlockEditing(Column, Row)) { return LanguageTool.DoTranslate("Eine Regel sperrt diese Zelle."); }
            //    }
            //}
            return !Column.Database.PermissionCheck(Column.PermissionGroups_ChangeCell, Row)
                ? LanguageTool.DoTranslate("Sie haben nicht die n�tigen Rechte, um diesen Wert zu �ndern.")
                : string.Empty;
        }

        public static string KeyOfCell(int ColKey, int RowKey) => ColKey + "|" + RowKey;

        public static string KeyOfCell(ColumnItem Column, RowItem Row) {
            // Alte verweise eleminieren.
            if (Column != null) { Column = Column.Database.Column.SearchByKey(Column.Key); }
            if (Row != null) { Row = Row.Database.Row.SearchByKey(Row.Key); }
            return Column == null && Row == null
                ? string.Empty
                : Column == null ? KeyOfCell(-1, Row.Key) : Row == null ? KeyOfCell(Column.Key, -1) : KeyOfCell(Column.Key, Row.Key);
        }

        public static (ColumnItem column, RowItem row) LinkedCellData(ColumnItem column, RowItem row, bool RepairEmpties, bool AddRowIfNotExists) {
            if (column == null || row == null) { return (null, null); }
            if (column.Format != enDataFormat.LinkedCell) { return (null, null); }
            var LinkedDatabase = column.LinkedDatabase();
            if (LinkedDatabase == null) { return (null, null); }
            if (RepairEmpties) {
                return RepairLinkedCellValue(LinkedDatabase, column, row, AddRowIfNotExists);
            }
            var Key = column.Database.Cell.GetStringBehindLinkedValue(column, row);
            if (string.IsNullOrEmpty(Key)) { return (null, null); }
            var V = Key.SplitBy("|");
            if (V.Length != 2) { return RepairLinkedCellValue(LinkedDatabase, column, row, AddRowIfNotExists); }
            var LinkedColumn = LinkedDatabase.Column.SearchByKey(int.Parse(V[0]));
            var LinkedRow = LinkedDatabase.Row.SearchByKey(int.Parse(V[1]));
            return KeyOfCell(LinkedColumn, LinkedRow) == Key
                ? (LinkedColumn, LinkedRow)
                : RepairLinkedCellValue(LinkedDatabase, column, row, AddRowIfNotExists);
        }

        /// <summary>
        ///  Gibt zur�ck, ob die Zelle bearbeitet werden kann.
        ///  Optional zus�tzlich mit den Dateirechten.
        /// </summary>
        /// <param name="Column"></param>
        /// <param name="Row"></param>
        /// <param name="DateiRechtePr�fen"></param>
        /// <returns></returns>
        public static bool UserEditPossible(ColumnItem Column, RowItem Row, enErrorReason mode) => string.IsNullOrEmpty(ErrorReason(Column, Row, mode));

        /// <summary>
        /// Gibt einen Datainamen/Pfad zur�ck, der sich aus dem Standard Angaben der Spalte und den Zelleninhalt zusammensetzt.
        /// Keine Garantie, dass die Datei auch existiert.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public string BestFile(string columnName, RowItem row) => BestFile(_database.Column[columnName], row);

        /// <summary>
        /// Gibt einen Datainamen/Pfad zur�ck, der sich aus dem Standard Angaben der Spalte und den Zelleninhalt zusammensetzt.
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
            var cd = CellKey.SplitBy("|");
            if (cd.GetUpperBound(0) != 1) { Develop.DebugPrint(enFehlerArt.Fehler, "Falscher CellKey �bergeben: " + CellKey); }
            Column = _database.Column.SearchByKey(int.Parse(cd[0]));
            Row = _database.Row.SearchByKey(int.Parse(cd[1]));
        }

        public void Delete(ColumnItem Column, int RowKey) {
            var CellKey = KeyOfCell(Column.Key, RowKey);
            if (!ContainsKey(CellKey)) { return; }
            //           var Inhalt = _cells[CellKey].Value;
            Remove(CellKey);
            //  DoSpecialFormats(Column, RowKey, Inhalt, false, false, true);
        }

        public void Dispose() {
            // �ndern Sie diesen Code nicht. F�gen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="Column"></param>
        /// <param name="RowKey"></param>
        /// <param name="PreviewsValue"></param>
        /// <param name="DoAlways">Auch wenn der PreviewsValue gleich dem CurrentValue ist, wird die Routine durchberechnet</param>
        public void DoSpecialFormats(ColumnItem Column, int RowKey, string PreviewsValue, bool DoAlways) {
            if (Column == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Spalte ung�ltig!<br>" + _database.Filename); }
            var CurrentValue = GetString(Column, _database.Row.SearchByKey(RowKey));
            if (!DoAlways && CurrentValue == PreviewsValue) { return; }
            switch (Column.Format) {
                case enDataFormat.RelationText:
                    RepairRelationText(Column, _database.Row.SearchByKey(RowKey), PreviewsValue);
                    SetSameValueOfKey(Column, RowKey, CurrentValue);
                    break;

                case enDataFormat.LinkedCell:
                    if (DoAlways) {
                        LinkedCellData(Column, _database.Row.SearchByKey(RowKey), true, false); // Repariert auch Cellbez�ge
                    }
                    break;
            }
            if (!string.IsNullOrEmpty(Column.I_Am_A_Key_For_Other_Column)) {
                SetSameValueOfKey(Column, RowKey, CurrentValue);
            }
            if (Column.IsFirst()) {
                foreach (var ThisColumnItem in _database.Column) {
                    if (ThisColumnItem != null) {
                        switch (ThisColumnItem.Format) {
                            //case enDataFormat.Relation:
                            //    RelationNameChanged(ThisColumnItem, PreviewsValue, CurrentValue);
                            //    break;
                            case enDataFormat.RelationText:
                                RelationTextNameChanged(ThisColumnItem, RowKey, PreviewsValue, CurrentValue);
                                break;
                        }
                    }
                }
            }
            if (Column.KeyColumnKey > -1) {
                ChangeValueOfKey(CurrentValue, Column, RowKey);
            }
        }

        public bool GetBoolean(string columnName, RowItem row) => GetBoolean(_database.Column[columnName], row);

        public bool GetBoolean(ColumnItem column, RowItem row) // Main Method
=> GetString(column, row).FromPlusMinus();

        public Color GetColor(string columnName, RowItem row) => GetColor(_database.Column[columnName], row);

        public Color GetColor(ColumnItem column, RowItem row) // Main Method
=> Color.FromArgb(GetInteger(column, row));

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

        public List<string> GetList(ColumnItem column, RowItem row) // Main Method
=> GetString(column, row).SplitByCRToList();

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
                if (column == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Spalte ung�ltig!<br>" + _database.Filename); }
                if (row == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Zeile ung�ltig!<br>" + _database.Filename); }
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
            //Ein Filter kann mehrere Werte haben, diese m�ssen ein Attribut UND oder ODER haben.
            //Bei UND m�ssen alle Werte des Filters im Multiline vorkommen.
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
                // Tats�chlichen String ermitteln --------------------------------------------
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
                if (TMPMultiLine && !_String.Contains("\r")) { TMPMultiLine = false; } // Zeilen mit nur einem Eintrag k�nnen ohne Multiline behandel werden.
                //if (Typ == enFilterType.KeinFilter)
                //{
                //    Develop.DebugPrint(enFehlerArt.Fehler, "'Kein Filter' wurde �bergeben: " + ToString());
                //}
                if (!TMPMultiLine) {
                    var BedingungErf�llt = false;
                    for (var FiltNachNr = 0; FiltNachNr < filter.SearchValue.Count; FiltNachNr++) {
                        BedingungErf�llt = CompareValues(_String, filter.SearchValue[FiltNachNr], Typ);
                        if (Oder && BedingungErf�llt) { return true; }
                        if (Und && BedingungErf�llt == false) { return false; } // Bei diesem UND hier m�ssen allezutreffen, deshalb kann getrost bei einem False dieses zur�ckgegeben werden.
                    }
                    return BedingungErf�llt;
                }
                List<string> VorhandenWerte = new(_String.SplitByCR());
                if (VorhandenWerte.Count == 0) // Um den Filter, der nach 'Leere' Sucht, zu befriediegen
                {
                    VorhandenWerte.Add("");
                }
                // Diese Reihenfolge der For Next ist unglaublich wichtig:
                // Sind wenigere VORHANDEN vorhanden als FilterWerte, dann durchsucht diese Routine zu wenig Eintr�ge,
                // bevor sie bei einem UND ein False zur�ckgibt
                for (var FiltNachNr = 0; FiltNachNr < filter.SearchValue.Count; FiltNachNr++) {
                    var BedingungErf�llt = false;
                    foreach (var t in VorhandenWerte) {
                        BedingungErf�llt = CompareValues(t, filter.SearchValue[FiltNachNr], Typ);
                        if (Oder && BedingungErf�llt) { return true; }// Irgendein vorhandener Value trifft zu!!! Super!!!
                        if (Und && BedingungErf�llt) { break; }// Irgend ein vorhandener Value trifft zu, restliche Pr�fung uninteresant
                    }
                    if (Und && !BedingungErf�llt) // Einzelne UND konnte nicht erf�llt werden...
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
            if (column == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Spalte ung�ltig!<br>" + _database.Filename); }
            if (row == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Zeile ung�ltig!!<br>" + _database.Filename); }
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

        public void Set(ColumnItem column, RowItem row, Point value) // Main Method
=>
            // {X=253,Y=194} MUSS ES SEIN, pr�fen
            Set(column, row, value.ToString());

        public void Set(string columnName, RowItem row, int value) => Set(_database.Column[columnName], row, value.ToString());

        public void Set(ColumnItem column, RowItem row, int value) => Set(column, row, value.ToString());

        public void Set(string columnName, RowItem row, double value) => Set(_database.Column[columnName], row, value.ToString());

        public void Set(ColumnItem column, RowItem row, double value) => Set(column, row, value.ToString());

        /// <summary>
        ///
        /// </summary>
        /// <param name="Column"></param>
        /// <param name="Row"></param>
        /// <param name="ContentSize">Wird im Scale der Datenbank gespeichert, da es ja Benutzer�bergreifend ist</param>
        public void SetSizeOfCellContent(ColumnItem Column, RowItem Row, Size ContentSize) {
            var CellKey = KeyOfCell(Column, Row);
            if (!ContainsKey(CellKey)) { return; }
            this[CellKey].Size = ContentSize;
        }

        public List<string> ValuesReadable(ColumnItem Column, RowItem Row, enShortenStyle style) => CellItem.ValuesReadable(Column, Row, style);

        internal static List<RowItem> ConnectedRowsOfRelations(string CompleteRelationText, RowItem Row) {
            List<RowItem> AllRows = new();
            var Names = Row.Database.Column[0].GetUcaseNamesSortedByLenght();
            var RelationTextLine = CompleteRelationText.ToUpper().SplitByCR();
            foreach (var thisTextLine in RelationTextLine) {
                var tmp = thisTextLine;
                List<RowItem> R = new();
                for (var Z = Names.Count - 1; Z > -1; Z--) {
                    if (tmp.IndexOfWord(Names[Z], 0, RegexOptions.IgnoreCase) > -1) {
                        R.Add(Row.Database.Row[Names[Z]]);
                        tmp = tmp.Replace(Names[Z], string.Empty);
                    }
                }
                if (R.Count == 1 || R.Contains(Row)) { AllRows.AddIfNotExists(R); } // Bei mehr als einer verkn�pften Reihe MUSS die die eigene Reihe dabei sein.
            }
            return AllRows;
        }

        internal static void Invalidate_CellContentSize(ColumnItem Column, RowItem Row) {
            var CellKey = KeyOfCell(Column, Row);
            if (Column.Database.Cell.ContainsKey(CellKey)) {
                Column.Database.Cell[CellKey].InvalidateSize();
            }
        }

        internal string CompareKey(ColumnItem Column, RowItem Row) => DataFormat.CompareKey(GetString(Column, Row), Column.Format);

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

        internal void Load_310(ColumnItem _Column, RowItem _Row, string Value, int Width, int Height) {
            if (_Row == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Row konnte nicht generiert werden."); }
            if (_Column == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Column konnte nicht generiert werden."); }
            var CellKey = KeyOfCell(_Column, _Row);
            if (ContainsKey(CellKey)) {
                var c = this[CellKey];
                c.Value = Value; // Auf jeden Fall setzen. Auch falls es nachher entfernt wird, so ist es sicher leer
                c.Size = Width > 0 ? new Size(Width, Height) : Size.Empty;
                if (string.IsNullOrEmpty(Value)) { Remove(CellKey); }
            } else {
                Add(CellKey, new CellItem(Value, Width, Height));
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
                Develop.CheckStackForOverflow(); // Um Rauszufinden, ob endlos-Schleifen �fters  vorkommen. Zuletzt 24.11.2020
                return RemoveOrphans();
            }
        }

        internal void SaveToByteList(ref List<byte> l) {
            RemoveOrphans();
            foreach (var ThisString in this) {
                _database.SaveToByteList(l, ThisString);
            }
        }

        internal void SetValueBehindLinkedValue(ColumnItem Column, RowItem Row, string Value) {
            _database.BlockReload(false);
            if (Column == null || _database.Column.SearchByKey(Column.Key) == null) {
                _database?.DevelopWarnung("Spalte ung�ltig!");
                Develop.DebugPrint(enFehlerArt.Fehler, "Spalte ung�ltig!<br>" + _database.Filename);
            }
            if (Row == null || _database.Row.SearchByKey(Row.Key) == null) {
                _database?.DevelopWarnung("Zeile ung�ltig!!");
                Develop.DebugPrint(enFehlerArt.Fehler, "Zeile ung�ltig!!<br>" + _database.Filename);
            }
            Value = Column.AutoCorrect(Value);
            var CellKey = KeyOfCell(Column.Key, Row.Key);
            var OldValue = string.Empty;
            if (ContainsKey(CellKey)) { OldValue = this[CellKey].Value; }
            if (Value == OldValue) { return; }
            _database.WaitEditable();
            _database.AddPending(enDatabaseDataType.ce_Value_withoutSizeData, Column.Key, Row.Key, OldValue, Value, true);
            Column._UcaseNamesSortedByLenght = null;
            DoSpecialFormats(Column, Row.Key, OldValue, false);
            SystemSet(_database.Column.SysRowChanger, Row, _database.UserName);
            SystemSet(_database.Column.SysRowChangeDate, Row, DateTime.Now.ToString(Constants.Format_Date5));
            Invalidate_CellContentSize(Column, Row);
            Column.Invalidate_TmpColumnContentWidth();
            OnCellValueChanged(new CellEventArgs(Column, Row));
        }

        internal void SystemSet(ColumnItem Column, RowItem Row, string Value) {
            if (Column == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Spalte ung�ltig!<br>" + _database.Filename); }
            if (Row == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Zeile ung�ltig!<br>" + _database.Filename); }
            if (string.IsNullOrEmpty(Column.Identifier)) { Develop.DebugPrint(enFehlerArt.Fehler, "SystemSet nur bei System-Spalten m�glich: " + ToString()); }
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

        private static (ColumnItem column, RowItem row) RepairLinkedCellValue(Database LinkedDatabase, ColumnItem column, RowItem row, bool AddRowIfNotExists) {
            // if (column.Format != enDataFormat.LinkedCell) { Develop.DebugPrint(enFehlerArt.Fehler, "Falsches Format! " + Database.Filename + " " + column.Name); }
            // var targetColumnKeyx = -1;
            //var targetRowKey = -1;
            //if (column.LinkedDatabase() == null) { return Ergebnis("Die verlinkte Datenbank existiert nicht"); }
            ColumnItem targetColumn = null;
            RowItem targetRow = null;
            ///
            /// Spaltenschl�ssel in der Ziel-Datenbank ermitteln
            ///
            if (column.LinkedCell_ColumnKey >= 0) {
                // Fixe angabe
                targetColumn = LinkedDatabase.Column.SearchByKey(column.LinkedCell_ColumnKey);
            } else {
                // Spalte aus einer Spalte lesen
                var LinkedCell_ColumnValueFoundInColumn = column.Database.Column.SearchByKey(column.LinkedCell_ColumnValueFoundIn);
                if (LinkedCell_ColumnValueFoundInColumn == null) { return Ergebnis("Die Spalte, aus der der Spaltenschl�ssel kommen soll, existiert nicht."); }
                if (!int.TryParse(row.CellGetString(LinkedCell_ColumnValueFoundInColumn), out var colKey)) { return Ergebnis("Der Text Spalte der Spalte, aus der der Spaltenschl�ssel kommen soll, ist fehlerhaft."); }
                if (string.IsNullOrEmpty(column.LinkedCell_ColumnValueAdd)) {   // Ohne Vorsatz
                    targetColumn = LinkedDatabase.Column.SearchByKey(colKey);
                } else {
                    // Mit Vorsatz
                    var tarCx = LinkedDatabase.Column.SearchByKey(colKey);
                    if (tarCx == null) { return Ergebnis("Die Spalte, aus der der Spaltenschl�ssel (mit anschlie�enden Zusatz) kommen soll, existiert nicht."); }
                    targetColumn = LinkedDatabase.Column[column.LinkedCell_ColumnValueAdd + tarCx.Name];
                }
            }
            if (targetColumn == null) { return Ergebnis("Die Spalte ist in der Zieldatenbank nicht vorhanden."); }
            //  if (targetColumnKeyx < 0) { return Ergebnis("Die Spalte ist in der verlinkten Datenbank nicht vorhanden."); }
            //var targetColumn = LinkedDatabase.Column[targetColumnKeyx];
            //if (targetColumn == null) { return Ergebnis("Die Spalte ist in der verlinkten Datenbank nicht vorhanden."); }
            ///
            /// Zeilenschl�ssel lesen
            ///
            var LinkedCell_RowColumn = column.Database.Column.SearchByKey(column.LinkedCell_RowKey);
            if (LinkedCell_RowColumn == null) { return Ergebnis("Die Spalte, aus der der Zeilenschl�ssel kommen soll, existiert nicht."); }
            if (row.CellIsNullOrEmpty(LinkedCell_RowColumn)) { return Ergebnis("Kein Zeilenschl�ssel angegeben."); }
            targetRow = LinkedDatabase.Row[row.CellGetString(LinkedCell_RowColumn)];
            if (targetRow == null && AddRowIfNotExists) {
                targetRow = LinkedDatabase.Row.Add(row.CellGetString(LinkedCell_RowColumn));
            }
            return targetRow == null ? Ergebnis("Die Zeile ist in der Zieldatenbank nicht vorhanden.") : Ergebnis(string.Empty);
            /// --------Subroutine---------------------------
            (ColumnItem column, RowItem row) Ergebnis(string fehler) {
                column.Database.BlockReload(false);
                if (string.IsNullOrEmpty(fehler)) {
                    column.Database.Cell.SetValueBehindLinkedValue(column, row, KeyOfCell(targetColumn.Key, targetRow.Key));
                    return (targetColumn, targetRow);
                } else {
                    column.Database.Cell.SetValueBehindLinkedValue(column, row, string.Empty);
                    return (null, null);
                }
            }
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

        private string ChangeTextToRowId(string CompleteRelationText, string OldValue, string NewValue, int KeyOfCHangedRow) {
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
            // Nochmal am Schluss, wenn die W�rter alle lang sind, und die nicht mehr zum ZUg kommen.
            if (OldValue.Length > NewValue.Length) {
                DoReplace(OldValue, KeyOfCHangedRow);
                DoReplace(NewValue, KeyOfCHangedRow);
            } else {
                DoReplace(NewValue, KeyOfCHangedRow);
                DoReplace(OldValue, KeyOfCHangedRow);
            }
            return CompleteRelationText;
            void DoReplace(string Name, int Key) {
                if (!string.IsNullOrEmpty(Name)) {
                    CompleteRelationText = CompleteRelationText.Replace(Name, "/@X" + Key.ToString() + "X@/", RegexOptions.IgnoreCase);
                }
            }
        }

        /// <summary>
        /// �ndert bei allen Zeilen - die den gleichen Key (KeyColumn, Relation) benutzen wie diese Zeile - den Inhalt der Zellen ab um diese gleich zu halten.
        /// </summary>
        /// <param name="currentvalue"></param>
        /// <param name="column"></param>
        /// <param name="rowKey"></param>
        private void ChangeValueOfKey(string currentvalue, ColumnItem column, int rowKey) {
            var keyc = _database.Column.SearchByKey(column.KeyColumnKey); // Schl�sselspalte f�r diese Spalte bestimmen
            if (keyc is null) { return; }
            List<RowItem> Rows;
            var ownRow = _database.Row.SearchByKey(rowKey);
            Rows = keyc.Format == enDataFormat.RelationText
                ? ConnectedRowsOfRelations(ownRow.CellGetString(keyc), ownRow)
                : RowCollection.MatchesTo(new FilterItem(keyc, enFilterType.Istgleich_Gro�KleinEgal, ownRow.CellGetString(keyc)));
            Rows.Remove(ownRow);
            if (Rows.Count < 1) { return; }
            foreach (var thisRow in Rows) {
                thisRow.CellSet(column, currentvalue);
            }
        }

        private bool CompareValues(string IstValue, string FilterValue, enFilterType Typ) {
            // if (Column.Format == enDataFormat.LinkedCell) { Develop.DebugPrint(enFehlerArt.Fehler, "Falscher Fremdzellenzugriff"); }
            if (Typ.HasFlag(enFilterType.Gro�KleinEgal)) {
                IstValue = IstValue.ToUpper();
                FilterValue = FilterValue.ToUpper();
                Typ ^= enFilterType.Gro�KleinEgal;
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
                    var fval = FilterValue.SplitBy("|");
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
                // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer �berschreiben
                // TODO: Gro�e Felder auf NULL setzen
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

        private void RelationTextNameChanged(ColumnItem ColumnToRepair, int RowKey, string OldValue, string NewValue) {
            if (string.IsNullOrEmpty(NewValue)) { return; }
            foreach (var ThisRowItem in _database.Row) {
                if (ThisRowItem != null) {
                    if (!ThisRowItem.CellIsNullOrEmpty(ColumnToRepair)) {
                        var t = ThisRowItem.CellGetString(ColumnToRepair);
                        if (!string.IsNullOrEmpty(OldValue) && t.ToUpper().Contains(OldValue.ToUpper())) {
                            t = ChangeTextToRowId(t, OldValue, NewValue, RowKey);
                            t = ChangeTextFromRowId(t);
                            var t2 = t.SplitByCRToList().SortedDistinctList();
                            ThisRowItem.CellSet(ColumnToRepair, t2);
                        }
                        if (t.ToUpper().Contains(NewValue.ToUpper())) {
                            MakeNewRelations(ColumnToRepair, ThisRowItem, new List<string>(), t.SplitByCRToList());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// �ndert die anderen Zeilen dieser Spalte, so dass der verkn�pfte Text bei dieser und den anderen Spalten gleich ist ab.
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
            var OldBZ = new List<string>(PreviewsValue.SplitByCR()).SortedDistinctList();
            var NewBZ = new List<string>(CurrentString.SplitByCR()).SortedDistinctList();
            // Zuerst Beziehungen L�SCHEN
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
        /// �ndert bei allen anderen Spalten den Inhalt der Zelle ab (um diese gleich zuhalten), wenn diese Spalte der Key f�r die anderen ist.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="rowKey"></param>
        /// <param name="currentvalue"></param>
        private void SetSameValueOfKey(ColumnItem column, int rowKey, string currentvalue) {
            List<RowItem> Rows = null;
            var ownRow = _database.Row.SearchByKey(rowKey);
            foreach (var ThisColumn in _database.Column) {
                if (ThisColumn.LinkedCell_RowKey == column.Key || ThisColumn.LinkedCell_ColumnValueFoundIn == column.Key) {
                    LinkedCellData(ThisColumn, ownRow, true, false); // Repariert auch Zellbez�ge
                }
                if (ThisColumn.KeyColumnKey == column.Key) {
                    if (Rows == null) {
                        Rows = column.Format == enDataFormat.RelationText
                            ? ConnectedRowsOfRelations(currentvalue, ownRow)
                            : RowCollection.MatchesTo(new FilterItem(column, enFilterType.Istgleich_Gro�KleinEgal, currentvalue));
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