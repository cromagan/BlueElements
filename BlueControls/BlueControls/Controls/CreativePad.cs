#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2021 Christian Peter
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
#endregion


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
using System.Drawing.Printing;

namespace BlueControls.Controls {

    [Designer(typeof(BasicDesigner))]
    [DefaultEvent("Click")]
    public sealed partial class CreativePad : ZoomPad, IContextMenu {

        #region Constructor

        public CreativePad(ItemCollectionPad itemCollectionPad) : base() {
            // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
            InitializeComponent();


            // Initialisierungen nach dem Aufruf InitializeComponent() hinzufügen

            Item = itemCollectionPad;

            Unselect();
            _MouseHighlight = false;
        }



        public CreativePad() : this(new ItemCollectionPad()) { }

        #endregion

        private IMouseAndKeyHandle _GivesMouseComandsTo;

        private bool _ShowInPrintMode;

        private ItemCollectionPad _Item;

        //private readonly List<BasicPadItem> _ItemsSelected = new();
        private readonly List<IMoveable> _ItemsToMove = new();


        private string _LastQuickInfo = string.Empty;

        public BasicPadItem HotItem { get; private set; } = null;
        public BasicPadItem LastClickedItem { get; private set; } = null;


        private bool RepairPrinterData_Prepaired;

        public bool _editAllowed = true;


        #region  Events 
        public event EventHandler<ContextMenuInitEventArgs> ContextMenuInit;
        public event EventHandler<ContextMenuItemClickedEventArgs> ContextMenuItemClicked;

        public event PrintPageEventHandler PrintPage;

        public event PrintEventHandler BeginnPrint;
        public event PrintEventHandler EndPrint;

        public event EventHandler PreviewModeChanged;
        public event EventHandler ClickedItemChanged;
        #endregion


        #region  Properties 

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

        [DefaultValue(true)]
        public bool EditAllowed {
            get => _editAllowed;
            set => _editAllowed = value;
        }



        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ItemCollectionPad Item {
            get => _Item;
            set {

                if (_Item == value) { return; }


                if (_Item != null) {
                    _Item.ItemRemoved -= _Item_ItemRemoved;
                    _Item.DoInvalidate -= Item_DoInvalidate;
                }

                _Item = value;

                if (_Item != null) {
                    _Item.ItemRemoved += _Item_ItemRemoved;
                    _Item.DoInvalidate += Item_DoInvalidate;
                    _Item.OnDoInvalidate();
                }
            }
        }


        #endregion


        private void OnPreviewModeChanged() {
            PreviewModeChanged?.Invoke(this, System.EventArgs.Empty);
        }



        internal Point MiddleOfVisiblesScreen() {
            return new Point((int)((Width / 2 + _shiftX) / _Zoom), (int)((Height / 2 + _shiftY) / _Zoom));
        }



        private void Item_DoInvalidate(object sender, System.EventArgs e) {
            Invalidate();
        }

        protected override RectangleM MaxBounds() {
            if (_Item == null) { return new RectangleM(0, 0, 0, 0); }

            return _Item.MaxBounds(null);
        }
        protected override bool IsInputKey(System.Windows.Forms.Keys keyData) {
            // Ganz wichtig diese Routine!
            // Wenn diese NICHT ist, geht der Fokus weg, sobald der cursor gedrückt wird.
            // http://technet.microsoft.com/de-de/subscriptions/control.isinputkey%28v=vs.100%29

            switch (keyData) {
                case System.Windows.Forms.Keys.Up:
                case System.Windows.Forms.Keys.Down:
                case System.Windows.Forms.Keys.Left:
                case System.Windows.Forms.Keys.Right:
                    return true;
            }

            return false;

        }

        protected override void OnKeyUp(System.Windows.Forms.KeyEventArgs e) => DoKeyUp(e, true); // Kann nicht public gemacht werden, deswegen Umleitung


