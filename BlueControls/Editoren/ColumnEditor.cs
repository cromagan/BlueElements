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

#nullable enable

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.CellRenderer;
using BlueControls.Controls;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueTable;
using BlueTable.Enums;
using BlueTable.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static BlueBasics.Converter;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using MessageBox = BlueControls.Forms.MessageBox;

namespace BlueControls.BlueTableDialogs;

internal sealed partial class ColumnEditor : IIsEditor {

    #region Fields

    private readonly TableView? _table;

    private ColumnItem? _column;

    private Renderer_Abstract? _renderer;

    #endregion

    #region Constructors

    public ColumnEditor() : this(null, null) {
    }

    public ColumnEditor(ColumnItem? column, TableView? table) : base() {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
        _table = table;
        _column = column;

        cbxChunk.ItemAddRange(ItemsOf(typeof(ChunkType)));
        cbxRelationType.ItemAddRange(ItemsOf(typeof(RelationType)));
        cbxRandLinks.ItemAddRange(ItemsOf(typeof(ColumnLineStyle)));
        cbxRandRechts.ItemAddRange(ItemsOf(typeof(ColumnLineStyle)));
        cbxAlign.ItemAddRange(ItemsOf(typeof(AlignmentHorizontal)));
        cbxAdditionalCheck.ItemAddRange(ItemsOf(typeof(AdditionalCheck)));
        cbxScriptType.ItemAddRange(ItemsOf(typeof(ScriptType)));
        cbxTranslate.ItemAddRange(ItemsOf(typeof(TranslationType)));

        var l = Generic.GetInstaceOfType<Renderer_Abstract>();
        foreach (var thisr in l) {
            cbxRenderer.ItemAdd(ItemOf(thisr.ReadableText(), thisr.MyClassId, thisr.SymbolForReadableText()));
        }

        cbxSort.ItemAddRange(ItemsOf(typeof(SortierTyp)));

        Column_DatenAuslesen();
    }

    #endregion

    #region Properties

    public IEditable? ToEdit {
        set {
            if (value == _column) { return; }
            if (IsDisposed) { return; }

            if (_column is { IsDisposed: false }) { AllOk(); }

            _column = value is ColumnItem c ? c : null;
            Column_DatenAuslesen();
        }
    }

    #endregion

    #region Methods

    protected override void OnFormClosing(FormClosingEventArgs e) {
        base.OnFormClosing(e);
        if (!AllOk()) {
            e.Cancel = true;
        }
    }

    private void _renderer_DoUpdateSideOptionMenu(object? sender, System.EventArgs e) => _renderer.DoForm(RendererEditor);

    private bool AllOk() {
        var feh = string.Empty;

        //// Diese Fehler sind so schwer und darf auf keinen Fall in die Umwelt gelassen werden
        if (_column is not { IsDisposed: false }) {
            feh = "Spalte verworfen!";
        } else {
            if (!txbName.Text.StartsWith("SYS_")) {
                if (!_column.ColumNameAllowed(txbName.Text)) { feh = "Spaltenname nicht erlaubt!"; }
            }
        }

        if (string.IsNullOrEmpty(feh)) {
            Column_DatenZurückschreiben();
            if (string.IsNullOrEmpty(feh)) { feh = _column!.ErrorReason(); }
        }

        if (!string.IsNullOrEmpty(feh)) {
            MessageBox.Show("<b><u>Bitte korrigieren sie zuerst folgenden Fehler:</u></b><br>" + feh, ImageCode.Warnung, "Ok");
            return false;
        }

        _column?.Repair();
        return true;
    }

