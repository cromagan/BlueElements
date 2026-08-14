// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueControls.PadItems.Abstract;
using static BlueBasics.ClassesStatic.Geometry;

namespace BlueControls.PadItems;

public sealed class DimensionPadItem : PadItem, IStyleableOne, ISupportsTextScale {

    #region Fields

    private readonly PointM _bezugslinie1 = new(null, "Bezugslinie 1, Ende der Hilfslinie", 0, 0);

    private readonly PointM _bezugslinie2 = new(null, "Bezugslinie 2, Ende der Hilfslinien", 0, 0);

    /// <summary>
    /// Dieser Punkt ist sichtbar und kann verschoben werden.
    /// </summary>
    private readonly PointM _point1 = new(null, "Punkt 1", 0, 0);

    /// <summary>
    /// Dieser Punkt ist sichtbar und kann verschoben werden.
    /// </summary>
    private readonly PointM _point2 = new(null, "Punkt 2", 0, 0);

    private readonly PointM _schnittPunkt1 = new(null, "Schnittpunkt 1, Zeigt der Pfeil hin", 0, 0);

    private readonly PointM _schnittPunkt2 = new(null, "Schnittpunkt 2, Zeigt der Pfeil hin", 0, 0);

    /// <summary>
    /// Dieser Punkt ist sichtbar und kann verschoben werden.
    /// </summary>
    private readonly PointM _textPoint = new(null, "Mitte Text", 0, 0);

    private float _länge;
    private float _winkel;

    #endregion

    #region Constructors

    public DimensionPadItem() : this(string.Empty, null, null, 0) { }

    public DimensionPadItem(PointM? point1, PointM? point2, float abstandinMm) : this(string.Empty, point1, point2, abstandinMm) { }

    public DimensionPadItem(string keyName, PointM? point1, PointM? point2, float abstandinMm) : base(keyName) {
        // Suppress-Modus whrend der Konstruktion: Property-Setter (z. B. Style,
        // Text_Oben, Nachkommastellen) lsen keine Change-Events aus.
        // Siehe ParseableItem.ISupportInitialize.
        BeginInit();
        try {
            if (point1 is not null) { _point1.SetTo(point1.X, point1.Y, false); }
            if (point2 is not null) { _point2.SetTo(point2.X, point2.Y, false); }
            ComputeData();

            var a = PolarToCartesian(MmToPixel(abstandinMm, CollectionPadItem.Dpi), _winkel - 90f);
            _textPoint.SetTo(_point1, _länge / 2, _winkel, false);
            _textPoint.X += a.X;
            _textPoint.Y += a.Y;

            Text_Oben = string.Empty;
            Text_Unten = string.Empty;
            Nachkommastellen = 1;

            Style = PadStyles.Alternative;
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
            PointsForSuccessfullyMove.AddRange(MovablePoint);

            CalculateOtherPoints();
        } finally {
            EndInit();
        }
    }

    public DimensionPadItem(PointF point1, PointF point2, float abstandInMm) : this(new PointM(point1), new PointM(point2), abstandInMm) { }

    #endregion

    #region Properties

    public static string ClassId => "DIMENSION";

    public override string Description => string.Empty;

    public BlueFont? Font { get; set; }

    public float Länge_In_Mm => (float)Math.Round(PixelToMm(_länge, CollectionPadItem.Dpi), Nachkommastellen, MidpointRounding.AwayFromZero);

    public int Nachkommastellen { get; set; }

