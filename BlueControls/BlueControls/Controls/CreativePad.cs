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

namespace BlueControls.Controls {

    [Designer(typeof(BasicDesigner))]
    [DefaultEvent("Click")]
    public sealed partial class CreativePad : ZoomPad, IContextMenu {

        #region Fields

        public bool _editAllowed = true;

        private readonly List<IMoveable> _ItemsToMove = new();

        private IMouseAndKeyHandle _GivesMouseComandsTo;

        private ItemCollectionPad _Item;

        private string _LastQuickInfo = string.Empty;

        private bool _ShowInPrintMode;

        private bool RepairPrinterData_Prepaired;

        #endregion

        #region Constructors

        public CreativePad(ItemCollectionPad itemCollectionPad) : base() {
            // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
            InitializeComponent();
            // Initialisierungen nach dem Aufruf InitializeComponent() hinzufügen
            Item = itemCollectionPad;
            Unselect();
            _MouseHighlight = false;
        }

        public CreativePad() : this(new ItemCollectionPad()) {
        }

        #endregion

        #region Events

        public event PrintEventHandler BeginnPrint;

        public event EventHandler ClickedItemChanged;

        public event EventHandler<ContextMenuInitEventArgs> ContextMenuInit;

        public event EventHandler<ContextMenuItemClickedEventArgs> ContextMenuItemClicked;

        public event PrintEventHandler EndPrint;

        public event EventHandler GotNewItemCollection;

        public event EventHandler PreviewModeChanged;

        public event PrintPageEventHandler PrintPage;

        #endregion

        #region Properties

        [DefaultValue(true)]
        public bool EditAllowed {
            get => _editAllowed;
            set => _editAllowed = value;
        }

        public BasicPadItem HotItem { get; private set; } = null;

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ItemCollectionPad Item {
            get => _Item;
            set {
                if (_Item == value) { return; }
                if (_Item != null) {
                    _Item.ItemRemoved -= _Item_ItemRemoved;
                    _Item.ItemAdded -= _Item_ItemAdded;
                    _Item.DoInvalidate -= Item_DoInvalidate;
                }
                _Item = value;
                if (_Item != null) {
                    _Item.ItemRemoved += _Item_ItemRemoved;
                    _Item.ItemAdded += _Item_ItemAdded;
                    _Item.DoInvalidate += Item_DoInvalidate;
                    _Item.OnDoInvalidate();
                }

                OnGotNewItemCollection();
            }
        }

        public BasicPadItem LastClickedItem { get; private set; } = null;

        public override string QuickInfoText => _LastQuickInfo;

        [DefaultValue(false)]
        public bool ShowInPrintMode {
            get => _ShowInPrintMode;
            set {
                if (_ShowInPrintMode == value) { return; }
                _ShowInPrintMode = value;
                OnPreviewModeChanged();
                Invalidate();
            }
        }

        #endregion

        #region Methods

        public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e) {
            BasicPadItem thisItem = null;
            if (e.HotItem is BasicPadItem item) { thisItem = item; }
            var Done = false;
            if (thisItem != null) {
                switch (e.ClickedComand.ToLower()) {
                    case "#erweitert":
                        Done = true;
                        ShowErweitertMenü(thisItem);
                        break;

                    case "#vordergrund":
                        Done = true;
                        thisItem.InDenVordergrund();
                        break;

                    case "#hintergrund":
                        Done = true;
                        thisItem.InDenHintergrund();
                        break;

                    case "#vorne":
                        Done = true;
                        thisItem.EineEbeneNachVorne();
                        break;

                    case "#hinten":
                        Done = true;
                        thisItem.EineEbeneNachHinten();
                        break;

                    case "#duplicate":
                        Done = true;
                        _Item.Add((BasicPadItem)((ICloneable)thisItem).Clone());
                        break;
                }
            }
            Invalidate();
            return Done;
        }

