// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using BlueControls.Classes.ItemCollectionPad.Abstract;
using BlueControls.Controls;
using BlueControls.EventArgs;
using static BlueBasics.ClassesStatic.Geometry;

namespace BlueControls.Classes.ItemCollectionPad;

public class ComicCompPadItem : AbstractPadItem {

    #region Fields

    /// <summary>
    /// Diese Punkte bestimmen die gedrehten Eckpunkte des Bildes und werden von den Gelenkpunkten aus berechnet. Unskaliert und auch ohne Berücksichtigung der 'MoveAllItems' Koordinaten
    /// </summary>
    private readonly PointM _ber_Lo = new();

    private readonly PointM _ber_Lu = new();

    private readonly PointM _ber_Ro = new();

    private readonly PointM _ber_Ru = new();

    private Bitmap? _bitmap;

    private int _width;

    #endregion

    #region Constructors

    public ComicCompPadItem() : this(string.Empty, null) { }

    public ComicCompPadItem(string keyName, Bitmap? bitmap) : base(keyName) {
        BeginInit();

        try {
            _bitmap = bitmap;
            _width = 100;
            P1 = new PointM(this, "Punkt1", 0, 0);
            P2 = new PointM(this, "Punkt2", 0, 0);
            PointsForSuccessfullyMove.Add(P1);
            PointsForSuccessfullyMove.Add(P2);
            MovablePoint.Add(P1);
            MovablePoint.Add(P2);
            _bitmap = null;
            CalculateJointMiddle(P1, P2);
            JointPoints.CollectionChanged += JointPoints_CollectionChanged;
            JointMiddle.Moved += JointMiddle_Moved;
            ImageChanged();
        } finally { EndInit(); }
    }

    #endregion

    #region Properties

    public static string ClassId => "COMIC";

    public Bitmap? Bitmap {
        get => _bitmap;
        set {
            _bitmap = value;
            ImageChanged();
        }
    }

    public override string Description => string.Empty;

    /// <summary>
    /// Diese Punkte sind Verbindungspunkte.
    /// Sie können an sich verschoben werden, aber dessen CanvasPosition ist immer in Relation zum JointMiddle.
    /// Deswegen verursacht ein Verschieben auch nur eine Relations-Änderung.
    /// Zusätzlich werden diese Punkte auf Bewegungen getrackt und auch gespeichert.
    /// </summary>
    public ObservableCollection<PointM> JointPoints { get; } = [];

    /// <summary>
    /// Haupt Gelenkpunkt 1
    /// </summary>
    public PointM P1 { get; }

    /// <summary>
    /// Haupt Gelenkpunkt 2
    /// </summary>
    public PointM P2 { get; }

    public int Width {
        get => _width;
        set {
            if (_width == value) { return; }
            _width = value;
            OnPropertyChanged();
        }
    }

    protected override int SaveOrder => 999;

    private bool ShowJointPoints {
        get {
            if (Parent is ItemCollectionPadItem { IsDisposed: false } icpi) { return icpi.ShowJointPoints; }
            return false;
        }
    }

    #endregion

    #region Methods

    public void AddJointPointAbsolute(string name, float x, float y) {
        var p = new PointM(name, x, y);
        p.Distance = GetLength(JointMiddle, p);
        p.Angle = GetAngle(JointMiddle, p) - GetAngle(P1, P2);
        p.Parent = this;
        JointPoints.Add(p);
    }

    public override bool CanvasContains(PointF value, float zoom) {
        var ne = 6.ControlToCanvas(zoom) + 1;

        if (value.DistanzZuStrecke(P1, P2) < ne) { return true; }
        foreach (var thispoint in PointsForSuccessfullyMove) {
            if (GetLength(value, (PointF)thispoint) < ne) { return true; }
        }
        return false;
    }

    public void DoJointPoint(PointM p) {
        if (JointPoints.Contains(p)) {
            p.Distance = GetLength(JointMiddle, p);
            p.Angle = GetAngle(JointMiddle, p) - GetAngle(P1, P2);
        }
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result = [
            new FlexiControlForProperty<int>(() => Width),
                .. base.GetProperties(widthOfControl),
        ];
        result.Add(new FlexiControlForDelegate(Verbindungspunkt_hinzu, "Verbindungspunkt hinzu", ImageCode.PlusZeichen));
        return result;
    }

