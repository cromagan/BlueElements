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
using BlueControls.Controls;
using BlueControls.Forms;
using BlueControls.ItemCollection;
using BlueDatabase;
using BlueDatabase.Enums;
using System.Collections.Generic;
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
            // Dieser Aufruf ist f�r den Windows Form-Designer erforderlich.
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
                Column_DatenZur�ckschreiben();
                if (string.IsNullOrEmpty(Feh)) { Feh = _Column.ErrorReason(); }
            }
            if (!string.IsNullOrEmpty(Feh)) {
                MessageBox.Show("<b><u>Bitte korrigieren sie zuerst folgenden Fehler:</u></b><br>" + Feh, enImageCode.Warnung, "Ok");
                return false;
            }

            _Column.Repair();
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

        private void btnSchnellAuswahloptionen_Click(object sender, System.EventArgs e) {
            Column_DatenZur�ckschreiben();
            _Column.SetFormatForTextOptions();
            Column_DatenAuslesen(_Column);
        }

        private void btnSchnellBildCode_Click(object sender, System.EventArgs e) {
            Column_DatenZur�ckschreiben();
            _Column.SetFormatForBildCode();
            Column_DatenAuslesen(_Column);
        }

        private void btnSchnellBit_Click(object sender, System.EventArgs e) {
            Column_DatenZur�ckschreiben();
            _Column.SetFormatForBit();
            Column_DatenAuslesen(_Column);
        }

        private void btnSchnellDatum_Click(object sender, System.EventArgs e) {
            Column_DatenZur�ckschreiben();
            _Column.SetFormatForDate();
            Column_DatenAuslesen(_Column);
        }

        private void btnSchnellDatumUhrzeit_Click(object sender, System.EventArgs e) {
            Column_DatenZur�ckschreiben();
            _Column.SetFormatForDateTime();
            Column_DatenAuslesen(_Column);
        }

        private void btnSchnellEmail_Click(object sender, System.EventArgs e) {
            Column_DatenZur�ckschreiben();
            _Column.SetFormatForEmail();
            Column_DatenAuslesen(_Column);
        }

        private void btnSchnellGanzzahl_Click(object sender, System.EventArgs e) {
            Column_DatenZur�ckschreiben();
            _Column.SetFormatForInteger();
            Column_DatenAuslesen(_Column);
        }

        private void btnSchnellGleitkommazahl_Click(object sender, System.EventArgs e) {
            Column_DatenZur�ckschreiben();
            _Column.SetFormatForFloat();
            Column_DatenAuslesen(_Column);
        }

        private void btnSchnellIInternetAdresse_Click(object sender, System.EventArgs e) {
            Column_DatenZur�ckschreiben();
            _Column.SetFormatForUrl();
            Column_DatenAuslesen(_Column);
        }

        private void btnSchnellTelefonNummer_Click(object sender, System.EventArgs e) {
            Column_DatenZur�ckschreiben();
            _Column.SetFormatForPhoneNumber();
            Column_DatenAuslesen(_Column);
        }

        private void btnSchnellText_Click(object sender, System.EventArgs e) {
            Column_DatenZur�ckschreiben();
            _Column.SetFormatForText();
            Column_DatenAuslesen(_Column);
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
            // Rechtschreibpr�fung
            btnSpellChecking.Enabled = tmpFormat.SpellCheckingPossible();
            if (!tmpFormat.SpellCheckingPossible()) { btnSpellChecking.Checked = false; }
            //// Format: Bildcode
            //grpBildCode.Enabled = tmpFormat == enDataFormat.BildCode;
            //if (tmpFormat != enDataFormat.BildCode) {
            //    txbBildCodeConstHeight.Text = string.Empty;
            //    cbxBildTextVerhalten.Text = string.Empty;
            //}
            // Format: LinkToFileSystem
            grpLinkToFileSystem.Enabled = tmpFormat == enDataFormat.Link_To_Filesystem;
            //if (tmpFormat != enDataFormat.BildCode) {
            //    txbBestFileStandardFolder.Text = string.Empty;
            //    txbBestFileStandardSuffix.Text = string.Empty;
            //}
            // LinkedDatabase - Verkn�pfte Datenbank
            grpLinkedDatabase.Enabled = tmpFormat.NeedTargetDatabase();
            if (!tmpFormat.NeedTargetDatabase()) {
                cbxLinkedDatabase.Text = string.Empty;
            }
            // Format: Columns f�r Linked Database / LinkedKey-Kennung
            grpColumnsForLinkedDatabase.Enabled = tmpFormat.NeedLinkedKeyKennung();
            if (!tmpFormat.NeedLinkedKeyKennung()) { txbLinkedKeyKennung.Text = string.Empty; }
            // Format: LinkedCell
            grpVerlinkteZellen.Enabled = tmpFormat is enDataFormat.Verkn�pfung_zu_anderer_Datenbank_Skriptgesteuert or enDataFormat.Verkn�pfung_zu_anderer_Datenbank;
        }

        private void cbxFormat_TextChanged(object sender, System.EventArgs e) => ButtonCheck();

        /// <summary>
        /// K�mmert sich um erlaubte Spalten f�r LinkedCell
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxLinkedDatabase_TextChanged(object sender, System.EventArgs e) {
            _Column.LinkedDatabaseFile = cbxLinkedDatabase.Text;

            cbxTargetColumn.Item.Clear();

            if (_Column.LinkedDatabase() != null) {
                //foreach (var ThisColumn in _Column.Database.Column) {
                //    if (ThisColumn.Format.CanBeCheckedByRules() && !ThisColumn.MultiLine && !ThisColumn.Format.NeedTargetDatabase()) {
                //        cbxRowKeyInColumn.Item.Add(ThisColumn);
                //    }
                //    //if (ThisColumn.Format == enDataFormat.Values_f�r_LinkedCellDropdownx && ThisColumn.LinkedDatabase() == _Column.LinkedDatabase()) {
                //    //    cbxRowKeyInColumn.Item.Add(ThisColumn);
                //    //}
                //}
                foreach (var ThisLinkedColumn in _Column.LinkedDatabase().Column) {
                    if (!ThisLinkedColumn.IsFirst() && ThisLinkedColumn.Format.CanBeChangedByRules() && !ThisLinkedColumn.Format.NeedTargetDatabase()) {
                        cbxTargetColumn.Item.Add(ThisLinkedColumn);
                    }
                }
            }
            cbxTargetColumn.Item.Sort();
            SetKeyTo(cbxTargetColumn, _Column.LinkedCell_ColumnKeyOfLinkedDatabase);
            cbxTargetColumn.Enabled = cbxTargetColumn.Item.Count > 0;
            capTargetColumn.Enabled = cbxTargetColumn.Enabled;
            if (!cbxTargetColumn.Enabled) {
                cbxTargetColumn.Text = string.Empty;
            }

            GeneratFilterListe();
        }

        private void cbxTargetColumn_TextChanged(object sender, System.EventArgs e) {
            GeneratFilterListe();
        }

        private int ColumKeyFrom(Database database, string columnKey) => database == null || string.IsNullOrEmpty(columnKey) ? -1 : IntParse(columnKey);

        private void Column_DatenAuslesen(ColumnItem FromColumn) {
            _Column = FromColumn;
            cbxFormat.Item.AddRange(typeof(enDataFormat));
            cbxRandLinks.Item.AddRange(typeof(enColumnLineStyle));
            cbxRandRechts.Item.AddRange(typeof(enColumnLineStyle));
            cbxBildTextVerhalten.Item.AddRange(typeof(enBildTextVerhalten));
            cbxAlign.Item.AddRange(typeof(enAlignmentHorizontal));
            cbxAdditionalCheck.Item.AddRange(typeof(enAdditionalCheck));
            cbxScriptType.Item.AddRange(typeof(enScriptType));
            cbxTranslate.Item.AddRange(typeof(enTranslationType));
            cbxSort.Item.AddRange(typeof(enSortierTyp));
            cbxLinkedDatabase.Item.Clear();
            if (!string.IsNullOrEmpty(_Column.Database.Filename)) {
                var All = Directory.GetFiles(_Column.Database.Filename.FilePath(), "*.mdb", SearchOption.TopDirectoryOnly);
                foreach (var ThisString in All) {
                    if (ThisString.ToLower() != _Column.Database.Filename.ToLower()) { cbxLinkedDatabase.Item.Add(ThisString.FileNameWithSuffix()); }
                }
            }
            cbxLinkedDatabase.Item.Sort();
            if (cbxEinheit.Item.Count < 1) {
                cbxEinheit.Item.Add("�m", enImageCode.Lineal);
                cbxEinheit.Item.Add("mm", enImageCode.Lineal);
                cbxEinheit.Item.Add("cm", enImageCode.Lineal);
                cbxEinheit.Item.Add("dm", enImageCode.Lineal);
                cbxEinheit.Item.Add("m", enImageCode.Lineal);
                cbxEinheit.Item.Add("km", enImageCode.Lineal);
                cbxEinheit.Item.Add("mm�", enImageCode.Gr��e�ndern);
                cbxEinheit.Item.Add("m�", enImageCode.Gr��e�ndern);
                cbxEinheit.Item.Add("�g", enImageCode.Gewicht);
                cbxEinheit.Item.Add("mg", enImageCode.Gewicht);
                cbxEinheit.Item.Add("g", enImageCode.Gewicht);
                cbxEinheit.Item.Add("kg", enImageCode.Gewicht);
                cbxEinheit.Item.Add("t", enImageCode.Gewicht);
                cbxEinheit.Item.Add("h", enImageCode.Uhr);
                cbxEinheit.Item.Add("min", enImageCode.Uhr);
                cbxEinheit.Item.Add("St.", enImageCode.Eins);
            }
            lbxCellEditor.Suggestions.Clear();
            lbxCellEditor.Suggestions.AddRange(_Column.Database.Permission_AllUsed(true));
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
            cbxAdditionalCheck.Text = ((int)_Column.AdditionalCheck).ToString();
            cbxScriptType.Text = ((int)_Column.ScriptType).ToString();
            cbxTranslate.Text = ((int)_Column.Translate).ToString();
            cbxSort.Text = ((int)_Column.SortType).ToString();
            btnAutoFilterMoeglich.Checked = _Column.FilterOptions.HasFlag(enFilterOptions.Enabled);
            btnAutoFilterTXTErlaubt.Checked = _Column.FilterOptions.HasFlag(enFilterOptions.TextFilterEnabled);
            btnAutoFilterErweitertErlaubt.Checked = _Column.FilterOptions.HasFlag(enFilterOptions.ExtendedFilterEnabled);
            chkFilterOnlyOr.Checked = _Column.FilterOptions.HasFlag(enFilterOptions.OnlyOrAllowed);
            chkFilterOnlyAND.Checked = _Column.FilterOptions.HasFlag(enFilterOptions.OnlyAndAllowed);
            btnZeilenFilterIgnorieren.Checked = _Column.IgnoreAtRowFilter;
            btnEditableStandard.Checked = _Column.TextBearbeitungErlaubt;
            btnEditableDropdown.Checked = _Column.DropdownBearbeitungErlaubt;
            btnCanBeEmpty.Checked = _Column.DropdownAllesAbw�hlenErlaubt;
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
            btnFormatierungErlaubt.Checked = _Column.FormatierungErlaubt;
            btnSpellChecking.Checked = _Column.SpellChecking;
            btnEinzeiligDarstellen.Checked = _Column.ShowMultiLineInOneLine;
            tbxAuswaehlbareWerte.Text = _Column.DropDownItems.JoinWithCr();
            txbReplacer.Text = _Column.OpticalReplace.JoinWithCr();
            txbAutoReplace.Text = _Column.AfterEdit_AutoReplace.JoinWithCr();
            txbRegex.Text = _Column.Regex;
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

            txbAutoRemove.Text = _Column.AutoRemove;
            butSaveContent.Checked = _Column.SaveContent;

            cbxSchl�sselspalte.Item.Clear();
            cbxSchl�sselspalte.Item.Add("#Ohne", "-1");
            cbxDropDownKey.Item.Clear();
            cbxDropDownKey.Item.Add("#Ohne", "-1");
            cbxVorschlagSpalte.Item.Clear();
            cbxVorschlagSpalte.Item.Add("#Ohne", "-1");
            // Einige Dropdown-Men�s sind abh�ngig von der LinkedDatabase und werden in dessen TextChanged-Event bef�llt
            // siehe Ende dieser Routine
            foreach (var ThisColumn in _Column.Database.Column) {
                if ((ThisColumn.Format == enDataFormat.RelationText || !ThisColumn.MultiLine) && ThisColumn.Format.CanBeCheckedByRules()) { cbxSchl�sselspalte.Item.Add(ThisColumn); }
                if (ThisColumn.Format.CanBeCheckedByRules() && !ThisColumn.MultiLine && !ThisColumn.Format.NeedTargetDatabase()) {
                    cbxDropDownKey.Item.Add(ThisColumn);
                    cbxVorschlagSpalte.Item.Add(ThisColumn);
                }
            }
            cbxSchl�sselspalte.Item.Sort();
            cbxVorschlagSpalte.Item.Sort();
            cbxDropDownKey.Item.Sort();

            SetKeyTo(cbxSchl�sselspalte, _Column.KeyColumnKey);
            SetKeyTo(cbxDropDownKey, _Column.DropdownKey);
            SetKeyTo(cbxVorschlagSpalte, _Column.VorschlagsColumn);
            cbxLinkedDatabase_TextChanged(null, System.EventArgs.Empty);
        }

        private void Column_DatenZur�ckschreiben() {
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
            _Column.FormatierungErlaubt = btnFormatierungErlaubt.Checked;
            _Column.SpellChecking = btnSpellChecking.Checked;
            var tmpf = enFilterOptions.None;
            if (btnAutoFilterMoeglich.Checked) { tmpf |= enFilterOptions.Enabled; }
            if (btnAutoFilterTXTErlaubt.Checked) { tmpf |= enFilterOptions.TextFilterEnabled; }
            if (btnAutoFilterErweitertErlaubt.Checked) { tmpf |= enFilterOptions.ExtendedFilterEnabled; }
            if (chkFilterOnlyOr.Checked) { tmpf |= enFilterOptions.OnlyOrAllowed; }
            if (chkFilterOnlyAND.Checked) { tmpf |= enFilterOptions.OnlyAndAllowed; }
            //_Column.AutoFilterErlaubt = AutoFilterM�glich.Checked;
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
            _Column.Regex = txbRegex.Text;
            _Column.TextBearbeitungErlaubt = btnEditableStandard.Checked;
            _Column.DropdownBearbeitungErlaubt = btnEditableDropdown.Checked;
            _Column.DropdownAllesAbw�hlenErlaubt = btnCanBeEmpty.Checked;
            _Column.DropdownWerteAndererZellenAnzeigen = btnOtherValuesToo.Checked;
            _Column.EditTrotzSperreErlaubt = btnIgnoreLock.Checked;
            _Column.AllowedChars = tbxAllowedChars.Text;

            _Column.BildCode_ConstantHeight = txbBildCodeConstHeight.Text;
            int.TryParse(cbxBildTextVerhalten.Text, out var ImNF);
            _Column.BildTextVerhalten = (enBildTextVerhalten)ImNF;
            _Column.BestFile_StandardFolder = txbBestFileStandardFolder.Text;
            _Column.BestFile_StandardSuffix = txbBestFileStandardSuffix.Text;
            _Column.LinkedDatabaseFile = cbxLinkedDatabase.Text; // Muss vor LinkedCell_RowKey zur�ckgeschrieben werden
            _Column.LinkedKeyKennung = txbLinkedKeyKennung.Text;
            _Column.KeyColumnKey = ColumKeyFrom(_Column.Database, cbxSchl�sselspalte.Text);
            _Column.LinkedCell_ColumnKeyOfLinkedDatabase = ColumKeyFrom(_Column.LinkedDatabase(), cbxTargetColumn.Text); // LINKED DATABASE
            _Column.DropdownKey = ColumKeyFrom(_Column.Database, cbxDropDownKey.Text);
            _Column.VorschlagsColumn = ColumKeyFrom(_Column.Database, cbxVorschlagSpalte.Text);
            _Column.Align = (enAlignmentHorizontal)int.Parse(cbxAlign.Text);
            _Column.AdditionalCheck = (enAdditionalCheck)int.Parse(cbxAdditionalCheck.Text);
            _Column.ScriptType = (enScriptType)int.Parse(cbxScriptType.Text);
            _Column.Translate = (enTranslationType)int.Parse(cbxTranslate.Text);
            _Column.SortType = (enSortierTyp)int.Parse(cbxSort.Text);
            _Column.AutoRemove = txbAutoRemove.Text;
            _Column.SaveContent = butSaveContent.Checked;
            GetLinkedCellFilter();

            _Column.Repair();
            //_Column.Database.Rules.Sort();
        }

        private void Database_ShouldICancelDiscOperations(object sender, System.ComponentModel.CancelEventArgs e) => e.Cancel = true;

        private void GeneratFilterListe() {
            GetLinkedCellFilter();

            _Column.LinkedDatabaseFile = cbxLinkedDatabase.Text;

            var linkdb = _Column.LinkedDatabase();
            if (linkdb == null) { tblFilterliste.Database = null; }
            if (tblFilterliste.Database == null) { tblFilterliste.Database = null; }

            if (tblFilterliste.Database != null && tblFilterliste.Database.Tags.TagGet("Filename") != linkdb.Filename) { tblFilterliste.Database = null; }

            if (linkdb == null) { return; }

            if (tblFilterliste.Database == null) {
                Database x = new(false);
                x.Column.Add("count", "count", enVarType.Integer);
                var vis = x.Column.Add("visible", "visible", enVarType.Bit);
                var sp = x.Column.Add("Spalte", "Spalte", enVarType.Text);
                sp.Align = enAlignmentHorizontal.Rechts;
                var b = x.Column.Add("Such", "Suchtext", enVarType.Text);
                b.Quickinfo = "<b>Entweder</b> ~Spaltenname~<br><b>oder</b> fester Text zum suchen<br>Mischen wird nicht unterst�tzt.";
                b.MultiLine = false;
                b.TextBearbeitungErlaubt = true;
                b.DropdownAllesAbw�hlenErlaubt = true;
                b.DropdownBearbeitungErlaubt = true;

                foreach (var ThisColumn in _Column.Database.Column) {
                    if (ThisColumn.Format.CanBeCheckedByRules() && !ThisColumn.MultiLine) {
                        b.DropDownItems.Add("~" + ThisColumn.Name.ToLower() + "~");
                        b.OpticalReplace.Add("~" + ThisColumn.Name.ToLower() + "~|[Spalte: " + ThisColumn.ReadableText() + "]");
                    }
                }

                x.RepairAfterParse();
                x.ColumnArrangements[1].ShowAllColumns();
                x.ColumnArrangements[1].Hide("visible");
                x.ColumnArrangements[1].HideSystemColumns();
                x.SortDefinition = new RowSortDefinition(x, "Count", false);
                tblFilterliste.Database = x;
                tblFilterliste.Arrangement = 1;

                x.Tags.TagSet("Filename", linkdb.Filename);

                tblFilterliste.Filter.Add(vis, enFilterType.Istgleich, "+");
            }

            linkdb.RepairAfterParse(); // Dass ja die 0 Ansicht stimmt

            var ok = int.TryParse(cbxTargetColumn.Text, out var key);
            ColumnItem SpalteauDB = null;
            if (ok) { SpalteauDB = linkdb.Column.SearchByKey(key); }

            for (var z = 0; z < linkdb.Column.Count; z++) {
                var col = linkdb.Column[z];

                var r = tblFilterliste.Database.Row[z.ToString()];
                if (r == null) {
                    r = tblFilterliste.Database.Row.Add(z.ToString());
                }

                r.CellSet("Spalte", col.ReadableText() + " = ");

                if (col.Format.Autofilter_m�glich() && !col.MultiLine && col != SpalteauDB && col != SpalteauDB && !col.Format.NeedTargetDatabase() && string.IsNullOrEmpty(col.Identifier)) {
                    r.CellSet("visible", true);
                } else {
                    r.CellSet("visible", false);
                }

                SetLinkedCellFilter();
            }
        }

        /// <summary>
        /// Holt die Werte aus tblFilterliste und schreibt sie in _Column.LinkedCellFilter
        /// Hat tblFilterliste keine Datenbank, bleibt die Variable _Column.LinkedCellFilter unver�ndert
        /// </summary>
        private void GetLinkedCellFilter() {
            if (tblFilterliste.Database == null) { return; }

            var nf = new List<string>();
            foreach (var thisr in tblFilterliste.Database.Row) {
                var x = thisr.CellGetInteger("Count");

                while (nf.Count <= x) { nf.Add(string.Empty); }
                if (thisr.CellGetBoolean("visible")) {
                    var tmp = "@" + thisr.CellGetString("Such");

                    foreach (var thisColumn in _Column.Database.Column) {
                        if ("@~" + thisColumn.Name.ToLower() + "~" == tmp) { tmp = thisColumn.Key.ToString(); }
                    }
                    if (tmp == "@") { tmp = string.Empty; }

                    nf[x] = tmp;
                }
            }

            nf = nf.JoinWithCr().SplitAndCutByCRToList();

            if (_Column.LinkedCellFilter.IsDifferentTo(nf)) {
                _Column.LinkedCellFilter.Clear();
                _Column.LinkedCellFilter.AddRange(nf);
            }
        }

        private void SetKeyTo(ComboBox combobox, long columnKey) => combobox.Text = columnKey.ToString();

        /// <summary>
        /// Holt die Werte aus _Column.LinkedCellFilter und schreibt sie in tblFilterliste
        ///Leer evtl. Werte aus tblFilterliste
        /// </summary>
        private void SetLinkedCellFilter() {
            foreach (var thisr in tblFilterliste.Database.Row) {
                var x = thisr.CellGetInteger("Count");

                if (x < _Column.LinkedCellFilter.Count) {
                    var tmp = _Column.LinkedCellFilter[x];

                    if (int.TryParse(tmp, out var key)) {
                        var col = _Column.Database.Column.SearchByKey(key);
                        if (col != null) {
                            tmp = "~" + col.Name.ToLower() + "~";
                        } else {
                            tmp = string.Empty;
                        }
                    } else if (tmp.StartsWith("@")) {
                        tmp = tmp.Substring(1);
                    } else {
                        tmp = string.Empty;
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