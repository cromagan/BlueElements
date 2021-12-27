// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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
using BlueControls.Controls;
using BlueControls.Forms;
using BlueControls.ItemCollection;
using BlueDatabase;
using BlueDatabase.Enums;
using System.IO;
using System.Text.RegularExpressions;
using static BlueBasics.Converter;
using static BlueBasics.Extensions;

namespace BlueControls.BlueDatabaseDialogs {

    internal sealed partial class ColumnEditor {

        #region Fields

        private readonly Table _Table;
        private ColumnItem _Column;

        #endregion

        #region Constructors

        public ColumnEditor(ColumnItem column, Table table) : base() {
            // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
            InitializeComponent();
            _Table = table;
            Column_DatenAuslesen(column);

            _Column.Database.ShouldICancelSaveOperations += Database_ShouldICancelDiscOperations;
        }

        #endregion

        #region Methods

        protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e) {
            base.OnFormClosing(e);
            if (!AllOk()) {
                e.Cancel = true;
            } else {
                _Column.Database.ShouldICancelSaveOperations -= Database_ShouldICancelDiscOperations;
            }
        }

        private bool AllOk() {
            var Feh = "";
            // Diese Fehler sind so schwer und darf auf keinen Fall in die Umwelt gelassen werden
            if (string.IsNullOrEmpty(Feh)) {
                if (string.IsNullOrEmpty(tbxName.Text)) { Feh = "Spaltenname nicht definiert."; }
            }
            // Diese Fehler sind so schwer und darf auf keinen Fall in die Umwelt gelassen werden
            if (string.IsNullOrEmpty(Feh)) {
                foreach (var ThisColumn in _Column.Database.Column) {
                    if (ThisColumn != _Column && ThisColumn != null) {
                        if (tbxName.Text.ToUpper() == ThisColumn.Name.ToUpper()) { Feh = "Spalten-Name bereits vorhanden."; }
                    }
                }
            }
            if (string.IsNullOrEmpty(Feh)) {
                Column_DatenZurückschreibenx();
                if (string.IsNullOrEmpty(Feh)) { Feh = _Column.ErrorReason(); }
            }
            if (!string.IsNullOrEmpty(Feh)) {
                MessageBox.Show("<b><u>Bitte korrigieren sie zuerst folgenden Fehler:</u></b><br>" + Feh, enImageCode.Warnung, "Ok");
                return false;
            }
            return true;
        }

        private void btnBackColor_Click(object sender, System.EventArgs e) {
            ColorDia.Color = QuickImage.Get(btnBackColor.ImageCode).ChangeGreenTo.FromHTMLCode();
            ColorDia.ShowDialog();
            btnBackColor.ImageCode = QuickImage.Get(enImageCode.Kreis, 16, "", ColorDia.Color.ToHTMLCode()).ToString();
        }

        private void btnOk_Click(object sender, System.EventArgs e) {
            if (!AllOk()) { return; }
            Close();
        }

        private void btnQI_Vorschau_Click(object sender, System.EventArgs e) => Notification.Show(tbxQuickinfo.Text.Replace("\r", "<BR>") + "<br><br><br>" + tbxAdminInfo.Text.Replace("\r", "<BR>"));

        private void btnRegexEmail_Click(object sender, System.EventArgs e) {
            //http://emailregex.com/
            txbRegex.Text = "^[a-z0-9A-Z._-]{1,40}[@][a-z0-9A-Z._-]{1,40}[.][a-zA-Z]{1,3}$";
        }

        private void btnRegexTelefonnummer_Click(object sender, System.EventArgs e) {
            //https://regex101.com/r/OzJr8j/1
            txbRegex.Text = @"^[+][1-9][\s0-9]*[0-9]$";
        }

        private void btnRegexUrl_Click(object sender, System.EventArgs e) {
            //    https://regex101.com/r/S2CbwM/1
            txbRegex.Text = @"^(https:|http:|www\.)\S*$";
        }

