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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad;
using BlueControls.ItemCollectionPad.Abstract;
using BlueDatabase;
using static BlueBasics.Converter;
using static BlueBasics.Geometry;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using PageSetupDialog = BlueControls.Forms.PageSetupDialog;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
[DefaultEvent("Click")]
public sealed partial class CreativePad : ZoomPad, IContextMenu, IPropertyChangedFeedback {

    #region Fields

    private readonly List<IMoveable> _itemsToMove = [];

    private ItemCollectionPadItem? _items;
    private AbstractPadItem? _lastClickedItem;
    private bool _repairPrinterDataPrepaired;
    private bool _showInPrintMode;
    private bool _showJointPoints;

    #endregion

    #region Constructors

    public CreativePad(ItemCollectionPadItem page, RowItem? row) : this() {
        Items = page;
        Unselect();
        MouseHighlight = false;

        if (row is { }) {
            Items.ResetVariables();
            Items.ReplaceVariables(row);
        }
    }

    public CreativePad() : base() {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
        // Initialisierungen nach dem Aufruf InitializeComponent() hinzufügen
    }

    #endregion

    #region Events

    public event PrintEventHandler? BeginnPrint;

    public event EventHandler? ClickedItemChanged;

    public event EventHandler? ClickedItemChanging;

    public event EventHandler<ContextMenuInitEventArgs>? ContextMenuInit;

    public event EventHandler<ContextMenuItemClickedEventArgs>? ContextMenuItemClicked;

    public event EventHandler? DrawModeChanged;

    public event EventHandler? GotNewItemCollection;

    public event EventHandler<System.EventArgs>? ItemRemoved;

    public event PrintPageEventHandler? PrintPage;

    public event EventHandler? PropertyChanged;

    #endregion

    #region Properties

    [DefaultValue(true)]
    public bool ContextMenuAllowed { get; set; } = true;

    [DefaultValue(true)]
    public bool EditAllowed { get; set; } = true;

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

            RegisterEvents();

            ZoomFit();
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

    [DefaultValue(false)]
    public bool ShowJointPoint {
        get => _showJointPoints;
        set {
            if (_showJointPoints == value) { return; }
            _showJointPoints = value;
            OnDrawModeChanged();
            Unselect();
        }
    }

    #endregion

    #region Methods

    public void CopyPrinterSettingsToWorkingArea() {
        if (_items is not { IsDisposed: false }) { return; }
        if (DruckerDokument.DefaultPageSettings.Landscape) {
            _items.Breite = PixelToMm(DruckerDokument.DefaultPageSettings.PaperSize.Height, 100);
            _items.Höhe = PixelToMm(DruckerDokument.DefaultPageSettings.PaperSize.Width, 100);
            _items.RandinMm = new Padding((int)PixelToMm(DruckerDokument.DefaultPageSettings.Margins.Left, 100), (int)PixelToMm(DruckerDokument.DefaultPageSettings.Margins.Top, 100), (int)PixelToMm(DruckerDokument.DefaultPageSettings.Margins.Right, 100), (int)PixelToMm(DruckerDokument.DefaultPageSettings.Margins.Bottom, 100));
        } else {
            // Hochformat
            _items.Breite = PixelToMm(DruckerDokument.DefaultPageSettings.PaperSize.Width, 100);
            _items.Höhe = PixelToMm(DruckerDokument.DefaultPageSettings.PaperSize.Height, 100);
            _items.RandinMm = new Padding((int)PixelToMm(DruckerDokument.DefaultPageSettings.Margins.Left, 100), (int)PixelToMm(DruckerDokument.DefaultPageSettings.Margins.Top, 100), (int)PixelToMm(DruckerDokument.DefaultPageSettings.Margins.Right, 100), (int)PixelToMm(DruckerDokument.DefaultPageSettings.Margins.Bottom, 100));
        }
    }

