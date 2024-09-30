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
using BlueBasics.EventArgs;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using BlueControls.ItemCollectionPad.Abstract;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueDatabase;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Forms;
using static BlueBasics.Constants;
using static BlueBasics.Converter;
using static BlueBasics.Generic;
using MessageBox = BlueControls.Forms.MessageBox;

namespace BlueControls.ItemCollectionPad;

public sealed class ItemCollectionPadItem : RectanglePadItem, IDisposableExtended, IReadableTextWithKey, IParseable, ICanHaveVariables, IMirrorable, IMouseAndKeyHandle {

    #region Fields

    public const int Dpi = 300;

    public static List<AbstractPadItem>? PadItemTypes;

    /// <summary>
    /// Für automatische Generierungen, die zu schnell hintereinander kommen, ein Counter für den Dateinamen
    /// </summary>
    private readonly int _idCount;

    private string _caption = string.Empty;

    private float _gridShow = 10;

    private float _gridsnap = 1;

    private PointM? _prLo;

    private PointM? _prLu;

    private PointM? _prRo;

    private PointM? _prRu;

    private Padding _randinMm = Padding.Empty;

    private SizeF _sheetSizeInMm = SizeF.Empty;

    private RowItem? _sheetStyle;

    private float _sheetStyleScale;

    private SnapMode _snapMode = SnapMode.SnapToGrid;

    #endregion

    #region Constructors

    public ItemCollectionPadItem() : base(string.Empty) {
        BindingOperations.EnableCollectionSynchronization(Items, new object());

        PadItemTypes ??= GetInstaceOfType<AbstractPadItem>("NAME");

        if (Skin.StyleDb == null) { Skin.InitStyles(); }
        SheetSizeInMm = Size.Empty;
        RandinMm = Padding.Empty;
        _idCount++;
        //Caption = "#" + DateTime.UtcNow.ToString1() + _idCount; // # ist die erkennung, dass es kein Dateiname sondern ein Item ist
        if (Skin.StyleDb == null) { Skin.InitStyles(); }
        _sheetStyle = null;
        _sheetStyleScale = 1f;
        if (Skin.StyleDb != null) { _sheetStyle = Skin.StyleDb.Row.First(); }

        Connections.CollectionChanged += ConnectsTo_CollectionChanged;
    }

    public ItemCollectionPadItem(string layoutFileName) : this() {
        this.Parse(File.ReadAllText(layoutFileName, Win1252));
        IsSaved = true;
    }

    #endregion

    #region Destructors

    ~ItemCollectionPadItem() {
        Dispose(false);
    }

    #endregion

    #region Events

    public event EventHandler<ListEventArgs>? ItemAdded;

    public event EventHandler? ItemRemoved;

    public event EventHandler<ListEventArgs>? ItemRemoving;

    #endregion

    #region Properties

    public Color BackColor { get; set; } = Color.White;

