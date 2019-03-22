#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2019 Christian Peter
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
#endregion

using System;
using System.IO;
using System.Text.RegularExpressions;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.DialogBoxes;
using BlueControls.ItemCollection;
using BlueDatabase;
using BlueDatabase.Enums;


namespace BlueControls.BlueDatabaseDialogs
{


    internal sealed partial class ColumnEditor
    {
        private ColumnItem _Column;





        public ColumnEditor(ColumnItem vColumn)
        {
            // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
            InitializeComponent();

            Column_DatenAuslesen(vColumn);

        }



        private void Column_DatenAuslesen(ColumnItem FromColumn)
        {


            _Column = FromColumn;

            cbxFormat.Item.AddRange(typeof(enDataFormat));
            cbxRandLinks.Item.AddRange(typeof(enColumnLineStyle));
            cbxRandRechts.Item.AddRange(typeof(enColumnLineStyle));
            cbxBildCodeImageNotfound.Item.AddRange(typeof(enImageNotFound));
            cbxFehlendesZiel.Item.AddRange(typeof(enFehlendesZiel));

            cbxLinkedDatabase.Item.Clear();
            if (!string.IsNullOrEmpty(_Column.Database.Filename))
            {
                var All = Directory.GetFiles(_Column.Database.Filename.FilePath(), "*.mdb", SearchOption.TopDirectoryOnly);
                foreach (var ThisString in All)
                {
                    if (ThisString.ToLower() != _Column.Database.Filename.ToLower()) { cbxLinkedDatabase.Item.Add(new TextListItem(ThisString.FileNameWithSuffix())); }
                }
            }
            cbxLinkedDatabase.Item.Sort();


            if (cbxEinheit.Item.Count < 1)
            {
                cbxEinheit.Item.Add(new TextListItem("µm", enImageCode.Lineal));
                cbxEinheit.Item.Add(new TextListItem("mm", enImageCode.Lineal));
                cbxEinheit.Item.Add(new TextListItem("cm", enImageCode.Lineal));
                cbxEinheit.Item.Add(new TextListItem("dm", enImageCode.Lineal));
                cbxEinheit.Item.Add(new TextListItem("m", enImageCode.Lineal));
                cbxEinheit.Item.Add(new TextListItem("km", enImageCode.Lineal));

                cbxEinheit.Item.Add(new TextListItem("µg", enImageCode.Gewicht));
                cbxEinheit.Item.Add(new TextListItem("mg", enImageCode.Gewicht));
                cbxEinheit.Item.Add(new TextListItem("g", enImageCode.Gewicht));
                cbxEinheit.Item.Add(new TextListItem("kg", enImageCode.Gewicht));
                cbxEinheit.Item.Add(new TextListItem("t", enImageCode.Gewicht));
            }


            lbxCellEditor.Suggestions.Clear();
            lbxCellEditor.Suggestions.AddRange(_Column.Database.Permission_AllUsed(false));

            btnZurueck.Enabled = _Column.Previous() != null;
            btnVor.Enabled = _Column.Next() != null;








            if (string.IsNullOrEmpty(_Column.Identifier))
            {
                btnStandard.Enabled = false;
                capInfo.Text = "<Imagecode=" + _Column.SymbolForReadableText() + "> Normale Spalte (Key: " + _Column.Key + ")";
            }
            else
            {
                btnStandard.Enabled = true;
                capInfo.Text = "<Imagecode=" + _Column.SymbolForReadableText() + "> " + _Column.Identifier + " (Key: " + _Column.Key + ")";
            }


            tbxName.Text = _Column.Name;
            tbxName.AllowedChars = ColumnItem.AllowedCharsInternalName;


            tbxCaption.Text = _Column.Caption;

            H_Colorx.ImageCode = QuickImage.Get(enImageCode.Kreis, 16, "", _Column.BackColor.ToHTMLCode()).ToString();
            T_Colorx.ImageCode = QuickImage.Get(enImageCode.Kreis, 16, "", _Column.ForeColor.ToHTMLCode()).ToString();
            btnMultiline.Checked = _Column.MultiLine;

            cbxFormat.Text = Convert.ToInt32(_Column.Format).ToString();

            cbxRandLinks.Text = Convert.ToInt32(_Column.LineLeft).ToString();
            cbxRandRechts.Text = Convert.ToInt32(_Column.LineRight).ToString();

            AutoFilterMöglich.Checked = _Column.AutoFilterErlaubt;
            AutoFilterTXT.Checked = _Column.AutofilterTextFilterErlaubt;
            AutoFilterErw.Checked = _Column.AutoFilterErweitertErlaubt;
            ZeilenFilter.Checked = _Column.IgnoreAtRowFilter;
            btnEditableStandard.Checked = _Column.TextBearbeitungErlaubt;
            btnEditableDropdown.Checked = _Column.DropdownBearbeitungErlaubt;
            btnCanBeEmpty.Checked = _Column.DropdownAllesAbwählenErlaubt;
            btnAutoEditAutoSort.Checked = _Column.AfterEdit_QuickSortRemoveDouble;
            if (_Column.AfterEdit_Runden > -1 && _Column.AfterEdit_Runden < 7)
            {
                tbxRunden.Text = _Column.AfterEdit_Runden.ToString();
            }
            else
            {
                tbxRunden.Text = string.Empty;
            }
            btnAutoEditToUpper.Checked = _Column.AfterEdit_DoUCase;
            btnAutoEditKleineFehler.Checked = _Column.AfterEdit_AutoCorrect;
            tbxInitValue.Text = _Column.CellInitValue;

            tbxJoker.Text = _Column.AutoFilterJoker;

            txbUeberschift1.Text = _Column.Ueberschrift1;
            txbUeberschift2.Text = _Column.Ueberschrift2;
            txbUeberschift3.Text = _Column.Ueberschrift3;



            txbPrefix.Text = _Column.Prefix;

            btnKompakteAnzeige.Checked = _Column.CompactView;
            btnLogUndo.Checked = _Column.ShowUndo;
            btnSpellChecking.Checked = _Column.SpellCheckingEnabled;
            btnEinzeiligDarstellen.Checked = _Column.ShowMultiLineInOneLine;

            tbxAuswaehlbareWerte.Text = _Column.DropDownItems.JoinWithCr();

            txbReplacer.Text = _Column.Replacer.JoinWithCr();

            tbxTags.Text = _Column.Tags.JoinWithCr();


            lbxCellEditor.Item.Clear();
            lbxCellEditor.Item.AddRange(_Column.PermissionGroups_ChangeCell.ToArray());

            tbxAllowedChars.Text = _Column.AllowedChars;

            btnOtherValuesToo.Checked = _Column.DropdownWerteAndererZellenAnzeigen;
            btnIgnoreLock.Checked = _Column.EditTrotzSperreErlaubt;

            tbxAdminInfo.Text = _Column.AdminInfo.Replace("<br>", "\r", RegexOptions.IgnoreCase);
            tbxQuickinfo.Text = _Column.Quickinfo.Replace("<br>", "\r", RegexOptions.IgnoreCase);


            cbxEinheit.Text = _Column.Suffix;


            picCaptionImage.SetBitmap(_Column.CaptionBitmap);



            txbBildCodeConstHeight.Text = _Column.BildCode_ConstantHeight.ToString();
            cbxBildCodeImageNotfound.Text = ((int)_Column.BildCode_ImageNotFound).ToString();
            txbBestFileStandardFolder.Text = _Column.BestFile_StandardFolder;
            txbBestFileStandardSuffix.Text = _Column.BestFile_StandardSuffix;
            cbxLinkedDatabase.Text = _Column.LinkedDatabaseFile;
            txbLinkedKeyKennung.Text = _Column.LinkedKeyKennung;



            cbxSchlüsselspalte.Item.Clear();
            cbxSchlüsselspalte.Item.Add("#Ohne");
            foreach (var ThisColumn in _Column.Database.Column)
            {
                if (ThisColumn.Format == enDataFormat.RelationText || ThisColumn.Format == enDataFormat.KeyForSame)
                {
                    cbxSchlüsselspalte.Item.Add(ThisColumn);
                }
            }
            if (_Column.KeyColumnKey < 0)
            {
                cbxSchlüsselspalte.Text = "#Ohne";
            }
            else
            {
                var c = _Column.Database.Column.SearchByKey(_Column.KeyColumnKey);
                if (c != null)
                {
                    cbxSchlüsselspalte.Text = c.Name;
                }
                else
                {
                    cbxSchlüsselspalte.Text = "#Ohne";
                }
            }

            RegelTabVorbereiten(false);


            btnFehlerWennLeer.Checked = _Column.Database.Rules_Has(_Column, "Leer-Fehler") != null;
            btnFormatFehler.Checked = _Column.Database.Rules_Has(_Column, "Format-Fehler") != null;
            btnFehlerWennUnsichtbare.Checked = _Column.Database.Rules_Has(_Column, "Unsichtbare-Zeichen-Fehler") != null;


            var R = _Column.Database.Rules_Has(_Column, "Verlinkung");
            cbxRowKeyInColumn.Text = string.Empty;
            cbxColumnKeyInColumn.Text = string.Empty;
            cbxTargetColumn.Text = string.Empty;

            if (R != null && _Column.Format == enDataFormat.LinkedCell)
            {

                // Info:o[5] ist ein Dummy, der immer auf  gesetzt wird. Damit werden beim Split die Dimenionen repariert.
                // o[4] ist neu dazu gekommen, und nicht immer gesetzt. Da es vorher der Dummy war, ist es immer plus, was gut so ist, da das die vorherige standard einstellung war.
                var o = (R.Actions[0].Text + "\r+").SplitByCR();

                if (o.Length > 4)
                {
                    if (int.TryParse(o[0], out var RowKey)) { cbxRowKeyInColumn.Text = _Column.Database.Column.SearchByKey(RowKey).Name; }
                    if (int.TryParse(o[1], out var ColKey))
                    {
                        cbxColumnKeyInColumn.Text = _Column.Database.Column.SearchByKey(ColKey).Name;
                        btnColumnKeyInColumn.Checked = true;
                    }
                    if (int.TryParse(o[2], out var TarCol))
                    {
                        cbxTargetColumn.Text = _Column.LinkedDatabase().Column.SearchByKey(TarCol).Name;
                        btnTargetColumn.Checked = true;
                    }
                    txbZeichenkette.Text = o[3];
                    cbxFehlendesZiel.Text = o[4];

                }
            }
        }

