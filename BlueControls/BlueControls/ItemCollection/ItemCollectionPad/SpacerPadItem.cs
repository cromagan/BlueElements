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
    public class SpacerPadItem : FormPadItemRectangle
    {


        #region  Variablen-Deklarationen 


        private decimal _Größe_Distanzhalter;

        private readonly decimal mm125x; //Math.Round(mmToPixel(1.25D, _DPIx), 1)

        public decimal Größe_Distanzhalter
        {
            get => _Größe_Distanzhalter;


            set
            {

                if (value < mm125x * 1m) { value = mm125x * 1m; }

                if (value == _Größe_Distanzhalter) { return; }

                _Größe_Distanzhalter = value;


                p_RU.SetTo(p_LO.X + _Größe_Distanzhalter, p_LO.Y + _Größe_Distanzhalter);

                RecalculateAndOnChanged();

            }
        }

        #endregion


        #region  Event-Deklarationen + Delegaten 

        #endregion


        #region  Construktor + Initialize 


        public SpacerPadItem(ItemCollectionPad parent) : this(parent, string.Empty) { }


        public SpacerPadItem(ItemCollectionPad parent, string internalname) : base(parent, internalname, true)
        {
            mm125x = Math.Round(modConverter.mmToPixel(1.25M, ItemCollectionPad.DPI), 1);

            Größe_Distanzhalter = mm125x * 2; // 19,68 = 2,5 mm

            Größe_fixiert = true;


        }


        #endregion


        #region  Properties 


        #endregion


        public override void DesignOrStyleChanged()
        {
            // Muss angepasst werden, evtl. wegen 70% größe
            CaluclatePointsWORelations();
        }

        protected override string ClassId()
        {
            return "SPACER";
        }

        //public override bool Contains(PointF value, decimal zoomfactor)
        //{
        //    var mp = UsedArea().PointOf(enAlignment.Horizontal_Vertical_Center);
        //    return GeometryDF.Länge(value.ToPointM(), mp) < decimal.Parse(Größe_Distanzhalter) / 2;
        //}



        protected override void DrawExplicit(Graphics GR, RectangleF DCoordinates, decimal cZoom, decimal shiftX, decimal shiftY, enStates vState, Size SizeOfParentControl, bool ForPrinting)
        {
            if (ForPrinting) { return; }

            GR.DrawEllipse(CreativePad.PenGray, DCoordinates);
        }


        //public override void Move(decimal x, decimal y)
        //{
        //    p_M.SetTo(p_M.X + x, p_M.Y + y);
        //    base.Move(x, y);
        //}


        //public override void SetCoordinates(RectangleM r)
        //{
        //    p_M.SetTo(r.PointOf(enAlignment.Horizontal_Vertical_Center));
        //    base.SetCoordinates(r);
        //}


        //        protected override RectangleM CalculateUsedArea()
        //{
        //    var t = decimal.Parse(Größe_Distanzhalter) * Parent.SheetStyleScale;
        //    return new RectangleM(p_L.X, p_O.Y, t, t);
        //}



        public override bool ParseThis(string tag, string value)
        {
            if (base.ParseThis(tag, value)) { return true; }

            switch (tag)
            {
                case "size":
                    Größe_Distanzhalter = BlueBasics.modConverter.DecimalParse(value.FromNonCritical());
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
            return t + "Size=" + Größe_Distanzhalter.ToString(Constants.Format_Float10).ToNonCritical() + "}";
        }


        //public override void CaluclatePointsWORelations()
        //{
        //    var p_M = new PointM((p_LO.X + p_RU.Y) / 2, (p_LO.Y + p_RU.Y) / 2);


        //    var t = decimal.Parse(Größe_Distanzhalter) * Parent.SheetStyleScale / 2;
        //    p_O.SetTo(p_M.X, p_M.Y - t);
        //    p_U.SetTo(p_M.X, p_M.Y + t);
        //    p_L.SetTo(p_M.X - t, p_M.Y);
        //    p_R.SetTo(p_M.X + t, p_M.Y);

        //    base.CaluclatePointsWORelations();
        //}


        // protected override void GenerateInternalRelationExplicit()
        // {
        //}


        public override List<FlexiControl> GetStyleOptions()
        {
            var l = new List<FlexiControl>();


            var Size = new ItemCollectionList
            {
                { "Klein (1,25 mm)", (mm125x * 1m).ToString(Constants.Format_Float4), enImageCode.GrößeÄndern },
                { "Normal (2,5 mm)", (mm125x * 2m).ToString(Constants.Format_Float4), enImageCode.GrößeÄndern },
                { "Groß (5,0 mm)", (mm125x * 4m).ToString(Constants.Format_Float4), enImageCode.GrößeÄndern },
                { "Sehr groß (10,0 mm)", (mm125x * 5m).ToString(Constants.Format_Float4), enImageCode.GrößeÄndern }
            };


            l.Add(new FlexiControlForProperty(this, "Größe Distanzhalter", Size));

            l.AddRange(base.GetStyleOptions());
            return l;
        }

        protected override void ParseFinished() { }

        //public override void DoStyleCommands(object sender, List<string> Tags, ref bool CloseMenu)
        //{

        //    _Size = decimal.Parse(Tags.TagGet("Größe Distanzhalter").FromNonCritical());
        //    SetCoordinates(new RectangleM(p_m.X - 5, p_m.Y - 5, 10, 10));

        //}
    }
}