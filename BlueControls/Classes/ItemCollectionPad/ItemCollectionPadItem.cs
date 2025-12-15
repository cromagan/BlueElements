// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using BlueBasics.FileSystemCaching;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using BlueControls.ItemCollectionPad.Abstract;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueScript;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using BlueTable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using static BlueBasics.Constants;
using static BlueBasics.Converter;
using static BlueBasics.Generic;

namespace BlueControls.ItemCollectionPad;

public sealed class ItemCollectionPadItem : RectanglePadItem, IEnumerable<AbstractPadItem>, IReadableTextWithKey, IParseable, ICanHaveVariables, IStyleable {

    #region Fields

    public const int Dpi = 300;

    public bool AutoZoomFit = true;

    private readonly ObservableCollection<AbstractPadItem> _internal = [];
    private readonly object _itemLock = new();
    private string _caption = string.Empty;

    private bool _endless;

    private float _gridShow = 10;

    private float _gridsnap = 1;
    private Padding _randinMm = Padding.Empty;

    private string _sheetStyle;

    private SnapMode _snapMode = SnapMode.SnapToGrid;

    #endregion

    #region Constructors

    public ItemCollectionPadItem() : base(string.Empty) {
        Breite = 10;
        Höhe = 10;
        _endless = false;
        RandinMm = Padding.Empty;
        _sheetStyle = string.Empty;

        Connections.CollectionChanged += ConnectsTo_CollectionChanged;
        IsSaved = true;
    }

    public ItemCollectionPadItem(string layoutFileName) : this() {
        if (layoutFileName.IsFormat(FormatHolder.FilepathAndName)) {

            if(!IO.DirectoryExists(layoutFileName.FilePath())) {
                IO.CreateDirectory(layoutFileName.FilePath());
            }

            var f = CachedFileSystem.Get(layoutFileName.FilePath());

            if (f.FileExists(layoutFileName)) {
                this.Parse(f.ReadAllText(layoutFileName, Win1252));
            }
        }

        IsSaved = true;
    }

    #endregion

    #region Destructors

    ~ItemCollectionPadItem() {
        Dispose(false);
    }

    #endregion

    #region Events

    public event EventHandler<System.EventArgs>? ItemAdded;

    public event EventHandler? ItemRemoved;

    public event EventHandler? StyleChanged;

    #endregion

    #region Properties

    public static string ClassId => "ITEMCOLLECTION";

    public Color BackColor { get; set; } = Color.White;

    public override float Breite {
        get => base.Breite;
        set {
            base.Breite = value;
            if (Breite > 0) { Endless = false; }
        }
    }

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

    public bool Endless {
        get => Parent == null && _endless;
        set {
            if (Parent != null) { value = false; }
            if (value == _endless) { return; }

            _endless = value;
            OnPropertyChanged();
        }
    }

    public new bool ForPrinting { get; set; }

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

    /// <summary>
    /// Gibt zurück, ob die Collection Items enthält.
    /// Die Collection "Head" gibt immer true zurück.
    /// </summary>
    public bool HasItems {
        get {
            if (string.Equals(_caption, "Head", StringComparison.OrdinalIgnoreCase)) { return true; }

            return _internal.Count > 0;
        }
    }

    public override float Höhe {
        get => base.Höhe;
        set {
            base.Höhe = value;
            if (Höhe > 0) { Endless = false; }
        }
    }

    [DefaultValue(true)]
    public bool IsSaved { get; set; }

    public Padding RandinMm {
        get => _randinMm;
        set {
            _randinMm = new Padding(Math.Max(0, value.Left), Math.Max(0, value.Top), Math.Max(0, value.Right), Math.Max(0, value.Bottom));
            OnPropertyChanged();
        }
    }

    public string SheetStyle {
        get => Parent is IStyleable ist ? ist.SheetStyle : _sheetStyle;
        set {
            if (IsDisposed) { return; }

            if (Parent is IStyleable ist) { value = ist.SheetStyle; }

            if (_sheetStyle == value) { return; }
            _sheetStyle = value;
            OnStyleChanged();
            OnPropertyChanged();
        }
    }

    public new bool ShowAlways { get; set; }