        private bool AllOk()
        {
            var Feh = "";

            // Diese Fehler sind so schwer und darf auf keinen Fall in die Umwelt gelassen werden
            if (string.IsNullOrEmpty(Feh))
            {
                if (string.IsNullOrEmpty(tbxName.Text)) { Feh = "Spaltenname nicht definiert."; }
            }

            // Diese Fehler sind so schwer und darf auf keinen Fall in die Umwelt gelassen werden
            if (string.IsNullOrEmpty(Feh))
            {
                foreach (var ThisColumn in _Column.Database.Column)
                {
                    if (ThisColumn != _Column && ThisColumn != null)
                    {
                        if (tbxName.Text.ToUpper() == ThisColumn.Name.ToUpper()) { Feh = "Spalten-Name bereits vorhanden."; }
                    }
                }
            }



            if (string.IsNullOrEmpty(Feh))
            {
                Column_DatenZurückschreibenx();
                if (string.IsNullOrEmpty(Feh)) { Feh = _Column.ErrorReason(); }
            }

            if (!string.IsNullOrEmpty(Feh))
            {
                MessageBox.Show("<b><u>Bitte korrigieren sie zuerst folgenden Fehler:</u></b><br>" + Feh, enImageCode.Warnung, "Ok");
                return false;
            }

            return true;
        }


