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
using System;
using System.Drawing;

namespace BlueControls.ItemCollectionList;

/// <summary>
/// Kümmert sich um die Anzeige des Spaltenkopfes
/// </summary>
public sealed class ColumnsHeadListItem : RowBackgroundListItem {

    #region Fields

    public const string Identifier = "ColumnsHeadListItem";

    #endregion

    #region Constructors

    public ColumnsHeadListItem(ColumnViewCollection? arrangement) : base(Identifier, arrangement, string.Empty) => IgnoreYOffset = true;

    #endregion

    #region Properties

    public BlueFont Font_Head_Default => Skin.GetBlueFont(SheetStyle, PadStyles.Hervorgehoben);
    public override string QuickInfo => string.Empty;
    protected override bool DoSpezialOrder => true;

    #endregion

    #region Methods

    public static QuickImage? CaptionBitmap(ColumnItem column) {
        if (string.IsNullOrEmpty(column.CaptionBitmapCode)) { return null; }

        return QuickImage.Get(column.CaptionBitmapCode + "|100");
    }

    public static string CaptionTranslated(string caption) => LanguageTool.DoTranslate(caption, true).Replace("\r", "\r\n");

    public override void Draw_LowerLine(Graphics gr, ColumnLineStyle lin, float left, float right, float bottom) => base.Draw_LowerLine(gr, ColumnLineStyle.Ohne, left, right, bottom);

    public override void DrawColumn(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, float scale, TranslationType translate, float offsetX, float offsetY, States state) {
        base.DrawColumn(gr, viewItem, positionControl, scale, translate, offsetX, offsetY, state);
        if (viewItem.Column is not { IsDisposed: false } column) { return; }

        gr.FillRectangle(GrayBrush, positionControl);

        #region Roten Rand für Split-Spalten

        if (column == column.Table?.Column.ChunkValueColumn) {
            var t = positionControl;
            t.Inflate(-3, -3);
            gr.DrawRectangle(new Pen(Color.Red, 6), t);
        }

        #endregion

        var capTranslated = CaptionTranslated(column.Caption);
        var Font_Head_Default_Scaled = Font_Head_Default.Scale(scale).MeasureString(capTranslated);

        if (CaptionBitmap(column) is { IsError: false } cb) {

            #region Spalte mit Bild zeichnen

            var pos = new Point((int)positionControl.X + (int)((positionControl.Width - Font_Head_Default_Scaled.Width) / 2.0), 3);
            gr.DrawImageInRectAspectRatio(cb, (int)positionControl.X + 2, (int)(pos.Y + Font_Head_Default_Scaled.Height), (int)positionControl.Width - 4, (int)positionControl.Bottom - (int)(pos.Y + Font_Head_Default_Scaled.Height) - 6 - 18);
            // Dann der Text
            gr.TranslateTransform(pos.X, pos.Y);
            Font_Head_Colored(viewItem).Scale(scale).DrawString(gr, capTranslated, 0, 0);
            gr.TranslateTransform(-pos.X, -pos.Y);

            #endregion
        } else {

            #region Spalte ohne Bild zeichnen

            var p4 = 4.CanvasToControl(scale);
            var pos = new Point((int)positionControl.X + (int)((positionControl.Width - Font_Head_Default_Scaled.Height) / 2.0), (int)positionControl.Bottom - p4);
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

    public override int HeightInControl(ListBoxAppearance style, int columnWidth, Design itemdesign) => UntrimmedCanvasSize(itemdesign).Height;

    protected override Size ComputeUntrimmedCanvasSize(Design itemdesign) {
        if (IsDisposed || Arrangement is null) { return new(16, 16); }

        if (!Arrangement.ShowHead) { return new(0, 0); }

        var minH = 16;

        var f = Font_Head_Default;

        foreach (var thisC in Arrangement) {
            if (thisC.Column is { IsDisposed: false } column) {
                var capTranslated = CaptionTranslated(column.Caption);
                var s = f.MeasureString(capTranslated);

                minH = Math.Max(minH, (int)s.Width);
            }
        }

        return new(100, minH + 3);
    }

    #endregion
}