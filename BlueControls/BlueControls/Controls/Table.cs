// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
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
using static BlueBasics.Constants;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
[DefaultEvent("SelectedRowChanged")]
[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public partial class Table : GenericControl, IContextMenu, IBackgroundNone, ITranslateable, IHasDatabase, IControlAcceptSomething, IControlSendSomething {

    #region Fields

    public readonly FilterCollection Filter = new("FilterIput 4");

    private readonly List<string> _collapsed = [];

    private readonly object _lockUserAction = new();

    private string _arrangement = string.Empty;

    private AutoFilter? _autoFilter;

    private BlueFont _cellFont = BlueFont.DefaultFont;

    private BlueFont _chapterFont = BlueFont.DefaultFont;

    private BlueFont _columnFilterFont = BlueFont.DefaultFont;

    private BlueFont _columnFont = BlueFont.DefaultFont;

    private DateTime? _databaseDrawError;

    private bool _drawing;

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

    private BlueFont _newRowFont = BlueFont.DefaultFont;

    private Progressbar? _pg;

    private int _pix16 = 16;

    private int _pix18 = 18;

    private int _rowCaptionFontY = 26;

    private List<RowData>? _rowsFilteredAndPinned;

    private SearchAndReplace? _searchAndReplace;

    private bool _showNumber;

    private RowSortDefinition? _sortDefinitionTemporary;

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
        Filter.Changed += Filter_Changed;
        Filter.RowsChanged += Filter_Changed;
    }

    #endregion

    #region Events

    public event EventHandler<FilterEventArgs>? AutoFilterClicked;

    public event EventHandler<CellEditBlockReasonEventArgs>? BlockEdit;

    public event EventHandler<CellEventArgs>? ButtonCellClicked;

    public event EventHandler<ContextMenuInitEventArgs>? ContextMenuInit;

    public event EventHandler<ContextMenuItemClickedEventArgs>? ContextMenuItemClicked;

    public event EventHandler? DatabaseChanged;

    public new event EventHandler<CellExtEventArgs>? DoubleClick;

    public event EventHandler? FilterChanged;

    public event EventHandler<ButtonCellEventArgs>? NeedButtonArgs;

    public event EventHandler? PinnedChanged;

    public event EventHandler<CellExtEventArgs>? SelectedCellChanged;

    public event EventHandler<RowEventArgs>? SelectedRowChanged;

    public event EventHandler? ViewChanged;

    public event EventHandler? VisibleRowsChanged;

    #endregion

    #region Properties

    [DefaultValue(1)]
    [Description("Welche Spaltenanordnung angezeigt werden soll")]
    public string Arrangement {
        get => _arrangement;
        set {
            if (value != _arrangement) {
                _arrangement = value;

                var ca = CurrentArrangement;
                ca?.Invalidate_HeadSize();
                ca?.Invalidate_DrawWithOfAllItems();

                Invalidate();
                OnViewChanged();
                CursorPos_Set(CursorPosColumn, CursorPosRow, true);
            }
        }
    }

    public List<IControlAcceptSomething> Childs { get; } = [];

    public ColumnViewCollection? CurrentArrangement {
        get {
            if (IsDisposed || Database is not Database db || db.IsDisposed) { return null; }

            var l = db.ColumnArrangements.Get(_arrangement);
            if (string.IsNullOrEmpty(_arrangement) || l == null) {
                if (db.ColumnArrangements.Count > 0) { return db.ColumnArrangements[1]; }
                return null;
            }

            return l;
        }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ColumnItem? CursorPosColumn { get; private set; }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public RowData? CursorPosRow { get; private set; }

    /// <summary>
    /// Datenbanken können mit DatabaseSet gesetzt werden.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Database? Database { get; private set; }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FilterCollection? FilterInput { get; set; }

    [DefaultValue(false)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool FilterManualSeted { get; set; } = false;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FilterCollection FilterOutput { get; } = new("FilterOutPt 4");

    public Filterausgabe FilterOutputType { get; set; } = Filterausgabe.Gewähle_Zeile;

    [DefaultValue(1.0f)]
    public double FontScale => Database?.GlobalScale ?? 1f;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<IControlSendSomething> Parents { get; } = [];

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<RowItem> PinnedRows { get; } = [];

    public DateTime PowerEdit {
        //private get => _database?.PowerEdit;
        set {
            if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }
            Database.PowerEdit = value;
            Invalidate_SortedRowData(); // Neue Zeilen können nun erlaubt sein
        }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ReadOnlyCollection<RowItem> RowsFiltered {
        get {
            if (IsDisposed || Database is not Database db || db.IsDisposed) { return new List<RowItem>().AsReadOnly(); }
            return Filter.Rows;
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

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public long VisibleRowCount { get; private set; }

    protected override string QuickInfoText {
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

    #endregion

    #region Methods

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
        i.Appearance = ListBoxAppearance.Listbox;
        var v = InputBoxListBoxStyle.Show("Vorherigen Eintrag wählen:", i, AddType.None, true);
        if (v == null || v.Count != 1) { return; }
        if (v[0] == "Cancel") { return; } // =Aktueller Eintrag angeklickt
        row.CellSet(column, v[0].Substring(5));
        //Database.Cell.Set(column, row, v[0].Substring(5), false);
        //_ = row.ExecuteScript(EventTypes.value_changedx, string.Empty, true, true, true, 5);
        //row.Database?.AddBackgroundWork(row);
        row.Database?.Row.ExecuteValueChangedEvent(true);
    }

    public static void Draw_FormatedText(Graphics gr, string text, ShortenStyle style, ColumnItem? column, Rectangle fitInRect, Design design, States state, BildTextVerhalten bildTextverhalten, float scale) {
        if (string.IsNullOrEmpty(text)) { return; }
        var d = Skin.DesignOf(design, state).BFont;
        if (d == null) { return; }

        Draw_CellTransparentDirect(gr, text, style, fitInRect, d, column, 16, bildTextverhalten, state, scale);
    }

    /// <summary>
    /// Füllt die Liste rowsToExpand auf, bis sie 100 Einträge enthält.
    /// </summary>
    /// <param name="rowsToExpand"></param>
    /// <param name="sortedRows"></param>
    /// <returns>Gibt false zurück, wenn ALLE Zeilen dadurch geladen sind.</returns>
    public static bool FillUp100(List<RowItem> rowsToExpand, List<RowData> sortedRows) {
        if (rowsToExpand.Count is > 99 or 0) { return false; }

        if (rowsToExpand[0].IsDisposed) { return false; }
        if (rowsToExpand[0].Database is not Database db || db.IsDisposed) { return false; }
        if (db.IsDisposed) { return false; }

        if (sortedRows.Count == 0) { return false; } // Komisch, dürfte nie passieren

        var tmpRowsToExpand = new List<RowItem>();
        tmpRowsToExpand.AddRange(rowsToExpand);

        foreach (var thisRow in tmpRowsToExpand) {
            var all = FillUp(rowsToExpand, thisRow, sortedRows, (100 / tmpRowsToExpand.Count) + 1);
            if (all) { return true; }
            if (rowsToExpand.Count > 200) { return false; }
        }

        return false;
    }

    public static void ImportCsv(Database database, string csvtxt) {
        using Import x = new(database, csvtxt);
        _ = x.ShowDialog();
    }

    public static void SearchNextText(string searchTxt, Table tableView, ColumnItem? column, RowData? row, out ColumnItem? foundColumn, out RowData? foundRow, bool vereinfachteSuche) {
        if (tableView.Database is not Database db || db.IsDisposed) {
            MessageBox.Show("Datenbank-Fehler.", ImageCode.Information, "OK");
            foundColumn = null;
            foundRow = null;
            return;
        }

        searchTxt = searchTxt.Trim();
        if (tableView.CurrentArrangement is not ColumnViewCollection ca) {
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
            column = ca.NextVisible(column);

            if (column == null || column.IsDisposed) {
                column = ca.First()?.Column;
                if (rowsChecked > db.Row.Count() + 1) {
                    foundColumn = null;
                    foundRow = null;
                    return;
                }
                rowsChecked++;
                row = tableView.View_NextRow(row) ?? tableView.View_RowFirst();
            }
            ColumnItem? contentHolderCellColumn = column;
            RowItem? contentHolderCellRow = row?.Row;
            if (column != null && column.Format == DataFormat.Verknüpfung_zu_anderer_Datenbank) {
                (contentHolderCellColumn, contentHolderCellRow, _, _) = CellCollection.LinkedCellData(contentHolderCellColumn, contentHolderCellRow, false, false);
            }
            var ist1 = string.Empty;
            var ist2 = string.Empty;
            if (contentHolderCellRow != null && contentHolderCellColumn != null) {
                ist1 = contentHolderCellRow.CellGetString(contentHolderCellColumn);
                ist2 = CellItem.ValuesReadable(contentHolderCellColumn, contentHolderCellRow, ShortenStyle.Both).JoinWithCr();
            }
            if (contentHolderCellColumn != null && contentHolderCellColumn.FormatierungErlaubt) {
                ExtText l = new(Design.TextBox, States.Standard) {
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
    public static ItemCollectionList.ItemCollectionList UndoItems(Database? db, string cellkey) {
        ItemCollectionList.ItemCollectionList i = new(ListBoxAppearance.KontextMenu, false) {
            CheckBehavior = CheckBehavior.AlwaysSingleSelection
        };

        if (db != null && !db.IsDisposed) {
            //database.GetUndoCache();

            if (db.Undo.Count == 0) { return i; }

            var isfirst = true;
            TextListItem? las = null;
            var lasNr = -1;
            var co = 0;
            for (var z = db.Undo.Count - 1; z >= 0; z--) {
                if (db.Undo[z].CellKey == cellkey) {
                    co++;
                    lasNr = z;
                    if (isfirst) {
                        las = new TextListItem(
                            "Aktueller Text - ab " + db.Undo[z].DateTimeUtc + " UTC, geändert von " +
                            db.Undo[z].User, "Cancel", null, false, true, string.Empty);
                    } else {
                        las = new TextListItem(
                            "ab " + db.Undo[z].DateTimeUtc + " UTC, geändert von " + db.Undo[z].User,
                            co.ToString(Format_Integer5) + db.Undo[z].ChangedTo, null, false, true,
                            string.Empty);
                    }
                    isfirst = false;
                    i.Add(las);
                }
            }

            if (las != null) {
                co++;
                _ = i.Add("vor " + db.Undo[lasNr].DateTimeUtc + " UTC",
                    co.ToString(Format_Integer5) + db.Undo[lasNr].PreviousValue);
            }
        }

        return i;
    }

    public static void WriteColumnArrangementsInto(ComboBox? columnArrangementSelector, Database? database, string showingKey) {
        if (columnArrangementSelector == null || columnArrangementSelector.IsDisposed) { return; }

        columnArrangementSelector.Item.Clear();
        columnArrangementSelector.Item.AutoSort = false;
        columnArrangementSelector.DropDownStyle = ComboBoxStyle.DropDownList;

        if (database is Database db && !db.IsDisposed) {
            foreach (var thisArrangement in db.ColumnArrangements) {
                if (thisArrangement != null) {
                    if (db.PermissionCheck(thisArrangement.PermissionGroups_Show, null)) {
                        _ = columnArrangementSelector.Item.Add(thisArrangement);
                    }
                }
            }
        }

        columnArrangementSelector.Enabled = columnArrangementSelector.Item.Count > 1;

        if (columnArrangementSelector.Item[showingKey] == null) {
            if (columnArrangementSelector.Item.Count > 1) {
                showingKey = columnArrangementSelector.Item[1].KeyName;
            } else {
                showingKey = string.Empty;
            }
        }

        columnArrangementSelector.Text = showingKey;
    }

    public List<RowData> CalculateSortedRows(ICollection<RowItem> filteredRows, List<RowItem>? pinnedRows) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return []; }

        VisibleRowCount = 0;

        #region Ermitteln, ob mindestens eine Überschrift vorhanden ist (capName)

        var capName = pinnedRows != null && pinnedRows.Count > 0;
        if (!capName && db.Column.SysChapter is ColumnItem cap) {
            foreach (var thisRow in filteredRows) {
                if (thisRow.Database != null && !thisRow.CellIsNullOrEmpty(cap)) {
                    capName = true;
                    break;
                }
            }
        }

        #endregion

        #region Refresh

        var colsToRefresh = new List<ColumnItem>();
        var reverse = false;
        if (SortUsed() is RowSortDefinition rsd) { colsToRefresh.AddRange(rsd.Columns); reverse = rsd.Reverse; }
        if (db.Column.SysChapter is ColumnItem csc) { _ = colsToRefresh.AddIfNotExists(csc); }
        if (db.Column.First() is ColumnItem cf) { _ = colsToRefresh.AddIfNotExists(cf); }

        db.RefreshColumnsData(colsToRefresh);

        #endregion

        #region _Angepinnten Zeilen erstellen (_pinnedData)

        List<RowData> pinnedData = [];
        var lockMe = new object();
        if (pinnedRows != null) {
            _ = Parallel.ForEach(pinnedRows, thisRow => {
                var rd = new RowData(thisRow, "Angepinnt");
                rd.PinStateSortAddition = "1";
                rd.MarkYellow = true;
                rd.AdditionalSort = thisRow.CompareKey(colsToRefresh);

                lock (lockMe) {
                    VisibleRowCount++;
                    pinnedData.Add(rd);
                }
            });
        }

        #endregion

        #region Gefiltere Zeilen erstellen (_rowData)

        List<RowData> rowData = [];
        _ = Parallel.ForEach(filteredRows, thisRow => {
            var adk = thisRow.CompareKey(colsToRefresh);

            var markYellow = pinnedRows != null && pinnedRows.Contains(thisRow);
            var added = markYellow;

            List<string> caps;
            if (db.Column.SysChapter is ColumnItem sc) {
                caps = thisRow.CellGetList(sc);
            } else {
                caps = [];
            }

            if (caps.Count > 0) {
                if (caps.Contains(string.Empty)) {
                    _ = caps.Remove(string.Empty);
                    caps.Add("-?-");
                }
            }

            if (caps.Count == 0 && capName) { caps.Add("Weitere Zeilen"); }
            if (caps.Count == 0) { caps.Add(string.Empty); }

            foreach (var thisCap in caps) {
                var rd = new RowData(thisRow, thisCap);

                rd.PinStateSortAddition = "2";
                rd.MarkYellow = markYellow;
                rd.AdditionalSort = adk;
                lock (lockMe) {
                    rowData.Add(rd);
                    if (!added) { VisibleRowCount++; added = true; }
                }
            }
        });

        #endregion

        pinnedData.Sort();
        rowData.Sort();

        if (reverse) { rowData.Reverse(); }

        rowData.InsertRange(0, pinnedData);
        return rowData;
    }

    public void CheckView() {
        var db = Database;
        if (_mouseOverColumn?.Database != db) { _mouseOverColumn = null; }
        if (_mouseOverRow?.Row.Database != db) { _mouseOverRow = null; }
        if (CursorPosColumn?.Database != db) { CursorPosColumn = null; }
        if (CursorPosRow?.Row.Database != db) { CursorPosRow = null; }

        if (CurrentArrangement is ColumnViewCollection ca && db != null) {
            if (!db.PermissionCheck(ca.PermissionGroups_Show, null)) { Arrangement = string.Empty; }
        } else {
            Arrangement = string.Empty;
        }
    }

    public void CollapesAll() {
        if (Database?.Column.SysChapter is not ColumnItem sc) { return; }

        _collapsed.Clear();
        _collapsed.AddRange(sc.Contents());

        Invalidate_SortedRowData();
    }

    public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e) => false;

    public void CursorPos_Reset() => CursorPos_Set(null, null, false);

    public void CursorPos_Set(ColumnItem? column, RowData? row, bool ensureVisible) {
        if (IsDisposed || Database is not Database db1 || db1.IsDisposed || row == null || column == null ||
            db1.ColumnArrangements.Count == 0 || CurrentArrangement?[column] == null ||
            RowsFilteredAndPinned() is not List<RowData> s || !s.Contains(row)) {
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

        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }

        if (ensureVisible) {
            if (CurrentArrangement is ColumnViewCollection ca) {
                _ = EnsureVisible(ca, CursorPosColumn, CursorPosRow);
            }
        }
        Invalidate();

        OnSelectedCellChanged(new CellExtEventArgs(CursorPosColumn, CursorPosRow));

        if (!sameRow) {
            OnSelectedRowChanged(new RowEventArgs(setedrow));

            if (FilterOutputType == Filterausgabe.Gewähle_Zeile) {
                this.DoOutputSettings(db, Name);
                FilterOutput.Add(new FilterItem(setedrow));
            }
        }
    }

    public void DatabaseSet(Database? db, string viewCode) {
        if (Database == db && string.IsNullOrEmpty(viewCode)) { return; }

        CloseAllComponents();

        if (Database is Database db1 && !db1.IsDisposed) {
            // auch Disposed Datenbanken die Bezüge entfernen!
            db1.Cell.CellValueChanged -= _Database_CellValueChanged;
            db1.Loaded -= _Database_DatabaseLoaded;
            db1.Loading -= _Database_StoreView;
            db1.ViewChanged -= _Database_ViewChanged;
            db1.Column.ColumnInternalChanged -= _Database_ColumnContentChanged;
            db1.SortParameterChanged -= _Database_SortParameterChanged;
            db1.Row.RowRemoving -= Row_RowRemoving;
            db1.Row.RowGotData -= _Database_Row_RowGotData;
            db1.Column.ColumnRemoving -= Column_ItemRemoving;
            db1.Column.ColumnRemoved -= _Database_ViewChanged;
            db1.Column.ColumnAdded -= _Database_ViewChanged;
            db1.ProgressbarInfo -= _Database_ProgressbarInfo;
            db1.DisposingEvent -= _database_Disposing;
            db1.InvalidateView -= Database_InvalidateView;
            Database.ForceSaveAll();
            MultiUserFile.ForceLoadSaveAll();
        }
        ShowWaitScreen = true;
        Refresh(); // um die Uhr anzuzeigen
        Database = db;
        Filter.Database = db;

        _databaseDrawError = null;
        InitializeSkin(); // Neue Schriftgrößen
        if (Database is Database db2 && !db2.IsDisposed) {
            this.DoOutputSettings(db2, Name);

            db2.Cell.CellValueChanged += _Database_CellValueChanged;
            db2.Loaded += _Database_DatabaseLoaded;
            db2.Loading += _Database_StoreView;
            db2.ViewChanged += _Database_ViewChanged;
            db2.Column.ColumnInternalChanged += _Database_ColumnContentChanged;
            db2.SortParameterChanged += _Database_SortParameterChanged;
            db2.Row.RowRemoving += Row_RowRemoving;
            db2.Row.RowGotData += _Database_Row_RowGotData;
            db2.Column.ColumnAdded += _Database_ViewChanged;
            db2.Column.ColumnRemoving += Column_ItemRemoving;
            db2.Column.ColumnRemoved += _Database_ViewChanged;
            db2.ProgressbarInfo += _Database_ProgressbarInfo;
            db2.DisposingEvent += _database_Disposing;
            db2.InvalidateView += Database_InvalidateView;
        }

        ParseView(viewCode);

        ShowWaitScreen = false;
        OnDatabaseChanged();
    }

    public void Draw_Column_Head(Graphics gr, ColumnViewItem viewItem, Rectangle displayRectangleWoSlider, int lfdNo, ColumnViewCollection ca) {
        if (!IsOnScreen(viewItem, displayRectangleWoSlider)) { return; }
        if (!ca.ShowHead) { return; }

        if (viewItem.Column != null) {
            if (viewItem.OrderTmpSpalteX1 != null) {
                gr.FillRectangle(new SolidBrush(viewItem.Column.BackColor), (int)viewItem.OrderTmpSpalteX1, 0, viewItem.DrawWidth(displayRectangleWoSlider, _pix16, _cellFont), ca.HeadSize(_columnFont));
                Draw_Border(gr, viewItem, displayRectangleWoSlider, true, ca);
                gr.FillRectangle(new SolidBrush(Color.FromArgb(100, 200, 200, 200)), (int)viewItem.OrderTmpSpalteX1, 0, viewItem.DrawWidth(displayRectangleWoSlider, _pix16, _cellFont), ca.HeadSize(_columnFont));

                var down = 0;
                if (!string.IsNullOrEmpty(viewItem.Column.CaptionGroup3)) {
                    down = ColumnCaptionSizeY * 3;
                } else if (!string.IsNullOrEmpty(viewItem.Column.CaptionGroup2)) {
                    down = ColumnCaptionSizeY * 2;
                } else if (!string.IsNullOrEmpty(viewItem.Column.CaptionGroup1)) {
                    down = ColumnCaptionSizeY;
                }

                #region Recude-Button zeichnen

                if (viewItem.DrawWidth(displayRectangleWoSlider, _pix16, _cellFont) > 70 || viewItem.TmpReduced) {
                    viewItem.TmpReduceLocation = new Rectangle((int)viewItem.OrderTmpSpalteX1 + viewItem.DrawWidth(displayRectangleWoSlider, _pix16, _cellFont) - 18, down, 18, 18);
                    gr.DrawImage(viewItem.TmpReduced ? QuickImage.Get("Pfeil_Rechts|16|||FF0000|||||20") : QuickImage.Get("Pfeil_Links|16||||||||75"), viewItem.TmpReduceLocation.Left + 2, viewItem.TmpReduceLocation.Top + 2);
                }

                #endregion

                #region Filter-Knopf mit Trichter

                var trichterText = string.Empty;
                QuickImage? trichterIcon = null;
                var trichterState = States.Undefiniert;
                viewItem.TmpAutoFilterLocation = new Rectangle((int)viewItem.OrderTmpSpalteX1 + viewItem.DrawWidth(displayRectangleWoSlider, _pix16, _cellFont) - AutoFilterSize, ca.HeadSize(_columnFont) - AutoFilterSize, AutoFilterSize, AutoFilterSize);
                var fi = Filter[viewItem.Column];

                if (viewItem.Column.AutoFilterSymbolPossible()) {
                    if (fi != null) {
                        trichterState = States.Checked;
                        var anz = Autofilter_Text(viewItem.Column);
                        trichterText = anz > -100 ? (anz * -1).ToString() : "∞";
                    } else {
                        trichterState = States.Standard;
                    }
                }

                var trichterSize = (AutoFilterSize - 4).ToString();
                if (fi != null) {
                    trichterIcon = QuickImage.Get("Trichter|" + trichterSize + "|||FF0000");
                } else if (Filter.MayHaveRowFilter(viewItem.Column)) {
                    trichterIcon = QuickImage.Get("Trichter|" + trichterSize + "|||227722");
                } else if (viewItem.Column.AutoFilterSymbolPossible()) {
                    trichterIcon = QuickImage.Get("Trichter|" + trichterSize);
                }

                if (trichterState != States.Undefiniert) {
                    Skin.Draw_Back(gr, Design.Button_AutoFilter, trichterState, viewItem.TmpAutoFilterLocation, null, false);
                    Skin.Draw_Border(gr, Design.Button_AutoFilter, trichterState, viewItem.TmpAutoFilterLocation);
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
                            BlueFont.DrawString(gr, "#" + lfdNo, _columnFont, Brushes.Black, (int)viewItem.OrderTmpSpalteX1 + x, viewItem.TmpAutoFilterLocation.Top + y);
                        }
                    }

                    BlueFont.DrawString(gr, "#" + lfdNo, _columnFont, Brushes.White, (int)viewItem.OrderTmpSpalteX1, viewItem.TmpAutoFilterLocation.Top);
                }

                #endregion LaufendeNummer

                var tx = viewItem.Column.Caption;
                tx = LanguageTool.DoTranslate(tx, Translate).Replace("\r", "\r\n");
                var fs = gr.MeasureString(tx, (Font)_columnFont);

                #region Spalten-Kopf-Bild erzeugen

                if (!string.IsNullOrEmpty(viewItem.Column.CaptionBitmapCode) && viewItem.Column.TmpCaptionBitmapCode == null) {
                    viewItem.Column.TmpCaptionBitmapCode = QuickImage.Get(viewItem.Column.CaptionBitmapCode + "|100");
                }

                #endregion

                if (viewItem.Column.TmpCaptionBitmapCode != null && !viewItem.Column.TmpCaptionBitmapCode.IsError) {

                    #region Spalte mit Bild zeichnen

                    Point pos = new(
                        (int)viewItem.OrderTmpSpalteX1 +
                        (int)((viewItem.DrawWidth(displayRectangleWoSlider, _pix16, _cellFont) - fs.Width) / 2.0), 3 + down);
                    gr.DrawImageInRectAspectRatio(viewItem.Column.TmpCaptionBitmapCode, (int)viewItem.OrderTmpSpalteX1 + 2, (int)(pos.Y + fs.Height), viewItem.DrawWidth(displayRectangleWoSlider, _pix16, _cellFont) - 4, ca.HeadSize(_columnFont) - (int)(pos.Y + fs.Height) - 6 - 18);
                    // Dann der Text
                    gr.TranslateTransform(pos.X, pos.Y);
                    //GR.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    BlueFont.DrawString(gr, tx, _columnFont, new SolidBrush(viewItem.Column.ForeColor), 0, 0);
                    gr.TranslateTransform(-pos.X, -pos.Y);

                    #endregion
                } else {

                    #region Spalte ohne Bild zeichnen

                    Point pos = new(
                        (int)viewItem.OrderTmpSpalteX1 +
                        (int)((viewItem.DrawWidth(displayRectangleWoSlider, _pix16, _cellFont) - fs.Height) / 2.0),
                        ca.HeadSize(_columnFont) - 4 - AutoFilterSize);
                    gr.TranslateTransform(pos.X, pos.Y);
                    gr.RotateTransform(-90);
                    //GR.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    BlueFont.DrawString(gr, tx, _columnFont, new SolidBrush(viewItem.Column.ForeColor), 0, 0);
                    gr.TranslateTransform(-pos.X, -pos.Y);
                    gr.ResetTransform();

                    #endregion
                }

                #region Sortierrichtung Zeichnen

                var tmpSortDefinition = SortUsed();
                if (tmpSortDefinition != null && (tmpSortDefinition.UsedForRowSort(viewItem.Column) || viewItem.Column == Database?.Column.SysChapter)) {
                    if (viewItem.OrderTmpSpalteX1 != null) {
                        gr.DrawImage(tmpSortDefinition.Reverse ? QuickImage.Get("ZA|11|5||||50") : QuickImage.Get("AZ|11|5||||50"),
                            (float)((viewItem.OrderTmpSpalteX1 ?? 0) + (viewItem.DrawWidth(displayRectangleWoSlider, _pix16, _cellFont) / 2.0) - 6),
                            ca.HeadSize(_columnFont) - 6 - AutoFilterSize);
                    }
                }
            }
        }

        #endregion
    }

    public string EditableErrorReason(ColumnItem? cellInThisDatabaseColumn, RowData? cellInThisDatabaseRow, EditableErrorReasonType mode, bool checkUserRights, bool checkEditmode, bool maychangeview) {
        var f = CellCollection.EditableErrorReason(cellInThisDatabaseColumn, cellInThisDatabaseRow?.Row, mode, checkUserRights, checkEditmode, true, false);
        if (!string.IsNullOrWhiteSpace(f)) { return f; }

        if (checkEditmode) {
            if (CurrentArrangement is not ColumnViewCollection ca || ca[cellInThisDatabaseColumn] is not ColumnViewItem viewItem) {
                return "Ansicht veraltet";
            }

            if (cellInThisDatabaseRow != null) {
                if (maychangeview && !EnsureVisible(ca, cellInThisDatabaseColumn, cellInThisDatabaseRow)) {
                    return "Zelle konnte nicht angezeigt werden.";
                }
                if (!IsOnScreen(ca, cellInThisDatabaseColumn, cellInThisDatabaseRow, DisplayRectangle)) {
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

    public bool EnsureVisible(ColumnViewCollection ca, ColumnItem? column, RowData? row) {
        var ok1 = EnsureVisible(ca[column]);
        var ok2 = EnsureVisible(ca, row);
        return ok1 && ok2;
    }

    public void ExpandAll() {
        if (Database?.Column.SysChapter is null) { return; }

        _collapsed.Clear();
        CursorPos_Reset(); // Wenn eine Zeile markiert ist, man scrollt und expandiert, springt der Screen zurück, was sehr irriteiert
        Invalidate_SortedRowData();
    }

    public string Export_CSV(FirstRow firstRow) => Database == null ? string.Empty : Database.Export_CSV(firstRow, CurrentArrangement, RowsVisibleUnique());

    public string Export_CSV(FirstRow firstRow, ColumnItem onlyColumn) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return string.Empty; }
        List<ColumnItem> l = [onlyColumn];
        return Database.Export_CSV(firstRow, l, RowsVisibleUnique());
    }

    public void Export_HTML(string filename = "", bool execute = true) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }
        if (string.IsNullOrEmpty(filename)) { filename = TempFile(string.Empty, string.Empty, "html"); }
        Database.Export_HTML(filename, CurrentArrangement, RowsVisibleUnique(), execute);
    }

    public void FilterInput_Changed(object? sender, System.EventArgs e) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }

        this.DoInputFilter(FilterOutput.Database, false);

        const string t = "Übergeordnetes Element";

        Filter.RemoveRange(t);

        if (FilterOutputType == Filterausgabe.Im_Element_Gewählte_Filter) {
            FilterOutput.ChangeTo(Filter);
        }

        Filter.RemoveOtherAndAdd(FilterInput, t);

        if (FilterOutputType == Filterausgabe.Alle_Filter) {
            FilterOutput.ChangeTo(Filter);
        }
    }

    public void FilterInput_Changing(object sender, System.EventArgs e) { }

    public void FilterInput_RowChanged(object? sender, System.EventArgs e) { }

    /// <summary>
    /// Alle gefilteren Zeilen. Jede Zeile ist maximal einmal in dieser Liste vorhanden. Angepinnte Zeilen addiert worden
    /// </summary>
    /// <returns></returns>
    public new void Focus() {
        if (Focused()) { return; }
        _ = base.Focus();
    }

    public new bool Focused() => base.Focused || SliderY.Focused() || SliderX.Focused() || BTB.Focused || BCB.Focused;

    public void GetContextMenuItems(MouseEventArgs? e, ItemCollectionList.ItemCollectionList items, out object? hotItem) {
        hotItem = null;
        if (e == null) { return; }

        if (CurrentArrangement is not ColumnViewCollection ca) { return; }

        CellOnCoordinate(ca, e.X, e.Y, out _mouseOverColumn, out _mouseOverRow);
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
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }
        ImportCsv(Database, csvtxt);
    }

    public void Invalidate_AllColumnArrangements() {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }

        foreach (var thisArrangement in db.ColumnArrangements) {
            thisArrangement.Invalidate_DrawWithOfAllItems();
            thisArrangement.Invalidate_HeadSize();
        }
    }

    public bool Mouse_IsInHead() {
        if (CurrentArrangement is not ColumnViewCollection ca) { return false; }

        return MousePos().Y <= ca.HeadSize(_columnFont);
    }

    public void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);

    public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) => ContextMenuItemClicked?.Invoke(this, e);

    public void OpenSearchAndReplace() {
        if (TableView.ErrorMessage(Database, EditableErrorReasonType.EditCurrently) || Database == null) { return; }

        if (!Database.IsAdministrator()) { return; }

        if (_searchAndReplace == null || _searchAndReplace.IsDisposed || !_searchAndReplace.Visible) {
            _searchAndReplace = new SearchAndReplace(this);
            _searchAndReplace.Show();
        }
    }

    public void Parents_Added(bool hasFilter) {
        if (IsDisposed) { return; }
        if (!hasFilter) { return; }
        FilterInput_Changed(null, System.EventArgs.Empty);
    }

    public void Pin(List<RowItem>? rows) {
        // Arbeitet mit Rows, weil nur eine Anpinngug möglich ist

        rows ??= [];

        rows = rows.Distinct().ToList();
        if (!rows.IsDifferentTo(PinnedRows)) { return; }

        PinnedRows.Clear();
        PinnedRows.AddRange(rows);
        Invalidate_SortedRowData();
        OnPinnedChanged();
    }

    public void PinAdd(RowItem? row) {
        if (row == null || row.IsDisposed) { return; }
        PinnedRows.Add(row);
        Invalidate_SortedRowData();
        OnPinnedChanged();
    }

    public void PinRemove(RowItem? row) {
        if (row == null || row.IsDisposed) { return; }
        _ = PinnedRows.Remove(row);
        Invalidate_SortedRowData();
        OnPinnedChanged();
    }

    public void ResetView() {
        Filter.Clear();

        PinnedRows.Clear();
        _collapsed.Clear();

        _mouseOverText = string.Empty;
        _sortDefinitionTemporary = null;
        _mouseOverColumn = null;
        _mouseOverRow = null;
        CursorPosColumn = null;
        CursorPosRow = null;
        _arrangement = string.Empty;
        _unterschiede = null;

        Invalidate_AllColumnArrangements();
        Invalidate_SortedRowData();
        OnViewChanged();
        Invalidate();
    }

    public List<RowData>? RowsFilteredAndPinned() {
        if (IsDisposed) { return null; }
        if (CurrentArrangement is not ColumnViewCollection ca) { return null; }

        if (_rowsFilteredAndPinned != null) { return _rowsFilteredAndPinned; }

        try {
            var displayR = DisplayRectangleWithoutSlider();
            var maxY = 0;
            if (UserEdit_NewRowAllowed()) { maxY += _pix18; }
            var expanded = true;
            var lastCap = string.Empty;

            List<RowData> sortedRowDataNew;
            if (IsDisposed || Database is not Database db || db.IsDisposed) {
                sortedRowDataNew = [];
            } else {
                sortedRowDataNew = CalculateSortedRows(RowsFiltered, PinnedRows);
            }

            //if (_rowsFilteredAndPinned != null && !_rowsFilteredAndPinned.IsDifferentTo(sortedRowDataNew)) { return _rowsFilteredAndPinned; }

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
                    thisRowData.DrawHeight = Row_DrawHeight(ca, thisRowData.Row, displayR);
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

            SliderSchaltenSenk(ca, displayR, maxY);

            #endregion

            _rowsFilteredAndPinned = sortedRowDataTmp;

            //         _ = EnsureVisible(CursorPosColumn, CursorPosRow);
            // EnsureVisible macht probleme, wenn mit der Maus auf ein linked Cell gezogen wird und eine andere Cell
            //markiert ist. Dann wird sortiren angestoßen, und der Cursor zurückgesprungen

            OnVisibleRowsChanged();

            return RowsFilteredAndPinned(); // Rekursiver Aufruf. Manchmal funktiniert OnRowsSorted nicht ...
        } catch {
            // Komisch, manchmal wird die Variable _sortedRowDatax verworfen.
            Develop.CheckStackForOverflow();
            Invalidate_SortedRowData();
            return RowsFilteredAndPinned();
        }
    }

    public List<RowItem> RowsVisibleUnique() {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return []; }

        var f = RowsFiltered;

        ConcurrentBag<RowItem> l = [];

        try {
            var lockMe = new object();
            _ = Parallel.ForEach(Database.Row, thisRowItem => {
                if (thisRowItem != null) {
                    if (f.Contains(thisRowItem) || PinnedRows.Contains(thisRowItem)) {
                        lock (lockMe) { l.Add(thisRowItem); }
                    }
                }
            });
        } catch {
            Develop.CheckStackForOverflow();
            return RowsVisibleUnique();
        }

        return l.ToList();
    }

    public ColumnItem? View_ColumnFirst() {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return null; }
        var s = CurrentArrangement;

        return s == null || s.Count == 0 ? null : s[0]?.Column;
    }

    public RowData? View_NextRow(RowData? row) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return null; }
        if (row == null || row.IsDisposed) { return null; }

        if (RowsFilteredAndPinned() is not List<RowData> sr) { return null; }

        var rowNr = sr.IndexOf(row);
        return rowNr < 0 || rowNr >= sr.Count - 1 ? null : sr[rowNr + 1];
    }

    public RowData? View_PreviousRow(RowData? row) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return null; }
        if (row == null || row.IsDisposed) { return null; }
        if (RowsFilteredAndPinned() is not List<RowData> sr) { return null; }
        var rowNr = sr.IndexOf(row);
        return rowNr < 1 ? null : sr[rowNr - 1];
    }

    public RowData? View_RowFirst() {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return null; }
        if (RowsFilteredAndPinned() is not List<RowData> sr) { return null; }
        return sr.Count == 0 ? null : sr[0];
    }

    public RowData? View_RowLast() {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return null; }
        if (RowsFilteredAndPinned() is not List<RowData> sr) { return null; }
        return sr.Count == 0 ? null : sr[sr.Count - 1];
    }

    public string ViewToString() {
        List<string> result = [];
        result.ParseableAdd("Arrangement", _arrangement);
        result.ParseableAdd("Filters", (IStringable?)Filter);
        result.ParseableAdd("SliderX", SliderX.Value);
        result.ParseableAdd("SliderY", SliderY.Value);
        result.ParseableAdd("Pin", PinnedRows);
        result.ParseableAdd("Collapsed", _collapsed);
        result.ParseableAdd("Reduced", CurrentArrangement?.ReducedColumns());
        result.ParseableAdd("TempSort", _sortDefinitionTemporary);
        result.ParseableAdd("CursorPos", CellCollection.KeyOfCell(CursorPosColumn, CursorPosRow?.Row));
        return result.Parseable();
    }

    //UserControl überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
    [DebuggerNonUserCode]
    protected override void Dispose(bool disposing) {
        try {
            if (disposing) {
                Filter.Changed -= Filter_Changed;
                Filter.RowsChanged -= Filter_Changed;
                DatabaseSet(null, string.Empty); // Wichtig (nicht _Database) um Events zu lösen
                this.Invalidate_FilterInput(false);
                FilterOutput.Dispose();
            }
        } finally {
            base.Dispose(disposing);
        }
    }

    protected override void DrawControl(Graphics gr, States state) {
        if (InvokeRequired) {
            _ = Invoke(new Action(() => DrawControl(gr, state)));
            return;
        }

        if (IsDisposed) { return; }

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

        if (Database is not Database db || db.IsDisposed || DesignMode || ShowWaitScreen || _drawing) {
            DrawWaitScreen(gr);
            return;
        }

        if (CurrentArrangement is not ColumnViewCollection ca) {
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

                sortedRowData = RowsFilteredAndPinned();

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

                foreach (var thisRow in sortedRowData) {
                    if (thisRow?.Row is RowItem r) {
                        if (IsOnScreen(ca, thisRow, displayRectangleWoSlider)) {
                            if (r.IsInCache == null) { _ = rowsToRefreh.AddIfNotExists(r); }

                            var T = sortedRowData.IndexOf(thisRow);
                            firstVisibleRow = Math.Min(T, firstVisibleRow);
                            lastVisibleRow = Math.Max(T, lastVisibleRow);
                        }
                    }
                }

                _ = FillUp100(rowsToRefreh, sortedRowData);

                var (didreload, errormessage) = Database.RefreshRowData(rowsToRefreh);

                if (!string.IsNullOrEmpty(errormessage)) {
                    FormWithStatusBar.UpdateStatusBar(FehlerArt.Warnung, errormessage, true);
                }
                if (!didreload || count > 15) { break; }

                Invalidate_SortedRowData();
            } while (true);

            Draw_Table_Std(gr, sortedRowData, state, displayRectangleWoSlider, firstVisibleRow, lastVisibleRow, CurrentArrangement);

            _drawing = false;
        }
    }

    protected void InitializeSkin() {
        _cellFont = Skin.GetBlueFont(Design.Table_Cell, States.Standard).Scale(FontScale);
        _columnFont = Skin.GetBlueFont(Design.Table_Column, States.Standard).Scale(FontScale);
        _chapterFont = Skin.GetBlueFont(Design.Table_Cell_Chapter, States.Standard).Scale(FontScale);
        _columnFilterFont = BlueFont.Get(_columnFont.FontName, _columnFont.Size, false, false, false, false, true, Color.White, Color.Red, false, false, false);

        _newRowFont = Skin.GetBlueFont(Design.Table_Cell_New, States.Standard).Scale(FontScale);
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
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }
        if (CurrentArrangement is not ColumnViewCollection ca) { return; }

        lock (_lockUserAction) {
            if (_isinClick) { return; }
            _isinClick = true;

            CellOnCoordinate(ca, MousePos().X, MousePos().Y, out _mouseOverColumn, out _mouseOverRow);
            _isinClick = false;
        }
    }

    protected override void OnDoubleClick(System.EventArgs e) {
        //    base.OnDoubleClick(e); Wird komplett selbst gehandlet und das neue Ereigniss ausgelöst
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }

        if (CurrentArrangement is not ColumnViewCollection ca) { return; }

        lock (_lockUserAction) {
            if (_isinDoubleClick) { return; }
            _isinDoubleClick = true;
            CellOnCoordinate(ca, MousePos().X, MousePos().Y, out _mouseOverColumn, out _mouseOverRow);
            CellExtEventArgs ea = new(_mouseOverColumn, _mouseOverRow);
            DoubleClick?.Invoke(this, ea);

            if (!Mouse_IsInHead()) {
                if (_mouseOverRow != null || MousePos().Y <= ca.HeadSize(_columnFont) + _pix18) {
                    DoubleClick?.Invoke(this, ea);
                    Cell_Edit(ca, _mouseOverColumn, _mouseOverRow, true);
                }
            }
            _isinDoubleClick = false;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e) {
        base.OnKeyDown(e);

        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }
        if (CursorPosColumn is null) { return; }

        if (CurrentArrangement is not ColumnViewCollection ca) { return; }

        lock (_lockUserAction) {
            if (_isinKeyDown) { return; }
            _isinKeyDown = true;

            //_database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());

            switch (e.KeyCode) {
                case Keys.Oemcomma: // normales ,
                    if (e.Modifiers == Keys.Control) {
                        var lp = EditableErrorReason(CursorPosColumn, CursorPosRow, EditableErrorReasonType.EditCurrently, true, false, true);
                        Neighbour(CursorPosColumn, CursorPosRow, Direction.Oben, out _, out var newRow);
                        if (newRow == CursorPosRow) { lp = "Das geht nicht bei dieser Zeile."; }
                        if (string.IsNullOrEmpty(lp) && newRow?.Row != null) {
                            UserEdited(this, newRow.Row.CellGetString(CursorPosColumn), CursorPosColumn, CursorPosRow, true);
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
                        var l2 = EditableErrorReason(CursorPosColumn, CursorPosRow, EditableErrorReasonType.EditCurrently, true, false, true);
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
                    var l = EditableErrorReason(CursorPosColumn, CursorPosRow, EditableErrorReasonType.EditCurrently, true, false, true);
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
                    Cell_Edit(ca, CursorPosColumn, CursorPosRow, true);
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
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }

        if (CurrentArrangement is not ColumnViewCollection ca) { return; }

        lock (_lockUserAction) {
            if (_isinMouseDown) { return; }
            _isinMouseDown = true;
            //_database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
            CellOnCoordinate(ca, e.X, e.Y, out _mouseOverColumn, out _mouseOverRow);
            // Die beiden Befehle nur in Mouse Down!
            // Wenn der Cursor bei Click/Up/Down geändert wird, wird ein Ereignis ausgelöst.
            // Das könnte auch sehr Zeit intensiv sein. Dann kann die Maus inzwischen wo ander sein.
            // Somit würde das Ereignis doppelt und dreifach ausgelöste werden können.
            // Beipiel: MouseDown-> Bildchen im Pad erzeugen, dauert.... Maus bewegt sich
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
                    Invalidate_SortedRowData();
                }
            }
            _ = EnsureVisible(ca, _mouseOverColumn, _mouseOverRow);
            CursorPos_Set(_mouseOverColumn, _mouseOverRow, false);
            _isinMouseDown = false;
        }
    }

    protected override void OnMouseEnter(System.EventArgs e) {
        base.OnMouseEnter(e);
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }
        lock (_lockUserAction) {
            if (_isinMouseEnter) { return; }
            _isinMouseEnter = true;
            Forms.QuickInfo.Close();
            _isinMouseEnter = false;
        }
    }

    protected override void OnMouseLeave(System.EventArgs e) {
        base.OnMouseLeave(e);
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }
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
            if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }
            if (CurrentArrangement is not ColumnViewCollection ca) { return; }

            if (_isinMouseMove) { return; }

            _isinMouseMove = true;
            _mouseOverText = string.Empty;
            CellOnCoordinate(ca, e.X, e.Y, out _mouseOverColumn, out _mouseOverRow);
            if (e.Button != MouseButtons.None) {
                //_database?.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
            } else {
                if (_mouseOverColumn != null && e.Y < ca.HeadSize(_columnFont)) {
                    _mouseOverText = _mouseOverColumn.QuickInfoText(string.Empty);
                } else if (_mouseOverColumn != null && _mouseOverRow != null) {
                    if (_mouseOverColumn.Format.NeedTargetDatabase()) {
                        if (_mouseOverColumn.LinkedDatabase != null) {
                            switch (_mouseOverColumn.Format) {
                                case DataFormat.Verknüpfung_zu_anderer_Datenbank:

                                    var (lcolumn, _, info, _) = CellCollection.LinkedCellData(_mouseOverColumn, _mouseOverRow?.Row, true, false);
                                    if (lcolumn != null) { _mouseOverText = lcolumn.QuickInfoText(_mouseOverColumn.ReadableText() + " bei " + lcolumn.ReadableText() + ":"); }

                                    if (!string.IsNullOrEmpty(info) && db.IsAdministrator()) {
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
                    } else if (db.IsAdministrator()) {
                        _mouseOverText = Database.UndoText(_mouseOverColumn, _mouseOverRow?.Row);
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
        if (IsDisposed) { return; }
        base.OnMouseUp(e);

        lock (_lockUserAction) {
            if (Database is not Database db || db.IsDisposed || CurrentArrangement is not ColumnViewCollection ca) {
                Forms.QuickInfo.Close();
                return;
            }

            CellOnCoordinate(ca, e.X, e.Y, out _mouseOverColumn, out _mouseOverRow);
            if (CursorPosColumn != _mouseOverColumn || CursorPosRow != _mouseOverRow) { Forms.QuickInfo.Close(); }
            // TXTBox_Close() NICHT! Weil sonst nach dem Öffnen sofort wieder gschlossen wird
            // AutoFilter_Close() NICHT! Weil sonst nach dem Öffnen sofort wieder geschlossen wird
            FloatingForm.Close(this, Design.Form_KontextMenu);

            if (ca[_mouseOverColumn] is not ColumnViewItem viewItem) { return; }

            if (e.Button == MouseButtons.Left) {
                if (_mouseOverColumn != null) {
                    if (Mouse_IsInAutofilter(viewItem, e, ca)) {
                        var screenX = Cursor.Position.X - e.X;
                        var screenY = Cursor.Position.Y - e.Y;
                        AutoFilter_Show(ca, viewItem, screenX, screenY);
                        return;
                    }

                    if (Mouse_IsInRedcueButton(viewItem, e, ca)) {
                        viewItem.TmpReduced = !viewItem.TmpReduced;
                        viewItem.TmpDrawWidth = null;
                        Invalidate();
                        return;
                    }

                    if (_mouseOverRow?.Row is RowItem r && _mouseOverColumn.Format == DataFormat.Button) {
                        OnButtonCellClicked(new CellEventArgs(_mouseOverColumn, r));
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
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }

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
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }
        lock (_lockUserAction) {
            if (_isinSizeChanged) { return; }
            _isinSizeChanged = true;
            //Database?.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
            CurrentArrangement?.Invalidate_DrawWithOfAllItems();
            _isinSizeChanged = false;
        }
        Invalidate_SortedRowData(); // Zellen können ihre Größe ändern. z.B. die Zeilenhöhe
    }

    protected override void OnVisibleChanged(System.EventArgs e) {
        base.OnVisibleChanged(e);
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }
        lock (_lockUserAction) {
            if (_isinVisibleChanged) { return; }
            _isinVisibleChanged = true;
            //Database?.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
            _isinVisibleChanged = false;
        }
    }

    private static void Draw_CellTransparentDirect(Graphics gr, string txt, ShortenStyle style, Rectangle drawarea, BlueFont font, ColumnItem? contentHolderCellColumn, int pix16, BildTextVerhalten bildTextverhalten, States state, double scale) {
        if (contentHolderCellColumn == null) { return; }

        if (!ShowMultiLine(txt, contentHolderCellColumn.MultiLine)) {
            Draw_CellTransparentDirect_OneLine(gr, txt, contentHolderCellColumn, drawarea, 0, false, font, pix16, style, bildTextverhalten, state, scale);
        } else {
            var mei = txt.SplitAndCutByCrAndBr();
            if (contentHolderCellColumn.ShowMultiLineInOneLine) {
                Draw_CellTransparentDirect_OneLine(gr, mei.JoinWith("; "), contentHolderCellColumn, drawarea, 0, false, font, pix16, style, bildTextverhalten, state, scale);
            } else {
                var y = 0;
                for (var z = 0; z <= mei.GetUpperBound(0); z++) {
                    Draw_CellTransparentDirect_OneLine(gr, mei[z], contentHolderCellColumn, drawarea, y, z != mei.GetUpperBound(0), font, pix16, style, bildTextverhalten, state, scale);
                    y += CellItem.ContentSize(contentHolderCellColumn.KeyName, contentHolderCellColumn.Format, mei[z], font, style, pix16 - 1, bildTextverhalten, contentHolderCellColumn.Prefix, contentHolderCellColumn.Suffix, contentHolderCellColumn.DoOpticalTranslation, contentHolderCellColumn.OpticalReplace, scale, contentHolderCellColumn.ConstantHeightOfImageCode).Height;
                }
            }
        }
    }

    private static void Draw_CellTransparentDirect_OneLine(Graphics gr, string drawString, ColumnItem contentHolderColumnStyle, Rectangle drawarea, int txtYPix, bool changeToDot, BlueFont font, int pix16, ShortenStyle style, BildTextVerhalten bildTextverhalten, States state, double scale) {
        Rectangle r = new(drawarea.Left, drawarea.Top + txtYPix, drawarea.Width, pix16);

        if (r.Bottom + pix16 > drawarea.Bottom) {
            if (r.Bottom > drawarea.Bottom) { return; }
            if (changeToDot) { drawString = "..."; }// Die letzte Zeile noch ganz hinschreiben
        }

        var (text, qi) = CellItem.GetDrawingData(contentHolderColumnStyle.KeyName, contentHolderColumnStyle.Format, drawString, style, bildTextverhalten, contentHolderColumnStyle.Prefix, contentHolderColumnStyle.Suffix, contentHolderColumnStyle.DoOpticalTranslation, contentHolderColumnStyle.OpticalReplace, scale, contentHolderColumnStyle.ConstantHeightOfImageCode);
        var tmpImageCode = qi;
        if (tmpImageCode != null) { tmpImageCode = QuickImage.Get(tmpImageCode, Skin.AdditionalState(state)); }

        Skin.Draw_FormatedText(gr, text, tmpImageCode, (Alignment)contentHolderColumnStyle.Align, r, null, false, font, false);
    }

    /// <summary>
    /// Füllt die Liste rowsToExpand um expandCount Einträge auf. Ausgehend von rowToCheck
    /// </summary>
    /// <param name="rowsToExpand"></param>
    /// <param name="rowToCheck"></param>
    /// <param name="sortedRows"></param>
    /// <param name="expandCount"></param>
    /// <returns>Gibt false zurück, wenn ALLE Zeilen dadurch geladen sind.</returns>
    private static bool FillUp(ICollection<RowItem> rowsToExpand, RowItem rowToCheck, IReadOnlyList<RowData> sortedRows, int expandCount) {
        var indexPosition = -1;

        for (var z = 0; z < sortedRows.Count; z++) {
            var tmpr = sortedRows[z];
            if (!tmpr.MarkYellow && tmpr.Row == rowToCheck) { indexPosition = z; break; }
        }

        if (indexPosition == -1) { return false; } // Wie bitte?

        var modi = 0;

        while (expandCount > 0) {
            modi++;
            var n1 = indexPosition - modi;
            var n2 = indexPosition + modi;

            if (n1 < 0 && n2 >= sortedRows.Count) { return true; }

            #region Zeile "vorher" prüfen und aufnehmen

            if (n1 >= 0) {
                var tmpr = sortedRows[n1].Row;
                if (!tmpr.IsDisposed && tmpr.IsInCache == null && !rowsToExpand.Contains(tmpr)) {
                    rowsToExpand.Add(tmpr);
                    expandCount--;
                }
            }

            #endregion

            #region Zeile "nachher" prüfen und aufnehmen

            if (n2 < sortedRows.Count) {
                var tmpr = sortedRows[n2].Row;
                if (tmpr.IsInCache == null && !rowsToExpand.Contains(tmpr)) {
                    rowsToExpand.Add(tmpr);
                    expandCount--;
                }
            }

            #endregion
        }

        return false;
    }

    private static int GetPix(int pix, Font f, double scale) => f.FormatedText_NeededSize("@|", null, (int)((pix * scale) + 0.5)).Height;

    private static bool Mouse_IsInAutofilter(ColumnViewItem? viewItem, MouseEventArgs e, ColumnViewCollection ca) {
        if (!ca.ShowHead) { return false; }

        return viewItem?.Column != null && viewItem.TmpAutoFilterLocation.Width != 0 && viewItem.Column.AutoFilterSymbolPossible() && viewItem.TmpAutoFilterLocation.Contains(e.X, e.Y);
    }

    private static bool Mouse_IsInRedcueButton(ColumnViewItem? viewItem, MouseEventArgs e, ColumnViewCollection ca) {
        if (!ca.ShowHead) { return false; }
        return viewItem != null && viewItem.TmpReduceLocation.Width != 0 && viewItem.TmpReduceLocation.Contains(e.X, e.Y);
    }

    private static void NotEditableInfo(string reason) => Notification.Show(LanguageTool.DoTranslate(reason), ImageCode.Kreuz);

    private static bool ShowMultiLine(string txt, bool ml) {
        if (!ml) { return false; }

        if (txt.Contains("\r")) { return true; }
        if (txt.Contains("<br>")) { return true; }

        return false;
    }

    private static void UserEdited(Table table, string newValue, ColumnItem? cellInThisDatabaseColumn, RowData? cellInThisDatabaseRow, bool formatWarnung) {
        var er = table.EditableErrorReason(cellInThisDatabaseColumn, cellInThisDatabaseRow, EditableErrorReasonType.EditCurrently, true, false, true);
        if (!string.IsNullOrEmpty(er)) { NotEditableInfo(er); return; }

        if (cellInThisDatabaseColumn == null) { return; } // Dummy prüfung

        #region Den wahren Zellkern finden contentHolderCellColumn, contentHolderCellRow

        ColumnItem? contentHolderCellColumn = cellInThisDatabaseColumn;
        RowItem? contentHolderCellRow = cellInThisDatabaseRow?.Row;
        if (contentHolderCellRow != null && contentHolderCellColumn.Format == DataFormat.Verknüpfung_zu_anderer_Datenbank) {
            (contentHolderCellColumn, contentHolderCellRow, _, _) = CellCollection.LinkedCellData(contentHolderCellColumn, contentHolderCellRow, true, true);
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
            if (string.IsNullOrEmpty(newValue) || !(table.Database?.Row.IsNewRowPossible() ?? false)) { return; }

            var fe = table.EditableErrorReason(cellInThisDatabaseColumn.Database?.Column.First(), null, EditableErrorReasonType.EditCurrently, true, false, true);
            if (!string.IsNullOrEmpty(fe)) {
                NotEditableInfo(fe);
                return;
            }

            var newr = cellInThisDatabaseColumn.Database?.Row.GenerateAndAdd(newValue, "Neue Zeile über Tabellen-Ansicht");

            var l = table.RowsFiltered;
            if (newr != null && !l.Contains(newr)) {
                if (MessageBox.Show("Die neue Zeile ist ausgeblendet.<br>Soll sie <b>angepinnt</b> werden?", ImageCode.Pinnadel, "anpinnen", "abbrechen") == 0) {
                    table.PinAdd(newr);
                }
            }

            var sr = table.RowsFilteredAndPinned();
            var rd = sr.Get(newr);
            table.CursorPos_Set(table.View_ColumnFirst(), rd, true);
            return;
        }

        #endregion

        newValue = contentHolderCellColumn.AutoCorrect(newValue, false);
        if (contentHolderCellRow != null) {
            if (newValue == contentHolderCellRow.CellGetString(contentHolderCellColumn)) { return; }

            var f = CellCollection.EditableErrorReason(contentHolderCellColumn, contentHolderCellRow, EditableErrorReasonType.EditCurrently, true, false, true, false);
            if (!string.IsNullOrEmpty(f)) { NotEditableInfo(f); return; }
            contentHolderCellRow.CellSet(contentHolderCellColumn, newValue);

            if (table.Database == cellInThisDatabaseColumn.Database) { table.CursorPos_Set(cellInThisDatabaseColumn, cellInThisDatabaseRow, false); }
        }
    }

    private void _Database_CellValueChanged(object sender, CellChangedEventArgs e) {
        if (e.Row.IsDisposed || e.Column.IsDisposed) { return; }

        if (SortUsed() is RowSortDefinition rsd) {
            if (rsd.UsedForRowSort(e.Column) || e.Column == Database?.Column.SysChapter) {
                Invalidate_SortedRowData();
            }
        }

        if (CurrentArrangement is ColumnViewCollection cvc) {
            if (cvc[e.Column] != null) {
                if (e.Column.MultiLine ||
                    e.Column.BehaviorOfImageAndText is BildTextVerhalten.Nur_Bild or BildTextVerhalten.Wenn_möglich_Bild_und_immer_Text) {
                    Invalidate_SortedRowData(); // Zeichenhöhe kann sich ändern...
                }
            }
        }

        Invalidate_DrawWidth(e.Column);
        Invalidate();
    }

    private void _Database_ColumnContentChanged(object sender, ColumnEventArgs e) => Invalidate_AllColumnArrangements();

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

    private void _database_Disposing(object sender, System.EventArgs e) => DatabaseSet(null, string.Empty);

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
        _pg?.Update(e.Current);
    }

    private void _Database_Row_RowGotData(object sender, RowEventArgs e) => Invalidate_SortedRowData();

    private void _Database_SortParameterChanged(object sender, System.EventArgs e) => Invalidate_SortedRowData();

    private void _Database_StoreView(object sender, System.EventArgs e) =>
            //if (!string.IsNullOrEmpty(_StoredView)) { Develop.DebugPrint("Stored View nicht Empty!"); }
            _storedView = ViewToString();

    private void _Database_ViewChanged(object sender, System.EventArgs e) {
        InitializeSkin(); // Sicher ist sicher, um die neuen Schrift-Größen zu haben.
        Invalidate_AllColumnArrangements();
        CursorPos_Set(CursorPosColumn, CursorPosRow, true);
        Invalidate();
    }

    private void AutoFilter_Close() {
        if (_autoFilter != null) {
            _autoFilter.FilterCommand -= AutoFilter_FilterCommand;
            _autoFilter?.Dispose();
            _autoFilter = null;
        }
    }

    private void AutoFilter_FilterCommand(object sender, FilterCommandEventArgs e) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }

        switch (e.Command.ToLower()) {
            case "":
                break;

            case "filter":
                Filter.RemoveOtherAndAdd(e.Filter);
                //Filter.Remove(e.Column);
                //Filter.Add(e.Filter);
                break;

            case "filterdelete":
                Filter.Remove(e.Column);
                break;

            case "doeinzigartig":
                Filter.Remove(e.Column);
                e.Column.GetUniques(RowsVisibleUnique(), out var einzigartig, out _);
                if (einzigartig.Count > 0) {
                    Filter.Add(new FilterItem(e.Column, FilterType.Istgleich_ODER_GroßKleinEgal, einzigartig));
                    Notification.Show("Die aktuell einzigartigen Einträge wurden berechnet<br>und als <b>ODER-Filter</b> gespeichert.", ImageCode.Trichter);
                } else {
                    Notification.Show("Filterung dieser Spalte gelöscht,<br>da <b>alle Einträge</b> mehrfach vorhanden sind.", ImageCode.Trichter);
                }
                break;

            case "donichteinzigartig":
                Filter.Remove(e.Column);
                e.Column.GetUniques(RowsVisibleUnique(), out _, out var xNichtEinzigartig);
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
                    var clipTmp = Clipboard.GetText().RemoveChars(Char_NotFromClip).TrimEnd("\r\n");
                    Filter.Remove(e.Column);

                    var searchValue = new List<string>(clipTmp.SplitAndCutByCr()).SortedDistinctList();

                    if (searchValue.Count > 0) {
                        Filter.Add(new FilterItem(e.Column, FilterType.IstGleich_ODER | FilterType.GroßKleinEgal, searchValue));
                    }
                    break;
                }

            case "donotclipboard": {
                    var clipTmp = Clipboard.GetText().RemoveChars(Char_NotFromClip).TrimEnd("\r\n");
                    Filter.Remove(e.Column);

                    var searchValue = e.Column.Contents();//  db.Export_CSV(FirstRow.Without, e.Column, null).SplitAndCutByCrToList().SortedDistinctList();
                    searchValue.RemoveString(clipTmp.SplitAndCutByCrToList().SortedDistinctList(), false);

                    if (searchValue.Count > 0) {
                        Filter.Add(new FilterItem(e.Column, FilterType.IstGleich_ODER | FilterType.GroßKleinEgal, searchValue));
                    }
                    break;
                }
            default:
                Develop.DebugPrint("Unbekannter Command:   " + e.Command);
                break;
        }
        OnAutoFilterClicked(new FilterEventArgs(e.Filter));
    }

    private void AutoFilter_Show(ColumnViewCollection ca, ColumnViewItem columnviewitem, int screenx, int screeny) {
        if (columnviewitem.Column == null) { return; }
        if (!ca.ShowHead) { return; }
        if (!columnviewitem.Column.AutoFilterSymbolPossible()) { return; }
        if (Filter.Any(thisFilter => thisFilter != null && thisFilter.Column == columnviewitem.Column && !string.IsNullOrEmpty(thisFilter.Origin))) {
            MessageBox.Show("Dieser Filter wurde<br>automatisch gesetzt.", ImageCode.Information, "OK");
            return;
        }
        //Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
        //OnBeforeAutoFilterShow(new ColumnEventArgs(columnviewitem.Column));
        _autoFilter = new AutoFilter(columnviewitem.Column, Filter, PinnedRows, columnviewitem.DrawWidth(DisplayRectangleWithoutSlider(), _pix16, _cellFont));
        _autoFilter.Position_LocateToPosition(new Point(screenx + columnviewitem.OrderTmpSpalteX1 ?? 0, screeny + ca.HeadSize(_columnFont)));
        _autoFilter.Show();
        _autoFilter.FilterCommand += AutoFilter_FilterCommand;
        Develop.Debugprint_BackgroundThread();
    }

    private int Autofilter_Text(ColumnItem column) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return 0; }
        if (column.TmpIfFilterRemoved != null) { return (int)column.TmpIfFilterRemoved; }
        using var fc = (FilterCollection)Filter.Clone("Autofilter_Text");
        fc.Remove(column);

        var ro = fc.Rows;
        var c = RowsFiltered.Count - ro.Count;
        column.TmpIfFilterRemoved = c;
        return c;
    }

    /// <summary>
    /// Gibt die Anzahl der SICHTBAREN Zeilen zurück, die mehr angezeigt werden würden, wenn dieser Filter deaktiviert wäre.
    /// </summary>
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

    private void Cell_Edit(ColumnViewCollection ca, ColumnItem? cellInThisDatabaseColumn, RowData? cellInThisDatabaseRow, bool preverDropDown) {
        var f = EditableErrorReason(cellInThisDatabaseColumn, cellInThisDatabaseRow, EditableErrorReasonType.EditCurrently, true, true, true);
        if (!string.IsNullOrEmpty(f)) { NotEditableInfo(f); return; }
        if (cellInThisDatabaseColumn == null) { return; }// Klick ins Leere

        ColumnItem? contentHolderCellColumn = cellInThisDatabaseColumn;
        RowItem? contentHolderCellRow = cellInThisDatabaseRow?.Row;

        if (cellInThisDatabaseColumn.Format == DataFormat.Verknüpfung_zu_anderer_Datenbank) {
            (contentHolderCellColumn, contentHolderCellRow, _, _) = CellCollection.LinkedCellData(contentHolderCellColumn, contentHolderCellRow, true, true);
        }

        if (contentHolderCellColumn == null) {
            NotEditableInfo("Keine Spalte angeklickt.");
            return;
        }

        var dia = ColumnItem.UserEditDialogTypeInTable(contentHolderCellColumn, preverDropDown);

        switch (dia) {
            case EditTypeTable.Textfeld:
                _ = Cell_Edit_TextBox(ca, cellInThisDatabaseColumn, cellInThisDatabaseRow, contentHolderCellColumn, contentHolderCellRow, BTB, 0, 0);
                break;

            case EditTypeTable.Textfeld_mit_Auswahlknopf:
                _ = Cell_Edit_TextBox(ca, cellInThisDatabaseColumn, cellInThisDatabaseRow, contentHolderCellColumn, contentHolderCellRow, BCB, 20, 18);
                break;

            case EditTypeTable.Dropdown_Single:
                Cell_Edit_Dropdown(ca, cellInThisDatabaseColumn, cellInThisDatabaseRow, contentHolderCellColumn, contentHolderCellRow);
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

    private void Cell_Edit_Color(ColumnItem cellInThisDatabaseColumn, RowData? cellInThisDatabaseRow) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }

        ColorDialog colDia = new ColorDialog();

        if (cellInThisDatabaseRow?.Row is RowItem r) {
            colDia.Color = r.CellGetColor(cellInThisDatabaseColumn);
        }
        colDia.Tag = new List<object?> { cellInThisDatabaseColumn, cellInThisDatabaseRow };
        List<int> colList = [];
        foreach (var thisRowItem in db.Row) {
            if (thisRowItem != null) {
                if (thisRowItem.CellGetInteger(cellInThisDatabaseColumn) != 0) {
                    colList.Add(thisRowItem.CellGetColorBgr(cellInThisDatabaseColumn));
                }
            }
        }
        colList.Sort();
        colDia.CustomColors = colList.Distinct().ToArray();
        _ = colDia.ShowDialog();
        colDia.Dispose();

        UserEdited(this, Color.FromArgb(255, colDia.Color).ToArgb().ToString(), cellInThisDatabaseColumn, cellInThisDatabaseRow, false);
    }

    private void Cell_Edit_Dropdown(ColumnViewCollection ca, ColumnItem cellInThisDatabaseColumn, RowData? cellInThisDatabaseRow, ColumnItem contentHolderCellColumn, RowItem? contentHolderCellRow) {
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
            Appearance = ListBoxAppearance.DropdownSelectbox
        };

        ItemCollectionList.ItemCollectionList.GetItemCollection(t, contentHolderCellColumn, contentHolderCellRow, ShortenStyle.Replaced, 1000);
        if (t.Count == 0) {
            // Hm ... Dropdown kein Wert vorhanden.... also gar kein Dropdown öffnen!
            if (contentHolderCellColumn.TextBearbeitungErlaubt) { Cell_Edit(ca, cellInThisDatabaseColumn, cellInThisDatabaseRow, false); } else {
                NotEditableInfo("Keine Items zum Auswählen vorhanden.");
            }
            return;
        }

        if (contentHolderCellColumn.TextBearbeitungErlaubt) {
            if (t.Count == 0 && (cellInThisDatabaseRow?.Row.CellIsNullOrEmpty(cellInThisDatabaseColumn) ?? true)) {
                // Bei nur einem Wert, wenn Texteingabe erlaubt, Dropdown öffnen
                Cell_Edit(ca, cellInThisDatabaseColumn, cellInThisDatabaseRow, false);
                return;
            }
            _ = t.Add("Erweiterte Eingabe", "#Erweitert", QuickImage.Get(ImageCode.Stift), true, FirstSortChar + "1");
            _ = t.AddSeparator(FirstSortChar + "2");
            //t.Sort();
        }

        var dropDownMenu = FloatingInputBoxListBoxStyle.Show(t, new CellExtEventArgs(cellInThisDatabaseColumn, cellInThisDatabaseRow), this, Translate);
        dropDownMenu.ItemClicked += DropDownMenu_ItemClicked;
        Develop.Debugprint_BackgroundThread();
    }

    private bool Cell_Edit_TextBox(ColumnViewCollection ca, ColumnItem cellInThisDatabaseColumn, RowData? cellInThisDatabaseRow, ColumnItem contentHolderCellColumn, RowItem? contentHolderCellRow, TextBox box, int addWith, int isHeight) {
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

        if (ca[cellInThisDatabaseColumn] is not ColumnViewItem viewItem) {
            NotEditableInfo("Spalte nicht angezeigt.");
            return false;
        }

        if (contentHolderCellRow != null) {
            var h = cellInThisDatabaseRow?.DrawHeight ?? _pix16;// Row_DrawHeight(cellInThisDatabaseRow, DisplayRectangle);
            if (isHeight > 0) { h = isHeight; }
            box.Location = new Point(viewItem.OrderTmpSpalteX1 ?? 0, DrawY(ca, cellInThisDatabaseRow));
            box.Size = new Size(viewItem.DrawWidth(DisplayRectangle, _pix16, _cellFont) + addWith, h);
            box.Text = contentHolderCellRow.CellGetString(contentHolderCellColumn);
        } else {
            // Neue Zeile...
            box.Location = new Point(viewItem.OrderTmpSpalteX1 ?? 0, ca.HeadSize(_columnFont));
            box.Size = new Size(viewItem.DrawWidth(DisplayRectangle, _pix16, _cellFont) + addWith, _pix18);
            box.Text = string.Empty;
        }

        box.GetStyleFrom(contentHolderCellColumn);

        box.Tag = new List<object?> { cellInThisDatabaseColumn, cellInThisDatabaseRow };

        if (box is ComboBox cbox) {
            ItemCollectionList.ItemCollectionList.GetItemCollection(cbox.Item, contentHolderCellColumn, contentHolderCellRow, ShortenStyle.Replaced, 1000);
            if (cbox.Item.Count == 0) {
                return Cell_Edit_TextBox(ca, cellInThisDatabaseColumn, cellInThisDatabaseRow, contentHolderCellColumn, contentHolderCellRow, BTB, 0, 0);
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

    private void CellOnCoordinate(ColumnViewCollection ca, int xpos, int ypos, out ColumnItem? column, out RowData? row) {
        column = ca.ColumnOnCoordinate(xpos, DisplayRectangleWithoutSlider(), _pix16, _cellFont);
        row = RowOnCoordinate(ca, ypos);
    }

    private void CloseAllComponents() {
        if (InvokeRequired) {
            _ = Invoke(new Action(CloseAllComponents));
            return;
        }
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }
        TXTBox_Close(BTB);
        TXTBox_Close(BCB);
        FloatingForm.Close(this);
        AutoFilter_Close();
        Forms.QuickInfo.Close();
    }

    private void Column_ItemRemoving(object sender, ColumnEventArgs e) {
        if (e.Column == CursorPosColumn) {
            CursorPos_Reset();
        }
        if (e.Column == _mouseOverColumn) {
            _mouseOverColumn = null;
        }
    }

    private bool ComputeAllColumnPositions() {
        try {
            // Kommt vor, dass spontan doch geparsed wird...
            //if (Database?.ColumnArrangements == null || _arrangementNr >= Database.ColumnArrangements.Count) { return false; }

            if (CurrentArrangement is not ColumnViewCollection ca) { return false; }
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
                        maxX += thisViewItem.DrawWidth(displayR, _pix16, _cellFont);
                        _wiederHolungsSpaltenWidth = Math.Max(_wiederHolungsSpaltenWidth, maxX);
                    } else {
                        thisViewItem.OrderTmpSpalteX1 = se ? (int)(maxX - sx) : maxX;
                        maxX += thisViewItem.DrawWidth(displayR, _pix16, _cellFont);
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
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }
        Neighbour(CursorPosColumn, CursorPosRow, richtung, out var newCol, out var newRow);
        CursorPos_Set(newCol, newRow, richtung != Direction.Nichts);
    }

    private void Database_InvalidateView(object sender, System.EventArgs e) => Invalidate();

    //private void Database_IsTableVisibleForUser(object sender, VisibleEventArgs e) => e.IsVisible = true;

    private Rectangle DisplayRectangleWithoutSlider() => new(DisplayRectangle.Left, DisplayRectangle.Left, DisplayRectangle.Width - SliderY.Width, DisplayRectangle.Height - SliderX.Height);

    private void Draw_Border(Graphics gr, ColumnViewItem column, Rectangle displayRectangleWoSlider, bool onlyhead, ColumnViewCollection ca) {
        var yPos = displayRectangleWoSlider.Height;
        if (onlyhead) { yPos = ca.HeadSize(_columnFont); }
        for (var z = 0; z <= 1; z++) {
            int xPos;
            ColumnLineStyle lin;
            if (z == 0) {
                xPos = column.OrderTmpSpalteX1 ?? 0;
                lin = column.Column?.LineLeft ?? ColumnLineStyle.Dünn;
            } else {
                xPos = (column.OrderTmpSpalteX1 ?? 0) + column.DrawWidth(displayRectangleWoSlider, _pix16, _cellFont);
                lin = column.Column?.LineRight ?? ColumnLineStyle.Ohne;
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
                    var c = Skin.Color_Border(Design.Table_Lines_thick, States.Standard);
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

    private void Draw_CellAsButton(Graphics gr, ColumnItem cellInThisDatabaseColumn, RowItem cellInThisDatabaseRow, Rectangle cellrectangle) {
        ButtonCellEventArgs e = new(cellInThisDatabaseColumn, cellInThisDatabaseRow);
        OnNeedButtonArgs(e);

        var s = States.Standard;
        if (!Enabled) { s = States.Standard_Disabled; }
        if (e.Cecked) { s |= States.Checked; }

        var x = new ExtText(Design.Button_CheckBox, s);
        Button.DrawButton(this, gr, Design.Button_CheckBox, s, e.Image, Alignment.Horizontal_Vertical_Center, false, x, e.Text, cellrectangle, true);
    }

    /// <summary>
    /// Zeichnet die gesamte Zelle ohne Hintergrund und prüft noch, ob der verlinkte Inhalt gezeichnet werden soll.
    /// </summary>
    /// <param name="gr"></param>
    /// <param name="cellInThisDatabaseColumn"></param>
    /// <param name="cellInThisDatabaseRow"></param>
    /// <param name="cellrectangle"></param>
    /// <param name="font"></param>
    /// <param name="state"></param>
    private void Draw_CellTransparent(Graphics gr, ColumnItem cellInThisDatabaseColumn, RowItem cellInThisDatabaseRow, Rectangle cellrectangle, BlueFont font, States state) {
        if (cellInThisDatabaseColumn.Format == DataFormat.Verknüpfung_zu_anderer_Datenbank) {
            var (lcolumn, lrow, _, _) = CellCollection.LinkedCellData(cellInThisDatabaseColumn, cellInThisDatabaseRow, false, false);

            if (lcolumn != null && lrow != null) {
                Draw_CellTransparentDirect(gr, cellInThisDatabaseColumn, cellInThisDatabaseRow, cellrectangle, lcolumn, lrow, font, state);
            } else {
                if (cellInThisDatabaseRow.Database?.IsAdministrator() ?? false) {
                    gr.DrawImage(QuickImage.Get("Warnung|10||||||120||60"), cellrectangle.Left + 3, cellrectangle.Top + 1);
                }
            }
            return;
        }

        Draw_CellTransparentDirect(gr, cellInThisDatabaseColumn, cellInThisDatabaseRow, cellrectangle, cellInThisDatabaseColumn, cellInThisDatabaseRow, font, state);
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
    /// <param name="state"></param>
    private void Draw_CellTransparentDirect(Graphics gr, ColumnItem cellInThisDatabaseColumn, RowItem cellInThisDatabaseRow, Rectangle cellrectangle, ColumnItem contentHolderCellColumn, RowItem contentHolderCellRow, BlueFont font, States state) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }

        if (cellInThisDatabaseColumn.Format == DataFormat.Button) {
            Draw_CellAsButton(gr, cellInThisDatabaseColumn, cellInThisDatabaseRow, cellrectangle);
            return;
        }

        var toDraw = contentHolderCellRow.CellGetString(contentHolderCellColumn);

        Draw_CellTransparentDirect(gr, toDraw, ShortenStyle.Replaced, cellrectangle, font, contentHolderCellColumn, _pix16, contentHolderCellColumn.BehaviorOfImageAndText, state, db.GlobalScale);
    }

    private void Draw_Column_Body(Graphics gr, ColumnViewItem cellInThisDatabaseColumn, Rectangle displayRectangleWoSlider, ColumnViewCollection ca) {
        if (cellInThisDatabaseColumn.Column is not ColumnItem tmpc) { return; }

        gr.SmoothingMode = SmoothingMode.None;
        gr.FillRectangle(new SolidBrush(tmpc.BackColor), cellInThisDatabaseColumn.OrderTmpSpalteX1 ?? 0, ca.HeadSize(_columnFont), cellInThisDatabaseColumn.DrawWidth(displayRectangleWoSlider, _pix16, _cellFont), displayRectangleWoSlider.Height);
        Draw_Border(gr, cellInThisDatabaseColumn, displayRectangleWoSlider, false, ca);
    }

    private void Draw_Column_Cells(Graphics gr, IReadOnlyList<RowData> sr, ColumnViewItem viewItem, Rectangle displayRectangleWoSlider, int firstVisibleRow, int lastVisibleRow, States state, bool firstOnScreen, ColumnViewCollection ca) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }
        if (viewItem.Column is not ColumnItem tmpc) { return; }

        if (Database.Column.First() is ColumnItem columnFirst && tmpc == columnFirst && UserEdit_NewRowAllowed()) {
            Skin.Draw_FormatedText(gr, "[Neue Zeile]", QuickImage.Get(ImageCode.PlusZeichen, _pix16), Alignment.Left, new Rectangle(viewItem.OrderTmpSpalteX1 ?? 0 + 1, (int)(-SliderY.Value + ca.HeadSize(_columnFont) + 1), viewItem.DrawWidth(displayRectangleWoSlider, _pix16, _cellFont) - 2, 16 - 2), this, false, _newRowFont, Translate);
        }

        for (var zei = firstVisibleRow; zei <= lastVisibleRow; zei++) {
            var currentRow = sr[zei];
            gr.SmoothingMode = SmoothingMode.None;

            Rectangle cellrectangle = new(viewItem.OrderTmpSpalteX1 ?? 0, DrawY(ca, currentRow),
                viewItem.DrawWidth(displayRectangleWoSlider, _pix16, _cellFont), Math.Max(currentRow.DrawHeight, _pix16));

            if (currentRow.Expanded) {
                if (currentRow.MarkYellow) { gr.FillRectangle(BrushYellowTransparent, cellrectangle); }

                if (db.IsAdministrator()) {
                    if (currentRow.Row.NeedsUpdate()) {
                        gr.FillRectangle(BrushRedTransparent, cellrectangle);
                        db.Row.AddRowWithChangedValue(currentRow.Row);
                    }
                }

                gr.DrawLine(Skin.PenLinieDünn, cellrectangle.Left, cellrectangle.Bottom - 1, cellrectangle.Right - 1, cellrectangle.Bottom - 1);

                if (!Thread.CurrentThread.IsBackground && CursorPosColumn == tmpc && CursorPosRow == currentRow) {
                    _tmpCursorRect = cellrectangle;
                    _tmpCursorRect.Height -= 1;
                    Draw_Cursor(gr, displayRectangleWoSlider, false);
                }

                Draw_CellTransparent(gr, tmpc, currentRow.Row, cellrectangle, _cellFont, state);

                if (_unterschiede != null && _unterschiede != currentRow.Row) {
                    if (currentRow.Row.CellGetString(tmpc) != _unterschiede.CellGetString(tmpc)) {
                        Rectangle tmpr = new(viewItem.OrderTmpSpalteX1 ?? 0 + 1, DrawY(ca, currentRow) + 1,
                            viewItem.DrawWidth(displayRectangleWoSlider, _pix16, _cellFont) - 2, currentRow.DrawHeight - 2);
                        gr.DrawRectangle(PenRed1, tmpr);
                    }
                }
            }

            if (firstOnScreen) {
                // Überschrift in der ersten Spalte zeichnen
                currentRow.CaptionPos = Rectangle.Empty;
                if (currentRow.ShowCap) {
                    var si = gr.MeasureString(currentRow.Chapter, (Font)_chapterFont);
                    gr.FillRectangle(new SolidBrush(Skin.Color_Back(Design.Table_And_Pad, States.Standard).SetAlpha(50)), 1, DrawY(ca, currentRow) - RowCaptionSizeY, displayRectangleWoSlider.Width - 2, RowCaptionSizeY);
                    currentRow.CaptionPos = new Rectangle(1, DrawY(ca, currentRow) - _rowCaptionFontY, (int)si.Width + 28, (int)si.Height);

                    if (_collapsed.Contains(currentRow.Chapter)) {
                        var x = new ExtText(Design.Button_CheckBox, States.Checked);
                        Button.DrawButton(this, gr, Design.Button_CheckBox, States.Checked, null, Alignment.Horizontal_Vertical_Center, false, x, string.Empty, currentRow.CaptionPos, false);
                        gr.DrawImage(QuickImage.Get("Pfeil_Unten_Scrollbar|14|||FF0000||200|200"), 5, DrawY(ca, currentRow) - _rowCaptionFontY + 6);
                    } else {
                        var x = new ExtText(Design.Button_CheckBox, States.Standard);
                        Button.DrawButton(this, gr, Design.Button_CheckBox, States.Standard, null, Alignment.Horizontal_Vertical_Center, false, x, string.Empty, currentRow.CaptionPos, false);
                        gr.DrawImage(QuickImage.Get("Pfeil_Rechts_Scrollbar|14|||||0"), 5, DrawY(ca, currentRow) - _rowCaptionFontY + 6);
                    }
                    _chapterFont.DrawString(gr, currentRow.Chapter, 23, DrawY(ca, currentRow) - _rowCaptionFontY);
                    gr.DrawLine(Skin.PenLinieDick, 0, DrawY(ca, currentRow), displayRectangleWoSlider.Width, DrawY(ca, currentRow));
                }
            }
        }
    }

    /// <summary>
    /// Zeichnet die Überschriften
    /// </summary>
    /// <param name="gr"></param>
    /// <param name="ca"></param>
    /// <param name="displayRectangleWoSlider"></param>
    private void Draw_Column_Head_Captions(Graphics gr, ColumnViewCollection ca, Rectangle displayRectangleWoSlider) {
        var prevViewItemWithOtherCaption = new ColumnViewItem?[3];
        var prevPermanentViewItemWithOtherCaption = new ColumnViewItem[3];
        ColumnViewItem? prevViewItem = null;
        var permaX = 0;

        foreach (var thisViewItem in ca) {
            var thisViewItemWidth = thisViewItem.DrawWidth(displayRectangleWoSlider, _pix16, _cellFont);
            var thisViewItemLeft = thisViewItem.OrderTmpSpalteX1 ?? 0;
            var thisViewItemRight = thisViewItemLeft + thisViewItemWidth;

            if (thisViewItem.ViewType == ViewType.PermanentColumn) { permaX = Math.Max(permaX, thisViewItemRight); }

            if (thisViewItem.ViewType == ViewType.PermanentColumn || thisViewItemRight > permaX) {
                for (var u = 0; u < 3; u++) {
                    var newCaptionGroup = thisViewItem?.Column?.CaptionGroup(u) ?? string.Empty;
                    var prevCaptionGroup = prevViewItemWithOtherCaption[u]?.Column?.CaptionGroup(u) ?? string.Empty;

                    if (newCaptionGroup != prevCaptionGroup) {
                        if (!string.IsNullOrEmpty(prevCaptionGroup) && prevViewItem != null && prevViewItemWithOtherCaption[u] is ColumnViewItem tmp) {
                            var le = Math.Max(0, prevViewItemWithOtherCaption[u]?.OrderTmpSpalteX1 ?? 0);
                            var re = (prevViewItem.OrderTmpSpalteX1 ?? 0) + prevViewItem.DrawWidth(displayRectangleWoSlider, _pix16, _cellFont) - 1;
                            if (thisViewItem?.ViewType != ViewType.PermanentColumn && tmp.ViewType != ViewType.PermanentColumn) {
                                le = Math.Max(le, permaX);
                            }

                            if (thisViewItem?.ViewType != ViewType.PermanentColumn && tmp.ViewType == ViewType.PermanentColumn) {
                                var tmp2 = prevPermanentViewItemWithOtherCaption[u].DrawWidth(displayRectangleWoSlider, _pix16, _cellFont);
                                re = Math.Max(re, prevPermanentViewItemWithOtherCaption[u].OrderTmpSpalteX1 ?? 0 + tmp2);
                            }

                            if (le < re && tmp.Column is ColumnItem tmpc) {
                                Rectangle r = new(le, u * ColumnCaptionSizeY, re - le, ColumnCaptionSizeY);
                                gr.FillRectangle(new SolidBrush(tmpc.BackColor), r);
                                gr.FillRectangle(new SolidBrush(Color.FromArgb(80, 200, 200, 200)), r);
                                gr.DrawRectangle(Skin.PenLinieKräftig, r);
                                Skin.Draw_FormatedText(gr, prevCaptionGroup, null, Alignment.Horizontal_Vertical_Center, r, this, false, _columnFont, Translate);
                            }
                        }

                        prevViewItemWithOtherCaption[u] = thisViewItem;
                        if (thisViewItem?.ViewType == ViewType.PermanentColumn) {
                            prevPermanentViewItemWithOtherCaption[u] = thisViewItem;
                        }
                    }
                }
            }
            prevViewItem = thisViewItem;
        }
    }

    private void Draw_Cursor(Graphics gr, Rectangle displayRectangleWoSlider, bool onlyCursorLines) {
        if (_tmpCursorRect.Width < 1) { return; }

        var stat = States.Standard;
        if (Focused()) { stat = States.Standard_HasFocus; }

        if (onlyCursorLines) {
            Pen pen = new(Skin.Color_Border(Design.Table_Cursor, stat).SetAlpha(180));
            gr.DrawRectangle(pen, new Rectangle(-1, _tmpCursorRect.Top - 1, displayRectangleWoSlider.Width + 2, _tmpCursorRect.Height + 1));
        } else {
            Skin.Draw_Back(gr, Design.Table_Cursor, stat, _tmpCursorRect, this, false);
            Skin.Draw_Border(gr, Design.Table_Cursor, stat, _tmpCursorRect);
        }
    }

    private void Draw_Table_Std(Graphics gr, List<RowData> sr, States state, Rectangle displayRectangleWoSlider, int firstVisibleRow, int lastVisibleRow, ColumnViewCollection? ca) {
        try {
            if (IsDisposed || Database is not Database db || db.IsDisposed || ca == null) { return; }   // Kommt vor, dass spontan doch geparsed wird...
            Skin.Draw_Back(gr, Design.Table_And_Pad, state, base.DisplayRectangle, this, true);

            // Maximale Rechten Pixel der Permanenten Columns ermitteln
            var permaX = 0;
            foreach (var viewItem in ca) {
                if (viewItem?.Column != null && viewItem.ViewType == ViewType.PermanentColumn) {
                    if (viewItem.TmpDrawWidth == null) {
                        // Veränderte Werte!
                        DrawControl(gr, state);
                        return;
                    }

                    permaX = Math.Max(permaX, viewItem.OrderTmpSpalteX1 ?? 0 + viewItem.DrawWidth(displayRectangleWoSlider, _pix16, _cellFont));
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

            // Überschriften 1-3 Zeichnen
            Draw_Column_Head_Captions(gr, ca, displayRectangleWoSlider);
            Skin.Draw_Border(gr, Design.Table_And_Pad, state, displayRectangleWoSlider);

            ////gr.Clear(Color.White);
            //gr.DrawString("Test " + Constants.GlobalRND.Next().ToString(false), new Font("Arial", 20), Brushes.Black, new Point(0, 0));

            //return;

            if (db.HasPendingChanges && !string.IsNullOrEmpty(db.Filename)) { gr.DrawImage(QuickImage.Get(ImageCode.Stift, 16), 16, 8); }
            if (db.ReadOnly) {
                gr.DrawImage(QuickImage.Get(ImageCode.Schloss, 32), 16, 8);
                if (!string.IsNullOrEmpty(db.FreezedReason)) {
                    BlueFont.DrawString(gr, db.FreezedReason, _columnFont, Brushes.Blue, 52, 12);
                }
            }

            if (db.AmITemporaryMaster(false)) {
                gr.DrawImage(QuickImage.Get(ImageCode.Stern, 8), 0, 0);
            } else if (db.AmITemporaryMaster(true)) {
                gr.DrawImage(QuickImage.Get(ImageCode.Stern, 8, Color.Blue, Color.Transparent), 0, 0);
            }
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
                    if ((col == TableDrawColumn.NonPermament && viewItem.ViewType != ViewType.PermanentColumn && (viewItem.OrderTmpSpalteX1 ?? 0) + viewItem.DrawWidth(displayRectangleWoSlider, _pix16, _cellFont) > permaX) ||
                        (col == TableDrawColumn.Permament && viewItem.ViewType == ViewType.PermanentColumn)) {
                        switch (type) {
                            case TableDrawType.ColumnBackBody:
                                Draw_Column_Body(gr, viewItem, displayRectangleWoSlider, ca);
                                break;

                            case TableDrawType.Cells:
                                Draw_Column_Cells(gr, sr, viewItem, displayRectangleWoSlider, firstVisibleRow, lastVisibleRow, state, firstOnScreen, ca);
                                break;

                            case TableDrawType.ColumnHead:
                                Draw_Column_Head(gr, viewItem, displayRectangleWoSlider, lfdno, ca);
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

        Skin.Draw_Back(gr, Design.Table_And_Pad, States.Standard_Disabled, base.DisplayRectangle, this, true);

        var i = QuickImage.Get(ImageCode.Uhr, 64);
        gr.DrawImage(i, (Width - 64) / 2, (Height - 64) / 2);
        Skin.Draw_Border(gr, Design.Table_And_Pad, States.Standard_Disabled, base.DisplayRectangle);
    }

    private int DrawY(ColumnViewCollection ca, RowData? r) => r == null ? 0 : r.Y + ca.HeadSize(_columnFont) - (int)SliderY.Value;

    /// <summary>
    /// Berechent die Y-Position auf dem aktuellen Controll
    /// </summary>
    /// <returns></returns>
    private void DropDownMenu_ItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
        FloatingForm.Close(this);
        if (string.IsNullOrEmpty(e.ClickedCommand)) { return; }

        if (CurrentArrangement is not ColumnViewCollection ca) { return; }

        CellExtEventArgs? ck = null;
        if (e.HotItem is CellExtEventArgs tmp) { ck = tmp; }

        if (ck?.Column == null) { return; }

        var toAdd = e.ClickedCommand;
        var toRemove = string.Empty;
        if (toAdd == "#Erweitert") {
            Cell_Edit(ca, ck.Column, ck.RowData, false);
            return;
        }
        if (ck.RowData == null) {
            // Neue Zeile!
            UserEdited(this, toAdd, ck.Column, null, false);
            return;
        }

        //if (ck.RowData.Row == null) { return; }

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

    private bool EnsureVisible(ColumnViewCollection ca, RowData? rowdata) {
        if (rowdata == null) { return false; }
        var r = DisplayRectangleWithoutSlider();
        if (DrawY(ca, rowdata) < ca.HeadSize(_columnFont)) {
            SliderY.Value = SliderY.Value + DrawY(ca, rowdata) - ca.HeadSize(_columnFont);
        } else if (DrawY(ca, rowdata) + rowdata.DrawHeight > r.Height) {
            SliderY.Value = SliderY.Value + DrawY(ca, rowdata) + rowdata.DrawHeight - r.Height;
        }
        return true;
    }

    private bool EnsureVisible(ColumnViewItem? viewItem) {
        if (viewItem?.Column == null) { return false; }
        if (viewItem.OrderTmpSpalteX1 == null && !ComputeAllColumnPositions()) { return false; }
        var r = DisplayRectangleWithoutSlider();
        _ = ComputeAllColumnPositions();
        if (viewItem.ViewType == ViewType.PermanentColumn) {
            if (viewItem.OrderTmpSpalteX1 + viewItem.DrawWidth(r, _pix16, _cellFont) <= r.Width) { return true; }
            //Develop.DebugPrint(enFehlerArt.Info,"Unsichtbare Wiederholungsspalte: " + ViewItem.Column.KeyName);
            return false;
        }
        if (viewItem.OrderTmpSpalteX1 < _wiederHolungsSpaltenWidth) {
            SliderX.Value = SliderX.Value + (int)viewItem.OrderTmpSpalteX1 - _wiederHolungsSpaltenWidth;
        } else if (viewItem.OrderTmpSpalteX1 + viewItem.DrawWidth(r, _pix16, _cellFont) > r.Width) {
            SliderX.Value = SliderX.Value + viewItem.OrderTmpSpalteX1 ?? 0 + viewItem.DrawWidth(r, _pix16, _cellFont) - r.Width;
        }
        return true;
    }

    private void Filter_Changed(object sender, System.EventArgs e) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }

        foreach (var thisColumn in db.Column) {
            if (thisColumn != null) {
                thisColumn.TmpIfFilterRemoved = null;
            }
        }

        Invalidate_SortedRowData();
        OnFilterChanged();
    }

    private void Invalidate_DrawWidth(ColumnItem? column) {
        if (column == null || column.IsDisposed) { return; }
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }

        foreach (var thisArr in db.ColumnArrangements) {
            thisArr[column]?.Invalidate_DrawWidth();
        }
    }

    private void Invalidate_SortedRowData() {
        _rowsFilteredAndPinned = null;
        Invalidate();
    }

    private bool IsOnScreen(ColumnViewItem? viewItem, Rectangle displayRectangleWoSlider) {
        if (viewItem == null) { return false; }

        if (viewItem.OrderTmpSpalteX1 + viewItem.DrawWidth(displayRectangleWoSlider, _pix16, _cellFont) >= 0 && viewItem.OrderTmpSpalteX1 <= displayRectangleWoSlider.Width) { return true; }

        return false;
    }

    private bool IsOnScreen(ColumnViewCollection ca, ColumnItem? column, RowData? row, Rectangle displayRectangleWoSlider) => IsOnScreen(ca[column], displayRectangleWoSlider) && IsOnScreen(ca, row, displayRectangleWoSlider);

    private bool IsOnScreen(ColumnViewCollection ca, RowData? vrow, Rectangle displayRectangleWoSlider) => vrow != null && DrawY(ca, vrow) + vrow.DrawHeight >= ca.HeadSize(_columnFont) && DrawY(ca, vrow) <= displayRectangleWoSlider.Height;

    private void Neighbour(ColumnItem? column, RowData? row, Direction direction, out ColumnItem? newColumn, out RowData? newRow) {
        newColumn = column;
        newRow = row;

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

    //private void OnCellValueChanged(CellChangedEventArgs e) => CellValueChanged?.Invoke(this, e);

    private void OnDatabaseChanged() => DatabaseChanged?.Invoke(this, System.EventArgs.Empty);

    private void OnFilterChanged() => FilterChanged?.Invoke(this, System.EventArgs.Empty);

    private void OnNeedButtonArgs(ButtonCellEventArgs e) => NeedButtonArgs?.Invoke(this, e);

    private void OnPinnedChanged() => PinnedChanged?.Invoke(this, System.EventArgs.Empty);

    //private void OnRowAdded(object sender, RowChangedEventArgs e) => RowAdded?.Invoke(sender, e);

    private void OnSelectedCellChanged(CellExtEventArgs e) => SelectedCellChanged?.Invoke(this, e);

    private void OnSelectedRowChanged(RowEventArgs e) => SelectedRowChanged?.Invoke(this, e);

    private void OnViewChanged() {
        ViewChanged?.Invoke(this, System.EventArgs.Empty);
        Invalidate_SortedRowData(); // evtl. muss [Neue Zeile] ein/ausgebelndet werden
    }

    private void OnVisibleRowsChanged() => VisibleRowsChanged?.Invoke(this, System.EventArgs.Empty);

    /// <summary>
    /// Reset - Parse - CheckView
    /// </summary>
    /// <param name="toParse"></param>

    private void ParseView(string toParse) {
        ResetView();

        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }

        if (!string.IsNullOrEmpty(toParse)) {
            foreach (var pair in toParse.GetAllTags()) {
                switch (pair.Key) {
                    case "arrangement":
                        Arrangement = pair.Value.FromNonCritical();
                        break;

                    case "arrangementnr":
                        break;

                    case "filters":
                        Filter.Changed -= Filter_Changed;
                        Filter.RowsChanged -= Filter_Changed;
                        Filter.Database = Database;
                        var code = pair.Value.FromNonCritical();
                        Filter.Clear();
                        Filter.Parse(code);
                        Filter.ParseFinished(code);
                        Filter.Changed += Filter_Changed;
                        Filter.RowsChanged += Filter_Changed;
                        Filter_Changed(this, System.EventArgs.Empty);
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
                        CursorPos_Set(column, RowsFilteredAndPinned().Get(row), false);
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
                }
            }
        }

        CheckView();
    }

    private int Row_DrawHeight(ColumnViewCollection ca, RowItem row, Rectangle displayRectangleWoSlider) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return _pix18; }

        var tmp = _pix18;

        foreach (var thisViewItem in ca) {
            if (CellCollection.IsInCache(thisViewItem.Column, row) && thisViewItem.Column is ColumnItem tmpc && !row.CellIsNullOrEmpty(tmpc)) {
                tmp = Math.Max(tmp, CellItem.ContentSize(tmpc, row, _cellFont, _pix16).Height);
            }
        }

        tmp = Math.Min(tmp, (int)(displayRectangleWoSlider.Height * 0.9) - ca.HeadSize(_columnFont));
        tmp = Math.Max(tmp, _pix18);
        return tmp;
    }

    private void Row_RowRemoving(object sender, RowEventArgs e) {
        if (e.Row == CursorPosRow?.Row) { CursorPos_Reset(); }
        if (e.Row == _mouseOverRow?.Row) { _mouseOverRow = null; }
        if (PinnedRows.Contains(e.Row)) {
            _ = PinnedRows.Remove(e.Row);
            Invalidate_SortedRowData();
        }
    }

    private string RowCaptionOnCoordinate(int pixelX, int pixelY) {
        try {
            var s = RowsFilteredAndPinned();
            if (s == null) { return string.Empty; }
            foreach (var thisRow in s) {
                if (thisRow.ShowCap && thisRow.CaptionPos is var r) {
                    if (r.Contains(pixelX, pixelY)) { return thisRow.Chapter; }
                }
            }
        } catch { }
        return string.Empty;
    }

    private RowData? RowOnCoordinate(ColumnViewCollection ca, int pixelY) {
        if (IsDisposed || Database is not Database db || db.IsDisposed || pixelY <= ca.HeadSize(_columnFont)) { return null; }
        var s = RowsFilteredAndPinned();

        return s?.FirstOrDefault(thisRowItem => thisRowItem != null && pixelY >= DrawY(ca, thisRowItem) && pixelY <= DrawY(ca, thisRowItem) + thisRowItem.DrawHeight && thisRowItem.Expanded);
    }

    private void SliderSchaltenSenk(ColumnViewCollection ca, Rectangle displayR, int maxY) {
        SliderY.Minimum = 0;
        SliderY.Maximum = Math.Max(maxY - displayR.Height + 1 + ca.HeadSize(_columnFont), 0);
        SliderY.LargeChange = displayR.Height - ca.HeadSize(_columnFont);
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
        if (IsDisposed || textbox == null || Database is not Database db || db.IsDisposed) { return; }
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
        if (IsDisposed || Database is not Database db || db.IsDisposed || db.Column.Count == 0 || db.Column.First() is not ColumnItem fc) { return false; }
        if (db.ColumnArrangements.Count == 0) { return false; }
        if (CurrentArrangement?[fc] == null) { return false; }
        if (!db.Row.IsNewRowPossible()) { return false; }

        if (db.PowerEdit.Subtract(DateTime.UtcNow).TotalSeconds > 0) { return true; }

        if (!db.PermissionCheck(db.PermissionGroupsNewRow, null)) { return false; }

        return string.IsNullOrEmpty(EditableErrorReason(fc, null, EditableErrorReasonType.EditNormaly, true, true, false));
    }

    #endregion
}