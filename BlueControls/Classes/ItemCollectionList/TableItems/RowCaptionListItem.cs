// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueControls.Extended_Text;

namespace BlueControls.Classes.ItemCollectionList.TableItems;

/// <summary>
/// Enthält Daten, wie eine Zeile angezeigt werden soll.
/// RowData kann mehrfach in einer Tabelle angezeigt werden.
/// Ein RowItem ist einzigartig, kann aber in mehreren RowData enthalten sein.
/// </summary>
public sealed class RowCaptionListItem : RowBackgroundListItem {

    #region Constructors

    public RowCaptionListItem(string chapterText, ColumnViewCollection arrangement) : base(Identifier(chapterText), arrangement, chapterText.Trim('\\').PathParent()) {
        ChapterText = chapterText;
        IsExpanded = true;
    }

    #endregion

    #region Properties

    public string ChapterText { get; }
    public BlueFont Font_RowChapter => Skin.GetBlueFont(SheetStyle, PadStyles.Title);
    public bool IsExpanded { get; set; }
    protected override bool DoSpezialOrder => true;

    #endregion

    #region Methods

    public static string Identifier(string caption) => $"CAP-{caption.ToUpperInvariant()}";

    public override void Draw_Border(Graphics gr, ColumnViewItem viewItem, ColumnLineStyle lin, float xPos, float top, float bottom) { }

    public override void Draw_LowerLine(Graphics gr, ColumnViewItem viewItem, ColumnLineStyle lin, float left, float right, float bottom) => base.Draw_LowerLine(gr, viewItem, ColumnLineStyle.Dick, left, right, bottom);

    public override void Draw_UpperLine(Graphics gr, ColumnLineStyle lin, float left, float right, float bottom) => base.Draw_UpperLine(gr, ColumnLineStyle.Dick, left, right, bottom);

    public override int HeightInControl(ListBoxAppearance style, int columnWidth, Design itemdesign) => 40;

    public override string QuickInfoForColumn(ColumnViewItem cvi, int mouseXinColumn, int mouseYinColumn, float scale) {
        var expandInfo = IsExpanded ? "Kapitel zuklappen" : "Kapitel aufklappen";
        var text = ChapterText.Trim('\\');

        if (CanEditChapter) {
            return $"{expandInfo}: {text}\rDoppelklick zum Bearbeiten";
        }

        return $"{expandInfo}: {text}";
    }

    internal void EditChapter(TableView tableView) {
        if (Arrangement?.ColumnForChapter is not { IsDisposed: false }) { return; }
        if (tableView.Table is not { IsDisposed: false }) { return; }

        var capPos = ControlPosition(tableView.Zoom, tableView.OffsetX, tableView.OffsetY);

        var bt = tableView.BTB;
        bt.GetStyleFrom(ColumnFormatHolder_TextOneLine.Instance);
        bt.MultiLine = false;
        bt.Text = ChapterText.Trim('\\');
        bt.Location = new Point(0, capPos.Y);
        bt.Size = new Size(tableView.Width, capPos.Height);
        bt.Tag = (List<object?>)[null, this, "ChapterEdit"];
        bt.Verhalten = SteuerelementVerhalten.Scrollen_ohne_Textumbruch;
        bt.Visible = true;
        bt.BringToFront();
        bt.Focus();
    }

    /// <summary>
    /// Gibt an, ob dieses Kapitel per Doppelklick bearbeitet werden darf.
    /// Nur bei echten Kapiteln (nicht Ohne/Angepinnt/Weitere Zeilen),
    /// mit vorhandener Kapitel-Spalte, außerhalb von TableChunk
    /// und wenn die Benutzerrechte es erlauben.
    /// </summary>
    internal bool CanEditChapter {
        get {
            if (Arrangement?.ColumnForChapter is not { IsDisposed: false } capCol) { return false; }
            if (Arrangement.Table is not { IsDisposed: false } tb) { return false; }
            if (tb is TableChunk) { return false; }
            if (ChapterText == TableView.Ohne || ChapterText == TableView.Angepinnt || ChapterText == TableView.Weitere_Zeilen) { return false; }
            return tb.PermissionCheck(capCol.PermissionGroupsChangeCell, null);
        }
    }

    protected override Size ComputeUntrimmedCanvasSize(Design itemdesign) => new(40, 40);

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, Design itemdesign, States state, bool drawBorderAndBack, bool translate, float offsetX, float offsetY, float zoom) {
        base.DrawExplicit(gr, visibleAreaControl, positionControl, itemdesign, state, drawBorderAndBack, translate, offsetX, offsetY, zoom);

        if (Arrangement is null) { return; }

        var Font_RowChapter_Scaled = Font_RowChapter.Scale(zoom);

        var tmp = ChapterText.Trim('\\');

        var p14 = 14.CanvasToControl(zoom);
        var p5 = 5.CanvasToControl(zoom);
        var p23 = 23.CanvasToControl(zoom);

        var si = Font_RowChapter_Scaled.MeasureString(tmp);
        gr.FillRectangle(new SolidBrush(Skin.Color_Back(Design.Table_And_Pad, States.Standard).SetAlpha(50)), positionControl);
        var buttonPos = new Rectangle(1, (int)(positionControl.Bottom - si.Height - p5 - 2), (int)si.Width + p23 + p14, (int)si.Height + p5);

        if (!IsExpanded) {
            var x = new ExtText(Design.Button_CheckBox, States.Checked);
            Button.DrawButton(null, gr, Design.Button_CheckBox, States.Checked, null, Alignment.Horizontal_Vertical_Center, false, x, string.Empty, buttonPos, false);
            gr.DrawImageUnscaled(QuickImage.Get("Pfeil_Unten_Scrollbar|" + p14 + "|||FF0000||200|200"), p5, buttonPos.Top + p5);
        } else {
            var x = new ExtText(Design.Button_CheckBox, States.Standard);
            Button.DrawButton(null, gr, Design.Button_CheckBox, States.Standard, null, Alignment.Horizontal_Vertical_Center, false, x, string.Empty, buttonPos, false);
            gr.DrawImageUnscaled(QuickImage.Get("Pfeil_Rechts_Scrollbar|" + p14 + "|||||0"), p5, buttonPos.Top + p5);
        }
        Font_RowChapter_Scaled.DrawString(gr, tmp, p23, buttonPos.Top);
    }

    protected override string GetCompareKey() => ChapterText;

    #endregion
}