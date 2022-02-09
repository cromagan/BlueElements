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
using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.ItemCollection;
using BlueDatabase;
using System.ComponentModel;
using static BlueBasics.FileOperations;

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;

using BlueControls.Enums;

using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.ItemCollection;
using BlueDatabase;

using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using System;
using static BlueBasics.Converter;

using static BlueBasics.FileOperations;

namespace BlueControls.BlueDatabaseDialogs {

    public partial class frmColumnArrangementPadEditor : PadEditor {

        #region Fields

        public static Database Database = null;

        #endregion

        #region Constructors

        public frmColumnArrangementPadEditor(Database database) : this() {
            Database = database;
            Database.ShouldICancelSaveOperations += TmpDatabase_ShouldICancelDiscOperations;

            _TableView.WriteColumnArrangementsInto(cbxInternalColumnArrangementSelector);
        }

        private frmColumnArrangementPadEditor() {
            InitializeComponent();
        }

        #endregion

        #region Methods

        protected override void OnFormClosed(System.Windows.Forms.FormClosedEventArgs e) {
            Database.ShouldICancelSaveOperations -= TmpDatabase_ShouldICancelDiscOperations;
            base.OnFormClosed(e);
        }

        private void btnAktuelleAnsichtLoeschen_Click(object sender, System.EventArgs e) {
            if (_TableView.Arrangement < 2 || _TableView.Arrangement >= Database.ColumnArrangements.Count) { return; }
            if (MessageBox.Show("Anordung <b>'" + _TableView.CurrentArrangement.Name + "'</b><br>wirklich löschen?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
            Database.ColumnArrangements.RemoveAt(_TableView.Arrangement);
            _TableView.Arrangement = 1;
        }

        private void btnAlleSpaltenEinblenden_Click(object sender, System.EventArgs e) {
            if (MessageBox.Show("Alle Spalten anzeigen?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
            _TableView.CurrentArrangement.ShowAllColumns();
        }

        private void btnAnsichtUmbenennen_Click(object sender, System.EventArgs e) {
            var n = InputBox.Show("Umbenennen:", _TableView.CurrentArrangement.Name, enVarType.Text);
            if (!string.IsNullOrEmpty(n)) { _TableView.CurrentArrangement.Name = n; }
        }

        private void btnBerechtigungsgruppen_Click(object sender, System.EventArgs e) {
            ItemCollectionList aa = new();
            aa.AddRange(Database.Permission_AllUsed(true));
            aa.Sort();
            aa.CheckBehavior = enCheckBehavior.MultiSelection;
            aa.Check(_TableView.CurrentArrangement.PermissionGroups_Show, true);
            var b = InputBoxListBoxStyle.Show("Wählen sie, wer anzeigeberechtigt ist:<br><i>Info: Administratoren sehen alle Ansichten", aa, enAddType.Text, true);
            if (b == null) { return; }
            _TableView.CurrentArrangement.PermissionGroups_Show.Clear();
            _TableView.CurrentArrangement.PermissionGroups_Show.AddRange(b.ToArray());
            if (_TableView.Arrangement == 1) { _TableView.CurrentArrangement.PermissionGroups_Show.Add("#Everybody"); }
        }

        private void btnNeueAnsichtErstellen_Click(object sender, System.EventArgs e) {
            var MitVorlage = false;
            if (_TableView.Arrangement > 0 && _TableView.CurrentArrangement != null) {
                MitVorlage = Convert.ToBoolean(MessageBox.Show("<b>Neue Spaltenanordnung erstellen:</b><br>Wollen sie die aktuelle Ansicht kopieren?", enImageCode.Frage, "Ja", "Nein") == 0);
            }
            if (Database.ColumnArrangements.Count < 1) {
                Database.ColumnArrangements.Add(new ColumnViewCollection(Database, "", ""));
            }
            string newname;
            if (MitVorlage) {
                newname = InputBox.Show("Die aktuelle Ansicht wird <b>kopiert</b>.<br><br>Geben sie den Namen<br>der neuen Anordnung ein:", "", enVarType.Text);
                if (string.IsNullOrEmpty(newname)) { return; }
                Database.ColumnArrangements.Add(new ColumnViewCollection(Database, _TableView.CurrentArrangement.ToString(), newname));
            } else {
                newname = InputBox.Show("Geben sie den Namen<br>der neuen Anordnung ein:", "", enVarType.Text);
                if (string.IsNullOrEmpty(newname)) { return; }
                Database.ColumnArrangements.Add(new ColumnViewCollection(Database, "", newname));
            }
        }

        private void btnSpalteEinblenden_Click(object sender, System.EventArgs e) {
            ItemCollectionList ic = new();
            foreach (var ThisColumnItem in Database.Column) {
                if (ThisColumnItem != null && _TableView.CurrentArrangement[ThisColumnItem] == null) { ic.Add(ThisColumnItem); }
            }
            if (ic.Count == 0) {
                MessageBox.Show("Es werden bereits alle<br>Spalten angezeigt.", enImageCode.Information, "Ok");
                return;
            }
            ic.Sort();
            var r = InputBoxListBoxStyle.Show("Wählen sie:", ic, enAddType.None, true);
            if (r == null || r.Count == 0) { return; }
            _TableView.CurrentArrangement.Add(Database.Column.SearchByKey(LongParse(r[0])), false);
            _TableView.Invalidate_HeadSize();
        }

        private void btnSystemspaltenAusblenden_Click(object sender, System.EventArgs e) => _TableView.CurrentArrangement.HideSystemColumns();

        private void cbxInternalColumnArrangementSelector_ItemClicked(object sender, BasicListItemEventArgs e) {
            if (string.IsNullOrEmpty(cbxInternalColumnArrangementSelector.Text)) { return; }
            _TableView.Arrangement = int.Parse(e.Item.Internal);
            Check_OrderButtons();
        }

        private void TmpDatabase_ShouldICancelDiscOperations(object sender, System.ComponentModel.CancelEventArgs e) => e.Cancel = true;

        #endregion
    }
}