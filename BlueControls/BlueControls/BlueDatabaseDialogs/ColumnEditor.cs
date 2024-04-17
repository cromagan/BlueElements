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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Forms;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static BlueBasics.Converter;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

using MessageBox = BlueControls.Forms.MessageBox;

namespace BlueControls.BlueDatabaseDialogs;

internal sealed partial class ColumnEditor {

    #region Fields

    private readonly Table? _table;
    private ColumnItem? _column;

    #endregion

    #region Constructors

    public ColumnEditor(ColumnItem column, Table? table) : base() {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
        _table = table;
        _column = column;
        Column_DatenAuslesen();
    }

    #endregion

    #region Methods

    protected override void OnFormClosing(FormClosingEventArgs e) {
        base.OnFormClosing(e);
        if (!AllOk()) {
            e.Cancel = true;
        }
    }

    private bool AllOk() {
        var feh = string.Empty;

        //// Diese Fehler sind so schwer und darf auf keinen Fall in die Umwelt gelassen werden
        if (_column == null || _column.IsDisposed) {
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
        btnBackColor.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, ColorDia.Color).ToString();
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
        _column.GetStyleFrom(ColumnFormatHolder.Integer);
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
        if (IsDisposed || _column?.Database is not Database db || db.IsDisposed) { return; }
        if (!AllOk()) { return; }
        _column.GetStyleFrom(ColumnFormatHolder.Text);
        Column_DatenAuslesen();
    }

    private void btnStandard_Click(object sender, System.EventArgs e) {
        if (IsDisposed || _column?.Database is not Database db || db.IsDisposed) { return; }
        if (!AllOk()) { return; }
        _column.ResetSystemToDefault(true);
        Column_DatenAuslesen();
    }

