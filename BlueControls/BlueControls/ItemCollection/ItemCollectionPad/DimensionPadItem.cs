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
        private string _text_oben = string.Empty;

        public int Nachkommastellen { get; set; } = 1;

        public string Text_oben
        {
            get
            {
                return _text_oben;
            }
            set
            {
                if (_text_oben == Länge_in_MM.ToString()) { value = string.Empty; }
                _text_oben = value;
                OnChanged(false);
            }
        }
        public string Text_unten { get; set; } = string.Empty;


        //http://www.kurztutorial.info/programme/punkt-mm/rechner.html
        // Dim Ausgleich As Double = mmToPixel(1 / 72 * 25.4, 300)
        public decimal Skalierung { get; set; } = 3.07m;


        public string Präfix { get; set; } = string.Empty;
        public string Suffix { get; set; } = string.Empty;



        #endregion


        #region  Event-Deklarationen + Delegaten 

        #endregion


        #region  Construktor + Initialize 

        public DimensionPadItem(ItemCollectionPad parent) : this(parent, string.Empty, null, null, 0) { }


        public DimensionPadItem(ItemCollectionPad parent, PointDF cPoint1, PointDF cPoint2, int AbstandinMM) : this(parent, string.Empty, cPoint1, cPoint2, AbstandinMM) { }




        public DimensionPadItem(ItemCollectionPad parent, string internalname, PointDF cPoint1, PointDF cPoint2, int AbstandinMM) : base(parent, internalname)
        {


            Point1.SetTo(cPoint1.X, cPoint1.Y);
            Point2.SetTo(cPoint2.X, cPoint2.Y);

            ComputeData();

            var a = GeometryDF.PolarToCartesian(modConverter.mmToPixel(AbstandinMM, ItemCollectionPad.DPI), Convert.ToDouble(_Winkel - 90));

            TextPointx.SetTo(Point1, _Länge / 2, _Winkel);
            TextPointx.X += a.X;
            TextPointx.Y += a.Y;

            if (string.IsNullOrEmpty(Internal)) { Develop.DebugPrint(enFehlerArt.Fehler, "Interner Name nicht vergeben."); }

            Text_oben = string.Empty;
            Text_unten = string.Empty;
            Nachkommastellen = 1;

            Stil = PadStyles.Style_StandardAlternativ;

            Point1.Parent = this;
            Point2.Parent = this;
            TextPointx.Parent = this;
            _SchnittPunkt1.Parent = this;
            _SchnittPunkt2.Parent = this;
            _Bezugslinie1.Parent = this;
            _Bezugslinie2.Parent = this;


            Points.Add(Point1);
            Points.Add(Point2);
            Points.Add(TextPointx);


        }


        public DimensionPadItem(ItemCollectionPad parent, PointF cPoint1, PointF cPoint2, int AbstandInMM) : this(parent, new PointDF(cPoint1), new PointDF(cPoint2), AbstandInMM) { }


        #endregion


        #region  Properties 


        #endregion


        public override void DesignOrStyleChanged()
        {
            // Keine Variablen zum Reseten, ein Invalidate reicht
        }


        public override bool ParseThis(string tag, string value)
        {
            if (base.ParseThis(tag, value)) { return true; }

            switch (tag)
            {
                case "text": // TODO: Alt 06.09.2019
                case "text1":
                    Text_oben = value.FromNonCritical();
                    return true;

                case "text2":
                    Text_unten = value.FromNonCritical();
                    return true;

                case "color": // TODO: Alt 06.09.2019
                    return true;

                case "fontsize": // TODO: Alt 06.09.2019
                    return true;

                case "accuracy": // TODO: Alt 06.09.2019
                    return true;

                case "decimal":
                    Nachkommastellen = int.Parse(value);
                    return true;

                case "checked": // TODO: Alt 06.09.2019
                    return true;

                case "prefix": // TODO: Alt 06.09.2019
                    Präfix = value.FromNonCritical();
                    return true;

                case "suffix":
                    Suffix = value.FromNonCritical();
                    return true;

                case "additionalscale":
                    Skalierung = decimal.Parse(value.FromNonCritical());
                    return true;
            }

            return false;
        }

        protected override void ParseFinished() { }


        protected override void GenerateInternalRelationExplicit() { }

        public override string ToString()
        {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";

            return t +
                   ", Text1=" + Text_oben.ToNonCritical() +
                   ", Text2=" + Text_unten.ToNonCritical() +
                   ", Decimal=" + Nachkommastellen +
                   ", Prefix=" + Präfix.ToNonCritical() +
                   ", Suffix=" + Suffix.ToNonCritical() +
                   ", AdditionalScale=" + Skalierung.ToString().ToNonCritical() + "}";
        }


        public string AngezeigterText1()
        {
            if (!string.IsNullOrEmpty(Text_oben)) { return Text_oben; }
            var s = Länge_in_MM.ToString(Constants.Format_Float10);
            s = s.Replace(".", ",");

            if (s.Contains(","))
            {
                s = s.TrimEnd("0");
                s = s.TrimEnd(",");
            }
            return Präfix + s + Suffix;
        }

        public decimal Länge_in_MM
        {
            get
            {
                return Math.Round(modConverter.PixelToMM(_Länge, ItemCollectionPad.DPI), Nachkommastellen);
            }
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

        protected override void DrawExplicit(Graphics GR, RectangleF DCoordinates, decimal cZoom, decimal MoveX, decimal MoveY, enStates vState, Size SizeOfParentControl, bool ForPrinting)
        {

            if (Stil == PadStyles.Undefiniert) { return; }




            var geszoom = Parent.SheetStyleScale * Skalierung * cZoom;


            var f = Skin.GetBlueFont(Stil, Parent.SheetStyle);

            var PfeilG = (decimal)f.Font(geszoom).Size * 0.8m;
            var pen2 = f.Pen(cZoom);

            GR.DrawLine(pen2, Point1.ZoomAndMove(cZoom, MoveX, MoveY), _Bezugslinie1.ZoomAndMove(cZoom, MoveX, MoveY)); // Bezugslinie 1
            GR.DrawLine(pen2, Point2.ZoomAndMove(cZoom, MoveX, MoveY), _Bezugslinie2.ZoomAndMove(cZoom, MoveX, MoveY)); // Bezugslinie 2
            GR.DrawLine(pen2, _SchnittPunkt1.ZoomAndMove(cZoom, MoveX, MoveY), _SchnittPunkt2.ZoomAndMove(cZoom, MoveX, MoveY)); // Maßhilfslinie
            GR.DrawLine(pen2, _SchnittPunkt1.ZoomAndMove(cZoom, MoveX, MoveY), TextPointx.ZoomAndMove(cZoom, MoveX, MoveY)); // Maßhilfslinie


            var sz1 = GR.MeasureString(AngezeigterText1(), f.Font(geszoom));
            var sz2 = GR.MeasureString(Text_unten, f.Font(geszoom));
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
            GR.DrawString(Text_unten, f.Font(geszoom), f.Brush_Color_Main, new PointF((float)(-sz2.Width / 2.0), (float)(-sz2.Height / 2.0)));
            GR.Restore(x);
        }



        public override RectangleDF UsedArea()
        {
            if (Stil == PadStyles.Undefiniert) { return new RectangleDF(0, 0, 0, 0); }
            var geszoom = Parent.SheetStyleScale * Skalierung;

            var f = Skin.GetBlueFont(Stil, Parent.SheetStyle);

            var sz1 = BlueFont.MeasureString(AngezeigterText1(), f.Font(geszoom));
            var sz2 = BlueFont.MeasureString(Text_unten, f.Font(geszoom));

            var maxrad = (decimal)(Math.Max(Math.Max(sz1.Width, sz1.Height), Math.Max(sz2.Width, sz2.Height)) / 2 + 10);


            var X = new RectangleDF(Point1, Point2);
            X.ExpandTo(_Bezugslinie1);
            X.ExpandTo(_Bezugslinie2);

            X.ExpandTo(TextPointx, maxrad);





            //return new RectangleDF(P1_x - 2, P1_y - 2, P2_x - P1_x + 4, P2_y - P1_y + 4); 

            X.Inflate(-2, -2); // die Sicherheits koordinaten damit nicht linien abgeschnitten werden

            return X;

        }




        private void ComputeData()
        {
            _Länge = GeometryDF.Länge(Point1, Point2);
            _Winkel = GeometryDF.Winkel(Point1, Point2);
        }

        public override void CaluclatePointsWORelations()
        {
            //Gegeben sind:
            // Point1, Point2 und Textpoint

            var MaßL = 0M;
            var tmppW = -90;
            var MHLAb = modConverter.mmToPixel(1.5M * Skalierung / 3.07m, ItemCollectionPad.DPI); // Den Abstand der Maßhilsfline, in echten MM
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

            base.CaluclatePointsWORelations();
        }



        public override List<FlexiControl> GetStyleOptions()
        {
            var l = new List<FlexiControl>
            {
                new FlexiControlForProperty(this, "Länge_in_MM"),

                new FlexiControlForProperty(this, "Text oben"),
                new FlexiControlForProperty(this, "Suffix"),
                new FlexiControlForProperty(this, "Text unten"),
                new FlexiControlForProperty(this, "Präfix")
            };


            AddStyleOption(l);


            l.Add(new FlexiControlForProperty(this, "Skalierung"));


            l.AddRange(base.GetStyleOptions());
            return l;
        }

        //public override void DoStyleCommands(object sender, List<string> Tags, ref bool CloseMenu)
        //{
        //    Text_oben = Tags.TagGet("Text (oben)").FromNonCritical();
        //    if (Text_oben == Länge_in_MM.ToString()) { Text_oben = ""; }
        //    Text_unten = Tags.TagGet("Text (unten)").FromNonCritical();

        //    Suffix = Tags.TagGet("Suffix").FromNonCritical();
        //    Präfix = Tags.TagGet("Präfix").FromNonCritical();
        //    Skalierung = decimal.Parse(Tags.TagGet("Skalierung").FromNonCritical());

        //    Stil = (PadStyles)int.Parse(Tags.TagGet("Stil"));
        //}



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