﻿// Authors:
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
using BlueBasics.EventArgs;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueControls.ItemCollection;
using BlueDatabase;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using static BlueBasics.FileOperations;
using static BlueBasics.modConverter;

namespace BlueControls.BlueDatabaseDialogs {

    public partial class tabAdministration : System.Windows.Forms.TabPage // System.Windows.Forms.UserControl //
    {
        #region Fields

        private Database _originalDB;
        private Table _TableView;

        #endregion

        #region Constructors

        public tabAdministration() : base() {
            InitializeComponent();
            Check_OrderButtons();
        }

        #endregion

        #region Properties

        [DefaultValue((Table)null)]
        public Table Table {
            get => _TableView;
            set {
                if (_TableView == value) { return; }
                if (_TableView != null) {
                    _TableView.DatabaseChanged -= TableView_DatabaseChanged;
                    _TableView.EnabledChanged -= TableView_EnabledChanged;
                    ChangeDatabase(null);
                }
                _TableView = value;
                Check_OrderButtons();
                if (_TableView != null) {
                    ChangeDatabase(_TableView.Database);
                    _TableView.DatabaseChanged += TableView_DatabaseChanged;
                    _TableView.EnabledChanged += TableView_EnabledChanged;
                }
            }
        }

        #endregion

        #region Methods

        public static void CheckDatabase(object sender, LoadedEventArgs e) {
            var _database = (Database)sender;
            if (_database != null && !_database.ReadOnly) {
                if (_database.IsAdministrator()) {
                    //while (!_database.AllRulesOK()) {
                    //    MessageBox.Show("Bitte reparieren sie<br>die fehlerhaften Regeln.", enImageCode.Information, "OK");
                    //    OpenDatabaseHeadEditor(_database);
                    //}
                    foreach (var ThisColumnItem in _database.Column) {
                        while (!ThisColumnItem.IsOk()) {
                            MessageBox.Show("Die folgende Spalte enthält einen Fehler:<br>" + ThisColumnItem.ErrorReason() + "<br><br>Bitte reparieren.", enImageCode.Information, "OK");
                            OpenColumnEditor(ThisColumnItem, null);
                        }
                    }
                }
            }
        }

        public static void OpenColumnEditor(ColumnItem column, RowItem Row, Table tableview) {
            if (column == null) { return; }
            if (Row == null) {
                OpenColumnEditor(column, tableview);
                return;
            }
            ColumnItem column2 = null;
            var PosError = false;
            switch (column.Format) {
                case enDataFormat.Columns_für_LinkedCellDropdown:
                    var Txt = Row.CellGetString(column);
                    if (int.TryParse(Txt, out var ColKey)) {
                        column2 = column.LinkedDatabase().Column.SearchByKey(ColKey);
                    }
                    break;

                case enDataFormat.LinkedCell:

                case enDataFormat.Values_für_LinkedCellDropdown:
                    (column2, _) = CellCollection.LinkedCellData(column, Row, true, false);
                    PosError = true;
                    break;
            }
            if (column2 != null) {
                if (MessageBox.Show("Welche Spalte bearbeiten?", enImageCode.Frage, "Spalte in dieser Datenbank", "Verlinkte Spalte") == 1) { column = column2; }
            } else {
                if (PosError) {
                    Notification.Show("Keine aktive Verlinkung.<br>Spalte in dieser Datenbank wird angezeigt.<br><br>Ist die Ziel-Zelle in der Ziel-Datenbank vorhanden?", enImageCode.Information);
                }
            }
            OpenColumnEditor(column, tableview);
        }

        public static void OpenColumnEditor(ColumnItem column, Table tableview) {
            using ColumnEditor w = new(column, tableview);
            w.ShowDialog();
            column.Invalidate_ColumAndContent();
        }

        /// <summary>
        /// Löst das DatabaseLoadedEvengt aus, weil es fast einem Neuladen gleichkommt.
        /// </summary>
        /// <param name="DB"></param>
        public static void OpenDatabaseHeadEditor(Database DB) {
            DB.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
            if (!DB.IsLoading) { DB.Load_Reload(); } // Die Routine wird evtl. in der Laderoutine aufgerufen. z.B. bei Fehlerhaften Regeln
            using DatabaseHeadEditor w = new(DB);
            w.ShowDialog();
            // DB.OnLoaded(new LoadedEventArgs(true));
        }

        public static void OpenLayoutEditor(Database DB, string LayoutToOpen) {
            var x = DB.ErrorReason(enErrorReason.EditNormaly);
            if (!string.IsNullOrEmpty(x)) {
                MessageBox.Show(x);
                return;
            }
            DB.CancelBackGroundWorker();
            LayoutDesigner w = new(DB);
            if (!string.IsNullOrEmpty(LayoutToOpen)) { w.LoadLayout(LayoutToOpen); }
            w.ShowDialog();
        }

