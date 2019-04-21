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
        private ColumnViewItem _columview;
        private int _RowKey = -1;

        private bool _IsFilling;

        public ColumnViewItem ColumnViewItem
        {
            get
            {
                return _columview;
            }
            set
            {
                if (_columview == value) { return; }

                FillCellNow();

                if (_columview != null && _columview.Column != null)
                {
                    _columview.Column.Database.Cell.CellValueChanged -= Database_CellValueChanged;
                    _columview.Column.Database.Row.RowRemoved -= Database_RowRemoved;
                    _columview.Column.Database.Column.ItemInternalChanged -= Column_ItemInternalChanged;
                    _columview.Column.Database.ConnectedControlsStopAllWorking -= Database_ConnectedControlsStopAllWorking;
                    _columview.Column.Database.Row.RowChecked -= Database_RowChecked;
                    _columview.Column.Database.RowKeyChanged -= _Database_RowKeyChanged;
                }

                _columview = value;
                UpdateColumnData();

                if (_columview != null && _columview.Column != null)
                {
                    _columview.Column.Database.Cell.CellValueChanged += Database_CellValueChanged;
                    _columview.Column.Database.Row.RowRemoved += Database_RowRemoved;
                    _columview.Column.Database.Column.ItemInternalChanged += Column_ItemInternalChanged;
                    _columview.Column.Database.ConnectedControlsStopAllWorking += Database_ConnectedControlsStopAllWorking;
                    _columview.Column.Database.Row.RowChecked += Database_RowChecked;
                    _columview.Column.Database.RowKeyChanged += _Database_RowKeyChanged;
                    //V.Column.Database.ColumnKeyChanged += _Database_ColumnKeyChanged; // Columns sind als Objektverweis vermerkt
                }

                CheckEnabledState();
            }
        }


        public FlexiControlForCell() : this(null)
        {
            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();
        }

        public FlexiControlForCell(ColumnViewItem ColumnView)
        {
            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            Size = new Size(300, 300);
            ColumnViewItem = ColumnView;
        }

        private void UpdateColumnData()
        {
            if (_columview == null || _columview.Column == null)
            {
                CaptionPosition = enÜberschriftAnordnung.ohne;
                Caption = string.Empty;
                EditType = enEditTypeFormula.None;
                QuickInfo = string.Empty;
                FileEncryptionKey = string.Empty;
            }
            else
            {
                CaptionPosition = _columview.ÜberschriftAnordnung;
                Caption = _columview.Column.Caption;
                EditType = _columview.Column.EditType;
                QuickInfo = _columview.Column.QickInfoText(string.Empty);
                FileEncryptionKey = _columview.Column.Database.FileEncryptionKey;
            }
        }

        private void Database_RowChecked(object sender, RowCheckedEventArgs e)
        {
            if (e.Row != GetRow()) { return; }

            var NewT = string.Empty;

            foreach (var ThisString in e.ColumnsWithErrors)
            {

                var X = ThisString.SplitBy("|");
                if (_columview.Column != null && X[0].ToUpper() == _columview.Column.Name.ToUpper())
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


            if (e.Column == _columview.Column) { SetValueFromCell(e.Row); }

            CheckEnabledState();
        }

        private void SetValueFromCell(RowItem row)
        {


            if (_columview == null || _columview.Column == null || row == null)
            {
                Value = string.Empty;
                return;
            }



            switch (_columview.Column.Format)
            {
                case enDataFormat.Link_To_Filesystem:
                    var tmp = row.CellGetList(_columview.Column);
                    var tmp2 = new List<string>();
                    foreach (var file in tmp)
                    {
                        var tmpF = _columview.Column.BestFile(file, false);
                        if (FileExists(tmpF))
                        {
                            tmp2.Add(tmpF);
                        }
                        else
                        {
                            tmp2.Add(file);
                        }
                    }

                    Value = tmp2.JoinWithCr();


                    break;

                default:
                    Value = row.CellGetString(_columview.Column);
                    break;


            }



        }

        private void Database_ConnectedControlsStopAllWorking(object sender, System.EventArgs e)
        {
            FillCellNow();
        }

        private void Column_ItemInternalChanged(object sender, ListEventArgs e)
        {
            if ((ColumnItem)e.Item == _columview.Column)
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
        public void ChangeRowKeyTo(int newRowKey)
        {
            if (newRowKey == _RowKey) { return; }

            FillCellNow();


            _IsFilling = true;

            _RowKey = newRowKey;

            SetValueFromCell(GetRow());

            CheckEnabledState();

            _IsFilling = false;
        }



        internal void CheckEnabledState()
        {

            var Row = GetRow();
            if (Parent == null || !Parent.Enabled || _columview == null || _columview.Column == null || Row == null)
            {
                Enabled = false;
                return;
            }

            Enabled = Row.Database.Cell.UserEditPossible(_columview.Column, Row, false); // Rechteverwaltung einfliesen lassen
        }

        internal RowItem GetRow()
        {
            if (_columview == null || _columview.Column == null) { return null; }
            if (_RowKey < 0) { return null; }
            return _columview.Column.Database.Row.SearchByKey(_RowKey);
        }

        private void FillCellNow()
        {
            if (_IsFilling) { return; }

            if (!Enabled) { return; } // Versuch. Eigentlich darf das Steuerelement dann nur empfangen und nix ändern.

            var Row = GetRow();
            if (_columview == null || _columview.Column == null || Row == null) { return; }


            var OldVal = Row.CellGetString(_columview.Column);




            string NewValue = string.Empty;

            switch (_columview.Column.Format)
            {
                case enDataFormat.Link_To_Filesystem:
                    var tmp = Value.SplitByCRToList();
                    var tmp2 = new List<string>();

                    foreach (var file in tmp)
                    {
                        tmp2.Add(_columview.Column.SimplyFile(file));
                    }
                    NewValue = tmp2.JoinWithCr();
                    break;

                default:
                    NewValue = Value;
                    break;


            }





            if (OldVal == NewValue) { return; }

            Row.CellSet(_columview.Column, NewValue);
            if (OldVal != Row.CellGetString(_columview.Column)) { Row.DoAutomatic(false, false, false); }
        }


        private void textBox_NeedDatabaseOfAdditinalSpecialChars(object sender, DatabaseGiveBackEventArgs e)
        {
            var Row = GetRow();
            if (_columview == null || _columview.Column == null || Row == null) { return; }
            e.Database = _columview.Column.Database;
        }


        protected override void OnControlAdded(System.Windows.Forms.ControlEventArgs e)
        {
            base.OnControlAdded(e);


            switch (e.Control)
            {
                case ComboBox comboBox:


                    var Item2 = new ItemCollectionList();
                    ItemCollectionList.GetItemCollection(Item2, _columview.Column, null, enShortenStyle.Both, 10000);

                    if (_columview.Column.TextBearbeitungErlaubt)
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
                    StyleTextBox(textBox, _columview.Column.Format, _columview.Column.MultiLine, _columview.Column.AllowedChars, _columview.Column.SpellCheckingEnabled, _columview.Column.Suffix, false);
                    textBox.NeedDatabaseOfAdditinalSpecialChars += textBox_NeedDatabaseOfAdditinalSpecialChars;
                    textBox.GotFocus += GotFocus_TextBox;
                    textBox.TextChanged += TextBox_TextChanged;

                    break;

                case ListBox listBox:
                    if (listBox.Name == "Main")
                    {
                        StyleListBox(listBox, _columview.Column);
                    }
                    listBox.AddClicked += ListBox_AddClicked;
                    //listBox.NeedRow += ListBox_NeedRow;
                    break;


                case Button _:
                    break;

                case Caption _:
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

            if (_columview == null && _columview.Column == null) { return; }

            if (_columview.Column.Format != enDataFormat.RelationText) { return; }

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
                    textBox.NeedDatabaseOfAdditinalSpecialChars -= textBox_NeedDatabaseOfAdditinalSpecialChars;
                    textBox.GotFocus -= GotFocus_TextBox;
                    textBox.TextChanged -= TextBox_TextChanged;
                    break;

                case ListBox listBox:
                    listBox.AddClicked -= ListBox_AddClicked;
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
            if (_columview.Column != null) { e.Database = _columview.Column.Database; }
        }

        private void GotFocus_ComboBox(object sender, System.EventArgs e)
        {
            var Row = GetRow();
            if (_columview.Column == null || Row == null) { return; }
            if (!string.IsNullOrEmpty(((ComboBox)sender).Text)) { return; }



            Value = CellCollection.AutomaticInitalValue(_columview.Column, Row);   // TODO: SetValue(FromCell) benutzen

        }



        private void GotFocus_TextBox(object sender, System.EventArgs e)
        {
            var Row = GetRow();
            if (_columview.Column == null || Row == null) { return; }
            if (!string.IsNullOrEmpty(((TextBox)sender).Text)) { return; }


            Value = CellCollection.AutomaticInitalValue(_columview.Column, Row);   // TODO: SetValue(FromCell) benutzen

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

                case enEditTypeFormula.None:
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
                    if (_columview.Column == null && Row == null) { Develop.DebugPrint_NichtImplementiert(); }
                    if (_columview.Column.Format != enDataFormat.Link_To_Filesystem) { Develop.DebugPrint_NichtImplementiert(); }


                    switch (Control.SorceType)
                    {
                        case enSorceType.ScreenShot:
                            var fil = _columview.Column.BestFile(_columview.Column.Name + ".PNG", true);
                            Control.Bitmap.Save(fil, ImageFormat.Png);
                            Control.ChangeSource(fil, enSorceType.LoadedFromDisk, false);
                            Value = fil;   // Ruft rekursiv DoEasyPicValueChanged und springt zu LoadedFromDisk
                            return;

                        case enSorceType.Nichts:
                            Value = string.Empty;   // TODO: SetValue(FromCell) benutzen
                            FillCellNow();
                            return;

                        case enSorceType.LoadedFromDisk:
                            if (Control.SorceName != _columview.Column.SimplyFile(Control.SorceName))
                            {
                                // DEr name kann nur vereifacht werden, wenn es bereits im richtigen Verzeichniss ist. Name wird vereinfacht (ungleich) - bereits im richtigen verzeichniss!
                                Value = Control.SorceName;
                                FillCellNow();
                                return;
                            }



                            var fil2 = _columview.Column.BestFile(_columview.Column.Name + ".PNG", true);

                            if (fil2.FilePath().ToUpper() != Control.SorceName.FilePath().ToUpper())
                            {
                                Control.Bitmap.Save(fil2, ImageFormat.Png);
                            }
                            else
                            {
                                fil2 = Control.SorceName;
                            }
                            Control.ChangeSource(fil2, enSorceType.LoadedFromDisk, false);
                            Value = fil2;    // Ruft rekursiv DoEasyPicValueChanged und springt zu LoadedFromDisk
                            return;

                        case enSorceType.EntryWithoutPic:
                            Value = Control.SorceName;   // TODO: SetValue(FromCell) benutzen
                            // Entweder ein Dummy eintrag (Bildzeichen-Liste, wo Haupt das Bild sein sollte, aber eben nur bei den 3 Seitensichten eines da ist
                            // Oder datenbank wird von einem andern PC aus gestartet
                            return;


                    }
                }
            }
            Develop.DebugPrint_NichtImplementiert();
        }



        private void ListBox_AddClicked(object sender, System.EventArgs e)
        {

            var Dia = ColumnItem.UserEditDialogTypeInTable(_columview.Column, false);

            var lbx = (ListBox)sender;

            switch (Dia)
            {
                case enEditTypeTable.None:
                    return;

                case enEditTypeTable.FileHandling_InDateiSystem:
                    // korrektheit der Zelle bereits geprüft
                    if (_columview.Column != null && string.IsNullOrEmpty(lbx.LastFilePath)) { lbx.LastFilePath = _columview.Column.Database.Filename.FilePath(); }
                    var DelList = new List<string>();
                    using (var f = new System.Windows.Forms.OpenFileDialog())
                    {
                        f.CheckFileExists = true;
                        f.CheckPathExists = true;
                        f.Multiselect = true;
                        f.InitialDirectory = lbx.LastFilePath;
                        f.Title = "Datei(en) hinzufügen:";
                        f.ShowDialog();

                        if (f.FileNames == null || f.FileNames.Length == 0) { return; }


                        for (var z = 0 ; z <= f.FileNames.GetUpperBound(0) ; z++)
                        {
                            var b = modConverter.FileToByte(f.FileNames[z]);

                            if (!string.IsNullOrEmpty(_columview.Column.Database.FileEncryptionKey)) { b = modAllgemein.SimpleCrypt(b, _columview.Column.Database.FileEncryptionKey, 1); }

                            var neu = f.FileNames[z].FileNameWithSuffix();
                            neu = _columview.Column.BestFile(neu.FileNameWithSuffix(), true);
                            lbx.LastFilePath = f.FileNames[z].FilePath();

                            modConverter.ByteToFile(neu, b);
                            lbx.Item.Add(neu.FileNameWithSuffix(), _columview.Column, enShortenStyle.Replaced);

                            DelList.Add(f.FileNames[z]);
                        }
                    }

                    DialogBoxes.FileDialogs.DeleteFile(DelList, true);
                    return;

                case enEditTypeTable.Textfeld:
                    lbx.Add_Text();
                    return;

                case enEditTypeTable.Listbox:
                    lbx.Add_TextBySuggestion();
                    return;


                default:
                    Develop.DebugPrint(Dia);
                    return;
            }


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



        protected override void OnRemovingAll()
        {
            FillCellNow();
            base.OnRemovingAll();
        }

    }
}