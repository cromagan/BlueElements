// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueControls.PadItems.Abstract;
using static BlueBasics.ClassesStatic.Geometry;

namespace BlueControls.PadItems;

/// <summary>
/// Eine Linie zwischen zwei Punkten.
/// </summary>
public class LinePadItem : PadItem, IStyleableOne {

    #region Fields

    /// <summary>
    /// Abstand der Umwege-Eckpunkte außerhalb der verdeckt geprüften Fläche.
    /// </summary>
    private const float AusweichAbstand = 5f;

    private readonly PointM _point1;

    private readonly PointM _point2;

    private string _calcTempPointsCode = string.Empty;

    private List<PointF>? _tempPoints;

    #endregion

    #region Constructors

    public LinePadItem() : this(string.Empty, PadStyles.Standard, Point.Empty, Point.Empty) { }

    public LinePadItem(PadStyles format, Point point1, Point point2) : this(string.Empty, format, point1, point2) { }

    public LinePadItem(string keyName, PadStyles format, Point point1, Point point2) : this(keyName, format, (PointF)point1, (PointF)point2) { }

    public LinePadItem(string keyName, PadStyles format, PointF point1, PointF point2) : base(keyName) {
        // Suppress-Modus whrend der Konstruktion: Property-Setter (z. B. Style)
        // lsen keine Change-Events aus. Siehe ParseableItem.ISupportInitialize.
        BeginInit();
        try {
            _point1 = new PointM(this, "Punkt 1", point1.X, point1.Y);
            _point2 = new PointM(this, "Punkt 2", point2.X, point2.Y);
            MovablePoint.Add(_point1);
            MovablePoint.Add(_point2);
            PointsForSuccessfullyMove.AddRange(MovablePoint);
            CalculateJointMiddle(_point1, _point2);
            Style = format;
            _tempPoints = [];
            Linien_Verhalten = ConnectorStyle.Direct;
        } finally {
            EndInit();
        }
    }

    #endregion

    #region Properties

    public static string ClassId => "LINE";

    public BlueFont? Font { get; set; }

