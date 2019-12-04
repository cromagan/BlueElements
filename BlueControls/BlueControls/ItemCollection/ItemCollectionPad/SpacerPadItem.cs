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
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.ItemCollection
{
    public class SpacerPadItem : BasicPadItem
    {


        #region  Variablen-Deklarationen 

        internal PointDF p_o;
        internal PointDF p_u;
        internal PointDF p_l;
        internal PointDF p_r;
        internal PointDF p_m;


        readonly private decimal mm125x; //Math.Round(mmToPixel(1.25D, _DPIx), 1)

        private decimal _Size;

        #endregion


        #region  Event-Deklarationen + Delegaten 

        #endregion


        #region  Construktor + Initialize 


        public SpacerPadItem() : this(string.Empty) { }


        public SpacerPadItem(string internalname) : base(internalname)
        {
            mm125x = Math.Round(modConverter.mmToPixel(1.25M, ItemCollectionPad.DPI), 1);

            _Size = mm125x * 2; // 19,68 = 2,5 mm
            p_o = new PointDF(this, "O", 0, 0);
            p_u = new PointDF(this, "U", 0, 0);
            p_l = new PointDF(this, "L", 0, 0);
            p_r = new PointDF(this, "R", 0, 0);
            p_m = new PointDF(this, "M", 0, 0, false, true, true);

        }





        #endregion


        #region  Properties 


        #endregion


        public override void DesignOrStyleChanged()
        {
            // Muss angepasst werden, evtl. wegen 70% größe
            SetCoordinates(new RectangleDF(p_m.X - 5, p_m.Y - 5, 10, 10));
        }

        protected override string ClassId()
        {
            return "SPACER";
        }

        public override bool Contains(PointF value, decimal zoomfactor)
        {
            var mp = UsedArea().PointOf(enAlignment.Horizontal_Vertical_Center);
            return GeometryDF.Länge(value.ToPointDF(), mp) < _Size / 2;
        }



        protected override void DrawExplicit(Graphics GR, Rectangle DCoordinates, decimal cZoom, decimal MoveX, decimal MoveY, enStates vState, Size SizeOfParentControl, bool ForPrinting)
        {
            if (ForPrinting) { return; }
            GR.DrawEllipse(CreativePad.PenGray, DCoordinates);
        }


        public override List<PointDF> PointList()
        {

            var l = new List<PointDF>();
            l.Add(p_m);
            l.Add(p_o);
            l.Add(p_u);
            l.Add(p_l);
            l.Add(p_r);

            return l;
        }

        public override void SetCoordinates(RectangleDF r)
        {
            p_m.SetTo(r.PointOf(enAlignment.Horizontal_Vertical_Center));
            RecomputePointAndRelations();
        }


        public override RectangleDF UsedArea()
        {
            return new RectangleDF(p_l.X, p_o.Y, _Size * Parent.SheetStyleScale, _Size * Parent.SheetStyleScale);
        }



        public override bool ParseThis(string tag, string value)
        {
            if (base.ParseThis(tag, value)) { return true; }

            switch (tag)
            {
                case "size":
                    _Size = decimal.Parse(value);
                    return true;
                case "checked":
                    return true;
            }

            return false;
        }





        public override string ToString()
        {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";
            return t + "Size=" + _Size + "}";
        }


        protected override void KeepInternalLogic()
        {
            p_o.SetTo(p_m.X, p_m.Y - _Size * Parent.SheetStyleScale / 2);
            p_u.SetTo(p_m.X, p_m.Y + _Size * Parent.SheetStyleScale / 2);
            p_l.SetTo(p_m.X - _Size * Parent.SheetStyleScale / 2, p_m.Y);
            p_r.SetTo(p_m.X + _Size * Parent.SheetStyleScale / 2, p_m.Y);
        }


        public override void GenerateInternalRelation(List<clsPointRelation> relations)
        {
            relations.Add(new clsPointRelation(enRelationType.PositionZueinander, p_m, p_u));
            relations.Add(new clsPointRelation(enRelationType.PositionZueinander, p_m, p_o));
            relations.Add(new clsPointRelation(enRelationType.PositionZueinander, p_m, p_r));
            relations.Add(new clsPointRelation(enRelationType.PositionZueinander, p_m, p_l));
        }

        public override List<FlexiControl> GetStyleOptions(object sender, System.EventArgs e)
        {
            var l = new List<FlexiControl>();

            var Size = new ItemCollectionList();
            Size.Add(new TextListItem((mm125x * 1m).ToString(Constants.Format_Float4), "Klein (1,25 mm)", enImageCode.GrößeÄndern));
            Size.Add(new TextListItem((mm125x * 2m).ToString(Constants.Format_Float4), "Normal (2,5 mm)", enImageCode.GrößeÄndern));
            Size.Add(new TextListItem((mm125x * 4m).ToString(Constants.Format_Float4), "Groß (5,0 mm)", enImageCode.GrößeÄndern));
            Size.Add(new TextListItem((mm125x * 5m).ToString(Constants.Format_Float4), "Sehr groß (10,0 mm)", enImageCode.GrößeÄndern));

            l.Add(new FlexiControl("Größe Distanzhalter", _Size.ToString(Constants.Format_Float4), Size));


            return l;
        }

        public override void DoStyleCommands(object sender, List<string> Tags, ref bool CloseMenu)
        {

            _Size = decimal.Parse(Tags.TagGet("Größe Distanzhalter").FromNonCritical());
            SetCoordinates(new RectangleDF(p_m.X - 5, p_m.Y - 5, 10, 10));

        }
    }
}