        public static ItemCollectionList Vorgängervsersionen(Database db) {
            List<string> Zusatz = new();
            ItemCollectionList L = new();
            foreach (var ThisExport in db.Export) {
                if (ThisExport.Typ == enExportTyp.DatenbankOriginalFormat) {
                    foreach (var ThisString in ThisExport._BereitsExportiert) {
                        var t = ThisString.SplitBy("|");
                        if (FileExists(t[0])) {
                            var q1 = QuickImage.Get(enImageCode.Kugel, 16, Extensions.MixColor(Color.Red, Color.Green, DateTime.Now.Subtract(DateTimeParse(t[1])).TotalDays / ThisExport.AutomatischLöschen).ToHTMLCode(), "");
                            L.Add(t[1], t[0], q1, true, DataFormat.CompareKey(t[1], enDataFormat.Datum_und_Uhrzeit));
                        }
                    }
                    Zusatz.AddRange(Directory.GetFiles(ThisExport.Verzeichnis, db.Filename.FileNameWithoutSuffix() + "_*.MDB"));
                }
            }
            foreach (var ThisString in Zusatz) {
                if (L[ThisString] == null) {
                    L.Add(ThisString.FileNameWithSuffix(), ThisString, QuickImage.Get(enImageCode.Warnung), true, DataFormat.CompareKey(new FileInfo(ThisString).CreationTime.ToString(), enDataFormat.Datum_und_Uhrzeit));
                }
            }
            L.Sort();
            return L;
        }

        private void _originalDB_Disposing(object sender, System.EventArgs e) => ChangeDatabase(null);

        private void btnAdminMenu_Click(object sender, System.EventArgs e) {
            if (_TableView == null) { return; }
            AdminMenu adm = new(_TableView);
            adm.Show();
            adm.BringToFront();
        }

        private void btnClipboardImport_Click(object sender, System.EventArgs e) => _TableView.ImportClipboard();

        private void btnDatenbankKopf_Click(object sender, System.EventArgs e) => OpenDatabaseHeadEditor(_TableView.Database);

        private void btnDatenüberprüfung_Click(object sender, System.EventArgs e) {
            if (_TableView.Database == null || !_TableView.Database.IsAdministrator()) { return; }
            _TableView.Database.Row.DoAutomatic(_TableView.Filter, true, _TableView.PinnedRows, "manual check");
        }

        private void btnLayouts_Click(object sender, System.EventArgs e) {
            Develop.DebugPrint_InvokeRequired(InvokeRequired, true);
            if (_TableView.Database == null) { return; }
            OpenLayoutEditor(_TableView.Database, string.Empty);
        }

        private void btnSpaltenUebersicht_Click(object sender, System.EventArgs e) => _TableView.Database.Column.GenerateOverView();

        private void btnVorherigeVersion_Click(object sender, System.EventArgs e) {
            btnVorherigeVersion.Enabled = false;
            if (_originalDB != null && _TableView.Database != _originalDB) {
                _TableView.Database = _originalDB;
                _originalDB.Disposing -= _originalDB_Disposing;
                _originalDB = null;
                btnVorherigeVersion.Text = "Vorherige Version";
                btnVorherigeVersion.Enabled = true;
                return;
            }
            var _merker = _TableView.Database;
            var L = Vorgängervsersionen(_TableView.Database);
            if (L.Count == 0) {
                MessageBox.Show("Kein Backup vorhanden.", enImageCode.Information, "OK");
                btnVorherigeVersion.Enabled = true;
                return;
            }
            var Files = InputBoxListBoxStyle.Show("Stand wählen:", L, enAddType.None, true);
            if (Files == null || Files.Count != 1) {
                btnVorherigeVersion.Enabled = true;
                return;
            }

            _TableView.Database = Database.GetByFilename(Files[0], false);
            _originalDB = _merker;
            _originalDB.Disposing += _originalDB_Disposing;
            btnVorherigeVersion.Text = "zurück";
            btnVorherigeVersion.Enabled = true;
        }

        private void btnZeileLöschen_Click(object sender, System.EventArgs e) {
            if (!_TableView.Database.IsAdministrator()) { return; }
            var m = MessageBox.Show("Angezeigte Zeilen löschen?", enImageCode.Warnung, "Ja", "Nein");
            if (m != 0) { return; }
            _TableView.Database.Row.Remove(_TableView.Filter);
        }

        private void ChangeDatabase(Database database) {
            if (_originalDB != null) {
                _originalDB.Disposing -= _originalDB_Disposing;
            }
            _originalDB = null;
            btnVorherigeVersion.Text = "Vorherige Version";
            CheckDatabase(database, null);
            Check_OrderButtons();
        }

        private void Check_OrderButtons() {
            if (InvokeRequired) {
                Invoke(new Action(() => Check_OrderButtons()));
                return;
            }
            var enTabAllgemein = true;
            var enTabellenAnsicht = true;
            if (_TableView?.Database == null || !_TableView.Database.IsAdministrator()) {
                Enabled = false;
                return; // Weitere funktionen benötigen sicher eine Datenbank um keine Null Exception auszulösen
            }
            if (_TableView.Design != enBlueTableAppearance.Standard || !_TableView.Enabled || _TableView.Database.ReadOnly) {
                enTabellenAnsicht = false;
            }
            grpAllgemein.Enabled = enTabAllgemein;
            grpTabellenAnsicht.Enabled = enTabellenAnsicht;
            Enabled = true;
        }

        private void TableView_DatabaseChanged(object sender, System.EventArgs e) {
            ChangeDatabase(_TableView.Database);
            Check_OrderButtons();
        }

        private void TableView_EnabledChanged(object sender, System.EventArgs e) => Check_OrderButtons();

        #endregion
    }
}