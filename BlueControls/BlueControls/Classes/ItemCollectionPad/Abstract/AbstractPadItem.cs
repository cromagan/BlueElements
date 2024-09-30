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
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using static BlueBasics.Converter;
using static BlueBasics.Geometry;

namespace BlueControls.ItemCollectionPad.Abstract;

public abstract class AbstractPadItem : ParsebleItem, IReadableTextWithKey, ICloneable, IMoveable, IDisposableExtended, IComparable, ISimpleEditor {

    #region Fields

    public static readonly BlueFont? ColumnFont = Skin.GetBlueFont(Design.Table_Column, States.Standard);

    public string Page = string.Empty;

    /// <summary>
    /// Soll es gedruckt werden?
    /// </summary>
    /// <remarks></remarks>
    private bool _beiExportSichtbar = true;

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

    private string _keyName;

    private ItemCollectionPadItem? _parent;

    private PadStyles _style = PadStyles.Style_Standard;

    private RectangleF _usedArea;

    private int _zoomPadding;

    #endregion

    #region Constructors

    protected AbstractPadItem(string keyName) : base() {
        _keyName = keyName;
        if (string.IsNullOrEmpty(_keyName)) { _keyName = Generic.GetUniqueKey(); }

        JointMiddle = new PointM("JointMiddle", 0, 0);
        JointMiddle.Moved += JointMiddle_Moved;

        MovablePoint.CollectionChanged += Point_CollectionChanged;
        JointPoints.CollectionChanged += Point_CollectionChanged;
    }

    #endregion

    #region Events

    public event EventHandler? DoUpdateSideOptionMenu;

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

    //public List<FlexiControl>? AdditionalStyleOptions { get; set; } = null;
    public abstract string Description { get; }

    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Dieser Punkt stammt aus der Mittenbrechnung mittles _jointReference.
    /// Aus _jointReference und _jointMiddle wird die Mitte des Objekts berechnet
    /// </summary>
    public PointM JointMiddle { get; private set; }

    /// <summary>
    /// Diese Punket sind Verbindungspunkte.
    /// Sie können an sich verschoben werden, aber dessen Position ist immer in Relation zum JointParentPoint.
    /// Deswegen verursacht ein Verschoeben auch nur eine Relations-Änderung.
    /// Zusätzlich werden diese Punkt auf Bewegungen getrackt und auch bei ToString gespeichert
    /// </summary>
    public ObservableCollection<PointM> JointPoints { get; } = [];

