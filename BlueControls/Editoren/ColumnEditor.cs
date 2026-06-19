// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
using BlueControls.Controls;
using BlueControls.Editoren;
using BlueControls.Renderer;
using System.Globalization;
using BlueTable.EventArgs;
using BlueTable.Interfaces;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static BlueBasics.ClassesStatic.Converter;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;
using static BlueTable.Classes.ColumnErrorConstants;

namespace BlueControls.BlueTableDialogs;

internal sealed partial class ColumnEditor : IIsEditor, IHasTable {

    #region Fields

    private readonly TableView? _table;
    private Renderer_Abstract? _renderer;
    private bool _writeAccessLost;

    #endregion

    #region Constructors

    public ColumnEditor() : this(null, null) { }

    public ColumnEditor(ColumnItem? column, TableView? table) : base() {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
        _table = table;

        cbxChunk.ItemAddRange(ItemsOf(typeof(ChunkType)));
        cbxRelationType.ItemAddRange(ItemsOf(typeof(RelationType)));
        cbxRandLinks.ItemAddRange(ItemsOf(typeof(ColumnLineStyle)));
        cbxRandRechts.ItemAddRange(ItemsOf(typeof(ColumnLineStyle)));
        cbxBackground.ItemAddRange(ItemsOf(typeof(ColumnBackgroundStyle)));
        cbxAlign.ItemAddRange(ItemsOf(typeof(AlignmentHorizontal)));
        cbxAdditionalCheck.ItemAddRange(ItemsOf(typeof(AdditionalCheck)));
        cbxScriptType.ItemAddRange(ItemsOf(typeof(ScriptType)));
        cbxTranslate.ItemAddRange(ItemsOf(typeof(TranslationType)));

        foreach (var thisr in Renderer_Abstract.AllRenderers.Instances) {
            cbxRenderer.ItemAdd(ItemOf(thisr.ReadableText(), thisr.MyClassId, thisr.SymbolForReadableText()));
        }

        cbxSort.ItemAddRange(ItemsOf(typeof(SortierTyp)));

        foreach (var thisItem in ColumnFormatHolder.AllFormats.Instances) {
            var bli = new BitmapListItem(thisItem.SymbolForReadableText(), thisItem.KeyName, thisItem.ReadableText(), thisItem.QuickInfo) {
                Padding = 5,
                QuickInfo = thisItem.QuickInfo
            };
            lstStyles.ItemAdd(bli);
        }

        InputItem = column;
    }

    #endregion

    #region Properties

    public Type? EditorFor => typeof(ColumnItem);

    public object? InputItem {
        get;
        set {
            if (value == field) { return; }
            if (IsDisposed) { return; }

            if (tblFilterliste.Table is { IsDisposed: false } tb) {
                tb.Dispose();
            }

            if (field is ColumnItem { IsDisposed: false } oldCol && oldCol.Table is { IsDisposed: false } oldTb) {
                oldTb.WriteAccessChanged -= _table_WriteAccessChanged;
            }

            if (value is not ColumnItem) { return; }

            if (field is ColumnItem { IsDisposed: false }) {
                if (!AllOk()) { return; }
            }

            field = value;

            if (field is ColumnItem { IsDisposed: false } newCol && newCol.Table is { IsDisposed: false } newTb) {
                newTb.WriteAccessChanged += _table_WriteAccessChanged;
            }

            Column_DatenAuslesen();
        }
    }

    public EditorMode Mode { get; set; } = EditorMode.EditItem;
    public EditorMode SupportedModes => EditorMode.EditItem; // EditCopy nicht möglich: Spalte wird inline in Tabelle bearbeitet
    public Table? Table => (InputItem as ColumnItem)?.Table;

    #endregion

    #region Methods

    public object? CreateNewItem() => null;

    protected override void OnFormClosing(FormClosingEventArgs e) {
        if (Table is { IsDisposed: false } closingTb) {
            closingTb.WriteAccessChanged -= _table_WriteAccessChanged;
        }
        base.OnFormClosing(e);
        if (_writeAccessLost) { return; }
        if (!AllOk()) {
            e.Cancel = true;
            return;
        }
        InputItem = null;
    }

    private void _table_WriteAccessChanged(object? sender, WriteAccessChangedEventArgs e) {
        if (e.IsEditable || _writeAccessLost || IsDisposed) { return; }
        _writeAccessLost = true;
        Forms.Notification.Show("Spalten-Editor wird geschlossen:<br>Schreibrechte fehlen (" + e.Reason + ")", ImageCode.Warnung);
        Close();
    }

