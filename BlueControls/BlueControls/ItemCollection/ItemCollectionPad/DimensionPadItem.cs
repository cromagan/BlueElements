// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using BlueBasics;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using static BlueBasics.Converter;
using static BlueBasics.Geometry;
using static BlueBasics.Polygons;

namespace BlueControls.ItemCollection;

public class DimensionPadItem : BasicPadItem {

    #region Fields

    private readonly PointM _bezugslinie1 = new(null, "Bezugslinie 1, Ende der Hilfslinie", 0, 0);
    private readonly PointM _bezugslinie2 = new(null, "Bezugslinie 2, Ende der Hilfslinien", 0, 0);

    /// <summary>
    /// Dieser Punkt ist sichtbar und kann verschoben werden.
    /// </summary>
    private readonly PointM? _point1 = new(null, "Punkt 1", 0, 0);

    /// <summary>
    /// Dieser Punkt ist sichtbar und kann verschoben werden.
    /// </summary>
    private readonly PointM? _point2 = new(null, "Punkt 2", 0, 0);

    private readonly PointM? _schnittPunkt1 = new(null, "Schnittpunkt 1, Zeigt der Pfeil hin", 0, 0);
    private readonly PointM? _schnittPunkt2 = new(null, "Schnittpunkt 2, Zeigt der Pfeil hin", 0, 0);

    /// <summary>
    /// Dieser Punkt ist sichtbar und kann verschoben werden.
    /// </summary>
    private readonly PointM? _textPoint = new(null, "Mitte Text", 0, 0);

    private float _länge;
    private string _textOben = string.Empty;
    private float _winkel;

    #endregion

    #region Constructors

    public DimensionPadItem(string internalname) : this(internalname, null, null, 0) { }

    public DimensionPadItem() : this(string.Empty, null, null, 0) { }

    public DimensionPadItem(PointM? point1, PointM? point2, float abstandinMm) : this(string.Empty, point1, point2, abstandinMm) { }

    public DimensionPadItem(string internalname, PointM? point1, PointM? point2, float abstandinMm) : base(internalname) {
        if (point1 != null) { _point1.SetTo(point1.X, point1.Y); }
        if (point2 != null) { _point2.SetTo(point2.X, point2.Y); }
        ComputeData();

        var a = PolarToCartesian(MmToPixel(abstandinMm, ItemCollectionPad.Dpi), _winkel - 90f);
        _textPoint.SetTo(_point1, _länge / 2, _winkel);
        _textPoint.X += a.X;
        _textPoint.Y += a.Y;

        Text_Oben = string.Empty;
        Text_Unten = string.Empty;
        Nachkommastellen = 1;

        Stil = PadStyles.Style_StandardAlternativ;
        _point1.Parent = this;
        _point2.Parent = this;
        _textPoint.Parent = this;
        _schnittPunkt1.Parent = this;
        _schnittPunkt2.Parent = this;
        _bezugslinie1.Parent = this;
        _bezugslinie2.Parent = this;

        MovablePoint.Add(_point1);
        MovablePoint.Add(_point2);
        MovablePoint.Add(_textPoint);
        PointsForSuccesfullyMove.AddRange(MovablePoint);

        CalculateOtherPoints();
    }

    public DimensionPadItem(PointF point1, PointF point2, float abstandInMm) : this(new PointM(point1), new PointM(point2), abstandInMm) { }

    #endregion

    #region Properties

    public static string ClassId => "DIMENSION";

    public float Länge_In_Mm => (float)Math.Round(PixelToMm(_länge, ItemCollectionPad.Dpi), Nachkommastellen);

    public int Nachkommastellen { get; set; }

    public string Präfix { get; set; } = string.Empty;

    //http://www.kurztutorial.info/programme/punkt-mm/rechner.html
    // Dim Ausgleich As float = MmToPixel(1 / 72 * 25.4, 300)
    public float Skalierung { get; set; } = 3.07f;

    public string Suffix { get; set; } = string.Empty;

    public string Text_Oben {
        get => _textOben;
        set {
            if (_textOben == Länge_In_Mm.ToString(CultureInfo.InvariantCulture)) { value = string.Empty; }
            _textOben = value;
            OnChanged();
        }
    }

    public string Text_Unten { get; set; }

    protected override int SaveOrder => 999;

    #endregion

    #region Methods

    public static void DrawArrow(Graphics gr, PointF point, float winkel, Color col, float fontSize) {
        var m1 = fontSize * 1.5f;
        var px2 = PolarToCartesian(m1, winkel + 10);
        var px3 = PolarToCartesian(m1, winkel - 10);
        var pa = Poly_Triangle(point, new PointF(point.X + px2.X, point.Y + px2.Y), new PointF(point.X + px3.X, point.Y + px3.Y));
        gr.FillPath(new SolidBrush(col), pa);
    }

    public string Angezeigter_Text_Oben() {
        if (!string.IsNullOrEmpty(Text_Oben)) { return Text_Oben; }
        var s = Länge_In_Mm.ToString(Constants.Format_Float10);
        s = s.Replace(".", ",");
        if (s.Contains(",")) {
            s = s.TrimEnd("0");
            s = s.TrimEnd(",");
        }
        return Präfix + s + Suffix;
    }

