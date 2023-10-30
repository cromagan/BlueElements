// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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
using static BlueBasics.Converter;
using MessageBox = BlueControls.Forms.MessageBox;

namespace BlueControls.BlueDatabaseDialogs;

public partial class ColumnArrangementPadEditor : PadEditor, IHasDatabase {

    #region Fields

    private int _arrangement = -1;

    #endregion

    #region Constructors

    public ColumnArrangementPadEditor(DatabaseAbstract? database) : this() {
        var m = DatabaseAbstract.EditableErrorReason(database, EditableErrorReasonType.EditCurrently);
        if (!string.IsNullOrEmpty(m)) {
            MessageBox.Show(m, ImageCode.Information, "OK");
            Close();
            return;
        }

        Database = database;
        _arrangement = 1;
        UpdateCombobox();
        ShowOrder();
    }

    private ColumnArrangementPadEditor() => InitializeComponent();

    #endregion

    #region Properties

    public ColumnViewCollection? CloneOfCurrentArrangement => Database?.ColumnArrangements == null || Database.ColumnArrangements.Count <= _arrangement
        ? null
        : (ColumnViewCollection)Database.ColumnArrangements[_arrangement].Clone();

    public DatabaseAbstract? Database { get; }

    public int Fixing { get; private set; }
    public bool Generating { get; private set; }

    public bool Sorting { get; private set; }

    #endregion

    #region Methods

    protected override void OnFormClosing(FormClosingEventArgs e) {
        FixColumnArrangement();
        base.OnFormClosing(e);
    }

    private void btnAktuelleAnsichtLoeschen_Click(object sender, System.EventArgs e) {
        if (Database is not DatabaseAbstract db || _arrangement < 2 || _arrangement >= db.ColumnArrangements.Count) { return; }

        var ca = CloneOfCurrentArrangement;
        if (ca == null) { return; }

        if (MessageBox.Show("Anordung <b>'" + ca.KeyName + "'</b><br>wirklich löschen?", ImageCode.Warnung, "Ja", "Nein") != 0) { return; }
        var car = db.ColumnArrangements.CloneWithClones();
        car.RemoveAt(_arrangement);
        Database.ColumnArrangements = car.AsReadOnly();
        _arrangement = 1;
        UpdateCombobox();
        ShowOrder();
    }

    private void btnAlleSpaltenEinblenden_Click(object sender, System.EventArgs e) {
        var ca = CloneOfCurrentArrangement;
        if (ca == null) { return; }

        if (MessageBox.Show("Alle Spalten anzeigen?", ImageCode.Warnung, "Ja", "Nein") != 0) { return; }
        ca.ShowAllColumns();

        Change(_arrangement, ca);
        ShowOrder();
    }

    private void btnAnsichtUmbenennen_Click(object sender, System.EventArgs e) {
        var ca = CloneOfCurrentArrangement;
        if (ca == null) { return; }

        var n = InputBox.Show("Umbenennen:", ca.KeyName, FormatHolder.Text);
        if (string.IsNullOrEmpty(n)) { return; }
        ca.KeyName = n;

        Change(_arrangement, ca);
        UpdateCombobox();
    }

