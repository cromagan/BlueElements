// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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
using BlueBasics.Interfaces;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using BlueControls.CellRenderer;
using static BlueBasics.IO;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using BlueControls.Interfaces;
using BlueControls.BlueDatabaseDialogs;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public partial class FlexiControlForCell : GenericControlReciver, IOpenScriptEditor {


    public void OpenScriptEditor() {
        if (IsDisposed || DatabaseInput is not { IsDisposed: false } db) { return; }

        var se = IUniqueWindowExtension.ShowOrCreate<DatabaseScriptEditor>(db);
        se.Row = _lastrow;

    }



    #region Fields

    private ColumnItem? _column;
    private string _columnName = string.Empty;

    private RowItem? _lastrow;

    #endregion

    #region Constructors

    /// <summary>
    /// Für den Designer
    /// </summary>
    public FlexiControlForCell() : this(string.Empty, CaptionPosition.Über_dem_Feld, EditTypeFormula.None) { }

    public FlexiControlForCell(string columnName, CaptionPosition captionPosition, EditTypeFormula editType) : base(false, false) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        Size = new Size(300, 300);
        f.ShowInfoWhenDisabled = true;
        f.CaptionPosition = captionPosition;
        f.EditType = editType;
        _columnName = columnName;
    }

    #endregion

    #region Properties

    public string Caption {
        get {
            _column ??= GetTmpColumn();

            if (_column != null) {
                return _column.ReadableText() + ":";
            } else if (!string.IsNullOrEmpty(_columnName)) {
                return _columnName + ":";
            }

            return "[?]";
        }
    }

    public CaptionPosition CaptionPosition { get => f.CaptionPosition; set => f.CaptionPosition = value; }

    [DefaultValue("")]
    public string ColumnName {
        get => _columnName;
        set {
            if (_columnName == value) { return; }
            _columnName = value;
            _column = null;
            Invalidate_FilterInput();
        }
    }

    public int ControlX {
        get => f.ControlX;
        set => f.ControlX = value;
    }

    public EditTypeFormula EditType { get => f.EditType; set => f.EditType = value; }
    public string Value => f.Value;

    #endregion

    //public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e) {
    //    var (column, row) = GetTmpVariables();

    //    switch (e.Item.KeyName.ToLowerInvariant()) {
    //        case "spalteneigenschaftenbearbeiten":
    //            TableView.OpenColumnEditor(column, null);
    //            return true;

    //        case "vorherigeninhaltwiederherstellen":
    //            Table.DoUndo(column, row);
    //            return true;

    //            //default:
    //            //    //if (Parent is Formula f) {
    //            //    //    return f.ContextMenuItemClickedInternalProcessig(sender, e);
    //            //    //}
    //            //    break;
    //    }
    //    return false;
    //}

    //public void GetContextMenuItems(ContextMenuInitEventArgs e) {
    //    var (column, row) = GetTmpVariables();
    //    if (column?.Database != null && column.Database.IsAdministrator()) {
    //        items.Add(ItemOf(ContextMenuCommands.SpaltenEigenschaftenBearbeiten));
    //    }
    //    if (column?.Database != null && row != null && column.Database.IsAdministrator()) {
    //        items.Add(ItemOf(ContextMenuCommands.VorherigenInhaltWiederherstellen));
    //    }
    //    //if (Parent is Formula f) {
    //    //    ItemCollectionList x = new(BlueListBoxAppearance.KontextMenu, false);
    //    //    f.GetContextMenuItems(null, x, out _, tags, ref cancel, ref translate);
    //    //    if (x.Count > 0) {
    //    //        if (items.Count > 0) {
    //    //            items.Add(AddSeparator());
    //    //        }
    //    //        items.AddClonesFrom(x);
    //    //    }
    //    //}
    //    hotItem = column;
    //}

    #region Methods

    protected override void DatabaseInput_CellValueChanged(object sender, CellChangedEventArgs e) {
        try {
            if (InvokeRequired) {
                _ = Invoke(new Action(() => DatabaseInput_CellValueChanged(sender, e)));
                return;
            }

            if (e.Row != _lastrow) { return; }

            if (e.Column == _column) {
                SetValueFromCell(_column, e.Row);
            }

            if (e.Column == _column || e.Column == e.Column.Database?.Column.SysLocked) {
                CheckEnabledState(_column, e.Row);
            }
        } catch {
            // Invoke: auf das verworfene Ojekt blah blah
            if (!IsDisposed) {
                Develop.CheckStackForOverflow();
                DatabaseInput_CellValueChanged(sender, e);
            }
        }
    }

    protected override void DatabaseInput_ColumnPropertyChanged(object sender, ColumnEventArgs e) {
        if (e.Column == _column) {
            Invalidate_FilterInput();
        }
    }

    protected override void DatabaseInput_Loaded(object sender, System.EventArgs e) {
        if (Disposing || IsDisposed) { return; }

        if (InvokeRequired) {
            try {
                _ = Invoke(new Action(() => DatabaseInput_Loaded(sender, e)));
                return;
            } catch {
                // Kann dank Multitasking disposed sein
                Develop.CheckStackForOverflow();
                DatabaseInput_Loaded(sender, e); // am Anfang der Routine wird auf disposed geprüft
                return;
            }
        }

        Invalidate_FilterInput();
    }

    protected override void DatabaseInput_RowChecked(object sender, RowCheckedEventArgs e) {
        if (!FilterInputChangedHandled || !RowsInputChangedHandled) { return; }

        if (e.Row != _lastrow) { return; }
        if (e.ColumnsWithErrors == null) {
            f.InfoText = string.Empty;
            return;
        }

        var newT = string.Empty;
        foreach (var thisString in e.ColumnsWithErrors) {
            var x = thisString.SplitAndCutBy("|");
            if (_column != null && string.Equals(x[0], _column.KeyName, StringComparison.OrdinalIgnoreCase)) {
                if (!string.IsNullOrEmpty(f.InfoText)) { f.InfoText += "<br><hr><br>"; }
                newT += x[1];
            }
        }
        f.InfoText = newT;
    }

    /// <summary>
    /// Verwendete Ressourcen bereinigen.
    /// </summary>
    /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
    protected override void Dispose(bool disposing) {
        if (disposing) {
            f.Dispose();
            components?.Dispose();
            RestartMarker(); // Restart beendet den Marker und starten ihn bei Bedarf
        }

        base.Dispose(disposing);
    }

    protected override void HandleChangesNow() {
        base.HandleChangesNow();
        if (IsDisposed) { return; }
        if (RowsInputChangedHandled && FilterInputChangedHandled) { return; }

        if (!f.Allinitialized) { return; }

        DoInputFilter(null, false);
        DoRows();

        _lastrow = RowSingleOrNull();
        _column ??= GetTmpColumn();

        StyleControls(_column, _lastrow);
        SetValueFromCell(_column, _lastrow);
        CheckEnabledState(_column, _lastrow);

        _lastrow?.CheckRowDataIfNeeded();

        if (_lastrow?.LastCheckedEventArgs is { } rce) {
            DatabaseInput_RowChecked(this, rce);
        }
    }

    //protected override void RemoveAll() {
    //    ValueToCell();
    //    base.RemoveAll();
    //}

    private static void ListBox_ContextMenuInit(object sender, ContextMenuInitEventArgs e) {
        if (e.HotItem is TextListItem t) {
            if (FileExists(t.KeyName)) {
                e.ContextMenu.Add(ItemOf(ContextMenuCommands.DateiÖffnen));
            }
        }
        if (e.HotItem is BitmapListItem) {
            e.ContextMenu.Add(ItemOf("Bild öffnen"));
        }
    }

    private void ActivateMarker() {
        if (IsDisposed || !Visible || !Enabled) { return; }

        if (!FilterInputChangedHandled || !RowsInputChangedHandled) { return; }
        if (_column is not { IsDisposed: false }) { return; }
        if (_column.Function != ColumnFunction.RelationText) { return; }
        Marker.RunWorkerAsync();
    }

    private void CheckEnabledState(ColumnItem? column, RowItem? row) {
        if (Parent == null) {
            f.DisabledReason = "Kein Bezug zu einem Formular.";
            return;
        }

        if (!Parent.Enabled) {
            f.DisabledReason = "Übergeordnetes Formular deaktiviert.";
            return;
        }

        if (column == null) {
            f.DisabledReason = "Kein Bezug zu einer Spalte.";
            return;
        }

        if (row == null) {
            f.DisabledReason = "Kein Bezug zu einer Zelle.";
            return;
        }

        if (row.Database != column.Database) {
            f.DisabledReason = "Interner Fehler. Admin verständigen.";
            return;
        }

        f.DisabledReason = CellCollection.EditableErrorReason(column, row, EditableErrorReasonType.EditNormaly, true, false, true, false); // Rechteverwaltung einfliesen lassen.
    }

    private void F_ControlAdded(object sender, ControlEventArgs e) {
        switch (e.Control) {
            case TextBox textBox:
                textBox.NeedDatabaseOfAdditinalSpecialChars += textBox_NeedDatabaseOfAdditinalSpecialChars;
                //textBox.GotFocus += GotFocus_TextBox;
                textBox.TextChanged += TextBox_TextChanged;
                break;

            case ListBox listBox:
                listBox.ContextMenuInit += ListBox_ContextMenuInit;
                listBox.ContextMenuItemClicked += ListBox_ContextMenuItemClicked;
                break;

            case BlueControls.Controls.Caption:
            case SwapListBox:
            case Button:
            case Line:
            case GroupBox:
                break;

            default:
                Develop.DebugPrint("Control unbekannt");
                break;
        }

        Invalidate_FilterInput();
    }

    private void F_ControlRemoved(object sender, ControlEventArgs e) {
        switch (e.Control) {
            case TextBox textBox:
                textBox.NeedDatabaseOfAdditinalSpecialChars -= textBox_NeedDatabaseOfAdditinalSpecialChars;
                textBox.TextChanged -= TextBox_TextChanged;
                break;

            case ListBox listBox:
                listBox.ContextMenuInit -= ListBox_ContextMenuInit;
                listBox.ContextMenuItemClicked -= ListBox_ContextMenuItemClicked;
                break;

            case SwapListBox:
            case BlueControls.Controls.Caption:
            case Button:
            case Line:
            case GroupBox:
                break;

            default:
                Develop.DebugPrint("Control unbekannt");
                break;
        }
    }

    private void F_EnabledChanged(object sender, System.EventArgs e) => RestartMarker();

    private void F_ValueChanged(object sender, System.EventArgs e) => ValueToCell();

    private void F_VisibleChanged(object sender, System.EventArgs e) => RestartMarker();

    private ColumnItem? GetRealColumn(ColumnItem? column, RowItem? row) {
        ColumnItem? gbColumn;

        if (column?.Function == ColumnFunction.Verknüpfung_zu_anderer_Datenbank) {
            (gbColumn, _, _, _) = CellCollection.LinkedCellData(column, row, true, false);
        } else {
            gbColumn = column;
        }

        if (gbColumn != null) { f.GetStyleFrom(gbColumn); }

        return gbColumn;
    }

    private ColumnItem? GetTmpColumn() {
        try {
            ColumnItem? tmpColumn;

            if (DatabaseInput is { IsDisposed: false } db) {
                tmpColumn = db.Column[_columnName];
            } else {
                tmpColumn = null;
            }

            return tmpColumn;
        } catch {
            // Multitasking sei dank kann _database trotzem null sein...
            Develop.CheckStackForOverflow();
            return GetTmpColumn();
        }
    }

    private void ListBox_ContextMenuItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
        switch (e.Item.KeyName.ToLowerInvariant()) {
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
        if (IsDisposed || DatabaseInput is not { IsDisposed: false } db) { return; }

        #region  in Frage kommende Textbox ermitteln txb

        TextBox? txb = null;
        foreach (var control in f.Controls) {
            if (control is TextBox t) { txb = t; }
        }

        if (txb == null) { return; }

        #endregion

        if (Marker.CancellationPending) { return; }

        if (!FilterInputChangedHandled || !RowsInputChangedHandled) { return; }

        if (_lastrow is not { IsDisposed: false } row) { return; }
        if (Marker.CancellationPending) { return; }

        var col = db.Column.First();
        if (col == null) { return; }

        List<string> names = [.. col.GetUcaseNamesSortedByLenght()];

        if (Marker.CancellationPending) { return; }

        var myname = row.CellFirstString().ToUpperInvariant();
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

    private void Marker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
        //ActivateMarker();
    }

    private void RestartMarker() {
        if (Marker is { IsBusy: true, CancellationPending: false }) {
            Marker.CancelAsync();
        } else {
            ActivateMarker();
        }
    }

    private void SetValueFromCell(ColumnItem? column, RowItem? row) {
        if (IsDisposed) { return; }

        if (column == null || row == null) {
            f.ValueSet(string.Empty, true);
            f.InfoText = string.Empty;
            return;
        }

        switch (column.Function) {
            case ColumnFunction.Verknüpfung_zu_anderer_Datenbank:
                _ = GetRealColumn(column, row);
                f.ValueSet(row.CellGetString(column), true);
                break;

            default:
                f.ValueSet(row.CellGetString(column), true);
                break;
        }
    }

    private void StyleControls(ColumnItem? column, RowItem? row) {
        var realColumn = GetRealColumn(column, row);
        f.Caption = Caption;

        if (realColumn != null) {
   
            QuickInfo = realColumn.QuickInfoText(string.Empty);

            f.GetStyleFrom(realColumn);
            if (Renderer_Abstract.RendererOf(realColumn) is Renderer_TextOneLine r) {
                f.Suffix = r.Suffix;
            }
        } 

        foreach (var thisControl in f.Controls) {
            switch (thisControl) {
                case ComboBox comboBox:
                    var item2 = new List<AbstractListItem>();
                    if (realColumn != null) {
                        var r = Renderer_Abstract.RendererOf(column);
                        item2.AddRange(ItemsOf(realColumn, null, 10000, r));
                    }

                    if (realColumn is { TextBearbeitungErlaubt: true }) {
                        f.StyleComboBox(comboBox, item2, ComboBoxStyle.DropDown, false);
                    } else {
                        f.StyleComboBox(comboBox, item2, ComboBoxStyle.DropDownList, true);
                    }

                    break;

                case TextBox textBox:
                    f.StyleTextBox(textBox);
                    break;

                case ListBox listBox:
                    StyleListBox(listBox, realColumn);
                    break;

                case SwapListBox swapListBox:
                    StyleSwapListBox(swapListBox, realColumn);
                    break;

                case BlueControls.Controls.Caption:
                case Button:
                case Line:
                case GroupBox:
                    break;

                default:
                    Develop.DebugPrint("Control unbekannt");
                    break;
            }
        }
    }

    private void StyleListBox(ListBox control, ColumnItem? column) {
        //control.Enabled = Enabled;
        //control.ItemClear();
        control.CheckBehavior = CheckBehavior.MultiSelection;
        if (column is not { IsDisposed: false }) { return; }

        var item = new List<AbstractListItem>();
        if (column.DropdownBearbeitungErlaubt) {
            var r = Renderer_Abstract.RendererOf(column);
            item.AddRange(ItemsOf(column, null, 10000, r));
            if (!column.DropdownWerteAndererZellenAnzeigen) {
                bool again;
                do {
                    again = false;
                    foreach (var thisItem in item) {
                        if (!column.DropDownItems.Contains(thisItem.KeyName)) {
                            again = true;
                            item.Remove(thisItem);
                            break;
                        }
                    }
                } while (again);
            }
        }
        control.ItemAddRange(item);

        switch (ColumnItem.UserEditDialogTypeInTable(column, false)) {
            case EditTypeTable.Textfeld:
                control.AddAllowed = AddType.Text;
                break;

            case EditTypeTable.Listbox:
                control.AddAllowed = AddType.OnlySuggests;
                break;

            default:
                control.AddAllowed = AddType.None;
                break;
        }

        control.MoveAllowed = false;
        switch (f.EditType) {
            //case EditTypeFormula.Gallery:
            //    control.Appearance = BlueListBoxAppearance.Gallery;
            //    control.RemoveAllowed = true;
            //    break;

            case EditTypeFormula.Listbox:
                control.RemoveAllowed = true;
                control.Appearance = ListBoxAppearance.Listbox;
                break;
        }
    }

    private void StyleSwapListBox(SwapListBox control, ColumnItem? column) {
        //control.Enabled = Enabled;
        //control.UnCheck();
        control.SuggestionsClear();
        if (column is not { IsDisposed: false }) { return; }

        var r = Renderer_Abstract.RendererOf(column);

        var item = new List<AbstractListItem>();
        item.AddRange(ItemsOf(column, null, 10000, r));
        control.SuggestionsAdd(item);
        switch (ColumnItem.UserEditDialogTypeInTable(column, false)) {
            case EditTypeTable.Textfeld:
                control.AddAllowed = AddType.Text;
                break;

            case EditTypeTable.Listbox:
                control.AddAllowed = AddType.OnlySuggests;
                break;

            default:
                control.AddAllowed = AddType.None;
                break;
        }
    }

    private void textBox_NeedDatabaseOfAdditinalSpecialChars(object sender, DatabaseFileGiveBackEventArgs e) => e.File = DatabaseInput;

    private void TextBox_TextChanged(object sender, System.EventArgs e) => RestartMarker();

    private void ValueToCell() {
        if (f.IsFilling) { return; }
        if (!Enabled) { return; } // Versuch. Eigentlich darf das Steuerelement dann nur empfangen und nix ändern.

        if (_column is not { IsDisposed: false }) { return; }

        if (!FilterInputChangedHandled || !RowsInputChangedHandled) { return; }

        if (_lastrow is not { IsDisposed: false } row) { return; }

        var oldVal = row.CellGetString(_column);
        var newValue = f.Value;

        if (oldVal == newValue) { return; }

        row.CellSet(_column, newValue, "Über Formular bearbeitet (FlexiControl)");
        //if (oldVal != row.CellGetString(column)) {
        //    _ = row.ExecuteScript(EventTypes.value_changedx, string.Empty, false, false, true, 1);
        //    row.Database?.AddBackgroundWork(row);
        //}
    }

    #endregion
}