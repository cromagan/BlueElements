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
using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueControls.Enums;
using BlueTable.Classes;
using BlueTable.Enums;
using BlueTable.EventArgs;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace BlueControls.Classes.ItemCollectionList.TableItems;

/// <summary>
/// Enthält Daten, wie eine Zeile angezeigt werden soll.
/// RowData kann mehrfach in einer Tabelle angezeigt werden.
/// Ein RowItem ist einzigartig, kann aber in mehreren RowData enthalten sein.
/// </summary>
public sealed class RowListItem : RowBackgroundListItem {

    #region Fields

    public static readonly SolidBrush BrushYellowTransparent = new(Color.FromArgb(180, 255, 255, 0));
    public static readonly HatchBrush RedStripesBrush1 = new HatchBrush(HatchStyle.DiagonalCross, Color.FromArgb(30, 255, 0, 0), Color.FromArgb(5, 255, 0, 0));
    public static readonly HatchBrush RedStripesBrush2 = new HatchBrush(HatchStyle.DarkVertical, Color.FromArgb(80, 255, 100, 0), Color.FromArgb(80, 255, 0, 0));

    private static readonly Brush BrushBrighten = new SolidBrush(Color.FromArgb(128, 255, 255, 255));
    private static readonly Brush BrushDarken = new SolidBrush(Color.FromArgb(128, 0, 0, 0));
    private static readonly Pen PenBrighten = new Pen(Color.FromArgb(128, 255, 255, 255));
    private static readonly Pen PenDarken = new Pen(Color.FromArgb(128, 0, 0, 0));
    private RowPrepareFormulaEventArgs? _rowCheckedEventArgs = null;

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

    public RowItem Row { get; }
    protected override bool DoSpezialOrder => true;

    #endregion

    #region Methods

    public static void ColumnBackGround(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, States state) {
        switch (viewItem.BackgroundStyle) {
            case ColumnBackgroundStyle.None:
            case ColumnBackgroundStyle.Brighten:
            case ColumnBackgroundStyle.Darken:
                break;

            case ColumnBackgroundStyle.PopIn:

                for (var z = 0; z < 4; z++) {
                    gr.DrawLine(PenDarken, positionControl.Left + z, positionControl.Top + z, positionControl.Right - z, positionControl.Top + z);
                    gr.DrawLine(PenDarken, positionControl.Left + z, positionControl.Top + z, positionControl.Left + z, positionControl.Bottom - z);
                    gr.DrawLine(PenBrighten, positionControl.Right - z, positionControl.Top + z, positionControl.Right - z, positionControl.Bottom - z);
                    gr.DrawLine(PenBrighten, positionControl.Left + z, positionControl.Bottom - z, positionControl.Right - z, positionControl.Bottom - z);
                }
                break;

            case ColumnBackgroundStyle.PopOut:

                for (var z = 0; z < 4; z++) {
                    gr.DrawLine(PenBrighten, positionControl.Left + z, positionControl.Top + z, positionControl.Right - z, positionControl.Top + z);
                    gr.DrawLine(PenBrighten, positionControl.Left + z, positionControl.Top + z, positionControl.Left + z, positionControl.Bottom - z);
                    gr.DrawLine(PenDarken, positionControl.Right - z, positionControl.Top + z, positionControl.Right - z, positionControl.Bottom - z);
                    gr.DrawLine(PenDarken, positionControl.Left + z, positionControl.Bottom - z, positionControl.Right - z, positionControl.Bottom - z);
                }
                break;
        }
    }

    public static void ColumnOverlay(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, States state) {
        switch (viewItem.BackgroundStyle) {
            case ColumnBackgroundStyle.None:
            case ColumnBackgroundStyle.PopIn:
            case ColumnBackgroundStyle.PopOut:
                break;

            case ColumnBackgroundStyle.Brighten:

                gr.FillRectangle(BrushBrighten, positionControl);
                break;

            case ColumnBackgroundStyle.Darken:
                gr.FillRectangle(BrushDarken, positionControl);
                break;
        }
    }

    public static string Identifier(RowItem row, string chapter) => chapter.Trim('\\').ToUpperInvariant() + "\\" + row.KeyName;

