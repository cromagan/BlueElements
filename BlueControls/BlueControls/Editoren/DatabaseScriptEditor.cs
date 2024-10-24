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
using System.Linq;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.Interfaces;
using BlueScript.EventArgs;
using BlueScript.Structures;
using static BlueBasics.Constants;
using static BlueBasics.IO;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using MessageBox = BlueControls.Forms.MessageBox;

namespace BlueControls.BlueDatabaseDialogs;

public sealed partial class DatabaseScriptEditor : IHasDatabase, IUniqueWindow {

    #region Fields

    public static Befehlsreferenz? _befehlsReferenz;

    private bool _allowTemporay;

    private Database? _database;

    private DatabaseScriptDescription? _item;

    private bool _testmode = true;

    #endregion

    #region Constructors

    public DatabaseScriptEditor() {
        // Dieser Aufruf ist f�r den Windows Form-Designer erforderlich.
        InitializeComponent();
        tbcScriptEigenschaften.Enabled = false;
        eventScriptEditor.Enabled = false;

        FormManager.RegisterForm(this);
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
                        "Der Fehlerspeicher wird jetzt gel�scht. Es kann u.U. l�nger dauern, bis der Fehler erneut auftritt.",
                        "Deswegen w�re es sinnvoll, den Fehler jetzt zu reparieren.",
                        "Datenbank: " + _database.Caption,
                        " ",
                        " ",
                        "Letzte Fehlermeldung, die zum Deaktivieren des Skriptes f�hrte:",
                        " ",
                        _database.ScriptNeedFix
                    ];
                    l.WriteAllText(TempFile(string.Empty, string.Empty, "txt"), Win1252, true);
                }

                _database.ScriptNeedFix = string.Empty;
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

            _item = null; // Um keine werte zur�ck zu Schreiben werden des anzeigen

            if (value != null) {
                tbcScriptEigenschaften.Enabled = true;
                eventScriptEditor.Enabled = true;
                txbName.Text = value.KeyName;
                txbQuickInfo.Text = value.QuickInfo;

                cbxPic.Text = value.Image;

                chkZeile.Checked = value.NeedRow;
                txbTestZeile.Enabled = value.NeedRow;
                chkAusl�ser_newrow.Checked = value.EventTypes.HasFlag(ScriptEventTypes.InitialValues);
                chkAusl�ser_valuechanged.Checked = value.EventTypes.HasFlag(ScriptEventTypes.value_changed);
                chkExtendend.Visible = chkAusl�ser_valuechanged.Checked;
                chkAusl�ser_valuechangedThread.Checked = value.EventTypes.HasFlag(ScriptEventTypes.value_changed_extra_thread);
                chkAusl�ser_prepaireformula.Checked = value.EventTypes.HasFlag(ScriptEventTypes.prepare_formula);
                chkAusl�ser_databaseloaded.Checked = value.EventTypes.HasFlag(ScriptEventTypes.loaded);
                chkAusl�ser_export.Checked = value.EventTypes.HasFlag(ScriptEventTypes.export);
                chkAusl�ser_deletingRow.Checked = value.EventTypes.HasFlag(ScriptEventTypes.row_deleting);
                chkAusl�ser_Fehlerfrei.Checked = value.EventTypes.HasFlag(ScriptEventTypes.correct_changed);
                //chkAendertWerte.Checked = value.ChangeValues;
                eventScriptEditor.Script = value.Script;

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
                eventScriptEditor.Enabled = false;
                txbTestZeile.Enabled = false;

                txbName.Text = string.Empty;
                cbxPic.Text = string.Empty;
                txbQuickInfo.Text = string.Empty;
                eventScriptEditor.Script = string.Empty;
                chkAusl�ser_newrow.Checked = false;
                chkAusl�ser_valuechanged.Checked = false;
                chkAusl�ser_prepaireformula.Checked = false;
                chkAusl�ser_valuechangedThread.Checked = false;
                chkAusl�ser_databaseloaded.Checked = false;
                chkAusl�ser_export.Checked = false;
                btnVerlauf.Enabled = false;
            }

            _item_PropertyChanged(null, System.EventArgs.Empty);
        }
    }

    public object? Object {
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

    public static void btnBefehlsUebersicht_Click(object sender, System.EventArgs e) {
        if (_befehlsReferenz is { Visible: true }) {
            _befehlsReferenz.Close();
            _befehlsReferenz.Dispose();
            _befehlsReferenz = null;
        }

        _befehlsReferenz = new Befehlsreferenz();
        _befehlsReferenz.Show();
    }

    protected override void OnFormClosing(FormClosingEventArgs e) {
        WriteInfosBack();

        if (_befehlsReferenz is { Visible: true }) {
            _befehlsReferenz.Close();
            _befehlsReferenz.Dispose();
            _befehlsReferenz = null;
        }

        base.OnFormClosing(e);

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
            capFehler.Text = "<imagecode=H�kchen|16> Keine Skript-Konflikte.";
            return;
        }

        capFehler.Text = "<imagecode=Warnung|16> " + _item.ErrorReason();
    }

    private void btnAusf�hren_Click(object sender, System.EventArgs e) {
        _testmode = false;
        eventScriptEditor.TesteScript(txbName.Text);
    }

    private void btnDatenbankKopf_Click(object sender, System.EventArgs e) => InputBoxEditor.Show(Database, typeof(DatabaseHeadEditor), false);

    private void btnSave_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return; }

        btnSaveLoad.Enabled = false;

        WriteInfosBack();
        _ = db.Save();

        btnSaveLoad.Enabled = true;
    }

    private void btnSpaltenuebersicht_Click(object sender, System.EventArgs e) => Database?.Column.GenerateOverView();

    private void btnTest_Click(object sender, System.EventArgs e) {
        _testmode = true;
        eventScriptEditor.TesteScript(txbName.Text);
    }

    private void btnVerlauf_Click(object sender, System.EventArgs e) {
        // �berpr�fen, ob die Datenbank oder die Instanz selbst verworfen wurde
        if (IsDisposed || Database is not { IsDisposed: false } db) { return; }
        // �berpr�fen, ob die Tabelle bearbeitet werden kann
        if (TableView.ErrorMessage(Database, EditableErrorReasonType.EditNormaly)) {
            Item = null;
            btnVerlauf.Enabled = false;
            return;
        }
        // Das ausgew�hlte Skript aus der Liste abrufen
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
                // �berpr�fen, ob das Skript ge�ndert wurde
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
                    l.Add("    -> Keine �nderung am gew�hlten Skript");
                }
            }
        }
        // Schreiben der Liste in eine tempor�re Datei
        l.WriteAllText(TempFile(string.Empty, "Scrip.txt"), Win1252, true);
    }

    private void btnVersionErh�hen_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Database is not { IsDisposed: false }) { return; }

        btnVersionErh�hen.Enabled = false;

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

    private void chkAusl�ser_newrow_CheckedChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }

        ScriptEventTypes tmp = 0;
        if (chkAusl�ser_newrow.Checked) { tmp |= ScriptEventTypes.InitialValues; }
        if (chkAusl�ser_valuechanged.Checked) { tmp |= ScriptEventTypes.value_changed; }
        if (chkAusl�ser_prepaireformula.Checked) { tmp |= ScriptEventTypes.prepare_formula; }
        if (chkAusl�ser_valuechangedThread.Checked) { tmp |= ScriptEventTypes.value_changed_extra_thread; }
        if (chkAusl�ser_databaseloaded.Checked) { tmp |= ScriptEventTypes.loaded; }
        if (chkAusl�ser_export.Checked) { tmp |= ScriptEventTypes.export; }
        if (chkAusl�ser_deletingRow.Checked) { tmp |= ScriptEventTypes.row_deleting; }
        if (chkAusl�ser_Fehlerfrei.Checked) { tmp |= ScriptEventTypes.correct_changed; }
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
        e.CancelReason = "Skript-Editor ge�ffnet";
    }

    private bool EnableScript() {
        if (IsDisposed || Database is not { IsDisposed: false }) { return false; }

        var s = MessageBox.Show("F�r Zeilenskripte werden bestimmte Systemspalten ben�tigt.<br>Sollen diese erstellt werden?", ImageCode.Spalte, "Ja", "Nein");

        if (s == 1) { return false; }

        Database.EnableScript();

        if (!Database.IsRowScriptPossible(false)) {
            MessageBox.Show("Systemspalten konnten nicht erstellt werden.", ImageCode.Information, "Ok");
            return false;
        }
        return true;
    }

    private void eventScriptEditor_ExecuteScript(object sender, ScriptEventArgs e) {
        if (IsDisposed || Database is not { IsDisposed: false }) {
            e.Feedback = new ScriptEndedFeedback("Keine Datenbank geladen.", false, false, "Allgemein");
            return;
        }

        if (_item == null) {
            e.Feedback = new ScriptEndedFeedback("Kein Skript gew�hlt.", false, false, "Allgemein");
            return;
        }

        if (!_item.IsOk()) {
            e.Feedback = new ScriptEndedFeedback("Bitte zuerst den Fehler korrigieren: " + _item.ErrorReason(), false, false, "Allgemein");
            return;
        }

        WriteInfosBack();

        RowItem? r = null;

        if (_item.NeedRow) {
            if (Database.Row.Count == 0) {
                e.Feedback = new ScriptEndedFeedback("Zum Test wird zumindest eine Zeile ben�tigt.", false, false, "Allgemein");
                return;
            }
            if (string.IsNullOrEmpty(txbTestZeile.Text)) {
                txbTestZeile.Text = Database?.Row.First()?.CellFirstString() ?? string.Empty;
            }

            r = Database?.Row[txbTestZeile.Text];
            if (r is not { IsDisposed: false }) {
                e.Feedback = new ScriptEndedFeedback("Zeile nicht gefunden.", false, false, "Allgemein");
                return;
            }
        }

        if (!_testmode) {
            if (MessageBox.Show("Skript �ndert Werte!<br>Fortfahren?", ImageCode.Warnung, "Fortfahren", "Abbruch") != 0) { return; }
        }

        var ext = chkExtendend is { Checked: true, Visible: true };

        _allowTemporay = true;
        e.Feedback = Database?.ExecuteScript(_item, !_testmode, r, null, true, ext);
        _allowTemporay = false;
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

    private void ScriptEditor_PropertyChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.Script = eventScriptEditor.Script;
    }

    private void txbName_TextChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.KeyName = txbName.Text;
    }

    //            break;
    //    }
    //}
    private void txbQuickInfo_TextChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.QuickInfo = txbQuickInfo.Text;
    }

    private void UnRegisterEvent() {
        if (_item == null) { return; }

        _item.PropertyChanged -= _item_PropertyChanged;
    }

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

    //    switch (e.Item.ToLowerInvariant()) {
    //        case "spalteneigenschaftenbearbeiten":
    //            if (c != null && !c.IsDisposed) {
    //                TableView.OpenColumnEditor(c, null, null);
    //            }
    private void WriteInfosBack() {
        if (IsDisposed || TableView.ErrorMessage(Database, EditableErrorReasonType.EditNormaly) || Database is not
            {
                IsDisposed: false
            }) { return; }

        if (_item != null) {
            _item.Script = eventScriptEditor.Script;
        }

        #region Items sicherheitshalber in die Datenbank zur�ck schreiben, nur so werden die gel�schten und neuen erfasst

        var t2 = new List<DatabaseScriptDescription>();

        foreach (var thisItem in lstEventScripts.Items) {
            if (thisItem is ReadableListItem { Item: DatabaseScriptDescription dbs }) { t2.Add(dbs); }
        }

        //t2.AddRange(lstEventScripts.Items.Select(thisItem => (DatabaseScriptDescription)((ReadableListItem)thisItem).Item));
        Database.EventScript = new(t2);
        Database.ScriptNeedFix = string.Empty;

        #endregion
    }

    #endregion
}