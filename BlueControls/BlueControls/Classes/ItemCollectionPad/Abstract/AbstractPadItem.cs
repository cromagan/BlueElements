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
using System.Linq;
using static BlueBasics.Converter;

namespace BlueControls.ItemCollectionPad.Abstract;

public abstract class AbstractPadItem : ParsebleItem, IParseable, ICloneable, IPropertyChangedFeedback, IMoveable, IDisposableExtended, IComparable, IHasKeyName, ISimpleEditor {

    #region Fields

    public static readonly BlueFont? ColumnFont = Skin.GetBlueFont(Design.Table_Column, States.Standard);

    /// <summary>
    /// Soll es gedruckt werden?
    /// </summary>
    /// <remarks></remarks>
    private bool _beiExportSichtbar = true;

    private string _page = string.Empty;

    private ItemCollectionPad? _parent;

    private PadStyles _style = PadStyles.Style_Standard;

    private RectangleF _usedArea;

    private int _zoomPadding;

    #endregion

    #region Constructors

    protected AbstractPadItem(string keyName) : base(keyName) => MovablePoint.CollectionChanged += MovablePoint_CollectionChanged;

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

    /// <summary>
    /// Wird ein Element gelöscht, das diese Feld befüllt hat, werden automatisch alle andern Elemente mit der selben Gruppe gelöscht.
    /// </summary>
    [Description("Alle Elemente, die der selben Gruppe angehören, werden beim Löschen eines Elements ebenfalls gelöscht.")]
    public string Gruppenzugehörigkeit { get; set; } = string.Empty;

    public bool IsDisposed { get; private set; }
    public ObservableCollection<PointM> MovablePoint { get; } = [];

    [Description("Ist Page befüllt, wird das Item nur angezeigt, wenn die anzuzeigende Seite mit dem String übereinstimmt.")]
    public string Page {
        get => _page;
        set {
            if (_page == value) { return; }
            _page = value;
            OnPropertyChanged();
        }
    }

    // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    // ~AbstractPadItem()
    // {
    //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
    //     Dispose(disposing: false);
    // }
    public ItemCollectionPad? Parent {
        get => _parent;
        set {
            if (_parent == null || _parent == value) {
                _parent = value;
                //OnParentChanged();
                return;
            }

            Develop.DebugPrint(FehlerArt.Fehler, "Parent Fehler!");
        }
    }

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
    /// Gibt den Bereich zurück, den das Element benötigt, um komplett dargestellt zu werden. Unabhängig von der aktuellen Ansicht.
    /// nicht berücksichtigt werden z.b. Verbindungslinien zu anderen Objekten
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

    public virtual void AddedToCollection() { }

    public object? Clone() {
        var x = ToParseableString();

        var i = NewByParsing<AbstractPadItem>(x);
        i?.Parse(x);

        return i;
    }

    //    if (ItemCollectionPad.PadItemTypes != null) {
    //        foreach (var thisType in ItemCollectionPad.PadItemTypes) {
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

    //            case "keyName":
    //                name = thisIt.Value;
    //                break;
    //        }
    //    }
    //    if (string.IsNullOrEmpty(ding)) {
    //        Develop.DebugPrint(FehlerArt.Fehler, "Itemtyp unbekannt: " + code);
    //        return null;
    //    }
    //    if (string.IsNullOrEmpty(name)) {
    //        Develop.DebugPrint(FehlerArt.Fehler, "Itemname unbekannt: " + code);
    //        return null;
    //    }
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

