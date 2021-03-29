#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using static BlueBasics.FileOperations;

namespace BlueControls.Controls {
    [Designer(typeof(BasicDesigner))]
    public partial class FlexiControlForCell : FlexiControl, IContextMenu {



        #region Constructor
        public FlexiControlForCell() : this(null, -1, enÜberschriftAnordnung.Über_dem_Feld) { }

        public FlexiControlForCell(Database database, int columnKey, enÜberschriftAnordnung captionPosition) : base() {
            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            Size = new Size(300, 300);
            ShowInfoWhenDisabled = true;
            CaptionPosition = captionPosition;
            Database = database;
            ColumnKey = columnKey;
        }

        #endregion

        // Für automatisches Datenbank-Management
        private int _ColKey = -1;
        private int _RowKey = -1;
        private Database _Database = null;
        private string _ColumnName = string.Empty;


        private ColumnItem _tmpColumn = null;
        private RowItem _tmpRow = null;

        public event EventHandler<ContextMenuInitEventArgs> ContextMenuInit;
        public event EventHandler<ContextMenuItemClickedEventArgs> ContextMenuItemClicked;

        [Description("Dieses Feld kann für den Forms-Editor verwendet werden. Falls ein Key und ein Name befüllt sind, ist der Name führend.")]
        [DefaultValue("")]
        public string ColumnName {
            get => _ColumnName;
            set {
                if (_ColumnName == value) { return; }

                _ColumnName = value;
                GetTmpVariables();
                UpdateColumnData();
                SetValueFromCell();
            }
        }






        public int RowKey {
            get => _RowKey;
            set {
                if (value == _RowKey) { return; }
                FillCellNow();

                _RowKey = value;
                GetTmpVariables();
                SetValueFromCell();
                CheckEnabledState();
            }
        }


        [Description("Falls ein Key und ein Name befüllt sind, ist der Name führend.")]
        public int ColumnKey {
            //get {
            //    return _ColKey;
            //}
            set {
                if (value == _ColKey) { return; }
                FillCellNow();

                _ColKey = value;
                GetTmpVariables();
                UpdateColumnData();
                SetValueFromCell();

            }
        }

        public Database Database {
            get => _Database;
            set {
                if (value == _Database) { return; }

                FillCellNow();

                if (_Database != null) {
                    _Database.Cell.CellValueChanged -= Database_CellValueChanged;
                    //_Database.Row.RowRemoved -= Database_RowRemoved;
                    _Database.Row.RowRemoving -= Row_RowRemoving;
                    _Database.Column.ItemInternalChanged -= Column_ItemInternalChanged;
                    _Database.ConnectedControlsStopAllWorking -= Database_ConnectedControlsStopAllWorking;
                    _Database.Row.RowChecked -= Database_RowChecked;
                    _Database.RowKeyChanged -= _Database_RowKeyChanged;
                    _Database.ColumnKeyChanged -= _Database_ColumnKeyChanged;
                    _Database.Loaded -= _Database_Loaded;
                }

                _Database = value;
                GetTmpVariables();
                UpdateColumnData();

                if (_Database != null) {
                    _Database.Cell.CellValueChanged += Database_CellValueChanged;
                    //_Database.Row.RowRemoved += Database_RowRemoved;
                    _Database.Row.RowRemoving += Row_RowRemoving;
                    _Database.Column.ItemInternalChanged += Column_ItemInternalChanged;
                    _Database.ConnectedControlsStopAllWorking += Database_ConnectedControlsStopAllWorking;
                    _Database.Row.RowChecked += Database_RowChecked;
                    _Database.RowKeyChanged += _Database_RowKeyChanged;
                    _Database.Loaded += _Database_Loaded;
                    _Database.ColumnKeyChanged += _Database_ColumnKeyChanged;

                }

                SetValueFromCell();

                CheckEnabledState();
            }
        }

        private void Row_RowRemoving(object sender, RowEventArgs e) {
            if (e.Row.Key == _RowKey) {
                _RowKey = -1;
                GetTmpVariables();
            }

        }


        private void _Database_Loaded(object sender, LoadedEventArgs e) {
            if (InvokeRequired) {
                Invoke(new Action(() => _Database_Loaded(sender, e)));
                return;
            }

            UpdateColumnData();
            SetValueFromCell();
        }

