// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using System;
using System.Collections.Generic;
using System.Drawing;
using static BlueBasics.Geometry;
using static BlueBasics.Polygons;

namespace BlueControls.ItemCollection {

    public class DimensionPadItem : BasicPadItem {

        #region Fields

        private readonly PointM _Bezugslinie1 = new(null, "Bezugslinie 1, Ende der Hilfslinie", 0, 0);
        private readonly PointM _Bezugslinie2 = new(null, "Bezugslinie 2, Ende der Hilfslinien", 0, 0);

        /// <summary>
        /// Dieser Punkt ist sichtbar und kann verschoben werden.
        /// </summary>
        private readonly PointM _Point1 = new(null, "Punkt 1", 0, 0);

        /// <summary>
        /// Dieser Punkt ist sichtbar und kann verschoben werden.
        /// </summary>
        private readonly PointM _Point2 = new(null, "Punkt 2", 0, 0);

        private readonly PointM _SchnittPunkt1 = new(null, "Schnittpunkt 1, Zeigt der Pfeil hin", 0, 0);
        private readonly PointM _SchnittPunkt2 = new(null, "Schnittpunkt 2, Zeigt der Pfeil hin", 0, 0);

        /// <summary>
        /// Dieser Punkt ist sichtbar und kann verschoben werden.
        /// </summary>
        private readonly PointM _TextPoint = new(null, "Mitte Text", 0, 0);

        private float _Länge;
        private string _text_oben = string.Empty;
        private float _Winkel;

        #endregion

        #region Constructors

        public DimensionPadItem(ItemCollectionPad parent) : this(parent, string.Empty, null, null, 0) { }

        public DimensionPadItem(ItemCollectionPad parent, PointM point1, PointM point2, int abstandinMM) : this(parent, string.Empty, point1, point2, abstandinMM) { }

        public DimensionPadItem(ItemCollectionPad parent, string internalname, PointM point1, PointM point2, int abstandinMM) : base(parent, internalname) {
            if (string.IsNullOrEmpty(Internal)) { Develop.DebugPrint(enFehlerArt.Fehler, "Interner Name nicht vergeben."); }

            if (point1 != null) { _Point1.SetTo(point1.X, point1.Y); }
            if (point2 != null) { _Point2.SetTo(point2.X, point2.Y); }
            ComputeData();

            var a = PolarToCartesian(Converter.mmToPixel(abstandinMM, ItemCollectionPad.DPI), _Winkel - 90f);
            _TextPoint.SetTo(_Point1, _Länge / 2, _Winkel);
            _TextPoint.X += a.X;
            _TextPoint.Y += a.Y;

            Text_oben = string.Empty;
            Text_unten = string.Empty;
            Nachkommastellen = 1;

            Stil = PadStyles.Style_StandardAlternativ;
            _Point1.Parent = this;
            _Point2.Parent = this;
            _TextPoint.Parent = this;
            _SchnittPunkt1.Parent = this;
            _SchnittPunkt2.Parent = this;
            _Bezugslinie1.Parent = this;
            _Bezugslinie2.Parent = this;

            MovablePoint.Add(_Point1);
            MovablePoint.Add(_Point2);
            MovablePoint.Add(_TextPoint);
            PointsForSuccesfullyMove.AddRange(MovablePoint);

            CalculateOtherPoints();
        }

        public DimensionPadItem(ItemCollectionPad parent, PointF point1, PointF point2, int abstandInMM) : this(parent, new PointM(point1), new PointM(point2), abstandInMM) { }

        #endregion

        #region Properties

        public float Länge_in_MM => (float)Math.Round(Converter.PixelToMM(_Länge, ItemCollectionPad.DPI), Nachkommastellen);
        public int Nachkommastellen { get; set; } = 1;

        public string Präfix { get; set; } = string.Empty;

        //http://www.kurztutorial.info/programme/punkt-mm/rechner.html
        // Dim Ausgleich As float = mmToPixel(1 / 72 * 25.4, 300)
        public float Skalierung { get; set; } = 3.07f;

        public string Suffix { get; set; } = string.Empty;

        public string Text_oben {
            get => _text_oben;
            set {
                if (_text_oben == Länge_in_MM.ToString()) { value = string.Empty; }
                _text_oben = value;
                OnChanged();
            }
        }

        public string Text_unten { get; set; } = string.Empty;

        #endregion

        #region Methods

        public static void DrawArrow(Graphics gr, PointF point, float winkel, Color col, float fontSize) {
            var m1 = fontSize * 1.5f;
            var Px2 = Geometry.PolarToCartesian(m1, winkel + 10);
            var Px3 = Geometry.PolarToCartesian(m1, winkel - 10);
            var pa = Poly_Triangle(point, new PointF(point.X + (float)Px2.X, point.Y + (float)Px2.Y), new PointF(point.X + (float)Px3.X, point.Y + (float)Px3.Y));
            gr.FillPath(new SolidBrush(col), pa);
        }

