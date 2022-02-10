// Authors:
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
using System.Drawing.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static BlueBasics.Converter;
using static BlueBasics.FileOperations;
using static BlueBasics.ListOfExtension;

namespace BlueControls.Controls {

    [Designer(typeof(BasicDesigner))]
    [DefaultEvent("CursorPosChanged")]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class Table : GenericControl, IContextMenu, IBackgroundNone, ITranslateable {

        #region Fields

        public static SolidBrush Brush_Yellow_Transparent = new(Color.FromArgb(180, 255, 255, 0));

        public static Pen Pen_Red_1 = new(Color.Red, 1);

        private const int _AutoFilterSize = 22;

        private const int ColumnCaptionSizeY = 22;

        private const int RowCaptionSizeY = 50;
        private static bool ServiceStarted = false;
        private readonly List<string> _collapsed = new();

        // Die Sortierung der Zeile
        private readonly object Lock_UserAction = new();

        private int _ArrangementNr = 1;
        private AutoFilter _AutoFilter;
        private BlueFont _Cell_Font;
        private BlueFont _Chapter_Font;
        private BlueFont _Column_Filter_Font;
        private BlueFont _Column_Font;
        private ColumnItem _CursorPosColumn;
        private RowData _CursorPosRow;
        private Database _Database;
        private enBlueTableAppearance _Design = enBlueTableAppearance.Standard;

        // Die Sortierung der Zeile
        private List<RowItem> _FilteredRows;

        private int? _HeadSize = null;

        /// <summary>
        ///  Wird DatabaseAdded gehandlet?
        /// </summary>
        private ColumnItem _MouseOverColumn;

        private RowData _MouseOverRow;
        private string _MouseOverText;
        private BlueFont _NewRow_Font;
        private SearchAndReplace _searchAndReplace;

        //private readonly FontSelectDialog _FDia = null;
        private bool _ShowNumber = false;

        private RowSortDefinition _sortDefinitionTemporary;
        private List<RowData> _SortedRowData;
        private string _StoredView = string.Empty;
        private RowItem _Unterschiede;
        private int _WiederHolungsSpaltenWidth;
        private bool ISIN_Click;
        private bool ISIN_DoubleClick;
        private bool ISIN_KeyDown;
        private bool ISIN_MouseDown;
        private bool ISIN_MouseEnter;
        private bool ISIN_MouseLeave;
        private bool ISIN_MouseMove;
        private bool ISIN_MouseUp;
        private bool ISIN_MouseWheel;
        private bool ISIN_SizeChanged;

        //private bool ISIN_Resize;
        private bool ISIN_VisibleChanged;

        private Progressbar PG = null;

        // Die Sortierung der Zeile
        private int Pix16 = 16;

        private int Pix18 = 18;
        private int RowCaptionFontY = 26;
        private Rectangle tmpCursorRect = Rectangle.Empty;

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
            get => _ArrangementNr;
            set {
                if (value != _ArrangementNr) {
                    _ArrangementNr = value;
                    Invalidate_HeadSize();
                    CurrentArrangement?.Invalidate_DrawWithOfAllItems();
                    Invalidate();
                    OnViewChanged();
                    CursorPos_Set(_CursorPosColumn, _CursorPosRow, true);
                }
            }
        }