    public string Präfix {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    public string SheetStyle => Parent is IStyleable ist ? ist.SheetStyle : string.Empty;

    public PadStyles Style {
        get;
        set {
            if (field == value) { return; }
            field = value;
            this.InvalidateFont();
            OnPropertyChanged();
        }
    } = PadStyles.Standard;

    public string Suffix {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    public string Text_Oben {
        get;
        set {
            if (IsDisposed) { return; }
            if (value == Länge_In_Mm.ToString1_3()) { value = string.Empty; }
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    public string Text_Unten {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    public float TextScale {
        get;
        set {
            value = Math.Clamp(value, 0.01f, 20);
            if (Math.Abs(value - field) < DefaultTolerance) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = 3.07f;

    protected override int SaveOrder => 999;

    #endregion

    #region Methods

    public static void DrawArrow(Graphics gr, PointF point, float winkel, Color col, float fontSize) {
        var m1 = fontSize * 1.5f;
        var px2 = PolarToCartesian(m1, winkel + 10);
        var px3 = PolarToCartesian(m1, winkel - 10);
        var pa = GraphicsPaths.Triangle(point, new PointF(point.X + px2.X, point.Y + px2.Y), new PointF(point.X + px3.X, point.Y + px3.Y));
        gr.FillPath(new SolidBrush(col), pa);
    }

    public string Angezeigter_Text_Oben() {
        if (!string.IsNullOrEmpty(Text_Oben)) { return Text_Oben; }
        var s = Länge_In_Mm.ToString1_3(); // nur 3, wegen umrechnungsfehlern Inch zu mm
        s = s.Replace('.', ',');
        if (s.Contains(',')) {
            s = s.TrimEnd('0').TrimEnd(',');
        }

        return Präfix + s + Suffix;
    }

    public override bool CanvasContains(PointF value, float zoom) {
        var ne = 6.ControlToCanvas(zoom) + 1;
        return value.DistanzZuStrecke(_point1, _bezugslinie1) < ne
               || value.DistanzZuStrecke(_point2, _bezugslinie2) < ne
               || value.DistanzZuStrecke(_schnittPunkt1, _schnittPunkt2) < ne
               || value.DistanzZuStrecke(_schnittPunkt1, _textPoint) < ne
               || GetLength(new PointM(value), _textPoint) < ne * 10;
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result =
        [
            new FlexiControlForProperty<float>(() => Länge_In_Mm) { Suffix = "mm" },

            new FlexiControlForProperty<string>(() => Text_Oben),

            new FlexiControlForProperty<string>(() => Suffix),

            new FlexiControlForProperty<string>(() => Text_Unten),

            new FlexiControlForProperty<string>(() => Präfix),
            new FlexiControlForProperty<PadStyles>(() => Style, Skin.GetFonts(SheetStyle)),
            new FlexiControlForProperty<float>(() => TextScale)
        ];
        result.AddRange(base.GetProperties(widthOfControl));
        return result;
    }

    public override void InitialPosition(int x, int y, int width, int height) {
        _point1.SetTo(x, y + height, false);
        _point2.SetTo(x + width, y + height, false);
        _textPoint.SetTo(x + width / 2, y, false);
    }

    public void Mirror(PointM? p, bool vertical, bool horizontal) {
        p ??= new PointM(_textPoint);

        _point1.Mirror(p, vertical, horizontal);
        _point2.Mirror(p, vertical, horizontal);
        _textPoint.Mirror(p, vertical, horizontal);
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("Text1", Text_Oben);
        result.ParseableAdd("Text2", Text_Unten);
        result.ParseableAdd("Decimal", Nachkommastellen);
        result.ParseableAdd("refix", Präfix);
        result.ParseableAdd("Suffix", Suffix);
        result.ParseableAdd("AdditionalScale", TextScale);
        result.ParseableAdd("Style", Style);
        return result;
    }

    public override JsonObject ParseableJson() {
        var json = base.ParseableJson();
        json.Set("text1", Text_Oben);
        json.Set("text2", Text_Unten);
        json.Set("decimal", Nachkommastellen);
        json.Set("prefix", Präfix);
        json.Set("suffix", Suffix);
        json.Set("additionalscale", TextScale);
        json.Set("style", (int)Style);
        return json;
    }

    public override void ParseFinished(string parsed) {
        base.ParseFinished(parsed);
        CalculateOtherPoints();
    }

    public override void ParseFinishedJson(JsonObject parsed) {
        base.ParseFinishedJson(parsed);
        CalculateOtherPoints();
    }

    public override void ParseJson(JsonObject json) {
        BeginInit();
        try {
            Text_Oben = json.GetString("text1", Text_Oben);
            Text_Unten = json.GetString("text2", Text_Unten);
            Nachkommastellen = json.GetInt("decimal", Nachkommastellen);
            Präfix = json.GetString("prefix", Präfix);
            Suffix = json.GetString("suffix", Suffix);
            TextScale = json.GetFloat("additionalscale", TextScale);
            Style = json.GetEnum("style", Style);
            base.ParseJson(json);
        } finally {
            EndInit();
        }
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "text": // TODO: Alt 06.09.2019
            case "text1":
                Text_Oben = value.FromNonCritical();
                return true;

            case "text2":
                Text_Unten = value.FromNonCritical();
                return true;

            case "checked": // TODO: Alt 06.09.2019
            case "color": // TODO: Alt 06.09.2019
            case "fontsize": // TODO: Alt 06.09.2019
            case "accuracy": // TODO: Alt 06.09.2019

            case "decimal":
                Nachkommastellen = IntParse(value);
                return true;

            case "prefix":
                Präfix = value.FromNonCritical();
                return true;

            case "suffix":
                Suffix = value.FromNonCritical();
                return true;

            case "additionalscale":
                TextScale = FloatParse(value.FromNonCritical());
                return true;

            case "style":
                Style = (PadStyles)IntParse(value);
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override void PointMoved(object? sender, MoveEventArgs e) {
        if (sender is not PointM point) { return; }

        CalculateOtherPoints();
        base.PointMoved(sender, e);
    }

    public override string ReadableText() => "Bemaßung";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Bemaßung, 16);

    protected override RectangleF CalculateCanvasUsedArea() {
        if (Style == PadStyles.Undefined) { return new RectangleF(0, 0, 0, 0); }
        var f2 = this.GetFont(TextScale);

        var sz1 = f2.MeasureString(Angezeigter_Text_Oben());
        var sz2 = f2.MeasureString(Text_Unten);
        var maxrad = Math.Max(Math.Max(sz1.Width, sz1.Height), Math.Max(sz2.Width, sz2.Height));
        RectangleF x = new(_point1, new SizeF(0, 0));
        x = x.ExpandTo(_point2);
        x = x.ExpandTo(_bezugslinie1);
        x = x.ExpandTo(_bezugslinie2);
        x = x.ExpandTo(_textPoint, maxrad);

        x.Inflate(-2, -2); // die Sicherheits koordinaten damit nicht linien abgeschnitten werden
        return x;
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        UnRegisterEvents();
    }

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, float zoom, float offsetX, float offsetY, bool forPrinting) {
        if (Style != PadStyles.Undefined) {
            var geszoom = (float)Math.Round(TextScale * zoom, 2, MidpointRounding.AwayFromZero);

            var f = this.GetFont(geszoom);
            var pfeilG = f.Size * 0.8f;
            var pen2 = f.Pen(1f);

            //DrawOutline(gr, zoom, offsetX, offsetY, Color.Red);
            //gr.DrawLine(pen2, CanvasUsedArea().PointOf(enAlignment.Top_Left).CanvasToControl(zoom, offsetX, offsetY), CanvasUsedArea().PointOf(enAlignment.Bottom_Right).CanvasToControl(zoom, offsetX, offsetY)); // Bezugslinie 1
            //gr.DrawLine(pen2, CanvasUsedArea().PointOf(enAlignment.Top_Left).CanvasToControl(zoom, offsetX, offsetY), CanvasUsedArea().PointOf(enAlignment.Bottom_Left).CanvasToControl(zoom, offsetX, offsetY)); // Bezugslinie 1

            gr.DrawLine(pen2, _point1.CanvasToControl(zoom, offsetX, offsetY), _bezugslinie1.CanvasToControl(zoom, offsetX, offsetY)); // Bezugslinie 1
            gr.DrawLine(pen2, _point2.CanvasToControl(zoom, offsetX, offsetY), _bezugslinie2.CanvasToControl(zoom, offsetX, offsetY)); // Bezugslinie 2
            gr.DrawLine(pen2, _schnittPunkt1.CanvasToControl(zoom, offsetX, offsetY), _schnittPunkt2.CanvasToControl(zoom, offsetX, offsetY)); // Maßhilfslinie
            gr.DrawLine(pen2, _schnittPunkt1.CanvasToControl(zoom, offsetX, offsetY), _textPoint.CanvasToControl(zoom, offsetX, offsetY)); // Maßhilfslinie
            var sz1 = f.MeasureString(Angezeigter_Text_Oben());
            var sz2 = f.MeasureString(Text_Unten);
            var p1 = _schnittPunkt1.CanvasToControl(zoom, offsetX, offsetY);
            var p2 = _schnittPunkt2.CanvasToControl(zoom, offsetX, offsetY);
            if (sz1.Width + pfeilG * 2f < GetLength(p1, p2)) {
                DrawArrow(gr, p1, _winkel, f.ColorMain, pfeilG);
                DrawArrow(gr, p2, _winkel + 180, f.ColorMain, pfeilG);
            } else {
                DrawArrow(gr, p1, _winkel + 180, f.ColorMain, pfeilG);
                DrawArrow(gr, p2, _winkel, f.ColorMain, pfeilG);
            }
            var mitte = _textPoint.CanvasToControl(zoom, offsetX, offsetY);
            var textWinkel = _winkel % 360;
            if (textWinkel is > 90 and <= 270) { textWinkel = _winkel - 180; }

            if (geszoom < 0.15d) { return; } // Schrift zu klein, würde abstürzen

            PointM mitte1 = new(mitte, (float)(sz1.Height / 2.1), textWinkel + 90);
            var x = gr.Save();
            gr.TranslateTransform(mitte1.X, mitte1.Y);
            gr.RotateTransform(-textWinkel);
            gr.FillRectangle(new SolidBrush(Color.White), new RectangleF((int)(-sz1.Width * 0.9 / 2), (int)(-sz1.Height * 0.8 / 2), (int)(sz1.Width * 0.9), (int)(sz1.Height * 0.8)));
            f.DrawString(gr, Angezeigter_Text_Oben(), 1f, (float)(-sz1.Width / 2.0), (float)(-sz1.Height / 2.0));
            gr.Restore(x);
            PointM mitte2 = new(mitte, (float)(sz2.Height / 2.1), textWinkel - 90);
            x = gr.Save();
            gr.TranslateTransform(mitte2.X, mitte2.Y);
            gr.RotateTransform(-textWinkel);
            gr.FillRectangle(new SolidBrush(Color.White), new RectangleF((int)(-sz2.Width * 0.9 / 2), (int)(-sz2.Height * 0.8 / 2), (int)(sz2.Width * 0.9), (int)(sz2.Height * 0.8)));
            f.DrawString(gr, Text_Unten, 1f, (float)(-sz2.Width / 2.0), (float)(-sz2.Height / 2.0));
            gr.Restore(x);
        }
    }

    protected override void OnParentChanged() {
        base.OnParentChanged();
        this.InvalidateFont();
        if (Parent is CollectionPadItem icpi) {
            icpi.StyleChanged += Icpi_StyleChanged;
        }
    }

    protected override void OnParentChanging() {
        base.OnParentChanging();
        UnRegisterEvents();
    }

    private void CalculateOtherPoints() {
        var tmppW = -90;
        var mhlAb = MmToPixel(1.5f * TextScale / 3.07f, CollectionPadItem.Dpi); // Den Abstand der Maßhilsfline, in echten MM
        ComputeData();

        //Gegeben sind:
        // Point1, Point2 und Textpoint
        var maßL = _textPoint.DistanzZuLinie(_point1, _point2);
        _schnittPunkt1.SetTo(_point1, maßL, _winkel - 90, false);
        _schnittPunkt2.SetTo(_point2, maßL, _winkel - 90, false);
        if (_textPoint.DistanzZuLinie(_schnittPunkt1, _schnittPunkt2) > 0.5d) {
            _schnittPunkt1.SetTo(_point1, maßL, _winkel + 90, false);
            _schnittPunkt2.SetTo(_point2, maßL, _winkel + 90, false);
            tmppW = 90;
        }
        _bezugslinie1.SetTo(_schnittPunkt1, mhlAb, _winkel + tmppW, false);
        _bezugslinie2.SetTo(_schnittPunkt2, mhlAb, _winkel + tmppW, false);
    }

    private void ComputeData() {
        _länge = GetLength(_point1, _point2);
        _winkel = GetAngle(_point1, _point2);
    }

    private void Icpi_StyleChanged(object? sender, System.EventArgs e) => this.InvalidateFont();

    private void UnRegisterEvents() {
        if (Parent is CollectionPadItem icpi) {
            icpi.StyleChanged -= Icpi_StyleChanged;
        }
    }

    #endregion
}