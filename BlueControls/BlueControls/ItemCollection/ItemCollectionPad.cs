// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Data;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueScript.Variables;
using static BlueBasics.Converter;
using static BlueBasics.Generic;
using MessageBox = BlueControls.Forms.MessageBox;

namespace BlueControls.ItemCollection;

public class ItemCollectionPad : ObservableCollection<BasicPadItem>, IDisposableExtended, IHasKeyName {

    #region Fields

    public const int Dpi = 300;

    public static List<BasicPadItem>? PadItemTypes;

    internal string Caption;

    /// <summary>
    /// Für automatische Generierungen, die zu schnell hintereinander kommen, ein Counter für den Dateinamen
    /// </summary>
    private readonly int _idCount;

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

    public ItemCollectionPad() : base() {
        BindingOperations.EnableCollectionSynchronization(this, new object());

        PadItemTypes ??= GetInstaceOfType<BasicPadItem>("NAME");

        if (Skin.StyleDb == null) { Skin.InitStyles(); }
        SheetSizeInMm = Size.Empty;
        RandinMm = Padding.Empty;
        Caption = string.Empty;
        _idCount++;
        KeyName = "#" + DateTime.UtcNow.ToString(Constants.Format_Date) + _idCount; // # ist die erkennung, dass es kein Dateiname sondern ein Item ist
        if (Skin.StyleDb == null) { Skin.InitStyles(); }
        _sheetStyle = null;
        _sheetStyleScale = 1f;
        if (Skin.StyleDb != null) { _sheetStyle = Skin.StyleDb.Row.First(); }

        Connections.CollectionChanged += ConnectsTo_CollectionChanged;
    }

    public ItemCollectionPad(string layoutId, DatabaseAbstract database, long rowkey) : this(database.Layouts[database.Layouts.LayoutIdToIndex(layoutId)], string.Empty) {
        // Wenn nur die Row ankommt und diese null ist, kann gar nix generiert werden
        _ = ResetVariables();
        ParseVariable(database.Row.SearchByKey(rowkey));

        Connections.CollectionChanged += ConnectsTo_CollectionChanged;
    }

    public ItemCollectionPad(RowItem r, int index) : this(r.Database.Layouts[index], string.Empty) {
        // Wenn nur die Row ankommt und diese null ist, kann gar nix generiert werden
        _ = ResetVariables();
        ParseVariable(r);

        Connections.CollectionChanged += ConnectsTo_CollectionChanged;
    }