    private static string ColumnUsage(ColumnItem? column) {
        if (column?.Table is not { IsDisposed: false } tb) { return string.Empty; }

        var t = $"<b><u>Infos zu '{column.ReadableText()}'</b></u>\r";

        if (column.LinkedTable is { } tb2) {
            t += GenQIText("Tabelle", ImageCode.Tabelle, tb2.Caption);
        } else {
            t += GenQIText("Tabelle", ImageCode.Tabelle, tb.Caption);
        }

        if (!column.IsSystemColumn()) {
            t += GenQIText("Typ", column.SymbolForReadableText(), "Normale Spalte");
        } else {
            t += GenQIText("Typ", column.SymbolForReadableText(), "System Spalte");
        }

        //var editable = string.IsNullOrEmpty(CellCollection.IsCellEditable(column, tb.Row.First(), "dummy"));
        //t += GenQIText("Admin bearbeitbar", ImageCode.Stift, editable);

        t += GenQIText("Für Sortierung", ImageCode.AZ, tb.SortDefinition?.UsedColumns.Contains(column) ?? false);

        var cola = new List<string>();
        var tcvc = ColumnViewCollection.ParseAll(tb);
        foreach (var thisView in tcvc) {
            if (thisView[column] is not null) { cola.Add(thisView.KeyName); }
        }
        t += GenQIText("In Spalten-Anord.", ImageCode.Spalte, cola);
        t += GenQIText("In intern. Skripten", ImageCode.Skript, column.UsedInScript());
        t += GenQIText("In Zeilen-Quick-Info", ImageCode.Zeile, tb.RowQuickInfo.Contains(column.KeyName, StringComparison.OrdinalIgnoreCase));
        t += GenQIText("Schlüssel für", ImageCode.Schlüssel, column.Am_A_Key_For);

        if (column.SaveContent) {
            var l = column.Contents();
            t += GenQIText("Verschiedene Werte", ImageCode.Textfeld, l.Count.ToString1());
        } else {
            t += GenQIText("Verschiedene Werte", ImageCode.Textfeld, "[wird nicht gespeichert]");
        }

        if (!string.IsNullOrEmpty(column.ColumnSystemInfo)) {
            t += "\r\r<b><u>Ermittelt:</b></u>\r";
            t += GenQIText("Seit UTC", ImageCode.Uhr, "Seit UTC", column.ColumnSystemInfo);
            t += GenQIText("Bearbeitet in Tab.", ImageCode.Stift, "Edit in Table", column.ColumnSystemInfo);
            t += GenQIText("Gefiltert in Tab.", ImageCode.Trichter, "Filter Clicked", column.ColumnSystemInfo);
            t += GenQIText("Gibt Werte an", ImageCode.Tabelle, "Links to me", column.ColumnSystemInfo);
            t += GenQIText("Ext. Skript-'Filter'", ImageCode.Skript, "Filter in Script", column.ColumnSystemInfo);
            t += GenQIText("Ext. Skript-'Get'", ImageCode.Skript, "Value used in Script", column.ColumnSystemInfo);
            t += GenQIText("Ext. Skript-'Set'", ImageCode.Skript, "Edit with Script", column.ColumnSystemInfo);
            //t += column.ColumnSystemInfo;
        }

        return t;
    }

    private static TabPage? FindParentTabPage(Control? control) {
        while (control is not null) {
            if (control is TabPage tabPage) { return tabPage; }
            control = control.Parent;
        }
        return null;
    }

    private static string GenQIText(string name, QuickImage? quickImage, string text) => $"<b>{quickImage?.HTMLCode} {name}:</b><tab>{text}\r";

    private static string GenQIText(string name, ImageCode img, string text) => GenQIText(name, QuickImage.Get(img, 16), text);

    private static string GenQIText(string name, ImageCode img, bool state) {
        if (state) {
            return $"<b>{QuickImage.Get(img, 16).HTMLCode} {name}:</b><tab>{QuickImage.Get(ImageCode.Häkchen, 16).HTMLCode}\r";
        } else {
            return $"<b>{QuickImage.Get(img, 16).HTMLCode} {name}:</b><tab>{QuickImage.Get(ImageCode.Kreuz, 16).HTMLCode}\r";
        }
    }

    private static string GenQIText(string name, ImageCode img, string suchwert, string columnSystemInfo) {
        var t = columnSystemInfo.SplitAndCutByCrAndBr().SortedDistinctList();

        var l = t.TagGetAll(suchwert).ToList();

        return GenQIText(name, img, l);
    }

    private static string GenQIText(string name, ImageCode img, List<string> werte) {
        string t2;

        if (werte.Count == 0) {
            t2 = "-";
        } else if (werte.Count < 4) {
            t2 = string.Join("; ", werte);
        } else {
            t2 = string.Join("; ", werte.Take(4)) + "; ...";
        }

        return $"<b>{QuickImage.Get(img, 16).HTMLCode} {name} </b><i>({werte.Count})</i><b>:</b><tab>{t2}\r";
    }

    private void _renderer_DoUpdateSideOptionMenu(object? sender, System.EventArgs e) => _renderer.DoForm(RendererEditor);

    private bool AllOk() {
        var feh = string.Empty;

        if (InputItem is not ColumnItem { IsDisposed: false } column) {
            feh = "Spalte verworfen!";
        } else {
            if (!txbName.Text.StartsWith("SYS_", StringComparison.OrdinalIgnoreCase)) {
                if (!column.ColumNameAllowed(txbName.Text)) { feh = "Spaltenname nicht erlaubt!"; }
            }
        }

        if (string.IsNullOrEmpty(feh)) {
            Column_DatenZurückschreiben();
            if (string.IsNullOrEmpty(feh) && InputItem is ColumnItem { IsDisposed: false } col2) { feh = col2.ErrorReason(); }
        }

        if (!string.IsNullOrEmpty(feh)) {
            var solutions = GenerateSolutions(feh);

            if (solutions.Count == 0) {
                Forms.MessageBox.Show($"<b><u>Bitte korrigieren sie zuerst folgenden Fehler:</u></b><br>{feh}", ImageCode.Warnung, "Ok");
            } else {
                InputBoxListBoxStyle.Show($"<b><u>Bitte korrigieren sie zuerst folgenden Fehler:</u></b><br>{feh}", solutions, true);
            }

            return false;
        }

        (InputItem as ColumnItem)?.Repair();
        return true;
    }

