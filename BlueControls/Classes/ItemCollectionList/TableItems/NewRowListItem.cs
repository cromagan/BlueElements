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
using BlueBasics.Enums;
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

    #region Fields

    public const string Identifier = "NewRowListItem";

    #endregion

    #region Constructors

    public NewRowListItem(ColumnViewCollection? arrangement) : base(Identifier, arrangement, string.Empty) => IgnoreYOffset = true;

    #endregion

    #region Properties

    public FilterCollection? FilterCombined { get; set; }
    protected override bool DoSpezialOrder => true;

    #endregion

    #region Methods

    public override void Draw_ColumnBackGround(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, States state) {
        base.Draw_ColumnBackGround(gr, viewItem, positionControl, state);

        RowListItem.ColumnBackGround(gr, viewItem, positionControl, state);
    }

    public override void Draw_ColumnContent(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, float scale, TranslationType translate, float offsetX, float offsetY, States state) {
        base.Draw_ColumnContent(gr, viewItem, positionControl, scale, translate, offsetX, offsetY, state);

        if (viewItem.Column == null) { return; }

        //var p14 = 14.CanvasToControl(scale);
        //var p1 = 1.CanvasToControl(scale);

        string toDrawd;
        var doWhiteAfter = true;
        //QuickImage? plusszeichen;
        if (viewItem.Column.IsFirst) {
            toDrawd = "[Neue Zeile]";
            //plusszeichen = QuickImage.Get(ImageCode.PlusZeichen, p14);
            doWhiteAfter = false;
        } else {
            toDrawd = FilterCollection.InitValue(viewItem.Column, false, false, [.. FilterCombined]) ?? string.Empty;
            //plusszeichen = QuickImage.Get(ImageCode.PlusZeichen, p14, Color.Transparent, Color.Transparent, 200);
        }

        if (!doWhiteAfter) {
            gr.FillRectangle(GrayBrush2, positionControl);
        }

        if (!string.IsNullOrEmpty(toDrawd)) {
            gr.SetClip(positionControl);
            viewItem.GetRenderer(SheetStyle).Draw(gr, toDrawd, null, positionControl.ToRect(), translate, (Alignment)viewItem.Column.Align, scale);

            //gr.DrawImage(plusszeichen, new Point((int)positionControl.Left + p1, (int)positionControl.Top + p1));
            gr.ResetClip();
        }

        if (doWhiteAfter) {
            gr.FillRectangle(GrayBrush2, positionControl);
        }
    }

    public override void Draw_ColumnOverlay(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, States state) {
        base.Draw_ColumnOverlay(gr, viewItem, positionControl, state);

        RowListItem.ColumnOverlay(gr, viewItem, positionControl, state);
    }

    public override int HeightInControl(ListBoxAppearance style, int columnWidth, Design itemdesign) => UntrimmedCanvasSize(itemdesign).Height;

    public override string QuickInfoForColumn(ColumnViewItem cvi) => string.Empty;

    protected override Size ComputeUntrimmedCanvasSize(Design itemdesign) => new(18, 18);

    #endregion
}