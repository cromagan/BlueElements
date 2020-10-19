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
    class clsPhysicPadItem : clsAbstractPhysicPadItem
    {




        public clsPhysicPadItem(ItemCollectionPad parent) : base(parent, string.Empty)
        {

        }


        protected override string ClassId()
        {
            return "Physics-Object";
        }
        //public override bool Contains(PointF value, decimal zoomfactor)
        //{
        //    throw new NotImplementedException();
        //}


        //public override RectangleM UsedArea()
        //{
        //    throw new NotImplementedException();
        //}

        //protected override string ClassId()
        //{
        //    throw new NotImplementedException();
        //}

        //protected override void DrawExplicit(Graphics GR, RectangleF DCoordinates, decimal cZoom, decimal MoveX, decimal MoveY, enStates vState, Size SizeOfParentControl, bool ForPrinting)
        //{
        //    throw new NotImplementedException();
        //}

        //protected override void GenerateInternalRelationExplicit()
        //{
        //    throw new NotImplementedException();
        //}

        //protected override void ParseFinished()
        //{
        //    throw new NotImplementedException();
        //}
    }
}
