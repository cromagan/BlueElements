// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.ItemCollection;
using BlueControls.ItemCollectionPad;
using BlueControls.ItemCollectionPad.Abstract;
using BlueControls.ItemCollectionPad.FunktionsItems_ColumnArrangement_Editor;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.Interfaces;
using MessageBox = BlueControls.Forms.MessageBox;

namespace BlueControls.BlueDatabaseDialogs;

public partial class ColumnArrangementPadEditor : PadEditor, IHasDatabase {

    #region Fields

    private string _arrangement = string.Empty;

    #endregion

    #region Constructors

    public ColumnArrangementPadEditor(Database? database) : this() {
        var m = Database.EditableErrorReason(database, EditableErrorReasonType.EditCurrently);
        if (!string.IsNullOrEmpty(m)) {
            MessageBox.Show(m, ImageCode.Information, "OK");
            Close();
            return;
        }

        Database = database;

        if (database is Database db) {
            if (db.ColumnArrangements.Count > 1) {
                _arrangement = db.ColumnArrangements[1].KeyName;
            } else if (db.ColumnArrangements.Count > 0) {
                _arrangement = db.ColumnArrangements[0].KeyName;
            }
        }

        UpdateCombobox();
        ShowOrder();
    }

    private ColumnArrangementPadEditor() => InitializeComponent();

    #endregion

    #region Properties

    public Database? Database { get; }

    public int Fixing { get; private set; }

    public bool Generating { get; private set; }

    public bool Sorting { get; private set; }

    #endregion

    #region Methods

    public ColumnViewCollection? CloneOfCurrentArrangement() {
        if (Database is not Database db || db.IsDisposed) { return null; }
        var ca = db.ColumnArrangements.Get(_arrangement);
        return ca == null ? null : (ColumnViewCollection)ca.Clone();
    }

    public int IndexOfCurrentArr() {
        if (Database is not Database db || db.IsDisposed) { return -1; }
        return db.ColumnArrangements.IndexOf(_arrangement);
    }

    public bool IsAllColumnView() => IndexOfCurrentArr() == 0;

    public bool IsDefaultView() => IndexOfCurrentArr() == 1;

    protected override void OnFormClosing(FormClosingEventArgs e) {
        FixColumnArrangement();
        base.OnFormClosing(e);
    }

    private void btnAktuelleAnsichtLoeschen_Click(object sender, System.EventArgs e) {
        if (Database is not Database db || db.IsDisposed) { return; }

        if (CloneOfCurrentArrangement() is not ColumnViewCollection ca) { return; }

        var arn = IndexOfCurrentArr();

        if (arn < 2) { return; }

        if (MessageBox.Show("Anordung <b>'" + ca.KeyName + "'</b><br>wirklich löschen?", ImageCode.Warnung, "Ja", "Nein") != 0) { return; }
        var car = db.ColumnArrangements.CloneWithClones();
        car.RemoveAt(arn);
        db.ColumnArrangements = car.AsReadOnly();
        _arrangement = string.Empty;
        UpdateCombobox();
        ShowOrder();
    }

    private void btnAlleSpaltenEinblenden_Click(object sender, System.EventArgs e) {
        if (CloneOfCurrentArrangement() is not ColumnViewCollection ca) { return; }

        if (MessageBox.Show("Alle Spalten anzeigen?", ImageCode.Warnung, "Ja", "Nein") != 0) { return; }
        ca.ShowAllColumns();

        ChangeCurrentArrangementto(ca);
        ShowOrder();
    }

    private void btnAnsichtUmbenennen_Click(object sender, System.EventArgs e) {
        if (CloneOfCurrentArrangement() is not ColumnViewCollection ca) { return; }

        var n = InputBox.Show("Umbenennen:", ca.KeyName, FormatHolder.Text);
        if (string.IsNullOrEmpty(n)) { return; }
        ca.KeyName = n;

        ChangeCurrentArrangementto(ca);
        UpdateCombobox();
    }