    public override bool Contains(PointF value, float zoomfactor) {
        var ne = 5 / zoomfactor;
        return value.DistanzZuStrecke(_point1, _bezugslinie1) < ne
               || value.DistanzZuStrecke(_point2, _bezugslinie2) < ne
               || value.DistanzZuStrecke(_schnittPunkt1, _schnittPunkt2) < ne
               || value.DistanzZuStrecke(_schnittPunkt1, _textPoint) < ne
               || Länge(new PointM(value), _textPoint) < ne * 10;
    }

    public override List<GenericControl> GetStyleOptions() {
        List<GenericControl> l = new()
        {
            new FlexiControlForProperty<float>(() => Länge_In_Mm),
            new FlexiControlForProperty<string>(() => Text_Oben),
            new FlexiControlForProperty<string>(() => Suffix),
            new FlexiControlForProperty<string>(() => Text_Unten),
            new FlexiControlForProperty<string>(() => Präfix)
        };
        AddStyleOption(l);
        l.Add(new FlexiControlForProperty<float>(() => Skalierung));
        l.AddRange(base.GetStyleOptions());
        return l;
    }

    public override void InitialPosition(int x, int y, int width, int height) {
        _point1.SetTo(x, y + height);
        _point2.SetTo(x + width, y + height);
        _textPoint.SetTo(x + (width / 2), y);
    }

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }
        switch (tag) {
            case "text": // TODO: Alt 06.09.2019

            case "text1":
                Text_Oben = value.FromNonCritical();
                return true;

            case "text2":
                Text_Unten = value.FromNonCritical();
                return true;

            case "color": // TODO: Alt 06.09.2019
                return true;

            case "fontsize": // TODO: Alt 06.09.2019
                return true;

            case "accuracy": // TODO: Alt 06.09.2019
                return true;

            case "decimal":
                Nachkommastellen = IntParse(value);
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
                Skalierung = FloatParse(value.FromNonCritical());
                return true;
        }
        return false;
    }

    public override void PointMoved(object sender, MoveEventArgs e) => CalculateOtherPoints();

    public override string ToString() {
        var result = new List<string>();
        result.ParseableAdd("Text1", Text_Oben);
        result.ParseableAdd("Text2", Text_Unten);
        result.ParseableAdd("Decimal", Nachkommastellen);
        result.ParseableAdd("refix", Präfix);
        result.ParseableAdd("Suffix", Suffix);
        result.ParseableAdd("AdditionalScale", Skalierung);
        return result.Parseable(base.ToString());
    }

    protected override RectangleF CalculateUsedArea() {
        if (Stil == PadStyles.Undefiniert) { return new RectangleF(0, 0, 0, 0); }
        var geszoom = Parent.SheetStyleScale * Skalierung;
        var f = Skin.GetBlueFont(Stil, Parent.SheetStyle);
        var sz1 = BlueFont.MeasureString(Angezeigter_Text_Oben(), f.Font(geszoom));
        var sz2 = BlueFont.MeasureString(Text_Unten, f.Font(geszoom));
        var maxrad = Math.Max(Math.Max(sz1.Width, sz1.Height), Math.Max(sz2.Width, sz2.Height));
        RectangleF x = new(_point1, new SizeF(0, 0));
        x = x.ExpandTo(_point1);
        x = x.ExpandTo(_bezugslinie1);
        x = x.ExpandTo(_bezugslinie2);
        x = x.ExpandTo(_textPoint, maxrad);

        x.Inflate(-2, -2); // die Sicherheits koordinaten damit nicht linien abgeschnitten werden
        return x;
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        if (Stil != PadStyles.Undefiniert) {
            var geszoom = Parent.SheetStyleScale * Skalierung * zoom;
            var f = Skin.GetBlueFont(Stil, Parent.SheetStyle);
            var pfeilG = f.Font(geszoom).Size * 0.8f;
            var pen2 = f.Pen(zoom);

            //DrawOutline(gr, zoom, shiftX, shiftY, Color.Red);
            //gr.DrawLine(pen2, UsedArea().PointOf(enAlignment.Top_Left).ZoomAndMove(zoom, shiftX, shiftY), UsedArea().PointOf(enAlignment.Bottom_Right).ZoomAndMove(zoom, shiftX, shiftY)); // Bezugslinie 1
            //gr.DrawLine(pen2, UsedArea().PointOf(enAlignment.Top_Left).ZoomAndMove(zoom, shiftX, shiftY), UsedArea().PointOf(enAlignment.Bottom_Left).ZoomAndMove(zoom, shiftX, shiftY)); // Bezugslinie 1

            gr.DrawLine(pen2, _point1.ZoomAndMove(zoom, shiftX, shiftY), _bezugslinie1.ZoomAndMove(zoom, shiftX, shiftY)); // Bezugslinie 1
            gr.DrawLine(pen2, _point2.ZoomAndMove(zoom, shiftX, shiftY), _bezugslinie2.ZoomAndMove(zoom, shiftX, shiftY)); // Bezugslinie 2
            gr.DrawLine(pen2, _schnittPunkt1.ZoomAndMove(zoom, shiftX, shiftY), _schnittPunkt2.ZoomAndMove(zoom, shiftX, shiftY)); // Maßhilfslinie
            gr.DrawLine(pen2, _schnittPunkt1.ZoomAndMove(zoom, shiftX, shiftY), _textPoint.ZoomAndMove(zoom, shiftX, shiftY)); // Maßhilfslinie
            var sz1 = gr.MeasureString(Angezeigter_Text_Oben(), f.Font(geszoom));
            var sz2 = gr.MeasureString(Text_Unten, f.Font(geszoom));
            var p1 = _schnittPunkt1.ZoomAndMove(zoom, shiftX, shiftY);
            var p2 = _schnittPunkt2.ZoomAndMove(zoom, shiftX, shiftY);
            if (sz1.Width + (pfeilG * 2f) < GetLenght(p1, p2)) {
                DrawArrow(gr, p1, _winkel, f.ColorMain, pfeilG);
                DrawArrow(gr, p2, _winkel + 180, f.ColorMain, pfeilG);
            } else {
                DrawArrow(gr, p1, _winkel + 180, f.ColorMain, pfeilG);
                DrawArrow(gr, p2, _winkel, f.ColorMain, pfeilG);
            }
            var mitte = _textPoint.ZoomAndMove(zoom, shiftX, shiftY);
            var textWinkel = _winkel % 360;
            if (textWinkel is > 90 and <= 270) { textWinkel = _winkel - 180; }
            if (geszoom < 0.15d) { return; } // Schrift zu klein, würde abstürzen
            PointM mitte1 = new(mitte, (float)(sz1.Height / 2.1), textWinkel + 90);
            var x = gr.Save();
            gr.TranslateTransform(mitte1.X, mitte1.Y);
            gr.RotateTransform(-textWinkel);
            gr.FillRectangle(new SolidBrush(Color.White), new RectangleF((int)(-sz1.Width * 0.9 / 2), (int)(-sz1.Height * 0.8 / 2), (int)(sz1.Width * 0.9), (int)(sz1.Height * 0.8)));
            f.DrawString(gr, Angezeigter_Text_Oben(), (float)(-sz1.Width / 2.0), (float)(-sz1.Height / 2.0), geszoom, StringFormat.GenericDefault);
            gr.Restore(x);
            PointM mitte2 = new(mitte, (float)(sz2.Height / 2.1), textWinkel - 90);
            x = gr.Save();
            gr.TranslateTransform(mitte2.X, mitte2.Y);
            gr.RotateTransform(-textWinkel);
            gr.FillRectangle(new SolidBrush(Color.White), new RectangleF((int)(-sz2.Width * 0.9 / 2), (int)(-sz2.Height * 0.8 / 2), (int)(sz2.Width * 0.9), (int)(sz2.Height * 0.8)));
            f.DrawString(gr, Text_Unten, (float)(-sz2.Width / 2.0), (float)(-sz2.Height / 2.0), geszoom, StringFormat.GenericDefault);
            gr.Restore(x);
        }
        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
    }

    protected override void ParseFinished() {
        base.ParseFinished();
        CalculateOtherPoints();
    }

    //protected override BasicPadItem? TryCreate(string id, string name) {
    //    if (id.Equals("blueelements.clsitemimage", StringComparison.OrdinalIgnoreCase) ||
    //        id.Equals("blueelements.imageitem", StringComparison.OrdinalIgnoreCase) ||
    //        id.Equals(ClassId, StringComparison.OrdinalIgnoreCase)) {
    //        return new DimensionPadItem(name);
    //    }
    //    return null;
    //}

    private void CalculateOtherPoints() {
        var tmppW = -90;
        var mhlAb = MmToPixel(1.5f * Skalierung / 3.07f, ItemCollectionPad.Dpi); // Den Abstand der Maßhilsfline, in echten MM
        ComputeData();

        //Gegeben sind:
        // Point1, Point2 und Textpoint
        var maßL = _textPoint.DistanzZuLinie(_point1, _point2);
        _schnittPunkt1.SetTo(_point1, maßL, _winkel - 90);
        _schnittPunkt2.SetTo(_point2, maßL, _winkel - 90);
        if (_textPoint.DistanzZuLinie(_schnittPunkt1, _schnittPunkt2) > 0.5d) {
            _schnittPunkt1.SetTo(_point1, maßL, _winkel + 90);
            _schnittPunkt2.SetTo(_point2, maßL, _winkel + 90);
            tmppW = 90;
        }
        _bezugslinie1.SetTo(_schnittPunkt1, mhlAb, _winkel + tmppW);
        _bezugslinie2.SetTo(_schnittPunkt2, mhlAb, _winkel + tmppW);
    }

    private void ComputeData() {
        _länge = Länge(_point1, _point2);
        _winkel = Winkel(_point1, _point2);
    }

    #endregion
}