    private void btnBerechtigungsgruppen_Click(object sender, System.EventArgs e) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }
        var ca = CloneOfCurrentArrangement;
        if (ca == null) { return; }

        ItemCollectionList.ItemCollectionList aa = new(true);
        aa.AddRange(db.Permission_AllUsed(false));
        //aa.Sort();
        aa.CheckBehavior = CheckBehavior.MultiSelection;
        aa.Check(ca.PermissionGroups_Show, true);
        var b = InputBoxListBoxStyle.Show("Wählen sie, wer anzeigeberechtigt ist:<br><i>Info: Administratoren sehen alle Ansichten", aa, AddType.Text, true);
        if (b == null) { return; }
        if (_arrangement == 1) { b.Add(Constants.Everybody); }
        ca.PermissionGroups_Show = b.AsReadOnly();
        Change(_arrangement, ca);
    }

    private void btnNeueAnsichtErstellen_Click(object sender, System.EventArgs e) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }

        var ca = CloneOfCurrentArrangement;

        var mitVorlage = false;
        if (_arrangement > 0 && ca != null) {
            mitVorlage = MessageBox.Show("<b>Neue Spaltenanordnung erstellen:</b><br>Wollen sie die aktuelle Ansicht kopieren?", ImageCode.Frage, "Ja", "Nein") == 0;
        }

        var car = db.ColumnArrangements.CloneWithClones();
        //car.RemoveAt(Arrangement);

        if (car.Count < 1) {
            car.Add(new ColumnViewCollection(Database, string.Empty, string.Empty));
        }

        string newname;
        if (mitVorlage && ca != null) {
            newname = InputBox.Show("Die aktuelle Ansicht wird <b>kopiert</b>.<br><br>Geben sie den Namen<br>der neuen Anordnung ein:", string.Empty, FormatHolder.Text);
            if (string.IsNullOrEmpty(newname)) { return; }
            car.Add(new ColumnViewCollection(Database, ca.ToString(), newname));
        } else {
            newname = InputBox.Show("Geben sie den Namen<br>der neuen Anordnung ein:", string.Empty, FormatHolder.Text);
            if (string.IsNullOrEmpty(newname)) { return; }
            car.Add(new ColumnViewCollection(Database, string.Empty, newname));
        }

        Database.ColumnArrangements = car.AsReadOnly();
        _arrangement = car.Count - 1;
        UpdateCombobox();

        ShowOrder();
    }

    private void btnNeueSpalte_Click(object sender, System.EventArgs e) {
        if (Database is not DatabaseAbstract db || TableView.ErrorMessage(db, EditableErrorReasonType.EditAcut)) { return; }

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
            newc.CloneFrom(vorlage, false, true);
            if (mitDaten) {
                foreach (var thisR in db.Row) {
                    thisR.CellSet(newc, thisR.CellGetString(vorlage));
                }
            }
        }
        using (ColumnEditor w = new(newc, null)) {
            _ = w.ShowDialog();
            newc.Invalidate_ColumAndContent();
            w.Dispose();
        }
        db.Column.Repair();

        var ca = CloneOfCurrentArrangement;

        if (_arrangement > 0 && ca != null) {
            ca.Add(newc, false);
            Change(_arrangement, ca);
        }

        db.RepairAfterParse();
        ShowOrder();
    }

    private void btnSpalteEinblenden_Click(object sender, System.EventArgs e) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }

        var ca = CloneOfCurrentArrangement;
        if (ca == null) { return; }

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
        Change(_arrangement, ca);
        ShowOrder();
    }

    private void btnSystemspaltenAusblenden_Click(object sender, System.EventArgs e) {
        var ca = CloneOfCurrentArrangement;
        if (ca == null) { return; }

        ca.HideSystemColumns();
        Change(_arrangement, ca);

        ShowOrder();
    }

    private void cbxInternalColumnArrangementSelector_ItemClicked(object sender, AbstractListItemEventArgs e) {
        if (string.IsNullOrEmpty(cbxInternalColumnArrangementSelector.Text)) { return; }

        var tmporder = IntParse(e.Item.KeyName);

        if (_arrangement == tmporder) { return; }

        FixColumnArrangement();

        _arrangement = tmporder;
        ShowOrder();
        Pad.ZoomFit();
    }

    private void Change(int no, ColumnViewCollection cv) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }
        var car = db.ColumnArrangements.CloneWithClones();
        car[no] = cv;
        db.ColumnArrangements = car.AsReadOnly();
    }

    /// <summary>
    /// Überträgt die aktuelle Ansicht fest in den Datenbankcode hinein
    /// </summary>

    private void FixColumnArrangement() {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }
        if (Generating || Sorting) { return; }
        if (db.ColumnArrangements.Count == 0) { return; }

        var cloneOfColumnArrangements = db.ColumnArrangements.CloneWithClones();
        var thisColumnViewCollection = cloneOfColumnArrangements[_arrangement];

        if (thisColumnViewCollection == null) { return; }
        var did = false;

        Fixing++;

        #region Items Ordnen / erstellen / Permament

        var permanentPossible = true;

        var itemsdone = new List<AbstractPadItem>();

        do {
            var leftestItem = LeftestItem(itemsdone);
            if (leftestItem == null) { break; }

            var columnIndexWhoShouldBeThere = thisColumnViewCollection.IndexOf(thisColumnViewCollection[leftestItem.Column]);

            #region  Noch nicht in der View, wurde also hinzugfügt. Auch fest hizufügen

            if (columnIndexWhoShouldBeThere < 0) {
                thisColumnViewCollection.Add(leftestItem.Column, leftestItem.Permanent);
                columnIndexWhoShouldBeThere = thisColumnViewCollection.IndexOf(thisColumnViewCollection[leftestItem.Column]);
                did = true;
            }

            #endregion

            #region Stimmen Positionen überein?

            var columnIndexWhoIsOnPos = thisColumnViewCollection.IndexOf(thisColumnViewCollection[itemsdone.Count]);
            if (columnIndexWhoShouldBeThere != itemsdone.Count) {
                // Position stimmt nicht, also swapen
                thisColumnViewCollection.Swap(columnIndexWhoIsOnPos, columnIndexWhoShouldBeThere);
                did = true;
            }

            #endregion

            #region Permanent

            if (thisColumnViewCollection[itemsdone.Count] is ColumnViewItem cvi) {
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

        if (thisColumnViewCollection.Count > itemsdone.Count) {
            if (_arrangement > 0) {

                #region Code für Ansichten > 0

                while (thisColumnViewCollection.Count > itemsdone.Count) {
                    // Item, dass nun durch die Swaps an die letzten
                    // Stellen gewandert und zu viel sind, einfach am Ende weglöschen
                    thisColumnViewCollection.RemoveAt(thisColumnViewCollection.Count - 1);
                    did = true;
                }

                #endregion
            } else {

                #region Code für Ansicht 0

                var col = thisColumnViewCollection[thisColumnViewCollection.Count - 1]?.Column;
                if (col != null && MessageBox.Show("Spalte <b>" + col.ReadableText() + "</b> endgültig löschen?", ImageCode.Warnung,
                        "Ja", "Nein") == 0) {
                    Database.Column.Remove(col, "Benutzer löscht im ColArrangement Editor");
                    did = true;
                }

                #endregion
            }
        }

        #endregion

        Fixing--;

        db.ColumnArrangements = cloneOfColumnArrangements.AsReadOnly();

        if (did) {
            db.RepairAfterParse();
            ShowOrder();
        }
    }

    private void Item_ItemRemoved(object sender, System.EventArgs e) => Pad_MouseUp(null, null);

    private ColumnPadItem? LeftestItem(ICollection<AbstractPadItem> ignore) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return null; }
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
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }
        if (Pad?.Item == null) { return; }

        if (Generating || Fixing > 0) { Develop.DebugPrint("Generating falsch!"); }

        Generating = true;
        Pad.Item.Clear();

        ColumnPadItem? anyitem = null;

        var ca = CloneOfCurrentArrangement;
        if (ca == null) { Generating = false; return; }

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
                var dbN = thisc.LinkedDatabase.ConnectionData.TableName + "|" + thisc.LinkedCellFilter.JoinWithCr();
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
                        var toCheckCombi = c.LinkedDatabase.ConnectionData.TableName + "|" + c.LinkedCellFilter.JoinWithCr();

                        if (toCheckCombi == thisCombi) {
                            if (Pad.Item[toCheckCombi] is not GenericPadItem databItem) {
                                var nam = c.LinkedDatabase.ConnectionData.TableName;
                                databItem = new GenericPadItem(toCheckCombi, nam, new Size((int)(anyitem.UsedArea.Height / 2), (int)anyitem.UsedArea.Height));
                                Pad.Item.Add(databItem);
                                if (it != null) {
                                    databItem.SetLeftTopPoint(Math.Max(kx, it.UsedArea.Left - databItem.UsedArea.Width), 600);
                                }
                                kx = databItem.UsedArea.Right;
                            }

                            foreach (var thisitem in c.LinkedCellFilter) {
                                var tmp = thisitem.SplitBy("|");

                                if (c.Database is DatabaseAbstract db2)
                                    foreach (var thisc2 in db2.Column) {
                                        if (tmp[2].Contains("~" + thisc2.KeyName + "~")) {
                                            if (thisc2.Database is DatabaseAbstract db3) {
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