    /// <summary>
    /// Der Verlauf der Linie.
    /// </summary>
    public ConnectorStyle Linien_Verhalten {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public string SheetStyle => Parent is IStyleable ist ? ist.SheetStyle : string.Empty;

    /// <summary>
    /// Aussehen der Linie, z. B. Strichstärke und Muster.
    /// </summary>
    public PadStyles Style {
        get;
        set {
            if (field == value) { return; }
            field = value;
            this.InvalidateFont();
            OnPropertyChanged();
        }
    } = PadStyles.Standard;

    protected override int SaveOrder => 999;

    #endregion

    #region Methods

    public override bool CanvasContains(PointF value, float zoom) {
        var ne = 6.ControlToCanvas(zoom) + 1;

        if (_point1.X == 0d && _point2.X == 0d && _point1.Y == 0d && _point2.Y == 0d) { return false; }
        if (_tempPoints is { Count: 0 }) { CalcTempPoints(); }
        if (_tempPoints is { Count: 0 }) { return false; }

        if (_tempPoints is not null) {
            for (var z = 0; z <= _tempPoints.Count - 2; z++) {
                if (value.DistanzZuStrecke(_tempPoints[z], _tempPoints[z + 1]) < ne) { return true; }
            }
        }

        return false;
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<ListItem> verhalt =
        [
            ItemOf("Linie direkt zwischen zwei Punkten", ((int)ConnectorStyle.Direct).ToString1(),
                QuickImage.Get(ImageCode.Linie)),
            ItemOf("Linie soll Objekten ausweichen", ((int)ConnectorStyle.Ausweichen).ToString1(),
                QuickImage.Get(ImageCode.Linie)),
            ItemOf("Linie soll Objekten ausweichen und rechtwinklig sein",
                ((int)ConnectorStyle.AusweichenUndGerade).ToString1(), QuickImage.Get(ImageCode.Linie))
        ];

        List<GenericControl> result =
        [
            new FlexiControlForProperty<ConnectorStyle>(() => Linien_Verhalten, verhalt),
            new FlexiControlForProperty<PadStyles>(() => Style, Skin.GetRahmenArt(SheetStyle, true))
        ];

        result.AddRange(base.GetProperties(widthOfControl));
        return result;
    }

    //public LinePadItem(string vInternal, PadStyles vFormat, enConectorStyle vArt, PointM cPoint1, PointM cPoint2)
    //{
    //    _Internal = vInternal;
    //    Point1.SetTo(cPoint1);
    //    Point2.SetTo(cPoint2);
    //    Style = vFormat;
    //    Art = vArt;
    //    if (string.IsNullOrEmpty(_Internal))
    //    {
    //        Develop.DebugError("Interner Name nicht vergeben.");
    //    }
    //}
    public override void InitialPosition(int x, int y, int width, int height) {
        _point1.SetTo(x, y + height / 2, false);
        _point2.SetTo(x + width, y + height / 2, false);
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("Connection", Linien_Verhalten);
        result.ParseableAdd("Style", Style);
        return result;
    }

    public override JsonObject ParseableJson() {
        var json = base.ParseableJson();
        json.Set("connection", (int)Linien_Verhalten);
        json.Set("style", (int)Style);
        return json;
    }

    public override void ParseJson(JsonObject json) {
        BeginInit();
        try {
            Linien_Verhalten = json.GetEnum("connection", Linien_Verhalten);
            Style = json.GetEnum("style", Style);
            base.ParseJson(json);
        } finally {
            EndInit();
        }
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "connection":
                Linien_Verhalten = (ConnectorStyle)IntParse(value);
                return true;

            case "style":
                Style = (PadStyles)IntParse(value);
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override void PointMoved(object? sender, MoveEventArgs e) {
        if (sender is not PointM) { return; }

        CalcTempPoints();
        CalculateJointMiddle(_point1, _point2);
        base.PointMoved(sender, e);
    }

    public override string ReadableText() => "Line";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Linie, 16);

    protected override RectangleF CalculateCanvasUsedArea() {
        if (_point1.X == 0d && _point2.X == 0d && _point1.Y == 0d && _point2.Y == 0d) { return RectangleF.Empty; }
        if (_tempPoints is not { Count: not 0 }) { CalcTempPoints(); }
        if (_tempPoints is not { Count: not 0 }) { return RectangleF.Empty; }
        var x1 = float.MaxValue;
        var y1 = float.MaxValue;
        var x2 = float.MinValue;
        var y2 = float.MinValue;
        foreach (var thisPoint in _tempPoints) {
            x1 = Math.Min(thisPoint.X, x1);
            y1 = Math.Min(thisPoint.Y, y1);
            x2 = Math.Max(thisPoint.X, x2);
            y2 = Math.Max(thisPoint.Y, y2);
        }
        return new RectangleF(x1, y1, x2 - x1, y2 - y1);
        //Return New Rectangle(CInt(Math.Min(Point1.ControlX, Point2.ControlX)), CInt(Math.Min(Point1.Y, Point2.Y)), CInt(Math.Abs(Point2.ControlX - Point1.ControlX)), CInt(Math.Abs(Point2.Y - Point1.Y)))
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        UnRegisterEvents();
    }

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, float zoom, float offsetX, float offsetY, bool forPrinting) {
        if (Style != PadStyles.Undefined) {
            CalcTempPoints();
            if (_tempPoints is not { Count: not 0 } || Parent is null) { return; }

            var clipState = gr.Save();
            if (Parent is CollectionPadItem icpi) {
                gr.SetClip(icpi.CanvasUsedArea.CanvasToControl(zoom, offsetX, offsetY, false));
            }

            for (var z = 0; z <= _tempPoints.Count - 2; z++) {
                gr.DrawLine(this.GetFont().Pen(zoom), _tempPoints[z].CanvasToControl(zoom, offsetX, offsetY), _tempPoints[z + 1].CanvasToControl(zoom, offsetX, offsetY));
            }

            gr.Restore(clipState);
        }
    }

