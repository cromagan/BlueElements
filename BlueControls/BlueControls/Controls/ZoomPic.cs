#region BlueElements - a collection of useful tools, database and controls
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
#endregion


using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BlueControls.Controls {
    [Designer(typeof(BasicDesigner))]
    public partial class ZoomPic : ZoomPad {
        private MouseEventArgs1_1 _MouseDown = null;
        private MouseEventArgs1_1 _MouseCurrent = null;

        private Bitmap _bmp = null;
        //public Bitmap OverlayBMP = null;

        [DefaultValue(false)]
        public bool AlwaysSmooth { get; set; } = false;


        public Bitmap BMP {
            get => _bmp;


            set {

                if (value == _bmp) { return; }

                var dozoomfit = true;

                if (_bmp != null && value != null) {
                    dozoomfit = value.Width != _bmp.Width || value.Height != _bmp.Height;
                }


                _bmp = value;

                if (dozoomfit) {
                    ZoomFit();
                } else {
                    Invalidate();
                }

            }
        }

        #region Constructor
        public ZoomPic() : base() {
            InitializeComponent();
            _MouseHighlight = false;
        }

        #endregion
        public event EventHandler<MouseEventArgs1_1> ImageMouseDown;
        public event EventHandler<MouseEventArgs1_1DownAndCurrent> ImageMouseMove;
        public event EventHandler<MouseEventArgs1_1DownAndCurrent> ImageMouseUp;
        public event EventHandler<AdditionalDrawing> DoAdditionalDrawing;
        public event EventHandler<PositionEventArgs> OverwriteMouseImageData;

        protected override RectangleM MaxBounds() {
            if (_bmp != null) { return new RectangleM(0, 0, _bmp.Width, _bmp.Height); }
            return new RectangleM(0, 0, 0, 0);
        }


        protected override void DrawControl(Graphics gr, enStates state) {
            //if (_BitmapOfControl == null)
            //{
            //    _BitmapOfControl = new Bitmap(ClientSize.Width, ClientSize.Height, PixelFormat.Format32bppPArgb);
            //}

            //var TMPGR = Graphics.FromImage(_BitmapOfControl);

            var lgb = new LinearGradientBrush(ClientRectangle, Color.White, Color.LightGray, LinearGradientMode.Vertical);

            gr.FillRectangle(lgb, ClientRectangle);

            if (_bmp != null) {

                var r = new RectangleM(0, 0, _bmp.Width, _bmp.Height).ZoomAndMoveRect(_Zoom, _shiftX, _shiftY);


                if (_Zoom < 1 || AlwaysSmooth) {
                    gr.SmoothingMode = SmoothingMode.AntiAlias;
                    gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                } else {
                    gr.SmoothingMode = SmoothingMode.HighSpeed;
                    gr.InterpolationMode = InterpolationMode.NearestNeighbor;
                }

                gr.PixelOffsetMode = PixelOffsetMode.Half;



                gr.DrawImage(_bmp, r);


                //if (OverlayBMP != null)
                //{
                //    TMPGR.DrawImage(OverlayBMP, r);
                //}


            }



            OnDoAdditionalDrawing(new AdditionalDrawing(gr, _Zoom, _shiftX, _shiftY, _MouseDown, _MouseCurrent));


            Skin.Draw_Border(gr, enDesign.Table_And_Pad, state, new Rectangle(1, 1, Size.Width - SliderY.Width, Size.Height - SliderX.Height));
            //gr.DrawImage(_BitmapOfControl, 0, 0);
        }


        protected virtual void OnDoAdditionalDrawing(AdditionalDrawing e) {
            DoAdditionalDrawing?.Invoke(this, e);
        }


        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);
            _MouseCurrent = GenerateNewMouseEventArgs(e);
            _MouseDown = _MouseCurrent;
            OnImageMouseDown(_MouseDown);
        }


        private MouseEventArgs1_1 GenerateNewMouseEventArgs(MouseEventArgs e) {

            var en = new PositionEventArgs(MousePos_1_1.X, MousePos_1_1.Y);
            OnOverwriteMouseImageData(en);

            var p = PointInsidePic(en.X, en.Y);
            return new MouseEventArgs1_1(e.Button, e.Clicks, en.X, en.Y, e.Delta, p.X, p.Y, IsInBitmap(en.X, en.Y));

        }

        protected void OnOverwriteMouseImageData(PositionEventArgs e) {
            OverwriteMouseImageData?.Invoke(this, e);
        }

        private void OnImageMouseDown(MouseEventArgs1_1 e) {
            ImageMouseDown?.Invoke(this, e);
        }

        /// <summary>
        /// Zuerst ImageMouseUp, dann MouseUp
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnImageMouseUp(MouseEventArgs1_1 e) {
            ImageMouseUp?.Invoke(this, new MouseEventArgs1_1DownAndCurrent(_MouseDown, e));
        }


        private bool IsInBitmap(int X, int Y) {
            if (_bmp == null) { return false; }
            if (X < 0 || Y < 0) { return false; }
            if (X > _bmp.Width || Y > _bmp.Height) { return false; }
            return true;
        }

        //private bool IsInBitmap() {
        //    if (_bmp == null) { return false; }
        //    if (MousePos_1_1 == null) { return false; }
        //    return IsInBitmap(MousePos_1_1.X, MousePos_1_1.Y);
        //}

        /// <summary>
        /// Zuerst ImageMouseUp, dann MouseUp
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseUp(MouseEventArgs e) {
            _MouseCurrent = GenerateNewMouseEventArgs(e);
            OnImageMouseUp(_MouseCurrent);
            base.OnMouseUp(e);
            _MouseDown = null;
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);

            _MouseCurrent = GenerateNewMouseEventArgs(e);

            OnImageMouseMove(_MouseCurrent);
        }

        private void OnImageMouseMove(MouseEventArgs1_1 e) {
            ImageMouseMove?.Invoke(this, new MouseEventArgs1_1DownAndCurrent(_MouseDown, e));
        }



        public Point PointInsidePic(int x, int y) {

            if (_bmp == null) { return Point.Empty; }

            x = Math.Max(0, x);
            y = Math.Max(0, y);

            x = Math.Min(_bmp.Width - 1, x);
            y = Math.Min(_bmp.Height - 1, y);

            return new Point(x, y);
        }
    }
}
