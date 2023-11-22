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

using System.Collections.Generic;
using System.Linq;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueBasics.Interfaces;
using BlueControls.Forms;
using BlueControls.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Interfaces;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueBasics.IO;
using static BlueBasics.Converter;

namespace BlueControls.BlueDatabaseDialogs;

public sealed partial class DatabaseScriptEditor : IHasDatabase {

    #region Fields

    private DatabaseScriptDescription? _item;

    private bool allowTemporay;

    #endregion

    #region Constructors

    public DatabaseScriptEditor(DatabaseAbstract database) {
        // Dieser Aufruf ist f�r den Windows Form-Designer erforderlich.
        InitializeComponent();
        grpEigenschaften.Enabled = false;
        eventScriptEditor.Enabled = false;
        Database = database;

        if (!string.IsNullOrEmpty(Database.EventScriptErrorMessage)) {
            var l = new List<string>();
            l.Add("### ACHTUNG - EINMALIGE ANZEIGE ###");
            l.Add("Der Fehlerspeicher wird jetzt gel�scht. Es kann u.U. l�nger dauern, bis der Fehler erneut auftritt.");
            l.Add("Deswegen w�re es sinnvoll, den Fehler jetzt zu reparieren.");
            l.Add(" ");
            l.Add(" ");
            l.Add("Letzte Fehlermeldung, die zum Deaktivieren des Skriptes f�hrte:");
            l.Add(" ");
            l.Add(Database.EventScriptErrorMessage);
            l.WriteAllText(TempFile("", "", "txt"), Constants.Win1252, true);
        }

        Database.EventScriptErrorMessage = string.Empty;
        Database.DisposingEvent += Database_DisposingEvent;
        Database.CanDoScript += Database_CanDoScript;

        FormManager.RegisterForm(this);
    }

    #endregion

    #region Properties

    public DatabaseAbstract? Database { get; private set; }

    public DatabaseScriptDescription? Item {
        get {
            if (Database is not DatabaseAbstract db || db.IsDisposed) { return null; }

            return _item;
        }
        set {
            if (_item == value) { return; }

            WriteInfosBack();

            _item = null; // Um keine werte zur�ck zu Schreiben werden des anzeigen

            if (value != null) {
                grpEigenschaften.Enabled = true;
                eventScriptEditor.Enabled = true;
                txbName.Text = value.KeyName;

                chkZeile.Checked = value.NeedRow;
                txbTestZeile.Enabled = value.NeedRow;
                chkAusl�ser_newrow.Checked = value.EventTypes.HasFlag(ScriptEventTypes.new_row);
                chkAusl�ser_valuechanged.Checked = value.EventTypes.HasFlag(ScriptEventTypes.value_changed);
                chkAusl�ser_valuechangedThread.Checked = value.EventTypes.HasFlag(ScriptEventTypes.value_changed_extra_thread);
                chkAusl�ser_prepaireformula.Checked = value.EventTypes.HasFlag(ScriptEventTypes.prepare_formula);
                chkAusl�ser_databaseloaded.Checked = value.EventTypes.HasFlag(ScriptEventTypes.loaded);
                chkAusl�ser_export.Checked = value.EventTypes.HasFlag(ScriptEventTypes.export);
                chkExternVerf�gbar.Checked = value.ManualExecutable;
                chkAendertWerte.Checked = value.ChangeValues;
                eventScriptEditor.ScriptText = value.ScriptText;

                _item = value;
            } else {
                grpEigenschaften.Enabled = false;
                eventScriptEditor.Enabled = false;
                txbTestZeile.Enabled = false;

                txbName.Text = string.Empty;
                eventScriptEditor.ScriptText = string.Empty;
                chkAusl�ser_newrow.Checked = false;
                chkAusl�ser_valuechanged.Checked = false;
                chkAusl�ser_prepaireformula.Checked = false;
                chkExternVerf�gbar.Checked = false;
                chkAendertWerte.Checked = false;
                chkAusl�ser_valuechangedThread.Checked = false;
                chkAusl�ser_databaseloaded.Checked = false;
                chkAusl�ser_export.Checked = false;
            }
        }
    }

