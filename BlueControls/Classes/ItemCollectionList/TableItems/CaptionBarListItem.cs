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
using BlueBasics.Enums;
using BlueControls.Enums;
using BlueTable;
using BlueTable.Enums;
using System.Drawing;

namespace BlueControls.ItemCollectionList;

public sealed class CaptionBarListItem : RowBackgroundListItem {

    #region Fields

    public static readonly int CaptionHeight = 22;

    private string prevCaptionGroup = string.Empty;

    private ColumnViewItem? prevViewItem;

    private ColumnViewItem? prevViewItemWithOtherCaption;
    private int prevViewItemWithOtherCaptionLe;

    #endregion

    #region Constructors

    public CaptionBarListItem(ColumnViewCollection? arrangement, int caption) : base(Identifier(caption), arrangement, string.Empty) {
        IgnoreYOffset = true;
        Caption = caption;
    }

    #endregion

    #region Properties

    public int Caption { get; private set; }

    public BlueFont Font_Head_Default => Skin.GetBlueFont(SheetStyle, PadStyles.Hervorgehoben);

    protected override bool DoSpezialOrder => false;

    #endregion

    #region Methods

    public static string Identifier(int captionRow) => $"CaptionBar{captionRow}";

    public override void Draw_Border(Graphics gr, ColumnViewItem viewItem, ColumnLineStyle lin, float xPos, float top, float bottom) {
        var newCaptionGroup = viewItem.Column?.CaptionGroup(Caption) ?? string.Empty;

        if (string.IsNullOrEmpty(newCaptionGroup) || string.IsNullOrEmpty(prevCaptionGroup)) {
            base.Draw_Border(gr, viewItem, lin, xPos, top, bottom);
        } else {
            base.Draw_Border(gr, viewItem, ColumnLineStyle.Ohne, xPos, top, bottom);
        }
    }

    public override void Draw_ColumnBackGround(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, States state) {
        base.Draw_ColumnBackGround(gr, viewItem, positionControl, state);
        gr.FillRectangle(GrayBrush, positionControl);
    }

    public override void Draw_ColumnContent(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, float scale, TranslationType translate, float offsetX, float offsetY, States state) {
        base.Draw_ColumnContent(gr, viewItem, positionControl, scale, translate, offsetX, offsetY, state);
        var newCaptionGroup = viewItem.Column?.CaptionGroup(Caption) ?? string.Empty;

        if (newCaptionGroup != prevCaptionGroup) {

            #region Ende einer Gruppierung gefunden

            if (!string.IsNullOrEmpty(prevCaptionGroup) && prevViewItem is { IsDisposed: false } && prevViewItemWithOtherCaption is { }) {
                Draw_Column_Head_Captions_Now(gr, prevViewItemWithOtherCaption, positionControl, prevCaptionGroup, scale);
            }

            prevViewItemWithOtherCaption = viewItem;
            prevViewItemWithOtherCaptionLe = (int)positionControl.Left;

            #endregion
        }

        prevViewItem = viewItem;
        prevCaptionGroup = newCaptionGroup;

        // Zeichen-Routine für das letzte Element aufrufen
        if (!string.IsNullOrEmpty(prevCaptionGroup) && prevViewItem is { IsDisposed: false } && prevViewItemWithOtherCaption is { }) {
            Draw_Column_Head_Captions_Now(gr, prevViewItemWithOtherCaption, Rectangle.Empty, prevCaptionGroup, scale);
        }
    }

    public override void Draw_LowerLine(Graphics gr, ColumnViewItem viewItem, ColumnLineStyle lin, float left, float right, float bottom) {
        var newCaptionGroup = viewItem.Column?.CaptionGroup(Caption) ?? string.Empty;

        if (string.IsNullOrEmpty(newCaptionGroup)) {
            base.Draw_LowerLine(gr, viewItem, ColumnLineStyle.Ohne, left, right, bottom);
        } else {
            base.Draw_LowerLine(gr, viewItem, ColumnLineStyle.Kräftig, left, right, bottom);
        }
    }

    public override int HeightInControl(ListBoxAppearance style, int columnWidth, Design itemdesign) => CaptionHeight;

    public override string QuickInfoForColumn(ColumnViewItem cvi) => string.Empty;

    protected override Size ComputeUntrimmedCanvasSize(Design itemdesign) => new(CaptionHeight, CaptionHeight);

    private void Draw_Column_Head_Captions_Now(Graphics gr, ColumnViewItem prevViewItemWithOtherCaption, RectangleF positionControlOfNextItem, string prevCaptionGroup, float _zoom) {
        if (prevViewItemWithOtherCaptionLe < positionControlOfNextItem.Left) {
            var capTranslated = ColumnsHeadListItem.CaptionTranslated(prevCaptionGroup);

            var r = new RectangleF(prevViewItemWithOtherCaptionLe, positionControlOfNextItem.Top, positionControlOfNextItem.Left - prevViewItemWithOtherCaptionLe, positionControlOfNextItem.Height);
            //gr.FillRectangle(new SolidBrush(prevViewItemWithOtherCaption.BackColor_ColumnHead), r);
            //gr.FillRectangle(new SolidBrush(Color.FromArgb(80, 200, 200, 200)), r);
            gr.DrawRectangle(Skin.PenLinieKräftig, r);
            Skin.Draw_FormatedText(gr, capTranslated, null, Alignment.Horizontal_Vertical_Center, r.ToRect(), null, false, Font_Head_Default.Scale(_zoom), false);
        }
    }

    #endregion
}