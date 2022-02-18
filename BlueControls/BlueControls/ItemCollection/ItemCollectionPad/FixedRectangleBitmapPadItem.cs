// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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
using BlueControls.EventArgs;
using System;
using System.Collections.Generic;
using System.Drawing;
using static BlueBasics.Extensions;

namespace BlueControls.ItemCollection {

    public abstract class FixedRectangleBitmapPadItem : BasicPadItem {

        #region Fields

        public readonly List<FixedRectangleBitmapPadItem> ConnectsTo = new();
        protected PointM p_L;

        /// <summary>
        /// Dieser Punkt bestimmt die ganzen Koordinaten. Die anderen werden nur mitgeschleift
        /// </summary>
        protected PointM p_LO;

        protected PointM p_LU;
        protected PointM p_O;
        protected PointM p_R;
        protected PointM p_RO;
        protected PointM p_RU;
        protected PointM p_U;

        private Bitmap _GeneratedBitmap = null;

        #endregion

        #region Constructors

        public FixedRectangleBitmapPadItem(string internalname) : base(internalname) {
            p_LO = new PointM(this, "LO", 0, 0);
            p_RO = new PointM(this, "RO", 0, 0);
            p_RU = new PointM(this, "RU", 0, 0);
            p_LU = new PointM(this, "LU", 0, 0);
            p_L = new PointM(this, "L", 0, 0);
            p_R = new PointM(this, "R", 0, 0);
            p_O = new PointM(this, "O", 0, 0);
            p_U = new PointM(this, "U", 0, 0);
            MovablePoint.Add(p_LO);
            MovablePoint.Add(p_RO);
            MovablePoint.Add(p_LU);
            MovablePoint.Add(p_RU);
            MovablePoint.Add(p_L);
            MovablePoint.Add(p_R);
            MovablePoint.Add(p_U);
            MovablePoint.Add(p_O);
            PointsForSuccesfullyMove.Add(p_LO);

            RemovePic();
            //GeneratePic(true);
        }

        #endregion

        #region Properties

        public Bitmap GeneratedBitmap {
            get {
                if (_GeneratedBitmap != null) { return _GeneratedBitmap; }
                _GeneratedBitmap = GeneratePic();
                return _GeneratedBitmap;
            }
        }

        #endregion

        //public override List<FlexiControl> GetStyleOptions() {
        //    List<FlexiControl> l = new()
        //    {
        //        new FlexiControl(),
        //        //new FlexiControlForProperty(this, "Drehwinkel"),
        //        //new FlexiControlForProperty(this, "Größe_fixiert")
        //    };
        //    l.AddRange(base.GetStyleOptions());
        //    return l;
        //}

        //public override bool ParseThis(string tag, string value) {
        //    if (base.ParseThis(tag, value)) { return true; }
        //    switch (tag) {
        //        case "fixsize": // TODO: Entfernt am 24.05.2021
        //            //_größe_fixiert = value.FromPlusMinus();
        //            return true;

        //        case "rotation":
        //            _drehwinkel = int.Parse(value);
        //            return true;
        //    }
        //    return false;
        //}

        #region Methods

        public override void PointMoved(object sender, MoveEventArgs e) {
            base.PointMoved(sender, e);
            var x = 0f;
            var y = 0f;

            var point = (PointM)sender;

            if (point != null) {
                x = point.X;
                y = point.Y;
            }

            if (point == p_LO) {
                if (e.Y) {
                    p_RU.Y = y + GeneratedBitmap.Height;
                    p_O.Y = y;
                }
                if (e.X) {
                    p_RU.X = x + GeneratedBitmap.Width;
                    p_L.X = x;
                }
            }

            if (point == p_RU) {
                if (e.X) {
                    p_LO.X = x - GeneratedBitmap.Width;
                    p_R.X = x;
                }
                if (e.Y) {
                    p_LO.Y = y - GeneratedBitmap.Height;
                    p_U.Y = y;
                }
            }

            if (point == p_RO) {
                if (e.Y) { p_O.Y = y; }
                if (e.X) { p_R.X = x; }
            }

            if (point == p_LU) {
                if (e.X) { p_L.X = x; }
                if (e.Y) { p_U.Y = y; }
            }

            if (point == p_O && e.Y) {
                p_LO.Y = y;
                p_RO.Y = y;
            }

            if (point == p_U && e.Y) {
                p_LU.Y = y;
                p_RU.Y = y;
            }

            if (point == p_L && e.X) {
                p_LO.X = x;
                p_LU.X = x;
            }

            if (point == p_R && e.X) {
                p_RO.X = x;
                p_RU.X = x;
            }

            SizeChanged();
        }

