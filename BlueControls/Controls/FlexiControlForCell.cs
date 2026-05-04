// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
using BlueControls.Classes.ItemCollectionList.TableItems;
using BlueControls.Controls.ConnectedFormula;
using BlueControls.Designer_Support;
using BlueControls.EventArgs;
using BlueControls.Renderer;
using BlueTable.EventArgs;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;
using static BlueBasics.ClassesStatic.IO;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public partial class FlexiControlForCell : GenericControlReciver {

    #region Fields

    private ColumnItem? _column;

    private string _columnKey;
    private RowItem? _lastrow;
    private CancellationTokenSource? _markerCancellation;

    #endregion

    #region Constructors

    /// <summary>
    /// Für den Designer
    /// </summary>
    public FlexiControlForCell() : this(string.Empty, CaptionPosition.Über_dem_Feld, EditTypeFormula.None) { }

    public FlexiControlForCell(string columnKey, CaptionPosition captionPosition, EditTypeFormula editType) : base(false, false, false) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        Size = new Size(300, 300);
        f.ShowInfoWhenDisabled = true;
        f.CaptionPosition = captionPosition;
        f.EditType = editType;
        _columnKey = columnKey;
    }

    #endregion

    #region Properties

    public string Caption {
        get {
            _column ??= Column;

            if (_column != null) {
                return _column.ReadableText() + ":";
            }

            if (!string.IsNullOrEmpty(_columnKey)) {
                return _columnKey + ":";
            }

            return "[?]";
        }
    }

    [DefaultValue(CaptionPosition.Über_dem_Feld)]
    public CaptionPosition CaptionPosition { get => f?.CaptionPosition ?? CaptionPosition.Über_dem_Feld; set => f?.CaptionPosition = value; }

    public ColumnItem? Column {
        get {
            try {
                return _column ??= TableInput is { IsDisposed: false } tb ? tb.Column[_columnKey] : null;
            } catch {
                // Multitasking sei dank kann _table trotzem null sein...
                Develop.AbortAppIfStackOverflow();
                return Column;
            }
        }
    }

    [DefaultValue("")]
    public string ColumnKey {
        get => _columnKey;
        set {
            if (_columnKey == value) { return; }
            _columnKey = value;
            Invalidate_CachedColumn();
            Invalidate();
        }
    }

    [DefaultValue(-1)]
    public int ControlX {
        get => f.ControlX;
        set => f.ControlX = value;
    }

    public EditTypeFormula EditType { get => f.EditType; set => f.EditType = value; }

    public string Value => f.Value;

    #endregion

    #region Methods

    public void StyleFromColumn(ColumnItem? frontcolumn, ColumnItem? backcolumn) {
        f.BeginUpdate();
        try {
            if (backcolumn is { IsDisposed: false } && frontcolumn is { IsDisposed: false }) {
                // Front Column
                f.QuickInfo = RowListItem.QuickInfoText(frontcolumn, string.Empty);
                f.CustomVocabulary = frontcolumn.Table is { } tb ? new HashSet<string>(tb.DictionaryWords) : null;

                var r = TableView.RendererOf(frontcolumn, Constants.Win11);
                f.Suffix = r switch {
                    Renderer_TextOneLine rol => rol.Suffix,
                    Renderer_Number rn => rn.Suffix,
                    _ => string.Empty
                };

                // Back Column
                f.ListItems = ItemsOf(backcolumn, null, 10000, r).ToList();
                f.UserEditDialogType = ColumnItem.UserEditDialogTypeInTable(backcolumn, false);
                f.TextInputAllowed = backcolumn.EditableWithTextInput;
                f.DropdownAllowed = backcolumn.EditableWithDropdown;
                f.ShowValuesOfOtherCellsInDropdown = backcolumn.ShowValuesOfOtherCellsInDropdown;
                f.DropdownItems = backcolumn.DropDownItems;
                f.RaiseChangeDelay = backcolumn.HasAutoRepair ? 10 : 1;
                f.CustomContextMenuItems = new([
                    ItemOf("Öffnen / Ausführen", ImageCode.Blitz, Contextmenu_DateiÖffnen, true),
                    ItemOf("Bild öffnen", ImageCode.Bild, Contextmenu_BildÖffnen, true)
                ]);
            } else {
                f.ListItems = null;
                f.TextInputAllowed = false;
                f.DropdownAllowed = false;
                f.ShowValuesOfOtherCellsInDropdown = false;
                f.DropdownItems = null;
                f.RaiseChangeDelay = 1;
                f.Caption = "[?]";
                f.CustomContextMenuItems = null;
            }

            f.Caption = Caption;
        } finally {
            f.EndUpdate();
        }
    }

    /// <summary>
    /// Verwendete Ressourcen bereinigen.
    /// </summary>
    /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
    protected override void Dispose(bool disposing) {
        if (disposing) {
            _markerCancellation?.Cancel();
            _markerCancellation?.Dispose();
            f.Dispose();
            components?.Dispose();
            RestartMarker(); // Restart beendet den Marker und starten ihn bei Bedarf
        }

        base.Dispose(disposing);
    }

    protected override void HandleChangesNow() {
        if (IsDisposed) { return; }
        if (RowsInputChangedHandled && FilterInputChangedHandled) { return; }

        if (!f.Allinitialized) { return; }

        DoInputFilter(null, false);
        base.HandleChangesNow(); // Erst nach DoInputFilter - so kann die InputTabelle befüllt werden.
        RowsInputChangedHandled = true;
        _lastrow = RowSingleOrNull();
        _column ??= Column;

        if (_lastrow != null) { StyleFromColumn(_column, _lastrow); }
        SetValueFromCell(_column, _lastrow);
        CheckEnabledState(_column, _lastrow);

        if (_lastrow?.CheckRow() is { } rce) {
            TableInput_RowChecked(this, rce);
        }
    }

    protected override void TableInput_CellValueChanged(object sender, CellEventArgs e) {
        try {
            if (InvokeRequired) {
                Invoke(new Action(() => TableInput_CellValueChanged(sender, e)));
                return;
            }

            if (e.Row != _lastrow) { return; }
            if (e.Column == _column) {
                SetValueFromCell(_column, e.Row);
            }

            if (e.Column == _column || e.Column == e.Column.Table?.Column.SysLocked) {
                CheckEnabledState(_column, e.Row);
            }
        } catch {
            // Invoke: auf das verworfene Ojekt blah blah
            if (!IsDisposed) {
                Develop.AbortAppIfStackOverflow();
                TableInput_CellValueChanged(sender, e);
            }
        }
    }

    protected override void TableInput_ColumnPropertyChanged(object sender, ColumnEventArgs e) {
        if (e.Column == _column) {
            Invalidate_FilterInput();
        }
    }

    protected override void TableInput_Loaded(object sender, System.EventArgs e) {
        if (Disposing || IsDisposed) { return; }

        if (InvokeRequired) {
            try {
                Invoke(new Action(() => TableInput_Loaded(sender, e)));
                return;
            } catch {
                // Kann dank Multitasking disposed sein
                Develop.AbortAppIfStackOverflow();
                TableInput_Loaded(sender, e); // am Anfang der Routine wird auf disposed geprüft
                return;
            }
        }

        Invalidate_CachedColumn();
    }

    protected override void TableInput_RowChecked(object sender, RowPrepareFormulaEventArgs e) {
        if (!FilterInputChangedHandled || !RowsInputChangedHandled) { return; }

        if (e.Row != _lastrow) { return; }
        if (e.ColumnsWithErrors == null) {
            f.InfoText = string.Empty;
            return;
        }

        var sb = new System.Text.StringBuilder();
        foreach (var thisString in e.ColumnsWithErrors) {
            var x = thisString.SplitAndCutBy("|");
            if (_column != null && string.Equals(x[0], _column.KeyName, StringComparison.OrdinalIgnoreCase)) {
                if (!string.IsNullOrEmpty(f.InfoText)) { f.InfoText += "<br><hr><br>"; }
                sb.Append(x[1]);
            }
        }
        f.InfoText = sb.ToString();
    }

    private static void Contextmenu_BildÖffnen(object? sender, ContextMenuEventArgs e) {
        if (e.HotItem is not BitmapListItem bi) { return; }
        if (bi.ImageLoaded()) {
            PictureView x = new(bi.Bitmap);
            x.Show();
        }
    }

    private static void Contextmenu_DateiÖffnen(object? sender, ContextMenuEventArgs e) {
        if (e.HotItem is not TextListItem t) { return; }
        if (FileExists(t.KeyName)) {
            ExecuteFile(t.KeyName);
        }
    }

    private async Task ActivateMarker() {
        if (IsDisposed || !Visible || !Enabled) { return; }
        if (f.EditType != EditTypeFormula.Textfeld) { return; }
        if (!FilterInputChangedHandled || !RowsInputChangedHandled) { return; }
        if (string.IsNullOrEmpty(f.Value)) { return; }
        if (_column is not { IsDisposed: false }) { return; }
        if (!_column.Relationship_to_First) { return; }
        if (TableInput is not { IsDisposed: false } tb) { return; }
        if (_lastrow is not { IsDisposed: false }) { return; }

        var col = tb.Column.First;
        if (col == null) { return; }

        var names = col.GetCellContentsSortedByLength().Select(x => x.value).ToList();
        if (names.Count == 0) { return; }

        var ownWord = _lastrow.CellFirstString();
        if (string.IsNullOrEmpty(ownWord)) { return; }

        // Alten Task abbrechen
        _markerCancellation?.Cancel();
        _markerCancellation?.Dispose();
        _markerCancellation = new CancellationTokenSource();

        try {
            await f.HighlightWordsAsync(names, ownWord, _markerCancellation.Token);
        } catch (OperationCanceledException) {
            // Normal bei Cancel
        } catch (Exception ex) {
            Develop.DebugPrint("Marker Fehler: " + ex.Message);
        }
    }

    private void CheckEnabledState(ColumnItem? column, RowItem? row) {
        if (Parent == null) {
            f.DisabledReason = "Kein Bezug zu einem Formular.";
            return;
        }

        if (!Parent.Enabled) {
            f.DisabledReason = "Übergeordnetes Formular deaktiviert.";
            return;
        }

        if (column == null) {
            f.DisabledReason = "Kein Bezug zu einer Spalte.";
            return;
        }

        if (row == null) {
            f.DisabledReason = "Kein Bezug zu einer Zelle.";
            return;
        }

        if (row.Table != column.Table) {
            f.DisabledReason = "Interner Fehler. Admin verständigen.";
            return;
        }

        f.DisabledReason = CellCollection.IsCellEditable(column, row, row.ChunkValue); // Rechteverwaltung einfliesen lassen.
    }

    private void F_ControlAdded(object? sender, ControlEventArgs e) {
        if (e.Control is TextBox textBox) {
            textBox.TextChanged += TextBox_TextChanged;
        }
        Invalidate_RowsInput();
        if (!IsUpdating) { Invalidate(); }
    }

    private void F_ControlRemoved(object? sender, ControlEventArgs e) {
        if (e.Control is TextBox textBox) {
            textBox.TextChanged -= TextBox_TextChanged;
        }
    }

    private void F_EnabledChanged(object? sender, System.EventArgs e) {
        if (Visible && Enabled) { RestartMarker(); }
    }

    private void F_NavigateToNext(object? sender, BlueControls.EventArgs.NavigationDirectionEventArgs e) => NextControl(e.Direction);

    private void F_ValueChanged(object? sender, System.EventArgs e) => ValueToCell();

    private void F_VisibleChanged(object? sender, System.EventArgs e) {
        if (Visible && Enabled) { RestartMarker(); }
    }

    private void Invalidate_CachedColumn() => _column = null;

    private void RestartMarker() {
        if (f.EditType != EditTypeFormula.Textfeld) { return; }
        // Fire-and-forget Pattern für Event-Handler
        Task.Run(async () => {
            try {
                await ActivateMarker();
            } catch (Exception ex) {
                Develop.DebugPrint("RestartMarker Fehler: " + ex.Message);
            }
        });
    }

    private void SetValueFromCell(ColumnItem? column, RowItem? row) {
        if (IsDisposed) { return; }

        if (column == null || row == null) {
            f.Value = string.Empty;
            f.InfoText = string.Empty;
            return;
        }

        f.Value = row.CellGetString(column);
    }

    private void StyleFromColumn(ColumnItem? column, RowItem row) {
        var realColumn = column;

        if (column?.RelationType == RelationType.CellValues) {
            (realColumn, _, _, _) = row.LinkedCellData(column, true, false);
        }

        StyleFromColumn(column, realColumn);
    }

    private void TextBox_TextChanged(object? sender, System.EventArgs e) => RestartMarker();

    private void ValueToCell() {
        if (!f.Enabled) { return; } // Versuch. Eigentlich darf das Steuerelement dann nur empfangen und nix ändern.

        if (_column is not { IsDisposed: false }) { return; }

        if (!FilterInputChangedHandled || !RowsInputChangedHandled) { return; }

        if (_lastrow is not { IsDisposed: false } row) { return; }

        var oldVal = row.CellGetString(_column);
        var newValue = _column.AutoCorrect(f.Value, true);

        if (oldVal == newValue) { return; }

        var cellResult = row.CellSet(_column, newValue, "Über Formular bearbeitet (FlexiControl)");
        if (!string.IsNullOrEmpty(cellResult)) {
            QuickNote.Show(NoteSymbols.Critical, "Fehler");
            return;
        }
        row.CheckRow();
    }

    #endregion
}