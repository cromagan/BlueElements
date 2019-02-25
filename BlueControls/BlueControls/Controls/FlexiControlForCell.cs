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
using System.Drawing;
using System.Drawing.Imaging;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using BlueDatabase;
using BlueDatabase.EventArgs;
using BlueDatabase.Enums;
using static BlueBasics.FileOperations;
using System.Collections.Generic;
using BlueControls.Enums;

namespace BlueControls.Controls
{
    public partial class FlexiControlForCell : FlexiControl, IContextMenu
    {




        // Für automatisches Datenbank-Management
        public readonly ColumnViewItem V;
        private int _RowKey = -1;

        private bool _IsFilling;

        public event EventHandler<DatabaseGiveBackEventArgs> NeedDatabaseOfAdditinalSpecialChars;


        public FlexiControlForCell(ColumnViewItem ColumnView)
        {
            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            Size = new Size(300, 300);
            V = ColumnView;



            _Caption = ColumnView.Column.Caption;
            _EditType = ColumnView.Column.EditType;
            _CaptionPosition = ColumnView.ÜberschriftAnordnung;
            Create_Control();




            V.Column.Database.Cell.CellValueChanged += Database_CellValueChanged;
            V.Column.Database.Row.RowRemoved += Database_RowRemoved;
            V.Column.Database.Column.ItemInternalChanged += Column_ItemInternalChanged;
            V.Column.Database.ConnectedControlsStopAllWorking += Database_ConnectedControlsStopAllWorking;
            V.Column.Database.Row.RowChecked += Database_RowChecked;
            V.Column.Database.RowKeyChanged += _Database_RowKeyChanged;
            //V.Column.Database.ColumnKeyChanged += _Database_ColumnKeyChanged; // Columns sind als Objektverweis vermerkt


            UpdateColumnData();
        }

        private void UpdateColumnData()
        {
            Caption = V.Column.Caption;
            EditType = V.Column.EditType;
            CaptionPosition = V.ÜberschriftAnordnung;
            QuickInfo = V.Column.QickInfoText(string.Empty);
        }

        private void Database_RowChecked(object sender, RowCheckedEventArgs e)
        {
            if (e.Row != GetRow()) { return; }

            var NewT = string.Empty;

            foreach (var ThisString in e.ColumnsWithErrors)
            {

                var X = ThisString.SplitBy("|");
                if (V.Column != null && X[0].ToUpper() == V.Column.Name.ToUpper())
                {
                    if (!string.IsNullOrEmpty(InfoText)) { InfoText = InfoText + "<br><hr><br>"; }
                    NewT = NewT + X[1];
                }

            }
            InfoText = NewT;
        }
        private void Database_RowRemoved(object sender, System.EventArgs e)
        {
            if (_RowKey >= 0 && GetRow() == null) { ChangeRowKeyTo(-1); }
        }

        private void Database_CellValueChanged(object sender, CellEventArgs e)
        {

            if (e.Row != GetRow()) { return; }


            if (e.Column == V.Column) { Value = e.Row.CellGetString(V.Column); }

            CheckEnabledState();
        }

        protected override void RemoveAll()
        {
            base.RemoveAll();

            if (V.Column != null)
            {
                V.Column.Database.Cell.CellValueChanged -= Database_CellValueChanged;
                V.Column.Database.Row.RowRemoved -= Database_RowRemoved;
                V.Column.Database.Column.ItemInternalChanged -= Column_ItemInternalChanged;
                V.Column.Database.ConnectedControlsStopAllWorking -= Database_ConnectedControlsStopAllWorking;
                V.Column.Database.Row.RowChecked -= Database_RowChecked;
            }

        }

        private void Database_ConnectedControlsStopAllWorking(object sender, System.EventArgs e)
        {
            FillCellNow();
        }

        private void Column_ItemInternalChanged(object sender, ListEventArgs e)
        {
            if ((ColumnItem)e.Item == V.Column)
            {
                UpdateColumnData();
                CheckEnabledState();
                OnNeedRefresh();

            }


        }