        private void btnStandard_Click(object sender, System.EventArgs e) {
            if (!AllOk()) { return; }
            _Column.ResetSystemToDefault(true);
            Column_DatenAuslesen(_Column);
        }

        private void btnTextColor_Click(object sender, System.EventArgs e) {
            ColorDia.Color = QuickImage.Get(btnTextColor.ImageCode).ChangeGreenTo.FromHTMLCode();
            ColorDia.ShowDialog();
            btnTextColor.ImageCode = QuickImage.Get(enImageCode.Kreis, 16, "", ColorDia.Color.ToHTMLCode()).ToString();
        }

        private void btnVerwendung_Click(object sender, System.EventArgs e) => MessageBox.Show(_Column.Verwendung());

        private void btnVor_Click(object sender, System.EventArgs e) {
            if (!AllOk()) { return; }
            Column_DatenAuslesen(_Column.Next());
        }

        private void btnZurueck_Click(object sender, System.EventArgs e) {
            if (!AllOk()) { return; }
            Column_DatenAuslesen(_Column.Previous());
        }

        private void butAktuellVor_Click(object sender, System.EventArgs e) {
            if (!AllOk()) { return; }
            Column_DatenAuslesen(_Table.CurrentArrangement[_Column].NextVisible(_Table.CurrentArrangement).Column);
        }

        private void butAktuellZurueck_Click(object sender, System.EventArgs e) {
            if (!AllOk()) { return; }
            Column_DatenAuslesen(_Table.CurrentArrangement[_Column].PreviewsVisible(_Table.CurrentArrangement).Column);
        }

        private void ButtonCheck() {
            var tmpFormat = (enDataFormat)int.Parse(cbxFormat.Text);
            // Mehrzeilig
            btnMultiline.Enabled = tmpFormat.MultilinePossible();
            if (!tmpFormat.MultilinePossible()) { btnMultiline.Checked = false; }
            // Rechtschreibprüfung
            btnSpellChecking.Enabled = tmpFormat.SpellCheckingPossible();
            if (!tmpFormat.SpellCheckingPossible()) { btnSpellChecking.Checked = false; }
            // Format: Bildcode
            grpBildCode.Enabled = tmpFormat == enDataFormat.BildCode;
            if (tmpFormat != enDataFormat.BildCode) {
                txbBildCodeConstHeight.Text = string.Empty;
                cbxBildTextVerhalten.Text = string.Empty;
            }
            // Format: LinkToFileSystem
            grpLinkToFileSystem.Enabled = tmpFormat == enDataFormat.Link_To_Filesystem;
            if (tmpFormat != enDataFormat.BildCode) {
                txbBestFileStandardFolder.Text = string.Empty;
                txbBestFileStandardSuffix.Text = string.Empty;
            }
            // LinkedDatabase - Verknüpfte Datenbank
            grpLinkedDatabase.Enabled = tmpFormat.NeedTargetDatabase();
            if (!tmpFormat.NeedTargetDatabase()) {
                cbxLinkedDatabase.Text = string.Empty;
            }
            // Format: Columns für Linked Database / LinkedKey-Kennung
            grpColumnsForLinkedDatabase.Enabled = tmpFormat.NeedLinkedKeyKennung();
            if (!tmpFormat.NeedLinkedKeyKennung()) { txbLinkedKeyKennung.Text = string.Empty; }
            // Format: LinkedCell
            grpVerlinkteZellen.Enabled = tmpFormat == enDataFormat.LinkedCell;
        }

        private void cbxFormat_TextChanged(object sender, System.EventArgs e) => ButtonCheck();

