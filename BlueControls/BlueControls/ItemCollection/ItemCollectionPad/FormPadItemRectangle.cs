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

namespace BlueControls.ItemCollection
{
    public abstract class FormPadItemRectangle : BasicPadItem
    {

        #region  Variablen-Deklarationen 

        internal PointDF p_LO;
        internal PointDF p_RO;
        internal PointDF p_RU;
        internal PointDF p_LU;
        public int Drehwinkel { get; set; } = 0;
        public bool Größe_fixiert { get; set; } = false;
        #endregion

        #region  Event-Deklarationen + Delegaten 

        #endregion
        #region  Construktor 

        public FormPadItemRectangle(ItemCollectionPad parent, string internalname) : base(parent, internalname)
        {
            p_LO = new PointDF(this, "LO", 0, 0, false, true, true);
            p_RO = new PointDF(this, "RO", 0, 0);
            p_RU = new PointDF(this, "RU", 0, 0);
            p_LU = new PointDF(this, "LU", 0, 0);


            Points.Add(p_LO);
            Points.Add(p_RO);
            Points.Add(p_LU);
            Points.Add(p_RU);

            Drehwinkel = 0;
        }

        #endregion

        public override void Move(decimal x, decimal y)
        {
            p_LO.SetTo(p_LO.X + x, p_LO.Y + y);
            p_RU.SetTo(p_RU.X + x, p_RU.Y + y);
            base.Move(x, y);
        }

        public override bool Contains(PointF value, decimal zoomfactor)
        {
            var tmp = UsedArea();
            var ne = (int)(5 / zoomfactor);
            tmp.Inflate(-ne, -ne);
            return tmp.Contains(value.ToPointDF());
        }


        protected override void GenerateInternalRelationExplicit()
        {
            if (Größe_fixiert)
            {
                Relations.Add(new clsPointRelation(Parent, this, enRelationType.PositionZueinander, p_LO, p_RO));
                Relations.Add(new clsPointRelation(Parent, this, enRelationType.PositionZueinander, p_LO, p_RU));
                Relations.Add(new clsPointRelation(Parent, this, enRelationType.PositionZueinander, p_LO, p_LU));
            }
            else
            {
                //relations.Add(new clsPointRelation(enRelationType.YPositionZueinander, p_LO, p_RU));

                Relations.Add(new clsPointRelation(Parent, this, enRelationType.WaagerechtSenkrecht, p_LO, p_RO));
                Relations.Add(new clsPointRelation(Parent, this, enRelationType.WaagerechtSenkrecht, p_RU, p_LU));

                Relations.Add(new clsPointRelation(Parent, this, enRelationType.WaagerechtSenkrecht, p_LO, p_LU));
                Relations.Add(new clsPointRelation(Parent, this, enRelationType.WaagerechtSenkrecht, p_RO, p_RU));
            }
        }

        public override List<FlexiControl> GetStyleOptions()
        {
            var l = new List<FlexiControl>
            {
                new FlexiControl(),
                new FlexiControlForProperty(this, "Drehwinkel")
            };

            if (!Größe_fixiert && !p_LO.CanMove(Parent.AllRelations) && !p_RU.CanMove(Parent.AllRelations))
            {
                l.Add(new FlexiControl());
                l.Add(new FlexiControl("Objekt fest definiert,<br>Größe kann nicht fixiert werden"));
            }
            else
            {
                l.Add(new FlexiControlForProperty(this, "Größe_fixiert"));
            }

            l.AddRange(base.GetStyleOptions());
            return l;
        }


        public override void SetCoordinates(RectangleDF r)
        {
            p_LO.SetTo(r.PointOf(enAlignment.Top_Left));
            p_RU.SetTo(r.PointOf(enAlignment.Bottom_Right));
            base.SetCoordinates(r);
        }

        public override RectangleDF UsedArea()
        {
            if (p_LO == null || p_RU == null) { return new RectangleDF(); }
            return new RectangleDF(Math.Min(p_LO.X, p_RU.X), Math.Min(p_LO.Y, p_RU.Y), Math.Abs(p_RU.X - p_LO.X), Math.Abs(p_RU.Y - p_LO.Y));
        }

        public override void CaluclatePointsWORelations()
        {
            p_RO.SetTo(p_RU.X, p_LO.Y);
            p_LU.SetTo(p_LO.X, p_RU.Y);

            base.CaluclatePointsWORelations();
        }


        public override bool ParseThis(string tag, string value)
        {
            if (base.ParseThis(tag, value)) { return true; }

            switch (tag)
            {
                case "fixsize":
                    Größe_fixiert = value.FromPlusMinus();
                    return true;

                case "rotation":
                    Drehwinkel = int.Parse(value);
                    return true;
            }
            return false;
        }
        public override string ToString()
        {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";
            if (Drehwinkel != 0) { t = t + "Rotation=" + Drehwinkel + ", "; }
            t = t + "Fixsize=" + Größe_fixiert.ToPlusMinus() + ", ";
            return t.Trim(", ") + "}";
        }

        protected override void DrawExplicit(Graphics GR, RectangleF DCoordinates, decimal cZoom, decimal MoveX, decimal MoveY, enStates vState, Size SizeOfParentControl, bool ForPrinting)
        {
            try
            {

                if (!ForPrinting)
                {
                    GR.DrawRectangle(CreativePad.PenGray, DCoordinates);
                }
            }
            catch { }
        }
    }
}
