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
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Media.Converters;
using static BlueBasics.Develop;
using static BlueBasics.FileOperations;
using static BlueBasics.Converter;

#nullable enable

namespace BlueControls.Forms {

    public partial class TableView_OLD : TableView {

        #region Fields

        private const string Version = "1.0001";

        private Ansicht _ansicht = Ansicht.Nur_Tabelle;

        #endregion

        #region Constructors

        public TableView_OLD() : base(null, true, true) { }

        public TableView_OLD(Database? database) : base(database, false, false) { }

        #endregion

        #region Methods

        public void LöscheZeile(object sender, System.EventArgs e) {
            Formula.HideViewEditor();
            if (Ansicht1.Checked) {
                if (Formula.ShowingRow == null) {
                    MessageBox.Show("Kein Eintrag gewählt.", ImageCode.Information, "OK");
                    return;
                }
                var tmpr = Formula.ShowingRow;
                if (MessageBox.Show("Soll der Eintrag<br><b>" + tmpr.CellFirstString() + "</b><br>wirklich <b>gelöscht</b> werden?", ImageCode.Warnung, "Ja", "Nein") != 0) { return; }
                SuchEintragNoSave(Direction.Unten, out var column, out var row);
                Table.CursorPos_Set(column, row, false);
                Table.Database.Row.Remove(tmpr);
            } else {
                Table.Database.Row.Remove(Table.Filter, Table.PinnedRows);
            }
        }

        public void TableView_ContextMenuItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
            var bt = (Table)sender;
            var cellKey = e.Tags.TagGet("CellKey");
            if (string.IsNullOrEmpty(cellKey)) { return; }
            bt.Database.Cell.DataOfCellKey(cellKey, out var column, out var row);
            switch (e.ClickedComand) {
                case "SpaltenSortierungAZ":
                    bt.SortDefinitionTemporary = new RowSortDefinition(bt.Database, column.Name, false);
                    break;

                case "SpaltenSortierungZA":
                    bt.SortDefinitionTemporary = new RowSortDefinition(bt.Database, column.Name, true);
                    break;

                case "Fehlersuche":
                    MessageBox.Show(row.DoAutomatic(true, true, 10, "manual check").error);
                    break;

                case "ZeileLöschen":
                    if (row != null) {
                        if (MessageBox.Show("Zeile löschen?", ImageCode.Frage, "Ja", "Nein") == 0) {
                            bt.Database.Row.Remove(row);
                        }
                    }
                    break;

                case "ContentDelete":
                    Table.Database.Cell.Delete(column, row.Key);
                    break;

                case "SpaltenEigenschaftenBearbeiten":
                    OpenColumnEditor(column, row, Table);
                    CheckButtons();
                    break;

                case "ContentCopy":
                    Table.CopyToClipboard(column, row, true);
                    break;

                case "SuchenUndErsetzen":
                    Table.OpenSearchAndReplace();
                    break;

                case "ÜberallDel":
                    Table.OpenSearchAndReplace();
                    break;

                case "Summe":
                    var summe = column.Summe(Table.Filter);
                    if (!summe.HasValue) {
                        MessageBox.Show("Die Summe konnte nicht berechnet werden.", ImageCode.Summe, "OK");
                    } else {
                        MessageBox.Show("Summe dieser Spalte, nur angezeigte Zeilen: <br><b>" + summe, ImageCode.Summe, "OK");
                    }
                    break;

                case "Statistik":
                    column.Statisik(Table.Filter, Table.PinnedRows);
                    break;

                case "VorherigenInhaltWiederherstellen":
                    Table.DoUndo(column, row);
                    break;

                case "ContentPaste":
                    row.CellSet(column, System.Windows.Forms.Clipboard.GetText());
                    break;

                case "ColumnContentDelete":
                    if (column != null) {
                        if (MessageBox.Show("Angezeite Inhalte dieser Spalte löschen?", ImageCode.Frage, "Ja", "Nein") == 0) {
                            column.DeleteContents(Table.Filter, Table.PinnedRows);
                        }
                    }
                    break;
            }
        }

        protected override void CheckButtons() {
            base.CheckButtons();
            var datenbankDa = Convert.ToBoolean(Table.Database != null);
            btnNeu.Enabled = datenbankDa && _ansicht == Ansicht.Überschriften_und_Formular && Table.Database.PermissionCheck(Table.Database.PermissionGroupsNewRow, null);
            btnLoeschen.Enabled = datenbankDa;
            Ansicht0.Enabled = datenbankDa;
            Ansicht1.Enabled = datenbankDa;
            Ansicht2.Enabled = datenbankDa;

            SuchenUndErsetzen.Enabled = datenbankDa && Table.Design != BlueTableAppearance.OnlyMainColumnWithoutHead;
            AngezeigteZeilenLöschen.Enabled = datenbankDa && Table.Design != BlueTableAppearance.OnlyMainColumnWithoutHead;
            Datenüberprüfung.Enabled = datenbankDa;
            btnVorwärts.Enabled = datenbankDa;
            btnZurück.Enabled = datenbankDa;
            such.Enabled = datenbankDa;
            FilterLeiste.Enabled = datenbankDa && Table.Design != BlueTableAppearance.OnlyMainColumnWithoutHead;
        }

