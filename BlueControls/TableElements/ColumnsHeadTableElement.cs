// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.BlueTableDialogs;
using BlueControls.Controls;
using BlueControls.ControlStrategies;
using BlueControls.EventArgs;
using BlueTable.ColumnFormats;
using System.Windows.Forms;

namespace BlueControls.TableElements;

/// <summary>
/// Kümmert sich um die Anzeige des Spaltenkopfes
/// </summary>
public sealed class ColumnsHeadTableElement : TableElement {

    #region Fields

    public const int HeadButtonSize = 16;
    public const string Identifier = "ColumnsHeadTableElement";

    #endregion

    #region Constructors

    public ColumnsHeadTableElement(ColumnViewCollection? arrangement) : base(Identifier, arrangement, string.Empty) => IgnoreYOffset = true;

    #endregion

    #region Properties

    public BlueFont Font_Head_Default => Skin.GetBlueFont(SheetStyle, PadStyles.Emphasized);
    protected override bool DoSpezialOrder => true;

    #endregion

    #region Methods

    public static QuickImage? CaptionBitmap(ColumnItem column) {
        if (string.IsNullOrEmpty(column.CaptionBitmapCode)) { return null; }

        return QuickImage.Get(column.CaptionBitmapCode + "|100");
    }

    public static string CaptionTranslated(string caption) => LanguageTool.DoTranslate(caption, true).Replace("\r", "\r\n");

    public static void ShowDummyColumnDropDown(ColumnViewCollection ca, TableView tableView, ColumnItem? insertAfterColumn) {
        if (ca is not { IsDisposed: false }) { return; }
        if (ca.Table is not { IsDisposed: false } tb) { return; }

        var items = new List<ListItem>();
        var usedColumns = ca.ListOfUsedColumn();

        #region Ausgeblendete Spalten

        var hiddenAdded = false;
        foreach (var col in tb.Column.OrderBy(c => c.ReadableText(), StringComparer.OrdinalIgnoreCase)) {
            if (col is not { IsDisposed: false }) { continue; }
            if (usedColumns.Contains(col)) { continue; }

            if (!hiddenAdded) {
                items.Add(ItemOf("Ausgeblendete Spalten", "Ausgeblendete Spalten", true, "0"));
                hiddenAdded = true;
            }
            var hiddenItem = ItemOf(col.ReadableText(), col.KeyName, col.SymbolForReadableText());
            hiddenItem.UserDefCompareKey = "0_" + col.KeyName;
            items.Add(hiddenItem);
        }

        #endregion

        #region Systemspalten

        var systemKeys = new List<(string key, string display)> {
            (SystemColumnKeys.RowKey, "RowKey"),
            (SystemColumnKeys.RowState, "RowState"),
            (SystemColumnKeys.DateChanged, "DateChanged"),
            (SystemColumnKeys.Changer, "Changer"),
            (SystemColumnKeys.DateCreated, "DateCreated"),
            (SystemColumnKeys.Creator, "Creator"),
            (SystemColumnKeys.Correct, "Correct"),
            (SystemColumnKeys.Locked, "Locked"),
            (SystemColumnKeys.CellNote, "CellNote")
        };

        systemKeys.Sort((a, b) => string.Compare(a.display, b.display, StringComparison.OrdinalIgnoreCase));

        var sysAdded = false;
        foreach (var (key, display) in systemKeys) {
            if (tb.Column.Any(c => c is { IsDisposed: false } && string.Equals(c.KeyName, key, StringComparison.OrdinalIgnoreCase))) { continue; }

            if (!sysAdded) {
                items.Add(ItemOf("Neue Systemspalten", "Neue Systemspalten", true, "1"));
                sysAdded = true;
            }
            var sysItem = ItemOf("NEU: " + display, "SYSNEW:" + key, QuickImage.Get(ImageCode.Schloss, 16));
            sysItem.UserDefCompareKey = "1_" + key;
            items.Add(sysItem);
        }

        #endregion

        #region ColumnFormat

        var fmtAdded = false;
        foreach (var format in ColumnFormat.AllFormats.Instances.OrderBy(f => f.ReadableText(), StringComparer.OrdinalIgnoreCase)) {
            if (!fmtAdded) {
                items.Add(ItemOf("Neue Spalten aus Vorlagen", "Neue Spalten aus Vorlagen", true, "2"));
                fmtAdded = true;
            }

            var prefix = format.KeyName.ToUpperInvariant();
            var nr = 1;
            var testKey = prefix + "_" + nr;
            while (tb.Column[testKey] is not null) {
                nr++;
                testKey = prefix + "_" + nr;
            }

            var fmtItem = ItemOf("NEU: " + format.ReadableText(), "FMTNEW:" + format.KeyName + "|" + testKey, format.SymbolForReadableText());
            fmtItem.UserDefCompareKey = "2_" + format.KeyName;
            items.Add(fmtItem);
        }

        #endregion

        #region Virtuelle Spalten (Pin, Hinzufügen)

        // In Ansicht 0 ("Alle Spalten") werden keine virtuellen Spalten
        // angeboten — sie sind dort nicht erlaubt (siehe ColumnViewCollection.Repair).
        if (!tableView.IsAnsicht0(ca)) {
            var virtualAdded = false;

            foreach (var (vKey, display, symbol) in new (string, string, ImageCode)[] {
                ("PIN", "Pin-Spalte (Zeilen anpinnen)", ImageCode.Pinnadel),
                ("ADD", "Spalte zum Hinzufügen", ImageCode.PlusZeichen)
            }) {
                if (ca.Any(x => x.StorageKey == "VIR_" + vKey)) { continue; }

                if (!virtualAdded) {
                    items.Add(ItemOf("Virtuelle Spalten", "Virtuelle Spalten", true, "3"));
                    virtualAdded = true;
                }

                var virtualItem = ItemOf(display, "VIRADD:" + vKey, QuickImage.Get(symbol, 16));
                virtualItem.UserDefCompareKey = "3_" + vKey;
                items.Add(virtualItem);
            }
        }

        #endregion

        var dropDown = FloatingInputBoxListBoxStyle.Show(items, CheckBehavior.SingleSelection, null, tableView, true, ListBoxAppearance.DropdownSelectbox, Design.Item_DropdownMenu, true);
        dropDown.ItemClicked += (_, e) => HandleDummyColumnSelection(ca, e.Item, tableView, insertAfterColumn);
    }

