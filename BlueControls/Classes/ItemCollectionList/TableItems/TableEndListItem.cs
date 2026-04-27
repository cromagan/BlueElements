// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Enums;
using BlueTable.Classes;
using BlueTable.Enums;
using System.Drawing;

namespace BlueControls.Classes.ItemCollectionList.TableItems;

public sealed class TableEndListItem : RowBackgroundListItem {

    #region Fields

    public const string Identifier = "TableEndListItem";

    #endregion

    #region Constructors

    public TableEndListItem(ColumnViewCollection? arrangement) : base(Identifier, arrangement, string.Empty) { }

    #endregion

    #region Properties

    protected override bool DoSpezialOrder => true;

    #endregion

    #region Methods

    public override void Draw_ColumnOverlay(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, States state) {
        base.Draw_ColumnOverlay(gr, viewItem, positionControl, state);

        RowListItem.ColumnOverlay(gr, viewItem, positionControl, state);
    }

    public override void Draw_LowerLine(Graphics gr, ColumnViewItem viewItem, ColumnLineStyle lin, float left, float right, float bottom) => base.Draw_LowerLine(gr, viewItem, ColumnLineStyle.Ohne, left, right, bottom);

    public override int HeightInControl(ListBoxAppearance style, int columnWidth, Design itemdesign) => 4;

    public override string QuickInfoForColumn(ColumnViewItem cvi) => string.Empty;

    protected override Size ComputeUntrimmedCanvasSize(Design itemdesign) => new(18 * 3, 4);

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, Design itemdesign, States state, bool drawBorderAndBack, bool translate, float offsetX, float offsetY, float zoom) {
        var newSize = new RectangleF(positionControl.Left, positionControl.Top, positionControl.Width, visibleAreaControl.Height - positionControl.Top);

        base.DrawExplicit(gr, visibleAreaControl, newSize, itemdesign, state, drawBorderAndBack, translate, offsetX, offsetY, zoom);
        //var lgb = new LinearGradientBrush(positionControl, Color.Transparent, Color.White, LinearGradientMode.Vertical);
        //gr.FillRectangle(lgb, positionControl);
    }

    #endregion
}