        /// <summary>
        /// Bewirkt, dass der Status in die Zelle zurückgeschrieben wird und anschließend wird der Zelleninhalt der neuen Zelle eingeschrieben.
        /// </summary>
        /// <param name="newRowKey"></param>
        internal void ChangeRowKeyTo(int newRowKey)
        {
            if (newRowKey == _RowKey) { return; }

            FillCellNow();


            _IsFilling = true;

            _RowKey = newRowKey;

            var Row = GetRow();


            if (V.Column == null || Row == null)
            {
                Value = string.Empty;
            }
            else
            {
                Value = Row.CellGetString(V.Column);
            }

            CheckEnabledState();

            _IsFilling = false;


            //TextBox_TextChanged(null, System.EventArgs.Empty);

            //Marker.RunWorkerAsync();
        }



        internal void CheckEnabledState()
        {

            var Row = GetRow();
            if (Parent == null || !Parent.Enabled || V.Column == null || Row == null)
            {
                Enabled = false;
                return;
            }

            Enabled = Row.Database.Cell.UserEditPossible(V.Column, Row, false); // Rechteverwaltung einfliesen lassen
        }

        internal RowItem GetRow()
        {
            if (V.Column == null) { return null; }
            if (_RowKey < 0) { return null; }
            return V.Column.Database.Row.SearchByKey(_RowKey);
        }

        private void FillCellNow()
        {
            if (_IsFilling) { return; }

            if (!Enabled) { return; } // Versuch. Eigentlich darf das Steuerelement dann nur empfangen und nix ändern.

            var Row = GetRow();
            if (V.Column == null || Row == null) { return; }


            var OldVal = Row.CellGetString(V.Column);

            if (OldVal == Value) { return; }

            Row.CellSet(V.Column, Value);
            if (OldVal != Row.CellGetString(V.Column)) { Row.DoAutomatic(false, false); }
        }


        private void OnNeedDatabaseOfAdditinalSpecialChars(object sender, DatabaseGiveBackEventArgs e)
        {
            NeedDatabaseOfAdditinalSpecialChars?.Invoke(this, e);
        }


        protected override void OnControlAdded(System.Windows.Forms.ControlEventArgs e)
        {
            base.OnControlAdded(e);


            switch (e.Control)
            {
                case ComboBox comboBox:


                    var Item2 = new ItemCollectionList();
                    ItemCollectionList.GetItemCollection(Item2, V.Column, null, enShortenStyle.Both, 10000);

                    if (V.Column.TextBearbeitungErlaubt)
                    {
                        StyleComboBox(comboBox, Item2, System.Windows.Forms.ComboBoxStyle.DropDown);
                    }
                    else
                    {
                        StyleComboBox(comboBox, Item2, System.Windows.Forms.ComboBoxStyle.DropDownList);
                    }

                    comboBox.GotFocus += GotFocus_ComboBox;
                    break;

                case EasyPic easyPic:
                    easyPic.ConnectedDatabase += EasyPicConnectedDatabase;
                    easyPic.ImageChanged += EasyPicImageChanged2;
                    break;

                case TextBox textBox:
                    StyleTextBox(textBox, V.Column.Format, V.Column.MultiLine, V.Column.AllowedChars, V.Column.SpellCheckingEnabled, V.Column.Suffix, false);
                    textBox.NeedDatabaseOfAdditinalSpecialChars += OnNeedDatabaseOfAdditinalSpecialChars;
                    textBox.GotFocus += GotFocus_TextBox;
                    textBox.TextChanged += TextBox_TextChanged;

                    break;

                case ListBox listBox:
                    if (listBox.Name == "Main")
                    {
                        StyleListBox(listBox, V.Column);
                    }
                    listBox.NeedColumn += ListBox_NeedColumn;
                    //listBox.NeedRow += ListBox_NeedRow;
                    break;


                case Button button:
                    break;

                case Caption caption:
                    break;


                default:
                    Develop.DebugPrint(enFehlerArt.Fehler, "Control unbekannt");
                    break;


            }


        }

