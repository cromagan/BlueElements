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

#nullable enable

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
using BlueBasics.EventArgs;
using BlueControls.ItemCollection.ItemCollectionList;
using System.Threading.Tasks;
using System.Drawing;

namespace BlueControls.Forms {

    public partial class TableView : Form {

        #region Fields

        private Database? _originalDb;

        #endregion

        #region Constructors

        public TableView() {
            InitializeComponent();
            Check_OrderButtons();
        }

        #endregion

        //[DefaultValue((Table?)null)]
        //public Table? Table {
        //    get => TableView;
        //    set {
        //        if (TableView == value) { return; }
        //        if (TableView != null) {
        //            TableView.DatabaseChanged -= TableView_DatabaseChanged;
        //            TableView.EnabledChanged -= TableView_EnabledChanged;
        //            ChangeDatabase(null);
        //        }
        //        TableView = value;
        //        Check_OrderButtons();
        //        if (TableView == null) {
        //            return;
        //        }

        //        ChangeDatabase(TableView.Database);
        //        TableView.DatabaseChanged += TableView_DatabaseChanged;
        //        TableView.EnabledChanged += TableView_EnabledChanged;
        //    }
        //}

        #region Methods

        public static void CheckDatabase(object? sender, LoadedEventArgs? e) {
            if (sender is Database database && !database.ReadOnly) {
                if (database.IsAdministrator()) {
                    foreach (var thisColumnItem in database.Column) {
                        while (!thisColumnItem.IsOk()) {
                            DebugPrint(enFehlerArt.Info, "Datenbank:" + database.Filename + "\r\nSpalte:" + thisColumnItem.Name + "\r\nSpaltenfehler: " + thisColumnItem.ErrorReason() + "\r\nUser: " + database.UserName + "\r\nGroup: " + database.UserGroup + "\r\nAdmins: " + database.DatenbankAdmin.JoinWith(";"));
                            MessageBox.Show("Die folgende Spalte enthält einen Fehler:<br>" + thisColumnItem.ErrorReason() + "<br><br>Bitte reparieren.", enImageCode.Information, "OK");
                            OpenColumnEditor(thisColumnItem, null);
                        }
                    }
                }
            }
        }

        public static void OpenColumnEditor(ColumnItem? column, RowItem? row, Table? tableview) {
            if (column == null) { return; }
            if (row == null) {
                OpenColumnEditor(column, tableview);
                return;
            }
            ColumnItem? columnLinked = null;
            var posError = false;
            switch (column.Format) {
                //case enDataFormat.Columns_für_LinkedCellDropdown:
                //    var txt = row.CellGetString(column);
                //    if (IntTryParse(txt, out var colKey)) {
                //        columnLinked = column.LinkedDatabase().Column.SearchByKey(colKey);
                //    }
                //    break;

                //case enDataFormat.Verknüpfung_zu_anderer_Datenbank_Skriptgesteuert:
                case BlueBasics.Enums.DataFormat.Verknüpfung_zu_anderer_Datenbank:
                case BlueBasics.Enums.DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems:
                    (columnLinked, _, _) = CellCollection.LinkedCellData(column, row, true, false);
                    posError = true;
                    break;
            }

            var bearbColumn = column;
            if (columnLinked != null) {
                columnLinked.Repair();
                if (MessageBox.Show("Welche Spalte bearbeiten?", enImageCode.Frage, "Spalte in dieser Datenbank", "Verlinkte Spalte") == 1) { bearbColumn = columnLinked; }
            } else {
                if (posError) {
                    Notification.Show("Keine aktive Verlinkung.<br>Spalte in dieser Datenbank wird angezeigt.<br><br>Ist die Ziel-Zelle in der Ziel-Datenbank vorhanden?", enImageCode.Information);
                }
            }
            column.Repair();
            OpenColumnEditor(bearbColumn, tableview);
            bearbColumn.Repair();
        }

        public static void OpenColumnEditor(ColumnItem? column, Table? tableview) {
            using ColumnEditor w = new(column, tableview);
            w.ShowDialog();
            column?.Invalidate_ColumAndContent();
        }

        /// <summary>
        /// Löst das DatabaseLoadedEvengt aus, weil es fast einem Neuladen gleichkommt.
        /// </summary>
        /// <param name="db"></param>
        public static void OpenDatabaseHeadEditor(Database db) {
            db.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
            if (!db.IsLoading) { db.Load_Reload(); } // Die Routine wird evtl. in der Laderoutine aufgerufen. z.B. bei Fehlerhaften Regeln
            using DatabaseHeadEditor w = new(db);
            w.ShowDialog();
            // DB.OnLoaded(new LoadedEventArgs(true));
        }

        public static void OpenLayoutEditor(Database db, string layoutToOpen) {
            var x = db.ErrorReason(enErrorReason.EditNormaly);
            if (!string.IsNullOrEmpty(x)) {
                MessageBox.Show(x);
                return;
            }
            db.CancelBackGroundWorker();
            LayoutPadEditor w = new(db);
            if (!string.IsNullOrEmpty(layoutToOpen)) { w.LoadLayout(layoutToOpen); }
            w.ShowDialog();
        }

