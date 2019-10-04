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

namespace BlueControls.Controls
{

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


        public event EventHandler<System.Windows.Forms.MouseEventArgs> ImageMouseEnter;
        public event EventHandler<System.Windows.Forms.MouseEventArgs> ImageMouseDown;
        public event EventHandler<System.Windows.Forms.MouseEventArgs> ImageMouseMove;
        public event EventHandler<System.Windows.Forms.MouseEventArgs> ImageMouseUp;
        public event EventHandler ImageMouseLeave;

        private bool _IsInPic = false;

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

                var r = MaxBounds().ZoomAndMoveRect(_Zoom, (decimal)SliderX.Value, (decimal)SliderY.Value);


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


            Skin.Draw_Border(TMPGR, enDesign.Table_And_Pad, state, new Rectangle(0, 0, Size.Width - SliderY.Width, Size.Height - SliderX.Height));
            gr.DrawImage(_BitmapOfControl, 0, 0);

        }



        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            var P = KoordinatesUnscaled(e);
            var mc = new MouseEventArgs(e.Button,e.Clicks, (int)P.X, (int)P.Y, e.Delta);

            if (IsInBitmap(mc))
            {
                OnImageMouseDown(mc);
            }

        }

        private void OnImageMouseDown(MouseEventArgs e)
        {
            ImageMouseDown?.Invoke(this, e);
        }

        private void OnImageMouseUp(MouseEventArgs e)
        {
            ImageMouseUp?.Invoke(this, e);
        }

        private bool IsInBitmap(MouseEventArgs e)
        {
            if (BMP == null) { return false; }

            if (e.X < 0 || e.Y < 0) { return false; }

            if (e.X > BMP.Width || e.Y > BMP.Height) { return false; }
            return true;

        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            var P = KoordinatesUnscaled(e);
            var mc = new MouseEventArgs(e.Button, e.Clicks, (int)P.X, (int)P.Y, e.Delta);

            if (IsInBitmap(mc))
            {
                OnImageMouseUp(mc);
            }

        }


        protected override void OnMouseLeave(System.EventArgs e)
        {
            base.OnMouseLeave(e);
            ChangeIsInPic(false, null);
        }

        private void ChangeIsInPic(bool NewState, MouseEventArgs e)
        {

            if (!NewState && _IsInPic)
            {
                _IsInPic = false;
                OnImageMouseLeave();
            }

            if (NewState && !_IsInPic)
            {
                _IsInPic = true;
                OnImageMouseEnter(e);
            }

        }

        protected void OnImageMouseEnter(MouseEventArgs e)
        {
            ImageMouseEnter?.Invoke(this, e);
        }

        protected void OnImageMouseLeave()
        {
            ImageMouseLeave?.Invoke(this, System.EventArgs.Empty);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var P = KoordinatesUnscaled(e);
            var mc = new MouseEventArgs(e.Button, e.Clicks, (int)P.X, (int)P.Y, e.Delta);

            if (!IsInBitmap(mc))
            {
                ChangeIsInPic(false, null);
                return;
            }

            ChangeIsInPic(true, mc);

            OnImageMouseMove(mc);


        }

        private void OnImageMouseMove(MouseEventArgs e)
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



    }
}