        protected override void OnLoad(System.EventArgs e) {
            base.OnLoad(e);
            CaptionAnzeige();
        }

        protected override void OnShown(System.EventArgs e) {
            base.OnShown(e);
            ribMain.SelectedIndex = 0;
            InitView();
        }

        //private void AllgemeinerEditor_Click(object sender, System.EventArgs e) {
        //    Hide();
        //    PadEditorWithFileAccess r = new() {
        //        WindowState = System.Windows.Forms.FormWindowState.Maximized
        //    };
        //    r.ShowDialog();
        //    Show();
        //}

        private void AngezeigteZeilenLöschen_Click(object sender, System.EventArgs e) {
            Table.Database.Row.Remove(Table.Filter, Table.PinnedRows);
            CheckButtons();
        }

        private void Ansicht_Click(object sender, System.EventArgs e) {
            _ansicht = (Ansicht)IntParse(((Button)sender).Name.Substring(7, 1));
            InitView();
            CheckButtons();
        }

        private void CaptionAnzeige() {
            if (Table.Database == null) {
                Text = "Be Creative! V" + Version;
                return;
            }
            Text = Table.Database != null
                ? Table.Database.Filename.FileNameWithSuffix() + " - Be Creative! V" + Version
                : "[Neue Datenbank] - Be Creative! V" + Version;
        }

        private void cbxColumnArr_TextChanged(object sender, System.EventArgs e) {
        }

        private void Check_SuchButton() => SuchB.Enabled = Table.Database != null && Table.Database.Row.Count >= 1 && !string.IsNullOrEmpty(such.Text) && !string.IsNullOrEmpty(such.Text.RemoveChars(" "));

        private void Datenüberprüfung_Click(object sender, System.EventArgs e) => Table.Database.Row.DoAutomatic(Table.Filter, true, Table.PinnedRows, "manual check");

        private void InitView() {
            //Formula.HideViewEditor();
            //Ansicht0.Checked = _ansicht == Ansicht.Nur_Tabelle;
            //Ansicht1.Checked = _ansicht == Ansicht.Überschriften_und_Formular;
            //Ansicht2.Checked = _ansicht == Ansicht.Tabelle_und_Formular_nebeneinander;
            //Table?.Filter?.Clear();
            //switch (_ansicht) {
            //    case Ansicht.Nur_Tabelle:
            //        grpFormularSteuerung.Visible = false;
            //        Formula.Visible = false;
            //        Formula.Dock = System.Windows.Forms.DockStyle.None;
            //        Table.Dock = System.Windows.Forms.DockStyle.None;
            //        Table.Design = BlueTableAppearance.Standard;
            //        Formula.Dock = System.Windows.Forms.DockStyle.None;
            //        Table.Dock = System.Windows.Forms.DockStyle.Fill;
            //        break;

            //    case Ansicht.Überschriften_und_Formular:
            //        grpFormularSteuerung.Visible = true;
            //        Table.Design = BlueTableAppearance.OnlyMainColumnWithoutHead;
            //        Formula.Visible = true;
            //        Table.Dock = System.Windows.Forms.DockStyle.Left;
            //        Table.Width = 250;
            //        Formula.BringToFront();
            //        Formula.Dock = System.Windows.Forms.DockStyle.Fill;
            //        break;

            //    case Ansicht.Tabelle_und_Formular_nebeneinander:
            //        grpFormularSteuerung.Visible = false;
            //        Table.Design = BlueTableAppearance.Standard;
            //        Formula.Visible = true;
            //        Table.Dock = System.Windows.Forms.DockStyle.None;
            //        Formula.Dock = System.Windows.Forms.DockStyle.Right;
            //        Formula.Width = (int)(MinimumSize.Width / 2.0);
            //        Table.BringToFront();
            //        Table.Dock = System.Windows.Forms.DockStyle.Fill;
            //        break;

            //    //case Ansicht.Tabelle_und_Formular_übereinander:
            //    //    grpFormularSteuerung.Visible = false;
            //    //    TableView.Design = BlueTableAppearance.Standard;
            //    //    Formula.Visible = true;
            //    //    TableView.Dock = System.Windows.Forms.DockStyle.None;
            //    //    Formula.Dock = System.Windows.Forms.DockStyle.Top;
            //    //    Formula.Height = (int)(MinimumSize.Height / 2.0);
            //    //    TableView.BringToFront();
            //    //    TableView.Dock = System.Windows.Forms.DockStyle.Fill;
            //    //    break;

            //    default:
            //        DebugPrint(_ansicht);
            //        break;
            //}
            //if (Table.Visible) {
            //    if (Table.Database != null) {
            //        if (Table.CursorPosRow == null && Table.View_RowFirst() != null) {
            //            Table.CursorPos_Set(Table.Database.Column[0], Table.View_RowFirst(), false);
            //        }
            //        if (Table.CursorPosRow != null) { Formula.ShowingRowKey = Table.CursorPosRow.Row.Key; }
            //    }
            //} else {
            //    Formula.ShowingRowKey = -1;
            //}
        }

