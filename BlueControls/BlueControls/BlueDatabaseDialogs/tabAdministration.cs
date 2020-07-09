#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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
using BlueBasics.EventArgs;
using BlueControls.Controls;
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
using System.Drawing;
using System.IO;
using static BlueBasics.FileOperations;
using static BlueBasics.modConverter;

namespace BlueControls.BlueDatabaseDialogs
{
    public partial class tabAdministration : TabPage // System.Windows.Forms.UserControl //
    {

        private Table _TableView;
        private Database _database;
        private Database _originalDB;


        public tabAdministration() : base()
        {
            InitializeComponent();
            Check_OrderButtons();
        }

        [DefaultValue((Table)null)]
        public Table Table
        {

            get
            {
                return _TableView;
            }
            set
            {

                if (_TableView == value) { return; }

                if (_TableView != null)
                {
                    _TableView.DatabaseChanged -= _TableView_DatabaseChanged;
                    _TableView.CursorPosChanged -= _TableView_CursorPosChanged;
                    _TableView.ViewChanged -= _TableView_ViewChanged;
                    _TableView.EnabledChanged -= _TableView_EnabledChanged;
                    ChangeDatabase(null);
                }
                _TableView = value;
                Check_OrderButtons();


                if (_TableView != null)
                {
                    ChangeDatabase(_TableView.Database);
                    _TableView.DatabaseChanged += _TableView_DatabaseChanged;
                    _TableView.CursorPosChanged += _TableView_CursorPosChanged;
                    _TableView.ViewChanged += _TableView_ViewChanged;
                    _TableView.EnabledChanged += _TableView_EnabledChanged;
                }
            }
        }

        private void _TableView_EnabledChanged(object sender, System.EventArgs e)
        {
            UpdateViewControlls();
            Check_OrderButtons();
        }

        private void _TableView_ViewChanged(object sender, System.EventArgs e)
        {
            UpdateViewControlls();
            Check_OrderButtons();
        }

        private void _TableView_CursorPosChanged(object sender, CellEventArgs e)
        {
            Check_OrderButtons();
        }

        private void _TableView_DatabaseChanged(object sender, System.EventArgs e)
        {
            ChangeDatabase(_TableView.Database);
            UpdateViewControlls();
            Check_OrderButtons();
        }

        public static void CheckDatabase(object sender, LoadedEventArgs e)
        {

            var _database = (Database)sender;

            if (_database != null && !_database.ReadOnly)
            {
                if (_database.IsAdministrator())
                {
                    while (!_database.AllRulesOK())
                    {
                        MessageBox.Show("Bitte reparieren sie<br>die fehlerhaften Regeln.", enImageCode.Information, "OK");
                        OpenDatabaseHeadEditor(_database);
                    }


                    foreach (var ThisColumnItem in _database.Column)
                    {
                        while (!ThisColumnItem.IsOk())
                        {
                            MessageBox.Show("Die folgende Spalte enthält einen Fehler:<br>" + ThisColumnItem.ErrorReason() + "<br><br>Bitte reparieren.", enImageCode.Information, "OK");
                            OpenColumnEditor(ThisColumnItem, null);

                        }
                    }
                }
            }
        }

        private void ChangeDatabase(Database database)
        {
            _originalDB = null;
            btnVorherigeVersion.Text = "Vorherige Version";

            _database = database;

            CheckDatabase(database, null);


            UpdateViewControlls();
            Check_OrderButtons();
        }



        private void UpdateViewControlls()
        {
            _TableView.WriteColumnArrangementsInto(cbxInternalColumnArrangementSelector);
        }

        private void cbxInternalColumnArrangementSelector_ItemClicked(object sender, BasicListItemEventArgs e)
        {
            if (string.IsNullOrEmpty(cbxInternalColumnArrangementSelector.Text)) { return; }
            _TableView.Arrangement = int.Parse(e.Item.Internal);
            Check_OrderButtons();
        }

        private void OrderAdd_Click(object sender, System.EventArgs e)
        {
            _TableView.Arrangement_Add();
        }