    public override IJsonParseable? GetSubItemByKey(string containerName, string key) {
        if (string.Equals(containerName, "JointPoints", StringComparison.OrdinalIgnoreCase)) {
            return JointPoints.GetByKey(key);
        }

        return base.GetSubItemByKey(containerName, key);
    }

    public Bitmap GetTransformedBitmap() { //USED: BZL
        var r = CanvasUsedArea;
        var bmp = new Bitmap((int)r.Width, (int)r.Height);
        using var gr = Graphics.FromImage(bmp);
        gr.Clear(Color.White);
        var p = new PointF[4];
        p[0] = (PointF)_ber_Lo;
        p[1] = (PointF)_ber_Ro;
        p[2] = (PointF)_ber_Lu;
        p[3] = (PointF)_ber_Ru;
        var minX = float.MaxValue;
        var minY = float.MaxValue;
        for (var z = 0; z <= 3; z++) {
            minX = Math.Min(p[z].X, minX);
            minY = Math.Min(p[z].Y, minY);
        }
        for (var z = 0; z <= 3; z++) {
            p[z].X -= minX;
            p[z].Y -= minY;
        }
        PointF[] destPara2 = [p[0], p[1], p[2]]; //LO,RO,RU
        if (_bitmap is not null) {
            gr.DrawImage(_bitmap, destPara2, new RectangleF(0, 0, _bitmap.Width, _bitmap.Height), GraphicsUnit.Pixel);
        }
        return bmp;
    }

    public override void InitialPosition(int x, int y, int width, int height) {
        P1.SetTo(x + width / 2, y, false);
        P2.SetTo(x + width / 2, y + height, false);
    }

    public override List<string> ParseableItems() => [];

    public override JsonObject ParseableJson() {
        var json = base.ParseableJson();
        json.SetArrayIfNotEmpty("jointpoints", JointPoints);
        return json;
    }

    public override void ParseJson(JsonObject json) {
        BeginInit();
        try {
            base.ParseJson(json);

            if (json["jointpoints"] is JsonArray jps) {
                foreach (var item in jps) {
                    if (item is not JsonObject jo) { continue; }
                    var jp = new PointM(this, string.Empty, 0f, 0f);
                    jp.ParseJson(jo);
                    JointPoints.Add(jp);
                }
            }
        } finally {
            EndInit();
        }
    }

    public override bool ParseThis(string key, string value) {
        Develop.DebugPrint_NichtImplementiert(true);
        return true;
    }

    public override void PointMoved(object? sender, MoveEventArgs e) {
        if (sender == P1 || sender == P2) {
            CalculateJointMiddle(P1, P2);
        }

        if (sender is PointM p) {
            if (e.ByMouse) { DoJointPoint(p); }
            OnPropertyChanged("JointPoint");
        }
    }

    public override string ReadableText() => "Bewegliches Element";

