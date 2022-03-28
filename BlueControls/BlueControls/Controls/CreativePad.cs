// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;
using BlueControls.ItemCollection.ItemCollectionList;
using static BlueBasics.Geometry;
using PageSetupDialog = BlueControls.Forms.PageSetupDialog;
using BlueBasics.Interfaces;
using BlueBasics.EventArgs;

namespace BlueControls.Controls {

    [Designer(typeof(BasicDesigner))]
    [DefaultEvent("Click")]
    public sealed partial class CreativePad : ZoomPad, IContextMenu, IChangedFeedback {

        #region Fields

        private readonly List<IMoveable> _itemsToMove = new();
        private IMouseAndKeyHandle? _givesMouseComandsTo;
        private ItemCollectionPad? _item;
        private string _lastQuickInfo = string.Empty;
        private bool _repairPrinterDataPrepaired;
        private bool _showInPrintMode;

        #endregion

        #region Constructors

        public CreativePad(ItemCollectionPad itemCollectionPad) : base() {
            // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
            InitializeComponent();
            // Initialisierungen nach dem Aufruf InitializeComponent() hinzufügen
            Item = itemCollectionPad;
            Unselect();
            MouseHighlight = false;
        }

        public CreativePad() : this(new ItemCollectionPad()) { }

        #endregion

        #region Events

        public event PrintEventHandler BeginnPrint;

        public event EventHandler Changed;

        public event EventHandler ClickedItemChanged;

        public event EventHandler<ContextMenuInitEventArgs> ContextMenuInit;

        public event EventHandler<ContextMenuItemClickedEventArgs> ContextMenuItemClicked;

        public event PrintEventHandler EndPrint;

        public event EventHandler GotNewItemCollection;

        public event EventHandler<ListEventArgs> ItemAdded;

        public event EventHandler<System.EventArgs> ItemRemoved;

        //public event EventHandler<ListEventArgs> ItemInternalChanged;
        public event EventHandler<ListEventArgs> ItemRemoving;

        public event EventHandler PreviewModeChanged;

        public event PrintPageEventHandler PrintPage;

        #endregion

        #region Properties

        [DefaultValue(true)]
        public bool EditAllowed { get; set; } = true;

        public BasicPadItem? HotItem { get; private set; }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ItemCollectionPad? Item {
            get => _item;
            set {
                if (_item == value) { return; }
                if (_item != null) {
                    _item.ItemRemoved -= _Item_ItemRemoved;
                    _item.ItemRemoving -= _Item_ItemRemoving;
                    _item.ItemAdded -= _Item_ItemAdded;
                    _item.Changed -= Item_Changed;
                    _item.ItemInternalChanged -= _item_ItemInternalChanged;
                }
                _item = value;
                if (_item != null) {
                    _item.ItemRemoved += _Item_ItemRemoved;
                    _item.ItemRemoving += _Item_ItemRemoving;
                    _item.ItemAdded += _Item_ItemAdded;
                    _item.Changed += Item_Changed;
                    _item.ItemInternalChanged += _item_ItemInternalChanged;
                }
                Invalidate();
                OnGotNewItemCollection();
            }
        }

        public BasicPadItem? LastClickedItem { get; private set; }

        public override string QuickInfoText => _lastQuickInfo;

        [DefaultValue(false)]
        public bool ShowInPrintMode {
            get => _showInPrintMode;
            set {
                if (_showInPrintMode == value) { return; }
                _showInPrintMode = value;
                OnPreviewModeChanged();
                Invalidate();
            }
        }

        #endregion

        #region Methods

        public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e) {
            BasicPadItem? thisItem = null;

            if (e.HotItem is BasicPadItem item) { thisItem = item; }
            var done = false;
            if (thisItem != null) {
                switch (e.ClickedComand.ToLower()) {
                    case "#vordergrund":
                        done = true;
                        thisItem.InDenVordergrund();
                        break;

                    case "#hintergrund":
                        done = true;
                        thisItem.InDenHintergrund();
                        break;

                    case "#vorne":
                        done = true;
                        thisItem.EineEbeneNachVorne();
                        break;

                    case "#hinten":
                        done = true;
                        thisItem.EineEbeneNachHinten();
                        break;

                    case "#duplicate":
                        done = true;
                        _item.Add((BasicPadItem)((ICloneable)thisItem).Clone());
                        break;
                }
            }
            Invalidate();
            return done;
        }