    public void DoContextMenuItemClick(ContextMenuItemClickedEventArgs e) {
        if (e.HotItem is AbstractPadItem item && item?.Parent is ItemCollectionPadItem { IsDisposed: false } icpi) {
            switch (e.Item.KeyName.ToLowerInvariant()) {
                case "#vordergrund":
                    icpi.BringToFront(item);
                    return;

                case "#hintergrund":
                    icpi.SendToBack(item);
                    return;

                case "#vorne":
                    icpi.EineEbeneNachVorne(item);
                    return;

                case "#hinten":
                    icpi.EineEbeneNachHinten(item);
                    return;

                case "#duplicate":
                    var cloned = item.Clone();
                    if (cloned is AbstractPadItem clonedapi) {
                        clonedapi.GetNewIdsForEverything();
                        _items?.Add(clonedapi);
                    }
                    return;

                //case "#page":
                //    item.Pagex = InputBox.Show("Seite:", item.Pagex, BlueBasics.FormatHolder.SystemName);
                //    Unselect();
                //    return;

                case "#connect":
                    foreach (var pt in item.JointPoints) {
                        var p = Items?.GetJointPoint(pt.KeyName, item);
                        if (p != null) {
                            item.ConnectJointPoint(pt, p);
                            return;
                        }
                    }
                    return;

                case "#export":
                    if (item is IParseable ps) {
                        using SaveFileDialog f = new();
                        f.CheckFileExists = false;
                        f.CheckPathExists = true;
                        if (!string.IsNullOrEmpty(IO.LastFilePath)) { f.InitialDirectory = IO.LastFilePath; }
                        f.AddExtension = true;
                        f.DefaultExt = "bcs";
                        f.Title = "Speichern:";
                        _ = f.ShowDialog();
                        if (string.IsNullOrEmpty(f.FileName)) { return; }
                        File.WriteAllText(f.FileName, ps.ParseableItems().FinishParseable(), Constants.Win1252);
                        IO.LastFilePath = f.FileName.FilePath();
                    }
                    return;
            }
        }

        if (e.HotItem is PointM pm) {
            switch (e.Item.KeyName.ToLowerInvariant()) {
                case "löschen":
                    if (pm.Parent is AbstractPadItem api) {
                        api.JointPoints.Remove(pm);
                    }

                    return;

                case "umbenennen":
                    var t = InputBox.Show("Neuer Name:", pm.KeyName, FormatHolder.SystemName);
                    if (!string.IsNullOrEmpty(t)) {
                        pm.KeyName = t;
                    }
                    return;

                case "verschieben":

                    var tn = InputBox.Show("Zu welchem Punkt:", pm.KeyName, FormatHolder.SystemName);
                    if (!string.IsNullOrEmpty(tn)) {
                        if (pm.Parent is AbstractPadItem api2) {
                            var p = Items?.GetJointPoint(tn, api2);
                            if (p != null) {
                                pm.SetTo(p.X, p.Y, true);
                            }
                        }
                    }

                    return;
            }
        }

        OnContextMenuItemClicked(e);
    }

    public void GetContextMenuItems(ContextMenuInitEventArgs e) {
        if (EditAllowed) {
            //var hotitem = GetHotItem(e.Mouse);

            if (e.HotItem is AbstractPadItem bpi) {
                LastClickedItem = bpi;
                e.ContextMenu.Add(ItemOf("Allgemeine Element-Aktionen", true));
                e.ContextMenu.Add(ItemOf("Objekt duplizieren", "#Duplicate", ImageCode.Kopieren, e.HotItem is ICloneable));
                e.ContextMenu.Add(ItemOf("Objekt exportieren", "#Export", ImageCode.Diskette, e.HotItem is IParseable));
                //e.ContextMenu.Add(ItemOf("Objekt auf anderes Blatt verschieben", "#Page", ImageCode.Datei, e.HotItem is IParseable));
                e.ContextMenu.Add(ItemOf("Objekt mit Punkten automatisch verbinden", "#Connect", ImageCode.HäkchenDoppelt, e.HotItem is IParseable));
                e.ContextMenu.Add(Separator());
                e.ContextMenu.Add(ItemOf("In den Vordergrund", "#Vordergrund", ImageCode.InDenVordergrund));
                e.ContextMenu.Add(ItemOf("In den Hintergrund", "#Hintergrund", ImageCode.InDenHintergrund));
                e.ContextMenu.Add(ItemOf("Eine Ebene nach vorne", "#Vorne", ImageCode.EbeneNachVorne));
                e.ContextMenu.Add(ItemOf("Eine Ebene nach hinten", "#Hinten", ImageCode.EbeneNachHinten));

                return;
            }

            LastClickedItem = null;

            if (e.HotItem is PointM) {
                e.ContextMenu.Add(ItemOf(ContextMenuCommands.Umbenennen));
                e.ContextMenu.Add(ItemOf(ContextMenuCommands.Verschieben));
                e.ContextMenu.Add(ItemOf(ContextMenuCommands.Löschen));
            }
        }

        OnContextMenuInit(e);
    }

