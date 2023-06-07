// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using BlueDatabase.Interfaces;
using static BlueBasics.IO;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public partial class FlexiControlVariable : FlexiControl, IContextMenu, IDisabledReason, IHasDatabase, IControlAcceptRow {

    #region Fields

    private string _columnName = string.Empty;
    private IControlSendRow? _getRowFrom;
    private long _rowKey = -1;
    private ColumnItem? _tmpColumn;
    private RowItem? _tmpRow;

    #endregion

    #region Constructors

    public FlexiControlVariable() : this(null, string.Empty, ÜberschriftAnordnung.Über_dem_Feld, EditTypeFormula.None) { }

    // Für den Designer
    public FlexiControlVariable(DatabaseAbstract? database, string column, ÜberschriftAnordnung captionPosition, EditTypeFormula editType) : base() {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        Size = new Size(300, 300);
        ShowInfoWhenDisabled = true;
        CaptionPosition = captionPosition;
        EditType = editType;
        ColumnName = column;
        SetData(database, -1);
        CheckEnabledState();
    }

    #endregion

    #region Events

    public event EventHandler<ContextMenuInitEventArgs>? ContextMenuInit;

    public event EventHandler<ContextMenuItemClickedEventArgs>? ContextMenuItemClicked;

    #endregion

    #region Properties

    [Description("Dieses Feld kann für den Forms-Editor verwendet werden. Falls ein Key und ein Name befüllt sind, ist der Name führend.")]
    [DefaultValue("")]
    public string ColumnName {
        get => _columnName;
        set {
            if (_columnName == value) { return; }
            _columnName = value;
            _tmpColumn = null;
            UpdateColumnData();
            SetValueFromCell();
        }
    }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public DatabaseAbstract? Database { get; private set; }

    public IControlSendRow? GetRowFrom {
        get => _getRowFrom;
        set {
            if (_getRowFrom == value) { return; }
            if (_getRowFrom != null) {
                Develop.DebugPrint(FehlerArt.Fehler, "Änderung nicht erlaubt");
            }

            _getRowFrom = value;
            if (_getRowFrom != null) { _getRowFrom.ChildAdd(this); }
        }
    }

    public RowItem? LastInputRow { get; private set; }

    #endregion

    #region Methods

    public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e) {
        var (column, row) = GetTmpVariables();

        switch (e.ClickedComand.ToLower()) {
            case "spalteneigenschaftenbearbeiten":
                TableView.OpenColumnEditor(column, null);
                return true;

            case "vorherigeninhaltwiederherstellen":
                Table.DoUndo(column, row);
                return true;

                //default:
                //    //if (Parent is Formula f) {
                //    //    return f.ContextMenuItemClickedInternalProcessig(sender, e);
                //    //}
                //    break;
        }
        return false;
    }

    public void GetContextMenuItems(MouseEventArgs? e, ItemCollectionList items, out object? hotItem, ref bool cancel, ref bool translate) {
        var (column, row) = GetTmpVariables();
        if (column?.Database != null && column.Database.IsAdministrator()) {
            _ = items.Add(ContextMenuComands.SpaltenEigenschaftenBearbeiten);
        }
        if (column?.Database != null && row != null && column.Database.IsAdministrator()) {
            _ = items.Add(ContextMenuComands.VorherigenInhaltWiederherstellen);
        }
        //if (Parent is Formula f) {
        //    ItemCollectionList x = new(BlueListBoxAppearance.KontextMenu, false);
        //    f.GetContextMenuItems(null, x, out _, tags, ref cancel, ref translate);
        //    if (x.Count > 0) {
        //        if (items.Count > 0) {
        //            items.AddSeparator();
        //        }
        //        items.AddClonesFrom(x);
        //    }
        //}
        hotItem = column;
    }

    public void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);

    public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) => ContextMenuItemClicked?.Invoke(this, e);

    public void SetData(DatabaseAbstract? database, long? rowkey) {
        if (rowkey == _rowKey && database == Database) { return; }
        FillCellNow();

        if (Database != null) {
            Database.Cell.CellValueChanged -= Database_CellValueChanged;
            Database.Row.RowRemoving -= Row_RowRemoving;
            Database.Column.ColumnInternalChanged -= Column_ItemInternalChanged;
            Database.Row.RowChecked -= Database_RowChecked;
            Database.Loaded -= _Database_Loaded;
            Database.Disposing -= _Database_Disposing;
        }

        Database = database;
        _rowKey = rowkey ?? -1;
        _tmpRow = null;
        _tmpColumn = null;

        UpdateColumnData();

        if (Database != null) {
            Database.Cell.CellValueChanged += Database_CellValueChanged;
            Database.Row.RowRemoving += Row_RowRemoving;
            Database.Column.ColumnInternalChanged += Column_ItemInternalChanged;
            Database.Row.RowChecked += Database_RowChecked;
            Database.Loaded += _Database_Loaded;
            Database.Disposing += _Database_Disposing;
        }
        SetValueFromCell();
        CheckEnabledState();

        var (_, row) = GetTmpVariables();
        row?.CheckRowDataIfNeeded();

        if (row?.LastCheckedEventArgs is RowCheckedEventArgs e) {
            Database_RowChecked(this, e);
        }
    }

    internal void CheckEnabledState() {
        var (column, row) = GetTmpVariables();

        if (Parent == null || !Parent.Enabled || column == null || row == null) {
            DisabledReason = "Kein Bezug zu einer Zelle.";
            return;
        }
        DisabledReason = CellCollection.EditableErrorReason(column, row, EditableErrorReason.EditNormaly, true, false); // Rechteverwaltung einfliesen lassen.
    }

    protected override void OnControlAdded(ControlEventArgs e) {
        base.OnControlAdded(e);
        if (e.Control is Caption) { return; } // z.B. Info Caption

        var (column, _) = GetTmpVariables();

        var column1 = GetRealColumn(column, null);

        if (column1 != null) {
            Suffix = column1.Suffix;
            Regex = column1.Regex;
            AllowedChars = column1.AllowedChars;
            MaxTextLenght = column1.MaxTextLenght;
        }

        switch (e.Control) {
            case ComboBox comboBox:
                ItemCollectionList item2 = new(true);
                ItemCollectionList.GetItemCollection(item2, column1, null, ShortenStyle.Replaced, 10000);
                if (column1 != null && column1.TextBearbeitungErlaubt) {
                    StyleComboBox(comboBox, item2, ComboBoxStyle.DropDown, false);
                } else {
                    StyleComboBox(comboBox, item2, ComboBoxStyle.DropDownList, true);
                }
                //comboBox.GotFocus += GotFocus_ComboBox;
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
                //textBox.GotFocus += GotFocus_TextBox;
                textBox.TextChanged += TextBox_TextChanged;
                break;

            case ListBox listBox:
                StyleListBox(listBox, column1);
                listBox.ContextMenuInit += ListBox_ContextMenuInit;
                listBox.ContextMenuItemClicked += ListBox_ContextMenuItemClicked;
                break;

            case SwapListBox swapListBox:
                StyleSwapListBox(swapListBox, column1);
                break;

            case Button:
            case Line:
                break;

            default:
                Develop.DebugPrint("Control unbekannt");
                break;
        }
    }

    protected override void OnControlRemoved(ControlEventArgs e) {
        base.OnControlRemoved(e);
        switch (e.Control) {
            case ComboBox:
                break;

            case TextBox textBox:
                textBox.NeedDatabaseOfAdditinalSpecialChars -= textBox_NeedDatabaseOfAdditinalSpecialChars;
                textBox.TextChanged -= TextBox_TextChanged;
                break;

            case ListBox listBox:
                listBox.ContextMenuInit -= ListBox_ContextMenuInit;
                listBox.ContextMenuItemClicked -= ListBox_ContextMenuItemClicked;
                break;

            case SwapListBox _:
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
    }

    private static void ListBox_ContextMenuInit(object sender, ContextMenuInitEventArgs e) {
        if (e.HotItem is TextListItem t) {
            if (FileExists(t.KeyName)) {
                _ = e.UserMenu.Add(ContextMenuComands.DateiÖffnen);
            }
        }
        if (e.HotItem is BitmapListItem) {
            _ = e.UserMenu.Add("Bild öffnen");
        }
    }

    private void _Database_Disposing(object sender, System.EventArgs e) => SetData(null, null);

    private void _Database_Loaded(object sender, System.EventArgs e) {
        if (Disposing || IsDisposed) { return; }

        if (InvokeRequired) {
            try {
                _ = Invoke(new Action(() => _Database_Loaded(sender, e)));
                return;
            } catch {
                // Kann dank Multitasking disposed sein
                _Database_Loaded(sender, e); // am Anfang der Routine wird auf disposed geprüft
                return;
            }
        }

        UpdateColumnData();
        SetValueFromCell();
    }

    private void Column_ItemInternalChanged(object sender, ColumnEventArgs e) {
        var (column, _) = GetTmpVariables();

        if (e.Column == column) {
            UpdateColumnData();
            CheckEnabledState();
            OnNeedRefresh();
        }
    }

    private void Database_CellValueChanged(object sender, CellEventArgs e) {
        if (InvokeRequired) {
            _ = Invoke(new Action(() => Database_CellValueChanged(sender, e)));
            return;
        }

        var (column, row) = GetTmpVariables();

        if (e.Row != row) { return; }

        if (e.Column == column) { SetValueFromCell(); }

        if (e.Column == column || e.Column == e.Column?.Database?.Column.SysLocked) { CheckEnabledState(); }
    }

    private void Database_RowChecked(object sender, RowCheckedEventArgs e) {
        var (column, row) = GetTmpVariables();

        if (e.Row != row) { return; }
        if (e.ColumnsWithErrors == null) {
            InfoText = string.Empty;
            return;
        }

        var newT = string.Empty;
        foreach (var thisString in e.ColumnsWithErrors) {
            var x = thisString.SplitAndCutBy("|");
            if (column != null && string.Equals(x[0], column.Name, StringComparison.OrdinalIgnoreCase)) {
                if (!string.IsNullOrEmpty(InfoText)) { InfoText += "<br><hr><br>"; }
                newT += x[1];
            }
        }
        InfoText = newT;
    }

    private void FillCellNow() {
        if (IsFilling) { return; }
        if (!Enabled) { return; } // Versuch. Eigentlich darf das Steuerelement dann nur empfangen und nix ändern.
        var (column, row) = GetTmpVariables();
        if (column == null || row == null) { return; }
        if (column.IsDisposed || row.IsDisposed) { return; }

        var oldVal = row.CellGetString(column);
        var newValue = Value;

        if (oldVal == newValue) { return; }

        row.CellSet(column, newValue);
        //if (oldVal != row.CellGetString(column)) {
        //    _ = row.ExecuteScript(EventTypes.value_changedx, string.Empty, false, false, true, 1);
        //    row.Database?.AddBackgroundWork(row);
        //}
    }

    private ColumnItem? GetRealColumn(ColumnItem? column, RowItem? row) {
        ColumnItem? gbColumn;

        if (column?.Format == DataFormat.Verknüpfung_zu_anderer_Datenbank) {
            (gbColumn, _, _) = CellCollection.LinkedCellData(column, row, true, false);
        } else {
            gbColumn = column;
        }

        if (gbColumn != null) { this.GetStyleFrom(gbColumn); }

        return gbColumn;
    }

    private (ColumnItem? column, RowItem? row) GetTmpVariables() {
        try {
            if (_tmpColumn != null && _tmpRow != null) { return (_tmpColumn, _tmpRow); }

            if (Database != null && !Database.IsDisposed) {
                _tmpColumn = Database.Column.Exists(_columnName);
                _tmpRow = Database.Row.SearchByKey(_rowKey);
            } else {
                _tmpColumn = null;
                _tmpRow = null;
            }

            return (_tmpColumn, _tmpRow);
        } catch {
            // Multitasking sei dank kann _database trotzem null sein...
            return GetTmpVariables();
        }
    }

    private void ListBox_ContextMenuItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
        switch (e.ClickedComand.ToLower()) {
            case "dateiöffnen":
                if (e.HotItem is TextListItem t) {
                    if (FileExists(t.KeyName)) {
                        _ = ExecuteFile(t.KeyName);
                    }
                }
                break;

            case "bild öffnen":
                if (e.HotItem is BitmapListItem bi) {
                    if (bi.ImageLoaded()) {
                        PictureView x = new(bi.Bitmap);
                        x.Show();
                    }
                }
                break;
        }
    }

    private void Marker_DoWork(object sender, DoWorkEventArgs e) {
        if (Database == null || Database.IsDisposed) { return; }

        #region  in Frage kommende Textbox ermitteln txb

        TextBox? txb = null;
        foreach (var control in Controls) {
            if (control is TextBox t) { txb = t; }
        }

        if (txb == null) { return; }

        #endregion

        if (Marker.CancellationPending) { return; }

        var (_, row) = GetTmpVariables();
        if (row == null || row.IsDisposed) { return; }
        if (Marker.CancellationPending) { return; }

        var col = Database.Column.First();
        if (col == null) { return; }

        List<string> names = new();
        names.AddRange(col.GetUcaseNamesSortedByLenght());

        if (Marker.CancellationPending) { return; }

        var myname = row.CellFirstString().ToUpper();
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
                        var fo = initT.IndexOfWord(thisWord, cap, RegexOptions.IgnoreCase);
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
            _tmpRow = null;
        }
    }

    private void SetValueFromCell() {
        if (IsDisposed) { return; }

        var (column, row) = GetTmpVariables();

        if (column == null || row == null) {
            ValueSet(string.Empty, true, true);
            InfoText = string.Empty;
            return;
        }

        switch (column.Format) {
            case DataFormat.Verknüpfung_zu_anderer_Datenbank:
                _ = GetRealColumn(column, row);
                ValueSet(row.CellGetString(column), true, true);
                break;

            default:
                ValueSet(row.CellGetString(column), true, true);
                break;
        }
    }

    private void textBox_NeedDatabaseOfAdditinalSpecialChars(object sender, MultiUserFileGiveBackEventArgs e) => e.File = Database;

    private void TextBox_TextChanged(object sender, System.EventArgs e) {
        while (Marker.IsBusy) {
            if (!Marker.CancellationPending) { Marker.CancelAsync(); }
            Develop.DoEvents();
        }

        var (column, _) = GetTmpVariables();

        if (column == null || column.IsDisposed) { return; }
        if (column.Format != DataFormat.RelationText) { return; }
        Marker.RunWorkerAsync();
    }

    private void UpdateColumnData() {
        var (column, _) = GetTmpVariables();

        if (column == null || column.IsDisposed) {
            if (string.IsNullOrEmpty(_columnName)) {
                Caption = "[?]";
                //EditType = EditTypeFormula.None;
                QuickInfo = string.Empty;
            } else {
                Caption = _columnName + ":";
            }
        } else {
            Caption = column.ReadableText() + ":";

            if (string.IsNullOrEmpty(_columnName)) {
                QuickInfo = column.QuickInfoText(string.Empty);
            }
        }
    }

    #endregion
}