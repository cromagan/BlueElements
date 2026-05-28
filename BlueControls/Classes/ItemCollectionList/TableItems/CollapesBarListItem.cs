// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Classes.ItemCollectionList.TableItems;

public sealed class CollapesBarListItem : RowBackgroundListItem {

    #region Fields

    public const string Identifier = "CollapesBarListItem";
    public static readonly int CollapseButtonSize = 14;

    #endregion

    #region Constructors

    public CollapesBarListItem(ColumnViewCollection? arrangement) : base(Identifier, arrangement, string.Empty) => IgnoreYOffset = true;

    #endregion

    #region Properties

    protected override bool DoSpezialOrder => true;

    #endregion

    #region Methods

    public override void Draw_ColumnBackGround(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, States state) {
        base.Draw_ColumnBackGround(gr, viewItem, positionControl, state);
        gr.FillRectangle(GrayBrush, positionControl);
    }

    public override void Draw_ColumnContent(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, float scale, TranslationType translate, float offsetX, float offsetY, States state) {
        base.Draw_ColumnContent(gr, viewItem, positionControl, scale, translate, offsetX, offsetY, state);

        if (viewItem.CollapsableEnabled()) {
            // Anpassen der Reduce-Button-CanvasPosition

            var p14 = CollapseButtonSize.CanvasToControl(scale);

            var origReduceButtonLocation = new Rectangle((int)positionControl.Right - p14, (int)positionControl.Top, p14, p14);

            gr.DrawImageUnscaled(
                !viewItem.IsExpanded ? QuickImage.Get("Pfeil_Rechts|" + origReduceButtonLocation.Width + "|||FF0000|||||20")
                                : QuickImage.Get("Pfeil_Links|" + origReduceButtonLocation.Width + "||||||||75"),
                origReduceButtonLocation.Left,
                origReduceButtonLocation.Top
            );
        }
    }

    public override void Draw_LowerLine(Graphics gr, ColumnViewItem viewItem, ColumnLineStyle lin, float left, float right, float bottom) => base.Draw_LowerLine(gr, viewItem, ColumnLineStyle.Ohne, left, right, bottom);

    public override int HeightInControl(ListBoxAppearance style, int columnWidth, Design itemdesign) => CollapseButtonSize;

    public override string QuickInfoForColumn(ColumnViewItem cvi, int mouseXinColumn, int mouseYinColumn, float scale) => cvi.CollapsableEnabled() ? "Spalte auf-/zuklappen" : string.Empty;

    protected override Size ComputeUntrimmedCanvasSize(Design itemdesign) => new(CollapseButtonSize, CollapseButtonSize);

    #endregion
}