    public override void Draw_ColumnBackGround(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, States state, Brush? rowcolor) {
        base.Draw_ColumnBackGround(gr, viewItem, positionControl, state, rowcolor);
        gr.FillRectangle(TableHeadOverlayBrush, positionControl);
    }

    public override void Draw_ColumnContent(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, float scale, TranslationType translate, float offsetX, float offsetY, States state) {
        base.Draw_ColumnContent(gr, viewItem, positionControl, scale, translate, offsetX, offsetY, state);

        var column = viewItem.Column is { IsDisposed: false } c ? c : null;

        #region Roten Rand für Split-Spalten

        if (column is not null && column == column.Table?.Column.ChunkValueColumn) {
            var t = positionControl;
            t.Inflate(-3, -3);
            using var redPen = new Pen(Color.Red, 6);
            gr.DrawRectangle(redPen, t);
        }

        #endregion

        var capTranslated = column is not null ? CaptionTranslated(column.Caption) : viewItem.Caption;
        var headFont = Font_Head_Colored(viewItem);
        var Font_Head_Default_Scaled = headFont.Scale(scale).MeasureString(capTranslated);

        if (column is not null && CaptionBitmap(column) is { IsError: false } cb) {

            #region Spalte mit Bild zeichnen

            var p3 = 3.CanvasToControl(scale);

            var pos = new Point((int)positionControl.X + (int)((positionControl.Width - Font_Head_Default_Scaled.Width) / 2.0), (int)(positionControl.Top + p3));
            var imgHeight = (int)positionControl.Bottom - (int)(pos.Y + Font_Head_Default_Scaled.Height) - 6 - 18;
            if (imgHeight > 10) {
                gr.DrawImageInRectAspectRatio(cb, (int)positionControl.X + 2, (int)(pos.Y + Font_Head_Default_Scaled.Height), (int)positionControl.Width - 4, imgHeight);
            }
            gr.TranslateTransform(pos.X, pos.Y);
            Font_Head_Colored(viewItem).Scale(scale).DrawString(gr, capTranslated, 0, 0);
            gr.TranslateTransform(-pos.X, -pos.Y);

            #endregion
        } else {

            #region Spalte ohne Bild zeichnen

            var p4 = 4.CanvasToControl(scale);
            var pos = new Point((int)positionControl.X + (int)((positionControl.Width - Font_Head_Default_Scaled.Height) / 2.0), (int)positionControl.Bottom - p4);
            gr.TranslateTransform(pos.X, pos.Y);
            gr.RotateTransform(-90);
            headFont.Scale(scale).DrawString(gr, capTranslated, 0, 0);
            gr.TranslateTransform(-pos.X, -pos.Y);
            gr.ResetTransform();

            #endregion
        }
    }