    [DefaultValue(false)]
    public string Caption {
        get => _caption;
        set {
            if (_caption == value) { return; }
            _caption = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<ItemConnection> Connections { get; } = [];

    public override string Description => "Eine Sammlung von Anzeige-Objekten";

    /// <summary>
    /// in mm
    /// </summary>
    [DefaultValue(10.0)]
    public float GridShow {
        get => _gridShow;
        set {
            if (IsDisposed) { return; }
            if (Math.Abs(_gridShow - value) < DefaultTolerance) { return; }
            _gridShow = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// in mm
    /// </summary>
    [DefaultValue(10.0)]
    public float GridSnap {
        get => _gridsnap;
        set {
            if (IsDisposed) { return; }
            if (Math.Abs(_gridsnap - value) < DefaultTolerance) { return; }
            _gridsnap = value;
            OnPropertyChanged();
        }
    }

    [DefaultValue(true)]
    public bool IsSaved { get; set; }

    public ObservableCollection<AbstractPadItem> Items { get; } = new ObservableCollection<AbstractPadItem>();

    public override string MyClassId => "PadCollection";

    IMouseAndKeyHandle? IMouseAndKeyHandle.Parent { get; set; }

    public Padding RandinMm {
        get => _randinMm;
        set {
            _randinMm = new Padding(Math.Max(0, value.Left), Math.Max(0, value.Top), Math.Max(0, value.Right), Math.Max(0, value.Bottom));
            GenPoints();
        }
    }

    public SizeF SheetSizeInMm {
        get => _sheetSizeInMm;
        set {
            if (IsDisposed) { return; }
            if (Math.Abs(value.Width - _sheetSizeInMm.Width) < DefaultTolerance &&
                Math.Abs(value.Height - _sheetSizeInMm.Height) < DefaultTolerance) { return; }
            _sheetSizeInMm = new SizeF(value.Width, value.Height);
            GenPoints();
            OnPropertyChanged();
        }
    }

    public SizeF SheetSizeInPix {
        get {
            if (_sheetSizeInMm.Width < 0.01 || _sheetSizeInMm.Height < 0.01) {
                return SizeF.Empty;
            }

            return new(MmToPixel(_sheetSizeInMm.Width, Dpi), MmToPixel(_sheetSizeInMm.Height, Dpi));
        }
    }

    public RowItem? SheetStyle {
        get => _sheetStyle;
        set {
            if (IsDisposed) { return; }
            if (_sheetStyle == value) { return; }
            _sheetStyle = value;
            ApplyDesignToItems();
            OnPropertyChanged();
        }
    }

    [DefaultValue(1.0)]
    public float SheetStyleScale {
        get => _sheetStyleScale;
        set {
            if (IsDisposed) { return; }
            if (value < 0.1f) { value = 0.1f; }
            if (Math.Abs(_sheetStyleScale - value) < DefaultTolerance) { return; }
            _sheetStyleScale = value;
            ApplyDesignToItems();
            OnPropertyChanged();
        }
    }

    [DefaultValue(false)]
    public SnapMode SnapMode {
        get => _snapMode;
        set {
            if (IsDisposed) { return; }
            if (_snapMode == value) { return; }
            _snapMode = value;
            OnPropertyChanged();
        }
    }

    protected override int SaveOrder => 1000;

    #endregion

    #region Indexers

    public AbstractPadItem? this[string keyName] => Items.Get(keyName);

    public AbstractPadItem? this[int nr] => Items[nr];

    public List<AbstractPadItem> this[int x, int y] => this[new Point(x, y)];

    public List<AbstractPadItem> this[Point p] => Items.Where(thisItem => thisItem != null && thisItem.Contains(p, 1)).ToList();

    #endregion

    #region Methods

    /// <summary>
    /// Gibt den Versatz der Linken oben Ecke aller Objekte zurück, um mittig zu sein.
    /// </summary>
    /// <param name="maxBounds"></param>
    /// <param name="sizeOfPaintArea"></param>
    /// <param name="zoomToUse"></param>
    /// <returns></returns>
    public static Point CenterPos(RectangleF maxBounds, Size sizeOfPaintArea, float zoomToUse) {
        var w = sizeOfPaintArea.Width - (maxBounds.Width * zoomToUse);
        var h = sizeOfPaintArea.Height - (maxBounds.Height * zoomToUse);
        return new Point((int)w, (int)h);
    }

    public static List<RectangleF> ResizeControls(List<IAutosizable> its, float newWidth, float newHeight, float currentWidth, float currentHeight) {
        var scaleY = newHeight / currentHeight;
        var scaleX = newWidth / currentWidth;

        #region Alle Items an die neue gedachte Y-Position schieben (newY), neue bevorzugte Höhe berechnen (newH), und auch newX und newW

        List<float> newX = [];
        List<float> newW = [];
        List<float> newY = [];
        List<float> newH = [];
        foreach (var thisIt in its) {

            #region  newY

            newY.Add(thisIt.UsedArea.Y * scaleY);

            #endregion

            #region  newX

            newX.Add(thisIt.UsedArea.X * scaleX);

            #endregion

            #region  newH

            var nh = thisIt.UsedArea.Height * scaleY;

            if (thisIt.AutoSizeableHeight) {
                if (!thisIt.CanChangeHeightTo(nh)) {
                    nh = AutosizableExtension.MinHeigthCapAndBox;
                }
            } else {
                nh = thisIt.UsedArea.Height;
            }

            newH.Add(nh);

            #endregion

            #region  newW

            newW.Add(thisIt.UsedArea.Width * scaleX);

            #endregion
        }

        #endregion

        #region  Alle Items von unten nach oben auf Überlappungen (auch dem Rand) prüfen.

        // Alle prüfen

        for (var tocheck = its.Count - 1; tocheck >= 0; tocheck--) {
            var pos = PositioOf(tocheck);

            #region Unterer Rand

            if (pos.Bottom > newHeight) {
                newY[tocheck] = newHeight - pos.Height;
                pos = PositioOf(tocheck);
            }

            #endregion

            for (var coll = its.Count - 1; coll > tocheck; coll--) {
                var poscoll = PositioOf(coll);
                if (pos.IntersectsWith(poscoll)) {
                    newY[tocheck] = poscoll.Top - pos.Height;
                    pos = PositioOf(tocheck);
                }
            }
        }

        #endregion

        #region  Alle UNveränderlichen Items von oben nach unten auf Überlappungen (auch dem Rand) prüfen.

        // Und von oben nach unten muss sein, weil man ja oben bündig haben will
        // Wichtig, das CanScaleHeightTo nochmal geprüft wird.
        // Nur so kann festgestellt werden, ob es eigentlich veränerlich wäre, aber durch die Mini-Größe doch als unveränderlich gilt

        for (var tocheck = 0; tocheck < its.Count; tocheck++) {
            if (!its[tocheck].CanScaleHeightTo(scaleY)) {
                var pos = PositioOf(tocheck);

                #region Oberer Rand

                if (pos.Y < 0) {
                    newY[tocheck] = 0;
                    pos = PositioOf(tocheck);
                }

                #endregion

                for (var coll = 0; coll < tocheck; coll++) {
                    if (!its[tocheck].CanScaleHeightTo(scaleY)) {
                        var poscoll = PositioOf(coll);
                        if (pos.IntersectsWith(poscoll)) {
                            newY[tocheck] = poscoll.Top + poscoll.Height;
                            pos = PositioOf(tocheck);
                        }
                    }
                }
            }
        }

        #endregion

        #region Alle Items, den Abstand stutzen, wenn der vorgänger unveränderlich ist - nur bei ScaleY >1

        if (scaleY > 1) {
            for (var tocheck = 0; tocheck < its.Count; tocheck++) {
                if (!its[tocheck].CanScaleHeightTo(scaleY)) {
                    //var pos = PositioOf(tocheck);

                    for (var coll = tocheck + 1; coll < its.Count; coll++) {
                        //var poscoll = PositioOf(coll);

                        if (its[coll].UsedArea.Y >= its[tocheck].UsedArea.Bottom && its[coll].UsedArea.IntersectsVericalyWith(its[tocheck].UsedArea)) {
                            newY[coll] = newY[tocheck] + newH[tocheck] + its[coll].UsedArea.Top - its[tocheck].UsedArea.Bottom;
                            //pos = PositioOf(tocheck);
                        }
                    }
                }
            }
        }

        #endregion

        #region  Alle veränderlichen Items von oben nach unten auf Überlappungen (auch dem Rand) prüfen - nur den Y-Wert.

        for (var tocheck = 0; tocheck < its.Count; tocheck++) {
            if (its[tocheck].CanScaleHeightTo(scaleY)) {
                var pos = PositioOf(tocheck);

                #region Oberer Rand

                if (pos.Y < 0) {
                    newY[tocheck] = 0;
                    pos = PositioOf(tocheck);
                }

                #endregion

                for (var coll = 0; coll < tocheck; coll++) {
                    var poscoll = PositioOf(coll);
                    if (pos.IntersectsWith(poscoll)) {
                        newY[tocheck] = poscoll.Top + poscoll.Height;
                        pos = PositioOf(tocheck);
                    }
                }
            }
        }

        #endregion

        #region  Alle veränderlichen Items von oben nach unten auf Überlappungen (auch dem Rand) prüfen - nur den Height-Wert stutzen.

        for (var tocheck = 0; tocheck < its.Count; tocheck++) {
            if (its[tocheck].CanScaleHeightTo(scaleY)) {
                var pos = PositioOf(tocheck);

                #region  Unterer Rand

                if (pos.Bottom > newHeight) {
                    newH[tocheck] = newHeight - pos.Y;
                    pos = PositioOf(tocheck);
                }

                #endregion

                #region  Alle Items stimmen mit dem Y-Wert, also ALLE prüfen, NACH dem Item

                for (var coll = tocheck + 1; coll < its.Count; coll++) {
                    var poscoll = PositioOf(coll);
                    if (pos.IntersectsWith(poscoll)) {
                        newH[tocheck] = poscoll.Top - pos.Top;
                        pos = PositioOf(tocheck);
                    }
                }

                #endregion
            }
        }

        #endregion

        #region Feedback-Liste erstellen (p)

        var p = new List<RectangleF>();
        for (var ite = 0; ite < its.Count; ite++) {
            p.Add(PositioOf(ite));
        }

        #endregion

        return p;

        RectangleF PositioOf(int no) => new(newX[no], newY[no], newW[no], newH[no]);
    }

    public static List<(IAutosizable item, RectangleF newpos)> ResizeControls(ItemCollectionPadItem padData, float newWidthPixel, float newhHeightPixel, string mode) {

        #region Items und Daten in einer sortierene Liste ermitteln, die es betrifft (its)

        List<IAutosizable> its = [];

        foreach (var thisc in padData.Items) {
            if (thisc is IAutosizable aas && aas.IsVisibleForMe(mode, true) &&
                thisc.IsInDrawingArea(thisc.UsedArea, padData.SheetSizeInPix.ToSize())) {
                its.Add(aas);
            }
        }

        its.Sort((it1, it2) => it1.UsedArea.Y.CompareTo(it2.UsedArea.Y));

        #endregion

        var p = ResizeControls(its, newWidthPixel, newhHeightPixel, padData.SheetSizeInPix.Width, padData.SheetSizeInPix.Height);

        var erg = new List<(IAutosizable item, RectangleF newpos)>();

        for (var x = 0; x < its.Count; x++) {
            erg.Add((its[x], p[x]));
        }

        return erg;
    }

    public static PointF SliderValues(RectangleF bounds, float zoomToUse, Point topLeftPos) => new((float)((bounds.Left * zoomToUse) - (topLeftPos.X / 2d)),
            (float)((bounds.Top * zoomToUse) - (topLeftPos.Y / 2d)));

    public static float ZoomFitValue(RectangleF maxBounds, Size sizeOfPaintArea) {
        if (maxBounds.IsEmpty) { return 1f; }

        return Math.Min(sizeOfPaintArea.Width / maxBounds.Width,
            sizeOfPaintArea.Height / maxBounds.Height);
    }

    public new void Add(AbstractPadItem? item) {
        if (item == null) { Develop.DebugPrint(FehlerArt.Fehler, "Item ist null"); return; }
        if (Items.Contains(item)) { Develop.DebugPrint(FehlerArt.Fehler, "Bereits vorhanden!"); return; }
        if (this[item.KeyName] != null) { Develop.DebugPrint(FehlerArt.Warnung, "Name bereits vorhanden: " + item.KeyName); return; }

        if (string.IsNullOrEmpty(item.KeyName)) { Develop.DebugPrint(FehlerArt.Fehler, "Item ohne Namen!"); return; }

        Items.Add(item);
        item.Parent = this;

        IsSaved = false;
        OnItemAdded(item);
        item.AddedToCollection();
        item.PropertyChanged += Item_PropertyChanged;
        //item.CompareKeyChanged += Item_CompareKeyChangedChanged;
        //item.CheckedChanged += Item_CheckedChanged;
        //item.CompareKeyChanged += Item_CompareKeyChangedChanged;
    }

    public void Clear() {
        var l = new List<AbstractPadItem>(Items);

        foreach (var thisit in l) {
            Remove(thisit);
        }

        Items.Clear();
    }

    public bool DrawTo(Graphics gr, float scale, float shiftX, float shiftY, Size sizeOfParentControl, bool showinprintmode, bool showJointPoints, States state) {
        try {
            gr.PixelOffsetMode = PixelOffsetMode.None;

            #region Hintergrund und evtl. Zeichenbereich

            if (_prLo != null && _prRu != null) {
                if (BackColor.A > 0) {
                    var p = SheetSizeInPix;
                    var rLo2 = new PointM(0, 0).ZoomAndMove(scale, shiftX, shiftY);
                    var rRu2 = new PointM(p.Width, p.Height).ZoomAndMove(scale, shiftX, shiftY);
                    Rectangle rr2 = new((int)rLo2.X, (int)rLo2.Y, (int)(rRu2.X - rLo2.X), (int)(rRu2.Y - rLo2.Y));

                    gr.FillRectangle(new SolidBrush(BackColor), rr2);
                    if (!showinprintmode) { gr.DrawRectangle(ZoomPad.PenGray, rr2); }
                }

                var rLo = _prLo.ZoomAndMove(scale, shiftX, shiftY);
                var rRu = _prRu.ZoomAndMove(scale, shiftX, shiftY);
                Rectangle rr = new((int)rLo.X, (int)rLo.Y, (int)(rRu.X - rLo.X), (int)(rRu.Y - rLo.Y));
                if (!showinprintmode) { gr.DrawRectangle(ZoomPad.PenGray, rr); }
            } else {
                if (BackColor.A > 0) { gr.Clear(BackColor); }
            }

            #endregion

            #region Grid

            if (_gridShow > 0.1) {
                var po = new PointM(0, 0).ZoomAndMove(scale, shiftX, shiftY);

                var tmpgrid = _gridShow;

                while (MmToPixel(tmpgrid, Dpi) * scale < 5) { tmpgrid *= 2; }

                var p = new Pen(Color.FromArgb(10, 0, 0, 0));
                float ex = 0;

                do {
                    var mo = MmToPixel(ex * tmpgrid, Dpi) * scale;

                    gr.DrawLine(p, po.X + (int)mo, 0, po.X + (int)mo, sizeOfParentControl.Height);
                    gr.DrawLine(p, 0, po.Y + (int)mo, sizeOfParentControl.Width, po.Y + (int)mo);

                    if (ex > 0) {
                        // erste Linie nicht doppelt zeichnen
                        gr.DrawLine(p, po.X - (int)mo, 0, po.X - (int)mo, sizeOfParentControl.Height);
                        gr.DrawLine(p, 0, po.Y - (int)mo, sizeOfParentControl.Width, po.Y - (int)mo);
                    }

                    ex++;

                    if (po.X - mo < 0 &&
                        po.Y - mo < 0 &&
                        po.X + mo > sizeOfParentControl.Width &&
                        po.Y + mo > sizeOfParentControl.Height) {
                        break;
                    }
                } while (true);
            }

            #endregion

            #region Items selbst

            if (!DrawItems(gr, scale, shiftX, shiftY, sizeOfParentControl, showinprintmode, showJointPoints)) {
                return DrawTo(gr, scale, shiftX, shiftY, sizeOfParentControl, showinprintmode, showJointPoints, state);
            }

            #endregion
        } catch {
            Develop.CheckStackForOverflow();
            return DrawTo(gr, scale, shiftX, shiftY, sizeOfParentControl, showinprintmode, showJointPoints, state);
        }
        return true;
    }

    public void DrawTo(Bitmap? bmp, States state, float scale, float shiftX, float shiftY) {
        if (bmp == null) { return; }
        var gr = Graphics.FromImage(bmp);
        _ = DrawTo(gr, scale, shiftX, shiftY, bmp.Size, true, false, state);
        gr.Dispose();
    }

    public void EineEbeneNachHinten(AbstractPadItem bpi) {
        var i2 = Previous(bpi);
        if (i2 != null) {
            Swap(Items.IndexOf(bpi), Items.IndexOf(i2));
        }
    }

    public void EineEbeneNachVorne(AbstractPadItem bpi) {
        var i2 = Next(bpi);
        if (i2 != null) {
            Swap(Items.IndexOf(bpi), Items.IndexOf(i2));
        }
    }

    /// <summary>
    /// Prüft, ob das Formular sichtbare Elemente hat.
    /// Zeilenselectionen werden dabei ignoriert.
    /// </summary>
    /// <param name="page">Wird dieser Wert leer gelassen, wird das komplette Formular geprüft</param>
    /// <returns></returns>
    public bool HasVisibleItemsForMe(string mode) {
        if (Items == null || Items.Count == 0) { return false; }

        foreach (var thisItem in Items) {
            if (thisItem is ReciverControlPadItem { MustBeInDrawingArea: true } cspi) {
                if (cspi.IsVisibleForMe(mode, false)) { return true; }
            }
        }

        return false;
    }

    public List<AbstractPadItem> HotItems(MouseEventArgs e, float zoom, float shiftX, float shiftY) {
        throw new NotImplementedException();
    }

    public void InDenHintergrund(AbstractPadItem thisItem) {
        if (Items.IndexOf(thisItem) == 0) { return; }
        Remove(thisItem);
        Items.Insert(0, thisItem);
    }

    public void InDenVordergrund(AbstractPadItem thisItem) {
        if (Items.IndexOf(thisItem) == Items.Count - 1) { return; }
        Remove(thisItem);
        Add(thisItem);
    }

    public bool KeyUp(KeyEventArgs e, float zoom, float shiftX, float shiftY) {
        throw new NotImplementedException();
    }

    public override void Mirror(PointM? p, bool vertical, bool horizontal) {
        foreach (var thisItem in Items) {
            if (thisItem is IMirrorable m) { m.Mirror(p, vertical, horizontal); }
        }
    }

    public bool MouseDown(MouseEventArgs e, float zoom, float shiftX, float shiftY) {
        throw new NotImplementedException();
    }

    public bool MouseMove(MouseEventArgs e, float zoom, float shiftX, float shiftY) {
        throw new NotImplementedException();
    }

    public bool MouseUp(MouseEventArgs e, float zoom, float shiftX, float shiftY) {
        throw new NotImplementedException();
    }

    public void Move(float x, float y) {
        if (x == 0 && y == 0) { return; }

        foreach (var thisItem in Items) {
            thisItem.Move(x, y, false);
        }
    }

    public override void OnPropertyChanged() {
        IsSaved = false;
        base.OnPropertyChanged();
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }

        List<string> result = [.. base.ParseableItems()];

        result.ParseableAdd("Style", SheetStyle?.CellFirstString());

        result.ParseableAdd("BackColor", BackColor.ToArgb());
        if (Math.Abs(SheetStyleScale - 1) > 0.001d) {
            result.ParseableAdd("FontScale", SheetStyleScale);
        }

        if (SheetSizeInMm is { Width: > 0, Height: > 0 }) {
            result.ParseableAdd("SheetSize", SheetSizeInMm);
            result.ParseableAdd("PrintArea", RandinMm.ToString());
        }

        result.ParseableAdd("Item", Items as IStringable);

        result.ParseableAdd("SnapMode", _snapMode);
        result.ParseableAdd("GridShow", _gridShow);
        result.ParseableAdd("GridSnap", _gridsnap);

        foreach (var thisCon in Connections) {
            if (thisCon?.Item1 != null) {
                result.ParseableAdd("Connection", thisCon.ToString());
            }
        }

        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key.ToLowerInvariant()) {
            case "sheetsize":
                _sheetSizeInMm = value.SizeFParse();
                GenPoints();
                return true;

            case "printarea":
                _randinMm = value.PaddingParse();
                GenPoints();
                return true;

            case "caption":
                _caption = value.FromNonCritical();
                return true;

            case "backcolor":
                BackColor = Color.FromArgb(IntParse(value));
                return true;

            case "style":
                _sheetStyle = Skin.StyleDb?.Row[value];
                _sheetStyle ??= Skin.StyleDb?.Row.First();// Einfach die Erste nehmen
                return true;

            case "fontscale":
                _sheetStyleScale = FloatParse(value);
                return true;

            case "snapmode":
                _snapMode = (SnapMode)IntParse(value);
                return true;

            case "gridshow":
                _gridShow = FloatParse(value);
                return true;

            case "gridsnap":
                _gridsnap = FloatParse(value);
                return true;

            case "items":
                CreateItems(value); // im ersten Step nur die items erzeugen....
                ParseItems(value);// ...im zweiten können nun auch Beziehungen erstellt werden!
                return true;

            case "connection":
                CreateConnection(value);
                return true;

            case "dpi":
                if (IntParse(value) != Dpi) {
                    Develop.DebugPrint("Dpi Unterschied: " + Dpi + " <> " + value);
                }
                return true;

            case "sheetstyle":
                if (Skin.StyleDb == null) { Skin.InitStyles(); }
                _sheetStyle = Skin.StyleDb?.Row[value];
                return true;

            case "sheetstylescale":
                _sheetStyleScale = FloatParse(value);
                return true;
        }

        return base.ParseThis(key, value);
    }

    public override string ReadableText() => _caption;

    public void Remove(string keyName) => Remove(this[keyName]);

    public void Remove(AbstractPadItem? item) {
        if (IsDisposed) { return; }
        if (item == null || !Items.Contains(item)) { return; }
        item.PropertyChanged -= Item_PropertyChanged;
        OnItemRemoving(item);
        _ = Items.Remove(item);
        OnItemRemoved();
        OnPropertyChanged();
    }

    public void RemoveRange(List<AbstractPadItem> remove) {
        foreach (var thisItem in remove) {
            Remove(thisItem);
        }
    }

    public bool ReplaceVariable(string name, string wert) => ReplaceVariable(new VariableString(name, wert));

    public bool ReplaceVariable(Variable variable) {
        var did = false;
        foreach (var thisItem in Items) {
            if (thisItem is ICanHaveVariables variables) {
                if (variables.ReplaceVariable(variable)) { did = true; }
            }
        }
        return did;
    }

    public ScriptEndedFeedback ReplaceVariables(RowItem? row) {
        if (row is not { IsDisposed: false }) { return new ScriptEndedFeedback("Keine Zeile angekommen", false, false, "Export"); }

        var script = row.ExecuteScript(ScriptEventTypes.export, string.Empty, true, 0, null, true, false);
        if (!script.AllOk) { return script; }

        this.ParseVariables(script.Variables);

        return script;
    }

    public ScriptEndedFeedback ReplaceVariables(Database database, string rowkey) => ReplaceVariables(database.Row.SearchByKey(rowkey));

    public bool ResetVariables() {
        if (IsDisposed) { return false; }
        var did = false;
        foreach (var thisItem in Items) {
            if (thisItem is ICanHaveVariables variables) {
                if (variables.ResetVariables()) { did = true; }
            }
        }
        if (did) { OnPropertyChanged(); }
        return did;
    }

    public void SaveAsBitmap(string filename) {
        var i = ToBitmap(1);
        if (i == null) { return; }

        switch (filename.FileSuffix().ToUpperInvariant()) {
            case "JPG":
            case "JPEG":
                i.Save(filename, ImageFormat.Jpeg);
                break;

            case "PNG":
                i.Save(filename, ImageFormat.Png);
                break;

            case "BMP":
                i.Save(filename, ImageFormat.Bmp);
                break;

            default:
                MessageBox.Show("Dateiformat unbekannt: " + filename.FileSuffix().ToUpperInvariant(), ImageCode.Warnung, "OK");
                return;
        }
    }

    public void Swap(int index1, int index2) {
        if (IsDisposed) { return; }
        if (index1 == index2) { return; }
        (Items[index1], Items[index2]) = (Items[index2], Items[index1]);
        OnPropertyChanged();
    }

    public override QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Register);

    public Bitmap? ToBitmap(float scale) {
        var r = MaxBounds();
        if (r.Width == 0) { return null; }

        CollectGarbage();

        do {
            if ((int)(r.Width * scale) > 15000) {
                scale *= 0.8f;
            } else if ((int)(r.Height * scale) > 15000) {
                scale *= 0.8f;
            } else if ((int)(r.Height * scale) * (int)(r.Height * scale) > 90000000) {
                scale *= 0.8f;
            } else {
                break;
            }
        } while (true);

        Bitmap I = new((int)(r.Width * scale), (int)(r.Height * scale));

        using var gr = Graphics.FromImage(I);
        gr.Clear(BackColor);
        if (!DrawTo(gr, scale, r.Left * scale, r.Top * scale, I.Size, true, false, States.Standard)) {
            return ToBitmap(scale);
        }

        return I;
    }

    public List<string> VisibleFor_AllUsed() {
        var l = new List<string>();

        foreach (var thisIt in Items) {
            if (thisIt is ReciverControlPadItem csi) {
                l.AddRange(csi.VisibleFor);
            }
        }

        l.Add(Administrator);
        l.Add(Everybody);
        l.Add("#User: " + UserName);

        l.AddRange(Table.Permission_AllUsed(false));

        return Database.RepairUserGroups(l);
    }

    /// <summary>
    /// Enthält Names keine Eintrag (Count =0) , werden alle Punkte gelöscht
    /// </summary>
    /// <param name="names"></param>
    internal void DeleteJointPoints(List<string> names) {
        foreach (var thisItem in Items) {
            thisItem.DeleteJointPoints(names);
        }
    }

    internal Rectangle DruckbereichRect() =>
                    _prLo == null || _prRu == null ? Rectangle.Empty :
                                                 new Rectangle((int)_prLo.X, (int)_prLo.Y, (int)(_prRu.X - _prLo.X), (int)(_prRu.Y - _prLo.Y));

    internal ScriptEndedFeedback? ExecuteScript(string scripttext, string mode, RowItem rowIn) {
        //var generatedentityID = rowIn.ReplaceVariables(entitiId, true, null);
        var vars = rowIn.Database?.CreateVariableCollection(rowIn, true, false, true, false) ?? new VariableCollection();

        //var vars = new VariableCollection();
        vars.Add(new VariableString("Application", Develop.AppName(), true, "Der Name der App, die gerade geöffnet ist."));
        vars.Add(new VariableString("User", Generic.UserName, true, "ACHTUNG: Keinesfalls dürfen benutzerabhängig Werte verändert werden."));
        vars.Add(new VariableString("Usergroup", Generic.UserGroup, true, "ACHTUNG: Keinesfalls dürfen gruppenabhängig Werte verändert werden."));
        //vars.Add(new VariableListString("Menu", null, false, "Diese Variable muss das Rückgabemenü enthalten."));
        //vars.Add(new VariableListString("Infos", null, false, "Diese Variable kann Zusatzinfos zum Menu enthalten."));
        //vars.Add(new VariableListString("CurrentlySelected", selected, true, "Was der Benutzer aktuell angeklickt hat."));
        //vars.Add(new VariableString("EntityId", generatedentityID, true, "Dies ist die Eingangsvariable."));
        vars.Add(new VariableString("Mode", mode, true, "In welchem Modus die Formulare angezeigt werden."));

        vars.Add(new VariableItemCollectionPad("Pad", this, true, "Auf diesem Objekt wird gezeichnet"));

        var m = BlueScript.Methods.Method.GetMethods(MethodType.Standard | MethodType.Database | MethodType.MyDatabaseRow | MethodType.Math | MethodType.DrawOnBitmap | MethodType.ManipulatesUser);

        var scp = new ScriptProperties("CreativePad-Generator", m, true, [], rowIn, 0);

        var sc = new BlueScript.Script(vars, scp);
        sc.ScriptText = scripttext;

        var t = sc.Parse(0, "Main", null);

        if (!t.Successful || !t.AllOk) {
            var ep = new BitmapPadItem(string.Empty, QuickImage.Get(ImageCode.Kritisch, 64), new Size(500, 500));

            this.Add(ep);
        }
        return t;
    }

    internal int GetFreeColorId() {
        var usedids = new List<int>();

        foreach (var thisIt in Items) {
            if (thisIt is ReciverSenderControlPadItem hci) {
                usedids.Add(hci.OutputColorId);
            }
        }

        for (var c = 0; c < 9999; c++) {
            if (!usedids.Contains(c)) { return c; }
        }
        return -1;
    }

    internal PointM? GetJointPoint(string pointName, AbstractPadItem? notOfMe) {
        foreach (var thisIt in Items) {
            if (thisIt != notOfMe) {
                foreach (var thisPt in thisIt.JointPoints) {
                    if (string.Equals(thisPt.KeyName, pointName, StringComparison.OrdinalIgnoreCase)) { return thisPt; }
                }
            }
        }

        return null;
    }

    internal RectangleF MaxBounds() {
        var r = MaximumBounds();
        if (SheetSizeInMm is { Width: > 0, Height: > 0 }) {
            var x1 = Math.Min(r.Left, 0);
            var y1 = Math.Min(r.Top, 0);
            var x2 = Math.Max(r.Right, MmToPixel(SheetSizeInMm.Width, Dpi));
            var y2 = Math.Max(r.Bottom, MmToPixel(SheetSizeInMm.Height, Dpi));
            return new RectangleF(x1, y1, x2 - x1, y2 - y1);
        }
        return r;
    }

    internal AbstractPadItem? Next(AbstractPadItem bpi) {
        var itemCount = Items.IndexOf(bpi);
        if (itemCount < 0) { Develop.DebugPrint(FehlerArt.Fehler, "Item im SortDefinition nicht enthalten"); }
        do {
            itemCount++;
            if (itemCount >= Items.Count) { return null; }
            if (Items[itemCount] != null) { return Items[itemCount]; }
        } while (true);
    }

    internal AbstractPadItem? Previous(AbstractPadItem bpi) {
        var itemCount = Items.IndexOf(bpi);
        if (itemCount < 0) { Develop.DebugPrint(FehlerArt.Fehler, "Item im SortDefinition nicht enthalten"); }
        do {
            itemCount--;
            if (itemCount < 0) { return null; }
            if (Items[itemCount] != null) { return Items[itemCount]; }
        } while (true);
    }

    internal void Resize(float newWidthPixel, float newhHeightPixel, bool changeControls, string mode) {
        if (Items == null || Items.Count == 0) { return; }

        if (changeControls) {
            var x = ResizeControls(this, newWidthPixel, newhHeightPixel, mode);

            #region Die neue Position in die Items schreiben

            foreach (var (item, newpos) in x) {
                item.SetCoordinates(newpos, true);
            }

            #endregion
        }

        SheetSizeInMm = new SizeF(PixelToMm(newWidthPixel, Dpi), PixelToMm(newhHeightPixel, Dpi));
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        foreach (var thisIt in Items) {
            thisIt.PropertyChanged -= Item_PropertyChanged;
            thisIt.Dispose();
        }

        if (disposing) {
            _sheetStyle = null;
        }

        Connections.CollectionChanged -= ConnectsTo_CollectionChanged;
        Connections.RemoveAll();
    }

    private void ApplyDesignToItems() {
        if (IsDisposed) { return; }
        foreach (var thisItem in Items) {
            thisItem?.ProcessStyleChange();
        }
        OnPropertyChanged();
    }

    private void ConnectsTo_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
        if (e.NewItems != null) {
            foreach (var thisit in e.NewItems) {
                if (thisit is ItemConnection x) {
                    x.Item2.PropertyChanged += Item_PropertyChanged;
                }
            }
        }

        if (e.OldItems != null) {
            foreach (var thisit in e.OldItems) {
                if (thisit is ItemConnection x) {
                    x.Item2.PropertyChanged -= Item_PropertyChanged;
                }
            }
        }

        if (e.Action == NotifyCollectionChangedAction.Reset) {
            Develop.DebugPrint_NichtImplementiert(true);
        }

        if (IsDisposed) { return; }
        OnPropertyChanged();
    }