        public string Angezeigter_Text_Oben() {
            if (!string.IsNullOrEmpty(Text_oben)) { return Text_oben; }
            var s = Länge_in_MM.ToString(Constants.Format_Float10);
            s = s.Replace(".", ",");
            if (s.Contains(",")) {
                s = s.TrimEnd("0");
                s = s.TrimEnd(",");
            }
            return Präfix + s + Suffix;
        }

        public override bool Contains(PointF value, float zoomfactor) {
            var ne = 5 / zoomfactor;
            return value.DistanzZuStrecke(_Point1, _Bezugslinie1) < ne
                    || value.DistanzZuStrecke(_Point2, _Bezugslinie2) < ne
                    || value.DistanzZuStrecke(_SchnittPunkt1, _SchnittPunkt2) < ne
                    || value.DistanzZuStrecke(_SchnittPunkt1, _TextPoint) < ne
                    || Länge(new PointM(value), _TextPoint) < ne * 10;
        }

        public override List<FlexiControl> GetStyleOptions() {
            List<FlexiControl> l = new()
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

        public override bool ParseThis(string tag, string value) {
            if (base.ParseThis(tag, value)) { return true; }
            switch (tag) {
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
                    Skalierung = float.Parse(value.FromNonCritical());
                    return true;
            }
            return false;
        }

        public override void PointMoved(object sender, MoveEventArgs e) => CalculateOtherPoints();

        public override string ToString() {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1);
            return t +
                   ", Text1=" + Text_oben.ToNonCritical() +
                   ", Text2=" + Text_unten.ToNonCritical() +
                   ", Decimal=" + Nachkommastellen +
                   ", Prefix=" + Präfix.ToNonCritical() +
                   ", Suffix=" + Suffix.ToNonCritical() +
                   ", AdditionalScale=" + Skalierung.ToString().ToNonCritical() + "}";
        }

        protected override RectangleF CalculateUsedArea() {
            if (Stil == PadStyles.Undefiniert) { return new RectangleF(0, 0, 0, 0); }
            var geszoom = Parent.SheetStyleScale * Skalierung;
            var f = Skin.GetBlueFont(Stil, Parent.SheetStyle);
            var sz1 = BlueFont.MeasureString(Angezeigter_Text_Oben(), f.Font(geszoom));
            var sz2 = BlueFont.MeasureString(Text_unten, f.Font(geszoom));
            var maxrad = (float)Math.Max(Math.Max(sz1.Width, sz1.Height), Math.Max(sz2.Width, sz2.Height));
            RectangleF X = new(_Point1, new SizeF(0, 0));
            X = X.ExpandTo(_Point1);
            X = X.ExpandTo(_Bezugslinie1);
            X = X.ExpandTo(_Bezugslinie2);
            X = X.ExpandTo(_TextPoint, maxrad);

            X.Inflate(-2, -2); // die Sicherheits koordinaten damit nicht linien abgeschnitten werden
            return X;
        }

        protected override string ClassId() => "DIMENSION";