    public void SetCoordinates(Rectangle r) {
        _width = r.Width;
        P1.SetTo(r.PointOf(Alignment.Top_HorizontalCenter), false);
        P2.SetTo(r.PointOf(Alignment.Bottom_HorizontalCenter), false);
    }

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Verschieben, 16);

    public void Verbindungspunkt_hinzu() => AddJointPointAbsolute("Neuer Verbindungspunkt", JointMiddle.X, JointMiddle.Y);

    internal void ConnectJointPoint(PointM myPoint, PointM otherPoint) {
        if (!JointPoints.Contains(myPoint)) { return; }
        Move(otherPoint.X - myPoint.X, otherPoint.Y - myPoint.Y, false);
    }

    protected override RectangleF CalculateCanvasUsedArea() {
        //var wp12 = AngleOfMiddleLine();
        var angleOfMiddleLine = GetAngle(P1, JointMiddle);

        _ber_Lo.SetTo(P1, _width / 2f, angleOfMiddleLine - 90, false);
        _ber_Ro.SetTo(P1, _width / 2f, angleOfMiddleLine + 90, false);
        _ber_Lu.SetTo(P2, _width / 2f, angleOfMiddleLine - 90, false);
        _ber_Ru.SetTo(P2, _width / 2f, angleOfMiddleLine + 90, false);
        List<PointM> x =
        [
            P1,
            P2,
            _ber_Lo,
            _ber_Ro,
            _ber_Lu,
            _ber_Ru
        ];
        var x1 = float.MaxValue;
        var y1 = float.MaxValue;
        var x2 = float.MinValue;
        var y2 = float.MinValue;
        foreach (var thisPoint in x) {
            x1 = Math.Min(thisPoint.X, x1);
            y1 = Math.Min(thisPoint.Y, y1);
            x2 = Math.Max(thisPoint.X, x2);
            y2 = Math.Max(thisPoint.Y, y2);
        }
        return new RectangleF(x1, y1, x2 - x1, y2 - y1);
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            JointPoints.CollectionChanged -= JointPoints_CollectionChanged;
            JointMiddle.Moved -= JointMiddle_Moved;
            foreach (var p in JointPoints) { p.Dispose(); }
            JointPoints.Clear();
        }
        base.Dispose(disposing);
    }

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, float zoom, float offsetX, float offsetY, bool forPrinting) {
        var lOt = _ber_Lo.CanvasToControl(zoom, offsetX, offsetY);
        var rOt = _ber_Ro.CanvasToControl(zoom, offsetX, offsetY);
        var rUt = _ber_Ru.CanvasToControl(zoom, offsetX, offsetY);
        var lUt = _ber_Lu.CanvasToControl(zoom, offsetX, offsetY);
        PointF[] destPara2 = [lOt, rOt, lUt];
        if (_bitmap is not null) {
            gr.DrawImage(_bitmap, destPara2, new RectangleF(0, 0, _bitmap.Width, _bitmap.Height), GraphicsUnit.Pixel);
        }
        if (_bitmap is null || !forPrinting) {
            gr.DrawLine(ZoomPad.PenGray, lOt, rOt);
            gr.DrawLine(ZoomPad.PenGray, rOt, rUt);
            gr.DrawLine(ZoomPad.PenGray, rUt, lUt);
            gr.DrawLine(ZoomPad.PenGray, lUt, lOt);
            gr.DrawLine(ZoomPad.PenGray, P1.CanvasToControl(zoom, offsetX, offsetY), P2.CanvasToControl(zoom, offsetX, offsetY));
        }

        if (!forPrinting && ShowJointPoints) {
            DrawPoints(gr, JointPoints, zoom, offsetX, offsetY, Design.HandlePoint_Joint, States.Standard, true);
        }
    }

    private void ImageChanged() {
        P1.X = 0f;
        P1.Y = 0f;
        if (_bitmap is null) {
            P2.X = 100f;
            P2.Y = 100f;
        } else {
            P2.X = _bitmap.Width;
            P2.Y = _bitmap.Height;
        }
        OnPropertyChanged();
    }

    private void JointMiddle_Moved(object? sender, MoveEventArgs e) {
        if (JointPoints.Count == 0) { return; }

        var angle = GetAngle(P1, P2);
        foreach (var thispoint in JointPoints) {
            thispoint.SetTo(JointMiddle, thispoint.Distance, thispoint.Angle + angle, false);
        }
    }

    private void JointPoint_PropertyChangedExt(object? sender, JsonPathChangedEventArgs e) {
        if (sender is not PointM p) { return; }
        OnPropertyChangedExt($"JointPoints[{p.KeyName}].{e.RelativePath}", e.Partial);
    }

    /// <summary>
    /// Wird aufgerufen, wenn sich die Auflistung der Verbindungspunkte ändert.
    /// Sorgt dafür, dass neue Punkte auf Bewegungen getrackt werden.
    /// </summary>
    private void JointPoints_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        if (e.NewItems is not null) {
            foreach (var thisit in e.NewItems) {
                if (thisit is PointM p) {
                    p.Moved += PointMoved;
                    p.PropertyChangedExt += JointPoint_PropertyChangedExt;
                }
            }
        }

        if (e.OldItems is not null) {
            foreach (var thisit in e.OldItems) {
                if (thisit is PointM p) {
                    p.Moved -= PointMoved;
                    p.PropertyChangedExt -= JointPoint_PropertyChangedExt;
                }
            }
        }

        OnPropertyChanged("JointPoint");
    }

    #endregion
}