        public void CopyPrinterSettingsToWorkingArea() {
            if (DruckerDokument.DefaultPageSettings.Landscape) {
                _item.SheetSizeInMm = new SizeF((int)(DruckerDokument.DefaultPageSettings.PaperSize.Height * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.PaperSize.Width * 25.4 / 100));
                _item.RandinMm = new Padding((int)(DruckerDokument.DefaultPageSettings.Margins.Left * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.Margins.Top * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.Margins.Right * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.Margins.Bottom * 25.4 / 100));
            } else {
                // Hochformat
                _item.SheetSizeInMm = new SizeF((int)(DruckerDokument.DefaultPageSettings.PaperSize.Width * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.PaperSize.Height * 25.4 / 100));
                _item.RandinMm = new Padding((int)(DruckerDokument.DefaultPageSettings.Margins.Left * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.Margins.Top * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.Margins.Right * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.Margins.Bottom * 25.4 / 100));
            }
        }

        public void DoKeyUp(KeyEventArgs e, bool hasbase) {
            // Ganz seltsam: Wird BAse.OnKeyUp IMMER ausgelöst, passiert folgendes:
            // Wird ein Objekt gelöscht, wird anschließend das OnKeyUp Ereignis nicht mehr ausgelöst.
            if (hasbase) { base.OnKeyUp(e); }
            if (!EditAllowed) { return; }
            if (_givesMouseComandsTo != null) {
                if (_givesMouseComandsTo.KeyUp(this, e, Zoom, ShiftX, ShiftY)) { return; }
            }
            var multi = 1f;
            if (Item.SnapMode == SnapMode.SnapToGrid) {
                multi = Converter.MmToPixel(Item.GridSnap, ItemCollectionPad.Dpi);
            }
            if (multi < 1) { multi = 1f; }
            switch (e.KeyCode) {
                case Keys.Delete:

                case Keys.Back:
                    List<BasicPadItem> itemsDoDelete = new();
                    foreach (var thisit in _itemsToMove) {
                        if (thisit is BasicPadItem bi) { itemsDoDelete.Add(bi); }
                    }
                    Unselect();
                    _item.RemoveRange(itemsDoDelete);
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

        public void GetContextMenuItems(MouseEventArgs? e, ItemCollectionList? items, out object? selectedHotItem, List<string> tags, ref bool cancel, ref bool translate) {
            CheckHotItem(e, true);
            selectedHotItem = HotItem;
            if (selectedHotItem != null) {
                items.Add("Allgemeine Element-Aktionen", true);
                items.Add("Objekt bearbeiten", "#Erweitert", ImageCode.Stift);
                items.AddSeparator();
                items.Add("Objekt duplizieren", "#Duplicate", ImageCode.Kopieren, selectedHotItem is ICloneable);
                items.AddSeparator();
                items.Add("In den Vordergrund", "#Vordergrund", ImageCode.InDenVordergrund);
                items.Add("In den Hintergrund", "#Hintergrund", ImageCode.InDenHintergrund);
                items.Add("Eine Ebene nach vorne", "#Vorne", ImageCode.EbeneNachVorne);
                items.Add("Eine Ebene nach hinten", "#Hinten", ImageCode.EbeneNachHinten);
            }
        }

        public List<BasicPadItem> HotItems(MouseEventArgs? e) {
            if (e == null || _item == null) { return new List<BasicPadItem>(); }
            Point p = new((int)((e.X + ShiftX) / Zoom), (int)((e.Y + ShiftY) / Zoom));
            return _item.Where(thisItem => thisItem != null && thisItem.Contains(p, Zoom)).ToList();
        }

        public void OnChanged() => Changed?.Invoke(this, System.EventArgs.Empty);

        public void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);

        public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) => ContextMenuItemClicked?.Invoke(this, e);

        public void OnItemAdded(ListEventArgs e) => ItemAdded?.Invoke(this, e);

        public void OnItemRemoved() => ItemRemoved?.Invoke(this, System.EventArgs.Empty);

        //public void OnItemInternalChanged(ListEventArgs e) => ItemInternalChanged?.Invoke(this, e);
        public void OnItemRemoving(ListEventArgs e) => ItemRemoving?.Invoke(this, e);

