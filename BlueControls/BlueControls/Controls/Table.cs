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
using BlueControls.BlueDatabaseDialogs;
using BlueControls.DialogBoxes;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollection.ItemCollectionList;
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

namespace BlueControls.Controls
{
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class Table : GenericControl, IContextMenu, IBackgroundNone
    {

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

        private Bitmap DummyBMP;
        private Graphics DummyGR;

        private const int RowCaptionFontY = 26;
        private const int RowCaptionSizeY = 50;
        private const int ColumnCaptionSizeY = 22;

        public Table()
        {

            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            SetDoubleBuffering();
            _MouseHighlight = false;
        }






        #region  Variablen 

        private DateTime _Reasondate = DateTime.Now.AddDays(-1);


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

        private readonly object Lock_UserAction = new object();
        private static BlueFont Column_Font;
        private static BlueFont Chapter_Font;
        private static BlueFont Cell_Font;
        private static BlueFont NewRow_Font;

        private FontSelectDialog _FDia = null;


        private string _StoredView = string.Empty;
        #endregion


        #region  Events 




        public event EventHandler<ContextMenuInitEventArgs> ContextMenuInit;

        public event EventHandler<ContextMenuItemClickedEventArgs> ContextMenuItemClicked;

        public new event EventHandler<CellEventArgs> DoubleClick;

        public event EventHandler<RowCancelEventArgs> EditBeforeNewRow;

        public event EventHandler<CellCancelEventArgs> EditBeforeBeginEdit;

        public event EventHandler<BeforeNewValueEventArgs> EditBeforeNewValueSet;

        public event EventHandler<CellEventArgs> CellValueChanged;

        public event EventHandler<CellEventArgs> CursorPosChanged;

        public event EventHandler ColumnArrangementChanged;

        public event EventHandler ViewChanged;

        public event EventHandler RowsSorted;

        public event EventHandler<FilterEventArgs> AutoFilterClicked;

        public event EventHandler DatabaseChanged;


        #endregion

        #region  Properties 

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

                if (value == null) { value = null; }

                if (_Database == value) { return; }
                {
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
                        _Database.DatabaseChanged -= _Database_DatabaseChanged;
                        _Database.StoreView -= _Database_StoreView;
                        _Database.RowKeyChanged -= _Database_RowKeyChanged;
                        _Database.ColumnKeyChanged -= _Database_ColumnKeyChanged;
                        _Database.RestoreView -= _Database_RestoreView;
                        _Database.Column.ItemInternalChanged -= _Database_ColumnContentChanged;
                        _Database.SortParameterChanged -= _Database_SortParameterChanged;
                        _Database.Row.RowRemoved -= _Database_RowRemoved;
                        _Database.Row.RowAdded -= _Database_RowRemoved;
                        _Database.Column.ItemRemoved -= _Database_ColumnAddedOrDeleted;
                        _Database.Column.ItemAdded -= _Database_ColumnAddedOrDeleted;
                        _Database.SavedToDisk -= _Database_SavedToDisk;
                        _Database.ColumnArrangements.ItemInternalChanged -= ColumnArrangements_ItemInternalChanged;

                        _Database.Release(false);         // Datenbank nicht reseten, weil sie ja anderweitig noch benutzt werden kann

                    }
                    _Database = value;

                    if (_Database != null)
                    {
                        _Database.Cell.CellValueChanged += _Database_CellValueChanged;
                        _Database.ConnectedControlsStopAllWorking += _Database_StopAllWorking;
                        _Database.DatabaseChanged += _Database_DatabaseChanged;
                         _Database.StoreView += _Database_StoreView;
                        _Database.RowKeyChanged += _Database_RowKeyChanged;
                        _Database.ColumnKeyChanged += _Database_ColumnKeyChanged;
                        _Database.RestoreView += _Database_RestoreView;
                        _Database.Column.ItemInternalChanged += _Database_ColumnContentChanged;
                        _Database.SortParameterChanged += _Database_SortParameterChanged;
                        _Database.Row.RowRemoved += _Database_RowRemoved;
                        _Database.Row.RowAdded += _Database_RowRemoved;
                        _Database.Column.ItemAdded += _Database_ColumnAddedOrDeleted;
                        _Database.Column.ItemRemoved += _Database_ColumnAddedOrDeleted;
                        _Database.SavedToDisk += _Database_SavedToDisk;
                        _Database.ColumnArrangements.ItemInternalChanged += ColumnArrangements_ItemInternalChanged;
                    }

                    _Database_DatabaseChanged(this, new DatabaseChangedEventArgs(false));

                }
            }
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
            Column_Font = Skin.GetBlueFont(enDesign.Table_Column, enStates.Standard);
            Chapter_Font = Skin.GetBlueFont(enDesign.Table_Cell_Chapter, enStates.Standard);
            Cell_Font = Skin.GetBlueFont(enDesign.Table_Cell, enStates.Standard);
            NewRow_Font = Skin.GetBlueFont(enDesign.Table_Cell_New, enStates.Standard);
        }



        protected override void DrawControl(Graphics GR, enStates vState)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => DrawControl(GR, vState)));
                return;
            }

            // Listboxen bekommen keinen Focus, also Tabellen auch nicht. Basta.
            if (Convert.ToBoolean(vState & enStates.Standard_HasFocus))
            {
                vState = vState ^ enStates.Standard_HasFocus;
            }

            if (_Database == null || DesignMode)
            {
                DrawWhite(GR);
                return;
            }


            lock (Lock_UserAction)
            {
                //if (_InvalidExternal) { FillExternalControls(); }
                if (Convert.ToBoolean(vState & enStates.Standard_Disabled)) { CursorPos_Set(string.Empty, false); }

                var DisplayRectangleWOSlider = DisplayRectangleWithoutSlider();


                // Haupt-Aufbau-Routine ------------------------------------
                GR.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                if (!ComputeAllCellPositions())
                {
                    DrawWhite(GR);
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
                        Draw_Table_Std(GR, vState, DisplayRectangleWOSlider, FirstVisibleRow, LastVisibleRow);

                        break;
                    case enBlueTableAppearance.OnlyMainColumnWithoutHead:
                        Draw_Table_ListboxStyle(GR, vState, DisplayRectangleWOSlider, FirstVisibleRow, LastVisibleRow);
                        break;
                    default:
                        Develop.DebugPrint(_Design);
                        break;
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
            if (_Database.ColumnArrangements == null || _ArrangementNr >= _Database.ColumnArrangements.Count) { return; }   // Kommt vor, dass spontan doch geparsed wird...

            Skin.Draw_Back(GR, enDesign.Table_And_Pad, State, DisplayRectangle, this, true);



            var PermaX = 0;


            // Durchlauf 0 = Spalten
            // Durchlauf 1 = Permanente Spalten

            for (var Durchlauf = 0 ; Durchlauf < 2 ; Durchlauf++)
            {


                foreach (var ViewItem in _Database.ColumnArrangements[_ArrangementNr])
                {
                    if (ViewItem?.Column != null && IsOnScreen(ViewItem, DisplayRectangleWOSlider))
                    {

                        if (ViewItem.Type == enViewType.PermanentColumn)
                        {
                            PermaX = Math.Max(PermaX, (int)ViewItem.OrderTMP_Spalte_X1 + (int)ViewItem._TMP_DrawWidth);
                        }


                        if ((Durchlauf == 0 & ViewItem.Type != enViewType.PermanentColumn && (int)ViewItem.OrderTMP_Spalte_X1 + (int)ViewItem._TMP_DrawWidth > PermaX) ||
                            (Durchlauf == 1 & ViewItem.Type == enViewType.PermanentColumn))
                        {

                            // Den Body-Hintergrund Zeichnen
                            Draw_Column_Body(GR, ViewItem, State, DisplayRectangleWOSlider);

                            // Den Cursor zeichnen
                            Draw_Cursor(GR, ViewItem, DisplayRectangleWOSlider);

                            // Zeilen Zeichnen (Alle Zellen)
                            for (var Zei = FirstVisibleRow ; Zei <= LastVisibleRow ; Zei++)
                            {
                                Draw_Cell(GR, ViewItem, SortedRows()[Zei], DisplayRectangleWOSlider);
                            }


                            Draw_Column_Head(GR, ViewItem, State, DisplayRectangleWOSlider);
                        }
                    }
                }

            }

            Draw_Column_Head_Captions(GR, State, DisplayRectangleWOSlider);

            // ZEilenlinien, Überschriften und "Warn-Symbol" Zeichnen

            for (var Zei = FirstVisibleRow ; Zei <= LastVisibleRow ; Zei++)
            {
                if (!string.IsNullOrEmpty(SortedRows()[Zei].TMP_Chapter))
                {
                    GR.DrawString(SortedRows()[Zei].TMP_Chapter, Chapter_Font.Font(), Chapter_Font.Brush_Color_Main, 0, (int)SortedRows()[Zei].TMP_Y - RowCaptionFontY);
                }
                if (SortedRows()[Zei].TMP_Y >= HeadSize())
                {
                    if (!SortedRows()[Zei].CellGetBoolean(_Database.Column.SysCorrect()))
                    {
                        GR.DrawImage(QuickImage.Get("Warnung|14||||||120||60").BMP, 3, (int)SortedRows()[Zei].TMP_Y + 1);
                    }
                    //else if (!SortedRows()[Zei].CellGetBoolean(_Database.Column.SysLocked()))
                    //{
                    //    GR.DrawImage(QuickImage.Get("Schlüssel|8|||FF0000|||120||60").BMP, 3, (int)SortedRows()[Zei].TMP_Y + 1);
                    //}


                }

                Draw_Line(GR, SortedRows()[Zei], State, DisplayRectangleWOSlider, _Database.ColumnArrangements[_ArrangementNr].LastThisViewItem());
            }




            //  Neue Zeile & Linienabgrenzungen- Zeichnen
            if (UserEdit_NewRowAllowed())
            {
                var ViewItem = _Database.ColumnArrangements[_ArrangementNr][Database.Column[0]];
                if (IsOnScreen(ViewItem, DisplayRectangleWOSlider))
                {
                    Skin.Draw_FormatedText(GR, "[Neue Zeile]", QuickImage.Get(enImageCode.PlusZeichen, 16), enAlignment.Left, new Rectangle((int)ViewItem.OrderTMP_Spalte_X1 + 1, (int)(-SliderY.Value + HeadSize() + 1), (int)ViewItem._TMP_DrawWidth - 2, 16 - 2), this, false, NewRow_Font);
                    //GR.DrawString("[Neue Zeile]", NewRow_Font.Font(), NewRow_Font.Brush_Color_Main, (int)ViewItem.OrderTMP_Spalte_X1, Convert.ToInt32(-SliderY.Value + HeadSize()));
                }
            }



            Skin.Draw_Border(GR, enDesign.Table_And_Pad, State, DisplayRectangleWOSlider);


            if (Database.ReloadNeeded()) { GR.DrawImage(QuickImage.Get(enImageCode.Uhr, 16).BMP, 8, 8); }
            if (Database.HasPendingChanges()) { GR.DrawImage(QuickImage.Get(enImageCode.Stift, 16).BMP, 16, 8); }
            if (Database.ReadOnly) { GR.DrawImage(QuickImage.Get(enImageCode.Schloss, 32).BMP, 16, 8); }
        }


        private void Draw_Column_Head_Captions(Graphics GR, enStates State, Rectangle DisplayRectangleWOSlider)
        {
            var BVI = new ColumnViewItem[3];
            var LCBVI = new ColumnViewItem[3];
            ColumnViewItem ViewItem;
            ColumnViewItem LastViewItem = null;

            var PermaX = 0;

            for (var X = 0 ; X < _Database.ColumnArrangements[_ArrangementNr].Count() + 1 ; X++)
            {
                if (X < _Database.ColumnArrangements[_ArrangementNr].Count())
                {
                    ViewItem = _Database.ColumnArrangements[_ArrangementNr][X];
                }
                else
                {
                    ViewItem = null;
                }


                if (ViewItem?.Type == enViewType.PermanentColumn)
                {
                    PermaX = Math.Max(PermaX, (int)ViewItem.OrderTMP_Spalte_X1 + (int)ViewItem._TMP_DrawWidth);
                }




                if (ViewItem == null ||
                    ViewItem.Type == enViewType.PermanentColumn ||
                     (int)ViewItem.OrderTMP_Spalte_X1 + (int)ViewItem._TMP_DrawWidth > PermaX)
                {


                    for (var u = 0 ; u < 3 ; u++)
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
                                if (ViewItem?.Type != enViewType.PermanentColumn && BVI[u].Type != enViewType.PermanentColumn) { LE = Math.Max(LE, PermaX); }

                                if (ViewItem?.Type != enViewType.PermanentColumn && BVI[u].Type == enViewType.PermanentColumn) { RE = Math.Max(RE, (int)LCBVI[u].OrderTMP_Spalte_X1 + (int)LCBVI[u]._TMP_DrawWidth); }


                                if (LE < RE)
                                {

                                    var r = new Rectangle(LE, u * ColumnCaptionSizeY, RE - LE, ColumnCaptionSizeY);
                                    GR.FillRectangle(new SolidBrush(BVI[u].Column.BackColor), r);
                                    GR.FillRectangle(new SolidBrush(Color.FromArgb(80, 200, 200, 200)), r);
                                    GR.DrawRectangle(Skin.Pen_LinieKräftig, r);
                                    Skin.Draw_FormatedText(GR, V, null, enAlignment.Horizontal_Vertical_Center, r, this, false, Column_Font);
                                }
                            }

                            BVI[u] = ViewItem;

                            if (ViewItem?.Type == enViewType.PermanentColumn) { LCBVI[u] = ViewItem; }
                        }
                    }
                    LastViewItem = ViewItem;
                }
            }
        }

        private void Draw_Cursor(Graphics GR, ColumnViewItem ViewItem, Rectangle DisplayRectangleWOSlider)
        {
            if (!Thread.CurrentThread.IsBackground && _CursorPosColumn != null && _CursorPosRow != null && ViewItem.Column == _CursorPosColumn)
            {
                if (IsOnScreen(_CursorPosColumn, _CursorPosRow, DisplayRectangleWOSlider))
                {
                    var r = new Rectangle((int)ViewItem.OrderTMP_Spalte_X1 + 1, (int)_CursorPosRow.TMP_Y + 1, Column_DrawWidth(ViewItem, DisplayRectangleWOSlider) - 1, Row_DrawHeight(_CursorPosRow, DisplayRectangleWOSlider) - 1);
                    if (Focused())
                    {
                        Skin.Draw_Back(GR, enDesign.Table_Cursor, enStates.Standard_HasFocus, r, this, false);
                        Skin.Draw_Border(GR, enDesign.Table_Cursor, enStates.Standard_HasFocus, r);
                    }
                    else
                    {
                        Skin.Draw_Back(GR, enDesign.Table_Cursor, enStates.Standard, r, this, false);
                        Skin.Draw_Border(GR, enDesign.Table_Cursor, enStates.Standard, r);
                    }
                }
            }
        }

        private void Draw_Line(Graphics GR, RowItem vrow, enStates vState, Rectangle DisplayRectangleWOSlider, ColumnViewItem ViewItem)
        {

            if (ViewItem == null) { return; }

            if (!IsOnScreen(vrow, DisplayRectangleWOSlider)) { return; }

            if (vrow.TMP_Y < HeadSize()) { return; }



            if (ViewItem.OrderTMP_Spalte_X1 + Column_DrawWidth(ViewItem, DisplayRectangleWOSlider) < 0) { return; }
            GR.SmoothingMode = SmoothingMode.None;
            GR.DrawLine(Skin.Pen_LinieDünn, 0, (int)vrow.TMP_Y, (int)ViewItem.OrderTMP_Spalte_X1 + Column_DrawWidth(ViewItem, DisplayRectangleWOSlider) - 1, (int)vrow.TMP_Y);


        }

        private void Draw_Table_ListboxStyle(Graphics GR, enStates vState, Rectangle DisplayRectangleWOSlider, int vFirstVisibleRow, int vLastVisibleRow)
        {
            var ItStat = vState;

            Skin.Draw_Back(GR, enDesign.ListBox, vState, DisplayRectangle, this, true);

            var Col = Database.Column[0];


            var r = new Rectangle();
            // Zeilen Zeichnen (Alle Zellen)
            for (var Zeiv = vFirstVisibleRow ; Zeiv <= vLastVisibleRow ; Zeiv++)
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

                Draw_Cell(GR, ViewItem, Row, r, enDesign.Item_Listbox, ItStat);


                if (!Row.CellGetBoolean(_Database.Column.SysCorrect()))
                {
                    GR.DrawImage(QuickImage.Get("Warnung|16||||||120||50").BMP, new Point(r.Right - 19, Convert.ToInt32(r.Top + (r.Height - 16) / 2.0)));
                }

                if (!string.IsNullOrEmpty(Row.TMP_Chapter))
                {
                    GR.DrawString(Row.TMP_Chapter, Chapter_Font.Font(), Chapter_Font.Brush_Color_Main, 0, (int)Row.TMP_Y - RowCaptionFontY);
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

        private void Draw_Cell(Graphics GR, ColumnViewItem CellInThisDatabaseColumn, RowItem CellInThisDatabaseRow, Rectangle DisplayRectangleWOSlider)
        {
            Draw_Cell(GR, CellInThisDatabaseColumn, CellInThisDatabaseRow, DisplayRectangleWOSlider, Cell_Font);
        }


        private void Draw_Cell(Graphics GR, ColumnViewItem CellInThisDatabaseColumn, RowItem CellInThisDatabaseRow, ColumnItem ContentHolderCellColumn, RowItem ContentHolderCellRow, Rectangle DisplayRectangleWOSlider, BlueFont vfont)
        {


            var toDraw = ContentHolderCellRow.CellGetString(ContentHolderCellColumn);


            if (string.IsNullOrEmpty(toDraw)) { return; }

            if (!ContentHolderCellColumn.MultiLine || !toDraw.Contains("\r"))
            {
                Draw_Cell_OnLine(GR, toDraw, CellInThisDatabaseColumn, CellInThisDatabaseRow, ContentHolderCellColumn, true, DisplayRectangleWOSlider, vfont, 0);
            }
            else
            {
                var MEI = toDraw.SplitByCR();
                if (ContentHolderCellColumn.ShowMultiLineInOneLine)
                {
                    Draw_Cell_OnLine(GR, MEI.JoinWith("; "), CellInThisDatabaseColumn, CellInThisDatabaseRow, ContentHolderCellColumn, true, DisplayRectangleWOSlider, vfont, 0);
                }
                else
                {
                    var y = 0;
                    for (var z = 0 ; z <= MEI.GetUpperBound(0) ; z++)
                    {
                        Draw_Cell_OnLine(GR, MEI[z], CellInThisDatabaseColumn, CellInThisDatabaseRow, ContentHolderCellColumn, Convert.ToBoolean(z == MEI.GetUpperBound(0)), DisplayRectangleWOSlider, vfont, y);
                        y += Skin.FormatedText_NeededSize(CellInThisDatabaseColumn.Column, MEI[z], null, vfont, enShortenStyle.Replaced).Height;
                    }
                }
            }


        }



        private void Draw_Cell(Graphics GR, ColumnViewItem CellInThisDatabaseColumn, RowItem CellInThisDatabaseRow, Rectangle DisplayRectangleWOSlider, BlueFont vfont)
        {

            if (CellInThisDatabaseRow == null) { return; }

            if (CellInThisDatabaseColumn.Column.Format == enDataFormat.LinkedCell)
            {
                Database.Cell.LinkedCellData(CellInThisDatabaseColumn.Column, CellInThisDatabaseRow, out var ContentHolderCellColumn, out var ContenHolderCellRow);
                if (ContentHolderCellColumn != null && ContenHolderCellRow != null)
                {
                    Draw_Cell(GR, CellInThisDatabaseColumn, CellInThisDatabaseRow, ContentHolderCellColumn, ContenHolderCellRow, DisplayRectangleWOSlider, vfont);
                }
                return;
            }




            Draw_Cell(GR, CellInThisDatabaseColumn, CellInThisDatabaseRow, CellInThisDatabaseColumn.Column, CellInThisDatabaseRow, DisplayRectangleWOSlider, vfont);


        }

        private void Draw_Column_Body(Graphics GR, ColumnViewItem CellInThisDatabaseColumn, enStates State, Rectangle DisplayRectangleWOSlider)
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

            for (z = 0 ; z <= 1 ; z++)
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




        private void Draw_Column_Head(Graphics GR, ColumnViewItem ViewItem, enStates vState, Rectangle DisplayRectangleWOSlider)
        {
            if (!IsOnScreen(ViewItem, DisplayRectangleWOSlider)) { return; }
            if (_Design == enBlueTableAppearance.OnlyMainColumnWithoutHead) { return; }


            if (Column_Font == null) { return; }
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

            // Autofilter-Button zeichnen
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


            if (tstate == enStates.Undefiniert)
            {
                ViewItem._TMP_AutoFilterLocation = new Rectangle(0, 0, 0, 0);
            }

            var tx = ViewItem.Column.Caption.Replace("\r", "\r\n");





            if (ViewItem.Column.CaptionBitmap != null && ViewItem.Column.CaptionBitmap.Width > 10)
            {
                // Erst das Bild
                var Sc = Math.Min(150 / (double)ViewItem.Column.CaptionBitmap.Width, 150 / (double)ViewItem.Column.CaptionBitmap.Height);
                var xP = (150 - ViewItem.Column.CaptionBitmap.Width * Sc) / 2;
                var yP = (150 - ViewItem.Column.CaptionBitmap.Height * Sc - Down) / 2;
                var pos = new Point((int)ViewItem.OrderTMP_Spalte_X1 + Convert.ToInt32((Column_DrawWidth(ViewItem, DisplayRectangleWOSlider) - 150) / 2.0), HeadSize() - 4 - 18 + Down);

                GR.DrawImage(ViewItem.Column.CaptionBitmap, Convert.ToInt32(pos.X + xP), Convert.ToInt32(pos.Y + yP * 2 - 150), Convert.ToInt32(150 - xP * 2), Convert.ToInt32(150 - yP * 2 - Down));

                // Dann der Text

                var FS = GR.MeasureString(tx, Column_Font.Font());
                pos = new Point((int)ViewItem.OrderTMP_Spalte_X1 + Convert.ToInt32((Column_DrawWidth(ViewItem, DisplayRectangleWOSlider) - FS.Width) / 2.0), 3 + Down);
                GR.TranslateTransform(pos.X, pos.Y);
                GR.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                GR.DrawString(tx, Column_Font.Font(), new SolidBrush(ViewItem.Column.ForeColor), 0, 0);
                GR.TranslateTransform(-pos.X, -pos.Y);

            }
            else
            {
                var FS = GR.MeasureString(tx, Column_Font.Font());
                var pos = new Point((int)ViewItem.OrderTMP_Spalte_X1 + Convert.ToInt32((Column_DrawWidth(ViewItem, DisplayRectangleWOSlider) - FS.Height) / 2.0), HeadSize() - 4 - 18);

                GR.TranslateTransform(pos.X, pos.Y);

                GR.RotateTransform(-90);
                GR.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                GR.DrawString(tx, Column_Font.Font(), new SolidBrush(ViewItem.Column.ForeColor), 0, 0);
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



        private void Draw_Cell(Graphics GR, ColumnViewItem Column, RowItem Row, Rectangle DisplayRectangleWOSlider, enDesign vDesign, enStates vState)
        {

            Skin.Draw_Back(GR, vDesign, vState, DisplayRectangleWOSlider, null, false);
            Skin.Draw_Border(GR, vDesign, vState, DisplayRectangleWOSlider);

            var f = Skin.GetBlueFont(vDesign, vState);
            if (f == null) { return; }


            Draw_Cell(GR, Column, Row, DisplayRectangleWOSlider, f);

        }

        private void Draw_Cell_OnLine(Graphics GR, string DrawString, ColumnViewItem CellInThisDatabaseColumn, RowItem CellInThisDatabaseRow, ColumnItem ContentHolderColumnStyle, bool IsLastRow, Rectangle DisplayRectangleWOSlider, BlueFont vfont, int Y)
        {


            var r = new Rectangle((int)CellInThisDatabaseColumn.OrderTMP_Spalte_X1, (int)CellInThisDatabaseRow.TMP_Y + Y, Column_DrawWidth(CellInThisDatabaseColumn, DisplayRectangleWOSlider), 16);

            if (r.Bottom > CellInThisDatabaseRow.TMP_Y + Row_DrawHeight(CellInThisDatabaseRow, DisplayRectangleWOSlider) - 16)
            {
                if (r.Bottom > CellInThisDatabaseRow.TMP_Y + Row_DrawHeight(CellInThisDatabaseRow, DisplayRectangleWOSlider)) { return; }
                if (!IsLastRow) { DrawString = "..."; }// Die Letzte Zeile noch ganz hinschreiben
            }

            Skin.Draw_FormatedText(GR, ContentHolderColumnStyle, DrawString, null, enAlignment.Top_Left, r, null, false, vfont, enShortenStyle.Replaced);

        }



        #endregion


        public static string Database_NeedPassword()
        {

            return InputBox.Show("Bitte geben sie das Passwort ein,<br>um Zugriff auf diese Datenbank<br>zu erhalten:", string.Empty, enDataFormat.Text);
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

        public string CursorPosKey()
        {
            return CellCollection.KeyOfCell(_CursorPosColumn, _CursorPosRow);
        }

        public ColumnItem MouseOverColumn()
        {
            return _MouseOverColumn;
        }
        public RowItem MouseOverRow()
        {
            return _MouseOverRow;
        }

        public string MouseOverKey()
        {
            return CellCollection.KeyOfCell(_MouseOverColumn, _MouseOverRow);
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
            var l = new List<ColumnItem>();
            l.Add(OnlyColumn);

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

                _Database.OnConnectedControlsStopAllWorking();

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





        //private void Arrangement_Item_Click(object sender, BasicListItemEventArgs e)
        //{
        //    if (_Database == null) { return; }

        //    lock (Lock_UserAction)
        //    {
        //        Arrangement = int.Parse(e.Item.Internal());
        //    }
        //}


        private void DropDownMenu_ItemClicked(object sender, ContextMenuItemClickedEventArgs e)
        {

            FloatingInputBoxListBoxStyle.Close(this);
            if (string.IsNullOrEmpty(e.ClickedComand.Internal())) { return; }


            var CellKey = string.Empty;
            if (e.Tag is string s) { CellKey = s; }
            if (string.IsNullOrEmpty(CellKey)) { return; }
            Database.Cell.DataOfCellKey(CellKey, out var Column, out var Row);



            var ToAdd = e.ClickedComand.Internal();
            var ToRemove = string.Empty;



            //if (Column.Format == enDataFormat.Relation)
            //{
            //    Develop.DebugPrint_NichtImplementiert();
            //    // Bei Relations ist es ein bisschen anders:
            //    // Das, was angeklickt wird, wird geändert. Also muss das angeklickte ENTFERNT werden.
            //    // Dafür wird ein neues Objekt hinzugefügt.

            //    //if (!ToAdd.StartsWith("#"))
            //    //{
            //    //    ToRemove = ToAdd;
            //    //    var nc = DialogBox.eEditClass(new clsRelation(Column, Row, ToAdd), true);
            //    //    if (nc != null) { ToAdd = nc.ToString(); }
            //    //}

            //    //else if (ToAdd == "#Relation")
            //    //{
            //    //    ToRemove = string.Empty;
            //    //    ToAdd = DialogBox.eEditClass(new clsRelation(Column, Row), false).ToString();
            //    //}
            //    //if (ToAdd == ToRemove) { return; }
            //}

            if (ToAdd == "#Erweitert")
            {
                Cell_Edit(Column, Row, false);
                return;
            }

            if (Row == null)
            {
                // Neue Zeile!
                UserEdited(ToAdd, Column, null, false);
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

                UserEdited(E.JoinWithCr(), Column, Row, false);
            }
            else
            {

                if (Column.DropdownAllesAbwählenErlaubt)
                {
                    if (ToAdd == Row.CellGetString(Column))
                    {
                        UserEdited("", Column, Row, false);
                        return;
                    }
                }


                UserEdited(ToAdd, Column, Row, false);
            }


        }




        private void Cell_Edit(ColumnItem CellInThisDatabaseColumn, RowItem CellInThisDatabaseRow, bool WithDropDown)
        {
            Database.OnConnectedControlsStopAllWorking();

            ColumnItem ContentHolderCellColumn;
            RowItem ContentHolderCellRow;


            if (Database.ReloadNeeded()) { Database.Reload(); }

            if (CellInThisDatabaseColumn == null)
            {
                NotEditableInfo("Interner Zellenfehler");
                return;
            }

            var ViewItem = _Database.ColumnArrangements[_ArrangementNr][CellInThisDatabaseColumn];


            if (ViewItem == null)
            {
                NotEditableInfo("Ansicht veraltet");
                return;
            }

            if (CellInThisDatabaseColumn.Format == enDataFormat.LinkedCell)
            {
                Database.Cell.LinkedCellData(CellInThisDatabaseColumn, CellInThisDatabaseRow, out ContentHolderCellColumn, out ContentHolderCellRow);
                if (ContentHolderCellColumn == null) { return; }
            }
            else
            {
                ContentHolderCellColumn = CellInThisDatabaseColumn;
                ContentHolderCellRow = CellInThisDatabaseRow;
            }


            if (!ContentHolderCellColumn.DropdownBearbeitungErlaubt) { WithDropDown = false; }
            var dia = ColumnItem.UserEditDialogTypeInTable(ContentHolderCellColumn, WithDropDown);
            if (dia == enEditTypeTable.None || dia == enEditTypeTable.FileHandling_InDateiSystem)
            {
                NotEditableInfo("Spalte kann generell nicht bearbeitet werden.");
                return;
            }
            if (!ContentHolderCellColumn.Database.Cell.UserEditPossible(ContentHolderCellColumn, ContentHolderCellRow, false))
            {
                NotEditableInfo(ContentHolderCellColumn.Database.Cell.UserEditErrorReason(ContentHolderCellColumn, ContentHolderCellRow, false));
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

        private bool Cell_Edit_Relations(ColumnItem CellInThisDatabaseColumn, RowItem CellInThisDatabaseRow)
        {


            if (CellInThisDatabaseRow == null) { return false; }// Neue Zeilen nicht erlaubt


            var t = new ItemCollectionList();
            t.Appearance = enBlueListBoxAppearance.DropdownSelectbox;
            t.GetItemCollection(CellInThisDatabaseColumn, t, CellInThisDatabaseRow, enShortenStyle.Replaced);
            t.UncheckAll();


            var I1 = new TextListItem("#Relation", "Neue Beziehung hinzufügen", QuickImage.Get(enImageCode.Herz), true);




            switch (t.Count)
            {
                case 0:
                    DropDownMenu_ItemClicked(null, new ContextMenuItemClickedEventArgs(CellCollection.KeyOfCell(CellInThisDatabaseColumn, CellInThisDatabaseRow), I1));
                    return true;
                case 1 when !CellInThisDatabaseColumn.MultiLine:
                    DropDownMenu_ItemClicked(null, new ContextMenuItemClickedEventArgs(CellCollection.KeyOfCell(CellInThisDatabaseColumn, CellInThisDatabaseRow), I1));
                    return true;
            }


            if (CellInThisDatabaseColumn.MultiLine)
            {

                I1.UserDefCompareKey = Constants.FirstSortChar + "2";
                t.Add(I1);


                t.Add(new LineListItem(Constants.FirstSortChar + "4"));
            }


            var _DropDownMenu = FloatingInputBoxListBoxStyle.Show(t, CellCollection.KeyOfCell(CellInThisDatabaseColumn, CellInThisDatabaseRow), this);
            _DropDownMenu.ItemClicked += DropDownMenu_ItemClicked;




            Develop.Debugprint_BackgroundThread();
            return true;


        }


        //Private Function Cell_Edit_SuperFormat(Text As String) As String


        //    Return Text
        //End Function
        //private void Cell_Edit_Font(ColumnItem CellInThisDatabaseColumn, RowItem CellInThisDatabaseRow)
        //{
        //    if (InvokeRequiredXXX) { return; }

        //    BlueFont bf = CellInThisDatabaseRow.CellGetBlueFont(CellInThisDatabaseColumn);

        //    _FDia = new FontSelectDialog();
        //    _FDia.Font = bf;

        //    _FDia.ShowDialog();
        //    BlueFont bf2 = _FDia.Font;

        //    _FDia.Dispose();

        //    if (bf == bf2) { return; } // cancel

        //    UserEdited(bf2.ToString(), CellInThisDatabaseColumn, CellInThisDatabaseRow, false);
        //}




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



            UserEdited(Color.FromArgb(255, ColDia.Color).ToArgb().ToString(), CellInThisDatabaseColumn, CellInThisDatabaseRow, false);


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
                Box.Size = new Size(Column_DrawWidth(ViewItemx, DisplayRectangle) + AddWith, 18);
                Box.Text = "";
            }


            Box.Format = ContentHolderCellColumn.Format;
            Box.AllowedChars = ContentHolderCellColumn.AllowedChars;
            Box.MultiLine = ContentHolderCellColumn.MultiLine;
            Box.Tag = CellCollection.KeyOfCell(CellInThisDatabaseColumn, CellInThisDatabaseRow); // ThisDatabase, der Wert wird beim einchecken in die Fremdzelle geschrieben




            if (Box is ComboBox box)
            {
                box.Item.GetItemCollection(ContentHolderCellColumn, box.Item, ContentHolderCellRow, enShortenStyle.Both);
                if (box.Item.Count == 0)
                {
                    return Cell_Edit_TextBox(CellInThisDatabaseColumn, CellInThisDatabaseRow, ContentHolderCellColumn, ContentHolderCellRow, BTB, 0, 0);
                }
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
            t.GetItemCollection(ContentHolderCellColumn, t, ContentHolderCellRow, enShortenStyle.Both);

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

            var _DropDownMenu = FloatingInputBoxListBoxStyle.Show(t, CellCollection.KeyOfCell(CellInThisDatabaseColumn, CellInThisDatabaseRow), this);
            _DropDownMenu.ItemClicked += DropDownMenu_ItemClicked;
            Develop.Debugprint_BackgroundThread();
        }










        private void UserEdited(string NewValue, ColumnItem CellInThisDatabaseColumn, RowItem CellInThisDatabaseRow, bool FormatWarnung)
        {


            if (CellInThisDatabaseRow == null && CellInThisDatabaseColumn != Database.Column[0])
            {

                NotEditableInfo("Neue Zeilen müssen mit der ersten Spalte beginnen");
                return;
            }


            NewValue = CellInThisDatabaseColumn.AutoCorrect(NewValue);


            if (CellInThisDatabaseRow != null)
            {
                if (NewValue == CellInThisDatabaseRow.CellGetString(CellInThisDatabaseColumn)) { return; }
            }
            else
            {
                if (string.IsNullOrEmpty(NewValue)) { return; }
            }


            var ed = new BeforeNewValueEventArgs(CellInThisDatabaseColumn, CellInThisDatabaseRow, NewValue, "");
            OnEditBeforeNewValueSet(ed);
            var CancelReason = ed.CancelReason;


            if (string.IsNullOrEmpty(CancelReason) && FormatWarnung && !string.IsNullOrEmpty(NewValue))
            {

                if (!NewValue.IsFormat(CellInThisDatabaseColumn.Format, CellInThisDatabaseColumn.MultiLine))
                {
                    if (MessageBox.Show("Ihre Eingabe entspricht<br><u>nicht</u> dem erwarteten Format!<br><br>Trotzdem übernehmen?", enImageCode.Information, "Ja", "Nein") != 0)
                    {
                        CancelReason = "Abbruch, das das erwartete Format nicht eingehalten wurde.";
                    }
                }
            }


            if (string.IsNullOrEmpty(CancelReason))
            {
                if (CellInThisDatabaseRow == null)
                {
                    CellInThisDatabaseRow = _Database.Row.Add(NewValue);
                }
                else
                {
                    CellInThisDatabaseRow.CellSet(CellInThisDatabaseColumn, NewValue);
                }


                CursorPos_Set(CellInThisDatabaseColumn, CellInThisDatabaseRow, false);
                CellInThisDatabaseRow.DoAutomatic(false, true);

                // EnsureVisible ganz schlecht: Daten verändert, keine Positionen bekannt - und da soll sichtbar gemacht werden?
                // CursorPos.EnsureVisible(SliderX, SliderY, DisplayRectangle)

            }
            else
            {
                NotEditableInfo(CancelReason);


            }

        }

        private void OnEditBeforeNewValueSet(BeforeNewValueEventArgs ed)
        {
            EditBeforeNewValueSet?.Invoke(this, ed);
        }

        //private bool IsUserWorking()
        //{

        //    if (vUserDidSomething)
        //    {
        //        vUserDidSomething = false;
        //        return true;
        //    }

        //    if (BTB != null && !BTB.IsDisposed && BTB.Visible) { return true; }
        //    if (BCB != null && !BCB.IsDisposed && BCB.Visible) { return true; }
        //    if (isContextMenuCurentlyShowing()) { return true; }
        //    if (_AutoFilter != null && !_AutoFilter.IsDisposed && _AutoFilter.Visible) { return true; }
        //    if (_DropDownMenu != null && !_DropDownMenu.IsDisposed && _DropDownMenu.Visible) { return true; }
        //    if (_searchAndReplace != null && !_searchAndReplace.IsDisposed && _searchAndReplace.Visible) { return true; }


        //    return false;

        //}



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

            UserEdited(w, column, row, true);

            Focus();

        }

        /// <summary>
        /// Setzt die Variable CursorPos um X Columns und Y Reihen um. Dabei wird die Columns und Zeilensortierung berücksichtigt. 
        /// </summary>
        /// <remarks></remarks>
        private void Cursor_Move(enDirection Richtung)
        {

            if (_Database == null) { return; }
            CursorPos_Set(Neighbour(_CursorPosColumn, _CursorPosRow, Richtung));

            if (Richtung != enDirection.Nichts) { EnsureVisible(_CursorPosColumn, _CursorPosRow); }

        }


        private bool ComputeAllCellPositions()
        {
            Develop.DebugPrint_InvokeRequired(InvokeRequired, true);


            if (Database.IsParsing()) { return false; }

            try
            {

                var MaxX = 0;
                var MaxY = 0;
                var LastCap = "";

                var DisplayR = DisplayRectangleWithoutSlider();


                if (UserEdit_NewRowAllowed()) { MaxY += 18; }


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
                        if (ThisViewItem.Type != enViewType.PermanentColumn) { wdh = false; }

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
                                ThisViewItem.OrderTMP_Spalte_X1 = Convert.ToInt32(MaxX - SliderX.Value);
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

                    if (ThisRow.CellGetString(_Database.Column.SysChapter()) != LastCap)
                    {
                        ThisRow.TMP_Y += RowCaptionSizeY;
                        MaxY += RowCaptionSizeY;
                        LastCap = ThisRow.CellGetString(_Database.Column.SysChapter());


                        if (string.IsNullOrEmpty(LastCap))
                        {
                            ThisRow.TMP_Chapter = "*";
                        }
                        else
                        {
                            ThisRow.TMP_Chapter = LastCap.Replace("\r\n", ", ");
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
            return Convert.ToBoolean(prev.Type == enViewType.PermanentColumn);


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
            return Convert.ToBoolean(NX.Type != enViewType.PermanentColumn);


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

            Database.OnConnectedControlsStopAllWorking();
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





        private void GetUniques(ColumnItem Column, List<string> Einzigartig, List<string> NichtEinzigartig)
        {
            //  Dim List_FilterString As List(Of String) = Column.Autofilter_ItemList(_Filter)
            foreach (var ThisRow in SortedRows())
            {
                List<string> TMP = null;
                if (Column.MultiLine)
                {
                    TMP = ThisRow.CellGetList(Column);
                }
                else
                {
                    TMP = new List<string>();
                    TMP.Add(ThisRow.CellGetString(Column));
                }

                foreach (var ThisString in TMP)
                {
                    if (Einzigartig.Contains(ThisString))
                    {
                        NichtEinzigartig.AddIfNotExists(ThisString);
                    }
                    else
                    {
                        Einzigartig.AddIfNotExists(ThisString);
                    }
                }
            }

            Einzigartig.RemoveString(NichtEinzigartig, false);

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
                    var Einzigartig = new List<string>();
                    var NichtEinzigartig = new List<string>();
                    GetUniques(e.Column, Einzigartig, NichtEinzigartig);

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
                    var xEinzigartig = new List<string>();
                    var xNichtEinzigartig = new List<string>();
                    GetUniques(e.Column, xEinzigartig, xNichtEinzigartig);

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


        //private void OnBeforeAutoFilterShow(ColumnEventArgs e)
        //{
        //    BeforeAutoFilterShow?.Invoke(this, e);
        //}



        #endregion

        #region  ContextMenu 



        private void ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e)
        {
            FloatingInputBoxListBoxStyle.Close(this);
            if (e.ClickedComand.Internal().ToLower() == "abbruch") { return; }

            if (e.ClickedComand.Internal().Substring(0, 1) == "*") { return; }


            var CellKey = string.Empty;
            if (e.Tag is string s) { CellKey = s; }

            Database.Cell.DataOfCellKey(CellKey, out _, out _);



            //if (DoInternalContextMenuComand(e.ClickedComand.Internal(), Column, Row)) { return; }

            OnContextMenuItemClicked(e);
        }

        #endregion



        #region  Ereignisse der Slider 
        private void SliderY_ValueChanged(object sender, System.EventArgs e)
        {
            if (_Database == null) { return; }

            lock (Lock_UserAction)
            {
                Database.OnConnectedControlsStopAllWorking();
                Invalidate();
            }

        }

        private void SliderX_ValueChanged(object sender, System.EventArgs e)
        {
            if (_Database == null) { return; }

            lock (Lock_UserAction)
            {
                Database.OnConnectedControlsStopAllWorking();
                Invalidate();
            }
        }
        #endregion



        #region  Ereignisse der Datenbank 




        private void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e)
        {
            ContextMenuItemClicked?.Invoke(this, e);
        }

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


        private void _Database_StopAllWorking(object sender, System.EventArgs e)
        {
            CloseAllComponents();
        }






        private void _Database_DatabaseChanged(object sender, DatabaseChangedEventArgs e)
        {
            // Wird auch bei einem Reload ausgeführt.
            // Es kann aber sein, dass eine Ansicht zurückgeholt wurde, und die Werte stimmen. 
            // Deswegen prüfen, ob wirklich alles geleöscht werden muss, oder weiter behalten werden kann.

            // Auf Nothing  muss auch geprüft werden, da bei einem Dispose oder beim Beenden sich die Datenbank auch änsdert....



            Invalidate_HeadSize();

            var f = string.Empty;


            if (Filter != null)
            {
                if (e.OnlyReloaded) { f = Filter.ToString(); }
                Filter.Changed -= FilterChanged;
                Filter = null;
            }



            if (_Database != null)
            {


                Filter = new FilterCollection(_Database, f);
                Filter.Changed += FilterChanged;

                if (e.OnlyReloaded)
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
            OnDatabaseChanged();
            OnViewChanged();

            Invalidate();
        }

        private void OnDatabaseChanged()
        {
            DatabaseChanged?.Invoke(this, System.EventArgs.Empty);
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
            DialogBoxes.QuickInfo.Close();


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
                Database.OnConnectedControlsStopAllWorking();
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

                Database.OnConnectedControlsStopAllWorking();
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
                Database.OnConnectedControlsStopAllWorking();

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

                Database.OnConnectedControlsStopAllWorking();

                switch (e.KeyCode)
                {
                    case System.Windows.Forms.Keys.Delete:
                        if (_CursorPosRow.CellIsNullOrEmpty(_CursorPosColumn))
                        {
                            ISIN_KeyDown = false;
                            return;
                        }

                        var l = _Database.Cell.UserEditErrorReason(_CursorPosColumn, _CursorPosRow, false);

                        if (string.IsNullOrEmpty(l))
                        {
                            UserEdited(string.Empty, _CursorPosColumn, _CursorPosRow, true);
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
                            CursorPos_Set(string.Empty);
                            SliderY.Value += SliderY.LargeChange;
                        }
                        break;

                    case System.Windows.Forms.Keys.PageUp: //Bildab
                        if (SliderY.Enabled)
                        {
                            CursorPos_Set(string.Empty);
                            SliderY.Value -= SliderY.LargeChange;
                        }
                        break;

                    case System.Windows.Forms.Keys.Home:
                        if (SliderY.Enabled)
                        {
                            CursorPos_Set(null);
                            SliderY.Value = SliderY.Minimum;

                        }
                        break;

                    case System.Windows.Forms.Keys.End:
                        if (SliderY.Enabled)
                        {
                            CursorPos_Set(null);
                            SliderY.Value = SliderY.Maximum;
                        }
                        break;

                    case System.Windows.Forms.Keys.C:
                        if (e.Modifiers == System.Windows.Forms.Keys.Control)
                        {
                            CopyToClipboard(_CursorPosColumn, _CursorPosRow, false);
                        }
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


                                var l2 = _Database.Cell.UserEditErrorReason(_CursorPosColumn, _CursorPosRow, false);

                                if (string.IsNullOrEmpty(l2))
                                {
                                    UserEdited(ntxt, _CursorPosColumn, _CursorPosRow, true);
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
                DialogBoxes.QuickInfo.Close();
                return;
            }

            lock (Lock_UserAction)
            {
                if (ISIN_MouseMove) { return; }
                ISIN_MouseMove = true;


                CellOnCoordinate(e.X, e.Y, out _MouseOverColumn, out _MouseOverRow);

                if (e.Button != System.Windows.Forms.MouseButtons.None)
                {
                    _Database.OnConnectedControlsStopAllWorking();
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
                                        Database.Cell.LinkedCellData(_MouseOverColumn, _MouseOverRow, out var ContentHolderCellColumn, out _);
                                        if (ContentHolderCellColumn != null) { T = ContentHolderCellColumn.QickInfoText(_MouseOverColumn.ReadableText() + " bei " + ContentHolderCellColumn.ReadableText() + ":"); }
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
                            T = Database.UndoText(MouseOverKey());
                        }
                    }


                    T = T.Trim();
                    T = T.Trim("<br>");
                    T = T.Trim();

                    if (!string.IsNullOrEmpty(T))
                    {
                        DialogBoxes.QuickInfo.Show(T);
                    }
                    else
                    {
                        DialogBoxes.QuickInfo.Close();
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
                    DialogBoxes.QuickInfo.Close();
                    ISIN_MouseUp = false;
                    return;
                }

                CellOnCoordinate(e.X, e.Y, out _MouseOverColumn, out _MouseOverRow);



                if (CursorPosKey() != MouseOverKey()) { DialogBoxes.QuickInfo.Close(); }



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
                    ContextMenu_Show(this, e);
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

                if (Mouse_IsInHead())
                {
                    Database.OnConnectedControlsStopAllWorking();

                    DoubleClick?.Invoke(this, new CellEventArgs(_MouseOverColumn, _MouseOverRow));
                    ISIN_DoubleClick = false;
                    return;
                }

                var c1 = MouseOverKey();

                CellOnCoordinate(MousePos().X, MousePos().Y, out _MouseOverColumn, out _MouseOverRow);
                if (c1 != MouseOverKey()) // Da hat das eventx z.B. die Zeile gelöscht
                {
                    ISIN_DoubleClick = false;
                    return;
                }

                Cell_Edit(_MouseOverColumn, _MouseOverRow, true);

                DoubleClick?.Invoke(this, new CellEventArgs(_MouseOverColumn, _MouseOverRow));

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




        public void CursorPos_Set(ColumnItem Column, RowItem Row, bool EnsureVisible)
        {
            CursorPos_Set(CellCollection.KeyOfCell(Column, Row), EnsureVisible);
        }


        public void CursorPos_Set(string CellKey)
        {
            CursorPos_Set(CellKey, false);
        }



        public void CursorPos_Set(string CellKey, bool EnsureVisiblex)
        {

            if (CellKey == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Null bei CellKeys nicht erlaubt"); }
            if (CellKey == CursorPosKey()) { return; }

            _Database.Cell.DataOfCellKey(CellKey, out _CursorPosColumn, out _CursorPosRow);


            if (EnsureVisiblex) { EnsureVisible(_CursorPosColumn, _CursorPosRow); }
            Invalidate();

            OnCursorPosChanged(new CellEventArgs(_CursorPosColumn, _CursorPosRow));

            //Check_OrderButtons();

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
                DialogBoxes.QuickInfo.Close();


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
                DialogBoxes.QuickInfo.Close();

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
                Database.OnConnectedControlsStopAllWorking();

                Invalidate_AllDraw(false);
                ISIN_Resize = false;
            }
        }

        public void WriteColumnArrangementsInto(ComboBox _ColumnArrangementSelector)
        {

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

            x = x + ", CursorPos=" + CursorPosKey();

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
                var PairValuexxx = Value.ParseValue(T, Beg);
                Beg = Beg + T.Length + PairValuexxx.Length + 2;
                switch (T)
                {
                    case "arrangementnr":
                        Arrangement = int.Parse(PairValuexxx);
                        break;

                    case "filters":
                        Filter.Parse(PairValuexxx);
                        break;

                    case "sliderx":
                        SliderX.Maximum = Math.Max(SliderX.Maximum, int.Parse(PairValuexxx));
                        SliderX.Value = int.Parse(PairValuexxx);
                        break;

                    case "slidery":
                        SliderY.Maximum = Math.Max(SliderY.Maximum, int.Parse(PairValuexxx));
                        SliderY.Value = int.Parse(PairValuexxx);
                        break;

                    case "cursorpos":
                        CursorPos_Set(PairValuexxx);
                        break;

                    case "tempsort":
                        _sortDefinitionTemporary = new RowSortDefinition(_Database, PairValuexxx);
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

            if (!string.IsNullOrEmpty(_StoredView)) { Develop.DebugPrint("Stored View nicht Empty!"); }

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

        private void _Database_RestoreView(object sender, System.EventArgs e)
        {
            if (string.IsNullOrEmpty(_StoredView)) { Develop.DebugPrint("Stored View Empty!"); }

            ParseView(_StoredView);
            _StoredView = string.Empty;

        }

        #endregion






        //public void Invalidatex_ExternalContols()
        //{
        //    _InvalidExternal = true;
        //}




        #region  Arrangement Only 










        public void Arrangement_Add()
        {
            string e = null;

            var MitVorlage = false;

            if (_ArrangementNr > 0)
            {
                MitVorlage = Convert.ToBoolean(MessageBox.Show("<b>Neue Spaltenanordnung erstellen:</b><br>Wollen sie die aktuelle Ansicht kopieren?", enImageCode.Frage, "Ja", "Nein") == 0);
            }

            if (_Database.ColumnArrangements.Count < 1)
            {
                _Database.ColumnArrangements.Add(new ColumnViewCollection(_Database, "", ""));
            }

            if (MitVorlage)
            {
                e = InputBox.Show("Die aktuelle Ansicht wird <b>kopiert</b>.<br><br>Geben sie den Namen<br>der neuen Anordnung ein:", "", enDataFormat.Text_Ohne_Kritische_Zeichen);
                if (string.IsNullOrEmpty(e)) { return; }
                _Database.ColumnArrangements.Add(new ColumnViewCollection(_Database, _Database.ColumnArrangements[_ArrangementNr].ToString(), e));
            }
            else
            {
                e = InputBox.Show("Geben sie den Namen<br>der neuen Anordnung ein:", "", enDataFormat.Text_Ohne_Kritische_Zeichen);
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
            if (!EnsureVisible(_Database.ColumnArrangements[_ArrangementNr][Column])) { return false; }
            if (!EnsureVisible(Row)) { return false; }
            return true;
        }

        private bool EnsureVisible(RowItem vRow)
        {

            if (!SortedRows().Contains(vRow)) { return false; }

            if (vRow.TMP_Y == null && !ComputeAllCellPositions()) { return false; }


            var r = DisplayRectangleWithoutSlider();


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

            if (ViewItem.Type == enViewType.PermanentColumn)
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
            if (_Database.ColumnArrangements.Count - 1 < _ArrangementNr) { return null; }

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


        public static int tmpColumnContentWidth(ColumnItem Column)
        {

            if (Column.TMP_ColumnContentWidth != null) { return (int)Column.TMP_ColumnContentWidth; }

            Column.TMP_ColumnContentWidth = 0;

            foreach (var ThisRowItem in Column.Database.Row)
            {
                if (ThisRowItem != null && !ThisRowItem.CellIsNullOrEmpty(Column))
                {
                    Column.TMP_ColumnContentWidth = Math.Max((int)Column.TMP_ColumnContentWidth, Cell_ContentSize(Column, ThisRowItem).Width);
                }
            }

            return (int)Column.TMP_ColumnContentWidth;
        }




        private int Column_DrawWidth(ColumnViewItem ViewItem, Rectangle DisplayRectangleWOSlider)
        {
            // Hier wird die ORIGINAL-Spalte gezeichnet, nicht die FremdZelle!!!!
            Develop.DebugPrint_InvokeRequired(InvokeRequired, true);

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

                if (ViewItem.Type == enViewType.PermanentColumn)
                {
                    ViewItem._TMP_DrawWidth = Math.Min(tmpColumnContentWidth(ViewItem.Column), Convert.ToInt32(DisplayRectangleWOSlider.Width * 0.3));
                }
                else
                {
                    ViewItem._TMP_DrawWidth = Math.Min(tmpColumnContentWidth(ViewItem.Column), Convert.ToInt32(DisplayRectangleWOSlider.Width * 0.75));
                }
            }

            ViewItem._TMP_DrawWidth = Math.Max((int)ViewItem._TMP_DrawWidth, 18); // Mindesten so groß wie der Autofilter;
            ViewItem._TMP_DrawWidth = Math.Max((int)ViewItem._TMP_DrawWidth, (int)ColumnHead_Size(ViewItem.Column).Width);

            return (int)ViewItem._TMP_DrawWidth;
        }
        private int Row_DrawHeight(RowItem vrow, Rectangle DisplayRectangleWOSlider)
        {
            if (_Design == enBlueTableAppearance.OnlyMainColumnWithoutHead) { return Cell_ContentSize(_Database.Column[0], vrow).Height; }
            if (vrow.TMP_DrawHeight != null) { return (int)vrow.TMP_DrawHeight; }
            var tmp = 18; // Der Scale wird gleich noch dazu gerechnet

            foreach (var ThisViewItem in _Database.ColumnArrangements[_ArrangementNr])
            {
                if (ThisViewItem?.Column != null)
                {
                    if (!vrow.CellIsNullOrEmpty(ThisViewItem.Column))
                    {
                        tmp = Math.Max(tmp, Cell_ContentSize(ThisViewItem.Column, vrow).Height);
                    }
                }
            }
            vrow.TMP_DrawHeight = Math.Min(tmp, (int)(DisplayRectangleWOSlider.Height * 0.9) - HeadSize());
            vrow.TMP_DrawHeight = Math.Max((int)vrow.TMP_DrawHeight, 18);

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
            _SortedRows = _Database.Row.CalculateSortedRows(Filter, SortUsed());
            OnRowsSorted();
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

            for (var rowcount = 0 ; rowcount <= SortedRows().Count - 2 ; rowcount++)
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
            if (YPos <= HeadSize()) { return null; }


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
        /// </summary>
        /// <remarks></remarks>
        private string Neighbour(ColumnItem column, RowItem row, enDirection Direction)
        {





            if (_Design == enBlueTableAppearance.OnlyMainColumnWithoutHead)
            {
                if (Direction != enDirection.Oben && Direction != enDirection.Unten) { return CellCollection.KeyOfCell(_Database.Column[0], row); }
            }



            if (column != null)
            {
                if (Convert.ToBoolean(Direction & enDirection.Links))
                {
                    if (_Database.ColumnArrangements[_ArrangementNr].PreviousVisible(column) != null)
                    {
                        column = _Database.ColumnArrangements[_ArrangementNr].PreviousVisible(column);
                    }
                }

                if (Convert.ToBoolean(Direction & enDirection.Rechts))
                {
                    if (_Database.ColumnArrangements[_ArrangementNr].NextVisible(column) != null)
                    {
                        column = _Database.ColumnArrangements[_ArrangementNr].NextVisible(column);
                    }
                }
            }

            if (row != null)
            {
                if (Convert.ToBoolean(Direction & enDirection.Oben))
                {
                    if (View_PreviousRow(row) != null) { row = View_PreviousRow(row); }
                }

                if (Convert.ToBoolean(Direction & enDirection.Unten))
                {
                    if (View_NextRow(row) != null) { row = View_NextRow(row); }
                }

            }


            return CellCollection.KeyOfCell(column, row);

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
            if (Column_Font == null) { return new SizeF(16, 16); }


            Column.TMP_CaptionText_Size = DummyGraphics().MeasureString(Column.Caption.Replace("\r", "\r\n"), Column_Font.Font());
            return Column.TMP_CaptionText_Size;
        }

        private bool UserEdit_NewRowAllowed()
        {

            if (Thread.CurrentThread.IsBackground) { return false; }

            if (_Database.Column[0] == null) { return false; }

            if (_Design == enBlueTableAppearance.OnlyMainColumnWithoutHead) { return false; }
            if (_Database.ColumnArrangements.Count == 0) { return false; }

            if (_Database.ColumnArrangements[_ArrangementNr][_Database.Column[0]] == null) { return false; }
            if (!_Database.PermissionCheck(_Database.PermissionGroups_NewRow, null)) { return false; }

            if (!_Database.Cell.UserEditPossible(_Database.Column[0], null, false)) { return false; }
            return true;
        }







        private void _Database_RowRemoved(object sender, System.EventArgs e)
        {
            Invalidate_RowSort();
            Invalidate();
        }

        private void _Database_ColumnAddedOrDeleted(object sender, System.EventArgs e)
        {
            Invalidate_HeadSize();
            Invalidate_AllDraw(true);
            Invalidate_RowSort();
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
                Database.OnConnectedControlsStopAllWorking();
                ISIN_VisibleChanged = false;
            }

        }




        public FilterCollection Filter { get; private set; }


        private void _Database_SavedToDisk(object sender, System.EventArgs e)
        {
            Invalidate();
        }

        public void OpenSearchAndReplace()
        {

            if (_searchAndReplace == null || _searchAndReplace.IsDisposed || !_searchAndReplace.Visible)
            {
                _searchAndReplace = new SearchAndReplace(this);
                _searchAndReplace.Show();
            }

        }

        public void ContextMenu_Show(object sender, System.Windows.Forms.MouseEventArgs e)
        {

            if (_Database.IsParsing()) { return; }

            Database?.Reload();


            var ThisContextMenu = new ItemCollectionList(enBlueListBoxAppearance.KontextMenu);
            var UserMenu = new ItemCollectionList(enBlueListBoxAppearance.KontextMenu);
            var OverColumn = OnCoordinateColumn(e.X);
            var OverRow = OnCoordinateRow(e.Y);

            OnContextMenuInit(new ContextMenuInitEventArgs(CellCollection.KeyOfCell(OverColumn, OverRow), UserMenu));


            if (ThisContextMenu.Count > 0 && UserMenu.Count > 0) { ThisContextMenu.Add(new LineListItem()); }
            if (UserMenu.Count > 0) { ThisContextMenu.AddRange(UserMenu.ToList()); }

            if (ThisContextMenu.Count > 0)
            {
                ThisContextMenu.Add(new LineListItem());
                ThisContextMenu.Add(enContextMenuComands.Abbruch);
                var _ContextMenu = FloatingInputBoxListBoxStyle.Show(ThisContextMenu, CellCollection.KeyOfCell(OverColumn, OverRow), this);
                _ContextMenu.ItemClicked += ContextMenuItemClickedInternalProcessig;
            }


        }


        private void OnContextMenuInit(ContextMenuInitEventArgs e)
        {
            ContextMenuInit?.Invoke(this, e);
        }

        private SizeF ColumnHead_Size(ColumnItem column)
        {
            float wi = 0;
            float he = 0;

            if (column.CaptionBitmap != null && column.CaptionBitmap.Width > 10)
            {
                wi = Math.Max(150, ColumnCaptionText_Size(column).Width);
                he = 150 + ColumnCaptionText_Size(column).Height + 3;
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
            _Reasondate = DateTime.Now;
            DialogBoxes.QuickInfo.Show("<IMAGECODE=Stift|16||1> " + Reason);
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





        public static Size Cell_ContentSize(ColumnItem Column, RowItem Row)
        {

            if (Column.Format == enDataFormat.LinkedCell)
            {
                Column.Database.Cell.LinkedCellData(Column, Row, out var LCColumn, out var LCrow);
                if (LCColumn != null && LCrow != null) { return Cell_ContentSize(LCColumn, LCrow); }
                return new Size(16, 16);
            }


            var _ContentSize = Column.Database.Cell.GetSizeOfCellContent(Column, Row);
            if (_ContentSize.Width > 0 && _ContentSize.Height > 0) { return _ContentSize; }


            if (Column.MultiLine)
            {
                var TMP = Column.Database.Cell.GetArray(Column, Row);
                if (Column.ShowMultiLineInOneLine)
                {
                    _ContentSize = Skin.FormatedText_NeededSize(Column, TMP.JoinWith("; "), null, Cell_Font, enShortenStyle.Replaced);
                }
                else
                {

                    var TMPSize = Size.Empty;
                    for (var z = 0 ; z <= TMP.GetUpperBound(0) ; z++)
                    {
                        if (!string.IsNullOrEmpty(Column.Suffix)) { TMP[z] = TMP[z] + " " + Column.Suffix; }

                        TMPSize = Skin.FormatedText_NeededSize(Column, TMP[z], null, Cell_Font, enShortenStyle.Replaced);
                        if (TMPSize.Width > _ContentSize.Width) { _ContentSize.Width = TMPSize.Width; }

                        if (TMPSize.Height > 16)
                        {
                            _ContentSize.Height += TMPSize.Height;
                        }
                        else
                        {
                            _ContentSize.Height += 16;
                        }

                    }

                }

            }
            else
            {

                var _String = Column.Database.Cell.GetString(Column, Row);
                _ContentSize = Skin.FormatedText_NeededSize(Column, _String, null, Cell_Font, enShortenStyle.Replaced);
            }


            if (_ContentSize.Width < 16) { _ContentSize.Width = 16; }
            if (_ContentSize.Height < 16) { _ContentSize.Height = 16; }


            if (clsSkin.Scale == 1)
            {
                Column.Database.Cell.SetSizeOfCellContent(Column, Row, _ContentSize);

            }

            return _ContentSize;
        }


        public void DoUndo(ColumnItem Column, RowItem Row)
        {
            if (Column == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Spalte ungültig!<br>" + Database.Filename); }
            if (Row == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Zeile ungültig!<br>" + Database.Filename); }

            if (Column.Format == enDataFormat.LinkedCell)
            {
                Column.Database.Cell.LinkedCellData(Column, Row, out var LCColumn, out var LCrow);
                if (LCColumn != null && LCrow != null) { DoUndo(LCColumn, LCrow); }
                return;
            }

            var CellKey = CellCollection.KeyOfCell(Column, Row);

            var i = UndoItems(CellKey);

            if (i.Count < 1)
            {
                MessageBox.Show("Keine vorherigen Inhalte<br>(mehr) vorhanden.", enImageCode.Information, "OK");
                return;
            }

            i.Appearance = enBlueListBoxAppearance.Listbox;


            var v = InputBoxListBoxStyle.Show("Vorherigen Eintrag wählen:", i, enAddType.None, true);
            if (v == null || v.Count != 1) { return; }

            if (v[0] == "Cancel") { return; } // =Aktueller Eintrag angeklickt



            Database.Cell.Set(Column, Row, v[0].Substring(5));
            Row.DoAutomatic(false, true);
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


            for (var z = _Database.Works.Count - 1 ; z >= 0 ; z--)
            {

                if (_Database.Works[z].CellKey == CellKey)
                {
                    co += 1;
                    LasNr = z;
                    if (isfirst)
                    {
                        Las = new TextListItem("Cancel", "Aktueller Text - ab " + _Database.Works[z].Date + ", geändert von " + _Database.Works[z].User);
                    }
                    else
                    {
                        Las = new TextListItem(co.Nummer(5) + _Database.Works[z].ChangedTo, "ab " + _Database.Works[z].Date + ", geändert von " + _Database.Works[z].User);
                    }

                    isfirst = false;
                    if (Las != null) { i.Add(Las); }
                }
            }


            if (Las != null)
            {
                co += 1;
                i.Add(new TextListItem(co.Nummer(5) + _Database.Works[LasNr].PreviousValue, "vor " + _Database.Works[LasNr].Date));
            }


            return i;
        }

        public static void CopyToClipboard(ColumnItem Column, RowItem Row, bool Meldung)
        {

            if (Column.Format.CanBeCheckedByRules())
            {
                var c = Row.CellGetString(Column);
                c = c.Replace("\r\n", "\r");
                c = c.Replace("\r", "\r\n");

                System.Windows.Forms.Clipboard.SetDataObject(c, true);
                if (Meldung) { Notification.Show("<b>" + c + "</b><br>ist nun in der Zwischenablage.", enImageCode.Kopieren); }
            }
        }
    }
}
