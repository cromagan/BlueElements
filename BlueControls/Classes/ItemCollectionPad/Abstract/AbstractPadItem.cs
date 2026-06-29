// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueControls.EventArgs;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using static BlueBasics.ClassesStatic.Geometry;

namespace BlueControls.Classes.ItemCollectionPad.Abstract;

public abstract class AbstractPadItem : ParseableItem, IReadableTextWithKey, IMoveable, IComparable, ISimpleEditor, IJsonParseable {

    #region Fields

    public static readonly AssemblyAwareCache<AbstractPadItem> AllPadItems = new();

    public static readonly HatchBrush RedStripesBrush = new HatchBrush(HatchStyle.BackwardDiagonal, Color.FromArgb(200, 255, 0, 0), Color.Transparent);

    private static readonly Pen TinyItemPen = new(Color.Gray, 3);

    /// <summary>
    /// Soll es gedruckt werden?
    /// </summary>
    /// <remarks></remarks>
    private bool _beiExportSichtbar = true;

    private RectangleF _canvasUsedArea;
    private bool _enabled = true;

    /// <summary>
    /// Dieser Punkt muss zur Mittenbrechnung (JointMiddle) benutzt werden!
    /// Aus _jointReference und _jointMiddle wird die Mitte des Objekts berechnet
    /// </summary>
    private PointM? _jointReferenceFirst;

    /// <summary>
    /// Dieser Punkt muss zur Mittenbrechnung (JointMiddle) benutzt werden!
    /// Aus _jointReference und _jointMiddle wird die Mitte des Objekts berechnet
    /// </summary>
    private PointM? _jointReferenceSecond;

    #endregion

    #region Constructors

    protected AbstractPadItem(string keyName) : base() {
        KeyName = keyName;
        if (string.IsNullOrEmpty(KeyName)) { KeyName = GetUniqueKey(); }

        JointMiddle = new PointM(nameof(JointMiddle), 0, 0);
        JointMiddle.Moved += JointMiddle_Moved;

        MovablePoint.CollectionChanged += Point_CollectionChanged;
        JointPoints.CollectionChanged += Point_CollectionChanged;
    }

    #endregion

    #region Events

    public event EventHandler? DoUpdateSideOptionMenu;

    public event EventHandler? ParentChanged;

    public event EventHandler? ParentChanging;

    #endregion

    #region Properties

