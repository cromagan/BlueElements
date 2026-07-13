// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.BlueTableDialogs;
using BlueControls.Classes.ItemCollectionList;
using BlueControls.Controls;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Classes.TableItems;

/// <summary>
/// Kümmert sich um die Anzeige des Spaltenkopfes
/// </summary>
public sealed class ColumnsHeadListItem : RowBackground {

    #region Fields

    public const int DummyColumnWidth = 40;
    public const int HeadButtonSize = 16;
    public const string Identifier = "ColumnsHeadListItem";

    #endregion

    #region Constructors

    public ColumnsHeadListItem(ColumnViewCollection? arrangement) : base(Identifier, arrangement, string.Empty) => IgnoreYOffset = true;

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

        var items = new List<AbstractListItem>();
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

        #region Systemspalten (NEU)

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

        #region ColumnFormatHolder (NEU)

        var fmtAdded = false;
        foreach (var format in ColumnFormatHolder.AllFormats.Instances.OrderBy(f => f.ReadableText(), StringComparer.OrdinalIgnoreCase)) {
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

        var dropDown = FloatingInputBoxListBoxStyle.Show(items, CheckBehavior.SingleSelection, null, tableView, true, ListBoxAppearance.DropdownSelectbox, Design.Item_DropdownMenu, true);
        dropDown.ItemClicked += (_, e) => HandleDummyColumnSelection(ca, e.Item, tableView, insertAfterColumn);
    }

    public override void Draw_ColumnBackGround(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, States state, Brush? rowcolor) {
        base.Draw_ColumnBackGround(gr, viewItem, positionControl, state, rowcolor);
        gr.FillRectangle(TableHeadOverlayBrush, positionControl);
    }