        public void RemovePic() {
            if (_GeneratedBitmap != null) { _GeneratedBitmap.Dispose(); }
            _GeneratedBitmap = null;
        }

        public void SetLeftTopPoint(float x, float y) {
            p_LO.SetTo(x, y);
        }

        //    public void SetCoordinates(RectangleF r) {
        //    var vr = r.PointOf(enAlignment.Horizontal_Vertical_Center);
        //    var ur = UsedArea();
        //    p_LO.SetTo(vr.X - (ur.Width / 2), vr.Y - (ur.Height / 2));
        //    p_RU.SetTo(p_LO.X + ur.Width, p_LO.Y + ur.Height);
        //}

        public virtual void SizeChanged() {
            // Punkte immer komplett setzen. Um eventuelle Parsing-Fehler auszugleichen
            p_L.SetTo(p_LO.X, p_LO.Y + ((p_LU.Y - p_LO.Y) / 2));
            p_R.SetTo(p_RO.X, p_LO.Y + ((p_LU.Y - p_LO.Y) / 2));
            p_U.SetTo(p_LO.X + ((p_RO.X - p_LO.X) / 2), p_RU.Y);
            p_O.SetTo(p_LO.X + ((p_RO.X - p_LO.X) / 2), p_RO.Y);
        }

        //public virtual void SizeChanged() {
        //    // Punkte immer komplett setzen. Um eventuelle Parsing-Fehler auszugleichen
        //    p_L.SetTo(p_LO.X, p_LO.Y + ((p_LU.Y - p_LO.Y) / 2));
        //    p_R.SetTo(p_RO.X, p_LO.Y + ((p_LU.Y - p_LO.Y) / 2));
        //    p_U.SetTo(p_LO.X + ((p_RO.X - p_LO.X) / 2), p_RU.Y);
        //    p_O.SetTo(p_LO.X + ((p_RO.X - p_LO.X) / 2), p_RO.Y);
        //}

        //public override string ToString() {
        //    var t = base.ToString();
        //    t = t.Substring(0, t.Length - 1) + ", ";
        //    if (Drehwinkel != 0) { t = t + "Rotation=" + Drehwinkel + ", "; }
        //    return t.Trim(", ") + "}";
        //}

        protected override RectangleF CalculateUsedArea() {
            var bmp = GeneratedBitmap;
            if (bmp == null || p_LO == null) { return RectangleF.Empty; }
            return new RectangleF(p_LO.X, p_LO.Y, bmp.Width, bmp.Height);
        }

        protected override void DrawExplicit(Graphics gr, RectangleF drawingCoordinates, float zoom, float shiftX, float shiftY, enStates state, Size sizeOfParentControl, bool forPrinting) {

            #region Bild zeichnen

            try {
                if (_GeneratedBitmap != null) {
                    if (forPrinting) {
                        gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        gr.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                    } else {
                        gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
                        gr.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                    }
                    gr.DrawImage(_GeneratedBitmap, drawingCoordinates);
                }
            } catch { }

            #endregion

            var Line = 1f;
            if (zoom > 1) { Line = zoom; }

            #region Umrandung Zeichnen

            try {
                if (!forPrinting || _GeneratedBitmap == null) {
                    gr.DrawRectangle(new Pen(Color.Gray, zoom), drawingCoordinates);

                    if (drawingCoordinates.Width < 1 || drawingCoordinates.Height < 1) {
                        gr.DrawEllipse(new Pen(Color.Gray, 3), drawingCoordinates.Left - 5, drawingCoordinates.Top + 5, 10, 10);
                        gr.DrawLine(ZoomPad.PenGray, drawingCoordinates.PointOf(enAlignment.Top_Left), drawingCoordinates.PointOf(enAlignment.Bottom_Right));
                    }
                }
            } catch { }

            #endregion

            #region Verknüpfte Pfeile Zeichnen

            foreach (var thisV in ConnectsTo) {
                if (Parent.Contains(thisV) && thisV != null && thisV != this) {
                    var m1 = UsedArea.PointOf(enAlignment.Horizontal_Vertical_Center);
                    var m2 = thisV.UsedArea.PointOf(enAlignment.Horizontal_Vertical_Center);

                    var t2 = UsedArea.NearestLineMiddle(m2).ZoomAndMove(zoom, shiftX, shiftY);
                    var t1 = thisV.UsedArea.NearestLineMiddle(m1).ZoomAndMove(zoom, shiftX, shiftY);

                    if (Geometry.GetLenght(t1, t2) > 1) {
                        gr.DrawLine(new Pen(Color.Gray, zoom), t1, t2);
                    }
                }
            }

            #endregion
        }

        protected abstract Bitmap GeneratePic();

        protected override void ParseFinished() => SizeChanged();

        #endregion
    }
}