        public void OpenSaveDialog(string title) {
            title = title.RemoveChars(Constants.Char_DateiSonderZeichen);
            PicsSave.FileName = title + ".png";
            PicsSave.ShowDialog();
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
            PrintPreviewDialog1.ShowDialog();
            RepairPrinterData();
        }

        public void ShowWorkingAreaSetup() {
            PrintDocument oriD = new();
            oriD.DefaultPageSettings.Landscape = false;
            oriD.DefaultPageSettings.PaperSize = new PaperSize("Benutzerdefiniert", (int)(_item.SheetSizeInMm.Width / 25.4 * 100), (int)(_item.SheetSizeInMm.Height / 25.4 * 100));
            oriD.DefaultPageSettings.Margins.Top = (int)(_item.RandinMm.Top / 25.4 * 100);
            oriD.DefaultPageSettings.Margins.Bottom = (int)(_item.RandinMm.Bottom / 25.4 * 100);
            oriD.DefaultPageSettings.Margins.Left = (int)(_item.RandinMm.Left / 25.4 * 100);
            oriD.DefaultPageSettings.Margins.Right = (int)(_item.RandinMm.Right / 25.4 * 100);
            var nOriD = PageSetupDialog.Show(oriD, true);
            if (nOriD == null) { return; }
            _item.SheetSizeInMm = new SizeF((int)(nOriD.DefaultPageSettings.PaperSize.Width * 25.4 / 100), (int)(nOriD.DefaultPageSettings.PaperSize.Height * 25.4 / 100));
            _item.RandinMm = new Padding((int)(nOriD.DefaultPageSettings.Margins.Left * 25.4 / 100), (int)(nOriD.DefaultPageSettings.Margins.Top * 25.4 / 100), (int)(nOriD.DefaultPageSettings.Margins.Right * 25.4 / 100), (int)(nOriD.DefaultPageSettings.Margins.Bottom * 25.4 / 100));
        }

        public void Unselect() {
            _itemsToMove.Clear();
            Invalidate();
        }

        internal void AddCentered(BasicPadItem it) {
            var pos = MiddleOfVisiblesScreen();
            var wid = (int)((Width + Zoom) * 0.8);
            var he = (int)((Height + Zoom) * 0.8);

            wid = Math.Max(wid, 10);
            he = Math.Max(he, 10);

            wid = Math.Min(wid, 200);
            he = Math.Min(he, 200);

            it.InitialPosition(pos.X - wid / 2, pos.Y - he / 2, wid, he);

            Item.Add(it);
        }

