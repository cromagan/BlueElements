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

#nullable enable

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using BlueControls.ItemCollection.ItemCollectionList;
using static BlueBasics.FileOperations;
using MessageBox = System.Windows.Forms.MessageBox;
using BlueDatabase.Interfaces;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public partial class FlexiControlForCell : FlexiControl, IContextMenu, IAcceptRowKey, IDisabledReason {

    #region Fields

    // Für automatisches Datenbank-Management
    private long _colKey = -1;

    private string _columnName = string.Empty;

    private Database? _database;

    private long _rowKey = -1;

    private ColumnItem? _tmpColumn;

    private RowItem? _tmpRow;

    #endregion

    #region Constructors

    // Für den Designer
    public FlexiControlForCell() : this(null, -1, ÜberschriftAnordnung.Über_dem_Feld, EditTypeFormula.None) { }

    public FlexiControlForCell(Database? database, long columnKey, ÜberschriftAnordnung captionPosition, EditTypeFormula editType) : base() {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        Size = new Size(300, 300);
        ShowInfoWhenDisabled = true;
        CaptionPosition = captionPosition;
        EditType = editType;
        Database = database;
        ColumnKey = columnKey;
        CheckEnabledState();
    }

    #endregion

    #region Events

    public event EventHandler<ContextMenuInitEventArgs> ContextMenuInit;

    public event EventHandler<ContextMenuItemClickedEventArgs> ContextMenuItemClicked;

    #endregion

    #region Properties

    [Description("Falls ein Key und ein Name befüllt sind, ist der Name führend.")]
    public long ColumnKey {
        //get {
        //    return _ColKey;
        //}
        set {
            if (value == _colKey) { return; }
            FillCellNow();
            _colKey = value;
            GetTmpVariables();
            UpdateColumnData();
            SetValueFromCell();
        }
    }

    [Description("Dieses Feld kann für den Forms-Editor verwendet werden. Falls ein Key und ein Name befüllt sind, ist der Name führend.")]
    [DefaultValue("")]
    public string ColumnName {
        get => _columnName;
        set {
            if (_columnName == value) { return; }
            _columnName = value;
            GetTmpVariables();
            UpdateColumnData();
            SetValueFromCell();
        }
    }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Database? Database {
        get => _database;
        set {
            if (value == _database) { return; }
            FillCellNow();
            if (_database != null) {
                _database.Cell.CellValueChanged -= Database_CellValueChanged;
                _database.Row.RowRemoving -= Row_RowRemoving;
                _database.Column.ItemInternalChanged -= Column_ItemInternalChanged;
                _database.ConnectedControlsStopAllWorking -= Database_ConnectedControlsStopAllWorking;
                _database.Row.RowChecked -= Database_RowChecked;
                //_Database.RowKeyChanged -= _Database_RowKeyChanged;
                //_Database.ColumnKeyChanged -= _Database_ColumnKeyChanged;
                _database.Loaded -= _Database_Loaded;
                _database.Disposing -= _Database_Disposing;
            }
            _database = value;
            GetTmpVariables();
            UpdateColumnData();
            if (_database != null) {
                _database.Cell.CellValueChanged += Database_CellValueChanged;
                //_Database.Row.RowRemoved += Database_RowRemoved;
                _database.Row.RowRemoving += Row_RowRemoving;
                _database.Column.ItemInternalChanged += Column_ItemInternalChanged;
                _database.ConnectedControlsStopAllWorking += Database_ConnectedControlsStopAllWorking;
                _database.Row.RowChecked += Database_RowChecked;
                //_Database.RowKeyChanged += _Database_RowKeyChanged;
                _database.Loaded += _Database_Loaded;
                //_Database.ColumnKeyChanged += _Database_ColumnKeyChanged;
                _database.Disposing += _Database_Disposing;
            }
            SetValueFromCell();
            CheckEnabledState();
        }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public long RowKey {
        get => _rowKey;
        set {
            if (value == _rowKey) { return; }
            FillCellNow();
            _rowKey = value;
            GetTmpVariables();
            SetValueFromCell();
            CheckEnabledState();
        }
    }

    #endregion

    #region Methods

    public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e) {
        GetTmpVariables();
        //var CellKey = e.Tags.TagGet("CellKey");
        //if (string.IsNullOrEmpty(CellKey)) { return; }
        //TableView.Database.Cell.DataOfCellKey(CellKey, out var Column, out var Row);
        switch (e.ClickedComand.ToLower()) {
            case "spalteneigenschaftenbearbeiten":
                Forms.TableView.OpenColumnEditor(_tmpColumn, null);
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

    public void GetContextMenuItems(MouseEventArgs? e, ItemCollectionList? items, out object? hotItem, List<string> tags, ref bool cancel, ref bool translate) {
        GetTmpVariables();
        if (_tmpColumn != null && _tmpColumn.Database.IsAdministrator()) {
            items.Add(ContextMenuComands.SpaltenEigenschaftenBearbeiten);
        }
        if (_tmpColumn != null && _tmpRow != null && _tmpColumn.Database.IsAdministrator()) {
            items.Add(ContextMenuComands.VorherigenInhaltWiederherstellen);
        }
        if (Parent is Formula f) {
            ItemCollectionList x = new(BlueListBoxAppearance.KontextMenu);
            f.GetContextMenuItems(null, x, out _, tags, ref cancel, ref translate);
            if (x.Count > 0) {
                if (items.Count > 0) {
                    items.AddSeparator();
                }
                items.AddClonesFrom(x);
            }
        }
        hotItem = _tmpColumn;
    }

    public void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);

    public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) => ContextMenuItemClicked?.Invoke(this, e);

    internal void CheckEnabledState() {
        if (Parent == null || !Parent.Enabled || _tmpColumn == null || _tmpRow == null) {
            DisabledReason = "Kein Bezug zu einer Zelle.";
            return;
        }
        DisabledReason = CellCollection.ErrorReason(_tmpColumn, _tmpRow, ErrorReason.EditNormaly); // Rechteverwaltung einfliesen lassen.
    }

    protected override void OnControlAdded(ControlEventArgs e) {
        base.OnControlAdded(e);
        if (e.Control is Caption) { return; } // z.B. Info Caption

        var column1 = GetRealColumn(_tmpColumn, null);

        if (column1 != null) {
            Suffix = column1.Suffix;
            Regex = column1.Regex;
            AllowedChars = column1.AllowedChars;
        }

        switch (e.Control) {
            case ComboBox comboBox:
                ItemCollectionList item2 = new();
                ItemCollectionList.GetItemCollection(item2, column1, null, ShortenStyle.Replaced, 10000);
                if (column1 != null && column1.TextBearbeitungErlaubt) {
                    StyleComboBox(comboBox, item2, ComboBoxStyle.DropDown, false);
                } else {
                    StyleComboBox(comboBox, item2, ComboBoxStyle.DropDownList, true);
                }
                comboBox.GotFocus += GotFocus_ComboBox;
                break;

            //case EasyPic easyPic:
            //    easyPic.ConnectedDatabase += EasyPicConnectedDatabase;
            //    easyPic.ImageChanged += EasyPicImageChanged;
            //    break;

            case TextBox textBox:
                if (column1 == null) {
                    StyleTextBox(textBox);
                } else {
                    StyleTextBox(textBox);
                }
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

            case Button:
            case Line:
                break;
            //case Caption _:
            //    break;
            default:
                Develop.DebugPrint("Control unbekannt");
                break;
        }
    }

    protected override void OnControlRemoved(ControlEventArgs e) {
        base.OnControlRemoved(e);
        switch (e.Control) {
            case ComboBox comboBox:
                comboBox.GotFocus -= GotFocus_ComboBox;
                break;

            //case EasyPic easyPic:
            //    easyPic.ConnectedDatabase -= EasyPicConnectedDatabase;
            //    easyPic.ImageChanged -= EasyPicImageChanged;
            //    break;

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

            case Button:
                break;

            case Line:
                break;

            default:
                Develop.DebugPrint("Control unbekannt");
                break;
        }
    }

    protected override void OnValueChanged() {
        base.OnValueChanged();
        FillCellNow();
    }

    protected override void RemoveAll() {
        FillCellNow();
        base.RemoveAll();
        //base.OnRemovingAll();
    }

    //private void _Database_ColumnKeyChanged(object sender, KeyChangedEventArgs e) {
    //    if (e.KeyOld != _ColKey) { return; }
    //    _ColKey = e.KeyNew;
    //    GetTmpVariables();
    //    SetValueFromCell();
    //}

    private static void ListBox_ContextMenuInit(object sender, ContextMenuInitEventArgs e) {
        if (e.HotItem is TextListItem t) {
            if (FileExists(t.Internal)) {
                e.UserMenu.Add(ContextMenuComands.DateiÖffnen);
            }
        }
        if (e.HotItem is BitmapListItem) {
            //if (FileExists(t.Internal))
            //{
            e.UserMenu.Add("Bild öffnen");
            //}
        }
    }

    private void _Database_Disposing(object sender, System.EventArgs e) => Database = null;

    private void _Database_Loaded(object sender, LoadedEventArgs e) {
        if (InvokeRequired) {
            Invoke(new Action(() => _Database_Loaded(sender, e)));
            return;
        }
        if (Disposing || IsDisposed) { return; }

        UpdateColumnData();
        SetValueFromCell();
    }

    //private void _Database_RowKeyChanged(object sender, KeyChangedEventArgs e) {
    //    if (e.KeyOld != _RowKey) { return; }
    //    _RowKey = e.KeyNew;
    //    GetTmpVariables();
    //    SetValueFromCell();
    //}

    private void Column_ItemInternalChanged(object sender, ListEventArgs e) {
        if ((ColumnItem)e.Item == _tmpColumn) {
            UpdateColumnData();
            CheckEnabledState();
            OnNeedRefresh();
        }
    }

    private void Database_CellValueChanged(object sender, CellEventArgs e) {
        if (e.Row != _tmpRow) { return; }

        if (e.Column == _tmpColumn) { SetValueFromCell(); }

        if (e.Column == _tmpColumn || e.Column == e.Column.Database.Column.SysLocked) { CheckEnabledState(); }
    }

    private void Database_ConnectedControlsStopAllWorking(object sender, System.EventArgs e) => FillCellNow();

    private void Database_RowChecked(object sender, RowCheckedEventArgs e) {
        if (e.Row != _tmpRow) { return; }
        var newT = string.Empty;
        foreach (var thisString in e.ColumnsWithErrors) {
            var x = thisString.SplitAndCutBy("|");
            if (_tmpColumn != null && string.Equals(x[0], _tmpColumn.Name, StringComparison.CurrentCultureIgnoreCase)) {
                if (!string.IsNullOrEmpty(InfoText)) { InfoText += "<br><hr><br>"; }
                newT += x[1];
            }
        }
        InfoText = newT;
    }

    //private void EasyPicConnectedDatabase(object sender, MultiUserFileGiveBackEventArgs e) => e.File = _database;

    //private void EasyPicImageChanged(object sender, System.EventArgs e) {
    //    foreach (Control thisControl in Controls) {
    //        if (thisControl is EasyPic ep) {
    //            if (_tmpColumn == null && _tmpRow == null) { Develop.DebugPrint_NichtImplementiert(); }
    //            if (_tmpColumn.Format != DataFormat.Link_To_Filesystem) { Develop.DebugPrint_NichtImplementiert(); }
    //            switch (ep.SorceType) {
    //                case SorceType.ScreenShot:
    //                    var fil = _tmpColumn.BestFile(_tmpColumn.Name + ".png", true);
    //                    ep.Bitmap.Save(fil, ImageFormat.Png);
    //                    ep.ChangeSource(fil, SorceType.LoadedFromDisk, false);
    //                    ValueSet(fil, false, true);
    //                    return;

    //                case SorceType.Nichts:
    //                    ValueSet(string.Empty, false, true);
    //                    return;

    //                case SorceType.LoadedFromDisk:
    //                    if (ep.SorceName != _tmpColumn.SimplyFile(ep.SorceName)) {
    //                        // DEr name kann nur vereifacht werden, wenn es bereits im richtigen Verzeichniss ist. Name wird vereinfacht (ungleich) - bereits im richtigen verzeichniss!
    //                        ValueSet(ep.SorceName, false, true);
    //                        return;
    //                    }
    //                    var fil2 = _tmpColumn.BestFile(_tmpColumn.Name + ".png", true);
    //                    if (!string.Equals(fil2.FilePath(), ep.SorceName.FilePath(), StringComparison.CurrentCultureIgnoreCase)) {
    //                        ep.Bitmap.Save(fil2, ImageFormat.Png);
    //                    } else {
    //                        fil2 = ep.SorceName;
    //                    }
    //                    ep.ChangeSource(fil2, SorceType.LoadedFromDisk, false);
    //                    ValueSet(fil2, false, true);
    //                    return;

    //                case SorceType.EntryWithoutPic:
    //                    ValueSet(ep.SorceName, false, true);
    //                    // Entweder ein Dummy eintrag (Bildzeichen-Liste, wo Haupt das Bild sein sollte, aber eben nur bei den 3 Seitensichten eines da ist
    //                    // Oder datenbank wird von einem andern PC aus gestartet
    //                    return;
    //            }
    //        }
    //    }
    //    Develop.DebugPrint_NichtImplementiert();
    //}

    private void FillCellNow() {
        if (IsFilling) { return; }
        if (!Enabled) { return; } // Versuch. Eigentlich darf das Steuerelement dann nur empfangen und nix ändern.
        GetTmpVariables(); // Falls der Key inzwischen nicht mehr in der Collection ist, deswegen neu prüfen. RowREmoved greift zwar, kann aber durchaus erst nach RowSortesd/CursorposChanges auftreten.
        if (_tmpColumn == null || _tmpRow == null) { return; }
        var oldVal = _tmpRow.CellGetString(_tmpColumn);
        var newValue = Value;

        if (oldVal == newValue) { return; }

        var tmpR2 = _tmpRow; // Manchmal wird die Sortierung verändert, was zur Folge hat, dass der Cursor verschwindet, wass die _tmpRow verwirft....
        var tmpC2 = _tmpColumn;

        tmpR2.Database.WaitEditable();
        tmpR2.CellSet(tmpC2, newValue);
        if (oldVal != tmpR2.CellGetString(tmpC2)) { tmpR2.DoAutomatic(false, false, 1, "value changed"); }
    }

    private ColumnItem? GetRealColumn(ColumnItem? column, RowItem? row) {
        ColumnItem? gbColumn;

        if (column?.Format == DataFormat.Verknüpfung_zu_anderer_Datenbank) {
            //var skriptgesteuert = column.LinkedCell_RowKey == -9999;

            //if (column.LinkedDatabase() != null && column.LinkedCell_ColumnKey > -1) {
            //    gbColumn = column.LinkedDatabase().Column.SearchByKey(column.LinkedCell_ColumnKey);
            //}

            //if(gbColumn == null && skriptgesteuert) {
            (gbColumn, _, _) = CellCollection.LinkedCellData(column, row, true, false);
            //}

            //if (gbColumn == null) {
            //    Develop.DebugPrint("Column nicht gefunden");
            //    return null;
            //}
        } else {
            gbColumn = column;
        }

        if (gbColumn != null) {
            this.GetStyleFrom(gbColumn);
            //Suffix = gbColumn.Suffix;
            //AllowedChars = gbColumn.AllowedChars;
            //Regex = gbColumn.Regex;
            //MultiLine = gbColumn.MultiLine;
        } else {
            if (column == null) { return null; }  // Bei Steuerelementen, die manuell hinzugefügt werden
            if (row == null) { return null; }  // Beim initualisieren des Controls und Linked Cell kann das vorkommen
            Develop.DebugPrint("Column nicht gefunden: " + column.Name + " " + column.Database.Filename);
        }

        return gbColumn;
    }

    private void GetTmpVariables() {
        try {
            if (_database != null) {
                _tmpColumn = string.IsNullOrEmpty(_columnName)
                    ? _database.Column.SearchByKey(_colKey)
                    : _database.Column[_columnName];
                _tmpRow = _database.Row.SearchByKey(_rowKey);
            } else {
                _tmpColumn = null;
                _tmpRow = null;
            }
        } catch {
            // Multitasking sei dank kann _database trotzem null sein...
            GetTmpVariables();
        }
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

    private void ListBox_AddClicked(object sender, System.EventArgs e) {
        var dia = ColumnItem.UserEditDialogTypeInTable(_tmpColumn, false);

        ListBox lbx = null;

        if (sender is ListBox lbx2) { lbx = lbx2; }
        if (sender is SwapListBox lbx3) { lbx = lbx3.Main; }

        if (lbx == null) { return; }

        switch (dia) {
            case EditTypeTable.None:
                return;

            //case EditTypeTable.FileHandling_InDateiSystem:
            //    // korrektheit der Zelle bereits geprüft
            //    var l = Table.FileSystem(_tmpColumn);

            //    if (l == null) { return; }

            //    foreach (var thisF in l) {
            //        lbx.Item.Add(thisF.FileNameWithSuffix(), _tmpColumn, ShortenStyle.Replaced, _tmpColumn.BildTextVerhalten);
            //    }

            //    return;

            case EditTypeTable.Textfeld:
                lbx.Add_Text();
                return;

            case EditTypeTable.Listbox:
                lbx.Add_TextBySuggestion();
                return;

            default:
                Develop.DebugPrint(dia);
                return;
        }
    }

    private void ListBox_ContextMenuItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
        switch (e.ClickedComand.ToLower()) {
            case "dateiöffnen":
                if (e.HotItem is TextListItem t) {
                    if (FileExists(t.Internal)) {
                        ExecuteFile(t.Internal);
                        //var b = Converter.FileToByte(t.Internal);
                        ////b = Cryptography.SimpleCrypt(b, FileEncryptionKey, -1);
                        //var tmp = TempFile(string.Empty, string.Empty, t.Internal.FileSuffix());
                        //Converter.ByteToFile(tmp, b);
                        //ExecuteFile(tmp, string.Empty, true, false);
                        //MessageBox.Show("Warte...");
                        //DeleteFile(tmp, true);
                    }
                }
                break;

            case "bild öffnen":
                if (e.HotItem is BitmapListItem bi) {
                    if (bi.ImageLoaded()) {
                        Forms.PictureView x = new(bi.Bitmap);
                        x.Show();
                        //var b = modConverter.FileToByte(t.Internal);
                        //b = Generic.SimpleCrypt(b, FileEncryptionKey, -1);
                        //var tmp = TempFile(string.Empty, string.Empty, t.Internal.FileSuffix());
                        //modConverter.ByteToFile(tmp, b);
                        //Generic.ExecuteFile(tmp, null, true, false);
                        //MessageBox.Show("Warte...");
                        //DeleteFile(tmp, true);
                    }
                }
                break;
        }
    }

    private void Marker_DoWork(object sender, DoWorkEventArgs e) {
        TextBox? txb = null;

        foreach (var control in Controls) {
            if (control is TextBox t) { txb = t; }
        }

        if (Marker.CancellationPending) { return; }
        if (txb == null) { return; }
        if (_tmpRow == null) { return; }
        if (Marker.CancellationPending) { return; }
        List<string> names = new();
        if (_database == null) { return; }

        var col = _database.Column[0];
        if (col == null) { return; }
        names.AddRange(col.GetUcaseNamesSortedByLenght());
        if (Marker.CancellationPending) { return; }
        var myname = _tmpRow.CellFirstString().ToUpper();
        var initT = txb.Text;
        bool ok;

        do {
            ok = true;
            Marker.ReportProgress(0, new List<object?> { txb, "Unmark1" });
            Develop.DoEvents();
            if (Marker.CancellationPending || initT != txb.Text) { return; }
            Marker.ReportProgress(0, new List<object?> { txb, "Unmark2" });
            Develop.DoEvents();
            if (Marker.CancellationPending || initT != txb.Text) { return; }
            try {
                foreach (var thisWord in names) {
                    var cap = 0;
                    do {
                        Develop.DoEvents();
                        if (Marker.CancellationPending || initT != txb.Text) { return; }
                        var fo = initT.IndexOfWord(thisWord, cap, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        if (fo < 0) { break; }
                        if (thisWord == myname) {
                            Marker.ReportProgress(0, new List<object?> { txb, "Mark1", fo, fo + thisWord.Length - 1 });
                        } else {
                            Marker.ReportProgress(0, new List<object?> { txb, "Mark2", fo, fo + thisWord.Length - 1 });
                        }
                        cap = fo + thisWord.Length;
                    } while (true);
                }
            } catch {
                ok = false;
            }
        } while (!ok);
    }

    private void Marker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
        //Ja, Multithreading ist kompliziert...
        if (Marker.CancellationPending) { return; }
        var x = (List<object>)e.UserState;
        var txb = (TextBox)x[0];
        switch ((string)x[1]) {
            case "Unmark1":
                txb.Unmark(MarkState.MyOwn);
                txb.Invalidate();
                break;

            case "Unmark2":
                txb.Unmark(MarkState.Other);
                txb.Invalidate();
                break;

            case "Mark1":
                txb.Mark(MarkState.MyOwn, (int)x[2], (int)x[3]);
                txb.Invalidate();
                break;

            case "Mark2":
                txb.Mark(MarkState.Other, (int)x[2], (int)x[3]);
                txb.Invalidate();
                break;

            default:
                Develop.DebugPrint((string)x[1]);
                break;
        }
    }

    private void Row_RowRemoving(object sender, RowEventArgs e) {
        if (e.Row.Key == _rowKey) {
            _rowKey = -1;
            GetTmpVariables();
        }
    }

    private void SetValueFromCell() {
        Develop.DebugPrint_Disposed(IsDisposed);

        if (_tmpColumn == null || _tmpRow == null) {
            ValueSet(string.Empty, true, true);
            InfoText = string.Empty;
            return;
        }

        switch (_tmpColumn.Format) {
            //case DataFormat.Link_To_Filesystem:
            //    var tmp = _tmpRow.CellGetList(_tmpColumn);
            //    List<string> tmp2 = new();
            //    foreach (var file in tmp) {
            //        var tmpF = _tmpColumn.BestFile(file, false);
            //        if (FileExists(tmpF)) {
            //            tmp2.Add(tmpF);
            //        } else {
            //            tmp2.Add(file);
            //        }
            //    }
            //    ValueSet(tmp2.JoinWithCr(), true, true);
            //    //if (Value.ToUpper() != tmp2.JoinWithCr().ToUpper()) {
            //    //    // Dieser Fehler tritt auf, wenn ein BestFile nicht gefunden wurde, weil es auf der Festplatte nicht (mehr) existiert
            //    //  Ode noch ni existiert hat
            //    //    Develop.DebugPrint(enFehlerArt.Warnung, "Werte ungleich: " + Value + " - " + tmp2.JoinWithCr());
            //    //}
            //    break;

            case DataFormat.Verknüpfung_zu_anderer_Datenbank:
                GetRealColumn(_tmpColumn, _tmpRow);
                ValueSet(_tmpRow.CellGetString(_tmpColumn), true, true);
                break;

            default:
                ValueSet(_tmpRow.CellGetString(_tmpColumn), true, true);
                break;
        }
    }

    private void textBox_NeedDatabaseOfAdditinalSpecialChars(object sender, MultiUserFileGiveBackEventArgs e) => e.File = _database;

    private void TextBox_TextChanged(object sender, System.EventArgs e) {
        while (Marker.IsBusy) {
            if (!Marker.CancellationPending) { Marker.CancelAsync(); }
            Develop.DoEvents();
        }
        if (_tmpColumn == null) { return; }
        if (_tmpColumn.Format != DataFormat.RelationText) { return; }
        Marker.RunWorkerAsync();
    }

    private void UpdateColumnData() {
        if (_tmpColumn == null) {
            if (string.IsNullOrEmpty(_columnName)) {
                Caption = "[?]";
                //EditType = EditTypeFormula.None;
                QuickInfo = string.Empty;
            } else {
                Caption = _columnName + ":";
            }
        } else {
            Caption = _tmpColumn.ReadableText() + ":";

            if (string.IsNullOrEmpty(_columnName)) {
                //EditType = _tmpColumn.EditType;
                QuickInfo = _tmpColumn.QuickInfoText(string.Empty);
            }
        }
    }

    #endregion
}