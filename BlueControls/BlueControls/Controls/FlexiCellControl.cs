// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using BlueControls.BlueTableDialogs;
using BlueControls.CellRenderer;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueTable;
using BlueTable.Enums;
using BlueTable.EventArgs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static BlueBasics.IO;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public partial class FlexiCellControl : GenericControlReciver, IOpenScriptEditor {

    #region Fields

    private ColumnItem? _column;

    private string _columnName;

    private RowItem? _lastrow;

    private CancellationTokenSource? _markerCancellation;

    #endregion

    #region Constructors

    /// <summary>
    /// Für den Designer
    /// </summary>
    public FlexiCellControl() : this(string.Empty, CaptionPosition.Über_dem_Feld, EditTypeFormula.None) { }

    public FlexiCellControl(string columnName, CaptionPosition captionPosition, EditTypeFormula editType) : base(false, false, false) {
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
            _column ??= Column;

            if (_column != null) {
                return _column.ReadableText() + ":";
            }

            if (!string.IsNullOrEmpty(_columnName)) {
                return _columnName + ":";
            }

            return "[?]";
        }
    }

    public CaptionPosition CaptionPosition { get => f.CaptionPosition; set => f.CaptionPosition = value; }

    public ColumnItem? Column {
        get {
            try {
                return TableInput is { IsDisposed: false } tb ? tb.Column[_columnName] : null;
            } catch {
                // Multitasking sei dank kann _table trotzem null sein...
                Develop.AbortAppIfStackOverflow();
                return Column;
            }
        }
    }

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

    #region Methods

    public void OpenScriptEditor() {
        if (IsDisposed || TableInput is not { IsDisposed: false } tb) { return; }

        var se = IUniqueWindowExtension.ShowOrCreate<TableScriptEditor>(tb);
        se.Row = _lastrow;
    }

    /// <summary>
    /// Verwendete Ressourcen bereinigen.
    /// </summary>
    /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
    protected override void Dispose(bool disposing) {
        if (disposing) {
            _markerCancellation?.Cancel();
            _markerCancellation?.Dispose();
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
        RowsInputChangedHandled = true;

        _lastrow = RowSingleOrNull();
        _column ??= Column;

        if (_lastrow != null) { StyleControls(_column, _lastrow); }
        SetValueFromCell(_column, _lastrow);
        CheckEnabledState(_column, _lastrow);

        if (_lastrow?.CheckRow() is { } rce) {
            TableInput_RowChecked(this, rce);
        }
    }

    protected override void TableInput_CellValueChanged(object sender, CellEventArgs e) {
        try {
            if (InvokeRequired) {
                Invoke(new Action(() => TableInput_CellValueChanged(sender, e)));
                return;
            }

            if (e.Row != _lastrow) { return; }

            if (e.Column == _column) {
                SetValueFromCell(_column, e.Row);
            }

            if (e.Column == _column || e.Column == e.Column.Table?.Column.SysLocked) {
                CheckEnabledState(_column, e.Row);
            }
        } catch {
            // Invoke: auf das verworfene Ojekt blah blah
            if (!IsDisposed) {
                Develop.AbortAppIfStackOverflow();
                TableInput_CellValueChanged(sender, e);
            }
        }
    }

    protected override void TableInput_ColumnPropertyChanged(object sender, ColumnEventArgs e) {
        if (e.Column == _column) {
            Invalidate_FilterInput();
        }
    }

    protected override void TableInput_Loaded(object sender, System.EventArgs e) {
        if (Disposing || IsDisposed) { return; }

        if (InvokeRequired) {
            try {
                Invoke(new Action(() => TableInput_Loaded(sender, e)));
                return;
            } catch {
                // Kann dank Multitasking disposed sein
                Develop.AbortAppIfStackOverflow();
                TableInput_Loaded(sender, e); // am Anfang der Routine wird auf disposed geprüft
                return;
            }
        }

        Invalidate_FilterInput();
    }

    protected override void TableInput_RowChecked(object sender, RowCheckedEventArgs e) {
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

    private static void ListBox_ContextMenuInit(object sender, ContextMenuInitEventArgs e) {
        if (e.HotItem is TextListItem t) {
            if (FileExists(t.KeyName)) {
                e.ContextMenu.Add(ItemOf("Öffnen / Ausführen", "DateiÖffnen", QuickImage.Get(ImageCode.Blitz)));
            }
        }
        if (e.HotItem is BitmapListItem) {
            e.ContextMenu.Add(ItemOf("Bild öffnen"));
        }
    }

    private async Task ActivateMarker() {
        if (IsDisposed || !Visible || !Enabled) { return; }
        if (!FilterInputChangedHandled || !RowsInputChangedHandled) { return; }
        if (_column is not { IsDisposed: false }) { return; }
        if (!_column.Relationship_to_First) { return; }

        // Alten Task abbrechen
        _markerCancellation?.Cancel();
        _markerCancellation?.Dispose();
        _markerCancellation = new CancellationTokenSource();

        try {
            await RunMarkerAsync(_markerCancellation.Token);
        } catch (OperationCanceledException) {
            // Normal bei Cancel
        } catch (Exception ex) {
            Develop.DebugPrint("Marker Fehler: " + ex.Message);
        }
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

        if (row.Table != column.Table) {
            f.DisabledReason = "Interner Fehler. Admin verständigen.";
            return;
        }

        f.DisabledReason = CellCollection.IsCellEditable(column, row, row.ChunkValue); // Rechteverwaltung einfliesen lassen.
    }

    private void F_ControlAdded(object sender, ControlEventArgs e) {
        switch (e.Control) {
            case TextBox textBox:
                textBox.NeedTableOfAdditinalSpecialChars += textBox_NeedTableOfAdditinalSpecialChars;
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
        Invalidate_RowsInput();
        Invalidate();
    }

    private void F_ControlRemoved(object sender, ControlEventArgs e) {
        switch (e.Control) {
            case TextBox textBox:
                textBox.NeedTableOfAdditinalSpecialChars -= textBox_NeedTableOfAdditinalSpecialChars;
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

    private void ListBox_ContextMenuItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
        switch (e.Item.KeyName.ToLowerInvariant()) {
            case "dateiöffnen":
                if (e.HotItem is TextListItem t) {
                    if (FileExists(t.KeyName)) {
                        ExecuteFile(t.KeyName);
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

    private void RestartMarker() {
        // Fire-and-forget Pattern für Event-Handler
        Task.Run(async () => {
            try {
                await ActivateMarker();
            } catch (Exception ex) {
                Develop.DebugPrint("RestartMarker Fehler: " + ex.Message);
            }
        });
    }

    private async Task RunMarkerAsync(CancellationToken cancellationToken) {
        if (IsDisposed || TableInput is not { IsDisposed: false } db) { return; }

        // Thread-sichere TextBox ermitteln
        var txb = f.GetControl<TextBox>();
        if (txb == null) { return; }

        // Thread-sicherer Text-Zugriff
        var initT = await Develop.GetSafePropertyValueAsync(() => txb.Text);
        if (string.IsNullOrEmpty(initT)) { return; }

        cancellationToken.ThrowIfCancellationRequested();

        if (!FilterInputChangedHandled || !RowsInputChangedHandled) { return; }
        if (_lastrow is not { IsDisposed: false } row) { return; }

        var col = db.Column.First;
        if (col == null) { return; }

        // Background-Thread für schwere Berechnungen
        await Task.Run(async () => {
            var names = col.GetUcaseNamesSortedByLength().ToList();
            cancellationToken.ThrowIfCancellationRequested();

            var myname = row.CellFirstString().ToUpperInvariant();

            bool processSuccessful;
            do {
                processSuccessful = true;

                try {
                    // UI-Thread: Textbox zurücksetzen
                    await Develop.InvokeAsync(() => {
                        if (!txb.IsDisposed && !IsDisposed) {
                            txb.Unmark(MarkState.MyOwn);
                            txb.Unmark(MarkState.Other);
                            txb.Invalidate();
                        }
                    });

                    cancellationToken.ThrowIfCancellationRequested();

                    // Verarbeitung der Wörter
                    foreach (var thisWord in names) {
                        var cap = 0;
                        do {
                            cancellationToken.ThrowIfCancellationRequested();

                            // Thread-sicherer Text-Check
                            var currentText = await Develop.GetSafePropertyValueAsync(() => txb.Text);
                            if (currentText != initT) { return; }

                            var fo = initT.IndexOfWord(thisWord, cap, RegexOptions.IgnoreCase);
                            if (fo < 0) { break; }

                            // UI-Thread: Markierung setzen
                            await Develop.InvokeAsync(() => {
                                if (!txb.IsDisposed && !IsDisposed) {
                                    if (thisWord == myname) {
                                        txb.Mark(MarkState.MyOwn, fo, fo + thisWord.Length - 1);
                                    } else {
                                        txb.Mark(MarkState.Other, fo, fo + thisWord.Length - 1);
                                    }
                                    txb.Invalidate();
                                }
                            });

                            cap = fo + thisWord.Length;
                        } while (true);
                    }
                } catch (Exception) {
                    processSuccessful = false;
                    await Task.Delay(100, cancellationToken); // Kurz warten vor Retry
                }
            } while (!processSuccessful && !cancellationToken.IsCancellationRequested);
        }, cancellationToken);
    }

    private void SetValueFromCell(ColumnItem? column, RowItem? row) {
        if (IsDisposed) { return; }

        if (column == null || row == null) {
            f.ValueSet(string.Empty, true);
            f.InfoText = string.Empty;
            return;
        }

        f.ValueSet(row.CellGetString(column), true);
    }

    private void StyleControls(ColumnItem? column, RowItem row) {
        ColumnItem? realColumn = column;

        if (column?.RelationType == RelationType.CellValues) {
            (realColumn, _, _, _) = row.LinkedCellData(column, true, false);
        }

        f.Caption = Caption;
        int delay = 1;

        if (realColumn != null) {
            QuickInfo = TableView.QuickInfoText(realColumn, string.Empty);

            f.GetStyleFrom(realColumn);
            if (TableView.RendererOf(realColumn, Constants.Win11) is Renderer_TextOneLine r) {
                f.Suffix = r.Suffix;
            }

            if (realColumn.HasAutoRepair) { delay = 10; }
        }

        foreach (var thisControl in f.Controls) {
            switch (thisControl) {
                case ComboBox comboBox:
                    var item2 = new List<AbstractListItem>();
                    if (realColumn != null) {
                        var r = TableView.RendererOf(column, Constants.Win11);
                        item2.AddRange(ItemsOf(realColumn, null, 10000, r));
                    }

                    if (realColumn is { IsDisposed: false, EditableWithTextInput: true }) {
                        f.StyleComboBox(comboBox, item2, ComboBoxStyle.DropDown, false, delay);
                    } else {
                        f.StyleComboBox(comboBox, item2, ComboBoxStyle.DropDownList, true, delay);
                    }

                    break;

                case TextBox textBox:
                    f.StyleTextBox(textBox, delay);
                    break;

                case ListBox listBox:
                    f.StyleListBox(listBox, realColumn);
                    break;

                case SwapListBox swapListBox:
                    f.StyleSwapListBox(swapListBox, realColumn);
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

    private void textBox_NeedTableOfAdditinalSpecialChars(object sender, TableFileGiveBackEventArgs e) => e.Table = TableInput;

    private void TextBox_TextChanged(object sender, System.EventArgs e) => RestartMarker();

    private void ValueToCell() {
        if (!f.Enabled) { return; } // Versuch. Eigentlich darf das Steuerelement dann nur empfangen und nix ändern.

        if (_column is not { IsDisposed: false }) { return; }

        if (!FilterInputChangedHandled || !RowsInputChangedHandled) { return; }

        if (_lastrow is not { IsDisposed: false } row) { return; }

        var oldVal = row.CellGetString(_column);
        var newValue = _column.AutoCorrect(f.Value, true);

        if (oldVal == newValue) { return; }

        row.CellSet(_column, newValue, "Über Formular bearbeitet (FlexiControl)");
    }

    #endregion
}