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
using BlueBasics.Classes;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Classes.ItemCollectionList;
using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueScript.Classes;
using BlueScript.EventArgs;
using BlueTable.Classes;
using BlueTable.Enums;
using BlueTable.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using static BlueBasics.ClassesStatic.Constants;
using static BlueBasics.ClassesStatic.IO;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.BlueTableDialogs;

public sealed partial class TableScriptEditor : ScriptEditorGeneric, IHasTable, IIsEditor {

    #region Fields

    private bool _allowTemporay;

    private bool _didMessage;

    private TableScriptDescription? _item;

    private bool _loaded;

    #endregion

    #region Constructors

    public TableScriptEditor() : base() {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
        tbcScriptEigenschaften.Enabled = false;
    }

    #endregion

    #region Properties

    public Type? EditorFor => null;

    public TableScriptDescription? Item {
        get => IsDisposed || Table is not { IsDisposed: false } ? null : _item;
        set {
            if (IsDisposed || Table is not { IsDisposed: false }) { value = null; }
            if (_item == value) { return; }

            _item = null; // Um keine werte zurück zu Schreiben während des Anzeigens

            if (value != null) {
                tbcScriptEigenschaften.Enabled = true;
                txbName.Text = value.KeyName;
                txbQuickInfo.Text = value.QuickInfo;

                cbxPic.Text = value.Image;

                chkZeile.Checked = value.NeedRow;
                txbTestZeile.Enabled = value.NeedRow;
                chkReadOnly.Checked = value.ValuesReadOnly || TableScriptDescription.MustBeReadonly(value.EventTypes);
                chkReadOnly.Enabled = !TableScriptDescription.MustBeReadonly(value.EventTypes);
                chkAuslöser_newrow.Checked = value.EventTypes.HasFlag(ScriptEventTypes.InitialValues);
                chkAuslöser_valuechanged.Checked = value.EventTypes.HasFlag(ScriptEventTypes.value_changed);
                chkExtendend.Visible = chkAuslöser_valuechanged.Checked;
                chkAuslöser_valuechangedThread.Checked = value.EventTypes.HasFlag(ScriptEventTypes.value_changed_extra_thread);
                chkAuslöser_prepaireformula.Checked = value.EventTypes.HasFlag(ScriptEventTypes.prepare_formula);
                chkAuslöser_export.Checked = value.EventTypes.HasFlag(ScriptEventTypes.export);
                chkAuslöser_deletingRow.Checked = value.EventTypes.HasFlag(ScriptEventTypes.row_deleting);
                Script = value.Script;
                LastFailedReason = value.FailedReason;

                lstPermissionExecute.ItemClear();
                var l = TableView.Permission_AllUsed(false).ToList();
                l.AddIfNotExists(Administrator);
                lstPermissionExecute.ItemAddRange(l);
                lstPermissionExecute.Check(value.UserGroups, true);
                lstPermissionExecute.Suggestions.Clear();

                _item = value;

                btnVerlauf.Enabled = true;
                btnAnzeigen_Click(null, System.EventArgs.Empty);
                if (value.IsOk()) {
                    capFehler.Text = "<imagecode=Häkchen|16> Keine Skript-Konflikte.";
                } else {
                    capFehler.Text = "<imagecode=Warnung|16> " + value.ErrorReason();
                }
            } else {
                tbcScriptEigenschaften.Enabled = false;
                tbcScriptEigenschaften.Enabled = false;
                txbTestZeile.Enabled = false;
                chkReadOnly.Enabled = false;
                txbName.Text = string.Empty;
                cbxPic.Text = string.Empty;
                txbQuickInfo.Text = string.Empty;
                Script = string.Empty;
                LastFailedReason = string.Empty;
                chkAuslöser_newrow.Checked = false;
                chkAuslöser_valuechanged.Checked = false;
                chkAuslöser_prepaireformula.Checked = false;
                chkAuslöser_valuechangedThread.Checked = false;
                chkAuslöser_export.Checked = false;
                btnVerlauf.Enabled = false;
                capFehler.Text = string.Empty;
            }
        }
    }

    public override object? Object {
        get => ToEdit;
        set => ToEdit = value as IEditable;
    }

    public Table? Table {
        get;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == field) { return; }

            WriteInfosBack();
            lstEventScripts.UncheckAll();

            if (field != null) {
                field.DisposingEvent -= _table_Disposing;
                field.CanDoScript -= Table_CanDoScript;
            }
            field = value;

