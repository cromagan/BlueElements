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
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueTable;
using BlueTable.Enums;
using System.Drawing;

namespace BlueControls.ItemCollectionList;

/// <summary>
/// Enthält Daten, wie eine Zeile angezeigt werden soll.
/// RowData kann mehrfach in einer Tabelle angezeigt werden.
/// Ein RowItem ist einzigartig, kann aber in mehreren RowData enthalten sein.
/// </summary>
public sealed class NewRowListItem : RowBackgroundListItem {

    #region Constructors

    public NewRowListItem(ColumnViewCollection? arrangement) : base(string.Empty, arrangement, string.Empty) {
    }

    #endregion

    #region Properties

    public FilterCollection? FilterCombined { get; set; }
    public override string QuickInfo => string.Empty;

    #endregion

    #region Methods

    public override void DrawColumn(Graphics gr, ColumnViewItem viewItem, RectangleF positionModified, float scale, TranslationType translate, float shiftX, float shiftY) {
        base.DrawColumn(gr, viewItem, positionModified, scale, translate, shiftX, shiftY);

        if (viewItem.Column == null) { return; }

        var p14 = ZoomPad.GetPix(14, scale);
        var p1 = ZoomPad.GetPix(1, scale);

        string toDrawd;
        var plus = 0;
        QuickImage? qi;
        if (viewItem.Column.IsFirst) {
            toDrawd = "[Neue Zeile]";
            plus = ZoomPad.GetPix(16, scale);
            qi = QuickImage.Get(ImageCode.PlusZeichen, p14);
        } else {
            toDrawd = FilterCollection.InitValue(viewItem.Column, false, false, [.. FilterCombined]) ?? string.Empty;
            qi = QuickImage.Get(ImageCode.PlusZeichen, p14, Color.Transparent, Color.Transparent, 200);
        }

        if (!string.IsNullOrEmpty(toDrawd)) {
            gr.DrawImage(qi, new Point((int)positionModified.Left + p1, (int)positionModified.Top + p1));
            viewItem.GetRenderer(SheetStyle).Draw(gr, toDrawd, null, positionModified.ToRect(), translate, (Alignment)viewItem.Column.Align, scale);
        }
    }

    public override int HeightForListBox(ListBoxAppearance style, int columnWidth, Design itemdesign) => SizeUntouchedForListBox(itemdesign).Height;

    protected override Size ComputeSizeUntouchedForListBox(Design itemdesign) {
        return new(18, 18);
    }

    #endregion
}