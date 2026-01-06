// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueControls.ItemCollectionPad.Abstract;
using BlueControls.ItemCollectionPad.FunktionsItems_ColumnArrangement_Editor;
using BlueTable;
using BlueTable.Enums;
using BlueTable.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.BlueTableDialogs;

public partial class ColumnArrangementPadEditor : PadEditor, IHasTable, IIsEditor {

    #region Fields

    private string _arrangement = string.Empty;

    #endregion

    #region Constructors

    public ColumnArrangementPadEditor() => InitializeComponent();

    #endregion

    #region Properties

    public virtual Type? EditorFor => typeof(ColumnViewCollection);

    public Table? Table {
        get;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == field) { return; }

            if (field != null) {
                field.DisposingEvent -= _table_Disposing;
            }

            field = value;

            if (field != null) {
                field.DisposingEvent += _table_Disposing;
            }
        }
    }

    public IEditable? ToEdit {
        set {
            if (value is ColumnViewCollection cvc) {
                Table = cvc.Table;
                _arrangement = cvc.KeyName;
                UpdateCombobox();
                ShowOrder();
            }
        }
    }

    #endregion

    #region Methods

    public ColumnViewCollection? CloneOfCurrentArrangement() {
        // Überprüfen, ob die Tabelle oder das aktuelle Objekt verworfen wurde
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return null; }

        var tcvc = ColumnViewCollection.ParseAll(tb);

        return tcvc.GetByKey(_arrangement);
    }

    public int IndexOfCurrentArr() {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return -1; }

        var tcvc = ColumnViewCollection.ParseAll(tb);

        return tcvc.IndexOf(_arrangement);
    }

    public bool IsAllColumnView() => IndexOfCurrentArr() == 0;

    public bool IsDefaultView() => IndexOfCurrentArr() == 1;

    protected override void OnFormClosing(FormClosingEventArgs e) {
        FixColumnArrangement();
        Table = null;
        base.OnFormClosing(e);
    }

    protected override void Pad_ClickedItemChanged(object sender, System.EventArgs e) {
        base.Pad_ClickedItemChanged(sender, e);

        if (Pad.LastClickedItem == null && Pad.Items != null) {
            foreach (var thisIt in Pad.Items) {
                if (thisIt is DummyHeadPadItem dhpi) {
                    Pad.LastClickedItem = dhpi;
                    return;
                }
            }
        }

        //LastClickedItem_DoUpdateSideOptionMenu(this, System.EventArgs.Empty);
    }

    private void _table_Disposing(object sender, System.EventArgs e) {
        Table = null;
        Close();
    }

    private void btnAktuelleAnsichtLoeschen_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        if (CloneOfCurrentArrangement() is not { IsDisposed: false } ca) { return; }

        var arn = IndexOfCurrentArr();

        if (arn < 2) { return; }

        if (Forms.MessageBox.Show("Anordung <b>'" + ca.KeyName + "'</b><br>wirklich löschen?", ImageCode.Warnung, "Ja", "Nein") != 0) { return; }
        var tcvc = ColumnViewCollection.ParseAll(tb);
        tcvc.RemoveAt(arn);
        tb.ColumnArrangements = tcvc.ToString(false);
        _arrangement = string.Empty;
        UpdateCombobox();
        ShowOrder();
    }

    private void btnAlleSpaltenEinblenden_Click(object sender, System.EventArgs e) {
        if (CloneOfCurrentArrangement() is not { IsDisposed: false } ca) { return; }

        if (Forms.MessageBox.Show("Alle Spalten anzeigen?", ImageCode.Warnung, "Ja", "Nein") != 0) { return; }
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
        if (IsDisposed || Table is not { IsDisposed: false }) { return; }
        if (CloneOfCurrentArrangement() is not { IsDisposed: false } ca) { return; }

        List<AbstractListItem> aa = [];
        aa.AddRange(ItemsOf(TableView.Permission_AllUsed(false)));
        var b = InputBoxListBoxStyle.Show("Wählen sie, wer anzeigeberechtigt ist:<br><i>Info: Administratoren sehen alle Ansichten", aa, CheckBehavior.MultiSelection, [.. ca.PermissionGroups_Show], AddType.Text);
        if (b == null) { return; }

        if (IsDefaultView()) { b.Add(Constants.Everybody); }
        ca.PermissionGroups_Show = b.AsReadOnly();
        ChangeCurrentArrangementto(ca);
    }

    private void btnNeueAnsichtErstellen_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        var ca = CloneOfCurrentArrangement();

        var mitVorlage = false;
        if (!IsAllColumnView() && ca != null) {
            mitVorlage = Forms.MessageBox.Show("<b>Neue Spaltenanordnung erstellen:</b><br>Wollen sie die aktuelle Ansicht kopieren?", ImageCode.Frage, "Ja", "Nein") == 0;
        }

        var tcvc = ColumnViewCollection.ParseAll(tb);

        if (tcvc.Count < 1) {
            tcvc.Add(new ColumnViewCollection(tb, string.Empty, string.Empty));
        }

        string newname;
        if (mitVorlage && ca != null) {
            newname = InputBox.Show("Die aktuelle Ansicht wird <b>kopiert</b>.<br><br>Geben sie den Namen<br>der neuen Anordnung ein:", string.Empty, FormatHolder.Text);
            if (string.IsNullOrEmpty(newname)) { return; }
            tcvc.Add(new ColumnViewCollection(tb, ca.ParseableItems().FinishParseable(), newname));
        } else {
            newname = InputBox.Show("Geben sie den Namen<br>der neuen Anordnung ein:", string.Empty, FormatHolder.Text);
            if (string.IsNullOrEmpty(newname)) { return; }
            tcvc.Add(new ColumnViewCollection(tb, string.Empty, newname));
        }

        tb.ColumnArrangements = tcvc.ToString(false);
        _arrangement = newname;
        UpdateCombobox();

        ShowOrder();
    }

    private void btnNeueSpalte_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } tb || TableViewForm.EditabelErrorMessage(tb)) { return; }

        FixColumnArrangement();

        ColumnItem? vorlage = null;
        if (Pad.LastClickedItem is ColumnPadItem cpi && cpi.CVI?.Column?.Table == tb) {
            vorlage = cpi.CVI?.Column;
        }

        var mitDaten = false;
        if (vorlage?.IsSystemColumn() == true) { vorlage = null; }
        if (vorlage != null) {
            switch (Forms.MessageBox.Show("Spalte '" + vorlage.ReadableText() + "' als<br>Vorlage verwenden?", ImageCode.Frage, "Ja", "Ja, mit allen Daten", "Nein", "Abbrechen")) {
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
        var newc = tb.Column.GenerateAndAdd();

        if (newc == null) { return; }

        if (vorlage != null) {
            newc.CloneFrom(vorlage, false);
            if (mitDaten) {
                foreach (var thisR in tb.Row) {
                    thisR.CellSet(newc, thisR.CellGetString(vorlage), "Neue Spalte mit allem Daten");
                }
            }
        }
        newc.Edit();
        newc.Invalidate_ColumAndContent();

        tb.Column.Repair();

        var ca = CloneOfCurrentArrangement();

        if (!IsAllColumnView() && ca != null) {
            ca.Add(newc);
            ChangeCurrentArrangementto(ca);
        }

        tb.RepairAfterParse();
        ShowOrder();
    }

    private void btnSpalteEinblenden_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        if (CloneOfCurrentArrangement() is not { IsDisposed: false } ca) { return; }

        var ic = ItemsOf(tb.Column, true);

        foreach (var thisColumnItem in tb.Column) {
            if (thisColumnItem is { IsDisposed: false } &&
                ca[thisColumnItem] is { IsDisposed: false } &&
                ic.GetByKey(thisColumnItem.KeyName) is { } ali) {
                ali.Enabled = false;
            }
        }

        var r = InputBoxListBoxStyle.Show("Wählen sie:", ic, CheckBehavior.SingleSelection, null, AddType.None);
        if (r is not { Count: not 0 }) { return; }

        if (tb.Column[r[0]] is not { IsDisposed: false } col) { return; }

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

    private void butSystemspaltenErstellen_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } tb || TableViewForm.EditabelErrorMessage(tb)) { return; }
        FixColumnArrangement();
        tb.Column.GenerateAndAddSystem();
        tb.RepairAfterParse();
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
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        var tcvc = ColumnViewCollection.ParseAll(tb);

        var no = IndexOfCurrentArr();
        if (no < 0 || no >= tcvc.Count) { return; }

        tcvc[no] = cv;
        tb.ColumnArrangements = tcvc.ToString(false);
    }

    private void FixColumnArrangement() {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        if (CloneOfCurrentArrangement() is not { IsDisposed: false } ca) { return; }

        var oldcode = ca.ParseableItems().FinishParseable();

        var view = Pad.ViewToString().FinishParseable();
        if (Pad.Fitting) { view = string.Empty; }

        ca.RemoveAll();

        var permanentPossible = true;

        var itemsdone = new List<AbstractPadItem>();

        do {
            var leftestItem = LeftestItem(itemsdone);
            if (leftestItem?.CVI is not { IsDisposed: false } cvi) { break; }

            var item = new ColumnViewItem(tb, cvi.ParseableItems().FinishParseable());

            if (!permanentPossible) { item.ViewType = ViewType.Column; }
            if (item.ViewType != ViewType.PermanentColumn) { permanentPossible = false; }

            ca.Add(item);
            itemsdone.Add(leftestItem);
        } while (true);

        if (Pad.Items != null) {
            foreach (var thisItem in Pad.Items) {
                if (thisItem is DummyHeadPadItem d) {
                    ca.ShowHead = d.ShowHead;
                    ca.FilterRows = d.FilterRows;
                    ca.ColumnForChapter = tb.Column[d.Chapter_Column];
                    break;
                }
            }
        }

        var did = oldcode != ca.ParseableItems().FinishParseable();

        #region Prüfen, ob Items gelöscht wurde, diese dann ebenfalls löschen

        if (IsAllColumnView()) {
            foreach (var thiscol in tb.Column) {
                if (thiscol != null && ca[thiscol] is null) {
                    if (Forms.MessageBox.Show("Spalte <b>" + thiscol.ReadableText() + "</b> endgültig löschen?", ImageCode.Warnung,
                            "Ja", "Nein") == 0) {
                        tb.Column.Remove(thiscol, "Benutzer löscht im ColArrangement Editor");
                        did = true;
                    }
                }
            }
        }

        #endregion

        if (did) {
            ChangeCurrentArrangementto(ca);
            tb.RepairAfterParse();
            ShowOrder();

            if (!string.IsNullOrEmpty(view)) { Pad.ParseView(view); }
        }
    }

    private void Item_ItemRemoved(object sender, System.EventArgs e) => Pad_MouseUp(null, null);

    private ColumnPadItem? LeftestItem(IEnumerable<AbstractPadItem> ignore) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return null; }
        if (Pad?.Items is not { IsDisposed: false } ic) { return null; }

        ColumnPadItem? found = null;

        foreach (var thisIt in ic) {
            if (!ignore.Contains(thisIt) && thisIt is ColumnPadItem fi) {
                if (fi.CVI?.Column?.Table == tb) {
                    if (found == null || fi.CanvasUsedArea.X < found.CanvasUsedArea.X) {
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
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        Pad.Items = [];
        Pad.Items.Endless = true;
        Pad.Items.ForPrinting = true;

        Pad.Items.Clear();

        if (CloneOfCurrentArrangement() is not { IsDisposed: false } ca) {
            return;
        }

        #region Erst alle Spalten der eigenen Tabelle erzeugen, um später verweisen zu können

        var x = 0f;
        foreach (var thisColumnViewItem in ca) {
            if (thisColumnViewItem?.Column is { IsDisposed: false } c) {
                var it = new ColumnPadItem(thisColumnViewItem, thisColumnViewItem.GetRenderer(Constants.Win11));
                Pad.Items.Add(it);
                it.SetLeftTopPoint(x, 0);
                x = it.CanvasUsedArea.Right;
            }
        }

        #endregion

        SortColumns();

        var t = new DummyHeadPadItem(tb);
        t.Page = "xxx";
        t.ShowHead = ca.ShowHead;
        t.FilterRows = ca.FilterRows;
        t.Chapter_Column = ca.ColumnForChapter?.KeyName ?? "#ohne";

        Pad.Items.Add(t);

        Pad.LastClickedItem = t;
    }

    private void SortColumns() {
        var done = new List<AbstractPadItem>();
        var left = 0f;
        do {
            var x = LeftestItem(done);
            if (x == null) { break; }
            done.Add(x);
            x.SetLeftTopPoint(left, 0);
            left = x.CanvasUsedArea.Right;
        } while (true);
    }

    private void UpdateCombobox() => TableView.WriteColumnArrangementsInto(cbxInternalColumnArrangementSelector, Table, _arrangement);

    #endregion
}