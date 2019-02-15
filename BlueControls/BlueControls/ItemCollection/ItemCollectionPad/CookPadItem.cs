using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using static BlueBasics.Extensions;

namespace BlueControls.ItemCollection.ItemCollectionPad
{
    public class CookPadItem : BasicPadItem
        {
           



            #region  Variablen-Deklarationen 

            public string Anzeige;

            public double Menge;


            public CookPadItem Container;

            public List<CookPadItem> Childs;

            public PointDF Middlex;

            #endregion


            #region  Event-Deklarationen + Delegaten 

            #endregion


            #region  Construktor + Initialize 




            protected override void InitializeLevel2()
            {
                Childs = new List<CookPadItem>();
                Middlex = new PointDF(this, "Middle", 0, 0);
                Anzeige = string.Empty;
                Menge = 0;
                Container = null;
                Childs.Clear();
            }


            #endregion






            protected override void DrawExplicit(Graphics GR, Rectangle DCoordinates, decimal cZoom, decimal MoveX, decimal MoveY, enStates vState, Size SizeOfParentControl, bool ForPrinting)
            {


                GR.FillEllipse(Brushes.White, DCoordinates);


                GR.DrawEllipse(CreativePad.PenGray, DCoordinates);



                var f = new Font("Arial", (float)(5 * cZoom));
                var l = GR.MeasureString(Anzeige, f);

                var p = DCoordinates.PointOf(enAlignment.Bottom_HorizontalCenter);


                GR.FillRectangle(new SolidBrush(Color.FromArgb(128, 255, 255, 255)), new Rectangle(Convert.ToInt32(p.X - l.Width / 2.0), (int)(p.Y - l.Height * 2), (int)l.Width, (int)l.Height));

                GR.DrawString(Anzeige, f, Brushes.Black, new PointF((float)(p.X - l.Width / 2.0), p.Y - l.Height * 2));


            }

            public override void GenerateInternalRelation(List<clsPointRelation> Relations)
            {

            }

            public override void SetCoordinates(RectangleDF r)
            {
                Middlex.SetTo(r.PointOf(enAlignment.Horizontal_Vertical_Center));
            }

            public override void DesignOrStyleChanged()
            {

            }



            internal override void KeepInternalLogic()
            {

            }

            protected override string ClassId()
            {
                return "COOK";
            }

            public override bool Contains(PointF P, decimal Zoomf)
            {
                return UsedArea().Contains(Convert.ToInt32(P.X), Convert.ToInt32(P.Y));
            }


            public override List<PointDF> PointList()
            {
                var l = new List<PointDF>();
                l.Add(Middlex);
                return l;
            }

            public override RectangleDF UsedArea()
            {
                return new RectangleDF(Middlex.X - 30, Middlex.Y - 30, 60, 60);
            }

            protected override bool ParseLevel2(KeyValuePair<string, string> Pair)
            {
                return false;
            }

            protected override string ToStringLevel2()
            {
                return string.Empty;
            }

            public override List<FlexiControl> GetStyleOptions(object sender, System.EventArgs e)
            {
                var l = new List<FlexiControl>();
                return l;
            }

            public override void DoStyleCommands(object sender, List<string> Tags, ref bool CloseMenu)
            {
            }
        }
    }