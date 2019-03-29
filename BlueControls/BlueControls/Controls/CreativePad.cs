#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2019 Christian Peter
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
using BlueControls.DialogBoxes;
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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using static BlueBasics.FileOperations;

namespace BlueControls.Controls
{
    [DefaultEvent("Click")]
    public sealed partial class CreativePad : IBackgroundBitmap, IContextMenu
    {

        private bool ComputeOrders_isin;
        private int RepairAll_OldItemc = -9999;
        private bool RepairPrinterData_Prepaired;




        public static bool Debug_ShowPointOrder = false;
        public static bool Debug_ShowRelationOrder = false;

        internal static readonly Pen PenGray = new Pen(Color.FromArgb(40, 0, 0, 0));
        internal static readonly Pen PenGrayLarge = new Pen(Color.FromArgb(40, 0, 0, 0), 5);


        private Bitmap _BitmapOfControl;
        private bool _GeneratingBitmapOfControl;

        private decimal _Zoom = 1;
        private decimal _ZoomMinx = 1;

        private bool _Fitting = true;

        private bool _isParsing;


        private SizeF _SheetSizeInMM = SizeF.Empty;
        private System.Windows.Forms.Padding _RandinMM = System.Windows.Forms.Padding.Empty;


        private PointDF _MouseDownx;

        public string Caption = "";


        private BasicPadItem _GivesMouseComandsTo;

        private bool _KeyboardEditEnabled = true;
        private bool _MouseEditEnabled = true;
        private bool _KontextMenuEnabled = true;

        private Bitmap DummyBMP;
        private Graphics DummyGR;



        public readonly int DPI = 300;


        private PointDF P_LO;
        private PointDF P_LU;
        private PointDF P_RU;
        private PointDF P_RO;



        private PointDF P_rLO;
        private PointDF P_rLU;
        private PointDF P_rRU;
        private PointDF P_rRO;

        private enAutoRelationMode _AutoRelation = enAutoRelationMode.Alle_Erhalten;
        private bool _ShowInPrintMode;

        /// <summary>
        /// Objektübergreifende Beziehungen
        /// </summary>
        private readonly List<clsPointRelation> _ExternalRelations = new List<clsPointRelation>();

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



        private RowItem _SheetStyle;
        private decimal _SheetStyleScale = 1.0m;



        private bool _Grid;
        private float _GridShow = 10;
        private float _Gridsnap = 1;


        private bool _OrdersValid;



        private static readonly CreativePad PadForCreation = new CreativePad();

        #region  Events 
        public event EventHandler<ContextMenuInitEventArgs> ContextMenuInit;
        public event EventHandler<ContextMenuItemClickedEventArgs> ContextMenuItemClicked;

        public event EventHandler Parsed;

        public event EventHandler NeedRefresh;

        public event PrintPageEventHandler PrintPage;
        #endregion



        #region  Properties 

        [DefaultValue(1.0)]
        public decimal SheetStyleScale
        {
            get
            {
                return _SheetStyleScale;
            }
            set
            {

                if (value < 0.1m) { value = 0.1m; }

                if (!_isParsing && _SheetStyleScale == value) { return; }

                _SheetStyleScale = value;


                Item.SheetStyle = _SheetStyle;
                Item.SheetStyleScale = _SheetStyleScale;
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

        [DefaultValue(true)]
        public bool KeyboardEditEnabled
        {
            get
            {
                return _KeyboardEditEnabled;
            }
            set
            {
                _KeyboardEditEnabled = value;
                EigenschaftenDurchschleifen();
            }
        }

        [DefaultValue(true)]
        public bool MouseEditEnabled
        {
            get
            {
                return _MouseEditEnabled;
            }
            set
            {
                _MouseEditEnabled = value;
                EigenschaftenDurchschleifen();
            }
        }
        [DefaultValue(true)]
        public bool KontextMenuEnabled
        {
            get
            {
                return _KontextMenuEnabled;
            }
            set
            {
                _KontextMenuEnabled = value;
                EigenschaftenDurchschleifen();
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
                //  _AutoSnap = value


                if (Item != null)
                {
                    foreach (var thisItem in Item)
                    {
                        if (thisItem is ChildPadItem cpi)
                        {
                            cpi.PadInternal.AutoRelation = value;
                        }
                    }
                }


            }
        }

        [DefaultValue(false)]
        public bool Changed { get; set; }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ItemCollectionPad Item { get; }

        public SizeF SheetSizeInMM
        {
            get
            {
                return _SheetSizeInMM;
            }
            set
            {
                if (value == _SheetSizeInMM) { return; }

                _SheetSizeInMM = new SizeF(value.Width, value.Height);

                GenPoints();


            }
        }

        public System.Windows.Forms.Padding RandinMM
        {
            get
            {
                return _RandinMM;
            }
            set
            {
                _RandinMM = new System.Windows.Forms.Padding(Math.Max(0, value.Left), Math.Max(0, value.Top), Math.Max(0, value.Right), Math.Max(0, value.Bottom));
                GenPoints();
            }
        }

        internal new void MouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            _MouseDownx = null;


            var Ho = HotItem(e);

            if (Ho is IMouseAndKeyHandle ho2)
            {
                if (ho2.MouseDown(this, e, _Zoom, (decimal)SliderX.Value, (decimal)SliderY.Value))
                {
                    _GivesMouseComandsTo = Ho;
                    return;
                }
            }


            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                var p = MousePos11(e);
                foreach (var thisPoint in Sel_P)
                {

                    if (GeometryDF.Länge(thisPoint, new PointDF(p)) < 5m / _Zoom)
                    {
                        var Relations = AllRelations();
                        if (!thisPoint.CanMove(Relations))
                        {
                            Invalidate();
                            DialogBoxes.QuickInfo.Show("Dieser Punkt ist fest definiert<br>und kann nicht verschoben werden.");
                            return;
                        }

                        Unselect();
                        Sel_P.Add(thisPoint);
                        _OrdersValid = false;
                        ComputeMovingData();
                        _MouseDownx = new PointDF(e.X / _Zoom, e.Y / _Zoom);
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
                    Sel_P.AddIfNotExists(Ho.PointList());
                    _OrdersValid = false;
                    ComputeMovingData();
                    Invalidate();
                }

            }

            _MouseDownx = new PointDF(e.X / _Zoom, e.Y / _Zoom);
        }

        internal new void MouseMove(System.Windows.Forms.MouseEventArgs e)
        {

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
                    if (!((IMouseAndKeyHandle)_GivesMouseComandsTo).MouseMove(this, e, _Zoom, (decimal)SliderX.Value, (decimal)SliderY.Value))
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
                    if (Ho2.MouseMove(this, e, _Zoom, (decimal)SliderX.Value, (decimal)SliderY.Value))
                    {
                        _GivesMouseComandsTo = ho;
                        Invalidate();
                        return;
                    }
                }
            }