        private void OkBut_Click(object sender, System.EventArgs e)
        {
            if (!AllOk()) { return; }
            Close();
        }


        private void Column_DatenZurückschreibenx()
        {

            if (_Column.Database.ReadOnly) { return; }

            _Column.Name = tbxName.Text;

            _Column.Caption = tbxCaption.Text.Replace("\r\n", "\r").Trim().Trim("\r").Trim();

            _Column.Format = (enDataFormat)Convert.ToInt32(cbxFormat.Text);

            _Column.Quickinfo = tbxQuickinfo.Text.Replace("\r", "<BR>");
            _Column.AdminInfo = tbxAdminInfo.Text.Replace("\r", "<BR>");
            _Column.Suffix = cbxEinheit.Text;


            _Column.BackColor = QuickImage.Get(H_Colorx.ImageCode).ChangeGreenTo.FromHTMLCode();
            _Column.ForeColor = QuickImage.Get(T_Colorx.ImageCode).ChangeGreenTo.FromHTMLCode();
            _Column.LineLeft = (enColumnLineStyle)Convert.ToInt32(cbxRandLinks.Text);
            _Column.LineRight = (enColumnLineStyle)Convert.ToInt32(cbxRandRechts.Text);


            _Column.MultiLine = btnMultiline.Checked;
            _Column.AfterEdit_QuickSortRemoveDouble = btnAutoEditAutoSort.Checked;

            if (tbxRunden.Text.IsLong())
            {
                var zahl = int.Parse(tbxRunden.Text);
                if (zahl > -1 && zahl < 7)
                {
                    _Column.AfterEdit_Runden = zahl;
                }
            }
            else
            {
                _Column.AfterEdit_Runden = -1;
            }
            _Column.AfterEdit_DoUCase = btnAutoEditToUpper.Checked;
            _Column.AfterEdit_AutoCorrect = btnAutoEditKleineFehler.Checked;
            _Column.CellInitValue = tbxInitValue.Text;


            _Column.ShowMultiLineInOneLine = btnEinzeiligDarstellen.Checked;
            _Column.CompactView = btnKompakteAnzeige.Checked;
            _Column.ShowUndo = btnLogUndo.Checked;
            _Column.SpellCheckingEnabled = btnSpellChecking.Checked;

            _Column.AutoFilterErlaubt = AutoFilterMöglich.Checked;
            _Column.AutofilterTextFilterErlaubt = AutoFilterTXT.Checked;
            _Column.AutoFilterErweitertErlaubt = AutoFilterErw.Checked;

            _Column.IgnoreAtRowFilter = ZeilenFilter.Checked;



            if (lbxCellEditor.Item.ToListOfString().IsDifferentTo(_Column.PermissionGroups_ChangeCell))
            {
                _Column.PermissionGroups_ChangeCell.Clear();
                _Column.PermissionGroups_ChangeCell.AddRange(lbxCellEditor.Item.ToListOfString());
            }


            var NewDD = tbxAuswaehlbareWerte.Text.SplitByCRToList().SortedDistinctList();
            if (NewDD.IsDifferentTo(_Column.DropDownItems))
            {
                _Column.DropDownItems.Clear();
                _Column.DropDownItems.AddRange(NewDD);
            }


            var NewRep = txbReplacer.Text.SplitByCRToList();
            //NewRep.QuickSortAndRemoveDouble();
            if (NewRep.IsDifferentTo(_Column.Replacer))
            {
                _Column.Replacer.Clear();
                _Column.Replacer.AddRange(NewRep);
            }

            _Column.AutoFilterJoker = tbxJoker.Text;

            _Column.Ueberschrift1 = txbUeberschift1.Text;
            _Column.Ueberschrift2 = txbUeberschift2.Text;
            _Column.Ueberschrift3 = txbUeberschift3.Text;

            _Column.Prefix = txbPrefix.Text;


            var NewTags = tbxTags.Text.SplitByCRToList();
            if (NewTags.IsDifferentTo(_Column.Tags))
            {
                _Column.Tags.Clear();
                _Column.Tags.AddRange(NewTags);
            }






            _Column.TextBearbeitungErlaubt = btnEditableStandard.Checked;
            _Column.DropdownBearbeitungErlaubt = btnEditableDropdown.Checked;
            _Column.DropdownAllesAbwählenErlaubt = btnCanBeEmpty.Checked;

            _Column.DropdownWerteAndererZellenAnzeigen = btnOtherValuesToo.Checked;
            _Column.EditTrotzSperreErlaubt = btnIgnoreLock.Checked;

            _Column.AllowedChars = tbxAllowedChars.Text;

            _Column.CaptionBitmap = picCaptionImage.Bitmap;


            _Column.BildCode_ConstantHeight = int.Parse(txbBildCodeConstHeight.Text);
            _Column.BildCode_ImageNotFound = (enImageNotFound)int.Parse(cbxBildCodeImageNotfound.Text);
            _Column.BestFile_StandardFolder = txbBestFileStandardFolder.Text;
            _Column.BestFile_StandardSuffix = txbBestFileStandardSuffix.Text;
            _Column.LinkedDatabaseFile = cbxLinkedDatabase.Text;
            _Column.LinkedKeyKennung = txbLinkedKeyKennung.Text;




            var c = _Column.Database.Column[cbxSchlüsselspalte.Text];

            if (c is null)
            {
                _Column.KeyColumnKey = -1;
            }
            else
            {
                _Column.KeyColumnKey = c.Key;
            }
            

            // Regel: Wenn Leer, gib Fehler aus
            var tmpR = _Column.Database.Rules_Has(_Column, "Leer-Fehler");
            if (!btnFehlerWennLeer.Checked && tmpR != null) { _Column.Database.Rules.Remove(tmpR); }
            if (btnFehlerWennLeer.Checked && tmpR == null)
            {
                tmpR = new RuleItem(_Column, "Leer-Fehler");
                tmpR.Actions.Add(new RuleActionItem(tmpR, enAction.Ist, string.Empty, _Column));
                tmpR.Actions.Add(new RuleActionItem(tmpR, enAction.Setze_Fehlerhaft, string.Empty, null));
                _Column.Database.Rules.Add(tmpR);
            }

            // Regel: Wenn Format Fehlerhaft, gib Fehler aus
            tmpR = _Column.Database.Rules_Has(_Column, "Format-Fehler");
            if (!btnFormatFehler.Checked && tmpR != null) { _Column.Database.Rules.Remove(tmpR); }
            if (btnFormatFehler.Checked && tmpR == null)
            {
                tmpR = new RuleItem(_Column, "Format-Fehler");
                tmpR.Actions.Add(new RuleActionItem(tmpR, enAction.Formatfehler_des_Zelleninhaltes, string.Empty, _Column));
                tmpR.Actions.Add(new RuleActionItem(tmpR, enAction.Setze_Fehlerhaft, string.Empty, null));
                _Column.Database.Rules.Add(tmpR);
            }

            // Regel: Wenn Unsichtbare vorhanden, gib Fehler aus
            tmpR = _Column.Database.Rules_Has(_Column, "Unsichtbare-Zeichen-Fehler");
            if (!btnFehlerWennUnsichtbare.Checked && tmpR != null) { _Column.Database.Rules.Remove(tmpR); }
            if (btnFehlerWennUnsichtbare.Checked && tmpR == null)
            {
                tmpR = new RuleItem(_Column, "Unsichtbare-Zeichen-Fehler");
                tmpR.Actions.Add(new RuleActionItem(tmpR, enAction.Unsichtbare_Zeichen_am_Ende_Enthält, string.Empty, _Column));
                tmpR.Actions.Add(new RuleActionItem(tmpR, enAction.Setze_Fehlerhaft, string.Empty, null));
                _Column.Database.Rules.Add(tmpR);
            }



            // Regel: Verlinkung richtig stellen:
            tmpR = _Column.Database.Rules_Has(_Column, "Verlinkung");
            if (_Column.Format != enDataFormat.LinkedCell && tmpR != null)
            {
                _Column.Database.Rules.Remove(tmpR);
                tmpR = null;
            }
            if (_Column.Format == enDataFormat.LinkedCell && tmpR == null)
            {
                tmpR = new RuleItem(_Column, "Verlinkung");
                tmpR.Actions.Add(new RuleActionItem(tmpR, enAction.LinkedCell, string.Empty, _Column));
                _Column.Database.Rules.Add(tmpR);
            }

            if (tmpR != null)
            {
                var tmp = string.Empty;

                if (!string.IsNullOrEmpty(cbxRowKeyInColumn.Text))
                {
                    tmp = tmp + _Column.Database.Column[cbxRowKeyInColumn.Text].Key;
                }
                tmp = tmp + "\r";

                if (btnColumnKeyInColumn.Checked && !string.IsNullOrEmpty(cbxColumnKeyInColumn.Text))
                {
                    tmp = tmp + _Column.Database.Column[cbxColumnKeyInColumn.Text].Key;
                }
                tmp = tmp + "\r";
                if (btnTargetColumn.Checked && !string.IsNullOrEmpty(cbxTargetColumn.Text))
                {
                    if (_Column.LinkedDatabase() != null) { tmp = tmp + _Column.LinkedDatabase().Column[cbxTargetColumn.Text].Key; }
                }

                tmp = tmp + "\r" + txbZeichenkette.Text + "\r" + cbxFehlendesZiel.Text + "\r+"; // Plus, das split die Dimensionen richtig erstellt.
                tmpR.Actions[0].Text = tmp;
            }

            _Column.Database.Rules.Sort();
        }