        private string NameRepair(string istName, RowItem? vRow) {
            var newName = istName;
            var istZ = 0;
            do {
                var changed = false;
                if (Table.Database.Row.Any(thisRow => thisRow != null && thisRow != vRow && string.Equals(thisRow.CellFirstString(), newName, StringComparison.CurrentCultureIgnoreCase))) {
                    istZ++;
                    newName = istName + " (" + istZ + ")";
                    changed = true;
                }
                if (!changed) { return newName; }
            } while (true);
        }

        private void Neu_Click(object sender, System.EventArgs e) {
            var r = Table.Database.Column.First().SortType == SortierTyp.Datum_Uhrzeit
                ? Table.Database.Row.Add(NameRepair(DateTime.Now.ToString(Constants.Format_Date5), null))
                : Table.Database.Row.Add(NameRepair("Neuer Eintrag", null));
            Table.CursorPos_Set(Table.Database.Column.First(), Table.SortedRows().Get(r), true);
        }

        private void SelectStandardTab() => ribMain.SelectedIndex = 1;

        private void such_Enter(object sender, System.EventArgs e) {
            if (SuchB.Enabled) { SuchB_Click(SuchB, System.EventArgs.Empty); }
        }

        private void such_TextChanged(object sender, System.EventArgs e) => Check_SuchButton();

        private void SuchB_Click(object sender, System.EventArgs e) {
            Formula.HideViewEditor();
            var suchtT = such.Text.Trim();
            if (string.IsNullOrEmpty(suchtT)) {
                MessageBox.Show("Bitte Text zum Suchen eingeben.", ImageCode.Information, "OK");
                return;
            }
            Table.SearchNextText(suchtT, Table, null, Table.CursorPosRow, out _, out var gefRow, true);
            //var CheckRow = BlueFormulax.ShowingRow;
            //RowItem GefRow = null;
            //if (CheckRow == null) { CheckRow = TableView.View_RowFirst(); }
            //var Count = 0;
            //do
            //{
            //    if (Count > TableView.Database.Row.Count() + 1) { break; }
            //    if (GefRow != null && GefRow != BlueFormulax.ShowingRow) { break; }
            //    Count++;
            //    CheckRow = TableView.View_NextRow(CheckRow);
            //    if (CheckRow == null) { CheckRow = TableView.View_RowFirst(); }
            //    foreach (var ThisColumnItem in TableView.Database.Column)
            //    {
            //        if (ThisColumnItem != null)
            //        {
            //            if (!ThisColumnItem.IgnoreAtRowFilter)
            //            {
            //                var IsT = CheckRow.CellGetString(ThisColumnItem);
            //                if (!string.IsNullOrEmpty(IsT))
            //                {
            //                    if (ThisColumnItem.Format == DataFormat.Text_mit_Formatierung)
            //                    {
            //                        var l = new ExtText(enDesign.TextBox, enStates.Standard);
            //                        l.HtmlText = IsT;
            //                        IsT = l.PlainText;
            //                    }
            //                    // Allgemeine Prüfung
            //                    if (IsT.ToLower().Contains(SuchtT.ToLower()))
            //                    {
            //                        GefRow = CheckRow;
            //                    }
            //                    // Spezielle Format-Prüfung
            //                    var SuchT2 = DataFormat.CleanFormat(SuchtT, ThisColumnItem.Format);
            //                    IsT = DataFormat.CleanFormat(IsT, ThisColumnItem.Format);
            //                    if (!string.IsNullOrEmpty(SuchT2) && !string.IsNullOrEmpty(IsT))
            //                    {
            //                        if (IsT.ToLower().Contains(SuchT2.ToLower()))
            //                        {
            //                            GefRow = CheckRow;
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //} while (true);
            if (gefRow == null) {
                MessageBox.Show("Kein Eintrag gefunden!", ImageCode.Information, "OK");
            } else {
                if (gefRow?.Row == Formula.ShowingRow) {
                    MessageBox.Show("Text nur im <b>aktuellen Eintrag</b> gefunden,<br>aber sonst keine weiteren Einträge!", ImageCode.Information, "OK");
                } else {
                    Table.CursorPos_Set(Table.Database.Column[0], gefRow, true);
                }
            }
        }

