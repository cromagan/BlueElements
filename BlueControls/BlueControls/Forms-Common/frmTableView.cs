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
using static BlueBasics.Develop;
using static BlueBasics.FileOperations;

namespace BlueControls.Forms {

    public partial class frmTableView {

        #region Fields

        private const string _Version = "1.0001";

        private enAnsicht _Ansicht = enAnsicht.Nur_Tabelle;

        #endregion

        #region Constructors

        public frmTableView() : this(null, true, true) {
        }

        public frmTableView(Database Database) : this(Database, false, false) {
        }

        public frmTableView(Database database, bool loadTabVisible, bool adminTabVisible) {
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

        //private void btnTextLöschen_Click(object sender, System.EventArgs e)
        //{
        //    ZeilenFilter_TextFeld.Text = string.Empty;
        //}
        public List<RowItem> GetFilteredItems() {
            ShowDialog();
            return TableView.SortedRows();
        }

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
                Formula.ShowingRowKey = -1;
                TableView.Database.Row.Remove(TableView.Filter);
            }
        }

        public void TableView_ContextMenuItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
            var bt = (Table)sender;
            var CellKey = e.Tags.TagGet("CellKey");
            if (string.IsNullOrEmpty(CellKey)) { return; }
            bt.Database.Cell.DataOfCellKey(CellKey, out var Column, out var Row);
            switch (e.ClickedComand) {
                case "SpaltenSortierungAZ":
                    bt.SortDefinitionTemporary = new RowSortDefinition(bt.Database, Column.Name, false);
                    break;

                case "SpaltenSortierungZA":
                    bt.SortDefinitionTemporary = new RowSortDefinition(bt.Database, Column.Name, true);
                    break;

                case "Fehlersuche":
                    MessageBox.Show(Row.DoAutomatic(true, true, 10, "manual check").error);
                    break;

                case "ZeileLöschen":
                    if (Row != null) {
                        if (MessageBox.Show("Zeile löschen?", enImageCode.Frage, "Ja", "Nein") == 0) {
                            bt.Database.Row.Remove(Row);
                        }
                    }
                    break;

                case "ContentDelete":
                    TableView.Database.Cell.Delete(Column, Row.Key);
                    break;

                case "SpaltenEigenschaftenBearbeiten":
                    tabAdministration.OpenColumnEditor(Column, TableView);
                    CheckButtons();
                    break;

                case "ContentCopy":
                    Table.CopyToClipboard(Column, Row, true);
                    break;

                case "SuchenUndErsetzen":
                    TableView.OpenSearchAndReplace();
                    break;

                case "ÜberallDel":
                    TableView.OpenSearchAndReplace();
                    break;

                case "Summe":
                    var summe = Column.Summe(TableView.Filter);
                    if (!summe.HasValue) {
                        MessageBox.Show("Die Summe konnte nicht berechnet werden.", enImageCode.Summe, "OK");
                    } else {
                        MessageBox.Show("Summe dieser Spalte, nur angezeigte Zeilen: <br><b>" + summe, enImageCode.Summe, "OK");
                    }
                    break;

                case "VorherigenInhaltWiederherstellen":
                    Table.DoUndo(Column, Row);
                    break;

                case "ContentPaste":
                    Row.CellSet(Column, System.Windows.Forms.Clipboard.GetText());
                    break;

                case "ColumnContentDelete":
                    if (Column != null) {
                        if (MessageBox.Show("Angezeite Inhalte dieser Spalte löschen?", enImageCode.Frage, "Ja", "Nein") == 0) {
                            Column.DeleteContents(TableView.Filter, TableView.PinnedRows);
                        }
                    }
                    break;
            }
        }

        protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e) {
            SetDatabasetoNothing();
            BlueBasics.MultiUserFile.clsMultiUserFile.SaveAll(true);
            base.OnFormClosing(e);
        }

        //private void ZeilenFilter_TextFeld_TextChanged(object sender, System.EventArgs e)
        //{
        //    if (grpFilter.Visible == false) { return; }
        //    var NeuerT = ZeilenFilter_TextFeld.Text.TrimStart();
        //    NeuerT = NeuerT.TrimStart('+');
        //    NeuerT = NeuerT.Replace("++", "+");
        //    if (NeuerT == "+") { NeuerT = string.Empty; }
        //    if (NeuerT != ZeilenFilter_TextFeld.Text)
        //    {
        //        ZeilenFilter_TextFeld.Text = NeuerT;
        //        return;
        //    }
        //    Filter_ZeilenFilterSetzen();
        //}
        //private void ZeilenFilter_TextFeld_Enter(object sender, System.EventArgs e)
        //{
        //    Filter_ZeilenFilterSetzen();
        //}
        //private void Filter_ZeilenFilterSetzen()
        //{
        //    if (TableView.Database != null) { TableView.Filter.Remove_RowFilter(); }
        //    if (TableView.Database != null && !string.IsNullOrEmpty(ZeilenFilter_TextFeld.Text))
        //    {
        //        TableView.Filter.Add(enFilterType.Instr_UND_GroßKleinEgal, new List<string>(ZeilenFilter_TextFeld.Text.SplitBy("+")));
        //    }
        //}
        //private void AlleFilterAus_Click(object sender, System.EventArgs e)
        //{
        //    ZeilenFilter_TextFeld.Text = string.Empty;
        //    if (TableView.Filter != null)
        //    {
        //        TableView.Filter.Clear();
        //    }
        //}
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
            PadEditor r = new() {
                WindowState = System.Windows.Forms.FormWindowState.Maximized
            };
            r.ShowDialog();
            Show();
        }

        private void AngezeigteZeilenLöschen_Click(object sender, System.EventArgs e) {
            TableView.Database.Row.Remove(TableView.Filter);
            CheckButtons();
        }

        private void Ansicht_Click(object sender, System.EventArgs e) {
            _Ansicht = (enAnsicht)int.Parse(((Button)sender).Name.Substring(7, 1));
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
            BlueBasics.MultiUserFile.clsMultiUserFile.SaveAll(false);
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
            StandardTabx();
            if (bu.Name == "btnNeuDB") {
                DatabaseSet(new Database(false)); // Ab jetzt in der Variable _Database zu finden
            }
            if (FileExists(SaveTab.FileName)) { DeleteFile(SaveTab.FileName, true); }
            TableView.Database.SaveAsAndChangeTo(SaveTab.FileName);
            DatabaseSet(SaveTab.FileName);
        }

        private void btnOeffnen_Click(object sender, System.EventArgs e) {
            BlueBasics.MultiUserFile.clsMultiUserFile.SaveAll(false);
            LoadTab.ShowDialog();
        }

        private void CaptionAnzeige() {
            if (TableView.Database == null) {
                Text = "Be Creative! V" + _Version;
                return;
            }
            Text = TableView.Database != null
                ? TableView.Database.Filename.FileNameWithSuffix() + " - Be Creative! V" + _Version
                : "[Neue Datenbank] - Be Creative! V" + _Version;
        }

        private void cbxColumnArr_ItemClicked(object sender, BasicListItemEventArgs e) {
            if (string.IsNullOrEmpty(cbxColumnArr.Text)) { return; }
            TableView.Arrangement = int.Parse(e.Item.Internal);
        }

        private void Check_SuchButton() => SuchB.Enabled = TableView.Database != null && TableView.Database.Row.Count >= 1