    public override void Draw_ColumnContent(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, float scale, TranslationType translate, float offsetX, float offsetY, States state) {
        base.Draw_ColumnContent(gr, viewItem, positionControl, scale, translate, offsetX, offsetY, state);

        if (viewItem.IsDummyColumn) {
            var bs = HeadButtonSize.CanvasToControl(scale);
            var btnRect = new Rectangle((int)(positionControl.X + (positionControl.Width - bs) / 2.0), (int)positionControl.Top + 2, bs, bs);
            Skin.Draw_Back(gr, Design.Button_AutoFilter, States.Standard, btnRect, null, false);
            Skin.Draw_Border(gr, Design.Button_AutoFilter, States.Standard, btnRect);
            gr.DrawImageUnscaled(QuickImage.Get(ImageCode.PlusZeichen, bs - 4), btnRect.Left + 2, btnRect.Top + 2);
        }

        var column = viewItem.Column is { IsDisposed: false } c ? c : null;
        if (column is null && !viewItem.IsDummyColumn) { return; }

        #region Roten Rand für Split-Spalten

        if (column is not null && column == column.Table?.Column.ChunkValueColumn) {
            var t = positionControl;
            t.Inflate(-3, -3);
            using var redPen = new Pen(Color.Red, 6);
            gr.DrawRectangle(redPen, t);
        }

        #endregion

        var capTranslated = column is not null ? CaptionTranslated(column.Caption) : viewItem.Caption;
        var headFont = viewItem.IsDummyColumn ? Font_Head_Default : Font_Head_Colored(viewItem);
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
        if (viewItem.IsDummyColumn) { return; }
        if (viewItem.Column is not { IsDisposed: false } column) { return; }
        if (!Arrangement?.Table.IsAdministrator() ?? true) { return; }

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

    public override bool HandleClick(ColumnViewCollection ca, ColumnViewItem clickedColumn, int mouseXinColumn, int mouseYinColumn, float zoom, TableView tableView) {
        if (!ca.Table?.IsAdministrator() ?? true) { return false; }

        if (clickedColumn?.IsDummyColumn == true) {
            ShowDummyColumnDropDown(ca, tableView, null);
            return true;
        }

        if (clickedColumn?.Column is { IsDisposed: false } col) {
            if (!string.IsNullOrEmpty(col.ErrorReason())) {
                var bs = HeadButtonSize.CanvasToControl(zoom);
                var btnX = (int)((clickedColumn.ControlColumnWidth() - bs) / 2.0);
                if (mouseXinColumn >= btnX && mouseXinColumn <= btnX + bs && mouseYinColumn >= 2 && mouseYinColumn <= 2 + bs) {
                    col.Repair();
                    using var editor = new ColumnEditor(col, tableView);
                    editor.ShowDialog();
                    col.Repair();
                    return true;
                }
            }
        }

        return false;
    }

    public override int HeightInControl(ListBoxAppearance style, int columnWidth, Design itemdesign) => UntrimmedCanvasSize(itemdesign).Height;

    public override string QuickInfoForColumn(ColumnViewItem cvi, int mouseXinColumn, int mouseYinColumn, float scale) {
        if (cvi.IsDummyColumn) { return "Neue Spalte hinzufügen"; }
        if (cvi.Column is not { IsDisposed: false } col) { return string.Empty; }

        if (!string.IsNullOrEmpty(col.ErrorReason()) && (Arrangement?.Table.IsAdministrator() ?? false)) {
            var bs = HeadButtonSize.CanvasToControl(scale);
            var btnX = (int)((cvi.ControlColumnWidth() - bs) / 2.0);
            if (mouseXinColumn >= btnX && mouseXinColumn <= btnX + bs && mouseYinColumn >= 2 && mouseYinColumn <= 2 + bs) {
                return "Spalte bearbeiten\rFehler: " + col.ErrorReason();
            }
        }

        return RowListItem.QuickInfoText(col, string.Empty);
    }

    internal void EditCaption(ColumnViewItem viewItem, TableView tableView) {
        if (viewItem.Column is not { IsDisposed: false } col) { return; }
        if (tableView.Table is not { IsDisposed: false }) { return; }

        var headPos = ControlPosition(tableView.Zoom, tableView.OffsetX, tableView.OffsetY);
        var colX = viewItem.ControlColumnLeft(tableView.OffsetX);
        var colW = viewItem.ControlColumnWidth();

        var bt = tableView.BTB;
        bt.GetStyleFrom(ColumnFormatHolder_TextMultiline.Instance);
        bt.MultiLine = true;
        bt.Text = col.Caption.Replace("\r", "\r\n");
        bt.Location = new Point(colX, headPos.Y);
        bt.Size = new Size(colW, headPos.Height);
        bt.Tag = (List<object?>)[viewItem, this, "CaptionEdit"];
        bt.Verhalten = SteuerelementVerhalten.Scrollen_ohne_Textumbruch;
        bt.Visible = true;
        bt.BringToFront();
        bt.Focus();
    }

    protected override Size ComputeUntrimmedCanvasSize(Design itemdesign) {
        if (IsDisposed || Arrangement is null) { return new(16, 16); }

        if (!Arrangement.ShowHead) { return new(0, 0); }

        var minH = 16;

        var f = Font_Head_Default;

        foreach (var thisC in Arrangement) {
            if (thisC.IsDummyColumn) {
                var capTranslated = "Neue Spalte";
                var s = f.MeasureString(capTranslated);
                minH = Math.Max(minH, (int)s.Width);
            } else if (thisC.Column is { IsDisposed: false } column) {
                var capTranslated = CaptionTranslated(column.Caption);
                var s = f.MeasureString(capTranslated);

                minH = Math.Max(minH, (int)s.Width);
            }
        }

        return new(100, minH + 3);
    }

    private static void HandleDummyColumnSelection(ColumnViewCollection ca, AbstractListItem selectedItem, TableView tableView, ColumnItem? insertAfterColumn) {
        if (ca is not { IsDisposed: false }) { return; }
        if (ca.Table is not { IsDisposed: false } tb) { return; }
        if (selectedItem is null) { return; }

        var key = selectedItem.KeyName;
        var isNewColumn = key.StartsWith("SYSNEW:", StringComparison.OrdinalIgnoreCase) || key.StartsWith("FMTNEW:", StringComparison.OrdinalIgnoreCase);

        if (isNewColumn) { tableView.SetPendingSmoothScroll(); }

        ColumnItem? newCol = null;

        if (key.StartsWith("SYSNEW:", StringComparison.OrdinalIgnoreCase)) {
            var sysKey = key[7..];
            tb.Column.GenerateAndAddSystem(sysKey);
            newCol = tb.Column[sysKey];
        } else if (key.StartsWith("FMTNEW:", StringComparison.OrdinalIgnoreCase)) {
            var parts = key[7..].Split('|');
            if (parts.Length == 2) {
                var formatName = parts[0];
                var targetKey = parts[1];
                var format = ColumnFormatHolder.AllFormats[formatName];
                newCol = tb.Column.GenerateAndAdd(targetKey, targetKey, format);
            }
        } else {
            newCol = tb.Column[key];
        }

        if (newCol is not { IsDisposed: false }) {
            if (isNewColumn) { tableView.BeginSmoothScrollToColumn(tableView.OffsetX, tableView.OffsetY); }
            return;
        }

        newCol.Repair();

        var tcvc = ColumnViewCollection.ParseAll(tb);
        var currentArrName = ca.KeyName;

        for (var z = 0; z < tcvc.Count; z++) {
            if (tcvc[z][newCol] is null && (z == 0 || string.Equals(tcvc[z].KeyName, currentArrName, StringComparison.OrdinalIgnoreCase))) {
                tcvc[z].Add(newCol, insertAfterColumn);
            }
        }

        ca.ComputeAllColumnPositions(tableView.AvailableControlPaintArea.Width, tableView.Zoom);

        if (!isNewColumn) { tableView.SetPendingSmoothScroll(); }
        tb.ColumnArrangements = tcvc.AsReadOnly();

        tableView.BeginInvoke(new Action(() => tableView.BeginSmoothScrollToColumn(int.MinValue, tableView.OffsetY)));
    }

    #endregion
}