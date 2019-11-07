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


using BlueControls.Enums;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using BlueControls.EventArgs;
using BlueControls.Designer_Support;

namespace BlueControls.Controls
{
    [Designer(typeof(BasicDesigner))]
    public partial class ZoomPic : ZoomPad
    {

        public Bitmap BMP = null;
        public Bitmap OverlayBMP = null;






        [DefaultValue(true)]
        public bool AlwaysSmooth
        {
            get
            {
                return _AlwaysSmooth;
            }
            set
            {
                _AlwaysSmooth = value;
            }
        }


        //public event EventHandler<MouseEventArgs1_1> ImageMouseEnter;
        public event EventHandler<MouseEventArgs1_1> ImageMouseDown;
        public event EventHandler<MouseEventArgs1_1> ImageMouseMove;
        public event EventHandler<MouseEventArgs1_1> ImageMouseUp;
        public event EventHandler<AdditionalDrawing> DoAdditionalDrawing;
        public event EventHandler<PositionEventArgs> OverwriteMouseImageData;
        //public event EventHandler ImageMouseLeave;

        //private bool _IsInPic = false;

        private bool _AlwaysSmooth = false;



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

                var r =  new RectangleDF(0, 0, BMP.Width, BMP.Height).ZoomAndMoveRect(_Zoom, _MoveX, _MoveY);


                if (_Zoom < 1 || _AlwaysSmooth)
                {
                    TMPGR.SmoothingMode = SmoothingMode.AntiAlias;
                    TMPGR.InterpolationMode = InterpolationMode.HighQualityBicubic;
                }
                else
                {
                    TMPGR.SmoothingMode = SmoothingMode.HighSpeed;
                    TMPGR.InterpolationMode = InterpolationMode.NearestNeighbor;
                }

                TMPGR.PixelOffsetMode = PixelOffsetMode.Half;



                TMPGR.DrawImage(BMP, r);


                if (OverlayBMP != null)
                {
                    TMPGR.DrawImage(OverlayBMP, r);
                }


            }



            OnDoAdditionalDrawing(new AdditionalDrawing(TMPGR, _Zoom, _MoveX, _MoveY));


            Skin.Draw_Border(TMPGR, enDesign.Table_And_Pad, state, new Rectangle(1, 1, Size.Width - SliderY.Width, Size.Height - SliderX.Height));
            gr.DrawImage(_BitmapOfControl, 0, 0);
        }


        protected virtual void OnDoAdditionalDrawing(AdditionalDrawing e)
        {
            DoAdditionalDrawing?.Invoke(this, e);
        }


        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            OnImageMouseDown(GenerateNewMouseEventArgs(e));
        }


        private MouseEventArgs1_1 GenerateNewMouseEventArgs(MouseEventArgs e)
        {

            var en = new PositionEventArgs(MousePos_1_1.X, MousePos_1_1.Y);
            OnOverwriteMouseImageData(en);

            var p = PointInsidePic(en.X, en.Y);
            return new MouseEventArgs1_1(e.Button, e.Clicks, en.X, en.Y, e.Delta, p.X, p.Y, IsInBitmap(en.X, en.Y));

        }

        protected void OnOverwriteMouseImageData(PositionEventArgs e)
        {
            OverwriteMouseImageData?.Invoke(this, e);
        }

        protected virtual void OnImageMouseDown(MouseEventArgs1_1 e)
        {
            ImageMouseDown?.Invoke(this, e);
        }

        /// <summary>
        /// Zuerst ImageMouseUp, dann MouseUp
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnImageMouseUp(MouseEventArgs1_1 e)
        {
            ImageMouseUp?.Invoke(this, e);
        }


        private bool IsInBitmap(int X, int Y)
        {
            if (BMP == null) { return false; }
            if (X < 0 || Y < 0) { return false; }
            if (X > BMP.Width || Y > BMP.Height) { return false; }
            return true;
        }

        private bool IsInBitmap()
        {
            if (BMP == null) { return false; }
            if (MousePos_1_1 == null) { return false; }
            return IsInBitmap(MousePos_1_1.X, MousePos_1_1.Y);
        }

        /// <summary>
        /// Zuerst ImageMouseUp, dann MouseUp
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            OnImageMouseUp(GenerateNewMouseEventArgs(e));
            base.OnMouseUp(e);
        }


        //protected override void OnMouseLeave(System.EventArgs e)
        //{
        //    base.OnMouseLeave(e);
        //    ChangeIsInPic(false, null);
        //}

        //private void ChangeIsInPic(bool NewState, MouseEventArgs e)
        //{

        //    if (!NewState && _IsInPic)
        //    {
        //        _IsInPic = false;
        //        OnImageMouseLeave();
        //    }

        //    if (NewState && !_IsInPic)
        //    {
        //        _IsInPic = true;
        //        OnImageMouseEnter(e);
        //    }

        //}

        //protected void OnImageMouseEnter(MouseEventArgs e)
        //{
        //    ImageMouseEnter?.Invoke(this, e);
        //}

        //protected void OnImageMouseLeave()
        //{
        //    ImageMouseLeave?.Invoke(this, System.EventArgs.Empty);
        //}

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);


            //if (!IsInBitmap())
            //{
            //    ChangeIsInPic(false, null);
            //    return;
            //}

            //var mc = new MouseEventArgs1_1((int)MousePos_1_1.X, (int)MousePos_1_1.Y, IsInBitmap);


            OnImageMouseMove(GenerateNewMouseEventArgs(e));


        }

        protected virtual void OnImageMouseMove(MouseEventArgs1_1 e)
        {
            ImageMouseMove?.Invoke(this, e);
        }



        ///// <summary>
        ///// Berechnet Maus Koordinaten auf dem Großen Bild
        ///// in in Koordinaten um, als ob auf dem Bild direkt gewählt werden würde.
        ///// Falls die Maus-Koordinaten ausserhalb des tatsächlichen Bildes sind
        ///// </summary>
        ///// <remarks>
        ///// </remarks>
        //protected MouseEventArgs MouseToPixelKoordsOnImage(System.Windows.Forms.MouseEventArgs e)
        //{

        //    if (_Zoom < 0.01m ) { return null; }

        //    //var r = MaxBounds().ZoomAndMoveRect(_Zoom, (decimal)SliderX.Value, (decimal)SliderY.Value);


        //    //var x1 = Convert.ToInt32(Math.Floor((e.X - r.Left) / _Zoom));
        //    //var y1 = Convert.ToInt32(Math.Floor((e.Y - r.Top) / _Zoom));

        //    //if (TrimIntoPic)
        //    //{
        //    //    x1 = Math.Max(0, x1);
        //    //    y1 = Math.Max(0, y1);

        //    //    x1 = Math.Min(BMP.Width, x1);
        //    //    y1 = Math.Min(BMP.Height, y1);

        //    //}


        //    return new MouseEventArgs(e.Button, e.Clicks, x1, y1, e.Delta);

        //    //            return new Point(x1, y1);

        //}

        public Point PointInsidePic(int x, int y)
        {

            if (BMP == null) { return Point.Empty; }



            x = Math.Max(0, x);
            y = Math.Max(0, y);

            x = Math.Min(BMP.Width - 1, x);
            y = Math.Min(BMP.Height - 1, y);

            return new Point(x, y);

        }



    }
}
