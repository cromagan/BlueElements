// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueControls.Enums;
using BlueTable;
using BlueTable.Enums;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace BlueControls.ItemCollectionList;

public sealed class TableEndListItem : RowBackgroundListItem {

    #region Fields

    public const string Identifier = "TableEndListItem";

    #endregion

    #region Constructors

    public TableEndListItem(ColumnViewCollection? arrangement) : base(Identifier, arrangement, string.Empty) { }

    #endregion

    #region Properties

    public RowSortDefinition? Sort { get; set; }

    protected override bool DoSpezialOrder => true;

    #endregion

    //public override void Draw_ColumnBackGround(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, States state) {
    //    base.Draw_ColumnBackGround(gr, viewItem, positionControl, state); // Weiße Lüken schließen

    //    var lgb = new LinearGradientBrush(positionControl, viewItem.BackColor_ColumnCell, Color.White, LinearGradientMode.Vertical);

    //    gr.FillRectangle(lgb, positionControl);
    //}

    #region Methods

    public override void Draw_ColumnOverlay(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, States state) {
        base.Draw_ColumnOverlay(gr, viewItem, positionControl, state);

        RowListItem.ColumnOverlay(gr, viewItem, positionControl, state);
    }

    public override void Draw_LowerLine(Graphics gr, ColumnViewItem viewItem, ColumnLineStyle lin, float left, float right, float bottom) => base.Draw_LowerLine(gr, viewItem, ColumnLineStyle.Ohne, left, right, bottom);

    public override int HeightInControl(ListBoxAppearance style, int columnWidth, Design itemdesign) => 18 * 3;

    public override string QuickInfoForColumn(ColumnViewItem cvi) => string.Empty;

    protected override Size ComputeUntrimmedCanvasSize(Design itemdesign) => new(18 * 3, 18 * 3);

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, Design itemdesign, States state, bool drawBorderAndBack, bool translate, float offsetX, float offsetY, float zoom) {
        base.DrawExplicit(gr, visibleAreaControl, positionControl, itemdesign, state, drawBorderAndBack, translate, offsetX, offsetY, zoom);
        var lgb = new LinearGradientBrush(positionControl, Color.Transparent, Color.White, LinearGradientMode.Vertical);
        gr.FillRectangle(lgb, positionControl);
    }

    #endregion
}