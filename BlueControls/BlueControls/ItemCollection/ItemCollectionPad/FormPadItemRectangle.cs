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
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.ItemCollection {
    public abstract class FormPadItemRectangle : BasicPadItem {

        #region  Variablen-Deklarationen 
        internal PointM p_LO;
        internal PointM p_RO;
        internal PointM p_RU;
        internal PointM p_LU;
        internal PointM p_O;
        internal PointM p_U;
        internal PointM p_L;
        internal PointM p_R;
        private int _drehwinkel = 0;
        //private bool _größe_fixiert = false;
        public int Drehwinkel {
            get => _drehwinkel;
            set {
                if (_drehwinkel == value) { return; }
                _drehwinkel = value;
                OnChanged();
            }
        }
        #endregion


        #region  Event-Deklarationen + Delegaten 
        #endregion


        #region  Construktor 
        public FormPadItemRectangle(ItemCollectionPad parent, string internalname) : base(parent, internalname) {
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
            PointsForSuccesfullyMove.Add(p_RU);
            Drehwinkel = 0;
        }
        #endregion

        public override List<FlexiControl> GetStyleOptions() {
            List<FlexiControl> l = new()
            {
                new FlexiControl(),
                new FlexiControlForProperty(this, "Drehwinkel")
            };
            l.Add(new FlexiControlForProperty(this, "Größe_fixiert"));
            l.AddRange(base.GetStyleOptions());
            return l;
        }
        public void SetCoordinates(RectangleM r, bool overrideFixedSize) {
            if (!overrideFixedSize) {
                var vr = r.PointOf(enAlignment.Horizontal_Vertical_Center);
                var ur = UsedArea();
                p_LO.SetTo(vr.X - (ur.Width / 2), vr.Y - (ur.Height / 2));
                p_RU.SetTo(p_LO.X + ur.Width, p_LO.Y + ur.Height);
            } else {
                p_LO.SetTo(r.PointOf(enAlignment.Top_Left));
                p_RU.SetTo(r.PointOf(enAlignment.Bottom_Right));
            }
        }
        protected override RectangleM CalculateUsedArea() => p_LO == null || p_RU == null ? new RectangleM()
: new RectangleM(Math.Min(p_LO.X, p_RU.X), Math.Min(p_LO.Y, p_RU.Y), Math.Abs(p_RU.X - p_LO.X), Math.Abs(p_RU.Y - p_LO.Y));
        public override void PointMoved(PointM point) {
            //PointM Second = null;
            var x = 0m;
            var y = 0m;
            if (point != null) {
                x = point.X;
                y = point.Y;
            }
            if (point == p_LO) {
                p_O.Y = y;
                p_L.X = x;
            }
            if (point == p_RO) {
                p_O.Y = y;
                p_R.X = x;
            }
            if (point == p_LU) {
                p_L.X = x;
                p_U.Y = y;
            }
            if (point == p_RU) {
                p_R.X = x;
                p_U.Y = y;
            }
            if (point == p_O) {
                p_LO.Y = y;
                p_RO.Y = y;
            }
            if (point == p_U) {
                p_LU.Y = y;
                p_RU.Y = y;
            }
            if (point == p_L) {
                p_LO.X = x;
                p_LU.X = x;
            }
            if (point == p_R) {
                p_RO.X = x;
                p_RU.X = x;
            }
            if (point is null) {
                //  Wichtig, weil wenn p_r, p_l, p_o, p_u mittig gesetzt werde und diese auf null sind, ist das schlecht.
                var x1 = p_RU.X;
                var y1 = p_RU.Y;
                PointMoved(p_LO);
                p_RU.SetTo(x1, y1);
                return;
            }
            //p_RO.SetTo(p_RU.X, p_LO.Y);
            //p_LU.SetTo(p_LO.X, p_RU.Y);
            p_L.Y = p_LO.Y + ((p_LU.Y - p_LO.Y) / 2);
            p_R.Y = p_L.Y;
            p_O.X = p_LO.X + ((p_RO.X - p_LO.X) / 2);
            p_U.X = p_O.X;
        }
        public override bool ParseThis(string tag, string value) {
            if (base.ParseThis(tag, value)) { return true; }
            switch (tag) {
                case "fixsize": // TODO: Entfernt am 24.05.2021
                    //_größe_fixiert = value.FromPlusMinus();
                    return true;
                case "rotation":
                    _drehwinkel = int.Parse(value);
                    return true;
            }
            return false;
        }
        public override string ToString() {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";
            if (Drehwinkel != 0) { t = t + "Rotation=" + Drehwinkel + ", "; }
            return t.Trim(", ") + "}";
        }
        protected override void DrawExplicit(Graphics GR, RectangleF DCoordinates, decimal cZoom, decimal shiftX, decimal shiftY, enStates vState, Size SizeOfParentControl, bool ForPrinting) {
            try {
                if (!ForPrinting) {
                    if (cZoom > 1) {
                        GR.DrawRectangle(new Pen(Color.Gray, (float)cZoom), DCoordinates);
                    } else {
                        GR.DrawRectangle(ZoomPad.PenGray, DCoordinates);
                    }
                    if (DCoordinates.Width < 1 || DCoordinates.Height < 1) {
                        GR.DrawEllipse(new Pen(Color.Gray, 3), DCoordinates.Left - 5, DCoordinates.Top + 5, 10, 10);
                    }
                }
            } catch { }
        }
        //internal PointM PointOf(enAlignment P)
        //{
        //    switch (P)
        //    {
        //        case enAlignment.Bottom_Left: return p_LU;
        //        case enAlignment.Bottom_Right: return p_RU;
        //        case enAlignment.Top_Left: return p_LO;
        //        case enAlignment.Top_Right: return p_RO;
        //        case enAlignment.Bottom_HorizontalCenter: return p_U;
        //        case enAlignment.Top_HorizontalCenter: return p_O;
        //        case enAlignment.VerticalCenter_Left: return p_L;
        //        case enAlignment.VerticalCenter_Right: return p_R;
        //        default: return null;
        //    }
        //}
    }
}
