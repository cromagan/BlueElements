// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionList;
using BlueControls.Classes.ItemCollectionPad;
using BlueControls.Classes.ItemCollectionPad.Abstract;
using BlueControls.Designer_Support;
using BlueControls.EventArgs;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.Runtime.CompilerServices;
using static BlueBasics.ClassesStatic.Geometry;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
[DefaultEvent(nameof(Click))]
public partial class CreativePad : ZoomPad, IContextMenu, INotifyPropertyChanged {

    #region Fields

    private readonly List<IMoveable> _itemsToMove = [];
    private ItemCollectionPadItem? _items;
    private bool _repairPrinterDataPrepaired;

    #endregion

    #region Constructors

    public CreativePad(ItemCollectionPadItem page, RowItem? row) : this() {
        Items = page;
        Unselect();

        if (row is not null) {
            Items.ResetVariables();
            Items.ReplaceVariables(row);
        }
    }

    public CreativePad() : base() {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
        // Initialisierungen nach dem Aufruf InitializeComponent() hinzufügen
        Items = [];
    }

    #endregion

    #region Events

    public event PrintEventHandler? BeginnPrint;

    public event EventHandler? ClickedItemChanged;

    public event EventHandler? ClickedItemChanging;

    public event EventHandler? DrawModeChanged;

    public event EventHandler? GotNewItemCollection;

    public event EventHandler<System.EventArgs>? ItemRemoved;

    public event PrintPageEventHandler? PrintPage;

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Properties