    protected override void OnParentChanged() {
        base.OnParentChanged();
        this.InvalidateFont();
        if (Parent is CollectionPadItem icpi) {
            icpi.StyleChanged += Icpi_StyleChanged;
            icpi.PropertyChanged += Icpi_PropertyChanged;
        }
    }

    protected override void OnParentChanging() {
        base.OnParentChanging();
        UnRegisterEvents();
    }

    private static bool SchneidetDas(PadItem? thisBasicItem, PointF p1, PointF p2) {
        if (thisBasicItem is null) { return false; }
        if (thisBasicItem is not LinePadItem) {
            var a = thisBasicItem.CanvasUsedArea;
            if (a is { Width: > 0, Height: > 0 }) {
                a.Inflate(2, 2);

                var lo = a.PointOf(Alignment.Top_Left);
                var ro = a.PointOf(Alignment.Top_Right);
                var lu = a.PointOf(Alignment.Bottom_Left);
                var ru = a.PointOf(Alignment.Bottom_Right);
                if (!LinesIntersect(p1, p2, lo, ro, true).IsEmpty ||
                    !LinesIntersect(p1, p2, lu, ru, true).IsEmpty ||
                    !LinesIntersect(p1, p2, lo, lu, true).IsEmpty ||
                    !LinesIntersect(p1, p2, ro, ru, true).IsEmpty) { return true; }
            }
        }
        return false;
    }

    private bool Begradige(int p1) {
        if (_tempPoints is null || p1 >= _tempPoints.Count - 1) { return false; }
        if ((int)_tempPoints[p1].X == (int)_tempPoints[p1 + 1].X || (int)_tempPoints[p1].Y == (int)_tempPoints[p1 + 1].Y) { return false; }

        PointF ecke1;
        PointF ecke2;
        if (Math.Abs(_tempPoints[p1].X - _tempPoints[p1 + 1].X) > Math.Abs(_tempPoints[p1].Y - _tempPoints[p1 + 1].Y)) {
            var mitteY = (_tempPoints[p1].Y + _tempPoints[p1 + 1].Y) / 2;
            ecke1 = new PointF(_tempPoints[p1].X, mitteY);
            ecke2 = new PointF(_tempPoints[p1 + 1].X, mitteY);
        } else {
            var mitteX = (_tempPoints[p1].X + _tempPoints[p1 + 1].X) / 2;
            ecke1 = new PointF(mitteX, _tempPoints[p1].Y);
            ecke2 = new PointF(mitteX, _tempPoints[p1 + 1].Y);
        }

        // Eine verdeckte Ecke würde LöscheVerdeckte sofort wieder entfernen: Diagonale belassen.
        if (IsVerdeckt(ecke1.X, ecke1.Y) || IsVerdeckt(ecke2.X, ecke2.Y)) { return false; }

        _tempPoints.Insert(p1 + 1, ecke1);
        _tempPoints.Insert(p1 + 2, ecke2);
        return true;
    }

    private void CalcTempPoints() {
        var newCode = _point1 + _point2.ParseableItems().FinishParseable();
        if (_calcTempPointsCode != newCode) {
            _calcTempPointsCode = newCode;
            _tempPoints = null;
        }
        if (_tempPoints is { Count: > 1 }) { return; }
        _calcTempPointsCode = newCode;
        _tempPoints =
        [
            _point1,
            _point2
        ];
        if (Linien_Verhalten == ConnectorStyle.Direct) { return; }
        for (var count = 1; count <= 50; count++) {
            var again = false;
            if (_tempPoints.Count > 100) { break; }

            for (var z = 0; z < _tempPoints.Count; z++) {
                if (LöscheVerdeckte(z)) {
                    again = true;
                    break;
                }
                if (Linien_Verhalten == ConnectorStyle.AusweichenUndGerade && Begradige(z)) {
                    again = true;
                    break;
                }
                if (Linien_Verhalten is ConnectorStyle.AusweichenUndGerade or ConnectorStyle.Ausweichen) {
                    if (WeicheAus(z)) {
                        again = true;
                        break;
                    }
                }
                if (Vereinfache(z)) {
                    again = true;
                    break;
                }
            }
            if (!again) { break; }
        }
    }

