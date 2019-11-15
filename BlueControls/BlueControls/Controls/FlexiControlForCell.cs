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
using System;
using BlueControls.Designer_Support;
using System.ComponentModel;

namespace BlueControls.Controls
{
    [Designer(typeof(BasicDesigner))]
    public partial class FlexiControlForCell : FlexiControl, IContextMenu
    {




        // Für automatisches Datenbank-Management
        private int _ColKey = -1;
        private int _RowKey = -1;
        private Database _Database = null;


        private ColumnItem _tmpColumn = null;
        private RowItem _tmpRow = null;



        public int RowKey
        {
            get
            {
                return _RowKey;
            }
            set
            {
                if (value == _RowKey) { return; }
                FillCellNow();

                _RowKey = value;
                GetTmpVariables();
                SetValueFromCell();
                CheckEnabledState();
            }
        }

        public int ColumnKey
        {
            get
            {
                return _ColKey;
            }
            set
            {
                if (value == _ColKey) { return; }
                FillCellNow();

                _ColKey = value;
                GetTmpVariables();
                UpdateColumnData();
                SetValueFromCell();

            }
        }

        public Database Database
        {
            get
            {
                return _Database;
            }
            set
            {
                if (value == _Database) { return; }

                FillCellNow();

                if (_Database != null)
                {
                    _Database.Cell.CellValueChanged -= Database_CellValueChanged;
                    _Database.Row.RowRemoved -= Database_RowRemoved;
                    _Database.Column.ItemInternalChanged -= Column_ItemInternalChanged;
                    _Database.ConnectedControlsStopAllWorking -= Database_ConnectedControlsStopAllWorking;
                    _Database.Row.RowChecked -= Database_RowChecked;
                    _Database.RowKeyChanged -= _Database_RowKeyChanged;
                    _Database.Loaded -= _Database_Loaded;
                }

                _Database = value;
                GetTmpVariables();
                UpdateColumnData();

                if (_Database != null)
                {
                    _Database.Cell.CellValueChanged += Database_CellValueChanged;
                    _Database.Row.RowRemoved += Database_RowRemoved;
                    _Database.Column.ItemInternalChanged += Column_ItemInternalChanged;
                    _Database.ConnectedControlsStopAllWorking += Database_ConnectedControlsStopAllWorking;
                    _Database.Row.RowChecked += Database_RowChecked;
                    _Database.RowKeyChanged += _Database_RowKeyChanged;
                    _Database.Loaded += _Database_Loaded;
                    //V.Column.Database.ColumnKeyChanged += _Database_ColumnKeyChanged; // Columns sind als Objektverweis vermerkt

                }

                CheckEnabledState();
            }
        }

