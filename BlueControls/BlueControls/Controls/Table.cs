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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
public partial class Table : GenericControl, IContextMenu, IBackgroundNone, ITranslateable, IHasDatabase, IControlAcceptSomething, IControlSendSomething {

    #region Fields

    public static readonly int AutoFilterSize = 22;

    public static readonly SolidBrush BrushRedTransparent = new(Color.FromArgb(40, 255, 128, 128));

    public static readonly SolidBrush BrushYellowTransparent = new(Color.FromArgb(180, 255, 255, 0));

    public static readonly int ColumnCaptionSizeY = 22;

    public static readonly Pen PenRed1 = new(Color.Red, 1);

    public static readonly int RowCaptionSizeY = 50;

    public FilterCollection? Filter;

    private readonly List<string> _collapsed = new();

    private readonly object _lockUserAction = new();

    private int _arrangementNr = 1;

    private AutoFilter? _autoFilter;

    private BlueFont _cellFont = BlueFont.DefaultFont;

    private BlueFont _chapterFont = BlueFont.DefaultFont;

    private BlueFont _columnFilterFont = BlueFont.DefaultFont;

    private BlueFont _columnFont = BlueFont.DefaultFont;

    private DateTime? _databaseDrawError;

    private BlueTableAppearance _design = BlueTableAppearance.Standard;