    public new bool ShowJointPoints { get; set; }

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

    public string UniqueId {
        get {
            if (Parent is ItemCollectionPadItem icpi) {
                return "C" + (icpi.UniqueId + "|" + KeyName).GetHashString();
            }

            return "S" + KeyName.GetHashString();
        }
    }

    protected override int SaveOrder => 1000;

    #endregion

    #region Indexers

    public AbstractPadItem? this[string keyName] => _internal.GetByKey(keyName);

    public AbstractPadItem? this[int nr] => _internal[nr];

    #endregion

    #region Methods

    public static (float scale, float offsetX, float offsetY) AlterView(RectangleF positionControl, float scale, float offsetX, float offsetY, bool autoZoomFit, RectangleF usedArea) {
        var newX = offsetX;
        var newY = offsetY;
        var newS = scale;

        if (autoZoomFit) {
            newS = ZoomFitValue(usedArea, positionControl.ToRect().Size);
            newX = -positionControl.X - (positionControl.Width / 2f);
            newY = -positionControl.Y - (positionControl.Height / 2f);
            newX += (usedArea.Left + (usedArea.Width / 2f)) * newS;
            newY += (usedArea.Top + (usedArea.Height / 2f)) * newS;
        }

        return (newS, newX, newY);
    }

    /// <summary>
    /// Gibt den Versatz der Linken oben Ecke aller Objekte zurück, um mittig zu sein.
    /// </summary>
    /// <param name="maxCanvasBounds"></param>
    /// <param name="controlArea"></param>
    /// <param name="zoom"></param>
    /// <returns></returns>
    public static Point FreiraumControl(RectangleF maxCanvasBounds, Size controlArea, float zoom) {
        var w = controlArea.Width - maxCanvasBounds.Width.CanvasToControl(zoom);
        var h = controlArea.Height - maxCanvasBounds.Height.CanvasToControl(zoom);
        return new Point(w, h);
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

            newY.Add(thisIt.CanvasUsedArea.Y.CanvasToControl(scaleY));

            #endregion

            #region  newX

            newX.Add(thisIt.CanvasUsedArea.X.CanvasToControl(scaleX));

            #endregion

            #region  newH

            var nh = thisIt.CanvasUsedArea.Height.CanvasToControl(scaleY);

            if (thisIt.AutoSizeableHeight) {
                if (!thisIt.CanChangeHeightTo(nh)) {
                    nh = (int)AutosizableExtension.MinHeigthCapAndBox;
                }
            } else {
                nh = (int)thisIt.CanvasUsedArea.Height;
            }

            newH.Add(nh);

            #endregion

            #region  newW

            newW.Add(thisIt.CanvasUsedArea.Width.CanvasToControl(scaleX));

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

                        if (its[coll].CanvasUsedArea.Y >= its[tocheck].CanvasUsedArea.Bottom && its[coll].CanvasUsedArea.IntersectsVericalyWith(its[tocheck].CanvasUsedArea)) {
                            newY[coll] = newY[tocheck] + newH[tocheck] + its[coll].CanvasUsedArea.Top - its[tocheck].CanvasUsedArea.Bottom;
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

        foreach (var thisc in padData._internal) {
            if (thisc is IAutosizable aas && aas.IsVisibleForMe(mode, true) &&
                IsInDrawingArea(thisc.CanvasUsedArea, padData.CanvasUsedArea.ToRect())) {
                its.Add(aas);
            }
        }

        its.Sort((it1, it2) => it1.CanvasUsedArea.Y.CompareTo(it2.CanvasUsedArea.Y));

        #endregion

        var p = ResizeControls(its, newWidthPixel, newhHeightPixel, padData.CanvasUsedArea.Width, padData.CanvasUsedArea.Height);

        var erg = new List<(IAutosizable item, RectangleF newpos)>();

        for (var x = 0; x < its.Count; x++) {
            erg.Add((its[x], p[x]));
        }

        return erg;
    }

    public static float ZoomFitValue(RectangleF canvasBounds, Size controlSize) => canvasBounds.IsEmpty
            ? 1f
            : Math.Min(controlSize.Width / canvasBounds.Width,
            controlSize.Height / canvasBounds.Height);

    public void Add(AbstractPadItem? item) {
        if (item == null) { Develop.DebugPrint(ErrorType.Error, "Item ist null"); return; }
        if (_internal.Contains(item)) { Develop.DebugPrint(ErrorType.Error, "Bereits vorhanden!"); return; }
        if (this[item.KeyName] != null) { Develop.DebugPrint(ErrorType.Warning, "Name bereits vorhanden: " + item.KeyName); return; }

        if (string.IsNullOrEmpty(item.KeyName)) { Develop.DebugPrint(ErrorType.Error, "Item ohne Namen!"); return; }
        lock (_itemLock) {
            _internal.Add(item);
        }
        item.AddedToCollection(this);

        IsSaved = false;
        OnItemAdded();

        item.PropertyChanged += Item_PropertyChanged;
        //item.CompareKeyChanged += Item_CompareKeyChangedChanged;
        //item.CheckedChanged += Item_CheckedChanged;
        //item.CompareKeyChanged += Item_CompareKeyChangedChanged;
    }

    public void BringToFront(AbstractPadItem thisItem) {
        if (_internal.IndexOf(thisItem) == _internal.Count - 1) { return; }
        lock (_itemLock) {
            _internal.Remove(thisItem);
            _internal.Add(thisItem);
        }
    }

    public void Clear() {
        List<AbstractPadItem> l = [.. _internal];

        foreach (var thisit in l) {
            Remove(thisit);
        }

        lock (_itemLock) {
            _internal.Clear();
        }
    }

    public bool Contains(AbstractPadItem item) => _internal.Contains(item);

    public void EineEbeneNachHinten(AbstractPadItem bpi) {
        var i2 = Previous(bpi);
        if (i2 != null) {
            Swap(_internal.IndexOf(bpi), _internal.IndexOf(i2));
        }
    }

    public void EineEbeneNachVorne(AbstractPadItem bpi) {
        var i2 = Next(bpi);
        if (i2 != null) {
            Swap(_internal.IndexOf(bpi), _internal.IndexOf(i2));
        }
    }

    public IEnumerator<AbstractPadItem> GetEnumerator() => ((IEnumerable<AbstractPadItem>)_internal).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_internal).GetEnumerator();

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result =
        [   .. base.GetProperties(widthOfControl),
            new FlexiControl(),
            new FlexiControlForProperty<float>(() => GridShow),
            new FlexiControlForProperty<bool>(() => AutoZoomFit),
        ];
        return result;
    }

    public RowEntryPadItem? GetRowEntryItem() {
        foreach (var thisit in this) {
            if (thisit is RowEntryPadItem repi) {
                return repi;
            }
        }

        return null;
    }

    public ItemCollectionPadItem? GetSubItemCollection(string keyOrCaption) {
        foreach (var thisP in this) {
            if (thisP is ItemCollectionPadItem { IsDisposed: false } icp2) {
                if (string.Equals(icp2.KeyName, keyOrCaption, StringComparison.OrdinalIgnoreCase)) { return icp2; }
                if (string.Equals(icp2.Caption, keyOrCaption, StringComparison.OrdinalIgnoreCase)) { return icp2; }
            }
        }

        return null;
    }

    public AbstractPadItem? HotItem(Point controlPoint, bool topLevel, float zoom, float offsetX, float offsetY) {
        // Berechne die unscaled Koordinaten für dieses Item

        var canvasPoint = controlPoint.ControlToCanvas(zoom, offsetX, offsetY);

        //CreativePad.MouseCoords = canvasPoint.ToString();

        //// Prüfe die Grenzen nur, wenn nicht endlos
        //if (!_endless && !CanvasUsedArea.CanvasContainsx(canvasPoint)) { return null; }

        // Finde alle Items, die den Punkt enthalten
        var hotItems = _internal.Where(item => item?.CanvasContains(canvasPoint, zoom) == true)
                                        .OrderBy(item => item.CanvasUsedArea.Width * item.CanvasUsedArea.Height)
                                        .ToList();

        // Wenn kein Item gefunden wurde, return null
        if (hotItems.Count == 0) { return null; }

        // Nehme das kleinste Item (das oberste in der Z-Reihenfolge bei gleicher Größe)
        var smallestHotItem = hotItems.First();

        // Wenn topLevel true ist, geben wir das gefundene Item zurück ohne tiefer zu gehen
        if (topLevel) {
            //CreativePad.Highlight = smallestHotItem;
            return smallestHotItem;
        }

        // Wenn das kleinste Item eine ItemCollection ist, gehen wir tiefer
        if (smallestHotItem is ItemCollectionPadItem { IsDisposed: false } icpi) {
            var alteredUsedArea = icpi.CanvasUsedArea.CanvasToControl(zoom, offsetX, offsetY, false);
            var (childScale, childOffsetX, childOffsetY) = AlterView(alteredUsedArea, zoom, offsetX, offsetY, icpi.AutoZoomFit, icpi.UsedAreaOfItems());

            ////Berechne den neuen Punkt für das Kind - Item
            //var childPoint = new Point(
            //    (int)(-(controlPoint.ControlX - alteredUsedArea.ControlX)),
            //    (int)(-(controlPoint.Y - alteredUsedArea.Y))
            //);

            //var childPoint = new Point(
            //    (int)alteredUsedArea.ControlX + (int)( controlPoint.ControlX- alteredUsedArea.ControlX),
            //    (int)alteredUsedArea.Y + (int)(controlPoint.Y - alteredUsedArea.Y));

            //var pn = new Point(controlPoint.ControlX - (int) alteredUsedArea.ControlX, canvasPoint.Y);
            // Berechnung des childPoint
            //var childPoint = ZoomPad.CoordinatesUnscaled(controlPoint, childScale, childOffsetX, childOffsetY);

            //var childPoint = ZoomPad.CoordinatesUnscaled(pn, childScale, childOffsetX, childOffsetY);

            // Rekursiver Aufruf mit den angepassten Koordinaten
            var childHotItem = icpi.HotItem(controlPoint, false, childScale, childOffsetX, childOffsetY);
            //CreativePad.XXX = CreativePad.XXX  + ";" + childOffsetX.ToString();
            // Wenn ein Kind-Item gefunden wurde, geben wir dieses zurück
            if (childHotItem != null) {
                //CreativePad.Highlight = childHotItem;
                return childHotItem;
            }
        }
        // Wenn kein Kind-Item gefunden wurde oder es kein ItemCollection war,
        // geben wir das ursprünglich gefundene Item zurück
        //CreativePad.Highlight = smallestHotItem;
        return smallestHotItem;
    }

    public void Items_Mirror(PointM? p, bool vertical, bool horizontal) {
        foreach (var thisItem in _internal) {
            if (thisItem is IMirrorable m) { m.Mirror(p, vertical, horizontal); }
        }
    }

    public void Items_Move(float x, float y) {
        if (x == 0 && y == 0) { return; }

        foreach (var thisItem in _internal) {
            thisItem.Move(x, y, false);
        }
    }

    public void Items_SetTestScale(float scale) {
        foreach (var thisItem in _internal) {
            if (thisItem is ISupportsTextScale m) {
                m.TextScale = scale;
            }
        }
    }

    public void OnStyleChanged() => StyleChanged?.Invoke(this, System.EventArgs.Empty);

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }

        if (!HasItems) { return []; }

        List<string> result = [.. base.ParseableItems()];

        result.ParseableAdd("Caption", _caption);

        result.ParseableAdd("Style", _sheetStyle);

        result.ParseableAdd("BackColor", BackColor.ToArgb());

        result.ParseableAdd("PrintArea", _randinMm.ToString());

        result.ParseableAdd("Endless", Endless);

        result.ParseableAdd("Item", _internal);

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
        switch (key) {
            case "sheetsize":
                var _sheetSizeInMm = value.SizeFParse();
                Breite = _sheetSizeInMm.Width;
                Höhe = _sheetSizeInMm.Height;
                return true;

            case "printarea":
                _randinMm = value.PaddingParse();
                return true;

            case "caption":
                _caption = value.FromNonCritical();
                return true;

            case "backcolor":
                BackColor = Color.FromArgb(IntParse(value));
                return true;

            case "style":
                _sheetStyle = value;
                return true;

            case "fontscale":
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
                if (!CreateItems(value)) { return false; }  // im ersten Step nur die items erzeugen....
                if (!ParseItems(value)) { return false; }// ...im zweiten können nun auch Beziehungen erstellt werden!
                return true;

            case "item":
                var i = NewByParsing<AbstractPadItem>(value.FromNonCritical());
                if (i != null) { Add(i); }
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
                _sheetStyle = value;
                return true;

            case "sheetstylescale":
                return true;

            case "endless":
                _endless = value.FromPlusMinus();
                return true;
        }

        return base.ParseThis(key, value);
    }

    public override string ReadableText() => BestCaption();

    public void Remove(AbstractPadItem? item) {
        if (IsDisposed) { return; }
        if (item == null || !_internal.Contains(item)) { return; }
        item.PropertyChanged -= Item_PropertyChanged;
        lock (_itemLock) {
            _internal.Remove(item);
        }
        item.Parent = null;
        OnItemRemoved();
        OnPropertyChanged("Items");
    }

    public void RemoveRange(List<AbstractPadItem> remove) {
        foreach (var thisItem in remove) {
            Remove(thisItem);
        }
    }

    //Used: Only BZL
    public bool ReplaceVariable(string name, string wert) => ReplaceVariable(new VariableString(name, wert));

    public bool ReplaceVariable(Variable variable) {
        var did = false;
        foreach (var thisItem in _internal) {
            if (thisItem is ICanHaveVariables variables) {
                if (variables.ReplaceVariable(variable)) { did = true; }
            }
        }
        return did;
    }

    public ScriptEndedFeedback ReplaceVariables(RowItem? row) {
        if (row is not { IsDisposed: false }) { return new ScriptEndedFeedback("Keine Zeile angekommen", false, false, "Export"); }

        var script = row.ExecuteScript(ScriptEventTypes.export, string.Empty, true, 0, null, true, false);
        if (script.Failed) { return script; }

        this.ParseVariables(script.Variables);

        return script;
    }

    public ScriptEndedFeedback ReplaceVariables(Table table, string rowkey) => ReplaceVariables(table.Row.GetByKey(rowkey));

    public bool ResetVariables() {
        if (IsDisposed) { return false; }
        var did = false;
        foreach (var thisItem in _internal) {
            if (thisItem is ICanHaveVariables variables) {
                if (variables.ResetVariables()) { did = true; }
            }
        }
        if (did) { OnPropertyChanged("Variables"); }
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
                Forms.MessageBox.Show("Dateiformat unbekannt: " + filename.FileSuffix().ToUpperInvariant(), ImageCode.Warnung, "OK");
                return;
        }
    }

    public void SendToBack(AbstractPadItem thisItem) {
        if (_internal.IndexOf(thisItem) == 0) { return; }
        lock (_itemLock) {
            _internal.Remove(thisItem);
            _internal.Insert(0, thisItem);
        }
    }

    public void Swap(int index1, int index2) {
        if (IsDisposed) { return; }
        if (index1 == index2) { return; }
        lock (_itemLock) {
            (_internal[index1], _internal[index2]) = (_internal[index2], _internal[index1]);
        }
        OnPropertyChanged("Items");
    }

    public override QuickImage SymbolForReadableText() => IsHead() ? QuickImage.Get(ImageCode.Diskette) : QuickImage.Get(ImageCode.Gruppe);

    public List<string> VisibleFor_AllUsed() {
        var l = new List<string>();

        foreach (var thisIt in _internal) {
            if (thisIt is ReciverControlPadItem csi) {
                l.AddRange(csi.VisibleFor);
            }

            if (thisIt is ItemCollectionPadItem { IsDisposed: false } icpi) {
                l.AddRange(icpi.VisibleFor_AllUsed());
            }
        }

        l.Add(Administrator);
        l.Add(Everybody);
        l.Add("#User: " + UserName);

        l.AddRange(TableView.Permission_AllUsed(false));

        return Table.RepairUserGroups(l);
    }

    internal string BestCaption() {
        if (IsHead() && GetConnectedFormula() is { IsDisposed: false } cf) {
            return cf.Filename.FileNameWithoutSuffix();
        }

        if (string.IsNullOrEmpty(_caption)) { return "?"; }

        return _caption;
    }

    internal ScriptEndedFeedback ExecuteScript(string scripttext, string mode, RowItem rowIn) {
        //var generatedentityID = rowIn.ReplaceVariables(entitiId, true, null);
        var vars = rowIn.Table?.CreateVariableCollection(rowIn, true, false, true, false) ?? [];

        //var vars = new VariableCollection();
        vars.Add(new VariableString("Application", Develop.AppName(), true, "Der Name der App, die gerade geöffnet ist."));
        vars.Add(new VariableString("User", UserName, true, "ACHTUNG: Keinesfalls dürfen benutzerabhängig Werte verändert werden."));
        vars.Add(new VariableString("Usergroup", UserGroup, true, "ACHTUNG: Keinesfalls dürfen gruppenabhängig Werte verändert werden."));
        //vars.Add(new VariableListString("Menu", null, false, "Diese Variable muss das Rückgabemenü enthalten."));
        //vars.Add(new VariableListString("Infos", null, false, "Diese Variable kann Zusatzinfos zum Menu enthalten."));
        //vars.Add(new VariableListString("CurrentlySelected", selected, true, "Was der Benutzer aktuell angeklickt hat."));
        //vars.Add(new VariableString("EntityId", generatedentityID, true, "Dies ist die Eingangsvariable."));
        vars.Add(new VariableString("Mode", mode, true, "In welchem Modus die Formulare angezeigt werden."));

        vars.Add(new VariableItemCollectionPad("Pad", this, true, "Auf diesem Objekt wird gezeichnet"));

        var m = Method.GetMethods(MethodType.ManipulatesUser);

        var scp = new ScriptProperties("CreativePad-Generator", m, true, [], rowIn, "CreativePad-Generator", "CreativePad-Generator");

        var sc = new Script(vars, scp) {
            ScriptText = scripttext
        };

        var t = sc.Parse(0, "Main", null, null);

        if (t.Failed) {
            var bpi = new BitmapPadItem(string.Empty, QuickImage.Get(ImageCode.Kritisch, 64), new Size(500, 500));
            Add(bpi);
        }
        return t;
    }

    internal ConnectedFormula.ConnectedFormula? GetConnectedFormula() {
        if (Parent is ConnectedFormula.ConnectedFormula cf) { return cf; }

        if (Parent is ItemCollectionPadItem icpi) { return icpi.GetConnectedFormula(); }
        return null;
    }

    internal PointM? GetJointPoint(string pointName, AbstractPadItem? notOfMe) {
        foreach (var thisIt in _internal) {
            if (thisIt != notOfMe) {
                foreach (var thisPt in thisIt.JointPoints) {
                    if (string.Equals(thisPt.KeyName, pointName, StringComparison.OrdinalIgnoreCase)) { return thisPt; }
                }
            }
        }

        return null;
    }

    internal List<PointM> GetJointPoints(string pointName, AbstractPadItem? notOfMe) {
        var l = new List<PointM>();

        foreach (var thisIt in _internal) {
            if (thisIt != notOfMe) {
                foreach (var thisPt in thisIt.JointPoints) {
                    if (string.Equals(thisPt.KeyName, pointName, StringComparison.OrdinalIgnoreCase)) { l.Add(thisPt); }
                }
            }
        }

        return l;
    }

    /// <summary>
    /// Prüft, ob das Formular sichtbare Elemente hat.
    /// Zeilenselectionen werden dabei ignoriert.
    /// </summary>
    /// <returns></returns>
    internal bool HasVisibleItemsForMe(string mode) {
        if (IsDisposed) { return false; }

        foreach (var thisItem in this) {
            if (thisItem is ReciverControlPadItem { MustBeInDrawingArea: true } cspi) {
                if (cspi.IsVisibleForMe(mode, false)) { return true; }
            }
        }

        return false;
    }

    internal bool IsHead() => string.Equals(_caption, "Head", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Enthält Names keine Eintrag (Count =0) , werden alle Punkte gelöscht
    /// </summary>
    /// <param name="names"></param>
    internal void Items_DeleteJointPoints(List<string> names) {
        foreach (var thisItem in _internal) {
            thisItem.DeleteJointPoints(names);
        }
    }

    internal AbstractPadItem? Next(AbstractPadItem bpi) {
        var itemCount = _internal.IndexOf(bpi);
        if (itemCount < 0) { Develop.DebugPrint(ErrorType.Error, "Item im SortDefinition nicht enthalten"); }
        do {
            itemCount++;
            if (itemCount >= _internal.Count) { return null; }
            if (_internal[itemCount] != null) { return _internal[itemCount]; }
        } while (true);
    }

    internal AbstractPadItem? Previous(AbstractPadItem bpi) {
        var itemCount = _internal.IndexOf(bpi);
        if (itemCount < 0) { Develop.DebugPrint(ErrorType.Error, "Item im SortDefinition nicht enthalten"); }
        do {
            itemCount--;
            if (itemCount < 0) { return null; }
            if (_internal[itemCount] != null) { return _internal[itemCount]; }
        } while (true);
    }

    internal void Resize(float newWidthPixel, float newhHeightPixel, bool changeControls, string mode) {
        if (_internal.Count == 0) { return; }

        if (changeControls) {
            var x = ResizeControls(this, newWidthPixel, newhHeightPixel, mode);

            #region Die neue Position in die Items schreiben

            foreach (var (item, newpos) in x) {
                item.SetCoordinates(newpos);
            }

            #endregion
        }

        Breite = PixelToMm(newWidthPixel, Dpi);
        Höhe = PixelToMm(newhHeightPixel, Dpi);
    }

    protected override RectangleF CalculateCanvasUsedArea() {
        if (!Endless) { return base.CalculateCanvasUsedArea(); }

        var f = UsedAreaOfItems();
        return f with { X = f.X + _pLo.X, Y = f.Y + _pLo.Y };
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        foreach (var thisIt in _internal) {
            thisIt.PropertyChanged -= Item_PropertyChanged;
            thisIt.Dispose();
        }
        UnRegisterEvents();

        Connections.CollectionChanged -= ConnectsTo_CollectionChanged;
        Connections.RemoveAll();
    }

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, float scale, float offsetX, float offsetY) {
        gr.PixelOffsetMode = PixelOffsetMode.None;

        var d = !Endless ? CanvasUsedArea.CanvasToControl(scale, offsetX, offsetY, false) : visibleAreaControl;
        var ds = !Endless ? positionControl : visibleAreaControl;

        if (BackColor.A > 0) {
            gr.FillRectangle(new SolidBrush(BackColor), d);
        }

        #region Grid

        if (_gridShow > 0.1) {
            var tmpgrid = _gridShow;

            while (MmToPixel(tmpgrid, Dpi).CanvasToControl(scale) < 5) { tmpgrid *= 2; }

            var p = new Pen(Color.FromArgb(10, 0, 0, 0));
            float ex = 0;

            var po = new PointM(0, 0).CanvasToControl(scale, offsetX, offsetY);
            float dxp, dyp, dxm, dym;

            do {
                var mo = MmToPixel(ex * tmpgrid, Dpi).CanvasToControl(scale);

                dxp = po.X + mo;
                dxm = po.X - mo;
                dyp = po.Y + mo;
                dym = po.Y - mo;

                if (dxp > ds.Left && dxp < ds.Right) { gr.DrawLine(p, dxp, ds.Top, dxp, ds.Bottom); }
                if (dyp > ds.Top && dyp < ds.Bottom) { gr.DrawLine(p, ds.Left, dyp, ds.Right, dyp); }

                if (ex > 0) {
                    // erste Linie nicht doppelt zeichnen
                    if (dxm > ds.Left && dxm < ds.Right) { gr.DrawLine(p, dxm, ds.Top, dxm, ds.Bottom); }
                    if (dym > ds.Top && dym < ds.Bottom) { gr.DrawLine(p, ds.Left, dym, ds.Right, dym); }
                }

                ex++;
            } while (!(dxm < ds.Left &&
                    dym < ds.Top &&
                    dxp > ds.Right &&
                    dyp > ds.Bottom));
        }

        #endregion

        #region Items selbst

        var (childScale, childOffsetX, childOffsetY) = AlterView(positionControl, scale, offsetX, offsetY, AutoZoomFit, UsedAreaOfItems());

        foreach (var thisItem in _internal) {
            gr.PixelOffsetMode = PixelOffsetMode.None;
            thisItem.Draw(gr, positionControl.ToRect(), childScale, childOffsetX, childOffsetY);
        }

        #endregion
    }

    protected override void OnParentChanged() {
        base.OnParentChanged();

        if (Parent is ItemCollectionPadItem icpi) {
            icpi.StyleChanged += Icpi_StyleChanged;
        }

        Icpi_StyleChanged(Parent, System.EventArgs.Empty);
    }

    protected override void OnParentChanging() {
        base.OnParentChanging();
        UnRegisterEvents();
    }

    protected override void OnPropertyChanged([CallerMemberName] string propertyName = "unknown") {
        IsSaved = false;
        base.OnPropertyChanged(propertyName);
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
        OnPropertyChanged(nameof(Connections));
    }

    private void CreateConnection(string toParse) {
        if (toParse.StartsWith("[I]")) { toParse = toParse.FromNonCritical(); }

        if (toParse.GetAllTags() is not { } x) { return; }

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

    private bool CreateItems(string toParse) {
        if (toParse.GetAllTags() is not { } x) { return false; }

        foreach (var pair in x) {
            switch (pair.Key.ToLowerInvariant()) {
                case "dpi":
                case "sheetstylescale":
                case "sheetstyle":
                    break;

                case "item":
                    var i = NewByParsing<AbstractPadItem>(pair.Value);
                    if (i != null) { Add(i); }
                    break;

                default:
                    Develop.DebugPrint(ErrorType.Warning, "Tag unbekannt: " + pair.Key);
                    break;
            }
        }
        return true;
    }

    private void Icpi_StyleChanged(object? sender, System.EventArgs e) {
        if (sender is IStyleable ist) {
            SheetStyle = ist.SheetStyle;
        }
    }

    private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e) => OnPropertyChanged(e.PropertyName);

    private void OnItemAdded() {
        if (IsDisposed) { return; }
        OnPropertyChanged("Items");
        ItemAdded?.Invoke(this, System.EventArgs.Empty);
    }

    private void OnItemRemoved() {
        ItemRemoved?.Invoke(this, System.EventArgs.Empty);
        if (IsDisposed) { return; }
        OnPropertyChanged("Items");
    }

    private bool ParseItems(string toParse) {
        if (toParse.GetAllTags() is not { } xa) { return false; }

        foreach (var pair in xa) {
            switch (pair.Key.ToLowerInvariant()) {
                case "dpi":
                case "sheetstyle":
                case "sheetstylescale":
                    break;

                case "item":
                    var t = pair.Value;
                    if (t.StartsWith("[I]")) { t = t.FromNonCritical(); }
                    if (t.GetAllTags() is not { } xi) { return false; }

                    foreach (var thisIt in xi) {
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
                    Develop.DebugPrint(ErrorType.Warning, "Tag unbekannt: " + pair.Key);
                    break;
            }
        }
        return true;
    }

    private void UnRegisterEvents() {
        if (Parent is ItemCollectionPadItem icpi) {
            icpi.StyleChanged -= Icpi_StyleChanged;
        }
    }

    private RectangleF UsedAreaOfItems() {
        var x1 = float.MaxValue;
        var y1 = float.MaxValue;
        var x2 = float.MinValue;
        var y2 = float.MinValue;
        var done = false;
        foreach (var thisItem in _internal) {
            if (thisItem != null) {
                var ua = thisItem.CanvasUsedArea;
                x1 = Math.Min(x1, ua.Left);
                y1 = Math.Min(y1, ua.Top);
                x2 = Math.Max(x2, ua.Right);
                y2 = Math.Max(y2, ua.Bottom);
                done = true;
            }
        }

        return !done ? new RectangleF(-5, -5, 10, 10) : new RectangleF(x1, y1, x2 - x1, y2 - y1);
    }

    #endregion
}