    public override void Draw_ColumnOverlay(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, States state) {
        base.Draw_ColumnOverlay(gr, viewItem, positionControl, state);
        if (viewItem.Column is not { IsDisposed: false } column) { return; }
        if (Arrangement?.Table is not { IsDisposed: false } table || !table.IsAdministrator()) { return; }

        var errorReason = column.ErrorReason();
        if (string.IsNullOrEmpty(errorReason)) { return; }

        var bs = HeadButtonSize;
        var btnX = (int)(positionControl.X + (positionControl.Width - bs) / 2.0);
        var btnY = (int)positionControl.Top + 2;
        var btnRect = new Rectangle(btnX, btnY, bs, bs);

        Skin.Draw_Back(gr, Design.Button_AutoFilter, States.Standard, btnRect, null, false);
        Skin.Draw_Border(gr, Design.Button_AutoFilter, States.Standard, btnRect);
        gr.DrawImageUnscaled(QuickImage.Get(ImageCode.Kritisch, bs - 4), btnRect.Left + 2, btnRect.Top + 2);
    }

    public override void Draw_LowerLine(Graphics gr, ColumnViewItem viewItem, ColumnLineStyle lin, float left, float right, float bottom) => base.Draw_LowerLine(gr, viewItem, ColumnLineStyle.Ohne, left, right, bottom);

    public BlueFont Font_Head_Colored(ColumnViewItem viewItem) {
        if (viewItem?.Column is { IsDisposed: false } column) {
            var baseFont = Font_Head_Default;
            return BlueFont.Get(baseFont.FontName, baseFont.Size, baseFont.Bold, baseFont.Italic, baseFont.Underline, baseFont.StrikeOut, column.ForeColor, Color.Transparent, Color.Transparent);
        } else {
            return Font_Head_Default;
        }
    }

    public override void HandleMouseUp(ColumnViewItem? mouseOverColumn, TableView tableView, CanvasMouseEventArgs e) {
        if (mouseOverColumn is not { IsDisposed: false } clickedColumn) { return; }
        if (Arrangement is not { IsDisposed: false } ca) { return; }
        if (!ca.Table?.IsAdministrator() ?? true) { return; }

        if (clickedColumn is AddColumnItem) {
            ShowDummyColumnDropDown(ca, tableView, null);
            tableView.Invalidate_CurrentArrangement();
            return;
        }

        if (clickedColumn.Column is { IsDisposed: false } col) {
            if (!string.IsNullOrEmpty(col.ErrorReason())) {
                var bs = HeadButtonSize.CanvasToControl(tableView.Zoom);
                var btnX = (int)((clickedColumn.ControlColumnWidth() - bs) / 2.0);
                var mouseInColumn = MousePositionInColumn(clickedColumn, tableView, e);
                if (mouseInColumn.X >= btnX && mouseInColumn.X <= btnX + bs && mouseInColumn.Y >= 2 && mouseInColumn.Y <= 2 + bs) {
                    col.Repair();
                    using var editor = new ColumnEditor(col, tableView);
                    editor.ShowDialog();
                    col.Repair();
                    tableView.Invalidate_CurrentArrangement();
                }
            }
        }
    }

