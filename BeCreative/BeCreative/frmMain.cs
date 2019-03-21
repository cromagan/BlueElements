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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls;
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Controls;
using BlueControls.DialogBoxes;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.ItemCollection;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using static BlueBasics.Develop;
using static BlueBasics.FileOperations;
using static BlueBasics.modAllgemein;
using static BlueControls.clsSkin;

namespace BeCreative
{
    public partial class frmMain
    {
        public frmMain()
        {
            InitializeComponent();
            tabAdmin.Table = TableView;
        }

        private Database _Database;
        private enAnsicht _Ansicht = enAnsicht.Nur_Tabelle;
        private const string _Version = "1.0001";


        private void SetDatabasetoNothing()
        {
            BlueFormulax.Database = null;
            TableView.Database = null;

            if (_Database != null)
            {
                _Database.Release(false);
                _Database = null;
            }
        }


        private void zurück_Click(object sender, EventArgs e)
        {
            BlueFormulax.HideViewEditor();
            TableView.CursorPos_Set(SuchEintragNoSave(enDirection.Oben));
        }

        private void vor_Click(object sender, EventArgs e)
        {
            BlueFormulax.HideViewEditor();
            TableView.CursorPos_Set(SuchEintragNoSave(enDirection.Unten));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Richtung"></param>
        /// <returns>Den CellKey</returns>
        private string SuchEintragNoSave(enDirection Richtung)
        {

            if (TableView.Database.Row.Count() < 1) { return null; }


            // Temporär berechnen, um geflacker zu vermeiden (Endabled - > Disabled bei Nothing)
            RowItem nr = null;

            if (Convert.ToBoolean(Richtung & enDirection.Unten))
            {
                nr = TableView.View_NextRow(BlueFormulax.ShowingRow);
                if (nr == null) { nr = TableView.View_RowFirst(); }
            }

            if (Convert.ToBoolean(Richtung & enDirection.Oben))
            {
                nr = TableView.View_PreviousRow(BlueFormulax.ShowingRow);
                if (nr == null) { nr = TableView.View_RowLast(); }
            }

            if (nr == null) { nr = TableView.View_RowFirst(); }

            return CellCollection.KeyOfCell(TableView.Database.Column[0].Key, nr.Key);
        }


        private void TableView_CursorPosChanged(object sender, CellEventArgs e)
        {

            if (e.Column == null || _Ansicht == enAnsicht.Nur_Tabelle || e.Row == null)
            {
                BlueFormulax.ShowingRowKey = -1;
            }
            else
            {
                BlueFormulax.ShowingRowKey = e.Row.Key;
            }


            if (_Ansicht == enAnsicht.Überschriften_und_Formular)
            {
                TableView.EnsureVisible(e.Column, e.Row);
            }

        }


        private void TableView_EditBeforeBeginEdit(object sender, CellCancelEventArgs e)
        {
            if (TableView.Design == enBlueTableAppearance.OnlyMainColumnWithoutHead)
            {
                e.CancelReason = "In dieser Ansicht kann der Eintrag nicht bearbeitet werden.";
            }
        }

        private void such_Enter(object sender, EventArgs e)
        {
            if (SuchB.Enabled) { SuchB_Click(SuchB, null); }
        }

        private void such_TextChanged(object sender, EventArgs e)
        {
            Check_SuchButton();
        }

        private void SuchB_Click(object sender, EventArgs e)
        {
            BlueFormulax.HideViewEditor();
            var SuchtT = such.Text.Trim();

            if (string.IsNullOrEmpty(SuchtT))
            {
                MessageBox.Show("Bitte Text zum Suchen eingeben.", enImageCode.Information, "OK");
                return;
            }



            Table.SearchNextText(SuchtT, TableView, null, BlueFormulax.ShowingRow, out var found, out var GefRow, true);


            //var CheckRow = BlueFormulax.ShowingRow;
            //RowItem GefRow = null;


            //if (CheckRow == null) { CheckRow = TableView.View_RowFirst(); }

            //var Count = 0;




            //do
            //{

            //    if (Count > TableView.Database.Row.Count() + 1) { break; }
            //    if (GefRow != null && GefRow != BlueFormulax.ShowingRow) { break; }

            //    Count += 1;
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


            if (GefRow == null)
            {
                MessageBox.Show("Kein Eintrag gefunden!", enImageCode.Information, "OK");
            }
            else
            {
                if (GefRow == BlueFormulax.ShowingRow)
                {
                    MessageBox.Show("Text nur im <b>aktuellen Eintrag</b> gefunden,<br>aber sonst keine weiteren Einträge!", enImageCode.Information, "OK");
                }
                else
                {
                    TableView.CursorPos_Set(_Database.Column[0], GefRow, true);
                }
            }
        }


        private void Neu_Click(object sender, EventArgs e)
        {
            RowItem vRow = null;


            switch (_Database.Column[0].Format)
            {
                case enDataFormat.Datum_und_Uhrzeit:
                    vRow = _Database.Row.Add(NameRepair(DateTime.Now.ToString(), null));
                    break;
                default:
                    vRow = _Database.Row.Add(NameRepair("Neuer Eintrag", null));
                    break;
            }


            TableView.CursorPos_Set(_Database.Column[0], vRow, true);
        }

        private string NameRepair(string IstName, RowItem vRow)
        {
            var NewName = IstName;
            var IstZ = 0;

            do
            {
                var Changed = false;

                foreach (var ThisRow in _Database.Row)
                {
                    if (ThisRow != null && ThisRow != vRow)
                    {
                        if (ThisRow.CellFirstString().ToUpper() == NewName.ToUpper())
                        {
                            IstZ += 1;
                            NewName = IstName + " (" + IstZ + ")";
                            Changed = true;
                            break;
                        }

                    }
                }

                if (!Changed) { return NewName; }
            } while (true);

        }


        public void LöscheZeile(object sender, EventArgs e)
        {

            BlueFormulax.HideViewEditor();

            if (Ansicht1.Checked)
            {
                if (BlueFormulax.ShowingRow == null)
                {
                    MessageBox.Show("Kein Eintrag gewählt.", enImageCode.Information, "OK");
                    return;
                }


                var tmpr = BlueFormulax.ShowingRow;
                if (MessageBox.Show("Soll der Eintrag<br><b>" + tmpr.CellFirstString() + "</b><br>wirklich <b>gelöscht</b> werden?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }

                var vCell = SuchEintragNoSave(enDirection.Unten);
                TableView.CursorPos_Set(vCell);
                TableView.Database.Row.Remove(tmpr);
            }
            else
            {
                BlueFormulax.ShowingRowKey = -1;
                TableView.Database.Row.Remove(TableView.Filter);
            }
        }



        private void Check_SuchButton()
        {

            if (_Database == null || _Database.Row.Count() < 1)
            {
                SuchB.Enabled = false;
            }
            else
            {
                if (!string.IsNullOrEmpty(such.Text) && !string.IsNullOrEmpty(such.Text.RemoveChars(" ")))
                {
                    SuchB.Enabled = true;
                }
                else
                {
                    SuchB.Enabled = false;
                }

            }
        }


        private void DatabaseSet(string Datei)
        {

            SetDatabasetoNothing();

            if (!FileExists(Datei))
            {
                CheckButtons();
                return;
            }


            LastDatabases.AddFileName(Datei);
            LoadTab.FileName = Datei;


            Database tmpDatabase = null;

            tmpDatabase = Database.Load(Datei, false, Table.Database_NeedPassword, CreativePad.GenerateLayoutFromRow, CreativePad.RenameColumnInLayout);

            if (tmpDatabase == null) { return; }

            DatabaseSet(tmpDatabase);
        }


        public void DatabaseSet(Database cDatabase)
        {
            _Database = cDatabase;
            TableView.Database = cDatabase;
            BlueFormulax.Database = cDatabase;


            StandardTabx();


            SuspendLayout();

            if (_Database.Skin != (int)enSkin.Unverändert && (int)GenericControl.Skin.Skin != _Database.Skin)
            {
                GenericControl.Skin.Skin = (enSkin)_Database.Skin;
                Refresh();
            }

            if (_Database.Ansicht != enAnsicht.Unverändert)
            {
                _Ansicht = _Database.Ansicht;
            }
            InitView();
            CheckButtons();

            CaptionAnzeige();
            CheckButtons();

            if (TableView.View_RowFirst() != null)
            {
                TableView.CursorPos_Set(TableView.Database.Column[0], TableView.View_RowFirst(), false);
            }

            ResumeLayout();
        }





        private void ZeilenFilter_TextFeld_TextChanged(object sender, EventArgs e)
        {
            if (TableViewBar.Visible == false) { return; }

            var NeuerT = ZeilenFilter_TextFeld.Text.TrimStart();


            NeuerT = NeuerT.TrimStart('+');
            NeuerT = NeuerT.Replace("++", "+");
            if (NeuerT == "+") { NeuerT = string.Empty; }



            if (NeuerT != ZeilenFilter_TextFeld.Text)
            {
                ZeilenFilter_TextFeld.Text = NeuerT;
                return;
            }


            Filter_ZeilenFilterSetzen();
        }

        private void ZeilenFilter_TextFeld_Enter(object sender, EventArgs e)
        {
            Filter_ZeilenFilterSetzen();
        }


        private void Filter_ZeilenFilterSetzen()
        {
            if (TableView.Database != null) { TableView.Filter.Delete_RowFilter(); }

            if (TableView.Database != null && !string.IsNullOrEmpty(ZeilenFilter_TextFeld.Text))
            {
                TableView.Filter.Add(enFilterType.Instr_UND_GroßKleinEgal, new List<string>(ZeilenFilter_TextFeld.Text.SplitBy("+")));
            }

        }

        private void AlleFilterAus_Click(object sender, EventArgs e)
        {
            ZeilenFilter_TextFeld.Text = string.Empty;
        }



        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Copyright.Text = "(c) 2010-" + DateTime.Now.Year + " Christian Peter";

            Drucken.Item.Clear();
            Drucken.Item.Add(new TextListItem("erweitert", "Drucken bzw. Export", QuickImage.Get(enImageCode.Drucker, 28)));
            Drucken.Item.Add(new LineListItem());
            Drucken.Item.Add(new TextListItem("csv", "CSV-Format für Excel in die Zwischenablage", QuickImage.Get(enImageCode.Excel, 28)));
            Drucken.Item.Add(new TextListItem("html", "HTML-Format für Internet-Seiten", QuickImage.Get(enImageCode.Globus, 28)));
            Drucken.Item.Add(new LineListItem());
            Drucken.Item.Add(new TextListItem("editor", "Layout-Editor öffnen", QuickImage.Get(enImageCode.Layout, 28)));

            CheckButtons();
            CaptionAnzeige();

        }


        private void Drucken_Item_Click(object sender, BasicListItemEventArgs e)
        {


            switch (e.Item.Internal())
            {

                case "erweitert":
                    Visible = false;

                    TableView.Visible = false;

                    var Ara = new List<RowItem>();


                    if (BlueFormulax.ShowingRow != null)
                    {
                        Ara.Add(BlueFormulax.ShowingRow);
                    }
                    else
                    {
                        Ara = TableView.SortedRows(); //Database.Column().Liste_SingleRow(0, enListenOptionen.MitFilter_Sortiert_Unique)
                    }


                    using (var l = new ExportDialog(_Database, Ara, true))
                    {
                        l.ShowDialog();
                    }

                    TableView.Visible = true;
                    Visible = true;
                    break;

                case "csv":
                    System.Windows.Forms.Clipboard.SetDataObject(TableView.Export_CSV(enFirstRow.ColumnCaption), true);
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

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            InitView();
        }




        private void InitView()
        {

            BlueFormulax.HideViewEditor();


            Ansicht0.Checked = _Ansicht == enAnsicht.Nur_Tabelle;
            Ansicht1.Checked = _Ansicht == enAnsicht.Überschriften_und_Formular;
            Ansicht2.Checked = _Ansicht == enAnsicht.Tabelle_und_Formular_nebeneinander;
            Ansicht3.Checked = _Ansicht == enAnsicht.Tabelle_und_Formular_übereinander;

            AlleFilterAus_Click(null, null);


            switch (_Ansicht)
            {
                case enAnsicht.Nur_Tabelle:
                    FormulaViewBar.Visible = false;
                    TableViewBar.Visible = true;

                    BlueFormulax.Visible = false;

                    BlueFormulax.Dock = System.Windows.Forms.DockStyle.None;
                    TableView.Dock = System.Windows.Forms.DockStyle.None;


                    TableView.Design = enBlueTableAppearance.Standard;


                    BlueFormulax.Dock = System.Windows.Forms.DockStyle.None;
                    TableView.Dock = System.Windows.Forms.DockStyle.Fill;
                    break;

                case enAnsicht.Überschriften_und_Formular:
                    FormulaViewBar.Visible = true;
                    TableViewBar.Visible = false;
                    TableView.Design = enBlueTableAppearance.OnlyMainColumnWithoutHead;
                    BlueFormulax.Visible = true;

                    TableView.Dock = System.Windows.Forms.DockStyle.Left;
                    TableView.Width = 250;

                    BlueFormulax.BringToFront();
                    BlueFormulax.Dock = System.Windows.Forms.DockStyle.Fill;
                    break;

                case enAnsicht.Tabelle_und_Formular_nebeneinander:
                    FormulaViewBar.Visible = false;
                    TableViewBar.Visible = true;
                    TableView.Design = enBlueTableAppearance.Standard;
                    BlueFormulax.Visible = true;

                    TableView.Dock = System.Windows.Forms.DockStyle.None;

                    BlueFormulax.Dock = System.Windows.Forms.DockStyle.Right;
                    BlueFormulax.Width = Convert.ToInt32(MinimumSize.Width / 2.0);

                    TableView.BringToFront();
                    TableView.Dock = System.Windows.Forms.DockStyle.Fill;
                    break;

                case enAnsicht.Tabelle_und_Formular_übereinander:
                    FormulaViewBar.Visible = false;
                    TableViewBar.Visible = true;
                    TableView.Design = enBlueTableAppearance.Standard;
                    BlueFormulax.Visible = true;

                    TableView.Dock = System.Windows.Forms.DockStyle.None;

                    BlueFormulax.Dock = System.Windows.Forms.DockStyle.Top;
                    BlueFormulax.Height = Convert.ToInt32(MinimumSize.Height / 2.0);

                    TableView.BringToFront();
                    TableView.Dock = System.Windows.Forms.DockStyle.Fill;
                    break;

                default:
                    DebugPrint(_Ansicht);
                    break;
            }


            if (TableView.Visible)
            {
                if (TableView.Database != null)
                {
                    if (string.IsNullOrEmpty(TableView.CursorPosKey()) && TableView.View_RowFirst() != null)
                    {
                        TableView.CursorPos_Set(TableView.Database.Column[0], TableView.View_RowFirst(), false);

                    }
                    if (TableView.CursorPosRow() != null) { BlueFormulax.ShowingRowKey = TableView.CursorPosRow().Key; }
                }
            }
            else
            {
                BlueFormulax.ShowingRowKey = -1;
            }
        }


        private void LoadTab_FileOk(object sender, CancelEventArgs e)
        {
            DatabaseSet(LoadTab.FileName);
        }


        private void NeuDBSaveAs_Click(object sender, EventArgs e)
        {

            var bu = (Button)sender;

            switch (bu.Name)
            {
                case "SaveAs":
                    if (_Database == null) { return; }
                    break;
                case "NeuDB":
                    if (_Database != null) { SetDatabasetoNothing(); }
                    break;
                default:
                    DebugPrint(enFehlerArt.Fehler, "Ungültiger Aufruf!");
                    break;
            }


            SaveTab.ShowDialog();

            if (!PathExists(SaveTab.FileName.FilePath())) { return; }
            if (string.IsNullOrEmpty(SaveTab.FileName)) { return; }

            StandardTabx();

            if (bu.Name == "NeuDB")
            {
                DatabaseSet(new Database(false, Table.Database_NeedPassword, CreativePad.GenerateLayoutFromRow, CreativePad.RenameColumnInLayout)); // Ab jetzt in der Variable _Database zu finden
            }

            if (FileExists(SaveTab.FileName)) { DeleteFile(SaveTab.FileName, true); }
            _Database.SaveAsAndChangeTo(SaveTab.FileName);
            DatabaseSet(SaveTab.FileName);

        }

        private void CaptionAnzeige()
        {

            if (_Database == null)
            {
                Text = "Be Creative! V" + _Version;
                return;
            }


            if (_Database != null)
            {
                Text = _Database.Filename.FileNameWithSuffix() + " - Be Creative! V" + _Version;
            }
            else
            {
                Text = "[Neue Datenbank] - Be Creative! V" + _Version;
            }
        }


        private void CheckButtons()
        {
            var DatenbankDa = Convert.ToBoolean(_Database != null);


            NeuDB.Enabled = true;

            Öffnen.Enabled = true;


            Neu.Enabled = DatenbankDa && _Ansicht == enAnsicht.Überschriften_und_Formular && _Database.PermissionCheck(_Database.PermissionGroups_NewRow, null);
            löschen.Enabled = DatenbankDa;
            Drucken.Enabled = DatenbankDa;
            Ansicht0.Enabled = DatenbankDa;
            Ansicht1.Enabled = DatenbankDa;
            Ansicht2.Enabled = DatenbankDa;
            Ansicht3.Enabled = DatenbankDa;
            Ordn.Enabled = DatenbankDa && !string.IsNullOrEmpty(_Database.Filename);




            BeziehungsEditor.Enabled = false;
            if (DatenbankDa)
            {
                foreach (var ThisColumnItem in _Database.Column)
                {
                    if (ThisColumnItem != null)
                    {

                        if (ThisColumnItem.Format == enDataFormat.RelationText)
                        {
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



            SaveAs.Enabled = DatenbankDa;




            Drucken.Item["csv"].Enabled = DatenbankDa && TableView.Design != enBlueTableAppearance.OnlyMainColumnWithoutHead;
            Drucken.Item["html"].Enabled = DatenbankDa && TableView.Design != enBlueTableAppearance.OnlyMainColumnWithoutHead;



            Vorwärts.Enabled = DatenbankDa;
            zurück.Enabled = DatenbankDa;
            such.Enabled = DatenbankDa;
            ZeilenFilter_TextFeld.Enabled = DatenbankDa;
            btnTextLöschen.Enabled = DatenbankDa;
            AlleFilterAus.Enabled = DatenbankDa;


        }




        private void SuchenUndErsetzen_Click(object sender, EventArgs e)
        {
            TableView.OpenSearchAndReplace();
        }

        private void AngezeigteZeilenLöschen_Click(object sender, EventArgs e)
        {
            TableView.Database.Row.Remove(TableView.Filter);
            CheckButtons();
        }


        private void TableView_ContextMenu_Init(object sender, ContextMenuInitEventArgs e)
        {
            var CellKey = string.Empty;
            if (e.Tag is string s) { CellKey = s; }
            if (string.IsNullOrEmpty(CellKey)) { return; }
            TableView.Database.Cell.DataOfCellKey(CellKey, out var Column, out var Row);

            if (_Ansicht != enAnsicht.Überschriften_und_Formular)
            {

                e.UserMenu.Add(new TextListItem(true, "Sortierung"));
                e.UserMenu.Add(enContextMenuComands.SpaltenSortierungAZ, Column != null && Column.Format.CanBeChangedByRules());
                e.UserMenu.Add(enContextMenuComands.SpaltenSortierungZA, Column != null && Column.Format.CanBeChangedByRules());

                e.UserMenu.Add(new LineListItem());


                e.UserMenu.Add(new TextListItem(true, "Zelle"));
                e.UserMenu.Add(new TextListItem("ContentCopy", "Inhalt Kopieren", enImageCode.Kopieren, Column != null && Column.Format.CanBeChangedByRules()));
                e.UserMenu.Add(new TextListItem("ContentPaste", "Inhalt Einfügen", enImageCode.Clipboard, Column != null && Column.Format.CanBeChangedByRules()));

                e.UserMenu.Add(new TextListItem("ContentDelete", "Inhalt löschen", enImageCode.Radiergummi, Column != null && Column.Format.CanBeChangedByRules()));
                e.UserMenu.Add(enContextMenuComands.VorherigenInhaltWiederherstellen, Column != null && Column.Format.CanBeChangedByRules() && Column.ShowUndo);

                e.UserMenu.Add(enContextMenuComands.SuchenUndErsetzen, Column != null && Column.Format.CanBeChangedByRules());

                e.UserMenu.Add(new LineListItem());

                e.UserMenu.Add(new TextListItem(true, "Spalte"));
                e.UserMenu.Add(enContextMenuComands.SpaltenEigenschaftenBearbeiten, Column != null);

                e.UserMenu.Add(new TextListItem("ColumnContentDelete", "Inhalte aller angezeigten Zellen dieser Spalte löschen", enImageCode.Radiergummi, Column != null && Column.Format.CanBeChangedByRules()));

                e.UserMenu.Add(new TextListItem("Summe", "Summe", enImageCode.Summe, Column != null));


                e.UserMenu.Add(new LineListItem());

            }
            e.UserMenu.Add(new TextListItem(true, "Zeile"));
            e.UserMenu.Add(enContextMenuComands.ZeileLöschen, Row != null);


            e.UserMenu.Add(new TextListItem("Fehlersuche", "Fehler anzeigen", enImageCode.Kritisch, Row != null));


        }


        private void TableView_ContextMenuItemClicked(object sender, ContextMenuItemClickedEventArgs e)
        {


            var bt = (Table)sender;

            var CellKey = string.Empty;
            if (e.Tag is string s) { CellKey = s; }
            if (string.IsNullOrEmpty(CellKey)) { return; }
            TableView.Database.Cell.DataOfCellKey(CellKey, out var Column, out var Row);


            switch (e.ClickedComand.Internal())
            {

                case "SpaltenSortierungAZ":
                    bt.SortDefinitionTemporary = new RowSortDefinition(bt.Database, Column.Name, false);
                    break;

                case "SpaltenSortierungZA":
                    bt.SortDefinitionTemporary = new RowSortDefinition(bt.Database, Column.Name, true);
                    break;

                case "Fehlersuche":
                    MessageBox.Show(Row.DoAutomatic(false, true, false));
                    break;

                case "ZeileLöschen":
                    if (Row != null)
                    {
                        if (MessageBox.Show("Zeile löschen?", enImageCode.Frage, "Ja", "Nein") == 0)
                        {
                            bt.Database.Row.Remove(Row);
                        }
                    }
                    break;

                case "ContentDelete":
                    TableView.Database.Cell.Delete(Column, Row.Key);
                    break;

                case "SpaltenEigenschaftenBearbeiten":
                    tabAdministration.OpenColumnEditor(Column);
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
                    if (!summe.HasValue)
                    {
                        MessageBox.Show("Die Summe konnte nicht berechnet werden.", enImageCode.Summe, "OK");
                    }
                    else
                    {
                        MessageBox.Show("Summe dieser Spalte, nur angezeigte Zeilen: <br><b>" + summe, enImageCode.Summe, "OK");
                    }
                    break;

                case "VorherigenInhaltWiederherstellen":
                    TableView.DoUndo(Column, Row);
                    break;

                case "ContentPaste":
                    //     bt.Database.BeginnEdit()
                    Row.CellSet(Column, Convert.ToString(System.Windows.Forms.Clipboard.GetDataObject().GetData(System.Windows.Forms.DataFormats.Text)));
                    break;

                case "ColumnContentDelete":
                    if (Column != null)
                    {
                        if (MessageBox.Show("Angezeite Inhalt dieser Spalte löschen?", enImageCode.Frage, "Ja", "Nein") == 0)
                        {
                            Column.DeleteContents(TableView.Filter);
                        }
                    }


                    break;

                default:
                    DebugPrint(e.ClickedComand);
                    break;
            }
        }

        private void TableView_RowsSorted(object sender, EventArgs e)
        {
            if (TableView.Database.Column[0] != null)
            {
                Zei.Text = "<ImageCode=Information|16>Zeilen: " + TableView.SortedRows().Count + " St.";
            }
            else
            {
                Zei.Text = "-";
            }

            CheckButtons();
        }


        private void Öffne_Click(object sender, EventArgs e)
        {
            LoadTab.ShowDialog();
        }


        private void ÜberDiesesProgramm_Click(object sender, EventArgs e)
        {
            MessageBox.Show("(c) Christian Peter<br>V " + _Version, enImageCode.Information, "OK");
        }

        private void Ansicht_Click(object sender, EventArgs e)
        {
            _Ansicht = (enAnsicht)int.Parse(((Button)sender).Name.Substring(7, 1));


            InitView();
            CheckButtons();
        }

        private void TemporärenSpeicherortÖffnen_Click(object sender, EventArgs e)
        {
            StandardTabx();
            ExecuteFile(Path.GetTempPath());
        }



        private void BeziehungsEditor_Click(object sender, EventArgs e)
        {

            Hide();

            var r = new RelationDiagram(_Database);

            r.ShowDialog();

            Show();
        }


        private void Ordn_Click(object sender, EventArgs e)
        {
            StandardTabx();
            ExecuteFile(_Database.Filename.FilePath());
        }


        private void Datenüberprüfung_Click(object sender, EventArgs e)
        {
            TableView.Database.Row.DoAutomatic(TableView.Filter, false);
        }





        private void LastDatabases_Item_Click(object sender, BasicListItemEventArgs e)
        {
            DatabaseSet(e.Item.Internal());
        }


        private void StandardTabx()
        {

            MainRibbon.SelectedIndex = 1;
        }


        protected override void OnFormClosed(System.Windows.Forms.FormClosedEventArgs e)
        {
            TraceLogging_End();
            base.OnFormClosed(e);
        }

        protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e)
        {
            SetDatabasetoNothing();
            Database.ReleaseAll(true);
            base.OnFormClosing(e);
        }





        private void AllgemeinerEditor_Click(object sender, EventArgs e)
        {
            Hide();

            var r = new PictureView
            {
                WindowState = System.Windows.Forms.FormWindowState.Maximized
            };
            r.ShowDialog();

            Show();
        }




        private void TableView_ColumnArrangementChanged(object sender, EventArgs e)
        {
            TableView.WriteColumnArrangementsInto(cbxColumnArr);
        }

        private void cbxColumnArr_ItemClicked(object sender, BasicListItemEventArgs e)
        {
            if (string.IsNullOrEmpty(cbxColumnArr.Text)) { return; }
            TableView.Arrangement = int.Parse(e.Item.Internal());
        }

        private void TableView_ViewChanged(object sender, EventArgs e)
        {
            TableView.WriteColumnArrangementsInto(cbxColumnArr);
        }

        private void btnTextLöschen_Click(object sender, EventArgs e)
        {
            ZeilenFilter_TextFeld.Text = string.Empty;
        }
    }
}