        internal void DoMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);
            CheckHotItem(e, true);
            if (!EditAllowed) { return; }
            _lastQuickInfo = string.Empty;
            if (HotItem is IMouseAndKeyHandle ho2) {
                if (ho2.MouseDown(this, e, Zoom, ShiftX, ShiftY)) {
                    _givesMouseComandsTo = ho2;
                    return;
                }
            }
            if (e.Button == MouseButtons.Left) {
                var p = KoordinatesUnscaled(e);
                if (_itemsToMove.Count > 0) {
                    foreach (var thisItem in _itemsToMove) {
                        if (thisItem is BasicPadItem bpi) {
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
            if (_givesMouseComandsTo != null) {
                if (e.Button == MouseButtons.None && HotItem != _givesMouseComandsTo) {
                    _givesMouseComandsTo = null;
                    Invalidate();
                } else {
                    if (!_givesMouseComandsTo.MouseMove(this, e, Zoom, ShiftX, ShiftY)) {
                        _givesMouseComandsTo = null;
                        Invalidate();
                    } else {
                        return;
                    }
                }
            } else {
                if (HotItem is IMouseAndKeyHandle ho2) {
                    if (ho2.MouseMove(this, e, Zoom, ShiftX, ShiftY)) {
                        _givesMouseComandsTo = ho2;
                        Invalidate();
                        return;
                    }
                }
                _lastQuickInfo = HotItem != null && e.Button == MouseButtons.None ? HotItem.QuickInfo : string.Empty;
            }
            if (e.Button == MouseButtons.Left) {
                _lastQuickInfo = string.Empty;
                MoveItemsWithMouse();
                Refresh(); // Ansonsten werden einige Redraws übersprungen
            }
        }

        internal void DoMouseUp(MouseEventArgs e) {
            base.OnMouseUp(e);
            if (_givesMouseComandsTo != null) {
                if (!_givesMouseComandsTo.MouseUp(this, e, Zoom, ShiftX, ShiftY)) {
                    _givesMouseComandsTo = null;
                } else {
                    return;
                }
            }
            switch (e.Button) {
                case MouseButtons.Left:
                    if (!EditAllowed) { return; }
                    // Da ja evtl. nur ein Punkt verschoben wird, das Ursprüngliche Element wieder komplett auswählen.
                    BasicPadItem? select = null;
                    if (_itemsToMove.Count == 1 && _itemsToMove[0] is PointM thispoint) {
                        if (thispoint.Parent is BasicPadItem item) {
                            select = item;
                        }
                    } else {
                        break; // sollen ja Items ein. Ansonsten wäre es immer ein Punkt
                    }
                    SelectItem(select, false);
                    break;

                case MouseButtons.Right:
                    FloatingInputBoxListBoxStyle.ContextMenuShow(this, e);
                    break;
            }
            Invalidate();
        }

        internal Point MiddleOfVisiblesScreen() =>
            new((int)(((Width / 2) + ShiftX) / Zoom), (int)(((Height / 2) + ShiftY) / Zoom));

        protected override void DrawControl(Graphics gr, States state) {
            base.DrawControl(gr, state);

            LinearGradientBrush lgb = new(ClientRectangle, Color.White, Color.LightGray,
                LinearGradientMode.Vertical);
            gr.FillRectangle(lgb, ClientRectangle);
            _item?.DrawCreativePadTo(gr, Size, state, Zoom, ShiftX, ShiftY, null, _showInPrintMode);

            #region Dann die selectiereren Punkte

            foreach (var thisItem in _itemsToMove) {
                if (thisItem is BasicPadItem bpi) {
                    foreach (var p in bpi.MovablePoint) {
                        p.Draw(gr, Zoom, ShiftX, ShiftY, Design.Button_EckpunktSchieber, States.Standard);
                    }
                }
                if (thisItem is PointM p2) {
                    if (p2.Parent is BasicPadItem bpi2) {
                        foreach (var p in bpi2.MovablePoint) {
                            p.Draw(gr, Zoom, ShiftX, ShiftY, Design.Button_EckpunktSchieber_Phantom, States.Standard);
                        }
                    }
                    p2.Draw(gr, Zoom, ShiftX, ShiftY, Design.Button_EckpunktSchieber, States.Standard);
                }
            }
            if (_givesMouseComandsTo is BasicPadItem pa) {
                var drawingCoordinates = pa.UsedArea.ZoomAndMoveRect(Zoom, ShiftX, ShiftY, false);
                gr.DrawRectangle(new Pen(Brushes.Red, 3), drawingCoordinates);
            }

            #endregion

            Skin.Draw_Border(gr, Design.Table_And_Pad, state, DisplayRectangle);
        }

        protected override bool IsInputKey(Keys keyData) =>
            // Ganz wichtig diese Routine!
            // Wenn diese NICHT ist, geht der Fokus weg, sobald der cursor gedrückt wird.
            // http://technet.microsoft.com/de-de/subscriptions/control.isinputkey%28v=vs.100%29
            keyData switch {
                Keys.Up or Keys.Down or Keys.Left or Keys.Right => true,
                _ => false
            };

        protected override RectangleF MaxBounds() => _item == null ? new RectangleF(0, 0, 0, 0) : _item.MaxBounds(null);

        protected override void OnKeyUp(KeyEventArgs e) => DoKeyUp(e, true);

        protected override void OnMouseDown(MouseEventArgs e) => DoMouseDown(e);

        protected override void OnMouseMove(MouseEventArgs e) => DoMouseMove(e);

        protected override void OnMouseUp(MouseEventArgs e) => DoMouseUp(e);

        private void _Item_ItemAdded(object sender, ListEventArgs e) {
            if (_item.Count == 1 || Fitting) { ZoomFit(); }
            Invalidate();
            OnItemAdded(e);
        }

        private void _item_ItemInternalChanged(object sender, ListEventArgs e) => OnChanged();

        private void _Item_ItemRemoved(object sender, System.EventArgs e) {
            if (Fitting) { ZoomFit(); }

            CheckHotItem(null, true);
            Unselect();
            ZoomFit();
            Invalidate();
            OnItemRemoved();
        }

        private void _Item_ItemRemoving(object sender, ListEventArgs e) => OnItemRemoving(e);

        private void CheckHotItem(MouseEventArgs? e, bool doLastClicked) {
            var oldClicked = LastClickedItem;
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
            if (doLastClicked && HotItem != oldClicked) {
                LastClickedItem = HotItem;
                OnClickedItemChanged();
            }
        }

        private void DruckerDokument_BeginPrint(object sender, PrintEventArgs e) => OnBeginnPrint(e);

        private void DruckerDokument_EndPrint(object sender, PrintEventArgs e) => OnEndPrint(e);

        private void DruckerDokument_PrintPage(object sender, PrintPageEventArgs e) {
            e.HasMorePages = false;
            OnPrintPage(e);
            var i = _item?.ToBitmap(3);
            if (i == null) { return; }
            e.Graphics.DrawImageInRectAspectRatio(i, 0, 0, e.PageBounds.Width, e.PageBounds.Height);
        }

        private void Item_Changed(object sender, System.EventArgs e) {
            Invalidate();
            OnChanged();
        }

        private void MoveItems(float x, float y, bool doSnap, bool modifyMouseDown) {
            PointM? pointToMove = null;
            foreach (var thisIt in _itemsToMove) {
                if (thisIt is PointM p) { pointToMove = p; break; }
            }
            if (pointToMove == null) {
                foreach (var thisIt in _itemsToMove) {
                    if (thisIt is BasicPadItem bpi && bpi.PointsForSuccesfullyMove.Count > 0) {
                        pointToMove = bpi.PointsForSuccesfullyMove[0]; break;
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

        private void OnEndPrint(PrintEventArgs e) => EndPrint?.Invoke(this, e);

        private void OnGotNewItemCollection() => GotNewItemCollection?.Invoke(this, System.EventArgs.Empty);

        private void OnPreviewModeChanged() => PreviewModeChanged?.Invoke(this, System.EventArgs.Empty);

        private void OnPrintPage(PrintPageEventArgs e) => PrintPage?.Invoke(this, e);

        private void PicsSave_FileOk(object sender, CancelEventArgs e) {
            if (e.Cancel) { return; }
            _item.SaveAsBitmap(PicsSave.FileName);
        }

        private void RepairPrinterData() {
            if (_repairPrinterDataPrepaired) { return; }
            _repairPrinterDataPrepaired = true;
            DruckerDokument.DocumentName = _item.Caption;
            var done = false;
            foreach (PaperSize ps in DruckerDokument.PrinterSettings.PaperSizes) {
                if (ps.Width == (int)(_item.SheetSizeInMm.Width / 25.4 * 100) && ps.Height == (int)(_item.SheetSizeInMm.Height / 25.4 * 100)) {
                    done = true;
                    DruckerDokument.DefaultPageSettings.PaperSize = ps;
                    break;
                }
            }
            if (!done) {
                DruckerDokument.DefaultPageSettings.PaperSize = new PaperSize("Custom", (int)(_item.SheetSizeInMm.Width / 25.4 * 100), (int)(_item.SheetSizeInMm.Height / 25.4 * 100));
            }
            DruckerDokument.DefaultPageSettings.PrinterResolution = DruckerDokument.DefaultPageSettings.PrinterSettings.PrinterResolutions[0];
            DruckerDokument.OriginAtMargins = true;
            DruckerDokument.DefaultPageSettings.Margins = new Margins((int)(_item.RandinMm.Left / 25.4 * 100), (int)(_item.RandinMm.Right / 25.4 * 100), (int)(_item.RandinMm.Top / 25.4 * 100), (int)(_item.RandinMm.Bottom / 25.4 * 100));
        }

        private float SnapToGrid(bool doX, PointM? movedPoint, float mouseMovedTo) {
            if (Item.SnapMode != SnapMode.SnapToGrid || Math.Abs(Item.GridSnap) < 0.001) { return mouseMovedTo; }
            if (movedPoint is null) { return 0f; }

            var multi = Converter.MmToPixel(Item.GridSnap, ItemCollectionPad.Dpi);
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
}