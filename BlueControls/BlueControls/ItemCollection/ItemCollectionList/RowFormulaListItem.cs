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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueDatabase;
using System;
using System.Drawing;

namespace BlueControls.ItemCollection {

    public class RowFormulaListItem : BasicListItem {

        #region Fields

        private string _LayoutID;

        private RowItem? _Row;

        private Bitmap? _tmpBmp;

        #endregion

        #region Constructors

        public RowFormulaListItem(RowItem? row, string internalname, string layoutID, string userDefCompareKey) : base(internalname) {
            _Row = row;
            _LayoutID = layoutID;
            UserDefCompareKey = userDefCompareKey;
        }

        public RowFormulaListItem(RowItem? row, string layoutID, string userDefCompareKey) : this(row, string.Empty, layoutID, userDefCompareKey) { }

        #endregion

        #region Properties

        public string LayoutID {
            get => _LayoutID;
            set {
                if (value == _LayoutID) { return; }
                _LayoutID = value;
                RemovePic();
            }
        }

        public override string QuickInfo {
            get {
                if (_Row == null) { return string.Empty; }

                return !string.IsNullOrEmpty(_Row.Database.ZeilenQuickInfo)
                    ? _Row.QuickInfo.CreateHtmlCodes(true)
                    : _Row.CellFirstString().CreateHtmlCodes(true);
            }
        }

        public RowItem? Row {
            get => _Row;
            set {
                if (_Row == value) { return; }
                _Row = value;
                RemovePic();
            }
        }

        #endregion

        #region Methods

        public override object Clone() {
            var x = new RowFormulaListItem(_Row, Internal, _LayoutID, UserDefCompareKey);
            x.CloneBasicStatesFrom(this);
            return x;
        }

        public override int HeightForListBox(enBlueListBoxAppearance style, int columnWidth) {
            if (_tmpBmp == null) { GeneratePic(); }
            return _tmpBmp == null ? 200 : _tmpBmp.Height;

            //var sc = ((float)_tmpBmp.Height / _tmpBmp.Width);

            //if (sc > 1) { sc = 1; }

            //return (int)(sc * columnWidth);
        }

        protected override Size ComputeSizeUntouchedForListBox() {
            if (_tmpBmp == null) { GeneratePic(); }
            return _tmpBmp == null ? new Size(200, 200) : _tmpBmp.Size;

            //var sc = ((float)_tmpBmp.Height / _tmpBmp.Width);

            //if (sc > 1) { sc = 1; }

            //return new Size(300, (int)(sc * 300));
        }

        protected override void DrawExplicit(Graphics GR, Rectangle PositionModified, enDesign itemdesign, enStates vState, bool DrawBorderAndBack, bool Translate) {
            if (_tmpBmp == null) { GeneratePic(); }
            if (DrawBorderAndBack) {
                Skin.Draw_Back(GR, itemdesign, vState, PositionModified, null, false);
            }
            if (_tmpBmp != null) {
                var scale = (float)Math.Min(PositionModified.Width / (double)_tmpBmp.Width, PositionModified.Height / (double)_tmpBmp.Height);
                RectangleF r2 = new(((PositionModified.Width - (_tmpBmp.Width * scale)) / 2) + PositionModified.Left, ((PositionModified.Height - (_tmpBmp.Height * scale)) / 2) + PositionModified.Top, _tmpBmp.Width * scale, _tmpBmp.Height * scale);
                GR.DrawImage(_tmpBmp, r2, new RectangleF(0, 0, _tmpBmp.Width, _tmpBmp.Height), GraphicsUnit.Pixel);
            }
            if (DrawBorderAndBack) {
                Skin.Draw_Border(GR, itemdesign, vState, PositionModified);
            }
        }

        protected override string GetCompareKey() => _Row.CompareKey();

        private void GeneratePic() {
            if (string.IsNullOrEmpty(_LayoutID) || !_LayoutID.StartsWith("#")) {
                _tmpBmp = QuickImage.Get(enImageCode.Warnung, 128);
                return;
            }
            CreativePad _pad = new(new ItemCollectionPad(_LayoutID, Row.Database, Row.Key));
            var mb = _pad.Item.MaxBounds(null);
            if (_tmpBmp != null) {
                if (_tmpBmp.Width != mb.Width || _tmpBmp.Height != mb.Height) {
                    RemovePic();
                }
            }

            var InternalZoom = Math.Min(500 / mb.Width, 500 / mb.Height);
            InternalZoom = Math.Min(1, InternalZoom);

            if (_tmpBmp == null) { _tmpBmp = new Bitmap((int)(mb.Width * InternalZoom), (int)(mb.Height * InternalZoom)); }
            var zoomv = _pad.ZoomFitValue(mb, false, _tmpBmp.Size);
            var centerpos = _pad.CenterPos(mb, false, _tmpBmp.Size, zoomv);
            var slidervalues = _pad.SliderValues(mb, zoomv, centerpos);
            _pad.ShowInPrintMode = true;
            _pad.Unselect();
            _pad.Item.DrawCreativePadToBitmap(_tmpBmp, enStates.Standard, zoomv, slidervalues.X, slidervalues.Y, null);
        }

        private void RemovePic() {
            if (_tmpBmp != null) {
                _tmpBmp.Dispose();
                _tmpBmp = null;
            }
        }

        #endregion
    }
}