    //    foreach (var thisIt in x) {
    //        switch (thisIt.Key) {
    //            case "type":
    //            case "classid":
    //                ding = thisIt.Value;
    //                break;
    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    //    var x = code.GetAllTags();
    public void Draw(Graphics gr, float zoom, float shiftX, float shiftY, Size sizeOfParentControl, bool forPrinting) {
        if (_parent == null) {
            Develop.DebugPrint(FehlerArt.Fehler, "Parent nicht definiert");
            return;
        }

        if (forPrinting && !_beiExportSichtbar) { return; }

        var positionModified = UsedArea.ZoomAndMoveRect(zoom, shiftX, shiftY, false);

        if (IsInDrawingArea(positionModified, sizeOfParentControl)) {
            DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
        }

        #region Verknüpfte Pfeile Zeichnen

        if (!forPrinting) {
            var line = 1f;
            if (zoom > 1) { line = zoom; }

            foreach (var thisV in _parent.Connections) {
                if (thisV.Item1 == this && thisV.Bei_Export_sichtbar) {
                    if (_parent.Contains(thisV.Item2) && thisV.Item2 != this) {
                        if (thisV.Item2.Bei_Export_sichtbar) {
                            var t1 = ItemConnection.GetConnectionPoint(this, thisV.Item1Type, thisV.Item2).ZoomAndMove(zoom, shiftX, shiftY);
                            var t2 = ItemConnection.GetConnectionPoint(thisV.Item2, thisV.Item2Type, this).ZoomAndMove(zoom, shiftX, shiftY);

                            if (Geometry.GetLenght(t1, t2) > 1) {
                                gr.DrawLine(new Pen(Color.Gray, line), t1, t2);
                                var wi = Geometry.Winkel(t1, t2);
                                if (thisV.ArrowOnItem1) { DimensionPadItem.DrawArrow(gr, t1, wi, Color.Gray, zoom * 20); }
                                if (thisV.ArrowOnItem2) { DimensionPadItem.DrawArrow(gr, t2, wi + 180, Color.Gray, zoom * 20); }
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
        [   new FlexiControlForProperty<string>(() => Gruppenzugehörigkeit),
            new FlexiControlForProperty<bool>(() => Bei_Export_sichtbar)
        ];
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

    public bool IsOnPage(string page) {
        if (string.IsNullOrEmpty(_page)) { return true; }

        return string.Equals(_page, page, StringComparison.OrdinalIgnoreCase);
    }

    public void Move(float x, float y) {
        if (x == 0 && y == 0) { return; }
        foreach (var t in PointsForSuccesfullyMove) {
            t.Move(x, y);
        }
        OnPropertyChanged();
    }

    /// <summary>
    /// Invalidiert UsedArea und löst das Ereignis Changed aus
    /// </summary>
    public override void OnPropertyChanged() {
        _usedArea = default;
        base.OnPropertyChanged();
    }

    //public void Parse(List<KeyValuePair<string, string>> toParse, string parsestring) {
    //    foreach (var pair in toParse) {
    //        if (!ParseThis(pair.Key, pair.Value)) {
    //            Develop.DebugPrint(FehlerArt.Warnung, "Kann nicht geparsed werden: " + pair.Key + "/" + pair.Value + "/" + toParse);
    //        }
    //    }

    //    ParseFinished(parsestring);
    //}

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "classid": // Wurde bereits abgefragt, dadurch st erst die Routine aufgerufen worden
            case "type":
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

            case "format": // = Textformat!!!
            case "design":
            case "style":
                _style = (PadStyles)IntParse(value);
                return true;

            case "removetoo": // TODO: Alt, löschen, 02.03.2020
                //RemoveToo.AddRange(value.FromNonCritical().SplitAndCutByCr());
                return true;

            case "removetoogroup":
                Gruppenzugehörigkeit = value.FromNonCritical();
                return true;

            case "key":
            case "keyname":
            case "internalname":
                if (value != KeyName) {
                    Develop.DebugPrint(FehlerArt.Fehler, "Namen unterschiedlich: " + value + " / " + KeyName);
                }
                return true;

            case "zoompadding":
                _zoomPadding = IntParse(value);
                return true;

            case "quickinfo":
                QuickInfo = value.FromNonCritical();
                return true;

            case "page":
                _page = value.FromNonCritical();
                return true;

            case "tags":
                Tags.Clear();
                Tags.AddRange(value.SplitBy("|").ToList().FromNonCritical());
                return true;
        }

        return false;
    }

    public virtual void PointMoved(object sender, MoveEventArgs e) => OnPropertyChanged();

    public void PointMoving(object sender, MoveEventArgs e) { }

    /// <summary>
    /// Teilt dem Item mit, dass das Design geändert wurde.
    /// Es löst kein Ereigniss aus.
    /// </summary>
    public virtual void ProcessStyleChange() { }

    public override string ToParseableString() {
        if (IsDisposed) { return string.Empty; }
        List<string> result = [];

        result.ParseableAdd("Style", _style);
        result.ParseableAdd("Page", _page);
        result.ParseableAdd("Print", _beiExportSichtbar);
        result.ParseableAdd("QuickInfo", QuickInfo);
        result.ParseableAdd("ZoomPadding", _zoomPadding);

        foreach (var thisPoint in MovablePoint) {
            result.ParseableAdd("Point", thisPoint as IStringable);
        }
        if (!string.IsNullOrEmpty(Gruppenzugehörigkeit)) {
            result.ParseableAdd("RemoveTooGroup", Gruppenzugehörigkeit);
        }

        result.ParseableAdd("Tags", Tags, false);

        return result.Parseable(base.ToParseableString());
    }

    //public void UpdateSideOptionMenu() => OnDoUpdateSideOptionMenu();

    //public void RemoveAllConnections() {
    //    foreach (var thisCon in Parent.Connections) {
    //        if (thisCon.Item1 == this || thisCon.Item2 == this) {
    //            Parent.Connections.Remove(thisCon);
    //            RemoveAllConnections();
    //            return;
    //        }
    //    }
    //}
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

    protected abstract RectangleF CalculateUsedArea();

    protected virtual void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
            }

            MovablePoint.CollectionChanged -= MovablePoint_CollectionChanged;
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
            }

            if (!_beiExportSichtbar) {
                var q = QuickImage.Get("Drucker|16||1");
                gr.DrawImage(q, positionModified.X, positionModified.Y);
            }
        } catch { }
    }

    private void MovablePoint_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
        if (e.NewItems != null) {
            foreach (var thisit in e.NewItems) {
                if (thisit is PointM p) {
                    p.Moving += PointMoving;
                    p.Moved += PointMoved;
                }
            }
        }

        if (e.OldItems != null) {
            foreach (var thisit in e.OldItems) {
                if (thisit is PointM p) {
                    p.Moving -= PointMoving;
                    p.Moved -= PointMoved;
                }
            }
        }

        if (e.Action == NotifyCollectionChangedAction.Reset) {
            Develop.DebugPrint_NichtImplementiert(true);
        }
    }

    protected void OnDoUpdateSideOptionMenu() => DoUpdateSideOptionMenu?.Invoke(this, System.EventArgs.Empty);

    #endregion
}