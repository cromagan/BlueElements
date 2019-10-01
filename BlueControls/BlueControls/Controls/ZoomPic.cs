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
using BlueControls.Forms;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using BlueDatabase.EventArgs;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using static BlueBasics.FileOperations;
using static BlueBasics.Extensions;
using System.Windows.Forms;

namespace BlueControls.Controls
{

    public partial class ZoomPic : ZoomPad
    {

        public Bitmap BMP = null;

        public ZoomPic()
        {
            InitializeComponent();
            _MouseHighlight = false;
        }


        protected override void InitializeSkin()
        {

        }

        protected override RectangleDF MaxBounds()
        {
            if (BMP != null) { return new RectangleDF(0, 0, BMP.Width, BMP.Height); }

            return new RectangleDF(0, 0, 0, 0);
        }


        protected override void DrawControl(Graphics gr, enStates state)
        {
            if (_BitmapOfControl == null)
            {
                _BitmapOfControl = new Bitmap(ClientSize.Width, ClientSize.Height, PixelFormat.Format32bppPArgb);
            }

            var TMPGR = Graphics.FromImage(_BitmapOfControl);

            var lgb = new LinearGradientBrush(ClientRectangle, Color.White, Color.LightGray, LinearGradientMode.Vertical);

            TMPGR.FillRectangle(lgb, ClientRectangle);

            if (BMP != null)
            {

                var r = MaxBounds().ZoomAndMoveRect(_Zoom, (decimal)SliderX.Value, (decimal)SliderY.Value);
                TMPGR.PixelOffsetMode = PixelOffsetMode.Half;

                if (_Zoom <1)
                {
                    TMPGR.SmoothingMode = SmoothingMode.AntiAlias;
                    TMPGR.InterpolationMode = InterpolationMode.HighQualityBicubic;
                }
                else
                {
                    TMPGR.SmoothingMode = SmoothingMode.HighSpeed;
                    TMPGR.InterpolationMode = InterpolationMode.NearestNeighbor;
                }



                TMPGR.DrawImage(BMP, r);

            }


            Skin.Draw_Border(TMPGR, enDesign.Table_And_Pad, state, new Rectangle(0,0,Size.Width- SliderY.Width, Size.Height- SliderX.Height));
            gr.DrawImage(_BitmapOfControl, 0, 0);

        }

    }
}
