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

using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.ItemCollectionList;
using BlueScript.EventArgs;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueControls.ConnectedFormula;

public sealed partial class ConnectedFormulaScriptEditor {

    #region Fields

    private FormulaScriptDescription? _item;

    #endregion

    #region Constructors

    public ConnectedFormulaScriptEditor(ConnectedFormula formula) {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
        grpEigenschaften.Enabled = false;
        eventScriptEditor.Enabled = false;
        Formula = formula;
        FormManager.RegisterForm(this);
    }

    #endregion

    #region Properties

    public ConnectedFormula? Formula { get; }

    public FormulaScriptDescription? Item {
        get {
            if (Formula == null || Formula.IsDisposed) { return null; }

            return _item;
        }
        set {
            if (_item == value) { return; }

            WriteInfosBack();

            _item = null; // Um keine werte zurück zu Schreiben werden des anzeigen

            if (value != null) {
                grpEigenschaften.Enabled = true;
                eventScriptEditor.Enabled = true;
                txbName.Text = value.KeyName;

                chkAuslöser_valuechanged.Checked = value.EventTypes.HasFlag(ScriptEventTypes.value_changed);
                chkAuslöser_valuechangedThread.Checked = value.EventTypes.HasFlag(ScriptEventTypes.value_changed_extra_thread);
                chkAuslöser_prepaireformula.Checked = value.EventTypes.HasFlag(ScriptEventTypes.prepare_formula);
                chkAuslöser_formulaloaded.Checked = value.EventTypes.HasFlag(ScriptEventTypes.loaded);
                chkAuslöser_export.Checked = value.EventTypes.HasFlag(ScriptEventTypes.export);
                chkExternVerfügbar.Checked = value.ManualExecutable;
                chkAendertWerte.Checked = value.ChangeValues;
                eventScriptEditor.ScriptText = value.ScriptText;

                _item = value;
            } else {
                grpEigenschaften.Enabled = false;
                eventScriptEditor.Enabled = false;

                txbName.Text = string.Empty;
                eventScriptEditor.ScriptText = string.Empty;
                chkAuslöser_valuechanged.Checked = false;
                chkAuslöser_prepaireformula.Checked = false;
                chkExternVerfügbar.Checked = false;
                chkAendertWerte.Checked = false;
                chkAuslöser_valuechangedThread.Checked = false;
                chkAuslöser_formulaloaded.Checked = false;
                chkAuslöser_export.Checked = false;
            }
        }
    }

    #endregion

    #region Methods

    protected override void OnFormClosing(FormClosingEventArgs e) {
        base.OnFormClosing(e);
        if (Formula == null || Formula.IsDisposed) { return; }

        WriteInfosBack();
        //RemoveDatabase();
    }

    protected override void OnLoad(System.EventArgs e) {
        base.OnLoad(e);

        lstEventScripts.Item.Clear();
        if (Formula == null || Formula.IsDisposed) { return; }

        foreach (var thisSet in Formula.EventScript) {
            if (thisSet != null) {
                _ = lstEventScripts.Item.Add(thisSet);
            }
        }
    }

    protected override void OnShown(System.EventArgs e) => variableEditor.WriteVariablesToTable(Formula?.Variables);

    private void btnSave_Click(object sender, System.EventArgs e) {
        btnSave.Enabled = false;

        if (Formula != null) {
            WriteInfosBack();
            Formula.Save();
        }
        btnSave.Enabled = true;
    }

    private void chkAendertWerte_CheckedChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.ChangeValues = chkAendertWerte.Checked;
    }

    private void chkAuslöser_newrow_CheckedChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }

        ScriptEventTypes tmp = 0;
        if (chkAuslöser_valuechanged.Checked) { tmp |= ScriptEventTypes.value_changed; }
        if (chkAuslöser_prepaireformula.Checked) { tmp |= ScriptEventTypes.prepare_formula; }
        if (chkAuslöser_valuechangedThread.Checked) { tmp |= ScriptEventTypes.value_changed_extra_thread; }
        if (chkAuslöser_formulaloaded.Checked) { tmp |= ScriptEventTypes.loaded; }
        if (chkAuslöser_export.Checked) { tmp |= ScriptEventTypes.export; }
        Item.EventTypes = tmp;
    }

    private void chkExternVerfügbar_CheckedChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.ManualExecutable = chkExternVerfügbar.Checked;
    }

    private void eventScriptEditor_ExecuteScript(object sender, ScriptEventArgs e) {
        if (Formula == null || Formula.IsDisposed) {
            e.Feedback = new ScriptEndedFeedback("Keine Formular geladen.", false, false, "Allgemein");
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

        //if (chkChangeValuesInTest.Checked) {
        //    if (MessageBox.Show("Skript ändert Werte!<br>Fortfahren?", ImageCode.Warnung, "Fortfahren", "Abbruch") != 0) { return; }
        //}

        e.Feedback = Formula?.ExecuteScript(_item);
    }

    private void GlobalTab_SelectedIndexChanged(object sender, System.EventArgs e) => WriteInfosBack();

    private void lstEventScripts_AddClicked(object sender, System.EventArgs e) {
        if (Formula == null || Formula.IsDisposed) { return; }

        var newScriptItem = lstEventScripts.Item.Add(new FormulaScriptDescription(Formula));

        WriteInfosBack();

        newScriptItem.Checked = true;
    }

    private void lstEventScripts_ItemCheckedChanged(object sender, System.EventArgs e) {
        if (lstEventScripts.Item.Checked().Count != 1) {
            Item = null;
            return;
        }

        var selectedlstEventScripts = (FormulaScriptDescription)((ReadableListItem)lstEventScripts.Item.Checked()[0]).Item;
        Item = selectedlstEventScripts;
    }

    private void ScriptEditor_Changed(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.ScriptText = eventScriptEditor.ScriptText;
    }

    private void txbName_TextChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.KeyName = txbName.Text;
    }

    private void WriteInfosBack() {
        if (_item != null) {
            _item.ScriptText = eventScriptEditor.ScriptText;
        }

        #region Items sicherheitshalber in die Datenbank zurück schreiben, nur so werden die gelöschten und neuen erfasst

        var t2 = new List<FormulaScriptDescription>();
        t2.AddRange(lstEventScripts.Item.Select(thisItem => (FormulaScriptDescription)((ReadableListItem)thisItem).Item));
        Formula.EventScript = new(t2);

        #endregion

        var l = variableEditor.GetVariables();
        var l2 = new List<VariableString>();
        foreach (var thisv in l) {
            if (thisv is VariableString vs) {
                l2.Add(vs);
            }
        }

        Formula.Variables = new VariableCollection(l2);
    }

    #endregion
}