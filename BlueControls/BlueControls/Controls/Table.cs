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
using BlueBasics.EventArgs;
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
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
using static BlueBasics.ListOfExtension;


namespace BlueControls.Controls {
    [Designer(typeof(BasicDesigner))]
    [DefaultEvent("CursorPosChanged")]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class Table : GenericControl, IContextMenu, IBackgroundNone {


        #region Constructor
        public Table() : base(true, false) {
            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();
            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            _MouseHighlight = false;
        }
        #endregion



        /// <summary>
        ///  Wird DatabaseAdded gehandlet?
        /// </summary>


        #region  Variablen 
        private Database _Database;
        private ColumnItem _CursorPosColumn;
        private clsRowDrawData _CursorPosRow;
        private ColumnItem _MouseOverColumn;
        private clsRowDrawData _MouseOverRow;
        private RowItem _Unterschiede;
        private string _MouseOverText;
        private AutoFilter _AutoFilter;
        private SearchAndReplace _searchAndReplace;
        private int? _HeadSize = null;
        private int _WiederHolungsSpaltenWidth;
        private int _ArrangementNr = 1;
        private RowSortDefinition _sortDefinitionTemporary;
        private enBlueTableAppearance _Design = enBlueTableAppearance.Standard;
        private List<clsRowDrawData> _SortedRowData; // Die Sortierung der Zeile
        private List<RowItem> _SortedRows; // Die Sortierung der Zeile
        private readonly List<RowItem> _SortedRowsBefore = new(); // Die Sortierung der Zeile
        private readonly List<RowItem> _PinnedRows = new();
        private readonly List<string> _collapsed = new();
        private readonly object Lock_UserAction = new();
        private BlueFont _Column_Font;
        private BlueFont _Chapter_Font;
        private BlueFont _Cell_Font;
        private BlueFont _NewRow_Font;
        private int Pix16 = 16;
        private int Pix18 = 18;
        private const int _AutoFilterSize = 22;
        private Rectangle tmpCursorRect = Rectangle.Empty;
        //private readonly FontSelectDialog _FDia = null;
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
        private static bool ServiceStarted = false;
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
        public event EventHandler FilterChanged;
        public event EventHandler VisibleRowsChanged;
        public event EventHandler PinnedChanged;
        public event EventHandler<FilterEventArgs> AutoFilterClicked;
        public event EventHandler DatabaseChanged;
        public event EventHandler<ButtonCellEventArgs> NeedButtonArgs;
        public event EventHandler<CellEventArgs> ButtonCellClicked;
        #endregion



        #region  Properties 
        public FilterCollection Filter { get; private set; }
        // <Obsolete("Database darf nicht im Designer gesetzt werden.", True)>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Database Database {
            get => _Database;
            set {
                if (_Database == value) { return; }
                CloseAllComponents();
                _collapsed.Clear();
                _PinnedRows.Clear();
                _MouseOverColumn = null;
                _MouseOverRow = null;
                _CursorPosColumn = null;
                _CursorPosRow = null;
                _Unterschiede = null;
                _MouseOverText = string.Empty;
                if (_Database != null) {
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
                    _Database.Row.RowRemoving -= Row_RowRemoving;
                    _Database.Row.RowRemoved -= _Database_RowRemoved;
                    _Database.Row.RowAdded -= _Database_Row_RowAdded;
                    _Database.Column.ItemRemoved -= _Database_ViewChanged;
                    _Database.Column.ItemAdded -= _Database_ViewChanged;
                    _Database.SavedToDisk -= _Database_SavedToDisk;
                    _Database.ColumnArrangements.ItemInternalChanged -= ColumnArrangements_ItemInternalChanged;
                    _Database.ProgressbarInfo -= _Database_ProgressbarInfo;
                    _Database.DropMessage -= _Database_DropMessage;
                    _Database.Disposing -= _Database_Disposing;
                    _Database.Save(false);         // Datenbank nicht reseten, weil sie ja anderweitig noch benutzt werden kann
                }
                ShowWaitScreen = true;
                Refresh(); // um die Uhr anzuzeigen
                _Database = value;
                InitializeSkin(); // Neue Schriftgrößen 
                if (_Database != null) {
                    _Database.Cell.CellValueChanged += _Database_CellValueChanged;
                    _Database.ConnectedControlsStopAllWorking += _Database_StopAllWorking;
                    _Database.Loaded += _Database_DatabaseLoaded;
                    _Database.Loading += _Database_StoreView;
                    _Database.ViewChanged += _Database_ViewChanged;
                    _Database.RowKeyChanged += _Database_RowKeyChanged;
                    _Database.ColumnKeyChanged += _Database_ColumnKeyChanged;
                    _Database.Column.ItemInternalChanged += _Database_ColumnContentChanged;
                    _Database.SortParameterChanged += _Database_SortParameterChanged;
                    _Database.Row.RowRemoving += Row_RowRemoving;
                    _Database.Row.RowRemoved += _Database_RowRemoved;
                    _Database.Row.RowAdded += _Database_Row_RowAdded;
                    _Database.Column.ItemAdded += _Database_ViewChanged;
                    _Database.Column.ItemRemoving += Column_ItemRemoving;
                    _Database.Column.ItemRemoved += _Database_ViewChanged;
                    _Database.SavedToDisk += _Database_SavedToDisk;
                    _Database.ColumnArrangements.ItemInternalChanged += ColumnArrangements_ItemInternalChanged;
                    _Database.ProgressbarInfo += _Database_ProgressbarInfo;
                    _Database.DropMessage += _Database_DropMessage;
                    _Database.Disposing += _Database_Disposing;
                }
                _Database_DatabaseLoaded(this, new LoadedEventArgs(false));
                ShowWaitScreen = false;
                Invalidate();
                OnDatabaseChanged();
            }
        }
        private void _Database_Disposing(object sender, System.EventArgs e) => Database = null;
        public bool ShowWaitScreen { get; set; } = true;
        private void Column_ItemRemoving(object sender, ListEventArgs e) {
            if (e.Item == _CursorPosColumn) {
                CursorPos_Reset();
            }
            if (e.Item == _MouseOverColumn) {
                _MouseOverColumn = null;
            }
        }
        private void Row_RowRemoving(object sender, RowEventArgs e) {
            if (e.Row == _CursorPosRow?.Row) { CursorPos_Reset(); }
            if (e.Row == _MouseOverRow?.Row) { _MouseOverRow = null; }
            if (_PinnedRows.Contains(e.Row)) { _PinnedRows.Remove(e.Row); }
        }
        private void _Database_Row_RowAdded(object sender, RowEventArgs e) {
            OnRowAdded(sender, e);
            Invalidate_RowSort();
            Invalidate();
        }
        //  <Obsolete("Database darf nicht im Designer gesetzt werden.", True)>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RowSortDefinition SortDefinitionTemporary {
            get => _sortDefinitionTemporary;
            set {
                if (_sortDefinitionTemporary != null && value != null && _sortDefinitionTemporary.ToString() == value.ToString()) { return; }
                if (_sortDefinitionTemporary == value) { return; }
                _sortDefinitionTemporary = value;
                _Database_SortParameterChanged(this, System.EventArgs.Empty);
            }
        }
        //  <Obsolete("Database darf nicht im Designer gesetzt werden.", True)>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RowItem Unterschiede {
            get => _Unterschiede;
            set {
                //if (_Unterschiede != null && value != null && _sortDefinitionTemporary.ToString() == value.ToString()) { return; }
                if (_Unterschiede == value) { return; }
                _Unterschiede = value;
                Invalidate();
            }
        }
        [DefaultValue(1.0f)]
        public double FontScale => _Database == null ? 1f : _Database.GlobalScale;
        [DefaultValue(false)]
        public bool ShowNumber {
            get => _ShowNumber;
            set {
                if (value == _ShowNumber) { return; }
                CloseAllComponents();
                _ShowNumber = value;
                Invalidate();
            }
        }
        [DefaultValue(enBlueTableAppearance.Standard)]
        public enBlueTableAppearance Design {
            get => _Design;
            set {
                SliderY.Visible = true;
                SliderX.Visible = Convert.ToBoolean(value == enBlueTableAppearance.Standard);
                if (value == _Design) { return; }
                CloseAllComponents();
                _Design = value;
                if (!SliderX.Visible) {
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
        private void DrawWeitScreen(Graphics GR) {
            if (SliderX != null) { SliderX.Enabled = false; }
            if (SliderY != null) { SliderY.Enabled = false; }
            Skin.Draw_Back(GR, enDesign.Table_And_Pad, enStates.Standard_Disabled, DisplayRectangle, this, true);
            var i = QuickImage.Get(enImageCode.Uhr, 64).BMP;
            GR.DrawImage(i, (Width - 64) / 2, (Height - 64) / 2);
            Skin.Draw_Border(GR, enDesign.Table_And_Pad, enStates.Standard_Disabled, DisplayRectangle);
        }
        protected void InitializeSkin() {
            _Cell_Font = Skin.GetBlueFont(enDesign.Table_Cell, enStates.Standard).Scale(FontScale);
            _Column_Font = Skin.GetBlueFont(enDesign.Table_Column, enStates.Standard).Scale(FontScale);
            _Chapter_Font = Skin.GetBlueFont(enDesign.Table_Cell_Chapter, enStates.Standard).Scale(FontScale);
            _NewRow_Font = Skin.GetBlueFont(enDesign.Table_Cell_New, enStates.Standard).Scale(FontScale);
            if (Database != null) {
                Pix16 = GetPix(16, _Cell_Font, Database.GlobalScale);
                Pix18 = GetPix(18, _Cell_Font, Database.GlobalScale);
            } else {
                Pix16 = 16;
                Pix18 = 18;
            }
        }
        public void Pin(List<RowItem> rows) {
            if (rows == null) { rows = new List<RowItem>(); }
            rows = rows.Distinct().ToList();
            if (!rows.IsDifferentTo(_PinnedRows)) { return; }
            Invalidate_Filterinfo();
            _PinnedRows.Clear();
            _PinnedRows.AddRange(rows);
            Invalidate_RowSort();
            Invalidate();
            OnPinnedChanged();
        }
        private static int GetPix(int Pix, BlueFont F, double Scale) => Skin.FormatedText_NeededSize("@|", null, F, (int)((Pix * Scale) + 0.5)).Height;
        protected override void DrawControl(Graphics gr, enStates state) {
            if (InvokeRequired) {
                Invoke(new Action(() => DrawControl(gr, state)));
                return;
            }
            // Listboxen bekommen keinen Focus, also Tabellen auch nicht. Basta.
            if (Convert.ToBoolean(state & enStates.Standard_HasFocus)) {
                state ^= enStates.Standard_HasFocus;
            }
            if (_Database == null || DesignMode || ShowWaitScreen) {
                DrawWeitScreen(gr);
                return;
            }
            lock (Lock_UserAction) {
                //if (_InvalidExternal) { FillExternalControls(); }
                if (Convert.ToBoolean(state & enStates.Standard_Disabled)) { CursorPos_Reset(); }
                var displayRectangleWOSlider = DisplayRectangleWithoutSlider();
                // Haupt-Aufbau-Routine ------------------------------------
                gr.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                if (!ComputeAllColumnPositions()) {
                    DrawWeitScreen(gr);
                    return;
                }
                var FirstVisibleRow = SortedRowData().Count;
                var LastVisibleRow = -1;
                foreach (var thisRow in SortedRowData()) {
                    if (IsOnScreen(thisRow, displayRectangleWOSlider)) {
                        var T = SortedRowData().IndexOf(thisRow);
                        FirstVisibleRow = Math.Min(T, FirstVisibleRow);
                        LastVisibleRow = Math.Max(T, LastVisibleRow);
                    }
                }
                switch (_Design) {

                    case enBlueTableAppearance.Standard:
                        Draw_Table_Std(gr, state, displayRectangleWOSlider, FirstVisibleRow, LastVisibleRow);
                        break;

                    case enBlueTableAppearance.OnlyMainColumnWithoutHead:
                        Draw_Table_ListboxStyle(gr, state, displayRectangleWOSlider, FirstVisibleRow, LastVisibleRow);
                        break;
                    default:
                        Develop.DebugPrint(_Design);
                        break;
                }
            }
        }
        private void Draw_Table_What(Graphics GR, enTableDrawColumn col, enTableDrawType type, int PermaX, Rectangle displayRectangleWOSlider, int FirstVisibleRow, int LastVisibleRow) {
            var lfdno = 0;
            if (type == enTableDrawType.PinnedRows) {
                if (col == enTableDrawColumn.Permament) {
                    Draw_Pinned(GR, displayRectangleWOSlider, 0, PermaX);
                }
                if (col == enTableDrawColumn.NonPermament) {
                    Draw_Pinned(GR, displayRectangleWOSlider, PermaX, displayRectangleWOSlider.Width);
                }
                return;
            }
            foreach (var ViewItem in CurrentArrangement) {
                if (ViewItem != null && ViewItem.Column != null) {
                    lfdno++;
                    if (IsOnScreen(ViewItem, displayRectangleWOSlider)) {
                        if ((col == enTableDrawColumn.NonPermament && ViewItem.ViewType != enViewType.PermanentColumn && (int)ViewItem.OrderTMP_Spalte_X1 + (int)ViewItem._TMP_DrawWidth > PermaX) ||
                            (col == enTableDrawColumn.Permament && ViewItem.ViewType == enViewType.PermanentColumn)) {
                            switch (type) {

                                case enTableDrawType.ColumnBackBody:
                                    Draw_Column_Body(GR, ViewItem, displayRectangleWOSlider);
                                    break;

                                case enTableDrawType.Cells:
                                    Draw_Column_Cells(GR, ViewItem, displayRectangleWOSlider, FirstVisibleRow, LastVisibleRow, lfdno);
                                    break;

                                case enTableDrawType.ColumnHead:
                                    Draw_Column_Head(GR, ViewItem, displayRectangleWOSlider, lfdno);
                                    break;
                            }
                        }
                    }
                }
            }
        }
        private void Draw_Column_Cells(Graphics GR, ColumnViewItem ViewItem, Rectangle displayRectangleWOSlider, int FirstVisibleRow, int LastVisibleRow, int lfdno) {
            // Die Cursorposition ermittleln
            if (!Thread.CurrentThread.IsBackground && _CursorPosColumn != null && _CursorPosRow != null && ViewItem.Column == _CursorPosColumn) {
                if (IsOnScreen(_CursorPosColumn, _CursorPosRow, displayRectangleWOSlider)) {
                    tmpCursorRect = new Rectangle((int)ViewItem.OrderTMP_Spalte_X1, DrawY(_CursorPosRow) + 1, Column_DrawWidth(ViewItem, displayRectangleWOSlider), _CursorPosRow.DrawHeight - 1);
                    Draw_Cursor(GR, displayRectangleWOSlider, false);
                }
            }
            //  Neue Zeile 
            if (UserEdit_NewRowAllowed() && ViewItem == CurrentArrangement[Database.Column[0]]) {
                Skin.Draw_FormatedText(GR, "[Neue Zeile]", QuickImage.Get(enImageCode.PlusZeichen, Pix16), enAlignment.Left, new Rectangle((int)ViewItem.OrderTMP_Spalte_X1 + 1, (int)(-SliderY.Value + HeadSize() + 1), (int)ViewItem._TMP_DrawWidth - 2, 16 - 2), this, false, _NewRow_Font, Translate);
            }
            // Zeilen Zeichnen (Alle Zellen)
            for (var Zei = FirstVisibleRow; Zei <= LastVisibleRow; Zei++) {
                var CurrentRow = SortedRowData()[Zei];
                GR.SmoothingMode = SmoothingMode.None;
                if (CurrentRow.Expanded) {
                    GR.DrawLine(Skin.Pen_LinieDünn, (int)ViewItem.OrderTMP_Spalte_X1, DrawY(CurrentRow), (int)ViewItem.OrderTMP_Spalte_X1 + Column_DrawWidth(ViewItem, displayRectangleWOSlider) - 1, DrawY(CurrentRow));
                    // Zelleninhalt Zeichnen
                    Draw_CellTransparent(GR, ViewItem, CurrentRow, displayRectangleWOSlider, _Cell_Font);
                }
                if (_Unterschiede != null && _Unterschiede != CurrentRow.Row) {
                    if (CurrentRow.Row.CellGetString(ViewItem.Column) != _Unterschiede.CellGetString(ViewItem.Column)) {
                        Rectangle tmpr = new((int)ViewItem.OrderTMP_Spalte_X1 + 1, DrawY(CurrentRow) + 1, Column_DrawWidth(ViewItem, displayRectangleWOSlider) - 2, CurrentRow.DrawHeight - 2);
                        GR.DrawRectangle(new Pen(Color.Red, 1), tmpr);
                    }
                }
                if (lfdno == 1) {
                    // Überschrift in der ersten Spalte zeichnen
                    CurrentRow.CaptionPos = Rectangle.Empty;
                    if (!string.IsNullOrEmpty(CurrentRow.Chapter)) {
                        var si = GR.MeasureString(CurrentRow.Chapter, _Chapter_Font.Font());
                        GR.FillRectangle(new SolidBrush(Skin.Color_Back(enDesign.Table_And_Pad, enStates.Standard).SetAlpha(50)), 1, DrawY(CurrentRow) - RowCaptionSizeY, displayRectangleWOSlider.Width - 2, RowCaptionSizeY);
                        CurrentRow.CaptionPos = new Rectangle(1, DrawY(CurrentRow) - RowCaptionFontY, (int)si.Width + 28, (int)si.Height);
                        if (_collapsed.Contains(CurrentRow.Chapter)) {
                            Button.DrawButton(this, GR, enDesign.Button_CheckBox, enStates.Checked, null, enAlignment.Horizontal_Vertical_Center, false, null, string.Empty, CurrentRow.CaptionPos, false);
                            GR.DrawImage(QuickImage.Get("Pfeil_Unten_Scrollbar|14|||FF0000||200|200").BMP, 5, DrawY(CurrentRow) - RowCaptionFontY + 6);
                        } else {
                            Button.DrawButton(this, GR, enDesign.Button_CheckBox, enStates.Standard, null, enAlignment.Horizontal_Vertical_Center, false, null, string.Empty, CurrentRow.CaptionPos, false);
                            GR.DrawImage(QuickImage.Get("Pfeil_Rechts_Scrollbar|14|||||0").BMP, 5, DrawY(CurrentRow) - RowCaptionFontY + 6);
                        }
                        GR.DrawString(CurrentRow.Chapter, _Chapter_Font.Font(), _Chapter_Font.Brush_Color_Main, 23, DrawY(CurrentRow) - RowCaptionFontY);
                        GR.DrawLine(Skin.Pen_LinieDick, 0, DrawY(CurrentRow), displayRectangleWOSlider.Width, DrawY(CurrentRow));
                    }
                }
            }
        }
        private void SliderSchaltenWaage(Rectangle DisplayR, int MaxX) {
            SliderX.Minimum = 0;
            SliderX.Maximum = MaxX - DisplayR.Width + 1;
            SliderX.LargeChange = DisplayR.Width;
            SliderX.Enabled = Convert.ToBoolean(SliderX.Maximum > 0);
        }
        private void SliderSchaltenSenk(Rectangle DisplayR, int MaxY) {
            SliderY.Minimum = 0;
            SliderY.Maximum = Math.Max(MaxY - DisplayR.Height + 1 + HeadSize(), 0);
            SliderY.LargeChange = DisplayR.Height - HeadSize();
            SliderY.Enabled = Convert.ToBoolean(SliderY.Maximum > 0);
        }
        private void Draw_Table_Std(Graphics GR, enStates State, Rectangle displayRectangleWOSlider, int FirstVisibleRow, int LastVisibleRow) {
            try {
                tmpCursorRect = Rectangle.Empty;
                if (_Database.ColumnArrangements == null || _ArrangementNr >= _Database.ColumnArrangements.Count) { return; }   // Kommt vor, dass spontan doch geparsed wird...
                Skin.Draw_Back(GR, enDesign.Table_And_Pad, State, DisplayRectangle, this, true);
                /// Maximale Rechten Pixel der Permanenten Columns ermitteln
                var PermaX = 0;
                foreach (var ViewItem in CurrentArrangement) {
                    if (ViewItem != null && ViewItem.Column != null && ViewItem.ViewType == enViewType.PermanentColumn) {
                        if (ViewItem._TMP_DrawWidth == null) {
                            // Veränderte Werte!
                            DrawControl(GR, State);
                            return;
                        }
                        PermaX = Math.Max(PermaX, (int)ViewItem.OrderTMP_Spalte_X1 + (int)ViewItem._TMP_DrawWidth);
                    }
                }
                Draw_Table_What(GR, enTableDrawColumn.NonPermament, enTableDrawType.ColumnBackBody, PermaX, displayRectangleWOSlider, FirstVisibleRow, LastVisibleRow);
                Draw_Table_What(GR, enTableDrawColumn.NonPermament, enTableDrawType.PinnedRows, PermaX, displayRectangleWOSlider, FirstVisibleRow, LastVisibleRow);
                Draw_Table_What(GR, enTableDrawColumn.NonPermament, enTableDrawType.Cells, PermaX, displayRectangleWOSlider, FirstVisibleRow, LastVisibleRow);
                Draw_Table_What(GR, enTableDrawColumn.Permament, enTableDrawType.ColumnBackBody, PermaX, displayRectangleWOSlider, FirstVisibleRow, LastVisibleRow);
                Draw_Table_What(GR, enTableDrawColumn.Permament, enTableDrawType.PinnedRows, PermaX, displayRectangleWOSlider, FirstVisibleRow, LastVisibleRow);
                Draw_Table_What(GR, enTableDrawColumn.Permament, enTableDrawType.Cells, PermaX, displayRectangleWOSlider, FirstVisibleRow, LastVisibleRow);
                //// PinnedRowsMarkieren
                //Draw_Pinned(GR, displayRectangleWOSlider);
                // Den CursorLines zeichnen
                Draw_Cursor(GR, displayRectangleWOSlider, true);
                Draw_Table_What(GR, enTableDrawColumn.NonPermament, enTableDrawType.ColumnHead, PermaX, displayRectangleWOSlider, FirstVisibleRow, LastVisibleRow);
                Draw_Table_What(GR, enTableDrawColumn.Permament, enTableDrawType.ColumnHead, PermaX, displayRectangleWOSlider, FirstVisibleRow, LastVisibleRow);
                /// Überschriften 1-3 Zeichnen
                Draw_Column_Head_Captions(GR);
                Skin.Draw_Border(GR, enDesign.Table_And_Pad, State, displayRectangleWOSlider);
                if (Database.ReloadNeeded) { GR.DrawImage(QuickImage.Get(enImageCode.Uhr, 16).BMP, 8, 8); }
                if (Database.HasPendingChanges()) { GR.DrawImage(QuickImage.Get(enImageCode.Stift, 16).BMP, 16, 8); }
                if (Database.ReadOnly) { GR.DrawImage(QuickImage.Get(enImageCode.Schloss, 32).BMP, 16, 8); }
            } catch (Exception ex) {
                Develop.DebugPrint(ex);
            }
        }
        private void Draw_Pinned(Graphics gR, Rectangle displayRectangleWOSlider, int startx, int endx) {
            if (_PinnedRows == null || _PinnedRows.Count == 0) { return; }
            SolidBrush b = new(Color.FromArgb(180, 255, 255, 0));
            foreach (var ThisRowItem in _PinnedRows) {
                var r = SortedRowData().Get(ThisRowItem);
                var y = DrawY(r);
                if (y >= 0 && y < displayRectangleWOSlider.Height) {
                    gR.FillRectangle(b, startx, y, endx - startx, r.DrawHeight);
                }
            }
        }
        private void Draw_Column_Head_Captions(Graphics GR) {
            var BVI = new ColumnViewItem[3];
            var LCBVI = new ColumnViewItem[3];
            ColumnViewItem ViewItem;
            ColumnViewItem LastViewItem = null;
            var PermaX = 0;
            var ca = CurrentArrangement;
            for (var X = 0; X < ca.Count() + 1; X++) {
                ViewItem = X < ca.Count() ? ca[X] : null;
                if (ViewItem?.ViewType == enViewType.PermanentColumn) {
                    PermaX = Math.Max(PermaX, (int)ViewItem.OrderTMP_Spalte_X1 + (int)ViewItem._TMP_DrawWidth);
                }
                if (ViewItem == null ||
                    ViewItem.ViewType == enViewType.PermanentColumn ||
                     (int)ViewItem.OrderTMP_Spalte_X1 + (int)ViewItem._TMP_DrawWidth > PermaX) {
                    for (var u = 0; u < 3; u++) {
                        var N = ViewItem?.Column.Ueberschrift(u);
                        var V = BVI[u]?.Column.Ueberschrift(u);
                        if (N != V) {
                            if (!string.IsNullOrEmpty(V) && LastViewItem != null) {
                                var LE = Math.Max(0, (int)BVI[u].OrderTMP_Spalte_X1);
                                //int RE = (int)LastViewItem.OrderTMP_Spalte_X1 - 1 ;
                                var RE = (int)LastViewItem.OrderTMP_Spalte_X1 + (int)LastViewItem._TMP_DrawWidth - 1;
                                if (ViewItem?.ViewType != enViewType.PermanentColumn && BVI[u].ViewType != enViewType.PermanentColumn) { LE = Math.Max(LE, PermaX); }
                                if (ViewItem?.ViewType != enViewType.PermanentColumn && BVI[u].ViewType == enViewType.PermanentColumn) { RE = Math.Max(RE, (int)LCBVI[u].OrderTMP_Spalte_X1 + (int)LCBVI[u]._TMP_DrawWidth); }
                                if (LE < RE) {
                                    Rectangle r = new(LE, u * ColumnCaptionSizeY, RE - LE, ColumnCaptionSizeY);
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
        private void Draw_Cursor(Graphics GR, Rectangle displayRectangleWOSlider, bool OnlyCursorLines) {
            if (tmpCursorRect.Width < 1) { return; }
            var stat = enStates.Standard;
            if (Focused()) { stat = enStates.Standard_HasFocus; }
            if (OnlyCursorLines) {
                Pen pen = new(Skin.Color_Border(enDesign.Table_Cursor, stat).SetAlpha(180));
                GR.DrawRectangle(pen, new Rectangle(-1, tmpCursorRect.Top - 1, displayRectangleWOSlider.Width + 2, tmpCursorRect.Height + 1));
            } else {
                Skin.Draw_Back(GR, enDesign.Table_Cursor, stat, tmpCursorRect, this, false);
                Skin.Draw_Border(GR, enDesign.Table_Cursor, stat, tmpCursorRect);
            }
        }
        private void Draw_Table_ListboxStyle(Graphics GR, enStates vState, Rectangle displayRectangleWOSlider, int vFirstVisibleRow, int vLastVisibleRow) {
            var ItStat = vState;
            Skin.Draw_Back(GR, enDesign.ListBox, vState, DisplayRectangle, this, true);
            var Col = Database.Column[0];
            // Zeilen Zeichnen (Alle Zellen)
            for (var Zeiv = vFirstVisibleRow; Zeiv <= vLastVisibleRow; Zeiv++) {
                var Row = SortedRowData()[Zeiv];
                var ViewItem = _Database.ColumnArrangements[0][Col];
                Rectangle r = new(0, DrawY(Row), DisplayRectangleWithoutSlider().Width, Row.DrawHeight);
                if (_CursorPosColumn != null && _CursorPosRow.Row == Row.Row) {
                    ItStat |= enStates.Checked;
                } else {
                    if (Convert.ToBoolean(ItStat & enStates.Checked)) {
                        ItStat ^= enStates.Checked;
                    }
                }
                ViewItem.OrderTMP_Spalte_X1 = 0;
                Draw_CellListBox(GR, ViewItem, Row, r, enDesign.Item_Listbox, ItStat);
                if (!Row.Row.CellGetBoolean(_Database.Column.SysCorrect)) {
                    GR.DrawImage(QuickImage.Get("Warnung|16||||||120||50").BMP, new Point(r.Right - 19, (int)(r.Top + ((r.Height - 16) / 2.0))));
                }
                if (!string.IsNullOrEmpty(Row.Chapter)) {
                    GR.DrawString(Row.Chapter, _Chapter_Font.Font(), _Chapter_Font.Brush_Color_Main, 0, DrawY(Row) - RowCaptionFontY);
                }
            }
            Skin.Draw_Border(GR, enDesign.ListBox, vState, displayRectangleWOSlider);
        }
        public void ImportClipboard() {
            Develop.DebugPrint_InvokeRequired(InvokeRequired, false);
            if (!System.Windows.Forms.Clipboard.ContainsText()) {
                Notification.Show("Abbruch,<br>kein Text im Zwischenspeicher!", enImageCode.Information);
                return;
            }
            //    If Not Editablexx("Import gerade nicht möglich") Then Exit Sub
            var nt = Convert.ToString(System.Windows.Forms.Clipboard.GetDataObject().GetData(System.Windows.Forms.DataFormats.Text));
            ImportCSV(nt);
        }
        public static void ImportCSV(Database _Database, string csvtxt) {
            using Import x = new(_Database, csvtxt);
            x.ShowDialog();
        }
        public void ImportCSV(string csvtxt) => ImportCSV(_Database, csvtxt);
        /// <summary>
        /// Zeichnet die gesamte Zelle ohne Hintergrund. Die verlinkte Zelle ist bereits bekannt.
        /// </summary>
        /// <param name="GR"></param>
        /// <param name="cellInThisDatabaseColumn"></param>
        /// <param name="cellInThisDatabaseRow"></param>
        /// <param name="rowY"></param>
        /// <param name="ContentHolderCellColumn"></param>
        /// <param name="ContentHolderCellRow"></param>
        /// <param name="displayRectangleWOSlider"></param>
        /// <param name="vfont"></param>
        private void Draw_CellTransparentDirect(Graphics GR, ColumnViewItem cellInThisDatabaseColumn, clsRowDrawData cellInThisDatabaseRow, ColumnItem ContentHolderCellColumn, RowItem ContentHolderCellRow, Rectangle displayRectangleWOSlider, BlueFont vfont) {
            //var rh = Row_DrawHeight(cellInThisDatabaseRow, displayRectangleWOSlider);
            var cw = Column_DrawWidth(cellInThisDatabaseColumn, displayRectangleWOSlider);
            if (cellInThisDatabaseColumn.Column.Format == enDataFormat.Button) {
                Draw_CellAsButton(GR, cellInThisDatabaseColumn, cellInThisDatabaseRow, cw);
                return;
            }
            var toDraw = ContentHolderCellRow.CellGetString(ContentHolderCellColumn);
            if (toDraw == null) { toDraw = string.Empty; }
            if (!ContentHolderCellColumn.MultiLine || !toDraw.Contains("\r")) {
                Draw_CellTransparentDirect_OneLine(GR, toDraw, cellInThisDatabaseColumn, DrawY(cellInThisDatabaseRow), 0, ContentHolderCellColumn, true, vfont, cellInThisDatabaseRow.DrawHeight, cw);
            } else {
                var MEI = toDraw.SplitByCR();
                if (ContentHolderCellColumn.ShowMultiLineInOneLine) {
                    Draw_CellTransparentDirect_OneLine(GR, MEI.JoinWith("; "), cellInThisDatabaseColumn, DrawY(cellInThisDatabaseRow), 0, ContentHolderCellColumn, true, vfont, cellInThisDatabaseRow.DrawHeight, cw);
                } else {
                    var y = 0;
                    for (var z = 0; z <= MEI.GetUpperBound(0); z++) {
                        Draw_CellTransparentDirect_OneLine(GR, MEI[z], cellInThisDatabaseColumn, DrawY(cellInThisDatabaseRow), y, ContentHolderCellColumn, Convert.ToBoolean(z == MEI.GetUpperBound(0)), vfont, cellInThisDatabaseRow.DrawHeight, cw);
                        y += FormatedText_NeededSize(cellInThisDatabaseColumn.Column, MEI[z], vfont, enShortenStyle.Replaced, Pix16 - 1, cellInThisDatabaseColumn.Column.BildTextVerhalten).Height;
                    }
                }
            }
        }
        public void CollapesAll() {
            _collapsed.Clear();
            if (Database != null) {
                foreach (var thisr in Database.Row) {
                    _collapsed.AddIfNotExists(thisr.CaptionReadable());
                }
            }
            Invalidate_RowSort();
            Invalidate();
        }
        public void ExpandAll() {
            _collapsed.Clear();
            Invalidate_RowSort();
            Invalidate();
        }
        private void Draw_CellAsButton(Graphics gR, ColumnViewItem cellInThisDatabaseColumn, clsRowDrawData cellInThisDatabaseRow, int drawColumnWidth) {
            ButtonCellEventArgs e = new(cellInThisDatabaseColumn.Column, cellInThisDatabaseRow.Row);
            OnNeedButtonArgs(e);
            Rectangle r = new((int)cellInThisDatabaseColumn.OrderTMP_Spalte_X1,
                                       DrawY(cellInThisDatabaseRow),
                                       drawColumnWidth,
                                        Math.Min(cellInThisDatabaseRow.DrawHeight, 24));
            var s = enStates.Standard;
            if (!Enabled) { s = enStates.Standard_Disabled; }
            if (e.Cecked) { s |= enStates.Checked; }
            Button.DrawButton(this, gR, enDesign.Button_CheckBox, s, e.Image, enAlignment.Horizontal_Vertical_Center, false, null, e.Text, r, true);
        }
        /// <summary>
        /// Zeichnet die gesamte Zelle ohne Hintergrund und prüft noch, ob der verlinkte Inhalt gezeichnet werden soll.
        /// </summary>
        /// <param name="GR"></param>
        /// <param name="cellInThisDatabaseColumn"></param>
        /// <param name="cellInThisDatabaseRow"></param>
        /// <param name="displayRectangleWOSlider"></param>
        /// <param name="vfont"></param>
        private void Draw_CellTransparent(Graphics GR, ColumnViewItem cellInThisDatabaseColumn, clsRowDrawData cellInThisDatabaseRow, Rectangle displayRectangleWOSlider, BlueFont vfont) {
            if (cellInThisDatabaseRow == null) { return; }
            if (cellInThisDatabaseColumn.Column.Format == enDataFormat.LinkedCell) {
                (var lcolumn, var lrow) = CellCollection.LinkedCellData(cellInThisDatabaseColumn.Column, cellInThisDatabaseRow.Row, false, false);
                if (lcolumn != null && lrow != null) {
                    Draw_CellTransparentDirect(GR, cellInThisDatabaseColumn, cellInThisDatabaseRow, lcolumn, lrow, displayRectangleWOSlider, vfont);
                }
                return;
            }
            Draw_CellTransparentDirect(GR, cellInThisDatabaseColumn, cellInThisDatabaseRow, cellInThisDatabaseColumn.Column, cellInThisDatabaseRow.Row, displayRectangleWOSlider, vfont);
        }
        private void Draw_Column_Body(Graphics GR, ColumnViewItem cellInThisDatabaseColumn, Rectangle displayRectangleWOSlider) {
            GR.SmoothingMode = SmoothingMode.None;
            GR.FillRectangle(new SolidBrush(cellInThisDatabaseColumn.Column.BackColor), (int)cellInThisDatabaseColumn.OrderTMP_Spalte_X1, HeadSize(), Column_DrawWidth(cellInThisDatabaseColumn, displayRectangleWOSlider), displayRectangleWOSlider.Height);
            Draw_Border(GR, cellInThisDatabaseColumn, displayRectangleWOSlider, false);
        }
        private void Draw_Border(Graphics GR, ColumnViewItem vcolumn, Rectangle displayRectangleWOSlider, bool Onlyhead) {
            var yPos = displayRectangleWOSlider.Height;
            if (Onlyhead) { yPos = HeadSize(); }
            for (var z = 0; z <= 1; z++) {
                int xPos;
                enColumnLineStyle Lin;
                if (z == 0) {
                    xPos = (int)vcolumn.OrderTMP_Spalte_X1;
                    Lin = vcolumn.Column.LineLeft;
                } else {
                    xPos = (int)vcolumn.OrderTMP_Spalte_X1 + Column_DrawWidth(vcolumn, displayRectangleWOSlider);
                    Lin = vcolumn.Column.LineRight;
                }
                switch (Lin) {

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
        private void Draw_Column_Head(Graphics GR, ColumnViewItem ViewItem, Rectangle displayRectangleWOSlider, int lfdNo) {
            if (!IsOnScreen(ViewItem, displayRectangleWOSlider)) { return; }
            if (_Design == enBlueTableAppearance.OnlyMainColumnWithoutHead) { return; }
            if (_Column_Font == null) { return; }
            GR.FillRectangle(new SolidBrush(ViewItem.Column.BackColor), (int)ViewItem.OrderTMP_Spalte_X1, 0, Column_DrawWidth(ViewItem, displayRectangleWOSlider), HeadSize());
            Draw_Border(GR, ViewItem, displayRectangleWOSlider, true);
            GR.FillRectangle(new SolidBrush(Color.FromArgb(100, 200, 200, 200)), (int)ViewItem.OrderTMP_Spalte_X1, 0, Column_DrawWidth(ViewItem, displayRectangleWOSlider), HeadSize());
            var Down = 0;
            if (!string.IsNullOrEmpty(ViewItem.Column.Ueberschrift3)) {
                Down = ColumnCaptionSizeY * 3;
            } else if (!string.IsNullOrEmpty(ViewItem.Column.Ueberschrift2)) {
                Down = ColumnCaptionSizeY * 2;
            } else if (!string.IsNullOrEmpty(ViewItem.Column.Ueberschrift1)) {
                Down = ColumnCaptionSizeY;
            }

            #region Recude-Button zeichnen
            if (Column_DrawWidth(ViewItem, displayRectangleWOSlider) > 70 || ViewItem._TMP_Reduced) {
                ViewItem._TMP_ReduceLocation = new Rectangle((int)ViewItem.OrderTMP_Spalte_X1 + Column_DrawWidth(ViewItem, displayRectangleWOSlider) - 18, Down, 18, 18);
                if (ViewItem._TMP_Reduced) {
                    GR.DrawImage(QuickImage.Get("Pfeil_Rechts|16|||FF0000|||||20").BMP, ViewItem._TMP_ReduceLocation.Left + 2, ViewItem._TMP_ReduceLocation.Top + 2);
                } else {
                    GR.DrawImage(QuickImage.Get("Pfeil_Links|16||||||||75").BMP, ViewItem._TMP_ReduceLocation.Left + 2, ViewItem._TMP_ReduceLocation.Top + 2);
                }
            }
            #endregion



            #region Filter-Knopf mit Trichter
            var TrichterText = string.Empty;
            QuickImage TrichterIcon = null;
            var TrichterState = enStates.Undefiniert;
            ViewItem._TMP_AutoFilterLocation = new Rectangle((int)ViewItem.OrderTMP_Spalte_X1 + Column_DrawWidth(ViewItem, displayRectangleWOSlider) - _AutoFilterSize, HeadSize() - _AutoFilterSize, _AutoFilterSize, _AutoFilterSize);
            var filtIt = Filter[ViewItem.Column];
            if (ViewItem.Column.AutoFilterSymbolPossible()) {
                if (filtIt != null) {
                    TrichterState = enStates.Checked;
                    var anz = Autofilter_Text(ViewItem.Column);
                    TrichterText = anz > -100 ? (anz * -1).ToString() : "∞";
                } else {
                    TrichterState = Autofilter_Sinnvoll(ViewItem.Column) ? enStates.Standard : enStates.Standard_Disabled;
                }
            }
            var TrichterSize = (_AutoFilterSize - 4).ToString();
            if (filtIt != null) {
                TrichterIcon = QuickImage.Get("Trichter|" + TrichterSize + "|||FF0000");
            } else if (Filter.MayHasRowFilter(ViewItem.Column)) {
                TrichterIcon = QuickImage.Get("Trichter|" + TrichterSize + "|||227722");
            } else if (ViewItem.Column.AutoFilterSymbolPossible()) {
                TrichterIcon = Autofilter_Sinnvoll(ViewItem.Column)
                    ? QuickImage.Get("Trichter|" + TrichterSize)
                    : QuickImage.Get("Trichter|" + TrichterSize + "||128");
            }
            if (TrichterState != enStates.Undefiniert) {
                Skin.Draw_Back(GR, enDesign.Button_AutoFilter, TrichterState, ViewItem._TMP_AutoFilterLocation, null, false);
                Skin.Draw_Border(GR, enDesign.Button_AutoFilter, TrichterState, ViewItem._TMP_AutoFilterLocation);
            }
            if (TrichterIcon != null) {
                GR.DrawImage(TrichterIcon.BMP, ViewItem._TMP_AutoFilterLocation.Left + 2, ViewItem._TMP_AutoFilterLocation.Top + 2);
            }
            if (!string.IsNullOrEmpty(TrichterText)) {
                var s = _Column_Font.MeasureString(TrichterText);
                for (var x = -1; x < 2; x++) {
                    for (var y = -1; y < 2; y++) {
                        GR.DrawString(TrichterText, _Column_Font.Font(), Brushes.Red,
                                           ViewItem._TMP_AutoFilterLocation.Left + ((_AutoFilterSize - s.Width) / 2) + x,
                                           ViewItem._TMP_AutoFilterLocation.Top + ((_AutoFilterSize - s.Height) / 2) + y);
                    }
                }
                GR.DrawString(TrichterText, _Column_Font.Font(), Brushes.White,
                                    ViewItem._TMP_AutoFilterLocation.Left + ((_AutoFilterSize - s.Width) / 2),
                                    ViewItem._TMP_AutoFilterLocation.Top + ((_AutoFilterSize - s.Height) / 2));
            }
            if (TrichterState == enStates.Undefiniert) {
                ViewItem._TMP_AutoFilterLocation = new Rectangle(0, 0, 0, 0);
            }
            #endregion



            #region LaufendeNummer
            if (_ShowNumber) {
                for (var x = -1; x < 2; x++) {
                    for (var y = -1; y < 2; y++) {
                        GR.DrawString("#" + lfdNo.ToString(), _Column_Font.Font(), Brushes.Black, (int)ViewItem.OrderTMP_Spalte_X1 + x, ViewItem._TMP_AutoFilterLocation.Top + y);
                    }
                }
                GR.DrawString("#" + lfdNo.ToString(), _Column_Font.Font(), Brushes.White, (int)ViewItem.OrderTMP_Spalte_X1, ViewItem._TMP_AutoFilterLocation.Top);
            }
            #endregion


            var tx = ViewItem.Column.Caption;
            tx = LanguageTool.DoTranslate(tx, Translate).Replace("\r", "\r\n");
            var FS = GR.MeasureString(tx, _Column_Font.Font());
            if (!string.IsNullOrEmpty(ViewItem.Column.CaptionBitmap) && ViewItem.Column.TMP_CaptionBitmap == null) {
                ViewItem.Column.TMP_CaptionBitmap = QuickImage.Get(ViewItem.Column.CaptionBitmap).BMP;
            }
            if (ViewItem.Column.TMP_CaptionBitmap != null && ViewItem.Column.TMP_CaptionBitmap.Width > 10) {
                Point pos = new((int)ViewItem.OrderTMP_Spalte_X1 + (int)((Column_DrawWidth(ViewItem, displayRectangleWOSlider) - FS.Width) / 2.0), 3 + Down);
                GR.DrawImageInRectAspectRatio(ViewItem.Column.TMP_CaptionBitmap, (int)ViewItem.OrderTMP_Spalte_X1 + 2, (int)(pos.Y + FS.Height), Column_DrawWidth(ViewItem, displayRectangleWOSlider) - 4, HeadSize() - (int)(pos.Y + FS.Height) - 6 - 18);
                // Dann der Text
                GR.TranslateTransform(pos.X, pos.Y);
                GR.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                GR.DrawString(tx, _Column_Font.Font(), new SolidBrush(ViewItem.Column.ForeColor), 0, 0);
                GR.TranslateTransform(-pos.X, -pos.Y);
            } else {
                Point pos = new((int)ViewItem.OrderTMP_Spalte_X1 + (int)((Column_DrawWidth(ViewItem, displayRectangleWOSlider) - FS.Height) / 2.0), HeadSize() - 4 - _AutoFilterSize);
                GR.TranslateTransform(pos.X, pos.Y);
                GR.RotateTransform(-90);
                GR.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                GR.DrawString(tx, _Column_Font.Font(), new SolidBrush(ViewItem.Column.ForeColor), 0, 0);
                GR.TranslateTransform(-pos.X, -pos.Y);
                GR.ResetTransform();
            }
            // Sortierrichtung Zeichnen
            var tmpSortDefinition = SortUsed();
            if (tmpSortDefinition != null && tmpSortDefinition.UsedForRowSort(ViewItem.Column)) {
                if (tmpSortDefinition.Reverse) {
                    GR.DrawImage(QuickImage.Get("ZA|11|5||||50").BMP, (float)(ViewItem.OrderTMP_Spalte_X1 + (Column_DrawWidth(ViewItem, displayRectangleWOSlider) / 2.0) - 6), HeadSize() - 6 - _AutoFilterSize);
                } else {
                    GR.DrawImage(QuickImage.Get("AZ|11|5||||50").BMP, (float)(ViewItem.OrderTMP_Spalte_X1 + (Column_DrawWidth(ViewItem, displayRectangleWOSlider) / 2.0) - 6), HeadSize() - 6 - _AutoFilterSize);
                }
            }
        }
        /// <summary>
        /// Zeichnet die gesamte Zelle mit Listbox-Hintergrund und prüft noch, ob der verlinkte Inhalt gezeichnet werden soll.
        /// </summary>
        /// <param name="GR"></param>
        /// <param name="Column"></param>
        /// <param name="Row"></param>
        /// <param name="displayRectangleWOSlider"></param>
        /// <param name="vDesign"></param>
        /// <param name="vState"></param>
        private void Draw_CellListBox(Graphics GR, ColumnViewItem Column, clsRowDrawData Row, Rectangle displayRectangleWOSlider, enDesign vDesign, enStates vState) {
            Skin.Draw_Back(GR, vDesign, vState, displayRectangleWOSlider, null, false);
            Skin.Draw_Border(GR, vDesign, vState, displayRectangleWOSlider);
            var f = Skin.GetBlueFont(vDesign, vState);
            if (f == null) { return; }
            Draw_CellTransparent(GR, Column, Row, displayRectangleWOSlider, f);
        }
        /// Zeichnet die eine Zeile der Zelle ohne Hintergrund und prüft noch, ob der verlinkte Inhalt gezeichnet werden soll.
        private void Draw_CellTransparentDirect_OneLine(Graphics GR, string DrawString, ColumnViewItem cellInThisDatabaseColumn, int rowY, int TxtY, ColumnItem ContentHolderColumnStyle, bool IsLastRow, BlueFont vfont, int drawRowHeight, int drawColumnWidth) {
            Rectangle r = new((int)cellInThisDatabaseColumn.OrderTMP_Spalte_X1,
                                   rowY + TxtY,
                                  drawColumnWidth,
                                  Pix16);
            if (r.Bottom > rowY + drawRowHeight - Pix16) {
                if (r.Bottom > rowY + drawRowHeight) { return; }
                if (!IsLastRow) { DrawString = "..."; }// Die Letzte Zeile noch ganz hinschreiben
            }
            Draw_FormatedText(GR, ContentHolderColumnStyle, DrawString, r, false, vfont, enShortenStyle.Replaced, enStates.Standard, ContentHolderColumnStyle.BildTextVerhalten);
        }
        #endregion


        internal static void StartDatabaseService() {
            if (ServiceStarted) { return; }
            ServiceStarted = true;
            BlueBasics.MultiUserFile.clsMultiUserFile.AllFiles.ItemAdded += AllFiles_ItemAdded;
            BlueBasics.MultiUserFile.clsMultiUserFile.AllFiles.ItemRemoving += AllFiles_ItemRemoving;
        }
        private static void AllFiles_ItemRemoving(object sender, ListEventArgs e) {
            if (e.Item is Database DB) {
                DB.NeedPassword -= Database_NeedPassword;
                DB.GenerateLayoutInternal -= DB_GenerateLayoutInternal;
                DB.Loaded -= tabAdministration.CheckDatabase;
            }
        }
        private static void AllFiles_ItemAdded(object sender, ListEventArgs e) {
            if (e.Item is Database DB) {
                DB.NeedPassword += Database_NeedPassword;
                DB.GenerateLayoutInternal += DB_GenerateLayoutInternal;
                DB.Loaded += tabAdministration.CheckDatabase;
            }
        }
        public static void Database_NeedPassword(object sender, PasswordEventArgs e) {
            if (e.Handled) { return; }
            e.Handled = true;
            e.Password = InputBox.Show("Bitte geben sie das Passwort ein,<br>um Zugriff auf diese Datenbank<br>zu erhalten:", string.Empty, enDataFormat.Text);
        }
        private static void DB_GenerateLayoutInternal(object sender, GenerateLayoutInternalEventargs e) {
            if (e.Handled) { return; }
            e.Handled = true;
            ItemCollectionPad Pad = new(e.LayoutID, e.Row.Database, e.Row.Key);
            Pad.SaveAsBitmap(e.Filename);
        }
        private void ColumnArrangements_ItemInternalChanged(object sender, ListEventArgs e) {
            OnColumnArrangementChanged();
            Invalidate();
        }
        public new void Focus() {
            if (Focused()) { return; }
            base.Focus();
        }
        public new bool Focused() => base.Focused || SliderY.Focused() || SliderX.Focused() || BTB.Focused() || BCB.Focused();
        public ColumnItem CursorPosColumn() => _CursorPosColumn;
        public RowItem CursorPosRow() => _CursorPosRow?.Row;
        public void Export_HTML(string Filename = "", bool Execute = true) {
            if (_Database == null) { return; }
            if (string.IsNullOrEmpty(Filename)) { Filename = TempFile("", "", "html"); }
            _Database.Export_HTML(Filename, CurrentArrangement, SortedRows(), Execute);
        }
        public string Export_CSV(enFirstRow FirstRow) => _Database == null ? string.Empty : _Database.Export_CSV(FirstRow, CurrentArrangement, SortedRows());
        public string Export_CSV(enFirstRow FirstRow, ColumnItem OnlyColumn) {
            if (_Database == null) { return string.Empty; }
            List<ColumnItem> l = new() { OnlyColumn };
            return _Database.Export_CSV(FirstRow, l, SortedRows());
        }
        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e) {
            base.OnMouseDown(e);
            if (_Database == null) { return; }
            lock (Lock_UserAction) {
                if (ISIN_MouseDown) { return; }
                ISIN_MouseDown = true;
                _Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
                CellOnCoordinate(e.X, e.Y, out _MouseOverColumn, out _MouseOverRow);
                // Die beiden Befehle nur in Mouse Down!
                // Wenn der Cursor bei Click/Up/Down geändert wird, wird ein Ereignis ausgelöst.
                // Das könnte auch sehr Zeitintensiv sein. Dann kann die Maus inzwischen wo ander sein.
                // Somit würde das Ereignis doppelt und dreifach ausgelöste werden können.
                // Beipiel: MouseDown-> Bildchen im Pad erzeugen, dauert.... Maus Bewegt sich
                //          MouseUp  -> Cursor wird umgesetzt, Ereginis CursorChanged wieder ausgelöst, noch ein Bildchen
                if (_MouseOverRow == null) {
                    var rc = RowCaptionOnCoordinate(e.X, e.Y);
                    if (!string.IsNullOrEmpty(rc)) {
                        if (_collapsed.Contains(rc)) {
                            _collapsed.Remove(rc);
                        } else {
                            _collapsed.Add(rc);
                        }
                        Invalidate_RowSort();
                        Invalidate();
                    }
                }
                EnsureVisible(_MouseOverColumn, _MouseOverRow);
                CursorPos_Set(_MouseOverColumn, _MouseOverRow, false);
                ISIN_MouseDown = false;
            }
        }
        private void DropDownMenu_ItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
            FloatingForm.Close(this);
            if (string.IsNullOrEmpty(e.ClickedComand)) { return; }
            var CellKey = string.Empty;
            if (e.HotItem is string s) { CellKey = s; }
            if (string.IsNullOrEmpty(CellKey)) { return; }
            Database.Cell.DataOfCellKey(CellKey, out var Column, out var Row);
            var rowd = SortedRowData().Get(Row);
            var ToAdd = e.ClickedComand;
            var ToRemove = string.Empty;
            if (ToAdd == "#Erweitert") {
                Cell_Edit(Column, rowd, false);
                return;
            }
            if (Row == null) {
                // Neue Zeile!
                UserEdited(this, ToAdd, Column, null, false);
                return;
            }
            if (Column.MultiLine) {
                var E = Row.CellGetList(Column);
                if (E.Contains(ToAdd, false)) {
                    // Ist das angeklickte Element schon vorhanden, dann soll es wohl abgewählt (gelöscht) werden.
                    if (E.Count > -1 || Column.DropdownAllesAbwählenErlaubt) {
                        ToRemove = ToAdd;
                        ToAdd = string.Empty;
                    }
                }
                if (!string.IsNullOrEmpty(ToRemove)) { E.RemoveString(ToRemove, false); }
                if (!string.IsNullOrEmpty(ToAdd)) { E.Add(ToAdd); }
                UserEdited(this, E.JoinWithCr(), Column, Row, false);
            } else {
                if (Column.DropdownAllesAbwählenErlaubt) {
                    if (ToAdd == Row.CellGetString(Column)) {
                        UserEdited(this, string.Empty, Column, Row, false);
                        return;
                    }
                }
                UserEdited(this, ToAdd, Column, Row, false);
            }
        }
        private void Cell_Edit(ColumnItem cellInThisDatabaseColumn, clsRowDrawData cellInThisDatabaseRow, bool WithDropDown) {
            Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
            ColumnItem ContentHolderCellColumn;
            RowItem ContentHolderCellRow;

            if (Database.ReloadNeeded) { Database.Load_Reload(); }

            var f = Database.ErrorReason(enErrorReason.EditGeneral);
            if (!string.IsNullOrEmpty(f)) { NotEditableInfo(f); return; }
            if (cellInThisDatabaseColumn == null) { return; }// Klick ins Leere

            var ViewItem = CurrentArrangement[cellInThisDatabaseColumn];
            if (ViewItem == null) {
                NotEditableInfo("Ansicht veraltet");
                return;
            }

            if (cellInThisDatabaseColumn.Format == enDataFormat.LinkedCell) {
                (ContentHolderCellColumn, ContentHolderCellRow) = CellCollection.LinkedCellData(cellInThisDatabaseColumn, cellInThisDatabaseRow.Row, true, true);
                if (ContentHolderCellColumn == null) {
                    NotEditableInfo("In verknüpfter Datenbank nicht vorhanden");
                    return;
                }
            } else {
                ContentHolderCellColumn = cellInThisDatabaseColumn;
                ContentHolderCellRow = cellInThisDatabaseRow?.Row;
            }


            if (!ContentHolderCellColumn.DropdownBearbeitungErlaubt) { WithDropDown = false; }
            var dia = ColumnItem.UserEditDialogTypeInTable(ContentHolderCellColumn, WithDropDown);
            if (dia == enEditTypeTable.None) {
                NotEditableInfo("Diese Spalte kann generell nicht bearbeitet werden.");
                return;
            }
            if (!CellCollection.UserEditPossible(ContentHolderCellColumn, ContentHolderCellRow, enErrorReason.EditGeneral)) {
                NotEditableInfo(CellCollection.ErrorReason(ContentHolderCellColumn, ContentHolderCellRow, enErrorReason.EditGeneral));
                return;
            }
            if (cellInThisDatabaseRow != null) {
                if (!EnsureVisible(cellInThisDatabaseColumn, cellInThisDatabaseRow)) {
                    NotEditableInfo("Zelle konnte nicht angezeigt werden.");
                    return;
                }
                if (!IsOnScreen(cellInThisDatabaseColumn, cellInThisDatabaseRow, DisplayRectangle)) {
                    NotEditableInfo("Die Zelle wird nicht angezeigt.");
                    return;
                }
                CursorPos_Set(cellInThisDatabaseColumn, cellInThisDatabaseRow, false);
            } else {
                if (!UserEdit_NewRowAllowed()) {
                    NotEditableInfo("Keine neuen Zeilen erlaubt.");
                    return;
                }
                if (!EnsureVisible(ViewItem)) {
                    NotEditableInfo("Zelle konnte nicht angezeigt werden.");
                    return;
                }
                if (!IsOnScreen(ViewItem, DisplayRectangle)) {
                    NotEditableInfo("Die Zelle wird nicht angezeigt.");
                    return;
                }
                SliderY.Value = 0;
            }
            var Cancel = "";
            if (cellInThisDatabaseRow != null) {
                CellCancelEventArgs ed = new(cellInThisDatabaseColumn, cellInThisDatabaseRow.Row, Cancel);
                OnEditBeforeBeginEdit(ed);
                Cancel = ed.CancelReason;
            } else {
                RowCancelEventArgs ed = new(null, Cancel);
                OnEditBeforeNewRow(ed);
                Cancel = ed.CancelReason;
            }
            if (!string.IsNullOrEmpty(Cancel)) {
                NotEditableInfo(Cancel);
                return;
            }
            switch (dia) {

                case enEditTypeTable.Textfeld:
                    Cell_Edit_TextBox(cellInThisDatabaseColumn, cellInThisDatabaseRow, ContentHolderCellColumn, ContentHolderCellRow, BTB, 0, 0);
                    break;

                case enEditTypeTable.Textfeld_mit_Auswahlknopf:
                    Cell_Edit_TextBox(cellInThisDatabaseColumn, cellInThisDatabaseRow, ContentHolderCellColumn, ContentHolderCellRow, BCB, 20, 18);
                    break;

                case enEditTypeTable.Dropdown_Single:
                    Cell_Edit_Dropdown(cellInThisDatabaseColumn, cellInThisDatabaseRow, ContentHolderCellColumn, ContentHolderCellRow);
                    break;
                //break; 
                //case enEditType.Dropdown_Multi:
                //    Cell_Edit_Dropdown(cellInThisDatabaseColumn, cellInThisDatabaseRow, ContentHolderCellColumn, ContentHolderCellRow);
                //    break;
                //
                //case enEditTypeTable.RelationEditor_InTable:
                //    if (cellInThisDatabaseColumn != ContentHolderCellColumn || cellInThisDatabaseRow != ContentHolderCellRow)
                //    {
                //        NotEditableInfo("Ziel-Spalte ist kein Textformat");
                //        return;
                //    }
                //    Cell_Edit_Relations(cellInThisDatabaseColumn, cellInThisDatabaseRow);
                //    break;

                case enEditTypeTable.Farb_Auswahl_Dialog:
                    if (cellInThisDatabaseColumn != ContentHolderCellColumn || cellInThisDatabaseRow?.Row != ContentHolderCellRow) {
                        NotEditableInfo("Verlinkte Zellen hier verboten.");
                        return;
                    }
                    Cell_Edit_Color(cellInThisDatabaseColumn, cellInThisDatabaseRow);
                    break;

                case enEditTypeTable.Font_AuswahlDialog:
                    Develop.DebugPrint_NichtImplementiert();
                    //if (cellInThisDatabaseColumn != ContentHolderCellColumn || cellInThisDatabaseRow != ContentHolderCellRow)
                    //{
                    //    NotEditableInfo("Ziel-Spalte ist kein Textformat");
                    //    return;
                    //}
                    //Cell_Edit_Font(cellInThisDatabaseColumn, cellInThisDatabaseRow);
                    break;

                case enEditTypeTable.WarnungNurFormular:
                    NotEditableInfo("Dieser Zelltyp kann nur in einem Formular-Fenster bearbeitet werden");
                    break;


                case enEditTypeTable.FileHandling_InDateiSystem:
                    if (cellInThisDatabaseColumn != ContentHolderCellColumn || cellInThisDatabaseRow?.Row != ContentHolderCellRow) {
                        NotEditableInfo("Verlinkte Zellen hier verboten.");
                        return;
                    }
                    Cell_Edit_FileSystem(cellInThisDatabaseColumn, cellInThisDatabaseRow);
                    break;

                default:
                    Develop.DebugPrint(dia);
                    NotEditableInfo("Unbekannte Bearbeitungs-Methode");
                    break;
            }
        }


        public static List<string> FileSystem(ColumnItem _tmpColumn) {

            if (_tmpColumn == null) { return null; }

            var f = FileOperations.GetFilesWithFileSelector(_tmpColumn.Database.Filename.FilePath(), _tmpColumn.MultiLine);


            if (f == null) { return null; }


            List<string> DelList = new();
            List<string> NewFiles = new();

            foreach (var thisf in f) {

                var b = modConverter.FileToByte(thisf);

                if (!string.IsNullOrEmpty(_tmpColumn.Database.FileEncryptionKey)) { b = modAllgemein.SimpleCrypt(b, _tmpColumn.Database.FileEncryptionKey, 1); }

                var neu = thisf.FileNameWithSuffix();
                neu = _tmpColumn.BestFile(neu.FileNameWithSuffix(), true);
                modConverter.ByteToFile(neu, b);

                NewFiles.Add(neu);
                DelList.Add(thisf);
            }

            Forms.FileDialogs.DeleteFile(DelList, true);
            return NewFiles;
        }


        public void PinRemove(RowItem row) {
            _PinnedRows.Remove(row);
            Invalidate_Filterinfo();
            Invalidate_RowSort();
            Invalidate();
            OnPinnedChanged();
        }
        public void PinAdd(RowItem row) {
            _PinnedRows.Add(row);
            Invalidate_Filterinfo();
            Invalidate_RowSort();
            Invalidate();
            OnPinnedChanged();
        }
        private void OnEditBeforeNewRow(RowCancelEventArgs e) => EditBeforeNewRow?.Invoke(this, e);
        private void OnEditBeforeBeginEdit(CellCancelEventArgs e) => EditBeforeBeginEdit?.Invoke(this, e);
        private void Cell_Edit_Color(ColumnItem cellInThisDatabaseColumn, clsRowDrawData cellInThisDatabaseRow) {
            ColDia.Color = cellInThisDatabaseRow.Row.CellGetColor(cellInThisDatabaseColumn);
            ColDia.Tag = CellCollection.KeyOfCell(cellInThisDatabaseColumn, cellInThisDatabaseRow.Row);
            List<int> ColList = new();
            foreach (var ThisRowItem in _Database.Row) {
                if (ThisRowItem != null) {
                    if (ThisRowItem.CellGetInteger(cellInThisDatabaseColumn) != 0) {
                        ColList.Add(ThisRowItem.CellGetColorBGR(cellInThisDatabaseColumn));
                    }
                }
            }
            ColList.Sort();
            ColDia.CustomColors = ColList.Distinct().ToArray();
            ColDia.ShowDialog();
            UserEdited(this, Color.FromArgb(255, ColDia.Color).ToArgb().ToString(), cellInThisDatabaseColumn, cellInThisDatabaseRow?.Row, false);
        }

        private void Cell_Edit_FileSystem(ColumnItem cellInThisDatabaseColumn, clsRowDrawData cellInThisDatabaseRow) {
            var l = Table.FileSystem(cellInThisDatabaseColumn);
            if (l == null) { return; }
            UserEdited(this, l.JoinWithCr(), cellInThisDatabaseColumn, cellInThisDatabaseRow?.Row, false);

        }


        private bool Cell_Edit_TextBox(ColumnItem cellInThisDatabaseColumn, clsRowDrawData cellInThisDatabaseRow, ColumnItem ContentHolderCellColumn, RowItem ContentHolderCellRow, TextBox Box, int AddWith, int IsHeight) {
            if (ContentHolderCellColumn != cellInThisDatabaseColumn) {
                if (ContentHolderCellRow == null) {
                    NotEditableInfo("Bei Zellverweisen kann keine neue Zeile erstellt werden.");
                    return false;
                }
                if (cellInThisDatabaseRow == null) {
                    NotEditableInfo("Bei Zellverweisen kann keine neue Zeile erstellt werden.");
                    return false;
                }
            }

            var ViewItemx = CurrentArrangement[cellInThisDatabaseColumn];
            if (ContentHolderCellRow != null) {
                var h = cellInThisDatabaseRow.DrawHeight;// Row_DrawHeight(cellInThisDatabaseRow, DisplayRectangle);
                if (IsHeight > 0) { h = IsHeight; }
                Box.Location = new Point((int)ViewItemx.OrderTMP_Spalte_X1, DrawY(cellInThisDatabaseRow));
                Box.Size = new Size(Column_DrawWidth(ViewItemx, DisplayRectangle) + AddWith, h);
                Box.Text = ContentHolderCellRow.CellGetString(ContentHolderCellColumn).Replace(Constants.beChrW1.ToString(), "\r"); // Texte aus alter Zeit...
            } else {
                // Neue Zeile...
                Box.Location = new Point((int)ViewItemx.OrderTMP_Spalte_X1, HeadSize());
                Box.Size = new Size(Column_DrawWidth(ViewItemx, DisplayRectangle) + AddWith, Pix18);
                Box.Text = "";
            }

            Box.Format = ContentHolderCellColumn.Format;
            Box.AllowedChars = ContentHolderCellColumn.AllowedChars;
            Box.MultiLine = ContentHolderCellColumn.MultiLine;
            Box.Tag = CellCollection.KeyOfCell(cellInThisDatabaseColumn, cellInThisDatabaseRow?.Row); // ThisDatabase, der Wert wird beim einchecken in die Fremdzelle geschrieben
            if (Box is ComboBox box) {
                ItemCollectionList.GetItemCollection(box.Item, ContentHolderCellColumn, ContentHolderCellRow, enShortenStyle.Both, 1000);
                if (box.Item.Count == 0) {
                    return Cell_Edit_TextBox(cellInThisDatabaseColumn, cellInThisDatabaseRow, ContentHolderCellColumn, ContentHolderCellRow, BTB, 0, 0);
                }
            }

            if (string.IsNullOrEmpty(Box.Text)) {
                Box.Text = CellCollection.AutomaticInitalValue(ContentHolderCellColumn, ContentHolderCellRow);
            }

            Box.Visible = true;
            Box.BringToFront();
            Box.Focus();
            return true;
        }
        private void Cell_Edit_Dropdown(ColumnItem cellInThisDatabaseColumn, clsRowDrawData cellInThisDatabaseRow, ColumnItem ContentHolderCellColumn, RowItem ContentHolderCellRow) {
            if (cellInThisDatabaseColumn != ContentHolderCellColumn) {
                if (ContentHolderCellRow == null) {
                    NotEditableInfo("Bei Zellverweisen kann keine neue Zeile erstellt werden.");
                    return;
                }
                if (cellInThisDatabaseRow == null) {
                    NotEditableInfo("Bei Zellverweisen kann keine neue Zeile erstellt werden.");
                    return;
                }
            }
            ItemCollectionList t = new() {
                Appearance = enBlueListBoxAppearance.DropdownSelectbox
            };
            ItemCollectionList.GetItemCollection(t, ContentHolderCellColumn, ContentHolderCellRow, enShortenStyle.Both, 1000);
            if (t.Count == 0) {
                // Hm.. Dropdown kein Wert vorhanden.... also gar kein Dropdown öffnen!
                if (ContentHolderCellColumn.TextBearbeitungErlaubt) { Cell_Edit(cellInThisDatabaseColumn, cellInThisDatabaseRow, false); }
                return;
            }
            if (ContentHolderCellColumn.TextBearbeitungErlaubt) {
                if (t.Count == 1 && cellInThisDatabaseRow.Row.CellIsNullOrEmpty(cellInThisDatabaseColumn)) {
                    // Bei nur einem Wert, wenn Texteingabe erlaubt, Dropdown öffnen
                    Cell_Edit(cellInThisDatabaseColumn, cellInThisDatabaseRow, false);
                    return;
                }
                t.Add("Erweiterte Eingabe", "#Erweitert", QuickImage.Get(enImageCode.Stift), true, Constants.FirstSortChar + "1");
                t.AddSeparator(Constants.FirstSortChar + "2");
                t.Sort();
            }
            var _DropDownMenu = FloatingInputBoxListBoxStyle.Show(t, CellCollection.KeyOfCell(cellInThisDatabaseColumn, cellInThisDatabaseRow.Row), this, Translate);
            _DropDownMenu.ItemClicked += DropDownMenu_ItemClicked;
            Develop.Debugprint_BackgroundThread();
        }
        private static void UserEdited(Table table, string newValue, ColumnItem column, RowItem row, bool formatWarnung) {
            if (column == null) {
                table.NotEditableInfo("Keine Spalte angegeben.");
                return;
            }
            if (column.Format == enDataFormat.LinkedCell) {
                (var lcolumn, var lrow) = CellCollection.LinkedCellData(column, row, true, false);
                if (lcolumn == null || lrow == null) {
                    table.NotEditableInfo("Zelle in verlinkter Datenbank nicht vorhanden.");
                    return;
                }
                UserEdited(table, newValue, lcolumn, lrow, formatWarnung);
                if (table.Database == column.Database) { table.CursorPos_Set(column, row, false); }
                return;
            }
            if (row == null && column != column.Database.Column[0]) {
                table.NotEditableInfo("Neue Zeilen müssen mit der ersten Spalte beginnen");
                return;
            }
            newValue = column.AutoCorrect(newValue);
            if (row != null) {
                if (newValue == row.CellGetString(column)) { return; }
            } else {
                if (string.IsNullOrEmpty(newValue)) { return; }
            }
            BeforeNewValueEventArgs ed = new(column, row, newValue, string.Empty);
            table.OnEditBeforeNewValueSet(ed);
            var CancelReason = ed.CancelReason;
            if (string.IsNullOrEmpty(CancelReason) && formatWarnung && !string.IsNullOrEmpty(newValue)) {
                if (!newValue.IsFormat(column.Format, column.MultiLine)) {
                    if (MessageBox.Show("Ihre Eingabe entspricht<br><u>nicht</u> dem erwarteten Format!<br><br>Trotzdem übernehmen?", enImageCode.Information, "Ja", "Nein") != 0) {
                        CancelReason = "Abbruch, das das erwartete Format nicht eingehalten wurde.";
                    }
                }
            }
            if (string.IsNullOrEmpty(CancelReason)) {
                //var f = column.Database.ErrorReason(enErrorReason.EditGeneral);
                //if (!string.IsNullOrEmpty(f)) { table.NotEditableInfo(f); return; }
                //d
                if (row == null) {
                    var f = CellCollection.ErrorReason(column.Database.Column[0], null, enErrorReason.EditGeneral);
                    if (!string.IsNullOrEmpty(f)) { table.NotEditableInfo(f); return; }
                    row = column.Database.Row.Add(newValue);
                    if (table.Database == column.Database) { table.PinAdd(row); }
                } else {
                    var f = CellCollection.ErrorReason(column, row, enErrorReason.EditGeneral);
                    if (!string.IsNullOrEmpty(f)) { table.NotEditableInfo(f); return; }
                    row.CellSet(column, newValue);
                }
                if (table.Database == column.Database) { table.CursorPos_Set(column, row, false); }
                row.DoAutomatic(true, false, 5, "value changed");
                // EnsureVisible ganz schlecht: Daten verändert, keine Positionen bekannt - und da soll sichtbar gemacht werden?
                // CursorPos.EnsureVisible(SliderX, SliderY, DisplayRectangle)
            } else {
                table.NotEditableInfo(CancelReason);
            }
        }
        private void OnEditBeforeNewValueSet(BeforeNewValueEventArgs ed) => EditBeforeNewValueSet?.Invoke(this, ed);
        private void TXTBox_Close(TextBox BTBxx) {
            if (BTBxx == null) { return; }
            if (!BTBxx.Visible) { return; }
            if (BTBxx.Tag == null || string.IsNullOrEmpty(BTBxx.Tag.ToString())) {
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
        private void Cursor_Move(enDirection Richtung) {
            if (_Database == null) { return; }
            Neighbour(_CursorPosColumn, _CursorPosRow.Row, Richtung, out var _newCol, out var _newRow);
            CursorPos_Set(_newCol, _newRow, Richtung != enDirection.Nichts);
        }
        private bool ComputeAllColumnPositions() {
            //Develop.DebugPrint_InvokeRequired(InvokeRequired, true);
            if (Database.IsParsing) { return false; }
            try {
                // Kommt vor, dass spontan doch geparsed wird...
                if (_Database.ColumnArrangements == null || _ArrangementNr >= _Database.ColumnArrangements.Count) { return false; }
                foreach (var ThisViewItem in CurrentArrangement) {
                    if (ThisViewItem != null) {
                        ThisViewItem.OrderTMP_Spalte_X1 = null;
                    }
                }
                //foreach (var ThisRowItem in _Database.Row) {
                //    if (ThisRowItem != null) { ThisRowItem.TMP_Y = null; }
                //}
                _WiederHolungsSpaltenWidth = 0;
                _MouseOverText = string.Empty;
                var wdh = true;
                var MaxX = 0;
                var DisplayR = DisplayRectangleWithoutSlider();
                // Spalten berechnen
                foreach (var ThisViewItem in CurrentArrangement) {
                    if (ThisViewItem?.Column != null) {
                        if (ThisViewItem.ViewType != enViewType.PermanentColumn) { wdh = false; }
                        if (wdh) {
                            ThisViewItem.OrderTMP_Spalte_X1 = MaxX;
                            MaxX += Column_DrawWidth(ThisViewItem, DisplayR);
                            _WiederHolungsSpaltenWidth = Math.Max(_WiederHolungsSpaltenWidth, MaxX);
                        } else {
                            ThisViewItem.OrderTMP_Spalte_X1 = SliderX.Visible ? (int)(MaxX - SliderX.Value) : 0;
                            MaxX += Column_DrawWidth(ThisViewItem, DisplayR);
                        }
                    }
                }
                SliderSchaltenWaage(DisplayR, MaxX);
            } catch (Exception ex) {
                Develop.DebugPrint(ex);
                return false;
            }
            return true;
        }
        internal bool PermanentPossible(ColumnViewItem ThisViewItem) {
            if (_ArrangementNr < 1) {
                return ThisViewItem.Column.IsFirst();
            }
            var prev = ThisViewItem.PreviewsVisible(CurrentArrangement);
            return prev == null || Convert.ToBoolean(prev.ViewType == enViewType.PermanentColumn);
        }
        internal bool NonPermanentPossible(ColumnViewItem ThisViewItem) {
            if (_ArrangementNr < 1) {
                return !ThisViewItem.Column.IsFirst();
            }
            var NX = ThisViewItem.NextVisible(CurrentArrangement);
            return NX == null || Convert.ToBoolean(NX.ViewType != enViewType.PermanentColumn);
        }
        protected override bool IsInputKey(System.Windows.Forms.Keys keyData) =>
            // Ganz wichtig diese Routine!
            // Wenn diese NICHT ist, geht der Fokus weg, sobald der cursor gedrückt wird.
            keyData switch {
                System.Windows.Forms.Keys.Up or System.Windows.Forms.Keys.Down or System.Windows.Forms.Keys.Left or System.Windows.Forms.Keys.Right => true,
                _ => false,
            };

        #region  AutoFilter 
        private void AutoFilter_Show(ColumnViewItem columnviewitem, int screenx, int screeny) {
            if (columnviewitem == null) { return; }
            if (_Design == enBlueTableAppearance.OnlyMainColumnWithoutHead) { return; }
            if (!columnviewitem.Column.AutoFilterSymbolPossible()) { return; }
            foreach (var thisFilter in Filter) {
                if (thisFilter != null) {
                    if (thisFilter.Column == columnviewitem.Column && !string.IsNullOrEmpty(thisFilter.Herkunft)) {
                        MessageBox.Show("Dieser Filter wurde<br>automatisch gesetzt.", enImageCode.Information, "OK");
                        return;
                    }
                }
            }
            Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
            //OnBeforeAutoFilterShow(new ColumnEventArgs(columnviewitem.Column));
            _AutoFilter = new AutoFilter(columnviewitem.Column, Filter, PinnedRows);
            _AutoFilter.Position_LocateToPosition(new Point(screenx + (int)columnviewitem.OrderTMP_Spalte_X1, screeny + HeadSize()));
            _AutoFilter.Show();
            _AutoFilter.FilterComand += AutoFilter_FilterComand;
            Develop.Debugprint_BackgroundThread();
        }
        private void AutoFilter_Close() {
            if (_AutoFilter != null) {
                _AutoFilter.FilterComand -= AutoFilter_FilterComand;
                _AutoFilter.Dispose();
                _AutoFilter = null;
            }
        }
        private void AutoFilter_FilterComand(object sender, FilterComandEventArgs e) {
            switch (e.Comand.ToLower()) {

                case "":
                    break;

                case "filter":
                    Filter.Remove(e.Column);
                    Filter.Add(e.Filter);
                    break;

                case "filterdelete":
                    Filter.Remove(e.Column);
                    break;

                case "doeinzigartig":
                    Filter.Remove(e.Column);
                    e.Column.GetUniques(SortedRows(), out var Einzigartig, out _);
                    if (Einzigartig.Count > 0) {
                        Filter.Add(new FilterItem(e.Column, enFilterType.Istgleich_ODER_GroßKleinEgal, Einzigartig));
                        Notification.Show("Die aktuell einzigartigen Einträge wurden berechnet<br>und als <b>ODER-Filter</b> gespeichert.", enImageCode.Trichter);
                    } else {
                        Notification.Show("Filterung dieser Spalte gelöscht,<br>da <b>alle Einträge</b> mehrfach vorhanden sind.", enImageCode.Trichter);
                    }
                    break;

                case "donichteinzigartig":
                    Filter.Remove(e.Column);
                    e.Column.GetUniques(SortedRows(), out _, out var xNichtEinzigartig);
                    if (xNichtEinzigartig.Count > 0) {
                        Filter.Add(new FilterItem(e.Column, enFilterType.Istgleich_ODER_GroßKleinEgal, xNichtEinzigartig));
                        Notification.Show("Die aktuell <b>nicht</b> einzigartigen Einträge wurden berechnet<br>und als <b>ODER-Filter</b> gespeichert.", enImageCode.Trichter);
                    } else {
                        Notification.Show("Filterung dieser Spalte gelöscht,<br>da <b>alle Einträge</b> einzigartig sind.", enImageCode.Trichter);
                    }
                    break;

                case "dospaltenvergleich": {
                        List<RowItem> ro = new();
                        ro.AddRange(SortedRows());
                        ItemCollectionList ic = new();
                        foreach (var ThisColumnItem in e.Column.Database.Column) {
                            if (ThisColumnItem != null && ThisColumnItem != e.Column) { ic.Add(ThisColumnItem, false); }
                        }
                        ic.Sort();
                        var r = InputBoxListBoxStyle.Show("Mit welcher Spalte vergleichen?", ic, enAddType.None, true);
                        if (r == null || r.Count == 0) { return; }
                        //Filter.Remove(e.Column);
                        List<string> d = new();
                        foreach (var thisR in ro) {
                            if (thisR.CellGetString(e.Column) != thisR.CellGetString(r[0])) { d.Add(thisR.CellFirstString()); }
                        }
                        if (d.Count > 0) {
                            Filter.Add(new FilterItem(e.Column.Database.Column[0], enFilterType.Istgleich_ODER_GroßKleinEgal, d));
                            Notification.Show("Die aktuell <b>unterschiedlichen</b> Einträge wurden berechnet<br>und als <b>ODER-Filter</b> in der <b>ersten Spalte</b> gespeichert.", enImageCode.Trichter);
                        } else {
                            Notification.Show("Keine Filter verändert,<br>da <b>alle Einträge</b> indentisch sind.", enImageCode.Trichter);
                        }
                        break;
                    }

                case "doclipboard": {
                        var ClipTMP = Convert.ToString(System.Windows.Forms.Clipboard.GetDataObject().GetData(System.Windows.Forms.DataFormats.Text));
                        ClipTMP = ClipTMP.RemoveChars(Constants.Char_NotFromClip);
                        ClipTMP = ClipTMP.TrimEnd("\r\n");
                        var SearchValue = new List<string>(ClipTMP.SplitByCR()).SortedDistinctList();
                        Filter.Remove(e.Column);
                        if (SearchValue.Count > 0) {
                            Filter.Add(new FilterItem(e.Column, enFilterType.IstGleich_ODER | enFilterType.GroßKleinEgal, SearchValue));
                        }
                        break;
                    }

                case "donotclipboard": {
                        var ClipTMP = Convert.ToString(System.Windows.Forms.Clipboard.GetDataObject().GetData(System.Windows.Forms.DataFormats.Text));
                        ClipTMP = ClipTMP.RemoveChars(Constants.Char_NotFromClip);
                        ClipTMP = ClipTMP.TrimEnd("\r\n");
                        var ColumInhalt = Database.Export_CSV(enFirstRow.Without, e.Column, null).SplitByCRToList().SortedDistinctList();
                        ColumInhalt.RemoveString(ClipTMP.SplitByCRToList().SortedDistinctList(), false);
                        Filter.Remove(e.Column);
                        if (ColumInhalt.Count > 0) {
                            Filter.Add(new FilterItem(e.Column, enFilterType.IstGleich_ODER | enFilterType.GroßKleinEgal, ColumInhalt));
                        }
                        break;
                    }
                default:
                    Develop.DebugPrint("Unbekannter Comand:   " + e.Comand);
                    break;
            }
            OnAutoFilterClicked(new FilterEventArgs(e.Filter));
        }
        private void OnAutoFilterClicked(FilterEventArgs e) => AutoFilterClicked?.Invoke(this, e);
        private void OnRowAdded(object sender, RowEventArgs e) => RowAdded?.Invoke(sender, e);
        //private void OnBeforeAutoFilterShow(ColumnEventArgs e)
        //{
        //    BeforeAutoFilterShow?.Invoke(this, e);
        //}
        #endregion



        #region  ContextMenu 
        public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e) => false;
        public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) => ContextMenuItemClicked?.Invoke(this, e);
        #endregion



        #region  Ereignisse der Slider 
        private void SliderY_ValueChanged(object sender, System.EventArgs e) {
            if (_Database == null) { return; }
            lock (Lock_UserAction) {
                Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
                Invalidate();
            }
        }
        private void SliderX_ValueChanged(object sender, System.EventArgs e) {
            if (_Database == null) { return; }
            lock (Lock_UserAction) {
                Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
                Invalidate();
            }
        }
        #endregion



        #region  Ereignisse der Datenbank 
        private void _Database_CellValueChanged(object sender, CellEventArgs e) {
            if (AreRowsSorted()) {
                if (Filter[e.Column] != null || SortUsed() == null || SortUsed().UsedForRowSort(e.Column) || Filter.MayHasRowFilter(e.Column)) {
                    Invalidate_RowSort();
                }
            }
            Invalidate_DrawWidth(e.Column);
            Invalidate_SortedRowData();
            Invalidate();
            OnCellValueChanged(e);
        }
        private void OnCellValueChanged(CellEventArgs e) => CellValueChanged?.Invoke(this, e);
        private void Invalidate_Filterinfo() {
            if (_Database == null) { return; }
            foreach (var thisColumn in _Database.Column) {
                if (thisColumn != null) {
                    thisColumn.TMP_IfFilterRemoved = null;
                    thisColumn.TMP_AutoFilterSinnvoll = null;
                }
            }
        }
        private void Invalidate_RowSort() {
            _SortedRows = null;
            Invalidate_SortedRowData();
        }
        private void Invalidate_SortedRowData() => _SortedRowData = null;
        public bool AreRowsSorted() => _SortedRows != null;
        private void _Database_StopAllWorking(object sender, MultiUserFileStopWorkingEventArgs e) => CloseAllComponents();
        private void _Database_DatabaseLoaded(object sender, LoadedEventArgs e) {
            // Wird auch bei einem Reload ausgeführt.
            // Es kann aber sein, dass eine Ansicht zurückgeholt wurde, und die Werte stimmen. 
            // Deswegen prüfen, ob wirklich alles geleöscht werden muss, oder weiter behalten werden kann.
            // Auf Nothing  muss auch geprüft werden, da bei einem Dispose oder beim Beenden sich die Datenbank auch änsdert....
            Invalidate_HeadSize();
            var f = string.Empty;
            _MouseOverText = string.Empty;
            if (Filter != null) {
                if (e.OnlyReload) { f = Filter.ToString(); }
                Filter.Changed -= Filter_Changed;
                Filter = null;
            }
            if (_Database != null) {
                Filter = new FilterCollection(_Database, f);
                Filter.Changed += Filter_Changed;
                if (e.OnlyReload) {
                    if (_ArrangementNr != 1) {
                        if (_Database.ColumnArrangements == null || _ArrangementNr >= _Database.ColumnArrangements.Count || CurrentArrangement == null || !_Database.PermissionCheck(CurrentArrangement.PermissionGroups_Show, null)) {
                            _ArrangementNr = 1;
                        }
                    }
                    if (_MouseOverColumn != null && _MouseOverColumn.Database != _Database) {
                        _MouseOverColumn = null;
                        _MouseOverRow = null;
                    }
                    if (_CursorPosColumn != null && _CursorPosColumn.Database != _Database) {
                        _CursorPosColumn = null;
                        _CursorPosRow = null;
                    }
                } else {
                    _MouseOverColumn = null;
                    _MouseOverRow = null;
                    _CursorPosColumn = null;
                    _CursorPosRow = null;
                    _ArrangementNr = 1;
                }
            } else {
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
            if (e.OnlyReload) {
                if (string.IsNullOrEmpty(_StoredView)) { Develop.DebugPrint("Stored View Empty!"); }
                ParseView(_StoredView);
                _StoredView = string.Empty;
            }
        }
        private void CloseAllComponents() {
            if (InvokeRequired) {
                Invoke(new Action(() => CloseAllComponents()));
                return;
            }
            if (_Database == null) { return; }
            TXTBox_Close(BTB);
            TXTBox_Close(BCB);
            FloatingForm.Close(this);
            AutoFilter_Close();
            Forms.QuickInfo.Close();
        }
        #endregion



        #region  Ereignisse der Form 
        protected override void OnMouseWheel(System.Windows.Forms.MouseEventArgs e) {
            base.OnMouseWheel(e);
            if (_Database == null) { return; }
            lock (Lock_UserAction) {
                if (ISIN_MouseWheel) { return; }
                ISIN_MouseWheel = true;
                Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
                if (!SliderY.Visible) {
                    ISIN_MouseWheel = false;
                    return;
                }
                SliderY.DoMouseWheel(e);
                ISIN_MouseWheel = false;
            }
        }
        protected override void OnClick(System.EventArgs e) {
            base.OnClick(e);
            if (_Database == null) { return; }
            lock (Lock_UserAction) {
                if (ISIN_Click) { return; }
                ISIN_Click = true;
                Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
                CellOnCoordinate(MousePos().X, MousePos().Y, out _MouseOverColumn, out _MouseOverRow);
                ISIN_Click = false;
            }
        }
        protected override void OnSizeChanged(System.EventArgs e) {
            base.OnSizeChanged(e);
            if (_Database == null) { return; }
            lock (Lock_UserAction) {
                if (ISIN_SizeChanged) { return; }
                ISIN_SizeChanged = true;
                Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
                Invalidate_AllDraw(false);
                ISIN_SizeChanged = false;
            }
        }
        protected override void OnKeyDown(System.Windows.Forms.KeyEventArgs e) {
            base.OnKeyDown(e);
            if (_Database == null) { return; }
            lock (Lock_UserAction) {
                if (ISIN_KeyDown) { return; }
                ISIN_KeyDown = true;
                Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
                switch (e.KeyCode) {

                    case System.Windows.Forms.Keys.Oemcomma: // normales ,
                        if (e.Modifiers == System.Windows.Forms.Keys.Control) {
                            var lp = CellCollection.ErrorReason(_CursorPosColumn, _CursorPosRow.Row, enErrorReason.EditGeneral);
                            Neighbour(_CursorPosColumn, _CursorPosRow.Row, enDirection.Oben, out var _newCol, out var _newRow);
                            if (_newRow == _CursorPosRow.Row) { lp = "Das geht nicht bei dieser Zeile."; }
                            if (string.IsNullOrEmpty(lp) && _newRow != null) {
                                UserEdited(this, _newRow.CellGetString(_CursorPosColumn), _CursorPosColumn, _CursorPosRow.Row, true);
                            } else {
                                NotEditableInfo(lp);
                            }
                        }
                        break;

                    case System.Windows.Forms.Keys.X:
                        if (e.Modifiers == System.Windows.Forms.Keys.Control) {
                            CopyToClipboard(_CursorPosColumn, _CursorPosRow.Row, true);
                            if (_CursorPosRow.Row.CellIsNullOrEmpty(_CursorPosColumn)) {
                                ISIN_KeyDown = false;
                                return;
                            }
                            var l2 = CellCollection.ErrorReason(_CursorPosColumn, _CursorPosRow.Row, enErrorReason.EditGeneral);
                            if (string.IsNullOrEmpty(l2)) {
                                UserEdited(this, string.Empty, _CursorPosColumn, _CursorPosRow.Row, true);
                            } else {
                                NotEditableInfo(l2);
                            }
                        }
                        break;

                    case System.Windows.Forms.Keys.Delete:
                        if (_CursorPosRow.Row.CellIsNullOrEmpty(_CursorPosColumn)) {
                            ISIN_KeyDown = false;
                            return;
                        }
                        var l = CellCollection.ErrorReason(_CursorPosColumn, _CursorPosRow.Row, enErrorReason.EditGeneral);
                        if (string.IsNullOrEmpty(l)) {
                            UserEdited(this, string.Empty, _CursorPosColumn, _CursorPosRow.Row, true);
                        } else {
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
                        if (SliderY.Enabled) {
                            CursorPos_Reset();
                            SliderY.Value += SliderY.LargeChange;
                        }
                        break;

                    case System.Windows.Forms.Keys.PageUp: //Bildab
                        if (SliderY.Enabled) {
                            CursorPos_Reset();
                            SliderY.Value -= SliderY.LargeChange;
                        }
                        break;

                    case System.Windows.Forms.Keys.Home:
                        if (SliderY.Enabled) {
                            CursorPos_Reset();
                            SliderY.Value = SliderY.Minimum;
                        }
                        break;

                    case System.Windows.Forms.Keys.End:
                        if (SliderY.Enabled) {
                            CursorPos_Reset();
                            SliderY.Value = SliderY.Maximum;
                        }
                        break;

                    case System.Windows.Forms.Keys.C:
                        if (e.Modifiers == System.Windows.Forms.Keys.Control) {
                            CopyToClipboard(_CursorPosColumn, _CursorPosRow.Row, true);
                        }
                        break;

                    case System.Windows.Forms.Keys.F:
                        if (e.Modifiers == System.Windows.Forms.Keys.Control) {
                            Search x = new(this);
                            x.Show();
                        }
                        break;

                    case System.Windows.Forms.Keys.F2:
                        Cell_Edit(_CursorPosColumn, _CursorPosRow, true);
                        break;

                    case System.Windows.Forms.Keys.V:
                        if (e.Modifiers == System.Windows.Forms.Keys.Control) {
                            if (_CursorPosColumn != null && _CursorPosRow != null) {
                                if (!_CursorPosColumn.Format.TextboxEditPossible() && _CursorPosColumn.Format != enDataFormat.Columns_für_LinkedCellDropdown && _CursorPosColumn.Format != enDataFormat.Values_für_LinkedCellDropdown) {
                                    NotEditableInfo("Die Zelle hat kein passendes Format.");
                                    ISIN_KeyDown = false;
                                    return;
                                }
                                if (!System.Windows.Forms.Clipboard.GetDataObject().GetDataPresent(System.Windows.Forms.DataFormats.Text)) {
                                    NotEditableInfo("Kein Text in der Zwischenablage.");
                                    ISIN_KeyDown = false;
                                    return;
                                }
                                var ntxt = Convert.ToString(System.Windows.Forms.Clipboard.GetDataObject().GetData(System.Windows.Forms.DataFormats.UnicodeText));
                                if (_CursorPosRow.Row.CellGetString(_CursorPosColumn) == ntxt) {
                                    ISIN_KeyDown = false;
                                    return;
                                }
                                var l2 = CellCollection.ErrorReason(_CursorPosColumn, _CursorPosRow.Row, enErrorReason.EditGeneral);
                                if (string.IsNullOrEmpty(l2)) {
                                    UserEdited(this, ntxt, _CursorPosColumn, _CursorPosRow.Row, true);
                                } else {
                                    NotEditableInfo(l2);
                                }
                            }
                        }
                        break;
                }
                ISIN_KeyDown = false;
            }
        }
        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e) {
            base.OnMouseMove(e);
            lock (Lock_UserAction) {
                if (ISIN_MouseMove) { return; }
                ISIN_MouseMove = true;
                _MouseOverText = string.Empty;
                CellOnCoordinate(e.X, e.Y, out _MouseOverColumn, out _MouseOverRow);
                if (e.Button != System.Windows.Forms.MouseButtons.None) {
                    _Database?.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
                } else {
                    if (_MouseOverColumn != null && e.Y < HeadSize()) {
                        _MouseOverText = _MouseOverColumn.QuickInfoText(string.Empty);
                    } else if (_MouseOverColumn != null && _MouseOverRow != null) {
                        if (_MouseOverColumn.Format.NeedTargetDatabase()) {
                            if (_MouseOverColumn.LinkedDatabase() != null) {
                                switch (_MouseOverColumn.Format) {

                                    case enDataFormat.Columns_für_LinkedCellDropdown:
                                        var Txt = _MouseOverRow.Row.CellGetString(_MouseOverColumn);
                                        if (int.TryParse(Txt, out var ColKey)) {
                                            var C = _MouseOverColumn.LinkedDatabase().Column.SearchByKey(ColKey);
                                            if (C != null) { _MouseOverText = C.QuickInfoText(_MouseOverColumn.Caption + ": " + C.Caption); }
                                        }
                                        break;

                                    case enDataFormat.LinkedCell:

                                    case enDataFormat.Values_für_LinkedCellDropdown:
                                        (var lcolumn, var lrow) = CellCollection.LinkedCellData(_MouseOverColumn, _MouseOverRow.Row, false, false);
                                        if (lcolumn != null) { _MouseOverText = lcolumn.QuickInfoText(_MouseOverColumn.ReadableText() + " bei " + lcolumn.ReadableText() + ":"); }
                                        break;
                                    default:
                                        Develop.DebugPrint(_MouseOverColumn.Format);
                                        break;
                                }
                            } else {
                                _MouseOverText = "Verknüpfung zur Ziel-Datenbank fehlerhaft.";
                            }
                        } else if (_Database.IsAdministrator()) {
                            _MouseOverText = Database.UndoText(_MouseOverColumn, _MouseOverRow.Row);
                        }
                    }
                    _MouseOverText = _MouseOverText.Trim();
                    _MouseOverText = _MouseOverText.Trim("<br>");
                    _MouseOverText = _MouseOverText.Trim();
                }
                ISIN_MouseMove = false;
            }
        }
        private Rectangle DisplayRectangleWithoutSlider() => _Design == enBlueTableAppearance.Standard
? new Rectangle(DisplayRectangle.Left, DisplayRectangle.Left, DisplayRectangle.Width - SliderY.Width, DisplayRectangle.Height - SliderX.Height)
: new Rectangle(DisplayRectangle.Left, DisplayRectangle.Left, DisplayRectangle.Width - SliderY.Width, DisplayRectangle.Height);// Return ExpandRectangle(DisplayRectangle, 0, 0, -SliderY.Width, 0)
        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e) {
            base.OnMouseUp(e);
            if (_Database == null) { return; }
            lock (Lock_UserAction) {
                if (ISIN_MouseUp) { return; }
                ISIN_MouseUp = true;
                var ScreenX = System.Windows.Forms.Cursor.Position.X - e.X;
                var ScreenY = System.Windows.Forms.Cursor.Position.Y - e.Y;
                if (_Database == null) {
                    Forms.QuickInfo.Close();
                    ISIN_MouseUp = false;
                    return;
                }
                CellOnCoordinate(e.X, e.Y, out _MouseOverColumn, out _MouseOverRow);
                if (_CursorPosColumn != _MouseOverColumn || _CursorPosRow != _MouseOverRow) { Forms.QuickInfo.Close(); }
                // TXTBox_Close() NICHT! Weil sonst nach dem öffnen sofort wieder gschlossen wird
                // AutoFilter_Close() NICHT! Weil sonst nach dem öffnen sofort wieder geschlossen wird
                FloatingForm.Close(this, enDesign.Form_KontextMenu);
                //EnsureVisible(_MouseOver) <-Nur MouseDown, siehe Da
                //CursorPos_Set(_MouseOver) <-Nur MouseDown, siehe Da
                ColumnViewItem ViewItem = null;
                if (_MouseOverColumn != null) {
                    ViewItem = CurrentArrangement[_MouseOverColumn];
                }
                if (e.Button == System.Windows.Forms.MouseButtons.Left) {
                    if (_MouseOverColumn != null) {
                        if (Mouse_IsInAutofilter(ViewItem, e)) {
                            AutoFilter_Show(ViewItem, ScreenX, ScreenY);
                            ISIN_MouseUp = false;
                            return;
                        }
                        if (Mouse_IsInRedcueButton(ViewItem, e)) {
                            ViewItem._TMP_Reduced = !ViewItem._TMP_Reduced;
                            ViewItem._TMP_DrawWidth = null;
                            Invalidate();
                            ISIN_MouseUp = false;
                            return;
                        }
                        if (_MouseOverRow != null && _MouseOverColumn.Format == enDataFormat.Button) {
                            OnButtonCellClicked(new CellEventArgs(_MouseOverColumn, _MouseOverRow.Row));
                            Invalidate();
                        }
                    }
                }
                if (e.Button == System.Windows.Forms.MouseButtons.Right) {
                    FloatingInputBoxListBoxStyle.ContextMenuShow(this, e);
                }
                ISIN_MouseUp = false;
            }
            //   End SyncLock
        }
        protected override void OnDoubleClick(System.EventArgs e) {
            //    base.OnDoubleClick(e); Wird komplett selsbt gehandlet und das neue Ereigniss ausgelöst
            if (_Database == null) { return; }
            lock (Lock_UserAction) {
                if (ISIN_DoubleClick) { return; }
                ISIN_DoubleClick = true;
                CellOnCoordinate(MousePos().X, MousePos().Y, out _MouseOverColumn, out _MouseOverRow);
                CellDoubleClickEventArgs ea = new(_MouseOverColumn, _MouseOverRow?.Row, true);
                if (Mouse_IsInHead()) {
                    Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
                    DoubleClick?.Invoke(this, ea);
                } else {
                    if (_MouseOverRow == null && MousePos().Y > HeadSize() + Pix18) {
                        Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
                        DoubleClick?.Invoke(this, ea);
                    } else {
                        DoubleClick?.Invoke(this, ea);
                        if (ea.StartEdit) { Cell_Edit(_MouseOverColumn, _MouseOverRow, true); }
                    }
                }
                ISIN_DoubleClick = false;
            }
        }
        #endregion



        #region  Ereignisse der Textbox 
        private void BB_Enter(object sender, System.EventArgs e) {
            if (((TextBox)sender).MultiLine) { return; }
            CloseAllComponents();
        }
        private void BB_ESC(object sender, System.EventArgs e) {
            BTB.Tag = null;
            BTB.Visible = false;
            BCB.Tag = null;
            BCB.Visible = false;
            CloseAllComponents();
        }
        private void BTB_NeedDatabaseOfAdditinalSpecialChars(object sender, MultiUserFileGiveBackEventArgs e) => e.File = Database;
        private void BB_TAB(object sender, System.EventArgs e) => CloseAllComponents();
        private void BB_LostFocus(object sender, System.EventArgs e) {
            if (FloatingForm.IsShowing(BTB) || FloatingForm.IsShowing(BCB)) { return; }
            CloseAllComponents();
        }
        #endregion


        public void CursorPos_Reset() => CursorPos_Set(null, (clsRowDrawData)null, false);
        public void CursorPos_Set(ColumnItem column, RowItem row, bool ensureVisible) => CursorPos_Set(column, SortedRowData().Get(row), ensureVisible);
        private void CursorPos_Set(ColumnItem column, clsRowDrawData row, bool ensureVisible) {
            //if (column != null) { column = Database.Column.SearchByKey(column.Key); }
            //if (row != null) { row = Database.Row.SearchByKey(row.Key); }
            if (_Database.ColumnArrangements.Count == 0 || CurrentArrangement[column] == null || !SortedRowData().Contains(row)) {
                column = null;
                row = null;
            }
            if (_CursorPosColumn == column && _CursorPosRow == row) { return; }
            _MouseOverText = string.Empty;
            _CursorPosColumn = column;
            _CursorPosRow = row;
            if (ensureVisible) { EnsureVisible(_CursorPosColumn, _CursorPosRow); }
            Invalidate();
            OnCursorPosChanged(new CellEventArgs(_CursorPosColumn, _CursorPosRow?.Row));
        }
        private void OnCursorPosChanged(CellEventArgs e) => CursorPosChanged?.Invoke(this, e);
        protected override void OnMouseLeave(System.EventArgs e) {
            base.OnMouseLeave(e);
            if (_Database == null) { return; }
            lock (Lock_UserAction) {
                if (ISIN_MouseLeave) { return; }
                ISIN_MouseLeave = true;
                Forms.QuickInfo.Close();
                ISIN_MouseLeave = false;
            }
        }
        protected override void OnMouseEnter(System.EventArgs e) {
            base.OnMouseEnter(e);
            if (_Database == null) { return; }
            lock (Lock_UserAction) {
                if (ISIN_MouseEnter) { return; }
                ISIN_MouseEnter = true;
                Forms.QuickInfo.Close();
                ISIN_MouseEnter = false;
            }
        }
        protected override void OnResize(System.EventArgs e) {
            base.OnResize(e);
            if (_Database == null) { return; }
            lock (Lock_UserAction) {
                if (ISIN_Resize) { return; }
                ISIN_Resize = true;
                Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
                Invalidate_AllDraw(false);
                ISIN_Resize = false;
            }
        }
        public void WriteColumnArrangementsInto(ComboBox _ColumnArrangementSelector) {
            if (InvokeRequired) {
                Invoke(new Action(() => WriteColumnArrangementsInto(_ColumnArrangementSelector)));
                return;
            }
            if (_ColumnArrangementSelector != null) {
                _ColumnArrangementSelector.Item.Clear();
                _ColumnArrangementSelector.Enabled = false;
                _ColumnArrangementSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            }
            if (_Database == null || _Design != enBlueTableAppearance.Standard || _ColumnArrangementSelector == null) {
                if (_ColumnArrangementSelector != null) {
                    _ColumnArrangementSelector.Enabled = false;
                    _ColumnArrangementSelector.Text = string.Empty;
                }
                return;
            }
            foreach (var ThisArrangement in _Database.ColumnArrangements) {
                if (ThisArrangement != null) {
                    if (_ColumnArrangementSelector != null && _Database.PermissionCheck(ThisArrangement.PermissionGroups_Show, null)) {
                        _ColumnArrangementSelector.Item.Add(ThisArrangement.Name, _Database.ColumnArrangements.IndexOf(ThisArrangement).ToString());
                    }
                }
            }
            if (_ColumnArrangementSelector != null) {
                _ColumnArrangementSelector.Enabled = Convert.ToBoolean(_ColumnArrangementSelector.Item.Count > 1);
                _ColumnArrangementSelector.Text = _ColumnArrangementSelector.Item.Count > 0 ? _ArrangementNr.ToString() : string.Empty;
            }
        }

        #region  Store and Restore View 
        public string ViewToString() {
            var x = "{";
            //   x = x & "<Filename>" & _Database.Filename
            x = x + "ArrangementNr=" + _ArrangementNr;
            var tmp = Filter.ToString();
            if (tmp.Length > 2) {
                x = x + ", Filters=" + Filter;
            }
            x = x + ", SliderX=" + SliderX.Value;
            x = x + ", SliderY=" + SliderY.Value;
            if (_PinnedRows != null && _PinnedRows.Count > 0) {
                foreach (var thisRow in _PinnedRows) {
                    x = x + ", Pin=" + thisRow.Key.ToString();
                }
            }
            if (_collapsed != null && _collapsed.Count > 0) {
                foreach (var thiss in _collapsed) {
                    x = x + ", Collapsed=" + thiss.ToNonCritical();
                }
            }
            foreach (var thiscol in CurrentArrangement) {
                if (thiscol._TMP_Reduced) { x = x + ", Reduced=" + thiscol.Column.Key.ToString(); }
            }
            if (_sortDefinitionTemporary?.Columns != null) {
                x = x + ", TempSort=" + _sortDefinitionTemporary;
            }
            x = x + ", CursorPos=" + CellCollection.KeyOfCell(_CursorPosColumn, _CursorPosRow?.Row);
            return x + "}";
        }
        public void ParseView(string ToParse) {
            if (string.IsNullOrEmpty(ToParse)) { return; }
            _PinnedRows.Clear();
            _collapsed.Clear();
            foreach (var pair in ToParse.GetAllTags()) {
                switch (pair.Key) {

                    case "arrangementnr":
                        Arrangement = int.Parse(pair.Value);
                        break;

                    case "filters":
                        Filter.Parse(pair.Value);
                        break;

                    case "sliderx":
                        SliderX.Maximum = Math.Max(SliderX.Maximum, int.Parse(pair.Value));
                        SliderX.Value = int.Parse(pair.Value);
                        break;

                    case "slidery":
                        SliderY.Maximum = Math.Max(SliderY.Maximum, int.Parse(pair.Value));
                        SliderY.Value = int.Parse(pair.Value);
                        break;

                    case "cursorpos":
                        Database.Cell.DataOfCellKey(pair.Value, out var column, out var row);
                        CursorPos_Set(column, row, false);
                        break;

                    case "tempsort":
                        _sortDefinitionTemporary = new RowSortDefinition(_Database, pair.Value);
                        break;

                    case "pin":
                        _PinnedRows.Add(_Database.Row.SearchByKey(int.Parse(pair.Value)));
                        break;

                    case "collapsed":
                        _collapsed.Add(pair.Value.FromNonCritical());
                        break;

                    case "reduced":
                        var c = _Database.Column.SearchByKey(int.Parse(pair.Value));
                        var cv = CurrentArrangement[c];
                        if (cv != null) { cv._TMP_Reduced = true; }
                        break;
                    default:
                        Develop.DebugPrint(enFehlerArt.Warnung, "Tag unbekannt: " + pair.Key);
                        break;
                }
            }
            Filter.OnChanged();
            Invalidate_RowSort(); // beim Parsen wirft der Filter kein Event ab
        }
        private void _Database_StoreView(object sender, System.EventArgs e) =>
            //if (!string.IsNullOrEmpty(_StoredView)) { Develop.DebugPrint("Stored View nicht Empty!"); }
            _StoredView = ViewToString();
        private void _Database_RowKeyChanged(object sender, KeyChangedEventArgs e) {
            // Ist aktuell nur möglich, wenn Pending Changes eine neue Zeile machen
            if (string.IsNullOrEmpty(_StoredView)) { return; }
            _StoredView = _StoredView.Replace("RowKey=" + e.KeyOld + "}", "RowKey=" + e.KeyNew + "}");
        }
        private void _Database_ColumnKeyChanged(object sender, KeyChangedEventArgs e) {
            // Ist aktuell nur möglich,wenn Pending Changes eine neue Zeile machen
            if (string.IsNullOrEmpty(_StoredView)) { return; }
            _StoredView = ColumnCollection.ChangeKeysInString(_StoredView, e.KeyOld, e.KeyNew);
        }
        #endregion


        public bool EnsureVisible(string cellKey) {
            Database.Cell.DataOfCellKey(cellKey, out var column, out var row);
            return EnsureVisible(column, row);
        }
        public bool EnsureVisible(ColumnItem Column, RowItem Row) {
            var ok1 = EnsureVisible(CurrentArrangement[Column]);
            var ok2 = EnsureVisible(Row);
            return ok1 && ok2;
        }
        public bool EnsureVisible(ColumnItem column, clsRowDrawData row) {
            var ok1 = EnsureVisible(CurrentArrangement[column]);
            var ok2 = EnsureVisible(row);
            return ok1 && ok2;
        }
        private bool EnsureVisible(RowItem row) => EnsureVisible(SortedRowData().Get(row));
        private bool EnsureVisible(clsRowDrawData rowdata) {
            if (rowdata == null) { return false; }
            var r = DisplayRectangleWithoutSlider();
            if (DrawY(rowdata) < HeadSize()) {
                SliderY.Value = SliderY.Value + DrawY(rowdata) - HeadSize();
            } else if (DrawY(rowdata) + rowdata.DrawHeight > r.Height) {
                SliderY.Value = SliderY.Value + DrawY(rowdata) + rowdata.DrawHeight - r.Height;
            }
            return true;
        }
        private bool EnsureVisible(ColumnViewItem ViewItem) {
            if (ViewItem == null || ViewItem.Column == null) { return false; }
            if (ViewItem.OrderTMP_Spalte_X1 == null && !ComputeAllColumnPositions()) { return false; }
            var r = DisplayRectangleWithoutSlider();
            ComputeAllColumnPositions();
            if (ViewItem.ViewType == enViewType.PermanentColumn) {
                if (ViewItem.OrderTMP_Spalte_X1 + Column_DrawWidth(ViewItem, r) <= r.Width) { return true; }
                //Develop.DebugPrint(enFehlerArt.Info,"Unsichtbare Wiederholungsspalte: " + ViewItem.Column.Name);
                return false;
            }
            if (ViewItem.OrderTMP_Spalte_X1 < _WiederHolungsSpaltenWidth) {
                SliderX.Value = SliderX.Value + (int)ViewItem.OrderTMP_Spalte_X1 - _WiederHolungsSpaltenWidth;
            } else if (ViewItem.OrderTMP_Spalte_X1 + Column_DrawWidth(ViewItem, r) > r.Width) {
                SliderX.Value = SliderX.Value + (int)ViewItem.OrderTMP_Spalte_X1 + Column_DrawWidth(ViewItem, r) - r.Width;
            }
            return true;
        }
        [DefaultValue(1)]
        [Description("Welche Spaltenanordnung angezeigt werden soll")]
        public int Arrangement {
            get => _ArrangementNr;
            set {
                if (value != _ArrangementNr) {
                    _ArrangementNr = value;
                    Invalidate_HeadSize();
                    Invalidate_AllDraw(false);
                    Invalidate();
                    OnViewChanged();
                    CursorPos_Set(_CursorPosColumn, _CursorPosRow, true);
                }
            }
        }
        public ColumnViewCollection CurrentArrangement => _Database == null || _Database.ColumnArrangements == null || _Database.ColumnArrangements.Count <= _ArrangementNr
                    ? null
                    : _Database.ColumnArrangements[_ArrangementNr];
        private void CellOnCoordinate(int Xpos, int Ypos, out ColumnItem Column, out clsRowDrawData Row) {
            Column = ColumnOnCoordinate(Xpos);
            Row = RowOnCoordinate(Ypos);
        }
        private ColumnItem ColumnOnCoordinate(int Xpos) {
            if (_Database == null || _Database.ColumnArrangements.Count - 1 < _ArrangementNr) { return null; }
            foreach (var ThisViewItem in CurrentArrangement) {
                if (ThisViewItem?.Column != null) {
                    if (Xpos >= ThisViewItem.OrderTMP_Spalte_X1 && Xpos <= ThisViewItem.OrderTMP_Spalte_X1 + Column_DrawWidth(ThisViewItem, DisplayRectangleWithoutSlider())) {
                        return ThisViewItem.Column;
                    }
                }
            }
            return null;
        }
        private clsRowDrawData RowOnCoordinate(int y) {
            if (_Database == null || y <= HeadSize()) { return null; }
            var s = SortedRowData();
            foreach (var ThisRowItem in s) {
                if (ThisRowItem != null) {
                    if (y >= DrawY(ThisRowItem) &&
                        y <= DrawY(ThisRowItem) + ThisRowItem.DrawHeight
                        && ThisRowItem.Expanded) {
                        return ThisRowItem;
                    }
                }
            }
            return null;
        }
        private string RowCaptionOnCoordinate(int XPos, int YPos) {
            try {
                var s = SortedRowData();
                foreach (var thisRow in s) {
                    if (thisRow.CaptionPos is Rectangle r) {
                        if (r.Contains(XPos, YPos)) { return thisRow.Chapter; }
                    }
                }
            } catch { }
            return string.Empty;
        }
        private int HeadSize() {
            if (_HeadSize != null) { return (int)_HeadSize; }
            if (_Database.ColumnArrangements.Count - 1 < _ArrangementNr) {
                _HeadSize = 0;
                return 0;
            }
            if (_Design == enBlueTableAppearance.OnlyMainColumnWithoutHead || CurrentArrangement.Count() - 1 < 0) {
                _HeadSize = 0;
                return 0;
            }
            _HeadSize = 16;
            foreach (var ThisViewItem in CurrentArrangement) {
                if (ThisViewItem?.Column != null) {
                    _HeadSize = Math.Max((int)_HeadSize, (int)ColumnHead_Size(ThisViewItem.Column).Height);
                }
            }
            _HeadSize += 8;
            _HeadSize += _AutoFilterSize;
            return (int)_HeadSize;
        }
        private bool IsOnScreen(ColumnViewItem ViewItem, Rectangle displayRectangleWOSlider) {
            if (ViewItem == null) { return false; }
            if (_Design is enBlueTableAppearance.Standard or enBlueTableAppearance.OnlyMainColumnWithoutHead) {
                if (ViewItem.OrderTMP_Spalte_X1 + Column_DrawWidth(ViewItem, displayRectangleWOSlider) >= 0 && ViewItem.OrderTMP_Spalte_X1 <= displayRectangleWOSlider.Width) { return true; }
            } else {
                Develop.DebugPrint(_Design);
            }
            return false;
        }
        private bool IsOnScreen(ColumnItem column, clsRowDrawData row, Rectangle displayRectangleWOSlider) => IsOnScreen(CurrentArrangement[column], displayRectangleWOSlider) && IsOnScreen(row, displayRectangleWOSlider);
        private bool IsOnScreen(clsRowDrawData vrow, Rectangle displayRectangleWOSlider) => vrow != null && DrawY(vrow) + vrow.DrawHeight >= HeadSize() && DrawY(vrow) <= displayRectangleWOSlider.Height;
        public static int tmpColumnContentWidth(Table table, ColumnItem Column, BlueFont CellFont, int Pix16) {
            if (Column.TMP_ColumnContentWidth != null) { return (int)Column.TMP_ColumnContentWidth; }
            Column.TMP_ColumnContentWidth = 0;
            if (Column.Format == enDataFormat.Button) {
                // Beim Button reicht eine Abfrage mit Row null
                Column.TMP_ColumnContentWidth = Cell_ContentSize(table, Column, null, CellFont, Pix16).Width;
            } else {
                foreach (var ThisRowItem in Column.Database.Row) {
                    if (ThisRowItem != null && !ThisRowItem.CellIsNullOrEmpty(Column)) {
                        var t = Column.TMP_ColumnContentWidth; // ja, dank Multithreading kann es sein, dass hier das hier null ist
                        if (t == null) { t = 0; }
                        Column.TMP_ColumnContentWidth = Math.Max((int)t, Cell_ContentSize(table, Column, ThisRowItem, CellFont, Pix16).Width);
                    }
                }
            }
            return Column.TMP_ColumnContentWidth is int w ? w : 0;
        }
        private int Column_DrawWidth(ColumnViewItem ViewItem, Rectangle displayRectangleWOSlider) {
            // Hier wird die ORIGINAL-Spalte gezeichnet, nicht die FremdZelle!!!!
            //Develop.DebugPrint_InvokeRequired(InvokeRequired, true);
            if (ViewItem._TMP_DrawWidth != null) { return (int)ViewItem._TMP_DrawWidth; }
            if (ViewItem == null || ViewItem.Column == null) { return 0; }
            if (_Design == enBlueTableAppearance.OnlyMainColumnWithoutHead) {
                ViewItem._TMP_DrawWidth = displayRectangleWOSlider.Width - 1;
                return (int)ViewItem._TMP_DrawWidth;
            }
            ViewItem._TMP_DrawWidth = ViewItem._TMP_Reduced
                ? ViewItem.Column.BildCode_ConstantHeight + 2
                : ViewItem.ViewType == enViewType.PermanentColumn
                    ? Math.Min(tmpColumnContentWidth(this, ViewItem.Column, _Cell_Font, Pix16), (int)(displayRectangleWOSlider.Width * 0.3))
                    : Math.Min(tmpColumnContentWidth(this, ViewItem.Column, _Cell_Font, Pix16), (int)(displayRectangleWOSlider.Width * 0.75));
            ViewItem._TMP_DrawWidth = Math.Max((int)ViewItem._TMP_DrawWidth, _AutoFilterSize); // Mindestens so groß wie der Autofilter;
            ViewItem._TMP_DrawWidth = Math.Max((int)ViewItem._TMP_DrawWidth, (int)ColumnHead_Size(ViewItem.Column).Width);
            return (int)ViewItem._TMP_DrawWidth;
        }
        private int DrawY(clsRowDrawData r) => r.Y + HeadSize() - (int)SliderY.Value;
        private int Row_DrawHeight(RowItem vrow, Rectangle displayRectangleWOSlider) {
            if (_Design == enBlueTableAppearance.OnlyMainColumnWithoutHead) { return Cell_ContentSize(this, _Database.Column[0], vrow, _Cell_Font, Pix16).Height; }
            var tmp = Pix18;
            foreach (var ThisViewItem in CurrentArrangement) {
                if (ThisViewItem?.Column != null) {
                    if (!vrow.CellIsNullOrEmpty(ThisViewItem.Column)) {
                        tmp = Math.Max(tmp, Cell_ContentSize(this, ThisViewItem.Column, vrow, _Cell_Font, Pix16).Height);
                    }
                }
            }
            tmp = Math.Min(tmp, (int)(displayRectangleWOSlider.Height * 0.9) - HeadSize());
            tmp = Math.Max(tmp, Pix18);
            return tmp;
        }
        private RowSortDefinition SortUsed() => _sortDefinitionTemporary?.Columns != null
? _sortDefinitionTemporary
: _Database.SortDefinition?.Columns != null ? _Database.SortDefinition : null;
        public List<RowItem> PinnedRows => _PinnedRows;
        public List<RowItem> SortedRows() {
            if (AreRowsSorted()) { return _SortedRows; }
            if (Database == null) { return new List<RowItem>(); }
            _SortedRows = Database.Row.CalculateSortedRows(Filter, SortUsed(), _PinnedRows);
            if (_SortedRows.IsDifferentTo(_SortedRowsBefore)) {
                _SortedRowsBefore.Clear();
                _SortedRowData = null;
                if (_SortedRows != null) { _SortedRowsBefore.AddRange(_SortedRows); }
                if (_CursorPosRow != null && _CursorPosRow.Row != null && !_SortedRows.Contains(_CursorPosRow.Row)) { CursorPos_Reset(); }
                _MouseOverRow = null;
                EnsureVisible(_CursorPosColumn, _CursorPosRow);
                OnVisibleRowsChanged();
            }
            return SortedRows(); // Rekursiver aufruf. Manchmal funktiniert OnRowsSorted nicht...
        }
        private List<clsRowDrawData> SortedRowData() {
            if (_SortedRowData != null) { return _SortedRowData; }
            var DisplayR = DisplayRectangleWithoutSlider();
            var MaxY = 0;
            if (UserEdit_NewRowAllowed()) { MaxY += Pix18; }
            var expanded = true;
            var LastCap = string.Empty;
            var sr = SortedRows(); //  SortedRows setzt evtl. _SortedRowData auf null
            _SortedRowData = new List<clsRowDrawData>();
            foreach (var ThisRow in sr) {
                clsRowDrawData ThisRowData = new(ThisRow) {
                    Y = MaxY // (int)(MaxY - SliderY.Value + HeadSize());
                };
                var tmpCap = ThisRow.CellGetString(Database.Column.SysChapter);
                if (tmpCap != LastCap) {
                    ThisRowData.Y += RowCaptionSizeY;
                    LastCap = tmpCap;
                    ThisRowData.Chapter = ThisRow.CaptionReadable();
                    expanded = !_collapsed.Contains(ThisRowData.Chapter);
                    MaxY += RowCaptionSizeY;
                } else {
                    ThisRowData.Chapter = string.Empty;
                }
                ThisRowData.Expanded = expanded || _PinnedRows.Contains(ThisRow);
                if (ThisRowData.Expanded) {
                    ThisRowData.DrawHeight = Row_DrawHeight(ThisRow, DisplayR);
                    MaxY += ThisRowData.DrawHeight;
                }
                _SortedRowData.Add(ThisRowData);
            }
            // Slider berechnen ---------------------------------------------------------------
            SliderSchaltenSenk(DisplayR, MaxY);
            //if (AreRowsSorted()) { return _SortedRowData; }
            //if (Database == null) { return new List<clsRowDrawData>(); }
            //_SortedRowData = Database.Row.CalculateSortedRows(Filter, SortUsed(), _PinnedRows);
            //if (_SortedRowData.IsDifferentTo(_SortedRowsBefore)) {
            //    _SortedRowsBefore.Clear();
            //    if (_SortedRowData != null) { _SortedRowsBefore.AddRange(_SortedRowData); }
            //    EnsureVisible(_CursorPosColumn, _CursorPosRow);
            //    OnVisibleRowsChanged();
            //}
            return SortedRowData(); // Rekursiver aufruf. Manchmal funktiniert OnRowsSorted nicht...
        }
        private void OnNeedButtonArgs(ButtonCellEventArgs e) => NeedButtonArgs?.Invoke(this, e);
        private void OnButtonCellClicked(CellEventArgs e) => ButtonCellClicked?.Invoke(this, e);
        private void OnVisibleRowsChanged() => VisibleRowsChanged?.Invoke(this, System.EventArgs.Empty);
        private void OnPinnedChanged() => PinnedChanged?.Invoke(this, System.EventArgs.Empty);
        private void OnViewChanged() => ViewChanged?.Invoke(this, System.EventArgs.Empty);
        private void OnDatabaseChanged() => DatabaseChanged?.Invoke(this, System.EventArgs.Empty);
        private void OnColumnArrangementChanged() => ColumnArrangementChanged?.Invoke(this, System.EventArgs.Empty);
        public RowItem View_RowFirst() => _Database == null ? null : SortedRowData().Count == 0 ? null : SortedRowData()[0].Row;
        public RowItem View_RowLast() => _Database == null ? null : SortedRowData().Count == 0 ? null : SortedRowData()[SortedRowData().Count - 1].Row;
        private bool Autofilter_Sinnvoll(ColumnItem column) {
            if (column.TMP_AutoFilterSinnvoll != null) { return (bool)column.TMP_AutoFilterSinnvoll; }
            for (var rowcount = 0; rowcount <= SortedRows().Count - 2; rowcount++) {
                if (SortedRows()[rowcount].CellGetString(column) != SortedRows()[rowcount + 1].CellGetString(column)) {
                    column.TMP_AutoFilterSinnvoll = true;
                    return true;
                }
            }
            column.TMP_AutoFilterSinnvoll = false;
            return false;
        }
        private int Autofilter_Text(ColumnItem column) {
            if (column.TMP_IfFilterRemoved != null) { return (int)column.TMP_IfFilterRemoved; }
            FilterCollection tfilter = new(Database);
            foreach (var ThisFilter in Filter) {
                if (ThisFilter != null && ThisFilter.Column != column) { tfilter.Add(ThisFilter); }
            }
            var temp = Database.Row.CalculateSortedRows(tfilter, SortUsed(), _PinnedRows);
            column.TMP_IfFilterRemoved = SortedRowData().Count - temp.Count;
            return (int)column.TMP_IfFilterRemoved;
        }
        private void Invalidate_AllDraw(bool allOrder) {
            if (_Database == null) { return; }
            Invalidate_SortedRowData();
            if (allOrder) {
                foreach (var ThisArrangement in _Database.ColumnArrangements) {
                    if (ThisArrangement != null) {
                        foreach (var ThisViewItem in ThisArrangement) {
                            Invalidate_DrawWidth(ThisViewItem);
                        }
                        if (_Database.ColumnArrangements.IndexOf(ThisArrangement) == 0) {
                            _Database.ColumnArrangements[0].ShowAllColumns();
                        }
                    }
                }
                Invalidate_HeadSize();
            } else {
                if (_ArrangementNr < _Database.ColumnArrangements.Count - 1) {
                    foreach (var ThisViewItem in CurrentArrangement) {
                        Invalidate_DrawWidth(ThisViewItem);
                    }
                }
            }
        }
        private void Invalidate_DrawWidth(ColumnItem vcolumn) => Invalidate_DrawWidth(CurrentArrangement[vcolumn]);
        private void Invalidate_DrawWidth(ColumnViewItem ViewItem) {
            if (ViewItem == null) { return; }
            ViewItem._TMP_DrawWidth = null;
        }
        public void Invalidate_HeadSize() {
            if (_HeadSize != null) { Invalidate(); }
            _HeadSize = null;
        }
        //private void Invalidate_TMPDrawHeight(RowItem vrow) {
        //    if (vrow == null) { return; }
        //    vrow.TMP_DrawHeight = null;
        //}
        private void _Database_ColumnContentChanged(object sender, ListEventArgs e) {
            Invalidate_AllDraw(true);
            Invalidate_RowSort();
            Invalidate();
        }
        public RowItem View_NextRow(RowItem row) {
            if (_Database == null) { return null; }
            var RowNr = SortedRows().IndexOf(row);
            return RowNr < 0 || RowNr >= SortedRows().Count - 1 ? null : SortedRows()[RowNr + 1];
        }
        public RowItem View_PreviousRow(RowItem row) {
            if (_Database == null) { return null; }
            var RowNr = SortedRows().IndexOf(row);
            return RowNr < 1 ? null : SortedRows()[RowNr - 1];
        }
        /// <summary>
        /// Berechnet die Zelle, ausgehend von einer Zellenposition. Dabei wird die Columns und Zeilensortierung berücksichtigt.
        /// Gibt des keine Nachbarszelle, wird die Eingangszelle zurückgegeben.
        /// </summary>
        /// <remarks></remarks>
        private void Neighbour(ColumnItem column, RowItem row, enDirection direction, out ColumnItem newColumn, out RowItem newRow) {
            newColumn = column;
            newRow = row;
            if (_Design == enBlueTableAppearance.OnlyMainColumnWithoutHead) {
                if (direction is not enDirection.Oben and not enDirection.Unten) {
                    newColumn = _Database.Column[0];
                    return;
                }
            }
            if (newColumn != null) {
                if (Convert.ToBoolean(direction & enDirection.Links)) {
                    if (CurrentArrangement.PreviousVisible(newColumn) != null) {
                        newColumn = CurrentArrangement.PreviousVisible(newColumn);
                    }
                }
                if (Convert.ToBoolean(direction & enDirection.Rechts)) {
                    if (CurrentArrangement.NextVisible(newColumn) != null) {
                        newColumn = CurrentArrangement.NextVisible(newColumn);
                    }
                }
            }
            if (newRow != null) {
                if (Convert.ToBoolean(direction & enDirection.Oben)) {
                    if (View_PreviousRow(newRow) != null) { newRow = View_PreviousRow(newRow); }
                }
                if (Convert.ToBoolean(direction & enDirection.Unten)) {
                    if (View_NextRow(newRow) != null) { newRow = View_NextRow(newRow); }
                }
            }
        }
        private void _Database_SortParameterChanged(object sender, System.EventArgs e) {
            Invalidate_RowSort();
            Invalidate();
        }
        private void Filter_Changed(object sender, System.EventArgs e) {
            Invalidate_Filterinfo();
            Invalidate_RowSort();
            Invalidate();
            OnFilterChanged();
        }
        private void OnFilterChanged() => FilterChanged?.Invoke(this, System.EventArgs.Empty);
        public bool Mouse_IsInHead() => Convert.ToBoolean(MousePos().Y <= HeadSize());
        private bool Mouse_IsInAutofilter(ColumnViewItem ViewItem, System.Windows.Forms.MouseEventArgs e) => ViewItem != null && ViewItem._TMP_AutoFilterLocation.Width != 0 && ViewItem.Column.AutoFilterSymbolPossible() && ViewItem._TMP_AutoFilterLocation.Contains(e.X, e.Y);
        private bool Mouse_IsInRedcueButton(ColumnViewItem ViewItem, System.Windows.Forms.MouseEventArgs e) => ViewItem != null && ViewItem._TMP_ReduceLocation.Width != 0 && ViewItem._TMP_ReduceLocation.Contains(e.X, e.Y);
        private SizeF ColumnCaptionText_Size(ColumnItem Column) {
            if (Column.TMP_CaptionText_Size.Width > 0) { return Column.TMP_CaptionText_Size; }
            if (_Column_Font == null) { return new SizeF(Pix16, Pix16); }
            Column.TMP_CaptionText_Size = BlueFont.MeasureString(Column.Caption.Replace("\r", "\r\n"), _Column_Font.Font());
            return Column.TMP_CaptionText_Size;
        }
        private bool UserEdit_NewRowAllowed() => _Database != null && _Database.Column.Count != 0
&& _Database.Column[0] != null
&& _Design != enBlueTableAppearance.OnlyMainColumnWithoutHead
&& _Database.ColumnArrangements.Count != 0
&& (CurrentArrangement == null || CurrentArrangement[_Database.Column[0]] != null)
&& _Database.PermissionCheck(_Database.PermissionGroups_NewRow, null)
&& CellCollection.UserEditPossible(_Database.Column[0], null, enErrorReason.EditNormaly);
        private void _Database_RowRemoved(object sender, System.EventArgs e) {
            Invalidate_RowSort();
            Invalidate();
        }
        private void _Database_ViewChanged(object sender, System.EventArgs e) {
            InitializeSkin(); // Sicher ist sicher, um die neuen Schrift-Größen zu haben.
            Invalidate_HeadSize();
            Invalidate_AllDraw(true);
            Invalidate_RowSort();
            CursorPos_Set(_CursorPosColumn, _CursorPosRow, false);
            Invalidate();
        }
        protected override void OnVisibleChanged(System.EventArgs e) {
            base.OnVisibleChanged(e);
            if (_Database == null) { return; }
            lock (Lock_UserAction) {
                if (ISIN_VisibleChanged) { return; }
                ISIN_VisibleChanged = true;
                Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
                ISIN_VisibleChanged = false;
            }
        }
        private void _Database_SavedToDisk(object sender, System.EventArgs e) => Invalidate();
        private void _Database_DropMessage(object sender, MessageEventArgs e) {
            if (_Database.IsAdministrator()) {
                MessageBox.Show(e.Message);
            }
        }
        private void _Database_ProgressbarInfo(object sender, ProgressbarEventArgs e) {
            if (e.Ends) {
                PG?.Close();
                return;
            }
            if (e.Beginns) {
                PG = Progressbar.Show(e.Name, e.Count);
                return;
            }
            PG.Update(e.Current);
        }
        public void OpenSearchAndReplace() {
            if (!Database.IsAdministrator()) { return; }
            if (Database.ReadOnly) { return; }
            if (_searchAndReplace == null || _searchAndReplace.IsDisposed || !_searchAndReplace.Visible) {
                _searchAndReplace = new SearchAndReplace(this);
                _searchAndReplace.Show();
            }
        }
        public void GetContextMenuItems(System.Windows.Forms.MouseEventArgs e, ItemCollectionList Items, out object HotItem, List<string> Tags, ref bool Cancel, ref bool Translate) {
            HotItem = null;
            if (_Database.IsParsing || _Database.IsLoading) {
                Cancel = true;
                return;
            }
            Database?.Load_Reload();
            if (e == null) {
                Cancel = true;
                return;
            }
            CellOnCoordinate(e.X, e.Y, out _MouseOverColumn, out _MouseOverRow);
            Tags.TagSet("CellKey", CellCollection.KeyOfCell(_MouseOverColumn, _MouseOverRow?.Row));
            if (_MouseOverColumn != null) {
                Tags.TagSet("ColumnKey", _MouseOverColumn.Key.ToString());
            }
            if (_MouseOverRow != null && _MouseOverRow.Row != null) {
                Tags.TagSet("RowKey", _MouseOverRow.Row.Key.ToString());
            }
        }
        public void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);
        private SizeF ColumnHead_Size(ColumnItem column) {
            float wi;
            float he;
            Bitmap CaptionBitmap = null; // TODO: Caption Bitmap neu erstellen
            if (CaptionBitmap != null && CaptionBitmap.Width > 10) {
                wi = Math.Max(50, ColumnCaptionText_Size(column).Width + 4);
                he = 50 + ColumnCaptionText_Size(column).Height + 3;
            } else {
                wi = ColumnCaptionText_Size(column).Height + 4;
                he = ColumnCaptionText_Size(column).Width + 3;
            }
            if (!string.IsNullOrEmpty(column.Ueberschrift3)) {
                he += ColumnCaptionSizeY * 3;
            } else if (!string.IsNullOrEmpty(column.Ueberschrift2)) {
                he += ColumnCaptionSizeY * 2;
            } else if (!string.IsNullOrEmpty(column.Ueberschrift1)) {
                he += ColumnCaptionSizeY;
            }
            return new SizeF(wi, he);
        }
        private void NotEditableInfo(string Reason) => Notification.Show(Reason, enImageCode.Kreuz);// QickInfo beisst sich mit den letzten Änderungen Quickinfo//DialogBoxes.QuickInfo.Show("<IMAGECODE=Stift|16||1> " + Reason);
        public static Size Cell_ContentSize(Table table, ColumnItem Column, RowItem Row, BlueFont CellFont, int Pix16) {
            if (Column.Format == enDataFormat.LinkedCell) {
                (var lcolumn, var lrow) = CellCollection.LinkedCellData(Column, Row, false, false);
                return lcolumn != null && lrow != null ? Cell_ContentSize(table, lcolumn, lrow, CellFont, Pix16) : new Size(Pix16, Pix16);
            }
            var _ContentSize = Column.Database.Cell.GetSizeOfCellContent(Column, Row);
            if (_ContentSize.Width > 0 && _ContentSize.Height > 0) { return _ContentSize; }
            if (Column.Format == enDataFormat.Button) {
                if (table is null) {
                    _ContentSize = new Size(Pix16, Pix16);
                } else {
                    ButtonCellEventArgs e = new(Column, Row);
                    table.OnNeedButtonArgs(e);
                    _ContentSize = Button.StandardSize(e.Text, e.Image);
                }
            } else if (Column.MultiLine) {
                var TMP = Column.Database.Cell.GetList(Column, Row);
                if (Column.ShowMultiLineInOneLine) {
                    _ContentSize = FormatedText_NeededSize(Column, TMP.JoinWith("; "), CellFont, enShortenStyle.Replaced, Pix16, Column.BildTextVerhalten);
                } else {
                    foreach (var ThisString in TMP) {
                        var TMPSize = FormatedText_NeededSize(Column, ThisString, CellFont, enShortenStyle.Replaced, Pix16, Column.BildTextVerhalten);
                        _ContentSize.Width = Math.Max(TMPSize.Width, _ContentSize.Width);
                        _ContentSize.Height += Math.Max(TMPSize.Height, Pix16);
                    }
                }
            } else {
                var _String = Column.Database.Cell.GetString(Column, Row);
                _ContentSize = FormatedText_NeededSize(Column, _String, CellFont, enShortenStyle.Replaced, Pix16, Column.BildTextVerhalten);
            }
            _ContentSize.Width = Math.Max(_ContentSize.Width, Pix16);
            _ContentSize.Height = Math.Max(_ContentSize.Height, Pix16);
            if (Skin.Scale == 1 && LanguageTool.Translation == null) { Column.Database.Cell.SetSizeOfCellContent(Column, Row, _ContentSize); }
            return _ContentSize;
        }
        public static void DoUndo(ColumnItem column, RowItem row) {
            if (column == null) { return; }
            if (row == null) { return; }
            if (column.Format == enDataFormat.LinkedCell) {
                (var lcolumn, var lrow) = CellCollection.LinkedCellData(column, row, true, false);
                if (lcolumn != null && lrow != null) { DoUndo(lcolumn, lrow); }
                return;
            }
            var CellKey = CellCollection.KeyOfCell(column, row);
            var i = UndoItems(column.Database, CellKey);
            if (i.Count < 1) {
                MessageBox.Show("Keine vorherigen Inhalte<br>(mehr) vorhanden.", enImageCode.Information, "OK");
                return;
            }
            i.Appearance = enBlueListBoxAppearance.Listbox;
            var v = InputBoxListBoxStyle.Show("Vorherigen Eintrag wählen:", i, enAddType.None, true);
            if (v == null || v.Count != 1) { return; }
            if (v[0] == "Cancel") { return; } // =Aktueller Eintrag angeklickt
            row.CellSet(column, v[0].Substring(5));
            //Database.Cell.Set(column, row, v[0].Substring(5), false);
            row.DoAutomatic(true, true, 5, "value changed");
        }
        public static ItemCollectionList UndoItems(Database database, string cellkey) {
            ItemCollectionList i = new(enBlueListBoxAppearance.KontextMenu) {
                CheckBehavior = enCheckBehavior.AlwaysSingleSelection
            };
            if (database.Works == null || database.Works.Count == 0) { return i; }
            var isfirst = true;
            TextListItem Las = null;
            var LasNr = -1;
            var co = 0;
            for (var z = database.Works.Count - 1; z >= 0; z--) {
                if (database.Works[z].CellKey == cellkey && database.Works[z].HistorischRelevant) {
                    co++;
                    LasNr = z;
                    Las = isfirst
                        ? new TextListItem("Aktueller Text - ab " + database.Works[z].Date + " UTC, geändert von " + database.Works[z].User, "Cancel", null, false, true, string.Empty)
                        : new TextListItem("ab " + database.Works[z].Date + " UTC, geändert von " + database.Works[z].User, co.ToString(Constants.Format_Integer5) + database.Works[z].ChangedTo, null, false, true, string.Empty);
                    isfirst = false;
                    if (Las != null) { i.Add(Las); }
                }
            }
            if (Las != null) {
                co++;
                i.Add("vor " + database.Works[LasNr].Date + " UTC", co.ToString(Constants.Format_Integer5) + database.Works[LasNr].PreviousValue);
            }
            return i;
        }
        public static void CopyToClipboard(ColumnItem Column, RowItem Row, bool Meldung) {
            try {
                if (Row != null && Column.Format.CanBeCheckedByRules()) {
                    var c = Row.CellGetString(Column);
                    c = c.Replace("\r\n", "\r");
                    c = c.Replace("\r", "\r\n");
                    System.Windows.Forms.Clipboard.SetDataObject(c, true);
                    if (Meldung) { Notification.Show(LanguageTool.DoTranslate("<b>{0}</b><br>ist nun in der Zwischenablage.", true, c), enImageCode.Kopieren); }
                } else {
                    if (Meldung) { Notification.Show(LanguageTool.DoTranslate("Bei dieser Spalte nicht möglich."), enImageCode.Warnung); }
                }
            } catch {
                if (Meldung) { Notification.Show(LanguageTool.DoTranslate("Unerwarteter Fehler beim Kopieren."), enImageCode.Warnung); }
            }
        }
        public static void SearchNextText(string searchTXT, Table TableView, ColumnItem column, RowItem row, out ColumnItem foundColumn, out RowItem foundRow, bool VereinfachteSuche) {
            searchTXT = searchTXT.Trim();
            var ca = TableView.CurrentArrangement;
            if (TableView.Design == enBlueTableAppearance.OnlyMainColumnWithoutHead) {
                ca = TableView.Database.ColumnArrangements[0];
            }
            if (row == null) { row = TableView.View_RowLast(); }
            if (column == null) { column = TableView.Database.Column.SysLocked; }
            var rowsChecked = 0;
            if (string.IsNullOrEmpty(searchTXT)) {
                MessageBox.Show("Bitte Text zum Suchen eingeben.", enImageCode.Information, "OK");
                foundColumn = null;
                foundRow = null;
                return;
            }
            do {
                column = TableView.Design == enBlueTableAppearance.OnlyMainColumnWithoutHead ? column.Next() : ca.NextVisible(column);
                if (column == null) {
                    column = ca[0].Column;
                    if (rowsChecked > TableView.Database.Row.Count() + 1) {
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
                if (column.Format == enDataFormat.LinkedCell) {
                    (ContentHolderCellColumn, ContenHolderCellRow) = CellCollection.LinkedCellData(column, row, false, false);
                }
                var _Ist1 = string.Empty;
                var _Ist2 = string.Empty;
                if (ContenHolderCellRow != null && ContentHolderCellColumn != null) {
                    _Ist1 = ContenHolderCellRow.CellGetString(ContentHolderCellColumn);
                    _Ist2 = CellItem.ValuesReadable(ContentHolderCellColumn, ContenHolderCellRow, enShortenStyle.Both).JoinWithCr();
                }
                if (ContentHolderCellColumn != null && ContentHolderCellColumn.Format == enDataFormat.Text_mit_Formatierung) {
                    ExtText l = new(enDesign.TextBox, enStates.Standard) {
                        HtmlText = _Ist1
                    };
                    _Ist1 = l.PlainText;
                }
                // Allgemeine Prüfung
                if (!string.IsNullOrEmpty(_Ist1) && _Ist1.ToLower().Contains(searchTXT.ToLower())) {
                    foundColumn = column;
                    foundRow = row;
                    return;
                }
                // Prüfung mit und ohne Ersetzungen / Prefix / Suffix
                if (!string.IsNullOrEmpty(_Ist2) && _Ist2.ToLower().Contains(searchTXT.ToLower())) {
                    foundColumn = column;
                    foundRow = row;
                    return;
                }
                if (VereinfachteSuche) {
                    var _Ist3 = _Ist2.StarkeVereinfachung(" ,");
                    var _searchTXT3 = searchTXT.StarkeVereinfachung(" ,");
                    if (!string.IsNullOrEmpty(_Ist3) && _Ist3.ToLower().Contains(_searchTXT3.ToLower())) {
                        foundColumn = column;
                        foundRow = row;
                        return;
                    }
                }
            } while (true);
        }
        /// <summary>
        /// Der Status des Bildes wird geändert, Texte werden gekürzt
        /// </summary>
        /// <param name="gr"></param>
        /// <param name="column"></param>
        /// <param name="originalText"></param>
        /// <param name="fitInRect"></param>
        /// <param name="Child"></param>
        /// <param name="deleteBack"></param>
        /// <param name="font"></param>
        private static void Draw_FormatedText(Graphics gr, ColumnItem column, string originalText, Rectangle fitInRect, bool deleteBack, BlueFont font, enShortenStyle style, enStates state, enBildTextVerhalten bildTextverhalten) {
            var tmpData = CellItem.GetDrawingData(column, originalText, style, bildTextverhalten);
            var tmpImageCode = tmpData.Item3;
            if (tmpImageCode != null) { tmpImageCode = QuickImage.Get(tmpImageCode, Skin.AdditionalState(state)); }
            Skin.Draw_FormatedText(gr, tmpData.Item1, tmpImageCode, tmpData.Item2, fitInRect, null, deleteBack, font, false);
        }
        /// <summary>
        /// Status des Bildes (Disabled) wird geändert. Diese Routine sollte nicht innerhalb der Table Klasse aufgerufen werden.
        /// Sie dient nur dazu, das Aussehen eines Textes wie eine Zelle zu imitieren.
        /// </summary>
        public static void Draw_FormatedText(ColumnItem column, string Txt, Graphics GR, Rectangle FitInRect, bool DeleteBack, enShortenStyle Style, enDesign vDesign, enStates vState, enBildTextVerhalten bildTextverhalten) {
            if (string.IsNullOrEmpty(Txt)) { return; }
            var d = Skin.DesignOf(vDesign, vState);
            Draw_FormatedText(GR, column, Txt, FitInRect, DeleteBack, d.bFont, Style, vState, bildTextverhalten);
        }
        public static Size FormatedText_NeededSize(ColumnItem column, string originalText, BlueFont font, enShortenStyle style, int minSize, enBildTextVerhalten bildTextverhalten) {
            var tmpData = CellItem.GetDrawingData(column, originalText, style, bildTextverhalten);
            return Skin.FormatedText_NeededSize(tmpData.Item1, tmpData.Item3, font, minSize);
        }
        public override string QuickInfoText {
            get {
                var t1 = base.QuickInfoText;
                var t2 = _MouseOverText;
                //if (_MouseOverItem != null) { t2 = _MouseOverItem.QuickInfo; }
                if (string.IsNullOrEmpty(t1) && string.IsNullOrEmpty(t2)) {
                    return string.Empty;
                } else if (string.IsNullOrEmpty(t1) && string.IsNullOrEmpty(t2)) {
                    return t1 + "<br><hr><br>" + t2;
                } else {
                    return t1 + t2; // Eins davon ist leer
                }
            }
        }
    }
}
