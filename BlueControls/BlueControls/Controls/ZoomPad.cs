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
using BlueControls.Interfaces;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace BlueControls.Controls
{
    public partial class ZoomPad : GenericControl, IBackgroundBitmap
    {
        public ZoomPad()
        {
            InitializeComponent();
        }




        public static readonly Pen PenGray = new Pen(Color.FromArgb(40, 0, 0, 0));
        public static readonly Pen PenGrayLarge = new Pen(Color.FromArgb(40, 0, 0, 0), 5);


        protected Bitmap _BitmapOfControl;
        protected bool _GeneratingBitmapOfControl;

        protected decimal _Zoom = 1;
        protected decimal _ZoomFit = 1;

        protected bool _Fitting = true;

        protected decimal _MoveX = -1;
        protected decimal _MoveY = -1;



        /// <summary>
        /// Die Koordinaten, an der Stelle der Mausknopf gedrückt wurde. Zoom und Slider wurden eingerechnet, dass die Koordinaten Massstabsunabhängis sind.
        /// </summary>
        public Point MouseDownPos_1_1;

        /// <summary>
        /// Die Koordinaten, an der die der Mauspfeil zuletzt war. Zoom und Slider wurden eingerechnet, dass die Koordinaten Massstabsunabhängis sind.
        /// </summary>
        public Point MousePos_1_1;


        //  protected RectangleDF MaxBounds { get; set; }


        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseDown(e);
            MousePos_1_1 = KoordinatesUnscaled(e);
            MouseDownPos_1_1 = KoordinatesUnscaled(e);
        }


        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseUp(e);
            MousePos_1_1 = KoordinatesUnscaled(e);
            MouseDownPos_1_1 = Point.Empty;
        }


        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            MousePos_1_1 = KoordinatesUnscaled(e);
        }

        protected override void OnMouseLeave(System.EventArgs e)
        {
            base.OnMouseLeave(e);
            MousePos_1_1 = Point.Empty;
        }


        public void ZoomIn(System.Windows.Forms.MouseEventArgs e)
        {
            var x = new System.Windows.Forms.MouseEventArgs(e.Button, e.Clicks, e.X, e.Y, 1);
            OnMouseWheel(x);
        }

        public void ZoomOut(System.Windows.Forms.MouseEventArgs e)
        {
            var x = new System.Windows.Forms.MouseEventArgs(e.Button, e.Clicks, e.X, e.Y, -1);
            OnMouseWheel(x);
        }

        public void ZoomFit()
        {
            _Fitting = true;

            var mb = MaxBounds();

            _ZoomFit = ZoomFitValue(mb, true, Size);

            if (_Fitting && !MousePressing()) { _Zoom = _ZoomFit; }
            _Zoom = _ZoomFit; // Math.Max(_Zoom, _ZoomFit / 1.2m);
            ComputeSliders(mb);
            Invalidate();
        }




        public decimal ZoomFitValue(RectangleDF MaxBounds, bool sliderShowing, Size sizeOfPaintArea)
        {
            if (MaxBounds.Width < 0.01m || MaxBounds.Height < 0.01m) { return 1m; }

            if (sliderShowing)
            {
                return Math.Min((sizeOfPaintArea.Width - SliderY.Width - 32) / MaxBounds.Width, (sizeOfPaintArea.Height - SliderX.Height - 32) / MaxBounds.Height);
            }
            else
            {
                return Math.Min(sizeOfPaintArea.Width / MaxBounds.Width, sizeOfPaintArea.Height / MaxBounds.Height);
            }
        }


        /// <summary>
        /// Berechnet Maus Koordinaten des Steuerelements in in Koordinaten um, als ob auf dem unscalierten Inhalt direkt gewählt werden würde.
        /// Falls die Maus-Koordinaten ausserhalb der grenzen sind, wird nichts getrimmt.
        /// </summary>
        /// <remarks>
        /// </remarks>
        protected Point KoordinatesUnscaled(System.Windows.Forms.MouseEventArgs e)
        {
            return new Point((int)((e.X + _MoveX) / _Zoom), (int)((e.Y + _MoveY) / _Zoom));
        }


        protected virtual RectangleDF MaxBounds()
        {
            Develop.DebugPrint_RoutineMussUeberschriebenWerden();
            return null;
        }



        public Bitmap BitmapOfControl()
        {
            if (_GeneratingBitmapOfControl) { return null; }
            _GeneratingBitmapOfControl = true;
            if (_BitmapOfControl == null) { Refresh(); }
            _GeneratingBitmapOfControl = false;
            return _BitmapOfControl;
        }

        protected override void OnMouseWheel(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            _Fitting = false;

            var m = KoordinatesUnscaled(e);

            if (e.Delta > 0)
            {
                _Zoom = _Zoom * 1.5m;
            }
            else
            {
                _Zoom = _Zoom * (1m / 1.5m);
            }




            _Zoom = Math.Max(_ZoomFit / 1.2m, _Zoom);
            _Zoom = Math.Min(20, _Zoom);

            var mb = MaxBounds();

            ComputeSliders(mb);


            // M Beeinhaltet den Punkt, wo die Maus hinzeigt Maßstabunabhängig.

            // Der Slider ist abhängig vom Maßsstab - sowie die echten Mauskoordinaten ebenfalls.

            // Deswegen die M mit dem neuen Zoom-Faktor berechnen umrechen, um auch Masstababhängig zu sein
            // Die Verschiebung der echten Mauskoordinaten berechnen und den Slider auf den Wert setzen.



            SliderX.Value = (double)(m.X * _Zoom - e.X);
            SliderY.Value = (double)(m.Y * _Zoom - e.Y);

            // Alte Berechnung für Mittig Setzen
            //SliderX.Value = (m.X * _Zoom) - (Width / 2) - SliderY.Width
            //SliderY.Value = (m.Y * _Zoom) - (Height / 2) - SliderX.Height



        }


        protected override void OnSizeChanged(System.EventArgs e)
        {
            if (_BitmapOfControl != null)
            {
                if (_BitmapOfControl.Width < Width || _BitmapOfControl.Height < Height)
                {
                    _BitmapOfControl.Dispose();
                    _BitmapOfControl = null;
                }
            }

            ZoomFit();
            base.OnSizeChanged(e);
        }





        private void SliderX_ValueChanged(object sender, System.EventArgs e)
        {
            _MoveX = (decimal)SliderX.Value;
            Invalidate();
        }

        private void SliderY_ValueChanged(object sender, System.EventArgs e)
        {
            _MoveY = (decimal)SliderY.Value;
            Invalidate();
        }

        internal PointF SliderValues(RectangleDF bounds, decimal ZoomToUse, Point TopLeftPos)
        {
            return new PointF((float)(bounds.Left * ZoomToUse - TopLeftPos.X / 2m), (float)(bounds.Top * ZoomToUse - TopLeftPos.Y / 2m));
        }

        private void ComputeSliders(RectangleDF maxBounds)
        {


            if (maxBounds.Width == 0) { return; }

            var p = CenterPos(maxBounds, true, Size, _Zoom);
            var sliderv = SliderValues(maxBounds, _Zoom, p);



            if (p.X < 0)
            {
                SliderX.Enabled = true;
                SliderX.Minimum = (double)(maxBounds.Left * _Zoom - Width * 0.6m);
                SliderX.Maximum = (double)(maxBounds.Right * _Zoom - Width + Width * 0.6m);

            }
            else
            {
                SliderX.Enabled = false;

                if (MousePressing() == false)
                {
                    SliderX.Minimum = sliderv.X;
                    SliderX.Maximum = sliderv.X;
                    SliderX.Value = sliderv.X;
                }
            }

            if (p.Y < 0)
            {
                SliderY.Enabled = true;
                SliderY.Minimum = (double)(maxBounds.Top * _Zoom - Height * 0.6m);
                SliderY.Maximum = (double)(maxBounds.Bottom * _Zoom - Height + Height * 0.6m);

            }
            else
            {
                SliderY.Enabled = false;
                if (MousePressing() == false)
                {
                    SliderY.Minimum = sliderv.Y;
                    SliderY.Maximum = sliderv.Y;
                    SliderY.Value = sliderv.Y;
                }
            }

            Invalidate();
        }



        public void SetZoom(decimal Zoom)
        {
            _Zoom = Zoom;

            SliderX.Minimum = 0;
            SliderX.Maximum = 0;
            SliderX.Value = 0;

            SliderY.Minimum = 0;
            SliderY.Maximum = 0;
            SliderY.Value = 0;

        }

        /// <summary>
        /// Gibt den Versatz der Linken oben Ecke aller Objekte zurück, um mittig zu sein.
        /// </summary>
        /// <param name="SliderShowing"></param>
        /// <param name="sizeOfPaintArea"></param>
        /// <param name="ZoomToUse"></param>
        /// <returns></returns>
        public Point CenterPos(RectangleDF MaxBounds, bool SliderShowing, Size sizeOfPaintArea, decimal ZoomToUse)
        {
            var w = 0;
            var h = 0;


            if (SliderShowing)
            {
                w = (int)(sizeOfPaintArea.Width - SliderY.Width - MaxBounds.Width * ZoomToUse);
                h = (int)(sizeOfPaintArea.Height - SliderX.Height - MaxBounds.Height * ZoomToUse);
            }
            else
            {
                w = (int)(sizeOfPaintArea.Width - MaxBounds.Width * ZoomToUse);
                h = (int)(sizeOfPaintArea.Height - MaxBounds.Height * ZoomToUse);
            }

            return new Point(w, h);
        }



        public Rectangle AviablePaintArea()
        {

            var wi = Size.Width;
            if (SliderY.Visible) { wi -= SliderY.Width; }

            var he = Size.Width;
            if (SliderX.Visible) { he -= SliderX.Height; }

            return new Rectangle(0, 0, wi, he);

        }



    }
}

