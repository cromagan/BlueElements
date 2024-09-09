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
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad.Abstract;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using static BlueBasics.Geometry;
using PageSetupDialog = BlueControls.Forms.PageSetupDialog;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
[DefaultEvent("Click")]
public sealed partial class CreativePad : ZoomPad, IContextMenu, IPropertyChangedFeedback {

    #region Fields

    private readonly List<IMoveable> _itemsToMove = [];
    private string _currentPage = string.Empty;
    private IMouseAndKeyHandle? _givesMouseCommandsTo;
    private ItemCollectionPad.ItemCollectionPad? _items;
    private AbstractPadItem? _lastClickedItem;
    private string _lastQuickInfo = string.Empty;
    private bool _repairPrinterDataPrepaired;
    private bool _showInPrintMode;

    #endregion

    #region Constructors

    public CreativePad(ItemCollectionPad.ItemCollectionPad itemCollectionPad) : base() {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
        // Initialisierungen nach dem Aufruf InitializeComponent() hinzufügen
        Items = itemCollectionPad;
        Unselect();
        MouseHighlight = false;
    }

    public CreativePad() : this([]) { }

    #endregion

    #region Events

    public event PrintEventHandler? BeginnPrint;

    public event EventHandler? ClickedItemChanged;

    public event EventHandler? ClickedItemChanging;

    public event EventHandler<ContextMenuInitEventArgs>? ContextMenuInit;

    public event EventHandler<ContextMenuItemClickedEventArgs>? ContextMenuItemClicked;

    public event EventHandler? DrawModeChanged;

    public event PrintEventHandler? EndPrint;

    public event EventHandler? GotNewItemCollection;

    public event EventHandler<ListEventArgs>? ItemAdded;

    public event EventHandler<System.EventArgs>? ItemRemoved;

    public event EventHandler<ListEventArgs>? ItemRemoving;

    public event PrintPageEventHandler? PrintPage;

    public event EventHandler? PropertyChanged;

    #endregion

    #region Properties

    [DefaultValue(true)]
    public bool ContextMenuAllowed { get; set; } = true;

    [DefaultValue("")]
    public string CurrentPage {
        get => _currentPage;
        set {
            if (_currentPage == value) { return; }
            _currentPage = value;
            OnDrawModeChanged();
            Unselect();
        }
    }

    [DefaultValue(true)]
    public bool EditAllowed { get; set; } = true;

    public AbstractPadItem? HotItem { get; private set; }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ItemCollectionPad.ItemCollectionPad? Items {
        get => _items;
        set {
            if (_items == value) { return; }
            if (_items != null) {
                _items.ItemRemoved -= _Items_ItemRemoved;
                _items.ItemRemoving -= _Items_ItemRemoving;
                _items.ItemAdded -= _Items_ItemAdded;
                _items.PropertyChanged -= Items_PropertyChanged;
            }
            _items = value;
            if (_items != null) {
                _items.ItemRemoved += _Items_ItemRemoved;
                _items.ItemRemoving += _Items_ItemRemoving;
                _items.ItemAdded += _Items_ItemAdded;
                _items.PropertyChanged += Items_PropertyChanged;
            }
            Invalidate();
            OnGotNewItemCollection();
        }
    }

    public AbstractPadItem? LastClickedItem {
        get => _lastClickedItem;
        private set {
            if (_lastClickedItem != value) {
                OnClickedItemChanging();
                _lastClickedItem = value;
                OnClickedItemChanged();
            }
        }
    }

    [DefaultValue(false)]
    public bool ShowInPrintMode {
        get => _showInPrintMode;
        set {
            if (_showInPrintMode == value) { return; }
            _showInPrintMode = value;
            OnDrawModeChanged();
            Unselect();
        }
    }

    protected override string QuickInfoText => _lastQuickInfo;

    #endregion

    #region Methods

    public void CopyPrinterSettingsToWorkingArea() {
        if (_items is not { IsDisposed: not true }) { return; }
        if (DruckerDokument.DefaultPageSettings.Landscape) {
            _items.SheetSizeInMm = new SizeF((int)(DruckerDokument.DefaultPageSettings.PaperSize.Height * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.PaperSize.Width * 25.4 / 100));
            _items.RandinMm = new Padding((int)(DruckerDokument.DefaultPageSettings.Margins.Left * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.Margins.Top * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.Margins.Right * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.Margins.Bottom * 25.4 / 100));
        } else {
            // Hochformat
            _items.SheetSizeInMm = new SizeF((int)(DruckerDokument.DefaultPageSettings.PaperSize.Width * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.PaperSize.Height * 25.4 / 100));
            _items.RandinMm = new Padding((int)(DruckerDokument.DefaultPageSettings.Margins.Left * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.Margins.Top * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.Margins.Right * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.Margins.Bottom * 25.4 / 100));
        }
    }

