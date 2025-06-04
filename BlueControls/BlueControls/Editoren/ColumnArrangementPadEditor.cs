// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using BlueControls.ItemCollectionList;
using BlueControls.ItemCollectionPad;
using BlueControls.ItemCollectionPad.Abstract;
using BlueControls.ItemCollectionPad.FunktionsItems_ColumnArrangement_Editor;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using MessageBox = BlueControls.Forms.MessageBox;

namespace BlueControls.BlueDatabaseDialogs;

public partial class ColumnArrangementPadEditor : PadEditor, IHasDatabase, IIsEditor {

    #region Fields

    private string _arrangement = string.Empty;

    private Database? _database;

    #endregion

    #region Constructors

    public ColumnArrangementPadEditor() => InitializeComponent();

    #endregion

    #region Properties

    public Database? Database {
        get => _database;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == _database) { return; }

            if (_database != null) {
                _database.DisposingEvent -= _database_Disposing;
            }

            _database = value;

            if (_database != null) {
                _database.DisposingEvent += _database_Disposing;
            }
        }
    }

    public IEditable? ToEdit {
        set {
            if (value is ColumnViewCollection cvc) {
                Database = cvc.Database;
                _arrangement = cvc.KeyName;
                UpdateCombobox();
                ShowOrder();
            }
        }
    }

    #endregion

    #region Methods

    public ColumnViewCollection? CloneOfCurrentArrangement() {
        // Überprüfen, ob die Datenbank oder das aktuelle Objekt verworfen wurde
        if (IsDisposed || Database is not { IsDisposed: false } db) { return null; }

        var tcvc = ColumnViewCollection.ParseAll(db);

        return tcvc.Get(_arrangement);
    }

    public int IndexOfCurrentArr() {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return -1; }

        var tcvc = ColumnViewCollection.ParseAll(db);

        return tcvc.IndexOf(_arrangement);
    }

    public bool IsAllColumnView() => IndexOfCurrentArr() == 0;

    public bool IsDefaultView() => IndexOfCurrentArr() == 1;

    protected override void OnFormClosing(FormClosingEventArgs e) {
        FixColumnArrangement();
        Database = null;
        base.OnFormClosing(e);
    }

    private void _database_Disposing(object sender, System.EventArgs e) {
        Database = null;
        Close();
    }

    private void btnAktuelleAnsichtLoeschen_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return; }

        if (CloneOfCurrentArrangement() is not { IsDisposed: false } ca) { return; }

        var arn = IndexOfCurrentArr();

        if (arn < 2) { return; }

        if (MessageBox.Show("Anordung <b>'" + ca.KeyName + "'</b><br>wirklich löschen?", ImageCode.Warnung, "Ja", "Nein") != 0) { return; }
        var tcvc = ColumnViewCollection.ParseAll(db);
        tcvc.RemoveAt(arn);
        db.ColumnArrangements = tcvc.ToString(false);
        _arrangement = string.Empty;
        UpdateCombobox();
        ShowOrder();
    }

    private void btnAlleSpaltenEinblenden_Click(object sender, System.EventArgs e) {
        if (CloneOfCurrentArrangement() is not { IsDisposed: false } ca) { return; }

        if (MessageBox.Show("Alle Spalten anzeigen?", ImageCode.Warnung, "Ja", "Nein") != 0) { return; }
        ca.ShowAllColumns();

        ChangeCurrentArrangementto(ca);
        ShowOrder();
    }

    private void btnAnsichtUmbenennen_Click(object sender, System.EventArgs e) {
        if (CloneOfCurrentArrangement() is not { IsDisposed: false } ca) { return; }

        var n = InputBox.Show("Umbenennen:", ca.KeyName, FormatHolder.Text);
        if (string.IsNullOrEmpty(n)) { return; }
        ca.KeyName = n;

        ChangeCurrentArrangementto(ca);
        UpdateCombobox();
    }

    private void btnBerechtigungsgruppen_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Database is not { IsDisposed: false }) { return; }
        if (CloneOfCurrentArrangement() is not { IsDisposed: false } ca) { return; }

        List<AbstractListItem> aa = [];
        aa.AddRange(ItemsOf(Table.Permission_AllUsed(false)));
        var b = InputBoxListBoxStyle.Show("Wählen sie, wer anzeigeberechtigt ist:<br><i>Info: Administratoren sehen alle Ansichten", aa, CheckBehavior.MultiSelection, ca.PermissionGroups_Show.ToList(), AddType.Text);
        if (b == null) { return; }

        if (IsDefaultView()) { b.Add(Constants.Everybody); }
        ca.PermissionGroups_Show = b.AsReadOnly();
        ChangeCurrentArrangementto(ca);
    }

    private void btnNeueAnsichtErstellen_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return; }

        var ca = CloneOfCurrentArrangement();

        var mitVorlage = false;
        if (!IsAllColumnView() && ca != null) {
            mitVorlage = MessageBox.Show("<b>Neue Spaltenanordnung erstellen:</b><br>Wollen sie die aktuelle Ansicht kopieren?", ImageCode.Frage, "Ja", "Nein") == 0;
        }

        var tcvc = ColumnViewCollection.ParseAll(db);

        if (tcvc.Count < 1) {
            tcvc.Add(new ColumnViewCollection(db, string.Empty, string.Empty));
        }

        string newname;
        if (mitVorlage && ca != null) {
            newname = InputBox.Show("Die aktuelle Ansicht wird <b>kopiert</b>.<br><br>Geben sie den Namen<br>der neuen Anordnung ein:", string.Empty, FormatHolder.Text);
            if (string.IsNullOrEmpty(newname)) { return; }
            tcvc.Add(new ColumnViewCollection(db, ca.ParseableItems().FinishParseable(), newname));
        } else {
            newname = InputBox.Show("Geben sie den Namen<br>der neuen Anordnung ein:", string.Empty, FormatHolder.Text);
            if (string.IsNullOrEmpty(newname)) { return; }
            tcvc.Add(new ColumnViewCollection(db, string.Empty, newname));
        }

        db.ColumnArrangements = tcvc.ToString(false);
        _arrangement = newname;
        UpdateCombobox();

        ShowOrder();
    }

    private void btnNeueSpalte_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Database is not { IsDisposed: false } db || TableView.EditabelErrorMessage(db)) { return; }

        ColumnItem? vorlage = null;
        if (Pad.LastClickedItem is ColumnPadItem cpi && cpi.CVI?.Column?.Database == db) {
            vorlage = cpi.CVI?.Column;
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
        newc.Editor = typeof(ColumnEditor);

        if (vorlage != null) {
            newc.CloneFrom(vorlage, false);
            if (mitDaten) {
                foreach (var thisR in db.Row) {
                    thisR.CellSet(newc, thisR.CellGetString(vorlage), "Neue Spalte mit allem Daten");
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
            ca.Add(newc);
            ChangeCurrentArrangementto(ca);
        }

        db.RepairAfterParse();
        ShowOrder();
    }

    private void btnSpalteEinblenden_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return; }

        if (CloneOfCurrentArrangement() is not { IsDisposed: false } ca) { return; }

        var ic = ItemsOf(db.Column, true);

        foreach (var thisColumnItem in db.Column) {
            if (thisColumnItem is { IsDisposed: false } &&
                ca[thisColumnItem] is { IsDisposed: false } &&
                ic.Get(thisColumnItem.KeyName) is { } ali) {
                ali.Enabled = false;
            }
        }

        var r = InputBoxListBoxStyle.Show("Wählen sie:", ic, CheckBehavior.SingleSelection, null, AddType.None);
        if (r is not { Count: not 0 }) { return; }

        if (db.Column[r[0]] is not { IsDisposed: false } col) { return; }

        ca.Add(col);
        ChangeCurrentArrangementto(ca);
        ShowOrder();
    }

    private void btnSystemspaltenAusblenden_Click(object sender, System.EventArgs e) {
        if (CloneOfCurrentArrangement() is not { IsDisposed: false } ca) { return; }

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
        if (IsDisposed || Database is not { IsDisposed: false } db) { return; }

        var tcvc = ColumnViewCollection.ParseAll(db);

        var no = IndexOfCurrentArr();
        if (no < 0 || no >= tcvc.Count) { return; }

        tcvc[no] = cv;
        db.ColumnArrangements = tcvc.ToString(false);
    }

    /// <summary>
    /// Überträgt die aktuelle Ansicht fest in den Datenbankcode hinein
    /// </summary>

    private void chkShowCaptions_CheckedChanged(object sender, System.EventArgs e) {
    }

    private void chkShowCaptions_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Database is not { IsDisposed: false }) { return; }

        if (CloneOfCurrentArrangement() is not { IsDisposed: false } ca) { return; }

        ca.ShowHead = chkShowCaptions.Checked;

        ChangeCurrentArrangementto(ca);
        //ShowOrder();
    }

    private void FixColumnArrangement() {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return; }

        if (CloneOfCurrentArrangement() is not { IsDisposed: false } ca) { return; }

        var oldcode = ca.ParseableItems().FinishParseable();

        ca.RemoveAll();

        var permanentPossible = true;

        var itemsdone = new List<AbstractPadItem>();

        do {
            var leftestItem = LeftestItem(itemsdone);
            if (leftestItem?.CVI is not { IsDisposed: false } cvi) { break; }

            var item = new ColumnViewItem(ca, cvi.ParseableItems().FinishParseable());

            if (!permanentPossible) { item.ViewType = ViewType.Column; }
            if (item.ViewType != ViewType.PermanentColumn) { permanentPossible = false; }

            ca.Add(item);
            itemsdone.Add(leftestItem);
        } while (true);

        var did = oldcode != ca.ParseableItems().FinishParseable();

        #region Prüfen, ob Items gelöscht wurde, diese dann ebenfalls löschen

        if (IsAllColumnView()) {
            foreach (var thiscol in db.Column) {
                if (thiscol != null && ca[thiscol] is null) {
                    if (MessageBox.Show("Spalte <b>" + thiscol.ReadableText() + "</b> endgültig löschen?", ImageCode.Warnung,
                            "Ja", "Nein") == 0) {
                        _ = db.Column.Remove(thiscol, "Benutzer löscht im ColArrangement Editor");
                        did = true;
                    }
                }
            }
        }

        #endregion

        if (did) {
            ChangeCurrentArrangementto(ca);
            db.RepairAfterParse();
            ShowOrder();
        }
    }

    private void Item_ItemRemoved(object sender, System.EventArgs e) => Pad_MouseUp(null, null);

    private ColumnPadItem? LeftestItem(IEnumerable<AbstractPadItem> ignore) {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return null; }
        if (Pad?.Items is not { IsDisposed: false } ic) { return null; }

        ColumnPadItem? found = null;

        foreach (var thisIt in ic) {
            if (!ignore.Contains(thisIt) && thisIt is ColumnPadItem fi) {
                if (fi.CVI?.Column?.Database == db) {
                    if (found == null || fi.UsedArea.X < found.UsedArea.X) {
                        found = fi;
                    }
                }
            }
        }

        return found;
    }

    private void Pad_MouseUp(object? sender, MouseEventArgs? e) {
        SortColumns();
        FixColumnArrangement();
    }

    private void ShowOrder() {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return; }

        Pad.Items = [];
        Pad.Items.Endless = true;
        Pad.Items.ForPrinting = true;

        Pad.Items.Clear();

        ColumnPadItem? anyitem = null;

        if (CloneOfCurrentArrangement() is not { IsDisposed: false } ca) {
            return;
        }

        #region Erst alle Spalten der eigenen Datenbank erzeugen, um später verweisen zu können

        var x = 0f;
        foreach (var thisColumnViewItem in ca) {
            if (thisColumnViewItem?.Column is { IsDisposed: false } c) {
                var it = new ColumnPadItem(thisColumnViewItem, thisColumnViewItem.GetRenderer(Constants.Win11));
                Pad.Items.Add(it);
                it.SetLeftTopPoint(x, 0);
                x = it.UsedArea.Right;
                anyitem = it;
            }
        }

        #endregion

        if (anyitem == null) { return; }

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
                if (thisc?.Column is { IsDisposed: false } c) {
                    var it = Pad.Items[c.Database?.TableName + "|" + c.KeyName] as ColumnPadItem;

                    if (c.LinkedDatabase != null) {
                        // String als Namen als eindeutige Kennung
                        var toCheckCombi = c.LinkedDatabase.TableName + "|" + c.LinkedCellFilter.JoinWithCr();

                        if (toCheckCombi == thisCombi) {
                            if (Pad.Items[toCheckCombi] is not TextPadItem databItem) {
                                var nam = c.LinkedDatabase.TableName;
                                databItem = new TextPadItem(toCheckCombi, nam);
                                Pad.Items.Add(databItem);
                                if (it != null) {
                                    var r = new RectangleF(Math.Max(kx, it.UsedArea.Left - databItem.UsedArea.Width),
                                                            600,
                                                           (int)(anyitem.UsedArea.Height / 2),
                                                           (int)anyitem.UsedArea.Height);

                                    databItem.SetCoordinates(r);
                                }
                                kx = databItem.UsedArea.Right;
                            }

                            foreach (var thisitem in c.LinkedCellFilter) {
                                var tmp = thisitem.SplitBy("|");

                                if (c.Database is { IsDisposed: false } db2) {
                                    foreach (var thisc2 in db2.Column) {
                                        if (tmp[2].Contains("~" + thisc2.KeyName + "~")) {
                                            if (thisc2.Database is { IsDisposed: false } db3) {
                                                var rkcolit = (ColumnPadItem?)Pad.Items[db3.TableName + "|" + thisc2.KeyName];
                                                if (rkcolit != null) {
                                                    _ = Pad.Items.Connections.AddIfNotExists(new ItemConnection(rkcolit, ConnectionType.Bottom, false, databItem, ConnectionType.Top, true, false));
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            var c2 = c.LinkedDatabase.Column[c.ColumnNameOfLinkedDatabase];
                            if (c2 != null) {
                                var it2 = new ColumnPadItem(new ColumnViewItem(c2, ca), thisc.GetRenderer(Constants.Win11));
                                Pad.Items.Add(it2);
                                it2.SetLeftTopPoint(kx, 600);
                                if (it != null) {
                                    Pad.Items.Connections.Add(new ItemConnection(it2, ConnectionType.Top, false, it, ConnectionType.Bottom, true, false));
                                }
                                kx = it2.UsedArea.Right;

                                // und noch die Datenbank auf die Spalte zeigen lassem
                                _ = Pad.Items.Connections.AddIfNotExists(new ItemConnection(databItem, ConnectionType.Bottom, false, it2, ConnectionType.Bottom, false, false));
                            }
                        }
                    }
                }
            }

            kx += 30;
        }

        SortColumns();

        chkShowCaptions.Checked = ca.ShowHead;
    }

    private void SortColumns() {
        var done = new List<AbstractPadItem>();
        var left = 0f;
        do {
            var x = LeftestItem(done);
            if (x == null) { break; }
            done.Add(x);
            x.SetLeftTopPoint(left, 0);
            left = x.UsedArea.Right;
        } while (true);
    }

    private void UpdateCombobox() => Table.WriteColumnArrangementsInto(cbxInternalColumnArrangementSelector, Database, _arrangement);

    #endregion
}