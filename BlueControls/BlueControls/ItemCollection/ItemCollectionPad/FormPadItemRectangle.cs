#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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
        private bool _größe_fixiert = false;

        public int Drehwinkel {
            get {
                return _drehwinkel;
            }
            set {
                if (_drehwinkel == value) { return; }
                _drehwinkel = value;
                OnChanged();
            }
        }
        public bool Größe_fixiert {
            get {
                return _größe_fixiert;
            }
            set {
                if (_größe_fixiert == value) { return; }

                _größe_fixiert = value;

                //if (Größe_fixiert) {
                //    Points.AddIfNotExists(p_L);
                //    Points.AddIfNotExists(p_R);
                //    Points.AddIfNotExists(p_O);
                //    Points.AddIfNotExists(p_U);
                //} else {
                //    Points.Remove(p_L);
                //    Points.Remove(p_R);
                //    Points.Remove(p_O);
                //    Points.Remove(p_U);
                //}

                RecalculateAndOnChanged();
            }
        }
        #endregion

        #region  Event-Deklarationen + Delegaten 

        #endregion
        #region  Construktor 

        public FormPadItemRectangle(ItemCollectionPad parent, string internalname, bool sizefix) : base(parent, internalname) {
            p_LO = new PointM(this, "LO", 0, 0, false, true, true);
            p_RO = new PointM(this, "RO", 0, 0, false);
            p_RU = new PointM(this, "RU", 0, 0);
            p_LU = new PointM(this, "LU", 0, 0);

            p_L = new PointM(this, "L", 0, 0, false) {
                UserSelectable = false
            };
            p_R = new PointM(this, "R", 0, 0, false) {
                UserSelectable = false
            };
            p_O = new PointM(this, "O", 0, 0, false) {
                UserSelectable = false
            };
            p_U = new PointM(this, "U", 0, 0, false) {
                UserSelectable = false
            };

            Points.Add(p_LO);
            Points.Add(p_RO);
            Points.Add(p_LU);
            Points.Add(p_RU);
            Points.Add(p_L);
            Points.Add(p_R);
            Points.Add(p_U);
            Points.Add(p_O);

            Größe_fixiert = sizefix;


            Drehwinkel = 0;
        }

        #endregion

        public override void Move(decimal x, decimal y) {
            p_LO.SetTo(p_LO.X + x, p_LO.Y + y);
            p_RU.SetTo(p_RU.X + x, p_RU.Y + y);
            base.Move(x, y);
        }



        protected override void GenerateInternalRelationExplicit() {
            if (Größe_fixiert) {
                Relations.Add(new clsPointRelation(Parent, this, enRelationType.PositionZueinander, p_LO, p_RO));
                Relations.Add(new clsPointRelation(Parent, this, enRelationType.PositionZueinander, p_LO, p_RU));
                Relations.Add(new clsPointRelation(Parent, this, enRelationType.PositionZueinander, p_LO, p_LU));

                Relations.Add(new clsPointRelation(Parent, this, enRelationType.PositionZueinander, p_LO, p_L));
                Relations.Add(new clsPointRelation(Parent, this, enRelationType.PositionZueinander, p_LO, p_R));
                Relations.Add(new clsPointRelation(Parent, this, enRelationType.PositionZueinander, p_LO, p_O));
                Relations.Add(new clsPointRelation(Parent, this, enRelationType.PositionZueinander, p_LO, p_U));

            } else {
                Relations.Add(new clsPointRelation(Parent, this, enRelationType.WaagerechtSenkrecht, p_LO, p_RO));
                Relations.Add(new clsPointRelation(Parent, this, enRelationType.WaagerechtSenkrecht, p_RU, p_LU));

                Relations.Add(new clsPointRelation(Parent, this, enRelationType.WaagerechtSenkrecht, p_LO, p_LU));
                Relations.Add(new clsPointRelation(Parent, this, enRelationType.WaagerechtSenkrecht, p_RO, p_RU));

            }
        }

        public override List<FlexiControl> GetStyleOptions() {
            var l = new List<FlexiControl>
            {
                new FlexiControl(),
                new FlexiControlForProperty(this, "Drehwinkel")
            };

            if (!Größe_fixiert && !p_LO.CanMove(Parent.AllRelations) && !p_RU.CanMove(Parent.AllRelations)) {
                l.Add(new FlexiControl());
                l.Add(new FlexiControl("Objekt fest definiert,<br>Größe kann nicht fixiert werden"));
            } else {
                l.Add(new FlexiControlForProperty(this, "Größe_fixiert"));
            }

            l.AddRange(base.GetStyleOptions());
            return l;
        }


        public void SetCoordinates(RectangleM r, bool overrideFixedSize) {

            if (_größe_fixiert && !overrideFixedSize) {
                var vr = r.PointOf(enAlignment.Horizontal_Vertical_Center);
                var ur = UsedArea();

                p_LO.SetTo(vr.X - ur.Width / 2, vr.Y - ur.Height / 2);
                p_RU.SetTo(p_LO.X + ur.Width, p_LO.Y + ur.Height);

            } else {

                p_LO.SetTo(r.PointOf(enAlignment.Top_Left));
                p_RU.SetTo(r.PointOf(enAlignment.Bottom_Right));
            }

            RecalculateAndOnChanged();
        }


        protected override RectangleM CalculateUsedArea() {
            if (p_LO == null || p_RU == null) { return new RectangleM(); }
            return new RectangleM(Math.Min(p_LO.X, p_RU.X), Math.Min(p_LO.Y, p_RU.Y), Math.Abs(p_RU.X - p_LO.X), Math.Abs(p_RU.Y - p_LO.Y));
        }

        public override void CaluclatePointsWORelations() {
            p_RO.SetTo(p_RU.X, p_LO.Y);
            p_LU.SetTo(p_LO.X, p_RU.Y);



            p_L.SetTo(p_LO.X, p_LO.Y + (p_LU.Y - p_LO.Y) / 2);
            p_R.SetTo(p_RO.X, p_L.Y);

            p_O.SetTo(p_LO.X + (p_RO.X - p_LO.X) / 2, p_LO.Y);
            p_U.SetTo(p_O.X, p_LU.Y);

            base.CaluclatePointsWORelations();
        }


        public override bool ParseThis(string tag, string value) {
            if (base.ParseThis(tag, value)) { return true; }

            switch (tag) {
                case "fixsize":
                    _größe_fixiert = value.FromPlusMinus();
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
            t = t + "Fixsize=" + Größe_fixiert.ToPlusMinus() + ", ";
            return t.Trim(", ") + "}";
        }

        protected override void DrawExplicit(Graphics GR, RectangleF DCoordinates, decimal cZoom, decimal shiftX, decimal shiftY, enStates vState, Size SizeOfParentControl, bool ForPrinting) {
            try {

                if (!ForPrinting) {
                    if (cZoom > 1) {
                        GR.DrawRectangle(new Pen(Color.Gray, (float)cZoom), DCoordinates);
                    } else {
                        GR.DrawRectangle(CreativePad.PenGray, DCoordinates);
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
