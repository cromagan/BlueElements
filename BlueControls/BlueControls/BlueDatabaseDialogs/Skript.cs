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
using BlueControls.Forms;
using BlueDatabase;
using BlueDatabase.EventArgs;
using System;

namespace BlueControls.BlueDatabaseDialogs {
    public partial class Skript : Forms.Form {

        private readonly Table _BlueTable;
        private Database _Database;
        private RowItem _row = null;

        private readonly string _DidImport = string.Empty;

        public Skript(Table table) {
            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            _BlueTable = table;
            Database = _BlueTable.Database;

            _BlueTable.CursorPosChanged += CursorPosChanged;
            _BlueTable.DatabaseChanged += _BlueTable_DatabaseChanged;
            //_BlueTable.DatabaseChanging += _BlueTable_DatabaseChanging;

            CursorPosChanged(_BlueTable, new CellEventArgs(_BlueTable.CursorPosColumn(), _BlueTable.CursorPosRow()));
        }

        private void WriteBack() {
            if (_Database != null) {
                _Database.ImportScript = txbImportSkript.Text.ToNonCritical();
            }

        }

        private void FillNow() {
            if (_Database == null) {
                txbImportSkript.Text = string.Empty;
                txbImportSkript.Enabled = false;
            } else {
                txbImportSkript.Enabled = _Database.IsAdministrator();
                txbImportSkript.Text = _Database.ImportScript.FromNonCritical();

                chkFehlgeschlageneSpalten.Visible = _Database.IsAdministrator();
                chkFehlgeschlageneSpalten.Enabled = _Database.IsAdministrator();

                if (!_Database.IsAdministrator()) {
                    chkFehlgeschlageneSpalten.Checked = false;
                }
            }



        }

        private void _BlueTable_DatabaseChanged(object sender, System.EventArgs e) {
            Database = _BlueTable.Database;
        }

        public Database Database {
            get {
                return _Database;
            }
            set {

                if (_Database == value) { return; }

                WriteBack();

                if (_Database != null) {

                    _Database.Loaded -= _DatabaseLoaded;
                    _Database.Save(false); // Datenbank nicht reseten, weil sie ja anderweitig noch benutzt werden kann

                }
                _Database = value;
                FillNow();

                if (_Database != null) {
                    _Database.Loaded += _DatabaseLoaded;
                }

            }
        }


        private void CursorPosChanged(object sender, CellEventArgs e) {
            var ok = true;
            if (e.Row == null) {
                optVorhandenZeile.Text = "Aktuell angewählte Zeile überschreiben";
                ok = false;
            } else {
                optVorhandenZeile.Text = "Aktuell angewählte Zeile  <b>(" + e.Row.CellFirstString() + ")</b> überschreiben";

                if (!Database.IsAdministrator()) {
                    foreach (var ThisColumn in Database.Column) {
                        if (ThisColumn != Database.Column.SysRowChangeDate && ThisColumn != Database.Column.SysRowChanger) {
                            if (!CellCollection.UserEditPossible(ThisColumn, e.Row, enErrorReason.EditGeneral)) { ok = false; }
                        }
                    }
                }



                if (e.Row.CellGetBoolean(Database.Column.SysLocked)) { ok = false; }


            }

            optVorhandenZeile.Enabled = ok;

            if (!ok) {
                optVorhandenZeile.Checked = false;
                _row = null;
            } else {
                _row = e.Row;
            }
        }


        protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e) {
            Database = null;
            _BlueTable.CursorPosChanged -= CursorPosChanged;
            _BlueTable.DatabaseChanged -= _BlueTable_DatabaseChanged;
            //_BlueTable.DatabaseChanging -= _BlueTable_DatabaseChanging;
            base.OnFormClosing(e);

        }



        private void _DatabaseLoaded(object sender, LoadedEventArgs e) {
            FillNow();
        }

        private void btnClipboard_Click(object sender, System.EventArgs e) {

            WriteBack();

            if (Database == null) {
                MessageBox.Show("Keine Datenbank angewählt, abbruch.", enImageCode.Information, "OK");
                return;
            }


            if (!opbNeueZeile.Checked && !optVorhandenZeile.Checked) {
                MessageBox.Show("Vorher anwählen, ob eine neue Zeile gewünscht ist, abbruch.", enImageCode.Information, "OK");
                return;
            }

            if (opbNeueZeile.Checked && optVorhandenZeile.Checked) {
                MessageBox.Show("Zu viele Zeilen-Optionen, abbruch.", enImageCode.Information, "OK");
                Develop.DebugPrint(enFehlerArt.Warnung, "Eigentlich nicht möglich");
                return;
            }

            if (!System.Windows.Forms.Clipboard.ContainsText()) {
                MessageBox.Show("Kein Text im Zwischenspeicher, abbruch.", enImageCode.Information, "OK");
                return;
            }


            var nt = Convert.ToString(System.Windows.Forms.Clipboard.GetDataObject().GetData(System.Windows.Forms.DataFormats.Text));


            if (string.IsNullOrEmpty(nt)) {
                MessageBox.Show("Nur leerer Text im Zwischenspeicher, abbruch.", enImageCode.Information, "OK");
                return;
            }

            if (optVorhandenZeile.Checked && _row == null) {
                MessageBox.Show("Keine Zeile angewählt, abbruch.", enImageCode.Information, "OK");
                return;
            }




            var _newdid = nt + "\r" + _DidImport;

            if (_newdid == _DidImport && opbNeueZeile.Checked) {
                if (MessageBox.Show("Es wurde bereits eine neue Zeile mit diesen Werten angelegt.\r<b>Noch eine anlegen?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
            }




            string feh;


            if (opbNeueZeile.Checked) {
                feh = Database.DoImportScript(nt, null, chkFehlgeschlageneSpalten.Checked);
            } else {
                feh = Database.DoImportScript(nt, _row, chkFehlgeschlageneSpalten.Checked);
            }


            if (!string.IsNullOrEmpty(feh)) {
                MessageBox.Show("Fehler während es Imports:<br>" + feh, enImageCode.Warnung, "OK");
                return;
            } else {
                MessageBox.Show("Erfolgreich!", enImageCode.Smiley, "OK");
                return;
            }
        }
    }
}