    [DefaultValue(100)]
    public int CanvasMargin {
        get;
        set {
            if (value == field) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = 100;

    [DefaultValue(true)]
    public bool ContextMenuDefault { get; set; } = true;

    public override bool ControlMustPressedForZoomWithWheel => false;

    [DefaultValue(null)]
    public ReadOnlyCollection<AbstractListItem>? CustomContextMenuItems { get; set; }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ItemCollectionPadItem? Items {
        get => _items;
        set {
            if (_items == value) { return; }

            Unselect();

            UnRegisterEvents();

            _items = value;

            if (_items is { IsDisposed: false }) {
                _items.ShowJointPoints = ShowJointPoint;
                _items.ShowAlways = true;
                _items.AutoZoomFit = false;
            }

            RegisterEvents();
            Invalidate_MaxBounds();
            ZoomFit();
            Invalidate();
            OnGotNewItemCollection();
            OnPropertyChanged();
        }
    }

    public AbstractPadItem? LastClickedItem {
        get;
        set {
            if (field == value) { return; }
            OnClickedItemChanging();
            field = value;
            OnClickedItemChanged();
        }
    }

    [DefaultValue(false)]
    public bool ShowInPrintMode {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnDrawModeChanged();
            Unselect();
            OnPropertyChanged();
        }
    } = false;

    [DefaultValue(false)]
    public bool ShowJointPoint {
        get;
        set {
            if (field == value) { return; }
            field = value;
            if (_items is { IsDisposed: false }) {
                _items.ShowJointPoints = value;
            }
            OnDrawModeChanged();
            Unselect();
            OnPropertyChanged();
        }
    }

    protected override bool ShowSliderX => true;
    protected override int SmallChangeY => 5;

    #endregion

    #region Methods

    public void CopyPrinterSettingsToWorkingArea() {
        if (_items is not { IsDisposed: false }) { return; }
        if (DruckerDokument.DefaultPageSettings.Landscape) {
            _items.Breite = PixelToMm(DruckerDokument.DefaultPageSettings.PaperSize.Height, 100);
            _items.Höhe = PixelToMm(DruckerDokument.DefaultPageSettings.PaperSize.Width, 100);
            _items.RandinMm = new System.Windows.Forms.Padding((int)PixelToMm(DruckerDokument.DefaultPageSettings.Margins.Left, 100), (int)PixelToMm(DruckerDokument.DefaultPageSettings.Margins.Top, 100), (int)PixelToMm(DruckerDokument.DefaultPageSettings.Margins.Right, 100), (int)PixelToMm(DruckerDokument.DefaultPageSettings.Margins.Bottom, 100));
        } else {
            // Hochformat
            _items.Breite = PixelToMm(DruckerDokument.DefaultPageSettings.PaperSize.Width, 100);
            _items.Höhe = PixelToMm(DruckerDokument.DefaultPageSettings.PaperSize.Height, 100);
            _items.RandinMm = new System.Windows.Forms.Padding((int)PixelToMm(DruckerDokument.DefaultPageSettings.Margins.Left, 100), (int)PixelToMm(DruckerDokument.DefaultPageSettings.Margins.Top, 100), (int)PixelToMm(DruckerDokument.DefaultPageSettings.Margins.Right, 100), (int)PixelToMm(DruckerDokument.DefaultPageSettings.Margins.Bottom, 100));
        }
    }

    public List<AbstractListItem>? GetContextMenuItems(object? hotItem) {
        List<AbstractListItem> contextMenu = [];

        if (hotItem is AbstractPadItem bpi) {
            LastClickedItem = bpi;
            contextMenu.Add(ItemOf("Allgemeine Element-Aktionen", true));
            contextMenu.Add(ItemOf("Objekt duplizieren", ImageCode.Kopieren, ContextMenu_Duplicate, hotItem is ICloneable));
            contextMenu.Add(ItemOf("Objekt exportieren", ImageCode.Diskette, ContextMenu_Export, hotItem is IStringable));
            //contextMenu.Add(ItemOf("Objekt auf anderes Blatt verschieben", ImageCode.Datei, ContextMenu_Page, ContextMenuHotItem is IStringable));
            contextMenu.Add(ItemOf("Objekt mit Punkten automatisch verbinden", ImageCode.HäkchenDoppelt, ContextMenu_Connect, hotItem is IStringable));
            contextMenu.Add(Separator());
            contextMenu.Add(ItemOf("In den Vordergrund", ImageCode.InDenVordergrund, ContextMenu_Vordergrund, true));
            contextMenu.Add(ItemOf("In den Hintergrund", ImageCode.InDenHintergrund, ContextMenu_Hintergrund, true));
            contextMenu.Add(ItemOf("Eine Ebene nach vorne", ImageCode.EbeneNachVorne, ContextMenu_Vorne, true));
            contextMenu.Add(ItemOf("Eine Ebene nach hinten", ImageCode.EbeneNachHinten, ContextMenu_Hinten, true));

            return contextMenu;
        }

        LastClickedItem = null;

        if (hotItem is PointM) {
            contextMenu.Add(ItemOf("Umbenennen", QuickImage.Get(ImageCode.Stift), ContextMenu_Umbenennen, true, string.Empty));
            contextMenu.Add(ItemOf("Verschieben", QuickImage.Get(ImageCode.Mauspfeil), ContextMenu_Verschieben, true, string.Empty));
            contextMenu.Add(ItemOf("Löschen", QuickImage.Get(ImageCode.Kreuz), ContextMenu_Löschen, true, string.Empty));
        }

        return contextMenu;
    }

    public void OnItemRemoved() => ItemRemoved?.Invoke(this, System.EventArgs.Empty);

    public void OpenSaveDialog(string title) {
        title = title.RemoveChars(Constants.Char_DateiSonderZeichen);
        PicsSave.FileName = title + ".png";
        PicsSave.ShowDialog();
    }

    //public void OnItemInternalChanged(ListEventArgs e) => ItemInternalChanged?.Invoke(this, e);
    public void Print() {
        DruckerDokument.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
        if (PrintDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
            PrintDialog1.Document?.Print();
        }
        RepairPrinterData();
    }

    public void SelectItem(IMoveable? item, bool additional) {
        if (!additional) { Unselect(); }
        if (item is null) { return; }
        _itemsToMove.Add(item);

        Invalidate();
    }

    public void ShowPrinterPageSetup() {
        RepairPrinterData();
        InputBoxEditor.Edit(DruckerDokument);
    }

    public void ShowPrintPreview() {
        DruckerDokument.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
        PrintPreviewDialog1.ShowDialog();
        RepairPrinterData();
    }

    public void ShowWorkingAreaSetup() {
        if (_items is null) { return; }

        var oriD = new PrintDocument();
        oriD.DefaultPageSettings.Landscape = false;
        oriD.DefaultPageSettings.PaperSize = new PaperSize("Benutzerdefiniert", (int)MmToPixel(_items.Breite, 100), (int)MmToPixel(_items.Höhe, 100));
        oriD.DefaultPageSettings.Margins.Top = (int)MmToPixel(_items.RandinMm.Top, 100);
        oriD.DefaultPageSettings.Margins.Bottom = (int)MmToPixel(_items.RandinMm.Bottom, 100);
        oriD.DefaultPageSettings.Margins.Left = (int)MmToPixel(_items.RandinMm.Left, 100);
        oriD.DefaultPageSettings.Margins.Right = (int)MmToPixel(_items.RandinMm.Right, 100);

        if (!InputBoxEditor.Edit(oriD, true)) { return; }

        _items.Breite = PixelToMm(oriD.DefaultPageSettings.PaperSize.Width, 100);
        _items.Höhe = PixelToMm(oriD.DefaultPageSettings.PaperSize.Height, 100);

        _items.RandinMm = new System.Windows.Forms.Padding((int)PixelToMm(oriD.DefaultPageSettings.Margins.Left, 100),
                                      (int)PixelToMm(oriD.DefaultPageSettings.Margins.Top, 100),
                                      (int)PixelToMm(oriD.DefaultPageSettings.Margins.Right, 100),
                                      (int)PixelToMm(oriD.DefaultPageSettings.Margins.Bottom, 100));

        Invalidate_MaxBounds();
    }

    public void Unselect() {
        _itemsToMove.Clear();
        Invalidate();
    }

    internal void AddCentered(AbstractPadItem it) {
        var pos = MiddleOfVisiblesScreen();
        var wid = Math.Clamp((int)((Width + Zoom) * 0.8), 10, 200);
        var he = Math.Clamp((int)((Height + Zoom) * 0.8), 10, 200);

        it.InitialPosition(pos.X - (wid / 2), pos.Y - (he / 2), wid, he);

        Items?.Add(it);
    }

    internal Point MiddleOfVisiblesScreen() => new(
        (int)(((float)Width / 2 - OffsetX) / Zoom),
        (int)(((float)Height / 2 - OffsetY) / Zoom)
    );

    protected override RectangleF CalculateCanvasMaxBounds() {
        if (_items?.CanvasUsedArea is not { } a) { return new RectangleF(0, 0, 0, 0); }

        return new RectangleF(a.Left - CanvasMargin, a.Top - CanvasMargin, a.Width + (CanvasMargin * 2), a.Height + (CanvasMargin * 2));
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            BeginnPrint = null;
            ClickedItemChanged = null;
            ClickedItemChanging = null;
            DrawModeChanged = null;
            GotNewItemCollection = null;
            ItemRemoved = null;
            PrintPage = null;
            PropertyChanged = null;
        }

        UnRegisterEvents();
        _items = null;
        base.Dispose(disposing);
    }