    private void CreateConnection(string toParse) {
        if (toParse.StartsWith("[I]")) { toParse = toParse.FromNonCritical(); }

        var x = toParse.GetAllTags();

        AbstractPadItem? item1 = null;
        AbstractPadItem? item2 = null;
        var arrow1 = false;
        var arrow2 = false;
        var con1 = ConnectionType.Auto;
        var con2 = ConnectionType.Auto;
        var pm = true;

        foreach (var thisIt in x) {
            switch (thisIt.Key) {
                case "item1":
                    item1 = this[thisIt.Value.FromNonCritical()];
                    break;

                case "item2":
                    item2 = this[thisIt.Value.FromNonCritical()];
                    break;

                case "arrow1":
                    arrow1 = thisIt.Value.FromPlusMinus();
                    break;

                case "arrow2":
                    arrow2 = thisIt.Value.FromPlusMinus();
                    break;

                case "type1":
                    con1 = (ConnectionType)IntParse(thisIt.Value);
                    break;

                case "type2":
                    con2 = (ConnectionType)IntParse(thisIt.Value);
                    break;

                case "print":
                    pm = thisIt.Value.FromPlusMinus();
                    break;
            }
        }
        if (item1 == null || item2 == null) { return; }
        Connections.Add(new ItemConnection(item1, con1, arrow1, item2, con2, arrow2, pm));
    }

