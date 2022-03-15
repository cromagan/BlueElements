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

    public partial class FrmTableView {

        #region Fields

        private const string Version = "1.0001";

        private enAnsicht _ansicht = enAnsicht.Nur_Tabelle;

        #endregion

        #region Constructors

        public FrmTableView() : this(null, true, true) { }

        public FrmTableView(Database? database) : this(database, false, false) { }

        public FrmTableView(Database? database, bool loadTabVisible, bool adminTabVisible) {
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
                MainRibbon.Controls.Remove(tabAdmin);
                MainRibbon.Controls.Remove(tabExtras);
                grpAllgemein.Visible = false;
                grpBearbeitung.Visible = false;
            } else {
                tabAdmin.Table = TableView;
            }
            if (!loadTabVisible) {
                MainRibbon.Controls.Remove(tabDatei);
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
                TableView.CursorPos_Set(column, row, false);
                TableView.Database.Row.Remove(tmpr);
            } else {
                TableView.Database.Row.Remove(TableView.Filter, TableView.PinnedRows);
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
                    TableView.Database.Cell.Delete(column, row.Key);
                    break;

                case "SpaltenEigenschaftenBearbeiten":
                    TabAdministration.OpenColumnEditor(column, row, TableView);
                    CheckButtons();
                    break;

                case "ContentCopy":
                    Table.CopyToClipboard(column, row, true);
                    break;

                case "SuchenUndErsetzen":
                    TableView.OpenSearchAndReplace();
                    break;

                case "ÜberallDel":
                    TableView.OpenSearchAndReplace();
                    break;

                case "Summe":
                    var summe = column.Summe(TableView.Filter);
                    if (!summe.HasValue) {
                        MessageBox.Show("Die Summe konnte nicht berechnet werden.", enImageCode.Summe, "OK");
                    } else {
                        MessageBox.Show("Summe dieser Spalte, nur angezeigte Zeilen: <br><b>" + summe, enImageCode.Summe, "OK");
                    }
                    break;

                case "Statistik":
                    column.Statisik(TableView.Filter, TableView.PinnedRows);
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
                            column.DeleteContents(TableView.Filter, TableView.PinnedRows);
                        }
                    }
                    break;
            }
        }

        protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e) {
            SetDatabasetoNothing();
            BlueBasics.MultiUserFile.MultiUserFile.SaveAll(true);
            base.OnFormClosing(e);
        }

        protected override void OnLoad(System.EventArgs e) {
            base.OnLoad(e);
            CheckButtons();
            CaptionAnzeige();
        }

        protected override void OnShown(System.EventArgs e) {
            base.OnShown(e);
            MainRibbon.SelectedIndex = 0;
            InitView();
        }

        private void AllgemeinerEditor_Click(object sender, System.EventArgs e) {
            Hide();
            PadEditorWithFileAccess r = new() {
                WindowState = System.Windows.Forms.FormWindowState.Maximized
            };
            r.ShowDialog();
            Show();
        }

        private void AngezeigteZeilenLöschen_Click(object sender, System.EventArgs e) {
            TableView.Database.Row.Remove(TableView.Filter, TableView.PinnedRows);
            CheckButtons();
        }

        private void Ansicht_Click(object sender, System.EventArgs e) {
            _ansicht = (enAnsicht)IntParse(((Button)sender).Name.Substring(7, 1));
            InitView();
            CheckButtons();
        }

        private void BeziehungsEditor_Click(object sender, System.EventArgs e) {
            Hide();
            RelationDiagram r = new(TableView.Database);
            r.ShowDialog();
            Show();
        }

        private void btnAlleErweitern_Click(object sender, System.EventArgs e) => TableView.ExpandAll();

        private void btnAlleSchließen_Click(object sender, System.EventArgs e) => TableView.CollapesAll();

        private void btnNeuDB_SaveAs_Click(object sender, System.EventArgs e) {
            BlueBasics.MultiUserFile.MultiUserFile.SaveAll(false);
            var bu = (Button)sender;
            switch (bu.Name) {
                case "btnSaveAs":
                    if (TableView.Database == null) { return; }
                    break;

                case "btnNeuDB":
                    if (TableView.Database != null) { SetDatabasetoNothing(); }
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
            TableView.Database.SaveAsAndChangeTo(SaveTab.FileName);
            DatabaseSet(SaveTab.FileName);
        }

        private void btnOeffnen_Click(object sender, System.EventArgs e) {
            BlueBasics.MultiUserFile.MultiUserFile.SaveAll(false);
            LoadTab.ShowDialog();
        }

        private void CaptionAnzeige() {
            if (TableView.Database == null) {
                Text = "Be Creative! V" + Version;
                return;
            }
            Text = TableView.Database != null
                ? TableView.Database.Filename.FileNameWithSuffix() + " - Be Creative! V" + Version
                : "[Neue Datenbank] - Be Creative! V" + Version;
        }

        private void cbxColumnArr_ItemClicked(object sender, BasicListItemEventArgs e) {
            if (string.IsNullOrEmpty(cbxColumnArr.Text)) { return; }
            TableView.Arrangement = IntParse(e.Item.Internal);
        }

        private void Check_SuchButton() => SuchB.Enabled = TableView.Database != null && TableView.Database.Row.Count >= 1 && !string.IsNullOrEmpty(such.Text) && !string.IsNullOrEmpty(such.Text.RemoveChars(" "));

        private void CheckButtons() {
            var datenbankDa = Convert.ToBoolean(TableView.Database != null);
            btnNeuDB.Enabled = true;
            btnOeffnen.Enabled = true;
            btnNeu.Enabled = datenbankDa && _ansicht == enAnsicht.Überschriften_und_Formular && TableView.Database.PermissionCheck(TableView.Database.PermissionGroupsNewRow, null);
            btnLoeschen.Enabled = datenbankDa;
            btnDrucken.Enabled = datenbankDa;
            Ansicht0.Enabled = datenbankDa;
            Ansicht1.Enabled = datenbankDa;
            Ansicht2.Enabled = datenbankDa;
            Ansicht3.Enabled = datenbankDa;
            btnDatenbanken.Enabled = datenbankDa && !string.IsNullOrEmpty(TableView.Database.Filename);
            BeziehungsEditor.Enabled = false;
            if (datenbankDa) {
                if (TableView.Database.Column.Any(thisColumnItem => thisColumnItem != null && thisColumnItem.Format == enDataFormat.RelationText)) {
                    BeziehungsEditor.Enabled = true;
                }
            }
            cbxColumnArr.Enabled = datenbankDa && TableView.Design != enBlueTableAppearance.OnlyMainColumnWithoutHead;
            SpaltAnsichtCap.Enabled = datenbankDa && TableView.Design != enBlueTableAppearance.OnlyMainColumnWithoutHead;
            SuchenUndErsetzen.Enabled = datenbankDa && TableView.Design != enBlueTableAppearance.OnlyMainColumnWithoutHead;
            AngezeigteZeilenLöschen.Enabled = datenbankDa && TableView.Design != enBlueTableAppearance.OnlyMainColumnWithoutHead;
            Datenüberprüfung.Enabled = datenbankDa;
            btnSaveAs.Enabled = datenbankDa;
            btnDrucken.Item["csv"].Enabled = datenbankDa && TableView.Design != enBlueTableAppearance.OnlyMainColumnWithoutHead;
            btnDrucken.Item["html"].Enabled = datenbankDa && TableView.Design != enBlueTableAppearance.OnlyMainColumnWithoutHead;
            btnVorwärts.Enabled = datenbankDa;
            btnZurück.Enabled = datenbankDa;
            such.Enabled = datenbankDa;
            Filter.Enabled = datenbankDa && TableView.Design != enBlueTableAppearance.OnlyMainColumnWithoutHead;
        }

        private void DatabaseSet(string filename) {
            SetDatabasetoNothing();
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

        private void DatabaseSet(Database database) {
            TableView.Database = database;
            Formula.Database = database;
            Filter.Table = TableView;
            SelectStandardTab();
            if (TableView.Database == null) {
                SetDatabasetoNothing();
            } else {
                if (TableView.Database.Ansicht != enAnsicht.Unverändert) {
                    _ansicht = TableView.Database.Ansicht;
                }
            }
            InitView();
            CheckButtons();
            CaptionAnzeige();
            CheckButtons();
            if (TableView.View_RowFirst() != null) {
                TableView.CursorPos_Set(TableView.Database.Column[0], TableView.View_RowFirst(), false);
            }
        }

        private void Datenüberprüfung_Click(object sender, System.EventArgs e) => TableView.Database.Row.DoAutomatic(TableView.Filter, true, TableView.PinnedRows, "manual check");

        private void Drucken_ItemClicked(object sender, BasicListItemEventArgs e) {
            BlueBasics.MultiUserFile.MultiUserFile.SaveAll(false);
            switch (e.Item.Internal) {
                case "erweitert":
                    Visible = false;
                    List<RowItem?> selectedRows = new();
                    if (Ansicht1.Checked && Formula.ShowingRow != null) {
                        selectedRows.Add(Formula.ShowingRow);
                    } else {
                        selectedRows = TableView.VisibleUniqueRows();
                    }
                    using (ExportDialog l = new(TableView.Database, selectedRows)) {
                        l.ShowDialog();
                    }
                    Visible = true;
                    break;

                case "csv":
                    Generic.CopytoClipboard(TableView.Export_CSV(enFirstRow.ColumnCaption));
                    MessageBox.Show("Die gewünschten Daten<br>sind nun im Zwischenspeicher.", enImageCode.Clipboard, "Ok");
                    break;

                case "html":
                    TableView.Export_HTML();
                    break;

                default:
                    DebugPrint(e.Item);
                    break;
            }
        }

        private void InitView() {
            Formula.HideViewEditor();
            Ansicht0.Checked = _ansicht == enAnsicht.Nur_Tabelle;
            Ansicht1.Checked = _ansicht == enAnsicht.Überschriften_und_Formular;
            Ansicht2.Checked = _ansicht == enAnsicht.Tabelle_und_Formular_nebeneinander;
            Ansicht3.Checked = _ansicht == enAnsicht.Tabelle_und_Formular_übereinander;
            TableView?.Filter?.Clear();
            switch (_ansicht) {
                case enAnsicht.Nur_Tabelle:
                    grpFormularSteuerung.Visible = false;
                    Formula.Visible = false;
                    Formula.Dock = System.Windows.Forms.DockStyle.None;
                    TableView.Dock = System.Windows.Forms.DockStyle.None;
                    TableView.Design = enBlueTableAppearance.Standard;
                    Formula.Dock = System.Windows.Forms.DockStyle.None;
                    TableView.Dock = System.Windows.Forms.DockStyle.Fill;
                    break;

                case enAnsicht.Überschriften_und_Formular:
                    grpFormularSteuerung.Visible = true;
                    TableView.Design = enBlueTableAppearance.OnlyMainColumnWithoutHead;
                    Formula.Visible = true;
                    TableView.Dock = System.Windows.Forms.DockStyle.Left;
                    TableView.Width = 250;
                    Formula.BringToFront();
                    Formula.Dock = System.Windows.Forms.DockStyle.Fill;
                    break;

                case enAnsicht.Tabelle_und_Formular_nebeneinander:
                    grpFormularSteuerung.Visible = false;
                    TableView.Design = enBlueTableAppearance.Standard;
                    Formula.Visible = true;
                    TableView.Dock = System.Windows.Forms.DockStyle.None;
                    Formula.Dock = System.Windows.Forms.DockStyle.Right;
                    Formula.Width = (int)(MinimumSize.Width / 2.0);
                    TableView.BringToFront();
                    TableView.Dock = System.Windows.Forms.DockStyle.Fill;
                    break;

                case enAnsicht.Tabelle_und_Formular_übereinander:
                    grpFormularSteuerung.Visible = false;
                    TableView.Design = enBlueTableAppearance.Standard;
                    Formula.Visible = true;
                    TableView.Dock = System.Windows.Forms.DockStyle.None;
                    Formula.Dock = System.Windows.Forms.DockStyle.Top;
                    Formula.Height = (int)(MinimumSize.Height / 2.0);
                    TableView.BringToFront();
                    TableView.Dock = System.Windows.Forms.DockStyle.Fill;
                    break;

                default:
                    DebugPrint(_ansicht);
                    break;
            }
            if (TableView.Visible) {
                if (TableView.Database != null) {
                    if (TableView.CursorPosRow() == null && TableView.View_RowFirst() != null) {
                        TableView.CursorPos_Set(TableView.Database.Column[0], TableView.View_RowFirst(), false);
                    }
                    if (TableView.CursorPosRow() != null) { Formula.ShowingRowKey = TableView.CursorPosRow().Row.Key; }
                }
            } else {
                Formula.ShowingRowKey = -1;
            }
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
                if (TableView.Database.Row.Any(thisRow => thisRow != null && thisRow != vRow && string.Equals(thisRow.CellFirstString(), newName, StringComparison.CurrentCultureIgnoreCase))) {
                    istZ++;
                    newName = istName + " (" + istZ + ")";
                    changed = true;
                }
                if (!changed) { return newName; }
            } while (true);
        }

        private void Neu_Click(object sender, System.EventArgs e) {
            var r = TableView.Database.Column.First().SortType == enSortierTyp.Datum_Uhrzeit
                ? TableView.Database.Row.Add(NameRepair(DateTime.Now.ToString(Constants.Format_Date5), null))
                : TableView.Database.Row.Add(NameRepair("Neuer Eintrag", null));
            TableView.CursorPos_Set(TableView.Database.Column.First(), TableView.SortedRows().Get(r), true);
        }

        private void Ordn_Click(object sender, System.EventArgs e) {
            BlueBasics.MultiUserFile.MultiUserFile.SaveAll(false);
            SelectStandardTab();
            ExecuteFile(TableView.Database.Filename.FilePath());
        }

        private void SelectStandardTab() => MainRibbon.SelectedIndex = 1;

        private void SetDatabasetoNothing() {
            Formula.Database = null;
            TableView.Database = null;
        }

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
            Table.SearchNextText(suchtT, TableView, null, TableView.CursorPosRow(), out _, out var gefRow, true);
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
                    TableView.CursorPos_Set(TableView.Database.Column[0], gefRow, true);
                }
            }
        }

        private void SuchEintragNoSave(enDirection richtung, out ColumnItem? column, out RowData? row) {
            column = TableView.Database.Column[0];
            row = null;
            if (TableView.Database.Row.Count < 1) { return; }
            // Temporär berechnen, um geflacker zu vermeiden (Endabled - > Disabled bei Nothing)
            if (Convert.ToBoolean(richtung & enDirection.Unten)) {
                row = TableView.View_NextRow(TableView.CursorPosRow());
                if (row == null) { row = TableView.View_RowFirst(); }
            }
            if (Convert.ToBoolean(richtung & enDirection.Oben)) {
                row = TableView.View_PreviousRow(TableView.CursorPosRow());
                if (row == null) { row = TableView.View_RowLast(); }
            }
            if (row == null) { row = TableView.View_RowFirst(); }
        }

        private void SuchenUndErsetzen_Click(object sender, System.EventArgs e) => TableView.OpenSearchAndReplace();

        private void TableView_ColumnArrangementChanged(object sender, System.EventArgs e) => Table.WriteColumnArrangementsInto(cbxColumnArr, TableView.Database, TableView.Arrangement);

        private void TableView_ContextMenu_Init(object sender, ContextMenuInitEventArgs e) {
            var bt = (Table)sender;
            var cellKey = e.Tags.TagGet("Cellkey");
            if (string.IsNullOrEmpty(cellKey)) { return; }
            bt.Database.Cell.DataOfCellKey(cellKey, out var column, out var row);
            if (_ansicht != enAnsicht.Überschriften_und_Formular) {
                e.UserMenu.Add("Sortierung", true);
                e.UserMenu.Add(enContextMenuComands.SpaltenSortierungAZ, column != null && column.Format.CanBeCheckedByRules());
                e.UserMenu.Add(enContextMenuComands.SpaltenSortierungZA, column != null && column.Format.CanBeCheckedByRules());
                e.UserMenu.AddSeparator();
                e.UserMenu.Add("Zelle", true);
                e.UserMenu.Add("Inhalt Kopieren", "ContentCopy", enImageCode.Kopieren, column != null && column.Format.CanBeChangedByRules());
                e.UserMenu.Add("Inhalt Einfügen", "ContentPaste", enImageCode.Clipboard, column != null && column.Format.CanBeChangedByRules());
                e.UserMenu.Add("Inhalt löschen", "ContentDelete", enImageCode.Radiergummi, column != null && column.Format.CanBeChangedByRules());
                e.UserMenu.Add(enContextMenuComands.VorherigenInhaltWiederherstellen, column != null && column.Format.CanBeChangedByRules() && column.ShowUndo);
                e.UserMenu.Add(enContextMenuComands.SuchenUndErsetzen, column != null && column.Format.CanBeChangedByRules());
                e.UserMenu.AddSeparator();
                e.UserMenu.Add("Spalte", true);
                e.UserMenu.Add(enContextMenuComands.SpaltenEigenschaftenBearbeiten, column != null);
                e.UserMenu.Add("Statistik", "Statistik", QuickImage.Get(enImageCode.Balken, 16), column != null);
                e.UserMenu.Add("Inhalte aller angezeigten Zellen dieser Spalte löschen", "ColumnContentDelete", enImageCode.Radiergummi, column != null && column.Format.CanBeChangedByRules());
                e.UserMenu.Add("Summe", "Summe", enImageCode.Summe, column != null);
                e.UserMenu.AddSeparator();
            }
            e.UserMenu.Add("Zeile", true);
            e.UserMenu.Add(enContextMenuComands.ZeileLöschen, row != null);
            e.UserMenu.Add("Zeile prüfen", "Fehlersuche", enImageCode.Zeile, row != null);
        }

        private void TableView_CursorPosChanged(object sender, CellExtEventArgs e) {
            if (InvokeRequired) {
                Invoke(new Action(() => TableView_CursorPosChanged(sender, e)));
                return;
            }
            Formula.ShowingRowKey = e.Column == null || _ansicht == enAnsicht.Nur_Tabelle || e.RowData == null ? -1 : e.RowData.Row.Key;

            if (_ansicht == enAnsicht.Überschriften_und_Formular) {
                TableView.EnsureVisible(e.Column, e.RowData);
            }
        }

        private void TableView_EditBeforeBeginEdit(object sender, CellCancelEventArgs e) {
            if (TableView.Design == enBlueTableAppearance.OnlyMainColumnWithoutHead) {
                e.CancelReason = "In dieser Ansicht kann der Eintrag nicht bearbeitet werden.";
            }
        }

        private void TableView_RowsSorted(object sender, System.EventArgs e) {
            Zei.Text = TableView.Database.Column[0] != null ? "<ImageCode=Information|16>" + LanguageTool.DoTranslate("Einzigartige Zeilen:") + " " + TableView.Database.Row.VisibleRowCount + " St." : "-";
            CheckButtons();
        }

        private void TableView_ViewChanged(object sender, System.EventArgs e) => Table.WriteColumnArrangementsInto(cbxColumnArr, TableView.Database, TableView.Arrangement);

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
            TableView.CursorPos_Set(column, row, false);
        }

        private void zurück_Click(object sender, System.EventArgs e) {
            Formula.HideViewEditor();
            SuchEintragNoSave(enDirection.Oben, out var column, out var row);
            TableView.CursorPos_Set(column, row, false);
        }

        #endregion
    }
}