        /// <summary>
        /// Kümmert sich um erlaubte Spalten für LinkedCell
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxLinkedDatabase_TextChanged(object sender, System.EventArgs e) {
            _Column.LinkedDatabaseFile = cbxLinkedDatabase.Text;

            cbxRowKeyInColumn.Item.Clear();
            cbxRowKeyInColumn.Item.Add("#Ohne", "-1");
            cbxRowKeyInColumn.Item.Add("<Skript-gesteuert>", "-9999");

            cbxTargetColumn.Item.Clear();

            if (_Column.LinkedDatabase() != null) {
                foreach (var ThisColumn in _Column.Database.Column) {
                    if (ThisColumn.Format.CanBeCheckedByRules() && !ThisColumn.MultiLine && !ThisColumn.Format.NeedTargetDatabase()) {
                        cbxRowKeyInColumn.Item.Add(ThisColumn);
                    }
                    if (ThisColumn.Format == enDataFormat.Values_für_LinkedCellDropdown && ThisColumn.LinkedDatabase() == _Column.LinkedDatabase()) {
                        cbxRowKeyInColumn.Item.Add(ThisColumn);
                    }
                }
                foreach (var ThisLinkedColumn in _Column.LinkedDatabase().Column) {
                    if (!ThisLinkedColumn.IsFirst() && ThisLinkedColumn.Format.CanBeChangedByRules() && !ThisLinkedColumn.Format.NeedTargetDatabase()) {
                        cbxTargetColumn.Item.Add(ThisLinkedColumn);
                    }
                }
            }
            cbxTargetColumn.Item.Sort();
            cbxRowKeyInColumn.Item.Sort();
            cbxRowKeyInColumn.Enabled = cbxRowKeyInColumn.Item.Count > 0;
            SetKeyTo(cbxRowKeyInColumn, _Column.LinkedCell_RowKey);
            SetKeyTo(cbxTargetColumn, _Column.LinkedCell_ColumnKey);
            cbxTargetColumn.Enabled = cbxTargetColumn.Item.Count > 0;
            capTargetColumn.Enabled = cbxTargetColumn.Enabled;
            if (!cbxTargetColumn.Enabled) {
                cbxTargetColumn.Text = string.Empty;
            }
        }

        private int ColumKeyFrom(Database database, string columnKey) => database == null || string.IsNullOrEmpty(columnKey) ? -1 : IntParse(columnKey);

