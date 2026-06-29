// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueControls.EventArgs;

namespace BlueControls.Classes.ItemCollectionPad.Abstract;

public abstract class RectanglePadItem : AbstractPadItem {

    #region Fields

    private int _drehwinkel;

    #endregion

    #region Constructors

    protected RectanglePadItem(string keyName) : base(keyName) {
        Plo = new PointM(this, "LO", 0, 0);
        Pro = new PointM(this, "RO", 0, 0);
        Pru = new PointM(this, "RU", 0, 0);
        Plu = new PointM(this, "LU", 0, 0);
        Pl = new PointM(this, "L", 0, 0);
        Pr = new PointM(this, "R", 0, 0);
        Po = new PointM(this, "O", 0, 0);
        Pu = new PointM(this, "U", 0, 0);

        Po.MoveXByMouse = false;
        Pu.MoveXByMouse = false;
        Pl.MoveYByMouse = false;
        Pr.MoveYByMouse = false;

        MovablePoint.Add(Plo);
        MovablePoint.Add(Pro);
        MovablePoint.Add(Plu);
        MovablePoint.Add(Pru);
        MovablePoint.Add(Pl);
        MovablePoint.Add(Pr);
        MovablePoint.Add(Pu);
        MovablePoint.Add(Po);
        PointsForSuccessfullyMove.Add(Plo);
        PointsForSuccessfullyMove.Add(Pru);
        CalculateJointMiddle(Pl, Pr);
        Drehwinkel = 0;
    }

    #endregion

    #region Properties

    [Description("Die Breite des Objekts in mm.")]
    public virtual float Breite {
        get => (float)Math.Round(PixelToMm(CanvasUsedArea.Width, ItemCollectionPadItem.Dpi), 2, MidpointRounding.AwayFromZero);
        set {
            if (IsDisposed) { return; }
            if (Math.Abs(Breite - value) < Constants.DefaultTolerance) { return; }
            Pru.X = Plo.X + MmToPixel(value, ItemCollectionPadItem.Dpi);
        }
    }

    public int Drehwinkel {
        get => _drehwinkel;
        set {
            if (IsDisposed) { return; }
            if (_drehwinkel == value) { return; }
            _drehwinkel = value;
            OnPropertyChanged();
        }
    }

    [Description("Die Höhe des Objekts in mm.")]
    public virtual float Höhe {
        get => (float)Math.Round(PixelToMm(CanvasUsedArea.Height, ItemCollectionPadItem.Dpi), 2, MidpointRounding.AwayFromZero);
        set {
            if (IsDisposed) { return; }
            if (Math.Abs(Höhe - value) < Constants.DefaultTolerance) { return; }
            Pru.Y = Plo.Y + MmToPixel(value, ItemCollectionPadItem.Dpi);
        }
    }

    protected PointM Pl { get; }
    protected PointM Plo { get; }
    protected PointM Plu { get; }
    protected PointM Po { get; }
    protected PointM Pr { get; }
    protected PointM Pro { get; }
    protected PointM Pru { get; }
    protected PointM Pu { get; }

    #endregion

    #region Methods

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result =
        [   .. base.GetProperties(widthOfControl),
            new FlexiControl(),
            new FlexiControlForProperty<float>(() => Breite),
            new FlexiControlForProperty<float>(() => Höhe),
            new FlexiControlForProperty<int>(() => Drehwinkel),

        ];
        return result;
    }

    public override void InitialPosition(int x, int y, int width, int height) => SetCoordinates(new RectangleF(x, y, width, height));

    public virtual void Mirror(PointM? p, bool vertical, bool horizontal) {
        p ??= new PointM(JointMiddle);

        foreach (var thisP in JointPoints) {
            thisP.Mirror(p, vertical, horizontal);
            DoJointPoint(thisP);
        }

        Plo.Mirror(p, vertical, horizontal);
        Pru.Mirror(p, vertical, horizontal);
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("Rotation", Drehwinkel);
        return result;
    }

    public override void ParseFinished(string parsed) {
        base.ParseFinished(parsed);
        CalculateSlavePoints();
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "fixsize": // TODO: Entfernt am 24.05.2021
                //_größe_fixiert = value.FromPlusMinus();
                return true;

            case "rotation":
                _drehwinkel = IntParse(value);
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override JsonObject ParseableJson() {
        var json = base.ParseableJson();
        json["rotation"] = _drehwinkel;
        return json;
    }

    public override bool ParseThisJson(string key, JsonElement value) {
        switch (key) {
            case "rotation":
                _drehwinkel = value.TryGetInt32(out var ri) ? ri : 0;
                return true;
        }
        return base.ParseThisJson(key, value);
    }

    public override void PointMoved(object sender, MoveEventArgs e) {
        if (sender is not PointM point) { return; }
        if (JointPoints.Contains(point)) {
            base.PointMoved(sender, e);
            return;
        }

        var x = point.X;
        var y = point.Y;

        if (point == Plo) {
            Po.Y = y;
            Pl.X = x;
        }

        if (point == Pro) {
            Po.Y = y;
            Pr.X = x;
        }

        if (point == Plu) {
            Pl.X = x;
            Pu.Y = y;
        }

        if (point == Pru) {
            Pr.X = x;
            Pu.Y = y;
        }

        if (point == Po) {
            Plo.Y = y;
            Pro.Y = y;
        }

        if (point == Pu) {
            Plu.Y = y;
            Pru.Y = y;
        }

        if (point == Pl) {
            Plo.X = x;
            Plu.X = x;
        }

        if (point == Pr) {
            Pro.X = x;
            Pru.X = x;
        }

        CalculateSlavePoints();
        base.PointMoved(sender, e);
    }

    public void SetCoordinates(RectangleF r) {
        Plo.SetTo(r.PointOf(Alignment.Top_Left), false);
        Pru.SetTo(r.PointOf(Alignment.Bottom_Right), false);
    }

    protected override RectangleF CalculateCanvasUsedArea() => new(Math.Min(Plo.X, Pru.X),
                                                               Math.Min(Plo.Y, Pru.Y),
                                                               Math.Abs(Pru.X - Plo.X),
                                                               Math.Abs(Pru.Y - Plo.Y));

    private void CalculateSlavePoints() {
        // Punkte immer komplett setzen. Um eventuelle Parsing-Fehler auszugleichen
        Pl.SetTo(Plo.X, Plo.Y + (Plu.Y - Plo.Y) / 2, false);
        Pr.SetTo(Pro.X, Plo.Y + (Plu.Y - Plo.Y) / 2, false);
        Pu.SetTo(Plo.X + (Pro.X - Plo.X) / 2, Pru.Y, false);
        Po.SetTo(Plo.X + (Pro.X - Plo.X) / 2, Pro.Y, false);
        CalculateJointMiddle(Pl, Pr);
    }

    #endregion
}