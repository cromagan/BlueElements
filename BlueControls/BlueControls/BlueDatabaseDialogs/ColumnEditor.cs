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

#nullable enable

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Forms;
using BlueDatabase;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BlueControls.ItemCollection.ItemCollectionList;
using static BlueBasics.Converter;
using System.Drawing;

namespace BlueControls.BlueDatabaseDialogs {

    internal sealed partial class ColumnEditor {

        #region Fields

        private readonly Table? _table;
        private ColumnItem? _column;

        #endregion

        #region Constructors

        public ColumnEditor(ColumnItem? column, Table? table) : base() {
            // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
            InitializeComponent();
            _table = table;
            Column_DatenAuslesen(column);

            if (_column != null) {
                _column.Database.ShouldICancelSaveOperations += Database_ShouldICancelDiscOperations;
            }
        }

        #endregion

        #region Methods

        protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e) {
            base.OnFormClosing(e);
            if (!AllOk()) {
                e.Cancel = true;
            } else {
                _column.Database.ShouldICancelSaveOperations -= Database_ShouldICancelDiscOperations;
            }
        }

        private static int ColumKeyFrom(Database? database, string columnKey) {
            if (database == null || string.IsNullOrEmpty(columnKey)) { return -1; }

            IntTryParse(columnKey, out var c);
            return c;
        }

        private static void Database_ShouldICancelDiscOperations(object sender, System.ComponentModel.CancelEventArgs e) => e.Cancel = true;

        private static void SetKeyTo(TextBox combobox, long columnKey) => combobox.Text = columnKey.ToString();

        private bool AllOk() {
            var feh = "";
            // Diese Fehler sind so schwer und darf auf keinen Fall in die Umwelt gelassen werden
            if (string.IsNullOrEmpty(feh)) {
                if (string.IsNullOrEmpty(tbxName.Text)) { feh = "Spaltenname nicht definiert."; }
            }
            // Diese Fehler sind so schwer und darf auf keinen Fall in die Umwelt gelassen werden
            if (string.IsNullOrEmpty(feh)) {
                foreach (var _ in _column.Database.Column.Where(thisColumn => thisColumn != _column && thisColumn != null && string.Equals(tbxName.Text, thisColumn.Name, StringComparison.CurrentCultureIgnoreCase))) {
                    feh = "Spalten-Name bereits vorhanden.";
                }
            }
            if (string.IsNullOrEmpty(feh)) {
                Column_DatenZurückschreiben();
                if (string.IsNullOrEmpty(feh)) { feh = _column.ErrorReason(); }
            }
            if (!string.IsNullOrEmpty(feh)) {
                MessageBox.Show("<b><u>Bitte korrigieren sie zuerst folgenden Fehler:</u></b><br>" + feh, ImageCode.Warnung, "Ok");
                return false;
            }

            _column.Repair();
            return true;
        }