    private void btnTextColor_Click(object sender, System.EventArgs e) {
        ColorDia.Color = QuickImage.Get(btnTextColor.ImageCode).ChangeGreenTo.FromHtmlCode();
        _ = ColorDia.ShowDialog();
        btnTextColor.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, ColorDia.Color).ToString();
    }

    private void btnVerwendung_Click(object sender, System.EventArgs e) => MessageBox.Show(_column?.Useage() ?? "Fehler");

    private void butAktuellVor_Click(object sender, System.EventArgs e) {
        if (IsDisposed || _column?.Database is not Database db || db.IsDisposed) { return; }
        if (!AllOk()) { return; }

        _column = _table?.CurrentArrangement?[_column]?.NextVisible()?.Column;
        Column_DatenAuslesen();
    }

    private void butAktuellZurueck_Click(object sender, System.EventArgs e) {
        if (IsDisposed || _column?.Database is not Database db || db.IsDisposed) { return; }
        if (!AllOk()) { return; }
        _column = _table?.CurrentArrangement?[_column]?.PreviewsVisible()?.Column;
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
        if (_column == null || _column.IsDisposed) { return; }

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
                if (!thisLinkedColumn.IsFirst() && thisLinkedColumn.Function.CanBeCheckedByRules() && !thisLinkedColumn.Function.NeedTargetDatabase()) {
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

    private void cbxTargetColumn_TextChanged(object sender, System.EventArgs e) => GeneratFilterListe();

    private void Column_DatenAuslesen() {
        if (_column == null || _column.IsDisposed) { return; }

        capTabellenname.Text = LanguageTool.DoTranslate("<b>Tabellenname: </b>{0}", true, _column.Database?.TableName);

        cbxFunction.ItemAddRange(ItemsOf(typeof(ColumnFunction)));
        cbxRandLinks.ItemAddRange(ItemsOf(typeof(ColumnLineStyle)));
        cbxRandRechts.ItemAddRange(ItemsOf(typeof(ColumnLineStyle)));
        cbxBildTextVerhalten.ItemAddRange(ItemsOf(typeof(BildTextVerhalten)));
        cbxAlign.ItemAddRange(ItemsOf(typeof(AlignmentHorizontal)));
        cbxAdditionalCheck.ItemAddRange(ItemsOf(typeof(AdditionalCheck)));
        cbxScriptType.ItemAddRange(ItemsOf(typeof(ScriptType)));
        cbxTranslate.ItemAddRange(ItemsOf(typeof(TranslationType)));
        cbxSort.ItemAddRange(ItemsOf(typeof(SortierTyp)));
        cbxLinkedDatabase.ItemClear();

        if (cbxEinheit.ItemCount < 1) {
            cbxEinheit.ItemAdd(ItemOf("µm", ImageCode.Lineal));
            cbxEinheit.ItemAdd(ItemOf("mm", ImageCode.Lineal));
            cbxEinheit.ItemAdd(ItemOf("cm", ImageCode.Lineal));
            cbxEinheit.ItemAdd(ItemOf("dm", ImageCode.Lineal));
            cbxEinheit.ItemAdd(ItemOf("m", ImageCode.Lineal));
            cbxEinheit.ItemAdd(ItemOf("km", ImageCode.Lineal));
            cbxEinheit.ItemAdd(ItemOf("mm²", ImageCode.GrößeÄndern));
            cbxEinheit.ItemAdd(ItemOf("m²", ImageCode.GrößeÄndern));
            cbxEinheit.ItemAdd(ItemOf("µg", ImageCode.Gewicht));
            cbxEinheit.ItemAdd(ItemOf("mg", ImageCode.Gewicht));
            cbxEinheit.ItemAdd(ItemOf("g", ImageCode.Gewicht));
            cbxEinheit.ItemAdd(ItemOf("kg", ImageCode.Gewicht));
            cbxEinheit.ItemAdd(ItemOf("t", ImageCode.Gewicht));
            cbxEinheit.ItemAdd(ItemOf("h", ImageCode.Uhr));
            cbxEinheit.ItemAdd(ItemOf("min", ImageCode.Uhr));
            cbxEinheit.ItemAdd(ItemOf("St.", ImageCode.Eins));
        }
        lbxCellEditor.Suggestions.Clear();

        lbxCellEditor.ItemAddRange(Database.Permission_AllUsed(true));
        if (_table?.CurrentArrangement != null) {
            butAktuellZurueck.Enabled = _table.CurrentArrangement[_column]?.PreviewsVisible() != null;
            butAktuellVor.Enabled = _table.CurrentArrangement[_column]?.NextVisible() != null;
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
        btnBackColor.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, _column.BackColor).ToString();
        btnTextColor.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, _column.ForeColor).ToString();
        btnMultiline.Checked = _column.MultiLine;
        cbxFunction.Text = ((int)_column.Function).ToString();
        cbxRandLinks.Text = ((int)_column.LineLeft).ToString();
        cbxRandRechts.Text = ((int)_column.LineRight).ToString();
        cbxAlign.Text = ((int)_column.Align).ToString();
        cbxAdditionalCheck.Text = ((int)_column.AdditionalFormatCheck).ToString();
        cbxScriptType.Text = ((int)_column.ScriptType).ToString();
        cbxTranslate.Text = ((int)_column.DoOpticalTranslation).ToString();
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
        txbPrefix.Text = _column.Prefix;
        txbSpaltenbild.Text = _column.CaptionBitmapCode;
        btnLogUndo.Checked = _column.ShowUndo;
        btnFormatierungErlaubt.Checked = _column.FormatierungErlaubt;
        btnSpellChecking.Checked = _column.SpellCheckingEnabled;
        txbAuswaehlbareWerte.Text = _column.DropDownItems.JoinWithCr();
        txbReplacer.Text = _column.OpticalReplace.JoinWithCr();
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
        cbxEinheit.Text = _column.Suffix;
        txbBildCodeConstHeight.Text = _column.ConstantHeightOfImageCode;
        cbxBildTextVerhalten.Text = ((int)_column.BehaviorOfImageAndText).ToString();
        //txbBestFileStandardFolder.Text = _column.BestFile_StandardFolder;
        //txbBestFileStandardSuffix.Text = _column.BestFile_StandardSuffix;
        cbxLinkedDatabase.Text = _column.LinkedDatabaseTableName;
        //txbLinkedKeyKennung.Text = _column.LinkedKeyKennung;

        txbAutoRemove.Text = _column.AutoRemove;
        //butSaveContent.Checked = _column.SaveContent;

        //cbxSchlüsselspalte.Item.Clear();
        //cbxSchlüsselspalte.ItemAdd(Add("#Ohne", "-1"));
        //cbxDropDownKey.Item.Clear();
        //cbxDropDownKey.ItemAdd(Add("#Ohne", "-1"));
        //cbxVorschlagSpalte.Item.Clear();
        //cbxVorschlagSpalte.ItemAdd(Add("#Ohne", "-1"));
        // Einige Dropdown-Menüs sind abhängig von der LinkedDatabase und werden in dessen TextChanged-Event befüllt
        // siehe Ende dieser Routine
        //foreach (var thisColumn in _column.Database.Column) {
        //    if (thisColumn.IsOk() && thisColumn.Format.CanBeCheckedByRules()) {
        //        //if (thisColumn.Format == DataFormat.RelationText || !thisColumn.MultiLine) { cbxSchlüsselspalte.ItemAdd(thisColumn); }
        //        if (!thisColumn.MultiLine && !thisColumn.Format.NeedTargetDatabase()) {
        //            //cbxDropDownKey.ItemAdd(thisColumn);
        //            //cbxVorschlagSpalte.ItemAdd(thisColumn);
        //        }
        //    }
        //}
        //cbxSchlüsselspalte.Item.Sort();
        //cbxVorschlagSpalte.Item.Sort();
        //cbxDropDownKey.Item.Sort();

        //SetKeyTo(cbxSchlüsselspalte, _column.KeyColumnKey);
        //SetKeyTo(cbxDropDownKey, _column.DropdownKey);
        //SetKeyTo(cbxVorschlagSpalte, _column.VorschlagsColumn);
        cbxLinkedDatabase_TextChanged(null, System.EventArgs.Empty);
    }

    /// <summary>
    /// Schreibt die Werte hart zurück.
    /// Diese Routine sollte nicht benutzt werden, sondern AllOk
    /// </summary>

    private void Column_DatenZurückschreiben() {
        if (TableView.ErrorMessage(_column?.Database, EditableErrorReasonType.EditAcut) || _column?.Database is null) { return; }

        if (_column.IsDisposed) { return; }

        if (_column.ColumNameAllowed(txbName.Text)) {
            _column.KeyName = txbName.Text;
        }

        _column.Caption = txbCaption.Text.Replace("\r\n", "\r").Trim().Trim("\r").Trim();
        _column.Function = (ColumnFunction)IntParse(cbxFunction.Text);
        _column.QuickInfo = txbQuickinfo.Text.Replace("\r", "<BR>");
        _column.AdminInfo = txbAdminInfo.Text.Replace("\r", "<BR>");
        _column.Suffix = cbxEinheit.Text;
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
        _column.OpticalReplace = new(txbReplacer.Text.SplitAndCutByCrToList());
        _column.AfterEditAutoReplace = new(txbAutoReplace.Text.SplitAndCutByCrToList());

        _column.AutoFilterJoker = txbJoker.Text;
        _column.CaptionGroup1 = txbUeberschift1.Text;
        _column.CaptionGroup2 = txbUeberschift2.Text;
        _column.CaptionGroup3 = txbUeberschift3.Text;
        _column.Prefix = txbPrefix.Text;
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

        _column.ConstantHeightOfImageCode = txbBildCodeConstHeight.Text;
        _ = IntTryParse(cbxBildTextVerhalten.Text, out var imNf);
        _column.BehaviorOfImageAndText = (BildTextVerhalten)imNf;
        //_column.BestFile_StandardFolder = txbBestFileStandardFolder.Text;
        //_column.BestFile_StandardSuffix = txbBestFileStandardSuffix.Text;
        _column.LinkedDatabaseTableName = cbxLinkedDatabase.Text; // Muss vor LinkedCell_RowKey zurückgeschrieben werden
        //_column.KeyColumnKey = ColumKeyFrom(_column.Database, cbxSchlüsselspalte.Text);
        _column.LinkedCell_ColumnNameOfLinkedDatabase = cbxTargetColumn.Text; // LINKED DATABASE
        //_column.VorschlagsColumn = ColumKeyFrom(_column.Database, cbxVorschlagSpalte.Text);
        _column.Align = (AlignmentHorizontal)IntParse(cbxAlign.Text);
        _column.AdditionalFormatCheck = (AdditionalCheck)IntParse(cbxAdditionalCheck.Text);
        _column.ScriptType = (ScriptType)IntParse(cbxScriptType.Text);
        _column.DoOpticalTranslation = (TranslationType)IntParse(cbxTranslate.Text);
        _column.SortType = (SortierTyp)IntParse(cbxSort.Text);
        _column.AutoRemove = txbAutoRemove.Text;
        _column.Invalidate_ColumAndContent();
        GetLinkedCellFilter();

        _column.Repair();
    }

    private void GeneratFilterListe() {
        if (IsDisposed || _column?.Database is not Database db2 || db2.IsDisposed) { return; }

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
            if (vis == null || vis.IsDisposed) { return; }
            var sp = db.Column.GenerateAndAdd("Spalte", "Spalte", ColumnFormatHolder.SystemName);
            if (sp == null || sp.IsDisposed) { return; }

            sp.Align = AlignmentHorizontal.Rechts;

            var b = db.Column.GenerateAndAdd("Such", "Suchtext", ColumnFormatHolder.Text);
            if (b == null || b.IsDisposed) { return; }
            b.QuickInfo = "<b>Entweder</b> ~Spaltenname~<br><b>oder</b> fester Text zum Suchen<br>Mischen wird nicht unterstützt.";
            b.MultiLine = false;
            b.TextBearbeitungErlaubt = true;
            b.DropdownAllesAbwählenErlaubt = true;
            b.DropdownBearbeitungErlaubt = true;

            var dd = b.DropDownItems.Clone();
            var or = b.OpticalReplace.Clone();

            foreach (var thisColumn in db2.Column) {
                if (thisColumn.Function.CanBeCheckedByRules() && !thisColumn.MultiLine) {
                    dd.Add("~" + thisColumn.KeyName.ToLowerInvariant() + "~");
                    or.Add("~" + thisColumn.KeyName.ToLowerInvariant() + "~|[Spalte: " + thisColumn.ReadableText() + "]");
                }
            }

            b.DropDownItems = dd.AsReadOnly();
            b.OpticalReplace = or.AsReadOnly();

            db.RepairAfterParse();
            var car = db.ColumnArrangements.CloneWithClones();

            car[1].Add(sp, false);
            car[1].Add(b, false);

            //car[1].ShowAllColumns();
            //car[1].Hide("visible");
            //car[1].HideSystemColumns();

            db.ColumnArrangements = car.AsReadOnly();

            db.SortDefinition = new RowSortDefinition(db, "Spalte", false);
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
        if (IsDisposed || tblFilterliste.Database is not Database db || db.IsDisposed) { return; }
        if (_column?.Database is not Database db2 || db2.IsDisposed) { return; }

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
        if (IsDisposed || tblFilterliste.Database is not Database db || db.IsDisposed) { return; }
        if (_column == null || _column.IsDisposed) { return; }

        foreach (var thisr in db.Row) {
            thisr.CellSet("Such", string.Empty, string.Empty);
        }

        if (db.Column["SpalteName"] is not ColumnItem c) { return; }

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
        if (IsDisposed || _column?.Database is not Database db || db.IsDisposed) { return; }

        if (tabControl.SelectedTab == tabSpaltenVerlinkung && cbxLinkedDatabase.ItemCount == 0) {
            var l = Database.AllAvailableTables(db.FreezedReason);

            foreach (var thisString in l) {
                if (!string.Equals(thisString.UniqueId, db.ConnectionData.UniqueId, StringComparison.OrdinalIgnoreCase)) { cbxLinkedDatabase.ItemAdd(ItemOf(thisString.TableName, thisString.UniqueId)); }
            }
        }
    }

    #endregion
}