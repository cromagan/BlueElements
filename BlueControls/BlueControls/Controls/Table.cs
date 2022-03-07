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
using static BlueBasics.Converter;
using static BlueBasics.FileOperations;

#nullable enable

namespace BlueControls.Controls {

    [Designer(typeof(BasicDesigner))]
    [DefaultEvent("CursorPosChanged")]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class Table : GenericControl, IContextMenu, IBackgroundNone, ITranslateable {

        #region Fields

        public static SolidBrush BrushYellowTransparent = new(Color.FromArgb(180, 255, 255, 0));

        public static Pen PenRed1 = new(Color.Red, 1);

        private const int AutoFilterSize = 22;

        private const int ColumnCaptionSizeY = 22;

        private const int RowCaptionSizeY = 50;
        private static bool _serviceStarted;
        private readonly List<string> _collapsed = new();

        // Die Sortierung der Zeile
        private readonly object _lockUserAction = new();

        private int _arrangementNr = 1;
        private AutoFilter _autoFilter;
        private BlueFont? _cellFont;
        private BlueFont? _chapterFont;
        private BlueFont? _columnFilterFont;
        private BlueFont? _columnFont;
        private ColumnItem? _cursorPosColumn;
        private RowData? _cursorPosRow;
        private Database? _database;
        private enBlueTableAppearance _design = enBlueTableAppearance.Standard;

        // Die Sortierung der Zeile
        private List<RowItem>? _filteredRows;

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

        //private bool ISIN_Resize;
        private bool _isinVisibleChanged;

        /// <summary>
        ///  Wird DatabaseAdded gehandlet?
        /// </summary>
        private ColumnItem? _mouseOverColumn;

        private RowData? _mouseOverRow;
        private string _mouseOverText;
        private BlueFont? _newRowFont;
        private Progressbar? _pg;

        // Die Sortierung der Zeile
        private int _pix16 = 16;

        private int _pix18 = 18;
        private int _rowCaptionFontY = 26;
        private SearchAndReplace _searchAndReplace;

        //private readonly FontSelectDialog _FDia = null;
        private bool _showNumber;

        private RowSortDefinition _sortDefinitionTemporary;
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
            _MouseHighlight = false;
        }

        #endregion

        #region Events

        public event EventHandler<FilterEventArgs> AutoFilterClicked;

        public event EventHandler<CellEventArgs> ButtonCellClicked;

        public event EventHandler<CellEventArgs> CellValueChanged;

        public event EventHandler<CellValueChangingByUserEventArgs> CellValueChangingByUser;

        public event EventHandler ColumnArrangementChanged;

        public event EventHandler<ContextMenuInitEventArgs> ContextMenuInit;

        public event EventHandler<ContextMenuItemClickedEventArgs> ContextMenuItemClicked;

        public event EventHandler<CellExtEventArgs> CursorPosChanged;

        public event EventHandler DatabaseChanged;

        public new event EventHandler<CellDoubleClickEventArgs> DoubleClick;

        public event EventHandler<CellCancelEventArgs> EditBeforeBeginEdit;

        public event EventHandler<RowCancelEventArgs> EditBeforeNewRow;

        public event EventHandler FilterChanged;

        public event EventHandler<ButtonCellEventArgs> NeedButtonArgs;

        public event EventHandler PinnedChanged;

        public event EventHandler<RowEventArgs> RowAdded;

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

        public ColumnViewCollection? CurrentArrangement => _database?.ColumnArrangements == null || _database.ColumnArrangements.Count <= _arrangementNr
                    ? null
                    : _database.ColumnArrangements[_arrangementNr];