        private void GetTmpVariables() {

            if (_Database != null) {
                if (!string.IsNullOrEmpty(_ColumnName)) {
                    _tmpColumn = _Database.Column[DataHolder.ColumnName(_ColumnName)];
                } else {
                    _tmpColumn = _Database.Column.SearchByKey(_ColKey);
                }
                _tmpRow = _Database.Row.SearchByKey(_RowKey);
            } else {
                _tmpColumn = null;
                _tmpRow = null;
            }

        }



        private void UpdateColumnData() {
            if (_tmpColumn == null) {

                if (string.IsNullOrEmpty(_ColumnName)) {
                    Caption = string.Empty;
                    EditType = enEditTypeFormula.None;
                    QuickInfo = string.Empty;
                    FileEncryptionKey = string.Empty;
                } else {
                    Caption = _ColumnName + ":";
                }
            } else {

                Caption = _tmpColumn.ReadableText() + ":";
                FileEncryptionKey = _Database.FileEncryptionKey;

                if (string.IsNullOrEmpty(_ColumnName)) {
                    EditType = _tmpColumn.EditType;
                    QuickInfo = _tmpColumn.QuickInfoText(string.Empty);
                } else {
                    _tmpColumn.EditType = EditType;
                    //_tmpColumn.Quickinfo = QuickInfo;
                }

            }
        }

        private void Database_RowChecked(object sender, RowCheckedEventArgs e) {
            if (e.Row != _tmpRow) { return; }

            var NewT = string.Empty;

            foreach (var ThisString in e.ColumnsWithErrors) {

                var X = ThisString.SplitBy("|");
                if (_tmpColumn != null && X[0].ToUpper() == _tmpColumn.Name.ToUpper()) {
                    if (!string.IsNullOrEmpty(InfoText)) { InfoText += "<br><hr><br>"; }
                    NewT += X[1];
                }

            }
            InfoText = NewT;
        }

        private void Database_CellValueChanged(object sender, CellEventArgs e) {
            if (e.Row != _tmpRow) { return; }


            if (e.Column == _tmpColumn) {
                SetValueFromCell();
            }


            if (e.Column == _tmpColumn || e.Column == e.Column.Database.Column.SysLocked) {
                CheckEnabledState();
            }

        }

        private void SetValueFromCell() {


            if (_tmpColumn == null || _tmpRow == null) {
                ValueSet(string.Empty, true, true);
                InfoText = string.Empty;
                return;
            }



            switch (_tmpColumn.Format) {
                case enDataFormat.Link_To_Filesystem:
                    var tmp = _tmpRow.CellGetList(_tmpColumn);
                    var tmp2 = new List<string>();
                    foreach (var file in tmp) {
                        var tmpF = _tmpColumn.BestFile(file, false);
                        if (FileExists(tmpF)) {
                            tmp2.Add(tmpF);
                        } else {
                            tmp2.Add(file);
                        }
                    }

                    ValueSet(tmp2.JoinWithCr(), true, true);

                    if (Value.ToUpper() != tmp2.JoinWithCr().ToUpper()) {
                        Develop.DebugPrint(enFehlerArt.Warnung, "Werte ungleich: " + Value + " - " + tmp2.JoinWithCr());
                    }
                    break;

                default:
                    ValueSet(_tmpRow.CellGetString(_tmpColumn), true, true);
                    break;
            }
        }

        private void Database_ConnectedControlsStopAllWorking(object sender, System.EventArgs e) {
            FillCellNow();
        }

        private void Column_ItemInternalChanged(object sender, ListEventArgs e) {
            if ((ColumnItem)e.Item == _tmpColumn) {
                UpdateColumnData();
                CheckEnabledState();
                OnNeedRefresh();
            }
        }





        internal void CheckEnabledState() {


            if (Parent == null || !Parent.Enabled || _tmpColumn == null || _tmpRow == null) {
                DisabledReason = "Bezug zur Zelle verloren.";
                return;
            }

            DisabledReason = CellCollection.ErrorReason(_tmpColumn, _tmpRow, enErrorReason.EditNormaly); // Rechteverwaltung einfliesen lassen.
        }