    public ItemCollectionPad(string toParse, string useThisKeyName) : this() {
        if (string.IsNullOrEmpty(toParse) || toParse.Length < 3) { return; }
        if (toParse.Substring(0, 1) != "{") { return; }// Alte Daten gehen eben verloren.
        KeyName = useThisKeyName;
        foreach (var pair in toParse.GetAllTags()) {
            switch (pair.Key.ToLower()) {
                case "sheetsize":
                    _sheetSizeInMm = pair.Value.SizeFParse();
                    GenPoints();
                    break;

                case "printarea":
                    _randinMm = pair.Value.PaddingParse();
                    GenPoints();
                    break;
                //case "items":
                //    Parse(pvalue);
                //    break;
                case "relation": // TODO: Entfernt, 24.05.2021
                    //AllRelations.Add(new clsPointRelation(this, null, pair.Value));
                    break;

                case "caption":
                    Caption = pair.Value.FromNonCritical();
                    break;

                case "backcolor":
                    BackColor = Color.FromArgb(IntParse(pair.Value));
                    break;

                //case "scriptname":
                //    _scriptName = pair.Value;
                //    break;
                case "keyname":
                case "id":
                    if (string.IsNullOrEmpty(KeyName)) { KeyName = pair.Value.FromNonCritical(); }
                    break;

                case "style":
                    _sheetStyle = Skin.StyleDb?.Row[pair.Value];
                    _sheetStyle ??= Skin.StyleDb?.Row.First();// Einfach die Erste nehmen
                    break;

                case "fontscale":
                    _sheetStyleScale = FloatParse(pair.Value);
                    break;

                case "snapmode":
                    _snapMode = (SnapMode)IntParse(pair.Value);
                    break;

                case "grid":
                    //_Grid = pair.Value.FromPlusMinus();
                    break;

                case "gridshow":
                    _gridShow = FloatParse(pair.Value);
                    break;

                case "gridsnap":
                    _gridsnap = FloatParse(pair.Value);
                    break;

                case "format": //_Format = DirectCast(Integer.Parse(pair.Value.Value), DataFormat)
                    break;

                case "items":
                    CreateItems(pair.Value); // im ersten Step nur die items erzeugen....
                    ParseItems(pair.Value);// ...im zweiten können nun auch Beziehungen erstellt werden!

                    break;

                case "connection":
                    CreateConnection(pair.Value);
                    break;

                case "connections": // TODO: Obsolet ab 07.02.2023
                    ParseConnections(pair.Value);
                    break;

                case "dpi":
                    if (IntParse(pair.Value) != Dpi) {
                        Develop.DebugPrint("Dpi Unterschied: " + Dpi + " <> " + pair.Value);
                    }
                    break;

                case "sheetstyle":
                    if (Skin.StyleDb == null) { Skin.InitStyles(); }
                    _sheetStyle = Skin.StyleDb?.Row[pair.Value];
                    break;

                case "sheetstylescale":
                    _sheetStyleScale = FloatParse(pair.Value);
                    break;

                default:
                    Develop.DebugPrint(FehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                    break;
            }
        }
    }

    #endregion

    #region Destructors

    /// <summary>
    ///
    /// </summary>
    /// <param name="toParse"></param>
    /// <param name="useThisId">Wenn das Blatt bereits eine Id hat, muss die Id verwendet werden. Wird das Feld leer gelassen, wird die beinhaltete Id benutzt.</param>
    ~ItemCollectionPad() {
        Dispose(false);
    }

    #endregion

    #region Events

    public event EventHandler? Changed;

    public event EventHandler<ListEventArgs>? ItemAdded;

    public event EventHandler? ItemRemoved;

    public event EventHandler<ListEventArgs>? ItemRemoving;

    #endregion

    #region Properties

    public Color BackColor { get; set; } = Color.White;
    public ObservableCollection<ItemConnection> Connections { get; } = new();

    [DefaultValue(10.0)]
    public float GridShow {
        get => _gridShow;
        set {
            if (Math.Abs(_gridShow - value) < 0.00001) { return; }
            _gridShow = value;
            OnChanged();
        }
    }

    [DefaultValue(10.0)]
    public float GridSnap {
        get => _gridsnap;
        set {
            if (Math.Abs(_gridsnap - value) < 0.00001) { return; }
            _gridsnap = value;
            OnChanged();
        }
    }

    public bool IsDisposed { get; private set; }

    [DefaultValue(true)]
    public bool IsSaved { get; set; }

    public string KeyName { get; set; }

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
            if (Math.Abs(value.Width - _sheetSizeInMm.Width) < 0.0001 &&
                Math.Abs(value.Height - _sheetSizeInMm.Height) < 0.0001) { return; }
            _sheetSizeInMm = new SizeF(value.Width, value.Height);
            GenPoints();
            OnChanged();
        }
    }

    public SizeF SheetSizeInPix {
        get {
            if (_prRu == null || _prLo == null || _prRu == null || _prLo == null) {
                return SizeF.Empty;
            }

            return new(_prRu.X - _prLo.X, _prRu.Y - _prLo.Y);
        }
    }

    public RowItem? SheetStyle {
        get => _sheetStyle;
        set {
            if (_sheetStyle == value) { return; }
            _sheetStyle = value;
            ApplyDesignToItems();
            OnChanged();
        }
    }

    [DefaultValue(1.0)]
    public float SheetStyleScale {
        get => _sheetStyleScale;
        set {
            if (value < 0.1d) { value = 0.1f; }
            if (Math.Abs(_sheetStyleScale - value) < 0.0001) { return; }
            _sheetStyleScale = value;
            ApplyDesignToItems();
            OnChanged();
        }
    }

    [DefaultValue(false)]
    public SnapMode SnapMode {
        get => _snapMode;
        set {
            if (_snapMode == value) { return; }
            _snapMode = value;
            OnChanged();
        }
    }

    #endregion

    #region Indexers

    public BasicPadItem? this[string @internal] {
        get {
            if (string.IsNullOrEmpty(@internal)) {
                return null;
            }

            return this.FirstOrDefault(thisItem => thisItem != null && string.Equals(@internal, thisItem.KeyName, StringComparison.OrdinalIgnoreCase));
        }
    }

    public List<BasicPadItem> this[int x, int y] => this[new Point(x, y)];

    public List<BasicPadItem> this[Point p] => this.Where(thisItem => thisItem != null && thisItem.Contains(p, 1)).ToList();

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

    public static PointF SliderValues(RectangleF bounds, float zoomToUse, Point topLeftPos) => new((float)((bounds.Left * zoomToUse) - (topLeftPos.X / 2d)),
        (float)((bounds.Top * zoomToUse) - (topLeftPos.Y / 2d)));

    public static float ZoomFitValue(RectangleF maxBounds, Size sizeOfPaintArea) {
        if (maxBounds.IsEmpty) { return 1f; }

        return Math.Min(sizeOfPaintArea.Width / maxBounds.Width,
            sizeOfPaintArea.Height / maxBounds.Height);
    }

    public new void Add(BasicPadItem? item) {
        if (item == null) { Develop.DebugPrint(FehlerArt.Fehler, "Item ist null"); return; }
        if (Contains(item)) { Develop.DebugPrint(FehlerArt.Fehler, "Bereits vorhanden!"); return; }
        if (this[item.KeyName] != null) { Develop.DebugPrint(FehlerArt.Warnung, "Name bereits vorhanden: " + item.KeyName); return; }

        if (string.IsNullOrEmpty(item.KeyName)) { Develop.DebugPrint(FehlerArt.Fehler, "Item ohne Namen!"); return; }

        base.Add(item);
        item.Parent = this;

        IsSaved = false;
        OnItemAdded(item);
        item.Changed += Item_Changed;
        //item.CompareKeyChanged += Item_CompareKeyChangedChanged;
        //item.CheckedChanged += Item_CheckedChanged;
        //item.CompareKeyChanged += Item_CompareKeyChangedChanged;
    }

    public List<string> AllPages() {
        var p = new List<string>();

        foreach (var thisp in this) {
            if (!string.IsNullOrEmpty(thisp.Page)) {
                _ = p.AddIfNotExists(thisp.Page);
            }
        }

        return p;
    }

    public new void Clear() {
        var l = new List<BasicPadItem>(this);

        foreach (var thisit in l) {
            Remove(thisit);
        }

        base.Clear();
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public bool DrawCreativePadTo(Graphics gr, Size sizeOfParentControl, States state, float zoom, float shiftX, float shiftY, string seite, bool showinprintmode) {
        try {
            gr.PixelOffsetMode = PixelOffsetMode.None;

            #region Hintergrund und evtl. Zeichenbereich

            if (SheetSizeInMm.Width > 0 && SheetSizeInMm.Height > 0) {
                //Skin.Draw_Back(gr, enDesign.Table_And_Pad, state, DisplayRectangle, this, true);
                var ssw = (float)Math.Round(MmToPixel(SheetSizeInMm.Width, Dpi), 1);
                var ssh = (float)Math.Round(MmToPixel(SheetSizeInMm.Height, Dpi), 1);
                var lo = new PointM(0f, 0f).ZoomAndMove(zoom, shiftX, shiftY);
                var ru = new PointM(ssw, ssh).ZoomAndMove(zoom, shiftX, shiftY);

                if (BackColor.A > 0) {
                    Rectangle r = new((int)lo.X, (int)lo.Y, (int)(ru.X - lo.X), (int)(ru.Y - lo.Y));
                    gr.FillRectangle(new SolidBrush(BackColor), r);
                }

                if (!showinprintmode && _prLo != null && _prRu != null) {
                    var rLo = new PointM(_prLo.X, _prLo.Y).ZoomAndMove(zoom, shiftX, shiftY);
                    var rRu = new PointM(_prRu.X, _prRu.Y).ZoomAndMove(zoom, shiftX, shiftY);
                    Rectangle rr = new((int)rLo.X, (int)rLo.Y, (int)(rRu.X - rLo.X),
                        (int)(rRu.Y - rLo.Y));
                    gr.DrawRectangle(ZoomPad.PenGray, rr);
                }
            } else {
                if (BackColor.A > 0) {
                    gr.Clear(BackColor);
                }
            }

            #endregion

            #region Grid

            if (_gridShow > 0.1) {
                var po = new PointM(0, 0).ZoomAndMove(zoom, shiftX, shiftY);
                var mo = MmToPixel(_gridShow, Dpi) * zoom;

                var p = new Pen(Color.FromArgb(10, 0, 0, 0));
                float ex = 0;

                for (var z = 0; z < 20; z++) {
                    if (mo < 5) { mo *= 2; }
                }

                if (mo >= 5) {
                    do {
                        gr.DrawLine(p, po.X + (int)ex, 0, po.X + (int)ex, sizeOfParentControl.Height);
                        gr.DrawLine(p, 0, po.Y + (int)ex, sizeOfParentControl.Width, po.Y + (int)ex);

                        if (ex > 0) {
                            // erste Linie nicht doppelt zeichnen
                            gr.DrawLine(p, po.X - (int)ex, 0, po.X - (int)ex, sizeOfParentControl.Height);
                            gr.DrawLine(p, 0, po.Y - (int)ex, sizeOfParentControl.Width, po.Y - (int)ex);
                        }
                        ex += mo;
                        if (po.X - ex < 0 && po.Y - ex < 0 && po.X + ex > sizeOfParentControl.Width && po.Y + ex > sizeOfParentControl.Height) {
                            break;
                        }
                    } while (true);
                }
            }

            #endregion

            #region Items selbst

            if (!DrawItems(gr, zoom, shiftX, shiftY, sizeOfParentControl, showinprintmode, seite)) {
                return DrawCreativePadTo(gr, sizeOfParentControl, state, zoom, shiftX, shiftY, seite, showinprintmode);
            }

            #endregion
        } catch {
            return DrawCreativePadTo(gr, sizeOfParentControl, state, zoom, shiftX, shiftY, seite, showinprintmode);
        }
        return true;
    }

    public void DrawCreativePadToBitmap(Bitmap? bmp, States vState, float zoomf, float x, float y, string seite) {
        if (bmp == null) { return; }
        var gr = Graphics.FromImage(bmp);
        _ = DrawCreativePadTo(gr, bmp.Size, vState, zoomf, x, y, seite, true);
        gr.Dispose();
    }

    public void EineEbeneNachHinten(BasicPadItem bpi) {
        var i2 = Previous(bpi);
        if (i2 != null) {
            var tempVar = bpi;
            Swap(IndexOf(tempVar), IndexOf(i2));
        }
    }

    public void EineEbeneNachVorne(BasicPadItem bpi) {
        var i2 = Next(bpi);
        if (i2 != null) {
            var tempVar = bpi;
            Swap(IndexOf(tempVar), IndexOf(i2));
        }
    }

    public void OnChanged() {
        IsSaved = false;
        Changed?.Invoke(this, System.EventArgs.Empty);
    }

    public bool ParseVariable(string name, string wert) => ParseVariable(new VariableString(name, wert));

    public bool ParseVariable(Variable variable) {
        var did = false;
        foreach (var thisItem in this) {
            if (thisItem is ICanHaveVariablesItemLevel variables) {
                if (variables.ReplaceVariable(variable)) { did = true; }
            }
        }
        return did;
    }

    public void ParseVariable(RowItem? row) {
        if (row == null) { return; }

        var script = row.ExecuteScript(EventTypes.export, string.Empty, false, false, true, 0);
        if (!script.AllOk || script.Variables == null) { return; }
        foreach (var thisV in script.Variables) {
            _ = ParseVariable(thisV);
        }
    }

    public void Remove(string internalname) => Remove(this[internalname]);

    public new void Remove(BasicPadItem? item) {
        if (item == null || !Contains(item)) { return; }
        item.Changed -= Item_Changed;
        //item.CheckedChanged -= Item_CheckedChanged;
        //item.CompareKeyChanged -= Item_CompareKeyChangedChanged;
        OnItemRemoving(item);
        _ = base.Remove(item);
        OnItemRemoved();

        if (!string.IsNullOrEmpty(item.Gruppenzugehörigkeit)) {
            foreach (var thisToo in this) {
                if (item.Gruppenzugehörigkeit.ToLower() == thisToo.Gruppenzugehörigkeit.ToLower()) {
                    Remove(thisToo);
                    return; // Wird eh eine Kettenreaktion ausgelöst -  und der Iteraor hier wird beschädigt
                }
            }
        }

        OnChanged();
    }

    public void RemoveRange(List<BasicPadItem> remove) {
        foreach (var thisItem in remove) {
            Remove(thisItem);
        }
    }

    public bool ResetVariables() {
        var did = false;
        foreach (var thisItem in this) {
            if (thisItem is ICanHaveVariablesItemLevel variables) {
                if (variables.ResetVariables()) { did = true; }
            }
        }
        if (did) { OnChanged(); }
        return did;
    }

    public void SaveAsBitmap(string filename) {
        var i = ToBitmap(1);
        if (i == null) { return; }
        switch (filename.FileSuffix().ToUpper()) {
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
                MessageBox.Show("Dateiformat unbekannt: " + filename.FileSuffix().ToUpper(), ImageCode.Warnung, "OK");
                return;
        }
    }

    public void Swap(int index1, int index2) {
        if (index1 == index2) { return; }
        //var l = ItemOrder.ToList();
        (this[index1], this[index2]) = (this[index2], this[index1]);
        //_maxNeededItemSize = Size.Empty;
        //_itemOrder = null;
        //OnChanged();
    }

    public Bitmap? ToBitmap(float scale) {
        var r = MaxBounds(null);
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
        if (!DrawCreativePadTo(gr, I.Size, States.Standard, scale, r.Left * scale, r.Top * scale, string.Empty, true)) {
            return ToBitmap(scale);
        }

        return I;
    }

    public new string ToString() {
        List<string> result = new();
        result.ParseableAdd("ID", KeyName);
        result.ParseableAdd("Caption", Caption);
        result.ParseableAdd("Style", SheetStyle?.CellFirstString());

        result.ParseableAdd("BackColor", BackColor.ToArgb());
        if (Math.Abs(SheetStyleScale - 1) > 0.001d) {
            result.ParseableAdd("FontScale", SheetStyleScale);
        }

        if (SheetSizeInMm.Width > 0 && SheetSizeInMm.Height > 0) {
            result.ParseableAdd("SheetSize", SheetSizeInMm);
            result.ParseableAdd("PrintArea", RandinMm.ToString());
        }

        result.ParseableAdd("Items", "Item", this.ToList());

        //result.ParseableAdd("ScriptName", _scriptName);

        result.ParseableAdd("SnapMode", _snapMode);
        result.ParseableAdd("GridShow", _gridShow);
        result.ParseableAdd("GridSnap", _gridsnap);

        var tmp = result.Parseable();

        result.Clear();
        foreach (var thisitem in this) {
            foreach (var thisCon in Connections) {
                if (thisCon.Item1 != null && thisCon.Item2 != null) {
                    result.ParseableAdd("Connection", thisCon.ToString());
                }
            }
        }

        return result.Parseable(tmp);
    }

    internal Rectangle DruckbereichRect() =>
                _prLo == null || _prRu == null ? Rectangle.Empty :
                                                 new Rectangle((int)_prLo.X, (int)_prLo.Y, (int)(_prRu.X - _prLo.X), (int)(_prRu.Y - _prLo.Y));

    internal int GetFreeColorId(string page) {
        var usedids = new List<int>();

        foreach (var thisIt in this) {
            if (thisIt != null && thisIt is IItemSendSomething hci && thisIt.IsVisibleOnPage(page))

                usedids.Add(hci.OutputColorId);
        }

        for (var c = 0; c < 9999; c++) {
            if (!usedids.Contains(c)) { return c; }
        }
        return -1;
    }

    internal void InDenHintergrund(BasicPadItem thisItem) {
        if (IndexOf(thisItem) == 0) { return; }
        var g1 = thisItem.Gruppenzugehörigkeit;
        thisItem.Gruppenzugehörigkeit = string.Empty;
        Remove(thisItem);
        Insert(0, thisItem);
        thisItem.Gruppenzugehörigkeit = g1;
    }

    internal void InDenVordergrund(BasicPadItem thisItem) {
        if (IndexOf(thisItem) == Count - 1) { return; }
        var g1 = thisItem.Gruppenzugehörigkeit;
        thisItem.Gruppenzugehörigkeit = string.Empty;
        Remove(thisItem);
        Add(thisItem);
        thisItem.Gruppenzugehörigkeit = g1;
    }

    internal RectangleF MaxBounds(List<BasicPadItem>? zoomItems) {
        var r = MaximumBounds(zoomItems);
        if (SheetSizeInMm.Width > 0 && SheetSizeInMm.Height > 0) {
            var x1 = Math.Min(r.Left, 0);
            var y1 = Math.Min(r.Top, 0);
            var x2 = Math.Max(r.Right, MmToPixel(SheetSizeInMm.Width, Dpi));
            var y2 = Math.Max(r.Bottom, MmToPixel(SheetSizeInMm.Height, Dpi));
            return new RectangleF(x1, y1, x2 - x1, y2 - y1);
        }
        return r;
    }

    internal BasicPadItem? Next(BasicPadItem bpi) {
        var itemCount = IndexOf(bpi);
        if (itemCount < 0) { Develop.DebugPrint(FehlerArt.Fehler, "Item im SortDefinition nicht enthalten"); }
        do {
            itemCount++;
            if (IsSaved || itemCount >= Count) { return null; }
            if (this[itemCount] != null) { return this[itemCount]; }
        } while (true);
    }

    internal BasicPadItem? Previous(BasicPadItem bpi) {
        var itemCount = IndexOf(bpi);
        if (itemCount < 0) { Develop.DebugPrint(FehlerArt.Fehler, "Item im SortDefinition nicht enthalten"); }
        do {
            itemCount--;
            if (IsSaved || itemCount < 0) { return null; }
            if (this[itemCount] != null) { return this[itemCount]; }
        } while (true);
    }

    protected RectangleF MaxBounds() => MaxBounds(null);

    protected virtual void OnItemAdded(BasicPadItem item) {
        ItemAdded?.Invoke(this, new ListEventArgs(item));
        OnChanged();
    }

    protected virtual void OnItemRemoved() {
        ItemRemoved?.Invoke(this, System.EventArgs.Empty);
        OnChanged();
    }

    protected virtual void OnItemRemoving(BasicPadItem item) => ItemRemoving?.Invoke(this, new ListEventArgs(item));

    private void ApplyDesignToItems() {
        foreach (var thisItem in this) {
            thisItem?.ProcessStyleChange();
        }
        OnChanged();
    }

    private void ConnectsTo_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
        if (e.NewItems != null) {
            foreach (var thisit in e.NewItems) {
                if (thisit is ItemConnection x) {
                    x.Item2.Changed += Item_Changed;
                }
            }
        }

        if (e.OldItems != null) {
            foreach (var thisit in e.OldItems) {
                if (thisit is ItemConnection x) {
                    x.Item2.Changed -= Item_Changed;
                }
            }
        }

        if (e.Action == NotifyCollectionChangedAction.Reset) {
            Develop.DebugPrint_NichtImplementiert();
        }

        OnChanged();
    }

    private void CreateConnection(string toParse) {
        if (toParse.StartsWith("[I]")) { toParse = toParse.FromNonCritical(); }

        var x = toParse.GetAllTags();

        BasicPadItem? item1 = null;
        BasicPadItem? item2 = null;
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
            switch (pair.Key.ToLower()) {
                case "dpi":
                case "sheetstylescale":
                case "sheetstyle":
                    break;

                case "item":
                    var i = ParsebleItem.NewByParsing<BasicPadItem>(pair.Value);
                    if (i != null) { Add(i); }
                    break;

                default:
                    Develop.DebugPrint(FehlerArt.Warnung, "Tag unbekannt: " + pair.Key);
                    break;
            }
        }
    }

    private void Dispose(bool disposing) {
        IsDisposed = true;
        foreach (var thisIt in this) {
            thisIt.Changed -= Item_Changed;
            thisIt.Dispose();
        }

        if (disposing) {
            _sheetStyle = null;
        }

        Connections.CollectionChanged -= ConnectsTo_CollectionChanged;
        Connections.RemoveAll();
    }

    private bool DrawItems(Graphics gr, float zoom, float shiftX, float shiftY, Size sizeOfParentControl, bool forPrinting, string seite) {
        try {
            if (SheetStyle == null || SheetStyleScale < 0.1d) { return true; }

            foreach (var thisItem in this) {
                if (thisItem != null) {
                    gr.PixelOffsetMode = PixelOffsetMode.None;
                    if (string.IsNullOrEmpty(seite) || thisItem.Page.Equals(seite, StringComparison.OrdinalIgnoreCase)) {
                        thisItem.Draw(gr, zoom, shiftX, shiftY, sizeOfParentControl, forPrinting);
                    }
                }
            }

            return true;
        } catch {
            CollectGarbage();
            return false;
        }
    }

    private void GenPoints() {
        if (Math.Abs(_sheetSizeInMm.Width) < 0.001 || Math.Abs(_sheetSizeInMm.Height) < 0.001) {
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

        var ssw = (float)Math.Round(MmToPixel(_sheetSizeInMm.Width, Dpi), 1);
        var ssh = (float)Math.Round(MmToPixel(_sheetSizeInMm.Height, Dpi), 1);
        var rr = (float)Math.Round(MmToPixel(_randinMm.Right, Dpi), 1);
        var rl = (float)Math.Round(MmToPixel(_randinMm.Left, Dpi), 1);
        var ro = (float)Math.Round(MmToPixel(_randinMm.Top, Dpi), 1);
        var ru = (float)Math.Round(MmToPixel(_randinMm.Bottom, Dpi), 1);
        _prLo.SetTo(rl, ro);
        _prRo.SetTo(ssw - rr, ro);
        _prRu.SetTo(ssw - rr, ssh - ru);
        _prLu.SetTo(rl, ssh - ru);
    }

    private void Item_Changed(object sender, System.EventArgs e) => OnChanged();

    private RectangleF MaximumBounds(ICollection<BasicPadItem>? zoomItems) {
        var x1 = float.MaxValue;
        var y1 = float.MaxValue;
        var x2 = float.MinValue;
        var y2 = float.MinValue;
        var done = false;
        foreach (var thisItem in this) {
            if (thisItem != null) {
                if (zoomItems == null || zoomItems.Contains(thisItem)) {
                    var ua = thisItem.ZoomToArea();
                    x1 = Math.Min(x1, ua.Left);
                    y1 = Math.Min(y1, ua.Top);
                    x2 = Math.Max(x2, ua.Right);
                    y2 = Math.Max(y2, ua.Bottom);
                    done = true;
                }
            }
        }
        return !done ? RectangleF.Empty : new RectangleF(x1, y1, x2 - x1, y2 - y1);
    }

    private void ParseConnections(string toParse) {
        if (toParse.StartsWith("[I]")) { toParse = toParse.FromNonCritical(); }

        foreach (var pair in toParse.GetAllTags()) {
            switch (pair.Key.ToLower()) {
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
            switch (pair.Key.ToLower()) {
                case "dpi":
                case "sheetstyle":
                case "sheetstylescale":
                    break;

                case "item":
                    var x = pair.Value.GetAllTags();
                    foreach (var thisIt in x) {
                        switch (thisIt.Key) {
                            case "internalname":
                                var it = this[thisIt.Value];
                                it?.Parse(x);
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