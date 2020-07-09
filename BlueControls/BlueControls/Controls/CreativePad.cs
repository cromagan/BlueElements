#region BlueElements - a collection of useful tools, database and controls
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

namespace BlueControls.Controls
{

    [Designer(typeof(BasicDesigner))]
    [DefaultEvent("Click")]
    public sealed partial class CreativePad : ZoomPad, IContextMenu
    {

        #region Constructor

        public CreativePad(ItemCollectionPad itemCollectionPad) : base()
        {
            // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
            InitializeComponent();


            // Initialisierungen nach dem Aufruf InitializeComponent() hinzufügen

            Item = itemCollectionPad;


            _NewAutoRelations.Clear();


            Sel_P = new ListExt<PointDF>();
            Sel_P.ItemAdded += Sel_P_ItemAdded;
            Sel_P.ItemRemoved += Sel_P_ItemRemoved;

            Move_X.Clear();
            Move_Y.Clear();

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


        /// <summary>
        /// Die Punkte, die zum Schieben markiert sind.
        /// </summary>
        private readonly ListExt<PointDF> Sel_P;
        /// <summary>
        /// Diese Punkte bewegen sich in der X-Richtung mit
        /// </summary>
        private readonly List<PointDF> Move_X = new List<PointDF>();
        /// <summary>
        /// Diese Punkte bewegen sich in der Y-Richtung mit
        /// </summary>
        private readonly List<PointDF> Move_Y = new List<PointDF>();



        private bool _Grid;
        private float _GridShow = 10;
        private float _Gridsnap = 1;

        public BasicPadItem HotItem { get; private set; } = null;



        private bool RepairPrinterData_Prepaired;
        private ItemCollectionPad _Item;


        #region  Events 
        public event EventHandler<ContextMenuInitEventArgs> ContextMenuInit;
        public event EventHandler<ContextMenuItemClickedEventArgs> ContextMenuItemClicked;

        public event PrintPageEventHandler PrintPage;

        public event PrintEventHandler BeginnPrint;
        public event PrintEventHandler EndPrint;


        public event EventHandler HotItemChanged;
        #endregion


        #region  Properties 

        [DefaultValue(false)]
        public bool ShowInPrintMode
        {
            get
            {
                return _ShowInPrintMode;
            }
            set
            {

                if (_ShowInPrintMode == value) { return; }

                _ShowInPrintMode = value;
                Invalidate();
            }
        }



        [DefaultValue(false)]
        public bool Grid
        {
            get
            {
                return _Grid;
            }
            set
            {

                if (_Grid == value) { return; }

                _Grid = value;
                CheckGrid();
            }
        }

        [DefaultValue(10.0)]
        public float GridShow
        {
            get
            {
                return _GridShow;
            }
            set
            {

                if (_GridShow == value) { return; }

                _GridShow = value;
                CheckGrid();
            }
        }

        [DefaultValue(10.0)]
        public float GridSnap
        {
            get
            {
                return _Gridsnap;
            }
            set
            {

                if (_Gridsnap == value) { return; }

                _Gridsnap = value;
                CheckGrid();
            }
        }

        [DefaultValue(enAutoRelationMode.Alle_Erhalten)]
        public enAutoRelationMode AutoRelation
        {
            get
            {
                return _AutoRelation;
            }
            set
            {
                _AutoRelation = value;

                if (_Item != null)
                {
                    foreach (var thisItem in _Item)
                    {
                        if (thisItem is ChildPadItem cpi)
                        {
                            if (cpi.PadInternal != null)
                            {
                                cpi.PadInternal.AutoRelation = value;
                            }
                        }
                    }
                }
            }
        }

        #endregion

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ItemCollectionPad Item
        {
            get => _Item;
            set
            {

                if (_Item == value) { return; }


                if (_Item != null)
                {
                    _Item.ItemRemoved -= _Item_ItemRemoved;
                    _Item.DoInvalidate -= Item_DoInvalidate;
                }

                _Item = value;

                if (_Item != null)
                {
                    _Item.ItemRemoved += _Item_ItemRemoved;
                    _Item.DoInvalidate += Item_DoInvalidate;
                    _Item.OnDoInvalidate();
                }
            }
        }





        private void Sel_P_ItemAdded(object sender, BlueBasics.EventArgs.ListEventArgs e)
        {
            _Item.InvalidateOrder();
        }

        private void Sel_P_ItemRemoved(object sender, System.EventArgs e)
        {
            _Item.InvalidateOrder();
        }

        private void Item_DoInvalidate(object sender, System.EventArgs e)
        {
            Invalidate();
        }











        protected override RectangleDF MaxBounds()
        {
            if (_Item == null) { return new RectangleDF(0, 0, 0, 0); }

            return _Item.MaxBounds(null);
        }
        protected override bool IsInputKey(System.Windows.Forms.Keys keyData)
        {
            // Ganz wichtig diese Routine!
            // Wenn diese NICHT ist, geht der Fokus weg, sobald der cursor gedrückt wird.
            // http://technet.microsoft.com/de-de/subscriptions/control.isinputkey%28v=vs.100%29

            switch (keyData)
            {
                case System.Windows.Forms.Keys.Up:
                case System.Windows.Forms.Keys.Down:
                case System.Windows.Forms.Keys.Left:
                case System.Windows.Forms.Keys.Right:
                    return true;
            }

            return false;

        }

        protected override void OnKeyUp(System.Windows.Forms.KeyEventArgs e) => DoKeyUp(e, true); // Kann nicht public gemacht werden, deswegen Umleitung
        public void DoKeyUp(System.Windows.Forms.KeyEventArgs e, bool hasbase)
        {

            // Ganz seltsam: Wird BAse.OnKeyUp IMMER ausgelöst, passiert folgendes:
            // Wird ein Objekt gelöscht, wird anschließend das OnKeyUp Ereignis nicht mehr ausgelöst.

            if (hasbase) { base.OnKeyUp(e); }


            if (_GivesMouseComandsTo != null)
            {
                if (_GivesMouseComandsTo.KeyUp(this, e, _Zoom, _MoveX, _MoveY)) { return; }
            }


            var Multi = modConverter.mmToPixel((decimal)_Gridsnap, ItemCollectionPad.DPI);


            switch (e.KeyCode)
            {
                case System.Windows.Forms.Keys.Delete:
                case System.Windows.Forms.Keys.Back:

                    var Del = new List<BasicPadItem>();
                    foreach (var ThisPoint in Sel_P)
                    {
                        if (ThisPoint?.Parent is BasicPadItem bpi)
                        {
                            Del.Add(bpi);
                        }

                    }

                    Unselect();


                    do
                    {
                        if (Del.Count == 0) { break; }
                        _Item.Remove(Del[0]);
                        Del[0] = null;
                        Del.RemoveAt(0);
                    } while (true);

                    ComputeMovingData();
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
        internal void DoMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseDown(e);


            CheckHotItem(e);

            if (HotItem is IMouseAndKeyHandle ho2)
            {
                if (ho2.MouseDown(this, e, _Zoom, _MoveX, _MoveY))
                {
                    _GivesMouseComandsTo = ho2;
                    return;
                }
            }


            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                var p = KoordinatesUnscaled(e);
                foreach (var thisPoint in Sel_P)
                {

                    if (GeometryDF.Länge(thisPoint, new PointDF(p)) < 5m / _Zoom)
                    {

                        if (!thisPoint.CanMove(_Item.AllRelations))
                        {
                            Invalidate();
                            Forms.QuickInfo.Show("Dieser Punkt ist fest definiert<br>und kann nicht verschoben werden.");
                            return;
                        }

                        Unselect();
                        Sel_P.Add(thisPoint);
                        ComputeMovingData();
                        Invalidate();
                        return;
                    }
                }



                if ((ModifierKeys & System.Windows.Forms.Keys.Control) <= 0 && Sel_P.Count > 0)
                {
                    Unselect();

                    if (HotItem == null) { ComputeMovingData(); }
                    Invalidate();

                }

                if (HotItem != null)
                {
                    Sel_P.AddIfNotExists(HotItem.Points);
                    ComputeMovingData();
                    Invalidate();
                }

            }
        }

        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e) => DoMouseMove(e); // Kann nicht public gemacht werden, deswegen Umleitung
        internal void DoMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_GivesMouseComandsTo != null)
            {
                if (e.Button == System.Windows.Forms.MouseButtons.None && HotItem != _GivesMouseComandsTo)
                {
                    _GivesMouseComandsTo = null;
                    Invalidate();
                }
                else
                {
                    if (!_GivesMouseComandsTo.MouseMove(this, e, _Zoom, _MoveX, _MoveY))
                    {
                        _GivesMouseComandsTo = null;
                        Invalidate();
                    }
                    else
                    {
                        return;
                    }
                }
            }
            else
            {
                if (HotItem is IMouseAndKeyHandle Ho2)
                {
                    if (Ho2.MouseMove(this, e, _Zoom, _MoveX, _MoveY))
                    {
                        _GivesMouseComandsTo = Ho2;
                        Invalidate();
                        return;
                    }
                }
            }