    public void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);

    public void OnItemRemoved() => ItemRemoved?.Invoke(this, System.EventArgs.Empty);

    //public void OnItemInternalChanged(ListEventArgs e) => ItemInternalChanged?.Invoke(this, e);

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
        oriD.DefaultPageSettings.PaperSize = new PaperSize("Benutzerdefiniert", (int)MmToPixel(_items.Breite, 100), (int)MmToPixel(_items.Höhe, 100));
        oriD.DefaultPageSettings.Margins.Top = (int)MmToPixel(_items.RandinMm.Top, 100);
        oriD.DefaultPageSettings.Margins.Bottom = (int)MmToPixel(_items.RandinMm.Bottom, 100);
        oriD.DefaultPageSettings.Margins.Left = (int)MmToPixel(_items.RandinMm.Left, 100);
        oriD.DefaultPageSettings.Margins.Right = (int)MmToPixel(_items.RandinMm.Right, 100);
        var nOriD = PageSetupDialog.Show(oriD, true);
        if (nOriD == null) { return; }

        _items.Breite = PixelToMm(nOriD.DefaultPageSettings.PaperSize.Width, 100);
        _items.Höhe = PixelToMm(nOriD.DefaultPageSettings.PaperSize.Height, 100);

        _items.RandinMm = new Padding((int)PixelToMm(nOriD.DefaultPageSettings.Margins.Left, 100),
                                      (int)PixelToMm(nOriD.DefaultPageSettings.Margins.Top, 100),
                                      (int)PixelToMm(nOriD.DefaultPageSettings.Margins.Right, 100),
                                      (int)PixelToMm(nOriD.DefaultPageSettings.Margins.Bottom, 100));
    }

    public void Unselect() {
        _itemsToMove.Clear();
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

    internal Point MiddleOfVisiblesScreen() {
        var valx = (((float)Width) / 2) + ShiftX;
        var valy = (((float)Height) / 2) + ShiftY;

        return new((int)(valx / Zoom), (int)(valy / Zoom));
    }

    protected override void Dispose(bool disposing) {
        UnRegisterEvents();
        _items = null;
        base.Dispose(disposing);
    }

    protected override void DrawControl(Graphics gr, States state) {
        if (IsDisposed) { return; }

        base.DrawControl(gr, state);

        LinearGradientBrush lgb = new(ClientRectangle, Color.White, Color.LightGray, LinearGradientMode.Vertical);
        gr.FillRectangle(lgb, ClientRectangle);
        if (_items != null) {
            _items.ShowJointPoints = _showJointPoints;
            _items.ForPrinting = _showInPrintMode;
            _items.ShowAlways = true;
            _items.AutoZoomFit = false;

            _items.Draw(gr, ClientRectangle, Zoom, ShiftX, ShiftY);

            #region Dann die selektierten Punkte

            foreach (var thisItem in _itemsToMove) {
                if (thisItem is AbstractPadItem bpi) {
                    AbstractPadItem.DrawPoints(gr, bpi.MovablePoint, Zoom, ShiftX, ShiftY, Design.Button_EckpunktSchieber, States.Standard, false);
                    AbstractPadItem.DrawPoints(gr, bpi.JointPoints, Zoom, ShiftX, ShiftY, Design.Button_EckpunktSchieber_Joint, States.Standard, true);
                }

                if (thisItem is PointM p2) {
                    if (p2.Parent is AbstractPadItem bpi2) {
                        AbstractPadItem.DrawPoints(gr, bpi2.JointPoints, Zoom, ShiftX, ShiftY, Design.Button_EckpunktSchieber_Phantom, States.Standard, false);
                        AbstractPadItem.DrawPoints(gr, bpi2.MovablePoint, Zoom, ShiftX, ShiftY, Design.Button_EckpunktSchieber_Phantom, States.Standard, false);
                    }
                    p2.Draw(gr, Zoom, ShiftX, ShiftY, Design.Button_EckpunktSchieber, States.Standard);
                }
            }

            #endregion
        }

        //var t = string.Empty;
        //if(Highlight  is AbstractPadItem api) { t = api.KeyName; }

        //Skin.Draw_FormatedText(gr, MouseCoords+ t, null, Alignment.VerticalCenter, new Rectangle(0, 0, 1000, 100), Design.Caption, States.Standard, null, false, false);

        Skin.Draw_Border(gr, Design.Table_And_Pad, state, DisplayRectangle);
    }

    protected override bool IsInputKey(Keys keyData) =>
        // http://technet.microsoft.com/de-de/subscriptions/control.isinputkey%28v=vs.100%29
        // Wenn diese NICHT ist, geht der Fokus weg, sobald der cursor gedrückt wird.
        // Ganz wichtig diese Routine!
        keyData is Keys.Up or Keys.Down or Keys.Left or Keys.Right;

    protected override RectangleF MaxBounds() => _items?.UsedArea ?? new RectangleF(0, 0, 0, 0);

    protected override void OnKeyUp(KeyEventArgs e) {
        // Ganz seltsam: Wird BAse.OnKeyUp IMMER ausgelöst, passiert folgendes:
        // Wird ein Objekt gelöscht, wird anschließend das OnKeyUp Ereignis nicht mehr ausgelöst.
        base.OnKeyUp(e);
        if (!EditAllowed || _items == null) { return; }

        var multi = 1f;
        if (_items.SnapMode == SnapMode.SnapToGrid) {
            multi = MmToPixel(_items.GridSnap, ItemCollectionPadItem.Dpi);
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

    protected override void OnMouseDown(MouseEventArgs e) {
        base.OnMouseDown(e);
        if (!EditAllowed) { return; }

        QuickInfo = string.Empty;

        if (e.Button == MouseButtons.Left) {
            var hotitem = GetHotItem(e, true);
            var p = CoordinatesUnscaled(e, Zoom, ShiftX, ShiftY);
            if (_itemsToMove.Count > 0) {
                foreach (var thisItem in _itemsToMove) {
                    if (thisItem is AbstractPadItem bpi) {
                        foreach (var thisPoint in bpi.JointPoints) {
                            if (GetLenght(thisPoint, p) < 5f / Zoom) {
                                SelectItem(thisPoint, false);
                                return;
                            }
                        }

                        foreach (var thisPoint in bpi.MovablePoint) {
                            if (GetLenght(thisPoint, p) < 5f / Zoom) {
                                SelectItem(thisPoint, false);
                                return;
                            }
                        }
                    }
                }
            }

            if (hotitem is { } imv) {
                SelectItem(imv, ModifierKeys.HasFlag(Keys.Control));
            } else {
                Unselect();
            }

            if (hotitem is AbstractPadItem api) {
                LastClickedItem = api;
            } else {
                LastClickedItem = null;
            }
        }
    }

    protected override void OnMouseMove(MouseEventArgs e) {
        base.OnMouseMove(e);
        Invalidate();

        var it = GetHotItem(e, false);

        if (!EditAllowed) { return; }

        QuickInfo = string.Empty;

        if (e.Button == MouseButtons.None && it is AbstractPadItem bpi) {
            if (!string.IsNullOrEmpty(bpi.QuickInfo)) {
                QuickInfo = bpi.QuickInfo + "<hr>" + bpi.Description;
            } else {
                QuickInfo = bpi.Description;
            }
        }

        if (e.Button == MouseButtons.Left) {
            QuickInfo = string.Empty;
            MoveItemsWithMouse();
            Refresh(); // Ansonsten werden einige Redraws übersprungen
        }
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        base.OnMouseUp(e);

        switch (e.Button) {
            case MouseButtons.Left:
                if (!EditAllowed) { return; }

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
                FloatingInputBoxListBoxStyle.ContextMenuShow(this, GetHotItem(e, false), e);
                break;
        }
        Invalidate();
    }

    protected override void OnSizeChanged(System.EventArgs e) {
        Unselect();
        base.OnSizeChanged(e);
    }

    private void _Items_ItemAdded(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        if (_items.Count() > 0 || Fitting) { ZoomFit(); }
        Invalidate();
    }

    private void _Items_ItemRemoved(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        if (Fitting) { ZoomFit(); }

        Unselect();
        Invalidate();
        OnItemRemoved();
    }

    private void DruckerDokument_BeginPrint(object sender, PrintEventArgs e) => OnBeginnPrint(e);

    private void DruckerDokument_PrintPage(object sender, PrintPageEventArgs e) {
        e.HasMorePages = false;
        OnPrintPage(e);
        var i = _items?.ToBitmap(3);
        if (i == null) { return; }
        e.Graphics.DrawImageInRectAspectRatio(i, 0, 0, e.PageBounds.Width, e.PageBounds.Height);
    }

    private IMoveable? GetHotItem(MouseEventArgs? e, bool topLevel) {
        if (e == null || Items == null) { return null; }

        var tmp = Items.HotItem(e.Location, topLevel, Zoom, ShiftX, ShiftY);
        if (LastClickedItem is { } bpi) {
            var p = CoordinatesUnscaled(e, Zoom, ShiftX, ShiftY);

            foreach (var thisPoint in bpi.JointPoints) {
                if (GetLenght(p, thisPoint) < 5f / Zoom) { return thisPoint; }
            }
        }

        return tmp;
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

        if (pointToMove != null) {
            if (x != 0f) { x = SnapToGrid(true, pointToMove, x); }
            if (y != 0f) { y = SnapToGrid(false, pointToMove, y); }
        }

        foreach (var thisIt in _itemsToMove) {
            if (!thisIt.MoveXByMouse) { x = 0f; }
            if (!thisIt.MoveYByMouse) { y = 0f; }
        }

        if (x == 0f && y == 0f) { return; }

        foreach (var thisIt in _itemsToMove) {
            thisIt.Move(x, y, modifyMouseDown);
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

    private void OnGotNewItemCollection() => GotNewItemCollection?.Invoke(this, System.EventArgs.Empty);

    private void OnPrintPage(PrintPageEventArgs e) => PrintPage?.Invoke(this, e);

    private void PicsSave_FileOk(object sender, CancelEventArgs e) {
        if (e.Cancel || _items == null) { return; }
        _items.SaveAsBitmap(PicsSave.FileName);
    }

    private void RegisterEvents() {
        if (_items != null) {
            _items.ItemRemoved += _Items_ItemRemoved;
            _items.ItemAdded += _Items_ItemAdded;
            _items.PropertyChanged += Items_PropertyChanged;
        }
    }

    private void RepairPrinterData() {
        if (_repairPrinterDataPrepaired || _items == null) { return; }
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
        if (_items is not { SnapMode: SnapMode.SnapToGrid } || Math.Abs(_items.GridSnap) < 0.001) { return mouseMovedTo; }
        if (movedPoint is null) { return 0f; }

        var multi = MmToPixel(_items.GridSnap, ItemCollectionPadItem.Dpi);
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

    private void UnRegisterEvents() {
        if (_items != null) {
            _items.ItemRemoved -= _Items_ItemRemoved;
            _items.ItemAdded -= _Items_ItemAdded;
            _items.PropertyChanged -= Items_PropertyChanged;
        }
    }

    #endregion
}