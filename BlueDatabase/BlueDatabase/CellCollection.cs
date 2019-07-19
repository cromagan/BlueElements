#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2019 Christian Peter
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
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using static BlueBasics.modAllgemein;
using static BlueBasics.Extensions;
using System.Text.RegularExpressions;

namespace BlueDatabase
{
    public sealed class CellCollection
    {


        #region  Variablen-Deklarationen 

        private readonly Database Database;
        internal Dictionary<string, CellItem> _cells = new Dictionary<string, CellItem>();


        private Dictionary<string, string> _freezed = null;

        #endregion


        #region  Event-Deklarationen + Delegaten 
        public event EventHandler<CellEventArgs> CellValueChanged;
        #endregion


        #region  Construktor + Initialize 


        public void Initialize()
        {
            _cells.Clear();
            _freezed = null;

        }


        public CellCollection(Database cDatabase)
        {
            Database = cDatabase;
            //       Cell = New Dictionary(Of String, CellItem)
            Initialize();
        }





        #endregion


        public CellItem this[ColumnItem colum, RowItem row]
        {
            get
            {
                if (colum == null || row == null) { return null; }
                return this[colum.Key, row.Key];
            }
        }


        public CellItem this[int colum, int row]
        {
            get
            {
                return _cells[KeyOfCell(colum, row)];
            }
        }

        /// <summary>
        ///  Gibt zurück, ob die Zelle bearbeitet werden kann.
        ///  Optional zusätzlich mit den Dateirechten.
        /// </summary>
        /// <param name="Column"></param>
        /// <param name="Row"></param>
        /// <param name="DateiRechtePrüfen"></param>
        /// <returns></returns>
        public static bool UserEditPossible(ColumnItem Column, RowItem Row, bool DateiRechtePrüfen)
        {
            return string.IsNullOrEmpty(UserEditErrorReason(Column, Row, DateiRechtePrüfen));
        }

        /// <summary>
        /// Gibt einen Fehlergrund zurück, ob die Zelle bearbeitet werden kann.
        /// Optional zusätzlich mit den Dateirechten.
        /// </summary>
        /// <param name="Row"></param>
        /// <param name="DateiRechtePrüfen"></param>
        /// <param name="Column"></param>
        /// <returns></returns>
        public static string UserEditErrorReason(ColumnItem Column, RowItem Row, bool DateiRechtePrüfen)
        {

            if (IsNullOrEmpty(Column, Row))
            {
                var c = Column.Database.Cell[Column, Row];

                if (c != null && c.DatabaseReal != c.Database)
                {
                    var t = UserEditErrorReason(c.Column, c.Row, DateiRechtePrüfen);
                    if (!string.IsNullOrEmpty(t)) { return t; }
                }

            }

            if (Column == null) { return LanguageTool.DoTranslate("Es ist keine Spalte ausgewählt.", true); }
            if (Column.Database.ReadOnly) { return LanguageTool.DoTranslate("Datenbank wurde schreibgeschützt geöffnet", true); }

            if (Row != null)
            {
                //  if (Row.Database != Database) { return LanguageTool.DoTranslate("Interner Fehler: Bezug der Datenbank zur Zeile ist fehlerhaft.", true); }
                if (Column != Column.Database.Column.SysLocked && Row.CellGetBoolean(Column.Database.Column.SysLocked) && !Column.EditTrotzSperreErlaubt) { return LanguageTool.DoTranslate("Da die Zeile als abgeschlossen markiert ist, kann die Zelle nicht bearbeitet werden.", true); }
            }
            else
            {
                //Auf neue Zeile wird geprüft
                if (!Column.IsFirst()) { return LanguageTool.DoTranslate("Neue Zeilen müssen mit der ersten Spalte beginnen.", true); }
            }


            // if (Column.Database != Row.Database) { return LanguageTool.DoTranslate("Interner Fehler: Bezug der Datenbank zur Spalte ist fehlerhaft.", true); }

            if (!Column.SaveContent) { return LanguageTool.DoTranslate("Der Spalteninhalt wird nicht gespeichert.", true); }







            if (!Column.TextBearbeitungErlaubt && !Column.DropdownBearbeitungErlaubt)
            {
                return LanguageTool.DoTranslate("Die Inhalte dieser Spalte können nicht manuell bearbeitet werden, da keine Bearbeitungsmethode erlaubt ist.", true);
            }

            if (ColumnItem.UserEditDialogTypeInTable(Column.Format, false, true, Column.MultiLine) == enEditTypeTable.None)
            {
                return "Interner Programm-Fehler: Es ist keine Bearbeitungsmethode für den Typ des Spalteninhalts '" + Column.Format + "' definiert.";
            }


            foreach (var ThisRule in Column.Database.Rules)
            {
                if (ThisRule != null)
                {
                    if (ThisRule.WillAlwaysCellOverride(Column)) { return LanguageTool.DoTranslate("Diese Zelle wird von automatischen Regeln befüllt.", true); }
                    if (ThisRule.BlockEditing(Column, Row)) { return LanguageTool.DoTranslate("Eine Regel sperrt diese Zelle.", true); }
                }
            }

            if (!Column.Database.PermissionCheck(Column.PermissionGroups_ChangeCell, Row)) { return LanguageTool.DoTranslate("Sie haben nicht die nötigen Rechte, um diesen Wert zu ändern.", true); }
            return string.Empty;
        }


