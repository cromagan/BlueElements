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

using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.ItemCollection;
using BlueDatabase;
using System;
using static BlueBasics.Converter;

namespace BlueControls.BlueDatabaseDialogs {

    public partial class frmColumnArrangementPadEditor : PadEditor {

        #region Fields

        public static Database Database = null;

        private int Arrangement = -1;

        #endregion

        #region Constructors

        public frmColumnArrangementPadEditor(Database database) : this() {
            Database = database;
            Database.ShouldICancelSaveOperations += TmpDatabase_ShouldICancelDiscOperations;
            Arrangement = 1;
            UpdateCombobox();
            ShowOrder();
        }

        private frmColumnArrangementPadEditor() => InitializeComponent();

        #endregion

        #region Properties

        public ColumnViewCollection CurrentArrangement => Database == null || Database.ColumnArrangements == null || Database.ColumnArrangements.Count <= Arrangement
                    ? null
                    : Database.ColumnArrangements[Arrangement];

        #endregion

        #region Methods

        protected override void OnFormClosed(System.Windows.Forms.FormClosedEventArgs e) {
            Database.ShouldICancelSaveOperations -= TmpDatabase_ShouldICancelDiscOperations;
            base.OnFormClosed(e);
        }

        private void btnAktuelleAnsichtLoeschen_Click(object sender, System.EventArgs e) {
            if (Arrangement < 2 || Arrangement >= Database.ColumnArrangements.Count) { return; }
            if (MessageBox.Show("Anordung <b>'" + CurrentArrangement.Name + "'</b><br>wirklich löschen?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
            Database.ColumnArrangements.RemoveAt(Arrangement);
            Arrangement = 1;
            UpdateCombobox();
            ShowOrder();
        }

        private void btnAlleSpaltenEinblenden_Click(object sender, System.EventArgs e) {
            if (MessageBox.Show("Alle Spalten anzeigen?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
            CurrentArrangement.ShowAllColumns();
            ShowOrder();
        }

        private void btnAnsichtUmbenennen_Click(object sender, System.EventArgs e) {
            var n = InputBox.Show("Umbenennen:", CurrentArrangement.Name, enVarType.Text);
            if (!string.IsNullOrEmpty(n)) { CurrentArrangement.Name = n; }
            UpdateCombobox();
        }

        private void btnBerechtigungsgruppen_Click(object sender, System.EventArgs e) {
            ItemCollectionList aa = new();
            aa.AddRange(Database.Permission_AllUsed(true));
            aa.Sort();
            aa.CheckBehavior = enCheckBehavior.MultiSelection;
            aa.Check(CurrentArrangement.PermissionGroups_Show, true);
            var b = InputBoxListBoxStyle.Show("Wählen sie, wer anzeigeberechtigt ist:<br><i>Info: Administratoren sehen alle Ansichten", aa, enAddType.Text, true);
            if (b == null) { return; }
            CurrentArrangement.PermissionGroups_Show.Clear();
            CurrentArrangement.PermissionGroups_Show.AddRange(b.ToArray());
            if (Arrangement == 1) { CurrentArrangement.PermissionGroups_Show.Add("#Everybody"); }
        }

        private void btnNeueAnsichtErstellen_Click(object sender, System.EventArgs e) {
            var MitVorlage = false;
            if (Arrangement > 0 && CurrentArrangement != null) {
                MitVorlage = Convert.ToBoolean(MessageBox.Show("<b>Neue Spaltenanordnung erstellen:</b><br>Wollen sie die aktuelle Ansicht kopieren?", enImageCode.Frage, "Ja", "Nein") == 0);
            }
            if (Database.ColumnArrangements.Count < 1) {
                Database.ColumnArrangements.Add(new ColumnViewCollection(Database, "", ""));
            }
            string newname;
            if (MitVorlage) {
                newname = InputBox.Show("Die aktuelle Ansicht wird <b>kopiert</b>.<br><br>Geben sie den Namen<br>der neuen Anordnung ein:", "", enVarType.Text);
                if (string.IsNullOrEmpty(newname)) { return; }
                Database.ColumnArrangements.Add(new ColumnViewCollection(Database, CurrentArrangement.ToString(), newname));
            } else {
                newname = InputBox.Show("Geben sie den Namen<br>der neuen Anordnung ein:", "", enVarType.Text);
                if (string.IsNullOrEmpty(newname)) { return; }
                Database.ColumnArrangements.Add(new ColumnViewCollection(Database, "", newname));
            }
            UpdateCombobox();
            ShowOrder();
        }

        private void btnSpalteEinblenden_Click(object sender, System.EventArgs e) {
            ItemCollectionList ic = new();
            foreach (var ThisColumnItem in Database.Column) {
                if (ThisColumnItem != null && CurrentArrangement[ThisColumnItem] == null) { ic.Add(ThisColumnItem); }
            }
            if (ic.Count == 0) {
                MessageBox.Show("Es werden bereits alle<br>Spalten angezeigt.", enImageCode.Information, "Ok");
                return;
            }
            ic.Sort();
            var r = InputBoxListBoxStyle.Show("Wählen sie:", ic, enAddType.None, true);
            if (r == null || r.Count == 0) { return; }
            CurrentArrangement.Add(Database.Column.SearchByKey(LongParse(r[0])), false);
            ShowOrder();
        }

        private void btnSystemspaltenAusblenden_Click(object sender, System.EventArgs e) {
            CurrentArrangement.HideSystemColumns();
            ShowOrder();
        }

        private void cbxInternalColumnArrangementSelector_ItemClicked(object sender, BasicListItemEventArgs e) {
            if (string.IsNullOrEmpty(cbxInternalColumnArrangementSelector.Text)) { return; }

            var tmporder = int.Parse(e.Item.Internal);

            if (Arrangement == tmporder) { return; }

            Arrangement = tmporder;
            ShowOrder();
        }

        private void ShowOrder() => Pad.Item.Clear();//foreach (var thisc in CurrentArrangement) {//    var it = new TextPadItem(Pad.Item, thisc.Column.Name, thisc.Column.ReadableText);//    it.SetCoordinates(new RectangleF)//        Pad.Item.Add(it)//}

        private void TmpDatabase_ShouldICancelDiscOperations(object sender, System.ComponentModel.CancelEventArgs e) => e.Cancel = true;

        private void UpdateCombobox() => Table.WriteColumnArrangementsInto(cbxInternalColumnArrangementSelector, Database, Arrangement);

        #endregion
    }
}