        public void CopyPrinterSettingsToWorkingArea() {
            if (DruckerDokument.DefaultPageSettings.Landscape) {
                _Item.SheetSizeInMM = new SizeF((int)(DruckerDokument.DefaultPageSettings.PaperSize.Height * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.PaperSize.Width * 25.4 / 100));
                _Item.RandinMM = new System.Windows.Forms.Padding((int)(DruckerDokument.DefaultPageSettings.Margins.Left * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.Margins.Top * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.Margins.Right * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.Margins.Bottom * 25.4 / 100));
            } else {
                // Hochformat
                _Item.SheetSizeInMM = new SizeF((int)(DruckerDokument.DefaultPageSettings.PaperSize.Width * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.PaperSize.Height * 25.4 / 100));
                _Item.RandinMM = new System.Windows.Forms.Padding((int)(DruckerDokument.DefaultPageSettings.Margins.Left * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.Margins.Top * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.Margins.Right * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.Margins.Bottom * 25.4 / 100));
            }
        }

        public void DoKeyUp(System.Windows.Forms.KeyEventArgs e, bool hasbase) {
            // Ganz seltsam: Wird BAse.OnKeyUp IMMER ausgelöst, passiert folgendes:
            // Wird ein Objekt gelöscht, wird anschließend das OnKeyUp Ereignis nicht mehr ausgelöst.
            if (hasbase) { base.OnKeyUp(e); }
            if (!_editAllowed) { return; }
            if (_GivesMouseComandsTo != null) {
                if (_GivesMouseComandsTo.KeyUp(this, e, _Zoom, _shiftX, _shiftY)) { return; }
            }
            var Multi = 1D;
            if (Item.SnapMode == enSnapMode.SnapToGrid) {
                Multi = Converter.mmToPixel(Item.GridSnap, ItemCollectionPad.DPI);
            }
            if (Multi < 1) { Multi = 1D; }
            switch (e.KeyCode) {
                case System.Windows.Forms.Keys.Delete:

                case System.Windows.Forms.Keys.Back:
                    List<BasicPadItem> ItemsDoDelete = new();
                    foreach (var thisit in _ItemsToMove) {
                        if (thisit is BasicPadItem bi) { ItemsDoDelete.Add(bi); }
                    }
                    Unselect();
                    _Item.RemoveRange(ItemsDoDelete);
                    break;

                case System.Windows.Forms.Keys.Up:
                    MoveItems(0d, -1 * Multi, false, false);
                    break;

                case System.Windows.Forms.Keys.Down:
                    MoveItems(0d, 1 * Multi, false, false);
                    break;

                case System.Windows.Forms.Keys.Left:
                    MoveItems(-1 * Multi, 0d, false, false);
                    break;

                case System.Windows.Forms.Keys.Right:
                    MoveItems(1 * Multi, 0d, false, false);
                    break;
            }
        }

        public void GetContextMenuItems(System.Windows.Forms.MouseEventArgs e, ItemCollectionList Items, out object selectedHotItem, List<string> Tags, ref bool Cancel, ref bool Translate) {
            CheckHotItem(e, true);
            selectedHotItem = HotItem;
            if (selectedHotItem != null) {
                Items.Add("Allgemeine Element-Aktionen", true);
                Items.Add("Objekt bearbeiten", "#Erweitert", enImageCode.Stift);
                Items.AddSeparator();
                Items.Add("Objekt duplizieren", "#Duplicate", enImageCode.Kopieren, selectedHotItem is ICloneable);
                Items.AddSeparator();
                Items.Add("In den Vordergrund", "#Vordergrund", enImageCode.InDenVordergrund);
                Items.Add("In den Hintergrund", "#Hintergrund", enImageCode.InDenHintergrund);
                Items.Add("Eine Ebene nach vorne", "#Vorne", enImageCode.EbeneNachVorne);
                Items.Add("Eine Ebene nach hinten", "#Hinten", enImageCode.EbeneNachHinten);
            }
        }

        public List<BasicPadItem> HotItems(System.Windows.Forms.MouseEventArgs e) {
            if (e == null) { return new List<BasicPadItem>(); }
            Point P = new((int)((e.X + _shiftX) / _Zoom), (int)((e.Y + _shiftY) / _Zoom));
            List<BasicPadItem> l = new();
            foreach (var ThisItem in _Item) {
                if (ThisItem != null && ThisItem.Contains(P, _Zoom)) {
                    l.Add(ThisItem);
                }
            }
            return l;
        }

