﻿using System.Collections.Generic;


namespace BlueControls.ItemCollection
{
    internal class clsPhysicPadItem : clsAbstractPhysicPadItem
    {


        public readonly List<clsKraft> Kraft = new();



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
