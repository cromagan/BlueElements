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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.CellRenderer;
using BlueControls.Controls;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.Interfaces;
using static BlueBasics.Converter;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using MessageBox = BlueControls.Forms.MessageBox;

namespace BlueControls.BlueDatabaseDialogs;

internal sealed partial class ColumnEditor : IIsEditor {

    #region Fields

    private readonly Table? _table;

    private ColumnItem? _column;

    private Renderer_Abstract? _renderer;

    #endregion

    #region Constructors

    public ColumnEditor() : this(null, null) { }

    public ColumnEditor(ColumnItem? column, Table? table) : base() {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
        _table = table;
        _column = column;

        cbxFunction.ItemAddRange(ItemsOf(typeof(ColumnFunction)));
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

            if (value is ColumnItem c) {
                _column = c;
            } else {
                _column = null;
            }
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
        ColorDia.Color = QuickImage.Get(btnBackColor.ImageCode).ChangeGreenTo.FromHtmlCode();
        _ = ColorDia.ShowDialog();
        btnBackColor.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, ColorDia.Color).Code;
    }

    private void btnCalculateMaxCellLenght_Click(object sender, System.EventArgs e) {
        if (_column == null) { return; }

        txbMaxCellLenght.Text = _column.CalculatePreveredMaxCellLenght(1.2f).ToString();
    }

    private void btnMaxTextLenght_Click(object sender, System.EventArgs e) {
        if (_column == null) { return; }
        txbMaxTextLenght.Text = _column.CalculatePreveredMaxTextLenght(1.2f).ToString();
    }

    private void btnOk_Click(object sender, System.EventArgs e) {
        if (!AllOk()) { return; }
        Close();
    }

    private void btnQI_Vorschau_Click(object sender, System.EventArgs e) => Notification.Show(txbQuickinfo.Text.Replace("\r", "<BR>") + "<br><br><br>" + txbAdminInfo.Text.Replace("\r", "<BR>"));

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
        if (IsDisposed || _column?.Database is not { IsDisposed: false }) { return; }
        if (!AllOk()) { return; }
        _column.GetStyleFrom(ColumnFormatHolder.Text);
        Column_DatenAuslesen();
    }

    private void btnSpaltenkopf_Click(object sender, System.EventArgs e) {
        if (IsDisposed || _column?.Database is not { IsDisposed: false } db) { return; }

        db.Edit(typeof(DatabaseHeadEditor));
    }

    private void btnStandard_Click(object sender, System.EventArgs e) {
        if (IsDisposed || _column?.Database is not { IsDisposed: false }) { return; }
        if (!AllOk()) { return; }
        _column.ResetSystemToDefault(true);
        Column_DatenAuslesen();
    }

    private void btnSystemInfo_Click(object sender, System.EventArgs e) {
        if (IsDisposed || _column?.Database is not { IsDisposed: false }) { return; }

        _column.SystemInfoReset(true);
    }

    private void btnTextColor_Click(object sender, System.EventArgs e) {
        ColorDia.Color = QuickImage.Get(btnTextColor.ImageCode).ChangeGreenTo.FromHtmlCode();
        _ = ColorDia.ShowDialog();
        btnTextColor.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, ColorDia.Color).Code;
    }

    private void btnVerwendung_Click(object sender, System.EventArgs e) => MessageBox.Show(Table.ColumnUsage(_column));

    private void butAktuellVor_Click(object sender, System.EventArgs e) {
        if (IsDisposed || _column?.Database is not { IsDisposed: false }) { return; }
        if (!AllOk()) { return; }

        _column = _table?.CurrentArrangement?[_column]?.NextVisible()?.Column ?? _column;
        Column_DatenAuslesen();
    }

    private void butAktuellZurueck_Click(object sender, System.EventArgs e) {
        if (IsDisposed || _column?.Database is not { IsDisposed: false }) { return; }
        if (!AllOk()) { return; }
        _column = _table?.CurrentArrangement?[_column]?.PreviewsVisible()?.Column ?? _column;
        Column_DatenAuslesen();
    }

    private void ButtonCheck() {
        var tmpFormat = (ColumnFunction)IntParse(cbxFunction.Text);
        // Mehrzeilig
        btnMultiline.Enabled = tmpFormat.MultilinePossible();
        if (!tmpFormat.MultilinePossible()) { btnMultiline.Checked = false; }
        // Rechtschreibprüfung
        btnSpellChecking.Enabled = tmpFormat.SpellCheckingPossible();
        if (!tmpFormat.SpellCheckingPossible()) { btnSpellChecking.Checked = false; }
        //// Format: Bildcode
        //grpBildCode.Enabled = tmpFormat == DataFormat.BildCode;
        //if (tmpFormat != DataFormat.BildCode) {
        //    txbBildCodeConstHeight.Text = string.Empty;
        //    cbxBildTextVerhalten.Text = string.Empty;
        //}
        // Format: LinkToFileSystem
        //grpLinkToFileSystem.Enabled = tmpFormat == DataFormat.Link_To_Filesystem;
        //if (tmpFormat != DataFormat.BildCode) {
        //    txbBestFileStandardFolder.Text = string.Empty;
        //    txbBestFileStandardSuffix.Text = string.Empty;
        //}
        // LinkedDatabase - Verknüpfte Datenbank
        //grpLinkedDatabase.Enabled = tmpFormat.NeedTargetDatabase();
        //if (!tmpFormat.NeedTargetDatabase()) {
        //    cbxLinkedDatabase.Text = string.Empty;
        //}

        //// Format: LinkedCell
        //grpVerlinkteZellen.Enabled = tmpFormat is DataFormat.Verknüpfung_zu_anderer_Datenbank;
    }

    private void cbxFunction_TextChanged(object sender, System.EventArgs e) => ButtonCheck();

    /// <summary>
    /// Kümmert sich um erlaubte Spalten für LinkedCell
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>

    private void cbxLinkedDatabase_TextChanged(object? sender, System.EventArgs e) {
        if (_column is not { IsDisposed: false }) { return; }

        _column.LinkedDatabaseTableName = cbxLinkedDatabase.Text;

        cbxTargetColumn.ItemClear();

        if (_column.LinkedDatabase != null) {
            //foreach (var ThisColumn in _Column.Database.Column) {
            //    if (ThisColumn.Format.CanBeCheckedByRules() && !ThisColumn.MultiLine && !ThisColumn.Format.NeedTargetDatabase()) {
            //        cbxRowKeyInColumn.ItemAdd(ThisColumn);
            //    }
            //    //if (ThisColumn.Format == DataFormat.Values_für_LinkedCellDropdownx && ThisColumn.LinkedDatabase() == _Column.LinkedDatabase()) {
            //    //    cbxRowKeyInColumn.ItemAdd(ThisColumn);
            //    //}
            //}
            foreach (var thisLinkedColumn in _column.LinkedDatabase.Column) {
                if (thisLinkedColumn.Function.CanBeCheckedByRules() && !thisLinkedColumn.Function.NeedTargetDatabase()) {
                    cbxTargetColumn.ItemAdd(ItemOf(thisLinkedColumn));
                }
            }
        }
        //cbxTargetColumn.Item.Sort();
        cbxTargetColumn.Text = _column.LinkedCell_ColumnNameOfLinkedDatabase;
        //SetKeyTo(cbxTargetColumn, _column.LinkedCell_ColumnKeyOfLinkedDatabase);
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
            _renderer = ParsebleItem.NewByTypeName<Renderer_Abstract>(cbxRenderer.Text);
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

        capTabellenname.Text = LanguageTool.DoTranslate("<b>Tabellenname: </b>{0}", true, _column.Database?.TableName);

        cbxLinkedDatabase.ItemClear();

        lbxCellEditor.Suggestions.Clear();

        lbxCellEditor.ItemAddRange(Table.Permission_AllUsed(true));

        if (_table?.CurrentArrangement is { } car) {
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
        btnMultiline.Checked = _column.MultiLine;
        cbxFunction.Text = ((int)_column.Function).ToString();
        cbxRandLinks.Text = ((int)_column.LineLeft).ToString();
        cbxRandRechts.Text = ((int)_column.LineRight).ToString();
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
        btnEditableStandard.Checked = _column.TextBearbeitungErlaubt;
        btnEditableDropdown.Checked = _column.DropdownBearbeitungErlaubt;
        btnCanBeEmpty.Checked = _column.DropdownAllesAbwählenErlaubt;
        btnAutoEditAutoSort.Checked = _column.AfterEditQuickSortRemoveDouble;
        txbRunden.Text = _column.RoundAfterEdit is > -1 and < 7 ? _column.RoundAfterEdit.ToString() : string.Empty;
        txbFixedColumnWidth.Text = _column.FixedColumnWidth > 0 ? _column.FixedColumnWidth.ToString() : string.Empty;
        btnAutoEditToUpper.Checked = _column.AfterEditDoUCase;
        btnAutoEditKleineFehler.Checked = _column.AfterEditAutoCorrect;
        txbJoker.Text = _column.AutoFilterJoker;
        txbUeberschift1.Text = _column.CaptionGroup1;
        txbUeberschift2.Text = _column.CaptionGroup2;
        txbUeberschift3.Text = _column.CaptionGroup3;
        txbSpaltenbild.Text = _column.CaptionBitmapCode;
        btnLogUndo.Checked = _column.ShowUndo;
        btnFormatierungErlaubt.Checked = _column.FormatierungErlaubt;
        btnSpellChecking.Checked = _column.SpellCheckingEnabled;
        txbAuswaehlbareWerte.Text = _column.DropDownItems.JoinWithCr();
        txbAutoReplace.Text = _column.AfterEditAutoReplace.JoinWithCr();
        txbRegex.Text = _column.Regex;
        txbTags.Text = _column.Tags.JoinWithCr();
        lbxCellEditor.UncheckAll();
        lbxCellEditor.Check(_column.PermissionGroupsChangeCell);
        txbAllowedChars.Text = _column.AllowedChars;
        txbMaxTextLenght.Text = _column.MaxTextLenght.ToString();
        txbMaxCellLenght.Text = _column.MaxCellLenght.ToString();
        btnOtherValuesToo.Checked = _column.DropdownWerteAndererZellenAnzeigen;
        btnIgnoreLock.Checked = _column.EditAllowedDespiteLock;
        txbAdminInfo.Text = _column.AdminInfo.Replace("<br>", "\r", RegexOptions.IgnoreCase);
        txbQuickinfo.Text = _column.QuickInfo.Replace("<br>", "\r", RegexOptions.IgnoreCase);
        cbxLinkedDatabase.Text = _column.LinkedDatabaseTableName;
        txbAutoRemove.Text = _column.AutoRemove;
        cbxLinkedDatabase_TextChanged(null, System.EventArgs.Empty);
    }

    /// <summary>
    /// Schreibt die Werte hart zurück.
    /// Diese Routine sollte nicht benutzt werden, sondern AllOk
    /// </summary>

    private void Column_DatenZurückschreiben() {
        if (TableView.ErrorMessage(_column?.Database, EditableErrorReasonType.EditAcut) || _column?.Database is null) { return; }

        if (_column.IsDisposed) { return; }
        if (IsClosed) { return; }

        if (_column.ColumNameAllowed(txbName.Text)) {
            _column.KeyName = txbName.Text;
        }

        _column.Caption = txbCaption.Text.Replace("\r\n", "\r").Trim().Trim("\r").Trim();
        _column.Function = (ColumnFunction)IntParse(cbxFunction.Text);
        _column.QuickInfo = txbQuickinfo.Text.Replace("\r", "<BR>");
        _column.AdminInfo = txbAdminInfo.Text.Replace("\r", "<BR>");
        _column.BackColor = QuickImage.Get(btnBackColor.ImageCode).ChangeGreenTo.FromHtmlCode();
        _column.ForeColor = QuickImage.Get(btnTextColor.ImageCode).ChangeGreenTo.FromHtmlCode();
        _column.LineLeft = (ColumnLineStyle)IntParse(cbxRandLinks.Text);
        _column.LineRight = (ColumnLineStyle)IntParse(cbxRandRechts.Text);
        _column.MultiLine = btnMultiline.Checked;
        _column.AfterEditQuickSortRemoveDouble = btnAutoEditAutoSort.Checked;
        if (txbRunden.Text.IsLong()) {
            var zahl = Math.Max(IntParse(txbRunden.Text), -1);
            zahl = Math.Min(zahl, 7);
            _column.RoundAfterEdit = zahl;
        } else {
            _column.RoundAfterEdit = -1;
        }

        if (txbFixedColumnWidth.Text.IsLong()) {
            var zahl = Math.Max(IntParse(txbFixedColumnWidth.Text), 0);
            _column.FixedColumnWidth = zahl;
        } else {
            _column.FixedColumnWidth = 0;
        }

        _column.AfterEditDoUCase = btnAutoEditToUpper.Checked;
        _column.AfterEditAutoCorrect = btnAutoEditKleineFehler.Checked;
        _column.ShowUndo = btnLogUndo.Checked;
        _column.FormatierungErlaubt = btnFormatierungErlaubt.Checked;
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
        _column.DropDownItems = txbAuswaehlbareWerte.Text.SplitAndCutByCrToList().SortedDistinctList().AsReadOnly();
        _column.AfterEditAutoReplace = new(txbAutoReplace.Text.SplitAndCutByCrToList());

        _column.AutoFilterJoker = txbJoker.Text;
        _column.CaptionGroup1 = txbUeberschift1.Text;
        _column.CaptionGroup2 = txbUeberschift2.Text;
        _column.CaptionGroup3 = txbUeberschift3.Text;
        _column.CaptionBitmapCode = txbSpaltenbild.Text;
        _column.Tags = txbTags.Text.SplitAndCutByCrToList();
        _column.Regex = txbRegex.Text;
        _column.TextBearbeitungErlaubt = btnEditableStandard.Checked;
        _column.DropdownBearbeitungErlaubt = btnEditableDropdown.Checked;
        _column.DropdownAllesAbwählenErlaubt = btnCanBeEmpty.Checked;
        _column.DropdownWerteAndererZellenAnzeigen = btnOtherValuesToo.Checked;
        _column.EditAllowedDespiteLock = btnIgnoreLock.Checked;
        _column.AllowedChars = txbAllowedChars.Text;
        _column.MaxTextLenght = IntParse(txbMaxTextLenght.Text);
        _column.MaxCellLenght = IntParse(txbMaxCellLenght.Text);
        _column.LinkedDatabaseTableName = cbxLinkedDatabase.Text; // Muss vor LinkedCell_RowKey zurückgeschrieben werden
        _column.LinkedCell_ColumnNameOfLinkedDatabase = cbxTargetColumn.Text; // LINKED DATABASE
        _column.Align = (AlignmentHorizontal)IntParse(cbxAlign.Text);
        _column.AdditionalFormatCheck = (AdditionalCheck)IntParse(cbxAdditionalCheck.Text);
        _column.ScriptType = (ScriptType)IntParse(cbxScriptType.Text);
        _column.DoOpticalTranslation = (TranslationType)IntParse(cbxTranslate.Text);
        _column.DefaultRenderer = cbxRenderer.Text;
        _column.RendererSettings = _renderer?.ParseableItems().FinishParseable() ?? string.Empty;
        _column.SortType = (SortierTyp)IntParse(cbxSort.Text);
        _column.AutoRemove = txbAutoRemove.Text;
        _column.Invalidate_ColumAndContent();
        GetLinkedCellFilter();

        _column.Repair();

        cbxRenderer.Text = string.Empty;
    }

    private void GeneratFilterListe() {
        if (IsDisposed || _column?.Database is not { IsDisposed: false } db2) { return; }

        _column.LinkedDatabaseTableName = cbxLinkedDatabase.Text;

        var linkdb = _column.LinkedDatabase;

        if (linkdb == null || tblFilterliste.Database != null) { tblFilterliste.DatabaseSet(null, string.Empty); }

        if (tblFilterliste.Database != null &&
            tblFilterliste.Database.Tags.TagGet("Filename") != linkdb?.ConnectionData.UniqueId) {
            tblFilterliste.DatabaseSet(null, string.Empty);
        }

        if (linkdb == null) { return; }

        if (tblFilterliste.Database == null) {
            Database db = new(Database.UniqueKeyValue());
            db.LogUndo = false;
            //db.Column.GenerateAndAdd("count", "count", ColumnFormatHolder.IntegerPositive);
            _ = db.Column.GenerateAndAdd("SpalteName", "Spalte-Name", ColumnFormatHolder.Text);

            var vis = db.Column.GenerateAndAdd("visible", "visible", ColumnFormatHolder.Bit);
            if (vis is not { IsDisposed: false }) { return; }
            var sp = db.Column.GenerateAndAdd("Spalte", "Spalte", ColumnFormatHolder.SystemName);
            if (sp is not { IsDisposed: false }) { return; }

            sp.Align = AlignmentHorizontal.Rechts;

            var b = db.Column.GenerateAndAdd("Such", "Suchtext", ColumnFormatHolder.Text);
            if (b is not { IsDisposed: false }) { return; }
            b.QuickInfo = "<b>Entweder</b> ~Spaltenname~<br><b>oder</b> fester Text zum Suchen<br>Mischen wird nicht unterstützt.";
            b.MultiLine = false;
            b.TextBearbeitungErlaubt = true;
            b.DropdownAllesAbwählenErlaubt = true;
            b.DropdownBearbeitungErlaubt = true;

            var dd = b.DropDownItems.Clone();
            var or = new List<string>();

            foreach (var thisColumn in db2.Column) {
                if (thisColumn.Function.CanBeCheckedByRules() && !thisColumn.MultiLine) {
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

            db.RepairAfterParse();
            var tcvc = ColumnViewCollection.ParseAll(db);

            tcvc[1].Add(sp);
            tcvc[1].Add(b);

            db.ColumnArrangements = tcvc.ToString(false);

            db.SortDefinition = new RowSortDefinition(db, sp, false);
            tblFilterliste.DatabaseSet(db, string.Empty);
            //tblFilterliste.Arrangement = 1;

            var t = db.Tags.Clone();
            t.TagSet("Filename", linkdb.ConnectionData.UniqueId);
            db.Tags = t.AsReadOnly();

            tblFilterliste?.Filter.Add(new FilterItem(vis, FilterType.Istgleich, "+"));
        }

        linkdb.RepairAfterParse(); // Dass ja die 0 Ansicht stimmt

        var spalteauDb = linkdb.Column[cbxTargetColumn.Text];

        foreach (var col in linkdb.Column) {
            var r = tblFilterliste?.Database?.Row[col.KeyName] ?? tblFilterliste?.Database?.Row.GenerateAndAdd(col.KeyName, null, "Neue Spalte");

            if (r != null) {
                r.CellSet("Spalte", col.ReadableText() + " = ", string.Empty);
                r.CellSet("SpalteName", col.KeyName, string.Empty);

                if (col.Function.Autofilter_möglich() && col != spalteauDb && !col.Function.NeedTargetDatabase() && !col.IsSystemColumn()) {
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
    /// Hat tblFilterliste keine Datenbank, bleibt die Variable _Column.LinkedCellFilter unverändert
    /// </summary>

    private void GetLinkedCellFilter() {
        if (IsDisposed || tblFilterliste.Database is not { IsDisposed: false } db) { return; }
        if (_column?.Database is not { IsDisposed: false }) { return; }

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
        if (IsDisposed || tblFilterliste.Database is not { IsDisposed: false } db) { return; }
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
        if (IsDisposed || _column?.Database is not { IsDisposed: false } db) { return; }

        if (tabControl.SelectedTab == tabSpaltenVerlinkung && cbxLinkedDatabase.ItemCount == 0) {
            var l = Database.AllAvailableTables(db.FreezedReason);

            foreach (var thisString in l) {
                if (!string.Equals(thisString.UniqueId, db.ConnectionData.UniqueId, StringComparison.OrdinalIgnoreCase)) { cbxLinkedDatabase.ItemAdd(ItemOf(thisString.TableName, thisString.UniqueId)); }
            }
        }
    }

    #endregion
}