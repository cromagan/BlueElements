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

using BlueBasics;
using BlueControls.Enums;
using BlueTable;
using BlueTable.Enums;
using System.Drawing;

namespace BlueControls.ItemCollectionList;

public sealed class CollapesBarListItem : RowBackgroundListItem {

    #region Fields

    public const string Identifier = "CollapesBarListItem";
    public static readonly int CollapseButtonSize = 14;

    #endregion

    #region Constructors

    public CollapesBarListItem(ColumnViewCollection? arrangement) : base(Identifier, arrangement, string.Empty) => IgnoreYOffset = true;

    #endregion

    #region Properties

    public override string QuickInfo => string.Empty;
    protected override bool DoSpezialOrder => true;

    #endregion

    #region Methods

    public override void Draw_LowerLine(Graphics gr, ColumnLineStyle lin, float left, float right, float bottom) => base.Draw_LowerLine(gr, ColumnLineStyle.Ohne, left, right, bottom);

    public override void DrawColumn(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, float scale, TranslationType translate, float offsetX, float offsetY, States state) {
        base.DrawColumn(gr, viewItem, positionControl, scale, translate, offsetX, offsetY, state);

        gr.FillRectangle(GrayBrush, positionControl);

        if (viewItem.CollapsableEnabled()) {
            // Anpassen der Reduce-Button-CanvasPosition

            var p14 = CollapseButtonSize.CanvasToControl(scale);

            var origReduceButtonLocation = new Rectangle((int)positionControl.Right - p14, (int)positionControl.Top, p14, p14);

            gr.DrawImage(
                !viewItem.IsExpanded ? QuickImage.Get("Pfeil_Rechts|" + origReduceButtonLocation.Width + "|||FF0000|||||20")
                                : QuickImage.Get("Pfeil_Links|" + origReduceButtonLocation.Width + "||||||||75"),
                origReduceButtonLocation.Left,
                origReduceButtonLocation.Top
            );
        }
    }

    public override int HeightInControl(ListBoxAppearance style, int columnWidth, Design itemdesign) => CollapseButtonSize;

    protected override Size ComputeUntrimmedCanvasSize(Design itemdesign) => new(CollapseButtonSize, CollapseButtonSize);

    #endregion
}