        private void TextBox_TextChanged(object sender, System.EventArgs e)
        {
            while (Marker.IsBusy)
            {
                if (!Marker.CancellationPending) { Marker.CancelAsync(); }
                Develop.DoEvents();
            }

            if (V == null && V.Column == null) { return; }

            if (V.Column.Format != enDataFormat.RelationText) { return; }

            Marker.RunWorkerAsync();
        }

        protected override void OnControlRemoved(System.Windows.Forms.ControlEventArgs e)
        {
            base.OnControlRemoved(e);

            switch (e.Control)
            {
                case ComboBox comboBox:
                    comboBox.GotFocus -= GotFocus_ComboBox;
                    break;

                case EasyPic easyPic:
                    easyPic.ConnectedDatabase -= EasyPicConnectedDatabase;
                    easyPic.ImageChanged -= EasyPicImageChanged2;
                    break;

                case TextBox textBox:
                    textBox.NeedDatabaseOfAdditinalSpecialChars -= OnNeedDatabaseOfAdditinalSpecialChars;
                    textBox.GotFocus -= GotFocus_TextBox;
                    textBox.TextChanged -= TextBox_TextChanged;
                    break;

                case ListBox listBox:
                    listBox.NeedColumn -= ListBox_NeedColumn;
                    //listBox.NeedRow -= ListBox_NeedRow;
                    break;

                case Caption _:
                    break;

                case Button _:
                    break;

                default:
                    Develop.DebugPrint(enFehlerArt.Fehler, "Control unbekannt");
                    break;


            }
        }



        private void EasyPicImageChanged2(object sender, System.EventArgs e)
        {
            DoEasyPicValueChanged();
        }


        private void EasyPicConnectedDatabase(object sender, DatabaseGiveBackEventArgs e)
        {
            if (V.Column != null) { e.Database = V.Column.Database; }
        }

        private void GotFocus_ComboBox(object sender, System.EventArgs e)
        {
            var Row = GetRow();
            if (V.Column == null || Row == null) { return; }
            if (!string.IsNullOrEmpty(((ComboBox)sender).Text)) { return; }


            Value = V.Column.Database.Cell.AutomaticInitalValue(V.Column, Row);

        }



        private void GotFocus_TextBox(object sender, System.EventArgs e)
        {
            var Row = GetRow();
            if (V.Column == null || Row == null) { return; }
            if (!string.IsNullOrEmpty(((TextBox)sender).Text)) { return; }


            Value = V.Column.Database.Cell.AutomaticInitalValue(V.Column, Row);

        }




        protected override void OnValueChanged()
        {
            base.OnValueChanged();
            FillCellNow();


            switch (EditType)
            {
                case enEditTypeFormula.EasyPic:
                    DoEasyPicValueChanged();
                    break;

                case enEditTypeFormula.Gallery:
                case enEditTypeFormula.Listbox_1_Zeile:
                case enEditTypeFormula.Listbox_3_Zeilen:
                case enEditTypeFormula.Listbox_6_Zeilen:
                    DoListBoxValueChanged();
                    break;

                case enEditTypeFormula.Textfeld_mit_Auswahlknopf:
                    break;

                case enEditTypeFormula.Textfeld:
                    break;

                case enEditTypeFormula.nur_als_Text_anzeigen:
                    break;

                case enEditTypeFormula.Ja_Nein_Knopf:
                    break;

                default:
                    Develop.DebugPrint_NichtImplementiert();
                    break;


            }

        }


