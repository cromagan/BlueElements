// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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

#nullable enable

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.ItemCollectionPad.Abstract;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using BlueControls.ItemCollectionList;
using static BlueBasics.Converter;
using static BlueBasics.Polygons;

namespace BlueControls.ItemCollectionPad;

public class SymbolPadItem : RectanglePadItem {

    #region Constructors

    public SymbolPadItem() : base(string.Empty) {
        Symbol = Symbol.Pfeil;
        Hintergrundfarbe = Color.White;
        Randfarbe = Color.Black;
        Randdicke = 1;
    }

    #endregion

    #region Properties

    public static string ClassId => "Symbol";

    public override string Description => string.Empty;

    public Color Hintergrundfarbe { get; set; }

    public override string MyClassId => ClassId;

    public float Randdicke { get; set; }

    public Color Randfarbe { get; set; }

    public Symbol Symbol { get; set; }

    protected override int SaveOrder => 999;

    #endregion

    #region Methods

    public override List<GenericControl> GetProperties(int widthOfControl) {
        var comms = new List<AbstractListItem> {
            ItemOf("Ohne", ((int)Symbol.Ohne).ToString(), QuickImage.Get("Datei|32")),
            ItemOf("Rechteck", ((int)Symbol.Rechteck).ToString(), QuickImage.Get("Stop|32")),
            ItemOf("Rechteck gerundet", ((int)Symbol.Rechteck_gerundet).ToString()),
            ItemOf("Pfeil", ((int)Symbol.Pfeil).ToString(), QuickImage.Get("Pfeil_Rechts|32")),
            ItemOf("Bruchlinie", ((int)Symbol.Bruchlinie).ToString())
        };
        List<GenericControl> result =
        [
            new FlexiControlForProperty<Symbol>(() => Symbol, comms),
            new FlexiControlForProperty<float>(() => Randdicke),
            new FlexiControlForProperty<Color>(() => Randfarbe),
            new FlexiControlForProperty<Color>(() => Hintergrundfarbe),
            .. base.GetProperties(widthOfControl),
        ];
        return result;
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("Symbol", Symbol);
        result.ParseableAdd("Backcolor", Hintergrundfarbe.ToArgb());
        result.ParseableAdd("BorderColor", Randfarbe.ToArgb());
        result.ParseableAdd("BorderWidth", Randdicke);
        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "symbol":
                Symbol = (Symbol)IntParse(value);
                return true;

            case "backcolor":
                Hintergrundfarbe = value.FromHtmlCode();
                return true;

            case "bordercolor":
                Randfarbe = value.FromHtmlCode();
                return true;

            case "borderwidth":
                _ = FloatTryParse(value.FromNonCritical(), out var tRanddicke);
                Randdicke = tRanddicke;
                return true;

            case "fill": // alt: 28.11.2019
            case "whiteback": // alt: 28.11.2019
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() => "Symbol";

    public override QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Stern, 16);

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
            case Symbol.Ohne:
                break;

            case Symbol.Pfeil:
                p = Poly_Arrow(d2.ToRect());
                break;

            case Symbol.Bruchlinie:
                p = Poly_Bruchlinie(d2.ToRect());
                break;

            case Symbol.Rechteck:
                p = Poly_Rechteck(d2.ToRect());
                break;

            case Symbol.Rechteck_gerundet:
                p = Poly_RoundRec(d2.ToRect(), (int)(20 * zoom));
                break;

            default:
                Develop.DebugPrint(Symbol);
                break;
        }

        if (p != null && Parent != null) {
            gr.FillPath(new SolidBrush(Hintergrundfarbe), p);
            gr.DrawPath(new Pen(Randfarbe, Randdicke * zoom * Parent.SheetStyleScale), p);
        }

        gr.TranslateTransform(-trp.X, -trp.Y);
        gr.ResetTransform();
        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
    }

    #endregion
}