    /// <summary>
    /// Übernimmt die gesamte Doppelklick-Logik für den Spaltenkopf: prüft
    /// Spalten- und Tabellen-Zustand sowie Admin-Berechtigung und startet
    /// die Caption-Editierung über TableView.BeginEdit.
    /// </summary>
    public override bool HandleDoubleClick(ColumnViewItem? mouseOverColumn, TableView tableView) {
        if (mouseOverColumn is not { IsDisposed: false } cvi) { return false; }
        if (cvi.Column is not { IsDisposed: false } col) { return false; }
        if (Arrangement?.Table is not { IsDisposed: false } tb || !tb.IsAdministrator()) { return false; }

        var headPos = ControlPosition(tableView.Zoom, tableView.OffsetX, tableView.OffsetY);
        var colX = cvi.ControlColumnLeft(tableView.OffsetX);
        var colW = cvi.ControlColumnWidth();

        tableView.BeginEdit(
            TextBoxControlStrategy.ClassId,
            new Rectangle(colX, headPos.Y, colW, headPos.Height),
            col.Caption.Replace("\r", "\r\n"),
            v => ApplyCaption(tableView, cvi, v),
            TextMultilineColumnFormat.Instance,
            null,
            null,
            null,
            null,
            Font_Head_Default);
        return true;
    }

    public override int HeightInControl(ListBoxAppearance style, int columnWidth, Design itemdesign) => UntrimmedCanvasSize(itemdesign).Height;

    public override void HandleMouseMove(ColumnViewItem? mouseOverColumn, TableView tableView, CanvasMouseEventArgs e) {
        if (mouseOverColumn is not { IsDisposed: false } cvi || e.Button != MouseButtons.None) {
            base.HandleMouseMove(mouseOverColumn, tableView, e);
            return;
        }

        if (cvi.Column is not { IsDisposed: false } col) {
            tableView.QuickInfo = string.Empty;
            return;
        }

        if (!string.IsNullOrEmpty(col.ErrorReason()) && Arrangement?.Table is { IsDisposed: false } table && table.IsAdministrator()) {
            var bs = HeadButtonSize.CanvasToControl(tableView.Zoom);
            var btnX = (int)((cvi.ControlColumnWidth() - bs) / 2.0);
            var mouseInColumn = MousePositionInColumn(cvi, tableView, e);
            if (mouseInColumn.X >= btnX && mouseInColumn.X <= btnX + bs && mouseInColumn.Y >= 2 && mouseInColumn.Y <= 2 + bs) {
                tableView.QuickInfo = "Spalte bearbeiten\rFehler: " + col.ErrorReason();
                return;
            }
        }

        tableView.QuickInfo = RowTableElement.QuickInfoText(col, string.Empty);
    }

    protected override Size ComputeUntrimmedCanvasSize(Design itemdesign) {
        if (IsDisposed || Arrangement is null) { return new(16, 16); }

        if (!Arrangement.ShowHead) { return new(0, 0); }

        var minH = 16;

        var f = Font_Head_Default;

        foreach (var thisC in Arrangement) {
            var capTranslated = CaptionTranslated(thisC.Caption);
            var s = f.MeasureString(capTranslated);
            minH = Math.Max(minH, (int)s.Width);
        }

        return new(100, minH + 3);
    }