    protected virtual void DrawBackground(Graphics gr, Rectangle drawArea) {
        using var lgb = new LinearGradientBrush(drawArea, Color.White, Color.LightGray, LinearGradientMode.Vertical);
        gr.FillRectangle(lgb, drawArea);
    }

    protected override void DrawControl(Graphics gr, States state) {
        if (IsDisposed) { return; }

        base.DrawControl(gr, state);

        var controla = AvailableControlPaintArea;

        DrawBackground(gr, controla);

        DrawCreativePadItems(gr, controla);

        Skin.Draw_Border(gr, Design.Table_And_Pad, state, DisplayRectangle);
    }

    protected virtual void DrawCreativePadItems(Graphics gr, Rectangle drawArea) {
        if (_items is not { IsDisposed: false }) { return; }

        _items.Draw(gr, drawArea, Zoom, OffsetX, OffsetY, ShowInPrintMode);

        #region Dann die selektierten Punkte

        foreach (var thisItem in _itemsToMove) {
            // Eltern-Item bestimmen: beim AbstractPadItem das Item selbst, bei einem PointM sein Parent.
            // So muss GetEffectiveViewForItem nur einmal berechnet werden.
            var bpi = thisItem switch {
                AbstractPadItem api => api,
                PointM { Parent: AbstractPadItem p } => p,
                _ => null
            };
            if (bpi is null) { continue; }

            var (ez, ex, ey) = GetEffectiveViewForItem(bpi);

            switch (thisItem) {
                case AbstractPadItem:
                    AbstractPadItem.DrawPoints(gr, bpi.MovablePoint, ez, ex, ey, Design.HandlePoint, States.Standard, false);
                    AbstractPadItem.DrawPoints(gr, bpi.JointPoints, ez, ex, ey, Design.HandlePoint_Joint, States.Standard, true);
                    break;

                case PointM p2:
                    AbstractPadItem.DrawPoints(gr, bpi.JointPoints, ez, ex, ey, Design.HandlePoint_Ghost, States.Standard, false);
                    AbstractPadItem.DrawPoints(gr, bpi.MovablePoint, ez, ex, ey, Design.HandlePoint_Ghost, States.Standard, false);
                    p2.Draw(gr, ez, ex, ey, Design.HandlePoint, States.Standard);
                    break;
            }
        }

        #endregion
    }

