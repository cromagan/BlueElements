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
using System;
using System.Drawing;

namespace BlueTable;

/// <summary>
/// Kümmert sich um die Anzeige des Spaltenkopfes
/// </summary>
public sealed class ColumnsHeadListItem : RowBackgroundListItem {

    #region Constructors

    public ColumnsHeadListItem(ColumnViewCollection? arrangement) : base(string.Empty, arrangement, string.Empty) {
    }

    #endregion

    #region Properties

    public BlueFont Font_Head_Default => Skin.GetBlueFont(SheetStyle, PadStyles.Hervorgehoben);
    public override string QuickInfo => string.Empty;

    #endregion

    #region Methods

    public static QuickImage? CaptionBitmap(ColumnItem column) {
        if (string.IsNullOrEmpty(column.CaptionBitmapCode)) { return null; }

        return QuickImage.Get(column.CaptionBitmapCode + "|100");
    }

    public override void DrawColumn(Graphics gr, ColumnViewItem viewItem, RectangleF positionModified, float scale, TranslationType translate, float shiftX, float shiftY) {
        base.DrawColumn(gr, viewItem, positionModified, scale, translate, shiftX, shiftY);
        if (viewItem.Column is not { IsDisposed: false } column) { return; }

        #region Recude-Button zeichnen

        if (positionModified.Width > 70 || viewItem.Reduced) {
            // Anpassen der Reduce-Button-Position

            var p14 = ZoomPad.GetPix(14, scale);

            var origReduceButtonLocation = new Rectangle((int)positionModified.Right - p14, (int)positionModified.Top, p14, p14);

            gr.DrawImage(
                viewItem.Reduced ? QuickImage.Get("Pfeil_Rechts|" + origReduceButtonLocation.Width + "|||FF0000|||||20")
                                : QuickImage.Get("Pfeil_Links|" + origReduceButtonLocation.Width + "||||||||75"),
                origReduceButtonLocation.Left,
                origReduceButtonLocation.Top
            );
        }

        #endregion

        #region Roten Rand für Split-Spalten

        if (column == column.Table?.Column.ChunkValueColumn) {
            var t = positionModified;
            t.Inflate(-3, -3);
            gr.DrawRectangle(new Pen(Color.Red, 6), t);
        }

        #endregion

        var capTranslated = CaptionTranslated(column);
        var Font_Head_Default_Scaled = Font_Head_Default.Scale(scale).MeasureString(capTranslated);

        if (CaptionBitmap(column) is { IsError: false } cb) {

            #region Spalte mit Bild zeichnen

            var pos = new Point((int)positionModified.X + (int)((positionModified.Width - Font_Head_Default_Scaled.Width) / 2.0), 3);
            gr.DrawImageInRectAspectRatio(cb, (int)positionModified.X + 2, (int)(pos.Y + Font_Head_Default_Scaled.Height), (int)positionModified.Width - 4, (int)positionModified.Bottom - (int)(pos.Y + Font_Head_Default_Scaled.Height) - 6 - 18);
            // Dann der Text
            gr.TranslateTransform(pos.X, pos.Y);
            Font_Head_Colored(viewItem).Scale(scale).DrawString(gr, capTranslated, 0, 0);
            gr.TranslateTransform(-pos.X, -pos.Y);

            #endregion
        } else {

            #region Spalte ohne Bild zeichnen

            var p4 = ZoomPad.GetPix(4, scale);
            var pos = new Point((int)positionModified.X + (int)((positionModified.Width - Font_Head_Default_Scaled.Height) / 2.0), (int)positionModified.Bottom - p4);
            gr.TranslateTransform(pos.X, pos.Y);
            gr.RotateTransform(-90);
            Font_Head_Colored(viewItem).Scale(scale).DrawString(gr, capTranslated, 0, 0);
            gr.TranslateTransform(-pos.X, -pos.Y);
            gr.ResetTransform();

            #endregion
        }
    }

    public BlueFont Font_Head_Colored(ColumnViewItem column) {
        if (column != null) {
            var baseFont = Font_Head_Default;
            return BlueFont.Get(baseFont.FontName, baseFont.Size, baseFont.Bold, baseFont.Italic, baseFont.Underline, baseFont.StrikeOut, column.FontColor_Caption, Color.Transparent, Color.Transparent);
        } else {
            return Font_Head_Default;
        }
    }

    public override int HeightForListBox(ListBoxAppearance style, int columnWidth, Design itemdesign) => SizeUntouchedForListBox(itemdesign).Height;

    protected override Size ComputeSizeUntouchedForListBox(Design itemdesign) {
        if (IsDisposed || Arrangement is null) { return new(16, 16); }

        if (!Arrangement.ShowHead) { return new(0, 0); }

        var minH = 16;

        var f = Font_Head_Default;

        foreach (var thisC in Arrangement) {
            if (thisC.Column is { IsDisposed: false } column) {
                var capTranslated = CaptionTranslated(column);
                var s = f.MeasureString(capTranslated);

                minH = Math.Max(minH, (int)s.Width);
            }
        }

        return new(100, minH + 3);
    }

    private static string CaptionTranslated(ColumnItem column) => LanguageTool.DoTranslate(column.Caption, true).Replace("\r", "\r\n");

    #endregion
}