            if (MouseDownPos_1_1 != null && e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                MoveItems();

            }
        }


        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e) => DoMouseUp(e); // Kann nicht public gemacht werden, deswegen Umleitung
        internal void DoMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseUp(e);


            if (_GivesMouseComandsTo != null)
            {
                if (!_GivesMouseComandsTo.MouseUp(this, e, _Zoom, _MoveX, _MoveY))
                {
                    _GivesMouseComandsTo = null;
                }
                else
                {
                    return;
                }
            }





            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                //   AmRasterAusrichten()

                foreach (var Thispoint in Sel_P)
                {
                    if (Thispoint.Parent is BasicPadItem item)
                    {
                        Sel_P.AddIfNotExists(item.Points);
                        Invalidate();
                        break;
                    }
                }

                if (Convert.ToBoolean(_AutoRelation | enAutoRelationMode.NurBeziehungenErhalten) && _NewAutoRelations.Count > 0)
                {
                    _Item.AllRelations.AddRange(_NewAutoRelations);
                    _Item.PerformAll(0, false);
                }

            }

            _NewAutoRelations.Clear();

            _Item.PerformAll(1, false);

            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {

                FloatingInputBoxListBoxStyle.ContextMenuShow(this, e);

            }
            ComputeMovingData();
            Invalidate(); // Damit auch snap-Punkte wieder gelöscht werden
        }


        internal void DrawCreativePadTo(Graphics gr, Size maxs, enStates vState, decimal zoomf, decimal X, decimal Y, List<BasicPadItem> visibleItems)
        {
            try
            {


                if (_Item.SheetSizeInMM.Width > 0 && _Item.SheetSizeInMM.Height > 0)
                {
                    Skin.Draw_Back(gr, enDesign.Table_And_Pad, vState, DisplayRectangle, this, true);
                    var SSW = Math.Round(modConverter.mmToPixel((decimal)_Item.SheetSizeInMM.Width, ItemCollectionPad.DPI), 1);
                    var SSH = Math.Round(modConverter.mmToPixel((decimal)_Item.SheetSizeInMM.Height, ItemCollectionPad.DPI), 1);
                    var LO = new PointDF(0m, 0m).ZoomAndMove(zoomf, X, Y);
                    var RU = new PointDF(SSW, SSH).ZoomAndMove(zoomf, X, Y);

                    var R = new Rectangle((int)LO.X, (int)LO.Y, (int)(RU.X - LO.X), (int)(RU.Y - LO.Y));
                    gr.FillRectangle(Brushes.White, R);
                    gr.DrawRectangle(PenGray, R);

                    var rtx = (int)(_Item.P_rLO.X * zoomf - X);
                    var rty = (int)(_Item.P_rLO.Y * zoomf - Y);
                    var rtx2 = (int)(_Item.P_rRU.X * zoomf - X);
                    var rty2 = (int)(_Item.P_rRU.Y * zoomf - Y);
                    var Rr = new Rectangle(rtx, rty, rtx2 - rtx, rty2 - rty);
                    if (!_ShowInPrintMode)
                    {
                        gr.DrawRectangle(PenGray, Rr);
                    }

                }
                else
                {
                    gr.Clear(Color.White);
                }

                if (!_Item.Draw(gr, zoomf, X, Y, maxs, _ShowInPrintMode, visibleItems))
                {
                    DrawCreativePadTo(gr, maxs, vState, zoomf, X, Y, visibleItems);
                    return;
                }


                // Erst Beziehungen, weil die die Grauen Punkte zeichnet
                foreach (var ThisRelation in _Item.AllRelations)
                {
                    ThisRelation.Draw(gr, zoomf, X, Y, _Item.AllRelations.IndexOf(ThisRelation));
                }


                if (Debug_ShowPointOrder)
                {

                    // Alle Punkte mit Order anzeigen
                    foreach (var ThisPoint in _Item.AllPoints)
                    {
                        ThisPoint.Draw(gr, zoomf, X, Y, enDesign.Button_EckpunktSchieber_Phantom, enStates.Standard, false);
                    }
                }


                //     If _ItemsEditable Then

                // Dann die selectiereren Punkte

                foreach (var ThisPoint in Sel_P)
                {
                    if (ThisPoint.CanMove(_Item.AllRelations))
                    {
                        ThisPoint.Draw(gr, zoomf, X, Y, enDesign.Button_EckpunktSchieber, enStates.Standard, false);
                    }
                    else
                    {
                        ThisPoint.Draw(gr, zoomf, X, Y, enDesign.Button_EckpunktSchieber_Phantom, enStates.Standard, false);
                    }
                }


                foreach (var ThisRelation in _NewAutoRelations)
                {


                    var P1 = ThisRelation.Points[0].ZoomAndMove(zoomf, X, Y);
                    var P2 = ThisRelation.Points[1].ZoomAndMove(zoomf, X, Y);

                    if (ThisRelation.RelationType == enRelationType.WaagerechtSenkrecht)
                    {
                        gr.DrawEllipse(new Pen(Color.Green, 3), P1.X - 4, P1.Y - 3, 7, 7);
                        gr.DrawEllipse(new Pen(Color.Green, 3), P2.X - 4, P2.Y - 3, 7, 7);
                        gr.DrawLine(new Pen(Color.Green, 1), P1, P2);
                    }
                    else
                    {
                        gr.DrawEllipse(new Pen(Color.Green, 3), P1.X - 4, P1.Y - 4, 7, 7);
                        gr.DrawEllipse(new Pen(Color.Green, 3), P1.X - 8, P1.Y - 8, 15, 15);
                    }
                }



                if (_GivesMouseComandsTo is BasicPadItem PA)
                {
                    var DCoordinates = PA.UsedArea().ZoomAndMoveRect(zoomf, X, Y);

                    gr.DrawRectangle(new Pen(Brushes.Red, 3), DCoordinates);
                }

                //   TMPGR.Dispose();
            }
            catch
            {
                DrawCreativePadTo(gr, maxs, vState, zoomf, X, Y, visibleItems);
            }
        }




        internal void DrawCreativePadToBitmap(Bitmap BMP, enStates vState, decimal zoomf, decimal X, decimal Y, List<BasicPadItem> visibleItems)
        {

            var gr = Graphics.FromImage(BMP);
            DrawCreativePadTo(gr, BMP.Size, vState, zoomf, X, Y, visibleItems);
            gr.Dispose();
        }



        protected override void DrawControl(Graphics gr, enStates state)
        {

            DrawCreativePadTo(gr, Size, state, _Zoom, _MoveX, _MoveY, null);

            Skin.Draw_Border(gr, enDesign.Table_And_Pad, state, DisplayRectangle);
        }



        private void _Item_ItemRemoved(object sender, System.EventArgs e)
        {
            CheckHotItem(null);
            Unselect();
            ZoomFit();
            Invalidate();
        }




        private bool MoveSelectedPoints(decimal X, decimal Y)
        {
            var errorsBefore = _Item.NotPerforming(false);

            foreach (var thispoint in _Item.AllPoints)
            {
                thispoint?.Store();
            }


            foreach (var thispoint in Move_X)
            {
                thispoint.SetTo(thispoint.X + X, thispoint.Y);
            }
            foreach (var thispoint in Move_Y)
            {
                thispoint.SetTo(thispoint.X, thispoint.Y + Y);
            }

            CaluclateOtherPointsOf(Sel_P);

            if (errorsBefore == 0)
            {
                _Item.PerformAll(1, true);
            }
            else
            {
                _Item.PerformAll(2, false);
                // Sind eh schon fehler drinne, also schneller abbrechen, um nicht allzusehr verzögen
                // und sicherheitshalber große Änderungen verbieten, um nicht noch mehr kaputt zu machen...
            }

            var errorsAfter = _Item.NotPerforming(false);

            if (errorsAfter > errorsBefore)
            {
                foreach (var thispoint in _Item.AllPoints)
                {
                    thispoint?.ReStore();
                }
                CaluclateOtherPointsOf(Sel_P);

            }
            else if (errorsAfter < errorsBefore)
            {
                ComputeMovingData(); // Evtl. greifen nun vorher invalide Beziehungen.
            }



            Invalidate();
            return Convert.ToBoolean(errorsAfter == 0);
        }

        private void CaluclateOtherPointsOf(ListExt<PointDF> sel_P)
        {
            var x = new List<BasicPadItem>();


            foreach (var thispoint in Sel_P)
            {
                if (thispoint.Parent is BasicPadItem BP)
                {
                    if (!x.Contains(BP))
                    {
                        BP.CaluclatePointsWORelations();
                        x.Add(BP);
                    }

                }
            }
        }

        private void ComputeMovingData()
        {
            _Item.ComputeOrders(Sel_P);

            Move_X.Clear();
            Move_Y.Clear();
            var CanMove_X = true;
            var CanMove_Y = true;

            foreach (var thispoint in Sel_P)
            {
                Move_X.AddIfNotExists(_Item.ConnectsWith(thispoint, true, false));
                Move_Y.AddIfNotExists(_Item.ConnectsWith(thispoint, false, false));
            }

            foreach (var thispoint in Move_X)
            {
                if (!thispoint.CanMoveX(_Item.AllRelations))
                {
                    CanMove_X = false;
                    break;
                }
            }

            if (!CanMove_X)
            {
                Move_X.Clear();
            }

            foreach (var thispoint in Move_Y)
            {
                if (!thispoint.CanMoveY(_Item.AllRelations))
                {
                    CanMove_Y = false;
                    break;
                }
            }
            if (!CanMove_Y)
            {
                Move_Y.Clear();
            }



        }







        private void MoveItems()
        {
            if (MouseDownPos_1_1 == null) { return; }

            PointDF PMoveX = null;
            PointDF PMoveY = null;
            PointDF PSnapToX = null;
            PointDF PSnapToY = null;

            _NewAutoRelations.Clear();

            var MoveX = (decimal)(MousePos_1_1.X - MouseDownPos_1_1.X);
            var MoveY = (decimal)(MousePos_1_1.Y - MouseDownPos_1_1.Y);

            var MouseMovedTo = new PointDF(MousePos_1_1);


            if (Move_X.Count > 0)
            {
                MoveX = SnapToGrid(true, Move_X, MoveX);

                if (_Grid == false || Math.Abs(_Gridsnap) < 0.001 || MoveX == 0M)
                {
                    if (_AutoRelation != enAutoRelationMode.None) { SnapToPoint(true, Sel_P, MouseMovedTo, ref PMoveX, ref PSnapToX); }
                    if (PMoveX != null) { MoveX = PSnapToX.X - PMoveX.X; }
                }
            }
            else
            {
                MoveX = 0M;
            }

            if (Move_Y.Count > 0)
            {
                MoveY = SnapToGrid(false, Move_Y, MoveY);

                if (_Grid == false || Math.Abs(_Gridsnap) < 0.001 || MoveY == 0M)
                {
                    if (_AutoRelation != enAutoRelationMode.None) { SnapToPoint(false, Sel_P, MouseMovedTo, ref PMoveY, ref PSnapToY); }
                    if (PMoveY != null) { MoveY = PSnapToY.Y - PMoveY.Y; }
                }
            }
            else
            {
                MoveY = 0M;
            }

            if (MoveX == 0M && MoveY == 0M && PMoveX == null && PMoveY == null) { return; }

            if (MoveSelectedPoints(MoveX, MoveY))
            {
                // Wenn Strongmode true ist es für den Anwender unerklärlich, warum er denn nix macht,obwohl kein Fehler da ist...
                if (_Item.NotPerforming(false) == 0)
                {
                    AddAllAutoRelations(PMoveX, PSnapToX, PMoveY, PSnapToY);
                }
                else
                {
                    _NewAutoRelations.Clear();
                }
            }

            MouseDownPos_1_1 = new Point((int)(MouseDownPos_1_1.X + MoveX), (int)(MouseDownPos_1_1.Y + MoveY));
        }

        private decimal SnapToGrid(bool DoX, List<PointDF> Movep, decimal MouseMovedTo)
        {

            if (!_Grid || Math.Abs(_Gridsnap) < 0.001) { return MouseMovedTo; }

            if (Movep == null || Movep.Count == 0) { return 0M; }

            PointDF MasterPoint = null;
            var LowOrderPoint = Movep[0];

            foreach (var thisPoint in Movep)
            {
                if (thisPoint == null) // Keine Lust das zu berücksichte. Muss halt stimmen!
                {
                    Develop.DebugPrint(enFehlerArt.Fehler, "Punkt verworfen");
                }
                if (thisPoint.PrimaryGridSnapPoint)
                {
                    if (MasterPoint == null || thisPoint.Order < MasterPoint.Order) { MasterPoint = thisPoint; }
                }

                if (thisPoint.Order < LowOrderPoint.Order) { LowOrderPoint = thisPoint; }

            }

            if (MasterPoint == null) { MasterPoint = LowOrderPoint; }

            var Multi = modConverter.mmToPixel((decimal)_Gridsnap, ItemCollectionPad.DPI);
            var Value = 0M;

            if (DoX)
            {
                Value = MasterPoint.X + MouseMovedTo;
                Value = (int)(Value / Multi) * Multi;
                // Formel umgestellt
                return Value - MasterPoint.X;
            }

            Value = MasterPoint.Y + MouseMovedTo;
            Value = (int)(Value / Multi) * Multi;
            return Value - MasterPoint.Y;
        }


        private void SnapToPoint(bool DoX, List<PointDF> Movep, PointDF MouseMovedTo, ref PointDF PMove, ref PointDF PSnapTo)
        {
            decimal ShortestDist = 10;
            var Nearest = decimal.MaxValue;


            if (!Convert.ToBoolean(_AutoRelation & enAutoRelationMode.Senkrecht) && !Convert.ToBoolean(_AutoRelation & enAutoRelationMode.Waagerecht) && !Convert.ToBoolean(_AutoRelation & enAutoRelationMode.DirektVerbindungen))
            {
                return;
            }

            if (!Convert.ToBoolean(_AutoRelation & enAutoRelationMode.DirektVerbindungen))
            {
                if (DoX && !Convert.ToBoolean(_AutoRelation & enAutoRelationMode.Senkrecht)) { return; }
                if (!DoX && !Convert.ToBoolean(_AutoRelation & enAutoRelationMode.Waagerecht)) { return; }

            }
            else
            {
                if (!Convert.ToBoolean(_AutoRelation & enAutoRelationMode.Senkrecht) && !Convert.ToBoolean(_AutoRelation & enAutoRelationMode.Waagerecht))
                {
                    Nearest = 2;
                }
            }



            // Berechne, welcher Punkt snappt
            foreach (var ThisPoint in Movep)
            {
                var tempVar = ThisPoint;
                SnapToPoint(DoX, ref tempVar, MouseMovedTo, ref ShortestDist, ref PMove, ref PSnapTo, ref Nearest);
            }

        }

        private void SnapToPoint(bool DoX, ref PointDF PointToTest, PointDF MouseMovedTo, ref decimal ShortestDist, ref PointDF PMove, ref PointDF PSnapTo, ref decimal Nearest)
        {

            if (PointToTest == null) { return; }

            var dr = AviablePaintArea();


            var WillMoveTo = new PointDF(PointToTest.X + MouseMovedTo.X - MouseDownPos_1_1.X, PointToTest.Y + MouseMovedTo.Y - MouseDownPos_1_1.Y);


            foreach (var ThisPoint in _Item.AllPoints)
            {
                if (ThisPoint != null && ThisPoint.CanUsedForAutoRelation && ThisPoint.IsOnScreen(_Zoom, _MoveX, _MoveY, dr))
                {

                    if (PointToTest.Parent == null || ThisPoint.Parent != PointToTest.Parent)
                    {
                        var Distanz = GeometryDF.Länge(WillMoveTo, ThisPoint);

                        if (Distanz < 1000 * _Zoom)
                        {
                            decimal SnapDist = 0;
                            if (DoX)
                            {
                                SnapDist = Math.Abs(WillMoveTo.X - ThisPoint.X) * _Zoom;
                            }
                            else
                            {
                                SnapDist = Math.Abs(WillMoveTo.Y - ThisPoint.Y) * _Zoom;
                            }


                            if (!Convert.ToBoolean(_AutoRelation & enAutoRelationMode.Senkrecht) && !Convert.ToBoolean(_AutoRelation & enAutoRelationMode.Waagerecht))
                            {

                                SnapDist = Math.Max(Math.Abs(WillMoveTo.X - ThisPoint.X), Math.Abs(WillMoveTo.Y - ThisPoint.Y)) * _Zoom;
                                SnapDist = Math.Max(SnapDist, Distanz) * _Zoom;
                            }


                            if (SnapDist < ShortestDist || SnapDist == ShortestDist && Distanz < Nearest)
                            {
                                if (!HängenZusammen(DoX, PointToTest, ThisPoint, null))
                                {
                                    ShortestDist = SnapDist;
                                    Nearest = Distanz;
                                    PMove = PointToTest;
                                    PSnapTo = ThisPoint;
                                }
                            }
                        }
                    }
                }
            }

        }










        public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e)
        {

            BasicPadItem thisItem = null;

            if (e.HotItem is BasicPadItem item) { thisItem = item; }

            var Done = false;

            if (thisItem != null)
            {
                switch (e.ClickedComand.ToLower())
                {
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

            //if (!Done) { Done = thisItem.ContextMenuItemClicked(new ContextMenuItemClickedEventArgs(e.ClickedComand, null, e.Tags)); }





            _Item.PerformAll(0, false); // Da könnte sich ja alles geändert haben...
            Invalidate();

            return Done;

        }

        public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e)
        {
            ContextMenuItemClicked?.Invoke(this, e);
        }

        public List<BasicPadItem> HotItems(System.Windows.Forms.MouseEventArgs e)
        {

            if (e == null) { return new List<BasicPadItem>(); }


            var P = new Point((int)((e.X + _MoveX) / _Zoom), (int)((e.Y + _MoveY) / _Zoom));

            var l = new List<BasicPadItem>();

            foreach (var ThisItem in _Item)
            {
                if (ThisItem != null && ThisItem.Contains(P, _Zoom))
                {
                    l.Add(ThisItem);
                }
            }
            return l;
        }


        private void CheckHotItem(System.Windows.Forms.MouseEventArgs e)
        {
            var OldHot = HotItem;

            var l = HotItems(e);

            var Mina = long.MaxValue;

            HotItem = null;

            if (e != null)
            {

                foreach (var ThisItem in l)
                {
                    var a = (long)Math.Abs(ThisItem.UsedArea().Width) * (long)Math.Abs(ThisItem.UsedArea().Height);
                    if (a <= Mina)
                    {
                        // Gleich deswegen, dass neuere, IDENTISCHE Items dass oberste gewählt wird.
                        Mina = a;
                        HotItem = ThisItem;
                    }
                }
            }


            if (HotItem != OldHot) { OnHotItemChanged(); }

        }


        public void GetContextMenuItems(System.Windows.Forms.MouseEventArgs e, ItemCollectionList Items, out object selectedHotItem, List<string> Tags, ref bool Cancel, ref bool Translate)
        {

            CheckHotItem(e);
            selectedHotItem = HotItem;


            if (selectedHotItem != null)
            {
                Items.Add(new TextListItem("Allgemeine Element-Aktionen", true));
                Items.Add(new TextListItem("#Erweitert", "Objekt bearbeiten", enImageCode.Stift));
                Items.Add(new LineListItem());


                Items.Add(new TextListItem("#ExterneBeziehungen", "Objektübergreifende Punkt-Beziehungen aufheben", enImageCode.Kreuz));
                //if (((BasicPadItem)HotItem).Bei_Export_sichtbar)
                //{
                //    Items.Add(new TextListItem("#PrintMeNot", "Objekt nicht drucken", QuickImage.Get("Drucker|16||1")));
                //}
                //else
                //{
                //    Items.Add(new TextListItem("#PrintMe", "Objekt drucken", enImageCode.Drucker));
                //}


                Items.Add(new TextListItem("#Duplicate", "Objekt duplizieren", enImageCode.Kopieren, selectedHotItem is ICloneable));


                Items.Add(new LineListItem());
                Items.Add(new TextListItem("#Vordergrund", "In den Vordergrund", enImageCode.InDenVordergrund));
                Items.Add(new TextListItem("#Hintergrund", "In den Hintergrund", enImageCode.InDenHintergrund));
                Items.Add(new TextListItem("#Vorne", "Eine Ebene nach vorne", enImageCode.EbeneNachVorne));
                Items.Add(new TextListItem("#Hinten", "Eine Ebene nach hinten", enImageCode.EbeneNachHinten));
            }


            foreach (var Thispoint in _Item.AllPoints)
            {
                Thispoint?.Store();
            }

        }

        public void OnContextMenuInit(ContextMenuInitEventArgs e)
        {
            ContextMenuInit?.Invoke(this, e);
        }


        private clsPointRelation AddOneAutoRelation(enRelationType rel, PointDF Snap1, PointDF SnaP2)
        {

            // Wegen den beziehungen kann ein Snap-Point sich verschieben, sicherheitshalber
            if (Math.Abs(Snap1.X - SnaP2.X) > 0.01m && Math.Abs(Snap1.Y - SnaP2.Y) > 0.01m)
            {
                return null;
            }


            switch (rel)
            {
                case enRelationType.PositionZueinander when !Convert.ToBoolean(_AutoRelation | enAutoRelationMode.DirektVerbindungen):
                case enRelationType.WaagerechtSenkrecht when !Convert.ToBoolean(_AutoRelation | enAutoRelationMode.Waagerecht) && !Convert.ToBoolean(_AutoRelation | enAutoRelationMode.Senkrecht):
                    return null;
            }


            var r = new clsPointRelation(_Item, null, rel, Snap1, SnaP2);


            foreach (var thisRelation in _Item.AllRelations)
            {
                if (thisRelation != null)
                {
                    if (thisRelation.RelationType == rel && thisRelation.Richtmaß() == r.Richtmaß())
                    {
                        if (thisRelation.Points[0] == Snap1 && thisRelation.Points[1] == SnaP2) { return null; }
                        if (thisRelation.Points[0] == SnaP2 && thisRelation.Points[1] == Snap1) { return null; }
                    }
                }
            }


            return r;


        }

        private bool HängenZusammen(bool CheckX, PointDF Point1, PointDF Point2, List<clsPointRelation> l)
        {

            if (l == null) { l = new List<clsPointRelation>(); }

            foreach (var thisRelation in _Item.AllRelations)
            {
                if (thisRelation != null && !l.Contains(thisRelation) && thisRelation.Connects(CheckX))
                {

                    if (thisRelation.Points.Contains(Point1))
                    {
                        l.Add(thisRelation);
                        if (thisRelation.Points.Contains(Point2)) { return true; }
                        foreach (var thispoint in thisRelation.Points)
                        {
                            if (thispoint != Point1)
                            {
                                if (HängenZusammen(CheckX, thispoint, Point2, l)) { return true; }
                            }
                        }
                    }
                }
            }

            return false;
        }

        private void AddAllAutoRelations(PointDF PMoveX, PointDF PSnapToX, PointDF PMoveY, PointDF PSnapToY)
        {
            clsPointRelation tmpRl = null;


            if (!Convert.ToBoolean(_AutoRelation | enAutoRelationMode.NurBeziehungenErhalten)) { return; }


            if (PMoveX != null && PMoveY == null && Math.Abs(PMoveX.Y - PSnapToX.Y) < 0.01m)
            {
                // Ausnahme 1:
                // Es ist zwar die Y-Richtung gesperrt oder der Punkt verschiebt sich mit, aber der X-Punkt ist GENAU der Y-Punkt.
                // Dann lieber eine "Position-Zueinander" als nur die Fehlende X-Beziehung
                PMoveY = PMoveX;
                PSnapToY = PSnapToX;
            }

            if (PMoveY != null && PMoveX == null && Math.Abs(PMoveY.X - PSnapToY.X) < 0.01m)
            {
                // Ausnahme 2:
                // Gleiche wie 1, nur x und y verdreht
                PMoveX = PMoveY;
                PSnapToX = PSnapToY;
            }




            if (PMoveX != null && PMoveY != null)
            {
                PointDF BP1 = null;
                PointDF BP2 = null;
                if (PMoveX == PMoveY)
                {
                    BP1 = PMoveX; // kann NichtSnapable sein
                    BP2 = _Item.Getbetterpoint(Convert.ToDouble(PSnapToX.X), Convert.ToDouble(PSnapToY.Y), BP1, true); // muß Snapable sein
                }
                else
                {
                    BP1 = _Item.Getbetterpoint(Convert.ToDouble(PSnapToX.X), Convert.ToDouble(PSnapToY.Y), null, true); // muß Snapable sein
                    BP2 = null;
                    if (BP1 != null) // kann auch NichtSnapable sein
                    {
                        BP2 = _Item.Getbetterpoint(Convert.ToDouble(PMoveX.X), Convert.ToDouble(PMoveY.Y), BP1, false);
                    }
                }

                if (BP1 != null && BP2 != null)
                {
                    tmpRl = AddOneAutoRelation(enRelationType.PositionZueinander, BP1, BP2);
                    if (tmpRl != null)
                    {
                        _NewAutoRelations.Add(tmpRl);
                    }

                    // Auf nothing setzen, es kann nämlich sein, daß AutoRelation die Beziehung verwirft (Doppelt, Fehlerhaft)
                    PMoveX = null;
                    PMoveY = null;
                }
            }




            if (PMoveX != null)
            {
                tmpRl = AddOneAutoRelation(enRelationType.WaagerechtSenkrecht, PMoveX, PSnapToX);
                if (tmpRl != null) { _NewAutoRelations.Add(tmpRl); }
            }

            if (PMoveY != null)
            {
                tmpRl = AddOneAutoRelation(enRelationType.WaagerechtSenkrecht, PMoveY, PSnapToY);
                if (tmpRl != null) { _NewAutoRelations.Add(tmpRl); }
            }



        }

        //public void Relation_Add(enRelationType enRelationType, PointDF Point1, PointDF Point2)
        //{
        //    Item.AllRelations.Add(new clsPointRelation(enRelationType, Point1, Point2));
        //}














        public void Unselect()
        {
            Sel_P.Clear();
            Move_X.Clear();
            Move_Y.Clear();
            _NewAutoRelations.Clear();
        }





        public void Print()
        {
            DruckerDokument.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
            if (PrintDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                PrintDialog1.Document.Print();
            }
            RepairPrinterData();
        }

        public void ShowPrintPreview()
        {
            DruckerDokument.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
            PrintPreviewDialog1.ShowDialog();
            RepairPrinterData();
        }


        private void DruckerDokument_PrintPage(object sender, PrintPageEventArgs e)
        {
            e.HasMorePages = false;

            OnPrintPage(e);


            var i = _Item.ToBitmap(3);
            if (i == null) { return; }
            e.Graphics.DrawImageInRectAspectRatio(i, 0, 0, e.PageBounds.Width, e.PageBounds.Height);
        }

        private void OnPrintPage(PrintPageEventArgs e)
        {
            PrintPage?.Invoke(this, e);
        }

        public void ShowPrinterPageSetup()
        {

            RepairPrinterData();

            var x = new BlueControls.Forms.PageSetupDialog(DruckerDokument, false);
            x.ShowDialog();
            x.Dispose();

        }

        public void CopyPrinterSettingsToWorkingArea()
        {


            if (DruckerDokument.DefaultPageSettings.Landscape)
            {
                _Item.SheetSizeInMM = new SizeF((int)(DruckerDokument.DefaultPageSettings.PaperSize.Height * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.PaperSize.Width * 25.4 / 100));
                _Item.RandinMM = new System.Windows.Forms.Padding((int)(DruckerDokument.DefaultPageSettings.Margins.Left * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.Margins.Top * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.Margins.Right * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.Margins.Bottom * 25.4 / 100));


            }
            else
            {
                // Hochformat
                _Item.SheetSizeInMM = new SizeF((int)(DruckerDokument.DefaultPageSettings.PaperSize.Width * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.PaperSize.Height * 25.4 / 100));
                _Item.RandinMM = new System.Windows.Forms.Padding((int)(DruckerDokument.DefaultPageSettings.Margins.Left * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.Margins.Top * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.Margins.Right * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.Margins.Bottom * 25.4 / 100));
            }

        }


        public void ShowWorkingAreaSetup()
        {

            var OriD = new PrintDocument();


            OriD.DefaultPageSettings.Landscape = false;
            OriD.DefaultPageSettings.PaperSize = new PaperSize("Benutzerdefiniert", (int)(_Item.SheetSizeInMM.Width / 25.4 * 100), (int)(_Item.SheetSizeInMM.Height / 25.4 * 100));
            OriD.DefaultPageSettings.Margins.Top = (int)(_Item.RandinMM.Top / 25.4 * 100);
            OriD.DefaultPageSettings.Margins.Bottom = (int)(_Item.RandinMM.Bottom / 25.4 * 100);
            OriD.DefaultPageSettings.Margins.Left = (int)(_Item.RandinMM.Left / 25.4 * 100);
            OriD.DefaultPageSettings.Margins.Right = (int)(_Item.RandinMM.Right / 25.4 * 100);




            using (var x = new BlueControls.Forms.PageSetupDialog(OriD, true))
            {
                x.ShowDialog();
                if (x.Canceled()) { return; }
            }


            _Item.SheetSizeInMM = new SizeF((int)(OriD.DefaultPageSettings.PaperSize.Width * 25.4 / 100), (int)(OriD.DefaultPageSettings.PaperSize.Height * 25.4 / 100));
            _Item.RandinMM = new System.Windows.Forms.Padding((int)(OriD.DefaultPageSettings.Margins.Left * 25.4 / 100), (int)(OriD.DefaultPageSettings.Margins.Top * 25.4 / 100), (int)(OriD.DefaultPageSettings.Margins.Right * 25.4 / 100), (int)(OriD.DefaultPageSettings.Margins.Bottom * 25.4 / 100));

        }



        private void RepairPrinterData()
        {

            if (RepairPrinterData_Prepaired) { return; }

            RepairPrinterData_Prepaired = true;

            DruckerDokument.DocumentName = _Item.Caption;

            var done = false;
            foreach (PaperSize ps in DruckerDokument.PrinterSettings.PaperSizes)
            {
                if (ps.Width == (int)(_Item.SheetSizeInMM.Width / 25.4 * 100) && ps.Height == (int)(_Item.SheetSizeInMM.Height / 25.4 * 100))
                {
                    done = true;
                    DruckerDokument.DefaultPageSettings.PaperSize = ps;
                    break;
                }
            }

            if (!done)
            {
                DruckerDokument.DefaultPageSettings.PaperSize = new PaperSize("Custom", (int)(_Item.SheetSizeInMM.Width / 25.4 * 100), (int)(_Item.SheetSizeInMM.Height / 25.4 * 100));
            }
            DruckerDokument.DefaultPageSettings.PrinterResolution = DruckerDokument.DefaultPageSettings.PrinterSettings.PrinterResolutions[0];
            DruckerDokument.OriginAtMargins = true;
            DruckerDokument.DefaultPageSettings.Margins = new Margins((int)(_Item.RandinMM.Left / 25.4 * 100), (int)(_Item.RandinMM.Right / 25.4 * 100), (int)(_Item.RandinMM.Top / 25.4 * 100), (int)(_Item.RandinMM.Bottom / 25.4 * 100));
        }

        public void SaveAsBitmap(string Title, string OptionalFileName)
        {

            if (!string.IsNullOrEmpty(OptionalFileName))
            {
                _Item.SaveAsBitmap(OptionalFileName);
                return;
            }

            Title = Title.RemoveChars(Constants.Char_DateiSonderZeichen);

            PicsSave.FileName = Title + ".png";

            PicsSave.ShowDialog();
        }


        private void PicsSave_FileOk(object sender, CancelEventArgs e)
        {
            if (e.Cancel) { return; }
            _Item.SaveAsBitmap(PicsSave.FileName);
        }


        public void CheckGrid()
        {
            GridPadItem Found = null;

            Invalidate();

            foreach (var ThisItem in _Item)
            {
                if (ThisItem is GridPadItem gpi)
                {
                    Found = gpi;
                    break;
                }
            }


            if (Found == null)
            {
                if (!_Grid) { return; }
                Found = new GridPadItem(_Item, PadStyles.Style_Standard, new Point(0, 0));
                _Item.Add(Found);

            }
            else
            {
                if (!_Grid)
                {
                    _Item.Remove(Found);
                    return;
                }

            }


            Found.InDenHintergrund();


            Found.GridShow = (decimal)_GridShow;

            if (_Gridsnap < 0.0001F) { _Gridsnap = 0; }
        }













        public void ShowErweitertMenü(BasicPadItem Item)
        {
            var l = Item.GetStyleOptions();

            if (l.Count == 0)
            {
                MessageBox.Show("Objekt hat keine<br>Bearbeitungsmöglichkeiten.", enImageCode.Information);
                return;
            }


            EditBoxFlexiControl.Show(l);


            //if (tg.Count == 0) { return; }

            //var ClosMe = true;

            ////Item.DoStyleCommands(this, tg, ref ClosMe);



            //Item.RecomputePointAndRelationsx();


            //if (!ClosMe) { ShowErweitertMenü(Item); }



        }



        private void DruckerDokument_BeginPrint(object sender, PrintEventArgs e)
        {
            OnBeginnPrint(e);
        }

        private void DruckerDokument_EndPrint(object sender, PrintEventArgs e)
        {
            OnEndPrint(e);
        }

        private void OnHotItemChanged()
        {
            HotItemChanged?.Invoke(this, System.EventArgs.Empty);
        }

        private void OnBeginnPrint(PrintEventArgs e)
        {
            BeginnPrint?.Invoke(this, e);
        }

        private void OnEndPrint(PrintEventArgs e)
        {
            EndPrint?.Invoke(this, e);
        }






    }
}
