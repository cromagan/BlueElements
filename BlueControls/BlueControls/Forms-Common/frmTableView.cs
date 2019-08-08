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
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
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

namespace BlueControls.Forms
{
    public partial class frmTableView
    {
        public frmTableView() : this(null, true, true, false) { }
        public frmTableView(Database Database) : this(Database, false, false, false) { }

        public frmTableView(Database database, bool loadTabVisible, bool adminTabVisible, bool changeSkin)
        {
            ChangeSkin = changeSkin;
            InitializeComponent();


            Copyright.Text = "(c) 2010-" + DateTime.Now.Year + " Christian Peter";

            btnDrucken.Item.Clear();
            btnDrucken.Item.Add(new TextListItem("erweitert", "Drucken bzw. Export", QuickImage.Get(enImageCode.Drucker, 28)));
            btnDrucken.Item.Add(new LineListItem());
            btnDrucken.Item.Add(new TextListItem("csv", "CSV-Format f�r Excel in die Zwischenablage", QuickImage.Get(enImageCode.Excel, 28)));
            btnDrucken.Item.Add(new TextListItem("html", "HTML-Format f�r Internet-Seiten", QuickImage.Get(enImageCode.Globus, 28)));
            btnDrucken.Item.Add(new LineListItem());
            btnDrucken.Item.Add(new TextListItem("editor", "Layout-Editor �ffnen", QuickImage.Get(enImageCode.Layout, 28)));


            if (!adminTabVisible)
            {
                this.MainRibbon.Controls.Remove(tabAdmin);
                this.MainRibbon.Controls.Remove(tabExtras);
                grpAllgemein.Visible = false;
                grpBearbeitung.Visible = false;
            }
            else
            {
                tabAdmin.Table = TableView;

            }


            if (!loadTabVisible)
            {
                this.MainRibbon.Controls.Remove(tabDatei);
            }


            DatabaseSet(database);
        }

        private bool ChangeSkin = false;
        private Database _Database;
        private enAnsicht _Ansicht = enAnsicht.Nur_Tabelle;
        private const string _Version = "1.0001";


        private void SetDatabasetoNothing()
        {
            Formula.Database = null;
            TableView.Database = null;

            if (_Database != null)
            {
                _Database.Release(false, 180);
                _Database = null;
            }
        }


        private void zur�ck_Click(object sender, System.EventArgs e)
        {
            Formula.HideViewEditor();
            SuchEintragNoSave(enDirection.Oben, out var column, out var row);
            TableView.CursorPos_Set(column, row, false);
        }

        private void vor_Click(object sender, System.EventArgs e)
        {
            Formula.HideViewEditor();
            SuchEintragNoSave(enDirection.Unten, out var column, out var row);
            TableView.CursorPos_Set(column, row, false);
        }


        private void SuchEintragNoSave(enDirection Richtung, out ColumnItem column, out RowItem row)
        {

            column = TableView.Database.Column[0];
            row = null;

            if (TableView.Database.Row.Count() < 1) { return; }


            // Tempor�r berechnen, um geflacker zu vermeiden (Endabled - > Disabled bei Nothing)


            if (Convert.ToBoolean(Richtung & enDirection.Unten))
            {
                row = TableView.View_NextRow(Formula.ShowingRow);
                if (row == null) { row = TableView.View_RowFirst(); }
            }

            if (Convert.ToBoolean(Richtung & enDirection.Oben))
            {
                row = TableView.View_PreviousRow(Formula.ShowingRow);
                if (row == null) { row = TableView.View_RowLast(); }
            }

            if (row == null) { row = TableView.View_RowFirst(); }

        }