    private void Icpi_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
        // Umgebung (z. B. Boxen) hat sich geändert: Route verwerfen, damit sie beim nächsten Zeichnen neu berechnet wird.
        if (Linien_Verhalten == ConnectorStyle.Direct) { return; }
        _tempPoints = null;
    }

    private void Icpi_StyleChanged(object? sender, System.EventArgs e) => this.InvalidateFont();

    private bool IsVerdeckt(float x, float y) {
        if (Parent is not CollectionPadItem { IsDisposed: false } icpi) { return false; }

        foreach (var thisBasicItem in icpi) {
            if (thisBasicItem is { IsDisposed: false } and not LinePadItem) {
                var a = thisBasicItem.CanvasUsedArea;
                if (a is { Width: > 0, Height: > 0 }) {
                    a.Inflate(2, 2);
                    if (a.Contains(x, y)) { return true; }
                }
            }
        }
        return false;
    }

    private bool LöscheVerdeckte(int p1) {
        if (_tempPoints?[p1] is not { } p) { return false; }
        if (p1 == 0 || p1 >= _tempPoints.Count - 1) { return false; } // Endpunkte der Linie niemals löschen
        if (IsVerdeckt(p.X, p.Y)) {
            _tempPoints.RemoveAt(p1);
            return true;
        }
        return false;
    }

    private bool SchneidetWas(float x1, float y1, float x2, float y2) {
        if (Parent is not CollectionPadItem { IsDisposed: false } icpi) { return false; }
        var p1 = new PointF(x1, y1);
        var p2 = new PointF(x2, y2);
        return icpi.Any(thisItemBasic => SchneidetDas(thisItemBasic, p1, p2));
    }

    private void UnRegisterEvents() {
        if (Parent is CollectionPadItem icpi) {
            icpi.StyleChanged -= Icpi_StyleChanged;
            icpi.PropertyChanged -= Icpi_PropertyChanged;
        }
    }

    private bool Vereinfache(int p1) {
        if (_tempPoints is null) { return false; }

        if (Linien_Verhalten != ConnectorStyle.AusweichenUndGerade) {
            if (p1 > 0 && p1 < _tempPoints.Count - 1) {
                if (!SchneidetWas(_tempPoints[p1 - 1].X, _tempPoints[p1 - 1].Y, _tempPoints[p1 + 1].X, _tempPoints[p1 + 1].Y)) {
                    _tempPoints.RemoveAt(p1);
                    return true;
                }
            }
        }
        if (p1 < _tempPoints.Count - 3) {
            if ((int)_tempPoints[p1].X == (int)_tempPoints[p1 + 1].X && (int)_tempPoints[p1].X == (int)_tempPoints[p1 + 2].X) {
                _tempPoints.RemoveAt(p1 + 1);
                return true;
            }
            if ((int)_tempPoints[p1].Y == (int)_tempPoints[p1 + 1].Y && (int)_tempPoints[p1].Y == (int)_tempPoints[p1 + 2].Y) {
                _tempPoints.RemoveAt(p1 + 1);
                return true;
            }
        }
        if (p1 > 0 && p1 < _tempPoints.Count - 3) {
            if ((int)_tempPoints[p1].X == (int)_tempPoints[p1 + 1].X && (int)_tempPoints[p1 + 1].Y == (int)_tempPoints[p1 + 2].Y) {
                if (!IsVerdeckt(_tempPoints[p1 + 2].X, _tempPoints[p1].Y)) {
                    if (!SchneidetWas(_tempPoints[p1 - 1].X, _tempPoints[p1 - 1].Y, _tempPoints[p1 + 2].X, _tempPoints[p1].Y)) {
                        if (!SchneidetWas(_tempPoints[p1 + 3].X, _tempPoints[p1 + 3].Y, _tempPoints[p1 + 2].X, _tempPoints[p1].Y)) {
                            _tempPoints[p1] = new PointF(_tempPoints[p1 + 2].X, _tempPoints[p1].Y);
                            _tempPoints.RemoveAt(p1 + 1);
                            _tempPoints.RemoveAt(p1 + 1);
                            return true;
                        }
                    }
                }
            }
            if ((int)_tempPoints[p1].Y == (int)_tempPoints[p1 + 1].Y && (int)_tempPoints[p1 + 1].X == (int)_tempPoints[p1 + 2].X) {
                if (!IsVerdeckt(_tempPoints[p1].X, _tempPoints[p1 + 2].Y)) {
                    if (!SchneidetWas(_tempPoints[p1 - 1].X, _tempPoints[p1 - 1].Y, _tempPoints[p1].X, _tempPoints[p1 + 2].Y)) {
                        if (!SchneidetWas(_tempPoints[p1 + 3].X, _tempPoints[p1 + 3].Y, _tempPoints[p1].X, _tempPoints[p1 + 2].Y)) {
                            _tempPoints[p1] = new PointF(_tempPoints[p1].X, _tempPoints[p1 + 2].Y);
                            _tempPoints.RemoveAt(p1 + 1);
                            _tempPoints.RemoveAt(p1 + 1);
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    private bool WeicheAus(int p1) {
        if (_tempPoints is null || p1 >= _tempPoints.Count - 1) { return false; }
        if (Parent is not CollectionPadItem { IsDisposed: false } icpi) { return false; }
        if (_tempPoints.Count > 100) { return false; }

        var pA = _tempPoints[p1];
        var pB = _tempPoints[p1 + 1];
        var start = _tempPoints[0]; // Seitwahl läuft über den Linienanfang, nicht über das Teilstück.

        foreach (var thisItemBasic in icpi) {
            if (thisItemBasic is { IsDisposed: false } and not LinePadItem) {
                var a = thisItemBasic.CanvasUsedArea;
                if (a is { Width: > 0, Height: > 0 }) {
                    a.Inflate(2, 2);

                    var tOben = LinesIntersect(pA, pB, a.PointOf(Alignment.Top_Left), a.PointOf(Alignment.Top_Right), true);
                    var tUnten = LinesIntersect(pA, pB, a.PointOf(Alignment.Bottom_Left), a.PointOf(Alignment.Bottom_Right), true);
                    var tLinks = LinesIntersect(pA, pB, a.PointOf(Alignment.Top_Left), a.PointOf(Alignment.Bottom_Left), true);
                    var tRechts = LinesIntersect(pA, pB, a.PointOf(Alignment.Top_Right), a.PointOf(Alignment.Bottom_Right), true);

                    if (tOben.IsEmpty && tUnten.IsEmpty && tLinks.IsEmpty && tRechts.IsEmpty) { continue; }

                    // Eckpunkte außerhalb der verdeckt geprüften Fläche, damit LöscheVerdeckte sie nicht sofort wieder entfernt.
                    var lo = new PointF(a.Left - AusweichAbstand, a.Top - AusweichAbstand);
                    var ro = new PointF(a.Right + AusweichAbstand, a.Top - AusweichAbstand);
                    var lu = new PointF(a.Left - AusweichAbstand, a.Bottom + AusweichAbstand);
                    var ru = new PointF(a.Right + AusweichAbstand, a.Bottom + AusweichAbstand);

                    if (!tOben.IsEmpty && !tUnten.IsEmpty) {
                        // Senkrecht durch das Objekt: links oder rechts herum, die Fahrtrichtung bestimmt die Reihenfolge.
                        var linksErst = start.X <= (a.Left + a.Right) / 2;
                        PointF[] ersteWahl;
                        PointF[] zweiteWahl;
                        if (pA.Y < pB.Y) {
                            ersteWahl = linksErst ? new[] { lo, lu } : new[] { ro, ru };
                            zweiteWahl = linksErst ? new[] { ro, ru } : new[] { lo, lu };
                        } else {
                            ersteWahl = linksErst ? new[] { lu, lo } : new[] { ru, ro };
                            zweiteWahl = linksErst ? new[] { ru, ro } : new[] { lu, lo };
                        }
                        return EckenEinsetzen(p1, a, ersteWahl) || EckenEinsetzen(p1, a, zweiteWahl);
                    }

                    if (!tLinks.IsEmpty && !tRechts.IsEmpty) {
                        // Waagerecht durch das Objekt: oben oder unten herum, die Fahrtrichtung bestimmt die Reihenfolge.
                        var obenErst = start.Y <= (a.Top + a.Bottom) / 2;
                        PointF[] ersteWahl;
                        PointF[] zweiteWahl;
                        if (pA.X < pB.X) {
                            ersteWahl = obenErst ? new[] { lo, ro } : new[] { lu, ru };
                            zweiteWahl = obenErst ? new[] { lu, ru } : new[] { lo, ro };
                        } else {
                            ersteWahl = obenErst ? new[] { ro, lo } : new[] { ru, lu };
                            zweiteWahl = obenErst ? new[] { ru, lu } : new[] { ro, lo };
                        }
                        return EckenEinsetzen(p1, a, ersteWahl) || EckenEinsetzen(p1, a, zweiteWahl);
                    }

                    // Nur eine Ecke getroffen: diese eine Ecke ausweichen.
                    if (!tUnten.IsEmpty && !tRechts.IsEmpty) { return EckenEinsetzen(p1, a, ru); }
                    if (!tOben.IsEmpty && !tRechts.IsEmpty) { return EckenEinsetzen(p1, a, ro); }
                    if (!tUnten.IsEmpty && !tLinks.IsEmpty) { return EckenEinsetzen(p1, a, lu); }
                    if (!tOben.IsEmpty && !tLinks.IsEmpty) { return EckenEinsetzen(p1, a, lo); }

                    // Nur eine Kante getroffen: zur Schnittstelle nächste Ecke ausweichen.
                    if (!tOben.IsEmpty) { return EckenEinsetzen(p1, a, tOben.X < (a.Left + a.Right) / 2 ? lo : ro); }
                    if (!tUnten.IsEmpty) { return EckenEinsetzen(p1, a, tUnten.X < (a.Left + a.Right) / 2 ? lu : ru); }
                    if (!tLinks.IsEmpty) { return EckenEinsetzen(p1, a, tLinks.Y < (a.Top + a.Bottom) / 2 ? lo : lu); }
                    return EckenEinsetzen(p1, a, tRechts.Y < (a.Top + a.Bottom) / 2 ? ro : ru);
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Fügt die Eckpunkte in die Punktliste ein, wenn keiner verdeckt ist und der Umweg das Objekt frei umrundet.
    /// </summary>
    private bool EckenEinsetzen(int p1, RectangleF umwegUm, params PointF[] ecken) {
        if (_tempPoints is null) { return false; }

        var vorher = _tempPoints[p1];
        var nachher = _tempPoints[p1 + 1];

        foreach (var ecke in ecken) {
            if (IsVerdeckt(ecke.X, ecke.Y)) { return false; }
        }

        if (StreckeSchneidetRechteck(vorher, ecken[0], umwegUm)) { return false; }
        for (var z = 0; z < ecken.Length - 1; z++) {
            if (StreckeSchneidetRechteck(ecken[z], ecken[z + 1], umwegUm)) { return false; }
        }
        if (StreckeSchneidetRechteck(ecken[^1], nachher, umwegUm)) { return false; }

        for (var z = 0; z < ecken.Length; z++) {
            _tempPoints.Insert(p1 + 1 + z, ecken[z]);
        }
        return true;
    }

    private static bool StreckeSchneidetRechteck(PointF p1, PointF p2, RectangleF rechteck) {
        return !LinesIntersect(p1, p2, rechteck.PointOf(Alignment.Top_Left), rechteck.PointOf(Alignment.Top_Right), true).IsEmpty ||
            !LinesIntersect(p1, p2, rechteck.PointOf(Alignment.Bottom_Left), rechteck.PointOf(Alignment.Bottom_Right), true).IsEmpty ||
            !LinesIntersect(p1, p2, rechteck.PointOf(Alignment.Top_Left), rechteck.PointOf(Alignment.Bottom_Left), true).IsEmpty ||
            !LinesIntersect(p1, p2, rechteck.PointOf(Alignment.Top_Right), rechteck.PointOf(Alignment.Bottom_Right), true).IsEmpty;
    }

    #endregion
}