    private void btnBerechtigungsgruppen_Click(object sender, System.EventArgs e) {
        if (Database is not Database db || db.IsDisposed) { return; }
        if (CloneOfCurrentArrangement() is not ColumnViewCollection ca) { return; }

        ItemCollectionList.ItemCollectionList aa = new(true);
        aa.AddRange(db.Permission_AllUsed(false));
        //aa.Sort();
        aa.CheckBehavior = CheckBehavior.MultiSelection;
        aa.Check(ca.PermissionGroups_Show, true);
        var b = InputBoxListBoxStyle.Show("Wählen sie, wer anzeigeberechtigt ist:<br><i>Info: Administratoren sehen alle Ansichten", aa, AddType.Text, true);
        if (b == null) { return; }

        if (IsDefaultView()) { b.Add(Constants.Everybody); }
        ca.PermissionGroups_Show = b.AsReadOnly();
        ChangeCurrentArrangementto(ca);
    }

    private void btnNeueAnsichtErstellen_Click(object sender, System.EventArgs e) {
        if (Database is not Database db || db.IsDisposed) { return; }

        var ca = CloneOfCurrentArrangement();

        var mitVorlage = false;
        if (!IsAllColumnView() && ca != null) {
            mitVorlage = MessageBox.Show("<b>Neue Spaltenanordnung erstellen:</b><br>Wollen sie die aktuelle Ansicht kopieren?", ImageCode.Frage, "Ja", "Nein") == 0;
        }

        var car = db.ColumnArrangements.CloneWithClones();
        //car.RemoveAt(Arrangement);

        if (car.Count < 1) {
            car.Add(new ColumnViewCollection(db, string.Empty, string.Empty));
        }

        string newname;
        if (mitVorlage && ca != null) {
            newname = InputBox.Show("Die aktuelle Ansicht wird <b>kopiert</b>.<br><br>Geben sie den Namen<br>der neuen Anordnung ein:", string.Empty, FormatHolder.Text);
            if (string.IsNullOrEmpty(newname)) { return; }
            car.Add(new ColumnViewCollection(db, ca.ToString(), newname));
        } else {
            newname = InputBox.Show("Geben sie den Namen<br>der neuen Anordnung ein:", string.Empty, FormatHolder.Text);
            if (string.IsNullOrEmpty(newname)) { return; }
            car.Add(new ColumnViewCollection(db, string.Empty, newname));
        }

        db.ColumnArrangements = car.AsReadOnly();
        _arrangement = newname;
        UpdateCombobox();

        ShowOrder();
    }