        private void Column_DatenAuslesen(ColumnItem FromColumn) {
            _Column = FromColumn;
            cbxFormat.Item.AddRange(typeof(enDataFormat));
            cbxRandLinks.Item.AddRange(typeof(enColumnLineStyle));
            cbxRandRechts.Item.AddRange(typeof(enColumnLineStyle));
            cbxBildTextVerhalten.Item.AddRange(typeof(enBildTextVerhalten));
            cbxAlign.Item.AddRange(typeof(enAlignmentHorizontal));
            cbxLinkedDatabase.Item.Clear();
            if (!string.IsNullOrEmpty(_Column.Database.Filename)) {
                var All = Directory.GetFiles(_Column.Database.Filename.FilePath(), "*.mdb", SearchOption.TopDirectoryOnly);
                foreach (var ThisString in All) {
                    if (ThisString.ToLower() != _Column.Database.Filename.ToLower()) { cbxLinkedDatabase.Item.Add(ThisString.FileNameWithSuffix()); }
                }
            }
            cbxLinkedDatabase.Item.Sort();
            if (cbxEinheit.Item.Count < 1) {
                cbxEinheit.Item.Add("µm", enImageCode.Lineal);
                cbxEinheit.Item.Add("mm", enImageCode.Lineal);
                cbxEinheit.Item.Add("cm", enImageCode.Lineal);
                cbxEinheit.Item.Add("dm", enImageCode.Lineal);
                cbxEinheit.Item.Add("m", enImageCode.Lineal);
                cbxEinheit.Item.Add("km", enImageCode.Lineal);
                cbxEinheit.Item.Add("mm²", enImageCode.GrößeÄndern);
                cbxEinheit.Item.Add("m²", enImageCode.GrößeÄndern);
                cbxEinheit.Item.Add("µg", enImageCode.Gewicht);
                cbxEinheit.Item.Add("mg", enImageCode.Gewicht);
                cbxEinheit.Item.Add("g", enImageCode.Gewicht);
                cbxEinheit.Item.Add("kg", enImageCode.Gewicht);
                cbxEinheit.Item.Add("t", enImageCode.Gewicht);
                cbxEinheit.Item.Add("h", enImageCode.Uhr);
                cbxEinheit.Item.Add("min", enImageCode.Uhr);
                cbxEinheit.Item.Add("St.", enImageCode.Eins);
            }
            lbxCellEditor.Suggestions.Clear();
            lbxCellEditor.Suggestions.AddRange(_Column.Database.Permission_AllUsed(false));
            btnZurueck.Enabled = _Column.Previous() != null;
            btnVor.Enabled = _Column.Next() != null;
            if (_Table != null && _Table.CurrentArrangement != null) {
                butAktuellZurueck.Enabled = _Table.CurrentArrangement[_Column]?.PreviewsVisible(_Table.CurrentArrangement) != null;
                butAktuellVor.Enabled = _Table.CurrentArrangement[_Column]?.NextVisible(_Table.CurrentArrangement) != null;
            } else {
                butAktuellVor.Enabled = false;
                butAktuellZurueck.Enabled = false;
            }
            if (string.IsNullOrEmpty(_Column.Identifier)) {
                btnStandard.Enabled = false;
                capInfo.Text = "<Imagecode=" + _Column.SymbolForReadableText() + "> Normale Spalte (Key: " + _Column.Key + ")";
            } else {
                btnStandard.Enabled = true;
                capInfo.Text = "<Imagecode=" + _Column.SymbolForReadableText() + "> " + _Column.Identifier + " (Key: " + _Column.Key + ")";
            }
            tbxName.Text = _Column.Name;
            tbxName.AllowedChars = Constants.AllowedCharsVariableName;
            tbxCaption.Text = _Column.Caption;
            btnBackColor.ImageCode = QuickImage.Get(enImageCode.Kreis, 16, "", _Column.BackColor.ToHTMLCode()).ToString();
            btnTextColor.ImageCode = QuickImage.Get(enImageCode.Kreis, 16, "", _Column.ForeColor.ToHTMLCode()).ToString();
            btnMultiline.Checked = _Column.MultiLine;
            cbxFormat.Text = ((int)_Column.Format).ToString();
            cbxRandLinks.Text = ((int)_Column.LineLeft).ToString();
            cbxRandRechts.Text = ((int)_Column.LineRight).ToString();
            cbxAlign.Text = ((int)_Column.Align).ToString();
            btnAutoFilterMoeglich.Checked = _Column.FilterOptions.HasFlag(enFilterOptions.Enabled);
            btnAutoFilterTXTErlaubt.Checked = _Column.FilterOptions.HasFlag(enFilterOptions.TextFilterEnabled);
            btnAutoFilterErweitertErlaubt.Checked = _Column.FilterOptions.HasFlag(enFilterOptions.ExtendedFilterEnabled);
            chkFilterOnlyOr.Checked = _Column.FilterOptions.HasFlag(enFilterOptions.OnlyOrAllowed);
            chkFilterOnlyAND.Checked = _Column.FilterOptions.HasFlag(enFilterOptions.OnlyAndAllowed);
            btnZeilenFilterIgnorieren.Checked = _Column.IgnoreAtRowFilter;
            btnEditableStandard.Checked = _Column.TextBearbeitungErlaubt;
            btnEditableDropdown.Checked = _Column.DropdownBearbeitungErlaubt;
            btnCanBeEmpty.Checked = _Column.DropdownAllesAbwählenErlaubt;
            btnAutoEditAutoSort.Checked = _Column.AfterEdit_QuickSortRemoveDouble;
            tbxRunden.Text = _Column.AfterEdit_Runden is > -1 and < 7 ? _Column.AfterEdit_Runden.ToString() : string.Empty;
            btnAutoEditToUpper.Checked = _Column.AfterEdit_DoUCase;
            btnAutoEditKleineFehler.Checked = _Column.AfterEdit_AutoCorrect;
            tbxInitValue.Text = _Column.CellInitValue;
            tbxJoker.Text = _Column.AutoFilterJoker;
            txbUeberschift1.Text = _Column.Ueberschrift1;
            txbUeberschift2.Text = _Column.Ueberschrift2;
            txbUeberschift3.Text = _Column.Ueberschrift3;
            txbDauerFilterPos.Text = _Column.DauerFilterPos.ToString();
            txbPrefix.Text = _Column.Prefix;
            txbSpaltenbild.Text = _Column.CaptionBitmap;
            btnLogUndo.Checked = _Column.ShowUndo;
            btnSpellChecking.Checked = _Column.SpellCheckingEnabled;
            btnEinzeiligDarstellen.Checked = _Column.ShowMultiLineInOneLine;
            tbxAuswaehlbareWerte.Text = _Column.DropDownItems.JoinWithCr();
            txbReplacer.Text = _Column.OpticalReplace.JoinWithCr();
            txbAutoReplace.Text = _Column.AfterEdit_AutoReplace.JoinWithCr();
            txbRegex.Text = _Column.Regex.JoinWithCr();
            tbxTags.Text = _Column.Tags.JoinWithCr();
            lbxCellEditor.Item.Clear();
            lbxCellEditor.Item.AddRange(_Column.PermissionGroups_ChangeCell.ToArray());
            tbxAllowedChars.Text = _Column.AllowedChars;
            btnOtherValuesToo.Checked = _Column.DropdownWerteAndererZellenAnzeigen;
            btnIgnoreLock.Checked = _Column.EditTrotzSperreErlaubt;
            tbxAdminInfo.Text = _Column.AdminInfo.Replace("<br>", "\r", RegexOptions.IgnoreCase);
            tbxQuickinfo.Text = _Column.Quickinfo.Replace("<br>", "\r", RegexOptions.IgnoreCase);
            cbxEinheit.Text = _Column.Suffix;
            txbBildCodeConstHeight.Text = _Column.BildCode_ConstantHeight.ToString();
            cbxBildTextVerhalten.Text = ((int)_Column.BildTextVerhalten).ToString();
            txbBestFileStandardFolder.Text = _Column.BestFile_StandardFolder;
            txbBestFileStandardSuffix.Text = _Column.BestFile_StandardSuffix;
            cbxLinkedDatabase.Text = _Column.LinkedDatabaseFile;
            txbLinkedKeyKennung.Text = _Column.LinkedKeyKennung;
            txbSortMask.Text = _Column.SortMask;
            txbAutoRemove.Text = _Column.AutoRemove;
            butSaveContent.Checked = _Column.SaveContent;

            cbxSchlüsselspalte.Item.Clear();
            cbxSchlüsselspalte.Item.Add("#Ohne", "-1");
            cbxDropDownKey.Item.Clear();
            cbxDropDownKey.Item.Add("#Ohne", "-1");
            cbxVorschlagSpalte.Item.Clear();
            cbxVorschlagSpalte.Item.Add("#Ohne", "-1");
            // Einige Dropdown-Menüs sind abhängig von der LinkedDatabase und werden in dessen TextChanged-Event befüllt
            // siehe Ende dieser Routine
            foreach (var ThisColumn in _Column.Database.Column) {
                if ((ThisColumn.Format == enDataFormat.RelationText || !ThisColumn.MultiLine) && ThisColumn.Format.CanBeCheckedByRules()) { cbxSchlüsselspalte.Item.Add(ThisColumn); }
                if (ThisColumn.Format.CanBeCheckedByRules() && !ThisColumn.MultiLine && !ThisColumn.Format.NeedTargetDatabase()) {
                    cbxDropDownKey.Item.Add(ThisColumn);
                    cbxVorschlagSpalte.Item.Add(ThisColumn);
                }
            }
            cbxSchlüsselspalte.Item.Sort();
            cbxVorschlagSpalte.Item.Sort();
            cbxDropDownKey.Item.Sort();

            SetKeyTo(cbxSchlüsselspalte, _Column.KeyColumnKey);
            SetKeyTo(cbxDropDownKey, _Column.DropdownKey);
            SetKeyTo(cbxVorschlagSpalte, _Column.VorschlagsColumn);
            cbxLinkedDatabase_TextChanged(null, System.EventArgs.Empty);
        }

