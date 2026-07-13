// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Classes.TableItems;

public sealed class SortBarListItem : RowBackground {

    #region Fields

    public const string Identifier = "SortBarListItem";

    #endregion

    #region Constructors

    public SortBarListItem(ColumnViewCollection? arrangement) : base(Identifier, arrangement, string.Empty) => IgnoreYOffset = true;

    #endregion

    #region Properties

    public FilterCollection? FilterCombined { get; set; }

    public BlueFont Font_TextInFilter {
        get {
            var baseFont = Skin.GetBlueFont(SheetStyle, PadStyles.Emphasized);
            return BlueFont.Get(baseFont.FontName, baseFont.Size - 2, true, false, false, false, Color.White, Color.Red, Color.Transparent);
        }
    }

    public RowSortDefinition? Sort { get; set; }

    protected override bool DoSpezialOrder => true;

    #endregion

    #region Methods

    public override void Draw_ColumnContent(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, float scale, TranslationType translate, float offsetX, float offsetY, States state) {
        base.Draw_ColumnContent(gr, viewItem, positionControl, scale, translate, offsetX, offsetY, state);
        gr.FillRectangle(TableHeadOverlayBrush, positionControl);

        if (Sort is not null && Sort.UsedForRowSort(viewItem.Column)) {
            var p6 = 6.CanvasToControl(scale);
            var p12 = 12.CanvasToControl(scale);
            var im = Sort.Reverse ? QuickImage.Get("ZA|" + p12 + "|" + p6 + "||||50") : QuickImage.Get("AZ|" + p12 + "|" + p6 + "||||50");

            gr.DrawImageUnscaled(im,
                (int)(positionControl.X + (positionControl.Width - im.Width) / 2f),
                (int)(positionControl.Y + (positionControl.Height - im.Height) / 2f));
        }
    }

    public override void Draw_LowerLine(Graphics gr, ColumnViewItem viewItem, ColumnLineStyle lin, float left, float right, float bottom) => base.Draw_LowerLine(gr, viewItem, ColumnLineStyle.Ohne, left, right, bottom);

    public override int HeightInControl(ListBoxAppearance style, int columnWidth, Design itemdesign) => 14;

    public override string QuickInfoForColumn(ColumnViewItem cvi, int mouseXinColumn, int mouseYinColumn, float scale) {
        if (Sort is not null && Sort.UsedForRowSort(cvi.Column)) {
            return "Sortierung: " + (Sort.Reverse ? "Absteigend" : "Aufsteigend");
        }
        return string.Empty;
    }

    protected override Size ComputeUntrimmedCanvasSize(Design itemdesign) => new(14, 14);

    #endregion
}