    private void CreateItems(string toParse) {
        foreach (var pair in toParse.GetAllTags()) {
            switch (pair.Key.ToLowerInvariant()) {
                case "dpi":
                case "sheetstylescale":
                case "sheetstyle":
                    break;

                case "item":
                    var i = ParsebleItem.NewByParsing<AbstractPadItem>(pair.Value);
                    if (i != null) { Add(i); }
                    break;

                default:
                    Develop.DebugPrint(FehlerArt.Warnung, "Tag unbekannt: " + pair.Key);
                    break;
            }
        }
    }

    private bool DrawItems(Graphics gr, float zoom, float shiftX, float shiftY, Size sizeOfParentControl, bool forPrinting, bool showJointPoints) {
        try {
            if (SheetStyleScale < 0.1d) { return true; }

            foreach (var thisItem in Items) {
                gr.PixelOffsetMode = PixelOffsetMode.None;
                thisItem.Draw(gr, zoom, shiftX, shiftY, sizeOfParentControl, forPrinting, showJointPoints);
            }
            return true;
        } catch {
            CollectGarbage();
            return false;
        }
    }

    private void GenPoints() {
        if (Math.Abs(_sheetSizeInMm.Width) < DefaultTolerance || Math.Abs(_sheetSizeInMm.Height) < DefaultTolerance) {
            if (_prLo != null) {
                _prLo.Parent = null;
                _prLo = null;
            }
            if (_prRo != null) {
                _prRo.Parent = null;
                _prRo = null;
            }
            if (_prRu != null) {
                _prRu.Parent = null;
                _prRu = null;
            }
            if (_prLu != null) {
                _prLu.Parent = null;
                _prLu = null;
            }
            return;
        }

        _prLo ??= new PointM(this, "Druckbereich LO", 0, 0);
        _prRo ??= new PointM(this, "Druckbereich RO", 0, 0);
        _prRu ??= new PointM(this, "Druckbereich RU", 0, 0);
        _prLu ??= new PointM(this, "Druckbereich LU", 0, 0);

        var ssw = (float)Math.Round(MmToPixel(_sheetSizeInMm.Width, Dpi), 1, MidpointRounding.AwayFromZero);
        var ssh = (float)Math.Round(MmToPixel(_sheetSizeInMm.Height, Dpi), 1, MidpointRounding.AwayFromZero);
        var rr = (float)Math.Round(MmToPixel(_randinMm.Right, Dpi), 1, MidpointRounding.AwayFromZero);
        var rl = (float)Math.Round(MmToPixel(_randinMm.Left, Dpi), 1, MidpointRounding.AwayFromZero);
        var ro = (float)Math.Round(MmToPixel(_randinMm.Top, Dpi), 1, MidpointRounding.AwayFromZero);
        var ru = (float)Math.Round(MmToPixel(_randinMm.Bottom, Dpi), 1, MidpointRounding.AwayFromZero);
        _prLo.SetTo(rl, ro, false);
        _prRo.SetTo(ssw - rr, ro, false);
        _prRu.SetTo(ssw - rr, ssh - ru, false);
        _prLu.SetTo(rl, ssh - ru, false);
    }

