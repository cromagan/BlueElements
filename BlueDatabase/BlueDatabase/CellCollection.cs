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
        private Dictionary<string, CellItem> _cells = new Dictionary<string, CellItem>();


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




        public bool Freezed { get; private set; }


        public void Delete(ColumnItem Column, int RowKey)
        {
            var CellKey = KeyOfCell(Column.Key, RowKey);
            if (!_cells.ContainsKey(CellKey)) { return; }

            var Inhalt = _cells[CellKey].Value;
            _cells.Remove(CellKey);
            DoSpecialFormats(Column, RowKey, Inhalt, false, false);
        }

        internal void Load_310(ColumnItem _Column, RowItem _Row, string Value, int Width, int Height)
        {

            if (_Row == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Row konnte nicht generiert werden."); }
            if (_Column == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Column konnte nicht generiert werden."); }

            var CellKey = KeyOfCell(_Column, _Row);

            if (_cells.ContainsKey(CellKey))
            {
                var c = _cells[CellKey];
                c.Value = Value; // Auf jeden Fall setzen. Auch falls es nachher entfernt wird, so ist es sicher leer

                if (Width > 0)
                {
                    c.Size = new Size(Width, Height);
                }
                else
                {
                    c.Size = Size.Empty;
                }

                if (string.IsNullOrEmpty(Value)) { _cells.Remove(CellKey); }
            }
            else
            {
                _cells.Add(CellKey, new CellItem(Value, Width, Height));
            }
        }



        public static string KeyOfCell(int ColKey, int RowKey)
        {
            return ColKey + "|" + RowKey;
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

        public void Set(string ColumnName, RowItem Row, List<string> Value)
        {
            Set(Database.Column[ColumnName], Row, Value);
        }

        public bool IsNullOrEmpty(ColumnItem Column, RowItem Row)
        {

            if (Column == null) { return true; }
            if (Row == null) { return true; }

            if (Column.Format == enDataFormat.LinkedCell)
            {
                var LinkedData = LinkedCellData(Column, Row, false, false);
                if (LinkedData.Item1 != null && LinkedData.Item2 != null) { return LinkedData.Item2.CellIsNullOrEmpty(LinkedData.Item1); }
                return true;
            }


            var CellKey = KeyOfCell(Column.Key, Row.Key);

            if (!_cells.ContainsKey(CellKey)) { return true; }
            return string.IsNullOrEmpty(_cells[CellKey].Value);

        }




        public static Tuple<ColumnItem, RowItem> LinkedCellData(ColumnItem column, RowItem row, bool freezemode, bool RepairEmpties)
        {

            if (column == null || row == null) { return new Tuple<ColumnItem, RowItem>(null, null); }
            if (column.Format != enDataFormat.LinkedCell) { return new Tuple<ColumnItem, RowItem>(null, null); }

            var LinkedDatabase = column.LinkedDatabase();

            if (LinkedDatabase == null) { return new Tuple<ColumnItem, RowItem>(null, null); }


            if (RepairEmpties)
            {
                return RepairLinkedCellValue(LinkedDatabase, column, row, freezemode);
            }


            var Key = column.Database.Cell.GetStringBehindLinkedValue(column, row);

            if (string.IsNullOrEmpty(Key)) { return new Tuple<ColumnItem, RowItem>(null, null); }

            var V = Key.SplitBy("|");

            if (V.Length != 2) { return RepairLinkedCellValue(LinkedDatabase, column, row, freezemode); }

            var LinkedColumn = LinkedDatabase.Column.SearchByKey(int.Parse(V[0]));
            var LinkedRow = LinkedDatabase.Row.SearchByKey(int.Parse(V[1]));

            if (KeyOfCell(LinkedColumn, LinkedRow) == Key) { return new Tuple<ColumnItem, RowItem>(LinkedColumn, LinkedRow); }

            return RepairLinkedCellValue(LinkedDatabase, column, row, freezemode);
        }

        private static Tuple<ColumnItem, RowItem> RepairLinkedCellValue(Database LinkedDatabase, ColumnItem column, RowItem row, bool FreezeMode)
        {
            // if (column.Format != enDataFormat.LinkedCell) { Develop.DebugPrint(enFehlerArt.Fehler, "Falsches Format! " + Database.Filename + " " + column.Name); }
            // var targetColumnKeyx = -1;
            //var targetRowKey = -1;

            //if (column.LinkedDatabase() == null) { return Ergebnis("Die verlinkte Datenbank existiert nicht"); }
            ColumnItem targetColumn = null;
            RowItem targetRow = null;

            ///
            /// Spaltenschlüssel in der Ziel-Datenbank ermitteln
            ///
            if (column.LinkedCell_ColumnKey >= 0)
            {
                // Fixe angabe
                targetColumn = LinkedDatabase.Column.SearchByKey(column.LinkedCell_ColumnKey);
            }
            else
            {
                // Spalte aus einer Spalte lesen
                var LinkedCell_ColumnValueFoundInColumn = column.Database.Column.SearchByKey(column.LinkedCell_ColumnValueFoundIn);
                if (LinkedCell_ColumnValueFoundInColumn == null) { return Ergebnis("Die Spalte, aus der der Spaltenschlüssel kommen soll, existiert nicht."); }

                if (!int.TryParse(row.CellGetString(LinkedCell_ColumnValueFoundInColumn), out var colKey)) { return Ergebnis("Der Text Spalte der Spalte, aus der der Spaltenschlüssel kommen soll, ist fehlerhaft."); }

                if (string.IsNullOrEmpty(column.LinkedCell_ColumnValueAdd))
                {   // Ohne Vorsatz
                    targetColumn = LinkedDatabase.Column.SearchByKey(colKey);
                }
                else
                {
                    // Mit Vorsatz
                    var tarCx = LinkedDatabase.Column.SearchByKey(colKey);
                    if (tarCx == null) { return Ergebnis("Die Spalte, aus der der Spaltenschlüssel (mit anschließenden Zusatz) kommen soll, existiert nicht."); }
                    targetColumn = LinkedDatabase.Column[column.LinkedCell_ColumnValueAdd + tarCx.Name];

                }
            }


            if (targetColumn == null) { return Ergebnis("Die Spalte ist in der Zieldatenbank nicht vorhanden."); }

            //  if (targetColumnKeyx < 0) { return Ergebnis("Die Spalte ist in der verlinkten Datenbank nicht vorhanden."); }
            //var targetColumn = LinkedDatabase.Column[targetColumnKeyx];
            //if (targetColumn == null) { return Ergebnis("Die Spalte ist in der verlinkten Datenbank nicht vorhanden."); }

            ///
            /// Zeilenschlüssel lesen
            ///   
            var LinkedCell_RowColumn = column.Database.Column.SearchByKey(column.LinkedCell_RowKey);
            if (LinkedCell_RowColumn == null) { return Ergebnis("Die Spalte, aus der der Zeilenschlüssel kommen soll, existiert nicht."); }

            if (row.CellIsNullOrEmpty(LinkedCell_RowColumn)) { return Ergebnis("Kein Zeilenschlüssel angegeben."); }

            targetRow = LinkedDatabase.Row[row.CellGetString(LinkedCell_RowColumn)];

            if (targetRow == null && column.LinkedCell_Behaviour == enFehlendesZiel.ZeileAnlegen)
            {
                targetRow = LinkedDatabase.Row.Add(row.CellGetString(LinkedCell_RowColumn));
            }

            if (targetRow == null) { return Ergebnis("Die Zeile ist in der Zieldatenbank nicht vorhanden."); }

            return Ergebnis(string.Empty);

            /// --------Subroutine---------------------------
            Tuple<ColumnItem, RowItem> Ergebnis(string fehler)
            {
                if (string.IsNullOrEmpty(fehler))
                {
                    column.Database.Cell.SetValueBehindLinkedValue(column, row, CellCollection.KeyOfCell(targetColumn.Key, targetRow.Key), FreezeMode);
                    return new Tuple<ColumnItem, RowItem>(targetColumn, targetRow);

                }
                else
                {
                    column.Database.Cell.SetValueBehindLinkedValue(column, row, string.Empty, FreezeMode);
                    return new Tuple<ColumnItem, RowItem>(null, null);
                }
            }

        }


        internal void SetAllValuesToEmpty()
        {
            _cells.Clear();
            //_cellsize.Clear();
        }


        internal bool RemoveOrphans()
        {
            try
            {
                var RemoveKeys = new List<string>();

                foreach (var pair in _cells)
                {
                    if (!string.IsNullOrEmpty(pair.Value.Value))
                    {
                        DataOfCellKey(pair.Key, out var Column, out var Row);
                        if (Column == null || Row == null) { RemoveKeys.Add(pair.Key); }
                    }
                }

                if (RemoveKeys.Count == 0) { return false; }

                foreach (var ThisKey in RemoveKeys)
                {
                    _cells.Remove(ThisKey);
                }
                return true;
            }
            catch
            {
                return RemoveOrphans();
            }
        }


        public string GetString(string Column, RowItem Row)
        {
            return GetString(Database.Column[Column], Row);
        }



        public string GetString(string Column, string ValueOfFirstColumn)
        {
            return GetString(Database.Column[Column], Database.Row[ValueOfFirstColumn]);
        }


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
                        LinkedCellData(Column, Database.Row.SearchByKey(RowKey), FreezeMode, true); // Repariert auch Cellbezüge
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
                    LinkedCellData(ThisColumn, ownRow, freezeMode, true); // Repariert auch Zellbezüge
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






        public DateTime GetDate(ColumnItem Column, RowItem Row)
        {
            if (Column == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Spalte ungültig!<br>" + Database.Filename); }
            if (Row == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Zeile ungültig!<br>" + Database.Filename); }
            var _String = GetString(Column, Row);
            if (string.IsNullOrEmpty(_String)) { return default(DateTime); }

            if (DateTimeTryParse(_String, out var d)) { return d; }

            return default(DateTime);

        }
        public void Set(ColumnItem Column, RowItem Row, List<string> Value, bool FreezeMode)
        {
            Set(Column, Row, Value.JoinWithCr(), FreezeMode);
        }
        public void Set(ColumnItem Column, RowItem Row, List<string> Value)
        {
            Set(Column, Row, Value.JoinWithCr(), false);
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


        public void Set(ColumnItem column, RowItem row, string value, bool freezemode)
        {
            if (column == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Spalte ungültig!<br>" + Database.Filename); }
            if (row == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Zeile ungültig!!<br>" + Database.Filename); }


            if (column.Format == enDataFormat.LinkedCell)
            {
                var LinkedData = LinkedCellData(column, row, freezemode, true);
                LinkedData.Item2?.Database.Cell.Set(LinkedData.Item1, LinkedData.Item2, value, false);
                return;
            }
            SetValueBehindLinkedValue(column, row, value, freezemode);

        }

        internal void InvalidateAllSizes()
        {

            foreach (var ThisColumn in Database.Column)
            {
                ThisColumn.Invalidate_ColumAndContent();
            }

        }

        internal void SetValueBehindLinkedValue(ColumnItem Column, RowItem Row, string Value, bool FreezeMode)
        {


            if (Column == null || Database.Column.SearchByKey(Column.Key) == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Spalte ungültig!<br>" + Database.Filename); }
            if (Row == null || Database.Row.SearchByKey(Row.Key) == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Zeile ungültig!!<br>" + Database.Filename); }

            Value = Column.AutoCorrect(Value);

            var CellKey = KeyOfCell(Column.Key, Row.Key);
            var OldValue = string.Empty;

            if (_cells.ContainsKey(CellKey)) { OldValue = _cells[CellKey].Value; }

            if (Value == OldValue) { return; }

            Database.AddPending(enDatabaseDataType.ce_Value_withoutSizeData, Column.Key, Row.Key, OldValue, Value, true, FreezeMode);

            Column._UcaseNamesSortedByLenght = null;

            DoSpecialFormats(Column, Row.Key, OldValue, FreezeMode, false);


            SystemSet(Database.Column.SysRowChanger, Row, Database.UserName, FreezeMode);
            SystemSet(Database.Column.SysRowChangeDate, Row, DateTime.Now.ToString(), FreezeMode);


            Invalidate_CellContentSize(Column, Row);
            Column.Invalidate_TmpColumnContentWidth();

            OnCellValueChanged(new CellEventArgs(Column, Row));
        }


        public static string AutomaticInitalValue(ColumnItem column, RowItem row)
        {
            if (column == null || row == null) { return string.Empty; }

            if (column.Format == enDataFormat.LinkedCell)
            {
                var LinkedData = LinkedCellData(column, row, false, true);
                return AutomaticInitalValue(LinkedData.Item1, LinkedData.Item2);
            }


            if (column.VorschlagsColumn < 0) { return string.Empty; }


            var cc = column.Database.Column.SearchByKey(column.VorschlagsColumn);
            if (cc == null) { return string.Empty; }

            var F = new FilterCollection(column.Database);
            F.Add(new FilterItem(cc, enFilterType.Istgleich_GroßKleinEgal, row.CellGetString(cc)));
            F.Add(new FilterItem(column, enFilterType.Ungleich_MultiRowIgnorieren, string.Empty));

            var rows = RowCollection.CalculateSortedRows(column.Database, F, null);
            rows.Remove(row);
            if (rows.Count == 0) { return string.Empty; }
            return rows[0].CellGetString(column);
        }


        internal static void Invalidate_CellContentSize(ColumnItem Column, RowItem Row)
        {
            var CellKey = KeyOfCell(Column, Row);

            if (Column.Database.Cell._cells.ContainsKey(CellKey))
            {
                Column.Database.Cell._cells[CellKey].InvalidateSize();
            }
        }

        public bool GetBoolean(ColumnItem Column, RowItem Row)
        {
            return GetString(Column, Row).FromPlusMinus();
        }

        public void Set(ColumnItem Column, RowItem Row, bool Value)
        {
            Set(Column, Row, Value.ToPlusMinus(), false);
        }
        public void Set(ColumnItem Column, RowItem Row, bool Value, bool FreezeMode)
        {
            Set(Column, Row, Value.ToPlusMinus(), FreezeMode);
        }


        internal void SystemSet(ColumnItem Column, RowItem Row, string Value, bool FreezeMode)
        {
            if (Column == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Spalte ungültig!<br>" + Database.Filename); }
            if (Row == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Zeile ungültig!<br>" + Database.Filename); }
            if (string.IsNullOrEmpty(Column.Identifier)) { Develop.DebugPrint(enFehlerArt.Fehler, "SystemSet nur bei System-Spalten möglich: " + ToString()); }

            if (!Column.SaveContent) { return; }

            var CellKey = KeyOfCell(Column, Row);
            var _String = string.Empty;

            if (_cells.ContainsKey(CellKey))
            {
                _String = _cells[CellKey].Value;
            }
            else
            {
                _cells.Add(CellKey, new CellItem(string.Empty, 0, 0));
            }

            if (Value == _String) { return; }

            Database.AddPending(enDatabaseDataType.ce_Value_withoutSizeData, Column.Key, Row.Key, _String, Value, true, FreezeMode);


        }

        public bool MatchesTo(ColumnItem column, RowItem row, FilterItem filter)
        {
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

            try
            {

                var Typ = filter.FilterType;


                // Oder-Flag ermitteln --------------------------------------------
                var Oder = Typ.HasFlag(enFilterType.ODER);
                if (Oder) { Typ = Typ ^ enFilterType.ODER; }
                // Und-Flag Ermitteln --------------------------------------------
                var Und = Typ.HasFlag(enFilterType.UND);
                if (Und) { Typ = Typ ^ enFilterType.UND; }

                if (filter.SearchValue.Count < 2)
                {
                    Oder = true;
                    Und = false; // Wenn nur EIN Eintrag gecheckt wird, ist es EGAL, ob UND oder ODER.
                }

                //if (Und && Oder) { Develop.DebugPrint(enFehlerArt.Fehler, "Filter-Anweisung erwartet ein 'Und' oder 'Oder': " + ToString()); }


                // Tatsächlichen String ermitteln --------------------------------------------
                var _String = string.Empty;
                var fColumn = column;
                if (column.Format == enDataFormat.LinkedCell)
                {
                    var LinkedData = LinkedCellData(column, row, false, false);
                    if (LinkedData.Item1 != null && LinkedData.Item2 != null)
                    {
                        _String = LinkedData.Item2.CellGetString(LinkedData.Item1);
                        fColumn = LinkedData.Item1;
                    }

                }
                else
                {
                    var CellKey = KeyOfCell(column, row);
                    if (_cells.ContainsKey(CellKey))
                    {
                        _String = _cells[CellKey].Value;
                    }
                    else
                    {
                        _String = string.Empty;
                    }

                }
                if (_String is null) { _String = string.Empty; }
                if (Typ.HasFlag(enFilterType.Instr)) { _String = LanguageTool.ColumnReplace(_String, fColumn, enShortenStyle.Both); }





                // Multiline-Typ ermitteln  --------------------------------------------
                var TMPMultiLine = false;
                if (column != null) { TMPMultiLine = column.MultiLine; }


                if (Typ.HasFlag(enFilterType.MultiRowIgnorieren))
                {
                    TMPMultiLine = false;
                    Typ = Typ ^ enFilterType.MultiRowIgnorieren;
                }
                if (TMPMultiLine && !_String.Contains("\r")) { TMPMultiLine = false; } // Zeilen mit nur einem Eintrag können ohne Multinline behandel werden.






                //if (Typ == enFilterType.KeinFilter)
                //{
                //    Develop.DebugPrint(enFehlerArt.Fehler, "'Kein Filter' wurde übergeben: " + ToString());
                //}









                if (!TMPMultiLine)
                {
                    var BedingungErfüllt = false;
                    for (var FiltNachNr = 0; FiltNachNr < filter.SearchValue.Count; FiltNachNr++)
                    {
                        BedingungErfüllt = CompareValues(_String, filter.SearchValue[FiltNachNr], Typ);
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


                for (var FiltNachNr = 0; FiltNachNr < filter.SearchValue.Count; FiltNachNr++)
                {
                    var BedingungErfüllt = false;
                    foreach (var t in VorhandenWerte)
                    {
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

            }
            catch (Exception ex)
            {
                Develop.DebugPrint(ex);
                Pause(1, true);
                return MatchesTo(column, row, filter);
            }
        }


        public Point GetPoint(ColumnItem Column, RowItem Row)
        {
            var _String = GetString(Column, Row);
            if (string.IsNullOrEmpty(_String)) { return new Point(); }
            return Extensions.PointParse(_String);
        }

        public void Set(ColumnItem Column, RowItem Row, DateTime Value)
        {
            Set(Column, Row, Value.ToString(Constants.Format_Date5), false);
        }

        public void Set(ColumnItem Column, RowItem Row, Point Value)
        {
            // {X=253,Y=194} MUSS ES SEIN, prüfen
            Set(Column, Row, Value.ToString(), false);
        }
        public void Set(string ColumnName, RowItem Row, Point Value)
        {
            Set(Database.Column[ColumnName], Row, Value);
        }


        private bool CompareValues(string IstValue, string FilterValue, enFilterType Typ)
        {
            // if (Column.Format == enDataFormat.LinkedCell) { Develop.DebugPrint(enFehlerArt.Fehler, "Falscher Fremdzellenzugriff"); }
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



        public string GetString(ColumnItem Column, RowItem Row)
        {
            if (Column == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Spalte ungültig!<br>" + Database.Filename); }
            if (Row == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Zeile ungültig!<br>" + Database.Filename); }

            if (Column.Format == enDataFormat.LinkedCell)
            {
                var LinkedData = LinkedCellData(Column, Row, false, false);
                if (LinkedData.Item1 != null && LinkedData.Item2 != null) { return LinkedData.Item2.Database.Cell.GetString(LinkedData.Item1, LinkedData.Item2); }
                return string.Empty;
            }

            var CellKey = KeyOfCell(Column, Row);

            if (!_cells.ContainsKey(CellKey)) { return string.Empty; }

            return _cells[CellKey].Value;
        }

        public string GetStringBehindLinkedValue(ColumnItem Column, RowItem Row)
        {
            if (Column == null || Row == null) { return string.Empty; }
            var CellKey = KeyOfCell(Column, Row);
            if (!_cells.ContainsKey(CellKey)) { return string.Empty; }
            return _cells[CellKey].Value;
        }

        public static string KeyOfCell(ColumnItem Column, RowItem Row)
        {
            // Alte verweise eleminieren.
            if (Column != null) { Column = Column.Database.Column.SearchByKey(Column.Key); }
            if (Row != null) { Row = Row.Database.Row.SearchByKey(Row.Key); }

            if (Column == null && Row == null) { return string.Empty; }

            if (Column == null) { return KeyOfCell(-1, Row.Key); }
            if (Row == null) { return KeyOfCell(Column.Key, -1); }
            return KeyOfCell(Column.Key, Row.Key);
        }

        public List<string> GetList(ColumnItem Column, RowItem Row)
        {
            return GetString(Column, Row).SplitByCRToList();
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
            if (Column.Format == enDataFormat.LinkedCell)
            {
                var LinkedData = LinkedCellData(Column, Row, false, true);
                if (LinkedData.Item1 != null && LinkedData.Item2 != null) { return UserEditPossible(LinkedData.Item1, LinkedData.Item2, DateiRechtePrüfen); }
                return false;
            }

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
            if (Column == null) { return LanguageTool.DoTranslate("Es ist keine Spalte ausgewählt.", true); }
            if (Column.Database.ReadOnly) { return LanguageTool.DoTranslate("Datenbank wurde schreibgeschützt geöffnet", true); }



            if (!Column.SaveContent) { return LanguageTool.DoTranslate("Der Spalteninhalt wird nicht gespeichert.", true); }

            if (Column.Format == enDataFormat.LinkedCell)
            {
                var LinkedData = LinkedCellData(Column, Row, false, true);
                if (LinkedData.Item1 != null && LinkedData.Item2 != null)
                {
                    var tmp = UserEditErrorReason(LinkedData.Item1, LinkedData.Item2, DateiRechtePrüfen);
                    if (!string.IsNullOrEmpty(tmp)) { return LanguageTool.DoTranslate("Die verlinkte Zelle kann nicht bearbeitet werden: ", true) + tmp; }
                    return string.Empty;
                }
                if (LinkedData.Item1 == null) { return LanguageTool.DoTranslate("Die Spalte ist in der Quell-Datenbank nicht vorhanden.", true); }
                // if (LinkedData.Item2 == null) { return LanguageTool.DoTranslate("Die Zeile ist in der Quell-Datenbank nicht vorhanden.", true); }
                return LanguageTool.DoTranslate("Die Zeile ist in der Quell-Datenbank nicht vorhanden.", true);
            }

            if (Row != null)
            {
                if (Row.Database != Column.Database) { return LanguageTool.DoTranslate("Interner Fehler: Bezug der Datenbank zur Zeile ist fehlerhaft.", true); }
                if (Column != Column.Database.Column.SysLocked && Row.CellGetBoolean(Column.Database.Column.SysLocked) && !Column.EditTrotzSperreErlaubt) { return LanguageTool.DoTranslate("Da die Zeile als abgeschlossen markiert ist, kann die Zelle nicht bearbeitet werden.", true); }
            }
            else
            {
                //Auf neue Zeile wird geprüft
                if (!Column.IsFirst()) { return LanguageTool.DoTranslate("Neue Zeilen müssen mit der ersten Spalte beginnen.", true); }
            }





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

        public void Set(ColumnItem Column, RowItem Row, int Value, bool FreezeMode)
        {
            Set(Column, Row, Value.ToString(), FreezeMode);
        }

        public void Set(ColumnItem Column, RowItem Row, int Value)
        {
            Set(Column, Row, Value.ToString(), false);
        }

        public void Set(ColumnItem Column, RowItem Row, double Value)
        {
            Set(Column, Row, Value.ToString(), false);
        }

        public void Set(ColumnItem Column, RowItem Row, double Value, bool FreezeMode)
        {
            Set(Column, Row, Value.ToString(), FreezeMode);
        }




        public double GetDouble(ColumnItem Column, RowItem Row)
        {
            var x = GetString(Column, Row);
            if (string.IsNullOrEmpty(x)) { return 0; }
            return double.Parse(x);
        }

        public Color GetColor(ColumnItem Column, RowItem Row)
        {
            return Color.FromArgb(GetInteger(Column, Row));
        }

        public int GetInteger(ColumnItem Column, RowItem Row)
        {
            var x = GetString(Column, Row);
            if (string.IsNullOrEmpty(x)) { return 0; }
            return int.Parse(x);
        }

        public decimal GetDecimal(ColumnItem Column, RowItem Row)
        {
            var x = GetString(Column, Row);
            if (string.IsNullOrEmpty(x)) { return 0; }
            return decimal.Parse(x);
        }

        public int GetColorBGR(ColumnItem Column, RowItem Row)
        {
            var c = GetColor(Column, Row);
            int colorBlue = c.B;
            int colorGreen = c.G;
            int colorRed = c.R;
            return (colorBlue << 16) | (colorGreen << 8) | colorRed;
        }

        public bool IsNullOrEmpty(string CellKey)
        {
            DataOfCellKey(CellKey, out var Column, out var Row);
            return IsNullOrEmpty(Column, Row);

        }


        internal Size ContentSizeToSave(KeyValuePair<string, CellItem> vCell, ColumnItem Column)
        {
            if (Column.Format.SaveSizeData())
            {

                if (vCell.Value.Size.Height > 4 &&
                    vCell.Value.Size.Height < 65025 &&
                    vCell.Value.Size.Width > 4 &&
                    vCell.Value.Size.Width < 65025) { return vCell.Value.Size; }
            }
            return Size.Empty;
        }

        public Bitmap GetBitmap(ColumnItem Column, RowItem Row)
        {
            return modConverter.StringToBitmap(GetString(Column, Row));
        }


        internal string CompareKey(ColumnItem Column, RowItem Row)
        {
            return DataFormat.CompareKey(GetString(Column, Row), Column.Format);
        }

        public void Set(string ColumnName, RowItem Row, string Value)
        {
            Set(Database.Column[ColumnName], Row, Value, false);
        }

        public void Set(string ColumnName, RowItem Row, string Value, bool FreezeMode)
        {
            Set(Database.Column[ColumnName], Row, Value, FreezeMode);
        }

        public List<string> GetList(string ColumnName, RowItem Row)
        {
            return GetList(Database.Column[ColumnName], Row);
        }

        public List<string> GetList(string Column, string ValueOfFirstColumn)
        {
            return GetList(Database.Column[Column], Database.Row[ValueOfFirstColumn]);
        }

        public decimal GetDecimal(string ColumnName, RowItem Row)
        {
            return GetDecimal(Database.Column[ColumnName], Row);
        }

        public int GetInteger(string ColumnName, RowItem Row)
        {
            return GetInteger(Database.Column[ColumnName], Row);
        }

        public bool IsNullOrEmpty(string ColumnName, RowItem Row)
        {
            return IsNullOrEmpty(Database.Column[ColumnName], Row);
        }

        public void Set(string ColumnName, RowItem Row, float Value)
        {
            Set(Database.Column[ColumnName], Row, Value);
        }

        public void Set(string ColumnName, RowItem Row, double Value)
        {
            Set(Database.Column[ColumnName], Row, Value);
        }

        public void Set(string ColumnName, RowItem Row, DateTime Value)
        {
            Set(Database.Column[ColumnName], Row, Value);
        }

        public DateTime GetDate(string ColumnName, RowItem Row)
        {
            return GetDate(Database.Column[ColumnName], Row);
        }

        internal void SaveToByteList(ref List<byte> l)
        {

            RemoveOrphans();

            foreach (var ThisString in _cells)
            {
                Database.SaveToByteList(l, ThisString);
            }
        }

        internal void OnCellValueChanged(CellEventArgs e)
        {
            e.Column._UcaseNamesSortedByLenght = null;
            CellValueChanged?.Invoke(this, e);
        }


        public List<string> ValuesReadable(ColumnItem Column, RowItem Row, enShortenStyle style)
        {
            return CellItem.ValuesReadable(Column, Row, style);
        }

        //public string GetStringForExport(string Column, RowItem Row)
        //{
        //    return GetStringForExport(Database.Column[Column], Row);
        //}

        public Size GetSizeOfCellContent(ColumnItem Column, RowItem Row)
        {
            var CellKey = KeyOfCell(Column, Row);
            if (_cells.ContainsKey(CellKey))
            {
                return Column.Database.Cell._cells[CellKey].Size;
            }
            return Size.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Column"></param>
        /// <param name="Row"></param>
        /// <param name="ContentSize">Wird im Scale der Datenbank gespeichert, da es ja Benutzerübergreifend ist</param>
        public void SetSizeOfCellContent(ColumnItem Column, RowItem Row, Size ContentSize)
        {
            var CellKey = KeyOfCell(Column, Row);
            if (!_cells.ContainsKey(CellKey)) { return; }

            _cells[CellKey].Size = ContentSize;
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
                    tmp = _cells[thisv.Key].Value;
                }
                else
                {
                    tmp = string.Empty;
                }

                if (tmp != thisv.Value)
                {
                    DataOfCellKey(thisv.Key, out var c, out var r);
                    if (c != Database.Column.SysRowChangeDate && c != Database.Column.SysRowChanger)
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
                            DataOfCellKey(thisv.Key, out var c, out var r);
                            if (c == Database.Column.SysRowChangeDate || c == Database.Column.SysRowChanger)
                            {
                                SystemSet(c, r, thisv.Value, true);
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
