#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2019 Christian Peter
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

namespace BlueControls.ItemCollection
{
    public class RowFormulaListItem : BasicListItem
    {
        #region  Variablen-Deklarationen 

        private RowItem _Row;
        private Bitmap _tmpBMP;
        private string _LayoutID;

        #endregion


        #region  Event-Deklarationen + Delegaten 

        #endregion


        #region  Construktor + Initialize 


        //public RowFormulaListItem()
        //{ }
        public RowFormulaListItem(RowItem cRow)
        {
            _Row = cRow;
        }


        public RowFormulaListItem(RowItem row, string userDefCompareKey)
        {
            _Row = row;
            UserDefCompareKey = userDefCompareKey;
        }


        public RowFormulaListItem(RowItem row, string layoutID, string userDefCompareKey)
        {
            _Row = row;
            _LayoutID = layoutID;
            UserDefCompareKey = userDefCompareKey;
        }



        protected override void Initialize()
        {
            base.Initialize();
            _Row = null;
            _tmpBMP = null;
        }


        #endregion


        public string LayoutID
        {
            get
            {
                return _LayoutID;
            }
            set
            {

                if (value == _LayoutID) { return; }

                _LayoutID = value;


                if (_tmpBMP != null)
                {
                    _tmpBMP.Dispose();
                    _tmpBMP = null;
                }
                OnChanged();
            }
        }


        public RowItem Row
        {
            get
            {
                return _Row;
            }
            set
            {

                if (_Row == value) { return; }

                _Row = value;

                removePic();

                OnChanged();
            }
        }


        private void removePic()
        {
            if (_tmpBMP != null)
            {
                _tmpBMP.Dispose();
                _tmpBMP = null;
            }
        }

        public override string Internal()
        {
            return _Internal;
        }


        public override void DesignOrStyleChanged()
        {
            removePic();
        }


        protected override void DrawExplicit(Graphics GR, Rectangle PositionModified, enStates vState, bool DrawBorderAndBack, bool Translate)
        {
            if (_tmpBMP == null) { GeneratePic(); }

            if (DrawBorderAndBack)
            {
                Skin.Draw_Back(GR, Parent.ItemDesign, vState, PositionModified, null, false);
            }


            if (_tmpBMP != null)
            {
                var scale = (float)Math.Min(PositionModified.Width / (double)_tmpBMP.Width, PositionModified.Height / (double)_tmpBMP.Height);
                var r2 = new RectangleF((PositionModified.Width - _tmpBMP.Width * scale) / 2 + PositionModified.Left, (PositionModified.Height - _tmpBMP.Height * scale) / 2 + PositionModified.Top, _tmpBMP.Width * scale, _tmpBMP.Height * scale);

                GR.DrawImage(_tmpBMP, r2, new RectangleF(0, 0, _tmpBMP.Width, _tmpBMP.Height), GraphicsUnit.Pixel);
            }

            if (DrawBorderAndBack)
            {
                Skin.Draw_Border(GR, Parent.ItemDesign, vState, PositionModified);
            }
        }





        private void GeneratePic()
        {

            if (Row == null || string.IsNullOrEmpty(_LayoutID) || !_LayoutID.StartsWith("#"))
            {
                _tmpBMP = (Bitmap)QuickImage.Get(enImageCode.Warnung, 128).BMP.Clone();
                return;
            }


            var _pad = new CreativePad();

            _pad.GenerateFromRow(_LayoutID, _Row, false);

            var mb = _pad.MaxBounds(null);

            if (_tmpBMP != null)
            {
                if (_tmpBMP.Width != mb.Width || _tmpBMP.Height != mb.Height)
                {
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

        public override SizeF SizeUntouchedForListBox()
        {

            if (_tmpBMP == null) { GeneratePic(); }
            if (_tmpBMP == null) { return SizeF.Empty; }
            return _tmpBMP.Size;

        }

        public override bool IsClickable()
        {
            return true;
        }

        public override void ComputePositionForListBox(enBlueListBoxAppearance IsIn, float X, float Y, float MultiX, int SliderWidth, int MaxWidth)
        {

            MaxWidth = Math.Min(MaxWidth, 800);

            if (IsIn == enBlueListBoxAppearance.Gallery || IsIn == enBlueListBoxAppearance.FileSystem)
            {
                SetCoordinates(new Rectangle((int)(X), (int)(Y), (int)(MultiX), (int)(MultiX * 0.8)));
            }
            else
            {
                SetCoordinates(new Rectangle((int)(X), (int)(Y), (int)(MultiX - SliderWidth), (int)(MaxWidth * 0.8)));
            }

        }


        public override SizeF QuickAndDirtySize(int PreferedHeigth)
        {

            if (_tmpBMP != null)
            {
                return SizeUntouchedForListBox();
            }

            return new SizeF(400, PreferedHeigth);

        }


        protected override string GetCompareKey()
        {
            return _Row.CellFirstString();
        }

        protected override BasicListItem CloneLVL2()
        {
            Develop.DebugPrint_NichtImplementiert();
            return null;
        }

        protected override bool FilterMatchLVL2(string FilterText)
        {
            Develop.DebugPrint_NichtImplementiert();
            return false;
        }
    }
}