        private void OrderDelete_Click(object sender, System.EventArgs e)
        {
            if (_TableView.Arrangement < 2 || _TableView.Arrangement >= _TableView.Database.ColumnArrangements.Count) { return; }
            if (MessageBox.Show("Anordung <b>'" + _TableView.CurrentArrangement.Name + "'</b><br>wirklich löschen?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
            _TableView.Database.ColumnArrangements.RemoveAt(_TableView.Arrangement);

            _TableView.Arrangement = 1;
        }

        private void Rename_Click(object sender, System.EventArgs e)
        {
            var n = InputBox.Show("Umbenennen:", _TableView.CurrentArrangement.Name, enDataFormat.Text);
            if (!string.IsNullOrEmpty(n)) { _TableView.Database.ColumnArrangements[_TableView.Arrangement].Name = n; }
        }

        private void Rechtex_Click(object sender, System.EventArgs e)
        {
            var aa = new ItemCollectionList();
            aa.AddRange(_TableView.Database.Permission_AllUsed(true));
            aa.Sort();
            aa.CheckBehavior = enCheckBehavior.MultiSelection;
            aa.Check(_TableView.CurrentArrangement.PermissionGroups_Show, true);


            var b = InputBoxListBoxStyle.Show("Wählen sie, wer anzeigeberechtigt ist:<br><i>Info: Administratoren sehen alle Ansichten", aa, enAddType.Text, true);
            if (b == null) { return; }

            _TableView.CurrentArrangement.PermissionGroups_Show.Clear();
            _TableView.CurrentArrangement.PermissionGroups_Show.AddRange(b.ToArray());

            if (_TableView.Arrangement == 1) { _TableView.CurrentArrangement.PermissionGroups_Show.Add("#Everybody"); }

        }

        private void SpalteEinblenden_Click(object sender, System.EventArgs e)
        {
            var ic = new ItemCollectionList();

            foreach (var ThisColumnItem in _TableView.Database.Column)
            {
                if (ThisColumnItem != null && _TableView.CurrentArrangement[ThisColumnItem] == null) { ic.Add(ThisColumnItem, false); }

            }


            if (ic.Count == 0)
            {
                if (MessageBox.Show("Es werden bereits alle<br>Spalten angezeigt.<br><br>Wollen sie eine neue Spalte erstellen?", enImageCode.Frage, "Ja", "Nein") == 0) { btnNeueSpalteErstellen_Click(sender, e); }
                return;
            }

            ic.Sort();

            var r = InputBoxListBoxStyle.Show("Wählen sie:", ic, enAddType.None, true);
            if (r == null || r.Count == 0) { return; }
            _TableView.CurrentArrangement.Add(_TableView.Database.Column[r[0]], false);

            _TableView.Invalidate_HeadSize();

        }


        private void OrderReset_Click(object sender, System.EventArgs e)
        {
            if (MessageBox.Show("Alle Spalten anzeigen?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
            _TableView.CurrentArrangement.ShowAllColumns(_TableView.Database);
        }

        private void btnSpalteAusblenden_Click(object sender, System.EventArgs e)
        {
            ColumnViewItem ViewItem = null;
            if (_TableView.CursorPosColumn() != null) { ViewItem = _TableView.CurrentArrangement[_TableView.CursorPosColumn()]; }

            _TableView.CurrentArrangement.Remove(ViewItem);
        }

        private void btnSpalteBearbeiten_Click(object sender, System.EventArgs e)
        {
            OpenColumnEditor(_TableView.CursorPosColumn(), _TableView.CursorPosRow(), _TableView);
        }



        public static void OpenColumnEditor(ColumnItem column, RowItem Row, Table tableview)
        {
            if (column == null) { return; }

            if (Row == null)
            {
                OpenColumnEditor(column, tableview);
                return;
            }

            ColumnItem column2 = null;
            var PosError = false;


            switch (column.Format)
            {

                case enDataFormat.Columns_für_LinkedCellDropdown:
                    var Txt = Row.CellGetString(column);
                    if (int.TryParse(Txt, out var ColKey))
                    {
                        column2 = column.LinkedDatabase().Column.SearchByKey(ColKey);
                    }
                    break;

                case enDataFormat.LinkedCell:
                case enDataFormat.Values_für_LinkedCellDropdown:
                    var LinkedData = CellCollection.LinkedCellData(column, Row, false, true, false);
                    column2 = LinkedData.Item1;
                    PosError = true;
                    break;
            }




            if (column2 != null)
            {
                if (MessageBox.Show("Welche Spalte bearbeiten?", enImageCode.Frage, "Spalte in dieser Datenbank", "Verlinkte Spalte") == 1) { column = column2; }
            }
            else
            {
                if (PosError)
                {
                    Notification.Show("Keine aktive Verlinkung.<br>Spalte in dieser Datenbank wird angezeigt.<br><br>Ist die Ziel-Zelle in der Ziel-Datenbank vorhanden?", enImageCode.Information);
                }
            }


            OpenColumnEditor(column, tableview);






        }

        public static void OpenColumnEditor(ColumnItem column, Table tableview)
        {

            using (var w = new ColumnEditor(column, tableview))
            {
                w.ShowDialog();
                column.Invalidate_ColumAndContent();
            }

        }


        private void btnSpalteDauerhaftloeschen_Click(object sender, System.EventArgs e)
        {
            if (MessageBox.Show("Spalte <b>" + _TableView.CursorPosColumn().ReadableText() + "</b> endgültig löschen?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
            _TableView.Database.Column.Remove(_TableView.CursorPosColumn());
        }

        private void btnNeueSpalteErstellen_Click(object sender, System.EventArgs e)
        {


            if (_TableView.Database.ReadOnly) { return; }

            var Vorlage = _TableView.CursorPosColumn();

            if (Vorlage != null && !string.IsNullOrEmpty(Vorlage.Identifier)) { Vorlage = null; }
            if (Vorlage != null)
            {
                switch (MessageBox.Show("Spalte '" + Vorlage.ReadableText() + "' als<br>Vorlage verwenden?", enImageCode.Frage, "Ja", "Nein", "Abbrechen"))
                {
                    case 0:
                        break;

                    case 1:
                        Vorlage = null;
                        break;

                    default:
                        return;
                }
            }


            ColumnItem newc = null;

            if (Vorlage != null)
            {
                newc = _TableView.Database.Column.AddACloneFrom(Vorlage);

            }
            else
            {
                newc = _TableView.Database.Column.Add();
            }


            using (var w = new ColumnEditor(newc, _TableView))
            {
                w.ShowDialog();
                newc.Invalidate_ColumAndContent();
            }


            _TableView.Database.Column.Repair();

            if (_TableView.Arrangement > 0) { _TableView.CurrentArrangement.Add(newc, false); }

            _TableView.Invalidate_HeadSize();

        }

        private void Systemspalten_Click(object sender, System.EventArgs e)
        {
            _TableView.CurrentArrangement.HideSystemColumns();
        }

        private void btnSpalteNachLinks_Click(object sender, System.EventArgs e)
        {

            if (_TableView.Arrangement > 0)
            {
                ColumnViewItem ViewItem = null;
                if (_TableView.CursorPosColumn() != null) { ViewItem = _TableView.CurrentArrangement[_TableView.CursorPosColumn()]; }
                _TableView.CurrentArrangement.Swap(ViewItem, ViewItem.PreviewsVisible(_TableView.CurrentArrangement));
            }
            else
            {
                _TableView.Database.Column.Swap(_TableView.CursorPosColumn(), _TableView.CursorPosColumn().Previous());
            }

            _TableView.EnsureVisible(_TableView.CursorPosColumn(), _TableView.CursorPosRow());
            Check_OrderButtons();
        }

        private void btnSpalteNachRechts_Click(object sender, System.EventArgs e)
        {
            if (_TableView.Arrangement > 0)
            {
                ColumnViewItem ViewItem = null;
                if (_TableView.CursorPosColumn() != null) { ViewItem = _TableView.CurrentArrangement[_TableView.CursorPosColumn()]; }
                _TableView.CurrentArrangement.Swap(ViewItem, ViewItem.NextVisible(_TableView.CurrentArrangement));
            }
            else
            {
                _TableView.Database.Column.Swap(_TableView.CursorPosColumn(), _TableView.CursorPosColumn().Next());
            }

            _TableView.EnsureVisible(_TableView.CursorPosColumn(), _TableView.CursorPosRow());
            Check_OrderButtons();
        }


        private void btnSpaltenUebersicht_Click(object sender, System.EventArgs e)
        {
            _TableView.Database.Column.GenerateOverView();
        }

        private void Check_OrderButtons()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Check_OrderButtons()));
                return;
            }

            var enTabAllgemein = true;
            var enTabellenAnsicht = true;
            var enAnsichtsVerwaltung = true;
            var enAktuelleAnsicht = true;
            var enAktuelleSpalte = true;

            if (_TableView?.Database == null || !_TableView.Database.IsAdministrator())
            {
                Enabled = false;
                return; // Weitere funktionen benötigen sicher eine Datenbank um keine Null Exception auszulösen
            }

            if (_TableView.Design != enBlueTableAppearance.Standard || !_TableView.Enabled || !_TableView.Visible || _TableView.Database.ReadOnly)
            {
                enTabellenAnsicht = false;
                enAnsichtsVerwaltung = false;
                enAktuelleAnsicht = false;
                enAktuelleSpalte = false;
            }


            ColumnViewItem ViewItem = null;
            var column = _TableView.CursorPosColumn();

            if (column != null) { ViewItem = _TableView.CurrentArrangement[column]; }
            var IndexOfViewItem = -1;
            if (_TableView.Arrangement <= _TableView.Database.ColumnArrangements.Count) { IndexOfViewItem = _TableView.CurrentArrangement.IndexOf(ViewItem); }


            var enLayoutEditable = Convert.ToBoolean(_TableView.Arrangement > 0); // Hauptansicht (0) kann nicht bearbeitet werden
            var enLayoutDeletable = Convert.ToBoolean(_TableView.Arrangement > 1); // Hauptansicht (0) und Allgemeine Ansicht (1) können nicht gelöscht werden

            btnAktuelleAnsichtLoeschen.Enabled = enLayoutDeletable;

            btnAlleSpaltenEinblenden.Enabled = enLayoutEditable;
            btnSpalteAusblenden.Enabled = enLayoutEditable;
            btnSpalteEinblenden.Enabled = enLayoutEditable;
            btnSystemspaltenAusblenden.Enabled = enLayoutEditable;

            btnSpalteDauerhaftloeschen.Enabled = Convert.ToBoolean(column != null && string.IsNullOrEmpty(column.Identifier));


            if (column == null || ViewItem == null)
            {
                enAktuelleSpalte = false;
                //grpAktuelleSpalte.Text = "Spalte: -";
            }
            else
            {
                // grpAktuelleSpalte.Text = "Spalte: " + column.ReadableText();
                btnSpalteNachLinks.Enabled = Convert.ToBoolean(IndexOfViewItem > 0);
                btnSpalteNachRechts.Enabled = Convert.ToBoolean(IndexOfViewItem >= 0) && Convert.ToBoolean(IndexOfViewItem < _TableView.CurrentArrangement.Count() - 1);



                btnPosEingeben.Enabled = _TableView.Arrangement > 0;

                if (_TableView.PermanentPossible(ViewItem) && _TableView.NonPermanentPossible(ViewItem))
                {
                    btnPermanent.Enabled = true;
                    btnPermanent.Checked = ViewItem.ViewType == enViewType.PermanentColumn;
                }
                else if (_TableView.PermanentPossible(ViewItem))
                {
                    btnPermanent.Enabled = false;
                    btnPermanent.Checked = true;
                }
                else
                {
                    btnPermanent.Enabled = false;
                    btnPermanent.Checked = false;
                }



            }

            grpAllgemein.Enabled = enTabAllgemein;
            grpTabellenAnsicht.Enabled = enTabellenAnsicht;
            grpAnsichtsVerwaltung.Enabled = enAnsichtsVerwaltung;
            grpAktuelleAnsicht.Enabled = enAktuelleAnsicht;
            grpAktuelleSpalte.Enabled = enAktuelleSpalte;
            Enabled = true;
        }


        /// <summary>
        /// Löst das DatabaseLoadedEvengt aus, weil es fast einem Neuladen gleichkommt.
        /// </summary>
        /// <param name="DB"></param>
        public static void OpenDatabaseHeadEditor(Database DB)
        {
            DB.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
            DB.Load_Reload();
            using (var w = new DatabaseHeadEditor(DB))
            {
                w.ShowDialog();
            }
            // DB.OnLoaded(new LoadedEventArgs(true));
        }

        private void btnLayouts_Click(object sender, System.EventArgs e)
        {
            Develop.DebugPrint_InvokeRequired(InvokeRequired, true);

            OpenLayoutEditor(_TableView.Database, string.Empty, string.Empty);



        }

        public static void OpenLayoutEditor(Database DB, string AdditionalLayoutPath, string LayoutToOpen)
        {
            if (!string.IsNullOrEmpty(DB.ErrorReason(enErrorReason.EditNormaly))) { return; }

            DB.AbortBackup();

            var w = new LayoutDesigner(DB, AdditionalLayoutPath);
            if (!string.IsNullOrEmpty(LayoutToOpen)) { w.LoadLayout(LayoutToOpen); }
            w.ShowDialog();
        }


        private void btnDatenbankKopf_Click(object sender, System.EventArgs e)
        {
            OpenDatabaseHeadEditor(_TableView.Database);
        }

        private void btnClipboardImport_Click(object sender, System.EventArgs e)
        {
            _TableView.ImportClipboard();
        }

        private void btnVorherigeVersion_Click(object sender, System.EventArgs e)
        {
            btnVorherigeVersion.Enabled = false;


            if (_originalDB != null && _TableView.Database != _originalDB)
            {
                _TableView.Database = _originalDB;
                _originalDB = null;
                btnVorherigeVersion.Text = "Vorherige Version";
                btnVorherigeVersion.Enabled = true;
                return;
            }

            var _merker = _TableView.Database;

            var Zusatz = new List<string>();


            var L = new ItemCollectionList();


            foreach (var ThisExport in _TableView.Database.Export)
            {
                if (ThisExport.Typ == enExportTyp.DatenbankOriginalFormat)
                {
                    foreach (var ThisString in ThisExport._BereitsExportiert)
                    {
                        var t = ThisString.SplitBy("|");
                        if (FileExists(t[0]))
                        {
                            var q1 = QuickImage.Get(enImageCode.Kugel, 16, Extensions.MixColor(Color.Red, Color.Green, DateTime.Now.Subtract(DateTimeParse(t[1])).TotalDays / ThisExport.AutomatischLöschen).ToHTMLCode(), "");
                            L.Add(new TextListItem(t[0], t[1], q1, true, DataFormat.CompareKey(t[1], enDataFormat.Datum_und_Uhrzeit)));
                        }
                    }

                    Zusatz.AddRange(Directory.GetFiles(ThisExport.Verzeichnis, _TableView.Database.Filename.FileNameWithoutSuffix() + "_*.MDB"));
                }
            }


            foreach (var ThisString in Zusatz)
            {

                if (L[ThisString] == null)
                {
                    L.Add(new TextListItem(ThisString, ThisString.FileNameWithSuffix(), QuickImage.Get(enImageCode.Warnung), true, DataFormat.CompareKey(new FileInfo(ThisString).CreationTime.ToString(), enDataFormat.Datum_und_Uhrzeit)));
                }

            }



            if (L.Count == 0)
            {
                MessageBox.Show("Kein Backup vorhanden.", enImageCode.Information, "OK");
                btnVorherigeVersion.Enabled = true;
                return;
            }

            L.Sort();

            var Files = InputBoxListBoxStyle.Show("Stand wählen:", L, enAddType.None, true);
            if (Files == null || Files.Count != 1)
            {
                btnVorherigeVersion.Enabled = true;
                return;
            }

            var tmp = (Database)Database.GetByFilename(Files[0], false);
            if (tmp == null)
            {
                tmp = new Database(true);
                tmp.Load(Files[0], false);
            }

            _TableView.Database = tmp;

            _originalDB = _merker;
            btnVorherigeVersion.Text = "zurück";
            btnVorherigeVersion.Enabled = true;
        }

        private void btnPermanent_CheckedChanged(object sender, System.EventArgs e)
        {

            ColumnViewItem ViewItem = null;
            if (_TableView.CursorPosColumn() != null) { ViewItem = _TableView.CurrentArrangement[_TableView.CursorPosColumn()]; }

            if (ViewItem == null) { return; }

            if (btnPermanent.Checked)
            {
                ViewItem.ViewType = enViewType.PermanentColumn;
            }
            else
            {
                ViewItem.ViewType = enViewType.Column;
            }
            Check_OrderButtons();
        }

        private void btnScripting_Click(object sender, System.EventArgs e)
        {
            var o = new Skript(_TableView);
            o.Show();
        }

        private void btnPosEingeben_Click(object sender, System.EventArgs e)
        {

            if (_TableView.Arrangement < 0) { return; }

            var c = _TableView.CursorPosColumn();

            if (c == null) { return; }


            var p = InputBox.Show("<b>" + _TableView.CursorPosColumn().ReadableText() + "</b><br>Auf welche Position verschieben?<br>Info: Nummerierung beginnt mit 1", "", enDataFormat.Ganzzahl);


            if (int.TryParse(p, out var index))
            {
                if (index < 1) { return; }
                index--;
                var ViewItem = _TableView.CurrentArrangement[c];

                if (ViewItem != null)
                {
                    _TableView.CurrentArrangement.Remove(ViewItem);
                }

                if (index >= _TableView.CurrentArrangement.Count()) { index = _TableView.CurrentArrangement.Count(); }
                _TableView.CurrentArrangement.InsertAt(index, c);
                _TableView.CursorPos_Set(c, _TableView.CursorPosRow(), true);
                Check_OrderButtons();
            }
        }
    }
}

