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
using BlueControls.Designer_Support;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace BlueControls.Controls {

    [Designer(typeof(BasicDesigner))]
    public partial class ZoomPad : GenericControl {

        #region Fields

        public static readonly Pen PenGray = new(Color.FromArgb(40, 0, 0, 0));

        public static readonly Pen PenGrayLarge = new(Color.FromArgb(40, 0, 0, 0), 5);

        /// <summary>
        /// Die Koordinaten, an der Stelle der Mausknopf gedrückt wurde. Zoom und Slider wurden eingerechnet, dass die Koordinaten Massstabsunabhängis sind.
        /// </summary>
        public Point MouseDownPos_1_1;

        /// <summary>
        /// Die Koordinaten, an der die der Mauspfeil zuletzt war. Zoom und Slider wurden eingerechnet, dass die Koordinaten Massstabsunabhängis sind.
        /// </summary>
        public Point MousePos_1_1;

        protected bool _Fitting = true;

        protected double _shiftX = -1;

        protected double _shiftY = -1;

        protected double _Zoom = 1;

        protected double _ZoomFit = 1;

        #endregion

        #region Constructors

        public ZoomPad() : base(true, true) => InitializeComponent();

        #endregion

        #region Methods

        public Rectangle AvailablePaintArea() {
            var wi = Size.Width;
            if (SliderY.Visible) { wi -= SliderY.Width; }
            var he = Size.Width;
            if (SliderX.Visible) { he -= SliderX.Height; }
            return new Rectangle(0, 0, wi, he);
        }

        /// <summary>
        /// Gibt den Versatz der Linken oben Ecke aller Objekte zurück, um mittig zu sein.
        /// </summary>
        /// <param name="SliderShowing"></param>
        /// <param name="sizeOfPaintArea"></param>
        /// <param name="ZoomToUse"></param>
        /// <returns></returns>
        public Point CenterPos(RectangleM MaxBounds, bool SliderShowing, Size sizeOfPaintArea, double ZoomToUse) {
            double w;
            double h;
            if (SliderShowing) {
                w = sizeOfPaintArea.Width - SliderY.Width - (MaxBounds.Width * ZoomToUse);
                h = sizeOfPaintArea.Height - SliderX.Height - (MaxBounds.Height * ZoomToUse);
            } else {
                w = sizeOfPaintArea.Width - (MaxBounds.Width * ZoomToUse);
                h = sizeOfPaintArea.Height - (MaxBounds.Height * ZoomToUse);
            }
            return new Point((int)w, (int)h);
        }

        public void SetZoom(double Zoom) {
            _Zoom = Zoom;
            SliderX.Minimum = 0;
            SliderX.Maximum = 0;
            SliderX.Value = 0;
            SliderY.Minimum = 0;
            SliderY.Maximum = 0;
            SliderY.Value = 0;
            ZoomOrShiftChanged();
        }

        public void Zoom100() => CalculateZoomFitAndSliders(1D);

        public double ZoomCurrent() => _Zoom;

        public void ZoomFit() => CalculateZoomFitAndSliders(_ZoomFit);

        public double ZoomFitValue(RectangleM MaxBounds, bool sliderShowing, Size sizeOfPaintArea) => MaxBounds == null || MaxBounds.Width < 0.01d || MaxBounds.Height < 0.01d
? 1d
: sliderShowing
? Math.Min((sizeOfPaintArea.Width - SliderY.Width - 32) / MaxBounds.Width, (sizeOfPaintArea.Height - SliderX.Height - 32) / MaxBounds.Height)
: Math.Min(sizeOfPaintArea.Width / MaxBounds.Width, sizeOfPaintArea.Height / MaxBounds.Height);

        public void ZoomIn(MouseEventArgs e) {
            MouseEventArgs x = new(e.Button, e.Clicks, e.X, e.Y, 1);
            OnMouseWheel(x);
        }

        public void ZoomOut(MouseEventArgs e) {
            MouseEventArgs x = new(e.Button, e.Clicks, e.X, e.Y, -1);
            OnMouseWheel(x);
        }

        internal PointF SliderValues(RectangleM bounds, double ZoomToUse, Point TopLeftPos) => new((float)((bounds.Left * ZoomToUse) - (TopLeftPos.X / 2d)), (float)((bounds.Top * ZoomToUse) - (TopLeftPos.Y / 2d)));

        /// <summary>
        /// Kümmert sich um Slider und Maximal-Setting.
        /// Bei einem negativen Wert wird der neue Zoom nicht gesetzt.
        /// </summary>
        /// <param name="newzoom"></param>
        protected void CalculateZoomFitAndSliders(double newzoom) {
            var mb = MaxBounds();
            _ZoomFit = ZoomFitValue(mb, true, Size);
            if (newzoom >= 0) {
                _Zoom = newzoom;

                _Fitting = Math.Abs(_Zoom - newzoom) < 0.01;
            }
            ComputeSliders(mb);
            if (newzoom >= 0) {
                ZoomOrShiftChanged();
                Invalidate();
            }
        }

        /// <summary>
        /// Berechnet Maus Koordinaten des Steuerelements in in Koordinaten um, als ob auf dem unscalierten Inhalt direkt gewählt werden würde.
        /// Falls die Maus-Koordinaten ausserhalb der grenzen sind, wird nichts getrimmt.
        /// </summary>
        /// <remarks>
        /// </remarks>
        protected Point KoordinatesUnscaled(MouseEventArgs e) => new((int)Math.Round(((e.X + _shiftX) / _Zoom) - 0.5d, 0), (int)Math.Round(((e.Y + _shiftY) / _Zoom) - 0.5d, 0));

        protected virtual RectangleM MaxBounds() {
            Develop.DebugPrint_RoutineMussUeberschriebenWerden();
            return null;
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            MousePos_1_1 = KoordinatesUnscaled(e);
            MouseDownPos_1_1 = KoordinatesUnscaled(e);
            base.OnMouseDown(e);
        }

        protected override void OnMouseLeave(System.EventArgs e) {
            MousePos_1_1 = Point.Empty;
            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            MousePos_1_1 = KoordinatesUnscaled(e);
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            MousePos_1_1 = KoordinatesUnscaled(e);
            base.OnMouseUp(e);
            MouseDownPos_1_1 = Point.Empty;
        }

        protected override void OnMouseWheel(MouseEventArgs e) {
            base.OnMouseWheel(e);
            _Fitting = false;
            var m = KoordinatesUnscaled(e);
            if (e.Delta > 0) {
                _Zoom *= 1.5d;
            } else {
                _Zoom *= 1d / 1.5d;
            }
            _Zoom = Math.Max(_ZoomFit / 1.2d, _Zoom);
            _Zoom = Math.Min(20, _Zoom);
            var mb = MaxBounds();
            ComputeSliders(mb);
            // M Beeinhaltet den Punkt, wo die Maus hinzeigt Maßstabunabhängig.
            // Der Slider ist abhängig vom Maßsstab - sowie die echten Mauskoordinaten ebenfalls.
            // Deswegen die M mit dem neuen Zoom-Faktor berechnen umrechen, um auch Masstababhängig zu sein
            // Die Verschiebung der echten Mauskoordinaten berechnen und den Slider auf den Wert setzen.
            SliderX.Value = (double)((m.X * _Zoom) - e.X);
            SliderY.Value = (double)((m.Y * _Zoom) - e.Y);
            ZoomOrShiftChanged();
            // Alte Berechnung für Mittig Setzen
            //SliderX.Value = (m.X * _Zoom) - (Width / 2) - SliderY.Width
            //SliderY.Value = (m.Y * _Zoom) - (Height / 2) - SliderX.Height
        }

        protected override void OnSizeChanged(System.EventArgs e) {
            ZoomFit();
            base.OnSizeChanged(e);
        }

        protected virtual void ZoomOrShiftChanged() {
        }

        private void ComputeSliders(RectangleM maxBounds) {
            if (maxBounds == null || maxBounds.Width == 0) { return; }
            var p = CenterPos(maxBounds, true, Size, _Zoom);
            var sliderv = SliderValues(maxBounds, _Zoom, p);
            if (p.X < 0) {
                SliderX.Enabled = true;
                SliderX.Minimum = (double)((maxBounds.Left * _Zoom) - (Width * 0.6d));
                SliderX.Maximum = (double)((maxBounds.Right * _Zoom) - Width + (Width * 0.6d));
            } else {
                SliderX.Enabled = false;
                if (MousePressing() == false) {
                    SliderX.Minimum = sliderv.X;
                    SliderX.Maximum = sliderv.X;
                    SliderX.Value = sliderv.X;
                }
            }
            if (p.Y < 0) {
                SliderY.Enabled = true;
                SliderY.Minimum = (double)((maxBounds.Top * _Zoom) - (Height * 0.6d));
                SliderY.Maximum = (double)((maxBounds.Bottom * _Zoom) - Height + (Height * 0.6d));
            } else {
                SliderY.Enabled = false;
                if (MousePressing() == false) {
                    SliderY.Minimum = sliderv.Y;
                    SliderY.Maximum = sliderv.Y;
                    SliderY.Value = sliderv.Y;
                }
            }
            Invalidate();
        }

        private void SliderX_ValueChanged(object sender, System.EventArgs e) {
            _shiftX = SliderX.Value;
            ZoomOrShiftChanged();
            Invalidate();
        }

        private void SliderY_ValueChanged(object sender, System.EventArgs e) {
            _shiftY = SliderY.Value;
            ZoomOrShiftChanged();
            Invalidate();
        }

        #endregion
    }
}