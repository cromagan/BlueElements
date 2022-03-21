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
using System.Globalization;
using static BlueBasics.Polygons;
using static BlueBasics.Converter;
using System;

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
            ItemCollectionList.ItemCollectionList comms = new()
            {
                { "Ohne", ((int)enSymbol.Ohne).ToString(), QuickImage.Get("Datei|32") },
                { "Rechteck", ((int)enSymbol.Rechteck).ToString(), QuickImage.Get("Stop|32") },
                { "Rechteck gerundet", ((int)enSymbol.Rechteck_gerundet).ToString() },
                { "Pfeil", ((int)enSymbol.Pfeil).ToString(), QuickImage.Get("Pfeil_Rechts|32") },
                { "Bruchlinie", ((int)enSymbol.Bruchlinie).ToString() }
            };
            List<FlexiControl> l = new()
            {
                new FlexiControlForProperty<enSymbol>(() => Symbol, comms),
                new FlexiControlForProperty<float>(() => Randdicke),
                new FlexiControlForProperty<Color>(() => Randfarbe),
                new FlexiControlForProperty<Color>(() => Hintergrundfarbe)
            };
            l.AddRange(base.GetStyleOptions());
            return l;
        }

        public override bool ParseThis(string tag, string value) {
            if (base.ParseThis(tag, value)) { return true; }
            switch (tag) {
                case "symbol":
                    Symbol = (enSymbol)IntParse(value);
                    return true;

                case "backcolor":
                    Hintergrundfarbe = value.FromHtmlCode();
                    return true;

                case "bordercolor":
                    Randfarbe = value.FromHtmlCode();
                    return true;

                case "borderwidth":
                    FloatTryParse(value.FromNonCritical(), out var tRanddicke);
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
            t = t + "Backcolor=" + Hintergrundfarbe.ToHtmlCode() + ", ";
            t = t + "BorderColor=" + Randfarbe.ToHtmlCode() + ", ";
            t = t + "BorderWidth=" + Randdicke.ToString(CultureInfo.InvariantCulture).ToNonCritical() + ", ";
            return t.Trim(", ") + "}";
        }

        protected override string ClassId() => "Symbol";

        protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
            var trp = positionModified.PointOf(Alignment.Horizontal_Vertical_Center);
            gr.TranslateTransform(trp.X, trp.Y);
            gr.RotateTransform(-Drehwinkel);
            GraphicsPath? p = null;

            // Wegen der Nullpunktverschiebung wird ein temporäres Rechteck benötigt
            var d2 = positionModified;
            d2.X = -positionModified.Width / 2;
            d2.Y = -positionModified.Height / 2;

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
            base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
        }

        protected override BasicPadItem? TryParse(string id, string name, List<KeyValuePair<string, string>> toParse) {
            if (id.Equals(ClassId(), StringComparison.OrdinalIgnoreCase)) {
                var x = new SymbolPadItem(name);
                x.Parse(toParse);
                return x;
            }
            return null;
        }

        #endregion
    }
}