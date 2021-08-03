// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using static BlueBasics.Polygons;

namespace BlueControls.ItemCollection {

    public class SymbolPadItem : FormPadItemRectangle {

        #region Constructors

        public SymbolPadItem(ItemCollectionPad parent) : this(parent, string.Empty) {
        }

        public SymbolPadItem(ItemCollectionPad parent, string internalname) : base(parent, internalname) {
            Symbol = enSymbol.Pfeil;
            Hintergrundfarbe = Color.White;
            Randfarbe = Color.Black;
            Randdicke = 1;
        }

        #endregion

        #region Properties

        public Color Hintergrundfarbe { get; set; }

        public double Randdicke { get; set; }

        public Color Randfarbe { get; set; }

        public enSymbol Symbol { get; set; } = enSymbol.Pfeil;

        #endregion

        #region Methods

        public override List<FlexiControl> GetStyleOptions() {
            List<FlexiControl> l = new()
            {
                new FlexiControl()
            };
            ItemCollectionList Comms = new()
            {
                { "Ohne", ((int)enSymbol.Ohne).ToString(), QuickImage.Get("Datei|32") },
                { "Rechteck", ((int)enSymbol.Rechteck).ToString(), QuickImage.Get("Stop|32") },
                { "Rechteck gerundet", ((int)enSymbol.Rechteck_gerundet).ToString() },
                { "Pfeil", ((int)enSymbol.Pfeil).ToString(), QuickImage.Get("Pfeil_Rechts|32") },
                { "Bruchlinie", ((int)enSymbol.Bruchlinie).ToString() }
            };
            l.Add(new FlexiControl());
            l.Add(new FlexiControlForProperty(this, "Symbol", Comms));
            l.Add(new FlexiControlForProperty(this, "Randdicke"));
            l.Add(new FlexiControlForProperty(this, "Randfarbe"));
            l.Add(new FlexiControlForProperty(this, "Hintergrundfarbe"));
            l.AddRange(base.GetStyleOptions());
            return l;
        }

        public override bool ParseThis(string tag, string value) {
            if (base.ParseThis(tag, value)) { return true; }
            switch (tag) {
                case "symbol":
                    Symbol = (enSymbol)int.Parse(value);
                    return true;

                case "backcolor":
                    Hintergrundfarbe = value.FromHTMLCode();
                    return true;

                case "bordercolor":
                    Randfarbe = value.FromHTMLCode();
                    return true;

                case "borderwidth":
                    double.TryParse(value.FromNonCritical(), out var tRanddicke);
                    Randdicke = tRanddicke;
                    return true;

                case "fill": // alt: 28.11.2019
                case "whiteback": // alt: 28.11.2019
                    return true;
            }
            return false;
        }

        public override string ToString() {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";
            t = t + "Symbol=" + (int)Symbol + ", ";
            t = t + "Backcolor=" + Hintergrundfarbe.ToHTMLCode() + ", ";
            t = t + "BorderColor=" + Randfarbe.ToHTMLCode() + ", ";
            t = t + "BorderWidth=" + Randdicke.ToString().ToNonCritical() + ", ";
            return t.Trim(", ") + "}";
        }

        protected override string ClassId() => "Symbol";

        protected override void DrawExplicit(Graphics GR, RectangleF DCoordinates, double cZoom, double shiftX, double shiftY, enStates vState, Size SizeOfParentControl, bool ForPrinting) {
            var trp = DCoordinates.PointOf(enAlignment.Horizontal_Vertical_Center);
            GR.TranslateTransform(trp.X, trp.Y);
            GR.RotateTransform(-Drehwinkel);
            GraphicsPath p = null;
            var d2 = DCoordinates;
            d2.X = -DCoordinates.Width / 2;
            d2.Y = -DCoordinates.Height / 2;
            switch (Symbol) {
                case enSymbol.Ohne:
                    break;

                case enSymbol.Pfeil:
                    p = Poly_Arrow(d2.ToRect());
                    break;

                case enSymbol.Bruchlinie:
                    p = Poly_Bruchlinie(d2.ToRect());
                    break;

                case enSymbol.Rechteck:
                    p = Poly_Rechteck(d2.ToRect());
                    break;

                case enSymbol.Rechteck_gerundet:
                    p = Poly_RoundRec(d2.ToRect(), (int)(20 * cZoom));
                    break;

                default:
                    Develop.DebugPrint(Symbol);
                    break;
            }
            if (p != null) {
                GR.FillPath(new SolidBrush(Hintergrundfarbe), p);
                GR.DrawPath(new Pen(Randfarbe, (float)(Randdicke * cZoom * Parent.SheetStyleScale)), p);
            }
            GR.TranslateTransform(-trp.X, -trp.Y);
            GR.ResetTransform();
            base.DrawExplicit(GR, DCoordinates, cZoom, shiftX, shiftY, vState, SizeOfParentControl, ForPrinting);
        }

        protected override void ParseFinished() {
        }

        #endregion
    }
}