        private void TableView_CursorPosChanged(object sender, CellEventArgs e)
        {

            if (e.Column == null || _Ansicht == enAnsicht.Nur_Tabelle || e.Row == null)
            {
                Formula.ShowingRowKey = -1;
            }
            else
            {
                Formula.ShowingRowKey = e.Row.Key;
            }


            if (_Ansicht == enAnsicht.�berschriften_und_Formular)
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

        private void such_Enter(object sender, System.EventArgs e)
        {
            if (SuchB.Enabled) { SuchB_Click(SuchB, null); }
        }

        private void such_TextChanged(object sender, System.EventArgs e)
        {
            Check_SuchButton();
        }

        private void SuchB_Click(object sender, System.EventArgs e)
        {
            Formula.HideViewEditor();
            var SuchtT = such.Text.Trim();

            if (string.IsNullOrEmpty(SuchtT))
            {
                MessageBox.Show("Bitte Text zum Suchen eingeben.", enImageCode.Information, "OK");
                return;
            }



            Table.SearchNextText(SuchtT, TableView, null, Formula.ShowingRow, out var found, out var GefRow, true);


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


            //                    // Allgemeine Pr�fung
            //                    if (IsT.ToLower().Contains(SuchtT.ToLower()))
            //                    {
            //                        GefRow = CheckRow;
            //                    }

            //                    // Spezielle Format-Pr�fung
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
                if (GefRow == Formula.ShowingRow)
                {
                    MessageBox.Show("Text nur im <b>aktuellen Eintrag</b> gefunden,<br>aber sonst keine weiteren Eintr�ge!", enImageCode.Information, "OK");
                }
                else
                {
                    TableView.CursorPos_Set(_Database.Column[0], GefRow, true);
                }
            }
        }


        private void Neu_Click(object sender, System.EventArgs e)
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


