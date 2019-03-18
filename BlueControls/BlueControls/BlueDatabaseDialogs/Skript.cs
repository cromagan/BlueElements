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
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.DialogBoxes;
using BlueDatabase;
using BlueDatabase.EventArgs;

namespace BlueControls.BlueDatabaseDialogs
{
    public partial class Skript : Forms.Form
    {

        private readonly Table _BlueTable;
        private Database _Database;
        private RowItem _row = null;

        public Skript(Table table)
        {
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

        private void WriteBack()
        {
            if (_Database != null)
            {
                _Database.ImportScript = txbImportSkript.Text.ToNonCritical();
            }

        }

        private void FillNow()
        {
            if (_Database == null)
            {
                txbImportSkript.Text = string.Empty;
                txbImportSkript.Enabled = false;
            }
            else
            {
                txbImportSkript.Enabled = true;
                txbImportSkript.Text = _Database.ImportScript.FromNonCritical();
            }
        }

        private void _BlueTable_DatabaseChanged(object sender, System.EventArgs e)
        {
            Database = _BlueTable.Database;
        }

        public Database Database
        {
            get
            {
                return _Database;
            }
            set
            {

                if (_Database == value) { return; }

                WriteBack();

                if (_Database != null)
                {

                    _Database.Loaded -= _DatabaseLoaded;
                    _Database.Release(false); // Datenbank nicht reseten, weil sie ja anderweitig noch benutzt werden kann

                }
                _Database = value;
                FillNow();

                if (_Database != null)
                {
                    _Database.Loaded += _DatabaseLoaded;
                }

            }
        }


        private void CursorPosChanged(object sender, CellEventArgs e)
        {


            if (e.Row == null)
            {
                optVorhandenZeile.Enabled = false;
                optVorhandenZeile.Text = "Aktuell angewählte Zeile überschreiben";
                optVorhandenZeile.Checked = false;
                _row = null;
            }
            else
            {
                optVorhandenZeile.Enabled = true;
                optVorhandenZeile.Text = "Aktuell angewählte Zeile  <b>(" + e.Row.CellFirstString() + ")</b> überschreiben";
                _row = e.Row;
            }

            //if (e.Column == null)
            //{
            //    NurinAktuellerSpalte.Text = "Nur in der <b>aktuell gewählten Spalte</b> ersetzen.";
            //}
            //else
            //{
            //    NurinAktuellerSpalte.Text = "Nur in Spalte <b>'" + e.Column.ReadableText() + "'</b> ersetzen.";
            //}

            //Checkbuttons();
        }


        protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e)
        {
            Database = null;
            _BlueTable.CursorPosChanged -= CursorPosChanged;
            _BlueTable.DatabaseChanged -= _BlueTable_DatabaseChanged;
            //_BlueTable.DatabaseChanging -= _BlueTable_DatabaseChanging;
            base.OnFormClosing(e);

        }



        private void _DatabaseLoaded(object sender, LoadedEventArgs e)
        {
            FillNow();
        }

        private void btnClipboard_Click(object sender, System.EventArgs e)
        {

            WriteBack();

            if (Database == null)
            {
                MessageBox.Show("Keine Datenbank angewählt, abbruch.", enImageCode.Information, "OK");
                return;
            }


            if (!opbNeueZeile.Checked && !optVorhandenZeile.Checked)
            {
                MessageBox.Show("Vorher anwählen, ob eine neue Zeile gewünscht ist, abbruch.", enImageCode.Information, "OK");
                return;
            }

            if (opbNeueZeile.Checked && optVorhandenZeile.Checked)
            {
                MessageBox.Show("Zu viele Zeilen-optionen, abbruch.", enImageCode.Information, "OK");
                Develop.DebugPrint(enFehlerArt.Warnung, "Eigentlich nicht möglich");
                return;
            }

            if (!System.Windows.Forms.Clipboard.ContainsText())
            {
                MessageBox.Show("Kkein Text im Clipboard, abbruch.", enImageCode.Information, "OK");
                return;
            }


            var nt = Convert.ToString(System.Windows.Forms.Clipboard.GetDataObject().GetData(System.Windows.Forms.DataFormats.Text));


            if (string.IsNullOrEmpty(nt))
            {
                MessageBox.Show("Nur leerer Text im Clipboard, abbruch.", enImageCode.Information, "OK");
                return;
            }

            if (optVorhandenZeile.Checked && _row == null)
            {
                MessageBox.Show("Keine Zeile angewählt, abbruch.", enImageCode.Information, "OK");
                return;
            }


            string feh;


            if (opbNeueZeile.Checked)
            {
                feh = Database.DoImportScript(nt, null, chkFehlgeschlageneSpalten.Checked);
            }
            else
            {
                feh = Database.DoImportScript(nt, _row, chkFehlgeschlageneSpalten.Checked);
            }


            if (!string.IsNullOrEmpty(feh))
            {
                MessageBox.Show("Fehler während es Imports:<br>" + feh, enImageCode.Warnung, "OK");
                return;
            }
            else
            {
                MessageBox.Show("Erfolgreich!", enImageCode.Smiley, "OK");
                return;
            }



        }


    }
}
