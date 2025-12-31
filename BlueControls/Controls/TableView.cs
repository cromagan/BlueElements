// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using BlueBasics.Interfaces;
using BlueBasics.MultiUserFile;
using BlueControls.BlueTableDialogs;
using BlueControls.CellRenderer;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Extended_Text;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueTable;
using BlueTable.Enums;
using BlueTable.EventArgs;
using BlueTable.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static BlueBasics.Constants;
using static BlueBasics.Converter;
using static BlueBasics.Generic;
using static BlueBasics.IO;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using static BlueTable.Table;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
[DefaultEvent(nameof(SelectedRowChanged))]
[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public partial class TableView : ZoomPad, IContextMenu, ITranslateable, IHasTable, IOpenScriptEditor, IStyleable {

    #region Fields

    public const string Angepinnt = "Angepinnt";
    public const string CellDataFormat = "BlueElements.CellLink";

    public const string Weitere_Zeilen = "Weitere Zeilen";
    private readonly Dictionary<string, AbstractListItem> _allViewItems = [];

    /// <summary>
    /// Großschreibung
    /// </summary>
    private readonly List<string> _collapsed = [];

    private readonly object _lockUserAction = new();
    private readonly object lockMe = new object();
    private string _arrangement = string.Empty;
    private AutoFilter? _autoFilter;
    private bool _isinDoubleClick;
    private bool _isinKeyDown;
    private bool _isinMouseDown;
    private bool _isinMouseMove;
    private bool _isinSizeChanged;
    private string _newRowsAllowed = string.Empty;
    private Progressbar? _pg;
    private List<RowItem> _rowsVisibleUnique = new([]);
    private RowSortDefinition? _sortDefinitionTemporary;
    private string _storedView = string.Empty;
    private DateTime? _tableDrawError;
    private bool mustDoAllViewItems = true;

    #endregion

    #region Constructors

    public TableView() : base() {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.

        Filter.RowsChanged += FilterAny_RowsChanged;
        Filter.PropertyChanged += Filter_PropertyChanged;
        FilterCombined.RowsChanged += FilterAny_RowsChanged;
        FilterCombined.PropertyChanged += FilterCombined_PropertyChanged;
        FilterFix.PropertyChanged += FilterFix_PropertyChanged;
    }

    #endregion

    #region Events

    public event EventHandler<FilterEventArgs>? AutoFilterClicked;

    public event EventHandler<CellEventArgs>? CellClicked;

    public event EventHandler<ContextMenuInitEventArgs>? ContextMenuInit;

    public new event EventHandler<CellExtEventArgs>? DoubleClick;

    public event EventHandler? FilterCombinedChanged;

    public event EventHandler? PinnedChanged;

    public event EventHandler<CellExtEventArgs>? SelectedCellChanged;

    public event EventHandler<RowNullableEventArgs>? SelectedRowChanged;

    public event EventHandler? TableChanged;

    public event EventHandler? ViewChanged;

    public event EventHandler? VisibleRowsChanged;

    #endregion

    #region Properties

    [DefaultValue("")]
    [Description("Welche Spaltenanordnung angezeigt werden soll")]
    public string Arrangement {
        get => _arrangement;
        set {
            if (value != _arrangement) {
                _arrangement = value;

                OnViewChanged();
                CursorPos_Set(CursorPosColumn, CursorPosRow, true);
            }
        }
    }

    /// <summary>
    /// Gibt an, ob das Standard-Kontextmenu der Tabellenansicht angezeitgt werden soll oder nicht
    /// </summary>
    public bool ContextMenuDefault { get; set; } = true;

    public override bool ControlMustPressed => true;

    public ColumnViewCollection? CurrentArrangement {
        get {
            if (IsDisposed || Table is not { IsDisposed: false } tb) { return null; }

            if (field == null) {
                var tcvc = ColumnViewCollection.ParseAll(tb);
                field = tcvc.GetByKey(_arrangement);

                if (field == null && tcvc.Count > 1) { field = tcvc[1]; }
                if (field == null && tcvc.Count > 0) { field = tcvc[0]; }
            }

            field?.SheetStyle = SheetStyle;
            field?.ComputeAllColumnPositions(AvailableControlPaintArea.Width, Zoom);

            return field;
        }

        private set;
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ColumnViewItem? CursorPosColumn { get; private set; }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public RowListItem? CursorPosRow { get; private set; }

    [DefaultValue(false)]
    public bool EditButton {
        get;
        set {
            if (field == value) { return; }
            field = value;
            btnEdit.Visible = field;
        }
    }

    public FilterCollection FilterCombined { get; } = new("TableFilterCombined");

    /// <summary>
    /// Filter, die fest an das Element übergeben werden und nicht verändert werden können.
    /// Werden bei FilterCombined ausgegeben.
    /// </summary>
    public FilterCollection FilterFix { get; } = new("FilterFix");

    public new bool Focused => base.Focused || BTB.Focused || BCB.Focused;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<RowItem> PinnedRows { get; } = [];

    public bool PowerEdit {
        set {
            if (IsDisposed || Table is not { IsDisposed: false }) { return; }
            Table.PowerEdit = value;
            Filter.Invalidate_FilteredRows(); // Split-Spalten-Filter
            FilterCombined.Invalidate_FilteredRows();
            Invalidate_AllViewItems(false); // Neue Zeilen können nun erlaubt sein
        }
    }

    public List<RowListItem> RowViewItems => AllViewItems?.Values
        .OfType<RowListItem>()
        .Where(thisitem => thisitem.Visible)
        .ToList() ?? [];

    public string SheetStyle {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            Invalidate_CurrentArrangement();
            Invalidate();
        }
    } = Win11;

    [DefaultValue(false)]
    public bool ShowNumber {
        get;
        set {
            if (value == field) { return; }
            CloseAllComponents();
            field = value;
            Invalidate_AllViewItems(false);
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
            if (_sortDefinitionTemporary != null && value != null && _sortDefinitionTemporary.ParseableItems().FinishParseable() == value.ParseableItems().FinishParseable()) { return; }
            if (_sortDefinitionTemporary == value) { return; }
            _sortDefinitionTemporary = value;
            _Table_SortParameterChanged(this, System.EventArgs.Empty);
        }
    }

    /// <summary>
    /// Tabellen können mit TableSet gesetzt werden.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Table? Table { get; private set; }

    [DefaultValue(true)]
    public bool Translate { get; set; } = true;

    /// <summary>
    /// Interne Filter, die im Controll erstellt wurden und auch geändert werden dürfen.
    /// Werden bei FilterCombined ausgegeben.
    /// </summary>
    internal FilterCollection Filter { get; } = new("DefaultTableFilter");

    protected override bool AutoCenter => false;

    protected override bool ShowSliderX => true;

    protected override int SmallChangeY => 10;

    private Dictionary<string, AbstractListItem>? AllViewItems {
        get {
            if (IsDisposed) { return null; }
            if (!mustDoAllViewItems) { return _allViewItems; }

            try {
                mustDoAllViewItems = false;
                CalculateAllViewItems(_allViewItems);

                OnVisibleRowsChanged();

                return _allViewItems;
            } catch {
                // Komisch, manchmal wird die Variable _sortedRowData verworfen.
                Develop.AbortAppIfStackOverflow();
                Invalidate_AllViewItems(false);
                return AllViewItems;
            }
        }
    }

    #endregion

    #region Methods

    public static void ContextMenu_DataValidation(object sender, ObjectEventArgs e) {
        var rows = new List<RowItem>();
        if (e.Data is RowItem row) { rows.Add(row); }
        if (e.Data is ICollection<RowItem> lrow) { rows.AddRange(lrow); }
        if (e.Data is Func<IReadOnlyList<RowItem>> fRows) { rows.AddRange(fRows()); }

        if (rows.Count == 0) {
            Forms.MessageBox.Show("Keine Zeilen zum Prüfen vorhanden.", ImageCode.Kreuz, "OK");
            return;
        }

        foreach (var thisR in rows) {
            if (thisR.Table is { IsDisposed: false } tb) {
                if (!tb.CanDoValueChangedScript(true)) {
                    Forms.MessageBox.Show("Abbruch, Skriptfehler sind aufgetreten.", ImageCode.Warnung, "OK");
                    RowCollection.InvalidatedRowsManager.DoAllInvalidatedRows(null, true, null);
                    return;
                }

                thisR.InvalidateRowState("TableView, Kontextmenü, Datenüberprüfung");
                thisR.UpdateRow(true, "TableView, Kontextmenü, Datenüberprüfung");
            }
        }

        if (rows.Count == 1) {
            Forms.MessageBox.Show("Datenüberprüfung:\r\n" + rows[0].CheckRow().Message, ImageCode.HäkchenDoppelt, "Ok");
        } else {
            Forms.MessageBox.Show($"Alle {rows.Count} Zeilen überprüft.", ImageCode.HäkchenDoppelt, "OK");
        }
    }

    public static void ContextMenu_DeleteRow(object sender, ObjectEventArgs e) {
        var rows = new List<RowItem>();
        if (e.Data is RowItem row) { rows.Add(row); }
        if (e.Data is ICollection<RowItem> lrow) { rows.AddRange(lrow); }

        if (rows.Count == 0) {
            Forms.MessageBox.Show("Keine Zeilen zum Löschen vorhanden.", ImageCode.Kreuz, "OK");
            return;
        }

        if (rows[0].Table is not { IsDisposed: false } tb || !tb.IsAdministrator()) { return; }

        if (rows.Count == 1) {
            if (Forms.MessageBox.Show($"Zeile wirklich löschen? (<b>{rows[0].CellFirstString()}</b>)", ImageCode.Frage, "Löschen", "Abbruch") != 0) { return; }
        } else {
            if (Forms.MessageBox.Show($"{rows.Count} Zeilen wirklich löschen?", ImageCode.Frage, "Löschen", "Abbruch") != 0) { return; }
        }
        RowCollection.Remove(rows, "Benutzer: löschen Befehl");
    }

    public static void ContextMenu_EditColumnProperties(object sender, ObjectEventArgs e) {
        ColumnItem? column = null;
        RowItem? row = null;
        TableView? view = null;

        if (e.Data is ColumnItem c) {
            column = c;
        } else if (e.Data is { } data) {
            var type = data.GetType();
            column = type.GetProperty("Column")?.GetValue(data) as ColumnItem;

            row = type.GetProperty("Row")?.GetValue(data) as RowItem;
            view = type.GetProperty("View")?.GetValue(data) as TableView;
        }

        if (column is not { IsDisposed: false }) { return; }
        column.Editor = typeof(ColumnEditor);

        if (TableViewForm.EditabelErrorMessage(column.Table)) { return; }

        //if (row is not { IsDisposed: false }) {
        //    column.Edit();
        //    return;
        //}

        ColumnItem? columnLinked = null;
        var posError = false;

        if (column.RelationType == RelationType.CellValues && row is { }) {
            (columnLinked, _, _, _) = row.LinkedCellData(column, true, false);
            posError = true;
        }

        var bearbColumn = column;
        if (columnLinked != null) {
            columnLinked.Repair();
            if (!columnLinked.IsOk()) {
                bearbColumn = columnLinked;
                Forms.MessageBox.Show("Zuerst muss die verlinkte Spalte\rrepariert werden.", ImageCode.Information, "Ok");
            } else if (Forms.MessageBox.Show("Welche Spalte bearbeiten?", ImageCode.Frage, "Spalte in dieser Tabelle", "Verlinkte Spalte") == 1) {
                bearbColumn = columnLinked;
            }
        } else {
            if (posError) {
                Notification.Show(
                    "Keine aktive Verlinkung.<br>Spalte in dieser Tabelle wird angezeigt.<br><br>Ist die Ziel-Zelle in der Ziel-Tabelle vorhanden?",
                    ImageCode.Information);
            }
        }

        column.Repair();

        using var w = new ColumnEditor(bearbColumn, view);
        w.ShowDialog();

        bearbColumn.Repair();
    }

    public static void ContextMenu_ExecuteScript(object sender, ObjectEventArgs e) {
        Develop.SetUserDidSomething();
        if (e.Data is not { } data) { return; }

        var type = data.GetType();

        if (type.GetProperty("Script")?.GetValue(data) is not TableScriptDescription sc || sc.Table is not { } tb) { return; }

        if (TableViewForm.EditabelErrorMessage(sc.Table)) { return; }

        string m;

        if (sc.NeedRow) {
            var rows = new List<RowItem>();

            if (type.GetProperty("Row")?.GetValue(data) is RowItem singleRow) {
                rows.Add(singleRow);
            } else if (type.GetProperty("Rows")?.GetValue(data) is List<RowItem> rowList) {
                rows.AddRange(rowList);
            } else if (type.GetProperty("Rows")?.GetValue(data) is Func<IReadOnlyList<RowItem>> fRows) {
                rows.AddRange(fRows());
            }

            if (rows.Count > 0) {
                if (Forms.MessageBox.Show($"Skript für {rows.Count} Zeile(n) ausführen?", ImageCode.Skript, "Ja", "Nein") == 0) {
                    m = tb.Row.ExecuteScript(null, sc.KeyName, rows);
                } else {
                    m = "Durch Benutzer abgebrochen";
                }
            } else {
                m = "Keine Zeile zum Ausführen des Skriptes vorhanden.";
            }
        } else {
            var s = tb.ExecuteScript(sc, sc.ChangeValuesAllowed, null, null, true, true, false);
            m = s.Protocol.JoinWithCr();
        }

        if (string.IsNullOrEmpty(m)) {
            Forms.MessageBox.Show("Skript erfolgreich ausgeführt.", ImageCode.Häkchen, "Ok");
        } else {
            Forms.MessageBox.Show("Skript abgebrochen:\r\n" + m, ImageCode.Kreuz, "OK");
        }
    }

    public static void CopyToClipboard(ColumnItem? column, RowItem? row, bool meldung) {
        try {
            if (row != null && column != null && column.Table is { } tb) {
                var c = row.CellGetString(column);
                c = c.Replace("\r\n", "\r");
                c = c.Replace("\r", "\r\n");

                var dataObject = new DataObject();

                if (tb is TableFile) {
                    dataObject.SetData(CellDataFormat, $"{tb.KeyName}\r{column.KeyName}\r{row.KeyName}");// 1. Als ExtChar-Format (für interne Verwendung)
                }
                dataObject.SetText(c);// 2. Als Plain Text (für externe Anwendungen)
                Clipboard.SetDataObject(dataObject, true);

                //_ = CopytoClipboard(c);
                if (meldung) { Notification.Show(LanguageTool.DoTranslate("<b>{0}</b><br>ist nun in der Zwischenablage.", true, c), ImageCode.Kopieren); }
            } else {
                if (meldung) { Notification.Show(LanguageTool.DoTranslate("Bei dieser Zelle nicht möglich."), ImageCode.Warnung); }
            }
        } catch {
            if (meldung) { Notification.Show(LanguageTool.DoTranslate("Unerwarteter Fehler beim Kopieren."), ImageCode.Warnung); }
        }
    }

    public static void DoUndo(ColumnItem? column, RowItem? row) {
        if (column is not { IsDisposed: false }) { return; }
        if (row is not { IsDisposed: false }) { return; }
        if (!column.SaveContent) { return; }

        if (column.RelationType == RelationType.CellValues) {
            var (lcolumn, lrow, _, _) = row.LinkedCellData(column, true, false);
            if (lcolumn != null && lrow != null) { DoUndo(lcolumn, lrow); }
            return;
        }
        var cellKey = CellCollection.KeyOfCell(column, row);
        var i = UndoItems(column.Table, cellKey);
        if (i.Count < 1) {
            Forms.MessageBox.Show("Keine vorherigen Inhalte<br>(mehr) vorhanden.", ImageCode.Information, "OK");
            return;
        }
        var v = InputBoxListBoxStyle.Show("Vorherigen Eintrag wählen:", i, CheckBehavior.SingleSelection, ["Cancel"], AddType.None);
        if (v is not { Count: 1 }) { return; }
        if (v[0] == "Cancel") { return; } // =Aktueller Eintrag angeklickt
        row.CellSet(column, v[0].Substring(5), "Undo-Befehl");
        //row.Table?.Row.ExecuteValueChangedEvent(true);
    }

    public static string Export_CSV(Table tbl, FirstRow firstRow, IEnumerable<ColumnItem>? columnList, IEnumerable<RowItem> sortedRows) {
        //BeSureAllDataLoaded(-1);

        var columnListtmp = columnList?.ToList();
        columnListtmp ??= [.. tbl.Column.Where(thisColumnItem => thisColumnItem != null)];

        StringBuilder sb = new();
        switch (firstRow) {
            case FirstRow.Without:
                break;

            case FirstRow.ColumnCaption:
                for (var colNr = 0; colNr < columnListtmp.Count; colNr++) {
                    if (columnListtmp[colNr] != null) {
                        var tmp = columnListtmp[colNr].ReadableText();
                        tmp = tmp.Replace(";", "|");
                        tmp = tmp.Replace(" |", "|");
                        tmp = tmp.Replace("| ", "|");
                        sb.Append(tmp);
                        if (colNr < columnListtmp.Count - 1) { sb.Append(';'); }
                    }
                }
                sb.Append("\r\n");
                break;

            case FirstRow.ColumnInternalName:
                for (var colNr = 0; colNr < columnListtmp.Count; colNr++) {
                    if (columnListtmp[colNr] != null) {
                        sb.Append(columnListtmp[colNr].KeyName);
                        if (colNr < columnListtmp.Count - 1) { sb.Append(';'); }
                    }
                }
                sb.Append("\r\n");
                break;

            default:
                Develop.DebugPrint(firstRow);
                break;
        }

        foreach (var thisRow in sortedRows) {
            if (thisRow is { IsDisposed: false }) {
                for (var colNr = 0; colNr < columnListtmp.Count; colNr++) {
                    if (columnListtmp[colNr] is { } columnItem) {
                        var tmp = thisRow.CellGetString(columnItem);

                        if (columnItem.TextFormatingAllowed) {
                            using var t = new ExtText();
                            t.HtmlText = tmp;
                            tmp = t.PlainText;
                        }

                        tmp = tmp.Replace("\r\n", "|");
                        tmp = tmp.Replace("\r", "|");
                        tmp = tmp.Replace("\n", "|");
                        tmp = tmp.Replace(";", "<sk>");
                        sb.Append(tmp);
                        if (colNr < columnListtmp.Count - 1) { sb.Append(';'); }
                    }
                }
                sb.Append("\r\n");
            }
        }
        return sb.ToString().TrimEnd("\r\n");
    }

    public static void ImportBtb(Table table) {
        using var x = new ImportBtb(table);
        x.ShowDialog();
    }

    public static void ImportCsv(Table table, string csvtxt) {
        using ImportCsv x = new(table, csvtxt);
        x.ShowDialog();
    }

    public static List<string> Permission_AllUsed(bool mitRowCreator) {
        var l = new List<string>();

        foreach (var thisTb in AllFiles) {
            if (!thisTb.IsDisposed) {
                l.AddRange(Permission_AllUsedInThisTable(thisTb, mitRowCreator));
            }
        }

        return RepairUserGroups(l);
    }

    public static List<string> Permission_AllUsedInThisTable(Table tb, bool mitRowCreator) {
        List<string> e = [];
        foreach (var thisColumnItem in tb.Column) {
            if (thisColumnItem != null) {
                e.AddRange(thisColumnItem.PermissionGroupsChangeCell);
            }
        }
        e.AddRange(tb.PermissionGroupsNewRow);
        e.AddRange(tb.TableAdmin);

        var tcvc = ColumnViewCollection.ParseAll(tb);
        foreach (var thisArrangement in tcvc) {
            e.AddRange(thisArrangement.PermissionGroups_Show);
        }

        foreach (var thisEv in tb.EventScript) {
            e.AddRange(thisEv.UserGroups);
        }

        e.Add(Everybody);
        e.Add("#User: " + UserName);

        if (mitRowCreator) {
            e.Add("#RowCreator");
        } else {
            e.RemoveString("#RowCreator", false);
        }
        e.Add(UserGroup);
        e.RemoveString(Administrator, false);

        return RepairUserGroups(e);
    }

    public static Renderer_Abstract RendererOf(ColumnItem? column, string style) {
        if (column == null || string.IsNullOrEmpty(column.DefaultRenderer)) { return Renderer_Abstract.Default; }

        var renderer = ParseableItem.NewByTypeName<Renderer_Abstract>(column.DefaultRenderer);
        if (renderer == null) { return Renderer_Abstract.Default; }

        if (!renderer.Parse(column.RendererSettings)) { return Renderer_Abstract.Default; }
        renderer.SheetStyle = style;

        return renderer;
    }

    public static void SearchNextText(string searchTxt, TableView tableView, ColumnViewItem? column, RowListItem? row, out ColumnViewItem? foundColumn, out RowListItem? foundRow, bool vereinfachteSuche) {
        if (tableView.Table is not { IsDisposed: false } tb) {
            Forms.MessageBox.Show("Tabellen-Fehler.", ImageCode.Information, "OK");
            foundColumn = null;
            foundRow = null;
            return;
        }

        searchTxt = searchTxt.Trim();
        if (tableView.CurrentArrangement is not { IsDisposed: false } ca) {
            Forms.MessageBox.Show("Tabellen-Ansichts-Fehler.", ImageCode.Information, "OK");
            foundColumn = null;
            foundRow = null;
            return;
        }

        row ??= tableView.View_RowLast();
        column ??= ca.Last();
        var rowsChecked = 0;
        if (string.IsNullOrEmpty(searchTxt)) {
            Forms.MessageBox.Show("Bitte Text zum Suchen eingeben.", ImageCode.Information, "OK");
            foundColumn = null;
            foundRow = null;
            return;
        }

        do {
            column = ca.NextVisible(column);

            var renderer = column?.GetRenderer(tableView.SheetStyle);

            if (column is not { }) {
                column = ca.First();
                if (rowsChecked > tb.Row.Count + 1) {
                    foundColumn = null;
                    foundRow = null;
                    return;
                }
                rowsChecked++;
                row = tableView.View_NextRow(row) ?? tableView.View_RowFirst();
            }

            var ist1 = string.Empty;
            var ist2 = string.Empty;

            var tmprow = row?.Row;
            //if (column?.Column is { Function: ColumnFunction.Verknüpfung_zu_anderer_Tabellex } cv) {
            //    var (contentHolderCellColumn, contentHolderCellRow, _, _) = CellCollection.LinkedCellData(cv, tmprow, false, false);

            //    if (contentHolderCellColumn != null && contentHolderCellRow != null) {
            //        ist1 = contentHolderCellRow.CellGetString(contentHolderCellColumn);
            //        if (renderer is { }) {
            //            ist2 = renderer.ValueReadable(contentHolderCellRow.CellGetString(contentHolderCellColumn),
            //                ShortenStyle.Both, contentHolderCellColumn.DoOpticalTranslation);
            //        }
            //    }
            //} else {
            if (tmprow != null && column?.Column is { IsDisposed: false } c) {
                ist1 = tmprow.CellGetString(c);
                if (renderer is { }) {
                    ist2 = renderer.ValueReadable(tmprow.CellGetString(c), ShortenStyle.Both, c.DoOpticalTranslation);
                }
            }
            //}

            if (column?.Column is { IsDisposed: false, TextFormatingAllowed: true }) {
                ExtText l = new(Design.TextBox, States.Standard) {
                    HtmlText = ist1
                };
                ist1 = l.PlainText;
            }
            // Allgemeine Prüfung
            if (!string.IsNullOrEmpty(ist1) && ist1.ContainsIgnoreCase(searchTxt)) {
                foundColumn = column;
                foundRow = row;
                return;
            }
            // Prüfung mit und ohne Ersetzungen / Prefix / Suffix
            if (!string.IsNullOrEmpty(ist2) && ist2.ContainsIgnoreCase(searchTxt)) {
                foundColumn = column;
                foundRow = row;
                return;
            }
            if (vereinfachteSuche) {
                var ist3 = ist2.StarkeVereinfachung(" ,", true);
                var searchTxt3 = searchTxt.StarkeVereinfachung(" ,", true);
                if (!string.IsNullOrEmpty(ist3) && ist3.ContainsIgnoreCase(searchTxt3)) {
                    foundColumn = column;
                    foundRow = row;
                    return;
                }
            }
        } while (true);
    }

    //    return renderer.GetSizeOfCellContent(column, row.CellGetString(column), Design.Table_Cell, States.Standard,
    //        column.BehaviorOfImageAndText, column.DoOpticalTranslation, column.OpticalReplace, tb.GlobalScale, column.ConstantHeightOfImageCode);
    //}
    public static void Table_AdditionalRepair(object sender, System.EventArgs e) {
        if (sender is not Table tbl) { return; }

        RepairColumnArrangements(tbl);
    }

    public static void Table_CanDoScript(object sender, CanDoScriptEventArgs e) {
        if (!string.IsNullOrEmpty(e.CancelReason)) { return; }

        if (sender is not Table tbl) { return; }
        if (!FormManager.Running) { e.CancelReason = "Programm wird beendet"; return; }

        foreach (var thisf in FormManager.Forms) {
            if (thisf is TableHeadEditor) { e.CancelReason = "Head Editor geöffnet"; return; }
            if (thisf is TableScriptEditor tbs && tbs.Table != tbl) { e.CancelReason = "Fremder Skript Editor geöffnet"; return; }
        }
    }

    //    if (column.Function == ColumnFunction.Verknüpfung_zu_anderer_Tabellex) {
    //        var (lcolumn, lrow, _, _) = CellCollection.LinkedCellData(column, row, false, false);
    //        return lcolumn != null && lrow != null ? ContentSize(lcolumn, lrow, renderer)
    //            : new CanvasSize(16, 16);
    //    }
    public static string Table_NeedPassword() => InputBox.Show("Bitte geben sie das Passwort ein,<br>um Zugriff auf diese Tabelle<br>zu erhalten:", string.Empty, FormatHolder.Text);

    public static List<AbstractListItem> UndoItems(Table? tb, string cellkey) {
        List<AbstractListItem> i = [];

        if (tb is not { IsDisposed: false }) { return i; }
        if (tb.Undo.Count == 0) { return i; }

        var sortedUndoItems = tb.Undo.Where(item => item.CellKey == cellkey).OrderByDescending(item => item.DateTimeUtc);

        var isfirst = true;
        TextListItem? las = null;
        var lasNr = -1;
        var co = 0;

        foreach (var undoItem in sortedUndoItems) {
            co++;
            lasNr = tb.Undo.IndexOf(undoItem);
            las = isfirst
                ? new TextListItem(
                    $"Aktueller Text - ab {undoItem.DateTimeUtc} UTC, geändert von {undoItem.User}",
                    "Cancel", null, false, true, undoItem.DateTimeUtc.ToString9())
                : new TextListItem(
                   $"ab {undoItem.DateTimeUtc}  UTC, geändert von {undoItem.User}",
                    co.ToString5() + undoItem.ChangedTo, null, false, true, undoItem.DateTimeUtc.ToString9());
            isfirst = false;

            i.Add(las);
        }

        if (las != null) {
            var undoItem = tb.Undo[lasNr];
            var l2 = ItemOf($"vor {undoItem.DateTimeUtc} UTC",
                co.ToString5() + undoItem.PreviousValue, null, false, true, undoItem.DateTimeUtc.ToString9());

            i.Add(l2);
        }

        return i;
    }

    public static void WriteColumnArrangementsInto(ComboBox? columnArrangementSelector, Table? table, string showingKey) {
        if (columnArrangementSelector is not { IsDisposed: false }) { return; }

        columnArrangementSelector.AutoSort = false;

        columnArrangementSelector.ItemClear();
        columnArrangementSelector.DropDownStyle = ComboBoxStyle.DropDownList;

        if (table is { IsDisposed: false } tb) {
            var tcvc = ColumnViewCollection.ParseAll(tb);

            foreach (var thisArrangement in tcvc) {
                if (tb.PermissionCheck(thisArrangement.PermissionGroups_Show, null)) {
                    columnArrangementSelector.ItemAdd(ItemOf(thisArrangement as IReadableTextWithKey));
                }
            }
        }

        columnArrangementSelector.Enabled = columnArrangementSelector.ItemCount > 1;

        if (columnArrangementSelector[showingKey] == null) {
            showingKey = columnArrangementSelector.ItemCount > 1 ? columnArrangementSelector[1].KeyName : string.Empty;
        }

        columnArrangementSelector.Text = showingKey;
    }

    public void CheckView() {
        var tb = Table;
        if (CursorPosColumn?.Column?.Table != tb) { CursorPosColumn = null; }
        if (CursorPosRow?.Row.Table != tb) { CursorPosRow = null; }

        if (CurrentArrangement is { IsDisposed: false } ca && tb != null) {
            if (!tb.PermissionCheck(ca.PermissionGroups_Show, null)) { Arrangement = string.Empty; }
        } else {
            Arrangement = string.Empty;
        }
    }

    public void CollapesAll() {
        var did = false;

        if (AllViewItems is not { } avi) { return; }

        foreach (var thisItem in avi.Values) {
            if (thisItem is RowCaptionListItem { IsDisposed: false, IsExpanded: true } rcli) { rcli.IsExpanded = false; did = true; }
        }

        if (did) { Invalidate_AllViewItems(false); }
    }

    public void CursorPos_Set(ColumnViewItem? column, AbstractListItem? row, bool ensureVisible) {
        if (IsDisposed || Table is not { IsDisposed: false } || row == null || column == null ||
            CurrentArrangement is not { IsDisposed: false } ca2 || !ca2.Contains(column) ||
            AllViewItems is not { } avi || !avi.Values.Contains(row)) {
            column = null;
            row = null;
        }

        var sameRow = CursorPosRow == row;

        if (CursorPosColumn == column && CursorPosRow == row) { return; }
        QuickInfo = string.Empty;
        CursorPosColumn = column;
        CursorPosRow = row as RowListItem;

        //if (CursorPosColumn != column) { return; }

        if (IsDisposed || Table is not { IsDisposed: false }) { return; }

        DoCursorPos();

        if (ensureVisible) {
            EnsureVisible(CursorPosColumn, CursorPosRow);
        }
        Invalidate();

        OnSelectedCellChanged(new CellExtEventArgs(CursorPosColumn, CursorPosRow));

        if (!sameRow) {
            OnSelectedRowChanged(new RowNullableEventArgs(CursorPosRow?.Row));
        }
    }

    public bool EnsureVisible(ColumnViewItem? viewItem, AbstractListItem? row) => EnsureVisible(viewItem) && EnsureVisible(row);

    public void ExpandAll() {
        var did = false;

        if (AllViewItems is not { } avi) { return; }

        foreach (var thisItem in avi.Values) {
            if (thisItem is RowCaptionListItem { IsDisposed: false, IsExpanded: false } rcli) { rcli.IsExpanded = true; did = true; }
        }

        if (did) {
            CursorPos_Reset(); // Wenn eine Zeile markiert ist, man scrollt und expandiert, springt der Screen zurück, was sehr irriteiert

            Invalidate_AllViewItems(false);
        }
    }

    public string Export_CSV(FirstRow firstRow) => Table == null ? string.Empty : Export_CSV(Table, firstRow, CurrentArrangement?.ListOfUsedColumn(), RowsVisibleUnique());

    public string Export_CSV(FirstRow firstRow, ColumnItem onlyColumn) {
        if (IsDisposed || Table is not { IsDisposed: false }) { return string.Empty; }
        List<ColumnItem> l = [onlyColumn];
        return Export_CSV(Table, firstRow, l, RowsVisibleUnique());
    }

    public void Export_HTML(string filename = "", bool execute = true) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }
        if (CurrentArrangement is not { IsDisposed: false } ca) { return; }

        if (string.IsNullOrEmpty(filename)) { filename = TempFile(string.Empty, string.Empty, "html"); }

        if (string.IsNullOrEmpty(filename)) {
            filename = TempFile(string.Empty, "Export", "html");
        }

        var da = new Html(tb.KeyName.FileNameWithoutSuffix());
        da.AddCaption(tb.Caption);
        da.TableBeginn();

        #region Spaltenköpfe

        da.RowBeginn();
        foreach (var thisColumn in ca) {
            if (thisColumn.Column != null) {
                da.CellAdd(thisColumn.Column.ReadableText().Replace(";", "<br>"), thisColumn.Column.BackColor);
            }
        }

        da.RowEnd();

        #endregion

        #region Zeilen

        if (AllViewItems is { } avi) {
            foreach (var thisItem in avi.Values) {
                if (thisItem is RowListItem { IsDisposed: false } rdli) {
                    da.RowBeginn();
                    foreach (var thisColumn in ca) {
                        if (thisColumn.Column != null) {
                            var lcColumn = thisColumn.Column;
                            var lCrow = rdli.Row;

                            if (lcColumn != null) {
                                da.CellAdd(lCrow.CellGetList(lcColumn).JoinWith("<br>"), thisColumn.Column.BackColor);
                            } else {
                                da.CellAdd(" ", thisColumn.Column.BackColor);
                            }
                        }
                    }

                    da.RowEnd();
                }
            }
        }

        #endregion

        da.TableEnd();
        da.AddFoot();
        da.Save(filename, execute);
    }

    /// <summary>
    /// Alle gefilteren Zeilen. Jede Zeile ist maximal einmal in dieser Liste vorhanden. Angepinnte Zeilen addiert worden
    /// </summary>
    /// <returns></returns>
    public new void Focus() {
        if (Focused) { return; }
        base.Focus();
    }

    public void GetContextMenuItems(ContextMenuInitEventArgs e) {
        if (ContextMenuDefault && Table is { IsDisposed: false } tb) {
            if (e.HotItem is not { } data) { return; }
            var _mouseOverColumn = data.GetType().GetProperty("Column")?.GetValue(data) as ColumnItem;
            var _mouseOverRow = data.GetType().GetProperty("Row")?.GetValue(data) as RowItem;

            #region Pinnen

            if (_mouseOverRow is { IsDisposed: false } row) {
                e.ContextMenu.Add(ItemOf("Anheften", true));
                if (PinnedRows.Contains(row)) {
                    e.ContextMenu.Add(ItemOf("Zeile nicht mehr pinnen", ImageCode.Pinnadel, ContextMenu_Unpin, row, true));
                } else {
                    e.ContextMenu.Add(ItemOf("Zeile anpinnen", ImageCode.Pinnadel, ContextMenu_Pin, row, true));
                }
            }

            #endregion

            #region Sortierung

            if (_mouseOverColumn is { IsDisposed: false } column) {
                e.ContextMenu.Add(ItemOf("Sortierung", true));
                e.ContextMenu.Add(ItemOf("Sortierung zurückstetzen", QuickImage.Get("AZ|16|8|1"), ContextMenu_ResetSort, null, true));
                e.ContextMenu.Add(ItemOf("Nach dieser Spalte aufsteigend sortieren", QuickImage.Get("AZ|16|8"), ContextMenu_SortAZ, column, true));
                e.ContextMenu.Add(ItemOf("Nach dieser Spalte absteigend sortieren", QuickImage.Get("ZA|16|8"), ContextMenu_SortZA, column, true));
            }

            #endregion

            #region Zelle

            if (_mouseOverColumn is { IsDisposed: false } column2 && _mouseOverRow is { IsDisposed: false } row2) {
                var editable = string.IsNullOrEmpty(CellCollection.IsCellEditable(column2, row2, row2.ChunkValue));

                e.ContextMenu.Add(ItemOf("Zelle", true));
                e.ContextMenu.Add(ItemOf("Inhalt kopieren", ImageCode.Kopieren, ContextMenu_ContentCopy, new { Column = column2, Row = row2 }, column2.CanBeChangedByRules()));
                e.ContextMenu.Add(ItemOf("Inhalt einfügen", ImageCode.Clipboard, ContextMenu_ContentPaste, null, editable && column2.CanBeChangedByRules()));
                e.ContextMenu.Add(ItemOf("Inhalt löschen", ImageCode.Radiergummi, ContextMenu_ContentDelete, new { Column = column2, Row = row2 }, editable && column2.CanBeChangedByRules()));
                e.ContextMenu.Add(ItemOf("Vorherigen Inhalt wiederherstellen", QuickImage.Get(ImageCode.Undo, 16), ContextMenu_RestorePreviousContent, new { Column = column2, Row = row2 }, editable && column2.CanBeChangedByRules() && column2.SaveContent));
                e.ContextMenu.Add(ItemOf("Suchen und ersetzen", QuickImage.Get(ImageCode.Lupe, 16), ContextMenu_SearchAndReplace, null, tb.IsAdministrator()));
                e.ContextMenu.Add(ItemOf("Zeilenschlüssel kopieren", ImageCode.Schlüssel, ContextMenu_KeyCopy, row2, tb.IsAdministrator()));
            }

            #endregion

            #region Spalte

            if (_mouseOverColumn is { IsDisposed: false } column3) {
                e.ContextMenu.Add(ItemOf("Spalte", true));
                e.ContextMenu.Add(ItemOf("Spalteneigenschaften bearbeiten", ImageCode.Stift, ContextMenu_EditColumnProperties, new { Column = column3, Row = _mouseOverRow, View = this }, tb.IsAdministrator()));
                e.ContextMenu.Add(ItemOf("Gesamten Spalteninhalt kopieren", ImageCode.Clipboard, ContextMenu_CopyAll, column3, tb.IsAdministrator()));
                e.ContextMenu.Add(ItemOf("Gesamten Spalteninhalt kopieren + sortieren", ImageCode.Clipboard, ContextMenu_CopyAllSorted, column3, tb.IsAdministrator()));
                e.ContextMenu.Add(ItemOf("Statistik", QuickImage.Get(ImageCode.Balken, 16), ContextMenu_Statistics, column3, tb.IsAdministrator()));
                e.ContextMenu.Add(ItemOf("Summe", ImageCode.Summe, ContextMenu_Sum, column3, tb.IsAdministrator()));

                if (_mouseOverRow is { IsDisposed: false } row3) {
                    var editable = string.IsNullOrEmpty(CellCollection.IsCellEditable(column3, row3, row3.ChunkValue));
                    e.ContextMenu.Add(ItemOf("Voting", ImageCode.Herz, ContextMenu_Voting, column3, tb.IsAdministrator() && editable && column3.CanBeChangedByRules()));
                }
            }

            #endregion

            #region Zeile

            if (_mouseOverRow is { IsDisposed: false } row4) {
                e.ContextMenu.Add(ItemOf("Zeile", true));
                e.ContextMenu.Add(ItemOf("Zeile löschen", QuickImage.Get(ImageCode.Kreuz, 16), ContextMenu_DeleteRow, row4, tb.IsAdministrator() && tb.IsThisScriptBroken(ScriptEventTypes.row_deleting, true)));
                e.ContextMenu.Add(ItemOf("Komplette Datenüberprüfung", QuickImage.Get(ImageCode.HäkchenDoppelt, 16), ContextMenu_DataValidation, row4, tb.CanDoValueChangedScript(true)));

                var didmenu = false;
                foreach (var thiss in tb.EventScript) {
                    if (thiss is { UserGroups.Count: > 0 } && tb.PermissionCheck(thiss.UserGroups, null) && thiss.NeedRow && thiss.IsOk()) {
                        if (!didmenu) {
                            e.ContextMenu.Add(ItemOf("Skripte", true));
                            didmenu = true;
                        }
                        e.ContextMenu.Add(ItemOf("Skript: " + thiss.ReadableText(), thiss.SymbolForReadableText(), ContextMenu_ExecuteScript, new { Script = thiss, Row = row4 }, thiss.IsOk()));
                    }
                }
            }

            #endregion
        }

        OnContextMenuInit(e);
    }

    public string GrantWriteAccess(ColumnViewItem? cellInThisTableColumn, RowListItem? cellInThisTableRow, string newChunkVal) {
        var f = IsCellEditable(cellInThisTableColumn, cellInThisTableRow, newChunkVal, true);
        return !string.IsNullOrWhiteSpace(f)
            ? f
            : Table.GrantWriteAccess(cellInThisTableColumn?.Column, cellInThisTableRow?.Row, newChunkVal, 2, false);
    }

    public void ImportBtb() {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }
        ImportBtb(tb);
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
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }
        ImportCsv(tb, csvtxt);
    }

    public string IsCellEditable(ColumnViewItem? cellInThisTableColumn, RowListItem? cellInThisTableRow, string? newChunkVal, bool maychangeview) {
        var f = CellCollection.IsCellEditable(cellInThisTableColumn?.Column, cellInThisTableRow?.Row, newChunkVal);
        if (!string.IsNullOrWhiteSpace(f)) { return f; }

        if (CurrentArrangement is not { IsDisposed: false } ca || !ca.Contains(cellInThisTableColumn)) {
            return "Ansicht veraltet";
        }

        if (cellInThisTableColumn == null) {
            return "Keine Spalte angekommen.";
        }

        //var visCanvasArea = AvailableControlPaintArea.ControlToCanvas(Zoom, OffsetX, OffsetY).ToRect();

        if (cellInThisTableRow != null) {
            if (maychangeview && !EnsureVisible(cellInThisTableColumn, cellInThisTableRow)) {
                return "Zelle konnte nicht angezeigt werden.";
            }

            //var realHead = cellInThisTableColumn.RealHead(Zoom, OffsetX);
            //if (realHead.Right < 0 || realHead.Left > DisplayRectangle.Width) {
            //    return "Spalte konnte nicht angezeigt werden.";
            //}

            if (!cellInThisTableRow.IsVisible(AvailableControlPaintArea, Zoom, OffsetX, OffsetY)) {
                return "Die Zeile wird nicht angezeigt.";
            }
        } else {
            if (maychangeview && !EnsureVisible(cellInThisTableColumn)) {
                return "Zelle konnte nicht angezeigt werden.";
            }
        }

        return string.Empty;
    }

    public void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);

    public void OpenScriptEditor() {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        var se = IUniqueWindowExtension.ShowOrCreate<TableScriptEditor>(tb);
        se.Row = CursorPosRow?.Row;
    }

    public void OpenSearchAndReplaceInCells() {
        if (TableViewForm.EditabelErrorMessage(Table) || Table == null) { return; }

        if (!Table.IsAdministrator()) { return; }

        IUniqueWindowExtension.ShowOrCreate<SearchAndReplaceInCells>(this);
    }

    public void OpenSearchAndReplaceInTbScripts() {
        if (TableViewForm.EditabelErrorMessage(Table) || Table == null) { return; }
        if (!IsAdministrator()) { return; }

        IUniqueWindowExtension.ShowOrCreate<SearchAndReplaceInTbScripts>(null);
    }

    public void OpenSearchInCells() => IUniqueWindowExtension.ShowOrCreate<OpenSearchInCells>(this);

    public override void ParseView(string toParse) {
        ResetView();

        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        if (!string.IsNullOrEmpty(toParse) && toParse.GetAllTags() is { } x) {
            foreach (var pair in x) {
                switch (pair.Key) {
                    case "arrangement":
                        Arrangement = pair.Value.FromNonCritical();
                        break;

                    case "arrangementnr":
                        break;

                    case "filters":
                        Filter.PropertyChanged -= Filter_PropertyChanged;
                        Filter.Table = Table;
                        var code = pair.Value.FromNonCritical();
                        Filter.Clear();
                        Filter.Parse(code);
                        Filter.ParseFinished(code);
                        Filter.PropertyChanged += Filter_PropertyChanged;
                        DoFilterCombined();
                        break;

                    case "cursorpos":
                        tb.Cell.DataOfCellKey(pair.Value.FromNonCritical(), out var column, out var row);
                        CursorPos_Set(CurrentArrangement?[column], GetRow(row), false);
                        break;

                    case "tempsort":
                        _sortDefinitionTemporary = new RowSortDefinition(Table, pair.Value.FromNonCritical());
                        break;

                    case "pin":
                        foreach (var thisk in pair.Value.FromNonCritical().SplitBy("|")) {
                            var r = tb.Row.GetByKey(thisk);
                            if (r is { IsDisposed: false }) { PinnedRows.Add(r); }
                        }

                        break;

                    case "collapsed":
                        var t = pair.Value.FromNonCritical().SplitAndCutBy("|");
                        CollapseThis(t);
                        break;

                    case "reduced":
                        var cols = pair.Value.FromNonCritical().SplitBy("|");
                        CurrentArrangement?.Reduce(cols);
                        break;

                    case "zoom":
                        Zoom = FloatParse(pair.Value.FromNonCritical());
                        break;
                }
            }
        }

        base.ParseView(toParse);

        CheckView();
    }

    public void Pin(IReadOnlyList<RowItem>? rows) {
        // Arbeitet mit Rows, weil nur eine Anpinngug möglich ist

        rows ??= [];

        rows = [.. rows.Distinct()];
        if (!rows.IsDifferentTo(PinnedRows)) { return; }

        PinnedRows.Clear();
        PinnedRows.AddRange(rows);
        Invalidate_AllViewItems(false);
        OnPinnedChanged();
    }

    public void PinAdd(RowItem? row) {
        if (row is not { IsDisposed: false }) { return; }
        PinnedRows.Add(row);
        Invalidate_AllViewItems(false);
        OnPinnedChanged();
    }

    public void PinRemove(RowItem? row) {
        if (row is not { IsDisposed: false }) { return; }
        PinnedRows.Remove(row);
        Invalidate_AllViewItems(false);
        OnPinnedChanged();
    }

    public void ResetView() {
        Filter.Clear();
        FilterCombined.Clear();

        PinnedRows.Clear();

        Invalidate_AllViewItems(true);

        QuickInfo = string.Empty;
        _sortDefinitionTemporary = null;
        CursorPosColumn = null;
        CursorPosRow = null;
        _arrangement = string.Empty;
        Zoom = 1f;

        OnViewChanged();
    }

    public IReadOnlyList<RowItem> RowsVisibleUnique() => _rowsVisibleUnique;

    public void TableSet(Table? tb, string viewCode) {
        if (Table == tb && string.IsNullOrEmpty(viewCode)) { return; }

        CloseAllComponents();

        if (Table is { IsDisposed: false } tb1) {
            // auch Disposed Tabellen die Bezüge entfernen!
            tb1.Cell.CellValueChanged -= _Table_CellValueChanged;
            tb1.Loaded -= _Table_TableLoaded;
            tb1.Loading -= _Table_StoreView;
            tb1.ViewChanged -= _Table_ViewChanged;
            tb1.SortParameterChanged -= _Table_SortParameterChanged;
            tb1.Row.RowRemoving -= Row_RowRemoving;
            tb1.Row.RowRemoved -= Row_RowRemoved;
            tb1.Column.ColumnRemoving -= Column_ItemRemoving;
            tb1.Column.ColumnRemoved -= _Table_ViewChanged;
            tb1.Column.ColumnAdded -= _Table_ViewChanged;
            tb1.ProgressbarInfo -= _Table_ProgressbarInfo;
            tb1.DisposingEvent -= _table_Disposing;
            tb1.InvalidateView -= Table_InvalidateView;
            SaveAll(false);
            MultiUserFile.SaveAll(false);
        }
        ShowWaitScreen = true;
        Refresh(); // um die Uhr anzuzeigen
        Table = tb;
        Invalidate_CurrentArrangement();
        Invalidate_AllViewItems(true);
        Filter.Table = tb;
        FilterCombined.Table = tb;
        FilterFix.Table = tb;

        _tableDrawError = null;
        //InitializeSkin(); // Neue Schriftgrößen
        if (Table is { IsDisposed: false } tb2) {
            RepairColumnArrangements(tb2);

            tb2.Cell.CellValueChanged += _Table_CellValueChanged;
            tb2.Loaded += _Table_TableLoaded;
            tb2.Loading += _Table_StoreView;
            tb2.ViewChanged += _Table_ViewChanged;
            tb2.SortParameterChanged += _Table_SortParameterChanged;
            tb2.Row.RowRemoving += Row_RowRemoving;
            tb2.Row.RowRemoved += Row_RowRemoved;
            tb2.Column.ColumnAdded += _Table_ViewChanged;
            tb2.Column.ColumnRemoving += Column_ItemRemoving;
            tb2.Column.ColumnRemoved += _Table_ViewChanged;
            tb2.ProgressbarInfo += _Table_ProgressbarInfo;
            tb2.DisposingEvent += _table_Disposing;
            tb2.InvalidateView += Table_InvalidateView;
        }

        ParseView(viewCode);

        ShowWaitScreen = false;

        // Aktualisiere den Status der Steuerelemente
        OnEnabledChanged(System.EventArgs.Empty);

        OnTableChanged();
    }

    public ColumnViewItem? View_ColumnFirst() => IsDisposed || Table is not { IsDisposed: false } ? null : CurrentArrangement is { Count: not 0 } ca ? ca[0] : null;

    public RowListItem? View_NextRow(RowListItem? row) {
        if (IsDisposed || Table is not { IsDisposed: false }) { return null; }
        if (row is not { IsDisposed: false }) { return null; }

        if (AllViewItems is not { } avi) { return null; }

        var currentBottom = row.CanvasPosition.Bottom;

        RowListItem? closestRow = null;
        var minDistance = double.MaxValue;

        foreach (var thisItem in avi.Values) {
            if (thisItem is RowListItem rli && thisItem.Visible && thisItem.CanvasPosition.Top >= currentBottom) {
                var distance = thisItem.CanvasPosition.Top - currentBottom;
                if (distance < minDistance) {
                    minDistance = distance;
                    closestRow = rli;
                }
            }
        }

        return closestRow;
    }

    public RowListItem? View_PreviousRow(RowListItem? row) {
        if (IsDisposed || Table is not { IsDisposed: false }) { return null; }
        if (row is not { IsDisposed: false }) { return null; }

        if (AllViewItems is not { } avi) { return null; }

        var currentTop = row.CanvasPosition.Top;

        RowListItem? closestRow = null;
        var minDistance = double.MaxValue;

        foreach (var thisItem in avi.Values) {
            if (thisItem is RowListItem rli && thisItem.Visible && thisItem.CanvasPosition.Bottom <= currentTop) {
                var distance = currentTop - thisItem.CanvasPosition.Bottom;
                if (distance < minDistance) {
                    minDistance = distance;
                    closestRow = rli;
                }
            }
        }

        return closestRow;
    }

    public RowListItem? View_RowFirst() {
        if (IsDisposed || Table is not { IsDisposed: false }) { return null; }

        if (AllViewItems is not { } avi) { return null; }

        RowListItem? firstRow = null;
        var minTop = int.MaxValue;

        foreach (var thisItem in avi.Values) {
            if (thisItem is RowListItem rli && thisItem.Visible) {
                var top = thisItem.CanvasPosition.Top;
                if (top < minTop) {
                    minTop = top;
                    firstRow = rli;
                }
            }
        }

        return firstRow;
    }

    public RowListItem? View_RowLast() {
        if (IsDisposed || Table is not { IsDisposed: false }) { return null; }

        if (AllViewItems is not { } avi) { return null; }

        RowListItem? lastRow = null;
        var maxBottom = int.MinValue;

        foreach (var thisItem in avi.Values) {
            if (thisItem is RowListItem rli && thisItem.Visible) {
                var bottom = thisItem.CanvasPosition.Bottom;
                if (bottom > maxBottom) {
                    maxBottom = bottom;
                    lastRow = rli;
                }
            }
        }

        return lastRow;
    }

    public override List<string> ViewToString() {
        List<string> result = [];
        result.ParseableAdd("Arrangement", _arrangement);
        result.ParseableAdd("Filters", (IStringable?)Filter);
        result.ParseableAdd("Pin", PinnedRows, false);
        result.ParseableAdd("Collapsed", _collapsed, false);
        result.ParseableAdd("Reduced", CurrentArrangement?.ReducedColumns(), false);
        result.ParseableAdd("TempSort", _sortDefinitionTemporary);
        result.ParseableAdd("CursorPos", CellCollection.KeyOfCell(CursorPosColumn?.Column, CursorPosRow?.Row));
        result.AddRange(base.ViewToString());
        return result;
    }

    internal static Renderer_Abstract RendererOf(ColumnViewItem columnViewItem, string style) {
        if (!string.IsNullOrEmpty(columnViewItem.Renderer)) {
            var renderer = ParseableItem.NewByTypeName<Renderer_Abstract>(columnViewItem.Renderer);
            if (renderer == null) { return RendererOf(columnViewItem.Column, style); }

            renderer.Parse(columnViewItem.RendererSettings);

            return renderer;
        }

        return RendererOf(columnViewItem.Column, style);
    }

    internal static bool RepairColumnArrangements(Table tb) {
        if (!tb.IsEditable(false)) { return false; }

        var tcvc = ColumnViewCollection.ParseAll(tb);

        for (var z = 0; z < Math.Max(2, tcvc.Count); z++) {
            if (tcvc.Count < z + 1) { tcvc.Add(new ColumnViewCollection(tb, string.Empty)); }
            ColumnViewCollection.Repair(tcvc[z], z);
        }

        //var i = tb.Column.IndexOf(tcvc[0][0].Column.KeyName);

        //if(i!= 0) {
        //    Develop.DebugPrint(ErrorType.Warning, "Spalte 0 nicht auf erster CanvasPosition!");

        //    tb.Column.RemoveAt

        //    Generic.Swap(tb.Column[0], tb.Column[i]);
        //}

        var n = tcvc.ToString(false);

        if (n != tb.ColumnArrangements) {
            tb.ColumnArrangements = n;
            return true;
        }

        return false;
    }

    internal void EnsureVisibleX(int controlX) {
        if (CurrentArrangement is not { } ca) { return; }

        var controlLeft = ca.ControlColumnsPermanentWidth;
        var controlWidth = AvailableControlPaintArea.Right; // Bottom = Height

        if (controlX < controlLeft) {
            OffsetX = OffsetX - controlX + controlLeft;
        } else if (controlX > controlWidth) {
            OffsetX = OffsetX - controlX + controlWidth;
        }
    }

    internal void EnsureVisibleY(int controlY) {
        if (IsDisposed || Table is not { IsDisposed: false }) { return; }

        if (AllViewItems is not { } avi) { return; }

        var maxBottom = 0;

        foreach (var thisItem in avi.Values) {
            if (thisItem.Visible && thisItem.IgnoreYOffset) {
                maxBottom = Math.Max(thisItem.CanvasPosition.Bottom, maxBottom);
            }
        }

        var controlTop = maxBottom.CanvasToControl(Zoom);

        var controlHeight = AvailableControlPaintArea.Bottom; // Bottom = Height

        if (controlY < controlTop) {
            OffsetY = OffsetY - controlY + controlTop;
        } else if (controlY > controlHeight) {
            OffsetY = OffsetY - controlY + controlHeight;
        }
    }

    internal void RowCleanUp() {
        if (IsDisposed || Table is not { IsDisposed: false }) { return; }
        var l = new RowCleanUp(this);
        l.Show();
    }

    protected override RectangleF CalculateCanvasMaxBounds() {
        var x = AvailableControlPaintArea.Width;
        var y = AvailableControlPaintArea.Height;

        if (CurrentArrangement is { } ca) {
            x = (int)ca.ControlColumnsWidth.ControlToCanvas(Zoom);
        }

        if (AllViewItems is { } avi) {
            (_, _, y, _) = avi.Values.ToList().CanvasItemData(Design.Item_Listbox);
        }

        return new RectangleF(0, 0, x + 8, y + 8);
    }

    //UserControl überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
    [DebuggerNonUserCode]
    protected override void Dispose(bool disposing) {
        try {
            if (disposing) {
                Filter.RowsChanged -= FilterAny_RowsChanged;
                Filter.PropertyChanged -= Filter_PropertyChanged;
                FilterCombined.RowsChanged -= FilterAny_RowsChanged;
                FilterCombined.PropertyChanged -= FilterCombined_PropertyChanged;
                FilterFix.PropertyChanged -= FilterFix_PropertyChanged;
                TableSet(null, string.Empty); // Wichtig (nicht _Table) um Events zu lösen
                _pg?.Dispose();
            }
        } finally {
            base.Dispose(disposing);
        }
    }

    protected override void DrawControl(Graphics gr, States state) {
        if (IsDisposed) { return; }

        if (InvokeRequired) {
            Invoke(new Action(() => DrawControl(gr, state)));
            return;
        }
        base.DrawControl(gr, state);

        // Haupthintergrund der gesamten Tabelle zeichnen
        Skin.Draw_Back(gr, Design.Table_And_Pad, state, base.DisplayRectangle, this, true);

        if (_tableDrawError is { } dt) {
            if (DateTime.UtcNow.Subtract(dt).TotalSeconds < 60) {
                DrawWaitScreen(gr, string.Empty);
                return;
            }
            _tableDrawError = null;
        }

        //// Listboxen bekommen keinen Focus, also Tabellen auch nicht. Basta.
        //if (state.HasFlag(States.Standard_HasFocus)) {
        //    state ^= States.Standard_HasFocus;
        //}

        if (Table is not { IsDisposed: false } tb) {
            DrawWaitScreen(gr, "Keine Tabelle geladen.");
            return;
        }

        tb.LastUsedDate = DateTime.UtcNow;

        if (DesignMode || ShowWaitScreen) {
            DrawWaitScreen(gr, string.Empty);
            return;
        }

        if (CurrentArrangement is not { IsDisposed: false } ca || ca.Count < 1) {
            DrawWaitScreen(gr, "Aktuelle Ansicht fehlerhaft");
            return;
        }

        if (!FilterCombined.IsOk()) {
            DrawWaitScreen(gr, FilterCombined.ErrorReason());
            return;
        }

        if (FilterCombined.Table != null && Table != FilterCombined.Table) {
            DrawWaitScreen(gr, "Filter fremder Tabelle: " + FilterCombined.Table.Caption);
            return;
        }

        if (AllViewItems is not { } avi) {
            DrawWaitScreen(gr, "Fehler der angezeigten Zeilen");
            return;
        }

        if (state.HasFlag(States.Standard_Disabled)) { CursorPos_Reset(); }

        ca.SheetStyle = SheetStyle;
        ca.ComputeAllColumnPositions(AvailableControlPaintArea.Width, Zoom);

        // Haupt-Aufbau-Routine ------------------------------------

        //var t = sortedRowData.CanvasItemData(Design.Item_Listbox);
        avi.Values.ToList().DrawItems(gr, AvailableControlPaintArea, null, OffsetX, OffsetY, string.Empty, state, Design.Table_And_Pad, Design.Item_Listbox, Design.Undefiniert, null, Zoom);

        // Rahmen um die gesamte Tabelle zeichnen
        Skin.Draw_Border(gr, Design.Table_And_Pad, state, base.DisplayRectangle);
    }

    protected override bool IsInputKey(Keys keyData) {
        // Ganz wichtig diese Routine!
        // Wenn diese NICHT ist, geht der Fokus weg, sobald der cursor gedrückt wird.
        switch (keyData) {
            case Keys.Up or Keys.Down or Keys.Left or Keys.Right:
                return true;

            default:
                return false;
        }
    }

    protected override void OnDoubleClick(CanvasMouseEventArgs e) {
        //    base.OnDoubleClick(e); Wird komplett selbst gehandlet und das neue Ereignis ausgelöst
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        if (CurrentArrangement is not { IsDisposed: false } ca) { return; }

        lock (_lockUserAction) {
            var (_mouseOverColumn, _mouseOverRow) = CellOnCoordinate(ca, e);
            if (_mouseOverRow is not { }) { return; }

            if (_isinDoubleClick) { return; }
            _isinDoubleClick = true;

            var ea = new CellExtEventArgs(_mouseOverColumn, _mouseOverRow as RowListItem);
            DoubleClick?.Invoke(this, ea);

            if (_mouseOverRow is RowListItem rli) {
                Cell_Edit(_mouseOverColumn, rli, true, rli.Row.ChunkValue);
            } else if (_mouseOverRow is NewRowListItem nrli) {
                if (tb.Column.ChunkValueColumn == tb.Column.First) {
                    Cell_Edit(_mouseOverColumn, nrli, true, null);
                } else {
                    Cell_Edit(_mouseOverColumn, nrli, true, FilterCombined.ChunkVal);
                }
            }

            _isinDoubleClick = false;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e) {
        base.OnKeyDown(e);

        if (IsDisposed || Table is not { IsDisposed: false }) { return; }
        if (CursorPosColumn?.Column is not { IsDisposed: false } c) { return; }
        if (CursorPosRow?.Row is not { IsDisposed: false }) { return; }

        if (CurrentArrangement is not { IsDisposed: false }) { return; }

        lock (_lockUserAction) {
            if (_isinKeyDown) { return; }
            _isinKeyDown = true;

            Develop.SetUserDidSomething();

            //_table.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());

            // var chunkval = r.ChunkValue;

            switch (e.KeyCode) {
                //case Keys.Oemcomma: // normales ,
                //    if (e.Modifiers == Keys.Control) {
                //        var lp = EditableErrorReason(CursorPosColumn, CursorPosRow, EditableErrorReasonType.EditCurrently, true, false, true, chunkval);
                //        Neighbour(CursorPosColumn, CursorPosRow, Direction.Oben, out _, out var newRow);
                //        if (newRow == CursorPosRow) { lp = "Das geht nicht bei dieser Zeile."; }
                //        if (string.IsNullOrEmpty(lp) && newRow?.Row != null) {
                //            UserEdited(this, newRow.Row.CellGetString(c), CursorPosColumn, CursorPosRow, true, oldval);
                //        } else {
                //            NotEditableInfo(lp);
                //        }
                //    }
                //    break;

                case Keys.X:
                    if (e.Modifiers == Keys.Control) {
                        CopyToClipboard(c, CursorPosRow?.Row, true);
                        NotEditableInfo(UserEdited(this, string.Empty, CursorPosColumn, CursorPosRow, true));
                    }
                    break;

                case Keys.Delete:
                    NotEditableInfo(UserEdited(this, string.Empty, CursorPosColumn, CursorPosRow, true));
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
                    CursorPos_Reset();
                    break;

                case Keys.PageUp: //Bildab
                    CursorPos_Reset();
                    break;

                case Keys.Home:
                    CursorPos_Reset();
                    break;

                case Keys.End:
                    CursorPos_Reset();
                    break;

                case Keys.C:
                    if (e.Modifiers == Keys.Control) {
                        CopyToClipboard(c, CursorPosRow?.Row, true);
                    }
                    break;

                case Keys.F:
                    if (e.Modifiers == Keys.Control) {
                        OpenSearchInCells();
                    }
                    break;

                case Keys.F2:
                    Cell_Edit(CursorPosColumn, CursorPosRow, true, CursorPosRow?.Row.ChunkValue ?? FilterCombined.ChunkVal);
                    break;

                case Keys.V:
                    if (e.Modifiers == Keys.Control) {
                        PasteToCursor();
                    }
                    break;
            }
            _isinKeyDown = false;
        }
    }

    protected override void OnMouseDown(CanvasMouseEventArgs e) {
        base.OnMouseDown(e);
        if (IsDisposed || Table is not { IsDisposed: false }) { return; }

        if (CurrentArrangement is not { IsDisposed: false } ca) { return; }

        lock (_lockUserAction) {
            if (_isinMouseDown) { return; }
            _isinMouseDown = true;
            //_table.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
            var (_mouseOverColumn, _mouseOverRow) = CellOnCoordinate(ca, e);
            // Die beiden Befehle nur in Mouse Down!
            // Wenn der Cursor bei Click/Up/Down geändert wird, wird ein Ereignis ausgelöst.
            // Das könnte auch sehr Zeit intensiv sein. Dann kann die Maus inzwischen wo ander sein.
            // Somit würde das Ereignis doppelt und dreifach ausgelöste werden können.
            // Beipiel: MouseDown-> Bildchen im Pad erzeugen, dauert.... Maus bewegt sich
            //          MouseUp  -> Cursor wird umgesetzt, Ereginis CursorChanged wieder ausgelöst, noch ein Bildchen
            if (_mouseOverRow is RowCaptionListItem rcli) {
                CursorPos_Reset(); // Wenn eine Zeile markiert ist, man scrollt und expandiert, springt der Screen zurück, was sehr irriteiert

                rcli.IsExpanded = !rcli.IsExpanded;

                Invalidate_AllViewItems(false);
            }
            EnsureVisible(_mouseOverColumn, _mouseOverRow);
            CursorPos_Set(_mouseOverColumn, _mouseOverRow, false);
            Develop.SetUserDidSomething();
            _isinMouseDown = false;
        }
    }

    protected override void OnMouseMove(CanvasMouseEventArgs e) {
        base.OnMouseMove(e);

        lock (_lockUserAction) {
            if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }
            if (CurrentArrangement is not { IsDisposed: false } ca) { return; }

            if (_isinMouseMove) { return; }

            _isinMouseMove = true;

            var (_mouseOverColumn, _mouseOverRowItem) = CellOnCoordinate(ca, e);

            if (_mouseOverColumn is { IsDisposed: false } &&
                _mouseOverRowItem is RowBackgroundListItem { } rbi &&
                e.Button == MouseButtons.None) {
                QuickInfo = rbi.QuickInfoForColumn(_mouseOverColumn);
            } else {
                QuickInfo = string.Empty;
            }

            _isinMouseMove = false;
        }
    }

    protected override void OnMouseUp(CanvasMouseEventArgs e) {
        if (IsDisposed) { return; }
        base.OnMouseUp(e);

        lock (_lockUserAction) {
            if (Table is not { IsDisposed: false } || CurrentArrangement is not { IsDisposed: false } ca) {
                return;
            }

            var (_mouseOverColumn, _mouseOverRowItem) = CellOnCoordinate(ca, e);
            var _mouseOverRow = _mouseOverRowItem as RowListItem;

            // TXTBox_Close() NICHT! Weil sonst nach dem Öffnen sofort wieder gschlossen wird
            // AutoFilter_Close() NICHT! Weil sonst nach dem Öffnen sofort wieder geschlossen wird
            FloatingForm.Close(this, Design.Form_KontextMenu);

            if (_mouseOverColumn?.Column is not { IsDisposed: false } column) { return; }

            if (e.Button == MouseButtons.Left) {
                if (_mouseOverRowItem is FilterBarListItem cfli) {
                    var screenX = Cursor.Position.X - e.ControlX;
                    var screenY = Cursor.Position.Y - e.ControlY;
                    AutoFilter_Show(ca, _mouseOverColumn, screenX, screenY, cfli.ControlPosition(Zoom, OffsetX, OffsetY).Bottom);
                    return;
                }

                if (_mouseOverRowItem is CollapesBarListItem && _mouseOverColumn.CollapsableEnabled()) {
                    _mouseOverColumn.IsExpanded = !_mouseOverColumn.IsExpanded;
                    Invalidate_AllViewItems(false);
                    return;
                }

                if (_mouseOverRow?.Row is { IsDisposed: false } r) {
                    OnCellClicked(new CellEventArgs(column, r));
                    Invalidate();
                }
            }

            if (e.Button == MouseButtons.Right) {
                FloatingInputBoxListBoxStyle.ContextMenuShow(this, new { _mouseOverColumn?.Column, _mouseOverRow?.Row }, e.ToMouseEventArgs());
            }
        }
    }

    protected override void OnOffsetXChanged() {
        base.OnOffsetXChanged();

        //Invalidate_CurrentArrangement();

        CloseAllComponents();
    }

    protected override void OnOffsetYChanged() {
        base.OnOffsetYChanged();

        CloseAllComponents();
    }

    protected override void OnSizeChanged(System.EventArgs e) {
        base.OnSizeChanged(e);
        if (IsDisposed || Table is not { IsDisposed: false }) { return; }
        lock (_lockUserAction) {
            if (_isinSizeChanged) { return; }
            _isinSizeChanged = true;

            Invalidate_CurrentArrangement();
            Invalidate_AllViewItems(false); // Zellen können ihre Größe ändern. z.B. die Zeilenhöhe
                                            //CurrentArrangement?.Invalidate_DrawWithOfAllItems();

            _isinSizeChanged = false;
        }
    }

    protected override void OnZoomChanged() {
        Invalidate_CurrentArrangement();
        base.OnZoomChanged();
    }

    private static void NotEditableInfo(string reason) {
        if (string.IsNullOrEmpty(reason)) { return; }
        Notification.Show(LanguageTool.DoTranslate(reason), ImageCode.Kreuz);
    }

    private static string UserEdited(TableView table, string newValue, ColumnViewItem? cellInThisTableColumn, RowListItem? cellInThisTableRow, bool formatWarnung) {
        if (cellInThisTableColumn?.Column is not { IsDisposed: false } contentHolderCellColumn) { return "Spalte nicht vorhanden"; } // Dummy prüfung

        #region Den wahren Zellkern finden contentHolderCellColumn, contentHolderCellRow

        var contentHolderCellRow = cellInThisTableRow?.Row;
        if (contentHolderCellRow is { IsDisposed: false } && contentHolderCellColumn.RelationType == RelationType.CellValues) {
            (contentHolderCellColumn, contentHolderCellRow, _, _) = contentHolderCellRow.LinkedCellData(contentHolderCellColumn, true, true);
            if (contentHolderCellColumn == null || contentHolderCellRow == null) { return "Spalte/Zeile nicht vorhanden"; } // Dummy prüfung
        }

        #endregion

        #region Format prüfen

        if (formatWarnung && !string.IsNullOrEmpty(newValue)) {
            if (!newValue.IsFormat(contentHolderCellColumn)) {
                if (Forms.MessageBox.Show("Ihre Eingabe entspricht<br><u>nicht</u> dem erwarteten Format!<br><br>Trotzdem übernehmen?", ImageCode.Information, "Ja", "Nein") != 0) {
                    return "Abbruch, da das erwartete Format nicht eingehalten wurde.";
                }
            }
        }

        #endregion

        newValue = contentHolderCellColumn.AutoCorrect(newValue, false);

        #region neue Zeile anlegen? (Das ist niemals in der ein LinkedCell-Tabelle)

        if (cellInThisTableRow == null) {
            if (string.IsNullOrEmpty(newValue)) { return string.Empty; }
            if (cellInThisTableColumn.Column?.Table is not { IsDisposed: false } tb) { return "Tabelle verworfen"; }
            if (table.Table?.Column.First is not { IsDisposed: false } colfirst) { return "Keine Erstspalte definiert."; }

            using var filterColNewRow = new FilterCollection(table.Table, "Edit-Filter");
            filterColNewRow.AddIfNotExists([.. table.FilterCombined]);
            filterColNewRow.RemoveOtherAndAdd(new FilterItem(colfirst, FilterType.Istgleich, newValue));

            var newChunkVal = filterColNewRow.ChunkVal;
            var fe = table.GrantWriteAccess(cellInThisTableColumn, null, newChunkVal);
            if (!string.IsNullOrEmpty(fe)) { return fe; }

            var (newrow, message, _) = tb.Row.GenerateAndAdd([.. filterColNewRow], "Neue Zeile über Tabellen-Ansicht");

            if (!string.IsNullOrEmpty(message)) { return message; }

            var l = table.FilterCombined.Rows;
            if (newrow != null && !l.Contains(newrow)) {
                if (Forms.MessageBox.Show("Die neue Zeile ist ausgeblendet.<br>Soll sie <b>angepinnt</b> werden?", ImageCode.Pinnadel, "anpinnen", "abbrechen") == 0) {
                    table.PinAdd(newrow);
                }
            }

            if (newrow != null) {
                var rd = table.GetRow(newrow);
                table.CursorPos_Set(table.View_ColumnFirst(), rd, true);
            }

            return string.Empty;
        }

        #endregion

        if (contentHolderCellRow != null) {
            var oldval = contentHolderCellRow.CellGetString(contentHolderCellColumn);

            if (newValue == oldval) { return string.Empty; }

            var newChunkVal = cellInThisTableRow.Row.ChunkValue;

            if (cellInThisTableColumn.Column == cellInThisTableColumn.Column.Table?.Column.ChunkValueColumn) {
                newChunkVal = newValue;
            }

            var check1 = table.GrantWriteAccess(cellInThisTableColumn, cellInThisTableRow, newChunkVal);
            if (!string.IsNullOrEmpty(check1)) { return check1; }

            contentHolderCellRow.CellSet(contentHolderCellColumn, newValue, "Benutzerbearbeitung in Tabellenansicht");

            if (contentHolderCellColumn.SaveContent) {
                contentHolderCellRow.UpdateRow(true, "Nach Benutzereingabe");
            } else {
                // Variablen sind en nicht im Script enthalten, also nur die schnelle Berechnung
                contentHolderCellRow.InvalidateCheckData();
                contentHolderCellRow.CheckRow();
            }

            if (table.Table == cellInThisTableColumn.Column?.Table) { table.CursorPos_Set(cellInThisTableColumn, cellInThisTableRow, false); }
        }

        return string.Empty;
    }

    private void _Table_CellValueChanged(object sender, CellEventArgs e) {
        if (e.Row.IsDisposed || e.Column.IsDisposed) { return; }

        if (SortUsed() is { } rsd) {
            if (rsd.UsedForRowSort(e.Column) || e.Column == Table?.Column.SysChapter) {
                Invalidate_AllViewItems(false);
            }
        }

        if (e.Column.MultiLine) {
            if (CurrentArrangement is { IsDisposed: false } ca) {
                if (ca[e.Column] is { IsDisposed: false }) {
                    Invalidate_AllViewItems(false); // Zeichenhöhe kann sich ändern...
                }

                //cv.Invalidate_CanvasContentWidth(); // Kann auf sich selbst aufpassen
            }
        }

        Invalidate();
    }

    private void _table_Disposing(object sender, System.EventArgs e) => TableSet(null, string.Empty);

    private void _Table_ProgressbarInfo(object sender, ProgressbarEventArgs e) {
        if (IsDisposed) { return; }
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

    private void _Table_SortParameterChanged(object sender, System.EventArgs e) => Invalidate_AllViewItems(false);

    private void _Table_StoreView(object sender, System.EventArgs e) =>
                //if (!string.IsNullOrEmpty(_StoredView)) { Develop.DebugPrint("Stored View nicht Empty!"); }
                _storedView = ViewToString().FinishParseable();

    private void _Table_TableLoaded(object sender, FirstEventArgs e) {
        if (IsDisposed) { return; }
        // Wird auch bei einem Reload ausgeführt.
        // Es kann aber sein, dass eine Ansicht zurückgeholt wurde, und die Werte stimmen.
        // Deswegen prüfen, ob wirklich alles gelöscht werden muss, oder weiter behalten werden kann.
        // Auf Nothing muss auch geprüft werden, da bei einem Dispose oder beim Beenden sich die Tabelle auch änsdert....

        if (e.IsFirst) {
            if (!string.IsNullOrEmpty(_storedView)) {
                ParseView(_storedView);
                _storedView = string.Empty;
            } else {
                ResetView();
            }
        } else {
            _storedView = string.Empty;
        }

        Invalidate_AllViewItems(false); // Neue Zeilen können nun erlaubt sein
        Invalidate_CurrentArrangement(); // Wegen der Spaltenbreite
        CheckView();
    }

    private void _Table_ViewChanged(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        Invalidate_CurrentArrangement();
        CursorPos_Set(CursorPosColumn, CursorPosRow, true);
    }

    private void AutoFilter_Close() {
        if (_autoFilter != null) {
            _autoFilter.FilterCommand -= AutoFilter_FilterCommand;
            _autoFilter?.Dispose();
            _autoFilter = null;
        }
    }

    private void AutoFilter_FilterCommand(object sender, FilterCommandEventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false }) { return; }

        switch (e.Command.ToLowerInvariant()) {
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
                RowCollection.GetUniques(e.Column, _rowsVisibleUnique, out var einzigartig, out _);
                if (einzigartig.Count > 0) {
                    Filter.Add(new FilterItem(e.Column, FilterType.Istgleich_ODER_GroßKleinEgal, einzigartig));
                    Notification.Show("Die aktuell einzigartigen Einträge wurden berechnet<br>und als <b>ODER-Filter</b> gespeichert.", ImageCode.Trichter);
                } else {
                    Notification.Show("Filterung dieser Spalte gelöscht,<br>da <b>alle Einträge</b> mehrfach vorhanden sind.", ImageCode.Trichter);
                }
                break;

            case "donichteinzigartig":
                Filter.Remove(e.Column);
                RowCollection.GetUniques(e.Column, _rowsVisibleUnique, out _, out var xNichtEinzigartig);
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
            //        foreach (var thisColumnItem in e.Column.Table.Column) {
            //            if (thisColumnItem != null && thisColumnItem != e.Column) { ic.Add(thisColumnItem); }
            //        }
            //        ic.Sort();

            //        var r = InputBoxListBoxStyle.Show("Mit welcher Spalte vergleichen?", ic, AddType.None, true);
            //        if (r == null || r.Count == 0) { return; }

            //        var c = e.Column.Table.Column[r[0]);

            //        List<string> d = new();
            //        foreach (var thisR in ro) {
            //            if (thisR.CellGetString(e.Column) != thisR.CellGetString(c)) { d.Add(thisR.CellFirstString()); }
            //        }
            //        if (d.Count > 0) {
            //            Filter.Add(new FilterItem(e.Column.Table.Column.First, FilterType.Istgleich_ODER_GroßKleinEgal, d));
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
                        Filter.Add(new FilterItem(e.Column, FilterType.Istgleich_ODER_GroßKleinEgal, searchValue));
                    }
                    break;
                }

            case "donotclipboard": {
                    var clipTmp = Clipboard.GetText().RemoveChars(Char_NotFromClip).TrimEnd("\r\n");
                    Filter.Remove(e.Column);

                    var searchValue = e.Column.Contents();//  tb.Export_CSV(FirstRow.Without, e.Column, null).SplitAndCutByCr().SortedDistinctList();
                    searchValue.RemoveString(clipTmp.SplitAndCutByCr().SortedDistinctList(), false);

                    if (searchValue.Count > 0) {
                        Filter.Add(new FilterItem(e.Column, FilterType.Istgleich_ODER_GroßKleinEgal, searchValue));
                    }
                    break;
                }
            default:
                Develop.DebugPrint("Unbekannter Command: " + e.Command);
                break;
        }

        if (e.Filter?.Column is { IsDisposed: false } col) {
            col.AddSystemInfo("Filter Clicked", UserName);
        }

        OnAutoFilterClicked(new FilterEventArgs(e.Filter));
    }

    private void AutoFilter_Show(ColumnViewCollection ca, ColumnViewItem columnviewitem, int screenx, int screeny, int bottom) {
        if (columnviewitem.Column == null) { return; }
        if (!ca.ShowHead) { return; }
        if (!columnviewitem.AutoFilterSymbolPossible) { return; }
        if (IsDisposed || Table is not { IsDisposed: false }) { return; }

        if (FilterCombined.HasAlwaysFalse()) {
            Forms.MessageBox.Show("Ein Filter, der nie ein Ergebnis zurückgibt,\r\nverhindert aktuell Filterungen.", ImageCode.Information, "OK");
            return;
        }

        var t = string.Empty;
        foreach (var thisFilter in Filter) {
            if (thisFilter != null && thisFilter.Column == columnviewitem.Column && !string.IsNullOrEmpty(thisFilter.Origin)) {
                t += "\r\n" + thisFilter.Origin; // thisFilter.ReadableText();
            }
        }

        if (!string.IsNullOrEmpty(t)) {
            Forms.MessageBox.Show("<b>Dieser Filter wurde automatisch gesetzt:</b>" + t, ImageCode.Information, "OK");
            return;
        }

        var headX = columnviewitem.ControlColumnLeft(OffsetX);
        //headX = headX.CanvasToControl(Zoom, OffsetX);// ControlToCanvasX((columnviewitem.ControlX ?? 0), Zoom) - OffsetX;

        _autoFilter = new AutoFilter(columnviewitem.Column, FilterCombined, PinnedRows, columnviewitem.CanvasContentWidth, columnviewitem.GetRenderer(SheetStyle));
        _autoFilter.Position_LocateToPosition(new Point(screenx + headX, screeny + bottom));
        _autoFilter.Show();
        _autoFilter.FilterCommand += AutoFilter_FilterCommand;
    }

    /// <summary>
    /// Gibt die Anzahl der SICHTBAREN Zeilen zurück, die mehr angezeigt werden würden, wenn dieser Filter deaktiviert wäre.
    /// </summary>
    /// <returns></returns>
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

    private void btnEdit_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }
        tb.Edit(typeof(TableHeadEditor));
    }

    private void CalculateAllViewItems(Dictionary<string, AbstractListItem> allItems) {
        if (IsDisposed || Table is not { IsDisposed: false } tb || allItems == null || SortUsed() is not { } sortused) {
            _rowsVisibleUnique = new([]);
            allItems?.Clear();
            return;
        }
        if (CurrentArrangement is not { } arrangement) { return; }
        _newRowsAllowed = UserEdit_NewRowAllowed();

        List<RowItem> pinnedRows = [.. PinnedRows];
        var filteredRows = sortused.SortedRows(FilterCombined.Rows);

        List<RowItem> allrows = [.. pinnedRows, .. filteredRows];
        allrows = [.. allrows.Distinct()];

        var sortedItems = new List<AbstractListItem>();

        CalculateAllViewItems_AddHeadElements(allItems, arrangement, sortedItems, FilterCombined, sortused);

        var allVisibleCaps = CalculateAllViewItems_AddCaptions(allItems, arrangement, tb, filteredRows, pinnedRows);

        var visibleRowListItems = CalculateAllViewItems_Rows(allItems, arrangement, tb, allrows, pinnedRows, allVisibleCaps, sortused);

        CalculateAllViewItems_Collapsed(allItems);

        CalculateAllViewItems_HildeAllItems(allItems, arrangement);

        var captionOrder = CalculateAllViewItems_CaptionOrder(visibleRowListItems, allVisibleCaps);

        CalculateAllViewItems_AddCaptionsAndRows(allItems, sortedItems, captionOrder, sortused, visibleRowListItems);

        CalculateAllViewItems_AddFootElements(allItems, arrangement, sortedItems, FilterCombined, sortused);

        CalculateAllViewItems_CalculateYPosition(sortedItems, arrangement);

        DoCursorPos();

        _rowsVisibleUnique = allrows;
    }

    private HashSet<string> CalculateAllViewItems_AddCaptions(Dictionary<string, AbstractListItem> allItems, ColumnViewCollection arrangement, Table tb, List<RowItem> filteredRows, List<RowItem> pinnedRows) {
        HashSet<string> allCaps = [];

        if (tb.Column.SysChapter is { IsDisposed: false } cap) {
            var caps = cap.Contents(filteredRows);

            foreach (var capValue in caps) {
                var parts = capValue.Trim('\\').Split('\\');
                var currentPath = parts[0];
                allCaps.Add(parts[0]);

                for (var i = 1; i < parts.Length; i++) {
                    currentPath += "\\" + parts[i];
                    allCaps.Add(currentPath);
                }
            }
        }

        if (pinnedRows.Count > 0) {
            allCaps.Add(Angepinnt);
            allCaps.Add(Weitere_Zeilen);
        }

        foreach (var thisCap in allCaps) {
            if (!allItems.TryGetValue(RowCaptionListItem.Identifier(thisCap), out _)) {
                AbstractListItem? capi = new RowCaptionListItem(thisCap, arrangement);
                allItems.Add(capi.KeyName, capi);
            }
        }

        return allCaps;
    }

    private void CalculateAllViewItems_AddCaptionsAndRows(Dictionary<string, AbstractListItem> allItems, List<AbstractListItem> sortedItems, List<string> captionOrder, RowSortDefinition sortused, List<RowListItem> visibleRowListItems) {
        // Captions und ihre RowDataListItems in der ermittelten Reihenfolge hinzufügen
        foreach (var captionKey in captionOrder) {
            // Caption hinzufügen

            allItems.TryGetValue(RowCaptionListItem.Identifier(captionKey), out var caption);
            if (caption != null) { sortedItems.Add(caption); }

            if (!_collapsed.Contains(captionKey)) {
                if (sortused.Reverse) {
                    var captionDataItems = visibleRowListItems.Where(x => x.AlignsToChapter == captionKey).OrderByDescending(item => item.CompareKey());
                    sortedItems.AddRange(captionDataItems);
                } else {
                    var captionDataItems = visibleRowListItems.Where(x => x.AlignsToChapter == captionKey).OrderBy(item => item.CompareKey());
                    sortedItems.AddRange(captionDataItems);
                }
            }
        }
    }

    private void CalculateAllViewItems_AddFootElements(Dictionary<string, AbstractListItem> allItems, ColumnViewCollection arrangement, List<AbstractListItem> sortedItems, FilterCollection filterCombined, RowSortDefinition sortused) {
        allItems.TryGetValue(TableEndListItem.Identifier, out var item0);
        if (item0 is not TableEndListItem tableEnd) {
            tableEnd = new TableEndListItem(arrangement);
            allItems.Add(tableEnd.KeyName, tableEnd);
        }
        tableEnd.Visible = arrangement.ShowHead;
        tableEnd.IgnoreYOffset = false;
        sortedItems.Add(tableEnd);
    }

    private void CalculateAllViewItems_AddHeadElements(Dictionary<string, AbstractListItem> allItems, ColumnViewCollection arrangement, List<AbstractListItem> sortedItems, FilterCollection filterCombined, RowSortDefinition sortused) {
        if (!arrangement.ShowHead) { return; }

        for (var z = 0; z < 3; z++) {
            var add = false;
            foreach (var thisColumn in arrangement) {
                if (thisColumn.Column is { } c && !string.IsNullOrEmpty(c.CaptionGroup(z))) { add = true; break; }
            }

            if (add) {
                // Caption 1 bis 3 Expand Button
                allItems.TryGetValue(CaptionBarListItem.Identifier(z), out var itemcap);
                if (itemcap is not CaptionBarListItem captionBar) {
                    captionBar = new CaptionBarListItem(arrangement, z);
                    allItems.Add(captionBar.KeyName, captionBar);
                }
                captionBar.Visible = arrangement.ShowHead;
                captionBar.IgnoreYOffset = true;
                sortedItems.Add(captionBar);
            }
        }

        // Grüner Expand Button
        allItems.TryGetValue(CollapesBarListItem.Identifier, out var item0);
        if (item0 is not CollapesBarListItem collapseBar) {
            collapseBar = new CollapesBarListItem(arrangement);
            allItems.Add(collapseBar.KeyName, collapseBar);
        }
        collapseBar.Visible = arrangement.ShowHead;
        collapseBar.IgnoreYOffset = true;
        sortedItems.Add(collapseBar);

        // Spaltenköpfe direkt
        allItems.TryGetValue(ColumnsHeadListItem.Identifier, out var itemHead);
        if (itemHead is not ColumnsHeadListItem columnHead) {
            columnHead = new ColumnsHeadListItem(arrangement);
            allItems.Add(columnHead.KeyName, columnHead);
        }
        columnHead.Visible = arrangement.ShowHead;
        columnHead.IgnoreYOffset = true;
        sortedItems.Add(columnHead);

        //// Die Infos
        //allItems.TryGetValue(RowInfoListItem.Identifier, out var itemAdmin);
        //if (itemAdmin is not RowInfoListItem itemHeadx) {
        //    itemHeadx = new RowInfoListItem(arrangement);
        //    allItems.Add(itemHeadx.KeyName, itemHeadx);
        //}
        //itemHeadx.Visible = arrangement.ShowHead;
        //sortedItems.Add(itemHeadx);

        // Die Sortierung
        allItems.TryGetValue(SortBarListItem.Identifier, out var item1);
        if (item1 is not SortBarListItem sortAnzeige) {
            sortAnzeige = new SortBarListItem(arrangement);
            allItems.Add(sortAnzeige.KeyName, sortAnzeige);
        }
        sortAnzeige.Visible = arrangement.ShowHead;
        sortAnzeige.FilterCombined = filterCombined;
        sortAnzeige.Sort = sortused;
        sortAnzeige.IgnoreYOffset = true;
        sortedItems.Add(sortAnzeige);

        // Filterleiste
        allItems.TryGetValue(FilterBarListItem.Identifier, out var item2);
        if (item2 is not FilterBarListItem columnFilter) {
            columnFilter = new FilterBarListItem(arrangement);
            allItems.Add(columnFilter.KeyName, columnFilter);
        }
        columnFilter.Visible = arrangement.ShowHead;
        columnFilter.ShowNumber = ShowNumber;
        columnFilter.FilterCombined = filterCombined;
        columnFilter.RowsFilteredCount = filterCombined.Rows.Count;
        columnFilter.IgnoreYOffset = true;
        sortedItems.Add(columnFilter);

        // Neue Zeile
        if (string.IsNullOrEmpty(_newRowsAllowed)) {
            allItems.TryGetValue(NewRowListItem.Identifier, out var item3);
            if (item3 is not NewRowListItem newRow) {
                newRow = new NewRowListItem(arrangement);
                allItems.Add(newRow.KeyName, newRow);
            }
            newRow.Visible = string.IsNullOrEmpty(_newRowsAllowed);
            newRow.FilterCombined = FilterCombined;
            newRow.IgnoreYOffset = true;
            sortedItems.Add(newRow);
        }
    }

    private void CalculateAllViewItems_CalculateYPosition(List<AbstractListItem> sortedItems, ColumnViewCollection arrangement) {
        var wi = (int)arrangement.ControlColumnsWidth.ControlToCanvas(Zoom);

        var y = 0;

        foreach (var thisItem in sortedItems) {
            thisItem.Visible = true;
            thisItem.CanvasPosition = new Rectangle(0, y, wi, thisItem.HeightInControl(ListBoxAppearance.Listbox, wi, Design.Item_Listbox));
            y = thisItem.CanvasPosition.Bottom;
        }
    }

    private List<string> CalculateAllViewItems_CaptionOrder(List<RowListItem> visibleRowListItems, HashSet<string> allVisibleCaps) {
        var captionOrder = new List<string>();

        // "Angepinnt" zuerst (falls vorhanden)
        if (allVisibleCaps.Contains(Angepinnt)) { captionOrder.Add(Angepinnt.ToUpperInvariant()); }

        // Alle anderen Captions in der Reihenfolge, wie sie durch sortedDataItems vorkommen
        foreach (var dataItem in visibleRowListItems) {
            captionOrder.AddIfNotExists(dataItem.AlignsToChapter);
        }

        return captionOrder;
    }

    /// <summary>
    /// Berechnet die Variable _collapsed
    /// </summary>
    /// <param name="allItems"></param>
    private void CalculateAllViewItems_Collapsed(Dictionary<string, AbstractListItem> allItems) {
        var l = new List<string>();

        foreach (var thisR in allItems.Values) {
            lock (lockMe) {
                if (thisR is RowCaptionListItem { IsDisposed: false, IsExpanded: false } rcli) {
                    l.Add(rcli.ChapterText.ToUpperInvariant());

                    // Alle Untereinträge hinzufügen
                    var prefix = rcli.ChapterText.ToUpperInvariant() + "\\";
                    foreach (var otherR in allItems.Values) {
                        if (otherR is RowCaptionListItem { IsDisposed: false } otherRcli) {
                            if (otherRcli.ChapterText.ToUpperInvariant().StartsWith(prefix)) {
                                l.Add(otherRcli.ChapterText);
                            }
                        }
                    }
                }
            }
        }

        _collapsed.Clear();
        _collapsed.AddRange(l.SortedDistinctList());
    }

    private void CalculateAllViewItems_HildeAllItems(Dictionary<string, AbstractListItem> allItems, ColumnViewCollection arrangement) {
        var wi = (int)arrangement.ControlColumnsWidth.ControlToCanvas(Zoom);

        foreach (var thisItem in allItems.Values) {
            thisItem.Visible = false;

            if (thisItem is RowBackgroundListItem rbli) {
                rbli.Arrangement = arrangement;
                rbli.CanvasPosition = rbli.CanvasPosition with { Width = wi };
            }
        }
    }

    private List<RowListItem> CalculateAllViewItems_Rows(Dictionary<string, AbstractListItem> allItems, ColumnViewCollection arrangement, Table tb, List<RowItem> allrows, List<RowItem> pinnedRows, HashSet<string> allVisibleCaps, RowSortDefinition sortused) {
        var nullcap = allVisibleCaps.Count > 0;

        var visibleRowListItems = new List<RowListItem>();

        Parallel.ForEach(allrows, thisRow => {
            var markYellow = pinnedRows.Contains(thisRow);

            var capsOfRow = tb.Column.SysChapter is { IsDisposed: false } sc ? thisRow.CellGetList(sc) : [];
            capsOfRow.Remove(string.Empty);
            if (capsOfRow.Count == 0 && nullcap) { capsOfRow.Add(Weitere_Zeilen); }
            if (markYellow) { capsOfRow.Add(Angepinnt); }
            if (capsOfRow.Count == 0) { capsOfRow.Add(string.Empty); }

            foreach (var thisCap in capsOfRow) {
                RowListItem? rowListItem;
                lock (lockMe) {
                    allItems.TryGetValue(RowListItem.Identifier(thisRow, thisCap), out var it2);
                    rowListItem = it2 as RowListItem;
                }

                if (rowListItem is null) {
                    rowListItem = new RowListItem(thisRow, thisCap, arrangement);
                    lock (lockMe) {
                        allItems.Add(rowListItem.KeyName, rowListItem);
                    }
                }

                rowListItem.UserDefCompareKey = rowListItem.Row.CompareKey(sortused.UsedColumns);
                rowListItem.Visible = false;

                lock (lockMe) {
                    visibleRowListItems.Add(rowListItem);
                }

                rowListItem.MarkYellow = markYellow;
            }
        });

        return visibleRowListItems;
    }

    private void Cell_Edit(ColumnViewItem? viewItem, AbstractListItem? rowItem, bool preverDropDown, string? chunkval) {
        var f = IsCellEditable(viewItem, rowItem as RowListItem, chunkval, true);
        if (!string.IsNullOrEmpty(f)) { NotEditableInfo(f); return; }

        if (viewItem?.Column is not { IsDisposed: false } contentHolderCellColumn) {
            NotEditableInfo("Keine Spalte angeklickt.");
            return;
        }

        var contentHolderCellRow = (rowItem as RowListItem)?.Row;

        if (viewItem.Column.RelationType == RelationType.CellValues) {
            (contentHolderCellColumn, contentHolderCellRow, _, _) = contentHolderCellRow.LinkedCellData(contentHolderCellColumn, true, true);
        }

        if (contentHolderCellColumn is not { IsDisposed: false }) {
            NotEditableInfo("Keine Spalte angeklickt.");
            return;
        }

        var dia = ColumnItem.UserEditDialogTypeInTable(contentHolderCellColumn, preverDropDown);

        if (dia == EditTypeTable.None && (contentHolderCellColumn.Table?.PowerEdit ?? false)) {
            dia = ColumnItem.UserEditDialogTypeInTable(contentHolderCellColumn, false, true);
        }

        switch (dia) {
            case EditTypeTable.Textfeld:
                contentHolderCellColumn.AddSystemInfo("Edit in Table", UserName);
                Cell_Edit_TextBox(viewItem, rowItem, BTB, 0);
                break;

            case EditTypeTable.Textfeld_mit_Auswahlknopf:
                contentHolderCellColumn.AddSystemInfo("Edit in Table", UserName);
                Cell_Edit_TextBox(viewItem, rowItem, BCB, 20);
                break;

            case EditTypeTable.Dropdown_Single:
                contentHolderCellColumn.AddSystemInfo("Edit in Table", UserName);
                Cell_Edit_Dropdown(viewItem, rowItem, contentHolderCellColumn, contentHolderCellRow);
                break;

            case EditTypeTable.Farb_Auswahl_Dialog:
                contentHolderCellColumn.AddSystemInfo("Edit in Table", UserName);
                //if (viewItem.Column != contentHolderCellColumn || rowItem?.Row != contentHolderCellRow) {
                //    NotEditableInfo("Verlinkte Zellen hier verboten.");
                //    return;
                //}
                Cell_Edit_Color(viewItem, rowItem);
                break;

            case EditTypeTable.Font_AuswahlDialog:
                contentHolderCellColumn.AddSystemInfo("Edit in Table", UserName);
                Develop.DebugPrint_NichtImplementiert(false);
                //if (cellInThisTableColumn != ContentHolderCellColumn || cellInThisTableRow != ContentHolderCellRow)
                //{
                //    NotEditableInfo("Ziel-Spalte ist kein Textformat");
                //    return;
                //}
                //Cell_Edit_Font(cellInThisTableColumn, cellInThisTableRow);
                break;

            case EditTypeTable.WarnungNurFormular:
                NotEditableInfo("Dieser Zelltyp kann nur in einem Formular-Fenster bearbeitet werden");
                break;

            case EditTypeTable.None:
                break;

            default:
                Develop.DebugPrint(dia);
                NotEditableInfo("Unbekannte Bearbeitungs-Methode");
                break;
        }
    }

    private void Cell_Edit_Color(ColumnViewItem viewItem, AbstractListItem? cellInThisTableRow) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        if (cellInThisTableRow is not RowListItem rli) { return; }

        var colDia = new ColorDialog();

        if (rli.Row is { IsDisposed: false } r) {
            colDia.Color = r.CellGetColor(viewItem.Column);
        }
        colDia.Tag = (List<object?>)[viewItem.Column, cellInThisTableRow];
        List<int> colList = [];
        foreach (var thisRowItem in tb.Row) {
            if (thisRowItem != null) {
                if (thisRowItem.CellGetInteger(viewItem.Column) != 0) {
                    colList.Add(thisRowItem.CellGetColorBgr(viewItem.Column));
                }
            }
        }
        colList.Sort();
        colDia.CustomColors = [.. colList.Distinct()];
        colDia.ShowDialog();
        colDia.Dispose();

        NotEditableInfo(UserEdited(this, Color.FromArgb(255, colDia.Color).ToArgb().ToString1(), viewItem, cellInThisTableRow as RowListItem, false));
    }

    private void Cell_Edit_Dropdown(ColumnViewItem viewItem, AbstractListItem? cellInThisTableRow, ColumnItem contentHolderCellColumn, RowItem? contentHolderCellRow) {
        if (viewItem.Column != contentHolderCellColumn) {
            if (contentHolderCellRow == null) {
                NotEditableInfo("Bei Zellverweisen kann keine neue Zeile erstellt werden.");
                return;
            }
            if (cellInThisTableRow == null) {
                NotEditableInfo("Bei Zellverweisen kann keine neue Zeile erstellt werden.");
                return;
            }
        }

        if (cellInThisTableRow is not RowListItem rli) { return; }

        var t = new List<AbstractListItem>();

        var r = viewItem.GetRenderer(SheetStyle);
        var cell = new CellExtEventArgs(viewItem, cellInThisTableRow as RowListItem);

        t.AddRange(ItemsOf(contentHolderCellColumn, contentHolderCellRow, 1000, r, cell));
        if (t.Count == 0) {
            // Hm ... Dropdown kein Wert vorhanden.... also gar kein Dropdown öffnen!
            if (contentHolderCellColumn.EditableWithTextInput) { Cell_Edit(viewItem, cellInThisTableRow, false, rli.Row.ChunkValue ?? FilterCombined.ChunkVal); } else {
                NotEditableInfo("Keine Items zum Auswählen vorhanden.");
            }
            return;
        }

        if (contentHolderCellColumn.EditableWithTextInput) {
            if (t.Count == 0 && string.IsNullOrWhiteSpace(rli.Row.CellGetString(viewItem.Column))) {
                // Bei nur einem Wert, wenn Texteingabe erlaubt, Dropdown öffnen
                Cell_Edit(viewItem, cellInThisTableRow, false, rli.Row.ChunkValue ?? FilterCombined.ChunkVal);
                return;
            }
            var erw = ItemOf("Erweiterte Eingabe", "#Erweitert", QuickImage.Get(ImageCode.Stift), true, FirstSortChar + "1");
            erw.Tag = cell;

            t.Add(erw);
            t.Add(SeparatorWith(FirstSortChar + "2"));
        }

        List<string> toc = [];

        if (contentHolderCellRow != null) {
            toc.AddRange(contentHolderCellRow.CellGetList(contentHolderCellColumn));
        }

        var dropDownMenu = FloatingInputBoxListBoxStyle.Show(t, CheckBehavior.MultiSelection, toc, this, Translate, ListBoxAppearance.DropdownSelectbox, Design.Item_DropdownMenu, true);
        dropDownMenu.ItemClicked += DropDownMenu_ItemClicked;
        Develop.Debugprint_BackgroundThread();
    }

    private bool Cell_Edit_TextBox(ColumnViewItem viewItem, AbstractListItem? cellInThisTableRow, TextBox box, int addWith) {
        if (IsDisposed) { return false; }

        if (viewItem.Column == null) { return false; }

        //if (contentHolderCellColumn != viewItem.Column) {
        //    if (contentHolderCellRow == null) {
        //        NotEditableInfo("Bei Zellverweisen kann keine neue Zeile erstellt werden.");
        //        return false;
        //    }
        //    if (cellInThisTableRow == null) {
        //        NotEditableInfo("Bei Zellverweisen kann keine neue Zeile erstellt werden.");
        //        return false;
        //    }
        //}

        var controlX = viewItem.ControlColumnLeft(OffsetX);

        var controlWidth = viewItem.ControlColumnWidth ?? 16;

        box.GetStyleFrom(viewItem.Column);
        RowItem? contentHolderCellRow = null;
        if (cellInThisTableRow is RowListItem rli) {
            var controlPos = cellInThisTableRow.ControlPosition(Zoom, OffsetX, OffsetY);
            box.Location = new Point(controlX, controlPos.Y);
            box.Size = new Size(controlWidth + addWith, controlPos.Height);
            box.Text = rli.Row.CellGetString(viewItem.Column);
            contentHolderCellRow = rli.Row;
        } else if (cellInThisTableRow is NewRowListItem) {
            // Neue Zeile...
            var controlPos = cellInThisTableRow.ControlPosition(Zoom, OffsetX, 0);
            box.Location = new Point(controlX, controlPos.Y);
            box.Size = new Size(controlWidth + addWith, controlPos.Height);
            box.Text = string.Empty;
        }

        box.Tag = (List<object?>)[viewItem, cellInThisTableRow];

        if (box is ComboBox cbox) {
            cbox.ItemClear();
            cbox.ItemAddRange(ItemsOf(viewItem.Column, contentHolderCellRow, 1000, viewItem.GetRenderer(SheetStyle), null));
            if (cbox.ItemCount == 0) {
                return Cell_Edit_TextBox(viewItem, cellInThisTableRow, BTB, 0);
            }
        }

        box.Visible = true;
        box.BringToFront();
        box.Focus();
        return true;
    }

    private (ColumnViewItem?, AbstractListItem?) CellOnCoordinate(ColumnViewCollection ca, CanvasMouseEventArgs e) => (ColumnOnCoordinate(ca, e), AllViewItems?.Values.ToList().ElementAtPosition(1, e.ControlY, Zoom, OffsetX, OffsetY));

    private void CloseAllComponents() {
        if (InvokeRequired) {
            Invoke(new Action(CloseAllComponents));
            return;
        }
        if (IsDisposed || Table is not { IsDisposed: false }) { return; }
        TXTBox_Close(BTB);
        TXTBox_Close(BCB);
        FloatingForm.Close(this);
        AutoFilter_Close();
        Forms.QuickInfo.Close();
    }

    private void CollapseThis(string[] t) {
        if (AllViewItems is not { } avi) { return; }
        var did = false;

        foreach (var thisItem in avi.Values) {
            if (thisItem is RowCaptionListItem { IsDisposed: false } rcli) {
                if (rcli.IsExpanded == t.Contains(rcli.ChapterText)) {
                    rcli.IsExpanded = !rcli.IsExpanded;
                    did = true;
                }
            }
        }

        if (did) { Invalidate_AllViewItems(false); }
    }

    private void Column_ItemRemoving(object sender, ColumnEventArgs e) {
        if (e.Column == CursorPosColumn?.Column) { CursorPos_Reset(); }
    }

    private ColumnViewItem? ColumnOnCoordinate(ColumnViewCollection ca, CanvasMouseEventArgs e) {
        if (ca.IsDisposed) { return null; }

        foreach (var thisViewItem in ca) {
            if (e.ControlX >= thisViewItem.ControlColumnLeft(OffsetX) && e.ControlX <= thisViewItem.ControlColumnRight(OffsetX)) { return thisViewItem; }
        }

        return null;
    }

    private void ContextMenu_ContentCopy(object sender, ObjectEventArgs e) {
        if (e.Data is not { } data) { return; }
        var column = data.GetType().GetProperty("Column")?.GetValue(data) as ColumnItem;
        var row = data.GetType().GetProperty("Row")?.GetValue(data) as RowItem;
        CopyToClipboard(column, row, true);
    }

    private void ContextMenu_ContentDelete(object sender, ObjectEventArgs e) {
        if (e.Data is not { } data) { return; }
        var column = data.GetType().GetProperty("Column")?.GetValue(data) as ColumnItem;
        var row = data.GetType().GetProperty("Row")?.GetValue(data) as RowItem;
        if (TableViewForm.EditabelErrorMessage(Table)) { return; }
        row?.CellSet(column, string.Empty, "Inhalt Löschen Kontextmenu");
    }

    private void ContextMenu_ContentPaste(object sender, ObjectEventArgs e) => PasteToCursor();

    private void ContextMenu_CopyAll(object sender, ObjectEventArgs e) {
        if (e.Data is not ColumnItem column || Table is not { IsDisposed: false } tb || !tb.IsAdministrator()) { return; }
        var txt = Export_CSV(FirstRow.Without, column);
        txt = txt.Replace("|", "\r\n");
        txt = txt.Replace(";", string.Empty);
        CopytoClipboard(txt);
        Notification.Show("Die Daten sind nun<br>in der Zwischenablage.", ImageCode.Clipboard);
    }

    private void ContextMenu_CopyAllSorted(object sender, ObjectEventArgs e) {
        if (e.Data is not ColumnItem column || Table is not { IsDisposed: false } tb || !tb.IsAdministrator()) { return; }
        var txt = Export_CSV(FirstRow.Without, column);
        txt = txt.Replace("|", "\r\n");
        txt = txt.Replace(";", string.Empty);
        var l = txt.SplitAndCutByCr().SortedDistinctList().JoinWithCr();
        CopytoClipboard(l);
        Notification.Show("Die Daten sind nun<br>in der Zwischenablage.", ImageCode.Clipboard);
    }

    private void ContextMenu_KeyCopy(object sender, ObjectEventArgs e) {
        if (e.Data is not RowItem row) { return; }
        CopytoClipboard(row.KeyName);
        Notification.Show(LanguageTool.DoTranslate("Schlüssel kopiert.", true), ImageCode.Schlüssel);
    }

    private void ContextMenu_Pin(object sender, ObjectEventArgs e) {
        if (e.Data is not RowItem row) { return; }
        PinAdd(row);
    }

    private void ContextMenu_ResetSort(object sender, ObjectEventArgs e) => SortDefinitionTemporary = null;

    private void ContextMenu_RestorePreviousContent(object sender, ObjectEventArgs e) {
        if (e.Data is not { } data) { return; }
        var column = data.GetType().GetProperty("Column")?.GetValue(data) as ColumnItem;
        var row = data.GetType().GetProperty("Row")?.GetValue(data) as RowItem;
        if (TableViewForm.EditabelErrorMessage(Table)) { return; }
        DoUndo(column, row);
    }

    private void ContextMenu_SearchAndReplace(object sender, ObjectEventArgs e) {
        if (Table is not { IsDisposed: false } tb || !tb.IsAdministrator()) { return; }
        OpenSearchAndReplaceInCells();
    }

    private void ContextMenu_SortAZ(object sender, ObjectEventArgs e) {
        if (e.Data is not ColumnItem column || column.Table is not { IsDisposed: false } tb) { return; }
        SortDefinitionTemporary = new RowSortDefinition(tb, column, false);
    }

    private void ContextMenu_SortZA(object sender, ObjectEventArgs e) {
        if (e.Data is not ColumnItem column || column.Table is not { IsDisposed: false } tb) { return; }
        SortDefinitionTemporary = new RowSortDefinition(tb, column, true);
    }

    private void ContextMenu_Statistics(object sender, ObjectEventArgs e) {
        if (e.Data is not ColumnItem column || Table is not { IsDisposed: false } tb || !tb.IsAdministrator()) { return; }
        var split = false;
        if (column.MultiLine) {
            split = Forms.MessageBox.Show("Zeilen als Ganzes oder aufsplitten?", ImageCode.Frage, "Ganzes", "Splitten") != 0;
        }
        column.Statistik(_rowsVisibleUnique, !split);
    }

    private void ContextMenu_Sum(object sender, ObjectEventArgs e) {
        if (e.Data is not ColumnItem column || Table is not { IsDisposed: false } tb || !tb.IsAdministrator()) { return; }
        var summe = column.Summe(FilterCombined);
        if (!summe.HasValue) {
            Forms.MessageBox.Show("Die Summe konnte nicht berechnet werden.", ImageCode.Summe, "OK");
        } else {
            Forms.MessageBox.Show("Summe dieser Spalte, nur angezeigte Zeilen: <br><b>" + summe, ImageCode.Summe, "OK");
        }
    }

    private void ContextMenu_Unpin(object sender, ObjectEventArgs e) {
        if (e.Data is not RowItem row) { return; }
        PinRemove(row);
    }

    private void ContextMenu_Voting(object sender, ObjectEventArgs e) {
        if (e.Data is not ColumnItem column || Table is not { IsDisposed: false } tb || !tb.IsAdministrator()) { return; }
        var v = new Voting(column, [.. FilterCombined.Rows]);
        v.ShowDialog();
    }

    private void Cursor_Move(Direction direction) {
        if (IsDisposed || Table is not { IsDisposed: false }) { return; }

        if (CurrentArrangement is not { IsDisposed: false } ca) {
            CursorPos_Set(null, null, false);
            return;
        }

        if (direction == Direction.Nichts) { return; }

        var newColumn = CursorPosColumn;
        var newRow = CursorPosRow;

        if (newColumn != null) {
            if (direction.HasFlag(Direction.Links)) {
                if (ca.PreviousVisible(newColumn) is { } c) { newColumn = c; }
            }
            if (direction.HasFlag(Direction.Rechts)) {
                if (ca.NextVisible(newColumn) is { } c) { newColumn = c; }
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

        CursorPos_Set(newColumn, newRow, true);
    }

    private void CursorPos_Reset() => CursorPos_Set(null, null, false);

    private void DoCursorPos() {
        foreach (var thisItem in _allViewItems.Values) {
            if (thisItem is RowListItem rdli) {
                rdli.Column = CursorPosRow == rdli ? CursorPosColumn?.Column : null;
            }
        }
    }

    private void DoFilterCombined() {
        if (Filter.Count == 0 && (FilterFix is not { IsDisposed: false } fi || fi.Count == 0)) {
            FilterCombined.Clear();
            return;
        }

        using var nfc = new FilterCollection(Filter.Table, "TmpFilterCombined");

        nfc.Table = Filter.Table;
        nfc.RemoveOtherAndAdd(Filter, null);
        nfc.RemoveOtherAndAdd(FilterFix, "Filter aus übergeordneten Element");

        FilterCombined.ChangeTo(nfc);
    }

    /// <summary>
    /// Berechent die Y-CanvasPosition auf dem aktuellen Controll
    /// </summary>
    /// <returns></returns>
    private void DropDownMenu_ItemClicked(object sender, AbstractListItemEventArgs e) {
        FloatingForm.Close(this);

        if (CurrentArrangement is not { IsDisposed: false }) { return; }

        CellExtEventArgs? ck = null;
        if (e.Item.Tag is CellExtEventArgs tmp) { ck = tmp; }

        if (ck?.ColumnView?.Column is not { IsDisposed: false } c) { return; }

        var toAdd = e.Item.KeyName;
        var toRemove = string.Empty;
        if (toAdd == "#Erweitert") {
            Cell_Edit(ck.ColumnView, ck.RowData, false, ck.RowData?.Row.ChunkValue ?? FilterCombined.ChunkVal);
            return;
        }
        if (ck.RowData?.Row is not { IsDisposed: false } r) {
            // Neue Zeile!
            NotEditableInfo(UserEdited(this, toAdd, ck.ColumnView, null, false));
            return;
        }

        if (c.MultiLine) {
            var li = r.CellGetList(c);
            if (li.Contains(toAdd, false)) {
                // Ist das angeklickte Element schon vorhanden, dann soll es wohl abgewählt (gelöscht) werden.
                if (li.Count > -1 || c.DropdownDeselectAllAllowed) {
                    toRemove = toAdd;
                    toAdd = string.Empty;
                }
            }
            if (!string.IsNullOrEmpty(toRemove)) { li.RemoveString(toRemove, false); }
            if (!string.IsNullOrEmpty(toAdd)) { li.Add(toAdd); }
            NotEditableInfo(UserEdited(this, li.JoinWithCr(), ck.ColumnView, ck.RowData, false));
        } else {
            if (c.DropdownDeselectAllAllowed) {
                if (toAdd == ck.RowData.Row.CellGetString(c)) {
                    NotEditableInfo(UserEdited(this, string.Empty, ck.ColumnView, ck.RowData, false));
                    return;
                }
            }
            NotEditableInfo(UserEdited(this, toAdd, ck.ColumnView, ck.RowData, false));
        }
    }

    private bool EnsureVisible(AbstractListItem? rowdata) {
        if (rowdata is not RowListItem rli) { return false; }

        var p = rli.ControlPosition(Zoom, OffsetX, OffsetY);
        EnsureVisibleY(p.Bottom);
        EnsureVisibleY(p.Top);
        return true;
    }

    private bool EnsureVisible(ColumnViewItem? viewItem) {
        if (IsDisposed) { return false; }
        if (viewItem?.Column is not { IsDisposed: false }) { return false; }
        if (viewItem.Permanent) { return true; }
        //var dispR = DisplayRectangleWithoutSlider();

        //if (CurrentArrangement is not { IsDisposed: false } cax) { return false; }

        //var realhead = viewItem.RealHead(Zoom, OffsetX); // Filterleiste kann ignoriert werden, da nur ControlX-Koordinaten berechnet werden.

        //if (viewItem.ViewType == ViewType.PermanentColumn) {
        //    return realhead.Right <= dispR.Width;
        //}

        //if (realhead.Left < ControlToCanvasX(cax.WiederHolungsSpaltenWidth, Zoom)) {
        //    OffsetX = OffsetX + realhead.ControlX - ControlToCanvasX(cax.WiederHolungsSpaltenWidth, Zoom);
        //} else if (realhead.Right > dispR.Width) {
        //    OffsetX = OffsetX + realhead.Right - dispR.Width;
        //}

        EnsureVisibleX(viewItem.ControlColumnRight(OffsetX));
        EnsureVisibleX(viewItem.ControlColumnLeft(OffsetX));

        return true;
    }

    private void Filter_PropertyChanged(object sender, PropertyChangedEventArgs e) => DoFilterCombined();

    private void FilterAny_RowsChanged(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false }) { return; }
        if (CurrentArrangement is { IsDisposed: false } ca) {
            foreach (var thisColumn in ca) {
                thisColumn.TmpIfFilterRemoved = null;
            }
        }

        Invalidate_AllViewItems(false);
    }

    private void FilterCombined_PropertyChanged(object sender, PropertyChangedEventArgs e) => OnFilterCombinedChanged();

    private void FilterFix_PropertyChanged(object sender, PropertyChangedEventArgs e) => DoFilterCombined();

    private AbstractListItem? GetRow(RowItem? row) {
        if (row == null) { return null; }
        if (AllViewItems is not { } avi) { return null; }

        foreach (var thisItem in avi.Values) {
            if (thisItem is RowListItem rli && rli.Row == row) { return rli; }
        }
        return null;
    }

    private void Invalidate_AllViewItems(bool andclear) {
        mustDoAllViewItems = true;
        if (andclear) { _allViewItems.Clear(); }

        Invalidate_MaxBounds();
        Invalidate();
    }

    private void Invalidate_CurrentArrangement() {
        CurrentArrangement = null;
        Invalidate_AllViewItems(false); // Spaltenbreite, Slider
        Invalidate();
    }

    //private bool Mouse_IsInAutofilter(ColumnViewItem viewItem, MouseEventArgs e) => viewItem.AutoFilterLocation(Zoom, OffsetX, 0).Contains(e.Location);

    private void OnAutoFilterClicked(FilterEventArgs e) => AutoFilterClicked?.Invoke(this, e);

    private void OnCellClicked(CellEventArgs e) => CellClicked?.Invoke(this, e);

    private void OnFilterCombinedChanged() =>
        // Bestehenden Code belassen
        FilterCombinedChanged?.Invoke(this, System.EventArgs.Empty);//FillFilters(); // Die Flexs reagiren nur auf FilterOutput der Table

    private void OnPinnedChanged() =>
        // Bestehenden Code belassen
        PinnedChanged?.Invoke(this, System.EventArgs.Empty);

    private void OnSelectedCellChanged(CellExtEventArgs e) => SelectedCellChanged?.Invoke(this, e);

    private void OnSelectedRowChanged(RowNullableEventArgs e) => SelectedRowChanged?.Invoke(this, e);

    private void OnTableChanged() => TableChanged?.Invoke(this, System.EventArgs.Empty);

    private void OnViewChanged() {
        Invalidate_CurrentArrangement();
        Invalidate_AllViewItems(false); // evtl. muss [Neue Zeile] ein/ausgebelndet werden
        ViewChanged?.Invoke(this, System.EventArgs.Empty);
    }

    private void OnVisibleRowsChanged() => VisibleRowsChanged?.Invoke(this, System.EventArgs.Empty);

    private void PasteToCursor() {
        if (CursorPosColumn?.Column is not { } column || CursorPosRow?.Row is not { } row) {
            NotEditableInfo("Interner Fehler.");
            return;
        }

        if (!column.TextboxEditPossible() && column.RelationType != RelationType.DropDownValues) {
            NotEditableInfo("Die Zelle hat kein passendes Format.");
            return;
        }
        if (!Clipboard.ContainsText()) {
            NotEditableInfo("Kein Text in der Zwischenablage.");
            return;
        }
        var ntxt = Clipboard.GetText();
        if (row.CellGetString(column) == ntxt) { return; }
        NotEditableInfo(UserEdited(this, ntxt, CursorPosColumn, CursorPosRow, true));
    }

    private void Row_RowRemoved(object sender, RowEventArgs e) => Invalidate_CurrentArrangement();

    private void Row_RowRemoving(object sender, RowEventArgs e) {
        if (IsDisposed) { return; }
        if (e.Row == CursorPosRow?.Row) { CursorPos_Reset(); }
        if (PinnedRows.Contains(e.Row)) {
            PinnedRows.Remove(e.Row);
            Invalidate_AllViewItems(false);
        }
    }

    private RowSortDefinition? SortUsed() => _sortDefinitionTemporary ?? Table?.SortDefinition;

    private void Table_InvalidateView(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        Invalidate();
    }

    private void TXTBox_Close(TextBox? textbox) {
        if (IsDisposed || textbox == null || Table is not { IsDisposed: false }) { return; }
        if (!textbox.Visible) { return; }
        if (textbox.Tag is not List<object?> { Count: >= 2 } tags) {
            textbox.Visible = false;
            return; // Ohne dem hier wird ganz am Anfang ein Ereignis ausgelöst
        }
        var w = textbox.Text;

        ColumnViewItem? column = null;
        RowListItem? row = null;

        if (tags[0] is ColumnViewItem c) { column = c; }
        if (tags[1] is RowListItem r) { row = r; }

        textbox.Tag = null;
        textbox.Visible = false;
        NotEditableInfo(UserEdited(this, w, column, row, true));

        Focus();
    }

    private string UserEdit_NewRowAllowed() {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return "Tabelle verworfen"; }

        if (tb.Column.First is not { IsDisposed: false } fc) { return "Erste Spalte nicht definiert"; }

        if (CurrentArrangement?[fc] is not { IsDisposed: false }) { return "Erste Spalte nicht sichtbar"; }

        string? chunkValue = null;

        if (fc != tb.Column.ChunkValueColumn) {
            chunkValue = FilterCombined.ChunkVal;
        }

        return tb.IsNowNewRowPossible(chunkValue, true);
    }

    #endregion
}