        private void Column_DatenZurückschreibenx() {
            if (_Column.Database.ReadOnly) { return; }
            _Column.Name = tbxName.Text;
            _Column.Caption = tbxCaption.Text.Replace("\r\n", "\r").Trim().Trim("\r").Trim();
            _Column.Format = (enDataFormat)int.Parse(cbxFormat.Text);
            _Column.Quickinfo = tbxQuickinfo.Text.Replace("\r", "<BR>");
            _Column.AdminInfo = tbxAdminInfo.Text.Replace("\r", "<BR>");
            _Column.Suffix = cbxEinheit.Text;
            _Column.BackColor = QuickImage.Get(btnBackColor.ImageCode).ChangeGreenTo.FromHTMLCode();
            _Column.ForeColor = QuickImage.Get(btnTextColor.ImageCode).ChangeGreenTo.FromHTMLCode();
            _Column.LineLeft = (enColumnLineStyle)int.Parse(cbxRandLinks.Text);
            _Column.LineRight = (enColumnLineStyle)int.Parse(cbxRandRechts.Text);
            _Column.MultiLine = btnMultiline.Checked;
            _Column.AfterEdit_QuickSortRemoveDouble = btnAutoEditAutoSort.Checked;
            if (tbxRunden.Text.IsLong()) {
                var zahl = int.Parse(tbxRunden.Text);
                if (zahl is > -1 and < 7) { _Column.AfterEdit_Runden = zahl; }
            } else {
                _Column.AfterEdit_Runden = -1;
            }
            _Column.AfterEdit_DoUCase = btnAutoEditToUpper.Checked;
            _Column.AfterEdit_AutoCorrect = btnAutoEditKleineFehler.Checked;
            _Column.CellInitValue = tbxInitValue.Text;
            _Column.ShowMultiLineInOneLine = btnEinzeiligDarstellen.Checked;
            _Column.ShowUndo = btnLogUndo.Checked;
            _Column.SpellCheckingEnabled = btnSpellChecking.Checked;
            var tmpf = enFilterOptions.None;
            if (btnAutoFilterMoeglich.Checked) { tmpf |= enFilterOptions.Enabled; }
            if (btnAutoFilterTXTErlaubt.Checked) { tmpf |= enFilterOptions.TextFilterEnabled; }
            if (btnAutoFilterErweitertErlaubt.Checked) { tmpf |= enFilterOptions.ExtendedFilterEnabled; }
            if (chkFilterOnlyOr.Checked) { tmpf |= enFilterOptions.OnlyOrAllowed; }
            if (chkFilterOnlyAND.Checked) { tmpf |= enFilterOptions.OnlyAndAllowed; }
            //_Column.AutoFilterErlaubt = AutoFilterMöglich.Checked;
            //_Column.FilterOptions.HasFlag(enFilterOptions.TextFilterEnabled) = AutoFilterTXT.Checked;
            //_Column.FilterOptions.HasFlag(enFilterOptions.ExtendedFilterEnabled) = AutoFilterErw.Checked;
            _Column.FilterOptions = tmpf;
            _Column.IgnoreAtRowFilter = btnZeilenFilterIgnorieren.Checked;
            if (lbxCellEditor.Item.ToListOfString().IsDifferentTo(_Column.PermissionGroups_ChangeCell)) {
                _Column.PermissionGroups_ChangeCell.Clear();
                _Column.PermissionGroups_ChangeCell.AddRange(lbxCellEditor.Item.ToListOfString());
            }
            var NewDD = tbxAuswaehlbareWerte.Text.SplitAndCutByCRToList().SortedDistinctList();
            if (NewDD.IsDifferentTo(_Column.DropDownItems)) {
                _Column.DropDownItems.Clear();
                _Column.DropDownItems.AddRange(NewDD);
            }
            var NewRep = txbReplacer.Text.SplitAndCutByCRToList();
            if (NewRep.IsDifferentTo(_Column.OpticalReplace)) {
                _Column.OpticalReplace.Clear();
                _Column.OpticalReplace.AddRange(NewRep);
            }
            var NewRep2 = txbAutoReplace.Text.SplitAndCutByCRToList();
            if (NewRep2.IsDifferentTo(_Column.AfterEdit_AutoReplace)) {
                _Column.AfterEdit_AutoReplace.Clear();
                _Column.AfterEdit_AutoReplace.AddRange(NewRep2);
            }
            _Column.AutoFilterJoker = tbxJoker.Text;
            _Column.Ueberschrift1 = txbUeberschift1.Text;
            _Column.Ueberschrift2 = txbUeberschift2.Text;
            _Column.Ueberschrift3 = txbUeberschift3.Text;
            _Column.Prefix = txbPrefix.Text;
            _Column.CaptionBitmap = txbSpaltenbild.Text;
            _Column.DauerFilterPos = Extensions.PointParse(txbDauerFilterPos.Text);
            var NewTags = tbxTags.Text.SplitAndCutByCRToList();
            if (NewTags.IsDifferentTo(_Column.Tags)) {
                _Column.Tags.Clear();
                _Column.Tags.AddRange(NewTags);
            }
            var NewRegex = txbRegex.Text.SplitAndCutByCRToList();
            if (NewRegex.IsDifferentTo(_Column.Regex)) {
                _Column.Regex.Clear();
                _Column.Regex.AddRange(NewRegex);
            }
            _Column.TextBearbeitungErlaubt = btnEditableStandard.Checked;
            _Column.DropdownBearbeitungErlaubt = btnEditableDropdown.Checked;
            _Column.DropdownAllesAbwählenErlaubt = btnCanBeEmpty.Checked;
            _Column.DropdownWerteAndererZellenAnzeigen = btnOtherValuesToo.Checked;
            _Column.EditTrotzSperreErlaubt = btnIgnoreLock.Checked;
            _Column.AllowedChars = tbxAllowedChars.Text;
            int.TryParse(txbBildCodeConstHeight.Text, out var Res);
            _Column.BildCode_ConstantHeight = Res;
            int.TryParse(cbxBildTextVerhalten.Text, out var ImNF);
            _Column.BildTextVerhalten = (enBildTextVerhalten)ImNF;
            _Column.BestFile_StandardFolder = txbBestFileStandardFolder.Text;
            _Column.BestFile_StandardSuffix = txbBestFileStandardSuffix.Text;
            _Column.LinkedDatabaseFile = cbxLinkedDatabase.Text; // Muss vor LinkedCell_RowKey zurückgeschrieben werden
            _Column.LinkedKeyKennung = txbLinkedKeyKennung.Text;
            _Column.KeyColumnKey = ColumKeyFrom(_Column.Database, cbxSchlüsselspalte.Text);
            _Column.LinkedCell_RowKey = ColumKeyFrom(_Column.Database, cbxRowKeyInColumn.Text);
            _Column.LinkedCell_ColumnKey = ColumKeyFrom(_Column.LinkedDatabase(), cbxTargetColumn.Text); // LINKED DATABASE
            _Column.DropdownKey = ColumKeyFrom(_Column.Database, cbxDropDownKey.Text);
            _Column.VorschlagsColumn = ColumKeyFrom(_Column.Database, cbxVorschlagSpalte.Text);
            _Column.Align = (enAlignmentHorizontal)int.Parse(cbxAlign.Text);
            _Column.SortMask = txbSortMask.Text;
            _Column.AutoRemove = txbAutoRemove.Text;
            _Column.SaveContent = butSaveContent.Checked;
            //_Column.Database.Rules.Sort();
        }

        private void Database_ShouldICancelDiscOperations(object sender, System.ComponentModel.CancelEventArgs e) => e.Cancel = true;

        private void SetKeyTo(ComboBox combobox, long columnKey) => combobox.Text = columnKey.ToString();

        #endregion

        //if (database is null || columnKey < 0) {//    combobox.Text = "#Ohne";//} else {//    var c = database.Column.SearchByKey(columnKey);//    combobox.Text = c == null ? "#Ohne" : c.Name;//}
    }
}