    private static void HandleDummyColumnSelection(ColumnViewCollection ca, ListItem selectedItem, TableView tableView, ColumnItem? insertAfterColumn) {
        if (ca is not { IsDisposed: false }) { return; }
        if (ca.Table is not { IsDisposed: false } tb) { return; }
        if (selectedItem is null) { return; }

        var key = selectedItem.KeyName;
        var currentArrName = ca.KeyName;

        // Schreibschutz prüfen, bevor Änderungen versucht werden.
        // Echte Spalten (Systemspalten neu, Vorlagenspalten) benötigen das
        // Recht zum Hinzufügen von Spalten; virtuelle/existierende Spalten
        // nur das Recht zum Ändern des Arrangements.
        var createsColumn = key.StartsWith("SYSNEW:", StringComparison.OrdinalIgnoreCase)
                         || key.StartsWith("FMTNEW:", StringComparison.OrdinalIgnoreCase);

        var editReason = createsColumn
            ? tb.IsValueEditable(TableDataType.Command_AddColumnByName, string.Empty)
            : tb.IsValueEditable(TableDataType.ColumnArrangement, string.Empty);

        if (editReason is { Length: > 0 }) {
            Notification.Show("Spalte konnte nicht hinzugefügt werden:<br>" + editReason, ImageCode.Warnung);
            return;
        }

        var tcvc = ColumnViewCollection.ParseAll(tb);
        tableView.SetPendingSmoothScroll();

        ColumnViewItem? newCol = null;

        if (key.StartsWith("SYSNEW:", StringComparison.OrdinalIgnoreCase)) {
            var sysKey = key[7..];
            tb.Column.GenerateAndAddSystem(sysKey);
            if (tb.Column[sysKey] is { } sysCol) { newCol = new ColumnViewItem(sysCol); }
        } else if (key.StartsWith("VIRADD:", StringComparison.OrdinalIgnoreCase)) {
            newCol = ParseableItem.NewByTypeName<ColumnViewItem>("VIR_" + key[7..].ToUpperInvariant());
        } else if (key.StartsWith("FMTNEW:", StringComparison.OrdinalIgnoreCase)) {
            var parts = key[7..].Split('|');
            if (parts.Length == 2) {
                var formatName = parts[0];
                var targetKey = parts[1];
                var format = ColumnFormat.AllFormats[formatName];
                if (format is not null && tb.Column.GenerateAndAdd(targetKey, targetKey, format) is { } genCol) {
                    newCol = new ColumnViewItem(genCol);
                }
            }
        } else {
            if (tb.Column[key] is { } existCol) { newCol = new ColumnViewItem(existCol); }
        }

        if (newCol is not { IsDisposed: false }) { return; }

        newCol.Column?.Repair();

        for (var z = 0; z < tcvc.Count; z++) {
            if (!tcvc[z].Any(v => string.Equals(v?.ColumnName, newCol.ColumnName, StringComparison.OrdinalIgnoreCase)) && (z == 0 || string.Equals(tcvc[z].KeyName, currentArrName, StringComparison.OrdinalIgnoreCase))) {
                tcvc[z].Add(newCol, insertAfterColumn);
            }
        }

        var availWidth = tableView.AvailableControlPaintArea.Width - IndentWidth.CanvasToControl(tableView.Zoom) * tableView.MaxIndentOfRows;
        ca.ComputeAllColumnPositions(Math.Max(16, availWidth), tableView.Zoom);

        tb.ColumnArrangements = tcvc.AsReadOnly();

        tableView.BeginInvoke(new Action(() => tableView.BeginSmoothScrollToColumn(int.MinValue, tableView.OffsetY)));
    }

    /// <summary>
    /// Übernimmt die neue Spalten-Überschrift. Wenn Caption und KeyName
    /// bisher identisch waren, wird der KeyName aus der neuen Caption
    /// abgeleitet (sofern das ein gültiger Schlüssel ist). Commit-Callback
    /// aus HandleDoubleClick, der an
    /// TableView.BeginEdit übergeben wird.
    /// </summary>
    private void ApplyCaption(TableView tableView, ColumnViewItem? column, string value) {
        if (column?.Column is not { IsDisposed: false } col) { return; }

        var newCaption = value.Replace("\r\n", "\r").Trim();
        if (!string.IsNullOrEmpty(newCaption)) {
            var namesMatch = col.Caption.Equals(col.KeyName, StringComparison.OrdinalIgnoreCase);
            col.Caption = newCaption;
            if (namesMatch) {
                var newKey = newCaption.ReduceToChars(AllowedCharsVariableName).ToUpperInvariant();
                if (!string.IsNullOrEmpty(newKey) && ColumnItem.IsValidColumnKey(newKey)) {
                    col.KeyName = newKey;
                }
            }
        }
        tableView.Invalidate_CurrentArrangement();
    }

    #endregion
}