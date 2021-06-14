// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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
        private RowItem _Row;
        private Bitmap _tmpBMP;

        #endregion

        #region Constructors

        public RowFormulaListItem(RowItem row, string layoutID, string userDefCompareKey) : base(string.Empty) {
            _Row = row;
            _LayoutID = layoutID;
            UserDefCompareKey = userDefCompareKey;
        }

        #endregion

        #region Properties

        public string LayoutID {
            get => _LayoutID;
            set {
                if (value == _LayoutID) { return; }
                _LayoutID = value;
                if (_tmpBMP != null) {
                    _tmpBMP.Dispose();
                    _tmpBMP = null;
                }
                //OnChanged();
            }
        }

        public override string QuickInfo => _Row == null ? string.Empty : _Row.CellFirstString().CreateHtmlCodes(true);

        public RowItem Row {
            get => _Row;
            set {
                if (_Row == value) { return; }
                _Row = value;
                removePic();
                //OnChanged();
            }
        }

        #endregion

        #region Methods

        public override int HeightForListBox(enBlueListBoxAppearance style, int columnWidth) => (int)(columnWidth * 0.8);

        protected override Size ComputeSizeUntouchedForListBox() => new(300, 300);

        protected override void DrawExplicit(Graphics GR, Rectangle PositionModified, enDesign itemdesign, enStates vState, bool DrawBorderAndBack, bool Translate) {
            if (_tmpBMP == null) { GeneratePic(); }
            if (DrawBorderAndBack) {
                Skin.Draw_Back(GR, itemdesign, vState, PositionModified, null, false);
            }
            if (_tmpBMP != null) {
                var scale = (float)Math.Min(PositionModified.Width / (double)_tmpBMP.Width, PositionModified.Height / (double)_tmpBMP.Height);
                RectangleF r2 = new(((PositionModified.Width - (_tmpBMP.Width * scale)) / 2) + PositionModified.Left, ((PositionModified.Height - (_tmpBMP.Height * scale)) / 2) + PositionModified.Top, _tmpBMP.Width * scale, _tmpBMP.Height * scale);
                GR.DrawImage(_tmpBMP, r2, new RectangleF(0, 0, _tmpBMP.Width, _tmpBMP.Height), GraphicsUnit.Pixel);
            }
            if (DrawBorderAndBack) {
                Skin.Draw_Border(GR, itemdesign, vState, PositionModified);
            }
        }

        protected override string GetCompareKey() => _Row.CellFirstString();

        private void GeneratePic() {
            if (string.IsNullOrEmpty(_LayoutID) || !_LayoutID.StartsWith("#")) {
                _tmpBMP = (Bitmap)QuickImage.Get(enImageCode.Warnung, 128).BMP.Clone();
                return;
            }
            CreativePad _pad = new(new ItemCollectionPad(_LayoutID, Row.Database, Row.Key));
            var mb = _pad.Item.MaxBounds(null);
            if (_tmpBMP != null) {
                if (_tmpBMP.Width != mb.Width || _tmpBMP.Height != mb.Height) {
                    _tmpBMP.Dispose();
                    _tmpBMP = null;
                }
            }
            if (_tmpBMP == null) { _tmpBMP = new Bitmap((int)mb.Width, (int)mb.Height); }
            var zoomv = _pad.ZoomFitValue(mb, false, _tmpBMP.Size);
            var centerpos = _pad.CenterPos(mb, false, _tmpBMP.Size, zoomv);
            var slidervalues = _pad.SliderValues(mb, zoomv, centerpos);
            _pad.ShowInPrintMode = true;
            _pad.Unselect();
            _pad.DrawCreativePadToBitmap(_tmpBMP, enStates.Standard, zoomv, (double)slidervalues.X, (double)slidervalues.Y, null);
        }

        private void removePic() {
            if (_tmpBMP != null) {
                _tmpBMP.Dispose();
                _tmpBMP = null;
            }
        }

        #endregion
    }
}