        private void FillCellNow() {
            if (_IsFilling) { return; }

            if (!Enabled) { return; } // Versuch. Eigentlich darf das Steuerelement dann nur empfangen und nix ändern.
            GetTmpVariables(); // Falls der Key inzwischen nicht mehr in der Collection ist, deswegen neu prüfen. RowREmoved greift zwar, kann aber durchaus erst nach RowSortesd/CursorposChanges auftreten.


            if (_tmpColumn == null || _tmpRow == null) { return; }

            var OldVal = _tmpRow.CellGetString(_tmpColumn);
            string NewValue;

            switch (_tmpColumn.Format) {
                case enDataFormat.Link_To_Filesystem:
                    var tmp = Value.SplitByCRToList();
                    var tmp2 = new List<string>();

                    foreach (var file in tmp) {
                        tmp2.Add(_tmpColumn.SimplyFile(file));
                    }
                    NewValue = tmp2.JoinWithCr();
                    break;

                default:
                    NewValue = Value;
                    break;
            }

            if (OldVal == NewValue) { return; }

            _tmpRow.Database.WaitEditable();

            _tmpRow.CellSet(_tmpColumn, NewValue);
            if (OldVal != _tmpRow.CellGetString(_tmpColumn)) { _tmpRow.DoAutomatic(false, false, 1, "value changed"); }
        }


        private void textBox_NeedDatabaseOfAdditinalSpecialChars(object sender, MultiUserFileGiveBackEventArgs e) {
            e.File = _Database;
        }

        protected override void OnControlAdded(System.Windows.Forms.ControlEventArgs e) {
            base.OnControlAdded(e);

            if (e.Control is Caption) { return; } // z.B. Info Caption


            var column1 = _tmpColumn;

            if (column1 == null) {
                //            Develop.DebugPrint("Column nicht gefunden"); 
                // Bei Steuerelementen, die manuell hinzugefügt werden
                return;
            }


            if (column1.Format == enDataFormat.LinkedCell) {
                column1 = null;

                if (_tmpColumn.LinkedDatabase() != null && _tmpColumn.LinkedCell_ColumnKey > -1) {
                    column1 = _tmpColumn.LinkedDatabase().Column.SearchByKey(_tmpColumn.LinkedCell_ColumnKey);
                }

                if (column1 == null) {
                    Develop.DebugPrint("Column nicht gefunden");
                    return;
                }

            }


            Suffix = column1.Suffix;
            Format = column1.Format;
            MultiLine = column1.MultiLine;

            switch (e.Control) {
                case ComboBox comboBox:
                    var Item2 = new ItemCollectionList();
                    ItemCollectionList.GetItemCollection(Item2, column1, null, enShortenStyle.Replaced, 10000);

                    if (column1.TextBearbeitungErlaubt) {
                        StyleComboBox(comboBox, Item2, System.Windows.Forms.ComboBoxStyle.DropDown);
                    } else {
                        StyleComboBox(comboBox, Item2, System.Windows.Forms.ComboBoxStyle.DropDownList);
                    }

                    comboBox.GotFocus += GotFocus_ComboBox;
                    break;

                case EasyPic easyPic:
                    easyPic.ConnectedDatabase += EasyPicConnectedDatabase;
                    easyPic.ImageChanged += EasyPicImageChanged;
                    break;

                case TextBox textBox:
                    StyleTextBox(textBox, column1.AllowedChars, column1.SpellCheckingEnabled);
                    textBox.NeedDatabaseOfAdditinalSpecialChars += textBox_NeedDatabaseOfAdditinalSpecialChars;
                    textBox.GotFocus += GotFocus_TextBox;
                    textBox.TextChanged += TextBox_TextChanged;

                    break;

                case ListBox listBox:
                    StyleListBox(listBox, column1);
                    listBox.ContextMenuInit += ListBox_ContextMenuInit;
                    listBox.ContextMenuItemClicked += ListBox_ContextMenuItemClicked;
                    listBox.AddClicked += ListBox_AddClicked;
                    break;

                case SwapListBox swapListBox:
                    StyleSwapListBox(swapListBox, column1);
                    //swapListBox.ContextMenuInit += ListBox_ContextMenuInit;
                    //swapListBox.ContextMenuItemClicked += ListBox_ContextMenuItemClicked;
                    swapListBox.AddClicked += ListBox_AddClicked;
                    break;

                case Button _:
                    break;

                //case Caption _:
                //    break;


                default:
                    Develop.DebugPrint("Control unbekannt");
                    break;
            }
        }