    private void Item_PropertyChanged(object sender, System.EventArgs e) => OnPropertyChanged();

    private RectangleF MaximumBounds() {
        var x1 = float.MaxValue;
        var y1 = float.MaxValue;
        var x2 = float.MinValue;
        var y2 = float.MinValue;
        var done = false;
        foreach (var thisItem in Items) {
            if (thisItem != null) {
                var ua = thisItem.ZoomToArea();
                x1 = Math.Min(x1, ua.Left);
                y1 = Math.Min(y1, ua.Top);
                x2 = Math.Max(x2, ua.Right);
                y2 = Math.Max(y2, ua.Bottom);
                done = true;
            }
        }
        return !done ? RectangleF.Empty : new RectangleF(x1, y1, x2 - x1, y2 - y1);
    }

    private void OnItemAdded(AbstractPadItem item) {
        if (IsDisposed) { return; }
        ItemAdded?.Invoke(this, new ListEventArgs(item));
        OnPropertyChanged();
    }

    private void OnItemRemoved() {
        ItemRemoved?.Invoke(this, System.EventArgs.Empty);
        if (IsDisposed) { return; }
        OnPropertyChanged();
    }

    private void OnItemRemoving(AbstractPadItem item) => ItemRemoving?.Invoke(this, new ListEventArgs(item));