    public string KeyName {
        get => _keyName;
        set {
            if (_keyName == value) { return; }
            _keyName = value;
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

    // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    // ~AbstractPadItem()
    // {
    //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
    //     Dispose(disposing: false);
    // }
    public ItemCollectionPadItem? Parent {
        get => _parent;
        set {
            if (_parent == null || _parent == value) {
                _parent = value;
                //OnParentChanged();
                return;
            }

            if (value == null) {
                _parent = null;
                return;
            }

            Develop.DebugPrint(FehlerArt.Fehler, "Parent Fehler!");
        }
    }

    /// <summary>
    /// Diese Punkte müssen gleichzeitig bewegt werden,
    /// wenn das ganze Objekt verschoben werden muss.
    /// </summary>
    public List<PointM> PointsForSuccesfullyMove { get; } = [];

    public virtual string QuickInfo { get; set; } = string.Empty;

    public PadStyles Stil {
        get => _style;
        set {
            if (_style == value) { return; }
            _style = value;
            ProcessStyleChange();
            OnPropertyChanged();
        }
    }

    public List<string> Tags { get; } = [];

    /// <summary>
    /// Gibt die aktuellen Koordinaten des Objektes zurück. Unabhängig von der aktuellen Ansicht.
    /// Nicht berücksichtigt werden z.b. Verbindungslinien zu anderen Objekten
    /// </summary>
    /// <remarks></remarks>
    public RectangleF UsedArea {
        get {
            if (_usedArea.IsEmpty) { _usedArea = CalculateUsedArea(); }
            return _usedArea;
        }
    }

    public int ZoomPadding {
        get => _zoomPadding;
        set {
            if (_zoomPadding == value) { return; }
            _zoomPadding = value;
            OnPropertyChanged();
        }
    }

    protected abstract int SaveOrder { get; }

    #endregion

    #region Methods

    public static void DrawPoints(Graphics gr, ObservableCollection<PointM> points, float zoom, float shiftX, float shiftY, Design design, States state, bool showName) {
        foreach (var p in points) {
            p.Draw(gr, zoom, shiftX, shiftY, design, state);

            if (showName) {
                var t = p.ZoomAndMove(zoom, shiftX, shiftY);
                Rectangle r = new((int)(t.X + 5), (int)(t.Y + 0), 200, 200);
                Skin.Draw_FormatedText(gr, p.KeyName, design, state, null, Alignment.Top_Left, r, null, false, false);
            }
        }
    }

    public virtual void AddedToCollection() { }

    public void AddJointPointAbsolute(string name, float x, float y) {
        if (_jointReferenceFirst == null || _jointReferenceSecond == null) { return; }

        var p = new PointM(name, x, y);
        p.Distance = GetLenght(JointMiddle, p);
        p.Angle = GetAngle(JointMiddle, p) - GetAngle(_jointReferenceFirst, _jointReferenceSecond);
        p.Parent = this;
        JointPoints.Add(p);
    }

    public object? Clone() {
        var x = ParseableItems();
        var i = NewByParsing<AbstractPadItem>(x.FinishParseable());
        return i;
    }

    //    if (ItemCollectionPadItem.PadItemTypes != null) {
    //        foreach (var thisType in ItemCollectionPadItem.PadItemTypes) {
    //            var i = thisType.TryCreate(ding, name);
    //            if (i != null) { return i; }
    //        }
    //    }
    //    return null;
    //}
    public int CompareTo(object obj) {
        if (obj is AbstractPadItem v) {
            return SaveOrder.CompareTo(v.SaveOrder);
        }

        Develop.DebugPrint(FehlerArt.Fehler, "Falscher Objecttyp!");
        return 0;
    }

    /// <summary>
    /// Prüft, ob die angegebenen Koordinaten das Element berühren.
    /// Der Zoomfaktor wird nur benötigt, um Maßstabsunabhängige Punkt oder Linienberührungen zu berechnen.
    /// </summary>
    /// <remarks></remarks>
    public virtual bool Contains(PointF value, float zoomfactor) {
        var tmp = UsedArea; // Umwandlung, um den Bezug zur Klasse zu zerstören

        var ne = (6 / zoomfactor) + 1;
        tmp.Inflate(ne, ne);
        return tmp.Contains(value);
    }

    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void DoJointPoint(PointM p) {
        if (_jointReferenceFirst != null && _jointReferenceSecond != null) {
            if (JointPoints.Contains(p)) {
                p.Distance = GetLenght(JointMiddle, p);
                p.Angle = GetAngle(JointMiddle, p) - GetAngle(_jointReferenceFirst, _jointReferenceSecond);
            }
        }
    }

    public void Draw(Graphics gr, float zoom, float shiftX, float shiftY, Size sizeOfParentControl, bool forPrinting, bool showJointPoints) {
        if (forPrinting && !_beiExportSichtbar) { return; }

        var positionModified = UsedArea.ZoomAndMoveRect(zoom, shiftX, shiftY, false);

        if (IsInDrawingArea(positionModified, sizeOfParentControl)) {
            DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);

            if (showJointPoints) {
                DrawPoints(gr, JointPoints, zoom, shiftX, shiftY, Design.Button_EckpunktSchieber_Joint, States.Standard, true);
            }
        }

        #region Verknüpfte Pfeile Zeichnen

        if (!forPrinting) {
            var line = 1f;
            if (zoom > 1) { line = zoom; }

            if (Parent is { }) {
                foreach (var thisV in Parent.Connections) {
                    if (thisV.Item1 == this && thisV.Bei_Export_sichtbar) {
                        if (Parent.Items.Contains(thisV.Item2) && thisV.Item2 != this) {
                            if (thisV.Item2.Bei_Export_sichtbar) {
                                var t1 = ItemConnection.GetConnectionPoint(this, thisV.Item1Type, thisV.Item2).ZoomAndMove(zoom, shiftX, shiftY);
                                var t2 = ItemConnection.GetConnectionPoint(thisV.Item2, thisV.Item2Type, this).ZoomAndMove(zoom, shiftX, shiftY);

                                if (GetLenght(t1, t2) > 1) {
                                    gr.DrawLine(new Pen(Color.Gray, line), t1, t2);
                                    var wi = GetAngle(t1, t2);
                                    if (thisV.ArrowOnItem1) { DimensionPadItem.DrawArrow(gr, t1, wi, Color.Gray, zoom * 20); }
                                    if (thisV.ArrowOnItem2) { DimensionPadItem.DrawArrow(gr, t2, wi + 180, Color.Gray, zoom * 20); }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion
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

        if (_jointReferenceFirst != null && _jointReferenceSecond != null) {
            result.Add(new FlexiControlForDelegate(Verbindungspunkt_hinzu, "Verbindungspunkt hinzu", ImageCode.PlusZeichen));

            //new FlexiControl("Verbindungspunkte:", widthOfControl, true),
            //d
        }

        if (this is IMirrorable) {
            result.Add(new FlexiControlForDelegate(Spiegeln_Vertikal, "Vertikal", ImageCode.SpiegelnVertikal));
            result.Add(new FlexiControlForDelegate(Spiegeln_Horizontal, "Horizontal", ImageCode.SpiegelnHorizontal));
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

    public bool IsInDrawingArea(RectangleF drawingKoordinates, Size sizeOfParentControl) =>
        sizeOfParentControl.IsEmpty ||
        sizeOfParentControl.Width == 0 ||
        sizeOfParentControl.Height == 0 ||
        drawingKoordinates.IntersectsWith(new Rectangle(Point.Empty, sizeOfParentControl));

    public void Move(float x, float y, bool isMouse) {
        if (x == 0 && y == 0) { return; }
        foreach (var t in PointsForSuccesfullyMove) {
            t.Move(x, y, isMouse);
        }
        // JointPoint werden bewegt, wenn der JointParentPoint angesprochen wird

        OnPropertyChanged();
    }

    /// <summary>
    /// Invalidiert UsedArea und löst das Ereignis Changed aus
    /// </summary>
    public override void OnPropertyChanged() {
        _usedArea = default;
        base.OnPropertyChanged();
    }

    //}
    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("Key", KeyName);
        result.ParseableAdd("Style", _style);
        result.ParseableAdd("Print", _beiExportSichtbar);
        result.ParseableAdd("QuickInfo", QuickInfo);
        result.ParseableAdd("ZoomPadding", _zoomPadding);

        foreach (var thisPoint in MovablePoint) {
            result.ParseableAdd("Point", thisPoint as IStringable);
        }
        foreach (var thisPoint in JointPoints) {
            result.ParseableAdd("JointPoint", thisPoint as IStringable);
        }

        result.ParseableAdd("Tags", Tags, false);

        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "type":
            case "classid": // Wurde bereits abgefragt, dadurch ist erst die Routine aufgerufen worden
                return value.ToNonCritical() == MyClassId;

            case "enabled":
            case "checked":
                return true;

            case "tag":
                //Tags.Add(value.FromNonCritical());
                return true;

            case "print":
                _beiExportSichtbar = value.FromPlusMinus();
                return true;

            case "point":
                if (value.StartsWith("[I]")) { value = value.FromNonCritical(); }

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
                if (value.StartsWith("[I]")) { value = value.FromNonCritical(); }

                var p = new PointM(this, value);
                JointPoints.Add(p);

                return true;

            case "format": // = Textformat!!!
            case "design":
            case "style":
                _style = (PadStyles)IntParse(value);
                return true;

            case "removetoo": // TODO: Alt, löschen, 02.03.2020
                //RemoveToo.AddRange(value.FromNonCritical().SplitAndCutByCr());
                return true;

            case "removetoogroup":// TODO: Alt, löschen, 30.09.2024
                return true;

            case "key":
            case "keyname":
            case "internalname":
                _keyName = value;
                return true;

            case "zoompadding":
                _zoomPadding = IntParse(value);
                return true;

            case "quickinfo":
                QuickInfo = value.FromNonCritical();
                return true;

            case "page":
                Page = value.FromNonCritical();
                return true;

            case "tags":
                Tags.Clear();
                Tags.AddRange(value.SplitBy("|").ToList().FromNonCritical());
                return true;
        }

        return false;
    }

    //    ParseFinished(parsestring);
    //}
    public virtual void PointMoved(object sender, MoveEventArgs e) {
        if (sender is not PointM p) { return; }

        if (e.ByMouse) {
            DoJointPoint(p);
        }

        OnPropertyChanged();
    }

    /// <summary>
    /// Teilt dem Item mit, dass das Design geändert wurde.
    /// Es löst kein Ereigniss aus.
    /// </summary>
    public virtual void ProcessStyleChange() { }

    public abstract string ReadableText();

    public abstract QuickImage? SymbolForReadableText();

    public void Verbindungspunkt_hinzu() {
        AddJointPointAbsolute("Neuer Verbindungspunkt", JointMiddle.X, JointMiddle.Y);
    }

    /// <summary>
    /// Gibt den Bereich zurück, den das Element benötigt, um komplett dargestellt zu werden. Unabhängig von der aktuellen Ansicht. Zusätzlich mit dem Wert aus Padding.
    /// </summary>
    /// <remarks></remarks>
    public RectangleF ZoomToArea() {
        var x = UsedArea;
        if (_zoomPadding == 0) { return x; }
        x.Inflate(-ZoomPadding, -ZoomPadding);
        return x;
    }

    internal void ConnectJointPoint(PointM myPoint, PointM otherPoint) {
        if (!JointPoints.Contains(myPoint)) { return; }
        Move(otherPoint.X - myPoint.X, otherPoint.Y - myPoint.Y, false);
    }

    internal void ConnectJointPoint(AbstractPadItem itemToConnect, string pointnameInItem, string otherPointName, bool connectX, bool connectY) {
        var myPoint = itemToConnect.JointPoints.Get(pointnameInItem);
        if (myPoint == null) { return; }

        var otherPoint = itemToConnect.Parent?.GetJointPoint(otherPointName, itemToConnect);
        if (otherPoint == null) { return; }

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
            if (names.Count == 0 || names.Contains(thispoint.KeyName, false)) {
                JointPoints.Remove(thispoint);
            }
        }
    }

    internal void GetNewIdsForEverything() {
        KeyName = Generic.GetUniqueKey();

        //foreach (var thispoint in JointPoints) {
        //    thispoint.KeyName = Generic.GetUniqueKey();

        //}

        //foreach (var thispoint in MovablePoint) {
        //    thispoint.KeyName = Generic.GetUniqueKey();

        //}

        ////Doppelt gemoppelt
        //foreach (var thispoint in PointsForSuccesfullyMove) {
        //    thispoint.KeyName = Generic.GetUniqueKey();
        //}

        ////Doppelt gemoppelt
        //_jointMiddle.KeyName = Generic.GetUniqueKey();
        //_jointReferenceFirst.KeyName =  Generic.GetUniqueKey();
        //_jointReferenceSecond.KeyName = Generic.GetUniqueKey();
    }

    protected void CalculateJointMiddle(PointM firstPoint, PointM secondPoint) {
        if (_jointReferenceFirst == null) { _jointReferenceFirst = firstPoint; }
        if (_jointReferenceSecond == null) { _jointReferenceSecond = secondPoint; }

        if (firstPoint != _jointReferenceFirst) {
            Develop.DebugPrint(FehlerArt.Fehler, "Refernz-Punkt falsch!");
            return;
        }

        if (_jointReferenceSecond != secondPoint) {
            Develop.DebugPrint(FehlerArt.Fehler, "Refernz-Punkt falsch!");
            return;
        }

        JointMiddle.SetTo((firstPoint.X + secondPoint.X) / 2, (firstPoint.Y + secondPoint.Y) / 2, false);
    }

    protected abstract RectangleF CalculateUsedArea();

    protected virtual void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
            }

            JointMiddle.Moved -= JointMiddle_Moved;
            MovablePoint.CollectionChanged -= Point_CollectionChanged;
            JointPoints.CollectionChanged -= Point_CollectionChanged;
            MovablePoint.RemoveAll();

            // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
            // TODO: Große Felder auf NULL setzen
            IsDisposed = true;
        }
    }

    protected virtual void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        try {
            if (!forPrinting) {
                if (positionModified is { Width: > 1, Height: > 1 }) {
                    gr.DrawRectangle(zoom > 1 ? new Pen(Color.Gray, zoom) : ZoomPad.PenGray, positionModified);
                }
                if (positionModified is { Width: < 1, Height: < 1 }) {
                    gr.DrawEllipse(new Pen(Color.Gray, 3), positionModified.Left - 5, positionModified.Top + 5, 10, 10);
                    gr.DrawLine(ZoomPad.PenGray, positionModified.PointOf(Alignment.Top_Left), positionModified.PointOf(Alignment.Bottom_Right));
                }

                if (!_beiExportSichtbar) {
                    var q = QuickImage.Get("Drucker|16||1");
                    gr.DrawImage(q, positionModified.X, positionModified.Y);
                }

                if (this is IErrorCheckable iec) {
                    var r = iec.ErrorReason();

                    if (!string.IsNullOrEmpty(r)) {
                        using var brush = new HatchBrush(HatchStyle.BackwardDiagonal, Color.FromArgb(200, 255, 0, 0), Color.Transparent);
                        gr.FillRectangle(brush, positionModified);
                        var q = QuickImage.Get("Kritisch|32||1");
                        gr.DrawImage(q, positionModified.X, positionModified.Y);
                    }
                }
            }
        } catch { }
    }

    protected void OnDoUpdateSideOptionMenu() => DoUpdateSideOptionMenu?.Invoke(this, System.EventArgs.Empty);

    private void JointMiddle_Moved(object sender, MoveEventArgs e) {
        if (_jointReferenceFirst == null || _jointReferenceSecond == null) { return; }

        if (JointPoints.Count > 0) {
            var angle = GetAngle(_jointReferenceFirst, _jointReferenceSecond);

            foreach (var thisPoint in JointPoints) {
                foreach (var thispoint in JointPoints) {
                    thispoint.SetTo(JointMiddle, thispoint.Distance, thispoint.Angle + angle, false);
                }
            }
        }
    }

    private void Point_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
        if (e.NewItems != null) {
            foreach (var thisit in e.NewItems) {
                if (thisit is PointM p) {
                    p.Moved += PointMoved;
                }
            }
        }

        if (e.OldItems != null) {
            foreach (var thisit in e.OldItems) {
                if (thisit is PointM p) {
                    p.Moved -= PointMoved;
                }
            }
        }

        if (e.Action == NotifyCollectionChangedAction.Reset) {
            Develop.DebugPrint_NichtImplementiert(true);
        }

        OnPropertyChanged();
    }

    private void Spiegeln_Horizontal() {
        if (this is IMirrorable m) {
            m.Mirror(null, false, true);
        }
    }

    private void Spiegeln_Vertikal() {
        if (this is IMirrorable m) {
            m.Mirror(null, true, false);
        }
    }

    #endregion
}