    private void btnBackColor_Click(object sender, System.EventArgs e) {
        ColorDia.Color = QuickImage.Get(btnBackColor.ImageCode).ChangeGreenTo ?? Color.White;
        ColorDia.ShowDialog();
        btnBackColor.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, ColorDia.Color).KeyName;
    }

    private void btnCalculateMaxCellLength_Click(object sender, System.EventArgs e) {
        if (InputItem is not ColumnItem c) { return; }

        txbMaxCellLength.Text = c.CalculatePreveredMaxCellLength(1.2f).ToString1();
    }

    private void btnMaxTextLength_Click(object sender, System.EventArgs e) {
        if (InputItem is not ColumnItem c) { return; }
        txbMaxTextLength.Text = c.CalculatePreveredMaxTextLength(1.2f).ToString1();
    }

    private void btnOk_Click(object sender, System.EventArgs e) {
        if (!AllOk()) { return; }
        Close();
    }

    private void btnQI_Vorschau_Click(object sender, System.EventArgs e) => Notification.Show(txbQuickinfo.Text.Replace("\r", "<br>") + "<br><br><br>" + txbAdminInfo.Text.Replace("\r", "<br>"));

    private void btnSpaltenkopf_Click(object sender, System.EventArgs e) {
        if (IsDisposed || (InputItem as ColumnItem)?.Table is not { IsDisposed: false } tb) { return; }
        if (TableViewForm.EditableErrorMessage(tb, null)) { return; }

        tb.Edit();
    }

    private void btnStandard_Click(object sender, System.EventArgs e) {
        if (IsDisposed || InputItem is not ColumnItem { IsDisposed: false } c) { return; }
        if (!AllOk()) { return; }
        c.ResetSystemToDefault(true);
        Column_DatenAuslesen();
    }

    private void btnSystemInfo_Click(object sender, System.EventArgs e) {
        if (IsDisposed || (InputItem as ColumnItem)?.Table is not { IsDisposed: false } tb) { return; }

        foreach (var item in tb.Column) {
            item.SystemInfoReset(true);
        }

        capInfos.Text = ColumnUsage(InputItem as ColumnItem);
    }

    private void btnTextColor_Click(object sender, System.EventArgs e) {
        ColorDia.Color = QuickImage.Get(btnTextColor.ImageCode).ChangeGreenTo ?? Color.White;
        ColorDia.ShowDialog();
        btnTextColor.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, ColorDia.Color).KeyName;
    }

    private void butAktuellVor_Click(object sender, System.EventArgs e) {
        if (InputItem is not ColumnItem { IsDisposed: false } c) { return; }

        if (IsDisposed || c.Table is not { IsDisposed: false }) { return; }
        if (!AllOk()) { return; }
        var cu = _table?.CurrentArrangement?[c];
        InputItem = _table?.CurrentArrangement?.NextVisible(cu)?.Column ?? c;
        Column_DatenAuslesen();
    }

    private void butAktuellZurueck_Click(object sender, System.EventArgs e) {
        if (InputItem is not ColumnItem { IsDisposed: false } c) { return; }

        if (IsDisposed || c.Table is not { IsDisposed: false }) { return; }
        if (!AllOk()) { return; }
        var cu = _table?.CurrentArrangement?[c];
        InputItem = _table?.CurrentArrangement?.PreviousVisible(cu)?.Column ?? c;
        Column_DatenAuslesen();
    }

    private void cbxLinkedTable_TextChanged(object sender, System.EventArgs e) {
        if (InputItem is not ColumnItem { IsDisposed: false } c) { return; }

        c.LinkedTableTableName = cbxLinkedTable.Text;

        cbxTargetColumn.ItemClear();

        if (c.LinkedTable is not null) {
            foreach (var thisLinkedColumn in c.LinkedTable.Column) {
                if (thisLinkedColumn.CanBeCheckedByRules() && thisLinkedColumn.RelationType == RelationType.None) {
                    cbxTargetColumn.ItemAdd(ItemOf(thisLinkedColumn));
                }
            }
        }
        cbxTargetColumn.Text = c.ColumnKeyOfLinkedTable;
        //SetKeyTo(cbxTargetColumn, _column.LinkedCell_ColumnKeyOfLinkedTable);
        cbxTargetColumn.Enabled = cbxTargetColumn.ItemCount > 0;
        capTargetColumn.Enabled = cbxTargetColumn.Enabled;
        if (!cbxTargetColumn.Enabled) {
            cbxTargetColumn.Text = string.Empty;
        }

        GeneratFilterListe();
    }

    private void cbxRenderer_TextChanged(object sender, System.EventArgs e) {
        _renderer?.DoUpdateSideOptionMenu -= _renderer_DoUpdateSideOptionMenu;

        if (InputItem is ColumnItem c && !string.IsNullOrEmpty(cbxRenderer.Text)) {
            _renderer = ParseableItem.NewByTypeName<Renderer_Abstract>(cbxRenderer.Text);
            _renderer?.Parse(c.RendererSettings);
        } else {
            _renderer = null;
        }

        _renderer_DoUpdateSideOptionMenu(_renderer, System.EventArgs.Empty);

        _renderer?.DoUpdateSideOptionMenu += _renderer_DoUpdateSideOptionMenu;
    }

    private void cbxTargetColumn_TextChanged(object sender, System.EventArgs e) => GeneratFilterListe();

    private void Column_DatenAuslesen() {
        if (InputItem is not ColumnItem { IsDisposed: false } c) { return; }

        capTabellenname.Text = LanguageTool.DoTranslate("<b>Tabellenname: </b>{0}", true, c.Table?.KeyName);

        cbxLinkedTable.ItemClear();

        lbxCellEditor.Suggestions.Clear();

        lbxCellEditor.ItemAddRange(TableView.Permission_AllUsed(true));

        if (_table?.CurrentArrangement is { IsDisposed: false } cu) {
            var cvi = cu[c];
            butAktuellZurueck.Enabled = cu.PreviousVisible(cvi) is not null;
            butAktuellVor.Enabled = cu.NextVisible(cvi) is not null;
        } else {
            butAktuellVor.Enabled = false;
            butAktuellZurueck.Enabled = false;
        }

        if (!c.IsSystemColumn()) {
            btnStandard.Enabled = false;
            capInfo.Text = c.SymbolForReadableText()?.HTMLCode + " Normale Spalte";
        } else {
            btnStandard.Enabled = true;
            capInfo.Text = c.SymbolForReadableText()?.HTMLCode + " System Spalte";
        }

        txbName.Text = c.KeyName;
        txbName.Enabled = !c.KeyName.StartsWith("SYS_", StringComparison.Ordinal);

        txbName.AllowedChars = Constants.AllowedCharsVariableName;
        txbCaption.Text = c.Caption;
        btnBackColor.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, c.BackColor).KeyName;
        btnTextColor.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, c.ForeColor).KeyName;
        chkMultiline.Checked = c.MultiLine;
        chkIsFirst.Checked = c.IsFirst;
        chkIsKeyColumn.Checked = c.IsKeyColumn;
        chkRelation.Checked = c.Relationship_to_First;
        cbxRelationType.Text = ((int)c.RelationType).ToString1();
        cbxChunk.Text = ((int)c.Value_for_Chunk).ToString1();
        cbxRandLinks.Text = ((int)c.LineStyleLeft).ToString1();
        cbxRandRechts.Text = ((int)c.LineStyleRight).ToString1();
        cbxAlign.Text = ((int)c.Align).ToString1();
        cbxBackground.Text = ((long)c.BackgroundStyle).ToString1();
        cbxAdditionalCheck.Text = ((int)c.AdditionalFormatCheck).ToString1();
        cbxScriptType.Text = ((int)c.ScriptType).ToString1();
        cbxTranslate.Text = ((int)c.DoOpticalTranslation).ToString1();
        cbxRenderer.Text = c.DefaultRenderer;
        cbxRenderer_TextChanged(cbxRenderer, System.EventArgs.Empty);
        cbxSort.Text = ((int)c.SortType).ToString1();
        btnAutoFilterMoeglich.Checked = c.FilterOptions.HasFlag(FilterOptions.Enabled);
        btnAutoFilterTXTErlaubt.Checked = c.FilterOptions.HasFlag(FilterOptions.TextFilterEnabled);
        btnAutoFilterErweitertErlaubt.Checked = c.FilterOptions.HasFlag(FilterOptions.ExtendedFilterEnabled);
        chkFilterOnlyOr.Checked = c.FilterOptions.HasFlag(FilterOptions.OnlyOrAllowed);
        chkFilterOnlyAND.Checked = c.FilterOptions.HasFlag(FilterOptions.OnlyAndAllowed);
        btnZeilenFilterIgnorieren.Checked = c.IgnoreAtRowFilter;
        btnEditableStandard.Checked = c.EditableWithTextInput;
        btnEditableDropdown.Checked = c.EditableWithDropdown;
        btnRequired.Checked = c.ValueRequired;
        btnAutoEditAutoSort.Checked = c.AfterEditQuickSortRemoveDouble;
        txbRunden.Text = c.AfterEditRound is > -1 and < 7 ? c.AfterEditRound.ToString1() : string.Empty;
        txbFixedColumnWidth.Text = c.FixedColumnWidth > 0 ? c.FixedColumnWidth.ToString1() : string.Empty;
        btnAutoEditToUpper.Checked = c.AfterEditDoUCase;
        btnAutoEditKleineFehler.Checked = c.AfterEditAutoCorrect;
        txbJoker.Text = c.AutoFilterJoker;
        txbUeberschift1.Text = c.CaptionGroup1;
        txbUeberschift2.Text = c.CaptionGroup2;
        txbUeberschift3.Text = c.CaptionGroup3;
        txbSpaltenbild.Text = c.CaptionBitmapCode;
        chkSaveContent.Checked = c.SaveContent;
        chkFormatierungErlaubt.Checked = c.TextFormatingAllowed;
        btnSpellChecking.Checked = c.SpellCheckingEnabled;
        txbAuswaehlbareWerte.Text = string.Join('\r', c.DropDownItems);
        txbAutoReplace.Text = string.Join('\r', c.AfterEditAutoReplace);
        txbRegex.Text = c.RegexCheck;
        txbTags.Text = string.Join('\r', c.ColumnTags);
        lbxCellEditor.Check(c.PermissionGroupsChangeCell, true);
        txbAllowedChars.Text = c.AllowedChars;
        txbMaxTextLength.Text = c.MaxTextLength.ToString1();
        txbMaxCellLength.Text = c.MaxCellLength.ToString1();
        btnOtherValuesToo.Checked = c.ShowValuesOfOtherCellsInDropdown;
        btnIgnoreLock.Checked = c.EditAllowedDespiteLock;
        txbAdminInfo.Text = c.AdminInfo.Replace("<br>", "\r", RegexOptions.IgnoreCase);
        txbQuickinfo.Text = c.QuickInfo.Replace("<br>", "\r", RegexOptions.IgnoreCase);
        cbxLinkedTable.Text = c.LinkedTableTableName;
        txbAutoRemove.Text = c.AfterEditAutoRemoveChar;
        cbxLinkedTable_TextChanged(cbxLinkedTable, System.EventArgs.Empty);
        capInfos.Text = ColumnUsage(c);
    }

    private void Column_DatenZurückschreiben() {
        if (TableViewForm.EditableErrorMessage((InputItem as ColumnItem)?.Table, null)) { return; }

        if (InputItem is not ColumnItem { IsDisposed: false } c) { return; }
        if (IsClosed) { return; }

        if (c.ColumNameAllowed(txbName.Text)) {
            c.KeyName = txbName.Text;
        }

        c.Caption = txbCaption.Text.Replace("\r\n", "\r").Trim().Trim("\r").Trim();
        c.QuickInfo = txbQuickinfo.Text.Replace("\r", "<br>");
        c.AdminInfo = txbAdminInfo.Text.Replace("\r", "<br>");
        c.BackColor = QuickImage.Get(btnBackColor.ImageCode).ChangeGreenTo ?? Color.White;
        c.ForeColor = QuickImage.Get(btnTextColor.ImageCode).ChangeGreenTo ?? Color.White;
        c.LineStyleLeft = (ColumnLineStyle)IntParse(cbxRandLinks.Text);
        c.LineStyleRight = (ColumnLineStyle)IntParse(cbxRandRechts.Text);
        c.RelationType = (RelationType)IntParse(cbxRelationType.Text);
        c.Value_for_Chunk = (ChunkType)IntParse(cbxChunk.Text);
        c.MultiLine = chkMultiline.Checked;
        c.IsFirst = chkIsFirst.Checked;
        c.IsKeyColumn = chkIsKeyColumn.Checked;
        c.Relationship_to_First = chkRelation.Checked;
        c.AfterEditQuickSortRemoveDouble = btnAutoEditAutoSort.Checked;
        if (txbRunden.Text.IsLong()) {
            var zahl = Math.Clamp(IntParse(txbRunden.Text), -1, 7);
            c.AfterEditRound = zahl;
        } else {
            c.AfterEditRound = -1;
        }

        if (txbFixedColumnWidth.Text.IsLong()) {
            var zahl = Math.Max(IntParse(txbFixedColumnWidth.Text), 0);
            c.FixedColumnWidth = zahl;
        } else {
            c.FixedColumnWidth = 0;
        }

        c.AfterEditDoUCase = btnAutoEditToUpper.Checked;
        c.AfterEditAutoCorrect = btnAutoEditKleineFehler.Checked;
        c.SaveContent = chkSaveContent.Checked;
        c.TextFormatingAllowed = chkFormatierungErlaubt.Checked;
        c.SpellCheckingEnabled = btnSpellChecking.Checked;
        var tmpf = FilterOptions.None;
        if (btnAutoFilterMoeglich.Checked) { tmpf |= FilterOptions.Enabled; }
        if (btnAutoFilterTXTErlaubt.Checked) { tmpf |= FilterOptions.TextFilterEnabled; }
        if (btnAutoFilterErweitertErlaubt.Checked) { tmpf |= FilterOptions.ExtendedFilterEnabled; }
        if (chkFilterOnlyOr.Checked) { tmpf |= FilterOptions.OnlyOrAllowed; }
        if (chkFilterOnlyAND.Checked) { tmpf |= FilterOptions.OnlyAndAllowed; }
        c.FilterOptions = tmpf;
        c.IgnoreAtRowFilter = btnZeilenFilterIgnorieren.Checked;
        c.PermissionGroupsChangeCell = new(lbxCellEditor.Checked);
        c.DropDownItems = txbAuswaehlbareWerte.Text.SplitAndCutByCr().SortedDistinctList().AsReadOnly();
        c.AfterEditAutoReplace = new(txbAutoReplace.Text.SplitAndCutByCr());

        c.AutoFilterJoker = txbJoker.Text;
        c.CaptionGroup1 = txbUeberschift1.Text;
        c.CaptionGroup2 = txbUeberschift2.Text;
        c.CaptionGroup3 = txbUeberschift3.Text;
        c.CaptionBitmapCode = txbSpaltenbild.Text;
        c.ColumnTags = txbTags.Text;
        c.RegexCheck = txbRegex.Text;
        c.EditableWithTextInput = btnEditableStandard.Checked;
        c.EditableWithDropdown = btnEditableDropdown.Checked;
        c.ValueRequired = btnRequired.Checked;
        c.ShowValuesOfOtherCellsInDropdown = btnOtherValuesToo.Checked;
        c.EditAllowedDespiteLock = btnIgnoreLock.Checked;
        c.AllowedChars = txbAllowedChars.Text;
        c.MaxTextLength = IntParse(txbMaxTextLength.Text);
        c.MaxCellLength = IntParse(txbMaxCellLength.Text);
        c.LinkedTableTableName = cbxLinkedTable.Text;
        c.ColumnKeyOfLinkedTable = cbxTargetColumn.Text;
        c.Align = (AlignmentHorizontal)IntParse(cbxAlign.Text);
        c.BackgroundStyle = (ColumnBackgroundStyle)LongParse(cbxBackground.Text);
        c.AdditionalFormatCheck = (AdditionalCheck)IntParse(cbxAdditionalCheck.Text);
        c.ScriptType = (ScriptType)IntParse(cbxScriptType.Text);
        c.DoOpticalTranslation = (TranslationType)IntParse(cbxTranslate.Text);
        c.DefaultRenderer = cbxRenderer.Text;
        c.RendererSettings = _renderer?.ParseableItems().FinishParseable() ?? string.Empty;
        c.SortType = (SortierTyp)IntParse(cbxSort.Text);
        c.AfterEditAutoRemoveChar = txbAutoRemove.Text;

        if (c.RelationType == RelationType.None) {
            c.LinkedTableTableName = string.Empty;
            c.ColumnKeyOfLinkedTable = string.Empty;
        }

        c.Invalidate_ColumAndContent();
        GetLinkedCellFilter();

        c.Repair();

        //cbxRenderer.Text = string.Empty;
    }

    private AbstractListItem CreateSolution(string text, Action action, params Control[]? focusControls) {
        var item = ItemOf(text, text, QuickImage.Get(ImageCode.Häkchen, 16), false, true, string.Empty);

        Action combinedAction = () => {
            action.Invoke();
            if (focusControls is not null && focusControls.Length > 0) {
                OpenTabAndFocusControl(focusControls[0]);
            }
        };

        item.LeftClickExecute = (sender, e) => combinedAction();

        return item;
    }

    private List<AbstractListItem> GenerateSolutions(string fehler) {
        var solutions = new List<AbstractListItem>();

        //if (fehler == ColumnKeyInvalid) {
        //    solutions.Add(CreateSolution("Anderen Spaltennamen (Schlüssel) eingeben", () => txbName.Text = (InputItem as ColumnItem)?.KeyName ?? string.Empty, txbName));
        //}

        if (fehler == CellSizeTooSmall) {
            solutions.Add(CreateSolution("Zellengröße anpassen", () => txbMaxCellLength.Text = ((InputItem as ColumnItem)?.MaxTextLength ?? 64).ToString(CultureInfo.InvariantCulture), txbMaxCellLength));
        }

        if (fehler == CellSizeTooLarge) {
            solutions.Add(CreateSolution("Zellengröße auf 4000 setzen", () => txbMaxCellLength.Text = "4000", txbMaxCellLength));
        }

        if (fehler == MaxLengthTooLarge) {
            solutions.Add(CreateSolution("Maximallänge auf 4000 setzen", () => txbMaxTextLength.Text = "4000", txbMaxTextLength));
        }

        //if (fehler == CaptionMissing) {
        //    solutions.Add(CreateSolution("Beschriftung eingeben", () => txbCaption.Text = Column?.Caption ?? string.Empty, txbCaption));
        //}

        if (fehler is ScriptTypeUndefined or LinkedCellScriptInvalid) {
            solutions.Add(CreateSolution("Skript-Typ auf 'Nicht vorhanden' setzen", () => cbxScriptType.Text = ((int)ScriptType.Nicht_vorhanden).ToString1(), cbxScriptType));
        }

        if (fehler == KeyColumnScriptReadonly) {
            solutions.Add(CreateSolution("Skript-Typ auf 'String Readonly' setzen", () => cbxScriptType.Text = ((int)ScriptType.String_Readonly).ToString1(), cbxScriptType));
        }

        if (fehler == RendererMissing) {
            solutions.Add(CreateSolution("Einzeiligen-Renderer setzen", () => cbxRenderer.Text = Renderer_TextOneLine.ClassId, cbxRenderer));
            solutions.Add(CreateSolution("Mehrzeiligen-Renderer setzen", () => cbxRenderer.Text = Renderer_ImageAndText.ClassId, cbxRenderer));
        }

        if (fehler is LinkedTableMissing
                   or CircularReference
                   or LinkedCellScriptInvalid
                   or ChunkNoRelation
                   or FirstColumnNoRelation
                   or KeyColumnNoRowRelation) {
            solutions.Add(CreateSolution("Verbindung zu anderer Tabelle entfernen", () => { cbxLinkedTable.Text = string.Empty; cbxRelationType.Text = ((int)RelationType.None).ToString1(); }, cbxLinkedTable));
        }

        if (fehler == LinkedKeyColumnMissing) {
            solutions.Add(CreateSolution("Verknüpfte Spalte auswählen", () => { }, cbxTargetColumn));
        }

        if (fehler == NoLinkedFilterDefined) {
            solutions.Add(CreateSolution("Filter definieren", () => { }, tblFilterliste));
        }

        if (fehler == RelationRequiresMultiline) {
            solutions.Add(CreateSolution("Mehrzeilig aktivieren", () => chkMultiline.Checked = true, chkMultiline));
            solutions.Add(CreateSolution("'Mit erster Spalte abgleichen' deaktivieren", () => chkRelation.Checked = false, chkRelation));
        }

        if (fehler == FixedWidthRequired) {
            solutions.Add(CreateSolution("Feste Spaltenbreite setzen", () => txbFixedColumnWidth.Text = "100", txbFixedColumnWidth));
        }

        if (fehler is MustIgnoreRowFilter
                   or ChunkMustIgnoreRowFilter) {
            solutions.Add(CreateSolution("Bei Zeilenfiltern ignorieren", () => btnZeilenFilterIgnorieren.Checked = true, btnZeilenFilterIgnorieren));
        }

        if (fehler is ChunkMustSaveContent
                   or FirstColumnMustSaveContent
                   or KeyColumnMustSaveContent
                   or RelationMustSaveContent) {
            solutions.Add(CreateSolution("Inhalt speichern aktivieren", () => chkSaveContent.Checked = true, chkSaveContent));
        }

        if (fehler == ChunkExtendedFilterInvalid) {
            solutions.Add(CreateSolution("Erweiterte AutoFilter deaktivieren", () => btnAutoFilterErweitertErlaubt.Checked = false, btnAutoFilterErweitertErlaubt));
        }

        if (fehler == SpellCheckNotPossible) {
            solutions.Add(CreateSolution("Rechtschreibprüfung deaktivieren", () => btnSpellChecking.Checked = false, btnSpellChecking));
        }

        if (fehler == NoDropdownItems) {
            solutions.Add(CreateSolution("Werte anderer Zellen anzeigen", () => btnOtherValuesToo.Checked = true, btnOtherValuesToo));
        }

        if (fehler is DropdownNotSelectedAddAll
                   or DropdownNotSelectedItems) {
            solutions.Add(CreateSolution("Dropdown-Menü aktivieren", () => btnEditableDropdown.Checked = true, btnEditableDropdown));
            solutions.Add(CreateSolution("Dropdown-Einstellungen zurücksetzen", () => {
                btnOtherValuesToo.Checked = false;
                btnRequired.Checked = false;
                txbAuswaehlbareWerte.Text = string.Empty;
            }, txbAuswaehlbareWerte));
            if (fehler == DropdownNotSelectedItems) {
                solutions.Add(CreateSolution("Als Vorschläge verwenden (Texteingabe aktivieren)", () => btnEditableStandard.Checked = true, btnEditableStandard));
            }
        }

        if (fehler == RemoveEditPermissions) {
            solutions.Add(CreateSolution("Berechtigungen entfernen", lbxCellEditor.UncheckAll, lbxCellEditor));
        }

        if (fehler == FilterCombinationInvalid) {
            solutions.Add(CreateSolution("Filter zurücksetzen", () => {
                btnAutoFilterMoeglich.Checked = true;
                btnAutoFilterTXTErlaubt.Checked = true;
                btnAutoFilterErweitertErlaubt.Checked = true;
                chkFilterOnlyOr.Checked = false;
                chkFilterOnlyAND.Checked = false;
            }, btnAutoFilterMoeglich));
        }

        if (fehler is FirstColumnNoRelation
                   or ValueRequiredMissingFirst
                   or EmptyNotAllowed) {
            solutions.Add(CreateSolution("Nicht als 'Erste Spalte' markieren", () => chkIsFirst.Checked = false, chkIsFirst));
        }

        if (fehler == KeyColumnScriptReadonly) {
            solutions.Add(CreateSolution("Nicht als 'Schlüsselspalte' markieren", () => chkIsKeyColumn.Checked = false, chkIsKeyColumn));
        }

        if (fehler is ValueRequiredMissingFirst
                   or ValueRequiredMissingScript) {
            solutions.Add(CreateSolution("Wert erforderlich aktivieren", () => btnRequired.Checked = true, btnRequired));
        }

        if (fehler == ValueRequiredMissingScript) {
            solutions.Add(CreateSolution("Skript-Typ auf 'String' ändern", () => cbxScriptType.Text = ((int)ScriptType.String).ToString1(), cbxScriptType));
        }

        if (fehler == EmptyNotAllowed) {
            solutions.Add(CreateSolution("'Wert erforderlich' deaktivieren", () => btnRequired.Checked = false, btnRequired));
        }

        return solutions;
    }

    private void GeneratFilterListe() {
        if (InputItem is not ColumnItem c) { return; }
        if (IsDisposed || c.Table is not { IsDisposed: false } tb2) { return; }

        c.LinkedTableTableName = cbxLinkedTable.Text;

        var linkedTb = c.LinkedTable;

        if (linkedTb is null || tblFilterliste.Table is not null) { tblFilterliste.Table = null; }

        if (tblFilterliste.Table is not null &&
            !string.Equals(tblFilterliste.Table.Tags.TagGet("Filename").FileNameWithoutSuffix(), linkedTb?.KeyName, StringComparison.OrdinalIgnoreCase)) {
            tblFilterliste.Table = null;
        }

        if (linkedTb is null) { return; }

        if (tblFilterliste.Table is null) {
            var tb = Table.Get();
            //tb.Column.GenerateAndAdd("count", "count", ColumnFormatHolder.IntegerPositive);
            var spn = tb.Column.GenerateAndAdd("SpalteName", "Spalte-Name", ColumnFormatHolder_TextOneLine.Instance);
            if (spn is not { IsDisposed: false }) { return; }
            spn.IsFirst = true;
            var vis = tb.Column.GenerateAndAdd("visible", "visible", ColumnFormatHolder_Bit.Instance);
            if (vis is not { IsDisposed: false }) { return; }
            var sp = tb.Column.GenerateAndAdd("Spalte", "Spalte", ColumnFormatHolder_Systemname.Instance);
            if (sp is not { IsDisposed: false }) { return; }

            var info = tb.Column.GenerateAndAdd("info", "Info", ColumnFormatHolder_ImageCode.Instance);
            if (info is not { IsDisposed: false }) { return; }

            sp.Align = AlignmentHorizontal.Rechts;

            var b = tb.Column.GenerateAndAdd("Such", "Suchtext", ColumnFormatHolder_TextOneLine.Instance);
            if (b is not { IsDisposed: false }) { return; }
            b.QuickInfo = "<b>Entweder</b> ~Spaltenname~<br><b>oder</b> fester Text zum Suchen<br>Mischen wird nicht unterstützt.";
            b.MultiLine = false;
            b.EditableWithTextInput = true;
            b.ValueRequired = true;
            b.EditableWithDropdown = true;

            var dd = b.DropDownItems.Clone();
            var or = new List<string>();

            foreach (var thisColumn in tb2.Column) {
                if (thisColumn.CanBeCheckedByRules() && !thisColumn.MultiLine) {
                    dd.Add("~" + thisColumn.KeyName.ToUpperInvariant() + "~");
                    or.Add("~" + thisColumn.KeyName.ToUpperInvariant() + "~|[Spalte: " + thisColumn.ReadableText() + "]");
                }
            }

            b.DropDownItems = dd.AsReadOnly();

            b.DefaultRenderer = Renderer_ImageAndText.ClassId;

            var s = new Renderer_ImageAndText {
                Text_ersetzen = string.Join('\r', or)
            };
            b.RendererSettings = s.ReadableText();

            tb.RepairAfterParse();
            var tcvc = ColumnViewCollection.ParseAll(tb);

            tcvc[1].Add(sp);
            tcvc[1].Add(b);
            tcvc[1].Add(info);
            tb.ColumnArrangements = tcvc.AsReadOnly();

            tb.SortDefinition = new RowSortDefinition(tb, sp, false);
            tblFilterliste.Table = tb;
            //tblFilterliste.Arrangement = 1;

            var t = tb.Tags.Clone();
            t.TagSet("Filename", linkedTb.KeyName);
            tb.Tags = t.AsReadOnly();

            tblFilterliste.FilterFix.Add(new FilterItem(vis, FilterType.Istgleich, "+"));
        }

        linkedTb.RepairAfterParse(); // Dass ja die 0 Ansicht stimmt

        var columnFromLinkedTb = linkedTb.Column[cbxTargetColumn.Text];

        foreach (var col in linkedTb.Column) {
            var r = tblFilterliste.Table?.Row[col.KeyName] ?? tblFilterliste.Table?.Row.GenerateAndAdd(col.KeyName, "Neue Spalte");

            if (r is not null) {
                r.CellSet("Spalte", col.ReadableText() + " = ", string.Empty);
                r.CellSet("SpalteName", col.KeyName, string.Empty);

                if (col != columnFromLinkedTb && col.RelationType == RelationType.None && !col.IsSystemColumn()) {
                    r.CellSet("visible", true, string.Empty);
                } else {
                    r.CellSet("visible", false, string.Empty);
                }

                if (col == col.Table?.Column.ChunkValueColumn) {
                    r.CellSet("info", "Warnung", string.Empty);
                } else {
                    r.CellSet("info", string.Empty, string.Empty);
                }
            }
        }

        SetLinkedCellFilter();
    }

    /// <summary>
    /// Holt die Werte aus tblFilterliste und schreibt sie in _Column.LinkedCellFilter
    /// Hat tblFilterliste keine Tabelle, bleibt die Variable _Column.LinkedCellFilter unverändert
    /// </summary>
    private void GetLinkedCellFilter() {
        if (IsDisposed || tblFilterliste.Table is not { IsDisposed: false } tb) { return; }
        if (InputItem is not ColumnItem c) { return; }
        if (c.Table is not { IsDisposed: false }) { return; }

        var nf = new List<string>();
        foreach (var thisr in tb.Row) {
            if (thisr.CellGetBoolean("visible") && !string.IsNullOrEmpty(thisr.CellGetString("such"))) {
                nf.Add(thisr.CellGetString("spaltename") + "|=|" + thisr.CellGetString("Such").ToNonCritical());
            }
        }

        c.LinkedCellFilter = nf.AsReadOnly();
    }

    /// <summary>
    /// Holt die Werte aus _Column.LinkedCellFilter und schreibt sie in tblFilterliste
    ///Leer evtl. Werte aus tblFilterliste
    /// </summary>
    private void lstStyles_ItemClicked(object sender, EventArgs.AbstractListItemEventArgs e) {
        var chf = ColumnFormatHolder.AllFormats.Instances.FirstOrDefault(f => f.KeyName == e.Item.KeyName);
        if (chf is null) { return; }

        if (!AllOk()) { return; }
        ((ColumnItem)InputItem).GetStyleFrom(chf);
        Column_DatenAuslesen();
    }

    private void OpenTabAndFocusControl(Control? control) {
        if (control is null) { return; }

        var parentTab = FindParentTabPage(control);
        if (parentTab is not null) {
            tabControl.SelectedTab = parentTab;
        }
        control.Focus();
    }

    private void SetLinkedCellFilter() {
        if (IsDisposed || tblFilterliste.Table is not { IsDisposed: false } tb) { return; }
        if (InputItem is not ColumnItem { IsDisposed: false } c) { return; }

        foreach (var thisr in tb.Row) {
            thisr.CellSet("Such", string.Empty, string.Empty);
        }

        if (tb.Column["SpalteName"] is not { IsDisposed: false } sc) { return; }

        foreach (var thisFi in c.LinkedCellFilter) {
            var x = thisFi.SplitBy("|");

            if (x.Length == 3) {
                var r = tb.Row[new FilterItem(sc, FilterType.Istgleich_GroßKleinEgal, x[0])];

                if (r?.CellGetBoolean("Visible") == true) {
                    r.CellSet("Such", x[2].FromNonCritical(), string.Empty);
                }
            }
        }
    }

    private void tabControl_SelectedIndexChanged(object sender, System.EventArgs e) {
        if (IsDisposed || (InputItem as ColumnItem)?.Table is not { IsDisposed: false } tb) { return; }

        if (tabControl.SelectedTab == tabSpaltenVerlinkung && cbxLinkedTable.ItemCount == 0) {
            var l = Table.AllAvailableTables();

            foreach (var thisString in l) {
                if (!string.Equals(thisString.FileNameWithoutSuffix(), tb.KeyName, StringComparison.OrdinalIgnoreCase)) { cbxLinkedTable.ItemAdd(ItemOf(thisString.FileNameWithoutSuffix(), thisString)); }
            }
        }
    }

    #endregion
}