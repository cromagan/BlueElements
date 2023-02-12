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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueScript.Variables;
using static BlueBasics.Converter;
using MessageBox = BlueControls.Forms.MessageBox;

namespace BlueControls.BlueDatabaseDialogs;

public sealed partial class DatabaseScriptEditor {

    #region Fields

    private EventScript? _item;
    private DatabaseAbstract? _database;

    #endregion

    #region Constructors

    public DatabaseScriptEditor(DatabaseAbstract database) {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
        _database = database;
        _database.Disposing += Database_Disposing;

        eventScriptEditor.Database = _database;
    }

    #endregion

    #region Properties
    public EventScript? Item {
        get {
            if (_database == null || _database.IsDisposed) { return null; }

            return _item;
        }
        set {
            if (_item == value) { return; }

            if (_item != null) {
                _item.EventTypes = eventScriptEditor.EventTypes;
                _item.NeedRow = eventScriptEditor.IsRowScript;
                _item.Script = eventScriptEditor.ScriptText;
            }

            _item = value;

            if (_item != null) {
                Enabled = true;
                txbName.Text = _item.Name;

                chkZeile.Checked = _item.NeedRow;
                chkAuslöser_newrow.Checked = _item.EventTypes.HasFlag(EventTypes.new_row);
                chkAuslöser_valuechanged.Checked = _item.EventTypes.HasFlag(EventTypes.value_changed);
                chkAuslöser_errorcheck.Checked = _item.EventTypes.HasFlag(EventTypes.error_check);
                chkExternVerfügbar.Checked = _item.ManualExecutable;
                chkAendertWerte.Checked = _item.ChangeValues;

                eventScriptEditor.EventTypes = _item.EventTypes;
                eventScriptEditor.IsRowScript = _item.NeedRow;
                eventScriptEditor.ScriptText = _item.Script;
            } else {
                Enabled = false;

                txbName.Text = string.Empty;
                eventScriptEditor.ScriptText = string.Empty;
                eventScriptEditor.IsRowScript = false;
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

    private void btnSave_Click(object sender, System.EventArgs e) {
        btnSave.Enabled = false;

        if (_database != null) {
            WriteInfosBack();
            _ = _database.Save();
        }
        btnSave.Enabled = true;
    }

    private void CheckEvents() {
        if (Item == null) { return; }

        EventTypes tmp = 0;
        if (chkAuslöser_newrow.Checked) { tmp |= EventTypes.new_row; }
        if (chkAuslöser_valuechanged.Checked) { tmp |= EventTypes.value_changed; }
        if (chkAuslöser_errorcheck.Checked) { tmp |= EventTypes.error_check; }
        Item.EventTypes = tmp;
    }

    private void chkAendertWerte_CheckedChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.ChangeValues = chkAendertWerte.Checked;
    }

    private void chkAuslöser_newrow_CheckedChanged(object sender, System.EventArgs e) => CheckEvents();

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

    private void lstEventScripts_AddClicked(object sender, System.EventArgs e) {
        if (_database == null || _database.IsDisposed) { return; }

        var newScriptItem = lstEventScripts.Item.Add(new EventScript(_database));
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

    private void OkBut_Click(object sender, System.EventArgs e) => Close();

    private void RemoveDatabase() {
        if (_database == null || _database.IsDisposed) { return; }
        _database.Disposing -= Database_Disposing;
        _database = null;
    }

    private void ScriptEditor_Changed(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.Script = eventScriptEditor.ScriptText;
    }

    private void txbName_TextChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.Name = txbName.Text;
    }

    private void WriteInfosBack() {
        if (_database == null || _database.IsDisposed || _database.ReadOnly) { return; } // Disposed

        var t2 = new List<EventScript?>();
        t2.AddRange(lstEventScripts.Item.Select(thisItem => (EventScript)((ReadableListItem)thisItem).Item));
        _database.EventScript = new(t2);

        var l = variableEditor.GetVariables();
        var l2 = new List<VariableString>();
        foreach (var thisv in l) {
            if (thisv is VariableString vs) {
                l2.Add(vs);
            }
        }

        _database.Variables = new ReadOnlyCollection<VariableString>(l2);
    }

    #endregion
}