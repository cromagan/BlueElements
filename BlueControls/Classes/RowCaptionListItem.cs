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

using BlueControls;
using BlueControls.Enums;
using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Extended_Text;
using BlueControls.Controls;

namespace BlueTable;

/// <summary>
/// Enthält Daten, wie eine Zeile angezeigt werden soll.
/// RowData kann mehrfach in einer Tabelle angezeigt werden.
/// Ein RowItem ist einzigartig, kann aber in mehreren RowData enthalten sein.
/// </summary>
public sealed class RowCaptionListItem : RowBackgroundListItem {

    #region Constructors

    public RowCaptionListItem(string chapter, ColumnViewCollection arrangement) : base(Key(null, chapter), arrangement) {
        RowChapter = chapter;
        Expanded = true;
    }

    #endregion

    #region Properties

    public bool Expanded { get; set; }

    public override string QuickInfo => string.Empty;
    public string RowChapter { get; }

    #endregion

    #region Methods

    public override int HeightForListBox(ListBoxAppearance style, int columnWidth, Design itemdesign) => 18;

    protected override Size ComputeSizeUntouchedForListBox(Design itemdesign) => new(18, 18);

    protected override void DrawExplicit(Graphics gr, Rectangle visibleArea, Rectangle positionModified, Design itemdesign, States state, bool drawBorderAndBack, bool translate, float shiftX, float shiftY, float scale) {
        base.DrawExplicit(gr, visibleArea, positionModified, itemdesign, state, drawBorderAndBack, translate, shiftX, shiftY, scale);

        if (Arrangement == null) { return; }

        var chpF = Arrangement.Font_RowChapter.Scale(scale);

        var tmp = RowChapter.Trim('\\');

        var p14 = ZoomPad.GetPix(14, scale);
        var p5 = ZoomPad.GetPix(5, scale);
        var p23 = ZoomPad.GetPix(23, scale);

        var si = chpF.MeasureString(tmp);
        gr.FillRectangle(new SolidBrush(Skin.Color_Back(Design.Table_And_Pad, States.Standard).SetAlpha(50)), positionModified);
        var CaptionPos = new Rectangle(1, (int)(positionModified.Bottom - si.Height), (int)si.Width + p23 + p5, (int)si.Height);

        if (!Expanded) {
            var x = new ExtText(Design.Button_CheckBox, States.Checked);
            Button.DrawButton(null, gr, Design.Button_CheckBox, States.Checked, null, Alignment.Horizontal_Vertical_Center, false, x, string.Empty, CaptionPos, false);
            gr.DrawImage(QuickImage.Get("Pfeil_Unten_Scrollbar|" + p14 + "|||FF0000||200|200"), p5, CaptionPos.Top);
        } else {
            var x = new ExtText(Design.Button_CheckBox, States.Standard);
            Button.DrawButton(null, gr, Design.Button_CheckBox, States.Standard, null, Alignment.Horizontal_Vertical_Center, false, x, string.Empty, CaptionPos, false);
            gr.DrawImage(QuickImage.Get("Pfeil_Rechts_Scrollbar|" + p14 + "|||||0"), p5, CaptionPos.Top);
        }
        chpF.DrawString(gr, tmp, p23, CaptionPos.Top);
        gr.DrawLine(Skin.PenLinieDick, 0, positionModified.Y, positionModified.Width, positionModified.Y);
    }

    protected override string GetCompareKey() => RowChapter;

    #endregion
}