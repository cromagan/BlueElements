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
using BlueBasics.Classes;
using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionPad;
using BlueControls.Classes.ItemCollectionPad.Abstract;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueTable.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;

using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using static BlueBasics.ClassesStatic.Converter;
using static BlueBasics.ClassesStatic.Geometry;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;
using PageSetupDialog = BlueControls.Forms.PageSetupDialog;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
[DefaultEvent(nameof(Click))]
public sealed partial class CreativePad : ZoomPad, IContextMenu, INotifyPropertyChanged {

    #region Fields

    private readonly List<IMoveable> _itemsToMove = [];
    private ItemCollectionPadItem? _items = [];
    private bool _repairPrinterDataPrepaired;

    #endregion

    #region Constructors

    public CreativePad(ItemCollectionPadItem page, RowItem? row) : this() {
        Items = page;
        Unselect();

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

    public event EventHandler? DrawModeChanged;

    public event EventHandler? GotNewItemCollection;

    public event EventHandler<System.EventArgs>? ItemRemoved;

    public event PrintPageEventHandler? PrintPage;

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Properties

    [DefaultValue(true)]
    public bool ContextMenuAllowed { get; set; } = true;

    public override bool ControlMustPressedForZoomWithWheel => false;

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
    }

    [DefaultValue(false)]
    public bool ShowJointPoint {
        get;
        set {
            if (field == value) { return; }
            field = value;
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
            _items.RandinMm = new Padding((int)PixelToMm(DruckerDokument.DefaultPageSettings.Margins.Left, 100), (int)PixelToMm(DruckerDokument.DefaultPageSettings.Margins.Top, 100), (int)PixelToMm(DruckerDokument.DefaultPageSettings.Margins.Right, 100), (int)PixelToMm(DruckerDokument.DefaultPageSettings.Margins.Bottom, 100));
        } else {
            // Hochformat
            _items.Breite = PixelToMm(DruckerDokument.DefaultPageSettings.PaperSize.Width, 100);
            _items.Höhe = PixelToMm(DruckerDokument.DefaultPageSettings.PaperSize.Height, 100);
            _items.RandinMm = new Padding((int)PixelToMm(DruckerDokument.DefaultPageSettings.Margins.Left, 100), (int)PixelToMm(DruckerDokument.DefaultPageSettings.Margins.Top, 100), (int)PixelToMm(DruckerDokument.DefaultPageSettings.Margins.Right, 100), (int)PixelToMm(DruckerDokument.DefaultPageSettings.Margins.Bottom, 100));
        }
    }

    public void GetContextMenuItems(ContextMenuInitEventArgs e) {
        if (EditAllowed) {
            if (e.HotItem is AbstractPadItem bpi) {
                LastClickedItem = bpi;
                e.ContextMenu.Add(ItemOf("Allgemeine Element-Aktionen", true));
                e.ContextMenu.Add(ItemOf("Objekt duplizieren", ImageCode.Kopieren, ContextMenu_Duplicate, e.HotItem, e.HotItem is ICloneable));
                e.ContextMenu.Add(ItemOf("Objekt exportieren", ImageCode.Diskette, ContextMenu_Export, e.HotItem, e.HotItem is IStringable));
                //e.ContextMenu.Add(ItemOf("Objekt auf anderes Blatt verschieben", ImageCode.Datei, ContextMenu_Page, e.HotItem, e.HotItem is IStringable));
                e.ContextMenu.Add(ItemOf("Objekt mit Punkten automatisch verbinden", ImageCode.HäkchenDoppelt, ContextMenu_Connect, e.HotItem, e.HotItem is IStringable));
                e.ContextMenu.Add(Separator());
                e.ContextMenu.Add(ItemOf("In den Vordergrund", ImageCode.InDenVordergrund, ContextMenu_Vordergrund, e.HotItem, true));
                e.ContextMenu.Add(ItemOf("In den Hintergrund", ImageCode.InDenHintergrund, ContextMenu_Hintergrund, e.HotItem, true));
                e.ContextMenu.Add(ItemOf("Eine Ebene nach vorne", ImageCode.EbeneNachVorne, ContextMenu_Vorne, e.HotItem, true));
                e.ContextMenu.Add(ItemOf("Eine Ebene nach hinten", ImageCode.EbeneNachHinten, ContextMenu_Hinten, e.HotItem, true));

                return;
            }

            LastClickedItem = null;

            if (e.HotItem is PointM) {
                e.ContextMenu.Add(ItemOf("Umbenennen", QuickImage.Get(ImageCode.Stift), ContextMenu_Umbenennen, e.HotItem, true, string.Empty));
                e.ContextMenu.Add(ItemOf("Verschieben", QuickImage.Get(ImageCode.Mauspfeil), ContextMenu_Verschieben, e.HotItem, true, string.Empty));
                e.ContextMenu.Add(ItemOf("Löschen", QuickImage.Get(ImageCode.Kreuz), ContextMenu_Löschen, e.HotItem, true, string.Empty));
            }
        }

        OnContextMenuInit(e);
    }

