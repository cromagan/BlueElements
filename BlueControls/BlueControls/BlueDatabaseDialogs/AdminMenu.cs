// Authors:
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

    public sealed partial class AdminMenu : Form {

        #region Fields

        private readonly Table _TableView;
        private Database tmpDatabase;

        #endregion

        #region Constructors

        public AdminMenu(Table table) {
            // Dieser Aufruf ist f�r den Designer erforderlich.
            InitializeComponent();
            // F�gen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            _TableView = table;
            if (_TableView != null) {
                _TableView.DatabaseChanged += _TableView_DatabaseChanged;
                _TableView.CursorPosChanged += _TableView_CursorPosChanged;
                _TableView.ViewChanged += _TableView_ViewChanged;
                _TableView.EnabledChanged += _TableView_EnabledChanged;
            }
            SetDatabase(_TableView?.Database);
            UpdateViewControls();
            Check_OrderButtons();
        }

        #endregion

        #region Methods

        protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e) {
            SetDatabase(null);
            if (_TableView != null) {
                _TableView.DatabaseChanged -= _TableView_DatabaseChanged;
                _TableView.CursorPosChanged -= _TableView_CursorPosChanged;
                _TableView.ViewChanged -= _TableView_ViewChanged;
                _TableView.EnabledChanged -= _TableView_EnabledChanged;
            }
            base.OnFormClosing(e);
        }

        private void _TableView_CursorPosChanged(object sender, CellExtEventArgs e) => Check_OrderButtons();

        private void _TableView_DatabaseChanged(object sender, System.EventArgs e) {
            SetDatabase(_TableView.Database);
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
            if (_TableView.Arrangement < 2 || _TableView.Arrangement >= _TableView.Database.ColumnArrangements.Count) { return; }
            if (MessageBox.Show("Anordung <b>'" + _TableView.CurrentArrangement.Name + "'</b><br>wirklich l�schen?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
            _TableView.Database.ColumnArrangements.RemoveAt(_TableView.Arrangement);
            _TableView.Arrangement = 1;
        }

        private void btnAlleSpaltenEinblenden_Click(object sender, System.EventArgs e) {
            if (MessageBox.Show("Alle Spalten anzeigen?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
            _TableView.CurrentArrangement.ShowAllColumns();
        }

        private void btnAnsichtUmbenennen_Click(object sender, System.EventArgs e) {
            var n = InputBox.Show("Umbenennen:", _TableView.CurrentArrangement.Name, enDataFormat.Text);
            if (!string.IsNullOrEmpty(n)) { _TableView.CurrentArrangement.Name = n; }
        }

        private void btnBerechtigungsgruppen_Click(object sender, System.EventArgs e) {
            ItemCollectionList aa = new();
            aa.AddRange(_TableView.Database.Permission_AllUsed(true));
            aa.Sort();
            aa.CheckBehavior = enCheckBehavior.MultiSelection;
            aa.Check(_TableView.CurrentArrangement.PermissionGroups_Show, true);
            var b = InputBoxListBoxStyle.Show("W�hlen sie, wer anzeigeberechtigt ist:<br><i>Info: Administratoren sehen alle Ansichten", aa, enAddType.Text, true);
            if (b == null) { return; }
            _TableView.CurrentArrangement.PermissionGroups_Show.Clear();
            _TableView.CurrentArrangement.PermissionGroups_Show.AddRange(b.ToArray());
            if (_TableView.Arrangement == 1) { _TableView.CurrentArrangement.PermissionGroups_Show.Add("#Everybody"); }
        }

        private void btnDatenbankPfad_Click(object sender, System.EventArgs e) {
            var P = _TableView.Database.Filename.FilePath();
            if (PathExists(P)) { ExecuteFile(P); }
        }

        private void btnNeueSpalteErstellen_Click(object sender, System.EventArgs e) {
            if (_TableView.Database.ReadOnly) { return; }
            var Vorlage = _TableView.CursorPosColumn();
            var mitDaten = false;
            if (Vorlage != null && !string.IsNullOrEmpty(Vorlage.Identifier)) { Vorlage = null; }
            if (Vorlage != null) {
                switch (MessageBox.Show("Spalte '" + Vorlage.ReadableText() + "' als<br>Vorlage verwenden?", enImageCode.Frage, "Ja", "Ja, mit allen Daten", "Nein", "Abbrechen")) {
                    case 0:
                        break;

                    case 1:
                        mitDaten = true;
                        break;

                    case 2:
                        Vorlage = null;
                        break;

                    default:
                        return;
                }
            }
            ColumnItem newc;
            if (Vorlage != null) {
                newc = _TableView.Database.Column.AddACloneFrom(Vorlage);
                if (mitDaten) {
                    foreach (var thisR in _TableView.Database.Row) {
                        thisR.CellSet(newc, thisR.CellGetString(Vorlage));
                    }
                }
            } else {
                newc = _TableView.Database.Column.Add();
            }
            using (ColumnEditor w = new(newc, _TableView)) {
                w.ShowDialog();
                newc.Invalidate_ColumAndContent();
            }
            _TableView.Database.Column.Repair();
            if (_TableView.Arrangement > 0 && _TableView.CurrentArrangement != null) { _TableView.CurrentArrangement.Add(newc, false); }
            _TableView.Invalidate_HeadSize();
        }

        private void btnPermanent_CheckedChanged(object sender, System.EventArgs e) {
            ColumnViewItem ViewItem = null;
            if (_TableView.CursorPosColumn() != null) { ViewItem = _TableView.CurrentArrangement[_TableView.CursorPosColumn()]; }
            if (ViewItem == null) { return; }
            ViewItem.ViewType = btnPermanent.Checked ? enViewType.PermanentColumn : enViewType.Column;
            Check_OrderButtons();
        }

        private void btnPosEingeben_Click(object sender, System.EventArgs e) {
            if (_TableView.Arrangement < 0) { return; }
            var c = _TableView.CursorPosColumn();
            if (c == null) { return; }
            var p = InputBox.Show("<b>" + _TableView.CursorPosColumn().ReadableText() + "</b><br>Auf welche Position verschieben?<br>Info: Nummerierung beginnt mit 1", "", enDataFormat.Ganzzahl);
            if (int.TryParse(p, out var index)) {
                if (index < 1) { return; }
                index--;
                var ViewItem = _TableView.CurrentArrangement[c];
                if (ViewItem != null) {
                    _TableView.CurrentArrangement.Remove(ViewItem);
                }
                if (index >= _TableView.CurrentArrangement.Count) { index = _TableView.CurrentArrangement.Count; }
                _TableView.CurrentArrangement.Insert(index, c);
                _TableView.CursorPos_Set(c, _TableView.CursorPosRow(), true);
                Check_OrderButtons();
            }
        }

        private void btnSpalteAusblenden_Click(object sender, System.EventArgs e) {
            ColumnViewItem ViewItem = null;
            if (_TableView.CursorPosColumn() != null) { ViewItem = _TableView.CurrentArrangement[_TableView.CursorPosColumn()]; }
            _TableView.CurrentArrangement.Remove(ViewItem);
        }

        private void btnSpalteBearbeiten_Click(object sender, System.EventArgs e) => tabAdministration.OpenColumnEditor(_TableView.CursorPosColumn(), _TableView.CursorPosRow()?.Row, _TableView);

        private void btnSpalteDauerhaftloeschen_Click(object sender, System.EventArgs e) {
            if (MessageBox.Show("Spalte <b>" + _TableView.CursorPosColumn().ReadableText() + "</b> endg�ltig l�schen?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
            _TableView.Database.Column.Remove(_TableView.CursorPosColumn());
        }

        private void btnSpalteEinblenden_Click(object sender, System.EventArgs e) {
            ItemCollectionList ic = new();
            foreach (var ThisColumnItem in _TableView.Database.Column) {
                if (ThisColumnItem != null && _TableView.CurrentArrangement[ThisColumnItem] == null) { ic.Add(ThisColumnItem); }
            }
            if (ic.Count == 0) {
                if (MessageBox.Show("Es werden bereits alle<br>Spalten angezeigt.<br><br>Wollen sie eine neue Spalte erstellen?", enImageCode.Frage, "Ja", "Nein") == 0) { btnNeueSpalteErstellen_Click(sender, e); }
                return;
            }
            ic.Sort();
            var r = InputBoxListBoxStyle.Show("W�hlen sie:", ic, enAddType.None, true);
            if (r == null || r.Count == 0) { return; }
            _TableView.CurrentArrangement.Add(_TableView.Database.Column.SearchByKey(IntParse(r[0])), false);
            _TableView.Invalidate_HeadSize();
        }

        private void btnSpalteNachLinks_Click(object sender, System.EventArgs e) {
            var tmpc = _TableView.CursorPosColumn();
            var tmpr = _TableView.CursorPosRow();

            if (_TableView.Arrangement > 0) {
                ColumnViewItem ViewItem = null;
                if (_TableView.CursorPosColumn() != null) { ViewItem = _TableView.CurrentArrangement[_TableView.CursorPosColumn()]; }
                _TableView.CurrentArrangement.Swap(ViewItem, ViewItem.PreviewsVisible(_TableView.CurrentArrangement));
            } else {
                _TableView.Database.Column.Swap(_TableView.CursorPosColumn(), _TableView.CursorPosColumn().Previous());
            }

            _TableView.CursorPos_Set(tmpc, tmpr, true);
            Check_OrderButtons();
        }

        private void btnSpalteNachRechts_Click(object sender, System.EventArgs e) {
            var tmpc = _TableView.CursorPosColumn();
            var tmpr = _TableView.CursorPosRow();

            if (_TableView.Arrangement > 0) {
                ColumnViewItem ViewItem = null;
                if (_TableView.CursorPosColumn() != null) { ViewItem = _TableView.CurrentArrangement[_TableView.CursorPosColumn()]; }
                _TableView.CurrentArrangement.Swap(ViewItem, ViewItem.NextVisible(_TableView.CurrentArrangement));
            } else {
                _TableView.Database.Column.Swap(_TableView.CursorPosColumn(), _TableView.CursorPosColumn().Next());
            }

            _TableView.CursorPos_Set(tmpc, tmpr, true);
            Check_OrderButtons();
        }

        private void btnSystemspaltenAusblenden_Click(object sender, System.EventArgs e) => _TableView.CurrentArrangement.HideSystemColumns();

        private void cbxInternalColumnArrangementSelector_ItemClicked(object sender, BasicListItemEventArgs e) {
            if (string.IsNullOrEmpty(cbxInternalColumnArrangementSelector.Text)) { return; }
            _TableView.Arrangement = int.Parse(e.Item.Internal);
            Check_OrderButtons();
        }

        private void Check_OrderButtons() {
            if (InvokeRequired) {
                Invoke(new Action(() => Check_OrderButtons()));
                return;
            }
            var enAnsichtsVerwaltung = true;
            var enAktuelleAnsicht = true;
            var enAktuelleSpalte = true;
            if (_TableView?.Database == null || !_TableView.Database.IsAdministrator()) {
                Enabled = false;
                return; // Weitere funktionen ben�tigen sicher eine Datenbank um keine Null Exception auszul�sen
            }
            if (_TableView.Design != enBlueTableAppearance.Standard || !_TableView.Enabled || !_TableView.Visible || _TableView.Database.ReadOnly) {
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
            var enLayoutDeletable = Convert.ToBoolean(_TableView.Arrangement > 1); // Hauptansicht (0) und Allgemeine Ansicht (1) k�nnen nicht gel�scht werden
            btnAktuelleAnsichtLoeschen.Enabled = enLayoutDeletable;
            btnAlleSpaltenEinblenden.Enabled = enLayoutEditable;
            btnSpalteAusblenden.Enabled = enLayoutEditable;
            btnSpalteEinblenden.Enabled = enLayoutEditable;
            btnSystemspaltenAusblenden.Enabled = enLayoutEditable;
            btnSpalteDauerhaftloeschen.Enabled = Convert.ToBoolean(column != null && string.IsNullOrEmpty(column.Identifier));
            if (column == null || ViewItem == null) {
                enAktuelleSpalte = false;
                //grpAktuelleSpalte.Text = "Spalte: -";
            } else {
                // grpAktuelleSpalte.Text = "Spalte: " + column.ReadableText();
                btnSpalteNachLinks.Enabled = Convert.ToBoolean(IndexOfViewItem > 0);
                btnSpalteNachRechts.Enabled = Convert.ToBoolean(IndexOfViewItem >= 0) && Convert.ToBoolean(IndexOfViewItem < _TableView.CurrentArrangement.Count - 1);
                btnPosEingeben.Enabled = _TableView.Arrangement > 0;
                if (_TableView.PermanentPossible(ViewItem) && _TableView.NonPermanentPossible(ViewItem)) {
                    btnPermanent.Enabled = true;
                    btnPermanent.Checked = ViewItem.ViewType == enViewType.PermanentColumn;
                } else if (_TableView.PermanentPossible(ViewItem)) {
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
            var MitVorlage = false;
            if (_TableView.Arrangement > 0 && _TableView.CurrentArrangement != null) {
                MitVorlage = Convert.ToBoolean(MessageBox.Show("<b>Neue Spaltenanordnung erstellen:</b><br>Wollen sie die aktuelle Ansicht kopieren?", enImageCode.Frage, "Ja", "Nein") == 0);
            }
            if (_TableView.Database.ColumnArrangements.Count < 1) {
                _TableView.Database.ColumnArrangements.Add(new ColumnViewCollection(_TableView.Database, "", ""));
            }
            string newname;
            if (MitVorlage) {
                newname = InputBox.Show("Die aktuelle Ansicht wird <b>kopiert</b>.<br><br>Geben sie den Namen<br>der neuen Anordnung ein:", "", enDataFormat.Text);
                if (string.IsNullOrEmpty(newname)) { return; }
                _TableView.Database.ColumnArrangements.Add(new ColumnViewCollection(_TableView.Database, _TableView.CurrentArrangement.ToString(), newname));
            } else {
                newname = InputBox.Show("Geben sie den Namen<br>der neuen Anordnung ein:", "", enDataFormat.Text);
                if (string.IsNullOrEmpty(newname)) { return; }
                _TableView.Database.ColumnArrangements.Add(new ColumnViewCollection(_TableView.Database, "", newname));
            }
        }

        private void SetDatabase(Database database) {
            if (tmpDatabase != null) {
                tmpDatabase.ShouldICancelSaveOperations -= TmpDatabase_ShouldICancelDiscOperations;
            }
            tmpDatabase = database;
            if (tmpDatabase != null) {
                tmpDatabase.ShouldICancelSaveOperations += TmpDatabase_ShouldICancelDiscOperations;
            }
        }

        private void TmpDatabase_ShouldICancelDiscOperations(object sender, System.ComponentModel.CancelEventArgs e) => e.Cancel = true;

        private void UpdateViewControls() => _TableView.WriteColumnArrangementsInto(cbxInternalColumnArrangementSelector);

        #endregion
    }
}