        public static ItemCollectionList Vorgängerversionen(Database db) {
            List<string> zusatz = new();
            ItemCollectionList l = new();
            foreach (var thisExport in db.Export) {
                if (thisExport.Typ == ExportTyp.DatenbankOriginalFormat) {
                    var lockMe = new object();
                    Parallel.ForEach(thisExport.BereitsExportiert, (thisString, _) => {
                        var t = thisString.SplitAndCutBy("|");
                        if (FileExists(t[0])) {
                            var q1 = QuickImage.Get(enImageCode.Kugel, 16, Color.Red.MixColor(Color.Green, DateTime.Now.Subtract(DateTimeParse(t[1])).TotalDays / thisExport.AutoDelete).ToHtmlCode(), "");
                            lock (lockMe) {
                                l.Add(t[1], t[0], q1, true, t[1].CompareKey(enSortierTyp.Datum_Uhrzeit));
                            }
                        }
                    });

                    //foreach (var ThisString in ThisExport.BereitsExportiert) {
                    //    var t = ThisString.SplitAndCutBy("|");
                    //    if (FileExists(t[0])) {
                    //        var q1 = QuickImage.Get(enImageCode.Kugel, 16, Extensions.MixColor(Color.Red, Color.Green, DateTime.Now.Subtract(DateTimeParse(t[1])).TotalDays / ThisExport.AutoDelete).ToHTMLCode(), "");
                    //        L.Add(t[1], t[0], q1, true, t[1].CompareKey(enSortierTyp.Datum_Uhrzeit));
                    //    }
                    //}
                    zusatz.AddRange(Directory.GetFiles(thisExport.Verzeichnis, db.Filename.FileNameWithoutSuffix() + "_*.MDB"));
                }
            }
            foreach (var thisString in zusatz.Where(thisString => l[thisString] == null)) {
                l.Add(thisString.FileNameWithSuffix(), thisString, QuickImage.Get(enImageCode.Warnung), true, new FileInfo(thisString).CreationTime.ToString().CompareKey(enSortierTyp.Datum_Uhrzeit));
            }
            l.Sort();
            return l;
        }

        public void DatabaseSet(Database? database) {
            Table.Database = database;
            Formula.Database = database;
            FilterLeiste.Table = Table;

            //if (Table.Database == null) {
            //    SetDatabasetoNothing();
            //} else {
            //    //if (Table.Database.Ansicht != Ansicht.Unverändert) {
            //    //    _ansicht = Table.Database.Ansicht;
            //    //}
            //}
            //InitView();
            //CheckButtons();
            //CaptionAnzeige();
            //CheckButtons();
            if (Table.View_RowFirst() != null) {
                Table.CursorPos_Set(Table.Database.Column[0], Table.View_RowFirst(), false);
            }
        }

        protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e) {
            DatabaseSet(null);
            BlueBasics.MultiUserFile.MultiUserFile.SaveAll(true);
            base.OnFormClosing(e);
        }

        private void _originalDB_Disposing(object sender, System.EventArgs e) => ChangeDatabase(null);

        private void btnAlleErweitern_Click(object sender, System.EventArgs e) => Table.ExpandAll();

        private void btnAlleSchließen_Click(object sender, System.EventArgs e) => Table.CollapesAll();

        private void btnClipboardImport_Click(object sender, System.EventArgs e) => Table.ImportClipboard();

        private void btnDatenbankKopf_Click(object sender, System.EventArgs e) => OpenDatabaseHeadEditor(Table.Database);

        private void btnDatenüberprüfung_Click(object sender, System.EventArgs e) {
            if (Table.Database == null || !Table.Database.IsAdministrator()) { return; }
            Table.Database.Row.DoAutomatic(Table.Filter, true, Table.PinnedRows, "manual check");
        }

        private void btnLayouts_Click(object sender, System.EventArgs e) {
            DebugPrint_InvokeRequired(InvokeRequired, true);
            if (Table.Database == null) { return; }
            OpenLayoutEditor(Table.Database, string.Empty);
        }

        private void btnPowerBearbeitung_Click(object sender, System.EventArgs e) {
            Notification.Show("20 Sekunden (fast) rechtefreies<br>Vearbeiten akiviert.", enImageCode.Stift);
            Table.PowerEdit = DateTime.Now.AddSeconds(20);
        }

        private void btnSpaltenanordnung_Click(object sender, System.EventArgs e) {
            var x = new frmColumnArrangementPadEditor(Table.Database);
            x.ShowDialog();
            Table.Database.ColumnArrangements[0].ShowAllColumns();
            Table.Invalidate_HeadSize();
            Table.Invalidate_AllColumnArrangements();
        }

        private void btnSpaltenUebersicht_Click(object sender, System.EventArgs e) => Table.Database.Column.GenerateOverView();

