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

using BlueBasics;
using BlueBasics.Classes;
using BlueControls.Enums;
using BlueTable.Classes;
using BlueTable.Enums;
using System.Drawing;

namespace BlueControls.Classes.ItemCollectionList.TableItems;

public sealed class SortBarListItem : RowBackgroundListItem {

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
            var baseFont = Skin.GetBlueFont(SheetStyle, PadStyles.Hervorgehoben);
            return BlueFont.Get(baseFont.FontName, baseFont.Size - 2, true, false, false, false, Color.White, Color.Red, Color.Transparent);
        }
    }

    public RowSortDefinition? Sort { get; set; }

    protected override bool DoSpezialOrder => true;

    #endregion

    #region Methods

    public override void Draw_ColumnContent(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, float scale, TranslationType translate, float offsetX, float offsetY, States state) {
        base.Draw_ColumnContent(gr, viewItem, positionControl, scale, translate, offsetX, offsetY, state);
        gr.FillRectangle(GrayBrush, positionControl);

        if (Sort != null && Sort.UsedForRowSort(viewItem.Column)) {
            var p6 = 6.CanvasToControl(scale);
            var p12 = 12.CanvasToControl(scale);
            var im = Sort.Reverse ? QuickImage.Get("ZA|" + p12 + "|" + p6 + "||||50") : QuickImage.Get("AZ|" + p12 + "|" + p6 + "||||50");

            gr.DrawImage(im,
                positionControl.X + (positionControl.Width - im.Width) / 2f,
                positionControl.Y + (positionControl.Height - im.Height) / 2f);
        }
    }

    public override void Draw_LowerLine(Graphics gr, ColumnViewItem viewItem, ColumnLineStyle lin, float left, float right, float bottom) => base.Draw_LowerLine(gr, viewItem, ColumnLineStyle.Ohne, left, right, bottom);

    public override int HeightInControl(ListBoxAppearance style, int columnWidth, Design itemdesign) => 14;

    public override string QuickInfoForColumn(ColumnViewItem cvi) => string.Empty;

    protected override Size ComputeUntrimmedCanvasSize(Design itemdesign) => new(14, 14);

    #endregion
}