        private void DoListBoxValueChanged()
        {
            //ListBoxen(out ListBox Main, out ListBox Suggest);
            //RowItem Row = GetRow();

            //if (V.Column is null || Row is null || DataFormat.TextboxEditPossible(V.Column.Format))
            //{

            //    // Items von der Auswahl zurückspielen zu den Suggests
            //    if (Suggest != null)
            //    {
            //        while (Main.Item.Count > 0)
            //        {
            //            MoveItemBetweenList(Main, Suggest, Main.Item[0].Internal());
            //        }
            //    }

            //    // Items zum Haupt hinzufügen
            //    List<string> Vals = new List<string>(Value.SplitByCR());
            //    foreach (string ThisString in Vals)
            //    {
            //        MoveItemBetweenList(Suggest, Main, ThisString);
            //    }

            //    return;
            //}
            //else if (V.Column.Format.TextboxEditPossible())
            //{
            //    Main.Item.Clear();
            //    Main.Item.AddRange(Row.CellGetList(V.Column));
            //}
            //else if (V.Column.Format == enDataFormat.Link_To_Filesystem)
            //{
            //    Main.Item.Clear();
            //    //' Erst mal Ohne Bestfile. Weil ansonsten beim Rückspeichern der komplette Pfad rückgespeichert wird
            //    Main.Item.AddRange(Row.CellGetList(V.Column), V.Column, enShortenStyletyle.Replaced);
            //}
            //else if (V.Column.Format == enDataFormat.Relation)
            //{
            //    Main.Item.Clear();
            //    List<string> w = Row.CellGetList(V.Column);
            //    w.QuickSortAndRemoveDouble();
            //    for (int z = 0 ; z < w.Count ; z++)
            //    {
            //        Main.Item.Add(new ObjectListItem(new clsRelation(V.Column, Row, w[z])));
            //    }
            //}
            //else
            //{
            //    DebugPrint(enFehlerArt.Fehler, "Unbekannte Methode");
            //}


            //Main.Item.QuickSort();
        }

        /// <summary>
        /// Sucht passendes Bild oder setzt den Value in die Datebank.
        /// </summary>
        private void DoEasyPicValueChanged()
        {
            foreach (System.Windows.Forms.Control ThisControl in Controls)
            {

                if (ThisControl is EasyPic Control)
                {

                    var Row = GetRow();
                    if (V.Column == null && Row == null) { Develop.DebugPrint_NichtImplementiert(); }
                    if (V.Column.Format != enDataFormat.Link_To_Filesystem) { Develop.DebugPrint_NichtImplementiert(); }


                    switch (Control.SorceType)
                    {
                        case EasyPic.enSorceType.SourceNameCorrectButImageNotLoaded:
                            Control.LoadFromDatabase(V.Column);
                            return;

                        case EasyPic.enSorceType.ScreenShot:
                            var fil = TempFile(V.Column.BestFile(V.Column.Name + ".PNG"));
                            Control.Bitmap.Save(fil, ImageFormat.Png);
                            Control.ChangeSource(fil.FileNameWithSuffix(), EasyPic.enSorceType.DatabaseInternal);
                            Value = fil.FileNameWithSuffix();
                            return;

                        case EasyPic.enSorceType.DatabaseInternal:
                            // Bild geladen, evtl. noch die ZElle richtig stellen (Siehe .Screenshot). Muss deswegen sein, weil Database Internal auch woanders herkommen kann.
                            FillCellNow();
                            return;

                        case EasyPic.enSorceType.Nichts:
                            Value = string.Empty;
                            FillCellNow();
                            return;

                        case EasyPic.enSorceType.LoadedFromDisk:
                            var fil2 = TempFile(V.Column.BestFile(V.Column.Name + ".PNG"));

                            if (fil2.FilePath().ToUpper() != Control.SorceName.FilePath().ToUpper())
                            {
                                Control.Bitmap.Save(fil2, ImageFormat.Png);
                            }
                            else
                            {
                                fil2 = Control.SorceName;
                            }
                            Control.ChangeSource(fil2.FileNameWithSuffix(), EasyPic.enSorceType.DatabaseInternal);
                            Value = fil2.FileNameWithSuffix();
                            return;

                        case EasyPic.enSorceType.EntryWithoutPic:
                            Value = Control.SorceName;
                            // Entweder ein Dummy eintrag (Bildzeichen-Liste, wo Haupt das Bild sein sollte, aber eben nur bei den 3 Seitensichten eines da ist
                            // Oder datenbank wird von einem andern PC aus gestartet
                            return;


                    }
                }
            }
            Develop.DebugPrint_NichtImplementiert();
        }


        //private void ListBox_NeedRow(object sender, RowEventArgs e)
        //{
        //    e.Row = GetRow();
        //}

        private void ListBox_NeedColumn(object sender, ColumnEventArgs e)
        {
            e.Column = V.Column;
        }