        public CellItem this[string cellkey]
        {
            get
            {
                if (!_cells.ContainsKey(cellkey))
                {
                    DataOfCellKey(cellkey, out var column, out var row);
                    var c = new CellItem(column, row);
                    _cells.Add(cellkey, c);
                }
                return _cells[cellkey];
            }
        }

        public bool Freezed { get; private set; }


        public void Delete(ColumnItem Column, int RowKey)
        {
            var CellKey = KeyOfCell(Column.Key, RowKey);
            if (!_cells.ContainsKey(CellKey)) { return; }

            var Inhalt = _cells[CellKey].GetString();
            _cells.Remove(CellKey);
            DoSpecialFormats(Column, RowKey, Inhalt, false, false);
        }

        internal void Load_310(ColumnItem column, RowItem row, string value, int width, int height, Dictionary<string, CellItem> OldCells)
        {

            if (row == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Row konnte nicht generiert werden."); }
            if (column == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Column konnte nicht generiert werden."); }

            var CellKey = KeyOfCell(column, row);

            CellItem c = null;

            if (_cells.ContainsKey(CellKey))
            {
                c = _cells[CellKey];
            }
            else
            {
                if (string.IsNullOrEmpty(value)) { return; }

                if (OldCells != null && OldCells.ContainsKey(CellKey))
                {

                    c = OldCells[CellKey];
                    _cells.Add(CellKey, c);
                    OldCells.Remove(CellKey);
                }
                else
                {
                    c = new CellItem(column, row);
                    _cells.Add(CellKey, c);
                }


            }

            c.Load_310(column, row, value, width, height);

            if (string.IsNullOrEmpty(value)) { _cells.Remove(c.CellKeyReal()); }
        }



        //internal void SystemSet(ColumnItem column, RowItem row, string value, bool freezeMode)
        //{
        //    if (column == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Spalte ungültig!<br>" + Database.Filename); }
        //    if (row == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Zeile ungültig!<br>" + Database.Filename); }
        //    if (string.IsNullOrEmpty(column.Identifier)) { Develop.DebugPrint(enFehlerArt.Fehler, "SystemSet nur bei System-Spalten möglich: " + ToString()); }

        //    if (!column.SaveContent) { return; }

        //    var c = GenerateOrGetCellItem(column, row);

        //    if (value == c.String) { return; }

        //    Database.AddPending(enDatabaseDataType.ce_Value_withoutSizeData, column.Key, row.Key, c.String, value, true, freezeMode);


        //}


        #region +++ Get / Set +++

        #region Get/Set String


        public string GetString(ColumnItem column, RowItem row)
        {
            var cellkey = KeyOfCell(column, row);
            if (IsNullOrEmpty(cellkey)) { return string.Empty; } // NullOrEmpty erstellt keine leeren Zellen
            return this[cellkey].GetString();
        }

        public void Set(ColumnItem column, RowItem row, string value)
        {
            Set(column, row, value, false);
        }

        public void Set(ColumnItem column, RowItem row, string value, bool freezeMode)
        {
            var cellkey = KeyOfCell(column, row);
            if (IsNullOrEmpty(cellkey) && string.IsNullOrEmpty(value)) { return; } // NullOrEmpty erstellt keine leeren Zellen
            this[cellkey].Set(value, freezeMode);
        }

        #endregion

        #region Get/Set List<string>


        public List<string> GetList(ColumnItem column, RowItem row)
        {
            var cellkey = KeyOfCell(column, row);
            if (IsNullOrEmpty(cellkey)) { return new List<string>(); } // NullOrEmpty erstellt keine leeren Zellen
            return this[cellkey].GetList();
        }

        public void Set(ColumnItem column, RowItem row, List<string> value)
        {
            Set(column, row, value, false);
        }

        public void Set(ColumnItem column, RowItem row, List<string> value, bool freezeMode)
        {
            var cellkey = KeyOfCell(column, row);
            if (IsNullOrEmpty(cellkey))
            {
                if (value == null) { return; }
                if (value.Count == 0) { return; }
            }

            this[cellkey].Set(value, freezeMode);
        }


        #endregion

        #region Get/Set Integer


        public int GetInteger(ColumnItem column, RowItem row)
        {
            var cellkey = KeyOfCell(column, row);
            if (IsNullOrEmpty(cellkey)) { return 0; }
            return this[cellkey].GetInteger();
        }

        public void Set(ColumnItem column, RowItem row, int value)
        {
            Set(column, row, value, false);
        }

        public void Set(ColumnItem column, RowItem row, int value, bool freezeMode)
        {
            var cellkey = KeyOfCell(column, row);
            this[cellkey].Set(value, freezeMode);
        }

        #endregion

        #region Get/Set Double


        public double GetDouble(ColumnItem column, RowItem row)
        {
            var cellkey = KeyOfCell(column, row);
            if (IsNullOrEmpty(cellkey)) { return 0f; }
            return this[cellkey].GetDouble();
        }

        public void Set(ColumnItem column, RowItem row, double value)
        {
            Set(column, row, value, false);
        }

        public void Set(ColumnItem column, RowItem row, double value, bool freezeMode)
        {
            var cellkey = KeyOfCell(column, row);
            this[cellkey].Set(value, freezeMode);
        }

        #endregion

        #region Get/Set Boolean


        public bool GetBoolean(ColumnItem column, RowItem row)
        {
            var cellkey = KeyOfCell(column, row);
            if (IsNullOrEmpty(cellkey)) { return false; }
            return this[cellkey].GetBoolean();
        }

        public void Set(ColumnItem column, RowItem row, bool value)
        {
            Set(column, row, value, false);
        }

        public void Set(ColumnItem column, RowItem row, bool value, bool freezeMode)
        {
            var cellkey = KeyOfCell(column, row);
            this[cellkey].Set(value, freezeMode);
        }

        #endregion

        #region Get/Set Decimal


        public decimal GetDecimal(ColumnItem column, RowItem row)
        {
            var cellkey = KeyOfCell(column, row);
            if (IsNullOrEmpty(cellkey)) { return 0m; }
            return this[cellkey].GetDecimal();
        }

        public void Set(ColumnItem column, RowItem row, decimal value)
        {
            Set(column, row, value, false);
        }

        public void Set(ColumnItem column, RowItem row, decimal value, bool freezeMode)
        {
            var cellkey = KeyOfCell(column, row);
            this[cellkey].Set(value, freezeMode);
        }

        #endregion

        #region Get/Set Point


        public Point GetPoint(ColumnItem column, RowItem row)
        {
            var cellkey = KeyOfCell(column, row);
            if (IsNullOrEmpty(cellkey)) { return Point.Empty; }
            return this[cellkey].GetPoint();
        }

        public void Set(ColumnItem column, RowItem row, Point value)
        {
            Set(column, row, value, false);
        }

        public void Set(ColumnItem column, RowItem row, Point value, bool freezeMode)
        {
            var cellkey = KeyOfCell(column, row);
            this[cellkey].Set(value, freezeMode);
        }

        #endregion

        #region Get/Set Date


        public DateTime GetDate(ColumnItem column, RowItem row)
        {
            var cellkey = KeyOfCell(column, row);
            if (IsNullOrEmpty(cellkey)) { return new DateTime(0); }
            return this[cellkey].GetDate();
        }

        public void Set(ColumnItem column, RowItem row, DateTime value)
        {
            Set(column, row, value, false);
        }

        public void Set(ColumnItem column, RowItem row, DateTime value, bool freezeMode)
        {
            var cellkey = KeyOfCell(column, row);
            this[cellkey].Set(value, freezeMode);
        }

        #endregion

        #endregion


        public static string KeyOfCell(ColumnItem column, RowItem row)
        {
            if (column == null || row == null) { return string.Empty; }

            return KeyOfCell(column.Key, row.Key);
        }


        public static string KeyOfCell(int ColKey, int RowKey)
        {
            return ColKey + "|" + RowKey;
        }

        //public bool IsNullOrEmpty(string ColumnName, RowItem Row)
        //{
        //    return IsNullOrEmpty(Database.Column[ColumnName], Row);
        //}

        /// <summary>
        /// Prüft ob eine Zelle vorhanden oder leer ist. Diese Routine erstellt keine neuen Zellen.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(ColumnItem column, RowItem row)
        {
            if (column == null) { return true; }
            if (row == null) { return true; }

            var CellKey = KeyOfCell(column, row);
            return column.Database.Cell.IsNullOrEmpty(CellKey);
        }

        /// <summary>
        /// Prüft ob eine Zelle vorhanden oder leer ist. Diese Routine erstellt keine neuen Zellen.
        /// </summary>
        /// <param name="CellKey"></param>
        /// <returns></returns>
        public bool IsNullOrEmpty(string CellKey)
        {
            if (!_cells.ContainsKey(CellKey)) { return true; }
            return string.IsNullOrEmpty(_cells[CellKey].GetString());
        }


        internal bool RemoveOrphans()
        {
            var RemoveKeys = new List<string>();

            foreach (var pair in _cells)
            {
                if (IsNullOrEmpty(pair.Key)) { RemoveKeys.Add(pair.Key); }

                //if (!string.IsNullOrEmpty(pair.Value.String))
                //{
                //    DataOfCellKey(pair.Key, out var Column, out var Row);
                //    if (Column == null || Row == null) { RemoveKeys.Add(pair.Key); }
                //}
            }

            if (RemoveKeys.Count == 0) { return false; }

            foreach (var ThisKey in RemoveKeys)
            {
                _cells.Remove(ThisKey);
            }
            return true;
        }


        //public string GetString(string Column, RowItem Row)
        //{
        //    return GetString(Database.Column[Column], Row);
        //}



        //public string GetString(string Column, string ValueOfFirstColumn)
        //{
        //    return GetString(Database.Column[Column], Database.Row[ValueOfFirstColumn]);
        //}


        public void DoSpecialFormats(ColumnItem Column, int RowKey, string PreviewsValue, bool FreezeMode, bool DoAlways)
        {
            if (Column == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Spalte ungültig!<br>" + Database.Filename); }

            var CurrentValue = GetString(Column, Database.Row.SearchByKey(RowKey));


            if (!DoAlways && CurrentValue == PreviewsValue) { return; }


            switch (Column.Format)
            {
                case enDataFormat.RelationText:
                    RepairRelationText(Column, Database.Row.SearchByKey(RowKey), PreviewsValue, FreezeMode);
                    SetSameValueOfKey(Column, RowKey, CurrentValue, FreezeMode);
                    break;
                case enDataFormat.LinkedCell:
                    if (DoAlways)
                    {
                        RepairLinkedCellValue(Column, Database.Row.SearchByKey(RowKey), false);
                    }
                    break;

            }

            if (!string.IsNullOrEmpty(Column.I_Am_A_Key_For_Other_Column))
            {
                SetSameValueOfKey(Column, RowKey, CurrentValue, FreezeMode);
            }



            if (Column.IsFirst())
            {
                foreach (var ThisColumnItem in Database.Column)
                {
                    if (ThisColumnItem != null)
                    {
                        switch (ThisColumnItem.Format)
                        {
                            //case enDataFormat.Relation:
                            //    RelationNameChanged(ThisColumnItem, PreviewsValue, CurrentValue);
                            //    break;
                            case enDataFormat.RelationText:
                                RelationTextNameChanged(ThisColumnItem, RowKey, PreviewsValue, CurrentValue, FreezeMode);
                                break;
                        }
                    }
                }
            }

            if (Column.KeyColumnKey > -1)
            {
                ChangeValueOfKey(CurrentValue, Column, RowKey, FreezeMode);
            }


        }

        private void ChangeValueOfKey(string currentvalue, ColumnItem column, int rowKey, bool freezeMode)
        {
            var keyc = Database.Column.SearchByKey(column.KeyColumnKey);
            if (keyc is null) { return; }

            List<RowItem> Rows = null;
            var ownRow = Database.Row.SearchByKey(rowKey);
            if (keyc.Format == enDataFormat.RelationText)
            {
                Rows = CellCollection.ConnectedRowsOfRelations(ownRow.CellGetString(keyc), ownRow);
            }
            else
            {
                Rows = RowCollection.MatchesTo(new FilterItem(keyc, enFilterType.Istgleich_GroßKleinEgal, ownRow.CellGetString(keyc)));
            }
            Rows.Remove(ownRow);
            if (Rows.Count < 1) { return; }


            foreach (var thisRow in Rows)
            {
                thisRow.CellSet(column, currentvalue, freezeMode);
            }


        }

        private void SetSameValueOfKey(ColumnItem column, int rowKey, string currentvalue, bool freezeMode)
        {

            List<RowItem> Rows = null;
            var ownRow = Database.Row.SearchByKey(rowKey);


            foreach (var ThisColumn in Database.Column)
            {

                if (ThisColumn.LinkedCell_RowKey == column.Key || ThisColumn.LinkedCell_ColumnValueFoundIn == column.Key)
                {
                    RepairLinkedCellValue(ThisColumn, ownRow, freezeMode);
                }


                if (ThisColumn.KeyColumnKey == column.Key)
                {

                    if (Rows == null)
                    {
                        if (column.Format == enDataFormat.RelationText)
                        {
                            Rows = CellCollection.ConnectedRowsOfRelations(currentvalue, ownRow);
                        }
                        else
                        {
                            Rows = RowCollection.MatchesTo(new FilterItem(column, enFilterType.Istgleich_GroßKleinEgal, currentvalue));
                        }
                        Rows.Remove(ownRow);
                    }
                    if (Rows.Count < 1)
                    {
                        ownRow.CellSet(ThisColumn, string.Empty, freezeMode);
                    }
                    else
                    {
                        ownRow.CellSet(ThisColumn, Rows[0].CellGetString(ThisColumn), freezeMode);
                    }

                }
            }
        }



        private static string RepairLinkedCellValue(ColumnItem column, RowItem row, bool FreezeMode)
        {
            var cell = column.Database.Cell[column, row];

            if (cell.ColumnReal.Format != enDataFormat.LinkedCell) { Develop.DebugPrint(enFehlerArt.Fehler, "Falsches Format! " + cell.ColumnReal.Database.Filename + " " + column.Name); }
            var targetColumnKey = -1;
            var targetRowKey = -1;

            if (cell.Database == null) { return Ergebnis("Die verlinkte Datenbank existiert nicht"); }
            if (cell.Database == cell.DatabaseReal) { return Ergebnis("Die Datenbank ist nicht verlinkt"); }


            ///
            /// Spaltenschlüssel in der Ziel-Datenbank ermitteln
            ///
            if (cell.ColumnReal.LinkedCell_ColumnKey >= 0)
            {
                // Fixe angabe
                targetColumnKey = cell.ColumnReal.LinkedCell_ColumnKey;
            }
            else
            {
                // Spalte aus einer Spalte lesen
                var LinkedCell_ColumnValueFoundInColumn = cell.ColumnReal.Database.Column.SearchByKey(cell.ColumnReal.LinkedCell_ColumnValueFoundIn);
                if (LinkedCell_ColumnValueFoundInColumn == null) { return Ergebnis("Die Spalte, aus der der Spaltenschlüssel kommen soll, existiert nicht."); }

                if (!int.TryParse(cell.RowReal.CellGetString(LinkedCell_ColumnValueFoundInColumn), out var colKey)) { return Ergebnis("Der Text Spalte der Spalte, aus der der Spaltenschlüssel kommen soll, ist fehlerhaft."); }

                if (string.IsNullOrEmpty(cell.ColumnReal.LinkedCell_ColumnValueAdd))
                {   // Ohne Vorsatz
                    targetColumnKey = colKey;
                }
                else
                {
                    // Mit Vorsatz
                    var tarCx = cell.ColumnReal.LinkedDatabase().Column.SearchByKey(colKey);
                    var tarCxn = cell.ColumnReal.LinkedDatabase().Column[cell.ColumnReal.LinkedCell_ColumnValueAdd + tarCx.Name];
                    if (tarCxn != null) { targetColumnKey = tarCxn.Key; }
                }
            }

            if (targetColumnKey < 0) { return Ergebnis("Die Spalte ist in der verlinkten Datenbank nicht vorhanden."); }


            ///
            /// Zeilenschlüssel lesen
            ///   
            var LinkedCell_RowColumn = cell.DatabaseReal.Column.SearchByKey(cell.ColumnReal.LinkedCell_RowKey);
            if (LinkedCell_RowColumn == null) { return Ergebnis("Die Spalte, aus der der Zeilenschlüssel kommen soll, existiert nicht."); }

            if (cell.RowReal.CellIsNullOrEmpty(LinkedCell_RowColumn)) { return Ergebnis("Kein Zeilenschlüssel angegeben."); }

            var tarR = cell.ColumnReal.LinkedDatabase().Row[cell.RowReal.CellGetString(LinkedCell_RowColumn)];

            if (tarR == null && cell.ColumnReal.LinkedCell_Behaviour == enFehlendesZiel.ZeileAnlegen)
            {
                tarR = cell.ColumnReal.LinkedDatabase().Row.Add(cell.RowReal.CellGetString(LinkedCell_RowColumn));
            }

            if (tarR == null) { return Ergebnis("Die Zeile ist in der Zieldatenbank nicht vorhanden."); }

            targetRowKey = tarR.Key;

            return Ergebnis(string.Empty);

            /// --------Subroutine---------------------------
            string Ergebnis(string fehler)
            {
                if (string.IsNullOrEmpty(fehler))
                {
                    cell.SetReal(CellCollection.KeyOfCell(targetColumnKey, targetRowKey), FreezeMode);
                }
                else
                {
                    cell.SetReal(string.Empty, FreezeMode);
                }

                return fehler;
            }

        }




        private void RepairRelationText(ColumnItem Column, RowItem Row, string PreviewsValue, bool FreezeMode)
        {
            var CurrentString = GetString(Column, Row);
            CurrentString = ChangeTextToRowId(CurrentString, string.Empty, string.Empty, -1);
            CurrentString = ChangeTextFromRowId(CurrentString);


            if (CurrentString != GetString(Column, Row))
            {
                Set(Column, Row, CurrentString, FreezeMode);
                return;
            }


            var OldBZ = new List<string>(PreviewsValue.SplitByCR()).SortedDistinctList();
            var NewBZ = new List<string>(CurrentString.SplitByCR()).SortedDistinctList();

            // Zuerst Beziehungen LÖSCHEN
            foreach (var t in OldBZ)
            {
                if (!NewBZ.Contains(t))
                {

                    var X = ConnectedRowsOfRelations(t, Row);
                    foreach (var ThisRow in X)
                    {

                        if (ThisRow != null && ThisRow != Row)
                        {
                            var ex = ThisRow.CellGetList(Column);

                            if (X.Contains(Row))
                            {
                                ex.Remove(t);
                            }
                            else
                            {
                                ex.Remove(t.ReplaceWord(ThisRow.CellFirstString(), Row.CellFirstString(), RegexOptions.IgnoreCase));
                            }
                            ThisRow.CellSet(Column, ex.SortedDistinctList(), FreezeMode);
                        }

                    }
                }
            }

            MakeNewRelations(Column, Row, OldBZ, NewBZ, FreezeMode);
        }

        private void MakeNewRelations(ColumnItem Column, RowItem Row, List<string> OldBZ, List<string> NewBZ, bool FreezeMode)
        {
            //Develop.CheckStackForOverflow();
            //// Dann die neuen Erstellen


            foreach (var t in NewBZ)
            {
                if (!OldBZ.Contains(t))
                {
                    var X = ConnectedRowsOfRelations(t, Row);
                    foreach (var ThisRow in X)
                    {
                        if (ThisRow != Row)
                        {
                            var ex = ThisRow.CellGetList(Column);
                            if (X.Contains(Row))
                            {
                                ex.Add(t);
                            }
                            else
                            {
                                ex.Add(t.ReplaceWord(ThisRow.CellFirstString(), Row.CellFirstString(), RegexOptions.IgnoreCase));
                            }
                            ThisRow.CellSet(Column, ex.SortedDistinctList(), FreezeMode);
                        }
                    }
                }
            }
        }


        private string ChangeTextToRowId(string CompleteRelationText, string OldValue, string NewValue, int KeyOfCHangedRow)
        {
            var Names = Database.Column[0].GetUcaseNamesSortedByLenght();
            var DidOld = false;
            var DidNew = false;




            for (var Z = Names.Count - 1; Z > -1; Z--)
            {



                if (!DidOld && Names[Z].Length <= OldValue.Length)
                {
                    DidOld = true;
                    DoReplace(OldValue, KeyOfCHangedRow);
                }
                if (!DidNew && Names[Z].Length <= NewValue.Length)
                {
                    DidNew = true;
                    DoReplace(NewValue, KeyOfCHangedRow);
                }

                if (CompleteRelationText.ToUpper().Contains(Names[Z]))
                {
                    DoReplace(Names[Z], Database.Row[Names[Z]].Key);
                }


            }


            if (string.IsNullOrEmpty(NewValue)) { return CompleteRelationText; }


            // Nochmal am Schluss, wenn die Wörter alle lang sind, und die nicht mehr zum ZUg kommen.
            if (OldValue.Length > NewValue.Length)
            {
                DoReplace(OldValue, KeyOfCHangedRow);
                DoReplace(NewValue, KeyOfCHangedRow);
            }
            else
            {
                DoReplace(NewValue, KeyOfCHangedRow);
                DoReplace(OldValue, KeyOfCHangedRow);
            }

            return CompleteRelationText;

            void DoReplace(string Name, int Key)
            {
                if (!string.IsNullOrEmpty(Name))
                {
                    CompleteRelationText = CompleteRelationText.Replace(Name, "/@X" + Key.ToString() + "X@/", RegexOptions.IgnoreCase);
                }
            }

        }

        internal string CompareKey(ColumnItem Column, RowItem Row)
        {
            return DataFormat.CompareKey(GetString(Column, Row), Column.Format);
        }

        private string ChangeTextFromRowId(string CompleteRelationText)
        {


            foreach (var RowItem in Database.Row)
            {
                if (RowItem != null)
                {
                    CompleteRelationText = CompleteRelationText.Replace("/@X" + RowItem.Key.ToString() + "X@/", RowItem.CellFirstString());
                }
            }
            return CompleteRelationText;
        }


        internal static List<RowItem> ConnectedRowsOfRelations(string CompleteRelationText, RowItem Row)
        {
            var AllRows = new List<RowItem>();
            var Names = Row.Database.Column[0].GetUcaseNamesSortedByLenght();

            var RelationTextLine = CompleteRelationText.ToUpper().SplitByCR();

            foreach (var thisTextLine in RelationTextLine)
            {
                var tmp = thisTextLine;
                var R = new List<RowItem>();

                for (var Z = Names.Count - 1; Z > -1; Z--)
                {
                    if (tmp.IndexOfWord(Names[Z], 0, RegexOptions.IgnoreCase) > -1)
                    {
                        R.Add(Row.Database.Row[Names[Z]]);
                        tmp = tmp.Replace(Names[Z], string.Empty);
                    }
                }
                if (R.Count == 1 || R.Contains(Row)) { AllRows.AddIfNotExists(R); } // Bei mehr als einer verknüpften Reihe MUSS die die eigene Reihe dabei sein.

            }



            return AllRows;
        }

        public void DataOfCellKey(string CellKey, out ColumnItem Column, out RowItem Row)
        {


            if (string.IsNullOrEmpty(CellKey))
            {
                Column = null;
                Row = null;
                return;
            }

            var cd = CellKey.SplitBy("|");
            if (cd.GetUpperBound(0) != 1) { Develop.DebugPrint(enFehlerArt.Fehler, "Falscher CellKey übergeben: " + CellKey); }

            Column = Database.Column.SearchByKey(int.Parse(cd[0]));
            Row = Database.Row.SearchByKey(int.Parse(cd[1]));
        }

        private void RelationTextNameChanged(ColumnItem ColumnToRepair, int RowKey, string OldValue, string NewValue, bool FreezeMode)
        {

            if (string.IsNullOrEmpty(NewValue)) { return; }


            foreach (var ThisRowItem in Database.Row)
            {
                if (ThisRowItem != null)
                {
                    if (!ThisRowItem.CellIsNullOrEmpty(ColumnToRepair))
                    {
                        var t = ThisRowItem.CellGetString(ColumnToRepair);

                        if (!string.IsNullOrEmpty(OldValue) && t.ToUpper().Contains(OldValue.ToUpper()))
                        {
                            t = ChangeTextToRowId(t, OldValue, NewValue, RowKey);
                            t = ChangeTextFromRowId(t);
                            var t2 = t.SplitByCRToList().SortedDistinctList();
                            ThisRowItem.CellSet(ColumnToRepair, t2, FreezeMode);
                        }

                        if (t.ToUpper().Contains(NewValue.ToUpper()))
                        {
                            MakeNewRelations(ColumnToRepair, ThisRowItem, new List<string>(), t.SplitByCRToList(), FreezeMode);
                        }
                    }
                }
            }
        }


        //public void Set(ColumnItem Column, RowItem Row, string Value, bool FreezeMode)
        //{
        //    if (Column == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Spalte ungültig!<br>" + Database.Filename); }
        //    if (Row == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Zeile ungültig!!<br>" + Database.Filename); }


        //    if (Column.Format == enDataFormat.LinkedCell)
        //    {
        //        var LCcell = CellCollection.LinkedCellData(column, row);
        //        if (LCcell != null) { LCrow?.Database.Cell.Set(LCColumn, LCrow, Value, false); }
        //        return;
        //    }
        //    SetValueBehindLinkedValue(Column, Row, Value, FreezeMode);

        //}

        internal void InvalidateAllSizes()
        {

            foreach (var ThisColumn in Database.Column)
            {
                ThisColumn.Invalidate_ColumAndContent();
            }

        }

        //internal void SetValueBehindLinkedValue(ColumnItem Column, RowItem Row, string Value, bool FreezeMode)
        //{
        //    Value = Column.AutoCorrect(Value);

        //    var CellKey = KeyOfCell(Column.Key, Row.Key);
        //    var OldValue = string.Empty;

        //    if (_cells.ContainsKey(CellKey)) { OldValue = _cells[CellKey].String; }

        //    if (Value == OldValue) { return; }

        //    Database.AddPending(enDatabaseDataType.ce_Value_withoutSizeData, Column.Key, Row.Key, OldValue, Value, true, FreezeMode);

        //    Column._UcaseNamesSortedByLenght = null;

        //    DoSpecialFormats(Column, Row.Key, OldValue, FreezeMode, false);


        //    Set(Database.Column.SysRowChanger, Row, Database.UserName, FreezeMode);
        //    Set(Database.Column.SysRowChangeDate, Row, DateTime.Now.ToString(), FreezeMode);


        //    Invalidate_CellContentSize(Column, Row);
        //    Column.Invalidate_TmpColumnContentWidth();

        //    OnCellValueChanged(new CellEventArgs(Column, Row));
        //}


        public static string AutomaticInitalValue(CellItem cell)

        {
            if (cell == null) { return string.Empty; }



            if (cell.Column.VorschlagsColumn < 0) { return string.Empty; }


            var cc = cell.Database.Column.SearchByKey(cell.Column.VorschlagsColumn);
            if (cc == null) { return string.Empty; }

            var F = new FilterCollection(cell.Database);
            F.Add(new FilterItem(cc, enFilterType.Istgleich_GroßKleinEgal, cell.Row.CellGetString(cc)));
            F.Add(new FilterItem(cell.Column, enFilterType.Ungleich_MultiRowIgnorieren, string.Empty));

            var rows = RowCollection.CalculateSortedRows(cell.Database, F, null);
            rows.Remove(cell.Row);
            if (rows.Count == 0) { return string.Empty; }
            return rows[0].CellGetString(cell.Column);
        }


        internal static void Invalidate_CellContentSize(ColumnItem Column, RowItem Row)
        {
            var CellKey = KeyOfCell(Column, Row);

            if (Column.Database.Cell._cells.ContainsKey(CellKey))
            {
                Column.Database.Cell._cells[CellKey].InvalidateSize();
            }
        }


        public bool MatchesTo(ColumnItem column, RowItem row, FilterItem Filter)
        {
            //lock (Database.Lock_Parsing)
            //{
            //Grundlegendes zu UND und ODER:
            //Ein Filter kann mehrere Werte heben, diese müssen ein Attrribut UND oder ODER haben.
            //Bei UND müssen alle Werte des Filters im Multiline vorkommen.
            //Bei ODER muss ein Wert des Filters im Multiline vorkommen.
            //Beispiel: UND-Filter mit C & D
            //Wenn die Zelle       A B C D hat, trifft der UND-Filter zwar nicht bei den ersten beiden zu, aber bei den letzten.
            //Um genau zu sein C:  - - + -
            //                 D:  - - - +
            //Deswegen muss beim einem UND-Filter nur EINER der Zellenwerte zutreffen. 



            if (Filter.FilterType == enFilterType.KeinFilter) { Develop.DebugPrint(enFehlerArt.Fehler, "Kein Filter angegeben: " + ToString()); }


            var TMPMultiLine = false;
            var Typ = Filter.FilterType;
            var BedingungErfüllt = false;
            int FiltNachNr;


            var Oder = Convert.ToBoolean(Typ & enFilterType.ODER);
            if (Oder) { Typ = Typ ^ enFilterType.ODER; }
            var Und = Convert.ToBoolean(Typ & enFilterType.UND);
            if (Und) { Typ = Typ ^ enFilterType.UND; }


            if (Und && Oder) { Develop.DebugPrint(enFehlerArt.Fehler, "Filter-Anweisung erwartet ein 'Und' oder 'Oder': " + ToString()); }

            if (column != null) { TMPMultiLine = column.MultiLine; }


            if (Convert.ToBoolean(Typ & enFilterType.MultiRowIgnorieren))
            {
                TMPMultiLine = false;
                Typ = Typ ^ enFilterType.MultiRowIgnorieren;
            }


            if (Typ == enFilterType.KeinFilter)
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "'Kein Filter' wurde übergeben: " + ToString());
            }


            if (Filter.SearchValue.Count < 2)
            {
                Oder = true;
                Und = false; // Wenn nur EIN Eintrag gecheckt wird, ist es EGAL, ob UND oder ODER.
            }



            var _String = string.Empty;
            var CellKey = KeyOfCell(column, row);

            if (_cells.ContainsKey(CellKey))
            {
                try
                {
                    _String = _cells[CellKey].GetString();
                    if (Typ.HasFlag(enFilterType.Instr)) { _String = LanguageTool.ColumnReplace(_String, column, enShortenStyle.Both); }
                }
                catch (Exception ex)
                {
                    Develop.DebugPrint(ex);
                    Pause(1, true);
                    return MatchesTo(column, row, Filter);
                }
            }
            else
            {
                _String = string.Empty;
                if (Typ.HasFlag(enFilterType.Instr)) { _String = LanguageTool.ColumnReplace(string.Empty, column, enShortenStyle.Both); }
            }


            if (_String is null) { _String = string.Empty; }



            if (TMPMultiLine && !_String.Contains("\r")) { TMPMultiLine = false; } // Zeilen mit nur einem Eintrag können ohne Multinline behandel werden.

            if (!TMPMultiLine)
            {
                for (FiltNachNr = 0; FiltNachNr < Filter.SearchValue.Count; FiltNachNr++)
                {
                    BedingungErfüllt = CompareValues(_String, Filter.SearchValue[FiltNachNr], Typ, column);
                    if (Oder && BedingungErfüllt) { return true; }
                    if (Und && BedingungErfüllt == false) { return false; } // Bei diesem UND hier müssen allezutreffen, deshalb kann getrost bei einem False dieses zurückgegeben werden.
                }

                return BedingungErfüllt;
            }

            var VorhandenWerte = new List<string>(_String.SplitByCR());
            if (VorhandenWerte.Count == 0) // Um den Filter, der nach 'Leere' Sucht, zu befriediegen
            {
                VorhandenWerte.Add("");
            }


            // Diese Reihenfolge der For Next ist unglaublich wichtig:
            // Sind wenigere VORHANDEN vorhanden als FilterWerte, dann durchsucht diese Routine zu wenig Einträge,
            // bevor sie bei einem UND ein False zurückgibt


            for (FiltNachNr = 0; FiltNachNr < Filter.SearchValue.Count; FiltNachNr++)
            {
                foreach (var t in VorhandenWerte)
                {
                    BedingungErfüllt = CompareValues(t, Filter.SearchValue[FiltNachNr], Typ, column);
                    if (Oder && BedingungErfüllt) { return true; }// Irgendein vorhandener Value trifft zu!!! Super!!!
                    if (Und && BedingungErfüllt) { break; }// Irgend ein vorhandener Value trifft zu, restliche Prüfung uninteresant
                }

                if (Und && BedingungErfüllt == false) // Einzelne UND konnte nicht erfüllt werden...
                {
                    return false;
                }
            }
            if (Und) { return true; } // alle"Und" stimmen!

            return false; // Gar kein "Oder" trifft zu...
                          //}
        }




        private bool CompareValues(string IstValue, string FilterValue, enFilterType Typ, ColumnItem Column)
        {
            if (Column.Format == enDataFormat.LinkedCell) { Develop.DebugPrint(enFehlerArt.Fehler, "Falscher Fremdzellenzugriff"); }
            if (Typ.HasFlag(enFilterType.GroßKleinEgal))
            {
                IstValue = IstValue.ToUpper();
                FilterValue = FilterValue.ToUpper();
                Typ = Typ ^ enFilterType.GroßKleinEgal;
            }


            switch (Typ)
            {
                case enFilterType.Istgleich:
                    return Convert.ToBoolean(IstValue == FilterValue);

                case (enFilterType)2: // Ungleich
                    return Convert.ToBoolean(IstValue != FilterValue);

                case enFilterType.Instr:
                    return IstValue.Contains(FilterValue);

                case enFilterType.Berechne:
                    if (string.IsNullOrEmpty(IstValue)) { return false; }
                    if (!FilterValue.ToUpper().Contains("VALUE")) { return false; }
                    var d = modErgebnis.Ergebnis(FilterValue.Replace("VALUE", IstValue, RegexOptions.IgnoreCase));
                    if (d == null) { return false; }
                    return Convert.ToBoolean(d == -1);


                case enFilterType.KeinFilter:
                    Develop.DebugPrint("Kein Filter!");
                    return true;

                case enFilterType.BeginntMit:
                    return IstValue.StartsWith(FilterValue);

                default:
                    {
                        Develop.DebugPrint(Typ);
                        return false;
                    }
            }
        }





        internal void SaveToByteList(ref List<byte> l)
        {

            try
            {
                RemoveOrphans();

                foreach (var thisCell in _cells)
                {
                    thisCell.Value.SaveToByteList(l);

                  //  Database.SaveToByteList(l, thisCell);
                }
            }
            catch (Exception ex)
            {
                Develop.DebugPrint(enFehlerArt.Fehler, ex);
            }
        }

        internal void OnCellValueChanged(CellEventArgs e)
        {
            e.Cell.Column._UcaseNamesSortedByLenght = null;
            CellValueChanged?.Invoke(this, e);
        }




        internal void Freeze()
        {
            if (Freezed) { Develop.DebugPrint(enFehlerArt.Fehler, "Datenbank ist bereits eingefrohren"); }
            _freezed = new Dictionary<string, string>();
            Freezed = true;
        }

        internal void UnFreeze()
        {
            if (!Freezed) { Develop.DebugPrint(enFehlerArt.Fehler, "Datenbank ist nicht eingefrohren"); }

            var discard = true;
            var tmp = string.Empty;

            foreach (var thisv in _freezed)
            {
                if (_cells.ContainsKey(thisv.Key))
                {
                    tmp = _cells[thisv.Key].GetString();
                }
                else
                {
                    tmp = string.Empty;
                }

                if (tmp != thisv.Value)
                {
                    var c = this[thisv.Key];
                    if (c.ColumnReal != Database.Column.SysRowChangeDate && c.ColumnReal != Database.Column.SysRowChanger)
                    {
                        discard = false;
                        break;
                    }
                }
            }


            if (discard)
            {
                foreach (var thisv in _freezed)
                {
                    if (_cells.ContainsKey(thisv.Key))
                    {


                        if (tmp != thisv.Value)
                        {
                            var c = this[thisv.Key];
                            if (c.ColumnReal == Database.Column.SysRowChangeDate || c.ColumnReal == Database.Column.SysRowChanger)
                            {
                                c.Set(thisv.Value, true);
                            }
                        }
                    }

                    Database.ChangeWorkItems(enItemState.FreezedPending, enItemState.FreezedDiscard);

                }
            }
            else
            {
                Database.ChangeWorkItems(enItemState.FreezedPending, enItemState.Pending);
            }


            //    Develop.DebugPrint_NichtImplementiert();
            Freezed = false;
            _freezed = null;

        }

        internal void AddFreeze(int columnKey, int rowKey, string previousValue)
        {
            var c = KeyOfCell(columnKey, rowKey);
            if (_freezed.ContainsKey(c)) { return; } // der Ursprüngliche Wert ist bereits gesetzt
            _freezed.Add(c, previousValue);
        }
    }
}
