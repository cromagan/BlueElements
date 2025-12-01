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

using BlueControls;
using BlueControls.Enums;
using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Extended_Text;
using BlueControls.Controls;
using BlueTable.Enums;
using BlueControls.ItemCollectionList;

namespace BlueTable;

/// <summary>
/// Enthält Daten, wie eine Zeile angezeigt werden soll.
/// RowData kann mehrfach in einer Tabelle angezeigt werden.
/// Ein RowItem ist einzigartig, kann aber in mehreren RowData enthalten sein.
/// </summary>
public sealed class RowCaptionListItem : RowBackgroundListItem {

    #region Constructors

    public RowCaptionListItem(string chapterText, ColumnViewCollection arrangement) : base(Key(null, chapterText), arrangement, chapterText.Trim('\\').PathParent()) {
        ChapterText = chapterText;
        Expanded = true;
    }

    #endregion

    #region Properties

    public string ChapterText { get; }
    public bool Expanded { get; set; }

    public BlueFont Font_RowChapter => Skin.GetBlueFont(SheetStyle, PadStyles.Überschrift);
    public override string QuickInfo => string.Empty;

    #endregion

    #region Methods

    public override void Draw_Border(Graphics gr, ColumnLineStyle lin, float xPos, float top, float bottom) {
    }

    public override void Draw_LowerLine(Graphics gr, ColumnLineStyle lin, float left, float right, float bottom) => base.Draw_LowerLine(gr, ColumnLineStyle.Dick, left, right, bottom);

    public override void Draw_UpperLine(Graphics gr, ColumnLineStyle lin, float left, float right, float bottom) => base.Draw_UpperLine(gr, ColumnLineStyle.Dick, left, right, bottom);

    public override int HeightInControl(ListBoxAppearance style, int columnWidth, Design itemdesign) => 40;

    protected override Size ComputeUntrimmedCanvasSize(Design itemdesign) => new(40, 40);

    protected override void DrawExplicit(Graphics gr, Rectangle visibleArea, RectangleF positionInControl, Design itemdesign, States state, bool drawBorderAndBack, bool translate, float offsetX, float offsetY, float scale) {
        base.DrawExplicit(gr, visibleArea, positionInControl, itemdesign, state, drawBorderAndBack, translate, offsetX, offsetY, scale);

        if (Arrangement == null) { return; }

        var Font_RowChapter_Scaled = Font_RowChapter.Scale(scale);

        var tmp = ChapterText.Trim('\\');

        var p14 = 14.CanvasToControl(scale);
        var p5 = 5.CanvasToControl(scale);
        var p23 = 23.CanvasToControl(scale);

        var si = Font_RowChapter_Scaled.MeasureString(tmp);
        gr.FillRectangle(new SolidBrush(Skin.Color_Back(Design.Table_And_Pad, States.Standard).SetAlpha(50)), positionInControl);
        var buttonPos = new Rectangle(1, (int)(positionInControl.Bottom - si.Height - p5 - 2), (int)si.Width + p23 + p14, (int)si.Height + p5);

        if (!Expanded) {
            var x = new ExtText(Design.Button_CheckBox, States.Checked);
            Button.DrawButton(null, gr, Design.Button_CheckBox, States.Checked, null, Alignment.Horizontal_Vertical_Center, false, x, string.Empty, buttonPos, false);
            gr.DrawImage(QuickImage.Get("Pfeil_Unten_Scrollbar|" + p14 + "|||FF0000||200|200"), p5, buttonPos.Top + p5);
        } else {
            var x = new ExtText(Design.Button_CheckBox, States.Standard);
            Button.DrawButton(null, gr, Design.Button_CheckBox, States.Standard, null, Alignment.Horizontal_Vertical_Center, false, x, string.Empty, buttonPos, false);
            gr.DrawImage(QuickImage.Get("Pfeil_Rechts_Scrollbar|" + p14 + "|||||0"), p5, buttonPos.Top + p5);
        }
        Font_RowChapter_Scaled.DrawString(gr, tmp, p23, buttonPos.Top);
        //gr.DrawLine(Skin.PenLinieDick, 0, positionInControl.Y, positionInControl.Width, positionInControl.Y);
    }

    protected override string GetCompareKey() => ChapterText;

    #endregion
}