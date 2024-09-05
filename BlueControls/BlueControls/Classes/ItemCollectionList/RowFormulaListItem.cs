// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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
using BlueDatabase;
using System;
using System.Drawing;
using BlueControls.CellRenderer;

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
    public RowFormulaListItem(RowItem row, string layoutId, string userDefCompareKey) : base(row.KeyName, true) {
        _row = row;
        _layoutFileName = layoutId;
        UserDefCompareKey = userDefCompareKey;
    }

    /// <summary>
    /// Als Interner Name wird der RowKey als String abgelegt
    /// </summary>
    public RowFormulaListItem(RowItem row, string layoutId) : this(row, layoutId, string.Empty) { }

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
            if (_row?.Database is not { IsDisposed: false } db) { return string.Empty; }

            return !string.IsNullOrEmpty(db.ZeilenQuickInfo)
                ? _row.QuickInfo.CreateHtmlCodes(true)
                : _row.CellFirstString().CreateHtmlCodes(true);
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

    public override int HeightForListBox(ListBoxAppearance style, int columnWidth, Design itemdesign, Renderer_Abstract renderer) {
        if (_tmpBmp == null) { GeneratePic(); }
        return _tmpBmp?.Height ?? 200;

        //var sc = ((float)_tmpBmp.Height / _tmpBmp.Width);

        //if (sc > 1) { sc = 1; }

        //return (int)(sc * columnWidth);
    }

    protected override Size ComputeSizeUntouchedForListBox(Design itemdesign) {
        try {
            if (_tmpBmp == null) { GeneratePic(); }
            return _tmpBmp?.Size ?? new Size(200, 200);
        } catch {
            Develop.CheckStackForOverflow();
            return ComputeSizeUntouchedForListBox(itemdesign);
        }
    }

    protected override void DrawExplicit(Graphics gr, Rectangle positionModified, Design itemdesign, States vState, bool drawBorderAndBack, bool translate) {
        if (_tmpBmp == null) { GeneratePic(); }
        if (drawBorderAndBack) {
            Skin.Draw_Back(gr, itemdesign, vState, positionModified, null, false);
        }
        if (_tmpBmp != null) {
            var scale = (float)Math.Min(positionModified.Width / (double)_tmpBmp.Width, positionModified.Height / (double)_tmpBmp.Height);
            RectangleF r2 = new(
                ((positionModified.Width - (_tmpBmp.Width * scale)) / 2) + positionModified.Left,
                ((positionModified.Height - (_tmpBmp.Height * scale)) / 2) + positionModified.Top,
                _tmpBmp.Width * scale, _tmpBmp.Height * scale);
            gr.DrawImage(_tmpBmp, r2, new RectangleF(0, 0, _tmpBmp.Width, _tmpBmp.Height), GraphicsUnit.Pixel);
        }
        if (drawBorderAndBack) {
            Skin.Draw_Border(gr, itemdesign, vState, positionModified);
        }
    }

    protected override string GetCompareKey() => _row?.CompareKey() ?? string.Empty;

    private void GeneratePic() {
        if (string.IsNullOrEmpty(_layoutFileName) || !_layoutFileName.StartsWith("#") || Row?.Database is not { IsDisposed: false }) {
            _tmpBmp = QuickImage.Get(ImageCode.Warnung, 128);
            return;
        }
        ItemCollectionPad.ItemCollectionPad pad = new(_layoutFileName);
        pad.ResetVariables();
        var l = pad.ReplaceVariables(Row);
        if (!l.AllOk) {
            _tmpBmp = QuickImage.Get(ImageCode.Warnung, 128);
            return;
        }

        var mb = pad.MaxBounds(string.Empty).ToRect();
        if (_tmpBmp != null) {
            if (_tmpBmp.Width != mb.Width || _tmpBmp.Height != mb.Height) {
                RemovePic();
            }
        }

        var internalZoom = Math.Min(500 / mb.Width, 500 / mb.Height);
        internalZoom = Math.Min(1, internalZoom);

        _tmpBmp ??= new Bitmap(mb.Width * internalZoom, mb.Height * internalZoom);
        var zoomv = ItemCollectionPad.ItemCollectionPad.ZoomFitValue(mb, _tmpBmp.Size);
        var centerpos = ItemCollectionPad.ItemCollectionPad.CenterPos(mb, _tmpBmp.Size, zoomv);
        var slidervalues = ItemCollectionPad.ItemCollectionPad.SliderValues(mb, zoomv, centerpos);
        //pad.ShowInPrintMode = true;
        //pad.Unselect();
        pad.DrawCreativePadToBitmap(_tmpBmp, States.Standard, zoomv, slidervalues.X, slidervalues.Y, string.Empty);
    }

    private void RemovePic() {
        if (_tmpBmp != null) {
            _tmpBmp?.Dispose();
            _tmpBmp = null;
        }
    }

    #endregion
}