// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueBasics.MultiUserFile;
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Extended_Text;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using BlueDatabase.Interfaces;
using static BlueBasics.Converter;
using static BlueBasics.IO;
using Clipboard = System.Windows.Clipboard;
using MessageBox = BlueControls.Forms.MessageBox;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
[DefaultEvent("SelectedRowChanged")]
[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public partial class Table : GenericControl, IContextMenu, IBackgroundNone, ITranslateable, IHasDatabase, IControlAcceptFilter, IControlSendRow, ICalculateRows {

    #region Fields

    public static readonly int AutoFilterSize = 22;
    public static readonly SolidBrush BrushYellowTransparent = new(Color.FromArgb(180, 255, 255, 0));
    public static readonly int ColumnCaptionSizeY = 22;
    public static readonly Pen PenRed1 = new(Color.Red, 1);
    public static readonly int RowCaptionSizeY = 50;
    private readonly List<IControlAcceptRow> _childs = new();
    private readonly List<string> _collapsed = new();
    private readonly List<IControlSendFilter> _getFilterFrom = new();
    private readonly object _lockUserAction = new();
    private int _arrangementNr = 1;
    private AutoFilter? _autoFilter;
    private BlueFont? _cellFont;
    private BlueFont? _chapterFont;
    private BlueFont? _columnFilterFont;
    private BlueFont? _columnFont;
    private DateTime? _databaseDrawError;
    private BlueTableAppearance _design = BlueTableAppearance.Standard;
    private bool _drawing = false;
    private FilterCollection? _filter;
    private List<RowItem>? _filteredRows;
    private FilterCollection? _filterFromParents = null;
    private int? _headSize;
    private bool _isinClick;
    private bool _isinDoubleClick;
    private bool _isinKeyDown;
    private bool _isinMouseDown;
    private bool _isinMouseEnter;
    private bool _isinMouseLeave;
    private bool _isinMouseMove;
    private bool _isinMouseWheel;
    private bool _isinSizeChanged;
    private bool _isinVisibleChanged;

    /// <summary>
    ///  Wird DatabaseAdded gehandlet?
    /// </summary>
    private ColumnItem? _mouseOverColumn;

    private RowData? _mouseOverRow;

    private string _mouseOverText = string.Empty;

    private BlueFont? _newRowFont;

    private Progressbar? _pg;

    private int _pix16 = 16;

    private int _pix18 = 18;

    private int _rowCaptionFontY = 26;

    private SearchAndReplace? _searchAndReplace;

    private bool _showNumber;

    private RowSortDefinition? _sortDefinitionTemporary;

    private List<RowData>? _sortedRowData;

    private string _storedView = string.Empty;

    private Rectangle _tmpCursorRect = Rectangle.Empty;

    private RowItem? _unterschiede;

    private int _wiederHolungsSpaltenWidth;

    #endregion

    #region Constructors

    public Table() : base(true, false) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.

        InitializeSkin();
        MouseHighlight = false;
    }

    #endregion

    #region Events

    public event EventHandler<FilterEventArgs>? AutoFilterClicked;

    public event EventHandler<CellEditBlockReasonEventArgs>? BlockEdit;

    public event EventHandler<CellEventArgs>? ButtonCellClicked;

    public event EventHandler<CellEventArgs>? CellValueChanged;

    public event EventHandler<ContextMenuInitEventArgs>? ContextMenuInit;

    public event EventHandler<ContextMenuItemClickedEventArgs>? ContextMenuItemClicked;

    public event EventHandler? DatabaseChanged;

    public new event EventHandler<CellEventArgs>? DoubleClick;

    public event EventHandler? FilterChanged;

    public event EventHandler<ButtonCellEventArgs>? NeedButtonArgs;

    public event EventHandler? PinnedChanged;

    public event EventHandler<RowEventArgs>? RowAdded;

    public event EventHandler<CellExtEventArgs>? SelectedCellChanged;

    public event EventHandler<RowEventArgs>? SelectedRowChanged;

    public event EventHandler? ViewChanged;

    public event EventHandler? VisibleRowsChanged;

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
                CursorPos_Set(CursorPosColumn, CursorPosRow, true);
            }
        }
    }

    public ColumnViewCollection? CurrentArrangement {
        get {
            if (Database?.ColumnArrangements == null || Database.ColumnArrangements.Count <= _arrangementNr) {
                return null;
            }

            return Database.ColumnArrangements[_arrangementNr];
        }
    }

    public ColumnItem? CursorPosColumn { get; private set; }

    public RowData? CursorPosRow { get; private set; }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public DatabaseAbstract? Database { get; private set; } // DatabaseSet benutzen!

    [DefaultValue(BlueTableAppearance.Standard)]
    public BlueTableAppearance Design {
        get => _design;
        set {
            SliderY.Visible = true;
            SliderX.Visible = value == BlueTableAppearance.Standard;
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

    public FilterCollection? Filter {
        get {
            if (Database == null || Database.IsDisposed) { return null; }
            if (_filter == null) { Filter = new FilterCollection(Database); }

            return _filter;
        }
        private set {
            if (value == null && _filter == null) { return; }

            if ((value == null && _filter != null) ||
                (_filter == null && value != null) ||
                (value!.ToString(true) != _filter!.ToString(true))) {
                if (_filter != null) {
                    _filter.Changed -= Filter_Changed;
                    _filter.CollectionChanged -= Filter_CollectionChanged;
                }

                _filter = value;

                if (_filter != null) {
                    _filter.Changed += Filter_Changed;
                    _filter.CollectionChanged += Filter_CollectionChanged;
                }

                OnFilterChanged();
            }
        }
    }

    public List<RowItem> FilteredRows {
        get {
            if (_filterFromParents == null) {
                return this.CalculateFilteredRows(ref _filteredRows, Filter, Database);
            }

            if (Filter == null) {
                return this.CalculateFilteredRows(ref _filteredRows, _filterFromParents, Database);
            }

            var f = new FilterCollection(Database);
            f.AddIfNotExists(Filter);
            f.AddIfNotExists(_filterFromParents);
            return this.CalculateFilteredRows(ref _filteredRows, f, Database);
        }
    }

    [DefaultValue(1.0f)]
    public double FontScale => Database?.GlobalScale ?? 1f;

    public ReadOnlyCollection<IControlSendFilter> GetFilterFrom => new(_getFilterFrom);

    public DatabaseAbstract? InputDatabase { get => Database; set => DatabaseSet(value, string.Empty); }

    public DatabaseAbstract? OutputDatabase { get => Database; set => DatabaseSet(value, string.Empty); }

    public List<RowItem> PinnedRows { get; } = new();

    public DateTime PowerEdit {
        //private get => _database?.PowerEdit;
        set {
            if (Database == null || Database.IsDisposed) { return; }
            Database.PowerEdit = value;
            Invalidate_sortedRowData(); // Neue Zeilen können nun erlaubt sein
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
    public RowSortDefinition? SortDefinitionTemporary {
        get => _sortDefinitionTemporary;
        set {
            if (_sortDefinitionTemporary != null && value != null && _sortDefinitionTemporary.ToString() == value.ToString()) { return; }
            if (_sortDefinitionTemporary == value) { return; }
            _sortDefinitionTemporary = value;
            _Database_SortParameterChanged(this, System.EventArgs.Empty);
        }
    }

    [DefaultValue(true)]
    public bool Translate { get; set; } = true;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public RowItem? Unterschiede {
        get => _unterschiede;
        set {
            //if (_Unterschiede != null && value != null && _sortDefinitionTemporary.ToString(false) == value.ToString(false)) { return; }
            if (_unterschiede == value) { return; }
            _unterschiede = value;
            Invalidate();
        }
    }

    #endregion

    #region Methods

    public static int CalculateColumnContentWidth(Table table, ColumnItem column, BlueFont? cellFont, int pix16) {
        if (column.FixedColumnWidth > 0) { return column.FixedColumnWidth; }
        if (column.ContentWidthIsValid) { return column.ContentWidth; }

        if (column.Database == null) { return 16; }

        column.RefreshColumnsData();

        var newContentWidth = 16; // Wert muss gesetzt werden, dass er am ende auch gespeichert wird

        try {
            if (column.Format == DataFormat.Button) {
                // Beim Button reicht eine Abfrage mit Row null
                newContentWidth = Cell_ContentSize(table, column, null, cellFont, pix16).Width;
            } else {
                _ = Parallel.ForEach(column.Database.Row, thisRowItem => {
                    //var originalText = thisRowItem.CellGetString(column);

                    if (thisRowItem != null && !thisRowItem.CellIsNullOrEmpty(column)) {
                        //var (s, quickImage) = CellItem.GetDrawingData(column, originalText, style, bildTextverhalten);

                        var wx = Cell_ContentSize(table, column, thisRowItem, cellFont, pix16).Width;

                        newContentWidth = Math.Max(newContentWidth, wx);
                    }
                });
            }
        } catch {
            Develop.CheckStackForOverflow();
            return CalculateColumnContentWidth(table, column, cellFont, pix16);
        }

        column.ContentWidth = newContentWidth;
        return newContentWidth;
    }

    public static Size Cell_ContentSize(Table? table, ColumnItem column, RowItem row, BlueFont? cellFont, int pix16) {
        if (column?.Database == null || row == null || column.Database.IsDisposed) { return new Size(pix16, pix16); }

        if (column.Format == DataFormat.Verknüpfung_zu_anderer_Datenbank) {
            var (lcolumn, lrow, _, _) = CellCollection.LinkedCellData(column, row, false, false);
            return lcolumn != null && lrow != null ? Cell_ContentSize(table, lcolumn, lrow, cellFont, pix16)
                                                   : new Size(pix16, pix16);
        }

        var contentSizex = column.Database.Cell.GetSizeOfCellContent(column, row);
        if (contentSizex != null) { return (Size)contentSizex; }

        var contentSize = Size.Empty;

        if (column.Format == DataFormat.Button) {
            ButtonCellEventArgs e = new(column, row);
            table?.OnNeedButtonArgs(e);
            contentSize = Button.StandardSize(e.Text, e.Image);
        } else if (column.MultiLine) {
            var tmp = SplitMultiLine(column.Database.Cell.GetString(column, row));
            if (column.ShowMultiLineInOneLine) {
                contentSize = FormatedText_NeededSize(column, tmp.JoinWith("; "), cellFont, ShortenStyle.Replaced, pix16, column.BehaviorOfImageAndText);
            } else {
                foreach (var thisString in tmp) {
                    var tmpSize = FormatedText_NeededSize(column, thisString, cellFont, ShortenStyle.Replaced, pix16, column.BehaviorOfImageAndText);
                    contentSize.Width = Math.Max(tmpSize.Width, contentSize.Width);
                    contentSize.Height += Math.Max(tmpSize.Height, pix16);
                }
            }
        } else {
            var @string = column.Database.Cell.GetString(column, row);
            contentSize = FormatedText_NeededSize(column, @string, cellFont, ShortenStyle.Replaced, pix16, column.BehaviorOfImageAndText);
        }
        contentSize.Width = Math.Max(contentSize.Width, pix16);
        contentSize.Height = Math.Max(contentSize.Height, pix16);
        column.Database.Cell.SetSizeOfCellContent(column, row, contentSize);
        return contentSize;
    }

    public static void CopyToClipboard(ColumnItem? column, RowItem? row, bool meldung) {
        try {
            if (row != null && column != null && column.Format.CanBeCheckedByRules()) {
                var c = row.CellGetString(column);
                c = c.Replace("\r\n", "\r");
                c = c.Replace("\r", "\r\n");
                _ = Generic.CopytoClipboard(c);
                if (meldung) { Notification.Show(LanguageTool.DoTranslate("<b>{0}</b><br>ist nun in der Zwischenablage.", true, c), ImageCode.Kopieren); }
            } else {
                if (meldung) { Notification.Show(LanguageTool.DoTranslate("Bei dieser Zelle nicht möglich."), ImageCode.Warnung); }
            }
        } catch {
            if (meldung) { Notification.Show(LanguageTool.DoTranslate("Unerwarteter Fehler beim Kopieren."), ImageCode.Warnung); }
        }
    }

    public static string Database_NeedPassword() => InputBox.Show("Bitte geben sie das Passwort ein,<br>um Zugriff auf diese Datenbank<br>zu erhalten:", string.Empty, FormatHolder.Text);

    public static void DoUndo(ColumnItem? column, RowItem? row) {
        if (column == null || column.IsDisposed) { return; }
        if (row == null || row.IsDisposed) { return; }
        if (column.Format == DataFormat.Verknüpfung_zu_anderer_Datenbank) {
            var (lcolumn, lrow, _, _) = CellCollection.LinkedCellData(column, row, true, false);
            if (lcolumn != null && lrow != null) { DoUndo(lcolumn, lrow); }
            return;
        }
        var cellKey = CellCollection.KeyOfCell(column, row);
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
        //Database.Cell.Set(column, row, v[0].Substring(5), false);
        //_ = row.ExecuteScript(EventTypes.value_changedx, string.Empty, true, true, true, 5);
        //row.Database?.AddBackgroundWork(row);
        row.Database?.Row.ExecuteValueChanged();
    }

    public static void Draw_FormatedText(Graphics gr, string text, ColumnItem? column, Rectangle fitInRect, Design design, States state, ShortenStyle style, BildTextVerhalten bildTextverhalten) {
        if (string.IsNullOrEmpty(text)) { return; }
        var d = Skin.DesignOf(design, state);

        Draw_CellTransparentDirect(gr, text, fitInRect, d.BFont, column, 16, style, bildTextverhalten, state);
    }

    /// <summary>
    /// Status des Bildes (Disabled) wird geändert. Diese Routine sollte nicht innerhalb der Table Klasse aufgerufen werden.
    /// Sie dient nur dazu, das Aussehen eines Textes wie eine Zelle zu imitieren.
    /// </summary>
    public static Size FormatedText_NeededSize(ColumnItem? column, string originalText, BlueFont? font, ShortenStyle style, int minSize, BildTextVerhalten bildTextverhalten) {
        var (s, quickImage) = CellItem.GetDrawingData(column, originalText, style, bildTextverhalten);
        return Skin.FormatedText_NeededSize(s, quickImage, font, minSize);
    }

    //    FileDialogs.DeleteFile(delList, true);
    //    return newFiles;
    //}
    public static void ImportCsv(DatabaseAbstract database, string csvtxt) {
        using Import x = new(database, csvtxt);
        _ = x.ShowDialog();
        x?.Dispose();
    }

    public static void SearchNextText(string searchTxt, Table tableView, ColumnItem? column, RowData? row, out ColumnItem? foundColumn, out RowData? foundRow, bool vereinfachteSuche) {
        if (tableView.Database is not DatabaseAbstract db) {
            MessageBox.Show("Datenbank-Fehler.", ImageCode.Information, "OK");
            foundColumn = null;
            foundRow = null;
            return;
        }

        searchTxt = searchTxt.Trim();
        var ca = tableView.CurrentArrangement;
        if (tableView.Design == BlueTableAppearance.OnlyMainColumnWithoutHead) {
            ca = db.ColumnArrangements[0];
        }

        if (ca == null) {
            MessageBox.Show("Datenbank-Ansichts-Fehler.", ImageCode.Information, "OK");
            foundColumn = null;
            foundRow = null;
            return;
        }

        row ??= tableView.View_RowLast();
        column ??= ca.Last()?.Column;
        var rowsChecked = 0;
        if (string.IsNullOrEmpty(searchTxt)) {
            MessageBox.Show("Bitte Text zum Suchen eingeben.", ImageCode.Information, "OK");
            foundColumn = null;
            foundRow = null;
            return;
        }

        do {
            column = tableView.Design == BlueTableAppearance.OnlyMainColumnWithoutHead ?
                        db.ColumnArrangements[0].NextVisible(column) :
                        ca.NextVisible(column);

            if (column == null || column.IsDisposed) {
                column = ca.First()?.Column;
                if (rowsChecked > tableView.Database.Row.Count() + 1) {
                    foundColumn = null;
                    foundRow = null;
                    return;
                }
                rowsChecked++;
                row = tableView.View_NextRow(row) ?? tableView.View_RowFirst();
            }
            var contentHolderCellColumn = column;
            var contenHolderCellRow = row?.Row;
            if (column != null && column.Format == DataFormat.Verknüpfung_zu_anderer_Datenbank) {
                (contentHolderCellColumn, contenHolderCellRow, _, _) = CellCollection.LinkedCellData(column, row?.Row, false, false);
            }
            var ist1 = string.Empty;
            var ist2 = string.Empty;
            if (contenHolderCellRow != null && contentHolderCellColumn != null) {
                ist1 = contenHolderCellRow.CellGetString(contentHolderCellColumn);
                ist2 = CellItem.ValuesReadable(contentHolderCellColumn, contenHolderCellRow, ShortenStyle.Both).JoinWithCr();
            }
            if (contentHolderCellColumn != null && contentHolderCellColumn.FormatierungErlaubt) {
                ExtText l = new(Enums.Design.TextBox, States.Standard) {
                    HtmlText = ist1
                };
                ist1 = l.PlainText;
            }
            // Allgemeine Prüfung
            if (!string.IsNullOrEmpty(ist1) && ist1.ToLower().Contains(searchTxt.ToLower())) {
                foundColumn = column;
                foundRow = row;
                return;
            }
            // Prüfung mit und ohne Ersetzungen / Prefix / Suffix
            if (!string.IsNullOrEmpty(ist2) && ist2.ToLower().Contains(searchTxt.ToLower())) {
                foundColumn = column;
                foundRow = row;
                return;
            }
            if (vereinfachteSuche) {
                var ist3 = ist2.StarkeVereinfachung(" ,");
                var searchTxt3 = searchTxt.StarkeVereinfachung(" ,");
                if (!string.IsNullOrEmpty(ist3) && ist3.ToLower().Contains(searchTxt3.ToLower())) {
                    foundColumn = column;
                    foundRow = row;
                    return;
                }
            }
        } while (true);
    }

    //        newFiles.GenerateAndAdd(neu);
    //        delList.GenerateAndAdd(thisf);
    //    }
    public static BlueControls.ItemCollectionList.ItemCollectionList UndoItems(DatabaseAbstract? db, string cellkey) {
        BlueControls.ItemCollectionList.ItemCollectionList i = new(BlueListBoxAppearance.KontextMenu, false) {
            CheckBehavior = CheckBehavior.AlwaysSingleSelection
        };

        if (db is DatabaseAbstract database) {
            database.GetUndoCache();

            if (database.Undo.Count == 0) { return i; }

            var isfirst = true;
            TextListItem? las = null;
            var lasNr = -1;
            var co = 0;
            for (var z = database.Undo.Count - 1; z >= 0; z--) {
                if (database.Undo[z].CellKey == cellkey) {
                    co++;
                    lasNr = z;
                    if (isfirst) {
                        las = new TextListItem(
                            "Aktueller Text - ab " + database.Undo[z].DateTimeUtc + " UTC, geändert von " +
                            database.Undo[z].User, "Cancel", null, false, true, string.Empty);
                    } else {
                        las = new TextListItem(
                            "ab " + database.Undo[z].DateTimeUtc + " UTC, geändert von " + database.Undo[z].User,
                            co.ToString(Constants.Format_Integer5) + database.Undo[z].ChangedTo, null, false, true,
                            string.Empty);
                    }
                    isfirst = false;
                    i.Add(las);
                }
            }

            if (las != null) {
                co++;
                _ = i.Add("vor " + database.Undo[lasNr].DateTimeUtc + " UTC",
                    co.ToString(Constants.Format_Integer5) + database.Undo[lasNr].PreviousValue);
            }
        }

        return i;
    }

    public static void WriteColumnArrangementsInto(ComboBox? columnArrangementSelector, DatabaseAbstract? database, int showingNo) {
        //if (InvokeRequired) {
        //    Invoke(new Action(() => WriteColumnArrangementsInto(columnArrangementSelector)));
        //    return;
        //}
        if (columnArrangementSelector != null) {
            columnArrangementSelector.Item.Clear();
            columnArrangementSelector.Enabled = false;
            columnArrangementSelector.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        if (database == null || columnArrangementSelector == null) {
            if (columnArrangementSelector != null) {
                columnArrangementSelector.Enabled = false;
                columnArrangementSelector.Text = string.Empty;
            }
            return;
        }

        foreach (var thisArrangement in database.ColumnArrangements) {
            if (thisArrangement != null) {
                if (columnArrangementSelector != null && database.PermissionCheck(thisArrangement.PermissionGroups_Show, null)) {
                    _ = columnArrangementSelector.Item.Add(thisArrangement.KeyName, database.ColumnArrangements.IndexOf(thisArrangement).ToString());
                }
            }
        }

        if (columnArrangementSelector != null) {
            columnArrangementSelector.Enabled = columnArrangementSelector.Item.Count > 1;
            columnArrangementSelector.Text = columnArrangementSelector.Item.Count > 0 ? showingNo.ToString() : string.Empty;
        }
    }

    public void AddGetFilterFrom(IControlSendFilter item) {
        _getFilterFrom.AddIfNotExists(item);
        FilterFromParentsChanged();
        item.ChildAdd(this);
    }

    public void CheckView() {
        if (_arrangementNr != 1) {
            if (Database?.ColumnArrangements == null || _arrangementNr >= Database.ColumnArrangements.Count || CurrentArrangement == null || !Database.PermissionCheck(CurrentArrangement.PermissionGroups_Show, null)) {
                _arrangementNr = 1;
            }
        }

        if (_mouseOverColumn != null && _mouseOverColumn.Database != Database) { _mouseOverColumn = null; }
        if (_mouseOverRow?.Row != null && _mouseOverRow?.Row.Database != Database) { _mouseOverRow = null; }
        if (CursorPosColumn != null && CursorPosColumn.Database != Database) { CursorPosColumn = null; }
        if (CursorPosRow?.Row != null && CursorPosRow?.Row.Database != Database) { CursorPosRow = null; }
    }

    public void ChildAdd(IControlAcceptRow c) {
        if (IsDisposed) { return; }
        _childs.AddIfNotExists(c);
        this.DoChilds(_childs, CursorPosRow?.Row);
    }

    public void CollapesAll() {
        if (Database?.Column.SysChapter is not ColumnItem sc) { return; }

        _collapsed.Clear();
        if (Database != null && !Database.IsDisposed && sc != null) {
            _collapsed.AddRange(sc.Contents());
        }
        Invalidate_sortedRowData();
    }

    public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e) => false;

    public void CursorPos_Reset() => CursorPos_Set(null, null, false);

    public void CursorPos_Set(ColumnItem? column, RowData? row, bool ensureVisible) {
        if (Database == null || row == null || column == null ||
            Database.ColumnArrangements.Count == 0 || CurrentArrangement?[column] == null ||
            SortedRows() is not List<RowData> s || !s.Contains(row)) {
            column = null;
            row = null;
        }

        var sameRow = CursorPosRow == row;

        if (CursorPosColumn == column && CursorPosRow == row) { return; }
        _mouseOverText = string.Empty;
        CursorPosColumn = column;
        CursorPosRow = row;

        if (CursorPosRow?.Row is not RowItem setedrow) { return; }
        if (CursorPosColumn != column) { return; }

        if (ensureVisible) { _ = EnsureVisible(CursorPosColumn, CursorPosRow); }
        Invalidate();

        OnSelectedCellChanged(new CellExtEventArgs(CursorPosColumn, CursorPosRow));

        if (!sameRow) {
            OnSelectedRowChanged(new RowEventArgs(setedrow));
            this.DoChilds(_childs, setedrow);
        }
    }

    public void DatabaseSet(DatabaseAbstract? value, string viewCode) {
        if (Database == value && string.IsNullOrEmpty(viewCode)) { return; }

        CloseAllComponents();

        if (Database != null && !Database.IsDisposed) {
            // auch Disposed Datenbanken die Bezüge entfernen!
            Database.Cell.CellValueChanged -= _Database_CellValueChanged;
            Database.Loaded -= _Database_DatabaseLoaded;
            Database.Loading -= _Database_StoreView;
            Database.ViewChanged -= _Database_ViewChanged;
            //_Database.RowKeyChanged -= _Database_RowKeyChanged;
            //_Database.ColumnKeyChanged -= _Database_ColumnKeyChanged;
            Database.Column.ColumnInternalChanged -= _Database_ColumnContentChanged;
            Database.SortParameterChanged -= _Database_SortParameterChanged;
            Database.Row.RowRemoving -= Row_RowRemoving;
            Database.Row.RowRemoved -= _Database_RowRemoved;
            Database.Row.RowAdded -= _Database_Row_RowAdded;
            Database.Column.ColumnRemoving -= Column_ItemRemoving;
            Database.Column.ColumnRemoved -= _Database_ViewChanged;
            Database.Column.ColumnAdded -= _Database_ViewChanged;
            Database.ProgressbarInfo -= _Database_ProgressbarInfo;
            Database.Disposing -= _Database_Disposing;
            DatabaseAbstract.ForceSaveAll();
            MultiUserFile.ForceLoadSaveAll();
            //_database.Save(false);         // Datenbank nicht reseten, weil sie ja anderweitig noch benutzt werden kann
        }
        ShowWaitScreen = true;
        Refresh(); // um die Uhr anzuzeigen
        Database = value;
        _databaseDrawError = null;
        InitializeSkin(); // Neue Schriftgrößen
        if (Database != null && !Database.IsDisposed) {
            Database.Cell.CellValueChanged += _Database_CellValueChanged;
            Database.Loaded += _Database_DatabaseLoaded;
            Database.Loading += _Database_StoreView;
            Database.ViewChanged += _Database_ViewChanged;
            //_Database.RowKeyChanged += _Database_RowKeyChanged;
            //_Database.ColumnKeyChanged += _Database_ColumnKeyChanged;
            Database.Column.ColumnInternalChanged += _Database_ColumnContentChanged;
            Database.SortParameterChanged += _Database_SortParameterChanged;
            Database.Row.RowRemoving += Row_RowRemoving;
            Database.Row.RowRemoved += _Database_RowRemoved;
            Database.Row.RowAdded += _Database_Row_RowAdded;
            Database.Column.ColumnAdded += _Database_ViewChanged;
            Database.Column.ColumnRemoving += Column_ItemRemoving;
            Database.Column.ColumnRemoved += _Database_ViewChanged;
            Database.ProgressbarInfo += _Database_ProgressbarInfo;
            Database.Disposing += _Database_Disposing;
        }

        ParseView(viewCode);
        //ResetView();
        //CheckView();

        ShowWaitScreen = false;
        OnDatabaseChanged();
    }

    public void Draw_Column_Head(Graphics gr, ColumnViewItem viewItem, Rectangle displayRectangleWoSlider, int lfdNo) {
        if (!IsOnScreen(viewItem, displayRectangleWoSlider)) { return; }
        if (_design == BlueTableAppearance.OnlyMainColumnWithoutHead) { return; }
        if (_columnFont == null) { return; }

        if (viewItem.Column != null) {
            if (viewItem.OrderTmpSpalteX1 != null) {
                gr.FillRectangle(new SolidBrush(viewItem.Column.BackColor), (int)viewItem.OrderTmpSpalteX1, 0, Column_DrawWidth(viewItem, displayRectangleWoSlider), HeadSize());
                Draw_Border(gr, viewItem, displayRectangleWoSlider, true);
                gr.FillRectangle(new SolidBrush(Color.FromArgb(100, 200, 200, 200)), (int)viewItem.OrderTmpSpalteX1, 0, Column_DrawWidth(viewItem, displayRectangleWoSlider), HeadSize());

                var down = 0;
                if (!string.IsNullOrEmpty(viewItem.Column.CaptionGroup3)) {
                    down = ColumnCaptionSizeY * 3;
                } else if (!string.IsNullOrEmpty(viewItem.Column.CaptionGroup2)) {
                    down = ColumnCaptionSizeY * 2;
                } else if (!string.IsNullOrEmpty(viewItem.Column.CaptionGroup1)) {
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
                FilterItem? filtIt = null;
                if (Filter != null) {
                    filtIt = Filter[viewItem.Column];
                }

                if (viewItem.Column.AutoFilterSymbolPossible()) {
                    if (filtIt != null) {
                        trichterState = States.Checked;
                        var anz = Autofilter_Text(viewItem.Column);
                        trichterText = anz > -100 ? (anz * -1).ToString() : "∞";
                    } else {
                        trichterState = States.Standard;
                    }
                }

                var trichterSize = (AutoFilterSize - 4).ToString();
                if (filtIt != null) {
                    trichterIcon = QuickImage.Get("Trichter|" + trichterSize + "|||FF0000");
                } else if (Filter != null && Filter.MayHasRowFilter(viewItem.Column)) {
                    trichterIcon = QuickImage.Get("Trichter|" + trichterSize + "|||227722");
                } else if (viewItem.Column.AutoFilterSymbolPossible()) {
                    trichterIcon = QuickImage.Get("Trichter|" + trichterSize);
                }

                if (trichterState != States.Undefiniert) {
                    Skin.Draw_Back(gr, Enums.Design.Button_AutoFilter, trichterState, viewItem.TmpAutoFilterLocation, null, false);
                    Skin.Draw_Border(gr, Enums.Design.Button_AutoFilter, trichterState, viewItem.TmpAutoFilterLocation);
                }

                if (trichterIcon != null) {
                    gr.DrawImage(trichterIcon, viewItem.TmpAutoFilterLocation.Left + 2, viewItem.TmpAutoFilterLocation.Top + 2);
                }

                if (!string.IsNullOrEmpty(trichterText)) {
                    if (_columnFilterFont != null) {
                        var s = _columnFilterFont.MeasureString(trichterText, StringFormat.GenericDefault);

                        _columnFilterFont.DrawString(gr, trichterText,
                            viewItem.TmpAutoFilterLocation.Left + ((AutoFilterSize - s.Width) / 2),
                            viewItem.TmpAutoFilterLocation.Top + ((AutoFilterSize - s.Height) / 2));
                    }
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

                if (!string.IsNullOrEmpty(viewItem.Column.CaptionBitmapCode) && viewItem.Column.TmpCaptionBitmapCode == null) {
                    viewItem.Column.TmpCaptionBitmapCode = QuickImage.Get(viewItem.Column.CaptionBitmapCode + "|100");
                }

                #endregion

                if (viewItem.Column.TmpCaptionBitmapCode != null && !viewItem.Column.TmpCaptionBitmapCode.IsError) {

                    #region Spalte mit Bild zeichnen

                    Point pos = new(
                        (int)viewItem.OrderTmpSpalteX1 +
                        (int)((Column_DrawWidth(viewItem, displayRectangleWoSlider) - fs.Width) / 2.0), 3 + down);
                    gr.DrawImageInRectAspectRatio(viewItem.Column.TmpCaptionBitmapCode, (int)viewItem.OrderTmpSpalteX1 + 2, (int)(pos.Y + fs.Height), Column_DrawWidth(viewItem, displayRectangleWoSlider) - 4, HeadSize() - (int)(pos.Y + fs.Height) - 6 - 18);
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
                if (tmpSortDefinition != null && (tmpSortDefinition.UsedForRowSort(viewItem.Column) || viewItem.Column == Database?.Column.SysChapter)) {
                    if (viewItem.OrderTmpSpalteX1 != null) {
                        if (tmpSortDefinition.Reverse) {
                            gr.DrawImage(QuickImage.Get("ZA|11|5||||50"), (float)(viewItem.OrderTmpSpalteX1 + (Column_DrawWidth(viewItem, displayRectangleWoSlider) / 2.0) - 6), HeadSize() - 6 - AutoFilterSize);
                        } else {
                            gr.DrawImage(QuickImage.Get("AZ|11|5||||50"), (float)(viewItem.OrderTmpSpalteX1 + (Column_DrawWidth(viewItem, displayRectangleWoSlider) / 2.0) - 6), HeadSize() - 6 - AutoFilterSize);
                        }
                    }
                }
            }
        }

        #endregion
    }

    public string EditableErrorReason(ColumnItem? cellInThisDatabaseColumn, RowData? cellInThisDatabaseRow, EditableErrorReasonType mode, bool checkUserRights, bool checkEditmode, bool maychangeview) {
        var f = CellCollection.EditableErrorReason(cellInThisDatabaseColumn, cellInThisDatabaseRow?.Row, mode, checkUserRights, checkEditmode);
        if (!string.IsNullOrWhiteSpace(f)) { return f; }

        if (checkEditmode) {
            if (Design == BlueTableAppearance.OnlyMainColumnWithoutHead) {
                return "In dieser Ansicht kann der Eintrag nicht bearbeitet werden.";
            }

            if (CurrentArrangement is not ColumnViewCollection ca || ca[cellInThisDatabaseColumn] is not ColumnViewItem viewItem) {
                return "Ansicht veraltet";
            }

            if (cellInThisDatabaseRow != null) {
                if (maychangeview && !EnsureVisible(cellInThisDatabaseColumn, cellInThisDatabaseRow)) {
                    return "Zelle konnte nicht angezeigt werden.";
                }
                if (!IsOnScreen(cellInThisDatabaseColumn, cellInThisDatabaseRow, DisplayRectangle)) {
                    return "Die Zelle wird nicht angezeigt.";
                }
            } else {
                if (maychangeview && !EnsureVisible(viewItem)) {
                    return "Zelle konnte nicht angezeigt werden.";
                }
                //if (!IsOnScreen(viewItem, DisplayRectangle)) {
                //    return "Die Zelle wird nicht angezeigt.";
                //}
            }
        }

        CellEditBlockReasonEventArgs ed = new(cellInThisDatabaseColumn, cellInThisDatabaseRow?.Row, string.Empty);
        OnBlockEdit(ed);
        return ed.BlockReason;
    }

    public bool EnsureVisible(ColumnItem? column, RowData? row) {
        var ok1 = EnsureVisible(CurrentArrangement?[column]);
        var ok2 = EnsureVisible(row);
        return ok1 && ok2;
    }

    public void ExpandAll() {
        if (Database?.Column.SysChapter is null) { return; }

        _collapsed.Clear();
        CursorPos_Reset(); // Wenn eine Zeile markiert ist, man scrollt und expandiert, springt der Screen zurück, was sehr irriteiert
        Invalidate_sortedRowData();
    }

    public string Export_CSV(FirstRow firstRow) => Database == null ? string.Empty : Database.Export_CSV(firstRow, CurrentArrangement, SortedRows());

    public string Export_CSV(FirstRow firstRow, ColumnItem onlyColumn) {
        if (Database == null || Database.IsDisposed) { return string.Empty; }
        List<ColumnItem> l = new() { onlyColumn };
        return Database.Export_CSV(firstRow, l, SortedRows());
    }

    public void Export_HTML(string filename = "", bool execute = true) {
        if (Database == null || Database.IsDisposed) { return; }
        if (string.IsNullOrEmpty(filename)) { filename = TempFile(string.Empty, string.Empty, "html"); }
        Database.Export_HTML(filename, CurrentArrangement, SortedRows(), execute);
    }

    public void FilterFromParentsChanged() {
        Invalidate_FilteredRows();
        _filterFromParents = this.FilterOfSender();
        OnFilterChanged();
    }

    /// <summary>
    /// Alle gefilteren Zeilen. Jede Zeile ist maximal einmal in dieser Liste vorhanden. Angepinnte Zeilen addiert worden
    /// </summary>
    /// <returns></returns>
    public new void Focus() {
        if (Focused()) { return; }
        _ = base.Focus();
    }

    public new bool Focused() => base.Focused || SliderY.Focused() || SliderX.Focused() || BTB.Focused || BCB.Focused;

    public void GetContextMenuItems(MouseEventArgs? e, ItemCollectionList.ItemCollectionList items, out object? hotItem, ref bool cancel, ref bool translate) {
        hotItem = null;
        if (e == null) { return; }

        CellOnCoordinate(e.X, e.Y, out _mouseOverColumn, out _mouseOverRow);
        hotItem = CellCollection.KeyOfCell(_mouseOverColumn, _mouseOverRow?.Row);
    }

    public void ImportClipboard() {
        Develop.DebugPrint_InvokeRequired(InvokeRequired, false);
        if (!Clipboard.ContainsText()) {
            Notification.Show("Abbruch,<br>kein Text im Zwischenspeicher!", ImageCode.Information);
            return;
        }

        var nt = Clipboard.GetText();
        ImportCsv(nt);
    }

    public void ImportCsv(string csvtxt) {
        if (Database == null || Database.IsDisposed) { return; }
        ImportCsv(Database, csvtxt);
    }

    public void Invalidate_AllColumnArrangements() {
        if (Database == null || Database.IsDisposed) { return; }

        foreach (var thisArrangement in Database.ColumnArrangements) {
            thisArrangement?.Invalidate_DrawWithOfAllItems();
        }
    }

    public void Invalidate_FilteredRows() {
        _filteredRows = null;
        ////CursorPos_Reset(); // Gibt Probleme bei Formularen, wenn die Key-Spalte geändert wird. Mal abgesehen davon macht es einen Sinn, den Cursor proforma zu löschen, dass soll der RowSorter übernehmen.
        Invalidate_Filterinfo();
        Invalidate_sortedRowData();
        Invalidate();
    }

    public void Invalidate_HeadSize() {
        if (_headSize != null) { Invalidate(); }
        _headSize = null;
    }

    public bool Mouse_IsInHead() => MousePos().Y <= HeadSize();

    public void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);

    public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) => ContextMenuItemClicked?.Invoke(this, e);

    public void OpenSearchAndReplace() {
        if (TableView.ErrorMessage(Database, EditableErrorReasonType.EditGeneral) || Database == null) { return; }

        if (!Database.IsAdministrator()) { return; }

        if (_searchAndReplace == null || _searchAndReplace.IsDisposed || !_searchAndReplace.Visible) {
            _searchAndReplace = new SearchAndReplace(this);
            _searchAndReplace.Show();
        }
    }

    public void Pin(List<RowItem>? rows) {
        // Arbeitet mit Rows, weil nur eine Anpinngug möglich ist

        rows ??= new List<RowItem>();

        rows = rows.Distinct().ToList();
        if (!rows.IsDifferentTo(PinnedRows)) { return; }

        PinnedRows.Clear();
        PinnedRows.AddRange(rows);
        Invalidate_sortedRowData();
        OnPinnedChanged();
    }

    public void PinAdd(RowItem? row) {
        if (row == null || row.IsDisposed) { return; }
        PinnedRows.Add(row);
        Invalidate_sortedRowData();
        OnPinnedChanged();
    }

    public void PinRemove(RowItem? row) {
        if (row == null || row.IsDisposed) { return; }
        _ = PinnedRows.Remove(row);
        Invalidate_sortedRowData();
        OnPinnedChanged();
    }

    public void ResetView() {
        Filter = null;

        PinnedRows.Clear();
        _collapsed.Clear();

        _mouseOverText = string.Empty;
        _sortDefinitionTemporary = null;
        _mouseOverColumn = null;
        _mouseOverRow = null;
        CursorPosColumn = null;
        CursorPosRow = null;
        _arrangementNr = 1;
        _unterschiede = null;

        Invalidate_AllColumnArrangements();
        Invalidate_HeadSize();
        Invalidate_FilteredRows();
        OnViewChanged();
        Invalidate();
    }

    public List<RowData>? SortedRows() {
        if (_sortedRowData != null) { return _sortedRowData; }

        try {
            var displayR = DisplayRectangleWithoutSlider();
            var maxY = 0;
            if (UserEdit_NewRowAllowed()) { maxY += _pix18; }
            var expanded = true;
            var lastCap = string.Empty;

            List<RowData> sortedRowDataNew;
            if (Database == null || Database.IsDisposed) {
                sortedRowDataNew = new List<RowData>();
            } else {
                sortedRowDataNew = Database.Row.CalculateSortedRows(FilteredRows, SortUsed(), PinnedRows, _sortedRowData);
            }

            if (_sortedRowData != null && !_sortedRowData.IsDifferentTo(sortedRowDataNew)) { return _sortedRowData; }

            var sortedRowDataTmp = new List<RowData>();

            foreach (var thisRow in sortedRowDataNew) {
                var thisRowData = thisRow;

                if (_mouseOverRow != null && _mouseOverRow.Row == thisRow.Row && _mouseOverRow.Chapter == thisRow.Chapter) {
                    _mouseOverRow.GetDataFrom(thisRowData);
                    thisRowData = _mouseOverRow;
                } // Mouse-Daten wiederverwenden

                if (CursorPosRow?.Row != null && CursorPosRow.Row == thisRow.Row && CursorPosRow.Chapter == thisRow.Chapter) {
                    CursorPosRow.GetDataFrom(thisRowData);
                    thisRowData = CursorPosRow;
                } // Cursor-Daten wiederverwenden

                thisRowData.Y = maxY;

                #region Caption bestimmen

                if (thisRow.Chapter != lastCap) {
                    thisRowData.Y += RowCaptionSizeY;
                    expanded = !_collapsed.Contains(thisRowData.Chapter);
                    maxY += RowCaptionSizeY;
                    thisRowData.ShowCap = true;
                    lastCap = thisRow.Chapter;
                } else {
                    thisRowData.ShowCap = false;
                }

                #endregion

                #region Expaned (oder pinned) bestimmen

                thisRowData.Expanded = expanded;
                if (thisRowData.Expanded) {
                    thisRowData.DrawHeight = Row_DrawHeight(thisRowData.Row, displayR);
                    //if (_sortedRowDatax == null) {
                    //    // Folgender Fall: Die Row height reparirt die LinkedCell.
                    //    // Dadruch wird eine Zelle geändert. Das wiederrum kann _sortedRowDatax auf null setzen..
                    //    return null;
                    //}
                    maxY += thisRowData.DrawHeight;
                }

                #endregion

                sortedRowDataTmp.Add(thisRowData);
            }

            if (CursorPosRow?.Row != null && !sortedRowDataTmp.Contains(CursorPosRow)) { CursorPos_Reset(); }
            _mouseOverRow = null;

            #region Slider berechnen

            SliderSchaltenSenk(displayR, maxY);

            #endregion

            _sortedRowData = sortedRowDataTmp;

            //         _ = EnsureVisible(CursorPosColumn, CursorPosRow);
            // EnsureVisible macht probleme, wenn mit der Maus auf ein linked Cell gezogen wird und eine andere Cell
            //markiert ist. Dann wird sortiren angestoßen, und der Cursor zurück gesprungen

            OnVisibleRowsChanged();

            return SortedRows(); // Rekursiver aufruf. Manchmal funktiniert OnRowsSorted nicht...
        } catch {
            // Komisch, manchmal wird die Variable _sortedRowDatax verworfen.
            Develop.CheckStackForOverflow();
            Invalidate_sortedRowData();
            return SortedRows();
        }
    }

    public ColumnItem? View_ColumnFirst() {
        if (Database == null || Database.IsDisposed) { return null; }
        var s = CurrentArrangement;

        return s == null || s.Count == 0 ? null : s[0]?.Column;
    }

    public RowData? View_NextRow(RowData? row) {
        if (Database == null || Database.IsDisposed) { return null; }
        var rowNr = SortedRows()?.IndexOf(row) ?? -1;
        return rowNr < 0 || rowNr >= SortedRows().Count - 1 ? null : SortedRows()[rowNr + 1];
    }

    public RowData? View_PreviousRow(RowData? row) {
        if (Database == null || Database.IsDisposed) { return null; }
        var rowNr = SortedRows()?.IndexOf(row) ?? -1;
        return rowNr < 1 ? null : SortedRows()[rowNr - 1];
    }

    public RowData? View_RowFirst() {
        if (Database == null || Database.IsDisposed) { return null; }
        var s = SortedRows();
        return s == null || s.Count == 0 ? null : s[0];
    }

    public RowData? View_RowLast() {
        if (Database == null || Database.IsDisposed) { return null; }
        var s = SortedRows();
        return s == null || s.Count == 0 ? null : s[s.Count - 1];
    }

    public string ViewToString() {
        var x = new List<string>();
        x.ParseableAdd("ArrangementNr", _arrangementNr);
        x.ParseableAdd("Filters", (IStringable?)Filter);
        x.ParseableAdd("SliderX", SliderX.Value);
        x.ParseableAdd("SliderY", SliderY.Value);
        x.ParseableAdd("Pin", PinnedRows);
        x.ParseableAdd("Collapsed", _collapsed);
        x.ParseableAdd("Reduced", CurrentArrangement?.ReducedColumns());
        x.ParseableAdd("TempSort", _sortDefinitionTemporary);
        x.ParseableAdd("CursorPos", CellCollection.KeyOfCell(CursorPosColumn, CursorPosRow?.Row));
        return x.Parseable();
    }

    public List<RowItem> VisibleUniqueRows() {
        var l = new List<RowItem>();

        if (Database == null || Database.IsDisposed) { return l; }

        var f = FilteredRows;
        var lockMe = new object();
        _ = Parallel.ForEach(Database.Row, thisRowItem => {
            if (thisRowItem != null) {
                if (f.Contains(thisRowItem) || PinnedRows.Contains(thisRowItem)) {
                    lock (lockMe) { l.Add(thisRowItem); }
                }
            }
        });

        return l;
        //return Database.Row.CalculateVisibleRows(Filter, PinnedRows);
    }

    protected override void DrawControl(Graphics gr, States state) {
        if (InvokeRequired) {
            _ = Invoke(new Action(() => DrawControl(gr, state)));
            return;
        }

        if (_databaseDrawError is DateTime dt) {
            if (DateTime.UtcNow.Subtract(dt).TotalSeconds < 60) {
                DrawWaitScreen(gr);
                return;
            }
            _databaseDrawError = null;
        }

        _tmpCursorRect = Rectangle.Empty;

        // Listboxen bekommen keinen Focus, also Tabellen auch nicht. Basta.
        if (state.HasFlag(States.Standard_HasFocus)) {
            state ^= States.Standard_HasFocus;
        }

        if (Database == null || DesignMode || ShowWaitScreen || _drawing) {
            DrawWaitScreen(gr);
            return;
        }

        lock (_lockUserAction) {
            _drawing = true;
            //if (_InvalidExternal) { FillExternalControls(); }
            if (state.HasFlag(States.Standard_Disabled)) { CursorPos_Reset(); }
            var displayRectangleWoSlider = DisplayRectangleWithoutSlider();
            // Haupt-Aufbau-Routine ------------------------------------
            //gr.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            if (!ComputeAllColumnPositions()) {
                DrawWaitScreen(gr);
                _drawing = false;
                return;
            }

            List<RowData>? sortedRowData;
            int firstVisibleRow;
            int lastVisibleRow;
            var count = 0;
            do {
                count++;
                //if (count > 10) {
                //    DrawWaitScreen(gr);
                //    FormWithStatusBar.UpdateStatusBar(FehlerArt.Warnung, "Datenbank-laden nach 10 Versuchen aufgegeben", true);
                //    _databaseDrawError = DateTime.UtcNow;
                //    _drawing = false;
                //    return;
                //}

                sortedRowData = SortedRows();

                if (sortedRowData == null) {
                    // Multitasking...
                    Develop.CheckStackForOverflow();
                    DrawControl(gr, state);
                    _drawing = false;
                    return;
                }

                firstVisibleRow = sortedRowData.Count;
                lastVisibleRow = -1;
                var rowsToRefreh = new List<RowItem>();
                List<RowItem> sortedRows = new();

                foreach (var thisRow in sortedRowData) {
                    if (thisRow?.Row is RowItem r) {
                        if (IsOnScreen(thisRow, displayRectangleWoSlider)) {
                            _ = rowsToRefreh.AddIfNotExists(r);
                            var T = sortedRowData.IndexOf(thisRow);
                            firstVisibleRow = Math.Min(T, firstVisibleRow);
                            lastVisibleRow = Math.Max(T, lastVisibleRow);
                        }

                        if (sortedRows.Count < 300) {
                            // 300 reichen, sonst dauerts so lange
                            _ = sortedRows.AddIfNotExists(r);
                        }
                    }
                }

                (bool didreload, string errormessage) = Database.RefreshRowData(rowsToRefreh, false, sortedRows);

                if (!string.IsNullOrEmpty(errormessage)) {
                    FormWithStatusBar.UpdateStatusBar(FehlerArt.Warnung, errormessage, true);
                }

                if (!didreload || count > 15) { break; }
                Invalidate_sortedRowData();
            } while (true);

            switch (_design) {
                case BlueTableAppearance.Standard:
                    Draw_Table_Std(gr, sortedRowData, state, displayRectangleWoSlider, firstVisibleRow, lastVisibleRow, CurrentArrangement);
                    break;

                case BlueTableAppearance.OnlyMainColumnWithoutHead:
                    Draw_Table_ListboxStyle(gr, sortedRowData, state, displayRectangleWoSlider, firstVisibleRow, lastVisibleRow);
                    break;

                default:
                    Develop.DebugPrint(_design);
                    break;
            }
            _drawing = false;
        }
    }

    protected void InitializeSkin() {
        _cellFont = Skin.GetBlueFont(Enums.Design.Table_Cell, States.Standard).Scale(FontScale);
        _columnFont = Skin.GetBlueFont(Enums.Design.Table_Column, States.Standard).Scale(FontScale);
        _chapterFont = Skin.GetBlueFont(Enums.Design.Table_Cell_Chapter, States.Standard).Scale(FontScale);
        _columnFilterFont = BlueFont.Get(_columnFont.FontName, _columnFont.FontSize, false, false, false, false, true, Color.White, Color.Red, false, false, false);

        _newRowFont = Skin.GetBlueFont(Enums.Design.Table_Cell_New, States.Standard).Scale(FontScale);
        if (Database != null && !Database.IsDisposed) {
            _pix16 = GetPix(16, _cellFont, Database.GlobalScale);
            _pix18 = GetPix(18, _cellFont, Database.GlobalScale);
            _rowCaptionFontY = GetPix(26, _cellFont, Database.GlobalScale);
        } else {
            _pix16 = 16;
            _pix18 = 18;
            _rowCaptionFontY = 26;
        }
    }

    protected override bool IsInputKey(Keys keyData) =>
            // Ganz wichtig diese Routine!
            // Wenn diese NICHT ist, geht der Fokus weg, sobald der cursor gedrückt wird.
            keyData switch {
                Keys.Up or Keys.Down or Keys.Left or Keys.Right => true,
                _ => false
            };

    protected override void OnClick(System.EventArgs e) {
        base.OnClick(e);
        if (Database == null || Database.IsDisposed) { return; }
        lock (_lockUserAction) {
            if (_isinClick) { return; }
            _isinClick = true;
            //Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
            CellOnCoordinate(MousePos().X, MousePos().Y, out _mouseOverColumn, out _mouseOverRow);
            _isinClick = false;
        }
    }

    protected override void OnDoubleClick(System.EventArgs e) {
        //    base.OnDoubleClick(e); Wird komplett selbst gehandlet und das neue Ereigniss ausgelöst
        if (Database == null || Database.IsDisposed) { return; }
        lock (_lockUserAction) {
            if (_isinDoubleClick) { return; }
            _isinDoubleClick = true;
            CellOnCoordinate(MousePos().X, MousePos().Y, out _mouseOverColumn, out _mouseOverRow);
            CellEventArgs ea = new(_mouseOverColumn, _mouseOverRow?.Row);
            DoubleClick?.Invoke(this, ea);

            if (!Mouse_IsInHead()) {
                if (_mouseOverRow != null || MousePos().Y <= HeadSize() + _pix18) {
                    DoubleClick?.Invoke(this, ea);
                    Cell_Edit(_mouseOverColumn, _mouseOverRow, true);
                }
            }
            _isinDoubleClick = false;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e) {
        base.OnKeyDown(e);

        if (Database == null || Database.IsDisposed) { return; }

        lock (_lockUserAction) {
            if (_isinKeyDown) { return; }
            _isinKeyDown = true;

            //_database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());

            switch (e.KeyCode) {
                case Keys.Oemcomma: // normales ,
                    if (e.Modifiers == Keys.Control) {
                        var lp = EditableErrorReason(CursorPosColumn, CursorPosRow, EditableErrorReasonType.EditGeneral, true, false, true);
                        Neighbour(CursorPosColumn, CursorPosRow, Direction.Oben, out _, out var newRow);
                        if (newRow == CursorPosRow) { lp = "Das geht nicht bei dieser Zeile."; }
                        if (string.IsNullOrEmpty(lp) && newRow?.Row != null) {
                            UserEdited(this, newRow?.Row.CellGetString(CursorPosColumn), CursorPosColumn, CursorPosRow, true);
                        } else {
                            NotEditableInfo(lp);
                        }
                    }
                    break;

                case Keys.X:
                    if (e.Modifiers == Keys.Control) {
                        CopyToClipboard(CursorPosColumn, CursorPosRow?.Row, true);
                        if (CursorPosRow?.Row is null || CursorPosRow.Row.CellIsNullOrEmpty(CursorPosColumn)) {
                            _isinKeyDown = false;
                            return;
                        }
                        var l2 = EditableErrorReason(CursorPosColumn, CursorPosRow, EditableErrorReasonType.EditGeneral, true, false, true);
                        if (string.IsNullOrEmpty(l2)) {
                            UserEdited(this, string.Empty, CursorPosColumn, CursorPosRow, true);
                        } else {
                            NotEditableInfo(l2);
                        }
                    }
                    break;

                case Keys.Delete:
                    if (CursorPosRow?.Row is null || CursorPosRow.Row.CellIsNullOrEmpty(CursorPosColumn)) {
                        _isinKeyDown = false;
                        return;
                    }
                    var l = EditableErrorReason(CursorPosColumn, CursorPosRow, EditableErrorReasonType.EditGeneral, true, false, true);
                    if (string.IsNullOrEmpty(l)) {
                        UserEdited(this, string.Empty, CursorPosColumn, CursorPosRow, true);
                    } else {
                        NotEditableInfo(l);
                    }
                    break;

                case Keys.Left:
                    Cursor_Move(Direction.Links);
                    break;

                case Keys.Right:
                    Cursor_Move(Direction.Rechts);
                    break;

                case Keys.Up:
                    Cursor_Move(Direction.Oben);
                    break;

                case Keys.Down:
                    Cursor_Move(Direction.Unten);
                    break;

                case Keys.PageDown:
                    if (SliderY.Enabled) {
                        CursorPos_Reset();
                        SliderY.Value += SliderY.LargeChange;
                    }
                    break;

                case Keys.PageUp: //Bildab
                    if (SliderY.Enabled) {
                        CursorPos_Reset();
                        SliderY.Value -= SliderY.LargeChange;
                    }
                    break;

                case Keys.Home:
                    if (SliderY.Enabled) {
                        CursorPos_Reset();
                        SliderY.Value = SliderY.Minimum;
                    }
                    break;

                case Keys.End:
                    if (SliderY.Enabled) {
                        CursorPos_Reset();
                        SliderY.Value = SliderY.Maximum;
                    }
                    break;

                case Keys.C:
                    if (e.Modifiers == Keys.Control) {
                        CopyToClipboard(CursorPosColumn, CursorPosRow?.Row, true);
                    }
                    break;

                case Keys.F:
                    if (e.Modifiers == Keys.Control) {
                        Search x = new(this);
                        x.Show();
                    }
                    break;

                case Keys.F2:
                    Cell_Edit(CursorPosColumn, CursorPosRow, true);
                    break;

                case Keys.V:
                    if (e.Modifiers == Keys.Control) {
                        if (CursorPosColumn != null && CursorPosRow?.Row != null) {
                            if (!CursorPosColumn.Format.TextboxEditPossible() && CursorPosColumn.Format != DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems) {
                                NotEditableInfo("Die Zelle hat kein passendes Format.");
                                _isinKeyDown = false;
                                return;
                            }
                            if (!Clipboard.ContainsText()) {
                                NotEditableInfo("Kein Text in der Zwischenablage.");
                                _isinKeyDown = false;
                                return;
                            }
                            var ntxt = Clipboard.GetText();
                            if (CursorPosRow?.Row.CellGetString(CursorPosColumn) == ntxt) {
                                _isinKeyDown = false;
                                return;
                            }
                            var l2 = EditableErrorReason(CursorPosColumn, CursorPosRow, EditableErrorReasonType.EditAcut, true, false, true);
                            if (string.IsNullOrEmpty(l2)) {
                                UserEdited(this, ntxt, CursorPosColumn, CursorPosRow, true);
                            } else {
                                NotEditableInfo(l2);
                            }
                        }
                    }
                    break;
            }
            _isinKeyDown = false;
        }
    }

    protected override void OnMouseDown(MouseEventArgs e) {
        base.OnMouseDown(e);
        if (Database == null || Database.IsDisposed) { return; }
        lock (_lockUserAction) {
            if (_isinMouseDown) { return; }
            _isinMouseDown = true;
            //_database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
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
                        _ = _collapsed.Remove(rc);
                    } else {
                        _collapsed.Add(rc);
                    }
                    Invalidate_sortedRowData();
                }
            }
            _ = EnsureVisible(_mouseOverColumn, _mouseOverRow);
            CursorPos_Set(_mouseOverColumn, _mouseOverRow, false);
            _isinMouseDown = false;
        }
    }

    protected override void OnMouseEnter(System.EventArgs e) {
        base.OnMouseEnter(e);
        if (Database == null || Database.IsDisposed) { return; }
        lock (_lockUserAction) {
            if (_isinMouseEnter) { return; }
            _isinMouseEnter = true;
            Forms.QuickInfo.Close();
            _isinMouseEnter = false;
        }
    }

    protected override void OnMouseLeave(System.EventArgs e) {
        base.OnMouseLeave(e);
        if (Database == null || Database.IsDisposed) { return; }
        lock (_lockUserAction) {
            if (_isinMouseLeave) { return; }
            _isinMouseLeave = true;
            Forms.QuickInfo.Close();
            _isinMouseLeave = false;
        }
    }

    protected override void OnMouseMove(MouseEventArgs e) {
        base.OnMouseMove(e);
        lock (_lockUserAction) {
            if (_isinMouseMove) { return; }
            _isinMouseMove = true;
            _mouseOverText = string.Empty;
            CellOnCoordinate(e.X, e.Y, out _mouseOverColumn, out _mouseOverRow);
            if (e.Button != MouseButtons.None) {
                //_database?.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
            } else {
                if (_mouseOverColumn != null && e.Y < HeadSize()) {
                    _mouseOverText = _mouseOverColumn.QuickInfoText(string.Empty);
                } else if (_mouseOverColumn != null && _mouseOverRow != null) {
                    if (_mouseOverColumn.Format.NeedTargetDatabase()) {
                        if (_mouseOverColumn.LinkedDatabase != null) {
                            switch (_mouseOverColumn.Format) {
                                case DataFormat.Verknüpfung_zu_anderer_Datenbank:

                                    var (lcolumn, _, info, _) = CellCollection.LinkedCellData(_mouseOverColumn, _mouseOverRow?.Row, true, false);
                                    if (lcolumn != null) { _mouseOverText = lcolumn.QuickInfoText(_mouseOverColumn.ReadableText() + " bei " + lcolumn.ReadableText() + ":"); }

                                    if (!string.IsNullOrEmpty(info) && _mouseOverColumn.Database.IsAdministrator()) {
                                        if (string.IsNullOrEmpty(_mouseOverText)) { _mouseOverText += "\r\n"; }
                                        _mouseOverText = "Verlinkungs-Status: " + info;
                                    }

                                    break;

                                case DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems:
                                    break;

                                default:
                                    Develop.DebugPrint(_mouseOverColumn.Format);
                                    break;
                            }
                        } else {
                            _mouseOverText = "Verknüpfung zur Ziel-Datenbank fehlerhaft.";
                        }
                    } else if (Database.IsAdministrator()) {
                        _mouseOverText = DatabaseAbstract.UndoText(_mouseOverColumn, _mouseOverRow?.Row);
                    }
                }
                _mouseOverText = _mouseOverText.Trim();
                _mouseOverText = _mouseOverText.Trim("<br>");
                _mouseOverText = _mouseOverText.Trim();
            }
            _isinMouseMove = false;
        }
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        base.OnMouseUp(e);

        if (Database == null || Database.IsDisposed) { return; }

        lock (_lockUserAction) {
            if (Database == null || Database.IsDisposed) {
                Forms.QuickInfo.Close();
                return;
            }
            CellOnCoordinate(e.X, e.Y, out _mouseOverColumn, out _mouseOverRow);
            if (CursorPosColumn != _mouseOverColumn || CursorPosRow != _mouseOverRow) { Forms.QuickInfo.Close(); }
            // TXTBox_Close() NICHT! Weil sonst nach dem öffnen sofort wieder gschlossen wird
            // AutoFilter_Close() NICHT! Weil sonst nach dem öffnen sofort wieder geschlossen wird
            FloatingForm.Close(this, Enums.Design.Form_KontextMenu);

            var viewItem = CurrentArrangement?[_mouseOverColumn];

            if (e.Button == MouseButtons.Left) {
                if (_mouseOverColumn != null) {
                    if (Mouse_IsInAutofilter(viewItem, e)) {
                        var screenX = Cursor.Position.X - e.X;
                        var screenY = Cursor.Position.Y - e.Y;
                        AutoFilter_Show(viewItem, screenX, screenY);
                        return;
                    }

                    if (viewItem != null && Mouse_IsInRedcueButton(viewItem, e)) {
                        viewItem.TmpReduced = !viewItem.TmpReduced;
                        viewItem.TmpDrawWidth = null;
                        Invalidate();
                        return;
                    }

                    if (_mouseOverRow != null && _mouseOverColumn.Format == DataFormat.Button) {
                        OnButtonCellClicked(new CellEventArgs(_mouseOverColumn, _mouseOverRow?.Row));
                        Invalidate();
                    }
                }
            }

            if (e.Button == MouseButtons.Right) {
                FloatingInputBoxListBoxStyle.ContextMenuShow(this, e);
            }
        }
    }

    protected override void OnMouseWheel(MouseEventArgs e) {
        base.OnMouseWheel(e);
        if (Database == null || Database.IsDisposed) { return; }

        lock (_lockUserAction) {
            if (_isinMouseWheel) { return; }
            _isinMouseWheel = true;

            //Database?.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
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
        if (Database == null || Database.IsDisposed) { return; }
        lock (_lockUserAction) {
            if (_isinSizeChanged) { return; }
            _isinSizeChanged = true;
            //Database?.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
            CurrentArrangement?.Invalidate_DrawWithOfAllItems();
            _isinSizeChanged = false;
        }
        Invalidate_sortedRowData(); // Zellen können ihre Größe ändern. z.B. die Zeilenhöhe
    }

    protected override void OnVisibleChanged(System.EventArgs e) {
        base.OnVisibleChanged(e);
        if (Database == null || Database.IsDisposed) { return; }
        lock (_lockUserAction) {
            if (_isinVisibleChanged) { return; }
            _isinVisibleChanged = true;
            //Database?.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
            _isinVisibleChanged = false;
        }
    }

    private static void Draw_CellTransparentDirect(Graphics gr, string? toDraw, Rectangle drawarea, BlueFont? font, ColumnItem? contentHolderCellColumn, int pix16, ShortenStyle style, BildTextVerhalten bildTextverhalten, States state) {
        if (contentHolderCellColumn == null) { return; }

        toDraw ??= string.Empty;

        if (!ShowMultiLine(toDraw, contentHolderCellColumn.MultiLine)) {
            Draw_CellTransparentDirect_OneLine(gr, toDraw, contentHolderCellColumn, drawarea, 0, false, font, pix16, style, bildTextverhalten, state);
        } else {
            var mei = SplitMultiLine(toDraw);
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

    private static void Draw_CellTransparentDirect_OneLine(Graphics gr, string drawString, ColumnItem contentHolderColumnStyle, Rectangle drawarea, int txtYPix, bool changeToDot, BlueFont? font, int pix16, ShortenStyle style, BildTextVerhalten bildTextverhalten, States state) {
        Rectangle r = new(drawarea.Left, drawarea.Top + txtYPix, drawarea.Width, pix16);

        if (r.Bottom + pix16 > drawarea.Bottom) {
            if (r.Bottom > drawarea.Bottom) { return; }
            if (changeToDot) { drawString = "..."; }// Die letzte Zeile noch ganz hinschreiben
        }

        var (text, image) = CellItem.GetDrawingData(contentHolderColumnStyle, drawString, style, bildTextverhalten);
        var tmpImageCode = image;
        if (tmpImageCode != null) { tmpImageCode = QuickImage.Get(tmpImageCode, Skin.AdditionalState(state)); }

        Skin.Draw_FormatedText(gr, text, tmpImageCode, (Alignment)contentHolderColumnStyle.Align, r, null, false, font, false);
    }

    //    if (e.Type is enFehlerArt.DevelopInfo or enFehlerArt.Info) { return; }
    private static int GetPix(int pix, BlueFont? f, double scale) => Skin.FormatedText_NeededSize("@|", null, f, (int)((pix * scale) + 0.5)).Height;

    private static bool Mouse_IsInAutofilter(ColumnViewItem? viewItem, MouseEventArgs e) => viewItem?.Column != null && viewItem.TmpAutoFilterLocation.Width != 0 && viewItem.Column.AutoFilterSymbolPossible() && viewItem.TmpAutoFilterLocation.Contains(e.X, e.Y);

    private static bool Mouse_IsInRedcueButton(ColumnViewItem? viewItem, MouseEventArgs e) => viewItem != null && viewItem.TmpReduceLocation.Width != 0 && viewItem.TmpReduceLocation.Contains(e.X, e.Y);

    private static void NotEditableInfo(string reason) => Notification.Show(LanguageTool.DoTranslate(reason), ImageCode.Kreuz);

    private static bool ShowMultiLine(string txt, bool ml) {
        if (!ml) { return false; }

        if (txt.Contains("\r")) { return true; }
        if (txt.Contains("<br>")) { return true; }

        return false;
    }

    private static string[] SplitMultiLine(string txt) {
        txt = txt.Replace("<br>", "\r", RegexOptions.IgnoreCase);

        return txt.SplitAndCutByCr();
    }

    private static void UserEdited(Table table, string newValue, ColumnItem? cellInThisDatabaseColumn, RowData? cellInThisDatabaseRow, bool formatWarnung) {
        var er = table.EditableErrorReason(cellInThisDatabaseColumn, cellInThisDatabaseRow, EditableErrorReasonType.EditGeneral, true, false, true);
        if (!string.IsNullOrEmpty(er)) { NotEditableInfo(er); return; }

        if (cellInThisDatabaseColumn == null) { return; } // Dummy prüfung

        #region Den wahren Zellkern finden contentHolderCellColumn, contentHolderCellRow

        ColumnItem? contentHolderCellColumn = cellInThisDatabaseColumn;
        RowItem? contentHolderCellRow = cellInThisDatabaseRow?.Row;
        if (cellInThisDatabaseColumn.Format == DataFormat.Verknüpfung_zu_anderer_Datenbank) {
            (contentHolderCellColumn, contentHolderCellRow, _, _) = CellCollection.LinkedCellData(cellInThisDatabaseColumn, cellInThisDatabaseRow?.Row, true, true);
            if (contentHolderCellColumn == null || contentHolderCellRow == null) { return; } // Dummy prüfung
        }

        #endregion

        #region Format prüfen

        if (formatWarnung && !string.IsNullOrEmpty(newValue)) {
            if (!newValue.IsFormat(contentHolderCellColumn)) {
                if (MessageBox.Show("Ihre Eingabe entspricht<br><u>nicht</u> dem erwarteten Format!<br><br>Trotzdem übernehmen?", ImageCode.Information, "Ja", "Nein") != 0) {
                    NotEditableInfo("Abbruch, da das erwartete Format nicht eingehalten wurde.");
                    return;
                }
            }
        }

        #endregion

        #region neue Zeile anlegen? (Das ist niemals in der ein LinkedCell-Datenbank)

        if (cellInThisDatabaseRow == null) {
            if (string.IsNullOrEmpty(newValue)) { return; }

            var fe = table.EditableErrorReason(cellInThisDatabaseColumn.Database?.Column.First(), null, EditableErrorReasonType.EditGeneral, true, false, true);
            if (!string.IsNullOrEmpty(fe)) {
                NotEditableInfo(fe);
                return;
            }

            var newr = cellInThisDatabaseColumn.Database?.Row.GenerateAndAdd(newValue, "Neue Zeile über Tabellen-Ansicht");

            var l = table.FilteredRows;
            if (newr != null && !l.Contains(newr)) {
                if (MessageBox.Show("Die neue Zeile ist ausgeblendet.<br>Soll sie <b>angepinnt</b> werden?", ImageCode.Pinnadel, "anpinnen", "abbrechen") == 0) {
                    table.PinAdd(newr);
                }
            }

            var sr = table.SortedRows();
            var rd = sr.Get(newr);
            table.CursorPos_Set(table.View_ColumnFirst(), rd, true);
            return;
        }

        #endregion

        newValue = contentHolderCellColumn.AutoCorrect(newValue, false);
        if (contentHolderCellRow != null) {
            if (newValue == contentHolderCellRow.CellGetString(contentHolderCellColumn)) { return; }

            var f = CellCollection.EditableErrorReason(contentHolderCellColumn, contentHolderCellRow, EditableErrorReasonType.EditGeneral, true, false);
            if (!string.IsNullOrEmpty(f)) { NotEditableInfo(f); return; }
            contentHolderCellRow.CellSet(contentHolderCellColumn, newValue);

            if (table.Database == cellInThisDatabaseColumn.Database) { table.CursorPos_Set(cellInThisDatabaseColumn, cellInThisDatabaseRow, false); }
        }
    }

    private void _Database_CellValueChanged(object sender, CellEventArgs e) {
        if (_filteredRows != null) {
            var f = Filter;
            var rsd = SortUsed();

            if ((f != null && f[e.Column] != null) ||
                rsd == null ||
               rsd.UsedForRowSort(e.Column) ||
              (f != null && f.MayHasRowFilter(e.Column)) ||
                e.Column == Database?.Column.SysChapter) {
                Invalidate_FilteredRows();
            }
        }
        Invalidate_sortedRowData(); // Zeichenhöhe kann sich ändern...

        Invalidate_DrawWidth(e.Column);

        Invalidate();
        OnCellValueChanged(e);
    }

    private void _Database_ColumnContentChanged(object sender, ColumnEventArgs e) {
        Invalidate_AllColumnArrangements();
        Invalidate_HeadSize();

        // kümmert sich _Database_CellValueChanged darum
        //   Invalidate_sortedRowDatax();
    }

    private void _Database_DatabaseLoaded(object sender, System.EventArgs e) {
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

    private void _Database_Disposing(object sender, System.EventArgs e) => DatabaseSet(null, string.Empty);

    //private void _Database_ColumnKeyChanged(object sender, KeyChangedEventArgs e) {
    //    // Ist aktuell nur möglich,wenn Pending Changes eine neue Zeile machen
    //    if (string.IsNullOrEmpty(_StoredView)) { return; }
    //    _StoredView = ColumnCollection.ChangeKeysInString(_StoredView, e.KeyOld, e.KeyNew);
    //}
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

    private void _Database_Row_RowAdded(object sender, RowEventArgs e) {
        OnRowAdded(sender, e);
        Invalidate_FilteredRows();
    }

    private void _Database_RowRemoved(object sender, System.EventArgs e) => Invalidate_FilteredRows();

    private void _Database_SortParameterChanged(object sender, System.EventArgs e) => Invalidate_sortedRowData();

    private void _Database_StoreView(object sender, System.EventArgs e) =>
            //if (!string.IsNullOrEmpty(_StoredView)) { Develop.DebugPrint("Stored View nicht Empty!"); }
            _storedView = ViewToString();

    private void _Database_ViewChanged(object sender, System.EventArgs e) {
        InitializeSkin(); // Sicher ist sicher, um die neuen Schrift-Größen zu haben.
        Invalidate_HeadSize();
        Invalidate_AllColumnArrangements();
        //Invalidate_RowSort();
        CursorPos_Set(CursorPosColumn, CursorPosRow, true);
        Invalidate();
    }

    private void AutoFilter_Close() {
        if (_autoFilter != null) {
            _autoFilter.FilterComand -= AutoFilter_FilterComand;
            _autoFilter?.Dispose();
            _autoFilter = null;
        }
    }

    private void AutoFilter_FilterComand(object sender, FilterComandEventArgs e) {
        Filter ??= new FilterCollection(Database);

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
                    Filter.Add(new FilterItem(e.Column, FilterType.Istgleich_ODER_GroßKleinEgal, einzigartig));
                    Notification.Show("Die aktuell einzigartigen Einträge wurden berechnet<br>und als <b>ODER-Filter</b> gespeichert.", ImageCode.Trichter);
                } else {
                    Notification.Show("Filterung dieser Spalte gelöscht,<br>da <b>alle Einträge</b> mehrfach vorhanden sind.", ImageCode.Trichter);
                }
                break;

            case "donichteinzigartig":
                Filter.Remove(e.Column);
                e.Column.GetUniques(VisibleUniqueRows(), out _, out var xNichtEinzigartig);
                if (xNichtEinzigartig.Count > 0) {
                    Filter.Add(new FilterItem(e.Column, FilterType.Istgleich_ODER_GroßKleinEgal, xNichtEinzigartig));
                    Notification.Show("Die aktuell <b>nicht</b> einzigartigen Einträge wurden berechnet<br>und als <b>ODER-Filter</b> gespeichert.", ImageCode.Trichter);
                } else {
                    Notification.Show("Filterung dieser Spalte gelöscht,<br>da <b>alle Einträge</b> einzigartig sind.", ImageCode.Trichter);
                }
                break;

            //case "dospaltenvergleich": {
            //        List<RowItem> ro = new();
            //        ro.AddRange(VisibleUniqueRows());

            //        ItemCollectionList ic = new();
            //        foreach (var thisColumnItem in e.Column.Database.Column) {
            //            if (thisColumnItem != null && thisColumnItem != e.Column) { ic.Add(thisColumnItem); }
            //        }
            //        ic.Sort();

            //        var r = InputBoxListBoxStyle.Show("Mit welcher Spalte vergleichen?", ic, AddType.None, true);
            //        if (r == null || r.Count == 0) { return; }

            //        var c = e.Column.Database.Column.Exists(r[0]);

            //        List<string> d = new();
            //        foreach (var thisR in ro) {
            //            if (thisR.CellGetString(e.Column) != thisR.CellGetString(c)) { d.Add(thisR.CellFirstString()); }
            //        }
            //        if (d.Count > 0) {
            //            Filter.Add(new FilterItem(e.Column.Database.Column.First, FilterType.Istgleich_ODER_GroßKleinEgal, d));
            //            Notification.Show("Die aktuell <b>unterschiedlichen</b> Einträge wurden berechnet<br>und als <b>ODER-Filter</b> in der <b>ersten Spalte</b> gespeichert.", ImageCode.Trichter);
            //        } else {
            //            Notification.Show("Keine Filter verändert,<br>da <b>alle Einträge</b> identisch sind.", ImageCode.Trichter);
            //        }
            //        break;
            //    }

            case "doclipboard": {
                    var clipTmp = Clipboard.GetText();
                    clipTmp = clipTmp.RemoveChars(Constants.Char_NotFromClip);
                    clipTmp = clipTmp.TrimEnd("\r\n");
                    var searchValue = new List<string>(clipTmp.SplitAndCutByCr()).SortedDistinctList();
                    Filter.Remove(e.Column);
                    if (searchValue.Count > 0) {
                        Filter.Add(new FilterItem(e.Column, FilterType.IstGleich_ODER | FilterType.GroßKleinEgal, searchValue));
                    }
                    break;
                }

            case "donotclipboard": {
                    var clipTmp = Clipboard.GetText();
                    clipTmp = clipTmp.RemoveChars(Constants.Char_NotFromClip);
                    clipTmp = clipTmp.TrimEnd("\r\n");
                    var columInhalt = Database.Export_CSV(FirstRow.Without, e.Column, null).SplitAndCutByCrToList().SortedDistinctList();
                    columInhalt.RemoveString(clipTmp.SplitAndCutByCrToList().SortedDistinctList(), false);
                    Filter.Remove(e.Column);
                    if (columInhalt.Count > 0) {
                        Filter.Add(new FilterItem(e.Column, FilterType.IstGleich_ODER | FilterType.GroßKleinEgal, columInhalt));
                    }
                    break;
                }
            default:
                Develop.DebugPrint("Unbekannter Comand:   " + e.Comand);
                break;
        }
        OnAutoFilterClicked(new FilterEventArgs(e.Filter));
    }

    private void AutoFilter_Show(ColumnViewItem? columnviewitem, int screenx, int screeny) {
        if (columnviewitem?.Column == null) { return; }
        if (_design == BlueTableAppearance.OnlyMainColumnWithoutHead) { return; }
        if (!columnviewitem.Column.AutoFilterSymbolPossible()) { return; }
        if (Filter.Any(thisFilter => thisFilter != null && thisFilter.Column == columnviewitem.Column && !string.IsNullOrEmpty(thisFilter.Herkunft))) {
            MessageBox.Show("Dieser Filter wurde<br>automatisch gesetzt.", ImageCode.Information, "OK");
            return;
        }
        //Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
        //OnBeforeAutoFilterShow(new ColumnEventArgs(columnviewitem.Column));
        _autoFilter = new AutoFilter(columnviewitem.Column, Filter, PinnedRows);
        _autoFilter.Position_LocateToPosition(new Point(screenx + (int)columnviewitem.OrderTmpSpalteX1, screeny + HeadSize()));
        _autoFilter.Show();
        _autoFilter.FilterComand += AutoFilter_FilterComand;
        Develop.Debugprint_BackgroundThread();
    }

    private int Autofilter_Text(ColumnItem? column) {
        if (column.TmpIfFilterRemoved != null) { return (int)column.TmpIfFilterRemoved; }
        var tfilter = new FilterCollection(column.Database);
        foreach (var thisFilter in Filter) {
            if (thisFilter != null && thisFilter.Column != column) { tfilter.Add(thisFilter); }
        }
        var temp = Database.Row.CalculateFilteredRows(tfilter);
        column.TmpIfFilterRemoved = FilteredRows.Count - temp.Count;
        return (int)column.TmpIfFilterRemoved;
    }

    /// <summary>
    /// Gibt die Anzahl der SICHTBAREN Zeilen zurück, die mehr angezeigt werden würden, wenn dieser Filter deaktiviert wäre.
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    private void BB_Enter(object sender, System.EventArgs e) {
        if (((TextBox)sender).MultiLine) { return; }
        CloseAllComponents();
    }

    //private bool Autofilter_Sinnvoll(ColumnItem? column) {
    //    if (column.TmpAutoFilterSinnvoll != null) { return (bool)column.TmpAutoFilterSinnvoll; }
    //    for (var rowcount = 0; rowcount <= SortedRows().Count - 2; rowcount++) {
    //        if (SortedRows()[rowcount]?.Row.CellGetString(column) != SortedRows()[rowcount + 1]?.Row.CellGetString(column)) {
    //            column.TmpAutoFilterSinnvoll = true;
    //            return true;
    //        }
    //    }
    //    column.TmpAutoFilterSinnvoll = false;
    //    return false;
    //}
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

    private void BTB_NeedDatabaseOfAdditinalSpecialChars(object sender, MultiUserFileGiveBackEventArgs e) => e.File = Database;

    private void Cell_Edit(ColumnItem? cellInThisDatabaseColumn, RowData? cellInThisDatabaseRow, bool preverDropDown) {
        var f = EditableErrorReason(cellInThisDatabaseColumn, cellInThisDatabaseRow, EditableErrorReasonType.EditGeneral, true, true, true);
        if (!string.IsNullOrEmpty(f)) { NotEditableInfo(f); return; }
        if (cellInThisDatabaseColumn == null) { return; }// Klick ins Leere

        ColumnItem? contentHolderCellColumn = cellInThisDatabaseColumn;
        RowItem? contentHolderCellRow = cellInThisDatabaseRow?.Row;
        if (cellInThisDatabaseColumn.Format == DataFormat.Verknüpfung_zu_anderer_Datenbank) {
            (contentHolderCellColumn, contentHolderCellRow, _, _) = CellCollection.LinkedCellData(cellInThisDatabaseColumn, cellInThisDatabaseRow?.Row, true, true);
        }

        if (contentHolderCellColumn == null) {
            NotEditableInfo("Keine Spalte angeklickt.");
            return;
        }

        var dia = ColumnItem.UserEditDialogTypeInTable(contentHolderCellColumn, preverDropDown);

        switch (dia) {
            case EditTypeTable.Textfeld:
                _ = Cell_Edit_TextBox(cellInThisDatabaseColumn, cellInThisDatabaseRow, contentHolderCellColumn, contentHolderCellRow, BTB, 0, 0);
                break;

            case EditTypeTable.Textfeld_mit_Auswahlknopf:
                _ = Cell_Edit_TextBox(cellInThisDatabaseColumn, cellInThisDatabaseRow, contentHolderCellColumn, contentHolderCellRow, BCB, 20, 18);
                break;

            case EditTypeTable.Dropdown_Single:
                Cell_Edit_Dropdown(cellInThisDatabaseColumn, cellInThisDatabaseRow, contentHolderCellColumn, contentHolderCellRow);
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

            case EditTypeTable.Farb_Auswahl_Dialog:
                if (cellInThisDatabaseColumn != contentHolderCellColumn || cellInThisDatabaseRow?.Row != contentHolderCellRow) {
                    NotEditableInfo("Verlinkte Zellen hier verboten.");
                    return;
                }
                Cell_Edit_Color(cellInThisDatabaseColumn, cellInThisDatabaseRow);
                break;

            case EditTypeTable.Font_AuswahlDialog:
                Develop.DebugPrint_NichtImplementiert();
                //if (cellInThisDatabaseColumn != ContentHolderCellColumn || cellInThisDatabaseRow != ContentHolderCellRow)
                //{
                //    NotEditableInfo("Ziel-Spalte ist kein Textformat");
                //    return;
                //}
                //Cell_Edit_Font(cellInThisDatabaseColumn, cellInThisDatabaseRow);
                break;

            case EditTypeTable.WarnungNurFormular:
                NotEditableInfo("Dieser Zelltyp kann nur in einem Formular-Fenster bearbeitet werden");
                break;

            //case EditTypeTable.FileHandling_InDateiSystem:
            //    if (cellInThisDatabaseColumn != contentHolderCellColumn || cellInThisDatabaseRow?.Row != contentHolderCellRow) {
            //        NotEditableInfo("Verlinkte Zellen hier verboten.");
            //        return;
            //    }
            //    Cell_Edit_FileSystem(cellInThisDatabaseColumn, cellInThisDatabaseRow);
            //    break;

            case EditTypeTable.None:
                break;

            default:
                Develop.DebugPrint(dia);
                NotEditableInfo("Unbekannte Bearbeitungs-Methode");
                break;
        }
    }

    private void Cell_Edit_Color(ColumnItem? cellInThisDatabaseColumn, RowData? cellInThisDatabaseRow) {
        ColDia.Color = cellInThisDatabaseRow.Row.CellGetColor(cellInThisDatabaseColumn);
        ColDia.Tag = new List<object?> { cellInThisDatabaseColumn, cellInThisDatabaseRow };
        List<int> colList = new();
        foreach (var thisRowItem in Database.Row) {
            if (thisRowItem != null) {
                if (thisRowItem.CellGetInteger(cellInThisDatabaseColumn) != 0) {
                    colList.Add(thisRowItem.CellGetColorBgr(cellInThisDatabaseColumn));
                }
            }
        }
        colList.Sort();
        ColDia.CustomColors = colList.Distinct().ToArray();
        _ = ColDia.ShowDialog();
        ColDia?.Dispose();
        UserEdited(this, Color.FromArgb(255, ColDia.Color).ToArgb().ToString(), cellInThisDatabaseColumn, cellInThisDatabaseRow, false);
    }

    private void Cell_Edit_Dropdown(ColumnItem? cellInThisDatabaseColumn, RowData? cellInThisDatabaseRow, ColumnItem contentHolderCellColumn, RowItem? contentHolderCellRow) {
        if (cellInThisDatabaseColumn != contentHolderCellColumn) {
            if (contentHolderCellRow == null) {
                NotEditableInfo("Bei Zellverweisen kann keine neue Zeile erstellt werden.");
                return;
            }
            if (cellInThisDatabaseRow == null) {
                NotEditableInfo("Bei Zellverweisen kann keine neue Zeile erstellt werden.");
                return;
            }
        }

        ItemCollectionList.ItemCollectionList t = new(true) {
            Appearance = BlueListBoxAppearance.DropdownSelectbox
        };

        ItemCollectionList.ItemCollectionList.GetItemCollection(t, contentHolderCellColumn, contentHolderCellRow, ShortenStyle.Replaced, 1000);
        if (t.Count == 0) {
            // Hm.. Dropdown kein Wert vorhanden.... also gar kein Dropdown öffnen!
            if (contentHolderCellColumn.TextBearbeitungErlaubt) { Cell_Edit(cellInThisDatabaseColumn, cellInThisDatabaseRow, false); } else {
                NotEditableInfo("Keine Items zum Auswählen vorhanden.");
            }
            return;
        }

        if (contentHolderCellColumn.TextBearbeitungErlaubt) {
            if (t.Count == 1 && cellInThisDatabaseRow.Row.CellIsNullOrEmpty(cellInThisDatabaseColumn)) {
                // Bei nur einem Wert, wenn Texteingabe erlaubt, Dropdown öffnen
                Cell_Edit(cellInThisDatabaseColumn, cellInThisDatabaseRow, false);
                return;
            }
            _ = t.Add("Erweiterte Eingabe", "#Erweitert", QuickImage.Get(ImageCode.Stift), true, Constants.FirstSortChar + "1");
            _ = t.AddSeparator(Constants.FirstSortChar + "2");
            //t.Sort();
        }

        var dropDownMenu = FloatingInputBoxListBoxStyle.Show(t, new CellExtEventArgs(cellInThisDatabaseColumn, cellInThisDatabaseRow), this, Translate);
        dropDownMenu.ItemClicked += DropDownMenu_ItemClicked;
        Develop.Debugprint_BackgroundThread();
    }

    private bool Cell_Edit_TextBox(ColumnItem? cellInThisDatabaseColumn, RowData? cellInThisDatabaseRow, ColumnItem contentHolderCellColumn, RowItem? contentHolderCellRow, TextBox box, int addWith, int isHeight) {
        if (contentHolderCellColumn != cellInThisDatabaseColumn) {
            if (contentHolderCellRow == null) {
                NotEditableInfo("Bei Zellverweisen kann keine neue Zeile erstellt werden.");
                return false;
            }
            if (cellInThisDatabaseRow == null) {
                NotEditableInfo("Bei Zellverweisen kann keine neue Zeile erstellt werden.");
                return false;
            }
        }

        var viewItemx = CurrentArrangement[cellInThisDatabaseColumn];

        if (contentHolderCellRow != null) {
            var h = cellInThisDatabaseRow.DrawHeight;// Row_DrawHeight(cellInThisDatabaseRow, DisplayRectangle);
            if (isHeight > 0) { h = isHeight; }
            box.Location = new Point((int)viewItemx.OrderTmpSpalteX1, DrawY(cellInThisDatabaseRow));
            box.Size = new Size(Column_DrawWidth(viewItemx, DisplayRectangle) + addWith, h);
            box.Text = contentHolderCellRow.CellGetString(contentHolderCellColumn);
        } else {
            // Neue Zeile...
            box.Location = new Point((int)viewItemx.OrderTmpSpalteX1, HeadSize());
            box.Size = new Size(Column_DrawWidth(viewItemx, DisplayRectangle) + addWith, _pix18);
            box.Text = string.Empty;
        }

        box.GetStyleFrom(contentHolderCellColumn);

        box.Tag = new List<object?> { cellInThisDatabaseColumn, cellInThisDatabaseRow };

        if (box is ComboBox cbox) {
            ItemCollectionList.ItemCollectionList.GetItemCollection(cbox.Item, contentHolderCellColumn, contentHolderCellRow, ShortenStyle.Replaced, 1000);
            if (cbox.Item.Count == 0) {
                return Cell_Edit_TextBox(cellInThisDatabaseColumn, cellInThisDatabaseRow, contentHolderCellColumn, contentHolderCellRow, BTB, 0, 0);
            }
        }

        //if (string.IsNullOrEmpty(Box.Text)) {
        //    Box.Text = CellCollection.AutomaticInitalValue(contentHolderCellColumn, contentHolderCellRow);
        //}

        box.Visible = true;
        box.BringToFront();
        _ = box.Focus();
        return true;
    }

    private void CellOnCoordinate(int xpos, int ypos, out ColumnItem? column, out RowData? row) {
        column = ColumnOnCoordinate(xpos);
        row = RowOnCoordinate(ypos);
    }

    //private void Cell_Edit_FileSystem(ColumnItem? cellInThisDatabaseColumn, RowData? cellInThisDatabaseRow) {
    //    var l = FileSystem(cellInThisDatabaseColumn);
    //    if (l == null) { return; }
    //    UserEdited(this, l.JoinWithCr(), cellInThisDatabaseColumn, cellInThisDatabaseRow?.Row, cellInThisDatabaseRow?.Chapter, false);
    //}
    private void CloseAllComponents() {
        if (InvokeRequired) {
            _ = Invoke(new Action(CloseAllComponents));
            return;
        }
        if (Database == null || Database.IsDisposed) { return; }
        TXTBox_Close(BTB);
        TXTBox_Close(BCB);
        FloatingForm.Close(this);
        AutoFilter_Close();
        Forms.QuickInfo.Close();
    }

    private int Column_DrawWidth(ColumnViewItem? viewItem, Rectangle displayRectangleWoSlider) {
        // Hier wird die ORIGINAL-Spalte gezeichnet, nicht die FremdZelle!!!!

        if (viewItem?.Column == null) { return 0; }
        if (viewItem.TmpDrawWidth != null) { return (int)viewItem.TmpDrawWidth; }

        if (_design == BlueTableAppearance.OnlyMainColumnWithoutHead) {
            viewItem.TmpDrawWidth = displayRectangleWoSlider.Width - 1;
            return (int)viewItem.TmpDrawWidth;
        }

        if (viewItem.TmpReduced) {
            viewItem.TmpDrawWidth = 16;
        } else {
            viewItem.TmpDrawWidth = viewItem.ViewType == ViewType.PermanentColumn
                ? Math.Min(CalculateColumnContentWidth(this, viewItem.Column, _cellFont, _pix16), (int)(displayRectangleWoSlider.Width * 0.3))
                : Math.Min(CalculateColumnContentWidth(this, viewItem.Column, _cellFont, _pix16), (int)(displayRectangleWoSlider.Width * 0.6));
        }

        viewItem.TmpDrawWidth = Math.Max((int)viewItem.TmpDrawWidth, AutoFilterSize); // Mindestens so groß wie der Autofilter;
        viewItem.TmpDrawWidth = Math.Max((int)viewItem.TmpDrawWidth, (int)ColumnHead_Size(viewItem.Column).Width);
        return (int)viewItem.TmpDrawWidth;
    }

    private void Column_ItemRemoving(object sender, ColumnEventArgs e) {
        if (e.Column == CursorPosColumn) {
            CursorPos_Reset();
        }
        if (e.Column == _mouseOverColumn) {
            _mouseOverColumn = null;
        }
    }

    private SizeF ColumnCaptionText_Size(ColumnItem? column) {
        if (column == null || column.IsDisposed) { return new SizeF(_pix16, _pix16); }

        if (column.TmpCaptionTextSize.Width > 0) { return column.TmpCaptionTextSize; }
        if (_columnFont == null) { return new SizeF(_pix16, _pix16); }
        column.TmpCaptionTextSize = BlueFont.MeasureString(column.Caption.Replace("\r", "\r\n"), _columnFont.Font());
        return column.TmpCaptionTextSize;
    }

    private SizeF ColumnHead_Size(ColumnItem? column) {
        //Bitmap? CaptionBitmapCode = null; // TODO: Caption Bitmap neu erstellen
        //if (CaptionBitmapCode != null && CaptionBitmapCode.Width > 10) {
        //    wi = Math.Max(50, ColumnCaptionText_Size(column).Width + 4);
        //    he = 50 + ColumnCaptionText_Size(column).Height + 3;
        //} else {
        var wi = ColumnCaptionText_Size(column).Height + 4;
        var he = ColumnCaptionText_Size(column).Width + 3;
        //}
        if (column != null && !column.IsDisposed) {
            if (!string.IsNullOrEmpty(column.CaptionGroup3)) {
                he += ColumnCaptionSizeY * 3;
            } else if (!string.IsNullOrEmpty(column.CaptionGroup2)) {
                he += ColumnCaptionSizeY * 2;
            } else if (!string.IsNullOrEmpty(column.CaptionGroup1)) {
                he += ColumnCaptionSizeY;
            }
        }

        return new SizeF(wi, he);
    }

    private ColumnItem? ColumnOnCoordinate(int xpos) {
        if (Database == null || Database.ColumnArrangements.Count - 1 < _arrangementNr) { return null; }
        var ca = CurrentArrangement;
        if (ca == null) { return null; }

        foreach (var thisViewItem in ca) {
            if (thisViewItem?.Column != null) {
                if (xpos >= thisViewItem.OrderTmpSpalteX1 && xpos <= thisViewItem.OrderTmpSpalteX1 + Column_DrawWidth(thisViewItem, DisplayRectangleWithoutSlider())) {
                    return thisViewItem.Column;
                }
            }
        }
        return null;
    }

    private bool ComputeAllColumnPositions() {
        try {
            // Kommt vor, dass spontan doch geparsed wird...
            if (Database?.ColumnArrangements == null || _arrangementNr >= Database.ColumnArrangements.Count) { return false; }

            var ca = CurrentArrangement;
            if (ca == null) { return false; }
            foreach (var thisViewItem in ca) {
                if (thisViewItem != null) {
                    thisViewItem.OrderTmpSpalteX1 = null;
                }
            }

            var sx = SliderX.Value;
            var se = SliderX.Enabled;

            _wiederHolungsSpaltenWidth = 0;
            _mouseOverText = string.Empty;
            var wdh = true;
            var maxX = 0;
            var displayR = DisplayRectangleWithoutSlider();
            // Spalten berechnen
            foreach (var thisViewItem in ca) {
                if (thisViewItem?.Column != null) {
                    if (thisViewItem.ViewType != ViewType.PermanentColumn) { wdh = false; }
                    if (wdh) {
                        thisViewItem.OrderTmpSpalteX1 = maxX;
                        maxX += Column_DrawWidth(thisViewItem, displayR);
                        _wiederHolungsSpaltenWidth = Math.Max(_wiederHolungsSpaltenWidth, maxX);
                    } else {
                        thisViewItem.OrderTmpSpalteX1 = se ? (int)(maxX - sx) : maxX;
                        maxX += Column_DrawWidth(thisViewItem, displayR);
                    }
                }
            }
            SliderSchaltenWaage(displayR, maxX);
        } catch (Exception ex) {
            Develop.DebugPrint("Fehler beim Berechnen der Zellpositionen", ex);
            return false;
        }
        return true;
    }

    private void Cursor_Move(Direction richtung) {
        if (Database == null || Database.IsDisposed) { return; }
        Neighbour(CursorPosColumn, CursorPosRow, richtung, out var newCol, out var newRow);
        CursorPos_Set(newCol, newRow, richtung != Direction.Nichts);
    }

    private Rectangle DisplayRectangleWithoutSlider() => _design == BlueTableAppearance.Standard
            ? new Rectangle(DisplayRectangle.Left, DisplayRectangle.Left, DisplayRectangle.Width - SliderY.Width, DisplayRectangle.Height - SliderX.Height)
            : new Rectangle(DisplayRectangle.Left, DisplayRectangle.Left, DisplayRectangle.Width - SliderY.Width, DisplayRectangle.Height);

    private void Draw_Border(Graphics gr, ColumnViewItem column, Rectangle displayRectangleWoSlider, bool onlyhead) {
        var yPos = displayRectangleWoSlider.Height;
        if (onlyhead) { yPos = HeadSize(); }
        for (var z = 0; z <= 1; z++) {
            int xPos;
            ColumnLineStyle lin;
            if (z == 0) {
                xPos = column?.OrderTmpSpalteX1 ?? 0;
                lin = column?.Column?.LineLeft ?? ColumnLineStyle.Dünn;
            } else {
                xPos = (column?.OrderTmpSpalteX1 ?? 0) + Column_DrawWidth(column, displayRectangleWoSlider);
                lin = column?.Column?.LineRight ?? ColumnLineStyle.Ohne;
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

    private void Draw_CellAsButton(Graphics gr, ColumnViewItem? cellInThisDatabaseColumn, RowData? cellInThisDatabaseRow, Rectangle cellrectangle) {
        ButtonCellEventArgs e = new(cellInThisDatabaseColumn?.Column, cellInThisDatabaseRow?.Row);
        OnNeedButtonArgs(e);

        var s = States.Standard;
        if (!Enabled) { s = States.Standard_Disabled; }
        if (e.Cecked) { s |= States.Checked; }

        var x = new ExtText(Enums.Design.Button_CheckBox, s);
        Button.DrawButton(this, gr, Enums.Design.Button_CheckBox, s, e.Image, Alignment.Horizontal_Vertical_Center, false, x, e.Text, cellrectangle, true);
    }

    /// <summary>
    /// Zeichnet die gesamte Zelle mit Listbox-Hintergrund und prüft noch, ob der verlinkte Inhalt gezeichnet werden soll.
    /// </summary>
    /// <param name="gr"></param>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <param name="cellrectangle"></param>
    /// <param name="displayRectangleWoSlider"></param>
    /// <param name="design"></param>
    /// <param name="state"></param>
    private void Draw_CellListBox(Graphics gr, ColumnViewItem column, RowData row, Rectangle cellrectangle, Rectangle displayRectangleWoSlider, Design design, States state) {
        Skin.Draw_Back(gr, design, state, displayRectangleWoSlider, null, false);
        Skin.Draw_Border(gr, design, state, displayRectangleWoSlider);
        var f = Skin.GetBlueFont(design, state);
        Draw_CellTransparent(gr, column, row, cellrectangle, f, state);
    }

    /// <summary>
    /// Zeichnet die gesamte Zelle ohne Hintergrund und prüft noch, ob der verlinkte Inhalt gezeichnet werden soll.
    /// </summary>
    /// <param name="gr"></param>
    /// <param name="cellInThisDatabaseColumn"></param>
    /// <param name="cellInThisDatabaseRow"></param>
    /// <param name="font"></param>
    private void Draw_CellTransparent(Graphics gr, ColumnViewItem? cellInThisDatabaseColumn, RowData? cellInThisDatabaseRow, Rectangle cellrectangle, BlueFont? font, States state) {
        if (cellInThisDatabaseRow?.Row == null) { return; }
        if (cellInThisDatabaseColumn?.Column == null) { return; }

        if (cellInThisDatabaseColumn.Column.Format == DataFormat.Verknüpfung_zu_anderer_Datenbank) {
            var (lcolumn, lrow, _, _) = CellCollection.LinkedCellData(cellInThisDatabaseColumn.Column, cellInThisDatabaseRow?.Row, false, false);

            if (lcolumn != null && lrow != null) {
                Draw_CellTransparentDirect(gr, cellInThisDatabaseColumn, cellInThisDatabaseRow, cellrectangle, lcolumn, lrow, font, state);
            } else {
                if (cellInThisDatabaseRow.Row.Database.IsAdministrator()) {
                    gr.DrawImage(QuickImage.Get("Warnung|10||||||120||60"), cellrectangle.Left + 3, cellrectangle.Top + 1);
                }
            }
            return;
        }

        Draw_CellTransparentDirect(gr, cellInThisDatabaseColumn, cellInThisDatabaseRow, cellrectangle, cellInThisDatabaseColumn.Column, cellInThisDatabaseRow?.Row, font, state);
    }

    /// <summary>
    /// Zeichnet die gesamte Zelle ohne Hintergrund. Die verlinkte Zelle ist bereits bekannt.
    /// </summary>
    /// <param name="gr"></param>
    /// <param name="cellInThisDatabaseColumn"></param>
    /// <param name="cellInThisDatabaseRow"></param>
    /// <param name="cellrectangle"></param>
    /// <param name="contentHolderCellColumn"></param>
    /// <param name="contentHolderCellRow"></param>
    /// <param name="font"></param>
    private void Draw_CellTransparentDirect(Graphics gr, ColumnViewItem? cellInThisDatabaseColumn, RowData? cellInThisDatabaseRow, Rectangle cellrectangle, ColumnItem? contentHolderCellColumn, RowItem? contentHolderCellRow, BlueFont? font, States state) {
        if (cellInThisDatabaseRow?.Row == null) { return; }
        if (cellInThisDatabaseColumn?.Column == null) { return; }
        if (contentHolderCellColumn == null) { return; }
        if (contentHolderCellRow == null) { return; }

        if (cellInThisDatabaseColumn.Column.Format == DataFormat.Button) {
            Draw_CellAsButton(gr, cellInThisDatabaseColumn, cellInThisDatabaseRow, cellrectangle);
            return;
        }

        var toDraw = contentHolderCellRow.CellGetString(contentHolderCellColumn);

        Draw_CellTransparentDirect(gr, toDraw, cellrectangle, font, contentHolderCellColumn, _pix16, ShortenStyle.Replaced, contentHolderCellColumn.BehaviorOfImageAndText, state);
    }

    private void Draw_Column_Body(Graphics gr, ColumnViewItem cellInThisDatabaseColumn, Rectangle displayRectangleWoSlider) {
        gr.SmoothingMode = SmoothingMode.None;
        gr.FillRectangle(new SolidBrush(cellInThisDatabaseColumn.Column.BackColor), (int)cellInThisDatabaseColumn.OrderTmpSpalteX1, HeadSize(), Column_DrawWidth(cellInThisDatabaseColumn, displayRectangleWoSlider), displayRectangleWoSlider.Height);
        Draw_Border(gr, cellInThisDatabaseColumn, displayRectangleWoSlider, false);
    }

    private void Draw_Column_Cells(Graphics gr, IReadOnlyList<RowData?>? sr, ColumnViewItem viewItem, Rectangle displayRectangleWoSlider, int firstVisibleRow, int lastVisibleRow, States state, bool firstOnScreen) {
        if (UserEdit_NewRowAllowed() && viewItem == CurrentArrangement[Database?.Column.First()]) {
            Skin.Draw_FormatedText(gr, "[Neue Zeile]", QuickImage.Get(ImageCode.PlusZeichen, _pix16), Alignment.Left, new Rectangle((int)viewItem.OrderTmpSpalteX1 + 1, (int)(-SliderY.Value + HeadSize() + 1), (int)viewItem.TmpDrawWidth - 2, 16 - 2), this, false, _newRowFont, Translate);
        }

        for (var zei = firstVisibleRow; zei <= lastVisibleRow; zei++) {
            var currentRow = sr[zei];
            gr.SmoothingMode = SmoothingMode.None;

            Rectangle cellrectangle = new((int)viewItem.OrderTmpSpalteX1, DrawY(currentRow),
                Column_DrawWidth(viewItem, displayRectangleWoSlider), Math.Max(currentRow.DrawHeight, _pix16));

            if (currentRow.Expanded) {
                if (currentRow.MarkYellow) {
                    gr.FillRectangle(BrushYellowTransparent, cellrectangle);
                }

                gr.DrawLine(Skin.PenLinieDünn, cellrectangle.Left, cellrectangle.Bottom - 1, cellrectangle.Right - 1, cellrectangle.Bottom - 1);

                if (!Thread.CurrentThread.IsBackground && CursorPosColumn == viewItem.Column && CursorPosRow == currentRow) {
                    _tmpCursorRect = cellrectangle;
                    _tmpCursorRect.Height -= 1;
                    Draw_Cursor(gr, displayRectangleWoSlider, false);
                }

                Draw_CellTransparent(gr, viewItem, currentRow, cellrectangle, _cellFont, state);

                if (_unterschiede != null && _unterschiede != currentRow?.Row) {
                    if (currentRow?.Row.CellGetString(viewItem.Column) != _unterschiede.CellGetString(viewItem.Column)) {
                        Rectangle tmpr = new((int)viewItem.OrderTmpSpalteX1 + 1, DrawY(currentRow) + 1,
                            Column_DrawWidth(viewItem, displayRectangleWoSlider) - 2, currentRow.DrawHeight - 2);
                        gr.DrawRectangle(PenRed1, tmpr);
                    }
                }
            }

            if (firstOnScreen) {
                // Überschrift in der ersten Spalte zeichnen
                currentRow.CaptionPos = Rectangle.Empty;
                if (currentRow.ShowCap) {
                    var si = gr.MeasureString(currentRow.Chapter, _chapterFont.Font());
                    gr.FillRectangle(new SolidBrush(Skin.Color_Back(Enums.Design.Table_And_Pad, States.Standard).SetAlpha(50)), 1, DrawY(currentRow) - RowCaptionSizeY, displayRectangleWoSlider.Width - 2, RowCaptionSizeY);
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
        }
    }

    private void Draw_Column_Head_Captions(Graphics gr) {
        var bvi = new ColumnViewItem[3];
        var lcbvi = new ColumnViewItem[3];
        ColumnViewItem? lastViewItem = null;
        var permaX = 0;
        var ca = CurrentArrangement;
        if (ca == null) { return; }

        for (var x = 0; x < ca.Count + 1; x++) {
            var viewItem = x < ca.Count ? ca[x] : null;
            if (viewItem != null && viewItem.ViewType == ViewType.PermanentColumn) {
                permaX = Math.Max(permaX, (int)viewItem.OrderTmpSpalteX1 + (int)viewItem.TmpDrawWidth);
            }
            if (viewItem == null ||
                viewItem.ViewType == ViewType.PermanentColumn ||
                (int)viewItem.OrderTmpSpalteX1 + (int)viewItem.TmpDrawWidth > permaX) {
                for (var u = 0; u < 3; u++) {
                    var n = viewItem?.Column.CaptionGroup(u);
                    var v = bvi[u]?.Column.CaptionGroup(u);
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
            Pen pen = new(Skin.Color_Border(Enums.Design.Table_Cursor, stat).SetAlpha(180));
            gr.DrawRectangle(pen, new Rectangle(-1, _tmpCursorRect.Top - 1, displayRectangleWoSlider.Width + 2, _tmpCursorRect.Height + 1));
        } else {
            Skin.Draw_Back(gr, Enums.Design.Table_Cursor, stat, _tmpCursorRect, this, false);
            Skin.Draw_Border(gr, Enums.Design.Table_Cursor, stat, _tmpCursorRect);
        }
    }

    private void Draw_Table_ListboxStyle(Graphics gr, IReadOnlyList<RowData>? sr, States state, Rectangle displayRectangleWoSlider, int vFirstVisibleRow, int vLastVisibleRow) {
        if (Database == null || Database.IsDisposed) {
            DrawWaitScreen(gr);
            return;
        }

        var itStat = state;
        Skin.Draw_Back(gr, Enums.Design.ListBox, state, base.DisplayRectangle, this, true);

        if (sr != null) {
            var col = Database.Column.First();

            // Zeilen Zeichnen (Alle Zellen)
            for (var zeiv = vFirstVisibleRow; zeiv <= vLastVisibleRow; zeiv++) {
                var currentRow = sr[zeiv];
                var viewItem = Database.ColumnArrangements[0][col];
                Rectangle r = new(0, DrawY(currentRow), DisplayRectangleWithoutSlider().Width, currentRow.DrawHeight);
                if (CursorPosColumn != null && CursorPosRow?.Row == currentRow?.Row) {
                    itStat |= States.Checked;
                } else {
                    if (itStat.HasFlag(States.Checked)) {
                        itStat ^= States.Checked;
                    }
                }

                Rectangle cellrectangle = new(0, DrawY(currentRow), displayRectangleWoSlider.Width,
                    Math.Min(currentRow.DrawHeight, 24));

                Draw_CellListBox(gr, viewItem, currentRow, cellrectangle, r, Enums.Design.Item_Listbox, itStat);
                if (Database.Column.SysCorrect is ColumnItem sc && !currentRow.Row.CellGetBoolean(sc)) {
                    gr.DrawImage(QuickImage.Get("Warnung|16||||||120||50"), new Point(r.Right - 19, (int)(r.Top + ((r.Height - 16) / 2.0))));
                }
                if (currentRow.ShowCap) {
                    BlueFont.DrawString(gr, currentRow.Chapter, _chapterFont.Font(), _chapterFont.BrushColorMain, 0, DrawY(currentRow) - _rowCaptionFontY);
                }
            }
        }
        Skin.Draw_Border(gr, Enums.Design.ListBox, state, displayRectangleWoSlider);
    }

    private void Draw_Table_Std(Graphics gr, List<RowData> sr, States state, Rectangle displayRectangleWoSlider, int firstVisibleRow, int lastVisibleRow, ColumnViewCollection? ca) {
        try {
            if (Database == null || Database.IsDisposed || ca == null) { return; }   // Kommt vor, dass spontan doch geparsed wird...
            Skin.Draw_Back(gr, Enums.Design.Table_And_Pad, state, base.DisplayRectangle, this, true);

            /// Maximale Rechten Pixel der Permanenten Columns ermitteln
            var permaX = 0;
            foreach (var viewItem in ca) {
                if (viewItem?.Column != null && viewItem.ViewType == ViewType.PermanentColumn) {
                    if (viewItem.TmpDrawWidth == null) {
                        // Veränderte Werte!
                        DrawControl(gr, state);
                        return;
                    }
                    permaX = Math.Max(permaX, (int)viewItem.OrderTmpSpalteX1 + (int)viewItem.TmpDrawWidth);
                }
            }

            Draw_Table_What(gr, sr, TableDrawColumn.NonPermament, TableDrawType.ColumnBackBody, permaX, displayRectangleWoSlider, firstVisibleRow, lastVisibleRow, state, ca);
            Draw_Table_What(gr, sr, TableDrawColumn.NonPermament, TableDrawType.Cells, permaX, displayRectangleWoSlider, firstVisibleRow, lastVisibleRow, state, ca);
            Draw_Table_What(gr, sr, TableDrawColumn.Permament, TableDrawType.ColumnBackBody, permaX, displayRectangleWoSlider, firstVisibleRow, lastVisibleRow, state, ca);
            Draw_Table_What(gr, sr, TableDrawColumn.Permament, TableDrawType.Cells, permaX, displayRectangleWoSlider, firstVisibleRow, lastVisibleRow, state, ca);
            // Den CursorLines zeichnen
            Draw_Cursor(gr, displayRectangleWoSlider, true);
            Draw_Table_What(gr, sr, TableDrawColumn.NonPermament, TableDrawType.ColumnHead, permaX, displayRectangleWoSlider, firstVisibleRow, lastVisibleRow, state, ca);
            Draw_Table_What(gr, sr, TableDrawColumn.Permament, TableDrawType.ColumnHead, permaX, displayRectangleWoSlider, firstVisibleRow, lastVisibleRow, state, ca);

            /// Überschriften 1-3 Zeichnen
            Draw_Column_Head_Captions(gr);
            Skin.Draw_Border(gr, Enums.Design.Table_And_Pad, state, displayRectangleWoSlider);

            ////gr.Clear(Color.White);
            //gr.DrawString("Test " + Constants.GlobalRND.Next().ToString(false), new Font("Arial", 20), Brushes.Black, new Point(0, 0));

            //return;

            if (Database?.HasPendingChanges ?? false) { gr.DrawImage(QuickImage.Get(ImageCode.Stift, 16), 16, 8); }
            if (Database?.ReadOnly ?? false) { gr.DrawImage(QuickImage.Get(ImageCode.Schloss, 32), 16, 8); }
        } catch {
            Invalidate();
            //Develop.DebugPrint(ex);
        }
    }

    private void Draw_Table_What(Graphics gr, List<RowData> sr, TableDrawColumn col, TableDrawType type, int permaX, Rectangle displayRectangleWoSlider, int firstVisibleRow, int lastVisibleRow, States state, ColumnViewCollection ca) {
        var lfdno = 0;

        var firstOnScreen = true;

        foreach (var viewItem in ca) {
            if (viewItem?.Column != null) {
                lfdno++;
                if (IsOnScreen(viewItem, displayRectangleWoSlider)) {
                    if ((col == TableDrawColumn.NonPermament && viewItem.ViewType != ViewType.PermanentColumn && (viewItem.OrderTmpSpalteX1 ?? 0) + (viewItem.TmpDrawWidth ?? 0) > permaX) ||
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
    }

    private void DrawWaitScreen(Graphics gr) {
        if (SliderX != null) { SliderX.Enabled = false; }
        if (SliderY != null) { SliderY.Enabled = false; }

        Skin.Draw_Back(gr, Enums.Design.Table_And_Pad, States.Standard_Disabled, base.DisplayRectangle, this, true);

        var i = QuickImage.Get(ImageCode.Uhr, 64);
        if (i != null) { gr.DrawImage(i, (Width - 64) / 2, (Height - 64) / 2); }
        Skin.Draw_Border(gr, Enums.Design.Table_And_Pad, States.Standard_Disabled, base.DisplayRectangle);
    }

    private int DrawY(RowData? r) => r == null ? 0 : r.Y + HeadSize() - (int)SliderY.Value;

    /// <summary>
    /// Berechent die Y-Position auf dem aktuellen Controll
    /// </summary>
    /// <param name="r"></param>
    /// <returns></returns>
    private void DropDownMenu_ItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
        FloatingForm.Close(this);
        if (string.IsNullOrEmpty(e.ClickedComand)) { return; }

        CellExtEventArgs? ck = null;
        if (e.HotItem is CellExtEventArgs tmp) { ck = tmp; }

        if (ck?.Column == null) { return; }

        var toAdd = e.ClickedComand;
        var toRemove = string.Empty;
        if (toAdd == "#Erweitert") {
            Cell_Edit(ck.Column, ck.RowData, false);
            return;
        }
        if (ck.RowData == null) {
            // Neue Zeile!
            UserEdited(this, toAdd, ck.Column, null, false);
            return;
        }

        if (ck?.RowData.Row == null) { return; }

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
            UserEdited(this, li.JoinWithCr(), ck.Column, ck.RowData, false);
        } else {
            if (ck.Column.DropdownAllesAbwählenErlaubt) {
                if (toAdd == ck.RowData.Row.CellGetString(ck.Column)) {
                    UserEdited(this, string.Empty, ck.Column, ck.RowData, false);
                    return;
                }
            }
            UserEdited(this, toAdd, ck.Column, ck.RowData, false);
        }
    }

    private bool EnsureVisible(RowData? rowdata) {
        if (rowdata == null) { return false; }
        var r = DisplayRectangleWithoutSlider();
        if (DrawY(rowdata) < HeadSize()) {
            SliderY.Value = SliderY.Value + DrawY(rowdata) - HeadSize();
        } else if (DrawY(rowdata) + rowdata.DrawHeight > r.Height) {
            SliderY.Value = SliderY.Value + DrawY(rowdata) + rowdata.DrawHeight - r.Height;
        }
        return true;
    }

    private bool EnsureVisible(ColumnViewItem? viewItem) {
        if (viewItem?.Column == null) { return false; }
        if (viewItem.OrderTmpSpalteX1 == null && !ComputeAllColumnPositions()) { return false; }
        var r = DisplayRectangleWithoutSlider();
        _ = ComputeAllColumnPositions();
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

    private void Filter_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
        Invalidate_FilteredRows();

        OnFilterChanged();
    }

    private int HeadSize() {
        if (_headSize != null) { return (int)_headSize; }
        if (Database.ColumnArrangements.Count - 1 < _arrangementNr) {
            _headSize = 0;
            return 0;
        }
        if (_design == BlueTableAppearance.OnlyMainColumnWithoutHead || CurrentArrangement.Count - 1 < 0) {
            _headSize = 0;
            return 0;
        }
        _headSize = 16;
        foreach (var thisViewItem in CurrentArrangement) {
            if (thisViewItem?.Column != null) {
                _headSize = Math.Max((int)_headSize, (int)ColumnHead_Size(thisViewItem.Column).Height);
            }
        }

        _headSize += 8;
        _headSize += AutoFilterSize;
        return (int)_headSize;
    }

    private void Invalidate_DrawWidth(ColumnItem? vcolumn) => CurrentArrangement[vcolumn]?.Invalidate_DrawWidth();

    private void Invalidate_Filterinfo() {
        if (Database == null || Database.IsDisposed) { return; }

        foreach (var thisColumn in Database.Column) {
            if (thisColumn != null) {
                thisColumn.TmpIfFilterRemoved = null;
                thisColumn.TmpAutoFilterSinnvoll = null;
            }
        }
    }

    private void Invalidate_sortedRowData() {
        _sortedRowData = null;
        Invalidate();
    }

    private bool IsOnScreen(ColumnViewItem? viewItem, Rectangle displayRectangleWoSlider) {
        if (viewItem == null) { return false; }
        if (_design is BlueTableAppearance.Standard or BlueTableAppearance.OnlyMainColumnWithoutHead) {
            if (viewItem.OrderTmpSpalteX1 + Column_DrawWidth(viewItem, displayRectangleWoSlider) >= 0 && viewItem.OrderTmpSpalteX1 <= displayRectangleWoSlider.Width) { return true; }
        } else {
            Develop.DebugPrint(_design);
        }
        return false;
    }

    private bool IsOnScreen(ColumnItem? column, RowData? row, Rectangle displayRectangleWoSlider) => IsOnScreen(CurrentArrangement[column], displayRectangleWoSlider) && IsOnScreen(row, displayRectangleWoSlider);

    private bool IsOnScreen(RowData? vrow, Rectangle displayRectangleWoSlider) => vrow != null && DrawY(vrow) + vrow.DrawHeight >= HeadSize() && DrawY(vrow) <= displayRectangleWoSlider.Height;

    private void Neighbour(ColumnItem? column, RowData? row, Direction direction, out ColumnItem? newColumn, out RowData? newRow) {
        newColumn = column;
        newRow = row;
        if (_design == BlueTableAppearance.OnlyMainColumnWithoutHead) {
            if (direction is not Direction.Oben and not Direction.Unten) {
                newColumn = Database.Column.First();
                return;
            }
        }
        if (newColumn != null) {
            if (direction.HasFlag(Direction.Links)) {
                if (CurrentArrangement?.PreviousVisible(newColumn) != null) {
                    newColumn = CurrentArrangement.PreviousVisible(newColumn);
                }
            }
            if (direction.HasFlag(Direction.Rechts)) {
                if (CurrentArrangement?.NextVisible(newColumn) != null) {
                    newColumn = CurrentArrangement.NextVisible(newColumn);
                }
            }
        }
        if (newRow != null) {
            if (direction.HasFlag(Direction.Oben)) {
                if (View_PreviousRow(newRow) != null) { newRow = View_PreviousRow(newRow); }
            }
            if (direction.HasFlag(Direction.Unten)) {
                if (View_NextRow(newRow) != null) { newRow = View_NextRow(newRow); }
            }
        }
    }

    /// <summary>
    /// Berechnet die Zelle, ausgehend von einer Zellenposition. Dabei wird die Columns und Zeilensortierung berücksichtigt.
    /// Gibt des keine Nachbarszelle, wird die Eingangszelle zurückgegeben.
    /// </summary>
    /// <remarks></remarks>
    private void OnAutoFilterClicked(FilterEventArgs e) => AutoFilterClicked?.Invoke(this, e);

    private void OnBlockEdit(CellEditBlockReasonEventArgs ed) => BlockEdit?.Invoke(this, ed);

    private void OnButtonCellClicked(CellEventArgs e) => ButtonCellClicked?.Invoke(this, e);

    private void OnCellValueChanged(CellEventArgs e) => CellValueChanged?.Invoke(this, e);

    private void OnDatabaseChanged() => DatabaseChanged?.Invoke(this, System.EventArgs.Empty);

    private void OnFilterChanged() => FilterChanged?.Invoke(this, System.EventArgs.Empty);

    private void OnNeedButtonArgs(ButtonCellEventArgs e) => NeedButtonArgs?.Invoke(this, e);

    private void OnPinnedChanged() => PinnedChanged?.Invoke(this, System.EventArgs.Empty);

    private void OnRowAdded(object sender, RowEventArgs e) => RowAdded?.Invoke(sender, e);

    private void OnSelectedCellChanged(CellExtEventArgs e) => SelectedCellChanged?.Invoke(this, e);

    private void OnSelectedRowChanged(RowEventArgs e) => SelectedRowChanged?.Invoke(this, e);

    private void OnViewChanged() {
        ViewChanged?.Invoke(this, System.EventArgs.Empty);
        Invalidate_sortedRowData(); // evtl. muss [Neue Zeile] ein/ausgebelndet werden
    }

    private void OnVisibleRowsChanged() => VisibleRowsChanged?.Invoke(this, System.EventArgs.Empty);

    /// <summary>
    /// Reset - Parse - CheckView
    /// </summary>
    /// <param name="toParse"></param>

    private void ParseView(string toParse) {
        ResetView();

        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }

        if (!string.IsNullOrEmpty(toParse)) {
            foreach (var pair in toParse.GetAllTags()) {
                switch (pair.Key) {
                    case "arrangementnr":
                        Arrangement = IntParse(pair.Value);
                        break;

                    case "filters":
                        Filter = new FilterCollection(Database, pair.Value.FromNonCritical());
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
                        db.Cell.DataOfCellKey(pair.Value.FromNonCritical(), out var column, out var row);
                        CursorPos_Set(column, SortedRows().Get(row), false);
                        break;

                    case "tempsort":
                        _sortDefinitionTemporary = new RowSortDefinition(Database, pair.Value.FromNonCritical());
                        break;

                    case "pin":
                        foreach (var thisk in pair.Value.FromNonCritical().SplitBy("|")) {
                            var r = db.Row.SearchByKey(thisk);
                            if (r != null && !r.IsDisposed) { PinnedRows.Add(r); }
                        }

                        break;

                    case "collapsed":
                        _collapsed.AddRange(pair.Value.FromNonCritical().SplitBy("|"));
                        break;

                    case "reduced":
                        var cols = pair.Value.FromNonCritical().SplitBy("|");
                        CurrentArrangement?.Reduce(cols);
                        break;

                    default:
                        Develop.DebugPrint(FehlerArt.Warnung, "Tag unbekannt: " + pair.Key);
                        break;
                }
            }
        }

        CheckView();
    }

    private int Row_DrawHeight(RowItem? vrow, Rectangle displayRectangleWoSlider) {
        if (Database == null || Database.IsDisposed || vrow == null) { return _pix18; }

        if (_design == BlueTableAppearance.OnlyMainColumnWithoutHead) { return Cell_ContentSize(this, Database?.Column.First(), vrow, _cellFont, _pix16).Height; }
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

    private void Row_RowRemoving(object sender, RowEventArgs e) {
        if (e.Row == CursorPosRow?.Row) { CursorPos_Reset(); }
        if (e.Row == _mouseOverRow?.Row) { _mouseOverRow = null; }
        if (PinnedRows.Contains(e.Row)) { _ = PinnedRows.Remove(e.Row); }
    }

    private string RowCaptionOnCoordinate(int pixelX, int pixelY) {
        try {
            var s = SortedRows();
            if (s == null) { return string.Empty; }
            foreach (var thisRow in s) {
                if (thisRow.ShowCap && thisRow.CaptionPos is var r) {
                    if (r.Contains(pixelX, pixelY)) { return thisRow.Chapter; }
                }
            }
        } catch { }
        return string.Empty;
    }

    private RowData? RowOnCoordinate(int pixelY) {
        if (Database == null || pixelY <= HeadSize()) { return null; }
        var s = SortedRows();
        if (s == null) { return null; }

        return s.FirstOrDefault(thisRowItem => thisRowItem != null && pixelY >= DrawY(thisRowItem) && pixelY <= DrawY(thisRowItem) + thisRowItem.DrawHeight && thisRowItem.Expanded);
    }

    private void SliderSchaltenSenk(Rectangle displayR, int maxY) {
        SliderY.Minimum = 0;
        SliderY.Maximum = Math.Max(maxY - displayR.Height + 1 + HeadSize(), 0);
        SliderY.LargeChange = displayR.Height - HeadSize();
        SliderY.Enabled = SliderY.Maximum > 0;
    }

    private void SliderSchaltenWaage(Rectangle displayR, int maxX) {
        SliderX.Minimum = 0;
        SliderX.Maximum = maxX - displayR.Width + 1;
        SliderX.LargeChange = displayR.Width;
        SliderX.Enabled = SliderX.Maximum > 0;
    }

    private void SliderX_ValueChanged(object sender, System.EventArgs e) => Invalidate();

    private void SliderY_ValueChanged(object sender, System.EventArgs e) => Invalidate();

    private RowSortDefinition? SortUsed() => _sortDefinitionTemporary?.Columns != null
        ? _sortDefinitionTemporary
        : Database?.SortDefinition?.Columns != null ? Database.SortDefinition : null;

    private void TXTBox_Close(TextBox? textbox) {
        if (textbox == null || Database == null || Database.IsDisposed) { return; }
        if (!textbox.Visible) { return; }
        if (textbox.Tag is not List<object?> tags || tags.Count < 2) {
            textbox.Visible = false;
            return; // Ohne dem hier wird ganz am Anfang ein Ereignis ausgelöst
        }
        var w = textbox.Text;

        ColumnItem? column = null;
        RowData? row = null;

        if (tags[0] is ColumnItem c) { column = c; }
        if (tags[1] is RowData r) { row = r; }

        textbox.Tag = null;
        textbox.Visible = false;
        UserEdited(this, w, column, row, true);
        Focus();
    }

    private bool UserEdit_NewRowAllowed() {
        if (Database == null || Database.Column.Count == 0 || Database.Column.First() is not ColumnItem fc) { return false; }
        if (_design == BlueTableAppearance.OnlyMainColumnWithoutHead) { return false; }
        if (Database.ColumnArrangements.Count == 0) { return false; }
        if (CurrentArrangement?[fc] == null) { return false; }

        if (Database.PowerEdit.Subtract(DateTime.Now).TotalSeconds > 0) { return true; }

        if (!Database.PermissionCheck(Database.PermissionGroupsNewRow, null)) { return false; }

        return string.IsNullOrEmpty(EditableErrorReason(fc, null, EditableErrorReasonType.EditNormaly, true, true, false));
    }

    #endregion
}