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
using BlueBasics.Interfaces;
using BlueControls.Enums;
using BlueTable;
using BlueTable.Enums;
using System;
using System.Drawing;

namespace BlueControls.ItemCollectionList;

// TODO: UNUSED!!!!!
public sealed class RowInfoListItem : RowBackgroundListItem {

    #region Fields

    public const string Identifier = "RowInfoListItem";

    #endregion

    #region Constructors

    public RowInfoListItem(ColumnViewCollection? arrangement) : base(Identifier, arrangement, string.Empty) => IgnoreYOffset = true;

    #endregion

    #region Properties

    public BlueFont Font_Default => Skin.GetBlueFont(SheetStyle, PadStyles.Standard);

    protected override bool DoSpezialOrder => true;

    #endregion

    #region Methods

    public override void Draw_ColumnContent(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, float scale, TranslationType translate, float offsetX, float offsetY, States state) {
        base.Draw_ColumnContent(gr, viewItem, positionControl, scale, translate, offsetX, offsetY, state);

        if (viewItem.Column is not { } c) { return; }
        if (c.Table is not { } tb) { return; }

        var pxx = 9.CanvasToControl(scale);
        var p1 = 1.CanvasToControl(scale);
        var p1pic = p1 + pxx;
        var p2pic = p1 + pxx + p1 + pxx;

        var editable = CellCollection.IsCellEditable(c, tb.Row.First(), "dummy");
        var bearbInfo = QuickImage.Get(ImageCode.Stift, pxx);
        var okInfo = QuickImage.Get(ImageCode.Häkchen, pxx);

        if (!string.IsNullOrEmpty(editable)) { bearbInfo = QuickImage.Get(ImageCode.Kreuz, pxx); }

        if (!c.IsOk()) { bearbInfo = QuickImage.Get(ImageCode.Kritisch, pxx); }
        if (!string.IsNullOrEmpty(c.LinkedTableTableName)) {
        }

        gr.DrawImage(okInfo, new Point((int)positionControl.Left + p1, (int)positionControl.Top + p1));
        gr.DrawImage(bearbInfo, new Point((int)positionControl.Left + p1pic, (int)positionControl.Top + p1));

        var txz = c.LinkedTableTableName.FileNameWithoutSuffix();
        //if (string.IsNullOrEmpty(txz)) { txz = "[DIESE]"; }

        var r = new RectangleF(positionControl.Left, positionControl.Top + p1pic,
           positionControl.Width, positionControl.Height - p1pic);

        Skin.Draw_FormatedText(gr, txz, null, Alignment.VerticalCenter_Left, r.ToRect(), null, false, Font_Default.Scale(scale * 0.8f), false);

        //string toDrawd;
        //var doWhiteAfter = true;
        ////QuickImage? plusszeichen;
        //if (viewItem.Column.IsFirst) {
        //    toDrawd = "[Neue Zeile]";
        //    //plusszeichen = QuickImage.Get(ImageCode.PlusZeichen, pxx);
        //    doWhiteAfter = false;
        //} else {
        //    toDrawd = FilterCollection.InitValue(viewItem.Column, false, false, [.. FilterCombined]) ?? string.Empty;
        //    //plusszeichen = QuickImage.Get(ImageCode.PlusZeichen, pxx, Color.Transparent, Color.Transparent, 200);
        //}

        //if (!doWhiteAfter) {
        //    gr.FillRectangle(GrayBrush2, positionControl);
        //}

        //if (!string.IsNullOrEmpty(toDrawd)) {
        //    gr.SetClip(positionControl);
        //    viewItem.GetRenderer(SheetStyle).Draw(gr, toDrawd, null, positionControl.ToRect(), translate, (Alignment)viewItem.Column.Alignx, scale);

        //    //gr.DrawImage(plusszeichen, new Point((int)positionControl.Left + p1, (int)positionControl.Top + p1));
        //    gr.ResetClip();
        //}

        //if (doWhiteAfter) {
        //    gr.FillRectangle(GrayBrush2, positionControl);
        //}
    }

    public override int HeightInControl(ListBoxAppearance style, int columnWidth, Design itemdesign) => UntrimmedCanvasSize(itemdesign).Height;

    public override string QuickInfoForColumn(ColumnViewItem cvi) {
        if (cvi.Column is not { } c) { return string.Empty; }
        if (c.Table is not { } tb) { return string.Empty; }

        var t = $"Ziel-Tabelle: {c.LinkedTableTableName.FileNameWithoutSuffix()}\r";
        t = t + $"Fehler: {c.ErrorReason()}\r";
        t = t + $"Bearbeitung: {CellCollection.IsCellEditable(c, tb.Row.First(), "dummy")}";

        return t;
    }

    protected override Size ComputeUntrimmedCanvasSize(Design itemdesign) => new(22, 22);

    #endregion
}