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
using BlueBasics.Enums;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.Interfaces;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueBasics.IO;

namespace BlueControls.BlueDatabaseDialogs;

public sealed partial class DatabaseScriptEditor : IHasDatabase {

    #region Fields

    private EventScript? _item;

    #endregion

    #region Constructors

    public DatabaseScriptEditor(DatabaseAbstract database) {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
        grpEigenschaften.Enabled = false;
        eventScriptEditor.Enabled = false;
        Database = database;
        Database.Disposing += Database_Disposing;

        FormManager.Current.RegisterForm(this);
    }

    #endregion

    #region Properties

    public DatabaseAbstract? Database { get; private set; }

    public EventScript? Item {
        get {
            if (Database == null || Database.IsDisposed) { return null; }

            return _item;
        }
        set {
            if (_item == value) { return; }

            WriteInfosBack();

            _item = null; // Um keine werte zurück zu Schreiben werden des anzeigen

            if (value != null) {
                grpEigenschaften.Enabled = true;
                eventScriptEditor.Enabled = true;
                txbName.Text = value.Name;

                chkZeile.Checked = value.NeedRow;
                txbTestZeile.Enabled = value.NeedRow;
                chkAuslöser_newrow.Checked = value.EventTypes.HasFlag(EventTypes.new_row);
                chkAuslöser_valuechanged.Checked = value.EventTypes.HasFlag(EventTypes.value_changed);
                chkAuslöser_valuechangedThread.Checked = value.EventTypes.HasFlag(EventTypes.value_changed_extra_thread);
                chkAuslöser_prepaireformula.Checked = value.EventTypes.HasFlag(EventTypes.prepare_formula);
                chkAuslöser_databaseloaded.Checked = value.EventTypes.HasFlag(EventTypes.database_loaded);
                chkAuslöser_export.Checked = value.EventTypes.HasFlag(EventTypes.export);
                chkExternVerfügbar.Checked = value.ManualExecutable;
                chkAendertWerte.Checked = value.ChangeValues;
                eventScriptEditor.ScriptText = value.Script;

                _item = value;
            } else {
                grpEigenschaften.Enabled = false;
                eventScriptEditor.Enabled = false;
                txbTestZeile.Enabled = false;

                txbName.Text = string.Empty;
                eventScriptEditor.ScriptText = string.Empty;
                chkAuslöser_newrow.Checked = false;
                chkAuslöser_valuechanged.Checked = false;
                chkAuslöser_prepaireformula.Checked = false;
                chkExternVerfügbar.Checked = false;
                chkAendertWerte.Checked = false;
                chkAuslöser_valuechangedThread.Checked = false;
                chkAuslöser_databaseloaded.Checked = false;
                chkAuslöser_export.Checked = false;
            }
        }
    }

    #endregion

    #region Methods

    public void OpenAdditionalFileFolder() {
        //Todo: Implementieren
        if (Database == null || Database.IsDisposed) {
            return;
        }
        if (DirectoryExists(Database.AdditionalFilesPfadWhole())) { _ = ExecuteFile(Database.AdditionalFilesPfadWhole()); }
    }

    protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e) {
        base.OnFormClosing(e);
        if (Database == null || Database.IsDisposed) { return; }

        WriteInfosBack();
        RemoveDatabase();
    }

    protected override void OnLoad(System.EventArgs e) {
        base.OnLoad(e);

        lstEventScripts.Item.Clear();
        if (Database == null || Database.IsDisposed) { return; }

        foreach (var thisSet in Database.EventScript) {
            if (thisSet != null) {
                _ = lstEventScripts.Item.Add(thisSet);
            }
        }
    }

    protected override void OnShown(System.EventArgs e) => variableEditor.WriteVariablesToTable(Database?.Variables);

    private void btnSave_Click(object sender, System.EventArgs e) {
        btnSave.Enabled = false;

        if (Database != null) {
            WriteInfosBack();
            _ = Database.Save();
        }
        btnSave.Enabled = true;
    }

    private void btnSpaltenuebersicht_Click(object sender, System.EventArgs e) => Database?.Column.GenerateOverView();

    private void chkAendertWerte_CheckedChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.ChangeValues = chkAendertWerte.Checked;
    }

    private void chkAuslöser_newrow_CheckedChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }

        EventTypes tmp = 0;
        if (chkAuslöser_newrow.Checked) { tmp |= EventTypes.new_row; }
        if (chkAuslöser_valuechanged.Checked) { tmp |= EventTypes.value_changed; }
        if (chkAuslöser_prepaireformula.Checked) { tmp |= EventTypes.prepare_formula; }
        if (chkAuslöser_valuechangedThread.Checked) { tmp |= EventTypes.value_changed_extra_thread; }
        if (chkAuslöser_databaseloaded.Checked) { tmp |= EventTypes.database_loaded; }
        if (chkAuslöser_export.Checked) { tmp |= EventTypes.export; }
        Item.EventTypes = tmp;
    }

    private void chkExternVerfügbar_CheckedChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.ManualExecutable = chkExternVerfügbar.Checked;
    }

    private void chkZeile_CheckedChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.NeedRow = chkZeile.Checked;
        txbTestZeile.Enabled = chkZeile.Checked;
    }

    private void Database_Disposing(object sender, System.EventArgs e) {
        RemoveDatabase();
        Close();
    }

    private void eventScriptEditor_ExecuteScript(object sender, BlueScript.EventArgs.ScriptEventArgs e) {
        if (Database == null || Database.IsDisposed) {
            e.Feedback = new ScriptEndedFeedback("Keine Datenbank geladen.", false, "Allgemein");
            return;
        }

        if (_item == null) {
            e.Feedback = new ScriptEndedFeedback("Kein Skript gewählt.", false, "Allgemein");
            return;
        }

        if (!_item.IsOk()) {
            e.Feedback = new ScriptEndedFeedback("Bitte zuerst den Fehler korrigieren: " + _item.ErrorReason(), false, "Allgemein");
            return;
        }

        WriteInfosBack();

        RowItem? r = null;

        if (_item.NeedRow) {
            if (Database.Row.Count == 0) {
                e.Feedback = new ScriptEndedFeedback("Zum Test wird zumindest eine Zeile benötigt.", false, "Allgemein");
                return;
            }
            if (string.IsNullOrEmpty(txbTestZeile.Text)) {
                txbTestZeile.Text = Database?.Row.First()?.CellFirstString() ?? string.Empty;
            }

            r = Database?.Row[txbTestZeile.Text];
            if (r== null || r.IsDisposed) {
                e.Feedback = new ScriptEndedFeedback("Zeile nicht gefunden.", false, "Allgemein");
                return;
            }
        }

        if (chkChangeValuesInTest.Checked) {
            if (MessageBox.Show("Skript ändert Werte!<br>Fortfahren?", ImageCode.Warnung, "Fortfahren", "Abbruch") != 0) { return; }
        }

        e.Feedback = Database?.ExecuteScript(_item, chkChangeValuesInTest.Checked, r);
    }

    private void GlobalTab_SelectedIndexChanged(object sender, System.EventArgs e) => WriteInfosBack();

    private void lstEventScripts_AddClicked(object sender, System.EventArgs e) {
        if (Database == null || Database.IsDisposed) { return; }

        var newScriptItem = lstEventScripts.Item.Add(new EventScript(Database));

        WriteInfosBack();

        newScriptItem.Checked = true;
    }

    private void lstEventScripts_ItemCheckedChanged(object sender, System.EventArgs e) {
        if (lstEventScripts.Item.Checked().Count != 1) {
            Item = null;
            return;
        }
        if (TableView.ErrorMessage(Database, EditableErrorReason.EditNormaly)) {
            Item = null;
            return;
        }
        var selectedlstEventScripts = (EventScript)((ReadableListItem)lstEventScripts.Item.Checked()[0]).Item;
        Item = selectedlstEventScripts;
    }

    private void RemoveDatabase() {
        if (Database == null || Database.IsDisposed) { return; }
        Item = null;
        Database.Disposing -= Database_Disposing;
        Database = null;
    }

    private void ScriptEditor_Changed(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.Script = eventScriptEditor.ScriptText;
    }

    private void scriptEditor_ContextMenuInit(object sender, ContextMenuInitEventArgs e) {
        //Todo: Implementieren
        if (e.HotItem is string txt) {
            var c = Database?.Column.Exists(txt);
            if (c is null) { return; }
            _ = e.UserMenu.Add(ContextMenuComands.SpaltenEigenschaftenBearbeiten);
        }
    }

    private void scriptEditor_ContextMenuItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
        //Todo: Implementieren
        ColumnItem? c = null;

        if (e.HotItem is string txt) { c = Database?.Column.Exists(txt); }

        switch (e.ClickedComand.ToLower()) {
            case "spalteneigenschaftenbearbeiten":
                if (c != null && !c.IsDisposed) {
                    TableView.OpenColumnEditor(c, null, null);
                }

                break;
        }
    }

    private void txbName_TextChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.Name = txbName.Text;
    }

    private void WriteInfosBack() {
        if (TableView.ErrorMessage(Database, EditableErrorReason.EditNormaly) || Database == null) { return; }

        if (_item != null) {
            _item.Script = eventScriptEditor.ScriptText;
        }

        #region Items sicherheitshalber in die Datenbank zurück schreiben, nur so werden die gelöschten und neuen erfasst

        var t2 = new List<EventScript>();
        t2.AddRange(lstEventScripts.Item.Select(thisItem => (EventScript)((ReadableListItem)thisItem).Item));
        Database.EventScript = new(t2);

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