    protected IMoveable? GetHotItem(CanvasMouseEventArgs? e, bool topLevel, bool mustEnabled) {
        if (e is null || Items is null) { return null; }

        var tmp = Items.HotItem(e.ControlPoint, topLevel, mustEnabled, Zoom, OffsetX, OffsetY);
        if (LastClickedItem is { IsDisposed: false, Enabled: true } bpi) {
            var (ez, ex, ey) = GetEffectiveViewForItem(bpi);
            foreach (var thisPoint in bpi.JointPoints) {
                if (GetLength(e.ControlPoint, thisPoint.CanvasToControl(ez, ex, ey)) < 5f) { return thisPoint; }
            }
        }

        return tmp;
    }

    protected override bool IsInputKey(System.Windows.Forms.Keys keyData) =>
            // http://technet.microsoft.com/de-de/subscriptions/control.isinputkey%28v=vs.100%29
            // Wenn diese NICHT ist, geht der Fokus weg, sobald der cursor gedrückt wird.
            // Ganz wichtig diese Routine!
            keyData is System.Windows.Forms.Keys.Up or
                       System.Windows.Forms.Keys.Down or
                       System.Windows.Forms.Keys.Left or
                       System.Windows.Forms.Keys.Right;

    protected override void OnKeyUp(System.Windows.Forms.KeyEventArgs e) {
        // Ganz seltsam: Wird BAse.OnKeyUp IMMER ausgelöst, passiert folgendes:
        // Wird ein Objekt gelöscht, wird anschließend das OnKeyUp Ereignis nicht mehr ausgelöst.
        base.OnKeyUp(e);
        if (_items is null) { return; }

        var parentCollection = ParentCollectionOfSelectedItems();
        if (parentCollection is null) { return; }

        var multi = 1f;
        if (parentCollection.SnapMode == SnapMode.SnapToGrid) {
            multi = MmToPixel(parentCollection.GridSnap, ItemCollectionPadItem.Dpi);
        }
        if (multi < 1) { multi = 1f; }
        switch (e.KeyCode) {
            case System.Windows.Forms.Keys.Delete:

            case System.Windows.Forms.Keys.Back: {
                var itemsDoDelete = _itemsToMove.OfType<AbstractPadItem>().ToList();
                Unselect();
                foreach (var bi in itemsDoDelete) {
                    if (bi.Parent is ItemCollectionPadItem { IsDisposed: false } p) {
                        p.Remove(bi);
                    }
                }
                break;
            }

            case System.Windows.Forms.Keys.Up:
                MoveItems(0, -multi, false);
                break;

            case System.Windows.Forms.Keys.Down:
                MoveItems(0, multi, false);
                break;

            case System.Windows.Forms.Keys.Left:
                MoveItems(-multi, 0, false);
                break;

            case System.Windows.Forms.Keys.Right:
                MoveItems(multi, 0, false);
                break;
        }
    }

