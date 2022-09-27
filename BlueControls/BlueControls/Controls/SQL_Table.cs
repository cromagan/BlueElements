﻿// Authors:
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
using BlueBasics.EventArgs;
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Extended_Text;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using BlueDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BlueControls.ItemCollection.ItemCollectionList;
using static BlueBasics.Converter;
using static BlueBasics.IO;

#nullable enable

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
[DefaultEvent("SelectedRowChanged")]
[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public partial class SQL_Table : GenericControl, IContextMenu, IBackgroundNone, ITranslateable {

    #region Fields

    public static SolidBrush BrushYellowTransparent = new(Color.FromArgb(180, 255, 255, 0));
    public static Pen PenRed1 = new(Color.Red, 1);
    private const int AutoFilterSize = 22;
    private const int ColumnCaptionSizeY = 22;
    private const int RowCaptionSizeY = 50;
    private static bool _serviceStarted;
    private readonly List<string> _collapsed = new();
    private readonly object _lockUserAction = new();
    private int _arrangementNr = 1;
    private SQL_AutoFilter? _autoFilter;
    private BlueFont? _cellFont;
    private BlueFont? _chapterFont;
    private BlueFont? _columnFilterFont;
    private BlueFont? _columnFont;
    private SQL_ColumnItem? _cursorPosColumn;
    private SQL_RowData? _cursorPosRow;
    private SQL_Database? _database;
    private BlueTableAppearance _design = BlueTableAppearance.Standard;
    private List<SQL_RowItem>? _filteredRows;
    private int? _headSize;
    private bool _isinClick;
    private bool _isinDoubleClick;
    private bool _isinKeyDown;
    private bool _isinMouseDown;
    private bool _isinMouseEnter;
    private bool _isinMouseLeave;
    private bool _isinMouseMove;
    private bool _isinMouseUp;
    private bool _isinMouseWheel;
    private bool _isinSizeChanged;
    private bool _isinVisibleChanged;

    /// <summary>
    ///  Wird SQL_DatabaseAdded gehandlet?
    /// </summary>
    private SQL_ColumnItem? _mouseOverColumn;

    private SQL_RowData? _mouseOverRow;
    private string _mouseOverText = string.Empty;
    private BlueFont? _newRowFont;
    private Progressbar? _pg;
    private int _pix16 = 16;
    private int _pix18 = 18;
    private int _rowCaptionFontY = 26;
    private SearchAndReplace? _searchAndReplace;
    private bool _showNumber;
    private SQL_RowSortDefinition? _sortDefinitionTemporary;
    private List<SQL_RowData>? _sortedSQL_RowData;
    private string _storedView = string.Empty;
    private Rectangle _tmpCursorRect = Rectangle.Empty;
    private SQL_RowItem? _unterschiede;
    private int _wiederHolungsSpaltenWidth;

    #endregion

    #region Constructors

    public SQL_Table() : base(true, false) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        MouseHighlight = false;
    }

    #endregion

    #region Events

    public event EventHandler<SQL_FilterEventArgs> AutoFilterClicked;

    public event EventHandler<SQL_CellEventArgs> ButtonCellClicked;

    public event EventHandler<SQL_CellEventArgs> CellValueChanged;

    public event EventHandler<CellValueChangingByUserEventArgs> CellValueChangingByUser;

    public event EventHandler ColumnArrangementChanged;

    public event EventHandler<ContextMenuInitEventArgs> ContextMenuInit;

    public event EventHandler<ContextMenuItemClickedEventArgs> ContextMenuItemClicked;

    public event EventHandler DatabaseChanged;

    public new event EventHandler<CellDoubleClickEventArgs> DoubleClick;

    public event EventHandler<SQL_CellCancelEventArgs> EditBeforeBeginEdit;

    public event EventHandler FilterChanged;

    public event EventHandler<ButtonCellEventArgs> NeedButtonArgs;

    public event EventHandler PinnedChanged;

    public event EventHandler<SQL_RowEventArgs> RowAdded;

    public event EventHandler<SQL_CellExtEventArgs> SelectedCellChanged;

    public event EventHandler<SQL_RowEventArgs> SelectedRowChanged;

    public event EventHandler ViewChanged;

    public event EventHandler VisibleRowsChanged;

    #endregion

    #region Properties

    [DefaultValue(1)]
    [Description("Welche Spaltenanordnung angezeigt werden soll")]
    public int Arrangement {
        get => _arrangementNr;
        set {
            if (value != _arrangementNr) {
                _arrangementNr = value;
                Invalidate_HeadSize();
                CurrentArrangement?.Invalidate_DrawWithOfAllItems();
                Invalidate();
                OnViewChanged();
                CursorPos_Set(_cursorPosColumn, _cursorPosRow, true);
            }
        }
    }

    public SQL_ColumnViewCollection? CurrentArrangement => _database?.ColumnArrangements == null || _database.ColumnArrangements.Count <= _arrangementNr
        ? null
        : _database.ColumnArrangements[_arrangementNr];

    public SQL_ColumnItem? CursorPosColumn => _cursorPosColumn;
    public SQL_RowData? CursorPosRow => _cursorPosRow;
    public SQL_Database? Database => _database;

    [DefaultValue(BlueTableAppearance.Standard)]
    public BlueTableAppearance Design {
        get => _design;
        set {
            SliderY.Visible = true;
            SliderX.Visible = Convert.ToBoolean(value == BlueTableAppearance.Standard);
            if (value == _design) { return; }
            CloseAllComponents();
            _design = value;
            if (!SliderX.Visible) {
                SliderX.Minimum = 0;
                SliderX.Maximum = 0;
                SliderX.Value = 0;
            }
            Invalidate_HeadSize();
            CurrentArrangement?.Invalidate_DrawWithOfAllItems();
            Invalidate_FilteredRows();
            OnViewChanged();
        }
    }

    public bool DropMessages { get; set; }

    public SQL_FilterCollection? Filter { get; private set; }

    [DefaultValue(1.0f)]
    public double FontScale => _database == null ? 1f : _database.GlobalScale;

    public List<SQL_RowItem?> PinnedRows { get; } = new();

    public DateTime PowerEdit {
        //private get => _SQL_Database?.PowerEdit;
        set {
            if (_database == null || _database.IsDisposed) { return; }
            _database.PowerEdit = value;
            Invalidate_SortedSQL_RowData(); // Neue Zeilen können nun erlaubt sein
        }
    }

    public override string QuickInfoText {
        get {
            var t1 = base.QuickInfoText;
            var t2 = _mouseOverText;
            //if (_MouseOverItem != null) { t2 = _MouseOverItem.QuickInfo; }
            if (string.IsNullOrEmpty(t1) && string.IsNullOrEmpty(t2)) {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(t1) && string.IsNullOrEmpty(t2)) {
                return t1 + "<br><hr><br>" + t2;
            }

            return t1 + t2; // Eins davon ist leer
        }
    }

    [DefaultValue(false)]
    public bool ShowNumber {
        get => _showNumber;
        set {
            if (value == _showNumber) { return; }
            CloseAllComponents();
            _showNumber = value;
            Invalidate();
        }
    }

    public bool ShowWaitScreen { get; set; } = true;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public SQL_RowSortDefinition SortDefinitionTemporary {
        get => _sortDefinitionTemporary;
        set {
            if (_sortDefinitionTemporary != null && value != null && _sortDefinitionTemporary.ToString() == value.ToString()) { return; }
            if (_sortDefinitionTemporary == value) { return; }
            _sortDefinitionTemporary = value;
            _Database_SortParameterChanged(this, System.EventArgs.Empty);
        }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public SQL_Database? SQL_Database => _database;

    [DefaultValue(true)]
    public bool Translate { get; set; } = true;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public SQL_RowItem Unterschiede {
        get => _unterschiede;
        set {
            //if (_Unterschiede != null && value != null && _sortDefinitionTemporary.ToString() == value.ToString()) { return; }
            if (_unterschiede == value) { return; }
            _unterschiede = value;
            Invalidate();
        }
    }

    #endregion

    #region Methods

    public static Size Cell_ContentSize(SQL_Table? SQL_Table, SQL_ColumnItem? column, SQL_RowItem? row, BlueFont? cellFont, int pix16) {
        if (column.Format == DataFormat.Verknüpfung_zu_anderer_Datenbank) {
            var (lcolumn, lrow, _) = SQL_CellCollection.LinkedCellData(column, row, false, false);
            return lcolumn != null && lrow != null ? Cell_ContentSize(SQL_Table, lcolumn, lrow, cellFont, pix16) : new Size(pix16, pix16);
        }
        var contentSize = column.Database.Cell.GetSizeOfCellContent(column, row);
        if (contentSize.Width > 0 && contentSize.Height > 0) { return contentSize; }
        if (column.MultiLine) {
            var tmp = column.Database.Cell.GetList(column, row);
            if (column.ShowMultiLineInOneLine) {
                contentSize = FormatedText_NeededSize(column, tmp.JoinWith("; "), cellFont, ShortenStyle.Replaced, pix16, column.BildTextVerhalten);
            } else {
                foreach (var thisString in tmp) {
                    var tmpSize = FormatedText_NeededSize(column, thisString, cellFont, ShortenStyle.Replaced, pix16, column.BildTextVerhalten);
                    contentSize.Width = Math.Max(tmpSize.Width, contentSize.Width);
                    contentSize.Height += Math.Max(tmpSize.Height, pix16);
                }
            }
        } else {
            var @string = column.Database.Cell.GetString(column, row);
            contentSize = FormatedText_NeededSize(column, @string, cellFont, ShortenStyle.Replaced, pix16, column.BildTextVerhalten);
        }
        contentSize.Width = Math.Max(contentSize.Width, pix16);
        contentSize.Height = Math.Max(contentSize.Height, pix16);
        if (Skin.Scale == 1 && LanguageTool.Translation == null) { column.Database.Cell.SetSizeOfCellContent(column, row, contentSize); }
        return contentSize;
    }

    public static void CopyToClipboard(SQL_ColumnItem? column, SQL_RowItem? row, bool meldung) {
        try {
            if (row != null && column.Format.CanBeCheckedByRules()) {
                var c = row.CellGetString(column);
                c = c.Replace("\r\n", "\r");
                c = c.Replace("\r", "\r\n");
                Generic.CopytoClipboard(c);
                if (meldung) { Notification.Show(LanguageTool.DoTranslate("<b>{0}</b><br>ist nun in der Zwischenablage.", true, c), ImageCode.Kopieren); }
            } else {
                if (meldung) { Notification.Show(LanguageTool.DoTranslate("Bei dieser Spalte nicht möglich."), ImageCode.Warnung); }
            }
        } catch {
            if (meldung) { Notification.Show(LanguageTool.DoTranslate("Unerwarteter Fehler beim Kopieren."), ImageCode.Warnung); }
        }
    }

    public static void Database_NeedPassword(object sender, PasswordEventArgs e) {
        if (e.Handled) { return; }
        e.Handled = true;
        e.Password = InputBox.Show("Bitte geben sie das Passwort ein,<br>um Zugriff auf diese Datenbank<br>zu erhalten:", string.Empty, VarType.Text);
    }

    public static void DoUndo(SQL_ColumnItem? column, SQL_RowItem? row) {
        if (column == null) { return; }
        if (row == null) { return; }
        if (column.Format == DataFormat.Verknüpfung_zu_anderer_Datenbank) {
            var (lcolumn, lrow, _) = SQL_CellCollection.LinkedCellData(column, row, true, false);
            if (lcolumn != null && lrow != null) { DoUndo(lcolumn, lrow); }
            return;
        }
        var cellKey = SQL_CellCollection.KeyOfCell(column, row);
        var i = UndoItems(column.Database, cellKey);
        if (i.Count < 1) {
            MessageBox.Show("Keine vorherigen Inhalte<br>(mehr) vorhanden.", ImageCode.Information, "OK");
            return;
        }
        i.Appearance = BlueListBoxAppearance.Listbox;
        var v = InputBoxListBoxStyle.Show("Vorherigen Eintrag wählen:", i, AddType.None, true);
        if (v == null || v.Count != 1) { return; }
        if (v[0] == "Cancel") { return; } // =Aktueller Eintrag angeklickt
        row.CellSet(column, v[0].Substring(5));
        //SQL_Database.Cell.Set(column, row, v[0].Substring(5), false);
        row.DoAutomatic(true, true, 5, "value changed");
    }

    /// <summary>
    /// Status des Bildes (Disabled) wird geändert. Diese Routine sollte nicht innerhalb der SQL_Table Klasse aufgerufen werden.
    /// Sie dient nur dazu, das Aussehen eines Textes wie eine Zelle zu imitieren.
    /// </summary>
    public static void Draw_FormatedText(Graphics gr, string text, SQL_ColumnItem? column, Rectangle fitInRect, Design design, States state, ShortenStyle style, BildTextVerhalten bildTextverhalten) {
        if (string.IsNullOrEmpty(text)) { return; }
        var d = Skin.DesignOf(design, state);

        Draw_CellTransparentDirect(gr, text, fitInRect, d.bFont, column, 16, style, bildTextverhalten, state);
    }

    public static Size FormatedText_NeededSize(SQL_ColumnItem? column, string originalText, BlueFont? font, ShortenStyle style, int minSize, BildTextVerhalten bildTextverhalten) {
        var (s, quickImage) = SQL_CellItem.GetDrawingData(column, originalText, style, bildTextverhalten);
        return Skin.FormatedText_NeededSize(s, quickImage, font, minSize);
    }

    //    FileDialogs.DeleteFile(delList, true);
    //    return newFiles;
    //}
    public static int TmpColumnContentWidth(SQL_Table? SQL_Table, SQL_ColumnItem? column, BlueFont? cellFont, int pix16) {
        if (column.TmpColumnContentWidth != null) { return (int)column.TmpColumnContentWidth; }
        column.TmpColumnContentWidth = 0;
        if (column.Format == DataFormat.Button) {
            // Beim Button reicht eine Abfrage mit Row null
            column.TmpColumnContentWidth = Cell_ContentSize(SQL_Table, column, null, cellFont, pix16).Width;
        } else {
            var locker = new object();

            Parallel.ForEach(column.Database.Row, thisSQL_RowItem => {
                if (thisSQL_RowItem != null && !thisSQL_RowItem.CellIsNullOrEmpty(column)) {
                    var wx = Cell_ContentSize(SQL_Table, column, thisSQL_RowItem, cellFont, pix16).Width;

                    lock (locker) {
                        var t = column.TmpColumnContentWidth; // ja, dank Multithreading kann es sein, dass hier das hier null ist
                        if (t == null) { t = 0; }
                        column.TmpColumnContentWidth = Math.Max((int)t, wx);
                    }
                }
            });

            //foreach (var ThisSQL_RowItem in Column.Database.Row) {
            //    if (ThisSQL_RowItem != null && !ThisSQL_RowItem.CellIsNullOrEmpty(Column)) {
            //        var t = Column.TMP_ColumnContentWidth; // ja, dank Multithreading kann es sein, dass hier das hier null ist
            //        if (t == null) { t = 0; }
            //        Column.TMP_ColumnContentWidth = Math.Max((int)t, Cell_ContentSize(SQL_Table, Column, ThisSQL_RowItem, CellFont, Pix16).Width);
            //    }
            //}
        }
        return column.TmpColumnContentWidth is int w ? w : 0;
    }

    public static ItemCollectionList UndoItems(SQL_Database SQL_Database, string cellkey) {
        ItemCollectionList i = new(BlueListBoxAppearance.KontextMenu) {
            CheckBehavior = CheckBehavior.AlwaysSingleSelection
        };
        //if (SQL_Database.Works == null || SQL_Database.Works.Count == 0) { return i; }
        //var isfirst = true;
        //TextListItem? las = null;
        //var lasNr = -1;
        //var co = 0;
        //for (var z = SQL_Database.Works.Count - 1; z >= 0; z--) {
        //    if (SQL_Database.Works[z].CellKey == cellkey && SQL_Database.Works[z].HistorischRelevant) {
        //        co++;
        //        lasNr = z;
        //        las = isfirst
        //            ? new TextListItem("Aktueller Text - ab " + SQL_Database.Works[z].Date + " UTC, geändert von " + SQL_Database.Works[z].User, "Cancel", null, false, true, string.Empty)
        //            : new TextListItem("ab " + SQL_Database.Works[z].Date + " UTC, geändert von " + SQL_Database.Works[z].User, co.ToString(Constants.Format_Integer5) + SQL_Database.Works[z].ChangedTo, null, false, true, string.Empty);
        //        isfirst = false;
        //        if (las != null) { i.Add(las); }
        //    }
        //}
        //if (las != null) {
        //    co++;
        //    i.Add("vor " + SQL_Database.Works[lasNr].Date + " UTC", co.ToString(Constants.Format_Integer5) + SQL_Database.Works[lasNr].PreviousValue);
        //}
        return i;
    }

    public static void WriteColumnArrangementsInto(ComboBox? columnArrangementSelector, SQL_Database? SQL_Database, int showingNo) {
        //if (InvokeRequired) {
        //    Invoke(new Action(() => WriteColumnArrangementsInto(columnArrangementSelector)));
        //    return;
        //}
        if (columnArrangementSelector != null) {
            columnArrangementSelector.Item.Clear();
            columnArrangementSelector.Enabled = false;
            columnArrangementSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        }

        if (SQL_Database == null || columnArrangementSelector == null) {
            if (columnArrangementSelector != null) {
                columnArrangementSelector.Enabled = false;
                columnArrangementSelector.Text = string.Empty;
            }
            return;
        }

        foreach (var thisArrangement in SQL_Database.ColumnArrangements) {
            if (thisArrangement != null) {
                if (columnArrangementSelector != null && SQL_Database.PermissionCheck(thisArrangement.PermissionGroups_Show, null)) {
                    columnArrangementSelector.Item.Add(thisArrangement.Name, SQL_Database.ColumnArrangements.IndexOf(thisArrangement).ToString());
                }
            }
        }

        if (columnArrangementSelector != null) {
            columnArrangementSelector.Enabled = Convert.ToBoolean(columnArrangementSelector.Item.Count > 1);
            columnArrangementSelector.Text = columnArrangementSelector.Item.Count > 0 ? showingNo.ToString() : string.Empty;
        }
    }

    public void CheckView() {
        if (Filter == null) {
            Filter = new SQL_FilterCollection(_database, string.Empty);
            Filter.Changed += Filter_Changed;
            OnFilterChanged();
        }

        if (_arrangementNr != 1) {
            if (_database?.ColumnArrangements == null || _arrangementNr >= _database.ColumnArrangements.Count || CurrentArrangement == null || !_database.PermissionCheck(CurrentArrangement.PermissionGroups_Show, null)) {
                _arrangementNr = 1;
            }
        }

        if (_mouseOverColumn != null && _mouseOverColumn.Database != _database) { _mouseOverColumn = null; }
        if (_mouseOverRow != null && _mouseOverRow.Row != null && _mouseOverRow.Row.Database != _database) { _mouseOverRow = null; }
        if (_cursorPosColumn != null && _cursorPosColumn.Database != _database) { _cursorPosColumn = null; }
        if (_cursorPosRow != null && _cursorPosRow.Row != null && _cursorPosRow.Row.Database != _database) { _cursorPosRow = null; }
    }

    public void CollapesAll() {
        _collapsed.Clear();
        if (SQL_Database != null) {
            _collapsed.AddRange(SQL_Database.Column.SysChapter.Contents());
        }
        Invalidate_SortedSQL_RowData();
    }

    public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e) => false;

    public void CursorPos_Reset() => CursorPos_Set(null, null, false);

    public void CursorPos_Set(SQL_ColumnItem? column, SQL_RowData? row, bool ensureVisible) {
        if (_database == null || _database.ColumnArrangements.Count == 0 || CurrentArrangement[column] == null || SortedRows() == null || !SortedRows().Contains(row)) {
            column = null;
            row = null;
        }

        var SameRow = _cursorPosRow == row;

        if (_cursorPosColumn == column && _cursorPosRow == row) { return; }
        _mouseOverText = string.Empty;
        _cursorPosColumn = column;
        _cursorPosRow = row;
        if (ensureVisible) { EnsureVisible(_cursorPosColumn, _cursorPosRow); }
        Invalidate();

        OnSelectedCellChanged(new SQL_CellExtEventArgs(_cursorPosColumn, _cursorPosRow));

        if (!SameRow) { OnSelectedRowChanged(new SQL_RowEventArgs(_cursorPosRow?.Row)); }
    }

    public void DatabaseSet(SQL_Database? value, string viewCode) {
        if (_database == value && string.IsNullOrEmpty(viewCode)) { return; }

        CloseAllComponents();

        if (_database != null) {
            // auch Disposed Datenbanken die Bezüge entfernen!
            //_SQL_Database.Cell.CellValueChanged -= _SQL_Database_CellValueChanged;
            //_SQL_Database.ConnectedControlsStopAllWorking -= _SQL_Database_StopAllWorking;
            _database.Loaded -= _Database_DatabaseLoaded;
            _database.Loading -= _Database_StoreView;
            _database.ViewChanged -= _Database_ViewChanged;
            //_SQL_Database.RowKeyChanged -= _SQL_Database_RowKeyChanged;
            //_SQL_Database.ColumnKeyChanged -= _SQL_Database_ColumnKeyChanged;
            _database.Column.ItemInternalChanged -= _SQL_Database_ColumnContentChanged;
            _database.SortParameterChanged -= _Database_SortParameterChanged;
            _database.Row.RowRemoving -= Row_RowRemoving;
            _database.Row.RowRemoved -= _Database_RowRemoved;
            //_SQL_Database.Row.RowAdded -= _SQL_Database_Row_RowAdded;
            _database.Column.ItemRemoved -= _Database_ViewChanged;
            _database.Column.ItemAdded -= _Database_ViewChanged;
            //_SQL_Database.SavedToDisk -= _SQL_Database_SavedToDisk;
            _database.ColumnArrangements.ItemInternalChanged -= ColumnArrangements_ItemInternalChanged;
            _database.ProgressbarInfo -= _Database_ProgressbarInfo;
            _database.DropMessage -= _Database_DropMessage;
            _database.Disposing -= _Database_Disposing;
            BlueBasics.MultiUserFile.MultiUserFile.ForceLoadSaveAll();
            //_SQL_Database.Save(false);         // Datenbank nicht reseten, weil sie ja anderweitig noch benutzt werden kann
        }
        ShowWaitScreen = true;
        Refresh(); // um die Uhr anzuzeigen
        _database = value;
        InitializeSkin(); // Neue Schriftgrößen
        if (_database != null) {
            //_SQL_Database.Cell.CellValueChanged += _SQL_Database_CellValueChanged;
            //_SQL_Database.ConnectedControlsStopAllWorking += _SQL_Database_StopAllWorking;
            _database.Loaded += _Database_DatabaseLoaded;
            _database.Loading += _Database_StoreView;
            _database.ViewChanged += _Database_ViewChanged;
            //_SQL_Database.RowKeyChanged += _SQL_Database_RowKeyChanged;
            //_SQL_Database.ColumnKeyChanged += _SQL_Database_ColumnKeyChanged;
            _database.Column.ItemInternalChanged += _SQL_Database_ColumnContentChanged;
            _database.SortParameterChanged += _Database_SortParameterChanged;
            _database.Row.RowRemoving += Row_RowRemoving;
            _database.Row.RowRemoved += _Database_RowRemoved;
            //_SQL_Database.Row.RowAdded += _SQL_Database_Row_RowAdded;
            _database.Column.ItemAdded += _Database_ViewChanged;
            _database.Column.ItemRemoving += Column_ItemRemoving;
            _database.Column.ItemRemoved += _Database_ViewChanged;
            //_SQL_Database.SavedToDisk += _SQL_Database_SavedToDisk;
            _database.ColumnArrangements.ItemInternalChanged += ColumnArrangements_ItemInternalChanged;
            _database.ProgressbarInfo += _Database_ProgressbarInfo;
            _database.DropMessage += _Database_DropMessage;
            _database.Disposing += _Database_Disposing;
        }

        ParseView(viewCode);
        //ResetView();
        //CheckView();

        ShowWaitScreen = false;
        OnSQL_DatabaseChanged();
    }

    public void Draw_Column_Head(Graphics gr, SQL_ColumnViewItem viewItem, Rectangle displayRectangleWoSlider, int lfdNo) {
        if (!IsOnScreen(viewItem, displayRectangleWoSlider)) { return; }
        if (_design == BlueTableAppearance.OnlyMainColumnWithoutHead) { return; }
        if (_columnFont == null) { return; }
        gr.FillRectangle(new SolidBrush(viewItem.Column.BackColor), (int)viewItem.OrderTmpSpalteX1, 0, Column_DrawWidth(viewItem, displayRectangleWoSlider), HeadSize());
        Draw_Border(gr, viewItem, displayRectangleWoSlider, true);
        gr.FillRectangle(new SolidBrush(Color.FromArgb(100, 200, 200, 200)), (int)viewItem.OrderTmpSpalteX1, 0, Column_DrawWidth(viewItem, displayRectangleWoSlider), HeadSize());

        var down = 0;
        if (!string.IsNullOrEmpty(viewItem.Column.Ueberschrift3)) {
            down = ColumnCaptionSizeY * 3;
        } else if (!string.IsNullOrEmpty(viewItem.Column.Ueberschrift2)) {
            down = ColumnCaptionSizeY * 2;
        } else if (!string.IsNullOrEmpty(viewItem.Column.Ueberschrift1)) {
            down = ColumnCaptionSizeY;
        }

        #region Recude-Button zeichnen

        if (Column_DrawWidth(viewItem, displayRectangleWoSlider) > 70 || viewItem.TmpReduced) {
            viewItem.TmpReduceLocation = new Rectangle((int)viewItem.OrderTmpSpalteX1 + Column_DrawWidth(viewItem, displayRectangleWoSlider) - 18, down, 18, 18);
            if (viewItem.TmpReduced) {
                gr.DrawImage(QuickImage.Get("Pfeil_Rechts|16|||FF0000|||||20"), viewItem.TmpReduceLocation.Left + 2, viewItem.TmpReduceLocation.Top + 2);
            } else {
                gr.DrawImage(QuickImage.Get("Pfeil_Links|16||||||||75"), viewItem.TmpReduceLocation.Left + 2, viewItem.TmpReduceLocation.Top + 2);
            }
        }

        #endregion

        #region Filter-Knopf mit Trichter

        var trichterText = string.Empty;
        QuickImage? trichterIcon = null;
        var trichterState = States.Undefiniert;
        viewItem.TmpAutoFilterLocation = new Rectangle((int)viewItem.OrderTmpSpalteX1 + Column_DrawWidth(viewItem, displayRectangleWoSlider) - AutoFilterSize, HeadSize() - AutoFilterSize, AutoFilterSize, AutoFilterSize);
        SQL_FilterItem? filtIt = null;
        if (Filter != null) { filtIt = Filter[viewItem.Column]; }

        if (viewItem.Column.AutoFilterSymbolPossible()) {
            if (filtIt != null) {
                trichterState = States.Checked;
                var anz = Autofilter_Text(viewItem.Column);
                trichterText = anz > -100 ? (anz * -1).ToString() : "∞";
            } else {
                trichterState = Autofilter_Sinnvoll(viewItem.Column) ? States.Standard : States.Standard_Disabled;
            }
        }
        var trichterSize = (AutoFilterSize - 4).ToString();
        if (filtIt != null) {
            trichterIcon = QuickImage.Get("Trichter|" + trichterSize + "|||FF0000");
        } else if (Filter != null && Filter.MayHasRowFilter(viewItem.Column)) {
            trichterIcon = QuickImage.Get("Trichter|" + trichterSize + "|||227722");
        } else if (viewItem.Column.AutoFilterSymbolPossible()) {
            trichterIcon = Autofilter_Sinnvoll(viewItem.Column)
                ? QuickImage.Get("Trichter|" + trichterSize)
                : QuickImage.Get("Trichter|" + trichterSize + "||128");
        }
        if (trichterState != States.Undefiniert) {
            Skin.Draw_Back(gr, Enums.Design.Button_AutoFilter, trichterState, viewItem.TmpAutoFilterLocation, null, false);
            Skin.Draw_Border(gr, Enums.Design.Button_AutoFilter, trichterState, viewItem.TmpAutoFilterLocation);
        }
        if (trichterIcon != null) {
            gr.DrawImage(trichterIcon, viewItem.TmpAutoFilterLocation.Left + 2, viewItem.TmpAutoFilterLocation.Top + 2);
        }
        if (!string.IsNullOrEmpty(trichterText)) {
            var s = _columnFilterFont.MeasureString(trichterText, StringFormat.GenericDefault);

            _columnFilterFont.DrawString(gr, trichterText,
                viewItem.TmpAutoFilterLocation.Left + ((AutoFilterSize - s.Width) / 2),
                viewItem.TmpAutoFilterLocation.Top + ((AutoFilterSize - s.Height) / 2));
        }
        if (trichterState == States.Undefiniert) {
            viewItem.TmpAutoFilterLocation = new Rectangle(0, 0, 0, 0);
        }

        #endregion Filter-Knopf mit Trichter

        #region LaufendeNummer

        if (_showNumber) {
            for (var x = -1; x < 2; x++) {
                for (var y = -1; y < 2; y++) {
                    BlueFont.DrawString(gr, "#" + lfdNo, _columnFont.Font(), Brushes.Black, (int)viewItem.OrderTmpSpalteX1 + x, viewItem.TmpAutoFilterLocation.Top + y);
                }
            }
            BlueFont.DrawString(gr, "#" + lfdNo, _columnFont.Font(), Brushes.White, (int)viewItem.OrderTmpSpalteX1, viewItem.TmpAutoFilterLocation.Top);
        }

        #endregion LaufendeNummer

        var tx = viewItem.Column.Caption;
        tx = LanguageTool.DoTranslate(tx, Translate).Replace("\r", "\r\n");
        var fs = gr.MeasureString(tx, _columnFont.Font());

        #region Spalten-Kopf-Bild erzeugen

        if (!string.IsNullOrEmpty(viewItem.Column.CaptionBitmap) && viewItem.Column.TmpCaptionBitmap == null) {
            viewItem.Column.TmpCaptionBitmap = QuickImage.Get(viewItem.Column.CaptionBitmap + "|100");
        }

        #endregion

        if (viewItem.Column.TmpCaptionBitmap != null && !viewItem.Column.TmpCaptionBitmap.IsError) {

            #region Spalte mit Bild zeichnen

            Point pos = new(
                (int)viewItem.OrderTmpSpalteX1 +
                (int)((Column_DrawWidth(viewItem, displayRectangleWoSlider) - fs.Width) / 2.0), 3 + down);
            gr.DrawImageInRectAspectRatio(viewItem.Column.TmpCaptionBitmap, (int)viewItem.OrderTmpSpalteX1 + 2, (int)(pos.Y + fs.Height), Column_DrawWidth(viewItem, displayRectangleWoSlider) - 4, HeadSize() - (int)(pos.Y + fs.Height) - 6 - 18);
            // Dann der Text
            gr.TranslateTransform(pos.X, pos.Y);
            //GR.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            BlueFont.DrawString(gr, tx, _columnFont.Font(), new SolidBrush(viewItem.Column.ForeColor), 0, 0);
            gr.TranslateTransform(-pos.X, -pos.Y);

            #endregion
        } else {

            #region Spalte ohne Bild zeichnen

            Point pos = new(
                (int)viewItem.OrderTmpSpalteX1 +
                (int)((Column_DrawWidth(viewItem, displayRectangleWoSlider) - fs.Height) / 2.0),
                HeadSize() - 4 - AutoFilterSize);
            gr.TranslateTransform(pos.X, pos.Y);
            gr.RotateTransform(-90);
            //GR.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            BlueFont.DrawString(gr, tx, _columnFont.Font(), new SolidBrush(viewItem.Column.ForeColor), 0, 0);
            gr.TranslateTransform(-pos.X, -pos.Y);
            gr.ResetTransform();

            #endregion
        }

        #region Sortierrichtung Zeichnen

        var tmpSortDefinition = SortUsed();
        if ((tmpSortDefinition != null && tmpSortDefinition.UsedForRowSort(viewItem.Column)) || viewItem.Column == SQL_Database.Column.SysChapter) {
            if (tmpSortDefinition.Reverse) {
                gr.DrawImage(QuickImage.Get("ZA|11|5||||50"), (float)(viewItem.OrderTmpSpalteX1 + (Column_DrawWidth(viewItem, displayRectangleWoSlider) / 2.0) - 6), HeadSize() - 6 - AutoFilterSize);
            } else {
                gr.DrawImage(QuickImage.Get("AZ|11|5||||50"), (float)(viewItem.OrderTmpSpalteX1 + (Column_DrawWidth(viewItem, displayRectangleWoSlider) / 2.0) - 6), HeadSize() - 6 - AutoFilterSize);
            }
        }

        #endregion
    }

    public bool EnsureVisible(SQL_ColumnItem? column, SQL_RowData? row) {
        var ok1 = EnsureVisible(CurrentArrangement?[column]);
        var ok2 = EnsureVisible(row);
        return ok1 && ok2;
    }

    public void ExpandAll() {
        _collapsed.Clear();
        CursorPos_Reset(); // Wenn eine Zeile markiert ist, man scrollt und expandiert, springt der Screen zurück, was sehr irriteiert
        Invalidate_SortedSQL_RowData();
    }

    public string Export_CSV(FirstRow firstRow) => _database == null ? string.Empty : _database.Export_CSV(firstRow, CurrentArrangement, SortedRows());

    public string Export_CSV(FirstRow firstRow, SQL_ColumnItem onlyColumn) {
        if (_database == null) { return string.Empty; }
        List<SQL_ColumnItem> l = new() { onlyColumn };
        return _database.Export_CSV(firstRow, l, SortedRows());
    }

    public void Export_HTML(string filename = "", bool execute = true) {
        if (_database == null) { return; }
        if (string.IsNullOrEmpty(filename)) { filename = TempFile("", "", "html"); }
        _database.Export_HTML(filename, CurrentArrangement, SortedRows(), execute);
    }

    /// <summary>
    /// Alle gefilteren Zeilen. Jede Zeile ist maximal einmal in dieser Liste vorhanden. Angepinnte Zeilen addiert worden
    /// </summary>
    /// <returns></returns>
    public List<SQL_RowItem> FilteredRows() {
        if (_filteredRows != null) { return _filteredRows; }
        _filteredRows = SQL_Database.Row.CalculateFilteredRows(Filter);
        return _filteredRows;
    }

    public new void Focus() {
        if (Focused()) { return; }
        base.Focus();
    }

    public new bool Focused() => base.Focused || SliderY.Focused() || SliderX.Focused() || BTB.Focused || BCB.Focused;

    public void GetContextMenuItems(System.Windows.Forms.MouseEventArgs? e, ItemCollectionList? items, out object? hotItem, List<string> tags, ref bool cancel, ref bool translate) {
        hotItem = null;
        if (_database.IsLoading) {
            cancel = true;
            return;
        }
        BlueBasics.MultiUserFile.MultiUserFile.ForceLoadSaveAll();

        //SQL_Database?.Load_Reload();
        if (e == null) {
            cancel = true;
            return;
        }
        CellOnCoordinate(e.X, e.Y, out _mouseOverColumn, out _mouseOverRow);
        tags.TagSet("CellKey", SQL_CellCollection.KeyOfCell(_mouseOverColumn, _mouseOverRow?.Row));
        if (_mouseOverColumn != null) {
            tags.TagSet("ColumnKey", _mouseOverColumn.Key.ToString());
        }
        if (_mouseOverRow?.Row != null) {
            tags.TagSet("RowKey", _mouseOverRow.Row.Key.ToString());
        }
    }

    //public void ImportClipboard() {
    //    Develop.DebugPrint_InvokeRequired(InvokeRequired, false);
    //    if (!System.Windows.Forms.Clipboard.ContainsText()) {
    //        Notification.Show("Abbruch,<br>kein Text im Zwischenspeicher!", ImageCode.Information);
    //        return;
    //    }

    //    var nt = System.Windows.Forms.Clipboard.GetText();
    //    ImportCsv(nt);
    //}

    //public void ImportCsv(string csvtxt) => ImportCsv(Database, csvtxt);

    public void Invalidate_AllColumnArrangements() {
        if (_database == null) { return; }

        foreach (var thisArrangement in _database.ColumnArrangements) {
            thisArrangement?.Invalidate_DrawWithOfAllItems();
        }
    }

    public void Invalidate_HeadSize() {
        if (_headSize != null) { Invalidate(); }
        _headSize = null;
    }

    public bool Mouse_IsInHead() => Convert.ToBoolean(MousePos().Y <= HeadSize());

    public void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);

    public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) => ContextMenuItemClicked?.Invoke(this, e);

    //public void OpenSearchAndReplace() {
    //    if (!SQL_Database.IsAdministrator()) { return; }
    //    if (SQL_Database.ReadOnly) { return; }
    //    if (_searchAndReplace == null || _searchAndReplace.IsDisposed || !_searchAndReplace.Visible) {
    //        _searchAndReplace = new SearchAndReplace(this);
    //        _searchAndReplace.Show();
    //    }
    //}

    public void Pin(List<SQL_RowItem>? rows) {
        // Arbeitet mit Rows, weil nur eine Anpinngug möglich ist

        if (rows == null) { rows = new List<SQL_RowItem>(); }

        rows = rows.Distinct().ToList();
        if (!rows.IsDifferentTo(PinnedRows)) { return; }

        PinnedRows.Clear();
        PinnedRows.AddRange(rows);
        Invalidate_SortedSQL_RowData();
        OnPinnedChanged();
    }

    public void PinAdd(SQL_RowItem? row) {
        PinnedRows.Add(row);
        Invalidate_SortedSQL_RowData();
        OnPinnedChanged();
    }

    public void PinRemove(SQL_RowItem? row) {
        PinnedRows.Remove(row);
        Invalidate_SortedSQL_RowData();
        OnPinnedChanged();
    }

    public void ResetView() {
        if (Filter != null) {
            Filter.Changed -= Filter_Changed;
            Filter = null;
            OnFilterChanged();
        }
        PinnedRows.Clear();
        _collapsed.Clear();

        _mouseOverText = string.Empty;
        _sortDefinitionTemporary = null;
        _mouseOverColumn = null;
        _mouseOverRow = null;
        _cursorPosColumn = null;
        _cursorPosRow = null;
        _arrangementNr = 1;
        _unterschiede = null;

        Invalidate_AllColumnArrangements();
        Invalidate_HeadSize();
        Invalidate_FilteredRows();
        OnViewChanged();
        Invalidate();
    }

    public List<SQL_RowData> SortedRows() {
        if (_sortedSQL_RowData != null) { return _sortedSQL_RowData; }

        try {
            var displayR = DisplayRectangleWithoutSlider();
            var maxY = 0;
            if (UserEdit_NewRowAllowed()) { maxY += _pix18; }
            var expanded = true;
            var lastCap = string.Empty;

            var sortedSQL_RowDataNew = SQL_Database == null
                ? new List<SQL_RowData?>()
                : SQL_Database.Row.CalculateSortedRows(FilteredRows(), SortUsed(), PinnedRows, _sortedSQL_RowData);
            if (!_sortedSQL_RowData.IsDifferentTo(sortedSQL_RowDataNew)) { return _sortedSQL_RowData; }

            _sortedSQL_RowData = new List<SQL_RowData>();

            foreach (var thisRow in sortedSQL_RowDataNew) {
                var thisSQL_RowData = thisRow;
                if (_mouseOverRow != null && _mouseOverRow.Row == thisRow.Row && _mouseOverRow.Chapter == thisRow.Chapter) {
                    _mouseOverRow.GetDataFrom(thisSQL_RowData);
                    thisSQL_RowData = _mouseOverRow;
                } // Mouse-Daten wiederverwenden
                if (_cursorPosRow != null && _cursorPosRow.Row == thisRow.Row && _cursorPosRow.Chapter == thisRow.Chapter) {
                    _cursorPosRow.GetDataFrom(thisSQL_RowData);
                    thisSQL_RowData = _cursorPosRow;
                } // Cursor-Daten wiederverwenden

                thisSQL_RowData.Y = maxY;

                #region Caption bestimmen

                if (thisRow.Chapter != lastCap) {
                    thisSQL_RowData.Y += RowCaptionSizeY;
                    expanded = !_collapsed.Contains(thisSQL_RowData.Chapter);
                    maxY += RowCaptionSizeY;
                    thisSQL_RowData.ShowCap = true;
                    lastCap = thisRow.Chapter;
                } else {
                    thisSQL_RowData.ShowCap = false;
                }

                #endregion

                #region Expaned (oder pinned) bestimmen

                thisSQL_RowData.Expanded = expanded;
                if (thisSQL_RowData.Expanded) {
                    thisSQL_RowData.DrawHeight = Row_DrawHeight(thisSQL_RowData.Row, displayR);
                    if (_sortedSQL_RowData == null) {
                        // Folgender Fall: Die Row height reparirt die LinkedCell.
                        // Dadruch wird eine Zelle geändert. Das wiederrum kann _SortedSQL_RowData auf null setzen..
                        return null;
                    }
                    maxY += thisSQL_RowData.DrawHeight;
                }

                #endregion

                _sortedSQL_RowData.Add(thisSQL_RowData);
            }

            if (_cursorPosRow?.Row != null && !_sortedSQL_RowData.Contains(_cursorPosRow)) { CursorPos_Reset(); }
            _mouseOverRow = null;

            #region Slider berechnen

            SliderSchaltenSenk(displayR, maxY);

            #endregion

            EnsureVisible(_cursorPosColumn, _cursorPosRow);
            OnVisibleRowsChanged();

            return SortedRows(); // Rekursiver aufruf. Manchmal funktiniert OnRowsSorted nicht...
        } catch {
            // Komisch, manchmal wird die Variable _SortedSQL_RowData verworfen.
            Invalidate_SortedSQL_RowData();
            return SortedRows();
        }
    }

    public SQL_RowData? View_NextRow(SQL_RowData? row) {
        if (_database == null) { return null; }
        var rowNr = SortedRows().IndexOf(row);
        return rowNr < 0 || rowNr >= SortedRows().Count - 1 ? null : SortedRows()[rowNr + 1];
    }

    public SQL_RowData? View_PreviousRow(SQL_RowData? row) {
        if (_database == null) { return null; }
        var rowNr = SortedRows().IndexOf(row);
        return rowNr < 1 ? null : SortedRows()[rowNr - 1];
    }

    public SQL_RowData? View_RowFirst() {
        if (_database == null) { return null; }
        var s = SortedRows();
        return s == null || s.Count == 0 ? null : SortedRows()[0];
    }

    public SQL_RowData? View_RowLast() => _database == null ? null : SortedRows().Count == 0 ? null : SortedRows()[SortedRows().Count - 1];

    public string ViewToString() {
        var x = "{";
        //   x = x & "<Filename>" & _SQL_Database.Filename
        x = x + "ArrangementNr=" + _arrangementNr;

        if (Filter != null) {
            var tmp = Filter.ToString();
            if (tmp.Length > 2) {
                x = x + ", Filters=" + Filter;
            }
        }

        x = x + ", SliderX=" + SliderX.Value;
        x = x + ", SliderY=" + SliderY.Value;
        if (PinnedRows != null && PinnedRows.Count > 0) {
            foreach (var thisRow in PinnedRows) {
                x = x + ", Pin=" + thisRow.Key;
            }
        }
        if (_collapsed != null && _collapsed.Count > 0) {
            foreach (var thiss in _collapsed) {
                x = x + ", Collapsed=" + thiss.ToNonCritical();
            }
        }
        if (CurrentArrangement != null) {
            foreach (var thiscol in CurrentArrangement) {
                if (thiscol.TmpReduced) { x = x + ", Reduced=" + thiscol.Column.Key; }
            }
        }

        if (_sortDefinitionTemporary?.Columns != null) {
            x = x + ", TempSort=" + _sortDefinitionTemporary;
        }
        x = x + ", CursorPos=" + SQL_CellCollection.KeyOfCell(_cursorPosColumn, _cursorPosRow?.Row);
        return x + "}";
    }

    public List<SQL_RowItem> VisibleUniqueRows() {
        var l = new List<SQL_RowItem>();
        var f = FilteredRows();
        var lockMe = new object();
        Parallel.ForEach(SQL_Database.Row, thisSQL_RowItem => {
            if (thisSQL_RowItem != null) {
                if (f.Contains(thisSQL_RowItem) || PinnedRows.Contains(thisSQL_RowItem)) {
                    lock (lockMe) { l.Add(thisSQL_RowItem); }
                }
            }
        });

        return l;
        //return SQL_Database.Row.CalculateVisibleRows(Filter, PinnedRows);
    }

    internal static void StartSQL_DatabaseService() {
        if (_serviceStarted) { return; }
        _serviceStarted = true;
        SQL_Database.AllFiles.ItemAdded += AllFiles_ItemAdded;
        SQL_Database.AllFiles.ItemRemoving += AllFiles_ItemRemoving;
        //SQL_Database.DropConstructorMessage += SQL_Database_DropConstructorMessage;
    }

    protected override void DrawControl(Graphics gr, States state) {
        if (InvokeRequired) {
            Invoke(new Action(() => DrawControl(gr, state)));
            return;
        }

        _tmpCursorRect = Rectangle.Empty;

        // Listboxen bekommen keinen Focus, also Tabellen auch nicht. Basta.
        if (Convert.ToBoolean(state & States.Standard_HasFocus)) {
            state ^= States.Standard_HasFocus;
        }

        if (_database == null || DesignMode || ShowWaitScreen) {
            DrawWaitScreen(gr);
            return;
        }

        lock (_lockUserAction) {
            //if (_InvalidExternal) { FillExternalControls(); }
            if (Convert.ToBoolean(state & States.Standard_Disabled)) { CursorPos_Reset(); }
            var displayRectangleWoSlider = DisplayRectangleWithoutSlider();
            // Haupt-Aufbau-Routine ------------------------------------
            //gr.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            if (!ComputeAllColumnPositions()) {
                DrawWaitScreen(gr);
                return;
            }
            var sr = SortedRows();

            if (sr == null) {
                // Multitasking...
                DrawWaitScreen(gr);
                return;
            }

            var firstVisibleRow = sr.Count;
            var lastVisibleRow = -1;
            foreach (var thisRow in sr) {
                if (IsOnScreen(thisRow, displayRectangleWoSlider)) {
                    var T = sr.IndexOf(thisRow);
                    firstVisibleRow = Math.Min(T, firstVisibleRow);
                    lastVisibleRow = Math.Max(T, lastVisibleRow);
                }
            }
            switch (_design) {
                case BlueTableAppearance.Standard:
                    Draw_SQL_Table_Std(gr, sr, state, displayRectangleWoSlider, firstVisibleRow, lastVisibleRow);
                    break;

                case BlueTableAppearance.OnlyMainColumnWithoutHead:
                    Draw_SQL_Table_ListboxStyle(gr, sr, state, displayRectangleWoSlider, firstVisibleRow, lastVisibleRow);
                    break;

                default:
                    Develop.DebugPrint(_design);
                    break;
            }
        }
    }

    protected void InitializeSkin() {
        _cellFont = Skin.GetBlueFont(Enums.Design.Table_Cell, States.Standard).Scale(FontScale);
        _columnFont = Skin.GetBlueFont(Enums.Design.Table_Column, States.Standard).Scale(FontScale);
        _chapterFont = Skin.GetBlueFont(Enums.Design.Table_Cell_Chapter, States.Standard).Scale(FontScale);
        _columnFilterFont = BlueFont.Get(_columnFont.FontName, _columnFont.FontSize, false, false, false, false, true, Color.White, Color.Red, false, false, false);

        _newRowFont = Skin.GetBlueFont(Enums.Design.Table_Cell_New, States.Standard).Scale(FontScale);
        if (SQL_Database != null) {
            _pix16 = GetPix(16, _cellFont, SQL_Database.GlobalScale);
            _pix18 = GetPix(18, _cellFont, SQL_Database.GlobalScale);
            _rowCaptionFontY = GetPix(26, _cellFont, SQL_Database.GlobalScale);
        } else {
            _pix16 = 16;
            _pix18 = 18;
            _rowCaptionFontY = 26;
        }
    }

    protected override bool IsInputKey(System.Windows.Forms.Keys keyData) =>
            // Ganz wichtig diese Routine!
            // Wenn diese NICHT ist, geht der Fokus weg, sobald der cursor gedrückt wird.
            keyData switch {
                System.Windows.Forms.Keys.Up or System.Windows.Forms.Keys.Down or System.Windows.Forms.Keys.Left or System.Windows.Forms.Keys.Right => true,
                _ => false
            };

    protected override void OnClick(System.EventArgs e) {
        base.OnClick(e);
        if (_database == null) { return; }
        lock (_lockUserAction) {
            if (_isinClick) { return; }
            _isinClick = true;
            //SQL_Database.OnConnectedControlsStopAllWorking(this, new MultiUserFileStopWorkingEventArgs());
            CellOnCoordinate(MousePos().X, MousePos().Y, out _mouseOverColumn, out _mouseOverRow);
            _isinClick = false;
        }
    }

    protected override void OnDoubleClick(System.EventArgs e) {
        //    base.OnDoubleClick(e); Wird komplett selbst gehandlet und das neue Ereigniss ausgelöst
        if (_database == null) { return; }
        lock (_lockUserAction) {
            if (_isinDoubleClick) { return; }
            _isinDoubleClick = true;
            CellOnCoordinate(MousePos().X, MousePos().Y, out _mouseOverColumn, out _mouseOverRow);
            //CellDoubleClickEventArgs ea = new(_mouseOverColumn, _mouseOverRow?.Row, true);
            if (Mouse_IsInHead()) {
                //SQL_Database.OnConnectedControlsStopAllWorking(this, new MultiUserFileStopWorkingEventArgs());
                //DoubleClick?.Invoke(this, ea);
            } else {
                if (_mouseOverRow == null && MousePos().Y > HeadSize() + _pix18) {
                    //SQL_Database.OnConnectedControlsStopAllWorking(this, new MultiUserFileStopWorkingEventArgs());
                    //DoubleClick?.Invoke(this, ea);
                } else {
                    //DoubleClick?.Invoke(this, ea);
                    //if (SQL_Database.PowerEdit.Subtract(DateTime.Now).TotalSeconds > 0) { ea.StartEdit = true; }
                    Cell_Edit(_mouseOverColumn, _mouseOverRow, true);
                }
            }
            _isinDoubleClick = false;
        }
    }

    protected override void OnKeyDown(System.Windows.Forms.KeyEventArgs e) {
        base.OnKeyDown(e);

        if (_database == null) { return; }

        lock (_lockUserAction) {
            if (_isinKeyDown) { return; }
            _isinKeyDown = true;

            //_Database.OnConnectedControlsStopAllWorking(this, new MultiUserFileStopWorkingEventArgs());

            switch (e.KeyCode) {
                case System.Windows.Forms.Keys.Oemcomma: // normales ,
                    if (e.Modifiers == System.Windows.Forms.Keys.Control) {
                        var lp = SQL_CellCollection.ErrorReason(_cursorPosColumn, _cursorPosRow?.Row, ErrorReason.EditGeneral);
                        Neighbour(_cursorPosColumn, _cursorPosRow, Direction.Oben, out _, out var newRow);
                        if (newRow == _cursorPosRow) { lp = "Das geht nicht bei dieser Zeile."; }
                        if (string.IsNullOrEmpty(lp) && newRow != null) {
                            UserEdited(this, newRow?.Row.CellGetString(_cursorPosColumn), _cursorPosColumn, _cursorPosRow?.Row, newRow?.Chapter, true);
                        } else {
                            NotEdiSQL_TableInfo(lp);
                        }
                    }
                    break;

                case System.Windows.Forms.Keys.X:
                    if (e.Modifiers == System.Windows.Forms.Keys.Control) {
                        CopyToClipboard(_cursorPosColumn, _cursorPosRow?.Row, true);
                        if (_cursorPosRow is null || _cursorPosRow.Row.CellIsNullOrEmpty(_cursorPosColumn)) {
                            _isinKeyDown = false;
                            return;
                        }
                        var l2 = SQL_CellCollection.ErrorReason(_cursorPosColumn, _cursorPosRow?.Row, ErrorReason.EditGeneral);
                        if (string.IsNullOrEmpty(l2)) {
                            UserEdited(this, string.Empty, _cursorPosColumn, _cursorPosRow?.Row, _cursorPosRow?.Chapter, true);
                        } else {
                            NotEdiSQL_TableInfo(l2);
                        }
                    }
                    break;

                case System.Windows.Forms.Keys.Delete:
                    if (_cursorPosRow is null || _cursorPosRow.Row.CellIsNullOrEmpty(_cursorPosColumn)) {
                        _isinKeyDown = false;
                        return;
                    }
                    var l = SQL_CellCollection.ErrorReason(_cursorPosColumn, _cursorPosRow?.Row, ErrorReason.EditGeneral);
                    if (string.IsNullOrEmpty(l)) {
                        UserEdited(this, string.Empty, _cursorPosColumn, _cursorPosRow?.Row, _cursorPosRow?.Chapter, true);
                    } else {
                        NotEdiSQL_TableInfo(l);
                    }
                    break;

                case System.Windows.Forms.Keys.Left:
                    Cursor_Move(Direction.Links);
                    break;

                case System.Windows.Forms.Keys.Right:
                    Cursor_Move(Direction.Rechts);
                    break;

                case System.Windows.Forms.Keys.Up:
                    Cursor_Move(Direction.Oben);
                    break;

                case System.Windows.Forms.Keys.Down:
                    Cursor_Move(Direction.Unten);
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
                        CopyToClipboard(_cursorPosColumn, _cursorPosRow?.Row, true);
                    }
                    break;

                //case System.Windows.Forms.Keys.F:
                //    if (e.Modifiers == System.Windows.Forms.Keys.Control) {
                //        Search x = new(this);
                //        x.Show();
                //    }
                //    break;

                case System.Windows.Forms.Keys.F2:
                    Cell_Edit(_cursorPosColumn, _cursorPosRow, true);
                    break;

                case System.Windows.Forms.Keys.V:
                    if (e.Modifiers == System.Windows.Forms.Keys.Control) {
                        if (_cursorPosColumn != null && _cursorPosRow != null) {
                            if (!_cursorPosColumn.Format.TextboxEditPossible() && _cursorPosColumn.Format != DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems) {
                                NotEdiSQL_TableInfo("Die Zelle hat kein passendes Format.");
                                _isinKeyDown = false;
                                return;
                            }
                            if (!System.Windows.Forms.Clipboard.ContainsText()) {
                                NotEdiSQL_TableInfo("Kein Text in der Zwischenablage.");
                                _isinKeyDown = false;
                                return;
                            }
                            var ntxt = System.Windows.Forms.Clipboard.GetText();
                            if (_cursorPosRow?.Row.CellGetString(_cursorPosColumn) == ntxt) {
                                _isinKeyDown = false;
                                return;
                            }
                            var l2 = SQL_CellCollection.ErrorReason(_cursorPosColumn, _cursorPosRow?.Row, ErrorReason.EditGeneral);
                            if (string.IsNullOrEmpty(l2)) {
                                UserEdited(this, ntxt, _cursorPosColumn, _cursorPosRow?.Row, _cursorPosRow?.Chapter, true);
                            } else {
                                NotEdiSQL_TableInfo(l2);
                            }
                        }
                    }
                    break;
            }
            _isinKeyDown = false;
        }
    }

    protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e) {
        base.OnMouseDown(e);
        if (_database == null) { return; }
        lock (_lockUserAction) {
            if (_isinMouseDown) { return; }
            _isinMouseDown = true;
            //_Database.OnConnectedControlsStopAllWorking(this, new MultiUserFileStopWorkingEventArgs());
            CellOnCoordinate(e.X, e.Y, out _mouseOverColumn, out _mouseOverRow);
            // Die beiden Befehle nur in Mouse Down!
            // Wenn der Cursor bei Click/Up/Down geändert wird, wird ein Ereignis ausgelöst.
            // Das könnte auch sehr Zeitintensiv sein. Dann kann die Maus inzwischen wo ander sein.
            // Somit würde das Ereignis doppelt und dreifach ausgelöste werden können.
            // Beipiel: MouseDown-> Bildchen im Pad erzeugen, dauert.... Maus Bewegt sich
            //          MouseUp  -> Cursor wird umgesetzt, Ereginis CursorChanged wieder ausgelöst, noch ein Bildchen
            if (_mouseOverRow == null) {
                var rc = RowCaptionOnCoordinate(e.X, e.Y);
                CursorPos_Reset(); // Wenn eine Zeile markiert ist, man scrollt und expandiert, springt der Screen zurück, was sehr irriteiert
                _mouseOverColumn = null;
                _mouseOverRow = null;

                if (!string.IsNullOrEmpty(rc)) {
                    if (_collapsed.Contains(rc)) {
                        _collapsed.Remove(rc);
                    } else {
                        _collapsed.Add(rc);
                    }
                    Invalidate_SortedSQL_RowData();
                }
            }
            EnsureVisible(_mouseOverColumn, _mouseOverRow);
            CursorPos_Set(_mouseOverColumn, _mouseOverRow, false);
            _isinMouseDown = false;
        }
    }

    protected override void OnMouseEnter(System.EventArgs e) {
        base.OnMouseEnter(e);
        if (_database == null) { return; }
        lock (_lockUserAction) {
            if (_isinMouseEnter) { return; }
            _isinMouseEnter = true;
            Forms.QuickInfo.Close();
            _isinMouseEnter = false;
        }
    }

    protected override void OnMouseLeave(System.EventArgs e) {
        base.OnMouseLeave(e);
        if (_database == null) { return; }
        lock (_lockUserAction) {
            if (_isinMouseLeave) { return; }
            _isinMouseLeave = true;
            Forms.QuickInfo.Close();
            _isinMouseLeave = false;
        }
    }

    protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e) {
        base.OnMouseMove(e);
        lock (_lockUserAction) {
            if (_isinMouseMove) { return; }
            _isinMouseMove = true;
            _mouseOverText = string.Empty;
            CellOnCoordinate(e.X, e.Y, out _mouseOverColumn, out _mouseOverRow);
            if (e.Button != System.Windows.Forms.MouseButtons.None) {
                //_Database?.OnConnectedControlsStopAllWorking(this, new MultiUserFileStopWorkingEventArgs());
            } else {
                if (_mouseOverColumn != null && e.Y < HeadSize()) {
                    _mouseOverText = _mouseOverColumn.QuickInfoText(string.Empty);
                } else if (_mouseOverColumn != null && _mouseOverRow != null) {
                    if (_mouseOverColumn.Format.NeedTargetDatabase()) {
                        if (_mouseOverColumn.LinkedDatabase != null) {
                            switch (_mouseOverColumn.Format) {
                                //case DataFormat.Columns_für_LinkedCellDropdown:
                                //    var txt = _mouseOverRow.Row.CellGetString(_mouseOverColumn);
                                //    if (IntTryParse(txt, out var colKey)) {
                                //        var c = _mouseOverColumn.LinkedDatabase().Column.SearchByKey(colKey);
                                //        if (c != null) { _mouseOverText = c.QuickInfoText(_mouseOverColumn.Caption + ": " + c.Caption); }
                                //    }
                                //    break;

                                //case DataFormat.Verknüpfung_zu_anderer_Datenbank_Skriptgesteuert:
                                case DataFormat.Verknüpfung_zu_anderer_Datenbank:
                                case DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems:
                                    var (lcolumn, _, info) = SQL_CellCollection.LinkedCellData(_mouseOverColumn, _mouseOverRow.Row, true, false);
                                    if (lcolumn != null) { _mouseOverText = lcolumn.QuickInfoText(_mouseOverColumn.ReadableText() + " bei " + lcolumn.ReadableText() + ":"); }

                                    if (!string.IsNullOrEmpty(info) && _mouseOverColumn.Database.IsAdministrator()) {
                                        if (string.IsNullOrEmpty(_mouseOverText)) { _mouseOverText += "\r\n"; }
                                        _mouseOverText = "Verlinkungs-Status: " + info;
                                    }

                                    break;

                                default:
                                    Develop.DebugPrint(_mouseOverColumn.Format);
                                    break;
                            }
                        } else {
                            _mouseOverText = "Verknüpfung zur Ziel-Datenbank fehlerhaft.";
                        }
                    } else if (_database.IsAdministrator()) {
                        _mouseOverText = SQL_Database.UndoText(_mouseOverColumn, _mouseOverRow.Row);
                    }
                }
                _mouseOverText = _mouseOverText.Trim();
                _mouseOverText = _mouseOverText.Trim("<br>");
                _mouseOverText = _mouseOverText.Trim();
            }
            _isinMouseMove = false;
        }
    }

    protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e) {
        base.OnMouseUp(e);
        if (_database == null) { return; }
        lock (_lockUserAction) {
            if (_isinMouseUp) { return; }
            _isinMouseUp = true;
            var screenX = System.Windows.Forms.Cursor.Position.X - e.X;
            var screenY = System.Windows.Forms.Cursor.Position.Y - e.Y;
            if (_database == null) {
                Forms.QuickInfo.Close();
                _isinMouseUp = false;
                return;
            }
            CellOnCoordinate(e.X, e.Y, out _mouseOverColumn, out _mouseOverRow);
            if (_cursorPosColumn != _mouseOverColumn || _cursorPosRow != _mouseOverRow) { Forms.QuickInfo.Close(); }
            // TXTBox_Close() NICHT! Weil sonst nach dem öffnen sofort wieder gschlossen wird
            // AutoFilter_Close() NICHT! Weil sonst nach dem öffnen sofort wieder geschlossen wird
            FloatingForm.Close(this, Enums.Design.Form_KontextMenu);
            //EnsureVisible(_MouseOver) <-Nur MouseDown, siehe Da
            //CursorPos_Set(_MouseOver) <-Nur MouseDown, siehe Da
            SQL_ColumnViewItem viewItem = null;
            if (_mouseOverColumn != null) {
                viewItem = CurrentArrangement[_mouseOverColumn];
            }
            if (e.Button == System.Windows.Forms.MouseButtons.Left) {
                if (_mouseOverColumn != null) {
                    if (Mouse_IsInAutofilter(viewItem, e)) {
                        AutoFilter_Show(viewItem, screenX, screenY);
                        _isinMouseUp = false;
                        return;
                    }
                    if (Mouse_IsInRedcueButton(viewItem, e)) {
                        viewItem.TmpReduced = !viewItem.TmpReduced;
                        viewItem.TmpDrawWidth = null;
                        Invalidate();
                        _isinMouseUp = false;
                        return;
                    }
                    if (_mouseOverRow != null && _mouseOverColumn.Format == DataFormat.Button) {
                        OnButtonCellClicked(new SQL_CellEventArgs(_mouseOverColumn, _mouseOverRow?.Row));
                        Invalidate();
                    }
                }
            }
            if (e.Button == System.Windows.Forms.MouseButtons.Right) {
                FloatingInputBoxListBoxStyle.ContextMenuShow(this, e);
            }
            _isinMouseUp = false;
        }
        //   End SyncLock
    }

    protected override void OnMouseWheel(System.Windows.Forms.MouseEventArgs e) {
        base.OnMouseWheel(e);
        if (_database == null) { return; }
        lock (_lockUserAction) {
            if (_isinMouseWheel) { return; }
            _isinMouseWheel = true;
            //SQL_Database?.OnConnectedControlsStopAllWorking(this, new MultiUserFileStopWorkingEventArgs());
            if (!SliderY.Visible) {
                _isinMouseWheel = false;
                return;
            }
            SliderY.DoMouseWheel(e);
            _isinMouseWheel = false;
        }
    }

    protected override void OnSizeChanged(System.EventArgs e) {
        base.OnSizeChanged(e);
        if (_database == null) { return; }
        lock (_lockUserAction) {
            if (_isinSizeChanged) { return; }
            _isinSizeChanged = true;
            //SQL_Database?.OnConnectedControlsStopAllWorking(this, new MultiUserFileStopWorkingEventArgs());
            CurrentArrangement?.Invalidate_DrawWithOfAllItems();
            _isinSizeChanged = false;
        }
        Invalidate_SortedSQL_RowData(); // Zellen können ihre Größe ändern. z.B. die Zeilenhöhe
    }

    //protected override void OnResize(System.EventArgs e) {
    //    base.OnResize(e);
    //    if (_SQL_Database == null) { return; }
    //    lock (Lock_UserAction) {
    //        if (ISIN_Resize) { return; }
    //        ISIN_Resize = true;
    //        SQL_Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
    //        Invalidate_AllDraw(false);
    //        ISIN_Resize = false;
    //    }
    //}
    protected override void OnVisibleChanged(System.EventArgs e) {
        base.OnVisibleChanged(e);
        if (_database == null) { return; }
        lock (_lockUserAction) {
            if (_isinVisibleChanged) { return; }
            _isinVisibleChanged = true;
            //SQL_Database.OnConnectedControlsStopAllWorking(this, new MultiUserFileStopWorkingEventArgs());
            _isinVisibleChanged = false;
        }
    }

    private static void AllFiles_ItemAdded(object sender, ListEventArgs e) {
        if (e.Item is SQL_Database db) {
            db.NeedPassword += Database_NeedPassword;
            //db.GenerateLayoutInternal += DB_GenerateLayoutInternal;
            db.Loaded += SQL_TableView.CheckDatabase;
        }
    }

    private static void AllFiles_ItemRemoving(object sender, ListEventArgs e) {
        if (e.Item is SQL_Database db) {
            db.NeedPassword -= Database_NeedPassword;
            //db.GenerateLayoutInternal -= DB_GenerateLayoutInternal;
            db.Loaded -= SQL_TableView.CheckDatabase;
        }
    }

    private static void DB_GenerateLayoutInternal(object sender, GenerateLayoutInternalEventargs e) {
        if (e.Handled) { return; }
        e.Handled = true;
        ItemCollectionPad pad = new(e.LayoutId, e.Row.Database, e.Row.Key);
        pad.SaveAsBitmap(e.Filename);
    }

    //    if (e.WrittenToLogifile) { return; }
    //    e.WrittenToLogifile = true;
    //    Develop.DebugPrint(e.Type, e.Message);
    //}
    private static void Draw_CellTransparentDirect(Graphics gr, string? toDraw, Rectangle drawarea, BlueFont? font, SQL_ColumnItem? contentHolderCellColumn, int pix16, ShortenStyle style, BildTextVerhalten bildTextverhalten, States state) {
        if (toDraw == null) { toDraw = string.Empty; }

        if (!contentHolderCellColumn.MultiLine || !toDraw.Contains("\r")) {
            Draw_CellTransparentDirect_OneLine(gr, toDraw, contentHolderCellColumn, drawarea, 0, false, font, pix16, style, bildTextverhalten, state);
        } else {
            var mei = toDraw.SplitAndCutByCr();
            if (contentHolderCellColumn.ShowMultiLineInOneLine) {
                Draw_CellTransparentDirect_OneLine(gr, mei.JoinWith("; "), contentHolderCellColumn, drawarea, 0, false, font, pix16, style, bildTextverhalten, state);
            } else {
                var y = 0;
                for (var z = 0; z <= mei.GetUpperBound(0); z++) {
                    Draw_CellTransparentDirect_OneLine(gr, mei[z], contentHolderCellColumn, drawarea, y, z != mei.GetUpperBound(0), font, pix16, style, bildTextverhalten, state);
                    y += FormatedText_NeededSize(contentHolderCellColumn, mei[z], font, style, pix16 - 1, bildTextverhalten).Height;
                }
            }
        }
    }

    //    if (e.Type is enFehlerArt.DevelopInfo or enFehlerArt.Info) { return; }
    private static void Draw_CellTransparentDirect_OneLine(Graphics gr, string drawString, SQL_ColumnItem? contentHolderColumnStyle, Rectangle drawarea, int txtYPix, bool changeToDot, BlueFont? font, int pix16, ShortenStyle style, BildTextVerhalten bildTextverhalten, States state) {
        Rectangle r = new(drawarea.Left, drawarea.Top + txtYPix, drawarea.Width, pix16);

        if (r.Bottom + pix16 > drawarea.Bottom) {
            if (r.Bottom > drawarea.Bottom) { return; }
            if (changeToDot) { drawString = "..."; }// Die letzte Zeile noch ganz hinschreiben
        }

        var tmpData = SQL_CellItem.GetDrawingData(contentHolderColumnStyle, drawString, style, bildTextverhalten);
        var tmpImageCode = tmpData.Item2;
        if (tmpImageCode != null) { tmpImageCode = QuickImage.Get(tmpImageCode, Skin.AdditionalState(state)); }

        Skin.Draw_FormatedText(gr, tmpData.Item1, tmpImageCode, (Alignment)contentHolderColumnStyle.Align, r, null, false, font, false);
    }

    //private static void SQL_Database_DropConstructorMessage(object sender, MessageEventArgs e) {
    //    if (!e.Shown) {
    //        e.Shown = true;
    //        Notification.Show(e.Message, ImageCode.Datenbank);
    //    }
    private static int GetPix(int pix, BlueFont? f, double scale) => Skin.FormatedText_NeededSize("@|", null, f, (int)((pix * scale) + 0.5)).Height;

    private static bool Mouse_IsInAutofilter(SQL_ColumnViewItem? viewItem, System.Windows.Forms.MouseEventArgs e) => viewItem != null && viewItem.TmpAutoFilterLocation.Width != 0 && viewItem.Column.AutoFilterSymbolPossible() && viewItem.TmpAutoFilterLocation.Contains(e.X, e.Y);

    private static bool Mouse_IsInRedcueButton(SQL_ColumnViewItem? viewItem, System.Windows.Forms.MouseEventArgs e) => viewItem != null && viewItem.TmpReduceLocation.Width != 0 && viewItem.TmpReduceLocation.Contains(e.X, e.Y);

    private static void NotEdiSQL_TableInfo(string reason) => Notification.Show(reason, ImageCode.Kreuz);

    private static void UserEdited(SQL_Table SQL_Table, string newValue, SQL_ColumnItem? column, SQL_RowItem? row, string chapter, bool formatWarnung) {
        if (column == null || column.Database == null || column.Database.Column.Count == 0) {
            NotEdiSQL_TableInfo("Keine Spalte angegeben.");
            return;
        }

        if (column.Format == DataFormat.Verknüpfung_zu_anderer_Datenbank) {
            var (lcolumn, lrow, info) = SQL_CellCollection.LinkedCellData(column, row, true, false);
            if (lcolumn == null || lrow == null) {
                NotEdiSQL_TableInfo("Zelle in verlinkter Datenbank nicht vorhanden:\r\n" + info);
                return;
            }
            UserEdited(SQL_Table, newValue, lcolumn, lrow, chapter, formatWarnung);
            if (SQL_Table._database == column.Database) { SQL_Table.CursorPos_Set(column, row, false, chapter); }
            return;
        }

        if (row == null && column != column.Database.Column[0]) {
            NotEdiSQL_TableInfo("Neue Zeilen müssen mit der ersten Spalte beginnen");
            return;
        }

        newValue = column.AutoCorrect(newValue);
        if (row != null) {
            if (newValue == row.CellGetString(column)) { return; }
        } else {
            if (string.IsNullOrEmpty(newValue)) { return; }
        }

        //CellValueChangingByUserEventArgs ed = new(column, row, newValue, string.Empty);
        //SQL_Table.OnCellValueChangingByUser(ed);
        var cancelReason = string.Empty;// ed.CancelReason;
        if (string.IsNullOrEmpty(cancelReason) && formatWarnung && !string.IsNullOrEmpty(newValue)) {
            if (!newValue.IsFormat(column)) {
                if (MessageBox.Show("Ihre Eingabe entspricht<br><u>nicht</u> dem erwarteten Format!<br><br>Trotzdem übernehmen?", ImageCode.Information, "Ja", "Nein") != 0) {
                    cancelReason = "Abbruch, das das erwartete Format nicht eingehalten wurde.";
                }
            }
        }

        if (string.IsNullOrEmpty(cancelReason)) {
            if (row == null) {
                var f = SQL_CellCollection.ErrorReason(column.Database.Column[0], null, ErrorReason.EditGeneral);
                if (!string.IsNullOrEmpty(f)) { NotEdiSQL_TableInfo(f); return; }
                row = column.Database.Row.Add(newValue);
                if (SQL_Table._database == column.Database) {
                    var l = SQL_Table.FilteredRows();
                    if (!l.Contains(row)) {
                        if (MessageBox.Show("Die neue Zeile ist ausgeblendet.<br>Soll sie <b>angepinnt</b> werden?", ImageCode.Pinnadel, "anpinnen", "abbrechen") == 0) {
                            SQL_Table.PinAdd(row);
                        }
                    }

                    var sr = SQL_Table.SortedRows();
                    var rd = sr.Get(row);
                    SQL_Table.CursorPos_Set(SQL_Table._database.Column[0], rd, true);
                }
            } else {
                var f = SQL_CellCollection.ErrorReason(column, row, ErrorReason.EditGeneral);
                if (!string.IsNullOrEmpty(f)) { NotEdiSQL_TableInfo(f); return; }
                row.CellSet(column, newValue);
            }
            if (SQL_Table._database == column.Database) { SQL_Table.CursorPos_Set(column, row, false, chapter); }
            row.DoAutomatic(true, false, 5, "value changed");

            // EnsureVisible ganz schlecht: Daten verändert, keine Positionen bekannt - und da soll sichtbar gemacht werden?
            // CursorPos.EnsureVisible(SliderX, SliderY, DisplayRectangle)
        } else {
            NotEdiSQL_TableInfo(cancelReason);
        }
    }

    private void _Database_DatabaseLoaded(object sender, LoadedEventArgs e) {
        // Wird auch bei einem Reload ausgeführt.
        // Es kann aber sein, dass eine Ansicht zurückgeholt wurde, und die Werte stimmen.
        // Deswegen prüfen, ob wirklich alles gelöscht werden muss, oder weiter behalten werden kann.
        // Auf Nothing muss auch geprüft werden, da bei einem Dispose oder beim Beenden sich die Datenbank auch änsdert....

        if (!string.IsNullOrEmpty(_storedView)) {
            ParseView(_storedView);
            _storedView = string.Empty;
        } else {
            ResetView();
            CheckView();
        }
    }

    //private void _SQL_Database_ColumnKeyChanged(object sender, KeyChangedEventArgs e) {
    //    // Ist aktuell nur möglich,wenn Pending Changes eine neue Zeile machen
    //    if (string.IsNullOrEmpty(_StoredView)) { return; }
    //    _StoredView = ColumnCollection.ChangeKeysInString(_StoredView, e.KeyOld, e.KeyNew);
    //}
    private void _Database_Disposing(object sender, System.EventArgs e) => DatabaseSet(null, string.Empty);

    private void _Database_DropMessage(object sender, MessageEventArgs e) {
        if (_database.IsAdministrator() && DropMessages) {
            MessageBox.Show(e.Message);
        }
    }

    private void _Database_ProgressbarInfo(object sender, ProgressbarEventArgs e) {
        if (e.Ends) {
            _pg?.Close();
            return;
        }
        if (e.Beginns) {
            _pg = Progressbar.Show(e.Name, e.Count);
            return;
        }
        _pg.Update(e.Current);
    }

    private void _Database_Row_RowAdded(object sender, SQL_RowEventArgs e) {
        OnRowAdded(sender, e);
        Invalidate_FilteredRows();
    }

    private void _Database_RowRemoved(object sender, System.EventArgs e) => Invalidate_FilteredRows();

    private void _Database_SortParameterChanged(object sender, System.EventArgs e) => Invalidate_SortedSQL_RowData();

    //private void _SQL_Database_RowKeyChanged(object sender, KeyChangedEventArgs e) {
    //    // Ist aktuell nur möglich, wenn Pending Changes eine neue Zeile machen
    //    if (string.IsNullOrEmpty(_StoredView)) { return; }
    //    _StoredView = _StoredView.Replace("RowKey=" + e.KeyOld + "}", "RowKey=" + e.KeyNew + "}");
    //}
    private void _Database_StoreView(object sender, System.EventArgs e) =>
            //if (!string.IsNullOrEmpty(_StoredView)) { Develop.DebugPrint("Stored View nicht Empty!"); }
            _storedView = ViewToString();

    private void _Database_ViewChanged(object sender, System.EventArgs e) {
        InitializeSkin(); // Sicher ist sicher, um die neuen Schrift-Größen zu haben.

        Invalidate_HeadSize();
        Invalidate_AllColumnArrangements();
        //Invalidate_RowSort();
        CursorPos_Set(_cursorPosColumn, _cursorPosRow, true);
        Invalidate();
    }

    private void _SQL_Database_CellValueChanged(object sender, SQL_CellEventArgs e) {
        if (_filteredRows != null) {
            if (Filter[e.Column] != null ||
                SortUsed() == null ||
                SortUsed().UsedForRowSort(e.Column) ||
                Filter.MayHasRowFilter(e.Column) ||
                e.Column == SQL_Database.Column.SysChapter) {
                Invalidate_FilteredRows();
            }
        }
        Invalidate_DrawWidth(e.Column);

        Invalidate_SortedSQL_RowData();
        Invalidate();
        OnCellValueChanged(e);
    }

    private void _SQL_Database_ColumnContentChanged(object sender, ListEventArgs e) {
        Invalidate_AllColumnArrangements();
        Invalidate_HeadSize();
        Invalidate_SortedSQL_RowData();
    }

    private void AutoFilter_Close() {
        if (_autoFilter != null) {
            _autoFilter.FilterComand -= AutoFilter_FilterComand;
            _autoFilter.Dispose();
            _autoFilter = null;
        }
    }

    private void AutoFilter_FilterComand(object sender, SQL_FilterComandEventArgs e) {
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
                e.Column.GetUniques(VisibleUniqueRows(), out var einzigartig, out _);
                if (einzigartig.Count > 0) {
                    Filter.Add(new SQL_FilterItem(e.Column, FilterType.Istgleich_ODER_GroßKleinEgal, einzigartig));
                    Notification.Show("Die aktuell einzigartigen Einträge wurden berechnet<br>und als <b>ODER-Filter</b> gespeichert.", ImageCode.Trichter);
                } else {
                    Notification.Show("Filterung dieser Spalte gelöscht,<br>da <b>alle Einträge</b> mehrfach vorhanden sind.", ImageCode.Trichter);
                }
                break;

            case "donichteinzigartig":
                Filter.Remove(e.Column);
                e.Column.GetUniques(VisibleUniqueRows(), out _, out var xNichtEinzigartig);
                if (xNichtEinzigartig.Count > 0) {
                    Filter.Add(new SQL_FilterItem(e.Column, FilterType.Istgleich_ODER_GroßKleinEgal, xNichtEinzigartig));
                    Notification.Show("Die aktuell <b>nicht</b> einzigartigen Einträge wurden berechnet<br>und als <b>ODER-Filter</b> gespeichert.", ImageCode.Trichter);
                } else {
                    Notification.Show("Filterung dieser Spalte gelöscht,<br>da <b>alle Einträge</b> einzigartig sind.", ImageCode.Trichter);
                }
                break;

            case "dospaltenvergleich": {
                    List<SQL_RowItem> ro = new();
                    ro.AddRange(VisibleUniqueRows());

                    ItemCollectionList ic = new();
                    foreach (var thisSQL_ColumnItem in e.Column.Database.Column) {
                        if (thisSQL_ColumnItem != null && thisSQL_ColumnItem != e.Column) { ic.Add(thisSQL_ColumnItem); }
                    }
                    ic.Sort();

                    var r = InputBoxListBoxStyle.Show("Mit welcher Spalte vergleichen?", ic, AddType.None, true);
                    if (r == null || r.Count == 0) { return; }

                    var c = e.Column.Database.Column.SearchByKey(LongParse(r[0]));

                    List<string> d = new();
                    foreach (var thisR in ro) {
                        if (thisR.CellGetString(e.Column) != thisR.CellGetString(c)) { d.Add(thisR.CellFirstString()); }
                    }
                    if (d.Count > 0) {
                        Filter.Add(new SQL_FilterItem(e.Column.Database.Column[0], FilterType.Istgleich_ODER_GroßKleinEgal, d));
                        Notification.Show("Die aktuell <b>unterschiedlichen</b> Einträge wurden berechnet<br>und als <b>ODER-Filter</b> in der <b>ersten Spalte</b> gespeichert.", ImageCode.Trichter);
                    } else {
                        Notification.Show("Keine Filter verändert,<br>da <b>alle Einträge</b> identisch sind.", ImageCode.Trichter);
                    }
                    break;
                }

            case "doclipboard": {
                    var clipTmp = System.Windows.Forms.Clipboard.GetText();
                    clipTmp = clipTmp.RemoveChars(Constants.Char_NotFromClip);
                    clipTmp = clipTmp.TrimEnd("\r\n");
                    var searchValue = new List<string>(clipTmp.SplitAndCutByCr()).SortedDistinctList();
                    Filter.Remove(e.Column);
                    if (searchValue.Count > 0) {
                        Filter.Add(new SQL_FilterItem(e.Column, FilterType.IstGleich_ODER | FilterType.GroßKleinEgal, searchValue));
                    }
                    break;
                }

            case "donotclipboard": {
                    var clipTmp = System.Windows.Forms.Clipboard.GetText();
                    clipTmp = clipTmp.RemoveChars(Constants.Char_NotFromClip);
                    clipTmp = clipTmp.TrimEnd("\r\n");
                    var columInhalt = SQL_Database.Export_CSV(FirstRow.Without, e.Column, null).SplitAndCutByCrToList().SortedDistinctList();
                    columInhalt.RemoveString(clipTmp.SplitAndCutByCrToList().SortedDistinctList(), false);
                    Filter.Remove(e.Column);
                    if (columInhalt.Count > 0) {
                        Filter.Add(new SQL_FilterItem(e.Column, FilterType.IstGleich_ODER | FilterType.GroßKleinEgal, columInhalt));
                    }
                    break;
                }
            default:
                Develop.DebugPrint("Unbekannter Comand:   " + e.Comand);
                break;
        }
        OnAutoFilterClicked(new SQL_FilterEventArgs(e.Filter));
    }

    private void AutoFilter_Show(SQL_ColumnViewItem? SQL_ColumnViewItem, int screenx, int screeny) {
        if (SQL_ColumnViewItem == null) { return; }
        if (_design == BlueTableAppearance.OnlyMainColumnWithoutHead) { return; }
        if (!SQL_ColumnViewItem.Column.AutoFilterSymbolPossible()) { return; }
        if (Filter.Any(thisFilter => thisFilter != null && thisFilter.Column == SQL_ColumnViewItem.Column && !string.IsNullOrEmpty(thisFilter.Herkunft))) {
            MessageBox.Show("Dieser Filter wurde<br>automatisch gesetzt.", ImageCode.Information, "OK");
            return;
        }
        //SQL_Database.OnConnectedControlsStopAllWorking(this, new MultiUserFileStopWorkingEventArgs());
        //OnBeforeAutoFilterShow(new ColumnEventArgs(SQL_ColumnViewItem.Column));
        _autoFilter = new SQL_AutoFilter(SQL_ColumnViewItem.Column, Filter, PinnedRows);
        _autoFilter.Position_LocateToPosition(new Point(screenx + (int)SQL_ColumnViewItem.OrderTmpSpalteX1, screeny + HeadSize()));
        _autoFilter.Show();
        _autoFilter.FilterComand += AutoFilter_FilterComand;
        Develop.Debugprint_BackgroundThread();
    }

    private bool Autofilter_Sinnvoll(SQL_ColumnItem? column) {
        if (column.TmpAutoFilterSinnvoll != null) { return (bool)column.TmpAutoFilterSinnvoll; }
        for (var rowcount = 0; rowcount <= SortedRows().Count - 2; rowcount++) {
            if (SortedRows()[rowcount]?.Row.CellGetString(column) != SortedRows()[rowcount + 1]?.Row.CellGetString(column)) {
                column.TmpAutoFilterSinnvoll = true;
                return true;
            }
        }
        column.TmpAutoFilterSinnvoll = false;
        return false;
    }

    /// <summary>
    /// Gibt die Anzahl der SICHTBAREN Zeilen zurück, die mehr angezeigt werden würden, wenn dieser Filter deaktiviert wäre.
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    private int Autofilter_Text(SQL_ColumnItem? column) {
        if (column.TmpIfFilterRemoved != null) { return (int)column.TmpIfFilterRemoved; }
        var tfilter = new SQL_FilterCollection(column.Database);
        foreach (var thisFilter in Filter) {
            if (thisFilter != null && thisFilter.Column != column) { tfilter.Add(thisFilter); }
        }
        var temp = SQL_Database.Row.CalculateFilteredRows(tfilter);
        column.TmpIfFilterRemoved = FilteredRows().Count - temp.Count;
        return (int)column.TmpIfFilterRemoved;
    }

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

    private void BB_LostFocus(object sender, System.EventArgs e) {
        if (FloatingForm.IsShowing(BTB) || FloatingForm.IsShowing(BCB)) { return; }
        CloseAllComponents();
    }

    private void BB_TAB(object sender, System.EventArgs e) => CloseAllComponents();

    //private void BTB_NeedSQL_DatabaseOfAdditinalSpecialChars(object sender, MultiUserFileGiveBackEventArgs e) => e.File = SQL_Database;

    private void Cell_Edit(SQL_ColumnItem? cellInThisDatabaseColumn, SQL_RowData? cellInThisDatabaseRow, bool withDropDown) {
        //SQL_Database.OnConnectedControlsStopAllWorking(this, new MultiUserFileStopWorkingEventArgs());
        SQL_ColumnItem? contentHolderCellColumn;
        SQL_RowItem? contentHolderCellRow;

        //if (SQL_Database.ReloadNeeded) { SQL_Database.Load_Reload(); }

        var f = SQL_Database.ErrorReason(ErrorReason.EditGeneral);
        if (!string.IsNullOrEmpty(f)) { NotEdiSQL_TableInfo(f); return; }
        if (cellInThisDatabaseColumn == null) { return; }// Klick ins Leere

        var viewItem = CurrentArrangement[cellInThisDatabaseColumn];
        if (viewItem == null) {
            NotEdiSQL_TableInfo("Ansicht veraltet");
            return;
        }

        if (cellInThisDatabaseColumn.Format == DataFormat.Verknüpfung_zu_anderer_Datenbank) {
            (contentHolderCellColumn, contentHolderCellRow, var info) = SQL_CellCollection.LinkedCellData(cellInThisDatabaseColumn, cellInThisDatabaseRow?.Row, true, true);
            if (contentHolderCellColumn == null || contentHolderCellRow == null) {
                NotEdiSQL_TableInfo("In verknüpfter Datenbank nicht vorhanden:\r\n" + info);
                return;
            }
        } else {
            contentHolderCellColumn = cellInThisDatabaseColumn;
            contentHolderCellRow = cellInThisDatabaseRow?.Row;
        }

        if (!contentHolderCellColumn.DropdownBearbeitungErlaubt) { withDropDown = false; }
        var dia = SQL_ColumnItem.UserEditDialogTypeInTable(contentHolderCellColumn, withDropDown);
        if (dia == EditTypeTable.None) {
            NotEdiSQL_TableInfo("Diese Spalte kann generell nicht bearbeitet werden.");
            return;
        }
        if (!SQL_CellCollection.UserEditPossible(contentHolderCellColumn, contentHolderCellRow, ErrorReason.EditGeneral)) {
            NotEdiSQL_TableInfo(SQL_CellCollection.ErrorReason(contentHolderCellColumn, contentHolderCellRow, ErrorReason.EditGeneral));
            return;
        }
        if (cellInThisDatabaseRow != null) {
            if (!EnsureVisible(cellInThisDatabaseColumn, cellInThisDatabaseRow)) {
                NotEdiSQL_TableInfo("Zelle konnte nicht angezeigt werden.");
                return;
            }
            if (!IsOnScreen(cellInThisDatabaseColumn, cellInThisDatabaseRow, DisplayRectangle)) {
                NotEdiSQL_TableInfo("Die Zelle wird nicht angezeigt.");
                return;
            }
            CursorPos_Set(cellInThisDatabaseColumn, cellInThisDatabaseRow, false);
        } else {
            if (!UserEdit_NewRowAllowed()) {
                NotEdiSQL_TableInfo("Keine neuen Zeilen erlaubt.");
                return;
            }
            if (!EnsureVisible(viewItem)) {
                NotEdiSQL_TableInfo("Zelle konnte nicht angezeigt werden.");
                return;
            }
            if (!IsOnScreen(viewItem, DisplayRectangle)) {
                NotEdiSQL_TableInfo("Die Zelle wird nicht angezeigt.");
                return;
            }
            SliderY.Value = 0;
        }
        var cancel = "";
        if (cellInThisDatabaseRow != null) {
            SQL_CellCancelEventArgs ed = new(cellInThisDatabaseColumn, cellInThisDatabaseRow.Row, cancel);
            OnEditBeforeBeginEdit(ed);
            cancel = ed.CancelReason;
        } else {
            RowCancelEventArgs ed = new(null, cancel);
            //OnEditBeforeNewRow(ed);
            cancel = ed.CancelReason;
        }
        if (!string.IsNullOrEmpty(cancel)) {
            NotEdiSQL_TableInfo(cancel);
            return;
        }
        switch (dia) {
            case EditTypeTable.Textfeld:
                Cell_Edit_TextBox(cellInThisDatabaseColumn, cellInThisDatabaseRow, contentHolderCellColumn, contentHolderCellRow, BTB, 0, 0);
                break;

            case EditTypeTable.Textfeld_mit_Auswahlknopf:
                Cell_Edit_TextBox(cellInThisDatabaseColumn, cellInThisDatabaseRow, contentHolderCellColumn, contentHolderCellRow, BCB, 20, 18);
                break;

            case EditTypeTable.Dropdown_Single:
                Cell_Edit_Dropdown(cellInThisDatabaseColumn, cellInThisDatabaseRow, contentHolderCellColumn, contentHolderCellRow);
                break;
            //break;
            //case enEditType.Dropdown_Multi:
            //    Cell_Edit_Dropdown(cellInThisDatabaseColumn, cellInThisDatabaseRow, ContentHolderCellColumn, ContentHolderCellRow);
            //    break;
            //
            //case enEditTypeTable.RelationEditor_InSQL_Table:
            //    if (cellInThisDatabaseColumn != ContentHolderCellColumn || cellInThisDatabaseRow != ContentHolderCellRow)
            //    {
            //        NotEdiSQL_TableInfo("Ziel-Spalte ist kein Textformat");
            //        return;
            //    }
            //    Cell_Edit_Relations(cellInThisDatabaseColumn, cellInThisDatabaseRow);
            //    break;

            case EditTypeTable.Farb_Auswahl_Dialog:
                if (cellInThisDatabaseColumn != contentHolderCellColumn || cellInThisDatabaseRow?.Row != contentHolderCellRow) {
                    NotEdiSQL_TableInfo("Verlinkte Zellen hier verboten.");
                    return;
                }
                Cell_Edit_Color(cellInThisDatabaseColumn, cellInThisDatabaseRow);
                break;

            case EditTypeTable.Font_AuswahlDialog:
                Develop.DebugPrint_NichtImplementiert();
                //if (cellInThisDatabaseColumn != ContentHolderCellColumn || cellInThisDatabaseRow != ContentHolderCellRow)
                //{
                //    NotEdiSQL_TableInfo("Ziel-Spalte ist kein Textformat");
                //    return;
                //}
                //Cell_Edit_Font(cellInThisDatabaseColumn, cellInThisDatabaseRow);
                break;

            case EditTypeTable.WarnungNurFormular:
                NotEdiSQL_TableInfo("Dieser Zelltyp kann nur in einem Formular-Fenster bearbeitet werden");
                break;

            //case EditTypeTable.FileHandling_InDateiSystem:
            //    if (cellInThisDatabaseColumn != contentHolderCellColumn || cellInThisDatabaseRow?.Row != contentHolderCellRow) {
            //        NotEdiSQL_TableInfo("Verlinkte Zellen hier verboten.");
            //        return;
            //    }
            //    Cell_Edit_FileSystem(cellInThisDatabaseColumn, cellInThisDatabaseRow);
            //    break;

            default:
                Develop.DebugPrint(dia);
                NotEdiSQL_TableInfo("Unbekannte Bearbeitungs-Methode");
                break;
        }
    }

    private void Cell_Edit_Color(SQL_ColumnItem? cellInThisDatabaseColumn, SQL_RowData? cellInThisDatabaseRow) {
        ColDia.Color = cellInThisDatabaseRow.Row.CellGetColor(cellInThisDatabaseColumn);
        ColDia.Tag = SQL_CellCollection.KeyOfCell(cellInThisDatabaseColumn, cellInThisDatabaseRow.Row);
        List<int> colList = new();
        foreach (var thisSQL_RowItem in _database.Row) {
            if (thisSQL_RowItem != null) {
                if (thisSQL_RowItem.CellGetInteger(cellInThisDatabaseColumn) != 0) {
                    colList.Add(thisSQL_RowItem.CellGetColorBgr(cellInThisDatabaseColumn));
                }
            }
        }
        colList.Sort();
        ColDia.CustomColors = colList.Distinct().ToArray();
        ColDia.ShowDialog();
        ColDia.Dispose();
        UserEdited(this, Color.FromArgb(255, ColDia.Color).ToArgb().ToString(), cellInThisDatabaseColumn, cellInThisDatabaseRow?.Row, cellInThisDatabaseRow?.Chapter, false);
    }

    private void Cell_Edit_Dropdown(SQL_ColumnItem? cellInThisDatabaseColumn, SQL_RowData? cellInThisDatabaseRow, SQL_ColumnItem? contentHolderCellColumn, SQL_RowItem? contentHolderCellRow) {
        if (cellInThisDatabaseColumn != contentHolderCellColumn) {
            if (contentHolderCellRow == null) {
                NotEdiSQL_TableInfo("Bei Zellverweisen kann keine neue Zeile erstellt werden.");
                return;
            }
            if (cellInThisDatabaseRow == null) {
                NotEdiSQL_TableInfo("Bei Zellverweisen kann keine neue Zeile erstellt werden.");
                return;
            }
        }

        ItemCollectionList t = new() {
            Appearance = BlueListBoxAppearance.DropdownSelectbox
        };

        ItemCollectionList.GetItemCollection(t, contentHolderCellColumn, contentHolderCellRow, ShortenStyle.Replaced, 1000);
        if (t.Count == 0) {
            // Hm.. Dropdown kein Wert vorhanden.... also gar kein Dropdown öffnen!
            if (contentHolderCellColumn.TextBearbeitungErlaubt) { Cell_Edit(cellInThisDatabaseColumn, cellInThisDatabaseRow, false); } else {
                NotEdiSQL_TableInfo("Keine Items zum Auswählen vorhanden.");
            }
            return;
        }

        if (contentHolderCellColumn.TextBearbeitungErlaubt) {
            if (t.Count == 1 && cellInThisDatabaseRow.Row.CellIsNullOrEmpty(cellInThisDatabaseColumn)) {
                // Bei nur einem Wert, wenn Texteingabe erlaubt, Dropdown öffnen
                Cell_Edit(cellInThisDatabaseColumn, cellInThisDatabaseRow, false);
                return;
            }
            t.Add("Erweiterte Eingabe", "#Erweitert", QuickImage.Get(ImageCode.Stift), true, Constants.FirstSortChar + "1");
            t.AddSeparator(Constants.FirstSortChar + "2");
            t.Sort();
        }

        var dropDownMenu = FloatingInputBoxListBoxStyle.Show(t, new SQL_CellExtEventArgs(cellInThisDatabaseColumn, cellInThisDatabaseRow), this, Translate);
        dropDownMenu.ItemClicked += DropDownMenu_ItemClicked;
        Develop.Debugprint_BackgroundThread();
    }

    private bool Cell_Edit_TextBox(SQL_ColumnItem? cellInThisDatabaseColumn, SQL_RowData? cellInThisDatabaseRow, SQL_ColumnItem? contentHolderCellColumn, SQL_RowItem? contentHolderCellRow, TextBox Box, int addWith, int isHeight) {
        if (contentHolderCellColumn != cellInThisDatabaseColumn) {
            if (contentHolderCellRow == null) {
                NotEdiSQL_TableInfo("Bei Zellverweisen kann keine neue Zeile erstellt werden.");
                return false;
            }
            if (cellInThisDatabaseRow == null) {
                NotEdiSQL_TableInfo("Bei Zellverweisen kann keine neue Zeile erstellt werden.");
                return false;
            }
        }

        var viewItemx = CurrentArrangement[cellInThisDatabaseColumn];
        if (contentHolderCellRow != null) {
            var h = cellInThisDatabaseRow.DrawHeight;// Row_DrawHeight(cellInThisDatabaseRow, DisplayRectangle);
            if (isHeight > 0) { h = isHeight; }
            Box.Location = new Point((int)viewItemx.OrderTmpSpalteX1, DrawY(cellInThisDatabaseRow));
            Box.Size = new Size(Column_DrawWidth(viewItemx, DisplayRectangle) + addWith, h);
            Box.Text = contentHolderCellRow.CellGetString(contentHolderCellColumn).Replace(Constants.beChrW1.ToString(), "\r"); // Texte aus alter Zeit...
        } else {
            // Neue Zeile...
            Box.Location = new Point((int)viewItemx.OrderTmpSpalteX1, HeadSize());
            Box.Size = new Size(Column_DrawWidth(viewItemx, DisplayRectangle) + addWith, _pix18);
            Box.Text = "";
        }

        Box.GetStyleFrom(contentHolderCellColumn);

        Box.Tag = SQL_CellCollection.KeyOfCell(cellInThisDatabaseColumn, cellInThisDatabaseRow?.Row); // ThisSQL_Database, der Wert wird beim einchecken in die Fremdzelle geschrieben
        if (Box is ComboBox box) {
            ItemCollectionList.GetItemCollection(box.Item, contentHolderCellColumn, contentHolderCellRow, ShortenStyle.Replaced, 1000);
            if (box.Item.Count == 0) {
                return Cell_Edit_TextBox(cellInThisDatabaseColumn, cellInThisDatabaseRow, contentHolderCellColumn, contentHolderCellRow, BTB, 0, 0);
            }
        }

        if (string.IsNullOrEmpty(Box.Text)) {
            Box.Text = SQL_CellCollection.AutomaticInitalValue(contentHolderCellColumn, contentHolderCellRow);
        }

        Box.Visible = true;
        Box.BringToFront();
        Box.Focus();
        return true;
    }

    //private void Cell_Edit_FileSystem(SQL_ColumnItem? cellInThisDatabaseColumn, SQL_RowData? cellInThisDatabaseRow) {
    //    var l = FileSystem(cellInThisDatabaseColumn);
    //    if (l == null) { return; }
    //    UserEdited(this, l.JoinWithCr(), cellInThisDatabaseColumn, cellInThisDatabaseRow?.Row, cellInThisDatabaseRow?.Chapter, false);
    //}
    private void CellOnCoordinate(int xpos, int ypos, out SQL_ColumnItem? column, out SQL_RowData? row) {
        column = ColumnOnCoordinate(xpos);
        row = RowOnCoordinate(ypos);
    }

    private void CloseAllComponents() {
        if (InvokeRequired) {
            Invoke(new Action(CloseAllComponents));
            return;
        }
        if (_database == null) { return; }
        TXTBox_Close(BTB);
        TXTBox_Close(BCB);
        FloatingForm.Close(this);
        AutoFilter_Close();
        Forms.QuickInfo.Close();
    }

    private int Column_DrawWidth(SQL_ColumnViewItem viewItem, Rectangle displayRectangleWoSlider) {
        // Hier wird die ORIGINAL-Spalte gezeichnet, nicht die FremdZelle!!!!

        if (viewItem.TmpDrawWidth != null) { return (int)viewItem.TmpDrawWidth; }
        if (viewItem?.Column == null) { return 0; }

        if (_design == BlueTableAppearance.OnlyMainColumnWithoutHead) {
            viewItem.TmpDrawWidth = displayRectangleWoSlider.Width - 1;
            return (int)viewItem.TmpDrawWidth;
        }

        if (viewItem.TmpReduced) {
            viewItem.TmpDrawWidth = 16;
        } else {
            viewItem.TmpDrawWidth = viewItem.ViewType == ViewType.PermanentColumn
                ? Math.Min(TmpColumnContentWidth(this, viewItem.Column, _cellFont, _pix16), (int)(displayRectangleWoSlider.Width * 0.3))
                : Math.Min(TmpColumnContentWidth(this, viewItem.Column, _cellFont, _pix16), (int)(displayRectangleWoSlider.Width * 0.75));
        }

        viewItem.TmpDrawWidth = Math.Max((int)viewItem.TmpDrawWidth, AutoFilterSize); // Mindestens so groß wie der Autofilter;
        viewItem.TmpDrawWidth = Math.Max((int)viewItem.TmpDrawWidth, (int)ColumnHead_Size(viewItem.Column).Width);
        return (int)viewItem.TmpDrawWidth;
    }

    private void Column_ItemRemoving(object sender, ListEventArgs e) {
        if (e.Item == _cursorPosColumn) {
            CursorPos_Reset();
        }
        if (e.Item == _mouseOverColumn) {
            _mouseOverColumn = null;
        }
    }

    private void ColumnArrangements_ItemInternalChanged(object sender, ListEventArgs e) {
        OnColumnArrangementChanged();
        Invalidate();
    }

    private SizeF ColumnCaptionText_Size(SQL_ColumnItem? column) {
        if (column.TmpCaptionTextSize.Width > 0) { return column.TmpCaptionTextSize; }
        if (_columnFont == null) { return new SizeF(_pix16, _pix16); }
        column.TmpCaptionTextSize = BlueFont.MeasureString(column.Caption.Replace("\r", "\r\n"), _columnFont.Font());
        return column.TmpCaptionTextSize;
    }

    private SizeF ColumnHead_Size(SQL_ColumnItem? column) {
        float wi;
        float he;
        Bitmap captionBitmap = null; // TODO: Caption Bitmap neu erstellen
        if (captionBitmap != null && captionBitmap.Width > 10) {
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

    private SQL_ColumnItem? ColumnOnCoordinate(int xpos) {
        if (_database == null || _database.ColumnArrangements.Count - 1 < _arrangementNr) { return null; }
        foreach (var thisViewItem in CurrentArrangement) {
            if (thisViewItem?.Column != null) {
                if (xpos >= thisViewItem.OrderTmpSpalteX1 && xpos <= thisViewItem.OrderTmpSpalteX1 + Column_DrawWidth(thisViewItem, DisplayRectangleWithoutSlider())) {
                    return thisViewItem.Column;
                }
            }
        }
        return null;
    }

    private bool ComputeAllColumnPositions() {
        //Develop.DebugPrint_InvokeRequired(InvokeRequired, true);
        //if (SQL_Database.IsParsing) { return false; }
        try {
            // Kommt vor, dass spontan doch geparsed wird...
            if (_database.ColumnArrangements == null || _arrangementNr >= _database.ColumnArrangements.Count) { return false; }
            foreach (var thisViewItem in CurrentArrangement) {
                if (thisViewItem != null) {
                    thisViewItem.OrderTmpSpalteX1 = null;
                }
            }
            //foreach (var ThisSQL_RowItem in _SQL_Database.Row) {
            //    if (ThisSQL_RowItem != null) { ThisSQL_RowItem.TMP_Y = null; }
            //}
            _wiederHolungsSpaltenWidth = 0;
            _mouseOverText = string.Empty;
            var wdh = true;
            var maxX = 0;
            var displayR = DisplayRectangleWithoutSlider();
            // Spalten berechnen
            foreach (var thisViewItem in CurrentArrangement) {
                if (thisViewItem?.Column != null) {
                    if (thisViewItem.ViewType != ViewType.PermanentColumn) { wdh = false; }
                    if (wdh) {
                        thisViewItem.OrderTmpSpalteX1 = maxX;
                        maxX += Column_DrawWidth(thisViewItem, displayR);
                        _wiederHolungsSpaltenWidth = Math.Max(_wiederHolungsSpaltenWidth, maxX);
                    } else {
                        thisViewItem.OrderTmpSpalteX1 = SliderX.Enabled ? (int)(maxX - SliderX.Value) : maxX;
                        maxX += Column_DrawWidth(thisViewItem, displayR);
                    }
                }
            }
            SliderSchaltenWaage(displayR, maxX);
        } catch (Exception ex) {
            Develop.DebugPrint(ex);
            return false;
        }
        return true;
    }

    /// <summary>
    /// Setzt die Variable CursorPos um X Columns und Y Reihen um. Dabei wird die Columns und Zeilensortierung berücksichtigt.
    /// </summary>
    /// <remarks></remarks>
    private void Cursor_Move(Direction richtung) {
        if (_database == null) { return; }
        Neighbour(_cursorPosColumn, _cursorPosRow, richtung, out var newCol, out var newRow);
        CursorPos_Set(newCol, newRow, richtung != Direction.Nichts);
    }

    private void CursorPos_Set(SQL_ColumnItem? column, SQL_RowItem? row, bool ensureVisible, string chapter) => CursorPos_Set(column, SortedRows().Get(row, chapter), ensureVisible);

    private Rectangle DisplayRectangleWithoutSlider() => _design == BlueTableAppearance.Standard
            ? new Rectangle(DisplayRectangle.Left, DisplayRectangle.Left, DisplayRectangle.Width - SliderY.Width, DisplayRectangle.Height - SliderX.Height)
            : new Rectangle(DisplayRectangle.Left, DisplayRectangle.Left, DisplayRectangle.Width - SliderY.Width, DisplayRectangle.Height);

    private void Draw_Border(Graphics gr, SQL_ColumnViewItem column, Rectangle displayRectangleWoSlider, bool onlyhead) {
        var yPos = displayRectangleWoSlider.Height;
        if (onlyhead) { yPos = HeadSize(); }
        for (var z = 0; z <= 1; z++) {
            int xPos;
            ColumnLineStyle lin;
            if (z == 0) {
                xPos = (int)column.OrderTmpSpalteX1;
                lin = column.Column.LineLeft;
            } else {
                xPos = (int)column.OrderTmpSpalteX1 + Column_DrawWidth(column, displayRectangleWoSlider);
                lin = column.Column.LineRight;
            }
            switch (lin) {
                case ColumnLineStyle.Ohne:
                    break;

                case ColumnLineStyle.Dünn:
                    gr.DrawLine(Skin.PenLinieDünn, xPos, 0, xPos, yPos);
                    break;

                case ColumnLineStyle.Kräftig:
                    gr.DrawLine(Skin.PenLinieKräftig, xPos, 0, xPos, yPos);
                    break;

                case ColumnLineStyle.Dick:
                    gr.DrawLine(Skin.PenLinieDick, xPos, 0, xPos, yPos);
                    break;

                case ColumnLineStyle.ShadowRight:
                    var c = Skin.Color_Border(Enums.Design.Table_Lines_thick, States.Standard);
                    gr.DrawLine(Skin.PenLinieKräftig, xPos, 0, xPos, yPos);
                    gr.DrawLine(new Pen(Color.FromArgb(80, c.R, c.G, c.B)), xPos + 1, 0, xPos + 1, yPos);
                    gr.DrawLine(new Pen(Color.FromArgb(60, c.R, c.G, c.B)), xPos + 2, 0, xPos + 2, yPos);
                    gr.DrawLine(new Pen(Color.FromArgb(40, c.R, c.G, c.B)), xPos + 3, 0, xPos + 3, yPos);
                    gr.DrawLine(new Pen(Color.FromArgb(20, c.R, c.G, c.B)), xPos + 4, 0, xPos + 4, yPos);
                    break;

                default:
                    Develop.DebugPrint(lin);
                    break;
            }
        }
    }

    //private void Draw_CellAsButton(Graphics gr, SQL_ColumnViewItem cellInThisDatabaseColumn, SQL_RowData cellInThisDatabaseRow, Rectangle cellrectangle) {
    //    ButtonCellEventArgs e = new(cellInThisDatabaseColumn.Column, cellInThisDatabaseRow.Row);
    //    OnNeedButtonArgs(e);

    //    var s = States.Standard;
    //    if (!Enabled) { s = States.Standard_Disabled; }
    //    if (e.Cecked) { s |= States.Checked; }

    //    var x = new ExtText(Enums.Design.Button_CheckBox, s);
    //    Button.DrawButton(this, gr, Enums.Design.Button_CheckBox, s, e.Image, Alignment.Horizontal_Vertical_Center, false, x, e.Text, cellrectangle, true);
    //}

    /// <summary>
    /// Zeichnet die gesamte Zelle mit Listbox-Hintergrund und prüft noch, ob der verlinkte Inhalt gezeichnet werden soll.
    /// </summary>
    /// <param name="gr"></param>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <param name="displayRectangleWoSlider"></param>
    /// <param name="design"></param>
    /// <param name="state"></param>
    private void Draw_CellListBox(Graphics gr, SQL_ColumnViewItem column, SQL_RowData row, Rectangle cellrectangle, Rectangle displayRectangleWoSlider, Design design, States state) {
        Skin.Draw_Back(gr, design, state, displayRectangleWoSlider, null, false);
        Skin.Draw_Border(gr, design, state, displayRectangleWoSlider);
        var f = Skin.GetBlueFont(design, state);
        if (f == null) { return; }
        Draw_CellTransparent(gr, column, row, cellrectangle, f, state);
    }

    /// <summary>
    /// Zeichnet die gesamte Zelle ohne Hintergrund und prüft noch, ob der verlinkte Inhalt gezeichnet werden soll.
    /// </summary>
    /// <param name="gr"></param>
    /// <param name="cellInThisDatabaseColumn"></param>
    /// <param name="cellInThisDatabaseRow"></param>
    /// <param name="displayRectangleWOSlider"></param>
    /// <param name="font"></param>
    private void Draw_CellTransparent(Graphics gr, SQL_ColumnViewItem cellInThisDatabaseColumn, SQL_RowData? cellInThisDatabaseRow, Rectangle cellrectangle, BlueFont? font, States state) {
        if (cellInThisDatabaseRow == null) { return; }

        if (cellInThisDatabaseColumn.Column.Format == DataFormat.Verknüpfung_zu_anderer_Datenbank) {
            var (lcolumn, lrow, _) = SQL_CellCollection.LinkedCellData(cellInThisDatabaseColumn.Column, cellInThisDatabaseRow.Row, false, false);

            if (lcolumn != null && lrow != null) {
                Draw_CellTransparentDirect(gr, cellInThisDatabaseColumn, cellInThisDatabaseRow, cellrectangle, lcolumn, lrow, font, state);
            } else {
                if (cellInThisDatabaseRow.Row.Database.IsAdministrator()) {
                    gr.DrawImage(QuickImage.Get("Warnung|10||||||120||60"), cellrectangle.Left + 3, cellrectangle.Top + 1);
                }
            }
            return;
        }

        Draw_CellTransparentDirect(gr, cellInThisDatabaseColumn, cellInThisDatabaseRow, cellrectangle, cellInThisDatabaseColumn.Column, cellInThisDatabaseRow.Row, font, state);
    }

    /// <summary>
    /// Zeichnet die gesamte Zelle ohne Hintergrund. Die verlinkte Zelle ist bereits bekannt.
    /// </summary>
    /// <param name="gr"></param>
    /// <param name="cellInThisDatabaseColumn"></param>
    /// <param name="cellInThisDatabaseRow"></param>
    /// <param name="rowY"></param>
    /// <param name="contentHolderCellColumn"></param>
    /// <param name="contentHolderCellRow"></param>
    /// <param name="displayRectangleWOSlider"></param>
    /// <param name="font"></param>
    private void Draw_CellTransparentDirect(Graphics gr, SQL_ColumnViewItem cellInThisDatabaseColumn, SQL_RowData cellInThisDatabaseRow, Rectangle cellrectangle, SQL_ColumnItem? contentHolderCellColumn, SQL_RowItem? contentHolderCellRow, BlueFont? font, States state) {
        //if (cellInThisDatabaseColumn.Column.Format == DataFormat.Button) {
        //    Draw_CellAsButton(gr, cellInThisDatabaseColumn, cellInThisDatabaseRow, cellrectangle);
        //    return;
        //}

        var toDraw = contentHolderCellRow.CellGetString(contentHolderCellColumn);

        Draw_CellTransparentDirect(gr, toDraw, cellrectangle, font, contentHolderCellColumn, _pix16, ShortenStyle.Replaced, contentHolderCellColumn.BildTextVerhalten, state);
    }

    private void Draw_Column_Body(Graphics gr, SQL_ColumnViewItem cellInThisDatabaseColumn, Rectangle displayRectangleWoSlider) {
        gr.SmoothingMode = SmoothingMode.None;
        gr.FillRectangle(new SolidBrush(cellInThisDatabaseColumn.Column.BackColor), (int)cellInThisDatabaseColumn.OrderTmpSpalteX1, HeadSize(), Column_DrawWidth(cellInThisDatabaseColumn, displayRectangleWoSlider), displayRectangleWoSlider.Height);
        Draw_Border(gr, cellInThisDatabaseColumn, displayRectangleWoSlider, false);
    }

    private void Draw_Column_Cells(Graphics gr, List<SQL_RowData?> sr, SQL_ColumnViewItem viewItem, Rectangle displayRectangleWoSlider, int firstVisibleRow, int lastVisibleRow, States state, bool firstOnScreen) {

        #region Neue Zeile

        if (UserEdit_NewRowAllowed() && viewItem == CurrentArrangement[SQL_Database.Column[0]]) {
            Skin.Draw_FormatedText(gr, "[Neue Zeile]", QuickImage.Get(ImageCode.PlusZeichen, _pix16), Alignment.Left, new Rectangle((int)viewItem.OrderTmpSpalteX1 + 1, (int)(-SliderY.Value + HeadSize() + 1), (int)viewItem.TmpDrawWidth - 2, 16 - 2), this, false, _newRowFont, Translate);
        }

        #endregion

        #region Zeilen Zeichnen (Alle Zellen)

        for (var zei = firstVisibleRow; zei <= lastVisibleRow; zei++) {
            var currentRow = sr[zei];
            gr.SmoothingMode = SmoothingMode.None;

            Rectangle cellrectangle = new((int)viewItem.OrderTmpSpalteX1, DrawY(currentRow),
                Column_DrawWidth(viewItem, displayRectangleWoSlider), Math.Max(currentRow.DrawHeight, _pix16));

            if (currentRow.Expanded) {

                #region Hintergrund gelb zeichnen

                if (currentRow.MarkYellow) {
                    gr.FillRectangle(BrushYellowTransparent, cellrectangle);
                }

                #endregion

                #region Trennlinie zeichnen

                gr.DrawLine(Skin.PenLinieDünn, cellrectangle.Left, cellrectangle.Bottom - 1, cellrectangle.Right - 1, cellrectangle.Bottom - 1);

                #endregion

                #region Die Cursorposition ermittleln und Zeichnen

                if (!Thread.CurrentThread.IsBackground && _cursorPosColumn == viewItem.Column && _cursorPosRow == currentRow) {
                    _tmpCursorRect = cellrectangle;
                    _tmpCursorRect.Height -= 1;
                    Draw_Cursor(gr, displayRectangleWoSlider, false);
                }

                #endregion

                #region Zelleninhalt zeichnen

                Draw_CellTransparent(gr, viewItem, currentRow, cellrectangle, _cellFont, state);

                #endregion

                #region Unterschiede rot markieren

                if (_unterschiede != null && _unterschiede != currentRow.Row) {
                    if (currentRow.Row.CellGetString(viewItem.Column) != _unterschiede.CellGetString(viewItem.Column)) {
                        Rectangle tmpr = new((int)viewItem.OrderTmpSpalteX1 + 1, DrawY(currentRow) + 1,
                            Column_DrawWidth(viewItem, displayRectangleWoSlider) - 2, currentRow.DrawHeight - 2);
                        gr.DrawRectangle(PenRed1, tmpr);
                    }
                }

                #endregion
            }

            #region Spaltenüberschrift

            if (firstOnScreen) {
                // Überschrift in der ersten Spalte zeichnen
                currentRow.CaptionPos = Rectangle.Empty;
                if (currentRow.ShowCap) {
                    var si = gr.MeasureString(currentRow.Chapter, _chapterFont.Font());
                    gr.FillRectangle(new SolidBrush(Skin.Color_Back(BlueControls.Enums.Design.Table_And_Pad, States.Standard).SetAlpha(50)), 1, DrawY(currentRow) - RowCaptionSizeY, displayRectangleWoSlider.Width - 2, RowCaptionSizeY);
                    currentRow.CaptionPos = new Rectangle(1, DrawY(currentRow) - _rowCaptionFontY, (int)si.Width + 28, (int)si.Height);

                    if (_collapsed.Contains(currentRow.Chapter)) {
                        var x = new ExtText(Enums.Design.Button_CheckBox, States.Checked);
                        Button.DrawButton(this, gr, Enums.Design.Button_CheckBox, States.Checked, null, Alignment.Horizontal_Vertical_Center, false, x, string.Empty, currentRow.CaptionPos, false);
                        gr.DrawImage(QuickImage.Get("Pfeil_Unten_Scrollbar|14|||FF0000||200|200")!, 5, DrawY(currentRow) - _rowCaptionFontY + 6);
                    } else {
                        var x = new ExtText(Enums.Design.Button_CheckBox, States.Standard);
                        Button.DrawButton(this, gr, Enums.Design.Button_CheckBox, States.Standard, null, Alignment.Horizontal_Vertical_Center, false, x, string.Empty, currentRow.CaptionPos, false);
                        gr.DrawImage(QuickImage.Get("Pfeil_Rechts_Scrollbar|14|||||0")!, 5, DrawY(currentRow) - _rowCaptionFontY + 6);
                    }
                    _chapterFont.DrawString(gr, currentRow.Chapter, 23, DrawY(currentRow) - _rowCaptionFontY);
                    gr.DrawLine(Skin.PenLinieDick, 0, DrawY(currentRow), displayRectangleWoSlider.Width, DrawY(currentRow));
                }
            }

            #endregion
        }

        #endregion
    }

    private void Draw_Column_Head_Captions(Graphics gr) {
        var bvi = new SQL_ColumnViewItem[3];
        var lcbvi = new SQL_ColumnViewItem[3];
        SQL_ColumnViewItem lastViewItem = null;
        var permaX = 0;
        var ca = CurrentArrangement;
        for (var x = 0; x < ca.Count + 1; x++) {
            var viewItem = x < ca.Count ? ca[x] : null;
            if (viewItem?.ViewType == ViewType.PermanentColumn) {
                permaX = Math.Max(permaX, (int)viewItem.OrderTmpSpalteX1 + (int)viewItem.TmpDrawWidth);
            }
            if (viewItem == null ||
                viewItem.ViewType == ViewType.PermanentColumn ||
                (int)viewItem.OrderTmpSpalteX1 + (int)viewItem.TmpDrawWidth > permaX) {
                for (var u = 0; u < 3; u++) {
                    var n = viewItem?.Column.Ueberschrift(u);
                    var v = bvi[u]?.Column.Ueberschrift(u);
                    if (n != v) {
                        if (!string.IsNullOrEmpty(v) && lastViewItem != null) {
                            var le = Math.Max(0, (int)bvi[u].OrderTmpSpalteX1);
                            //int RE = (int)LastViewItem.OrderTMP_Spalte_X1 - 1 ;
                            var re = (int)lastViewItem.OrderTmpSpalteX1 + (int)lastViewItem.TmpDrawWidth - 1;
                            if (viewItem?.ViewType != ViewType.PermanentColumn && bvi[u].ViewType != ViewType.PermanentColumn) { le = Math.Max(le, permaX); }
                            if (viewItem?.ViewType != ViewType.PermanentColumn && bvi[u].ViewType == ViewType.PermanentColumn) { re = Math.Max(re, (int)lcbvi[u].OrderTmpSpalteX1 + (int)lcbvi[u].TmpDrawWidth); }
                            if (le < re) {
                                Rectangle r = new(le, u * ColumnCaptionSizeY, re - le, ColumnCaptionSizeY);
                                gr.FillRectangle(new SolidBrush(bvi[u].Column.BackColor), r);
                                gr.FillRectangle(new SolidBrush(Color.FromArgb(80, 200, 200, 200)), r);
                                gr.DrawRectangle(Skin.PenLinieKräftig, r);
                                Skin.Draw_FormatedText(gr, v, null, Alignment.Horizontal_Vertical_Center, r, this, false, _columnFont, Translate);
                            }
                        }
                        bvi[u] = viewItem;
                        if (viewItem?.ViewType == ViewType.PermanentColumn) { lcbvi[u] = viewItem; }
                    }
                }
                lastViewItem = viewItem;
            }
        }
    }

    private void Draw_Cursor(Graphics gr, Rectangle displayRectangleWoSlider, bool onlyCursorLines) {
        if (_tmpCursorRect.Width < 1) { return; }

        var stat = States.Standard;
        if (Focused()) { stat = States.Standard_HasFocus; }

        if (onlyCursorLines) {
            Pen pen = new(Skin.Color_Border(BlueControls.Enums.Design.Table_Cursor, stat).SetAlpha(180));
            gr.DrawRectangle(pen, new Rectangle(-1, _tmpCursorRect.Top - 1, displayRectangleWoSlider.Width + 2, _tmpCursorRect.Height + 1));
        } else {
            Skin.Draw_Back(gr, Enums.Design.Table_Cursor, stat, _tmpCursorRect, this, false);
            Skin.Draw_Border(gr, Enums.Design.Table_Cursor, stat, _tmpCursorRect);
        }
    }

    private void Draw_SQL_Table_ListboxStyle(Graphics gr, List<SQL_RowData?> sr, States state, Rectangle displayRectangleWoSlider, int vFirstVisibleRow, int vLastVisibleRow) {
        var itStat = state;
        Skin.Draw_Back(gr, Enums.Design.ListBox, state, base.DisplayRectangle, this, true);
        var col = SQL_Database.Column[0];
        // Zeilen Zeichnen (Alle Zellen)
        for (var zeiv = vFirstVisibleRow; zeiv <= vLastVisibleRow; zeiv++) {
            var currentRow = sr[zeiv];
            var viewItem = _database.ColumnArrangements[0][col];
            Rectangle r = new(0, DrawY(currentRow), DisplayRectangleWithoutSlider().Width,
                currentRow.DrawHeight);
            if (_cursorPosColumn != null && _cursorPosRow.Row == currentRow.Row) {
                itStat |= States.Checked;
            } else {
                if (Convert.ToBoolean(itStat & States.Checked)) {
                    itStat ^= States.Checked;
                }
            }

            Rectangle cellrectangle = new(0, DrawY(currentRow), displayRectangleWoSlider.Width,
                Math.Min(currentRow.DrawHeight, 24));

            Draw_CellListBox(gr, viewItem, currentRow, cellrectangle, r, Enums.Design.Item_Listbox, itStat);
            if (!currentRow.Row.CellGetBoolean(_database.Column.SysCorrect)) {
                gr.DrawImage(QuickImage.Get("Warnung|16||||||120||50"), new Point(r.Right - 19, (int)(r.Top + ((r.Height - 16) / 2.0))));
            }
            if (currentRow.ShowCap) {
                BlueFont.DrawString(gr, currentRow.Chapter, _chapterFont.Font(), _chapterFont.BrushColorMain, 0, DrawY(currentRow) - _rowCaptionFontY);
            }
        }
        Skin.Draw_Border(gr, Enums.Design.ListBox, state, displayRectangleWoSlider);
    }

    private void Draw_SQL_Table_Std(Graphics gr, List<SQL_RowData?> sr, States state, Rectangle displayRectangleWoSlider, int firstVisibleRow, int lastVisibleRow) {
        try {
            if (_database.ColumnArrangements == null || _arrangementNr >= _database.ColumnArrangements.Count) { return; }   // Kommt vor, dass spontan doch geparsed wird...
            Skin.Draw_Back(gr, Enums.Design.Table_And_Pad, state, base.DisplayRectangle, this, true);
            /// Maximale Rechten Pixel der Permanenten Columns ermitteln
            var permaX = 0;
            foreach (var viewItem in CurrentArrangement) {
                if (viewItem?.Column != null && viewItem.ViewType == ViewType.PermanentColumn) {
                    if (viewItem.TmpDrawWidth == null) {
                        // Veränderte Werte!
                        DrawControl(gr, state);
                        return;
                    }
                    permaX = Math.Max(permaX, (int)viewItem.OrderTmpSpalteX1 + (int)viewItem.TmpDrawWidth);
                }
            }
            Draw_SQL_Table_What(gr, sr, TableDrawColumn.NonPermament, TableDrawType.ColumnBackBody, permaX, displayRectangleWoSlider, firstVisibleRow, lastVisibleRow, state);
            Draw_SQL_Table_What(gr, sr, TableDrawColumn.NonPermament, TableDrawType.Cells, permaX, displayRectangleWoSlider, firstVisibleRow, lastVisibleRow, state);
            Draw_SQL_Table_What(gr, sr, TableDrawColumn.Permament, TableDrawType.ColumnBackBody, permaX, displayRectangleWoSlider, firstVisibleRow, lastVisibleRow, state);
            Draw_SQL_Table_What(gr, sr, TableDrawColumn.Permament, TableDrawType.Cells, permaX, displayRectangleWoSlider, firstVisibleRow, lastVisibleRow, state);
            // Den CursorLines zeichnen
            Draw_Cursor(gr, displayRectangleWoSlider, true);
            Draw_SQL_Table_What(gr, sr, TableDrawColumn.NonPermament, TableDrawType.ColumnHead, permaX, displayRectangleWoSlider, firstVisibleRow, lastVisibleRow, state);
            Draw_SQL_Table_What(gr, sr, TableDrawColumn.Permament, TableDrawType.ColumnHead, permaX, displayRectangleWoSlider, firstVisibleRow, lastVisibleRow, state);

            /// Überschriften 1-3 Zeichnen
            Draw_Column_Head_Captions(gr);
            Skin.Draw_Border(gr, Enums.Design.Table_And_Pad, state, displayRectangleWoSlider);

            //if (SQL_Database.ReloadNeeded) { gr.DrawImage(QuickImage.Get(ImageCode.Uhr, 16), 8, 8); }

            //var e2 = new MultiUserFileHasPendingChangesEventArgs();
            //SQL_Database.HasPendingChanges(null, e2);

            //if (e2.HasPendingChanges) { gr.DrawImage(QuickImage.Get(ImageCode.Stift, 16), 16, 8); }
            //if (SQL_Database.ReadOnly) { gr.DrawImage(QuickImage.Get(ImageCode.Schloss, 32), 16, 8); }
        } catch {
            Invalidate();
            //Develop.DebugPrint(ex);
        }
    }

    private void Draw_SQL_Table_What(Graphics gr, List<SQL_RowData?> sr, TableDrawColumn col, TableDrawType type, int permaX, Rectangle displayRectangleWoSlider, int firstVisibleRow, int lastVisibleRow, States state) {
        var lfdno = 0;

        var firstOnScreen = true;

        foreach (var viewItem in CurrentArrangement.Where(viewItem => viewItem?.Column != null)) {
            lfdno++;
            if (IsOnScreen(viewItem, displayRectangleWoSlider)) {
                if ((col == TableDrawColumn.NonPermament && viewItem.ViewType != ViewType.PermanentColumn && (int)viewItem.OrderTmpSpalteX1 + (int)viewItem.TmpDrawWidth > permaX) ||
                    (col == TableDrawColumn.Permament && viewItem.ViewType == ViewType.PermanentColumn)) {
                    switch (type) {
                        case TableDrawType.ColumnBackBody:
                            Draw_Column_Body(gr, viewItem, displayRectangleWoSlider);
                            break;

                        case TableDrawType.Cells:
                            Draw_Column_Cells(gr, sr, viewItem, displayRectangleWoSlider, firstVisibleRow, lastVisibleRow, state, firstOnScreen);
                            break;

                        case TableDrawType.ColumnHead:
                            Draw_Column_Head(gr, viewItem, displayRectangleWoSlider, lfdno);
                            break;
                    }
                }
                firstOnScreen = false;
            }
        }
    }

    private void DrawWaitScreen(Graphics gr) {
        if (SliderX != null) { SliderX.Enabled = false; }
        if (SliderY != null) { SliderY.Enabled = false; }

        Skin.Draw_Back(gr, Enums.Design.Table_And_Pad, States.Standard_Disabled, base.DisplayRectangle, this, true);

        var i = QuickImage.Get(ImageCode.Uhr, 64);
        gr.DrawImage(i, (Width - 64) / 2, (Height - 64) / 2);
        Skin.Draw_Border(gr, Enums.Design.Table_And_Pad, States.Standard_Disabled, base.DisplayRectangle);
    }

    private int DrawY(SQL_RowData? r) => r == null ? 0 : r.Y + HeadSize() - (int)SliderY.Value;

    private void DropDownMenu_ItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
        FloatingForm.Close(this);
        if (string.IsNullOrEmpty(e.ClickedComand)) { return; }

        SQL_CellExtEventArgs ck = null;
        if (e.HotItem is SQL_CellExtEventArgs tmp) { ck = tmp; }

        var toAdd = e.ClickedComand;
        var toRemove = string.Empty;
        if (toAdd == "#Erweitert") {
            Cell_Edit(ck.Column, ck.RowData, false);
            return;
        }
        if (ck.RowData == null) {
            // Neue Zeile!
            UserEdited(this, toAdd, ck.Column, null, string.Empty, false);
            return;
        }

        if (ck.Column.MultiLine) {
            var li = ck.RowData.Row.CellGetList(ck.Column);
            if (li.Contains(toAdd, false)) {
                // Ist das angeklickte Element schon vorhanden, dann soll es wohl abgewählt (gelöscht) werden.
                if (li.Count > -1 || ck.Column.DropdownAllesAbwählenErlaubt) {
                    toRemove = toAdd;
                    toAdd = string.Empty;
                }
            }
            if (!string.IsNullOrEmpty(toRemove)) { li.RemoveString(toRemove, false); }
            if (!string.IsNullOrEmpty(toAdd)) { li.Add(toAdd); }
            UserEdited(this, li.JoinWithCr(), ck.Column, ck.RowData.Row, ck.RowData.Chapter, false);
        } else {
            if (ck.Column.DropdownAllesAbwählenErlaubt) {
                if (toAdd == ck.RowData.Row.CellGetString(ck.Column)) {
                    UserEdited(this, string.Empty, ck.Column, ck.RowData.Row, ck.RowData.Chapter, false);
                    return;
                }
            }
            UserEdited(this, toAdd, ck.Column, ck.RowData.Row, ck.RowData.Chapter, false);
        }
    }

    private bool EnsureVisible(SQL_RowData? SQL_RowData) {
        if (SQL_RowData == null) { return false; }
        var r = DisplayRectangleWithoutSlider();
        if (DrawY(SQL_RowData) < HeadSize()) {
            SliderY.Value = SliderY.Value + DrawY(SQL_RowData) - HeadSize();
        } else if (DrawY(SQL_RowData) + SQL_RowData.DrawHeight > r.Height) {
            SliderY.Value = SliderY.Value + DrawY(SQL_RowData) + SQL_RowData.DrawHeight - r.Height;
        }
        return true;
    }

    private bool EnsureVisible(SQL_ColumnViewItem? viewItem) {
        if (viewItem?.Column == null) { return false; }
        if (viewItem.OrderTmpSpalteX1 == null && !ComputeAllColumnPositions()) { return false; }
        var r = DisplayRectangleWithoutSlider();
        ComputeAllColumnPositions();
        if (viewItem.ViewType == ViewType.PermanentColumn) {
            if (viewItem.OrderTmpSpalteX1 + Column_DrawWidth(viewItem, r) <= r.Width) { return true; }
            //Develop.DebugPrint(enFehlerArt.Info,"Unsichtbare Wiederholungsspalte: " + ViewItem.Column.Name);
            return false;
        }
        if (viewItem.OrderTmpSpalteX1 < _wiederHolungsSpaltenWidth) {
            SliderX.Value = SliderX.Value + (int)viewItem.OrderTmpSpalteX1 - _wiederHolungsSpaltenWidth;
        } else if (viewItem.OrderTmpSpalteX1 + Column_DrawWidth(viewItem, r) > r.Width) {
            SliderX.Value = SliderX.Value + (int)viewItem.OrderTmpSpalteX1 + Column_DrawWidth(viewItem, r) - r.Width;
        }
        return true;
    }

    private void Filter_Changed(object sender, System.EventArgs e) {
        Invalidate_FilteredRows();
        OnFilterChanged();
    }

    private int HeadSize() {
        if (_headSize != null) { return (int)_headSize; }
        if (_database.ColumnArrangements.Count - 1 < _arrangementNr) {
            _headSize = 0;
            return 0;
        }
        if (_design == BlueTableAppearance.OnlyMainColumnWithoutHead || CurrentArrangement.Count - 1 < 0) {
            _headSize = 0;
            return 0;
        }
        _headSize = 16;
        foreach (var thisViewItem in CurrentArrangement.Where(thisViewItem => thisViewItem?.Column != null)) {
            _headSize = Math.Max((int)_headSize, (int)ColumnHead_Size(thisViewItem.Column).Height);
        }
        _headSize += 8;
        _headSize += AutoFilterSize;
        return (int)_headSize;
    }

    private void Invalidate_DrawWidth(SQL_ColumnItem? vcolumn) => CurrentArrangement[vcolumn]?.Invalidate_DrawWidth();

    private void Invalidate_FilteredRows() {
        _filteredRows = null;
        //CursorPos_Reset(); // Gibt Probleme bei Formularen, wenn die Key-Spalte geändert wird. Mal abgesehen davon macht es einen Sinn, den Cursor proforma zu löschen, dass soll der RowSorter übernehmen.
        Invalidate_Filterinfo();
        Invalidate_SortedSQL_RowData();
    }

    private void Invalidate_Filterinfo() {
        if (_database == null) { return; }

        foreach (var thisColumn in _database.Column.Where(thisColumn => thisColumn != null)) {
            thisColumn.TmpIfFilterRemoved = null;
            thisColumn.TmpAutoFilterSinnvoll = null;
        }
    }

    private void Invalidate_SortedSQL_RowData() {
        _sortedSQL_RowData = null;
        Invalidate();
    }

    private bool IsOnScreen(SQL_ColumnViewItem? viewItem, Rectangle displayRectangleWoSlider) {
        if (viewItem == null) { return false; }
        if (_design is BlueTableAppearance.Standard or BlueTableAppearance.OnlyMainColumnWithoutHead) {
            if (viewItem.OrderTmpSpalteX1 + Column_DrawWidth(viewItem, displayRectangleWoSlider) >= 0 && viewItem.OrderTmpSpalteX1 <= displayRectangleWoSlider.Width) { return true; }
        } else {
            Develop.DebugPrint(_design);
        }
        return false;
    }

    private bool IsOnScreen(SQL_ColumnItem? column, SQL_RowData? row, Rectangle displayRectangleWoSlider) => IsOnScreen(CurrentArrangement[column], displayRectangleWoSlider) && IsOnScreen(row, displayRectangleWoSlider);

    private bool IsOnScreen(SQL_RowData? vrow, Rectangle displayRectangleWoSlider) => vrow != null && DrawY(vrow) + vrow.DrawHeight >= HeadSize() && DrawY(vrow) <= displayRectangleWoSlider.Height;

    /// <summary>
    /// Berechnet die Zelle, ausgehend von einer Zellenposition. Dabei wird die Columns und Zeilensortierung berücksichtigt.
    /// Gibt des keine Nachbarszelle, wird die Eingangszelle zurückgegeben.
    /// </summary>
    /// <remarks></remarks>
    private void Neighbour(SQL_ColumnItem? column, SQL_RowData? row, Direction direction, out SQL_ColumnItem? newColumn, out SQL_RowData? newRow) {
        newColumn = column;
        newRow = row;
        if (_design == BlueTableAppearance.OnlyMainColumnWithoutHead) {
            if (direction is not Direction.Oben and not Direction.Unten) {
                newColumn = _database.Column[0];
                return;
            }
        }
        if (newColumn != null) {
            if (Convert.ToBoolean(direction & Direction.Links)) {
                if (CurrentArrangement.PreviousVisible(newColumn) != null) {
                    newColumn = CurrentArrangement.PreviousVisible(newColumn);
                }
            }
            if (Convert.ToBoolean(direction & Direction.Rechts)) {
                if (CurrentArrangement.NextVisible(newColumn) != null) {
                    newColumn = CurrentArrangement.NextVisible(newColumn);
                }
            }
        }
        if (newRow != null) {
            if (Convert.ToBoolean(direction & Direction.Oben)) {
                if (View_PreviousRow(newRow) != null) { newRow = View_PreviousRow(newRow); }
            }
            if (Convert.ToBoolean(direction & Direction.Unten)) {
                if (View_NextRow(newRow) != null) { newRow = View_NextRow(newRow); }
            }
        }
    }

    private void OnAutoFilterClicked(SQL_FilterEventArgs e) => AutoFilterClicked?.Invoke(this, e);

    private void OnButtonCellClicked(SQL_CellEventArgs e) => ButtonCellClicked?.Invoke(this, e);

    private void OnCellValueChanged(SQL_CellEventArgs e) => CellValueChanged?.Invoke(this, e);

    private void OnCellValueChangingByUser(CellValueChangingByUserEventArgs ed) => CellValueChangingByUser?.Invoke(this, ed);

    private void OnColumnArrangementChanged() => ColumnArrangementChanged?.Invoke(this, System.EventArgs.Empty);

    private void OnEditBeforeBeginEdit(SQL_CellCancelEventArgs e) => EditBeforeBeginEdit?.Invoke(this, e);

    private void OnFilterChanged() => FilterChanged?.Invoke(this, System.EventArgs.Empty);

    //private void OnEditBeforeNewRow(RowCancelEventArgs e) => EditBeforeNewRow?.Invoke(this, e);
    private void OnNeedButtonArgs(ButtonCellEventArgs e) => NeedButtonArgs?.Invoke(this, e);

    private void OnPinnedChanged() => PinnedChanged?.Invoke(this, System.EventArgs.Empty);

    private void OnRowAdded(object sender, SQL_RowEventArgs e) => RowAdded?.Invoke(sender, e);

    private void OnSelectedCellChanged(SQL_CellExtEventArgs e) => SelectedCellChanged?.Invoke(this, e);

    private void OnSelectedRowChanged(SQL_RowEventArgs e) => SelectedRowChanged?.Invoke(this, e);

    private void OnSQL_DatabaseChanged() => DatabaseChanged?.Invoke(this, System.EventArgs.Empty);

    private void OnViewChanged() {
        ViewChanged?.Invoke(this, System.EventArgs.Empty);
        Invalidate_SortedSQL_RowData(); // evtl. muss [Neue Zeile] ein/ausgebelndet werden
    }

    private void OnVisibleRowsChanged() => VisibleRowsChanged?.Invoke(this, System.EventArgs.Empty);

    /// <summary>
    /// Reset - Parse - CheckView
    /// </summary>
    /// <param name="toParse"></param>
    private void ParseView(string toParse) {
        ResetView();

        if (!string.IsNullOrEmpty(toParse)) {
            foreach (var pair in toParse.GetAllTags()) {
                switch (pair.Key) {
                    case "arrangementnr":
                        Arrangement = IntParse(pair.Value);
                        break;

                    case "filters":
                        Filter = new SQL_FilterCollection(_database, pair.Value);
                        Filter.Changed += Filter_Changed;
                        OnFilterChanged();
                        break;

                    case "sliderx":
                        SliderX.Maximum = Math.Max(SliderX.Maximum, IntParse(pair.Value));
                        SliderX.Value = IntParse(pair.Value);
                        break;

                    case "slidery":
                        SliderY.Maximum = Math.Max(SliderY.Maximum, IntParse(pair.Value));
                        SliderY.Value = IntParse(pair.Value);
                        break;

                    case "cursorpos":
                        SQL_Database.Cell.DataOfCellKey(pair.Value, out var column, out var row);
                        CursorPos_Set(column, SortedRows().Get(row), false);
                        break;

                    case "tempsort":
                        _sortDefinitionTemporary = new SQL_RowSortDefinition(_database, pair.Value);
                        break;

                    case "pin":
                        PinnedRows.Add(_database.Row.SearchByKey(LongParse(pair.Value)));
                        break;

                    case "collapsed":
                        _collapsed.Add(pair.Value.FromNonCritical());
                        break;

                    case "reduced":
                        var c = _database.Column.SearchByKey(LongParse(pair.Value));
                        var cv = CurrentArrangement[c];
                        if (cv != null) { cv.TmpReduced = true; }
                        break;

                    default:
                        Develop.DebugPrint(FehlerArt.Warnung, "Tag unbekannt: " + pair.Key);
                        break;
                }
            }
        }

        CheckView();
    }

    private int Row_DrawHeight(SQL_RowItem? vrow, Rectangle displayRectangleWoSlider) {
        if (_design == BlueTableAppearance.OnlyMainColumnWithoutHead) { return Cell_ContentSize(this, _database.Column[0], vrow, _cellFont, _pix16).Height; }
        var tmp = _pix18;
        if (CurrentArrangement == null) { return tmp; }

        foreach (var thisViewItem in CurrentArrangement) {
            if (thisViewItem?.Column != null && !vrow.CellIsNullOrEmpty(thisViewItem.Column)) {
                tmp = Math.Max(tmp, Cell_ContentSize(this, thisViewItem.Column, vrow, _cellFont, _pix16).Height);
            }
        }
        tmp = Math.Min(tmp, (int)(displayRectangleWoSlider.Height * 0.9) - HeadSize());
        tmp = Math.Max(tmp, _pix18);
        return tmp;
    }

    private void Row_RowRemoving(object sender, SQL_RowEventArgs e) {
        if (e.Row == _cursorPosRow?.Row) { CursorPos_Reset(); }
        if (e.Row == _mouseOverRow?.Row) { _mouseOverRow = null; }
        if (PinnedRows.Contains(e.Row)) { PinnedRows.Remove(e.Row); }
    }

    private string RowCaptionOnCoordinate(int pixelX, int pixelY) {
        try {
            var s = SortedRows();
            foreach (var thisRow in s) {
                if (thisRow.ShowCap && thisRow.CaptionPos is var r) {
                    if (r.Contains(pixelX, pixelY)) { return thisRow.Chapter; }
                }
            }
        } catch { }
        return string.Empty;
    }

    private SQL_RowData? RowOnCoordinate(int pixelY) {
        if (_database == null || pixelY <= HeadSize()) { return null; }
        var s = SortedRows();
        if (s == null) { return null; }

        return s.FirstOrDefault(thisSQL_RowItem => thisSQL_RowItem != null && pixelY >= DrawY(thisSQL_RowItem) && pixelY <= DrawY(thisSQL_RowItem) + thisSQL_RowItem.DrawHeight && thisSQL_RowItem.Expanded);
    }

    private void SliderSchaltenSenk(Rectangle displayR, int maxY) {
        SliderY.Minimum = 0;
        SliderY.Maximum = Math.Max(maxY - displayR.Height + 1 + HeadSize(), 0);
        SliderY.LargeChange = displayR.Height - HeadSize();
        SliderY.Enabled = Convert.ToBoolean(SliderY.Maximum > 0);
    }

    private void SliderSchaltenWaage(Rectangle displayR, int maxX) {
        SliderX.Minimum = 0;
        SliderX.Maximum = maxX - displayR.Width + 1;
        SliderX.LargeChange = displayR.Width;
        SliderX.Enabled = Convert.ToBoolean(SliderX.Maximum > 0);
    }

    private void SliderX_ValueChanged(object sender, System.EventArgs e) {
        if (_database == null) { return; }
        lock (_lockUserAction) {
            //SQL_Database.OnConnectedControlsStopAllWorking(this, new MultiUserFileStopWorkingEventArgs());
            Invalidate();
        }
    }

    private void SliderY_ValueChanged(object sender, System.EventArgs e) {
        if (_database == null) { return; }
        lock (_lockUserAction) {
            //SQL_Database.OnConnectedControlsStopAllWorking(this, new MultiUserFileStopWorkingEventArgs());
            Invalidate();
        }
    }

    private SQL_RowSortDefinition? SortUsed() => _sortDefinitionTemporary?.Columns != null
        ? _sortDefinitionTemporary
        : _database.SortDefinition?.Columns != null ? _database.SortDefinition : null;

    private void TXTBox_Close(TextBox? textbox) {
        if (textbox == null || _database == null || _database.IsDisposed) { return; }
        if (!textbox.Visible) { return; }
        if (textbox.Tag == null || string.IsNullOrEmpty(textbox.Tag.ToString())) {
            textbox.Visible = false;
            return; // Ohne dem hier wird ganz am Anfang ein Ereignis ausgelöst
        }
        var w = textbox.Text;
        var tmpTag = (string)textbox.Tag;
        _database.Cell.DataOfCellKey(tmpTag, out var column, out var row);
        textbox.Tag = null;
        textbox.Visible = false;
        UserEdited(this, w, column, row, _cursorPosRow?.Chapter, true);
        Focus();
    }

    private bool UserEdit_NewRowAllowed() {
        if (_database == null || _database.Column.Count == 0 || _database.Column[0] == null) { return false; }
        if (_design == BlueTableAppearance.OnlyMainColumnWithoutHead) { return false; }
        if (_database.ColumnArrangements.Count == 0) { return false; }
        if (CurrentArrangement?[_database.Column[0]] == null) { return false; }
        if (!_database.PermissionCheck(_database.PermissionGroupsNewRow, null)) { return false; }

        if (_database.PowerEdit.Subtract(DateTime.Now).TotalSeconds > 0) { return true; }

        return SQL_CellCollection.UserEditPossible(_database.Column[0], null, ErrorReason.EditNormaly);
    }

    #endregion

    // QickInfo beisst sich mit den letzten Änderungen Quickinfo//DialogBoxes.QuickInfo.Show("<IMAGECODE=Stift|16||1> " + Reason);
}