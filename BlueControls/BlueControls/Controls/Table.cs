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
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Forms;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Threading;
using static BlueBasics.FileOperations;
using BlueControls.Designer_Support;

namespace BlueControls.Controls
{
    [Designer(typeof(BasicDesigner))]
    [DefaultEvent("CursorPosChanged")]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class Table : GenericControl, IContextMenu, IBackgroundNone
    {

        public Table()
        {

            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            SetDoubleBuffering();
            _MouseHighlight = false;
        }


        /// <summary>
        ///  Wird DatabaseAdded gehandlet?
        /// </summary>
        private static bool ServiceStarted = false;

        #region  Variablen 

        private Database _Database;


        private ColumnItem _CursorPosColumn;
        private RowItem _CursorPosRow;
        private ColumnItem _MouseOverColumn;
        private RowItem _MouseOverRow;


        private AutoFilter _AutoFilter;
        private SearchAndReplace _searchAndReplace;

        private int? _HeadSize = null;
        private int WiederHolungsSpaltenWidth;
        private int _ArrangementNr = 1;
        private RowSortDefinition _sortDefinitionTemporary;

        private enBlueTableAppearance _Design = enBlueTableAppearance.Standard;
        private List<RowItem> _SortedRows; // Die Sortierung der Zeile
        private List<RowItem> _SortedRowsBefore = new List<RowItem>(); // Die Sortierung der Zeile

        private readonly object Lock_UserAction = new object();
        private BlueFont _Column_Font;
        private BlueFont _Chapter_Font;
        private BlueFont _Cell_Font;
        private BlueFont _NewRow_Font;
        private int Pix16 = 16;
        private int Pix18 = 18;

        private Rectangle tmpCursorRect = Rectangle.Empty;

        private FontSelectDialog _FDia = null;

        private bool _ShowNumber = false;

        private string _StoredView = string.Empty;


        private bool ISIN_MouseDown;
        private bool ISIN_MouseWheel;
        private bool ISIN_Click;
        private bool ISIN_SizeChanged;
        private bool ISIN_KeyDown;
        private bool ISIN_MouseMove;
        private bool ISIN_MouseUp;
        private bool ISIN_DoubleClick;
        private bool ISIN_MouseLeave;
        private bool ISIN_MouseEnter;
        private bool ISIN_Resize;
        private bool ISIN_VisibleChanged;

        private const int RowCaptionFontY = 26;
        private const int RowCaptionSizeY = 50;
        private const int ColumnCaptionSizeY = 22;


        private Progressbar PG = null;

        #endregion


        #region  Events 



        public event EventHandler<RowEventArgs> RowAdded;

        public event EventHandler<ContextMenuInitEventArgs> ContextMenuInit;

        public event EventHandler<ContextMenuItemClickedEventArgs> ContextMenuItemClicked;

        public new event EventHandler<CellDoubleClickEventArgs> DoubleClick;

        public event EventHandler<RowCancelEventArgs> EditBeforeNewRow;

        public event EventHandler<CellCancelEventArgs> EditBeforeBeginEdit;

        public event EventHandler<BeforeNewValueEventArgs> EditBeforeNewValueSet;

        public event EventHandler<CellEventArgs> CellValueChanged;

        public event EventHandler<CellEventArgs> CursorPosChanged;

        public event EventHandler ColumnArrangementChanged;

        public event EventHandler ViewChanged;

        internal static void StartDatabaseService()
        {
            if (ServiceStarted) { return; }
            ServiceStarted = true;


            Database.MultiUserFileAdded += Database_DatabaseAdded;

        }

        private static void Database_DatabaseAdded(object sender, MultiUserFileGiveBackEventArgs e)
        {

            if (e.File is Database DB)
            {
                DB.NeedPassword += Database_NeedPassword;
                DB.GenerateLayoutInternal += DB_GenerateLayoutInternal;
                DB.RenameColumnInLayout += DB_RenameColumnInLayout;
                DB.Loaded += tabAdministration.CheckDatabase;
            }
        }






        public event EventHandler RowsSorted;

        public event EventHandler<FilterEventArgs> AutoFilterClicked;

        public event EventHandler DatabaseChanged;


        #endregion

        #region  Properties 

        public FilterCollection Filter { get; private set; }


        // <Obsolete("Database darf nicht im Designer gesetzt werden.", True)>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Database Database
        {
            get
            {
                return _Database;
            }
            set
            {
                if (_Database == value) { return; }

                //OnDatabaseChanging();

                CloseAllComponents();

                _MouseOverColumn = null;
                _MouseOverRow = null;
                _CursorPosColumn = null;
                _CursorPosRow = null;

                if (_Database != null)
                {
                    // auch Disposed Datenbanken die Bezüge entfernen!

                    _Database.Cell.CellValueChanged -= _Database_CellValueChanged;
                    _Database.ConnectedControlsStopAllWorking -= _Database_StopAllWorking;
                    _Database.Loaded -= _Database_DatabaseLoaded;
                    _Database.Loading -= _Database_StoreView;
                    _Database.ViewChanged -= _Database_ViewChanged;
                    _Database.RowKeyChanged -= _Database_RowKeyChanged;
                    _Database.ColumnKeyChanged -= _Database_ColumnKeyChanged;
                    _Database.Column.ItemInternalChanged -= _Database_ColumnContentChanged;
                    _Database.SortParameterChanged -= _Database_SortParameterChanged;
                    _Database.Row.RowRemoved -= _Database_RowCountChanged;
                    _Database.Row.RowAdded -= _Database_Row_RowAdded;
                    _Database.Column.ItemRemoved -= _Database_ViewChanged;
                    _Database.Column.ItemAdded -= _Database_ViewChanged;
                    _Database.SavedToDisk -= _Database_SavedToDisk;
                    _Database.ColumnArrangements.ItemInternalChanged -= ColumnArrangements_ItemInternalChanged;
                    _Database.ProgressbarInfo -= _Database_ProgressbarInfo;

                    _Database.Save(false);         // Datenbank nicht reseten, weil sie ja anderweitig noch benutzt werden kann

                }
                _Database = value;
                InitializeSkin();

                if (_Database != null)
                {
                    _Database.Cell.CellValueChanged += _Database_CellValueChanged;
                    _Database.ConnectedControlsStopAllWorking += _Database_StopAllWorking;
                    _Database.Loaded += _Database_DatabaseLoaded;
                    _Database.Loading += _Database_StoreView;
                    _Database.ViewChanged += _Database_ViewChanged;
                    _Database.RowKeyChanged += _Database_RowKeyChanged;
                    _Database.ColumnKeyChanged += _Database_ColumnKeyChanged;
                    _Database.Column.ItemInternalChanged += _Database_ColumnContentChanged;
                    _Database.SortParameterChanged += _Database_SortParameterChanged;
                    _Database.Row.RowRemoved += _Database_RowCountChanged;
                    _Database.Row.RowAdded += _Database_Row_RowAdded;
                    _Database.Column.ItemAdded += _Database_ViewChanged;
                    _Database.Column.ItemRemoved += _Database_ViewChanged;
                    _Database.SavedToDisk += _Database_SavedToDisk;
                    _Database.ColumnArrangements.ItemInternalChanged += ColumnArrangements_ItemInternalChanged;
                    _Database.ProgressbarInfo += _Database_ProgressbarInfo;
                }

                _Database_DatabaseLoaded(this, new LoadedEventArgs(false));

                OnDatabaseChanged();

            }
        }

        private void _Database_Row_RowAdded(object sender, RowEventArgs e)
        {

            OnRowAdded(sender, e);
            _Database_RowCountChanged(sender, e);

        }






        //  <Obsolete("Database darf nicht im Designer gesetzt werden.", True)>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RowSortDefinition SortDefinitionTemporary
        {
            get
            {
                return _sortDefinitionTemporary;
            }
            set
            {
                if (_sortDefinitionTemporary != null && value != null && _sortDefinitionTemporary.ToString() == value.ToString()) { return; }
                if (_sortDefinitionTemporary == value) { return; }
                _sortDefinitionTemporary = value;

                _Database_SortParameterChanged(this, System.EventArgs.Empty);

            }
        }


        [DefaultValue(1.0f)]
        public double FontScale
        {
            get
            {
                if (_Database == null) { return 1f; }
                return _Database.GlobalScale;
            }
        }



        [DefaultValue(false)]
        public bool ShowNumber
        {
            get
            {
                return _ShowNumber;
            }
            set
            {

                if (value == _ShowNumber) { return; }
                CloseAllComponents();
                _ShowNumber = value;
                Invalidate();
            }
        }


        [DefaultValue(enBlueTableAppearance.Standard)]
        public enBlueTableAppearance Design
        {
            get
            {
                return _Design;
            }
            set
            {
                SliderY.Visible = true;
                SliderX.Visible = Convert.ToBoolean(value == enBlueTableAppearance.Standard);

                if (value == _Design) { return; }

                CloseAllComponents();
                _Design = value;

                if (!SliderX.Visible)
                {
                    SliderX.Minimum = 0;
                    SliderX.Maximum = 0;
                    SliderX.Value = 0;
                }

                Invalidate_HeadSize();
                Invalidate_AllDraw(false);
                Invalidate_RowSort(); // Neue Zeilen evtl.
                Invalidate();
                OnViewChanged();

            }
        }

        #endregion


        #region  Draw 

        private void DrawWhite(Graphics GR)
        {
            if (SliderX != null) { SliderX.Enabled = false; }
            if (SliderY != null) { SliderY.Enabled = false; }
            Skin.Draw_Back(GR, enDesign.Table_And_Pad, enStates.Standard_Disabled, DisplayRectangle, this, true);
            Skin.Draw_Border(GR, enDesign.Table_And_Pad, enStates.Standard_Disabled, DisplayRectangle);
        }





        protected override void InitializeSkin()
        {
            _Cell_Font = Skin.GetBlueFont(enDesign.Table_Cell, enStates.Standard).Scale(FontScale);
            _Column_Font = Skin.GetBlueFont(enDesign.Table_Column, enStates.Standard).Scale(FontScale);
            _Chapter_Font = Skin.GetBlueFont(enDesign.Table_Cell_Chapter, enStates.Standard).Scale(FontScale);
            _NewRow_Font = Skin.GetBlueFont(enDesign.Table_Cell_New, enStates.Standard).Scale(FontScale);
            if (Database != null)
            {
                Pix16 = GetPix(16, _Cell_Font, Database.GlobalScale);
                Pix18 = GetPix(18, _Cell_Font, Database.GlobalScale);

            }
            else
            {
                Pix16 = 16;
                Pix18 = 18;
            }

        }

        private static int GetPix(int Pix, BlueFont F, double Scale)
        {
            return Skin.FormatedText_NeededSize("@|", null, F, (int)(Pix * Scale + 0.5)).Height;
        }

        protected override void DrawControl(Graphics gr, enStates state)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => DrawControl(gr, state)));
                return;
            }

            // Listboxen bekommen keinen Focus, also Tabellen auch nicht. Basta.
            if (Convert.ToBoolean(state & enStates.Standard_HasFocus))
            {
                state = state ^ enStates.Standard_HasFocus;
            }

            if (_Database == null || DesignMode)
            {
                DrawWhite(gr);
                return;
            }


            lock (Lock_UserAction)
            {
                //if (_InvalidExternal) { FillExternalControls(); }
                if (Convert.ToBoolean(state & enStates.Standard_Disabled)) { CursorPos_Set(null, null, false); }

                var DisplayRectangleWOSlider = DisplayRectangleWithoutSlider();


                // Haupt-Aufbau-Routine ------------------------------------
                gr.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                if (!ComputeAllCellPositions())
                {
                    DrawWhite(gr);
                    return;
                }




                var FirstVisibleRow = SortedRows().Count;
                var LastVisibleRow = -1;
                foreach (var thisRow in SortedRows())
                {
                    if (IsOnScreen(thisRow, DisplayRectangleWOSlider))
                    {
                        var T = SortedRows().IndexOf(thisRow);
                        FirstVisibleRow = Math.Min(T, FirstVisibleRow);
                        LastVisibleRow = Math.Max(T, LastVisibleRow);
                    }
                }


                switch (_Design)
                {
                    case enBlueTableAppearance.Standard:
                        Draw_Table_Std(gr, state, DisplayRectangleWOSlider, FirstVisibleRow, LastVisibleRow);

                        break;
                    case enBlueTableAppearance.OnlyMainColumnWithoutHead:
                        Draw_Table_ListboxStyle(gr, state, DisplayRectangleWOSlider, FirstVisibleRow, LastVisibleRow);
                        break;
                    default:
                        Develop.DebugPrint(_Design);
                        break;
                }
            }
        }

        private void Draw_Table_What(Graphics GR, enTableDrawColumn col, enTableDrawType type, int PermaX, Rectangle DisplayRectangleWOSlider, int FirstVisibleRow, int LastVisibleRow)
        {

            var lfdno = 0;

            foreach (var ViewItem in _Database.ColumnArrangements[_ArrangementNr])
            {


                if (ViewItem != null && ViewItem.Column != null)
                {


                    if (IsOnScreen(ViewItem, DisplayRectangleWOSlider))
                    {

                        if ((col == enTableDrawColumn.NonPermament && ViewItem.ViewType != enViewType.PermanentColumn && (int)ViewItem.OrderTMP_Spalte_X1 + (int)ViewItem._TMP_DrawWidth > PermaX) ||
                            (col == enTableDrawColumn.Permament && ViewItem.ViewType == enViewType.PermanentColumn))
                        {
                            lfdno += 1;
                            switch (type)
                            {
                                case enTableDrawType.ColumnBackBody:
                                    Draw_Column_Body(GR, ViewItem, DisplayRectangleWOSlider);
                                    break;

                                case enTableDrawType.Cells:
                                    Draw_Column_Cells(GR, ViewItem, DisplayRectangleWOSlider, FirstVisibleRow, LastVisibleRow, lfdno);
                                    break;

                                case enTableDrawType.ColumnHead:
                                    Draw_Column_Head(GR, ViewItem, DisplayRectangleWOSlider, lfdno);
                                    break;

                            }
                        }
                    }
                }
            }
        }

        private void Draw_Column_Cells(Graphics GR, ColumnViewItem ViewItem, Rectangle DisplayRectangleWOSlider, int FirstVisibleRow, int LastVisibleRow, int lfdno)
        {


            // Die Cursorposition ermittleln
            if (!Thread.CurrentThread.IsBackground && _CursorPosColumn != null && _CursorPosRow != null && ViewItem.Column == _CursorPosColumn)
            {
                if (IsOnScreen(_CursorPosColumn, _CursorPosRow, DisplayRectangleWOSlider))
                {
                    tmpCursorRect = new Rectangle((int)ViewItem.OrderTMP_Spalte_X1, (int)_CursorPosRow.TMP_Y + 1, Column_DrawWidth(ViewItem, DisplayRectangleWOSlider), Row_DrawHeight(_CursorPosRow, DisplayRectangleWOSlider) - 1);
                    Draw_Cursor(GR, DisplayRectangleWOSlider, false);
                }
            }



            //  Neue Zeile 
            if (UserEdit_NewRowAllowed() && ViewItem == _Database.ColumnArrangements[_ArrangementNr][Database.Column[0]])
            {
                Skin.Draw_FormatedText(GR, "[Neue Zeile]", QuickImage.Get(enImageCode.PlusZeichen, Pix16), enAlignment.Left, new Rectangle((int)ViewItem.OrderTMP_Spalte_X1 + 1, (int)(-SliderY.Value + HeadSize() + 1), (int)ViewItem._TMP_DrawWidth - 2, 16 - 2), this, false, _NewRow_Font, Translate);
            }


            var Drawn = string.Empty;


            // Zeilen Zeichnen (Alle Zellen)
            for (var Zei = FirstVisibleRow; Zei <= LastVisibleRow; Zei++)
            {
                var CurrentRow = SortedRows()[Zei];
                var y = (int)CurrentRow.TMP_Y;
                GR.SmoothingMode = SmoothingMode.None;

                if (ViewItem.Column.ZellenZusammenfassen)
                {
                    var ToDraw = Database.Cell.GetStringBehindLinkedValue(ViewItem.Column, CurrentRow);

                    if (Drawn != ToDraw)
                    {

                        if (y - 2 < HeadSize() && Zei < SortedRows().Count)
                        {
                            if (Database.Cell.GetStringBehindLinkedValue(ViewItem.Column, SortedRows()[Zei + 1]) == ToDraw)
                            {
                                y = HeadSize();
                            }
                        }

                        // Zeilenlinie Zeichnen
                        GR.DrawLine(Skin.Pen_LinieDünn, (int)ViewItem.OrderTMP_Spalte_X1, y, (int)ViewItem.OrderTMP_Spalte_X1 + Column_DrawWidth(ViewItem, DisplayRectangleWOSlider) - 1, y);
                        // Zelleninhalt Zeichnen
                        Draw_CellTransparent(GR, ViewItem, CurrentRow, y, DisplayRectangleWOSlider, _Cell_Font);
                        Drawn = ToDraw;
                    }
                }
                else
                {
                    // Zeilenlinie Zeichnen
                    GR.DrawLine(Skin.Pen_LinieDünn, (int)ViewItem.OrderTMP_Spalte_X1, y, (int)ViewItem.OrderTMP_Spalte_X1 + Column_DrawWidth(ViewItem, DisplayRectangleWOSlider) - 1, y);
                    // Zelleninhalt Zeichnen
                    Draw_CellTransparent(GR, ViewItem, CurrentRow, y, DisplayRectangleWOSlider, _Cell_Font);
                }


                if (lfdno == 1)
                {
                    // Überschrift zeichnen
                    if (!string.IsNullOrEmpty(CurrentRow.TMP_Chapter))
                    {
                        var si = GR.MeasureString(CurrentRow.TMP_Chapter, _Chapter_Font.Font());
                        GR.FillRectangle(new SolidBrush(Skin.Color_Back(enDesign.Table_And_Pad, enStates.Standard).SetAlpha(50)), 1, (int)CurrentRow.TMP_Y - RowCaptionSizeY, DisplayRectangleWOSlider.Width - 2, RowCaptionSizeY);

                        GR.FillRectangle(new SolidBrush(Skin.Color_Back(enDesign.Table_And_Pad, enStates.Standard).SetAlpha(200)), 1, (int)CurrentRow.TMP_Y - RowCaptionFontY, si.Width, si.Height);
                        GR.DrawString(CurrentRow.TMP_Chapter, _Chapter_Font.Font(), _Chapter_Font.Brush_Color_Main, 0, (int)CurrentRow.TMP_Y - RowCaptionFontY);
                        GR.DrawLine(Skin.Pen_LinieDick, 0, (int)CurrentRow.TMP_Y, DisplayRectangleWOSlider.Width, (int)CurrentRow.TMP_Y);

                    }
                }
            }


        }

        private void SliderSchalten(Rectangle DisplayR, int MaxX, int MaxY)
        {

            SliderY.Minimum = 0;
            SliderY.Maximum = Math.Max(MaxY - DisplayR.Height + 1 + HeadSize(), 0);
            SliderY.LargeChange = DisplayR.Height - HeadSize();
            SliderY.Enabled = Convert.ToBoolean(SliderY.Maximum > 0);



            SliderX.Minimum = 0;
            SliderX.Maximum = MaxX - DisplayR.Width + 1;
            SliderX.LargeChange = DisplayR.Width;
            SliderX.Enabled = Convert.ToBoolean(SliderX.Maximum > 0);

        }

        private void Draw_Table_Std(Graphics GR, enStates State, Rectangle DisplayRectangleWOSlider, int FirstVisibleRow, int LastVisibleRow)
        {

            try
            {
                tmpCursorRect = Rectangle.Empty;

                if (_Database.ColumnArrangements == null || _ArrangementNr >= _Database.ColumnArrangements.Count) { return; }   // Kommt vor, dass spontan doch geparsed wird...

                Skin.Draw_Back(GR, enDesign.Table_And_Pad, State, DisplayRectangle, this, true);


                /// Maximale Rechten Pixel der Permanenten Columns ermitteln
                var PermaX = 0;
                foreach (var ViewItem in _Database.ColumnArrangements[_ArrangementNr])
                {
                    if (ViewItem != null && ViewItem.Column != null && ViewItem.ViewType == enViewType.PermanentColumn)
                    {

                        if (ViewItem._TMP_DrawWidth == null)
                        {
                            // Veränderte Werte!
                            DrawControl(GR, State);
                            return;
                        }

                        PermaX = Math.Max(PermaX, (int)ViewItem.OrderTMP_Spalte_X1 + (int)ViewItem._TMP_DrawWidth);
                    }
                }


                Draw_Table_What(GR, enTableDrawColumn.NonPermament, enTableDrawType.ColumnBackBody, PermaX, DisplayRectangleWOSlider, FirstVisibleRow, LastVisibleRow);
                Draw_Table_What(GR, enTableDrawColumn.NonPermament, enTableDrawType.Cells, PermaX, DisplayRectangleWOSlider, FirstVisibleRow, LastVisibleRow);


                Draw_Table_What(GR, enTableDrawColumn.Permament, enTableDrawType.ColumnBackBody, PermaX, DisplayRectangleWOSlider, FirstVisibleRow, LastVisibleRow);
                Draw_Table_What(GR, enTableDrawColumn.Permament, enTableDrawType.Cells, PermaX, DisplayRectangleWOSlider, FirstVisibleRow, LastVisibleRow);

                // Den CursorLines zeichnen
                Draw_Cursor(GR, DisplayRectangleWOSlider, true);

                Draw_Table_What(GR, enTableDrawColumn.NonPermament, enTableDrawType.ColumnHead, PermaX, DisplayRectangleWOSlider, FirstVisibleRow, LastVisibleRow);
                Draw_Table_What(GR, enTableDrawColumn.Permament, enTableDrawType.ColumnHead, PermaX, DisplayRectangleWOSlider, FirstVisibleRow, LastVisibleRow);




                /// Überschriften 1-3 Zeichnen
                Draw_Column_Head_Captions(GR);






                Skin.Draw_Border(GR, enDesign.Table_And_Pad, State, DisplayRectangleWOSlider);


                if (Database.ReloadNeeded()) { GR.DrawImage(QuickImage.Get(enImageCode.Uhr, 16).BMP, 8, 8); }
                if (Database.HasPendingChanges()) { GR.DrawImage(QuickImage.Get(enImageCode.Stift, 16).BMP, 16, 8); }
                if (Database.ReadOnly) { GR.DrawImage(QuickImage.Get(enImageCode.Schloss, 32).BMP, 16, 8); }



            }
            catch (Exception ex)
            {
                Develop.DebugPrint(ex);
            }


        }



        private void Draw_Column_Head_Captions(Graphics GR)
        {
            var BVI = new ColumnViewItem[3];
            var LCBVI = new ColumnViewItem[3];
            ColumnViewItem ViewItem;
            ColumnViewItem LastViewItem = null;

            var PermaX = 0;

            for (var X = 0; X < _Database.ColumnArrangements[_ArrangementNr].Count() + 1; X++)
            {
                if (X < _Database.ColumnArrangements[_ArrangementNr].Count())
                {
                    ViewItem = _Database.ColumnArrangements[_ArrangementNr][X];
                }
                else
                {
                    ViewItem = null;
                }


                if (ViewItem?.ViewType == enViewType.PermanentColumn)
                {
                    PermaX = Math.Max(PermaX, (int)ViewItem.OrderTMP_Spalte_X1 + (int)ViewItem._TMP_DrawWidth);
                }




                if (ViewItem == null ||
                    ViewItem.ViewType == enViewType.PermanentColumn ||
                     (int)ViewItem.OrderTMP_Spalte_X1 + (int)ViewItem._TMP_DrawWidth > PermaX)
                {


                    for (var u = 0; u < 3; u++)
                    {
                        var N = ViewItem?.Column.Ueberschrift(u);
                        var V = BVI[u]?.Column.Ueberschrift(u);



                        if (N != V)
                        {


                            if (!string.IsNullOrEmpty(V) && LastViewItem != null)
                            {
                                var LE = Math.Max(0, (int)BVI[u].OrderTMP_Spalte_X1);
                                //int RE = (int)LastViewItem.OrderTMP_Spalte_X1 - 1 ;
                                var RE = (int)LastViewItem.OrderTMP_Spalte_X1 + (int)LastViewItem._TMP_DrawWidth - 1;
                                if (ViewItem?.ViewType != enViewType.PermanentColumn && BVI[u].ViewType != enViewType.PermanentColumn) { LE = Math.Max(LE, PermaX); }

                                if (ViewItem?.ViewType != enViewType.PermanentColumn && BVI[u].ViewType == enViewType.PermanentColumn) { RE = Math.Max(RE, (int)LCBVI[u].OrderTMP_Spalte_X1 + (int)LCBVI[u]._TMP_DrawWidth); }


                                if (LE < RE)
                                {

                                    var r = new Rectangle(LE, u * ColumnCaptionSizeY, RE - LE, ColumnCaptionSizeY);
                                    GR.FillRectangle(new SolidBrush(BVI[u].Column.BackColor), r);
                                    GR.FillRectangle(new SolidBrush(Color.FromArgb(80, 200, 200, 200)), r);
                                    GR.DrawRectangle(Skin.Pen_LinieKräftig, r);
                                    Skin.Draw_FormatedText(GR, V, null, enAlignment.Horizontal_Vertical_Center, r, this, false, _Column_Font, Translate);
                                }
                            }

                            BVI[u] = ViewItem;

                            if (ViewItem?.ViewType == enViewType.PermanentColumn) { LCBVI[u] = ViewItem; }
                        }
                    }
                    LastViewItem = ViewItem;
                }
            }
        }

        private void Draw_Cursor(Graphics GR, Rectangle DisplayRectangleWOSlider, bool OnlyCursorLines)
        {

            if (tmpCursorRect.Width < 1) { return; }

            var stat = enStates.Standard;

            if (Focused()) { stat = enStates.Standard_HasFocus; }

            if (OnlyCursorLines)
            {
                var pen = new Pen(Skin.Color_Border(enDesign.Table_Cursor, stat).SetAlpha(180));
                GR.DrawRectangle(pen, new Rectangle(-1, tmpCursorRect.Top - 1, DisplayRectangleWOSlider.Width + 2, tmpCursorRect.Height + 1));
            }
            else
            {

                Skin.Draw_Back(GR, enDesign.Table_Cursor, stat, tmpCursorRect, this, false);
                Skin.Draw_Border(GR, enDesign.Table_Cursor, stat, tmpCursorRect);
            }


        }


        private void Draw_Table_ListboxStyle(Graphics GR, enStates vState, Rectangle DisplayRectangleWOSlider, int vFirstVisibleRow, int vLastVisibleRow)
        {
            var ItStat = vState;

            Skin.Draw_Back(GR, enDesign.ListBox, vState, DisplayRectangle, this, true);

            var Col = Database.Column[0];


            var r = new Rectangle();
            // Zeilen Zeichnen (Alle Zellen)
            for (var Zeiv = vFirstVisibleRow; Zeiv <= vLastVisibleRow; Zeiv++)
            {
                var Row = SortedRows()[Zeiv];

                var ViewItem = _Database.ColumnArrangements[0][Col];

                r = new Rectangle(0, (int)Row.TMP_Y, DisplayRectangleWithoutSlider().Width, Row_DrawHeight(Row, DisplayRectangleWOSlider));


                if (_CursorPosColumn != null && _CursorPosRow == Row)
                {
                    ItStat |= enStates.Checked;
                }
                else
                {
                    if (Convert.ToBoolean(ItStat & enStates.Checked))
                    {
                        ItStat = ItStat ^ enStates.Checked;
                    }
                }

                ViewItem.OrderTMP_Spalte_X1 = 0;

                Draw_CellListBox(GR, ViewItem, Row, r, enDesign.Item_Listbox, ItStat);


                if (!Row.CellGetBoolean(_Database.Column.SysCorrect))
                {
                    GR.DrawImage(QuickImage.Get("Warnung|16||||||120||50").BMP, new Point(r.Right - 19, (int)(r.Top + (r.Height - 16) / 2.0)));
                }

                if (!string.IsNullOrEmpty(Row.TMP_Chapter))
                {
                    GR.DrawString(Row.TMP_Chapter, _Chapter_Font.Font(), _Chapter_Font.Brush_Color_Main, 0, (int)Row.TMP_Y - RowCaptionFontY);
                }



            }

            Skin.Draw_Border(GR, enDesign.ListBox, vState, DisplayRectangleWOSlider);


        }

        public void ImportClipboard()
        {
            Develop.DebugPrint_InvokeRequired(InvokeRequired, false);


            if (!System.Windows.Forms.Clipboard.ContainsText())
            {
                Notification.Show("Abbruch,<br>kein Text im Clipboard!", enImageCode.Information);
                return;
            }

            //    If Not Editablexx("Import gerade nicht möglich") Then Exit Sub
            var nt = Convert.ToString(System.Windows.Forms.Clipboard.GetDataObject().GetData(System.Windows.Forms.DataFormats.Text));


            using (var x = new Import(_Database, nt))
            {
                x.ShowDialog();
            }


        }

        /// Zeichnet die gesamte Zelle ohne Hintergrund. Die verlinkte Zelle ist bereits bekannt.
        private void Draw_CellTransparentDirect(Graphics GR, ColumnViewItem CellInThisDatabaseColumn, RowItem CellInThisDatabaseRow, int RowY, ColumnItem ContentHolderCellColumn, RowItem ContentHolderCellRow, Rectangle DisplayRectangleWOSlider, BlueFont vfont)
        {


            var toDraw = ContentHolderCellRow.CellGetString(ContentHolderCellColumn);


            if (toDraw == null) { toDraw = string.Empty; }

            if (!ContentHolderCellColumn.MultiLine || !toDraw.Contains("\r"))
            {
                Draw_CellTransparentDirect_OneLine(GR, toDraw, CellInThisDatabaseColumn, CellInThisDatabaseRow, RowY, 0, ContentHolderCellColumn, true, DisplayRectangleWOSlider, vfont);
            }
            else
            {
                var MEI = toDraw.SplitByCR();
                if (ContentHolderCellColumn.ShowMultiLineInOneLine)
                {
                    Draw_CellTransparentDirect_OneLine(GR, MEI.JoinWith("; "), CellInThisDatabaseColumn, CellInThisDatabaseRow, RowY, 0, ContentHolderCellColumn, true, DisplayRectangleWOSlider, vfont);
                }
                else
                {
                    var y = 0;
                    for (var z = 0; z <= MEI.GetUpperBound(0); z++)
                    {
                        Draw_CellTransparentDirect_OneLine(GR, MEI[z], CellInThisDatabaseColumn, CellInThisDatabaseRow, RowY, y, ContentHolderCellColumn, Convert.ToBoolean(z == MEI.GetUpperBound(0)), DisplayRectangleWOSlider, vfont);
                        y += FormatedText_NeededSize(CellInThisDatabaseColumn.Column, MEI[z], vfont, enShortenStyle.Replaced, Pix16 - 1).Height;
                    }
                }
            }


        }


        /// <summary>
        /// Zeichnet die gesamte Zelle ohne Hintergrund und prüft noch, ob der verlinkte Inhalt gezeichnet werden soll.
        /// </summary>
        /// <param name="GR"></param>
        /// <param name="CellInThisDatabaseColumn"></param>
        /// <param name="CellInThisDatabaseRow"></param>
        /// <param name="DisplayRectangleWOSlider"></param>
        /// <param name="vfont"></param>
        private void Draw_CellTransparent(Graphics GR, ColumnViewItem CellInThisDatabaseColumn, RowItem CellInThisDatabaseRow, int RowY, Rectangle DisplayRectangleWOSlider, BlueFont vfont)
        {

            if (CellInThisDatabaseRow == null) { return; }

            if (CellInThisDatabaseColumn.Column.Format == enDataFormat.LinkedCell)
            {
                var LinkedData = CellCollection.LinkedCellData(CellInThisDatabaseColumn.Column, CellInThisDatabaseRow, false, false, false);
                if (LinkedData.Item1 != null && LinkedData.Item2 != null)
                {
                    Draw_CellTransparentDirect(GR, CellInThisDatabaseColumn, CellInThisDatabaseRow, RowY, LinkedData.Item1, LinkedData.Item2, DisplayRectangleWOSlider, vfont);
                }
                return;
            }

            Draw_CellTransparentDirect(GR, CellInThisDatabaseColumn, CellInThisDatabaseRow, RowY, CellInThisDatabaseColumn.Column, CellInThisDatabaseRow, DisplayRectangleWOSlider, vfont);
        }

        private void Draw_Column_Body(Graphics GR, ColumnViewItem CellInThisDatabaseColumn, Rectangle DisplayRectangleWOSlider)
        {

            GR.SmoothingMode = SmoothingMode.None;
            GR.FillRectangle(new SolidBrush(CellInThisDatabaseColumn.Column.BackColor), (int)CellInThisDatabaseColumn.OrderTMP_Spalte_X1, HeadSize(), Column_DrawWidth(CellInThisDatabaseColumn, DisplayRectangleWOSlider), DisplayRectangleWOSlider.Height);
            Draw_Border(GR, CellInThisDatabaseColumn, DisplayRectangleWOSlider, false);
        }

        private void Draw_Border(Graphics GR, ColumnViewItem vcolumn, Rectangle DisplayRectangleWOSlider, bool Onlyhead)
        {

            var z = 0;
            var yPos = DisplayRectangleWOSlider.Height;

            if (Onlyhead) { yPos = HeadSize(); }

            for (z = 0; z <= 1; z++)
            {
                var xPos = 0;
                enColumnLineStyle Lin = 0;
                if (z == 0)
                {
                    xPos = (int)vcolumn.OrderTMP_Spalte_X1;
                    Lin = vcolumn.Column.LineLeft;
                }
                else
                {
                    xPos = (int)vcolumn.OrderTMP_Spalte_X1 + Column_DrawWidth(vcolumn, DisplayRectangleWOSlider);
                    Lin = vcolumn.Column.LineRight;
                }


                switch (Lin)
                {
                    case enColumnLineStyle.Ohne:
                        break;
                    case enColumnLineStyle.Dünn:
                        GR.DrawLine(Skin.Pen_LinieDünn, xPos, 0, xPos, yPos);
                        break;
                    case enColumnLineStyle.Kräftig:
                        GR.DrawLine(Skin.Pen_LinieKräftig, xPos, 0, xPos, yPos);
                        break;
                    case enColumnLineStyle.Dick:
                        GR.DrawLine(Skin.Pen_LinieDick, xPos, 0, xPos, yPos);
                        break;
                    case enColumnLineStyle.ShadowRight:
                        var c = Skin.Color_Border(enDesign.Table_Lines_thick, enStates.Standard);
                        GR.DrawLine(Skin.Pen_LinieKräftig, xPos, 0, xPos, yPos);
                        GR.DrawLine(new Pen(Color.FromArgb(80, c.R, c.G, c.B)), xPos + 1, 0, xPos + 1, yPos);
                        GR.DrawLine(new Pen(Color.FromArgb(60, c.R, c.G, c.B)), xPos + 2, 0, xPos + 2, yPos);
                        GR.DrawLine(new Pen(Color.FromArgb(40, c.R, c.G, c.B)), xPos + 3, 0, xPos + 3, yPos);
                        GR.DrawLine(new Pen(Color.FromArgb(20, c.R, c.G, c.B)), xPos + 4, 0, xPos + 4, yPos);
                        break;
                    default:
                        Develop.DebugPrint(Lin);
                        break;
                }

            }



        }




        private void Draw_Column_Head(Graphics GR, ColumnViewItem ViewItem, Rectangle DisplayRectangleWOSlider, int lfdNo)
        {
            if (!IsOnScreen(ViewItem, DisplayRectangleWOSlider)) { return; }
            if (_Design == enBlueTableAppearance.OnlyMainColumnWithoutHead) { return; }


            if (_Column_Font == null) { return; }
            GR.FillRectangle(new SolidBrush(ViewItem.Column.BackColor), (int)ViewItem.OrderTMP_Spalte_X1, 0, Column_DrawWidth(ViewItem, DisplayRectangleWOSlider), HeadSize());


            Draw_Border(GR, ViewItem, DisplayRectangleWOSlider, true);
            GR.FillRectangle(new SolidBrush(Color.FromArgb(100, 200, 200, 200)), (int)ViewItem.OrderTMP_Spalte_X1, 0, Column_DrawWidth(ViewItem, DisplayRectangleWOSlider), HeadSize());

            var Down = 0;
            if (!string.IsNullOrEmpty(ViewItem.Column.Ueberschrift3))
            {
                Down = ColumnCaptionSizeY * 3;
            }
            else if (!string.IsNullOrEmpty(ViewItem.Column.Ueberschrift2))
            {
                Down = ColumnCaptionSizeY * 2;
            }
            else if (!string.IsNullOrEmpty(ViewItem.Column.Ueberschrift1))
            {
                Down = ColumnCaptionSizeY;
            }



            // Recude-Button zeichnen
            if (Column_DrawWidth(ViewItem, DisplayRectangleWOSlider) > 70 || ViewItem._TMP_Reduced)
            {

                ViewItem._TMP_ReduceLocation = new Rectangle((int)ViewItem.OrderTMP_Spalte_X1 + Column_DrawWidth(ViewItem, DisplayRectangleWOSlider) - 18, Down, 18, 18);
                if (ViewItem._TMP_Reduced)
                {
                    GR.DrawImage(QuickImage.Get("Pfeil_Rechts|16|||FF0000|||||20").BMP, ViewItem._TMP_ReduceLocation.Left + 2, ViewItem._TMP_ReduceLocation.Top + 2);
                }
                else
                {
                    GR.DrawImage(QuickImage.Get("Pfeil_Links|16||||||||75").BMP, ViewItem._TMP_ReduceLocation.Left + 2, ViewItem._TMP_ReduceLocation.Top + 2);
                }
            }

            // Autofilter-Button zeichnen und lfd Nummer zeichnen
            QuickImage i = null;
            var tstate = enStates.Undefiniert;
            ViewItem._TMP_AutoFilterLocation = new Rectangle((int)ViewItem.OrderTMP_Spalte_X1 + Column_DrawWidth(ViewItem, DisplayRectangleWOSlider) - 18, HeadSize() - 18, 18, 18);

            if (ViewItem.Column.AutoFilter_möglich())
            {
                if (Filter.Uses(ViewItem.Column))
                {
                    tstate = enStates.Checked;
                }
                else if (Autofilter_Sinnvoll(ViewItem))
                {
                    tstate = enStates.Standard;
                }
                else
                {
                    tstate = enStates.Standard_Disabled;
                }
            }

            if (Filter.Uses(ViewItem.Column))
            {
                i = QuickImage.Get("Trichter|14|||FF0000");
            }
            else if (Filter.MayHasRowFilter(ViewItem.Column))
            {
                i = QuickImage.Get("Trichter|14|||227722");


            }
            else if (ViewItem.Column.AutoFilter_möglich())
            {

                if (Autofilter_Sinnvoll(ViewItem))
                {
                    i = QuickImage.Get("Trichter|14");
                }
                else
                {
                    i = QuickImage.Get("Trichter|14||128");
                }
            }

            if (tstate != enStates.Undefiniert)
            {
                Skin.Draw_Back(GR, enDesign.Button_AutoFilter, tstate, ViewItem._TMP_AutoFilterLocation, null, false);
                Skin.Draw_Border(GR, enDesign.Button_AutoFilter, tstate, ViewItem._TMP_AutoFilterLocation);
            }

            if (i != null)
            {
                GR.DrawImage(i.BMP, ViewItem._TMP_AutoFilterLocation.Left + 2, ViewItem._TMP_AutoFilterLocation.Top + 2);
            }



            if (_ShowNumber)
            {
                for (var x = -1; x < 2; x++)
                {
                    for (var y = -1; y < 2; y++)
                    {
                        GR.DrawString("#" + lfdNo.ToString(), _Column_Font.Font(), Brushes.Black, (int)ViewItem.OrderTMP_Spalte_X1 + x, ViewItem._TMP_AutoFilterLocation.Top + y);

                    }
                }
                GR.DrawString("#" + lfdNo.ToString(), _Column_Font.Font(), Brushes.White, (int)ViewItem.OrderTMP_Spalte_X1, ViewItem._TMP_AutoFilterLocation.Top);
            }



            if (tstate == enStates.Undefiniert)
            {
                ViewItem._TMP_AutoFilterLocation = new Rectangle(0, 0, 0, 0);
            }

            var tx = ViewItem.Column.Caption;
            tx = BlueDatabase.LanguageTool.DoTranslate(tx, Translate).Replace("\r", "\r\n");
            var FS = GR.MeasureString(tx, _Column_Font.Font());



            if (ViewItem.Column.CaptionBitmap != null && ViewItem.Column.CaptionBitmap.Width > 10)
            {
                var pos = new Point((int)ViewItem.OrderTMP_Spalte_X1 + (int)((Column_DrawWidth(ViewItem, DisplayRectangleWOSlider) - FS.Width) / 2.0), 3 + Down);
                GR.DrawImageInRectAspectRatio(ViewItem.Column.CaptionBitmap, (int)ViewItem.OrderTMP_Spalte_X1 + 2, (int)(pos.Y + FS.Height), (int)Column_DrawWidth(ViewItem, DisplayRectangleWOSlider) - 4, HeadSize() - (int)(pos.Y + FS.Height) - 6 - 18);

                // Dann der Text
                GR.TranslateTransform(pos.X, pos.Y);
                GR.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                GR.DrawString(tx, _Column_Font.Font(), new SolidBrush(ViewItem.Column.ForeColor), 0, 0);
                GR.TranslateTransform(-pos.X, -pos.Y);

            }
            else
            {
                var pos = new Point((int)ViewItem.OrderTMP_Spalte_X1 + (int)((Column_DrawWidth(ViewItem, DisplayRectangleWOSlider) - FS.Height) / 2.0), HeadSize() - 4 - 18);

                GR.TranslateTransform(pos.X, pos.Y);

                GR.RotateTransform(-90);
                GR.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                GR.DrawString(tx, _Column_Font.Font(), new SolidBrush(ViewItem.Column.ForeColor), 0, 0);
                GR.TranslateTransform(-pos.X, -pos.Y);
                GR.ResetTransform();
            }


            // Sortierrichtung Zeichnen
            var tmpSortDefinition = SortUsed();
            if (tmpSortDefinition != null && tmpSortDefinition.UsedForRowSort(ViewItem.Column))
            {
                if (tmpSortDefinition.Reverse)
                {
                    GR.DrawImage(QuickImage.Get("ZA|11|5||||50").BMP, (float)(ViewItem.OrderTMP_Spalte_X1 + Column_DrawWidth(ViewItem, DisplayRectangleWOSlider) / 2.0 - 6), HeadSize() - 6 - 18);
                }
                else
                {
                    GR.DrawImage(QuickImage.Get("AZ|11|5||||50").BMP, (float)(ViewItem.OrderTMP_Spalte_X1 + Column_DrawWidth(ViewItem, DisplayRectangleWOSlider) / 2.0 - 6), HeadSize() - 6 - 18);
                }
            }

        }


        /// <summary>
        /// Zeichnet die gesamte Zelle mit Listbox-Hintergrund und prüft noch, ob der verlinkte Inhalt gezeichnet werden soll.
        /// </summary>
        /// <param name="GR"></param>
        /// <param name="Column"></param>
        /// <param name="Row"></param>
        /// <param name="DisplayRectangleWOSlider"></param>
        /// <param name="vDesign"></param>
        /// <param name="vState"></param>
        private void Draw_CellListBox(Graphics GR, ColumnViewItem Column, RowItem Row, Rectangle DisplayRectangleWOSlider, enDesign vDesign, enStates vState)
        {

            Skin.Draw_Back(GR, vDesign, vState, DisplayRectangleWOSlider, null, false);
            Skin.Draw_Border(GR, vDesign, vState, DisplayRectangleWOSlider);

            var f = Skin.GetBlueFont(vDesign, vState);
            if (f == null) { return; }


            Draw_CellTransparent(GR, Column, Row, (int)Row.TMP_Y, DisplayRectangleWOSlider, f);

        }

        /// Zeichnet die eine Zeile der Zelle ohne Hintergrund und prüft noch, ob der verlinkte Inhalt gezeichnet werden soll.
        private void Draw_CellTransparentDirect_OneLine(Graphics GR, string DrawString, ColumnViewItem CellInThisDatabaseColumn, RowItem CellInThisDatabaseRow, int RowY, int TxtY, ColumnItem ContentHolderColumnStyle, bool IsLastRow, Rectangle DisplayRectangleWOSlider, BlueFont vfont)
        {


            var r = new Rectangle((int)CellInThisDatabaseColumn.OrderTMP_Spalte_X1,
                                   RowY + TxtY,
                                  Column_DrawWidth(CellInThisDatabaseColumn, DisplayRectangleWOSlider),
                                  Pix16);

            if (r.Bottom > RowY + Row_DrawHeight(CellInThisDatabaseRow, DisplayRectangleWOSlider) - Pix16)
            {
                if (r.Bottom > RowY + Row_DrawHeight(CellInThisDatabaseRow, DisplayRectangleWOSlider)) { return; }
                if (!IsLastRow) { DrawString = "..."; }// Die Letzte Zeile noch ganz hinschreiben
            }

            var tmpImageCode = CellItem.StandardImage(ContentHolderColumnStyle, DrawString);

            Draw_FormatedText(GR, ContentHolderColumnStyle, DrawString, tmpImageCode, r, false, vfont, enShortenStyle.Replaced);
        }



        #endregion


        public static void Database_NeedPassword(object sender, PasswordEventArgs e)
        {
            if (e.Handled) { return; }
            e.Handled = true;
            e.Password = InputBox.Show("Bitte geben sie das Passwort ein,<br>um Zugriff auf diese Datenbank<br>zu erhalten:", string.Empty, enDataFormat.Text);
        }





        private static void DB_RenameColumnInLayout(object sender, RenameColumnInLayoutEventArgs e)
        {
            if (e.Handled) { return; }
            e.Handled = true;
            var Padx = new ItemCollectionPad(e.LayoutCode, string.Empty);
            Padx.RenameColumn(e.OldName, e.Column);
            e.LayoutCode = Padx.ToString();
        }

        private static void DB_GenerateLayoutInternal(object sender, GenerateLayoutInternalEventargs e)
        {
            if (e.Handled) { return; }
            e.Handled = true;

            var Pad = new ItemCollectionPad(e.LayoutID, e.Row);
            Pad.SaveAsBitmap(e.Filename);
        }



        private void ColumnArrangements_ItemInternalChanged(object sender, ListEventArgs e)
        {
            OnColumnArrangementChanged();
            Invalidate();
        }



        public new void Focus()
        {
            if (Focused()) { return; }
            base.Focus();
        }


        public new bool Focused()
        {
            if (base.Focused) { return true; }
            if (SliderY.Focused()) { return true; }
            if (SliderX.Focused()) { return true; }
            if (BTB.Focused()) { return true; }
            if (BCB.Focused()) { return true; }
            return false;
        }



        public ColumnItem CursorPosColumn()
        {
            return _CursorPosColumn;
        }
        public RowItem CursorPosRow()
        {
            return _CursorPosRow;
        }

        public void Export_HTML(string Filename = "", bool Execute = true)
        {
            if (_Database == null) { return; }

            if (string.IsNullOrEmpty(Filename)) { Filename = TempFile("", "", "html"); }

            _Database.Export_HTML(Filename, _Database.ColumnArrangements[_ArrangementNr], SortedRows(), Execute);
        }


        public string Export_CSV(enFirstRow FirstRow)
        {
            if (_Database == null) { return string.Empty; }
            return _Database.Export_CSV(FirstRow, _Database.ColumnArrangements[_ArrangementNr], SortedRows());
        }


        public string Export_CSV(enFirstRow FirstRow, ColumnItem OnlyColumn)
        {

            if (_Database == null) { return string.Empty; }
            var l = new List<ColumnItem> { OnlyColumn };

            return _Database.Export_CSV(FirstRow, l, SortedRows());


        }



        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (_Database == null) { return; }

            lock (Lock_UserAction)
            {
                if (ISIN_MouseDown) { return; }
                ISIN_MouseDown = true;

                _Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());

                CellOnCoordinate(e.X, e.Y, out _MouseOverColumn, out _MouseOverRow);

                // Die beiden Befehle nur in Mouse Down!
                // Wenn der Cursor bei Click/Up/Down geeändert wird, wird ein ereignis ausgelöst.
                // Das könnte auch sehr Zeitintensiv sein. Dann kann die Maus inzwischen wo ander sein.
                // Somit würde das Ereignis doppelt und dreifach ausgelöste werden können.
                // Beipiel: MouseDown-> Bildchen im Pad erzeugen, dauert.... Maus Bewegt sich
                //          MouseUp  -> Curswor wird umgesetzt, Ereginis CursorChanged wieder ausgelöst, noch ein Bildchen
                EnsureVisible(_MouseOverColumn, _MouseOverRow);
                CursorPos_Set(_MouseOverColumn, _MouseOverRow, false);
                ISIN_MouseDown = false;
            }

        }


        private void DropDownMenu_ItemClicked(object sender, ContextMenuItemClickedEventArgs e)
        {

            FloatingInputBoxListBoxStyle.Close(this);
            if (string.IsNullOrEmpty(e.ClickedComand)) { return; }


            var CellKey = string.Empty;
            if (e.HotItem is string s) { CellKey = s; }
            if (string.IsNullOrEmpty(CellKey)) { return; }
            Database.Cell.DataOfCellKey(CellKey, out var Column, out var Row);



            var ToAdd = e.ClickedComand;
            var ToRemove = string.Empty;


            if (ToAdd == "#Erweitert")
            {
                Cell_Edit(Column, Row, false);
                return;
            }

            if (Row == null)
            {
                // Neue Zeile!
                UserEdited(this, ToAdd, Column, null, false);
                return;
            }


            if (Column.MultiLine)
            {
                var E = Row.CellGetList(Column);

                if (E.Contains(ToAdd, false))
                {
                    // Ist das angeklickte Element schon vorhanden, dann soll es wohl abgewählt (gelöscht) werden.
                    if (E.Count > -1 || Column.DropdownAllesAbwählenErlaubt)
                    {
                        ToRemove = ToAdd;
                        ToAdd = string.Empty;
                    }
                }


                if (!string.IsNullOrEmpty(ToRemove)) { E.RemoveString(ToRemove, false); }
                if (!string.IsNullOrEmpty(ToAdd)) { E.Add(ToAdd); }

                UserEdited(this, E.JoinWithCr(), Column, Row, false);
            }
            else
            {

                if (Column.DropdownAllesAbwählenErlaubt)
                {
                    if (ToAdd == Row.CellGetString(Column))
                    {
                        UserEdited(this, string.Empty, Column, Row, false);
                        return;
                    }
                }


                UserEdited(this, ToAdd, Column, Row, false);
            }


        }




        private void Cell_Edit(ColumnItem CellInThisDatabaseColumn, RowItem CellInThisDatabaseRow, bool WithDropDown)
        {
            Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());

            ColumnItem ContentHolderCellColumn;
            RowItem ContentHolderCellRow;


            if (Database.ReloadNeeded()) { Database.Load_Reload(); }


            var f = Database.ErrorReason(enErrorReason.EditGeneral);
            if (!string.IsNullOrEmpty(f)) { NotEditableInfo(f); return; }


            if (CellInThisDatabaseColumn == null)
            {
                //NotEditableInfo("Interner Zellenfehler");
                return; // Klick ins Leere
            }

            var ViewItem = _Database.ColumnArrangements[_ArrangementNr][CellInThisDatabaseColumn];


            if (ViewItem == null)
            {
                NotEditableInfo("Ansicht veraltet");
                return;
            }

            if (CellInThisDatabaseColumn.Format == enDataFormat.LinkedCell)
            {
                var LinkedData = CellCollection.LinkedCellData(CellInThisDatabaseColumn, CellInThisDatabaseRow, false, true, true);
                if (LinkedData.Item1 == null)
                {
                    NotEditableInfo("In verknüpfter Datenbank nicht vorhanden");
                    return;
                }
                ContentHolderCellColumn = LinkedData.Item1;
                ContentHolderCellRow = LinkedData.Item2;
            }
            else
            {
                ContentHolderCellColumn = CellInThisDatabaseColumn;
                ContentHolderCellRow = CellInThisDatabaseRow;
            }


            if (!ContentHolderCellColumn.DropdownBearbeitungErlaubt) { WithDropDown = false; }
            var dia = ColumnItem.UserEditDialogTypeInTable(ContentHolderCellColumn, WithDropDown);
            if (dia == enEditTypeTable.None)
            {
                NotEditableInfo("Diese Spalte kann generell nicht bearbeitet werden.");
                return;
            }
            if (!CellCollection.UserEditPossible(ContentHolderCellColumn, ContentHolderCellRow, enErrorReason.EditGeneral))
            {
                NotEditableInfo(CellCollection.ErrorReason(ContentHolderCellColumn, ContentHolderCellRow, enErrorReason.EditGeneral));
                return;
            }

            if (CellInThisDatabaseRow != null)
            {
                if (!EnsureVisible(CellInThisDatabaseColumn, CellInThisDatabaseRow))
                {
                    NotEditableInfo("Zelle konnte nicht angezeigt werden.");
                    return;
                }
                if (!IsOnScreen(CellInThisDatabaseColumn, CellInThisDatabaseRow, DisplayRectangle))
                {
                    NotEditableInfo("Die Zelle wird nicht angezeigt.");
                    return;
                }
                CursorPos_Set(CellInThisDatabaseColumn, CellInThisDatabaseRow, false);
            }
            else
            {
                if (!UserEdit_NewRowAllowed())
                {
                    NotEditableInfo("Keine neuen Zeilen erlaubt.");
                    return;
                }
                if (!EnsureVisible(ViewItem))
                {
                    NotEditableInfo("Zelle konnte nicht angezeigt werden.");
                    return;
                }
                if (!IsOnScreen(ViewItem, DisplayRectangle))
                {
                    NotEditableInfo("Die Zelle wird nicht angezeigt.");
                    return;
                }
                SliderY.Value = 0;
            }


            var Cancel = "";

            if (CellInThisDatabaseRow != null)
            {
                var ed = new CellCancelEventArgs(CellInThisDatabaseColumn, CellInThisDatabaseRow, Cancel);
                OnEditBeforeBeginEdit(ed);
                Cancel = ed.CancelReason;
            }
            else
            {
                var ed = new RowCancelEventArgs(null, Cancel);
                OnEditBeforeNewRow(ed);
                Cancel = ed.CancelReason;
            }
            if (!string.IsNullOrEmpty(Cancel))
            {
                NotEditableInfo(Cancel);
                return;
            }




            switch (dia)
            {
                case enEditTypeTable.Textfeld:
                    Cell_Edit_TextBox(CellInThisDatabaseColumn, CellInThisDatabaseRow, ContentHolderCellColumn, ContentHolderCellRow, BTB, 0, 0);
                    break;
                case enEditTypeTable.Textfeld_mit_Auswahlknopf:
                    Cell_Edit_TextBox(CellInThisDatabaseColumn, CellInThisDatabaseRow, ContentHolderCellColumn, ContentHolderCellRow, BCB, 20, 18);
                    break;
                case enEditTypeTable.Dropdown_Single:
                    Cell_Edit_Dropdown(CellInThisDatabaseColumn, CellInThisDatabaseRow, ContentHolderCellColumn, ContentHolderCellRow);
                    break;
                //break; case enEditType.Dropdown_Multi:
                //    Cell_Edit_Dropdown(CellInThisDatabaseColumn, CellInThisDatabaseRow, ContentHolderCellColumn, ContentHolderCellRow);
                //    break;
                //case enEditTypeTable.RelationEditor_InTable:
                //    if (CellInThisDatabaseColumn != ContentHolderCellColumn || CellInThisDatabaseRow != ContentHolderCellRow)
                //    {
                //        NotEditableInfo("Ziel-Spalte ist kein Textformat");
                //        return;
                //    }
                //    Cell_Edit_Relations(CellInThisDatabaseColumn, CellInThisDatabaseRow);
                //    break;
                case enEditTypeTable.Farb_Auswahl_Dialog:
                    if (CellInThisDatabaseColumn != ContentHolderCellColumn || CellInThisDatabaseRow != ContentHolderCellRow)
                    {
                        NotEditableInfo("Ziel-Spalte ist kein Textformat");
                        return;
                    }
                    Cell_Edit_Color(CellInThisDatabaseColumn, CellInThisDatabaseRow);
                    break;
                case enEditTypeTable.Font_AuswahlDialog:
                    Develop.DebugPrint_NichtImplementiert();
                    //if (CellInThisDatabaseColumn != ContentHolderCellColumn || CellInThisDatabaseRow != ContentHolderCellRow)
                    //{
                    //    NotEditableInfo("Ziel-Spalte ist kein Textformat");
                    //    return;
                    //}
                    //Cell_Edit_Font(CellInThisDatabaseColumn, CellInThisDatabaseRow);
                    break;

                default:

                    Develop.DebugPrint(dia);
                    NotEditableInfo("Unbekannte Bearbeitungs-Methode");
                    break;
            }

        }

        private void OnEditBeforeNewRow(RowCancelEventArgs e)
        {
            EditBeforeNewRow?.Invoke(this, e);
        }

        private void OnEditBeforeBeginEdit(CellCancelEventArgs e)
        {
            EditBeforeBeginEdit?.Invoke(this, e);
        }


        private void Cell_Edit_Color(ColumnItem CellInThisDatabaseColumn, RowItem CellInThisDatabaseRow)
        {
            ColDia.Color = CellInThisDatabaseRow.CellGetColor(CellInThisDatabaseColumn);
            ColDia.Tag = CellCollection.KeyOfCell(CellInThisDatabaseColumn, CellInThisDatabaseRow);

            var ColList = new List<int>();

            foreach (var ThisRowItem in _Database.Row)
            {
                if (ThisRowItem != null)
                {
                    if (ThisRowItem.CellGetInteger(CellInThisDatabaseColumn) != 0)
                    {
                        ColList.Add(ThisRowItem.CellGetColorBGR(CellInThisDatabaseColumn));
                    }
                }
            }
            ColList.Sort();

            ColDia.CustomColors = ColList.Distinct().ToArray();


            ColDia.ShowDialog();

            UserEdited(this, Color.FromArgb(255, ColDia.Color).ToArgb().ToString(), CellInThisDatabaseColumn, CellInThisDatabaseRow, false);
        }

        private bool Cell_Edit_TextBox(ColumnItem CellInThisDatabaseColumn, RowItem CellInThisDatabaseRow, ColumnItem ContentHolderCellColumn, RowItem ContentHolderCellRow, TextBox Box, int AddWith, int IsHeight)
        {


            if (ContentHolderCellColumn != CellInThisDatabaseColumn)
            {
                if (ContentHolderCellRow == null)
                {
                    NotEditableInfo("Bei Zellverweisen kann keine neue Zeile erstellt werden.");
                    return false;
                }
                if (CellInThisDatabaseRow == null)
                {
                    NotEditableInfo("Bei Zellverweisen kann keine neue Zeile erstellt werden.");
                    return false;
                }
            }


            var ViewItemx = _Database.ColumnArrangements[_ArrangementNr][CellInThisDatabaseColumn];

            if (ContentHolderCellRow != null)
            {
                var h = Row_DrawHeight(CellInThisDatabaseRow, DisplayRectangle);
                if (IsHeight > 0) { h = IsHeight; }

                Box.Location = new Point((int)ViewItemx.OrderTMP_Spalte_X1, (int)CellInThisDatabaseRow.TMP_Y);

                Box.Size = new Size(Column_DrawWidth(ViewItemx, DisplayRectangle) + AddWith, h);
                Box.Text = ContentHolderCellRow.CellGetString(ContentHolderCellColumn).Replace(Constants.beChrW1.ToString(), "\r"); // Texte aus alter Zeit...
            }
            else
            {
                // Neue Zeile...
                Box.Location = new Point((int)ViewItemx.OrderTMP_Spalte_X1, HeadSize());
                Box.Size = new Size(Column_DrawWidth(ViewItemx, DisplayRectangle) + AddWith, Pix18);
                Box.Text = "";
            }


            Box.Format = ContentHolderCellColumn.Format;
            Box.AllowedChars = ContentHolderCellColumn.AllowedChars;
            Box.MultiLine = ContentHolderCellColumn.MultiLine;
            Box.Tag = CellCollection.KeyOfCell(CellInThisDatabaseColumn, CellInThisDatabaseRow); // ThisDatabase, der Wert wird beim einchecken in die Fremdzelle geschrieben




            if (Box is ComboBox box)
            {
                ItemCollectionList.GetItemCollection(box.Item, ContentHolderCellColumn, ContentHolderCellRow, enShortenStyle.Both, 1000);
                if (box.Item.Count == 0)
                {
                    return Cell_Edit_TextBox(CellInThisDatabaseColumn, CellInThisDatabaseRow, ContentHolderCellColumn, ContentHolderCellRow, BTB, 0, 0);
                }
            }


            if (string.IsNullOrEmpty(Box.Text))
            {
                Box.Text = CellCollection.AutomaticInitalValue(ContentHolderCellColumn, ContentHolderCellRow);
            }



            Box.Visible = true;
            Box.BringToFront();

            Box.Focus();

            return true;
        }


        private void Cell_Edit_Dropdown(ColumnItem CellInThisDatabaseColumn, RowItem CellInThisDatabaseRow, ColumnItem ContentHolderCellColumn, RowItem ContentHolderCellRow)
        {

            if (CellInThisDatabaseColumn != ContentHolderCellColumn)
            {
                if (ContentHolderCellRow == null)
                {
                    NotEditableInfo("Bei Zellverweisen kann keine neue Zeile erstellt werden.");
                    return;
                }
                if (CellInThisDatabaseRow == null)
                {
                    NotEditableInfo("Bei Zellverweisen kann keine neue Zeile erstellt werden.");
                    return;
                }
            }

            var t = new ItemCollectionList();
            t.Appearance = enBlueListBoxAppearance.DropdownSelectbox;
            ItemCollectionList.GetItemCollection(t, ContentHolderCellColumn, ContentHolderCellRow, enShortenStyle.Both, 1000);

            if (t.Count == 0)
            {
                // Hm.. Dropdown kein Wert vorhanden.... also gar kein Dropdown öffnen!
                if (ContentHolderCellColumn.TextBearbeitungErlaubt) { Cell_Edit(CellInThisDatabaseColumn, CellInThisDatabaseRow, false); }
                return;
            }


            if (ContentHolderCellColumn.TextBearbeitungErlaubt)
            {
                if (t.Count == 1)
                {
                    // Bei nur einem Wert, wenn Texteingabe erlaubt, Dropdown öffnen
                    Cell_Edit(CellInThisDatabaseColumn, CellInThisDatabaseRow, false);
                    return;
                }
                t.Add(new TextListItem("#Erweitert", "Erweiterte Eingabe", QuickImage.Get(enImageCode.Stift), true, Constants.FirstSortChar + "1"));
                t.Add(new LineListItem(Constants.FirstSortChar + "2"));
                t.Sort();
            }

            var _DropDownMenu = FloatingInputBoxListBoxStyle.Show(t, CellCollection.KeyOfCell(CellInThisDatabaseColumn, CellInThisDatabaseRow), this, Translate);
            _DropDownMenu.ItemClicked += DropDownMenu_ItemClicked;
            Develop.Debugprint_BackgroundThread();
        }










        private static void UserEdited(Table table, string newValue, ColumnItem column, RowItem row, bool formatWarnung)
        {

            if (column == null)
            {
                table.NotEditableInfo("Keine Spalte angegeben.");
                return;
            }


            if (column.Format == enDataFormat.LinkedCell)
            {
                var LinkedData = CellCollection.LinkedCellData(column, row, false, true, false);
                if (LinkedData.Item1 == null || LinkedData.Item2 == null)
                {
                    table.NotEditableInfo("Zelle in verlinkter Datenbank nicht vorhanden.");
                    return;
                }
                UserEdited(table, newValue, LinkedData.Item1, LinkedData.Item2, formatWarnung);
                if (table.Database == column.Database) { table.CursorPos_Set(column, row, false); }
                return;
            }

            if (row == null && column != column.Database.Column[0])
            {

                table.NotEditableInfo("Neue Zeilen müssen mit der ersten Spalte beginnen");
                return;
            }


            newValue = column.AutoCorrect(newValue);


            if (row != null)
            {
                if (newValue == row.CellGetString(column)) { return; }
            }
            else
            {
                if (string.IsNullOrEmpty(newValue)) { return; }
            }


            var ed = new BeforeNewValueEventArgs(column, row, newValue, string.Empty);
            table.OnEditBeforeNewValueSet(ed);
            var CancelReason = ed.CancelReason;


            if (string.IsNullOrEmpty(CancelReason) && formatWarnung && !string.IsNullOrEmpty(newValue))
            {

                if (!newValue.IsFormat(column.Format, column.MultiLine))
                {
                    if (Forms.MessageBox.Show("Ihre Eingabe entspricht<br><u>nicht</u> dem erwarteten Format!<br><br>Trotzdem übernehmen?", enImageCode.Information, "Ja", "Nein") != 0)
                    {
                        CancelReason = "Abbruch, das das erwartete Format nicht eingehalten wurde.";
                    }
                }
            }


            if (string.IsNullOrEmpty(CancelReason))
            {
                //var f = column.Database.ErrorReason(enErrorReason.EditGeneral);
                //if (!string.IsNullOrEmpty(f)) { table.NotEditableInfo(f); return; }
                //d
                if (row == null)
                {
                    var f = CellCollection.ErrorReason(column.Database.Column[0], null, enErrorReason.EditGeneral);
                    if (!string.IsNullOrEmpty(f)) { table.NotEditableInfo(f); return; }
                    row = column.Database.Row.Add(newValue);
                }
                else
                {
                    var f = CellCollection.ErrorReason(column, row, enErrorReason.EditGeneral);
                    if (!string.IsNullOrEmpty(f)) { table.NotEditableInfo(f); return; }
                    row.CellSet(column, newValue);
                }

                if (table.Database == column.Database) { table.CursorPos_Set(column, row, false); }

                row.DoAutomatic(true, false);

                // EnsureVisible ganz schlecht: Daten verändert, keine Positionen bekannt - und da soll sichtbar gemacht werden?
                // CursorPos.EnsureVisible(SliderX, SliderY, DisplayRectangle)

            }
            else
            {
                table.NotEditableInfo(CancelReason);
            }

        }

        private void OnEditBeforeNewValueSet(BeforeNewValueEventArgs ed)
        {
            EditBeforeNewValueSet?.Invoke(this, ed);
        }

        private void TXTBox_Close(TextBox BTBxx)
        {


            if (BTBxx == null) { return; }
            if (!BTBxx.Visible) { return; }
            if (BTBxx.Tag == null || string.IsNullOrEmpty(BTBxx.Tag.ToString()))
            {
                BTBxx.Visible = false;
                return; // Ohne Dem hier wird ganz am Anfang ein Ereignis ausgelöst
            }


            var w = BTBxx.Text;
            var tmpTag = (string)BTBxx.Tag;

            Database.Cell.DataOfCellKey(tmpTag, out var column, out var row);


            BTBxx.Tag = null;
            BTBxx.Visible = false;

            UserEdited(this, w, column, row, true);

            Focus();

        }

        /// <summary>
        /// Setzt die Variable CursorPos um X Columns und Y Reihen um. Dabei wird die Columns und Zeilensortierung berücksichtigt. 
        /// </summary>
        /// <remarks></remarks>
        private void Cursor_Move(enDirection Richtung)
        {
            if (_Database == null) { return; }
            Neighbour(_CursorPosColumn, _CursorPosRow, Richtung, out var _newCol, out var _newRow);

            CursorPos_Set(_newCol, _newRow, Richtung != enDirection.Nichts);
        }


        private bool ComputeAllCellPositions()
        {
            //Develop.DebugPrint_InvokeRequired(InvokeRequired, true);


            if (Database.IsParsing()) { return false; }

            try
            {

                var MaxX = 0;
                var MaxY = 0;
                var LastCap = "";

                var DisplayR = DisplayRectangleWithoutSlider();


                if (UserEdit_NewRowAllowed()) { MaxY += Pix18; }


                // Kommt vor, dass spontan doch geparsed wird...
                if (_Database.ColumnArrangements == null || _ArrangementNr >= _Database.ColumnArrangements.Count) { return false; }



                foreach (var ThisViewItem in _Database.ColumnArrangements[_ArrangementNr])
                {
                    if (ThisViewItem != null)
                    {
                        ThisViewItem.OrderTMP_Spalte_X1 = null;
                        ThisViewItem.TMP_AutoFilterSinnvoll = null;
                    }
                }
                foreach (var ThisRowItem in _Database.Row)
                {
                    if (ThisRowItem != null) { ThisRowItem.TMP_Y = null; }
                }



                WiederHolungsSpaltenWidth = 0;

                var wdh = true;

                // Spalten berechnen
                foreach (var ThisViewItem in _Database.ColumnArrangements[_ArrangementNr])
                {
                    if (ThisViewItem?.Column != null)
                    {
                        if (ThisViewItem.ViewType != enViewType.PermanentColumn) { wdh = false; }

                        if (wdh)
                        {
                            ThisViewItem.OrderTMP_Spalte_X1 = MaxX;
                            MaxX += Column_DrawWidth(ThisViewItem, DisplayR);
                            WiederHolungsSpaltenWidth = Math.Max(WiederHolungsSpaltenWidth, MaxX);
                        }
                        else
                        {
                            if (SliderX.Visible)
                            {
                                ThisViewItem.OrderTMP_Spalte_X1 = (int)(MaxX - SliderX.Value);
                            }
                            else
                            {
                                ThisViewItem.OrderTMP_Spalte_X1 = 0;
                            }
                            MaxX += Column_DrawWidth(ThisViewItem, DisplayR);
                        }
                    }
                }




                foreach (var ThisRow in SortedRows())
                {
                    ThisRow.TMP_Y = (int)(MaxY - SliderY.Value + HeadSize());

                    if (ThisRow.CellGetString(_Database.Column.SysChapter) != LastCap)
                    {
                        ThisRow.TMP_Y += RowCaptionSizeY;
                        MaxY += RowCaptionSizeY;
                        LastCap = ThisRow.CellGetString(_Database.Column.SysChapter);


                        if (string.IsNullOrEmpty(LastCap))
                        {
                            ThisRow.TMP_Chapter = "- ohne " + _Database.Column.SysChapter.Caption + " -";
                        }
                        else
                        {
                            ThisRow.TMP_Chapter = LastCap.Replace("\r", ", ");
                        }

                    }
                    else
                    {
                        ThisRow.TMP_Chapter = "";
                    }

                    MaxY += Row_DrawHeight(ThisRow, DisplayR);
                }

                // Slider berechnen ---------------------------------------------------------------
                SliderSchalten(DisplayR, MaxX, MaxY);


            }
            catch (Exception ex)
            {
                Develop.DebugPrint(ex);
                return false;
            }

            return true;


        }

        internal bool PermanentPossible(ColumnViewItem ThisViewItem)
        {
            if (_ArrangementNr < 1)
            {
                if (ThisViewItem.Column.IsFirst()) { return true; }
                return false;
            }


            var prev = ThisViewItem.PreviewsVisible(_Database.ColumnArrangements[_ArrangementNr]);

            if (prev == null) { return true; }
            return Convert.ToBoolean(prev.ViewType == enViewType.PermanentColumn);


        }

        internal bool NonPermanentPossible(ColumnViewItem ThisViewItem)
        {
            if (_ArrangementNr < 1)
            {
                if (ThisViewItem.Column.IsFirst()) { return false; }
                return true;
            }


            var NX = ThisViewItem.NextVisible(_Database.ColumnArrangements[_ArrangementNr]);

            if (NX == null) { return true; }
            return Convert.ToBoolean(NX.ViewType != enViewType.PermanentColumn);


        }










        protected override bool IsInputKey(System.Windows.Forms.Keys keyData)
        {
            // Ganz wichtig diese Routine!
            // Wenn diese NICHT ist, geht der Fokus weg, sobald der cursor gedrückt wird.

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


        #region  AutoFilter 

        private void AutoFilter_Show(ColumnViewItem columnviewitem, int screenx, int screeny)
        {

            if (columnviewitem == null) { return; }

            if (_Design == enBlueTableAppearance.OnlyMainColumnWithoutHead) { return; }

            if (!columnviewitem.Column.AutoFilter_möglich()) { return; }

            Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
            //OnBeforeAutoFilterShow(new ColumnEventArgs(columnviewitem.Column));

            _AutoFilter = new AutoFilter(columnviewitem.Column, Filter);


            _AutoFilter.Position_LocateToPosition(new Point(screenx + (int)columnviewitem.OrderTMP_Spalte_X1, screeny + HeadSize()));

            _AutoFilter.Show();

            _AutoFilter.FilterComand += AutoFilter_FilterComand;
            Develop.Debugprint_BackgroundThread();
        }


        private void AutoFilter_Close()
        {
            if (_AutoFilter != null)
            {
                _AutoFilter.FilterComand -= AutoFilter_FilterComand;

                _AutoFilter.Dispose();
                _AutoFilter = null;
            }
        }







        private void AutoFilter_FilterComand(object sender, FilterComandEventArgs e)
        {
            switch (e.Comand.ToLower())
            {
                case "":
                    break;

                case "filter":
                    Filter.Delete(e.Column);
                    Filter.Add(e.Filter);
                    break;

                case "filterdelete":
                    Filter.Delete(e.Column);
                    break;

                case "doeinzigartig":
                    Filter.Delete(e.Column);
                    e.Column.GetUniques(SortedRows(), out var Einzigartig, out var NichtEinzigartig);

                    if (Einzigartig.Count > 0)
                    {
                        Filter.Add(new FilterItem(e.Column, enFilterType.Istgleich_ODER_GroßKleinEgal, Einzigartig));
                        Notification.Show("Die aktuell einzigartigen Einträge wurden berechnet<br>und als <b>ODER-Filter</b> gespeichert.", enImageCode.Trichter);
                    }
                    else
                    {
                        Notification.Show("Filterung dieser Spalte gelöscht,<br>da <b>alle Einträge</b> mehrfach vorhanden sind.", enImageCode.Trichter);
                    }
                    break;

                case "donichteinzigartig":
                    Filter.Delete(e.Column);
                    e.Column.GetUniques(SortedRows(), out var xEinzigartig, out var xNichtEinzigartig);

                    if (xNichtEinzigartig.Count > 0)
                    {
                        Filter.Add(new FilterItem(e.Column, enFilterType.Istgleich_ODER_GroßKleinEgal, xNichtEinzigartig));
                        Notification.Show("Die aktuell <b>nicht</b> einzigartigen Einträge wurden berechnet<br>und als <b>ODER-Filter</b> gespeichert.", enImageCode.Trichter);
                    }
                    else
                    {
                        Notification.Show("Filterung dieser Spalte gelöscht,<br>da <b>alle Einträge</b> einzigartig sind.", enImageCode.Trichter);
                    }
                    break;

                default:
                    Develop.DebugPrint("Unbekannter Comand:   " + e.Comand);
                    break;
            }


            OnAutoFilterClicked(new FilterEventArgs(e.Filter));


        }

        private void OnAutoFilterClicked(FilterEventArgs e)
        {
            AutoFilterClicked?.Invoke(this, e);
        }


        private void OnRowAdded(object sender, RowEventArgs e)
        {
            RowAdded?.Invoke(sender, e);
        }


        //private void OnBeforeAutoFilterShow(ColumnEventArgs e)
        //{
        //    BeforeAutoFilterShow?.Invoke(this, e);
        //}



        #endregion

        #region  ContextMenu 



        public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e)
        {
            return false;
        }

        public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e)
        {
            ContextMenuItemClicked?.Invoke(this, e);
        }
        #endregion



        #region  Ereignisse der Slider 
        private void SliderY_ValueChanged(object sender, System.EventArgs e)
        {
            if (_Database == null) { return; }

            lock (Lock_UserAction)
            {
                Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
                Invalidate();
            }

        }

        private void SliderX_ValueChanged(object sender, System.EventArgs e)
        {
            if (_Database == null) { return; }

            lock (Lock_UserAction)
            {
                Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
                Invalidate();
            }
        }
        #endregion



        #region  Ereignisse der Datenbank 






        private void _Database_CellValueChanged(object sender, CellEventArgs e)
        {


            if (AreRowsSorted())
            {
                if (Filter.Uses(e.Column) || SortUsed() == null || SortUsed().UsedForRowSort(e.Column) || Filter.MayHasRowFilter(e.Column))
                {
                    Invalidate_RowSort();
                }
            }

            Invalidatex_DrawWidth(e.Column);
            Invalidate_TMPDrawHeight(e.Row);

            Invalidate();


            OnCellValueChanged(e);


        }

        private void OnCellValueChanged(CellEventArgs e)
        {
            CellValueChanged?.Invoke(this, e);
        }

        private void Invalidate_RowSort()
        {

            if (_Database == null) { return; }

            if (AreRowsSorted())
            {
                _SortedRows = null;
                // InvalidatexXXXXXXX() <- Tödlich, der InvalidatexXXXXXX  muss von wo anders ausgelöst werden!
            }

        }

        public bool AreRowsSorted()
        {

            return _SortedRows != null;
        }


        private void _Database_StopAllWorking(object sender, MultiUserFileStopWorkingEventArgs e)
        {
            CloseAllComponents();
        }






        private void _Database_DatabaseLoaded(object sender, LoadedEventArgs e)
        {
            // Wird auch bei einem Reload ausgeführt.
            // Es kann aber sein, dass eine Ansicht zurückgeholt wurde, und die Werte stimmen. 
            // Deswegen prüfen, ob wirklich alles geleöscht werden muss, oder weiter behalten werden kann.

            // Auf Nothing  muss auch geprüft werden, da bei einem Dispose oder beim Beenden sich die Datenbank auch änsdert....



            Invalidate_HeadSize();

            var f = string.Empty;


            if (Filter != null)
            {
                if (e.OnlyReload) { f = Filter.ToString(); }
                Filter.Changed -= FilterChanged;
                Filter = null;
            }



            if (_Database != null)
            {


                Filter = new FilterCollection(_Database, f);
                Filter.Changed += FilterChanged;

                if (e.OnlyReload)
                {
                    if (_ArrangementNr != 1)
                    {
                        if (_Database.ColumnArrangements == null || _ArrangementNr >= _Database.ColumnArrangements.Count || _Database.ColumnArrangements[_ArrangementNr] == null || !_Database.PermissionCheck(_Database.ColumnArrangements[_ArrangementNr].PermissionGroups_Show, null))
                        {
                            _ArrangementNr = 1;
                        }
                    }

                    if (_MouseOverColumn != null && _MouseOverColumn.Database != _Database)
                    {
                        _MouseOverColumn = null;
                        _MouseOverRow = null;
                    }
                    if (_CursorPosColumn != null && _CursorPosColumn.Database != _Database)
                    {
                        _CursorPosColumn = null;
                        _CursorPosRow = null;
                    }

                }
                else
                {
                    _MouseOverColumn = null;
                    _MouseOverRow = null;
                    _CursorPosColumn = null;
                    _CursorPosRow = null;
                    _ArrangementNr = 1;
                }



            }
            else
            {
                _MouseOverColumn = null;
                _MouseOverRow = null;
                _CursorPosColumn = null;
                _CursorPosRow = null;
                _ArrangementNr = 1;
            }


            _sortDefinitionTemporary = null;

            Invalidate_AllDraw(true);

            Invalidate_RowSort();
            OnViewChanged();

            Invalidate();


            if (e.OnlyReload)
            {
                if (string.IsNullOrEmpty(_StoredView)) { Develop.DebugPrint("Stored View Empty!"); }

                ParseView(_StoredView);
                _StoredView = string.Empty;
            }

        }

        private void CloseAllComponents()
        {

            if (InvokeRequired)
            {
                Invoke(new Action(() => CloseAllComponents()));
                return;
            }

            if (_Database == null) { return; }

            TXTBox_Close(BTB);
            TXTBox_Close(BCB);
            FloatingInputBoxListBoxStyle.Close(this);
            AutoFilter_Close();
            QuickInfo.Close();


        }
        #endregion

        #region  Ereignisse der Form 


        protected override void OnMouseWheel(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (_Database == null) { return; }

            lock (Lock_UserAction)
            {
                if (ISIN_MouseWheel) { return; }
                ISIN_MouseWheel = true;
                Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
                if (!SliderY.Visible)
                {
                    ISIN_MouseWheel = false;
                    return;
                }
                SliderY.DoMouseWheel(e);


                ISIN_MouseWheel = false;
            }

        }


        protected override void OnClick(System.EventArgs e)
        {
            base.OnClick(e);

            if (_Database == null) { return; }

            lock (Lock_UserAction)
            {
                if (ISIN_Click) { return; }
                ISIN_Click = true;

                Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
                CellOnCoordinate(MousePos().X, MousePos().Y, out _MouseOverColumn, out _MouseOverRow);


                ISIN_Click = false;
            }
        }

        protected override void OnSizeChanged(System.EventArgs e)
        {
            base.OnSizeChanged(e);


            if (_Database == null) { return; }

            lock (Lock_UserAction)
            {
                if (ISIN_SizeChanged) { return; }
                ISIN_SizeChanged = true;
                Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());

                Invalidate_AllDraw(false);

                ISIN_SizeChanged = false;
            }
        }

        protected override void OnKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (_Database == null) { return; }

            lock (Lock_UserAction)
            {
                if (ISIN_KeyDown) { return; }
                ISIN_KeyDown = true;

                Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());

                switch (e.KeyCode)
                {
                    case System.Windows.Forms.Keys.X:
                        if (e.Modifiers == System.Windows.Forms.Keys.Control)
                        {
                            CopyToClipboard(_CursorPosColumn, _CursorPosRow, true);

                            if (_CursorPosRow.CellIsNullOrEmpty(_CursorPosColumn))
                            {
                                ISIN_KeyDown = false;
                                return;
                            }

                            var l2 = CellCollection.ErrorReason(_CursorPosColumn, _CursorPosRow, enErrorReason.EditGeneral);

                            if (string.IsNullOrEmpty(l2))
                            {
                                UserEdited(this, string.Empty, _CursorPosColumn, _CursorPosRow, true);
                            }
                            else
                            {
                                NotEditableInfo(l2);
                            }


                        }
                        break;

                    case System.Windows.Forms.Keys.Delete:
                        if (_CursorPosRow.CellIsNullOrEmpty(_CursorPosColumn))
                        {
                            ISIN_KeyDown = false;
                            return;
                        }

                        var l = CellCollection.ErrorReason(_CursorPosColumn, _CursorPosRow, enErrorReason.EditGeneral);

                        if (string.IsNullOrEmpty(l))
                        {
                            UserEdited(this, string.Empty, _CursorPosColumn, _CursorPosRow, true);
                        }
                        else
                        {
                            NotEditableInfo(l);
                        }
                        break;

                    case System.Windows.Forms.Keys.Left:
                        Cursor_Move(enDirection.Links);
                        break;

                    case System.Windows.Forms.Keys.Right:
                        Cursor_Move(enDirection.Rechts);
                        break;

                    case System.Windows.Forms.Keys.Up:
                        Cursor_Move(enDirection.Oben);
                        break;

                    case System.Windows.Forms.Keys.Down:
                        Cursor_Move(enDirection.Unten);
                        break;

                    case System.Windows.Forms.Keys.PageDown:
                        if (SliderY.Enabled)
                        {
                            CursorPos_Set(null, null, false);
                            SliderY.Value += SliderY.LargeChange;
                        }
                        break;

                    case System.Windows.Forms.Keys.PageUp: //Bildab
                        if (SliderY.Enabled)
                        {
                            CursorPos_Set(null, null, false);
                            SliderY.Value -= SliderY.LargeChange;
                        }
                        break;

                    case System.Windows.Forms.Keys.Home:
                        if (SliderY.Enabled)
                        {
                            CursorPos_Set(null, null, false);
                            SliderY.Value = SliderY.Minimum;

                        }
                        break;

                    case System.Windows.Forms.Keys.End:
                        if (SliderY.Enabled)
                        {
                            CursorPos_Set(null, null, false);
                            SliderY.Value = SliderY.Maximum;
                        }
                        break;

                    case System.Windows.Forms.Keys.C:
                        if (e.Modifiers == System.Windows.Forms.Keys.Control)
                        {
                            CopyToClipboard(_CursorPosColumn, _CursorPosRow, true);
                        }
                        break;

                    case System.Windows.Forms.Keys.F:
                        if (e.Modifiers == System.Windows.Forms.Keys.Control)
                        {
                            var x = new Search(this);
                            x.Show();
                        }
                        break;

                    case System.Windows.Forms.Keys.F2:
                        Cell_Edit(_CursorPosColumn, _CursorPosRow, true);
                        break;

                    case System.Windows.Forms.Keys.V:
                        if (e.Modifiers == System.Windows.Forms.Keys.Control)
                        {
                            if (_CursorPosColumn != null && _CursorPosRow != null)
                            {

                                if (!_CursorPosColumn.Format.TextboxEditPossible() && _CursorPosColumn.Format != enDataFormat.Columns_für_LinkedCellDropdown && _CursorPosColumn.Format != enDataFormat.Values_für_LinkedCellDropdown)
                                {
                                    NotEditableInfo("Die Zelle hat kein passendes Format.");
                                    ISIN_KeyDown = false;
                                    return;
                                }

                                if (!System.Windows.Forms.Clipboard.GetDataObject().GetDataPresent(System.Windows.Forms.DataFormats.Text))
                                {
                                    NotEditableInfo("Kein Text in der Zwischenablage.");
                                    ISIN_KeyDown = false;
                                    return;
                                }

                                var ntxt = Convert.ToString(System.Windows.Forms.Clipboard.GetDataObject().GetData(System.Windows.Forms.DataFormats.UnicodeText));
                                if (_CursorPosRow.CellGetString(_CursorPosColumn) == ntxt)
                                {
                                    ISIN_KeyDown = false;
                                    return;
                                }


                                var l2 = CellCollection.ErrorReason(_CursorPosColumn, _CursorPosRow, enErrorReason.EditGeneral);

                                if (string.IsNullOrEmpty(l2))
                                {
                                    UserEdited(this, ntxt, _CursorPosColumn, _CursorPosRow, true);
                                }
                                else
                                {
                                    NotEditableInfo(l2);
                                }
                            }
                        }
                        break;

                }

                ISIN_KeyDown = false;
            }


        }


        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_Database == null)
            {
                Forms.QuickInfo.Close();
                return;
            }

            lock (Lock_UserAction)
            {
                if (ISIN_MouseMove) { return; }
                ISIN_MouseMove = true;


                CellOnCoordinate(e.X, e.Y, out _MouseOverColumn, out _MouseOverRow);

                if (e.Button != System.Windows.Forms.MouseButtons.None)
                {
                    _Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
                }
                else
                {

                    var T = "";
                    if (_MouseOverColumn != null && e.Y < HeadSize())
                    {
                        T = _MouseOverColumn.QickInfoText(string.Empty);
                    }
                    else if (_MouseOverColumn != null && _MouseOverRow != null)
                    {
                        if (_MouseOverColumn.Format.NeedTargetDatabase())
                        {
                            if (_MouseOverColumn.LinkedDatabase() != null)
                            {
                                switch (_MouseOverColumn.Format)
                                {
                                    case enDataFormat.Columns_für_LinkedCellDropdown:

                                        var Txt = _MouseOverRow.CellGetString(_MouseOverColumn);
                                        if (int.TryParse(Txt, out var ColKey))
                                        {
                                            var C = _MouseOverColumn.LinkedDatabase().Column.SearchByKey(ColKey);
                                            if (C != null) { T = C.QickInfoText(_MouseOverColumn.Caption + ": " + C.Caption); }
                                        }
                                        break;

                                    case enDataFormat.LinkedCell:
                                    case enDataFormat.Values_für_LinkedCellDropdown:
                                        var LinkedData = CellCollection.LinkedCellData(_MouseOverColumn, _MouseOverRow, false, false, false);
                                        if (LinkedData.Item1 != null) { T = LinkedData.Item1.QickInfoText(_MouseOverColumn.ReadableText() + " bei " + LinkedData.Item1.ReadableText() + ":"); }
                                        break;

                                    default:
                                        Develop.DebugPrint(_MouseOverColumn.Format);
                                        break;
                                }




                            }
                            else
                            {
                                T = "Verknüpfung zur Ziel-Datenbank fehlerhaft.";
                            }
                        }

                        else if (_Database.IsAdministrator())
                        {
                            T = Database.UndoText(_MouseOverColumn, _MouseOverRow);
                        }
                    }


                    T = T.Trim();
                    T = T.Trim("<br>");
                    T = T.Trim();

                    if (!string.IsNullOrEmpty(T))
                    {
                        Forms.QuickInfo.Show(T);
                    }
                    else
                    {
                        Forms.QuickInfo.Close();
                    }



                }

                ISIN_MouseMove = false;
            }

        }


        private Rectangle DisplayRectangleWithoutSlider()
        {


            if (_Design == enBlueTableAppearance.Standard)
            {
                return new Rectangle(DisplayRectangle.Left, DisplayRectangle.Left, DisplayRectangle.Width - SliderY.Width, DisplayRectangle.Height - SliderX.Height);
            }

            return new Rectangle(DisplayRectangle.Left, DisplayRectangle.Left, DisplayRectangle.Width - SliderY.Width, DisplayRectangle.Height);
            // Return ExpandRectangle(DisplayRectangle, 0, 0, -SliderY.Width, 0)
        }


        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (_Database == null) { return; }

            lock (Lock_UserAction)
            {
                if (ISIN_MouseUp) { return; }
                ISIN_MouseUp = true;
                var ScreenX = System.Windows.Forms.Cursor.Position.X - e.X;
                var ScreenY = System.Windows.Forms.Cursor.Position.Y - e.Y;


                if (_Database == null)
                {
                    Forms.QuickInfo.Close();
                    ISIN_MouseUp = false;
                    return;
                }

                CellOnCoordinate(e.X, e.Y, out _MouseOverColumn, out _MouseOverRow);



                if (_CursorPosColumn != _MouseOverColumn || _CursorPosRow != _MouseOverRow) { Forms.QuickInfo.Close(); }



                // TXTBox_Close() NICHT! Weil sonst nach dem öffnen sofort wieder gschlossen wird
                // AutoFilter_Close() NICHT! Weil sonst nach dem öffnen sofort wieder geschlossen wird
                //ContextMenu_Close();
                FloatingInputBoxListBoxStyle.Close(this, enDesign.Form_KontextMenu);

                //EnsureVisible(_MouseOver) <-Nur MouseDown, siehe Da
                //CursorPos_Set(_MouseOver) <-Nur MouseDown, siehe Da



                ColumnViewItem ViewItem = null;

                if (_MouseOverColumn != null)
                {
                    ViewItem = _Database.ColumnArrangements[_ArrangementNr][_MouseOverColumn];
                }


                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    if (_MouseOverColumn != null)
                    {
                        if (Mouse_IsInAutofilter(ViewItem, e))
                        {
                            AutoFilter_Show(ViewItem, ScreenX, ScreenY);
                            ISIN_MouseUp = false;
                            return;
                        }
                        if (Mouse_IsInRedcueButton(ViewItem, e))
                        {
                            ViewItem._TMP_Reduced = !ViewItem._TMP_Reduced;
                            ViewItem._TMP_DrawWidth = null;
                            Invalidate();
                            ISIN_MouseUp = false;
                            return;
                        }


                    }
                }


                if (e.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    FloatingInputBoxListBoxStyle.ContextMenuShow(this, e);
                }


                ISIN_MouseUp = false;
            }
            //   End SyncLock

        }

        protected override void OnDoubleClick(System.EventArgs e)
        {
            //    base.OnDoubleClick(e); Wird komplett selsbt gehandledt und das neue Ereigniss ausgelöst


            if (_Database == null) { return; }


            lock (Lock_UserAction)
            {
                if (ISIN_DoubleClick) { return; }
                ISIN_DoubleClick = true;

                CellOnCoordinate(MousePos().X, MousePos().Y, out _MouseOverColumn, out _MouseOverRow);

                var ea = new CellDoubleClickEventArgs(_MouseOverColumn, _MouseOverRow, true);

                if (Mouse_IsInHead())
                {
                    Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());

                    DoubleClick?.Invoke(this, ea);
                    ISIN_DoubleClick = false;
                    return;
                }

                var col = _MouseOverColumn;
                var row = _MouseOverRow;

                CellOnCoordinate(MousePos().X, MousePos().Y, out _MouseOverColumn, out _MouseOverRow);



                if (col != _MouseOverColumn || row != _MouseOverRow) // Da hat das eventx z.B. die Zeile gelöscht
                {
                    ISIN_DoubleClick = false;
                    return;
                }

                DoubleClick?.Invoke(this, ea);

                if (ea.StartEdit) { Cell_Edit(_MouseOverColumn, _MouseOverRow, true); }

                ISIN_DoubleClick = false;
            }

        }
        #endregion

        #region  Ereignisse der Textbox 
        private void BB_Enter(object sender, System.EventArgs e)
        {
            if (((TextBox)sender).MultiLine) { return; }

            CloseAllComponents();

        }

        private void BB_ESC(object sender, System.EventArgs e)
        {
            BTB.Tag = null;
            BTB.Visible = false;

            BCB.Tag = null;
            BCB.Visible = false;


            CloseAllComponents();
        }



        private void BB_TAB(object sender, System.EventArgs e)
        {
            CloseAllComponents();
        }

        private void BB_LostFocus(object sender, System.EventArgs e)
        {

            if (FloatingInputBoxListBoxStyle.IsShowing(BTB) || FloatingInputBoxListBoxStyle.IsShowing(BCB)) { return; }

            CloseAllComponents();
        }


        #endregion




        public void CursorPos_Set(ColumnItem column, RowItem row, bool ensureVisible)
        {

            if (column != null) { column = Database.Column.SearchByKey(column.Key); }
            if (row != null) { row = Database.Row.SearchByKey(row.Key); }

            if (_Database.ColumnArrangements.Count == 0 || _Database.ColumnArrangements[_ArrangementNr][column] == null || !SortedRows().Contains(row))
            {
                column = null;
                row = null;
            }

            if (_CursorPosColumn == column && _CursorPosRow == row) { return; }

            _CursorPosColumn = column;
            _CursorPosRow = row;

            if (ensureVisible) { EnsureVisible(_CursorPosColumn, _CursorPosRow); }
            Invalidate();

            OnCursorPosChanged(new CellEventArgs(_CursorPosColumn, _CursorPosRow));
        }


        private void OnCursorPosChanged(CellEventArgs e)
        {
            CursorPosChanged?.Invoke(this, e);
        }

        protected override void OnMouseLeave(System.EventArgs e)
        {
            base.OnMouseLeave(e);


            if (_Database == null) { return; }

            lock (Lock_UserAction)
            {
                if (ISIN_MouseLeave) { return; }
                ISIN_MouseLeave = true;
                QuickInfo.Close();


                ISIN_MouseLeave = false;
            }
        }


        protected override void OnMouseEnter(System.EventArgs e)
        {
            base.OnMouseEnter(e);

            if (_Database == null) { return; }

            lock (Lock_UserAction)
            {
                if (ISIN_MouseEnter) { return; }
                ISIN_MouseEnter = true;
                QuickInfo.Close();

                ISIN_MouseEnter = false;
            }
        }



        protected override void OnResize(System.EventArgs e)
        {
            base.OnResize(e);

            if (_Database == null) { return; }

            lock (Lock_UserAction)
            {
                if (ISIN_Resize) { return; }
                ISIN_Resize = true;
                Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());

                Invalidate_AllDraw(false);
                ISIN_Resize = false;
            }
        }

        public void WriteColumnArrangementsInto(ComboBox _ColumnArrangementSelector)
        {

            if (InvokeRequired)
            {
                Invoke(new Action(() => WriteColumnArrangementsInto(_ColumnArrangementSelector)));
                return;
            }

            if (_ColumnArrangementSelector != null)
            {
                _ColumnArrangementSelector.Item.Clear();
                _ColumnArrangementSelector.Enabled = false;
                _ColumnArrangementSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            }

            if (_Database == null || _Design != enBlueTableAppearance.Standard || _ColumnArrangementSelector == null)
            {
                if (_ColumnArrangementSelector != null)
                {
                    _ColumnArrangementSelector.Enabled = false;
                    _ColumnArrangementSelector.Text = string.Empty;
                }
                return;
            }

            foreach (var ThisArrangement in _Database.ColumnArrangements)
            {
                if (ThisArrangement != null)
                {
                    if (_ColumnArrangementSelector != null && _Database.PermissionCheck(ThisArrangement.PermissionGroups_Show, null))
                    {
                        _ColumnArrangementSelector.Item.Add(new TextListItem(_Database.ColumnArrangements.IndexOf(ThisArrangement).ToString(), ThisArrangement.Name));
                    }
                }
            }

            if (_ColumnArrangementSelector != null)
            {
                _ColumnArrangementSelector.Enabled = Convert.ToBoolean(_ColumnArrangementSelector.Item.Count > 1);
                if (_ColumnArrangementSelector.Item.Count > 0)
                {
                    _ColumnArrangementSelector.Text = _ArrangementNr.ToString();
                }
                else
                {
                    _ColumnArrangementSelector.Text = string.Empty;
                }
            }
        }




        #region  Store and Restore View 
        public string ViewToString()
        {
            string tmp = null;

            var x = "{";
            //   x = x & "<Filename>" & _Database.Filename
            x = x + "ArrangementNr=" + _ArrangementNr;

            tmp = Filter.ToString();

            if (tmp.Length > 2)
            {
                x = x + ", Filters=" + Filter;
            }
            x = x + ", SliderX=" + SliderX.Value;
            x = x + ", SliderY=" + SliderY.Value;



            if (_sortDefinitionTemporary?.Columns != null)
            {

                x = x + ", TempSort=" + _sortDefinitionTemporary;

            }

            x = x + ", CursorPos=" + CellCollection.KeyOfCell(_CursorPosColumn, _CursorPosRow);

            return x + "}";
        }

        public void ParseView(string Value)
        {

            if (string.IsNullOrEmpty(Value)) { return; }



            var Beg = 0;

            do
            {
                Beg += 1;
                if (Beg > Value.Length) { break; }
                var T = Value.ParseTag(Beg);
                var pairValue = Value.ParseValue(T, Beg);
                Beg = Beg + T.Length + pairValue.Length + 2;
                switch (T)
                {
                    case "arrangementnr":
                        Arrangement = int.Parse(pairValue);
                        break;

                    case "filters":
                        Filter.Parse(pairValue);
                        break;

                    case "sliderx":
                        SliderX.Maximum = Math.Max(SliderX.Maximum, int.Parse(pairValue));
                        SliderX.Value = int.Parse(pairValue);
                        break;

                    case "slidery":
                        SliderY.Maximum = Math.Max(SliderY.Maximum, int.Parse(pairValue));
                        SliderY.Value = int.Parse(pairValue);
                        break;

                    case "cursorpos":
                        Database.Cell.DataOfCellKey(pairValue, out var column, out var row);
                        CursorPos_Set(column, row, false);
                        break;

                    case "tempsort":
                        _sortDefinitionTemporary = new RowSortDefinition(_Database, pairValue);
                        break;

                    default:
                        Develop.DebugPrint(enFehlerArt.Fehler, "Tag unbekannt: " + T);
                        break;
                }
            } while (true);

            Invalidate_RowSort(); // beim Parsen wirft der Filter kein Event ab

        }


        private void _Database_StoreView(object sender, System.EventArgs e)
        {

            //if (!string.IsNullOrEmpty(_StoredView)) { Develop.DebugPrint("Stored View nicht Empty!"); }

            _StoredView = ViewToString();

        }
        private void _Database_RowKeyChanged(object sender, KeyChangedEventArgs e)
        {
            // Ist aktuell nur möglich, wenn Pending Changes eine neue Zeile machen
            if (string.IsNullOrEmpty(_StoredView)) { return; }
            _StoredView = _StoredView.Replace("RowKey=" + e.KeyOld + "}", "RowKey=" + e.KeyNew + "}");
        }


        private void _Database_ColumnKeyChanged(object sender, KeyChangedEventArgs e)
        {
            // Ist aktuell nur möglich,wenn Pending Changes eine neue Zeile machen
            if (string.IsNullOrEmpty(_StoredView)) { return; }
            _StoredView = ColumnCollection.ChangeKeysInString(_StoredView, e.KeyOld, e.KeyNew);
        }



        #endregion



        #region  Arrangement Only 










        public void Arrangement_Add()
        {
            string e = null;

            var MitVorlage = false;

            if (_ArrangementNr > 0)
            {
                MitVorlage = Convert.ToBoolean(Forms.MessageBox.Show("<b>Neue Spaltenanordnung erstellen:</b><br>Wollen sie die aktuelle Ansicht kopieren?", enImageCode.Frage, "Ja", "Nein") == 0);
            }

            if (_Database.ColumnArrangements.Count < 1)
            {
                _Database.ColumnArrangements.Add(new ColumnViewCollection(_Database, "", ""));
            }

            if (MitVorlage)
            {
                e = InputBox.Show("Die aktuelle Ansicht wird <b>kopiert</b>.<br><br>Geben sie den Namen<br>der neuen Anordnung ein:", "", enDataFormat.Text);
                if (string.IsNullOrEmpty(e)) { return; }
                _Database.ColumnArrangements.Add(new ColumnViewCollection(_Database, _Database.ColumnArrangements[_ArrangementNr].ToString(), e));
            }
            else
            {
                e = InputBox.Show("Geben sie den Namen<br>der neuen Anordnung ein:", "", enDataFormat.Text);
                if (string.IsNullOrEmpty(e)) { return; }
                _Database.ColumnArrangements.Add(new ColumnViewCollection(_Database, "", e));
            }

            //Invalidatex_ExternalContols();
        }

        #endregion

        public bool EnsureVisible(string CellKey)
        {
            Database.Cell.DataOfCellKey(CellKey, out var column, out var row);
            return EnsureVisible(column, row);
        }

        public bool EnsureVisible(ColumnItem Column, RowItem Row)
        {
            var ok1 = EnsureVisible(_Database.ColumnArrangements[_ArrangementNr][Column]);
            var ok2 = EnsureVisible(Row);
            return ok1 && ok2;
        }

        private bool EnsureVisible(RowItem vRow)
        {

            if (!SortedRows().Contains(vRow)) { return false; }

            if (vRow.TMP_Y == null && !ComputeAllCellPositions()) { return false; }


            var r = DisplayRectangleWithoutSlider();
            ComputeAllCellPositions();

            if (vRow.TMP_Y < HeadSize())
            {
                SliderY.Value = SliderY.Value + (int)vRow.TMP_Y - HeadSize();
            }
            else if (vRow.TMP_Y + Row_DrawHeight(vRow, r) > r.Height)
            {
                SliderY.Value = SliderY.Value + (int)vRow.TMP_Y + Row_DrawHeight(vRow, r) - r.Height;
            }

            return true;
        }


        private bool EnsureVisible(ColumnViewItem ViewItem)
        {

            if (ViewItem == null || ViewItem.Column == null) { return false; }

            if (ViewItem.OrderTMP_Spalte_X1 == null && !ComputeAllCellPositions()) { return false; }

            var r = DisplayRectangleWithoutSlider();
            ComputeAllCellPositions();


            if (ViewItem.ViewType == enViewType.PermanentColumn)
            {
                if (ViewItem.OrderTMP_Spalte_X1 + Column_DrawWidth(ViewItem, r) <= r.Width) { return true; }
                //Develop.DebugPrint(enFehlerArt.Info,"Unsichtbare Wiederholungsspalte: " + ViewItem.Column.Name);
                return false;
            }


            if (ViewItem.OrderTMP_Spalte_X1 < WiederHolungsSpaltenWidth)
            {
                SliderX.Value = SliderX.Value + (int)ViewItem.OrderTMP_Spalte_X1 - WiederHolungsSpaltenWidth;
            }
            else if (ViewItem.OrderTMP_Spalte_X1 + Column_DrawWidth(ViewItem, r) > r.Width)
            {
                SliderX.Value = SliderX.Value + (int)ViewItem.OrderTMP_Spalte_X1 + Column_DrawWidth(ViewItem, r) - r.Width;
            }

            return true;
        }

        [DefaultValue(1)]
        [Description("Welche Spaltenanordnung angezeigt werden soll")]
        public int Arrangement
        {
            get
            {
                return _ArrangementNr;
            }
            set
            {
                if (value != _ArrangementNr)
                {
                    _ArrangementNr = value;
                    //if (_ColumnArrangementSelector != null)
                    //{
                    //    _ColumnArrangementSelector.Text = _ArrangementNr.ToString();
                    //}
                    //InternalColumnArrangementSelector.Text = _ArrangementNr.ToString();
                    Invalidate_HeadSize();
                    Invalidate_AllDraw(false);
                    Invalidate();
                    OnViewChanged();
                    CursorPos_Set(_CursorPosColumn, _CursorPosRow, true);
                }

            }
        }




        private void CellOnCoordinate(int Xpos, int Ypos, out ColumnItem Column, out RowItem Row)
        {
            Column = OnCoordinateColumn(Xpos);
            Row = OnCoordinateRow(Ypos);
        }

        private ColumnItem OnCoordinateColumn(int Xpos)
        {
            if (_Database == null || _Database.ColumnArrangements.Count - 1 < _ArrangementNr) { return null; }

            foreach (var ThisViewItem in _Database.ColumnArrangements[_ArrangementNr])
            {

                if (ThisViewItem?.Column != null)
                {
                    if (Xpos >= ThisViewItem.OrderTMP_Spalte_X1 && Xpos <= ThisViewItem.OrderTMP_Spalte_X1 + Column_DrawWidth(ThisViewItem, DisplayRectangleWithoutSlider()))
                    {
                        return ThisViewItem.Column;
                    }
                }

            }

            return null;
        }



        private int HeadSize()
        {
            if (_HeadSize != null) { return (int)_HeadSize; }

            if (_Database.ColumnArrangements.Count - 1 < _ArrangementNr)
            {
                _HeadSize = 0;
                return 0;
            }


            if (_Design == enBlueTableAppearance.OnlyMainColumnWithoutHead || _Database.ColumnArrangements[_ArrangementNr].Count() - 1 < 0)
            {
                _HeadSize = 0;
                return 0;
            }


            _HeadSize = 16;

            foreach (var ThisViewItem in _Database.ColumnArrangements[_ArrangementNr])
            {
                if (ThisViewItem?.Column != null)
                {
                    _HeadSize = Math.Max((int)_HeadSize, (int)ColumnHead_Size(ThisViewItem.Column).Height);
                }
            }

            _HeadSize += 8;
            _HeadSize += 18;




            return (int)_HeadSize;
        }




        private bool IsOnScreen(ColumnViewItem ViewItem, Rectangle DisplayRectangleWOSlider)
        {
            if (ViewItem == null) { return false; }

            if (_Design == enBlueTableAppearance.Standard || _Design == enBlueTableAppearance.OnlyMainColumnWithoutHead)
            {
                if (ViewItem.OrderTMP_Spalte_X1 + Column_DrawWidth(ViewItem, DisplayRectangleWOSlider) >= 0 && ViewItem.OrderTMP_Spalte_X1 <= DisplayRectangleWOSlider.Width) { return true; }
            }
            else
            {
                Develop.DebugPrint(_Design);
            }

            return false;
        }

        private bool IsOnScreen(ColumnItem Column, RowItem Row, Rectangle DisplayRectangleWOSlider)
        {
            if (!IsOnScreen(_Database.ColumnArrangements[_ArrangementNr][Column], DisplayRectangleWOSlider)) { return false; }
            if (!IsOnScreen(Row, DisplayRectangleWOSlider)) { return false; }
            return true;
        }

        private bool IsOnScreen(RowItem vrow, Rectangle DisplayRectangleWOSlider)
        {
            if (vrow == null) { return false; }
            if (vrow.TMP_Y + Row_DrawHeight(vrow, DisplayRectangleWOSlider) >= HeadSize() && vrow.TMP_Y <= DisplayRectangleWOSlider.Height) { return true; }
            return false;
        }


        public static int tmpColumnContentWidth(ColumnItem Column, BlueFont CellFont, int Pix16)
        {

            if (Column.TMP_ColumnContentWidth != null) { return (int)Column.TMP_ColumnContentWidth; }

            Column.TMP_ColumnContentWidth = 0;

            foreach (var ThisRowItem in Column.Database.Row)
            {
                if (ThisRowItem != null && !ThisRowItem.CellIsNullOrEmpty(Column))
                {
                    var t = Column.TMP_ColumnContentWidth; // ja, dank Multithreading kann es sein, dass hier das hier null ist
                    if (t == null) { t = 0; }
                    Column.TMP_ColumnContentWidth = Math.Max((int)t, Cell_ContentSize(Column, ThisRowItem, CellFont, Pix16).Width);
                }
            }

            if (Column.TMP_ColumnContentWidth is int w) { return w; }
            return 0;
        }




        private int Column_DrawWidth(ColumnViewItem ViewItem, Rectangle DisplayRectangleWOSlider)
        {
            // Hier wird die ORIGINAL-Spalte gezeichnet, nicht die FremdZelle!!!!
            //Develop.DebugPrint_InvokeRequired(InvokeRequired, true);

            if (ViewItem._TMP_DrawWidth != null) { return (int)ViewItem._TMP_DrawWidth; }

            if (ViewItem == null || ViewItem.Column == null) { return 0; }

            if (_Design == enBlueTableAppearance.OnlyMainColumnWithoutHead)
            {
                ViewItem._TMP_DrawWidth = DisplayRectangleWOSlider.Width - 1;
                return (int)ViewItem._TMP_DrawWidth;
            }


            if (ViewItem._TMP_Reduced)
            {
                ViewItem._TMP_DrawWidth = ViewItem.Column.BildCode_ConstantHeight + 2;
            }

            else
            {

                if (ViewItem.ViewType == enViewType.PermanentColumn)
                {
                    ViewItem._TMP_DrawWidth = Math.Min(tmpColumnContentWidth(ViewItem.Column, _Cell_Font, Pix16), (int)(DisplayRectangleWOSlider.Width * 0.3));
                }
                else
                {
                    ViewItem._TMP_DrawWidth = Math.Min(tmpColumnContentWidth(ViewItem.Column, _Cell_Font, Pix16), (int)(DisplayRectangleWOSlider.Width * 0.75));
                }
            }

            ViewItem._TMP_DrawWidth = Math.Max((int)ViewItem._TMP_DrawWidth, 18); // Mindesten so groß wie der Autofilter;
            ViewItem._TMP_DrawWidth = Math.Max((int)ViewItem._TMP_DrawWidth, (int)ColumnHead_Size(ViewItem.Column).Width);

            return (int)ViewItem._TMP_DrawWidth;
        }
        private int Row_DrawHeight(RowItem vrow, Rectangle DisplayRectangleWOSlider)
        {
            if (_Design == enBlueTableAppearance.OnlyMainColumnWithoutHead) { return Cell_ContentSize(_Database.Column[0], vrow, _Cell_Font, Pix16).Height; }
            if (vrow.TMP_DrawHeight != null) { return (int)vrow.TMP_DrawHeight; }
            var tmp = Pix18;

            foreach (var ThisViewItem in _Database.ColumnArrangements[_ArrangementNr])
            {
                if (ThisViewItem?.Column != null)
                {
                    if (!vrow.CellIsNullOrEmpty(ThisViewItem.Column))
                    {
                        tmp = Math.Max(tmp, Cell_ContentSize(ThisViewItem.Column, vrow, _Cell_Font, Pix16).Height);
                    }
                }
            }
            vrow.TMP_DrawHeight = Math.Min(tmp, (int)(DisplayRectangleWOSlider.Height * 0.9) - HeadSize());
            vrow.TMP_DrawHeight = Math.Max((int)vrow.TMP_DrawHeight, Pix18);

            return (int)vrow.TMP_DrawHeight;
        }




        private RowSortDefinition SortUsed()
        {
            if (_sortDefinitionTemporary?.Columns != null) { return _sortDefinitionTemporary; }
            if (_Database.SortDefinition?.Columns != null) { return _Database.SortDefinition; }
            return null;
        }




        public List<RowItem> SortedRows()
        {
            if (AreRowsSorted()) { return _SortedRows; }

            if (Database == null) { return new List<RowItem>(); }

            _SortedRows = Database.Row.CalculateSortedRows(Filter, SortUsed());

            if (!_SortedRows.SequenceEqual(_SortedRowsBefore))
            {
                _SortedRowsBefore.Clear();
                if (_SortedRows != null) { _SortedRowsBefore.AddRange(_SortedRows); }
                EnsureVisible(_CursorPosColumn, _CursorPosRow);
                //          CursorPos_Set(null, null, true);
                OnRowsSorted();
            }

            return SortedRows(); // Rekursiver aufruf. Manchmal funktiniert OnRowsSorted nicht...
        }

        private void OnRowsSorted()
        {
            RowsSorted?.Invoke(this, System.EventArgs.Empty);
        }


        private void OnViewChanged()
        {
            ViewChanged?.Invoke(this, System.EventArgs.Empty);
        }

        private void OnDatabaseChanged()
        {
            DatabaseChanged?.Invoke(this, System.EventArgs.Empty);
        }

        private void OnColumnArrangementChanged()
        {
            ColumnArrangementChanged?.Invoke(this, System.EventArgs.Empty);
        }



        public RowItem View_RowFirst()
        {
            if (_Database == null) { return null; }

            if (SortedRows().Count == 0) { return null; }
            return SortedRows()[0];
        }
        public RowItem View_RowLast()
        {
            if (_Database == null) { return null; }

            if (SortedRows().Count == 0) { return null; }
            return SortedRows()[SortedRows().Count - 1];
        }


        private bool Autofilter_Sinnvoll(ColumnViewItem vcolumn)
        {

            if (vcolumn.TMP_AutoFilterSinnvoll != null) { return (bool)vcolumn.TMP_AutoFilterSinnvoll; }

            for (var rowcount = 0; rowcount <= SortedRows().Count - 2; rowcount++)
            {
                if (SortedRows()[rowcount].CellGetString(vcolumn.Column) != SortedRows()[rowcount + 1].CellGetString(vcolumn.Column))
                {
                    vcolumn.TMP_AutoFilterSinnvoll = true;
                    return true;
                }
            }

            vcolumn.TMP_AutoFilterSinnvoll = false;
            return false;
        }




        private RowItem OnCoordinateRow(int YPos)
        {
            if (_Database == null || YPos <= HeadSize()) { return null; }


            foreach (var ThisRowItem in _Database.Row)
            {
                if (ThisRowItem != null)
                {
                    if (YPos >= ThisRowItem.TMP_Y && YPos <= ThisRowItem.TMP_Y + Row_DrawHeight(ThisRowItem, DisplayRectangleWithoutSlider()))
                    {
                        return ThisRowItem;
                    }
                }
            }

            return null;
        }



        private void Invalidate_AllDraw(bool AllOrder)
        {
            if (_Database == null) { return; }


            foreach (var ThisRowItem in _Database.Row)
            {
                if (ThisRowItem != null) { Invalidate_TMPDrawHeight(ThisRowItem); }
            }


            if (AllOrder)
            {

                foreach (var ThisArrangement in _Database.ColumnArrangements)
                {
                    if (ThisArrangement != null)
                    {
                        foreach (var ThisViewItem in ThisArrangement)
                        {
                            Invalidate_DrawWidth(ThisViewItem);
                        }


                        if (_Database.ColumnArrangements.IndexOf(ThisArrangement) == 0)
                        {
                            _Database.ColumnArrangements[0].ShowAllColumns(_Database);
                        }
                    }
                }


                Invalidate_HeadSize();

            }
            else
            {

                if (_ArrangementNr < _Database.ColumnArrangements.Count - 1)
                {

                    foreach (var ThisViewItem in _Database.ColumnArrangements[_ArrangementNr])
                    {
                        Invalidate_DrawWidth(ThisViewItem);
                    }

                }

            }


        }


        private void Invalidatex_DrawWidth(ColumnItem vcolumn)
        {
            Invalidate_DrawWidth(_Database.ColumnArrangements[_ArrangementNr][vcolumn]);
        }
        private void Invalidate_DrawWidth(ColumnViewItem ViewItem)
        {
            if (ViewItem == null) { return; }
            ViewItem._TMP_DrawWidth = null;
        }

        public void Invalidate_HeadSize()
        {
            if (_HeadSize != null) { Invalidate(); }
            _HeadSize = null;
        }

        private void Invalidate_TMPDrawHeight(RowItem vrow)
        {
            if (vrow == null) { return; }
            vrow.TMP_DrawHeight = null;
        }

        private void _Database_ColumnContentChanged(object sender, ListEventArgs e)
        {
            Invalidate_AllDraw(true);
            Invalidate_RowSort();
            Invalidate();
        }



        public RowItem View_NextRow(RowItem vrow)
        {
            if (_Database == null) { return null; }

            var RowNr = SortedRows().IndexOf(vrow);
            if (RowNr < 0 || RowNr >= SortedRows().Count - 1) { return null; }
            return SortedRows()[RowNr + 1];
        }


        public RowItem View_PreviousRow(RowItem vrow)
        {
            if (_Database == null) { return null; }


            var RowNr = SortedRows().IndexOf(vrow);
            if (RowNr < 1) { return null; }
            return SortedRows()[RowNr - 1];

        }



        /// <summary>
        /// Berechnet die Zelle, ausgehend von einer Zellenposition. Dabei wird die Columns und Zeilensortierung berücksichtigt.
        /// Gibt des keine Nachbarszelle, wird die Eingangszelle zurückgegeben.
        /// </summary>
        /// <remarks></remarks>
        private void Neighbour(ColumnItem column, RowItem row, enDirection Direction, out ColumnItem NewColumn, out RowItem NewRow)
        {

            NewColumn = column;
            NewRow = row;



            if (_Design == enBlueTableAppearance.OnlyMainColumnWithoutHead)
            {
                if (Direction != enDirection.Oben && Direction != enDirection.Unten)
                {
                    NewColumn = _Database.Column[0];
                    return;
                }
            }



            if (NewColumn != null)
            {
                if (Convert.ToBoolean(Direction & enDirection.Links))
                {
                    if (_Database.ColumnArrangements[_ArrangementNr].PreviousVisible(NewColumn) != null)
                    {
                        NewColumn = _Database.ColumnArrangements[_ArrangementNr].PreviousVisible(NewColumn);
                    }
                }

                if (Convert.ToBoolean(Direction & enDirection.Rechts))
                {
                    if (_Database.ColumnArrangements[_ArrangementNr].NextVisible(NewColumn) != null)
                    {
                        NewColumn = _Database.ColumnArrangements[_ArrangementNr].NextVisible(NewColumn);
                    }
                }
            }

            if (NewRow != null)
            {
                if (Convert.ToBoolean(Direction & enDirection.Oben))
                {
                    if (View_PreviousRow(NewRow) != null) { NewRow = View_PreviousRow(NewRow); }
                }

                if (Convert.ToBoolean(Direction & enDirection.Unten))
                {
                    if (View_NextRow(NewRow) != null) { NewRow = View_NextRow(NewRow); }
                }

            }
        }



        private void _Database_SortParameterChanged(object sender, System.EventArgs e)
        {
            Invalidate_RowSort();
            Invalidate();
        }

        internal void FilterChanged(object sender, System.EventArgs e)
        {
            Invalidate_RowSort();
            Invalidate();
        }

        public bool Mouse_IsInHead()
        {
            return Convert.ToBoolean(MousePos().Y <= HeadSize());
        }

        private bool Mouse_IsInAutofilter(ColumnViewItem ViewItem, System.Windows.Forms.MouseEventArgs e)
        {
            if (ViewItem == null) { return false; }
            if (ViewItem._TMP_AutoFilterLocation.Width == 0) { return false; }
            if (!ViewItem.Column.AutoFilter_möglich()) { return false; }
            return ViewItem._TMP_AutoFilterLocation.Contains(e.X, e.Y);
        }


        private bool Mouse_IsInRedcueButton(ColumnViewItem ViewItem, System.Windows.Forms.MouseEventArgs e)
        {
            if (ViewItem == null) { return false; }
            if (ViewItem._TMP_ReduceLocation.Width == 0) { return false; }
            return ViewItem._TMP_ReduceLocation.Contains(e.X, e.Y);
        }




        private SizeF ColumnCaptionText_Size(ColumnItem Column)
        {

            if (Column.TMP_CaptionText_Size.Width > 0) { return Column.TMP_CaptionText_Size; }
            if (_Column_Font == null) { return new SizeF(Pix16, Pix16); }


            Column.TMP_CaptionText_Size = BlueFont.MeasureString(Column.Caption.Replace("\r", "\r\n"), _Column_Font.Font());
            return Column.TMP_CaptionText_Size;
        }

        private bool UserEdit_NewRowAllowed()
        {

            //if (Thread.CurrentThread.IsBackground) { return false; }

            if (_Database == null) { return false; }
            //if (!Enabled) { return false; }

            if (_Database.Column[0] == null) { return false; }

            if (_Design == enBlueTableAppearance.OnlyMainColumnWithoutHead) { return false; }
            if (_Database.ColumnArrangements.Count == 0) { return false; }

            if (_Database.ColumnArrangements[_ArrangementNr][_Database.Column[0]] == null) { return false; }
            if (!_Database.PermissionCheck(_Database.PermissionGroups_NewRow, null)) { return false; }

            if (!CellCollection.UserEditPossible(_Database.Column[0], null, enErrorReason.EditNormaly)) { return false; }
            return true;
        }


        private void _Database_RowCountChanged(object sender, System.EventArgs e)
        {
            Invalidate_RowSort();
            CursorPos_Set(_CursorPosColumn, _CursorPosRow, false);
            Invalidate();
        }

        private void _Database_ViewChanged(object sender, System.EventArgs e)
        {
            InitializeSkin(); // Sicher ist sicher, um die neuen Schrift-Größen zu haben.
            Invalidate_HeadSize();
            Invalidate_AllDraw(true);
            Invalidate_RowSort();
            CursorPos_Set(_CursorPosColumn, _CursorPosRow, false);
            Invalidate();
        }

        protected override void OnVisibleChanged(System.EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (_Database == null) { return; }

            lock (Lock_UserAction)
            {
                if (ISIN_VisibleChanged) { return; }
                ISIN_VisibleChanged = true;
                Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
                ISIN_VisibleChanged = false;
            }

        }





        private void _Database_SavedToDisk(object sender, System.EventArgs e)
        {
            Invalidate();
        }

        private void _Database_ProgressbarInfo(object sender, ProgressbarEventArgs e)
        {

            if (e.Ends)
            {
                PG?.Close();
                return;
            }


            if (e.Beginns)
            {
                PG = Progressbar.Show(e.Name, e.Count);
                return;
            }

            PG.Update(e.Current);

        }

        public void OpenSearchAndReplace()
        {

            if (_searchAndReplace == null || _searchAndReplace.IsDisposed || !_searchAndReplace.Visible)
            {
                _searchAndReplace = new SearchAndReplace(this);
                _searchAndReplace.Show();
            }

        }

        public void GetContextMenuItems(System.Windows.Forms.MouseEventArgs e, ItemCollectionList Items, out object HotItem, List<string> Tags, ref bool Cancel, ref bool Translate)
        {
            HotItem = null;

            if (_Database.IsParsing())
            {
                Cancel = true;
                return;
            }

            Database?.Load_Reload();




            CellOnCoordinate(e.X, e.Y, out _MouseOverColumn, out _MouseOverRow);

            Tags.TagSet("CellKey", CellCollection.KeyOfCell(_MouseOverColumn, _MouseOverRow));

            if (_MouseOverColumn != null)
            {
                Tags.TagSet("ColumnKey", _MouseOverColumn.Key.ToString());
            }


            if (_MouseOverRow != null)
            {
                Tags.TagSet("RowKey", _MouseOverRow.Key.ToString());
            }




            //OnContextMenuInit(new ContextMenuInitEventArgs(, UserMenu));



        }


        public void OnContextMenuInit(ContextMenuInitEventArgs e)
        {
            ContextMenuInit?.Invoke(this, e);
        }

        private SizeF ColumnHead_Size(ColumnItem column)
        {
            float wi = 0;
            float he = 0;

            if (column.CaptionBitmap != null && column.CaptionBitmap.Width > 10)
            {
                wi = Math.Max(50, ColumnCaptionText_Size(column).Width + 4);
                he = 50 + ColumnCaptionText_Size(column).Height + 3;
            }
            else
            {
                wi = ColumnCaptionText_Size(column).Height + 4;
                he = ColumnCaptionText_Size(column).Width + 3;
            }

            if (!string.IsNullOrEmpty(column.Ueberschrift3))
            {
                he += ColumnCaptionSizeY * 3;
            }
            else if (!string.IsNullOrEmpty(column.Ueberschrift2))
            {
                he += ColumnCaptionSizeY * 2;
            }
            else if (!string.IsNullOrEmpty(column.Ueberschrift1))
            {
                he += ColumnCaptionSizeY;
            }

            return new SizeF(wi, he);
        }

        private void NotEditableInfo(string Reason)
        {
            Forms.Notification.Show(Reason, enImageCode.Kreuz);
            // QickInfo beisst sich mit den letzten Änderungen Quickinfo
            //DialogBoxes.QuickInfo.Show("<IMAGECODE=Stift|16||1> " + Reason);
        }





        public static Size Cell_ContentSize(ColumnItem Column, RowItem Row, BlueFont CellFont, int Pix16)
        {

            if (Column.Format == enDataFormat.LinkedCell)
            {
                var LinkedData = CellCollection.LinkedCellData(Column, Row, false, false, false);
                if (LinkedData.Item1 != null && LinkedData.Item2 != null) { return Cell_ContentSize(LinkedData.Item1, LinkedData.Item2, CellFont, Pix16); }
                return new Size(Pix16, Pix16);
            }


            var _ContentSize = Column.Database.Cell.GetSizeOfCellContent(Column, Row);
            if (_ContentSize.Width > 0 && _ContentSize.Height > 0) { return _ContentSize; }


            if (Column.MultiLine)
            {
                var TMP = Column.Database.Cell.GetList(Column, Row);
                if (Column.ShowMultiLineInOneLine)
                {
                    _ContentSize = FormatedText_NeededSize(Column, TMP.JoinWith("; "), CellFont, enShortenStyle.Replaced, Pix16);
                }
                else
                {

                    var TMPSize = Size.Empty;
                    foreach (var ThisString in TMP)
                    {
                        TMPSize = FormatedText_NeededSize(Column, ThisString, CellFont, enShortenStyle.Replaced, Pix16);
                        _ContentSize.Width = Math.Max(TMPSize.Width, _ContentSize.Width);
                        _ContentSize.Height += Math.Max(TMPSize.Height, Pix16);

                    }

                }

            }
            else
            {

                var _String = Column.Database.Cell.GetString(Column, Row);
                _ContentSize = FormatedText_NeededSize(Column, _String, CellFont, enShortenStyle.Replaced, Pix16);
            }


            _ContentSize.Width = Math.Max(_ContentSize.Width, Pix16);
            _ContentSize.Height = Math.Max(_ContentSize.Height, Pix16);


            if (Skin.Scale == 1 && LanguageTool.Translation == null) { Column.Database.Cell.SetSizeOfCellContent(Column, Row, _ContentSize); }

            return _ContentSize;
        }


        public void DoUndo(ColumnItem Column, RowItem Row)
        {
            if (Column == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Spalte ungültig!<br>" + Database.Filename); }
            if (Row == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Zeile ungültig!<br>" + Database.Filename); }

            if (Column.Format == enDataFormat.LinkedCell)
            {
                var LinkedData = CellCollection.LinkedCellData(Column, Row, false, true, false);
                if (LinkedData.Item1 != null && LinkedData.Item2 != null) { DoUndo(LinkedData.Item1, LinkedData.Item2); }
                return;
            }

            var CellKey = CellCollection.KeyOfCell(Column, Row);

            var i = UndoItems(CellKey);

            if (i.Count < 1)
            {
                Forms.MessageBox.Show("Keine vorherigen Inhalte<br>(mehr) vorhanden.", enImageCode.Information, "OK");
                return;
            }

            i.Appearance = enBlueListBoxAppearance.Listbox;


            var v = InputBoxListBoxStyle.Show("Vorherigen Eintrag wählen:", i, enAddType.None, true);
            if (v == null || v.Count != 1) { return; }

            if (v[0] == "Cancel") { return; } // =Aktueller Eintrag angeklickt



            Database.Cell.Set(Column, Row, v[0].Substring(5), false);
            Row.DoAutomatic(true, true);
        }



        public ItemCollectionList UndoItems(string CellKey)
        {
            var i = new ItemCollectionList(enBlueListBoxAppearance.KontextMenu)
            {
                CheckBehavior = enCheckBehavior.AlwaysSingleSelection
            };


            if (_Database.Works == null || _Database.Works.Count == 0) { return i; }

            var isfirst = true;


            TextListItem Las = null;
            var LasNr = -1;
            var co = 0;


            for (var z = _Database.Works.Count - 1; z >= 0; z--)
            {

                if (_Database.Works[z].CellKey == CellKey && _Database.Works[z].HistorischRelevant)
                {
                    co += 1;
                    LasNr = z;
                    if (isfirst)
                    {
                        Las = new TextListItem("Cancel", "Aktueller Text - ab " + _Database.Works[z].Date + " UTC, geändert von " + _Database.Works[z].User);
                    }
                    else
                    {
                        Las = new TextListItem(co.ToString(Constants.Format_Integer5) + _Database.Works[z].ChangedTo, "ab " + _Database.Works[z].Date + " UTC, geändert von " + _Database.Works[z].User);
                    }

                    isfirst = false;
                    if (Las != null) { i.Add(Las); }
                }
            }


            if (Las != null)
            {
                co += 1;
                i.Add(new TextListItem(co.ToString(Constants.Format_Integer5) + _Database.Works[LasNr].PreviousValue, "vor " + _Database.Works[LasNr].Date + " UTC"));
            }


            return i;
        }

        public static void CopyToClipboard(ColumnItem Column, RowItem Row, bool Meldung)
        {
            try
            {


                if (Row != null && Column.Format.CanBeCheckedByRules())
                {
                    var c = Row.CellGetString(Column);
                    c = c.Replace("\r\n", "\r");
                    c = c.Replace("\r", "\r\n");

                    System.Windows.Forms.Clipboard.SetDataObject(c, true);
                    if (Meldung) { Notification.Show("<b>" + c + LanguageTool.DoTranslate("</b><br>ist nun in der Zwischenablage.", true), enImageCode.Kopieren); }
                }
                else
                {
                    if (Meldung) { Notification.Show(LanguageTool.DoTranslate("Bei dieser Spalte nicht möglich.", true), enImageCode.Warnung); }
                }

            }
            catch
            {
                if (Meldung) { Notification.Show(LanguageTool.DoTranslate("Unerwarteter Fehler beim Kopieren.", true), enImageCode.Warnung); }
            }



        }


        public static void SearchNextText(string searchTXT, Table TableView, ColumnItem column, RowItem row, out ColumnItem foundColumn, out RowItem foundRow, bool VereinfachteSuche)
        {
            searchTXT = searchTXT.Trim();

            var ca = TableView.Database.ColumnArrangements[TableView.Arrangement];


            if (TableView.Design == enBlueTableAppearance.OnlyMainColumnWithoutHead)
            {
                ca = TableView.Database.ColumnArrangements[0];
            }


            if (row == null) { row = TableView.View_RowLast(); }
            if (column == null) { column = TableView.Database.Column.SysLocked; }

            var rowsChecked = 0;

            if (string.IsNullOrEmpty(searchTXT))
            {
                Forms.MessageBox.Show("Bitte Text zum Suchen eingeben.", enImageCode.Information, "OK");
                foundColumn = null;
                foundRow = null;
                return;
            }




            do
            {

                if (TableView.Design == enBlueTableAppearance.OnlyMainColumnWithoutHead)
                {
                    column = column.Next();
                }
                else
                {
                    column = ca.NextVisible(column);
                }



                if (column == null)
                {
                    column = ca[0].Column;

                    if (rowsChecked > TableView.Database.Row.Count() + 1)
                    {
                        foundColumn = null;
                        foundRow = null;
                        return;
                    }

                    rowsChecked++;
                    row = TableView.View_NextRow(row);
                    if (row == null) { row = TableView.View_RowFirst(); }

                }



                var ContentHolderCellColumn = column;
                var ContenHolderCellRow = row;

                if (column.Format == enDataFormat.LinkedCell)
                {
                    var LinkedData = CellCollection.LinkedCellData(column, row, false, false, false);
                    ContentHolderCellColumn = LinkedData.Item1;
                    ContenHolderCellRow = LinkedData.Item2;
                }


                var _Ist1 = string.Empty;

                if (ContenHolderCellRow != null && ContentHolderCellColumn != null)
                {
                    _Ist1 = ContenHolderCellRow.CellGetString(ContentHolderCellColumn);
                }



                if (ContentHolderCellColumn.Format == enDataFormat.Text_mit_Formatierung)
                {
                    var l = new ExtText(enDesign.TextBox, enStates.Standard);
                    l.HtmlText = _Ist1;
                    _Ist1 = l.PlainText;
                }


                // Allgemeine Prüfung
                if (!string.IsNullOrEmpty(_Ist1) && _Ist1.ToLower().Contains(searchTXT.ToLower()))
                {
                    foundColumn = column;
                    foundRow = row;
                    return;
                }

                // Prüfung mit und ohne Ersetzungen / Prefix / Suffix
                var _Ist2 = CellItem.ValuesReadable(ContentHolderCellColumn, ContenHolderCellRow, enShortenStyle.Both).JoinWithCr();
                if (!string.IsNullOrEmpty(_Ist2) && _Ist2.ToLower().Contains(searchTXT.ToLower()))
                {
                    foundColumn = column;
                    foundRow = row;
                    return;
                }

                if (VereinfachteSuche)
                {
                    var _Ist3 = _Ist2.StarkeVereinfachung(" ,");
                    var _searchTXT3 = searchTXT.StarkeVereinfachung(" ,");
                    if (!string.IsNullOrEmpty(_Ist3) && _Ist3.ToLower().Contains(_searchTXT3.ToLower()))
                    {
                        foundColumn = column;
                        foundRow = row;
                        return;
                    }
                }

            } while (true);
        }






        /// <summary>
        /// Zeichnet den Text und das Bild ohne weitere Modifikation
        /// </summary>
        /// <param name="GR"></param>
        /// <param name="column"></param>
        /// <param name="Txt"></param>
        /// <param name="ImageCode"></param>
        /// <param name="vAlign"></param>
        /// <param name="FitInRect"></param>
        /// <param name="Child"></param>
        /// <param name="DeleteBack"></param>
        /// <param name="F"></param>
        private static void Draw_FormatedText(Graphics GR, ColumnItem column, string Txt, QuickImage ImageCode, Rectangle FitInRect, bool DeleteBack, BlueFont F, enShortenStyle Style)
        {

            var tmpText = CellItem.ValueReadable(column, Txt, Style);
            var tmpAlign = CellItem.StandardAlignment(column);

            Skin.Draw_FormatedText(GR, tmpText, ImageCode, tmpAlign, FitInRect, null, DeleteBack, F, false);
        }


        /// <summary>
        /// Status des Bildes (Disabled) wird geändert
        /// </summary>
        public static void Draw_FormatedText(ColumnItem column, string Txt, Graphics GR, Rectangle FitInRect, bool DeleteBack, enShortenStyle Style, enDesign vDesign, enStates vState)
        {
            if (string.IsNullOrEmpty(Txt)) { return; }

            var SkinRow = Skin.SkinRow(vDesign, vState);

            if (SkinRow == null) { return; }

            BlueFont f = null;
            if (!string.IsNullOrEmpty(Txt)) { f = Skin.GetBlueFont(SkinRow); }



            var tmpImage = CellItem.StandardImage(column, Txt);


            if (tmpImage != null)
            {
                tmpImage = QuickImage.Get(tmpImage, Skin.AdditionalState(vState));
            }


            Draw_FormatedText(GR, column, Txt, tmpImage, FitInRect, DeleteBack, f, Style);
        }

        public static Size FormatedText_NeededSize(ColumnItem Column, string txt, BlueFont F, enShortenStyle Style, int MinSize)
        {
            var tmpText = CellItem.ValueReadable(Column, txt, Style);
            var tmpImageCode = CellItem.StandardImage(Column, txt);


            return Skin.FormatedText_NeededSize(tmpText, tmpImageCode, F, MinSize);
        }
    }
}