    private void ParseConnections(string toParse) {
        if (toParse.StartsWith("[I]")) { toParse = toParse.FromNonCritical(); }

        foreach (var pair in toParse.GetAllTags()) {
            switch (pair.Key.ToLowerInvariant()) {
                case "connection":
                    CreateConnection(pair.Value);

                    break;

                default:
                    Develop.DebugPrint(FehlerArt.Warnung, "Tag unbekannt: " + pair.Key);
                    break;
            }
        }
    }

    private void ParseItems(string toParse) {
        foreach (var pair in toParse.GetAllTags()) {
            switch (pair.Key.ToLowerInvariant()) {
                case "dpi":
                case "sheetstyle":
                case "sheetstylescale":
                    break;

                case "item":
                    var t = pair.Value;
                    if (t.StartsWith("[I]")) { t = t.FromNonCritical(); }
                    var x = t.GetAllTags();
                    foreach (var thisIt in x) {
                        switch (thisIt.Key) {
                            case "internalname":
                            case "keyname":
                                var it = this[thisIt.Value];
                                it?.Parse(t);
                                break;
                        }
                    }
                    break;

                default:
                    Develop.DebugPrint(FehlerArt.Warnung, "Tag unbekannt: " + pair.Key);
                    break;
            }
        }
    }

    #endregion
}