        private void TextBox_TextChanged(object sender, System.EventArgs e) {
            while (Marker.IsBusy) {
                if (!Marker.CancellationPending) { Marker.CancelAsync(); }
                Develop.DoEvents();
            }

            if (_tmpColumn == null) { return; }

            if (_tmpColumn.Format != enDataFormat.RelationText) { return; }

            Marker.RunWorkerAsync();
        }

        protected override void OnControlRemoved(System.Windows.Forms.ControlEventArgs e) {
            base.OnControlRemoved(e);

            switch (e.Control) {
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
                    listBox.ContextMenuInit -= ListBox_ContextMenuInit;
                    listBox.ContextMenuItemClicked -= ListBox_ContextMenuItemClicked;
                    listBox.AddClicked -= ListBox_AddClicked;
                    break;


                case SwapListBox swaplistBox:
                    //swaplistBox.ContextMenuInit -= ListBox_ContextMenuInit;
                    //swaplistBox.ContextMenuItemClicked -= ListBox_ContextMenuItemClicked;
                    swaplistBox.AddClicked -= ListBox_AddClicked;
                    break;


                case Caption _:
                    break;

                case Button _:
                    break;

                case Line _:
                    break;

                default:
                    Develop.DebugPrint("Control unbekannt");
                    break;


            }
        }



        private void EasyPicImageChanged(object sender, System.EventArgs e) {

            foreach (System.Windows.Forms.Control ThisControl in Controls) {

                if (ThisControl is EasyPic ep) {
                    if (_tmpColumn == null && _tmpRow == null) { Develop.DebugPrint_NichtImplementiert(); }
                    if (_tmpColumn.Format != enDataFormat.Link_To_Filesystem) { Develop.DebugPrint_NichtImplementiert(); }



                    switch (ep.SorceType) {
                        case enSorceType.ScreenShot:
                            var fil = _tmpColumn.BestFile(_tmpColumn.Name + ".png", true);
                            ep.Bitmap.Save(fil, ImageFormat.Png);
                            ep.ChangeSource(fil, enSorceType.LoadedFromDisk, false);

                            ValueSet(fil, false, true);
                            return;

                        case enSorceType.Nichts:
                            ValueSet(string.Empty, false, true);
                            return;

                        case enSorceType.LoadedFromDisk:
                            if (ep.SorceName != _tmpColumn.SimplyFile(ep.SorceName)) {
                                // DEr name kann nur vereifacht werden, wenn es bereits im richtigen Verzeichniss ist. Name wird vereinfacht (ungleich) - bereits im richtigen verzeichniss!
                                ValueSet(ep.SorceName, false, true);
                                return;
                            }



                            var fil2 = _tmpColumn.BestFile(_tmpColumn.Name + ".png", true);

                            if (fil2.FilePath().ToUpper() != ep.SorceName.FilePath().ToUpper()) {
                                ep.Bitmap.Save(fil2, ImageFormat.Png);
                            } else {
                                fil2 = ep.SorceName;
                            }
                            ep.ChangeSource(fil2, enSorceType.LoadedFromDisk, false);
                            ValueSet(fil2, false, true);
                            return;

                        case enSorceType.EntryWithoutPic:
                            ValueSet(ep.SorceName, false, true);
                            // Entweder ein Dummy eintrag (Bildzeichen-Liste, wo Haupt das Bild sein sollte, aber eben nur bei den 3 Seitensichten eines da ist
                            // Oder datenbank wird von einem andern PC aus gestartet
                            return;


                    }
                }
            }
            Develop.DebugPrint_NichtImplementiert();
        }


        private void EasyPicConnectedDatabase(object sender, MultiUserFileGiveBackEventArgs e) {
            e.File = _Database;
        }

        private void GotFocus_ComboBox(object sender, System.EventArgs e) {
            if (_tmpColumn == null || _tmpRow == null) { return; }
            if (!string.IsNullOrEmpty(((ComboBox)sender).Text)) { return; }
            ValueSet(CellCollection.AutomaticInitalValue(_tmpColumn, _tmpRow), true, true);
        }