    protected override void OnMouseDown(CanvasMouseEventArgs e) {
        base.OnMouseDown(e);

        QuickInfo = string.Empty;

        if (e.Button == System.Windows.Forms.MouseButtons.Left) {
            var hotitem = GetHotItem(e, false, true);
            //var p = CoordinatesUnscaled(e, Zoom, OffsetX, OffsetY);
            if (_itemsToMove.Count > 0) {
                foreach (var thisItem in _itemsToMove) {
                    if (thisItem is not AbstractPadItem bpi) { continue; }
                    var (ez, ex, ey) = GetEffectiveViewForItem(bpi);
                    // JointPoints haben Vorrang, danach MovablePoint prüfen
                    var hit = bpi.JointPoints.Concat(bpi.MovablePoint)
                                              .FirstOrDefault(p => GetLength(e.ControlPoint, p.CanvasToControl(ez, ex, ey)) < 5f);
                    if (hit is not null) {
                        SelectItem(hit, false);
                        return;
                    }
                }
            }

            if (hotitem is { } imv) {
                if (imv is not AbstractPadItem { Parent: ItemCollectionPadItem { IsDisposed: false, EditMode: not EditMode.Editable } }) {
                    SelectItem(imv, ModifierKeys.HasFlag(System.Windows.Forms.Keys.Control));
                }
            } else {
                Unselect();
            }

            LastClickedItem = hotitem as AbstractPadItem;
        }
    }

    protected override void OnMouseMove(CanvasMouseEventArgs e) {
        base.OnMouseMove(e);

        var it = GetHotItem(e, false, true);

        if (e.Button == System.Windows.Forms.MouseButtons.None && it is AbstractPadItem bpi) {
            QuickInfo = !string.IsNullOrEmpty(bpi.QuickInfo) ? bpi.QuickInfo + "<hr>" + bpi.Description : bpi.Description;
        } else {
            QuickInfo = string.Empty;
        }

        if (e.Button == System.Windows.Forms.MouseButtons.Left && MouseDownData is not null) {
            MoveItems(e.CanvasX - MouseDownData.CanvasX, e.CanvasY - MouseDownData.CanvasY, true);

            Refresh();
        } else {
            Invalidate();
        }
    }

    protected override void OnMouseUp(CanvasMouseEventArgs e) {
        base.OnMouseUp(e);

        switch (e.Button) {
            case System.Windows.Forms.MouseButtons.Left:
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

            case System.Windows.Forms.MouseButtons.Right:
                ((IContextMenu)this).ContextMenuShow(GetHotItem(e, false, true));
                break;
        }
        Invalidate();
    }

    protected override void OnSizeChanged(System.EventArgs e) {
        Unselect();
        base.OnSizeChanged(e);
    }

    private void _Items_ItemAdded(object? sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        OnPropertyChanged(nameof(Items));
        Invalidate_MaxBounds();
        if (Fitting) { ZoomFit(); }
        Invalidate();
    }

    private void _Items_ItemRemoved(object? sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        OnPropertyChanged(nameof(Items));
        Invalidate_MaxBounds();
        if (Fitting) { ZoomFit(); }
        Unselect();
        Invalidate();
        OnItemRemoved();
    }

