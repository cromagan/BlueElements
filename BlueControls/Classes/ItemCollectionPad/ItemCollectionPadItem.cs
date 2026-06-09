// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Classes.FileSystemCaching;
using BlueControls.Classes.ItemCollectionPad.Abstract;
using BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular;
using BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueControls.Controls;
using BlueControls.Controls.ConnectedFormula;
using BlueScript.Classes;
using BlueScript.Variables;
using System.Collections;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using static BlueBasics.ClassesStatic.Constants;
using static BlueBasics.ClassesStatic.Converter;
using static BlueBasics.ClassesStatic.Generic;

namespace BlueControls.Classes.ItemCollectionPad;

public sealed class ItemCollectionPadItem : RectanglePadItem, IEnumerable<AbstractPadItem>, IReadableTextWithKey, IParseable, ICanHaveVariables, IStyleable {

    #region Fields

    public const int Dpi = 300;

    private static readonly Pen GridPen = new(Color.FromArgb(10, 0, 0, 0));
    private readonly ObservableCollection<AbstractPadItem> _internal = [];
    private readonly object _itemLock = new();

    private RectangleF _cachedUsedAreaOfItems;
    private bool _usedAreaOfItemsDirty = true;

    #endregion

    #region Constructors

    public ItemCollectionPadItem() : base(string.Empty) {
        Breite = 10;
        Höhe = 10;
        Endless = false;

        IsSaved = true;
    }

