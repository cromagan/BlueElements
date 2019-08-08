﻿using BlueBasics.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueControls.ItemCollection
{
    public abstract class BasicPadItemTwoPoints : BasicPadItem
    {
        protected PointDF p_ML;
        protected PointDF p_MR;
        protected decimal winkel;
        private decimal _laengePix;
        private decimal _breitePix;

        public PointDF p_OL { get; private set; }
        public PointDF p_OR { get; private set; }
        public PointDF p_UL { get; private set; }
        public PointDF p_UR { get; private set; }


        public decimal laengePix
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


        public decimal breitePix
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

        protected override void Initialize()
        {
            base.Initialize();
            p_ML = new PointDF(this, "ML", 0, 0);
            p_MR = new PointDF(this, "MR", 1000, 0);
        }


        public BasicPadItemTwoPoints(ItemCollectionPad vparent)
        {
            Parent = vparent;
            p_ML = new PointDF(this, "L", 0, 0);
            p_MR = new PointDF(this, "R", (int)laengePix, 0);
            //  this.KeepInternalLogic();

        }

        protected override void KeepInternalLogic()
        {

            if (p_OL == null)
            {
                p_OL = new PointDF();
                p_OR = new PointDF();
                p_UL = new PointDF();
                p_UR = new PointDF();
            }

            p_UL.SetTo(p_ML, breitePix / 2, winkel - 90);
            p_OL.SetTo(p_ML, breitePix / 2, winkel + 90);
            p_UR.SetTo(p_MR, breitePix / 2, winkel - 90);
            p_OR.SetTo(p_MR, breitePix / 2, winkel + 90);

            winkel = GeometryDF.Winkelx(p_ML, p_MR);
        }

        public override void GenerateInternalRelation(List<clsPointRelation> relations)
        {
            relations.Add(new clsPointRelation(enRelationType.AbstandZueinander, p_ML, p_MR));
        }

        public override List<PointDF> PointList()
        {
            var l = new List<PointDF>();
            l.Add(p_ML);
            l.Add(p_MR);

            return l;
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

        public override void SetCoordinates(RectangleDF r)
        {
            //TODO: Koordinaten setzen
            //p_m.SetTo(r.PointOf(enAlignment.Horizontal_Vertical_Center));
            RecomputePointAndRelations();
        }

        public override RectangleDF UsedArea()
        {
            var minX = Math.Min(Math.Min(p_OL.X, p_UL.X), Math.Min(p_OR.X, p_UR.X));
            var maxX = Math.Max(Math.Max(p_OL.X, p_UL.X), Math.Max(p_OR.X, p_UR.X));
            var minY = Math.Min(Math.Min(p_OL.Y, p_UL.Y), Math.Min(p_OR.Y, p_UR.Y));
            var maxY = Math.Max(Math.Max(p_OL.Y, p_UL.Y), Math.Max(p_OR.Y, p_UR.Y));

            return new RectangleDF(minX, minY, maxX - minX, maxY - minY);
            //return new RectangleDF(p_m.X - trLaenge/2, p_m.Y - trBreite/2, trLaenge, trBreite);
        }


    }
}