using BlueBasics;
using BlueControls;
using BlueControls.Enums;
using BlueControls.ItemCollection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueControls.ItemCollection
{
    abstract class clsAbstractPhysicPadItem : BasicPadItem
    {
        //    https://stackoverflow.com/questions/4243042/c-sharp-point-in-polygon


        protected clsAbstractPhysicPadItem(ItemCollectionPad parent, string internalname) : base(parent, internalname)
        {
            Points.Add(new PointM(5, 0));
            Points.Add(new PointM(10, 10));
            Points.Add(new PointM(0, 10));
        }

        protected override RectangleM CalculateUsedArea()
        {
            var minx = decimal.MaxValue;
            var miny = decimal.MaxValue;
            var maxx = decimal.MinValue;
            var maxy = decimal.MinValue;


            foreach (var thisP in Points)
            {
                minx = Math.Min(minx, thisP.X);
                maxx = Math.Max(maxx, thisP.X);
                miny = Math.Min(miny, thisP.Y);
                maxy = Math.Max(maxy, thisP.Y);
            }

            return new RectangleM(minx, miny, maxx - minx, maxy - miny);

        }






        protected override void DrawExplicit(Graphics GR, RectangleF DCoordinates, decimal cZoom, decimal MoveX, decimal MoveY, enStates vState, Size SizeOfParentControl, bool ForPrinting)
        {

            if (Points.Count <1) { return; }

            var lastP = Points[Points.Count - 1];

            foreach (var thisP in Points)
            {
                GR.DrawLine(Pens.Black, lastP.ZoomAndMove(cZoom,MoveX, MoveY), thisP.ZoomAndMove(cZoom, MoveX, MoveY));
                lastP = thisP;
            }
            


        }

        protected override void GenerateInternalRelationExplicit() { }

        protected override void ParseFinished() { }
    }
}
