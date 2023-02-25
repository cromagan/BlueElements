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
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
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
        // Dieser Aufruf ist f�r den Windows Form-Designer erforderlich.
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

            _item = null; // Um keine werte zur�ck zu Schreiben werden des anzeigen

            if (value != null) {
                grpEigenschaften.Enabled = true;
                eventScriptEditor.Enabled = true;
                txbName.Text = value.Name;

                chkZeile.Checked = value.NeedRow;
                txbTestZeile.Enabled = value.NeedRow;
                chkAusl�ser_newrow.Checked = value.EventTypes.HasFlag(EventTypes.new_row);
                chkAusl�ser_valuechanged.Checked = value.EventTypes.HasFlag(EventTypes.value_changed);
                chkAusl�ser_valuechangedThread.Checked = value.EventTypes.HasFlag(EventTypes.value_changed_extra_thread);
                chkAusl�ser_errorcheck.Checked = value.EventTypes.HasFlag(EventTypes.error_check);
                chkAusl�ser_databaseloaded.Checked = value.EventTypes.HasFlag(EventTypes.database_loaded);
                chkAusl�ser_export.Checked = value.EventTypes.HasFlag(EventTypes.export);
                chkExternVerf�gbar.Checked = value.ManualExecutable;
                chkAendertWerte.Checked = value.ChangeValues;
                eventScriptEditor.ScriptText = value.Script;

                _item = value;
            } else {
                grpEigenschaften.Enabled = false;
                eventScriptEditor.Enabled = false;
                txbTestZeile.Enabled = false;

                txbName.Text = string.Empty;
                eventScriptEditor.ScriptText = string.Empty;
                chkAusl�ser_newrow.Checked = false;
                chkAusl�ser_valuechanged.Checked = false;
                chkAusl�ser_errorcheck.Checked = false;
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

    protected override void OnFormClosing(FormClosingEventArgs e) {
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

    protected void OpenAdditionalFileFolder() {
        //Todo: Implementieren
        if (Database == null || Database.IsDisposed) {
            return;
        }
        if (DirectoryExists(Database.AdditionalFilesPfadWhole())) { _ = ExecuteFile(Database.AdditionalFilesPfadWhole()); }
    }

    private void btnSave_Click(object sender, System.EventArgs e) {
        btnSave.Enabled = false;

        if (Database != null) {
            WriteInfosBack();
            _ = Database.Save();
        }
        btnSave.Enabled = true;
    }

    private void chkAendertWerte_CheckedChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.ChangeValues = chkAendertWerte.Checked;
    }

    private void chkAusl�ser_newrow_CheckedChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }

        EventTypes tmp = 0;
        if (chkAusl�ser_newrow.Checked) { tmp |= EventTypes.new_row; }
        if (chkAusl�ser_valuechanged.Checked) { tmp |= EventTypes.value_changed; }
        if (chkAusl�ser_errorcheck.Checked) { tmp |= EventTypes.error_check; }
        if (chkAusl�ser_valuechangedThread.Checked) { tmp |= EventTypes.value_changed_extra_thread; }
        if (chkAusl�ser_databaseloaded.Checked) { tmp |= EventTypes.database_loaded; }
        if (chkAusl�ser_export.Checked) { tmp |= EventTypes.export; }
        Item.EventTypes = tmp;
    }

    private void chkExternVerf�gbar_CheckedChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.ManualExecutable = chkExternVerf�gbar.Checked;
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
            e.Feedback = new ScriptEndedFeedback("Keine Datenbank geladen.", false);
            return;
        }

        if (_item == null) {
            e.Feedback = new ScriptEndedFeedback("Kein Skript gew�hlt.", false);
            return;
        }

        if (!_item.IsOk()) {
            e.Feedback = new ScriptEndedFeedback("Bitte zuerst den Fehler korrigieren: " + _item.ErrorReason(), false);
            return;
        }

        WriteInfosBack();

        RowItem? r = null;

        if (_item.NeedRow) {
            if (Database.Row.Count == 0) {
                e.Feedback = new ScriptEndedFeedback("Zum Test wird zumindest eine Zeile ben�tigt.", false);
                return;
            }
            if (string.IsNullOrEmpty(txbTestZeile.Text)) {
                txbTestZeile.Text = Database?.Row.First()?.CellFirstString() ?? string.Empty;
            }

            r = Database?.Row[txbTestZeile.Text];
            if (r == null) {
                e.Feedback = new ScriptEndedFeedback("Zeile nicht gefunden.", false);
                return;
            }
        }
        e.Feedback = Database?.ExecuteScript(_item, false, r);
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
        if (Database == null || Database.IsDisposed || Database.ReadOnly) {
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
                if (c != null) {
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
        if (Database == null || Database.IsDisposed || Database.ReadOnly) { return; } // Disposed

        if (_item != null) {
            _item.Script = eventScriptEditor.ScriptText;
        }

        #region Items sicherheitshalber in die DAtenbank zur�ck schreiben, nur so werden die gel�schten und neuen erfasst

        var t2 = new List<EventScript?>();
        t2.AddRange(lstEventScripts.Item.Select(thisItem => (EventScript)((ReadableListItem)thisItem).Item));
        Database.EventScript = new(t2);

        #endregion

        #region

        var l = variableEditor.GetVariables();
        var l2 = new List<VariableString>();
        foreach (var thisv in l) {
            if (thisv is VariableString vs) {
                l2.Add(vs);
            }
        }

        #endregion

        Database.Variables = new ReadOnlyCollection<VariableString>(l2);
    }

    #endregion
}