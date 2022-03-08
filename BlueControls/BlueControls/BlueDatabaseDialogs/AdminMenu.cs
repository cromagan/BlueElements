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
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using System;
using System.Linq;
using BlueControls.ItemCollection.ItemCollectionList;
using static BlueBasics.Converter;
using static BlueBasics.FileOperations;

namespace BlueControls.BlueDatabaseDialogs {

    public sealed partial class AdminMenu : Form {

        #region Fields

        private readonly Table? _tableView;
        private Database? _tmpDatabase;

        #endregion

        #region Constructors

        public AdminMenu(Table? table) {
            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();
            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            _tableView = table;
            if (_tableView != null) {
                _tableView.DatabaseChanged += _TableView_DatabaseChanged;
                _tableView.CursorPosChanged += _TableView_CursorPosChanged;
                _tableView.ViewChanged += _TableView_ViewChanged;
                _tableView.EnabledChanged += _TableView_EnabledChanged;
            }
            SetDatabase(_tableView?.Database);
            UpdateViewControls();
            Check_OrderButtons();
        }

        #endregion

        #region Methods

        protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e) {
            SetDatabase(null);
            if (_tableView != null) {
                _tableView.DatabaseChanged -= _TableView_DatabaseChanged;
                _tableView.CursorPosChanged -= _TableView_CursorPosChanged;
                _tableView.ViewChanged -= _TableView_ViewChanged;
                _tableView.EnabledChanged -= _TableView_EnabledChanged;
            }
            base.OnFormClosing(e);
        }

        private static void TmpDatabase_ShouldICancelDiscOperations(object sender, System.ComponentModel.CancelEventArgs e) => e.Cancel = true;

        private void _TableView_CursorPosChanged(object sender, CellExtEventArgs e) => Check_OrderButtons();

        private void _TableView_DatabaseChanged(object sender, System.EventArgs e) {
            SetDatabase(_tableView?.Database);
            UpdateViewControls();
            Check_OrderButtons();
        }

        private void _TableView_EnabledChanged(object sender, System.EventArgs e) {
            UpdateViewControls();
            Check_OrderButtons();
        }

        private void _TableView_ViewChanged(object sender, System.EventArgs e) {
            UpdateViewControls();
            Check_OrderButtons();
        }