    #endregion

    #region Methods

    public void OpenAdditionalFileFolder() {
        //Todo: Implementieren
        if (Database is not DatabaseAbstract db || db.IsDisposed) {
            return;
        }

        if (DirectoryExists(Database.AdditionalFilesPfadWhole())) {
            _ = ExecuteFile(Database.AdditionalFilesPfadWhole());
        }
    }

    protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e) {
        base.OnFormClosing(e);
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }

        WriteInfosBack();
        RemoveDatabase();
    }

    protected override void OnLoad(System.EventArgs e) {
        base.OnLoad(e);

        var didMessage = false;

        lstEventScripts.Item.Clear();
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }

        foreach (var thisSet in Database.EventScript) {
            if (thisSet != null) {
                _ = lstEventScripts.Item.Add(thisSet);

                if (!didMessage && thisSet.NeedRow && !Database.IsRowScriptPossible(false)) {
                    didMessage = true;
                    EnableScript();
                }
            }
        }
    }

    protected override void OnShown(System.EventArgs e) => variableEditor.WriteVariablesToTable(Database?.Variables);

    private void btnSave_Click(object sender, System.EventArgs e) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }

        btnSave.Enabled = false;

        WriteInfosBack();
        _ = db.Save();

        btnSave.Enabled = true;
    }

    private void btnSpaltenuebersicht_Click(object sender, System.EventArgs e) => Database?.Column.GenerateOverView();

    private void btnVersionErh�hen_Click(object sender, System.EventArgs e) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }

        btnVersionErh�hen.Enabled = false;

        _ = IntTryParse(Database.EventScriptVersion, out var tmp);
        tmp++;
        if (tmp == int.MaxValue) { tmp = 0; }

        Database.EventScriptVersion = tmp.ToString();
    }

    private void chkAendertWerte_CheckedChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }

        Item.ChangeValues = chkAendertWerte.Checked;
    }

    private void chkAusl�ser_newrow_CheckedChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }

        ScriptEventTypes tmp = 0;
        if (chkAusl�ser_newrow.Checked) { tmp |= ScriptEventTypes.new_row; }
        if (chkAusl�ser_valuechanged.Checked) { tmp |= ScriptEventTypes.value_changed; }
        if (chkAusl�ser_prepaireformula.Checked) { tmp |= ScriptEventTypes.prepare_formula; }
        if (chkAusl�ser_valuechangedThread.Checked) { tmp |= ScriptEventTypes.value_changed_extra_thread; }
        if (chkAusl�ser_databaseloaded.Checked) { tmp |= ScriptEventTypes.loaded; }
        if (chkAusl�ser_export.Checked) { tmp |= ScriptEventTypes.export; }
        Item.EventTypes = tmp;
    }

    private void chkExternVerf�gbar_CheckedChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }

        Item.ManualExecutable = chkExternVerf�gbar.Checked;
    }

    private void chkZeile_CheckedChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }

        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }

        if (chkZeile.Checked && !Database.IsRowScriptPossible(false)) {
            if (!EnableScript()) { chkZeile.Checked = false; }

            return;
        }

        Item.NeedRow = chkZeile.Checked;
        txbTestZeile.Enabled = chkZeile.Checked;
    }

    private void Database_CanDoScript(object sender, CancelReasonEventArgs e) {
        if (allowTemporay) { return; }
        e.CancelReason = "Skript-Editor ge�ffnet";
    }

    private void Database_DisposingEvent(object sender, System.EventArgs e) {
        RemoveDatabase();
        Close();
    }

    private bool EnableScript() {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return false; }

        var s = MessageBox.Show("F�r Zeilenskripte werden bestimmte Systemspalten ben�tigt.<br>Sollen diese erstellt werden?", ImageCode.Spalte, "Ja", "Nein");

        if (s == 1) { return false; }

        Database.EnableScript();

        if (!Database.IsRowScriptPossible(false)) {
            MessageBox.Show("Systemspalten konnten nicht erstellt werden.", ImageCode.Information, "Ok");
            return false;
        }
        return true;
    }

    private void eventScriptEditor_ExecuteScript(object sender, BlueScript.EventArgs.ScriptEventArgs e) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) {
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
            if (r == null || r.IsDisposed) {
                e.Feedback = new ScriptEndedFeedback("Zeile nicht gefunden.", false, false, "Allgemein");
                return;
            }
        }

        if (chkChangeValuesInTest.Checked) {
            if (MessageBox.Show("Skript �ndert Werte!<br>Fortfahren?", ImageCode.Warnung, "Fortfahren", "Abbruch") != 0) { return; }
        }

        allowTemporay = true;
        e.Feedback = Database?.ExecuteScript(_item, chkChangeValuesInTest.Checked, r, null);
        allowTemporay = false;
        variableEditor.WriteVariablesToTable(Database?.Variables);
    }

    private void GlobalTab_SelectedIndexChanged(object sender, System.EventArgs e) => WriteInfosBack();

    private void lstEventScripts_AddClicked(object sender, System.EventArgs e) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }

        var newScriptItem = lstEventScripts.Item.Add(new DatabaseScriptDescription(Database));

        WriteInfosBack();

        newScriptItem.Checked = true;
    }

    private void lstEventScripts_ItemCheckedChanged(object sender, System.EventArgs e) {
        if (lstEventScripts.Item.Checked().Count != 1) {
            Item = null;
            return;
        }
        if (TableView.ErrorMessage(Database, EditableErrorReasonType.EditNormaly)) {
            Item = null;
            return;
        }
        var selectedlstEventScripts = (DatabaseScriptDescription)((ReadableListItem)lstEventScripts.Item.Checked()[0]).Item;
        Item = selectedlstEventScripts;
    }

    private void RemoveDatabase() {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }
        Item = null;
        Database.DisposingEvent -= Database_DisposingEvent;
        Database.CanDoScript -= Database_CanDoScript;
        Database = null;
    }

    private void ScriptEditor_Changed(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.ScriptText = eventScriptEditor.ScriptText;
    }

    //private void scriptEditor_ContextMenuInit(object sender, ContextMenuInitEventArgs e) {
    //    //Todo: Implementieren
    //    if (e.HotItem is string txt) {
    //        var c = Database?.Column.Exists(txt);
    //        if (c is null) { return; }
    //        _ = e.UserMenu.Add(ContextMenuCommands.SpaltenEigenschaftenBearbeiten);
    //    }
    //}

    //private void scriptEditor_ContextMenuItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
    //    //Todo: Implementieren
    //    ColumnItem? c = null;

    //    if (e.HotItem is string txt) { c = Database?.Column.Exists(txt); }

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

    private void WriteInfosBack() {
        if (TableView.ErrorMessage(Database, EditableErrorReasonType.EditNormaly) || Database == null || Database.IsDisposed) { return; }

        if (_item != null) {
            _item.ScriptText = eventScriptEditor.ScriptText;
        }

        #region Items sicherheitshalber in die Datenbank zur�ck schreiben, nur so werden die gel�schten und neuen erfasst

        var t2 = new List<DatabaseScriptDescription>();
        t2.AddRange(lstEventScripts.Item.Select(thisItem => (DatabaseScriptDescription)((ReadableListItem)thisItem).Item));
        Database.EventScript = new(t2);
        Database.EventScriptErrorMessage = string.Empty;

        #endregion

        var l = variableEditor.GetVariables();
        var l2 = new List<VariableString>();
        foreach (var thisv in l) {
            if (thisv is VariableString vs) {
                l2.Add(vs);
            }
        }

        Database.Variables = new VariableCollection(l2);
    }

    #endregion
}