    public static string QuickInfoText(ColumnItem? col, string additionalText) {
        if (col?.Table is not { IsDisposed: false }) { return string.Empty; }

        var T = string.Empty;
        if (!string.IsNullOrEmpty(col.QuickInfo)) { T += col.QuickInfo; }

        if (col.Table.IsAdministrator()) {
            if (!string.IsNullOrEmpty(col.AdminInfo)) { T = T + "<br><br><b><u>Administrator-Info:</b></u><br>" + col.AdminInfo; }
            //if (col.ColumnTags.Count > 0) { T = T + "<br><br><b><u>Spalten-Tags:</b></u><br>" + col.ColumnTags.JoinWith("<br>"); }
            //T = T + "<br><br>" + ColumnEditor.ColumnUsage(col);
        }

        T = T.Trim();
        T = T.Trim("<br>");
        T = T.Trim();
        if (!string.IsNullOrEmpty(T) && !string.IsNullOrEmpty(additionalText)) {
            T = "<b><u>" + additionalText + "</b></u><br><br>" + T;
        }
        return T;
    }

    public static string UndoText(ColumnItem? column, RowItem? row) {
        if (column?.Table is not { IsDisposed: false } tb) { return string.Empty; }

        if (tb.Undo.Count == 0) { return string.Empty; }

        var cellKey = CellCollection.KeyOfCell(column, row);
        var t = string.Empty;
        for (var z = tb.Undo.Count - 1; z >= 0; z--) {
            if (tb.Undo[z] != null && tb.Undo[z].CellKey == cellKey) {
                t = t + tb.Undo[z].UndoTextTableMouseOver() + "<br>";
            }
        }
        t = t.Trim("<br>");
        t = t.Trim("<hr>");
        t = t.Trim("<br>");
        t = t.Trim("<hr>");
        return t;
    }

    public override void Draw_ColumnBackGround(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, States state) {
        base.Draw_ColumnBackGround(gr, viewItem, positionControl, state);

        ColumnBackGround(gr, viewItem, positionControl, state);

        if (_rowCheckedEventArgs?.RowColor is { } c) {
            gr.FillRectangle(c, positionControl);
        }

        if (Generic.IsAdministrator()) {
            if (RowCollection.FailedRows.ContainsKey(Row)) {
                gr.FillRectangle(RedStripesBrush2, positionControl);
            } else if (Row.NeedsRowUpdate()) {
                gr.FillRectangle(RedStripesBrush1, positionControl);
            }
        }

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

    public override void Draw_ColumnOverlay(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, States state) {
        base.Draw_ColumnOverlay(gr, viewItem, positionControl, state);

        ColumnOverlay(gr, viewItem, positionControl, state);
    }

    public override int HeightInControl(ListBoxAppearance style, int columnWidth, Design itemdesign) => UntrimmedCanvasSize(itemdesign).Height;

    public override string QuickInfoForColumn(ColumnViewItem cvi) {
        if (cvi.Column is not { } column) { return string.Empty; }
        if (column.Table is not { } tb) { return string.Empty; }

        if (column.RelationType == RelationType.CellValues) {
            if (column.LinkedTable == null) { return "Verknüpfung zur Ziel-Tabelle fehlerhaft."; }

            var t = string.Empty;
            var (lcolumn, _, info, _) = Row.LinkedCellData(column, true, false);
            if (lcolumn != null) { t = QuickInfoText(lcolumn, column.ReadableText() + " bei " + lcolumn.ReadableText() + ":"); }

            if (!string.IsNullOrEmpty(info) && tb.IsAdministrator()) {
                if (string.IsNullOrEmpty(QuickInfo)) { t += "\r\n"; }
                t = "Verlinkungs-Status: " + info;
            }

            return t;
        }

        if (tb.IsAdministrator()) {
            return UndoText(column, Row);
        }

        return string.Empty;
    }

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
        drawHeight = Math.Max(drawHeight + 4, 18);

        return new(100, drawHeight);
    }

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, Design itemdesign, States state, bool drawBorderAndBack, bool translate, float offsetX, float offsetY, float zoom) {
        if (Row.Table?.ChangesRowColor == true) {
            _rowCheckedEventArgs = Row.CheckRow();
        }

        base.DrawExplicit(gr, visibleAreaControl, positionControl, itemdesign, state, drawBorderAndBack, translate, offsetX, offsetY, zoom);
        if (Column == null) { return; }

        //var stat = States.Standard;
        //if (Focused()) { stat = States.Standard_HasFocus; }
        var _tmpCursorRect = positionControl.ToRect();
        var pen = new Pen(Skin.Color_Border(Design.Table_Cursor, state).SetAlpha(180));
        gr.DrawRectangle(pen, new Rectangle(-1, _tmpCursorRect.Top, _tmpCursorRect.Width + 2, _tmpCursorRect.Height - 1));
    }

    #endregion
}