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

//TODO: IParseable implementieren
public sealed class ItemCollectionPad : ObservableCollection<AbstractPadItem>, IDisposableExtended, IHasKeyName, IStringable, ICanHaveVariables {

    #region Fields

    public const int Dpi = 300;

    public static List<AbstractPadItem>? PadItemTypes;

    public new EventHandler? PropertyChanged;
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

        PadItemTypes ??= GetInstaceOfType<AbstractPadItem>("NAME");

        if (Skin.StyleDb == null) { Skin.InitStyles(); }
        SheetSizeInMm = Size.Empty;
        RandinMm = Padding.Empty;
        _idCount++;
        Caption = "#" + DateTime.UtcNow.ToString1() + _idCount; // # ist die erkennung, dass es kein Dateiname sondern ein Item ist
        if (Skin.StyleDb == null) { Skin.InitStyles(); }
        _sheetStyle = null;
        _sheetStyleScale = 1f;
        if (Skin.StyleDb != null) { _sheetStyle = Skin.StyleDb.Row.First(); }

        Connections.CollectionChanged += ConnectsTo_CollectionChanged;
    }

    public ItemCollectionPad(string layoutFileName) : this() {
        Parse(File.ReadAllText(layoutFileName, Win1252));
        IsSaved = true;

        Connections.CollectionChanged += ConnectsTo_CollectionChanged;
    }

    #endregion

    #region Destructors

    ~ItemCollectionPad() {
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

    public ObservableCollection<ItemConnection> Connections { get; } = [];

    /// <summary>
    /// in mm
    /// </summary>
    [DefaultValue(10.0)]
    public float GridShow {
        get => _gridShow;
        set {
            if (IsDisposed) { return; }
            if (Math.Abs(_gridShow - value) < FineTolerance) { return; }
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
            if (Math.Abs(_gridsnap - value) < FineTolerance) { return; }
            _gridsnap = value;
            OnPropertyChanged();
        }
    }

    public bool IsDisposed { get; private set; }

    [DefaultValue(true)]
    public bool IsSaved { get; set; }

    public string KeyName => Caption;

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
            if (Math.Abs(value.Width - _sheetSizeInMm.Width) < FineTolerance &&
                Math.Abs(value.Height - _sheetSizeInMm.Height) < FineTolerance) { return; }
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

    #endregion

    #region Indexers

    public AbstractPadItem? this[string keyName] => this.Get(keyName);

    public List<AbstractPadItem> this[int x, int y] => this[new Point(x, y)];

    public List<AbstractPadItem> this[Point p] => this.Where(thisItem => thisItem != null && thisItem.Contains(p, 1)).ToList();

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

    public new void Add(AbstractPadItem? item) {
        if (item == null) { Develop.DebugPrint(FehlerArt.Fehler, "Item ist null"); return; }
        if (Contains(item)) { Develop.DebugPrint(FehlerArt.Fehler, "Bereits vorhanden!"); return; }
        if (this[item.KeyName] != null) { Develop.DebugPrint(FehlerArt.Warnung, "Name bereits vorhanden: " + item.KeyName); return; }

        if (string.IsNullOrEmpty(item.KeyName)) { Develop.DebugPrint(FehlerArt.Fehler, "Item ohne Namen!"); return; }

        base.Add(item);
        item.Parent = this;

        IsSaved = false;
        OnItemAdded(item);
        item.AddedToCollection();
        item.PropertyChanged += Item_PropertyChanged;
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
        var l = new List<AbstractPadItem>(this);

        foreach (var thisit in l) {
            Remove(thisit);
        }

        base.Clear();
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public bool DrawCreativePadTo(Graphics gr, List<AbstractPadItem> items, float zoom, float shiftX, float shiftY, Size sizeOfParentControl, bool showinprintmode, States state) {
        try {
            gr.PixelOffsetMode = PixelOffsetMode.None;

            #region Hintergrund und evtl. Zeichenbereich

            if (_prLo != null && _prRu != null) {
                if (BackColor.A > 0) {
                    var p = SheetSizeInPix;
                    var rLo2 = new PointM(0, 0).ZoomAndMove(zoom, shiftX, shiftY);
                    var rRu2 = new PointM(p.Width, p.Height).ZoomAndMove(zoom, shiftX, shiftY);
                    Rectangle rr2 = new((int)rLo2.X, (int)rLo2.Y, (int)(rRu2.X - rLo2.X), (int)(rRu2.Y - rLo2.Y));

                    gr.FillRectangle(new SolidBrush(BackColor), rr2);
                    if (!showinprintmode) { gr.DrawRectangle(ZoomPad.PenGray, rr2); }
                }

                var rLo = _prLo.ZoomAndMove(zoom, shiftX, shiftY);
                var rRu = _prRu.ZoomAndMove(zoom, shiftX, shiftY);
                Rectangle rr = new((int)rLo.X, (int)rLo.Y, (int)(rRu.X - rLo.X), (int)(rRu.Y - rLo.Y));
                if (!showinprintmode) { gr.DrawRectangle(ZoomPad.PenGray, rr); }
            } else {
                if (BackColor.A > 0) { gr.Clear(BackColor); }
            }

            #endregion

            #region Grid

            if (_gridShow > 0.1) {
                var po = new PointM(0, 0).ZoomAndMove(zoom, shiftX, shiftY);

                var tmpgrid = _gridShow;

                while (MmToPixel(tmpgrid, Dpi) * zoom < 5) { tmpgrid *= 2; }

                var p = new Pen(Color.FromArgb(10, 0, 0, 0));
                float ex = 0;

                do {
                    var mo = MmToPixel(ex * tmpgrid, Dpi) * zoom;

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

            if (!DrawItems(gr, items, zoom, shiftX, shiftY, sizeOfParentControl, showinprintmode)) {
                return DrawCreativePadTo(gr, items, zoom, shiftX, shiftY, sizeOfParentControl, showinprintmode, state);
            }

            #endregion
        } catch {
            Develop.CheckStackForOverflow();
            return DrawCreativePadTo(gr, items, zoom, shiftX, shiftY, sizeOfParentControl, showinprintmode, state);
        }
        return true;
    }

    public void DrawCreativePadToBitmap(Bitmap? bmp, States vState, float zoomf, float x, float y, string page) {
        if (bmp == null) { return; }
        var gr = Graphics.FromImage(bmp);
        _ = DrawCreativePadTo(gr, ItemsOnPage(page), zoomf, x, y, bmp.Size, true, vState);
        gr.Dispose();
    }

    public void EineEbeneNachHinten(AbstractPadItem bpi) {
        var i2 = Previous(bpi);
        if (i2 != null) {
            Swap(IndexOf(bpi), IndexOf(i2));
        }
    }

    public void EineEbeneNachVorne(AbstractPadItem bpi) {
        var i2 = Next(bpi);
        if (i2 != null) {
            Swap(IndexOf(bpi), IndexOf(i2));
        }
    }

    public List<AbstractPadItem> ItemsOnPage(string page) {
        var l = new List<AbstractPadItem>();

        foreach (var thisItem in this) {
            if (thisItem != null) {
                if (string.IsNullOrEmpty(page) || thisItem.Page.Equals(page, StringComparison.OrdinalIgnoreCase)) {
                    l.Add(thisItem);
                }
            }
        }

        return l;
    }

    public void OnPropertyChanged() {
        IsSaved = false;
        PropertyChanged?.Invoke(this, System.EventArgs.Empty);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="toParse"></param>
    public void Parse(string? toParse) {
        if (toParse == null || string.IsNullOrEmpty(toParse) || toParse.Length < 3) { return; }
        if (toParse.Substring(0, 1) != "{") { return; }// Alte Daten gehen eben verloren.
        //Caption = useThisKeyName;
        foreach (var pair in toParse.GetAllTags()) {
            switch (pair.Key.ToLowerInvariant()) {
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
                    if (string.IsNullOrEmpty(Caption)) { Caption = pair.Value.FromNonCritical(); }
                    break;

                case "backcolor":
                    BackColor = Color.FromArgb(IntParse(pair.Value));
                    break;

                //case "scriptname":
                //    _scriptName = pair.Value;
                //    break;
                case "keyName":
                case "id":
                    //if (string.IsNullOrEmpty(KeyName)) { KeyName = pair.Value.FromNonCritical(); }
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

    //public List<string> Permission_AllUsed() {
    //    var l = new List<string>();

    //    foreach (var thisIt in this) {
    //        if (thisIt is FakeControlPadItem csi) {
    //            l.AddRange(csi.VisibleFor);
    //        }
    //    }

    //    l.Add(Everybody);
    //    l.Add("#User: " + UserName);

    //    l = RepairUserGroups(l);

    //    return l.SortedDistinctList();
    //}

    public void Remove(string keyName) => Remove(this[keyName]);

    public new void Remove(AbstractPadItem? item) {
        if (IsDisposed) { return; }
        if (item == null || !Contains(item)) { return; }
        item.PropertyChanged -= Item_PropertyChanged;
        //item.CheckedChanged -= Item_CheckedChanged;
        //item.CompareKeyChanged -= Item_CompareKeyChangedChanged;
        OnItemRemoving(item);
        _ = base.Remove(item);
        OnItemRemoved();

        if (!string.IsNullOrEmpty(item.Gruppenzugehörigkeit)) {
            foreach (var thisToo in this) {
                if (string.Equals(item.Gruppenzugehörigkeit, thisToo.Gruppenzugehörigkeit, StringComparison.OrdinalIgnoreCase)) {
                    Remove(thisToo);
                    return; // Wird eh eine Kettenreaktion ausgelöst -  und der Iteraor hier wird beschädigt
                }
            }
        }

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
        foreach (var thisItem in this) {
            if (thisItem is ICanHaveVariables variables) {
                if (variables.ReplaceVariable(variable)) { did = true; }
            }
        }
        return did;
    }

    public ScriptEndedFeedback ReplaceVariables(RowItem? row) {
        if (row == null || row.IsDisposed) { return new ScriptEndedFeedback("Keine Zeile angekommen", false, false, "Export"); }

        var script = row.ExecuteScript(ScriptEventTypes.export, string.Empty, false, false, true, 0, null, true, false);
        if (!script.AllOk) { return script; }

        this.ParseVariables(script.Variables);

        return script;
    }

    public ScriptEndedFeedback ReplaceVariables(Database database, string rowkey) => ReplaceVariables(database.Row.SearchByKey(rowkey));

    public bool ResetVariables() {
        if (IsDisposed) { return false; }
        var did = false;
        foreach (var thisItem in this) {
            if (thisItem is ICanHaveVariables variables) {
                if (variables.ResetVariables()) { did = true; }
            }
        }
        if (did) { OnPropertyChanged(); }
        return did;
    }

    public void SaveAsBitmap(string filename, string page) {
        var i = ToBitmap(1, page);
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
        //var l = ItemOrder.ToList();
        (this[index1], this[index2]) = (this[index2], this[index1]);
        //_maxNeededItemSize = Size.Empty;
        //_itemOrder = null;
        //OnPropertyChanged();
    }

    public Bitmap? ToBitmap(float scale, string page) {
        var l = ItemsOnPage(page);

        var r = MaxBounds(l);
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
        if (!DrawCreativePadTo(gr, l, scale, r.Left * scale, r.Top * scale, I.Size, true, States.Standard)) {
            return ToBitmap(scale, page);
        }

        return I;
    }

    public new string ToString() {
        if (IsDisposed) { return string.Empty; }
        List<string> result = [];
        //result.ParseableAdd("ID", KeyName);
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

        foreach (var thisCon in Connections) {
            if (thisCon?.Item1 != null) {
                result.ParseableAdd("Connection", thisCon.ToString());
            }
        }

        return result.Parseable(tmp);
    }

    public List<string> VisibleFor_AllUsed() {
        var l = new List<string>();

        foreach (var thisIt in this) {
            if (thisIt is FakeControlPadItem csi) {
                l.AddRange(csi.VisibleFor);
            }
        }

        l.Add(Administrator);
        l.Add(Everybody);
        l.Add("#User: " + UserName);

        l.AddRange(Database.Permission_AllUsed(false));

        return Database.RepairUserGroups(l);
    }

    internal Rectangle DruckbereichRect() =>
                _prLo == null || _prRu == null ? Rectangle.Empty :
                                                 new Rectangle((int)_prLo.X, (int)_prLo.Y, (int)(_prRu.X - _prLo.X), (int)(_prRu.Y - _prLo.Y));

    internal int GetFreeColorId(string page) {
        var usedids = new List<int>();

        foreach (var thisIt in this) {
            if (thisIt is IItemSendFilter hci && thisIt.IsOnPage(page)) {
                usedids.Add(hci.OutputColorId);
            }
        }

        for (var c = 0; c < 9999; c++) {
            if (!usedids.Contains(c)) { return c; }
        }
        return -1;
    }

    internal void InDenHintergrund(AbstractPadItem thisItem) {
        if (IndexOf(thisItem) == 0) { return; }
        var g1 = thisItem.Gruppenzugehörigkeit;
        thisItem.Gruppenzugehörigkeit = string.Empty;
        Remove(thisItem);
        Insert(0, thisItem);
        thisItem.Gruppenzugehörigkeit = g1;
    }

    internal void InDenVordergrund(AbstractPadItem thisItem) {
        if (IndexOf(thisItem) == Count - 1) { return; }
        var g1 = thisItem.Gruppenzugehörigkeit;
        thisItem.Gruppenzugehörigkeit = string.Empty;
        Remove(thisItem);
        Add(thisItem);
        thisItem.Gruppenzugehörigkeit = g1;
    }

    internal RectangleF MaxBounds(string page) => MaxBounds(ItemsOnPage(page));

    internal RectangleF MaxBounds(List<AbstractPadItem>? zoomItems) {
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

    internal AbstractPadItem? Next(AbstractPadItem bpi) {
        var itemCount = IndexOf(bpi);
        if (itemCount < 0) { Develop.DebugPrint(FehlerArt.Fehler, "Item im SortDefinition nicht enthalten"); }
        do {
            itemCount++;
            if (itemCount >= Count) { return null; }
            if (this[itemCount] != null) { return this[itemCount]; }
        } while (true);
    }

    internal AbstractPadItem? Previous(AbstractPadItem bpi) {
        var itemCount = IndexOf(bpi);
        if (itemCount < 0) { Develop.DebugPrint(FehlerArt.Fehler, "Item im SortDefinition nicht enthalten"); }
        do {
            itemCount--;
            if (itemCount < 0) { return null; }
            if (this[itemCount] != null) { return this[itemCount]; }
        } while (true);
    }

    private void ApplyDesignToItems() {
        if (IsDisposed) { return; }
        foreach (var thisItem in this) {
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

    private void Dispose(bool disposing) {
        IsDisposed = true;
        foreach (var thisIt in this) {
            thisIt.PropertyChanged -= Item_PropertyChanged;
            thisIt.Dispose();
        }

        if (disposing) {
            _sheetStyle = null;
        }

        Connections.CollectionChanged -= ConnectsTo_CollectionChanged;
        Connections.RemoveAll();
    }

    private bool DrawItems(Graphics gr, List<AbstractPadItem> items, float zoom, float shiftX, float shiftY, Size sizeOfParentControl, bool forPrinting) {
        try {
            if (SheetStyle == null || SheetStyleScale < 0.1d) { return true; }

            foreach (var thisItem in items) {
                gr.PixelOffsetMode = PixelOffsetMode.None;
                thisItem.Draw(gr, zoom, shiftX, shiftY, sizeOfParentControl, forPrinting);
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
        _prLo.SetTo(rl, ro);
        _prRo.SetTo(ssw - rr, ro);
        _prRu.SetTo(ssw - rr, ssh - ru);
        _prLu.SetTo(rl, ssh - ru);
    }

    private void Item_PropertyChanged(object sender, System.EventArgs e) => OnPropertyChanged();

    private RectangleF MaximumBounds(IEnumerable<AbstractPadItem>? zoomItems) {
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