            if (field != null) {
                field.DisposingEvent += _table_Disposing;
                field.CanDoScript += Table_CanDoScript;

                tbcScriptEigenschaften.Enabled = true;
            } else {
                tbcScriptEigenschaften.Enabled = false;
            }

            UpdateList();
        }
    }

    public IEditable? ToEdit {
        get => Table;
        set => Table = value as Table;
    }

    #endregion

    #region Methods

    public override ScriptEndedFeedback ExecuteScript(bool testmode) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) {
            return new ScriptEndedFeedback("Keine Tabelle geladen.", false, false, "Allgemein");
        }

        if (_item == null) {
            return new ScriptEndedFeedback("Kein Skript gewählt.", false, false, "Allgemein");
        }

        //if (!_item.IsOk()) {
        //    return new ScriptEndedFeedback("Bitte zuerst den Fehler korrigieren: " + _item.ErrorReason(), false, false, "Allgemein");
        //}

        WriteInfosBack();

        RowItem? r = null;

        if (_item.NeedRow) {
            if (tb.Row.Count == 0) {
                return new ScriptEndedFeedback("Zum Test wird zumindest eine Zeile benötigt.", false, false, "Allgemein");
            }

            if (string.IsNullOrEmpty(txbTestZeile.Text)) {
                txbTestZeile.Text = tb.Row.First()?.CellFirstString() ?? string.Empty;
            }

            if (string.IsNullOrEmpty(txbTestZeile.Text)) {
                txbTestZeile.Text = tb.Row.First()?.KeyName ?? string.Empty;
            }

            r = tb.Row[txbTestZeile.Text] ?? tb.Row.GetByKey(txbTestZeile.Text);
            if (r is not { IsDisposed: false }) {
                return new ScriptEndedFeedback("Zeile nicht gefunden.", false, false, "Allgemein");
            }
        }

        if (!testmode) {
            if (Forms.MessageBox.Show("Skript ändert Werte!<br>Fortfahren?", ImageCode.Warnung, "Fortfahren", "Abbruch") != 0) {
                return new ScriptEndedFeedback("Abbruch.", false, false, "Allgemein");
            }
        }

        var ext = chkExtendend is { Checked: true, Visible: true };

        _allowTemporay = true;
        var f = tb.ExecuteScript(_item, !testmode, r, null, true, ext, true);
        _allowTemporay = false;

        return f;
    }

    public void UpdateSelectedItem(string? keyName = null, string? quickInfo = null, string? image = null, bool? needRow = null, bool? readOnly = null, ScriptEventTypes? eventTypes = null, string? script = null, ReadOnlyCollection<string>? userGroups = null, string? adminInfo = null, string? failedReason = null, bool isDisposed = false, int? stoppedtimecount = null, long? averageruntime = null) {
        if (IsDisposed || TableViewForm.EditabelErrorMessage(Table) || Table is not { IsDisposed: false } tb) { return; }

        if (_item == null) {
            capFehler.Text = string.Empty;
            return;
        }

        var tmpname = keyName ?? _item.KeyName;

        // Backend-Update
        tb.UpdateScript(_item.KeyName, keyName, script, image, quickInfo, adminInfo, eventTypes, needRow, userGroups, failedReason, isDisposed, readOnly, stoppedtimecount, averageruntime);
        UpdateList();

        Item = tb.EventScript.GetByKey(tmpname);
    }

    public override void WriteInfosBack() => UpdateSelectedItem(script: Script, keyName: txbName.Text, failedReason: LastFailedReason);

    protected override void OnFormClosing(FormClosingEventArgs e) {
        base.OnFormClosing(e);
        Item = null; // erst das Item!
        Table = null;
    }

    protected override void OnLoad(System.EventArgs e) {
        base.OnLoad(e);

        var im = QuickImage.Images();
        foreach (var thisIm in im) {
            cbxPic.ItemAdd(ItemOf(thisIm, thisIm, QuickImage.Get(thisIm, 16)));
        }
    }

    private void _table_Disposing(object sender, System.EventArgs e) {
        Table = null;
        Close();
    }

    private void btnSpaltenuebersicht_Click(object sender, System.EventArgs e) => Table?.Column.GenerateOverView();

    private void btnTabelleKopf_Click(object sender, System.EventArgs e) => InputBoxEditor.Show(Table, typeof(TableHeadEditor), false);

    private void btnTest_Click(object sender, System.EventArgs e) {
        if (!_loaded && Table is { Row.Count: 0 }) {
            _loaded = true;
            Table.LoadTableRows(false, 5);
        }
        TesteScript(true);
    }

    private void btnVerlauf_Click(object sender, System.EventArgs e) {
        // Überprüfen, ob die Tabelle oder die Instanz selbst verworfen wurde
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        // Das ausgewählte Skript aus der Liste abrufen
        var selectedlstEventScripts = lstEventScripts[lstEventScripts.Checked[0]] is ReadableListItem item ? (TableScriptDescription)item.Item : null;
        var l = new List<string>();
        // Durchlaufen aller Undox-Operationen in der Tabelle

        var sortedUndoItems = tb.Undo.Where(item => item.Command is TableDataType.EventScript).OrderByDescending(item => item.DateTimeUtc);

        foreach (var thisUndo in sortedUndoItems) {
            l.Add("############################################################################");
            l.Add("############################################################################");
            l.Add("############################################################################");
            l.Add("############################################################################");
            l.Add("############################################################################");
            l.Add(thisUndo.DateTimeUtc.ToString7() + " " + thisUndo.User);

            l.Add("Art: " + thisUndo.Command);
            // Überprüfen, ob das Skript geändert wurde
            var ai = thisUndo.ChangedTo.SplitAndCutByCr().ToList();
            var found = false;
            foreach (var t in ai) {
                var s = new TableScriptDescription(tb, t);
                if (s.KeyName == selectedlstEventScripts?.KeyName && selectedlstEventScripts.Script != s.Script) {
                    l.Add(s.Script);
                    found = true;
                    break;
                }
            }
            if (!found) {
                l.Add("    -> Keine Änderung am gewählten Skript");
            }
        }
        // Schreiben der Liste in eine temporäre Datei
        l.WriteAllText(TempFile(string.Empty, "Scrip.txt"), Win1252, true);
    }

    private void btnVersionErhöhen_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false }) { return; }

        btnVersionErhöhen.Enabled = false;

        Table.EventScriptVersion = DateTime.UtcNow;
    }

    private void btnZusatzDateien_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }
        ExecuteFile(tb.AdditionalFilesPathWhole());
    }

    private void cbxPic_TextChanged(object sender, System.EventArgs e) => UpdateSelectedItem(image: cbxPic.Text.TrimEnd("|16"));

    private void chkAuslöser_newrow_CheckedChanged(object sender, System.EventArgs e) {
        if (_item == null) { return; }

        ScriptEventTypes tmp = 0;
        if (chkAuslöser_newrow.Checked) { tmp |= ScriptEventTypes.InitialValues; }
        if (chkAuslöser_valuechanged.Checked) { tmp |= ScriptEventTypes.value_changed; }
        if (chkAuslöser_prepaireformula.Checked) { tmp |= ScriptEventTypes.prepare_formula; }
        if (chkAuslöser_valuechangedThread.Checked) { tmp |= ScriptEventTypes.value_changed_extra_thread; }
        if (chkAuslöser_export.Checked) { tmp |= ScriptEventTypes.export; }
        if (chkAuslöser_deletingRow.Checked) { tmp |= ScriptEventTypes.row_deleting; }

        UpdateSelectedItem(eventTypes: tmp);
    }

    private void chkReadOnly_CheckedChanged(object sender, System.EventArgs e) {
        if (_item == null) { return; }

        if (IsDisposed || Table is not { IsDisposed: false }) { return; }

        UpdateSelectedItem(readOnly: chkReadOnly.Checked);
    }

    private void chkZeile_CheckedChanged(object sender, System.EventArgs e) {
        if (_item == null) { return; }

        if (IsDisposed || Table is not { IsDisposed: false }) { return; }

        if (chkZeile.Checked && !Table.IsRowScriptPossible()) {
            if (!EnableScript()) { chkZeile.Checked = false; }

            return;
        }

        UpdateSelectedItem(needRow: chkZeile.Checked);
        txbTestZeile.Enabled = chkZeile.Checked;
    }

    private bool EnableScript() {
        if (IsDisposed || Table is not { IsDisposed: false }) { return false; }

        var s = Forms.MessageBox.Show("Für Zeilenskripte werden bestimmte Systemspalten benötigt.<br>Sollen diese erstellt werden?", ImageCode.Spalte, "Ja", "Nein");

        if (s == 1) { return false; }

        Table.EnableScript();

        if (!Table.IsRowScriptPossible()) {
            Forms.MessageBox.Show("Systemspalten konnten nicht erstellt werden.", ImageCode.Information, "Ok");
            return false;
        }
        return true;
    }

    private void GlobalTab_SelectedIndexChanged(object sender, System.EventArgs e) => WriteInfosBack();

    private void lstEventScripts_AddClicked(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        var newitem = new TableScriptDescription(Table);

        if (tb.EventScript.GetByKey(newitem.KeyName) != null) {
            Notification.Show("Es ist bereits ein neues Skript vorhanden,\r\ndieses zuerst bearbeiten.");
            return;
        }

        List<TableScriptDescription> l = [.. tb.EventScript];
        l.Add(newitem);
        tb.EventScript = l.AsReadOnly();

        UpdateList();
        lstEventScripts.Check(newitem.KeyName);
    }

    private void lstEventScripts_ItemCheckedChanged(object sender, System.EventArgs e) {
        var newItem = string.Empty;
        if (lstEventScripts.Checked.Count == 1 &&
            !TableViewForm.EditabelErrorMessage(Table)) {
            if (lstEventScripts[lstEventScripts.Checked[0]] is ReadableListItem rli) {
                newItem = rli.KeyName;
            }
        }

        WriteInfosBack();

        Item = Table?.EventScript.GetByKey(newItem);
    }

    private void lstEventScripts_RemoveClicked(object sender, AbstractListItemEventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        if (e.Item is not ReadableListItem rli || rli.Item is not TableScriptDescription dsd) { return; }

        var toDelete = dsd.KeyName;

        if (dsd != _item) { WriteInfosBack(); }

        //lstEventScripts.UncheckAll();

        var toDel = tb.EventScript.GetByKey(toDelete);

        if (toDel is null) { return; }

        Table.UpdateScript(toDel, isDisposed: true);
        UpdateList();
    }

    private void lstPermissionExecute_ItemClicked(object sender, AbstractListItemEventArgs e) => UpdateSelectedItem(userGroups: lstPermissionExecute.Checked.ToList().AsReadOnly());

    private void Table_CanDoScript(object sender, CanDoScriptEventArgs e) {
        if (_allowTemporay) { return; }
        e.CancelReason = "Skript-Editor geöffnet";
    }

    private void txbName_TextChanged(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }
        if (_item == null) { return; }

        if (!ScriptDescription.IsValidName(txbName.Text)) { return; }

        if (!string.Equals(txbName.Text, _item.KeyName, StringComparison.OrdinalIgnoreCase)) {
            if (tb.EventScript.GetByKey(txbName.Text) != null) { return; }
        }

        UpdateSelectedItem(keyName: txbName.Text);
    }

    private void txbQuickInfo_TextChanged(object sender, System.EventArgs e) => UpdateSelectedItem(quickInfo: txbQuickInfo.Text);

    private void UpdateList() {
        if (IsDisposed || Table is not { IsDisposed: false } tb) {
            lstEventScripts.ItemClear();
            return;
        }

        #region Veraltete Items ermitteln

        var toRemove = new List<ReadableListItem>();

        foreach (var thisSet in lstEventScripts.Items) {
            if (thisSet is ReadableListItem rli) {
                if (tb.EventScript.GetByKey(rli.KeyName) == null) {
                    toRemove.Add(rli);
                }
            }
        }

        #endregion

        #region Veraltete Items entfernen

        foreach (var thisSet in toRemove) {
            lstEventScripts.Remove(thisSet);
        }

        #endregion

        #region Neue Items hinzufügen

        foreach (var thisSet in tb.EventScript) {
            if (thisSet != null) {
                var cap = "Sonstige";

                if (thisSet.EventTypes != 0) { cap = thisSet.EventTypes.ToString(); }

                // Prüfen ob das Item bereits existiert
                var existingItem = lstEventScripts[thisSet.KeyName];
                if (existingItem == null) {
                    existingItem = ItemOf(thisSet);
                    lstEventScripts.ItemAdd(existingItem);
                }

                if (existingItem is ReadableListItem rli) {
                    existingItem.UserDefCompareKey = cap + SecondSortChar + thisSet.CompareKey;
                    rli.Item = thisSet;
                }

                // Kategorie-Item hinzufügen falls nicht vorhanden
                if (lstEventScripts[cap] == null) { lstEventScripts.ItemAdd(ItemOf(cap, cap, true, cap + FirstSortChar)); }

                if (!_didMessage && thisSet.NeedRow && !tb.IsRowScriptPossible()) {
                    _didMessage = true;
                    EnableScript();
                }
            }
        }

        #endregion
    }

    #endregion
}