        public void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);

        public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) => ContextMenuItemClicked?.Invoke(this, e);

        public void OpenSaveDialog(string title) {
            title = title.RemoveChars(Constants.Char_DateiSonderZeichen);
            PicsSave.FileName = title + ".png";
            PicsSave.ShowDialog();
        }

        public void Print() {
            DruckerDokument.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
            if (PrintDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                PrintDialog1.Document.Print();
            }
            RepairPrinterData();
        }

        public void SelectItem(IMoveable item, bool additional) {
            if (!additional) { Unselect(); }
            if (item == null) { return; }
            _ItemsToMove.Add(item);
            Invalidate();
        }

        public void ShowErweitertMenü(BasicPadItem Item) {
            var l = Item.GetStyleOptions();
            if (l.Count == 0) {
                MessageBox.Show("Objekt hat keine<br>Bearbeitungsmöglichkeiten.", enImageCode.Information);
                return;
            }
            EditBoxFlexiControl.Show(l);
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
            PrintDocument OriD = new();
            OriD.DefaultPageSettings.Landscape = false;
            OriD.DefaultPageSettings.PaperSize = new PaperSize("Benutzerdefiniert", (int)(_Item.SheetSizeInMM.Width / 25.4 * 100), (int)(_Item.SheetSizeInMM.Height / 25.4 * 100));
            OriD.DefaultPageSettings.Margins.Top = (int)(_Item.RandinMM.Top / 25.4 * 100);
            OriD.DefaultPageSettings.Margins.Bottom = (int)(_Item.RandinMM.Bottom / 25.4 * 100);
            OriD.DefaultPageSettings.Margins.Left = (int)(_Item.RandinMM.Left / 25.4 * 100);
            OriD.DefaultPageSettings.Margins.Right = (int)(_Item.RandinMM.Right / 25.4 * 100);
            var nOriD = PageSetupDialog.Show(OriD, true);
            if (nOriD == null) { return; }
            _Item.SheetSizeInMM = new SizeF((int)(nOriD.DefaultPageSettings.PaperSize.Width * 25.4 / 100), (int)(nOriD.DefaultPageSettings.PaperSize.Height * 25.4 / 100));
            _Item.RandinMM = new System.Windows.Forms.Padding((int)(nOriD.DefaultPageSettings.Margins.Left * 25.4 / 100), (int)(nOriD.DefaultPageSettings.Margins.Top * 25.4 / 100), (int)(nOriD.DefaultPageSettings.Margins.Right * 25.4 / 100), (int)(nOriD.DefaultPageSettings.Margins.Bottom * 25.4 / 100));
        }

        public void Unselect() {
            _ItemsToMove.Clear();
            Invalidate();
        }

        internal void DoMouseDown(System.Windows.Forms.MouseEventArgs e) {
            base.OnMouseDown(e);
            CheckHotItem(e, true);
            if (!_editAllowed) { return; }
            _LastQuickInfo = string.Empty;
            if (HotItem is IMouseAndKeyHandle ho2) {
                if (ho2.MouseDown(this, e, _Zoom, _shiftX, _shiftY)) {
                    _GivesMouseComandsTo = ho2;
                    return;
                }
            }
            if (e.Button == System.Windows.Forms.MouseButtons.Left) {
                var p = KoordinatesUnscaled(e);
                if (_ItemsToMove.Count > 0) {
                    foreach (var thisItem in _ItemsToMove) {
                        if (thisItem is BasicPadItem BPI) {
                            foreach (var thisPoint in BPI.MovablePoint) {
                                if (GeometryDF.Länge(thisPoint, new PointM(p)) < 5d / _Zoom) {
                                    SelectItem(thisPoint, false);
                                    return;
                                }
                            }
                        }
                    }
                }
                SelectItem(HotItem, ModifierKeys.HasFlag(System.Windows.Forms.Keys.Control));
            }
        }

        internal void DoMouseMove(System.Windows.Forms.MouseEventArgs e) {
            base.OnMouseMove(e);
            if (e.Button == System.Windows.Forms.MouseButtons.None) {
                CheckHotItem(e, false); // Für QuickInfo usw.
            }
            if (!_editAllowed) { return; }
            if (_GivesMouseComandsTo != null) {
                if (e.Button == System.Windows.Forms.MouseButtons.None && HotItem != _GivesMouseComandsTo) {
                    _GivesMouseComandsTo = null;
                    Invalidate();
                } else {
                    if (!_GivesMouseComandsTo.MouseMove(this, e, _Zoom, _shiftX, _shiftY)) {
                        _GivesMouseComandsTo = null;
                        Invalidate();
                    } else {
                        return;
                    }
                }
            } else {
                if (HotItem is IMouseAndKeyHandle Ho2) {
                    if (Ho2.MouseMove(this, e, _Zoom, _shiftX, _shiftY)) {
                        _GivesMouseComandsTo = Ho2;
                        Invalidate();
                        return;
                    }
                }
                _LastQuickInfo = HotItem != null && e.Button == System.Windows.Forms.MouseButtons.None ? HotItem.QuickInfo : string.Empty;
            }
            if (e.Button == System.Windows.Forms.MouseButtons.Left) {
                _LastQuickInfo = string.Empty;
                MoveItemsWithMouse();
                Refresh(); // Ansonsten werden einige Redraws übersprungen
            }
        }

        internal void DoMouseUp(System.Windows.Forms.MouseEventArgs e) {
            base.OnMouseUp(e);
            if (_GivesMouseComandsTo != null) {
                if (!_GivesMouseComandsTo.MouseUp(this, e, _Zoom, _shiftX, _shiftY)) {
                    _GivesMouseComandsTo = null;
                } else {
                    return;
                }
            }
            switch (e.Button) {
                case System.Windows.Forms.MouseButtons.Left:
                    if (!_editAllowed) { return; }
                    // Da ja evtl. nur ein Punkt verschoben wird, das Ursprüngliche Element wieder komplett auswählen.
                    BasicPadItem select = null;
                    if (_ItemsToMove.Count == 1 && _ItemsToMove[0] is PointM Thispoint) {
                        if (Thispoint.Parent is BasicPadItem item) {
                            select = item;
                        }
                    } else {
                        break; // sollen ja Items ein. Ansonsten wäre es immer ein Punkt
                    }
                    SelectItem(select, false);
                    break;

                case System.Windows.Forms.MouseButtons.Right:
                    FloatingInputBoxListBoxStyle.ContextMenuShow(this, e);
                    break;

                default:
                    break;
            }
            Invalidate();
        }

        internal Point MiddleOfVisiblesScreen() => new((int)(((Width / 2) + _shiftX) / _Zoom), (int)(((Height / 2) + _shiftY) / _Zoom));

        protected override void DrawControl(Graphics gr, enStates state) {
            LinearGradientBrush lgb = new(ClientRectangle, Color.White, Color.LightGray, LinearGradientMode.Vertical);
            gr.FillRectangle(lgb, ClientRectangle);
            _Item.DrawCreativePadTo(gr, Size, state, _Zoom, _shiftX, _shiftY, null, _ShowInPrintMode);

            #region Dann die selectiereren Punkte

            foreach (var thisItem in _ItemsToMove) {
                if (thisItem is BasicPadItem BPI) {
                    foreach (var P in BPI.MovablePoint) {
                        P.Draw(gr, _Zoom, _shiftX, _shiftY, enDesign.Button_EckpunktSchieber, enStates.Standard);
                    }
                }
                if (thisItem is PointM P2) {
                    if (P2.Parent is BasicPadItem BPI2) {
                        foreach (var P in BPI2.MovablePoint) {
                            P.Draw(gr, _Zoom, _shiftX, _shiftY, enDesign.Button_EckpunktSchieber_Phantom, enStates.Standard);
                        }
                    }
                    P2.Draw(gr, _Zoom, _shiftX, _shiftY, enDesign.Button_EckpunktSchieber, enStates.Standard);
                }
            }
            if (_GivesMouseComandsTo is BasicPadItem PA) {
                var drawingCoordinates = PA.UsedArea().ZoomAndMoveRect(_Zoom, _shiftX, _shiftY, false);
                gr.DrawRectangle(new Pen(Brushes.Red, 3), drawingCoordinates);
            }

            #endregion

            Skin.Draw_Border(gr, enDesign.Table_And_Pad, state, DisplayRectangle);
        }

        protected override bool IsInputKey(System.Windows.Forms.Keys keyData) =>
            // Ganz wichtig diese Routine!
            // Wenn diese NICHT ist, geht der Fokus weg, sobald der cursor gedrückt wird.
            // http://technet.microsoft.com/de-de/subscriptions/control.isinputkey%28v=vs.100%29
            keyData switch {
                System.Windows.Forms.Keys.Up or System.Windows.Forms.Keys.Down or System.Windows.Forms.Keys.Left or System.Windows.Forms.Keys.Right => true,
                _ => false,
            };

        protected override RectangleM MaxBounds() => _Item == null ? new RectangleM(0, 0, 0, 0) : _Item.MaxBounds(null);

        protected override void OnKeyUp(System.Windows.Forms.KeyEventArgs e) => DoKeyUp(e, true);

        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e) => DoMouseDown(e);

        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e) => DoMouseMove(e);

        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e) => DoMouseUp(e);

        private void _Item_ItemAdded(object sender, BlueBasics.EventArgs.ListEventArgs e) {
            CalculateZoomFitAndSliders(-1);
            if (_Item.Count == 1 || _Fitting) { ZoomFit(); }
        }

        // Kann nicht public gemacht werden, deswegen Umleitung

        // Kann nicht public gemacht werden, deswegen Umleitung

        // Kann nicht public gemacht werden, deswegen Umleitung

        private void _Item_ItemRemoved(object sender, System.EventArgs e) {
            CalculateZoomFitAndSliders(-1);

            if (_Fitting) { ZoomFit(); }

            CheckHotItem(null, true);
            Unselect();
            ZoomFit();
            Invalidate();
        }

        private void CheckHotItem(System.Windows.Forms.MouseEventArgs e, bool doLastClicked) {
            var OldClicked = LastClickedItem;
            var l = HotItems(e);
            var Mina = long.MaxValue;
            HotItem = null;
            if (e != null) {
                foreach (var ThisItem in l) {
                    var a = (long)Math.Abs(ThisItem.UsedArea().Width) * (long)Math.Abs(ThisItem.UsedArea().Height);
                    if (a <= Mina) {
                        // Gleich deswegen, dass neuere, IDENTISCHE Items dass oberste gewählt wird.
                        Mina = a;
                        HotItem = ThisItem;
                    }
                }
            }
            if (doLastClicked && HotItem != OldClicked) {
                LastClickedItem = HotItem;
                OnClickedItemChanged();
            }
        }

        private void DruckerDokument_BeginPrint(object sender, PrintEventArgs e) => OnBeginnPrint(e);

        private void DruckerDokument_EndPrint(object sender, PrintEventArgs e) => OnEndPrint(e);

        private void DruckerDokument_PrintPage(object sender, PrintPageEventArgs e) {
            e.HasMorePages = false;
            OnPrintPage(e);
            var i = _Item.ToBitmap(3);
            if (i == null) { return; }
            e.Graphics.DrawImageInRectAspectRatio(i, 0, 0, e.PageBounds.Width, e.PageBounds.Height);
        }

        private void Item_DoInvalidate(object sender, System.EventArgs e) => Invalidate();

        private void MoveItems(double x, double y, bool doSnap, bool modifyMouseDown) {
            PointM pointToMove = null;
            foreach (var thisIt in _ItemsToMove) {
                if (thisIt is PointM p) { pointToMove = p; break; }
            }
            if (pointToMove == null) {
                foreach (var thisIt in _ItemsToMove) {
                    if (thisIt is BasicPadItem BPI && BPI.PointsForSuccesfullyMove.Count > 0) {
                        pointToMove = BPI.PointsForSuccesfullyMove[0]; break;
                    }
                }
            }
            if (pointToMove != null && x != 0d) { x = SnapToGrid(true, pointToMove, x); }
            if (pointToMove != null && y != 0d) { y = SnapToGrid(false, pointToMove, y); }

            if (x == 0d && y == 0d) { return; }

            foreach (var thisIt in _ItemsToMove) {
                thisIt.Move(x, y);
            }
            if (doSnap && modifyMouseDown) {
                // Maus-Daten modifizieren, da ja die tasächliche Bewegung wegen der SnapPoints abweichen kann.
                MouseDownPos_1_1 = new Point((int)(MouseDownPos_1_1.X + x), (int)(MouseDownPos_1_1.Y + y));
            }
        }

        private void MoveItemsWithMouse() => MoveItems(MousePos_1_1.X - MouseDownPos_1_1.X, MousePos_1_1.Y - MouseDownPos_1_1.Y, true, true);

        private void OnBeginnPrint(PrintEventArgs e) => BeginnPrint?.Invoke(this, e);

        private void OnClickedItemChanged() => ClickedItemChanged?.Invoke(this, System.EventArgs.Empty);

        private void OnEndPrint(PrintEventArgs e) => EndPrint?.Invoke(this, e);

        private void OnGotNewItemCollection() => GotNewItemCollection?.Invoke(this, System.EventArgs.Empty);

        private void OnPreviewModeChanged() => PreviewModeChanged?.Invoke(this, System.EventArgs.Empty);

        private void OnPrintPage(PrintPageEventArgs e) => PrintPage?.Invoke(this, e);

        private void PicsSave_FileOk(object sender, CancelEventArgs e) {
            if (e.Cancel) { return; }
            _Item.SaveAsBitmap(PicsSave.FileName);
        }

        private void RepairPrinterData() {
            if (RepairPrinterData_Prepaired) { return; }
            RepairPrinterData_Prepaired = true;
            DruckerDokument.DocumentName = _Item.Caption;
            var done = false;
            foreach (PaperSize ps in DruckerDokument.PrinterSettings.PaperSizes) {
                if (ps.Width == (int)(_Item.SheetSizeInMM.Width / 25.4 * 100) && ps.Height == (int)(_Item.SheetSizeInMM.Height / 25.4 * 100)) {
                    done = true;
                    DruckerDokument.DefaultPageSettings.PaperSize = ps;
                    break;
                }
            }
            if (!done) {
                DruckerDokument.DefaultPageSettings.PaperSize = new PaperSize("Custom", (int)(_Item.SheetSizeInMM.Width / 25.4 * 100), (int)(_Item.SheetSizeInMM.Height / 25.4 * 100));
            }
            DruckerDokument.DefaultPageSettings.PrinterResolution = DruckerDokument.DefaultPageSettings.PrinterSettings.PrinterResolutions[0];
            DruckerDokument.OriginAtMargins = true;
            DruckerDokument.DefaultPageSettings.Margins = new Margins((int)(_Item.RandinMM.Left / 25.4 * 100), (int)(_Item.RandinMM.Right / 25.4 * 100), (int)(_Item.RandinMM.Top / 25.4 * 100), (int)(_Item.RandinMM.Bottom / 25.4 * 100));
        }

        private double SnapToGrid(bool doX, PointM movedPoint, double mouseMovedTo) {
            if (Item.SnapMode != enSnapMode.SnapToGrid || Math.Abs(Item.GridSnap) < 0.001) { return mouseMovedTo; }
            if (movedPoint is null) { return 0D; }

            var Multi = Converter.mmToPixel(Item.GridSnap, ItemCollectionPad.DPI);
            double Value;
            if (doX) {
                Value = movedPoint.X + mouseMovedTo;
                Value = (int)(Value / Multi) * Multi;
                // Formel umgestellt
                return Value - movedPoint.X;
            }
            Value = movedPoint.Y + mouseMovedTo;
            Value = (int)(Value / Multi) * Multi;
            return Value - movedPoint.Y;
        }

        #endregion
    }
}