    private void btnNeueSpalte_Click(object sender, System.EventArgs e) {
        if (Database is not Database db || TableView.ErrorMessage(db, EditableErrorReasonType.EditAcut)) { return; }

        ColumnItem? vorlage = null;
        if (Pad.LastClickedItem is ColumnPadItem cpi && cpi.Column?.Database == db) {
            vorlage = cpi.Column;
        }

        var mitDaten = false;
        if (vorlage != null && vorlage.IsSystemColumn()) { vorlage = null; }
        if (vorlage != null) {
            switch (MessageBox.Show("Spalte '" + vorlage.ReadableText() + "' als<br>Vorlage verwenden?", ImageCode.Frage, "Ja", "Ja, mit allen Daten", "Nein", "Abbrechen")) {
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
        var newc = db.Column.GenerateAndAdd();

        if (newc == null) { return; }

        if (vorlage != null) {
            newc.CloneFrom(vorlage, false);
            if (mitDaten) {
                foreach (var thisR in db.Row) {
                    thisR.CellSet(newc, thisR.CellGetString(vorlage));
                }
            }
        }
        using (ColumnEditor w = new(newc, null)) {
            _ = w.ShowDialog();
            newc.Invalidate_ColumAndContent();
            //w.Dispose();
        }
        db.Column.Repair();

        var ca = CloneOfCurrentArrangement();

        if (!IsAllColumnView() && ca != null) {
            ca.Add(newc, false);
            ChangeCurrentArrangementto(ca);
        }

        db.RepairAfterParse();
        ShowOrder();
    }

    private void btnSpalteEinblenden_Click(object sender, System.EventArgs e) {
        if (Database is not Database db || db.IsDisposed) { return; }

        if (CloneOfCurrentArrangement() is not ColumnViewCollection ca) { return; }

        ItemCollectionList.ItemCollectionList ic = new(true);
        foreach (var thisColumnItem in db.Column) {
            if (thisColumnItem != null && ca[thisColumnItem] == null) { _ = ic.Add(thisColumnItem); }
        }
        if (ic.Count == 0) {
            MessageBox.Show("Es werden bereits alle<br>Spalten angezeigt.", ImageCode.Information, "Ok");
            return;
        }
        //ic.Sort();
        var r = InputBoxListBoxStyle.Show("Wählen sie:", ic, AddType.None, true);
        if (r == null || r.Count == 0) { return; }
        ca.Add(db.Column.Exists(r[0]), false);
        ChangeCurrentArrangementto(ca);
        ShowOrder();
    }

    private void btnSystemspaltenAusblenden_Click(object sender, System.EventArgs e) {
        if (CloneOfCurrentArrangement() is not ColumnViewCollection ca) { return; }

        ca.HideSystemColumns();
        ChangeCurrentArrangementto(ca);

        ShowOrder();
    }

    private void cbxInternalColumnArrangementSelector_ItemClicked(object sender, AbstractListItemEventArgs e) {
        if (string.IsNullOrEmpty(cbxInternalColumnArrangementSelector.Text)) { return; }

        if (_arrangement == e.Item.KeyName) { return; }

        FixColumnArrangement();

        _arrangement = e.Item.KeyName;
        ShowOrder();
        Pad.ZoomFit();
    }

    private void ChangeCurrentArrangementto(ColumnViewCollection cv) {
        if (Database is not Database db || db.IsDisposed) { return; }
        var no = IndexOfCurrentArr();
        if (no < 0 || no >= db.ColumnArrangements.Count) { return; }

        var car = Database.ColumnArrangements.CloneWithClones();
        car[no] = cv;
        db.ColumnArrangements = car.AsReadOnly();
    }

    /// <summary>
    /// Überträgt die aktuelle Ansicht fest in den Datenbankcode hinein
    /// </summary>

    private void chkShowCaptions_CheckedChanged(object sender, System.EventArgs e) {
    }

    private void chkShowCaptions_Click(object sender, System.EventArgs e) {
        if (Database is not Database db || db.IsDisposed) { return; }

        if (CloneOfCurrentArrangement() is not ColumnViewCollection ca) { return; }

        ca.ShowHead = chkShowCaptions.Checked;

        ChangeCurrentArrangementto(ca);
        //ShowOrder();
    }

    private void FixColumnArrangement() {
        if (Database is not Database db || db.IsDisposed) { return; }
        if (Generating || Sorting) { return; }
        if (db.ColumnArrangements.Count == 0) { return; }

        if (CloneOfCurrentArrangement() is not ColumnViewCollection ca) { return; }
        var did = false;

        Fixing++;

        #region Items Ordnen / erstellen / Permament

        var permanentPossible = true;

        var itemsdone = new List<AbstractPadItem>();

        do {
            var leftestItem = LeftestItem(itemsdone);
            if (leftestItem == null) { break; }

            var columnIndexWhoShouldBeThere = ca.IndexOf(ca[leftestItem.Column]);

            #region  Noch nicht in der View, wurde also hinzugfügt. Auch fest hizufügen

            if (columnIndexWhoShouldBeThere < 0) {
                ca.Add(leftestItem.Column, leftestItem.Permanent);
                columnIndexWhoShouldBeThere = ca.IndexOf(ca[leftestItem.Column]);
                did = true;
            }

            #endregion

            #region Stimmen Positionen überein?

            var columnIndexWhoIsOnPos = ca.IndexOf(ca[itemsdone.Count]);
            if (columnIndexWhoShouldBeThere != itemsdone.Count) {
                // Position stimmt nicht, also swapen
                ca.Swap(columnIndexWhoIsOnPos, columnIndexWhoShouldBeThere);
                did = true;
            }

            #endregion

            #region Permanent

            if (ca[itemsdone.Count] is ColumnViewItem cvi) {
                if (permanentPossible) {
                    if (leftestItem.Permanent) {
                        cvi.ViewType = ViewType.PermanentColumn;
                    } else {
                        permanentPossible = false;
                    }
                } else {
                    leftestItem.Permanent = false;
                    cvi.ViewType = ViewType.Column;
                }
            }

            #endregion

            itemsdone.Add(leftestItem);
        } while (true);

        #endregion

        #region Prüfen, ob Items gelöscht wurde, diese dann ebenfalls löschen

        if (ca.Count > itemsdone.Count) {
            if (!IsAllColumnView()) {

                #region Code für Ansichten > 0

                while (ca.Count > itemsdone.Count) {
                    // Item, dass nun durch die Swaps an die letzten
                    // Stellen gewandert und zu viel sind, einfach am Ende weglöschen
                    ca.RemoveAt(ca.Count - 1);
                    did = true;
                }

                #endregion
            } else {

                #region Code für Ansicht 0

                var col = ca[ca.Count - 1]?.Column;
                if (col != null && MessageBox.Show("Spalte <b>" + col.ReadableText() + "</b> endgültig löschen?", ImageCode.Warnung,
                        "Ja", "Nein") == 0) {
                    db.Column.Remove(col, "Benutzer löscht im ColArrangement Editor");
                    did = true;
                }

                #endregion
            }
        }

        #endregion

        Fixing--;
        ChangeCurrentArrangementto(ca);

        if (did) {
            db.RepairAfterParse();
            ShowOrder();
        }
    }

    private void Item_ItemRemoved(object sender, System.EventArgs e) => Pad_MouseUp(null, null);

    private ColumnPadItem? LeftestItem(ICollection<AbstractPadItem> ignore) {
        if (Database is not Database db || db.IsDisposed) { return null; }
        if (Pad?.Item == null) { return null; }

        ColumnPadItem? found = null;

        foreach (var thisIt in Pad.Item) {
            if (!ignore.Contains(thisIt) && thisIt is ColumnPadItem fi) {
                if (fi.Column?.Database == db) {
                    if (found == null || fi.UsedArea.X < found.UsedArea.X) {
                        found = fi;
                    }
                }
            }
        }

        return found;
    }

    private void Pad_MouseUp(object? sender, MouseEventArgs? e) {
        if (Generating || Sorting || Fixing > 0) { return; }
        SortColumns();
        FixColumnArrangement();
    }

    private void ShowOrder() {
        if (Database is not Database db || db.IsDisposed) { return; }
        if (Pad?.Item == null) { return; }

        if (Generating || Fixing > 0) { Develop.DebugPrint("Generating falsch!"); }

        Generating = true;
        Pad.Item.Clear();

        ColumnPadItem? anyitem = null;

        if (CloneOfCurrentArrangement() is not ColumnViewCollection ca) {
            Generating = false;
            return;
        }

        #region Erst alle Spalten der eigenen Datenbank erzeugen, um später verweisen zu können

        var x = 0f;
        foreach (var thisc in ca) {
            if (thisc?.Column is ColumnItem c) {
                var it = new ColumnPadItem(c, thisc.ViewType == ViewType.PermanentColumn);
                Pad.Item.Add(it);
                it.SetLeftTopPoint(x, 0);
                x = it.UsedArea.Right;
                anyitem = it;
            }
        }

        #endregion

        if (anyitem == null) { Generating = false; return; }

        #region Im zweiten Durchlauf ermitteln, welche Verknüpfungen es gibt

        var dbColumnCombi = new List<string>();
        foreach (var thisc in db.Column) {
            if (thisc.LinkedDatabase != null) {
                var dbN = thisc.LinkedDatabase.TableName + "|" + thisc.LinkedCellFilter.JoinWithCr();
                _ = dbColumnCombi.AddIfNotExists(dbN);
            }
        }

        #endregion

        var kx = 0f;
        foreach (var thisCombi in dbColumnCombi) {
            foreach (var thisc in ca) {
                if (thisc?.Column is ColumnItem c) {
                    var it = Pad.Item[c.Database?.TableName + "|" + c.KeyName] as ColumnPadItem;

                    if (c.LinkedDatabase != null) {
                        // String als Namen als eindeutige Kennung
                        var toCheckCombi = c.LinkedDatabase.TableName + "|" + c.LinkedCellFilter.JoinWithCr();

                        if (toCheckCombi == thisCombi) {
                            if (Pad.Item[toCheckCombi] is not GenericPadItem databItem) {
                                var nam = c.LinkedDatabase.TableName;
                                databItem = new GenericPadItem(toCheckCombi, nam, new Size((int)(anyitem.UsedArea.Height / 2), (int)anyitem.UsedArea.Height));
                                Pad.Item.Add(databItem);
                                if (it != null) {
                                    databItem.SetLeftTopPoint(Math.Max(kx, it.UsedArea.Left - databItem.UsedArea.Width), 600);
                                }
                                kx = databItem.UsedArea.Right;
                            }

                            foreach (var thisitem in c.LinkedCellFilter) {
                                var tmp = thisitem.SplitBy("|");

                                if (c.Database is Database db2)
                                    foreach (var thisc2 in db2.Column) {
                                        if (tmp[2].Contains("~" + thisc2.KeyName + "~")) {
                                            if (thisc2.Database is Database db3) {
                                                var rkcolit = (ColumnPadItem?)Pad.Item[db3.TableName + "|" + thisc2.KeyName];
                                                if (rkcolit != null) {
                                                    _ = Pad.Item.Connections.AddIfNotExists(new ItemConnection(rkcolit, ConnectionType.Bottom, false, databItem, ConnectionType.Top, true, false));
                                                }
                                            }
                                        }
                                    }
                            }

                            var c2 = c.LinkedDatabase.Column.Exists(c.LinkedCell_ColumnNameOfLinkedDatabase);
                            if (c2 != null) {
                                var it2 = new ColumnPadItem(c2, false);
                                Pad.Item.Add(it2);
                                it2.SetLeftTopPoint(kx, 600);
                                if (it != null) {
                                    Pad.Item.Connections.Add(new ItemConnection(it2, ConnectionType.Top, false, it, ConnectionType.Bottom, true, false));
                                }
                                kx = it2.UsedArea.Right;

                                // und noch die Datenbank auf die Spalte zeigen lassem
                                _ = Pad.Item.Connections.AddIfNotExists(new ItemConnection(databItem, ConnectionType.Bottom, false, it2, ConnectionType.Bottom, false, false));
                            }
                        }
                    }
                }
            }

            kx += 30;
        }

        SortColumns();

        chkShowCaptions.Checked = ca.ShowHead;

        //Pad.ZoomFit();
        Generating = false;
    }

    private void SortColumns() {
        if (Sorting || Fixing > 0) { Develop.DebugPrint("Sorting falsch!"); }
        Sorting = true;
        var done = new List<AbstractPadItem>();
        var left = 0f;
        do {
            var x = LeftestItem(done);
            if (x == null) { break; }
            done.Add(x);
            x.SetLeftTopPoint(left, 0);
            left = x.UsedArea.Right;
        } while (true);

        Sorting = false;
    }

    private void UpdateCombobox() => Table.WriteColumnArrangementsInto(cbxInternalColumnArrangementSelector, Database, _arrangement);

    #endregion
}