        private void btnUnterschiede_CheckedChanged(object sender, System.EventArgs e) => Table.Unterschiede = btnUnterschiede.Checked ? Table.CursorPosRow().Row : null;

        private void btnVorherigeVersion_Click(object sender, System.EventArgs e) {
            btnVorherigeVersion.Enabled = false;
            if (_originalDb != null && Table.Database != _originalDb) {
                _originalDb.Disposing -= _originalDB_Disposing;
                Table.Database = _originalDb;
                _originalDb = null;
                btnVorherigeVersion.Text = "Vorherige Version";
                btnVorherigeVersion.Enabled = true;
                return;
            }
            var merker = Table.Database;
            var l = Vorgängerversionen(Table.Database);
            if (l.Count == 0) {
                MessageBox.Show("Kein Backup vorhanden.", enImageCode.Information, "OK");
                btnVorherigeVersion.Enabled = true;
                return;
            }
            var files = InputBoxListBoxStyle.Show("Stand wählen:", l, AddType.None, true);
            if (files == null || files.Count != 1) {
                btnVorherigeVersion.Enabled = true;
                return;
            }

            Table.Database = Database.GetByFilename(files[0], false, true);
            _originalDb = merker;
            _originalDb.Disposing += _originalDB_Disposing;
            btnVorherigeVersion.Text = "zurück";
            btnVorherigeVersion.Enabled = true;
        }

        private void btnZeileLöschen_Click(object sender, System.EventArgs e) {
            if (!Table.Database.IsAdministrator()) { return; }
            var m = MessageBox.Show("Angezeigte Zeilen löschen?", enImageCode.Warnung, "Ja", "Nein");
            if (m != 0) { return; }
            Table.Database.Row.Remove(Table.Filter, Table.PinnedRows);
        }

        private void cbxColumnArr_ItemClicked(object sender, BasicListItemEventArgs e) {
            if (string.IsNullOrEmpty(cbxColumnArr.Text)) { return; }
            Table.Arrangement = int.Parse(e.Item.Internal);
        }

        private void ChangeDatabase(Database? database) {
            if (_originalDb != null) {
                _originalDb.Disposing -= _originalDB_Disposing;
            }
            _originalDb = null;
            btnVorherigeVersion.Text = "Vorherige Version";
            CheckDatabase(database, null);
            Check_OrderButtons();
        }

        private void Check_OrderButtons() {
            if (InvokeRequired) {
                Invoke(new Action(Check_OrderButtons));
                return;
            }
            const bool enTabAllgemein = true;
            var enTabellenAnsicht = true;
            if (Table.Database == null || !Table.Database.IsAdministrator()) {
                tabAdmin.Enabled = false;
                return; // Weitere funktionen benötigen sicher eine Datenbank um keine Null Exception auszulösen
            }
            if (Table.Design != BlueTableAppearance.Standard || !Table.Enabled || Table.Database.ReadOnly) {
                enTabellenAnsicht = false;
            }
            grpAdminAllgemein.Enabled = enTabAllgemein;
            grpAdminImport.Enabled = enTabellenAnsicht;
            tabAdmin.Enabled = true;
        }

        private void Table_CursorPosChanged(object sender, CellExtEventArgs e) {
            if (InvokeRequired) {
                Invoke(new Action(() => Table_CursorPosChanged(sender, e)));
                return;
            }

            btnUnterschiede_CheckedChanged(null, System.EventArgs.Empty);

            if (e.Column != null) { Formula.Database = e.Column.Database; }
            if (e.RowData?.Row != null) { Formula.Database = e.RowData.Row.Database; }

            Formula.ShowingRowKey = e.Column == null || e.RowData?.Row == null ? -1 : e.RowData.Row.Key;
        }

        private void Table_ViewChanged(object sender, System.EventArgs e) => Table.WriteColumnArrangementsInto(cbxColumnArr, Table.Database, Table.Arrangement);

        private void Table_VisibleRowsChanged(object sender, System.EventArgs e) {
            if (InvokeRequired) {
                Invoke(new Action(() => Table_VisibleRowsChanged(sender, e)));
                return;
            }

            capZeilen1.Text = "<IMAGECODE=Information|16> " + LanguageTool.DoTranslate("Einzigartige Zeilen:") + " " + Table.Database.Row.VisibleRowCount + " " + LanguageTool.DoTranslate("St.");
            capZeilen1.Refresh(); // Backgroundworker lassen wenig luft
            capZeilen2.Text = capZeilen1.Text;
            capZeilen2.Refresh();
        }

        private void TableView_DatabaseChanged(object sender, System.EventArgs e) {
            Table.WriteColumnArrangementsInto(cbxColumnArr, Table.Database, Table.Arrangement);
            ChangeDatabase(Table.Database);
            Check_OrderButtons();
        }

        private void TableView_EnabledChanged(object sender, System.EventArgs e) => Check_OrderButtons();

        #endregion
    }
}