        private void _Database_Loaded(object sender, LoadedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => _Database_Loaded(sender, e)));
                return;
            }

            UpdateColumnData();
            SetValueFromCell();
        }

        private void GetTmpVariables()
        {



            if (_Database != null)
            {
                _tmpColumn = _Database.Column.SearchByKey(_ColKey);
                _tmpRow = _Database.Row.SearchByKey(_RowKey);
            }
            else
            {
                _tmpColumn = null;
                _tmpRow = null;
            }

        }

        public FlexiControlForCell() : this(null, -1, enÜberschriftAnordnung.Über_dem_Feld)
        {
            // Dieser Aufruf ist für den Designer erforderlich.
            // InitializeComponent();
        }

        public FlexiControlForCell(Database database, int columnKey, enÜberschriftAnordnung captionPosition)
        {
            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            Size = new Size(300, 300);
            CaptionPosition = captionPosition;
            Database = database;
            ColumnKey = columnKey;
        }

        private void UpdateColumnData()
        {
            if (_tmpColumn == null)
            {
                Caption = string.Empty;
                EditType = enEditTypeFormula.None;
                QuickInfo = string.Empty;
                FileEncryptionKey = string.Empty;
            }
            else
            {
                Caption = _tmpColumn.Caption.Replace("\r", " ").Replace("\n", " ").Replace("  ", " ").TrimEnd(":") + ":";
                EditType = _tmpColumn.EditType;
                QuickInfo = _tmpColumn.QickInfoText(string.Empty);
                FileEncryptionKey = _Database.FileEncryptionKey;
            }
        }

        private void Database_RowChecked(object sender, RowCheckedEventArgs e)
        {
            if (e.Row != _tmpRow) { return; }

            var NewT = string.Empty;

            foreach (var ThisString in e.ColumnsWithErrors)
            {

                var X = ThisString.SplitBy("|");
                if (_tmpColumn != null && X[0].ToUpper() == _tmpColumn.Name.ToUpper())
                {
                    if (!string.IsNullOrEmpty(InfoText)) { InfoText = InfoText + "<br><hr><br>"; }
                    NewT = NewT + X[1];
                }

            }
            InfoText = NewT;
        }
        private void Database_RowRemoved(object sender, System.EventArgs e)
        {
            GetTmpVariables();
            RowKey = -1;
        }

        private void Database_CellValueChanged(object sender, CellEventArgs e)
        {
            if (e.Row != _tmpRow) { return; }


            if (e.Column == _tmpColumn)
            {
                SetValueFromCell();
                CheckEnabledState();
            }


            if (e.Column == e.Column.Database.Column.SysLocked)
            {
                CheckEnabledState();
            }

        }

        private void SetValueFromCell()
        {


            if (_tmpColumn == null || _tmpRow == null)
            {
                Value = string.Empty;
                InfoText = string.Empty;
                return;
            }



            switch (_tmpColumn.Format)
            {
                case enDataFormat.Link_To_Filesystem:
                    var tmp = _tmpRow.CellGetList(_tmpColumn);
                    var tmp2 = new List<string>();
                    foreach (var file in tmp)
                    {
                        var tmpF = _tmpColumn.BestFile(file, false);
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

                    if (Value.ToUpper() != tmp2.JoinWithCr().ToUpper())
                    {
                        Develop.DebugPrint(enFehlerArt.Warnung, "Werte ungleich: " + Value + " - " + tmp2.JoinWithCr());
                    }
                    break;

                default:
                    Value = _tmpRow.CellGetString(_tmpColumn);
                    break;


            }



        }

        private void Database_ConnectedControlsStopAllWorking(object sender, System.EventArgs e)
        {
            FillCellNow();
        }

        private void Column_ItemInternalChanged(object sender, ListEventArgs e)
        {
            if ((ColumnItem)e.Item == _tmpColumn)
            {
                UpdateColumnData();
                CheckEnabledState();
                OnNeedRefresh();
            }
        }





        internal void CheckEnabledState()
        {


            if (Parent == null || !Parent.Enabled || _tmpColumn == null || _tmpRow == null)
            {
                Enabled = false;
                return;
            }

            Enabled = CellCollection.UserEditPossible(_tmpColumn, _tmpRow, false); // Rechteverwaltung einfliesen lassen
        }


        private void FillCellNow()
        {
            if (_IsFilling) { return; }

            if (!Enabled) { return; } // Versuch. Eigentlich darf das Steuerelement dann nur empfangen und nix ändern.
            GetTmpVariables(); // Falls der Key inzwischen nicht mehr in der Collection ist, deswegen neu prüfen. RowREmoved greift zwar, kann aber durchaus erst nach RowSortesd/CursorposChanges auftreten.


            if (_tmpColumn == null || _tmpRow == null) { return; }

            var OldVal = _tmpRow.CellGetString(_tmpColumn);
            var NewValue = string.Empty;

            switch (_tmpColumn.Format)
            {
                case enDataFormat.Link_To_Filesystem:
                    var tmp = Value.SplitByCRToList();
                    var tmp2 = new List<string>();

                    foreach (var file in tmp)
                    {
                        tmp2.Add(_tmpColumn.SimplyFile(file));
                    }
                    NewValue = tmp2.JoinWithCr();
                    break;

                default:
                    NewValue = Value;
                    break;


            }

            if (OldVal == NewValue) { return; }

            _tmpRow.CellSet(_tmpColumn, NewValue);
            if (OldVal != _tmpRow.CellGetString(_tmpColumn)) { _tmpRow.DoAutomatic(false, false); }
        }


        private void textBox_NeedDatabaseOfAdditinalSpecialChars(object sender, DatabaseGiveBackEventArgs e)
        {
            e.Database = _Database;
        }


        protected override void OnControlAdded(System.Windows.Forms.ControlEventArgs e)
        {
            base.OnControlAdded(e);
            var column1 = _tmpColumn;
            if (column1.Format == enDataFormat.LinkedCell)
            {
                column1 = null;

                if (_tmpColumn.LinkedDatabase() != null && _tmpColumn.LinkedCell_ColumnKey > -1)
                {
                    column1 = _tmpColumn.LinkedDatabase().Column.SearchByKey(_tmpColumn.LinkedCell_ColumnKey);
                }

                if (column1 == null)
                {
                    Develop.DebugPrint("Column nicht gefunden");
                    return;
                }

            }


            Suffix = column1.Suffix;
            Format = column1.Format;

            switch (e.Control)
            {
                case ComboBox comboBox:
                    var Item2 = new ItemCollectionList();
                    ItemCollectionList.GetItemCollection(Item2, column1, null, enShortenStyle.Both, 10000);

                    if (column1.TextBearbeitungErlaubt)
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
                    easyPic.ImageChanged += EasyPicImageChanged;
                    break;

                case TextBox textBox:
                    StyleTextBox(textBox, column1.MultiLine, column1.AllowedChars, column1.SpellCheckingEnabled, false);
                    textBox.NeedDatabaseOfAdditinalSpecialChars += textBox_NeedDatabaseOfAdditinalSpecialChars;
                    textBox.GotFocus += GotFocus_TextBox;
                    textBox.TextChanged += TextBox_TextChanged;

                    break;

                case ListBox listBox:
                    if (listBox.Name == "Main")
                    {
                        StyleListBox(listBox, column1);
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

            if (_tmpColumn == null) { return; }

            if (_tmpColumn.Format != enDataFormat.RelationText) { return; }

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
                    easyPic.ImageChanged -= EasyPicImageChanged;
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



        private void EasyPicImageChanged(object sender, System.EventArgs e)
        {

            foreach (System.Windows.Forms.Control ThisControl in Controls)
            {

                if (ThisControl is EasyPic Control)
                {
                    if (_tmpColumn == null && _tmpRow == null) { Develop.DebugPrint_NichtImplementiert(); }
                    if (_tmpColumn.Format != enDataFormat.Link_To_Filesystem) { Develop.DebugPrint_NichtImplementiert(); }



                    switch (Control.SorceType)
                    {
                        case enSorceType.ScreenShot:
                            var fil = _tmpColumn.BestFile(_tmpColumn.Name + ".png", true);
                            Control.Bitmap.Save(fil, ImageFormat.Png);
                            Control.ChangeSource(fil, enSorceType.LoadedFromDisk, false);
                            Value = fil;   // Ruft rekursiv DoEasyPicValueChanged und springt zu LoadedFromDisk
                            return;

                        case enSorceType.Nichts:
                            Value = string.Empty;   // TODO: SetValue(FromCell) benutzen
                            FillCellNow();
                            return;

                        case enSorceType.LoadedFromDisk:
                            if (Control.SorceName != _tmpColumn.SimplyFile(Control.SorceName))
                            {
                                // DEr name kann nur vereifacht werden, wenn es bereits im richtigen Verzeichniss ist. Name wird vereinfacht (ungleich) - bereits im richtigen verzeichniss!
                                Value = Control.SorceName;
                                FillCellNow();
                                return;
                            }



                            var fil2 = _tmpColumn.BestFile(_tmpColumn.Name + ".png", true);

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


        private void EasyPicConnectedDatabase(object sender, DatabaseGiveBackEventArgs e)
        {
            e.Database = _Database;
        }

        private void GotFocus_ComboBox(object sender, System.EventArgs e)
        {
            if (_tmpColumn == null || _tmpRow == null) { return; }
            if (!string.IsNullOrEmpty(((ComboBox)sender).Text)) { return; }
            Value = CellCollection.AutomaticInitalValue(_tmpColumn, _tmpRow);   // TODO: SetValue(FromCell) benutzen
        }



        private void GotFocus_TextBox(object sender, System.EventArgs e)
        {
            if (_tmpColumn == null || _tmpRow == null) { return; }
            if (!string.IsNullOrEmpty(((TextBox)sender).Text)) { return; }


            Value = CellCollection.AutomaticInitalValue(_tmpColumn, _tmpRow);   // TODO: SetValue(FromCell) benutzen
        }




        protected override void OnValueChanged()
        {
            base.OnValueChanged();
            FillCellNow();
        }


        private void ListBox_AddClicked(object sender, System.EventArgs e)
        {

            var Dia = ColumnItem.UserEditDialogTypeInTable(_tmpColumn, false);

            var lbx = (ListBox)sender;

            switch (Dia)
            {
                case enEditTypeTable.None:
                    return;

                case enEditTypeTable.FileHandling_InDateiSystem:
                    // korrektheit der Zelle bereits geprüft
                    if (_tmpColumn != null && string.IsNullOrEmpty(lbx.LastFilePath)) { lbx.LastFilePath = _Database.Filename.FilePath(); }
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


                        for (var z = 0; z <= f.FileNames.GetUpperBound(0); z++)
                        {
                            var b = modConverter.FileToByte(f.FileNames[z]);

                            if (!string.IsNullOrEmpty(_Database.FileEncryptionKey)) { b = modAllgemein.SimpleCrypt(b, _Database.FileEncryptionKey, 1); }

                            var neu = f.FileNames[z].FileNameWithSuffix();
                            neu = _tmpColumn.BestFile(neu.FileNameWithSuffix(), true);
                            lbx.LastFilePath = f.FileNames[z].FilePath();

                            modConverter.ByteToFile(neu, b);
                            lbx.Item.Add(neu.FileNameWithSuffix(), _tmpColumn, enShortenStyle.Replaced);

                            DelList.Add(f.FileNames[z]);
                        }
                    }

                    Forms.FileDialogs.DeleteFile(DelList, true);
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



            if (_tmpRow == null) { return; }

            if (Marker.CancellationPending) { return; }
            var Names = new List<string>();
            Names.AddRange(_Database.Column[0].GetUcaseNamesSortedByLenght());
            if (Marker.CancellationPending) { return; }

            var myname = _tmpRow.CellFirstString().ToUpper();

            var InitT = TXB.Text;

            bool Ok;
            do
            {
                Ok = true;

                Marker.ReportProgress(0, new List<object> { TXB, "Unmark1" });
                Develop.DoEvents();
                if (Marker.CancellationPending || InitT != TXB.Text) { return; }

                Marker.ReportProgress(0, new List<object> { TXB, "Unmark2" });
                Develop.DoEvents();
                if (Marker.CancellationPending || InitT != TXB.Text) { return; }

                try
                {
                    foreach (var ThisWord in Names)
                    {
                        var cap = 0;
                        do
                        {
                            Develop.DoEvents();
                            if (Marker.CancellationPending || InitT != TXB.Text) { return; }
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
                    TXB.Mark(enMarkState.MyOwn, (int)x[2], (int)x[3]);
                    TXB.Invalidate();
                    break;

                case "Mark2":
                    TXB.Mark(enMarkState.Other, (int)x[2], (int)x[3]);
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