        public void L�scheZeile(object sender, System.EventArgs e)
        {

            Formula.HideViewEditor();

            if (Ansicht1.Checked)
            {
                if (Formula.ShowingRow == null)
                {
                    MessageBox.Show("Kein Eintrag gew�hlt.", enImageCode.Information, "OK");
                    return;
                }


                var tmpr = Formula.ShowingRow;
                if (MessageBox.Show("Soll der Eintrag<br><b>" + tmpr.CellFirstString() + "</b><br>wirklich <b>gel�scht</b> werden?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }



                SuchEintragNoSave(enDirection.Unten, out var column, out var row);
                TableView.CursorPos_Set(column, row, false);
                TableView.Database.Row.Remove(tmpr);
            }
            else
            {
                Formula.ShowingRowKey = -1;
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


            btnLetzteDateien.AddFileName(Datei, string.Empty);
            LoadTab.FileName = Datei;


            var tmpDatabase = Database.GetByFilename(Datei);

            if (tmpDatabase == null)
            {
                tmpDatabase = new Database(false);
                tmpDatabase.Load(Datei);
            }


            if (tmpDatabase == null) { return; }

            DatabaseSet(tmpDatabase);
        }


        private void DatabaseSet(Database cDatabase)
        {
            _Database = cDatabase;
            TableView.Database = cDatabase;
            Formula.Database = cDatabase;


            StandardTabx();


            SuspendLayout();

            if (_Database == null)
            {
                SetDatabasetoNothing();
            }
            else
            {

                if (_Database.Skin != (int)enSkin.Unver�ndert && (int)Skin.Instance.SkinDesign != _Database.Skin)
                {
                    Skin.Instance.SkinDesign = (enSkin)_Database.Skin;
                    Refresh();
                }

                if (_Database.Ansicht != enAnsicht.Unver�ndert)
                {
                    _Ansicht = _Database.Ansicht;
                }
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





        private void ZeilenFilter_TextFeld_TextChanged(object sender, System.EventArgs e)
        {
            if (grpFilter.Visible == false) { return; }

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

        private void ZeilenFilter_TextFeld_Enter(object sender, System.EventArgs e)
        {
            Filter_ZeilenFilterSetzen();
        }


        private void Filter_ZeilenFilterSetzen()
        {
            if (TableView.Database != null) { TableView.Filter.Delete_RowFilter(); }

            if (TableView.Database != null && !string.IsNullOrEmpty(ZeilenFilter_TextFeld.Text))
            {
                TableView.Filter.Add(enFilterType.Instr_UND_Gro�KleinEgal, new List<string>(ZeilenFilter_TextFeld.Text.SplitBy("+")));
            }

        }

        private void AlleFilterAus_Click(object sender, System.EventArgs e)
        {
            ZeilenFilter_TextFeld.Text = string.Empty;

            if (TableView.Filter != null)
            {
                TableView.Filter.Clear();
            }
        }



        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);


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


                    if (Formula.ShowingRow != null)
                    {
                        Ara.Add(Formula.ShowingRow);
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
                    MessageBox.Show("Die gew�nschten Daten<br>sind nun im Zwischenspeicher.", enImageCode.Clipboard, "Ok");

                    break;
                case "html":
                    TableView.Export_HTML();
                    break;


                default:
                    DebugPrint(e.Item);
                    break;
            }
        }

        protected override void OnShown(System.EventArgs e)
        {
            base.OnShown(e);
            MainRibbon.SelectedIndex = 0;
            InitView();
        }




        private void InitView()
        {

            Formula.HideViewEditor();


            Ansicht0.Checked = _Ansicht == enAnsicht.Nur_Tabelle;
            Ansicht1.Checked = _Ansicht == enAnsicht.�berschriften_und_Formular;
            Ansicht2.Checked = _Ansicht == enAnsicht.Tabelle_und_Formular_nebeneinander;
            Ansicht3.Checked = _Ansicht == enAnsicht.Tabelle_und_Formular_�bereinander;

            AlleFilterAus_Click(null, null);


            switch (_Ansicht)
            {
                case enAnsicht.Nur_Tabelle:
                    grpFormularSteuerung.Visible = false;
                    grpFilter.Visible = true;

                    Formula.Visible = false;

                    Formula.Dock = System.Windows.Forms.DockStyle.None;
                    TableView.Dock = System.Windows.Forms.DockStyle.None;


                    TableView.Design = enBlueTableAppearance.Standard;


                    Formula.Dock = System.Windows.Forms.DockStyle.None;
                    TableView.Dock = System.Windows.Forms.DockStyle.Fill;
                    break;

                case enAnsicht.�berschriften_und_Formular:
                    grpFormularSteuerung.Visible = true;
                    grpFilter.Visible = false;
                    TableView.Design = enBlueTableAppearance.OnlyMainColumnWithoutHead;
                    Formula.Visible = true;

                    TableView.Dock = System.Windows.Forms.DockStyle.Left;
                    TableView.Width = 250;

                    Formula.BringToFront();
                    Formula.Dock = System.Windows.Forms.DockStyle.Fill;
                    break;

                case enAnsicht.Tabelle_und_Formular_nebeneinander:
                    grpFormularSteuerung.Visible = false;
                    grpFilter.Visible = true;
                    TableView.Design = enBlueTableAppearance.Standard;
                    Formula.Visible = true;

                    TableView.Dock = System.Windows.Forms.DockStyle.None;

                    Formula.Dock = System.Windows.Forms.DockStyle.Right;
                    Formula.Width = (int)(MinimumSize.Width / 2.0);

                    TableView.BringToFront();
                    TableView.Dock = System.Windows.Forms.DockStyle.Fill;
                    break;

                case enAnsicht.Tabelle_und_Formular_�bereinander:
                    grpFormularSteuerung.Visible = false;
                    grpFilter.Visible = true;
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


            if (TableView.Visible)
            {
                if (TableView.Database != null)
                {
                    if (TableView.CursorPosRow() == null && TableView.View_RowFirst() != null)
                    {
                        TableView.CursorPos_Set(TableView.Database.Column[0], TableView.View_RowFirst(), false);

                    }
                    if (TableView.CursorPosRow() != null) { Formula.ShowingRowKey = TableView.CursorPosRow().Key; }
                }
            }
            else
            {
                Formula.ShowingRowKey = -1;
            }
        }


        private void LoadTab_FileOk(object sender, CancelEventArgs e)
        {
            DatabaseSet(LoadTab.FileName);
        }


        private void NeuDBSaveAs_Click(object sender, System.EventArgs e)
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
                    DebugPrint(enFehlerArt.Fehler, "Ung�ltiger Aufruf!");
                    break;
            }


            SaveTab.ShowDialog();

            if (!PathExists(SaveTab.FileName.FilePath())) { return; }
            if (string.IsNullOrEmpty(SaveTab.FileName)) { return; }

            StandardTabx();

            if (bu.Name == "NeuDB")
            {
                DatabaseSet(new Database(false)); // Ab jetzt in der Variable _Database zu finden
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

            btnOeffnen.Enabled = true;


            btnNeu.Enabled = DatenbankDa && _Ansicht == enAnsicht.�berschriften_und_Formular && _Database.PermissionCheck(_Database.PermissionGroups_NewRow, null);
            btnLoeschen.Enabled = DatenbankDa;
            btnDrucken.Enabled = DatenbankDa;
            Ansicht0.Enabled = DatenbankDa;
            Ansicht1.Enabled = DatenbankDa;
            Ansicht2.Enabled = DatenbankDa;
            Ansicht3.Enabled = DatenbankDa;
            btnDatenbanken.Enabled = DatenbankDa && !string.IsNullOrEmpty(_Database.Filename);




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
            AngezeigteZeilenL�schen.Enabled = DatenbankDa && TableView.Design != enBlueTableAppearance.OnlyMainColumnWithoutHead;
            Daten�berpr�fung.Enabled = DatenbankDa;



            btnSaveAs.Enabled = DatenbankDa;




            btnDrucken.Item["csv"].Enabled = DatenbankDa && TableView.Design != enBlueTableAppearance.OnlyMainColumnWithoutHead;
            btnDrucken.Item["html"].Enabled = DatenbankDa && TableView.Design != enBlueTableAppearance.OnlyMainColumnWithoutHead;



            Vorw�rts.Enabled = DatenbankDa;
            zur�ck.Enabled = DatenbankDa;
            such.Enabled = DatenbankDa;
            ZeilenFilter_TextFeld.Enabled = DatenbankDa;
            btnTextL�schen.Enabled = DatenbankDa;
            AlleFilterAus.Enabled = DatenbankDa;


        }




        private void SuchenUndErsetzen_Click(object sender, System.EventArgs e)
        {
            TableView.OpenSearchAndReplace();
        }

        private void AngezeigteZeilenL�schen_Click(object sender, System.EventArgs e)
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

            if (_Ansicht != enAnsicht.�berschriften_und_Formular)
            {

                e.UserMenu.Add(new TextListItem(true, "Sortierung"));
                e.UserMenu.Add(enContextMenuComands.SpaltenSortierungAZ, Column != null && Column.Format.CanBeChangedByRules());
                e.UserMenu.Add(enContextMenuComands.SpaltenSortierungZA, Column != null && Column.Format.CanBeChangedByRules());

                e.UserMenu.Add(new LineListItem());


                e.UserMenu.Add(new TextListItem(true, "Zelle"));
                e.UserMenu.Add(new TextListItem("ContentCopy", "Inhalt Kopieren", enImageCode.Kopieren, Column != null && Column.Format.CanBeChangedByRules()));
                e.UserMenu.Add(new TextListItem("ContentPaste", "Inhalt Einf�gen", enImageCode.Clipboard, Column != null && Column.Format.CanBeChangedByRules()));

                e.UserMenu.Add(new TextListItem("ContentDelete", "Inhalt l�schen", enImageCode.Radiergummi, Column != null && Column.Format.CanBeChangedByRules()));
                e.UserMenu.Add(enContextMenuComands.VorherigenInhaltWiederherstellen, Column != null && Column.Format.CanBeChangedByRules() && Column.ShowUndo);

                e.UserMenu.Add(enContextMenuComands.SuchenUndErsetzen, Column != null && Column.Format.CanBeChangedByRules());

                e.UserMenu.Add(new LineListItem());

                e.UserMenu.Add(new TextListItem(true, "Spalte"));
                e.UserMenu.Add(enContextMenuComands.SpaltenEigenschaftenBearbeiten, Column != null);

                e.UserMenu.Add(new TextListItem("ColumnContentDelete", "Inhalte aller angezeigten Zellen dieser Spalte l�schen", enImageCode.Radiergummi, Column != null && Column.Format.CanBeChangedByRules()));

                e.UserMenu.Add(new TextListItem("Summe", "Summe", enImageCode.Summe, Column != null));


                e.UserMenu.Add(new LineListItem());

            }
            e.UserMenu.Add(new TextListItem(true, "Zeile"));
            e.UserMenu.Add(enContextMenuComands.ZeileL�schen, Row != null);


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
                    MessageBox.Show(Row.DoAutomatic(true, true));
                    break;

                case "ZeileL�schen":
                    if (Row != null)
                    {
                        if (MessageBox.Show("Zeile l�schen?", enImageCode.Frage, "Ja", "Nein") == 0)
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

                case "�berallDel":
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
                        if (MessageBox.Show("Angezeite Inhalt dieser Spalte l�schen?", enImageCode.Frage, "Ja", "Nein") == 0)
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

        private void TableView_RowsSorted(object sender, System.EventArgs e)
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


        private void �ffne_Click(object sender, System.EventArgs e)
        {
            LoadTab.ShowDialog();
        }


        private void �berDiesesProgramm_Click(object sender, System.EventArgs e)
        {
            MessageBox.Show("(c) Christian Peter<br>V " + _Version, enImageCode.Information, "OK");
        }

        private void Ansicht_Click(object sender, System.EventArgs e)
        {
            _Ansicht = (enAnsicht)int.Parse(((Button)sender).Name.Substring(7, 1));


            InitView();
            CheckButtons();
        }

        private void Tempor�renSpeicherort�ffnen_Click(object sender, System.EventArgs e)
        {
            StandardTabx();
            ExecuteFile(Path.GetTempPath());
        }



        private void BeziehungsEditor_Click(object sender, System.EventArgs e)
        {

            Hide();

            var r = new RelationDiagram(_Database);

            r.ShowDialog();

            Show();
        }


        private void Ordn_Click(object sender, System.EventArgs e)
        {
            StandardTabx();
            ExecuteFile(_Database.Filename.FilePath());
        }


        private void Daten�berpr�fung_Click(object sender, System.EventArgs e)
        {
            TableView.Database.Row.DoAutomatic(TableView.Filter, true);
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
            Database.ReleaseAll(true, 180);
            base.OnFormClosing(e);
        }





        private void AllgemeinerEditor_Click(object sender, System.EventArgs e)
        {
            Hide();

            var r = new PictureView
            {
                WindowState = System.Windows.Forms.FormWindowState.Maximized
            };
            r.ShowDialog();

            Show();
        }




        private void TableView_ColumnArrangementChanged(object sender, System.EventArgs e)
        {
            TableView.WriteColumnArrangementsInto(cbxColumnArr);
        }

        private void cbxColumnArr_ItemClicked(object sender, BasicListItemEventArgs e)
        {
            if (string.IsNullOrEmpty(cbxColumnArr.Text)) { return; }
            TableView.Arrangement = int.Parse(e.Item.Internal());
        }

        private void TableView_ViewChanged(object sender, System.EventArgs e)
        {
            TableView.WriteColumnArrangementsInto(cbxColumnArr);
        }

        private void btnTextL�schen_Click(object sender, System.EventArgs e)
        {
            ZeilenFilter_TextFeld.Text = string.Empty;
        }



        public List<RowItem> GetFilteredItems()
        {
            ShowDialog();
            return TableView.SortedRows();
        }


    }
}