        private void btnAktuelleAnsichtLoeschen_Click(object sender, System.EventArgs e) {
            if (_tableView?.Database == null || _tableView.Arrangement < 2 || _tableView.Arrangement >= _tableView.Database.ColumnArrangements.Count) { return; }
            if (MessageBox.Show("Anordung <b>'" + _tableView?.CurrentArrangement?.Name + "'</b><br>wirklich löschen?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
            _tableView?.Database.ColumnArrangements.RemoveAt(_tableView.Arrangement);
            _tableView.Arrangement = 1;
        }

        private void btnAlleSpaltenEinblenden_Click(object sender, System.EventArgs e) {
            if (MessageBox.Show("Alle Spalten anzeigen?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
            _tableView?.CurrentArrangement?.ShowAllColumns();
        }

        private void btnAnsichtUmbenennen_Click(object sender, System.EventArgs e) {
            var n = InputBox.Show("Umbenennen:", _tableView?.CurrentArrangement.Name, enVarType.Text);
            if (!string.IsNullOrEmpty(n)) { _tableView.CurrentArrangement.Name = n; }
        }

        private void btnBerechtigungsgruppen_Click(object sender, System.EventArgs e) {
            ItemCollectionList aa = new();
            aa.AddRange(_tableView?.Database?.Permission_AllUsed(false));
            aa.Sort();
            aa.CheckBehavior = enCheckBehavior.MultiSelection;
            aa.Check(_tableView.CurrentArrangement?.PermissionGroups_Show, true);
            var b = InputBoxListBoxStyle.Show("Wählen sie, wer anzeigeberechtigt ist:<br><i>Info: Administratoren sehen alle Ansichten", aa, enAddType.Text, true);
            if (b == null) { return; }
            _tableView.CurrentArrangement.PermissionGroups_Show.Clear();
            _tableView.CurrentArrangement.PermissionGroups_Show.AddRange(b.ToArray());
            if (_tableView.Arrangement == 1) { _tableView.CurrentArrangement.PermissionGroups_Show.Add("#Everybody"); }
        }

        private void btnDatenbankPfad_Click(object sender, System.EventArgs e) {
            var p = _tableView.Database.Filename.FilePath();
            if (PathExists(p)) { ExecuteFile(p); }
        }

        private void btnNeueSpalteErstellen_Click(object sender, System.EventArgs e) {
            if (_tableView.Database.ReadOnly) { return; }
            var vorlage = _tableView.CursorPosColumn();
            var mitDaten = false;
            if (vorlage != null && !string.IsNullOrEmpty(vorlage.Identifier)) { vorlage = null; }
            if (vorlage != null) {
                switch (MessageBox.Show("Spalte '" + vorlage.ReadableText() + "' als<br>Vorlage verwenden?", enImageCode.Frage, "Ja", "Ja, mit allen Daten", "Nein", "Abbrechen")) {
                    case 0:
                        break;

                    case 1:
                        mitDaten = true;
                        break;

                    case 2:
                        vorlage = null;
                        break;

                    default:
                        return;
                }
            }
            var newc = _tableView.Database.Column.Add();
            if (vorlage != null) {
                newc.CloneFrom(vorlage);
                if (mitDaten) {
                    foreach (var thisR in _tableView.Database.Row) {
                        thisR.CellSet(newc, thisR.CellGetString(vorlage));
                    }
                }
            }
            using (ColumnEditor w = new(newc, _tableView)) {
                w.ShowDialog();
                newc.Invalidate_ColumAndContent();
            }
            _tableView.Database.Column.Repair();

            if (_tableView.Arrangement > 0 && _tableView.CurrentArrangement != null) { _tableView.CurrentArrangement.Add(newc, false); }

            //_TableView.Database.CheckViewsAndArrangements();

            _tableView.Invalidate_HeadSize();
        }

        private void btnPermanent_CheckedChanged(object sender, System.EventArgs e) {
            ColumnViewItem viewItem = null;
            if (_tableView.CursorPosColumn() != null) { viewItem = _tableView.CurrentArrangement[_tableView.CursorPosColumn()]; }
            if (viewItem == null) { return; }
            viewItem.ViewType = btnPermanent.Checked ? enViewType.PermanentColumn : enViewType.Column;
            Check_OrderButtons();
        }

        private void btnPosEingeben_Click(object sender, System.EventArgs e) {
            if (_tableView.Arrangement < 0) { return; }
            var c = _tableView.CursorPosColumn();
            if (c == null) { return; }
            var p = InputBox.Show("<b>" + _tableView.CursorPosColumn().ReadableText() + "</b><br>Auf welche Position verschieben?<br>Info: Nummerierung beginnt mit 1", "", enVarType.Integer);
            if (int.TryParse(p, out var index)) {
                if (index < 1) { return; }
                index--;
                var viewItem = _tableView.CurrentArrangement[c];
                if (viewItem != null) {
                    _tableView.CurrentArrangement.Remove(viewItem);
                }
                if (index >= _tableView.CurrentArrangement.Count) { index = _tableView.CurrentArrangement.Count; }
                _tableView.CurrentArrangement.Insert(index, c);
                _tableView.CursorPos_Set(c, _tableView.CursorPosRow(), true);
                Check_OrderButtons();
            }
        }

        private void btnSpalteAusblenden_Click(object sender, System.EventArgs e) {
            ColumnViewItem? viewItem = null;
            if (_tableView.CursorPosColumn() != null) { viewItem = _tableView.CurrentArrangement[_tableView.CursorPosColumn()]; }
            _tableView.CurrentArrangement.Remove(viewItem);
        }

        private void btnSpalteBearbeiten_Click(object sender, System.EventArgs e) => TabAdministration.OpenColumnEditor(_tableView.CursorPosColumn(), _tableView.CursorPosRow()?.Row, _tableView);

        private void btnSpalteDauerhaftloeschen_Click(object sender, System.EventArgs e) {
            if (MessageBox.Show("Spalte <b>" + _tableView.CursorPosColumn().ReadableText() + "</b> endgültig löschen?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
            _tableView.Database.Column.Remove(_tableView.CursorPosColumn());
        }

        private void btnSpalteEinblenden_Click(object sender, System.EventArgs e) {
            ItemCollectionList ic = new();
            foreach (var thisColumnItem in _tableView.Database.Column.Where(thisColumnItem => thisColumnItem != null && _tableView.CurrentArrangement[thisColumnItem] == null)) {
                ic.Add(thisColumnItem);
            }
            if (ic.Count == 0) {
                if (MessageBox.Show("Es werden bereits alle<br>Spalten angezeigt.<br><br>Wollen sie eine neue Spalte erstellen?", enImageCode.Frage, "Ja", "Nein") == 0) { btnNeueSpalteErstellen_Click(sender, e); }
                return;
            }
            ic.Sort();
            var r = InputBoxListBoxStyle.Show("Wählen sie:", ic, enAddType.None, true);
            if (r == null || r.Count == 0) { return; }
            _tableView.CurrentArrangement.Add(_tableView.Database.Column.SearchByKey(LongParse(r[0])), false);
            _tableView.Invalidate_HeadSize();
        }

        private void btnSpalteNachLinks_Click(object sender, System.EventArgs e) {
            var tmpc = _tableView.CursorPosColumn();
            var tmpr = _tableView.CursorPosRow();

            if (_tableView.Arrangement > 0) {
                ColumnViewItem? viewItem = null;
                if (_tableView.CursorPosColumn() != null) { viewItem = _tableView.CurrentArrangement[_tableView.CursorPosColumn()]; }
                _tableView.CurrentArrangement.Swap(viewItem, viewItem.PreviewsVisible(_tableView.CurrentArrangement));
            } else {
                _tableView.Database.Column.Swap(_tableView.CursorPosColumn(), _tableView.CursorPosColumn().Previous());
            }

            _tableView.CursorPos_Set(tmpc, tmpr, true);
            Check_OrderButtons();
        }

        private void btnSpalteNachRechts_Click(object sender, System.EventArgs e) {
            var tmpc = _tableView.CursorPosColumn();
            var tmpr = _tableView.CursorPosRow();

            if (_tableView.Arrangement > 0) {
                ColumnViewItem? viewItem = null;
                if (_tableView.CursorPosColumn() != null) { viewItem = _tableView.CurrentArrangement[_tableView.CursorPosColumn()]; }
                _tableView.CurrentArrangement.Swap(viewItem, viewItem.NextVisible(_tableView.CurrentArrangement));
            } else {
                _tableView.Database.Column.Swap(_tableView.CursorPosColumn(), _tableView.CursorPosColumn().Next());
            }

            _tableView.CursorPos_Set(tmpc, tmpr, true);
            Check_OrderButtons();
        }

        private void btnSystemspaltenAusblenden_Click(object sender, System.EventArgs e) => _tableView.CurrentArrangement.HideSystemColumns();

        private void cbxInternalColumnArrangementSelector_ItemClicked(object sender, BasicListItemEventArgs e) {
            if (string.IsNullOrEmpty(cbxInternalColumnArrangementSelector.Text)) { return; }
            _tableView.Arrangement = int.Parse(e.Item.Internal);
            Check_OrderButtons();
        }

        private void Check_OrderButtons() {
            if (InvokeRequired) {
                Invoke(new Action(Check_OrderButtons));
                return;
            }
            var enAnsichtsVerwaltung = true;
            var enAktuelleAnsicht = true;
            var enAktuelleSpalte = true;
            if (_tableView?.Database == null || !_tableView.Database.IsAdministrator()) {
                Enabled = false;
                return; // Weitere funktionen benötigen sicher eine Datenbank um keine Null Exception auszulösen
            }
            if (_tableView.Design != enBlueTableAppearance.Standard || !_tableView.Enabled || !_tableView.Visible || _tableView.Database.ReadOnly) {
                enAnsichtsVerwaltung = false;
                enAktuelleAnsicht = false;
                enAktuelleSpalte = false;
            }
            ColumnViewItem viewItem = null;
            var column = _tableView.CursorPosColumn();
            if (column != null) { viewItem = _tableView.CurrentArrangement[column]; }
            var indexOfViewItem = -1;
            if (_tableView.Arrangement <= _tableView.Database.ColumnArrangements.Count) { indexOfViewItem = _tableView.CurrentArrangement.IndexOf(viewItem); }
            var enLayoutEditable = Convert.ToBoolean(_tableView.Arrangement > 0); // Hauptansicht (0) kann nicht bearbeitet werden
            var enLayoutDeletable = Convert.ToBoolean(_tableView.Arrangement > 1); // Hauptansicht (0) und Allgemeine Ansicht (1) können nicht gelöscht werden
            btnAktuelleAnsichtLoeschen.Enabled = enLayoutDeletable;
            btnAlleSpaltenEinblenden.Enabled = enLayoutEditable;
            btnSpalteAusblenden.Enabled = enLayoutEditable;
            btnSpalteEinblenden.Enabled = enLayoutEditable;
            btnSystemspaltenAusblenden.Enabled = enLayoutEditable;
            btnSpalteDauerhaftloeschen.Enabled = Convert.ToBoolean(column != null && string.IsNullOrEmpty(column.Identifier));
            if (column == null || viewItem == null) {
                enAktuelleSpalte = false;
                //grpAktuelleSpalte.Text = "Spalte: -";
            } else {
                // grpAktuelleSpalte.Text = "Spalte: " + column.ReadableText();
                btnSpalteNachLinks.Enabled = Convert.ToBoolean(indexOfViewItem > 0);
                btnSpalteNachRechts.Enabled = Convert.ToBoolean(indexOfViewItem >= 0) && Convert.ToBoolean(indexOfViewItem < _tableView.CurrentArrangement.Count - 1);
                btnPosEingeben.Enabled = _tableView.Arrangement > 0;
                if (_tableView.PermanentPossible(viewItem) && _tableView.NonPermanentPossible(viewItem)) {
                    btnPermanent.Enabled = true;
                    btnPermanent.Checked = viewItem.ViewType == enViewType.PermanentColumn;
                } else if (_tableView.PermanentPossible(viewItem)) {
                    btnPermanent.Enabled = false;
                    btnPermanent.Checked = true;
                } else {
                    btnPermanent.Enabled = false;
                    btnPermanent.Checked = false;
                }
            }
            grpAnsichtsVerwaltung.Enabled = enAnsichtsVerwaltung;
            grpAktuelleAnsicht.Enabled = enAktuelleAnsicht;
            grpAktuelleSpalte.Enabled = enAktuelleSpalte;
            Enabled = true;
        }

        private void OrderAdd_Click(object sender, System.EventArgs e) {
            var mitVorlage = false;
            if (_tableView.Arrangement > 0 && _tableView.CurrentArrangement != null) {
                mitVorlage = Convert.ToBoolean(MessageBox.Show("<b>Neue Spaltenanordnung erstellen:</b><br>Wollen sie die aktuelle Ansicht kopieren?", enImageCode.Frage, "Ja", "Nein") == 0);
            }
            if (_tableView.Database.ColumnArrangements.Count < 1) {
                _tableView.Database.ColumnArrangements.Add(new ColumnViewCollection(_tableView.Database, "", ""));
            }
            string newname;
            if (mitVorlage) {
                newname = InputBox.Show("Die aktuelle Ansicht wird <b>kopiert</b>.<br><br>Geben sie den Namen<br>der neuen Anordnung ein:", "", enVarType.Text);
                if (string.IsNullOrEmpty(newname)) { return; }
                _tableView.Database.ColumnArrangements.Add(new ColumnViewCollection(_tableView.Database, _tableView.CurrentArrangement.ToString(), newname));
            } else {
                newname = InputBox.Show("Geben sie den Namen<br>der neuen Anordnung ein:", "", enVarType.Text);
                if (string.IsNullOrEmpty(newname)) { return; }
                _tableView.Database.ColumnArrangements.Add(new ColumnViewCollection(_tableView.Database, "", newname));
            }
        }

        private void SetDatabase(Database? database) {
            if (_tmpDatabase != null) {
                _tmpDatabase.ShouldICancelSaveOperations -= TmpDatabase_ShouldICancelDiscOperations;
            }
            _tmpDatabase = database;
            if (_tmpDatabase != null) {
                _tmpDatabase.ShouldICancelSaveOperations += TmpDatabase_ShouldICancelDiscOperations;
            }
        }

        private void UpdateViewControls() => Table.WriteColumnArrangementsInto(cbxInternalColumnArrangementSelector, _tableView.Database, _tableView.Arrangement);

        #endregion
    }
}