        public ColumnViewCollection CurrentArrangement => _Database == null || _Database.ColumnArrangements == null || _Database.ColumnArrangements.Count <= _ArrangementNr
                    ? null
                    : _Database.ColumnArrangements[_ArrangementNr];

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
                PinnedRows.Clear();
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
                    //_Database.RowKeyChanged -= _Database_RowKeyChanged;
                    //_Database.ColumnKeyChanged -= _Database_ColumnKeyChanged;
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
                    //_Database.RowKeyChanged += _Database_RowKeyChanged;
                    //_Database.ColumnKeyChanged += _Database_ColumnKeyChanged;
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
                CurrentArrangement?.Invalidate_DrawWithOfAllItems();
                Invalidate_FilteredRows();
                OnViewChanged();
            }
        }

        public FilterCollection Filter { get; private set; }

        [DefaultValue(1.0f)]
        public double FontScale => _Database == null ? 1f : _Database.GlobalScale;

        public List<RowItem> PinnedRows { get; } = new();

        public DateTime PowerEdit {
            private get => _Database.PowerEdit;
            set {
                Database.PowerEdit = value;
                Invalidate_SortedRowData(); // Neue Zeilen können nun erlaubt sein
            }
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
            get => _Unterschiede;
            set {
                //if (_Unterschiede != null && value != null && _sortDefinitionTemporary.ToString() == value.ToString()) { return; }
                if (_Unterschiede == value) { return; }
                _Unterschiede = value;
                Invalidate();
            }
        }

        #endregion

        #region Methods

        public static Size Cell_ContentSize(Table table, ColumnItem Column, RowItem Row, BlueFont CellFont, int Pix16) {
            if (Column.Format == enDataFormat.LinkedCell) {
                (var lcolumn, var lrow, _) = CellCollection.LinkedCellData(Column, Row, false, false);
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

        public static void CopyToClipboard(ColumnItem Column, RowItem Row, bool Meldung) {
            try {
                if (Row != null && Column.Format.CanBeCheckedByRules()) {
                    var c = Row.CellGetString(Column);
                    c = c.Replace("\r\n", "\r");
                    c = c.Replace("\r", "\r\n");
                    Generic.CopytoClipboard(c);
                    if (Meldung) { Notification.Show(LanguageTool.DoTranslate("<b>{0}</b><br>ist nun in der Zwischenablage.", true, c), enImageCode.Kopieren); }
                } else {
                    if (Meldung) { Notification.Show(LanguageTool.DoTranslate("Bei dieser Spalte nicht möglich."), enImageCode.Warnung); }
                }
            } catch {
                if (Meldung) { Notification.Show(LanguageTool.DoTranslate("Unerwarteter Fehler beim Kopieren."), enImageCode.Warnung); }
            }
        }

        public static void Database_NeedPassword(object sender, PasswordEventArgs e) {
            if (e.Handled) { return; }
            e.Handled = true;
            e.Password = InputBox.Show("Bitte geben sie das Passwort ein,<br>um Zugriff auf diese Datenbank<br>zu erhalten:", string.Empty, enVarType.Text);
        }

        public static void DoUndo(ColumnItem column, RowItem row) {
            if (column == null) { return; }
            if (row == null) { return; }
            if (column.Format == enDataFormat.LinkedCell) {
                (var lcolumn, var lrow, _) = CellCollection.LinkedCellData(column, row, true, false);
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

        /// <summary>
        /// Status des Bildes (Disabled) wird geändert. Diese Routine sollte nicht innerhalb der Table Klasse aufgerufen werden.
        /// Sie dient nur dazu, das Aussehen eines Textes wie eine Zelle zu imitieren.
        /// </summary>
        public static void Draw_FormatedText(Graphics gr, string text, ColumnItem column, Rectangle fitInRect, enDesign design, enStates state, enShortenStyle style, enBildTextVerhalten bildTextverhalten) {
            if (string.IsNullOrEmpty(text)) { return; }
            var d = Skin.DesignOf(design, state);

            Draw_CellTransparentDirect(gr, text, fitInRect, d.bFont, column, 16, style, bildTextverhalten, state);
        }

        public static List<string> FileSystem(ColumnItem _tmpColumn) {
            if (_tmpColumn == null) { return null; }

            var f = GetFilesWithFileSelector(_tmpColumn.Database.Filename.FilePath(), _tmpColumn.MultiLine);

            if (f == null) { return null; }

            List<string> DelList = new();
            List<string> NewFiles = new();

            foreach (var thisf in f) {
                var b = FileToByte(thisf);

                if (!string.IsNullOrEmpty(_tmpColumn.Database.FileEncryptionKey)) { b = Cryptography.SimpleCrypt(b, _tmpColumn.Database.FileEncryptionKey, 1); }

                var neu = thisf.FileNameWithSuffix();
                neu = _tmpColumn.BestFile(neu.FileNameWithSuffix(), true);
                ByteToFile(neu, b);

                NewFiles.Add(neu);
                DelList.Add(thisf);
            }

            FileDialogs.DeleteFile(DelList, true);
            return NewFiles;
        }

        public static Size FormatedText_NeededSize(ColumnItem column, string originalText, BlueFont font, enShortenStyle style, int minSize, enBildTextVerhalten bildTextverhalten) {
            var tmpData = CellItem.GetDrawingData(column, originalText, style, bildTextverhalten);
            return Skin.FormatedText_NeededSize(tmpData.Item1, tmpData.Item2, font, minSize);
        }

        public static void ImportCSV(Database _Database, string csvtxt) {
            using Import x = new(_Database, csvtxt);
            x.ShowDialog();
        }

        public static void SearchNextText(string searchTXT, Table TableView, ColumnItem column, RowData row, out ColumnItem foundColumn, out RowData foundRow, bool VereinfachteSuche) {
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
                var ContenHolderCellRow = row?.Row;
                if (column.Format == enDataFormat.LinkedCell) {
                    (ContentHolderCellColumn, ContenHolderCellRow, _) = CellCollection.LinkedCellData(column, row?.Row, false, false);
                }
                var _Ist1 = string.Empty;
                var _Ist2 = string.Empty;
                if (ContenHolderCellRow != null && ContentHolderCellColumn != null) {
                    _Ist1 = ContenHolderCellRow.CellGetString(ContentHolderCellColumn);
                    _Ist2 = CellItem.ValuesReadable(ContentHolderCellColumn, ContenHolderCellRow, enShortenStyle.Both).JoinWithCr();
                }
                if (ContentHolderCellColumn != null && ContentHolderCellColumn.FormatierungErlaubt) {
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

        public static int tmpColumnContentWidth(Table table, ColumnItem Column, BlueFont CellFont, int Pix16) {
            if (Column.TMP_ColumnContentWidth != null) { return (int)Column.TMP_ColumnContentWidth; }
            Column.TMP_ColumnContentWidth = 0;
            if (Column.Format == enDataFormat.Button) {
                // Beim Button reicht eine Abfrage mit Row null
                Column.TMP_ColumnContentWidth = Cell_ContentSize(table, Column, null, CellFont, Pix16).Width;
            } else {
                Parallel.ForEach(Column.Database.Row, ThisRowItem => {
                    if (ThisRowItem != null && !ThisRowItem.CellIsNullOrEmpty(Column)) {
                        var t = Column.TMP_ColumnContentWidth; // ja, dank Multithreading kann es sein, dass hier das hier null ist
                        if (t == null) { t = 0; }
                        Column.TMP_ColumnContentWidth = Math.Max((int)t, Cell_ContentSize(table, Column, ThisRowItem, CellFont, Pix16).Width);
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
            return Column.TMP_ColumnContentWidth is int w ? w : 0;
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

        public static void WriteColumnArrangementsInto(ComboBox columnArrangementSelector, Database _Database, int showingNo) {
            //if (InvokeRequired) {
            //    Invoke(new Action(() => WriteColumnArrangementsInto(columnArrangementSelector)));
            //    return;
            //}
            if (columnArrangementSelector != null) {
                columnArrangementSelector.Item.Clear();
                columnArrangementSelector.Enabled = false;
                columnArrangementSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            }

            if (_Database == null || columnArrangementSelector == null) {
                if (columnArrangementSelector != null) {
                    columnArrangementSelector.Enabled = false;
                    columnArrangementSelector.Text = string.Empty;
                }
                return;
            }

            foreach (var ThisArrangement in _Database.ColumnArrangements) {
                if (ThisArrangement != null) {
                    if (columnArrangementSelector != null && _Database.PermissionCheck(ThisArrangement.PermissionGroups_Show, null)) {
                        columnArrangementSelector.Item.Add(ThisArrangement.Name, _Database.ColumnArrangements.IndexOf(ThisArrangement).ToString());
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

        public void CursorPos_Set(ColumnItem column, RowData row, bool ensureVisible) {
            if (_Database == null || _Database.ColumnArrangements.Count == 0 || CurrentArrangement[column] == null || SortedRows() == null || !SortedRows().Contains(row)) {
                column = null;
                row = null;
            }

            if (_CursorPosColumn == column && _CursorPosRow == row) { return; }
            _MouseOverText = string.Empty;
            _CursorPosColumn = column;
            _CursorPosRow = row;
            if (ensureVisible) { EnsureVisible(_CursorPosColumn, _CursorPosRow); }
            Invalidate();
            OnCursorPosChanged(new CellExtEventArgs(_CursorPosColumn, _CursorPosRow));
        }

        public ColumnItem CursorPosColumn() => _CursorPosColumn;

        public RowData CursorPosRow() => _CursorPosRow;

        public bool EnsureVisible(ColumnItem Column, RowData Row) {
            var ok1 = EnsureVisible(CurrentArrangement?[Column]);
            var ok2 = EnsureVisible(Row);
            return ok1 && ok2;
        }

        public void ExpandAll() {
            _collapsed.Clear();
            CursorPos_Reset(); // Wenn eine Zeile markiert ist, man scrollt und expandiert, springt der Screen zurück, was sehr irriteiert
            Invalidate_SortedRowData();
        }

        public string Export_CSV(enFirstRow FirstRow) => _Database == null ? string.Empty : _Database.Export_CSV(FirstRow, CurrentArrangement, SortedRows());

        public string Export_CSV(enFirstRow FirstRow, ColumnItem OnlyColumn) {
            if (_Database == null) { return string.Empty; }
            List<ColumnItem> l = new() { OnlyColumn };
            return _Database.Export_CSV(FirstRow, l, SortedRows());
        }

        public void Export_HTML(string Filename = "", bool Execute = true) {
            if (_Database == null) { return; }
            if (string.IsNullOrEmpty(Filename)) { Filename = TempFile("", "", "html"); }
            _Database.Export_HTML(Filename, CurrentArrangement, SortedRows(), Execute);
        }

        /// <summary>
        /// Alle gefilteren Zeilen. Jede Zeile ist maximal einmal in dieser Liste vorhanden. Angepinnte Zeilen addiert worden
        /// </summary>
        /// <returns></returns>
        public List<RowItem> FilteredRows() {
            if (_FilteredRows != null) { return _FilteredRows; }
            _FilteredRows = Database.Row.CalculateFilteredRows(Filter);
            return _FilteredRows;
        }

        public new void Focus() {
            if (Focused()) { return; }
            base.Focus();
        }

        public new bool Focused() => base.Focused || SliderY.Focused() || SliderX.Focused() || BTB.Focused() || BCB.Focused();

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

        public void ImportClipboard() {
            Develop.DebugPrint_InvokeRequired(InvokeRequired, false);
            if (!System.Windows.Forms.Clipboard.ContainsText()) {
                Notification.Show("Abbruch,<br>kein Text im Zwischenspeicher!", enImageCode.Information);
                return;
            }

            var nt = System.Windows.Forms.Clipboard.GetText();
            ImportCSV(nt);
        }

        public void ImportCSV(string csvtxt) => ImportCSV(_Database, csvtxt);

        public void Invalidate_AllColumnArrangements() {
            if (_Database == null) { return; }

            foreach (var ThisArrangement in _Database.ColumnArrangements) {
                if (ThisArrangement != null) {
                    ThisArrangement.Invalidate_DrawWithOfAllItems();
                }
            }
        }

        public void Invalidate_HeadSize() {
            if (_HeadSize != null) { Invalidate(); }
            _HeadSize = null;
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

        public void ParseView(string ToParse) {
            if (string.IsNullOrEmpty(ToParse)) { return; }
            PinnedRows.Clear();
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
                        CursorPos_Set(column, SortedRows().Get(row), false);
                        break;

                    case "tempsort":
                        _sortDefinitionTemporary = new RowSortDefinition(_Database, pair.Value);
                        break;

                    case "pin":
                        PinnedRows.Add(_Database.Row.SearchByKey(long.Parse(pair.Value)));
                        break;

                    case "collapsed":
                        _collapsed.Add(pair.Value.FromNonCritical());
                        break;

                    case "reduced":
                        var c = _Database.Column.SearchByKey(long.Parse(pair.Value));
                        var cv = CurrentArrangement[c];
                        if (cv != null) { cv._TMP_Reduced = true; }
                        break;

                    default:
                        Develop.DebugPrint(enFehlerArt.Warnung, "Tag unbekannt: " + pair.Key);
                        break;
                }
            }
            Filter.OnChanged();
            Invalidate_FilteredRows(); // beim Parsen wirft der Filter kein Event ab
        }

        public void Pin(List<RowItem> rows) {
            // Arbeitet mit Rows, weil nur eine Anpinngug möglich ist

            if (rows == null) { rows = new List<RowItem>(); }

            rows = rows.Distinct().ToList();
            if (!rows.IsDifferentTo(PinnedRows)) { return; }

            PinnedRows.Clear();
            PinnedRows.AddRange(rows);
            Invalidate_SortedRowData();
            OnPinnedChanged();
        }

        public void PinAdd(RowItem row) {
            PinnedRows.Add(row);
            Invalidate_SortedRowData();
            OnPinnedChanged();
        }

        public void PinRemove(RowItem row) {
            PinnedRows.Remove(row);
            Invalidate_SortedRowData();
            OnPinnedChanged();
        }

        public List<RowData> SortedRows() {
            if (_SortedRowData != null) { return _SortedRowData; }

            try {
                var DisplayR = DisplayRectangleWithoutSlider();
                var MaxY = 0;
                if (UserEdit_NewRowAllowed()) { MaxY += Pix18; }
                var expanded = true;
                var LastCap = string.Empty;

                List<RowData> _SortedRowDataNew;
                if (Database == null) {
                    _SortedRowDataNew = new List<RowData>();
                } else {
                    _SortedRowDataNew = Database.Row.CalculateSortedRows(FilteredRows(), SortUsed(), PinnedRows, _SortedRowData);
                }

                if (!_SortedRowData.IsDifferentTo(_SortedRowDataNew)) { return _SortedRowData; }

                _SortedRowData = new List<RowData>();

                foreach (var thisRow in _SortedRowDataNew) {
                    var ThisRowData = thisRow;
                    if (_MouseOverRow != null && _MouseOverRow.Row == thisRow.Row && _MouseOverRow.Chapter == thisRow.Chapter) {
                        _MouseOverRow.GetDataFrom(ThisRowData);
                        ThisRowData = _MouseOverRow;
                    } // Mouse-Daten wiederverwenden
                    if (_CursorPosRow != null && _CursorPosRow.Row == thisRow.Row && _CursorPosRow.Chapter == thisRow.Chapter) {
                        _CursorPosRow.GetDataFrom(ThisRowData);
                        ThisRowData = _CursorPosRow;
                    } // Cursor-Daten wiederverwenden

                    ThisRowData.Y = MaxY;

                    #region Caption bestimmen

                    if (thisRow.Chapter != LastCap) {
                        ThisRowData.Y += RowCaptionSizeY;
                        expanded = !_collapsed.Contains(ThisRowData.Chapter);
                        MaxY += RowCaptionSizeY;
                        ThisRowData.ShowCap = true;
                        LastCap = thisRow.Chapter;
                    } else {
                        ThisRowData.ShowCap = false;
                    }

                    #endregion

                    #region Expaned (oder pinned) bestimmen

                    ThisRowData.Expanded = expanded;
                    if (ThisRowData.Expanded) {
                        ThisRowData.DrawHeight = Row_DrawHeight(ThisRowData.Row, DisplayR);
                        if (_SortedRowData == null) {
                            // Folgender Fall: Die Row height reparirt die LinkedCell.
                            // Dadruch wird eine Zelle geändert. Das wiederrum kann _SortedRowData auf null setzen..
                            return null;
                        }
                        MaxY += ThisRowData.DrawHeight;
                    }

                    #endregion

                    _SortedRowData.Add(ThisRowData);
                }

                if (_CursorPosRow != null && _CursorPosRow.Row != null && !_SortedRowData.Contains(_CursorPosRow)) { CursorPos_Reset(); }
                _MouseOverRow = null;

                #region Slider berechnen

                SliderSchaltenSenk(DisplayR, MaxY);

                #endregion

                EnsureVisible(_CursorPosColumn, _CursorPosRow);
                OnVisibleRowsChanged();

                return SortedRows(); // Rekursiver aufruf. Manchmal funktiniert OnRowsSorted nicht...
            } catch {
                // Komisch, manchmal wird die Variable _SortedRowData verworfen.
                Invalidate_SortedRowData();
                return SortedRows();
            }
        }

        public RowData View_NextRow(RowData row) {
            if (_Database == null) { return null; }
            var RowNr = SortedRows().IndexOf(row);
            return RowNr < 0 || RowNr >= SortedRows().Count - 1 ? null : SortedRows()[RowNr + 1];
        }

        public RowData View_PreviousRow(RowData row) {
            if (_Database == null) { return null; }
            var RowNr = SortedRows().IndexOf(row);
            return RowNr < 1 ? null : SortedRows()[RowNr - 1];
        }

        public RowData View_RowFirst() => _Database == null ? null : SortedRows().Count == 0 ? null : SortedRows()[0];

        public RowData View_RowLast() => _Database == null ? null : SortedRows().Count == 0 ? null : SortedRows()[SortedRows().Count - 1];

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
            if (PinnedRows != null && PinnedRows.Count > 0) {
                foreach (var thisRow in PinnedRows) {
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

        public List<RowItem> VisibleUniqueRows() {
            var l = new List<RowItem>();
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
            if (ServiceStarted) { return; }
            ServiceStarted = true;
            BlueBasics.MultiUserFile.clsMultiUserFile.AllFiles.ItemAdded += AllFiles_ItemAdded;
            BlueBasics.MultiUserFile.clsMultiUserFile.AllFiles.ItemRemoving += AllFiles_ItemRemoving;
            Database.DropConstructorMessage += Database_DropConstructorMessage;
        }

        internal bool NonPermanentPossible(ColumnViewItem ThisViewItem) {
            if (_ArrangementNr < 1) {
                return !ThisViewItem.Column.IsFirst();
            }
            var NX = ThisViewItem.NextVisible(CurrentArrangement);
            return NX == null || Convert.ToBoolean(NX.ViewType != enViewType.PermanentColumn);
        }

        internal bool PermanentPossible(ColumnViewItem ThisViewItem) {
            if (_ArrangementNr < 1) {
                return ThisViewItem.Column.IsFirst();
            }
            var prev = ThisViewItem.PreviewsVisible(CurrentArrangement);
            return prev == null || Convert.ToBoolean(prev.ViewType == enViewType.PermanentColumn);
        }

        protected override void DrawControl(Graphics gr, enStates state) {
            if (InvokeRequired) {
                Invoke(new Action(() => DrawControl(gr, state)));
                return;
            }

            tmpCursorRect = Rectangle.Empty;

            // Listboxen bekommen keinen Focus, also Tabellen auch nicht. Basta.
            if (Convert.ToBoolean(state & enStates.Standard_HasFocus)) {
                state ^= enStates.Standard_HasFocus;
            }

            if (_Database == null || DesignMode || ShowWaitScreen) {
                DrawWaitScreen(gr);
                return;
            }

            lock (Lock_UserAction) {
                //if (_InvalidExternal) { FillExternalControls(); }
                if (Convert.ToBoolean(state & enStates.Standard_Disabled)) { CursorPos_Reset(); }
                var displayRectangleWOSlider = DisplayRectangleWithoutSlider();
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

                var FirstVisibleRow = sr.Count;
                var LastVisibleRow = -1;
                foreach (var thisRow in sr) {
                    if (IsOnScreen(thisRow, displayRectangleWOSlider)) {
                        var T = sr.IndexOf(thisRow);
                        FirstVisibleRow = Math.Min(T, FirstVisibleRow);
                        LastVisibleRow = Math.Max(T, LastVisibleRow);
                    }
                }
                switch (_Design) {
                    case enBlueTableAppearance.Standard:
                        Draw_Table_Std(gr, sr, state, displayRectangleWOSlider, FirstVisibleRow, LastVisibleRow);
                        break;

                    case enBlueTableAppearance.OnlyMainColumnWithoutHead:
                        Draw_Table_ListboxStyle(gr, sr, state, displayRectangleWOSlider, FirstVisibleRow, LastVisibleRow);
                        break;

                    default:
                        Develop.DebugPrint(_Design);
                        break;
                }
            }
        }

        protected void InitializeSkin() {
            _Cell_Font = Skin.GetBlueFont(enDesign.Table_Cell, enStates.Standard).Scale(FontScale);
            _Column_Font = Skin.GetBlueFont(enDesign.Table_Column, enStates.Standard).Scale(FontScale);
            _Chapter_Font = Skin.GetBlueFont(enDesign.Table_Cell_Chapter, enStates.Standard).Scale(FontScale);
            _Column_Filter_Font = BlueFont.Get(_Column_Font.FontName, _Column_Font.FontSize, false, false, false, false, true, Color.White, Color.Red, false, false, false);

            _NewRow_Font = Skin.GetBlueFont(enDesign.Table_Cell_New, enStates.Standard).Scale(FontScale);
            if (Database != null) {
                Pix16 = GetPix(16, _Cell_Font, Database.GlobalScale);
                Pix18 = GetPix(18, _Cell_Font, Database.GlobalScale);
                RowCaptionFontY = GetPix(26, _Cell_Font, Database.GlobalScale);
            } else {
                Pix16 = 16;
                Pix18 = 18;
                RowCaptionFontY = 26;
            }
        }

        protected override bool IsInputKey(System.Windows.Forms.Keys keyData) =>
            // Ganz wichtig diese Routine!
            // Wenn diese NICHT ist, geht der Fokus weg, sobald der cursor gedrückt wird.
            keyData switch {
                System.Windows.Forms.Keys.Up or System.Windows.Forms.Keys.Down or System.Windows.Forms.Keys.Left or System.Windows.Forms.Keys.Right => true,
                _ => false,
            };

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

        protected override void OnDoubleClick(System.EventArgs e) {
            //    base.OnDoubleClick(e); Wird komplett selbst gehandlet und das neue Ereigniss ausgelöst
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
                        if (Database.PowerEdit.Subtract(DateTime.Now).TotalSeconds > 0) { ea.StartEdit = true; }
                        if (ea.StartEdit) { Cell_Edit(_MouseOverColumn, _MouseOverRow, true); }
                    }
                }
                ISIN_DoubleClick = false;
            }
        }

        protected override void OnKeyDown(System.Windows.Forms.KeyEventArgs e) {
            base.OnKeyDown(e);

            if (_Database == null) { return; }

            lock (Lock_UserAction) {
                if (ISIN_KeyDown) { return; }
                ISIN_KeyDown = true;

                _Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());

                switch (e.KeyCode) {
                    case System.Windows.Forms.Keys.Oemcomma: // normales ,
                        if (e.Modifiers == System.Windows.Forms.Keys.Control) {
                            var lp = CellCollection.ErrorReason(_CursorPosColumn, _CursorPosRow?.Row, enErrorReason.EditGeneral);
                            Neighbour(_CursorPosColumn, _CursorPosRow, enDirection.Oben, out var _newCol, out var _newRow);
                            if (_newRow == _CursorPosRow) { lp = "Das geht nicht bei dieser Zeile."; }
                            if (string.IsNullOrEmpty(lp) && _newRow != null) {
                                UserEdited(this, _newRow?.Row.CellGetString(_CursorPosColumn), _CursorPosColumn, _CursorPosRow?.Row, _newRow?.Chapter, true);
                            } else {
                                NotEditableInfo(lp);
                            }
                        }
                        break;

                    case System.Windows.Forms.Keys.X:
                        if (e.Modifiers == System.Windows.Forms.Keys.Control) {
                            CopyToClipboard(_CursorPosColumn, _CursorPosRow?.Row, true);
                            if (_CursorPosRow is null || _CursorPosRow.Row.CellIsNullOrEmpty(_CursorPosColumn)) {
                                ISIN_KeyDown = false;
                                return;
                            }
                            var l2 = CellCollection.ErrorReason(_CursorPosColumn, _CursorPosRow?.Row, enErrorReason.EditGeneral);
                            if (string.IsNullOrEmpty(l2)) {
                                UserEdited(this, string.Empty, _CursorPosColumn, _CursorPosRow?.Row, _CursorPosRow?.Chapter, true);
                            } else {
                                NotEditableInfo(l2);
                            }
                        }
                        break;

                    case System.Windows.Forms.Keys.Delete:
                        if (_CursorPosRow is null || _CursorPosRow.Row.CellIsNullOrEmpty(_CursorPosColumn)) {
                            ISIN_KeyDown = false;
                            return;
                        }
                        var l = CellCollection.ErrorReason(_CursorPosColumn, _CursorPosRow?.Row, enErrorReason.EditGeneral);
                        if (string.IsNullOrEmpty(l)) {
                            UserEdited(this, string.Empty, _CursorPosColumn, _CursorPosRow?.Row, _CursorPosRow?.Chapter, true);
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
                            CopyToClipboard(_CursorPosColumn, _CursorPosRow?.Row, true);
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
                                if (!System.Windows.Forms.Clipboard.ContainsText()) {
                                    NotEditableInfo("Kein Text in der Zwischenablage.");
                                    ISIN_KeyDown = false;
                                    return;
                                }
                                var ntxt = System.Windows.Forms.Clipboard.GetText();
                                if (_CursorPosRow?.Row.CellGetString(_CursorPosColumn) == ntxt) {
                                    ISIN_KeyDown = false;
                                    return;
                                }
                                var l2 = CellCollection.ErrorReason(_CursorPosColumn, _CursorPosRow?.Row, enErrorReason.EditGeneral);
                                if (string.IsNullOrEmpty(l2)) {
                                    UserEdited(this, ntxt, _CursorPosColumn, _CursorPosRow?.Row, _CursorPosRow?.Chapter, true);
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
                    CursorPos_Reset(); // Wenn eine Zeile markiert ist, man scrollt und expandiert, springt der Screen zurück, was sehr irriteiert
                    _MouseOverColumn = null;
                    _MouseOverRow = null;

                    if (!string.IsNullOrEmpty(rc)) {
                        if (_collapsed.Contains(rc)) {
                            _collapsed.Remove(rc);
                        } else {
                            _collapsed.Add(rc);
                        }
                        Invalidate_SortedRowData();
                    }
                }
                EnsureVisible(_MouseOverColumn, _MouseOverRow);
                CursorPos_Set(_MouseOverColumn, _MouseOverRow, false);
                ISIN_MouseDown = false;
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
                                        (var lcolumn, var lrow, var info) = CellCollection.LinkedCellData(_MouseOverColumn, _MouseOverRow.Row, false, false);
                                        if (lcolumn != null) { _MouseOverText = lcolumn.QuickInfoText(_MouseOverColumn.ReadableText() + " bei " + lcolumn.ReadableText() + ":"); }

                                        if (!string.IsNullOrEmpty(info) && _MouseOverColumn.Database.IsAdministrator()) {
                                            if (string.IsNullOrEmpty(_MouseOverText)) { _MouseOverText += "\r\n"; }
                                            _MouseOverText = "Verlinkungs-Status: " + info;
                                        }

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
                            OnButtonCellClicked(new CellEventArgs(_MouseOverColumn, _MouseOverRow?.Row));
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

        protected override void OnSizeChanged(System.EventArgs e) {
            base.OnSizeChanged(e);
            if (_Database == null) { return; }
            lock (Lock_UserAction) {
                if (ISIN_SizeChanged) { return; }
                ISIN_SizeChanged = true;
                Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
                CurrentArrangement?.Invalidate_DrawWithOfAllItems();
                ISIN_SizeChanged = false;
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
            if (_Database == null) { return; }
            lock (Lock_UserAction) {
                if (ISIN_VisibleChanged) { return; }
                ISIN_VisibleChanged = true;
                Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
                ISIN_VisibleChanged = false;
            }
        }

        private static void AllFiles_ItemAdded(object sender, ListEventArgs e) {
            if (e.Item is Database DB) {
                DB.NeedPassword += Database_NeedPassword;
                DB.GenerateLayoutInternal += DB_GenerateLayoutInternal;
                DB.Loaded += tabAdministration.CheckDatabase;
            }
        }

        private static void AllFiles_ItemRemoving(object sender, ListEventArgs e) {
            if (e.Item is Database DB) {
                DB.NeedPassword -= Database_NeedPassword;
                DB.GenerateLayoutInternal -= DB_GenerateLayoutInternal;
                DB.Loaded -= tabAdministration.CheckDatabase;
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
            ItemCollectionPad Pad = new(e.LayoutID, e.Row.Database, e.Row.Key);
            Pad.SaveAsBitmap(e.Filename);
        }

        private static void Draw_CellTransparentDirect(Graphics gr, string toDraw, Rectangle drawarea, BlueFont font, ColumnItem contentHolderCellColumn, int Pix16, enShortenStyle style, enBildTextVerhalten bildTextverhalten, enStates state) {
            if (toDraw == null) { toDraw = string.Empty; }

            if (!contentHolderCellColumn.MultiLine || !toDraw.Contains("\r")) {
                Draw_CellTransparentDirect_OneLine(gr, toDraw, contentHolderCellColumn, drawarea, 0, false, font, Pix16, style, bildTextverhalten, state);
            } else {
                var MEI = toDraw.SplitAndCutByCR();
                if (contentHolderCellColumn.ShowMultiLineInOneLine) {
                    Draw_CellTransparentDirect_OneLine(gr, MEI.JoinWith("; "), contentHolderCellColumn, drawarea, 0, false, font, Pix16, style, bildTextverhalten, state);
                } else {
                    var y = 0;
                    for (var z = 0; z <= MEI.GetUpperBound(0); z++) {
                        Draw_CellTransparentDirect_OneLine(gr, MEI[z], contentHolderCellColumn, drawarea, y, z != MEI.GetUpperBound(0), font, Pix16, style, bildTextverhalten, state);
                        y += FormatedText_NeededSize(contentHolderCellColumn, MEI[z], font, style, Pix16 - 1, bildTextverhalten).Height;
                    }
                }
            }
        }

        private static void Draw_CellTransparentDirect_OneLine(Graphics gr, string drawString, ColumnItem contentHolderColumnStyle, Rectangle drawarea, int txtYPix, bool changeToDot, BlueFont font, int Pix16, enShortenStyle style, enBildTextVerhalten bildTextverhalten, enStates state) {
            Rectangle r = new(drawarea.Left, drawarea.Top + txtYPix, drawarea.Width, Pix16);

            if (r.Bottom + Pix16 > drawarea.Bottom) {
                if (r.Bottom > drawarea.Bottom) { return; }
                if (changeToDot) { drawString = "..."; }// Die letzte Zeile noch ganz hinschreiben
            }

            var tmpData = CellItem.GetDrawingData(contentHolderColumnStyle, drawString, style, bildTextverhalten);
            var tmpImageCode = tmpData.Item2;
            if (tmpImageCode != null) { tmpImageCode = QuickImage.Get(tmpImageCode, Skin.AdditionalState(state)); }

            Skin.Draw_FormatedText(gr, tmpData.Item1, tmpImageCode, (enAlignment)contentHolderColumnStyle.Align, r, null, false, font, false);
        }

        private static int GetPix(int Pix, BlueFont F, double Scale) => Skin.FormatedText_NeededSize("@|", null, F, (int)((Pix * Scale) + 0.5)).Height;

        private static void UserEdited(Table table, string newValue, ColumnItem column, RowItem row, string chapter, bool formatWarnung) {
            if (column == null) {
                table.NotEditableInfo("Keine Spalte angegeben.");
                return;
            }

            if (column.Format == enDataFormat.LinkedCell) {
                (var lcolumn, var lrow, var info) = CellCollection.LinkedCellData(column, row, true, false);
                if (lcolumn == null || lrow == null) {
                    table.NotEditableInfo("Zelle in verlinkter Datenbank nicht vorhanden:\r\n" + info);
                    return;
                }
                UserEdited(table, newValue, lcolumn, lrow, chapter, formatWarnung);
                if (table.Database == column.Database) { table.CursorPos_Set(column, row, false, chapter); }
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

            CellValueChangingByUserEventArgs ed = new(column, row, newValue, string.Empty);
            table.OnCellValueChangingByUser(ed);
            var CancelReason = ed.CancelReason;
            if (string.IsNullOrEmpty(CancelReason) && formatWarnung && !string.IsNullOrEmpty(newValue)) {
                if (!newValue.IsFormat(column)) {
                    if (MessageBox.Show("Ihre Eingabe entspricht<br><u>nicht</u> dem erwarteten Format!<br><br>Trotzdem übernehmen?", enImageCode.Information, "Ja", "Nein") != 0) {
                        CancelReason = "Abbruch, das das erwartete Format nicht eingehalten wurde.";
                    }
                }
            }

            if (string.IsNullOrEmpty(CancelReason)) {
                if (row == null) {
                    var f = CellCollection.ErrorReason(column.Database.Column[0], null, enErrorReason.EditGeneral);
                    if (!string.IsNullOrEmpty(f)) { table.NotEditableInfo(f); return; }
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
                    if (!string.IsNullOrEmpty(f)) { table.NotEditableInfo(f); return; }
                    row.CellSet(column, newValue);
                }
                if (table.Database == column.Database) { table.CursorPos_Set(column, row, false, chapter); }
                row.DoAutomatic(true, false, 5, "value changed");

                // EnsureVisible ganz schlecht: Daten verändert, keine Positionen bekannt - und da soll sichtbar gemacht werden?
                // CursorPos.EnsureVisible(SliderX, SliderY, DisplayRectangle)
            } else {
                table.NotEditableInfo(CancelReason);
            }
        }

        private void _Database_CellValueChanged(object sender, CellEventArgs e) {
            if (_FilteredRows != null) {
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
            Invalidate_AllColumnArrangements();
            Invalidate_HeadSize();
            Invalidate_FilteredRows();
            OnViewChanged();
            if (e.OnlyReload) {
                if (string.IsNullOrEmpty(_StoredView)) { Develop.DebugPrint("Stored View Empty!"); }
                ParseView(_StoredView);
                _StoredView = string.Empty;
            }
        }

        private void _Database_Disposing(object sender, System.EventArgs e) => Database = null;

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
            _StoredView = ViewToString();

        private void _Database_ViewChanged(object sender, System.EventArgs e) {
            InitializeSkin(); // Sicher ist sicher, um die neuen Schrift-Größen zu haben.

            Invalidate_HeadSize();
            Invalidate_AllColumnArrangements();
            //Invalidate_RowSort();
            CursorPos_Set(_CursorPosColumn, _CursorPosRow, true);
            Invalidate();
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
                    e.Column.GetUniques(VisibleUniqueRows(), out var Einzigartig, out _);
                    if (Einzigartig.Count > 0) {
                        Filter.Add(new FilterItem(e.Column, enFilterType.Istgleich_ODER_GroßKleinEgal, Einzigartig));
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
                        foreach (var ThisColumnItem in e.Column.Database.Column) {
                            if (ThisColumnItem != null && ThisColumnItem != e.Column) { ic.Add(ThisColumnItem); }
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
                        var ClipTMP = System.Windows.Forms.Clipboard.GetText();
                        ClipTMP = ClipTMP.RemoveChars(Constants.Char_NotFromClip);
                        ClipTMP = ClipTMP.TrimEnd("\r\n");
                        var SearchValue = new List<string>(ClipTMP.SplitAndCutByCR()).SortedDistinctList();
                        Filter.Remove(e.Column);
                        if (SearchValue.Count > 0) {
                            Filter.Add(new FilterItem(e.Column, enFilterType.IstGleich_ODER | enFilterType.GroßKleinEgal, SearchValue));
                        }
                        break;
                    }

                case "donotclipboard": {
                        var ClipTMP = System.Windows.Forms.Clipboard.GetText();
                        ClipTMP = ClipTMP.RemoveChars(Constants.Char_NotFromClip);
                        ClipTMP = ClipTMP.TrimEnd("\r\n");
                        var ColumInhalt = Database.Export_CSV(enFirstRow.Without, e.Column, null).SplitAndCutByCRToList().SortedDistinctList();
                        ColumInhalt.RemoveString(ClipTMP.SplitAndCutByCRToList().SortedDistinctList(), false);
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

        private bool Autofilter_Sinnvoll(ColumnItem column) {
            if (column.TMP_AutoFilterSinnvoll != null) { return (bool)column.TMP_AutoFilterSinnvoll; }
            for (var rowcount = 0; rowcount <= SortedRows().Count - 2; rowcount++) {
                if (SortedRows()[rowcount]?.Row.CellGetString(column) != SortedRows()[rowcount + 1]?.Row.CellGetString(column)) {
                    column.TMP_AutoFilterSinnvoll = true;
                    return true;
                }
            }
            column.TMP_AutoFilterSinnvoll = false;
            return false;
        }

        /// <summary>
        /// Gibt die Anzahl der SICHTBAREN Zeilen zurück, die mehr angezeigt werden würden, wenn dieser Filter deaktiviert wäre.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        private int Autofilter_Text(ColumnItem column) {
            if (column.TMP_IfFilterRemoved != null) { return (int)column.TMP_IfFilterRemoved; }
            FilterCollection tfilter = new(Database);
            foreach (var ThisFilter in Filter) {
                if (ThisFilter != null && ThisFilter.Column != column) { tfilter.Add(ThisFilter); }
            }
            var temp = Database.Row.CalculateFilteredRows(tfilter);
            column.TMP_IfFilterRemoved = FilteredRows().Count - temp.Count;
            return (int)column.TMP_IfFilterRemoved;
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

        private void Cell_Edit(ColumnItem cellInThisDatabaseColumn, RowData cellInThisDatabaseRow, bool WithDropDown) {
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
                string info;
                (ContentHolderCellColumn, ContentHolderCellRow, info) = CellCollection.LinkedCellData(cellInThisDatabaseColumn, cellInThisDatabaseRow?.Row, true, true);
                if (ContentHolderCellColumn == null || ContentHolderCellRow == null) {
                    NotEditableInfo("In verknüpfter Datenbank nicht vorhanden:\r\n" + info);
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

        private void Cell_Edit_Color(ColumnItem cellInThisDatabaseColumn, RowData cellInThisDatabaseRow) {
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
            UserEdited(this, Color.FromArgb(255, ColDia.Color).ToArgb().ToString(), cellInThisDatabaseColumn, cellInThisDatabaseRow?.Row, cellInThisDatabaseRow?.Chapter, false);
        }

        private void Cell_Edit_Dropdown(ColumnItem cellInThisDatabaseColumn, RowData cellInThisDatabaseRow, ColumnItem ContentHolderCellColumn, RowItem ContentHolderCellRow) {
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

            var _DropDownMenu = FloatingInputBoxListBoxStyle.Show(t, new CellExtEventArgs(cellInThisDatabaseColumn, cellInThisDatabaseRow), this, Translate);
            _DropDownMenu.ItemClicked += DropDownMenu_ItemClicked;
            Develop.Debugprint_BackgroundThread();
        }

        private void Cell_Edit_FileSystem(ColumnItem cellInThisDatabaseColumn, RowData cellInThisDatabaseRow) {
            var l = FileSystem(cellInThisDatabaseColumn);
            if (l == null) { return; }
            UserEdited(this, l.JoinWithCr(), cellInThisDatabaseColumn, cellInThisDatabaseRow?.Row, cellInThisDatabaseRow?.Chapter, false);
        }

        private bool Cell_Edit_TextBox(ColumnItem cellInThisDatabaseColumn, RowData cellInThisDatabaseRow, ColumnItem ContentHolderCellColumn, RowItem ContentHolderCellRow, TextBox Box, int AddWith, int IsHeight) {
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

            Box.GetStyleFrom(ContentHolderCellColumn);

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

        private void CellOnCoordinate(int Xpos, int Ypos, out ColumnItem Column, out RowData Row) {
            Column = ColumnOnCoordinate(Xpos);
            Row = RowOnCoordinate(Ypos);
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

        private int Column_DrawWidth(ColumnViewItem ViewItem, Rectangle displayRectangleWOSlider) {
            // Hier wird die ORIGINAL-Spalte gezeichnet, nicht die FremdZelle!!!!

            if (ViewItem._TMP_DrawWidth != null) { return (int)ViewItem._TMP_DrawWidth; }
            if (ViewItem == null || ViewItem.Column == null) { return 0; }

            if (_Design == enBlueTableAppearance.OnlyMainColumnWithoutHead) {
                ViewItem._TMP_DrawWidth = displayRectangleWOSlider.Width - 1;
                return (int)ViewItem._TMP_DrawWidth;
            }

            if (ViewItem._TMP_Reduced) {
                ViewItem._TMP_DrawWidth = 16;
            } else {
                if (ViewItem.ViewType == enViewType.PermanentColumn) {
                    ViewItem._TMP_DrawWidth = Math.Min(tmpColumnContentWidth(this, ViewItem.Column, _Cell_Font, Pix16), (int)(displayRectangleWOSlider.Width * 0.3));
                } else {
                    ViewItem._TMP_DrawWidth = Math.Min(tmpColumnContentWidth(this, ViewItem.Column, _Cell_Font, Pix16), (int)(displayRectangleWOSlider.Width * 0.75));
                }
            }

            ViewItem._TMP_DrawWidth = Math.Max((int)ViewItem._TMP_DrawWidth, _AutoFilterSize); // Mindestens so groß wie der Autofilter;
            ViewItem._TMP_DrawWidth = Math.Max((int)ViewItem._TMP_DrawWidth, (int)ColumnHead_Size(ViewItem.Column).Width);
            return (int)ViewItem._TMP_DrawWidth;
        }

        private void Column_ItemRemoving(object sender, ListEventArgs e) {
            if (e.Item == _CursorPosColumn) {
                CursorPos_Reset();
            }
            if (e.Item == _MouseOverColumn) {
                _MouseOverColumn = null;
            }
        }

        private void ColumnArrangements_ItemInternalChanged(object sender, ListEventArgs e) {
            OnColumnArrangementChanged();
            Invalidate();
        }

        private SizeF ColumnCaptionText_Size(ColumnItem Column) {
            if (Column.TMP_CaptionText_Size.Width > 0) { return Column.TMP_CaptionText_Size; }
            if (_Column_Font == null) { return new SizeF(Pix16, Pix16); }
            Column.TMP_CaptionText_Size = BlueFont.MeasureString(Column.Caption.Replace("\r", "\r\n"), _Column_Font.Font());
            return Column.TMP_CaptionText_Size;
        }

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

        /// <summary>
        /// Setzt die Variable CursorPos um X Columns und Y Reihen um. Dabei wird die Columns und Zeilensortierung berücksichtigt.
        /// </summary>
        /// <remarks></remarks>
        private void Cursor_Move(enDirection Richtung) {
            if (_Database == null) { return; }
            Neighbour(_CursorPosColumn, _CursorPosRow, Richtung, out var _newCol, out var _newRow);
            CursorPos_Set(_newCol, _newRow, Richtung != enDirection.Nichts);
        }

        private void CursorPos_Set(ColumnItem column, RowItem row, bool ensureVisible, string chapter) => CursorPos_Set(column, SortedRows().Get(row, chapter), ensureVisible);

        private Rectangle DisplayRectangleWithoutSlider() => _Design == enBlueTableAppearance.Standard
? new Rectangle(DisplayRectangle.Left, DisplayRectangle.Left, DisplayRectangle.Width - SliderY.Width, DisplayRectangle.Height - SliderX.Height)
: new Rectangle(DisplayRectangle.Left, DisplayRectangle.Left, DisplayRectangle.Width - SliderY.Width, DisplayRectangle.Height);

        private void Draw_Border(Graphics gr, ColumnViewItem column, Rectangle displayRectangleWOSlider, bool onlyhead) {
            var yPos = displayRectangleWOSlider.Height;
            if (onlyhead) { yPos = HeadSize(); }
            for (var z = 0; z <= 1; z++) {
                int xPos;
                enColumnLineStyle Lin;
                if (z == 0) {
                    xPos = (int)column.OrderTMP_Spalte_X1;
                    Lin = column.Column.LineLeft;
                } else {
                    xPos = (int)column.OrderTMP_Spalte_X1 + Column_DrawWidth(column, displayRectangleWOSlider);
                    Lin = column.Column.LineRight;
                }
                switch (Lin) {
                    case enColumnLineStyle.Ohne:
                        break;

                    case enColumnLineStyle.Dünn:
                        gr.DrawLine(Skin.Pen_LinieDünn, xPos, 0, xPos, yPos);
                        break;

                    case enColumnLineStyle.Kräftig:
                        gr.DrawLine(Skin.Pen_LinieKräftig, xPos, 0, xPos, yPos);
                        break;

                    case enColumnLineStyle.Dick:
                        gr.DrawLine(Skin.Pen_LinieDick, xPos, 0, xPos, yPos);
                        break;

                    case enColumnLineStyle.ShadowRight:
                        var c = Skin.Color_Border(enDesign.Table_Lines_thick, enStates.Standard);
                        gr.DrawLine(Skin.Pen_LinieKräftig, xPos, 0, xPos, yPos);
                        gr.DrawLine(new Pen(Color.FromArgb(80, c.R, c.G, c.B)), xPos + 1, 0, xPos + 1, yPos);
                        gr.DrawLine(new Pen(Color.FromArgb(60, c.R, c.G, c.B)), xPos + 2, 0, xPos + 2, yPos);
                        gr.DrawLine(new Pen(Color.FromArgb(40, c.R, c.G, c.B)), xPos + 3, 0, xPos + 3, yPos);
                        gr.DrawLine(new Pen(Color.FromArgb(20, c.R, c.G, c.B)), xPos + 4, 0, xPos + 4, yPos);
                        break;

                    default:
                        Develop.DebugPrint(Lin);
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
        /// <param name="displayRectangleWOSlider"></param>
        /// <param name="design"></param>
        /// <param name="state"></param>
        private void Draw_CellListBox(Graphics gr, ColumnViewItem column, RowData row, Rectangle cellrectangle, Rectangle displayRectangleWOSlider, enDesign design, enStates state) {
            Skin.Draw_Back(gr, design, state, displayRectangleWOSlider, null, false);
            Skin.Draw_Border(gr, design, state, displayRectangleWOSlider);
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
        private void Draw_CellTransparent(Graphics gr, ColumnViewItem cellInThisDatabaseColumn, RowData cellInThisDatabaseRow, Rectangle cellrectangle, BlueFont font, enStates state) {
            if (cellInThisDatabaseRow == null) { return; }

            if (cellInThisDatabaseColumn.Column.Format == enDataFormat.LinkedCell) {
                (var lcolumn, var lrow, _) = CellCollection.LinkedCellData(cellInThisDatabaseColumn.Column, cellInThisDatabaseRow.Row, false, false);

                if (lcolumn != null && lrow != null) {
                    Draw_CellTransparentDirect(gr, cellInThisDatabaseColumn, cellInThisDatabaseRow, cellrectangle, lcolumn, lrow, font, state);
                } else {
                    if (cellInThisDatabaseRow.Row.Database.IsAdministrator()) {
                        gr.DrawImage(QuickImage.Get("Warnung|10||||||120||60").BMP, cellrectangle.Left + 3, cellrectangle.Top + 1);
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
        private void Draw_CellTransparentDirect(Graphics gr, ColumnViewItem cellInThisDatabaseColumn, RowData cellInThisDatabaseRow, Rectangle cellrectangle, ColumnItem contentHolderCellColumn, RowItem contentHolderCellRow, BlueFont font, enStates state) {
            if (cellInThisDatabaseColumn.Column.Format == enDataFormat.Button) {
                Draw_CellAsButton(gr, cellInThisDatabaseColumn, cellInThisDatabaseRow, cellrectangle);
                return;
            }

            var toDraw = contentHolderCellRow.CellGetString(contentHolderCellColumn);

            Table.Draw_CellTransparentDirect(gr, toDraw, cellrectangle, font, contentHolderCellColumn, Pix16, enShortenStyle.Replaced, contentHolderCellColumn.BildTextVerhalten, state);
        }

        private void Draw_Column_Body(Graphics gr, ColumnViewItem cellInThisDatabaseColumn, Rectangle displayRectangleWOSlider) {
            gr.SmoothingMode = SmoothingMode.None;
            gr.FillRectangle(new SolidBrush(cellInThisDatabaseColumn.Column.BackColor), (int)cellInThisDatabaseColumn.OrderTMP_Spalte_X1, HeadSize(), Column_DrawWidth(cellInThisDatabaseColumn, displayRectangleWOSlider), displayRectangleWOSlider.Height);
            Draw_Border(gr, cellInThisDatabaseColumn, displayRectangleWOSlider, false);
        }

        private void Draw_Column_Cells(Graphics gr, List<RowData> sr, ColumnViewItem viewItem, Rectangle displayRectangleWOSlider, int firstVisibleRow, int lastVisibleRow, enStates state, bool firstOnScreen) {

            #region Neue Zeile

            if (UserEdit_NewRowAllowed() && viewItem == CurrentArrangement[Database.Column[0]]) {
                Skin.Draw_FormatedText(gr, "[Neue Zeile]", QuickImage.Get(enImageCode.PlusZeichen, Pix16), enAlignment.Left, new Rectangle((int)viewItem.OrderTMP_Spalte_X1 + 1, (int)(-SliderY.Value + HeadSize() + 1), (int)viewItem._TMP_DrawWidth - 2, 16 - 2), this, false, _NewRow_Font, Translate);
            }

            #endregion

            #region Zeilen Zeichnen (Alle Zellen)

            for (var Zei = firstVisibleRow; Zei <= lastVisibleRow; Zei++) {
                var CurrentRow = sr[Zei];
                gr.SmoothingMode = SmoothingMode.None;

                Rectangle cellrectangle = new((int)viewItem.OrderTMP_Spalte_X1,
                                       DrawY(CurrentRow),
                                       Column_DrawWidth(viewItem, displayRectangleWOSlider),
                                       Math.Max(CurrentRow.DrawHeight, Pix16));

                if (CurrentRow.Expanded) {

                    #region Hintergrund gelb zeichnen

                    if (CurrentRow.MarkYellow) {
                        gr.FillRectangle(Brush_Yellow_Transparent, cellrectangle);
                    }

                    #endregion

                    #region Trennlinie zeichnen

                    gr.DrawLine(Skin.Pen_LinieDünn, cellrectangle.Left, cellrectangle.Bottom - 1, cellrectangle.Right - 1, cellrectangle.Bottom - 1);

                    #endregion

                    #region Die Cursorposition ermittleln und Zeichnen

                    if (!Thread.CurrentThread.IsBackground && _CursorPosColumn == viewItem.Column && _CursorPosRow == CurrentRow) {
                        tmpCursorRect = cellrectangle;
                        tmpCursorRect.Height -= 1;
                        Draw_Cursor(gr, displayRectangleWOSlider, false);
                    }

                    #endregion

                    #region Zelleninhalt zeichnen

                    Draw_CellTransparent(gr, viewItem, CurrentRow, cellrectangle, _Cell_Font, state);

                    #endregion

                    #region Unterschiede rot markieren

                    if (_Unterschiede != null && _Unterschiede != CurrentRow.Row) {
                        if (CurrentRow.Row.CellGetString(viewItem.Column) != _Unterschiede.CellGetString(viewItem.Column)) {
                            Rectangle tmpr = new((int)viewItem.OrderTMP_Spalte_X1 + 1, DrawY(CurrentRow) + 1, Column_DrawWidth(viewItem, displayRectangleWOSlider) - 2, CurrentRow.DrawHeight - 2);
                            gr.DrawRectangle(Pen_Red_1, tmpr);
                        }
                    }

                    #endregion
                }

                #region Spaltenüberschrift

                if (firstOnScreen) {
                    // Überschrift in der ersten Spalte zeichnen
                    CurrentRow.CaptionPos = Rectangle.Empty;
                    if (CurrentRow.ShowCap) {
                        var si = gr.MeasureString(CurrentRow.Chapter, _Chapter_Font.Font());
                        gr.FillRectangle(new SolidBrush(Skin.Color_Back(enDesign.Table_And_Pad, enStates.Standard).SetAlpha(50)), 1, DrawY(CurrentRow) - RowCaptionSizeY, displayRectangleWOSlider.Width - 2, RowCaptionSizeY);
                        CurrentRow.CaptionPos = new Rectangle(1, DrawY(CurrentRow) - RowCaptionFontY, (int)si.Width + 28, (int)si.Height);
                        if (_collapsed.Contains(CurrentRow.Chapter)) {
                            Button.DrawButton(this, gr, enDesign.Button_CheckBox, enStates.Checked, null, enAlignment.Horizontal_Vertical_Center, false, null, string.Empty, CurrentRow.CaptionPos, false);
                            gr.DrawImage(QuickImage.Get("Pfeil_Unten_Scrollbar|14|||FF0000||200|200").BMP, 5, DrawY(CurrentRow) - RowCaptionFontY + 6);
                        } else {
                            Button.DrawButton(this, gr, enDesign.Button_CheckBox, enStates.Standard, null, enAlignment.Horizontal_Vertical_Center, false, null, string.Empty, CurrentRow.CaptionPos, false);
                            gr.DrawImage(QuickImage.Get("Pfeil_Rechts_Scrollbar|14|||||0").BMP, 5, DrawY(CurrentRow) - RowCaptionFontY + 6);
                        }
                        _Chapter_Font.DrawString(gr, CurrentRow.Chapter, 23, DrawY(CurrentRow) - RowCaptionFontY);
                        gr.DrawLine(Skin.Pen_LinieDick, 0, DrawY(CurrentRow), displayRectangleWOSlider.Width, DrawY(CurrentRow));
                    }
                }

                #endregion
            }

            #endregion
        }

        private void Draw_Column_Head(Graphics gr, ColumnViewItem viewItem, Rectangle displayRectangleWOSlider, int lfdNo) {
            if (!IsOnScreen(viewItem, displayRectangleWOSlider)) { return; }
            if (_Design == enBlueTableAppearance.OnlyMainColumnWithoutHead) { return; }
            if (_Column_Font == null) { return; }
            gr.FillRectangle(new SolidBrush(viewItem.Column.BackColor), (int)viewItem.OrderTMP_Spalte_X1, 0, Column_DrawWidth(viewItem, displayRectangleWOSlider), HeadSize());
            Draw_Border(gr, viewItem, displayRectangleWOSlider, true);
            gr.FillRectangle(new SolidBrush(Color.FromArgb(100, 200, 200, 200)), (int)viewItem.OrderTMP_Spalte_X1, 0, Column_DrawWidth(viewItem, displayRectangleWOSlider), HeadSize());

            var Down = 0;
            if (!string.IsNullOrEmpty(viewItem.Column.Ueberschrift3)) {
                Down = ColumnCaptionSizeY * 3;
            } else if (!string.IsNullOrEmpty(viewItem.Column.Ueberschrift2)) {
                Down = ColumnCaptionSizeY * 2;
            } else if (!string.IsNullOrEmpty(viewItem.Column.Ueberschrift1)) {
                Down = ColumnCaptionSizeY;
            }

            #region Recude-Button zeichnen

            if (Column_DrawWidth(viewItem, displayRectangleWOSlider) > 70 || viewItem._TMP_Reduced) {
                viewItem._TMP_ReduceLocation = new Rectangle((int)viewItem.OrderTMP_Spalte_X1 + Column_DrawWidth(viewItem, displayRectangleWOSlider) - 18, Down, 18, 18);
                if (viewItem._TMP_Reduced) {
                    gr.DrawImage(QuickImage.Get("Pfeil_Rechts|16|||FF0000|||||20").BMP, viewItem._TMP_ReduceLocation.Left + 2, viewItem._TMP_ReduceLocation.Top + 2);
                } else {
                    gr.DrawImage(QuickImage.Get("Pfeil_Links|16||||||||75").BMP, viewItem._TMP_ReduceLocation.Left + 2, viewItem._TMP_ReduceLocation.Top + 2);
                }
            }

            #endregion Recude-Button zeichnen

            #region Filter-Knopf mit Trichter

            var TrichterText = string.Empty;
            QuickImage TrichterIcon = null;
            var TrichterState = enStates.Undefiniert;
            viewItem._TMP_AutoFilterLocation = new Rectangle((int)viewItem.OrderTMP_Spalte_X1 + Column_DrawWidth(viewItem, displayRectangleWOSlider) - _AutoFilterSize, HeadSize() - _AutoFilterSize, _AutoFilterSize, _AutoFilterSize);
            var filtIt = Filter[viewItem.Column];
            if (viewItem.Column.AutoFilterSymbolPossible()) {
                if (filtIt != null) {
                    TrichterState = enStates.Checked;
                    var anz = Autofilter_Text(viewItem.Column);
                    TrichterText = anz > -100 ? (anz * -1).ToString() : "∞";
                } else {
                    TrichterState = Autofilter_Sinnvoll(viewItem.Column) ? enStates.Standard : enStates.Standard_Disabled;
                }
            }
            var TrichterSize = (_AutoFilterSize - 4).ToString();
            if (filtIt != null) {
                TrichterIcon = QuickImage.Get("Trichter|" + TrichterSize + "|||FF0000");
            } else if (Filter.MayHasRowFilter(viewItem.Column)) {
                TrichterIcon = QuickImage.Get("Trichter|" + TrichterSize + "|||227722");
            } else if (viewItem.Column.AutoFilterSymbolPossible()) {
                TrichterIcon = Autofilter_Sinnvoll(viewItem.Column)
                    ? QuickImage.Get("Trichter|" + TrichterSize)
                    : QuickImage.Get("Trichter|" + TrichterSize + "||128");
            }
            if (TrichterState != enStates.Undefiniert) {
                Skin.Draw_Back(gr, enDesign.Button_AutoFilter, TrichterState, viewItem._TMP_AutoFilterLocation, null, false);
                Skin.Draw_Border(gr, enDesign.Button_AutoFilter, TrichterState, viewItem._TMP_AutoFilterLocation);
            }
            if (TrichterIcon != null) {
                gr.DrawImage(TrichterIcon.BMP, viewItem._TMP_AutoFilterLocation.Left + 2, viewItem._TMP_AutoFilterLocation.Top + 2);
            }
            if (!string.IsNullOrEmpty(TrichterText)) {
                var s = _Column_Filter_Font.MeasureString(TrichterText, StringFormat.GenericDefault);

                _Column_Filter_Font.DrawString(gr, TrichterText,
                              viewItem._TMP_AutoFilterLocation.Left + ((_AutoFilterSize - s.Width) / 2),
                              viewItem._TMP_AutoFilterLocation.Top + ((_AutoFilterSize - s.Height) / 2));
            }
            if (TrichterState == enStates.Undefiniert) {
                viewItem._TMP_AutoFilterLocation = new Rectangle(0, 0, 0, 0);
            }

            #endregion Filter-Knopf mit Trichter

            #region LaufendeNummer

            if (_ShowNumber) {
                for (var x = -1; x < 2; x++) {
                    for (var y = -1; y < 2; y++) {
                        BlueFont.DrawString(gr, "#" + lfdNo.ToString(), _Column_Font.Font(), Brushes.Black, (int)viewItem.OrderTMP_Spalte_X1 + x, viewItem._TMP_AutoFilterLocation.Top + y);
                    }
                }
                BlueFont.DrawString(gr, "#" + lfdNo.ToString(), _Column_Font.Font(), Brushes.White, (int)viewItem.OrderTMP_Spalte_X1, viewItem._TMP_AutoFilterLocation.Top);
            }

            #endregion LaufendeNummer

            var tx = viewItem.Column.Caption;
            tx = LanguageTool.DoTranslate(tx, Translate).Replace("\r", "\r\n");
            var FS = gr.MeasureString(tx, _Column_Font.Font());
            if (!string.IsNullOrEmpty(viewItem.Column.CaptionBitmap) && viewItem.Column.TMP_CaptionBitmap == null) {
                viewItem.Column.TMP_CaptionBitmap = QuickImage.Get(viewItem.Column.CaptionBitmap).BMP;
            }
            if (viewItem.Column.TMP_CaptionBitmap != null && viewItem.Column.TMP_CaptionBitmap.Width > 10) {
                Point pos = new((int)viewItem.OrderTMP_Spalte_X1 + (int)((Column_DrawWidth(viewItem, displayRectangleWOSlider) - FS.Width) / 2.0), 3 + Down);
                gr.DrawImageInRectAspectRatio(viewItem.Column.TMP_CaptionBitmap, (int)viewItem.OrderTMP_Spalte_X1 + 2, (int)(pos.Y + FS.Height), Column_DrawWidth(viewItem, displayRectangleWOSlider) - 4, HeadSize() - (int)(pos.Y + FS.Height) - 6 - 18);
                // Dann der Text
                gr.TranslateTransform(pos.X, pos.Y);
                //GR.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                BlueFont.DrawString(gr, tx, _Column_Font.Font(), new SolidBrush(viewItem.Column.ForeColor), 0, 0);
                gr.TranslateTransform(-pos.X, -pos.Y);
            } else {
                Point pos = new((int)viewItem.OrderTMP_Spalte_X1 + (int)((Column_DrawWidth(viewItem, displayRectangleWOSlider) - FS.Height) / 2.0), HeadSize() - 4 - _AutoFilterSize);
                gr.TranslateTransform(pos.X, pos.Y);
                gr.RotateTransform(-90);
                //GR.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                BlueFont.DrawString(gr, tx, _Column_Font.Font(), new SolidBrush(viewItem.Column.ForeColor), 0, 0);
                gr.TranslateTransform(-pos.X, -pos.Y);
                gr.ResetTransform();
            }
            // Sortierrichtung Zeichnen
            var tmpSortDefinition = SortUsed();
            if (tmpSortDefinition != null && tmpSortDefinition.UsedForRowSort(viewItem.Column) || viewItem.Column == Database.Column.SysChapter) {
                if (tmpSortDefinition.Reverse) {
                    gr.DrawImage(QuickImage.Get("ZA|11|5||||50").BMP, (float)(viewItem.OrderTMP_Spalte_X1 + (Column_DrawWidth(viewItem, displayRectangleWOSlider) / 2.0) - 6), HeadSize() - 6 - _AutoFilterSize);
                } else {
                    gr.DrawImage(QuickImage.Get("AZ|11|5||||50").BMP, (float)(viewItem.OrderTMP_Spalte_X1 + (Column_DrawWidth(viewItem, displayRectangleWOSlider) / 2.0) - 6), HeadSize() - 6 - _AutoFilterSize);
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

        private void Draw_Cursor(Graphics gr, Rectangle displayRectangleWOSlider, bool onlyCursorLines) {
            if (tmpCursorRect.Width < 1) { return; }

            var stat = enStates.Standard;
            if (Focused()) { stat = enStates.Standard_HasFocus; }

            if (onlyCursorLines) {
                Pen pen = new(Skin.Color_Border(enDesign.Table_Cursor, stat).SetAlpha(180));
                gr.DrawRectangle(pen, new Rectangle(-1, tmpCursorRect.Top - 1, displayRectangleWOSlider.Width + 2, tmpCursorRect.Height + 1));
            } else {
                Skin.Draw_Back(gr, enDesign.Table_Cursor, stat, tmpCursorRect, this, false);
                Skin.Draw_Border(gr, enDesign.Table_Cursor, stat, tmpCursorRect);
            }
        }

        private void Draw_Table_ListboxStyle(Graphics GR, List<RowData> sr, enStates state, Rectangle displayRectangleWOSlider, int vFirstVisibleRow, int vLastVisibleRow) {
            var ItStat = state;
            Skin.Draw_Back(GR, enDesign.ListBox, state, DisplayRectangle, this, true);
            var Col = Database.Column[0];
            // Zeilen Zeichnen (Alle Zellen)
            for (var Zeiv = vFirstVisibleRow; Zeiv <= vLastVisibleRow; Zeiv++) {
                var CurrentRow = sr[Zeiv];
                var ViewItem = _Database.ColumnArrangements[0][Col];
                Rectangle r = new(0, DrawY(CurrentRow), DisplayRectangleWithoutSlider().Width, CurrentRow.DrawHeight);
                if (_CursorPosColumn != null && _CursorPosRow.Row == CurrentRow.Row) {
                    ItStat |= enStates.Checked;
                } else {
                    if (Convert.ToBoolean(ItStat & enStates.Checked)) {
                        ItStat ^= enStates.Checked;
                    }
                }

                Rectangle cellrectangle = new(0,
                       DrawY(CurrentRow),
                        displayRectangleWOSlider.Width,
                        Math.Min(CurrentRow.DrawHeight, 24));

                Draw_CellListBox(GR, ViewItem, CurrentRow, cellrectangle, r, enDesign.Item_Listbox, ItStat);
                if (!CurrentRow.Row.CellGetBoolean(_Database.Column.SysCorrect)) {
                    GR.DrawImage(QuickImage.Get("Warnung|16||||||120||50").BMP, new Point(r.Right - 19, (int)(r.Top + ((r.Height - 16) / 2.0))));
                }
                if (CurrentRow.ShowCap) {
                    BlueFont.DrawString(GR, CurrentRow.Chapter, _Chapter_Font.Font(), _Chapter_Font.Brush_Color_Main, 0, DrawY(CurrentRow) - RowCaptionFontY);
                }
            }
            Skin.Draw_Border(GR, enDesign.ListBox, state, displayRectangleWOSlider);
        }

        private void Draw_Table_Std(Graphics GR, List<RowData> sr, enStates state, Rectangle displayRectangleWOSlider, int FirstVisibleRow, int LastVisibleRow) {
            try {
                if (_Database.ColumnArrangements == null || _ArrangementNr >= _Database.ColumnArrangements.Count) { return; }   // Kommt vor, dass spontan doch geparsed wird...
                Skin.Draw_Back(GR, enDesign.Table_And_Pad, state, DisplayRectangle, this, true);
                /// Maximale Rechten Pixel der Permanenten Columns ermitteln
                var PermaX = 0;
                foreach (var ViewItem in CurrentArrangement) {
                    if (ViewItem != null && ViewItem.Column != null && ViewItem.ViewType == enViewType.PermanentColumn) {
                        if (ViewItem._TMP_DrawWidth == null) {
                            // Veränderte Werte!
                            DrawControl(GR, state);
                            return;
                        }
                        PermaX = Math.Max(PermaX, (int)ViewItem.OrderTMP_Spalte_X1 + (int)ViewItem._TMP_DrawWidth);
                    }
                }
                Draw_Table_What(GR, sr, enTableDrawColumn.NonPermament, enTableDrawType.ColumnBackBody, PermaX, displayRectangleWOSlider, FirstVisibleRow, LastVisibleRow, state);
                Draw_Table_What(GR, sr, enTableDrawColumn.NonPermament, enTableDrawType.Cells, PermaX, displayRectangleWOSlider, FirstVisibleRow, LastVisibleRow, state);
                Draw_Table_What(GR, sr, enTableDrawColumn.Permament, enTableDrawType.ColumnBackBody, PermaX, displayRectangleWOSlider, FirstVisibleRow, LastVisibleRow, state);
                Draw_Table_What(GR, sr, enTableDrawColumn.Permament, enTableDrawType.Cells, PermaX, displayRectangleWOSlider, FirstVisibleRow, LastVisibleRow, state);
                // Den CursorLines zeichnen
                Draw_Cursor(GR, displayRectangleWOSlider, true);
                Draw_Table_What(GR, sr, enTableDrawColumn.NonPermament, enTableDrawType.ColumnHead, PermaX, displayRectangleWOSlider, FirstVisibleRow, LastVisibleRow, state);
                Draw_Table_What(GR, sr, enTableDrawColumn.Permament, enTableDrawType.ColumnHead, PermaX, displayRectangleWOSlider, FirstVisibleRow, LastVisibleRow, state);

                /// Überschriften 1-3 Zeichnen
                Draw_Column_Head_Captions(GR);
                Skin.Draw_Border(GR, enDesign.Table_And_Pad, state, displayRectangleWOSlider);

                if (Database.ReloadNeeded) { GR.DrawImage(QuickImage.Get(enImageCode.Uhr, 16).BMP, 8, 8); }
                if (Database.HasPendingChanges()) { GR.DrawImage(QuickImage.Get(enImageCode.Stift, 16).BMP, 16, 8); }
                if (Database.ReadOnly) { GR.DrawImage(QuickImage.Get(enImageCode.Schloss, 32).BMP, 16, 8); }
            } catch {
                Invalidate();
                //Develop.DebugPrint(ex);
            }
        }

        private void Draw_Table_What(Graphics GR, List<RowData> sr, enTableDrawColumn col, enTableDrawType type, int PermaX, Rectangle displayRectangleWOSlider, int FirstVisibleRow, int LastVisibleRow, enStates state) {
            var lfdno = 0;

            bool firstOnScreen = true;

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
                                    Draw_Column_Cells(GR, sr, ViewItem, displayRectangleWOSlider, FirstVisibleRow, LastVisibleRow, state, firstOnScreen);
                                    break;

                                case enTableDrawType.ColumnHead:
                                    Draw_Column_Head(GR, ViewItem, displayRectangleWOSlider, lfdno);
                                    break;
                            }
                        }
                        firstOnScreen = false;
                    }
                }
            }
        }

        private void DrawWaitScreen(Graphics GR) {
            if (SliderX != null) { SliderX.Enabled = false; }
            if (SliderY != null) { SliderY.Enabled = false; }

            Skin.Draw_Back(GR, enDesign.Table_And_Pad, enStates.Standard_Disabled, DisplayRectangle, this, true);

            var i = QuickImage.Get(enImageCode.Uhr, 64).BMP;
            GR.DrawImage(i, (Width - 64) / 2, (Height - 64) / 2);
            Skin.Draw_Border(GR, enDesign.Table_And_Pad, enStates.Standard_Disabled, DisplayRectangle);
        }

        private int DrawY(RowData r) => r == null ? 0 : r.Y + HeadSize() - (int)SliderY.Value;

        private void DropDownMenu_ItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
            FloatingForm.Close(this);
            if (string.IsNullOrEmpty(e.ClickedComand)) { return; }

            CellExtEventArgs CK = null;
            if (e.HotItem is CellExtEventArgs tmp) { CK = tmp; }

            var ToAdd = e.ClickedComand;
            var ToRemove = string.Empty;
            if (ToAdd == "#Erweitert") {
                Cell_Edit(CK.Column, CK.Row, false);
                return;
            }
            if (CK.Row == null) {
                // Neue Zeile!
                UserEdited(this, ToAdd, CK.Column, null, string.Empty, false);
                return;
            }

            if (CK.Column.MultiLine) {
                var E = CK.Row.Row.CellGetList(CK.Column);
                if (E.Contains(ToAdd, false)) {
                    // Ist das angeklickte Element schon vorhanden, dann soll es wohl abgewählt (gelöscht) werden.
                    if (E.Count > -1 || CK.Column.DropdownAllesAbwählenErlaubt) {
                        ToRemove = ToAdd;
                        ToAdd = string.Empty;
                    }
                }
                if (!string.IsNullOrEmpty(ToRemove)) { E.RemoveString(ToRemove, false); }
                if (!string.IsNullOrEmpty(ToAdd)) { E.Add(ToAdd); }
                UserEdited(this, E.JoinWithCr(), CK.Column, CK.Row.Row, CK.Row.Chapter, false);
            } else {
                if (CK.Column.DropdownAllesAbwählenErlaubt) {
                    if (ToAdd == CK.Row.Row.CellGetString(CK.Column)) {
                        UserEdited(this, string.Empty, CK.Column, CK.Row.Row, CK.Row.Chapter, false);
                        return;
                    }
                }
                UserEdited(this, ToAdd, CK.Column, CK.Row.Row, CK.Row.Chapter, false);
            }
        }

        private bool EnsureVisible(RowData rowdata) {
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

        private void Filter_Changed(object sender, System.EventArgs e) {
            Invalidate_FilteredRows();
            OnFilterChanged();
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

        private void Invalidate_DrawWidth(ColumnItem vcolumn) => CurrentArrangement[vcolumn]?.Invalidate_DrawWidth();

        private void Invalidate_FilteredRows() {
            _FilteredRows = null;
            //CursorPos_Reset(); // Gibt Probleme bei Formularen, wenn die Key-Spalte geändert wird. Mal abgesehen davon macht es einen Sinn, den Cursor proforma zu löschen, dass soll der RowSorter übernehmen.
            Invalidate_Filterinfo();
            Invalidate_SortedRowData();
        }

        private void Invalidate_Filterinfo() {
            if (_Database == null) { return; }
            foreach (var thisColumn in _Database.Column) {
                if (thisColumn != null) {
                    thisColumn.TMP_IfFilterRemoved = null;
                    thisColumn.TMP_AutoFilterSinnvoll = null;
                }
            }
        }

        private void Invalidate_SortedRowData() {
            _SortedRowData = null;
            Invalidate();
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

        private bool IsOnScreen(ColumnItem column, RowData row, Rectangle displayRectangleWOSlider) => IsOnScreen(CurrentArrangement[column], displayRectangleWOSlider) && IsOnScreen(row, displayRectangleWOSlider);

        private bool IsOnScreen(RowData vrow, Rectangle displayRectangleWOSlider) => vrow != null && DrawY(vrow) + vrow.DrawHeight >= HeadSize() && DrawY(vrow) <= displayRectangleWOSlider.Height;

        private bool Mouse_IsInAutofilter(ColumnViewItem ViewItem, System.Windows.Forms.MouseEventArgs e) => ViewItem != null && ViewItem._TMP_AutoFilterLocation.Width != 0 && ViewItem.Column.AutoFilterSymbolPossible() && ViewItem._TMP_AutoFilterLocation.Contains(e.X, e.Y);

        private bool Mouse_IsInRedcueButton(ColumnViewItem ViewItem, System.Windows.Forms.MouseEventArgs e) => ViewItem != null && ViewItem._TMP_ReduceLocation.Width != 0 && ViewItem._TMP_ReduceLocation.Contains(e.X, e.Y);

        /// <summary>
        /// Berechnet die Zelle, ausgehend von einer Zellenposition. Dabei wird die Columns und Zeilensortierung berücksichtigt.
        /// Gibt des keine Nachbarszelle, wird die Eingangszelle zurückgegeben.
        /// </summary>
        /// <remarks></remarks>
        private void Neighbour(ColumnItem column, RowData row, enDirection direction, out ColumnItem newColumn, out RowData newRow) {
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

        private void NotEditableInfo(string Reason) => Notification.Show(Reason, enImageCode.Kreuz);

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

        private void Row_RowRemoving(object sender, RowEventArgs e) {
            if (e.Row == _CursorPosRow?.Row) { CursorPos_Reset(); }
            if (e.Row == _MouseOverRow?.Row) { _MouseOverRow = null; }
            if (PinnedRows.Contains(e.Row)) { PinnedRows.Remove(e.Row); }
        }

        private string RowCaptionOnCoordinate(int pixelX, int pixelY) {
            try {
                var s = SortedRows();
                foreach (var thisRow in s) {
                    if (thisRow.ShowCap && thisRow.CaptionPos is Rectangle r) {
                        if (r.Contains(pixelX, pixelY)) { return thisRow.Chapter; }
                    }
                }
            } catch { }
            return string.Empty;
        }

        private RowData RowOnCoordinate(int pixelY) {
            if (_Database == null || pixelY <= HeadSize()) { return null; }
            var s = SortedRows();
            foreach (var ThisRowItem in s) {
                if (ThisRowItem != null) {
                    if (pixelY >= DrawY(ThisRowItem) &&
                        pixelY <= DrawY(ThisRowItem) + ThisRowItem.DrawHeight
                        && ThisRowItem.Expanded) {
                        return ThisRowItem;
                    }
                }
            }
            return null;
        }

        private void SliderSchaltenSenk(Rectangle DisplayR, int MaxY) {
            SliderY.Minimum = 0;
            SliderY.Maximum = Math.Max(MaxY - DisplayR.Height + 1 + HeadSize(), 0);
            SliderY.LargeChange = DisplayR.Height - HeadSize();
            SliderY.Enabled = Convert.ToBoolean(SliderY.Maximum > 0);
        }

        private void SliderSchaltenWaage(Rectangle DisplayR, int MaxX) {
            SliderX.Minimum = 0;
            SliderX.Maximum = MaxX - DisplayR.Width + 1;
            SliderX.LargeChange = DisplayR.Width;
            SliderX.Enabled = Convert.ToBoolean(SliderX.Maximum > 0);
        }

        private void SliderX_ValueChanged(object sender, System.EventArgs e) {
            if (_Database == null) { return; }
            lock (Lock_UserAction) {
                Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
                Invalidate();
            }
        }

        private void SliderY_ValueChanged(object sender, System.EventArgs e) {
            if (_Database == null) { return; }
            lock (Lock_UserAction) {
                Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
                Invalidate();
            }
        }

        private RowSortDefinition SortUsed() {
            if (_sortDefinitionTemporary?.Columns != null) { return _sortDefinitionTemporary; }
            return _Database.SortDefinition?.Columns != null ? _Database.SortDefinition : null;
        }

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
            UserEdited(this, w, column, row, _CursorPosRow?.Chapter, true);
            Focus();
        }

        private bool UserEdit_NewRowAllowed() {
            if (_Database == null || _Database.Column.Count == 0 || _Database.Column[0] == null) { return false; }
            if (_Design == enBlueTableAppearance.OnlyMainColumnWithoutHead) { return false; }
            if (_Database.ColumnArrangements.Count == 0) { return false; }
            if (CurrentArrangement == null || CurrentArrangement[_Database.Column[0]] == null) { return false; }
            if (!_Database.PermissionCheck(_Database.PermissionGroups_NewRow, null)) { return false; }

            if (PowerEdit.Subtract(DateTime.Now).TotalSeconds > 0) { return true; }

            if (!CellCollection.UserEditPossible(_Database.Column[0], null, enErrorReason.EditNormaly)) { return false; }
            return true;
        }

        #endregion

        // QickInfo beisst sich mit den letzten Änderungen Quickinfo//DialogBoxes.QuickInfo.Show("<IMAGECODE=Stift|16||1> " + Reason);
    }
}