        private void GotFocus_TextBox(object sender, System.EventArgs e) {
            if (_tmpColumn == null || _tmpRow == null) { return; }
            if (!string.IsNullOrEmpty(((TextBox)sender).Text)) { return; }
            ValueSet(CellCollection.AutomaticInitalValue(_tmpColumn, _tmpRow), true, true);
        }




        protected override void OnValueChanged() {
            base.OnValueChanged();
            FillCellNow();
        }


        private void ListBox_ContextMenuItemClicked(object sender, ContextMenuItemClickedEventArgs e) {

            switch (e.ClickedComand.ToLower()) {
                case "dateiöffnen":
                    if (e.HotItem is TextListItem t) {
                        if (FileExists(t.Internal)) {
                            var b = modConverter.FileToByte(t.Internal);
                            b = modAllgemein.SimpleCrypt(b, FileEncryptionKey, -1);
                            var tmp = TempFile(string.Empty, string.Empty, t.Internal.FileSuffix());
                            modConverter.ByteToFile(tmp, b);
                            ExecuteFile(tmp, null, true, false);
                            MessageBox.Show("Warte...");
                            DeleteFile(tmp, true);
                        }
                    }
                    break;

                case "bild öffnen":
                    if (e.HotItem is BitmapListItem bi) {
                        if (bi.ImageLoaded()) {
                            var x = new BlueControls.Forms.PictureView(bi.Bitmap);
                            x.Show();

                            //var b = modConverter.FileToByte(t.Internal);
                            //b = modAllgemein.SimpleCrypt(b, FileEncryptionKey, -1);
                            //var tmp = TempFile(string.Empty, string.Empty, t.Internal.FileSuffix());
                            //modConverter.ByteToFile(tmp, b);
                            //modAllgemein.ExecuteFile(tmp, null, true, false);
                            //MessageBox.Show("Warte...");
                            //DeleteFile(tmp, true);
                        }
                    }
                    break;

            }


        }

        private void ListBox_ContextMenuInit(object sender, ContextMenuInitEventArgs e) {

            if (e.HotItem is TextListItem t) {
                if (FileExists(t.Internal)) {
                    e.UserMenu.Add(enContextMenuComands.DateiÖffnen);
                }
            }

            if (e.HotItem is BitmapListItem) {
                //if (FileExists(t.Internal))
                //{
                e.UserMenu.Add("Bild öffnen");
                //}
            }
        }


