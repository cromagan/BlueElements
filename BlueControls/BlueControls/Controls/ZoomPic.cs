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


using BlueBasics.Enums;
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

        static Pen Pen_RotTransp = new Pen(Color.FromArgb(50, 255, 0, 0));
        static Brush Brush_RotTransp = new SolidBrush(Color.FromArgb(128, 255, 0, 0));

        private enOrientation _MittelLinie = enOrientation.Ohne;

        private enHelpers _Helper = enHelpers.Ohne;


        [DefaultValue((enOrientation)(-1))]
        public enOrientation Mittellinie
        {
            get
            {
                return _MittelLinie;
            }
            set
            {


                if (_MittelLinie == value) { return; }
                _MittelLinie = value;
                Invalidate();
            }

        }


        [DefaultValue(enHelpers.Ohne)]
        public enHelpers Helper
        {
            get
            {
                return _Helper;
            }
            set
            {


                if (_Helper == value) { return; }
                _Helper = value;
                Invalidate();
            }

        }




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

                var r = MaxBounds().ZoomAndMoveRect(_Zoom, _MoveX, _MoveY);


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
                    PrepareOverlay();
                    TMPGR.DrawImage(OverlayBMP, r);
                }


            }



            Skin.Draw_Border(TMPGR, enDesign.Table_And_Pad, state, new Rectangle(0, 0, Size.Width - SliderY.Width, Size.Height - SliderX.Height));
            gr.DrawImage(_BitmapOfControl, 0, 0);

        }

        private void PrepareOverlay()
        {
            //OverlayBMP = (BMP.Clone();


            var TMPGR = Graphics.FromImage(OverlayBMP);

            // Mittellinie
            var PicturePos = MaxBounds();

            if (_MittelLinie.HasFlag(enOrientation.Waagerecht))
            {
                var p1 = PicturePos.PointOf(enAlignment.VerticalCenter_Left).ToPointF();
                var p2 = PicturePos.PointOf(enAlignment.VerticalCenter_Right).ToPointF();

                //var p1 = new Point(0, (int)(OverlayBMP.Height / 2));
                //var p2 = new Point(OverlayBMP.Width, (int)(OverlayBMP.Height / 2));

                TMPGR.DrawLine(new Pen(Color.FromArgb(10, 0, 0, 0), 3), p1, p2);
                TMPGR.DrawLine(new Pen(Color.FromArgb(220, 100, 255, 100)), p1, p2);
            }

            if (_MittelLinie.HasFlag(enOrientation.Senkrecht))
            {
                var p1 = PicturePos.PointOf(enAlignment.Top_HorizontalCenter).ToPointF();
                var p2 = PicturePos.PointOf(enAlignment.Bottom_HorizontalCenter).ToPointF();
                //var p1 = new Point((int)(OverlayBMP.Width / 2),0);
                //var p2 = new Point((int)(OverlayBMP.Width / 2), OverlayBMP.Height);
                TMPGR.DrawLine(new Pen(Color.FromArgb(10, 0, 0, 0), 3), p1, p2);
                TMPGR.DrawLine(new Pen(Color.FromArgb(220, 100, 255, 100)), p1, p2);
            }


            if (MousePos_1_1.IsEmpty) { return; }


            if (_Helper.HasFlag(enHelpers.HorizontalLine))
            {
                TMPGR.DrawLine(Pen_RotTransp, (int)MousePos_1_1.X, 0, (int)MousePos_1_1.X, OverlayBMP.Height);

            }
            if (_Helper.HasFlag(enHelpers.VerticalLine))
            {
                TMPGR.DrawLine(Pen_RotTransp, 0, (int)MousePos_1_1.Y, OverlayBMP.Width, (int)MousePos_1_1.Y);

            }



            if (_Helper.HasFlag(enHelpers.FilledRectancle))
            {
                if (!MouseDownPos_1_1.IsEmpty)
                {

                    var r = new Rectangle(Math.Min(MouseDownPos_1_1.X, MousePos_1_1.X), Math.Min(MouseDownPos_1_1.Y, MousePos_1_1.Y), Math.Abs(MouseDownPos_1_1.X - MousePos_1_1.X) + 1, Math.Abs(MouseDownPos_1_1.Y - MousePos_1_1.Y) + 1);
                    //var r = new Rectangle((int)Math.Min(MouseDownPos_1_1.X, MousePos_1_1.X), (int)Math.Min(MouseDownPos_1_1.Y, MousePos_1_1.Y), (int)Math.Abs(MouseDownPos_1_1.X - MousePos_1_1.X) + 1, (int)Math.Abs(MouseDownPos_1_1.Y - MousePos_1_1.Y) + 1);
                    TMPGR.FillRectangle(Brush_RotTransp, r);
                }
            }


        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (IsInBitmap())
            {
                var mc = new MouseEventArgs(e.Button, e.Clicks, (int)MousePos_1_1.X, (int)MousePos_1_1.Y, e.Delta);
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

        private bool IsInBitmap()
        {
            if (BMP == null) { return false; }
            if (MousePos_1_1 == null) { return false; }

            if (MousePos_1_1.X < 0 || MousePos_1_1.Y < 0) { return false; }

            if (MousePos_1_1.X > BMP.Width || MousePos_1_1.Y > BMP.Height) { return false; }
            return true;

        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);




            if (IsInBitmap())
            {
                var mc = new MouseEventArgs(e.Button, e.Clicks, (int)MousePos_1_1.X, (int)MousePos_1_1.Y, e.Delta);
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


            if (!IsInBitmap())
            {
                ChangeIsInPic(false, null);
                return;
            }

            var mc = new MouseEventArgs(e.Button, e.Clicks, (int)MousePos_1_1.X, (int)MousePos_1_1.Y, e.Delta);
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
