﻿using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.ItemCollection.ItemCollectionList;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.ItemCollection.ItemCollectionPad
{
    public class SpacerPadItem : BasicPadItem
        {


            #region  Variablen-Deklarationen 

            internal PointDF p_o;
            internal PointDF p_u;
            internal PointDF p_l;
            internal PointDF p_r;
            internal PointDF p_m;


            private decimal mm125x; //Math.Round(mmToPixel(1.25D, _DPIx), 1)

            private decimal _Size;

            #endregion


            #region  Event-Deklarationen + Delegaten 

            #endregion


            #region  Construktor + Initialize 




            public SpacerPadItem(ItemCollectionPad vparent)
            {
                Parent = vparent;
            }


            protected override void InitializeLevel2()
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

            public override bool Contains(PointF P, decimal Zoomf)
            {
                var mp = UsedArea().PointOf(enAlignment.Horizontal_Vertical_Center);
                return GeometryDF.Länge(P.ToPointDF(), mp) < _Size / 2;
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


            protected override bool ParseLevel2(KeyValuePair<string, string> pair)
            {
                switch (pair.Key)
                {
                    case "size":
                        _Size = decimal.Parse(pair.Value);
                        return true;
                    case "checked":
                        return true;
                }

                return false;
            }





            protected override string ToStringLevel2()
            {
                return "Size=" + _Size;
            }


            internal override void KeepInternalLogic()
            {
                p_o.SetTo(p_m.X, p_m.Y - _Size * Parent.SheetStyleScale / 2);
                p_u.SetTo(p_m.X, p_m.Y + _Size * Parent.SheetStyleScale / 2);
                p_l.SetTo(p_m.X - _Size * Parent.SheetStyleScale / 2, p_m.Y);
                p_r.SetTo(p_m.X + _Size * Parent.SheetStyleScale / 2, p_m.Y);
            }


            public override void GenerateInternalRelation(List<clsPointRelation> Relations)
            {
                Relations.Add(new clsPointRelation(enRelationType.PositionZueinander, p_m, p_u));
                Relations.Add(new clsPointRelation(enRelationType.PositionZueinander, p_m, p_o));
                Relations.Add(new clsPointRelation(enRelationType.PositionZueinander, p_m, p_r));
                Relations.Add(new clsPointRelation(enRelationType.PositionZueinander, p_m, p_l));
            }

            public override List<FlexiControl> GetStyleOptions(object sender, System.EventArgs e)
            {
                var l = new List<FlexiControl>();

                var Size = new ItemCollectionList.ItemCollectionList();
                Size.Add(new TextListItem((mm125x * 1m).Nummer(1, 4), "Klein (1,25 mm)", enImageCode.GrößeÄndern));
                Size.Add(new TextListItem((mm125x * 2m).Nummer(1, 4), "Normal (2,5 mm)", enImageCode.GrößeÄndern));
                Size.Add(new TextListItem((mm125x * 4m).Nummer(1, 4), "Groß (5,0 mm)", enImageCode.GrößeÄndern));
                Size.Add(new TextListItem((mm125x * 5m).Nummer(1, 4), "Sehr groß (10,0 mm)", enImageCode.GrößeÄndern));

                l.Add(new FlexiControl("Größe Distanzhalter", _Size.Nummer(1, 4), Size));


                return l;
            }

            public override void DoStyleCommands(object sender, List<string> Tags, ref bool CloseMenu)
            {

                _Size = decimal.Parse(Tags.TagGet("Größe Distanzhalter").FromNonCritical());
                SetCoordinates(new RectangleDF(p_m.X - 5, p_m.Y - 5, 10, 10));

            }
        }
    }