    public void DoContextMenuItemClick(ContextMenuItemClickedEventArgs e) {
        AbstractPadItem? thisItem = null;

        if (e.HotItem is AbstractPadItem item) { thisItem = item; }

        if (thisItem != null) {
            switch (e.Item.KeyName.ToLowerInvariant()) {
                case "#vordergrund":
                    thisItem.Parent?.InDenVordergrund(thisItem);
                    return;

                case "#hintergrund":
                    thisItem.Parent?.InDenHintergrund(thisItem);
                    return;

                case "#vorne":
                    thisItem.Parent?.EineEbeneNachVorne(thisItem);
                    return;

                case "#hinten":
                    thisItem.Parent?.EineEbeneNachHinten(thisItem);
                    return;

                case "#duplicate":
                    var n = (AbstractPadItem)((ICloneable)thisItem).Clone();
                    n.KeyName = Generic.GetUniqueKey();
                    _items?.Add(n);
                    return;
            }
        }

        OnContextMenuItemClicked(e);
    }

    public void DoKeyUp(KeyEventArgs e, bool hasbase) {
        // Ganz seltsam: Wird BAse.OnKeyUp IMMER ausgelöst, passiert folgendes:
        // Wird ein Objekt gelöscht, wird anschließend das OnKeyUp Ereignis nicht mehr ausgelöst.
        if (hasbase) { base.OnKeyUp(e); }
        if (!EditAllowed || _items == null) { return; }
        if (_givesMouseCommandsTo != null) {
            if (_givesMouseCommandsTo.KeyUp(e, Zoom, ShiftX, ShiftY)) { return; }
        }
        var multi = 1f;
        if (_items.SnapMode == SnapMode.SnapToGrid) {
            multi = Converter.MmToPixel(_items.GridSnap, ItemCollectionPad.ItemCollectionPad.Dpi);
        }
        if (multi < 1) { multi = 1f; }
        switch (e.KeyCode) {
            case Keys.Delete:

            case Keys.Back:
                List<AbstractPadItem> itemsDoDelete = [];
                foreach (var thisit in _itemsToMove) {
                    if (thisit is AbstractPadItem bi) { itemsDoDelete.Add(bi); }
                }
                Unselect();
                _items.RemoveRange(itemsDoDelete);
                break;

            case Keys.Up:
                MoveItems(0, -1 * multi, false, false);
                break;

            case Keys.Down:
                MoveItems(0, 1 * multi, false, false);
                break;

            case Keys.Left:
                MoveItems(-1 * multi, 0, false, false);
                break;

            case Keys.Right:
                MoveItems(1 * multi, 0, false, false);
                break;
        }
    }

    public void GetContextMenuItems(ContextMenuInitEventArgs e) {
        CheckHotItem(e.Mouse, true);
        e.HotItem = HotItem;

        if (e.HotItem != null) {
            e.ContextMenu.Add(ItemOf("Allgemeine Element-Aktionen", true));
            //items.GenerateAndAdd("Objekt bearbeiten", "#Erweitert", ImageCode.Stift);
            //items.Add(AddSeparator());
            e.ContextMenu.Add(ItemOf("Objekt duplizieren", "#Duplicate", ImageCode.Kopieren, e.HotItem is ICloneable));
            e.ContextMenu.Add(Separator());
            e.ContextMenu.Add(ItemOf("In den Vordergrund", "#Vordergrund", ImageCode.InDenVordergrund));
            e.ContextMenu.Add(ItemOf("In den Hintergrund", "#Hintergrund", ImageCode.InDenHintergrund));
            e.ContextMenu.Add(ItemOf("Eine Ebene nach vorne", "#Vorne", ImageCode.EbeneNachVorne));
            e.ContextMenu.Add(ItemOf("Eine Ebene nach hinten", "#Hinten", ImageCode.EbeneNachHinten));
        }

        OnContextMenuInit(e);
    }

