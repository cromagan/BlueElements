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
using BlueBasics.EventArgs;
using BlueControls.Forms;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using BlueDatabase;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using static BlueBasics.FileOperations;
using BlueDatabase.EventArgs;
using BlueControls.Designer_Support;

namespace BlueControls.Controls
{

    [Designer(typeof(BasicDesigner))]
    [DefaultEvent("Click")]
    public sealed partial class CreativePad : ZoomPad, IContextMenu
    {

        private bool ComputeOrders_isin;
        private bool RepairPrinterData_Prepaired;

        public static bool Debug_ShowPointOrder = false;
        public static bool Debug_ShowRelationOrder = false;
        private bool _isParsing;



        public string Caption = "";

        public string ID = string.Empty;

        /// <summary>
        /// Für automatische Generierungen, die zu schnell hintereinander kommen, ein Counter
        /// </summary>
        int IDCount = 0;

        private IMouseAndKeyHandle _GivesMouseComandsTo;

        public static readonly int DPI = 300;


        private enAutoRelationMode _AutoRelation = enAutoRelationMode.Alle_Erhalten;
        private bool _ShowInPrintMode;


        /// <summary>
        /// Die Auto-Beziehungen, die hinzukommen (würden)
        /// </summary>
        private readonly List<clsPointRelation> _NewAutoRelations = new List<clsPointRelation>();


        /// <summary>
        /// Die Punkte, die zum Schieben Markiert sind.
        /// </summary>
        private readonly List<PointDF> Sel_P = new List<PointDF>();
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


        private bool _OrdersValid;




        private static readonly CreativePad PadForCreation = new CreativePad();

        #region  Events 
        public event EventHandler<ContextMenuInitEventArgs> ContextMenuInit;
        public event EventHandler<ContextMenuItemClickedEventArgs> ContextMenuItemClicked;

        public event EventHandler Parsed;

        public event EventHandler DoInvalidate;

        public event PrintPageEventHandler PrintPage;

        public event PrintEventHandler BeginnPrint;
        public event PrintEventHandler EndPrint;
        #endregion



        #region  Properties 

        [DefaultValue(1.0)]
        public decimal SheetStyleScale
        {
            get
            {
                return Item.SheetStyleScale;
            }
            set
            {

                if (value < 0.1m) { value = 0.1m; }

                if (!_isParsing && Item.SheetStyleScale == value) { return; }

                Item.SheetStyleScale = value;

                if (_isParsing) { return; }


                Item.DesignOrStyleChanged();

                RepairAll(0, true);
                Invalidate();
            }
        }

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

        [DefaultValue(1.0)]
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

                if (Item != null)
                {
                    foreach (var thisItem in Item)
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


        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ItemCollectionPad Item { get; }


        private PointDF Getbetterpoint(double X, double Y, PointDF notPoint, bool MustUsableForAutoRelation)
        {

            foreach (var thispoint in Item.AllPoints)
            {

                if (thispoint != null)
                {

                    if (!MustUsableForAutoRelation || thispoint.CanUsedForAutoRelation)
                    {

                        if (thispoint != notPoint)
                        {
                            if (Math.Abs((double)thispoint.X - X) < 0.01 && Math.Abs((double)thispoint.Y - Y) < 0.01) { return GetPointWithLowerIndex(notPoint, thispoint, true); }
                        }

                    }

                }
            }

            return null;
        }

        [DefaultValue("")]
        public string SheetStyle
        {
            get
            {
                if (Skin.StyleDB == null) { Skin.InitStyles(); }
                if (Skin.StyleDB == null) { return string.Empty; }
                if (Item.SheetStyle == null) { return string.Empty; }
                return Item.SheetStyle.CellFirstString();
            }
            set
            {
                if (!_isParsing && value == SheetStyle) { return; }
                if (Skin.StyleDB == null) { Skin.InitStyles(); }

                Item.SheetStyle = Skin.StyleDB.Row[value];
                if (Item.SheetStyle == null) { Item.SheetStyle = Skin.StyleDB.Row.First(); }// Einfach die Erste nehmen

                if (_isParsing) { return; }

                Item.DesignOrStyleChanged();

                RepairAll(0, false);
                Invalidate();
            }
        }

        #endregion



        public CreativePad()
        {

            // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
            InitializeComponent();


            // Initialisierungen nach dem Aufruf InitializeComponent() hinzufügen


            Item = new ItemCollectionPad();
            Item.ListOrItemChanged += Item_ListOrItemChanged;
            Item.ItemRemoved += _Item_ItemRemoved;
            Item.ItemAdded += _Item_ItemAdded;
            Item.DoInvalidate += Item_DoInvalidate;

            SetDoubleBuffering();
            Initialize();
            _MouseHighlight = false;
            Item.GenPoints();


        }

        private void Item_DoInvalidate(object sender, System.EventArgs e)
        {
            Invalidate();
        }

        private void Item_ListOrItemChanged(object sender, System.EventArgs e)
        {
            Invalidate();
        }

        public void InvalidateOrder()
        {
            _OrdersValid = false;
        }



        //public List<clsPointRelation> AllRelations()
        //{

        //    var R = new List<clsPointRelation>();
        //    foreach (var thisItem in Item)
        //    {
        //        if (thisItem != null)
        //        {
        //            R.AddRange(thisItem.RelationList());
        //        }
        //    }
        //    R.AddIfNotExists(_ExternalRelations);
        //    return R;
        //}





        internal Rectangle DruckbereichRect()
        {
            if (Item.P_rLO == null) { return new Rectangle(0, 0, 0, 0); }
            return new Rectangle((int)Item.P_rLO.X, (int)Item.P_rLO.Y, (int)(Item.P_rRU.X - Item.P_rLO.X), (int)(Item.P_rRU.Y - Item.P_rLO.Y));
        }

        protected override RectangleDF MaxBounds()
        {
            return MaxBounds(null);
        }

        internal RectangleDF MaxBounds(List<BasicPadItem> ZoomItems)
        {

            RectangleDF r;
            if (Item == null || Item.Count == 0)
            {
                r = new RectangleDF(0, 0, 0, 0);
            }
            else
            {
                r = Item.MaximumBounds(ZoomItems);
            }







            if (Item.SheetSizeInMM.Width > 0 && Item.SheetSizeInMM.Height > 0)
            {

                var X1 = Math.Min(r.Left, 0);
                var y1 = Math.Min(r.Top, 0);


                var x2 = Math.Max(r.Right, modConverter.mmToPixel((decimal)Item.SheetSizeInMM.Width, DPI));
                var y2 = Math.Max(r.Bottom, modConverter.mmToPixel((decimal)Item.SheetSizeInMM.Height, DPI));

                return new RectangleDF(X1, y1, x2 - X1, y2 - y1);
            }

            return r;


        }


        public Bitmap ToBitmap(decimal Scale)
        {
            var r = MaxBounds(null);
            if (r.Width == 0) { return null; }


            do
            {
                if ((int)(r.Width * Scale) > 15000)
                {
                    Scale = Scale * 0.8m;
                }
                else if ((int)(r.Height * Scale) > 15000)
                {
                    Scale = Scale * 0.8m;
                }
                else if ((int)(r.Height * Scale) * (int)(r.Height * Scale) > 90000000)
                {
                    Scale = Scale * 0.8m;
                }
                else
                {
                    break;
                }

                modAllgemein.CollectGarbage();
            } while (true);



            var I = new Bitmap((int)(r.Width * Scale), (int)(r.Height * Scale));


            using (var gr = Graphics.FromImage(I))
            {
                gr.Clear(Color.White);

                if (!Draw(gr, Scale, r.Left * Scale, r.Top * Scale, Size.Empty, true, null))
                {
                    return ToBitmap(Scale);
                }

            }


            return I;
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
                if (((IMouseAndKeyHandle)_GivesMouseComandsTo).KeyUp(this, e, _Zoom, _MoveX, _MoveY)) { return; }
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
                        Item.Remove(Del[0]);
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


            var Ho = HotItem(e);

            if (Ho is IMouseAndKeyHandle ho2)
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

                        if (!thisPoint.CanMove(Item.AllRelations))
                        {
                            Invalidate();
                            Forms.QuickInfo.Show("Dieser Punkt ist fest definiert<br>und kann nicht verschoben werden.");
                            return;
                        }

                        Unselect();
                        Sel_P.Add(thisPoint);
                        _OrdersValid = false;
                        ComputeMovingData();
                        Invalidate();
                        return;
                    }
                }



