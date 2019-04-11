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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.DialogBoxes;
using BlueControls.EventArgs;
using BlueControls.ItemCollection;
using BlueDatabase;
using BlueDatabase.EventArgs;
using BlueDatabase.Enums;
using BlueControls.Enums;
using static BlueBasics.FileOperations;
using static BlueBasics.modAllgemein;

namespace BlueControls.BlueDatabaseDialogs
{
    public partial class tabAdministration : TabPage // System.Windows.Forms.UserControl //
    {

        private Table _TableView;
        private Database _database;
        private Database _originalDB;


        public tabAdministration()
        {
            InitializeComponent();
            Check_OrderButtons();
        }

        [DefaultValue((ComboBox)null)]
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

        private void ChangeDatabase(Database database)
        {
            _originalDB = null;
            btnVorherigeVersion.Text = "Vorherige Version";

            _database = database;

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

            _TableView.Arrangement = int.Parse(e.Item.Internal());

            _TableView.CursorPos_Set(_TableView.CursorPosKey(), true);
            Check_OrderButtons();
        }

        private void OrderAdd_Click(object sender, System.EventArgs e)
        {
            _TableView.Arrangement_Add();
        }

        private void OrderDelete_Click(object sender, System.EventArgs e)
        {
            if (_TableView.Arrangement < 2 || _TableView.Arrangement >= _TableView.Database.ColumnArrangements.Count) { return; }
            if (MessageBox.Show("Anordung <b>'" + _TableView.Database.ColumnArrangements[_TableView.Arrangement].Name + "'</b><br>wirklich löschen?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
            _TableView.Database.ColumnArrangements.RemoveAt(_TableView.Arrangement);

            _TableView.Arrangement = 1;
        }

        private void Rename_Click(object sender, System.EventArgs e)
        {
            var n = InputBox.Show("Umbenennen:", _TableView.Database.ColumnArrangements[_TableView.Arrangement].Name, enDataFormat.Text);
            if (!string.IsNullOrEmpty(n)) { _TableView.Database.ColumnArrangements[_TableView.Arrangement].Name = n; }
        }

        private void Rechtex_Click(object sender, System.EventArgs e)
        {
            var aa = new ItemCollectionList();
            aa.AddRange(_TableView.Database.Permission_AllUsed(true));
            aa.Sort();
            aa.CheckBehavior = enCheckBehavior.MultiSelection;
            aa.Check(_TableView.Database.ColumnArrangements[_TableView.Arrangement].PermissionGroups_Show, true);


            var b = InputBoxListBoxStyle.Show("Wählen sie, wer anzeigeberechtigt ist:<br><i>Info: Administratoren sehen alle Ansichten", aa, enAddType.Text, true);
            if (b == null) { return; }

            _TableView.Database.ColumnArrangements[_TableView.Arrangement].PermissionGroups_Show.Clear();
            _TableView.Database.ColumnArrangements[_TableView.Arrangement].PermissionGroups_Show.AddRange(b.ToArray());

            if (_TableView.Arrangement == 1) { _TableView.Database.ColumnArrangements[_TableView.Arrangement].PermissionGroups_Show.Add("#Everybody"); }

        }

        private void SpalteEinblenden_Click(object sender, System.EventArgs e)
        {
            var ic = new ItemCollectionList();

            foreach (var ThisColumnItem in _TableView.Database.Column)
            {
                if (ThisColumnItem != null && _TableView.Database.ColumnArrangements[_TableView.Arrangement][ThisColumnItem] == null) { ic.Add(ThisColumnItem); }

            }


            if (ic.Count == 0)
            {
                if (MessageBox.Show("Es werden bereits alle<br>Spalten angezeigt.<br><br>Wollen sie eine neue Spalte erstellen?", enImageCode.Frage, "Ja", "Nein") == 0) { btnNeueSpalteErstellen_Click(sender, e); }
                return;
            }

            ic.Sort();

            var r = InputBoxListBoxStyle.Show("Wählen sie:", ic, enAddType.None, true);
            if (r == null || r.Count == 0) { return; }
            _TableView.Database.ColumnArrangements[_TableView.Arrangement].Add(_TableView.Database.Column[r[0]], false);

            _TableView.Invalidate_HeadSize();

        }


        private void OrderReset_Click(object sender, System.EventArgs e)
        {
            if (MessageBox.Show("Alle Spalten anzeigen?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
            _TableView.Database.ColumnArrangements[_TableView.Arrangement].ShowAllColumns(_TableView.Database);
        }

        private void btnSpalteAusblenden_Click(object sender, System.EventArgs e)
        {
            ColumnViewItem ViewItem = null;
            if (_TableView.CursorPosColumn() != null) { ViewItem = _TableView.Database.ColumnArrangements[_TableView.Arrangement][_TableView.CursorPosColumn()]; }

            _TableView.Database.ColumnArrangements[_TableView.Arrangement].Remove(ViewItem);
        }

        private void btnSpalteBearbeiten_Click(object sender, System.EventArgs e)
        {
            OpenColumnEditor(_TableView.CursorPosColumn(), _TableView.CursorPosRow());
        }



        public static void OpenColumnEditor(ColumnItem column, RowItem Row)
        {
            if (column == null) { return; }

            if (Row == null)
            {
                OpenColumnEditor(column);
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
                    column.Database.Cell.LinkedCellData(column, Row, out column2, out _);
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


            OpenColumnEditor(column);






        }

        public static void OpenColumnEditor(ColumnItem column)
        {

            using (var w = new ColumnEditor(column))
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
                newc = new ColumnItem(Vorlage, true);
            }
            else
            {
                newc = new ColumnItem(_TableView.Database, true);
            }



            using (var w = new ColumnEditor(newc))
            {
                w.ShowDialog();
                newc.Invalidate_ColumAndContent();
            }


            _TableView.Database.Column.Repair();

            if (_TableView.Arrangement > 0) { _TableView.Database.ColumnArrangements[_TableView.Arrangement].Add(newc, false); }

            _TableView.Invalidate_HeadSize();

        }

        private void Systemspalten_Click(object sender, System.EventArgs e)
        {
            _TableView.Database.ColumnArrangements[_TableView.Arrangement].HideSystemColumns();
        }

        private void btnSpalteNachLinks_Click(object sender, System.EventArgs e)
        {

            if (_TableView.Arrangement > 0)
            {
                ColumnViewItem ViewItem = null;
                if (_TableView.CursorPosColumn() != null) { ViewItem = _TableView.Database.ColumnArrangements[_TableView.Arrangement][_TableView.CursorPosColumn()]; }
                _TableView.Database.ColumnArrangements[_TableView.Arrangement].Swap(ViewItem, ViewItem.PreviewsVisible(_TableView.Database.ColumnArrangements[_TableView.Arrangement]));
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
                if (_TableView.CursorPosColumn() != null) { ViewItem = _TableView.Database.ColumnArrangements[_TableView.Arrangement][_TableView.CursorPosColumn()]; }
                _TableView.Database.ColumnArrangements[_TableView.Arrangement].Swap(ViewItem, ViewItem.NextVisible(_TableView.Database.ColumnArrangements[_TableView.Arrangement]));
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

            if (column != null) { ViewItem = _TableView.Database.ColumnArrangements[_TableView.Arrangement][column]; }
            var IndexOfViewItem = -1;
            if (_TableView.Arrangement <= _TableView.Database.ColumnArrangements.Count) { IndexOfViewItem = _TableView.Database.ColumnArrangements[_TableView.Arrangement].IndexOf(ViewItem); }


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
                grpAktuelleSpalte.Text = "Spalte: -";
            }
            else
            {
                grpAktuelleSpalte.Text = "Spalte: " + column.ReadableText();
                btnSpalteNachLinks.Enabled = Convert.ToBoolean(IndexOfViewItem > 0);
                btnSpalteNachRechts.Enabled = Convert.ToBoolean(IndexOfViewItem >= 0) && Convert.ToBoolean(IndexOfViewItem < _TableView.Database.ColumnArrangements[_TableView.Arrangement].Count() - 1);



                btnPosEingeben.Enabled = _TableView.Arrangement > 0;

                if (_TableView.PermanentPossible(ViewItem) && _TableView.NonPermanentPossible(ViewItem))
                {
                    btnPermanent.Enabled = true;
                    btnPermanent.Checked = (ViewItem.ViewType == enViewType.PermanentColumn);
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
        private static void OpenDatabaseHeadEditor(Database DB)
        {
            DB.OnConnectedControlsStopAllWorking(new DatabaseStoppedEventArgs());
            DB.Reload();
            using (var w = new DatabaseHeadEditor(DB))
            {
                w.ShowDialog();
            }
            DB.OnLoaded(new LoadedEventArgs(true));
        }

        private void btnLayouts_Click(object sender, System.EventArgs e)
        {
            Develop.DebugPrint_InvokeRequired(InvokeRequired, true);

            OpenLayoutEditor(_TableView.Database, string.Empty, string.Empty);



        }

        public static void OpenLayoutEditor(Database DB, string AdditionalLayoutPath, string LayoutToOpen)
        {


            if (!DB.IsSaveAble()) { return; }

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


            if (_originalDB != null && _TableView.Database != _originalDB )
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


            foreach (var ThisExport in _TableView.Database._Export)
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

            var tmp = Database.GetByFilename(Files[0]);
            if (tmp == null)
            {
                tmp = new Database( true, _TableView.Database._PasswordSub, _TableView.Database._GenerateLayout, _TableView.Database._RenameColumnInLayout);
                tmp.LoadFromDisk(Files[0]);
            }

            _TableView.Database = tmp;

            _originalDB = _merker;
            btnVorherigeVersion.Text = "zurück";
            btnVorherigeVersion.Enabled = true;
        }

        private void btnPermanent_CheckedChanged(object sender, System.EventArgs e)
        {

            ColumnViewItem ViewItem = null;
            if (_TableView.CursorPosColumn() != null) { ViewItem = _TableView.Database.ColumnArrangements[_TableView.Arrangement][_TableView.CursorPosColumn()]; }

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
                var ViewItem = _TableView.Database.ColumnArrangements[_TableView.Arrangement][c];

                if (ViewItem != null)
                {
                    _TableView.Database.ColumnArrangements[_TableView.Arrangement].Remove(ViewItem);
                }

                if (index >= _TableView.Database.ColumnArrangements[_TableView.Arrangement].Count()) { index = _TableView.Database.ColumnArrangements[_TableView.Arrangement].Count() ; }
                _TableView.Database.ColumnArrangements[_TableView.Arrangement].InsertAt(index, c);
                _TableView.CursorPos_Set(c, _TableView.CursorPosRow(), true);
                Check_OrderButtons();
            }
        }
    }
}

