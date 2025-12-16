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
using System;
using System.Drawing;

namespace BlueControls.ItemCollectionList;

/// <summary>
/// Enthält Daten, wie eine Zeile angezeigt werden soll.
/// RowData kann mehrfach in einer Tabelle angezeigt werden.
/// Ein RowItem ist einzigartig, kann aber in mehreren RowData enthalten sein.
/// </summary>
public sealed class RowListItem : RowBackgroundListItem {

    #region Fields

    public static readonly SolidBrush BrushYellowTransparent = new(Color.FromArgb(180, 255, 255, 0));

    #endregion

    #region Constructors

    public RowListItem(RowItem row, string alignsToCaption, ColumnViewCollection? arrangement) : base(Identifier(row, alignsToCaption), arrangement, alignsToCaption) {
        Row = row;
        MarkYellow = false;
    }

    #endregion

    #region Properties

    public ColumnItem? Column { get; set; }

    public bool MarkYellow {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public override string QuickInfo => Row.GetQuickInfo();
    public RowItem Row { get; }
    protected override bool DoSpezialOrder => true;

    #endregion

    #region Methods

    public static string Identifier(RowItem row, string chapter) => chapter.Trim('\\').ToUpperInvariant() + "\\" + row.KeyName;

    public override void Draw_ColumnBackGround(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, States state) {
        base.Draw_ColumnBackGround(gr, viewItem, positionControl, state);

        if (MarkYellow) {
            gr.FillRectangle(BrushYellowTransparent, positionControl);
        }
    }

    public override void Draw_ColumnContent(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, float scale, TranslationType translate, float offsetX, float offsetY, States state) {
        base.Draw_ColumnContent(gr, viewItem, positionControl, scale, translate, offsetX, offsetY, state);

        if (viewItem.Column == null) { return; }

        if (!viewItem.Column.SaveContent) {
            Row.CheckRow();
        }

        if (viewItem.Column == Column) {
            var _tmpCursorRect = new Rectangle((int)positionControl.X + 1, (int)positionControl.Y + 1, (int)positionControl.Width - 2, (int)positionControl.Height - 2);
            Skin.Draw_Back(gr, Design.Table_Cursor, state, _tmpCursorRect, null, false);
            Skin.Draw_Border(gr, Design.Table_Cursor, state, _tmpCursorRect);
        }

        var toDrawd = Row.CellGetString(viewItem.Column);

        var pax = 4.ControlToCanvas(scale);
        var pay = 2.ControlToCanvas(scale);
        positionControl.Inflate(-pax, -pay);

        viewItem.GetRenderer(SheetStyle).Draw(gr, toDrawd, Row, positionControl.ToRect(), translate, (Alignment)viewItem.Column.Align, scale);
    }

    public override int HeightInControl(ListBoxAppearance style, int columnWidth, Design itemdesign) => UntrimmedCanvasSize(itemdesign).Height;

    protected override Size ComputeUntrimmedCanvasSize(Design itemdesign) {
        if (IsDisposed || Row.IsDisposed || Arrangement is null) { return new(16, 16); }

        var drawHeight = 18;

        foreach (var thisViewItem in Arrangement) {
            if (thisViewItem.Column is { IsDisposed: false } tmpc) {
                var renderer = thisViewItem.GetRenderer(SheetStyle);
                drawHeight = Math.Max(drawHeight, renderer.ContentSize(Row.CellGetString(tmpc), tmpc.DoOpticalTranslation).Height);
            }
        }

        drawHeight = Math.Min(drawHeight, 200);
        drawHeight = Math.Max(drawHeight+4, 18);

        return new(100, drawHeight);
    }

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, Design itemdesign, States state, bool drawBorderAndBack, bool translate, float offsetX, float offsetY, float scale) {
        base.DrawExplicit(gr, visibleAreaControl, positionControl, itemdesign, state, drawBorderAndBack, translate, offsetX, offsetY, scale);
        if (Column == null) { return; }

        //var stat = States.Standard;
        //if (Focused()) { stat = States.Standard_HasFocus; }
        var _tmpCursorRect = positionControl.ToRect();
        Pen pen = new(Skin.Color_Border(Design.Table_Cursor, state).SetAlpha(180));
        gr.DrawRectangle(pen, new Rectangle(-1, _tmpCursorRect.Top, _tmpCursorRect.Width + 2, _tmpCursorRect.Height - 1));
    }

    #endregion
}