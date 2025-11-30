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
using BlueBasics.Enums;
using BlueControls.Enums;
using BlueControls.ItemCollectionPad;
using BlueTable;
using System;
using System.Drawing;

namespace BlueControls.ItemCollectionList;

public class RowFormulaListItem : AbstractListItem {

    #region Fields

    private readonly string _layoutFileName;

    private RowItem? _row;

    private Bitmap? _tmpBmp;

    #endregion

    #region Constructors

    /// <summary>
    /// Als Interner Name wird der RowKey als String abgelegt
    /// </summary>
    /// <param name="row"></param>
    /// <param name="layoutId"></param>
    /// <param name="userDefCompareKey"></param>
    public RowFormulaListItem(RowItem row, string layoutId, string userDefCompareKey, object? tag) : base(row.KeyName, true) {
        _row = row;
        _layoutFileName = layoutId;
        UserDefCompareKey = userDefCompareKey;
        Tag = tag;
    }

    /// <summary>
    /// Als Interner Name wird der RowKey als String abgelegt
    /// </summary>
    public RowFormulaListItem(RowItem row, string layoutId, object? tag) : this(row, layoutId, string.Empty, tag) { }

    #endregion

    //public string LayoutId {
    //    get => _layoutId;
    //    set {
    //        if (value == _layoutId) { return; }
    //        _layoutId = value;
    //        RemovePic();
    //    }
    //}

    #region Properties

    public override string QuickInfo {
        get {
            if (_row?.Table is not { IsDisposed: false } db) { return string.Empty; }

            return !string.IsNullOrEmpty(db.RowQuickInfo)
                ? _row.GetQuickInfo().CreateHtmlCodes()
                : _row.CellFirstString().CreateHtmlCodes();
        }
    }

    public RowItem? Row {
        get => _row;
        set {
            if (_row == value) { return; }
            _row = value;
            RemovePic();
        }
    }

    #endregion

    #region Methods

    public override int HeightInControl(ListBoxAppearance style, int columnWidth, Design itemdesign) {
        if (_tmpBmp == null) { GeneratePic(); }
        return _tmpBmp?.Height ?? 200;

        //var sc = ((float)_tmpBmp.Height / _tmpBmp.Width);

        //if (sc > 1) { sc = 1; }

        //return (int)(sc * columnWidth);
    }

    protected override Size ComputeUntrimmedCanvasSize(Design itemdesign) {
        try {
            if (_tmpBmp == null) { GeneratePic(); }
            return _tmpBmp?.Size ?? new Size(200, 200);
        } catch {
            Develop.AbortAppIfStackOverflow();
            return ComputeUntrimmedCanvasSize(itemdesign);
        }
    }

    protected override void DrawExplicit(Graphics gr, Rectangle visibleArea, RectangleF positionInControl, Design itemdesign, States state, bool drawBorderAndBack, bool translate, float offsetX, float offsetY, float scale) {
        if (_tmpBmp == null) { GeneratePic(); }
        if (drawBorderAndBack) {
            Skin.Draw_Back(gr, itemdesign, state, positionInControl.ToRect(), null, false);
        }
        if (_tmpBmp != null) {
            scale = (float)Math.Min(positionInControl.Width / (double)_tmpBmp.Width, positionInControl.Height / (double)_tmpBmp.Height);
            RectangleF r2 = new(
                ((positionInControl.Width - (_tmpBmp.Width * scale)) / 2) + positionInControl.Left,
                ((positionInControl.Height - (_tmpBmp.Height * scale)) / 2) + positionInControl.Top,
                _tmpBmp.Width * scale, _tmpBmp.Height * scale);
            gr.DrawImage(_tmpBmp, r2, new RectangleF(0, 0, _tmpBmp.Width, _tmpBmp.Height), GraphicsUnit.Pixel);
        }
        if (drawBorderAndBack) {
            Skin.Draw_Border(gr, itemdesign, state, positionInControl.ToRect());
        }
    }

    protected override string GetCompareKey() => _row?.CompareKey() ?? string.Empty;

    private void GeneratePic() {
        if (string.IsNullOrEmpty(_layoutFileName) || !_layoutFileName.StartsWith("#") || Row?.Table is not { IsDisposed: false }) {
            _tmpBmp = QuickImage.Get(ImageCode.Warnung, 128);
            return;
        }
        ItemCollectionPadItem pad = new(_layoutFileName);
        pad.ResetVariables();
        var l = pad.ReplaceVariables(Row);
        if (l.Failed) {
            _tmpBmp = QuickImage.Get(ImageCode.Warnung, 128);
            return;
        }

        var canvasUsedArea = pad.CanvasUsedArea.ToRect();
        if (_tmpBmp != null) {
            if (_tmpBmp.Width != canvasUsedArea.Width || _tmpBmp.Height != canvasUsedArea.Height) {
                RemovePic();
            }
        }

        var internalZoom = Math.Min(500 / canvasUsedArea.Width, 500 / canvasUsedArea.Height);
        internalZoom = Math.Min(1, internalZoom);

        _tmpBmp ??= new Bitmap(canvasUsedArea.Width * internalZoom, canvasUsedArea.Height * internalZoom);
        var zoomv = ItemCollectionPadItem.ZoomFitValue(canvasUsedArea, _tmpBmp.Size);
        var centerpos = ItemCollectionPadItem.CenterPos(canvasUsedArea, _tmpBmp.Size, zoomv);
        var slidervalues = ItemCollectionPadItem.SliderValues(canvasUsedArea, zoomv, centerpos);
        //pad.ShowInPrintMode = true;
        //pad.Unselect();
        pad.DrawToBitmap(_tmpBmp, zoomv, slidervalues.X, slidervalues.Y);
    }

    private void RemovePic() {
        _tmpBmp?.Dispose();
        _tmpBmp = null;
    }

    #endregion
}