    public List<AbstractPadItem> HotItems(MouseEventArgs? e) {
        if (e == null || _items == null) { return []; }

        if (_givesMouseCommandsTo != null) {
            return _givesMouseCommandsTo.HotItems(e, Zoom, ShiftX, ShiftY);
        }

        Point p = new((int)((e.X + ShiftX) / Zoom), (int)((e.Y + ShiftY) / Zoom));
        return _items.Where(thisItem => thisItem != null &&
                                        thisItem.IsOnPage(CurrentPage) &&
                                        thisItem.Contains(p, Zoom)).ToList();
    }

    public void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);

    public void OnItemAdded(ListEventArgs e) => ItemAdded?.Invoke(this, e);

    public void OnItemRemoved() => ItemRemoved?.Invoke(this, System.EventArgs.Empty);

    //public void OnItemInternalChanged(ListEventArgs e) => ItemInternalChanged?.Invoke(this, e);
    public void OnItemRemoving(ListEventArgs e) => ItemRemoving?.Invoke(this, e);

    public void OnPropertyChanged() => PropertyChanged?.Invoke(this, System.EventArgs.Empty);

    public void OpenSaveDialog(string title) {
        title = title.RemoveChars(Constants.Char_DateiSonderZeichen);
        PicsSave.FileName = title + ".png";
        _ = PicsSave.ShowDialog();
    }

    public void Print() {
        DruckerDokument.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
        if (PrintDialog1.ShowDialog() == DialogResult.OK) {
            PrintDialog1.Document.Print();
        }
        RepairPrinterData();
    }

    public void SelectItem(IMoveable? item, bool additional) {
        if (!additional) { Unselect(); }
        if (item == null) { return; }
        _itemsToMove.Add(item);

        Invalidate();
    }

    public void ShowPrinterPageSetup() {
        RepairPrinterData();
        var x = PageSetupDialog.Show(DruckerDokument, false);
        if (x == null) { return; }
        DruckerDokument = x;
    }

    public void ShowPrintPreview() {
        DruckerDokument.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
        _ = PrintPreviewDialog1.ShowDialog();
        RepairPrinterData();
    }

    public void ShowWorkingAreaSetup() {
        if (_items == null) { return; }

        PrintDocument oriD = new();
        oriD.DefaultPageSettings.Landscape = false;
        oriD.DefaultPageSettings.PaperSize = new PaperSize("Benutzerdefiniert", (int)(_items.SheetSizeInMm.Width / 25.4 * 100), (int)(_items.SheetSizeInMm.Height / 25.4 * 100));
        oriD.DefaultPageSettings.Margins.Top = (int)(_items.RandinMm.Top / 25.4 * 100);
        oriD.DefaultPageSettings.Margins.Bottom = (int)(_items.RandinMm.Bottom / 25.4 * 100);
        oriD.DefaultPageSettings.Margins.Left = (int)(_items.RandinMm.Left / 25.4 * 100);
        oriD.DefaultPageSettings.Margins.Right = (int)(_items.RandinMm.Right / 25.4 * 100);
        var nOriD = PageSetupDialog.Show(oriD, true);
        if (nOriD == null) { return; }
        _items.SheetSizeInMm = new SizeF((int)(nOriD.DefaultPageSettings.PaperSize.Width * 25.4 / 100), (int)(nOriD.DefaultPageSettings.PaperSize.Height * 25.4 / 100));
        _items.RandinMm = new Padding((int)(nOriD.DefaultPageSettings.Margins.Left * 25.4 / 100), (int)(nOriD.DefaultPageSettings.Margins.Top * 25.4 / 100), (int)(nOriD.DefaultPageSettings.Margins.Right * 25.4 / 100), (int)(nOriD.DefaultPageSettings.Margins.Bottom * 25.4 / 100));
    }

    public void Unselect() {
        _itemsToMove.Clear();
        _givesMouseCommandsTo = null;
        Invalidate();
    }

    internal void AddCentered(AbstractPadItem it) {
        var pos = MiddleOfVisiblesScreen();
        var wid = (int)((Width + Zoom) * 0.8);
        var he = (int)((Height + Zoom) * 0.8);

        wid = Math.Max(wid, 10);
        he = Math.Max(he, 10);

        wid = Math.Min(wid, 200);
        he = Math.Min(he, 200);

        it.InitialPosition(pos.X - (wid / 2), pos.Y - (he / 2), wid, he);

        Items?.Add(it);
    }

    internal void DoMouseDown(MouseEventArgs e) {
        base.OnMouseDown(e);
        CheckHotItem(e, true);
        if (!EditAllowed) { return; }
        _lastQuickInfo = string.Empty;
        if (HotItem is IMouseAndKeyHandle ho2) {
            if (ho2.MouseDown(e, Zoom, ShiftX, ShiftY)) {
                _givesMouseCommandsTo = ho2;
                return;
            }
        }
        if (e.Button == MouseButtons.Left) {
            var p = KoordinatesUnscaled(e);
            if (_itemsToMove.Count > 0) {
                foreach (var thisItem in _itemsToMove) {
                    if (thisItem is AbstractPadItem bpi) {
                        foreach (var thisPoint in bpi.MovablePoint) {
                            if (Länge(thisPoint, p) < 5f / Zoom) {
                                SelectItem(thisPoint, false);
                                return;
                            }
                        }
                    }
                }
            }
            SelectItem(HotItem, ModifierKeys.HasFlag(Keys.Control));
        }
    }

    internal void DoMouseMove(MouseEventArgs e) {
        base.OnMouseMove(e);
        if (e.Button == MouseButtons.None) {
            CheckHotItem(e, false); // Für QuickInfo usw.
        }
        if (!EditAllowed) { return; }
        if (_givesMouseCommandsTo != null) {
            if (e.Button == MouseButtons.None && HotItem != _givesMouseCommandsTo) {
                _givesMouseCommandsTo = null;
                Invalidate();
            } else {
                if (!_givesMouseCommandsTo.MouseMove(e, Zoom, ShiftX, ShiftY)) {
                    _givesMouseCommandsTo = null;
                    Invalidate();
                } else {
                    return;
                }
            }
        } else {
            if (HotItem is IMouseAndKeyHandle ho2) {
                if (ho2.MouseMove(e, Zoom, ShiftX, ShiftY)) {
                    _givesMouseCommandsTo = ho2;
                    Invalidate();
                    return;
                }
            }

            _lastQuickInfo = string.Empty;
            if (HotItem != null && e.Button == MouseButtons.None) {
                if (!string.IsNullOrEmpty(HotItem.QuickInfo)) {
                    _lastQuickInfo = HotItem.QuickInfo + "<hr>" + HotItem.Description;
                } else {
                    _lastQuickInfo = HotItem.Description;
                }
            }
        }
        if (e.Button == MouseButtons.Left) {
            _lastQuickInfo = string.Empty;
            MoveItemsWithMouse();
            Refresh(); // Ansonsten werden einige Redraws übersprungen
        }
    }

    internal void DoMouseUp(MouseEventArgs e) {
        base.OnMouseUp(e);

        switch (e.Button) {
            case MouseButtons.Left:
                if (!EditAllowed) { return; }

                if (_givesMouseCommandsTo != null) {
                    if (!_givesMouseCommandsTo.MouseUp(e, Zoom, ShiftX, ShiftY)) {
                        _givesMouseCommandsTo = null;
                    } else {
                        return;
                    }
                }

                // Da ja evtl. nur ein Punkt verschoben wird, das Ursprüngliche Element wieder komplett auswählen.
                AbstractPadItem? select = null;
                if (_itemsToMove.Count == 1 && _itemsToMove[0] is PointM thispoint) {
                    if (thispoint.Parent is AbstractPadItem item) {
                        select = item;
                    }
                } else {
                    break; // sollen ja Items ein. Ansonsten wäre es immer ein Punkt
                }
                SelectItem(select, false);
                break;

            case MouseButtons.Right:
                if (!ContextMenuAllowed) { return; }
                FloatingInputBoxListBoxStyle.ContextMenuShow(this, e);
                break;
        }
        Invalidate();
    }

    internal Point MiddleOfVisiblesScreen() {
        var valx = (((float)Width) / 2) + ShiftX;
        var valy = (((float)Height) / 2) + ShiftY;

        return new((int)(valx / Zoom), (int)(valy / Zoom));
    }

    protected override void Dispose(bool disposing) {
        Items = null;
        base.Dispose(disposing);
    }

    protected override void DrawControl(Graphics gr, States state) {
        if (IsDisposed) { return; }

        base.DrawControl(gr, state);

        LinearGradientBrush lgb = new(ClientRectangle, Color.White, Color.LightGray, LinearGradientMode.Vertical);
        gr.FillRectangle(lgb, ClientRectangle);
        if (_items != null) {
            var l = _items.ItemsOnPage(_currentPage);
            _ = _items.DrawCreativePadTo(gr, l, Zoom, ShiftX, ShiftY, Size, _showInPrintMode, state);

            #region Dann die selektierten Punkte

            foreach (var thisItem in _itemsToMove) {
                if (thisItem is AbstractPadItem bpi) {
                    foreach (var p in bpi.MovablePoint) {
                        p.Draw(gr, Zoom, ShiftX, ShiftY, Design.Button_EckpunktSchieber, States.Standard);
                    }
                }
                if (thisItem is PointM p2) {
                    if (p2.Parent is AbstractPadItem bpi2) {
                        foreach (var p in bpi2.MovablePoint) {
                            p.Draw(gr, Zoom, ShiftX, ShiftY, Design.Button_EckpunktSchieber_Phantom, States.Standard);
                        }
                    }
                    p2.Draw(gr, Zoom, ShiftX, ShiftY, Design.Button_EckpunktSchieber, States.Standard);
                }
            }
            if (_givesMouseCommandsTo is AbstractPadItem { IsDisposed: false } pa) {
                var drawingCoordinates = pa.UsedArea.ZoomAndMoveRect(Zoom, ShiftX, ShiftY, false);
                gr.DrawRectangle(new Pen(Brushes.Red, 3), drawingCoordinates);
            }

            #endregion
        }

        Skin.Draw_Border(gr, Design.Table_And_Pad, state, DisplayRectangle);
    }

    protected override bool IsInputKey(Keys keyData) =>
        // http://technet.microsoft.com/de-de/subscriptions/control.isinputkey%28v=vs.100%29
        // Wenn diese NICHT ist, geht der Fokus weg, sobald der cursor gedrückt wird.
        // Ganz wichtig diese Routine!
        keyData is Keys.Up or Keys.Down or Keys.Left or Keys.Right;

    protected override RectangleF MaxBounds() => _items?.MaxBounds(_currentPage) ?? new RectangleF(0, 0, 0, 0);

    protected override void OnKeyUp(KeyEventArgs e) => DoKeyUp(e, true);

    protected override void OnMouseDown(MouseEventArgs e) => DoMouseDown(e);

    protected override void OnMouseMove(MouseEventArgs e) => DoMouseMove(e);

    protected override void OnMouseUp(MouseEventArgs e) => DoMouseUp(e);

    private void _Items_ItemAdded(object sender, ListEventArgs e) {
        if (IsDisposed) { return; }
        if (_items is not { Count: not 0 } || Fitting) { ZoomFit(); }
        Invalidate();

        var it = (AbstractPadItem)e.Item;

        if (string.IsNullOrEmpty(it.Page)) { it.Page = CurrentPage; }

        OnItemAdded(e);
    }

    private void _Items_ItemRemoved(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        if (Fitting) { ZoomFit(); }

        CheckHotItem(null, true);
        Unselect();
        //ZoomFit();
        Invalidate();
        OnItemRemoved();
    }

    private void _Items_ItemRemoving(object sender, ListEventArgs e) {
        if (IsDisposed) { return; }
        OnItemRemoving(e);
    }

    private void CheckHotItem(MouseEventArgs? e, bool doLastClicked) {
        var l = HotItems(e);
        var mina = long.MaxValue;
        HotItem = null;
        if (e != null) {
            foreach (var thisItem in l) {
                var a = (long)Math.Abs(thisItem.UsedArea.Width) * (long)Math.Abs(thisItem.UsedArea.Height);
                if (a <= mina) {
                    // Gleich deswegen, dass neuere, IDENTISCHE Items dass oberste gewählt wird.
                    mina = a;
                    HotItem = thisItem;
                }
            }
        }

        if (doLastClicked) {
            LastClickedItem = HotItem;
        }
    }

    private void DruckerDokument_BeginPrint(object sender, PrintEventArgs e) => OnBeginnPrint(e);

    private void DruckerDokument_EndPrint(object sender, PrintEventArgs e) => OnEndPrint(e);

    private void DruckerDokument_PrintPage(object sender, PrintPageEventArgs e) {
        e.HasMorePages = false;
        OnPrintPage(e);
        var i = _items?.ToBitmap(3, string.Empty);
        if (i == null) { return; }
        e.Graphics.DrawImageInRectAspectRatio(i, 0, 0, e.PageBounds.Width, e.PageBounds.Height);
    }

    private void Items_PropertyChanged(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        Invalidate();
        OnPropertyChanged();
    }

    private void MoveItems(float x, float y, bool doSnap, bool modifyMouseDown) {
        PointM? pointToMove = null;
        foreach (var thisIt in _itemsToMove) {
            if (thisIt is PointM p) { pointToMove = p; break; }
        }
        if (pointToMove == null) {
            foreach (var thisIt in _itemsToMove) {
                if (thisIt is AbstractPadItem { PointsForSuccesfullyMove.Count: > 0 } bpi) {
                    pointToMove = bpi.PointsForSuccesfullyMove[0];
                    break;
                }
            }
        }
        if (pointToMove != null && x != 0f) { x = SnapToGrid(true, pointToMove, x); }
        if (pointToMove != null && y != 0f) { y = SnapToGrid(false, pointToMove, y); }

        if (x == 0f && y == 0f) { return; }

        foreach (var thisIt in _itemsToMove) {
            thisIt.Move(x, y);
        }
        if (doSnap && modifyMouseDown) {
            // Maus-Daten modifizieren, da ja die tasächliche Bewegung wegen der SnapPoints abweichen kann.
            MouseDownPos11 = new Point((int)(MouseDownPos11.X + x), (int)(MouseDownPos11.Y + y));
        }
    }

    private void MoveItemsWithMouse() => MoveItems(MousePos11.X - MouseDownPos11.X, MousePos11.Y - MouseDownPos11.Y, true, true);

    private void OnBeginnPrint(PrintEventArgs e) => BeginnPrint?.Invoke(this, e);

    private void OnClickedItemChanged() => ClickedItemChanged?.Invoke(this, System.EventArgs.Empty);

    private void OnClickedItemChanging() => ClickedItemChanging?.Invoke(this, System.EventArgs.Empty);

    private void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) => ContextMenuItemClicked?.Invoke(this, e);

    private void OnDrawModeChanged() => DrawModeChanged?.Invoke(this, System.EventArgs.Empty);

    private void OnEndPrint(PrintEventArgs e) => EndPrint?.Invoke(this, e);

    private void OnGotNewItemCollection() => GotNewItemCollection?.Invoke(this, System.EventArgs.Empty);

    private void OnPrintPage(PrintPageEventArgs e) => PrintPage?.Invoke(this, e);

    private void PicsSave_FileOk(object sender, CancelEventArgs e) {
        if (e.Cancel || _items == null) { return; }
        _items.SaveAsBitmap(PicsSave.FileName, CurrentPage);
    }

    private void RepairPrinterData() {
        if (_repairPrinterDataPrepaired || _items == null) { return; }
        _repairPrinterDataPrepaired = true;
        DruckerDokument.DocumentName = _items.Caption;
        var done = false;
        foreach (PaperSize ps in DruckerDokument.PrinterSettings.PaperSizes) {
            if (ps.Width == (int)(_items.SheetSizeInMm.Width / 25.4 * 100) && ps.Height == (int)(_items.SheetSizeInMm.Height / 25.4 * 100)) {
                done = true;
                DruckerDokument.DefaultPageSettings.PaperSize = ps;
                break;
            }
        }
        if (!done) {
            DruckerDokument.DefaultPageSettings.PaperSize = new PaperSize("Custom", (int)(_items.SheetSizeInMm.Width / 25.4 * 100), (int)(_items.SheetSizeInMm.Height / 25.4 * 100));
        }
        DruckerDokument.DefaultPageSettings.PrinterResolution = DruckerDokument.DefaultPageSettings.PrinterSettings.PrinterResolutions[0];
        DruckerDokument.OriginAtMargins = true;
        DruckerDokument.DefaultPageSettings.Margins = new Margins((int)(_items.RandinMm.Left / 25.4 * 100), (int)(_items.RandinMm.Right / 25.4 * 100), (int)(_items.RandinMm.Top / 25.4 * 100), (int)(_items.RandinMm.Bottom / 25.4 * 100));
    }

    private float SnapToGrid(bool doX, PointM? movedPoint, float mouseMovedTo) {
        if (_items is not { SnapMode: SnapMode.SnapToGrid } || Math.Abs(_items.GridSnap) < 0.001) { return mouseMovedTo; }
        if (movedPoint is null) { return 0f; }

        var multi = Converter.MmToPixel(_items.GridSnap, ItemCollectionPad.ItemCollectionPad.Dpi);
        float value;
        if (doX) {
            value = movedPoint.X + mouseMovedTo;
            value = (int)(value / multi) * multi;
            // Formel umgestellt
            return value - movedPoint.X;
        }
        value = movedPoint.Y + mouseMovedTo;
        value = (int)(value / multi) * multi;
        return value - movedPoint.Y;
    }

    #endregion
}