            if (_MouseDownx != null && e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (_MouseEditEnabled)
                {
                    MoveItems(new PointDF(e.X / _Zoom, e.Y / _Zoom));
                }
            }
        }

        internal void DoMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            if (_GivesMouseComandsTo != null)
            {
                if (!((IMouseAndKeyHandle)_GivesMouseComandsTo).MouseUp(this, e, _Zoom, (decimal)SliderX.Value, (decimal)SliderY.Value))
                {
                    _GivesMouseComandsTo = null;
                }
                else
                {
                    return;
                }
            }


            _MouseDownx = null;


            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                //   AmRasterAusrichten()

                foreach (var Thispoint in Sel_P)
                {
                    if (Thispoint.Parent is BasicPadItem item)
                    {
                        Sel_P.AddIfNotExists(item.PointList());
                        _OrdersValid = false;
                        Invalidate();
                        break;
                    }
                }

                if (Convert.ToBoolean(_AutoRelation | enAutoRelationMode.NurBeziehungenErhalten) && _NewAutoRelations.Count > 0)
                {
                    _ExternalRelations.AddRange(_NewAutoRelations);
                    InvalidateOrder();
                    RepairAll(0, false);
                }

            }

            _NewAutoRelations.Clear();

            RepairAll(1, false);

            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                if (_KontextMenuEnabled)
                {
                    ContextMenu_Show(this, e);
                }
            }


            ComputeMovingData();

            Invalidate(); // Damit auch snap-Punkte wieder gelöscht werden

        }


        private PointDF Getbetterpoint(double X, double Y, PointDF notPoint, bool MustUsableForAutoRelation)
        {
            var _Points = AllPoints();

            foreach (var thispoint in _Points)
            {

                if (thispoint != null)
                {

                    if (!MustUsableForAutoRelation || thispoint.CanUsedForAutoRelation)
                    {

                        if (thispoint != notPoint)
                        {
                            if (Math.Abs((double)thispoint.X - X) < 0.01 && Math.Abs((double)thispoint.Y - Y) < 0.01) { return GetPointWithLowerIndex(_Points, notPoint, thispoint, true); }
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
                if (clsSkin.StyleDB == null) { clsSkin.InitStyles(); }
                if (clsSkin.StyleDB == null) { return string.Empty; }
                if (_SheetStyle == null) { return string.Empty; }

                return _SheetStyle.CellFirstString();


            }
            set
            {

                if (!_isParsing && value == SheetStyle) { return; }

                if (clsSkin.StyleDB == null) { clsSkin.InitStyles(); }


                _SheetStyle = clsSkin.StyleDB.Row[value];
                if (_SheetStyle == null) { _SheetStyle = clsSkin.StyleDB.Row.First(); }// Einfach die Erste nehmen


                Item.SheetStyle = _SheetStyle;
                Item.SheetStyleScale = _SheetStyleScale;

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

            Item.Changed += _Item_Changed;
            Item.ItemRemoved += _Item_ItemRemoved;
            Item.ItemAdded += _Item_ItemAdded;

            SetDoubleBuffering();
            Initialize();
            _MouseHighlight = false;
            GenPoints();


        }

        private void _Item_Changed(object sender, System.EventArgs e)
        {
            Changed = true;
            Invalidate();
        }

        public void InvalidateOrder()
        {
            _OrdersValid = false;
        }



        public List<clsPointRelation> AllRelations()
        {

            var R = new List<clsPointRelation>();
            foreach (var thisItem in Item)
            {
                if (thisItem != null)
                {
                    R.AddRange(thisItem.RelationList());
                }
            }
            R.AddIfNotExists(_ExternalRelations);
            return R;
        }

        public List<PointDF> AllPoints()
        {


            var P = new List<PointDF>();


            if (P_LO != null)
            {
                P.AddIfNotExists(P_LO);
                P.AddIfNotExists(P_LU);
                P.AddIfNotExists(P_RU);
                P.AddIfNotExists(P_RO);

                P.AddIfNotExists(P_rLO);
                P.AddIfNotExists(P_rLU);
                P.AddIfNotExists(P_rRU);
                P.AddIfNotExists(P_rRO);
            }



            foreach (var thisItem in Item)
            {
                if (thisItem != null)
                {
                    P.AddRange(thisItem.PointList());
                }
            }

            foreach (var ThisRelation in _ExternalRelations)
            {
                P.AddIfNotExists(ThisRelation.Points);
            }
            return P;

        }



        internal Rectangle DruckbereichRect()
        {
            if (P_rLO == null) { return new Rectangle(0, 0, 0, 0); }
            return new Rectangle((int)P_rLO.X, (int)P_rLO.Y, (int)(P_rRU.X - P_rLO.X), (int)(P_rRU.Y - P_rLO.Y));
        }



        internal RectangleDF MaxBounds()
        {

            RectangleDF r;
            if (Item == null || Item.Count == 0)
            {
                r = new RectangleDF(0, 0, 0, 0);
            }
            else
            {
                r = Item.MaximumBounds();
            }







            if (_SheetSizeInMM.Width > 0 && _SheetSizeInMM.Height > 0)
            {

                var X1 = Math.Min(r.Left, 0);
                var y1 = Math.Min(r.Top, 0);


                var x2 = Math.Max(r.Right, modConverter.mmToPixel((decimal)_SheetSizeInMM.Width, DPI));
                var y2 = Math.Max(r.Bottom, modConverter.mmToPixel((decimal)_SheetSizeInMM.Height, DPI));

                return new RectangleDF(X1, y1, x2 - X1, y2 - y1);
            }

            return r;


        }


        public Bitmap ToBitmap(decimal Scale)
        {
            var r = MaxBounds();
            if (r.Width == 0) { return null; }

            //  Dim MieseQuali As Boolean = False

            do
            {
                if ((int)(r.Width * Scale) > 10000)
                {
                    Scale = Scale * 0.5m;
                    //  MieseQuali = True
                }
                else if ((int)(r.Height * Scale) > 10000)
                {
                    Scale = Scale * 0.5m;
                    //  MieseQuali = True
                }
                else if ((int)(r.Height * Scale) * (int)(r.Height * Scale) > 60000000)
                {
                    Scale = Scale * 0.5m;
                    //         MieseQuali = True

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

                Draw(gr, Scale, r.Left * Scale, r.Top * Scale, true, true);

                //       If MieseQuali Then gr.DrawString("Qualität musste herabgesetzt werden!", New Font("Arial", 100), Brushes.Red, 0, 0)
            }


            return I;
        }

        private void ZoomFitInvalidateAndCheckButtons(bool SliderShowing)
        {


            var r = MaxBounds();

            if (r.Width < 0.01m || r.Height < 0.01m)
            {
                _Zoom = 1;
                _ZoomMinx = _Zoom;
                ComputeSliders(SliderShowing);
                return;
            }



            if (SliderShowing)
            {
                _ZoomMinx = Math.Min((Width - SliderY.Width - 32) / r.Width, (Height - SliderX.Height - 32) / r.Height);
            }
            else
            {
                _ZoomMinx = Math.Min(Width / r.Width, Height / r.Height);
            }



            _ZoomMinx = Math.Max(_ZoomMinx, 0.01m);


            if (_Fitting && !MousePressing())
            {
                _Zoom = _ZoomMinx;
            }


            _Zoom = Math.Max(_Zoom, _ZoomMinx / 5);



            ComputeSliders(SliderShowing);


        }

        public void ZoomIn(System.Windows.Forms.MouseEventArgs e)
        {
            var x = new System.Windows.Forms.MouseEventArgs(e.Button, e.Clicks, e.X, e.Y, 1);
            OnMouseWheel(x);
        }

        public void ZoomOut(System.Windows.Forms.MouseEventArgs e)
        {
            var x = new System.Windows.Forms.MouseEventArgs(e.Button, e.Clicks, e.X, e.Y, -1);
            OnMouseWheel(x);
        }

        public void ZoomFit()
        {
            _Fitting = true;
            ZoomFitInvalidateAndCheckButtons(true);
        }


        public void ZoomFitWithoutSliders()
        {
            _Fitting = true;
            //SliderX.Value = 0
            //SliderY.Value = 0
            //SliderX.Visible = False
            //SliderY.Visible = False
            ZoomFitInvalidateAndCheckButtons(false);
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


        public new void KeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            if (!_KeyboardEditEnabled) { return; }

            if (_GivesMouseComandsTo != null)
            {
                if (((IMouseAndKeyHandle)_GivesMouseComandsTo).KeyUp(this, e, _Zoom, (decimal)SliderX.Value, (decimal)SliderY.Value)) { return; }
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

        protected override void OnKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            base.OnKeyUp(e);
            KeyUp(e);
        }


        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseDown(e);

            MouseDown(e);


        }


        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseMove(e);
            MouseMove(e);

        }


        private PointF MousePos11(System.Windows.Forms.MouseEventArgs e)
        {
            return new PointF((float)((decimal)(e.X + SliderX.Value) / _Zoom), (float)((decimal)(e.Y + SliderY.Value) / _Zoom));
        }


        protected override void OnMouseWheel(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            _Fitting = false;

            var m = MousePos11(e);

            if (e.Delta > 0)
            {
                _Zoom = _Zoom * 1.5m;
            }
            else
            {
                _Zoom = _Zoom * (1m / 1.5m);
            }




            _Zoom = Math.Max(_ZoomMinx / 1.2m, _Zoom);
            _Zoom = Math.Min(10, _Zoom);

            ComputeSliders(true);


            // M Beeinhaltet den Punkt, wo die Maus hinzeigt Maßstabunabhängig.

            // Der Slider ist abhängig vom Maßsstab - sowie die echten Mauskoordinaten ebenfalls.

            // Deswegen die M mit dem neuen Zoom-Faktor berechnen umrechen, um auch Masstababhängig zu sein
            // Die Verschiebung der echten Mauskoordinaten berechnen und den Slider auf den Wert setzen.



            SliderX.Value = m.X * (float)_Zoom - e.X;
            SliderY.Value = m.Y * (float)_Zoom - e.Y;

            // Alte Berechnung für Mittig Setzen
            //SliderX.Value = (m.X * _Zoom) - (Width / 2) - SliderY.Width
            //SliderY.Value = (m.Y * _Zoom) - (Height / 2) - SliderX.Height



        }

        private void Draw(Graphics GR, decimal cZoom, decimal MoveX, decimal MoveY, bool Drawall, bool ForPrinting)
        {

            if (_SheetStyle == null || _SheetStyleScale < 0.1m) { return; }

            ComputeOrders();

            //   DoRepairIfNeeded


            foreach (var thisItem in Item)
            {
                if (thisItem != null)
                {
                    if (Drawall)
                    {
                        thisItem.Draw(GR, cZoom, MoveX, MoveY, 0, Size.Empty, ForPrinting);
                    }
                    else
                    {
                        thisItem.Draw(GR, cZoom, MoveX, MoveY, 0, Size, ForPrinting);
                    }
                }
            }


        }

        protected override void InitializeSkin()
        {

        }


        internal void DrawCreativePadToBitmap(Graphics TMPGR, enStates vState)
        {

            var X = (decimal)SliderX.Value;
            var Y = (decimal)SliderY.Value;

            if (_SheetSizeInMM.Width > 0 && _SheetSizeInMM.Height > 0)
            {
                Skin.Draw_Back(TMPGR, enDesign.Table_And_Pad, vState, DisplayRectangle, this, true);
                var tx = (int)(P_LO.X * _Zoom - X);
                var ty = (int)(P_LO.Y * _Zoom - Y);
                var tx2 = (int)(P_RU.X * _Zoom - X);
                var ty2 = (int)(P_RU.Y * _Zoom - Y);

                var R = new Rectangle(tx, ty, tx2 - tx, ty2 - ty);
                TMPGR.FillRectangle(Brushes.White, R);
                TMPGR.DrawRectangle(PenGray, R);

                var rtx = (int)(P_rLO.X * _Zoom - X);
                var rty = (int)(P_rLO.Y * _Zoom - Y);
                var rtx2 = (int)(P_rRU.X * _Zoom - X);
                var rty2 = (int)(P_rRU.Y * _Zoom - Y);
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



            Draw(TMPGR, _Zoom, X, Y, false, _ShowInPrintMode);


            var Relations = AllRelations();

            // Erst Beziehungen, weil die die Grauen Punkte zeichnet
            foreach (var ThisRelation in Relations)
            {
                ThisRelation.Draw(TMPGR, _Zoom, X, Y, Relations.IndexOf(ThisRelation));
            }


            if (Debug_ShowPointOrder)
            {
                var _Points = AllPoints();
                // Alle Punkte mit Order anzeigen
                foreach (var ThisPoint in _Points)
                {
                    ThisPoint.Draw(TMPGR, _Zoom, X, Y, enDesign.Button_EckpunktSchieber_Phantom, enStates.Standard);
                }
            }






            //     If _ItemsEditable Then

            // Dann die selectiereren Punkte

            foreach (var ThisPoint in Sel_P)
            {
                if (ThisPoint.CanMove(Relations))
                {
                    ThisPoint.Draw(TMPGR, _Zoom, X, Y, enDesign.Button_EckpunktSchieber, enStates.Standard);
                }
                else
                {
                    ThisPoint.Draw(TMPGR, _Zoom, X, Y, enDesign.Button_EckpunktSchieber_Phantom, enStates.Standard);
                }
            }


            foreach (var ThisRelation in _NewAutoRelations)
            {


                var P1 = ThisRelation.Points[0].ZoomAndMove(_Zoom, X, Y);
                var P2 = ThisRelation.Points[1].ZoomAndMove(_Zoom, X, Y);

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



            if (_GivesMouseComandsTo != null)
            {
                var DCoordinates = _GivesMouseComandsTo.DrawingKoordinates(_Zoom, X, Y);

                TMPGR.DrawRectangle(new Pen(Brushes.Red, 3), DCoordinates);
            }

        }



        protected override void DrawControl(Graphics gr, enStates state)
        {
            if (_BitmapOfControl == null)
            {
                _BitmapOfControl = new Bitmap(ClientSize.Width, ClientSize.Height, PixelFormat.Format32bppPArgb);
            }

            var TMPGR = Graphics.FromImage(_BitmapOfControl);

            DrawCreativePadToBitmap(gr, state);

            TMPGR.Dispose();


            gr.DrawImage(_BitmapOfControl, 0, 0);


            Skin.Draw_Border(gr, enDesign.Table_And_Pad, state, DisplayRectangle);

        }


        public Bitmap BitmapOfControl()
        {
            if (_GeneratingBitmapOfControl) { return null; }
            _GeneratingBitmapOfControl = true;
            if (_BitmapOfControl == null) { Refresh(); }
            _GeneratingBitmapOfControl = false;
            return _BitmapOfControl;
        }

        private void _Item_ItemRemoved(object sender, System.EventArgs e)
        {
            Unselect();
            InvalidateOrder();
            Changed = true;
            ZoomFitInvalidateAndCheckButtons(true);
            Invalidate();
        }

        private void _Item_ItemAdded(object sender, ListEventArgs e)
        {
            EigenschaftenDurchschleifen();
            Changed = true;
            InvalidateOrder();
        }


        protected override void OnSizeChanged(System.EventArgs e)
        {
            if (_BitmapOfControl != null)
            {
                if (_BitmapOfControl.Width < Width || _BitmapOfControl.Height < Height)
                {
                    _BitmapOfControl.Dispose();
                    _BitmapOfControl = null;
                }
            }

            ZoomFitInvalidateAndCheckButtons(true);
            base.OnSizeChanged(e);
        }

        private void SliderX_ValueChanged(object sender, System.EventArgs e)
        {
            Invalidate();
        }

        private void SliderY_ValueChanged(object sender, System.EventArgs e)
        {
            Invalidate();
        }

        private void ComputeSliders(bool SliderShowing)
        {

            var r = MaxBounds();

            if (r.Width == 0) { return; }

            var w = 0; //= CInt((Me.Width - SliderY.Width) - r.Width * _Zoom)
            var h = 0; //= CInt((Me.Height - SliderX.Height) - r.Height * _Zoom)

            if (SliderShowing)
            {
                w = (int)(Width - SliderY.Width - r.Width * _Zoom);
                h = (int)(Height - SliderX.Height - r.Height * _Zoom);
            }
            else
            {
                w = (int)(Width - r.Width * _Zoom);
                h = (int)(Height - r.Height * _Zoom);
            }


            if (w < 0)
            {
                SliderX.Enabled = true;
                SliderX.Minimum = (double)(r.Left * _Zoom - Width * 0.6m);
                SliderX.Maximum = (double)(r.Right * _Zoom - Width + Width * 0.6m);

            }
            else
            {
                SliderX.Enabled = false;

                if (MousePressing() == false)
                {

                    SliderX.Minimum = (double)(r.Left * _Zoom - w / 2m);
                    SliderX.Maximum = (double)(r.Left * _Zoom - w / 2m);
                    SliderX.Value = (double)(r.Left * _Zoom - w / 2m);
                }
            }

            if (h < 0)
            {
                SliderY.Enabled = true;
                SliderY.Minimum = (double)(r.Top * _Zoom - Height * 0.6m);
                SliderY.Maximum = (double)(r.Bottom * _Zoom - Height + Height * 0.6m);

            }
            else
            {
                SliderY.Enabled = false;
                if (MousePressing() == false)
                {
                    SliderY.Minimum = (double)(r.Top * _Zoom - h / 2m);
                    SliderY.Maximum = (double)(r.Top * _Zoom - h / 2m);
                    SliderY.Value = (double)(r.Top * _Zoom - h / 2m);
                }
            }

            if (SliderShowing) { Invalidate(); }
        }


        private bool MoveSelectedPoints(decimal X, decimal Y)
        {


            Item.RecomputePointAndRelations();
            var Relations = AllRelations();
            var _Points = AllPoints();


            var f = NotPerformingx(Relations, false);

            foreach (var thispoint in _Points)
            {
                thispoint?.Store();
            }

            foreach (var thispoint in _Points)
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



            var f2 = NotPerformingx(Relations, false);

            if (f2 > f)
            {
                foreach (var thispoint in _Points)
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

            var Relations = AllRelations();

            foreach (var thispoint in Sel_P)
            {
                Move_X.AddIfNotExists(ConnectsWith(thispoint, Relations, true, false));
                Move_Y.AddIfNotExists(ConnectsWith(thispoint, Relations, false, false));
            }

            foreach (var thispoint in Move_X)
            {
                if (!thispoint.CanMoveX(Relations))
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
                if (!thispoint.CanMoveY(Relations))
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


        public bool PerformAll(ItemCollectionPad ThisItemCol, int Level, bool AllowBigChanges)
        {
            // Level 0 gibt es nicht
            // Level 1 = Normal / Reparier nur die neuen Sachen
            // Level 2 = Leicht / Reparier nur die neuen Sachen mit schnelleren Abbruchbedingungen

            var ThisRelations = AllRelations();
            //var ThisPoints = AllPoints();


            var L = new List<string>();
            var Methode = 0;
            ComputeOrders();

            do
            {
                var tmp = "";

                foreach (var ThisRelation in ThisRelations)
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
                    Relations_Optimizex();
                    ThisItemCol.RecomputePointAndRelations();
                    ComputeOrders();
                    L.Clear();
                }
                else
                {
                    L.Add(tmp);
                }

                foreach (var ThisRelation in ThisRelations)
                {
                    if (!ThisRelation.Computed)
                    {
                        ThisRelation.Repair(LowestOrder(ThisRelation.Points), AllowBigChanges);
                    }
                }

            } while (true);

        }



        private void ComputeOrders()
        {

            if (_OrdersValid) { return; }

            if (ComputeOrders_isin) { return; }
            ComputeOrders_isin = true;

            //Points_DeleteInvalid();
            Relations_DeleteInvalid();

            ComputePointOrder();
            ComputeRelationOrderx();

            _OrdersValid = true;

            ComputeOrders_isin = false;
        }


        private void ComputePointOrder()
        {
            var Modus = 0;
            var Count = 1;


            var _Points = AllPoints();

            var Relations = AllRelations();





            foreach (var Thispoint in _Points)
            {
                Thispoint.Order = int.MaxValue;
            }

            _Points.Sort();

            var RelationNone = new List<clsPointRelation>();
            var RelationY = new List<clsPointRelation>();
            var RelationX = new List<clsPointRelation>();
            var RelationXY = new List<clsPointRelation>();


            foreach (var thisRelation in Relations)
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








        private void MoveItems(PointDF MouseMovedTo)
        {
            if (_MouseDownx == null) { return; }

            PointDF PMoveX = null;
            PointDF PMoveY = null;
            PointDF PSnapToX = null;
            PointDF PSnapToY = null;

            _NewAutoRelations.Clear();

            var MoveX = MouseMovedTo.X - _MouseDownx.X;
            var MoveY = MouseMovedTo.Y - _MouseDownx.Y;



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
                var Relations = AllRelations();
                if (NotPerformingx(Relations, false) == 0)
                {
                    AddAllAutoRelations(PMoveX, PSnapToX, PMoveY, PSnapToY);
                }
                else
                {
                    _NewAutoRelations.Clear();
                }
            }

            _MouseDownx = new PointDF(_MouseDownx.X + MoveX, _MouseDownx.Y + MoveY);
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


            var sX = (decimal)SliderX.Value;
            var sY = (decimal)SliderY.Value;

            var dr = new Rectangle(0, 0, Width - SliderY.Width, Height - SliderX.Height);


            var WillMoveTo = new PointDF(PointToTest.X + MouseMovedTo.X - _MouseDownx.X, PointToTest.Y + MouseMovedTo.Y - _MouseDownx.Y);

            var _Points = AllPoints();

            foreach (var ThisPoint in _Points)
            {
                if (ThisPoint != null && ThisPoint.CanUsedForAutoRelation && ThisPoint.IsOnScreen(_Zoom, sX, sY, dr))
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




        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseUp(e);
            DoMouseUp(e);


        }


        private void ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e)
        {
            FloatingInputBoxListBoxStyle.Close(this);
            BasicPadItem thisItem = null;

            if (e.Tag is BasicPadItem item) { thisItem = item; }
            if (e.ClickedComand.Internal().ToLower() == "abbruch") { return; }
            Changed = true;

            var Done = false;

            if (thisItem != null)
            {
                switch (e.ClickedComand.Internal().ToLower())
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
                }



            }

            if (!Done) { OnContextMenuItemClicked(new ContextMenuItemClickedEventArgs(thisItem, e.ClickedComand)); }



            InvalidateOrder();

            RepairAll(0, false); // Da könnte sich ja alles geändert haben...
            Invalidate();

        }

        private void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e)
        {
            ContextMenuItemClicked?.Invoke(this, e);
        }

        public List<BasicPadItem> HotItems(System.Windows.Forms.MouseEventArgs e)
        {


            var P = new Point((int)((decimal)(e.X + SliderX.Value) / _Zoom), (int)((decimal)(e.Y + SliderY.Value) / _Zoom));

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


        public void ContextMenu_Show(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var ThisContextMenu = new ItemCollectionList(enBlueListBoxAppearance.KontextMenu);
            var _HotItem = HotItem(e);
            var UserMenu = new ItemCollectionList(enBlueListBoxAppearance.KontextMenu);

            OnContextMenuInit(new ContextMenuInitEventArgs(_HotItem, UserMenu));



            if (UserMenu.Count > 0)
            {
                ThisContextMenu.Add(new TextListItem(true, "Allgemein"));
                ThisContextMenu.AddRange(UserMenu);
            }



            if (_HotItem != null)
            {
                ThisContextMenu.Add(new TextListItem(true, "Allgemeine Element-Aktionen"));
                ThisContextMenu.Add(new TextListItem("#Erweitert", "Objekt bearbeiten", enImageCode.Stift));
                ThisContextMenu.Add(new LineListItem());


                ThisContextMenu.Add(new TextListItem("#ExterneBeziehungen", "Objektübergreifende Punkt-Beziehungen aufheben", enImageCode.Kreuz));
                if (_HotItem.PrintMe)
                {
                    ThisContextMenu.Add(new TextListItem("#PrintMeNot", "Objekt nicht drucken", QuickImage.Get("Drucker|16||1")));
                }
                else
                {
                    ThisContextMenu.Add(new TextListItem("#PrintMe", "Objekt drucken", enImageCode.Drucker));
                }


                ThisContextMenu.Add(new TextListItem("#Duplicate", "Objekt duplizieren", enImageCode.Kopieren, _HotItem is ICloneable));


                ThisContextMenu.Add(new LineListItem());
                ThisContextMenu.Add(new TextListItem("#Vordergrund", "In den Vordergrund", enImageCode.InDenVordergrund));
                ThisContextMenu.Add(new TextListItem("#Hintergrund", "In den Hintergrund", enImageCode.InDenHintergrund));
                ThisContextMenu.Add(new TextListItem("#Vorne", "Eine Ebene nach vorne", enImageCode.EbeneNachVorne));
                ThisContextMenu.Add(new TextListItem("#Hinten", "Eine Ebene nach hinten", enImageCode.EbeneNachHinten));
            }


            var _Points = AllPoints();
            foreach (var Thispoint in _Points)
            {
                Thispoint?.Store();
            }



            if (ThisContextMenu.Count > 0)
            {
                ThisContextMenu.Add(new LineListItem());
                ThisContextMenu.Add(enContextMenuComands.Abbruch);
                var _ContextMenu = FloatingInputBoxListBoxStyle.Show(ThisContextMenu, _HotItem, this);
                _ContextMenu.ItemClicked += ContextMenuItemClickedInternalProcessig;
            }
        }

        private void OnContextMenuInit(ContextMenuInitEventArgs e)
        {
            ContextMenuInit?.Invoke(this, e);
        }

        public void GenPoints()
        {


            if (Math.Abs(_SheetSizeInMM.Width) < 0.001 || Math.Abs(_SheetSizeInMM.Height) < 0.001)
            {
                if (P_LO != null)
                {
                    P_LO = null;
                    P_RO = null;
                    P_RU = null;
                    P_LU = null;

                    P_rLO = null;
                    P_rRO = null;
                    P_rRU = null;
                    P_rLU = null;


                }

                return;
            }


            if (P_LO == null)
            {
                P_LO = new PointDF(this, "Rand LO", 0, 0, true);
                P_LU = new PointDF(this, "Rand LU", 0, 0, true);
                P_RU = new PointDF(this, "Rand RU", 0, 0, true);
                P_RO = new PointDF(this, "Rand RO", 0, 0, true);

                P_rLO = new PointDF(this, "Druckbereich LO", 0, 0, true);
                P_rRO = new PointDF(this, "Druckbereich RO", 0, 0, true);
                P_rRU = new PointDF(this, "Druckbereich RU", 0, 0, true);
                P_rLU = new PointDF(this, "Druckbereich LU", 0, 0, true);

            }

            var SSW = Math.Round(modConverter.mmToPixel((decimal)_SheetSizeInMM.Width, DPI), 1);
            var SSH = Math.Round(modConverter.mmToPixel((decimal)_SheetSizeInMM.Height, DPI), 1);
            var rr = Math.Round(modConverter.mmToPixel(_RandinMM.Right, DPI), 1);
            var rl = Math.Round(modConverter.mmToPixel(_RandinMM.Left, DPI), 1);
            var ro = Math.Round(modConverter.mmToPixel(_RandinMM.Top, DPI), 1);
            var ru = Math.Round(modConverter.mmToPixel(_RandinMM.Bottom, DPI), 1);

            P_LO.SetTo(0, 0);
            P_RO.SetTo(SSW, 0);
            P_RU.SetTo(SSW, SSH);
            P_LU.SetTo(0, SSH);

            P_rLO.SetTo(rl, ro);
            P_rRO.SetTo(SSW - rr, ro);
            P_rRU.SetTo(SSW - rr, SSH - ru);
            P_rLU.SetTo(rl, SSH - ru);




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
            var Relations = AllRelations();

            foreach (var thisRelation in Relations)
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
            var Relations = AllRelations();

            foreach (var thisRelation in Relations)
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
            _ExternalRelations.Add(new clsPointRelation(enRelationType, Point1, Point2));
            InvalidateOrder();
        }



        private void RelationDeleteExternal(BasicPadItem ThisItem)
        {

            foreach (var ThisRelation in _ExternalRelations)
            {
                if (ThisRelation != null)
                {

                    foreach (var Thispoint in ThisRelation.Points)
                    {

                        if (Thispoint.Parent is BasicPadItem tItem)
                        {
                            if (tItem == ThisItem)
                            {
                                _ExternalRelations.Remove(ThisRelation);
                                RelationDeleteExternal(ThisItem); // 'Rekursiv
                                return;
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

            if (!string.IsNullOrEmpty(Caption)) { t = t + "Caption=" + Caption.ToNonCritical() + ", "; }

            if (_SheetStyle != null) { t = t + "Style=" + _SheetStyle.CellFirstString().ToNonCritical() + ", "; }

            if (_SheetStyleScale < 0.1m) { _SheetStyleScale = 1.0m; }

            if (Math.Abs(_SheetStyleScale - 1) > 0.001m) { t = t + "FontScale=" + _SheetStyleScale + ", "; }

            if (_SheetSizeInMM.Width > 0 && _SheetSizeInMM.Height > 0)
            {
                t = t + "SheetSize=" + _SheetSizeInMM + ", ";
                t = t + "PrintArea=" + _RandinMM + ", ";
            }

            //  _AutoSort = False
            t = t + "Items=" + Item.ToString() + ", ";


            t = t + "Grid=" + _Grid + ", ";
            t = t + "GridShow=" + _GridShow + ", ";
            t = t + "GridSnap=" + _Gridsnap + ", ";

            //Dim One As Boolean

            foreach (var ThisRelation in _ExternalRelations)
            {
                if (ThisRelation != null) { t = t + "Relation=" + ThisRelation + ", "; }
            }


            return t.TrimEnd(", ") + "}";

        }

        public void ParseData(string Value, bool NeedPrinterData)
        {

            Initialize();
            if (string.IsNullOrEmpty(Value) || Value.Length < 3) { return; }
            if (Value.Substring(0, 1) != "{") { return; }// Alte Daten gehen eben verloren.

            _isParsing = true;
            var Beg = 0;

            do
            {
                Beg += 1;
                if (Beg > Value.Length) { break; }
                var T = Value.ParseTag(Beg);
                var PairValuexxx = Value.ParseValue(T, Beg);

                Beg = Beg + T.Length + PairValuexxx.Length + 2;
                switch (T)
                {
                    case "sheetsize":
                        _SheetSizeInMM = Extensions.SizeFParse(PairValuexxx);
                        GenPoints();
                        break;
                    case "printarea":
                        _RandinMM = Extensions.PaddingParse(PairValuexxx);
                        GenPoints();
                        break;
                    case "items":
                        Item.Parse(PairValuexxx);

                        break;
                    case "relation":
                        _ExternalRelations.Add(new clsPointRelation(PairValuexxx, AllPoints()));
                        InvalidateOrder();

                        break;
                    case "caption":
                        Caption = PairValuexxx.FromNonCritical();

                        break;
                    case "style":

                        SheetStyle = PairValuexxx;

                        break;
                    case "fontscale":
                        SheetStyleScale = decimal.Parse(PairValuexxx);

                        break;
                    case "grid":
                        _Grid = PairValuexxx.FromPlusMinus();

                        break;
                    case "gridshow":
                        _GridShow = float.Parse(PairValuexxx);
                        break;
                    case "gridsnap":
                        _Gridsnap = float.Parse(PairValuexxx);


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

            if (NeedPrinterData) { RepairPrinterData(); }

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

            _SheetSizeInMM = Size.Empty;
            _RandinMM = System.Windows.Forms.Padding.Empty;


            Caption = "";

            _NewAutoRelations.Clear();
            _ExternalRelations.Clear();


            if (clsSkin.StyleDB == null) { clsSkin.InitStyles(); }
            _SheetStyle = null;
            _SheetStyleScale = 1.0m;

            if (clsSkin.StyleDB != null) { _SheetStyle = clsSkin.StyleDB.Row.First(); }

            Item.SheetStyle = _SheetStyle;
            Item.SheetStyleScale = _SheetStyleScale;


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




        public bool RepairAll(int Levelx, bool AllowBigChanges)
        {
            // Level 0 = Hart / Reparier alles mit Gewalt
            // Level 1 = Normal / Reparier nur die neuen Sachen
            // Level 2 = Leicht / Reparier nur die neuen Sachen mit schnelleren Abbruchbedingungen

            var Itemc = 0;


            if (Item == null)
            {
                Itemc = -2;
            }
            else
            {
                Itemc = Item.Count - 1;
            }

            if (Levelx == 0)
            {
                RepairAll_OldItemc = Itemc + 1; // Löst eine Kettenreaktion aus
                Item.RecomputePointAndRelations();
                InvalidateOrder();
            }


            if (Itemc != RepairAll_OldItemc)
            {
                DrawCreativePadToBitmap(DummyGraphics(), enStates.Standard);
                modAllgemein.CollectGarbage();
            }

            RepairAll_OldItemc = Itemc;


            return PerformAll(Item, Levelx, AllowBigChanges);
        }


        private Graphics DummyGraphics()
        {

            if (DummyGR == null)
            {
                DummyBMP = new Bitmap(1, 1);
                DummyGR = Graphics.FromImage(DummyBMP);
            }
            return DummyGR;

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


        private void PrintDocument1_PrintPage(object sender, PrintPageEventArgs e)
        {
            e.HasMorePages = false;

            OnPrintPage(e);



            //   e.HasMorePages = False
            var i = ToBitmap(2).Resize(e.PageBounds.Width, e.PageBounds.Height, enSizeModes.Breite_oder_Höhe_Anpassen_MitVergrößern, InterpolationMode.HighQualityBicubic, true);
            if (i == null) { return; }
            e.Graphics.DrawImage(i, (int)((e.PageBounds.Width - i.Width) / 2.0), (int)((e.PageBounds.Height - i.Height) / 2.0), i.Width, i.Height);
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
                SheetSizeInMM = new SizeF((int)(DruckerDokument.DefaultPageSettings.PaperSize.Height * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.PaperSize.Width * 25.4 / 100));

                RandinMM = new System.Windows.Forms.Padding((int)(DruckerDokument.DefaultPageSettings.Margins.Left * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.Margins.Top * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.Margins.Right * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.Margins.Bottom * 25.4 / 100));


            }
            else
            {
                // Hochformat
                SheetSizeInMM = new SizeF((int)(DruckerDokument.DefaultPageSettings.PaperSize.Width * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.PaperSize.Height * 25.4 / 100));

                RandinMM = new System.Windows.Forms.Padding((int)(DruckerDokument.DefaultPageSettings.Margins.Left * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.Margins.Top * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.Margins.Right * 25.4 / 100), (int)(DruckerDokument.DefaultPageSettings.Margins.Bottom * 25.4 / 100));



            }




            GenPoints();


        }


        public void ShowWorkingAreaSetup()
        {

            var OriD = new PrintDocument();


            OriD.DefaultPageSettings.Landscape = false;
            OriD.DefaultPageSettings.PaperSize = new PaperSize("Benutzerdefiniert", (int)(SheetSizeInMM.Width / 25.4 * 100), (int)(SheetSizeInMM.Height / 25.4 * 100));
            OriD.DefaultPageSettings.Margins.Top = (int)(RandinMM.Top / 25.4 * 100);
            OriD.DefaultPageSettings.Margins.Bottom = (int)(RandinMM.Bottom / 25.4 * 100);
            OriD.DefaultPageSettings.Margins.Left = (int)(RandinMM.Left / 25.4 * 100);
            OriD.DefaultPageSettings.Margins.Right = (int)(RandinMM.Right / 25.4 * 100);




            using (var x = new BlueControls.Forms.PageSetupDialog(OriD, true))
            {
                x.ShowDialog();
                if (x.Canceled()) { return; }
            }


            SheetSizeInMM = new SizeF((int)(OriD.DefaultPageSettings.PaperSize.Width * 25.4 / 100), (int)(OriD.DefaultPageSettings.PaperSize.Height * 25.4 / 100));

            RandinMM = new System.Windows.Forms.Padding((int)(OriD.DefaultPageSettings.Margins.Left * 25.4 / 100), (int)(OriD.DefaultPageSettings.Margins.Top * 25.4 / 100), (int)(OriD.DefaultPageSettings.Margins.Right * 25.4 / 100), (int)(OriD.DefaultPageSettings.Margins.Bottom * 25.4 / 100));

            GenPoints();
        }



        private void RepairPrinterData()
        {


            if (RepairPrinterData_Prepaired)
            {
                return;
            }

            RepairPrinterData_Prepaired = true;

            DruckerDokument.DocumentName = Caption;

            var done = false;
            foreach (PaperSize ps in DruckerDokument.PrinterSettings.PaperSizes)
            {
                if (ps.Width == (int)(SheetSizeInMM.Width / 25.4 * 100) && ps.Height == (int)(SheetSizeInMM.Height / 25.4 * 100))
                {
                    done = true;
                    DruckerDokument.DefaultPageSettings.PaperSize = ps;
                    break;
                }
            }

            if (!done)
            {
                DruckerDokument.DefaultPageSettings.PaperSize = new PaperSize("Custom", (int)(SheetSizeInMM.Width / 25.4 * 100), (int)(SheetSizeInMM.Height / 25.4 * 100));
            }

            DruckerDokument.OriginAtMargins = true;
            DruckerDokument.DefaultPageSettings.Margins = new Margins((int)(RandinMM.Left / 25.4 * 100), (int)(RandinMM.Right / 25.4 * 100), (int)(RandinMM.Top / 25.4 * 100), (int)(RandinMM.Bottom / 25.4 * 100));
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
            OnNeedRefresh();

        }


        private void OnNeedRefresh()
        {
            NeedRefresh?.Invoke(this, System.EventArgs.Empty); // Invalidate-Befehl weitergeben an untergeordnete Steuerelemente
        }

        public void SetZoom(decimal Zoom)
        {
            _Zoom = Zoom;

            SliderX.Minimum = 0;
            SliderX.Maximum = 0;
            SliderX.Value = 0;

            SliderY.Minimum = 0;
            SliderY.Maximum = 0;
            SliderY.Value = 0;

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

                    var f = Row.CellBestFile(Column);

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



        public void GenerateFromRow(int LayoutNr, RowItem Row, bool NeedPrinterData)
        {

            ParseData(Row.Database.Layouts[LayoutNr], NeedPrinterData);
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
        public void ComputeRelationOrderx()
        {
            var Count = 0;

            // Zurücksetzen ---- 
            var Relations = AllRelations();
            foreach (var ThisRelation in Relations)
            {
                ThisRelation.Order = -1;
            }


            for (var Durch = 0; Durch <= 1; Durch++)
            {

                do
                {
                    clsPointRelation NextRel = null;
                    var RelPO = int.MaxValue;

                    foreach (var ThisRelation in Relations)
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

            Relations.Sort();
        }

        public void Relations_Optimizex()
        {
            var Relations = AllRelations();
            if (NotPerformingx(Relations, true) > 0) { return; }


            var Cb = new List<PointDF>();
            var DobR = new List<clsPointRelation>();

            var _Points = AllPoints();

            foreach (var thisPoint in _Points)
            {
                var CX = ConnectsWith(thisPoint, Relations, true, true);
                var CY = ConnectsWith(thisPoint, Relations, false, true);

                // Ermitteln, die auf X und Y miteinander verbunden sind
                Cb.Clear();
                foreach (var thisPoint2 in CX)
                {
                    if (CY.Contains(thisPoint2)) { Cb.Add(thisPoint2); }
                }


                if (Cb.Count > 1)
                {

                    DobR.Clear();
                    foreach (var ThisRelation in Relations)
                    {


                        // Wenn Punkte nicht direct verbunden sind, aber trotzdem Fix zueinander, die Beziehung optimieren
                        if (ThisRelation.RelationType == enRelationType.WaagerechtSenkrecht && !ThisRelation.IsInternal())
                        {
                            if (Cb.Contains(ThisRelation.Points[0]) && Cb.Contains(ThisRelation.Points[1]))
                            {
                                ThisRelation.RelationType = enRelationType.PositionZueinander;
                                ThisRelation.InitRelationData(false);
                                InvalidateOrder();
                                Relations_Optimizex();
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
                                            Relations.Remove(R1);
                                            InvalidateOrder();
                                            Relations_Optimizex();
                                            return;
                                        }

                                        if (R1.Points[0].Parent == R2.Points[1].Parent && R1.Points[1].Parent == R2.Points[0].Parent)
                                        {
                                            Relations.Remove(R1);
                                            InvalidateOrder();
                                            Relations_Optimizex();
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
            foreach (var r1 in Relations)
            {
                if (!r1.IsInternal())
                {
                    foreach (var r2 in Relations)
                    {
                        if (!r1.SinngemäßIdenitisch(r2) && !r2.IsInternal())
                        {
                            if (r1.SinngemäßIdenitisch(r2))
                            {
                                Relations.Remove(r2);
                                InvalidateOrder();
                                Relations_Optimizex();
                                return;

                            }

                            if (r1.UsesSamePoints(r2))
                            {
                                switch (r1.RelationType)
                                {
                                    case enRelationType.PositionZueinander:
                                        // Beziehungen mit gleichen punkten, aber einer mächtigen PositionZueinander -> andere löschen
                                        Relations.Remove(r2);
                                        InvalidateOrder();
                                        Relations_Optimizex();
                                        return;
                                    case enRelationType.WaagerechtSenkrecht when r2.RelationType == enRelationType.WaagerechtSenkrecht && r1.Richtmaß() != r2.Richtmaß():
                                        // Beziehungen mit gleichen punkten, aber spearat mix X und Y -> PositionZueinander konvertieren 
                                        r1.RelationType = enRelationType.PositionZueinander;
                                        r1.InitRelationData(false);
                                        Relations.Remove(r2);
                                        InvalidateOrder();
                                        Relations_Optimizex();
                                        return;
                                }
                            }
                        }
                    }
                }
            }

        }



        /// <summary>
        /// Es wird davon ausgegangen, dass alle Punkte gültig sind.
        /// </summary>
        /// <returns></returns>
        public bool Relations_DeleteInvalid()
        {
            var z = -1;
            var SomethingChanged = false;


            do
            {
                z += 1;
                if (z > _ExternalRelations.Count - 1) { break; }
                var RemoveMe = false;


                foreach (var Thispoint in _ExternalRelations[z].Points)
                {
                    if (Thispoint == null)
                    {
                        RemoveMe = true;
                    }
                    else
                    {
                        if (Thispoint.Parent == null)
                        {
                            RemoveMe = true;
                        }
                        else
                        {
                            if (Thispoint.Parent is BasicPadItem item)
                            {
                                if (!Item.Contains(item)) { RemoveMe = true; }
                            }
                        }
                    }


                    if (!RemoveMe)
                    {
                        if (_ExternalRelations[z].Points.Count != 2 || _ExternalRelations[z].Points[0] == _ExternalRelations[z].Points[1]) { RemoveMe = true; }
                    }
                }

                if (RemoveMe)
                {
                    _ExternalRelations.Remove(_ExternalRelations[z]);
                    z = -1;
                    SomethingChanged = true;
                }
            } while (true);

            return SomethingChanged;
        }

        public int NotPerformingx(List<clsPointRelation> ThisRelations, bool Strongmode)
        {

            var f = 0;

            foreach (var ThisRelation in ThisRelations)
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




        public List<PointDF> ConnectsWith(PointDF Point, List<clsPointRelation> ThisRelations, bool CheckX, bool IgnoreInternals)
        {

            var Points = new List<PointDF>();
            Points.Add(Point);

            var Ist = -1;

            // Nur, wenn eine Beziehung gut ist, kann man mit sicherheit sagen, daß das zusammenhängt. Deswegen auch ein Performs test

            do
            {
                Ist += 1;
                if (Ist >= Points.Count) { break; }


                foreach (var ThisRelation in ThisRelations)
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



        public PointDF GetPointWithLowerIndex(List<PointDF> ThisPoints, PointDF NotPoint, PointDF ErsatzFür, bool MustUsableForAutoRelation)
        {
            if (NotPoint != null && NotPoint.Parent == ErsatzFür.Parent) { return ErsatzFür; }

            var p = ErsatzFür;

            foreach (var thispoint in ThisPoints)
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

        public void EigenschaftenDurchschleifen()
        {

            foreach (var ThisItem in Item)
            {

                if (ThisItem is ChildPadItem tempVar)
                {

                    tempVar.PadInternal.KeyboardEditEnabled = _KeyboardEditEnabled;
                    tempVar.PadInternal.MouseEditEnabled = _MouseEditEnabled;
                    tempVar.PadInternal.KontextMenuEnabled = _KontextMenuEnabled;
                }
            }
        }

        public static void GenerateLayoutFromRow(RowItem Row, int LayoutNr, bool DirectPrint, bool DirectSave, string OptionalFilename)
        {



            if (!DirectPrint && !DirectSave)
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
                PadForCreation.GenerateFromRow(LayoutNr, Row, false);
                if (DirectSave)
                {
                    PadForCreation.SaveAsBitmap(Row.CellFirstString(), OptionalFilename);
                }
                if (DirectPrint)
                {
                    PadForCreation.Print();
                }
            }
        }


        public static string RenameColumnInLayout(Database database, string LayoutCode, string OldName, ColumnItem Column)
        {
            var Padx = new CreativePad(); // TODO: Creative-Pad unabhängig eines Controls erstellen.
            Padx.ParseData(LayoutCode, false);
            Padx.RenameColumn(OldName, Column);
            var R = Padx.DataToString();
            Padx.Dispose();
            return R;
        }






    }
}
