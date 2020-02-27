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

using BlueBasics.Enums;
using System.Drawing;

namespace BlueControls.ItemCollection
{
    public abstract class BasicPadItemTwoPoints : BasicPadItem
    {
        private decimal _laengePix;
        private decimal _breitePix;


        public PointDF p_ML { get; protected set; }
        public PointDF p_MR { get; protected set; }

        public PointDF p_OL { get; private set; }
        public PointDF p_OR { get; private set; }
        public PointDF p_UL { get; private set; }
        public PointDF p_UR { get; private set; }






        public BasicPadItemTwoPoints(ItemCollectionPad parent) : base(parent, string.Empty)
        {
            p_ML = new PointDF(this, "ML", 0, 0);
            p_MR = new PointDF(this, "MR", 1000, 0);
            p_OL = new PointDF();
            p_OR = new PointDF();
            p_UL = new PointDF();
            p_UR = new PointDF();


            Points.Add(p_ML);
            Points.Add(p_MR);


        }

        public decimal LaengePix
        {
            get
            {
                return _laengePix;
            }
            set

            {
                if (value == _laengePix) { return; }

                _laengePix = value;
                KeepInternalLogic();

            }
        }


        public decimal BreitePix
        {
            get
            { return _breitePix; }
            set

            {
                if (value == _breitePix) { return; }

                _breitePix = value;
                KeepInternalLogic();

            }
        }





        public decimal WinkelMLtoMR()
        {

            return GeometryDF.Winkel(p_ML, p_MR);
        }

        protected override void KeepInternalLogic()
        {

            var winkel = WinkelMLtoMR();

            p_UL.SetTo(p_ML, BreitePix / 2, winkel - 90);
            p_OL.SetTo(p_ML, BreitePix / 2, winkel + 90);
            p_UR.SetTo(p_MR, BreitePix / 2, winkel - 90);
            p_OR.SetTo(p_MR, BreitePix / 2, winkel + 90);

        }





        public override void GenerateInternalRelation()
        {
            Relations.Clear();
            Relations.Add(new clsPointRelation(Parent, enRelationType.AbstandZueinander, p_ML, p_MR));
            OnPointOrRelationsChanged();
        }



        public override bool Contains(PointF value, decimal zoomfactor)
        {
            if (!UsedArea().Contains((decimal)value.X, (decimal)value.Y))
            {
                return false;
            }

            //TODO: Wirklich im Rechteck?

            return true;
        }


        public override void Move(decimal x, decimal y)
        {
            p_ML.SetTo(p_ML.X + x, p_ML.Y + y);
            p_MR.SetTo(p_MR.X + x, p_MR.Y + y);

            RecomputePointAndRelations();
        }


        public override void SetCoordinates(RectangleDF r)
        {

            p_ML.SetTo(r.PointOf(enAlignment.VerticalCenter_Left));
            p_MR.SetTo(r.PointOf(enAlignment.VerticalCenter_Right));


            RecomputePointAndRelations();
        }

        public override RectangleDF UsedArea()
        {
           return new RectangleDF(p_OL, p_UR);
        }


    }
}