    [Description("Wird bei einem Export (wie z. B. Drucken) nur angezeigt, wenn das Häkchen gesetzt ist.")]
    public bool Bei_Export_sichtbar {
        get => _beiExportSichtbar;
        set {
            if (IsDisposed) { return; }
            if (_beiExportSichtbar == value) { return; }
            _beiExportSichtbar = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gibt die aktuellen Koordinaten des Objektes zurück. Unabhängig von der aktuellen Ansicht.
    /// Nicht berücksichtigt werden z.b. Verbindungslinien zu anderen Objekten
    /// </summary>
    /// <remarks></remarks>
    public RectangleF CanvasUsedArea {
        get {
            if (_canvasUsedArea.IsEmpty) {
                _canvasUsedArea = CalculateCanvasUsedArea();
                //if (_canvasUsedArea.Width < 1 || _canvasUsedArea.Height < 1) {
                //    throw Develop.DebugError("Ungültige Abmaße!");
                //}
            }
            return _canvasUsedArea;
        }
    }

    public abstract string Description { get; }

    [Description("Gibt an, ob das Element interaktiv ist (auswählbar, verschiebbar, Kontextmenü).")]
    public bool Enabled {
        get => _enabled;
        set {
            if (IsDisposed) { return; }
            if (_enabled == value) { return; }
            _enabled = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Dieser Punkt stammt aus der Mittenbrechnung mittles _jointReference.
    /// Aus _jointReference und _jointMiddle wird die Mitte des Objekts berechnet
    /// </summary>
    public PointM JointMiddle { get; }

    /// <summary>
    /// Diese Punket sind Verbindungspunkte.
    /// Sie können an sich verschoben werden, aber dessen CanvasPosition ist immer in Relation zum JointParentPoint.
    /// Deswegen verursacht ein Verschoeben auch nur eine Relations-Änderung.
    /// Zusätzlich werden diese Punkt auf Bewegungen getrackt und auch bei ToString gespeichert
    /// </summary>
    public ObservableCollection<PointM> JointPoints { get; } = [];

    public string KeyName {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Diese Punkte können vom Benutzer verschoben werden.
    /// Zusätzlich werden diese Punkt auf Bewegungen getrackt und auch bei ToString gespeichert
    /// </summary>
    public ObservableCollection<PointM> MovablePoint { get; } = [];

    public virtual bool MoveXByMouse => true;
    public virtual bool MoveYByMouse => true;
    public string Page { get; set; } = string.Empty;

    // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    // ~AbstractPadItem()
    // {
    //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
    //     Dispose(disposing: false);
    // }
    public object? Parent {
        get;
        set {
            if (value == field) { return; }

            OnParentChanging();
            field = value;
            OnParentChanged();
        }
    }

    /// <summary>
    /// Diese Punkte müssen gleichzeitig bewegt werden,
    /// wenn das ganze Objekt verschoben werden muss.
    /// </summary>
    public List<PointM> PointsForSuccessfullyMove { get; } = [];

    public virtual string QuickInfo { get; set; } = string.Empty;

    public bool ShowAlways {
        get {
            if (Parent is ItemCollectionPadItem { IsDisposed: false } icpi) { return icpi.ShowAlways; }
            return this is ItemCollectionPadItem { IsDisposed: false, ShowAlways: true }; // Wichtig, wegen NEW!
        }
    }

    public bool ShowJointPoints {
        get {
            if (Parent is ItemCollectionPadItem { IsDisposed: false } icpi) { return icpi.ShowJointPoints; }
            return this is ItemCollectionPadItem { IsDisposed: false, ShowJointPoints: true }; // Wichtig, wegen NEW!
        }
    }

    public List<string> Tags { get; } = [];
    protected abstract int SaveOrder { get; }

    #endregion

    #region Methods

    public static void DrawPoints(Graphics gr, ObservableCollection<PointM> points, float zoom, float offsetX, float offsetY, Design design, States state, bool showName) {
        foreach (var p in points) {
            p.Draw(gr, zoom, offsetX, offsetY, design, state);

            if (showName) {
                var t = p.CanvasToControl(zoom, offsetX, offsetY);
                var r = new Rectangle(t.X + 5, t.Y + 0, 200, 200);
                Skin.Draw_FormatedText(gr, p.KeyName, null, Alignment.Top_Left, r, design, state, null, false, false);
            }
        }
    }

    public static bool IsInDrawingArea(RectangleF drawingKoordinates, Rectangle visibleArea) => visibleArea.IsEmpty || drawingKoordinates.IntersectsWith(visibleArea);

    public virtual void AddedToCollection(ItemCollectionPadItem parent) {
        if (IsDisposed) { return; }
        Parent = parent;
    }

    public void AddJointPointAbsolute(string name, float x, float y) {
        if (_jointReferenceFirst is null || _jointReferenceSecond is null) { return; }

        var p = new PointM(name, x, y);
        p.Distance = GetLength(JointMiddle, p);
        p.Angle = GetAngle(JointMiddle, p) - GetAngle(_jointReferenceFirst, _jointReferenceSecond);
        p.Parent = this;
        JointPoints.Add(p);
    }

    /// <summary>
    /// Prüft, ob die angegebenen Koordinaten das Element berühren.
    /// Der Zoomfaktor wird nur benötigt, um Maßstabsunabhängige Punkt oder Linienberührungen zu berechnen.
    /// </summary>
    /// <remarks></remarks>
    public virtual bool CanvasContains(PointF value, float zoom) {
        var tmp = CanvasUsedArea; // Umwandlung, um den Bezug zur Klasse zu zerstören

        var ne = 6.ControlToCanvas(zoom) + 1;
        tmp.Inflate(ne, ne);
        return tmp.Contains(value);
    }

    public int CompareTo(object? obj) {
        if (obj is AbstractPadItem v) {
            return SaveOrder.CompareTo(v.SaveOrder);
        }

        Develop.DebugError("Falscher Objecttyp!");
        return 0;
    }

    public void DoJointPoint(PointM p) {
        if (_jointReferenceFirst is not null && _jointReferenceSecond is not null) {
            if (JointPoints.Contains(p)) {
                p.Distance = GetLength(JointMiddle, p);
                p.Angle = GetAngle(JointMiddle, p) - GetAngle(_jointReferenceFirst, _jointReferenceSecond);
            }
        }
    }

    public void Draw(Graphics gr, Rectangle visibleAreaControl, float zoom, float offsetX, float offsetY, bool forPrinting) {
        if (forPrinting && !_beiExportSichtbar && !ShowAlways || zoom < 0.00001) { return; }

        var positionControl = CanvasUsedArea.CanvasToControl(zoom, offsetX, offsetY, false);

        // Linien genau Waagerecht oder Senkrecht
        //if (positionControl.Width < 1 || positionControl.Height < 1) { return; }

        if (ShowAlways || IsInDrawingArea(positionControl, visibleAreaControl)) {
            DrawExplicit(gr, visibleAreaControl, positionControl, zoom, offsetX, offsetY, forPrinting);

            if (!forPrinting) {
                if (ShowJointPoints) {
                    DrawPoints(gr, JointPoints, zoom, offsetX, offsetY, Design.HandlePoint_Joint, States.Standard, true);
                }

                if (zoom > 1) {
                    var bp = BorderDraw.GetPen(Color.Gray, zoom);
                    lock (bp) { gr.DrawRectangle(bp, positionControl); }
                } else {
                    gr.DrawRectangle(ZoomPad.PenGray, positionControl);
                }

                if (positionControl is { Width: < 1, Height: < 1 }) {
                    gr.DrawEllipse(TinyItemPen, positionControl.Left - 5, positionControl.Top - 5, 10, 10);
                    gr.DrawLine(ZoomPad.PenGray, positionControl.PointOf(Alignment.Top_Left), positionControl.PointOf(Alignment.Bottom_Right));
                }

                if (!_beiExportSichtbar) {
                    var q = QuickImage.Get("Drucker|16||1");
                    gr.DrawImageUnscaled(q, positionControl.X, positionControl.Y);
                }

                if (this is IErrorCheckable iec) {
                    var r = iec.ErrorReason();

                    if (!string.IsNullOrEmpty(r)) {
                        gr.FillRectangle(RedStripesBrush, positionControl);
                        var q = QuickImage.Get("Kritisch|32||1");
                        gr.DrawImageUnscaled(q, positionControl.X, positionControl.Y);
                    }
                }
                //if (CreativePad.Highlight == this) { gr.DrawRectangle(new Pen(Color.Red, 5), positionControl); }
            }
        }
    }

    public void DrawToBitmap(Bitmap? bmp, float scale, float offsetX, float offsetY) {
        if (bmp is null) { return; }
        using var gr = Graphics.FromImage(bmp);
        var positionControl = CanvasUsedArea.CanvasToControl(scale, offsetX, offsetY, false);
        DrawExplicit(gr, new Rectangle(0, 0, bmp.Width, bmp.Height), positionControl, scale, offsetX, offsetY, true);
    }

    /// <summary>
    /// Gibt für das aktuelle Item das "Kontext-Menü" zurück.
    /// Alle Elemente für dieses Menü müssen neu erzeugt werden
    /// und werden bei nicht gebrauchen automatisch disposed
    /// </summary>
    /// <returns></returns>
    public virtual List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result =
        [
            new FlexiControl("Allgemein:", widthOfControl, true),

            new FlexiControlForProperty<bool>(() => Bei_Export_sichtbar)
        ];

        if (_jointReferenceFirst is not null && _jointReferenceSecond is not null) {
            result.Add(new FlexiControlForDelegate(Verbindungspunkt_hinzu, "Verbindungspunkt hinzu", ImageCode.PlusZeichen));

            //new FlexiControl("Verbindungspunkte:", widthOfControl, true),
            //d
        }

        return result;
    }

    /// <summary>
    /// Wird für den Editor benötigt, um bei hinzufügen es für den Benutzer mittig zu Plazieren
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public abstract void InitialPosition(int x, int y, int width, int height);

    public void Move(float x, float y, bool isMouse) {
        if (x == 0 && y == 0) { return; }
        foreach (var t in PointsForSuccessfullyMove) {
            t.Move(x, y, isMouse);
        }
        // JointPoint werden bewegt, wenn der JointParentPoint angesprochen wird

        OnPropertyChanged();
    }

    //}
    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("Key", KeyName);
        result.ParseableAdd("Enabled", _enabled);
        result.ParseableAdd("Print", _beiExportSichtbar);
        result.ParseableAdd("QuickInfo", QuickInfo);
        //result.ParseableAdd("ZoomPadding", _zoomPadding);

        foreach (var thisPoint in MovablePoint) {
            result.ParseableAdd("Point", thisPoint as IStringable);
        }
        foreach (var thisPoint in JointPoints) {
            result.ParseableAdd("JointPoint", thisPoint as IStringable);
        }

        result.ParseableAdd("Tags", Tags, false);

        return result;
    }

    /// <summary>
    /// Implementiert <see cref="IJsonStringable.ParseableJson" />. Subklassen
    /// überschreiben diese Methode, rufen <c>base.ParseableJson()</c> auf und
    /// ergänzen ihre eigenen Keys.
    /// </summary>
    public virtual JsonObject ParseableJson() {
        var json = new JsonObject();
        json["type"] = MyClassId;
        json["key"] = KeyName;
        json["enabled"] = _enabled;
        json["print"] = _beiExportSichtbar;
        json["quickInfo"] = QuickInfo;
        json["page"] = Page;

        json.SetArrayIfNotEmpty("points", MovablePoint);
        json.SetArrayIfNotEmpty("jointPoints", JointPoints);
        json.SetArrayIfNotEmpty("tags", Tags);

        return json;
    }

    public virtual void ParseFinishedJson(JsonElement parsed) { }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "type":
            case "classid": // Wurde bereits abgefragt, dadurch ist erst die Routine aufgerufen worden
                return value.ToNonCritical() == MyClassId;

            case "enabled":
                _enabled = value.FromPlusMinus();
                return true;

            case "checked":
                return true;

            case "tag":
                //Tags.Add(value.FromNonCritical());
                return true;

            case "print":
                _beiExportSichtbar = value.FromPlusMinus();
                return true;

            case "point":
                if (value.StartsWith("[I]", StringComparison.Ordinal)) { value = value.FromNonCritical(); }

                foreach (var thisPoint in MovablePoint) {
                    if (value.Contains("Name=" + thisPoint.KeyName + ",")) {
                        thisPoint.Parse(value);
                    }
                    if (value.Contains("Name=\"" + thisPoint.KeyName + "\",")) {
                        thisPoint.Parse(value);
                    }
                }

                return true;

            case "jointpoint":
                if (value.StartsWith("[I]", StringComparison.Ordinal)) { value = value.FromNonCritical(); }

                var p = new PointM(this, value);
                JointPoints.Add(p);

                return true;

            case "removetoo": // TODO: Alt, löschen, 02.03.2020
                //RemoveToo.AddRange(value.FromNonCritical().SplitAndCutByCr());
                return true;

            case "removetoogroup":// TODO: Alt, löschen, 30.09.2024
                return true;

            case "key":
            case "keyname":
            case "internalname":
                KeyName = value;
                return true;

            case "zoompadding":
                //_zoomPadding = IntParse(value);
                return true;

            case "quickinfo":
                QuickInfo = value.FromNonCritical();
                return true;

            case "page":
                Page = value.FromNonCritical();
                return true;

            case "tags":
                Tags.Clear();
                Tags.AddRange(value.SplitBy("|").FromNonCritical());
                return true;
        }

        return false;
    }

    /// <summary>
    /// Default-Implementation für <see cref="IJsonParseable" />. Subklassen
    /// überschreiben diese Methode, rufen <c>base.ParseThisJson</c> auf und
    /// ergänzen ihre eigenen Keys. So bleibt die Dispatch-Logik zentral.
    /// </summary>
    public virtual bool ParseThisJson(string key, JsonElement value) {
        switch (key) {
            case "type":
                // Klassenkennung - nur formal prüfen (wie bei ParseThis("classid"))
                return value.ValueKind == JsonValueKind.String;

            case "key":
                KeyName = value.ValueKind == JsonValueKind.String ? value.GetString() ?? string.Empty : string.Empty;
                return true;

            case "enabled":
                _enabled = value.ValueKind is JsonValueKind.True or JsonValueKind.False && value.GetBoolean();
                return true;

            case "print":
                _beiExportSichtbar = value.ValueKind is JsonValueKind.True or JsonValueKind.False && value.GetBoolean();
                return true;

            case "quickinfo":
                QuickInfo = value.ValueKind == JsonValueKind.String ? value.GetString() ?? string.Empty : string.Empty;
                return true;

            case "page":
                Page = value.ValueKind == JsonValueKind.String ? value.GetString() ?? string.Empty : string.Empty;
                return true;

            case "points":
                if (value.ValueKind == JsonValueKind.Array) {
                    foreach (var item in value.EnumerateArray()) {
                        if (item.ValueKind != JsonValueKind.Object) { continue; }
                        var name = item.GetString("name");
                        if (string.IsNullOrEmpty(name)) { continue; }

                        foreach (var thisPoint in MovablePoint) {
                            if (string.Equals(thisPoint.KeyName, name, StringComparison.Ordinal)) {
                                thisPoint.ParseJson(item);
                                break;
                            }
                        }
                    }
                }
                return true;

            case "jointpoints":
                if (value.ValueKind == JsonValueKind.Array) {
                    foreach (var item in value.EnumerateArray()) {
                        if (item.ValueKind != JsonValueKind.Object) { continue; }
                        var jp = new PointM(this, string.Empty, 0f, 0f);
                        jp.ParseJson(item);
                        JointPoints.Add(jp);
                    }
                }
                return true;

            case "tags":
                Tags.Clear();
                if (value.ValueKind == JsonValueKind.Array) {
                    foreach (var item in value.EnumerateArray()) {
                        if (item.ValueKind == JsonValueKind.String) { Tags.Add(item.GetString() ?? string.Empty); }
                    }
                }
                return true;
        }

        return false;
    }

    //    ParseFinished(parsestring);
    //}
    public virtual void PointMoved(object? sender, MoveEventArgs e) {
        if (sender is not PointM p) { return; }

        if (e.ByMouse) {
            DoJointPoint(p);
        }

        OnPropertyChanged("JointPoint");
    }

    public abstract string ReadableText();

    public abstract QuickImage? SymbolForReadableText();

    public Bitmap? ToBitmap(float scale) {
        var r = CanvasUsedArea;
        if (r.Width == 0) { return null; }

        CollectGarbage();

        do {
            if (r.Width.CanvasToControl(scale) > 15000) {
                scale *= 0.8f;
            } else if (r.Height.CanvasToControl(scale) > 15000) {
                scale *= 0.8f;
            } else if (r.Width.CanvasToControl(scale) * r.Height.CanvasToControl(scale) > 90000000) {
                scale *= 0.8f;
            } else {
                break;
            }
        } while (true);

        var bmp = new Bitmap(r.Width.CanvasToControl(scale), r.Height.CanvasToControl(scale));

        DrawToBitmap(bmp, scale, -r.Left.CanvasToControl(scale), -r.Top.CanvasToControl(scale));

        //using var gr = Graphics.FromImage(I);
        //gr.Clear(BackColor);
        //if (!DrawToBitmap(gr, r.Left.CanvasToControl(scale), r.Top.CanvasToControl(scale), I.CanvasSize, true, false, States.Standard)) {
        //    return ToBitmap(scale);
        //}

        return bmp;
    }

    public void Verbindungspunkt_hinzu() => AddJointPointAbsolute("Neuer Verbindungspunkt", JointMiddle.X, JointMiddle.Y);

    internal void ConnectJointPoint(PointM myPoint, PointM otherPoint) {
        if (!JointPoints.Contains(myPoint)) { return; }
        Move(otherPoint.X - myPoint.X, otherPoint.Y - myPoint.Y, false);
    }

    internal void ConnectJointPoint(AbstractPadItem itemToConnect, string pointnameInItem, string otherPointName, bool connectX, bool connectY) {
        var myPoint = itemToConnect.JointPoints.GetByKey(pointnameInItem);
        if (myPoint is null) { return; }

        if (itemToConnect.Parent is not ItemCollectionPadItem { IsDisposed: false } icpi) { return; }

        var otherPoint = icpi.GetJointPoint(otherPointName, itemToConnect);
        if (otherPoint is null) { return; }

        if (connectX && connectY) {
            Move(otherPoint.X - myPoint.X, otherPoint.Y - myPoint.Y, false);
        } else if (connectX) {
            Move(otherPoint.X - myPoint.X, 0, false);
        } else {
            Move(0, otherPoint.Y - myPoint.Y, false);
        }
    }

    /// <summary>
    /// Enthält Names keine Eintrag (Count =0) , werden alle Punkte gelöscht
    /// </summary>
    /// <param name="names"></param>
    internal void DeleteJointPoints(List<string> names) {
        var j = new List<PointM>();
        j.AddRange(JointPoints);

        foreach (var thispoint in j) {
            if (names.Count == 0 || names.Contains(thispoint.KeyName, StringComparer.OrdinalIgnoreCase)) {
                JointPoints.Remove(thispoint);
            }
        }
    }

    internal void GetNewIdsForEverything() => KeyName = GetUniqueKey();

    protected abstract RectangleF CalculateCanvasUsedArea();

    protected void CalculateJointMiddle(PointM firstPoint, PointM secondPoint) {
        _jointReferenceFirst ??= firstPoint;
        _jointReferenceSecond ??= secondPoint;

        if (firstPoint != _jointReferenceFirst) {
            Develop.DebugError("Refernz-Punkt falsch!");
            return;
        }

        if (_jointReferenceSecond != secondPoint) {
            Develop.DebugError("Refernz-Punkt falsch!");
            return;
        }

        JointMiddle.SetTo((firstPoint.X + secondPoint.X) / 2, (firstPoint.Y + secondPoint.Y) / 2, false);
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            DoUpdateSideOptionMenu = null;
            ParentChanged = null;
            ParentChanging = null;

            JointMiddle.Moved -= JointMiddle_Moved;
            MovablePoint.CollectionChanged -= Point_CollectionChanged;
            JointPoints.CollectionChanged -= Point_CollectionChanged;

            JointMiddle.Dispose();
            foreach (var p in MovablePoint) { p.Dispose(); }
            foreach (var p in JointPoints) { p.Dispose(); }
        }

        MovablePoint.RemoveAll();
        JointPoints.Clear();

        base.Dispose(disposing);
    }