&& !string.IsNullOrEmpty(such.Text) && !string.IsNullOrEmpty(such.Text.RemoveChars(" "));

        private void CheckButtons() {
            var DatenbankDa = Convert.ToBoolean(TableView.Database != null);
            btnNeuDB.Enabled = true;
            btnOeffnen.Enabled = true;
            btnNeu.Enabled = DatenbankDa && _Ansicht == enAnsicht.Überschriften_und_Formular && TableView.Database.PermissionCheck(TableView.Database.PermissionGroups_NewRow, null);
            btnLoeschen.Enabled = DatenbankDa;
            btnDrucken.Enabled = DatenbankDa;
            Ansicht0.Enabled = DatenbankDa;
            Ansicht1.Enabled = DatenbankDa;
            Ansicht2.Enabled = DatenbankDa;
            Ansicht3.Enabled = DatenbankDa;
            btnDatenbanken.Enabled = DatenbankDa && !string.IsNullOrEmpty(TableView.Database.Filename);
            BeziehungsEditor.Enabled = false;
            if (DatenbankDa) {
                foreach (var ThisColumnItem in TableView.Database.Column) {
                    if (ThisColumnItem != null) {
                        if (ThisColumnItem.Format == enDataFormat.RelationText) {
                            BeziehungsEditor.Enabled = true;
                            break;
                        }
                    }
                }
            }
            cbxColumnArr.Enabled = DatenbankDa && TableView.Design != enBlueTableAppearance.OnlyMainColumnWithoutHead;
            SpaltAnsichtCap.Enabled = DatenbankDa && TableView.Design != enBlueTableAppearance.OnlyMainColumnWithoutHead;
            SuchenUndErsetzen.Enabled = DatenbankDa && TableView.Design != enBlueTableAppearance.OnlyMainColumnWithoutHead;
            AngezeigteZeilenLöschen.Enabled = DatenbankDa && TableView.Design != enBlueTableAppearance.OnlyMainColumnWithoutHead;
            Datenüberprüfung.Enabled = DatenbankDa;
            btnSaveAs.Enabled = DatenbankDa;
            btnDrucken.Item["csv"].Enabled = DatenbankDa && TableView.Design != enBlueTableAppearance.OnlyMainColumnWithoutHead;
            btnDrucken.Item["html"].Enabled = DatenbankDa && TableView.Design != enBlueTableAppearance.OnlyMainColumnWithoutHead;
            Vorwärts.Enabled = DatenbankDa;
            zurück.Enabled = DatenbankDa;
            such.Enabled = DatenbankDa;
            Filter.Enabled = DatenbankDa && TableView.Design != enBlueTableAppearance.OnlyMainColumnWithoutHead;
        }

        private void DatabaseSet(string Datei) {
            SetDatabasetoNothing();
            if (!FileExists(Datei)) {
                CheckButtons();
                return;
            }
            btnLetzteDateien.AddFileName(Datei, string.Empty);
            LoadTab.FileName = Datei;
            var tmpDatabase = (Database)BlueBasics.MultiUserFile.clsMultiUserFile.GetByFilename(Datei, false);
            if (tmpDatabase == null) {
                tmpDatabase = new Database(Datei, false, false);
            }
            if (tmpDatabase == null) { return; }
            DatabaseSet(tmpDatabase);
        }

        private void DatabaseSet(Database cDatabase) {
            TableView.Database = cDatabase;
            Formula.Database = cDatabase;
            Filter.Table = TableView;
            StandardTabx();
            if (TableView.Database == null) {
                SetDatabasetoNothing();
            } else {
                if (TableView.Database.Ansicht != enAnsicht.Unverändert) {
                    _Ansicht = TableView.Database.Ansicht;
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
            BlueBasics.MultiUserFile.clsMultiUserFile.SaveAll(false);
            switch (e.Item.Internal) {
                case "erweitert":
                    Visible = false;
                    TableView.Visible = false;
                    List<RowItem> Ara = new();
                    if (Formula.ShowingRow != null) {
                        Ara.Add(Formula.ShowingRow);
                    } else {
                        Ara = TableView.SortedRows(); //Database.Column().Liste_SingleRow(0, enListenOptionen.MitFilter_Sortiert_Unique)
                    }
                    using (ExportDialog l = new(TableView.Database, Ara)) {
                        l.ShowDialog();
                    }
                    TableView.Visible = true;
                    Visible = true;
                    break;

                case "csv":
                    System.Windows.Clipboard.SetText(TableView.Export_CSV(enFirstRow.ColumnCaption));
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
            Ansicht0.Checked = _Ansicht == enAnsicht.Nur_Tabelle;
            Ansicht1.Checked = _Ansicht == enAnsicht.Überschriften_und_Formular;
            Ansicht2.Checked = _Ansicht == enAnsicht.Tabelle_und_Formular_nebeneinander;
            Ansicht3.Checked = _Ansicht == enAnsicht.Tabelle_und_Formular_übereinander;
            TableView?.Filter?.Clear();
            switch (_Ansicht) {
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
                    DebugPrint(_Ansicht);
                    break;
            }
            if (TableView.Visible) {
                if (TableView.Database != null) {
                    if (TableView.CursorPosRow() == null && TableView.View_RowFirst() != null) {
                        TableView.CursorPos_Set(TableView.Database.Column[0], TableView.View_RowFirst(), false);
                    }
                    if (TableView.CursorPosRow() != null) { Formula.ShowingRowKey = TableView.CursorPosRow().Key; }
                }
            } else {
                Formula.ShowingRowKey = -1;
            }
        }

        private void LastDatabases_ItemClicked(object sender, BasicListItemEventArgs e) {
            BlueBasics.MultiUserFile.clsMultiUserFile.SaveAll(false);
            DatabaseSet(e.Item.Internal);
        }

        private void LoadTab_FileOk(object sender, CancelEventArgs e) => DatabaseSet(LoadTab.FileName);

        private string NameRepair(string IstName, RowItem vRow) {
            var NewName = IstName;
            var IstZ = 0;
            do {
                var Changed = false;
                foreach (var ThisRow in TableView.Database.Row) {
                    if (ThisRow != null && ThisRow != vRow) {
                        if (ThisRow.CellFirstString().ToUpper() == NewName.ToUpper()) {
                            IstZ++;
                            NewName = IstName + " (" + IstZ + ")";
                            Changed = true;
                            break;
                        }
                    }
                }
                if (!Changed) { return NewName; }
            } while (true);
        }

        private void Neu_Click(object sender, System.EventArgs e) {
            var vRow = TableView.Database.Column[0].Format switch {
                enDataFormat.Datum_und_Uhrzeit => TableView.Database.Row.Add(NameRepair(DateTime.Now.ToString(Constants.Format_Date5), null)),
                _ => TableView.Database.Row.Add(NameRepair("Neuer Eintrag", null)),
            };
            TableView.CursorPos_Set(TableView.Database.Column[0], vRow, true);
        }

        private void Ordn_Click(object sender, System.EventArgs e) {
            BlueBasics.MultiUserFile.clsMultiUserFile.SaveAll(false);
            StandardTabx();
            ExecuteFile(TableView.Database.Filename.FilePath());
        }

        private void SetDatabasetoNothing() {
            Formula.Database = null;
            TableView.Database = null;
        }

        private void StandardTabx() => MainRibbon.SelectedIndex = 1;

        private void such_Enter(object sender, System.EventArgs e) {
            if (SuchB.Enabled) { SuchB_Click(SuchB, null); }
        }

        private void such_TextChanged(object sender, System.EventArgs e) => Check_SuchButton();

        private void SuchB_Click(object sender, System.EventArgs e) {
            Formula.HideViewEditor();
            var SuchtT = such.Text.Trim();
            if (string.IsNullOrEmpty(SuchtT)) {
                MessageBox.Show("Bitte Text zum Suchen eingeben.", enImageCode.Information, "OK");
                return;
            }
            Table.SearchNextText(SuchtT, TableView, null, Formula.ShowingRow, out _, out var GefRow, true);
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
            if (GefRow == null) {
                MessageBox.Show("Kein Eintrag gefunden!", enImageCode.Information, "OK");
            } else {
                if (GefRow == Formula.ShowingRow) {
                    MessageBox.Show("Text nur im <b>aktuellen Eintrag</b> gefunden,<br>aber sonst keine weiteren Einträge!", enImageCode.Information, "OK");
                } else {
                    TableView.CursorPos_Set(TableView.Database.Column[0], GefRow, true);
                }
            }
        }

        private void SuchEintragNoSave(enDirection Richtung, out ColumnItem column, out RowItem row) {
            column = TableView.Database.Column[0];
            row = null;
            if (TableView.Database.Row.Count < 1) { return; }
            // Temporär berechnen, um geflacker zu vermeiden (Endabled - > Disabled bei Nothing)
            if (Convert.ToBoolean(Richtung & enDirection.Unten)) {
                row = TableView.View_NextRow(Formula.ShowingRow);
                if (row == null) { row = TableView.View_RowFirst(); }
            }
            if (Convert.ToBoolean(Richtung & enDirection.Oben)) {
                row = TableView.View_PreviousRow(Formula.ShowingRow);
                if (row == null) { row = TableView.View_RowLast(); }
            }
            if (row == null) { row = TableView.View_RowFirst(); }
        }

        private void SuchenUndErsetzen_Click(object sender, System.EventArgs e) => TableView.OpenSearchAndReplace();

        private void TableView_ColumnArrangementChanged(object sender, System.EventArgs e) => TableView.WriteColumnArrangementsInto(cbxColumnArr);

        private void TableView_ContextMenu_Init(object sender, ContextMenuInitEventArgs e) {
            var bt = (Table)sender;
            var CellKey = e.Tags.TagGet("Cellkey");
            if (string.IsNullOrEmpty(CellKey)) { return; }
            bt.Database.Cell.DataOfCellKey(CellKey, out var Column, out var Row);
            if (_Ansicht != enAnsicht.Überschriften_und_Formular) {
                e.UserMenu.Add("Sortierung", true);
                e.UserMenu.Add(enContextMenuComands.SpaltenSortierungAZ, Column != null && Column.Format.CanBeCheckedByRules());
                e.UserMenu.Add(enContextMenuComands.SpaltenSortierungZA, Column != null && Column.Format.CanBeCheckedByRules());
                e.UserMenu.AddSeparator();
                e.UserMenu.Add("Zelle", true);
                e.UserMenu.Add("Inhalt Kopieren", "ContentCopy", enImageCode.Kopieren, Column != null && Column.Format.CanBeChangedByRules());
                e.UserMenu.Add("Inhalt Einfügen", "ContentPaste", enImageCode.Clipboard, Column != null && Column.Format.CanBeChangedByRules());
                e.UserMenu.Add("Inhalt löschen", "ContentDelete", enImageCode.Radiergummi, Column != null && Column.Format.CanBeChangedByRules());
                e.UserMenu.Add(enContextMenuComands.VorherigenInhaltWiederherstellen, Column != null && Column.Format.CanBeChangedByRules() && Column.ShowUndo);
                e.UserMenu.Add(enContextMenuComands.SuchenUndErsetzen, Column != null && Column.Format.CanBeChangedByRules());
                e.UserMenu.AddSeparator();
                e.UserMenu.Add("Spalte", true);
                e.UserMenu.Add(enContextMenuComands.SpaltenEigenschaftenBearbeiten, Column != null);
                e.UserMenu.Add("Inhalte aller angezeigten Zellen dieser Spalte löschen", "ColumnContentDelete", enImageCode.Radiergummi, Column != null && Column.Format.CanBeChangedByRules());
                e.UserMenu.Add("Summe", "Summe", enImageCode.Summe, Column != null);
                e.UserMenu.AddSeparator();
            }
            e.UserMenu.Add("Zeile", true);
            e.UserMenu.Add(enContextMenuComands.ZeileLöschen, Row != null);
            e.UserMenu.Add("Fehler anzeigen", "Fehlersuche", enImageCode.Kritisch, Row != null);
        }

        private void TableView_CursorPosChanged(object sender, CellEventArgs e) {
            if (InvokeRequired) {
                Invoke(new Action(() => TableView_CursorPosChanged(sender, e)));
                return;
            }
            Formula.ShowingRowKey = e.Column == null || _Ansicht == enAnsicht.Nur_Tabelle || e.Row == null ? -1 : e.Row.Key;
            if (_Ansicht == enAnsicht.Überschriften_und_Formular) {
                TableView.EnsureVisible(e.Column, e.Row);
            }
        }

        private void TableView_EditBeforeBeginEdit(object sender, CellCancelEventArgs e) {
            if (TableView.Design == enBlueTableAppearance.OnlyMainColumnWithoutHead) {
                e.CancelReason = "In dieser Ansicht kann der Eintrag nicht bearbeitet werden.";
            }
        }

        private void TableView_RowsSorted(object sender, System.EventArgs e) {
            Zei.Text = TableView.Database.Column[0] != null ? "<ImageCode=Information|16>Zeilen: " + TableView.SortedRows().Count + " St." : "-";
            CheckButtons();
        }

        private void TableView_ViewChanged(object sender, System.EventArgs e) => TableView.WriteColumnArrangementsInto(cbxColumnArr);

        private void TemporärenSpeicherortÖffnen_Click(object sender, System.EventArgs e) {
            BlueBasics.MultiUserFile.clsMultiUserFile.SaveAll(false);
            StandardTabx();
            ExecuteFile(Path.GetTempPath());
        }

        private void ÜberDiesesProgramm_Click(object sender, System.EventArgs e) {
            BlueBasics.MultiUserFile.clsMultiUserFile.SaveAll(false);
            MessageBox.Show("(c) Christian Peter<br>V " + _Version, enImageCode.Information, "OK");
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