    public ItemCollectionPadItem(string layoutFileName) : this() {
        if (layoutFileName.IsFormat(FormatHolder_FilepathAndName.Instance)) {
            if (!IO.DirectoryExists(layoutFileName.FilePath())) {
                IO.CreateDirectory(layoutFileName.FilePath());
            }

            var f = CachedFileSystem.Get<ConnectedFormula>(layoutFileName);

            if (f is not null) {
                this.Parse(Win1252.GetString(f.Content));
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

    [DefaultValue(true)]
    public bool AutoZoomFit {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = true;

    public Color BackColor {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = Color.White;

    public override float Breite {
        get => base.Breite;
        set {
            base.Breite = value;
            if (Breite > 0) { Endless = false; }
        }
    }

    [DefaultValue(false)]
    public string Caption {
        get => field;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    public override string Description => "Eine Sammlung von Anzeige-Objekten";

    public EditMode EditMode {
        get {
            if (Parent is ItemCollectionPadItem { IsDisposed: false } icpi) {
                return (EditMode)Math.Min((int)icpi.EditMode, (int)field);
            }
            return field;
        }
        set {
            if (value == field) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = EditMode.Editable;

    public bool Endless {
        get => Parent is null && field;
        set {
            if (Parent is not null) { value = false; }
            if (value == field) { return; }

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// in mm
    /// </summary>
    [DefaultValue(10.0)]
    public float GridShow {
        get;
        set {
            if (IsDisposed) { return; }
            if (Math.Abs(field - value) < DefaultTolerance) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = 10f;

    /// <summary>
    /// in mm
    /// </summary>
    [DefaultValue(10.0)]
    public float GridSnap {
        get;
        set {
            if (IsDisposed) { return; }
            if (Math.Abs(field - value) < DefaultTolerance) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = 10f;

    /// <summary>
    /// Gibt zurück, ob die Collection Items enthält.
    /// Die Collection "Head" gibt immer true zurück.
    /// </summary>
    public bool HasItems {
        get {
            if (string.Equals(Caption, "Head", StringComparison.OrdinalIgnoreCase)) { return true; }

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
        get;
        set {
            var v = new Padding(Math.Max(0, value.Left), Math.Max(0, value.Top), Math.Max(0, value.Right), Math.Max(0, value.Bottom));
            if (field == v) { return; }
            field = v;
            OnPropertyChanged();
        }
    } = Padding.Empty;

    public string SheetStyle {
        get => Parent is IStyleable ist ? ist.SheetStyle : field;
        set {
            if (IsDisposed) { return; }

            if (Parent is IStyleable ist) { value = ist.SheetStyle; }

            if (field == value) { return; }
            field = value;
            OnStyleChanged();
            OnPropertyChanged();
        }
    } = string.Empty;

    public new bool ShowAlways {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public new bool ShowJointPoints {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    [DefaultValue(false)]
    public SnapMode SnapMode {
        get => field;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = SnapMode.SnapToGrid;

    public string UniqueId {
        get {
            if (Parent is ItemCollectionPadItem icpi) {
                return "C" + (icpi.UniqueId + "|" + KeyName).GetMD5Hash();
            }

            return "S" + KeyName.GetMD5Hash();
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

            // Berechne den Mittelpunkt der 'usedArea' im unskalierten Koordinatensystem
            var usedAreaCenterX = usedArea.X + usedArea.Width / 2f;
            var usedAreaCenterY = usedArea.Y + usedArea.Height / 2f;

            // Berechne den Mittelpunkt der 'positionControl' (der Anzeigebereich)
            var positionControlCenterX = positionControl.X + positionControl.Width / 2f;
            var positionControlCenterY = positionControl.Y + positionControl.Height / 2f;

            // Der neue Offset sollte den Mittelpunkt der usedArea auf den Mittelpunkt von positionControl verschieben,
            // unter Berücksichtigung des neuen Skalierungsfaktors.
            newX = -usedAreaCenterX * newS + positionControlCenterX;
            newY = -usedAreaCenterY * newS + positionControlCenterY;
        }

        return (newS, newX, newY);
    }

    /// <summary>
    /// Gibt den Versatz der Linken oben Ecke aller Objekte zurück, um mittig zu sein.
    /// </summary>
    /// <param name="canvasUsedArea"></param>
    /// <param name="controlArea"></param>
    /// <param name="zoom"></param>
    /// <returns></returns>
    public static Point FreiraumControl(RectangleF canvasUsedArea, Size controlArea, float zoom) {
        var offsetX = canvasUsedArea.X.CanvasToControl(zoom);
        var offsetY = canvasUsedArea.Y.CanvasToControl(zoom);

        var w = controlArea.Width - canvasUsedArea.Width.CanvasToControl(zoom) - offsetX;
        var h = controlArea.Height - canvasUsedArea.Height.CanvasToControl(zoom) - offsetY;

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

        #region Alle veränderlichen Items ausdehnen, um freien Platz nach unten zu füllen

        // Fehlerfall: veränderliche Items wurden durch Verschiebung (z.B. gap-stutzen) nach oben
        // geschoben, haben aber ihre Höhe nicht angepasst. Dadurch bleibt Leerraum unterhalb.
        for (var tocheck = 0; tocheck < its.Count; tocheck++) {
            if (its[tocheck].CanScaleHeightTo(scaleY)) {
                var pos = PositioOf(tocheck);
                var availableBottom = newHeight;

                for (var coll = tocheck + 1; coll < its.Count; coll++) {
                    // Nur Items in der gleichen Spalte (X-Überlappung) berücksichtigen
                    if (its[coll].CanvasUsedArea.IntersectsVericalyWith(its[tocheck].CanvasUsedArea)) {
                        availableBottom = Math.Min(availableBottom, PositioOf(coll).Top);
                    }
                }

                if (pos.Bottom < availableBottom) {
                    newH[tocheck] = availableBottom - pos.Top;
                }
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
        List<IAutosizable> outsideItems = [];

        foreach (var thisc in padData._internal) {
            if (thisc is not IAutosizable aas || !aas.IsVisibleForMe(mode, true)) { continue; }

            if (IsInDrawingArea(thisc.CanvasUsedArea, padData.CanvasUsedArea.ToRect())) {
                its.Add(aas);
            } else if (thisc is ReciverControlPadItem { MustBeInDrawingArea: false }) {
                outsideItems.Add(aas);
            }
        }

        its.Sort((it1, it2) => it1.CanvasUsedArea.Y.CompareTo(it2.CanvasUsedArea.Y));

        #endregion

        var p = ResizeControls(its, newWidthPixel, newhHeightPixel, padData.CanvasUsedArea.Width, padData.CanvasUsedArea.Height);

        var erg = new List<(IAutosizable item, RectangleF newpos)>();

        for (var x = 0; x < its.Count; x++) {
            erg.Add((its[x], p[x]));
        }

        foreach (var thiso in outsideItems) {
            var ua = thiso.CanvasUsedArea;
            erg.Add((thiso, new RectangleF(-ua.Width, -ua.Height, ua.Width, ua.Height)));
        }

        return erg;
    }

    public static float ZoomFitValue(RectangleF canvasBounds, Size controlSize) => canvasBounds.IsEmpty
            ? 1f
            : Math.Min(controlSize.Width / canvasBounds.Width,
            controlSize.Height / canvasBounds.Height);

    public void Add(AbstractPadItem? item) {
        if (item is null) { Develop.DebugError("Item ist null"); return; }
        if (_internal.Contains(item)) { Develop.DebugError("Bereits vorhanden!"); return; }
        if (this[item.KeyName] is not null) { Develop.DebugPrint("Name bereits vorhanden: " + item.KeyName); return; }

        if (string.IsNullOrEmpty(item.KeyName)) { throw Develop.DebugError("Item ohne Namen!"); }
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
        if (i2 is not null) {
            Swap(_internal.IndexOf(bpi), _internal.IndexOf(i2));
        }
    }

    public void EineEbeneNachVorne(AbstractPadItem bpi) {
        var i2 = Next(bpi);
        if (i2 is not null) {
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
            new FlexiControlForProperty<EditMode>(() => EditMode),
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

    public AbstractPadItem? HotItem(Point controlPoint, bool topLevel, bool mustEnabled, float zoom, float offsetX, float offsetY) {
        if (EditMode == EditMode.Locked && mustEnabled) { return null; }

        var canvasPoint = controlPoint.ControlToCanvas(zoom, offsetX, offsetY);

        //CreativePad.MouseCoords = canvasPoint.ToString();

        //// Prüfe die Grenzen nur, wenn nicht endlos
        //if (!_endless && !CanvasUsedArea.CanvasContainsx(canvasPoint)) { return null; }

        // Finde alle Items, die den Punkt enthalten
        AbstractPadItem? smallestHotItem = null;
        var smallestArea = float.MaxValue;

        lock (_itemLock) {
            foreach (var item in _internal) {
                if (item is not { IsDisposed: false }) { continue; }
                if (mustEnabled && !item.Enabled) { continue; }
                if (!item.CanvasContains(canvasPoint, zoom)) { continue; }
                var area = item.CanvasUsedArea.Width * item.CanvasUsedArea.Height;
                if (area < smallestArea) {
                    smallestArea = area;
                    smallestHotItem = item;
                }
            }
        }

        // Nehme das kleinste Item (das oberste in der Z-Reihenfolge bei gleicher Größe)
        if (smallestHotItem is null) { return null; }
        // Wenn topLevel true ist, geben wir das gefundene Item zurück ohne tiefer zu gehen

        // Wenn topLevel true ist, geben wir das gefundene Item zurück ohne tiefer zu gehen
        if (topLevel) { return smallestHotItem; }

        // Wenn das kleinste Item eine ItemCollection ist, gehen wir tiefer
        if (smallestHotItem is ItemCollectionPadItem { IsDisposed: false } icpi) {
            var alteredUsedArea = icpi.CanvasUsedArea.CanvasToControl(zoom, offsetX, offsetY, false);
            var (childScale, childOffsetX, childOffsetY) = AlterView(alteredUsedArea, zoom, offsetX, offsetY, icpi.AutoZoomFit, icpi.UsedAreaOfItems());

            var childHotItem = icpi.HotItem(controlPoint, false, mustEnabled, childScale, childOffsetX, childOffsetY);
            if (childHotItem is not null) { return childHotItem; }
        }

        return smallestHotItem;
    }

    public void OnStyleChanged() => StyleChanged?.Invoke(this, System.EventArgs.Empty);

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }

        if (!HasItems) { return []; }

        List<string> result = [.. base.ParseableItems()];

        result.ParseableAdd("Caption", Caption);

        result.ParseableAdd("Style", SheetStyle);

        result.ParseableAdd("BackColor", BackColor.ToArgb());

        result.ParseableAdd("PrintArea", RandinMm.ToString());

        result.ParseableAdd("Endless", Endless);

        result.ParseableAdd("Item", _internal);

        result.ParseableAdd("SnapMode", SnapMode);
        result.ParseableAdd("GridShow", GridShow);
        result.ParseableAdd("GridSnap", GridSnap);
        result.ParseableAdd("EditMode", (int)EditMode);

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
                RandinMm = value.PaddingParse();
                return true;

            case "caption":
                Caption = value.FromNonCritical();
                return true;

            case "backcolor":
                BackColor = Color.FromArgb(IntParse(value));
                return true;

            case "style":
                SheetStyle = value;
                return true;

            case "fontscale":
                return true;

            case "snapmode":
                SnapMode = (SnapMode)IntParse(value);
                return true;

            case "gridshow":
                GridShow = FloatParse(value);
                return true;

            case "gridsnap":
                GridSnap = FloatParse(value);
                return true;

            case "editmode":
                EditMode = (EditMode)IntParse(value);
                return true;

            case "items":
                if (!CreateItems(value)) { return false; }  // im ersten Step nur die items erzeugen....
                if (!ParseItems(value)) { return false; }// ...im zweiten können nun auch Beziehungen erstellt werden!
                return true;

            case "item":
                var i = NewByParsing<AbstractPadItem>(value.FromNonCritical());
                if (i is not null) { Add(i); }
                return true;

            case "connection":
                return true;

            case "dpi":
                if (IntParse(value) != Dpi) {
                    Develop.DebugPrint("Dpi Unterschied: " + Dpi + " <> " + value);
                }
                return true;

            case "sheetstyle":
                SheetStyle = value;
                return true;

            case "sheetstylescale":
                return true;

            case "endless":
                Endless = value.FromPlusMinus();
                return true;
        }

        return base.ParseThis(key, value);
    }

    public override string ReadableText() => BestCaption();

    public void Remove(AbstractPadItem? item) {
        if (IsDisposed) { return; }
        if (item is null || !_internal.Contains(item)) { return; }
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

        var script = row.Table?.ExecuteScript(ScriptEventTypes.export, string.Empty, true, row, null, true, false, 0, false);
        if (script is null || script.Failed) { return script ?? new ScriptEndedFeedback("Tabelle verworfen", false, false, "Export"); }

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
        if (i is null) { return; }

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

        if (string.IsNullOrEmpty(Caption)) { return "?"; }

        return Caption;
    }

    internal ConnectedFormula? GetConnectedFormula() {
        if (Parent is ConnectedFormula cf) { return cf; }

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

    internal bool IsHead() => string.Equals(Caption, "Head", StringComparison.OrdinalIgnoreCase);

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
        if (itemCount < 0) { Develop.DebugError("Item im SortDefinition nicht enthalten"); }
        do {
            itemCount++;
            if (itemCount >= _internal.Count) { return null; }
            if (_internal[itemCount] is not null) { return _internal[itemCount]; }
        } while (true);
    }

    internal AbstractPadItem? Previous(AbstractPadItem bpi) {
        var itemCount = _internal.IndexOf(bpi);
        if (itemCount < 0) { Develop.DebugError("Item im SortDefinition nicht enthalten"); }
        do {
            itemCount--;
            if (itemCount < 0) { return null; }
            if (_internal[itemCount] is not null) { return _internal[itemCount]; }
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

    internal RectangleF UsedAreaOfItems() {
        if (!_usedAreaOfItemsDirty) { return _cachedUsedAreaOfItems; }

        _usedAreaOfItemsDirty = false;
        var x1 = float.MaxValue;
        var y1 = float.MaxValue;
        var x2 = float.MinValue;
        var y2 = float.MinValue;
        var done = false;
        lock (_itemLock) {
            foreach (var thisItem in _internal) {
                if (thisItem is not { IsDisposed: false }) { continue; }
                var ua = thisItem.CanvasUsedArea;
                if (ua.Width <= 0 || ua.Height <= 0) { continue; }
                if (!float.IsFinite(ua.Left) || !float.IsFinite(ua.Top) || !float.IsFinite(ua.Right) || !float.IsFinite(ua.Bottom)) { continue; }
                x1 = Math.Min(x1, ua.Left);
                y1 = Math.Min(y1, ua.Top);
                x2 = Math.Max(x2, ua.Right);
                y2 = Math.Max(y2, ua.Bottom);
                done = true;
            }
        }

        _cachedUsedAreaOfItems = !done ? new RectangleF(-5, -5, 10, 10) : new RectangleF(x1, y1, x2 - x1, y2 - y1);
        return _cachedUsedAreaOfItems;
    }

    protected override RectangleF CalculateCanvasUsedArea() {
        if (!Endless) { return base.CalculateCanvasUsedArea(); }

        var f = UsedAreaOfItems();
        return new RectangleF(f.X + Plo.X, f.Y + Plo.Y, f.Width, f.Height);
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (disposing) {
            foreach (var thisIt in _internal) {
                thisIt.PropertyChanged -= Item_PropertyChanged;
                thisIt.Dispose();
            }
            UnRegisterEvents();

            ItemAdded = null;
            ItemRemoved = null;
            StyleChanged = null;
        }

        _internal.Clear();
    }

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, float zoom, float offsetX, float offsetY, bool forPrinting) {
        gr.PixelOffsetMode = PixelOffsetMode.None;

        var d = !Endless ? CanvasUsedArea.CanvasToControl(zoom, offsetX, offsetY, false) : visibleAreaControl;
        var ds = !Endless ? positionControl : visibleAreaControl;

        if (BackColor.A > 0) {
            var bb = BackgroundFill.GetBrush(BackColor);
            lock (bb) { gr.FillRectangle(bb, d); }
        }

        #region Grid

        if (GridShow > 0.1) {
            var tmpgrid = GridShow;

            while (MmToPixel(tmpgrid, Dpi).CanvasToControl(zoom) < 5) { tmpgrid *= 2; }

            var p = GridPen;
            float ex = 0;

            var po = new PointM(0, 0).CanvasToControl(zoom, offsetX, offsetY);
            float dxp, dyp, dxm, dym;

            lock (p) {
                do {
                    var mo = MmToPixel(ex * tmpgrid, Dpi).CanvasToControl(zoom);

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
        }

        #endregion

        #region Items selbst

        var (childScale, childOffsetX, childOffsetY) = AlterView(positionControl, zoom, offsetX, offsetY, AutoZoomFit, UsedAreaOfItems());

        List<AbstractPadItem> snapshot;
        lock (_itemLock) {
            snapshot = [.. _internal];
        }

        foreach (var thisItem in snapshot) {
            gr.PixelOffsetMode = PixelOffsetMode.None;
            thisItem.Draw(gr, positionControl.ToRect(), childScale, childOffsetX, childOffsetY, forPrinting);
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
                    if (i is not null) { Add(i); }
                    break;

                default:
                    Develop.DebugPrint("Tag unbekannt: " + pair.Key);
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

    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
        _usedAreaOfItemsDirty = true;
        OnPropertyChanged(e.PropertyName ?? "unknown");
    }

    private void OnItemAdded() {
        if (IsDisposed) { return; }
        _usedAreaOfItemsDirty = true;
        OnPropertyChanged("Items");
        ItemAdded?.Invoke(this, System.EventArgs.Empty);
    }

    private void OnItemRemoved() {
        ItemRemoved?.Invoke(this, System.EventArgs.Empty);
        if (IsDisposed) { return; }
        _usedAreaOfItemsDirty = true;
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
                    if (t.StartsWith("[I]", StringComparison.Ordinal)) { t = t.FromNonCritical(); }
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
                    Develop.DebugPrint("Tag unbekannt: " + pair.Key);
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

    #endregion
}