    public void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);

    public void OnItemRemoved() => ItemRemoved?.Invoke(this, System.EventArgs.Empty);

    public void OpenSaveDialog(string title) {
        title = title.RemoveChars(Constants.Char_DateiSonderZeichen);
        PicsSave.FileName = title + ".png";
        PicsSave.ShowDialog();
    }

    //public void OnItemInternalChanged(ListEventArgs e) => ItemInternalChanged?.Invoke(this, e);
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
        PrintPreviewDialog1.ShowDialog();
        RepairPrinterData();
    }

    public void ShowWorkingAreaSetup() {
        if (_items == null) { return; }

        var oriD = new PrintDocument();
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

        Invalidate_MaxBounds();
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
        var valx = ((float)Width / 2) - OffsetX;
        var valy = ((float)Height / 2) - OffsetY;

        return new((int)(valx / Zoom), (int)(valy / Zoom));
    }

    protected override RectangleF CalculateCanvasMaxBounds() {
        if (_items?.CanvasUsedArea is not { } a) { return new RectangleF(0, 0, 0, 0); }

        var add = 100;// (float)Math.Max(a.Width * 0.1, a.Height * 0.1);

        return new RectangleF(a.Left - add, a.Top - add, a.Width + (add * 2), a.Height + (add * 2));
    }

    protected override void Dispose(bool disposing) {
        UnRegisterEvents();
        _items = null;
        base.Dispose(disposing);
    }

    protected override void DrawControl(Graphics gr, States state) {
        if (IsDisposed) { return; }

        base.DrawControl(gr, state);

        var controla = AvailableControlPaintArea;

        var lgb = new LinearGradientBrush(controla, Color.White, Color.LightGray, LinearGradientMode.Vertical);
        gr.FillRectangle(lgb, controla);

        if (_items != null) {
            _items.ShowJointPoints = ShowJointPoint;
            _items.ShowAlways = true;
            _items.AutoZoomFit = false;

            _items.Draw(gr, controla, Zoom, OffsetX, OffsetY, ShowInPrintMode);

            #region Dann die selektierten Punkte

            foreach (var thisItem in _itemsToMove) {
                if (thisItem is AbstractPadItem bpi) {
                    AbstractPadItem.DrawPoints(gr, bpi.MovablePoint, Zoom, OffsetX, OffsetY, Design.Button_EckpunktSchieber, States.Standard, false);
                    AbstractPadItem.DrawPoints(gr, bpi.JointPoints, Zoom, OffsetX, OffsetY, Design.Button_EckpunktSchieber_Joint, States.Standard, true);
                }

                if (thisItem is PointM p2) {
                    if (p2.Parent is AbstractPadItem bpi2) {
                        AbstractPadItem.DrawPoints(gr, bpi2.JointPoints, Zoom, OffsetX, OffsetY, Design.Button_EckpunktSchieber_Phantom, States.Standard, false);
                        AbstractPadItem.DrawPoints(gr, bpi2.MovablePoint, Zoom, OffsetX, OffsetY, Design.Button_EckpunktSchieber_Phantom, States.Standard, false);
                    }
                    p2.Draw(gr, Zoom, OffsetX, OffsetY, Design.Button_EckpunktSchieber, States.Standard);
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
                MoveItems(0, -multi, false);
                break;

            case Keys.Down:
                MoveItems(0, multi, false);
                break;

            case Keys.Left:
                MoveItems(-multi, 0, false);
                break;

            case Keys.Right:
                MoveItems(multi, 0, false);
                break;
        }
    }

    protected override void OnMouseDown(CanvasMouseEventArgs e) {
        base.OnMouseDown(e);

        if (!EditAllowed) { return; }

        QuickInfo = string.Empty;

        if (e.Button == MouseButtons.Left) {
            var hotitem = GetHotItem(e, true);
            //var p = CoordinatesUnscaled(e, Zoom, OffsetX, OffsetY);
            if (_itemsToMove.Count > 0) {
                foreach (var thisItem in _itemsToMove) {
                    if (thisItem is AbstractPadItem bpi) {
                        foreach (var thisPoint in bpi.JointPoints) {
                            if (GetLength(thisPoint, e.CanvasPoint).CanvasToControl(Zoom) < 5f) {
                                SelectItem(thisPoint, false);
                                return;
                            }
                        }

                        foreach (var thisPoint in bpi.MovablePoint) {
                            if (GetLength(thisPoint, e.CanvasPoint).CanvasToControl(Zoom) < 5f) {
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

            LastClickedItem = hotitem as AbstractPadItem;
        }
    }

    protected override void OnMouseMove(CanvasMouseEventArgs e) {
        base.OnMouseMove(e);
        Invalidate();

        var it = GetHotItem(e, false);

        if (!EditAllowed) { return; }

        QuickInfo = string.Empty;

        if (e.Button == MouseButtons.None && it is AbstractPadItem bpi) {
            QuickInfo = !string.IsNullOrEmpty(bpi.QuickInfo) ? bpi.QuickInfo + "<hr>" + bpi.Description : bpi.Description;
        }

        if (e.Button == MouseButtons.Left && MouseDownData is { }) {
            QuickInfo = string.Empty;

            MoveItems(e.CanvasX - MouseDownData.CanvasX, e.CanvasY - MouseDownData.CanvasY, true);

            Refresh(); // Ansonsten werden einige Redraws übersprungen
        }
    }

    protected override void OnMouseUp(CanvasMouseEventArgs e) {
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
                FloatingInputBoxListBoxStyle.ContextMenuShow(this, GetHotItem(e, false), e.ToMouseEventArgs());
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
        OnPropertyChanged(nameof(Items));
        if (Fitting) { ZoomFit(); }
        Invalidate();
    }

    private void _Items_ItemRemoved(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        OnPropertyChanged(nameof(Items));
        if (Fitting) { ZoomFit(); }

        Unselect();
        Invalidate();
        OnItemRemoved();
    }

    private void _Items_PropertyChanged(object sender, PropertyChangedEventArgs e) {
        if (IsDisposed) { return; }
        OnPropertyChanged(e.PropertyName);
        Invalidate_MaxBounds();
        if (!_items.Any() || Fitting) { ZoomFit(); }
        Invalidate();
    }

    private void ContextMenu_Connect(object sender, ObjectEventArgs e) {
        if (e.Data is not AbstractPadItem item) { return; }
        foreach (var pt in item.JointPoints) {
            var p = Items?.GetJointPoint(pt.KeyName, item);
            if (p != null) {
                item.ConnectJointPoint(pt, p);
                return;
            }
        }
    }

    private void ContextMenu_Duplicate(object sender, ObjectEventArgs e) {
        if (e.Data is not AbstractPadItem item) { return; }
        var cloned = item.Clone();
        if (cloned is AbstractPadItem clonedapi) {
            clonedapi.GetNewIdsForEverything();
            _items?.Add(clonedapi);
        }
    }

    //private void ContextMenu_Page(object sender, ObjectEventArgs e) {
    //    if (e.Data is not AbstractPadItem item) { return; }
    //    item.Pagex = InputBox.Show("Seite:", item.Pagex, BlueBasics.FormatHolder.SystemName);
    //    Unselect();
    //}
    private void ContextMenu_Export(object sender, ObjectEventArgs e) {
        if (e.Data is not IStringable ps) { return; }
        using var f = new SaveFileDialog();
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

    private void ContextMenu_Hinten(object sender, ObjectEventArgs e) {
        if (e.Data is not AbstractPadItem item) { return; }
        if (item.Parent is not ItemCollectionPadItem { IsDisposed: false } icpi) { return; }
        icpi.EineEbeneNachHinten(item);
    }

    private void ContextMenu_Hintergrund(object sender, ObjectEventArgs e) {
        if (e.Data is not AbstractPadItem item) { return; }
        if (item.Parent is not ItemCollectionPadItem { IsDisposed: false } icpi) { return; }
        icpi.SendToBack(item);
    }

    private void ContextMenu_Löschen(object sender, ObjectEventArgs e) {
        if (e.Data is not PointM pm) { return; }
        if (pm.Parent is AbstractPadItem api) {
            api.JointPoints.Remove(pm);
        }
    }

    private void ContextMenu_Umbenennen(object sender, ObjectEventArgs e) {
        if (e.Data is not PointM pm) { return; }
        var t = InputBox.Show("Neuer Name:", pm.KeyName, FormatHolder.SystemName);
        if (!string.IsNullOrEmpty(t)) {
            pm.KeyName = t;
        }
    }

    private void ContextMenu_Verschieben(object sender, ObjectEventArgs e) {
        if (e.Data is not PointM pm) { return; }
        var tn = InputBox.Show("Zu welchem Punkt:", pm.KeyName, FormatHolder.SystemName);
        if (!string.IsNullOrEmpty(tn)) {
            if (pm.Parent is AbstractPadItem api2) {
                var p = Items?.GetJointPoint(tn, api2);
                if (p != null) {
                    pm.SetTo(p.X, p.Y, true);
                }
            }
        }
    }

    private void ContextMenu_Vordergrund(object sender, ObjectEventArgs e) {
        if (e.Data is not AbstractPadItem item) { return; }
        if (item.Parent is not ItemCollectionPadItem { IsDisposed: false } icpi) { return; }
        icpi.BringToFront(item);
    }

    private void ContextMenu_Vorne(object sender, ObjectEventArgs e) {
        if (e.Data is not AbstractPadItem item) { return; }
        if (item.Parent is not ItemCollectionPadItem { IsDisposed: false } icpi) { return; }
        icpi.EineEbeneNachVorne(item);
    }

    private void DruckerDokument_BeginPrint(object sender, PrintEventArgs e) => OnBeginnPrint(e);

    private void DruckerDokument_PrintPage(object sender, PrintPageEventArgs e) {
        e.HasMorePages = false;
        OnPrintPage(e);
        var i = _items?.ToBitmap(3);
        if (i == null) { return; }
        e.Graphics.DrawImageInRectAspectRatio(i, 0, 0, e.PageBounds.Width, e.PageBounds.Height);
    }

    private IMoveable? GetHotItem(CanvasMouseEventArgs? e, bool topLevel) {
        if (e == null || Items == null) { return null; }

        var tmp = Items.HotItem(e.ControlPoint, topLevel, Zoom, OffsetX, OffsetY);
        if (LastClickedItem is { IsDisposed: false } bpi) {
            foreach (var thisPoint in bpi.JointPoints) {
                if (GetLength(e.CanvasPoint, thisPoint).CanvasToControl(Zoom) < 5f) { return thisPoint; }
            }
        }

        return tmp;
    }

    private void MoveItems(float canvasX, float canvasY, bool doSnap) {
        PointM? pointToMove = null;
        foreach (var thisIt in _itemsToMove) {
            if (thisIt is PointM p) { pointToMove = p; break; }
        }

        if (pointToMove == null) {
            foreach (var thisIt in _itemsToMove) {
                if (thisIt is AbstractPadItem { PointsForSuccessfullyMove.Count: > 0 } bpi) {
                    pointToMove = bpi.PointsForSuccessfullyMove[0];
                    break;
                }
            }
        }

        if (pointToMove != null) {
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

        if (doSnap && MouseDownData != null) {
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

    private void PicsSave_FileOk(object sender, CancelEventArgs e) {
        if (e.Cancel || _items == null) { return; }
        _items.SaveAsBitmap(PicsSave.FileName);
    }

    private void RegisterEvents() {
        if (_items != null) {
            _items.ItemRemoved += _Items_ItemRemoved;
            _items.ItemAdded += _Items_ItemAdded;
            _items.PropertyChanged += _Items_PropertyChanged;
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
            _items.PropertyChanged -= _Items_PropertyChanged;
        }
    }

    #endregion
}