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




        private readonly decimal mm125x; //Math.Round(mmToPixel(1.25D, _DPIx), 1)

        public string Größe_Distanzhalter { get; set; }

        #endregion


        #region  Event-Deklarationen + Delegaten 

        #endregion


        #region  Construktor + Initialize 


        public SpacerPadItem(ItemCollectionPad parent) : this(parent, string.Empty) { }


        public SpacerPadItem(ItemCollectionPad parent, string internalname) : base(parent, internalname)
        {
            mm125x = Math.Round(modConverter.mmToPixel(1.25M, ItemCollectionPad.DPI), 1);

            Größe_Distanzhalter = (mm125x * 2).ToString(Constants.Format_Float10); // 19,68 = 2,5 mm
            //p_O = new PointM(this, "O", 0, 0);
            //p_U = new PointM(this, "U", 0, 0);
            //p_L = new PointM(this, "L", 0, 0);
            //p_R = new PointM(this, "R", 0, 0);
            //p_M = new PointM(this, "M", 0, 0, false, true, true);


            //Points.Add(p_M);
            //Points.Add(p_O);
            //Points.Add(p_U);
            //Points.Add(p_L);
            //Points.Add(p_R);

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

        public override bool Contains(PointF value, decimal zoomfactor)
        {
            var mp = UsedArea().PointOf(enAlignment.Horizontal_Vertical_Center);
            return GeometryDF.Länge(value.ToPointM(), mp) < decimal.Parse(Größe_Distanzhalter) / 2;
        }



        protected override void DrawExplicit(Graphics GR, RectangleF DCoordinates, decimal cZoom, decimal MoveX, decimal MoveY, enStates vState, Size SizeOfParentControl, bool ForPrinting)
        {
            if (ForPrinting) { return; }
            GR.DrawEllipse(CreativePad.PenGray, DCoordinates);
        }


        //public override void Move(decimal x, decimal y)
        //{
        //    p_M.SetTo(p_M.X + x, p_M.Y + y);
        //    base.Move(x, y);
        //}


        //public override void SetCoordinates(RectangleDF r)
        //{
        //    p_M.SetTo(r.PointOf(enAlignment.Horizontal_Vertical_Center));
        //    base.SetCoordinates(r);
        //}


        //public override RectangleDF UsedArea()
        //{
        //    var t = decimal.Parse(Größe_Distanzhalter) * Parent.SheetStyleScale;
        //    return new RectangleDF(p_L.X, p_O.Y, t, t);
        //}



        public override bool ParseThis(string tag, string value)
        {
            if (base.ParseThis(tag, value)) { return true; }

            switch (tag)
            {
                case "size":
                    Größe_Distanzhalter = value.FromNonCritical();
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
            return t + "Size=" + Größe_Distanzhalter.ToNonCritical() + "}";
        }


        public override void CaluclatePointsWORelations()
        {
            var p_M = new PointM((p_LO.X + p_RU.Y) / 2, (p_LO.Y + p_RU.Y) / 2);


            var t = decimal.Parse(Größe_Distanzhalter) * Parent.SheetStyleScale / 2;
            p_O.SetTo(p_M.X, p_M.Y - t);
            p_U.SetTo(p_M.X, p_M.Y + t);
            p_L.SetTo(p_M.X - t, p_M.Y);
            p_R.SetTo(p_M.X + t, p_M.Y);

            base.CaluclatePointsWORelations();
        }


        // protected override void GenerateInternalRelationExplicit()
        // {
        //}


        public override List<FlexiControl> GetStyleOptions()
        {
            var l = new List<FlexiControl>();


            var Size = new ItemCollectionList
            {
                new TextListItem((mm125x * 1m).ToString(Constants.Format_Float4), "Klein (1,25 mm)", enImageCode.GrößeÄndern),
                new TextListItem((mm125x * 2m).ToString(Constants.Format_Float4), "Normal (2,5 mm)", enImageCode.GrößeÄndern),
                new TextListItem((mm125x * 4m).ToString(Constants.Format_Float4), "Groß (5,0 mm)", enImageCode.GrößeÄndern),
                new TextListItem((mm125x * 5m).ToString(Constants.Format_Float4), "Sehr groß (10,0 mm)", enImageCode.GrößeÄndern)
            };

            l.Add(new FlexiControlForProperty(this, "Größe Distanzhalter", Size));

            l.AddRange(base.GetStyleOptions());
            return l;
        }

        protected override void ParseFinished() { }

        //public override void DoStyleCommands(object sender, List<string> Tags, ref bool CloseMenu)
        //{

        //    _Size = decimal.Parse(Tags.TagGet("Größe Distanzhalter").FromNonCritical());
        //    SetCoordinates(new RectangleDF(p_m.X - 5, p_m.Y - 5, 10, 10));

        //}
    }
}