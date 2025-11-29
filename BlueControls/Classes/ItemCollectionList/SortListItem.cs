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
using BlueControls;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueTable.Enums;
using System.Drawing;

namespace BlueTable;

public sealed class SortListItem : RowBackgroundListItem {

    #region Constructors

    public SortListItem(ColumnViewCollection? arrangement) : base(string.Empty, arrangement, string.Empty) {
    }

    #endregion

    #region Properties

    public FilterCollection? FilterCombined { get; set; }

    public BlueFont Font_TextInFilter {
        get {
            var baseFont = Skin.GetBlueFont(SheetStyle, PadStyles.Hervorgehoben);
            return BlueFont.Get(baseFont.FontName, baseFont.Size - 2, true, false, false, false, Color.White, Color.Red, Color.Transparent);
        }
    }

    public override string QuickInfo => string.Empty;

    public RowSortDefinition? Sort { get; set; }

    #endregion

    #region Methods

    public override void DrawColumn(Graphics gr, ColumnViewItem viewItem, RectangleF positionModified, float scale, TranslationType translate, float shiftX, float shiftY) {
        base.DrawColumn(gr, viewItem, positionModified, scale, translate, shiftX, shiftY);

        if (Sort != null && Sort.UsedForRowSort(viewItem.Column)) {
            var p6 = ZoomPad.GetPix(6, scale);
            var p12 = ZoomPad.GetPix(12, scale);
            var im = Sort.Reverse ? QuickImage.Get("ZA|" + p12 + "|" + p6 + "||||50") : QuickImage.Get("AZ|" + p12 + "|" + p6 + "||||50");

            gr.DrawImage(im,
                positionModified.X + (positionModified.Width - im.Width) / 2f,
                positionModified.Y + (positionModified.Height - im.Height) / 2f);
        }
    }

    public override int HeightForListBox(ListBoxAppearance style, int columnWidth, Design itemdesign) => 14;

    protected override Size ComputeSizeUntouchedForListBox(Design itemdesign) => new(14, 14);

    #endregion
}