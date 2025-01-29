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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.Interfaces;
using BlueScript.Structures;
using static BlueBasics.Constants;
using static BlueBasics.IO;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using MessageBox = BlueControls.Forms.MessageBox;

namespace BlueControls.BlueDatabaseDialogs;

public sealed partial class DatabaseScriptEditor : ScriptEditorGeneric, IHasDatabase {

    #region Fields

    private bool _allowTemporay;

    private Database? _database;

    private DatabaseScriptDescription? _item;

    #endregion

    #region Constructors

    public DatabaseScriptEditor() : base() {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
        tbcScriptEigenschaften.Enabled = false;
    }

    #endregion

    #region Properties

    public Database? Database {
        get => _database;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == _database) { return; }

            if (_database != null) {
                _database.DisposingEvent -= _database_Disposing;
                _database.CanDoScript -= Database_CanDoScript;
            }
            _database = value;

            if (_database != null) {
                _database.DisposingEvent += _database_Disposing;
                _database.CanDoScript += Database_CanDoScript;

                if (!string.IsNullOrEmpty(_database.ScriptNeedFix)) {
                    List<string> l =
                    [
                        "### ACHTUNG - EINMALIGE ANZEIGE ###",
                        "Der Fehlerspeicher wird jetzt gelöscht. Es kann u.U. länger dauern, bis der Fehler erneut auftritt.",
                        "Deswegen wäre es sinnvoll, den Fehler jetzt zu reparieren.",
                        "Datenbank: " + _database.Caption,
                        " ",
                        " ",
                        "Letzte Fehlermeldung, die zum Deaktivieren des Skriptes führte:",
                        " ",
                        _database.ScriptNeedFix
                    ];
                    l.WriteAllText(TempFile(string.Empty, string.Empty, "txt"), Win1252, true);
                }

                _database.ScriptNeedFix = string.Empty;
                tbcScriptEigenschaften.Enabled = true;
            } else {
                tbcScriptEigenschaften.Enabled = false;
            }
        }
    }

    public DatabaseScriptDescription? Item {
        get {
            if (IsDisposed || Database is not { IsDisposed: false }) { return null; }

            return _item;
        }
        set {
            if (_item == value) { return; }
            if (IsDisposed || Database is not { IsDisposed: false }) { return; }

            WriteInfosBack();
            UnRegisterEvent();

            _item = null; // Um keine werte zurück zu Schreiben werden des anzeigen

            if (value != null) {
                tbcScriptEigenschaften.Enabled = true;
                tbcScriptEigenschaften.Enabled = true;
                txbName.Text = value.KeyName;
                txbQuickInfo.Text = value.QuickInfo;

                cbxPic.Text = value.Image;

                chkZeile.Checked = value.NeedRow;
                txbTestZeile.Enabled = value.NeedRow;
                chkAuslöser_newrow.Checked = value.EventTypes.HasFlag(ScriptEventTypes.InitialValues);
                chkAuslöser_valuechanged.Checked = value.EventTypes.HasFlag(ScriptEventTypes.value_changed);
                chkExtendend.Visible = chkAuslöser_valuechanged.Checked;
                chkAuslöser_valuechangedThread.Checked = value.EventTypes.HasFlag(ScriptEventTypes.value_changed_extra_thread);
                chkAuslöser_prepaireformula.Checked = value.EventTypes.HasFlag(ScriptEventTypes.prepare_formula);
                chkAuslöser_databaseloaded.Checked = value.EventTypes.HasFlag(ScriptEventTypes.loaded);
                chkAuslöser_export.Checked = value.EventTypes.HasFlag(ScriptEventTypes.export);
                chkAuslöser_deletingRow.Checked = value.EventTypes.HasFlag(ScriptEventTypes.row_deleting);
                chkAuslöser_Fehlerfrei.Checked = value.EventTypes.HasFlag(ScriptEventTypes.correct_changed);
                //chkAendertWerte.Checked = value.ChangeValues;
                Script = value.Script;

                lstPermissionExecute.ItemClear();
                var l = Table.Permission_AllUsed(false).ToList();
                l.AddIfNotExists(Administrator);
                lstPermissionExecute.ItemAddRange(l);
                lstPermissionExecute.Check(value.UserGroups);
                lstPermissionExecute.Suggestions.Clear();

                _item = value;
                RegisterEvent();

                btnVerlauf.Enabled = _item != null;
            } else {
                tbcScriptEigenschaften.Enabled = false;
                tbcScriptEigenschaften.Enabled = false;
                txbTestZeile.Enabled = false;

                txbName.Text = string.Empty;
                cbxPic.Text = string.Empty;
                txbQuickInfo.Text = string.Empty;
                Script = string.Empty;
                chkAuslöser_newrow.Checked = false;
                chkAuslöser_valuechanged.Checked = false;
                chkAuslöser_prepaireformula.Checked = false;
                chkAuslöser_valuechangedThread.Checked = false;
                chkAuslöser_databaseloaded.Checked = false;
                chkAuslöser_export.Checked = false;
                btnVerlauf.Enabled = false;
            }

            _item_PropertyChanged(null, System.EventArgs.Empty);
        }
    }

    public override object? Object {
        get => Database;
        set {
            if (value is Database db) {
                Database = db;
            } else {
                Database = null;
            }
        }
    }

    /// <summary>
    /// Nur zum setzen der Zeile zum Testen.
    /// </summary>
    public RowItem? Row {
        set {
            txbTestZeile.Text = value?.CellFirstString() ?? string.Empty;
            Database = value?.Database;
        }
    }

    #endregion

    #region Methods

    public override ScriptEndedFeedback ExecuteScript(bool testmode) {
        if (IsDisposed || Database is not { IsDisposed: false } db) {
            return new ScriptEndedFeedback("Keine Datenbank geladen.", false, false, "Allgemein");
        }

        if (_item == null) {
            return new ScriptEndedFeedback("Kein Skript gewählt.", false, false, "Allgemein");
        }

        if (!_item.IsOk()) {
            return new ScriptEndedFeedback("Bitte zuerst den Fehler korrigieren: " + _item.ErrorReason(), false, false,
                "Allgemein");
        }

        WriteInfosBack();

        RowItem? r = null;

        if (_item.NeedRow) {
            if (db.Row.Count == 0) {
                return new ScriptEndedFeedback("Zum Test wird zumindest eine Zeile benötigt.", false, false,
                    "Allgemein");
            }

            if (string.IsNullOrEmpty(txbTestZeile.Text)) {
                txbTestZeile.Text = db.Row.First()?.CellFirstString() ?? string.Empty;
            }

            r = db.Row[txbTestZeile.Text];
            if (r is not { IsDisposed: false }) {
                return new ScriptEndedFeedback("Zeile nicht gefunden.", false, false, "Allgemein");
            }
        }

        if (!testmode) {
            if (MessageBox.Show("Skript ändert Werte!<br>Fortfahren?", ImageCode.Warnung, "Fortfahren", "Abbruch") != 0) {
                return new ScriptEndedFeedback("Abbruch.", false, false, "Allgemein");
            }
        }

        var ext = chkExtendend is { Checked: true, Visible: true };

        _allowTemporay = true;
        var f = db.ExecuteScript(_item, !testmode, r, null, true, ext);
        _allowTemporay = false;

        return f;
    }

    //    switch (e.Item.ToLowerInvariant()) {
    //        case "spalteneigenschaftenbearbeiten":
    //            if (c != null && !c.IsDisposed) {
    //                TableView.OpenColumnEditor(c, null, null);
    //            }
    public override void WriteInfosBack() {
        if (IsDisposed || TableView.ErrorMessage(Database, EditableErrorReasonType.EditNormaly) || Database is not
            {
                IsDisposed: false
            }) { return; }

        if (_item != null) {
            _item.Script = Script;
        }

        #region Items sicherheitshalber in die Datenbank zurück schreiben, nur so werden die gelöschten und neuen erfasst

        var t2 = new List<DatabaseScriptDescription>();

        foreach (var thisItem in lstEventScripts.Items) {
            if (thisItem is ReadableListItem { Item: DatabaseScriptDescription dbs }) { t2.Add(dbs); }
        }

        //t2.AddRange(lstEventScripts.Items.Select(thisItem => (DatabaseScriptDescription)((ReadableListItem)thisItem).Item));
        Database.EventScript = new(t2);
        Database.ScriptNeedFix = Database.CheckScriptError();

        #endregion
    }

    protected override void OnFormClosing(FormClosingEventArgs e) {
        WriteInfosBack();

        Item = null; // erst das Item!
        Database = null;
    }

    protected override void OnLoad(System.EventArgs e) {
        base.OnLoad(e);

        var didMessage = false;

        var im = QuickImage.Images();
        foreach (var thisIm in im) {
            cbxPic.ItemAdd(ItemOf(thisIm, thisIm, QuickImage.Get(thisIm, 16)));
        }

        lstEventScripts.ItemClear();
        if (IsDisposed || Database is not { IsDisposed: false }) { return; }

        foreach (var thisSet in Database.EventScript) {
            if (thisSet != null) {
                var cap = "Sonstige";

                if (thisSet.EventTypes != 0) { cap = thisSet.EventTypes.ToString(); }

                var it = ItemOf(thisSet);
                it.UserDefCompareKey = cap + SecondSortChar;

                lstEventScripts.ItemAdd(it);

                if (lstEventScripts[cap] == null) {
                    lstEventScripts.ItemAdd(ItemOf(cap, cap, true, cap + FirstSortChar));
                }

                if (!didMessage && thisSet.NeedRow && !Database.IsRowScriptPossible(false)) {
                    didMessage = true;
                    EnableScript();
                }
            }
        }
    }

    private void _database_Disposing(object sender, System.EventArgs e) {
        Database = null;
        Close();
    }

    private void _item_PropertyChanged(object? sender, System.EventArgs e) {
        if (_item == null) {
            capFehler.Text = string.Empty;
            return;
        }

        if (_item.IsOk()) {
            capFehler.Text = "<imagecode=Häkchen|16> Keine Skript-Konflikte.";
            return;
        }

        capFehler.Text = "<imagecode=Warnung|16> " + _item.ErrorReason();
    }

    private void btnDatenbankKopf_Click(object sender, System.EventArgs e) => InputBoxEditor.Show(Database, typeof(DatabaseHeadEditor), false);

    private void btnSpaltenuebersicht_Click(object sender, System.EventArgs e) => Database?.Column.GenerateOverView();

    private void btnTest_Click(object sender, System.EventArgs e) {
        TesteScript(true);
    }

    private void btnVerlauf_Click(object sender, System.EventArgs e) {
        // Überprüfen, ob die Datenbank oder die Instanz selbst verworfen wurde
        if (IsDisposed || Database is not { IsDisposed: false } db) { return; }
        // Überprüfen, ob die Tabelle bearbeitet werden kann
        if (TableView.ErrorMessage(Database, EditableErrorReasonType.EditNormaly)) {
            Item = null;
            btnVerlauf.Enabled = false;
            return;
        }
        // Das ausgewählte Skript aus der Liste abrufen
        var selectedlstEventScripts = lstEventScripts[lstEventScripts.Checked[0]] is ReadableListItem item ? (DatabaseScriptDescription)item.Item : null;
        var l = new List<string>();
        // Durchlaufen aller Undo-Operationen in der Datenbank
        foreach (var thisUndo in db.Undo) {
            if (thisUndo.Command == DatabaseDataType.EventScript) {
                l.Add("############################################################################");
                l.Add("############################################################################");
                l.Add("############################################################################");
                l.Add("############################################################################");
                l.Add("############################################################################");
                l.Add(thisUndo.DateTimeUtc.ToString("dd.MM.yyyy HH:mm:ss.fff") + " " + thisUndo.User);

                l.Add(thisUndo.DateTimeUtc.ToString("dd.MM.yyyy HH:mm:ss.fff") + " " + thisUndo.User);
                // Überprüfen, ob das Skript geändert wurde
                List<string> ai = thisUndo.ChangedTo.SplitAndCutByCr().ToList();
                var found = false;
                foreach (var t in ai) {
                    var s = new DatabaseScriptDescription(db, t);
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
        }
        // Schreiben der Liste in eine temporäre Datei
        l.WriteAllText(TempFile(string.Empty, "Scrip.txt"), Win1252, true);
    }

    private void btnVersionErhöhen_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Database is not { IsDisposed: false }) { return; }

        btnVersionErhöhen.Enabled = false;

        Database.EventScriptVersion = DateTime.UtcNow;
    }

    private void btnZusatzDateien_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return; }
        ExecuteFile(db.AdditionalFilesPfadWhole());
    }

    private void cbxPic_TextChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.Image = cbxPic.Text.TrimEnd("|16");
    }

    //private void chkAendertWerte_CheckedChanged(object sender, System.EventArgs e) {
    //    if (Item == null) { return; }

    //    Item.ChangeValues = chkAendertWerte.Checked;
    //}

    private void chkAuslöser_newrow_CheckedChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }

        ScriptEventTypes tmp = 0;
        if (chkAuslöser_newrow.Checked) { tmp |= ScriptEventTypes.InitialValues; }
        if (chkAuslöser_valuechanged.Checked) { tmp |= ScriptEventTypes.value_changed; }
        if (chkAuslöser_prepaireformula.Checked) { tmp |= ScriptEventTypes.prepare_formula; }
        if (chkAuslöser_valuechangedThread.Checked) { tmp |= ScriptEventTypes.value_changed_extra_thread; }
        if (chkAuslöser_databaseloaded.Checked) { tmp |= ScriptEventTypes.loaded; }
        if (chkAuslöser_export.Checked) { tmp |= ScriptEventTypes.export; }
        if (chkAuslöser_deletingRow.Checked) { tmp |= ScriptEventTypes.row_deleting; }
        if (chkAuslöser_Fehlerfrei.Checked) { tmp |= ScriptEventTypes.correct_changed; }
        Item.EventTypes = tmp;
    }

    private void chkZeile_CheckedChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }

        if (IsDisposed || Database is not { IsDisposed: false }) { return; }

        if (chkZeile.Checked && !Database.IsRowScriptPossible(false)) {
            if (!EnableScript()) { chkZeile.Checked = false; }

            return;
        }

        Item.NeedRow = chkZeile.Checked;
        txbTestZeile.Enabled = chkZeile.Checked;
    }

    private void Database_CanDoScript(object sender, CancelReasonEventArgs e) {
        if (_allowTemporay) { return; }
        e.CancelReason = "Skript-Editor geöffnet";
    }

    private bool EnableScript() {
        if (IsDisposed || Database is not { IsDisposed: false }) { return false; }

        var s = MessageBox.Show("Für Zeilenskripte werden bestimmte Systemspalten benötigt.<br>Sollen diese erstellt werden?", ImageCode.Spalte, "Ja", "Nein");

        if (s == 1) { return false; }

        Database.EnableScript();

        if (!Database.IsRowScriptPossible(false)) {
            MessageBox.Show("Systemspalten konnten nicht erstellt werden.", ImageCode.Information, "Ok");
            return false;
        }
        return true;
    }

    private void GlobalTab_SelectedIndexChanged(object sender, System.EventArgs e) => WriteInfosBack();

    private void lstEventScripts_AddClicked(object sender, System.EventArgs e) {
        if (IsDisposed || Database is not { IsDisposed: false }) { return; }

        var newScriptItem = ItemOf(new DatabaseScriptDescription(Database));
        lstEventScripts.ItemAdd(newScriptItem);
        WriteInfosBack();

        lstEventScripts.Check(newScriptItem);
    }

    private void lstEventScripts_ItemCheckedChanged(object sender, System.EventArgs e) {
        DatabaseScriptDescription? newItem = null;
        if (lstEventScripts.Checked.Count == 1 &&
            !TableView.ErrorMessage(Database, EditableErrorReasonType.EditNormaly)) {
            if (lstEventScripts[lstEventScripts.Checked[0]] is ReadableListItem selectedlstEventScripts) {
                newItem = selectedlstEventScripts.Item as DatabaseScriptDescription;
            }
        }

        Item = newItem;
    }

    private void lstPermissionExecute_ItemClicked(object sender, AbstractListItemEventArgs e) {
        var tmp = lstPermissionExecute.Checked.ToList();
        //_ = tmp.Remove(Constants.Administrator);
        if (Item != null) { Item.UserGroups = [.. tmp]; }
    }

    private void RegisterEvent() {
        if (_item == null) { return; }
        _item.PropertyChanged += _item_PropertyChanged;
    }

    private void txbName_TextChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.KeyName = txbName.Text;
    }

    private void txbQuickInfo_TextChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.QuickInfo = txbQuickInfo.Text;
    }

    private void UnRegisterEvent() {
        if (_item == null) { return; }

        _item.PropertyChanged -= _item_PropertyChanged;
    }

    #endregion

    //private void scriptEditor_ContextMenuInit(object sender, ContextMenuInitEventArgs e) {
    //    //Todo: Implementieren
    //    if (e.HotItem is string txt) {
    //        var c = Database?.Column[txt);
    //        if (c is null) { return; }
    //       e.CurrentMenu.Add(Add(ContextMenuCommands.SpaltenEigenschaftenBearbeiten);
    //    }
    //}

    //private void scriptEditor_ContextMenuItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
    //    //Todo: Implementieren
    //    ColumnItem? c = null;

    //    if (e.HotItem is string txt) { c = Database?.Column[txt); }
}