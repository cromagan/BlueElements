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

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueBasics.Interfaces;
using BlueControls.Forms;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using BlueDatabase;
using BlueDatabase.Interfaces;
using BlueScript.EventArgs;
using BlueScript.Structures;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static BlueBasics.Constants;
using static BlueBasics.Converter;
using static BlueBasics.IO;
using MessageBox = BlueControls.Forms.MessageBox;
using BlueControls.ItemCollectionList;

namespace BlueControls.BlueDatabaseDialogs;

public sealed partial class DatabaseScriptEditor : IHasDatabase {

    #region Fields

    private bool _allowTemporay;
    private Befehlsreferenz? _befehlsReferenz;
    private Database? _database;
    private DatabaseScriptDescription? _item;

    private bool testmode = true;

    #endregion

    #region Constructors

    public DatabaseScriptEditor(Database database) {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
        tbcScriptEigenschaften.Enabled = false;
        eventScriptEditor.Enabled = false;
        Database = database;

        if (!string.IsNullOrEmpty(Database.EventScriptErrorMessage)) {
            var l = new List<string> {
                "### ACHTUNG - EINMALIGE ANZEIGE ###",
                "Der Fehlerspeicher wird jetzt gelöscht. Es kann u.U. länger dauern, bis der Fehler erneut auftritt.",
                "Deswegen wäre es sinnvoll, den Fehler jetzt zu reparieren.",
                "Datenbank: " + Database.Caption,
                " ",
                " ",
                "Letzte Fehlermeldung, die zum Deaktivieren des Skriptes führte:",
                " ",
                Database.EventScriptErrorMessage
            };
            l.WriteAllText(TempFile("", "", "txt"), Win1252, true);
        }

        Database.EventScriptErrorMessage = string.Empty;

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
            }
        }
    }

    public DatabaseScriptDescription? Item {
        get {
            if (IsDisposed || Database is not Database db || db.IsDisposed) { return null; }

            return _item;
        }
        set {
            if (_item == value) { return; }
            if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }

            WriteInfosBack();

            _item = null; // Um keine werte zurück zu Schreiben werden des anzeigen

            if (value != null) {
                tbcScriptEigenschaften.Enabled = true;
                eventScriptEditor.Enabled = true;
                txbName.Text = value.KeyName;
                txbQuickInfo.Text = value.QuickInfo;

                cbxPic.Text = value.Image;

                chkZeile.Checked = value.NeedRow;
                txbTestZeile.Enabled = value.NeedRow;
                chkAuslöser_newrow.Checked = value.EventTypes.HasFlag(ScriptEventTypes.new_row);
                chkAuslöser_valuechanged.Checked = value.EventTypes.HasFlag(ScriptEventTypes.value_changed);
                chkAuslöser_keyvaluechanged.Checked = value.EventTypes.HasFlag(ScriptEventTypes.keyvalue_changed);
                chkAuslöser_valuechangedThread.Checked = value.EventTypes.HasFlag(ScriptEventTypes.value_changed_extra_thread);
                chkAuslöser_prepaireformula.Checked = value.EventTypes.HasFlag(ScriptEventTypes.prepare_formula);
                chkAuslöser_databaseloaded.Checked = value.EventTypes.HasFlag(ScriptEventTypes.loaded);
                chkAuslöser_export.Checked = value.EventTypes.HasFlag(ScriptEventTypes.export);
                chkAendertWerte.Checked = value.ChangeValues;
                eventScriptEditor.ScriptText = value.ScriptText;

                lstPermissionExecute.ItemClear();
                var l = Database.Permission_AllUsed(false).ToList();
                l.AddIfNotExists(Administrator);
                lstPermissionExecute.ItemAddRange(l);
                lstPermissionExecute.Check(value.UserGroups);
                lstPermissionExecute.Suggestions.Clear();

                _item = value;
            } else {
                tbcScriptEigenschaften.Enabled = false;
                eventScriptEditor.Enabled = false;
                txbTestZeile.Enabled = false;

                txbName.Text = string.Empty;
                cbxPic.Text = string.Empty;
                txbQuickInfo.Text = string.Empty;
                eventScriptEditor.ScriptText = string.Empty;
                chkAuslöser_newrow.Checked = false;
                chkAuslöser_valuechanged.Checked = false;
                chkAuslöser_prepaireformula.Checked = false;
                chkAendertWerte.Checked = false;
                chkAuslöser_valuechangedThread.Checked = false;
                chkAuslöser_databaseloaded.Checked = false;
                chkAuslöser_export.Checked = false;
            }
        }
    }

    #endregion

    #region Methods

    protected override void OnFormClosing(FormClosingEventArgs e) {
        WriteInfosBack();

        if (_befehlsReferenz != null && _befehlsReferenz.Visible) {
            _befehlsReferenz.Close();
            _befehlsReferenz?.Dispose();
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
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }

        foreach (var thisSet in Database.EventScript) {
            if (thisSet != null) {
                lstEventScripts.ItemAdd(ItemOf(thisSet));

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

    private void btnAusführen_Click(object sender, System.EventArgs e) {
        testmode = false;
        eventScriptEditor.TesteScript(txbName.Text);
    }

    private void btnBefehlsUebersicht_Click(object sender, System.EventArgs e) {
        if (_befehlsReferenz != null && _befehlsReferenz.Visible) {
            _befehlsReferenz.Close();
            _befehlsReferenz?.Dispose();
            _befehlsReferenz = null;
        }

        _befehlsReferenz = new Befehlsreferenz();
        _befehlsReferenz.Show();
    }

    private void btnDatenbankKopf_Click(object sender, System.EventArgs e) => TableView.OpenDatabaseHeadEditor(Database);

    private void btnSave_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }

        btnSaveLoad.Enabled = false;

        WriteInfosBack();
        _ = db.Save();

        btnSaveLoad.Enabled = true;
    }

    private void btnSpaltenuebersicht_Click(object sender, System.EventArgs e) => Database?.Column.GenerateOverView();

    private void btnTest_Click(object sender, System.EventArgs e) {
        testmode = true;
        eventScriptEditor.TesteScript(txbName.Text);
    }

    private void btnVerlauf_Click(object sender, System.EventArgs e) {
        // Überprüfen, ob die Datenbank oder die Instanz selbst verworfen wurde
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }
        // Überprüfen, ob die Tabelle bearbeitet werden kann
        if (TableView.ErrorMessage(Database, EditableErrorReasonType.EditNormaly)) {
            Item = null;
            btnVerlauf.Enabled = false;
            return;
        }
        // Das ausgewählte Skript aus der Liste abrufen
        var selectedlstEventScripts = (DatabaseScriptDescription)((ReadableListItem)lstEventScripts[lstEventScripts.Checked[0]]).Item;
        var l = new List<string>();
        // Durchlaufen aller Undo-Operationen in der Datenbank
        foreach (var thisUndo in db.Undo) {
            if (thisUndo.Command == BlueDatabase.Enums.DatabaseDataType.EventScript) {
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
                    if (s.KeyName == selectedlstEventScripts.KeyName && selectedlstEventScripts.ScriptText != s.ScriptText) {
                        l.Add(s.ScriptText);
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
        l.WriteAllText(TempFile("", "Scrip.txt"), Win1252, true);
    }

    private void btnVersionErhöhen_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }

        btnVersionErhöhen.Enabled = false;

        _ = IntTryParse(Database.EventScriptVersion, out var tmp);
        tmp++;
        if (tmp == int.MaxValue) { tmp = 0; }

        Database.EventScriptVersion = tmp.ToString();
    }

    private void btnZusatzDateien_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }
        ExecuteFile(db.AdditionalFilesPfadWhole());
    }

    private void cbxPic_TextChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.Image = cbxPic.Text.TrimEnd("|16");
    }

    private void chkAendertWerte_CheckedChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }

        Item.ChangeValues = chkAendertWerte.Checked;
    }

    private void chkAuslöser_newrow_CheckedChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }

        ScriptEventTypes tmp = 0;
        if (chkAuslöser_newrow.Checked) { tmp |= ScriptEventTypes.new_row; }
        if (chkAuslöser_valuechanged.Checked) { tmp |= ScriptEventTypes.value_changed; }
        if (chkAuslöser_keyvaluechanged.Checked) { tmp |= ScriptEventTypes.keyvalue_changed; }
        if (chkAuslöser_prepaireformula.Checked) { tmp |= ScriptEventTypes.prepare_formula; }
        if (chkAuslöser_valuechangedThread.Checked) { tmp |= ScriptEventTypes.value_changed_extra_thread; }
        if (chkAuslöser_databaseloaded.Checked) { tmp |= ScriptEventTypes.loaded; }
        if (chkAuslöser_export.Checked) { tmp |= ScriptEventTypes.export; }
        Item.EventTypes = tmp;
    }

    private void chkZeile_CheckedChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }

        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }

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
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return false; }

        var s = MessageBox.Show("Für Zeilenskripte werden bestimmte Systemspalten benötigt.<br>Sollen diese erstellt werden?", ImageCode.Spalte, "Ja", "Nein");

        if (s == 1) { return false; }

        Database.EnableScript();

        if (!Database.IsRowScriptPossible(false)) {
            MessageBox.Show("Systemspalten konnten nicht erstellt werden.", ImageCode.Information, "Ok");
            return false;
        }
        return true;
    }

    private void eventScriptEditor_ExecuteScript(object sender, ScriptEventArgs e) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) {
            e.Feedback = new ScriptEndedFeedback("Keine Datenbank geladen.", false, false, "Allgemein");
            return;
        }

        if (_item == null) {
            e.Feedback = new ScriptEndedFeedback("Kein Skript gewählt.", false, false, "Allgemein");
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
                e.Feedback = new ScriptEndedFeedback("Zum Test wird zumindest eine Zeile benötigt.", false, false, "Allgemein");
                return;
            }
            if (string.IsNullOrEmpty(txbTestZeile.Text)) {
                txbTestZeile.Text = Database?.Row.First()?.CellFirstString() ?? string.Empty;
            }

            r = Database?.Row[txbTestZeile.Text];
            if (r == null || r.IsDisposed) {
                e.Feedback = new ScriptEndedFeedback("Zeile nicht gefunden.", false, false, "Allgemein");
                return;
            }
        }

        if (!testmode) {
            if (MessageBox.Show("Skript ändert Werte!<br>Fortfahren?", ImageCode.Warnung, "Fortfahren", "Abbruch") != 0) { return; }
        }

        _allowTemporay = true;
        e.Feedback = Database?.ExecuteScript(_item, !testmode, r, null);
        _allowTemporay = false;
    }

    private void GlobalTab_SelectedIndexChanged(object sender, System.EventArgs e) => WriteInfosBack();

    private void lstEventScripts_AddClicked(object sender, System.EventArgs e) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }

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
        btnVerlauf.Enabled = Item != null;
    }

    private void lstPermissionExecute_ItemClicked(object sender, EventArgs.AbstractListItemEventArgs e) {
        var tmp = lstPermissionExecute.Checked.ToList();
        //_ = tmp.Remove(Constants.Administrator);
        Item.UserGroups = new(tmp);
    }

    private void ScriptEditor_PropertyChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.ScriptText = eventScriptEditor.ScriptText;
    }

    //private void scriptEditor_ContextMenuInit(object sender, ContextMenuInitEventArgs e) {
    //    //Todo: Implementieren
    //    if (e.HotItem is string txt) {
    //        var c = Database?.Column[txt);
    //        if (c is null) { return; }
    //       e.UserMenu.Add(Add(ContextMenuCommands.SpaltenEigenschaftenBearbeiten);
    //    }
    //}

    //private void scriptEditor_ContextMenuItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
    //    //Todo: Implementieren
    //    ColumnItem? c = null;

    //    if (e.HotItem is string txt) { c = Database?.Column[txt); }

    //    switch (e.ClickedCommand.ToLower()) {
    //        case "spalteneigenschaftenbearbeiten":
    //            if (c != null && !c.IsDisposed) {
    //                TableView.OpenColumnEditor(c, null, null);
    //            }

    //            break;
    //    }
    //}

    private void txbName_TextChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.KeyName = txbName.Text;
    }

    private void txbQuickInfo_TextChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.QuickInfo = txbQuickInfo.Text;
    }

    private void WriteInfosBack() {
        if (IsDisposed || TableView.ErrorMessage(Database, EditableErrorReasonType.EditNormaly) || Database == null || Database.IsDisposed) { return; }

        if (_item != null) {
            _item.ScriptText = eventScriptEditor.ScriptText;
        }

        #region Items sicherheitshalber in die Datenbank zurück schreiben, nur so werden die gelöschten und neuen erfasst

        var t2 = new List<DatabaseScriptDescription>();
        t2.AddRange(lstEventScripts.Items.Select(thisItem => (DatabaseScriptDescription)((ReadableListItem)thisItem).Item));
        Database.EventScript = new(t2);
        Database.EventScriptErrorMessage = string.Empty;

        #endregion
    }

    #endregion
}