// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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
using System.Runtime.CompilerServices;
using static BlueBasics.Generic;
using static BlueBasics.Geometry;

namespace BlueControls.ItemCollectionPad.Abstract;

public abstract class AbstractPadItem : ParseableItem, IReadableTextWithKey, IMoveable, IDisposableExtended, IComparable, ISimpleEditor {

    #region Fields

    /// <summary>
    /// Soll es gedruckt werden?
    /// </summary>
    /// <remarks></remarks>
    private bool _beiExportSichtbar = true;

    private RectangleF _canvasUsedArea;

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

    #endregion

    #region Constructors

    protected AbstractPadItem(string keyName) : base() {
        _keyName = keyName;
        if (string.IsNullOrEmpty(_keyName)) { _keyName = GetUniqueKey(); }

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

    public event PropertyChangedEventHandler? PropertyChanged;

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
            if (_canvasUsedArea.IsEmpty) { _canvasUsedArea = CalculateCanvasUsedArea(); }
            return _canvasUsedArea;
        }
    }

    public abstract string Description { get; }

    public bool ForPrinting {
        get {
            if (Parent is ItemCollectionPadItem { IsDisposed: false } icpi) { return icpi.ForPrinting; }
            if (this is ItemCollectionPadItem { IsDisposed: false } icip2) { return icip2.ForPrinting; } // Wichtig, wegen NEW!
            return true;
        }
    }

    public bool IsDisposed { get; private set; }

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

    public bool KeyIsCaseSensitive => false;

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
    public List<PointM> PointsForSuccesfullyMove { get; } = [];

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
        if (_jointReferenceFirst == null || _jointReferenceSecond == null) { return; }

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

    public int CompareTo(object obj) {
        if (obj is AbstractPadItem v) {
            return SaveOrder.CompareTo(v.SaveOrder);
        }

        Develop.DebugPrint(ErrorType.Error, "Falscher Objecttyp!");
        return 0;
    }

    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void DoJointPoint(PointM p) {
        if (_jointReferenceFirst != null && _jointReferenceSecond != null) {
            if (JointPoints.Contains(p)) {
                p.Distance = GetLength(JointMiddle, p);
                p.Angle = GetAngle(JointMiddle, p) - GetAngle(_jointReferenceFirst, _jointReferenceSecond);
            }
        }
    }

    public void Draw(Graphics gr, Rectangle visibleAreaControl, float zoom, float offsetX, float offsetY) {
        if (ForPrinting && !_beiExportSichtbar && !ShowAlways) { return; }

        var positionControl = CanvasUsedArea.CanvasToControl(zoom, offsetX, offsetY, false);

        if (ShowAlways || IsInDrawingArea(positionControl, visibleAreaControl)) {
            DrawExplicit(gr, visibleAreaControl, positionControl, zoom, offsetX, offsetY);

            if (!ForPrinting) {
                if (ShowJointPoints) {
                    DrawPoints(gr, JointPoints, zoom, offsetX, offsetY, Design.Button_EckpunktSchieber_Joint, States.Standard, true);
                }

                gr.DrawRectangle(zoom > 1 ? new Pen(Color.Gray, zoom) : ZoomPad.PenGray, positionControl);

                if (positionControl is { Width: < 1, Height: < 1 }) {
                    gr.DrawEllipse(new Pen(Color.Gray, 3), positionControl.Left - 5, positionControl.Top + 5, 10, 10);
                    gr.DrawLine(ZoomPad.PenGray, positionControl.PointOf(Alignment.Top_Left), positionControl.PointOf(Alignment.Bottom_Right));
                }

                if (!_beiExportSichtbar) {
                    var q = QuickImage.Get("Drucker|16||1");
                    gr.DrawImage(q, positionControl.X, positionControl.Y);
                }

                if (this is IErrorCheckable iec) {
                    var r = iec.ErrorReason();

                    if (!string.IsNullOrEmpty(r)) {
                        using var brush = new HatchBrush(HatchStyle.BackwardDiagonal, Color.FromArgb(200, 255, 0, 0), Color.Transparent);
                        gr.FillRectangle(brush, positionControl);
                        var q = QuickImage.Get("Kritisch|32||1");
                        gr.DrawImage(q, positionControl.X, positionControl.Y);
                    }
                }
                //if (CreativePad.Highlight == this) { gr.DrawRectangle(new Pen(Color.Red, 5), positionControl); }
            }
        }

        #region Verknüpfte Pfeile Zeichnen

        if (!ForPrinting) {
            var line = 1f;
            if (zoom > 1) { line = zoom; }

            if (Parent is ItemCollectionPadItem { IsDisposed: false } icpi) {
                foreach (var thisV in icpi.Connections) {
                    if (thisV.Item1 == this && thisV.Bei_Export_sichtbar) {
                        if (icpi.Contains(thisV.Item2) && thisV.Item2 != this) {
                            if (thisV.Item2.Bei_Export_sichtbar) {
                                var t1 = ItemConnection.GetConnectionPoint(this, thisV.Item1Type, thisV.Item2).CanvasToControl(zoom, offsetX, offsetY);
                                var t2 = ItemConnection.GetConnectionPoint(thisV.Item2, thisV.Item2Type, this).CanvasToControl(zoom, offsetX, offsetY);

                                if (GetLength(t1, t2) > 1) {
                                    gr.DrawLine(new Pen(Color.Gray, line), t1, t2);
                                    var wi = GetAngle(t1, t2);
                                    if (thisV.ArrowOnItem1) { DimensionPadItem.DrawArrow(gr, t1, wi, Color.Gray, 20.CanvasToControl(zoom)); }
                                    if (thisV.ArrowOnItem2) { DimensionPadItem.DrawArrow(gr, t2, wi + 180, Color.Gray, 20.CanvasToControl(zoom)); }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }

    public void DrawToBitmap(Bitmap? bmp, float scale, float offsetX, float offsetY) {
        if (bmp == null) { return; }
        var gr = Graphics.FromImage(bmp);
        var positionControl = CanvasUsedArea.CanvasToControl(scale, offsetX, offsetY, false);
        DrawExplicit(gr, new Rectangle(0, 0, bmp.Width, bmp.Height), positionControl, scale, offsetX, offsetY);
        gr.Dispose();
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
            result.Add(new FlexiDelegateControl(Verbindungspunkt_hinzu, "Verbindungspunkt hinzu", ImageCode.PlusZeichen));

            //new FlexiControl("Verbindungspunkte:", widthOfControl, true),
            //d
        }

        if (this is IMirrorable) {
            result.Add(new FlexiDelegateControl(Spiegeln_Vertikal, "Vertikal", ImageCode.SpiegelnVertikal));
            result.Add(new FlexiDelegateControl(Spiegeln_Horizontal, "Horizontal", ImageCode.SpiegelnHorizontal));
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
        foreach (var t in PointsForSuccesfullyMove) {
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
            } else if (r.Height.CanvasToControl(scale) * r.Height.CanvasToControl(scale) > 90000000) {
                scale *= 0.8f;
            } else {
                break;
            }
        } while (true);

        var bmp = new Bitmap(r.Width.CanvasToControl(scale), r.Height.CanvasToControl(scale));

        DrawToBitmap(bmp, scale, r.Left.CanvasToControl(scale), r.Top.CanvasToControl(scale));

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
        if (myPoint == null) { return; }

        if (itemToConnect.Parent is not ItemCollectionPadItem { IsDisposed: false } icpi) { return; }

        var otherPoint = icpi.GetJointPoint(otherPointName, itemToConnect);
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

    internal void GetNewIdsForEverything() => KeyName = GetUniqueKey();

    //foreach (var thispoint in JointPoints) {//    thispoint.KeyName = Generic.GetUniqueKey();//}//foreach (var thispoint in MovablePoint) {//    thispoint.KeyName = Generic.GetUniqueKey();//}////Doppelt gemoppelt//foreach (var thispoint in PointsForSuccesfullyMove) {//    thispoint.KeyName = Generic.GetUniqueKey();//}////Doppelt gemoppelt//_jointMiddle.KeyName = Generic.GetUniqueKey();//_jointReferenceFirst.KeyName =  Generic.GetUniqueKey();//_jointReferenceSecond.KeyName = Generic.GetUniqueKey();
    protected abstract RectangleF CalculateCanvasUsedArea();

    protected void CalculateJointMiddle(PointM firstPoint, PointM secondPoint) {
        _jointReferenceFirst ??= firstPoint;
        _jointReferenceSecond ??= secondPoint;

        if (firstPoint != _jointReferenceFirst) {
            Develop.DebugPrint(ErrorType.Error, "Refernz-Punkt falsch!");
            return;
        }

        if (_jointReferenceSecond != secondPoint) {
            Develop.DebugPrint(ErrorType.Error, "Refernz-Punkt falsch!");
            return;
        }

        JointMiddle.SetTo((firstPoint.X + secondPoint.X) / 2, (firstPoint.Y + secondPoint.Y) / 2, false);
    }

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

    protected abstract void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, float zoom, float offsetX, float offsetY);

    protected void OnDoUpdateSideOptionMenu() => DoUpdateSideOptionMenu?.Invoke(this, System.EventArgs.Empty);

    protected virtual void OnParentChanged() => ParentChanged?.Invoke(this, System.EventArgs.Empty);

    protected virtual void OnParentChanging() => ParentChanging?.Invoke(this, System.EventArgs.Empty);

    /// <summary>
    /// Invalidiert CanvasUsedArea und löst das Ereignis Changed aus
    /// </summary>
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "unknown") {
        _canvasUsedArea = default;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void JointMiddle_Moved(object sender, MoveEventArgs e) {
        if (_jointReferenceFirst == null || _jointReferenceSecond == null) { return; }

        if (JointPoints.Count > 0) {
            var angle = GetAngle(_jointReferenceFirst, _jointReferenceSecond);

            foreach (var thispoint in JointPoints) {
                thispoint.SetTo(JointMiddle, thispoint.Distance, thispoint.Angle + angle, false);
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

        OnPropertyChanged("JointPoint");
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