    private void _Items_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (IsDisposed) { return; }
        OnPropertyChanged(e.PropertyName);
        Invalidate_MaxBounds();
        // Beim Ziehen eines Elements über den Rand wächst der CanvasUsedArea
        // (UsedAreaOfItems). Würde hier ZoomFit() aufgerufen, verkleinert das
        // den Zoom kontinuierlich - das Element "läuft" vor der Maus weg.
        if (!_items.Any() || (Fitting && !MousePressing)) {
            ZoomFit();
        } else {
            Invalidate();
        }
    }

    private void ContextMenu_Connect(object? sender, ContextMenuEventArgs e) {
        if (e.HotItem is not AbstractPadItem item) { return; }
        foreach (var pt in item.JointPoints) {
            var p = Items?.GetJointPoint(pt.KeyName, item);
            if (p is not null) {
                item.ConnectJointPoint(pt, p);
                return;
            }
        }
    }

    private void ContextMenu_Duplicate(object? sender, ContextMenuEventArgs e) {
        if (e.HotItem is not AbstractPadItem item) { return; }
        var cloned = item.Clone();
        if (cloned is AbstractPadItem clonedapi) {
            clonedapi.GetNewIdsForEverything();
            if (item.Parent is ItemCollectionPadItem { IsDisposed: false } icpi) {
                icpi.Add(clonedapi);
            } else {
                _items?.Add(clonedapi);
            }
        }
    }

    //private void ContextMenu_Page(object sender, AbstractListItemEventArgs e) {
    //    if (e.Data is not AbstractPadItem item) { return; }
    //    item.Pagex = InputBox.Show("Seite:", item.Pagex, BlueBasics.FormatHolder_Systemname.Instance);
    //    Unselect();
    //}
    private void ContextMenu_Export(object? sender, ContextMenuEventArgs e) {
        if (e.HotItem is not IStringable ps) { return; }
        using var f = new System.Windows.Forms.SaveFileDialog();
        f.CheckFileExists = false;
        f.CheckPathExists = true;
        if (!string.IsNullOrEmpty(IO.LastFilePath)) { f.InitialDirectory = IO.LastFilePath; }
        f.AddExtension = true;
        f.DefaultExt = "bcs";
        f.Title = "Speichern:";
        f.ShowDialog();
        if (string.IsNullOrEmpty(f.FileName)) { return; }
        IO.WriteAllText(f.FileName, ps.ParseableItems().FinishParseable(), Constants.Win1252, false);
        IO.LastFilePath = f.FileName.FilePath();
    }

    private void ContextMenu_Hinten(object? sender, ContextMenuEventArgs e) {
        if (e.HotItem is not AbstractPadItem item) { return; }
        if (item.Parent is not ItemCollectionPadItem { IsDisposed: false } icpi) { return; }
        icpi.EineEbeneNachHinten(item);
    }

    private void ContextMenu_Hintergrund(object? sender, ContextMenuEventArgs e) {
        if (e.HotItem is not AbstractPadItem item) { return; }
        if (item.Parent is not ItemCollectionPadItem { IsDisposed: false } icpi) { return; }
        icpi.SendToBack(item);
    }

    private void ContextMenu_Löschen(object? sender, ContextMenuEventArgs e) {
        if (e.HotItem is not PointM pm) { return; }
        if (pm.Parent is AbstractPadItem api) {
            api.JointPoints.Remove(pm);
        }
    }

    private void ContextMenu_Umbenennen(object? sender, ContextMenuEventArgs e) {
        if (e.HotItem is not PointM pm) { return; }
        var t = InputBox.Show("Neuer Name:", pm.KeyName, FormatHolder_SystemName.Instance);
        if (!string.IsNullOrEmpty(t)) {
            pm.KeyName = t;
        }
    }

    private void ContextMenu_Verschieben(object? sender, ContextMenuEventArgs e) {
        if (e.HotItem is not PointM pm) { return; }
        var tn = InputBox.Show("Zu welchem Punkt:", pm.KeyName, FormatHolder_SystemName.Instance);
        if (!string.IsNullOrEmpty(tn)) {
            if (pm.Parent is AbstractPadItem api2) {
                var p = Items?.GetJointPoint(tn, api2);
                if (p is not null) {
                    pm.SetTo(p.X, p.Y, true);
                }
            }
        }
    }

    private void ContextMenu_Vordergrund(object? sender, ContextMenuEventArgs e) {
        if (e.HotItem is not AbstractPadItem item) { return; }
        if (item.Parent is not ItemCollectionPadItem { IsDisposed: false } icpi) { return; }
        icpi.BringToFront(item);
    }

    private void ContextMenu_Vorne(object? sender, ContextMenuEventArgs e) {
        if (e.HotItem is not AbstractPadItem item) { return; }
        if (item.Parent is not ItemCollectionPadItem { IsDisposed: false } icpi) { return; }
        icpi.EineEbeneNachVorne(item);
    }

    private void DruckerDokument_BeginPrint(object? sender, PrintEventArgs e) => OnBeginnPrint(e);

    private void DruckerDokument_PrintPage(object? sender, PrintPageEventArgs e) {
        e.HasMorePages = false;
        OnPrintPage(e);
        var i = _items?.ToBitmap(3);
        if (i is null) { return; }
        e.Graphics.DrawImageInRectAspectRatio(i, 0, 0, e.PageBounds.Width, e.PageBounds.Height);
    }

    private (float zoom, float offsetX, float offsetY) GetEffectiveViewForItem(AbstractPadItem item) {
        if (item.Parent is not ItemCollectionPadItem parentIcpi ||
            parentIcpi == _items) {
            return (Zoom, OffsetX, OffsetY);
        }

        var (pz, px, py) = GetEffectiveViewForItem(parentIcpi);
        var positionControl = parentIcpi.CanvasUsedArea.CanvasToControl(pz, px, py, false);
        return ItemCollectionPadItem.AlterView(positionControl, pz, px, py, parentIcpi.AutoZoomFit, parentIcpi.UsedAreaOfItems());
    }

    private void MoveItems(float canvasX, float canvasY, bool doSnap) {
        PointM? pointToMove = null;
        foreach (var thisIt in _itemsToMove) {
            if (thisIt is PointM p) { pointToMove = p; break; }
        }

        if (pointToMove is null) {
            foreach (var thisIt in _itemsToMove) {
                if (thisIt is AbstractPadItem { PointsForSuccessfullyMove.Count: > 0 } bpi) {
                    pointToMove = bpi.PointsForSuccessfullyMove[0];
                    break;
                }
            }
        }

        if (pointToMove is not null) {
            if (canvasX != 0f) { canvasX = SnapToGrid(true, pointToMove, canvasX); }
            if (canvasY != 0f) { canvasY = SnapToGrid(false, pointToMove, canvasY); }
        }

        foreach (var thisIt in _itemsToMove) {
            if (!thisIt.MoveXByMouse) { canvasX = 0f; }
            if (!thisIt.MoveYByMouse) { canvasY = 0f; }
        }

        if (canvasX == 0f && canvasY == 0f) { return; }

        foreach (var thisIt in _itemsToMove) {
            thisIt.Move(canvasX, canvasY, doSnap);
        }

        if (doSnap && MouseDownData is not null && _itemsToMove.Count > 0) {
            // Maus-Daten modifizieren, da ja die tasächliche Bewegung wegen der SnapPoints abweichen kann.
            MouseDownData = new CanvasMouseEventArgs(MouseDownData.CanvasX + canvasX, MouseDownData.CanvasY + canvasY, Zoom, OffsetX, OffsetY);
        }
    }

    private void OnBeginnPrint(PrintEventArgs e) => BeginnPrint?.Invoke(this, e);

    private void OnClickedItemChanged() => ClickedItemChanged?.Invoke(this, System.EventArgs.Empty);

    private void OnClickedItemChanging() => ClickedItemChanging?.Invoke(this, System.EventArgs.Empty);

    private void OnDrawModeChanged() => DrawModeChanged?.Invoke(this, System.EventArgs.Empty);

    private void OnGotNewItemCollection() => GotNewItemCollection?.Invoke(this, System.EventArgs.Empty);

    private void OnPrintPage(PrintPageEventArgs e) => PrintPage?.Invoke(this, e);

    private void OnPropertyChanged([CallerMemberName] string propertyName = "unknown") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private ItemCollectionPadItem? ParentCollectionOfSelectedItems() {
        if (_items is null) { return null; }

        ItemCollectionPadItem? first = null;

        foreach (var thisIt in _itemsToMove) {
            // Parent-Collection bestimmen: direkt beim AbstractPadItem oder zwei Ebenen höher bei PointM
            var parent = thisIt switch {
                AbstractPadItem { Parent: ItemCollectionPadItem { IsDisposed: false } icpi } => icpi,
                PointM { Parent: AbstractPadItem { Parent: ItemCollectionPadItem { IsDisposed: false } icpi2 } } => icpi2,
                _ => null
            };

            if (parent is null) { return null; }
            if (first is null) { first = parent; }
            if (first != parent) { return null; }
        }

        return first ?? _items;
    }

    private void PicsSave_FileOk(object? sender, CancelEventArgs e) {
        if (e.Cancel || _items is null) { return; }
        _items.SaveAsBitmap(PicsSave.FileName);
    }

    private void RegisterEvents() {
        if (_items is not null) {
            _items.ItemRemoved += _Items_ItemRemoved;
            _items.ItemAdded += _Items_ItemAdded;
            _items.PropertyChanged += _Items_PropertyChanged;
        }
    }

    private void RepairPrinterData() {
        if (_repairPrinterDataPrepaired || _items is null) { return; }
        _repairPrinterDataPrepaired = true;
        DruckerDokument.DocumentName = _items.Caption;
        var done = false;
        foreach (PaperSize ps in DruckerDokument.PrinterSettings.PaperSizes) {
            if (ps.Width == (int)MmToPixel(_items.Breite, 100) && ps.Height == (int)MmToPixel(_items.Höhe, 100)) {
                done = true;
                DruckerDokument.DefaultPageSettings.PaperSize = ps;
                break;
            }
        }
        if (!done) {
            DruckerDokument.DefaultPageSettings.PaperSize = new PaperSize("Custom", (int)MmToPixel(_items.Breite, 100), (int)MmToPixel(_items.Höhe, 100));
        }
        DruckerDokument.DefaultPageSettings.PrinterResolution = DruckerDokument.DefaultPageSettings.PrinterSettings.PrinterResolutions[0];
        DruckerDokument.OriginAtMargins = true;
        DruckerDokument.DefaultPageSettings.Margins = new Margins((int)MmToPixel(_items.RandinMm.Left, 100), (int)MmToPixel(_items.RandinMm.Right, 100), (int)MmToPixel(_items.RandinMm.Top, 100), (int)MmToPixel(_items.RandinMm.Bottom, 100));
    }

    private float SnapToGrid(bool doX, PointM? movedPoint, float mouseMovedTo) {
        var parentCollection = ParentCollectionOfSelectedItems();
        if (parentCollection is not { SnapMode: SnapMode.SnapToGrid } || Math.Abs(parentCollection.GridSnap) < 0.001) { return mouseMovedTo; }
        if (movedPoint is null) { return 0f; }

        var multi = MmToPixel(parentCollection.GridSnap, ItemCollectionPadItem.Dpi);
        var origin = doX ? movedPoint.X : movedPoint.Y;
        var snapped = (int)((origin + mouseMovedTo) / multi) * multi;
        return snapped - origin;
    }

    private void UnRegisterEvents() {
        if (_items is not null) {
            _items.ItemRemoved -= _Items_ItemRemoved;
            _items.ItemAdded -= _Items_ItemAdded;
            _items.PropertyChanged -= _Items_PropertyChanged;
        }
    }

    #endregion
}