        private void H_Color_Click(object sender, System.EventArgs e)
        {
            ColorDia.Color = QuickImage.Get(H_Colorx.ImageCode).ChangeGreenTo.FromHTMLCode();
            ColorDia.ShowDialog();
            H_Colorx.ImageCode = QuickImage.Get(enImageCode.Kreis, 16, "", ColorDia.Color.ToHTMLCode()).ToString();
        }

        private void T_Color_Click(object sender, System.EventArgs e)
        {
            ColorDia.Color = QuickImage.Get(T_Colorx.ImageCode).ChangeGreenTo.FromHTMLCode();
            ColorDia.ShowDialog();
            T_Colorx.ImageCode = QuickImage.Get(enImageCode.Kreis, 16, "", ColorDia.Color.ToHTMLCode()).ToString();
        }



        private void QI_Vorschau_Click(object sender, System.EventArgs e)
        {
            Notification.Show(tbxQuickinfo.Text.Replace("\r", "<BR>") + "<br><br><br>" + tbxAdminInfo.Text.Replace("\r", "<BR>"));
        }



        private void Minus_Click(object sender, System.EventArgs e)
        {

            if (!AllOk()) { return; }

            if (_Column.Previous() == null)
            {
                MessageBox.Show("Spalte nicht gültig!", enImageCode.Warnung, "OK");
                return;
            }

            Column_DatenAuslesen(_Column.Previous());
        }