    private bool _drawing;

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
    }

    #endregion

    #region Events

    public event EventHandler<FilterEventArgs>? AutoFilterClicked;

    public event EventHandler<CellEditBlockReasonEventArgs>? BlockEdit;

    public event EventHandler<CellEventArgs>? ButtonCellClicked;

    public event EventHandler<CellChangedEventArgs>? CellValueChanged;

    public event EventHandler<ContextMenuInitEventArgs>? ContextMenuInit;

    public event EventHandler<ContextMenuItemClickedEventArgs>? ContextMenuItemClicked;

    public event EventHandler? DatabaseChanged;

    public new event EventHandler<CellExtEventArgs>? DoubleClick;

    public event EventHandler? FilterChanged;

    public event EventHandler<ButtonCellEventArgs>? NeedButtonArgs;

    public event EventHandler? PinnedChanged;

    public event EventHandler<RowReasonEventArgs>? RowAdded;

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

    public List<IControlAcceptSomething> Childs { get; } = new();

    public ColumnViewCollection? CurrentArrangement {
        get {
            if (Database is not DatabaseAbstract db || db.IsDisposed || db.ColumnArrangements.Count <= _arrangementNr) {
                return null;
            }

            return db.ColumnArrangements[_arrangementNr];
        }
    }

    public ColumnItem? CursorPosColumn { get; private set; }

    public RowData? CursorPosRow { get; private set; }

    /// <summary>
    /// Datenbanken können mit DatabaseSet gesetzt werden.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public DatabaseAbstract? Database { get; private set; }

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
            Invalidate_SortedRowData();
            OnViewChanged();
        }
    }

    public bool DropMessages { get; set; }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FilterCollection? FilterInput { get; set; }

    public FilterCollection FilterOutput { get; } = new();

    [DefaultValue(1.0f)]
    public double FontScale => Database?.GlobalScale ?? 1f;

    public List<IControlSendSomething> Parents { get; } = new();

    public List<RowItem> PinnedRows { get; } = new();

    public DateTime PowerEdit {
        //private get => _database?.PowerEdit;
        set {
            if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }
            Database.PowerEdit = value;
            Invalidate_SortedRowData(); // Neue Zeilen können nun erlaubt sein
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

    public ReadOnlyCollection<RowItem> RowsFiltered {
        get {
            if (Database is not DatabaseAbstract db || db.IsDisposed) { return new List<RowItem>().AsReadOnly(); }
            if (Filter != null) { return Filter.Rows; }
            return db.Row.ToList().AsReadOnly();
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

    public long VisibleRowCount { get; private set; }

    #endregion

    #region Methods

    //    //        FilterOutput_Changed();
    //    //        OnFilterChanged();
    //    //    }
    //    //}
    //}
    public static int CalculateColumnContentWidth(Table table, ColumnItem column, Font cellFont, int pix16) {
        if (column.IsDisposed) { return 16; }
        if (column.Database is not DatabaseAbstract db || db.IsDisposed) { return 16; }
        if (column.FixedColumnWidth > 0) { return column.FixedColumnWidth; }
        if (column.ContentWidthIsValid) { return column.ContentWidth; }

        column.RefreshColumnsData();

        var newContentWidth = 16; // Wert muss gesetzt werden, dass er am ende auch gespeichert wird

        try {
            //if (column.Format == DataFormat.Button) {
            //    // Beim Button reicht eine Abfrage aus
            //    newContentWidth = Cell_ContentSize(table, column, null, cellFont, pix16).Width;
            //} else {
            //_ = Parallel.ForEach(column.Database.Row, thisRowItem => {
            //        var wx = Cell_ContentSize(table, column, thisRowItem, cellFont, pix16).Width;
            //        newContentWidth = Math.Max(newContentWidth, wx);

            //});
            //}

            //  Parallel.ForEach füjhrt ab und zu zu DeadLocks
            foreach (var thisRowItem in db.Row) {
                var wx = Cell_ContentSize(table, column, thisRowItem, cellFont, pix16).Width;
                newContentWidth = Math.Max(newContentWidth, wx);
            }
        } catch {
            Develop.CheckStackForOverflow();
            return CalculateColumnContentWidth(table, column, cellFont, pix16);
        }

        column.ContentWidth = newContentWidth;
        return newContentWidth;
    }

    //    //        if (_fc != null) {
    //    //            _fc.Changed += Filter_Changed;
    //    //        }
    public static Size Cell_ContentSize(Table? table, ColumnItem column, RowItem row, Font cellFont, int pix16) {
        if (column.Database is not DatabaseAbstract db || db.IsDisposed) { return new Size(pix16, pix16); }

        if (column.Format == DataFormat.Verknüpfung_zu_anderer_Datenbank) {
            var (lcolumn, lrow, _, _) = CellCollection.LinkedCellData(column, row, false, false);
            return lcolumn != null && lrow != null ? Cell_ContentSize(table, lcolumn, lrow, cellFont, pix16)
                                                   : new Size(pix16, pix16);
        }

        var contentSizex = db.Cell.GetSizeOfCellContent(column, row);
        if (contentSizex != null) { return (Size)contentSizex; }

        var contentSize = Size.Empty;

        if (column.Format == DataFormat.Button) {
            ButtonCellEventArgs e = new(column, row);
            table?.OnNeedButtonArgs(e);
            contentSize = Button.StandardSize(e.Text, e.Image);
        } else if (column.MultiLine) {
            var tmp = SplitMultiLine(db.Cell.GetString(column, row));
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
            var @string = db.Cell.GetString(column, row);
            contentSize = FormatedText_NeededSize(column, @string, cellFont, ShortenStyle.Replaced, pix16, column.BehaviorOfImageAndText);
        }
        contentSize.Width = Math.Max(contentSize.Width, pix16);
        contentSize.Height = Math.Max(contentSize.Height, pix16);
        db.Cell.SetSizeOfCellContent(column, row, contentSize);
        return contentSize;
    }

    //    //        _fc = value;
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

    //    //        if (_fc != null) {
    //    //            _fc.Changed -= Filter_Changed;
    //    //        }
    public static string Database_NeedPassword() => InputBox.Show("Bitte geben sie das Passwort ein,<br>um Zugriff auf diese Datenbank<br>zu erhalten:", string.Empty, FormatHolder.Text);

    //    //    if ((value == null && _fc != null) ||
    //    //        (_fc == null && value != null) ||
    //    //        (value!.ToString(true) != _fc!.ToString(true))) {
    //    //        Output
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
        row.Database?.Row.ExecuteValueChangedEvent(true);
    }

    //        return _filterUser;
    //    }
    //    //private set {
    //    //    if (value == null && _fc == null) { return; }
    public static void Draw_FormatedText(Graphics gr, string text, ColumnItem? column, Rectangle fitInRect, Design design, States state, ShortenStyle style, BildTextVerhalten bildTextverhalten) {
        if (string.IsNullOrEmpty(text)) { return; }
        var d = Skin.DesignOf(design, state);

        Draw_CellTransparentDirect(gr, text, fitInRect, d.BFont, column, 16, style, bildTextverhalten, state);
    }

    //public FilterCollection? UserFilter {
    //    get {
    //        if (Database is not DatabaseAbstract db || db.IsDisposed) { return null; }
    //        if (_filterUser == null) { UserFilter = new FilterCollection(Database); }
    /// <summary>
    /// Füllt die Liste rowsToExpand auf, bis sie 100 Einträge enthält.
    /// </summary>
    /// <param name="rowsToExpand"></param>
    /// <param name="sortedRows"></param>
    /// <returns>Gibt false zurück, wenn ALLE Zeilen dadurch geladen sind.</returns>
    public static bool FillUp100(List<RowItem> rowsToExpand, List<RowData> sortedRows) {
        if (rowsToExpand.Count is > 99 or 0) { return false; }

        if (rowsToExpand[0].IsDisposed) { return false; }
        if (rowsToExpand[0].Database is not DatabaseAbstract db) { return false; }
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

    /// <summary>
    /// Status des Bildes (Disabled) wird geändert. Diese Routine sollte nicht innerhalb der Table Klasse aufgerufen werden.
    /// Sie dient nur dazu, das Aussehen eines Textes wie eine Zelle zu imitieren.
    /// </summary>
    public static Size FormatedText_NeededSize(ColumnItem? column, string originalText, Font font, ShortenStyle style, int minSize, BildTextVerhalten bildTextverhalten) {
        var (s, qi) = CellItem.GetDrawingData(column, originalText, style, bildTextverhalten);
        return Skin.FormatedText_NeededSize(s, qi, font, minSize);
    }

    /// <summary>
    /// Erstellt eine neue Spalte mit den aus den Filterkriterien. Nur Filter IstGleich wird unterstützt.
    /// Schägt irgendetwas fehl, wird NULL zurückgegeben.
    /// Ist ein Filter mehrfach vorhanden, erhält die Zelle den LETZTEN Wert.
    /// Am Schluß wird noch das Skript ausgeführt.
    /// </summary>
    /// <param name="fi"></param>
    /// <param name="comment"></param>
    /// <returns></returns>
    public static RowItem? GenerateAndAdd(List<FilterItem> fi, string comment) {
        IReadOnlyCollection<string>? first = null;

        DatabaseAbstract? database = null;

        foreach (var thisfi in fi) {
            if (thisfi.FilterType is not FilterType.Istgleich and not FilterType.Istgleich_GroßKleinEgal) { return null; }
            if (thisfi.Column == null) { return null; }
            if (thisfi.Database is not DatabaseAbstract db || db.IsDisposed) { return null; }
            database ??= db;

            if (db.Column.First() == thisfi.Column) {
                if (first != null) { return null; }
                first = thisfi.SearchValue;
            }

            if (thisfi.Column.Database != database) { return null; }
        }

        if (database == null || database.IsDisposed) { return null; }

        if (!database.Row.IsNewRowPossible()) { return null; }

        if (first == null) { return null; }

        var s = database.NextRowKey();
        if (s == null || string.IsNullOrEmpty(s)) { return null; }

        var row = database.Row.GenerateAndAdd(s, first.JoinWithCr(), false, true, comment);

        if (row == null || row.IsDisposed) { return null; }

        foreach (var thisfi in fi) {
            if (thisfi.Column is ColumnItem c) {
                row.CellSet(c, thisfi.SearchValue.ToList());
            }
        }

        _ = row.ExecuteScript(ScriptEventTypes.new_row, string.Empty, false, false, true, 1);

        return row;
    }

    //    FileDialogs.DeleteFile(delList, true);
    //    return newFiles;
    //}
    public static void ImportCsv(DatabaseAbstract database, string csvtxt) {
        using Import x = new(database, csvtxt);
        _ = x.ShowDialog();
        x.Dispose();
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

        if (database is null || columnArrangementSelector == null) {
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

    public List<RowData> CalculateSortedRows(IEnumerable<RowItem> filteredRows, RowSortDefinition? rowSortDefinition, List<RowItem>? pinnedRows, List<RowData>? reUseMe) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return new List<RowData>(); }

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

        var l = new List<ColumnItem>();
        if (rowSortDefinition != null) { l.AddRange(rowSortDefinition.Columns); }
        if (db.Column.SysChapter != null) { _ = l.AddIfNotExists(db.Column.SysChapter); }

        db.RefreshColumnsData(l);

        #region _Angepinnten Zeilen erstellen (_pinnedData)

        List<RowData> pinnedData = new();
        var lockMe = new object();
        if (pinnedRows != null) {
            _ = Parallel.ForEach(pinnedRows, thisRow => {
                var rd = reUseMe.Get(thisRow, "Angepinnt") ?? new RowData(thisRow, "Angepinnt");
                rd.PinStateSortAddition = "1";
                rd.MarkYellow = true;
                rd.AdditionalSort = rowSortDefinition == null ? thisRow.CompareKey(null) : thisRow.CompareKey(rowSortDefinition.Columns);

                lock (lockMe) {
                    VisibleRowCount++;
                    pinnedData.Add(rd);
                }
            });
        }

        #endregion

        #region Gefiltere Zeilen erstellen (_rowData)

        List<RowData> rowData = new();
        _ = Parallel.ForEach(filteredRows, thisRow => {
            var adk = rowSortDefinition == null ? thisRow.CompareKey(null) : thisRow.CompareKey(rowSortDefinition.Columns);

            var markYellow = pinnedRows != null && pinnedRows.Contains(thisRow);
            var added = markYellow;

            List<string> caps;
            if (db.Column.SysChapter is ColumnItem sc) {
                caps = thisRow.CellGetList(sc);
            } else {
                caps = new();
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
                var rd = reUseMe.Get(thisRow, thisCap) ?? new RowData(thisRow, thisCap);

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

        if (rowSortDefinition != null && rowSortDefinition.Reverse) { rowData.Reverse(); }

        rowData.InsertRange(0, pinnedData);
        return rowData;
    }

    public void CheckView() {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }

        if (_arrangementNr != 1) {
            if (db.ColumnArrangements == null || _arrangementNr >= db.ColumnArrangements.Count || CurrentArrangement == null || !db.PermissionCheck(CurrentArrangement.PermissionGroups_Show, null)) {
                _arrangementNr = 1;
            }
        }

        if (_mouseOverColumn != null && _mouseOverColumn.Database != db) { _mouseOverColumn = null; }
        if (_mouseOverRow?.Row != null && _mouseOverRow?.Row.Database != db) { _mouseOverRow = null; }
        if (CursorPosColumn != null && CursorPosColumn.Database != db) { CursorPosColumn = null; }
        if (CursorPosRow?.Row != null && CursorPosRow?.Row.Database != db) { CursorPosRow = null; }
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
        if (Database is not DatabaseAbstract db || row == null || column == null ||
            db.ColumnArrangements.Count == 0 || CurrentArrangement?[column] == null ||
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

        if (ensureVisible) {
            var ca = CurrentArrangement;
            if (ca != null) {
                _ = EnsureVisible(ca, CursorPosColumn, CursorPosRow);
            }
        }
        Invalidate();

        OnSelectedCellChanged(new CellExtEventArgs(CursorPosColumn, CursorPosRow));

        if (!sameRow) {
            OnSelectedRowChanged(new RowEventArgs(setedrow));
            FilterOutput.Database = Database;
            FilterOutput.Add(new FilterItem(setedrow));
        }
    }

    public void DatabaseSet(DatabaseAbstract? db, string viewCode) {
        if (Database == db && string.IsNullOrEmpty(viewCode)) { return; }

        CloseAllComponents();

        if (Database is DatabaseAbstract db1 && !db1.IsDisposed) {
            // auch Disposed Datenbanken die Bezüge entfernen!
            db1.Cell.CellValueChanged -= _Database_CellValueChanged;
            db1.Loaded -= _Database_DatabaseLoaded;
            db1.Loading -= _Database_StoreView;
            db1.ViewChanged -= _Database_ViewChanged;
            //db.RowKeyChanged -= _Database_RowKeyChanged;
            //db.ColumnKeyChanged -= _Database_ColumnKeyChanged;
            db1.Column.ColumnInternalChanged -= _Database_ColumnContentChanged;
            db1.SortParameterChanged -= _Database_SortParameterChanged;
            db1.Row.RowRemoving -= Row_RowRemoving;
            db1.Row.RowRemoved -= _Database_RowRemoved;
            db1.Row.RowAdded -= _Database_Row_RowAdded;
            db1.Row.RowGotData -= _Database_Row_RowGotData;
            db1.Column.ColumnRemoving -= Column_ItemRemoving;
            db1.Column.ColumnRemoved -= _Database_ViewChanged;
            db1.Column.ColumnAdded -= _Database_ViewChanged;
            db1.ProgressbarInfo -= _Database_ProgressbarInfo;
            db1.DisposingEvent -= _Database_Disposing;
            db1.InvalidateView -= Database_InvalidateView;
            if (Filter != null) { Filter.Changed -= Filter_Changed; }
            //db.IsTableVisibleForUser -= Database_IsTableVisibleForUser;
            DatabaseAbstract.ForceSaveAll();
            MultiUserFile.ForceLoadSaveAll();
            //db.Save(false);         // Datenbank nicht reseten, weil sie ja anderweitig noch benutzt werden kann
        }
        ShowWaitScreen = true;
        Refresh(); // um die Uhr anzuzeigen
        Database = db;
        _databaseDrawError = null;
        InitializeSkin(); // Neue Schriftgrößen
        if (Database is DatabaseAbstract db2 && !db2.IsDisposed) {
            db2.Cell.CellValueChanged += _Database_CellValueChanged;
            db2.Loaded += _Database_DatabaseLoaded;
            db2.Loading += _Database_StoreView;
            db2.ViewChanged += _Database_ViewChanged;
            //db2.RowKeyChanged += _Database_RowKeyChanged;
            //db2.ColumnKeyChanged += _Database_ColumnKeyChanged;
            db2.Column.ColumnInternalChanged += _Database_ColumnContentChanged;
            db2.SortParameterChanged += _Database_SortParameterChanged;
            db2.Row.RowRemoving += Row_RowRemoving;
            db2.Row.RowRemoved += _Database_RowRemoved;
            db2.Row.RowAdded += _Database_Row_RowAdded;
            db2.Row.RowGotData += _Database_Row_RowGotData;
            db2.Column.ColumnAdded += _Database_ViewChanged;
            db2.Column.ColumnRemoving += Column_ItemRemoving;
            db2.Column.ColumnRemoved += _Database_ViewChanged;
            db2.ProgressbarInfo += _Database_ProgressbarInfo;
            db2.DisposingEvent += _Database_Disposing;
            db2.InvalidateView += Database_InvalidateView;
            //db2.IsTableVisibleForUser += Database_IsTableVisibleForUser;
            Filter = new FilterCollection(db2);
            Filter.Changed += Filter_Changed;
        }

        ParseView(viewCode);
        //ResetView();
        //CheckView();

        ShowWaitScreen = false;
        OnDatabaseChanged();
    }

    public void Draw_Column_Head(Graphics gr, ColumnViewItem viewItem, Rectangle displayRectangleWoSlider, int lfdNo, ColumnViewCollection ca) {
        if (!IsOnScreen(viewItem, displayRectangleWoSlider)) { return; }
        if (_design == BlueTableAppearance.OnlyMainColumnWithoutHead) { return; }
        if (_columnFont == null) { return; }

        if (viewItem.Column != null) {
            if (viewItem.OrderTmpSpalteX1 != null) {
                gr.FillRectangle(new SolidBrush(viewItem.Column.BackColor), (int)viewItem.OrderTmpSpalteX1, 0, Column_DrawWidth(viewItem, displayRectangleWoSlider), HeadSize(ca));
                Draw_Border(gr, viewItem, displayRectangleWoSlider, true, ca);
                gr.FillRectangle(new SolidBrush(Color.FromArgb(100, 200, 200, 200)), (int)viewItem.OrderTmpSpalteX1, 0, Column_DrawWidth(viewItem, displayRectangleWoSlider), HeadSize(ca));

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
                viewItem.TmpAutoFilterLocation = new Rectangle((int)viewItem.OrderTmpSpalteX1 + Column_DrawWidth(viewItem, displayRectangleWoSlider) - AutoFilterSize, HeadSize(ca) - AutoFilterSize, AutoFilterSize, AutoFilterSize);
                FilterItem? fi = null;
                if (Filter != null) {
                    fi = Filter[viewItem.Column];
                }

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
                            BlueFont.DrawString(gr, "#" + lfdNo, (Font)_columnFont, Brushes.Black, (int)viewItem.OrderTmpSpalteX1 + x, viewItem.TmpAutoFilterLocation.Top + y);
                        }
                    }

                    BlueFont.DrawString(gr, "#" + lfdNo, (Font)_columnFont, Brushes.White, (int)viewItem.OrderTmpSpalteX1, viewItem.TmpAutoFilterLocation.Top);
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
                        (int)((Column_DrawWidth(viewItem, displayRectangleWoSlider) - fs.Width) / 2.0), 3 + down);
                    gr.DrawImageInRectAspectRatio(viewItem.Column.TmpCaptionBitmapCode, (int)viewItem.OrderTmpSpalteX1 + 2, (int)(pos.Y + fs.Height), Column_DrawWidth(viewItem, displayRectangleWoSlider) - 4, HeadSize(ca) - (int)(pos.Y + fs.Height) - 6 - 18);
                    // Dann der Text
                    gr.TranslateTransform(pos.X, pos.Y);
                    //GR.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    BlueFont.DrawString(gr, tx, (Font)_columnFont, new SolidBrush(viewItem.Column.ForeColor), 0, 0);
                    gr.TranslateTransform(-pos.X, -pos.Y);

                    #endregion
                } else {

                    #region Spalte ohne Bild zeichnen

                    Point pos = new(
                        (int)viewItem.OrderTmpSpalteX1 +
                        (int)((Column_DrawWidth(viewItem, displayRectangleWoSlider) - fs.Height) / 2.0),
                        HeadSize(ca) - 4 - AutoFilterSize);
                    gr.TranslateTransform(pos.X, pos.Y);
                    gr.RotateTransform(-90);
                    //GR.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    BlueFont.DrawString(gr, tx, (Font)_columnFont, new SolidBrush(viewItem.Column.ForeColor), 0, 0);
                    gr.TranslateTransform(-pos.X, -pos.Y);
                    gr.ResetTransform();

                    #endregion
                }

                #region Sortierrichtung Zeichnen

                var tmpSortDefinition = SortUsed();
                if (tmpSortDefinition != null && (tmpSortDefinition.UsedForRowSort(viewItem.Column) || viewItem.Column == Database?.Column.SysChapter)) {
                    if (viewItem.OrderTmpSpalteX1 != null) {
                        if (tmpSortDefinition.Reverse) {
                            gr.DrawImage(QuickImage.Get("ZA|11|5||||50"), (float)(viewItem.OrderTmpSpalteX1 + (Column_DrawWidth(viewItem, displayRectangleWoSlider) / 2.0) - 6), HeadSize(ca) - 6 - AutoFilterSize);
                        } else {
                            gr.DrawImage(QuickImage.Get("AZ|11|5||||50"), (float)(viewItem.OrderTmpSpalteX1 + (Column_DrawWidth(viewItem, displayRectangleWoSlider) / 2.0) - 6), HeadSize(ca) - 6 - AutoFilterSize);
                        }
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
            if (Design == BlueTableAppearance.OnlyMainColumnWithoutHead) {
                return "In dieser Ansicht kann der Eintrag nicht bearbeitet werden.";
            }

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
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return string.Empty; }
        List<ColumnItem> l = new() { onlyColumn };
        return Database.Export_CSV(firstRow, l, RowsVisibleUnique());
    }

    public void Export_HTML(string filename = "", bool execute = true) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }
        if (string.IsNullOrEmpty(filename)) { filename = TempFile(string.Empty, string.Empty, "html"); }
        Database.Export_HTML(filename, CurrentArrangement, RowsVisibleUnique(), execute);
    }

    public void FilterInput_Changed(object sender, System.EventArgs e) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }
        if (Filter == null) { return; }

        FilterInput = this.FilterOfSender();

        var t = "Übergeordnetes Element";

        if (FilterInput != null) {
            foreach (var thisFi in FilterInput) {
                thisFi.Herkunft = t;
            }
        }

        Filter.RemoveRange(t);

        Filter.RemoveOtherAndAddIfNotExists(FilterInput);

        //_filterall = new FilterCollection(Database);
        //_filterall.AddIfNotExists(Filter);

        //if (_filterFromParents != null) { _filterall.AddIfNotExists(_filterFromParents); }

        //_filterall = new FilterCollection(Database);
        //_filterall.AddIfNotExists(Filter);

        //if (_filterFromParents != null) { _filterall.AddIfNotExists(_filterFromParents); }

        //Invalidate_SortedRowData();
        //OnFilterChanged();
    }

    public void FilterInput_Changing(object sender, System.EventArgs e) { }

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

        var ca = CurrentArrangement;
        if (ca == null) { return; }

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
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }
        ImportCsv(Database, csvtxt);
    }

    public void Invalidate_AllColumnArrangements() {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }

        foreach (var thisArrangement in db.ColumnArrangements) {
            thisArrangement?.Invalidate_DrawWithOfAllItems();
        }
    }

    public void Invalidate_HeadSize() {
        if (_headSize != null) { Invalidate(); }
        _headSize = null;
    }

    public bool Mouse_IsInHead() => MousePos().Y <= HeadSize(CurrentArrangement);

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

    public void Pin(List<RowItem>? rows) {
        // Arbeitet mit Rows, weil nur eine Anpinngug möglich ist

        rows ??= new List<RowItem>();

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
        Invalidate_SortedRowData();
        OnViewChanged();
        Invalidate();
    }

    public List<RowData>? RowsFilteredAndPinned() {
        var ca = CurrentArrangement;
        if (ca == null) { return null; }

        if (_rowsFilteredAndPinned != null) { return _rowsFilteredAndPinned; }

        try {
            var displayR = DisplayRectangleWithoutSlider();
            var maxY = 0;
            if (UserEdit_NewRowAllowed()) { maxY += _pix18; }
            var expanded = true;
            var lastCap = string.Empty;

            List<RowData> sortedRowDataNew;
            if (Database is not DatabaseAbstract db || db.IsDisposed) {
                sortedRowDataNew = new List<RowData>();
            } else {
                sortedRowDataNew = CalculateSortedRows(RowsFiltered, SortUsed(), PinnedRows, _rowsFilteredAndPinned);
            }

            if (_rowsFilteredAndPinned != null && !_rowsFilteredAndPinned.IsDifferentTo(sortedRowDataNew)) { return _rowsFilteredAndPinned; }

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
            //markiert ist. Dann wird sortiren angestoßen, und der Cursor zurück gesprungen

            OnVisibleRowsChanged();

            return RowsFilteredAndPinned(); // Rekursiver aufruf. Manchmal funktiniert OnRowsSorted nicht...
        } catch {
            // Komisch, manchmal wird die Variable _sortedRowDatax verworfen.
            Develop.CheckStackForOverflow();
            Invalidate_SortedRowData();
            return RowsFilteredAndPinned();
        }
    }

    public List<RowItem> RowsVisibleUnique() {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return new List<RowItem>(); }

        var f = RowsFiltered;

        ConcurrentBag<RowItem> l = new();

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
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return null; }
        var s = CurrentArrangement;

        return s == null || s.Count == 0 ? null : s[0]?.Column;
    }

    public RowData? View_NextRow(RowData? row) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return null; }
        if (row == null || row.IsDisposed) { return null; }

        if (RowsFilteredAndPinned() is not List<RowData> sr) { return null; }

        var rowNr = sr.IndexOf(row);
        return rowNr < 0 || rowNr >= sr.Count - 1 ? null : sr[rowNr + 1];
    }

    public RowData? View_PreviousRow(RowData? row) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return null; }
        if (row == null || row.IsDisposed) { return null; }
        if (RowsFilteredAndPinned() is not List<RowData> sr) { return null; }
        var rowNr = sr.IndexOf(row);
        return rowNr < 1 ? null : sr[rowNr - 1];
    }

    public RowData? View_RowFirst() {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return null; }
        if (RowsFilteredAndPinned() is not List<RowData> sr) { return null; }
        return sr.Count == 0 ? null : sr[0];
    }

    public RowData? View_RowLast() {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return null; }
        if (RowsFilteredAndPinned() is not List<RowData> sr) { return null; }
        return sr.Count == 0 ? null : sr[sr.Count - 1];
    }

    public string ViewToString() {
        List<string> result = new();
        result.ParseableAdd("ArrangementNr", _arrangementNr);
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
    [DebuggerNonUserCode()]
    protected override void Dispose(bool disposing) {
        try {
            if (disposing) {
                DatabaseSet(null, string.Empty); // Wichtig (nicht _Database) um Events zu lösen
                FilterInput?.Dispose();
                FilterOutput.Dispose();
                FilterInput = null;
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

        if (Database is not DatabaseAbstract db || DesignMode || ShowWaitScreen || _drawing) {
            DrawWaitScreen(gr);
            return;
        }

        var ca = CurrentArrangement;
        if (ca == null) {
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
                //List<RowItem> sortedRows = new();

                foreach (var thisRow in sortedRowData) {
                    if (thisRow?.Row is RowItem r) {
                        if (IsOnScreen(ca, thisRow, displayRectangleWoSlider)) {
                            if (r.IsInCache == null) { _ = rowsToRefreh.AddIfNotExists(r); }

                            var T = sortedRowData.IndexOf(thisRow);
                            firstVisibleRow = Math.Min(T, firstVisibleRow);
                            lastVisibleRow = Math.Max(T, lastVisibleRow);
                        }

                        //if (sortedRows.Count < 300) {
                        //    // 300 reichen, sonst dauerts so lange
                        //    _ = sortedRows.AddIfNotExists(r);
                        //}
                    }
                }

                _ = FillUp100(rowsToRefreh, RowsFilteredAndPinned());

                (bool didreload, string errormessage) = Database.RefreshRowData(rowsToRefreh, false);

                if (!string.IsNullOrEmpty(errormessage)) {
                    FormWithStatusBar.UpdateStatusBar(FehlerArt.Warnung, errormessage, true);
                }
                if (!didreload || count > 15) { break; }

                Invalidate_SortedRowData();
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
        _columnFilterFont = BlueFont.Get(_columnFont.FontName, _columnFont.Size, false, false, false, false, true, Color.White, Color.Red, false, false, false);

        _newRowFont = Skin.GetBlueFont(Enums.Design.Table_Cell_New, States.Standard).Scale(FontScale);
        if (Database != null && !Database.IsDisposed) {
            _pix16 = GetPix(16, (Font)_cellFont, Database.GlobalScale);
            _pix18 = GetPix(18, (Font)_cellFont, Database.GlobalScale);
            _rowCaptionFontY = GetPix(26, (Font)_cellFont, Database.GlobalScale);
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
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }
        var ca = CurrentArrangement;
        if (ca == null) { return; }

        lock (_lockUserAction) {
            if (_isinClick) { return; }
            _isinClick = true;

            CellOnCoordinate(ca, MousePos().X, MousePos().Y, out _mouseOverColumn, out _mouseOverRow);
            _isinClick = false;
        }
    }

    protected override void OnDoubleClick(System.EventArgs e) {
        //    base.OnDoubleClick(e); Wird komplett selbst gehandlet und das neue Ereigniss ausgelöst
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }

        var ca = CurrentArrangement;
        if (ca == null) { return; }

        lock (_lockUserAction) {
            if (_isinDoubleClick) { return; }
            _isinDoubleClick = true;
            CellOnCoordinate(ca, MousePos().X, MousePos().Y, out _mouseOverColumn, out _mouseOverRow);
            CellExtEventArgs ea = new(_mouseOverColumn, _mouseOverRow);
            DoubleClick?.Invoke(this, ea);

            if (!Mouse_IsInHead()) {
                if (_mouseOverRow != null || MousePos().Y <= HeadSize(ca) + _pix18) {
                    DoubleClick?.Invoke(this, ea);
                    Cell_Edit(ca, _mouseOverColumn, _mouseOverRow, true);
                }
            }
            _isinDoubleClick = false;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e) {
        base.OnKeyDown(e);

        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }
        if (CursorPosColumn is null) { return; }

        var ca = CurrentArrangement;
        if (ca == null) { return; }

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
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }

        var ca = CurrentArrangement;
        if (ca == null) { return; }

        lock (_lockUserAction) {
            if (_isinMouseDown) { return; }
            _isinMouseDown = true;
            //_database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
            CellOnCoordinate(ca, e.X, e.Y, out _mouseOverColumn, out _mouseOverRow);
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
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }
        lock (_lockUserAction) {
            if (_isinMouseEnter) { return; }
            _isinMouseEnter = true;
            Forms.QuickInfo.Close();
            _isinMouseEnter = false;
        }
    }

    protected override void OnMouseLeave(System.EventArgs e) {
        base.OnMouseLeave(e);
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }
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
            var ca = CurrentArrangement;
            if (ca == null) { return; }
            if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }

            if (_isinMouseMove) { return; }
            _isinMouseMove = true;
            _mouseOverText = string.Empty;
            CellOnCoordinate(ca, e.X, e.Y, out _mouseOverColumn, out _mouseOverRow);
            if (e.Button != MouseButtons.None) {
                //_database?.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
            } else {
                if (_mouseOverColumn != null && e.Y < HeadSize(ca)) {
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

        lock (_lockUserAction) {
            var ca = CurrentArrangement;
            if (Database is not DatabaseAbstract db || db.IsDisposed || ca == null) {
                Forms.QuickInfo.Close();
                return;
            }
            CellOnCoordinate(ca, e.X, e.Y, out _mouseOverColumn, out _mouseOverRow);
            if (CursorPosColumn != _mouseOverColumn || CursorPosRow != _mouseOverRow) { Forms.QuickInfo.Close(); }
            // TXTBox_Close() NICHT! Weil sonst nach dem öffnen sofort wieder gschlossen wird
            // AutoFilter_Close() NICHT! Weil sonst nach dem öffnen sofort wieder geschlossen wird
            FloatingForm.Close(this, Enums.Design.Form_KontextMenu);

            var viewItem = ca[_mouseOverColumn];

            if (e.Button == MouseButtons.Left) {
                if (_mouseOverColumn != null) {
                    if (Mouse_IsInAutofilter(viewItem, e)) {
                        var screenX = Cursor.Position.X - e.X;
                        var screenY = Cursor.Position.Y - e.Y;
                        AutoFilter_Show(ca, viewItem, screenX, screenY);
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
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }

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
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }
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
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }
        lock (_lockUserAction) {
            if (_isinVisibleChanged) { return; }
            _isinVisibleChanged = true;
            //Database?.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
            _isinVisibleChanged = false;
        }
    }

    private static void Draw_CellTransparentDirect(Graphics gr, string toDraw, Rectangle drawarea, BlueFont font, ColumnItem? contentHolderCellColumn, int pix16, ShortenStyle style, BildTextVerhalten bildTextverhalten, States state) {
        if (contentHolderCellColumn == null) { return; }

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
                    y += FormatedText_NeededSize(contentHolderCellColumn, mei[z], (Font)font, style, pix16 - 1, bildTextverhalten).Height;
                }
            }
        }
    }

    private static void Draw_CellTransparentDirect_OneLine(Graphics gr, string drawString, ColumnItem contentHolderColumnStyle, Rectangle drawarea, int txtYPix, bool changeToDot, BlueFont font, int pix16, ShortenStyle style, BildTextVerhalten bildTextverhalten, States state) {
        Rectangle r = new(drawarea.Left, drawarea.Top + txtYPix, drawarea.Width, pix16);

        if (r.Bottom + pix16 > drawarea.Bottom) {
            if (r.Bottom > drawarea.Bottom) { return; }
            if (changeToDot) { drawString = "..."; }// Die letzte Zeile noch ganz hinschreiben
        }

        var (text, qi) = CellItem.GetDrawingData(contentHolderColumnStyle, drawString, style, bildTextverhalten);
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
    private static bool FillUp(List<RowItem> rowsToExpand, RowItem rowToCheck, List<RowData> sortedRows, int expandCount) {
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

    private static int GetPix(int pix, Font f, double scale) => Skin.FormatedText_NeededSize("@|", null, f, (int)((pix * scale) + 0.5)).Height;

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
        var er = table.EditableErrorReason(cellInThisDatabaseColumn, cellInThisDatabaseRow, EditableErrorReasonType.EditCurrently, true, false, true);
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
        var f = Filter;
        var rsd = SortUsed();

        if ((f != null && f[e.Column] != null) ||
            rsd == null ||
           rsd.UsedForRowSort(e.Column) ||
          (f != null && f.MayHasRowFilter(e.Column)) ||
            e.Column == Database?.Column.SysChapter) {
            f.Invalidate_FilteredRows();
            Invalidate_SortedRowData();
        }

        if (CurrentArrangement is ColumnViewCollection cvc) {
            if (cvc[e.Column] != null) {
                if (e.Column.MultiLine ||
                    e.Column.BehaviorOfImageAndText == BildTextVerhalten.Nur_Bild ||
                    e.Column.BehaviorOfImageAndText == BildTextVerhalten.Wenn_möglich_Bild_und_immer_Text) {
                    Invalidate_SortedRowData(); // Zeichenhöhe kann sich ändern...
                }
            }
        }

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

    private void _Database_Row_RowAdded(object sender, RowReasonEventArgs e) {
        OnRowAdded(sender, e);
        Filter?.Invalidate_FilteredRows();
        Invalidate_SortedRowData();
    }

    private void _Database_Row_RowGotData(object sender, RowEventArgs e) => Invalidate_SortedRowData();

    private void _Database_RowRemoved(object sender, System.EventArgs e) {
        Filter?.Invalidate_FilteredRows();
        Invalidate_SortedRowData();
    }

    private void _Database_SortParameterChanged(object sender, System.EventArgs e) => Invalidate_SortedRowData();

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

    private void AutoFilter_Show(ColumnViewCollection ca, ColumnViewItem columnviewitem, int screenx, int screeny) {
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
        _autoFilter.Position_LocateToPosition(new Point(screenx + (int)columnviewitem.OrderTmpSpalteX1, screeny + HeadSize(ca)));
        _autoFilter.Show();
        _autoFilter.FilterComand += AutoFilter_FilterComand;
        Develop.Debugprint_BackgroundThread();
    }

    private int Autofilter_Text(ColumnItem column) {
        if (Database is not DatabaseAbstract db || db.IsDisposed || Filter == null) { return 0; }
        if (column.TmpIfFilterRemoved != null) { return (int)column.TmpIfFilterRemoved; }
        var fc = (FilterCollection)Filter.Clone();
        fc.Remove(column);

        var ro = fc.Rows;
        var c = RowsFiltered.Count - ro.Count;
        column.TmpIfFilterRemoved = c;
        return c;
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

    private void Cell_Edit(ColumnViewCollection ca, ColumnItem? cellInThisDatabaseColumn, RowData? cellInThisDatabaseRow, bool preverDropDown) {
        var f = EditableErrorReason(cellInThisDatabaseColumn, cellInThisDatabaseRow, EditableErrorReasonType.EditCurrently, true, true, true);
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

    private void Cell_Edit_Dropdown(ColumnViewCollection ca, ColumnItem? cellInThisDatabaseColumn, RowData? cellInThisDatabaseRow, ColumnItem contentHolderCellColumn, RowItem? contentHolderCellRow) {
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
            if (contentHolderCellColumn.TextBearbeitungErlaubt) { Cell_Edit(ca, cellInThisDatabaseColumn, cellInThisDatabaseRow, false); } else {
                NotEditableInfo("Keine Items zum Auswählen vorhanden.");
            }
            return;
        }

        if (contentHolderCellColumn.TextBearbeitungErlaubt) {
            if (t.Count == 0 && cellInThisDatabaseRow.Row.CellIsNullOrEmpty(cellInThisDatabaseColumn)) {
                // Bei nur einem Wert, wenn Texteingabe erlaubt, Dropdown öffnen
                Cell_Edit(ca, cellInThisDatabaseColumn, cellInThisDatabaseRow, false);
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

    private bool Cell_Edit_TextBox(ColumnViewCollection ca, ColumnItem? cellInThisDatabaseColumn, RowData? cellInThisDatabaseRow, ColumnItem contentHolderCellColumn, RowItem? contentHolderCellRow, TextBox box, int addWith, int isHeight) {
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

        var viewItemx = ca[cellInThisDatabaseColumn];

        if (contentHolderCellRow != null) {
            var h = cellInThisDatabaseRow.DrawHeight;// Row_DrawHeight(cellInThisDatabaseRow, DisplayRectangle);
            if (isHeight > 0) { h = isHeight; }
            box.Location = new Point((int)viewItemx.OrderTmpSpalteX1, DrawY(ca, cellInThisDatabaseRow));
            box.Size = new Size(Column_DrawWidth(viewItemx, DisplayRectangle) + addWith, h);
            box.Text = contentHolderCellRow.CellGetString(contentHolderCellColumn);
        } else {
            // Neue Zeile...
            box.Location = new Point((int)viewItemx.OrderTmpSpalteX1, HeadSize(ca));
            box.Size = new Size(Column_DrawWidth(viewItemx, DisplayRectangle) + addWith, _pix18);
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
        column = ColumnOnCoordinate(ca, xpos);
        row = RowOnCoordinate(ca, ypos);
    }

    private void CloseAllComponents() {
        if (InvokeRequired) {
            _ = Invoke(new Action(CloseAllComponents));
            return;
        }
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }
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
                ? Math.Min(CalculateColumnContentWidth(this, viewItem.Column, (Font)_cellFont, _pix16), (int)(displayRectangleWoSlider.Width * 0.3))
                : Math.Min(CalculateColumnContentWidth(this, viewItem.Column, (Font)_cellFont, _pix16), (int)(displayRectangleWoSlider.Width * 0.6));
        }

        viewItem.TmpDrawWidth = Math.Max((int)viewItem.TmpDrawWidth, AutoFilterSize); // Mindestens so groß wie der Autofilter;
        viewItem.TmpDrawWidth = Math.Max((int)viewItem.TmpDrawWidth, (int)ColumnHead_Size(viewItem.Column).Width);
        return (int)viewItem.TmpDrawWidth;
    }

    private void Column_ItemRemoving(object sender, ColumnReasonEventArgs e) {
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
        column.TmpCaptionTextSize = BlueFont.MeasureString(column.Caption.Replace("\r", "\r\n"), (Font)_columnFont);
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

    private ColumnItem? ColumnOnCoordinate(ColumnViewCollection ca, int xpos) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return null; }

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
            //if (Database?.ColumnArrangements == null || _arrangementNr >= Database.ColumnArrangements.Count) { return false; }

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
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }
        Neighbour(CursorPosColumn, CursorPosRow, richtung, out var newCol, out var newRow);
        CursorPos_Set(newCol, newRow, richtung != Direction.Nichts);
    }

    private void Database_InvalidateView(object sender, System.EventArgs e) => Invalidate();

    //private void Database_IsTableVisibleForUser(object sender, VisibleEventArgs e) => e.IsVisible = true;

    private Rectangle DisplayRectangleWithoutSlider() => _design == BlueTableAppearance.Standard
            ? new Rectangle(DisplayRectangle.Left, DisplayRectangle.Left, DisplayRectangle.Width - SliderY.Width, DisplayRectangle.Height - SliderX.Height)
            : new Rectangle(DisplayRectangle.Left, DisplayRectangle.Left, DisplayRectangle.Width - SliderY.Width, DisplayRectangle.Height);

    private void Draw_Border(Graphics gr, ColumnViewItem column, Rectangle displayRectangleWoSlider, bool onlyhead, ColumnViewCollection ca) {
        var yPos = displayRectangleWoSlider.Height;
        if (onlyhead) { yPos = HeadSize(ca); }
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
    private void Draw_CellTransparent(Graphics gr, ColumnViewItem? cellInThisDatabaseColumn, RowData? cellInThisDatabaseRow, Rectangle cellrectangle, BlueFont font, States state) {
        if (cellInThisDatabaseRow?.Row == null) { return; }
        if (cellInThisDatabaseColumn?.Column == null) { return; }

        if (cellInThisDatabaseColumn.Column.Format == DataFormat.Verknüpfung_zu_anderer_Datenbank) {
            var (lcolumn, lrow, _, _) = CellCollection.LinkedCellData(cellInThisDatabaseColumn.Column, cellInThisDatabaseRow.Row, false, false);

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
    /// <param name="cellrectangle"></param>
    /// <param name="contentHolderCellColumn"></param>
    /// <param name="contentHolderCellRow"></param>
    /// <param name="font"></param>
    private void Draw_CellTransparentDirect(Graphics gr, ColumnViewItem? cellInThisDatabaseColumn, RowData? cellInThisDatabaseRow, Rectangle cellrectangle, ColumnItem? contentHolderCellColumn, RowItem? contentHolderCellRow, BlueFont font, States state) {
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

    private void Draw_Column_Body(Graphics gr, ColumnViewItem cellInThisDatabaseColumn, Rectangle displayRectangleWoSlider, ColumnViewCollection ca) {
        gr.SmoothingMode = SmoothingMode.None;
        gr.FillRectangle(new SolidBrush(cellInThisDatabaseColumn.Column.BackColor), (int)cellInThisDatabaseColumn.OrderTmpSpalteX1, HeadSize(ca), Column_DrawWidth(cellInThisDatabaseColumn, displayRectangleWoSlider), displayRectangleWoSlider.Height);
        Draw_Border(gr, cellInThisDatabaseColumn, displayRectangleWoSlider, false, ca);
    }

    private void Draw_Column_Cells(Graphics gr, IReadOnlyList<RowData> sr, ColumnViewItem viewItem, Rectangle displayRectangleWoSlider, int firstVisibleRow, int lastVisibleRow, States state, bool firstOnScreen, ColumnViewCollection ca) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }

        if (Database.Column.First() is ColumnItem columnFirst && viewItem.Column == columnFirst && UserEdit_NewRowAllowed()) {
            Skin.Draw_FormatedText(gr, "[Neue Zeile]", QuickImage.Get(ImageCode.PlusZeichen, _pix16), Alignment.Left, new Rectangle((int)viewItem.OrderTmpSpalteX1 + 1, (int)(-SliderY.Value + HeadSize(ca) + 1), (int)viewItem.TmpDrawWidth - 2, 16 - 2), this, false, _newRowFont, Translate);
        }

        for (var zei = firstVisibleRow; zei <= lastVisibleRow; zei++) {
            var currentRow = sr[zei];
            gr.SmoothingMode = SmoothingMode.None;

            Rectangle cellrectangle = new((int)viewItem.OrderTmpSpalteX1, DrawY(ca, currentRow),
                Column_DrawWidth(viewItem, displayRectangleWoSlider), Math.Max(currentRow.DrawHeight, _pix16));

            if (currentRow.Expanded) {
                if (currentRow.MarkYellow) { gr.FillRectangle(BrushYellowTransparent, cellrectangle); }

                if (db.IsAdministrator()) {
                    if (currentRow.Row.NeedsUpdate()) {
                        gr.FillRectangle(BrushRedTransparent, cellrectangle);
                        db.Row.AddRowWithChangedValue(currentRow.Row);
                    }
                }

                gr.DrawLine(Skin.PenLinieDünn, cellrectangle.Left, cellrectangle.Bottom - 1, cellrectangle.Right - 1, cellrectangle.Bottom - 1);

                if (!Thread.CurrentThread.IsBackground && CursorPosColumn == viewItem.Column && CursorPosRow == currentRow) {
                    _tmpCursorRect = cellrectangle;
                    _tmpCursorRect.Height -= 1;
                    Draw_Cursor(gr, displayRectangleWoSlider, false);
                }

                Draw_CellTransparent(gr, viewItem, currentRow, cellrectangle, _cellFont, state);

                if (_unterschiede != null && _unterschiede != currentRow.Row) {
                    if (currentRow.Row.CellGetString(viewItem.Column) != _unterschiede.CellGetString(viewItem.Column)) {
                        Rectangle tmpr = new((int)viewItem.OrderTmpSpalteX1 + 1, DrawY(ca, currentRow) + 1,
                            Column_DrawWidth(viewItem, displayRectangleWoSlider) - 2, currentRow.DrawHeight - 2);
                        gr.DrawRectangle(PenRed1, tmpr);
                    }
                }
            }

            if (firstOnScreen) {
                // Überschrift in der ersten Spalte zeichnen
                currentRow.CaptionPos = Rectangle.Empty;
                if (currentRow.ShowCap) {
                    var si = gr.MeasureString(currentRow.Chapter, (Font)_chapterFont);
                    gr.FillRectangle(new SolidBrush(Skin.Color_Back(Enums.Design.Table_And_Pad, States.Standard).SetAlpha(50)), 1, DrawY(ca, currentRow) - RowCaptionSizeY, displayRectangleWoSlider.Width - 2, RowCaptionSizeY);
                    currentRow.CaptionPos = new Rectangle(1, DrawY(ca, currentRow) - _rowCaptionFontY, (int)si.Width + 28, (int)si.Height);

                    if (_collapsed.Contains(currentRow.Chapter)) {
                        var x = new ExtText(Enums.Design.Button_CheckBox, States.Checked);
                        Button.DrawButton(this, gr, Enums.Design.Button_CheckBox, States.Checked, null, Alignment.Horizontal_Vertical_Center, false, x, string.Empty, currentRow.CaptionPos, false);
                        gr.DrawImage(QuickImage.Get("Pfeil_Unten_Scrollbar|14|||FF0000||200|200"), 5, DrawY(ca, currentRow) - _rowCaptionFontY + 6);
                    } else {
                        var x = new ExtText(Enums.Design.Button_CheckBox, States.Standard);
                        Button.DrawButton(this, gr, Enums.Design.Button_CheckBox, States.Standard, null, Alignment.Horizontal_Vertical_Center, false, x, string.Empty, currentRow.CaptionPos, false);
                        gr.DrawImage(QuickImage.Get("Pfeil_Rechts_Scrollbar|14|||||0"), 5, DrawY(ca, currentRow) - _rowCaptionFontY + 6);
                    }
                    _chapterFont.DrawString(gr, currentRow.Chapter, 23, DrawY(ca, currentRow) - _rowCaptionFontY);
                    gr.DrawLine(Skin.PenLinieDick, 0, DrawY(ca, currentRow), displayRectangleWoSlider.Width, DrawY(ca, currentRow));
                }
            }
        }
    }

    private void Draw_Column_Head_Captions(Graphics gr, ColumnViewCollection ca) {
        var bvi = new ColumnViewItem?[3];
        var lcbvi = new ColumnViewItem[3];
        ColumnViewItem? lastViewItem = null;
        var permaX = 0;

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

    private void Draw_Table_ListboxStyle(Graphics gr, IReadOnlyList<RowData>? sr, States state, Rectangle displayRectangleWoSlider, int firstVisibleRow, int lastVisibleRow) {
        if (Database is not DatabaseAbstract db || db.IsDisposed || db.ColumnArrangements.Count == 0) {
            DrawWaitScreen(gr);
            return;
        }

        var ca = db.ColumnArrangements[0];
        var col = db.Column.First();
        var viewItem = ca[col];

        var itStat = state;
        Skin.Draw_Back(gr, Enums.Design.ListBox, state, base.DisplayRectangle, this, true);

        if (sr != null && col != null && viewItem != null) {
            // Zeilen Zeichnen (Alle Zellen)
            for (var zeiv = firstVisibleRow; zeiv <= lastVisibleRow; zeiv++) {
                var currentRow = sr[zeiv];

                Rectangle r = new(0, DrawY(ca, currentRow), DisplayRectangleWithoutSlider().Width, currentRow.DrawHeight);
                if (CursorPosColumn != null && CursorPosRow?.Row == currentRow.Row) {
                    itStat |= States.Checked;
                } else {
                    if (itStat.HasFlag(States.Checked)) {
                        itStat ^= States.Checked;
                    }
                }

                Rectangle cellrectangle = new(0, DrawY(ca, currentRow), displayRectangleWoSlider.Width,
                    Math.Min(currentRow.DrawHeight, 24));

                Draw_CellListBox(gr, viewItem, currentRow, cellrectangle, r, Enums.Design.Item_Listbox, itStat);
                if (Database.Column.SysCorrect is ColumnItem sc && !currentRow.Row.CellGetBoolean(sc)) {
                    gr.DrawImage(QuickImage.Get("Warnung|16||||||120||50"), new Point(r.Right - 19, (int)(r.Top + ((r.Height - 16) / 2.0))));
                }
                if (currentRow.ShowCap) {
                    BlueFont.DrawString(gr, currentRow.Chapter, (Font)_chapterFont, _chapterFont.BrushColorMain, 0, DrawY(ca, currentRow) - _rowCaptionFontY);
                }
            }
        }
        Skin.Draw_Border(gr, Enums.Design.ListBox, state, displayRectangleWoSlider);
    }

    private void Draw_Table_Std(Graphics gr, List<RowData> sr, States state, Rectangle displayRectangleWoSlider, int firstVisibleRow, int lastVisibleRow, ColumnViewCollection? ca) {
        try {
            if (Database is not DatabaseAbstract db || db.IsDisposed || ca == null) { return; }   // Kommt vor, dass spontan doch geparsed wird...
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
            Draw_Column_Head_Captions(gr, ca);
            Skin.Draw_Border(gr, Enums.Design.Table_And_Pad, state, displayRectangleWoSlider);

            ////gr.Clear(Color.White);
            //gr.DrawString("Test " + Constants.GlobalRND.Next().ToString(false), new Font("Arial", 20), Brushes.Black, new Point(0, 0));

            //return;

            if (db.HasPendingChanges) { gr.DrawImage(QuickImage.Get(ImageCode.Stift, 16), 16, 8); }
            if (db.ReadOnly) {
                gr.DrawImage(QuickImage.Get(ImageCode.Schloss, 32), 16, 8);
                if (!string.IsNullOrEmpty(db.FreezedReason)) {
                    BlueFont.DrawString(gr, db.FreezedReason, (Font)_columnFont, Brushes.Blue, 52, 12);
                }
            }
            if (db.AmITemporaryMaster()) { gr.DrawImage(QuickImage.Get(ImageCode.Stern, 8), 0, 0); }
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

        Skin.Draw_Back(gr, Enums.Design.Table_And_Pad, States.Standard_Disabled, base.DisplayRectangle, this, true);

        var i = QuickImage.Get(ImageCode.Uhr, 64);
        gr.DrawImage(i, (Width - 64) / 2, (Height - 64) / 2);
        Skin.Draw_Border(gr, Enums.Design.Table_And_Pad, States.Standard_Disabled, base.DisplayRectangle);
    }

    private int DrawY(ColumnViewCollection ca, RowData? r) => r == null ? 0 : r.Y + HeadSize(ca) - (int)SliderY.Value;

    /// <summary>
    /// Berechent die Y-Position auf dem aktuellen Controll
    /// </summary>
    /// <param name="r"></param>
    /// <returns></returns>
    private void DropDownMenu_ItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
        FloatingForm.Close(this);
        if (string.IsNullOrEmpty(e.ClickedComand)) { return; }

        var ca = CurrentArrangement;
        if (ca == null) { return; }

        CellExtEventArgs? ck = null;
        if (e.HotItem is CellExtEventArgs tmp) { ck = tmp; }

        if (ck?.Column == null) { return; }

        var toAdd = e.ClickedComand;
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

        if (ck.RowData.Row == null) { return; }

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
        if (DrawY(ca, rowdata) < HeadSize(ca)) {
            SliderY.Value = SliderY.Value + DrawY(ca, rowdata) - HeadSize(ca);
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
            if (viewItem.OrderTmpSpalteX1 + Column_DrawWidth(viewItem, r) <= r.Width) { return true; }
            //Develop.DebugPrint(enFehlerArt.Info,"Unsichtbare Wiederholungsspalte: " + ViewItem.Column.KeyName);
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
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }

        foreach (var thisColumn in db.Column) {
            if (thisColumn != null) {
                thisColumn.TmpIfFilterRemoved = null;
                thisColumn.TmpAutoFilterSinnvoll = null;
            }
        }

        Invalidate_SortedRowData();
        OnFilterChanged();
    }

    private int HeadSize(ColumnViewCollection? ca) {
        if (ca == null) { return 0; }

        if (_headSize != null) { return (int)_headSize; }

        if (_design == BlueTableAppearance.OnlyMainColumnWithoutHead || ca.Count - 1 < 0) {
            _headSize = 0;
            return 0;
        }
        _headSize = 16;
        foreach (var thisViewItem in ca) {
            if (thisViewItem?.Column != null) {
                _headSize = Math.Max((int)_headSize, (int)ColumnHead_Size(thisViewItem.Column).Height);
            }
        }

        _headSize += 8;
        _headSize += AutoFilterSize;
        return (int)_headSize;
    }

    private void Invalidate_DrawWidth(ColumnItem? column) {
        if (column == null || column.IsDisposed) { return; }
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }

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
        if (_design is BlueTableAppearance.Standard or BlueTableAppearance.OnlyMainColumnWithoutHead) {
            if (viewItem.OrderTmpSpalteX1 + Column_DrawWidth(viewItem, displayRectangleWoSlider) >= 0 && viewItem.OrderTmpSpalteX1 <= displayRectangleWoSlider.Width) { return true; }
        } else {
            Develop.DebugPrint(_design);
        }
        return false;
    }

    private bool IsOnScreen(ColumnViewCollection ca, ColumnItem? column, RowData? row, Rectangle displayRectangleWoSlider) => IsOnScreen(ca[column], displayRectangleWoSlider) && IsOnScreen(ca, row, displayRectangleWoSlider);

    private bool IsOnScreen(ColumnViewCollection ca, RowData? vrow, Rectangle displayRectangleWoSlider) => vrow != null && DrawY(ca, vrow) + vrow.DrawHeight >= HeadSize(ca) && DrawY(ca, vrow) <= displayRectangleWoSlider.Height;

    private void Neighbour(ColumnItem? column, RowData? row, Direction direction, out ColumnItem? newColumn, out RowData? newRow) {
        newColumn = column;
        newRow = row;
        if (_design == BlueTableAppearance.OnlyMainColumnWithoutHead) {
            if (direction is not Direction.Oben and not Direction.Unten) {
                newColumn = Database?.Column.First();
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

    private void OnCellValueChanged(CellChangedEventArgs e) => CellValueChanged?.Invoke(this, e);

    private void OnDatabaseChanged() => DatabaseChanged?.Invoke(this, System.EventArgs.Empty);

    private void OnFilterChanged() => FilterChanged?.Invoke(this, System.EventArgs.Empty);

    private void OnNeedButtonArgs(ButtonCellEventArgs e) => NeedButtonArgs?.Invoke(this, e);

    private void OnPinnedChanged() => PinnedChanged?.Invoke(this, System.EventArgs.Empty);

    private void OnRowAdded(object sender, RowReasonEventArgs e) => RowAdded?.Invoke(sender, e);

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
        if (Database is not DatabaseAbstract db || db.IsDisposed || row == null) { return _pix18; }

        if (_design == BlueTableAppearance.OnlyMainColumnWithoutHead) { return Cell_ContentSize(this, db.Column.First(), row, (Font)_cellFont, _pix16).Height; }
        var tmp = _pix18;
        //if (CurrentArrangement == null) { return tmp; }

        //db.RefreshRowData(row, false);

        foreach (var thisViewItem in ca) {
            if (db.Cell.IsInCache(thisViewItem?.Column, row) && !row.CellIsNullOrEmpty(thisViewItem.Column)) {
                tmp = Math.Max(tmp, Cell_ContentSize(this, thisViewItem.Column, row, (Font)_cellFont, _pix16).Height);
            }
        }

        tmp = Math.Min(tmp, (int)(displayRectangleWoSlider.Height * 0.9) - HeadSize(ca));
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
        if (Database is not DatabaseAbstract db || db.IsDisposed || pixelY <= HeadSize(ca)) { return null; }
        var s = RowsFilteredAndPinned();
        if (s == null) { return null; }

        return s.FirstOrDefault(thisRowItem => thisRowItem != null && pixelY >= DrawY(ca, thisRowItem) && pixelY <= DrawY(ca, thisRowItem) + thisRowItem.DrawHeight && thisRowItem.Expanded);
    }

    private void SliderSchaltenSenk(ColumnViewCollection ca, Rectangle displayR, int maxY) {
        SliderY.Minimum = 0;
        SliderY.Maximum = Math.Max(maxY - displayR.Height + 1 + HeadSize(ca), 0);
        SliderY.LargeChange = displayR.Height - HeadSize(ca);
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
        if (textbox == null || Database is not DatabaseAbstract db || db.IsDisposed) { return; }
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
        if (Database is not DatabaseAbstract db || db.IsDisposed || db.Column.Count == 0 || db.Column.First() is not ColumnItem fc) { return false; }
        if (_design == BlueTableAppearance.OnlyMainColumnWithoutHead) { return false; }
        if (db.ColumnArrangements.Count == 0) { return false; }
        if (CurrentArrangement?[fc] == null) { return false; }
        if (!db.Row.IsNewRowPossible()) { return false; }

        if (db.PowerEdit.Subtract(DateTime.UtcNow).TotalSeconds > 0) { return true; }

        if (!db.PermissionCheck(db.PermissionGroupsNewRow, null)) { return false; }

        return string.IsNullOrEmpty(EditableErrorReason(fc, null, EditableErrorReasonType.EditNormaly, true, true, false));
    }

    #endregion
}