        private void SuchEintragNoSave(Direction richtung, out ColumnItem? column, out RowData? row) {
            column = Table.Database.Column[0];
            row = null;
            if (Table.Database.Row.Count < 1) { return; }
            // Temporär berechnen, um geflacker zu vermeiden (Endabled - > Disabled bei Nothing)
            if (Convert.ToBoolean(richtung & Direction.Unten)) {
                row = Table.View_NextRow(Table.CursorPosRow);
                if (row == null) { row = Table.View_RowFirst(); }
            }
            if (Convert.ToBoolean(richtung & Direction.Oben)) {
                row = Table.View_PreviousRow(Table.CursorPosRow);
                if (row == null) { row = Table.View_RowLast(); }
            }
            if (row == null) { row = Table.View_RowFirst(); }
        }

        private void SuchenUndErsetzen_Click(object sender, System.EventArgs e) => Table.OpenSearchAndReplace();

        private void TableView_ContextMenu_Init(object sender, ContextMenuInitEventArgs e) {
            var bt = (Table)sender;
            var cellKey = e.Tags.TagGet("Cellkey");
            if (string.IsNullOrEmpty(cellKey)) { return; }
            bt.Database.Cell.DataOfCellKey(cellKey, out var column, out var row);
            if (_ansicht != Ansicht.Überschriften_und_Formular) {
                e.UserMenu.Add("Sortierung", true);
                e.UserMenu.Add(ContextMenuComands.SpaltenSortierungAZ, column != null && column.Format.CanBeCheckedByRules());
                e.UserMenu.Add(ContextMenuComands.SpaltenSortierungZA, column != null && column.Format.CanBeCheckedByRules());
                e.UserMenu.AddSeparator();
                e.UserMenu.Add("Zelle", true);
                e.UserMenu.Add("Inhalt Kopieren", "ContentCopy", ImageCode.Kopieren, column != null && column.Format.CanBeChangedByRules());
                e.UserMenu.Add("Inhalt Einfügen", "ContentPaste", ImageCode.Clipboard, column != null && column.Format.CanBeChangedByRules());
                e.UserMenu.Add("Inhalt löschen", "ContentDelete", ImageCode.Radiergummi, column != null && column.Format.CanBeChangedByRules());
                e.UserMenu.Add(ContextMenuComands.VorherigenInhaltWiederherstellen, column != null && column.Format.CanBeChangedByRules() && column.ShowUndo);
                e.UserMenu.Add(ContextMenuComands.SuchenUndErsetzen, column != null && column.Format.CanBeChangedByRules());
                e.UserMenu.AddSeparator();
                e.UserMenu.Add("Spalte", true);
                e.UserMenu.Add(ContextMenuComands.SpaltenEigenschaftenBearbeiten, column != null);
                e.UserMenu.Add("Statistik", "Statistik", QuickImage.Get(ImageCode.Balken, 16), column != null);
                e.UserMenu.Add("Inhalte aller angezeigten Zellen dieser Spalte löschen", "ColumnContentDelete", ImageCode.Radiergummi, column != null && column.Format.CanBeChangedByRules());
                e.UserMenu.Add("Summe", "Summe", ImageCode.Summe, column != null);
                e.UserMenu.AddSeparator();
            }
            e.UserMenu.Add("Zeile", true);
            e.UserMenu.Add(ContextMenuComands.ZeileLöschen, row != null);
            e.UserMenu.Add("Zeile prüfen", "Fehlersuche", ImageCode.Zeile, row != null);
        }

        private void TableView_EditBeforeBeginEdit(object sender, CellCancelEventArgs e) {
            if (Table.Design == BlueTableAppearance.OnlyMainColumnWithoutHead) {
                e.CancelReason = "In dieser Ansicht kann der Eintrag nicht bearbeitet werden.";
            }
        }

        private void TableView_RowsSorted(object sender, System.EventArgs e) {
            CheckButtons();
        }

        private void vor_Click(object sender, System.EventArgs e) {
            Formula.HideViewEditor();
            SuchEintragNoSave(Direction.Unten, out var column, out var row);
            Table.CursorPos_Set(column, row, false);
        }

        private void zurück_Click(object sender, System.EventArgs e) {
            Formula.HideViewEditor();
            SuchEintragNoSave(Direction.Oben, out var column, out var row);
            Table.CursorPos_Set(column, row, false);
        }

        #endregion
    }
}