        public void DoKeyUp(System.Windows.Forms.KeyEventArgs e, bool hasbase) {

            // Ganz seltsam: Wird BAse.OnKeyUp IMMER ausgelöst, passiert folgendes:
            // Wird ein Objekt gelöscht, wird anschließend das OnKeyUp Ereignis nicht mehr ausgelöst.

            if (hasbase) { base.OnKeyUp(e); }

            if (!_editAllowed) { return; }


            if (_GivesMouseComandsTo != null) {
                if (_GivesMouseComandsTo.KeyUp(this, e, _Zoom, _shiftX, _shiftY)) { return; }
            }

            var Multi = 1m;

            if (Item.SnapMode == enSnapMode.SnapToGrid) {
                Multi = modConverter.mmToPixel((decimal)Item.GridSnap, ItemCollectionPad.DPI);
            }

            if (Multi < 1) { Multi = 1m; }


            switch (e.KeyCode) {
                case System.Windows.Forms.Keys.Delete:
                case System.Windows.Forms.Keys.Back:

                    var ItemsDoDelete = new List<BasicPadItem>();
                    foreach (var thisit in _ItemsToMove) {
                        if (thisit is BasicPadItem bi) { ItemsDoDelete.Add(bi); }
                    }
                    Unselect();
                    _Item.RemoveRange(ItemsDoDelete);
                    break;

                case System.Windows.Forms.Keys.Up:
                    MoveItems(0M, -1 * Multi, false, false);
                    break;

                case System.Windows.Forms.Keys.Down:
                    MoveItems(0M, 1 * Multi, false, false);
                    break;

                case System.Windows.Forms.Keys.Left:
                    MoveItems(-1 * Multi, 0M, false, false);
                    break;

                case System.Windows.Forms.Keys.Right:
                    MoveItems(1 * Multi, 0M, false, false);
                    break;
            }
        }

        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e) {
            DoMouseDown(e); // Kann nicht public gemacht werden, deswegen Umleitung
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
                                if (GeometryDF.Länge(thisPoint, new PointM(p)) < 5m / _Zoom) {
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


        public void SelectItem(IMoveable item, bool additional) {
            if (!additional) { Unselect(); }
            if (item == null) { return; }
            _ItemsToMove.Add(item);
            Invalidate();
        }

        public void Unselect() {
            _ItemsToMove.Clear();
        }


        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e) => DoMouseMove(e); // Kann nicht public gemacht werden, deswegen Umleitung


        internal void DoMouseMove(System.Windows.Forms.MouseEventArgs e) {
            base.OnMouseMove(e);

            if (e.Button == System.Windows.Forms.MouseButtons.None) {
                CheckHotItem(e, false); // Für QuickInfo usw.
            }

            if(!_editAllowed) { return; }

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

                if (HotItem != null && e.Button == System.Windows.Forms.MouseButtons.None) {

                    _LastQuickInfo = HotItem.QuickInfo;
                } else {
                    _LastQuickInfo = string.Empty;
                }



            }

            if (e.Button == System.Windows.Forms.MouseButtons.Left) {
                _LastQuickInfo = string.Empty;
                MoveItemsWithMouse();
                Refresh(); // Ansonsten werden einige Redraws übersprungen
            }
        }


        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e) {
            DoMouseUp(e); // Kann nicht public gemacht werden, deswegen Umleitung
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


        internal void DrawCreativePadTo(Graphics gr, Size maxs, enStates state, decimal zoom, decimal X, decimal Y, List<BasicPadItem> visibleItems) {
            try {
                gr.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;

                if (_Item.SheetSizeInMM.Width > 0 && _Item.SheetSizeInMM.Height > 0) {
                    Skin.Draw_Back(gr, enDesign.Table_And_Pad, state, DisplayRectangle, this, true);
                    var SSW = Math.Round(modConverter.mmToPixel((decimal)_Item.SheetSizeInMM.Width, ItemCollectionPad.DPI), 1);
                    var SSH = Math.Round(modConverter.mmToPixel((decimal)_Item.SheetSizeInMM.Height, ItemCollectionPad.DPI), 1);
                    var LO = new PointM(0m, 0m).ZoomAndMove(zoom, X, Y);
                    var RU = new PointM(SSW, SSH).ZoomAndMove(zoom, X, Y);

                    var R = new Rectangle((int)LO.X, (int)LO.Y, (int)(RU.X - LO.X), (int)(RU.Y - LO.Y));
                    gr.FillRectangle(Brushes.White, R);
                    gr.FillRectangle(new SolidBrush(Item.BackColor), R);
                    gr.DrawRectangle(PenGray, R);

                    if (!_ShowInPrintMode) {
                        var rLO = new PointM(_Item.P_rLO.X, _Item.P_rLO.Y).ZoomAndMove(zoom, X, Y);
                        var rRU = new PointM(_Item.P_rRU.X, _Item.P_rRU.Y).ZoomAndMove(zoom, X, Y);
                        var Rr = new Rectangle((int)rLO.X, (int)rLO.Y, (int)(rRU.X - rLO.X), (int)(rRU.Y - rLO.Y));
                        gr.DrawRectangle(PenGray, Rr);
                    }

                } else {
                    gr.Clear(Color.White);
                    gr.Clear(Item.BackColor);
                }
                gr.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
                if (!_Item.Draw(gr, zoom, X, Y, maxs, _ShowInPrintMode, visibleItems)) {
                    DrawCreativePadTo(gr, maxs, state, zoom, X, Y, visibleItems);
                    return;
                }




                // Dann die selectiereren Punkte

                foreach (var thisItem in _ItemsToMove) {
                    if (thisItem is BasicPadItem BPI) {
                        foreach (var P in BPI.MovablePoint) {
                            P.Draw(gr, zoom, X, Y, enDesign.Button_EckpunktSchieber, enStates.Standard);
                        }
                    }

                    if (thisItem is PointM P2) {


                        if (P2.Parent is BasicPadItem BPI2) {
                            foreach (var P in BPI2.MovablePoint) {
                                P.Draw(gr, zoom, X, Y, enDesign.Button_EckpunktSchieber_Phantom, enStates.Standard);
                            }
                        }

                        P2.Draw(gr, zoom, X, Y, enDesign.Button_EckpunktSchieber, enStates.Standard);
                    }


                }





                if (_GivesMouseComandsTo is BasicPadItem PA) {
                    var DCoordinates = PA.UsedArea().ZoomAndMoveRect(zoom, X, Y, false);

                    gr.DrawRectangle(new Pen(Brushes.Red, 3), DCoordinates);
                }

                //   TMPGR.Dispose();
            } catch {
                DrawCreativePadTo(gr, maxs, state, zoom, X, Y, visibleItems);
            }
        }

        internal void DrawCreativePadToBitmap(Bitmap BMP, enStates vState, decimal zoomf, decimal X, decimal Y, List<BasicPadItem> visibleItems) {

            var gr = Graphics.FromImage(BMP);
            DrawCreativePadTo(gr, BMP.Size, vState, zoomf, X, Y, visibleItems);
            gr.Dispose();
        }

        protected override void DrawControl(Graphics gr, enStates state) {
            DrawCreativePadTo(gr, Size, state, _Zoom, _shiftX, _shiftY, null);
            Skin.Draw_Border(gr, enDesign.Table_And_Pad, state, DisplayRectangle);
        }

        private void _Item_ItemRemoved(object sender, System.EventArgs e) {
            CheckHotItem(null, true);
            Unselect();
            ZoomFit();
            Invalidate();
        }


        private void MoveItemsWithMouse() => MoveItems(MousePos_1_1.X - MouseDownPos_1_1.X, MousePos_1_1.Y - MouseDownPos_1_1.Y, true, true);

        private void MoveItems(decimal x, decimal y, bool doSnap, bool modifyMouseDown) {

            //PointM ThisPointSnapsX = null;
            //PointM ThisPointSnapsY = null;
            //PointM PointSnapedToX = null;
            //PointM PointSnapedToY = null;

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



            if (pointToMove != null && x != 0m) {
                x = SnapToGrid(true, pointToMove, x);

                //    if (!_Grid || Math.Abs(_Gridsnap) < 0.001 || move.X == 0M) {
                //        if (_AutoRelation != enAutoRelationMode.None) { SnapToPoint(enXY.X, _pointsSelected, move, ref ThisPointSnapsX, ref PointSnapedToX); }
                //        if (ThisPointSnapsX != null) { move.X = PointSnapedToX.X - ThisPointSnapsX.X; }
                //    }
                //} else {
                //    move.X = 0M;
            }

            if (pointToMove != null && y != 0m) {
                y = SnapToGrid(false, pointToMove, y);

                //    if (!_Grid || Math.Abs(_Gridsnap) < 0.001 || move.Y == 0M) {
                //        if (_AutoRelation != enAutoRelationMode.None) { SnapToPoint(enXY.Y, _pointsSelected, move, ref ThisPointSnapsY, ref PointSnapedToY); }
                //        if (ThisPointSnapsY != null) { move.Y = PointSnapedToY.Y - ThisPointSnapsY.Y; }
                //    }
                //} else {
                //    move.Y = 0M;
            }

            if (x == 0M && y == 0M) { return; }



            //if (MoveSelectedPoints(move.X, move.Y)) {
            //    // Wenn Strongmode true ist es für den Anwender unerklärlich, warum er denn nix macht,obwohl kein Fehler da ist...
            //    if (_Item.NotPerforming(false) == 0) {
            //        AddAllAutoRelations(ThisPointSnapsX, PointSnapedToX, ThisPointSnapsY, PointSnapedToY);
            //    } else {
            //        _NewAutoRelations.Clear();
            //    }
            //}

            foreach (var thisIt in _ItemsToMove) {
                thisIt.Move(x, y);
            }


            if (doSnap && modifyMouseDown) {
                // Maus-Daten modifizieren, da ja die tasächliche Bewegung wegen der SnapPoints abweichen kann.
                MouseDownPos_1_1 = new Point((int)(MouseDownPos_1_1.X + x), (int)(MouseDownPos_1_1.Y + y));

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doX"></param>
        /// <param name="movedPoint">Die zu verschiebenden (testenden) Punkte. Die Reihenfolge muss nach Wichtigkeit sortiert sein</param>
        /// <param name="mouseMovedTo"></param>
        /// <returns></returns>
        private decimal SnapToGrid(bool doX, PointM movedPoint, decimal mouseMovedTo) {

            if (Item.SnapMode != enSnapMode.SnapToGrid || Math.Abs(Item.GridSnap) < 0.001) { return mouseMovedTo; }

            if (movedPoint == null || movedPoint == null) { return 0M; }

            //PointM MasterPoint = null;
            //PointM LowOrderPoint = null;
            //f 

            //foreach (var thisPoint in Movep) {
            //    if (thisPoint == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Punkt verworfen"); }// Keine Lust das zu berücksichten. Muss halt stimmen!
            //    if (thisPoint.PrimaryGridSnapPoint) {
            //        if (MasterPoint == null) { MasterPoint = thisPoint; }
            //    } else if (LowOrderPoint == null) {
            //        LowOrderPoint = thisPoint;
            //    }
            //}

            //if (MasterPoint == null) { MasterPoint = LowOrderPoint; }

            var Multi = modConverter.mmToPixel((decimal)Item.GridSnap, ItemCollectionPad.DPI);
            decimal Value;

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


        //private void SnapToPoint(enXY toCheck, List<PointM> movedPoints, PointM move, ref PointM thisPointSnaps, ref PointM snapedToPoint) {
        //    decimal ShortestDist = 10;
        //    var Nearest = decimal.MaxValue;


        //    if (!_AutoRelation.HasFlag(enAutoRelationMode.Senkrecht) &&
        //        !_AutoRelation.HasFlag(enAutoRelationMode.Waagerecht) &&
        //        !_AutoRelation.HasFlag(enAutoRelationMode.DirektVerbindungen)) {
        //        return;
        //    }

        //    if (_AutoRelation.HasFlag(enAutoRelationMode.DirektVerbindungen)) {
        //        if (!_AutoRelation.HasFlag(enAutoRelationMode.Senkrecht) && !_AutoRelation.HasFlag(enAutoRelationMode.Waagerecht)) { Nearest = 2; }
        //    } else {
        //        if (toCheck.HasFlag(enXY.X) && !_AutoRelation.HasFlag(enAutoRelationMode.Senkrecht)) { return; }
        //        if (toCheck.HasFlag(enXY.Y) && !_AutoRelation.HasFlag(enAutoRelationMode.Waagerecht)) { return; }

        //    }


        //    // Berechne, welcher Punkt snappt
        //    foreach (var thisPoint in movedPoints) {
        //        SnapToPoint(toCheck, thisPoint, move, ref ShortestDist, ref thisPointSnaps, ref snapedToPoint, ref Nearest);
        //    }

        //}






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

        public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) {
            ContextMenuItemClicked?.Invoke(this, e);
        }

        public List<BasicPadItem> HotItems(System.Windows.Forms.MouseEventArgs e) {

            if (e == null) { return new List<BasicPadItem>(); }


            var P = new Point((int)((e.X + _shiftX) / _Zoom), (int)((e.Y + _shiftY) / _Zoom));

            var l = new List<BasicPadItem>();

            foreach (var ThisItem in _Item) {
                if (ThisItem != null && ThisItem.Contains(P, _Zoom)) {
                    l.Add(ThisItem);
                }
            }
            return l;
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

        public void OnContextMenuInit(ContextMenuInitEventArgs e) {
            ContextMenuInit?.Invoke(this, e);
        }





        public void Print() {
            DruckerDokument.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
            if (PrintDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                PrintDialog1.Document.Print();
            }
            RepairPrinterData();
        }

        public void ShowPrintPreview() {
            DruckerDokument.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
            PrintPreviewDialog1.ShowDialog();
            RepairPrinterData();
        }


        private void DruckerDokument_PrintPage(object sender, PrintPageEventArgs e) {
            e.HasMorePages = false;

            OnPrintPage(e);


            var i = _Item.ToBitmap(3);
            if (i == null) { return; }
            e.Graphics.DrawImageInRectAspectRatio(i, 0, 0, e.PageBounds.Width, e.PageBounds.Height);
        }

        private void OnPrintPage(PrintPageEventArgs e) {
            PrintPage?.Invoke(this, e);
        }

        public void ShowPrinterPageSetup() {

            RepairPrinterData();

            var x = new BlueControls.Forms.PageSetupDialog(DruckerDokument, false);
            x.ShowDialog();
            x.Dispose();

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


        public void ShowWorkingAreaSetup() {

            var OriD = new PrintDocument();


            OriD.DefaultPageSettings.Landscape = false;
            OriD.DefaultPageSettings.PaperSize = new PaperSize("Benutzerdefiniert", (int)(_Item.SheetSizeInMM.Width / 25.4 * 100), (int)(_Item.SheetSizeInMM.Height / 25.4 * 100));
            OriD.DefaultPageSettings.Margins.Top = (int)(_Item.RandinMM.Top / 25.4 * 100);
            OriD.DefaultPageSettings.Margins.Bottom = (int)(_Item.RandinMM.Bottom / 25.4 * 100);
            OriD.DefaultPageSettings.Margins.Left = (int)(_Item.RandinMM.Left / 25.4 * 100);
            OriD.DefaultPageSettings.Margins.Right = (int)(_Item.RandinMM.Right / 25.4 * 100);




            using (var x = new BlueControls.Forms.PageSetupDialog(OriD, true)) {
                x.ShowDialog();
                if (x.Canceled()) { return; }
            }


            _Item.SheetSizeInMM = new SizeF((int)(OriD.DefaultPageSettings.PaperSize.Width * 25.4 / 100), (int)(OriD.DefaultPageSettings.PaperSize.Height * 25.4 / 100));
            _Item.RandinMM = new System.Windows.Forms.Padding((int)(OriD.DefaultPageSettings.Margins.Left * 25.4 / 100), (int)(OriD.DefaultPageSettings.Margins.Top * 25.4 / 100), (int)(OriD.DefaultPageSettings.Margins.Right * 25.4 / 100), (int)(OriD.DefaultPageSettings.Margins.Bottom * 25.4 / 100));

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

        public void OpenSaveDialog(string title, string optionalFileName) {

            title = title.RemoveChars(Constants.Char_DateiSonderZeichen);

            PicsSave.FileName = title + ".png";

            PicsSave.ShowDialog();
        }


        private void PicsSave_FileOk(object sender, CancelEventArgs e) {
            if (e.Cancel) { return; }
            _Item.SaveAsBitmap(PicsSave.FileName);
        }







        public void ShowErweitertMenü(BasicPadItem Item) {
            var l = Item.GetStyleOptions();

            if (l.Count == 0) {
                MessageBox.Show("Objekt hat keine<br>Bearbeitungsmöglichkeiten.", enImageCode.Information);
                return;
            }

            EditBoxFlexiControl.Show(l);
        }


        private void DruckerDokument_BeginPrint(object sender, PrintEventArgs e) => OnBeginnPrint(e);
        private void DruckerDokument_EndPrint(object sender, PrintEventArgs e) => OnEndPrint(e);
        private void OnClickedItemChanged() => ClickedItemChanged?.Invoke(this, System.EventArgs.Empty);
        private void OnBeginnPrint(PrintEventArgs e) => BeginnPrint?.Invoke(this, e);
        private void OnEndPrint(PrintEventArgs e) => EndPrint?.Invoke(this, e);
        public override string QuickInfoText => _LastQuickInfo;



    }
}