                if ((ModifierKeys & System.Windows.Forms.Keys.Control) <= 0 && Sel_P.Count > 0)
                {
                    Unselect();

                    if (HotItem(e) == null) { ComputeMovingData(); }
                    Invalidate();

                }

                if (Ho != null)
                {
                    Sel_P.AddIfNotExists(Ho.Points);
                    _OrdersValid = false;
                    ComputeMovingData();
                    Invalidate();
                }

            }
        }

        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e) => DoMouseMove(e); // Kann nicht public gemacht werden, deswegen Umleitung
        internal void DoMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var ho = HotItem(e);
            if (_GivesMouseComandsTo != null)
            {
                if (e.Button == System.Windows.Forms.MouseButtons.None && ho != _GivesMouseComandsTo)
                {
                    _GivesMouseComandsTo = null;
                    Invalidate();
                }
                else
                {
                    if (!((IMouseAndKeyHandle)_GivesMouseComandsTo).MouseMove(this, e, _Zoom, _MoveX, _MoveY))
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
                if (ho is IMouseAndKeyHandle Ho2)
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
                if (!((IMouseAndKeyHandle)_GivesMouseComandsTo).MouseUp(this, e, _Zoom, _MoveX, _MoveY))
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
                        _OrdersValid = false;
                        Invalidate();
                        break;
                    }
                }

                if (Convert.ToBoolean(_AutoRelation | enAutoRelationMode.NurBeziehungenErhalten) && _NewAutoRelations.Count > 0)
                {
                    Item.AllRelations.AddRange(_NewAutoRelations);
                    InvalidateOrder();
                    RepairAll(0, false);
                }

            }

            _NewAutoRelations.Clear();

            RepairAll(1, false);

            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {

                FloatingInputBoxListBoxStyle.ContextMenuShow(this, e);

            }
            ComputeMovingData();
            Invalidate(); // Damit auch snap-Punkte wieder gelöscht werden
        }








        private bool Draw(Graphics GR, decimal cZoom, decimal MoveX, decimal MoveY, Size SizeOfParentControl, bool ForPrinting, List<BasicPadItem> VisibleItems)
        {


            try
            {
                if (Item.SheetStyle == null || Item.SheetStyleScale < 0.1m) { return true; }

                ComputeOrders();

                foreach (var thisItem in Item)
                {
                    if (thisItem != null)
                    {
                        if (VisibleItems == null || VisibleItems.Contains(thisItem))
                        {
                            thisItem.Draw(GR, cZoom, MoveX, MoveY, 0, SizeOfParentControl, ForPrinting);
                        }
                    }
                }
                return true;
            }
            catch
            {
                modAllgemein.CollectGarbage();
                return false;
            }


        }

        protected override void InitializeSkin()
        {

        }


        internal void DrawCreativePadToBitmap(Bitmap BMP, enStates vState, decimal zoomf, decimal X, decimal Y, List<BasicPadItem> VisibleItems)
        {

            try
            {

                var TMPGR = Graphics.FromImage(BMP);

                if (Item.SheetSizeInMM.Width > 0 && Item.SheetSizeInMM.Height > 0)
                {
                    Skin.Draw_Back(TMPGR, enDesign.Table_And_Pad, vState, DisplayRectangle, this, true);
                    var SSW = Math.Round(modConverter.mmToPixel((decimal)Item.SheetSizeInMM.Width, DPI), 1);
                    var SSH = Math.Round(modConverter.mmToPixel((decimal)Item.SheetSizeInMM.Height, DPI), 1);
                    var LO = new PointDF(0m, 0m).ZoomAndMove(zoomf, X, Y);
                    var RU = new PointDF(SSW, SSH).ZoomAndMove(zoomf, X, Y);

                    var R = new Rectangle((int)LO.X, (int)LO.Y, (int)(RU.X - LO.X), (int)(RU.Y - LO.Y));
                    TMPGR.FillRectangle(Brushes.White, R);
                    TMPGR.DrawRectangle(PenGray, R);

                    var rtx = (int)(Item.P_rLO.X * zoomf - X);
                    var rty = (int)(Item.P_rLO.Y * zoomf - Y);
                    var rtx2 = (int)(Item.P_rRU.X * zoomf - X);
                    var rty2 = (int)(Item.P_rRU.Y * zoomf - Y);
                    var Rr = new Rectangle(rtx, rty, rtx2 - rtx, rty2 - rty);
                    if (!_ShowInPrintMode)
                    {
                        TMPGR.DrawRectangle(PenGray, Rr);
                    }

                }
                else
                {
                    TMPGR.Clear(Color.White);
                }



                if (!Draw(TMPGR, zoomf, X, Y, BMP.Size, _ShowInPrintMode, VisibleItems))
                {
                    DrawCreativePadToBitmap(BMP, vState, zoomf, X, Y, VisibleItems);
                    return;
                }


                // Erst Beziehungen, weil die die Grauen Punkte zeichnet
                foreach (var ThisRelation in Item.AllRelations)
                {
                    ThisRelation.Draw(TMPGR, zoomf, X, Y, Item.AllRelations.IndexOf(ThisRelation));
                }


                if (Debug_ShowPointOrder)
                {

                    // Alle Punkte mit Order anzeigen
                    foreach (var ThisPoint in Item.AllPoints)
                    {
                        ThisPoint.Draw(TMPGR, zoomf, X, Y, enDesign.Button_EckpunktSchieber_Phantom, enStates.Standard, false);
                    }
                }


                //     If _ItemsEditable Then

                // Dann die selectiereren Punkte

                foreach (var ThisPoint in Sel_P)
                {
                    if (ThisPoint.CanMove(Item.AllRelations))
                    {
                        ThisPoint.Draw(TMPGR, zoomf, X, Y, enDesign.Button_EckpunktSchieber, enStates.Standard, false);
                    }
                    else
                    {
                        ThisPoint.Draw(TMPGR, zoomf, X, Y, enDesign.Button_EckpunktSchieber_Phantom, enStates.Standard, false);
                    }
                }


                foreach (var ThisRelation in _NewAutoRelations)
                {


                    var P1 = ThisRelation.Points[0].ZoomAndMove(zoomf, X, Y);
                    var P2 = ThisRelation.Points[1].ZoomAndMove(zoomf, X, Y);

                    if (ThisRelation.RelationType == enRelationType.WaagerechtSenkrecht)
                    {
                        TMPGR.DrawEllipse(new Pen(Color.Green, 3), P1.X - 4, P1.Y - 3, 7, 7);
                        TMPGR.DrawEllipse(new Pen(Color.Green, 3), P2.X - 4, P2.Y - 3, 7, 7);
                        TMPGR.DrawLine(new Pen(Color.Green, 1), P1, P2);
                    }
                    else
                    {
                        TMPGR.DrawEllipse(new Pen(Color.Green, 3), P1.X - 4, P1.Y - 4, 7, 7);
                        TMPGR.DrawEllipse(new Pen(Color.Green, 3), P1.X - 8, P1.Y - 8, 15, 15);
                    }
                }



                if (_GivesMouseComandsTo is BasicPadItem PA)
                {
                    var DCoordinates = PA.UsedArea().ZoomAndMoveRect(zoomf, X, Y);

                    TMPGR.DrawRectangle(new Pen(Brushes.Red, 3), DCoordinates);
                }

                TMPGR.Dispose();
            }
            catch
            {
                DrawCreativePadToBitmap(BMP, vState, zoomf, X, Y, VisibleItems);
            }
        }



        protected override void DrawControl(Graphics gr, enStates state)
        {
            if (_BitmapOfControl == null)
            {
                _BitmapOfControl = new Bitmap(ClientSize.Width, ClientSize.Height, PixelFormat.Format32bppPArgb);
            }

            DrawCreativePadToBitmap(_BitmapOfControl, state, _Zoom, _MoveX, _MoveY, null);
            gr.DrawImage(_BitmapOfControl, 0, 0);
            Skin.Draw_Border(gr, enDesign.Table_And_Pad, state, DisplayRectangle);
        }



        private void _Item_ItemRemoved(object sender, System.EventArgs e)
        {
            Unselect();
            InvalidateOrder();
            ZoomFit();
            Invalidate();
        }

        private void _Item_ItemAdded(object sender, ListEventArgs e)
        {
            InvalidateOrder();
        }





        private bool MoveSelectedPoints(decimal X, decimal Y)
        {
            Item.RecomputePointAndRelations();



            var f = NotPerforming(false);

            foreach (var thispoint in Item.AllPoints)
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


            if (f == 0)
            {
                RepairAll(1, true);
            }
            else
            {
                RepairAll(2, false);
                // Sind eh schon fehler drinne, also schneller abbrechen, um nicht allzusehr verzögen
                // und sicherheitshalber große Änderungen verbieten, um nicht noch mehr kaputt zu machen...
            }

            Item.RecomputePointAndRelations();



            var f2 = NotPerforming(false);

            if (f2 > f)
            {
                foreach (var thispoint in Item.AllPoints)
                {
                    thispoint?.ReStore();
                }
                Item.RecomputePointAndRelations();

            }
            else if (f2 < f)
            {
                ComputeMovingData();
            }



            Invalidate();
            return Convert.ToBoolean(f2 == 0);
        }


        private void ComputeMovingData()
        {

            ComputeOrders();



            Move_X.Clear();
            Move_Y.Clear();
            var CanMove_X = true;
            var CanMove_Y = true;

            foreach (var thispoint in Sel_P)
            {
                Move_X.AddIfNotExists(ConnectsWith(thispoint, true, false));
                Move_Y.AddIfNotExists(ConnectsWith(thispoint, false, false));
            }

            foreach (var thispoint in Move_X)
            {
                if (!thispoint.CanMoveX(Item.AllRelations))
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
                if (!thispoint.CanMoveY(Item.AllRelations))
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ThisItemCol"></param>
        /// <param name="Level">             Level 0 gibt es nicht;
        /// Level 1 = Normal / Reparier nur die neuen Sachen ;
        /// Level 2 = Leicht / Reparier nur die neuen Sachen mit schnelleren Abbruchbedingungen</param>
        /// <param name="AllowBigChanges"></param>
        /// <returns></returns>
        public bool PerformAll(ItemCollectionPad ThisItemCol, int Level, bool AllowBigChanges)
        {

            var L = new List<string>();
            var Methode = 0;
            ComputeOrders();

            do
            {
                var tmp = "";

                foreach (var ThisRelation in Item.AllRelations)
                {

                    if (ThisRelation.Performs(true))
                    {
                        ThisRelation.Computed = true;
                    }
                    else
                    {
                        ThisRelation.Computed = false;
                        tmp = tmp + ThisRelation.Order + ";";
                    }

                }

                if (string.IsNullOrEmpty(tmp)) { return true; }



                if (L.Contains(tmp))
                {
                    if (Level == 2) { return false; }
                    if (Methode == 2) { return false; }

                    Methode += 1;
                    Relations_Optimize();
                    ThisItemCol.RecomputePointAndRelations();
                    ComputeOrders();
                    L.Clear();
                }
                else
                {
                    L.Add(tmp);
                }

                foreach (var ThisRelation in Item.AllRelations)
                {
                    if (!ThisRelation.Computed)
                    {
                        ThisRelation.MakePointKonsistent(LowestOrder(ThisRelation.Points), AllowBigChanges);
                    }
                }

            } while (true);

        }

        private void ComputeOrders()
        {
            if (_OrdersValid) { return; }

            if (ComputeOrders_isin) { return; }
            ComputeOrders_isin = true;


            Relations_DeleteInvalid();

            ComputePointOrder();
            ComputeRelationOrder();

            _OrdersValid = true;

            ComputeOrders_isin = false;
        }


        private void ComputePointOrder()
        {
            var Modus = 0;
            var Count = 1;


            var _Points = new List<PointDF>();
            _Points.AddRange(Item.AllPoints);


            foreach (var Thispoint in _Points)
            {
                Thispoint.Order = int.MaxValue;
            }

            _Points.Sort();

            var RelationNone = new List<clsPointRelation>();
            var RelationY = new List<clsPointRelation>();
            var RelationX = new List<clsPointRelation>();
            var RelationXY = new List<clsPointRelation>();


            foreach (var thisRelation in Item.AllRelations)
            {
                if (thisRelation.Connects(true))
                {
                    if (thisRelation.Connects(false))
                    {
                        RelationXY.Add(thisRelation);
                    }
                    else
                    {
                        RelationX.Add(thisRelation);
                    }
                }
                else
                {
                    if (thisRelation.Connects(false))
                    {
                        RelationY.Add(thisRelation);
                        //Stop
                    }
                    else
                    {
                        RelationNone.Add(thisRelation);
                    }
                }
            }


            do
            {
                PointDF DidPoint = null;
                clsPointRelation DidRel = null;


                foreach (var Thispoint in _Points)
                {
                    switch (Modus)
                    {
                        case 0: // Fixpunkte hinzufgen
                            if (Thispoint.PositionFix)
                            {
                                Thispoint.Order = Count;
                                DidPoint = Thispoint;
                            }
                            break;

                        case 1: // X und Y Fix hinzufügen

                            foreach (var thisRelation in RelationXY)
                            {
                                if (thisRelation.NeedCount(Thispoint))
                                {
                                    if (thisRelation.Connects(true) && thisRelation.Connects(false))
                                    {
                                        Thispoint.Order = Count;
                                        DidPoint = Thispoint;
                                        DidRel = thisRelation;
                                        break;
                                    }
                                }
                            }
                            break;

                        case 2: // Fixe Y-Punkte hinzufügen
                            foreach (var thisRelation in RelationY)
                            {
                                if (thisRelation.NeedCount(Thispoint))
                                {
                                    if (thisRelation.Connects(false))
                                    {
                                        Thispoint.Order = Count;
                                        DidPoint = Thispoint;
                                        DidRel = thisRelation;
                                        break;
                                    }
                                }
                            }

                            break;

                        case 3: // Fixe X-Punkte hinzufügen
                            foreach (var thisRelation in RelationX)
                            {
                                if (thisRelation.NeedCount(Thispoint))
                                {
                                    if (thisRelation.Connects(true))
                                    {
                                        Thispoint.Order = Count;
                                        DidPoint = Thispoint;
                                        DidRel = thisRelation;
                                        break;
                                    }
                                }
                            }
                            break;

                        case 4: // Punkte hinzufügen, die in einer Beziehung sind UND ein Punkt bereits einen Order hat

                            foreach (var thisRelation in RelationNone)
                            {
                                if (thisRelation.NeedCount(Thispoint))
                                {
                                    Thispoint.Order = Count;
                                    DidPoint = Thispoint;
                                    DidRel = thisRelation;
                                    break;
                                }
                            }
                            break;

                        case 5: // Selectierte Punkte bevorzugen
                            if (Sel_P.Contains(Thispoint))
                            {
                                Thispoint.Order = Count;
                                DidPoint = Thispoint;
                            }

                            break;

                        case 6: // Der gute Rest
                            Thispoint.Order = Count;
                            DidPoint = Thispoint;
                            break;

                        default:
                            return;
                    }

                    if (DidPoint != null) { break; }
                }



                if (DidPoint != null)
                {
                    _Points.Remove(DidPoint);
                    Count += 1;
                    if (Modus > 1) { Modus = 1; }
                }
                else
                {
                    Modus += 1;
                }

                if (Modus == 5 && Sel_P.Count == 0) { Modus += 1; }


                if (_Points.Count == 0) { return; }



                if (DidRel != null && DidRel.AllPointsHaveOrder())
                {
                    RelationNone.Remove(DidRel);
                    RelationY.Remove(DidRel);
                    RelationX.Remove(DidRel);
                    RelationXY.Remove(DidRel);
                }


            } while (true);

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
                   if (NotPerforming(false) == 0)
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


            foreach (var ThisPoint in Item.AllPoints)
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
                        RelationDeleteExternal(thisItem);
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

                    case "#printme":
                        Done = true;
                        thisItem.PrintMe = true;
                        break;

                    case "#printmenot":
                        Done = true;
                        thisItem.PrintMe = false;
                        break;
                    case "#duplicate":
                        Done = true;
                        Item.Add((BasicPadItem)((ICloneable)thisItem).Clone());
                        break;
                }



            }

            //if (!Done) { Done = thisItem.ContextMenuItemClicked(new ContextMenuItemClickedEventArgs(e.ClickedComand, null, e.Tags)); }



            InvalidateOrder();

            RepairAll(0, false); // Da könnte sich ja alles geändert haben...
            Invalidate();

            return Done;

        }

        public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e)
        {
            ContextMenuItemClicked?.Invoke(this, e);
        }

        public List<BasicPadItem> HotItems(System.Windows.Forms.MouseEventArgs e)
        {


            var P = new Point((int)((decimal)(e.X + _MoveX) / _Zoom), (int)((decimal)(e.Y + _MoveY) / _Zoom));

            var l = new List<BasicPadItem>();

            foreach (var ThisItem in Item)
            {
                if (ThisItem != null && ThisItem.Contains(P, _Zoom))
                {
                    l.Add(ThisItem);
                }
            }
            return l;
        }


        private BasicPadItem HotItem(System.Windows.Forms.MouseEventArgs e)
        {
            var l = HotItems(e); // _Item.Search(CInt((e.X + SliderX.Value) / _Zoom), CInt((e.Y + SliderY.Value) / _Zoom), _Zoom)

            var Mina = long.MaxValue;
            BasicPadItem _HotItem = null;


            foreach (var ThisItem in l)
            {
                var a = (long)Math.Abs(ThisItem.UsedArea().Width) * (long)Math.Abs(ThisItem.UsedArea().Height);
                if (a <= Mina)
                {
                    // Gleich deswegen, dass neuere, IDENTISCHE Items dass oberste gewählt wird.
                    Mina = a;
                    _HotItem = ThisItem;
                }
            }

            return _HotItem;

        }


        public void GetContextMenuItems(System.Windows.Forms.MouseEventArgs e, ItemCollectionList Items, out object HotItem, List<string> Tags, ref bool Cancel, ref bool Translate)
        {

            HotItem = this.HotItem(e);


            if (HotItem != null)
            {
                Items.Add(new TextListItem("Allgemeine Element-Aktionen", true));
                Items.Add(new TextListItem("#Erweitert", "Objekt bearbeiten", enImageCode.Stift));
                Items.Add(new LineListItem());


                Items.Add(new TextListItem("#ExterneBeziehungen", "Objektübergreifende Punkt-Beziehungen aufheben", enImageCode.Kreuz));
                if (((BasicPadItem)HotItem).PrintMe)
                {
                    Items.Add(new TextListItem("#PrintMeNot", "Objekt nicht drucken", QuickImage.Get("Drucker|16||1")));
                }
                else
                {
                    Items.Add(new TextListItem("#PrintMe", "Objekt drucken", enImageCode.Drucker));
                }


                Items.Add(new TextListItem("#Duplicate", "Objekt duplizieren", enImageCode.Kopieren, HotItem is ICloneable));


                Items.Add(new LineListItem());
                Items.Add(new TextListItem("#Vordergrund", "In den Vordergrund", enImageCode.InDenVordergrund));
                Items.Add(new TextListItem("#Hintergrund", "In den Hintergrund", enImageCode.InDenHintergrund));
                Items.Add(new TextListItem("#Vorne", "Eine Ebene nach vorne", enImageCode.EbeneNachVorne));
                Items.Add(new TextListItem("#Hinten", "Eine Ebene nach hinten", enImageCode.EbeneNachHinten));
            }


            foreach (var Thispoint in Item.AllPoints)
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


            var r = new clsPointRelation(rel, Snap1, SnaP2);


            foreach (var thisRelation in Item.AllRelations)
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

            foreach (var thisRelation in Item.AllRelations)
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
                    BP2 = Getbetterpoint(Convert.ToDouble(PSnapToX.X), Convert.ToDouble(PSnapToY.Y), BP1, true); // muß Snapable sein
                }
                else
                {
                    BP1 = Getbetterpoint(Convert.ToDouble(PSnapToX.X), Convert.ToDouble(PSnapToY.Y), null, true); // muß Snapable sein
                    BP2 = null;
                    if (BP1 != null) // kann auch NichtSnapable sein
                    {
                        BP2 = Getbetterpoint(Convert.ToDouble(PMoveX.X), Convert.ToDouble(PMoveY.Y), BP1, false);
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

        public void Relation_Add(enRelationType enRelationType, PointDF Point1, PointDF Point2)
        {
            Item.AllRelations.Add(new clsPointRelation(enRelationType, Point1, Point2));
            InvalidateOrder();
        }



        private void RelationDeleteExternal(BasicPadItem ThisItem)
        {

            foreach (var ThisRelation in Item.AllRelations)
            {
                if (ThisRelation != null)
                {

                    if (!ThisRelation.IsInternal())
                    {
                        foreach (var Thispoint in ThisRelation.Points)
                        {

                            if (Thispoint.Parent is BasicPadItem tItem)
                            {
                                if (tItem == ThisItem)
                                {
                                    Item.AllRelations.Remove(ThisRelation);
                                    RelationDeleteExternal(ThisItem); // 'Rekursiv
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            InvalidateOrder();
        }




        public string DataToString()
        {

            RepairAll(0, false);

            var t = "{";


            if (!string.IsNullOrEmpty(ID)) { t = t + "ID=" + ID.ToNonCritical() + ", "; }

            if (!string.IsNullOrEmpty(Caption)) { t = t + "Caption=" + Caption.ToNonCritical() + ", "; }

            if (Item.SheetStyle != null) { t = t + "Style=" + Item.SheetStyle.CellFirstString().ToNonCritical() + ", "; }

            if (Item.SheetStyleScale < 0.1m) { Item.SheetStyleScale = 1.0m; }

            if (Math.Abs(Item.SheetStyleScale - 1) > 0.001m) { t = t + "FontScale=" + Item.SheetStyleScale + ", "; }

            if (Item.SheetSizeInMM.Width > 0 && Item.SheetSizeInMM.Height > 0)
            {
                t = t + "SheetSize=" + Item.SheetSizeInMM + ", ";
                t = t + "PrintArea=" + Item.RandinMM + ", ";
            }

            //  _AutoSort = False
            t = t + "Items=" + Item.ToString() + ", ";


            t = t + "Grid=" + _Grid + ", ";
            t = t + "GridShow=" + _GridShow + ", ";
            t = t + "GridSnap=" + _Gridsnap + ", ";

            //Dim One As Boolean

            foreach (var ThisRelation in Item.AllRelations)
            {
                if (ThisRelation != null)
                {
                    if (!ThisRelation.IsInternal() && ThisRelation.IsOk())
                    {
                        t = t + "Relation=" + ThisRelation + ", ";
                    }
                }
            }


            return t.TrimEnd(", ") + "}";

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="needPrinterData"></param>
        /// <param name="useThisID">Wenn das Blatt bereits eine Id hat, muss die Id verwendet werden. Wird das Feld leer gelassen, wird die beinhaltete Id benutzt.</param>
        public void ParseData(string value, bool needPrinterData, string useThisID)
        {

            Initialize();
            if (string.IsNullOrEmpty(value) || value.Length < 3) { return; }
            if (value.Substring(0, 1) != "{") { return; }// Alte Daten gehen eben verloren.

            _isParsing = true;
            var Beg = 0;

            ID = useThisID;

            do
            {
                Beg += 1;
                if (Beg > value.Length) { break; }
                var T = value.ParseTag(Beg);
                var pvalue = value.ParseValue(T, Beg);

                Beg = Beg + T.Length + pvalue.Length + 2;
                switch (T)
                {
                    case "sheetsize":
                        Item.SheetSizeInMM = Extensions.SizeFParse(pvalue);
                        Item.GenPoints();
                        break;

                    case "printarea":
                        Item.RandinMM = Extensions.PaddingParse(pvalue);
                        Item.GenPoints();
                        break;

                    case "items":
                        Item.Parse(pvalue);
                        break;

                    case "relation":
                        Item.AllRelations.Add(new clsPointRelation(pvalue, Item.AllPoints));
                        InvalidateOrder();
                        break;

                    case "caption":
                        Caption = pvalue.FromNonCritical();
                        break;

                    case "id":
                        if (string.IsNullOrEmpty(ID)) { ID = pvalue.FromNonCritical(); }
                        break;

                    case "style":
                        SheetStyle = pvalue;
                        break;

                    case "fontscale":
                        SheetStyleScale = decimal.Parse(pvalue);
                        break;

                    case "grid":
                        _Grid = pvalue.FromPlusMinus();
                        break;

                    case "gridshow":
                        _GridShow = float.Parse(pvalue);
                        break;

                    case "gridsnap":
                        _Gridsnap = float.Parse(pvalue);
                        break;

                    default:
                        Develop.DebugPrint(enFehlerArt.Fehler, "Tag unbekannt: " + T);
                        break;
                }
            } while (true);


            //   _AutoSort = False ' False beim Parsen was anderes Rauskommt

            Invalidate();


            CheckGrid();

            _isParsing = false;


            RepairAll(0, true);

            if (needPrinterData) { RepairPrinterData(); }

            OnParsed();
        }

        private void OnParsed()
        {
            Parsed?.Invoke(this, System.EventArgs.Empty);
        }

        internal bool RenameColumn(string oldName, ColumnItem cColumnItem)
        {
            var did = false;

            foreach (var thisItem in Item)
            {
                if (thisItem is ICanHaveColumnVariables variables)
                {
                    if (variables.RenameColumn(oldName, cColumnItem))
                    {
                        thisItem.RecomputePointAndRelations();
                        did = true;
                    }
                }
            }

            if (!did) { return false; }


            RepairAll(0, true);
            RepairAll(1, true);

            return true;
        }

        private void Initialize()
        {
            Item.SheetSizeInMM = Size.Empty;
            Item.RandinMM = System.Windows.Forms.Padding.Empty;


            Caption = "";

            IDCount++;

            ID = "#" + DateTime.UtcNow.ToString(Constants.Format_Date) + IDCount; // # ist die erkennung, dass es kein Dateiname sondern ein Item ist


            _NewAutoRelations.Clear();
            Item.AllRelations.Clear();
            Item.AllPoints.Clear();


            if (Skin.StyleDB == null) { Skin.InitStyles(); }
            Item.SheetStyle = null;
            Item.SheetStyleScale = 1.0m;

            if (Skin.StyleDB != null) { Item.SheetStyle = Skin.StyleDB.Row.First(); }


            Sel_P.Clear();
            Move_X.Clear();
            Move_Y.Clear();
        }



        public void Unselect()
        {
            Sel_P.Clear();
            Move_X.Clear();
            Move_Y.Clear();
            _NewAutoRelations.Clear();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="Level">Level 0 = Hart / Reparier alles mit Gewalt; 
        /// Level 1 = Normal / Reparier nur die neuen Sachen;
        ///  Level 2 = Leicht / Reparier nur die neuen Sachen mit schnelleren Abbruchbedingungen</param>
        /// <param name="AllowBigChanges"></param>
        /// <returns></returns>

        public bool RepairAll(int Level, bool AllowBigChanges)
        {

            if (Level == 0)
            {
                //RepairAll_OldItemc = Itemc + 1; // Löst eine Kettenreaktion aus
                Item.RecomputePointAndRelations();
                InvalidateOrder();
            }

            return PerformAll(Item, Level, AllowBigChanges);
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


            var i = ToBitmap(3);
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
                Item.SheetSizeInMM = new SizeF((int)(DruckerDokument.DefaultPageSettings.PaperSize.Height * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.PaperSize.Width * 25.4 / 100));
                Item.RandinMM = new System.Windows.Forms.Padding((int)(DruckerDokument.DefaultPageSettings.Margins.Left * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.Margins.Top * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.Margins.Right * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.Margins.Bottom * 25.4 / 100));


            }
            else
            {
                // Hochformat
                Item.SheetSizeInMM = new SizeF((int)(DruckerDokument.DefaultPageSettings.PaperSize.Width * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.PaperSize.Height * 25.4 / 100));
                Item.RandinMM = new System.Windows.Forms.Padding((int)(DruckerDokument.DefaultPageSettings.Margins.Left * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.Margins.Top * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.Margins.Right * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.Margins.Bottom * 25.4 / 100));
            }


            Item.GenPoints();
        }


        public void ShowWorkingAreaSetup()
        {

            var OriD = new PrintDocument();


            OriD.DefaultPageSettings.Landscape = false;
            OriD.DefaultPageSettings.PaperSize = new PaperSize("Benutzerdefiniert", (int)(Item.SheetSizeInMM.Width / 25.4 * 100), (int)(Item.SheetSizeInMM.Height / 25.4 * 100));
            OriD.DefaultPageSettings.Margins.Top = (int)(Item.RandinMM.Top / 25.4 * 100);
            OriD.DefaultPageSettings.Margins.Bottom = (int)(Item.RandinMM.Bottom / 25.4 * 100);
            OriD.DefaultPageSettings.Margins.Left = (int)(Item.RandinMM.Left / 25.4 * 100);
            OriD.DefaultPageSettings.Margins.Right = (int)(Item.RandinMM.Right / 25.4 * 100);




            using (var x = new BlueControls.Forms.PageSetupDialog(OriD, true))
            {
                x.ShowDialog();
                if (x.Canceled()) { return; }
            }


            Item.SheetSizeInMM = new SizeF((int)(OriD.DefaultPageSettings.PaperSize.Width * 25.4 / 100), (int)(OriD.DefaultPageSettings.PaperSize.Height * 25.4 / 100));
            Item.RandinMM = new System.Windows.Forms.Padding((int)(OriD.DefaultPageSettings.Margins.Left * 25.4 / 100), (int)(OriD.DefaultPageSettings.Margins.Top * 25.4 / 100), (int)(OriD.DefaultPageSettings.Margins.Right * 25.4 / 100), (int)(OriD.DefaultPageSettings.Margins.Bottom * 25.4 / 100));
            Item.GenPoints();
        }



        private void RepairPrinterData()
        {

            if (RepairPrinterData_Prepaired) { return; }

            RepairPrinterData_Prepaired = true;

            DruckerDokument.DocumentName = Caption;

            var done = false;
            foreach (PaperSize ps in DruckerDokument.PrinterSettings.PaperSizes)
            {
                if (ps.Width == (int)(Item.SheetSizeInMM.Width / 25.4 * 100) && ps.Height == (int)(Item.SheetSizeInMM.Height / 25.4 * 100))
                {
                    done = true;
                    DruckerDokument.DefaultPageSettings.PaperSize = ps;
                    break;
                }
            }

            if (!done)
            {
                DruckerDokument.DefaultPageSettings.PaperSize = new PaperSize("Custom", (int)(Item.SheetSizeInMM.Width / 25.4 * 100), (int)(Item.SheetSizeInMM.Height / 25.4 * 100));
            }
            DruckerDokument.DefaultPageSettings.PrinterResolution = DruckerDokument.DefaultPageSettings.PrinterSettings.PrinterResolutions[0];
            DruckerDokument.OriginAtMargins = true;
            DruckerDokument.DefaultPageSettings.Margins = new Margins((int)(Item.RandinMM.Left / 25.4 * 100), (int)(Item.RandinMM.Right / 25.4 * 100), (int)(Item.RandinMM.Top / 25.4 * 100), (int)(Item.RandinMM.Bottom / 25.4 * 100));
        }

        public void SaveAsBitmap(string Title, string OptionalFileName)
        {

            if (!string.IsNullOrEmpty(OptionalFileName))
            {
                SaveAsBitmapInternal(OptionalFileName);
                return;
            }

            Title = Title.RemoveChars(Constants.Char_DateiSonderZeichen);

            PicsSave.FileName = Title + ".png";

            PicsSave.ShowDialog();
        }

        private void SaveAsBitmapInternal(string Filename)
        {

            var i = ToBitmap(1);

            if (i == null)
            {
                // Develop.DebugPrint(enFehlerArt.Warnung, "Bild ist null: " + Filename);
                return;
            }


            switch (Filename.FileSuffix().ToUpper())
            {

                case "JPG":
                case "JPEG":
                    i.Save(Filename, ImageFormat.Jpeg);
                    break;

                case "PNG":
                    i.Save(Filename, ImageFormat.Png);
                    break;

                case "BMP":
                    i.Save(Filename, ImageFormat.Bmp);
                    break;

                default:
                    MessageBox.Show("Dateiformat unbekannt: " + PicsSave.FileName.FileSuffix().ToUpper(), enImageCode.Warnung, "OK");
                    return;
            }
        }

        private void PicsSave_FileOk(object sender, CancelEventArgs e)
        {
            if (e.Cancel) { return; }
            SaveAsBitmapInternal(PicsSave.FileName);
        }

        public new void Invalidate()
        {
            base.Invalidate();
            OnDoInvalidate();

        }

        public void OnDoInvalidate()
        {
            DoInvalidate?.Invoke(this, System.EventArgs.Empty); // Invalidate-Befehl weitergeben an untergeordnete Steuerelemente
        }

        public bool ParseVariable(string VariableName, enValueType ValueType, string Value)
        {

            var did = false;

            foreach (var thisItem in Item)
            {
                if (thisItem is ICanHaveColumnVariables variables)
                {
                    if (variables.ParseVariable(VariableName, ValueType, Value))
                    {
                        thisItem.RecomputePointAndRelations();
                        did = true;
                    }
                }
            }

            if (did) { Invalidate(); }
            return did;
        }

        public void ParseVariableAndSpecialCodes(RowItem row)
        {

            foreach (var thiscolumnitem in row.Database.Column)
            {
                if (thiscolumnitem != null)
                {
                    ParseVariable(thiscolumnitem.Name, thiscolumnitem, row);
                }
            }

            ParseSpecialCodes();
        }

        private void ParseVariable(string VariableName, ColumnItem Column, RowItem Row)
        {


            switch (Column.Format)
            {
                case enDataFormat.Text:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.Gleitkommazahl:
                case enDataFormat.Datum_und_Uhrzeit:
                case enDataFormat.Bit:
                case enDataFormat.Ganzzahl:
                case enDataFormat.RelationText:
                    ParseVariable(VariableName, enValueType.Text, Row.CellGetString(Column));
                    break;

                case enDataFormat.Link_To_Filesystem:

                    var f = Column.BestFile(Row.CellGetString(Column), false);

                    if (FileExists(f))
                    {
                        if (Column.MultiLine)
                        {
                            ParseVariable(VariableName, enValueType.Text, f);
                        }
                        else
                        {
                            var x = modConverter.FileToString(f);
                            ParseVariable(VariableName, enValueType.BinaryImage, x);
                        }
                    }
                    break;


                //case enDataFormat.Relation:
                //    ParseVariable(VariableName, enValueType.Unknown, "Nicht implementiert");
                //    break;

                default:
                    Develop.DebugPrint("Format unbekannt: " + Column.Format);
                    break;

            }
        }




        public bool ParseSpecialCodes()
        {
            var did = false;

            foreach (var thisItem in Item)
            {
                if (thisItem is ICanHaveColumnVariables variables)
                {
                    if (variables.ParseSpecialCodes())
                    {
                        thisItem.RecomputePointAndRelations();
                        did = true;
                    }
                }
            }

            if (did) { Invalidate(); }

            return did;
        }

        public bool ResetVariables()
        {
            var did = false;

            foreach (var thisItem in Item)
            {
                if (thisItem is ICanHaveColumnVariables variables)
                {
                    if (variables.ResetVariables())
                    {
                        thisItem.RecomputePointAndRelations();
                        did = true;
                    }
                }
            }


            if (did) { Invalidate(); }
            return did;

        }



        public void GenerateFromRow(string LayoutID, RowItem Row, bool NeedPrinterData)
        {
            var LayoutNr = Row.Database.LayoutIDToIndex(LayoutID);
            ParseData(Row.Database.Layouts[LayoutNr], NeedPrinterData, string.Empty);
            ResetVariables();
            ParseVariableAndSpecialCodes(Row);

            var Count = 0;
            do
            {
                Count += 1;
                if (RepairAll(0, true)) { break; }
                if (Count > 20) { break; }
            } while (true);

            RepairAll(1, true);


        }

        private void CheckGrid()
        {
            GridPadItem Found = null;

            Invalidate();

            foreach (var ThisItem in Item)
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
                Found = new GridPadItem(PadStyles.Style_Standard, new Point(0, 0));
                Item.Add(Found);
                InvalidateOrder();
            }
            else
            {
                if (!_Grid)
                {
                    Item.Remove(Found);
                    InvalidateOrder();
                    return;
                }

            }


            Found.InDenHintergrund();


            Found.GridShow = (decimal)_GridShow;

            if (_Gridsnap < 0.0001F) { _Gridsnap = 0; }
        }
        public void ComputeRelationOrder()
        {
            var Count = 0;

            // Zurücksetzen ---- 
            foreach (var ThisRelation in Item.AllRelations)
            {
                ThisRelation.Order = -1;
            }


            for (var Durch = 0; Durch <= 1; Durch++)
            {

                do
                {
                    clsPointRelation NextRel = null;
                    var RelPO = int.MaxValue;

                    foreach (var ThisRelation in Item.AllRelations)
                    {
                        if (ThisRelation.Order < 0)
                        {
                            if (Durch > 0 || ThisRelation.IsInternal())
                            {
                                if (LowestOrder(ThisRelation.Points) < RelPO)
                                {
                                    NextRel = ThisRelation;
                                    RelPO = LowestOrder(ThisRelation.Points);
                                }
                            }
                        }
                    }

                    if (NextRel == null) { break; }

                    Count += 1;
                    NextRel.Order = Count;
                } while (true);

            }

            Item.AllRelations.Sort();
        }

        public void Relations_Optimize()
        {
            if (NotPerforming(true) > 0) { return; }


            var Cb = new List<PointDF>();
            var DobR = new List<clsPointRelation>();


            foreach (var thisPoint in Item.AllPoints)
            {
                var CX = ConnectsWith(thisPoint,  true, true);
                var CY = ConnectsWith(thisPoint,  false, true);

                // Ermitteln, die auf X und Y miteinander verbunden sind
                Cb.Clear();
                foreach (var thisPoint2 in CX)
                {
                    if (CY.Contains(thisPoint2)) { Cb.Add(thisPoint2); }
                }


                if (Cb.Count > 1)
                {

                    DobR.Clear();
                    foreach (var ThisRelation in Item.AllRelations)
                    {


                        // Wenn Punkte nicht direct verbunden sind, aber trotzdem Fix zueinander, die Beziehung optimieren
                        if (ThisRelation.RelationType == enRelationType.WaagerechtSenkrecht && !ThisRelation.IsInternal())
                        {
                            if (Cb.Contains(ThisRelation.Points[0]) && Cb.Contains(ThisRelation.Points[1]))
                            {
                                ThisRelation.RelationType = enRelationType.PositionZueinander;
                                ThisRelation.OverrideSavedRichtmaß(false);
                                InvalidateOrder();
                                Relations_Optimize();
                                return;
                            }
                        }


                        // Für nachher, die doppelten fixen Beziehungen merken
                        if (ThisRelation.RelationType == enRelationType.PositionZueinander)
                        {
                            if (Cb.Contains(ThisRelation.Points[0]) && Cb.Contains(ThisRelation.Points[1])) { DobR.Add(ThisRelation); }
                        }


                    }


                    // Und nun beziehungen löschen, die auf gleiche Objecte zugreifen
                    if (DobR.Count > 1)
                    {
                        foreach (var R1 in DobR)
                        {
                            // Mindestens eine muss external sein!!!
                            if (!R1.IsInternal())
                            {
                                foreach (var R2 in DobR)
                                {
                                    if (!R1.SinngemäßIdenitisch(R2))
                                    {

                                        if (R1.Points[0].Parent == R2.Points[0].Parent && R1.Points[1].Parent == R2.Points[1].Parent)
                                        {
                                            Item.AllRelations.Remove(R1);
                                            InvalidateOrder();
                                            Relations_Optimize();
                                            return;
                                        }

                                        if (R1.Points[0].Parent == R2.Points[1].Parent && R1.Points[1].Parent == R2.Points[0].Parent)
                                        {
                                            Item.AllRelations.Remove(R1);
                                            InvalidateOrder();
                                            Relations_Optimize();
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }



            // und nun direct nach doppelten suchen
            foreach (var r1 in Item.AllRelations)
            {
                if (!r1.IsInternal())
                {
                    foreach (var r2 in Item.AllRelations)
                    {
                        if (!r1.SinngemäßIdenitisch(r2) && !r2.IsInternal())
                        {
                            if (r1.SinngemäßIdenitisch(r2))
                            {
                                Item.AllRelations.Remove(r2);
                                InvalidateOrder();
                                Relations_Optimize();
                                return;

                            }

                            if (r1.UsesSamePoints(r2))
                            {
                                switch (r1.RelationType)
                                {
                                    case enRelationType.PositionZueinander:
                                        // Beziehungen mit gleichen punkten, aber einer mächtigen PositionZueinander -> andere löschen
                                        Item.AllRelations.Remove(r2);
                                        InvalidateOrder();
                                        Relations_Optimize();
                                        return;
                                    case enRelationType.WaagerechtSenkrecht when r2.RelationType == enRelationType.WaagerechtSenkrecht && r1.Richtmaß() != r2.Richtmaß():
                                        // Beziehungen mit gleichen punkten, aber spearat mit X und Y -> PositionZueinander konvertieren 
                                        r1.RelationType = enRelationType.PositionZueinander;
                                        r1.OverrideSavedRichtmaß(false);
                                        Item.AllRelations.Remove(r2);
                                        InvalidateOrder();
                                        Relations_Optimize();
                                        return;
                                }
                            }
                        }
                    }
                }
            }

        }



        public bool Relations_DeleteInvalid()
        {
            var z = -1;
            var SomethingChanged = false;


            do
            {
                z += 1;
                if (z > Item.AllRelations.Count - 1) { break; }

                if (!Item.AllRelations[z].IsOk())
                {
                    Item.AllRelations.Remove(Item.AllRelations[z]);
                    z = -1;
                    SomethingChanged = true;
                }
            } while (true);

            return SomethingChanged;
        }

        /// <summary>
        /// Ermittelt die anzahl der Beziehungen, die nicht korrekt sind.
        /// </summary>
        /// <param name="Strongmode"></param>
        /// <returns></returns>
        public int NotPerforming(bool Strongmode)
        {

            var f = 0;

            foreach (var ThisRelation in Item.AllRelations)
            {
                if (!ThisRelation.Performs(Strongmode)) { f += 1; }
            }

            return f;
        }


        public int LowestOrder(ListExt<PointDF> ThisPoints)
        {
            var l = int.MaxValue;

            foreach (var Thispouint in ThisPoints)
            {
                l = Math.Min(l, Thispouint.Order);
            }

            return l;
        }




        public List<PointDF> ConnectsWith(PointDF Point, bool CheckX, bool IgnoreInternals)
        {

            var Points = new List<PointDF>();
            Points.Add(Point);

            var Ist = -1;

            // Nur, wenn eine Beziehung gut ist, kann man mit sicherheit sagen, daß das zusammenhängt. Deswegen auch ein Performs test

            do
            {
                Ist += 1;
                if (Ist >= Points.Count) { break; }


                foreach (var ThisRelation in Item.AllRelations)
                {
                    if (ThisRelation != null && ThisRelation.Points.Contains(Points[Ist]) && ThisRelation.Performs(false) && ThisRelation.Connects(CheckX))
                    {


                        if (!IgnoreInternals || !ThisRelation.IsInternal())
                        {
                            Points.AddIfNotExists(ThisRelation.Points);
                        }
                    }
                }
            } while (true);



            return Points;
        }



        public PointDF GetPointWithLowerIndex(PointDF NotPoint, PointDF ErsatzFür, bool MustUsableForAutoRelation)
        {
            if (NotPoint != null && NotPoint.Parent == ErsatzFür.Parent) { return ErsatzFür; }

            var p = ErsatzFür;

            foreach (var thispoint in Item.AllPoints)
            {
                if (thispoint != null)
                {
                    if (!MustUsableForAutoRelation || thispoint.CanUsedForAutoRelation)
                    {
                        if (thispoint != NotPoint && thispoint != ErsatzFür)
                        {
                            if (Math.Abs(thispoint.X - ErsatzFür.X) < 0.01m && Math.Abs(thispoint.Y - ErsatzFür.Y) < 0.01m)
                            {
                                if (thispoint.Order < p.Order) { p = thispoint; }
                            }
                        }
                    }
                }
            }

            return p;
        }


        public void ShowErweitertMenü(BasicPadItem Item)
        {
            var l = Item.GetStyleOptions(this, System.EventArgs.Empty);

            if (l.Count == 0)
            {
                MessageBox.Show("Objekt hat keine<br>Bearbeitungsmöglichkeiten.", enImageCode.Information);
                return;
            }


            var tg = EditBoxFlexiControl.Show(l);


            if (tg.Count == 0) { return; }

            var ClosMe = true;

            Item.DoStyleCommands(this, tg, ref ClosMe);



            Item.RecomputePointAndRelations();


            if (!ClosMe) { ShowErweitertMenü(Item); }



        }


        public static void GenerateLayoutFromRow(object sender, GenerateLayoutInternalEventargs e)
        {

            if (e.Handled) { return; }
            e.Handled = true;

            if (!e.DirectPrint && !e.DirectSave)
            {
                Develop.DebugPrint_NichtImplementiert();
                //Dim x As New PictureView(Row.Database.Layouts(LayoutNr), Row.CellFirst().String)
                //x.Area_Dateisystem.Visible = False
                //x.Pad.ResetVariables()
                //x.Pad.ParseVariableAndSpecialCodes(Row)
                //x.Pad.ShowInPrintMode = True
                //'       x.ZoomIn.Checked = True
                //x.Pad.ZoomFit()
                //x.Show()
                //x.Pad.RepairAll(1, True)
                //x.BringToFront()
            }
            else
            {
                PadForCreation.GenerateFromRow(e.LayoutID, e.Row, false);
                if (e.DirectSave)
                {
                    PadForCreation.SaveAsBitmap(e.Row.CellFirstString(), e.OptionalFilename);
                }
                if (e.DirectPrint)
                {
                    PadForCreation.Print();
                }
            }
        }


        public static void RenameColumnInLayout(object sender, RenameColumnInLayoutEventArgs e)
        {
            if (e.Handled) { return; }
            e.Handled = true;
            var Padx = new CreativePad(); // TODO: Creative-Pad unabhängig eines Controls erstellen.
            Padx.ParseData(e.LayoutCode, false, string.Empty);
            Padx.RenameColumn(e.OldName, e.Column);
            e.LayoutCode = Padx.DataToString();
            Padx.Dispose();
        }

        private void DruckerDokument_BeginPrint(object sender, PrintEventArgs e)
        {
            OnBeginnPrint(e);
        }

        private void DruckerDokument_EndPrint(object sender, PrintEventArgs e)
        {
            OnEndPrint(e);
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