        private void Marker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            TextBox TXB = null;

            foreach (var Control in Controls)
            {
                if (Control is TextBox t) { TXB = t; }
            }

            if (Marker.CancellationPending) { return; }
            if (TXB == null) { return; }


            var R = GetRow();
            if (R == null) { return; }

            if (Marker.CancellationPending) { return; }
            var Names = new List<string>();
            Names.AddRange(R.Database.Column[0].GetUcaseNamesSortedByLenght());
            if (Marker.CancellationPending) { return; }

            var myname = R.CellFirstString().ToUpper();

            var InitT = TXB.Text;

            bool Ok;
            do
            {
                Ok = true;

                Marker.ReportProgress(0, new List<object> { TXB, "Unmark1" });
                Develop.DoEvents();
                if (Marker.CancellationPending || InitT != TXB.Text || R != GetRow()) { return; }

                Marker.ReportProgress(0, new List<object> { TXB, "Unmark2" });
                Develop.DoEvents();
                if (Marker.CancellationPending || InitT != TXB.Text || R != GetRow()) { return; }

                try
                {
                    foreach (var ThisWord in Names)
                    {
                        var cap = 0;
                        do
                        {
                            Develop.DoEvents();
                            if (Marker.CancellationPending || InitT != TXB.Text || R != GetRow()) { return; }
                            var fo = InitT.IndexOfWord(ThisWord, cap, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                            if (fo < 0) { break; }

                            if (ThisWord == myname)
                            {
                                Marker.ReportProgress(0, new List<object> { TXB, "Mark1", fo, fo + ThisWord.Length - 1 });
                            }
                            else
                            {
                                Marker.ReportProgress(0, new List<object> { TXB, "Mark2", fo, fo + ThisWord.Length - 1 });
                            }
                            cap = fo + ThisWord.Length;

                        } while (true);



                    }

                    //for (var cap = 0 ; cap < TXB.Text.Length ; cap++)
                    //{
                    //    foreach (var ThisWord in Names)
                    //    {
                    //        if (TXB.WordStarts(ThisWord, cap))
                    //        {
                    //            Develop.DoEvents();
                    //            if (Marker.CancellationPending || InitT != TXB.Text || R != GetRow()) { return; }


                    //            if (ThisWord == myname)
                    //            {
                    //                Marker.ReportProgress(0, new List<object> { TXB, "Mark1", cap, cap + ThisWord.Length - 1 });
                    //            }
                    //            else
                    //            {
                    //                Marker.ReportProgress(0, new List<object> { TXB, "Mark2", cap, cap + ThisWord.Length - 1 });

                    //            }
                    //            cap += ThisWord.Length - 1; // -1 eigentlich überflüssig, weil es ja immer ein Leerzeichen sein sollte, was als nächstes kommt
                    //            break;

                    //        }
                    //    }
                    //}
                }
                catch
                {
                    Ok = false;
                }

            } while (!Ok);


        }

        private void Marker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            //Ja, Multithreading ist kompliziert...
            if (Marker.CancellationPending) { return; }

            var x = (List<object>)e.UserState;
            var TXB = (TextBox)x[0];

            switch ((string)x[1])
            {
                case "Unmark1":
                    TXB.Unmark(enMarkState.MyOwn);
                    TXB.Invalidate();
                    break;

                case "Unmark2":
                    TXB.Unmark(enMarkState.Other);
                    TXB.Invalidate();
                    break;

                case "Mark1":
                    TXB.Mark(enMarkState.MyOwn, (int)x[2], (int)(x[3]));
                    TXB.Invalidate();
                    break;

                case "Mark2":
                    TXB.Mark(enMarkState.Other, (int)x[2], (int)(x[3]));
                    TXB.Invalidate();
                    break;



                default:
                    Develop.DebugPrint((string)x[1]);
                    break;
            }

        }

        private void _Database_RowKeyChanged(object sender, KeyChangedEventArgs e)
        {
            if (e.KeyOld != _RowKey) { return; }
            _RowKey = e.KeyNew;
        }

    }
}