    protected abstract void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, float zoom, float offsetX, float offsetY, bool forPrinting);

    protected void OnDoUpdateSideOptionMenu() => DoUpdateSideOptionMenu?.Invoke(this, System.EventArgs.Empty);

    protected virtual void OnParentChanged() => ParentChanged?.Invoke(this, System.EventArgs.Empty);

    protected virtual void OnParentChanging() => ParentChanging?.Invoke(this, System.EventArgs.Empty);

    /// <summary>
    /// Invalidiert CanvasUsedArea und löst das Ereignis Changed aus
    /// </summary>
    protected override void OnPropertyChanged([CallerMemberName] string propertyName = "unknown") {
        _canvasUsedArea = default;
        base.OnPropertyChanged(propertyName);
    }

    private void JointMiddle_Moved(object? sender, MoveEventArgs e) {
        if (_jointReferenceFirst is null || _jointReferenceSecond is null) { return; }

        if (JointPoints.Count > 0) {
            var angle = GetAngle(_jointReferenceFirst, _jointReferenceSecond);

            foreach (var thispoint in JointPoints) {
                thispoint.SetTo(JointMiddle, thispoint.Distance, thispoint.Angle + angle, false);
            }
        }
    }

    private void Point_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        if (e.NewItems is not null) {
            foreach (var thisit in e.NewItems) {
                if (thisit is PointM p) {
                    p.Moved += PointMoved;
                }
            }
        }

        if (e.OldItems is not null) {
            foreach (var thisit in e.OldItems) {
                if (thisit is PointM p) {
                    p.Moved -= PointMoved;
                }
            }
        }

        if (e.Action == NotifyCollectionChangedAction.Reset) {
            foreach (var thisit in JointPoints) {
                thisit.Moved -= PointMoved;
            }
        }

        OnPropertyChanged("JointPoint");
    }

    #endregion
}