        private void btnBackColor_Click(object sender, System.EventArgs e) {
            ColorDia.Color = QuickImage.Get(btnBackColor.ImageCode).ChangeGreenTo.FromHtmlCode();
            ColorDia.ShowDialog();
            btnBackColor.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, ColorDia.Color).ToString();
        }

        private void btnOk_Click(object sender, System.EventArgs e) {
            if (!AllOk()) { return; }
            Close();
        }

        private void btnQI_Vorschau_Click(object sender, System.EventArgs e) => Notification.Show(tbxQuickinfo.Text.Replace("\r", "<BR>") + "<br><br><br>" + tbxAdminInfo.Text.Replace("\r", "<BR>"));

        private void btnSchnellAuswahloptionen_Click(object sender, System.EventArgs e) {
            Column_DatenZurückschreiben();
            _column.SetFormatForTextOptions();
            Column_DatenAuslesen(_column);
        }

        private void btnSchnellBildCode_Click(object sender, System.EventArgs e) {
            Column_DatenZurückschreiben();
            _column.SetFormatForBildCode();
            Column_DatenAuslesen(_column);
        }

        private void btnSchnellBit_Click(object sender, System.EventArgs e) {
            Column_DatenZurückschreiben();
            _column.SetFormatForBit();
            Column_DatenAuslesen(_column);
        }

        private void btnSchnellDatum_Click(object sender, System.EventArgs e) {
            Column_DatenZurückschreiben();
            _column.SetFormatForDate();
            Column_DatenAuslesen(_column);
        }

        private void btnSchnellDatumUhrzeit_Click(object sender, System.EventArgs e) {
            Column_DatenZurückschreiben();
            _column.SetFormatForDateTime();
            Column_DatenAuslesen(_column);
        }

        private void btnSchnellEmail_Click(object sender, System.EventArgs e) {
            Column_DatenZurückschreiben();
            _column.SetFormatForEmail();
            Column_DatenAuslesen(_column);
        }

        private void btnSchnellGanzzahl_Click(object sender, System.EventArgs e) {
            Column_DatenZurückschreiben();
            _column.SetFormatForInteger();
            Column_DatenAuslesen(_column);
        }

        private void btnSchnellGleitkommazahl_Click(object sender, System.EventArgs e) {
            Column_DatenZurückschreiben();
            _column.SetFormatForFloat();
            Column_DatenAuslesen(_column);
        }

        private void btnSchnellIInternetAdresse_Click(object sender, System.EventArgs e) {
            Column_DatenZurückschreiben();
            _column.SetFormatForUrl();
            Column_DatenAuslesen(_column);
        }

        private void btnSchnellTelefonNummer_Click(object sender, System.EventArgs e) {
            Column_DatenZurückschreiben();
            _column.SetFormatForPhoneNumber();
            Column_DatenAuslesen(_column);
        }

        private void btnSchnellText_Click(object sender, System.EventArgs e) {
            Column_DatenZurückschreiben();
            _column.SetFormatForText();
            Column_DatenAuslesen(_column);
        }

        private void btnStandard_Click(object sender, System.EventArgs e) {
            if (!AllOk()) { return; }
            _column.ResetSystemToDefault(true);
            Column_DatenAuslesen(_column);
        }

        private void btnTextColor_Click(object sender, System.EventArgs e) {
            ColorDia.Color = QuickImage.Get(btnTextColor.ImageCode).ChangeGreenTo.FromHtmlCode();
            ColorDia.ShowDialog();
            btnTextColor.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, ColorDia.Color).ToString();
        }

        private void btnVerwendung_Click(object sender, System.EventArgs e) => MessageBox.Show(_column.Verwendung());

        private void btnVor_Click(object sender, System.EventArgs e) {
            if (!AllOk()) { return; }
            Column_DatenAuslesen(_column.Next());
        }

        private void btnZurueck_Click(object sender, System.EventArgs e) {
            if (!AllOk()) { return; }
            Column_DatenAuslesen(_column.Previous());
        }

        private void butAktuellVor_Click(object sender, System.EventArgs e) {
            if (!AllOk()) { return; }
            Column_DatenAuslesen(_table.CurrentArrangement[_column].NextVisible().Column);
        }

        private void butAktuellZurueck_Click(object sender, System.EventArgs e) {
            if (!AllOk()) { return; }
            Column_DatenAuslesen(_table.CurrentArrangement[_column].PreviewsVisible().Column);
        }

        private void ButtonCheck() {
            var tmpFormat = (DataFormat)IntParse(cbxFormat.Text);
            // Mehrzeilig
            btnMultiline.Enabled = tmpFormat.MultilinePossible();
            if (!tmpFormat.MultilinePossible()) { btnMultiline.Checked = false; }
            // Rechtschreibprüfung
            btnSpellChecking.Enabled = tmpFormat.SpellCheckingPossible();
            if (!tmpFormat.SpellCheckingPossible()) { btnSpellChecking.Checked = false; }
            //// Format: Bildcode
            //grpBildCode.Enabled = tmpFormat == enDataFormat.BildCode;
            //if (tmpFormat != enDataFormat.BildCode) {
            //    txbBildCodeConstHeight.Text = string.Empty;
            //    cbxBildTextVerhalten.Text = string.Empty;
            //}
            // Format: LinkToFileSystem
            grpLinkToFileSystem.Enabled = tmpFormat == DataFormat.Link_To_Filesystem;
            //if (tmpFormat != enDataFormat.BildCode) {
            //    txbBestFileStandardFolder.Text = string.Empty;
            //    txbBestFileStandardSuffix.Text = string.Empty;
            //}
            // LinkedDatabase - Verknüpfte Datenbank
            //grpLinkedDatabase.Enabled = tmpFormat.NeedTargetDatabase();
            //if (!tmpFormat.NeedTargetDatabase()) {
            //    cbxLinkedDatabase.Text = string.Empty;
            //}

            //// Format: LinkedCell
            //grpVerlinkteZellen.Enabled = tmpFormat is enDataFormat.Verknüpfung_zu_anderer_Datenbank;
        }

        private void cbxFormat_TextChanged(object sender, System.EventArgs e) => ButtonCheck();

        /// <summary>
        /// Kümmert sich um erlaubte Spalten für LinkedCell
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxLinkedDatabase_TextChanged(object? sender, System.EventArgs e) {
            _column.LinkedDatabaseFile = cbxLinkedDatabase.Text;

            cbxTargetColumn.Item.Clear();

            if (_column.LinkedDatabase != null) {
                //foreach (var ThisColumn in _Column.Database.Column) {
                //    if (ThisColumn.Format.CanBeCheckedByRules() && !ThisColumn.MultiLine && !ThisColumn.Format.NeedTargetDatabase()) {
                //        cbxRowKeyInColumn.Item.Add(ThisColumn);
                //    }
                //    //if (ThisColumn.Format == enDataFormat.Values_für_LinkedCellDropdownx && ThisColumn.LinkedDatabase() == _Column.LinkedDatabase()) {
                //    //    cbxRowKeyInColumn.Item.Add(ThisColumn);
                //    //}
                //}
                foreach (var thisLinkedColumn in _column.LinkedDatabase.Column) {
                    if (!thisLinkedColumn.IsFirst() && thisLinkedColumn.Format.CanBeChangedByRules() && !thisLinkedColumn.Format.NeedTargetDatabase()) {
                        cbxTargetColumn.Item.Add(thisLinkedColumn);
                    }
                }
            }
            cbxTargetColumn.Item.Sort();
            SetKeyTo(cbxTargetColumn, _column.LinkedCell_ColumnKeyOfLinkedDatabase);
            cbxTargetColumn.Enabled = cbxTargetColumn.Item.Count > 0;
            capTargetColumn.Enabled = cbxTargetColumn.Enabled;
            if (!cbxTargetColumn.Enabled) {
                cbxTargetColumn.Text = string.Empty;
            }

            GeneratFilterListe();
        }

        private void cbxTargetColumn_TextChanged(object sender, System.EventArgs e) => GeneratFilterListe();

        private void Column_DatenAuslesen(ColumnItem? fromColumn) {
            _column = fromColumn;
            cbxFormat.Item.AddRange(typeof(DataFormat));
            cbxRandLinks.Item.AddRange(typeof(ColumnLineStyle));
            cbxRandRechts.Item.AddRange(typeof(ColumnLineStyle));
            cbxBildTextVerhalten.Item.AddRange(typeof(BildTextVerhalten));
            cbxAlign.Item.AddRange(typeof(AlignmentHorizontal));
            cbxAdditionalCheck.Item.AddRange(typeof(AdditionalCheck));
            cbxScriptType.Item.AddRange(typeof(ScriptType));
            cbxTranslate.Item.AddRange(typeof(TranslationType));
            cbxSort.Item.AddRange(typeof(SortierTyp));
            cbxLinkedDatabase.Item.Clear();
            if (!string.IsNullOrEmpty(_column.Database.Filename)) {
                var all = Directory.GetFiles(_column.Database.Filename.FilePath(), "*.mdb", SearchOption.TopDirectoryOnly);
                foreach (var thisString in all) {
                    if (!string.Equals(thisString, _column.Database.Filename, StringComparison.CurrentCultureIgnoreCase)) { cbxLinkedDatabase.Item.Add(thisString.FileNameWithSuffix()); }
                }
            }
            cbxLinkedDatabase.Item.Sort();
            if (cbxEinheit.Item.Count < 1) {
                cbxEinheit.Item.Add("µm", ImageCode.Lineal);
                cbxEinheit.Item.Add("mm", ImageCode.Lineal);
                cbxEinheit.Item.Add("cm", ImageCode.Lineal);
                cbxEinheit.Item.Add("dm", ImageCode.Lineal);
                cbxEinheit.Item.Add("m", ImageCode.Lineal);
                cbxEinheit.Item.Add("km", ImageCode.Lineal);
                cbxEinheit.Item.Add("mm²", ImageCode.GrößeÄndern);
                cbxEinheit.Item.Add("m²", ImageCode.GrößeÄndern);
                cbxEinheit.Item.Add("µg", ImageCode.Gewicht);
                cbxEinheit.Item.Add("mg", ImageCode.Gewicht);
                cbxEinheit.Item.Add("g", ImageCode.Gewicht);
                cbxEinheit.Item.Add("kg", ImageCode.Gewicht);
                cbxEinheit.Item.Add("t", ImageCode.Gewicht);
                cbxEinheit.Item.Add("h", ImageCode.Uhr);
                cbxEinheit.Item.Add("min", ImageCode.Uhr);
                cbxEinheit.Item.Add("St.", ImageCode.Eins);
            }
            lbxCellEditor.Suggestions.Clear();
            lbxCellEditor.Suggestions.AddRange(_column.Database.Permission_AllUsed(true));
            btnZurueck.Enabled = _column.Previous() != null;
            btnVor.Enabled = _column.Next() != null;
            if (_table?.CurrentArrangement != null) {
                butAktuellZurueck.Enabled = _table.CurrentArrangement[_column]?.PreviewsVisible() != null;
                butAktuellVor.Enabled = _table.CurrentArrangement[_column]?.NextVisible() != null;
            } else {
                butAktuellVor.Enabled = false;
                butAktuellZurueck.Enabled = false;
            }
            if (string.IsNullOrEmpty(_column.Identifier)) {
                btnStandard.Enabled = false;
                capInfo.Text = "<Imagecode=" + _column.SymbolForReadableText() + "> Normale Spalte (Key: " + _column.Key + ")";
            } else {
                btnStandard.Enabled = true;
                capInfo.Text = "<Imagecode=" + _column.SymbolForReadableText() + "> " + _column.Identifier + " (Key: " + _column.Key + ")";
            }
            tbxName.Text = _column.Name;
            tbxName.AllowedChars = Constants.AllowedCharsVariableName;
            tbxCaption.Text = _column.Caption;
            btnBackColor.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, _column.BackColor).ToString();
            btnTextColor.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, _column.ForeColor).ToString();
            btnMultiline.Checked = _column.MultiLine;
            cbxFormat.Text = ((int)_column.Format).ToString();
            cbxRandLinks.Text = ((int)_column.LineLeft).ToString();
            cbxRandRechts.Text = ((int)_column.LineRight).ToString();
            cbxAlign.Text = ((int)_column.Align).ToString();
            cbxAdditionalCheck.Text = ((int)_column.AdditionalCheck).ToString();
            cbxScriptType.Text = ((int)_column.ScriptType).ToString();
            cbxTranslate.Text = ((int)_column.Translate).ToString();
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
            btnAutoEditAutoSort.Checked = _column.AfterEdit_QuickSortRemoveDouble;
            tbxRunden.Text = _column.AfterEdit_Runden is > -1 and < 7 ? _column.AfterEdit_Runden.ToString() : string.Empty;
            btnAutoEditToUpper.Checked = _column.AfterEdit_DoUCase;
            btnAutoEditKleineFehler.Checked = _column.AfterEdit_AutoCorrect;
            tbxInitValue.Text = _column.CellInitValue;
            tbxJoker.Text = _column.AutoFilterJoker;
            txbUeberschift1.Text = _column.Ueberschrift1;
            txbUeberschift2.Text = _column.Ueberschrift2;
            txbUeberschift3.Text = _column.Ueberschrift3;
            txbPrefix.Text = _column.Prefix;
            txbSpaltenbild.Text = _column.CaptionBitmap;
            btnLogUndo.Checked = _column.ShowUndo;
            btnFormatierungErlaubt.Checked = _column.FormatierungErlaubt;
            btnSpellChecking.Checked = _column.SpellChecking;
            btnEinzeiligDarstellen.Checked = _column.ShowMultiLineInOneLine;
            tbxAuswaehlbareWerte.Text = _column.DropDownItems.JoinWithCr();
            txbReplacer.Text = _column.OpticalReplace.JoinWithCr();
            txbAutoReplace.Text = _column.AfterEditAutoReplace.JoinWithCr();
            txbRegex.Text = _column.Regex;
            tbxTags.Text = _column.Tags.JoinWithCr();
            lbxCellEditor.Item.Clear();
            lbxCellEditor.Item.AddRange(_column.PermissionGroupsChangeCell.ToArray());
            tbxAllowedChars.Text = _column.AllowedChars;
            btnOtherValuesToo.Checked = _column.DropdownWerteAndererZellenAnzeigen;
            btnIgnoreLock.Checked = _column.EditTrotzSperreErlaubt;
            tbxAdminInfo.Text = _column.AdminInfo.Replace("<br>", "\r", RegexOptions.IgnoreCase);
            tbxQuickinfo.Text = _column.Quickinfo.Replace("<br>", "\r", RegexOptions.IgnoreCase);
            cbxEinheit.Text = _column.Suffix;
            txbBildCodeConstHeight.Text = _column.BildCode_ConstantHeight;
            cbxBildTextVerhalten.Text = ((int)_column.BildTextVerhalten).ToString();
            txbBestFileStandardFolder.Text = _column.BestFile_StandardFolder;
            txbBestFileStandardSuffix.Text = _column.BestFile_StandardSuffix;
            cbxLinkedDatabase.Text = _column.LinkedDatabaseFile;
            //txbLinkedKeyKennung.Text = _column.LinkedKeyKennung;

            txbAutoRemove.Text = _column.AutoRemove;
            butSaveContent.Checked = _column.SaveContent;

            cbxSchlüsselspalte.Item.Clear();
            cbxSchlüsselspalte.Item.Add("#Ohne", "-1");
            cbxDropDownKey.Item.Clear();
            cbxDropDownKey.Item.Add("#Ohne", "-1");
            cbxVorschlagSpalte.Item.Clear();
            cbxVorschlagSpalte.Item.Add("#Ohne", "-1");
            // Einige Dropdown-Menüs sind abhängig von der LinkedDatabase und werden in dessen TextChanged-Event befüllt
            // siehe Ende dieser Routine
            foreach (var thisColumn in _column.Database.Column) {
                if ((thisColumn.Format == DataFormat.RelationText || !thisColumn.MultiLine) && thisColumn.Format.CanBeCheckedByRules()) { cbxSchlüsselspalte.Item.Add(thisColumn); }
                if (thisColumn.Format.CanBeCheckedByRules() && !thisColumn.MultiLine && !thisColumn.Format.NeedTargetDatabase()) {
                    cbxDropDownKey.Item.Add(thisColumn);
                    cbxVorschlagSpalte.Item.Add(thisColumn);
                }
            }
            cbxSchlüsselspalte.Item.Sort();
            cbxVorschlagSpalte.Item.Sort();
            cbxDropDownKey.Item.Sort();

            SetKeyTo(cbxSchlüsselspalte, _column.KeyColumnKey);
            SetKeyTo(cbxDropDownKey, _column.DropdownKey);
            SetKeyTo(cbxVorschlagSpalte, _column.VorschlagsColumn);
            cbxLinkedDatabase_TextChanged(null, System.EventArgs.Empty);
        }

        private void Column_DatenZurückschreiben() {
            if (_column.Database.ReadOnly) { return; }
            _column.Name = tbxName.Text;
            _column.Caption = tbxCaption.Text.Replace("\r\n", "\r").Trim().Trim("\r").Trim();
            _column.Format = (DataFormat)IntParse(cbxFormat.Text);
            _column.Quickinfo = tbxQuickinfo.Text.Replace("\r", "<BR>");
            _column.AdminInfo = tbxAdminInfo.Text.Replace("\r", "<BR>");
            _column.Suffix = cbxEinheit.Text;
            _column.BackColor = QuickImage.Get(btnBackColor.ImageCode).ChangeGreenTo.FromHtmlCode();
            _column.ForeColor = QuickImage.Get(btnTextColor.ImageCode).ChangeGreenTo.FromHtmlCode();
            _column.LineLeft = (ColumnLineStyle)IntParse(cbxRandLinks.Text);
            _column.LineRight = (ColumnLineStyle)IntParse(cbxRandRechts.Text);
            _column.MultiLine = btnMultiline.Checked;
            _column.AfterEdit_QuickSortRemoveDouble = btnAutoEditAutoSort.Checked;
            if (tbxRunden.Text.IsLong()) {
                var zahl = IntParse(tbxRunden.Text);
                if (zahl is > -1 and < 7) { _column.AfterEdit_Runden = zahl; }
            } else {
                _column.AfterEdit_Runden = -1;
            }
            _column.AfterEdit_DoUCase = btnAutoEditToUpper.Checked;
            _column.AfterEdit_AutoCorrect = btnAutoEditKleineFehler.Checked;
            _column.CellInitValue = tbxInitValue.Text;
            _column.ShowMultiLineInOneLine = btnEinzeiligDarstellen.Checked;
            _column.ShowUndo = btnLogUndo.Checked;
            _column.FormatierungErlaubt = btnFormatierungErlaubt.Checked;
            _column.SpellChecking = btnSpellChecking.Checked;
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
            if (lbxCellEditor.Item.ToListOfString().IsDifferentTo(_column.PermissionGroupsChangeCell)) {
                _column.PermissionGroupsChangeCell.Clear();
                _column.PermissionGroupsChangeCell.AddRange(lbxCellEditor.Item.ToListOfString());
            }
            var newDd = tbxAuswaehlbareWerte.Text.SplitAndCutByCrToList().SortedDistinctList();
            if (newDd.IsDifferentTo(_column.DropDownItems)) {
                _column.DropDownItems.Clear();
                _column.DropDownItems.AddRange(newDd);
            }
            var newRep = txbReplacer.Text.SplitAndCutByCrToList();
            if (newRep.IsDifferentTo(_column.OpticalReplace)) {
                _column.OpticalReplace.Clear();
                _column.OpticalReplace.AddRange(newRep);
            }
            var newRep2 = txbAutoReplace.Text.SplitAndCutByCrToList();
            if (newRep2.IsDifferentTo(_column.AfterEditAutoReplace)) {
                _column.AfterEditAutoReplace.Clear();
                _column.AfterEditAutoReplace.AddRange(newRep2);
            }
            _column.AutoFilterJoker = tbxJoker.Text;
            _column.Ueberschrift1 = txbUeberschift1.Text;
            _column.Ueberschrift2 = txbUeberschift2.Text;
            _column.Ueberschrift3 = txbUeberschift3.Text;
            _column.Prefix = txbPrefix.Text;
            _column.CaptionBitmap = txbSpaltenbild.Text;
            var newTags = tbxTags.Text.SplitAndCutByCrToList();
            if (newTags.IsDifferentTo(_column.Tags)) {
                _column.Tags.Clear();
                _column.Tags.AddRange(newTags);
            }
            _column.Regex = txbRegex.Text;
            _column.TextBearbeitungErlaubt = btnEditableStandard.Checked;
            _column.DropdownBearbeitungErlaubt = btnEditableDropdown.Checked;
            _column.DropdownAllesAbwählenErlaubt = btnCanBeEmpty.Checked;
            _column.DropdownWerteAndererZellenAnzeigen = btnOtherValuesToo.Checked;
            _column.EditTrotzSperreErlaubt = btnIgnoreLock.Checked;
            _column.AllowedChars = tbxAllowedChars.Text;

            _column.BildCode_ConstantHeight = txbBildCodeConstHeight.Text;
            IntTryParse(cbxBildTextVerhalten.Text, out var imNf);
            _column.BildTextVerhalten = (BildTextVerhalten)imNf;
            _column.BestFile_StandardFolder = txbBestFileStandardFolder.Text;
            _column.BestFile_StandardSuffix = txbBestFileStandardSuffix.Text;
            _column.LinkedDatabaseFile = cbxLinkedDatabase.Text; // Muss vor LinkedCell_RowKey zurückgeschrieben werden
            _column.KeyColumnKey = ColumKeyFrom(_column.Database, cbxSchlüsselspalte.Text);
            _column.LinkedCell_ColumnKeyOfLinkedDatabase = ColumKeyFrom(_column.LinkedDatabase, cbxTargetColumn.Text); // LINKED DATABASE
            _column.DropdownKey = ColumKeyFrom(_column.Database, cbxDropDownKey.Text);
            _column.VorschlagsColumn = ColumKeyFrom(_column.Database, cbxVorschlagSpalte.Text);
            _column.Align = (AlignmentHorizontal)IntParse(cbxAlign.Text);
            _column.AdditionalCheck = (AdditionalCheck)IntParse(cbxAdditionalCheck.Text);
            _column.ScriptType = (ScriptType)IntParse(cbxScriptType.Text);
            _column.Translate = (TranslationType)IntParse(cbxTranslate.Text);
            _column.SortType = (SortierTyp)IntParse(cbxSort.Text);
            _column.AutoRemove = txbAutoRemove.Text;
            _column.SaveContent = butSaveContent.Checked;
            GetLinkedCellFilter();

            _column.Repair();
            //_Column.Database.Rules.Sort();
        }

        private void GeneratFilterListe() {
            GetLinkedCellFilter();

            _column.LinkedDatabaseFile = cbxLinkedDatabase.Text;

            var linkdb = _column.LinkedDatabase;
            if (linkdb == null) { tblFilterliste.Database = null; }
            if (tblFilterliste.Database == null) { tblFilterliste.Database = null; }

            if (tblFilterliste.Database != null && tblFilterliste.Database.Tags.TagGet("Filename") != linkdb.Filename) { tblFilterliste.Database = null; }

            if (linkdb == null) { return; }

            if (tblFilterliste.Database == null) {
                Database x = new(false);
                x.Column.Add("count", "count", VarType.Integer);
                var vis = x.Column.Add("visible", "visible", VarType.Bit);
                var sp = x.Column.Add("Spalte", "Spalte", VarType.Text);
                sp.Align = AlignmentHorizontal.Rechts;
                var b = x.Column.Add("Such", "Suchtext", VarType.Text);
                b.Quickinfo = "<b>Entweder</b> ~Spaltenname~<br><b>oder</b> fester Text zum suchen<br>Mischen wird nicht unterstützt.";
                b.MultiLine = false;
                b.TextBearbeitungErlaubt = true;
                b.DropdownAllesAbwählenErlaubt = true;
                b.DropdownBearbeitungErlaubt = true;

                foreach (var thisColumn in _column.Database.Column.Where(thisColumn => thisColumn.Format.CanBeCheckedByRules() && !thisColumn.MultiLine)) {
                    b.DropDownItems.Add("~" + thisColumn.Name.ToLower() + "~");
                    b.OpticalReplace.Add("~" + thisColumn.Name.ToLower() + "~|[Spalte: " + thisColumn.ReadableText() + "]");
                }

                x.RepairAfterParse();
                x.ColumnArrangements[1].ShowAllColumns();
                x.ColumnArrangements[1].Hide("visible");
                x.ColumnArrangements[1].HideSystemColumns();
                x.SortDefinition = new RowSortDefinition(x, "Count", false);
                tblFilterliste.Database = x;
                tblFilterliste.Arrangement = 1;

                x.Tags.TagSet("Filename", linkdb.Filename);

                tblFilterliste.Filter.Add(vis, FilterType.Istgleich, "+");
            }

            linkdb.RepairAfterParse(); // Dass ja die 0 Ansicht stimmt

            var ok = IntTryParse(cbxTargetColumn.Text, out var key);
            ColumnItem? spalteauDb = null;
            if (ok) { spalteauDb = linkdb.Column.SearchByKey(key); }

            for (var z = 0; z < linkdb.Column.Count; z++) {
                var col = linkdb.Column[z];

                var r = tblFilterliste.Database.Row[z.ToString()];
                if (r == null) {
                    r = tblFilterliste.Database.Row.Add(z.ToString());
                }

                r.CellSet("Spalte", col.ReadableText() + " = ");

                if (col.Format.Autofilter_möglich() && !col.MultiLine && col != spalteauDb && col != spalteauDb && !col.Format.NeedTargetDatabase() && string.IsNullOrEmpty(col.Identifier)) {
                    r.CellSet("visible", true);
                } else {
                    r.CellSet("visible", false);
                }

                SetLinkedCellFilter();
            }
        }

        /// <summary>
        /// Holt die Werte aus tblFilterliste und schreibt sie in _Column.LinkedCellFilter
        /// Hat tblFilterliste keine Datenbank, bleibt die Variable _Column.LinkedCellFilter unverändert
        /// </summary>
        private void GetLinkedCellFilter() {
            if (tblFilterliste.Database == null) { return; }

            var nf = new List<string?>();
            foreach (var thisr in tblFilterliste.Database.Row) {
                var x = thisr.CellGetInteger("Count");

                while (nf.Count <= x) { nf.Add(string.Empty); }
                if (thisr.CellGetBoolean("visible")) {
                    var tmp = "@" + thisr.CellGetString("Such");

                    foreach (var thisColumn in _column.Database.Column.Where(thisColumn => "@~" + thisColumn.Name.ToLower() + "~" == tmp)) {
                        tmp = thisColumn.Key.ToString();
                    }
                    if (tmp == "@") { tmp = string.Empty; }

                    nf[x] = tmp;
                }
            }

            nf = nf.JoinWithCr().SplitAndCutByCrToList();

            if (_column.LinkedCellFilter.IsDifferentTo(nf)) {
                _column.LinkedCellFilter.Clear();
                _column.LinkedCellFilter.AddRange(nf);
            }
        }

        /// <summary>
        /// Holt die Werte aus _Column.LinkedCellFilter und schreibt sie in tblFilterliste
        ///Leer evtl. Werte aus tblFilterliste
        /// </summary>
        private void SetLinkedCellFilter() {
            foreach (var thisr in tblFilterliste.Database.Row) {
                var x = thisr.CellGetInteger("Count");

                if (x < _column.LinkedCellFilter.Count) {
                    var tmp = _column.LinkedCellFilter[x];

                    if (IntTryParse(tmp, out var key)) {
                        var col = _column.Database.Column.SearchByKey(key);
                        tmp = col != null ? "~" + col.Name.ToLower() + "~" : string.Empty;
                    } else {
                        tmp = tmp.StartsWith("@") ? tmp.Substring(1) : string.Empty;
                    }
                    thisr.CellSet("Such", tmp);
                } else {
                    thisr.CellSet("Such", string.Empty);
                }
            }
        }

        #endregion

        //if (database is null || columnKey < 0) {//    combobox.Text = "#Ohne";//} else {//    var c = database.Column.SearchByKey(columnKey);//    combobox.Text = c == null ? "#Ohne" : c.Name;//}
    }
}