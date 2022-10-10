// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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

namespace BlueControls.ItemCollection.ItemCollectionList;

public class RowFormulaListItem : BasicListItem {

    #region Fields

    private string _layoutId;

    private RowItem _row;

    private Bitmap? _tmpBmp;

    #endregion

    #region Constructors

    /// <summary>
    /// Als Interner Name wird der RowKey als String abgelegt
    /// </summary>
    /// <param name="row"></param>
    /// <param name="layoutId"></param>
    /// <param name="userDefCompareKey"></param>
    public RowFormulaListItem(RowItem row, string layoutId, string userDefCompareKey) : base(row.Key.ToString(), true) {
        _row = row;
        _layoutId = layoutId;
        UserDefCompareKey = userDefCompareKey;
    }

    /// <summary>
    /// Als Interner Name wird der RowKey als String abgelegt
    /// </summary>
    public RowFormulaListItem(RowItem row, string layoutId) : this(row, layoutId, string.Empty) { }

    #endregion

    #region Properties

    public string LayoutId {
        get => _layoutId;
        set {
            if (value == _layoutId) { return; }
            _layoutId = value;
            RemovePic();
        }
    }

    public override string QuickInfo {
        get {
            if (_row == null) { return string.Empty; }

            return !string.IsNullOrEmpty(_row.Database.ZeilenQuickInfo)
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

    public override object Clone() {
        var x = new RowFormulaListItem(_row, _layoutId, UserDefCompareKey);
        x.CloneBasicStatesFrom(this);
        return x;
    }

    public override int HeightForListBox(BlueListBoxAppearance style, int columnWidth) {
        if (_tmpBmp == null) { GeneratePic(); }
        return _tmpBmp?.Height ?? 200;

        //var sc = ((float)_tmpBmp.Height / _tmpBmp.Width);

        //if (sc > 1) { sc = 1; }

        //return (int)(sc * columnWidth);
    }

    protected override Size ComputeSizeUntouchedForListBox() {
        try {
            if (_tmpBmp == null) { GeneratePic(); }
            return _tmpBmp == null ? new Size(200, 200) : _tmpBmp.Size;
        } catch {
            return ComputeSizeUntouchedForListBox();
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

    protected override string GetCompareKey() => _row.CompareKey();

    private void GeneratePic() {
        if (string.IsNullOrEmpty(_layoutId) || !_layoutId.StartsWith("#")) {
            _tmpBmp = QuickImage.Get(ImageCode.Warnung, 128);
            return;
        }
        ItemCollectionPad pad = new(_layoutId, Row.Database, Row.Key);
        var mb = pad.MaxBounds(null);
        if (_tmpBmp != null) {
            if (_tmpBmp.Width != mb.Width || _tmpBmp.Height != mb.Height) {
                RemovePic();
            }
        }

        var internalZoom = Math.Min(500 / mb.Width, 500 / mb.Height);
        internalZoom = Math.Min(1, internalZoom);

        if (_tmpBmp == null) { _tmpBmp = new Bitmap((int)(mb.Width * internalZoom), (int)(mb.Height * internalZoom)); }
        var zoomv = ItemCollectionPad.ZoomFitValue(mb, _tmpBmp.Size);
        var centerpos = ItemCollectionPad.CenterPos(mb, _tmpBmp.Size, zoomv);
        var slidervalues = ItemCollectionPad.SliderValues(mb, zoomv, centerpos);
        //pad.ShowInPrintMode = true;
        //pad.Unselect();
        pad.DrawCreativePadToBitmap(_tmpBmp, States.Standard, zoomv, slidervalues.X, slidervalues.Y, null);
    }

    private void RemovePic() {
        if (_tmpBmp != null) {
            _tmpBmp.Dispose();
            _tmpBmp = null;
        }
    }

    #endregion
}