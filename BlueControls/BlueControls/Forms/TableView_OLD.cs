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
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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

        public TableView_OLD() : this(null, true, true) { }

        public TableView_OLD(Database? database) : this(database, false, false) { }

        public TableView_OLD(Database? database, bool loadTabVisible, bool adminTabVisible) {
            InitializeComponent();
            //var bmp = new System.Drawing.Bitmap(111,112);
            //var gr = System.Drawing.Graphics.FromImage(bmp);
            //gr.Clear(System.Drawing.Color.Gray);
            //gr.Dispose();
            //var stUTF8 = BlueBasics.modConverter.BitmapToStringUnicode(bmp, System.Drawing.Imaging.ImageFormat.Png);
            //var b = stUTF8.UTF8_ToByte();
            //var newstUTF8 = b.ToStringUTF8();
            //var tmpb = newstUTF8.UTF8_ToByte();
            //var eq = b.SequenceEqual(tmpb);
            //var newbmp = BlueBasics.modConverter.StringUnicodeToBitmap(newstUTF8);
            Copyright.Text = "(c) 2010-" + DateTime.Now.Year + " Christian Peter";
            btnDrucken.Item.Clear();
            btnDrucken.Item.Add("Drucken bzw. Export", "erweitert", QuickImage.Get(enImageCode.Drucker, 28));
            btnDrucken.Item.AddSeparator();
            btnDrucken.Item.Add("CSV-Format für Excel in die Zwischenablage", "csv", QuickImage.Get(enImageCode.Excel, 28));
            btnDrucken.Item.Add("HTML-Format für Internet-Seiten", "html", QuickImage.Get(enImageCode.Globus, 28));
            btnDrucken.Item.AddSeparator();
            btnDrucken.Item.Add("Layout-Editor öffnen", "editor", QuickImage.Get(enImageCode.Layout, 28));
            if (!adminTabVisible) {
                grpAdminAllgemein.Visible = false;
                grpBearbeitung.Visible = false;
            }
            if (!loadTabVisible) {
                ribMain.Controls.Remove(tabDatei);
            }
            DatabaseSet(database);
        }

        #endregion

        #region Methods

        public void LöscheZeile(object sender, System.EventArgs e) {
            Formula.HideViewEditor();
            if (Ansicht1.Checked) {
                if (Formula.ShowingRow == null) {
                    MessageBox.Show("Kein Eintrag gewählt.", enImageCode.Information, "OK");
                    return;
                }
                var tmpr = Formula.ShowingRow;
                if (MessageBox.Show("Soll der Eintrag<br><b>" + tmpr.CellFirstString() + "</b><br>wirklich <b>gelöscht</b> werden?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
                SuchEintragNoSave(enDirection.Unten, out var column, out var row);
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
                        if (MessageBox.Show("Zeile löschen?", enImageCode.Frage, "Ja", "Nein") == 0) {
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
                        MessageBox.Show("Die Summe konnte nicht berechnet werden.", enImageCode.Summe, "OK");
                    } else {
                        MessageBox.Show("Summe dieser Spalte, nur angezeigte Zeilen: <br><b>" + summe, enImageCode.Summe, "OK");
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
                        if (MessageBox.Show("Angezeite Inhalte dieser Spalte löschen?", enImageCode.Frage, "Ja", "Nein") == 0) {
                            column.DeleteContents(Table.Filter, Table.PinnedRows);
                        }
                    }
                    break;
            }
        }

        protected override void OnLoad(System.EventArgs e) {
            base.OnLoad(e);
            CheckButtons();
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

        private void btnAlleErweitern_Click(object sender, System.EventArgs e) => Table.ExpandAll();

        private void btnAlleSchließen_Click(object sender, System.EventArgs e) => Table.CollapesAll();

        private void btnNeuDB_SaveAs_Click(object sender, System.EventArgs e) {
            BlueBasics.MultiUserFile.MultiUserFile.SaveAll(false);
            var bu = (Button)sender;
            switch (bu.Name) {
                case "btnSaveAs":
                    if (Table.Database == null) { return; }
                    break;

                case "btnNeuDB":
                    if (Table.Database != null) { DatabaseSet(null); }
                    break;

                default:
                    DebugPrint(enFehlerArt.Fehler, "Ungültiger Aufruf!");
                    break;
            }
            SaveTab.ShowDialog();
            if (!PathExists(SaveTab.FileName.FilePath())) { return; }
            if (string.IsNullOrEmpty(SaveTab.FileName)) { return; }
            SelectStandardTab();
            if (bu.Name == "btnNeuDB") {
                DatabaseSet(new Database(false)); // Ab jetzt in der Variable _Database zu finden
            }
            if (FileExists(SaveTab.FileName)) { DeleteFile(SaveTab.FileName, true); }
            Table.Database.SaveAsAndChangeTo(SaveTab.FileName);
            DatabaseSet(SaveTab.FileName);
        }

        private void btnOeffnen_Click(object sender, System.EventArgs e) {
            BlueBasics.MultiUserFile.MultiUserFile.SaveAll(false);
            LoadTab.ShowDialog();
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

        private void cbxColumnArr_ItemClicked(object sender, BasicListItemEventArgs e) {
            if (string.IsNullOrEmpty(cbxColumnArr.Text)) { return; }
            Table.Arrangement = IntParse(e.Item.Internal);
        }

        private void Check_SuchButton() => SuchB.Enabled = Table.Database != null && Table.Database.Row.Count >= 1 && !string.IsNullOrEmpty(such.Text) && !string.IsNullOrEmpty(such.Text.RemoveChars(" "));

        private void CheckButtons() {
            var datenbankDa = Convert.ToBoolean(Table.Database != null);
            btnNeuDB.Enabled = true;
            btnOeffnen.Enabled = true;
            btnNeu.Enabled = datenbankDa && _ansicht == Ansicht.Überschriften_und_Formular && Table.Database.PermissionCheck(Table.Database.PermissionGroupsNewRow, null);
            btnLoeschen.Enabled = datenbankDa;
            btnDrucken.Enabled = datenbankDa;
            Ansicht0.Enabled = datenbankDa;
            Ansicht1.Enabled = datenbankDa;
            Ansicht2.Enabled = datenbankDa;
            btnDatenbanken.Enabled = datenbankDa && !string.IsNullOrEmpty(Table.Database.Filename);
            cbxColumnArr.Enabled = datenbankDa && Table.Design != BlueTableAppearance.OnlyMainColumnWithoutHead;
            SpaltAnsichtCap.Enabled = datenbankDa && Table.Design != BlueTableAppearance.OnlyMainColumnWithoutHead;
            SuchenUndErsetzen.Enabled = datenbankDa && Table.Design != BlueTableAppearance.OnlyMainColumnWithoutHead;
            AngezeigteZeilenLöschen.Enabled = datenbankDa && Table.Design != BlueTableAppearance.OnlyMainColumnWithoutHead;
            Datenüberprüfung.Enabled = datenbankDa;
            btnSaveAs.Enabled = datenbankDa;
            btnDrucken.Item["csv"].Enabled = datenbankDa && Table.Design != BlueTableAppearance.OnlyMainColumnWithoutHead;
            btnDrucken.Item["html"].Enabled = datenbankDa && Table.Design != BlueTableAppearance.OnlyMainColumnWithoutHead;
            btnVorwärts.Enabled = datenbankDa;
            btnZurück.Enabled = datenbankDa;
            such.Enabled = datenbankDa;
            FilterLeiste.Enabled = datenbankDa && Table.Design != BlueTableAppearance.OnlyMainColumnWithoutHead;
        }

        private void DatabaseSet(string filename) {
            DatabaseSet((Database)null);
            if (!FileExists(filename)) {
                CheckButtons();
                return;
            }
            btnLetzteDateien.AddFileName(filename, string.Empty);
            LoadTab.FileName = filename;
            var tmpDatabase = Database.GetByFilename(filename, false, false);
            if (tmpDatabase == null) { return; }
            DatabaseSet(tmpDatabase);
        }

        private void Datenüberprüfung_Click(object sender, System.EventArgs e) => Table.Database.Row.DoAutomatic(Table.Filter, true, Table.PinnedRows, "manual check");

        private void Drucken_ItemClicked(object sender, BasicListItemEventArgs e) {
            BlueBasics.MultiUserFile.MultiUserFile.SaveAll(false);
            switch (e.Item.Internal) {
                case "erweitert":
                    Visible = false;
                    List<RowItem?> selectedRows = new();
                    if (Ansicht1.Checked && Formula.ShowingRow != null) {
                        selectedRows.Add(Formula.ShowingRow);
                    } else {
                        selectedRows = Table.VisibleUniqueRows();
                    }
                    using (ExportDialog l = new(Table.Database, selectedRows)) {
                        l.ShowDialog();
                    }
                    Visible = true;
                    break;

                case "csv":
                    Generic.CopytoClipboard(Table.Export_CSV(FirstRow.ColumnCaption));
                    MessageBox.Show("Die gewünschten Daten<br>sind nun im Zwischenspeicher.", enImageCode.Clipboard, "Ok");
                    break;

                case "html":
                    Table.Export_HTML();
                    break;

                default:
                    DebugPrint(e.Item);
                    break;
            }
        }

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
            //        if (Table.CursorPosRow() == null && Table.View_RowFirst() != null) {
            //            Table.CursorPos_Set(Table.Database.Column[0], Table.View_RowFirst(), false);
            //        }
            //        if (Table.CursorPosRow() != null) { Formula.ShowingRowKey = Table.CursorPosRow().Row.Key; }
            //    }
            //} else {
            //    Formula.ShowingRowKey = -1;
            //}
        }

        private void LastDatabases_ItemClicked(object sender, BasicListItemEventArgs e) {
            BlueBasics.MultiUserFile.MultiUserFile.SaveAll(false);
            DatabaseSet(e.Item.Internal);
        }

        private void LoadTab_FileOk(object sender, CancelEventArgs e) => DatabaseSet(LoadTab.FileName);

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
            var r = Table.Database.Column.First().SortType == enSortierTyp.Datum_Uhrzeit
                ? Table.Database.Row.Add(NameRepair(DateTime.Now.ToString(Constants.Format_Date5), null))
                : Table.Database.Row.Add(NameRepair("Neuer Eintrag", null));
            Table.CursorPos_Set(Table.Database.Column.First(), Table.SortedRows().Get(r), true);
        }

        private void Ordn_Click(object sender, System.EventArgs e) {
            BlueBasics.MultiUserFile.MultiUserFile.SaveAll(false);
            SelectStandardTab();
            ExecuteFile(Table.Database.Filename.FilePath());
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
                MessageBox.Show("Bitte Text zum Suchen eingeben.", enImageCode.Information, "OK");
                return;
            }
            Table.SearchNextText(suchtT, Table, null, Table.CursorPosRow(), out _, out var gefRow, true);
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
            //                    if (ThisColumnItem.Format == enDataFormat.Text_mit_Formatierung)
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
                MessageBox.Show("Kein Eintrag gefunden!", enImageCode.Information, "OK");
            } else {
                if (gefRow?.Row == Formula.ShowingRow) {
                    MessageBox.Show("Text nur im <b>aktuellen Eintrag</b> gefunden,<br>aber sonst keine weiteren Einträge!", enImageCode.Information, "OK");
                } else {
                    Table.CursorPos_Set(Table.Database.Column[0], gefRow, true);
                }
            }
        }

        private void SuchEintragNoSave(enDirection richtung, out ColumnItem? column, out RowData? row) {
            column = Table.Database.Column[0];
            row = null;
            if (Table.Database.Row.Count < 1) { return; }
            // Temporär berechnen, um geflacker zu vermeiden (Endabled - > Disabled bei Nothing)
            if (Convert.ToBoolean(richtung & enDirection.Unten)) {
                row = Table.View_NextRow(Table.CursorPosRow());
                if (row == null) { row = Table.View_RowFirst(); }
            }
            if (Convert.ToBoolean(richtung & enDirection.Oben)) {
                row = Table.View_PreviousRow(Table.CursorPosRow());
                if (row == null) { row = Table.View_RowLast(); }
            }
            if (row == null) { row = Table.View_RowFirst(); }
        }

        private void SuchenUndErsetzen_Click(object sender, System.EventArgs e) => Table.OpenSearchAndReplace();

        private void TableView_ColumnArrangementChanged(object sender, System.EventArgs e) => Table.WriteColumnArrangementsInto(cbxColumnArr, Table.Database, Table.Arrangement);

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
                e.UserMenu.Add("Inhalt Kopieren", "ContentCopy", enImageCode.Kopieren, column != null && column.Format.CanBeChangedByRules());
                e.UserMenu.Add("Inhalt Einfügen", "ContentPaste", enImageCode.Clipboard, column != null && column.Format.CanBeChangedByRules());
                e.UserMenu.Add("Inhalt löschen", "ContentDelete", enImageCode.Radiergummi, column != null && column.Format.CanBeChangedByRules());
                e.UserMenu.Add(ContextMenuComands.VorherigenInhaltWiederherstellen, column != null && column.Format.CanBeChangedByRules() && column.ShowUndo);
                e.UserMenu.Add(ContextMenuComands.SuchenUndErsetzen, column != null && column.Format.CanBeChangedByRules());
                e.UserMenu.AddSeparator();
                e.UserMenu.Add("Spalte", true);
                e.UserMenu.Add(ContextMenuComands.SpaltenEigenschaftenBearbeiten, column != null);
                e.UserMenu.Add("Statistik", "Statistik", QuickImage.Get(enImageCode.Balken, 16), column != null);
                e.UserMenu.Add("Inhalte aller angezeigten Zellen dieser Spalte löschen", "ColumnContentDelete", enImageCode.Radiergummi, column != null && column.Format.CanBeChangedByRules());
                e.UserMenu.Add("Summe", "Summe", enImageCode.Summe, column != null);
                e.UserMenu.AddSeparator();
            }
            e.UserMenu.Add("Zeile", true);
            e.UserMenu.Add(ContextMenuComands.ZeileLöschen, row != null);
            e.UserMenu.Add("Zeile prüfen", "Fehlersuche", enImageCode.Zeile, row != null);
        }

        private void TableView_EditBeforeBeginEdit(object sender, CellCancelEventArgs e) {
            if (Table.Design == BlueTableAppearance.OnlyMainColumnWithoutHead) {
                e.CancelReason = "In dieser Ansicht kann der Eintrag nicht bearbeitet werden.";
            }
        }

        private void TableView_RowsSorted(object sender, System.EventArgs e) {
            CheckButtons();
        }

        private void TableView_ViewChanged(object sender, System.EventArgs e) => Table.WriteColumnArrangementsInto(cbxColumnArr, Table.Database, Table.Arrangement);

        private void TemporärenSpeicherortÖffnen_Click(object sender, System.EventArgs e) {
            BlueBasics.MultiUserFile.MultiUserFile.SaveAll(false);
            SelectStandardTab();
            ExecuteFile(Path.GetTempPath());
        }

        private void ÜberDiesesProgramm_Click(object sender, System.EventArgs e) {
            BlueBasics.MultiUserFile.MultiUserFile.SaveAll(false);
            MessageBox.Show("(c) Christian Peter<br>V " + Version, enImageCode.Information, "OK");
        }

        private void vor_Click(object sender, System.EventArgs e) {
            Formula.HideViewEditor();
            SuchEintragNoSave(enDirection.Unten, out var column, out var row);
            Table.CursorPos_Set(column, row, false);
        }

        private void zurück_Click(object sender, System.EventArgs e) {
            Formula.HideViewEditor();
            SuchEintragNoSave(enDirection.Oben, out var column, out var row);
            Table.CursorPos_Set(column, row, false);
        }

        #endregion
    }
}