        private void ListBox_AddClicked(object sender, System.EventArgs e) {

            var Dia = ColumnItem.UserEditDialogTypeInTable(_tmpColumn, false);

            var lbx = (ListBox)sender;

            switch (Dia) {
                case enEditTypeTable.None:
                    return;

                case enEditTypeTable.FileHandling_InDateiSystem:
                    // korrektheit der Zelle bereits geprüft
                    if (_tmpColumn != null && string.IsNullOrEmpty(lbx.LastFilePath)) { lbx.LastFilePath = _Database.Filename.FilePath(); }
                    var DelList = new List<string>();
                    using (var f = new System.Windows.Forms.OpenFileDialog()) {
                        f.CheckFileExists = true;
                        f.CheckPathExists = true;
                        f.Multiselect = true;
                        f.InitialDirectory = lbx.LastFilePath;
                        f.Title = "Datei(en) hinzufügen:";
                        f.ShowDialog();

                        if (f.FileNames == null || f.FileNames.Length == 0) { return; }


                        for (var z = 0; z <= f.FileNames.GetUpperBound(0); z++) {
                            var b = modConverter.FileToByte(f.FileNames[z]);

                            if (!string.IsNullOrEmpty(_Database.FileEncryptionKey)) { b = modAllgemein.SimpleCrypt(b, _Database.FileEncryptionKey, 1); }

                            var neu = f.FileNames[z].FileNameWithSuffix();
                            neu = _tmpColumn.BestFile(neu.FileNameWithSuffix(), true);
                            lbx.LastFilePath = f.FileNames[z].FilePath();

                            modConverter.ByteToFile(neu, b);
                            lbx.Item.Add(neu.FileNameWithSuffix(), _tmpColumn, enShortenStyle.Replaced, _tmpColumn.BildTextVerhalten);

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



        private void Marker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e) {
            TextBox TXB = null;

            foreach (var Control in Controls) {
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
            do {
                Ok = true;

                Marker.ReportProgress(0, new List<object> { TXB, "Unmark1" });
                Develop.DoEvents();
                if (Marker.CancellationPending || InitT != TXB.Text) { return; }

                Marker.ReportProgress(0, new List<object> { TXB, "Unmark2" });
                Develop.DoEvents();
                if (Marker.CancellationPending || InitT != TXB.Text) { return; }

                try {
                    foreach (var ThisWord in Names) {
                        var cap = 0;
                        do {
                            Develop.DoEvents();
                            if (Marker.CancellationPending || InitT != TXB.Text) { return; }
                            var fo = InitT.IndexOfWord(ThisWord, cap, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                            if (fo < 0) { break; }

                            if (ThisWord == myname) {
                                Marker.ReportProgress(0, new List<object> { TXB, "Mark1", fo, fo + ThisWord.Length - 1 });
                            } else {
                                Marker.ReportProgress(0, new List<object> { TXB, "Mark2", fo, fo + ThisWord.Length - 1 });
                            }
                            cap = fo + ThisWord.Length;

                        } while (true);
                    }


                } catch {
                    Ok = false;
                }

            } while (!Ok);


        }

        private void Marker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e) {
            //Ja, Multithreading ist kompliziert...
            if (Marker.CancellationPending) { return; }

            var x = (List<object>)e.UserState;
            var TXB = (TextBox)x[0];

            switch ((string)x[1]) {
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

        private void _Database_RowKeyChanged(object sender, KeyChangedEventArgs e) {
            if (e.KeyOld != _RowKey) { return; }
            _RowKey = e.KeyNew;
            GetTmpVariables();
            SetValueFromCell();
        }

        private void _Database_ColumnKeyChanged(object sender, KeyChangedEventArgs e) {
            if (e.KeyOld != _ColKey) { return; }
            _ColKey = e.KeyNew;
            GetTmpVariables();
            SetValueFromCell();
        }


        protected override void RemoveAll() {
            FillCellNow();
            base.RemoveAll();
            //base.OnRemovingAll();
        }

        public void GetContextMenuItems(MouseEventArgs e, ItemCollectionList Items, out object HotItem, List<string> Tags, ref bool Cancel, ref bool Translate) {

            GetTmpVariables();

            if (_tmpColumn != null && _tmpColumn.Database.IsAdministrator()) {
                Items.Add(enContextMenuComands.SpaltenEigenschaftenBearbeiten);
            }

            if (_tmpColumn != null && _tmpRow != null && _tmpColumn.Database.IsAdministrator()) {

                Items.Add(enContextMenuComands.VorherigenInhaltWiederherstellen);
            }



            if (Parent is Formula f) {

                var x = new ItemCollectionList(enBlueListBoxAppearance.KontextMenu);

                f.GetContextMenuItems(null, x, out var _, Tags, ref Cancel, ref Translate);

                if (x.Count > 0) {
                    if (Items.Count > 0) {
                        Items.AddSeparator();
                    }

                    Items.AddRange(x);
                }
            }

            HotItem = _tmpColumn;
        }

        public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e) {

            GetTmpVariables();

            //var CellKey = e.Tags.TagGet("CellKey");

            //if (string.IsNullOrEmpty(CellKey)) { return; }
            //TableView.Database.Cell.DataOfCellKey(CellKey, out var Column, out var Row);

            switch (e.ClickedComand.ToLower()) {
                case "spalteneigenschaftenbearbeiten":
                    tabAdministration.OpenColumnEditor(_tmpColumn, null);
                    return true;

                case "vorherigeninhaltwiederherstellen":

                    Table.DoUndo(_tmpColumn, _tmpRow);

                    return true;

                default:

                    if (Parent is Formula f) {
                        return f.ContextMenuItemClickedInternalProcessig(sender, e);
                    }
                    break;
            }

            return false;


        }

        public void OnContextMenuInit(ContextMenuInitEventArgs e) {
            ContextMenuInit?.Invoke(this, e);
        }

        public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) {
            ContextMenuItemClicked?.Invoke(this, e);
        }
    }
}