        // <Obsolete("Database darf nicht im Designer gesetzt werden.", True)>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Database? Database {
            get => _database;
            set {
                if (_database == value) { return; }
                CloseAllComponents();
                _collapsed.Clear();
                PinnedRows.Clear();
                _mouseOverColumn = null;
                _mouseOverRow = null;
                _cursorPosColumn = null;
                _cursorPosRow = null;
                _unterschiede = null;
                _mouseOverText = string.Empty;
                if (_database != null) {
                    // auch Disposed Datenbanken die Bezüge entfernen!
                    _database.Cell.CellValueChanged -= _Database_CellValueChanged;
                    _database.ConnectedControlsStopAllWorking -= _Database_StopAllWorking;
                    _database.Loaded -= _Database_DatabaseLoaded;
                    _database.Loading -= _Database_StoreView;
                    _database.ViewChanged -= _Database_ViewChanged;
                    //_Database.RowKeyChanged -= _Database_RowKeyChanged;
                    //_Database.ColumnKeyChanged -= _Database_ColumnKeyChanged;
                    _database.Column.ItemInternalChanged -= _Database_ColumnContentChanged;
                    _database.SortParameterChanged -= _Database_SortParameterChanged;
                    _database.Row.RowRemoving -= Row_RowRemoving;
                    _database.Row.RowRemoved -= _Database_RowRemoved;
                    _database.Row.RowAdded -= _Database_Row_RowAdded;
                    _database.Column.ItemRemoved -= _Database_ViewChanged;
                    _database.Column.ItemAdded -= _Database_ViewChanged;
                    _database.SavedToDisk -= _Database_SavedToDisk;
                    _database.ColumnArrangements.ItemInternalChanged -= ColumnArrangements_ItemInternalChanged;
                    _database.ProgressbarInfo -= _Database_ProgressbarInfo;
                    _database.DropMessage -= _Database_DropMessage;
                    _database.Disposing -= _Database_Disposing;
                    _database.Save(false);         // Datenbank nicht reseten, weil sie ja anderweitig noch benutzt werden kann
                }
                ShowWaitScreen = true;
                Refresh(); // um die Uhr anzuzeigen
                _database = value;
                InitializeSkin(); // Neue Schriftgrößen
                if (_database != null) {
                    _database.Cell.CellValueChanged += _Database_CellValueChanged;
                    _database.ConnectedControlsStopAllWorking += _Database_StopAllWorking;
                    _database.Loaded += _Database_DatabaseLoaded;
                    _database.Loading += _Database_StoreView;
                    _database.ViewChanged += _Database_ViewChanged;
                    //_Database.RowKeyChanged += _Database_RowKeyChanged;
                    //_Database.ColumnKeyChanged += _Database_ColumnKeyChanged;
                    _database.Column.ItemInternalChanged += _Database_ColumnContentChanged;
                    _database.SortParameterChanged += _Database_SortParameterChanged;
                    _database.Row.RowRemoving += Row_RowRemoving;
                    _database.Row.RowRemoved += _Database_RowRemoved;
                    _database.Row.RowAdded += _Database_Row_RowAdded;
                    _database.Column.ItemAdded += _Database_ViewChanged;
                    _database.Column.ItemRemoving += Column_ItemRemoving;
                    _database.Column.ItemRemoved += _Database_ViewChanged;
                    _database.SavedToDisk += _Database_SavedToDisk;
                    _database.ColumnArrangements.ItemInternalChanged += ColumnArrangements_ItemInternalChanged;
                    _database.ProgressbarInfo += _Database_ProgressbarInfo;
                    _database.DropMessage += _Database_DropMessage;
                    _database.Disposing += _Database_Disposing;
                }
                _Database_DatabaseLoaded(this, new LoadedEventArgs(false));
                ShowWaitScreen = false;
                Invalidate();
                OnDatabaseChanged();
            }
        }

        [DefaultValue(enBlueTableAppearance.Standard)]
        public enBlueTableAppearance Design {
            get => _design;
            set {
                SliderY.Visible = true;
                SliderX.Visible = Convert.ToBoolean(value == enBlueTableAppearance.Standard);
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

        public FilterCollection? Filter { get; private set; }

        [DefaultValue(1.0f)]
        public double FontScale => _database == null ? 1f : _database.GlobalScale;

        public List<RowItem?> PinnedRows { get; } = new();

        public DateTime PowerEdit {
            private get => _database.PowerEdit;
            set {
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
                } else {
                    return t1 + t2; // Eins davon ist leer
                }
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

        [DefaultValue(true)]
        public bool Translate { get; set; } = true;

        //  <Obsolete("Database darf nicht im Designer gesetzt werden.", True)>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RowItem Unterschiede {
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

        public static Size Cell_ContentSize(Table? table, ColumnItem? column, RowItem? row, BlueFont? cellFont, int pix16) {
            if (column.Format is enDataFormat.Verknüpfung_zu_anderer_Datenbank_Skriptgesteuert or enDataFormat.Verknüpfung_zu_anderer_Datenbank) {
                var (lcolumn, lrow, _) = CellCollection.LinkedCellData(column, row, false, false);
                return lcolumn != null && lrow != null ? Cell_ContentSize(table, lcolumn, lrow, cellFont, pix16) : new Size(pix16, pix16);
            }
            var contentSize = column.Database.Cell.GetSizeOfCellContent(column, row);
            if (contentSize.Width > 0 && contentSize.Height > 0) { return contentSize; }
            if (column.Format == enDataFormat.Button) {
                if (table is null) {
                    contentSize = new Size(pix16, pix16);
                } else {
                    ButtonCellEventArgs e = new(column, row);
                    table.OnNeedButtonArgs(e);
                    contentSize = Button.StandardSize(e.Text, e.Image);
                }
            } else if (column.MultiLine) {
                var tmp = column.Database.Cell.GetList(column, row);
                if (column.ShowMultiLineInOneLine) {
                    contentSize = FormatedText_NeededSize(column, tmp.JoinWith("; "), cellFont, enShortenStyle.Replaced, pix16, column.BildTextVerhalten);
                } else {
                    foreach (var thisString in tmp) {
                        var tmpSize = FormatedText_NeededSize(column, thisString, cellFont, enShortenStyle.Replaced, pix16, column.BildTextVerhalten);
                        contentSize.Width = Math.Max(tmpSize.Width, contentSize.Width);
                        contentSize.Height += Math.Max(tmpSize.Height, pix16);
                    }
                }
            } else {
                var @string = column.Database.Cell.GetString(column, row);
                contentSize = FormatedText_NeededSize(column, @string, cellFont, enShortenStyle.Replaced, pix16, column.BildTextVerhalten);
            }
            contentSize.Width = Math.Max(contentSize.Width, pix16);
            contentSize.Height = Math.Max(contentSize.Height, pix16);
            if (Skin.Scale == 1 && LanguageTool.Translation == null) { column.Database.Cell.SetSizeOfCellContent(column, row, contentSize); }
            return contentSize;
        }

        public static void CopyToClipboard(ColumnItem? column, RowItem? row, bool meldung) {
            try {
                if (row != null && column.Format.CanBeCheckedByRules()) {
                    var c = row.CellGetString(column);
                    c = c.Replace("\r\n", "\r");
                    c = c.Replace("\r", "\r\n");
                    Generic.CopytoClipboard(c);
                    if (meldung) { Notification.Show(LanguageTool.DoTranslate("<b>{0}</b><br>ist nun in der Zwischenablage.", true, c), enImageCode.Kopieren); }
                } else {
                    if (meldung) { Notification.Show(LanguageTool.DoTranslate("Bei dieser Spalte nicht möglich."), enImageCode.Warnung); }
                }
            } catch {
                if (meldung) { Notification.Show(LanguageTool.DoTranslate("Unerwarteter Fehler beim Kopieren."), enImageCode.Warnung); }
            }
        }

        public static void Database_NeedPassword(object sender, PasswordEventArgs e) {
            if (e.Handled) { return; }
            e.Handled = true;
            e.Password = InputBox.Show("Bitte geben sie das Passwort ein,<br>um Zugriff auf diese Datenbank<br>zu erhalten:", string.Empty, enVarType.Text);
        }

        public static void DoUndo(ColumnItem? column, RowItem? row) {
            if (column == null) { return; }
            if (row == null) { return; }
            if (column.Format is enDataFormat.Verknüpfung_zu_anderer_Datenbank_Skriptgesteuert or enDataFormat.Verknüpfung_zu_anderer_Datenbank) {
                var (lcolumn, lrow, _) = CellCollection.LinkedCellData(column, row, true, false);
                if (lcolumn != null && lrow != null) { DoUndo(lcolumn, lrow); }
                return;
            }
            var cellKey = CellCollection.KeyOfCell(column, row);
            var i = UndoItems(column.Database, cellKey);
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

        /// <summary>
        /// Status des Bildes (Disabled) wird geändert. Diese Routine sollte nicht innerhalb der Table Klasse aufgerufen werden.
        /// Sie dient nur dazu, das Aussehen eines Textes wie eine Zelle zu imitieren.
        /// </summary>
        public static void Draw_FormatedText(Graphics gr, string text, ColumnItem? column, Rectangle fitInRect, enDesign design, enStates state, enShortenStyle style, enBildTextVerhalten bildTextverhalten) {
            if (string.IsNullOrEmpty(text)) { return; }
            var d = Skin.DesignOf(design, state);

            Draw_CellTransparentDirect(gr, text, fitInRect, d.bFont, column, 16, style, bildTextverhalten, state);
        }

        public static List<string?>? FileSystem(ColumnItem? tmpColumn) {
            if (tmpColumn == null) { return null; }

            var f = GetFilesWithFileSelector(tmpColumn.Database.Filename.FilePath(), tmpColumn.MultiLine);

            if (f == null) { return null; }

            List<string> delList = new();
            List<string> newFiles = new();

            foreach (var thisf in f) {
                var b = FileToByte(thisf);

                if (!string.IsNullOrEmpty(tmpColumn.Database.FileEncryptionKey)) { b = Cryptography.SimpleCrypt(b, tmpColumn.Database.FileEncryptionKey, 1); }

                var neu = thisf.FileNameWithSuffix();
                neu = tmpColumn.BestFile(neu.FileNameWithSuffix(), true);
                ByteToFile(neu, b);

                newFiles.Add(neu);
                delList.Add(thisf);
            }

            FileDialogs.DeleteFile(delList, true);
            return newFiles;
        }

        public static Size FormatedText_NeededSize(ColumnItem? column, string originalText, BlueFont? font, enShortenStyle style, int minSize, enBildTextVerhalten bildTextverhalten) {
            var (s, quickImage) = CellItem.GetDrawingData(column, originalText, style, bildTextverhalten);
            return Skin.FormatedText_NeededSize(s, quickImage, font, minSize);
        }

        public static void ImportCsv(Database database, string csvtxt) {
            using Import x = new(database, csvtxt);
            x.ShowDialog();
        }

        public static void SearchNextText(string searchTxt, Table tableView, ColumnItem? column, RowData? row, out ColumnItem? foundColumn, out RowData? foundRow, bool vereinfachteSuche) {
            searchTxt = searchTxt.Trim();
            var ca = tableView.CurrentArrangement;
            if (tableView.Design == enBlueTableAppearance.OnlyMainColumnWithoutHead) {
                ca = tableView.Database.ColumnArrangements[0];
            }
            if (row == null) { row = tableView.View_RowLast(); }
            if (column == null) { column = tableView.Database.Column.SysLocked; }
            var rowsChecked = 0;
            if (string.IsNullOrEmpty(searchTxt)) {
                MessageBox.Show("Bitte Text zum Suchen eingeben.", enImageCode.Information, "OK");
                foundColumn = null;
                foundRow = null;
                return;
            }
            do {
                column = tableView.Design == enBlueTableAppearance.OnlyMainColumnWithoutHead ? column.Next() : ca.NextVisible(column);
                if (column == null) {
                    column = ca[0].Column;
                    if (rowsChecked > tableView.Database.Row.Count() + 1) {
                        foundColumn = null;
                        foundRow = null;
                        return;
                    }
                    rowsChecked++;
                    row = tableView.View_NextRow(row);
                    if (row == null) { row = tableView.View_RowFirst(); }
                }
                var contentHolderCellColumn = column;
                var contenHolderCellRow = row?.Row;
                if (column.Format is enDataFormat.Verknüpfung_zu_anderer_Datenbank_Skriptgesteuert or enDataFormat.Verknüpfung_zu_anderer_Datenbank) {
                    (contentHolderCellColumn, contenHolderCellRow, _) = CellCollection.LinkedCellData(column, row?.Row, false, false);
                }
                var ist1 = string.Empty;
                var ist2 = string.Empty;
                if (contenHolderCellRow != null && contentHolderCellColumn != null) {
                    ist1 = contenHolderCellRow.CellGetString(contentHolderCellColumn);
                    ist2 = CellItem.ValuesReadable(contentHolderCellColumn, contenHolderCellRow, enShortenStyle.Both).JoinWithCr();
                }
                if (contentHolderCellColumn != null && contentHolderCellColumn.FormatierungErlaubt) {
                    ExtText l = new(enDesign.TextBox, enStates.Standard) {
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

        public static int TmpColumnContentWidth(Table? table, ColumnItem? column, BlueFont? cellFont, int pix16) {
            if (column.TmpColumnContentWidth != null) { return (int)column.TmpColumnContentWidth; }
            column.TmpColumnContentWidth = 0;
            if (column.Format == enDataFormat.Button) {
                // Beim Button reicht eine Abfrage mit Row null
                column.TmpColumnContentWidth = Cell_ContentSize(table, column, null, cellFont, pix16).Width;
            } else {
                var locker = new object();

                Parallel.ForEach(column.Database.Row, thisRowItem => {
                    if (thisRowItem != null && !thisRowItem.CellIsNullOrEmpty(column)) {
                        var wx = Cell_ContentSize(table, column, thisRowItem, cellFont, pix16).Width;

                        lock (locker) {
                            var t = column.TmpColumnContentWidth; // ja, dank Multithreading kann es sein, dass hier das hier null ist
                            if (t == null) { t = 0; }
                            column.TmpColumnContentWidth = Math.Max((int)t, wx);
                        }
                    }
                });

                //foreach (var ThisRowItem in Column.Database.Row) {
                //    if (ThisRowItem != null && !ThisRowItem.CellIsNullOrEmpty(Column)) {
                //        var t = Column.TMP_ColumnContentWidth; // ja, dank Multithreading kann es sein, dass hier das hier null ist
                //        if (t == null) { t = 0; }
                //        Column.TMP_ColumnContentWidth = Math.Max((int)t, Cell_ContentSize(table, Column, ThisRowItem, CellFont, Pix16).Width);
                //    }
                //}
            }
            return column.TmpColumnContentWidth is int w ? w : 0;
        }

        public static ItemCollectionList UndoItems(Database database, string cellkey) {
            ItemCollectionList i = new(enBlueListBoxAppearance.KontextMenu) {
                CheckBehavior = enCheckBehavior.AlwaysSingleSelection
            };
            if (database.Works == null || database.Works.Count == 0) { return i; }
            var isfirst = true;
            TextListItem? las = null;
            var lasNr = -1;
            var co = 0;
            for (var z = database.Works.Count - 1; z >= 0; z--) {
                if (database.Works[z].CellKey == cellkey && database.Works[z].HistorischRelevant) {
                    co++;
                    lasNr = z;
                    las = isfirst
                        ? new TextListItem("Aktueller Text - ab " + database.Works[z].Date + " UTC, geändert von " + database.Works[z].User, "Cancel", null, false, true, string.Empty)
                        : new TextListItem("ab " + database.Works[z].Date + " UTC, geändert von " + database.Works[z].User, co.ToString(Constants.Format_Integer5) + database.Works[z].ChangedTo, null, false, true, string.Empty);
                    isfirst = false;
                    if (las != null) { i.Add(las); }
                }
            }
            if (las != null) {
                co++;
                i.Add("vor " + database.Works[lasNr].Date + " UTC", co.ToString(Constants.Format_Integer5) + database.Works[lasNr].PreviousValue);
            }
            return i;
        }

        public static void WriteColumnArrangementsInto(ComboBox columnArrangementSelector, Database database, int showingNo) {
            //if (InvokeRequired) {
            //    Invoke(new Action(() => WriteColumnArrangementsInto(columnArrangementSelector)));
            //    return;
            //}
            if (columnArrangementSelector != null) {
                columnArrangementSelector.Item.Clear();
                columnArrangementSelector.Enabled = false;
                columnArrangementSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
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
                        columnArrangementSelector.Item.Add(thisArrangement.Name, database.ColumnArrangements.IndexOf(thisArrangement).ToString());
                    }
                }
            }

            if (columnArrangementSelector != null) {
                columnArrangementSelector.Enabled = Convert.ToBoolean(columnArrangementSelector.Item.Count > 1);
                columnArrangementSelector.Text = columnArrangementSelector.Item.Count > 0 ? showingNo.ToString() : string.Empty;
            }
        }

        public void CollapesAll() {
            _collapsed.Clear();
            if (Database != null) {
                _collapsed.AddRange(Database.Column.SysChapter.Contents());
            }
            Invalidate_SortedRowData();
        }

        public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e) => false;

        public void CursorPos_Reset() => CursorPos_Set(null, null, false);

        public void CursorPos_Set(ColumnItem? column, RowData? row, bool ensureVisible) {
            if (_database == null || _database.ColumnArrangements.Count == 0 || CurrentArrangement[column] == null || SortedRows() == null || !SortedRows().Contains(row)) {
                column = null;
                row = null;
            }

            if (_cursorPosColumn == column && _cursorPosRow == row) { return; }
            _mouseOverText = string.Empty;
            _cursorPosColumn = column;
            _cursorPosRow = row;
            if (ensureVisible) { EnsureVisible(_cursorPosColumn, _cursorPosRow); }
            Invalidate();
            OnCursorPosChanged(new CellExtEventArgs(_cursorPosColumn, _cursorPosRow));
        }

        public ColumnItem? CursorPosColumn() => _cursorPosColumn;

        public RowData? CursorPosRow() => _cursorPosRow;

        public void Draw_Column_Head(Graphics gr, ColumnViewItem viewItem, Rectangle displayRectangleWoSlider, int lfdNo) {
            if (!IsOnScreen(viewItem, displayRectangleWoSlider)) { return; }
            if (_design == enBlueTableAppearance.OnlyMainColumnWithoutHead) { return; }
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
            var trichterState = enStates.Undefiniert;
            viewItem.TmpAutoFilterLocation = new Rectangle((int)viewItem.OrderTmpSpalteX1 + Column_DrawWidth(viewItem, displayRectangleWoSlider) - AutoFilterSize, HeadSize() - AutoFilterSize, AutoFilterSize, AutoFilterSize);
            var filtIt = Filter[viewItem.Column];
            if (viewItem.Column.AutoFilterSymbolPossible()) {
                if (filtIt != null) {
                    trichterState = enStates.Checked;
                    var anz = Autofilter_Text(viewItem.Column);
                    trichterText = anz > -100 ? (anz * -1).ToString() : "∞";
                } else {
                    trichterState = Autofilter_Sinnvoll(viewItem.Column) ? enStates.Standard : enStates.Standard_Disabled;
                }
            }
            var trichterSize = (AutoFilterSize - 4).ToString();
            if (filtIt != null) {
                trichterIcon = QuickImage.Get("Trichter|" + trichterSize + "|||FF0000");
            } else if (Filter.MayHasRowFilter(viewItem.Column)) {
                trichterIcon = QuickImage.Get("Trichter|" + trichterSize + "|||227722");
            } else if (viewItem.Column.AutoFilterSymbolPossible()) {
                trichterIcon = Autofilter_Sinnvoll(viewItem.Column)
                    ? QuickImage.Get("Trichter|" + trichterSize)
                    : QuickImage.Get("Trichter|" + trichterSize + "||128");
            }
            if (trichterState != enStates.Undefiniert) {
                Skin.Draw_Back(gr, enDesign.Button_AutoFilter, trichterState, viewItem.TmpAutoFilterLocation, null, false);
                Skin.Draw_Border(gr, enDesign.Button_AutoFilter, trichterState, viewItem.TmpAutoFilterLocation);
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
            if (trichterState == enStates.Undefiniert) {
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

                Point pos = new((int)viewItem.OrderTmpSpalteX1 + (int)((Column_DrawWidth(viewItem, displayRectangleWoSlider) - fs.Width) / 2.0), 3 + down);
                gr.DrawImageInRectAspectRatio(viewItem.Column.TmpCaptionBitmap, (int)viewItem.OrderTmpSpalteX1 + 2, (int)(pos.Y + fs.Height), Column_DrawWidth(viewItem, displayRectangleWoSlider) - 4, HeadSize() - (int)(pos.Y + fs.Height) - 6 - 18);
                // Dann der Text
                gr.TranslateTransform(pos.X, pos.Y);
                //GR.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                BlueFont.DrawString(gr, tx, _columnFont.Font(), new SolidBrush(viewItem.Column.ForeColor), 0, 0);
                gr.TranslateTransform(-pos.X, -pos.Y);

                #endregion
            } else {

                #region Spalte ohne Bild zeichnen

                Point pos = new((int)viewItem.OrderTmpSpalteX1 + (int)((Column_DrawWidth(viewItem, displayRectangleWoSlider) - fs.Height) / 2.0), HeadSize() - 4 - AutoFilterSize);
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
            if ((tmpSortDefinition != null && tmpSortDefinition.UsedForRowSort(viewItem.Column)) || viewItem.Column == Database.Column.SysChapter) {
                if (tmpSortDefinition.Reverse) {
                    gr.DrawImage(QuickImage.Get("ZA|11|5||||50"), (float)(viewItem.OrderTmpSpalteX1 + (Column_DrawWidth(viewItem, displayRectangleWoSlider) / 2.0) - 6), HeadSize() - 6 - AutoFilterSize);
                } else {
                    gr.DrawImage(QuickImage.Get("AZ|11|5||||50"), (float)(viewItem.OrderTmpSpalteX1 + (Column_DrawWidth(viewItem, displayRectangleWoSlider) / 2.0) - 6), HeadSize() - 6 - AutoFilterSize);
                }
            }

            #endregion
        }

        public bool EnsureVisible(ColumnItem? column, RowData? row) {
            var ok1 = EnsureVisible(CurrentArrangement?[column]);
            var ok2 = EnsureVisible(row);
            return ok1 && ok2;
        }

        public void ExpandAll() {
            _collapsed.Clear();
            CursorPos_Reset(); // Wenn eine Zeile markiert ist, man scrollt und expandiert, springt der Screen zurück, was sehr irriteiert
            Invalidate_SortedRowData();
        }

        public string Export_CSV(enFirstRow firstRow) => _database == null ? string.Empty : _database.Export_CSV(firstRow, CurrentArrangement, SortedRows());

        public string Export_CSV(enFirstRow firstRow, ColumnItem onlyColumn) {
            if (_database == null) { return string.Empty; }
            List<ColumnItem?> l = new() { onlyColumn };
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
        public List<RowItem?> FilteredRows() {
            if (_filteredRows != null) { return _filteredRows; }
            _filteredRows = Database.Row.CalculateFilteredRows(Filter);
            return _filteredRows;
        }

        public new void Focus() {
            if (Focused()) { return; }
            base.Focus();
        }

        public new bool Focused() => base.Focused || SliderY.Focused() || SliderX.Focused() || BTB.Focused || BCB.Focused;

        public void GetContextMenuItems(System.Windows.Forms.MouseEventArgs? e, ItemCollectionList? items, out object? hotItem, List<string> tags, ref bool cancel, ref bool translate) {
            hotItem = null;
            if (_database.IsParsing || _database.IsLoading) {
                cancel = true;
                return;
            }
            Database?.Load_Reload();
            if (e == null) {
                cancel = true;
                return;
            }
            CellOnCoordinate(e.X, e.Y, out _mouseOverColumn, out _mouseOverRow);
            tags.TagSet("CellKey", CellCollection.KeyOfCell(_mouseOverColumn, _mouseOverRow?.Row));
            if (_mouseOverColumn != null) {
                tags.TagSet("ColumnKey", _mouseOverColumn.Key.ToString());
            }
            if (_mouseOverRow?.Row != null) {
                tags.TagSet("RowKey", _mouseOverRow.Row.Key.ToString());
            }
        }

        public void ImportClipboard() {
            Develop.DebugPrint_InvokeRequired(InvokeRequired, false);
            if (!System.Windows.Forms.Clipboard.ContainsText()) {
                Notification.Show("Abbruch,<br>kein Text im Zwischenspeicher!", enImageCode.Information);
                return;
            }

            var nt = System.Windows.Forms.Clipboard.GetText();
            ImportCsv(nt);
        }

        public void ImportCsv(string csvtxt) => ImportCsv(_database, csvtxt);

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

        public void OpenSearchAndReplace() {
            if (!Database.IsAdministrator()) { return; }
            if (Database.ReadOnly) { return; }
            if (_searchAndReplace == null || _searchAndReplace.IsDisposed || !_searchAndReplace.Visible) {
                _searchAndReplace = new SearchAndReplace(this);
                _searchAndReplace.Show();
            }
        }

        public void ParseView(string toParse) {
            if (string.IsNullOrEmpty(toParse)) { return; }
            PinnedRows.Clear();
            _collapsed.Clear();
            foreach (var pair in toParse.GetAllTags()) {
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
                        CursorPos_Set(column, SortedRows().Get(row), false);
                        break;

                    case "tempsort":
                        _sortDefinitionTemporary = new RowSortDefinition(_database, pair.Value);
                        break;

                    case "pin":
                        PinnedRows.Add(_database.Row.SearchByKey(long.Parse(pair.Value)));
                        break;

                    case "collapsed":
                        _collapsed.Add(pair.Value.FromNonCritical());
                        break;

                    case "reduced":
                        var c = _database.Column.SearchByKey(long.Parse(pair.Value));
                        var cv = CurrentArrangement[c];
                        if (cv != null) { cv.TmpReduced = true; }
                        break;

                    default:
                        Develop.DebugPrint(enFehlerArt.Warnung, "Tag unbekannt: " + pair.Key);
                        break;
                }
            }
            Filter.OnChanged();
            Invalidate_FilteredRows(); // beim Parsen wirft der Filter kein Event ab
        }

        public void Pin(List<RowItem>? rows) {
            // Arbeitet mit Rows, weil nur eine Anpinngug möglich ist

            if (rows == null) { rows = new List<RowItem>(); }

            rows = rows.Distinct().ToList();
            if (!rows.IsDifferentTo(PinnedRows)) { return; }

            PinnedRows.Clear();
            PinnedRows.AddRange(rows);
            Invalidate_SortedRowData();
            OnPinnedChanged();
        }

        public void PinAdd(RowItem? row) {
            PinnedRows.Add(row);
            Invalidate_SortedRowData();
            OnPinnedChanged();
        }

        public void PinRemove(RowItem? row) {
            PinnedRows.Remove(row);
            Invalidate_SortedRowData();
            OnPinnedChanged();
        }

        public List<RowData>? SortedRows() {
            if (_sortedRowData != null) { return _sortedRowData; }

            try {
                var displayR = DisplayRectangleWithoutSlider();
                var maxY = 0;
                if (UserEdit_NewRowAllowed()) { maxY += _pix18; }
                var expanded = true;
                var lastCap = string.Empty;

                var sortedRowDataNew = Database == null
                    ? new List<RowData?>()
                    : Database.Row.CalculateSortedRows(FilteredRows(), SortUsed(), PinnedRows, _sortedRowData);
                if (!_sortedRowData.IsDifferentTo(sortedRowDataNew)) { return _sortedRowData; }

                _sortedRowData = new List<RowData?>();

                foreach (var thisRow in sortedRowDataNew) {
                    var thisRowData = thisRow;
                    if (_mouseOverRow != null && _mouseOverRow.Row == thisRow.Row && _mouseOverRow.Chapter == thisRow.Chapter) {
                        _mouseOverRow.GetDataFrom(thisRowData);
                        thisRowData = _mouseOverRow;
                    } // Mouse-Daten wiederverwenden
                    if (_cursorPosRow != null && _cursorPosRow.Row == thisRow.Row && _cursorPosRow.Chapter == thisRow.Chapter) {
                        _cursorPosRow.GetDataFrom(thisRowData);
                        thisRowData = _cursorPosRow;
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
                        if (_sortedRowData == null) {
                            // Folgender Fall: Die Row height reparirt die LinkedCell.
                            // Dadruch wird eine Zelle geändert. Das wiederrum kann _SortedRowData auf null setzen..
                            return null;
                        }
                        maxY += thisRowData.DrawHeight;
                    }

                    #endregion

                    _sortedRowData.Add(thisRowData);
                }

                if (_cursorPosRow?.Row != null && !_sortedRowData.Contains(_cursorPosRow)) { CursorPos_Reset(); }
                _mouseOverRow = null;

                #region Slider berechnen

                SliderSchaltenSenk(displayR, maxY);

                #endregion

                EnsureVisible(_cursorPosColumn, _cursorPosRow);
                OnVisibleRowsChanged();

                return SortedRows(); // Rekursiver aufruf. Manchmal funktiniert OnRowsSorted nicht...
            } catch {
                // Komisch, manchmal wird die Variable _SortedRowData verworfen.
                Invalidate_SortedRowData();
                return SortedRows();
            }
        }

        public RowData? View_NextRow(RowData? row) {
            if (_database == null) { return null; }
            var rowNr = SortedRows().IndexOf(row);
            return rowNr < 0 || rowNr >= SortedRows().Count - 1 ? null : SortedRows()[rowNr + 1];
        }

        public RowData? View_PreviousRow(RowData? row) {
            if (_database == null) { return null; }
            var rowNr = SortedRows().IndexOf(row);
            return rowNr < 1 ? null : SortedRows()[rowNr - 1];
        }

        public RowData? View_RowFirst() {
            if (_database == null) { return null; }
            var s = SortedRows();
            return s == null || s.Count == 0 ? null : SortedRows()[0];
        }

        public RowData? View_RowLast() => _database == null ? null : SortedRows().Count == 0 ? null : SortedRows()[SortedRows().Count - 1];

        public string ViewToString() {
            var x = "{";
            //   x = x & "<Filename>" & _Database.Filename
            x = x + "ArrangementNr=" + _arrangementNr;
            var tmp = Filter.ToString();
            if (tmp.Length > 2) {
                x = x + ", Filters=" + Filter;
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
            foreach (var thiscol in CurrentArrangement) {
                if (thiscol.TmpReduced) { x = x + ", Reduced=" + thiscol.Column.Key; }
            }
            if (_sortDefinitionTemporary?.Columns != null) {
                x = x + ", TempSort=" + _sortDefinitionTemporary;
            }
            x = x + ", CursorPos=" + CellCollection.KeyOfCell(_cursorPosColumn, _cursorPosRow?.Row);
            return x + "}";
        }

        public List<RowItem?> VisibleUniqueRows() {
            var l = new List<RowItem?>();
            var f = FilteredRows();
            var lockMe = new object();
            Parallel.ForEach(Database.Row, thisRowItem => {
                if (thisRowItem != null) {
                    if (f.Contains(thisRowItem) || PinnedRows.Contains(thisRowItem)) {
                        lock (lockMe) { l.Add(thisRowItem); }
                    }
                }
            });

            return l;
            //return Database.Row.CalculateVisibleRows(Filter, PinnedRows);
        }

        internal static void StartDatabaseService() {
            if (_serviceStarted) { return; }
            _serviceStarted = true;
            BlueBasics.MultiUserFile.MultiUserFile.AllFiles.ItemAdded += AllFiles_ItemAdded;
            BlueBasics.MultiUserFile.MultiUserFile.AllFiles.ItemRemoving += AllFiles_ItemRemoving;
            Database.DropConstructorMessage += Database_DropConstructorMessage;
        }

        internal bool NonPermanentPossible(ColumnViewItem thisViewItem) {
            if (_arrangementNr < 1) {
                return !thisViewItem.Column.IsFirst();
            }
            var nx = thisViewItem.NextVisible(CurrentArrangement);
            return nx == null || Convert.ToBoolean(nx.ViewType != enViewType.PermanentColumn);
        }

        internal bool PermanentPossible(ColumnViewItem thisViewItem) {
            if (_arrangementNr < 1) {
                return thisViewItem.Column.IsFirst();
            }
            var prev = thisViewItem.PreviewsVisible(CurrentArrangement);
            return prev == null || Convert.ToBoolean(prev.ViewType == enViewType.PermanentColumn);
        }

        protected override void DrawControl(Graphics gr, enStates state) {
            if (InvokeRequired) {
                Invoke(new Action(() => DrawControl(gr, state)));
                return;
            }

            _tmpCursorRect = Rectangle.Empty;

            // Listboxen bekommen keinen Focus, also Tabellen auch nicht. Basta.
            if (Convert.ToBoolean(state & enStates.Standard_HasFocus)) {
                state ^= enStates.Standard_HasFocus;
            }

            if (_database == null || DesignMode || ShowWaitScreen) {
                DrawWaitScreen(gr);
                return;
            }

            lock (_lockUserAction) {
                //if (_InvalidExternal) { FillExternalControls(); }
                if (Convert.ToBoolean(state & enStates.Standard_Disabled)) { CursorPos_Reset(); }
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
                    case enBlueTableAppearance.Standard:
                        Draw_Table_Std(gr, sr, state, displayRectangleWoSlider, firstVisibleRow, lastVisibleRow);
                        break;

                    case enBlueTableAppearance.OnlyMainColumnWithoutHead:
                        Draw_Table_ListboxStyle(gr, sr, state, displayRectangleWoSlider, firstVisibleRow, lastVisibleRow);
                        break;

                    default:
                        Develop.DebugPrint(_design);
                        break;
                }
            }
        }

        protected void InitializeSkin() {
            _cellFont = Skin.GetBlueFont(enDesign.Table_Cell, enStates.Standard).Scale(FontScale);
            _columnFont = Skin.GetBlueFont(enDesign.Table_Column, enStates.Standard).Scale(FontScale);
            _chapterFont = Skin.GetBlueFont(enDesign.Table_Cell_Chapter, enStates.Standard).Scale(FontScale);
            _columnFilterFont = BlueFont.Get(_columnFont.FontName, _columnFont.FontSize, false, false, false, false, true, Color.White, Color.Red, false, false, false);

            _newRowFont = Skin.GetBlueFont(enDesign.Table_Cell_New, enStates.Standard).Scale(FontScale);
            if (Database != null) {
                _pix16 = GetPix(16, _cellFont, Database.GlobalScale);
                _pix18 = GetPix(18, _cellFont, Database.GlobalScale);
                _rowCaptionFontY = GetPix(26, _cellFont, Database.GlobalScale);
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
                Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
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
                CellDoubleClickEventArgs ea = new(_mouseOverColumn, _mouseOverRow?.Row, true);
                if (Mouse_IsInHead()) {
                    Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
                    DoubleClick?.Invoke(this, ea);
                } else {
                    if (_mouseOverRow == null && MousePos().Y > HeadSize() + _pix18) {
                        Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
                        DoubleClick?.Invoke(this, ea);
                    } else {
                        DoubleClick?.Invoke(this, ea);
                        if (Database.PowerEdit.Subtract(DateTime.Now).TotalSeconds > 0) { ea.StartEdit = true; }
                        if (ea.StartEdit) { Cell_Edit(_mouseOverColumn, _mouseOverRow, true); }
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

                _database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());

                switch (e.KeyCode) {
                    case System.Windows.Forms.Keys.Oemcomma: // normales ,
                        if (e.Modifiers == System.Windows.Forms.Keys.Control) {
                            var lp = CellCollection.ErrorReason(_cursorPosColumn, _cursorPosRow?.Row, enErrorReason.EditGeneral);
                            Neighbour(_cursorPosColumn, _cursorPosRow, enDirection.Oben, out _, out var newRow);
                            if (newRow == _cursorPosRow) { lp = "Das geht nicht bei dieser Zeile."; }
                            if (string.IsNullOrEmpty(lp) && newRow != null) {
                                UserEdited(this, newRow?.Row.CellGetString(_cursorPosColumn), _cursorPosColumn, _cursorPosRow?.Row, newRow?.Chapter, true);
                            } else {
                                NotEditableInfo(lp);
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
                            var l2 = CellCollection.ErrorReason(_cursorPosColumn, _cursorPosRow?.Row, enErrorReason.EditGeneral);
                            if (string.IsNullOrEmpty(l2)) {
                                UserEdited(this, string.Empty, _cursorPosColumn, _cursorPosRow?.Row, _cursorPosRow?.Chapter, true);
                            } else {
                                NotEditableInfo(l2);
                            }
                        }
                        break;

                    case System.Windows.Forms.Keys.Delete:
                        if (_cursorPosRow is null || _cursorPosRow.Row.CellIsNullOrEmpty(_cursorPosColumn)) {
                            _isinKeyDown = false;
                            return;
                        }
                        var l = CellCollection.ErrorReason(_cursorPosColumn, _cursorPosRow?.Row, enErrorReason.EditGeneral);
                        if (string.IsNullOrEmpty(l)) {
                            UserEdited(this, string.Empty, _cursorPosColumn, _cursorPosRow?.Row, _cursorPosRow?.Chapter, true);
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
                            CopyToClipboard(_cursorPosColumn, _cursorPosRow?.Row, true);
                        }
                        break;

                    case System.Windows.Forms.Keys.F:
                        if (e.Modifiers == System.Windows.Forms.Keys.Control) {
                            Search x = new(this);
                            x.Show();
                        }
                        break;

                    case System.Windows.Forms.Keys.F2:
                        Cell_Edit(_cursorPosColumn, _cursorPosRow, true);
                        break;

                    case System.Windows.Forms.Keys.V:
                        if (e.Modifiers == System.Windows.Forms.Keys.Control) {
                            if (_cursorPosColumn != null && _cursorPosRow != null) {
                                if (!_cursorPosColumn.Format.TextboxEditPossible() && _cursorPosColumn.Format != enDataFormat.Columns_für_LinkedCellDropdown && _cursorPosColumn.Format != enDataFormat.Values_für_LinkedCellDropdown) {
                                    NotEditableInfo("Die Zelle hat kein passendes Format.");
                                    _isinKeyDown = false;
                                    return;
                                }
                                if (!System.Windows.Forms.Clipboard.ContainsText()) {
                                    NotEditableInfo("Kein Text in der Zwischenablage.");
                                    _isinKeyDown = false;
                                    return;
                                }
                                var ntxt = System.Windows.Forms.Clipboard.GetText();
                                if (_cursorPosRow?.Row.CellGetString(_cursorPosColumn) == ntxt) {
                                    _isinKeyDown = false;
                                    return;
                                }
                                var l2 = CellCollection.ErrorReason(_cursorPosColumn, _cursorPosRow?.Row, enErrorReason.EditGeneral);
                                if (string.IsNullOrEmpty(l2)) {
                                    UserEdited(this, ntxt, _cursorPosColumn, _cursorPosRow?.Row, _cursorPosRow?.Chapter, true);
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

        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e) {
            base.OnMouseDown(e);
            if (_database == null) { return; }
            lock (_lockUserAction) {
                if (_isinMouseDown) { return; }
                _isinMouseDown = true;
                _database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
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
                        Invalidate_SortedRowData();
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
                    _database?.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
                } else {
                    if (_mouseOverColumn != null && e.Y < HeadSize()) {
                        _mouseOverText = _mouseOverColumn.QuickInfoText(string.Empty);
                    } else if (_mouseOverColumn != null && _mouseOverRow != null) {
                        if (_mouseOverColumn.Format.NeedTargetDatabase()) {
                            if (_mouseOverColumn.LinkedDatabase() != null) {
                                switch (_mouseOverColumn.Format) {
                                    case enDataFormat.Columns_für_LinkedCellDropdown:
                                        var txt = _mouseOverRow.Row.CellGetString(_mouseOverColumn);
                                        if (int.TryParse(txt, out var colKey)) {
                                            var c = _mouseOverColumn.LinkedDatabase().Column.SearchByKey(colKey);
                                            if (c != null) { _mouseOverText = c.QuickInfoText(_mouseOverColumn.Caption + ": " + c.Caption); }
                                        }
                                        break;

                                    case enDataFormat.Verknüpfung_zu_anderer_Datenbank_Skriptgesteuert:
                                    case enDataFormat.Verknüpfung_zu_anderer_Datenbank:
                                    case enDataFormat.Values_für_LinkedCellDropdown:
                                        var (lcolumn, _, info) = CellCollection.LinkedCellData(_mouseOverColumn, _mouseOverRow.Row, false, false);
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
                            _mouseOverText = Database.UndoText(_mouseOverColumn, _mouseOverRow.Row);
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
                FloatingForm.Close(this, enDesign.Form_KontextMenu);
                //EnsureVisible(_MouseOver) <-Nur MouseDown, siehe Da
                //CursorPos_Set(_MouseOver) <-Nur MouseDown, siehe Da
                ColumnViewItem viewItem = null;
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
                        if (_mouseOverRow != null && _mouseOverColumn.Format == enDataFormat.Button) {
                            OnButtonCellClicked(new CellEventArgs(_mouseOverColumn, _mouseOverRow?.Row));
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
                Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
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
                Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
                CurrentArrangement?.Invalidate_DrawWithOfAllItems();
                _isinSizeChanged = false;
            }
            Invalidate_SortedRowData(); // Zellen können ihre Größe ändern. z.B. die Zeilenhöhe
        }

        //protected override void OnResize(System.EventArgs e) {
        //    base.OnResize(e);
        //    if (_Database == null) { return; }
        //    lock (Lock_UserAction) {
        //        if (ISIN_Resize) { return; }
        //        ISIN_Resize = true;
        //        Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
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
                Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
                _isinVisibleChanged = false;
            }
        }

        private static void AllFiles_ItemAdded(object sender, ListEventArgs e) {
            if (e.Item is Database db) {
                db.NeedPassword += Database_NeedPassword;
                db.GenerateLayoutInternal += DB_GenerateLayoutInternal;
                db.Loaded += TabAdministration.CheckDatabase;
            }
        }

        private static void AllFiles_ItemRemoving(object sender, ListEventArgs e) {
            if (e.Item is Database db) {
                db.NeedPassword -= Database_NeedPassword;
                db.GenerateLayoutInternal -= DB_GenerateLayoutInternal;
                db.Loaded -= TabAdministration.CheckDatabase;
            }
        }

        private static void Database_DropConstructorMessage(object sender, MessageEventArgs e) {
            if (!e.Shown) {
                e.Shown = true;
                Notification.Show(e.Message, enImageCode.Datenbank);
            }

            if (e.Type is enFehlerArt.DevelopInfo or enFehlerArt.Info) { return; }

            if (e.WrittenToLogifile) { return; }
            e.WrittenToLogifile = true;
            Develop.DebugPrint(e.Type, e.Message);
        }

        private static void DB_GenerateLayoutInternal(object sender, GenerateLayoutInternalEventargs e) {
            if (e.Handled) { return; }
            e.Handled = true;
            ItemCollectionPad pad = new(e.LayoutId, e.Row.Database, e.Row.Key);
            pad.SaveAsBitmap(e.Filename);
        }

        private static void Draw_CellTransparentDirect(Graphics gr, string toDraw, Rectangle drawarea, BlueFont? font, ColumnItem? contentHolderCellColumn, int pix16, enShortenStyle style, enBildTextVerhalten bildTextverhalten, enStates state) {
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

        private static void Draw_CellTransparentDirect_OneLine(Graphics gr, string drawString, ColumnItem? contentHolderColumnStyle, Rectangle drawarea, int txtYPix, bool changeToDot, BlueFont? font, int pix16, enShortenStyle style, enBildTextVerhalten bildTextverhalten, enStates state) {
            Rectangle r = new(drawarea.Left, drawarea.Top + txtYPix, drawarea.Width, pix16);

            if (r.Bottom + pix16 > drawarea.Bottom) {
                if (r.Bottom > drawarea.Bottom) { return; }
                if (changeToDot) { drawString = "..."; }// Die letzte Zeile noch ganz hinschreiben
            }

            var tmpData = CellItem.GetDrawingData(contentHolderColumnStyle, drawString, style, bildTextverhalten);
            var tmpImageCode = tmpData.Item2;
            if (tmpImageCode != null) { tmpImageCode = QuickImage.Get(tmpImageCode, Skin.AdditionalState(state)); }

            Skin.Draw_FormatedText(gr, tmpData.Item1, tmpImageCode, (enAlignment)contentHolderColumnStyle.Align, r, null, false, font, false);
        }

        private static int GetPix(int pix, BlueFont? f, double scale) => Skin.FormatedText_NeededSize("@|", null, f, (int)((pix * scale) + 0.5)).Height;

        private static bool Mouse_IsInAutofilter(ColumnViewItem viewItem, System.Windows.Forms.MouseEventArgs e) => viewItem != null && viewItem.TmpAutoFilterLocation.Width != 0 && viewItem.Column.AutoFilterSymbolPossible() && viewItem.TmpAutoFilterLocation.Contains(e.X, e.Y);

        private static bool Mouse_IsInRedcueButton(ColumnViewItem viewItem, System.Windows.Forms.MouseEventArgs e) => viewItem != null && viewItem.TmpReduceLocation.Width != 0 && viewItem.TmpReduceLocation.Contains(e.X, e.Y);

        private static void NotEditableInfo(string reason) => Notification.Show(reason, enImageCode.Kreuz);

        private static void UserEdited(Table table, string newValue, ColumnItem? column, RowItem? row, string chapter, bool formatWarnung) {
            if (column == null) {
                NotEditableInfo("Keine Spalte angegeben.");
                return;
            }

            if (column.Format is enDataFormat.Verknüpfung_zu_anderer_Datenbank_Skriptgesteuert or enDataFormat.Verknüpfung_zu_anderer_Datenbank) {
                var (lcolumn, lrow, info) = CellCollection.LinkedCellData(column, row, true, false);
                if (lcolumn == null || lrow == null) {
                    NotEditableInfo("Zelle in verlinkter Datenbank nicht vorhanden:\r\n" + info);
                    return;
                }
                UserEdited(table, newValue, lcolumn, lrow, chapter, formatWarnung);
                if (table.Database == column.Database) { table.CursorPos_Set(column, row, false, chapter); }
                return;
            }

            if (row == null && column != column.Database.Column[0]) {
                NotEditableInfo("Neue Zeilen müssen mit der ersten Spalte beginnen");
                return;
            }

            newValue = column.AutoCorrect(newValue);
            if (row != null) {
                if (newValue == row.CellGetString(column)) { return; }
            } else {
                if (string.IsNullOrEmpty(newValue)) { return; }
            }

            CellValueChangingByUserEventArgs ed = new(column, row, newValue, string.Empty);
            table.OnCellValueChangingByUser(ed);
            var cancelReason = ed.CancelReason;
            if (string.IsNullOrEmpty(cancelReason) && formatWarnung && !string.IsNullOrEmpty(newValue)) {
                if (!newValue.IsFormat(column)) {
                    if (MessageBox.Show("Ihre Eingabe entspricht<br><u>nicht</u> dem erwarteten Format!<br><br>Trotzdem übernehmen?", enImageCode.Information, "Ja", "Nein") != 0) {
                        cancelReason = "Abbruch, das das erwartete Format nicht eingehalten wurde.";
                    }
                }
            }

            if (string.IsNullOrEmpty(cancelReason)) {
                if (row == null) {
                    var f = CellCollection.ErrorReason(column.Database.Column[0], null, enErrorReason.EditGeneral);
                    if (!string.IsNullOrEmpty(f)) { NotEditableInfo(f); return; }
                    row = column.Database.Row.Add(newValue);
                    if (table.Database == column.Database) {
                        var l = table.FilteredRows();
                        if (!l.Contains(row)) {
                            if (MessageBox.Show("Die neue Zeile ist ausgeblendet.<br>Soll sie <b>angepinnt</b> werden?", enImageCode.Pinnadel, "anpinnen", "abbrechen") == 0) {
                                table.PinAdd(row);
                            }
                        }

                        var sr = table.SortedRows();
                        var rd = sr.Get(row);
                        table.CursorPos_Set(table.Database.Column[0], rd, true);
                    }
                } else {
                    var f = CellCollection.ErrorReason(column, row, enErrorReason.EditGeneral);
                    if (!string.IsNullOrEmpty(f)) { NotEditableInfo(f); return; }
                    row.CellSet(column, newValue);
                }
                if (table.Database == column.Database) { table.CursorPos_Set(column, row, false, chapter); }
                row.DoAutomatic(true, false, 5, "value changed");

                // EnsureVisible ganz schlecht: Daten verändert, keine Positionen bekannt - und da soll sichtbar gemacht werden?
                // CursorPos.EnsureVisible(SliderX, SliderY, DisplayRectangle)
            } else {
                NotEditableInfo(cancelReason);
            }
        }

        private void _Database_CellValueChanged(object sender, CellEventArgs e) {
            if (_filteredRows != null) {
                if (Filter[e.Column] != null ||
                    SortUsed() == null ||
                    SortUsed().UsedForRowSort(e.Column) ||
                    Filter.MayHasRowFilter(e.Column) ||
                    e.Column == Database.Column.SysChapter) {
                    Invalidate_FilteredRows();
                }
            }
            Invalidate_DrawWidth(e.Column);

            Invalidate_SortedRowData();
            Invalidate();
            OnCellValueChanged(e);
        }

        private void _Database_ColumnContentChanged(object sender, ListEventArgs e) {
            Invalidate_AllColumnArrangements();
            Invalidate_HeadSize();
            Invalidate_SortedRowData();
        }

        //private void _Database_ColumnKeyChanged(object sender, KeyChangedEventArgs e) {
        //    // Ist aktuell nur möglich,wenn Pending Changes eine neue Zeile machen
        //    if (string.IsNullOrEmpty(_StoredView)) { return; }
        //    _StoredView = ColumnCollection.ChangeKeysInString(_StoredView, e.KeyOld, e.KeyNew);
        //}

        private void _Database_DatabaseLoaded(object sender, LoadedEventArgs e) {
            // Wird auch bei einem Reload ausgeführt.
            // Es kann aber sein, dass eine Ansicht zurückgeholt wurde, und die Werte stimmen.
            // Deswegen prüfen, ob wirklich alles geleöscht werden muss, oder weiter behalten werden kann.
            // Auf Nothing  muss auch geprüft werden, da bei einem Dispose oder beim Beenden sich die Datenbank auch änsdert....
            Invalidate_HeadSize();
            var f = string.Empty;
            _mouseOverText = string.Empty;
            if (Filter != null) {
                if (e.OnlyReload) { f = Filter.ToString(); }
                Filter.Changed -= Filter_Changed;
                Filter = null;
            }
            if (_database != null) {
                Filter = new FilterCollection(_database, f);
                Filter.Changed += Filter_Changed;
                if (e.OnlyReload) {
                    if (_arrangementNr != 1) {
                        if (_database.ColumnArrangements == null || _arrangementNr >= _database.ColumnArrangements.Count || CurrentArrangement == null || !_database.PermissionCheck(CurrentArrangement.PermissionGroups_Show, null)) {
                            _arrangementNr = 1;
                        }
                    }
                    if (_mouseOverColumn != null && _mouseOverColumn.Database != _database) {
                        _mouseOverColumn = null;
                        _mouseOverRow = null;
                    }
                    if (_cursorPosColumn != null && _cursorPosColumn.Database != _database) {
                        _cursorPosColumn = null;
                        _cursorPosRow = null;
                    }
                } else {
                    _mouseOverColumn = null;
                    _mouseOverRow = null;
                    _cursorPosColumn = null;
                    _cursorPosRow = null;
                    _arrangementNr = 1;
                }
            } else {
                _mouseOverColumn = null;
                _mouseOverRow = null;
                _cursorPosColumn = null;
                _cursorPosRow = null;
                _arrangementNr = 1;
            }
            _sortDefinitionTemporary = null;
            Invalidate_AllColumnArrangements();
            Invalidate_HeadSize();
            Invalidate_FilteredRows();
            OnViewChanged();
            if (e.OnlyReload) {
                if (string.IsNullOrEmpty(_storedView)) { Develop.DebugPrint("Stored View Empty!"); }
                ParseView(_storedView);
                _storedView = string.Empty;
            }
        }

        private void _Database_Disposing(object sender, System.EventArgs e) => Database = null;

        private void _Database_DropMessage(object sender, MessageEventArgs e) {
            if (_database.IsAdministrator()) {
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

        private void _Database_Row_RowAdded(object sender, RowEventArgs e) {
            OnRowAdded(sender, e);
            Invalidate_FilteredRows();
        }

        //private void _Database_RowKeyChanged(object sender, KeyChangedEventArgs e) {
        //    // Ist aktuell nur möglich, wenn Pending Changes eine neue Zeile machen
        //    if (string.IsNullOrEmpty(_StoredView)) { return; }
        //    _StoredView = _StoredView.Replace("RowKey=" + e.KeyOld + "}", "RowKey=" + e.KeyNew + "}");
        //}

        private void _Database_RowRemoved(object sender, System.EventArgs e) => Invalidate_FilteredRows();

        private void _Database_SavedToDisk(object sender, System.EventArgs e) => Invalidate();

        private void _Database_SortParameterChanged(object sender, System.EventArgs e) => Invalidate_SortedRowData();

        private void _Database_StopAllWorking(object sender, MultiUserFileStopWorkingEventArgs e) => CloseAllComponents();

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

        private void AutoFilter_Close() {
            if (_autoFilter != null) {
                _autoFilter.FilterComand -= AutoFilter_FilterComand;
                _autoFilter.Dispose();
                _autoFilter = null;
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
                    e.Column.GetUniques(VisibleUniqueRows(), out var einzigartig, out _);
                    if (einzigartig.Count > 0) {
                        Filter.Add(new FilterItem(e.Column, enFilterType.Istgleich_ODER_GroßKleinEgal, einzigartig));
                        Notification.Show("Die aktuell einzigartigen Einträge wurden berechnet<br>und als <b>ODER-Filter</b> gespeichert.", enImageCode.Trichter);
                    } else {
                        Notification.Show("Filterung dieser Spalte gelöscht,<br>da <b>alle Einträge</b> mehrfach vorhanden sind.", enImageCode.Trichter);
                    }
                    break;

                case "donichteinzigartig":
                    Filter.Remove(e.Column);
                    e.Column.GetUniques(VisibleUniqueRows(), out _, out var xNichtEinzigartig);
                    if (xNichtEinzigartig.Count > 0) {
                        Filter.Add(new FilterItem(e.Column, enFilterType.Istgleich_ODER_GroßKleinEgal, xNichtEinzigartig));
                        Notification.Show("Die aktuell <b>nicht</b> einzigartigen Einträge wurden berechnet<br>und als <b>ODER-Filter</b> gespeichert.", enImageCode.Trichter);
                    } else {
                        Notification.Show("Filterung dieser Spalte gelöscht,<br>da <b>alle Einträge</b> einzigartig sind.", enImageCode.Trichter);
                    }
                    break;

                case "dospaltenvergleich": {
                        List<RowItem> ro = new();
                        ro.AddRange(VisibleUniqueRows());

                        ItemCollectionList ic = new();
                        foreach (var thisColumnItem in e.Column.Database.Column) {
                            if (thisColumnItem != null && thisColumnItem != e.Column) { ic.Add(thisColumnItem); }
                        }
                        ic.Sort();

                        var r = InputBoxListBoxStyle.Show("Mit welcher Spalte vergleichen?", ic, enAddType.None, true);
                        if (r == null || r.Count == 0) { return; }

                        var c = e.Column.Database.Column.SearchByKey(LongParse(r[0]));

                        List<string> d = new();
                        foreach (var thisR in ro) {
                            if (thisR.CellGetString(e.Column) != thisR.CellGetString(c)) { d.Add(thisR.CellFirstString()); }
                        }
                        if (d.Count > 0) {
                            Filter.Add(new FilterItem(e.Column.Database.Column[0], enFilterType.Istgleich_ODER_GroßKleinEgal, d));
                            Notification.Show("Die aktuell <b>unterschiedlichen</b> Einträge wurden berechnet<br>und als <b>ODER-Filter</b> in der <b>ersten Spalte</b> gespeichert.", enImageCode.Trichter);
                        } else {
                            Notification.Show("Keine Filter verändert,<br>da <b>alle Einträge</b> identisch sind.", enImageCode.Trichter);
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
                            Filter.Add(new FilterItem(e.Column, enFilterType.IstGleich_ODER | enFilterType.GroßKleinEgal, searchValue));
                        }
                        break;
                    }

                case "donotclipboard": {
                        var clipTmp = System.Windows.Forms.Clipboard.GetText();
                        clipTmp = clipTmp.RemoveChars(Constants.Char_NotFromClip);
                        clipTmp = clipTmp.TrimEnd("\r\n");
                        var columInhalt = Database.Export_CSV(enFirstRow.Without, e.Column, null).SplitAndCutByCrToList().SortedDistinctList();
                        columInhalt.RemoveString(clipTmp.SplitAndCutByCrToList().SortedDistinctList(), false);
                        Filter.Remove(e.Column);
                        if (columInhalt.Count > 0) {
                            Filter.Add(new FilterItem(e.Column, enFilterType.IstGleich_ODER | enFilterType.GroßKleinEgal, columInhalt));
                        }
                        break;
                    }
                default:
                    Develop.DebugPrint("Unbekannter Comand:   " + e.Comand);
                    break;
            }
            OnAutoFilterClicked(new FilterEventArgs(e.Filter));
        }

        private void AutoFilter_Show(ColumnViewItem columnviewitem, int screenx, int screeny) {
            if (columnviewitem == null) { return; }
            if (_design == enBlueTableAppearance.OnlyMainColumnWithoutHead) { return; }
            if (!columnviewitem.Column.AutoFilterSymbolPossible()) { return; }
            if (Filter.Any(thisFilter => thisFilter != null && thisFilter.Column == columnviewitem.Column && !string.IsNullOrEmpty(thisFilter.Herkunft))) {
                MessageBox.Show("Dieser Filter wurde<br>automatisch gesetzt.", enImageCode.Information, "OK");
                return;
            }
            Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
            //OnBeforeAutoFilterShow(new ColumnEventArgs(columnviewitem.Column));
            _autoFilter = new AutoFilter(columnviewitem.Column, Filter, PinnedRows);
            _autoFilter.Position_LocateToPosition(new Point(screenx + (int)columnviewitem.OrderTmpSpalteX1, screeny + HeadSize()));
            _autoFilter.Show();
            _autoFilter.FilterComand += AutoFilter_FilterComand;
            Develop.Debugprint_BackgroundThread();
        }

        private bool Autofilter_Sinnvoll(ColumnItem? column) {
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
        private int Autofilter_Text(ColumnItem? column) {
            if (column.TmpIfFilterRemoved != null) { return (int)column.TmpIfFilterRemoved; }
            var tfilter = new FilterCollection(column.Database);
            foreach (var thisFilter in Filter) {
                if (thisFilter != null && thisFilter.Column != column) { tfilter.Add(thisFilter); }
            }
            var temp = Database.Row.CalculateFilteredRows(tfilter);
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

        private void BTB_NeedDatabaseOfAdditinalSpecialChars(object sender, MultiUserFileGiveBackEventArgs e) => e.File = Database;

        private void Cell_Edit(ColumnItem? cellInThisDatabaseColumn, RowData? cellInThisDatabaseRow, bool withDropDown) {
            Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
            ColumnItem? contentHolderCellColumn;
            RowItem? contentHolderCellRow;

            if (Database.ReloadNeeded) { Database.Load_Reload(); }

            var f = Database.ErrorReason(enErrorReason.EditGeneral);
            if (!string.IsNullOrEmpty(f)) { NotEditableInfo(f); return; }
            if (cellInThisDatabaseColumn == null) { return; }// Klick ins Leere

            var viewItem = CurrentArrangement[cellInThisDatabaseColumn];
            if (viewItem == null) {
                NotEditableInfo("Ansicht veraltet");
                return;
            }

            if (cellInThisDatabaseColumn.Format is enDataFormat.Verknüpfung_zu_anderer_Datenbank_Skriptgesteuert or enDataFormat.Verknüpfung_zu_anderer_Datenbank) {
                string info;
                (contentHolderCellColumn, contentHolderCellRow, info) = CellCollection.LinkedCellData(cellInThisDatabaseColumn, cellInThisDatabaseRow?.Row, true, true);
                if (contentHolderCellColumn == null || contentHolderCellRow == null) {
                    NotEditableInfo("In verknüpfter Datenbank nicht vorhanden:\r\n" + info);
                    return;
                }
            } else {
                contentHolderCellColumn = cellInThisDatabaseColumn;
                contentHolderCellRow = cellInThisDatabaseRow?.Row;
            }

            if (!contentHolderCellColumn.DropdownBearbeitungErlaubt) { withDropDown = false; }
            var dia = ColumnItem.UserEditDialogTypeInTable(contentHolderCellColumn, withDropDown);
            if (dia == enEditTypeTable.None) {
                NotEditableInfo("Diese Spalte kann generell nicht bearbeitet werden.");
                return;
            }
            if (!CellCollection.UserEditPossible(contentHolderCellColumn, contentHolderCellRow, enErrorReason.EditGeneral)) {
                NotEditableInfo(CellCollection.ErrorReason(contentHolderCellColumn, contentHolderCellRow, enErrorReason.EditGeneral));
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
                if (!EnsureVisible(viewItem)) {
                    NotEditableInfo("Zelle konnte nicht angezeigt werden.");
                    return;
                }
                if (!IsOnScreen(viewItem, DisplayRectangle)) {
                    NotEditableInfo("Die Zelle wird nicht angezeigt.");
                    return;
                }
                SliderY.Value = 0;
            }
            var cancel = "";
            if (cellInThisDatabaseRow != null) {
                CellCancelEventArgs ed = new(cellInThisDatabaseColumn, cellInThisDatabaseRow.Row, cancel);
                OnEditBeforeBeginEdit(ed);
                cancel = ed.CancelReason;
            } else {
                RowCancelEventArgs ed = new(null, cancel);
                OnEditBeforeNewRow(ed);
                cancel = ed.CancelReason;
            }
            if (!string.IsNullOrEmpty(cancel)) {
                NotEditableInfo(cancel);
                return;
            }
            switch (dia) {
                case enEditTypeTable.Textfeld:
                    Cell_Edit_TextBox(cellInThisDatabaseColumn, cellInThisDatabaseRow, contentHolderCellColumn, contentHolderCellRow, BTB, 0, 0);
                    break;

                case enEditTypeTable.Textfeld_mit_Auswahlknopf:
                    Cell_Edit_TextBox(cellInThisDatabaseColumn, cellInThisDatabaseRow, contentHolderCellColumn, contentHolderCellRow, BCB, 20, 18);
                    break;

                case enEditTypeTable.Dropdown_Single:
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

                case enEditTypeTable.Farb_Auswahl_Dialog:
                    if (cellInThisDatabaseColumn != contentHolderCellColumn || cellInThisDatabaseRow?.Row != contentHolderCellRow) {
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
                    if (cellInThisDatabaseColumn != contentHolderCellColumn || cellInThisDatabaseRow?.Row != contentHolderCellRow) {
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

        private void Cell_Edit_Color(ColumnItem? cellInThisDatabaseColumn, RowData? cellInThisDatabaseRow) {
            ColDia.Color = cellInThisDatabaseRow.Row.CellGetColor(cellInThisDatabaseColumn);
            ColDia.Tag = CellCollection.KeyOfCell(cellInThisDatabaseColumn, cellInThisDatabaseRow.Row);
            List<int> colList = new();
            foreach (var thisRowItem in _database.Row) {
                if (thisRowItem != null) {
                    if (thisRowItem.CellGetInteger(cellInThisDatabaseColumn) != 0) {
                        colList.Add(thisRowItem.CellGetColorBgr(cellInThisDatabaseColumn));
                    }
                }
            }
            colList.Sort();
            ColDia.CustomColors = colList.Distinct().ToArray();
            ColDia.ShowDialog();
            UserEdited(this, Color.FromArgb(255, ColDia.Color).ToArgb().ToString(), cellInThisDatabaseColumn, cellInThisDatabaseRow?.Row, cellInThisDatabaseRow?.Chapter, false);
        }

        private void Cell_Edit_Dropdown(ColumnItem? cellInThisDatabaseColumn, RowData? cellInThisDatabaseRow, ColumnItem? contentHolderCellColumn, RowItem? contentHolderCellRow) {
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

            ItemCollectionList t = new() {
                Appearance = enBlueListBoxAppearance.DropdownSelectbox
            };

            ItemCollectionList.GetItemCollection(t, contentHolderCellColumn, contentHolderCellRow, enShortenStyle.Replaced, 1000);
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
                t.Add("Erweiterte Eingabe", "#Erweitert", QuickImage.Get(enImageCode.Stift), true, Constants.FirstSortChar + "1");
                t.AddSeparator(Constants.FirstSortChar + "2");
                t.Sort();
            }

            var dropDownMenu = FloatingInputBoxListBoxStyle.Show(t, new CellExtEventArgs(cellInThisDatabaseColumn, cellInThisDatabaseRow), this, Translate);
            dropDownMenu.ItemClicked += DropDownMenu_ItemClicked;
            Develop.Debugprint_BackgroundThread();
        }

        private void Cell_Edit_FileSystem(ColumnItem? cellInThisDatabaseColumn, RowData? cellInThisDatabaseRow) {
            var l = FileSystem(cellInThisDatabaseColumn);
            if (l == null) { return; }
            UserEdited(this, l.JoinWithCr(), cellInThisDatabaseColumn, cellInThisDatabaseRow?.Row, cellInThisDatabaseRow?.Chapter, false);
        }

        private bool Cell_Edit_TextBox(ColumnItem? cellInThisDatabaseColumn, RowData? cellInThisDatabaseRow, ColumnItem? contentHolderCellColumn, RowItem? contentHolderCellRow, TextBox Box, int addWith, int isHeight) {
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

            Box.Tag = CellCollection.KeyOfCell(cellInThisDatabaseColumn, cellInThisDatabaseRow?.Row); // ThisDatabase, der Wert wird beim einchecken in die Fremdzelle geschrieben
            if (Box is ComboBox box) {
                ItemCollectionList.GetItemCollection(box.Item, contentHolderCellColumn, contentHolderCellRow, enShortenStyle.Replaced, 1000);
                if (box.Item.Count == 0) {
                    return Cell_Edit_TextBox(cellInThisDatabaseColumn, cellInThisDatabaseRow, contentHolderCellColumn, contentHolderCellRow, BTB, 0, 0);
                }
            }

            if (string.IsNullOrEmpty(Box.Text)) {
                Box.Text = CellCollection.AutomaticInitalValue(contentHolderCellColumn, contentHolderCellRow);
            }

            Box.Visible = true;
            Box.BringToFront();
            Box.Focus();
            return true;
        }

        private void CellOnCoordinate(int xpos, int ypos, out ColumnItem? column, out RowData? row) {
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

        private int Column_DrawWidth(ColumnViewItem viewItem, Rectangle displayRectangleWoSlider) {
            // Hier wird die ORIGINAL-Spalte gezeichnet, nicht die FremdZelle!!!!

            if (viewItem.TmpDrawWidth != null) { return (int)viewItem.TmpDrawWidth; }
            if (viewItem?.Column == null) { return 0; }

            if (_design == enBlueTableAppearance.OnlyMainColumnWithoutHead) {
                viewItem.TmpDrawWidth = displayRectangleWoSlider.Width - 1;
                return (int)viewItem.TmpDrawWidth;
            }

            if (viewItem.TmpReduced) {
                viewItem.TmpDrawWidth = 16;
            } else {
                viewItem.TmpDrawWidth = viewItem.ViewType == enViewType.PermanentColumn
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

        private SizeF ColumnCaptionText_Size(ColumnItem? column) {
            if (column.TmpCaptionTextSize.Width > 0) { return column.TmpCaptionTextSize; }
            if (_columnFont == null) { return new SizeF(_pix16, _pix16); }
            column.TmpCaptionTextSize = BlueFont.MeasureString(column.Caption.Replace("\r", "\r\n"), _columnFont.Font());
            return column.TmpCaptionTextSize;
        }

        private SizeF ColumnHead_Size(ColumnItem? column) {
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

        private ColumnItem? ColumnOnCoordinate(int xpos) {
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
            if (Database.IsParsing) { return false; }
            try {
                // Kommt vor, dass spontan doch geparsed wird...
                if (_database.ColumnArrangements == null || _arrangementNr >= _database.ColumnArrangements.Count) { return false; }
                foreach (var thisViewItem in CurrentArrangement) {
                    if (thisViewItem != null) {
                        thisViewItem.OrderTmpSpalteX1 = null;
                    }
                }
                //foreach (var ThisRowItem in _Database.Row) {
                //    if (ThisRowItem != null) { ThisRowItem.TMP_Y = null; }
                //}
                _wiederHolungsSpaltenWidth = 0;
                _mouseOverText = string.Empty;
                var wdh = true;
                var maxX = 0;
                var displayR = DisplayRectangleWithoutSlider();
                // Spalten berechnen
                foreach (var thisViewItem in CurrentArrangement) {
                    if (thisViewItem?.Column != null) {
                        if (thisViewItem.ViewType != enViewType.PermanentColumn) { wdh = false; }
                        if (wdh) {
                            thisViewItem.OrderTmpSpalteX1 = maxX;
                            maxX += Column_DrawWidth(thisViewItem, displayR);
                            _wiederHolungsSpaltenWidth = Math.Max(_wiederHolungsSpaltenWidth, maxX);
                        } else {
                            thisViewItem.OrderTmpSpalteX1 = SliderX.Visible ? (int)(maxX - SliderX.Value) : 0;
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
        private void Cursor_Move(enDirection richtung) {
            if (_database == null) { return; }
            Neighbour(_cursorPosColumn, _cursorPosRow, richtung, out var newCol, out var newRow);
            CursorPos_Set(newCol, newRow, richtung != enDirection.Nichts);
        }

        private void CursorPos_Set(ColumnItem? column, RowItem? row, bool ensureVisible, string chapter) => CursorPos_Set(column, SortedRows().Get(row, chapter), ensureVisible);

        private Rectangle DisplayRectangleWithoutSlider() => _design == enBlueTableAppearance.Standard
? new Rectangle(DisplayRectangle.Left, DisplayRectangle.Left, DisplayRectangle.Width - SliderY.Width, DisplayRectangle.Height - SliderX.Height)
: new Rectangle(DisplayRectangle.Left, DisplayRectangle.Left, DisplayRectangle.Width - SliderY.Width, DisplayRectangle.Height);

        private void Draw_Border(Graphics gr, ColumnViewItem column, Rectangle displayRectangleWoSlider, bool onlyhead) {
            var yPos = displayRectangleWoSlider.Height;
            if (onlyhead) { yPos = HeadSize(); }
            for (var z = 0; z <= 1; z++) {
                int xPos;
                enColumnLineStyle lin;
                if (z == 0) {
                    xPos = (int)column.OrderTmpSpalteX1;
                    lin = column.Column.LineLeft;
                } else {
                    xPos = (int)column.OrderTmpSpalteX1 + Column_DrawWidth(column, displayRectangleWoSlider);
                    lin = column.Column.LineRight;
                }
                switch (lin) {
                    case enColumnLineStyle.Ohne:
                        break;

                    case enColumnLineStyle.Dünn:
                        gr.DrawLine(Skin.PenLinieDünn, xPos, 0, xPos, yPos);
                        break;

                    case enColumnLineStyle.Kräftig:
                        gr.DrawLine(Skin.PenLinieKräftig, xPos, 0, xPos, yPos);
                        break;

                    case enColumnLineStyle.Dick:
                        gr.DrawLine(Skin.PenLinieDick, xPos, 0, xPos, yPos);
                        break;

                    case enColumnLineStyle.ShadowRight:
                        var c = Skin.Color_Border(enDesign.Table_Lines_thick, enStates.Standard);
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

        private void Draw_CellAsButton(Graphics gr, ColumnViewItem cellInThisDatabaseColumn, RowData cellInThisDatabaseRow, Rectangle cellrectangle) {
            ButtonCellEventArgs e = new(cellInThisDatabaseColumn.Column, cellInThisDatabaseRow.Row);
            OnNeedButtonArgs(e);

            var s = enStates.Standard;
            if (!Enabled) { s = enStates.Standard_Disabled; }
            if (e.Cecked) { s |= enStates.Checked; }
            Button.DrawButton(this, gr, enDesign.Button_CheckBox, s, e.Image, enAlignment.Horizontal_Vertical_Center, false, null, e.Text, cellrectangle, true);
        }

        /// <summary>
        /// Zeichnet die gesamte Zelle mit Listbox-Hintergrund und prüft noch, ob der verlinkte Inhalt gezeichnet werden soll.
        /// </summary>
        /// <param name="gr"></param>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <param name="displayRectangleWoSlider"></param>
        /// <param name="design"></param>
        /// <param name="state"></param>
        private void Draw_CellListBox(Graphics gr, ColumnViewItem column, RowData row, Rectangle cellrectangle, Rectangle displayRectangleWoSlider, enDesign design, enStates state) {
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
        private void Draw_CellTransparent(Graphics gr, ColumnViewItem cellInThisDatabaseColumn, RowData cellInThisDatabaseRow, Rectangle cellrectangle, BlueFont? font, enStates state) {
            if (cellInThisDatabaseRow == null) { return; }

            if (cellInThisDatabaseColumn.Column.Format is enDataFormat.Verknüpfung_zu_anderer_Datenbank_Skriptgesteuert or enDataFormat.Verknüpfung_zu_anderer_Datenbank) {
                var (lcolumn, lrow, _) = CellCollection.LinkedCellData(cellInThisDatabaseColumn.Column, cellInThisDatabaseRow.Row, false, false);

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
        private void Draw_CellTransparentDirect(Graphics gr, ColumnViewItem cellInThisDatabaseColumn, RowData cellInThisDatabaseRow, Rectangle cellrectangle, ColumnItem? contentHolderCellColumn, RowItem? contentHolderCellRow, BlueFont? font, enStates state) {
            if (cellInThisDatabaseColumn.Column.Format == enDataFormat.Button) {
                Draw_CellAsButton(gr, cellInThisDatabaseColumn, cellInThisDatabaseRow, cellrectangle);
                return;
            }

            var toDraw = contentHolderCellRow.CellGetString(contentHolderCellColumn);

            Draw_CellTransparentDirect(gr, toDraw, cellrectangle, font, contentHolderCellColumn, _pix16, enShortenStyle.Replaced, contentHolderCellColumn.BildTextVerhalten, state);
        }

        private void Draw_Column_Body(Graphics gr, ColumnViewItem cellInThisDatabaseColumn, Rectangle displayRectangleWoSlider) {
            gr.SmoothingMode = SmoothingMode.None;
            gr.FillRectangle(new SolidBrush(cellInThisDatabaseColumn.Column.BackColor), (int)cellInThisDatabaseColumn.OrderTmpSpalteX1, HeadSize(), Column_DrawWidth(cellInThisDatabaseColumn, displayRectangleWoSlider), displayRectangleWoSlider.Height);
            Draw_Border(gr, cellInThisDatabaseColumn, displayRectangleWoSlider, false);
        }

        private void Draw_Column_Cells(Graphics gr, List<RowData?> sr, ColumnViewItem viewItem, Rectangle displayRectangleWoSlider, int firstVisibleRow, int lastVisibleRow, enStates state, bool firstOnScreen) {

            #region Neue Zeile

            if (UserEdit_NewRowAllowed() && viewItem == CurrentArrangement[Database.Column[0]]) {
                Skin.Draw_FormatedText(gr, "[Neue Zeile]", QuickImage.Get(enImageCode.PlusZeichen, _pix16), enAlignment.Left, new Rectangle((int)viewItem.OrderTmpSpalteX1 + 1, (int)(-SliderY.Value + HeadSize() + 1), (int)viewItem.TmpDrawWidth - 2, 16 - 2), this, false, _newRowFont, Translate);
            }

            #endregion

            #region Zeilen Zeichnen (Alle Zellen)

            for (var zei = firstVisibleRow; zei <= lastVisibleRow; zei++) {
                var currentRow = sr[zei];
                gr.SmoothingMode = SmoothingMode.None;

                Rectangle cellrectangle = new((int)viewItem.OrderTmpSpalteX1,
                                       DrawY(currentRow),
                                       Column_DrawWidth(viewItem, displayRectangleWoSlider),
                                       Math.Max(currentRow.DrawHeight, _pix16));

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
                            Rectangle tmpr = new((int)viewItem.OrderTmpSpalteX1 + 1, DrawY(currentRow) + 1, Column_DrawWidth(viewItem, displayRectangleWoSlider) - 2, currentRow.DrawHeight - 2);
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
                        gr.FillRectangle(new SolidBrush(Skin.Color_Back(enDesign.Table_And_Pad, enStates.Standard).SetAlpha(50)), 1, DrawY(currentRow) - RowCaptionSizeY, displayRectangleWoSlider.Width - 2, RowCaptionSizeY);
                        currentRow.CaptionPos = new Rectangle(1, DrawY(currentRow) - _rowCaptionFontY, (int)si.Width + 28, (int)si.Height);
                        if (_collapsed.Contains(currentRow.Chapter)) {
                            Button.DrawButton(this, gr, enDesign.Button_CheckBox, enStates.Checked, null, enAlignment.Horizontal_Vertical_Center, false, null, string.Empty, currentRow.CaptionPos, false);
                            gr.DrawImage(QuickImage.Get("Pfeil_Unten_Scrollbar|14|||FF0000||200|200"), 5, DrawY(currentRow) - _rowCaptionFontY + 6);
                        } else {
                            Button.DrawButton(this, gr, enDesign.Button_CheckBox, enStates.Standard, null, enAlignment.Horizontal_Vertical_Center, false, null, string.Empty, currentRow.CaptionPos, false);
                            gr.DrawImage(QuickImage.Get("Pfeil_Rechts_Scrollbar|14|||||0"), 5, DrawY(currentRow) - _rowCaptionFontY + 6);
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
            var bvi = new ColumnViewItem[3];
            var lcbvi = new ColumnViewItem[3];
            ColumnViewItem lastViewItem = null;
            var permaX = 0;
            var ca = CurrentArrangement;
            for (var x = 0; x < ca.Count + 1; x++) {
                var viewItem = x < ca.Count ? ca[x] : null;
                if (viewItem?.ViewType == enViewType.PermanentColumn) {
                    permaX = Math.Max(permaX, (int)viewItem.OrderTmpSpalteX1 + (int)viewItem.TmpDrawWidth);
                }
                if (viewItem == null ||
                    viewItem.ViewType == enViewType.PermanentColumn ||
                     (int)viewItem.OrderTmpSpalteX1 + (int)viewItem.TmpDrawWidth > permaX) {
                    for (var u = 0; u < 3; u++) {
                        var n = viewItem?.Column.Ueberschrift(u);
                        var v = bvi[u]?.Column.Ueberschrift(u);
                        if (n != v) {
                            if (!string.IsNullOrEmpty(v) && lastViewItem != null) {
                                var le = Math.Max(0, (int)bvi[u].OrderTmpSpalteX1);
                                //int RE = (int)LastViewItem.OrderTMP_Spalte_X1 - 1 ;
                                var re = (int)lastViewItem.OrderTmpSpalteX1 + (int)lastViewItem.TmpDrawWidth - 1;
                                if (viewItem?.ViewType != enViewType.PermanentColumn && bvi[u].ViewType != enViewType.PermanentColumn) { le = Math.Max(le, permaX); }
                                if (viewItem?.ViewType != enViewType.PermanentColumn && bvi[u].ViewType == enViewType.PermanentColumn) { re = Math.Max(re, (int)lcbvi[u].OrderTmpSpalteX1 + (int)lcbvi[u].TmpDrawWidth); }
                                if (le < re) {
                                    Rectangle r = new(le, u * ColumnCaptionSizeY, re - le, ColumnCaptionSizeY);
                                    gr.FillRectangle(new SolidBrush(bvi[u].Column.BackColor), r);
                                    gr.FillRectangle(new SolidBrush(Color.FromArgb(80, 200, 200, 200)), r);
                                    gr.DrawRectangle(Skin.PenLinieKräftig, r);
                                    Skin.Draw_FormatedText(gr, v, null, enAlignment.Horizontal_Vertical_Center, r, this, false, _columnFont, Translate);
                                }
                            }
                            bvi[u] = viewItem;
                            if (viewItem?.ViewType == enViewType.PermanentColumn) { lcbvi[u] = viewItem; }
                        }
                    }
                    lastViewItem = viewItem;
                }
            }
        }

        private void Draw_Cursor(Graphics gr, Rectangle displayRectangleWoSlider, bool onlyCursorLines) {
            if (_tmpCursorRect.Width < 1) { return; }

            var stat = enStates.Standard;
            if (Focused()) { stat = enStates.Standard_HasFocus; }

            if (onlyCursorLines) {
                Pen pen = new(Skin.Color_Border(enDesign.Table_Cursor, stat).SetAlpha(180));
                gr.DrawRectangle(pen, new Rectangle(-1, _tmpCursorRect.Top - 1, displayRectangleWoSlider.Width + 2, _tmpCursorRect.Height + 1));
            } else {
                Skin.Draw_Back(gr, enDesign.Table_Cursor, stat, _tmpCursorRect, this, false);
                Skin.Draw_Border(gr, enDesign.Table_Cursor, stat, _tmpCursorRect);
            }
        }

        private void Draw_Table_ListboxStyle(Graphics gr, List<RowData?> sr, enStates state, Rectangle displayRectangleWoSlider, int vFirstVisibleRow, int vLastVisibleRow) {
            var itStat = state;
            Skin.Draw_Back(gr, enDesign.ListBox, state, DisplayRectangle, this, true);
            var col = Database.Column[0];
            // Zeilen Zeichnen (Alle Zellen)
            for (var zeiv = vFirstVisibleRow; zeiv <= vLastVisibleRow; zeiv++) {
                var currentRow = sr[zeiv];
                var viewItem = _database.ColumnArrangements[0][col];
                Rectangle r = new(0, DrawY(currentRow), DisplayRectangleWithoutSlider().Width, currentRow.DrawHeight);
                if (_cursorPosColumn != null && _cursorPosRow.Row == currentRow.Row) {
                    itStat |= enStates.Checked;
                } else {
                    if (Convert.ToBoolean(itStat & enStates.Checked)) {
                        itStat ^= enStates.Checked;
                    }
                }

                Rectangle cellrectangle = new(0,
                       DrawY(currentRow),
                        displayRectangleWoSlider.Width,
                        Math.Min(currentRow.DrawHeight, 24));

                Draw_CellListBox(gr, viewItem, currentRow, cellrectangle, r, enDesign.Item_Listbox, itStat);
                if (!currentRow.Row.CellGetBoolean(_database.Column.SysCorrect)) {
                    gr.DrawImage(QuickImage.Get("Warnung|16||||||120||50"), new Point(r.Right - 19, (int)(r.Top + ((r.Height - 16) / 2.0))));
                }
                if (currentRow.ShowCap) {
                    BlueFont.DrawString(gr, currentRow.Chapter, _chapterFont.Font(), _chapterFont.Brush_Color_Main, 0, DrawY(currentRow) - _rowCaptionFontY);
                }
            }
            Skin.Draw_Border(gr, enDesign.ListBox, state, displayRectangleWoSlider);
        }

        private void Draw_Table_Std(Graphics gr, List<RowData?> sr, enStates state, Rectangle displayRectangleWoSlider, int firstVisibleRow, int lastVisibleRow) {
            try {
                if (_database.ColumnArrangements == null || _arrangementNr >= _database.ColumnArrangements.Count) { return; }   // Kommt vor, dass spontan doch geparsed wird...
                Skin.Draw_Back(gr, enDesign.Table_And_Pad, state, DisplayRectangle, this, true);
                /// Maximale Rechten Pixel der Permanenten Columns ermitteln
                var permaX = 0;
                foreach (var viewItem in CurrentArrangement) {
                    if (viewItem?.Column != null && viewItem.ViewType == enViewType.PermanentColumn) {
                        if (viewItem.TmpDrawWidth == null) {
                            // Veränderte Werte!
                            DrawControl(gr, state);
                            return;
                        }
                        permaX = Math.Max(permaX, (int)viewItem.OrderTmpSpalteX1 + (int)viewItem.TmpDrawWidth);
                    }
                }
                Draw_Table_What(gr, sr, enTableDrawColumn.NonPermament, enTableDrawType.ColumnBackBody, permaX, displayRectangleWoSlider, firstVisibleRow, lastVisibleRow, state);
                Draw_Table_What(gr, sr, enTableDrawColumn.NonPermament, enTableDrawType.Cells, permaX, displayRectangleWoSlider, firstVisibleRow, lastVisibleRow, state);
                Draw_Table_What(gr, sr, enTableDrawColumn.Permament, enTableDrawType.ColumnBackBody, permaX, displayRectangleWoSlider, firstVisibleRow, lastVisibleRow, state);
                Draw_Table_What(gr, sr, enTableDrawColumn.Permament, enTableDrawType.Cells, permaX, displayRectangleWoSlider, firstVisibleRow, lastVisibleRow, state);
                // Den CursorLines zeichnen
                Draw_Cursor(gr, displayRectangleWoSlider, true);
                Draw_Table_What(gr, sr, enTableDrawColumn.NonPermament, enTableDrawType.ColumnHead, permaX, displayRectangleWoSlider, firstVisibleRow, lastVisibleRow, state);
                Draw_Table_What(gr, sr, enTableDrawColumn.Permament, enTableDrawType.ColumnHead, permaX, displayRectangleWoSlider, firstVisibleRow, lastVisibleRow, state);

                /// Überschriften 1-3 Zeichnen
                Draw_Column_Head_Captions(gr);
                Skin.Draw_Border(gr, enDesign.Table_And_Pad, state, displayRectangleWoSlider);

                if (Database.ReloadNeeded) { gr.DrawImage(QuickImage.Get(enImageCode.Uhr, 16), 8, 8); }
                if (Database.HasPendingChanges()) { gr.DrawImage(QuickImage.Get(enImageCode.Stift, 16), 16, 8); }
                if (Database.ReadOnly) { gr.DrawImage(QuickImage.Get(enImageCode.Schloss, 32), 16, 8); }
            } catch {
                Invalidate();
                //Develop.DebugPrint(ex);
            }
        }

        private void Draw_Table_What(Graphics gr, List<RowData?> sr, enTableDrawColumn col, enTableDrawType type, int permaX, Rectangle displayRectangleWoSlider, int firstVisibleRow, int lastVisibleRow, enStates state) {
            var lfdno = 0;

            var firstOnScreen = true;

            foreach (var viewItem in CurrentArrangement.Where(viewItem => viewItem?.Column != null)) {
                lfdno++;
                if (IsOnScreen(viewItem, displayRectangleWoSlider)) {
                    if ((col == enTableDrawColumn.NonPermament && viewItem.ViewType != enViewType.PermanentColumn && (int)viewItem.OrderTmpSpalteX1 + (int)viewItem.TmpDrawWidth > permaX) ||
                        (col == enTableDrawColumn.Permament && viewItem.ViewType == enViewType.PermanentColumn)) {
                        switch (type) {
                            case enTableDrawType.ColumnBackBody:
                                Draw_Column_Body(gr, viewItem, displayRectangleWoSlider);
                                break;

                            case enTableDrawType.Cells:
                                Draw_Column_Cells(gr, sr, viewItem, displayRectangleWoSlider, firstVisibleRow, lastVisibleRow, state, firstOnScreen);
                                break;

                            case enTableDrawType.ColumnHead:
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

            Skin.Draw_Back(gr, enDesign.Table_And_Pad, enStates.Standard_Disabled, DisplayRectangle, this, true);

            var i = QuickImage.Get(enImageCode.Uhr, 64);
            gr.DrawImage(i, (Width - 64) / 2, (Height - 64) / 2);
            Skin.Draw_Border(gr, enDesign.Table_And_Pad, enStates.Standard_Disabled, DisplayRectangle);
        }

        private int DrawY(RowData? r) => r == null ? 0 : r.Y + HeadSize() - (int)SliderY.Value;

        private void DropDownMenu_ItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
            FloatingForm.Close(this);
            if (string.IsNullOrEmpty(e.ClickedComand)) { return; }

            CellExtEventArgs ck = null;
            if (e.HotItem is CellExtEventArgs tmp) { ck = tmp; }

            var toAdd = e.ClickedComand;
            var toRemove = string.Empty;
            if (toAdd == "#Erweitert") {
                Cell_Edit(ck.Column, ck.Row, false);
                return;
            }
            if (ck.Row == null) {
                // Neue Zeile!
                UserEdited(this, toAdd, ck.Column, null, string.Empty, false);
                return;
            }

            if (ck.Column.MultiLine) {
                var li = ck.Row.Row.CellGetList(ck.Column);
                if (li.Contains(toAdd, false)) {
                    // Ist das angeklickte Element schon vorhanden, dann soll es wohl abgewählt (gelöscht) werden.
                    if (li.Count > -1 || ck.Column.DropdownAllesAbwählenErlaubt) {
                        toRemove = toAdd;
                        toAdd = string.Empty;
                    }
                }
                if (!string.IsNullOrEmpty(toRemove)) { li.RemoveString(toRemove, false); }
                if (!string.IsNullOrEmpty(toAdd)) { li.Add(toAdd); }
                UserEdited(this, li.JoinWithCr(), ck.Column, ck.Row.Row, ck.Row.Chapter, false);
            } else {
                if (ck.Column.DropdownAllesAbwählenErlaubt) {
                    if (toAdd == ck.Row.Row.CellGetString(ck.Column)) {
                        UserEdited(this, string.Empty, ck.Column, ck.Row.Row, ck.Row.Chapter, false);
                        return;
                    }
                }
                UserEdited(this, toAdd, ck.Column, ck.Row.Row, ck.Row.Chapter, false);
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

        private bool EnsureVisible(ColumnViewItem viewItem) {
            if (viewItem?.Column == null) { return false; }
            if (viewItem.OrderTmpSpalteX1 == null && !ComputeAllColumnPositions()) { return false; }
            var r = DisplayRectangleWithoutSlider();
            ComputeAllColumnPositions();
            if (viewItem.ViewType == enViewType.PermanentColumn) {
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
            if (_design == enBlueTableAppearance.OnlyMainColumnWithoutHead || CurrentArrangement.Count - 1 < 0) {
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

        private void Invalidate_DrawWidth(ColumnItem? vcolumn) => CurrentArrangement[vcolumn]?.Invalidate_DrawWidth();

        private void Invalidate_FilteredRows() {
            _filteredRows = null;
            //CursorPos_Reset(); // Gibt Probleme bei Formularen, wenn die Key-Spalte geändert wird. Mal abgesehen davon macht es einen Sinn, den Cursor proforma zu löschen, dass soll der RowSorter übernehmen.
            Invalidate_Filterinfo();
            Invalidate_SortedRowData();
        }

        private void Invalidate_Filterinfo() {
            if (_database == null) { return; }

            foreach (var thisColumn in _database.Column.Where(thisColumn => thisColumn != null)) {
                thisColumn.TmpIfFilterRemoved = null;
                thisColumn.TmpAutoFilterSinnvoll = null;
            }
        }

        private void Invalidate_SortedRowData() {
            _sortedRowData = null;
            Invalidate();
        }

        private bool IsOnScreen(ColumnViewItem viewItem, Rectangle displayRectangleWoSlider) {
            if (viewItem == null) { return false; }
            if (_design is enBlueTableAppearance.Standard or enBlueTableAppearance.OnlyMainColumnWithoutHead) {
                if (viewItem.OrderTmpSpalteX1 + Column_DrawWidth(viewItem, displayRectangleWoSlider) >= 0 && viewItem.OrderTmpSpalteX1 <= displayRectangleWoSlider.Width) { return true; }
            } else {
                Develop.DebugPrint(_design);
            }
            return false;
        }

        private bool IsOnScreen(ColumnItem? column, RowData? row, Rectangle displayRectangleWoSlider) => IsOnScreen(CurrentArrangement[column], displayRectangleWoSlider) && IsOnScreen(row, displayRectangleWoSlider);

        private bool IsOnScreen(RowData? vrow, Rectangle displayRectangleWoSlider) => vrow != null && DrawY(vrow) + vrow.DrawHeight >= HeadSize() && DrawY(vrow) <= displayRectangleWoSlider.Height;

        /// <summary>
        /// Berechnet die Zelle, ausgehend von einer Zellenposition. Dabei wird die Columns und Zeilensortierung berücksichtigt.
        /// Gibt des keine Nachbarszelle, wird die Eingangszelle zurückgegeben.
        /// </summary>
        /// <remarks></remarks>
        private void Neighbour(ColumnItem? column, RowData? row, enDirection direction, out ColumnItem? newColumn, out RowData? newRow) {
            newColumn = column;
            newRow = row;
            if (_design == enBlueTableAppearance.OnlyMainColumnWithoutHead) {
                if (direction is not enDirection.Oben and not enDirection.Unten) {
                    newColumn = _database.Column[0];
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

        private void OnAutoFilterClicked(FilterEventArgs e) => AutoFilterClicked?.Invoke(this, e);

        private void OnButtonCellClicked(CellEventArgs e) => ButtonCellClicked?.Invoke(this, e);

        private void OnCellValueChanged(CellEventArgs e) => CellValueChanged?.Invoke(this, e);

        private void OnCellValueChangingByUser(CellValueChangingByUserEventArgs ed) => CellValueChangingByUser?.Invoke(this, ed);

        private void OnColumnArrangementChanged() => ColumnArrangementChanged?.Invoke(this, System.EventArgs.Empty);

        private void OnCursorPosChanged(CellExtEventArgs e) => CursorPosChanged?.Invoke(this, e);

        private void OnDatabaseChanged() => DatabaseChanged?.Invoke(this, System.EventArgs.Empty);

        private void OnEditBeforeBeginEdit(CellCancelEventArgs e) => EditBeforeBeginEdit?.Invoke(this, e);

        private void OnEditBeforeNewRow(RowCancelEventArgs e) => EditBeforeNewRow?.Invoke(this, e);

        private void OnFilterChanged() => FilterChanged?.Invoke(this, System.EventArgs.Empty);

        private void OnNeedButtonArgs(ButtonCellEventArgs e) => NeedButtonArgs?.Invoke(this, e);

        private void OnPinnedChanged() => PinnedChanged?.Invoke(this, System.EventArgs.Empty);

        private void OnRowAdded(object sender, RowEventArgs e) => RowAdded?.Invoke(sender, e);

        private void OnViewChanged() {
            ViewChanged?.Invoke(this, System.EventArgs.Empty);
            Invalidate_SortedRowData(); // evtl. muss [Neue Zeile] ein/ausgebelndet werden
        }

        private void OnVisibleRowsChanged() => VisibleRowsChanged?.Invoke(this, System.EventArgs.Empty);

        private int Row_DrawHeight(RowItem? vrow, Rectangle displayRectangleWoSlider) {
            if (_design == enBlueTableAppearance.OnlyMainColumnWithoutHead) { return Cell_ContentSize(this, _database.Column[0], vrow, _cellFont, _pix16).Height; }
            var tmp = _pix18;
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

        private RowData? RowOnCoordinate(int pixelY) {
            if (_database == null || pixelY <= HeadSize()) { return null; }
            var s = SortedRows();
            if (s == null) { return null; }

            return s.FirstOrDefault(thisRowItem => thisRowItem != null && pixelY >= DrawY(thisRowItem) && pixelY <= DrawY(thisRowItem) + thisRowItem.DrawHeight && thisRowItem.Expanded);
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
                Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
                Invalidate();
            }
        }

        private void SliderY_ValueChanged(object sender, System.EventArgs e) {
            if (_database == null) { return; }
            lock (_lockUserAction) {
                Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
                Invalidate();
            }
        }

        private RowSortDefinition? SortUsed() => _sortDefinitionTemporary?.Columns != null
                ? _sortDefinitionTemporary
                : _database.SortDefinition?.Columns != null ? _database.SortDefinition : null;

        private void TXTBox_Close(TextBox btBxx) {
            if (btBxx == null) { return; }
            if (!btBxx.Visible) { return; }
            if (btBxx.Tag == null || string.IsNullOrEmpty(btBxx.Tag.ToString())) {
                btBxx.Visible = false;
                return; // Ohne Dem hier wird ganz am Anfang ein Ereignis ausgelöst
            }
            var w = btBxx.Text;
            var tmpTag = (string)btBxx.Tag;
            Database.Cell.DataOfCellKey(tmpTag, out var column, out var row);
            btBxx.Tag = null;
            btBxx.Visible = false;
            UserEdited(this, w, column, row, _cursorPosRow?.Chapter, true);
            Focus();
        }

        private bool UserEdit_NewRowAllowed() {
            if (_database == null || _database.Column.Count == 0 || _database.Column[0] == null) { return false; }
            if (_design == enBlueTableAppearance.OnlyMainColumnWithoutHead) { return false; }
            if (_database.ColumnArrangements.Count == 0) { return false; }
            if (CurrentArrangement?[_database.Column[0]] == null) { return false; }
            if (!_database.PermissionCheck(_database.PermissionGroupsNewRow, null)) { return false; }

            if (PowerEdit.Subtract(DateTime.Now).TotalSeconds > 0) { return true; }

            return CellCollection.UserEditPossible(_database.Column[0], null, enErrorReason.EditNormaly);
        }

        #endregion

        // QickInfo beisst sich mit den letzten Änderungen Quickinfo//DialogBoxes.QuickInfo.Show("<IMAGECODE=Stift|16||1> " + Reason);
    }
}