        protected override void DrawExplicit(Graphics gr, RectangleF drawingCoordinates, float zoom, float shiftX, float shiftY, enStates state, Size sizeOfParentControl, bool forPrinting) {
            if (Stil == PadStyles.Undefiniert) { return; }
            var geszoom = Parent.SheetStyleScale * Skalierung * zoom;
            var f = Skin.GetBlueFont(Stil, Parent.SheetStyle);
            var PfeilG = f.Font(geszoom).Size * 0.8f;
            var pen2 = f.Pen(zoom);

            //DrawOutline(gr, zoom, shiftX, shiftY, Color.Red);
            //gr.DrawLine(pen2, UsedArea().PointOf(enAlignment.Top_Left).ZoomAndMove(zoom, shiftX, shiftY), UsedArea().PointOf(enAlignment.Bottom_Right).ZoomAndMove(zoom, shiftX, shiftY)); // Bezugslinie 1
            //gr.DrawLine(pen2, UsedArea().PointOf(enAlignment.Top_Left).ZoomAndMove(zoom, shiftX, shiftY), UsedArea().PointOf(enAlignment.Bottom_Left).ZoomAndMove(zoom, shiftX, shiftY)); // Bezugslinie 1

            gr.DrawLine(pen2, _Point1.ZoomAndMove(zoom, shiftX, shiftY), _Bezugslinie1.ZoomAndMove(zoom, shiftX, shiftY)); // Bezugslinie 1
            gr.DrawLine(pen2, _Point2.ZoomAndMove(zoom, shiftX, shiftY), _Bezugslinie2.ZoomAndMove(zoom, shiftX, shiftY)); // Bezugslinie 2
            gr.DrawLine(pen2, _SchnittPunkt1.ZoomAndMove(zoom, shiftX, shiftY), _SchnittPunkt2.ZoomAndMove(zoom, shiftX, shiftY)); // Maßhilfslinie
            gr.DrawLine(pen2, _SchnittPunkt1.ZoomAndMove(zoom, shiftX, shiftY), _TextPoint.ZoomAndMove(zoom, shiftX, shiftY)); // Maßhilfslinie
            var sz1 = gr.MeasureString(Angezeigter_Text_Oben(), f.Font(geszoom));
            var sz2 = gr.MeasureString(Text_unten, f.Font(geszoom));
            var P1 = _SchnittPunkt1.ZoomAndMove(zoom, shiftX, shiftY);
            var P2 = _SchnittPunkt2.ZoomAndMove(zoom, shiftX, shiftY);
            if (sz1.Width + (PfeilG * 2f) < Geometry.GetLenght(P1, P2)) {
                DrawArrow(gr, P1, _Winkel, f.Color_Main, PfeilG);
                DrawArrow(gr, P2, _Winkel + 180, f.Color_Main, PfeilG);
            } else {
                DrawArrow(gr, P1, _Winkel + 180, f.Color_Main, PfeilG);
                DrawArrow(gr, P2, _Winkel, f.Color_Main, PfeilG);
            }
            var Mitte = _TextPoint.ZoomAndMove(zoom, shiftX, shiftY);
            var TextWinkel = _Winkel % 360;
            if (TextWinkel is > 90 and <= 270) { TextWinkel = _Winkel - 180; }
            if (geszoom < 0.15d) { return; } // Schrift zu klein, würde abstürzen
            PointM Mitte1 = new(Mitte, (float)(sz1.Height / 2.1), TextWinkel + 90);
            var x = gr.Save();
            gr.TranslateTransform((float)Mitte1.X, (float)Mitte1.Y);
            gr.RotateTransform(-TextWinkel);
            gr.FillRectangle(new SolidBrush(Color.White), new RectangleF((int)(-sz1.Width * 0.9 / 2), (int)(-sz1.Height * 0.8 / 2), (int)(sz1.Width * 0.9), (int)(sz1.Height * 0.8)));
            f.DrawString(gr, Angezeigter_Text_Oben(), (float)(-sz1.Width / 2.0), (float)(-sz1.Height / 2.0), (float)geszoom, StringFormat.GenericDefault);
            gr.Restore(x);
            PointM Mitte2 = new(Mitte, (float)(sz2.Height / 2.1), TextWinkel - 90);
            x = gr.Save();
            gr.TranslateTransform((float)Mitte2.X, (float)Mitte2.Y);
            gr.RotateTransform(-TextWinkel);
            gr.FillRectangle(new SolidBrush(Color.White), new RectangleF((int)(-sz2.Width * 0.9 / 2), (int)(-sz2.Height * 0.8 / 2), (int)(sz2.Width * 0.9), (int)(sz2.Height * 0.8)));
            f.DrawString(gr, Text_unten, (float)(-sz2.Width / 2.0), (float)(-sz2.Height / 2.0), (float)geszoom, StringFormat.GenericDefault);
            gr.Restore(x);
        }

        protected override void ParseFinished() => CalculateOtherPoints();

        private void CalculateOtherPoints() {
            var tmppW = -90;
            var MHLAb = Converter.mmToPixel(1.5f * Skalierung / 3.07f, ItemCollectionPad.DPI); // Den Abstand der Maßhilsfline, in echten MM
            ComputeData();

            //Gegeben sind:
            // Point1, Point2 und Textpoint
            var MaßL = _TextPoint.DistanzZuLinie(_Point1, _Point2);
            _SchnittPunkt1.SetTo(_Point1, MaßL, _Winkel - 90);
            _SchnittPunkt2.SetTo(_Point2, MaßL, _Winkel - 90);
            if (_TextPoint.DistanzZuLinie(_SchnittPunkt1, _SchnittPunkt2) > 0.5d) {
                _SchnittPunkt1.SetTo(_Point1, MaßL, _Winkel + 90);
                _SchnittPunkt2.SetTo(_Point2, MaßL, _Winkel + 90);
                tmppW = 90;
            }
            _Bezugslinie1.SetTo(_SchnittPunkt1, MHLAb, _Winkel + tmppW);
            _Bezugslinie2.SetTo(_SchnittPunkt2, MHLAb, _Winkel + tmppW);
        }

        private void ComputeData() {
            _Länge = Länge(_Point1, _Point2);
            _Winkel = Winkel(_Point1, _Point2);
        }

        #endregion
    }
}