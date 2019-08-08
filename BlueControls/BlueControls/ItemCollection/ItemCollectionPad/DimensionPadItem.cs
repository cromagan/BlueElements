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
    public class DimensionPadItem : BasicPadItem
    {
        #region  Variablen-Deklarationen 

        private readonly PointDF Point1 = new PointDF(null, "Punkt 1", 0, 0, false, false);
        private readonly PointDF Point2 = new PointDF(null, "Punkt 2", 0, 0, false, false);
        private readonly PointDF TextPointx = new PointDF(null, "Mitte Text", 0, 0, false, false);

        private readonly PointDF _SchnittPunkt1 = new PointDF(null, "Schnittpunkt 1, Zeigt der Pfeil hin", 0, 0);
        private readonly PointDF _SchnittPunkt2 = new PointDF(null, "Schnittpunkt 2, Zeigt der Pfeil hin", 0, 0);

        private readonly PointDF _Bezugslinie1 = new PointDF(null, "Bezugslinie 1, Ende der Hilfslinie", 0, 0);
        private readonly PointDF _Bezugslinie2 = new PointDF(null, "Bezugslinie 2, Ende der Hilfslinien", 0, 0);


        private decimal _Winkel;
        private decimal _Länge;

        public int NachKomma;

        public string Text1;
        public string Text2;


        //http://www.kurztutorial.info/programme/punkt-mm/rechner.html
        // Dim Ausgleich As Double = mmToPixel(1 / 72 * 25.4, 300)
        public decimal ZusatzScale = 3.07m;

        public string Prefix = "";
        public string Suffix = "";



        #endregion


        #region  Event-Deklarationen + Delegaten 

        #endregion


        #region  Construktor + Initialize 

        public DimensionPadItem()
        { }

        public DimensionPadItem(string cInternal, PointDF cPoint1, PointDF cPoint2, int AbstandinMM, int InitialDPI)
        {
            _Internal = cInternal;
            Point1.SetTo(cPoint1.X, cPoint1.Y);
            Point2.SetTo(cPoint2.X, cPoint2.Y);

            ComputeData();

            var a = GeometryDF.PolarToCartesian(modConverter.mmToPixel(AbstandinMM, InitialDPI), Convert.ToDouble(_Winkel - 90));

            TextPointx.SetTo(Point1, _Länge / 2, _Winkel);
            TextPointx.X += a.X;
            TextPointx.Y += a.Y;

            if (string.IsNullOrEmpty(_Internal)) { Develop.DebugPrint(enFehlerArt.Fehler, "Interner Name nicht vergeben."); }
        }


        public DimensionPadItem(PointF cPoint1, PointF cPoint2, int AbstandInMM, int InitialDPI)
        {
            Point1.SetTo(cPoint1.X, cPoint1.Y);
            Point2.SetTo(cPoint2.X, cPoint2.Y);

            ComputeData();


            var a = GeometryDF.PolarToCartesian(modConverter.mmToPixel(AbstandInMM, InitialDPI), Convert.ToDouble(_Winkel - 90));

            TextPointx.SetTo(Point1, _Länge / 2, _Winkel);
            TextPointx.X += a.X;
            TextPointx.Y += a.Y;


            if (string.IsNullOrEmpty(_Internal))
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "Interner Name nicht vergeben.");
            }
        }





        protected override void Initialize()
        {
            base.Initialize();
            Text1 = "";
            Text2 = "";
            NachKomma = 1;
            //  ImmerWaagerecht = True
            Format = PadStyles.Style_StandardAlternativ;

            Point1.Parent = this;
            Point2.Parent = this;
            TextPointx.Parent = this;
            _SchnittPunkt1.Parent = this;
            _SchnittPunkt2.Parent = this;
            _Bezugslinie1.Parent = this;
            _Bezugslinie2.Parent = this;
            //                private readonly PointDF Point1 = new PointDF(this, "Punkt 1", 0, 0, false, false);
            //private readonly PointDF Point2 = new PointDF(this, "Punkt 2", 0, 0, false, false);
            //private readonly PointDF TextPointx = new PointDF(this, "Mitte Text", 0, 0, false, false);

            //private readonly PointDF _SchnittPunkt1 = new PointDF(this, "Schnittpunkt 1, Zeigt der Pfeil hin", 0, 0);
            //private readonly PointDF _SchnittPunkt2 = new PointDF(this, "Schnittpunkt 2, Zeigt der Pfeil hin", 0, 0);

            //private readonly PointDF _Bezugslinie1 = new PointDF(this, "Bezugslinie 1, Ende der Hilfslinie", 0, 0);
            //private readonly PointDF _Bezugslinie2 = new PointDF(this, "Bezugslinie 2, Ende der Hilfslinien", 0, 0);




        }


        //Sub New()

        //    Initialize()


        //End Sub


        #endregion


        #region  Properties 


        #endregion


        public override void DesignOrStyleChanged()
        {
            // Keine Variablen zum Reseten, ein Invalidate reicht
        }


        protected override bool ParseExplicit(KeyValuePair<string, string> pair)
        {
            switch (pair.Key)
            {
                case "text": // TODO: Alt
                case "text1":
                    Text1 = pair.Value.FromNonCritical();
                    return true;

                case "text2":
                    Text2 = pair.Value.FromNonCritical();
                    return true;

                case "color": // TODO: Alt
                    return true;

                case "fontsize": // TODO: Alt
                    return true;

                case "accuracy": // TODO: Alt
                    return true;

                case "decimal":
                    NachKomma = int.Parse(pair.Value);
                    return true;

                case "checked": // TODO: Alt
                    return true;

                case "prefix": // TODO: Alt
                    Prefix = pair.Value.FromNonCritical();
                    return true;

                case "suffix":
                    Suffix = pair.Value.FromNonCritical();
                    return true;

                case "additionalscale":
                    ZusatzScale = decimal.Parse(pair.Value.FromNonCritical());
                    return true;
            }

            return false;
        }


        public override string ToString()
        {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";

            return t +
                   ", Text1=" + Text1.ToNonCritical() +
                   ", Text2=" + Text2.ToNonCritical() +
                   ", Decimal=" + NachKomma +
                   ", Prefix=" + Prefix.ToNonCritical() +
                   ", Suffix=" + Suffix.ToNonCritical() +
                   ", AdditionalScale=" + ZusatzScale.ToString().ToNonCritical() + "}";
        }


        public string AngezeigterText1()
        {
            if (!string.IsNullOrEmpty(Text1)) { return Text1; }
            return Prefix + LängeInMM() + Suffix;
        }


        public decimal LängeInMM()
        {
            return Math.Round(modConverter.PixelToMM(_Länge, ItemCollectionPad.DPI), NachKomma);
        }




        protected override string ClassId()
        {
            return "DIMENSION";
        }

        public override bool Contains(PointF value, decimal zoomfactor)
        {
            var ne = 5 / zoomfactor;

            if (value.DistanzZuStrecke(Point1, _Bezugslinie1) < ne) { return true; }
            if (value.DistanzZuStrecke(Point2, _Bezugslinie2) < ne) { return true; }
            if (value.DistanzZuStrecke(_SchnittPunkt1, _SchnittPunkt2) < ne) { return true; }
            if (value.DistanzZuStrecke(_SchnittPunkt1, TextPointx) < ne) { return true; }
            if (GeometryDF.Länge(new PointDF(value), TextPointx) < ne * 10) { return true; }

            return false;
        }

        protected override void DrawExplicit(Graphics GR, Rectangle DCoordinates, decimal cZoom, decimal MoveX, decimal MoveY, enStates vState, Size SizeOfParentControl, bool ForPrinting)
        {

            if (Format == PadStyles.Undefiniert) { return; }
            var geszoom = cZoom * Parent.SheetStyleScale * ZusatzScale;
            var f = Skin.GetBlueFont(Format, Parent.SheetStyle);

            var PfeilG = (decimal)f.Font(geszoom).Size * 0.8m;
            var pen2 = f.Pen(cZoom);

            GR.DrawLine(pen2, Point1.ZoomAndMove(cZoom, MoveX, MoveY), _Bezugslinie1.ZoomAndMove(cZoom, MoveX, MoveY)); // Bezugslinie 1
            GR.DrawLine(pen2, Point2.ZoomAndMove(cZoom, MoveX, MoveY), _Bezugslinie2.ZoomAndMove(cZoom, MoveX, MoveY)); // Bezugslinie 2
            GR.DrawLine(pen2, _SchnittPunkt1.ZoomAndMove(cZoom, MoveX, MoveY), _SchnittPunkt2.ZoomAndMove(cZoom, MoveX, MoveY)); // Maßhilfslinie
            GR.DrawLine(pen2, _SchnittPunkt1.ZoomAndMove(cZoom, MoveX, MoveY), TextPointx.ZoomAndMove(cZoom, MoveX, MoveY)); // Maßhilfslinie


            var sz1 = GR.MeasureString(AngezeigterText1(), f.Font(geszoom));
            var sz2 = GR.MeasureString(Text2, f.Font(geszoom));
            var P1 = _SchnittPunkt1.ZoomAndMove(cZoom, MoveX, MoveY);
            var P2 = _SchnittPunkt2.ZoomAndMove(cZoom, MoveX, MoveY);





            if ((decimal)sz1.Width + PfeilG * 2m < Geometry.Länge(P1, P2))
            {
                DrawPfeil(GR, P1, Convert.ToDouble(_Winkel), f.Color_Main, PfeilG);
                DrawPfeil(GR, P2, Convert.ToDouble(_Winkel + 180), f.Color_Main, PfeilG);
            }
            else
            {
                DrawPfeil(GR, P1, Convert.ToDouble(_Winkel + 180), f.Color_Main, PfeilG);
                DrawPfeil(GR, P2, Convert.ToDouble(_Winkel), f.Color_Main, PfeilG);
            }



            var Mitte = TextPointx.ZoomAndMove(cZoom, MoveX, MoveY);


            var TextWinkel = (float)(_Winkel % 360);

            if (TextWinkel > 90 && TextWinkel <= 270) { TextWinkel = (float)(_Winkel - 180); }


            if (geszoom < 0.15m) { return; } // Schrift zu klein, würde abstürzen


            var Mitte1 = new PointDF(Mitte, (decimal)(sz1.Height / 2.1), (decimal)(TextWinkel + 90));
            var x = GR.Save();
            GR.TranslateTransform((float)Mitte1.X, (float)Mitte1.Y);
            GR.RotateTransform(-TextWinkel);
            GR.FillRectangle(new SolidBrush(Color.White), new RectangleF((int)(-sz1.Width * 0.9 / 2), (int)(-sz1.Height * 0.8 / 2), (int)(sz1.Width * 0.9), (int)(sz1.Height * 0.8)));
            GR.DrawString(AngezeigterText1(), f.Font(geszoom), f.Brush_Color_Main, new PointF((float)(-sz1.Width / 2.0), (float)(-sz1.Height / 2.0)));
            GR.Restore(x);

            var Mitte2 = new PointDF(Mitte, (decimal)(sz2.Height / 2.1), (decimal)(TextWinkel - 90));
            x = GR.Save();
            GR.TranslateTransform((float)Mitte2.X, (float)Mitte2.Y);
            GR.RotateTransform(-TextWinkel);
            GR.FillRectangle(new SolidBrush(Color.White), new RectangleF((int)(-sz2.Width * 0.9 / 2), (int)(-sz2.Height * 0.8 / 2), (int)(sz2.Width * 0.9), (int)(sz2.Height * 0.8)));
            GR.DrawString(Text2, f.Font(geszoom), f.Brush_Color_Main, new PointF((float)(-sz2.Width / 2.0), (float)(-sz2.Height / 2.0)));
            GR.Restore(x);
        }


        public override List<PointDF> PointList()
        {
            var l = new List<PointDF>();
            l.Add(Point1);
            l.Add(Point2);
            l.Add(TextPointx);
            return l;
        }


        public override RectangleDF UsedArea()
        {
            if (Format == PadStyles.Undefiniert) { return new RectangleDF(0, 0, 0, 0); }
            var geszoom = Parent.SheetStyleScale * ZusatzScale;

            var f = Skin.GetBlueFont(Format, Parent.SheetStyle);

            var sz1 = BlueFont.MeasureString(AngezeigterText1(), f.Font(geszoom));
            var sz2 = BlueFont.MeasureString(Text2, f.Font(geszoom));

            var P1_x = decimal.MaxValue;
            var P1_y = decimal.MaxValue;
            var P2_x = decimal.MinValue;
            var P2_y = decimal.MinValue;


            P1_x = Math.Min(P1_x, Point1.X);
            P1_x = Math.Min(P1_x, _Bezugslinie1.X);
            P1_x = Math.Min(P1_x, Point2.X);
            P1_x = Math.Min(P1_x, _Bezugslinie2.X);
            P1_x = Math.Min(P1_x, TextPointx.X - (decimal)sz1.Width / 2m - 10);
            P1_x = Math.Min(P1_x, TextPointx.X - (decimal)sz2.Width / 2m - 10);

            P1_y = Math.Min(P1_y, Point1.Y);
            P1_y = Math.Min(P1_y, _Bezugslinie1.Y);
            P1_y = Math.Min(P1_y, Point2.Y);
            P1_y = Math.Min(P1_y, _Bezugslinie2.Y);
            P1_y = Math.Min(P1_y, TextPointx.Y - (decimal)sz1.Height / 2m - 10);
            P1_y = Math.Min(P1_y, TextPointx.Y - (decimal)sz2.Height / 2m - 10);

            P2_x = Math.Max(P2_x, Point1.X);
            P2_x = Math.Max(P2_x, _Bezugslinie1.X);
            P2_x = Math.Max(P2_x, Point2.X);
            P2_x = Math.Max(P2_x, _Bezugslinie2.X);
            P2_x = Math.Max(P2_x, TextPointx.X + (decimal)sz1.Width / 2m + 10);
            P2_x = Math.Max(P2_x, TextPointx.X + (decimal)sz2.Width / 2m + 10);

            P2_y = Math.Max(P2_y, Point1.Y);
            P2_y = Math.Max(P2_y, _Bezugslinie1.Y);
            P2_y = Math.Max(P2_y, Point2.Y);
            P2_y = Math.Max(P2_y, _Bezugslinie2.Y);
            P2_y = Math.Max(P2_y, TextPointx.Y + (decimal)sz1.Height / 2m + 10);
            P2_y = Math.Max(P2_y, TextPointx.Y + (decimal)sz2.Height / 2m + 10);

            return new RectangleDF(P1_x - 2, P1_y - 2, P2_x - P1_x + 4, P2_y - P1_y + 4); // die Sicherheits koordinaten damit nicht linien abgeschnitten werden
        }


        public override void SetCoordinates(RectangleDF r)
        {
            // Ignorieren, kann man nicht verschieben
            RecomputePointAndRelations();
        }


        private void ComputeData()
        {
            _Länge = GeometryDF.Länge(Point1, Point2);
            _Winkel = GeometryDF.Winkelx(Point1, Point2);
        }

        protected override void KeepInternalLogic()
        {
            //Gegeben sind:
            // Point1, Point2 und Textpoint

            var MaßL = 0M;
            var tmppW = -90;
            var MHLAb = modConverter.mmToPixel(1.5M, ItemCollectionPad.DPI); // Den Abstand der Maßhilsfline, in echten MM
            ComputeData();

            MaßL = TextPointx.DistanzZuLinie(Point1, Point2);

            _SchnittPunkt1.SetTo(Point1, MaßL, _Winkel - 90);
            _SchnittPunkt2.SetTo(Point2, MaßL, _Winkel - 90);


            if (TextPointx.DistanzZuLinie(_SchnittPunkt1, _SchnittPunkt2) > 0.5M)
            {
                _SchnittPunkt1.SetTo(Point1, MaßL, _Winkel + 90);
                _SchnittPunkt2.SetTo(Point2, MaßL, _Winkel + 90);
                tmppW = 90;
            }

            _Bezugslinie1.SetTo(_SchnittPunkt1, MHLAb, _Winkel + tmppW);
            _Bezugslinie2.SetTo(_SchnittPunkt2, MHLAb, _Winkel + tmppW);
        }


        public override void GenerateInternalRelation(List<clsPointRelation> relations)
        {
            // Nix zu tun
        }

        public override List<FlexiControl> GetStyleOptions(object sender, System.EventArgs e)
        {
            var l = new List<FlexiControl>();


            l.Add(new FlexiControl("Tatsächliche Länge: " + LängeInMM() + " mm"));

            l.Add(new FlexiControl("Text (oben)", Text1, enDataFormat.Text, 1));
            l.Add(new FlexiControl("Präfix", Prefix, enDataFormat.Text, 1));
            l.Add(new FlexiControl("Suffix", Suffix, enDataFormat.Text, 1));
            l.Add(new FlexiControl("Text (unten)", Text2, enDataFormat.Text, 1));



            l.Add(new FlexiControl("Stil", ((int)Format).ToString(), Skin.GetFonts(Parent.SheetStyle)));

            l.Add(new FlexiControl("Skalierung", ZusatzScale.ToString(), enDataFormat.Gleitkommazahl, 1));
            return l;
        }

        public override void DoStyleCommands(object sender, List<string> Tags, ref bool CloseMenu)
        {
            Text1 = Tags.TagGet("Text (oben)").FromNonCritical();
            if (Text1 == LängeInMM().ToString()) { Text1 = ""; }
            Text2 = Tags.TagGet("Text (unten)").FromNonCritical();

            Suffix = Tags.TagGet("Suffix").FromNonCritical();
            Prefix = Tags.TagGet("Präfix").FromNonCritical();
            ZusatzScale = decimal.Parse(Tags.TagGet("Skalierung").FromNonCritical());

            Format = (PadStyles)int.Parse(Tags.TagGet("Stil"));
        }



        public static void DrawPfeil(Graphics GR, PointF Point, double Winkel, Color Col, decimal FontSize)
        {
            var m1 = FontSize * 1.5m;
            var Px2 = GeometryDF.PolarToCartesian(m1, Winkel + 10);
            var Px3 = GeometryDF.PolarToCartesian(m1, Winkel - 10);

            var pa = modAllgemein.Poly_Triangle(Point, new PointF(Point.X + (float)Px2.X, Point.Y + (float)Px2.Y), new PointF(Point.X + (float)Px3.X, Point.Y + (float)Px3.Y));
            GR.FillPath(new SolidBrush(Col), pa);

        }
    }
}