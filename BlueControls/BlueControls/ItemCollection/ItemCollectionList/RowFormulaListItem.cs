#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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
#endregion

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueDatabase;
using System;
using System.Drawing;

namespace BlueControls.ItemCollection {
    public class RowFormulaListItem : BasicListItem {
        #region  Variablen-Deklarationen 

        private RowItem _Row;
        private Bitmap _tmpBMP;
        private string _LayoutID;

        #endregion


        #region  Event-Deklarationen + Delegaten 

        #endregion


        #region  Construktor + Initialize 


        public RowFormulaListItem(RowItem row, string layoutID, string userDefCompareKey) : base(string.Empty) {
            _Row = row;
            _LayoutID = layoutID;
            UserDefCompareKey = userDefCompareKey;
        }


        #endregion


        public override string QuickInfo {
            get {
                if (_Row == null) { return string.Empty; }
                return _Row.CellFirstString().CreateHtmlCodes(true);
            }
        }

        public string LayoutID {
            get {
                return _LayoutID;
            }
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


        public RowItem Row {
            get {
                return _Row;
            }
            set {

                if (_Row == value) { return; }

                _Row = value;

                removePic();

                //OnChanged();
            }
        }


        private void removePic() {
            if (_tmpBMP != null) {
                _tmpBMP.Dispose();
                _tmpBMP = null;
            }
        }


        protected override void DrawExplicit(Graphics GR, Rectangle PositionModified, enDesign itemdesign, enStates vState, bool DrawBorderAndBack, bool Translate) {
            if (_tmpBMP == null) { GeneratePic(); }

            if (DrawBorderAndBack) {
                Skin.Draw_Back(GR, itemdesign, vState, PositionModified, null, false);
            }


            if (_tmpBMP != null) {
                var scale = (float)Math.Min(PositionModified.Width / (double)_tmpBMP.Width, PositionModified.Height / (double)_tmpBMP.Height);
                var r2 = new RectangleF((PositionModified.Width - _tmpBMP.Width * scale) / 2 + PositionModified.Left, (PositionModified.Height - _tmpBMP.Height * scale) / 2 + PositionModified.Top, _tmpBMP.Width * scale, _tmpBMP.Height * scale);

                GR.DrawImage(_tmpBMP, r2, new RectangleF(0, 0, _tmpBMP.Width, _tmpBMP.Height), GraphicsUnit.Pixel);
            }

            if (DrawBorderAndBack) {
                Skin.Draw_Border(GR, itemdesign, vState, PositionModified);
            }
        }





        private void GeneratePic() {

            if (Row == null || string.IsNullOrEmpty(_LayoutID) || !_LayoutID.StartsWith("#")) {
                _tmpBMP = (Bitmap)QuickImage.Get(enImageCode.Warnung, 128).BMP.Clone();
                return;
            }


            var _pad = new CreativePad(new ItemCollectionPad(_LayoutID, _Row));

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

            _pad.DrawCreativePadToBitmap(_tmpBMP, enStates.Standard, zoomv, (decimal)slidervalues.X, (decimal)slidervalues.Y, null);



        }

        public override Size SizeUntouchedForListBox() {
            return new Size(300, 300);

        }


        public override int HeightForListBox(enBlueListBoxAppearance style, int columnWidth) {
            return (int)(columnWidth * 0.8);
        }


        protected override string GetCompareKey() {
            return _Row.CellFirstString();
        }
    }
}
