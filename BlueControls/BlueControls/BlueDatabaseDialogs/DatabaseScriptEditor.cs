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
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueBasics.IO;

namespace BlueControls.BlueDatabaseDialogs;

public sealed partial class DatabaseScriptEditor {
    #region Fields

    #region Fields

    private DatabaseAbstract? _database;
    private EventScript? _item;

    #endregion

    #region Constructors

    public DatabaseScriptEditor(DatabaseAbstract database) {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
        grpEigenschaften.Enabled = false;
        eventScriptEditor.Enabled = false;
        _database = database;
        _database.Disposing += Database_Disposing;

        FormManager.Current.RegisterForm(this);
    }

    #endregion

    #endregion

    #region Properties

    public EventScript? Item {
        get {
            if (_database == null || _database.IsDisposed) { return null; }

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
                chkAuslöser_newrow.Checked = value.EventTypes.HasFlag(EventTypes.new_row);
                chkAuslöser_valuechanged.Checked = value.EventTypes.HasFlag(EventTypes.value_changed);
                chkAuslöser_errorcheck.Checked = value.EventTypes.HasFlag(EventTypes.error_check);
                chkExternVerfügbar.Checked = value.ManualExecutable;
                chkAendertWerte.Checked = value.ChangeValues;
                eventScriptEditor.ScriptText = value.Script;

                _item = value;
            } else {
                grpEigenschaften.Enabled = false;
                eventScriptEditor.Enabled = false;

                txbName.Text = string.Empty;
                eventScriptEditor.ScriptText = string.Empty;
                chkAuslöser_newrow.Checked = false;
                chkAuslöser_valuechanged.Checked = false;
                chkAuslöser_errorcheck.Checked = false;
                chkExternVerfügbar.Checked = false;
                chkAendertWerte.Checked = false;
            }
        }
    }

    #endregion

    #region Methods

    protected override void OnFormClosing(FormClosingEventArgs e) {
        base.OnFormClosing(e);
        if (_database == null || _database.IsDisposed) {
            return;
        }

        WriteInfosBack();
        RemoveDatabase();
    }

    protected override void OnLoad(System.EventArgs e) {
        base.OnLoad(e);

        lstEventScripts.Item.Clear();
        foreach (var thisSet in _database.EventScript.Where(thisSet => thisSet != null)) {
            _ = lstEventScripts.Item.Add(thisSet);
        }
    }

    protected override void OnShown(System.EventArgs e) => variableEditor.WriteVariablesToTable(_database?.Variables);

    protected void OpenAdditionalFileFolder() {
        //Todo: Implementieren
        if (_database == null || _database.IsDisposed) {
            return;
        }
        if (DirectoryExists(_database.AdditionalFilesPfadWhole())) { _ = ExecuteFile(_database.AdditionalFilesPfadWhole()); }
    }

    private void btnSave_Click(object sender, System.EventArgs e) {
        btnSave.Enabled = false;

        if (_database != null) {
            WriteInfosBack();
            _ = _database.Save();
        }
        btnSave.Enabled = true;
    }

    private void chkAendertWerte_CheckedChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.ChangeValues = chkAendertWerte.Checked;
    }

    private void chkAuslöser_newrow_CheckedChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }

        EventTypes tmp = 0;
        if (chkAuslöser_newrow.Checked) { tmp |= EventTypes.new_row; }
        if (chkAuslöser_valuechanged.Checked) { tmp |= EventTypes.value_changed; }
        if (chkAuslöser_errorcheck.Checked) { tmp |= EventTypes.error_check; }
        Item.EventTypes = tmp;
    }

    private void chkExternVerfügbar_CheckedChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.ManualExecutable = chkExternVerfügbar.Checked;
    }

    private void chkZeile_CheckedChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.NeedRow = chkZeile.Checked;
    }

    private void Database_Disposing(object sender, System.EventArgs e) {
        RemoveDatabase();
        Close();
    }

    private void eventScriptEditor_ExecuteScript(object sender, BlueScript.EventArgs.ScriptEventArgs e) {
        if (_database == null || _database.IsDisposed) {
            e.Feedback = new ScriptEndedFeedback("Keine Datenbank geladen.", false);
            return;
        }

        if (_item == null) {
            e.Feedback = new ScriptEndedFeedback("Kein Skript gewählt.", false);
            return;
        }

        if (!_item.IsOk()) {
            e.Feedback = new ScriptEndedFeedback("Bitte zuerst den Fehler korrigieren: " + _item.ErrorReason(), false);
            return;
        }

        WriteInfosBack();

        RowItem? r = null;

        if (_item.NeedRow) {
            if (_database.Row.Count == 0) {
                e.Feedback = new ScriptEndedFeedback("Zum Test wird zumindest eine Zeile benötigt.", false);
                return;
            }
            if (string.IsNullOrEmpty(txbTestZeile.Text)) {
                txbTestZeile.Text = _database?.Row?.First()?.CellFirstString() ?? string.Empty;
            }

            r = _database?.Row?[txbTestZeile.Text];
            if (r == null) {
                e.Feedback = new ScriptEndedFeedback("Zeile nicht gefunden.", false);
                return;
            }
        }
        e.Feedback = _database?.ExecuteScript(_item, false, r);
    }

    private void GlobalTab_SelectedIndexChanged(object sender, System.EventArgs e) => WriteInfosBack();

    private void lstEventScripts_AddClicked(object sender, System.EventArgs e) {
        if (_database == null || _database.IsDisposed) { return; }

        var newScriptItem = lstEventScripts.Item.Add(new EventScript(_database));

        WriteInfosBack();

        newScriptItem.Checked = true;
    }

    private void lstEventScripts_ItemCheckedChanged(object sender, System.EventArgs e) {
        if (lstEventScripts.Item.Checked().Count != 1) {
            Item = null;
            return;
        }
        if (_database == null || _database.IsDisposed || _database.ReadOnly) {
            Item = null;
            return;
        }
        var selectedlstEventScripts = (EventScript)((ReadableListItem)lstEventScripts.Item.Checked()[0]).Item;
        Item = selectedlstEventScripts;
    }

    private void RemoveDatabase() {
        if (_database == null || _database.IsDisposed) { return; }
        Item = null;
        _database.Disposing -= Database_Disposing;
        _database = null;
    }

    private void ScriptEditor_Changed(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.Script = eventScriptEditor.ScriptText;
    }

    private void scriptEditor_ContextMenuInit(object sender, ContextMenuInitEventArgs e) {
        //Todo: Implementieren
        if (e.HotItem is string txt) {
            var c = _database.Column.Exists(txt);
            if (c is null) { return; }
            _ = e.UserMenu.Add(ContextMenuComands.SpaltenEigenschaftenBearbeiten);
        }
    }

    private void scriptEditor_ContextMenuItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
        //Todo: Implementieren
        ColumnItem? c = null;

        if (e.HotItem is string txt) { c = _database.Column.Exists(txt); }

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
        if (_database == null || _database.IsDisposed || _database.ReadOnly) { return; } // Disposed

        if (_item != null) {
            _item.Script = eventScriptEditor.ScriptText;
        }

        #region Items sicherheitshalber in die DAtenbank zurück schreiben, nur so werden die gelöschten und neuen erfasst

        var t2 = new List<EventScript?>();
        t2.AddRange(lstEventScripts.Item.Select(thisItem => (EventScript)((ReadableListItem)thisItem).Item));
        _database.EventScript = new(t2);

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

        _database.Variables = new ReadOnlyCollection<VariableString>(l2);
    }

    #endregion
}