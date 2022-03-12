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
using BlueControls.ItemCollection;
using BlueDatabase;
using System;
using System.Collections.Generic;
using System.Drawing;
using BlueControls.ItemCollection.ItemCollectionList;
using static BlueBasics.Converter;

namespace BlueControls.BlueDatabaseDialogs {

    public partial class frmColumnArrangementPadEditor : PadEditor {

        #region Fields

        public readonly Database? Database;

        public bool Generating;

        public bool Sorting;

        private int Arrangement = -1;

        #endregion

        #region Constructors

        public frmColumnArrangementPadEditor(Database database) : this() {
            Database = database;
            //AllDatabases.Add(Database.Filename);
            Database.ShouldICancelSaveOperations += TmpDatabase_ShouldICancelDiscOperations;
            Arrangement = 1;
            UpdateCombobox();
            ShowOrder();
        }

        private frmColumnArrangementPadEditor() => InitializeComponent();

        #endregion

        #region Properties

        public ColumnViewCollection? CurrentArrangement => Database?.ColumnArrangements == null || Database.ColumnArrangements.Count <= Arrangement
                    ? null
                    : Database.ColumnArrangements[Arrangement];

        #endregion

        #region Methods

        protected override void OnFormClosed(System.Windows.Forms.FormClosedEventArgs e) {
            Database.ShouldICancelSaveOperations -= TmpDatabase_ShouldICancelDiscOperations;
            base.OnFormClosed(e);
        }

        private static void TmpDatabase_ShouldICancelDiscOperations(object sender, System.ComponentModel.CancelEventArgs e) => e.Cancel = true;

        private void btnAktuelleAnsichtLoeschen_Click(object sender, System.EventArgs e) {
            if (Database == null || Arrangement < 2 || Arrangement >= Database.ColumnArrangements.Count) { return; }
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
            aa.AddRange(Database.Permission_AllUsed(false));
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

            var tmporder = IntParse(e.Item.Internal);

            if (Arrangement == tmporder) { return; }

            Arrangement = tmporder;
            ShowOrder();
        }

        private void Pad_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (Generating || Sorting) { return; }
        }

        private void Pad_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (Generating || Sorting) { return; }
            SortColumns();
        }

        private void ShowOrder() {
            if (Generating) { Develop.DebugPrint("Generating falsch!"); }

            Generating = true;
            Pad.Item.Clear();

            #region Erst alle Spalten der eigenen Datenbank erzeugen, um später verweisen zu können

            var X = 0f;
            foreach (var thisc in CurrentArrangement) {
                var it = new ColumnPadItem(thisc.Column);
                Pad.Item.Add(it);
                it.SetLeftTopPoint(X, 0);
                X = it.UsedArea.Right;
            }

            #endregion

            #region Im zweiten Durchlauf ermitteln, welche Verknüpfungen es gibt

            var verkn = new List<string>();
            foreach (var thisc in Database.Column) {
                if (thisc.LinkedDatabase != null) {
                    var dbN = thisc.LinkedDatabase.Filename + "|" + thisc.LinkedCellFilter.JoinWithCr();
                    verkn.AddIfNotExists(dbN);
                }
            }

            #endregion

            #region Im dritten Durchlauf nun die Verknüpfungen erstellen

            var kx = 0f;
            foreach (var thiscode in verkn) {
                foreach (var thisc in CurrentArrangement) {
                    var it = (ColumnPadItem)Pad.Item[thisc.Column.Name];

                    if (thisc.Column.LinkedDatabase != null) {
                        // String als Namen als eindeutige Kennung
                        var dbN = thisc.Column.LinkedDatabase.Filename + "|" + thisc.Column.LinkedCellFilter.JoinWithCr();

                        if (dbN == thiscode) {

                            #region Database-Item 'databItem' erzeugen

                            var databItem = (GenericConnectebilePadItem)Pad.Item[dbN];
                            if (databItem == null) {
                                var nam = thisc.Column.LinkedDatabase.Filename.FileNameWithSuffix();
                                //if (thisc.Column.LinkedCell_RowKeyIsInColumn == -9999) { nam += "\r\nSKRIPT"; }
                                databItem = new GenericConnectebilePadItem(dbN, nam, new Size(100, 300));
                                Pad.Item.Add(databItem);
                                databItem.SetLeftTopPoint(Math.Max(kx, it.UsedArea.Left), 600);
                                kx = databItem.UsedArea.Right;
                            }

                            #endregion

                            #region LinkedDatabase-Items erzeugen

                            #region Key aus eigener Datenbank zur Verknüpften Datenbank zeigen lassen

                            // Wenn der Key-Wert aus der Spalte geholt wird (nicht vom Skript gesteuert)....
                            if (thisc.Column.LinkedCellFilter.Count > 0) {
                                ///...Die Spalte ermitteln...
                                ColumnItem rkcol = null; //thisc.Column.Database.Column.SearchByKey(thisc.Column.LinkedCellFilter.JoinWithCr());

                                if (rkcol != null) {
                                    //... das dazugehörige Item ermitteln ...
                                    var rkcolit = (ColumnPadItem)Pad.Item[rkcol.Name];

                                    //...und auf die Datenbank zeigen lassen, wenn diese existiert
                                    rkcolit?.ConnectsTo.AddIfNotExists(new ItemConnection(databItem, enConnectionType.Bottom, enConnectionType.Top));
                                }
                            }

                            #endregion

                            #region Dann die Spalte erzeugen, aus der der der Wert kommt

                            var c2 = thisc.Column.LinkedDatabase.Column.SearchByKey(thisc.Column.LinkedCell_ColumnKeyOfLinkedDatabase);
                            var it2 = new ColumnPadItem(c2);
                            Pad.Item.Add(it2);
                            it2.SetLeftTopPoint(kx, 600);
                            it2.ConnectsTo.Add(new ItemConnection(it, enConnectionType.Left, enConnectionType.Right));
                            kx = it2.UsedArea.Right;

                            // und noch die Datenbank  auf die Spalte zeigen lassem
                            databItem?.ConnectsTo.AddIfNotExists(new ItemConnection(it, enConnectionType.Bottom, enConnectionType.Top));
                        }

                        #endregion
                    }
                }

                kx += 30;

                #endregion
            }

            #endregion

            SortColumns();
            Pad.ZoomFit();
            Generating = false;
        }

        private void SortColumns() {
            if (Sorting) { Develop.DebugPrint("Sorting falsch!"); }
            Sorting = true;
            var done = new List<BasicPadItem>();
            var left = 0f;
            do {
                FixedRectangleBitmapPadItem x = null;

                foreach (var thisIt in Pad.Item) {
                    if (!done.Contains(thisIt) && thisIt is ColumnPadItem fi) {
                        if (fi.Column.Database == Database) {
                            if (x == null || thisIt.UsedArea.X < x.UsedArea.X) {
                                x = fi;
                            }
                        }
                    }
                }

                if (x == null) { break; }
                done.Add(x);
                x.SetLeftTopPoint(left, 0);
                left = x.UsedArea.Right;
            } while (true);

            Sorting = false;
        }

        private void UpdateCombobox() => Table.WriteColumnArrangementsInto(cbxInternalColumnArrangementSelector, Database, Arrangement);

        #endregion
    }
}