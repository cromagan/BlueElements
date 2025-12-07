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

    public override string QuickInfo => string.Empty;

    #endregion

    #region Methods

    public static string Identifier(int captionRow) => $"CaptionBar{captionRow}";

    public override void Draw_LowerLine(Graphics gr, ColumnLineStyle lin, float left, float right, float bottom) => base.Draw_LowerLine(gr, ColumnLineStyle.Dick, left, right, bottom);

    public override void DrawColumn(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, float scale, TranslationType translate, float offsetX, float offsetY, States state) {
        base.DrawColumn(gr, viewItem, positionControl, scale, translate, offsetX, offsetY, state);

        var newCaptionGroup = viewItem.Column?.CaptionGroup(Caption) ?? string.Empty;

        if (newCaptionGroup != prevCaptionGroup) {

            #region Ende einer Gruppierung gefunden

            if (!string.IsNullOrEmpty(prevCaptionGroup) && prevViewItem is { IsDisposed: false } && prevViewItemWithOtherCaption is { }) {
                Draw_Column_Head_Captions_Now(gr, prevViewItemWithOtherCaption, positionControl, prevCaptionGroup);
            }

            prevViewItemWithOtherCaption = viewItem;
            prevViewItemWithOtherCaptionLe = (int)positionControl.Left;

            #endregion
        }

        prevViewItem = viewItem;
        prevCaptionGroup = newCaptionGroup;

        // Zeichen-Routine für das letzte Element aufrufen
        if (!string.IsNullOrEmpty(prevCaptionGroup) && prevViewItem is { IsDisposed: false } && prevViewItemWithOtherCaption is { }) {
            Draw_Column_Head_Captions_Now(gr, prevViewItemWithOtherCaption, Rectangle.Empty, displayRectangleWoSlider, pccy, pcch, prevCaptionGroup);
        }
    }

    public override int HeightInControl(ListBoxAppearance style, int columnWidth, Design itemdesign) => CaptionHeight;

    protected override Size ComputeUntrimmedCanvasSize(Design itemdesign) => new(CaptionHeight, CaptionHeight);

    private void Draw_Column_Head_Captions_Now(Graphics gr, ColumnViewItem prevViewItemWithOtherCaption, RectangleF positionControl, string prevCaptionGroup) {
        if (prevViewItemWithOtherCaptionLe < positionControl.Right) {
            var r = new Rectangle(prevViewItemWithOtherCaptionLe, positionControl.Top, positionControl.Right - prevViewItemWithOtherCaptionLe, positionControl.Height);
            gr.FillRectangle(new SolidBrush(prevViewItemWithOtherCaption.BackColor_ColumnHead), r);
            gr.FillRectangle(new SolidBrush(Color.FromArgb(80, 200, 200, 200)), r);
            gr.DrawRectangle(Skin.PenLinieKräftig, r);
            Skin.Draw_FormatedText(gr, prevCaptionGroup, null, Alignment.Horizontal_Vertical_Center, r, this, false, prevViewItemWithOtherCaption.Font_Head_Default.Scale(_zoom), Translate);
        }
    }

    #endregion
}