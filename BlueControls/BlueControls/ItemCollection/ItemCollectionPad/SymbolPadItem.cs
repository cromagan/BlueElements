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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using static BlueBasics.Polygons;

namespace BlueControls.ItemCollection {

    public class SymbolPadItem : RectanglePadItem {

        #region Constructors

        public SymbolPadItem() : this(string.Empty) { }

        public SymbolPadItem(string internalname) : base(internalname) {
            Symbol = enSymbol.Pfeil;
            Hintergrundfarbe = Color.White;
            Randfarbe = Color.Black;
            Randdicke = 1;
        }

        #endregion

        #region Properties

        public Color Hintergrundfarbe { get; set; }

        public float Randdicke { get; set; }

        public Color Randfarbe { get; set; }

        public enSymbol Symbol { get; set; }

        #endregion

        #region Methods

        public override List<FlexiControl> GetStyleOptions() {
            ItemCollectionList comms = new()
            {
                { "Ohne", ((int)enSymbol.Ohne).ToString(), QuickImage.Get("Datei|32") },
                { "Rechteck", ((int)enSymbol.Rechteck).ToString(), QuickImage.Get("Stop|32") },
                { "Rechteck gerundet", ((int)enSymbol.Rechteck_gerundet).ToString() },
                { "Pfeil", ((int)enSymbol.Pfeil).ToString(), QuickImage.Get("Pfeil_Rechts|32") },
                { "Bruchlinie", ((int)enSymbol.Bruchlinie).ToString() }
            };
            List<FlexiControl> l = new()
            {
                new FlexiControlForProperty(this, "Symbol", comms),
                new FlexiControlForProperty(this, "Randdicke"),
                new FlexiControlForProperty(this, "Randfarbe"),
                new FlexiControlForProperty(this, "Hintergrundfarbe")
            };
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
                    float.TryParse(value.FromNonCritical(), out var tRanddicke);
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

        protected override void DrawExplicit(Graphics gr, RectangleF drawingCoordinates, float zoom, float shiftX, float shiftY, bool forPrinting) {
            var trp = drawingCoordinates.PointOf(enAlignment.Horizontal_Vertical_Center);
            gr.TranslateTransform(trp.X, trp.Y);
            gr.RotateTransform(-Drehwinkel);
            GraphicsPath? p = null;

            // Wegen der Nullpunktverschiebung wird ein temporäres Rechteck benötigt
            var d2 = drawingCoordinates;
            d2.X = -drawingCoordinates.Width / 2;
            d2.Y = -drawingCoordinates.Height / 2;

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
                    p = Poly_RoundRec(d2.ToRect(), (int)(20 * zoom));
                    break;

                default:
                    Develop.DebugPrint(Symbol);
                    break;
            }

            if (p != null) {
                gr.FillPath(new SolidBrush(Hintergrundfarbe), p);
                gr.DrawPath(new Pen(Randfarbe, Randdicke * zoom * Parent.SheetStyleScale), p);
            }

            gr.TranslateTransform(-trp.X, -trp.Y);
            gr.ResetTransform();
            base.DrawExplicit(gr, drawingCoordinates, zoom, shiftX, shiftY, forPrinting);
        }

        #endregion
    }
}