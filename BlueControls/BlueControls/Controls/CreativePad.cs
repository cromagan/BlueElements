﻿#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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

        public static bool Debug_ShowPointOrder = false;
        public static bool Debug_ShowRelationOrder = false;
        private IMouseAndKeyHandle _GivesMouseComandsTo;


        private enAutoRelationMode _AutoRelation = enAutoRelationMode.Alle_Erhalten;
        private bool _ShowInPrintMode;


        /// <summary>
        /// Die Auto-Beziehungen, die hinzukommen (würden)
        /// </summary>
        private readonly List<clsPointRelation> _NewAutoRelations = new List<clsPointRelation>();



        private List<PointM> _pointsOnScreenWichAreNotSelected = null;


        /// <summary>
        /// Die Punkte, die zum Schieben markiert sind.
        /// </summary>
        private readonly ListExt<PointM> _pointsSelected = new ListExt<PointM>();
        /// <summary>
        /// Diese Punkte bewegen sich in der X-Richtung mit
        /// </summary>
        private readonly List<PointM> _pointsToMoveX = new List<PointM>();
        /// <summary>
        /// Diese Punkte bewegen sich in der Y-Richtung mit
        /// </summary>
        private readonly List<PointM> _pointsToMoveY = new List<PointM>();

        private readonly List<BasicPadItem> _ItemsToMove = new List<BasicPadItem>();


        private string _LastQuickInfo = string.Empty;

        private bool _Grid;
        private float _GridShow = 10;
        private float _Gridsnap = 1;

        public BasicPadItem HotItem { get; private set; } = null;
        public BasicPadItem LastClickedItem { get; private set; } = null;


        private bool RepairPrinterData_Prepaired;
        private ItemCollectionPad _Item;


        //private DateTime lastredraw = DateTime.Now;

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
            get {
                return _ShowInPrintMode;
            }
            set {

                if (_ShowInPrintMode == value) { return; }

                _ShowInPrintMode = value;
                OnPreviewModeChanged();
                Invalidate();
            }
        }

        private void OnPreviewModeChanged() {
            PreviewModeChanged?.Invoke(this, System.EventArgs.Empty);
        }

        [DefaultValue(false)]
        public bool Grid {
            get {
                return _Grid;
            }
            set {

                if (_Grid == value) { return; }
                _Grid = value;
                CheckGrid();
            }
        }

        internal Point MiddleOfVisiblesScreen() {
            return new Point((int)((Width / 2 + _shiftX) / _Zoom), (int)((Height / 2 + _shiftY) / _Zoom));
        }


        [DefaultValue(10.0)]
        public float GridShow {
            get {
                return _GridShow;
            }
            set {

                if (_GridShow == value) { return; }

                _GridShow = value;
                CheckGrid();
            }
        }

        [DefaultValue(10.0)]
        public float GridSnap {
            get {
                return _Gridsnap;
            }
            set {

                if (_Gridsnap == value) { return; }

                _Gridsnap = value;
                CheckGrid();
            }
        }

        [DefaultValue(enAutoRelationMode.Alle_Erhalten)]
        public enAutoRelationMode AutoRelation {
            get {
                return _AutoRelation;
            }
            set {
                _AutoRelation = value;

                if (_Item != null) {
                    foreach (BasicPadItem thisItem in _Item) {
                        if (thisItem is ChildPadItem cpi) {
                            if (cpi.PadInternal != null) {
                                cpi.PadInternal.AutoRelation = value;
                            }
                        }
                    }
                }
            }
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


            if (_GivesMouseComandsTo != null) {
                if (_GivesMouseComandsTo.KeyUp(this, e, _Zoom, _shiftX, _shiftY)) { return; }
            }


            decimal Multi = modConverter.mmToPixel((decimal)_Gridsnap, ItemCollectionPad.DPI);


            switch (e.KeyCode) {
                case System.Windows.Forms.Keys.Delete:
                case System.Windows.Forms.Keys.Back:

                    List<BasicPadItem> ItemsDoDelete = new List<BasicPadItem>();
                    foreach (PointM ThisPoint in _pointsSelected) {
                        if (ThisPoint?.Parent is BasicPadItem bpi) { ItemsDoDelete.Add(bpi); }
                    }

                    Unselect();
                    _Item.RemoveRange(ItemsDoDelete);
                    break;

                case System.Windows.Forms.Keys.Up:
                    MoveSelectedPoints(0M, -1 * Multi);
                    break;

                case System.Windows.Forms.Keys.Down:
                    MoveSelectedPoints(0M, 1 * Multi);
                    break;

                case System.Windows.Forms.Keys.Left:
                    MoveSelectedPoints(-1 * Multi, 0M);
                    break;

                case System.Windows.Forms.Keys.Right:
                    MoveSelectedPoints(1 * Multi, 0M);
                    break;
            }
        }

        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e) => DoMouseDown(e); // Kann nicht public gemacht werden, deswegen Umleitung
        internal void DoMouseDown(System.Windows.Forms.MouseEventArgs e) {
            base.OnMouseDown(e);


            CheckHotItem(e, true);


            _LastQuickInfo = string.Empty;

            if (HotItem is IMouseAndKeyHandle ho2) {
                if (ho2.MouseDown(this, e, _Zoom, _shiftX, _shiftY)) {
                    _GivesMouseComandsTo = ho2;
                    return;
                }
            }


            if (e.Button == System.Windows.Forms.MouseButtons.Left) {
                Point p = KoordinatesUnscaled(e);
                foreach (PointM thisPoint in _pointsSelected) {

                    if (thisPoint.UserSelectable && GeometryDF.Länge(thisPoint, new PointM(p)) < 5m / _Zoom) {

                        if (!thisPoint.CanMove(_Item.AllRelations)) {
                            Invalidate();
                            Forms.QuickInfo.Show("Dieser Punkt ist fest definiert<br>und kann nicht verschoben werden.");
                            return;
                        }

                        SelectPoint(thisPoint);
                        return;
                    }
                }
                SelectItem(HotItem, ModifierKeys.HasFlag(System.Windows.Forms.Keys.Control));
            }
        }

        public void SelectPoint(PointM point) {
            Unselect();
            if (point == null) { return; }

            _pointsSelected.Add(point);
            Item.InvalidateOrder();
            ComputeMovingData();
            Invalidate();
        }


        public void SelectItem(BasicPadItem item, bool additional) {


            if (!additional) { Unselect(); }

            if (item == null) { return; }


            foreach (PointM thisPoint in _pointsSelected) {
                if (thisPoint.Parent == item) { return; }
            }


            _pointsSelected.AddIfNotExists(item.Points);
            Item.InvalidateOrder();
            ComputeMovingData();
            Invalidate();

        }

        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e) => DoMouseMove(e); // Kann nicht public gemacht werden, deswegen Umleitung
        internal void DoMouseMove(System.Windows.Forms.MouseEventArgs e) {
            base.OnMouseMove(e);

            if (e.Button == System.Windows.Forms.MouseButtons.None) {
                CheckHotItem(e, false); // Für QuickInfo usw.
            }

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

            if (MouseDownPos_1_1 != null && e.Button == System.Windows.Forms.MouseButtons.Left) {
                _LastQuickInfo = string.Empty;
                MoveItems();
                Refresh(); // Ansonsten werden einige Redraws übersprungen
            }
        }


        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e) => DoMouseUp(e); // Kann nicht public gemacht werden, deswegen Umleitung
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
                    // Da ja evtl. nur ein Punkt verschoben wird, das Ursprüngliche Element wieder komplett auswählen.
                    BasicPadItem select = null;
                    foreach (PointM Thispoint in _pointsSelected) {
                        if (Thispoint.Parent is BasicPadItem item) {
                            select = item;
                            break;
                        }
                    }

                    if (_AutoRelation.HasFlag(enAutoRelationMode.NurBeziehungenErhalten) && _NewAutoRelations.Count > 0) {
                        _Item.AllRelations.AddRange(_NewAutoRelations);
                    }

                    SelectItem(select, false);
                    break;

                case System.Windows.Forms.MouseButtons.Right:
                    FloatingInputBoxListBoxStyle.ContextMenuShow(this, e);
                    break;

                default:

                    break;


            }



            _Item.PerformAllRelations();
            //ComputeMovingData();
            Invalidate(); // Damit auch snap-Punkte wieder gelöscht werden
        }


        internal void DrawCreativePadTo(Graphics gr, Size maxs, enStates state, decimal zoom, decimal X, decimal Y, List<BasicPadItem> visibleItems) {
            try {
                gr.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;

                if (_Item.SheetSizeInMM.Width > 0 && _Item.SheetSizeInMM.Height > 0) {
                    Skin.Draw_Back(gr, enDesign.Table_And_Pad, state, DisplayRectangle, this, true);
                    decimal SSW = Math.Round(modConverter.mmToPixel((decimal)_Item.SheetSizeInMM.Width, ItemCollectionPad.DPI), 1);
                    decimal SSH = Math.Round(modConverter.mmToPixel((decimal)_Item.SheetSizeInMM.Height, ItemCollectionPad.DPI), 1);
                    PointF LO = new PointM(0m, 0m).ZoomAndMove(zoom, X, Y);
                    PointF RU = new PointM(SSW, SSH).ZoomAndMove(zoom, X, Y);

                    Rectangle R = new Rectangle((int)LO.X, (int)LO.Y, (int)(RU.X - LO.X), (int)(RU.Y - LO.Y));
                    gr.FillRectangle(Brushes.White, R);
                    gr.FillRectangle(new SolidBrush(Item.BackColor), R);
                    gr.DrawRectangle(PenGray, R);

                    if (!_ShowInPrintMode) {
                        PointF rLO = new PointM(_Item.P_rLO.X, _Item.P_rLO.Y).ZoomAndMove(zoom, X, Y);
                        PointF rRU = new PointM(_Item.P_rRU.X, _Item.P_rRU.Y).ZoomAndMove(zoom, X, Y);
                        Rectangle Rr = new Rectangle((int)rLO.X, (int)rLO.Y, (int)(rRU.X - rLO.X), (int)(rRU.Y - rLO.Y));
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


                // Erst Beziehungen, weil die die Grauen Punkte zeichnet
                foreach (clsPointRelation ThisRelation in _Item.AllRelations) {
                    ThisRelation.Draw(gr, zoom, X, Y, _Item.AllRelations.IndexOf(ThisRelation));
                }


                if (Debug_ShowPointOrder) {

                    // Alle Punkte mit Order anzeigen
                    foreach (PointM ThisPoint in _Item.AllPoints) {
                        ThisPoint.Draw(gr, zoom, X, Y, enDesign.Button_EckpunktSchieber_Phantom, enStates.Standard, Item.AllPoints.IndexOf(ThisPoint).ToString());
                    }
                }


                //     If _ItemsEditable Then

                // Dann die selectiereren Punkte

                foreach (PointM ThisPoint in _pointsSelected) {
                    if (ThisPoint.CanMove(_Item.AllRelations)) {
                        ThisPoint.Draw(gr, zoom, X, Y, enDesign.Button_EckpunktSchieber, enStates.Standard, string.Empty);
                    } else {
                        ThisPoint.Draw(gr, zoom, X, Y, enDesign.Button_EckpunktSchieber_Phantom, enStates.Standard, string.Empty);
                    }
                }


                foreach (clsPointRelation ThisRelation in _NewAutoRelations) {


                    PointF P1 = ThisRelation.Points[0].ZoomAndMove(zoom, X, Y);
                    PointF P2 = ThisRelation.Points[1].ZoomAndMove(zoom, X, Y);

                    if (ThisRelation.RelationType == enRelationType.WaagerechtSenkrecht) {
                        gr.DrawEllipse(new Pen(Color.Green, 3), P1.X - 4, P1.Y - 3, 7, 7);
                        gr.DrawEllipse(new Pen(Color.Green, 3), P2.X - 4, P2.Y - 3, 7, 7);
                        gr.DrawLine(new Pen(Color.Green, 1), P1, P2);
                    } else {
                        gr.DrawEllipse(new Pen(Color.Green, 3), P1.X - 4, P1.Y - 4, 7, 7);
                        gr.DrawEllipse(new Pen(Color.Green, 3), P1.X - 8, P1.Y - 8, 15, 15);
                    }
                }



                if (_GivesMouseComandsTo is BasicPadItem PA) {
                    RectangleF DCoordinates = PA.UsedArea().ZoomAndMoveRect(zoom, X, Y);

                    gr.DrawRectangle(new Pen(Brushes.Red, 3), DCoordinates);
                }

                //   TMPGR.Dispose();
            } catch {
                DrawCreativePadTo(gr, maxs, state, zoom, X, Y, visibleItems);
            }
        }

        internal void DrawCreativePadToBitmap(Bitmap BMP, enStates vState, decimal zoomf, decimal X, decimal Y, List<BasicPadItem> visibleItems) {

            Graphics gr = Graphics.FromImage(BMP);
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

        private bool MoveSelectedPoints(decimal X, decimal Y) {
            int errorsBefore = _Item.NotPerforming(false);

            foreach (PointM thispoint in _Item.AllPoints) {
                thispoint?.Store();
            }


            foreach (PointM thispoint in _pointsToMoveX) {
                thispoint.SetTo(thispoint.X + X, thispoint.Y);
            }
            foreach (PointM thispoint in _pointsToMoveY) {
                thispoint.SetTo(thispoint.X, thispoint.Y + Y);
            }

            SelectedItems_CaluclatePointsWORelations();


            _Item.PerformAllRelations();


            int errorsAfter = _Item.NotPerforming(false);

            if (errorsAfter > errorsBefore) {
                foreach (PointM thispoint in _Item.AllPoints) {
                    thispoint?.ReStore();
                }
                SelectedItems_CaluclatePointsWORelations();

            } else {
                List<object> done = new List<object>();

                foreach (PointM thispoint in _Item.AllPoints) {
                    if (!done.Contains(thispoint.Parent)) {
                        done.Add(thispoint.Parent);
                        if (thispoint.Parent is BasicPadItem bpi) { bpi.OnChanged(); } // Allen Items mitteilen, dass sich was geändert hat. Zb. Ihr Zeichenbereich
                    }
                }

                SelectedItems_CaluclatePointsWORelations();

            }

            if (errorsAfter < errorsBefore) { ComputeMovingData(); }// Evtl. greifen nun vorher invalide Beziehungen

            Invalidate();
            return errorsAfter == 0;
        }


        /// <summary>
        /// Repariert bei allen selektierten Items (_ItemsToMove) die anderen Punkte, die keine Bezieheung haben und nur "Lose mitgehen"
        /// </summary>
        private void SelectedItems_CaluclatePointsWORelations() {
            foreach (BasicPadItem thisItem in _ItemsToMove) {
                thisItem.CaluclatePointsWORelations();

            }
        }

        private void ComputeMovingData() {
            _Item.ComputeOrders(_pointsSelected);


            _pointsToMoveX.Clear();
            _pointsToMoveY.Clear();
            bool CanMove_X = true;
            bool CanMove_Y = true;

            foreach (PointM thispoint in _pointsSelected) {
                _pointsToMoveX.AddIfNotExists(_Item.ConnectsWith(thispoint, enXY.X, false));
                _pointsToMoveY.AddIfNotExists(_Item.ConnectsWith(thispoint, enXY.Y, false));
            }

            foreach (PointM thispoint in _pointsToMoveX) {
                if (!thispoint.CanMoveX(_Item.AllRelations)) {
                    _pointsToMoveX.Clear();
                    CanMove_X = false;
                    break;
                }
            }

            if (!CanMove_X) {
                _pointsToMoveX.Clear();
            }

            foreach (PointM thispoint in _pointsToMoveY) {
                if (!thispoint.CanMoveY(_Item.AllRelations)) {
                    _pointsToMoveY.Clear();
                    CanMove_Y = false;
                    break;
                }
            }
            if (!CanMove_Y) {
                _pointsToMoveY.Clear();
            }


            foreach (PointM thispoint in _pointsToMoveX) {

                if (thispoint.Parent is BasicPadItem bpi) {

                    _ItemsToMove.AddIfNotExists(bpi);
                }
            }


            foreach (PointM thispoint in _pointsToMoveY) {

                if (thispoint.Parent is BasicPadItem bpi) {

                    _ItemsToMove.AddIfNotExists(bpi);
                }
            }



        }


        private void MoveItems() {
            if (MouseDownPos_1_1 == null) { return; }

            PointM ThisPointSnapsX = null;
            PointM ThisPointSnapsY = null;
            PointM PointSnapedToX = null;
            PointM PointSnapedToY = null;

            _NewAutoRelations.Clear();


            PointM move = new PointM((decimal)(MousePos_1_1.X - MouseDownPos_1_1.X), MousePos_1_1.Y - MouseDownPos_1_1.Y);

            //var MouseMovedToCoords = new PointM(MousePos_1_1);


            if (_pointsToMoveX.Count > 0) {
                move.X = SnapToGrid(true, _pointsToMoveX, move.X);

                if (!_Grid || Math.Abs(_Gridsnap) < 0.001 || move.X == 0M) {
                    if (_AutoRelation != enAutoRelationMode.None) { SnapToPoint(enXY.X, _pointsSelected, move, ref ThisPointSnapsX, ref PointSnapedToX); }
                    if (ThisPointSnapsX != null) { move.X = PointSnapedToX.X - ThisPointSnapsX.X; }
                }
            } else {
                move.X = 0M;
            }

            if (_pointsToMoveY.Count > 0) {
                move.Y = SnapToGrid(false, _pointsToMoveY, move.Y);

                if (!_Grid || Math.Abs(_Gridsnap) < 0.001 || move.Y == 0M) {
                    if (_AutoRelation != enAutoRelationMode.None) { SnapToPoint(enXY.Y, _pointsSelected, move, ref ThisPointSnapsY, ref PointSnapedToY); }
                    if (ThisPointSnapsY != null) { move.Y = PointSnapedToY.Y - ThisPointSnapsY.Y; }
                }
            } else {
                move.Y = 0M;
            }

            if (move.X == 0M && move.Y == 0M && ThisPointSnapsX == null && ThisPointSnapsY == null) { return; }



            if (MoveSelectedPoints(move.X, move.Y)) {
                // Wenn Strongmode true ist es für den Anwender unerklärlich, warum er denn nix macht,obwohl kein Fehler da ist...
                if (_Item.NotPerforming(false) == 0) {
                    AddAllAutoRelations(ThisPointSnapsX, PointSnapedToX, ThisPointSnapsY, PointSnapedToY);
                } else {
                    _NewAutoRelations.Clear();
                }
            }
            MouseDownPos_1_1 = new Point((int)(MouseDownPos_1_1.X + move.X), (int)(MouseDownPos_1_1.Y + move.Y));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="DoX"></param>
        /// <param name="Movep">Die zu verschiebenden (testenden) Punkte. Die Reihenfolge muss nach Wichtigkeit sortiert sein</param>
        /// <param name="MouseMovedTo"></param>
        /// <returns></returns>
        private decimal SnapToGrid(bool DoX, List<PointM> Movep, decimal MouseMovedTo) {

            if (!_Grid || Math.Abs(_Gridsnap) < 0.001) { return MouseMovedTo; }

            if (Movep == null || Movep.Count == 0) { return 0M; }

            PointM MasterPoint = null;
            PointM LowOrderPoint = null;

            foreach (PointM thisPoint in Movep) {
                if (thisPoint == null) // Keine Lust das zu berücksichten. Muss halt stimmen!
                {
                    Develop.DebugPrint(enFehlerArt.Fehler, "Punkt verworfen");
                }
                if (thisPoint.PrimaryGridSnapPoint) {
                    if (MasterPoint == null) { MasterPoint = thisPoint; }
                } else if (LowOrderPoint == null) {
                    LowOrderPoint = thisPoint;
                }
            }

            if (MasterPoint == null) { MasterPoint = LowOrderPoint; }

            decimal Multi = modConverter.mmToPixel((decimal)_Gridsnap, ItemCollectionPad.DPI);
            decimal Value;

            if (DoX) {
                Value = MasterPoint.X + MouseMovedTo;
                Value = (int)(Value / Multi) * Multi;
                // Formel umgestellt
                return Value - MasterPoint.X;
            }

            Value = MasterPoint.Y + MouseMovedTo;
            Value = (int)(Value / Multi) * Multi;
            return Value - MasterPoint.Y;
        }


        private void SnapToPoint(enXY toCheck, List<PointM> movedPoints, PointM move, ref PointM thisPointSnaps, ref PointM snapedToPoint) {
            decimal ShortestDist = 10;
            decimal Nearest = decimal.MaxValue;


            if (!_AutoRelation.HasFlag(enAutoRelationMode.Senkrecht) &&
                !_AutoRelation.HasFlag(enAutoRelationMode.Waagerecht) &&
                !_AutoRelation.HasFlag(enAutoRelationMode.DirektVerbindungen)) {
                return;
            }

            if (_AutoRelation.HasFlag(enAutoRelationMode.DirektVerbindungen)) {
                if (!_AutoRelation.HasFlag(enAutoRelationMode.Senkrecht) && !_AutoRelation.HasFlag(enAutoRelationMode.Waagerecht)) { Nearest = 2; }
            } else {
                if (toCheck.HasFlag(enXY.X) && !_AutoRelation.HasFlag(enAutoRelationMode.Senkrecht)) { return; }
                if (toCheck.HasFlag(enXY.Y) && !_AutoRelation.HasFlag(enAutoRelationMode.Waagerecht)) { return; }

            }


            // Berechne, welcher Punkt snappt
            foreach (PointM thisPoint in movedPoints) {
                SnapToPoint(toCheck, thisPoint, move, ref ShortestDist, ref thisPointSnaps, ref snapedToPoint, ref Nearest);
            }

        }

        private void SnapToPoint(enXY toCheck, PointM pointToTest, PointM move, ref decimal shortestDist, ref PointM thisPointSnaps, ref PointM snapedToPoint, ref decimal nearest) {

            if (pointToTest == null) { return; }

            PointM WillMoveTo = new PointM(pointToTest.X + move.X, pointToTest.Y + move.Y);

            List<PointM> l = PointsOnScreenWichAreNotSelected;

            foreach (PointM ThisPoint in l) {
                if (ThisPoint.CanUsedForAutoRelation && ThisPoint.Parent != pointToTest.Parent) {
                    decimal Distanz = GeometryDF.Länge(WillMoveTo, ThisPoint);


                    decimal SnapDist = 0;
                    if (toCheck.HasFlag(enXY.X)) { SnapDist = Math.Abs(WillMoveTo.X - ThisPoint.X) * _Zoom; }
                    if (toCheck.HasFlag(enXY.Y)) { SnapDist = Math.Abs(WillMoveTo.Y - ThisPoint.Y) * _Zoom; }


                    if (!_AutoRelation.HasFlag(enAutoRelationMode.Senkrecht) && !_AutoRelation.HasFlag(enAutoRelationMode.Waagerecht)) {
                        SnapDist = Math.Max(Math.Abs(WillMoveTo.X - ThisPoint.X), Math.Abs(WillMoveTo.Y - ThisPoint.Y)) * _Zoom;
                        SnapDist = Math.Max(SnapDist, Distanz) * _Zoom;
                    }


                    if (SnapDist < shortestDist || (SnapDist == shortestDist && Distanz < nearest)) {
                        if (!HängenZusammen(toCheck, pointToTest, ThisPoint, null)) {
                            shortestDist = SnapDist;
                            nearest = Distanz;
                            thisPointSnaps = pointToTest;
                            snapedToPoint = ThisPoint;
                        }
                    }
                }
            }
        }




        public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e) {

            BasicPadItem thisItem = null;

            if (e.HotItem is BasicPadItem item) { thisItem = item; }

            bool Done = false;

            if (thisItem != null) {
                switch (e.ClickedComand.ToLower()) {
                    case "#erweitert":
                        Done = true;
                        ShowErweitertMenü(thisItem);
                        break;

                    case "#externebeziehungen":
                        Done = true;
                        thisItem.RelationDeleteExternal();
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

            _Item.PerformAllRelations(); // Da könnte sich ja alles geändert haben...
            Invalidate();

            return Done;
        }

        public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) {
            ContextMenuItemClicked?.Invoke(this, e);
        }

        public List<BasicPadItem> HotItems(System.Windows.Forms.MouseEventArgs e) {

            if (e == null) { return new List<BasicPadItem>(); }


            Point P = new Point((int)((e.X + _shiftX) / _Zoom), (int)((e.Y + _shiftY) / _Zoom));

            List<BasicPadItem> l = new List<BasicPadItem>();

            foreach (BasicPadItem ThisItem in _Item) {
                if (ThisItem != null && ThisItem.Contains(P, _Zoom)) {
                    l.Add(ThisItem);
                }
            }
            return l;
        }


        private void CheckHotItem(System.Windows.Forms.MouseEventArgs e, bool doLastClicked) {
            BasicPadItem OldClicked = LastClickedItem;

            List<BasicPadItem> l = HotItems(e);

            long Mina = long.MaxValue;

            HotItem = null;

            if (e != null) {

                foreach (BasicPadItem ThisItem in l) {
                    long a = (long)Math.Abs(ThisItem.UsedArea().Width) * (long)Math.Abs(ThisItem.UsedArea().Height);
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


                Items.Add("Objektübergreifende Punkt-Beziehungen aufheben", "#ExterneBeziehungen", enImageCode.Kreuz);
                //if (((BasicPadItem)HotItem).Bei_Export_sichtbar)
                //{
                //    Items.Add("#PrintMeNot", "Objekt nicht drucken", QuickImage.Get("Drucker|16||1")));
                //}
                //else
                //{
                //    Items.Add("#PrintMe", "Objekt drucken", enImageCode.Drucker));
                //}


                Items.Add("Objekt duplizieren", "#Duplicate", enImageCode.Kopieren, selectedHotItem is ICloneable);


                Items.AddSeparator();
                Items.Add("In den Vordergrund", "#Vordergrund", enImageCode.InDenVordergrund);
                Items.Add("In den Hintergrund", "#Hintergrund", enImageCode.InDenHintergrund);
                Items.Add("Eine Ebene nach vorne", "#Vorne", enImageCode.EbeneNachVorne);
                Items.Add("Eine Ebene nach hinten", "#Hinten", enImageCode.EbeneNachHinten);
            }


            foreach (PointM Thispoint in _Item.AllPoints) {
                Thispoint?.Store();
            }

        }

        public void OnContextMenuInit(ContextMenuInitEventArgs e) {
            ContextMenuInit?.Invoke(this, e);
        }


        private clsPointRelation AddOneAutoRelation(enRelationType rel, PointM Snap1, PointM SnaP2) {

            // Wegen den beziehungen kann ein Snap-Point sich verschieben, sicherheitshalber
            if (Math.Abs(Snap1.X - SnaP2.X) > 0.01m && Math.Abs(Snap1.Y - SnaP2.Y) > 0.01m) {
                return null;
            }


            switch (rel) {
                case enRelationType.PositionZueinander when !Convert.ToBoolean(_AutoRelation | enAutoRelationMode.DirektVerbindungen):
                case enRelationType.WaagerechtSenkrecht when !Convert.ToBoolean(_AutoRelation | enAutoRelationMode.Waagerecht) && !Convert.ToBoolean(_AutoRelation | enAutoRelationMode.Senkrecht):
                    return null;
            }



            // Prüfen, ob die Beziehung vielleicht schon vorhanden ist

            clsPointRelation r = new clsPointRelation(_Item, null, rel, Snap1, SnaP2);
            ;


            foreach (clsPointRelation thisRelation in _Item.AllRelations) {
                if (thisRelation != null) {
                    if (thisRelation.SinngemäßIdenitisch(r)) { return null; }
                }
            }

            return r;
        }

        private bool HängenZusammen(enXY toCheck, PointM point1, PointM point2, List<clsPointRelation> allreadyChecked) {

            if (allreadyChecked == null) { allreadyChecked = new List<clsPointRelation>(); }

            foreach (clsPointRelation thisRelation in _Item.AllRelations) {
                if (thisRelation != null && !allreadyChecked.Contains(thisRelation) && thisRelation.Connects().HasFlag(toCheck)) {

                    if (thisRelation.Points.Contains(point1)) {
                        allreadyChecked.Add(thisRelation);
                        if (thisRelation.Points.Contains(point2)) { return true; }
                        foreach (PointM thispoint in thisRelation.Points) {
                            if (thispoint != point1) {
                                if (HängenZusammen(toCheck, thispoint, point2, allreadyChecked)) { return true; }
                            }
                        }
                    }
                }
            }

            return false;
        }

        private void AddAllAutoRelations(PointM PMoveX, PointM PSnapToX, PointM PMoveY, PointM PSnapToY) {
            clsPointRelation tmpRl = null;


            if (!Convert.ToBoolean(_AutoRelation | enAutoRelationMode.NurBeziehungenErhalten)) { return; }


            if (PMoveX != null && PMoveY == null && Math.Abs(PMoveX.Y - PSnapToX.Y) < 0.01m) {
                // Ausnahme 1:
                // Es ist zwar die Y-Richtung gesperrt oder der Punkt verschiebt sich mit, aber der X-Punkt ist GENAU der Y-Punkt.
                // Dann lieber eine "Position-Zueinander" als nur die Fehlende X-Beziehung
                PMoveY = PMoveX;
                PSnapToY = PSnapToX;
            }

            if (PMoveY != null && PMoveX == null && Math.Abs(PMoveY.X - PSnapToY.X) < 0.01m) {
                // Ausnahme 2:
                // Gleiche wie 1, nur x und y verdreht
                PMoveX = PMoveY;
                PSnapToX = PSnapToY;
            }




            if (PMoveX != null && PMoveY != null) {
                PointM BP1 = null;
                PointM BP2 = null;
                if (PMoveX == PMoveY) {
                    BP1 = PMoveX; // kann NichtSnapable sein
                    BP2 = _Item.Getbetterpoint(Convert.ToDouble(PSnapToX.X), Convert.ToDouble(PSnapToY.Y), BP1, true); // muß Snapable sein
                } else {
                    BP1 = _Item.Getbetterpoint(Convert.ToDouble(PSnapToX.X), Convert.ToDouble(PSnapToY.Y), null, true); // muß Snapable sein
                    BP2 = null;
                    if (BP1 != null) // kann auch NichtSnapable sein
                    {
                        BP2 = _Item.Getbetterpoint(Convert.ToDouble(PMoveX.X), Convert.ToDouble(PMoveY.Y), BP1, false);
                    }
                }

                if (BP1 != null && BP2 != null) {
                    tmpRl = AddOneAutoRelation(enRelationType.PositionZueinander, BP1, BP2);
                    if (tmpRl != null) {
                        _NewAutoRelations.Add(tmpRl);
                    }

                    // Auf nothing setzen, es kann nämlich sein, daß AutoRelation die Beziehung verwirft (Doppelt, Fehlerhaft)
                    PMoveX = null;
                    PMoveY = null;
                }
            }




            if (PMoveX != null) {
                tmpRl = AddOneAutoRelation(enRelationType.WaagerechtSenkrecht, PMoveX, PSnapToX);
                if (tmpRl != null) { _NewAutoRelations.Add(tmpRl); }
            }

            if (PMoveY != null) {
                tmpRl = AddOneAutoRelation(enRelationType.WaagerechtSenkrecht, PMoveY, PSnapToY);
                if (tmpRl != null) { _NewAutoRelations.Add(tmpRl); }
            }



        }


        public void Unselect() {
            if (_pointsSelected.Count > 0) { Item.InvalidateOrder(); }

            _pointsSelected.Clear();
            _pointsToMoveX.Clear();
            _pointsToMoveY.Clear();
            _NewAutoRelations.Clear();
            _ItemsToMove.Clear();
            _pointsOnScreenWichAreNotSelected = null;
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


            Bitmap i = _Item.ToBitmap(3);
            if (i == null) { return; }
            e.Graphics.DrawImageInRectAspectRatio(i, 0, 0, e.PageBounds.Width, e.PageBounds.Height);
        }

        private void OnPrintPage(PrintPageEventArgs e) {
            PrintPage?.Invoke(this, e);
        }

        public void ShowPrinterPageSetup() {

            RepairPrinterData();

            PageSetupDialog x = new BlueControls.Forms.PageSetupDialog(DruckerDokument, false);
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

            PrintDocument OriD = new PrintDocument();


            OriD.DefaultPageSettings.Landscape = false;
            OriD.DefaultPageSettings.PaperSize = new PaperSize("Benutzerdefiniert", (int)(_Item.SheetSizeInMM.Width / 25.4 * 100), (int)(_Item.SheetSizeInMM.Height / 25.4 * 100));
            OriD.DefaultPageSettings.Margins.Top = (int)(_Item.RandinMM.Top / 25.4 * 100);
            OriD.DefaultPageSettings.Margins.Bottom = (int)(_Item.RandinMM.Bottom / 25.4 * 100);
            OriD.DefaultPageSettings.Margins.Left = (int)(_Item.RandinMM.Left / 25.4 * 100);
            OriD.DefaultPageSettings.Margins.Right = (int)(_Item.RandinMM.Right / 25.4 * 100);




            using (PageSetupDialog x = new BlueControls.Forms.PageSetupDialog(OriD, true)) {
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

            bool done = false;
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

        public void SaveAsBitmap(string Title, string OptionalFileName) {

            if (!string.IsNullOrEmpty(OptionalFileName)) {
                _Item.SaveAsBitmap(OptionalFileName);
                return;
            }

            Title = Title.RemoveChars(Constants.Char_DateiSonderZeichen);

            PicsSave.FileName = Title + ".png";

            PicsSave.ShowDialog();
        }


        private void PicsSave_FileOk(object sender, CancelEventArgs e) {
            if (e.Cancel) { return; }
            _Item.SaveAsBitmap(PicsSave.FileName);
        }


        private void CheckGrid() {
            GridPadItem Found = null;

            Invalidate();

            foreach (BasicPadItem ThisItem in _Item) {
                if (ThisItem is GridPadItem gpi) {
                    Found = gpi;
                    break;
                }
            }


            if (Found == null) {
                if (!_Grid) { return; }
                Found = new GridPadItem(_Item, PadStyles.Style_Standard, new Point(0, 0));
                _Item.Add(Found);

            } else {
                if (!_Grid) {
                    _Item.Remove(Found);
                    return;
                }

            }


            Found.InDenHintergrund();


            Found.GridShow = (decimal)_GridShow;

            if (_Gridsnap < 0.0001F) { _Gridsnap = 0; }
        }




        public void ShowErweitertMenü(BasicPadItem Item) {
            List<FlexiControl> l = Item.GetStyleOptions();

            if (l.Count == 0) {
                MessageBox.Show("Objekt hat keine<br>Bearbeitungsmöglichkeiten.", enImageCode.Information);
                return;
            }

            EditBoxFlexiControl.Show(l);
        }


        private void DruckerDokument_BeginPrint(object sender, PrintEventArgs e) {
            OnBeginnPrint(e);
        }

        private void DruckerDokument_EndPrint(object sender, PrintEventArgs e) {
            OnEndPrint(e);
        }

        private void OnClickedItemChanged() {
            ClickedItemChanged?.Invoke(this, System.EventArgs.Empty);
        }

        private void OnBeginnPrint(PrintEventArgs e) {
            BeginnPrint?.Invoke(this, e);
        }

        private void OnEndPrint(PrintEventArgs e) {
            EndPrint?.Invoke(this, e);
        }

        public override string QuickInfoText {
            get { return _LastQuickInfo; }

        }


        protected override void ZoomOrShiftChanged() {
            _pointsOnScreenWichAreNotSelected = null;
            base.ZoomOrShiftChanged();
        }
        public List<PointM> PointsOnScreenWichAreNotSelected {
            get {
                if (_pointsOnScreenWichAreNotSelected != null) { return _pointsOnScreenWichAreNotSelected; }

                Rectangle dr = AvailablePaintArea();
                _pointsOnScreenWichAreNotSelected = new List<PointM>();

                foreach (PointM thisPoint in _Item.AllPoints) {
                    if (thisPoint != null && !_pointsSelected.Contains(thisPoint) && thisPoint.IsOnScreen(_Zoom, _shiftX, _shiftY, dr)) {
                        _pointsOnScreenWichAreNotSelected.AddIfNotExists(thisPoint);
                    }
                }
                return _pointsOnScreenWichAreNotSelected;
            }
        }
    }
}