    private void btnBackColor_Click(object sender, System.EventArgs e) {
        ColorDia.Color = QuickImage.Get(btnBackColor.ImageCode).ChangeGreenTo ?? Color.White;
        ColorDia.ShowDialog();
        btnBackColor.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, ColorDia.Color).Code;
    }

    private void btnCalculateMaxCellLength_Click(object sender, System.EventArgs e) {
        if (_column == null) { return; }

        txbMaxCellLength.Text = _column.CalculatePreveredMaxCellLength(1.2f).ToString();
    }

    private void btnMaxTextLength_Click(object sender, System.EventArgs e) {
        if (_column == null) { return; }
        txbMaxTextLength.Text = _column.CalculatePreveredMaxTextLength(1.2f).ToString();
    }

    private void btnOk_Click(object sender, System.EventArgs e) {
        if (!AllOk()) { return; }
        Close();
    }

    private void btnQI_Vorschau_Click(object sender, System.EventArgs e) => Notification.Show(txbQuickinfo.Text.Replace("\r", "<br>") + "<br><br><br>" + txbAdminInfo.Text.Replace("\r", "<br>"));

    private void btnSchnellAuswahloptionen_Click(object sender, System.EventArgs e) {
        if (!AllOk()) { return; }
        _column.GetStyleFrom(ColumnFormatHolder.TextOptions);
        Column_DatenAuslesen();
    }

    private void btnSchnellBildCode_Click(object sender, System.EventArgs e) {
        if (!AllOk()) { return; }
        _column.GetStyleFrom(ColumnFormatHolder.BildCode);
        Column_DatenAuslesen();
    }

    private void btnSchnellBit_Click(object sender, System.EventArgs e) {
        if (!AllOk()) { return; }
        _column.GetStyleFrom(ColumnFormatHolder.Bit);
        Column_DatenAuslesen();
    }

    private void btnSchnellDatum_Click(object sender, System.EventArgs e) {
        if (!AllOk()) { return; }
        _column.GetStyleFrom(ColumnFormatHolder.Date);
        Column_DatenAuslesen();
    }

    private void btnSchnellDatumUhrzeit_Click(object sender, System.EventArgs e) {
        if (!AllOk()) { return; }
        _column.GetStyleFrom(ColumnFormatHolder.DateTime);
        Column_DatenAuslesen();
    }

    private void btnSchnellEmail_Click(object sender, System.EventArgs e) {
        if (!AllOk()) { return; }
        _column.GetStyleFrom(ColumnFormatHolder.Email);
        Column_DatenAuslesen();
    }

    private void btnSchnellGanzzahl_Click(object sender, System.EventArgs e) {
        if (!AllOk()) { return; }
        _column.GetStyleFrom(ColumnFormatHolder.Long);
        Column_DatenAuslesen();
    }

    private void btnSchnellGleitkommazahl_Click(object sender, System.EventArgs e) {
        if (!AllOk()) { return; }
        _column.GetStyleFrom(ColumnFormatHolder.Float);
        Column_DatenAuslesen();
    }

    private void btnSchnellIInternetAdresse_Click(object sender, System.EventArgs e) {
        if (!AllOk()) { return; }
        _column.GetStyleFrom(ColumnFormatHolder.Url);
        Column_DatenAuslesen();
    }

    private void btnSchnellTelefonNummer_Click(object sender, System.EventArgs e) {
        if (!AllOk()) { return; }
        _column.GetStyleFrom(ColumnFormatHolder.PhoneNumber);
        Column_DatenAuslesen();
    }

    private void btnSchnellText_Click(object sender, System.EventArgs e) {
        if (IsDisposed || _column?.Table is not { IsDisposed: false }) { return; }
        if (!AllOk()) { return; }
        _column.GetStyleFrom(ColumnFormatHolder.Text);
        Column_DatenAuslesen();
    }

    private void btnSpaltenkopf_Click(object sender, System.EventArgs e) {
        if (IsDisposed || _column?.Table is not { IsDisposed: false } tb) { return; }
        if (TableViewForm.EditabelErrorMessage(tb)) { return; }

        tb.Edit(typeof(TableHeadEditor));
    }

    private void btnStandard_Click(object sender, System.EventArgs e) {
        if (IsDisposed || _column?.Table is not { IsDisposed: false }) { return; }
        if (!AllOk()) { return; }
        _column.ResetSystemToDefault(true);
        Column_DatenAuslesen();
    }

    private void btnSystemInfo_Click(object sender, System.EventArgs e) {
        if (IsDisposed || _column?.Table is not { IsDisposed: false }) { return; }

        _column.SystemInfoReset(true);
    }

    private void btnTextColor_Click(object sender, System.EventArgs e) {
        ColorDia.Color = QuickImage.Get(btnTextColor.ImageCode).ChangeGreenTo ?? Color.White;
        ColorDia.ShowDialog();
        btnTextColor.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, ColorDia.Color).Code;
    }

    private void btnVerwendung_Click(object sender, System.EventArgs e) => MessageBox.Show(TableView.ColumnUsage(_column));

    private void butAktuellVor_Click(object sender, System.EventArgs e) {
        if (IsDisposed || _column?.Table is not { IsDisposed: false }) { return; }
        if (!AllOk()) { return; }

        _column = _table?.CurrentArrangement?[_column]?.NextVisible()?.Column ?? _column;
        Column_DatenAuslesen();
    }

    private void butAktuellZurueck_Click(object sender, System.EventArgs e) {
        if (IsDisposed || _column?.Table is not { IsDisposed: false }) { return; }
        if (!AllOk()) { return; }
        _column = _table?.CurrentArrangement?[_column]?.PreviewsVisible()?.Column ?? _column;
        Column_DatenAuslesen();
    }

    /// <summary>
    /// Kümmert sich um erlaubte Spalten für LinkedCell
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>

    private void cbxLinkedTable_TextChanged(object? sender, System.EventArgs e) {
        if (_column is not { IsDisposed: false }) { return; }

        _column.LinkedTableTableName = cbxLinkedTable.Text;

        cbxTargetColumn.ItemClear();

        if (_column.LinkedTable != null) {
            //foreach (var ThisColumn in _Column.Table.Column) {
            //    if (ThisColumn.CanBeCheckedByRules() && !ThisColumn.MultiLine && !ThisColumn.Format.NeedTargetTable()) {
            //        cbxRowKeyInColumn.ItemAdd(ThisColumn);
            //    }
            //    //if (ThisColumn.Format == DataFormat.Values_für_LinkedCellDropdownx && ThisColumn.LinkedTable() == _Column.LinkedTable()) {
            //    //    cbxRowKeyInColumn.ItemAdd(ThisColumn);
            //    //}
            //}
            foreach (var thisLinkedColumn in _column.LinkedTable.Column) {
                if (thisLinkedColumn.CanBeCheckedByRules() && thisLinkedColumn.RelationType == RelationType.None) {
                    cbxTargetColumn.ItemAdd(ItemOf(thisLinkedColumn));
                }
            }
        }
        //cbxTargetColumn.Item.Sort();
        cbxTargetColumn.Text = _column.ColumnNameOfLinkedTable;
        //SetKeyTo(cbxTargetColumn, _column.LinkedCell_ColumnKeyOfLinkedTable);
        cbxTargetColumn.Enabled = cbxTargetColumn.ItemCount > 0;
        capTargetColumn.Enabled = cbxTargetColumn.Enabled;
        if (!cbxTargetColumn.Enabled) {
            cbxTargetColumn.Text = string.Empty;
        }

        GeneratFilterListe();
    }

    private void cbxRenderer_TextChanged(object sender, System.EventArgs e) {
        if (_renderer != null) {
            _renderer.DoUpdateSideOptionMenu -= _renderer_DoUpdateSideOptionMenu;
        }

        if (_column != null && !string.IsNullOrEmpty(cbxRenderer.Text)) {
            _renderer = ParseableItem.NewByTypeName<Renderer_Abstract>(cbxRenderer.Text);
            _renderer?.Parse(_column.RendererSettings);
        } else {
            _renderer = null;
        }

        _renderer_DoUpdateSideOptionMenu(_renderer, System.EventArgs.Empty);

        if (_renderer != null) {
            _renderer.DoUpdateSideOptionMenu += _renderer_DoUpdateSideOptionMenu;
        }
    }

    private void cbxTargetColumn_TextChanged(object sender, System.EventArgs e) => GeneratFilterListe();

    private void Column_DatenAuslesen() {
        if (_column is not { IsDisposed: false }) { return; }

        capTabellenname.Text = LanguageTool.DoTranslate("<b>Tabellenname: </b>{0}", true, _column.Table?.KeyName);

        cbxLinkedTable.ItemClear();

        lbxCellEditor.Suggestions.Clear();

        lbxCellEditor.ItemAddRange(TableView.Permission_AllUsed(true));

        if (_table?.CurrentArrangement is { IsDisposed: false } car) {
            butAktuellZurueck.Enabled = car[_column]?.PreviewsVisible() != null;
            butAktuellVor.Enabled = car[_column]?.NextVisible() != null;
        } else {
            butAktuellVor.Enabled = false;
            butAktuellZurueck.Enabled = false;
        }

        if (!_column.IsSystemColumn()) {
            btnStandard.Enabled = false;
            capInfo.Text = "<Imagecode=" + _column.SymbolForReadableText() + "> Normale Spalte";
        } else {
            btnStandard.Enabled = true;
            capInfo.Text = "<Imagecode=" + _column.SymbolForReadableText() + "> System Spalte";
        }

        txbName.Text = _column.KeyName;
        txbName.Enabled = !_column.KeyName.StartsWith("SYS_");

        txbName.AllowedChars = Constants.AllowedCharsVariableName;
        txbCaption.Text = _column.Caption;
        btnBackColor.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, _column.BackColor).Code;
        btnTextColor.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, _column.ForeColor).Code;
        chkMultiline.Checked = _column.MultiLine;
        chkIsFirst.Checked = _column.IsFirst;
        chkIsKeyColumn.Checked = _column.IsKeyColumn;
        chkRelation.Checked = _column.Relationship_to_First;
        cbxRelationType.Text = ((int)_column.RelationType).ToString();
        cbxChunk.Text = ((int)_column.Value_for_Chunk).ToString();
        cbxRandLinks.Text = ((int)_column.LineStyleLeft).ToString();
        cbxRandRechts.Text = ((int)_column.LineStyleRight).ToString();
        cbxAlign.Text = ((int)_column.Align).ToString();
        cbxAdditionalCheck.Text = ((int)_column.AdditionalFormatCheck).ToString();
        cbxScriptType.Text = ((int)_column.ScriptType).ToString();
        cbxTranslate.Text = ((int)_column.DoOpticalTranslation).ToString();
        cbxRenderer.Text = _column.DefaultRenderer;
        cbxRenderer_TextChanged(cbxRenderer, System.EventArgs.Empty);
        cbxSort.Text = ((int)_column.SortType).ToString();
        btnAutoFilterMoeglich.Checked = _column.FilterOptions.HasFlag(FilterOptions.Enabled);
        btnAutoFilterTXTErlaubt.Checked = _column.FilterOptions.HasFlag(FilterOptions.TextFilterEnabled);
        btnAutoFilterErweitertErlaubt.Checked = _column.FilterOptions.HasFlag(FilterOptions.ExtendedFilterEnabled);
        chkFilterOnlyOr.Checked = _column.FilterOptions.HasFlag(FilterOptions.OnlyOrAllowed);
        chkFilterOnlyAND.Checked = _column.FilterOptions.HasFlag(FilterOptions.OnlyAndAllowed);
        btnZeilenFilterIgnorieren.Checked = _column.IgnoreAtRowFilter;
        btnEditableStandard.Checked = _column.EditableWithTextInput;
        btnEditableDropdown.Checked = _column.EditableWithDropdown;
        btnCanBeEmpty.Checked = _column.DropdownDeselectAllAllowed;
        btnAutoEditAutoSort.Checked = _column.AfterEditQuickSortRemoveDouble;
        txbRunden.Text = _column.AfterEditRound is > -1 and < 7 ? _column.AfterEditRound.ToString() : string.Empty;
        txbFixedColumnWidth.Text = _column.FixedColumnWidth > 0 ? _column.FixedColumnWidth.ToString() : string.Empty;
        btnAutoEditToUpper.Checked = _column.AfterEditDoUCase;
        btnAutoEditKleineFehler.Checked = _column.AfterEditAutoCorrect;
        txbJoker.Text = _column.AutoFilterJoker;
        txbUeberschift1.Text = _column.CaptionGroup1;
        txbUeberschift2.Text = _column.CaptionGroup2;
        txbUeberschift3.Text = _column.CaptionGroup3;
        txbSpaltenbild.Text = _column.CaptionBitmapCode;
        chkSaveContent.Checked = _column.SaveContent;
        chkFormatierungErlaubt.Checked = _column.TextFormatingAllowed;
        btnSpellChecking.Checked = _column.SpellCheckingEnabled;
        txbAuswaehlbareWerte.Text = _column.DropDownItems.JoinWithCr();
        txbAutoReplace.Text = _column.AfterEditAutoReplace.JoinWithCr();
        txbRegex.Text = _column.RegexCheck;
        txbTags.Text = _column.ColumnTags.JoinWithCr();
        lbxCellEditor.UncheckAll();
        lbxCellEditor.Check(_column.PermissionGroupsChangeCell);
        txbAllowedChars.Text = _column.AllowedChars;
        txbMaxTextLength.Text = _column.MaxTextLength.ToString();
        txbMaxCellLength.Text = _column.MaxCellLength.ToString();
        btnOtherValuesToo.Checked = _column.ShowValuesOfOtherCellsInDropdown;
        btnIgnoreLock.Checked = _column.EditAllowedDespiteLock;
        txbAdminInfo.Text = _column.AdminInfo.Replace("<br>", "\r", RegexOptions.IgnoreCase);
        txbQuickinfo.Text = _column.ColumnQuickInfo.Replace("<br>", "\r", RegexOptions.IgnoreCase);
        cbxLinkedTable.Text = _column.LinkedTableTableName;
        txbAutoRemove.Text = _column.AfterEditAutoRemoveChar;
        cbxLinkedTable_TextChanged(null, System.EventArgs.Empty);
    }

    /// <summary>
    /// Schreibt die Werte hart zurück.
    /// Diese Routine sollte nicht benutzt werden, sondern AllOk
    /// </summary>

    private void Column_DatenZurückschreiben() {
        if (TableViewForm.EditabelErrorMessage(_column?.Table)) { return; }

        if (_column is not { IsDisposed: false }) { return; }
        if (IsClosed) { return; }

        if (_column.ColumNameAllowed(txbName.Text)) {
            _column.KeyName = txbName.Text;
        }

        _column.Caption = txbCaption.Text.Replace("\r\n", "\r").Trim().Trim("\r").Trim();
        _column.ColumnQuickInfo = txbQuickinfo.Text.Replace("\r", "<br>");
        _column.AdminInfo = txbAdminInfo.Text.Replace("\r", "<br>");
        _column.BackColor = QuickImage.Get(btnBackColor.ImageCode).ChangeGreenTo ?? Color.White;
        _column.ForeColor = QuickImage.Get(btnTextColor.ImageCode).ChangeGreenTo ?? Color.White;
        _column.LineStyleLeft = (ColumnLineStyle)IntParse(cbxRandLinks.Text);
        _column.LineStyleRight = (ColumnLineStyle)IntParse(cbxRandRechts.Text);
        _column.RelationType = (RelationType)IntParse(cbxRelationType.Text);
        _column.Value_for_Chunk = (ChunkType)IntParse(cbxChunk.Text);
        _column.MultiLine = chkMultiline.Checked;
        _column.IsFirst = chkIsFirst.Checked;
        _column.IsKeyColumn = chkIsKeyColumn.Checked;
        _column.Relationship_to_First = chkRelation.Checked;
        _column.AfterEditQuickSortRemoveDouble = btnAutoEditAutoSort.Checked;
        if (txbRunden.Text.IsLong()) {
            var zahl = Math.Max(IntParse(txbRunden.Text), -1);
            zahl = Math.Min(zahl, 7);
            _column.AfterEditRound = zahl;
        } else {
            _column.AfterEditRound = -1;
        }

        if (txbFixedColumnWidth.Text.IsLong()) {
            var zahl = Math.Max(IntParse(txbFixedColumnWidth.Text), 0);
            _column.FixedColumnWidth = zahl;
        } else {
            _column.FixedColumnWidth = 0;
        }

        _column.AfterEditDoUCase = btnAutoEditToUpper.Checked;
        _column.AfterEditAutoCorrect = btnAutoEditKleineFehler.Checked;
        _column.SaveContent = chkSaveContent.Checked;
        _column.TextFormatingAllowed = chkFormatierungErlaubt.Checked;
        _column.SpellCheckingEnabled = btnSpellChecking.Checked;
        var tmpf = FilterOptions.None;
        if (btnAutoFilterMoeglich.Checked) { tmpf |= FilterOptions.Enabled; }
        if (btnAutoFilterTXTErlaubt.Checked) { tmpf |= FilterOptions.TextFilterEnabled; }
        if (btnAutoFilterErweitertErlaubt.Checked) { tmpf |= FilterOptions.ExtendedFilterEnabled; }
        if (chkFilterOnlyOr.Checked) { tmpf |= FilterOptions.OnlyOrAllowed; }
        if (chkFilterOnlyAND.Checked) { tmpf |= FilterOptions.OnlyAndAllowed; }
        //_Column.AutoFilterErlaubt = AutoFilterMöglich.Checked;
        //_Column.FilterOptions.HasFlag(enFilterOptions.TextFilterEnabled) = AutoFilterTXT.Checked;
        //_Column.FilterOptions.HasFlag(enFilterOptions.ExtendedFilterEnabled) = AutoFilterErw.Checked;
        _column.FilterOptions = tmpf;
        _column.IgnoreAtRowFilter = btnZeilenFilterIgnorieren.Checked;
        _column.PermissionGroupsChangeCell = new(lbxCellEditor.Checked);
        _column.DropDownItems = txbAuswaehlbareWerte.Text.SplitAndCutByCr().SortedDistinctList().AsReadOnly();
        _column.AfterEditAutoReplace = new(txbAutoReplace.Text.SplitAndCutByCr());

        _column.AutoFilterJoker = txbJoker.Text;
        _column.CaptionGroup1 = txbUeberschift1.Text;
        _column.CaptionGroup2 = txbUeberschift2.Text;
        _column.CaptionGroup3 = txbUeberschift3.Text;
        _column.CaptionBitmapCode = txbSpaltenbild.Text;
        _column.ColumnTags = txbTags.Text.SplitAndCutByCr().ToList();
        _column.RegexCheck = txbRegex.Text;
        _column.EditableWithTextInput = btnEditableStandard.Checked;
        _column.EditableWithDropdown = btnEditableDropdown.Checked;
        _column.DropdownDeselectAllAllowed = btnCanBeEmpty.Checked;
        _column.ShowValuesOfOtherCellsInDropdown = btnOtherValuesToo.Checked;
        _column.EditAllowedDespiteLock = btnIgnoreLock.Checked;
        _column.AllowedChars = txbAllowedChars.Text;
        _column.MaxTextLength = IntParse(txbMaxTextLength.Text);
        _column.MaxCellLength = IntParse(txbMaxCellLength.Text);
        _column.LinkedTableTableName = cbxLinkedTable.Text; // Muss vor LinkedCell_RowKey zurückgeschrieben werden
        _column.ColumnNameOfLinkedTable = cbxTargetColumn.Text; // LINKED TABLE
        _column.Align = (AlignmentHorizontal)IntParse(cbxAlign.Text);
        _column.AdditionalFormatCheck = (AdditionalCheck)IntParse(cbxAdditionalCheck.Text);
        _column.ScriptType = (ScriptType)IntParse(cbxScriptType.Text);
        _column.DoOpticalTranslation = (TranslationType)IntParse(cbxTranslate.Text);
        _column.DefaultRenderer = cbxRenderer.Text;
        _column.RendererSettings = _renderer?.ParseableItems().FinishParseable() ?? string.Empty;
        _column.SortType = (SortierTyp)IntParse(cbxSort.Text);
        _column.AfterEditAutoRemoveChar = txbAutoRemove.Text;

        if (_column.RelationType == RelationType.None) {
            _column.LinkedTableTableName = string.Empty;
            _column.ColumnNameOfLinkedTable = string.Empty;
        }

        _column.Invalidate_ColumAndContent();
        GetLinkedCellFilter();

        _column.Repair();

        cbxRenderer.Text = string.Empty;
    }

    private void GeneratFilterListe() {
        if (IsDisposed || _column?.Table is not { IsDisposed: false } db2) { return; }

        _column.LinkedTableTableName = cbxLinkedTable.Text;

        var linkdb = _column.LinkedTable;

        if (linkdb == null || tblFilterliste.Table != null) { tblFilterliste.TableSet(null, string.Empty); }

        if (tblFilterliste.Table != null &&
            !string.Equals(tblFilterliste.Table.Tags.TagGet("Filename").FileNameWithoutSuffix(), linkdb?.KeyName, StringComparison.OrdinalIgnoreCase)) {
            tblFilterliste.TableSet(null, string.Empty);
        }

        if (linkdb == null) { return; }

        if (tblFilterliste.Table == null) {
            var tb = new Table();
            //db.Column.GenerateAndAdd("count", "count", ColumnFormatHolder.IntegerPositive);
            var spn = tb.Column.GenerateAndAdd("SpalteName", "Spalte-Name", ColumnFormatHolder.Text);
            spn.IsFirst = true;
            var vis = tb.Column.GenerateAndAdd("visible", "visible", ColumnFormatHolder.Bit);
            if (vis is not { IsDisposed: false }) { return; }
            var sp = tb.Column.GenerateAndAdd("Spalte", "Spalte", ColumnFormatHolder.SystemName);
            if (sp is not { IsDisposed: false }) { return; }

            sp.Align = AlignmentHorizontal.Rechts;

            var b = tb.Column.GenerateAndAdd("Such", "Suchtext", ColumnFormatHolder.Text);
            if (b is not { IsDisposed: false }) { return; }
            b.ColumnQuickInfo = "<b>Entweder</b> ~Spaltenname~<br><b>oder</b> fester Text zum Suchen<br>Mischen wird nicht unterstützt.";
            b.MultiLine = false;
            b.EditableWithTextInput = true;
            b.DropdownDeselectAllAllowed = true;
            b.EditableWithDropdown = true;

            var dd = b.DropDownItems.Clone();
            var or = new List<string>();

            foreach (var thisColumn in db2.Column) {
                if (thisColumn.CanBeCheckedByRules() && !thisColumn.MultiLine) {
                    dd.Add("~" + thisColumn.KeyName.ToLowerInvariant() + "~");
                    or.Add("~" + thisColumn.KeyName.ToLowerInvariant() + "~|[Spalte: " + thisColumn.ReadableText() + "]");
                }
            }

            b.DropDownItems = dd.AsReadOnly();

            b.DefaultRenderer = Renderer_ImageAndText.ClassId;

            var s = new Renderer_ImageAndText {
                Text_ersetzen = or.JoinWithCr()
            };
            b.RendererSettings = s.ReadableText();

            tb.RepairAfterParse();
            var tcvc = ColumnViewCollection.ParseAll(tb);

            tcvc[1].Add(sp);
            tcvc[1].Add(b);

            tb.ColumnArrangements = tcvc.ToString(false);

            tb.SortDefinition = new RowSortDefinition(tb, sp, false);
            tblFilterliste.TableSet(tb, string.Empty);
            //tblFilterliste.Arrangement = 1;

            var t = tb.Tags.Clone();
            t.TagSet("Filename", linkdb.KeyName);
            tb.Tags = t.AsReadOnly();

            tblFilterliste?.Filter.Add(new FilterItem(vis, FilterType.Istgleich, "+"));
        }

        linkdb.RepairAfterParse(); // Dass ja die 0 Ansicht stimmt

        var spalteauDb = linkdb.Column[cbxTargetColumn.Text];

        foreach (var col in linkdb.Column) {
            var r = tblFilterliste?.Table?.Row[col.KeyName] ?? tblFilterliste?.Table?.Row.GenerateAndAdd(col.KeyName, "Neue Spalte");

            if (r != null) {
                r.CellSet("Spalte", col.ReadableText() + " = ", string.Empty);
                r.CellSet("SpalteName", col.KeyName, string.Empty);

                if (col.IsAutofilterPossible() && col != spalteauDb && col.RelationType == RelationType.None && !col.IsSystemColumn()) {
                    r.CellSet("visible", true, string.Empty);
                } else {
                    r.CellSet("visible", false, string.Empty);
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
        if (IsDisposed || tblFilterliste.Table is not { IsDisposed: false } db) { return; }
        if (_column?.Table is not { IsDisposed: false }) { return; }

        var nf = new List<string>();
        foreach (var thisr in db.Row) {
            if (thisr.CellGetBoolean("visible") && !string.IsNullOrEmpty(thisr.CellGetString("such"))) {
                nf.Add(thisr.CellGetString("spaltename") + "|=|" + thisr.CellGetString("Such").ToNonCritical());
            }
        }

        _column.LinkedCellFilter = nf;
    }

    /// <summary>
    /// Holt die Werte aus _Column.LinkedCellFilter und schreibt sie in tblFilterliste
    ///Leer evtl. Werte aus tblFilterliste
    /// </summary>

    private void SetLinkedCellFilter() {
        if (IsDisposed || tblFilterliste.Table is not { IsDisposed: false } db) { return; }
        if (_column is not { IsDisposed: false }) { return; }

        foreach (var thisr in db.Row) {
            thisr.CellSet("Such", string.Empty, string.Empty);
        }

        if (db.Column["SpalteName"] is not { IsDisposed: false } c) { return; }

        foreach (var thisFi in _column.LinkedCellFilter) {
            var x = thisFi.SplitBy("|");

            if (x.Length == 3) {
                var r = db.Row[new FilterItem(c, FilterType.Istgleich_GroßKleinEgal, x[0])];

                if (r != null && r.CellGetBoolean("Visible")) {
                    r.CellSet("Such", x[2].FromNonCritical(), string.Empty);
                }
            }
        }
    }

    private void tabControl_SelectedIndexChanged(object sender, System.EventArgs e) {
        if (IsDisposed || _column?.Table is not { IsDisposed: false } db) { return; }

        if (tabControl.SelectedTab == tabSpaltenVerlinkung && cbxLinkedTable.ItemCount == 0) {
            var l = Table.AllAvailableTables();

            foreach (var thisString in l) {
                if (!string.Equals(thisString.FileNameWithoutSuffix(), db.KeyName, StringComparison.OrdinalIgnoreCase)) { cbxLinkedTable.ItemAdd(ItemOf(thisString.FileNameWithoutSuffix(), thisString)); }
            }
        }
    }

    #endregion
}