        private void Plus_Click(object sender, System.EventArgs e)
        {
            if (!AllOk()) { return; }

            if (_Column.Next() == null)
            {
                MessageBox.Show("Spalte nicht gültig!", enImageCode.Warnung, "OK");
                return;
            }

            Column_DatenAuslesen(_Column.Next());
        }



        protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (!AllOk()) { e.Cancel = true; }
        }

        private void tabControl_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            RegelTabVorbereiten(true);
        }


        private void RegelTabVorbereiten(bool Einlesen)
        {
            if (tabControl.SelectedIndex < 0) { return; }


            var xtab = tabControl.TabPages[tabControl.SelectedIndex];
            if (xtab.Text != "Regeln") { return; }


            if (Einlesen && !AllOk())
            {
                tabControl.SelectedIndex = 0;
                return;
            }

            cbxColumnKeyInColumn.Item.Clear();
            cbxTargetColumn.Item.Clear();
            cbxRowKeyInColumn.Item.Clear();


            if (_Column.Format == enDataFormat.LinkedCell)
            {
                gpxVerlinkteZellen.Enabled = true;
                foreach (var ThisLinkedColumn in _Column.LinkedDatabase().Column)
                {
                    if (ThisLinkedColumn.Format.CanBeChangedByRules() && !ThisLinkedColumn.Format.NeedTargetDatabase()) { cbxTargetColumn.Item.Add(ThisLinkedColumn); }

                }

                foreach (var ThisColumn in _Column.Database.Column)
                {
                    if (ThisColumn.Format.CanBeChangedByRules() && !ThisColumn.MultiLine && !ThisColumn.Format.NeedTargetDatabase()) { cbxRowKeyInColumn.Item.Add(ThisColumn); }
                    if (ThisColumn.Format == enDataFormat.Values_für_LinkedCellDropdown && ThisColumn.LinkedDatabase() == _Column.LinkedDatabase()) { cbxRowKeyInColumn.Item.Add(ThisColumn); }
                    if (ThisColumn.Format == enDataFormat.Columns_für_LinkedCellDropdown && ThisColumn.LinkedDatabase() == _Column.LinkedDatabase()) { cbxColumnKeyInColumn.Item.Add(ThisColumn); }
                }
            }
            else
            {
                gpxVerlinkteZellen.Enabled = false;
            }

            cbxRowKeyInColumn.Item.Sort();
            cbxColumnKeyInColumn.Item.Sort();
            cbxTargetColumn.Item.Sort();

            cbxColumnKeyInColumn.Enabled = cbxColumnKeyInColumn.Item.Count > 0;
            btnColumnKeyInColumn.Enabled = cbxColumnKeyInColumn.Enabled;
            txbZeichenkette.Enabled = cbxColumnKeyInColumn.Enabled;
            if (!btnColumnKeyInColumn.Enabled) { btnTargetColumn.Checked = true; } // Nicht perfekt die Lösung :-(

            cbxTargetColumn.Enabled = cbxTargetColumn.Item.Count > 0;
            btnTargetColumn.Enabled = cbxTargetColumn.Enabled;
            if (!btnTargetColumn.Enabled) { btnColumnKeyInColumn.Checked = true; } // Nicht perfekt die Lösung :-(

            cbxRowKeyInColumn.Enabled = cbxRowKeyInColumn.Item.Count > 0;
        }

        private void cbxFormat_TextChanged(object sender, System.EventArgs e)
        {
            var tmpFormat = (enDataFormat)Convert.ToInt32(cbxFormat.Text);

            // Verknüpfte Datenbank
            capLinkedDatabase.Enabled = tmpFormat.NeedTargetDatabase();
            cbxLinkedDatabase.Enabled = tmpFormat.NeedTargetDatabase();
            if (!tmpFormat.NeedTargetDatabase()) { cbxLinkedDatabase.Text = string.Empty; }

            // LinkedKey-Kennung
            capLinkedKeyKennung.Enabled = tmpFormat.NeedLinkedKeyKennung();
            txbLinkedKeyKennung.Enabled = tmpFormat.NeedLinkedKeyKennung();
            if (!tmpFormat.NeedLinkedKeyKennung()) { txbLinkedKeyKennung.Text = string.Empty; }

            // Mehrzeilig
            btnMultiline.Enabled = tmpFormat.MultilinePossible();
            if (!tmpFormat.MultilinePossible()) { btnMultiline.Checked = false; }

            // Kompakt
            btnKompakteAnzeige.Enabled = tmpFormat.CompactPossible();
            if (!tmpFormat.CompactPossible()) { btnKompakteAnzeige.Checked = false; }

            // Rechtschreibprüfung
            btnSpellChecking.Enabled = tmpFormat.SpellCheckingPossible();
            if (!tmpFormat.SpellCheckingPossible()) { btnSpellChecking.Checked = false; }

        }

        private void btnStandard_Click(object sender, System.EventArgs e)
        {
            if (!AllOk()) { return; }

            _Column.StandardWerteNachKennung(true);
            Column_DatenAuslesen(_Column);
        }
    }
}
